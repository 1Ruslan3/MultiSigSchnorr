#!/usr/bin/env bash
set -euo pipefail

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
WEB_PROJECT="$PROJECT_ROOT/src/MultiSigSchnorr.Web/MultiSigSchnorr.Web.csproj"

if [[ ! -f "$WEB_PROJECT" ]]; then
  echo "Web project was not found: $WEB_PROJECT" >&2
  exit 1
fi

echo "Running MultiSigSchnorr.Web..."
echo "Expected URL: http://localhost:5080"
echo

dotnet run --project "$WEB_PROJECT"
