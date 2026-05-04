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

Write-Host "Listing EF Core migrations..." -ForegroundColor Cyan

dotnet ef migrations list `
    --project $InfrastructureProject `
    --startup-project $ApiProject `
    --context MultiSigSchnorrDbContext
