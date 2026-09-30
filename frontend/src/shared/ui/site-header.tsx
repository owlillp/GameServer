"use client";

import { useLogout } from "@/src/features/logout";
import { isStaff } from "@/src/shared/auth/roles";
import { cn } from "@/src/shared/lib/utils";
import { routes } from "@/src/shared/routes";
import {
  sessionSelectors,
  useSessionStore,
} from "@/src/shared/stores/session-store";
import { Button } from "@/src/shared/ui/button";
import Link from "next/link";
import { usePathname } from "next/navigation";

type NavItem = {
  href: string;
  label: string;
};

export function SiteHeader() {
  const pathname = usePathname();
  const user = useSessionStore(sessionSelectors.user);
  const isAuthenticated = useSessionStore(sessionSelectors.isAuthenticated);
  const { logout, isPending } = useLogout();

  const userRoles = user?.roles ?? [];

  const navItems: NavItem[] = isAuthenticated
    ? [
        { href: routes.dashboard, label: "Панель" },
        { href: routes.profile, label: "Профиль" },
        ...(isStaff(userRoles)
          ? [{ href: routes.admin, label: "Админ-панель" }]
          : []),
      ]
    : [];

  return (
    <header className="border-b border-slate-200 bg-white">
      <div className="mx-auto flex w-full max-w-5xl flex-wrap items-center gap-x-6 gap-y-2 px-6 py-3">
        <Link
          href={routes.home}
          className="text-sm font-semibold tracking-tight text-slate-900"
        >
          Game Server
        </Link>

        {navItems.length > 0 && (
          <nav aria-label="Основная навигация" className="flex items-center">
            {navItems.map((item) => {
              const active =
                pathname === item.href || pathname.startsWith(`${item.href}/`);

              return (
                <Link
                  key={item.href}
                  href={item.href}
                  aria-current={active ? "page" : undefined}
                  className={cn(
                    "rounded-md px-3 py-1.5 text-sm transition-colors",
                    active
                      ? "bg-slate-100 font-medium text-slate-900"
                      : "text-slate-600 hover:bg-slate-50 hover:text-slate-900",
                  )}
                >
                  {item.label}
                </Link>
              );
            })}
          </nav>
        )}

        <div className="ml-auto flex items-center gap-2">
          {isAuthenticated ? (
            <>
              <span className="hidden text-xs text-slate-500 sm:inline">
                {user?.name ?? user?.email ?? user?.sub}
              </span>
              <Button
                variant="ghost"
                onClick={() => void logout()}
                disabled={isPending}
              >
                {isPending ? "Выходим..." : "Выйти"}
              </Button>
            </>
          ) : (
            <>
              <Link
                href={routes.login}
                className="inline-flex h-9 items-center justify-center rounded-md px-3 text-sm font-medium text-slate-700 transition-colors hover:bg-slate-100"
              >
                Войти
              </Link>
              <Link
                href={routes.register}
                className="inline-flex h-9 items-center justify-center rounded-md bg-slate-900 px-4 text-sm font-medium text-white transition-colors hover:bg-slate-800"
              >
                Регистрация
              </Link>
            </>
          )}
        </div>
      </div>
    </header>
  );
}
