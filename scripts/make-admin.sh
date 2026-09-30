#!/usr/bin/env bash
set -euo pipefail

# Выдаёт роль Admin аккаунту по email в локальном docker compose окружении.
# Использование: ./scripts/make-admin.sh <email>

EMAIL="${1:-}"
if [ -z "$EMAIL" ]; then
    echo "Использование: $0 <email>" >&2
    exit 1
fi

POSTGRES_CONTAINER="${POSTGRES_CONTAINER:-db-postgres}"
POSTGRES_USER="${POSTGRES_USER:-postgres}"
POSTGRES_DB="${POSTGRES_DB:-auth_service_db}"

docker exec -i "$POSTGRES_CONTAINER" psql -U "$POSTGRES_USER" -d "$POSTGRES_DB" -v ON_ERROR_STOP=1 <<SQL
INSERT INTO auth.account_roles (account_id, role_id)
SELECT a.id, r.id
FROM auth.accounts a
CROSS JOIN auth.roles r
WHERE a.normalized_email = UPPER('$EMAIL')
  AND r.normalized_name = UPPER('Admin')
ON CONFLICT DO NOTHING;

SELECT a.email, r.name AS role
FROM auth.accounts a
JOIN auth.account_roles ar ON ar.account_id = a.id
JOIN auth.roles r ON r.id = ar.role_id
WHERE a.normalized_email = UPPER('$EMAIL')
ORDER BY r.name;
SQL

echo "Готово. Войдите заново (или обновите токен), чтобы получить роль Admin."
