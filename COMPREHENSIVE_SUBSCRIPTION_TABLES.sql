-- =====================================================
-- COMPREHENSIVE SUBSCRIPTION MANAGEMENT TABLES
-- =====================================================
-- This script creates all subscription-related tables with proper constraints,
-- foreign keys, indexes, and BaseEntity properties
-- =====================================================

-- =====================================================
-- 1. MASTER TABLES
-- =====================================================

-- MasterPrivilegeType Table
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MasterPrivilegeTypes]') AND type in (N'U'))
    DROP TABLE [dbo].[MasterPrivilegeTypes]
GO

CREATE TABLE [dbo].[MasterPrivilegeTypes](
    [Id] [uniqueidentifier] NOT NULL,
    [Name] [nvarchar](50) NOT NULL,
    [Description] [nvarchar](200) NULL,
    [SortOrder] [int] NOT NULL,
    [IsActive] [bit] NOT NULL,
    [IsDeleted] [bit] NOT NULL,
    [CreatedBy] [int] NULL,
    [CreatedDate] [datetime2](7) NULL,
    [UpdatedBy] [int] NULL,
    [UpdatedDate] [datetime2](7) NULL,
    [DeletedBy] [int] NULL,
    [DeletedDate] [datetime2](7) NULL,
    
    CONSTRAINT [PK_MasterPrivilegeTypes] PRIMARY KEY CLUSTERED ([Id] ASC)
) ON [PRIMARY]
GO

-- MasterCurrency Table
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MasterCurrencies]') AND type in (N'U'))
    DROP TABLE [dbo].[MasterCurrencies]
GO

CREATE TABLE [dbo].[MasterCurrencies](
    [Id] [uniqueidentifier] NOT NULL,
    [Code] [nvarchar](10) NOT NULL,
    [Name] [nvarchar](50) NOT NULL,
    [Symbol] [nvarchar](10) NULL,
    [SortOrder] [int] NOT NULL,
    [IsActive] [bit] NOT NULL,
    [IsDeleted] [bit] NOT NULL,
    [CreatedBy] [int] NULL,
    [CreatedDate] [datetime2](7) NULL,
    [UpdatedBy] [int] NULL,
    [UpdatedDate] [datetime2](7) NULL,
    [DeletedBy] [int] NULL,
    [DeletedDate] [datetime2](7) NULL,
    
    CONSTRAINT [PK_MasterCurrencies] PRIMARY KEY CLUSTERED ([Id] ASC)
) ON [PRIMARY]
GO

-- MasterBillingCycle Table
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MasterBillingCycles]') AND type in (N'U'))
    DROP TABLE [dbo].[MasterBillingCycles]
GO

CREATE TABLE [dbo].[MasterBillingCycles](
    [Id] [uniqueidentifier] NOT NULL,
    [Name] [nvarchar](50) NOT NULL,
    [Description] [nvarchar](200) NULL,
    [DurationInDays] [int] NOT NULL,
    [SortOrder] [int] NOT NULL,
    [IsActive] [bit] NOT NULL,
    [IsDeleted] [bit] NOT NULL,
    [CreatedBy] [int] NULL,
    [CreatedDate] [datetime2](7) NULL,
    [UpdatedBy] [int] NULL,
    [UpdatedDate] [datetime2](7) NULL,
    [DeletedBy] [int] NULL,
    [DeletedDate] [datetime2](7) NULL,
    
    CONSTRAINT [PK_MasterBillingCycles] PRIMARY KEY CLUSTERED ([Id] ASC)
) ON [PRIMARY]
GO

-- =====================================================
-- 2. CORE ENTITY TABLES
-- =====================================================

-- Privilege Table
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Privileges]') AND type in (N'U'))
    DROP TABLE [dbo].[Privileges]
GO

CREATE TABLE [dbo].[Privileges](
    [Id] [uniqueidentifier] NOT NULL,
    [Name] [nvarchar](100) NOT NULL,
    [Description] [nvarchar](500) NULL,
    [PrivilegeTypeId] [uniqueidentifier] NOT NULL,
    [IsActive] [bit] NOT NULL,
    [IsDeleted] [bit] NOT NULL,
    [CreatedBy] [int] NULL,
    [CreatedDate] [datetime2](7) NULL,
    [UpdatedBy] [int] NULL,
    [UpdatedDate] [datetime2](7) NULL,
    [DeletedBy] [int] NULL,
    [DeletedDate] [datetime2](7) NULL,
    
    CONSTRAINT [PK_Privileges] PRIMARY KEY CLUSTERED ([Id] ASC)
) ON [PRIMARY]
GO

-- SubscriptionPlan Table (Enhanced version)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SubscriptionPlans]') AND type in (N'U'))
    DROP TABLE [dbo].[SubscriptionPlans]
GO

CREATE TABLE [dbo].[SubscriptionPlans](
    [Id] [uniqueidentifier] NOT NULL,
    [Name] [nvarchar](100) NOT NULL,
    [Description] [nvarchar](1000) NULL,
    [ShortDescription] [nvarchar](200) NULL,
    [IsFeatured] [bit] NOT NULL,
    [IsTrialAllowed] [bit] NOT NULL,
    [TrialDurationInDays] [int] NOT NULL,
    [IsMostPopular] [bit] NOT NULL,
    [IsTrending] [bit] NOT NULL,
    [DisplayOrder] [int] NOT NULL,
    [PlanType] [nvarchar](50) NOT NULL,
    [Price] [decimal](18, 2) NOT NULL,
    [DiscountedPrice] [decimal](18, 2) NULL,
    [DiscountValidUntil] [datetime2](7) NULL,
    [BillingCycleId] [uniqueidentifier] NOT NULL,
    [CurrencyId] [uniqueidentifier] NOT NULL,
    [CategoryId] [uniqueidentifier] NULL,
    [StripeProductId] [nvarchar](100) NULL,
    [StripeMonthlyPriceId] [nvarchar](100) NULL,
    [StripeQuarterlyPriceId] [nvarchar](100) NULL,
    [StripeAnnualPriceId] [nvarchar](100) NULL,
    [MessagingCount] [int] NOT NULL,
    [IncludesMedicationDelivery] [bit] NOT NULL,
    [IncludesFollowUpCare] [bit] NOT NULL,
    [DeliveryFrequencyDays] [int] NOT NULL,
    [MaxPauseDurationDays] [int] NOT NULL,
    [MaxConcurrentUsers] [int] NOT NULL,
    [GracePeriodDays] [int] NOT NULL,
    [Features] [nvarchar](1000) NULL,
    [Terms] [nvarchar](500) NULL,
    [EffectiveDate] [datetime2](7) NULL,
    [ExpirationDate] [datetime2](7) NULL,
    [IsActive] [bit] NOT NULL,
    [IsDeleted] [bit] NOT NULL,
    [CreatedBy] [int] NULL,
    [CreatedDate] [datetime2](7) NULL,
    [UpdatedBy] [int] NULL,
    [UpdatedDate] [datetime2](7) NULL,
    [DeletedBy] [int] NULL,
    [DeletedDate] [datetime2](7) NULL,
    
    CONSTRAINT [PK_SubscriptionPlans] PRIMARY KEY CLUSTERED ([Id] ASC)
) ON [PRIMARY]
GO

-- =====================================================
-- 3. DEFAULT VALUES FOR MASTER TABLES
-- =====================================================

-- MasterPrivilegeTypes defaults
ALTER TABLE [dbo].[MasterPrivilegeTypes] ADD DEFAULT (newid()) FOR [Id]
ALTER TABLE [dbo].[MasterPrivilegeTypes] ADD DEFAULT ((0)) FOR [SortOrder]
ALTER TABLE [dbo].[MasterPrivilegeTypes] ADD DEFAULT ((1)) FOR [IsActive]
ALTER TABLE [dbo].[MasterPrivilegeTypes] ADD DEFAULT ((0)) FOR [IsDeleted]
ALTER TABLE [dbo].[MasterPrivilegeTypes] ADD DEFAULT (getutcdate()) FOR [CreatedDate]

-- MasterCurrencies defaults
ALTER TABLE [dbo].[MasterCurrencies] ADD DEFAULT (newid()) FOR [Id]
ALTER TABLE [dbo].[MasterCurrencies] ADD DEFAULT ((0)) FOR [SortOrder]
ALTER TABLE [dbo].[MasterCurrencies] ADD DEFAULT ((1)) FOR [IsActive]
ALTER TABLE [dbo].[MasterCurrencies] ADD DEFAULT ((0)) FOR [IsDeleted]
ALTER TABLE [dbo].[MasterCurrencies] ADD DEFAULT (getutcdate()) FOR [CreatedDate]

-- MasterBillingCycles defaults
ALTER TABLE [dbo].[MasterBillingCycles] ADD DEFAULT (newid()) FOR [Id]
ALTER TABLE [dbo].[MasterBillingCycles] ADD DEFAULT ((0)) FOR [SortOrder]
ALTER TABLE [dbo].[MasterBillingCycles] ADD DEFAULT ((1)) FOR [IsActive]
ALTER TABLE [dbo].[MasterBillingCycles] ADD DEFAULT ((0)) FOR [IsDeleted]
ALTER TABLE [dbo].[MasterBillingCycles] ADD DEFAULT (getutcdate()) FOR [CreatedDate]

-- Privileges defaults
ALTER TABLE [dbo].[Privileges] ADD DEFAULT (newid()) FOR [Id]
ALTER TABLE [dbo].[Privileges] ADD DEFAULT ((1)) FOR [IsActive]
ALTER TABLE [dbo].[Privileges] ADD DEFAULT ((0)) FOR [IsDeleted]
ALTER TABLE [dbo].[Privileges] ADD DEFAULT (getutcdate()) FOR [CreatedDate]

-- SubscriptionPlans defaults
ALTER TABLE [dbo].[SubscriptionPlans] ADD DEFAULT (newid()) FOR [Id]
ALTER TABLE [dbo].[SubscriptionPlans] ADD DEFAULT ((0)) FOR [IsFeatured]
ALTER TABLE [dbo].[SubscriptionPlans] ADD DEFAULT ((0)) FOR [IsTrialAllowed]
ALTER TABLE [dbo].[SubscriptionPlans] ADD DEFAULT ((0)) FOR [TrialDurationInDays]
ALTER TABLE [dbo].[SubscriptionPlans] ADD DEFAULT ((0)) FOR [IsMostPopular]
ALTER TABLE [dbo].[SubscriptionPlans] ADD DEFAULT ((0)) FOR [IsTrending]
ALTER TABLE [dbo].[SubscriptionPlans] ADD DEFAULT ((0)) FOR [DisplayOrder]
ALTER TABLE [dbo].[SubscriptionPlans] ADD DEFAULT ('Standard') FOR [PlanType]
ALTER TABLE [dbo].[SubscriptionPlans] ADD DEFAULT ((10)) FOR [MessagingCount]
ALTER TABLE [dbo].[SubscriptionPlans] ADD DEFAULT ((1)) FOR [IncludesMedicationDelivery]
ALTER TABLE [dbo].[SubscriptionPlans] ADD DEFAULT ((1)) FOR [IncludesFollowUpCare]
ALTER TABLE [dbo].[SubscriptionPlans] ADD DEFAULT ((30)) FOR [DeliveryFrequencyDays]
ALTER TABLE [dbo].[SubscriptionPlans] ADD DEFAULT ((90)) FOR [MaxPauseDurationDays]
ALTER TABLE [dbo].[SubscriptionPlans] ADD DEFAULT ((1)) FOR [MaxConcurrentUsers]
ALTER TABLE [dbo].[SubscriptionPlans] ADD DEFAULT ((0)) FOR [GracePeriodDays]
ALTER TABLE [dbo].[SubscriptionPlans] ADD DEFAULT ((1)) FOR [IsActive]
ALTER TABLE [dbo].[SubscriptionPlans] ADD DEFAULT ((0)) FOR [IsDeleted]
ALTER TABLE [dbo].[SubscriptionPlans] ADD DEFAULT (getutcdate()) FOR [CreatedDate]

-- =====================================================
-- 4. FOREIGN KEY CONSTRAINTS FOR MASTER TABLES
-- =====================================================

-- MasterPrivilegeTypes foreign keys
ALTER TABLE [dbo].[MasterPrivilegeTypes] WITH CHECK ADD 
CONSTRAINT [FK_MasterPrivilegeTypes_User_CreatedBy] 
FOREIGN KEY([CreatedBy]) REFERENCES [dbo].[User] ([UserID])
GO
ALTER TABLE [dbo].[MasterPrivilegeTypes] CHECK CONSTRAINT [FK_MasterPrivilegeTypes_User_CreatedBy]
GO

ALTER TABLE [dbo].[MasterPrivilegeTypes] WITH CHECK ADD 
CONSTRAINT [FK_MasterPrivilegeTypes_User_UpdatedBy] 
FOREIGN KEY([UpdatedBy]) REFERENCES [dbo].[User] ([UserID])
GO
ALTER TABLE [dbo].[MasterPrivilegeTypes] CHECK CONSTRAINT [FK_MasterPrivilegeTypes_User_UpdatedBy]
GO

ALTER TABLE [dbo].[MasterPrivilegeTypes] WITH CHECK ADD 
CONSTRAINT [FK_MasterPrivilegeTypes_User_DeletedBy] 
FOREIGN KEY([DeletedBy]) REFERENCES [dbo].[User] ([UserID])
GO
ALTER TABLE [dbo].[MasterPrivilegeTypes] CHECK CONSTRAINT [FK_MasterPrivilegeTypes_User_DeletedBy]
GO

-- MasterCurrencies foreign keys
ALTER TABLE [dbo].[MasterCurrencies] WITH CHECK ADD 
CONSTRAINT [FK_MasterCurrencies_User_CreatedBy] 
FOREIGN KEY([CreatedBy]) REFERENCES [dbo].[User] ([UserID])
GO
ALTER TABLE [dbo].[MasterCurrencies] CHECK CONSTRAINT [FK_MasterCurrencies_User_CreatedBy]
GO

ALTER TABLE [dbo].[MasterCurrencies] WITH CHECK ADD 
CONSTRAINT [FK_MasterCurrencies_User_UpdatedBy] 
FOREIGN KEY([UpdatedBy]) REFERENCES [dbo].[User] ([UserID])
GO
ALTER TABLE [dbo].[MasterCurrencies] CHECK CONSTRAINT [FK_MasterCurrencies_User_UpdatedBy]
GO

ALTER TABLE [dbo].[MasterCurrencies] WITH CHECK ADD 
CONSTRAINT [FK_MasterCurrencies_User_DeletedBy] 
FOREIGN KEY([DeletedBy]) REFERENCES [dbo].[User] ([UserID])
GO
ALTER TABLE [dbo].[MasterCurrencies] CHECK CONSTRAINT [FK_MasterCurrencies_User_DeletedBy]
GO

-- MasterBillingCycles foreign keys
ALTER TABLE [dbo].[MasterBillingCycles] WITH CHECK ADD 
CONSTRAINT [FK_MasterBillingCycles_User_CreatedBy] 
FOREIGN KEY([CreatedBy]) REFERENCES [dbo].[User] ([UserID])
GO
ALTER TABLE [dbo].[MasterBillingCycles] CHECK CONSTRAINT [FK_MasterBillingCycles_User_CreatedBy]
GO

ALTER TABLE [dbo].[MasterBillingCycles] WITH CHECK ADD 
CONSTRAINT [FK_MasterBillingCycles_User_UpdatedBy] 
FOREIGN KEY([UpdatedBy]) REFERENCES [dbo].[User] ([UserID])
GO
ALTER TABLE [dbo].[MasterBillingCycles] CHECK CONSTRAINT [FK_MasterBillingCycles_User_UpdatedBy]
GO

ALTER TABLE [dbo].[MasterBillingCycles] WITH CHECK ADD 
CONSTRAINT [FK_MasterBillingCycles_User_DeletedBy] 
FOREIGN KEY([DeletedBy]) REFERENCES [dbo].[User] ([UserID])
GO
ALTER TABLE [dbo].[MasterBillingCycles] CHECK CONSTRAINT [FK_MasterBillingCycles_User_DeletedBy]
GO

-- Privileges foreign keys
ALTER TABLE [dbo].[Privileges] WITH CHECK ADD 
CONSTRAINT [FK_Privileges_MasterPrivilegeTypes_PrivilegeTypeId] 
FOREIGN KEY([PrivilegeTypeId]) REFERENCES [dbo].[MasterPrivilegeTypes] ([Id])
GO
ALTER TABLE [dbo].[Privileges] CHECK CONSTRAINT [FK_Privileges_MasterPrivilegeTypes_PrivilegeTypeId]
GO

ALTER TABLE [dbo].[Privileges] WITH CHECK ADD 
CONSTRAINT [FK_Privileges_User_CreatedBy] 
FOREIGN KEY([CreatedBy]) REFERENCES [dbo].[User] ([UserID])
GO
ALTER TABLE [dbo].[Privileges] CHECK CONSTRAINT [FK_Privileges_User_CreatedBy]
GO

ALTER TABLE [dbo].[Privileges] WITH CHECK ADD 
CONSTRAINT [FK_Privileges_User_UpdatedBy] 
FOREIGN KEY([UpdatedBy]) REFERENCES [dbo].[User] ([UserID])
GO
ALTER TABLE [dbo].[Privileges] CHECK CONSTRAINT [FK_Privileges_User_UpdatedBy]
GO

ALTER TABLE [dbo].[Privileges] WITH CHECK ADD 
CONSTRAINT [FK_Privileges_User_DeletedBy] 
FOREIGN KEY([DeletedBy]) REFERENCES [dbo].[User] ([UserID])
GO
ALTER TABLE [dbo].[Privileges] CHECK CONSTRAINT [FK_Privileges_User_DeletedBy]
GO

-- SubscriptionPlans foreign keys
ALTER TABLE [dbo].[SubscriptionPlans] WITH CHECK ADD 
CONSTRAINT [FK_SubscriptionPlans_MasterBillingCycles_BillingCycleId] 
FOREIGN KEY([BillingCycleId]) REFERENCES [dbo].[MasterBillingCycles] ([Id])
GO
ALTER TABLE [dbo].[SubscriptionPlans] CHECK CONSTRAINT [FK_SubscriptionPlans_MasterBillingCycles_BillingCycleId]
GO

ALTER TABLE [dbo].[SubscriptionPlans] WITH CHECK ADD 
CONSTRAINT [FK_SubscriptionPlans_MasterCurrencies_CurrencyId] 
FOREIGN KEY([CurrencyId]) REFERENCES [dbo].[MasterCurrencies] ([Id])
GO
ALTER TABLE [dbo].[SubscriptionPlans] CHECK CONSTRAINT [FK_SubscriptionPlans_MasterCurrencies_CurrencyId]
GO

ALTER TABLE [dbo].[SubscriptionPlans] WITH CHECK ADD 
CONSTRAINT [FK_SubscriptionPlans_Categories_CategoryId] 
FOREIGN KEY([CategoryId]) REFERENCES [dbo].[Categories] ([Id])
GO
ALTER TABLE [dbo].[SubscriptionPlans] CHECK CONSTRAINT [FK_SubscriptionPlans_Categories_CategoryId]
GO

ALTER TABLE [dbo].[SubscriptionPlans] WITH CHECK ADD 
CONSTRAINT [FK_SubscriptionPlans_User_CreatedBy] 
FOREIGN KEY([CreatedBy]) REFERENCES [dbo].[User] ([UserID])
GO
ALTER TABLE [dbo].[SubscriptionPlans] CHECK CONSTRAINT [FK_SubscriptionPlans_User_CreatedBy]
GO

ALTER TABLE [dbo].[SubscriptionPlans] WITH CHECK ADD 
CONSTRAINT [FK_SubscriptionPlans_User_UpdatedBy] 
FOREIGN KEY([UpdatedBy]) REFERENCES [dbo].[User] ([UserID])
GO
ALTER TABLE [dbo].[SubscriptionPlans] CHECK CONSTRAINT [FK_SubscriptionPlans_User_UpdatedBy]
GO

ALTER TABLE [dbo].[SubscriptionPlans] WITH CHECK ADD 
CONSTRAINT [FK_SubscriptionPlans_User_DeletedBy] 
FOREIGN KEY([DeletedBy]) REFERENCES [dbo].[User] ([UserID])
GO
ALTER TABLE [dbo].[SubscriptionPlans] CHECK CONSTRAINT [FK_SubscriptionPlans_User_DeletedBy]
GO

-- =====================================================
-- 5. CHECK CONSTRAINTS FOR MASTER TABLES
-- =====================================================

-- MasterPrivilegeTypes constraints
ALTER TABLE [dbo].[MasterPrivilegeTypes] WITH CHECK ADD 
CONSTRAINT [CK_MasterPrivilegeTypes_Name_NotEmpty] 
CHECK (LEN(TRIM([Name])) > 0)
GO
ALTER TABLE [dbo].[MasterPrivilegeTypes] CHECK CONSTRAINT [CK_MasterPrivilegeTypes_Name_NotEmpty]
GO

-- MasterCurrencies constraints
ALTER TABLE [dbo].[MasterCurrencies] WITH CHECK ADD 
CONSTRAINT [CK_MasterCurrencies_Code_NotEmpty] 
CHECK (LEN(TRIM([Code])) > 0)
GO
ALTER TABLE [dbo].[MasterCurrencies] CHECK CONSTRAINT [CK_MasterCurrencies_Code_NotEmpty]
GO

ALTER TABLE [dbo].[MasterCurrencies] WITH CHECK ADD 
CONSTRAINT [CK_MasterCurrencies_Name_NotEmpty] 
CHECK (LEN(TRIM([Name])) > 0)
GO
ALTER TABLE [dbo].[MasterCurrencies] CHECK CONSTRAINT [CK_MasterCurrencies_Name_NotEmpty]
GO

-- MasterBillingCycles constraints
ALTER TABLE [dbo].[MasterBillingCycles] WITH CHECK ADD 
CONSTRAINT [CK_MasterBillingCycles_Name_NotEmpty] 
CHECK (LEN(TRIM([Name])) > 0)
GO
ALTER TABLE [dbo].[MasterBillingCycles] CHECK CONSTRAINT [CK_MasterBillingCycles_Name_NotEmpty]
GO

ALTER TABLE [dbo].[MasterBillingCycles] WITH CHECK ADD 
CONSTRAINT [CK_MasterBillingCycles_Duration_Positive] 
CHECK ([DurationInDays] > 0)
GO
ALTER TABLE [dbo].[MasterBillingCycles] CHECK CONSTRAINT [CK_MasterBillingCycles_Duration_Positive]
GO

-- Privileges constraints
ALTER TABLE [dbo].[Privileges] WITH CHECK ADD 
CONSTRAINT [CK_Privileges_Name_NotEmpty] 
CHECK (LEN(TRIM([Name])) > 0)
GO
ALTER TABLE [dbo].[Privileges] CHECK CONSTRAINT [CK_Privileges_Name_NotEmpty]
GO

-- SubscriptionPlans constraints
ALTER TABLE [dbo].[SubscriptionPlans] WITH CHECK ADD 
CONSTRAINT [CK_SubscriptionPlans_Name_NotEmpty] 
CHECK (LEN(TRIM([Name])) > 0)
GO
ALTER TABLE [dbo].[SubscriptionPlans] CHECK CONSTRAINT [CK_SubscriptionPlans_Name_NotEmpty]
GO

ALTER TABLE [dbo].[SubscriptionPlans] WITH CHECK ADD 
CONSTRAINT [CK_SubscriptionPlans_Price_Positive] 
CHECK ([Price] > 0)
GO
ALTER TABLE [dbo].[SubscriptionPlans] CHECK CONSTRAINT [CK_SubscriptionPlans_Price_Positive]
GO

ALTER TABLE [dbo].[SubscriptionPlans] WITH CHECK ADD 
CONSTRAINT [CK_SubscriptionPlans_TrialDuration_NonNegative] 
CHECK ([TrialDurationInDays] >= 0)
GO
ALTER TABLE [dbo].[SubscriptionPlans] CHECK CONSTRAINT [CK_SubscriptionPlans_TrialDuration_NonNegative]
GO

ALTER TABLE [dbo].[SubscriptionPlans] WITH CHECK ADD 
CONSTRAINT [CK_SubscriptionPlans_MessagingCount_NonNegative] 
CHECK ([MessagingCount] >= 0)
GO
ALTER TABLE [dbo].[SubscriptionPlans] CHECK CONSTRAINT [CK_SubscriptionPlans_MessagingCount_NonNegative]
GO

ALTER TABLE [dbo].[SubscriptionPlans] WITH CHECK ADD 
CONSTRAINT [CK_SubscriptionPlans_DeliveryFrequency_Positive] 
CHECK ([DeliveryFrequencyDays] >= 1)
GO
ALTER TABLE [dbo].[SubscriptionPlans] CHECK CONSTRAINT [CK_SubscriptionPlans_DeliveryFrequency_Positive]
GO

ALTER TABLE [dbo].[SubscriptionPlans] WITH CHECK ADD 
CONSTRAINT [CK_SubscriptionPlans_MaxPauseDuration_NonNegative] 
CHECK ([MaxPauseDurationDays] >= 0)
GO
ALTER TABLE [dbo].[SubscriptionPlans] CHECK CONSTRAINT [CK_SubscriptionPlans_MaxPauseDuration_NonNegative]
GO

ALTER TABLE [dbo].[SubscriptionPlans] WITH CHECK ADD 
CONSTRAINT [CK_SubscriptionPlans_MaxConcurrentUsers_Positive] 
CHECK ([MaxConcurrentUsers] >= 1)
GO
ALTER TABLE [dbo].[SubscriptionPlans] CHECK CONSTRAINT [CK_SubscriptionPlans_MaxConcurrentUsers_Positive]
GO

ALTER TABLE [dbo].[SubscriptionPlans] WITH CHECK ADD 
CONSTRAINT [CK_SubscriptionPlans_GracePeriod_NonNegative] 
CHECK ([GracePeriodDays] >= 0)
GO
ALTER TABLE [dbo].[SubscriptionPlans] CHECK CONSTRAINT [CK_SubscriptionPlans_GracePeriod_NonNegative]
GO

ALTER TABLE [dbo].[SubscriptionPlans] WITH CHECK ADD 
CONSTRAINT [CK_SubscriptionPlans_ExpirationDate_Future] 
CHECK ([ExpirationDate] IS NULL OR [ExpirationDate] > GETUTCDATE())
GO
ALTER TABLE [dbo].[SubscriptionPlans] CHECK CONSTRAINT [CK_SubscriptionPlans_ExpirationDate_Future]
GO

ALTER TABLE [dbo].[SubscriptionPlans] WITH CHECK ADD 
CONSTRAINT [CK_SubscriptionPlans_EffectiveDate_PastOrPresent] 
CHECK ([EffectiveDate] IS NULL OR [EffectiveDate] <= GETUTCDATE())
GO
ALTER TABLE [dbo].[SubscriptionPlans] CHECK CONSTRAINT [CK_SubscriptionPlans_EffectiveDate_PastOrPresent]
GO

ALTER TABLE [dbo].[SubscriptionPlans] WITH CHECK ADD 
CONSTRAINT [CK_SubscriptionPlans_DiscountValidUntil_Future] 
CHECK ([DiscountValidUntil] IS NULL OR [DiscountValidUntil] > GETUTCDATE())
GO
ALTER TABLE [dbo].[SubscriptionPlans] CHECK CONSTRAINT [CK_SubscriptionPlans_DiscountValidUntil_Future]
GO

ALTER TABLE [dbo].[SubscriptionPlans] WITH CHECK ADD 
CONSTRAINT [CK_SubscriptionPlans_DiscountedPrice_Positive] 
CHECK ([DiscountedPrice] IS NULL OR [DiscountedPrice] > 0)
GO
ALTER TABLE [dbo].[SubscriptionPlans] CHECK CONSTRAINT [CK_SubscriptionPlans_DiscountedPrice_Positive]
GO

-- =====================================================
-- 6. INDEXES FOR MASTER TABLES
-- =====================================================

-- MasterPrivilegeTypes indexes
CREATE NONCLUSTERED INDEX [IX_MasterPrivilegeTypes_Name] 
ON [dbo].[MasterPrivilegeTypes] ([Name])
GO

CREATE NONCLUSTERED INDEX [IX_MasterPrivilegeTypes_SortOrder] 
ON [dbo].[MasterPrivilegeTypes] ([SortOrder])
GO

CREATE NONCLUSTERED INDEX [IX_MasterPrivilegeTypes_IsActive] 
ON [dbo].[MasterPrivilegeTypes] ([IsActive])
GO

-- MasterCurrencies indexes
CREATE NONCLUSTERED INDEX [IX_MasterCurrencies_Code] 
ON [dbo].[MasterCurrencies] ([Code])
GO

CREATE NONCLUSTERED INDEX [IX_MasterCurrencies_Name] 
ON [dbo].[MasterCurrencies] ([Name])
GO

CREATE NONCLUSTERED INDEX [IX_MasterCurrencies_SortOrder] 
ON [dbo].[MasterCurrencies] ([SortOrder])
GO

CREATE NONCLUSTERED INDEX [IX_MasterCurrencies_IsActive] 
ON [dbo].[MasterCurrencies] ([IsActive])
GO

-- MasterBillingCycles indexes
CREATE NONCLUSTERED INDEX [IX_MasterBillingCycles_Name] 
ON [dbo].[MasterBillingCycles] ([Name])
GO

CREATE NONCLUSTERED INDEX [IX_MasterBillingCycles_DurationInDays] 
ON [dbo].[MasterBillingCycles] ([DurationInDays])
GO

CREATE NONCLUSTERED INDEX [IX_MasterBillingCycles_SortOrder] 
ON [dbo].[MasterBillingCycles] ([SortOrder])
GO

CREATE NONCLUSTERED INDEX [IX_MasterBillingCycles_IsActive] 
ON [dbo].[MasterBillingCycles] ([IsActive])
GO

-- Privileges indexes
CREATE NONCLUSTERED INDEX [IX_Privileges_Name] 
ON [dbo].[Privileges] ([Name])
GO

CREATE NONCLUSTERED INDEX [IX_Privileges_PrivilegeTypeId] 
ON [dbo].[Privileges] ([PrivilegeTypeId])
GO

CREATE NONCLUSTERED INDEX [IX_Privileges_IsActive] 
ON [dbo].[Privileges] ([IsActive])
GO

-- SubscriptionPlans indexes
CREATE NONCLUSTERED INDEX [IX_SubscriptionPlans_Name] 
ON [dbo].[SubscriptionPlans] ([Name])
GO

CREATE NONCLUSTERED INDEX [IX_SubscriptionPlans_BillingCycleId] 
ON [dbo].[SubscriptionPlans] ([BillingCycleId])
GO

CREATE NONCLUSTERED INDEX [IX_SubscriptionPlans_CurrencyId] 
ON [dbo].[SubscriptionPlans] ([CurrencyId])
GO

CREATE NONCLUSTERED INDEX [IX_SubscriptionPlans_CategoryId] 
ON [dbo].[SubscriptionPlans] ([CategoryId])
GO

CREATE NONCLUSTERED INDEX [IX_SubscriptionPlans_IsActive] 
ON [dbo].[SubscriptionPlans] ([IsActive])
GO

CREATE NONCLUSTERED INDEX [IX_SubscriptionPlans_DisplayOrder] 
ON [dbo].[SubscriptionPlans] ([DisplayOrder])
GO

CREATE NONCLUSTERED INDEX [IX_SubscriptionPlans_StripeProductId] 
ON [dbo].[SubscriptionPlans] ([StripeProductId])
GO

CREATE NONCLUSTERED INDEX [IX_SubscriptionPlans_PlanType] 
ON [dbo].[SubscriptionPlans] ([PlanType])
GO

PRINT 'Master tables and core entities created successfully!'
