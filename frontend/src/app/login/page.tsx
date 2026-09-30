import { LoginForm } from "@/src/features/login";
import { RedirectIfAuthenticated } from "@/src/shared/auth/redirect-if-authenticated";
import { sanitizeAuthReturnUrl } from "@/src/shared/auth/return-url";
import { routes } from "@/src/shared/routes";
import Link from "next/link";

type SearchParams = {
  returnUrl?: string | string[];
};

export default async function LoginPage({
  searchParams,
}: {
  searchParams: Promise<SearchParams>;
}) {
  const params = await searchParams;
  const rawReturnUrl = Array.isArray(params.returnUrl)
    ? params.returnUrl[0]
    : params.returnUrl;
  const returnUrl = sanitizeAuthReturnUrl(rawReturnUrl);

  const registerHref = returnUrl
    ? `${routes.register}?returnUrl=${encodeURIComponent(returnUrl)}`
    : routes.register;

  return (
    <main className="mx-auto flex w-full max-w-md flex-1 flex-col justify-center px-6 py-10">
      <Link
        href={routes.home}
        className="mb-6 text-sm font-semibold tracking-tight text-slate-900"
      >
        Game Server
      </Link>

      <h1 className="mb-6 text-2xl font-semibold text-slate-900">Вход</h1>

      <RedirectIfAuthenticated>
        <section className="rounded-lg border border-slate-200 bg-white p-6">
          <LoginForm returnUrl={returnUrl} />
        </section>

        <p className="mt-4 text-sm text-slate-600">
          Нет аккаунта?{" "}
          <Link
            href={registerHref}
            className="font-medium text-slate-900 underline"
          >
            Зарегистрироваться
          </Link>
        </p>
      </RedirectIfAuthenticated>
    </main>
  );
}
