-- SQL Server DDL script for TeleHealth Subscription & Billing Tables
-- THIS SCRIPT IS UPDATED BASED ON THE MIGRATION INFO
-- Non-included FKs (e.g. to Users, MasterCurrencies, etc.) appear as commented constraints for manual activation.

-- 1. BillingRecords
CREATE TABLE BillingRecords (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    UserId INT NOT NULL,
    SubscriptionId UNIQUEIDENTIFIER NULL,
    ConsultationId UNIQUEIDENTIFIER NULL,
    MedicationDeliveryId UNIQUEIDENTIFIER NULL,
    BillingCycleId UNIQUEIDENTIFIER NULL,
    CurrencyId UNIQUEIDENTIFIER NOT NULL,
    Status NVARCHAR(450) NOT NULL,
    Type NVARCHAR(450) NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    TaxAmount DECIMAL(18,2) NOT NULL,
    ShippingAmount DECIMAL(18,2) NOT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL,
    BillingDate DATETIME2 NOT NULL,
    PaidAt DATETIME2 NULL,
    DueDate DATETIME2 NULL,
    InvoiceNumber NVARCHAR(100),
    StripePaymentIntentId NVARCHAR(100),
    StripeInvoiceId NVARCHAR(100),
    Description NVARCHAR(500),
    FailureReason NVARCHAR(500),
    PaymentMethod NVARCHAR(100),
    TransactionId NVARCHAR(100),
    ErrorMessage NVARCHAR(500),
    ProcessedAt DATETIME2 NULL,
    IsRecurring BIT NOT NULL DEFAULT 0,
    NextBillingDate DATETIME2 NULL,
    PaymentIntentId NVARCHAR(100),
    AccruedAmount DECIMAL(18,2),
    AccrualStartDate DATETIME2,
    AccrualEndDate DATETIME2,
    MasterCurrencyId UNIQUEIDENTIFIER,
    IsActive BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedBy INT NULL,
    CreatedDate DATETIME2 DEFAULT(GETUTCDATE()),
    UpdatedBy INT NULL,
    UpdatedDate DATETIME2 NULL,
    DeletedBy INT NULL,
    DeletedDate DATETIME2 NULL
    -- ,CONSTRAINT FK_BillingRecords_Users_UserId FOREIGN KEY (UserId) REFERENCES Users(Id)
    -- ,CONSTRAINT FK_BillingRecords_Subscription_SubscriptionId FOREIGN KEY (SubscriptionId) REFERENCES Subscriptions(Id)
    -- ,CONSTRAINT FK_BillingRecords_MasterCurrencies_CurrencyId FOREIGN KEY (CurrencyId) REFERENCES MasterCurrencies(Id)
);
GO
-- 2. BillingAdjustments
CREATE TABLE BillingAdjustments (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    BillingRecordId UNIQUEIDENTIFIER NOT NULL,
    Type NVARCHAR(450) NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Description NVARCHAR(500) NOT NULL,
    Reason NVARCHAR(500),
    IsPercentage BIT NOT NULL DEFAULT 0,
    Percentage DECIMAL(5,2),
    AppliedAt DATETIME2 NOT NULL,
    AppliedBy INT NULL,
    IsApproved BIT NOT NULL DEFAULT 1,
    ApprovalNotes NVARCHAR(500),
    IsActive BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedBy INT NULL,
    CreatedDate DATETIME2 DEFAULT(GETUTCDATE()),
    UpdatedBy INT NULL,
    UpdatedDate DATETIME2,
    DeletedBy INT,
    DeletedDate DATETIME2,
    CONSTRAINT FK_BillingAdjustments_BillingRecords FOREIGN KEY (BillingRecordId) REFERENCES BillingRecords(Id)
);
GO
-- 3. SubscriptionPlans
CREATE TABLE SubscriptionPlans (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(1000),
    ShortDescription NVARCHAR(200),
    IsFeatured BIT NOT NULL DEFAULT 0,
    IsTrialAllowed BIT NOT NULL DEFAULT 0,
    TrialDurationInDays INT NOT NULL DEFAULT 0,
    IsMostPopular BIT NOT NULL DEFAULT 0,
    IsTrending BIT NOT NULL DEFAULT 0,
    DisplayOrder INT NOT NULL DEFAULT 0,
    PlanType NVARCHAR(450) NOT NULL,
    BasePrice DECIMAL(18,2) NOT NULL,
    DiscountPercentage DECIMAL(5,2),
    DiscountValidUntil DATETIME2,
    BillingDiscountPercentage DECIMAL(5,2),
    VersionNumber INT NOT NULL DEFAULT 1,
    IsLatestVersion BIT NOT NULL DEFAULT 1,
    ParentPlanId UNIQUEIDENTIFIER,
    IsAutoCalculatedPrice BIT NOT NULL DEFAULT 1,
    PrivilegesTotalCost DECIMAL(18,2) NOT NULL DEFAULT 0,
    AdminCommissionPercent DECIMAL(5,2),
    PriceChangeNoticeDays INT NOT NULL DEFAULT 10,
    VersionCreatedDate DATETIME2 NOT NULL DEFAULT(GETUTCDATE()),
    BillingCycleId UNIQUEIDENTIFIER NOT NULL,
    CurrencyId UNIQUEIDENTIFIER NOT NULL,
    CategoryId UNIQUEIDENTIFIER NOT NULL,
    StripeProductId NVARCHAR(100),
    StripePriceId NVARCHAR(100),
    MessagingCount INT NOT NULL DEFAULT 10,
    IncludesMedicationDelivery BIT NOT NULL DEFAULT 1,
    IncludesFollowUpCare BIT NOT NULL DEFAULT 1,
    DeliveryFrequencyDays INT NOT NULL DEFAULT 30,
    MaxPauseDurationDays INT NOT NULL DEFAULT 90,
    Features NVARCHAR(1000),
    Terms NVARCHAR(500),
    EffectiveDate DATETIME2,
    ExpirationDate DATETIME2,
    DefaultTaxPercentage DECIMAL(5,2),
    TaxNotes NVARCHAR(500),
    MasterBillingCycleId UNIQUEIDENTIFIER,
    MasterCurrencyId UNIQUEIDENTIFIER,
    IsActive BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedBy INT,
    CreatedDate DATETIME2 DEFAULT(GETUTCDATE()),
    UpdatedBy INT,
    UpdatedDate DATETIME2,
    DeletedBy INT,
    DeletedDate DATETIME2
    -- ,CONSTRAINT FK_SubscriptionPlans_BillingCycle_BillingCycleId FOREIGN KEY (BillingCycleId) REFERENCES MasterBillingCycles(Id)
    -- ,CONSTRAINT FK_SubscriptionPlans_Currency_CurrencyId FOREIGN KEY (CurrencyId) REFERENCES MasterCurrencies(Id)
    -- ,CONSTRAINT FK_SubscriptionPlans_Category_CategoryId FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
    -- ,CONSTRAINT FK_SubscriptionPlans_ParentPlan_ParentPlanId FOREIGN KEY (ParentPlanId) REFERENCES SubscriptionPlans(Id)
);
GO
-- 4. Subscriptions
CREATE TABLE Subscriptions (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    UserId INT NOT NULL,
    SubscriptionPlanId UNIQUEIDENTIFIER NOT NULL,
    BillingCycleId UNIQUEIDENTIFIER,
    ProviderId INT,
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
    Currency NVARCHAR(MAX) NOT NULL,
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
    HealthAssessmentId UNIQUEIDENTIFIER,
    PendingCancellationAtRenewal BIT NOT NULL DEFAULT 0,
    PendingCancellationReason NVARCHAR(500),
    PendingChangeType NVARCHAR(50),
    PendingPlanChangeId UNIQUEIDENTIFIER,
    PlanChangeEffectiveDate DATETIME2,
    MasterBillingCycleId UNIQUEIDENTIFIER,
    MasterCurrencyId UNIQUEIDENTIFIER,
    IsActive BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedBy INT,
    CreatedDate DATETIME2 DEFAULT(GETUTCDATE()),
    UpdatedBy INT,
    UpdatedDate DATETIME2,
    DeletedBy INT,
    DeletedDate DATETIME2,
    CONSTRAINT FK_Subscriptions_SubscriptionPlan FOREIGN KEY (SubscriptionPlanId) REFERENCES SubscriptionPlans(Id)
    -- ,CONSTRAINT FK_Subscriptions_UserId FOREIGN KEY (UserId) REFERENCES Users(Id)
    -- ,CONSTRAINT FK_Subscriptions_PendingPlan FOREIGN KEY (PendingPlanChangeId) REFERENCES SubscriptionPlans(Id)
);
GO
-- 5. Privileges
CREATE TABLE Privileges (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500),
    PrivilegeTypeId UNIQUEIDENTIFIER NOT NULL,
    MasterPrivilegeTypeId UNIQUEIDENTIFIER,
    IsActive BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedBy INT,
    CreatedDate DATETIME2 DEFAULT(GETUTCDATE()),
    UpdatedBy INT,
    UpdatedDate DATETIME2,
    DeletedBy INT,
    DeletedDate DATETIME2
    -- ,CONSTRAINT FK_Privileges_MasterPrivilegeTypes_MasterPrivilegeTypeId FOREIGN KEY (MasterPrivilegeTypeId) REFERENCES MasterPrivilegeTypes(Id)
    -- ,CONSTRAINT FK_Privileges_MasterPrivilegeTypes_PrivilegeTypeId FOREIGN KEY (PrivilegeTypeId) REFERENCES MasterPrivilegeTypes(Id)
);
GO
-- 6. SubscriptionPlanPrivileges
CREATE TABLE SubscriptionPlanPrivileges (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    SubscriptionPlanId UNIQUEIDENTIFIER NOT NULL,
    PrivilegeId UNIQUEIDENTIFIER NOT NULL,
    Value INT NOT NULL,
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
    CreatedDate DATETIME2 DEFAULT(GETUTCDATE()),
    UpdatedBy INT,
    UpdatedDate DATETIME2,
    DeletedBy INT,
    DeletedDate DATETIME2,
    CONSTRAINT FK_SubscriptionPlanPrivileges_SubscriptionPlan FOREIGN KEY (SubscriptionPlanId) REFERENCES SubscriptionPlans(Id),
    CONSTRAINT FK_SubscriptionPlanPrivileges_Privilege FOREIGN KEY (PrivilegeId) REFERENCES Privileges(Id)
    -- ,CONSTRAINT FK_SubscriptionPlanPrivileges_MasterBillingCycles_UsagePeriodId FOREIGN KEY (UsagePeriodId) REFERENCES MasterBillingCycles(Id)
);
GO
-- 7. UserSubscriptionPrivilegeUsages
CREATE TABLE UserSubscriptionPrivilegeUsages (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    SubscriptionId UNIQUEIDENTIFIER NOT NULL,
    SubscriptionPlanPrivilegeId UNIQUEIDENTIFIER NOT NULL,
    PrivilegeId UNIQUEIDENTIFIER NOT NULL,
    UsedValue INT NOT NULL,
    AllowedValue INT NOT NULL,
    UsagePeriodStart DATETIME2 NOT NULL,
    UsagePeriodEnd DATETIME2 NOT NULL,
    LastUsedAt DATETIME2,
    ResetAt DATETIME2,
    Notes NVARCHAR(500),
    PrivilegeId1 UNIQUEIDENTIFIER,
    IsActive BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedBy INT,
    CreatedDate DATETIME2 DEFAULT(GETUTCDATE()),
    UpdatedBy INT,
    UpdatedDate DATETIME2,
    DeletedBy INT,
    DeletedDate DATETIME2,
    CONSTRAINT FK_UserSubscriptionPrivilegeUsages_SubscriptionPlanPrivilege FOREIGN KEY (SubscriptionPlanPrivilegeId) REFERENCES SubscriptionPlanPrivileges(Id),
    CONSTRAINT FK_UserSubscriptionPrivilegeUsages_Privilege FOREIGN KEY (PrivilegeId) REFERENCES Privileges(Id)
    -- ,CONSTRAINT FK_UserSubscriptionPrivilegeUsages_PrivilegeId1 FOREIGN KEY (PrivilegeId1) REFERENCES Privileges(Id)
    -- ,CONSTRAINT FK_UserSubscriptionPrivilegeUsages_Subscription FOREIGN KEY (SubscriptionId) REFERENCES Subscriptions(Id)
);
GO
-- 8. PrivilegeUsageHistories
CREATE TABLE PrivilegeUsageHistories (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
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
    CreatedDate DATETIME2 DEFAULT(GETUTCDATE()),
    UpdatedBy INT,
    UpdatedDate DATETIME2,
    DeletedBy INT,
    DeletedDate DATETIME2,
    CONSTRAINT FK_PrivilegeUsageHistories_UserSubscriptionPrivilegeUsage FOREIGN KEY (UserSubscriptionPrivilegeUsageId) REFERENCES UserSubscriptionPrivilegeUsages(Id)
);
GO
-- 9. SubscriptionStatusHistories
CREATE TABLE SubscriptionStatusHistories (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    SubscriptionId UNIQUEIDENTIFIER NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    ChangedAt DATETIME2 NOT NULL,
    ChangedBy INT,
    Reason NVARCHAR(500),
    IsActive BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedBy INT,
    CreatedDate DATETIME2 DEFAULT(GETUTCDATE()),
    UpdatedBy INT,
    UpdatedDate DATETIME2,
    DeletedBy INT,
    DeletedDate DATETIME2,
    CONSTRAINT FK_SubscriptionStatusHistories_Subscription FOREIGN KEY (SubscriptionId) REFERENCES Subscriptions(Id)
);
GO
-- 10. SubscriptionPayments
CREATE TABLE SubscriptionPayments (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    SubscriptionId UNIQUEIDENTIFIER NOT NULL,
    BillingRecordId UNIQUEIDENTIFIER NOT NULL,
    CurrencyId UNIQUEIDENTIFIER NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    TaxAmount DECIMAL(18,2) NOT NULL,
    NetAmount DECIMAL(18,2) NOT NULL,
    Description NVARCHAR(500) NOT NULL,
    Status NVARCHAR(450) NOT NULL,
    Type NVARCHAR(450) NOT NULL,
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
    RefundedAmount DECIMAL(18,2) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedBy INT,
    CreatedDate DATETIME2 DEFAULT(GETUTCDATE()),
    UpdatedBy INT,
    UpdatedDate DATETIME2,
    DeletedBy INT,
    DeletedDate DATETIME2,
    CONSTRAINT FK_SubscriptionPayments_Subscription FOREIGN KEY (SubscriptionId) REFERENCES Subscriptions(Id),
    CONSTRAINT FK_SubscriptionPayments_BillingRecord FOREIGN KEY (BillingRecordId) REFERENCES BillingRecords(Id)
    -- ,CONSTRAINT FK_SubscriptionPayments_CurrencyId FOREIGN KEY (CurrencyId) REFERENCES MasterCurrencies(Id)
);
GO
-- 11. PaymentRefunds
CREATE TABLE PaymentRefunds (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    SubscriptionPaymentId UNIQUEIDENTIFIER NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Reason NVARCHAR(500) NOT NULL,
    StripeRefundId NVARCHAR(100),
    RefundedAt DATETIME2 NOT NULL,
    ProcessedByUserId INT,
    IsActive BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedBy INT,
    CreatedDate DATETIME2 DEFAULT(GETUTCDATE()),
    UpdatedBy INT,
    UpdatedDate DATETIME2,
    DeletedBy INT,
    DeletedDate DATETIME2,
    CONSTRAINT FK_PaymentRefunds_SubscriptionPayment FOREIGN KEY (SubscriptionPaymentId) REFERENCES SubscriptionPayments(Id)
);
GO
-- 12. ScheduledPlanMigrations (Plan Versioning Support)
CREATE TABLE ScheduledPlanMigrations (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    SubscriptionId UNIQUEIDENTIFIER NOT NULL,
    FromPlanId UNIQUEIDENTIFIER NOT NULL,
    ToPlanId UNIQUEIDENTIFIER NOT NULL,
    NotificationDate DATETIME2 NOT NULL,
    ScheduledMigrationDate DATETIME2 NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    UserDecision NVARCHAR(50),
    UserDecisionDate DATETIME2,
    DowngradeToPlanId UNIQUEIDENTIFIER,
    CompletedDate DATETIME2,
    Notes NVARCHAR(500),
    IsActive BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedBy INT,
    CreatedDate DATETIME2 DEFAULT(GETUTCDATE()),
    UpdatedBy INT,
    UpdatedDate DATETIME2,
    DeletedBy INT,
    DeletedDate DATETIME2,
    CONSTRAINT FK_ScheduledPlanMigrations_Subscription FOREIGN KEY (SubscriptionId) REFERENCES Subscriptions(Id),
    CONSTRAINT FK_ScheduledPlanMigrations_FromPlan FOREIGN KEY (FromPlanId) REFERENCES SubscriptionPlans(Id),
    CONSTRAINT FK_ScheduledPlanMigrations_ToPlan FOREIGN KEY (ToPlanId) REFERENCES SubscriptionPlans(Id)
    -- ,CONSTRAINT FK_ScheduledPlanMigrations_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id)
    -- ,CONSTRAINT FK_ScheduledPlanMigrations_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id)
    -- ,CONSTRAINT FK_ScheduledPlanMigrations_DeletedBy FOREIGN KEY (DeletedBy) REFERENCES Users(Id)
);
GO
-- Indexes for ScheduledPlanMigrations
CREATE INDEX IX_ScheduledPlanMigrations_CreatedBy ON ScheduledPlanMigrations(CreatedBy);
CREATE INDEX IX_ScheduledPlanMigrations_DeletedBy ON ScheduledPlanMigrations(DeletedBy);
CREATE INDEX IX_ScheduledPlanMigrations_FromPlanId ON ScheduledPlanMigrations(FromPlanId);
CREATE INDEX IX_ScheduledPlanMigrations_ScheduledMigrationDate ON ScheduledPlanMigrations(ScheduledMigrationDate);
CREATE INDEX IX_ScheduledPlanMigrations_Status ON ScheduledPlanMigrations(Status);
CREATE INDEX IX_ScheduledPlanMigrations_Status_ScheduledMigrationDate ON ScheduledPlanMigrations(Status, ScheduledMigrationDate);
CREATE INDEX IX_ScheduledPlanMigrations_SubscriptionId ON ScheduledPlanMigrations(SubscriptionId);
CREATE INDEX IX_ScheduledPlanMigrations_ToPlanId ON ScheduledPlanMigrations(ToPlanId);
CREATE INDEX IX_ScheduledPlanMigrations_UpdatedBy ON ScheduledPlanMigrations(UpdatedBy);
GO
-- All FKs involving external tables (like Users, MasterCurrencies, Categories, MasterBillingCycles, etc.) are commented for manual integration per your instructions.
-- Review and uncomment them when their tables are defined in your target DB.
