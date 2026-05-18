#!/usr/bin/env bash
set -euo pipefail

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
API_PROJECT="$PROJECT_ROOT/src/MultiSigSchnorr.Api/MultiSigSchnorr.Api.csproj"

if [[ ! -f "$API_PROJECT" ]]; then
  echo "API project was not found: $API_PROJECT" >&2
  exit 1
fi

echo "Running MultiSigSchnorr.Api..."
echo "Expected URL: http://localhost:5227"
echo

dotnet run --project "$API_PROJECT"
