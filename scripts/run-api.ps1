Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$ProjectRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$ApiProject = Join-Path $ProjectRoot "src\MultiSigSchnorr.Api\MultiSigSchnorr.Api.csproj"

if (-not (Test-Path $ApiProject)) {
    throw "API project was not found: $ApiProject"
}

Write-Host "Running MultiSigSchnorr.Api..." -ForegroundColor Cyan
Write-Host "Expected URL: http://localhost:5227" -ForegroundColor Gray
Write-Host ""

dotnet run --project $ApiProject
