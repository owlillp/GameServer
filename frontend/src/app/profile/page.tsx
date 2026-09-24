import { ProfileCard } from "@/src/features/profile";

export default function ProfilePage() {
  return (
    <main className="mx-auto w-full max-w-2xl space-y-6 px-6 py-10">
      <header className="space-y-1">
        <h1 className="text-2xl font-semibold text-slate-900">Мой профиль</h1>
        <p className="text-xs text-slate-500">
          GET /auth/profile — работает и под cookie, и под JWT. Сервер берёт
          пользователя из claims.
        </p>
      </header>

      <section className="rounded-lg border border-slate-200 bg-white p-6">
        <ProfileCard />
      </section>
    </main>
  );
}
