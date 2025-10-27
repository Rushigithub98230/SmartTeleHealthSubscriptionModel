# PowerShell script to run comprehensive integration tests
# This script builds the test project and runs all integration tests with proper setup

param(
    [string]$TestType = "All",
    [switch]$Verbose = $false,
    [switch]$Clean = $false,
    [switch]$Build = $true
)

# Define paths
$testProjectDir = "backend/SmartTelehealth.Tests"
$testProjectFile = "$testProjectDir/SmartTelehealth.Tests.csproj"
$solutionFile = "backend/SmartTelehealth.sln"

Write-Host "=== SmartTelehealth Integration Test Runner ===" -ForegroundColor Green
Write-Host "Test Type: $TestType" -ForegroundColor Yellow
Write-Host "Verbose: $Verbose" -ForegroundColor Yellow
Write-Host "Clean: $Clean" -ForegroundColor Yellow
Write-Host "Build: $Build" -ForegroundColor Yellow
Write-Host ""

# Step 1: Clean if requested
if ($Clean) {
    Write-Host "Cleaning test project..." -ForegroundColor Blue
    dotnet clean $testProjectFile
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to clean test project"
        exit 1
    }
    Write-Host "Clean completed successfully" -ForegroundColor Green
    Write-Host ""
}

# Step 2: Build if requested
if ($Build) {
    Write-Host "Building solution..." -ForegroundColor Blue
    dotnet build $solutionFile --configuration Release
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Solution build failed"
        exit 1
    }
    Write-Host "Build completed successfully" -ForegroundColor Green
    Write-Host ""
}

# Step 3: Run tests
Write-Host "Running integration tests..." -ForegroundColor Blue

# Build test command
$testCommand = "dotnet test $testProjectFile --configuration Release --no-build"

# Add verbosity if requested
if ($Verbose) {
    $testCommand += " --verbosity normal"
} else {
    $testCommand += " --verbosity minimal"
}

# Add test filtering if specific type requested
if ($TestType -ne "All") {
    $testCommand += " --filter TestType=$TestType"
}

# Add additional test options
$testCommand += " --collect:\"XPlat Code Coverage\""
$testCommand += " --results-directory ./TestResults"

Write-Host "Executing: $testCommand" -ForegroundColor Cyan
Write-Host ""

# Execute test command
Invoke-Expression $testCommand
$testExitCode = $LASTEXITCODE

Write-Host ""

# Step 4: Report results
if ($testExitCode -eq 0) {
    Write-Host "=== ALL TESTS PASSED ===" -ForegroundColor Green
    Write-Host "Integration tests completed successfully!" -ForegroundColor Green
    
    # Display test results summary
    $testResultsDir = "./TestResults"
    if (Test-Path $testResultsDir) {
        $resultFiles = Get-ChildItem $testResultsDir -Filter "*.trx" | Sort-Object LastWriteTime -Descending
        if ($resultFiles.Count -gt 0) {
            Write-Host ""
            Write-Host "Test Results:" -ForegroundColor Yellow
            Write-Host "  Latest Results: $($resultFiles[0].Name)" -ForegroundColor White
            Write-Host "  Location: $($resultFiles[0].FullName)" -ForegroundColor White
        }
    }
} else {
    Write-Host "=== TESTS FAILED ===" -ForegroundColor Red
    Write-Host "Some integration tests failed. Check the output above for details." -ForegroundColor Red
    Write-Host ""
    Write-Host "Common issues and solutions:" -ForegroundColor Yellow
    Write-Host "  1. Database connection issues:" -ForegroundColor White
    Write-Host "     - Ensure SQL Server LocalDB is installed and running" -ForegroundColor Gray
    Write-Host "     - Check connection string in test configuration" -ForegroundColor Gray
    Write-Host "  2. Missing dependencies:" -ForegroundColor White
    Write-Host "     - Run 'dotnet restore' to restore packages" -ForegroundColor Gray
    Write-Host "  3. Stripe service issues:" -ForegroundColor White
    Write-Host "     - Stripe service is mocked, but check for configuration issues" -ForegroundColor Gray
    Write-Host "  4. Migration issues:" -ForegroundColor White
    Write-Host "     - Ensure all migrations are applied to test database" -ForegroundColor Gray
}

Write-Host ""
Write-Host "=== Test Run Complete ===" -ForegroundColor Green

exit $testExitCode
