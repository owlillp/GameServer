# Unity-клиент: авторизация через AuthService

Контракт AuthService для игрового клиента. Сам клиентский код живёт в проекте
Unity; здесь — как с сервером взаимодействовать.

Два способа входа для public-клиента `gameserver-unity` (без секрета):

| | A. In-game пароль | B. Browser (OIDC) |
| --- | --- | --- |
| UX | Как Supercell ID: email+пароль прямо в игре | Открывается системный браузер, вход на сайте |
| Grant | `password` | `authorization_code` + PKCE |
| Пароль видит клиент | да | нет |
| MFA/капча | невозможно без доработок | можно добавить |
| Дополнительно | rate limit + lockout (на сервере) | loopback/deep link redirect |

Общее: после первого входа игра хранит **refresh-токен** (30 дней, при каждом
обновлении срок продлевается) — при следующих запусках логин не спрашивается,
access-токен молча обновляется по refresh.

## A. In-game логин

```
POST https://<issuer>/connect/token
Content-Type: application/x-www-form-urlencoded

grant_type=password
client_id=gameserver-unity
username=<email|username>
password=<пароль>
scope=openid profile email offline_access auth clans
```

Ответ: `access_token` (15 мин), `refresh_token` (30 дней), `token_type=Bearer`.

Следующий запуск игры — без пароля:

```
grant_type=refresh_token
client_id=gameserver-unity
refresh_token=<сохранённый токен>
```

Ошибки: `invalid_grant` — неверные данные, блокировка после 5 неудач или
просроченный refresh; `429` — сработал rate limit (120/мин на IP).

## B. Browser flow (Authorization Code + PKCE)

```
Unity                        Системный браузер                     AuthService
  │  открыть URL ──────────────────►  /connect/authorize (PKCE)
  │                                     │  нет cookie сессии
  │                                     ▼
  │                                  /login?returnUrl=<authorize URL>
  │                                     │  пользователь входит в SPA
  │                                     ▼
  │                                  /connect/authorize (cookie + code)
  │  ◄── redirect_uri?code=... ────────┘
  │  обмен кода на токены (POST /connect/token + code_verifier)
  │  Authorization: Bearer <access_token> ─────────────────────────►  API
```

- Standalone/Editor — redirect на loopback: `http://127.0.0.1:7777/callback`
  (порт фиксирован и должен совпадать с зарегистрированным `redirect_uri`).
  Клиент поднимает локальный HTTP-listener и ждёт код.
- Android/iOS — redirect на custom scheme: `gameserver://auth/callback`
  (схема регистрируется в манифесте Android / Info.plist iOS).

Плюс browser flow — SSO: если пользователь уже вошёл на сайте, игра получит
код без ввода пароля. Минус — переключение в браузер.

## Настройка сервера

Уже настроено в `appsettings.Development.json` / `appsettings.Docker.json`:

```json
"OpenIddict": {
  "Clients": {
    "Game": {
      "ClientId": "gameserver-unity",
      "DisplayName": "GameServer Unity Client",
      "RedirectUris": ["http://127.0.0.1:7777/callback", "gameserver://auth/callback"]
    }
  }
}
```

Клиент получает grants `password`, `authorization_code`, `refresh_token`.
Скоупы в токене определяют аудитории: `auth` → `auth-service`, `clans` →
`clan-service`; с одним access-токеном можно ходить и в AuthService
(`/auth/profile`), и в ClanService (`/clans`).
Меняете порт/схему — обновите и клиент, и сервер.

## Что должен реализовать Unity-клиент

1. **Первый вход:** либо `grant_type=password` через форму
   (`UnityWebRequest` с `Content-Type: application/x-www-form-urlencoded`),
   либо browser flow с PKCE (`S256`) и проверкой `state`.
2. **Хранение сессии:** refresh-токен — в защищённом хранилище
   (Keychain/Keystore/Credential Manager; PlayerPrefs — только для отладки).
3. **Тихий вход:** при запуске `grant_type=refresh_token`; если истёк —
   показать форму логина.
4. **API-запросы:** заголовок `Authorization: Bearer <access_token>`,
   при `401` — обновить токен и повторить запрос.
5. **Выход:** `POST /connect/revoke` (`client_id`, `token`,
   `token_type_hint=refresh_token`), затем очистить локальную сессию.

## Сессия («запомнить меня»)

- Refresh-токен живёт 30 дней; каждая ротация продлевает окно, поэтому активный
  игрок остаётся в сессии, а после месяца простоя игра попросит логин снова.
- Смена пароля/сброс админом — refresh-токены перестают работать (security stamp).
- `offline_access` в scope обязателен, иначе refresh-токен не выдаётся.

## Платформы

| Платформа | In-game | Browser flow |
| --- | --- | --- |
| Standalone / Editor | да | loopback `127.0.0.1:7777` |
| Android / iOS | да | deep link `gameserver://` |
| WebGL | да (нужен CORS) | нет (нужен JS-redirect-сценарий, как в SPA) |

Для WebGL-запросов с игрового домена добавьте его origin в
`Cors:AllowedOrigins` AuthService.

## Безопасность

- **In-game пароль:** не хардкодьте учётки в билде; не логируйте пароль; TLS
  обязателен. Lockout и rate limit уже на сервере. MFA для password grant не
  поддерживается — для критичных аккаунтов используйте browser flow.
- **PKCE (S256)** обязателен для browser flow на сервере; `state` проверяется
  на клиенте.
- Access-токен — bearer: не логируйте и передавайте только по HTTPS.
- Для локального Docker с самоподписанным сертификатом браузер/клиент должны
  доверять сертификату (или отключить проверку только на время отладки).

## Диагностика

| Симптом | Причина / решение |
| --- | --- |
| `invalid_grant` | неверный email/пароль, аккаунт заблокирован (5 неудач) или просрочен refresh-токен |
| `HTTP 429` на `/connect/token` | сработал rate limit: слишком много запросов с одного IP |
| не удаётся занять loopback-порт 7777 | порт занят или `redirect_uri` клиента не совпадает с сервером |
| `OIDC state mismatch` | устаревший redirect; начните вход заново |
| `invalid_client` | `clientId` не зарегистрирован (проверьте `OpenIddict:Clients:Game`) |
| TLS-ошибка к `https://localhost:5001` | dev-сертификат не доверен; `./scripts/prepare-nginx-https.sh` |
