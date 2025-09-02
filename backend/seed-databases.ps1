# PowerShell script to seed both production and test databases
param(
    [switch]$Force
)

Write-Host "=== SmartTelehealth Database Seeding Script ===" -ForegroundColor Green
Write-Host "This script will seed both production and test databases with initial data" -ForegroundColor Yellow

if (-not $Force) {
    $confirmation = Read-Host "Are you sure you want to proceed? This will add seed data to the databases! (y/N)"
    if ($confirmation -ne 'y' -and $confirmation -ne 'Y') {
        Write-Host "Operation cancelled." -ForegroundColor Red
        exit 0
    }
}

Write-Host "`nStep 1: Seeding Production Database..." -ForegroundColor Cyan
try {
    $env:ASPNETCORE_ENVIRONMENT = ""
    Write-Host "Starting production application to trigger seeding..." -ForegroundColor White
    Start-Process -FilePath "dotnet" -ArgumentList "run", "--project", "SmartTelehealth.API", "--no-build" -Wait -NoNewWindow
    Write-Host "✓ Production database seeded successfully" -ForegroundColor Green
} catch {
    Write-Host "✗ Error seeding production database: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`nStep 2: Seeding Test Database..." -ForegroundColor Cyan
try {
    $env:ASPNETCORE_ENVIRONMENT = "Test"
    Write-Host "Starting test application to trigger seeding..." -ForegroundColor White
    Start-Process -FilePath "dotnet" -ArgumentList "run", "--project", "SmartTelehealth.API", "--no-build" -Wait -NoNewWindow
    Write-Host "✓ Test database seeded successfully" -ForegroundColor Green
} catch {
    Write-Host "✗ Error seeding test database: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n=== Database Seeding Complete ===" -ForegroundColor Green
Write-Host "Both databases have been seeded with initial data:" -ForegroundColor White
Write-Host "- User roles (Client, Provider, Admin)" -ForegroundColor White
Write-Host "- Master data (Billing cycles, Currencies, etc.)" -ForegroundColor White
Write-Host "- Default admin user (admin@test.com / Admin123!)" -ForegroundColor White
Write-Host "- Test categories and consultation modes" -ForegroundColor White
Write-Host "`nYou can now run the application or tests." -ForegroundColor Yellow

# Reset environment variable
$env:ASPNETCORE_ENVIRONMENT = ""
