-- =====================================================
-- COMPREHENSIVE SUBSCRIPTION MANAGEMENT TABLES - PART 2
-- =====================================================
-- This script creates the remaining subscription-related tables
-- =====================================================

-- =====================================================
-- 7. SUBSCRIPTION AND BILLING TABLES
-- =====================================================

-- Subscription Table
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Subscriptions]') AND type in (N'U'))
    DROP TABLE [dbo].[Subscriptions]
GO

CREATE TABLE [dbo].[Subscriptions](
    [Id] [uniqueidentifier] NOT NULL,
    [UserId] [int] NOT NULL,
    [SubscriptionPlanId] [uniqueidentifier] NOT NULL,
    [BillingCycleId] [uniqueidentifier] NOT NULL,
    [ProviderId] [int] NULL,
    [Status] [nvarchar](50) NOT NULL,
    [StatusReason] [nvarchar](500) NULL,
    [StartDate] [datetime2](7) NOT NULL,
    [EndDate] [datetime2](7) NULL,
    [NextBillingDate] [datetime2](7) NOT NULL,
    [CurrentPrice] [decimal](18, 2) NOT NULL,
    [AutoRenew] [bit] NOT NULL,
    [Notes] [nvarchar](1000) NULL,
    [PausedDate] [datetime2](7) NULL,
    [ResumedDate] [datetime2](7) NULL,
    [CancelledDate] [datetime2](7) NULL,
    [ExpirationDate] [datetime2](7) NULL,
    [SuspendedDate] [datetime2](7) NULL,
    [LastBillingDate] [datetime2](7) NULL,
    [CancellationReason] [nvarchar](500) NULL,
    [PauseReason] [nvarchar](500) NULL,
    [StripeSubscriptionId] [nvarchar](100) NULL,
    [StripeCustomerId] [nvarchar](100) NULL,
    [StripePriceId] [nvarchar](100) NULL,
    [PaymentMethodId] [nvarchar](100) NULL,
    [LastPaymentDate] [datetime2](7) NULL,
    [LastPaymentFailedDate] [datetime2](7) NULL,
    [LastPaymentError] [nvarchar](500) NULL,
    [FailedPaymentAttempts] [int] NOT NULL,
    [IsTrialSubscription] [bit] NOT NULL,
    [TrialStartDate] [datetime2](7) NULL,
    [TrialEndDate] [datetime2](7) NULL,
    [TrialDurationInDays] [int] NOT NULL,
    [LastUsedDate] [datetime2](7) NULL,
    [TotalUsageCount] [int] NOT NULL,
    [IsActive] [bit] NOT NULL,
    [IsDeleted] [bit] NOT NULL,
    [CreatedBy] [int] NULL,
    [CreatedDate] [datetime2](7) NULL,
    [UpdatedBy] [int] NULL,
    [UpdatedDate] [datetime2](7) NULL,
    [DeletedBy] [int] NULL,
    [DeletedDate] [datetime2](7) NULL,
    
    CONSTRAINT [PK_Subscriptions] PRIMARY KEY CLUSTERED ([Id] ASC)
) ON [PRIMARY]
GO

-- SubscriptionPayment Table
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SubscriptionPayments]') AND type in (N'U'))
    DROP TABLE [dbo].[SubscriptionPayments]
GO

CREATE TABLE [dbo].[SubscriptionPayments](
    [Id] [uniqueidentifier] NOT NULL,
    [SubscriptionId] [uniqueidentifier] NOT NULL,
    [CurrencyId] [uniqueidentifier] NOT NULL,
    [Amount] [decimal](18, 2) NOT NULL,
    [TaxAmount] [decimal](18, 2) NOT NULL,
    [NetAmount] [decimal](18, 2) NOT NULL,
    [Description] [nvarchar](500) NOT NULL,
    [Status] [int] NOT NULL,
    [Type] [int] NOT NULL,
    [FailureReason] [nvarchar](1000) NULL,
    [DueDate] [datetime2](7) NOT NULL,
    [PaidAt] [datetime2](7) NULL,
    [FailedAt] [datetime2](7) NULL,
    [BillingPeriodStart] [datetime2](7) NOT NULL,
    [BillingPeriodEnd] [datetime2](7) NOT NULL,
    [StripePaymentIntentId] [nvarchar](100) NULL,
    [StripeInvoiceId] [nvarchar](100) NULL,
    [ReceiptUrl] [nvarchar](500) NULL,
    [PaymentIntentId] [nvarchar](100) NULL,
    [InvoiceId] [nvarchar](100) NULL,
    [AttemptCount] [int] NOT NULL,
    [NextRetryAt] [datetime2](7) NULL,
    [RefundedAmount] [decimal](18, 2) NOT NULL,
    [IsActive] [bit] NOT NULL,
    [IsDeleted] [bit] NOT NULL,
    [CreatedBy] [int] NULL,
    [CreatedDate] [datetime2](7) NULL,
    [UpdatedBy] [int] NULL,
    [UpdatedDate] [datetime2](7) NULL,
    [DeletedBy] [int] NULL,
    [DeletedDate] [datetime2](7) NULL,
    
    CONSTRAINT [PK_SubscriptionPayments] PRIMARY KEY CLUSTERED ([Id] ASC)
) ON [PRIMARY]
GO

-- BillingRecord Table
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BillingRecords]') AND type in (N'U'))
    DROP TABLE [dbo].[BillingRecords]
GO

CREATE TABLE [dbo].[BillingRecords](
    [Id] [uniqueidentifier] NOT NULL,
    [UserId] [int] NOT NULL,
    [SubscriptionId] [uniqueidentifier] NULL,
    [ConsultationId] [uniqueidentifier] NULL,
    [MedicationDeliveryId] [uniqueidentifier] NULL,
    [BillingCycleId] [uniqueidentifier] NULL,
    [CurrencyId] [uniqueidentifier] NOT NULL,
    [Status] [int] NOT NULL,
    [Type] [int] NOT NULL,
    [Amount] [decimal](18, 2) NOT NULL,
    [TaxAmount] [decimal](18, 2) NOT NULL,
    [ShippingAmount] [decimal](18, 2) NOT NULL,
    [TotalAmount] [decimal](18, 2) NOT NULL,
    [BillingDate] [datetime2](7) NOT NULL,
    [PaidAt] [datetime2](7) NULL,
    [DueDate] [datetime2](7) NULL,
    [InvoiceNumber] [nvarchar](100) NULL,
    [StripePaymentIntentId] [nvarchar](100) NULL,
    [StripeInvoiceId] [nvarchar](100) NULL,
    [Description] [nvarchar](500) NULL,
    [FailureReason] [nvarchar](500) NULL,
    [PaymentMethod] [nvarchar](100) NULL,
    [TransactionId] [nvarchar](100) NULL,
    [ErrorMessage] [nvarchar](500) NULL,
    [ProcessedAt] [datetime2](7) NULL,
    [IsRecurring] [bit] NOT NULL,
    [NextBillingDate] [datetime2](7) NULL,
    [PaymentIntentId] [nvarchar](100) NULL,
    [AccruedAmount] [decimal](18, 2) NULL,
    [AccrualStartDate] [datetime2](7) NULL,
    [AccrualEndDate] [datetime2](7) NULL,
    [IsActive] [bit] NOT NULL,
    [IsDeleted] [bit] NOT NULL,
    [CreatedBy] [int] NULL,
    [CreatedDate] [datetime2](7) NULL,
    [UpdatedBy] [int] NULL,
    [UpdatedDate] [datetime2](7) NULL,
    [DeletedBy] [int] NULL,
    [DeletedDate] [datetime2](7) NULL,
    
    CONSTRAINT [PK_BillingRecords] PRIMARY KEY CLUSTERED ([Id] ASC)
) ON [PRIMARY]
GO

-- BillingAdjustment Table
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BillingAdjustments]') AND type in (N'U'))
    DROP TABLE [dbo].[BillingAdjustments]
GO

CREATE TABLE [dbo].[BillingAdjustments](
    [Id] [uniqueidentifier] NOT NULL,
    [BillingRecordId] [uniqueidentifier] NOT NULL,
    [Type] [int] NOT NULL,
    [Amount] [decimal](18, 2) NOT NULL,
    [Description] [nvarchar](500) NOT NULL,
    [Reason] [nvarchar](500) NULL,
    [IsPercentage] [bit] NOT NULL,
    [Percentage] [decimal](18, 2) NULL,
    [AppliedAt] [datetime2](7) NOT NULL,
    [AppliedBy] [int] NULL,
    [IsApproved] [bit] NOT NULL,
    [ApprovalNotes] [nvarchar](500) NULL,
    [IsActive] [bit] NOT NULL,
    [IsDeleted] [bit] NOT NULL,
    [CreatedBy] [int] NULL,
    [CreatedDate] [datetime2](7) NULL,
    [UpdatedBy] [int] NULL,
    [UpdatedDate] [datetime2](7) NULL,
    [DeletedBy] [int] NULL,
    [DeletedDate] [datetime2](7) NULL,
    
    CONSTRAINT [PK_BillingAdjustments] PRIMARY KEY CLUSTERED ([Id] ASC)
) ON [PRIMARY]
GO

-- PaymentRefund Table
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PaymentRefunds]') AND type in (N'U'))
    DROP TABLE [dbo].[PaymentRefunds]
GO

CREATE TABLE [dbo].[PaymentRefunds](
    [Id] [uniqueidentifier] NOT NULL,
    [SubscriptionPaymentId] [uniqueidentifier] NOT NULL,
    [Amount] [decimal](18, 2) NOT NULL,
    [Reason] [nvarchar](500) NOT NULL,
    [StripeRefundId] [nvarchar](100) NULL,
    [RefundedAt] [datetime2](7) NOT NULL,
    [ProcessedByUserId] [int] NULL,
    [IsActive] [bit] NOT NULL,
    [IsDeleted] [bit] NOT NULL,
    [CreatedBy] [int] NULL,
    [CreatedDate] [datetime2](7) NULL,
    [UpdatedBy] [int] NULL,
    [UpdatedDate] [datetime2](7) NULL,
    [DeletedBy] [int] NULL,
    [DeletedDate] [datetime2](7) NULL,
    
    CONSTRAINT [PK_PaymentRefunds] PRIMARY KEY CLUSTERED ([Id] ASC)
) ON [PRIMARY]
GO

-- =====================================================
-- 8. PRIVILEGE AND USAGE TRACKING TABLES
-- =====================================================

-- SubscriptionPlanPrivilege Table
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SubscriptionPlanPrivileges]') AND type in (N'U'))
    DROP TABLE [dbo].[SubscriptionPlanPrivileges]
GO

CREATE TABLE [dbo].[SubscriptionPlanPrivileges](
    [Id] [uniqueidentifier] NOT NULL,
    [SubscriptionPlanId] [uniqueidentifier] NOT NULL,
    [PrivilegeId] [uniqueidentifier] NOT NULL,
    [Value] [int] NOT NULL,
    [UsagePeriodId] [uniqueidentifier] NOT NULL,
    [DurationMonths] [int] NOT NULL,
    [Description] [nvarchar](500) NULL,
    [EffectiveDate] [datetime2](7) NULL,
    [ExpirationDate] [datetime2](7) NULL,
    [DailyLimit] [int] NULL,
    [WeeklyLimit] [int] NULL,
    [MonthlyLimit] [int] NULL,
    [UnitCost] [decimal](18, 2) NOT NULL,
    [IsActive] [bit] NOT NULL,
    [IsDeleted] [bit] NOT NULL,
    [CreatedBy] [int] NULL,
    [CreatedDate] [datetime2](7) NULL,
    [UpdatedBy] [int] NULL,
    [UpdatedDate] [datetime2](7) NULL,
    [DeletedBy] [int] NULL,
    [DeletedDate] [datetime2](7) NULL,
    
    CONSTRAINT [PK_SubscriptionPlanPrivileges] PRIMARY KEY CLUSTERED ([Id] ASC)
) ON [PRIMARY]
GO

-- UserSubscriptionPrivilegeUsage Table
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserSubscriptionPrivilegeUsages]') AND type in (N'U'))
    DROP TABLE [dbo].[UserSubscriptionPrivilegeUsages]
GO

CREATE TABLE [dbo].[UserSubscriptionPrivilegeUsages](
    [Id] [uniqueidentifier] NOT NULL,
    [SubscriptionId] [uniqueidentifier] NOT NULL,
    [SubscriptionPlanPrivilegeId] [uniqueidentifier] NOT NULL,
    [PrivilegeId] [uniqueidentifier] NOT NULL,
    [UsedValue] [int] NOT NULL,
    [AllowedValue] [int] NOT NULL,
    [UsagePeriodStart] [datetime2](7) NOT NULL,
    [UsagePeriodEnd] [datetime2](7) NOT NULL,
    [LastUsedAt] [datetime2](7) NULL,
    [ResetAt] [datetime2](7) NULL,
    [Notes] [nvarchar](500) NULL,
    [IsActive] [bit] NOT NULL,
    [IsDeleted] [bit] NOT NULL,
    [CreatedBy] [int] NULL,
    [CreatedDate] [datetime2](7) NULL,
    [UpdatedBy] [int] NULL,
    [UpdatedDate] [datetime2](7) NULL,
    [DeletedBy] [int] NULL,
    [DeletedDate] [datetime2](7) NULL,
    
    CONSTRAINT [PK_UserSubscriptionPrivilegeUsages] PRIMARY KEY CLUSTERED ([Id] ASC)
) ON [PRIMARY]
GO

-- PrivilegeUsageHistory Table
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PrivilegeUsageHistories]') AND type in (N'U'))
    DROP TABLE [dbo].[PrivilegeUsageHistories]
GO

CREATE TABLE [dbo].[PrivilegeUsageHistories](
    [Id] [uniqueidentifier] NOT NULL,
    [UserSubscriptionPrivilegeUsageId] [uniqueidentifier] NOT NULL,
    [UsedValue] [int] NOT NULL,
    [UsedAt] [datetime2](7) NOT NULL,
    [UsageDate] [date] NOT NULL,
    [UsageWeek] [nvarchar](10) NOT NULL,
    [UsageMonth] [nvarchar](7) NOT NULL,
    [Notes] [nvarchar](500) NULL,
    [IsActive] [bit] NOT NULL,
    [IsDeleted] [bit] NOT NULL,
    [CreatedBy] [int] NULL,
    [CreatedDate] [datetime2](7) NULL,
    [UpdatedBy] [int] NULL,
    [UpdatedDate] [datetime2](7) NULL,
    [DeletedBy] [int] NULL,
    [DeletedDate] [datetime2](7) NULL,
    
    CONSTRAINT [PK_PrivilegeUsageHistories] PRIMARY KEY CLUSTERED ([Id] ASC)
) ON [PRIMARY]
GO

-- SubscriptionStatusHistory Table
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SubscriptionStatusHistories]') AND type in (N'U'))
    DROP TABLE [dbo].[SubscriptionStatusHistories]
GO

CREATE TABLE [dbo].[SubscriptionStatusHistories](
    [Id] [uniqueidentifier] NOT NULL,
    [SubscriptionId] [uniqueidentifier] NOT NULL,
    [FromStatus] [nvarchar](50) NULL,
    [ToStatus] [nvarchar](50) NOT NULL,
    [Reason] [nvarchar](500) NULL,
    [ChangedByUserId] [int] NULL,
    [ChangedAt] [datetime2](7) NOT NULL,
    [Metadata] [nvarchar](1000) NULL,
    [IsActive] [bit] NOT NULL,
    [IsDeleted] [bit] NOT NULL,
    [CreatedBy] [int] NULL,
    [CreatedDate] [datetime2](7) NULL,
    [UpdatedBy] [int] NULL,
    [UpdatedDate] [datetime2](7) NULL,
    [DeletedBy] [int] NULL,
    [DeletedDate] [datetime2](7) NULL,
    
    CONSTRAINT [PK_SubscriptionStatusHistories] PRIMARY KEY CLUSTERED ([Id] ASC)
) ON [PRIMARY]
GO

-- ServiceConstraint Table
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ServiceConstraints]') AND type in (N'U'))
    DROP TABLE [dbo].[ServiceConstraints]
GO

CREATE TABLE [dbo].[ServiceConstraints](
    [Id] [uniqueidentifier] NOT NULL,
    [ServiceName] [nvarchar](100) NOT NULL,
    [Type] [int] NOT NULL,
    [Value] [int] NOT NULL,
    [Description] [nvarchar](500) NOT NULL,
    [MaxSessionsPerMonth] [int] NOT NULL,
    [MaxDurationPerSession] [int] NOT NULL,
    [MaxConcurrentSessions] [int] NOT NULL,
    [TotalMinutesPerMonth] [int] NULL,
    [AllowFileSharing] [bit] NOT NULL,
    [AllowVideoChat] [bit] NOT NULL,
    [PriorityQueue] [bit] NOT NULL,
    [MaxMessageLength] [int] NOT NULL,
    [SubscriptionPlanId] [uniqueidentifier] NOT NULL,
    [IsActive] [bit] NOT NULL,
    [IsDeleted] [bit] NOT NULL,
    [CreatedBy] [int] NULL,
    [CreatedDate] [datetime2](7) NULL,
    [UpdatedBy] [int] NULL,
    [UpdatedDate] [datetime2](7) NULL,
    [DeletedBy] [int] NULL,
    [DeletedDate] [datetime2](7) NULL,
    
    CONSTRAINT [PK_ServiceConstraints] PRIMARY KEY CLUSTERED ([Id] ASC)
) ON [PRIMARY]
GO

-- =====================================================
-- 9. DEFAULT VALUES FOR ALL TABLES
-- =====================================================

-- Subscriptions defaults
ALTER TABLE [dbo].[Subscriptions] ADD DEFAULT (newid()) FOR [Id]
ALTER TABLE [dbo].[Subscriptions] ADD DEFAULT ('Pending') FOR [Status]
ALTER TABLE [dbo].[Subscriptions] ADD DEFAULT (getutcdate()) FOR [StartDate]
ALTER TABLE [dbo].[Subscriptions] ADD DEFAULT (getutcdate()) FOR [NextBillingDate]
ALTER TABLE [dbo].[Subscriptions] ADD DEFAULT ((1)) FOR [AutoRenew]
ALTER TABLE [dbo].[Subscriptions] ADD DEFAULT ((0)) FOR [FailedPaymentAttempts]
ALTER TABLE [dbo].[Subscriptions] ADD DEFAULT ((0)) FOR [IsTrialSubscription]
ALTER TABLE [dbo].[Subscriptions] ADD DEFAULT ((0)) FOR [TrialDurationInDays]
ALTER TABLE [dbo].[Subscriptions] ADD DEFAULT ((0)) FOR [TotalUsageCount]
ALTER TABLE [dbo].[Subscriptions] ADD DEFAULT ((1)) FOR [IsActive]
ALTER TABLE [dbo].[Subscriptions] ADD DEFAULT ((0)) FOR [IsDeleted]
ALTER TABLE [dbo].[Subscriptions] ADD DEFAULT (getutcdate()) FOR [CreatedDate]

-- SubscriptionPayments defaults
ALTER TABLE [dbo].[SubscriptionPayments] ADD DEFAULT (newid()) FOR [Id]
ALTER TABLE [dbo].[SubscriptionPayments] ADD DEFAULT ((0)) FOR [TaxAmount]
ALTER TABLE [dbo].[SubscriptionPayments] ADD DEFAULT ((0)) FOR [Status]
ALTER TABLE [dbo].[SubscriptionPayments] ADD DEFAULT ((0)) FOR [Type]
ALTER TABLE [dbo].[SubscriptionPayments] ADD DEFAULT (getutcdate()) FOR [DueDate]
ALTER TABLE [dbo].[SubscriptionPayments] ADD DEFAULT (getutcdate()) FOR [BillingPeriodStart]
ALTER TABLE [dbo].[SubscriptionPayments] ADD DEFAULT (getutcdate()) FOR [BillingPeriodEnd]
ALTER TABLE [dbo].[SubscriptionPayments] ADD DEFAULT ((0)) FOR [AttemptCount]
ALTER TABLE [dbo].[SubscriptionPayments] ADD DEFAULT ((0)) FOR [RefundedAmount]
ALTER TABLE [dbo].[SubscriptionPayments] ADD DEFAULT ((1)) FOR [IsActive]
ALTER TABLE [dbo].[SubscriptionPayments] ADD DEFAULT ((0)) FOR [IsDeleted]
ALTER TABLE [dbo].[SubscriptionPayments] ADD DEFAULT (getutcdate()) FOR [CreatedDate]

-- BillingRecords defaults
ALTER TABLE [dbo].[BillingRecords] ADD DEFAULT (newid()) FOR [Id]
ALTER TABLE [dbo].[BillingRecords] ADD DEFAULT ((0)) FOR [Status]
ALTER TABLE [dbo].[BillingRecords] ADD DEFAULT ((0)) FOR [Type]
ALTER TABLE [dbo].[BillingRecords] ADD DEFAULT ((0)) FOR [TaxAmount]
ALTER TABLE [dbo].[BillingRecords] ADD DEFAULT ((0)) FOR [ShippingAmount]
ALTER TABLE [dbo].[BillingRecords] ADD DEFAULT (getutcdate()) FOR [BillingDate]
ALTER TABLE [dbo].[BillingRecords] ADD DEFAULT ((0)) FOR [IsRecurring]
ALTER TABLE [dbo].[BillingRecords] ADD DEFAULT ((1)) FOR [IsActive]
ALTER TABLE [dbo].[BillingRecords] ADD DEFAULT ((0)) FOR [IsDeleted]
ALTER TABLE [dbo].[BillingRecords] ADD DEFAULT (getutcdate()) FOR [CreatedDate]

-- BillingAdjustments defaults
ALTER TABLE [dbo].[BillingAdjustments] ADD DEFAULT (newid()) FOR [Id]
ALTER TABLE [dbo].[BillingAdjustments] ADD DEFAULT ((0)) FOR [IsPercentage]
ALTER TABLE [dbo].[BillingAdjustments] ADD DEFAULT (getutcdate()) FOR [AppliedAt]
ALTER TABLE [dbo].[BillingAdjustments] ADD DEFAULT ((1)) FOR [IsApproved]
ALTER TABLE [dbo].[BillingAdjustments] ADD DEFAULT ((1)) FOR [IsActive]
ALTER TABLE [dbo].[BillingAdjustments] ADD DEFAULT ((0)) FOR [IsDeleted]
ALTER TABLE [dbo].[BillingAdjustments] ADD DEFAULT (getutcdate()) FOR [CreatedDate]

-- PaymentRefunds defaults
ALTER TABLE [dbo].[PaymentRefunds] ADD DEFAULT (newid()) FOR [Id]
ALTER TABLE [dbo].[PaymentRefunds] ADD DEFAULT (getutcdate()) FOR [RefundedAt]
ALTER TABLE [dbo].[PaymentRefunds] ADD DEFAULT ((1)) FOR [IsActive]
ALTER TABLE [dbo].[PaymentRefunds] ADD DEFAULT ((0)) FOR [IsDeleted]
ALTER TABLE [dbo].[PaymentRefunds] ADD DEFAULT (getutcdate()) FOR [CreatedDate]

-- SubscriptionPlanPrivileges defaults
ALTER TABLE [dbo].[SubscriptionPlanPrivileges] ADD DEFAULT (newid()) FOR [Id]
ALTER TABLE [dbo].[SubscriptionPlanPrivileges] ADD DEFAULT ((1)) FOR [DurationMonths]
ALTER TABLE [dbo].[SubscriptionPlanPrivileges] ADD DEFAULT ((0)) FOR [UnitCost]
ALTER TABLE [dbo].[SubscriptionPlanPrivileges] ADD DEFAULT ((1)) FOR [IsActive]
ALTER TABLE [dbo].[SubscriptionPlanPrivileges] ADD DEFAULT ((0)) FOR [IsDeleted]
ALTER TABLE [dbo].[SubscriptionPlanPrivileges] ADD DEFAULT (getutcdate()) FOR [CreatedDate]

-- UserSubscriptionPrivilegeUsages defaults
ALTER TABLE [dbo].[UserSubscriptionPrivilegeUsages] ADD DEFAULT (newid()) FOR [Id]
ALTER TABLE [dbo].[UserSubscriptionPrivilegeUsages] ADD DEFAULT ((0)) FOR [UsedValue]
ALTER TABLE [dbo].[UserSubscriptionPrivilegeUsages] ADD DEFAULT (getutcdate()) FOR [UsagePeriodStart]
ALTER TABLE [dbo].[UserSubscriptionPrivilegeUsages] ADD DEFAULT (getutcdate()) FOR [UsagePeriodEnd]
ALTER TABLE [dbo].[UserSubscriptionPrivilegeUsages] ADD DEFAULT ((1)) FOR [IsActive]
ALTER TABLE [dbo].[UserSubscriptionPrivilegeUsages] ADD DEFAULT ((0)) FOR [IsDeleted]
ALTER TABLE [dbo].[UserSubscriptionPrivilegeUsages] ADD DEFAULT (getutcdate()) FOR [CreatedDate]

-- PrivilegeUsageHistories defaults
ALTER TABLE [dbo].[PrivilegeUsageHistories] ADD DEFAULT (newid()) FOR [Id]
ALTER TABLE [dbo].[PrivilegeUsageHistories] ADD DEFAULT ((1)) FOR [UsedValue]
ALTER TABLE [dbo].[PrivilegeUsageHistories] ADD DEFAULT (getutcdate()) FOR [UsedAt]
ALTER TABLE [dbo].[PrivilegeUsageHistories] ADD DEFAULT (CONVERT(date, GETUTCDATE())) FOR [UsageDate]
ALTER TABLE [dbo].[PrivilegeUsageHistories] ADD DEFAULT (FORMAT(GETUTCDATE(), 'yyyy-WW')) FOR [UsageWeek]
ALTER TABLE [dbo].[PrivilegeUsageHistories] ADD DEFAULT (FORMAT(GETUTCDATE(), 'yyyy-MM')) FOR [UsageMonth]
ALTER TABLE [dbo].[PrivilegeUsageHistories] ADD DEFAULT ((1)) FOR [IsActive]
ALTER TABLE [dbo].[PrivilegeUsageHistories] ADD DEFAULT ((0)) FOR [IsDeleted]
ALTER TABLE [dbo].[PrivilegeUsageHistories] ADD DEFAULT (getutcdate()) FOR [CreatedDate]

-- SubscriptionStatusHistories defaults
ALTER TABLE [dbo].[SubscriptionStatusHistories] ADD DEFAULT (newid()) FOR [Id]
ALTER TABLE [dbo].[SubscriptionStatusHistories] ADD DEFAULT (getutcdate()) FOR [ChangedAt]
ALTER TABLE [dbo].[SubscriptionStatusHistories] ADD DEFAULT ((1)) FOR [IsActive]
ALTER TABLE [dbo].[SubscriptionStatusHistories] ADD DEFAULT ((0)) FOR [IsDeleted]
ALTER TABLE [dbo].[SubscriptionStatusHistories] ADD DEFAULT (getutcdate()) FOR [CreatedDate]

-- ServiceConstraints defaults
ALTER TABLE [dbo].[ServiceConstraints] ADD DEFAULT (newid()) FOR [Id]
ALTER TABLE [dbo].[ServiceConstraints] ADD DEFAULT ((0)) FOR [MaxSessionsPerMonth]
ALTER TABLE [dbo].[ServiceConstraints] ADD DEFAULT ((0)) FOR [MaxDurationPerSession]
ALTER TABLE [dbo].[ServiceConstraints] ADD DEFAULT ((0)) FOR [MaxConcurrentSessions]
ALTER TABLE [dbo].[ServiceConstraints] ADD DEFAULT ((1)) FOR [AllowFileSharing]
ALTER TABLE [dbo].[ServiceConstraints] ADD DEFAULT ((0)) FOR [AllowVideoChat]
ALTER TABLE [dbo].[ServiceConstraints] ADD DEFAULT ((0)) FOR [PriorityQueue]
ALTER TABLE [dbo].[ServiceConstraints] ADD DEFAULT ((1000)) FOR [MaxMessageLength]
ALTER TABLE [dbo].[ServiceConstraints] ADD DEFAULT ((1)) FOR [IsActive]
ALTER TABLE [dbo].[ServiceConstraints] ADD DEFAULT ((0)) FOR [IsDeleted]
ALTER TABLE [dbo].[ServiceConstraints] ADD DEFAULT (getutcdate()) FOR [CreatedDate]

PRINT 'All subscription-related tables created successfully!'
