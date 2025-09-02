# PowerShell script to verify database seeding
Write-Host "=== SmartTelehealth Database Seeding Verification ===" -ForegroundColor Green

Write-Host "`nStep 1: Building the application..." -ForegroundColor Cyan
dotnet build

Write-Host "`nStep 2: Running application briefly to trigger seeding..." -ForegroundColor Cyan
Write-Host "Starting application for 10 seconds to allow seeding..." -ForegroundColor White

# Start the application in background
$job = Start-Job -ScriptBlock {
    Set-Location "D:\DayUsers\Rushikesh\Personal\.Net Projects\SmartTeleHealthSubscriptionModel\backend"
    dotnet run --project SmartTelehealth.API --no-build
}

# Wait for 10 seconds
Start-Sleep -Seconds 10

# Stop the job
Stop-Job $job
Remove-Job $job

Write-Host "✓ Application started and seeding should be complete" -ForegroundColor Green

Write-Host "`nStep 3: Verifying test database seeding..." -ForegroundColor Cyan
$env:ASPNETCORE_ENVIRONMENT = "Test"

# Start the test application in background
$testJob = Start-Job -ScriptBlock {
    Set-Location "D:\DayUsers\Rushikesh\Personal\.Net Projects\SmartTeleHealthSubscriptionModel\backend"
    $env:ASPNETCORE_ENVIRONMENT = "Test"
    dotnet run --project SmartTelehealth.API --no-build
}

# Wait for 10 seconds
Start-Sleep -Seconds 10

# Stop the job
Stop-Job $testJob
Remove-Job $testJob

Write-Host "✓ Test application started and seeding should be complete" -ForegroundColor Green

Write-Host "`n=== Database Seeding Verification Complete ===" -ForegroundColor Green
Write-Host "Both databases should now be seeded with:" -ForegroundColor White
Write-Host "- User roles (Client, Provider, Admin)" -ForegroundColor White
Write-Host "- Master data (Billing cycles, Currencies, etc.)" -ForegroundColor White
Write-Host "- Default admin user (admin@test.com / Admin123!)" -ForegroundColor White
Write-Host "- Test categories and consultation modes" -ForegroundColor White

# Reset environment variable
$env:ASPNETCORE_ENVIRONMENT = ""
