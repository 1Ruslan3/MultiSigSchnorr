Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$ProjectRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$BenchmarkProject = Join-Path $ProjectRoot "tests\MultiSigSchnorr.Benchmarks\MultiSigSchnorr.Benchmarks.csproj"

if (-not (Test-Path $BenchmarkProject)) {
    throw "Benchmark project was not found: $BenchmarkProject"
}

Write-Host "Running benchmarks in Release configuration..." -ForegroundColor Cyan
Write-Host "This may take some time." -ForegroundColor Yellow
Write-Host ""

dotnet run --project $BenchmarkProject -c Release
