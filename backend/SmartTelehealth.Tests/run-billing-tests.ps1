# PowerShell script to run comprehensive billing system tests
# This script executes all billing tests and generates detailed reports

param(
    [string]$TestCategory = "All",
    [string]$OutputFormat = "trx",
    [string]$LogLevel = "Normal"
)

Write-Host "===========================================" -ForegroundColor Cyan
Write-Host "SmartTelehealth Billing System Test Suite" -ForegroundColor Cyan
Write-Host "===========================================" -ForegroundColor Cyan
Write-Host ""

# Set test parameters
$TestProject = "SmartTelehealth.Tests"
$TestResultsDir = "TestResults"
$CoverageDir = "Coverage"

# Create output directories
if (Test-Path $TestResultsDir) {
    Remove-Item $TestResultsDir -Recurse -Force
}
New-Item -ItemType Directory -Path $TestResultsDir -Force | Out-Null

if (Test-Path $CoverageDir) {
    Remove-Item $CoverageDir -Recurse -Force
}
New-Item -ItemType Directory -Path $CoverageDir -Force | Out-Null

Write-Host "Test Configuration:" -ForegroundColor Yellow
Write-Host "  Category: $TestCategory" -ForegroundColor White
Write-Host "  Output Format: $OutputFormat" -ForegroundColor White
Write-Host "  Log Level: $LogLevel" -ForegroundColor White
Write-Host ""

# Build the solution first
Write-Host "Building solution..." -ForegroundColor Green
dotnet build --configuration Release --verbosity minimal
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "Build successful!" -ForegroundColor Green
Write-Host ""

# Run tests based on category
switch ($TestCategory) {
    "Unit" {
        Write-Host "Running Unit Tests..." -ForegroundColor Green
        $TestFilter = "Category=Unit"
        $TestResultsFile = "$TestResultsDir\UnitTests.$OutputFormat"
    }
    "Integration" {
        Write-Host "Running Integration Tests..." -ForegroundColor Green
        $TestFilter = "Category=Integration"
        $TestResultsFile = "$TestResultsDir\IntegrationTests.$OutputFormat"
    }
    "Billing" {
        Write-Host "Running Billing System Tests..." -ForegroundColor Green
        $TestFilter = "Category=Billing"
        $TestResultsFile = "$TestResultsDir\BillingTests.$OutputFormat"
    }
    "All" {
        Write-Host "Running All Tests..." -ForegroundColor Green
        $TestFilter = ""
        $TestResultsFile = "$TestResultsDir\AllTests.$OutputFormat"
    }
    default {
        Write-Host "Invalid test category: $TestCategory" -ForegroundColor Red
        Write-Host "Valid categories: Unit, Integration, Billing, All" -ForegroundColor Yellow
        exit 1
    }
}

# Run the tests
$TestCommand = "dotnet test $TestProject --configuration Release --logger `"trx;LogFileName=$TestResultsFile`" --collect:`"XPlat Code Coverage`" --results-directory $TestResultsDir --verbosity $LogLevel"

if ($TestFilter -ne "") {
    $TestCommand += " --filter `"$TestFilter`""
}

Write-Host "Executing: $TestCommand" -ForegroundColor Cyan
Write-Host ""

Invoke-Expression $TestCommand

if ($LASTEXITCODE -ne 0) {
    Write-Host "Tests failed!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Tests completed successfully!" -ForegroundColor Green

# Generate coverage report
Write-Host ""
Write-Host "Generating coverage report..." -ForegroundColor Green

$CoverageFiles = Get-ChildItem -Path $TestResultsDir -Recurse -Filter "coverage.cobertura.xml"
if ($CoverageFiles.Count -gt 0) {
    # Install reportgenerator if not already installed
    $ReportGeneratorPath = "reportgenerator"
    try {
        & $ReportGeneratorPath --version | Out-Null
    }
    catch {
        Write-Host "Installing ReportGenerator..." -ForegroundColor Yellow
        dotnet tool install -g dotnet-reportgenerator-globaltool
    }

    # Generate HTML coverage report
    $CoverageReportDir = "$CoverageDir\HtmlReport"
    & $ReportGeneratorPath -reports:"$($CoverageFiles[0].FullName)" -targetdir:"$CoverageReportDir" -reporttypes:"Html"
    
    Write-Host "Coverage report generated at: $CoverageReportDir\index.html" -ForegroundColor Green
}

# Display test summary
Write-Host ""
Write-Host "===========================================" -ForegroundColor Cyan
Write-Host "Test Execution Summary" -ForegroundColor Cyan
Write-Host "===========================================" -ForegroundColor Cyan

$TestResultFiles = Get-ChildItem -Path $TestResultsDir -Filter "*.trx"
foreach ($file in $TestResultFiles) {
    Write-Host "Test Results: $($file.FullName)" -ForegroundColor White
}

if (Test-Path "$CoverageDir\HtmlReport\index.html") {
    Write-Host "Coverage Report: $CoverageDir\HtmlReport\index.html" -ForegroundColor White
}

Write-Host ""
Write-Host "Test execution completed!" -ForegroundColor Green
Write-Host ""

# Optional: Open coverage report in browser
$OpenReport = Read-Host "Would you like to open the coverage report in your browser? (y/n)"
if ($OpenReport -eq "y" -or $OpenReport -eq "Y") {
    if (Test-Path "$CoverageDir\HtmlReport\index.html") {
        Start-Process "$CoverageDir\HtmlReport\index.html"
    }
}
