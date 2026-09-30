"use client";

import type { ClanSummary } from "@/src/entities/clans";
import { ApiErrorList } from "@/src/shared/ui/api-error-list";
import { Button } from "@/src/shared/ui/button";
import { Input } from "@/src/shared/ui/input";
import { Textarea } from "@/src/shared/ui/textarea";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { useCreateClan } from "../model/use-clans";

// Ограничения зеркалят ClanService.Domain.ClanConstants.
const createClanSchema = z.object({
  name: z
    .string()
    .min(1, "Название обязательно")
    .max(32, "Не более 32 символов"),
  tag: z
    .string()
    .min(2, "Минимум 2 символа")
    .max(5, "Максимум 5 символов")
    .regex(/^[A-Za-z0-9]+$/, "Только латинские буквы и цифры"),
  description: z.string().max(280, "Не более 280 символов").optional(),
});

type CreateClanValues = z.infer<typeof createClanSchema>;

export function CreateClanForm({
  onCreated,
}: {
  onCreated?: (clan: ClanSummary) => void;
}) {
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<CreateClanValues>({
    resolver: zodResolver(createClanSchema),
    mode: "onTouched",
    defaultValues: { name: "", tag: "", description: "" },
  });

  const { createClan, isPending, error } = useCreateClan();

  const onSubmit = handleSubmit(async (values) => {
    try {
      const clan = await createClan({
        name: values.name,
        tag: values.tag,
        description: values.description?.trim() || null,
      });

      reset();
      onCreated?.(clan);
    } catch {
      // Ошибка уже в error — покажем ниже.
    }
  });

  return (
    <form
      onSubmit={onSubmit}
      noValidate
      className="space-y-4 rounded-lg border border-slate-200 bg-white p-5"
    >
      <h2 className="text-sm font-semibold text-slate-900">Новый клан</h2>

      <div className="grid gap-4 sm:grid-cols-[1fr_8rem]">
        <div>
          <label
            htmlFor="clan-name"
            className="mb-1 block text-sm font-medium text-slate-700"
          >
            Название
          </label>
          <Input
            id="clan-name"
            autoComplete="off"
            maxLength={32}
            required
            enterKeyHint="next"
            {...register("name")}
          />
          {errors.name && (
            <p className="mt-1 text-xs text-red-600">{errors.name.message}</p>
          )}
        </div>

        <div>
          <label
            htmlFor="clan-tag"
            className="mb-1 block text-sm font-medium text-slate-700"
          >
            Тег
          </label>
          <Input
            id="clan-tag"
            autoComplete="off"
            autoCapitalize="characters"
            maxLength={5}
            required
            enterKeyHint="next"
            placeholder="ABC"
            aria-describedby="clan-tag-hint"
            {...register("tag")}
          />
          <p id="clan-tag-hint" className="mt-1 text-xs text-slate-500">
            2–5 символов, A–Z и 0–9
          </p>
          {errors.tag && (
            <p className="mt-1 text-xs text-red-600">{errors.tag.message}</p>
          )}
        </div>
      </div>

      <div>
        <label
          htmlFor="clan-description"
          className="mb-1 block text-sm font-medium text-slate-700"
        >
          Описание
        </label>
        <Textarea
          id="clan-description"
          rows={3}
          maxLength={280}
          placeholder="Коротко о клане"
          {...register("description")}
        />
        {errors.description && (
          <p className="mt-1 text-xs text-red-600">
            {errors.description.message}
          </p>
        )}
      </div>

      <Button type="submit" disabled={isPending}>
        {isPending ? "Создаём..." : "Создать клан"}
      </Button>

      <ApiErrorList error={error} fallback="Не удалось создать клан" />
    </form>
  );
}
