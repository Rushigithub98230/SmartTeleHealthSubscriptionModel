# PowerShell script to reset both production and test databases
param(
    [switch]$Force
)

Write-Host "=== SmartTelehealth Database Reset Script ===" -ForegroundColor Green
Write-Host "This script will delete and recreate both production and test databases" -ForegroundColor Yellow

if (-not $Force) {
    $confirmation = Read-Host "Are you sure you want to proceed? This will delete all data! (y/N)"
    if ($confirmation -ne 'y' -and $confirmation -ne 'Y') {
        Write-Host "Operation cancelled." -ForegroundColor Red
        exit 0
    }
}

Write-Host "`nStep 1: Dropping Production Database..." -ForegroundColor Cyan
try {
    dotnet ef database drop --force --project SmartTelehealth.Infrastructure --startup-project SmartTelehealth.API
    Write-Host "✓ Production database dropped successfully" -ForegroundColor Green
} catch {
    Write-Host "✗ Error dropping production database: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`nStep 2: Dropping Test Database..." -ForegroundColor Cyan
try {
    $env:ASPNETCORE_ENVIRONMENT = "Test"
    dotnet ef database drop --force --project SmartTelehealth.Infrastructure --startup-project SmartTelehealth.API
    Write-Host "✓ Test database dropped successfully" -ForegroundColor Green
} catch {
    Write-Host "✗ Error dropping test database: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`nStep 3: Creating Production Database with Migrations..." -ForegroundColor Cyan
try {
    $env:ASPNETCORE_ENVIRONMENT = ""
    dotnet ef database update --project SmartTelehealth.Infrastructure --startup-project SmartTelehealth.API
    Write-Host "✓ Production database created and migrated successfully" -ForegroundColor Green
} catch {
    Write-Host "✗ Error creating production database: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`nStep 4: Creating Test Database with Migrations..." -ForegroundColor Cyan
try {
    $env:ASPNETCORE_ENVIRONMENT = "Test"
    dotnet ef database update --project SmartTelehealth.Infrastructure --startup-project SmartTelehealth.API
    Write-Host "✓ Test database created and migrated successfully" -ForegroundColor Green
} catch {
    Write-Host "✗ Error creating test database: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`nStep 5: Seeding Production Database..." -ForegroundColor Cyan
try {
    $env:ASPNETCORE_ENVIRONMENT = ""
    dotnet run --project SmartTelehealth.API --no-build -- --seed-database
    Write-Host "✓ Production database seeded successfully" -ForegroundColor Green
} catch {
    Write-Host "✗ Error seeding production database: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Note: Database will be seeded automatically on first run" -ForegroundColor Yellow
}

Write-Host "`nStep 6: Seeding Test Database..." -ForegroundColor Cyan
try {
    $env:ASPNETCORE_ENVIRONMENT = "Test"
    dotnet run --project SmartTelehealth.API --no-build -- --seed-database
    Write-Host "✓ Test database seeded successfully" -ForegroundColor Green
} catch {
    Write-Host "✗ Error seeding test database: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Note: Database will be seeded automatically on first run" -ForegroundColor Yellow
}

Write-Host "`n=== Database Reset Complete ===" -ForegroundColor Green
Write-Host "Production Database: SmartTelehealthDb" -ForegroundColor White
Write-Host "Test Database: SmartTeleHealthTestDB" -ForegroundColor White
Write-Host "`nYou can now run the application or tests." -ForegroundColor Yellow

# Reset environment variable
$env:ASPNETCORE_ENVIRONMENT = ""
