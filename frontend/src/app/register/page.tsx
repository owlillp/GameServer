import { RegisterForm } from "@/src/features/register";
import { routes } from "@/src/shared/routes";
import Link from "next/link";

export default function RegisterPage() {
  return (
    <main className="mx-auto w-full max-w-md px-6 py-10">
      <h1 className="mb-6 text-2xl font-semibold text-slate-900">
        Регистрация
      </h1>

      <section className="rounded-lg border border-slate-200 bg-white p-6">
        <RegisterForm />
      </section>

      <p className="mt-4 text-sm text-slate-600">
        Уже есть аккаунт?{" "}
        <Link
          href={routes.login}
          className="font-medium text-slate-900 underline"
        >
          Войти
        </Link>
      </p>
    </main>
  );
}
