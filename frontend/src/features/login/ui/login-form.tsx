"use client";

import { routes } from "@/src/shared/routes";
import type { AuthScheme } from "@/src/shared/stores/session-store";
import { ApiErrorList } from "@/src/shared/ui/api-error-list";
import { Button } from "@/src/shared/ui/button";
import { Input } from "@/src/shared/ui/input";
import { PasswordInput } from "@/src/shared/ui/password-input";
import { SchemeToggle } from "@/src/shared/ui/scheme-toggle";
import { zodResolver } from "@hookform/resolvers/zod";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { loginSchema, type LoginFormValues } from "../model/schema";
import { useJwtLogin } from "../model/use-jwt-login";
import { useLogin } from "../model/use-login";

export function LoginForm() {
  const router = useRouter();
  const [scheme, setScheme] = useState<AuthScheme>("cookie");

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<LoginFormValues>({
    resolver: zodResolver(loginSchema),
    mode: "onTouched",
    defaultValues: { email: "", password: "" },
  });

  // Обе мутации живут одновременно — активную выбираем по схеме.
  const cookieMutation = useLogin();
  const jwtMutation = useJwtLogin();
  const active = scheme === "cookie" ? cookieMutation : jwtMutation;

  const onSubmit = handleSubmit(async (values) => {
    try {
      if (scheme === "cookie") {
        await cookieMutation.login(values);
      } else {
        await jwtMutation.jwtLogin(values);
      }
      // После успешного логина попадаем на профиль.
      router.push(routes.profile);
    } catch {
      // Ошибка уже в active.error — отрисуем ниже.
    }
  });

  return (
    <form onSubmit={onSubmit} className="space-y-4" noValidate>
      <div>
        <span className="mb-1 block text-sm font-medium text-slate-700">
          Способ входа
        </span>
        <SchemeToggle
          value={scheme}
          onChange={setScheme}
          disabled={active.isPending}
        />
      </div>

      <div>
        <label
          className="mb-1 block text-sm font-medium text-slate-700"
          htmlFor="email"
        >
          Email
        </label>
        <Input
          id="email"
          type="email"
          autoComplete="username"
          required
          enterKeyHint="next"
          {...register("email")}
        />
        {errors.email && (
          <p className="mt-1 text-xs text-red-600">{errors.email.message}</p>
        )}
      </div>

      <div>
        <label
          className="mb-1 block text-sm font-medium text-slate-700"
          htmlFor="current-password"
        >
          Пароль
        </label>
        <PasswordInput
          id="current-password"
          autoComplete="current-password"
          required
          enterKeyHint="done"
          {...register("password")}
        />
        {errors.password && (
          <p className="mt-1 text-xs text-red-600">{errors.password.message}</p>
        )}
      </div>

      <Button type="submit" disabled={isSubmitting || active.isPending} className="w-full">
        {active.isPending
          ? "Входим..."
          : scheme === "cookie"
            ? "Войти (cookie)"
            : "Войти (JWT)"}
      </Button>

      <ApiErrorList error={active.error} fallback="Не удалось войти" />
    </form>
  );
}
