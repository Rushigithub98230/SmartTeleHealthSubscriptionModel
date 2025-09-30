-- =====================================================
-- COMPREHENSIVE SUBSCRIPTION MANAGEMENT TABLES - PART 4
-- =====================================================
-- This script adds all performance indexes for optimal query performance
-- =====================================================

-- =====================================================
-- 12. PERFORMANCE INDEXES FOR ALL TABLES
-- =====================================================

-- Subscriptions indexes
CREATE NONCLUSTERED INDEX [IX_Subscriptions_UserId] 
ON [dbo].[Subscriptions] ([UserId])
GO

CREATE NONCLUSTERED INDEX [IX_Subscriptions_SubscriptionPlanId] 
ON [dbo].[Subscriptions] ([SubscriptionPlanId])
GO

CREATE NONCLUSTERED INDEX [IX_Subscriptions_BillingCycleId] 
ON [dbo].[Subscriptions] ([BillingCycleId])
GO

CREATE NONCLUSTERED INDEX [IX_Subscriptions_ProviderId] 
ON [dbo].[Subscriptions] ([ProviderId])
GO

CREATE NONCLUSTERED INDEX [IX_Subscriptions_Status] 
ON [dbo].[Subscriptions] ([Status])
GO

CREATE NONCLUSTERED INDEX [IX_Subscriptions_StartDate] 
ON [dbo].[Subscriptions] ([StartDate])
GO

CREATE NONCLUSTERED INDEX [IX_Subscriptions_EndDate] 
ON [dbo].[Subscriptions] ([EndDate])
GO

CREATE NONCLUSTERED INDEX [IX_Subscriptions_NextBillingDate] 
ON [dbo].[Subscriptions] ([NextBillingDate])
GO

CREATE NONCLUSTERED INDEX [IX_Subscriptions_StripeSubscriptionId] 
ON [dbo].[Subscriptions] ([StripeSubscriptionId])
GO

CREATE NONCLUSTERED INDEX [IX_Subscriptions_StripeCustomerId] 
ON [dbo].[Subscriptions] ([StripeCustomerId])
GO

CREATE NONCLUSTERED INDEX [IX_Subscriptions_IsActive] 
ON [dbo].[Subscriptions] ([IsActive])
GO

CREATE NONCLUSTERED INDEX [IX_Subscriptions_IsTrialSubscription] 
ON [dbo].[Subscriptions] ([IsTrialSubscription])
GO

CREATE NONCLUSTERED INDEX [IX_Subscriptions_TrialEndDate] 
ON [dbo].[Subscriptions] ([TrialEndDate])
GO

CREATE NONCLUSTERED INDEX [IX_Subscriptions_LastUsedDate] 
ON [dbo].[Subscriptions] ([LastUsedDate])
GO

-- Composite indexes for common queries
CREATE NONCLUSTERED INDEX [IX_Subscriptions_UserId_Status_IsActive] 
ON [dbo].[Subscriptions] ([UserId], [Status], [IsActive])
GO

CREATE NONCLUSTERED INDEX [IX_Subscriptions_Status_NextBillingDate] 
ON [dbo].[Subscriptions] ([Status], [NextBillingDate])
GO

CREATE NONCLUSTERED INDEX [IX_Subscriptions_IsTrialSubscription_TrialEndDate] 
ON [dbo].[Subscriptions] ([IsTrialSubscription], [TrialEndDate])
GO

-- SubscriptionPayments indexes
CREATE NONCLUSTERED INDEX [IX_SubscriptionPayments_SubscriptionId] 
ON [dbo].[SubscriptionPayments] ([SubscriptionId])
GO

CREATE NONCLUSTERED INDEX [IX_SubscriptionPayments_CurrencyId] 
ON [dbo].[SubscriptionPayments] ([CurrencyId])
GO

CREATE NONCLUSTERED INDEX [IX_SubscriptionPayments_Status] 
ON [dbo].[SubscriptionPayments] ([Status])
GO

CREATE NONCLUSTERED INDEX [IX_SubscriptionPayments_Type] 
ON [dbo].[SubscriptionPayments] ([Type])
GO

CREATE NONCLUSTERED INDEX [IX_SubscriptionPayments_DueDate] 
ON [dbo].[SubscriptionPayments] ([DueDate])
GO

CREATE NONCLUSTERED INDEX [IX_SubscriptionPayments_PaidAt] 
ON [dbo].[SubscriptionPayments] ([PaidAt])
GO

CREATE NONCLUSTERED INDEX [IX_SubscriptionPayments_FailedAt] 
ON [dbo].[SubscriptionPayments] ([FailedAt])
GO

CREATE NONCLUSTERED INDEX [IX_SubscriptionPayments_BillingPeriodStart] 
ON [dbo].[SubscriptionPayments] ([BillingPeriodStart])
GO

CREATE NONCLUSTERED INDEX [IX_SubscriptionPayments_BillingPeriodEnd] 
ON [dbo].[SubscriptionPayments] ([BillingPeriodEnd])
GO

CREATE NONCLUSTERED INDEX [IX_SubscriptionPayments_StripePaymentIntentId] 
ON [dbo].[SubscriptionPayments] ([StripePaymentIntentId])
GO

CREATE NONCLUSTERED INDEX [IX_SubscriptionPayments_StripeInvoiceId] 
ON [dbo].[SubscriptionPayments] ([StripeInvoiceId])
GO

CREATE NONCLUSTERED INDEX [IX_SubscriptionPayments_IsActive] 
ON [dbo].[SubscriptionPayments] ([IsActive])
GO

-- Composite indexes for common queries
CREATE NONCLUSTERED INDEX [IX_SubscriptionPayments_SubscriptionId_Status] 
ON [dbo].[SubscriptionPayments] ([SubscriptionId], [Status])
GO

CREATE NONCLUSTERED INDEX [IX_SubscriptionPayments_Status_DueDate] 
ON [dbo].[SubscriptionPayments] ([Status], [DueDate])
GO

CREATE NONCLUSTERED INDEX [IX_SubscriptionPayments_BillingPeriodStart_End] 
ON [dbo].[SubscriptionPayments] ([BillingPeriodStart], [BillingPeriodEnd])
GO

-- BillingRecords indexes
CREATE NONCLUSTERED INDEX [IX_BillingRecords_UserId] 
ON [dbo].[BillingRecords] ([UserId])
GO

CREATE NONCLUSTERED INDEX [IX_BillingRecords_SubscriptionId] 
ON [dbo].[BillingRecords] ([SubscriptionId])
GO

CREATE NONCLUSTERED INDEX [IX_BillingRecords_ConsultationId] 
ON [dbo].[BillingRecords] ([ConsultationId])
GO

CREATE NONCLUSTERED INDEX [IX_BillingRecords_MedicationDeliveryId] 
ON [dbo].[BillingRecords] ([MedicationDeliveryId])
GO

CREATE NONCLUSTERED INDEX [IX_BillingRecords_BillingCycleId] 
ON [dbo].[BillingRecords] ([BillingCycleId])
GO

CREATE NONCLUSTERED INDEX [IX_BillingRecords_CurrencyId] 
ON [dbo].[BillingRecords] ([CurrencyId])
GO

CREATE NONCLUSTERED INDEX [IX_BillingRecords_Status] 
ON [dbo].[BillingRecords] ([Status])
GO

CREATE NONCLUSTERED INDEX [IX_BillingRecords_Type] 
ON [dbo].[BillingRecords] ([Type])
GO

CREATE NONCLUSTERED INDEX [IX_BillingRecords_BillingDate] 
ON [dbo].[BillingRecords] ([BillingDate])
GO

CREATE NONCLUSTERED INDEX [IX_BillingRecords_PaidAt] 
ON [dbo].[BillingRecords] ([PaidAt])
GO

CREATE NONCLUSTERED INDEX [IX_BillingRecords_DueDate] 
ON [dbo].[BillingRecords] ([DueDate])
GO

CREATE NONCLUSTERED INDEX [IX_BillingRecords_InvoiceNumber] 
ON [dbo].[BillingRecords] ([InvoiceNumber])
GO

CREATE NONCLUSTERED INDEX [IX_BillingRecords_StripePaymentIntentId] 
ON [dbo].[BillingRecords] ([StripePaymentIntentId])
GO

CREATE NONCLUSTERED INDEX [IX_BillingRecords_StripeInvoiceId] 
ON [dbo].[BillingRecords] ([StripeInvoiceId])
GO

CREATE NONCLUSTERED INDEX [IX_BillingRecords_IsRecurring] 
ON [dbo].[BillingRecords] ([IsRecurring])
GO

CREATE NONCLUSTERED INDEX [IX_BillingRecords_NextBillingDate] 
ON [dbo].[BillingRecords] ([NextBillingDate])
GO

CREATE NONCLUSTERED INDEX [IX_BillingRecords_IsActive] 
ON [dbo].[BillingRecords] ([IsActive])
GO

-- Composite indexes for common queries
CREATE NONCLUSTERED INDEX [IX_BillingRecords_UserId_Status] 
ON [dbo].[BillingRecords] ([UserId], [Status])
GO

CREATE NONCLUSTERED INDEX [IX_BillingRecords_SubscriptionId_Status] 
ON [dbo].[BillingRecords] ([SubscriptionId], [Status])
GO

CREATE NONCLUSTERED INDEX [IX_BillingRecords_Status_DueDate] 
ON [dbo].[BillingRecords] ([Status], [DueDate])
GO

CREATE NONCLUSTERED INDEX [IX_BillingRecords_Type_BillingDate] 
ON [dbo].[BillingRecords] ([Type], [BillingDate])
GO

-- BillingAdjustments indexes
CREATE NONCLUSTERED INDEX [IX_BillingAdjustments_BillingRecordId] 
ON [dbo].[BillingAdjustments] ([BillingRecordId])
GO

CREATE NONCLUSTERED INDEX [IX_BillingAdjustments_Type] 
ON [dbo].[BillingAdjustments] ([Type])
GO

CREATE NONCLUSTERED INDEX [IX_BillingAdjustments_AppliedAt] 
ON [dbo].[BillingAdjustments] ([AppliedAt])
GO

CREATE NONCLUSTERED INDEX [IX_BillingAdjustments_AppliedBy] 
ON [dbo].[BillingAdjustments] ([AppliedBy])
GO

CREATE NONCLUSTERED INDEX [IX_BillingAdjustments_IsApproved] 
ON [dbo].[BillingAdjustments] ([IsApproved])
GO

CREATE NONCLUSTERED INDEX [IX_BillingAdjustments_IsActive] 
ON [dbo].[BillingAdjustments] ([IsActive])
GO

-- Composite indexes for common queries
CREATE NONCLUSTERED INDEX [IX_BillingAdjustments_BillingRecordId_Type] 
ON [dbo].[BillingAdjustments] ([BillingRecordId], [Type])
GO

CREATE NONCLUSTERED INDEX [IX_BillingAdjustments_Type_AppliedAt] 
ON [dbo].[BillingAdjustments] ([Type], [AppliedAt])
GO

-- PaymentRefunds indexes
CREATE NONCLUSTERED INDEX [IX_PaymentRefunds_SubscriptionPaymentId] 
ON [dbo].[PaymentRefunds] ([SubscriptionPaymentId])
GO

CREATE NONCLUSTERED INDEX [IX_PaymentRefunds_RefundedAt] 
ON [dbo].[PaymentRefunds] ([RefundedAt])
GO

CREATE NONCLUSTERED INDEX [IX_PaymentRefunds_ProcessedByUserId] 
ON [dbo].[PaymentRefunds] ([ProcessedByUserId])
GO

CREATE NONCLUSTERED INDEX [IX_PaymentRefunds_StripeRefundId] 
ON [dbo].[PaymentRefunds] ([StripeRefundId])
GO

CREATE NONCLUSTERED INDEX [IX_PaymentRefunds_IsActive] 
ON [dbo].[PaymentRefunds] ([IsActive])
GO

-- SubscriptionPlanPrivileges indexes
CREATE NONCLUSTERED INDEX [IX_SubscriptionPlanPrivileges_SubscriptionPlanId] 
ON [dbo].[SubscriptionPlanPrivileges] ([SubscriptionPlanId])
GO

CREATE NONCLUSTERED INDEX [IX_SubscriptionPlanPrivileges_PrivilegeId] 
ON [dbo].[SubscriptionPlanPrivileges] ([PrivilegeId])
GO

CREATE NONCLUSTERED INDEX [IX_SubscriptionPlanPrivileges_UsagePeriodId] 
ON [dbo].[SubscriptionPlanPrivileges] ([UsagePeriodId])
GO

CREATE NONCLUSTERED INDEX [IX_SubscriptionPlanPrivileges_EffectiveDate] 
ON [dbo].[SubscriptionPlanPrivileges] ([EffectiveDate])
GO

CREATE NONCLUSTERED INDEX [IX_SubscriptionPlanPrivileges_ExpirationDate] 
ON [dbo].[SubscriptionPlanPrivileges] ([ExpirationDate])
GO

CREATE NONCLUSTERED INDEX [IX_SubscriptionPlanPrivileges_IsActive] 
ON [dbo].[SubscriptionPlanPrivileges] ([IsActive])
GO

-- Composite indexes for common queries
CREATE NONCLUSTERED INDEX [IX_SubscriptionPlanPrivileges_SubscriptionPlanId_PrivilegeId] 
ON [dbo].[SubscriptionPlanPrivileges] ([SubscriptionPlanId], [PrivilegeId])
GO

CREATE NONCLUSTERED INDEX [IX_SubscriptionPlanPrivileges_PrivilegeId_IsActive] 
ON [dbo].[SubscriptionPlanPrivileges] ([PrivilegeId], [IsActive])
GO

CREATE NONCLUSTERED INDEX [IX_SubscriptionPlanPrivileges_EffectiveDate_ExpirationDate] 
ON [dbo].[SubscriptionPlanPrivileges] ([EffectiveDate], [ExpirationDate])
GO

-- UserSubscriptionPrivilegeUsages indexes
CREATE NONCLUSTERED INDEX [IX_UserSubscriptionPrivilegeUsages_SubscriptionId] 
ON [dbo].[UserSubscriptionPrivilegeUsages] ([SubscriptionId])
GO

CREATE NONCLUSTERED INDEX [IX_UserSubscriptionPrivilegeUsages_SubscriptionPlanPrivilegeId] 
ON [dbo].[UserSubscriptionPrivilegeUsages] ([SubscriptionPlanPrivilegeId])
GO

CREATE NONCLUSTERED INDEX [IX_UserSubscriptionPrivilegeUsages_PrivilegeId] 
ON [dbo].[UserSubscriptionPrivilegeUsages] ([PrivilegeId])
GO

CREATE NONCLUSTERED INDEX [IX_UserSubscriptionPrivilegeUsages_UsagePeriodStart] 
ON [dbo].[UserSubscriptionPrivilegeUsages] ([UsagePeriodStart])
GO

CREATE NONCLUSTERED INDEX [IX_UserSubscriptionPrivilegeUsages_UsagePeriodEnd] 
ON [dbo].[UserSubscriptionPrivilegeUsages] ([UsagePeriodEnd])
GO

CREATE NONCLUSTERED INDEX [IX_UserSubscriptionPrivilegeUsages_LastUsedAt] 
ON [dbo].[UserSubscriptionPrivilegeUsages] ([LastUsedAt])
GO

CREATE NONCLUSTERED INDEX [IX_UserSubscriptionPrivilegeUsages_ResetAt] 
ON [dbo].[UserSubscriptionPrivilegeUsages] ([ResetAt])
GO

CREATE NONCLUSTERED INDEX [IX_UserSubscriptionPrivilegeUsages_IsActive] 
ON [dbo].[UserSubscriptionPrivilegeUsages] ([IsActive])
GO

-- Composite indexes for common queries
CREATE NONCLUSTERED INDEX [IX_UserSubscriptionPrivilegeUsages_SubscriptionId_PrivilegeId] 
ON [dbo].[UserSubscriptionPrivilegeUsages] ([SubscriptionId], [PrivilegeId])
GO

CREATE NONCLUSTERED INDEX [IX_UserSubscriptionPrivilegeUsages_PrivilegeId_UsagePeriodStart] 
ON [dbo].[UserSubscriptionPrivilegeUsages] ([PrivilegeId], [UsagePeriodStart])
GO

CREATE NONCLUSTERED INDEX [IX_UserSubscriptionPrivilegeUsages_UsagePeriodStart_End] 
ON [dbo].[UserSubscriptionPrivilegeUsages] ([UsagePeriodStart], [UsagePeriodEnd])
GO

-- PrivilegeUsageHistories indexes
CREATE NONCLUSTERED INDEX [IX_PrivilegeUsageHistories_UserSubscriptionPrivilegeUsageId] 
ON [dbo].[PrivilegeUsageHistories] ([UserSubscriptionPrivilegeUsageId])
GO

CREATE NONCLUSTERED INDEX [IX_PrivilegeUsageHistories_UsedAt] 
ON [dbo].[PrivilegeUsageHistories] ([UsedAt])
GO

CREATE NONCLUSTERED INDEX [IX_PrivilegeUsageHistories_UsageDate] 
ON [dbo].[PrivilegeUsageHistories] ([UsageDate])
GO

CREATE NONCLUSTERED INDEX [IX_PrivilegeUsageHistories_UsageWeek] 
ON [dbo].[PrivilegeUsageHistories] ([UsageWeek])
GO

CREATE NONCLUSTERED INDEX [IX_PrivilegeUsageHistories_UsageMonth] 
ON [dbo].[PrivilegeUsageHistories] ([UsageMonth])
GO

CREATE NONCLUSTERED INDEX [IX_PrivilegeUsageHistories_IsActive] 
ON [dbo].[PrivilegeUsageHistories] ([IsActive])
GO

-- Composite indexes for common queries
CREATE NONCLUSTERED INDEX [IX_PrivilegeUsageHistories_UserSubscriptionPrivilegeUsageId_UsedAt] 
ON [dbo].[PrivilegeUsageHistories] ([UserSubscriptionPrivilegeUsageId], [UsedAt])
GO

CREATE NONCLUSTERED INDEX [IX_PrivilegeUsageHistories_UsageDate_UsageWeek] 
ON [dbo].[PrivilegeUsageHistories] ([UsageDate], [UsageWeek])
GO

CREATE NONCLUSTERED INDEX [IX_PrivilegeUsageHistories_UsageMonth_UsageDate] 
ON [dbo].[PrivilegeUsageHistories] ([UsageMonth], [UsageDate])
GO

-- SubscriptionStatusHistories indexes
CREATE NONCLUSTERED INDEX [IX_SubscriptionStatusHistories_SubscriptionId] 
ON [dbo].[SubscriptionStatusHistories] ([SubscriptionId])
GO

CREATE NONCLUSTERED INDEX [IX_SubscriptionStatusHistories_FromStatus] 
ON [dbo].[SubscriptionStatusHistories] ([FromStatus])
GO

CREATE NONCLUSTERED INDEX [IX_SubscriptionStatusHistories_ToStatus] 
ON [dbo].[SubscriptionStatusHistories] ([ToStatus])
GO

CREATE NONCLUSTERED INDEX [IX_SubscriptionStatusHistories_ChangedByUserId] 
ON [dbo].[SubscriptionStatusHistories] ([ChangedByUserId])
GO

CREATE NONCLUSTERED INDEX [IX_SubscriptionStatusHistories_ChangedAt] 
ON [dbo].[SubscriptionStatusHistories] ([ChangedAt])
GO

CREATE NONCLUSTERED INDEX [IX_SubscriptionStatusHistories_IsActive] 
ON [dbo].[SubscriptionStatusHistories] ([IsActive])
GO

-- Composite indexes for common queries
CREATE NONCLUSTERED INDEX [IX_SubscriptionStatusHistories_SubscriptionId_ChangedAt] 
ON [dbo].[SubscriptionStatusHistories] ([SubscriptionId], [ChangedAt])
GO

CREATE NONCLUSTERED INDEX [IX_SubscriptionStatusHistories_FromStatus_ToStatus] 
ON [dbo].[SubscriptionStatusHistories] ([FromStatus], [ToStatus])
GO

CREATE NONCLUSTERED INDEX [IX_SubscriptionStatusHistories_ToStatus_ChangedAt] 
ON [dbo].[SubscriptionStatusHistories] ([ToStatus], [ChangedAt])
GO

-- ServiceConstraints indexes
CREATE NONCLUSTERED INDEX [IX_ServiceConstraints_ServiceName] 
ON [dbo].[ServiceConstraints] ([ServiceName])
GO

CREATE NONCLUSTERED INDEX [IX_ServiceConstraints_Type] 
ON [dbo].[ServiceConstraints] ([Type])
GO

CREATE NONCLUSTERED INDEX [IX_ServiceConstraints_SubscriptionPlanId] 
ON [dbo].[ServiceConstraints] ([SubscriptionPlanId])
GO

CREATE NONCLUSTERED INDEX [IX_ServiceConstraints_IsActive] 
ON [dbo].[ServiceConstraints] ([IsActive])
GO

-- Composite indexes for common queries
CREATE NONCLUSTERED INDEX [IX_ServiceConstraints_SubscriptionPlanId_ServiceName] 
ON [dbo].[ServiceConstraints] ([SubscriptionPlanId], [ServiceName])
GO

CREATE NONCLUSTERED INDEX [IX_ServiceConstraints_ServiceName_Type] 
ON [dbo].[ServiceConstraints] ([ServiceName], [Type])
GO

CREATE NONCLUSTERED INDEX [IX_ServiceConstraints_SubscriptionPlanId_IsActive] 
ON [dbo].[ServiceConstraints] ([SubscriptionPlanId], [IsActive])
GO

-- =====================================================
-- 13. AUDIT TRAIL INDEXES (BaseEntity properties)
-- =====================================================

-- Common audit trail indexes for all tables
-- These indexes help with audit queries and user activity tracking

-- CreatedBy indexes
CREATE NONCLUSTERED INDEX [IX_MasterPrivilegeTypes_CreatedBy] 
ON [dbo].[MasterPrivilegeTypes] ([CreatedBy])
GO

CREATE NONCLUSTERED INDEX [IX_MasterCurrencies_CreatedBy] 
ON [dbo].[MasterCurrencies] ([CreatedBy])
GO

CREATE NONCLUSTERED INDEX [IX_MasterBillingCycles_CreatedBy] 
ON [dbo].[MasterBillingCycles] ([CreatedBy])
GO

CREATE NONCLUSTERED INDEX [IX_Privileges_CreatedBy] 
ON [dbo].[Privileges] ([CreatedBy])
GO

CREATE NONCLUSTERED INDEX [IX_SubscriptionPlans_CreatedBy] 
ON [dbo].[SubscriptionPlans] ([CreatedBy])
GO

CREATE NONCLUSTERED INDEX [IX_Subscriptions_CreatedBy] 
ON [dbo].[Subscriptions] ([CreatedBy])
GO

CREATE NONCLUSTERED INDEX [IX_SubscriptionPayments_CreatedBy] 
ON [dbo].[SubscriptionPayments] ([CreatedBy])
GO

CREATE NONCLUSTERED INDEX [IX_BillingRecords_CreatedBy] 
ON [dbo].[BillingRecords] ([CreatedBy])
GO

CREATE NONCLUSTERED INDEX [IX_BillingAdjustments_CreatedBy] 
ON [dbo].[BillingAdjustments] ([CreatedBy])
GO

CREATE NONCLUSTERED INDEX [IX_PaymentRefunds_CreatedBy] 
ON [dbo].[PaymentRefunds] ([CreatedBy])
GO

CREATE NONCLUSTERED INDEX [IX_SubscriptionPlanPrivileges_CreatedBy] 
ON [dbo].[SubscriptionPlanPrivileges] ([CreatedBy])
GO

CREATE NONCLUSTERED INDEX [IX_UserSubscriptionPrivilegeUsages_CreatedBy] 
ON [dbo].[UserSubscriptionPrivilegeUsages] ([CreatedBy])
GO

CREATE NONCLUSTERED INDEX [IX_PrivilegeUsageHistories_CreatedBy] 
ON [dbo].[PrivilegeUsageHistories] ([CreatedBy])
GO

CREATE NONCLUSTERED INDEX [IX_SubscriptionStatusHistories_CreatedBy] 
ON [dbo].[SubscriptionStatusHistories] ([CreatedBy])
GO

CREATE NONCLUSTERED INDEX [IX_ServiceConstraints_CreatedBy] 
ON [dbo].[ServiceConstraints] ([CreatedBy])
GO

-- CreatedDate indexes
CREATE NONCLUSTERED INDEX [IX_MasterPrivilegeTypes_CreatedDate] 
ON [dbo].[MasterPrivilegeTypes] ([CreatedDate])
GO

CREATE NONCLUSTERED INDEX [IX_MasterCurrencies_CreatedDate] 
ON [dbo].[MasterCurrencies] ([CreatedDate])
GO

CREATE NONCLUSTERED INDEX [IX_MasterBillingCycles_CreatedDate] 
ON [dbo].[MasterBillingCycles] ([CreatedDate])
GO

CREATE NONCLUSTERED INDEX [IX_Privileges_CreatedDate] 
ON [dbo].[Privileges] ([CreatedDate])
GO

CREATE NONCLUSTERED INDEX [IX_SubscriptionPlans_CreatedDate] 
ON [dbo].[SubscriptionPlans] ([CreatedDate])
GO

CREATE NONCLUSTERED INDEX [IX_Subscriptions_CreatedDate] 
ON [dbo].[Subscriptions] ([CreatedDate])
GO

CREATE NONCLUSTERED INDEX [IX_SubscriptionPayments_CreatedDate] 
ON [dbo].[SubscriptionPayments] ([CreatedDate])
GO

CREATE NONCLUSTERED INDEX [IX_BillingRecords_CreatedDate] 
ON [dbo].[BillingRecords] ([CreatedDate])
GO

CREATE NONCLUSTERED INDEX [IX_BillingAdjustments_CreatedDate] 
ON [dbo].[BillingAdjustments] ([CreatedDate])
GO

CREATE NONCLUSTERED INDEX [IX_PaymentRefunds_CreatedDate] 
ON [dbo].[PaymentRefunds] ([CreatedDate])
GO

CREATE NONCLUSTERED INDEX [IX_SubscriptionPlanPrivileges_CreatedDate] 
ON [dbo].[SubscriptionPlanPrivileges] ([CreatedDate])
GO

CREATE NONCLUSTERED INDEX [IX_UserSubscriptionPrivilegeUsages_CreatedDate] 
ON [dbo].[UserSubscriptionPrivilegeUsages] ([CreatedDate])
GO

CREATE NONCLUSTERED INDEX [IX_PrivilegeUsageHistories_CreatedDate] 
ON [dbo].[PrivilegeUsageHistories] ([CreatedDate])
GO

CREATE NONCLUSTERED INDEX [IX_SubscriptionStatusHistories_CreatedDate] 
ON [dbo].[SubscriptionStatusHistories] ([CreatedDate])
GO

CREATE NONCLUSTERED INDEX [IX_ServiceConstraints_CreatedDate] 
ON [dbo].[ServiceConstraints] ([CreatedDate])
GO

-- IsActive indexes (for filtering active records)
CREATE NONCLUSTERED INDEX [IX_MasterPrivilegeTypes_IsActive] 
ON [dbo].[MasterPrivilegeTypes] ([IsActive])
GO

CREATE NONCLUSTERED INDEX [IX_MasterCurrencies_IsActive] 
ON [dbo].[MasterCurrencies] ([IsActive])
GO

CREATE NONCLUSTERED INDEX [IX_MasterBillingCycles_IsActive] 
ON [dbo].[MasterBillingCycles] ([IsActive])
GO

CREATE NONCLUSTERED INDEX [IX_Privileges_IsActive] 
ON [dbo].[Privileges] ([IsActive])
GO

CREATE NONCLUSTERED INDEX [IX_SubscriptionPlans_IsActive] 
ON [dbo].[SubscriptionPlans] ([IsActive])
GO

CREATE NONCLUSTERED INDEX [IX_Subscriptions_IsActive] 
ON [dbo].[Subscriptions] ([IsActive])
GO

CREATE NONCLUSTERED INDEX [IX_SubscriptionPayments_IsActive] 
ON [dbo].[SubscriptionPayments] ([IsActive])
GO

CREATE NONCLUSTERED INDEX [IX_BillingRecords_IsActive] 
ON [dbo].[BillingRecords] ([IsActive])
GO

CREATE NONCLUSTERED INDEX [IX_BillingAdjustments_IsActive] 
ON [dbo].[BillingAdjustments] ([IsActive])
GO

CREATE NONCLUSTERED INDEX [IX_PaymentRefunds_IsActive] 
ON [dbo].[PaymentRefunds] ([IsActive])
GO

CREATE NONCLUSTERED INDEX [IX_SubscriptionPlanPrivileges_IsActive] 
ON [dbo].[SubscriptionPlanPrivileges] ([IsActive])
GO

CREATE NONCLUSTERED INDEX [IX_UserSubscriptionPrivilegeUsages_IsActive] 
ON [dbo].[UserSubscriptionPrivilegeUsages] ([IsActive])
GO

CREATE NONCLUSTERED INDEX [IX_PrivilegeUsageHistories_IsActive] 
ON [dbo].[PrivilegeUsageHistories] ([IsActive])
GO

CREATE NONCLUSTERED INDEX [IX_SubscriptionStatusHistories_IsActive] 
ON [dbo].[SubscriptionStatusHistories] ([IsActive])
GO

CREATE NONCLUSTERED INDEX [IX_ServiceConstraints_IsActive] 
ON [dbo].[ServiceConstraints] ([IsActive])
GO

-- =====================================================
-- 14. VERIFICATION QUERIES
-- =====================================================
PRINT 'All performance indexes created successfully!'
PRINT 'Verifying table structure...'

-- Check if all tables exist
DECLARE @TableCount INT = 0
DECLARE @ExpectedTables INT = 15

-- Count existing tables
SELECT @TableCount = COUNT(*) 
FROM sys.objects 
WHERE type = 'U' 
AND name IN (
    'MasterPrivilegeTypes', 'MasterCurrencies', 'MasterBillingCycles',
    'Privileges', 'SubscriptionPlans', 'Subscriptions', 'SubscriptionPayments',
    'BillingRecords', 'BillingAdjustments', 'PaymentRefunds',
    'SubscriptionPlanPrivileges', 'UserSubscriptionPrivilegeUsages',
    'PrivilegeUsageHistories', 'SubscriptionStatusHistories', 'ServiceConstraints'
)

IF @TableCount = @ExpectedTables
    PRINT '✓ All ' + CAST(@ExpectedTables AS VARCHAR(10)) + ' tables created successfully'
ELSE
    PRINT '✗ Only ' + CAST(@TableCount AS VARCHAR(10)) + ' out of ' + CAST(@ExpectedTables AS VARCHAR(10)) + ' tables were created'

-- Check constraints
SELECT 
    'Check Constraints' as ConstraintType,
    COUNT(*) as Count
FROM sys.check_constraints 
WHERE parent_object_id IN (
    SELECT object_id FROM sys.objects 
    WHERE type = 'U' 
    AND name IN (
        'MasterPrivilegeTypes', 'MasterCurrencies', 'MasterBillingCycles',
        'Privileges', 'SubscriptionPlans', 'Subscriptions', 'SubscriptionPayments',
        'BillingRecords', 'BillingAdjustments', 'PaymentRefunds',
        'SubscriptionPlanPrivileges', 'UserSubscriptionPrivilegeUsages',
        'PrivilegeUsageHistories', 'SubscriptionStatusHistories', 'ServiceConstraints'
    )
)

UNION ALL

-- Check foreign keys
SELECT 
    'Foreign Keys' as ConstraintType,
    COUNT(*) as Count
FROM sys.foreign_keys 
WHERE parent_object_id IN (
    SELECT object_id FROM sys.objects 
    WHERE type = 'U' 
    AND name IN (
        'MasterPrivilegeTypes', 'MasterCurrencies', 'MasterBillingCycles',
        'Privileges', 'SubscriptionPlans', 'Subscriptions', 'SubscriptionPayments',
        'BillingRecords', 'BillingAdjustments', 'PaymentRefunds',
        'SubscriptionPlanPrivileges', 'UserSubscriptionPrivilegeUsages',
        'PrivilegeUsageHistories', 'SubscriptionStatusHistories', 'ServiceConstraints'
    )
)

UNION ALL

-- Check indexes
SELECT 
    'Indexes' as ConstraintType,
    COUNT(*) as Count
FROM sys.indexes 
WHERE object_id IN (
    SELECT object_id FROM sys.objects 
    WHERE type = 'U' 
    AND name IN (
        'MasterPrivilegeTypes', 'MasterCurrencies', 'MasterBillingCycles',
        'Privileges', 'SubscriptionPlans', 'Subscriptions', 'SubscriptionPayments',
        'BillingRecords', 'BillingAdjustments', 'PaymentRefunds',
        'SubscriptionPlanPrivileges', 'UserSubscriptionPrivilegeUsages',
        'PrivilegeUsageHistories', 'SubscriptionStatusHistories', 'ServiceConstraints'
    )
) 
AND name IS NOT NULL

PRINT 'Database schema creation completed successfully!'
PRINT 'All subscription management tables are ready for use.'
