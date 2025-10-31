# Subscription Management Module Extraction Checklist

## 📋 Quick Reference Checklist

Use this checklist as you extract the module. Mark items as you complete them.

---

## 🔴 PHASE 1: Foundation (Critical Path)

### Core Foundation Files
- [ ] **BaseEntity.cs** → `SmartTelehealth.Core/Entities/`
- [ ] **PlanType.cs** (enum) → `SmartTelehealth.Core/Enums/`
- [ ] **JsonModel.cs** → `SmartTelehealth.Core/DTOs/`
- [ ] **TokenModel.cs** → `SmartTelehealth.Core/DTOs/`

---

## 🟡 PHASE 2: Entities

### Master Data Entities (5 files)
- [ ] **MasterBillingCycle** (from MasterTables.cs)
- [ ] **MasterCurrency** (from MasterTables.cs)
- [ ] **MasterPrivilegeType** (from MasterTables.cs)
- [ ] **PaymentStatus** (from MasterTables.cs)
- [ ] **RefundStatus** (from MasterTables.cs)

### Supporting Entities (3 files)
- [ ] **Category.cs**
- [ ] **SystemSettings.cs**
- [ ] **User.cs** (or stub)

### Privilege Entities (3 files)
- [ ] **Privilege.cs**
- [ ] **UserSubscriptionPrivilegeUsage.cs**
- [ ] **PrivilegeUsageHistory.cs**

### Plan Entities (2 files)
- [ ] **SubscriptionPlan.cs**
- [ ] **SubscriptionPlanPrivilege.cs**

### Subscription Entities (3 files)
- [ ] **Subscription.cs**
- [ ] **SubscriptionStatusHistory.cs**
- [ ] **SubscriptionPayment.cs**

### Billing Entities (4 files)
- [ ] **BillingRecord.cs**
- [ ] **BillingAdjustment.cs**
- [ ] **PaymentRefund.cs**
- [ ] **FailedRefund.cs**

### Versioning Entity (1 file)
- [ ] **ScheduledPlanMigration.cs**

### Webhook Entities (3 files)
- [ ] **ProcessedWebhookEvent.cs**
- [ ] **UnprocessedWebhookEvent.cs**
- [ ] **StripeSyncHistory.cs**

**Total Entities: 24 files**

---

## 🟡 PHASE 3: Master Data Scripts

- [ ] **seed_master_data.sql** (create from Phase 3.1)
- [ ] **seed_sample_privileges.sql** (optional)
- [ ] **seed_sample_categories.sql** (optional)

---

## 🟡 PHASE 4: Interfaces

### Base Interfaces (3 files)
- [ ] **IGenericRepository.cs**
- [ ] **IRepositoryBase.cs**
- [ ] **IUnitOfWork.cs**

### Subscription Repository Interfaces (5 files)
- [ ] **ISubscriptionPlanRepository.cs**
- [ ] **ISubscriptionPlanPrivilegeRepository.cs**
- [ ] **ISubscriptionRepository.cs**
- [ ] **ISubscriptionPaymentRepository.cs**
- [ ] **ISubscriptionStatusHistoryRepository.cs**

### Privilege Repository Interfaces (3 files)
- [ ] **IPrivilegeRepository.cs**
- [ ] **IUserSubscriptionPrivilegeUsageRepository.cs**
- [ ] **IPrivilegeUsageHistoryRepository.cs**

### Billing Repository Interfaces (3 files)
- [ ] **IBillingRepository.cs**
- [ ] **IBillingAdjustmentRepository.cs**
- [ ] **IFailedRefundRepository.cs**

### Supporting Repository Interfaces (6 files)
- [ ] **IScheduledPlanMigrationRepository.cs**
- [ ] **ICategoryRepository.cs**
- [ ] **ISystemSettingsRepository.cs**
- [ ] **IProcessedWebhookEventRepository.cs**
- [ ] **IUnprocessedWebhookEventRepository.cs**
- [ ] **IUserRepository.cs**

### Service Interfaces (18 files)
- [ ] **ISubscriptionPlanService.cs**
- [ ] **ISubscriptionService.cs**
- [ ] **ISubscriptionLifecycleService.cs**
- [ ] **ISubscriptionBillingService.cs**
- [ ] **ISubscriptionNotificationService.cs**
- [ ] **IPrivilegeService.cs**
- [ ] **IPlanPricingService.cs**
- [ ] **IPlanVersioningService.cs**
- [ ] **IStripeService.cs**
- [ ] **IStripeBillingService.cs**
- [ ] **IStripeSynchronizationService.cs**
- [ ] **IPaymentService.cs**
- [ ] **IAutomatedBillingService.cs**
- [ ] **ICategoryService.cs**
- [ ] **INotificationService.cs**
- [ ] **IWebhookService.cs**
- [ ] **IWebhookIdempotencyService.cs**
- [ ] **IUserService.cs**

**Total Interfaces: 38 files**

---

## 🟡 PHASE 5: Utilities & Constants

- [ ] **SubscriptionConstants.cs**
- [ ] **BillingCalculationService.cs**
- [ ] **BillingCycleCalculator.cs**
- [ ] **PrivilegeAllocationCalculator.cs**
- [ ] **PrivilegeResetHelper.cs**
- [ ] **BillingValidationService.cs**
- [ ] **CurrencyService.cs**

**Total Utilities: 7 files**

---

## 🟡 PHASE 6: DTOs

### Core DTOs (Already in Phase 1)
- [x] JsonModel
- [x] TokenModel

### Plan DTOs (4 files)
- [ ] **SubscriptionPlanDto.cs**
- [ ] **CreateSubscriptionPlanDto.cs**
- [ ] **UpdateSubscriptionPlanDto.cs**
- [ ] **SubscriptionPlanFilterDto.cs**

### Subscription DTOs (3 files)
- [ ] **SubscriptionDto.cs**
- [ ] **CreateSubscriptionDto.cs**
- [ ] **UpdateSubscriptionDto.cs**

### Privilege DTOs (6 files)
- [ ] **PrivilegeDto.cs**
- [ ] **CreatePrivilegeDto.cs**
- [ ] **UpdatePrivilegeDto.cs**
- [ ] **PrivilegeUsageDto.cs**
- [ ] **TrackUsageDto.cs**
- [ ] **UserPrivilegeUsageDto.cs**

### Billing DTOs (10 files)
- [ ] **BillingRecordDto.cs**
- [ ] **BillingDto.cs**
- [ ] **CreateBillingRecordDto.cs**
- [ ] **UpdateBillingRecordDto.cs**
- [ ] **BillingAdjustmentDto.cs**
- [ ] **CreateBillingAdjustmentDto.cs**
- [ ] **PaymentRequestDto.cs**
- [ ] **PaymentResultDto.cs**
- [ ] **ProcessPrivilegeUsageDto.cs**
- [ ] **PrivilegeBasedBillingDto.cs**

### Versioning DTOs (4 files)
- [ ] **CreatePlanVersionRequestDto.cs**
- [ ] **MigrateUsersRequestDto.cs**
- [ ] **SchedulePlanChangeDto.cs**
- [ ] **PlanVersionDto.cs**

### Support DTOs (7 files)
- [ ] **CategoryDto.cs**
- [ ] **CancelSubscriptionDto.cs**
- [ ] **ChangePlanRequest.cs**
- [ ] **SubscriptionAutomationDtos.cs**
- [ ] **SubscriptionDashboardDto.cs**
- [ ] **UsageStatisticsDto.cs**
- [ ] **PricingBreakdownDto.cs**

**Total DTOs: 34 files**

---

## 🟡 PHASE 7: Mapping

- [ ] **SubscriptionMappingProfile.cs** (extract subscription mappings from MappingProfile.cs)

---

## 🟡 PHASE 8: Repositories

### Base Repositories (3 files)
- [ ] **GenericRepository.cs**
- [ ] **RepositoryBase.cs**
- [ ] **UnitOfWork.cs**

### Subscription Repositories (5 files)
- [ ] **SubscriptionPlanRepository.cs**
- [ ] **SubscriptionPlanPrivilegeRepository.cs**
- [ ] **SubscriptionRepository.cs**
- [ ] **SubscriptionPaymentRepository.cs**
- [ ] **SubscriptionStatusHistoryRepository.cs**

### Privilege Repositories (3 files)
- [ ] **PrivilegeRepository.cs**
- [ ] **UserSubscriptionPrivilegeUsageRepository.cs**
- [ ] **PrivilegeUsageHistoryRepository.cs**

### Billing Repositories (3 files)
- [ ] **BillingRepository.cs**
- [ ] **BillingAdjustmentRepository.cs**
- [ ] **FailedRefundRepository.cs**

### Supporting Repositories (6 files)
- [ ] **ScheduledPlanMigrationRepository.cs**
- [ ] **CategoryRepository.cs**
- [ ] **SystemSettingsRepository.cs**
- [ ] **ProcessedWebhookEventRepository.cs**
- [ ] **UnprocessedWebhookEventRepository.cs**
- [ ] **UserRepository.cs**

**Total Repositories: 20 files**

---

## 🟡 PHASE 9: Services

### Core Services (4 files)
- [ ] **PlanPricingService.cs** + interface
- [ ] **StripeSynchronizationService.cs** + interface
- [ ] **PrivilegeService.cs** + interface
- [ ] **PaymentService.cs** + interface

### Subscription Services (3 files)
- [ ] **SubscriptionPlanService.cs** + interface
- [ ] **SubscriptionService.cs** + interface
- [ ] **SubscriptionLifecycleService.cs** + interface

### Billing Services (3 files)
- [ ] **SubscriptionBillingService.cs** + interface
- [ ] **PlanVersioningService.cs** + interface
- [ ] **AutomatedBillingService.cs** + interface

### Stripe Services (2 files)
- [ ] **StripeService.cs** + interface
- [ ] **StripeBillingService.cs** + interface

### Supporting Services (7 files)
- [ ] **CategoryService.cs** + interface
- [ ] **SubscriptionNotificationService.cs** + interface
- [ ] **NotificationService.cs** + interface
- [ ] **WebhookService.cs** + interface
- [ ] **WebhookIdempotencyService.cs** + interface
- [ ] **UserService.cs** + interface (or stub)
- [ ] **ReconciliationService.cs** + interface

**Total Services: 19 files**

---

## 🟡 PHASE 10: Background Services

- [ ] **AutomatedBillingBackgroundService.cs**
- [ ] **PrivilegeResetBackgroundService.cs**
- [ ] **ScheduledMigrationBackgroundService.cs**
- [ ] **FailedRefundRetryBackgroundService.cs**
- [ ] **UnprocessedWebhookRetryService.cs**
- [ ] **StripeSyncJob.cs**
- [ ] **ReconciliationBackgroundService.cs**

**Total Background Services: 7 files**

---

## 🟡 PHASE 11: Controllers

- [ ] **BaseController.cs**
- [ ] **SubscriptionPlansController.cs**
- [ ] **SubscriptionsController.cs**
- [ ] **BillingController.cs**
- [ ] **StripeController.cs**
- [ ] **StripeWebhookController.cs**

**Total Controllers: 6 files**

---

## 🔴 PHASE 12: Dependency Injection (CRITICAL)

- [ ] **Application.DependencyInjection.cs** - All service registrations
- [ ] **Infrastructure.DependencyInjection.cs** - All repository and background service registrations

---

## 🔴 PHASE 13: Database Configuration (CRITICAL)

- [ ] **ApplicationDbContext.cs** - All DbSets and relationships
- [ ] **Subscription_Management_CreateTables.sql** - Table creation script
- [ ] Run EF Core migrations OR execute SQL script
- [ ] Run **seed_master_data.sql**
- [ ] [OPTIONAL] Run **seed_sample_privileges.sql**
- [ ] [OPTIONAL] Run **seed_sample_categories.sql**

---

## 🔴 PHASE 14: Testing & Configuration

### NuGet Packages
- [ ] Microsoft.EntityFrameworkCore
- [ ] Microsoft.EntityFrameworkCore.SqlServer
- [ ] AutoMapper.Extensions.Microsoft.DependencyInjection
- [ ] Stripe.net
- [ ] Swashbuckle.AspNetCore
- [ ] Serilog.AspNetCore

### Configuration Files
- [ ] **appsettings.json** - All settings configured
- [ ] **Program.cs / Startup.cs** - All services registered

### Build & Test
- [ ] `dotnet restore` - No errors
- [ ] `dotnet build` - No compilation errors
- [ ] `dotnet ef database update` - Database created
- [ ] Application starts without errors
- [ ] All background services start
- [ ] API endpoints accessible (Swagger)

### Critical Flow Tests
- [ ] Create subscription plan ✅
- [ ] Create subscription ✅
- [ ] Use privilege ✅
- [ ] Automated billing runs ✅
- [ ] Stripe webhook processes ✅

---

## 📊 Extraction Summary

### File Count by Category
- **Entities**: 24 files
- **Interfaces**: 38 files
- **Utilities**: 7 files
- **DTOs**: 34 files
- **Mapping**: 1 file
- **Repositories**: 20 files
- **Services**: 19 files
- **Background Services**: 7 files
- **Controllers**: 6 files
- **Configuration**: 3 files
- **Database Scripts**: 4 files

**GRAND TOTAL: ~163 files**

---

## ✅ Final Validation

### Code Quality
- [ ] No compilation errors
- [ ] No missing dependencies
- [ ] All services registered correctly
- [ ] All repositories registered correctly

### Database
- [ ] All tables created
- [ ] All foreign keys working
- [ ] All indexes created
- [ ] Seed data loaded

### API
- [ ] Swagger accessible
- [ ] Authentication working (if enabled)
- [ ] All endpoints responding
- [ ] Webhook endpoint configured

### Background Services
- [ ] AutomatedBillingBackgroundService running
- [ ] PrivilegeResetBackgroundService running
- [ ] All other background services running

### Integration
- [ ] Stripe integration working
- [ ] Webhook processing working
- [ ] Email/SMS notifications working (if enabled)
- [ ] Error handling working

---

## 🎉 Extraction Complete!

**Date Completed:** _______________
**Extracted By:** _______________
**Verified By:** _______________

**Notes:**
_________________________________________________________________
_________________________________________________________________
_________________________________________________________________


