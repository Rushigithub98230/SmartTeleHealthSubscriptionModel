-- =====================================================
-- CORRECTED SUBSCRIPTIONPLANS TABLE CREATION SCRIPT
-- =====================================================
-- This script creates the SubscriptionPlans table with proper constraints
-- and fixes the issues found in the original script
-- =====================================================

-- Drop table if it exists (for testing purposes)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SubscriptionPlans]') AND type in (N'U'))
    DROP TABLE [dbo].[SubscriptionPlans]
GO

-- Create the SubscriptionPlans table
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
    [MaxConcurrentUsers] [int] NOT NULL, -- ADDED: Missing field from original
    [GracePeriodDays] [int] NOT NULL,    -- ADDED: Missing field from original
    [Features] [nvarchar](1000) NULL,
    [Terms] [nvarchar](500) NULL,
    [EffectiveDate] [datetime2](7) NULL,
    [ExpirationDate] [datetime2](7) NULL,
    [IsActive] [bit] NOT NULL,
    [CreatedDate] [datetime2](7) NOT NULL,
    [UpdatedDate] [datetime2](7) NULL,
    [IsDeleted] [bit] NOT NULL,
    [CreatedBy] [int] NOT NULL,
    [UpdatedBy] [int] NULL,
    [DeletedBy] [int] NULL,
    [DeletedDate] [datetime2](7) NULL,
    [PlanType] [nvarchar](50) NOT NULL,
    [DurationMonths] [int] NULL,
    [UsagePeriodId] [uniqueidentifier] NULL,
    
    -- Primary Key Constraint
    CONSTRAINT [PK_SubscriptionPlans] PRIMARY KEY CLUSTERED 
    (
        [Id] ASC
    ) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, 
            ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

-- =====================================================
-- ADD DEFAULT VALUES
-- =====================================================
ALTER TABLE [dbo].[SubscriptionPlans] ADD DEFAULT (newid()) FOR [Id]
GO

ALTER TABLE [dbo].[SubscriptionPlans] ADD DEFAULT ((0)) FOR [IsFeatured]
GO

ALTER TABLE [dbo].[SubscriptionPlans] ADD DEFAULT ((0)) FOR [IsTrialAllowed]
GO

ALTER TABLE [dbo].[SubscriptionPlans] ADD DEFAULT ((0)) FOR [TrialDurationInDays]
GO

ALTER TABLE [dbo].[SubscriptionPlans] ADD DEFAULT ((0)) FOR [IsMostPopular]
GO

ALTER TABLE [dbo].[SubscriptionPlans] ADD DEFAULT ((0)) FOR [IsTrending]
GO

ALTER TABLE [dbo].[SubscriptionPlans] ADD DEFAULT ((0)) FOR [DisplayOrder]
GO

ALTER TABLE [dbo].[SubscriptionPlans] ADD DEFAULT ((10)) FOR [MessagingCount]
GO

ALTER TABLE [dbo].[SubscriptionPlans] ADD DEFAULT ((1)) FOR [IncludesMedicationDelivery]
GO

ALTER TABLE [dbo].[SubscriptionPlans] ADD DEFAULT ((1)) FOR [IncludesFollowUpCare]
GO

ALTER TABLE [dbo].[SubscriptionPlans] ADD DEFAULT ((30)) FOR [DeliveryFrequencyDays]
GO

ALTER TABLE [dbo].[SubscriptionPlans] ADD DEFAULT ((90)) FOR [MaxPauseDurationDays]
GO

-- ADDED: Defaults for missing fields
ALTER TABLE [dbo].[SubscriptionPlans] ADD DEFAULT ((1)) FOR [MaxConcurrentUsers]
GO

ALTER TABLE [dbo].[SubscriptionPlans] ADD DEFAULT ((0)) FOR [GracePeriodDays]
GO

ALTER TABLE [dbo].[SubscriptionPlans] ADD DEFAULT ((1)) FOR [IsActive]
GO

ALTER TABLE [dbo].[SubscriptionPlans] ADD DEFAULT (getutcdate()) FOR [CreatedDate]
GO

ALTER TABLE [dbo].[SubscriptionPlans] ADD DEFAULT ((0)) FOR [IsDeleted]
GO

ALTER TABLE [dbo].[SubscriptionPlans] ADD DEFAULT ((0)) FOR [CreatedBy]
GO

ALTER TABLE [dbo].[SubscriptionPlans] ADD DEFAULT ('Standard') FOR [PlanType]
GO

-- =====================================================
-- ADD FOREIGN KEY CONSTRAINTS
-- =====================================================

-- Foreign Key to Categories
ALTER TABLE [dbo].[SubscriptionPlans] WITH CHECK ADD 
CONSTRAINT [FK_SubscriptionPlans_Categories_CategoryId] 
FOREIGN KEY([CategoryId]) REFERENCES [dbo].[Categories] ([Id])
GO

ALTER TABLE [dbo].[SubscriptionPlans] CHECK CONSTRAINT [FK_SubscriptionPlans_Categories_CategoryId]
GO

-- Foreign Key to MasterBillingCycles
ALTER TABLE [dbo].[SubscriptionPlans] WITH CHECK ADD 
CONSTRAINT [FK_SubscriptionPlans_MasterBillingCycles_BillingCycleId] 
FOREIGN KEY([BillingCycleId]) REFERENCES [dbo].[MasterBillingCycles] ([Id])
GO

ALTER TABLE [dbo].[SubscriptionPlans] CHECK CONSTRAINT [FK_SubscriptionPlans_MasterBillingCycles_BillingCycleId]
GO

-- Foreign Key to MasterCurrencies
ALTER TABLE [dbo].[SubscriptionPlans] WITH CHECK ADD 
CONSTRAINT [FK_SubscriptionPlans_MasterCurrencies_CurrencyId] 
FOREIGN KEY([CurrencyId]) REFERENCES [dbo].[MasterCurrencies] ([Id])
GO

ALTER TABLE [dbo].[SubscriptionPlans] CHECK CONSTRAINT [FK_SubscriptionPlans_MasterCurrencies_CurrencyId]
GO

-- Foreign Key to User (CreatedBy)
ALTER TABLE [dbo].[SubscriptionPlans] WITH CHECK ADD 
CONSTRAINT [FK_SubscriptionPlans_User_CreatedBy] 
FOREIGN KEY([CreatedBy]) REFERENCES [dbo].[User] ([UserID])
GO

ALTER TABLE [dbo].[SubscriptionPlans] CHECK CONSTRAINT [FK_SubscriptionPlans_User_CreatedBy]
GO

-- Foreign Key to User (UpdatedBy)
ALTER TABLE [dbo].[SubscriptionPlans] WITH CHECK ADD 
CONSTRAINT [FK_SubscriptionPlans_User_UpdatedBy] 
FOREIGN KEY([UpdatedBy]) REFERENCES [dbo].[User] ([UserID])
GO

ALTER TABLE [dbo].[SubscriptionPlans] CHECK CONSTRAINT [FK_SubscriptionPlans_User_UpdatedBy]
GO

-- Foreign Key to User (DeletedBy)
ALTER TABLE [dbo].[SubscriptionPlans] WITH CHECK ADD 
CONSTRAINT [FK_SubscriptionPlans_User_DeletedBy] 
FOREIGN KEY([DeletedBy]) REFERENCES [dbo].[User] ([UserID])
GO

ALTER TABLE [dbo].[SubscriptionPlans] CHECK CONSTRAINT [FK_SubscriptionPlans_User_DeletedBy]
GO

-- =====================================================
-- ADD CHECK CONSTRAINTS (CORRECTED)
-- =====================================================

-- CORRECTED: Price must be greater than 0 (not >= 0)
ALTER TABLE [dbo].[SubscriptionPlans] WITH CHECK ADD 
CONSTRAINT [CK_SubscriptionPlans_Price_Positive] 
CHECK (([Price] > 0))
GO

ALTER TABLE [dbo].[SubscriptionPlans] CHECK CONSTRAINT [CK_SubscriptionPlans_Price_Positive]
GO

-- ADDED: Trial duration must be non-negative
ALTER TABLE [dbo].[SubscriptionPlans] WITH CHECK ADD 
CONSTRAINT [CK_SubscriptionPlans_TrialDuration_NonNegative] 
CHECK (([TrialDurationInDays] >= 0))
GO

ALTER TABLE [dbo].[SubscriptionPlans] CHECK CONSTRAINT [CK_SubscriptionPlans_TrialDuration_NonNegative]
GO

-- ADDED: Messaging count must be non-negative
ALTER TABLE [dbo].[SubscriptionPlans] WITH CHECK ADD 
CONSTRAINT [CK_SubscriptionPlans_MessagingCount_NonNegative] 
CHECK (([MessagingCount] >= 0))
GO

ALTER TABLE [dbo].[SubscriptionPlans] CHECK CONSTRAINT [CK_SubscriptionPlans_MessagingCount_NonNegative]
GO

-- ADDED: Delivery frequency must be at least 1 day
ALTER TABLE [dbo].[SubscriptionPlans] WITH CHECK ADD 
CONSTRAINT [CK_SubscriptionPlans_DeliveryFrequency_Positive] 
CHECK (([DeliveryFrequencyDays] >= 1))
GO

ALTER TABLE [dbo].[SubscriptionPlans] CHECK CONSTRAINT [CK_SubscriptionPlans_DeliveryFrequency_Positive]
GO

-- ADDED: Max pause duration must be non-negative
ALTER TABLE [dbo].[SubscriptionPlans] WITH CHECK ADD 
CONSTRAINT [CK_SubscriptionPlans_MaxPauseDuration_NonNegative] 
CHECK (([MaxPauseDurationDays] >= 0))
GO

ALTER TABLE [dbo].[SubscriptionPlans] CHECK CONSTRAINT [CK_SubscriptionPlans_MaxPauseDuration_NonNegative]
GO

-- ADDED: Max concurrent users must be at least 1
ALTER TABLE [dbo].[SubscriptionPlans] WITH CHECK ADD 
CONSTRAINT [CK_SubscriptionPlans_MaxConcurrentUsers_Positive] 
CHECK (([MaxConcurrentUsers] >= 1))
GO

ALTER TABLE [dbo].[SubscriptionPlans] CHECK CONSTRAINT [CK_SubscriptionPlans_MaxConcurrentUsers_Positive]
GO

-- ADDED: Grace period must be non-negative
ALTER TABLE [dbo].[SubscriptionPlans] WITH CHECK ADD 
CONSTRAINT [CK_SubscriptionPlans_GracePeriod_NonNegative] 
CHECK (([GracePeriodDays] >= 0))
GO

ALTER TABLE [dbo].[SubscriptionPlans] CHECK CONSTRAINT [CK_SubscriptionPlans_GracePeriod_NonNegative]
GO

-- ADDED: Expiration date must be in the future (if provided)
ALTER TABLE [dbo].[SubscriptionPlans] WITH CHECK ADD 
CONSTRAINT [CK_SubscriptionPlans_ExpirationDate_Future] 
CHECK (([ExpirationDate] IS NULL OR [ExpirationDate] > GETUTCDATE()))
GO

ALTER TABLE [dbo].[SubscriptionPlans] CHECK CONSTRAINT [CK_SubscriptionPlans_ExpirationDate_Future]
GO

-- ADDED: Effective date must be in the past or present (if provided)
ALTER TABLE [dbo].[SubscriptionPlans] WITH CHECK ADD 
CONSTRAINT [CK_SubscriptionPlans_EffectiveDate_PastOrPresent] 
CHECK (([EffectiveDate] IS NULL OR [EffectiveDate] <= GETUTCDATE()))
GO

ALTER TABLE [dbo].[SubscriptionPlans] CHECK CONSTRAINT [CK_SubscriptionPlans_EffectiveDate_PastOrPresent]
GO

-- ADDED: Discount valid until must be in the future (if provided)
ALTER TABLE [dbo].[SubscriptionPlans] WITH CHECK ADD 
CONSTRAINT [CK_SubscriptionPlans_DiscountValidUntil_Future] 
CHECK (([DiscountValidUntil] IS NULL OR [DiscountValidUntil] > GETUTCDATE()))
GO

ALTER TABLE [dbo].[SubscriptionPlans] CHECK CONSTRAINT [CK_SubscriptionPlans_DiscountValidUntil_Future]
GO

-- ADDED: Discounted price must be positive (if provided)
ALTER TABLE [dbo].[SubscriptionPlans] WITH CHECK ADD 
CONSTRAINT [CK_SubscriptionPlans_DiscountedPrice_Positive] 
CHECK (([DiscountedPrice] IS NULL OR [DiscountedPrice] > 0))
GO

ALTER TABLE [dbo].[SubscriptionPlans] CHECK CONSTRAINT [CK_SubscriptionPlans_DiscountedPrice_Positive]
GO

-- =====================================================
-- CREATE INDEXES FOR PERFORMANCE
-- =====================================================

-- Index on BillingCycleId for foreign key lookups
CREATE NONCLUSTERED INDEX [IX_SubscriptionPlans_BillingCycleId] 
ON [dbo].[SubscriptionPlans] ([BillingCycleId])
GO

-- Index on CurrencyId for foreign key lookups
CREATE NONCLUSTERED INDEX [IX_SubscriptionPlans_CurrencyId] 
ON [dbo].[SubscriptionPlans] ([CurrencyId])
GO

-- Index on CategoryId for foreign key lookups
CREATE NONCLUSTERED INDEX [IX_SubscriptionPlans_CategoryId] 
ON [dbo].[SubscriptionPlans] ([CategoryId])
GO

-- Index on IsActive for filtering active plans
CREATE NONCLUSTERED INDEX [IX_SubscriptionPlans_IsActive] 
ON [dbo].[SubscriptionPlans] ([IsActive])
GO

-- Index on DisplayOrder for sorting
CREATE NONCLUSTERED INDEX [IX_SubscriptionPlans_DisplayOrder] 
ON [dbo].[SubscriptionPlans] ([DisplayOrder])
GO

-- Index on StripeProductId for Stripe integration
CREATE NONCLUSTERED INDEX [IX_SubscriptionPlans_StripeProductId] 
ON [dbo].[SubscriptionPlans] ([StripeProductId])
GO

-- Index on PlanType for filtering by plan type
CREATE NONCLUSTERED INDEX [IX_SubscriptionPlans_PlanType] 
ON [dbo].[SubscriptionPlans] ([PlanType])
GO

-- =====================================================
-- VERIFICATION QUERIES
-- =====================================================
PRINT 'SubscriptionPlans table created successfully!'
PRINT 'Verifying table structure...'

-- Check if table exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SubscriptionPlans]') AND type in (N'U'))
    PRINT '✓ Table [dbo].[SubscriptionPlans] exists'
ELSE
    PRINT '✗ Table [dbo].[SubscriptionPlans] does not exist'

-- Check constraints
SELECT 
    'Check Constraints' as ConstraintType,
    COUNT(*) as Count
FROM sys.check_constraints 
WHERE parent_object_id = OBJECT_ID('dbo.SubscriptionPlans')

UNION ALL

-- Check foreign keys
SELECT 
    'Foreign Keys' as ConstraintType,
    COUNT(*) as Count
FROM sys.foreign_keys 
WHERE parent_object_id = OBJECT_ID('dbo.SubscriptionPlans')

UNION ALL

-- Check indexes
SELECT 
    'Indexes' as ConstraintType,
    COUNT(*) as Count
FROM sys.indexes 
WHERE object_id = OBJECT_ID('dbo.SubscriptionPlans') 
    AND name IS NOT NULL

PRINT 'Table creation completed successfully!'
