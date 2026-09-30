"use client";

import { ClanDetailsPanel } from "@/src/features/clans";
import { RequireAuth } from "@/src/shared/auth/require-auth";
import { SiteHeader } from "@/src/shared/ui/site-header";
import { useParams } from "next/navigation";

export default function ClanDetailsPage() {
  const params = useParams<{ clanId: string }>();

  return (
    <RequireAuth>
      <SiteHeader />

      <main className="mx-auto w-full max-w-2xl flex-1 px-6 py-10">
        <ClanDetailsPanel clanId={params.clanId} />
      </main>
    </RequireAuth>
  );
}
