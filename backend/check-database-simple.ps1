# Simple PowerShell script to check database contents
Write-Host "=== SmartTelehealth Database Content Check ===" -ForegroundColor Green

Write-Host "`nStep 1: Checking if databases exist..." -ForegroundColor Cyan

# Check if production database exists
Write-Host "Checking Production Database (SmartTelehealthDb)..." -ForegroundColor Yellow
try {
    $prodCheck = sqlcmd -S "SDN-153\SQLEXPRESS2022" -Q "SELECT name FROM sys.databases WHERE name = 'SmartTelehealthDb'" -h -1
    if ($prodCheck -match "SmartTelehealthDb") {
        Write-Host "✓ Production database exists" -ForegroundColor Green
        
        # Check table count
        $tableCount = sqlcmd -S "SDN-153\SQLEXPRESS2022" -d "SmartTelehealthDb" -Q "SELECT COUNT(*) as TableCount FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'" -h -1
        Write-Host "  - Tables in database: $tableCount" -ForegroundColor White
        
        # Check UserRoles count
        $userRolesCount = sqlcmd -S "SDN-153\SQLEXPRESS2022" -d "SmartTelehealthDb" -Q "SELECT COUNT(*) as UserRolesCount FROM UserRoles" -h -1
        Write-Host "  - UserRoles records: $userRolesCount" -ForegroundColor White
        
        # Check Users count
        $usersCount = sqlcmd -S "SDN-153\SQLEXPRESS2022" -d "SmartTelehealthDb" -Q "SELECT COUNT(*) as UsersCount FROM Users" -h -1
        Write-Host "  - Users records: $usersCount" -ForegroundColor White
        
        # Check Categories count
        $categoriesCount = sqlcmd -S "SDN-153\SQLEXPRESS2022" -d "SmartTelehealthDb" -Q "SELECT COUNT(*) as CategoriesCount FROM Categories" -h -1
        Write-Host "  - Categories records: $categoriesCount" -ForegroundColor White
    } else {
        Write-Host "✗ Production database does not exist" -ForegroundColor Red
    }
} catch {
    Write-Host "✗ Error checking production database: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`nStep 2: Checking Test Database..." -ForegroundColor Cyan
Write-Host "Checking Test Database (SmartTeleHealthTestDB)..." -ForegroundColor Yellow
try {
    $testCheck = sqlcmd -S "localhost\SQLEXPRESS" -Q "SELECT name FROM sys.databases WHERE name = 'SmartTeleHealthTestDB'" -h -1
    if ($testCheck -match "SmartTeleHealthTestDB") {
        Write-Host "✓ Test database exists" -ForegroundColor Green
        
        # Check table count
        $tableCount = sqlcmd -S "localhost\SQLEXPRESS" -d "SmartTeleHealthTestDB" -Q "SELECT COUNT(*) as TableCount FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'" -h -1
        Write-Host "  - Tables in database: $tableCount" -ForegroundColor White
        
        # Check UserRoles count
        $userRolesCount = sqlcmd -S "localhost\SQLEXPRESS" -d "SmartTeleHealthTestDB" -Q "SELECT COUNT(*) as UserRolesCount FROM UserRoles" -h -1
        Write-Host "  - UserRoles records: $userRolesCount" -ForegroundColor White
        
        # Check Users count
        $usersCount = sqlcmd -S "localhost\SQLEXPRESS" -d "SmartTeleHealthTestDB" -Q "SELECT COUNT(*) as UsersCount FROM Users" -h -1
        Write-Host "  - Users records: $usersCount" -ForegroundColor White
        
        # Check Categories count
        $categoriesCount = sqlcmd -S "localhost\SQLEXPRESS" -d "SmartTeleHealthTestDB" -Q "SELECT COUNT(*) as CategoriesCount FROM Categories" -h -1
        Write-Host "  - Categories records: $categoriesCount" -ForegroundColor White
    } else {
        Write-Host "✗ Test database does not exist" -ForegroundColor Red
    }
} catch {
    Write-Host "✗ Error checking test database: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`nStep 3: Running Application to Trigger Seeding..." -ForegroundColor Cyan
Write-Host "Starting application briefly to ensure seeding..." -ForegroundColor Yellow

try {
    # Start application in background for 15 seconds
    $job = Start-Job -ScriptBlock {
        Set-Location "D:\DayUsers\Rushikesh\Personal\.Net Projects\SmartTeleHealthSubscriptionModel\backend"
        dotnet run --project SmartTelehealth.API --no-build
    }
    
    # Wait for 15 seconds
    Start-Sleep -Seconds 15
    
    # Stop the job
    Stop-Job $job
    Remove-Job $job
    
    Write-Host "✓ Application started and seeding should be complete" -ForegroundColor Green
} catch {
    Write-Host "✗ Error starting application: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n=== Database Check Complete ===" -ForegroundColor Green
Write-Host "If you see record counts > 0 above, the databases are seeded." -ForegroundColor White
Write-Host "If counts are 0, the seeding may need to be run again." -ForegroundColor Yellow
