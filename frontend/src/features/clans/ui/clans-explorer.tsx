"use client";

import { cn } from "@/src/shared/lib/utils";
import { routes } from "@/src/shared/routes";
import { ApiErrorList } from "@/src/shared/ui/api-error-list";
import { Button } from "@/src/shared/ui/button";
import { Input } from "@/src/shared/ui/input";
import { Spinner } from "@/src/shared/ui/spinner";
import Link from "next/link";
import { useState } from "react";
import { useDebounce } from "use-debounce";
import { useClansList } from "../model/use-clans";
import { CreateClanForm } from "./create-clan-form";

const PAGE_SIZE = 12;

export function ClansExplorer({ admin = false }: { admin?: boolean }) {
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const [showCreate, setShowCreate] = useState(false);
  const [debouncedSearch] = useDebounce(search, 300);

  const { clans, totalCount, totalPages, isPending, isFetching, error } =
    useClansList({ page, pageSize: PAGE_SIZE, search: debouncedSearch });

  const from = totalCount === 0 ? 0 : (page - 1) * PAGE_SIZE + 1;
  const to = Math.min(page * PAGE_SIZE, totalCount);

  return (
    <div>
      <div className="mb-4 flex flex-wrap items-end justify-between gap-3">
        <div className="w-full max-w-xs">
          <label
            htmlFor="clan-search"
            className="mb-1 block text-sm font-medium text-slate-700"
          >
            Поиск
          </label>
          <Input
            id="clan-search"
            name="search"
            type="search"
            enterKeyHint="search"
            placeholder="Название или тег"
            value={search}
            onChange={(event) => {
              setSearch(event.target.value);
              setPage(1);
            }}
          />
        </div>

        <div className="flex items-center gap-3">
          <p role="status" className="text-xs text-slate-500">
            Найдено: {totalCount}
          </p>
          <Button onClick={() => setShowCreate((value) => !value)}>
            {showCreate ? "Свернуть форму" : "Создать клан"}
          </Button>
        </div>
      </div>

      {admin && (
        <p className="mb-4 rounded-md border border-amber-200 bg-amber-50 px-3 py-2 text-xs text-amber-800">
          Режим админ-панели: открой клан, чтобы исключить участника (только
          Admin).
        </p>
      )}

      {showCreate && (
        <div className="mb-6">
          <CreateClanForm onCreated={() => setShowCreate(false)} />
        </div>
      )}

      {error ? (
        <ApiErrorList error={error} fallback="Не удалось загрузить кланы" />
      ) : isPending ? (
        <Spinner label="Загружаем кланы..." />
      ) : clans.length === 0 ? (
        <p className="rounded-lg border border-dashed border-slate-300 bg-white p-8 text-center text-sm text-slate-500">
          {debouncedSearch
            ? "Ничего не найдено."
            : "Кланов пока нет — создай первый."}
        </p>
      ) : (
        <div
          aria-busy={isFetching}
          className={cn("transition-opacity", isFetching && "opacity-60")}
        >
          <ul className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {clans.map((clan) => (
              <li key={clan.id}>
                <Link
                  href={`/clans/${clan.id}`}
                  className="block rounded-lg border border-slate-200 bg-white p-4 transition-colors hover:border-slate-400"
                >
                  <div className="flex items-center justify-between gap-2">
                    <span className="rounded bg-slate-900 px-2 py-0.5 font-mono text-xs font-medium text-white">
                      {clan.tag}
                    </span>
                    <span className="text-xs text-slate-500">
                      участников: {clan.memberCount}
                    </span>
                  </div>
                  <h3 className="mt-3 text-sm font-semibold text-slate-900">
                    {clan.name}
                  </h3>
                  <p className="mt-1 text-xs text-slate-500">
                    Создан {formatDate(clan.createdAt)}
                  </p>
                </Link>
              </li>
            ))}
          </ul>

          <nav
            aria-label="Постраничная навигация"
            className="mt-4 flex items-center justify-between gap-3"
          >
            <Button
              variant="ghost"
              disabled={page <= 1 || isFetching}
              onClick={() => setPage((current) => Math.max(1, current - 1))}
            >
              Назад
            </Button>
            <span className="text-xs text-slate-500">
              Показано {from}–{to} из {totalCount} · страница {page} из{" "}
              {totalPages}
            </span>
            <Button
              variant="ghost"
              disabled={page >= totalPages || isFetching}
              onClick={() => setPage((current) => current + 1)}
            >
              Вперёд
            </Button>
          </nav>
        </div>
      )}

      <p className="mt-6 text-xs text-slate-500">
        <Link href={routes.dashboard} className="underline">
          Вернуться в панель
        </Link>
      </p>
    </div>
  );
}

function formatDate(iso: string): string {
  const date = new Date(iso);
  if (Number.isNaN(date.getTime())) return iso;

  return date.toLocaleDateString("ru-RU", {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
  });
}
