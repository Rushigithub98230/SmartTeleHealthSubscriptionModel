# PowerShell script to update existing SubscriptionPlans with valid CategoryId values
# This script should be run before applying the migration that adds the CategoryId foreign key

Write-Host "Starting SubscriptionPlans CategoryId update process..." -ForegroundColor Green
Write-Host "This script will update existing SubscriptionPlans with valid CategoryId values." -ForegroundColor Yellow
Write-Host ""

try {
    # Build the project first
    Write-Host "Building the project..." -ForegroundColor Cyan
    dotnet build
    
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed. Please fix build errors before running the update script."
    }
    
    Write-Host "Build successful!" -ForegroundColor Green
    Write-Host ""
    
    # Run the update script
    Write-Host "Running SubscriptionPlans update..." -ForegroundColor Cyan
    dotnet run --project SmartTelehealth.Infrastructure UpdateSubscriptionPlansProgram.cs
    
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


