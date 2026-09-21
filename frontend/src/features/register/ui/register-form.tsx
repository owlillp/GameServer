"use client";

import { getErrorMessage, isEnvelopeError } from "@/src/shared/api/errors";
import { Button } from "@/src/shared/ui/button";
import { Input } from "@/src/shared/ui/input";
import { zodResolver } from "@hookform/resolvers/zod";
import { useRouter } from "next/navigation";
import { useForm } from "react-hook-form";
import { registerSchema, type RegisterFormValues } from "../model/schema";
import { useRegister } from "../model/use-register";

export function RegisterForm() {
  const router = useRouter();
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<RegisterFormValues>({
    resolver: zodResolver(registerSchema),
    defaultValues: { email: "", userName: "", password: "" },
  });

  const {
    register: registerAccount,
    isPending,
    isError,
    error,
  } = useRegister();

  const onSubmit = handleSubmit(async (values) => {
    try {
      await registerAccount(values);
      router.push("/login");
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
          htmlFor="userName"
        >
          Имя пользователя
        </label>
        <Input
          id="userName"
          autoComplete="username"
          {...register("userName")}
        />
        {errors.userName && (
          <p className="mt-1 text-xs text-red-600">{errors.userName.message}</p>
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
          autoComplete="new-password"
          {...register("password")}
        />
        {errors.password && (
          <p className="mt-1 text-xs text-red-600">{errors.password.message}</p>
        )}
      </div>

      <Button type="submit" disabled={isPending} className="w-full">
        {isPending ? "Регистрируем..." : "Зарегистрироваться"}
      </Button>

      {isError && (
        <div className="space-y-1 text-sm text-red-600">
          {isEnvelopeError(error) ? (
            error.allMessages.map((message, index) => (
              <p key={`${index}-${message}`}>{message}</p>
            ))
          ) : (
            <p>{getErrorMessage(error, "Не удалось зарегистрироваться")}</p>
          )}
        </div>
      )}
    </form>
  );
}
