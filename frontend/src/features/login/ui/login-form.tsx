"use client";

import { getErrorMessage, isEnvelopeError } from "@/src/shared/api/errors";
import { Button } from "@/src/shared/ui/button";
import { Input } from "@/src/shared/ui/input";
import { zodResolver } from "@hookform/resolvers/zod";
import { useRouter } from "next/navigation";
import { useForm } from "react-hook-form";
import { loginSchema, type LoginFormValues } from "../model/schema";
import { useLogin } from "../model/use-login";

export function LoginForm() {
  const router = useRouter();
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginFormValues>({
    resolver: zodResolver(loginSchema),
    defaultValues: { email: "", password: "" },
  });

  const { login, isPending, isError, error } = useLogin();

  const onSubmit = handleSubmit(async (values) => {
    try {
      await login(values);
      router.push("/dashboard");
    } catch {
      // Ошибка уже в `error` — отрисуем ниже.
    }
  });

  return (
    <form onSubmit={onSubmit} className="space-y-3" noValidate>
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
          {...register("email")}
        />
        {errors.email && (
          <p className="mt-1 text-xs text-red-600">{errors.email.message}</p>
        )}
      </div>

      <div>
        <label
          className="mb-1 block text-sm font-medium text-slate-700"
          htmlFor="password"
        >
          Пароль
        </label>
        <Input
          id="password"
          type="password"
          autoComplete="current-password"
          {...register("password")}
        />
        {errors.password && (
          <p className="mt-1 text-xs text-red-600">{errors.password.message}</p>
        )}
      </div>

      <Button type="submit" disabled={isPending} className="w-full">
        {isPending ? "Входим..." : "Войти"}
      </Button>

      {isError && (
        <div className="space-y-1 text-sm text-red-600">
          {isEnvelopeError(error) ? (
            error.allMessages.map((message, index) => (
              <p key={`${index}-${message}`}>{message}</p>
            ))
          ) : (
            <p>{getErrorMessage(error, "Не удалось войти")}</p>
          )}
        </div>
      )}
    </form>
  );
}
