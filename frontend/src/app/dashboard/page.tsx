"use client";

import { isStaff, resolveHomeRoute } from "@/src/shared/auth/roles";
import { RequireAuth } from "@/src/shared/auth/require-auth";
import { routes } from "@/src/shared/routes";
import {
  sessionSelectors,
  useSessionStore,
} from "@/src/shared/stores/session-store";
import { RoleBadge } from "@/src/shared/ui/role-badge";
import { SiteHeader } from "@/src/shared/ui/site-header";
import Link from "next/link";

export default function DashboardPage() {
  const user = useSessionStore(sessionSelectors.user);
  const userRoles = user?.roles ?? [];
  const staff = isStaff(userRoles);
  const displayName = user?.name ?? user?.email ?? user?.sub ?? "игрок";

  return (
    <RequireAuth>
      <SiteHeader />

      <main className="mx-auto w-full max-w-2xl flex-1 space-y-6 px-6 py-10">
        <header className="space-y-1">
          <h1 className="text-2xl font-semibold text-slate-900">
            Панель игрока
          </h1>
          <p className="text-sm text-slate-600">
            Вы вошли как <span className="font-medium">{displayName}</span>.
          </p>
        </header>

        <section className="rounded-lg border border-slate-200 bg-white p-6">
          <h2 className="text-xs font-semibold tracking-wide text-slate-500 uppercase">
            Аккаунт
          </h2>
          <dl className="mt-3 space-y-3 text-sm">
            <div className="flex flex-wrap justify-between gap-2">
              <dt className="text-slate-500">ID</dt>
              <dd className="font-mono text-xs break-all text-slate-900">
                {user?.sub ?? "—"}
              </dd>
            </div>
            <div className="flex flex-wrap justify-between gap-2">
              <dt className="text-slate-500">Email</dt>
              <dd className="font-medium text-slate-900">
                {user?.email ?? "—"}
              </dd>
            </div>
            <div className="flex flex-wrap justify-between gap-2">
              <dt className="text-slate-500">Имя</dt>
              <dd className="font-medium text-slate-900">
                {user?.name ?? "—"}
              </dd>
            </div>
            <div className="flex flex-wrap items-center justify-between gap-2">
              <dt className="text-slate-500">Роли из токена</dt>
              <dd className="flex flex-wrap gap-1">
                {userRoles.length > 0 ? (
                  userRoles.map((role) => <RoleBadge key={role} role={role} />)
                ) : (
                  <span className="text-slate-400">—</span>
                )}
              </dd>
            </div>
          </dl>
        </section>

        <section className="flex flex-wrap gap-3">
          <Link
            href={routes.profile}
            className="inline-flex h-9 items-center justify-center rounded-md bg-slate-900 px-4 text-sm font-medium text-white transition-colors hover:bg-slate-800"
          >
            Мой профиль
          </Link>
          {staff && (
            <Link
              href={resolveHomeRoute(userRoles)}
              className="inline-flex h-9 items-center justify-center rounded-md border border-slate-300 px-4 text-sm font-medium text-slate-700 transition-colors hover:bg-slate-100"
            >
              Админ-панель
            </Link>
          )}
        </section>
      </main>
    </RequireAuth>
  );
}
