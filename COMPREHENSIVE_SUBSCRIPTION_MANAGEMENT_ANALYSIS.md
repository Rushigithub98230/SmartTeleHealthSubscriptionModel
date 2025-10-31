# Comprehensive Subscription Management System Analysis

## Executive Summary

The SmartTelehealth backend implements a sophisticated, healthcare-focused subscription management system with privilege-based pricing, automated billing, plan versioning, and full Stripe integration. The system follows a clean architecture pattern with separation of concerns across Core, Application, Infrastructure, and API layers.

---

## 1. Architecture Overview

### 1.1 Layer Structure
```
SmartTelehealth.Core/          # Domain entities, interfaces, enums
SmartTelehealth.Application/   # Business logic, DTOs, services
SmartTelehealth.Infrastructure/# Data access, external integrations
SmartTelehealth.API/           # Controllers, API endpoints
```

### 1.2 Key Design Patterns
- **Repository Pattern**: Data access abstraction
- **Unit of Work**: Transaction management
- **Dependency Injection**: Service composition
- **DTO Pattern**: Data transfer objects
- **Service Layer**: Business logic encapsulation

---

## 2. Core Entities & Relationships

### 2.1 Subscription Plan Architecture

#### **SubscriptionPlan** (Core Template)
```12:454:backend/SmartTelehealth.Core/Entities/SubscriptionPlan.cs
// Core subscription plan entity
public class SubscriptionPlan : BaseEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; }  // e.g., "Basic - Monthly"
    public PlanType PlanType { get; set; }
    
    // Pricing Model
    public decimal BasePrice { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public decimal? BillingDiscountPercentage { get; set; }
    public bool IsAutoCalculatedPrice { get; set; } = true;
    public decimal PrivilegesTotalCost { get; set; }
    public decimal? AdminCommissionPercent { get; set; }
    
    // Plan Versioning (Healthcare Feature)
    public int VersionNumber { get; set; } = 1;
    public bool IsLatestVersion { get; set; } = true;
    public Guid? ParentPlanId { get; set; }
    public virtual SubscriptionPlan? ParentPlan { get; set; }
    public virtual ICollection<SubscriptionPlan> ChildVersions { get; set; }
    
    // Billing Cycle (NEW ARCHITECTURE)
    public Guid BillingCycleId { get; set; }  // One plan = One billing cycle
    public virtual MasterBillingCycle BillingCycle { get; set; }
    
    // Stripe Integration
    public string? StripeProductId { get; set; }
    public string? StripePriceId { get; set; }
    
    // Collections
    public virtual ICollection<SubscriptionPlanPrivilege> PlanPrivileges { get; set; }
    public virtual ICollection<Subscription> Subscriptions { get; set; }
}
```

**Key Features:**
- Each plan supports ONE billing cycle (e.g., "Basic - Monthly", "Basic - Annual")
- Privilege-based pricing: BasePrice = Σ(Privilege.Value × Privilege.PrivilegeBaseCost) + Commission
- Plan versioning preserves existing subscriptions
- Computed property `EffectivePrice` applies discounts sequentially

#### **SubscriptionPlanPrivilege** (Junction Entity)
```14:162:backend/SmartTelehealth.Core/Entities/SubscriptionPlanPrivilege.cs
public class SubscriptionPlanPrivilege : BaseEntity
{
    public Guid SubscriptionPlanId { get; set; }
    public Guid PrivilegeId { get; set; }
    public int Value { get; set; }  // -1 = unlimited, 0 = disabled, >0 = limited
    
    // Pricing
    public decimal PrivilegeBaseCost { get; set; }  // BASE COST per unit
    public decimal UnitCost { get; set; }  // OVERAGE COST per unit
    
    // Computed Properties
    public bool IsUnlimited => Value == -1;
    public bool HasOverageCharges => UnitCost > 0 && !IsUnlimited;
}
```

**Business Logic:**
- `PrivilegeBaseCost`: Used to calculate plan's base price
- `UnitCost`: Charged when user exceeds limits (uses latest plan pricing to prevent abuse)
- Example: 5 consultations × $3 base cost = $15 contribution to plan price

### 2.2 User Subscription Architecture

#### **Subscription** (User Instance)
```14:411:backend/SmartTelehealth.Core/Entities/Subscription.cs
public class Subscription : BaseEntity
{
    // Status Constants
    public static class SubscriptionStatuses
    {
        public const string Pending = "Pending";
        public const string Active = "Active";
        public const string Paused = "Paused";
        public const string Cancelled = "Cancelled";
        public const string Expired = "Expired";
        public const string PaymentFailed = "PaymentFailed";
        public const string TrialActive = "TrialActive";
        public const string TrialExpired = "TrialExpired";
        public const string Suspended = "Suspended";
    }
    
    public Guid Id { get; set; }
    public int UserId { get; set; }
    public Guid SubscriptionPlanId { get; set; }
    public string Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime NextBillingDate { get; set; }
    public decimal CurrentPrice { get; set; }
    public bool AutoRenew { get; set; } = true;
    
    // Trial Support
    public bool IsTrialSubscription { get; set; }
    public DateTime? TrialStartDate { get; set; }
    public DateTime? TrialEndDate { get; set; }
    
    // Stripe Integration
    public string? StripeSubscriptionId { get; set; }
    public string? StripeCustomerId { get; set; }
    public string? StripePriceId { get; set; }
    
    // Planned Changes (Scheduled without proration)
    public string? PendingChangeType { get; set; }
    public Guid? PendingPlanChangeId { get; set; }
    public DateTime? PlanChangeEffectiveDate { get; set; }
    
    // Collections
    public virtual User User { get; set; }
    public virtual SubscriptionPlan SubscriptionPlan { get; set; }
    public virtual ICollection<SubscriptionPayment> Payments { get; set; }
    public virtual ICollection<UserSubscriptionPrivilegeUsage> PrivilegeUsages { get; set; }
}
```

### 2.3 Privilege & Usage Tracking

#### **Privilege** (Core Definition)
```13:70:backend/SmartTelehealth.Core/Entities/Privilege.cs
public class Privilege : BaseEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; }  // e.g., "TeleConsultation", "Messaging"
    public string? Description { get; set; }
    public Guid PrivilegeTypeId { get; set; }
    
    // Collections
    public virtual ICollection<SubscriptionPlanPrivilege> PlanPrivileges { get; set; }
    public virtual ICollection<UserSubscriptionPrivilegeUsage> UsageRecords { get; set; }
}
```

#### **UserSubscriptionPrivilegeUsage** (Usage Tracking)
```14:170:backend/SmartTelehealth.Core/Entities/UserSubscriptionPrivilegeUsage.cs
public class UserSubscriptionPrivilegeUsage : BaseEntity
{
    public Guid Id { get; set; }
    public Guid SubscriptionId { get; set; }
    public Guid SubscriptionPlanPrivilegeId { get; set; }
    public Guid PrivilegeId { get; set; }
    
    public int UsedValue { get; set; }        // How many used
    public int AllowedValue { get; set; }     // -1 = unlimited, >0 = limited
    public DateTime UsagePeriodStart { get; set; }
    public DateTime UsagePeriodEnd { get; set; }
    
    // Computed Properties
    public int RemainingValue => AllowedValue == -1 ? int.MaxValue : Math.Max(0, AllowedValue - UsedValue);
    public bool IsExhausted => !IsUnlimited && UsedValue >= AllowedValue;
    public decimal UsagePercentage => IsUnlimited ? 0 : AllowedValue == 0 ? 100 : (decimal)UsedValue / AllowedValue * 100;
}
```

**Usage Flow:**
1. User subscribes → System creates `UserSubscriptionPrivilegeUsage` records
2. User uses privilege → Increment `UsedValue` and log to `PrivilegeUsageHistory`
3. Exceeds limit → Charge overage using `UnitCost` from latest plan version
4. Billing cycle ends → Background service resets `UsedValue` to 0

### 2.4 Billing & Payment Architecture

#### **BillingRecord** (Master Billing Entity)
```13:372:backend/SmartTelehealth.Core/Entities/BillingRecord.cs
public class BillingRecord : BaseEntity
{
    public Guid Id { get; set; }
    public int UserId { get; set; }
    public Guid? SubscriptionId { get; set; }
    
    // Amounts
    public decimal Amount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal TotalAmount { get; set; }
    
    // Status & Type
    public BillingStatus Status { get; set; }
    public BillingType Type { get; set; }
    
    // Stripe Integration
    public string? StripePaymentIntentId { get; set; }
    public string? StripeInvoiceId { get; set; }
    
    // Dates
    public DateTime BillingDate { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime DueDate { get; set; }
}
```

#### **SubscriptionPayment** (Subscription-Specific Payment)
```14:325:backend/SmartTelehealth.Core/Entities/SubscriptionPayment.cs
public class SubscriptionPayment : BaseEntity
{
    public Guid Id { get; set; }
    public Guid SubscriptionId { get; set; }
    public Guid BillingRecordId { get; set; }
    
    public decimal Amount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal NetAmount { get; set; }
    
    public PaymentStatus Status { get; set; }  // Pending, Succeeded, Failed, Refunded
    public PaymentType Type { get; set; }      // Subscription, Trial, Upgrade, Overage
    
    // Billing Period
    public DateTime BillingPeriodStart { get; set; }
    public DateTime BillingPeriodEnd { get; set; }
    
    // Stripe Integration
    public string? StripePaymentIntentId { get; set; }
    public string? StripeInvoiceId { get; set; }
    public string? ReceiptUrl { get; set; }
    
    // Retry Logic
    public int AttemptCount { get; set; }
    public DateTime? NextRetryAt { get; set; }
    public decimal RefundedAmount { get; set; }
}
```

### 2.5 Plan Versioning Architecture

#### **ScheduledPlanMigration** (Migration Tracking)
```10:66:backend/SmartTelehealth.Core/Entities/ScheduledPlanMigration.cs
public class ScheduledPlanMigration : BaseEntity
{
    public Guid Id { get; set; }
    public Guid SubscriptionId { get; set; }
    public Guid FromPlanId { get; set; }
    public Guid ToPlanId { get; set; }
    
    public DateTime NotificationDate { get; set; }
    public DateTime ScheduledMigrationDate { get; set; }  // User's next renewal
    
    public string Status { get; set; }  // Pending, UserOptedOut, Completed, Failed
    public string? UserDecision { get; set; }  // Accept, Cancel
    
    // Navigation
    public virtual Subscription Subscription { get; set; }
    public virtual SubscriptionPlan FromPlan { get; set; }
    public virtual SubscriptionPlan ToPlan { get; set; }
}
```

---

## 3. Service Layer Business Logic

### 3.1 SubscriptionPlanService

**Responsibilities:**
- CRUD operations for plans
- Activate/deactivate plans
- Privilege management
- Stripe product/price creation
- Plan comparison for UI

**Key Methods:**
```135:136:backend/SmartTelehealth.Application/DependencyInjection.cs
services.AddScoped<ISubscriptionPlanService, SubscriptionPlanService>(provider =>
    new SubscriptionPlanService(...));
```

### 3.2 SubscriptionService

**Responsibilities:**
- Fetch subscriptions by user/plan
- Get active subscriptions
- Query subscriptions with filters

**Not Lifecycle Operations:** (Delegated to `SubscriptionLifecycleService`)

### 3.3 SubscriptionLifecycleService (Core Lifecycle Logic)

**Responsibilities:**
- Create subscriptions (with Stripe integration)
- Cancel/pause/resume subscriptions
- Upgrade/downgrade subscriptions
- Auto-renewal logic
- Trial management
- Scheduled plan changes

**Subscription Creation Flow:**
```211:274:backend/SmartTelehealth.Application/Services/SubscriptionLifecycleService.cs
// 1. Validate plan exists and is active
var plan = await _subscriptionPlanRepository.GetByIdWithDetailsAsync(createDto.PlanId);

// 2. Check for duplicate active/paused subscriptions

// 3. Get or create Stripe customer
stripeCustomerId = await _stripeService.GetOrCreateCustomerAsync(
    createDto.UserId, createDto.PaymentMethodId, tokenModel);

// 4. Create Stripe subscription
stripeSubscriptionId = await _stripeService.CreateSubscriptionAsync(
    stripeCustomerId, stripePriceId, createDto.PaymentMethodId, tokenModel);

// 5. Create local subscription entity
var entity = new Subscription {
    UserId = createDto.UserId,
    SubscriptionPlanId = createDto.PlanId,
    Status = plan.IsTrialAllowed ? SubscriptionStatuses.TrialActive : SubscriptionStatuses.Active,
    StartDate = DateTime.UtcNow,
    NextBillingDate = BillingCycleCalculator.CalculateNextBillingDate(...),
    CurrentPrice = BillingCalculationService.GetEffectivePlanPrice(plan, ...),
    StripeSubscriptionId = stripeSubscriptionId,
    // ...
};

// 6. BEGIN TRANSACTION - Atomic operation
await _unitOfWork.BeginTransactionAsync();

// 7. Create subscription
created = await _subscriptionRepository.CreateAsync(entity);

// 8. Create status history
await RecordStatusChangeAsync(...);

// 9. Initialize privileges (allocate usage records)
await _privilegeService.InitializeSubscriptionPrivilegesAsync(...);

// 10. Create billing record
billingResult = await _subscriptionBillingService.CreateSubscriptionBillingAsync(...);

// 11. Create subscription payment
await CreateSubscriptionPaymentForCheckoutAsync(...);

// 12. COMMIT TRANSACTION
await _unitOfWork.CommitTransactionAsync();
```

### 3.4 SubscriptionBillingService (Consolidated Billing)

**51 Methods** covering all billing operations:

**Core Billing:**
- `CreateSubscriptionBillingAsync()`: Create billing records
- `ProcessRecurringBillingAsync()`: Automated recurring billing
- `ProcessOverageChargesAsync()`: Charge for exceeded usage
- `GenerateInvoiceAsync()`: Generate invoices

**Privilege Usage Tracking:**
```191:274:backend/SmartTelehealth.Application/Services/SubscriptionBillingService.cs
public async Task<JsonModel> ProcessPrivilegeUsageAsync(ProcessPrivilegeUsageDto usageDto, TokenModel tokenModel)
{
    // 1. Get active subscription
    var subscription = await _subscriptionRepository.GetActiveSubscriptionByUserIdAsync(usageDto.UserId);
    
    // 2. Get or create privilege usage record
    var privilegeUsage = await GetOrCreatePrivilegeUsageAsync(...);
    
    // 3. Check plan privilege configuration
    var planPrivilege = await _subscriptionPlanRepository.GetPlanPrivilegeAsync(...);
    
    // 4. Record usage event
    await RecordUsageEventAsync(usageDto.UserId, usageDto.PrivilegeId, usageDto.UsageCount, tokenModel);
    
    // 5. Check for overage
    var currentUsage = await _privilegeUsageRepository.GetByUserAndPrivilegeAsync(...);
    var isOverLimit = currentUsage != null && currentUsage.UsedValue >= currentUsage.AllowedValue;
    
    // 6. Return result with overage info
}
```

### 3.5 PrivilegeService

**Responsibilities:**
- Check if user can use privilege
- Record privilege usage
- Calculate remaining privileges
- Reset privilege counters
- Track usage history

**Key Method:**
```180:274:backend/SmartTelehealth.Application/Services/PrivilegeService.cs
public async Task<bool> UsePrivilegeAsync(Guid subscriptionId, string privilegeName, int amount, TokenModel tokenModel)
{
    // 1. Get plan privilege configuration
    var planPrivilege = await GetPlanPrivilegeAsync(subscriptionId, privilegeName);
    
    // 2. Check if privilege is disabled
    if (planPrivilege.Value == 0) return false;
    
    // 3. Handle unlimited privileges
    if (planPrivilege.Value == -1) {
        // Log and return true
        return true;
    }
    
    // 4. Check remaining amount
    var remaining = await GetRemainingPrivilegeAsync(subscriptionId, privilegeName, tokenModel);
    if (remaining < amount) return false;
    
    // 5. Update or create usage record
    limitedUsage.UsedValue += amount;
    
    // 6. Add usage history
    await AddUsageHistoryAsync(...);
    
    return true;
}
```

### 3.6 PlanVersioningService

**Healthcare Feature:** Create plan versions instead of modifying existing plans

**Workflow:**
```65:193:backend/SmartTelehealth.Application/Services/PlanVersioningService.cs
public async Task<JsonModel> CreateNewPlanVersionAsync(
    Guid existingPlanId, UpdateSubscriptionPlanDto updateDto, TokenModel tokenModel)
{
    // 1. Get existing plan
    var existingPlan = await _subscriptionPlanRepository.GetByIdWithDetailsAsync(existingPlanId);
    
    // 2. Check for active subscriptions
    var activeSubsCount = await _subscriptionPlanRepository.GetActiveSubscriptionsCountAsync(existingPlanId);
    
    // 3. Determine parent plan and new version number
    var parentPlanId = existingPlan.ParentPlanId ?? existingPlan.Id;
    var allVersions = await _subscriptionPlanRepository.GetAllVersionsOfPlanAsync(parentPlanId);
    var newVersionNumber = allVersions.Max(v => v.VersionNumber) + 1;
    
    // 4. Create new version entity (copy from old version)
    var newVersion = new SubscriptionPlan { ... };
    
    // 5. Copy privileges to new version
    await CopyPrivilegesToNewVersionAsync(existingPlan, newVersion, tokenModel);
    
    // 6. Save new version (marks old version as not latest)
    var createdVersion = await _subscriptionPlanRepository.CreateNewPlanVersionAsync(newVersion);
    
    // 7. Create Stripe resources for new version
    await CreateStripeResourcesForPlanAsync(createdVersion, tokenModel);
    
    // 8. Calculate auto price if enabled
    if (createdVersion.IsAutoCalculatedPrice) {
        var calculatedPrice = await _pricingService.CalculatePlanPriceAsync(createdVersion.Id, true);
        createdVersion.BasePrice = calculatedPrice;
    }
    
    // 9. Schedule migrations for existing subscribers
    if (activeSubsCount > 0) {
        await ScheduleMigrationsForActiveSubscribersAsync(...);
    }
    
    await _unitOfWork.CommitTransactionAsync();
}
```

### 3.7 PlanPricingService

**Healthcare Pricing Model:**
```
Plan Base Price = Σ(Privilege.Value × Privilege.PrivilegeBaseCost) + AdminCommission
Effective Price = Plan Base Price → Apply DiscountPercentage → Apply BillingDiscountPercentage
```

**Key Method:**
```183:194:backend/SmartTelehealth.Application/DependencyInjection.cs
services.AddScoped<IPlanPricingService, PlanPricingService>(provider =>
    new PlanPricingService(
        provider.GetRequiredService<ISubscriptionPlanRepository>(),
        provider.GetRequiredService<ISystemSettingsRepository>(),
        ...));
```

---

## 4. Stripe Integration

### 4.1 StripeService (Infrastructure Layer)

**Responsibilities:**
- Customer management (create, retrieve, update)
- Payment method management
- Subscription lifecycle operations
- Invoice management
- Product/price creation
- Webhook event processing

**Key Methods:**
```525:566:backend/SmartTelehealth.Infrastructure/Services/StripeService.cs
public async Task<string> CreateSubscriptionAsync(string customerId, string priceId, 
    string paymentMethodId, TokenModel tokenModel)
{
    var subscriptionCreateOptions = new SubscriptionCreateOptions {
        Customer = customerId,
        Items = new List<SubscriptionItemOptions> {
            new SubscriptionItemOptions { Price = priceId }
        },
        DefaultPaymentMethod = paymentMethodId,
        Metadata = new Dictionary<string, string> {
            { "created_by_user_id", tokenModel.UserID.ToString() },
            { "created_by_role_id", tokenModel.RoleID.ToString() }
        }
    };
    
    var subscriptionService = new SubscriptionService();
    var subscription = await subscriptionService.CreateAsync(subscriptionCreateOptions);
    return subscription.Id;
}
```

### 4.2 StripeWebhookController

**Handles Webhook Events:**
```101:203:backend/SmartTelehealth.API/Controllers/StripeWebhookController.cs
[HttpPost("webhook")]
public async Task<JsonModel> HandleWebhook()
{
    // 1. Validate webhook signature
    var stripeEvent = EventUtility.ConstructEvent(json, signature, webhookSecret);
    
    // 2. Check idempotency
    var idempotencyResult = await _webhookIdempotencyService.CheckIdempotencyAsync(stripeEvent.Id, stripeEvent.Type);
    if (!idempotencyResult.ShouldProcess) {
        return new JsonModel { Message = $"Event skipped: {idempotencyResult.Reason}", StatusCode = 200 };
    }
    
    // 3. Process webhook with retry logic
    await ProcessWebhookWithRetryAsync(stripeEvent);
    
    // 4. Mark as processed
    await _webhookIdempotencyService.MarkAsProcessedAsync(stripeEvent.Id, duration);
    
    return new JsonModel { Message = "Webhook processed successfully", StatusCode = 200 };
}
```

**Supported Events:**
- `customer.subscription.created`: Local subscription already exists
- `customer.subscription.updated`: Sync status, dates, plan changes
- `customer.subscription.deleted`: Mark as cancelled
- `invoice.payment_succeeded`: Create payment record, update subscription
- `invoice.payment_failed`: Handle failed payment, retry logic

### 4.3 StripeSynchronizationService

**Responsibilities:**
- Sync local database with Stripe
- Create missing Stripe products/prices
- Update local records from Stripe data
- Handle synchronization errors

---

## 5. Background Services (Automated Operations)

### 5.1 AutomatedBillingBackgroundService

**Runs Every Hour:**

```31:84:backend/SmartTelehealth.Infrastructure/Services/AutomatedBillingBackgroundService.cs
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    while (!stoppingToken.IsCancellationRequested) {
        try {
            await ProcessBillingCycleAsync();
            await Task.Delay(_billingInterval, stoppingToken);  // 1 hour
        } catch (Exception ex) {
            _logger.LogError(ex, "Error in automated billing cycle");
        }
    }
}

private async Task ProcessBillingCycleAsync()
{
    // 1. Process subscriptions due for billing
    await ProcessDueSubscriptionsAsync(...);
    
    // 2. Process failed payment retries
    await ProcessFailedPaymentRetriesAsync(...);
    
    // 3. Reset usage counters for new billing cycles
    await ResetUsageCountersAsync(...);
}
```

**Process Due Subscriptions:**
1. Query subscriptions where `NextBillingDate <= UtcNow` and `Status = Active`
2. Create `BillingRecord` with `BillingType.Subscription`
3. Process payment via Stripe
4. Update `NextBillingDate` to next cycle
5. Send confirmation notification

### 5.2 PrivilegeResetBackgroundService

**Resets usage counters:**
- Runs periodically
- Checks for `UsagePeriodEnd <= UtcNow`
- Resets `UsedValue` to 0
- Updates `UsagePeriodStart` and `UsagePeriodEnd` to next billing cycle

### 5.3 ScheduledMigrationBackgroundService

**Processes plan migrations:**
- Checks for `ScheduledMigrationDate <= UtcNow` and `Status = Pending`
- Updates subscription to new plan version
- Creates billing record if price changed
- Sends confirmation notification
- Marks migration as `Completed`

### 5.4 FailedRefundRetryBackgroundService

**Retries failed refunds:**
- Queries `FailedRefund` records
- Retries refund via Stripe
- Updates refund status
- Logs results

### 5.5 UnprocessedWebhookRetryService

**Retries failed webhooks:**
- Queries `UnprocessedWebhookEvent` records
- Retries webhook processing
- Handles errors gracefully

### 5.6 StripeSyncJob

**Hourly Stripe Reconciliation:**
- Sync subscriptions
- Sync products/prices
- Detect discrepancies
- Log synchronization results

### 5.7 ReconciliationBackgroundService

**Nightly Data Integrity Checks:**
- Detect orphaned records
- Validate relationships
- Flag inconsistencies
- Generate reports

---

## 6. API Controllers

### 6.1 SubscriptionPlansController

**Key Endpoints:**
- `GET /api/subscriptionplans/active`: Public active plans
- `GET /api/subscriptionplans/{id}`: Plan details
- `GET /api/subscriptionplans/category/{categoryId}/compare`: Compare billing cycles
- `POST /api/subscriptionplans`: Create plan (admin)
- `PUT /api/subscriptionplans/{id}`: Update plan (admin)
- `POST /api/subscriptionplans/{id}/activate`: Activate plan
- `POST /api/subscriptionplans/{id}/privileges`: Add privilege

### 6.2 SubscriptionsController

**Key Endpoints:**
- `GET /api/subscriptions/{id}`: Get subscription
- `GET /api/subscriptions/user/{userId}`: User's subscriptions
- `POST /api/subscriptions`: Create (admin only - direct subscription bypasses payment)
- `POST /api/subscriptions/{id}/cancel`: Cancel subscription
- `POST /api/subscriptions/{id}/pause`: Pause subscription
- `POST /api/subscriptions/{id}/resume`: Resume subscription
- `PUT /api/subscriptions/{id}/upgrade`: Upgrade subscription
- `POST /api/subscriptions/{id}/schedule-upgrade`: Schedule upgrade at renewal

**Security:** Regular users MUST use `POST /api/Checkout/create-session/{planId}` for Stripe Checkout flow.

### 6.3 StripeWebhookController

**Endpoints:**
- `POST /api/stripewebhook/webhook`: Handle Stripe webhooks

---

## 7. Master Data Tables

### 7.1 MasterBillingCycle
- Monthly (30 days)
- Quarterly (90 days)
- Annual (365 days)
- Custom cycles

### 7.2 MasterCurrency
- USD, EUR, GBP, etc.
- Exchange rates
- Display formats

### 7.3 MasterPrivilegeType
- TeleConsultation
- Messaging
- Medication Delivery
- Follow-up Care
- etc.

---

## 8. Complete Subscription Lifecycle

### 8.1 Subscription Creation Flow

**User Journey:**
1. User browses plans → `GET /api/subscriptionplans/active`
2. User selects plan → `POST /api/Checkout/create-session/{planId}`
3. Stripe redirects to payment → User completes payment
4. Stripe webhook fires → `POST /api/stripewebhook/webhook`
5. System creates local subscription → `SubscriptionLifecycleService.CreateSubscriptionAsync()`
6. System allocates privileges → `PrivilegeService.InitializeSubscriptionPrivilegesAsync()`
7. System creates billing record → `SubscriptionBillingService.CreateSubscriptionBillingAsync()`
8. User receives confirmation email

**Technical Flow:**
```
User Request → StripeController.CreateCheckoutSession()
               ↓
        Create Stripe Checkout Session
               ↓
        User Completes Payment
               ↓
        Stripe Webhook: checkout.session.completed
               ↓
        StripeWebhookController.HandleWebhook()
               ↓
        WebhookService.ProcessCheckoutCompletedAsync()
               ↓
        SubscriptionLifecycleService.SyncSubscriptionFromCheckoutAsync()
               ↓
        BEGIN TRANSACTION
               ↓
        Create Subscription Entity
               ↓
        Record Status History
               ↓
        Initialize Privileges (UserSubscriptionPrivilegeUsage)
               ↓
        Create BillingRecord
               ↓
        Create SubscriptionPayment
               ↓
        COMMIT TRANSACTION
               ↓
        Send Welcome Email
```

### 8.2 Subscription Renewal Flow

**Automated Renewal:**
1. `AutomatedBillingBackgroundService` runs every hour
2. Queries subscriptions where `NextBillingDate <= UtcNow` and `Status = Active`
3. Creates `BillingRecord` with `BillingType.Subscription`
4. Processes payment via Stripe `PaymentIntent`
5. Updates `NextBillingDate` to next cycle
6. Resets privilege usage counters
7. Creates `SubscriptionPayment` record
8. Sends renewal confirmation

### 8.3 Privilege Usage Flow

**User Uses Service:**
1. User requests service (e.g., consultation)
2. `PrivilegeService.UsePrivilegeAsync()` checks remaining usage
3. If allowed → Increment `UsedValue`, log to `PrivilegeUsageHistory`
4. If exceeded → Optional overage charge using `UnitCost`

**Usage Tracking:**
```
User Action → PrivilegeService.UsePrivilegeAsync()
              ↓
        Check Remaining Privilege
              ↓
        If Allowed:
              ↓
        Update UserSubscriptionPrivilegeUsage.UsedValue
              ↓
        Create PrivilegeUsageHistory Record
              ↓
        Return Success
              
        If Exceeded:
              ↓
        Optional: Charge Overage (SubscriptionBillingService.ProcessOverageChargesAsync())
              ↓
        Return Failure
```

### 8.4 Plan Versioning Flow

**Admin Updates Plan:**
1. Admin calls `PUT /api/subscriptionplans/{id}` (admin only)
2. `PlanVersioningService.CreateNewPlanVersionAsync()` creates new version
3. Old version marked as `IsLatestVersion = false`
4. New version marked as `IsLatestVersion = true`
5. Active subscribers stay on old version (grandfathered)
6. System schedules migrations for next renewal
7. Users receive notification X days before migration

**User Migration Flow:**
1. `ScheduledMigrationBackgroundService` detects due migrations
2. Queries `ScheduledPlanMigration` where `ScheduledMigrationDate <= UtcNow` and `Status = Pending`
3. Updates `Subscription.SubscriptionPlanId` to new version
4. If price increased → Creates billing adjustment
5. Marks migration as `Completed`

### 8.5 Subscription Cancellation Flow

**Immediate Cancellation:**
1. User calls `POST /api/subscriptions/{id}/cancel`
2. `SubscriptionLifecycleService.CancelSubscriptionAsync()` validates access
3. Updates `Status = "Cancelled"`, `CancelledDate = UtcNow`
4. Cancels Stripe subscription via `StripeService.CancelSubscriptionAsync()`
5. Records status history
6. Sends cancellation confirmation

---

## 9. Pricing & Billing Calculations

### 9.1 Plan Base Price Calculation

```
For each SubscriptionPlanPrivilege in Plan:
    Contribution = Privilege.Value × Privilege.PrivilegeBaseCost

PrivilegesTotalCost = Σ(Contributions)

AdminCommission = PrivilegesTotalCost × (AdminCommissionPercent / 100)
                OR PrivilegesTotalCost × (SystemDefaultCommissionPercent / 100)

BasePrice = PrivilegesTotalCost + AdminCommission
```

**Example:**
```
Plan: Basic - Monthly
Privileges:
- TeleConsultation: 5 × $3 = $15
- Messaging: 100 × $0.01 = $1
PrivilegesTotalCost = $16
AdminCommission = $16 × 10% = $1.60
BasePrice = $17.60
```

### 9.2 Effective Price Calculation

```
EffectivePrice = BasePrice

// Apply Promotional Discount
if (DiscountPercentage > 0 && DiscountValidUntil >= UtcNow):
    EffectivePrice = EffectivePrice × (1 - DiscountPercentage / 100)

// Apply Billing Cycle Discount
if (BillingDiscountPercentage > 0):
    EffectivePrice = EffectivePrice × (1 - BillingDiscountPercentage / 100)

Final Effective Price = max(EffectivePrice, 0)
```

**Example:**
```
BasePrice = $17.60
DiscountPercentage = 20%
BillingDiscountPercentage = 10%

Step 1: Apply promotional discount
$17.60 × (1 - 20/100) = $14.08

Step 2: Apply billing discount
$14.08 × (1 - 10/100) = $12.67

EffectivePrice = $12.67
```

### 9.3 Overage Charge Calculation

**When user exceeds privilege limit:**
```
OverageCharge = OverageCount × UnitCost (from latest plan version)

// Example: User used 6 consultations, plan allows 5
OverageCount = 6 - 5 = 1
UnitCost = $15 (from latest Basic plan version)
OverageCharge = 1 × $15 = $15
```

---

## 10. Database Relationships

### 10.1 Entity Relationship Diagram

```
User (1) ──┐
           │
           ├──→ (M) Subscription
           │          │
           │          ├──→ (1) SubscriptionPlan
           │          │          │
           │          │          ├──→ (M) SubscriptionPlanPrivilege
           │          │          │          ├──→ (1) Privilege
           │          │          │          └──→ (1) SubscriptionPlan
           │          │          │
           │          │          └──→ (1) MasterBillingCycle
           │          │
           │          ├──→ (M) SubscriptionPayment
           │          │          ├──→ (1) BillingRecord
           │          │          └──→ (1) MasterCurrency
           │          │
           │          ├──→ (M) UserSubscriptionPrivilegeUsage
           │          │          ├──→ (1) SubscriptionPlanPrivilege
           │          │          ├──→ (1) Privilege
           │          │          └──→ (M) PrivilegeUsageHistory
           │          │
           │          └──→ (M) ScheduledPlanMigration
           │                     ├──→ (1) SubscriptionPlan (from)
           │                     └──→ (1) SubscriptionPlan (to)
           │
           └──→ (M) BillingRecord
                      ├──→ (1) MasterCurrency
                      └──→ (0..1) Subscription
```

---

## 11. Key Business Rules

### 11.1 Subscription Rules
1. One active subscription per user
2. Admin can create subscriptions directly (bypasses payment)
3. Regular users must use Stripe Checkout
4. Trial subscriptions auto-convert to Active on trial end
5. Cancelled subscriptions can be reactivated within grace period

### 11.2 Privilege Rules
1. Unlimited privileges (`-1`) never exhaust
2. Disabled privileges (`0`) cannot be used
3. Limited privileges (`>0`) can be exhausted
4. Overage charges use latest plan pricing (prevents abuse)
5. Privilege resets happen at billing cycle boundary

### 11.3 Pricing Rules
1. Auto-calculated plans sum privilege costs + commission
2. Manual plans use admin-specified `BasePrice`
3. Discounts apply sequentially (promotional → billing)
4. Plan versions preserve existing subscriber pricing
5. New subscribers always get latest plan version

### 11.4 Payment Rules
1. Max 3 payment retry attempts
2. Failed payments trigger subscription status change after max retries
3. Overage charges billed separately
4. Refunds tracked with full audit trail
5. All payments linked to `BillingRecord` and `SubscriptionPayment`

---

## 12. Security & Access Control

### 12.1 API Security
- JWT authentication required (except public plan browsing)
- Role-based authorization
- User can only access own subscriptions (unless admin)
- Admin-only endpoints for plan creation/modification

### 12.2 Payment Security
- Stripe handles card data (PCI compliant)
- Webhook signature validation
- Idempotency checks prevent duplicate processing
- Audit trail for all billing operations

---

## 13. Testing & Quality Assurance

**Test Coverage:**
- Unit tests for services
- Integration tests for workflows
- Webhook processing tests
- Billing calculation accuracy tests
- Privilege usage tests
- Plan versioning tests

---

## 14. Conclusion

The SmartTelehealth subscription management system is a **production-grade, enterprise-level** implementation featuring:

✅ **Healthcare-focused pricing model** with privilege-based billing  
✅ **Plan versioning** to preserve existing subscriber pricing  
✅ **Full Stripe integration** with webhook processing  
✅ **Automated billing** with multiple background services  
✅ **Comprehensive usage tracking** and overage charges  
✅ **Robust error handling** and retry logic  
✅ **Complete audit trail** for all operations  
✅ **Scalable architecture** with clean separation of concerns  

The system supports complex business requirements while maintaining code quality, testability, and maintainability.

---

**Document Generated:** ${new Date().toISOString()}  
**Analysis By:** AI Assistant  
**Based On:** Complete codebase analysis of SmartTelehealth backend
