Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$ProjectRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$ComposeFile = Join-Path $ProjectRoot "deploy\docker-compose.postgres.yml"

if (-not (Test-Path $ComposeFile)) {
    throw "Docker Compose file was not found: $ComposeFile"
}

Write-Host "Starting PostgreSQL for demo..." -ForegroundColor Cyan
docker compose -f $ComposeFile up -d

Write-Host ""
Write-Host "Applying migrations..." -ForegroundColor Cyan
& (Join-Path $PSScriptRoot "apply-migrations.ps1")

Write-Host ""
Write-Host "Demo infrastructure is ready." -ForegroundColor Green
Write-Host "Now run API in one PowerShell window:" -ForegroundColor Gray
Write-Host ".\scripts\run-api.ps1" -ForegroundColor White
Write-Host ""
Write-Host "Then run Web in another PowerShell window:" -ForegroundColor Gray
Write-Host ".\scripts\run-web.ps1" -ForegroundColor White
Write-Host ""
Write-Host "Open:" -ForegroundColor Gray
Write-Host "http://localhost:5080/system-overview" -ForegroundColor White
