-- =====================================================
-- SUBSCRIPTION MANAGEMENT DATABASE SCHEMA
-- Complete SQL Server DDL for Subscription Management System
-- Generated for SmartTelehealth Subscription Management Extraction
-- =====================================================

-- =====================================================
-- SUBSCRIPTION MANAGEMENT DATABASE SCHEMA
-- Complete SQL Server DDL for Subscription Management System
-- Generated for SmartTelehealth Subscription Management Extraction
-- =====================================================

-- =====================================================
-- IMPORTANT PREREQUISITES
-- =====================================================

-- 1. This script assumes a User table already exists in your database
--    with the following structure:
--    CREATE TABLE [User] (
--        [UserID] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
--        [Email] nvarchar(255) NOT NULL,
--        [FirstName] nvarchar(100) NULL,
--        [LastName] nvarchar(100) NULL,
--        -- Add other user fields as needed
--    );
--
-- 2. All audit fields (CreatedBy, UpdatedBy, DeletedBy) reference dbo.User.UserID

-- =====================================================
-- MASTER TABLES (Reference Data)
-- =====================================================

-- User Roles Master Table - NOT NEEDED FOR SUBSCRIPTION MANAGEMENT
-- (Used only in UserService for user management, not subscription-related)

-- Master Billing Cycles
CREATE TABLE [MasterBillingCycles] (
    [Id] uniqueidentifier NOT NULL DEFAULT NEWID(),
    [Name] nvarchar(50) NOT NULL,
    [Description] nvarchar(200) NULL,
    [DurationInDays] int NOT NULL,
    [IsActive] bit NOT NULL DEFAULT 1,
    [SortOrder] int NOT NULL DEFAULT 0,
    [CreatedDate] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedDate] datetime2 NULL,
    [IsDeleted] bit NOT NULL DEFAULT 0,
    [CreatedBy] int NOT NULL DEFAULT 0,
    [UpdatedBy] int NULL,
    [DeletedBy] int NULL,
    [DeletedDate] datetime2 NULL,
    CONSTRAINT [PK_MasterBillingCycles] PRIMARY KEY ([Id])
);

-- Master Currencies
CREATE TABLE [MasterCurrencies] (
    [Id] uniqueidentifier NOT NULL DEFAULT NEWID(),
    [Code] nvarchar(10) NOT NULL,
    [Name] nvarchar(50) NOT NULL,
    [Symbol] nvarchar(10) NULL,
    [IsActive] bit NOT NULL DEFAULT 1,
    [SortOrder] int NOT NULL DEFAULT 0,
    [CreatedDate] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedDate] datetime2 NULL,
    [IsDeleted] bit NOT NULL DEFAULT 0,
    [CreatedBy] int NOT NULL DEFAULT 0,
    [UpdatedBy] int NULL,
    [DeletedBy] int NULL,
    [DeletedDate] datetime2 NULL,
    CONSTRAINT [PK_MasterCurrencies] PRIMARY KEY ([Id])
);

-- Master Privilege Types
CREATE TABLE [MasterPrivilegeTypes] (
    [Id] uniqueidentifier NOT NULL DEFAULT NEWID(),
    [Name] nvarchar(50) NOT NULL,
    [Description] nvarchar(200) NULL,
    [IsActive] bit NOT NULL DEFAULT 1,
    [SortOrder] int NOT NULL DEFAULT 0,
    [CreatedDate] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedDate] datetime2 NULL,
    [IsDeleted] bit NOT NULL DEFAULT 0,
    [CreatedBy] int NOT NULL DEFAULT 0,
    [UpdatedBy] int NULL,
    [DeletedBy] int NULL,
    [DeletedDate] datetime2 NULL,
    CONSTRAINT [PK_MasterPrivilegeTypes] PRIMARY KEY ([Id])
);

-- =====================================================
-- CORE SUBSCRIPTION ENTITIES
-- =====================================================

-- Categories
CREATE TABLE [Categories] (
    [Id] uniqueidentifier NOT NULL DEFAULT NEWID(),
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(1000) NULL,
    [Icon] nvarchar(100) NULL,
    [Color] nvarchar(50) NULL,
    [IsActive] bit NOT NULL DEFAULT 1,
    [DisplayOrder] int NOT NULL DEFAULT 0,
    [Features] nvarchar(1000) NULL,
    [ConsultationDescription] nvarchar(500) NULL,
    [BasePrice] decimal(18,2) NULL,
    [ConsultationFee] decimal(18,2) NULL,
    [ConsultationDurationMinutes] int NULL,
    [RequiresHealthAssessment] bit NOT NULL DEFAULT 1,
    [AllowsMedicationDelivery] bit NOT NULL DEFAULT 1,
    [AllowsFollowUpMessaging] bit NOT NULL DEFAULT 1,
    [AllowsOneTimeConsultation] bit NOT NULL DEFAULT 1,
    [OneTimeConsultationFee] decimal(18,2) NULL,
    [OneTimeConsultationDurationMinutes] int NULL,
    [IsMostPopular] bit NOT NULL DEFAULT 0,
    [IsTrending] bit NOT NULL DEFAULT 0,
    [CreatedDate] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedDate] datetime2 NULL,
    [IsDeleted] bit NOT NULL DEFAULT 0,
    [CreatedBy] int NOT NULL DEFAULT 0,
    [UpdatedBy] int NULL,
    [DeletedBy] int NULL,
    [DeletedDate] datetime2 NULL,
    CONSTRAINT [PK_Categories] PRIMARY KEY ([Id])
);

-- Subscription Plans
CREATE TABLE [SubscriptionPlans] (
    [Id] uniqueidentifier NOT NULL DEFAULT NEWID(),
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(1000) NULL,
    [ShortDescription] nvarchar(200) NULL,
    [IsFeatured] bit NOT NULL DEFAULT 0,
    [IsTrialAllowed] bit NOT NULL DEFAULT 0,
    [TrialDurationInDays] int NOT NULL DEFAULT 0,
    [IsMostPopular] bit NOT NULL DEFAULT 0,
    [IsTrending] bit NOT NULL DEFAULT 0,
    [DisplayOrder] int NOT NULL DEFAULT 0,
    [Price] decimal(18,2) NOT NULL,
    [DiscountedPrice] decimal(18,2) NULL,
    [DiscountValidUntil] datetime2 NULL,
    [BillingCycleId] uniqueidentifier NOT NULL,
    [CurrencyId] uniqueidentifier NOT NULL,
    [CategoryId] uniqueidentifier NULL,
    [StripeProductId] nvarchar(100) NULL,
    [StripeMonthlyPriceId] nvarchar(100) NULL,
    [StripeQuarterlyPriceId] nvarchar(100) NULL,
    [StripeAnnualPriceId] nvarchar(100) NULL,
    [MessagingCount] int NOT NULL DEFAULT 10,
    [IncludesMedicationDelivery] bit NOT NULL DEFAULT 1,
    [IncludesFollowUpCare] bit NOT NULL DEFAULT 1,
    [DeliveryFrequencyDays] int NOT NULL DEFAULT 30,
    [MaxPauseDurationDays] int NOT NULL DEFAULT 90,
    [Features] nvarchar(1000) NULL,
    [Terms] nvarchar(500) NULL,
    [EffectiveDate] datetime2 NULL,
    [ExpirationDate] datetime2 NULL,
    [IsActive] bit NOT NULL DEFAULT 1,
    [CreatedDate] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedDate] datetime2 NULL,
    [IsDeleted] bit NOT NULL DEFAULT 0,
    [CreatedBy] int NOT NULL DEFAULT 0,
    [UpdatedBy] int NULL,
    [DeletedBy] int NULL,
    [DeletedDate] datetime2 NULL,
    CONSTRAINT [PK_SubscriptionPlans] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SubscriptionPlans_MasterBillingCycles_BillingCycleId] FOREIGN KEY ([BillingCycleId]) REFERENCES [MasterBillingCycles] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_SubscriptionPlans_MasterCurrencies_CurrencyId] FOREIGN KEY ([CurrencyId]) REFERENCES [MasterCurrencies] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_SubscriptionPlans_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE NO ACTION
);

-- Subscriptions
CREATE TABLE [Subscriptions] (
    [Id] uniqueidentifier NOT NULL DEFAULT NEWID(),
    [UserId] int NOT NULL,
    [SubscriptionPlanId] uniqueidentifier NOT NULL,
    [BillingCycleId] uniqueidentifier NOT NULL,
    [ProviderId] int NULL,
    [Status] nvarchar(50) NOT NULL DEFAULT 'Pending',
    [StatusReason] nvarchar(500) NULL,
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NULL,
    [NextBillingDate] datetime2 NOT NULL,
    [CurrentPrice] decimal(18,2) NOT NULL,
    [AutoRenew] bit NOT NULL DEFAULT 1,
    [Notes] nvarchar(1000) NULL,
    [PausedDate] datetime2 NULL,
    [ResumedDate] datetime2 NULL,
    [CancelledDate] datetime2 NULL,
    [ExpirationDate] datetime2 NULL,
    [SuspendedDate] datetime2 NULL,
    [LastBillingDate] datetime2 NULL,
    [CancellationReason] nvarchar(500) NULL,
    [PauseReason] nvarchar(500) NULL,
    [StripeSubscriptionId] nvarchar(100) NULL,
    [StripeCustomerId] nvarchar(100) NULL,
    [StripePriceId] nvarchar(100) NULL,
    [PaymentMethodId] nvarchar(100) NULL,
    [LastPaymentDate] datetime2 NULL,
    [LastPaymentFailedDate] datetime2 NULL,
    [LastPaymentError] nvarchar(500) NULL,
    [FailedPaymentAttempts] int NOT NULL DEFAULT 0,
    [IsTrialSubscription] bit NOT NULL DEFAULT 0,
    [TrialStartDate] datetime2 NULL,
    [TrialEndDate] datetime2 NULL,
    [TrialDurationInDays] int NOT NULL DEFAULT 0,
    [LastUsedDate] datetime2 NULL,
    [TotalUsageCount] int NOT NULL DEFAULT 0,
    [IsActive] bit NOT NULL DEFAULT 1,
    [CreatedDate] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedDate] datetime2 NULL,
    [IsDeleted] bit NOT NULL DEFAULT 0,
    [CreatedBy] int NOT NULL DEFAULT 0,
    [UpdatedBy] int NULL,
    [DeletedBy] int NULL,
    [DeletedDate] datetime2 NULL,
    CONSTRAINT [PK_Subscriptions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Subscriptions_SubscriptionPlans_SubscriptionPlanId] FOREIGN KEY ([SubscriptionPlanId]) REFERENCES [SubscriptionPlans] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Subscriptions_MasterBillingCycles_BillingCycleId] FOREIGN KEY ([BillingCycleId]) REFERENCES [MasterBillingCycles] ([Id]) ON DELETE NO ACTION
);

-- Privileges
CREATE TABLE [Privileges] (
    [Id] uniqueidentifier NOT NULL DEFAULT NEWID(),
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NULL,
    [PrivilegeTypeId] uniqueidentifier NOT NULL,
    [IsActive] bit NOT NULL DEFAULT 1,
    [CreatedDate] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedDate] datetime2 NULL,
    [IsDeleted] bit NOT NULL DEFAULT 0,
    [CreatedBy] int NOT NULL DEFAULT 0,
    [UpdatedBy] int NULL,
    [DeletedBy] int NULL,
    [DeletedDate] datetime2 NULL,
    CONSTRAINT [PK_Privileges] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Privileges_MasterPrivilegeTypes_PrivilegeTypeId] FOREIGN KEY ([PrivilegeTypeId]) REFERENCES [MasterPrivilegeTypes] ([Id]) ON DELETE NO ACTION
);

-- Subscription Plan Privileges (Many-to-Many)
CREATE TABLE [SubscriptionPlanPrivileges] (
    [Id] uniqueidentifier NOT NULL DEFAULT NEWID(),
    [SubscriptionPlanId] uniqueidentifier NOT NULL,
    [PrivilegeId] uniqueidentifier NOT NULL,
    [Value] int NOT NULL DEFAULT 0,
    [IsUnlimited] bit NOT NULL DEFAULT 0,
    [DailyLimit] int NULL,
    [WeeklyLimit] int NULL,
    [MonthlyLimit] int NULL,
    [IsActive] bit NOT NULL DEFAULT 1,
    [CreatedDate] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedDate] datetime2 NULL,
    [IsDeleted] bit NOT NULL DEFAULT 0,
    [CreatedBy] int NOT NULL DEFAULT 0,
    [UpdatedBy] int NULL,
    [DeletedBy] int NULL,
    [DeletedDate] datetime2 NULL,
    CONSTRAINT [PK_SubscriptionPlanPrivileges] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SubscriptionPlanPrivileges_SubscriptionPlans_SubscriptionPlanId] FOREIGN KEY ([SubscriptionPlanId]) REFERENCES [SubscriptionPlans] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_SubscriptionPlanPrivileges_Privileges_PrivilegeId] FOREIGN KEY ([PrivilegeId]) REFERENCES [Privileges] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [UQ_SubscriptionPlanPrivileges_Plan_Privilege] UNIQUE ([SubscriptionPlanId], [PrivilegeId])
);

-- User Subscription Privilege Usage
CREATE TABLE [UserSubscriptionPrivilegeUsages] (
    [Id] uniqueidentifier NOT NULL DEFAULT NEWID(),
    [SubscriptionId] uniqueidentifier NOT NULL,
    [SubscriptionPlanPrivilegeId] uniqueidentifier NOT NULL,
    [UsedValue] int NOT NULL DEFAULT 0,
    [AllowedValue] int NOT NULL DEFAULT 0,
    [UsagePeriodStart] datetime2 NOT NULL,
    [UsagePeriodEnd] datetime2 NOT NULL,
    [LastUsedAt] datetime2 NOT NULL,
    [IsActive] bit NOT NULL DEFAULT 1,
    [CreatedDate] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedDate] datetime2 NULL,
    [IsDeleted] bit NOT NULL DEFAULT 0,
    [CreatedBy] int NOT NULL DEFAULT 0,
    [UpdatedBy] int NULL,
    [DeletedBy] int NULL,
    [DeletedDate] datetime2 NULL,
    CONSTRAINT [PK_UserSubscriptionPrivilegeUsages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_UserSubscriptionPrivilegeUsages_Subscriptions_SubscriptionId] FOREIGN KEY ([SubscriptionId]) REFERENCES [Subscriptions] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserSubscriptionPrivilegeUsages_SubscriptionPlanPrivileges_SubscriptionPlanPrivilegeId] FOREIGN KEY ([SubscriptionPlanPrivilegeId]) REFERENCES [SubscriptionPlanPrivileges] ([Id]) ON DELETE CASCADE
);

-- Privilege Usage History
CREATE TABLE [PrivilegeUsageHistories] (
    [Id] uniqueidentifier NOT NULL DEFAULT NEWID(),
    [UserSubscriptionPrivilegeUsageId] uniqueidentifier NOT NULL,
    [UsedValue] int NOT NULL,
    [UsedAt] datetime2 NOT NULL,
    [UsageDate] date NOT NULL,
    [UsageWeek] nvarchar(10) NOT NULL,
    [UsageMonth] nvarchar(7) NOT NULL,
    [Notes] nvarchar(500) NULL,
    [CreatedDate] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedDate] datetime2 NULL,
    [IsDeleted] bit NOT NULL DEFAULT 0,
    [CreatedBy] int NOT NULL DEFAULT 0,
    [UpdatedBy] int NULL,
    [DeletedBy] int NULL,
    [DeletedDate] datetime2 NULL,
    CONSTRAINT [PK_PrivilegeUsageHistories] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PrivilegeUsageHistories_UserSubscriptionPrivilegeUsages_UserSubscriptionPrivilegeUsageId] FOREIGN KEY ([UserSubscriptionPrivilegeUsageId]) REFERENCES [UserSubscriptionPrivilegeUsages] ([Id]) ON DELETE CASCADE
);

-- =====================================================
-- BILLING AND PAYMENT ENTITIES
-- =====================================================

-- Billing Records
CREATE TABLE [BillingRecords] (
    [Id] uniqueidentifier NOT NULL DEFAULT NEWID(),
    [UserId] int NOT NULL,
    [SubscriptionId] uniqueidentifier NULL,
    [ConsultationId] uniqueidentifier NULL,
    [MedicationDeliveryId] uniqueidentifier NULL,
    [Amount] decimal(18,2) NOT NULL,
    [TaxAmount] decimal(18,2) NULL,
    [ShippingAmount] decimal(18,2) NULL,
    [TotalAmount] decimal(18,2) NOT NULL,
    [Description] nvarchar(500) NULL,
    [DueDate] datetime2 NOT NULL,
    [Type] nvarchar(50) NOT NULL,
    [Status] nvarchar(50) NOT NULL DEFAULT 'Pending',
    [StripeInvoiceId] nvarchar(100) NULL,
    [StripePaymentIntentId] nvarchar(100) NULL,
    [BillingDate] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [PaidAt] datetime2 NULL,
    [InvoiceNumber] nvarchar(100) NULL,
    [FailureReason] nvarchar(500) NULL,
    [PaymentMethod] nvarchar(100) NULL,
    [IsRecurring] bit NOT NULL DEFAULT 0,
    [PaymentIntentId] nvarchar(100) NULL,
    [AccruedAmount] decimal(18,2) NULL,
    [CurrencyId] uniqueidentifier NULL,
    [IsActive] bit NOT NULL DEFAULT 1,
    [CreatedDate] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedDate] datetime2 NULL,
    [IsDeleted] bit NOT NULL DEFAULT 0,
    [CreatedBy] int NOT NULL DEFAULT 0,
    [UpdatedBy] int NULL,
    [DeletedBy] int NULL,
    [DeletedDate] datetime2 NULL,
    CONSTRAINT [PK_BillingRecords] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_BillingRecords_Subscriptions_SubscriptionId] FOREIGN KEY ([SubscriptionId]) REFERENCES [Subscriptions] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_BillingRecords_MasterCurrencies_CurrencyId] FOREIGN KEY ([CurrencyId]) REFERENCES [MasterCurrencies] ([Id]) ON DELETE NO ACTION
);

-- Billing Adjustments
CREATE TABLE [BillingAdjustments] (
    [Id] uniqueidentifier NOT NULL DEFAULT NEWID(),
    [BillingRecordId] uniqueidentifier NOT NULL,
    [Type] nvarchar(50) NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Percentage] decimal(5,2) NULL,
    [Description] nvarchar(500) NOT NULL,
    [IsPercentage] bit NOT NULL DEFAULT 0,
    [IsApproved] bit NOT NULL DEFAULT 1,
    [CreatedDate] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedDate] datetime2 NULL,
    [IsDeleted] bit NOT NULL DEFAULT 0,
    [CreatedBy] int NOT NULL DEFAULT 0,
    [UpdatedBy] int NULL,
    [DeletedBy] int NULL,
    [DeletedDate] datetime2 NULL,
    CONSTRAINT [PK_BillingAdjustments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_BillingAdjustments_BillingRecords_BillingRecordId] FOREIGN KEY ([BillingRecordId]) REFERENCES [BillingRecords] ([Id]) ON DELETE CASCADE
);

-- Subscription Payments
CREATE TABLE [SubscriptionPayments] (
    [Id] uniqueidentifier NOT NULL DEFAULT NEWID(),
    [SubscriptionId] uniqueidentifier NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [TaxAmount] decimal(18,2) NULL,
    [NetAmount] decimal(18,2) NOT NULL,
    [Description] nvarchar(500) NULL,
    [Status] nvarchar(50) NOT NULL,
    [Type] nvarchar(50) NOT NULL,
    [FailureReason] nvarchar(500) NULL,
    [DueDate] datetime2 NOT NULL,
    [PaidAt] datetime2 NULL,
    [FailedAt] datetime2 NULL,
    [BillingPeriodStart] datetime2 NOT NULL,
    [BillingPeriodEnd] datetime2 NOT NULL,
    [StripePaymentIntentId] nvarchar(100) NULL,
    [StripeInvoiceId] nvarchar(100) NULL,
    [ReceiptUrl] nvarchar(500) NULL,
    [PaymentIntentId] nvarchar(100) NULL,
    [InvoiceId] nvarchar(100) NULL,
    [AttemptCount] int NOT NULL DEFAULT 0,
    [NextRetryAt] datetime2 NULL,
    [RefundedAmount] decimal(18,2) NOT NULL DEFAULT 0,
    [IsActive] bit NOT NULL DEFAULT 1,
    [CreatedDate] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedDate] datetime2 NULL,
    [IsDeleted] bit NOT NULL DEFAULT 0,
    [CreatedBy] int NOT NULL DEFAULT 0,
    [UpdatedBy] int NULL,
    [DeletedBy] int NULL,
    [DeletedDate] datetime2 NULL,
    CONSTRAINT [PK_SubscriptionPayments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SubscriptionPayments_Subscriptions_SubscriptionId] FOREIGN KEY ([SubscriptionId]) REFERENCES [Subscriptions] ([Id]) ON DELETE CASCADE
);

-- Payment Refunds
CREATE TABLE [PaymentRefunds] (
    [Id] uniqueidentifier NOT NULL DEFAULT NEWID(),
    [SubscriptionPaymentId] uniqueidentifier NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Reason] nvarchar(500) NOT NULL,
    [StripeRefundId] nvarchar(100) NULL,
    [RefundedAt] datetime2 NOT NULL,
    [ProcessedByUserId] int NULL,
    [IsActive] bit NOT NULL DEFAULT 1,
    [CreatedDate] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedDate] datetime2 NULL,
    [IsDeleted] bit NOT NULL DEFAULT 0,
    [CreatedBy] int NOT NULL DEFAULT 0,
    [UpdatedBy] int NULL,
    [DeletedBy] int NULL,
    [DeletedDate] datetime2 NULL,
    CONSTRAINT [PK_PaymentRefunds] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PaymentRefunds_SubscriptionPayments_SubscriptionPaymentId] FOREIGN KEY ([SubscriptionPaymentId]) REFERENCES [SubscriptionPayments] ([Id]) ON DELETE CASCADE
);

-- =====================================================
-- AUDIT AND HISTORY ENTITIES
-- =====================================================

-- Subscription Status History
CREATE TABLE [SubscriptionStatusHistories] (
    [Id] uniqueidentifier NOT NULL DEFAULT NEWID(),
    [SubscriptionId] uniqueidentifier NOT NULL,
    [FromStatus] nvarchar(50) NULL,
    [ToStatus] nvarchar(50) NOT NULL,
    [Reason] nvarchar(500) NULL,
    [ChangedByUserId] nvarchar(100) NULL,
    [ChangedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [Metadata] nvarchar(1000) NULL,
    [IsActive] bit NOT NULL DEFAULT 1,
    [CreatedDate] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedDate] datetime2 NULL,
    [IsDeleted] bit NOT NULL DEFAULT 0,
    [CreatedBy] int NOT NULL DEFAULT 0,
    [UpdatedBy] int NULL,
    [DeletedBy] int NULL,
    [DeletedDate] datetime2 NULL,
    CONSTRAINT [PK_SubscriptionStatusHistories] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SubscriptionStatusHistories_Subscriptions_SubscriptionId] FOREIGN KEY ([SubscriptionId]) REFERENCES [Subscriptions] ([Id]) ON DELETE CASCADE
);

-- Audit Logs
CREATE TABLE [AuditLogs] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [UserId] int NULL,
    [Type] nvarchar(50) NOT NULL,
    [TableName] nvarchar(100) NOT NULL,
    [DateTime] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [OldValues] nvarchar(max) NULL,
    [NewValues] nvarchar(max) NULL,
    [AffectedColumns] nvarchar(2000) NULL,
    [PrimaryKey] nvarchar(50) NULL,
    [OrganizationId] int NULL,
    CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id])
);

-- =====================================================
-- CATEGORY FEE RANGES
-- =====================================================

-- Category Fee Ranges - NOT NEEDED FOR SUBSCRIPTION MANAGEMENT
-- (Used only in ProviderFeeService, not subscription-related)

-- =====================================================
-- INDEXES FOR PERFORMANCE
-- =====================================================

-- Subscription indexes
CREATE INDEX [IX_Subscriptions_UserId] ON [Subscriptions] ([UserId]);
CREATE INDEX [IX_Subscriptions_SubscriptionPlanId] ON [Subscriptions] ([SubscriptionPlanId]);
CREATE INDEX [IX_Subscriptions_Status] ON [Subscriptions] ([Status]);
CREATE INDEX [IX_Subscriptions_NextBillingDate] ON [Subscriptions] ([NextBillingDate]);
CREATE INDEX [IX_Subscriptions_StripeSubscriptionId] ON [Subscriptions] ([StripeSubscriptionId]);

-- Subscription Plan indexes
CREATE INDEX [IX_SubscriptionPlans_BillingCycleId] ON [SubscriptionPlans] ([BillingCycleId]);
CREATE INDEX [IX_SubscriptionPlans_CurrencyId] ON [SubscriptionPlans] ([CurrencyId]);
CREATE INDEX [IX_SubscriptionPlans_CategoryId] ON [SubscriptionPlans] ([CategoryId]);
CREATE INDEX [IX_SubscriptionPlans_IsActive] ON [SubscriptionPlans] ([IsActive]);

-- Billing Record indexes
CREATE INDEX [IX_BillingRecords_UserId] ON [BillingRecords] ([UserId]);
CREATE INDEX [IX_BillingRecords_SubscriptionId] ON [BillingRecords] ([SubscriptionId]);
CREATE INDEX [IX_BillingRecords_Status] ON [BillingRecords] ([Status]);
CREATE INDEX [IX_BillingRecords_DueDate] ON [BillingRecords] ([DueDate]);

-- Privilege Usage indexes
CREATE INDEX [IX_UserSubscriptionPrivilegeUsages_SubscriptionId] ON [UserSubscriptionPrivilegeUsages] ([SubscriptionId]);
CREATE INDEX [IX_UserSubscriptionPrivilegeUsages_SubscriptionPlanPrivilegeId] ON [UserSubscriptionPrivilegeUsages] ([SubscriptionPlanPrivilegeId]);

-- Status History indexes
CREATE INDEX [IX_SubscriptionStatusHistories_SubscriptionId] ON [SubscriptionStatusHistories] ([SubscriptionId]);
CREATE INDEX [IX_SubscriptionStatusHistories_ChangedAt] ON [SubscriptionStatusHistories] ([ChangedAt]);

-- Audit Log indexes
CREATE INDEX [IX_AuditLogs_UserId] ON [AuditLogs] ([UserId]);
CREATE INDEX [IX_AuditLogs_TableName] ON [AuditLogs] ([TableName]);
CREATE INDEX [IX_AuditLogs_DateTime] ON [AuditLogs] ([DateTime]);

-- =====================================================
-- CHECK CONSTRAINTS
-- =====================================================

-- Subscription status constraint
ALTER TABLE [Subscriptions] ADD CONSTRAINT [CK_Subscriptions_Status] 
CHECK ([Status] IN ('Pending', 'Active', 'Paused', 'Cancelled', 'Expired', 'PaymentFailed', 'TrialActive', 'TrialExpired', 'Suspended'));

-- Billing record status constraint
ALTER TABLE [BillingRecords] ADD CONSTRAINT [CK_BillingRecords_Status] 
CHECK ([Status] IN ('Pending', 'Paid', 'Failed', 'Cancelled', 'Refunded'));

-- Billing record type constraint
ALTER TABLE [BillingRecords] ADD CONSTRAINT [CK_BillingRecords_Type] 
CHECK ([Type] IN ('Subscription', 'Consultation', 'Medication', 'OneTime', 'Refund'));

-- Subscription payment status constraint
ALTER TABLE [SubscriptionPayments] ADD CONSTRAINT [CK_SubscriptionPayments_Status] 
CHECK ([Status] IN ('Pending', 'Paid', 'Failed', 'Cancelled', 'Refunded'));

-- Subscription payment type constraint
ALTER TABLE [SubscriptionPayments] ADD CONSTRAINT [CK_SubscriptionPayments_Type] 
CHECK ([Type] IN ('Subscription', 'Trial', 'Upgrade', 'Downgrade', 'Refund'));

-- Positive amount constraints
ALTER TABLE [Subscriptions] ADD CONSTRAINT [CK_Subscriptions_CurrentPrice_Positive] 
CHECK ([CurrentPrice] >= 0);

ALTER TABLE [SubscriptionPlans] ADD CONSTRAINT [CK_SubscriptionPlans_Price_Positive] 
CHECK ([Price] >= 0);

ALTER TABLE [BillingRecords] ADD CONSTRAINT [CK_BillingRecords_Amount_Positive] 
CHECK ([Amount] >= 0);

ALTER TABLE [SubscriptionPayments] ADD CONSTRAINT [CK_SubscriptionPayments_Amount_Positive] 
CHECK ([Amount] >= 0);

-- =====================================================
-- AUDIT FIELD FOREIGN KEY CONSTRAINTS
-- =====================================================

-- Categories audit constraints
ALTER TABLE [Categories] WITH CHECK ADD CONSTRAINT [FK_Categories_User_CreatedBy] FOREIGN KEY([CreatedBy]) REFERENCES [dbo].[User] ([UserID]);
ALTER TABLE [Categories] CHECK CONSTRAINT [FK_Categories_User_CreatedBy];

ALTER TABLE [Categories] WITH CHECK ADD CONSTRAINT [FK_Categories_User_UpdatedBy] FOREIGN KEY([UpdatedBy]) REFERENCES [dbo].[User] ([UserID]);
ALTER TABLE [Categories] CHECK CONSTRAINT [FK_Categories_User_UpdatedBy];

ALTER TABLE [Categories] WITH CHECK ADD CONSTRAINT [FK_Categories_User_DeletedBy] FOREIGN KEY([DeletedBy]) REFERENCES [dbo].[User] ([UserID]);
ALTER TABLE [Categories] CHECK CONSTRAINT [FK_Categories_User_DeletedBy];

-- SubscriptionPlans audit constraints
ALTER TABLE [SubscriptionPlans] WITH CHECK ADD CONSTRAINT [FK_SubscriptionPlans_User_CreatedBy] FOREIGN KEY([CreatedBy]) REFERENCES [dbo].[User] ([UserID]);
ALTER TABLE [SubscriptionPlans] CHECK CONSTRAINT [FK_SubscriptionPlans_User_CreatedBy];

ALTER TABLE [SubscriptionPlans] WITH CHECK ADD CONSTRAINT [FK_SubscriptionPlans_User_UpdatedBy] FOREIGN KEY([UpdatedBy]) REFERENCES [dbo].[User] ([UserID]);
ALTER TABLE [SubscriptionPlans] CHECK CONSTRAINT [FK_SubscriptionPlans_User_UpdatedBy];

ALTER TABLE [SubscriptionPlans] WITH CHECK ADD CONSTRAINT [FK_SubscriptionPlans_User_DeletedBy] FOREIGN KEY([DeletedBy]) REFERENCES [dbo].[User] ([UserID]);
ALTER TABLE [SubscriptionPlans] CHECK CONSTRAINT [FK_SubscriptionPlans_User_DeletedBy];

-- Subscriptions audit constraints
ALTER TABLE [Subscriptions] WITH CHECK ADD CONSTRAINT [FK_Subscriptions_User_CreatedBy] FOREIGN KEY([CreatedBy]) REFERENCES [dbo].[User] ([UserID]);
ALTER TABLE [Subscriptions] CHECK CONSTRAINT [FK_Subscriptions_User_CreatedBy];

ALTER TABLE [Subscriptions] WITH CHECK ADD CONSTRAINT [FK_Subscriptions_User_UpdatedBy] FOREIGN KEY([UpdatedBy]) REFERENCES [dbo].[User] ([UserID]);
ALTER TABLE [Subscriptions] CHECK CONSTRAINT [FK_Subscriptions_User_UpdatedBy];

ALTER TABLE [Subscriptions] WITH CHECK ADD CONSTRAINT [FK_Subscriptions_User_DeletedBy] FOREIGN KEY([DeletedBy]) REFERENCES [dbo].[User] ([UserID]);
ALTER TABLE [Subscriptions] CHECK CONSTRAINT [FK_Subscriptions_User_DeletedBy];

-- Privileges audit constraints
ALTER TABLE [Privileges] WITH CHECK ADD CONSTRAINT [FK_Privileges_User_CreatedBy] FOREIGN KEY([CreatedBy]) REFERENCES [dbo].[User] ([UserID]);
ALTER TABLE [Privileges] CHECK CONSTRAINT [FK_Privileges_User_CreatedBy];

ALTER TABLE [Privileges] WITH CHECK ADD CONSTRAINT [FK_Privileges_User_UpdatedBy] FOREIGN KEY([UpdatedBy]) REFERENCES [dbo].[User] ([UserID]);
ALTER TABLE [Privileges] CHECK CONSTRAINT [FK_Privileges_User_UpdatedBy];

ALTER TABLE [Privileges] WITH CHECK ADD CONSTRAINT [FK_Privileges_User_DeletedBy] FOREIGN KEY([DeletedBy]) REFERENCES [dbo].[User] ([UserID]);
ALTER TABLE [Privileges] CHECK CONSTRAINT [FK_Privileges_User_DeletedBy];

-- SubscriptionPlanPrivileges audit constraints
ALTER TABLE [SubscriptionPlanPrivileges] WITH CHECK ADD CONSTRAINT [FK_SubscriptionPlanPrivileges_User_CreatedBy] FOREIGN KEY([CreatedBy]) REFERENCES [dbo].[User] ([UserID]);
ALTER TABLE [SubscriptionPlanPrivileges] CHECK CONSTRAINT [FK_SubscriptionPlanPrivileges_User_CreatedBy];

ALTER TABLE [SubscriptionPlanPrivileges] WITH CHECK ADD CONSTRAINT [FK_SubscriptionPlanPrivileges_User_UpdatedBy] FOREIGN KEY([UpdatedBy]) REFERENCES [dbo].[User] ([UserID]);
ALTER TABLE [SubscriptionPlanPrivileges] CHECK CONSTRAINT [FK_SubscriptionPlanPrivileges_User_UpdatedBy];

ALTER TABLE [SubscriptionPlanPrivileges] WITH CHECK ADD CONSTRAINT [FK_SubscriptionPlanPrivileges_User_DeletedBy] FOREIGN KEY([DeletedBy]) REFERENCES [dbo].[User] ([UserID]);
ALTER TABLE [SubscriptionPlanPrivileges] CHECK CONSTRAINT [FK_SubscriptionPlanPrivileges_User_DeletedBy];

-- UserSubscriptionPrivilegeUsages audit constraints
ALTER TABLE [UserSubscriptionPrivilegeUsages] WITH CHECK ADD CONSTRAINT [FK_UserSubscriptionPrivilegeUsages_User_CreatedBy] FOREIGN KEY([CreatedBy]) REFERENCES [dbo].[User] ([UserID]);
ALTER TABLE [UserSubscriptionPrivilegeUsages] CHECK CONSTRAINT [FK_UserSubscriptionPrivilegeUsages_User_CreatedBy];

ALTER TABLE [UserSubscriptionPrivilegeUsages] WITH CHECK ADD CONSTRAINT [FK_UserSubscriptionPrivilegeUsages_User_UpdatedBy] FOREIGN KEY([UpdatedBy]) REFERENCES [dbo].[User] ([UserID]);
ALTER TABLE [UserSubscriptionPrivilegeUsages] CHECK CONSTRAINT [FK_UserSubscriptionPrivilegeUsages_User_UpdatedBy];

ALTER TABLE [UserSubscriptionPrivilegeUsages] WITH CHECK ADD CONSTRAINT [FK_UserSubscriptionPrivilegeUsages_User_DeletedBy] FOREIGN KEY([DeletedBy]) REFERENCES [dbo].[User] ([UserID]);
ALTER TABLE [UserSubscriptionPrivilegeUsages] CHECK CONSTRAINT [FK_UserSubscriptionPrivilegeUsages_User_DeletedBy];

-- PrivilegeUsageHistories audit constraints
ALTER TABLE [PrivilegeUsageHistories] WITH CHECK ADD CONSTRAINT [FK_PrivilegeUsageHistories_User_CreatedBy] FOREIGN KEY([CreatedBy]) REFERENCES [dbo].[User] ([UserID]);
ALTER TABLE [PrivilegeUsageHistories] CHECK CONSTRAINT [FK_PrivilegeUsageHistories_User_CreatedBy];

ALTER TABLE [PrivilegeUsageHistories] WITH CHECK ADD CONSTRAINT [FK_PrivilegeUsageHistories_User_UpdatedBy] FOREIGN KEY([UpdatedBy]) REFERENCES [dbo].[User] ([UserID]);
ALTER TABLE [PrivilegeUsageHistories] CHECK CONSTRAINT [FK_PrivilegeUsageHistories_User_UpdatedBy];

ALTER TABLE [PrivilegeUsageHistories] WITH CHECK ADD CONSTRAINT [FK_PrivilegeUsageHistories_User_DeletedBy] FOREIGN KEY([DeletedBy]) REFERENCES [dbo].[User] ([UserID]);
ALTER TABLE [PrivilegeUsageHistories] CHECK CONSTRAINT [FK_PrivilegeUsageHistories_User_DeletedBy];

-- BillingRecords audit constraints
ALTER TABLE [BillingRecords] WITH CHECK ADD CONSTRAINT [FK_BillingRecords_User_CreatedBy] FOREIGN KEY([CreatedBy]) REFERENCES [dbo].[User] ([UserID]);
ALTER TABLE [BillingRecords] CHECK CONSTRAINT [FK_BillingRecords_User_CreatedBy];

ALTER TABLE [BillingRecords] WITH CHECK ADD CONSTRAINT [FK_BillingRecords_User_UpdatedBy] FOREIGN KEY([UpdatedBy]) REFERENCES [dbo].[User] ([UserID]);
ALTER TABLE [BillingRecords] CHECK CONSTRAINT [FK_BillingRecords_User_UpdatedBy];

ALTER TABLE [BillingRecords] WITH CHECK ADD CONSTRAINT [FK_BillingRecords_User_DeletedBy] FOREIGN KEY([DeletedBy]) REFERENCES [dbo].[User] ([UserID]);
ALTER TABLE [BillingRecords] CHECK CONSTRAINT [FK_BillingRecords_User_DeletedBy];

-- BillingAdjustments audit constraints
ALTER TABLE [BillingAdjustments] WITH CHECK ADD CONSTRAINT [FK_BillingAdjustments_User_CreatedBy] FOREIGN KEY([CreatedBy]) REFERENCES [dbo].[User] ([UserID]);
ALTER TABLE [BillingAdjustments] CHECK CONSTRAINT [FK_BillingAdjustments_User_CreatedBy];

ALTER TABLE [BillingAdjustments] WITH CHECK ADD CONSTRAINT [FK_BillingAdjustments_User_UpdatedBy] FOREIGN KEY([UpdatedBy]) REFERENCES [dbo].[User] ([UserID]);
ALTER TABLE [BillingAdjustments] CHECK CONSTRAINT [FK_BillingAdjustments_User_UpdatedBy];

ALTER TABLE [BillingAdjustments] WITH CHECK ADD CONSTRAINT [FK_BillingAdjustments_User_DeletedBy] FOREIGN KEY([DeletedBy]) REFERENCES [dbo].[User] ([UserID]);
ALTER TABLE [BillingAdjustments] CHECK CONSTRAINT [FK_BillingAdjustments_User_DeletedBy];

-- SubscriptionPayments audit constraints
ALTER TABLE [SubscriptionPayments] WITH CHECK ADD CONSTRAINT [FK_SubscriptionPayments_User_CreatedBy] FOREIGN KEY([CreatedBy]) REFERENCES [dbo].[User] ([UserID]);
ALTER TABLE [SubscriptionPayments] CHECK CONSTRAINT [FK_SubscriptionPayments_User_CreatedBy];

ALTER TABLE [SubscriptionPayments] WITH CHECK ADD CONSTRAINT [FK_SubscriptionPayments_User_UpdatedBy] FOREIGN KEY([UpdatedBy]) REFERENCES [dbo].[User] ([UserID]);
ALTER TABLE [SubscriptionPayments] CHECK CONSTRAINT [FK_SubscriptionPayments_User_UpdatedBy];

ALTER TABLE [SubscriptionPayments] WITH CHECK ADD CONSTRAINT [FK_SubscriptionPayments_User_DeletedBy] FOREIGN KEY([DeletedBy]) REFERENCES [dbo].[User] ([UserID]);
ALTER TABLE [SubscriptionPayments] CHECK CONSTRAINT [FK_SubscriptionPayments_User_DeletedBy];

-- PaymentRefunds audit constraints
ALTER TABLE [PaymentRefunds] WITH CHECK ADD CONSTRAINT [FK_PaymentRefunds_User_CreatedBy] FOREIGN KEY([CreatedBy]) REFERENCES [dbo].[User] ([UserID]);
ALTER TABLE [PaymentRefunds] CHECK CONSTRAINT [FK_PaymentRefunds_User_CreatedBy];

ALTER TABLE [PaymentRefunds] WITH CHECK ADD CONSTRAINT [FK_PaymentRefunds_User_UpdatedBy] FOREIGN KEY([UpdatedBy]) REFERENCES [dbo].[User] ([UserID]);
ALTER TABLE [PaymentRefunds] CHECK CONSTRAINT [FK_PaymentRefunds_User_UpdatedBy];

ALTER TABLE [PaymentRefunds] WITH CHECK ADD CONSTRAINT [FK_PaymentRefunds_User_DeletedBy] FOREIGN KEY([DeletedBy]) REFERENCES [dbo].[User] ([UserID]);
ALTER TABLE [PaymentRefunds] CHECK CONSTRAINT [FK_PaymentRefunds_User_DeletedBy];

-- SubscriptionStatusHistories audit constraints
ALTER TABLE [SubscriptionStatusHistories] WITH CHECK ADD CONSTRAINT [FK_SubscriptionStatusHistories_User_CreatedBy] FOREIGN KEY([CreatedBy]) REFERENCES [dbo].[User] ([UserID]);
ALTER TABLE [SubscriptionStatusHistories] CHECK CONSTRAINT [FK_SubscriptionStatusHistories_User_CreatedBy];

ALTER TABLE [SubscriptionStatusHistories] WITH CHECK ADD CONSTRAINT [FK_SubscriptionStatusHistories_User_UpdatedBy] FOREIGN KEY([UpdatedBy]) REFERENCES [dbo].[User] ([UserID]);
ALTER TABLE [SubscriptionStatusHistories] CHECK CONSTRAINT [FK_SubscriptionStatusHistories_User_UpdatedBy];

ALTER TABLE [SubscriptionStatusHistories] WITH CHECK ADD CONSTRAINT [FK_SubscriptionStatusHistories_User_DeletedBy] FOREIGN KEY([DeletedBy]) REFERENCES [dbo].[User] ([UserID]);
ALTER TABLE [SubscriptionStatusHistories] CHECK CONSTRAINT [FK_SubscriptionStatusHistories_User_DeletedBy];

-- =====================================================
-- SCRIPT COMPLETION
-- =====================================================

-- All foreign key constraints have been created
-- SQL Server automatically enforces foreign key constraints

-- =====================================================
-- SEED DATA SCRIPT
-- =====================================================

-- Insert Master Billing Cycles
INSERT INTO [MasterBillingCycles] ([Id], [Name], [Description], [DurationInDays], [SortOrder], [IsActive])
VALUES 
    (NEWID(), 'Monthly', 'Monthly billing cycle', 30, 1, 1),
    (NEWID(), 'Quarterly', 'Quarterly billing cycle', 90, 2, 1),
    (NEWID(), 'Annual', 'Annual billing cycle', 365, 3, 1);

-- Insert Master Currencies
INSERT INTO [MasterCurrencies] ([Id], [Code], [Name], [Symbol], [SortOrder], [IsActive])
VALUES 
    (NEWID(), 'USD', 'US Dollar', '$', 1, 1),
    (NEWID(), 'EUR', 'Euro', '€', 2, 1),
    (NEWID(), 'GBP', 'British Pound', '£', 3, 1);

-- Insert Master Privilege Types
INSERT INTO [MasterPrivilegeTypes] ([Id], [Name], [Description], [SortOrder], [IsActive])
VALUES 
    (NEWID(), 'Consultation', 'Medical consultation privilege', 1, 1),
    (NEWID(), 'Messaging', 'Messaging privilege', 2, 1),
    (NEWID(), 'Video Call', 'Video call privilege', 3, 1),
    (NEWID(), 'Document', 'Document access privilege', 4, 1),
    (NEWID(), 'Prescription', 'Prescription privilege', 5, 1);

-- Insert Sample Categories
INSERT INTO [Categories] ([Id], [Name], [Description], [BasePrice], [ConsultationFee], [OneTimeConsultationFee], [IsActive], [RequiresHealthAssessment], [AllowsMedicationDelivery], [AllowsFollowUpMessaging])
VALUES 
    (NEWID(), 'Primary Care', 'General health consultations', 100.00, 100.00, 150.00, 1, 1, 1, 1),
    (NEWID(), 'Mental Health', 'Mental health and therapy services', 150.00, 150.00, 200.00, 1, 1, 1, 1),
    (NEWID(), 'Dermatology', 'Skin and dermatological consultations', 120.00, 120.00, 180.00, 1, 0, 1, 1);

-- Insert Sample Privileges
INSERT INTO [Privileges] ([Id], [Name], [Description], [PrivilegeTypeId], [IsActive])
SELECT 
    NEWID(), 'Video Consultation', 'Access to video consultation services', pt.Id, 1
FROM [MasterPrivilegeTypes] pt WHERE pt.Name = 'Consultation'
UNION ALL
SELECT 
    NEWID(), 'Messaging', 'Access to messaging services', pt.Id, 1
FROM [MasterPrivilegeTypes] pt WHERE pt.Name = 'Messaging'
UNION ALL
SELECT 
    NEWID(), 'Document Access', 'Access to medical documents', pt.Id, 1
FROM [MasterPrivilegeTypes] pt WHERE pt.Name = 'Document'
UNION ALL
SELECT 
    NEWID(), 'Prescription', 'Access to prescription services', pt.Id, 1
FROM [MasterPrivilegeTypes] pt WHERE pt.Name = 'Prescription';

-- =====================================================
-- COMPLETION MESSAGE
-- =====================================================

PRINT '=====================================================';
PRINT 'SUBSCRIPTION MANAGEMENT DATABASE SCHEMA CREATED';
PRINT '=====================================================';
PRINT 'Tables created:';
PRINT '- MasterBillingCycles';
PRINT '- MasterCurrencies';
PRINT '- MasterPrivilegeTypes';
PRINT '- Categories';
PRINT '- SubscriptionPlans';
PRINT '- Subscriptions';
PRINT '- Privileges';
PRINT '- SubscriptionPlanPrivileges';
PRINT '- UserSubscriptionPrivilegeUsages';
PRINT '- PrivilegeUsageHistories';
PRINT '- BillingRecords';
PRINT '- BillingAdjustments';
PRINT '- SubscriptionPayments';
PRINT '- PaymentRefunds';
PRINT '- SubscriptionStatusHistories';
PRINT '- AuditLogs';
PRINT '=====================================================';
PRINT 'Schema ready for subscription management system!';
PRINT '=====================================================';
