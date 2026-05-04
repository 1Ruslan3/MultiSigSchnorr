Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Write-Host "Opening psql inside multisig-postgres container..." -ForegroundColor Cyan
Write-Host "Use '\dt' to list tables and '\q' to exit." -ForegroundColor Gray
Write-Host ""

docker exec -it multisig-postgres psql -U multisig_user -d multisig_schnorr
