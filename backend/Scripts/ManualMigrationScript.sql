-- Manual Migration Script for SubscriptionPayment and BillingCycle Discounts
-- This script handles the existing database state properly

-- Check if BillingRecordId column exists in SubscriptionPayments table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[SubscriptionPayments]') AND name = 'BillingRecordId')
BEGIN
    -- Add BillingRecordId column if it doesn't exist
    ALTER TABLE [SubscriptionPayments] ADD [BillingRecordId] uniqueidentifier NULL;
    PRINT 'Added BillingRecordId column to SubscriptionPayments table'
END
ELSE
BEGIN
    PRINT 'BillingRecordId column already exists in SubscriptionPayments table'
END

-- Check if MonthlyBillingDiscount column exists in SubscriptionPlans table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[SubscriptionPlans]') AND name = 'MonthlyBillingDiscount')
BEGIN
    -- Add billing cycle discount columns if they don't exist
    ALTER TABLE [SubscriptionPlans] ADD [MonthlyBillingDiscount] decimal(5,2) NOT NULL DEFAULT 0.0;
    PRINT 'Added MonthlyBillingDiscount column to SubscriptionPlans table'
END
ELSE
BEGIN
    PRINT 'MonthlyBillingDiscount column already exists in SubscriptionPlans table'
END

-- Check if QuarterlyBillingDiscount column exists in SubscriptionPlans table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[SubscriptionPlans]') AND name = 'QuarterlyBillingDiscount')
BEGIN
    ALTER TABLE [SubscriptionPlans] ADD [QuarterlyBillingDiscount] decimal(5,2) NOT NULL DEFAULT 0.0;
    PRINT 'Added QuarterlyBillingDiscount column to SubscriptionPlans table'
END
ELSE
BEGIN
    PRINT 'QuarterlyBillingDiscount column already exists in SubscriptionPlans table'
END

-- Check if AnnualBillingDiscount column exists in SubscriptionPlans table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[SubscriptionPlans]') AND name = 'AnnualBillingDiscount')
BEGIN
    ALTER TABLE [SubscriptionPlans] ADD [AnnualBillingDiscount] decimal(5,2) NOT NULL DEFAULT 0.0;
    PRINT 'Added AnnualBillingDiscount column to SubscriptionPlans table'
END
ELSE
BEGIN
    PRINT 'AnnualBillingDiscount column already exists in SubscriptionPlans table'
END

-- Add foreign key constraint for BillingRecordId if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_SubscriptionPayments_BillingRecords_BillingRecordId')
BEGIN
    ALTER TABLE [SubscriptionPayments] 
    ADD CONSTRAINT [FK_SubscriptionPayments_BillingRecords_BillingRecordId] 
    FOREIGN KEY ([BillingRecordId]) REFERENCES [BillingRecords] ([Id]) ON DELETE CASCADE;
    PRINT 'Added foreign key constraint for BillingRecordId'
END
ELSE
BEGIN
    PRINT 'Foreign key constraint for BillingRecordId already exists'
END

-- Add index for BillingRecordId if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SubscriptionPayments_BillingRecordId')
BEGIN
    CREATE INDEX [IX_SubscriptionPayments_BillingRecordId] 
    ON [SubscriptionPayments] ([BillingRecordId]);
    PRINT 'Added index for BillingRecordId'
END
ELSE
BEGIN
    PRINT 'Index for BillingRecordId already exists'
END

-- Add index for NextRetryAt if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SubscriptionPayments_NextRetryAt')
BEGIN
    CREATE INDEX [IX_SubscriptionPayments_NextRetryAt] 
    ON [SubscriptionPayments] ([NextRetryAt], [Status])
    WHERE [Status] = CAST(2 AS int) AND [NextRetryAt] IS NOT NULL;
    PRINT 'Added index for NextRetryAt'
END
ELSE
BEGIN
    PRINT 'Index for NextRetryAt already exists'
END

-- Add index for CreatedDate if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SubscriptionPayments_CreatedDate')
BEGIN
    CREATE INDEX [IX_SubscriptionPayments_CreatedDate] 
    ON [SubscriptionPayments] ([CreatedDate] DESC);
    PRINT 'Added index for CreatedDate'
END
ELSE
BEGIN
    PRINT 'Index for CreatedDate already exists'
END

PRINT 'Migration script completed successfully'
