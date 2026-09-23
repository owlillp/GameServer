import { cn } from "@/src/shared/lib/utils";
import type { AuthScheme } from "@/src/shared/stores/session-store";

type Props = {
  value: AuthScheme;
  onChange: (scheme: AuthScheme) => void;
  disabled?: boolean;
};

const SCHEMES: { value: AuthScheme; label: string; sub: string }[] = [
  { value: "cookie", label: "Cookie", sub: "Identity.Application" },
  { value: "jwt", label: "JWT", sub: "Bearer" },
];

// Нативный radio-group на кнопках: выбор схемы аутентификации в формах.
export function SchemeToggle({ value, onChange, disabled }: Props) {
  return (
    <div
      className="grid grid-cols-2 gap-2"
      role="radiogroup"
      aria-label="Схема аутентификации"
    >
      {SCHEMES.map((scheme) => {
        const selected = scheme.value === value;

        return (
          <button
            key={scheme.value}
            type="button"
            role="radio"
            aria-checked={selected}
            disabled={disabled}
            onClick={() => onChange(scheme.value)}
            className={cn(
              "rounded-md border px-3 py-2 text-left transition-colors disabled:opacity-50",
              selected
                ? "border-slate-900 bg-slate-900 text-white"
                : "border-slate-300 bg-white text-slate-700 hover:border-slate-500",
            )}
          >
            <span className="block text-sm font-medium">{scheme.label}</span>
            <span
              className={cn(
                "block text-xs",
                selected ? "text-slate-300" : "text-slate-500",
              )}
            >
              {scheme.sub}
            </span>
          </button>
        );
      })}
    </div>
  );
}
