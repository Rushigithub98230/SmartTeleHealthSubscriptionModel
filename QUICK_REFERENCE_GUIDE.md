# Subscription Management System - Quick Reference Guide

## 🚀 Quick Start

### Key Concepts
- **SubscriptionPlan**: Template defining features and pricing
- **Subscription**: User's active subscription instance
- **Privilege**: Service/feature (e.g., TeleConsultation, Messaging)
- **UserSubscriptionPrivilegeUsage**: Tracks how user uses privileges
- **BillingRecord**: Master billing/payment record
- **SubscriptionPayment**: Links payment to specific subscription

---

## 📁 Project Structure

```
backend/
├── SmartTelehealth.Core/
│   ├── Entities/          # Domain models
│   │   ├── SubscriptionPlan.cs
│   │   ├── Subscription.cs
│   │   ├── Privilege.cs
│   │   ├── SubscriptionPlanPrivilege.cs
│   │   ├── UserSubscriptionPrivilegeUsage.cs
│   │   ├── BillingRecord.cs
│   │   ├── SubscriptionPayment.cs
│   │   └── ScheduledPlanMigration.cs
│   ├── Enums/
│   │   └── PlanType.cs
│   └── Interfaces/        # Repository interfaces
│
├── SmartTelehealth.Application/
│   ├── Services/          # Business logic
│   │   ├── SubscriptionPlanService.cs
│   │   ├── SubscriptionService.cs
│   │   ├── SubscriptionLifecycleService.cs
│   │   ├── SubscriptionBillingService.cs
│   │   ├── PrivilegeService.cs
│   │   ├── PlanVersioningService.cs
│   │   └── PlanPricingService.cs
│   ├── DTOs/             # Data transfer objects
│   └── Interfaces/       # Service interfaces
│
├── SmartTelehealth.Infrastructure/
│   ├── Repositories/     # Data access
│   ├── Services/         # External integrations
│   │   ├── StripeService.cs
│   │   ├── AutomatedBillingBackgroundService.cs
│   │   ├── PrivilegeResetBackgroundService.cs
│   │   └── ScheduledMigrationBackgroundService.cs
│   └── Configuration/
│
└── SmartTelehealth.API/
    └── Controllers/      # API endpoints
        ├── SubscriptionPlansController.cs
        ├── SubscriptionsController.cs
        ├── BillingController.cs
        └── StripeWebhookController.cs
```

---

## 🔑 Key Entities

### SubscriptionPlan
```csharp
- Id: Guid
- Name: string                           // "Basic - Monthly"
- PlanType: PlanType                     // Standard, UsageBased, Premium, Enterprise
- BasePrice: decimal                     // Calculated from privileges + commission
- DiscountPercentage: decimal?           // Promotional discount
- BillingDiscountPercentage: decimal?    // Billing cycle discount
- IsAutoCalculatedPrice: bool            // true = auto-calculate, false = manual
- VersionNumber: int                     // Plan version (e.g., 1, 2, 3)
- IsLatestVersion: bool                  // true = available for new subscriptions
- ParentPlanId: Guid?                    // For versioning
- BillingCycleId: Guid                   // ONE billing cycle per plan
- StripePriceId: string                  // Stripe price ID
- PlanPrivileges: ICollection            // Privileges in this plan
```

### Subscription
```csharp
- Id: Guid
- UserId: int
- SubscriptionPlanId: Guid
- Status: string                         // Pending, Active, Paused, Cancelled, etc.
- StartDate: DateTime
- NextBillingDate: DateTime
- CurrentPrice: decimal
- AutoRenew: bool
- IsTrialSubscription: bool
- TrialEndDate: DateTime?
- StripeSubscriptionId: string
- StripeCustomerId: string
- PendingPlanChangeId: Guid?             // Scheduled plan change
- PlanChangeEffectiveDate: DateTime?
```

### SubscriptionPlanPrivilege (Junction)
```csharp
- SubscriptionPlanId: Guid
- PrivilegeId: Guid
- Value: int                             // -1 = unlimited, 0 = disabled, >0 = limited
- PrivilegeBaseCost: decimal             // BASE: contributes to plan price
- UnitCost: decimal                      // OVERAGE: charged when exceeded
```

### UserSubscriptionPrivilegeUsage
```csharp
- SubscriptionId: Guid
- SubscriptionPlanPrivilegeId: Guid
- PrivilegeId: Guid
- UsedValue: int                         // How many used
- AllowedValue: int                      // -1 = unlimited, >0 = limit
- UsagePeriodStart: DateTime
- UsagePeriodEnd: DateTime
```

---

## 🎯 Core Workflows

### 1. Create Subscription
```
User → Stripe Checkout → Webhook → SubscriptionLifecycleService
                                      ↓
                                    Create Subscription
                                    Create BillingRecord
                                    Create SubscriptionPayment
                                    Initialize Privileges
                                    Send Welcome Email
```

### 2. Automated Billing (Hourly)
```
AutomatedBillingBackgroundService
  ↓
Process Due Subscriptions
  ↓
Create BillingRecord
  ↓
Process Payment via Stripe
  ↓
Create SubscriptionPayment
  ↓
Update NextBillingDate
  ↓
Reset Privilege Counters
  ↓
Send Renewal Confirmation
```

### 3. Use Privilege
```
User Request → PrivilegeService.UsePrivilegeAsync()
  ↓
Check Remaining Usage
  ↓
Update UserSubscriptionPrivilegeUsage
  ↓
Log to PrivilegeUsageHistory
  ↓
If Over Limit → Charge Overage
```

### 4. Plan Versioning
```
Admin Updates Plan → PlanVersioningService.CreateNewPlanVersion()
  ↓
Copy Old Plan → Create New Version
  ↓
Mark Old as Not Latest
  ↓
Schedule Migrations for Active Subscribers
  ↓
At User's Next Renewal → Migrate to New Version
```

---

## 🧮 Pricing Formula

### Plan Base Price
```
For each Privilege in Plan:
  Contribution = Value × PrivilegeBaseCost

PrivilegesTotalCost = Σ(Contributions)

AdminCommission = PrivilegesTotalCost × (CommissionPercent / 100)

BasePrice = PrivilegesTotalCost + AdminCommission
```

### Effective Price
```
EffectivePrice = BasePrice

// Apply Promotional Discount
if (DiscountPercentage > 0 AND still valid):
    EffectivePrice = EffectivePrice × (1 - DiscountPercentage / 100)

// Apply Billing Cycle Discount
if (BillingDiscountPercentage > 0):
    EffectivePrice = EffectivePrice × (1 - BillingDiscountPercentage / 100)

Final = max(EffectivePrice, 0)
```

### Overage Charge
```
OverageCharge = (UsedValue - AllowedValue) × UnitCost

// Uses latest plan version pricing to prevent abuse
```

---

## 🔌 Key Services

### SubscriptionPlanService
- `GetSubscriptionPlansWithFilteringAsync()` - Get plans with filters
- `GetPlanByIdAsync()` - Get plan details
- `CreateSubscriptionPlanAsync()` - Create plan
- `UpdateSubscriptionPlanAsync()` - Update plan
- `ActivatePlanAsync()` / `DeactivatePlanAsync()` - Change plan status
- `GetPlansForComparisonAsync()` - Compare billing cycles

### SubscriptionLifecycleService
- `CreateSubscriptionAsync()` - Create subscription (admin only)
- `SyncSubscriptionFromCheckoutAsync()` - Sync from Stripe Checkout
- `CancelSubscriptionAsync()` - Cancel subscription
- `PauseSubscriptionAsync()` / `ResumeSubscriptionAsync()` - Pause/Resume
- `UpgradeSubscriptionAsync()` - Upgrade immediately (with proration)
- `ScheduleUpgradeAsync()` - Schedule upgrade at renewal (no proration)

### SubscriptionBillingService (51 Methods)
- `CreateSubscriptionBillingAsync()` - Create billing record
- `ProcessRecurringBillingAsync()` - Process recurring billing
- `ProcessPrivilegeUsageAsync()` - Track privilege usage
- `ProcessOverageChargesAsync()` - Charge for exceeded usage
- `GenerateInvoiceAsync()` - Generate invoice
- `GetPrivilegeUsageSummaryAsync()` - Get usage summary

### PrivilegeService
- `UsePrivilegeAsync()` - Record privilege usage
- `GetRemainingPrivilegeAsync()` - Check remaining usage
- `InitializeSubscriptionPrivilegesAsync()` - Set up privileges for new subscription
- `ResetPrivilegeUsageAsync()` - Reset usage counters

### PlanVersioningService
- `CreateNewPlanVersionAsync()` - Create new version
- `GetPlanVersionHistoryAsync()` - Get version history
- `ScheduleMigrationsForPlanVersionAsync()` - Schedule migrations
- `ProcessUserMigrationResponseAsync()` - Handle user decision

### StripeService
- `CreateCustomerAsync()` - Create Stripe customer
- `CreateSubscriptionAsync()` - Create Stripe subscription
- `CancelSubscriptionAsync()` - Cancel Stripe subscription
- `CreateCheckoutSessionAsync()` - Create checkout session
- `ProcessWebhookEventAsync()` - Handle webhook events

---

## 🎛️ API Endpoints

### Public Endpoints
```
GET  /api/subscriptionplans/active
GET  /api/subscriptionplans/{id}
GET  /api/subscriptionplans/category/{categoryId}/compare
POST /api/subscriptionplans/filter
```

### User Endpoints (Authenticated)
```
GET  /api/subscriptions/{id}
GET  /api/subscriptions/user/{userId}
POST /api/subscriptions/{id}/cancel
POST /api/subscriptions/{id}/pause
POST /api/subscriptions/{id}/resume
POST /api/subscriptions/{id}/schedule-upgrade
POST /api/Checkout/create-session/{planId}
```

### Admin Endpoints (Role: Admin)
```
POST   /api/subscriptionplans
PUT    /api/subscriptionplans/{id}
POST   /api/subscriptionplans/{id}/activate
POST   /api/subscriptionplans/{id}/privileges
POST   /api/subscriptions              // Direct creation (bypasses payment)
PUT    /api/subscriptions/{id}/upgrade // Immediate upgrade with proration
```

### Webhook Endpoint
```
POST /api/stripewebhook/webhook
```

---

## 🔄 Background Services

### AutomatedBillingBackgroundService
- **Frequency**: Every 1 hour
- **Tasks**:
  - Process subscriptions due for billing
  - Retry failed payments
  - Reset privilege counters

### PrivilegeResetBackgroundService
- **Frequency**: Periodic
- **Tasks**: Reset usage counters for new billing cycles

### ScheduledMigrationBackgroundService
- **Frequency**: Periodic
- **Tasks**: Process scheduled plan migrations

### FailedRefundRetryBackgroundService
- **Frequency**: Periodic
- **Tasks**: Retry failed refund operations

### UnprocessedWebhookRetryService
- **Frequency**: Periodic
- **Tasks**: Retry failed webhook processing

### StripeSyncJob
- **Frequency**: Every 1 hour
- **Tasks**: Sync local data with Stripe

### ReconciliationBackgroundService
- **Frequency**: Nightly
- **Tasks**: Data integrity checks and validation

---

## 📊 Key Business Rules

### Subscription Rules
1. ✅ One active subscription per user
2. ✅ Admins can create subscriptions directly
3. ✅ Regular users must use Stripe Checkout
4. ✅ Trial subscriptions auto-convert to Active
5. ✅ Cancelled subscriptions preserve historical data

### Privilege Rules
1. ✅ Unlimited (-1) never exhausts
2. ✅ Disabled (0) cannot be used
3. ✅ Limited (>0) can be exhausted
4. ✅ Overage uses latest plan pricing
5. ✅ Counters reset at billing cycle boundary

### Pricing Rules
1. ✅ Auto-calculated plans: Sum privilege costs
2. ✅ Manual plans: Use specified BasePrice
3. ✅ Discounts apply sequentially
4. ✅ Plan versions preserve existing subscriber pricing
5. ✅ New subscribers always get latest version

### Payment Rules
1. ✅ Max 3 payment retry attempts
2. ✅ Failed payments trigger status change
3. ✅ Overage charges billed separately
4. ✅ All payments have audit trail
5. ✅ Linked to BillingRecord and SubscriptionPayment

---

## 🔐 Security Model

### Authentication
- JWT token required (except public plan browsing)
- Token includes: UserID, RoleID, permissions

### Authorization
- Users can only access own subscriptions (unless admin)
- Admin-only endpoints for plan management
- Webhook signature validation

### Payment Security
- Stripe handles card data (PCI compliant)
- Webhook idempotency checks
- Full audit trail for all billing operations

---

## 📝 Important Constants

```csharp
// SubscriptionConstants.cs
public const int MAX_PAYMENT_RETRY_ATTEMPTS = 3;
public const int DEFAULT_BILLING_GRACE_PERIOD_DAYS = 7;
public const int DEFAULT_TRIAL_DURATION_DAYS = 14;
public const int UNLIMITED_PRIVILEGE_VALUE = -1;
public const int DEFAULT_PRIVILEGE_RESET_PERIOD_DAYS = 30;

// Subscription Statuses
Pending, Active, Paused, Cancelled, Expired
PaymentFailed, TrialActive, TrialExpired, Suspended

// Plan Types
Standard, UsageBased, Premium, Enterprise

// Payment Statuses
Pending, Processing, Succeeded, Failed, Cancelled, Refunded

// Payment Types
Subscription, Trial, Setup, Upgrade, Downgrade, Overage, Recurring
```

---

## 🧪 Testing Strategy

### Unit Tests
- Service methods
- Billing calculations
- Privilege usage logic
- Plan versioning

### Integration Tests
- End-to-end subscription creation
- Automated billing flow
- Webhook processing
- Migration workflows

### Test Files
```
SmartTelehealth.Tests/
├── Services/
│   ├── SubscriptionPlanServiceTests.cs
│   ├── SubscriptionLifecycleServiceTests.cs
│   ├── SubscriptionBillingServiceTests.cs
│   └── PrivilegeServiceTests.cs
└── Integration/
    ├── SubscriptionFlowTests.cs
    └── BillingIntegrationTests.cs
```

---

## 🐛 Troubleshooting

### Common Issues

**1. Subscription Not Creating**
- Check Stripe webhook configuration
- Verify webhook secret in appsettings
- Check Stripe logs for errors
- Validate plan is active

**2. Billing Not Processing**
- Check AutomatedBillingBackgroundService is running
- Verify subscription status is Active
- Check NextBillingDate is in past
- Verify payment method in Stripe

**3. Privilege Usage Not Updating**
- Check subscription is Active
- Verify privilege configuration in plan
- Check UserSubscriptionPrivilegeUsage records exist
- Review PrivilegeService logs

**4. Plan Migration Not Occurring**
- Check ScheduledMigrationBackgroundService is running
- Verify ScheduledMigrationDate is in past
- Check migration Status is Pending
- Review PlanVersioningService logs

**5. Stripe Sync Issues**
- Check StripeSyncJob is running
- Verify Stripe API keys are valid
- Review sync logs for errors
- Check network connectivity to Stripe

---

## 📚 Additional Resources

1. **Main Analysis**: `COMPREHENSIVE_SUBSCRIPTION_MANAGEMENT_ANALYSIS.md`
2. **Flow Diagrams**: `SUBSCRIPTION_FLOW_DIAGRAMS.md`
3. **SQL Schema**: `backend/SmartTelehealth.Infrastructure/Migrations/Subscription_Management_CreateTables.sql`
4. **Extraction Summary**: `backend/Subscription_Plan_Management_Extraction_Summary.md`

---

## 🎓 Key Learnings

### Architecture Patterns
- **Clean Architecture**: Clear separation of concerns
- **Repository Pattern**: Data access abstraction
- **Unit of Work**: Transaction management
- **DTO Pattern**: Data transfer objects
- **Service Layer**: Business logic encapsulation

### Design Principles
- **DRY**: Don't Repeat Yourself
- **SOLID**: Single Responsibility, Open/Closed, etc.
- **Healthcare First**: Privilege-based pricing model
- **Versioning**: Preserve existing subscriber pricing
- **Audit Trail**: Complete history of all operations

### Best Practices
- **Idempotency**: Prevent duplicate webhook processing
- **Retry Logic**: Handle transient failures
- **Error Handling**: Comprehensive logging and recovery
- **Background Services**: Automated operations
- **Security**: Stripe PCI compliance, JWT authentication

---

**Quick Reference Guide Complete** 🎉

**For detailed information, see:**
- `COMPREHENSIVE_SUBSCRIPTION_MANAGEMENT_ANALYSIS.md` - Full technical analysis
- `SUBSCRIPTION_FLOW_DIAGRAMS.md` - Visual flow diagrams
- Source code - For implementation details

