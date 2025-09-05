# PowerShell script to run the DataUpdater console application
# This will update existing SubscriptionPlans with valid CategoryId values

Write-Host "Starting SubscriptionPlans CategoryId update process..." -ForegroundColor Green
Write-Host "This script will update existing SubscriptionPlans with valid CategoryId values." -ForegroundColor Yellow
Write-Host ""

try {
    # Build the DataUpdater project
    Write-Host "Building the DataUpdater project..." -ForegroundColor Cyan
    dotnet build SmartTelehealth.DataUpdater/SmartTelehealth.DataUpdater.csproj
    
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed. Please fix build errors before running the update script."
    }
    
    Write-Host "Build successful!" -ForegroundColor Green
    Write-Host ""
    
    # Run the DataUpdater
    Write-Host "Running SubscriptionPlans update..." -ForegroundColor Cyan
    dotnet run --project SmartTelehealth.DataUpdater/SmartTelehealth.DataUpdater.csproj
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "✅ SubscriptionPlans update completed successfully!" -ForegroundColor Green
        Write-Host "You can now apply the migration to add the CategoryId foreign key." -ForegroundColor Yellow
    } else {
        throw "Update script failed with exit code: $LASTEXITCODE"
    }
}
catch {
    Write-Host ""
    Write-Host "❌ Error updating SubscriptionPlans: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Please check the error details above and fix any issues." -ForegroundColor Yellow
    exit 1
}


