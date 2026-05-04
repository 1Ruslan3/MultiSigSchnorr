param(
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$Name
)

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

Write-Host "Creating EF Core migration: $Name" -ForegroundColor Cyan

dotnet ef migrations add $Name `
    --project $InfrastructureProject `
    --startup-project $ApiProject `
    --context MultiSigSchnorrDbContext `
    --output-dir Persistence\Migrations

Write-Host "Migration '$Name' was created successfully." -ForegroundColor Green
