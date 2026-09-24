import { routes } from "@/src/shared/routes";
import Link from "next/link";

export default function Home() {
  return (
    <div className="flex flex-1 flex-col items-center justify-center gap-6 p-8">
      <div className="text-center">
        <h1 className="text-3xl font-bold tracking-tight">Game Server</h1>
        <p className="mt-2 text-sm text-slate-600">
          Панель управления игровым сервером
        </p>
      </div>

      <div className="flex items-center gap-3">
        <Link
          href={routes.login}
          className="inline-flex h-9 items-center justify-center rounded-md bg-slate-900 px-4 text-sm font-medium text-white transition-colors hover:bg-slate-800"
        >
          Войти
        </Link>
        <Link
          href={routes.register}
          className="inline-flex h-9 items-center justify-center rounded-md border border-slate-300 px-4 text-sm font-medium text-slate-700 transition-colors hover:bg-slate-100"
        >
          Регистрация
        </Link>
        <Link
          href={routes.profile}
          className="inline-flex h-9 items-center justify-center rounded-md border border-slate-300 px-4 text-sm font-medium text-slate-700 transition-colors hover:bg-slate-100"
        >
          Мой профиль
        </Link>
      </div>
    </div>
  );
}
