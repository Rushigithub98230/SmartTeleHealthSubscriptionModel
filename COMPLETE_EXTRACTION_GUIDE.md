# Complete Subscription Management Module Extraction Guide

## 📋 Table of Contents
1. [Overview](#overview)
2. [Extraction Strategy](#extraction-strategy)
3. [Phase 1: Foundation & Configuration](#phase-1-foundation--configuration)
4. [Phase 2: Core Entities](#phase-2-core-entities)
5. [Phase 3: Master Data & Seed Scripts](#phase-3-master-data--seed-scripts)
6. [Phase 4: Interfaces & Contracts](#phase-4-interfaces--contracts)
7. [Phase 5: Utilities & Constants](#phase-5-utilities--constants)
8. [Phase 6: DTOs](#phase-6-dtos)
9. [Phase 7: AutoMapper Mapping Profiles](#phase-7-automapper-mapping-profiles)
10. [Phase 8: Repository Layer](#phase-8-repository-layer)
11. [Phase 9: Service Layer](#phase-9-service-layer)
12. [Phase 10: Background Services](#phase-10-background-services)
13. [Phase 11: Controllers](#phase-11-controllers)
14. [Phase 12: Dependency Injection](#phase-12-dependency-injection)
15. [Phase 13: Database Configuration](#phase-13-database-configuration)
16. [Phase 14: Testing & Verification](#phase-14-testing--verification)
17. [Final Checklist](#final-checklist)

---

## Overview

This guide provides step-by-step instructions for extracting the complete subscription management module from the SmartTelehealth backend. The extraction follows a **dependency-first approach** to ensure all components are extracted in the correct order.

### Extraction Order Strategy
```
Foundation → Entities → Interfaces → Utilities → DTOs → Repositories → 
Services → Controllers → Configuration → Testing
```

### Estimated File Count
- **Entities**: 15 files
- **Interfaces**: 25 files
- **Utilities**: 7 files
- **DTOs**: 25 files
- **Repositories**: 15 files
- **Services**: 20 files
- **Controllers**: 8 files
- **Migrations**: 6 files
- **Configuration**: 5 files

**Total: ~120+ files**

---

## Extraction Strategy

### Directory Structure for New Repository
```
NewSubscriptionManagementSystem/
├── SmartTelehealth.Core/
│   ├── Entities/
│   ├── Enums/
│   ├── Interfaces/
│   └── DTOs/
├── SmartTelehealth.Application/
│   ├── DTOs/
│   ├── Interfaces/
│   ├── Services/
│   ├── Utilities/
│   ├── Constants/
│   └── Mapping/
├── SmartTelehealth.Infrastructure/
│   ├── Data/
│   ├── Repositories/
│   ├── Services/
│   ├── Configuration/
│   └── Migrations/
├── SmartTelehealth.API/
│   ├── Controllers/
│   └── Startup.cs or Program.cs
└── SmartTelehealth.Tests/
    ├── Services/
    └── Integration/
```

---

## Phase 1: Foundation & Configuration

### Step 1.1: Create Base Entity
**Priority:** 🔴 CRITICAL - Required by all entities

**Extract from:** `backend/SmartTelehealth.Core/Entities/BaseEntity.cs`

**What to copy:**
```csharp
public class BaseEntity
{
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public int? UpdatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public int? DeletedBy { get; set; }
    public DateTime? DeletedDate { get; set; }
}
```

**Action:** Copy entire file to `SmartTelehealth.Core/Entities/BaseEntity.cs`

---

### Step 1.2: Create Enums
**Priority:** 🔴 CRITICAL

**Extract from:** `backend/SmartTelehealth.Core/Enums/PlanType.cs`

**What to copy:**
```csharp
public enum PlanType
{
    Standard = 0,
    UsageBased = 1,
    Premium = 2,
    Enterprise = 3
}
```

**Action:** Copy entire file to `SmartTelehealth.Core/Enums/PlanType.cs`

---

### Step 1.3: Create Core DTOs
**Priority:** 🔴 CRITICAL - Required by services

**Extract from:** `backend/SmartTelehealth.Application/DTOs/JsonModel.cs`

**What to copy:** Complete JsonModel class and TokenModel class

**Action:** Copy to `SmartTelehealth.Core/DTOs/JsonModel.cs`

---

## Phase 2: Core Entities

### Step 2.1: Master Data Entities
**Priority:** 🔴 CRITICAL - Foundation for all other entities

**Extract from:** `backend/SmartTelehealth.Core/Entities/MasterTables.cs`

**Entities to extract:**
1. ✅ **MasterBillingCycle** - Billing cycle definitions (Monthly, Quarterly, Annual)
2. ✅ **MasterCurrency** - Currency definitions (USD, EUR, GBP, etc.)
3. ✅ **MasterPrivilegeType** - Privilege type categorization
4. ✅ **PaymentStatus** - Payment status lookup table
5. ✅ **RefundStatus** - Refund status lookup table

**Dependencies:** BaseEntity

**Action:** Copy these classes to `SmartTelehealth.Core/Entities/MasterTables.cs`

**Important:** Extract ONLY these 5 master table classes. Skip others (AppointmentStatus, etc.) that are not subscription-related.

---

### Step 2.2: Supporting Entities
**Priority:** 🟡 HIGH

**Extract from:** `backend/SmartTelehealth.Core/Entities/`

**Entities to extract:**
1. ✅ **Category.cs** - Plan categorization
2. ✅ **SystemSettings.cs** - System-wide configuration
3. ✅ **User.cs** - User entity (if creating standalone module, else reference existing)

**Action:** Copy these files to `SmartTelehealth.Core/Entities/`

**Note on User.cs:** If extracting to new repo, create a minimal stub User entity with:
```csharp
public class User : BaseEntity
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    // Add other essential fields
}
```

---

### Step 2.3: Privilege Entities
**Priority:** 🟡 HIGH

**Extract from:** `backend/SmartTelehealth.Core/Entities/`

**Entities to extract:**
1. ✅ **Privilege.cs** - Privilege definitions
2. ✅ **UserSubscriptionPrivilegeUsage.cs** - Usage tracking
3. ✅ **PrivilegeUsageHistory.cs** - Historical usage records

**Dependencies:** BaseEntity, MasterPrivilegeType, Subscription, SubscriptionPlanPrivilege

**Action:** Copy these 3 files to `SmartTelehealth.Core/Entities/`

---

### Step 2.4: Subscription Plan Entities
**Priority:** 🟡 HIGH

**Extract from:** `backend/SmartTelehealth.Core/Entities/`

**Entities to extract:**
1. ✅ **SubscriptionPlan.cs** - Subscription plan template
2. ✅ **SubscriptionPlanPrivilege.cs** - Junction entity (Plan ↔ Privilege)

**Dependencies:** BaseEntity, MasterBillingCycle, MasterCurrency, Category, Privilege

**Action:** Copy these 2 files to `SmartTelehealth.Core/Entities/`

---

### Step 2.5: Subscription Entities
**Priority:** 🟡 HIGH

**Extract from:** `backend/SmartTelehealth.Core/Entities/`

**Entities to extract:**
1. ✅ **Subscription.cs** - User subscription instance
2. ✅ **SubscriptionStatusHistory.cs** - Status change tracking
3. ✅ **SubscriptionPayment.cs** - Payment records

**Dependencies:** User, SubscriptionPlan

**Action:** Copy these 3 files to `SmartTelehealth.Core/Entities/`

---

### Step 2.6: Billing Entities
**Priority:** 🟡 HIGH

**Extract from:** `backend/SmartTelehealth.Core/Entities/`

**Entities to extract:**
1. ✅ **BillingRecord.cs** - Master billing records
2. ✅ **BillingAdjustment.cs** - Billing adjustments/credits
3. ✅ **PaymentRefund.cs** - Refund records
4. ✅ **FailedRefund.cs** - Failed refund tracking

**Dependencies:** User, Subscription, MasterCurrency

**Action:** Copy these 4 files to `SmartTelehealth.Core/Entities/`

---

### Step 2.7: Plan Versioning Entities
**Priority:** 🟢 MEDIUM (if using versioning)

**Extract from:** `backend/SmartTelehealth.Core/Entities/`

**Entities to extract:**
1. ✅ **ScheduledPlanMigration.cs** - Plan migration tracking

**Dependencies:** Subscription, SubscriptionPlan

**Action:** Copy to `SmartTelehealth.Core/Entities/`

---

### Step 2.8: Webhook & Sync Entities
**Priority:** 🟢 MEDIUM (if using Stripe webhooks)

**Extract from:** `backend/SmartTelehealth.Core/Entities/`

**Entities to extract:**
1. ✅ **ProcessedWebhookEvent.cs** - Processed webhook tracking
2. ✅ **UnprocessedWebhookEvent.cs** - Failed webhook tracking
3. ✅ **StripeSyncHistory.cs** - Stripe sync audit trail

**Dependencies:** BaseEntity

**Action:** Copy these 3 files to `SmartTelehealth.Core/Entities/`

---

## Phase 3: Master Data & Seed Scripts

### Step 3.1: Create Seed Data SQL Script
**Priority:** 🔴 CRITICAL - Must run before services can work

**Create new file:** `database_scripts/seed_master_data.sql`

**Content:**
```sql
-- ==============================================
-- MASTER DATA SEEDING SCRIPT
-- Run this script after creating tables
-- ==============================================

-- 1. Seed Master Billing Cycles
IF NOT EXISTS (SELECT 1 FROM MasterBillingCycles WHERE Name = 'Monthly')
BEGIN
    INSERT INTO MasterBillingCycles (Id, Name, Description, DurationInDays, SortOrder, IsActive, CreatedDate)
    VALUES
        (NEWID(), 'Monthly', 'Monthly billing cycle', 30, 1, 1, GETUTCDATE()),
        (NEWID(), 'Quarterly', 'Quarterly billing cycle', 90, 2, 1, GETUTCDATE()),
        (NEWID(), 'Annual', 'Annual billing cycle', 365, 3, 1, GETUTCDATE())
END

-- 2. Seed Master Currencies
IF NOT EXISTS (SELECT 1 FROM MasterCurrencies WHERE Code = 'USD')
BEGIN
    INSERT INTO MasterCurrencies (Id, Code, Name, Symbol, SortOrder, IsActive, CreatedDate)
    VALUES
        (NEWID(), 'USD', 'US Dollar', '$', 1, 1, GETUTCDATE()),
        (NEWID(), 'EUR', 'Euro', '€', 2, 1, GETUTCDATE()),
        (NEWID(), 'GBP', 'British Pound', '£', 3, 1, GETUTCDATE()),
        (NEWID(), 'INR', 'Indian Rupee', '₹', 4, 1, GETUTCDATE())
END

-- 3. Seed Master Privilege Types
IF NOT EXISTS (SELECT 1 FROM MasterPrivilegeTypes WHERE Name = 'Consultation')
BEGIN
    INSERT INTO MasterPrivilegeTypes (Id, Name, Description, SortOrder, IsActive, CreatedDate)
    VALUES
        (NEWID(), 'Consultation', 'Medical consultation privileges', 1, 1, GETUTCDATE()),
        (NEWID(), 'Messaging', 'Messaging and communication privileges', 2, 1, GETUTCDATE()),
        (NEWID(), 'Medication', 'Medication delivery privileges', 3, 1, GETUTCDATE()),
        (NEWID(), 'FollowUp', 'Follow-up care privileges', 4, 1, GETUTCDATE())
END

-- 4. Seed Payment Statuses
IF NOT EXISTS (SELECT 1 FROM PaymentStatuses WHERE Name = 'Pending')
BEGIN
    INSERT INTO PaymentStatuses (Id, Name, Description, SortOrder, IsActive, CreatedDate)
    VALUES
        (NEWID(), 'Pending', 'Payment is pending', 1, 1, GETUTCDATE()),
        (NEWID(), 'Processing', 'Payment is being processed', 2, 1, GETUTCDATE()),
        (NEWID(), 'Succeeded', 'Payment was successful', 3, 1, GETUTCDATE()),
        (NEWID(), 'Failed', 'Payment failed', 4, 1, GETUTCDATE()),
        (NEWID(), 'Cancelled', 'Payment was cancelled', 5, 1, GETUTCDATE()),
        (NEWID(), 'Refunded', 'Payment was refunded', 6, 1, GETUTCDATE()),
        (NEWID(), 'PartiallyRefunded', 'Payment was partially refunded', 7, 1, GETUTCDATE())
END

-- 5. Seed Refund Statuses
IF NOT EXISTS (SELECT 1 FROM RefundStatuses WHERE Name = 'Pending')
BEGIN
    INSERT INTO RefundStatuses (Id, Name, Description, SortOrder, IsActive, CreatedDate)
    VALUES
        (NEWID(), 'Pending', 'Refund is pending', 1, 1, GETUTCDATE()),
        (NEWID(), 'Processing', 'Refund is being processed', 2, 1, GETUTCDATE()),
        (NEWID(), 'Completed', 'Refund was completed', 3, 1, GETUTCDATE()),
        (NEWID(), 'Failed', 'Refund failed', 4, 1, GETUTCDATE())
END

-- 6. Seed System Settings
IF NOT EXISTS (SELECT 1 FROM SystemSettings WHERE Key = 'AdminCommissionPercent')
BEGIN
    INSERT INTO SystemSettings (Id, Key, Value, Description, IsActive, CreatedDate)
    VALUES
        (NEWID(), 'AdminCommissionPercent', '10', 'Default admin commission percentage', 1, GETUTCDATE()),
        (NEWID(), 'MaxFailedPaymentAttempts', '3', 'Maximum failed payment attempts before suspension', 1, GETUTCDATE()),
        (NEWID(), 'DefaultBillingGracePeriodDays', '7', 'Default grace period for billing', 1, GETUTCDATE())
END

PRINT 'Master data seeded successfully';
```

**Action:** Create and save this SQL script for later execution

---

### Step 3.2: Create Sample Privileges SQL Script
**Priority:** 🟢 MEDIUM (optional - for testing)

**Create new file:** `database_scripts/seed_sample_privileges.sql`

**Content:**
```sql
-- ==============================================
-- SAMPLE PRIVILEGES SEEDING SCRIPT
-- Optional: For testing and development
-- ==============================================

DECLARE @ConsultationTypeId UNIQUEIDENTIFIER;
SELECT @ConsultationTypeId = Id FROM MasterPrivilegeTypes WHERE Name = 'Consultation';

IF NOT EXISTS (SELECT 1 FROM Privileges WHERE Name = 'TeleConsultation')
BEGIN
    INSERT INTO Privileges (Id, Name, Description, PrivilegeTypeId, IsActive, CreatedDate)
    VALUES
        (NEWID(), 'TeleConsultation', 'General teleconsultation service', @ConsultationTypeId, 1, GETUTCDATE()),
        (NEWID(), 'UrgentConsultation', 'Urgent teleconsultation service', @ConsultationTypeId, 1, GETUTCDATE()),
        (NEWID(), 'FollowUpConsultation', 'Follow-up consultation', @ConsultationTypeId, 1, GETUTCDATE())
END

DECLARE @MessagingTypeId UNIQUEIDENTIFIER;
SELECT @MessagingTypeId = Id FROM MasterPrivilegeTypes WHERE Name = 'Messaging';

IF NOT EXISTS (SELECT 1 FROM Privileges WHERE Name = 'SecureMessaging')
BEGIN
    INSERT INTO Privileges (Id, Name, Description, PrivilegeTypeId, IsActive, CreatedDate)
    VALUES
        (NEWID(), 'SecureMessaging', 'Secure messaging with healthcare provider', @MessagingTypeId, 1, GETUTCDATE())
END

PRINT 'Sample privileges seeded successfully';
```

**Action:** Create and save for optional use

---

### Step 3.3: Create Sample Category SQL Script
**Priority:** 🟢 MEDIUM (optional)

**Create new file:** `database_scripts/seed_sample_categories.sql`

**Content:**
```sql
-- ==============================================
-- SAMPLE CATEGORIES SEEDING SCRIPT
-- Optional: For testing and development
-- ==============================================

IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = 'Telehealth Basic')
BEGIN
    INSERT INTO Categories (Id, Name, Description, IsActive, CreatedDate)
    VALUES
        (NEWID(), 'Telehealth Basic', 'Basic telehealth services', 1, GETUTCDATE()),
        (NEWID(), 'Telehealth Premium', 'Premium telehealth services', 1, GETUTCDATE()),
        (NEWID(), 'Mental Health', 'Mental health services', 1, GETUTCDATE()),
        (NEWID(), 'Physical Health', 'Physical health services', 1, GETUTCDATE())
END

PRINT 'Sample categories seeded successfully';
```

**Action:** Create and save for optional use

---

## Phase 4: Interfaces & Contracts

### Step 4.1: Repository Interfaces
**Priority:** 🔴 CRITICAL - Required by repositories

**Extract from:** `backend/SmartTelehealth.Core/Interfaces/`

**Interfaces to extract:**
1. ✅ **IGenericRepository.cs** - Base generic repository
2. ✅ **IRepositoryBase.cs** - Repository base interface
3. ✅ **IUnitOfWork.cs** - Unit of work pattern

**Action:** Copy these 3 base interfaces first

---

### Step 4.2: Subscription Repository Interfaces
**Priority:** 🟡 HIGH

**Extract from:** `backend/SmartTelehealth.Core/Interfaces/`

**Interfaces to extract:**
1. ✅ **ISubscriptionPlanRepository.cs**
2. ✅ **ISubscriptionPlanPrivilegeRepository.cs**
3. ✅ **ISubscriptionRepository.cs**
4. ✅ **ISubscriptionPaymentRepository.cs**
5. ✅ **ISubscriptionStatusHistoryRepository.cs**

**Dependencies:** IGenericRepository

**Action:** Copy these 5 interfaces to `SmartTelehealth.Core/Interfaces/`

---

### Step 4.3: Privilege Repository Interfaces
**Priority:** 🟡 HIGH

**Extract from:** `backend/SmartTelehealth.Core/Interfaces/`

**Interfaces to extract:**
1. ✅ **IPrivilegeRepository.cs**
2. ✅ **IUserSubscriptionPrivilegeUsageRepository.cs**
3. ✅ **IPrivilegeUsageHistoryRepository.cs**

**Action:** Copy these 3 interfaces to `SmartTelehealth.Core/Interfaces/`

---

### Step 4.4: Billing Repository Interfaces
**Priority:** 🟡 HIGH

**Extract from:** `backend/SmartTelehealth.Core/Interfaces/`

**Interfaces to extract:**
1. ✅ **IBillingRepository.cs**
2. ✅ **IBillingAdjustmentRepository.cs**
3. ✅ **IFailedRefundRepository.cs**

**Action:** Copy these 3 interfaces to `SmartTelehealth.Core/Interfaces/`

---

### Step 4.5: Supporting Repository Interfaces
**Priority:** 🟡 HIGH

**Extract from:** `backend/SmartTelehealth.Core/Interfaces/`

**Interfaces to extract:**
1. ✅ **IScheduledPlanMigrationRepository.cs**
2. ✅ **ICategoryRepository.cs**
3. ✅ **ISystemSettingsRepository.cs**
4. ✅ **IProcessedWebhookEventRepository.cs**
5. ✅ **IUnprocessedWebhookEventRepository.cs**
6. ✅ **IUserRepository.cs** (or create stub interface)

**Action:** Copy these 6 interfaces to `SmartTelehealth.Core/Interfaces/`

---

### Step 4.6: Service Interfaces
**Priority:** 🟡 HIGH

**Extract from:** `backend/SmartTelehealth.Application/Interfaces/`

**Interfaces to extract:**
1. ✅ **ISubscriptionPlanService.cs**
2. ✅ **ISubscriptionService.cs**
3. ✅ **ISubscriptionLifecycleService.cs**
4. ✅ **ISubscriptionBillingService.cs**
5. ✅ **ISubscriptionNotificationService.cs**
6. ✅ **IPrivilegeService.cs**
7. ✅ **IPlanPricingService.cs**
8. ✅ **IPlanVersioningService.cs**
9. ✅ **IStripeService.cs**
10. ✅ **IStripeBillingService.cs**
11. ✅ **IStripeSynchronizationService.cs**
12. ✅ **IPaymentService.cs**
13. ✅ **IAutomatedBillingService.cs**
14. ✅ **ICategoryService.cs**
15. ✅ **INotificationService.cs**
16. ✅ **IWebhookService.cs**
17. ✅ **IWebhookIdempotencyService.cs**
18. ✅ **IUserService.cs** (or create stub interface)

**Action:** Copy all these interfaces to `SmartTelehealth.Application/Interfaces/`

---

## Phase 5: Utilities & Constants

### Step 5.1: Constants
**Priority:** 🟡 HIGH

**Extract from:** `backend/SmartTelehealth.Application/Constants/SubscriptionConstants.cs`

**What to copy:** Complete file

**Action:** Copy to `SmartTelehealth.Application/Constants/SubscriptionConstants.cs`

---

### Step 5.2: Utility Classes
**Priority:** 🟡 HIGH

**Extract from:** `backend/SmartTelehealth.Application/Utilities/`

**Utilities to extract:**
1. ✅ **BillingCalculationService.cs** - Billing price calculations
2. ✅ **BillingCycleCalculator.cs** - Billing cycle date calculations
3. ✅ **PrivilegeAllocationCalculator.cs** - Privilege allocation logic
4. ✅ **PrivilegeResetHelper.cs** - Privilege reset utilities
5. ✅ **BillingValidationService.cs** - Validation helpers
6. ✅ **CurrencyService.cs** - Currency operations (if exists)

**Dependencies:** Core.Entities

**Action:** Copy all 6 utility files to `SmartTelehealth.Application/Utilities/`

---

## Phase 6: DTOs

### Step 6.1: Core DTOs (Already extracted in Phase 1.3)
- ✅ JsonModel
- ✅ TokenModel

---

### Step 6.2: Subscription Plan DTOs
**Priority:** 🟡 HIGH

**Extract from:** `backend/SmartTelehealth.Application/DTOs/`

**DTOs to extract:**
1. ✅ **SubscriptionPlanDto.cs**
2. ✅ **CreateSubscriptionPlanDto.cs**
3. ✅ **UpdateSubscriptionPlanDto.cs** (may be combined with CreateSubscriptionPlanDto.cs)
4. ✅ **SubscriptionPlanFilterDto.cs** (in Core/DTOs)

**Action:** Copy these files to `SmartTelehealth.Application/DTOs/`

---

### Step 6.3: Subscription DTOs
**Priority:** 🟡 HIGH

**Extract from:** `backend/SmartTelehealth.Application/DTOs/`

**DTOs to extract:**
1. ✅ **SubscriptionDto.cs**
2. ✅ **CreateSubscriptionDto.cs**
3. ✅ **UpdateSubscriptionDto.cs**

**Action:** Copy these files to `SmartTelehealth.Application/DTOs/`

---

### Step 6.4: Privilege DTOs
**Priority:** 🟡 HIGH

**Extract from:** `backend/SmartTelehealth.Application/DTOs/`

**DTOs to extract:**
1. ✅ **PrivilegeDto.cs**
2. ✅ **CreatePrivilegeDto.cs**
3. ✅ **UpdatePrivilegeDto.cs**
4. ✅ **PrivilegeUsageDto.cs**
5. ✅ **TrackUsageDto.cs**
6. ✅ **UserPrivilegeUsageDto.cs**

**Action:** Copy these files to `SmartTelehealth.Application/DTOs/`

---

### Step 6.5: Billing DTOs
**Priority:** 🟡 HIGH

**Extract from:** `backend/SmartTelehealth.Application/DTOs/`

**DTOs to extract:**
1. ✅ **BillingRecordDto.cs**
2. ✅ **BillingDto.cs**
3. ✅ **CreateBillingRecordDto.cs**
4. ✅ **UpdateBillingRecordDto.cs**
5. ✅ **BillingAdjustmentDto.cs**
6. ✅ **CreateBillingAdjustmentDto.cs**
7. ✅ **PaymentRequestDto.cs**
8. ✅ **PaymentResultDto.cs**
9. ✅ **ProcessPrivilegeUsageDto.cs**
10. ✅ **PrivilegeBasedBillingDto.cs**

**Action:** Copy these files to `SmartTelehealth.Application/DTOs/`

---

### Step 6.6: Plan Versioning DTOs
**Priority:** 🟢 MEDIUM

**Extract from:** `backend/SmartTelehealth.Application/DTOs/`

**DTOs to extract:**
1. ✅ **CreatePlanVersionRequestDto.cs**
2. ✅ **MigrateUsersRequestDto.cs**
3. ✅ **SchedulePlanChangeDto.cs**
4. ✅ **PlanVersionDto.cs**

**Action:** Copy these files to `SmartTelehealth.Application/DTOs/`

---

### Step 6.7: Support DTOs
**Priority:** 🟢 MEDIUM

**Extract from:** `backend/SmartTelehealth.Application/DTOs/`

**DTOs to extract:**
1. ✅ **CategoryDto.cs**
2. ✅ **CancelSubscriptionDto.cs**
3. ✅ **ChangePlanRequest.cs**
4. ✅ **SubscriptionAutomationDtos.cs** (or individual DTOs within)
5. ✅ **SubscriptionDashboardDto.cs**
6. ✅ **UsageStatisticsDto.cs**
7. ✅ **PricingBreakdownDto.cs**

**Action:** Copy these files to `SmartTelehealth.Application/DTOs/`

---

## Phase 7: AutoMapper Mapping Profiles

### Step 7.1: Extract Mapping Profile
**Priority:** 🟡 HIGH

**Extract from:** `backend/SmartTelehealth.Application/Mapping/MappingProfile.cs`

**What to extract:**
Extract ONLY subscription-related mappings from the MappingProfile.cs file.

**Mappings to extract:**
1. ✅ CreateSubscriptionPlanDto → SubscriptionPlan
2. ✅ UpdateSubscriptionPlanDto → SubscriptionPlan
3. ✅ SubscriptionPlan → SubscriptionPlanDto
4. ✅ CreateSubscriptionDto → Subscription
5. ✅ Subscription → SubscriptionDto
6. ✅ Privilege → PrivilegeDto
7. ✅ Category → CategoryDto
8. ✅ BillingRecord → BillingRecordDto
9. ✅ SubscriptionPayment mappings

**Action:** Create new file `SmartTelehealth.Application/Mapping/SubscriptionMappingProfile.cs` and include only subscription-related mappings

---

### Step 7.2: Register AutoMapper
**See Phase 12: Dependency Injection**

---

## Phase 8: Repository Layer

### Step 8.1: Base Repository Implementation
**Priority:** 🔴 CRITICAL

**Extract from:** `backend/SmartTelehealth.Infrastructure/Repositories/`

**Files to extract:**
1. ✅ **GenericRepository.cs**
2. ✅ **RepositoryBase.cs**
3. ✅ **UnitOfWork.cs**

**Dependencies:** IGenericRepository, IRepositoryBase, IUnitOfWork, ApplicationDbContext

**Action:** Copy these 3 files to `SmartTelehealth.Infrastructure/Repositories/`

---

### Step 8.2: Subscription Repositories
**Priority:** 🟡 HIGH

**Extract from:** `backend/SmartTelehealth.Infrastructure/Repositories/`

**Files to extract:**
1. ✅ **SubscriptionPlanRepository.cs**
2. ✅ **SubscriptionPlanPrivilegeRepository.cs**
3. ✅ **SubscriptionRepository.cs**
4. ✅ **SubscriptionPaymentRepository.cs**
5. ✅ **SubscriptionStatusHistoryRepository.cs**

**Dependencies:** GenericRepository, UnitOfWork, respective interfaces

**Action:** Copy these 5 files to `SmartTelehealth.Infrastructure/Repositories/`

---

### Step 8.3: Privilege Repositories
**Priority:** 🟡 HIGH

**Extract from:** `backend/SmartTelehealth.Infrastructure/Repositories/`

**Files to extract:**
1. ✅ **PrivilegeRepository.cs**
2. ✅ **UserSubscriptionPrivilegeUsageRepository.cs**
3. ✅ **PrivilegeUsageHistoryRepository.cs**

**Action:** Copy these 3 files to `SmartTelehealth.Infrastructure/Repositories/`

---

### Step 8.4: Billing Repositories
**Priority:** 🟡 HIGH

**Extract from:** `backend/SmartTelehealth.Infrastructure/Repositories/`

**Files to extract:**
1. ✅ **BillingRepository.cs**
2. ✅ **BillingAdjustmentRepository.cs**
3. ✅ **PaymentRefundRepository.cs** (if separate file exists)
4. ✅ **FailedRefundRepository.cs**

**Action:** Copy these files to `SmartTelehealth.Infrastructure/Repositories/`

---

### Step 8.5: Supporting Repositories
**Priority:** 🟡 HIGH

**Extract from:** `backend/SmartTelehealth.Infrastructure/Repositories/`

**Files to extract:**
1. ✅ **ScheduledPlanMigrationRepository.cs**
2. ✅ **CategoryRepository.cs**
3. ✅ **SystemSettingsRepository.cs**
4. ✅ **ProcessedWebhookEventRepository.cs**
5. ✅ **UnprocessedWebhookEventRepository.cs**
6. ✅ **UserRepository.cs** (or create stub)

**Action:** Copy these files to `SmartTelehealth.Infrastructure/Repositories/`

---

## Phase 9: Service Layer

### Step 9.1: Core Services (Foundation)
**Priority:** 🟡 HIGH

**Extract from:** `backend/SmartTelehealth.Application/Services/`

**Services to extract:**
1. ✅ **IPlanPricingService.cs** and **PlanPricingService.cs** - Must register FIRST
2. ✅ **IStripeSynchronizationService.cs** and **StripeSynchronizationService.cs** - Must register early
3. ✅ **IPrivilegeService.cs** and **PrivilegeService.cs**
4. ✅ **IPaymentService.cs** and **PaymentService.cs**

**Action:** Copy these service pairs (interface + implementation) to `SmartTelehealth.Application/Services/`

---

### Step 9.2: Subscription Services
**Priority:** 🟡 HIGH

**Extract from:** `backend/SmartTelehealth.Application/Services/`

**Services to extract:**
1. ✅ **ISubscriptionPlanService.cs** and **SubscriptionPlanService.cs**
2. ✅ **ISubscriptionService.cs** and **SubscriptionService.cs**
3. ✅ **ISubscriptionLifecycleService.cs** and **SubscriptionLifecycleService.cs**

**Dependencies:** Services from Step 9.1

**Action:** Copy these service pairs to `SmartTelehealth.Application/Services/`

---

### Step 9.3: Billing Services
**Priority:** 🟡 HIGH

**Extract from:** `backend/SmartTelehealth.Application/Services/`

**Services to extract:**
1. ✅ **ISubscriptionBillingService.cs** and **SubscriptionBillingService.cs** (51 methods)
2. ✅ **IPlanVersioningService.cs** and **PlanVersioningService.cs**
3. ✅ **IAutomatedBillingService.cs** and **AutomatedBillingService.cs**

**Action:** Copy these service pairs to `SmartTelehealth.Application/Services/`

---

### Step 9.4: Stripe Services
**Priority:** 🟡 HIGH

**Extract from:** `backend/SmartTelehealth.Infrastructure/Services/`

**Services to extract:**
1. ✅ **IStripeService.cs** and **StripeService.cs**
2. ✅ **IStripeBillingService.cs** and **StripeBillingService.cs**

**Dependencies:** Stripe NuGet package

**Action:** Copy these service pairs to `SmartTelehealth.Infrastructure/Services/`

---

### Step 9.5: Supporting Services
**Priority:** 🟡 HIGH

**Extract from:** `backend/SmartTelehealth.Application/Services/` and `backend/SmartTelehealth.Infrastructure/Services/`

**Services to extract:**
1. ✅ **ICategoryService.cs** and **CategoryService.cs**
2. ✅ **ISubscriptionNotificationService.cs** and **SubscriptionNotificationService.cs**
3. ✅ **INotificationService.cs** and **NotificationService.cs**
4. ✅ **IWebhookService.cs** and **WebhookService.cs**
5. ✅ **IWebhookIdempotencyService.cs** and **WebhookIdempotencyService.cs**
6. ✅ **IUserService.cs** and **UserService.cs** (or create stub)
7. ✅ **IReconciliationService.cs** and **ReconciliationService.cs**

**Action:** Copy these service pairs to appropriate folders

---

## Phase 10: Background Services

### Step 10.1: Automated Billing Background Service
**Priority:** 🟡 HIGH

**Extract from:** `backend/SmartTelehealth.Infrastructure/Services/AutomatedBillingBackgroundService.cs`

**What to extract:** Complete file

**Dependencies:** ISubscriptionRepository, ISubscriptionBillingService, IAutomatedBillingService

**Action:** Copy to `SmartTelehealth.Infrastructure/Services/`

---

### Step 10.2: Privilege Reset Background Service
**Priority:** 🟡 HIGH

**Extract from:** `backend/SmartTelehealth.Infrastructure/Services/PrivilegeResetBackgroundService.cs`

**What to extract:** Complete file

**Dependencies:** IUserSubscriptionPrivilegeUsageRepository, IPrivilegeRepository

**Action:** Copy to `SmartTelehealth.Infrastructure/Services/`

---

### Step 10.3: Scheduled Migration Background Service
**Priority:** 🟢 MEDIUM (if using versioning)

**Extract from:** `backend/SmartTelehealth.Infrastructure/Services/ScheduledMigrationBackgroundService.cs`

**What to extract:** Complete file

**Dependencies:** IScheduledPlanMigrationRepository, ISubscriptionLifecycleService

**Action:** Copy to `SmartTelehealth.Infrastructure/Services/`

---

### Step 10.4: Other Background Services
**Priority:** 🟢 MEDIUM

**Extract from:** `backend/SmartTelehealth.Infrastructure/Services/`

**Services to extract:**
1. ✅ **FailedRefundRetryBackgroundService.cs**
2. ✅ **UnprocessedWebhookRetryService.cs**
3. ✅ **StripeSyncJob.cs**
4. ✅ **ReconciliationBackgroundService.cs**

**Action:** Copy these files to `SmartTelehealth.Infrastructure/Services/`

---

## Phase 11: Controllers

### Step 11.1: Base Controller
**Priority:** 🔴 CRITICAL

**Extract from:** `backend/SmartTelehealth.API/Controllers/BaseController.cs`

**What to extract:** GetToken method and base functionality

**Action:** Copy to `SmartTelehealth.API/Controllers/BaseController.cs`

---

### Step 11.2: Subscription Controllers
**Priority:** 🟡 HIGH

**Extract from:** `backend/SmartTelehealth.API/Controllers/`

**Controllers to extract:**
1. ✅ **SubscriptionPlansController.cs**
2. ✅ **SubscriptionsController.cs**
3. ✅ **BillingController.cs**

**Action:** Copy these 3 files to `SmartTelehealth.API/Controllers/`

---

### Step 11.3: Stripe Controllers
**Priority:** 🟡 HIGH

**Extract from:** `backend/SmartTelehealth.API/Controllers/`

**Controllers to extract:**
1. ✅ **StripeController.cs** (checkout, payment methods)
2. ✅ **StripeWebhookController.cs** (webhook processing)

**Action:** Copy these 2 files to `SmartTelehealth.API/Controllers/`

---

### Step 11.4: Supporting Controllers
**Priority:** 🟢 MEDIUM

**Extract from:** `backend/SmartTelehealth.API/Controllers/`

**Controllers to extract (if needed):**
1. ✅ **CategoriesController.cs** (if managing categories)
2. ✅ **PrivilegesController.cs** (if separate controller exists)

**Action:** Copy if they exist to `SmartTelehealth.API/Controllers/`

---

## Phase 12: Dependency Injection

### Step 12.1: Application Dependency Injection
**Priority:** 🔴 CRITICAL

**Extract from:** `backend/SmartTelehealth.Application/DependencyInjection.cs`

**Create new file:** `SmartTelehealth.Application/DependencyInjection.cs`

**What to include:**
```csharp
public static IServiceCollection AddApplicationServices(this IServiceCollection services)
{
    // 1. Register AutoMapper FIRST
    services.AddAutoMapper(typeof(DependencyInjection).Assembly);
    
    // 2. Register services in dependency order
    // Services with no subscription dependencies first
    services.AddScoped<IUserService, UserService>();
    services.AddScoped<ICategoryService, CategoryService>();
    services.AddScoped<IStripeSynchronizationService, StripeSynchronizationService>();
    services.AddScoped<IPlanPricingService, PlanPricingService>(...);
    
    // Then services that depend on above
    services.AddScoped<ISubscriptionPlanService, SubscriptionPlanService>(...);
    services.AddScoped<IPrivilegeService, PrivilegeService>();
    services.AddScoped<ISubscriptionBillingService, SubscriptionBillingService>(...);
    services.AddScoped<ISubscriptionLifecycleService, SubscriptionLifecycleService>(...);
    services.AddScoped<ISubscriptionService, SubscriptionService>(...);
    
    // Supporting services
    services.AddScoped<IPaymentService, PaymentService>(...);
    services.AddScoped<INotificationService, NotificationService>();
    services.AddScoped<ISubscriptionNotificationService, SubscriptionNotificationService>();
    
    // Webhook and automation
    services.AddScoped<IWebhookIdempotencyService, WebhookIdempotencyService>();
    services.AddScoped<IWebhookService, WebhookService>(...);
    services.AddScoped<IAutomatedBillingService, AutomatedBillingService>(...);
    
    return services;
}
```

**Action:** Copy subscription-related service registrations from the original file

---

### Step 12.2: Infrastructure Dependency Injection
**Priority:** 🔴 CRITICAL

**Extract from:** `backend/SmartTelehealth.Infrastructure/DependencyInjection.cs`

**Create new file:** `SmartTelehealth.Infrastructure/DependencyInjection.cs`

**What to include:**
```csharp
public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
{
    // 1. Database
    services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")), 
        ServiceLifetime.Scoped);
    
    // 2. Register ALL repositories (subscription-related)
    services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
    services.AddScoped<IUnitOfWork, UnitOfWork>();
    
    // Subscription repositories
    services.AddScoped<ISubscriptionPlanRepository, SubscriptionPlanRepository>();
    services.AddScoped<ISubscriptionPlanPrivilegeRepository, SubscriptionPlanPrivilegeRepository>();
    services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
    services.AddScoped<ISubscriptionPaymentRepository, SubscriptionPaymentRepository>();
    services.AddScoped<ISubscriptionStatusHistoryRepository, SubscriptionStatusHistoryRepository>();
    
    // Privilege repositories
    services.AddScoped<IPrivilegeRepository, PrivilegeRepository>();
    services.AddScoped<IUserSubscriptionPrivilegeUsageRepository, UserSubscriptionPrivilegeUsageRepository>();
    services.AddScoped<IPrivilegeUsageHistoryRepository, PrivilegeUsageHistoryRepository>();
    
    // Billing repositories
    services.AddScoped<IBillingRepository, BillingRepository>();
    services.AddScoped<IBillingAdjustmentRepository, BillingAdjustmentRepository>();
    services.AddScoped<IFailedRefundRepository, FailedRefundRepository>();
    
    // Supporting repositories
    services.AddScoped<IScheduledPlanMigrationRepository, ScheduledPlanMigrationRepository>();
    services.AddScoped<ICategoryRepository, CategoryRepository>();
    services.AddScoped<ISystemSettingsRepository, SystemSettingsRepository>();
    services.AddScoped<IProcessedWebhookEventRepository, ProcessedWebhookEventRepository>();
    services.AddScoped<IUnprocessedWebhookEventRepository, UnprocessedWebhookEventRepository>();
    services.AddScoped<IUserRepository, UserRepository>();
    
    // 3. Register Stripe services
    services.AddScoped<IStripeService, StripeService>();
    services.AddScoped<IStripeBillingService, StripeBillingService>();
    
    // 4. Register notification services
    services.AddScoped<INotificationService, NotificationService>();
    services.AddScoped<ICommunicationService, TwilioService>(); // Or your notification service
    
    // 5. Register background services as hosted services
    services.AddHostedService<AutomatedBillingBackgroundService>();
    services.AddHostedService<PrivilegeResetBackgroundService>();
    services.AddHostedService<ScheduledMigrationBackgroundService>();
    services.AddHostedService<FailedRefundRetryBackgroundService>();
    services.AddHostedService<UnprocessedWebhookRetryService>();
    services.AddHostedService<StripeSyncJob>();
    services.AddHostedService<ReconciliationBackgroundService>();
    
    // Also register them as scoped services
    services.AddScoped<IAutomatedBillingBackgroundService, AutomatedBillingBackgroundService>();
    
    return services;
}
```

**Action:** Copy subscription-related registrations from the original file

---

### Step 12.3: API/Program.cs Configuration
**Priority:** 🔴 CRITICAL

**Extract from:** `backend/SmartTelehealth.API/Program.cs`

**What to configure in your new Program.cs or Startup.cs:**
```csharp
// 1. Add database context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Add application services
builder.Services.AddApplicationServices();

// 3. Add infrastructure services
builder.Services.AddInfrastructure(builder.Configuration);

// 4. Configure JWT (if needed)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { /* JWT config */ });

// 5. Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 6. Add Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Subscription Management API", Version = "v1" });
});

var app = builder.Build();

// 7. Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();
    
    // OPTIONAL: Seed master data
    SeedData.SeedMasterTables(context);
}

// 8. Configure middleware
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

**Action:** Configure your API startup to use the dependency injection extensions

---

## Phase 13: Database Configuration

### Step 13.1: Extract DbContext
**Priority:** 🔴 CRITICAL

**Extract from:** `backend/SmartTelehealth.Infrastructure/Data/ApplicationDbContext.cs`

**What to extract:**
Create new file: `SmartTelehealth.Infrastructure/Data/ApplicationDbContext.cs`

**Include DbSets:**
```csharp
public class ApplicationDbContext : DbContext
{
    // Master Tables
    public DbSet<MasterBillingCycle> MasterBillingCycles { get; set; }
    public DbSet<MasterCurrency> MasterCurrencies { get; set; }
    public DbSet<MasterPrivilegeType> MasterPrivilegeTypes { get; set; }
    public DbSet<PaymentStatus> PaymentStatuses { get; set; }
    public DbSet<RefundStatus> RefundStatuses { get; set; }
    
    // Subscription Entities
    public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
    public DbSet<SubscriptionPlanPrivilege> SubscriptionPlanPrivileges { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<SubscriptionPayment> SubscriptionPayments { get; set; }
    public DbSet<SubscriptionStatusHistory> SubscriptionStatusHistories { get; set; }
    
    // Privilege Entities
    public DbSet<Privilege> Privileges { get; set; }
    public DbSet<UserSubscriptionPrivilegeUsage> UserSubscriptionPrivilegeUsages { get; set; }
    public DbSet<PrivilegeUsageHistory> PrivilegeUsageHistories { get; set; }
    
    // Billing Entities
    public DbSet<BillingRecord> BillingRecords { get; set; }
    public DbSet<BillingAdjustment> BillingAdjustments { get; set; }
    public DbSet<PaymentRefund> PaymentRefunds { get; set; }
    public DbSet<FailedRefund> FailedRefunds { get; set; }
    
    // Versioning
    public DbSet<ScheduledPlanMigration> ScheduledPlanMigrations { get; set; }
    
    // Supporting
    public DbSet<Category> Categories { get; set; }
    public DbSet<SystemSettings> SystemSettings { get; set; }
    public DbSet<User> Users { get; set; }
    
    // Webhook & Sync
    public DbSet<ProcessedWebhookEvent> ProcessedWebhookEvents { get; set; }
    public DbSet<UnprocessedWebhookEvent> UnprocessedWebhookEvents { get; set; }
    public DbSet<StripeSyncHistory> StripeSyncHistories { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure entity relationships
        ConfigureSubscriptionPlanRelationships(modelBuilder);
        ConfigureSubscriptionRelationships(modelBuilder);
        ConfigureBillingRelationships(modelBuilder);
        ConfigurePrivilegeRelationships(modelBuilder);
        // ... other configurations
    }
}
```

**Action:** Copy DbContext configuration from original file, include only subscription-related DbSets and relationships

---

### Step 13.2: Create Database Migration
**Priority:** 🔴 CRITICAL

**Use existing SQL script:** `backend/SmartTelehealth.Infrastructure/Migrations/Subscription_Management_CreateTables.sql`

**Or create EF Core Migration:**
```bash
dotnet ef migrations add InitialSubscriptionManagement --project SmartTelehealth.Infrastructure --startup-project SmartTelehealth.API
```

**Action:**
1. Copy `Subscription_Management_CreateTables.sql` to your new project
2. Run the SQL script on your target database, OR
3. Create EF Core migration and apply it

---

### Step 13.3: Run Seed Scripts
**Priority:** 🟡 HIGH

**Execute scripts in this order:**
1. Run `Subscription_Management_CreateTables.sql` (Phase 13.2)
2. Run `seed_master_data.sql` (Phase 3.1)
3. [OPTIONAL] Run `seed_sample_privileges.sql` (Phase 3.2)
4. [OPTIONAL] Run `seed_sample_categories.sql` (Phase 3.3)

**Action:** Execute scripts in sequence

---

## Phase 14: Testing & Verification

### Step 14.1: Install NuGet Packages
**Priority:** 🔴 CRITICAL

**Required packages for new project:**
```xml
<ItemGroup>
  <!-- Database -->
  <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.0" />
  
  <!-- AutoMapper -->
  <PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.1" />
  
  <!-- Stripe -->
  <PackageReference Include="Stripe.net" Version="43.0.0" />
  
  <!-- Utilities -->
  <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
  <PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
  
  <!-- API -->
  <PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
  <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
</ItemGroup>
```

**Action:** Add these packages to your `.csproj` files

---

### Step 14.2: Configuration Files
**Priority:** 🔴 CRITICAL

**Create `appsettings.json`:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=SubscriptionManagement;Trusted_Connection=true;TrustServerCertificate=true"
  },
  "StripeSettings": {
    "SecretKey": "sk_test_YOUR_SECRET_KEY",
    "PublishableKey": "pk_test_YOUR_PUBLISHABLE_KEY",
    "WebhookSecret": "whsec_YOUR_WEBHOOK_SECRET",
    "WebhookRetryAttempts": 3,
    "WebhookRetryDelaySeconds": 5,
    "ReturnUrl": "https://yourapp.com/payment/success",
    "CancelUrl": "https://yourapp.com/payment/cancel"
  },
  "JwtSettings": {
    "SecretKey": "your-jwt-secret-key-min-32-chars",
    "Issuer": "SubscriptionManagement",
    "Audience": "SubscriptionManagementUsers",
    "ExpirationHours": 24
  },
  "NotificationSettings": {
    "EnableEmail": true,
    "EnableSms": false,
    "FromEmail": "noreply@yourapp.com",
    "FromName": "Subscription Management"
  }
}
```

**Action:** Create appsettings.json with appropriate configuration

---

### Step 14.3: Build and Verify
**Priority:** 🟡 HIGH

**Commands:**
```bash
# 1. Restore packages
dotnet restore

# 2. Build solution
dotnet build

# 3. Fix any compilation errors
# - Missing dependencies
# - Namespace issues
# - Missing references

# 4. Run migrations
dotnet ef database update --project SmartTelehealth.Infrastructure --startup-project SmartTelehealth.API

# 5. Run application
dotnet run --project SmartTelehealth.API
```

**Action:** Build and fix any issues

---

### Step 14.4: Test Critical Flows
**Priority:** 🟡 HIGH

**Test these flows:**
1. ✅ **Create Subscription Plan**
   - POST /api/subscriptionplans
   - Verify plan created in database

2. ✅ **Create Subscription**
   - POST /api/subscriptions
   - Verify subscription and billing records created

3. ✅ **Process Privilege Usage**
   - Use privilege
   - Verify usage tracking works

4. ✅ **Automated Billing**
   - Wait for AutomatedBillingBackgroundService to run
   - Verify billing records created

5. ✅ **Stripe Webhook**
   - Send test webhook
   - Verify webhook processed

**Action:** Test each flow and fix issues

---

## Final Checklist

### ✅ Entities (15 files)
- [ ] BaseEntity.cs
- [ ] SubscriptionPlan.cs
- [ ] SubscriptionPlanPrivilege.cs
- [ ] Subscription.cs
- [ ] SubscriptionPayment.cs
- [ ] SubscriptionStatusHistory.cs
- [ ] Privilege.cs
- [ ] UserSubscriptionPrivilegeUsage.cs
- [ ] PrivilegeUsageHistory.cs
- [ ] BillingRecord.cs
- [ ] BillingAdjustment.cs
- [ ] PaymentRefund.cs
- [ ] FailedRefund.cs
- [ ] ScheduledPlanMigration.cs
- [ ] Category.cs
- [ ] SystemSettings.cs
- [ ] Master Billing Cycle (from MasterTables.cs)
- [ ] Master Currency (from MasterTables.cs)
- [ ] Master Privilege Type (from MasterTables.cs)
- [ ] PaymentStatus (from MasterTables.cs)
- [ ] RefundStatus (from MasterTables.cs)

### ✅ Enums (1 file)
- [ ] PlanType.cs

### ✅ Interfaces (25 files)
- [ ] IGenericRepository.cs
- [ ] IRepositoryBase.cs
- [ ] IUnitOfWork.cs
- [ ] ISubscriptionPlanRepository.cs
- [ ] ISubscriptionPlanPrivilegeRepository.cs
- [ ] ISubscriptionRepository.cs
- [ ] ISubscriptionPaymentRepository.cs
- [ ] ISubscriptionStatusHistoryRepository.cs
- [ ] IPrivilegeRepository.cs
- [ ] IUserSubscriptionPrivilegeUsageRepository.cs
- [ ] IPrivilegeUsageHistoryRepository.cs
- [ ] IBillingRepository.cs
- [ ] IBillingAdjustmentRepository.cs
- [ ] IFailedRefundRepository.cs
- [ ] IScheduledPlanMigrationRepository.cs
- [ ] ICategoryRepository.cs
- [ ] ISystemSettingsRepository.cs
- [ ] IProcessedWebhookEventRepository.cs
- [ ] IUnprocessedWebhookEventRepository.cs
- [ ] IUserRepository.cs
- [ ] ISubscriptionPlanService.cs
- [ ] ISubscriptionService.cs
- [ ] ISubscriptionLifecycleService.cs
- [ ] ISubscriptionBillingService.cs
- [ ] IStripeService.cs
- [ ] IStripeBillingService.cs
- [ ] IStripeSynchronizationService.cs
- [ ] IPrivilegeService.cs
- [ ] IPlanPricingService.cs
- [ ] IPlanVersioningService.cs
- [ ] IAutomatedBillingService.cs
- [ ] IWebhookService.cs
- [ ] IWebhookIdempotencyService.cs
- [ ] INotificationService.cs
- [ ] ICategoryService.cs
- [ ] IUserService.cs

### ✅ Utilities (7 files)
- [ ] SubscriptionConstants.cs
- [ ] BillingCalculationService.cs
- [ ] BillingCycleCalculator.cs
- [ ] PrivilegeAllocationCalculator.cs
- [ ] PrivilegeResetHelper.cs
- [ ] BillingValidationService.cs
- [ ] CurrencyService.cs (if exists)

### ✅ DTOs (25+ files)
- [ ] JsonModel.cs (Core/DTOs)
- [ ] TokenModel.cs (Core/DTOs)
- [ ] SubscriptionPlanDto.cs
- [ ] CreateSubscriptionPlanDto.cs
- [ ] UpdateSubscriptionPlanDto.cs
- [ ] SubscriptionPlanFilterDto.cs
- [ ] SubscriptionDto.cs
- [ ] CreateSubscriptionDto.cs
- [ ] UpdateSubscriptionDto.cs
- [ ] PrivilegeDto.cs
- [ ] CreatePrivilegeDto.cs
- [ ] UpdatePrivilegeDto.cs
- [ ] PrivilegeUsageDto.cs
- [ ] TrackUsageDto.cs
- [ ] UserPrivilegeUsageDto.cs
- [ ] BillingRecordDto.cs
- [ ] BillingDto.cs
- [ ] CreateBillingRecordDto.cs
- [ ] UpdateBillingRecordDto.cs
- [ ] BillingAdjustmentDto.cs
- [ ] CreateBillingAdjustmentDto.cs
- [ ] PaymentRequestDto.cs
- [ ] PaymentResultDto.cs
- [ ] ProcessPrivilegeUsageDto.cs
- [ ] CategoryDto.cs
- [ ] CreatePlanVersionRequestDto.cs
- [ ] MigrateUsersRequestDto.cs
- [ ] SchedulePlanChangeDto.cs
- [ ] PlanVersionDto.cs
- [ ] Other supporting DTOs

### ✅ Mapping (1 file)
- [ ] SubscriptionMappingProfile.cs

### ✅ Repositories (15 files)
- [ ] GenericRepository.cs
- [ ] RepositoryBase.cs
- [ ] UnitOfWork.cs
- [ ] SubscriptionPlanRepository.cs
- [ ] SubscriptionPlanPrivilegeRepository.cs
- [ ] SubscriptionRepository.cs
- [ ] SubscriptionPaymentRepository.cs
- [ ] SubscriptionStatusHistoryRepository.cs
- [ ] PrivilegeRepository.cs
- [ ] UserSubscriptionPrivilegeUsageRepository.cs
- [ ] PrivilegeUsageHistoryRepository.cs
- [ ] BillingRepository.cs
- [ ] BillingAdjustmentRepository.cs
- [ ] FailedRefundRepository.cs
- [ ] ScheduledPlanMigrationRepository.cs
- [ ] CategoryRepository.cs
- [ ] SystemSettingsRepository.cs
- [ ] ProcessedWebhookEventRepository.cs
- [ ] UnprocessedWebhookEventRepository.cs
- [ ] UserRepository.cs

### ✅ Services (20 files)
- [ ] SubscriptionPlanService.cs
- [ ] SubscriptionService.cs
- [ ] SubscriptionLifecycleService.cs
- [ ] SubscriptionBillingService.cs
- [ ] PrivilegeService.cs
- [ ] PlanPricingService.cs
- [ ] PlanVersioningService.cs
- [ ] AutomatedBillingService.cs
- [ ] StripeService.cs
- [ ] StripeBillingService.cs
- [ ] StripeSynchronizationService.cs
- [ ] PaymentService.cs
- [ ] WebhookService.cs
- [ ] WebhookIdempotencyService.cs
- [ ] NotificationService.cs
- [ ] SubscriptionNotificationService.cs
- [ ] CategoryService.cs
- [ ] ReconciliationService.cs
- [ ] UserService.cs (or stub)

### ✅ Background Services (7 files)
- [ ] AutomatedBillingBackgroundService.cs
- [ ] PrivilegeResetBackgroundService.cs
- [ ] ScheduledMigrationBackgroundService.cs
- [ ] FailedRefundRetryBackgroundService.cs
- [ ] UnprocessedWebhookRetryService.cs
- [ ] StripeSyncJob.cs
- [ ] ReconciliationBackgroundService.cs

### ✅ Controllers (5 files)
- [ ] BaseController.cs
- [ ] SubscriptionPlansController.cs
- [ ] SubscriptionsController.cs
- [ ] BillingController.cs
- [ ] StripeController.cs
- [ ] StripeWebhookController.cs

### ✅ Configuration (3 files)
- [ ] Application DependencyInjection.cs
- [ ] Infrastructure DependencyInjection.cs
- [ ] Program.cs or Startup.cs

### ✅ Database (4 files)
- [ ] ApplicationDbContext.cs
- [ ] Subscription_Management_CreateTables.sql
- [ ] seed_master_data.sql
- [ ] seed_sample_privileges.sql (optional)
- [ ] seed_sample_categories.sql (optional)

### ✅ App Configuration
- [ ] appsettings.json
- [ ] .csproj files with NuGet packages

---

## Post-Extraction Tasks

### 1. Verify All Services Start
- [ ] Check AutomatedBillingBackgroundService starts
- [ ] Check PrivilegeResetBackgroundService starts
- [ ] Check all other background services start
- [ ] Verify no startup errors

### 2. Test Database
- [ ] Verify all tables created
- [ ] Verify all foreign keys working
- [ ] Verify seed data loaded

### 3. Test API Endpoints
- [ ] GET /api/subscriptionplans/active
- [ ] POST /api/subscriptionplans
- [ ] GET /api/subscriptions/{id}
- [ ] POST /api/subscriptions
- [ ] POST /api/stripewebhook/webhook

### 4. Test Stripe Integration
- [ ] Verify Stripe keys configured
- [ ] Test checkout session creation
- [ ] Test webhook processing
- [ ] Verify subscription sync

### 5. Monitor Logs
- [ ] Check for errors in application logs
- [ ] Verify background services executing
- [ ] Check Stripe sync running

---

## Troubleshooting Common Issues

### Issue 1: Missing Dependency
**Symptom:** "Cannot resolve symbol X"
**Solution:** Ensure you've copied all dependency interfaces and classes

### Issue 2: Circular Dependency
**Symptom:** "Circular dependency detected in service provider"
**Solution:** Check service registration order, ensure services are registered in dependency order

### Issue 3: Database Context Not Found
**Symptom:** "Unable to resolve service for type ApplicationDbContext"
**Solution:** Ensure AddDbContext is called in Infrastructure.DependencyInjection

### Issue 4: Stripe Configuration Error
**Symptom:** "Stripe API keys not configured"
**Solution:** Verify appsettings.json has correct StripeSettings

### Issue 5: Background Services Not Starting
**Symptom:** Background services don't run
**Solution:** Ensure AddHostedService is called for each background service

---

## 📝 Notes

1. **Dependency Order Matters**: Always extract and register dependencies before dependents
2. **Test Incrementally**: Extract and test phases in order
3. **Keep Original**: Don't delete original files until extraction is complete and tested
4. **Configuration Keys**: Update all configuration keys for new environment
5. **Database Migration**: Always test on a development database first
6. **Stripe Webhooks**: Configure webhook endpoint in Stripe dashboard
7. **Error Handling**: Review and update error handling for standalone deployment

---

**END OF EXTRACTION GUIDE** 🎉

**Total Time Estimate:** 3-5 days for experienced developer
**Complexity:** High
**Risk Level:** Medium-High (proper testing required)

**Remember:** Test thoroughly before production deployment!

