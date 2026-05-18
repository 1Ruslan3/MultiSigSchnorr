#!/usr/bin/env bash
set -euo pipefail

MIGRATION_NAME="${1:-}"

if [[ -z "$MIGRATION_NAME" ]]; then
  echo "Usage: ./scripts/add-migration.sh MigrationName" >&2
  exit 1
fi

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
INFRA_PROJECT="$PROJECT_ROOT/src/MultiSigSchnorr.Infrastructure/MultiSigSchnorr.Infrastructure.csproj"
API_PROJECT="$PROJECT_ROOT/src/MultiSigSchnorr.Api/MultiSigSchnorr.Api.csproj"

if [[ ! -f "$INFRA_PROJECT" ]]; then
  echo "Infrastructure project was not found: $INFRA_PROJECT" >&2
  exit 1
fi

if [[ ! -f "$API_PROJECT" ]]; then
  echo "API project was not found: $API_PROJECT" >&2
  exit 1
fi

echo "Creating EF Core migration: $MIGRATION_NAME"

dotnet ef migrations add "$MIGRATION_NAME" \
  --project "$INFRA_PROJECT" \
  --startup-project "$API_PROJECT" \
  --context MultiSigSchnorrDbContext \
  --output-dir Persistence/Migrations

echo "Migration '$MIGRATION_NAME' was created successfully."
