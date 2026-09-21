"use client";

import { useIsHydrated } from "@/src/shared/lib/use-is-hydrated";
import { routes } from "@/src/shared/routes";
import {
  sessionSelectors,
  useSessionStore,
} from "@/src/shared/stores/session-store";
import { Button } from "@/src/shared/ui/button";
import { Spinner } from "@/src/shared/ui/spinner";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useEffect } from "react";

export default function DashboardPage() {
  const router = useRouter();
  const hydrated = useIsHydrated();
  const account = useSessionStore(sessionSelectors.account);

  useEffect(() => {
    if (hydrated && !account) {
      router.replace(routes.login);
    }
  }, [hydrated, account, router]);

  if (!hydrated || !account) {
    return (
      <main className="flex flex-1 items-center justify-center p-10">
        <Spinner label="Загружаем профиль..." />
      </main>
    );
  }

  const handleLogout = () => {
    useSessionStore.getState().clear();
    router.replace(routes.login);
  };

  return (
    <main className="mx-auto w-full max-w-2xl px-6 py-10">
      <h1 className="text-2xl font-semibold text-slate-900">Игровая панель</h1>
      <p className="mt-1 text-sm text-slate-600">Вы успешно вошли в аккаунт.</p>

      <section className="mt-6 rounded-lg border border-slate-200 bg-white p-6">
        <dl className="space-y-3">
          <div className="flex justify-between gap-4">
            <dt className="text-sm text-slate-500">Имя пользователя</dt>
            <dd className="text-sm font-medium text-slate-900">
              {account.userName}
            </dd>
          </div>
          <div className="flex justify-between gap-4">
            <dt className="text-sm text-slate-500">Email</dt>
            <dd className="text-sm font-medium text-slate-900">
              {account.email}
            </dd>
          </div>
          <div className="flex justify-between gap-4">
            <dt className="text-sm text-slate-500">ID аккаунта</dt>
            <dd className="text-sm font-medium break-all text-slate-900">
              {account.accountId}
            </dd>
          </div>
        </dl>

        <div className="mt-6 flex items-center gap-3">
          <Link
            href={routes.home}
            className="inline-flex h-9 items-center justify-center rounded-md px-3 text-sm font-medium text-slate-700 transition-colors hover:bg-slate-100"
          >
            На главную
          </Link>
          <Button variant="ghost" onClick={handleLogout}>
            Выйти
          </Button>
        </div>
      </section>
    </main>
  );
}
