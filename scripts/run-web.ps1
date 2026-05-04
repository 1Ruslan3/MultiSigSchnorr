Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$ProjectRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$WebProject = Join-Path $ProjectRoot "src\MultiSigSchnorr.Web\MultiSigSchnorr.Web.csproj"

if (-not (Test-Path $WebProject)) {
    throw "Web project was not found: $WebProject"
}

Write-Host "Running MultiSigSchnorr.Web..." -ForegroundColor Cyan
Write-Host "Expected URL: http://localhost:5080" -ForegroundColor Gray
Write-Host ""

dotnet run --project $WebProject
