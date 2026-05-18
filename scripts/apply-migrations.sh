#!/usr/bin/env bash
set -euo pipefail

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

echo "Applying EF Core migrations..."

dotnet ef database update \
  --project "$INFRA_PROJECT" \
  --startup-project "$API_PROJECT" \
  --context MultiSigSchnorrDbContext

echo "Database migrations were applied successfully."
