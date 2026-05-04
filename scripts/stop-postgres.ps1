param(
    [switch]$RemoveVolumes
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$ProjectRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$ComposeFile = Join-Path $ProjectRoot "deploy\docker-compose.postgres.yml"

if (-not (Test-Path $ComposeFile)) {
    throw "Docker Compose file was not found: $ComposeFile"
}

if ($RemoveVolumes) {
    Write-Host "Stopping PostgreSQL and removing Docker volumes..." -ForegroundColor Yellow
    Write-Host "This will delete PostgreSQL data." -ForegroundColor Red
    docker compose -f $ComposeFile down -v
}
else {
    Write-Host "Stopping PostgreSQL container..." -ForegroundColor Cyan
    docker compose -f $ComposeFile down
}

Write-Host "PostgreSQL stop command completed." -ForegroundColor Green
