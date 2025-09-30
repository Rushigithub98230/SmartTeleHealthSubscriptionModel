-- =====================================================
-- RESET SUBSCRIPTION PLAN TABLES
-- =====================================================
-- This script drops and recreates all subscription plan related tables
-- Use this script to completely reset the subscription plan system
-- =====================================================

-- =====================================================
-- STEP 1: DROP EXISTING TABLES
-- =====================================================
PRINT 'Step 1: Dropping existing subscription plan tables...';

-- Disable foreign key checks temporarily
SET FOREIGN_KEY_CHECKS = 0;

-- Drop tables in reverse dependency order
DROP TABLE IF EXISTS UserSubscriptionPrivilegeUsage;
DROP TABLE IF EXISTS PrivilegeUsageHistory;
DROP TABLE IF EXISTS SubscriptionPlanPrivilege;
DROP TABLE IF EXISTS Subscription;
DROP TABLE IF EXISTS SubscriptionPlan;
DROP TABLE IF EXISTS ServiceConstraint;

-- Re-enable foreign key checks
SET FOREIGN_KEY_CHECKS = 1;

PRINT 'Step 1 Complete: All existing tables dropped.';

-- =====================================================
-- STEP 2: CREATE NEW TABLES
-- =====================================================
PRINT 'Step 2: Creating new subscription plan tables...';

-- =====================================================
-- 1. CREATE SUBSCRIPTIONPLAN TABLE
-- =====================================================
CREATE TABLE SubscriptionPlan (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    
    -- Basic Plan Information
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500),
    ShortDescription NVARCHAR(200),
    
    -- Pricing Information
    Price DECIMAL(18,2) NOT NULL CHECK (Price > 0),
    DiscountedPrice DECIMAL(18,2),
    DiscountValidUntil DATETIME2,
    
    -- Foreign Keys
    BillingCycleId UNIQUEIDENTIFIER NOT NULL,
    CurrencyId UNIQUEIDENTIFIER NOT NULL,
    CategoryId UNIQUEIDENTIFIER,
    
    -- Trial Configuration
    IsTrialAllowed BIT NOT NULL DEFAULT 0,
    TrialDurationInDays INT NOT NULL DEFAULT 0 CHECK (TrialDurationInDays >= 0),
    
    -- Marketing and Display Properties
    IsFeatured BIT NOT NULL DEFAULT 0,
    IsMostPopular BIT NOT NULL DEFAULT 0,
    IsTrending BIT NOT NULL DEFAULT 0,
    DisplayOrder INT NOT NULL DEFAULT 0,
    
    -- Plan Features and Limits
    MessagingCount INT NOT NULL DEFAULT 10 CHECK (MessagingCount >= 0),
    IncludesMedicationDelivery BIT NOT NULL DEFAULT 1,
    IncludesFollowUpCare BIT NOT NULL DEFAULT 1,
    DeliveryFrequencyDays INT NOT NULL DEFAULT 30 CHECK (DeliveryFrequencyDays >= 1),
    MaxPauseDurationDays INT NOT NULL DEFAULT 90 CHECK (MaxPauseDurationDays >= 0),
    MaxConcurrentUsers INT NOT NULL DEFAULT 1 CHECK (MaxConcurrentUsers >= 1),
    GracePeriodDays INT NOT NULL DEFAULT 0 CHECK (GracePeriodDays >= 0),
    
    -- Plan Status
    IsActive BIT NOT NULL DEFAULT 1,
    
    -- Plan Metadata
    Features NVARCHAR(1000),
    Terms NVARCHAR(500),
    EffectiveDate DATETIME2,
    ExpirationDate DATETIME2,
    
    -- Stripe Integration Fields
    StripeProductId NVARCHAR(100),
    StripeMonthlyPriceId NVARCHAR(100),
    StripeQuarterlyPriceId NVARCHAR(100),
    StripeAnnualPriceId NVARCHAR(100),
    
    -- Audit Fields
    CreatedBy INT,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedBy INT,
    UpdatedDate DATETIME2,
    IsDeleted BIT NOT NULL DEFAULT 0,
    
    -- Constraints
    CONSTRAINT CK_SubscriptionPlan_ExpirationDate CHECK (ExpirationDate IS NULL OR ExpirationDate > GETUTCDATE()),
    CONSTRAINT CK_SubscriptionPlan_EffectiveDate CHECK (EffectiveDate IS NULL OR EffectiveDate <= GETUTCDATE()),
    CONSTRAINT CK_SubscriptionPlan_DiscountValidUntil CHECK (DiscountValidUntil IS NULL OR DiscountValidUntil > GETUTCDATE())
);

-- =====================================================
-- 2. CREATE SUBSCRIPTIONPLANPRIVILEGE TABLE
-- =====================================================
CREATE TABLE SubscriptionPlanPrivilege (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    
    -- Foreign Keys
    SubscriptionPlanId UNIQUEIDENTIFIER NOT NULL,
    PrivilegeId UNIQUEIDENTIFIER NOT NULL,
    UsagePeriodId UNIQUEIDENTIFIER NOT NULL,
    
    -- Privilege Configuration
    Value INT NOT NULL CHECK (Value >= -1), -- -1 = unlimited, 0 = disabled, >0 = limited
    DurationMonths INT NOT NULL DEFAULT 1 CHECK (DurationMonths >= 1),
    Description NVARCHAR(500),
    EffectiveDate DATETIME2,
    ExpirationDate DATETIME2,
    
    -- Time-based Limits
    DailyLimit INT CHECK (DailyLimit >= 0),
    WeeklyLimit INT CHECK (WeeklyLimit >= 0),
    MonthlyLimit INT CHECK (MonthlyLimit >= 0),
    
    -- Unit Cost for Overage Billing
    UnitCost DECIMAL(18,2) NOT NULL DEFAULT 0 CHECK (UnitCost >= 0),
    
    -- Status
    IsActive BIT NOT NULL DEFAULT 1,
    
    -- Audit Fields
    CreatedBy INT,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedBy INT,
    UpdatedDate DATETIME2,
    IsDeleted BIT NOT NULL DEFAULT 0,
    
    -- Constraints
    CONSTRAINT CK_SubscriptionPlanPrivilege_ExpirationDate CHECK (ExpirationDate IS NULL OR ExpirationDate > GETUTCDATE()),
    CONSTRAINT CK_SubscriptionPlanPrivilege_EffectiveDate CHECK (EffectiveDate IS NULL OR EffectiveDate <= GETUTCDATE()),
    
    -- Foreign Key Constraints
    CONSTRAINT FK_SubscriptionPlanPrivilege_SubscriptionPlan 
        FOREIGN KEY (SubscriptionPlanId) REFERENCES SubscriptionPlan(Id) ON DELETE CASCADE,
    CONSTRAINT FK_SubscriptionPlanPrivilege_Privilege 
        FOREIGN KEY (PrivilegeId) REFERENCES Privilege(Id) ON DELETE CASCADE,
    CONSTRAINT FK_SubscriptionPlanPrivilege_UsagePeriod 
        FOREIGN KEY (UsagePeriodId) REFERENCES MasterBillingCycles(Id) ON DELETE CASCADE
);

-- =====================================================
-- 3. CREATE SUBSCRIPTION TABLE
-- =====================================================
CREATE TABLE Subscription (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    
    -- Foreign Keys
    UserId INT NOT NULL,
    SubscriptionPlanId UNIQUEIDENTIFIER NOT NULL,
    BillingCycleId UNIQUEIDENTIFIER NOT NULL,
    ProviderId INT,
    
    -- Subscription Details
    Status NVARCHAR(50) NOT NULL DEFAULT 'Active',
    StartDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    EndDate DATETIME2,
    NextBillingDate DATETIME2,
    LastBillingDate DATETIME2,
    
    -- Trial Information
    IsTrial BIT NOT NULL DEFAULT 0,
    TrialStartDate DATETIME2,
    TrialEndDate DATETIME2,
    
    -- Billing Information
    Amount DECIMAL(18,2) NOT NULL,
    Currency NVARCHAR(3) NOT NULL DEFAULT 'USD',
    BillingFrequency NVARCHAR(20) NOT NULL DEFAULT 'Monthly',
    
    -- Stripe Integration
    StripeSubscriptionId NVARCHAR(100),
    StripeCustomerId NVARCHAR(100),
    StripePriceId NVARCHAR(100),
    
    -- Subscription Management
    IsCancelled BIT NOT NULL DEFAULT 0,
    CancelledDate DATETIME2,
    CancellationReason NVARCHAR(500),
    IsPaused BIT NOT NULL DEFAULT 0,
    PausedDate DATETIME2,
    PauseReason NVARCHAR(500),
    
    -- Usage Tracking
    TotalUsageCount INT NOT NULL DEFAULT 0,
    LastUsageDate DATETIME2,
    
    -- Audit Fields
    CreatedBy INT,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedBy INT,
    UpdatedDate DATETIME2,
    IsDeleted BIT NOT NULL DEFAULT 0,
    
    -- Constraints
    CONSTRAINT CK_Subscription_EndDate CHECK (EndDate IS NULL OR EndDate > StartDate),
    CONSTRAINT CK_Subscription_TrialEndDate CHECK (TrialEndDate IS NULL OR TrialEndDate > TrialStartDate),
    CONSTRAINT CK_Subscription_Amount CHECK (Amount > 0),
    
    -- Foreign Key Constraints
    CONSTRAINT FK_Subscription_User 
        FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Subscription_SubscriptionPlan 
        FOREIGN KEY (SubscriptionPlanId) REFERENCES SubscriptionPlan(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Subscription_BillingCycle 
        FOREIGN KEY (BillingCycleId) REFERENCES MasterBillingCycles(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Subscription_Provider 
        FOREIGN KEY (ProviderId) REFERENCES Providers(Id) ON DELETE SET NULL
);

-- =====================================================
-- 4. CREATE USERSUBSCRIPTIONPRIVILEGEUSAGE TABLE
-- =====================================================
CREATE TABLE UserSubscriptionPrivilegeUsage (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    
    -- Foreign Keys
    SubscriptionId UNIQUEIDENTIFIER NOT NULL,
    SubscriptionPlanPrivilegeId UNIQUEIDENTIFIER NOT NULL,
    PrivilegeId UNIQUEIDENTIFIER NOT NULL,
    UserId INT NOT NULL,
    
    -- Usage Information
    UsageDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UsageCount INT NOT NULL DEFAULT 1 CHECK (UsageCount > 0),
    UsageType NVARCHAR(50) NOT NULL DEFAULT 'Standard',
    
    -- Time-based Tracking
    UsageYear INT NOT NULL,
    UsageMonth INT NOT NULL,
    UsageWeek INT NOT NULL,
    UsageDay INT NOT NULL,
    
    -- Additional Information
    Description NVARCHAR(500),
    Metadata NVARCHAR(MAX), -- JSON data for additional usage details
    
    -- Audit Fields
    CreatedBy INT,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedBy INT,
    UpdatedDate DATETIME2,
    IsDeleted BIT NOT NULL DEFAULT 0,
    
    -- Constraints
    CONSTRAINT CK_UserSubscriptionPrivilegeUsage_UsageYear CHECK (UsageYear >= 2020 AND UsageYear <= 2100),
    CONSTRAINT CK_UserSubscriptionPrivilegeUsage_UsageMonth CHECK (UsageMonth >= 1 AND UsageMonth <= 12),
    CONSTRAINT CK_UserSubscriptionPrivilegeUsage_UsageWeek CHECK (UsageWeek >= 1 AND UsageWeek <= 53),
    CONSTRAINT CK_UserSubscriptionPrivilegeUsage_UsageDay CHECK (UsageDay >= 1 AND UsageDay <= 31),
    
    -- Foreign Key Constraints
    CONSTRAINT FK_UserSubscriptionPrivilegeUsage_Subscription 
        FOREIGN KEY (SubscriptionId) REFERENCES Subscription(Id) ON DELETE CASCADE,
    CONSTRAINT FK_UserSubscriptionPrivilegeUsage_SubscriptionPlanPrivilege 
        FOREIGN KEY (SubscriptionPlanPrivilegeId) REFERENCES SubscriptionPlanPrivilege(Id) ON DELETE CASCADE,
    CONSTRAINT FK_UserSubscriptionPrivilegeUsage_Privilege 
        FOREIGN KEY (PrivilegeId) REFERENCES Privilege(Id) ON DELETE CASCADE,
    CONSTRAINT FK_UserSubscriptionPrivilegeUsage_User 
        FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

-- =====================================================
-- 5. CREATE PRIVILEGEUSAGEHISTORY TABLE
-- =====================================================
CREATE TABLE PrivilegeUsageHistory (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    
    -- Foreign Keys
    SubscriptionId UNIQUEIDENTIFIER NOT NULL,
    SubscriptionPlanPrivilegeId UNIQUEIDENTIFIER NOT NULL,
    PrivilegeId UNIQUEIDENTIFIER NOT NULL,
    UserId INT NOT NULL,
    
    -- Usage History Information
    UsageDate DATETIME2 NOT NULL,
    UsageCount INT NOT NULL DEFAULT 1 CHECK (UsageCount > 0),
    UsageType NVARCHAR(50) NOT NULL DEFAULT 'Standard',
    
    -- Time Period Information
    PeriodStartDate DATETIME2 NOT NULL,
    PeriodEndDate DATETIME2 NOT NULL,
    PeriodType NVARCHAR(20) NOT NULL, -- Daily, Weekly, Monthly, Yearly
    
    -- Usage Limits and Tracking
    PeriodLimit INT,
    PeriodUsed INT NOT NULL DEFAULT 0,
    PeriodRemaining INT,
    IsOverLimit BIT NOT NULL DEFAULT 0,
    OverageCount INT NOT NULL DEFAULT 0,
    OverageCost DECIMAL(18,2) NOT NULL DEFAULT 0,
    
    -- Additional Information
    Description NVARCHAR(500),
    Metadata NVARCHAR(MAX), -- JSON data for additional usage details
    
    -- Audit Fields
    CreatedBy INT,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedBy INT,
    UpdatedDate DATETIME2,
    IsDeleted BIT NOT NULL DEFAULT 0,
    
    -- Constraints
    CONSTRAINT CK_PrivilegeUsageHistory_PeriodEndDate CHECK (PeriodEndDate > PeriodStartDate),
    CONSTRAINT CK_PrivilegeUsageHistory_PeriodUsed CHECK (PeriodUsed >= 0),
    CONSTRAINT CK_PrivilegeUsageHistory_PeriodRemaining CHECK (PeriodRemaining IS NULL OR PeriodRemaining >= 0),
    CONSTRAINT CK_PrivilegeUsageHistory_OverageCount CHECK (OverageCount >= 0),
    CONSTRAINT CK_PrivilegeUsageHistory_OverageCost CHECK (OverageCost >= 0),
    
    -- Foreign Key Constraints
    CONSTRAINT FK_PrivilegeUsageHistory_Subscription 
        FOREIGN KEY (SubscriptionId) REFERENCES Subscription(Id) ON DELETE CASCADE,
    CONSTRAINT FK_PrivilegeUsageHistory_SubscriptionPlanPrivilege 
        FOREIGN KEY (SubscriptionPlanPrivilegeId) REFERENCES SubscriptionPlanPrivilege(Id) ON DELETE CASCADE,
    CONSTRAINT FK_PrivilegeUsageHistory_Privilege 
        FOREIGN KEY (PrivilegeId) REFERENCES Privilege(Id) ON DELETE CASCADE,
    CONSTRAINT FK_PrivilegeUsageHistory_User 
        FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

-- =====================================================
-- 6. CREATE SERVICECONSTRAINT TABLE
-- =====================================================
CREATE TABLE ServiceConstraint (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    
    -- Service Information
    ServiceName NVARCHAR(100) NOT NULL,
    ConstraintType NVARCHAR(50) NOT NULL, -- Unlimited, SessionCount, TimeBased, Hybrid
    Description NVARCHAR(500),
    
    -- Constraint Configuration
    MaxSessions INT,
    MaxDurationMinutes INT,
    MaxConcurrentUsers INT,
    MaxDailyUsage INT,
    MaxWeeklyUsage INT,
    MaxMonthlyUsage INT,
    
    -- Subscription Plan Reference
    SubscriptionPlanId UNIQUEIDENTIFIER,
    
    -- Constraint Rules
    IsActive BIT NOT NULL DEFAULT 1,
    EffectiveDate DATETIME2,
    ExpirationDate DATETIME2,
    
    -- Additional Configuration
    Configuration NVARCHAR(MAX), -- JSON data for complex constraint rules
    
    -- Audit Fields
    CreatedBy INT,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedBy INT,
    UpdatedDate DATETIME2,
    IsDeleted BIT NOT NULL DEFAULT 0,
    
    -- Constraints
    CONSTRAINT CK_ServiceConstraint_ExpirationDate CHECK (ExpirationDate IS NULL OR ExpirationDate > GETUTCDATE()),
    CONSTRAINT CK_ServiceConstraint_EffectiveDate CHECK (EffectiveDate IS NULL OR EffectiveDate <= GETUTCDATE()),
    CONSTRAINT CK_ServiceConstraint_MaxSessions CHECK (MaxSessions IS NULL OR MaxSessions > 0),
    CONSTRAINT CK_ServiceConstraint_MaxDurationMinutes CHECK (MaxDurationMinutes IS NULL OR MaxDurationMinutes > 0),
    CONSTRAINT CK_ServiceConstraint_MaxConcurrentUsers CHECK (MaxConcurrentUsers IS NULL OR MaxConcurrentUsers > 0),
    CONSTRAINT CK_ServiceConstraint_MaxDailyUsage CHECK (MaxDailyUsage IS NULL OR MaxDailyUsage > 0),
    CONSTRAINT CK_ServiceConstraint_MaxWeeklyUsage CHECK (MaxWeeklyUsage IS NULL OR MaxWeeklyUsage > 0),
    CONSTRAINT CK_ServiceConstraint_MaxMonthlyUsage CHECK (MaxMonthlyUsage IS NULL OR MaxMonthlyUsage > 0),
    
    -- Foreign Key Constraints
    CONSTRAINT FK_ServiceConstraint_SubscriptionPlan 
        FOREIGN KEY (SubscriptionPlanId) REFERENCES SubscriptionPlan(Id) ON DELETE CASCADE
);

-- =====================================================
-- STEP 3: CREATE INDEXES FOR PERFORMANCE
-- =====================================================
PRINT 'Step 3: Creating indexes for performance...';

-- SubscriptionPlan Indexes
CREATE INDEX IX_SubscriptionPlan_BillingCycleId ON SubscriptionPlan(BillingCycleId);
CREATE INDEX IX_SubscriptionPlan_CurrencyId ON SubscriptionPlan(CurrencyId);
CREATE INDEX IX_SubscriptionPlan_CategoryId ON SubscriptionPlan(CategoryId);
CREATE INDEX IX_SubscriptionPlan_IsActive ON SubscriptionPlan(IsActive);
CREATE INDEX IX_SubscriptionPlan_DisplayOrder ON SubscriptionPlan(DisplayOrder);
CREATE INDEX IX_SubscriptionPlan_StripeProductId ON SubscriptionPlan(StripeProductId);

-- SubscriptionPlanPrivilege Indexes
CREATE INDEX IX_SubscriptionPlanPrivilege_SubscriptionPlanId ON SubscriptionPlanPrivilege(SubscriptionPlanId);
CREATE INDEX IX_SubscriptionPlanPrivilege_PrivilegeId ON SubscriptionPlanPrivilege(PrivilegeId);
CREATE INDEX IX_SubscriptionPlanPrivilege_UsagePeriodId ON SubscriptionPlanPrivilege(UsagePeriodId);
CREATE INDEX IX_SubscriptionPlanPrivilege_IsActive ON SubscriptionPlanPrivilege(IsActive);

-- Subscription Indexes
CREATE INDEX IX_Subscription_UserId ON Subscription(UserId);
CREATE INDEX IX_Subscription_SubscriptionPlanId ON Subscription(SubscriptionPlanId);
CREATE INDEX IX_Subscription_BillingCycleId ON Subscription(BillingCycleId);
CREATE INDEX IX_Subscription_ProviderId ON Subscription(ProviderId);
CREATE INDEX IX_Subscription_Status ON Subscription(Status);
CREATE INDEX IX_Subscription_IsCancelled ON Subscription(IsCancelled);
CREATE INDEX IX_Subscription_IsPaused ON Subscription(IsPaused);
CREATE INDEX IX_Subscription_StripeSubscriptionId ON Subscription(StripeSubscriptionId);
CREATE INDEX IX_Subscription_StripeCustomerId ON Subscription(StripeCustomerId);

-- UserSubscriptionPrivilegeUsage Indexes
CREATE INDEX IX_UserSubscriptionPrivilegeUsage_SubscriptionId ON UserSubscriptionPrivilegeUsage(SubscriptionId);
CREATE INDEX IX_UserSubscriptionPrivilegeUsage_SubscriptionPlanPrivilegeId ON UserSubscriptionPrivilegeUsage(SubscriptionPlanPrivilegeId);
CREATE INDEX IX_UserSubscriptionPrivilegeUsage_PrivilegeId ON UserSubscriptionPrivilegeUsage(PrivilegeId);
CREATE INDEX IX_UserSubscriptionPrivilegeUsage_UserId ON UserSubscriptionPrivilegeUsage(UserId);
CREATE INDEX IX_UserSubscriptionPrivilegeUsage_UsageDate ON UserSubscriptionPrivilegeUsage(UsageDate);
CREATE INDEX IX_UserSubscriptionPrivilegeUsage_UsageYear_Month ON UserSubscriptionPrivilegeUsage(UsageYear, UsageMonth);

-- PrivilegeUsageHistory Indexes
CREATE INDEX IX_PrivilegeUsageHistory_SubscriptionId ON PrivilegeUsageHistory(SubscriptionId);
CREATE INDEX IX_PrivilegeUsageHistory_SubscriptionPlanPrivilegeId ON PrivilegeUsageHistory(SubscriptionPlanPrivilegeId);
CREATE INDEX IX_PrivilegeUsageHistory_PrivilegeId ON PrivilegeUsageHistory(PrivilegeId);
CREATE INDEX IX_PrivilegeUsageHistory_UserId ON PrivilegeUsageHistory(UserId);
CREATE INDEX IX_PrivilegeUsageHistory_UsageDate ON PrivilegeUsageHistory(UsageDate);
CREATE INDEX IX_PrivilegeUsageHistory_PeriodType ON PrivilegeUsageHistory(PeriodType);

-- ServiceConstraint Indexes
CREATE INDEX IX_ServiceConstraint_ServiceName ON ServiceConstraint(ServiceName);
CREATE INDEX IX_ServiceConstraint_SubscriptionPlanId ON ServiceConstraint(SubscriptionPlanId);
CREATE INDEX IX_ServiceConstraint_IsActive ON ServiceConstraint(IsActive);

PRINT 'Step 3 Complete: All indexes created.';

-- =====================================================
-- STEP 4: VERIFICATION
-- =====================================================
PRINT 'Step 4: Verifying table creation...';

SELECT 
    TABLE_NAME,
    'CREATED' as Status
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME IN (
        'SubscriptionPlan',
        'SubscriptionPlanPrivilege',
        'Subscription',
        'UserSubscriptionPrivilegeUsage',
        'PrivilegeUsageHistory',
        'ServiceConstraint'
    )
ORDER BY TABLE_NAME;

-- =====================================================
-- COMPLETION MESSAGE
-- =====================================================
PRINT '=====================================================';
PRINT 'SUBSCRIPTION PLAN TABLES RESET COMPLETE!';
PRINT '=====================================================';
PRINT 'All subscription plan related tables have been:';
PRINT '1. Dropped (if they existed)';
PRINT '2. Recreated with proper structure';
PRINT '3. Indexed for optimal performance';
PRINT '4. Verified for successful creation';
PRINT '=====================================================';
PRINT 'You can now test your subscription plan API endpoints!';
PRINT '=====================================================';
