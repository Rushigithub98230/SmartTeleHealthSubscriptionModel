-- ═══════════════════════════════════════════════════════════════════════════════
-- COMPLETE SUBSCRIPTION MANAGEMENT MODULE - SEED DATA SCRIPT
-- Created: 2025-01-XX
-- Purpose: Insert all master data required for subscription management
-- Includes: Billing Cycles, Currencies, Privilege Types, Payment/Refund Statuses,
--           Privileges, Categories, and System Settings
-- ═══════════════════════════════════════════════════════════════════════════════

USE [SmartTelehealth]
GO

SET NOCOUNT ON;
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- SECTION 1: MASTER BILLING CYCLES
-- ═══════════════════════════════════════════════════════════════════════════════

IF NOT EXISTS (SELECT * FROM MasterBillingCycles)
BEGIN
    PRINT 'Seeding MasterBillingCycles...';
    
    INSERT INTO MasterBillingCycles (Id, Name, Description, DurationInDays, SortOrder, IsActive, IsDeleted, CreatedDate)
    VALUES 
        (NEWID(), 'Monthly', 'Monthly billing cycle (30 days)', 30, 1, 1, 0, GETUTCDATE()),
        (NEWID(), 'Quarterly', 'Quarterly billing cycle (90 days)', 90, 2, 1, 0, GETUTCDATE()),
        (NEWID(), 'Annual', 'Annual billing cycle (365 days)', 365, 3, 1, 0, GETUTCDATE());
    
    PRINT '✅ MasterBillingCycles seeded successfully';
END
ELSE
BEGIN
    PRINT '⏭️  MasterBillingCycles already contains data, skipping...';
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- SECTION 2: MASTER CURRENCIES
-- ═══════════════════════════════════════════════════════════════════════════════

IF NOT EXISTS (SELECT * FROM MasterCurrencies)
BEGIN
    PRINT 'Seeding MasterCurrencies...';
    
    INSERT INTO MasterCurrencies (Id, Code, Name, Symbol, SortOrder, IsActive, IsDeleted, CreatedDate)
    VALUES 
        (NEWID(), 'USD', 'US Dollar', '$', 1, 1, 0, GETUTCDATE()),
        (NEWID(), 'EUR', 'Euro', '€', 2, 1, 0, GETUTCDATE()),
        (NEWID(), 'GBP', 'British Pound', '£', 3, 1, 0, GETUTCDATE()),
        (NEWID(), 'INR', 'Indian Rupee', '₹', 4, 1, 0, GETUTCDATE());
    
    PRINT '✅ MasterCurrencies seeded successfully';
END
ELSE
BEGIN
    PRINT '⏭️  MasterCurrencies already contains data, skipping...';
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- SECTION 3: MASTER PRIVILEGE TYPES
-- ═══════════════════════════════════════════════════════════════════════════════

IF NOT EXISTS (SELECT * FROM MasterPrivilegeTypes)
BEGIN
    PRINT 'Seeding MasterPrivilegeTypes...';
    
    INSERT INTO MasterPrivilegeTypes (Id, Name, Description, SortOrder, IsActive, IsDeleted, CreatedDate)
    VALUES 
        (NEWID(), 'Consultation', 'Consultation privileges', 1, 1, 0, GETUTCDATE()),
        (NEWID(), 'Medication', 'Medication-related privileges', 2, 1, 0, GETUTCDATE()),
        (NEWID(), 'Messaging', 'Messaging privileges', 3, 1, 0, GETUTCDATE()),
        (NEWID(), 'Document', 'Document access privileges', 4, 1, 0, GETUTCDATE());
    
    PRINT '✅ MasterPrivilegeTypes seeded successfully';
END
ELSE
BEGIN
    PRINT '⏭️  MasterPrivilegeTypes already contains data, skipping...';
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- SECTION 4: PAYMENT STATUSES
-- ═══════════════════════════════════════════════════════════════════════════════

IF NOT EXISTS (SELECT * FROM PaymentStatuses)
BEGIN
    PRINT 'Seeding PaymentStatuses...';
    
    INSERT INTO PaymentStatuses (Id, Name, Description, SortOrder, Color, IsActive, IsDeleted, CreatedDate)
    VALUES 
        (NEWID(), 'Pending', 'Payment is pending', 1, '#FFA500', 1, 0, GETUTCDATE()),
        (NEWID(), 'Processing', 'Payment is being processed', 2, '#0000FF', 1, 0, GETUTCDATE()),
        (NEWID(), 'Completed', 'Payment completed successfully', 3, '#008000', 1, 0, GETUTCDATE()),
        (NEWID(), 'Failed', 'Payment failed', 4, '#FF0000', 1, 0, GETUTCDATE()),
        (NEWID(), 'Cancelled', 'Payment was cancelled', 5, '#808080', 1, 0, GETUTCDATE()),
        (NEWID(), 'Refunded', 'Payment was refunded', 6, '#FFA500', 1, 0, GETUTCDATE()),
        (NEWID(), 'PartiallyRefunded', 'Payment was partially refunded', 7, '#FFD700', 1, 0, GETUTCDATE());
    
    PRINT '✅ PaymentStatuses seeded successfully';
END
ELSE
BEGIN
    PRINT '⏭️  PaymentStatuses already contains data, skipping...';
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- SECTION 5: REFUND STATUSES
-- ═══════════════════════════════════════════════════════════════════════════════

IF NOT EXISTS (SELECT * FROM RefundStatuses)
BEGIN
    PRINT 'Seeding RefundStatuses...';
    
    INSERT INTO RefundStatuses (Id, Name, Description, SortOrder, Color, IsActive, IsDeleted, CreatedDate)
    VALUES 
        (NEWID(), 'None', 'No refund requested', 1, '#808080', 1, 0, GETUTCDATE()),
        (NEWID(), 'Requested', 'Refund has been requested', 2, '#FFA500', 1, 0, GETUTCDATE()),
        (NEWID(), 'Processing', 'Refund is being processed', 3, '#0000FF', 1, 0, GETUTCDATE()),
        (NEWID(), 'Completed', 'Refund completed successfully', 4, '#008000', 1, 0, GETUTCDATE()),
        (NEWID(), 'Failed', 'Refund failed', 5, '#FF0000', 1, 0, GETUTCDATE());
    
    PRINT '✅ RefundStatuses seeded successfully';
END
ELSE
BEGIN
    PRINT '⏭️  RefundStatuses already contains data, skipping...';
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- SECTION 6: CATEGORIES
-- ═══════════════════════════════════════════════════════════════════════════════

IF NOT EXISTS (SELECT * FROM Categories)
BEGIN
    PRINT 'Seeding Categories...';
    
    INSERT INTO Categories (Id, Name, Description, IsActive, IsDeleted, CreatedDate)
    VALUES 
        (NEWID(), 'Primary Care', 'General health consultations', 1, 0, GETUTCDATE()),
        (NEWID(), 'Mental Health', 'Mental health and therapy services', 1, 0, GETUTCDATE()),
        (NEWID(), 'Dermatology', 'Skin and dermatological consultations', 1, 0, GETUTCDATE()),
        (NEWID(), 'Cardiology', 'Heart and cardiovascular health', 1, 0, GETUTCDATE()),
        (NEWID(), 'Nutrition', 'Nutrition and dietary consultations', 1, 0, GETUTCDATE());
    
    PRINT '✅ Categories seeded successfully';
END
ELSE
BEGIN
    PRINT '⏭️  Categories already contains data, skipping...';
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- SECTION 7: PRIVILEGES
-- ═══════════════════════════════════════════════════════════════════════════════

DECLARE @ConsultationTypeId UNIQUEIDENTIFIER = (SELECT Id FROM MasterPrivilegeTypes WHERE Name = 'Consultation');
DECLARE @MedicationTypeId UNIQUEIDENTIFIER = (SELECT Id FROM MasterPrivilegeTypes WHERE Name = 'Medication');
DECLARE @MessagingTypeId UNIQUEIDENTIFIER = (SELECT Id FROM MasterPrivilegeTypes WHERE Name = 'Messaging');
DECLARE @DocumentTypeId UNIQUEIDENTIFIER = (SELECT Id FROM MasterPrivilegeTypes WHERE Name = 'Document');

IF NOT EXISTS (SELECT * FROM Privileges) AND @ConsultationTypeId IS NOT NULL
BEGIN
    PRINT 'Seeding Privileges...';
    
    INSERT INTO Privileges (Id, Name, Description, PrivilegeTypeId, IsActive, IsDeleted, CreatedDate)
    VALUES 
        (NEWID(), 'TeleConsultation', 'Video consultation with healthcare providers', @ConsultationTypeId, 1, 0, GETUTCDATE()),
        (NEWID(), 'Medication', 'Access to medication prescriptions and delivery', @MedicationTypeId, 1, 0, GETUTCDATE()),
        (NEWID(), 'Unlimited Messaging', 'Unlimited messaging with healthcare providers', @MessagingTypeId, 1, 0, GETUTCDATE()),
        (NEWID(), 'Document Access', 'Access to medical documents and reports', @DocumentTypeId, 1, 0, GETUTCDATE()),
        (NEWID(), 'Priority Support', 'Priority customer support access', @ConsultationTypeId, 1, 0, GETUTCDATE()),
        (NEWID(), 'Lab Test Access', 'Access to lab test results and recommendations', @DocumentTypeId, 1, 0, GETUTCDATE());
    
    PRINT '✅ Privileges seeded successfully';
END
ELSE
BEGIN
    PRINT '⏭️  Privileges already contains data, skipping...';
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- SECTION 8: SYSTEM SETTINGS
-- ═══════════════════════════════════════════════════════════════════════════════

IF NOT EXISTS (SELECT * FROM SystemSettings)
BEGIN
    PRINT 'Seeding SystemSettings...';
    
    INSERT INTO SystemSettings (Id, DefaultAdminCommissionPercent, DefaultPriceChangeNoticeDays, MaxFailedPaymentAttempts, LastUpdated, IsActive, IsDeleted, CreatedDate)
    VALUES 
        ('00000000-0000-0000-0000-000000000001', 20.00, 10, 3, GETUTCDATE(), 1, 0, GETUTCDATE());
    
    PRINT '✅ SystemSettings seeded successfully';
END
ELSE
BEGIN
    PRINT '⏭️  SystemSettings already contains data, skipping...';
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- VERIFICATION SECTION
-- ═══════════════════════════════════════════════════════════════════════════════

PRINT '';
PRINT '═══════════════════════════════════════════════════════════════════════════════';
PRINT 'SEED DATA SUMMARY';
PRINT '═══════════════════════════════════════════════════════════════════════════════';
PRINT '';
PRINT 'Master Billing Cycles:  ' + CAST((SELECT COUNT(*) FROM MasterBillingCycles) AS NVARCHAR(10));
PRINT 'Master Currencies:      ' + CAST((SELECT COUNT(*) FROM MasterCurrencies) AS NVARCHAR(10));
PRINT 'Master Privilege Types: ' + CAST((SELECT COUNT(*) FROM MasterPrivilegeTypes) AS NVARCHAR(10));
PRINT 'Payment Statuses:       ' + CAST((SELECT COUNT(*) FROM PaymentStatuses) AS NVARCHAR(10));
PRINT 'Refund Statuses:        ' + CAST((SELECT COUNT(*) FROM RefundStatuses) AS NVARCHAR(10));
PRINT 'Categories:             ' + CAST((SELECT COUNT(*) FROM Categories) AS NVARCHAR(10));
PRINT 'Privileges:             ' + CAST((SELECT COUNT(*) FROM Privileges) AS NVARCHAR(10));
PRINT 'System Settings:        ' + CAST((SELECT COUNT(*) FROM SystemSettings) AS NVARCHAR(10));
PRINT '';
PRINT '═══════════════════════════════════════════════════════════════════════════════';
PRINT '✅ SEED DATA SCRIPT COMPLETED SUCCESSFULLY!';
PRINT '═══════════════════════════════════════════════════════════════════════════════';
GO

