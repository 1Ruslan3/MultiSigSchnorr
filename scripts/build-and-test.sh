#!/usr/bin/env bash
set -euo pipefail

SKIP_INTEGRATION="${1:-}"

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

UNIT_TESTS="$PROJECT_ROOT/tests/MultiSigSchnorr.Tests.Unit/MultiSigSchnorr.Tests.Unit.csproj"
INTEGRATION_TESTS="$PROJECT_ROOT/tests/MultiSigSchnorr.Tests.Integration/MultiSigSchnorr.Tests.Integration.csproj"
CRYPTO_VECTOR_TESTS="$PROJECT_ROOT/tests/MultiSigSchnorr.Tests.CryptoVectors/MultiSigSchnorr.Tests.CryptoVectors.csproj"

echo "Restoring solution..."
dotnet restore "$PROJECT_ROOT"

echo
echo "Building solution..."
dotnet build "$PROJECT_ROOT" --no-restore

echo
echo "Running unit tests..."
dotnet test "$UNIT_TESTS" --no-build

echo
echo "Running crypto-vector tests..."
dotnet test "$CRYPTO_VECTOR_TESTS" --no-build

if [[ "$SKIP_INTEGRATION" == "--skip-integration" ]]; then
  echo
  echo "Integration tests were skipped."
else
  echo
  echo "Running integration tests..."
  echo "PostgreSQL must be running before this step."
  dotnet test "$INTEGRATION_TESTS" --no-build
fi

echo
echo "Build and test pipeline completed successfully."
