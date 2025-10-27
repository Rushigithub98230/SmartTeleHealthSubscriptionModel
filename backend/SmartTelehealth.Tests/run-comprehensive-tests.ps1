# Comprehensive Test Runner for SmartTelehealth Backend
# This script runs all integration tests using real services with mocked third-party services

param(
    [string]$TestType = "All",  # All, Billing, Subscription
    [switch]$Verbose = $false,
    [switch]$Clean = $false
)

Write-Host "🚀 Starting SmartTelehealth Comprehensive Test Suite" -ForegroundColor Green
Write-Host "=================================================" -ForegroundColor Green

# Set error action preference
$ErrorActionPreference = "Stop"

# Get the script directory
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectDir = Split-Path -Parent $ScriptDir
$TestProjectDir = Join-Path $ProjectDir "SmartTelehealth.Tests"

# Change to test project directory
Set-Location $TestProjectDir

try {
    # Clean test database if requested
    if ($Clean) {
        Write-Host "🧹 Cleaning test database..." -ForegroundColor Yellow
        # Add database cleanup logic here if needed
    }

    # Build the test project
    Write-Host "🔨 Building test project..." -ForegroundColor Yellow
    dotnet build --configuration Release --verbosity minimal
    
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed"
    }

    # Run tests based on type
    switch ($TestType.ToLower()) {
        "billing" {
            Write-Host "💰 Running Billing Service Tests..." -ForegroundColor Cyan
            dotnet test --filter "FullyQualifiedName~BillingServiceTests" --configuration Release --verbosity normal --logger "console;verbosity=detailed"
        }
        "subscription" {
            Write-Host "📋 Running Subscription Service Tests..." -ForegroundColor Cyan
            dotnet test --filter "FullyQualifiedName~SubscriptionServiceTests" --configuration Release --verbosity normal --logger "console;verbosity=detailed"
        }
        "all" {
            Write-Host "🎯 Running All Service Tests..." -ForegroundColor Cyan
            dotnet test --configuration Release --verbosity normal --logger "console;verbosity=detailed"
        }
        default {
            Write-Host "❌ Invalid test type: $TestType" -ForegroundColor Red
            Write-Host "Valid options: All, Billing, Subscription" -ForegroundColor Yellow
            exit 1
        }
    }

    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ All tests passed successfully!" -ForegroundColor Green
    } else {
        Write-Host "❌ Some tests failed!" -ForegroundColor Red
        exit $LASTEXITCODE
    }

} catch {
    Write-Host "❌ Error running tests: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
} finally {
    # Return to original directory
    Set-Location $ScriptDir
}

Write-Host "🏁 Test execution completed!" -ForegroundColor Green

