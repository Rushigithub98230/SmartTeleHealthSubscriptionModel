-- ═══════════════════════════════════════════════════════════════════════════════
-- COMPLETE SUBSCRIPTION MANAGEMENT MODULE - SQL SERVER DDL SCRIPT
-- Created: 2025-01-XX
-- Purpose: Create all tables needed for end-to-end subscription management
-- Includes: Master Data, Subscription Plans, Subscriptions, Privileges, Billing,
--           Payments, Refunds, Webhooks, Versioning, and Supporting Tables
-- ═══════════════════════════════════════════════════════════════════════════════
-- THIS SCRIPT USES EXACT TABLE NAMES FROM DbContext
-- ═══════════════════════════════════════════════════════════════════════════════
-- IMPORTANT: This script requires an existing [dbo].[User] table with [UserID] column
-- All foreign keys referencing Users are configured to use [dbo].[User]([UserID])
-- ═══════════════════════════════════════════════════════════════════════════════

USE [SmartTelehealth]
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- SECTION 1: MASTER DATA TABLES
-- ═══════════════════════════════════════════════════════════════════════════════

-- 1.1 MasterBillingCycles
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MasterBillingCycles')
BEGIN
    CREATE TABLE [MasterBillingCycles] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [Name] NVARCHAR(50) NOT NULL,
        [Description] NVARCHAR(200) NULL,
        [DurationInDays] INT NOT NULL,
        [SortOrder] INT NOT NULL DEFAULT 0,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [CreatedBy] INT NULL,
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedBy] INT NULL,
        [UpdatedDate] DATETIME2 NULL,
        [DeletedBy] INT NULL,
        [DeletedDate] DATETIME2 NULL
    );
    
    CREATE INDEX [IX_MasterBillingCycles_Name] ON [MasterBillingCycles]([Name]);
    CREATE INDEX [IX_MasterBillingCycles_DurationInDays] ON [MasterBillingCycles]([DurationInDays]);
    CREATE INDEX [IX_MasterBillingCycles_SortOrder] ON [MasterBillingCycles]([SortOrder]);
END
GO

-- 1.2 MasterCurrencies
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MasterCurrencies')
BEGIN
    CREATE TABLE [MasterCurrencies] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [Code] NVARCHAR(10) NOT NULL,
        [Name] NVARCHAR(50) NOT NULL,
        [Symbol] NVARCHAR(10) NULL,
        [SortOrder] INT NOT NULL DEFAULT 0,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [CreatedBy] INT NULL,
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedBy] INT NULL,
        [UpdatedDate] DATETIME2 NULL,
        [DeletedBy] INT NULL,
        [DeletedDate] DATETIME2 NULL
    );
    
    CREATE UNIQUE INDEX [IX_MasterCurrencies_Code] ON [MasterCurrencies]([Code]) WHERE [IsDeleted] = 0;
    CREATE INDEX [IX_MasterCurrencies_Name] ON [MasterCurrencies]([Name]);
    CREATE INDEX [IX_MasterCurrencies_SortOrder] ON [MasterCurrencies]([SortOrder]);
END
GO

-- 1.3 MasterPrivilegeTypes
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MasterPrivilegeTypes')
BEGIN
    CREATE TABLE [MasterPrivilegeTypes] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [Name] NVARCHAR(50) NOT NULL,
        [Description] NVARCHAR(200) NULL,
        [SortOrder] INT NOT NULL DEFAULT 0,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [CreatedBy] INT NULL,
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedBy] INT NULL,
        [UpdatedDate] DATETIME2 NULL,
        [DeletedBy] INT NULL,
        [DeletedDate] DATETIME2 NULL
    );
    
    CREATE INDEX [IX_MasterPrivilegeTypes_Name] ON [MasterPrivilegeTypes]([Name]);
    CREATE INDEX [IX_MasterPrivilegeTypes_SortOrder] ON [MasterPrivilegeTypes]([SortOrder]);
END
GO

-- 1.4 PaymentStatuses
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PaymentStatuses')
BEGIN
    CREATE TABLE [PaymentStatuses] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [Name] NVARCHAR(50) NOT NULL,
        [Description] NVARCHAR(200) NULL,
        [SortOrder] INT NOT NULL DEFAULT 0,
        [Color] NVARCHAR(50) NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [CreatedBy] INT NULL,
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedBy] INT NULL,
        [UpdatedDate] DATETIME2 NULL,
        [DeletedBy] INT NULL,
        [DeletedDate] DATETIME2 NULL
    );
    
    CREATE INDEX [IX_PaymentStatuses_Name] ON [PaymentStatuses]([Name]);
    CREATE INDEX [IX_PaymentStatuses_SortOrder] ON [PaymentStatuses]([SortOrder]);
END
GO

-- 1.5 RefundStatuses
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RefundStatuses')
BEGIN
    CREATE TABLE [RefundStatuses] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [Name] NVARCHAR(50) NOT NULL,
        [Description] NVARCHAR(200) NULL,
        [SortOrder] INT NOT NULL DEFAULT 0,
        [Color] NVARCHAR(50) NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [CreatedBy] INT NULL,
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedBy] INT NULL,
        [UpdatedDate] DATETIME2 NULL,
        [DeletedBy] INT NULL,
        [DeletedDate] DATETIME2 NULL
    );
    
    CREATE INDEX [IX_RefundStatuses_Name] ON [RefundStatuses]([Name]);
    CREATE INDEX [IX_RefundStatuses_SortOrder] ON [RefundStatuses]([SortOrder]);
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- SECTION 2: SUPPORTING TABLES
-- ═══════════════════════════════════════════════════════════════════════════════

-- 2.1 Categories
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Categories')
BEGIN
    CREATE TABLE [Categories] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [Name] NVARCHAR(100) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [CreatedBy] INT NULL,
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedBy] INT NULL,
        [UpdatedDate] DATETIME2 NULL,
        [DeletedBy] INT NULL,
        [DeletedDate] DATETIME2 NULL
    );
    
    CREATE INDEX [IX_Categories_Name] ON [Categories]([Name]);
    CREATE INDEX [IX_Categories_IsActive] ON [Categories]([IsActive]);
END
GO

-- 2.2 SystemSettings
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SystemSettings')
BEGIN
    CREATE TABLE [SystemSettings] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [DefaultAdminCommissionPercent] DECIMAL(5,2) NOT NULL DEFAULT 20.00,
        [DefaultPriceChangeNoticeDays] INT NOT NULL DEFAULT 10,
        [MaxFailedPaymentAttempts] INT NOT NULL DEFAULT 3,
        [LastUpdated] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [CreatedBy] INT NULL,
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedBy] INT NULL,
        [UpdatedDate] DATETIME2 NULL,
        [DeletedBy] INT NULL,
        [DeletedDate] DATETIME2 NULL
    );
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- SECTION 3: SUBSCRIPTION PLAN TABLES
-- ═══════════════════════════════════════════════════════════════════════════════

-- 3.1 SubscriptionPlans
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SubscriptionPlans')
BEGIN
    CREATE TABLE [SubscriptionPlans] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [Name] NVARCHAR(100) NOT NULL,
        [Description] NVARCHAR(1000) NULL,
        [ShortDescription] NVARCHAR(200) NULL,
        [IsFeatured] BIT NOT NULL DEFAULT 0,
        [IsTrialAllowed] BIT NOT NULL DEFAULT 0,
        [TrialDurationInDays] INT NOT NULL DEFAULT 0,
        [IsMostPopular] BIT NOT NULL DEFAULT 0,
        [IsTrending] BIT NOT NULL DEFAULT 0,
        [DisplayOrder] INT NOT NULL DEFAULT 0,
        [PlanType] NVARCHAR(50) NOT NULL,
        [BasePrice] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [DiscountPercentage] DECIMAL(5,2) NULL,
        [DiscountValidUntil] DATETIME2 NULL,
        [BillingDiscountPercentage] DECIMAL(5,2) NULL,
        [VersionNumber] INT NOT NULL DEFAULT 1,
        [IsLatestVersion] BIT NOT NULL DEFAULT 1,
        [ParentPlanId] UNIQUEIDENTIFIER NULL,
        [IsAutoCalculatedPrice] BIT NOT NULL DEFAULT 1,
        [PrivilegesTotalCost] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [AdminCommissionPercent] DECIMAL(5,2) NULL,
        [PriceChangeNoticeDays] INT NOT NULL DEFAULT 10,
        [VersionCreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [BillingCycleId] UNIQUEIDENTIFIER NOT NULL,
        [CurrencyId] UNIQUEIDENTIFIER NOT NULL,
        [CategoryId] UNIQUEIDENTIFIER NOT NULL,
        [StripeProductId] NVARCHAR(100) NULL,
        [StripePriceId] NVARCHAR(100) NULL,
        [MessagingCount] INT NOT NULL DEFAULT 10,
        [IncludesMedicationDelivery] BIT NOT NULL DEFAULT 1,
        [IncludesFollowUpCare] BIT NOT NULL DEFAULT 1,
        [DeliveryFrequencyDays] INT NOT NULL DEFAULT 30,
        [MaxPauseDurationDays] INT NOT NULL DEFAULT 90,
        [Features] NVARCHAR(1000) NULL,
        [Terms] NVARCHAR(500) NULL,
        [EffectiveDate] DATETIME2 NULL,
        [ExpirationDate] DATETIME2 NULL,
        [DefaultTaxPercentage] DECIMAL(5,2) NULL,
        [TaxNotes] NVARCHAR(500) NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [CreatedBy] INT NULL,
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedBy] INT NULL,
        [UpdatedDate] DATETIME2 NULL,
        [DeletedBy] INT NULL,
        [DeletedDate] DATETIME2 NULL
    );
    
    -- Foreign Keys
    ALTER TABLE [SubscriptionPlans] ADD CONSTRAINT [FK_SubscriptionPlans_BillingCycle_BillingCycleId]
        FOREIGN KEY ([BillingCycleId]) REFERENCES [MasterBillingCycles]([Id]) ON DELETE RESTRICT;
    ALTER TABLE [SubscriptionPlans] ADD CONSTRAINT [FK_SubscriptionPlans_Currency_CurrencyId]
        FOREIGN KEY ([CurrencyId]) REFERENCES [MasterCurrencies]([Id]) ON DELETE RESTRICT;
    ALTER TABLE [SubscriptionPlans] ADD CONSTRAINT [FK_SubscriptionPlans_Category_CategoryId]
        FOREIGN KEY ([CategoryId]) REFERENCES [Categories]([Id]) ON DELETE RESTRICT;
    ALTER TABLE [SubscriptionPlans] ADD CONSTRAINT [FK_SubscriptionPlans_ParentPlan_ParentPlanId]
        FOREIGN KEY ([ParentPlanId]) REFERENCES [SubscriptionPlans]([Id]) ON DELETE RESTRICT;
    
    -- Indexes
    CREATE INDEX [IX_SubscriptionPlans_Name] ON [SubscriptionPlans]([Name]);
    CREATE INDEX [IX_SubscriptionPlans_IsActive] ON [SubscriptionPlans]([IsActive]);
    CREATE INDEX [IX_SubscriptionPlans_IsFeatured] ON [SubscriptionPlans]([IsFeatured]);
    CREATE INDEX [IX_SubscriptionPlans_PlanType] ON [SubscriptionPlans]([PlanType]);
    CREATE INDEX [IX_SubscriptionPlans_CategoryId] ON [SubscriptionPlans]([CategoryId]);
    CREATE INDEX [IX_SubscriptionPlans_BillingCycleId] ON [SubscriptionPlans]([BillingCycleId]);
    CREATE INDEX [IX_SubscriptionPlans_CurrencyId] ON [SubscriptionPlans]([CurrencyId]);
    CREATE INDEX [IX_SubscriptionPlans_StripeProductId] ON [SubscriptionPlans]([StripeProductId]);
    CREATE INDEX [IX_SubscriptionPlans_IsLatestVersion] ON [SubscriptionPlans]([IsLatestVersion]);
    CREATE INDEX [IX_SubscriptionPlans_ParentPlanId] ON [SubscriptionPlans]([ParentPlanId]);
    CREATE INDEX [IX_SubscriptionPlans_ParentPlanId_VersionNumber] ON [SubscriptionPlans]([ParentPlanId], [VersionNumber]);
END
GO

-- 3.2 Privileges
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Privileges')
BEGIN
    CREATE TABLE [Privileges] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [Name] NVARCHAR(100) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [PrivilegeTypeId] UNIQUEIDENTIFIER NOT NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [CreatedBy] INT NULL,
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedBy] INT NULL,
        [UpdatedDate] DATETIME2 NULL,
        [DeletedBy] INT NULL,
        [DeletedDate] DATETIME2 NULL
    );
    
    ALTER TABLE [Privileges] ADD CONSTRAINT [FK_Privileges_MasterPrivilegeTypes_PrivilegeTypeId]
        FOREIGN KEY ([PrivilegeTypeId]) REFERENCES [MasterPrivilegeTypes]([Id]) ON DELETE RESTRICT;
    
    CREATE INDEX [IX_Privileges_Name] ON [Privileges]([Name]);
    CREATE INDEX [IX_Privileges_PrivilegeTypeId] ON [Privileges]([PrivilegeTypeId]);
    CREATE INDEX [IX_Privileges_IsActive] ON [Privileges]([IsActive]);
END
GO

-- 3.3 SubscriptionPlanPrivileges
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SubscriptionPlanPrivileges')
BEGIN
    CREATE TABLE [SubscriptionPlanPrivileges] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [SubscriptionPlanId] UNIQUEIDENTIFIER NOT NULL,
        [PrivilegeId] UNIQUEIDENTIFIER NOT NULL,
        [Value] INT NOT NULL,
        [DurationMonths] INT NOT NULL DEFAULT 1,
        [Description] NVARCHAR(500) NULL,
        [EffectiveDate] DATETIME2 NULL,
        [ExpirationDate] DATETIME2 NULL,
        [PrivilegeBaseCost] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [UnitCost] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [CreatedBy] INT NULL,
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedBy] INT NULL,
        [UpdatedDate] DATETIME2 NULL,
        [DeletedBy] INT NULL,
        [DeletedDate] DATETIME2 NULL
    );
    
    ALTER TABLE [SubscriptionPlanPrivileges] ADD CONSTRAINT [FK_SubscriptionPlanPrivileges_SubscriptionPlan_SubscriptionPlanId]
        FOREIGN KEY ([SubscriptionPlanId]) REFERENCES [SubscriptionPlans]([Id]) ON DELETE CASCADE;
    ALTER TABLE [SubscriptionPlanPrivileges] ADD CONSTRAINT [FK_SubscriptionPlanPrivileges_Privilege_PrivilegeId]
        FOREIGN KEY ([PrivilegeId]) REFERENCES [Privileges]([Id]) ON DELETE CASCADE;
    
    CREATE INDEX [IX_SubscriptionPlanPrivileges_SubscriptionPlanId] ON [SubscriptionPlanPrivileges]([SubscriptionPlanId]);
    CREATE INDEX [IX_SubscriptionPlanPrivileges_PrivilegeId] ON [SubscriptionPlanPrivileges]([PrivilegeId]);
    CREATE INDEX [IX_SubscriptionPlanPrivileges_EffectiveDate] ON [SubscriptionPlanPrivileges]([EffectiveDate]);
    CREATE INDEX [IX_SubscriptionPlanPrivileges_ExpirationDate] ON [SubscriptionPlanPrivileges]([ExpirationDate]);
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- SECTION 4: SUBSCRIPTION TABLES
-- ═══════════════════════════════════════════════════════════════════════════════

-- 4.1 Subscriptions
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Subscriptions')
BEGIN
    CREATE TABLE [Subscriptions] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [UserId] INT NOT NULL,
        [SubscriptionPlanId] UNIQUEIDENTIFIER NOT NULL,
        [BillingCycleId] UNIQUEIDENTIFIER NULL,
        [ProviderId] INT NULL,
        [Status] NVARCHAR(50) NOT NULL,
        [StatusReason] NVARCHAR(500) NULL,
        [StartDate] DATETIME2 NOT NULL,
        [EndDate] DATETIME2 NULL,
        [NextBillingDate] DATETIME2 NOT NULL,
        [CurrentPrice] DECIMAL(18,2) NOT NULL,
        [AutoRenew] BIT NOT NULL DEFAULT 1,
        [Notes] NVARCHAR(1000) NULL,
        [PausedDate] DATETIME2 NULL,
        [ResumedDate] DATETIME2 NULL,
        [CancelledDate] DATETIME2 NULL,
        [ExpirationDate] DATETIME2 NULL,
        [SuspendedDate] DATETIME2 NULL,
        [LastBillingDate] DATETIME2 NULL,
        [CancellationReason] NVARCHAR(500) NULL,
        [PauseReason] NVARCHAR(500) NULL,
        [StripeSubscriptionId] NVARCHAR(100) NULL,
        [StripeCustomerId] NVARCHAR(100) NULL,
        [StripePriceId] NVARCHAR(100) NULL,
        [PaymentMethodId] NVARCHAR(100) NULL,
        [LastPaymentDate] DATETIME2 NULL,
        [LastPaymentFailedDate] DATETIME2 NULL,
        [LastPaymentError] NVARCHAR(500) NULL,
        [FailedPaymentAttempts] INT NOT NULL DEFAULT 0,
        [IsTrialSubscription] BIT NOT NULL DEFAULT 0,
        [TrialStartDate] DATETIME2 NULL,
        [TrialEndDate] DATETIME2 NULL,
        [TrialDurationInDays] INT NOT NULL DEFAULT 0,
        [LastUsedDate] DATETIME2 NULL,
        [TotalUsageCount] INT NOT NULL DEFAULT 0,
        [HealthAssessmentId] UNIQUEIDENTIFIER NULL,
        [PendingCancellationAtRenewal] BIT NOT NULL DEFAULT 0,
        [PendingCancellationReason] NVARCHAR(500) NULL,
        [PendingChangeType] NVARCHAR(50) NULL,
        [PendingPlanChangeId] UNIQUEIDENTIFIER NULL,
        [PlanChangeEffectiveDate] DATETIME2 NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [CreatedBy] INT NULL,
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedBy] INT NULL,
        [UpdatedDate] DATETIME2 NULL,
        [DeletedBy] INT NULL,
        [DeletedDate] DATETIME2 NULL
    );
    
    ALTER TABLE [Subscriptions] ADD CONSTRAINT [FK_Subscriptions_SubscriptionPlan_SubscriptionPlanId]
        FOREIGN KEY ([SubscriptionPlanId]) REFERENCES [SubscriptionPlans]([Id]) ON DELETE RESTRICT;
    ALTER TABLE [Subscriptions] ADD CONSTRAINT [FK_Subscriptions_BillingCycle_BillingCycleId]
        FOREIGN KEY ([BillingCycleId]) REFERENCES [MasterBillingCycles]([Id]) ON DELETE RESTRICT;
    ALTER TABLE [Subscriptions] ADD CONSTRAINT [FK_Subscriptions_PendingPlan_PendingPlanChangeId]
        FOREIGN KEY ([PendingPlanChangeId]) REFERENCES [SubscriptionPlans]([Id]) ON DELETE RESTRICT;
    
    -- Foreign Key to existing User table
    ALTER TABLE [Subscriptions] ADD CONSTRAINT [FK_Subscriptions_Users_UserId]
        FOREIGN KEY ([UserId]) REFERENCES [dbo].[User]([UserID]) ON DELETE RESTRICT;
    
    CREATE INDEX [IX_Subscriptions_UserId] ON [Subscriptions]([UserId]);
    CREATE INDEX [IX_Subscriptions_SubscriptionPlanId] ON [Subscriptions]([SubscriptionPlanId]);
    CREATE INDEX [IX_Subscriptions_Status] ON [Subscriptions]([Status]);
    CREATE INDEX [IX_Subscriptions_StartDate] ON [Subscriptions]([StartDate]);
    CREATE INDEX [IX_Subscriptions_NextBillingDate] ON [Subscriptions]([NextBillingDate]);
    CREATE INDEX [IX_Subscriptions_StripeSubscriptionId] ON [Subscriptions]([StripeSubscriptionId]);
    CREATE INDEX [IX_Subscriptions_StripeCustomerId] ON [Subscriptions]([StripeCustomerId]);
    CREATE INDEX [IX_Subscriptions_ProviderId] ON [Subscriptions]([ProviderId]);
    CREATE INDEX [IX_Subscriptions_BillingCycleId] ON [Subscriptions]([BillingCycleId]);
    CREATE INDEX [IX_Subscriptions_IsTrialSubscription] ON [Subscriptions]([IsTrialSubscription]);
    CREATE INDEX [IX_Subscriptions_AutoRenew] ON [Subscriptions]([AutoRenew]);
    CREATE INDEX [IX_Subscriptions_PendingPlanChangeId] ON [Subscriptions]([PendingPlanChangeId]);
    
    CREATE UNIQUE INDEX [UK_User_Plan_Active] ON [Subscriptions]([UserId], [SubscriptionPlanId])
        WHERE [Status] IN ('Active', 'Paused');
END
GO

-- 4.2 SubscriptionStatusHistories
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SubscriptionStatusHistories')
BEGIN
    CREATE TABLE [SubscriptionStatusHistories] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [SubscriptionId] UNIQUEIDENTIFIER NOT NULL,
        [FromStatus] NVARCHAR(50) NULL,
        [ToStatus] NVARCHAR(50) NOT NULL,
        [ChangedAt] DATETIME2 NOT NULL,
        [ChangedByUserId] INT NULL,
        [Reason] NVARCHAR(500) NULL,
        [Metadata] NVARCHAR(1000) NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [CreatedBy] INT NULL,
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedBy] INT NULL,
        [UpdatedDate] DATETIME2 NULL,
        [DeletedBy] INT NULL,
        [DeletedDate] DATETIME2 NULL
    );
    
    ALTER TABLE [SubscriptionStatusHistories] ADD CONSTRAINT [FK_SubscriptionStatusHistories_Subscription_SubscriptionId]
        FOREIGN KEY ([SubscriptionId]) REFERENCES [Subscriptions]([Id]) ON DELETE CASCADE;
    ALTER TABLE [SubscriptionStatusHistories] ADD CONSTRAINT [FK_SubscriptionStatusHistories_Users_ChangedByUserId]
        FOREIGN KEY ([ChangedByUserId]) REFERENCES [dbo].[User]([UserID]) ON DELETE SET NULL;
    
    CREATE INDEX [IX_SubscriptionStatusHistories_SubscriptionId] ON [SubscriptionStatusHistories]([SubscriptionId]);
    CREATE INDEX [IX_SubscriptionStatusHistories_ChangedAt] ON [SubscriptionStatusHistories]([ChangedAt]);
    CREATE INDEX [IX_SubscriptionStatusHistories_ToStatus] ON [SubscriptionStatusHistories]([ToStatus]);
    CREATE INDEX [IX_SubscriptionStatusHistories_ChangedByUserId] ON [SubscriptionStatusHistories]([ChangedByUserId]);
END
GO

-- 4.3 UserSubscriptionPrivilegeUsages
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserSubscriptionPrivilegeUsages')
BEGIN
    CREATE TABLE [UserSubscriptionPrivilegeUsages] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [SubscriptionId] UNIQUEIDENTIFIER NOT NULL,
        [SubscriptionPlanPrivilegeId] UNIQUEIDENTIFIER NOT NULL,
        [PrivilegeId] UNIQUEIDENTIFIER NOT NULL,
        [UsedValue] INT NOT NULL,
        [AllowedValue] INT NOT NULL,
        [UsagePeriodStart] DATETIME2 NOT NULL,
        [UsagePeriodEnd] DATETIME2 NOT NULL,
        [LastUsedAt] DATETIME2 NULL,
        [ResetAt] DATETIME2 NULL,
        [Notes] NVARCHAR(500) NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [CreatedBy] INT NULL,
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedBy] INT NULL,
        [UpdatedDate] DATETIME2 NULL,
        [DeletedBy] INT NULL,
        [DeletedDate] DATETIME2 NULL
    );
    
    ALTER TABLE [UserSubscriptionPrivilegeUsages] ADD CONSTRAINT [FK_UserSubscriptionPrivilegeUsages_Subscription_SubscriptionId]
        FOREIGN KEY ([SubscriptionId]) REFERENCES [Subscriptions]([Id]) ON DELETE CASCADE;
    ALTER TABLE [UserSubscriptionPrivilegeUsages] ADD CONSTRAINT [FK_UserSubscriptionPrivilegeUsages_SubscriptionPlanPrivilege_SubscriptionPlanPrivilegeId]
        FOREIGN KEY ([SubscriptionPlanPrivilegeId]) REFERENCES [SubscriptionPlanPrivileges]([Id]) ON DELETE CASCADE;
    ALTER TABLE [UserSubscriptionPrivilegeUsages] ADD CONSTRAINT [FK_UserSubscriptionPrivilegeUsages_Privilege_PrivilegeId]
        FOREIGN KEY ([PrivilegeId]) REFERENCES [Privileges]([Id]) ON DELETE RESTRICT;
    
    CREATE INDEX [IX_UserSubscriptionPrivilegeUsages_SubscriptionId] ON [UserSubscriptionPrivilegeUsages]([SubscriptionId]);
    CREATE INDEX [IX_UserSubscriptionPrivilegeUsages_SubscriptionPlanPrivilegeId] ON [UserSubscriptionPrivilegeUsages]([SubscriptionPlanPrivilegeId]);
    CREATE INDEX [IX_UserSubscriptionPrivilegeUsages_PrivilegeId] ON [UserSubscriptionPrivilegeUsages]([PrivilegeId]);
    CREATE INDEX [IX_UserSubscriptionPrivilegeUsages_UsagePeriodStart] ON [UserSubscriptionPrivilegeUsages]([UsagePeriodStart]);
    CREATE INDEX [IX_UserSubscriptionPrivilegeUsages_UsagePeriodEnd] ON [UserSubscriptionPrivilegeUsages]([UsagePeriodEnd]);
    CREATE INDEX [IX_UserSubscriptionPrivilegeUsages_LastUsedAt] ON [UserSubscriptionPrivilegeUsages]([LastUsedAt]);
END
GO

-- 4.4 PrivilegeUsageHistories
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PrivilegeUsageHistories')
BEGIN
    CREATE TABLE [PrivilegeUsageHistories] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [UserSubscriptionPrivilegeUsageId] UNIQUEIDENTIFIER NOT NULL,
        [UsedValue] INT NOT NULL,
        [UsedAt] DATETIME2 NOT NULL,
        [UsageDate] DATETIME2 NOT NULL,
        [UsageWeek] NVARCHAR(10) NOT NULL,
        [UsageMonth] NVARCHAR(7) NOT NULL,
        [Notes] NVARCHAR(500) NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [CreatedBy] INT NULL,
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedBy] INT NULL,
        [UpdatedDate] DATETIME2 NULL,
        [DeletedBy] INT NULL,
        [DeletedDate] DATETIME2 NULL
    );
    
    ALTER TABLE [PrivilegeUsageHistories] ADD CONSTRAINT [FK_PrivilegeUsageHistories_UserSubscriptionPrivilegeUsage_UserSubscriptionPrivilegeUsageId]
        FOREIGN KEY ([UserSubscriptionPrivilegeUsageId]) REFERENCES [UserSubscriptionPrivilegeUsages]([Id]) ON DELETE CASCADE;
    
    CREATE INDEX [IX_PrivilegeUsageHistories_UserSubscriptionPrivilegeUsageId] ON [PrivilegeUsageHistories]([UserSubscriptionPrivilegeUsageId]);
    CREATE INDEX [IX_PrivilegeUsageHistories_UsageDate] ON [PrivilegeUsageHistories]([UsageDate]);
    CREATE INDEX [IX_PrivilegeUsageHistories_UsageWeek] ON [PrivilegeUsageHistories]([UsageWeek]);
    CREATE INDEX [IX_PrivilegeUsageHistories_UsageMonth] ON [PrivilegeUsageHistories]([UsageMonth]);
    CREATE INDEX [IX_PrivilegeUsageHistories_UserSubscriptionPrivilegeUsageId_UsageDate] ON [PrivilegeUsageHistories]([UserSubscriptionPrivilegeUsageId], [UsageDate]);
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- SECTION 5: BILLING TABLES
-- ═══════════════════════════════════════════════════════════════════════════════

-- 5.1 BillingRecords
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BillingRecords')
BEGIN
    CREATE TABLE [BillingRecords] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [UserId] INT NOT NULL,
        [SubscriptionId] UNIQUEIDENTIFIER NULL,
        [ConsultationId] UNIQUEIDENTIFIER NULL,
        [MedicationDeliveryId] UNIQUEIDENTIFIER NULL,
        [BillingCycleId] UNIQUEIDENTIFIER NULL,
        [CurrencyId] UNIQUEIDENTIFIER NOT NULL,
        [Status] NVARCHAR(50) NOT NULL,
        [Type] NVARCHAR(50) NOT NULL,
        [Amount] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [TaxAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [ShippingAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [TotalAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [BillingDate] DATETIME2 NOT NULL,
        [PaidAt] DATETIME2 NULL,
        [DueDate] DATETIME2 NULL,
        [InvoiceNumber] NVARCHAR(100) NULL,
        [StripePaymentIntentId] NVARCHAR(100) NULL,
        [StripeInvoiceId] NVARCHAR(100) NULL,
        [Description] NVARCHAR(500) NULL,
        [FailureReason] NVARCHAR(500) NULL,
        [PaymentMethod] NVARCHAR(100) NULL,
        [TransactionId] NVARCHAR(100) NULL,
        [ErrorMessage] NVARCHAR(500) NULL,
        [ProcessedAt] DATETIME2 NULL,
        [IsRecurring] BIT NOT NULL DEFAULT 0,
        [NextBillingDate] DATETIME2 NULL,
        [PaymentIntentId] NVARCHAR(100) NULL,
        [AccruedAmount] DECIMAL(18,2) NULL,
        [AccrualStartDate] DATETIME2 NULL,
        [AccrualEndDate] DATETIME2 NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [CreatedBy] INT NULL,
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedBy] INT NULL,
        [UpdatedDate] DATETIME2 NULL,
        [DeletedBy] INT NULL,
        [DeletedDate] DATETIME2 NULL
    );
    
    ALTER TABLE [BillingRecords] ADD CONSTRAINT [FK_BillingRecords_Subscription_SubscriptionId]
        FOREIGN KEY ([SubscriptionId]) REFERENCES [Subscriptions]([Id]) ON DELETE SET NULL;
    ALTER TABLE [BillingRecords] ADD CONSTRAINT [FK_BillingRecords_Currency_CurrencyId]
        FOREIGN KEY ([CurrencyId]) REFERENCES [MasterCurrencies]([Id]) ON DELETE RESTRICT;
    
    -- Foreign Key to existing User table
    ALTER TABLE [BillingRecords] ADD CONSTRAINT [FK_BillingRecords_Users_UserId]
        FOREIGN KEY ([UserId]) REFERENCES [dbo].[User]([UserID]) ON DELETE RESTRICT;
    
    CREATE INDEX [IX_BillingRecords_UserId] ON [BillingRecords]([UserId]);
    CREATE INDEX [IX_BillingRecords_SubscriptionId] ON [BillingRecords]([SubscriptionId]);
    CREATE INDEX [IX_BillingRecords_ConsultationId] ON [BillingRecords]([ConsultationId]);
    CREATE INDEX [IX_BillingRecords_MedicationDeliveryId] ON [BillingRecords]([MedicationDeliveryId]);
    CREATE INDEX [IX_BillingRecords_CurrencyId] ON [BillingRecords]([CurrencyId]);
    CREATE INDEX [IX_BillingRecords_BillingCycleId] ON [BillingRecords]([BillingCycleId]);
    CREATE INDEX [IX_BillingRecords_Status] ON [BillingRecords]([Status]);
    CREATE INDEX [IX_BillingRecords_Type] ON [BillingRecords]([Type]);
    CREATE INDEX [IX_BillingRecords_BillingDate] ON [BillingRecords]([BillingDate]);
    CREATE INDEX [IX_BillingRecords_DueDate] ON [BillingRecords]([DueDate]);
    CREATE INDEX [IX_BillingRecords_PaidAt] ON [BillingRecords]([PaidAt]);
    CREATE INDEX [IX_BillingRecords_IsRecurring] ON [BillingRecords]([IsRecurring]);
    CREATE INDEX [IX_BillingRecords_InvoiceNumber] ON [BillingRecords]([InvoiceNumber]);
    CREATE INDEX [IX_BillingRecords_StripePaymentIntentId] ON [BillingRecords]([StripePaymentIntentId]);
    CREATE INDEX [IX_BillingRecords_StripeInvoiceId] ON [BillingRecords]([StripeInvoiceId]);
    CREATE INDEX [IX_BillingRecords_PaymentIntentId] ON [BillingRecords]([PaymentIntentId]);
END
GO

-- 5.2 BillingAdjustments
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BillingAdjustments')
BEGIN
    CREATE TABLE [BillingAdjustments] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [BillingRecordId] UNIQUEIDENTIFIER NOT NULL,
        [Type] NVARCHAR(50) NOT NULL,
        [Amount] DECIMAL(18,2) NOT NULL,
        [Description] NVARCHAR(500) NOT NULL,
        [Reason] NVARCHAR(500) NULL,
        [IsPercentage] BIT NOT NULL DEFAULT 0,
        [Percentage] DECIMAL(5,2) NULL,
        [AppliedAt] DATETIME2 NOT NULL,
        [AppliedBy] INT NULL,
        [IsApproved] BIT NOT NULL DEFAULT 1,
        [ApprovalNotes] NVARCHAR(500) NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [CreatedBy] INT NULL,
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedBy] INT NULL,
        [UpdatedDate] DATETIME2 NULL,
        [DeletedBy] INT NULL,
        [DeletedDate] DATETIME2 NULL
    );
    
    ALTER TABLE [BillingAdjustments] ADD CONSTRAINT [FK_BillingAdjustments_BillingRecord_BillingRecordId]
        FOREIGN KEY ([BillingRecordId]) REFERENCES [BillingRecords]([Id]) ON DELETE CASCADE;
    
    CREATE INDEX [IX_BillingAdjustments_BillingRecordId] ON [BillingAdjustments]([BillingRecordId]);
    CREATE INDEX [IX_BillingAdjustments_Type] ON [BillingAdjustments]([Type]);
    CREATE INDEX [IX_BillingAdjustments_AppliedAt] ON [BillingAdjustments]([AppliedAt]);
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- SECTION 6: PAYMENT & REFUND TABLES
-- ═══════════════════════════════════════════════════════════════════════════════

-- 6.1 SubscriptionPayments
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SubscriptionPayments')
BEGIN
    CREATE TABLE [SubscriptionPayments] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [SubscriptionId] UNIQUEIDENTIFIER NOT NULL,
        [BillingRecordId] UNIQUEIDENTIFIER NOT NULL,
        [CurrencyId] UNIQUEIDENTIFIER NOT NULL,
        [Amount] DECIMAL(18,2) NOT NULL,
        [TaxAmount] DECIMAL(18,2) NOT NULL,
        [NetAmount] DECIMAL(18,2) NOT NULL,
        [Description] NVARCHAR(500) NOT NULL,
        [Status] NVARCHAR(50) NOT NULL,
        [Type] NVARCHAR(50) NOT NULL,
        [FailureReason] NVARCHAR(1000) NULL,
        [DueDate] DATETIME2 NOT NULL,
        [PaidAt] DATETIME2 NULL,
        [FailedAt] DATETIME2 NULL,
        [BillingPeriodStart] DATETIME2 NOT NULL,
        [BillingPeriodEnd] DATETIME2 NOT NULL,
        [StripePaymentIntentId] NVARCHAR(100) NULL,
        [StripeInvoiceId] NVARCHAR(100) NULL,
        [ReceiptUrl] NVARCHAR(500) NULL,
        [PaymentIntentId] NVARCHAR(100) NULL,
        [InvoiceId] NVARCHAR(100) NULL,
        [AttemptCount] INT NOT NULL DEFAULT 0,
        [NextRetryAt] DATETIME2 NULL,
        [RefundedAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [CreatedBy] INT NULL,
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedBy] INT NULL,
        [UpdatedDate] DATETIME2 NULL,
        [DeletedBy] INT NULL,
        [DeletedDate] DATETIME2 NULL
    );
    
    ALTER TABLE [SubscriptionPayments] ADD CONSTRAINT [FK_SubscriptionPayments_Subscription_SubscriptionId]
        FOREIGN KEY ([SubscriptionId]) REFERENCES [Subscriptions]([Id]) ON DELETE RESTRICT;
    ALTER TABLE [SubscriptionPayments] ADD CONSTRAINT [FK_SubscriptionPayments_BillingRecord_BillingRecordId]
        FOREIGN KEY ([BillingRecordId]) REFERENCES [BillingRecords]([Id]) ON DELETE RESTRICT;
    ALTER TABLE [SubscriptionPayments] ADD CONSTRAINT [FK_SubscriptionPayments_Currency_CurrencyId]
        FOREIGN KEY ([CurrencyId]) REFERENCES [MasterCurrencies]([Id]) ON DELETE RESTRICT;
    
    CREATE INDEX [IX_SubscriptionPayments_SubscriptionId] ON [SubscriptionPayments]([SubscriptionId]);
    CREATE INDEX [IX_SubscriptionPayments_Status] ON [SubscriptionPayments]([Status]);
    CREATE INDEX [IX_SubscriptionPayments_Type] ON [SubscriptionPayments]([Type]);
    CREATE INDEX [IX_SubscriptionPayments_DueDate] ON [SubscriptionPayments]([DueDate]);
    CREATE INDEX [IX_SubscriptionPayments_PaidAt] ON [SubscriptionPayments]([PaidAt]);
    CREATE INDEX [IX_SubscriptionPayments_StripePaymentIntentId] ON [SubscriptionPayments]([StripePaymentIntentId]);
    CREATE INDEX [IX_SubscriptionPayments_StripeInvoiceId] ON [SubscriptionPayments]([StripeInvoiceId]);
END
GO

-- 6.2 PaymentRefunds
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PaymentRefunds')
BEGIN
    CREATE TABLE [PaymentRefunds] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [SubscriptionPaymentId] UNIQUEIDENTIFIER NOT NULL,
        [Amount] DECIMAL(18,2) NOT NULL,
        [Reason] NVARCHAR(500) NOT NULL,
        [StripeRefundId] NVARCHAR(100) NULL,
        [RefundedAt] DATETIME2 NOT NULL,
        [ProcessedByUserId] INT NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [CreatedBy] INT NULL,
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedBy] INT NULL,
        [UpdatedDate] DATETIME2 NULL,
        [DeletedBy] INT NULL,
        [DeletedDate] DATETIME2 NULL
    );
    
    ALTER TABLE [PaymentRefunds] ADD CONSTRAINT [FK_PaymentRefunds_SubscriptionPayment_SubscriptionPaymentId]
        FOREIGN KEY ([SubscriptionPaymentId]) REFERENCES [SubscriptionPayments]([Id]) ON DELETE CASCADE;
    
    -- Foreign Key to existing User table
    ALTER TABLE [PaymentRefunds] ADD CONSTRAINT [FK_PaymentRefunds_Users_ProcessedByUserId]
        FOREIGN KEY ([ProcessedByUserId]) REFERENCES [dbo].[User]([UserID]) ON DELETE SET NULL;
    
    CREATE INDEX [IX_PaymentRefunds_SubscriptionPaymentId] ON [PaymentRefunds]([SubscriptionPaymentId]);
    CREATE INDEX [IX_PaymentRefunds_ProcessedByUserId] ON [PaymentRefunds]([ProcessedByUserId]);
    CREATE INDEX [IX_PaymentRefunds_RefundedAt] ON [PaymentRefunds]([RefundedAt]);
    CREATE INDEX [IX_PaymentRefunds_StripeRefundId] ON [PaymentRefunds]([StripeRefundId]);
END
GO

-- 6.3 FailedRefunds
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'FailedRefunds')
BEGIN
    CREATE TABLE [FailedRefunds] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [BillingRecordId] UNIQUEIDENTIFIER NOT NULL,
        [StripePaymentIntentId] NVARCHAR(255) NOT NULL,
        [StripeInvoiceId] NVARCHAR(255) NULL,
        [Amount] DECIMAL(18,2) NOT NULL,
        [UserId] INT NOT NULL,
        [ChargedAt] DATETIME2 NOT NULL,
        [DatabaseFailedAt] DATETIME2 NOT NULL,
        [FirstAttemptAt] DATETIME2 NOT NULL,
        [LastAttemptAt] DATETIME2 NULL,
        [RetryCount] INT NOT NULL DEFAULT 0,
        [MaxRetries] INT NOT NULL DEFAULT 5,
        [Status] INT NOT NULL DEFAULT 0,
        [LastErrorMessage] NVARCHAR(2000) NULL,
        [ErrorDetails] NVARCHAR(MAX) NULL,
        [DatabaseFailureReason] NVARCHAR(2000) NULL,
        [AdminNotified] BIT NOT NULL DEFAULT 0,
        [AdminNotifiedAt] DATETIME2 NULL,
        [ResolvedAt] DATETIME2 NULL,
        [ResolvedBy] INT NULL,
        [ResolutionNotes] NVARCHAR(2000) NULL,
        [Priority] NVARCHAR(20) NOT NULL DEFAULT 'High',
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] INT NOT NULL,
        [UpdatedDate] DATETIME2 NULL,
        [UpdatedBy] INT NULL
    );
    
    ALTER TABLE [FailedRefunds] ADD CONSTRAINT [FK_FailedRefunds_BillingRecord_BillingRecordId]
        FOREIGN KEY ([BillingRecordId]) REFERENCES [BillingRecords]([Id]) ON DELETE CASCADE;
    
    -- Foreign Key to existing User table
    ALTER TABLE [FailedRefunds] ADD CONSTRAINT [FK_FailedRefunds_Users_UserId]
        FOREIGN KEY ([UserId]) REFERENCES [dbo].[User]([UserID]) ON DELETE RESTRICT;
    
    CREATE INDEX [IX_FailedRefunds_BillingRecordId] ON [FailedRefunds]([BillingRecordId]);
    CREATE INDEX [IX_FailedRefunds_UserId] ON [FailedRefunds]([UserId]);
    CREATE INDEX [IX_FailedRefunds_Status] ON [FailedRefunds]([Status]);
    CREATE INDEX [IX_FailedRefunds_RetryCount] ON [FailedRefunds]([RetryCount]);
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- SECTION 7: VERSIONING & MIGRATION TABLES
-- ═══════════════════════════════════════════════════════════════════════════════

-- 7.1 ScheduledPlanMigrations
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ScheduledPlanMigrations')
BEGIN
    CREATE TABLE [ScheduledPlanMigrations] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [SubscriptionId] UNIQUEIDENTIFIER NOT NULL,
        [FromPlanId] UNIQUEIDENTIFIER NOT NULL,
        [ToPlanId] UNIQUEIDENTIFIER NOT NULL,
        [NotificationDate] DATETIME2 NOT NULL,
        [ScheduledMigrationDate] DATETIME2 NOT NULL,
        [Status] NVARCHAR(50) NOT NULL DEFAULT 'Pending',
        [UserDecision] NVARCHAR(50) NULL,
        [UserDecisionDate] DATETIME2 NULL,
        [CompletedDate] DATETIME2 NULL,
        [Notes] NVARCHAR(500) NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [CreatedBy] INT NULL,
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedBy] INT NULL,
        [UpdatedDate] DATETIME2 NULL,
        [DeletedBy] INT NULL,
        [DeletedDate] DATETIME2 NULL
    );
    
    ALTER TABLE [ScheduledPlanMigrations] ADD CONSTRAINT [FK_ScheduledPlanMigrations_Subscription_SubscriptionId]
        FOREIGN KEY ([SubscriptionId]) REFERENCES [Subscriptions]([Id]) ON DELETE RESTRICT;
    ALTER TABLE [ScheduledPlanMigrations] ADD CONSTRAINT [FK_ScheduledPlanMigrations_FromPlan_FromPlanId]
        FOREIGN KEY ([FromPlanId]) REFERENCES [SubscriptionPlans]([Id]) ON DELETE RESTRICT;
    ALTER TABLE [ScheduledPlanMigrations] ADD CONSTRAINT [FK_ScheduledPlanMigrations_ToPlan_ToPlanId]
        FOREIGN KEY ([ToPlanId]) REFERENCES [SubscriptionPlans]([Id]) ON DELETE RESTRICT;
    
    CREATE INDEX [IX_ScheduledPlanMigrations_SubscriptionId] ON [ScheduledPlanMigrations]([SubscriptionId]);
    CREATE INDEX [IX_ScheduledPlanMigrations_FromPlanId] ON [ScheduledPlanMigrations]([FromPlanId]);
    CREATE INDEX [IX_ScheduledPlanMigrations_ToPlanId] ON [ScheduledPlanMigrations]([ToPlanId]);
    CREATE INDEX [IX_ScheduledPlanMigrations_Status] ON [ScheduledPlanMigrations]([Status]);
    CREATE INDEX [IX_ScheduledPlanMigrations_ScheduledMigrationDate] ON [ScheduledPlanMigrations]([ScheduledMigrationDate]);
    CREATE INDEX [IX_ScheduledPlanMigrations_Status_ScheduledMigrationDate] ON [ScheduledPlanMigrations]([Status], [ScheduledMigrationDate]);
    CREATE INDEX [IX_ScheduledPlanMigrations_CreatedBy] ON [ScheduledPlanMigrations]([CreatedBy]);
    CREATE INDEX [IX_ScheduledPlanMigrations_UpdatedBy] ON [ScheduledPlanMigrations]([UpdatedBy]);
    CREATE INDEX [IX_ScheduledPlanMigrations_DeletedBy] ON [ScheduledPlanMigrations]([DeletedBy]);
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- SECTION 8: WEBHOOK & SYNC TABLES
-- ═══════════════════════════════════════════════════════════════════════════════

-- 8.1 ProcessedWebhookEvents
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ProcessedWebhookEvents')
BEGIN
    CREATE TABLE [ProcessedWebhookEvents] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [StripeEventId] NVARCHAR(255) NOT NULL,
        [EventType] NVARCHAR(100) NOT NULL,
        [ReceivedAt] DATETIME2 NOT NULL,
        [ProcessedAt] DATETIME2 NULL,
        [IsSuccess] BIT NOT NULL DEFAULT 0,
        [ErrorMessage] NVARCHAR(2000) NULL,
        [RetryCount] INT NOT NULL DEFAULT 0,
        [MaxRetries] INT NOT NULL DEFAULT 3,
        [LastAttemptAt] DATETIME2 NULL,
        [Metadata] NVARCHAR(4000) NULL,
        [ProcessingDurationMs] BIGINT NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [CreatedBy] INT NULL,
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedBy] INT NULL,
        [UpdatedDate] DATETIME2 NULL,
        [DeletedBy] INT NULL,
        [DeletedDate] DATETIME2 NULL
    );
    
    CREATE UNIQUE INDEX [IX_ProcessedWebhookEvents_StripeEventId] ON [ProcessedWebhookEvents]([StripeEventId]) WHERE [IsDeleted] = 0;
    CREATE INDEX [IX_ProcessedWebhookEvents_EventType] ON [ProcessedWebhookEvents]([EventType]);
    CREATE INDEX [IX_ProcessedWebhookEvents_ReceivedAt] ON [ProcessedWebhookEvents]([ReceivedAt]);
    CREATE INDEX [IX_ProcessedWebhookEvents_ProcessedAt] ON [ProcessedWebhookEvents]([ProcessedAt]);
    CREATE INDEX [IX_ProcessedWebhookEvents_IsSuccess] ON [ProcessedWebhookEvents]([IsSuccess]);
    CREATE INDEX [IX_ProcessedWebhookEvents_EventType_IsSuccess] ON [ProcessedWebhookEvents]([EventType], [IsSuccess]);
END
GO

-- 8.2 UnprocessedWebhookEvents
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UnprocessedWebhookEvents')
BEGIN
    CREATE TABLE [UnprocessedWebhookEvents] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [StripeEventId] NVARCHAR(255) NOT NULL,
        [EventType] NVARCHAR(100) NOT NULL,
        [EventData] NVARCHAR(MAX) NOT NULL,
        [StripeSubscriptionId] NVARCHAR(255) NULL,
        [StripeInvoiceId] NVARCHAR(255) NULL,
        [StripeCustomerId] NVARCHAR(255) NULL,
        [FailureReason] NVARCHAR(500) NOT NULL,
        [RetryCount] INT NOT NULL DEFAULT 0,
        [MaxRetries] INT NOT NULL DEFAULT 48,
        [NextRetryAt] DATETIME2 NOT NULL,
        [Status] INT NOT NULL DEFAULT 0,
        [ReceivedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [LastProcessedAt] DATETIME2 NULL,
        [LastError] NVARCHAR(1000) NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [CreatedBy] INT NULL,
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedBy] INT NULL,
        [UpdatedDate] DATETIME2 NULL,
        [DeletedBy] INT NULL,
        [DeletedDate] DATETIME2 NULL
    );
    
    CREATE INDEX [IX_UnprocessedWebhookEvents_StripeEventId] ON [UnprocessedWebhookEvents]([StripeEventId]);
    CREATE INDEX [IX_UnprocessedWebhookEvents_EventType] ON [UnprocessedWebhookEvents]([EventType]);
    CREATE INDEX [IX_UnprocessedWebhookEvents_Status] ON [UnprocessedWebhookEvents]([Status]);
    CREATE INDEX [IX_UnprocessedWebhookEvents_NextRetryAt] ON [UnprocessedWebhookEvents]([NextRetryAt]);
    CREATE INDEX [IX_UnprocessedWebhookEvents_ReceivedAt] ON [UnprocessedWebhookEvents]([ReceivedAt]);
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- SECTION 9: ADD MISSING CONSTRAINTS
-- ═══════════════════════════════════════════════════════════════════════════════

-- Foreign Key to existing User table for BillingAdjustments.AppliedBy
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'User' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    ALTER TABLE [BillingAdjustments] ADD CONSTRAINT [FK_BillingAdjustments_Users_AppliedBy]
        FOREIGN KEY ([AppliedBy]) REFERENCES [dbo].[User]([UserID]) ON DELETE RESTRICT;
END
GO

PRINT '✅ All subscription management tables created successfully!'
PRINT '✅ All Foreign Keys to User table have been configured.'
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- SCRIPT COMPLETE
-- ═══════════════════════════════════════════════════════════════════════════════

