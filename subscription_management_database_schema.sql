-- =====================================================
-- SUBSCRIPTION MANAGEMENT MODULE - COMPLETE DATABASE SCHEMA
-- =====================================================
-- This script creates all necessary tables for the subscription management module
-- extracted from SmartTelehealth project.
-- 
-- Created: 2025-01-27
-- Purpose: Standalone subscription management system
-- =====================================================

-- Enable foreign key constraints
PRAGMA foreign_keys = ON;

-- =====================================================
-- 1. MASTER TABLES (Reference Data)
-- =====================================================

-- Master Billing Cycles
CREATE TABLE MasterBillingCycles (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(50) NOT NULL,
    Description NVARCHAR(200),
    DurationInDays INT NOT NULL,
    SortOrder INT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedBy INT,
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedBy INT,
    UpdatedDate DATETIME2,
    DeletedBy INT,
    DeletedDate DATETIME2
);

-- Master Currencies
CREATE TABLE MasterCurrencies (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Code NVARCHAR(10) NOT NULL UNIQUE,
    Name NVARCHAR(50) NOT NULL,
    Symbol NVARCHAR(10),
    SortOrder INT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedBy INT,
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedBy INT,
    UpdatedDate DATETIME2,
    DeletedBy INT,
    DeletedDate DATETIME2
);

-- Master Privilege Types
CREATE TABLE MasterPrivilegeTypes (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(50) NOT NULL,
    Description NVARCHAR(200),
    SortOrder INT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedBy INT,
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedBy INT,
    UpdatedDate DATETIME2,
    DeletedBy INT,
    DeletedDate DATETIME2
);

-- Master Payment Statuses
CREATE TABLE MasterPaymentStatuses (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(50) NOT NULL,
    Description NVARCHAR(200),
    SortOrder INT NOT NULL DEFAULT 0,
    Color NVARCHAR(50),
    IsActive BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedBy INT,
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedBy INT,
    UpdatedDate DATETIME2,
    DeletedBy INT,
    DeletedDate DATETIME2
);

-- =====================================================
-- 2. CORE ENTITIES
-- =====================================================

-- Users Table (Simplified for subscription module)
CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(256) NOT NULL UNIQUE,
    PhoneNumber NVARCHAR(20),
    DateOfBirth DATETIME2 NOT NULL,
    Gender NVARCHAR(10),
    Address NVARCHAR(500),
    City NVARCHAR(100),
    State NVARCHAR(50),
    ZipCode NVARCHAR(20),
    Country NVARCHAR(100),
    EmergencyContactName NVARCHAR(100),
    EmergencyContactPhone NVARCHAR(20),
    StripeCustomerId NVARCHAR(100),
    UserType NVARCHAR(50) NOT NULL DEFAULT 'Patient',
    ProfilePicture NVARCHAR(MAX),
    IsEmailVerified BIT NOT NULL DEFAULT 0,
    IsPhoneVerified BIT NOT NULL DEFAULT 0,
    LastLoginAt DATETIME2,
    PasswordResetToken NVARCHAR(500),
    ResetTokenExpires DATETIME2,
    NotificationPreferences NVARCHAR(MAX),
    LanguagePreference NVARCHAR(50),
    TimeZonePreference NVARCHAR(50),
    UserRoleId INT NOT NULL DEFAULT 1,
    IsActive BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedBy INT,
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedBy INT,
    UpdatedDate DATETIME2,
    DeletedBy INT,
    DeletedDate DATETIME2,
    UserName NVARCHAR(256),
    NormalizedUserName NVARCHAR(256),
    NormalizedEmail NVARCHAR(256),
    EmailConfirmed BIT NOT NULL DEFAULT 0,
    PasswordHash NVARCHAR(MAX),
    SecurityStamp NVARCHAR(MAX),
    ConcurrencyStamp NVARCHAR(MAX),
    PhoneNumberConfirmed BIT NOT NULL DEFAULT 0,
    TwoFactorEnabled BIT NOT NULL DEFAULT 0,
    LockoutEnd DATETIME2,
    LockoutEnabled BIT NOT NULL DEFAULT 1,
    AccessFailedCount INT NOT NULL DEFAULT 0
);

-- Categories (for subscription plans)
CREATE TABLE Categories (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500),
    ParentCategoryId UNIQUEIDENTIFIER,
    DisplayOrder INT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedBy INT,
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedBy INT,
    UpdatedDate DATETIME2,
    DeletedBy INT,
    DeletedDate DATETIME2,
    FOREIGN KEY (ParentCategoryId) REFERENCES Categories(Id),
    FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
    FOREIGN KEY (DeletedBy) REFERENCES Users(Id)
);

-- Privileges
CREATE TABLE Privileges (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500),
    PrivilegeTypeId UNIQUEIDENTIFIER NOT NULL,
    MasterPrivilegeTypeId UNIQUEIDENTIFIER,
    IsActive BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedBy INT,
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedBy INT,
    UpdatedDate DATETIME2,
    DeletedBy INT,
    DeletedDate DATETIME2,
    FOREIGN KEY (PrivilegeTypeId) REFERENCES MasterPrivilegeTypes(Id),
    FOREIGN KEY (MasterPrivilegeTypeId) REFERENCES MasterPrivilegeTypes(Id),
    FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
    FOREIGN KEY (DeletedBy) REFERENCES Users(Id)
);

-- Subscription Plans
CREATE TABLE SubscriptionPlans (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(1000),
    ShortDescription NVARCHAR(200),
    IsFeatured BIT NOT NULL DEFAULT 0,
    IsTrialAllowed BIT NOT NULL DEFAULT 0,
    TrialDurationInDays INT NOT NULL DEFAULT 0,
    IsMostPopular BIT NOT NULL DEFAULT 0,
    IsTrending BIT NOT NULL DEFAULT 0,
    DisplayOrder INT NOT NULL DEFAULT 0,
    PlanType NVARCHAR(50) NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    DiscountedPrice DECIMAL(18,2),
    DiscountValidUntil DATETIME2,
    MonthlyBillingDiscount DECIMAL(5,2) NOT NULL DEFAULT 0,
    QuarterlyBillingDiscount DECIMAL(5,2) NOT NULL DEFAULT 0,
    AnnualBillingDiscount DECIMAL(5,2) NOT NULL DEFAULT 0,
    BillingCycleId UNIQUEIDENTIFIER NOT NULL,
    CurrencyId UNIQUEIDENTIFIER NOT NULL,
    CategoryId UNIQUEIDENTIFIER,
    StripeProductId NVARCHAR(100),
    StripePriceId NVARCHAR(100),
    IsActive BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedBy INT,
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedBy INT,
    UpdatedDate DATETIME2,
    DeletedBy INT,
    DeletedDate DATETIME2,
    
    -- Plan Versioning Fields
    VersionNumber INT NOT NULL DEFAULT 1,
    IsLatestVersion BIT NOT NULL DEFAULT 1,
    ParentPlanId UNIQUEIDENTIFIER,
    VersionCreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    VersionEndDate DATETIME2,
    PriceChangeNoticeDays INT NOT NULL DEFAULT 10,
    
    -- Privilege-Based Pricing Fields
    IsAutoCalculatedPrice BIT NOT NULL DEFAULT 0,
    PrivilegesTotalCost DECIMAL(18,2) NOT NULL DEFAULT 0,
    AdminCommissionPercent DECIMAL(5,2) NOT NULL DEFAULT 0,
    AdminCommissionFixed DECIMAL(18,2) NOT NULL DEFAULT 0,
    
    FOREIGN KEY (BillingCycleId) REFERENCES MasterBillingCycles(Id),
    FOREIGN KEY (CurrencyId) REFERENCES MasterCurrencies(Id),
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id),
    FOREIGN KEY (ParentPlanId) REFERENCES SubscriptionPlans(Id),
    FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
    FOREIGN KEY (DeletedBy) REFERENCES Users(Id)
);

-- Subscription Plan Privileges (Junction Table)
CREATE TABLE SubscriptionPlanPrivileges (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    SubscriptionPlanId UNIQUEIDENTIFIER NOT NULL,
    PrivilegeId UNIQUEIDENTIFIER NOT NULL,
    Value INT NOT NULL, -- -1 = unlimited, 0 = disabled, >0 = limited
    UsagePeriodId UNIQUEIDENTIFIER NOT NULL,
    DurationMonths INT NOT NULL DEFAULT 1,
    Description NVARCHAR(500),
    EffectiveDate DATETIME2,
    ExpirationDate DATETIME2,
    DailyLimit INT,
    WeeklyLimit INT,
    MonthlyLimit INT,
    PrivilegeBaseCost DECIMAL(18,2) NOT NULL DEFAULT 0,
    UnitCost DECIMAL(18,2) NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedBy INT,
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedBy INT,
    UpdatedDate DATETIME2,
    DeletedBy INT,
    DeletedDate DATETIME2,
    FOREIGN KEY (SubscriptionPlanId) REFERENCES SubscriptionPlans(Id) ON DELETE CASCADE,
    FOREIGN KEY (PrivilegeId) REFERENCES Privileges(Id) ON DELETE CASCADE,
    FOREIGN KEY (UsagePeriodId) REFERENCES MasterBillingCycles(Id),
    FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
    FOREIGN KEY (DeletedBy) REFERENCES Users(Id)
);

-- Subscriptions
CREATE TABLE Subscriptions (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId INT NOT NULL,
    SubscriptionPlanId UNIQUEIDENTIFIER NOT NULL,
    BillingCycleId UNIQUEIDENTIFIER NOT NULL,
    ProviderId INT, -- Optional for healthcare providers
    Status NVARCHAR(50) NOT NULL,
    StatusReason NVARCHAR(500),
    StartDate DATETIME2 NOT NULL,
    EndDate DATETIME2,
    NextBillingDate DATETIME2 NOT NULL,
    CurrentPrice DECIMAL(18,2) NOT NULL,
    AutoRenew BIT NOT NULL DEFAULT 1,
    Notes NVARCHAR(1000),
    SubscriptionStatus NVARCHAR(MAX) NOT NULL,
    PausedDate DATETIME2,
    ResumedDate DATETIME2,
    CancelledDate DATETIME2,
    ExpirationDate DATETIME2,
    SuspendedDate DATETIME2,
    LastBillingDate DATETIME2,
    CancellationReason NVARCHAR(500),
    PauseReason NVARCHAR(500),
    CancelledAt DATETIME2,
    ExpiredAt DATETIME2,
    RenewedAt DATETIME2,
    ExpiryDate DATETIME2,
    Amount DECIMAL(18,2) NOT NULL,
    Currency NVARCHAR(50) NOT NULL,
    StripeSubscriptionId NVARCHAR(100),
    StripeCustomerId NVARCHAR(100),
    StripePriceId NVARCHAR(100),
    PaymentMethodId NVARCHAR(100),
    LastPaymentDate DATETIME2,
    LastPaymentFailedDate DATETIME2,
    LastPaymentError NVARCHAR(500),
    FailedPaymentAttempts INT NOT NULL DEFAULT 0,
    IsTrialSubscription BIT NOT NULL DEFAULT 0,
    TrialStartDate DATETIME2,
    TrialEndDate DATETIME2,
    TrialDurationInDays INT NOT NULL DEFAULT 0,
    LastUsedDate DATETIME2,
    TotalUsageCount INT NOT NULL DEFAULT 0,
    HealthAssessmentId UNIQUEIDENTIFIER, -- Optional for healthcare
    MasterBillingCycleId UNIQUEIDENTIFIER,
    MasterCurrencyId UNIQUEIDENTIFIER,
    IsActive BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedBy INT,
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedBy INT,
    UpdatedDate DATETIME2,
    DeletedBy INT,
    DeletedDate DATETIME2,
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (SubscriptionPlanId) REFERENCES SubscriptionPlans(Id),
    FOREIGN KEY (BillingCycleId) REFERENCES MasterBillingCycles(Id),
    FOREIGN KEY (MasterBillingCycleId) REFERENCES MasterBillingCycles(Id),
    FOREIGN KEY (MasterCurrencyId) REFERENCES MasterCurrencies(Id),
    FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
    FOREIGN KEY (DeletedBy) REFERENCES Users(Id)
);

-- =====================================================
-- 3. BILLING & PAYMENT TABLES
-- =====================================================

-- Billing Records
CREATE TABLE BillingRecords (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId INT NOT NULL,
    SubscriptionId UNIQUEIDENTIFIER,
    ConsultationId UNIQUEIDENTIFIER, -- Optional for healthcare
    MedicationDeliveryId UNIQUEIDENTIFIER, -- Optional for healthcare
    BillingCycleId UNIQUEIDENTIFIER,
    CurrencyId UNIQUEIDENTIFIER NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    Type NVARCHAR(50) NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    TaxAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    ShippingAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    TotalAmount DECIMAL(18,2) NOT NULL,
    BillingDate DATETIME2 NOT NULL,
    PaidAt DATETIME2,
    DueDate DATETIME2,
    InvoiceNumber NVARCHAR(100),
    StripePaymentIntentId NVARCHAR(100),
    StripeInvoiceId NVARCHAR(100),
    Description NVARCHAR(500),
    FailureReason NVARCHAR(500),
    PaymentMethod NVARCHAR(100),
    TransactionId NVARCHAR(100),
    ErrorMessage NVARCHAR(500),
    ProcessedAt DATETIME2,
    IsRecurring BIT NOT NULL DEFAULT 0,
    NextBillingDate DATETIME2,
    PaymentIntentId NVARCHAR(100),
    AccruedAmount DECIMAL(18,2),
    AccrualStartDate DATETIME2,
    AccrualEndDate DATETIME2,
    MasterCurrencyId UNIQUEIDENTIFIER,
    IsActive BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedBy INT,
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedBy INT,
    UpdatedDate DATETIME2,
    DeletedBy INT,
    DeletedDate DATETIME2,
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (SubscriptionId) REFERENCES Subscriptions(Id),
    FOREIGN KEY (BillingCycleId) REFERENCES MasterBillingCycles(Id),
    FOREIGN KEY (CurrencyId) REFERENCES MasterCurrencies(Id),
    FOREIGN KEY (MasterCurrencyId) REFERENCES MasterCurrencies(Id),
    FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
    FOREIGN KEY (DeletedBy) REFERENCES Users(Id)
);

-- Billing Adjustments
CREATE TABLE BillingAdjustments (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    BillingRecordId UNIQUEIDENTIFIER NOT NULL,
    Type NVARCHAR(50) NOT NULL, -- Discount, Credit, Refund, ManualPayment
    Amount DECIMAL(18,2) NOT NULL,
    Description NVARCHAR(500) NOT NULL,
    Reason NVARCHAR(500),
    IsPercentage BIT NOT NULL DEFAULT 0,
    Percentage DECIMAL(5,2),
    AppliedAt DATETIME2 NOT NULL,
    AppliedBy INT,
    IsApproved BIT NOT NULL DEFAULT 1,
    ApprovalNotes NVARCHAR(500),
    IsActive BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedBy INT,
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedBy INT,
    UpdatedDate DATETIME2,
    DeletedBy INT,
    DeletedDate DATETIME2,
    FOREIGN KEY (BillingRecordId) REFERENCES BillingRecords(Id),
    FOREIGN KEY (AppliedBy) REFERENCES Users(Id),
    FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
    FOREIGN KEY (DeletedBy) REFERENCES Users(Id)
);

-- Subscription Payments
CREATE TABLE SubscriptionPayments (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    SubscriptionId UNIQUEIDENTIFIER NOT NULL,
    BillingRecordId UNIQUEIDENTIFIER NOT NULL,
    CurrencyId UNIQUEIDENTIFIER NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    TaxAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    NetAmount DECIMAL(18,2) NOT NULL,
    Description NVARCHAR(500) NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    Type NVARCHAR(50) NOT NULL,
    FailureReason NVARCHAR(1000),
    DueDate DATETIME2 NOT NULL,
    PaidAt DATETIME2,
    FailedAt DATETIME2,
    BillingPeriodStart DATETIME2 NOT NULL,
    BillingPeriodEnd DATETIME2 NOT NULL,
    StripePaymentIntentId NVARCHAR(100),
    StripeInvoiceId NVARCHAR(100),
    ReceiptUrl NVARCHAR(500),
    PaymentIntentId NVARCHAR(100),
    InvoiceId NVARCHAR(100),
    AttemptCount INT NOT NULL DEFAULT 0,
    NextRetryAt DATETIME2,
    RefundedAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedBy INT,
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedBy INT,
    UpdatedDate DATETIME2,
    DeletedBy INT,
    DeletedDate DATETIME2,
    FOREIGN KEY (SubscriptionId) REFERENCES Subscriptions(Id),
    FOREIGN KEY (BillingRecordId) REFERENCES BillingRecords(Id) ON DELETE CASCADE,
    FOREIGN KEY (CurrencyId) REFERENCES MasterCurrencies(Id),
    FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
    FOREIGN KEY (DeletedBy) REFERENCES Users(Id)
);

-- Payment Refunds
CREATE TABLE PaymentRefunds (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    SubscriptionPaymentId UNIQUEIDENTIFIER NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Reason NVARCHAR(500) NOT NULL,
    StripeRefundId NVARCHAR(100),
    RefundedAt DATETIME2 NOT NULL,
    ProcessedByUserId INT,
    IsActive BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedBy INT,
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedBy INT,
    UpdatedDate DATETIME2,
    DeletedBy INT,
    DeletedDate DATETIME2,
    FOREIGN KEY (SubscriptionPaymentId) REFERENCES SubscriptionPayments(Id),
    FOREIGN KEY (ProcessedByUserId) REFERENCES Users(Id),
    FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
    FOREIGN KEY (DeletedBy) REFERENCES Users(Id)
);

-- Failed Refunds (for retry mechanism)
CREATE TABLE FailedRefunds (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    BillingRecordId UNIQUEIDENTIFIER NOT NULL,
    StripePaymentIntentId NVARCHAR(255) NOT NULL,
    StripeInvoiceId NVARCHAR(255),
    Amount DECIMAL(18,2) NOT NULL,
    UserId INT NOT NULL,
    ChargedAt DATETIME2 NOT NULL,
    DatabaseFailedAt DATETIME2 NOT NULL,
    FirstAttemptAt DATETIME2 NOT NULL,
    LastAttemptAt DATETIME2,
    RetryCount INT NOT NULL DEFAULT 0,
    MaxRetries INT NOT NULL DEFAULT 3,
    Status INT NOT NULL,
    LastErrorMessage NVARCHAR(2000),
    ErrorDetails TEXT,
    DatabaseFailureReason NVARCHAR(2000),
    AdminNotified BIT NOT NULL DEFAULT 0,
    AdminNotifiedAt DATETIME2,
    ResolvedAt DATETIME2,
    ResolvedBy INT,
    ResolutionNotes NVARCHAR(2000),
    Priority NVARCHAR(20) NOT NULL DEFAULT 'Normal',
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy INT NOT NULL,
    UpdatedDate DATETIME2,
    UpdatedBy INT,
    FOREIGN KEY (BillingRecordId) REFERENCES BillingRecords(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (ResolvedBy) REFERENCES Users(Id),
    FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    FOREIGN KEY (UpdatedBy) REFERENCES Users(Id)
);

-- =====================================================
-- 4. PRIVILEGE USAGE TRACKING
-- =====================================================

-- User Subscription Privilege Usage
CREATE TABLE UserSubscriptionPrivilegeUsages (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    SubscriptionId UNIQUEIDENTIFIER NOT NULL,
    SubscriptionPlanPrivilegeId UNIQUEIDENTIFIER NOT NULL,
    PrivilegeId UNIQUEIDENTIFIER NOT NULL,
    UsedValue INT NOT NULL DEFAULT 0,
    AllowedValue INT NOT NULL,
    UsagePeriodStart DATETIME2 NOT NULL,
    UsagePeriodEnd DATETIME2 NOT NULL,
    LastUsedAt DATETIME2,
    ResetAt DATETIME2,
    Notes NVARCHAR(500),
    IsActive BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedBy INT,
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedBy INT,
    UpdatedDate DATETIME2,
    DeletedBy INT,
    DeletedDate DATETIME2,
    FOREIGN KEY (SubscriptionId) REFERENCES Subscriptions(Id) ON DELETE CASCADE,
    FOREIGN KEY (SubscriptionPlanPrivilegeId) REFERENCES SubscriptionPlanPrivileges(Id) ON DELETE CASCADE,
    FOREIGN KEY (PrivilegeId) REFERENCES Privileges(Id),
    FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
    FOREIGN KEY (DeletedBy) REFERENCES Users(Id)
);

-- Privilege Usage History (Audit Trail)
CREATE TABLE PrivilegeUsageHistories (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserSubscriptionPrivilegeUsageId UNIQUEIDENTIFIER NOT NULL,
    UsedValue INT NOT NULL,
    UsedAt DATETIME2 NOT NULL,
    UsageDate DATETIME2 NOT NULL,
    UsageWeek NVARCHAR(10) NOT NULL,
    UsageMonth NVARCHAR(7) NOT NULL,
    Notes NVARCHAR(500),
    IsActive BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedBy INT,
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedBy INT,
    UpdatedDate DATETIME2,
    DeletedBy INT,
    DeletedDate DATETIME2,
    FOREIGN KEY (UserSubscriptionPrivilegeUsageId) REFERENCES UserSubscriptionPrivilegeUsages(Id),
    FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
    FOREIGN KEY (DeletedBy) REFERENCES Users(Id)
);

-- =====================================================
-- 5. SUBSCRIPTION LIFECYCLE & STATUS TRACKING
-- =====================================================

-- Subscription Status History
CREATE TABLE SubscriptionStatusHistories (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    SubscriptionId UNIQUEIDENTIFIER NOT NULL,
    PreviousStatus NVARCHAR(50),
    NewStatus NVARCHAR(50) NOT NULL,
    ChangedAt DATETIME2 NOT NULL,
    ChangedBy INT,
    Reason NVARCHAR(500),
    Notes NVARCHAR(1000),
    IsActive BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedBy INT,
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedBy INT,
    UpdatedDate DATETIME2,
    DeletedBy INT,
    DeletedDate DATETIME2,
    FOREIGN KEY (SubscriptionId) REFERENCES Subscriptions(Id),
    FOREIGN KEY (ChangedBy) REFERENCES Users(Id),
    FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
    FOREIGN KEY (DeletedBy) REFERENCES Users(Id)
);

-- Scheduled Plan Migrations
CREATE TABLE ScheduledPlanMigrations (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    SubscriptionId UNIQUEIDENTIFIER NOT NULL,
    FromPlanId UNIQUEIDENTIFIER NOT NULL,
    ToPlanId UNIQUEIDENTIFIER NOT NULL,
    NotificationDate DATETIME2 NOT NULL,
    ScheduledMigrationDate DATETIME2 NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    UserDecision NVARCHAR(50), -- Accept, Decline, Downgrade
    UserDecisionDate DATETIME2,
    DowngradeToPlanId UNIQUEIDENTIFIER,
    CompletedDate DATETIME2,
    Notes NVARCHAR(500),
    IsActive BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedBy INT,
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedBy INT,
    UpdatedDate DATETIME2,
    DeletedBy INT,
    DeletedDate DATETIME2,
    FOREIGN KEY (SubscriptionId) REFERENCES Subscriptions(Id),
    FOREIGN KEY (FromPlanId) REFERENCES SubscriptionPlans(Id),
    FOREIGN KEY (ToPlanId) REFERENCES SubscriptionPlans(Id),
    FOREIGN KEY (DowngradeToPlanId) REFERENCES SubscriptionPlans(Id),
    FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
    FOREIGN KEY (DeletedBy) REFERENCES Users(Id)
);

-- =====================================================
-- 6. SYSTEM TABLES
-- =====================================================

-- Processed Webhook Events (for Stripe webhook idempotency)
CREATE TABLE ProcessedWebhookEvents (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    StripeEventId NVARCHAR(255) NOT NULL UNIQUE,
    EventType NVARCHAR(100) NOT NULL,
    ReceivedAt DATETIME2 NOT NULL,
    ProcessedAt DATETIME2,
    IsSuccess BIT NOT NULL DEFAULT 0,
    ErrorMessage NVARCHAR(2000),
    RetryCount INT NOT NULL DEFAULT 0,
    MaxRetries INT NOT NULL DEFAULT 3,
    LastAttemptAt DATETIME2,
    Metadata NVARCHAR(4000),
    ProcessingDurationMs BIGINT
);

-- =====================================================
-- 7. INDEXES FOR PERFORMANCE
-- =====================================================

-- Users indexes
CREATE INDEX IX_Users_Email ON Users(Email);
CREATE INDEX IX_Users_StripeCustomerId ON Users(StripeCustomerId);
CREATE INDEX IX_Users_IsActive ON Users(IsActive);

-- Subscriptions indexes
CREATE INDEX IX_Subscriptions_UserId ON Subscriptions(UserId);
CREATE INDEX IX_Subscriptions_Status ON Subscriptions(Status);
CREATE INDEX IX_Subscriptions_NextBillingDate ON Subscriptions(NextBillingDate);
CREATE INDEX IX_Subscriptions_StripeSubscriptionId ON Subscriptions(StripeSubscriptionId);
CREATE INDEX IX_Subscriptions_StripeCustomerId ON Subscriptions(StripeCustomerId);
CREATE INDEX IX_Subscriptions_IsActive ON Subscriptions(IsActive);

-- Subscription Plans indexes
CREATE INDEX IX_SubscriptionPlans_IsActive ON SubscriptionPlans(IsActive);
CREATE INDEX IX_SubscriptionPlans_IsLatestVersion ON SubscriptionPlans(IsLatestVersion);
CREATE INDEX IX_SubscriptionPlans_ParentPlanId ON SubscriptionPlans(ParentPlanId);
CREATE INDEX IX_SubscriptionPlans_StripeProductId ON SubscriptionPlans(StripeProductId);

-- Billing Records indexes
CREATE INDEX IX_BillingRecords_UserId ON BillingRecords(UserId);
CREATE INDEX IX_BillingRecords_SubscriptionId ON BillingRecords(SubscriptionId);
CREATE INDEX IX_BillingRecords_Status ON BillingRecords(Status);
CREATE INDEX IX_BillingRecords_BillingDate ON BillingRecords(BillingDate);
CREATE INDEX IX_BillingRecords_DueDate ON BillingRecords(DueDate);
CREATE INDEX IX_BillingRecords_StripePaymentIntentId ON BillingRecords(StripePaymentIntentId);

-- Subscription Payments indexes
CREATE INDEX IX_SubscriptionPayments_SubscriptionId ON SubscriptionPayments(SubscriptionId);
CREATE INDEX IX_SubscriptionPayments_BillingRecordId ON SubscriptionPayments(BillingRecordId);
CREATE INDEX IX_SubscriptionPayments_Status ON SubscriptionPayments(Status);
CREATE INDEX IX_SubscriptionPayments_DueDate ON SubscriptionPayments(DueDate);
CREATE INDEX IX_SubscriptionPayments_StripePaymentIntentId ON SubscriptionPayments(StripePaymentIntentId);

-- User Subscription Privilege Usage indexes
CREATE INDEX IX_UserSubscriptionPrivilegeUsages_SubscriptionId ON UserSubscriptionPrivilegeUsages(SubscriptionId);
CREATE INDEX IX_UserSubscriptionPrivilegeUsages_PrivilegeId ON UserSubscriptionPrivilegeUsages(PrivilegeId);
CREATE INDEX IX_UserSubscriptionPrivilegeUsages_UsagePeriodStart ON UserSubscriptionPrivilegeUsages(UsagePeriodStart);
CREATE INDEX IX_UserSubscriptionPrivilegeUsages_UsagePeriodEnd ON UserSubscriptionPrivilegeUsages(UsagePeriodEnd);

-- Privilege Usage History indexes
CREATE INDEX IX_PrivilegeUsageHistories_UserSubscriptionPrivilegeUsageId ON PrivilegeUsageHistories(UserSubscriptionPrivilegeUsageId);
CREATE INDEX IX_PrivilegeUsageHistories_UsageDate ON PrivilegeUsageHistories(UsageDate);
CREATE INDEX IX_PrivilegeUsageHistories_UsageWeek ON PrivilegeUsageHistories(UsageWeek);
CREATE INDEX IX_PrivilegeUsageHistories_UsageMonth ON PrivilegeUsageHistories(UsageMonth);

-- Subscription Status History indexes
CREATE INDEX IX_SubscriptionStatusHistories_SubscriptionId ON SubscriptionStatusHistories(SubscriptionId);
CREATE INDEX IX_SubscriptionStatusHistories_ChangedAt ON SubscriptionStatusHistories(ChangedAt);

-- Scheduled Plan Migrations indexes
CREATE INDEX IX_ScheduledPlanMigrations_SubscriptionId ON ScheduledPlanMigrations(SubscriptionId);
CREATE INDEX IX_ScheduledPlanMigrations_Status ON ScheduledPlanMigrations(Status);
CREATE INDEX IX_ScheduledPlanMigrations_ScheduledMigrationDate ON ScheduledPlanMigrations(ScheduledMigrationDate);
CREATE INDEX IX_ScheduledPlanMigrations_Status_ScheduledMigrationDate ON ScheduledPlanMigrations(Status, ScheduledMigrationDate);

-- Processed Webhook Events indexes
CREATE INDEX IX_ProcessedWebhookEvents_StripeEventId ON ProcessedWebhookEvents(StripeEventId);
CREATE INDEX IX_ProcessedWebhookEvents_EventType ON ProcessedWebhookEvents(EventType);
CREATE INDEX IX_ProcessedWebhookEvents_ReceivedAt ON ProcessedWebhookEvents(ReceivedAt);
CREATE INDEX IX_ProcessedWebhookEvents_IsSuccess ON ProcessedWebhookEvents(IsSuccess);

-- Failed Refunds indexes
CREATE INDEX IX_FailedRefunds_UserId ON FailedRefunds(UserId);
CREATE INDEX IX_FailedRefunds_Status ON FailedRefunds(Status);
CREATE INDEX IX_FailedRefunds_RetryCount ON FailedRefunds(RetryCount);
CREATE INDEX IX_FailedRefunds_LastAttemptAt ON FailedRefunds(LastAttemptAt);

-- =====================================================
-- 8. MASTER DATA SEEDING
-- =====================================================

-- Insert Master Billing Cycles
INSERT INTO MasterBillingCycles (Id, Name, Description, DurationInDays, SortOrder) VALUES
(NEWID(), 'Monthly', 'Monthly billing cycle', 30, 1),
(NEWID(), 'Quarterly', 'Quarterly billing cycle', 90, 2),
(NEWID(), 'Annual', 'Annual billing cycle', 365, 3);

-- Insert Master Currencies
INSERT INTO MasterCurrencies (Id, Code, Name, Symbol, SortOrder) VALUES
(NEWID(), 'USD', 'US Dollar', '$', 1),
(NEWID(), 'EUR', 'Euro', '€', 2),
(NEWID(), 'GBP', 'British Pound', '£', 3);

-- Insert Master Privilege Types
INSERT INTO MasterPrivilegeTypes (Id, Name, Description, SortOrder) VALUES
(NEWID(), 'Teleconsultation', 'Video consultation with healthcare provider', 1),
(NEWID(), 'Messaging', 'Secure messaging with healthcare provider', 2),
(NEWID(), 'Medication Delivery', 'Prescription medication delivery', 3),
(NEWID(), 'Health Records', 'Access to health records and history', 4),
(NEWID(), 'Lab Results', 'Access to laboratory test results', 5);

-- Insert Master Payment Statuses
INSERT INTO MasterPaymentStatuses (Id, Name, Description, SortOrder, Color) VALUES
(NEWID(), 'Pending', 'Payment is pending', 1, 'Yellow'),
(NEWID(), 'Succeeded', 'Payment succeeded', 2, 'Green'),
(NEWID(), 'Failed', 'Payment failed', 3, 'Red'),
(NEWID(), 'Refunded', 'Payment was refunded', 4, 'Orange'),
(NEWID(), 'Cancelled', 'Payment was cancelled', 5, 'Gray');

-- =====================================================
-- 9. CONSTRAINTS AND VALIDATION
-- =====================================================

-- Add check constraints for data validation
ALTER TABLE Subscriptions ADD CONSTRAINT CK_Subscriptions_Status 
CHECK (Status IN ('Pending', 'Active', 'Paused', 'Cancelled', 'Expired', 'PaymentFailed', 'TrialActive', 'TrialExpired', 'Suspended'));

ALTER TABLE SubscriptionPayments ADD CONSTRAINT CK_SubscriptionPayments_Status 
CHECK (Status IN ('Pending', 'Succeeded', 'Failed', 'Refunded'));

ALTER TABLE SubscriptionPayments ADD CONSTRAINT CK_SubscriptionPayments_Type 
CHECK (Type IN ('Subscription', 'Overage', 'Refund'));

ALTER TABLE BillingRecords ADD CONSTRAINT CK_BillingRecords_Status 
CHECK (Status IN ('Pending', 'Paid', 'Failed', 'Overdue'));

ALTER TABLE BillingRecords ADD CONSTRAINT CK_BillingRecords_Type 
CHECK (Type IN ('Subscription', 'Consultation', 'Overage'));

ALTER TABLE BillingAdjustments ADD CONSTRAINT CK_BillingAdjustments_Type 
CHECK (Type IN ('Discount', 'Credit', 'Refund', 'ManualPayment'));

ALTER TABLE ScheduledPlanMigrations ADD CONSTRAINT CK_ScheduledPlanMigrations_Status 
CHECK (Status IN ('Pending', 'Notified', 'Accepted', 'Declined', 'Completed', 'Cancelled'));

ALTER TABLE ScheduledPlanMigrations ADD CONSTRAINT CK_ScheduledPlanMigrations_UserDecision 
CHECK (UserDecision IN ('Accept', 'Decline', 'Downgrade') OR UserDecision IS NULL);

-- Add unique constraints
ALTER TABLE SubscriptionPlanPrivileges ADD CONSTRAINT UQ_SubscriptionPlanPrivileges_Plan_Privilege 
UNIQUE (SubscriptionPlanId, PrivilegeId);

-- =====================================================
-- 10. VIEWS FOR COMMON QUERIES
-- =====================================================

-- Active Subscriptions View
CREATE VIEW vw_ActiveSubscriptions AS
SELECT 
    s.Id,
    s.UserId,
    u.FirstName + ' ' + u.LastName AS UserName,
    u.Email,
    s.SubscriptionPlanId,
    sp.Name AS PlanName,
    s.Status,
    s.StartDate,
    s.NextBillingDate,
    s.CurrentPrice,
    s.StripeSubscriptionId,
    s.IsTrialSubscription,
    s.TrialEndDate
FROM Subscriptions s
INNER JOIN Users u ON s.UserId = u.Id
INNER JOIN SubscriptionPlans sp ON s.SubscriptionPlanId = sp.Id
WHERE s.IsActive = 1 
  AND s.IsDeleted = 0 
  AND s.Status IN ('Active', 'TrialActive');

-- Subscription Usage Summary View
CREATE VIEW vw_SubscriptionUsageSummary AS
SELECT 
    s.Id AS SubscriptionId,
    s.UserId,
    u.FirstName + ' ' + u.LastName AS UserName,
    sp.Name AS PlanName,
    COUNT(uspu.Id) AS TotalPrivileges,
    SUM(uspu.UsedValue) AS TotalUsage,
    SUM(uspu.AllowedValue) AS TotalAllowed,
    CASE 
        WHEN SUM(uspu.AllowedValue) > 0 
        THEN CAST(SUM(uspu.UsedValue) AS FLOAT) / SUM(uspu.AllowedValue) * 100
        ELSE 0 
    END AS UsagePercentage
FROM Subscriptions s
INNER JOIN Users u ON s.UserId = u.Id
INNER JOIN SubscriptionPlans sp ON s.SubscriptionPlanId = sp.Id
LEFT JOIN UserSubscriptionPrivilegeUsages uspu ON s.Id = uspu.SubscriptionId
WHERE s.IsActive = 1 AND s.IsDeleted = 0
GROUP BY s.Id, s.UserId, u.FirstName, u.LastName, sp.Name;

-- Billing Summary View
CREATE VIEW vw_BillingSummary AS
SELECT 
    br.Id,
    br.UserId,
    u.FirstName + ' ' + u.LastName AS UserName,
    br.SubscriptionId,
    sp.Name AS PlanName,
    br.Status,
    br.Type,
    br.TotalAmount,
    br.BillingDate,
    br.DueDate,
    br.PaidAt,
    br.StripePaymentIntentId
FROM BillingRecords br
INNER JOIN Users u ON br.UserId = u.Id
LEFT JOIN Subscriptions s ON br.SubscriptionId = s.Id
LEFT JOIN SubscriptionPlans sp ON s.SubscriptionPlanId = sp.Id
WHERE br.IsActive = 1 AND br.IsDeleted = 0;

-- =====================================================
-- END OF SCRIPT
-- =====================================================

PRINT 'Subscription Management Database Schema created successfully!';
PRINT 'Total tables created: 20+';
PRINT 'Total indexes created: 30+';
PRINT 'Master data seeded successfully!';
PRINT 'Views created for common queries!';
