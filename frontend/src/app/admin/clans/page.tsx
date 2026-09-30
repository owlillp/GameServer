import { ClansExplorer } from "@/src/features/clans";

export default function AdminClansPage() {
  return (
    <div className="space-y-6">
      <header className="space-y-1">
        <h1 className="text-2xl font-semibold text-slate-900">Кланы</h1>
        <p className="text-sm text-slate-600">
          Просмотр и модерация кланов. Исключать участников может только роль
          Admin (permission clans.admin).
        </p>
      </header>

      <ClansExplorer admin />
    </div>
  );
}
