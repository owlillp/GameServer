import { jwtDecode } from "jwt-decode";
import { create } from "zustand";
import { persist } from "zustand/middleware";

// Единый zustand-стор сессии. Поддерживает две схемы аутентификации:
//   • "cookie" — Identity.Application cookie; в сторе держим account из /auth/login.
//   • "jwt"    — Bearer-токен; в сторе держим accessToken + декодированный user.
// Хук имеет статические .getState()/.setState(), поэтому доступен и вне React
// (например, в интерсепторе axios).

export type AuthScheme = "cookie" | "jwt";

export type SessionUser = {
  sub: string;
  name?: string;
  email?: string;
  roles: string[];
  expiresAt: Date | null;
};

export type SessionAccount = {
  accountId: string;
  email: string;
  userName: string;
};

type SessionData = {
  scheme: AuthScheme | null;
  accessToken: string | null;
  user: SessionUser | null;
  account: SessionAccount | null;
};

type SessionActions = {
  setJwt: (accessToken: string) => void;
  setAccount: (account: SessionAccount) => void;
  clear: () => void;
};

export type SessionStore = SessionData & SessionActions;

const initialState: SessionData = {
  scheme: null,
  accessToken: null,
  user: null,
  account: null,
};

export const useSessionStore = create<SessionStore>()(
  persist(
    (set) => ({
      ...initialState,
      // Установка схемы jwt сбрасывает account, чтобы не смешивать схемы.
      setJwt: (accessToken) =>
        set({
          scheme: "jwt",
          accessToken,
          user: decodeUser(accessToken),
          account: null,
        }),
      // Установка cookie-схемы сбрасывает accessToken — запросы пойдут без Bearer.
      setAccount: (account) =>
        set({
          scheme: "cookie",
          account,
          accessToken: null,
          user: null,
        }),
      clear: () => set(initialState),
    }),
    {
      name: "gameserver-session",
      // user/expiresAt производные от токена и не сериализуются как Date —
      // пересчитываем их при гидратации (в partialize их нет).
      partialize: (state) => ({
        scheme: state.scheme,
        accessToken: state.accessToken,
        account: state.account,
      }),
      merge: (persisted, current) => {
        const p = (persisted ?? {}) as Partial<SessionData>;
        const accessToken = p.accessToken ?? null;
        const scheme = p.scheme ?? (p.account ? "cookie" : accessToken ? "jwt" : null);

        return {
          ...current,
          scheme,
          accessToken,
          account: p.account ?? null,
          user: accessToken ? decodeUser(accessToken) : null,
        };
      },
    },
  ),
);

const EMPTY_ROLES: string[] = [];

export const sessionSelectors = {
  scheme: (s: SessionStore) => s.scheme,
  accessToken: (s: SessionStore) => s.accessToken,
  user: (s: SessionStore) => s.user,
  account: (s: SessionStore) => s.account,
  isAuthenticated: (s: SessionStore) => s.scheme !== null,
  roles: (s: SessionStore) => s.user?.roles ?? EMPTY_ROLES,
  displayName: (s: SessionStore) =>
    s.account?.userName ?? s.user?.name ?? s.user?.email ?? s.user?.sub ?? null,
  setJwt: (s: SessionStore) => s.setJwt,
  setAccount: (s: SessionStore) => s.setAccount,
  clear: (s: SessionStore) => s.clear,
} as const;

type AuthJwtClaims = {
  sub?: string;
  // Разные версии/мапперы .NET кладут имя в name или unique_name.
  name?: string;
  unique_name?: string;
  email?: string;
  role?: string | string[];
  roles?: string | string[];
  exp?: number;
};

function decodeUser(token: string): SessionUser | null {
  let claims: AuthJwtClaims;
  try {
    claims = jwtDecode<AuthJwtClaims>(token);
  } catch {
    // Битый токен — user не определён, но accessToken оставляем: сервер
    // ответит 401 на следующий запрос.
    return null;
  }

  if (!claims.sub) return null;

  return {
    sub: claims.sub,
    name: claims.name ?? claims.unique_name,
    email: claims.email,
    roles: normalizeRoles(claims.role ?? claims.roles),
    expiresAt: typeof claims.exp === "number" ? new Date(claims.exp * 1000) : null,
  };
}

function normalizeRoles(role: string | string[] | undefined): string[] {
  if (role === undefined) return [];
  return Array.isArray(role) ? role : [role];
}
