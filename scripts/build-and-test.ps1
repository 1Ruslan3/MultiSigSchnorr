param(
    [switch]$SkipIntegration
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$ProjectRoot = Resolve-Path (Join-Path $PSScriptRoot "..")

$UnitTests = Join-Path $ProjectRoot "tests\MultiSigSchnorr.Tests.Unit\MultiSigSchnorr.Tests.Unit.csproj"
$IntegrationTests = Join-Path $ProjectRoot "tests\MultiSigSchnorr.Tests.Integration\MultiSigSchnorr.Tests.Integration.csproj"
$CryptoVectorTests = Join-Path $ProjectRoot "tests\MultiSigSchnorr.Tests.CryptoVectors\MultiSigSchnorr.Tests.CryptoVectors.csproj"

Write-Host "Restoring solution..." -ForegroundColor Cyan
dotnet restore $ProjectRoot

Write-Host ""
Write-Host "Building solution..." -ForegroundColor Cyan
dotnet build $ProjectRoot --no-restore

Write-Host ""
Write-Host "Running unit tests..." -ForegroundColor Cyan
dotnet test $UnitTests --no-build

Write-Host ""
Write-Host "Running crypto-vector tests..." -ForegroundColor Cyan
dotnet test $CryptoVectorTests --no-build

if (-not $SkipIntegration) {
    Write-Host ""
    Write-Host "Running integration tests..." -ForegroundColor Cyan
    Write-Host "PostgreSQL must be running before this step." -ForegroundColor Yellow
    dotnet test $IntegrationTests --no-build
}
else {
    Write-Host ""
    Write-Host "Integration tests were skipped." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Build and test pipeline completed successfully." -ForegroundColor Green
