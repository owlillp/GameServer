"use client";

import { isStaff, resolveHomeRoute } from "@/src/shared/auth/roles";
import { useIsHydrated } from "@/src/shared/lib/use-is-hydrated";
import { routes } from "@/src/shared/routes";
import {
  sessionSelectors,
  useSessionStore,
} from "@/src/shared/stores/session-store";
import { SiteHeader } from "@/src/shared/ui/site-header";
import { KeyRound, ShieldCheck, UserRound } from "lucide-react";
import Link from "next/link";

const primaryLinkClass =
  "inline-flex h-10 items-center justify-center rounded-md bg-slate-900 px-5 text-sm font-medium text-white transition-colors hover:bg-slate-800";
const secondaryLinkClass =
  "inline-flex h-10 items-center justify-center rounded-md border border-slate-300 px-5 text-sm font-medium text-slate-700 transition-colors hover:bg-slate-100";

const features = [
  {
    icon: KeyRound,
    title: "Вход через OIDC",
    description:
      "Authorization Code + PKCE, refresh-токены и роли прямо в access-токене.",
  },
  {
    icon: UserRound,
    title: "Профиль игрока",
    description: "Аккаунт, роли и данные профиля доступны по защищённому API.",
  },
  {
    icon: ShieldCheck,
    title: "Админ-панель",
    description:
      "Для Admin и Moderator: статистика аккаунтов и управление пользователями.",
  },
] as const;

export default function Home() {
  const hydrated = useIsHydrated();
  const isAuthenticated = useSessionStore(sessionSelectors.isAuthenticated);
  const user = useSessionStore(sessionSelectors.user);

  const userRoles = user?.roles ?? [];
  const staff = isStaff(userRoles);
  // До гидратации состояние сессии неизвестно — рендерим «гостевой» вариант,
  // как на сервере, чтобы не было расхождения разметки.
  const showAuthenticated = hydrated && isAuthenticated;

  return (
    <>
      <SiteHeader />

      <main className="mx-auto flex w-full max-w-5xl flex-1 flex-col items-center px-6 py-16">
        <div className="max-w-2xl text-center">
          <p className="text-xs font-semibold tracking-widest text-slate-500 uppercase">
            Панель управления игровым сервером
          </p>
          <h1 className="mt-3 text-4xl font-bold tracking-tight text-slate-900">
            Game Server
          </h1>
          <p className="mt-4 text-base text-slate-600">
            Единый вход в личный кабинет игрока и админ-панель сервера.
            Регистрация открыта для всех.
          </p>

          <div className="mt-8 flex flex-wrap items-center justify-center gap-3">
            {showAuthenticated ? (
              <>
                <Link
                  href={resolveHomeRoute(userRoles)}
                  className={primaryLinkClass}
                >
                  {staff ? "Открыть админ-панель" : "Открыть личный кабинет"}
                </Link>
                <Link href={routes.profile} className={secondaryLinkClass}>
                  Мой профиль
                </Link>
              </>
            ) : (
              <>
                <Link href={routes.login} className={primaryLinkClass}>
                  Войти
                </Link>
                <Link href={routes.register} className={secondaryLinkClass}>
                  Создать аккаунт
                </Link>
              </>
            )}
          </div>
        </div>

        <div className="mt-16 grid w-full gap-4 sm:grid-cols-3">
          {features.map((feature) => (
            <section
              key={feature.title}
              className="rounded-lg border border-slate-200 bg-white p-5"
            >
              <span
                aria-hidden="true"
                className="inline-flex rounded-md bg-slate-100 p-2 text-slate-700"
              >
                <feature.icon className="h-5 w-5" />
              </span>
              <h2 className="mt-3 text-sm font-semibold text-slate-900">
                {feature.title}
              </h2>
              <p className="mt-1 text-sm text-slate-600">
                {feature.description}
              </p>
            </section>
          ))}
        </div>
      </main>
    </>
  );
}
