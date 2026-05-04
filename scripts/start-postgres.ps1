param(
    [switch]$FollowLogs
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$ProjectRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$ComposeFile = Join-Path $ProjectRoot "deploy\docker-compose.postgres.yml"

if (-not (Test-Path $ComposeFile)) {
    throw "Docker Compose file was not found: $ComposeFile"
}

Write-Host "Starting PostgreSQL container..." -ForegroundColor Cyan
docker compose -f $ComposeFile up -d

Write-Host ""
Write-Host "Current Docker containers:" -ForegroundColor Cyan
docker ps --filter "name=multisig-postgres"

if ($FollowLogs) {
    Write-Host ""
    Write-Host "Following PostgreSQL logs. Press Ctrl+C to stop watching logs." -ForegroundColor Yellow
    docker logs -f multisig-postgres
}
else {
    Write-Host ""
    Write-Host "PostgreSQL startup command completed." -ForegroundColor Green
    Write-Host "Use '.\scripts\open-psql.ps1' to open psql." -ForegroundColor Gray
}
