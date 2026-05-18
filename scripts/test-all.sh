#!/usr/bin/env bash
set -euo pipefail

SKIP_INTEGRATION="${1:-}"

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

UNIT_TESTS="$PROJECT_ROOT/tests/MultiSigSchnorr.Tests.Unit/MultiSigSchnorr.Tests.Unit.csproj"
INTEGRATION_TESTS="$PROJECT_ROOT/tests/MultiSigSchnorr.Tests.Integration/MultiSigSchnorr.Tests.Integration.csproj"
CRYPTO_VECTOR_TESTS="$PROJECT_ROOT/tests/MultiSigSchnorr.Tests.CryptoVectors/MultiSigSchnorr.Tests.CryptoVectors.csproj"

echo "Running unit tests..."
dotnet test "$UNIT_TESTS"

echo
echo "Running crypto-vector tests..."
dotnet test "$CRYPTO_VECTOR_TESTS"

if [[ "$SKIP_INTEGRATION" == "--skip-integration" ]]; then
  echo
  echo "Integration tests were skipped."
else
  echo
  echo "Running integration tests..."
  echo "PostgreSQL must be running before this step."
  dotnet test "$INTEGRATION_TESTS"
fi

echo
echo "All selected tests completed successfully."
