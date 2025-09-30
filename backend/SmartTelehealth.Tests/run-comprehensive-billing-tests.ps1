# PowerShell script to run comprehensive billing system tests

param(
    [string]$TestCategory = "All",
    [switch]$Performance,
    [switch]$Integration,
    [switch]$Unit,
    [switch]$Verbose,
    [string]$OutputFormat = "console"
)

# Navigate to the test project directory
Push-Location (Join-Path $PSScriptRoot "SmartTelehealth.Tests")

Write-Host "=== SmartTelehealth Billing System Test Runner ===" -ForegroundColor Green
Write-Host "Test Category: $TestCategory" -ForegroundColor Yellow
Write-Host "Performance Tests: $Performance" -ForegroundColor Yellow
Write-Host "Integration Tests: $Integration" -ForegroundColor Yellow
Write-Host "Unit Tests: $Unit" -ForegroundColor Yellow
Write-Host "Verbose Output: $Verbose" -ForegroundColor Yellow
Write-Host ""

# Restore NuGet packages
Write-Host "Restoring NuGet packages..." -ForegroundColor Cyan
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to restore packages" -ForegroundColor Red
    Pop-Location
    exit 1
}

# Build test project
Write-Host "Building test project..." -ForegroundColor Cyan
dotnet build --configuration Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to build test project" -ForegroundColor Red
    Pop-Location
    exit 1
}

# Determine test filter based on parameters
$testFilter = ""
$loggerArgs = ""

if ($Performance) {
    $testFilter = "FullyQualifiedName~PerformanceBillingTests"
    Write-Host "Running Performance Tests..." -ForegroundColor Magenta
}
elseif ($Integration) {
    $testFilter = "FullyQualifiedName~Integration"
    Write-Host "Running Integration Tests..." -ForegroundColor Magenta
}
elseif ($Unit) {
    $testFilter = "FullyQualifiedName~Unit"
    Write-Host "Running Unit Tests..." -ForegroundColor Magenta
}
else {
    $testFilter = "FullyQualifiedName~Billing"
    Write-Host "Running All Billing Tests..." -ForegroundColor Magenta
}

# Set up logging
if ($Verbose) {
    $loggerArgs = "--logger `"console;verbosity=detailed`""
} else {
    $loggerArgs = "--logger `"console;verbosity=normal`""
}

# Set up output format
switch ($OutputFormat.ToLower()) {
    "trx" { $loggerArgs += " --logger `"trx;LogFileName=TestResults.trx`"" }
    "html" { $loggerArgs += " --logger `"html;LogFileName=TestResults.html`"" }
    "json" { $loggerArgs += " --logger `"json;LogFileName=TestResults.json`"" }
}

# Run tests
Write-Host "Executing tests with filter: $testFilter" -ForegroundColor Cyan
Write-Host ""

$testCommand = "dotnet test --filter `"$testFilter`" $loggerArgs --configuration Release"

if ($Verbose) {
    Write-Host "Command: $testCommand" -ForegroundColor Gray
    Write-Host ""
}

Invoke-Expression $testCommand
$testExitCode = $LASTEXITCODE

Write-Host ""
Write-Host "=== Test Execution Summary ===" -ForegroundColor Green

if ($testExitCode -eq 0) {
    Write-Host "All tests passed successfully!" -ForegroundColor Green
} else {
    Write-Host "Some tests failed. Exit code: $testExitCode" -ForegroundColor Red
}

# Display test results files if they exist
$resultFiles = @("TestResults.trx", "TestResults.html", "TestResults.json")
foreach ($file in $resultFiles) {
    if (Test-Path $file) {
        Write-Host "Test results saved to: $file" -ForegroundColor Yellow
    }
}

# Return to the original directory
Pop-Location

Write-Host ""
Write-Host "=== Test Runner Complete ===" -ForegroundColor Green

exit $testExitCode
