"use client";

import { jwtSessionQueryOptions } from "@/src/entities/auth";
import { useIsHydrated } from "@/src/shared/lib/use-is-hydrated";
import {
  sessionSelectors,
  useSessionStore,
} from "@/src/shared/stores/session-store";
import { ApiErrorList } from "@/src/shared/ui/api-error-list";
import { Button } from "@/src/shared/ui/button";
import { Spinner } from "@/src/shared/ui/spinner";
import { useQuery } from "@tanstack/react-query";
import { useJwtRefresh } from "../model/use-jwt-refresh";

// Карточка состояния JWT-сессии: access-токен + refresh-сессия (HttpOnly cookie).
// Показывается только для схемы "jwt".
export function JwtSessionCard() {
  const hydrated = useIsHydrated();
  const scheme = useSessionStore(sessionSelectors.scheme);
  const user = useSessionStore(sessionSelectors.user);

  const { data, isLoading, error, refetch, isFetching } = useQuery({
    ...jwtSessionQueryOptions(),
    enabled: hydrated && scheme === "jwt",
  });

  const { refresh, isPending, error: refreshError } = useJwtRefresh();

  if (!hydrated || scheme !== "jwt") return null;

  if (isLoading) {
    return <Spinner label="Проверяем JWT-сессию..." />;
  }

  return (
    <section className="space-y-3">
      <div className="flex items-center justify-between gap-4">
        <h3 className="text-xs font-semibold tracking-wide text-slate-500 uppercase">
          JWT-сессия
        </h3>
        <button
          type="button"
          onClick={() => refetch()}
          disabled={isFetching}
          className="text-xs text-slate-500 underline-offset-2 hover:text-slate-900 hover:underline disabled:opacity-50"
        >
          {isFetching ? "Обновляем..." : "Обновить статус"}
        </button>
      </div>

      {error && (
        <ApiErrorList
          error={error}
          fallback="Не удалось получить статус сессии"
        />
      )}

      {data && (
        <dl className="grid grid-cols-1 gap-y-2 rounded-md border border-slate-200 px-4 py-3 text-sm sm:grid-cols-[200px_1fr]">
          <Field
            label="Access-токен истекает"
            value={user?.expiresAt ? formatTime(user.expiresAt) : null}
          />
          <Field
            label="Refresh-cookie"
            value={data.hasRefreshCookie ? "есть" : "нет"}
          />
          <Field
            label="Refresh-сессия активна"
            value={data.isRefreshSessionActive ? "да" : "нет"}
          />
          <Field
            label="Refresh истекает"
            value={
              data.refreshSessionExpiresAt
                ? formatDateTime(data.refreshSessionExpiresAt)
                : null
            }
          />
          <Field
            label="Отозвана"
            value={
              data.refreshSessionRevokedAt
                ? formatDateTime(data.refreshSessionRevokedAt)
                : null
            }
          />
        </dl>
      )}

      <div className="flex items-center gap-3">
        <Button onClick={() => refresh()} disabled={isPending}>
          {isPending ? "Обновляем..." : "Обновить access-токен"}
        </Button>
      </div>

      {refreshError && (
        <ApiErrorList error={refreshError} fallback="Не удалось обновить токен" />
      )}

      <p className="text-xs text-slate-500">
        При 401 access-токен обновляется автоматически (один раз) и запрос
        повторяется. Refresh-токен хранится в HttpOnly cookie и недоступен из JS.
      </p>
    </section>
  );
}

function Field({ label, value }: { label: string; value: string | null }) {
  return (
    <>
      <dt className="text-slate-500">{label}</dt>
      <dd className={value === null ? "text-slate-400 italic" : "text-slate-900"}>
        {value ?? "—"}
      </dd>
    </>
  );
}

function formatTime(date: Date): string {
  return date.toLocaleTimeString("ru-RU");
}

function formatDateTime(iso: string): string {
  const date = new Date(iso);
  if (Number.isNaN(date.getTime())) return iso;

  return date.toLocaleString("ru-RU", {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
  });
}
