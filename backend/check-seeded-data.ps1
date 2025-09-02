# PowerShell script to check if seeded data exists in the databases
Write-Host "=== SmartTelehealth Database Seeded Data Verification ===" -ForegroundColor Green

Write-Host "`nStep 1: Checking Production Database Seeded Data..." -ForegroundColor Cyan
Write-Host "Production Database: SmartTelehealthDb" -ForegroundColor White
Write-Host "Server: SDN-153\SQLEXPRESS2022" -ForegroundColor White

try {
    # Check production database
    $env:ASPNETCORE_ENVIRONMENT = ""
    
    # Create a simple SQL query to check seeded data
    $sqlQuery = @"
SELECT 
    'UserRoles' as TableName, COUNT(*) as RecordCount FROM UserRoles
UNION ALL
SELECT 'MasterBillingCycles', COUNT(*) FROM MasterBillingCycles
UNION ALL
SELECT 'MasterCurrencies', COUNT(*) FROM MasterCurrencies
UNION ALL
SELECT 'MasterPrivilegeTypes', COUNT(*) FROM MasterPrivilegeTypes
UNION ALL
SELECT 'AppointmentStatuses', COUNT(*) FROM AppointmentStatuses
UNION ALL
SELECT 'PaymentStatuses', COUNT(*) FROM PaymentStatuses
UNION ALL
SELECT 'ConsultationModes', COUNT(*) FROM ConsultationModes
UNION ALL
SELECT 'Categories', COUNT(*) FROM Categories
UNION ALL
SELECT 'Users', COUNT(*) FROM Users
UNION ALL
SELECT 'AspNetRoles', COUNT(*) FROM AspNetRoles
"@

    Write-Host "Checking production database tables..." -ForegroundColor Yellow
    
    # Use sqlcmd to execute the query
    $prodResult = sqlcmd -S "SDN-153\SQLEXPRESS2022" -d "SmartTelehealthDb" -Q $sqlQuery -h -1 -W
    
    if ($prodResult) {
        Write-Host "✓ Production Database Seeded Data Found:" -ForegroundColor Green
        $prodResult | ForEach-Object {
            if ($_ -match "(\w+)\s+(\d+)") {
                $tableName = $matches[1]
                $count = $matches[2]
                if ([int]$count -gt 0) {
                    Write-Host "  - $tableName`: $count records" -ForegroundColor White
                } else {
                    Write-Host "  - $tableName`: $count records (EMPTY)" -ForegroundColor Red
                }
            }
        }
    } else {
        Write-Host "✗ Could not connect to production database or no data found" -ForegroundColor Red
    }
} catch {
    Write-Host "✗ Error checking production database: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`nStep 2: Checking Test Database Seeded Data..." -ForegroundColor Cyan
Write-Host "Test Database: SmartTeleHealthTestDB" -ForegroundColor White
Write-Host "Server: localhost\SQLEXPRESS" -ForegroundColor White

try {
    # Check test database
    $env:ASPNETCORE_ENVIRONMENT = "Test"
    
    Write-Host "Checking test database tables..." -ForegroundColor Yellow
    
    # Use sqlcmd to execute the query on test database
    $testResult = sqlcmd -S "localhost\SQLEXPRESS" -d "SmartTeleHealthTestDB" -Q $sqlQuery -h -1 -W
    
    if ($testResult) {
        Write-Host "✓ Test Database Seeded Data Found:" -ForegroundColor Green
        $testResult | ForEach-Object {
            if ($_ -match "(\w+)\s+(\d+)") {
                $tableName = $matches[1]
                $count = $matches[2]
                if ([int]$count -gt 0) {
                    Write-Host "  - $tableName`: $count records" -ForegroundColor White
                } else {
                    Write-Host "  - $tableName`: $count records (EMPTY)" -ForegroundColor Red
                }
            }
        }
    } else {
        Write-Host "✗ Could not connect to test database or no data found" -ForegroundColor Red
    }
} catch {
    Write-Host "✗ Error checking test database: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`nStep 3: Checking Default Admin User..." -ForegroundColor Cyan

try {
    # Check for admin user in production
    $adminQuery = "SELECT Email, UserName, UserType FROM Users WHERE Email = 'admin@test.com' OR Email = 'system@smarttelehealth.com'"
    
    Write-Host "Checking admin users in production database..." -ForegroundColor Yellow
    $adminProdResult = sqlcmd -S "SDN-153\SQLEXPRESS2022" -d "SmartTelehealthDb" -Q $adminQuery -h -1 -W
    
    if ($adminProdResult) {
        Write-Host "✓ Production Admin Users Found:" -ForegroundColor Green
        $adminProdResult | ForEach-Object {
            if ($_ -match "(\S+)\s+(\S+)\s+(\S+)") {
                Write-Host "  - Email: $($matches[1]), Username: $($matches[2]), Type: $($matches[3])" -ForegroundColor White
            }
        }
    }
    
    # Check for admin user in test
    Write-Host "Checking admin users in test database..." -ForegroundColor Yellow
    $adminTestResult = sqlcmd -S "localhost\SQLEXPRESS" -d "SmartTeleHealthTestDB" -Q $adminQuery -h -1 -W
    
    if ($adminTestResult) {
        Write-Host "✓ Test Admin Users Found:" -ForegroundColor Green
        $adminTestResult | ForEach-Object {
            if ($_ -match "(\S+)\s+(\S+)\s+(\S+)") {
                Write-Host "  - Email: $($matches[1]), Username: $($matches[2]), Type: $($matches[3])" -ForegroundColor White
            }
        }
    }
} catch {
    Write-Host "✗ Error checking admin users: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n=== Database Seeded Data Verification Complete ===" -ForegroundColor Green
Write-Host "If you see records in the tables above, the databases are properly seeded." -ForegroundColor White
Write-Host "If any tables show 0 records, the seeding may not have completed successfully." -ForegroundColor Yellow

# Reset environment variable
$env:ASPNETCORE_ENVIRONMENT = ""
