"use client";

import type { ClanMember } from "@/src/entities/clans";
import { hasAnyRole, isStaff, Role } from "@/src/shared/auth/roles";
import { routes } from "@/src/shared/routes";
import {
  sessionSelectors,
  useSessionStore,
} from "@/src/shared/stores/session-store";
import { ApiErrorList } from "@/src/shared/ui/api-error-list";
import { Button } from "@/src/shared/ui/button";
import { ConfirmDialog } from "@/src/shared/ui/confirm-dialog";
import { Spinner } from "@/src/shared/ui/spinner";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState } from "react";
import {
  useClanDetails,
  useDeleteClan,
  useJoinClan,
  useKickMember,
  useLeaveClan,
} from "../model/use-clans";

export function ClanDetailsPanel({ clanId }: { clanId: string }) {
  const router = useRouter();
  const user = useSessionStore(sessionSelectors.user);

  const { clan, isPending, error } = useClanDetails(clanId);
  const { joinClan, isPending: isJoining, error: joinError } = useJoinClan();
  const { leaveClan, isPending: isLeaving, error: leaveError } = useLeaveClan();
  const {
    deleteClan,
    isPending: isDeleting,
    error: deleteError,
  } = useDeleteClan();
  const {
    kickMember,
    isPending: isKicking,
    error: kickError,
  } = useKickMember();

  const [kickTarget, setKickTarget] = useState<ClanMember | null>(null);
  const [confirmDelete, setConfirmDelete] = useState(false);

  if (isPending) {
    return <Spinner label="Загружаем клан..." />;
  }

  if (error) {
    return <ApiErrorList error={error} fallback="Не удалось загрузить клан" />;
  }

  if (!clan) {
    return null;
  }

  const roles = user?.roles ?? [];
  const isAdmin = hasAnyRole(roles, [Role.ADMIN]);
  const staff = isStaff(roles);
  const isMember = clan.members.some((member) => member.userId === user?.sub);
  const isLeader = clan.leaderId === user?.sub;
  const canDelete = isLeader || staff;
  const actionError = joinError ?? leaveError ?? deleteError ?? kickError;

  const handleDelete = async () => {
    setConfirmDelete(false);
    try {
      await deleteClan(clan.id);
      router.replace(routes.clans);
    } catch {
      // Ошибка уже в deleteError.
    }
  };

  const handleKick = async () => {
    if (!kickTarget) return;

    setKickTarget(null);
    try {
      await kickMember({ clanId: clan.id, userId: kickTarget.userId });
    } catch {
      // Ошибка уже в kickError.
    }
  };

  return (
    <div className="space-y-6">
      <header className="space-y-2">
        <div className="flex flex-wrap items-center gap-3">
          <span className="rounded bg-slate-900 px-2 py-0.5 font-mono text-xs font-medium text-white">
            {clan.tag}
          </span>
          <h1 className="text-2xl font-semibold text-slate-900">{clan.name}</h1>
          {isLeader && (
            <span className="rounded-full bg-emerald-50 px-2 py-0.5 text-xs font-medium text-emerald-700 ring-1 ring-emerald-200 ring-inset">
              вы лидер
            </span>
          )}
        </div>

        {clan.description && (
          <p className="text-sm text-slate-600">{clan.description}</p>
        )}

        <p className="text-xs text-slate-500">
          Участников: {clan.members.length} · создан{" "}
          {formatDate(clan.createdAt)}
        </p>
      </header>

      <div className="flex flex-wrap gap-3">
        {!isMember && (
          <Button onClick={() => void joinClan(clan.id)} disabled={isJoining}>
            {isJoining ? "Вступаем..." : "Вступить"}
          </Button>
        )}

        {isMember && !isLeader && (
          <Button
            variant="ghost"
            onClick={() => void leaveClan(clan.id)}
            disabled={isLeaving}
          >
            {isLeaving ? "Выходим..." : "Покинуть клан"}
          </Button>
        )}

        {canDelete && (
          <Button variant="ghost" onClick={() => setConfirmDelete(true)}>
            Удалить клан
          </Button>
        )}
      </div>

      <ApiErrorList
        error={actionError}
        fallback="Не удалось выполнить действие"
      />

      <section className="space-y-2">
        <h2 className="text-xs font-semibold tracking-wide text-slate-500 uppercase">
          Состав
        </h2>

        <ul className="divide-y divide-slate-100 rounded-lg border border-slate-200 bg-white">
          {clan.members.map((member) => {
            const leader = member.userId === clan.leaderId;

            return (
              <li
                key={member.userId}
                className="flex flex-wrap items-center gap-3 px-4 py-3"
              >
                <div className="min-w-0 flex-1">
                  <p className="text-sm font-medium text-slate-900">
                    {displayName(member)}
                    {member.userId === user?.sub && (
                      <span className="ml-1 text-xs font-normal text-slate-400">
                        (вы)
                      </span>
                    )}
                  </p>
                  <p className="text-xs break-all text-slate-500">
                    {member.email ?? member.userName ?? member.userId}
                  </p>
                </div>

                {leader && (
                  <span className="rounded-full bg-slate-100 px-2 py-0.5 text-xs font-medium text-slate-700 ring-1 ring-slate-200 ring-inset">
                    Лидер
                  </span>
                )}

                {isAdmin && !leader && (
                  <Button
                    variant="ghost"
                    onClick={() => setKickTarget(member)}
                    className="text-red-600 hover:bg-red-50"
                  >
                    Исключить
                  </Button>
                )}
              </li>
            );
          })}
        </ul>
      </section>

      <p className="text-xs text-slate-500">
        <Link href={routes.clans} className="underline">
          Ко всем кланам
        </Link>
      </p>

      <ConfirmDialog
        open={confirmDelete}
        title="Удалить клан?"
        description={`«${clan.name}» будет удалён вместе с составом. Действие необратимо.`}
        confirmLabel="Удалить"
        isPending={isDeleting}
        onConfirm={() => void handleDelete()}
        onCancel={() => setConfirmDelete(false)}
      />

      <ConfirmDialog
        open={kickTarget !== null}
        title="Исключить участника?"
        description={
          kickTarget
            ? `${displayName(kickTarget)} будет исключён из клана.`
            : undefined
        }
        confirmLabel="Исключить"
        isPending={isKicking}
        onConfirm={() => void handleKick()}
        onCancel={() => setKickTarget(null)}
      />
    </div>
  );
}

function displayName(member: ClanMember): string {
  return member.name ?? member.userName ?? member.userId;
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
