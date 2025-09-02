# PowerShell script to complete database seeding with missing master data
Write-Host "=== SmartTelehealth Complete Database Seeding ===" -ForegroundColor Green
Write-Host "This script will add missing master data to the database" -ForegroundColor Yellow

Write-Host "`nStep 1: Adding Missing Master Data..." -ForegroundColor Cyan

# SQL script to add missing master data
$masterDataSQL = @"
-- Add Master Billing Cycles
IF NOT EXISTS (SELECT 1 FROM MasterBillingCycles)
BEGIN
    INSERT INTO MasterBillingCycles (Id, Name, Description, DurationInDays, SortOrder, CreatedDate, IsActive)
    VALUES 
        (NEWID(), 'Monthly', 'Monthly billing cycle', 30, 1, GETUTCDATE(), 1),
        (NEWID(), 'Quarterly', 'Quarterly billing cycle', 90, 2, GETUTCDATE(), 1),
        (NEWID(), 'Semi-Annual', 'Semi-annual billing cycle', 180, 3, GETUTCDATE(), 1),
        (NEWID(), 'Annual', 'Annual billing cycle', 365, 4, GETUTCDATE(), 1),
        (NEWID(), 'Weekly', 'Weekly billing cycle', 7, 5, GETUTCDATE(), 1)
END

-- Add Master Currencies
IF NOT EXISTS (SELECT 1 FROM MasterCurrencies)
BEGIN
    INSERT INTO MasterCurrencies (Id, Code, Name, Symbol, SortOrder, CreatedDate, IsActive)
    VALUES 
        (NEWID(), 'USD', 'US Dollar', '$', 1, GETUTCDATE(), 1),
        (NEWID(), 'EUR', 'Euro', '€', 2, GETUTCDATE(), 1),
        (NEWID(), 'GBP', 'British Pound', '£', 3, GETUTCDATE(), 1),
        (NEWID(), 'CAD', 'Canadian Dollar', 'C$', 4, GETUTCDATE(), 1),
        (NEWID(), 'AUD', 'Australian Dollar', 'A$', 5, GETUTCDATE(), 1),
        (NEWID(), 'INR', 'Indian Rupee', '₹', 6, GETUTCDATE(), 1)
END

-- Add Master Privilege Types
IF NOT EXISTS (SELECT 1 FROM MasterPrivilegeTypes)
BEGIN
    INSERT INTO MasterPrivilegeTypes (Id, Name, Description, SortOrder, CreatedDate, IsActive)
    VALUES 
        (NEWID(), 'Consultation', 'Consultation privileges', 1, GETUTCDATE(), 1),
        (NEWID(), 'Messaging', 'Messaging privileges', 2, GETUTCDATE(), 1),
        (NEWID(), 'Document', 'Document access privileges', 3, GETUTCDATE(), 1),
        (NEWID(), 'Video', 'Video call privileges', 4, GETUTCDATE(), 1),
        (NEWID(), 'Prescription', 'Prescription privileges', 5, GETUTCDATE(), 1),
        (NEWID(), 'Emergency', 'Emergency access privileges', 6, GETUTCDATE(), 1),
        (NEWID(), 'Family', 'Family member access privileges', 7, GETUTCDATE(), 1),
        (NEWID(), 'Analytics', 'Analytics and reporting privileges', 8, GETUTCDATE(), 1)
END

-- Add Categories (Healthcare Categories)
IF NOT EXISTS (SELECT 1 FROM Categories)
BEGIN
    INSERT INTO Categories (Id, Name, Description, BasePrice, ConsultationFee, OneTimeConsultationFee, IsActive, RequiresHealthAssessment, AllowsMedicationDelivery, AllowsFollowUpMessaging, CreatedDate)
    VALUES 
        (NEWID(), 'Primary Care', 'General health consultations and primary care services', 100.00, 100.00, 150.00, 1, 1, 1, 1, GETUTCDATE()),
        (NEWID(), 'Mental Health', 'Mental health and therapy services', 150.00, 150.00, 200.00, 1, 1, 1, 1, GETUTCDATE()),
        (NEWID(), 'Dermatology', 'Skin and dermatological consultations', 120.00, 120.00, 180.00, 1, 0, 1, 1, GETUTCDATE()),
        (NEWID(), 'Cardiology', 'Heart and cardiovascular consultations', 200.00, 200.00, 300.00, 1, 1, 1, 1, GETUTCDATE()),
        (NEWID(), 'Pediatrics', 'Child healthcare consultations', 130.00, 130.00, 190.00, 1, 1, 1, 1, GETUTCDATE()),
        (NEWID(), 'Gynecology', 'Women''s health consultations', 140.00, 140.00, 200.00, 1, 1, 1, 1, GETUTCDATE()),
        (NEWID(), 'Orthopedics', 'Bone and joint consultations', 180.00, 180.00, 250.00, 1, 1, 1, 1, GETUTCDATE()),
        (NEWID(), 'Neurology', 'Brain and nervous system consultations', 220.00, 220.00, 320.00, 1, 1, 1, 1, GETUTCDATE()),
        (NEWID(), 'Ophthalmology', 'Eye and vision consultations', 110.00, 110.00, 160.00, 1, 0, 1, 1, GETUTCDATE()),
        (NEWID(), 'Emergency', 'Emergency medical consultations', 300.00, 300.00, 400.00, 1, 1, 1, 1, GETUTCDATE())
END

PRINT 'Master data seeding completed successfully!'
"@

try {
    Write-Host "Executing master data seeding script..." -ForegroundColor Yellow
    $result = sqlcmd -S "SDN-153\SQLEXPRESS2022" -d "SmartTelehealthDb" -Q $masterDataSQL
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Master data seeding completed successfully" -ForegroundColor Green
    } else {
        Write-Host "✗ Error during master data seeding" -ForegroundColor Red
        Write-Host $result -ForegroundColor Red
    }
} catch {
    Write-Host "✗ Error executing master data seeding: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`nStep 2: Verifying Seeded Data..." -ForegroundColor Cyan

try {
    $verificationQuery = @"
SELECT 
    'MasterBillingCycles' as TableName, COUNT(*) as Count FROM MasterBillingCycles
UNION ALL
SELECT 'MasterCurrencies', COUNT(*) FROM MasterCurrencies
UNION ALL
SELECT 'MasterPrivilegeTypes', COUNT(*) FROM MasterPrivilegeTypes
UNION ALL
SELECT 'Categories', COUNT(*) FROM Categories
"@

    $verificationResult = sqlcmd -S "SDN-153\SQLEXPRESS2022" -d "SmartTelehealthDb" -Q $verificationQuery -h -1
    
    Write-Host "✓ Verification Results:" -ForegroundColor Green
    $verificationResult | ForEach-Object {
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
} catch {
    Write-Host "✗ Error verifying seeded data: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`nStep 3: Checking Complete Database Status..." -ForegroundColor Cyan

try {
    $completeStatusQuery = @"
SELECT 
    'UserRoles' as TableName, COUNT(*) as Count FROM UserRoles
UNION ALL
SELECT 'Users', COUNT(*) FROM Users
UNION ALL
SELECT 'AppointmentStatuses', COUNT(*) FROM AppointmentStatuses
UNION ALL
SELECT 'PaymentStatuses', COUNT(*) FROM PaymentStatuses
UNION ALL
SELECT 'ConsultationModes', COUNT(*) FROM ConsultationModes
UNION ALL
SELECT 'MasterBillingCycles', COUNT(*) FROM MasterBillingCycles
UNION ALL
SELECT 'MasterCurrencies', COUNT(*) FROM MasterCurrencies
UNION ALL
SELECT 'MasterPrivilegeTypes', COUNT(*) FROM MasterPrivilegeTypes
UNION ALL
SELECT 'Categories', COUNT(*) FROM Categories
"@

    $completeResult = sqlcmd -S "SDN-153\SQLEXPRESS2022" -d "SmartTelehealthDb" -Q $completeStatusQuery -h -1
    
    Write-Host "✓ Complete Database Status:" -ForegroundColor Green
    $completeResult | ForEach-Object {
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
} catch {
    Write-Host "✗ Error checking complete database status: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n=== Complete Seeding Process Finished ===" -ForegroundColor Green
Write-Host "The database should now have all required master data seeded." -ForegroundColor White
Write-Host "You can now run the application with full functionality." -ForegroundColor Yellow
