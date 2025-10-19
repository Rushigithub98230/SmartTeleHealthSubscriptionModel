# 📘 User Subscription Lifecycle - Developer Guide

> **✨ CURRENT IMPLEMENTATION** | Updated October 18, 2025
> 
> **Key Updates:**
> - ✅ Users select billing cycle at subscription (Monthly/Quarterly/Annual)
> - ✅ Price and privileges scale to selected billing cycle
> - ✅ BillingCycleValidator ensures appropriate billing cycle for plan
> - ✅ See **CURRENT_IMPLEMENTATION_QUICK_REFERENCE.md** for formulas

---

## Table of Contents
1. [Overview](#overview)
2. [Subscription States](#subscription-states)
3. [Database Schema](#database-schema)
4. [Service Architecture](#service-architecture)
5. [Complete Workflows](#complete-workflows)
6. [Privilege Initialization](#privilege-initialization)
7. [State Transitions](#state-transitions)
8. [Code Examples](#code-examples)

---

## 1. Overview

### What is Subscription Lifecycle Management?

The Subscription Lifecycle manages the complete journey of a user's subscription from creation through renewal or cancellation. It handles all state transitions, privilege initialization, billing integration, and Stripe synchronization.

### Key Responsibilities

- ✅ **Subscription Creation**: When user purchases a plan
- ✅ **State Management**: Handle all status transitions (Active, Paused, Cancelled, etc.)
- ✅ **Privilege Initialization**: Set up privilege usage tracking
- ✅ **Billing Integration**: Create initial billing records
- ✅ **Stripe Synchronization**: Maintain consistency with Stripe subscriptions
- ✅ **Status History Tracking**: Audit all state changes

---

## 2. Subscription States

### 2.1 All Possible States

```
┌──────────────┐
│   PENDING    │  ← Subscription created, awaiting payment
└──────┬───────┘
       │ Payment successful
       ↓
┌──────────────┐
│ TRIAL_ACTIVE │  ← If plan has trial period (optional state)
└──────┬───────┘
       │ Trial ends + payment succeeds
       ↓
┌──────────────┐
│    ACTIVE    │  ← Fully active subscription (primary state)
└──────┬───────┘
       │
       ├─→ User pauses ──────→ ┌─────────┐
       │                        │ PAUSED  │ ──→ User resumes ──┐
       │                        └─────────┘                    │
       │                                                        ↓
       ├─→ Payment fails ─────→ ┌───────────────┐      ┌──────────┐
       │                         │ PAYMENT_      │ ─────┤  ACTIVE  │
       │                         │ FAILED        │      └──────────┘
       │                         └───────────────┘  (after payment fixed)
       │
       ├─→ Max retry failures ─→ ┌────────────┐
       │                          │ SUSPENDED  │
       │                          └────────────┘
       │
       ├─→ User cancels ─────────→ ┌───────────┐
       │                            │ CANCELLED │
       │                            └───────────┘
       │
       └─→ Billing date passes without renewal ─→ ┌─────────┐
                                                    │ EXPIRED │
                                                    └─────────┘
```

### 2.2 State Definitions

| State | Description | User Access | Can Transition To |
|-------|-------------|-------------|-------------------|
| **Pending** | Just created, awaiting first payment | ❌ No | Active, TrialActive, PaymentFailed |
| **TrialActive** | In trial period | ✅ Yes | Active, TrialExpired, Cancelled |
| **Active** | Fully functional subscription | ✅ Yes | Paused, PaymentFailed, Cancelled, Expired |
| **Paused** | Temporarily suspended by user | ❌ No | Active, Cancelled |
| **PaymentFailed** | Payment issue needs resolution | ⚠️ Limited | Active, Suspended, Cancelled |
| **Suspended** | Admin or system suspended | ❌ No | Active, Cancelled |
| **Cancelled** | Permanently terminated | ❌ No | None (terminal state) |
| **Expired** | Reached end without renewal | ❌ No | Active (if reactivated) |
| **TrialExpired** | Trial ended, needs payment | ❌ No | Active, Cancelled |

---

## 3. Database Schema

### 3.1 Table: Subscriptions (Core Entity)

| Column | Type | Description | Example |
|--------|------|-------------|---------|
| Id | UNIQUEIDENTIFIER | Primary key | sub_111 |
| UserId | INT | FK to Users | 456 |
| SubscriptionPlanId | UNIQUEIDENTIFIER | FK to Plans | f3a1b2c3-... |
| BillingCycleId | UNIQUEIDENTIFIER | FK to Billing Cycles | monthly-guid |
| Status | NVARCHAR(50) | Current state | "Active" |
| StartDate | DATETIME2 | Subscription start | 2025-10-17 |
| EndDate | DATETIME2 | Subscription end | 2025-11-17 |
| NextBillingDate | DATETIME2 | Next billing date | 2025-11-17 |
| CurrentPrice | DECIMAL(18,2) | Current plan price | 275.00 |
| AutoRenew | BIT | Auto-renewal flag | 1 (true) |
| StripeCustomerId | NVARCHAR(255) | Stripe customer link | "cus_XYZ789" |
| StripeSubscriptionId | NVARCHAR(255) | Stripe subscription link | "sub_stripe_AAA" |
| IsTrialSubscription | BIT | Trial flag | 0 (false) |
| TrialStartDate | DATETIME2 | Trial start (if trial) | NULL |
| TrialEndDate | DATETIME2 | Trial end (if trial) | NULL |
| CancelledDate | DATETIME2 | Cancellation date | NULL |
| CancellationReason | NVARCHAR(MAX) | Why cancelled | NULL |
| FailedPaymentAttempts | INT | Failed payment count | 0 |
| LastPaymentFailedDate | DATETIME2 | Last failure date | NULL |
| LastPaymentError | NVARCHAR(MAX) | Error message | NULL |

### 3.2 Table: SubscriptionStatusHistory (Audit Trail)

| Column | Type | Description | Example |
|--------|------|-------------|---------|
| Id | UNIQUEIDENTIFIER | Primary key | history-123 |
| SubscriptionId | UNIQUEIDENTIFIER | FK to Subscription | sub_111 |
| FromStatus | NVARCHAR(50) | Old status | "Pending" |
| ToStatus | NVARCHAR(50) | New status | "Active" |
| Reason | NVARCHAR(MAX) | Change reason | "Payment successful" |
| ChangedByUserId | INT | Who made change | 456 or NULL (system) |
| ChangedAt | DATETIME2 | When changed | 2025-10-17 10:30:00 |

### 3.3 Table: UserSubscriptionPrivilegeUsage (Usage Tracking)

| Column | Type | Description | Example |
|--------|------|-------------|---------|
| Id | UNIQUEIDENTIFIER | Primary key | usage-123 |
| SubscriptionId | UNIQUEIDENTIFIER | FK to Subscription | sub_111 |
| PrivilegeId | UNIQUEIDENTIFIER | FK to Privilege | telecon-guid |
| SubscriptionPlanPrivilegeId | UNIQUEIDENTIFIER | FK to plan privilege config | plan-priv-guid |
| AllocatedLimit | INT | Total allowed | 5 |
| UsedValue | INT | How many used | 3 |
| AllowedValue | INT | Remaining | 2 (calculated: 5-3) |
| UsagePeriodStart | DATETIME2 | Period start | 2025-10-17 |
| UsagePeriodEnd | DATETIME2 | Period end | 2025-11-17 |
| LastUsedAt | DATETIME2 | Last usage time | 2025-10-20 |
| ResetAt | DATETIME2 | Last reset time | 2025-10-17 |

---

## 4. Service Architecture

### 4.1 Primary Services

#### **SubscriptionLifecycleService**
**Location:** `SmartTelehealth.Application/Services/SubscriptionLifecycleService.cs`

**Responsibilities:**
- Create subscriptions
- Cancel/pause/resume subscriptions
- Process state transitions
- Handle trial conversions
- Manage expiration

**Key Dependencies:**
```csharp
ISubscriptionRepository _subscriptionRepository
ISubscriptionStatusHistoryRepository _statusHistoryRepository
ISubscriptionPlanRepository _subscriptionPlanRepository
IStripeService _stripeService
ISubscriptionBillingService _billingService
IPrivilegeService _privilegeService
IUserSubscriptionPrivilegeUsageRepository _usageRepo
IUnitOfWork _unitOfWork
```

#### **SubscriptionService**
**Location:** `SmartTelehealth.Application/Services/SubscriptionService.cs`

**Responsibilities:**
- CRUD operations for subscriptions
- Filtering and querying
- User subscription management
- Additional credit purchases (overage)

---

## 5. Complete Workflows

### 5.1 Workflow: User Subscribes to Plan

```
┌─────────────────────────────────────────────────┐
│ USER ACTION: Subscribe to Plan                   │
│ Endpoint: POST /api/subscriptions                │
└─────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────┐
│ SubscriptionsController                          │
│ Method: CreateSubscription()                     │
└─────────────────────────────────────────────────┘
                    ↓
        Calls SubscriptionLifecycleService
                    ↓
┌─────────────────────────────────────────────────┐
│ SubscriptionLifecycleService                     │
│ Method: CreateSubscriptionAsync()                │
│                                                  │
│ [STEP 1] Validate Plan Exists                   │
│   plan = await _planRepo.GetByIdAsync(planId)   │
│   if (plan == null) → 404 Not Found             │
│   if (!plan.IsActive) → 400 Plan Inactive       │
│                                                  │
│ [STEP 2] Check for Duplicate Subscriptions      │
│   existing = await _repo                        │
│     .GetActiveSubscriptionByUserAndPlanAsync(   │
│       userId, planId                            │
│     )                                           │
│   if (existing != null) → 400 Already Subscribed│
│                                                  │
│ [STEP 3] Ensure Stripe Customer Exists          │
│   user = await _userService.GetUserByIdAsync()  │
│   if (user.StripeCustomerId == null) {          │
│     stripeCustomerId = await _stripeService     │
│       .CreateCustomerAsync(                     │
│         email: user.Email,                      │
│         name: user.FullName                     │
│       )                                         │
│     user.StripeCustomerId = stripeCustomerId    │
│     await _userRepo.UpdateAsync(user)           │
│   }                                             │
│                                                  │
│ [STEP 4] Attach Payment Method (if provided)    │
│   if (paymentMethodId != null) {                │
│     await _stripeService.AttachPaymentMethod(   │
│       paymentMethodId, stripeCustomerId         │
│     )                                           │
│   }                                             │
│                                                  │
│ [STEP 5] BEGIN TRANSACTION                      │
│   await _unitOfWork.BeginTransactionAsync()     │
│                                                  │
│ [STEP 6] Create Subscription Entity             │
│   subscription = new Subscription {             │
│     Id = Guid.NewGuid(),                        │
│     UserId = userId,                            │
│     SubscriptionPlanId = planId,                │
│     BillingCycleId = billingCycleId,            │
│     Status = "Pending",                         │
│     StartDate = DateTime.UtcNow,                │
│     EndDate = CalculateEndDate(),               │
│     NextBillingDate = CalculateNextBilling(),   │
│     CurrentPrice = plan.Price,                  │
│     AutoRenew = true,                           │
│     StripeCustomerId = stripeCustomerId,        │
│     IsTrialSubscription = plan.IsTrialAllowed,  │
│     TrialStartDate = plan.IsTrialAllowed        │
│       ? DateTime.UtcNow : null,                 │
│     TrialEndDate = plan.IsTrialAllowed          │
│       ? DateTime.UtcNow.AddDays(                │
│           plan.TrialDurationInDays) : null      │
│   };                                            │
│                                                  │
│   created = await _repo.CreateAsync(subscription)│
│                                                  │
│ [STEP 7] Create Stripe Subscription             │
│   stripeSubId = await _stripeService            │
│     .CreateSubscriptionAsync(                   │
│       customerId: stripeCustomerId,             │
│       priceId: plan.StripeMonthlyPriceId,       │
│       trialEnd: subscription.TrialEndDate,      │
│       metadata: {                               │
│         subscriptionId: created.Id,             │
│         planId: planId                          │
│       }                                         │
│     )                                           │
│                                                  │
│   subscription.StripeSubscriptionId = stripeSubId│
│   await _repo.UpdateAsync(subscription)         │
│                                                  │
│ [STEP 8] Initialize Privilege Usage             │
│   await InitializePrivilegeUsageAsync(          │
│     subscription, plan                          │
│   )                                             │
│   → See Section 6 for details                   │
│                                                  │
│ [STEP 9] Create Status History Record           │
│   await RecordStatusChangeAsync(                │
│     subscriptionId: created.Id,                 │
│     oldStatus: null,                            │
│     newStatus: "Pending",                       │
│     reason: "Subscription created"              │
│   )                                             │
│                                                  │
│ [STEP 10] Create Initial Billing Record         │
│   await _billingService                         │
│     .CreateSubscriptionBillingAsync(            │
│       subscription: created,                    │
│       amount: plan.Price,                       │
│       description: "Initial billing",           │
│       dueDate: DateTime.UtcNow                  │
│     )                                           │
│                                                  │
│ [STEP 11] COMMIT TRANSACTION                    │
│   await _unitOfWork.CommitTransactionAsync()    │
│                                                  │
│ [STEP 12] Send Notifications                    │
│   if (plan.IsTrialAllowed) {                    │
│     await _notificationService                  │
│       .SendTrialStartedNotification()           │
│   } else {                                      │
│     await _notificationService                  │
│       .SendSubscriptionConfirmation()           │
│   }                                             │
│                                                  │
│ [STEP 13] Return Success                        │
│   return JsonModel {                            │
│     data = subscriptionDto,                     │
│     Message = "Subscription created",           │
│     StatusCode = 201                            │
│   }                                             │
└─────────────────────────────────────────────────┘
```

#### **After Stripe Processes Payment**

```
┌─────────────────────────────────────────────────┐
│ STRIPE AUTOMATIC PAYMENT                         │
│                                                  │
│ Stripe detects new subscription                 │
│   ↓                                             │
│ Creates invoice for first billing               │
│   ↓                                             │
│ Charges payment method                          │
│   ↓                                             │
│ If successful:                                  │
│   Sends webhook: "invoice.payment_succeeded"    │
│                                                  │
│ If failed:                                      │
│   Sends webhook: "invoice.payment_failed"       │
└─────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────┐
│ YOUR SYSTEM (Webhook Handler)                   │
│ File: StripeWebhookController.cs                │
│                                                  │
│ [1] Validate webhook signature                  │
│ [2] Check idempotency (prevent duplicates)      │
│ [3] Extract subscription ID from metadata       │
│ [4] Find local subscription by ID               │
│                                                  │
│ If Payment Successful:                          │
│   subscription.Status = "Active"                │
│   billingRecord.Status = "Paid"                 │
│   await RecordStatusChange(                     │
│     "Pending" → "Active",                       │
│     reason: "Payment successful"                │
│   )                                             │
│   Send confirmation email                       │
│                                                  │
│ If Payment Failed:                              │
│   subscription.Status = "PaymentFailed"         │
│   subscription.FailedPaymentAttempts++          │
│   Send payment failure notification             │
│   Schedule retry                                │
└─────────────────────────────────────────────────┘
```

---

## 6. Privilege Initialization

### 6.1 How Privileges are Set Up

When a subscription is created, the system initializes privilege usage tracking:

```csharp
private async Task InitializePrivilegeUsageAsync(
    Subscription subscription, 
    SubscriptionPlan plan)
{
    // Get all privileges defined in the plan
    var planPrivileges = await _planPrivilegeRepo
        .GetByPlanIdAsync(plan.Id);
    
    foreach (var planPrivilege in planPrivileges)
    {
        // Create usage tracking record for each privilege
        var usage = new UserSubscriptionPrivilegeUsage
        {
            Id = Guid.NewGuid(),
            SubscriptionId = subscription.Id,
            PrivilegeId = planPrivilege.PrivilegeId,
            SubscriptionPlanPrivilegeId = planPrivilege.Id,
            
            // Initialize limits from plan configuration
            AllocatedLimit = planPrivilege.Value,  // e.g., 5 consultations
            UsedValue = 0,  // Start at zero
            AllowedValue = planPrivilege.Value,  // Initially same as allocated
            
            // Set usage period
            UsagePeriodStart = subscription.StartDate,
            UsagePeriodEnd = subscription.EndDate,
            
            // Reset tracking
            ResetAt = subscription.StartDate,
            LastUsedAt = null,
            
            // Audit fields
            CreatedBy = subscription.UserId,
            CreatedDate = DateTime.UtcNow
        };
        
        await _usageRepo.CreateAsync(usage);
    }
    
    _logger.LogInformation(
        "Initialized {Count} privilege usage records for subscription {SubId}",
        planPrivileges.Count(),
        subscription.Id
    );
}
```

### 6.2 Privilege Usage State After Initialization

```
SUBSCRIPTION: sub_111
PLAN: Basic Health Plan
USER: John Doe (ID: 456)

UserSubscriptionPrivilegeUsage Records Created:

┌──────────────────────────────────────────────────┐
│ [Record 1] - Teleconsultation                    │
├──────────────────────────────────────────────────┤
│ SubscriptionId: sub_111                          │
│ PrivilegeId: teleconsultation-guid               │
│ AllocatedLimit: 5        ← From plan config      │
│ UsedValue: 0             ← Not used yet          │
│ AllowedValue: 5          ← Can use 5             │
│ UsagePeriodStart: 2025-10-17                     │
│ UsagePeriodEnd: 2025-11-17                       │
│ ResetAt: 2025-10-17                              │
│ LastUsedAt: NULL         ← Never used            │
└──────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────┐
│ [Record 2] - Medication Refill                   │
├──────────────────────────────────────────────────┤
│ SubscriptionId: sub_111                          │
│ PrivilegeId: medication-guid                     │
│ AllocatedLimit: 3        ← From plan config      │
│ UsedValue: 0             ← Not used yet          │
│ AllowedValue: 3          ← Can use 3             │
│ UsagePeriodStart: 2025-10-17                     │
│ UsagePeriodEnd: 2025-11-17                       │
│ ResetAt: 2025-10-17                              │
│ LastUsedAt: NULL         ← Never used            │
└──────────────────────────────────────────────────┘
```

---

## 7. State Transitions

### 7.1 Valid State Transition Rules

**Implemented in:** `SubscriptionLifecycleService.ValidateStateTransition()`

```csharp
private static readonly Dictionary<string, HashSet<string>> ValidTransitions = new()
{
    [Subscription.SubscriptionStatuses.Pending] = new HashSet<string>
    {
        Subscription.SubscriptionStatuses.Active,
        Subscription.SubscriptionStatuses.TrialActive,
        Subscription.SubscriptionStatuses.PaymentFailed,
        Subscription.SubscriptionStatuses.Cancelled
    },
    
    [Subscription.SubscriptionStatuses.Active] = new HashSet<string>
    {
        Subscription.SubscriptionStatuses.Paused,
        Subscription.SubscriptionStatuses.PaymentFailed,
        Subscription.SubscriptionStatuses.Cancelled,
        Subscription.SubscriptionStatuses.Expired
    },
    
    [Subscription.SubscriptionStatuses.Paused] = new HashSet<string>
    {
        Subscription.SubscriptionStatuses.Active,
        Subscription.SubscriptionStatuses.Cancelled
    },
    
    [Subscription.SubscriptionStatuses.PaymentFailed] = new HashSet<string>
    {
        Subscription.SubscriptionStatuses.Active,  // After payment fixed
        Subscription.SubscriptionStatuses.Suspended,  // After max retries
        Subscription.SubscriptionStatuses.Cancelled
    },
    
    [Subscription.SubscriptionStatuses.Suspended] = new HashSet<string>
    {
        Subscription.SubscriptionStatuses.Active,  // Admin reactivation
        Subscription.SubscriptionStatuses.Cancelled
    },
    
    [Subscription.SubscriptionStatuses.TrialActive] = new HashSet<string>
    {
        Subscription.SubscriptionStatuses.Active,
        Subscription.SubscriptionStatuses.TrialExpired,
        Subscription.SubscriptionStatuses.Cancelled
    },
    
    [Subscription.SubscriptionStatuses.TrialExpired] = new HashSet<string>
    {
        Subscription.SubscriptionStatuses.Active,
        Subscription.SubscriptionStatuses.Cancelled
    },
    
    [Subscription.SubscriptionStatuses.Cancelled] = new HashSet<string>
    {
        // No transitions from Cancelled (terminal state)
    },
    
    [Subscription.SubscriptionStatuses.Expired] = new HashSet<string>
    {
        Subscription.SubscriptionStatuses.Active  // Reactivation
    }
};
```

### 7.2 Processing State Transitions

```csharp
public async Task<JsonModel> ProcessStateTransitionAsync(
    string subscriptionId,
    string newStatus,
    string reason,
    string changedByUserId = null)
{
    var subscription = await _subscriptionRepository
        .GetByIdWithDetailsAsync(Guid.Parse(subscriptionId));
    
    if (subscription == null)
        return new JsonModel { StatusCode = 404, Message = "Not found" };
    
    var oldStatus = subscription.Status;
    
    // Validate transition is allowed
    var validationResult = ValidateStateTransition(oldStatus, newStatus);
    if (!validationResult.IsValid)
        return new JsonModel { 
            StatusCode = 400, 
            Message = validationResult.ErrorMessage 
        };
    
    // Update subscription status
    subscription.Status = newStatus;
    subscription.UpdatedBy = !string.IsNullOrEmpty(changedByUserId) 
        ? int.Parse(changedByUserId) 
        : null;
    subscription.UpdatedDate = DateTime.UtcNow;
    
    // Update status-specific properties
    await UpdateStatusSpecificPropertiesAsync(subscription, newStatus, reason);
    
    // Record status change in history
    await _statusHistoryRepository.CreateAsync(new SubscriptionStatusHistory
    {
        SubscriptionId = subscription.Id,
        FromStatus = oldStatus,
        ToStatus = newStatus,
        Reason = reason,
        ChangedByUserId = !string.IsNullOrEmpty(changedByUserId) 
            ? int.Parse(changedByUserId) 
            : null,
        ChangedAt = DateTime.UtcNow
    });
    
    // Save changes
    await _subscriptionRepository.UpdateAsync(subscription);
    
    _logger.LogInformation(
        "Subscription {SubId} transitioned from {Old} to {New}: {Reason}",
        subscriptionId, oldStatus, newStatus, reason
    );
    
    return new JsonModel { 
        data = true, 
        Message = "Status updated", 
        StatusCode = 200 
    };
}
```

---

## 8. Code Examples

### 8.1 Complete Subscription Creation (Simplified)

```csharp
public async Task<JsonModel> CreateSubscriptionAsync(
    CreateSubscriptionDto createDto, 
    TokenModel tokenModel)
{
    // Validate plan
    var plan = await _subscriptionPlanRepository
        .GetByIdWithDetailsAsync(createDto.PlanId);
    
    if (plan == null || !plan.IsActive)
        return new JsonModel { StatusCode = 400, Message = "Invalid plan" };
    
    // Ensure Stripe customer exists
    var user = await _userService.GetUserByIdAsync(createDto.UserId, tokenModel);
    if (user.data is not UserDto userDto)
        return new JsonModel { StatusCode = 404, Message = "User not found" };
    
    string stripeCustomerId = userDto.StripeCustomerId;
    
    if (string.IsNullOrEmpty(stripeCustomerId))
    {
        stripeCustomerId = await _stripeService.CreateCustomerAsync(
            userDto.Email, 
            userDto.FullName, 
            tokenModel
        );
        
        await _userService.UpdateStripeCustomerIdAsync(
            createDto.UserId, 
            stripeCustomerId
        );
    }
    
    // Begin transaction
    await _unitOfWork.BeginTransactionAsync();
    string stripeSubscriptionId = null;
    
    try
    {
        // Create subscription entity
        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            UserId = createDto.UserId,
            SubscriptionPlanId = createDto.PlanId,
            BillingCycleId = createDto.BillingCycleId,
            Status = Subscription.SubscriptionStatuses.Pending,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(1),
            NextBillingDate = DateTime.UtcNow.AddMonths(1),
            CurrentPrice = plan.Price,
            AutoRenew = true,
            StripeCustomerId = stripeCustomerId,
            CreatedBy = createDto.UserId,
            CreatedDate = DateTime.UtcNow
        };
        
        var created = await _subscriptionRepository.CreateAsync(subscription);
        
        // Create Stripe subscription
        stripeSubscriptionId = await _stripeService.CreateSubscriptionAsync(
            customerId: stripeCustomerId,
            priceId: plan.StripeMonthlyPriceId,
            metadata: new Dictionary<string, string>
            {
                { "subscriptionId", created.Id.ToString() },
                { "planId", plan.Id.ToString() }
            },
            tokenModel: tokenModel
        );
        
        created.StripeSubscriptionId = stripeSubscriptionId;
        await _subscriptionRepository.UpdateAsync(created);
        
        // Initialize privilege usage
        await InitializePrivilegeUsageAsync(created, plan);
        
        // Record status history
        await RecordStatusChangeAsync(
            created.Id, 
            null, 
            Subscription.SubscriptionStatuses.Pending,
            "Subscription created",
            tokenModel
        );
        
        // Create initial billing record
        await _billingService.CreateSubscriptionBillingAsync(
            created, 
            plan.Price,
            $"Initial billing for {plan.Name}",
            DateTime.UtcNow,
            tokenModel
        );
        
        // Commit transaction
        await _unitOfWork.CommitTransactionAsync();
        
        _logger.LogInformation(
            "Created subscription {SubId} for user {UserId}",
            created.Id, createDto.UserId
        );
        
        var subscriptionDto = _mapper.Map<SubscriptionDto>(created);
        return new JsonModel { 
            data = subscriptionDto, 
            Message = "Subscription created", 
            StatusCode = 201 
        };
    }
    catch (Exception ex)
    {
        // Rollback transaction
        await _unitOfWork.RollbackTransactionAsync();
        
        // Cleanup Stripe subscription if created
        if (!string.IsNullOrEmpty(stripeSubscriptionId))
        {
            try
            {
                await _stripeService.CancelSubscriptionAsync(
                    stripeSubscriptionId, 
                    tokenModel
                );
            }
            catch (Exception cleanupEx)
            {
                _logger.LogError(cleanupEx, "Failed to cleanup Stripe subscription");
            }
        }
        
        _logger.LogError(ex, "Failed to create subscription");
        return new JsonModel { 
            StatusCode = 500, 
            Message = $"Failed: {ex.Message}" 
        };
    }
}
```

### 8.2 Cancelling a Subscription

```csharp
public async Task<JsonModel> CancelSubscriptionAsync(
    string subscriptionId, 
    string reason, 
    TokenModel tokenModel)
{
    var subscription = await _subscriptionRepository
        .GetByIdWithDetailsAsync(Guid.Parse(subscriptionId));
    
    if (subscription == null)
        return new JsonModel { StatusCode = 404, Message = "Not found" };
    
    // Validate user owns subscription or is admin
    if (tokenModel.RoleID != (int)RoleId.Admin && 
        tokenModel.UserID != subscription.UserId)
    {
        return new JsonModel { StatusCode = 403, Message = "Access denied" };
    }
    
    var oldStatus = subscription.Status;
    
    // Cancel in Stripe first
    if (!string.IsNullOrEmpty(subscription.StripeSubscriptionId))
    {
        await _stripeService.CancelSubscriptionAsync(
            subscription.StripeSubscriptionId, 
            tokenModel
        );
    }
    
    // Begin transaction
    await _unitOfWork.BeginTransactionAsync();
    
    try
    {
        // Update subscription
        subscription.Status = Subscription.SubscriptionStatuses.Cancelled;
        subscription.CancelledDate = DateTime.UtcNow;
        subscription.CancellationReason = reason;
        subscription.AutoRenew = false;
        subscription.UpdatedBy = tokenModel.UserID;
        subscription.UpdatedDate = DateTime.UtcNow;
        
        await _subscriptionRepository.UpdateAsync(subscription);
        
        // Record status change
        await RecordStatusChangeAsync(
            subscription.Id,
            oldStatus,
            Subscription.SubscriptionStatuses.Cancelled,
            reason,
            tokenModel
        );
        
        // Commit transaction
        await _unitOfWork.CommitTransactionAsync();
        
        // Send notification
        await _notificationService.SendSubscriptionCancelledNotificationAsync(
            userEmail: subscription.User.Email,
            userName: subscription.User.FullName,
            subscriptionDto: _mapper.Map<SubscriptionDto>(subscription),
            tokenModel: tokenModel
        );
        
        _logger.LogInformation(
            "Cancelled subscription {SubId}. Reason: {Reason}",
            subscriptionId, reason
        );
        
        return new JsonModel { 
            data = _mapper.Map<SubscriptionDto>(subscription),
            Message = "Subscription cancelled", 
            StatusCode = 200 
        };
    }
    catch (Exception ex)
    {
        await _unitOfWork.RollbackTransactionAsync();
        _logger.LogError(ex, "Failed to cancel subscription {SubId}", subscriptionId);
        return new JsonModel { 
            StatusCode = 500, 
            Message = $"Failed: {ex.Message}" 
        };
    }
}
```

---

## 9. Key Takeaways

### ✅ Critical Points

1. **Always use transactions** when creating/modifying subscriptions
2. **Always sync with Stripe** - create Stripe subscription alongside DB record
3. **Initialize privileges** immediately after subscription creation
4. **Track all status changes** in SubscriptionStatusHistory
5. **Validate state transitions** before allowing changes
6. **Clean up Stripe resources** if transaction fails

### 🔍 Common Scenarios

| Scenario | Initial State | Final State | Trigger |
|----------|---------------|-------------|---------|
| User subscribes | N/A | Pending | User action |
| Payment succeeds | Pending | Active | Stripe webhook |
| Payment fails | Pending | PaymentFailed | Stripe webhook |
| User pauses | Active | Paused | User action |
| User resumes | Paused | Active | User action |
| Payment retry succeeds | PaymentFailed | Active | Stripe webhook |
| Max retries reached | PaymentFailed | Suspended | System automation |
| User cancels | Active/Paused | Cancelled | User action |
| Period ends | Active | Expired | System automation |

---

## Next Steps

Continue to:
- **Guide 03**: Billing and Payment Processing
- **Guide 04**: Privilege Usage and Tracking
- **Guide 05**: Stripe Integration Deep Dive

---

**Document Version:** 1.0  
**Last Updated:** October 17, 2025



