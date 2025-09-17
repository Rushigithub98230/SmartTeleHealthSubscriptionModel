# PowerShell script to execute the SQL script for adding missing columns
# This script will run the AddMissingColumns.sql script against the database

param(
    [string]$ConnectionString = "Server=(localdb)\\mssqllocaldb;Database=SmartTelehealthDb;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true;"
)

Write-Host "Executing SQL script to add missing columns..." -ForegroundColor Green

try {
    # Load SQL Server module
    Import-Module SqlServer -ErrorAction SilentlyContinue
    
    if (-not (Get-Module SqlServer)) {
        Write-Host "SQL Server module not found. Installing..." -ForegroundColor Yellow
        Install-Module -Name SqlServer -Force -AllowClobber
        Import-Module SqlServer
    }
    
    # Execute the SQL script
    $sqlScript = Get-Content -Path "AddMissingColumns.sql" -Raw
    Invoke-Sqlcmd -ConnectionString $ConnectionString -Query $sqlScript
    
    Write-Host "SQL script executed successfully!" -ForegroundColor Green
    Write-Host "Missing columns have been added to the database." -ForegroundColor Green
}
catch {
    Write-Host "Error executing SQL script: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Please run the AddMissingColumns.sql script manually in SQL Server Management Studio." -ForegroundColor Yellow
    Write-Host "Connection String: $ConnectionString" -ForegroundColor Cyan
}
