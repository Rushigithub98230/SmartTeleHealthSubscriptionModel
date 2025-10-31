# Subscription Plan Management Module - Complete Extraction Summary

## Overview
This document provides a comprehensive list of all components required to extract the **Subscription Plan Management** functionality from the SmartTelehealth codebase into a standalone, fully functional module.

---

## 1. ENTITIES (Database Models)

### 1.1 Core Subscription Entities
**Location:** `backend/SmartTelehealth.Core/Entities/`

- ✅ **SubscriptionPlan.cs** - Main subscription plan entity
- ✅ **SubscriptionPlanPrivilege.cs** - Junction entity linking plans to privileges
- ✅ **Subscription.cs** - User subscription entity (depends on plans)
- ✅ **SubscriptionPayment.cs** - Payment records for subscriptions
- ✅ **SubscriptionStatusHistory.cs** - Status change history tracking

### 1.2 Privilege & Usage Entities
**Location:** `backend/SmartTelehealth.Core/Entities/`

- ✅ **Privilege.cs** - Privilege definitions
- ✅ **UserSubscriptionPrivilegeUsage.cs** - User privilege usage tracking
- ✅ **PrivilegeUsageHistory.cs** - Historical privilege usage records

### 1.3 Billing & Payment Entities
**Location:** `backend/SmartTelehealth.Core/Entities/`

- ✅ **BillingRecord.cs** - Billing record entity
- ✅ **BillingAdjustment.cs** - Billing adjustments/credits
- ✅ **PaymentRefund.cs** - Payment refund records

### 1.4 Supporting/Reference Entities
**Location:** `backend/SmartTelehealth.Core/Entities/MasterTables.cs`

- ✅ **MasterBillingCycle** - Billing cycle definitions (Monthly, Quarterly, Annual)
- ✅ **MasterCurrency** - Currency definitions
- ✅ **MasterPrivilegeType** - Privilege type categorization
- ✅ **Category.cs** - Plan categorization
- ✅ **User.cs** - User entity (reference dependency)
- ✅ **ScheduledPlanMigration.cs** - Plan versioning migration tracking
- ✅ **SystemSettings.cs** - System-wide configuration (for admin commission defaults)

### 1.5 Status Master Tables
**Location:** `backend/SmartTelehealth.Core/Entities/MasterTables.cs`

- ✅ **PaymentStatus** - Payment status lookup table
- ✅ **RefundStatus** - Refund status lookup table
- ✅ **AppointmentStatus** - Appointment status (if appointments are tied to subscriptions)
- ✅ **AppointmentType** - Appointment types
- ✅ **ConsultationMode** - Consultation modes

---

## 2. DTOs (Data Transfer Objects)

### 2.1 Subscription Plan DTOs
**Location:** `backend/SmartTelehealth.Application/DTOs/`

- ✅ **CreateSubscriptionPlanDto.cs** - DTO for creating new plans
- ✅ **SubscriptionPlanDto.cs** - General plan DTO
- ✅ **UpdateSubscriptionPlanDto.cs** - DTO for updating plans (implied)
- ✅ **SubscriptionPlanTimeLimitsDto.cs** - Time-based limits configuration
- ✅ **PlanPrivilegeDto** (nested in CreateSubscriptionPlanDto.cs) - Privilege configuration
- ✅ **SchedulePlanChangeDto.cs** - Plan change scheduling

### 2.2 Filter & Query DTOs
**Location:** `backend/SmartTelehealth.Core/DTOs/`

- ✅ **SubscriptionPlanFilterDto.cs** - Plan filtering and search

### 2.3 Subscription-Related DTOs
**Location:** `backend/SmartTelehealth.Application/DTOs/`

- ✅ **SubscriptionDto.cs** (implied) - Subscription DTO
- ✅ **CreateSubscriptionDto.cs** (implied) - Create subscription DTO
- ✅ **UpdateSubscriptionDto.cs** (implied) - Update subscription DTO

### 2.4 Privilege DTOs
**Location:** `backend/SmartTelehealth.Application/DTOs/`

- ✅ **CreatePrivilegeDto.cs** (implied) - Create privilege DTO
- ✅ **UpdatePrivilegeDto.cs** (implied) - Update privilege DTO
- ✅ **PrivilegeDto.cs** (implied) - Privilege DTO
- ✅ **UpdateTimeBasedLimitsDto.cs** (implied) - Time-based limits update DTO

### 2.5 Plan Versioning DTOs
**Location:** `backend/SmartTelehealth.Application/DTOs/`

- ✅ **CreatePlanVersionRequestDto.cs** (implied) - Create new plan version
- ✅ **MigrateUsersRequestDto.cs** (implied) - User migration request

---

## 3. MAPPING PROFILES (AutoMapper)

**Location:** `backend/SmartTelehealth.Application/Mappings/` (implied location)

**Note:** Need to locate/identify AutoMapper profiles specifically:
- ✅ **SubscriptionPlanMappingProfile** (implied) - Maps SubscriptionPlan ↔ DTOs
- ✅ **SubscriptionMappingProfile** (implied) - Maps Subscription ↔ DTOs
- ✅ **PrivilegeMappingProfile** (implied) - Maps Privilege ↔ DTOs

**Configuration:** AutoMapper is registered in `backend/SmartTelehealth.Application/DependencyInjection.cs` line 12.

---

## 4. DEPENDENCY INJECTION REGISTRATIONS

### 4.1 Repositories
**Location:** `backend/SmartTelehealth.Infrastructure/DependencyInjection.cs`

Lines 62-66:
```csharp
services.AddScoped<ISubscriptionPlanRepository, SubscriptionPlanRepository>();
services.AddScoped<ISubscriptionPlanPrivilegeRepository, SubscriptionPlanPrivilegeRepository>();
services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
services.AddScoped<ISubscriptionPaymentRepository, SubscriptionPaymentRepository>();
services.AddScoped<ISubscriptionStatusHistoryRepository, SubscriptionStatusHistoryRepository>();
services.AddScoped<IUserSubscriptionPrivilegeUsageRepository, UserSubscriptionPrivilegeUsageRepository>();
services.AddScoped<IPrivilegeUsageHistoryRepository, PrivilegeUsageHistoryRepository>();
services.AddScoped<IPrivilegeRepository, PrivilegeRepository>();
services.AddScoped<IScheduledPlanMigrationRepository, ScheduledPlanMigrationRepository>();
services.AddScoped<IBillingRepository, BillingRepository>();
services.AddScoped<IBillingAdjustmentRepository, BillingAdjustmentRepository>();
```

### 4.2 Supporting Repositories
**Location:** `backend/SmartTelehealth.Infrastructure/DependencyInjection.cs`

```csharp
services.AddScoped<ICategoryRepository, CategoryRepository>();
services.AddScoped<ISystemSettingsRepository, SystemSettingsRepository>();
services.AddScoped<IMasterDataRepository, MasterDataRepository>(); // If exists for MasterBillingCycle, MasterCurrency, etc.
```

### 4.3 Services
**Location:** `backend/SmartTelehealth.Application/DependencyInjection.cs`

Lines 197-206:
```csharp
services.AddScoped<ISubscriptionPlanService, SubscriptionPlanService>();
services.AddScoped<ISubscriptionService, SubscriptionService>();
services.AddScoped<IPlanPricingService, PlanPricingService>();
services.AddScoped<IPlanVersioningService, PlanVersioningService>();
services.AddScoped<ISubscriptionLifecycleService, SubscriptionLifecycleService>();
services.AddScoped<ISubscriptionBillingService, SubscriptionBillingService>();
services.AddScoped<ISubscriptionNotificationService, SubscriptionNotificationService>();
services.AddScoped<IPrivilegeService, PrivilegeService>();
services.AddScoped<IStripeService, StripeService>();
services.AddScoped<IStripeSynchronizationService, StripeSynchronizationService>();
services.AddScoped<ICategoryService, CategoryService>();
services.AddScoped<IUserService, UserService>();
```

---

## 5. SERVICES (Business Logic)

### 5.1 Core Subscription Plan Services
**Location:** `backend/SmartTelehealth.Application/Services/`

- ✅ **SubscriptionPlanService.cs** - Core plan CRUD, activation, privilege management
- ✅ **PlanPricingService.cs** - Healthcare pricing calculations, privilege-based pricing
- ✅ **PlanVersioningService.cs** - Plan versioning, migrations, grandfathered user management

### 5.2 Subscription Lifecycle Services
**Location:** `backend/SmartTelehealth.Application/Services/`

- ✅ **SubscriptionService.cs** - User subscription CRUD, lifecycle management
- ✅ **SubscriptionLifecycleService.cs** - Status transitions, renewals, cancellations
- ✅ **SubscriptionBillingService.cs** - Billing operations, payment processing
- ✅ **SubscriptionNotificationService.cs** - User notifications for subscription events

### 5.3 Supporting Services
**Location:** `backend/SmartTelehealth.Application/Services/`

- ✅ **PrivilegeService.cs** - Privilege CRUD, usage tracking, history management
- ✅ **CategoryService.cs** - Category management (plan categorization)
- ✅ **StripeService.cs** - Stripe payment integration
- ✅ **StripeSynchronizationService.cs** - Sync local DB with Stripe
- ✅ **StripeBillingService.cs** - Stripe billing operations
- ✅ **UserService.cs** (reference dependency) - User management
- ✅ **NotificationService.cs** - Email/SMS notifications

### 5.4 Background Services
**Location:** `backend/SmartTelehealth.Infrastructure/Services/`

- ✅ **AutomatedBillingBackgroundService.cs** - Automated billing runs
- ✅ **PrivilegeResetBackgroundService.cs** (implied) - Periodic privilege usage resets

---

## 6. REPOSITORIES (Data Access Layer)

### 6.1 Core Repositories
**Location:** `backend/SmartTelehealth.Infrastructure/Repositories/`

- ✅ **SubscriptionPlanRepository.cs**
- ✅ **SubscriptionPlanPrivilegeRepository.cs**
- ✅ **SubscriptionRepository.cs**
- ✅ **SubscriptionPaymentRepository.cs**
- ✅ **SubscriptionStatusHistoryRepository.cs**
- ✅ **PrivilegeRepository.cs**
- ✅ **PrivilegeUsageHistoryRepository.cs**
- ✅ **UserSubscriptionPrivilegeUsageRepository.cs**
- ✅ **BillingAdjustmentRepository.cs**
- ✅ **BillingRepository.cs**
- ✅ **PaymentRefundRepository.cs** (implied)
- ✅ **ScheduledPlanMigrationRepository.cs**

### 6.2 Supporting Repositories
**Location:** `backend/SmartTelehealth.Infrastructure/Repositories/`

- ✅ **CategoryRepository.cs**
- ✅ **SystemSettingsRepository.cs**
- ✅ **GenericRepository.cs** (base repository)
- ✅ **RepositoryBase.cs** (base implementation)

---

## 7. CONTROLLERS (API Endpoints)

### 7.1 Primary Controllers
**Location:** `backend/SmartTelehealth.API/Controllers/`

- ✅ **SubscriptionPlansController.cs** - Main plan management API (comprehensive: create, read, update, delete, activate, deactivate, privilege management, versioning, pricing)
- ✅ **SubscriptionsController.cs** - User subscription management API
- ✅ **BillingController.cs** - Billing operations API
- ✅ **StripeController.cs** - Stripe integration endpoints
- ✅ **StripeWebhookController.cs** - Stripe webhook handlers

### 7.2 Supporting Controllers
**Location:** `backend/SmartTelehealth.API/Controllers/`

- ✅ **BaseController.cs** - Base controller with common functionality
- ✅ **PrivilegesController.cs** (implied) - Or privilege endpoints in SubscriptionPlansController
- ✅ **CategoriesController.cs** - Category management (if used for plans)

---

## 8. ASSOCIATED SERVICES & REPOSITORIES

### 8.1 Dependent Modules Required
- ✅ **Billing Module** - BillingRecord, BillingAdjustment entities & services
- ✅ **Payment Module** - PaymentRefund, PaymentStatus, RefundStatus entities & services
- ✅ **Privilege Module** - Privilege, UserSubscriptionPrivilegeUsage, PrivilegeUsageHistory entities & services
- ✅ **Category Module** - Category entity & CategoryService
- ✅ **Master Data Module** - MasterBillingCycle, MasterCurrency, MasterPrivilegeType entities
- ✅ **User Module** - User entity & UserService (reference dependency)
- ✅ **Stripe Integration** - StripeService, StripeBillingService, StripeSynchronizationService

### 8.2 Database Context Configuration
**Location:** `backend/SmartTelehealth.Infrastructure/Data/ApplicationDbContext.cs`

**Required DbSet declarations:**
- ✅ `DbSet<SubscriptionPlan>`
- ✅ `DbSet<SubscriptionPlanPrivilege>`
- ✅ `DbSet<Subscription>`
- ✅ `DbSet<SubscriptionPayment>`
- ✅ `DbSet<SubscriptionStatusHistory>`
- ✅ `DbSet<Privilege>`
- ✅ `DbSet<UserSubscriptionPrivilegeUsage>`
- ✅ `DbSet<PrivilegeUsageHistory>`
- ✅ `DbSet<BillingRecord>`
- ✅ `DbSet<BillingAdjustment>`
- ✅ `DbSet<PaymentRefund>`
- ✅ `DbSet<Category>`
- ✅ `DbSet<ScheduledPlanMigration>`
- ✅ `DbSet<MasterBillingCycle>`
- ✅ `DbSet<MasterCurrency>`
- ✅ `DbSet<MasterPrivilegeType>`
- ✅ `DbSet<SystemSettings>`

**Required relationship configurations in `OnModelCreating`:**
- ✅ SubscriptionPlan ↔ SubscriptionPlanPrivilege (One-to-Many)
- ✅ SubscriptionPlan ↔ Subscription (One-to-Many)
- ✅ SubscriptionPlan ↔ Category (Many-to-One)
- ✅ SubscriptionPlan ↔ Parent/Child versions (Self-referencing)
- ✅ Privilege ↔ SubscriptionPlanPrivilege (One-to-Many)
- ✅ Subscription ↔ UserSubscriptionPrivilegeUsage (One-to-Many)
- ✅ Subscription ↔ SubscriptionPayment (One-to-Many)
- ✅ Subscription ↔ BillingRecord (One-to-Many)
- ✅ Subscription ↔ SubscriptionStatusHistory (One-to-Many)
- ✅ SubscriptionPayment ↔ PaymentRefund (One-to-Many)
- ✅ UserSubscriptionPrivilegeUsage ↔ PrivilegeUsageHistory (One-to-Many)

---

## 9. MIGRATIONS (Database Schema)

### 9.1 Required Migration Files
**Location:** `backend/SmartTelehealth.Infrastructure/Migrations/`

**Main Migration:**
- ✅ **20251018154706_InitialCreate.cs** - Contains all table definitions

**Additional Migrations:**
- ✅ **20251020193012_SubscriptionPlanArchitectureFix.cs** - Architecture updates
- ✅ **20251028195304_AddScheduledPlanChangeFields.cs** - Plan migration fields
- ✅ **20251029062431_AddTaxFieldsToSubscriptionPlan.cs** - Tax fields
- ✅ **20251021215516_SyncDatabaseSchema_AddStripePriceIdAndFailedRefunds.cs** - Stripe & refunds

**SQL DDL Script:**
- ✅ **Subscription_Management_CreateTables.sql** - Standalone table creation script

---

## 10. INTERFACES & CONTRACTS

### 10.1 Repository Interfaces
**Location:** `backend/SmartTelehealth.Core/Interfaces/`

- ✅ `ISubscriptionPlanRepository`
- ✅ `ISubscriptionPlanPrivilegeRepository`
- ✅ `ISubscriptionRepository`
- ✅ `ISubscriptionPaymentRepository`
- ✅ `ISubscriptionStatusHistoryRepository`
- ✅ `IPrivilegeRepository`
- ✅ `IPrivilegeUsageHistoryRepository`
- ✅ `IUserSubscriptionPrivilegeUsageRepository`
- ✅ `IBillingRepository`
- ✅ `IBillingAdjustmentRepository`
- ✅ `IScheduledPlanMigrationRepository`
- ✅ `ISystemSettingsRepository`
- ✅ `ICategoryRepository`

### 10.2 Service Interfaces
**Location:** `backend/SmartTelehealth.Application/Interfaces/`

- ✅ `ISubscriptionPlanService`
- ✅ `ISubscriptionService`
- ✅ `IPlanPricingService`
- ✅ `IPlanVersioningService`
- ✅ `ISubscriptionLifecycleService`
- ✅ `ISubscriptionBillingService`
- ✅ `ISubscriptionNotificationService`
- ✅ `IPrivilegeService`
- ✅ `IStripeService`
- ✅ `IStripeSynchronizationService`
- ✅ `ICategoryService`
- ✅ `IUserService`
- ✅ `INotificationService`

---

## 11. ENUMS & CONSTANTS

### 11.1 Enums
**Location:** `backend/SmartTelehealth.Core/Enums/`

- ✅ **PlanType.cs** - Subscription plan types (Standard, UsageBased, Premium, Enterprise)
- ✅ **SubscriptionStatus.cs** (implied) - Subscription status enumeration
- ✅ **PaymentStatus.cs** (implied) - Payment status values

### 11.2 Constants
**Location:** `backend/SmartTelehealth.Core/Entities/Subscription.cs`

- ✅ **SubscriptionStatuses static class** - String constants for subscription statuses

---

## 12. UTILITIES & HELPERS

### 12.1 Utilities
**Location:** `backend/SmartTelehealth.Application/Utilities/`

- ✅ **PrivilegeAllocationCalculator.cs** (implied) - Privilege allocation calculations
- ✅ **BillingCalculationService.cs** (implied) - Billing price calculations
- ✅ Any validation helpers specific to subscriptions

### 12.2 Configuration
**Location:** `backend/SmartTelehealth.Infrastructure/Configuration/`

- ✅ **StripeSettings.cs** - Stripe API configuration
- ✅ **System configuration** for admin commission defaults

---

## 13. BACKGROUND SERVICES & HOSTED SERVICES

**Location:** `backend/SmartTelehealth.Infrastructure/Services/`

- ✅ **AutomatedBillingBackgroundService.cs** - Periodic billing runs
- ✅ **PrivilegeResetBackgroundService.cs** (implied) - Privilege usage resets
- ✅ Registration in `DependencyInjection.cs` line 135

---

## 14. BASE ENTITIES

**Location:** `backend/SmartTelehealth.Core/Entities/BaseEntity.cs`

- ✅ **BaseEntity** - Common fields (Id, IsActive, IsDeleted, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate)

---

## 15. DATABASE SEED DATA

**Location:** `backend/SmartTelehealth.Infrastructure/Data/SeedData.cs`

- ✅ Master data seeding (MasterBillingCycle, MasterCurrency, MasterPrivilegeType, PaymentStatus, RefundStatus)
- ✅ Initial subscription plan data (optional)

---

## 16. TESTING REQUIREMENTS

### 16.1 Unit Tests
- ✅ SubscriptionPlanService tests
- ✅ PlanPricingService tests
- ✅ PlanVersioningService tests
- ✅ SubscriptionService tests
- ✅ PrivilegeService tests

### 16.2 Integration Tests
- ✅ End-to-end subscription plan creation flow
- ✅ Pricing calculation accuracy tests
- ✅ Versioning and migration tests
- ✅ Stripe integration tests

---

## EXTRACTION CHECKLIST

### Phase 1: Core Entities & Database
- [ ] Extract all entity files listed in Section 1
- [ ] Extract Migration files from Section 9
- [ ] Configure DbContext with all required DbSets
- [ ] Set up all entity relationships in OnModelCreating
- [ ] Run migrations on target database

### Phase 2: Data Access Layer
- [ ] Extract all Repository implementations
- [ ] Extract all Repository interfaces
- [ ] Extract GenericRepository and RepositoryBase
- [ ] Register all repositories in DI container

### Phase 3: DTOs & Mapping
- [ ] Extract all DTO files
- [ ] Create/identify AutoMapper profiles
- [ ] Configure mapping relationships
- [ ] Register AutoMapper in DI

### Phase 4: Business Logic
- [ ] Extract all Service implementations
- [ ] Extract all Service interfaces
- [ ] Register all services in DI
- [ ] Handle service dependencies

### Phase 5: API Layer
- [ ] Extract SubscriptionPlansController
- [ ] Extract SubscriptionsController
- [ ] Extract supporting controllers
- [ ] Configure API routing
- [ ] Add authentication/authorization

### Phase 6: Supporting Infrastructure
- [ ] Extract Stripe integration services
- [ ] Configure Stripe settings
- [ ] Set up notification services
- [ ] Configure background services
- [ ] Set up logging and error handling

### Phase 7: Testing & Validation
- [ ] Create unit tests for services
- [ ] Create integration tests
- [ ] Test Stripe webhook handling
- [ ] Validate pricing calculations
- [ ] Test versioning functionality

### Phase 8: Documentation
- [ ] API endpoint documentation
- [ ] Service documentation
- [ ] Entity relationship diagrams
- [ ] Deployment guide
- [ ] Configuration guide

---

## CRITICAL DEPENDENCIES

### External Services
- **Stripe Account** - Payment processing integration
- **Email Service** - User notifications (Twilio/Mailgun)
- **SMS Service** - SMS notifications (Twilio)

### Database Requirements
- SQL Server (or compatible)
- Proper indexing on foreign keys
- Transaction support for critical operations

### Configuration Requirements
- Stripe API keys (public & secret)
- System-wide admin commission percentage
- Email/SMS provider credentials
- Database connection string

---

## NOTES & CONSIDERATIONS

1. **User Module Dependency**: The subscription plan management requires the User entity. Consider if you need to extract User module as well or create a minimal user stub.

2. **Stripe Integration**: Fully functional module requires active Stripe account and proper webhook configuration.

3. **Migration Complexity**: Plan versioning feature requires careful handling of existing subscriptions during version changes.

4. **Pricing Calculations**: The healthcare-specific pricing model (privilege-based) is complex and requires thorough testing.

5. **Background Services**: Automated billing and privilege resets must run as background services.

6. **Audit Trail**: All plan changes, versioning, and migrations maintain comprehensive audit trails.

---

## FILE COUNT SUMMARY

- **Entities:** ~15 files
- **DTOs:** ~12 files
- **Services:** ~15 files
- **Repositories:** ~12 files
- **Controllers:** ~5 files
- **Interfaces:** ~20 files
- **Migrations:** ~5 files
- **Configuration:** ~3 files
- **Utilities:** ~3 files

**Total:** ~90+ files to extract

---

**END OF EXTRACTION SUMMARY**
