Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$ProjectRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$InfrastructureProject = Join-Path $ProjectRoot "src\MultiSigSchnorr.Infrastructure\MultiSigSchnorr.Infrastructure.csproj"
$ApiProject = Join-Path $ProjectRoot "src\MultiSigSchnorr.Api\MultiSigSchnorr.Api.csproj"

if (-not (Test-Path $InfrastructureProject)) {
    throw "Infrastructure project was not found: $InfrastructureProject"
}

if (-not (Test-Path $ApiProject)) {
    throw "API project was not found: $ApiProject"
}

Write-Host "Applying EF Core migrations..." -ForegroundColor Cyan

dotnet ef database update `
    --project $InfrastructureProject `
    --startup-project $ApiProject `
    --context MultiSigSchnorrDbContext

Write-Host "Database migrations were applied successfully." -ForegroundColor Green
