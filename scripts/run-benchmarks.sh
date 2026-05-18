#!/usr/bin/env bash
set -euo pipefail

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
BENCHMARK_PROJECT="$PROJECT_ROOT/tests/MultiSigSchnorr.Benchmarks/MultiSigSchnorr.Benchmarks.csproj"

if [[ ! -f "$BENCHMARK_PROJECT" ]]; then
  echo "Benchmark project was not found: $BENCHMARK_PROJECT" >&2
  exit 1
fi

echo "Running benchmarks in Release configuration..."
echo "This may take some time."
echo

dotnet run --project "$BENCHMARK_PROJECT" -c Release
