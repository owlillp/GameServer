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
import { registerSchema, type RegisterFormValues } from "../model/schema";
import { useJwtRegister } from "../model/use-jwt-register";
import { useRegister } from "../model/use-register";

export function RegisterForm() {
  const router = useRouter();
  const [scheme, setScheme] = useState<AuthScheme>("cookie");

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<RegisterFormValues>({
    resolver: zodResolver(registerSchema),
    mode: "onTouched",
    defaultValues: { email: "", userName: "", password: "" },
  });

  // Схема регистрации определяет endpoint: /auth/register или /auth/jwt/register.
  const cookieMutation = useRegister();
  const jwtMutation = useJwtRegister();
  const active = scheme === "cookie" ? cookieMutation : jwtMutation;

  const onSubmit = handleSubmit(async (values) => {
    try {
      if (scheme === "cookie") {
        await cookieMutation.register(values);
      } else {
        await jwtMutation.jwtRegister(values);
      }
      // После регистрации токен/кука не выдаются — отправляем на вход.
      router.push(routes.login);
    } catch {
      // Ошибка уже в active.error — отрисуем ниже.
    }
  });

  return (
    <form onSubmit={onSubmit} className="space-y-4" noValidate>
      <div>
        <span className="mb-1 block text-sm font-medium text-slate-700">
          Способ регистрации
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
          autoComplete="email"
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
          htmlFor="userName"
        >
          Имя пользователя
        </label>
        <Input
          id="userName"
          autoComplete="username"
          required
          enterKeyHint="next"
          {...register("userName")}
        />
        {errors.userName && (
          <p className="mt-1 text-xs text-red-600">{errors.userName.message}</p>
        )}
      </div>

      <div>
        <label
          className="mb-1 block text-sm font-medium text-slate-700"
          htmlFor="new-password"
        >
          Пароль
        </label>
        <PasswordInput
          id="new-password"
          autoComplete="new-password"
          required
          enterKeyHint="done"
          minLength={8}
          aria-describedby="password-constraints"
          {...register("password")}
        />
        <p id="password-constraints" className="mt-1 text-xs text-slate-500">
          Минимум 8 символов, строчная и заглавная буквы, цифра.
        </p>
        {errors.password && (
          <p className="mt-1 text-xs text-red-600">{errors.password.message}</p>
        )}
      </div>

      <Button
        type="submit"
        disabled={isSubmitting || active.isPending}
        className="w-full"
      >
        {active.isPending
          ? "Регистрируем..."
          : scheme === "cookie"
            ? "Зарегистрироваться (cookie)"
            : "Зарегистрироваться (JWT)"}
      </Button>

      <ApiErrorList
        error={active.error}
        fallback="Не удалось зарегистрироваться"
      />
    </form>
  );
}
