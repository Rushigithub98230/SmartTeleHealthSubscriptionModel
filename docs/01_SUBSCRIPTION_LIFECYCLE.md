# Subscription Lifecycle Management

## Table of Contents
1. [Overview](#overview)
2. [Subscription States](#subscription-states)
3. [Creation Flow](#creation-flow)
4. [Activation Flow](#activation-flow)
5. [Pause Flow](#pause-flow)
6. [Resume Flow](#resume-flow)
7. [Cancellation Flow](#cancellation-flow)
8. [Status Transitions](#status-transitions)
9. [Database Operations](#database-operations)

---

## Overview

The subscription lifecycle represents the complete journey of a user subscription from creation to termination, including all state transitions, billing events, and privilege management.

### Key Services Involved
- **SubscriptionLifecycleService** - Lifecycle operations
- **SubscriptionService** - Basic operations
- **SubscriptionBillingService** - Billing operations
- **PaymentService** - Payment processing
- **PrivilegeService** - Privilege allocation
- **StripeService** - Stripe integration

---

## Subscription States

### Status Constants
Defined in `Subscription.SubscriptionStatuses`:

```csharp
- Pending          // Created but not yet activated
- Active           // Fully active with services
- Paused           // Temporarily suspended
- Cancelled        // Terminated by user/admin
- Expired          // Reached natural end
- PaymentFailed    // Payment issues
- TrialActive      // In trial period
- TrialExpired     // Trial ended, needs conversion
- Suspended        // Admin suspended
```

### Valid State Transitions

```
Pending → [Active, TrialActive, Cancelled]
Active → [Paused, Cancelled, Expired, PaymentFailed]
Paused → [Active, Cancelled, Expired]
TrialActive → [Active, TrialExpired, Cancelled]
TrialExpired → [Active, Cancelled]
PaymentFailed → [Active, Cancelled, Expired]
Expired → [Active] (renewal)
Cancelled → [] (terminal state)
```

---

## Creation Flow

### Step-by-Step Process

#### 1. User Initiates Subscription

**Frontend Request**:
```typescript
POST /api/subscriptions
{
  "userId": 123,
  "planId": "guid",
  "paymentMethodId": "pm_xxx",
  "isTrialSubscription": false
}
```

#### 2. Backend Validation

**Service**: `SubscriptionLifecycleService.CreateSubscriptionAsync()`

**Validation Steps**:
```csharp
// Step 1: Validate plan exists and is active
var requestedPlan = await _subscriptionPlanRepository.GetSubscriptionPlanByIdAsync(planId);
if (requestedPlan == null) return NotFound();

// Step 2: Enforce latest version for new subscriptions
if (!requestedPlan.IsLatestVersion) {
    var parentPlanId = requestedPlan.ParentPlanId ?? requestedPlan.Id;
    var allVersions = await _subscriptionPlanRepository.GetAllVersionsOfPlanAsync(parentPlanId);
    var latestVersion = allVersions.FirstOrDefault(v => v.IsLatestVersion && v.IsActive);
    plan = latestVersion;  // Use latest version
}

// Step 3: Prevent duplicate active subscriptions
var userSubscriptions = await _subscriptionRepository.GetByUserIdAsync(userId);
if (userSubscriptions.Any(s => s.SubscriptionPlanId == plan.Id && 
    (s.Status == "Active" || s.Status == "Paused"))) {
    return BadRequest("User already has active subscription");
}

// Step 4: Ensure Stripe Customer exists
stripeCustomerId = await EnsureStripeCustomerAsync(user, tokenModel);

// Step 5: Validate Payment Method
if (!string.IsNullOrEmpty(paymentMethodId)) {
    var isValid = await _stripeService.ValidatePaymentMethodAsync(paymentMethodId);
    if (!isValid) return BadRequest("Invalid payment method");
}
```

#### 3. Create Subscription Entity

```csharp
var subscription = new Subscription
{
    Id = Guid.NewGuid(),
    UserId = userId,
    SubscriptionPlanId = plan.Id,
    Status = isTrialSubscription ? 
        SubscriptionStatuses.TrialActive : 
        SubscriptionStatuses.Pending,
    
    // Dates
    StartDate = DateTime.UtcNow,
    NextBillingDate = CalculateNextBillingDate(plan.BillingCycle),
    
    // Trial setup
    IsTrialSubscription = isTrialSubscription,
    TrialStartDate = isTrialSubscription ? DateTime.UtcNow : null,
    TrialEndDate = isTrialSubscription ? 
        DateTime.UtcNow.AddDays(plan.TrialDurationInDays) : null,
    
    // Pricing
    CurrentPrice = plan.EffectivePrice,
    
    // Stripe
    StripeCustomerId = stripeCustomerId,
    PaymentMethodId = paymentMethodId,
    
    // Audit
    CreatedBy = tokenModel.UserID,
    CreatedDate = DateTime.UtcNow,
    IsActive = true
};
```

#### 4. Create Stripe Subscription

**If not trial**:
```csharp
var stripeSubscription = await _stripeService.CreateSubscriptionAsync(
    stripeCustomerId,
    plan.StripePriceId,
    tokenModel
);

subscription.StripeSubscriptionId = stripeSubscription.Id;
```

#### 5. Save to Database

```csharp
await _unitOfWork.BeginTransactionAsync();
try {
    var createdSubscription = await _subscriptionRepository.AddAsync(subscription);
    await _subscriptionRepository.SaveChangesAsync();
    
    await _unitOfWork.CommitTransactionAsync();
} catch {
    // Rollback Stripe if database fails
    if (!string.IsNullOrEmpty(subscription.StripeSubscriptionId)) {
        await _stripeService.CancelSubscriptionAsync(
            subscription.StripeSubscriptionId, tokenModel);
    }
    await _unitOfWork.RollbackTransactionAsync();
    throw;
}
```

#### 6. Allocate Privileges

**Service**: `PrivilegeService`

```csharp
// Get plan privileges
var planPrivileges = await _planPrivilegeRepo.GetByPlanIdAsync(subscription.SubscriptionPlanId);

foreach (var planPrivilege in planPrivileges) {
    var usage = new UserSubscriptionPrivilegeUsage {
        Id = Guid.NewGuid(),
        SubscriptionId = subscription.Id,
        SubscriptionPlanPrivilegeId = planPrivilege.Id,
        PrivilegeId = planPrivilege.PrivilegeId,
        
        // Allocate based on plan value
        AllowedValue = planPrivilege.Value, // -1 = unlimited, 0 = disabled, >0 = limited
        UsedValue = 0,
        
        // Set usage period (aligned with billing cycle)
        UsagePeriodStart = subscription.StartDate,
        UsagePeriodEnd = subscription.NextBillingDate,
        
        CreatedBy = tokenModel.UserID,
        CreatedDate = DateTime.UtcNow
    };
    
    await _usageRepo.AddAsync(usage);
}
await _usageRepo.SaveChangesAsync();
```

#### 7. Create Initial Billing Record

**For non-trial subscriptions**:
```csharp
var billingRecord = new BillingRecord {
    Id = Guid.NewGuid(),
    UserId = subscription.UserId,
    SubscriptionId = subscription.Id,
    CurrencyId = plan.CurrencyId,
    
    // Amounts
    Amount = plan.EffectivePrice,
    TaxAmount = 0,
    TotalAmount = plan.EffectivePrice,
    
    // Status
    Status = BillingStatus.Pending,
    Type = BillingType.Subscription,
    
    // Dates
    BillingDate = DateTime.UtcNow,
    DueDate = DateTime.UtcNow.AddDays(7),
    
    // Description
    Description = $"Subscription to {plan.Name}",
    
    // Stripe
    StripeInvoiceId = null, // Set when payment processed
    
    // Audit
    CreatedBy = tokenModel.UserID,
    CreatedDate = DateTime.UtcNow
};

await _billingRepository.AddAsync(billingRecord);
```

#### 8. Process Initial Payment

**Service**: `PaymentService.ProcessPaymentAsync()`

```csharp
// Create SubscriptionPayment
var payment = new SubscriptionPayment {
    Id = Guid.NewGuid(),
    SubscriptionId = subscription.Id,
    BillingRecordId = billingRecord.Id,
    CurrencyId = plan.CurrencyId,
    
    Amount = billingRecord.TotalAmount,
    NetAmount = billingRecord.TotalAmount,
    
    Status = PaymentStatus.Pending,
    Type = PaymentType.Subscription,
    
    DueDate = billingRecord.DueDate,
    BillingPeriodStart = subscription.StartDate,
    BillingPeriodEnd = subscription.NextBillingDate,
    
    Description = billingRecord.Description
};

await _subscriptionPaymentRepository.AddAsync(payment);

// Process through Stripe
var result = await _stripeBillingService.ProcessStripePaymentAsync(billingRecord.Id, tokenModel);

if (result.StatusCode == 200) {
    // Update payment status
    payment.Status = PaymentStatus.Succeeded;
    payment.PaidAt = DateTime.UtcNow;
    
    // Update billing record
    billingRecord.Status = BillingStatus.Paid;
    billingRecord.PaidAt = DateTime.UtcNow;
    
    // Activate subscription
    subscription.Status = SubscriptionStatuses.Active;
    subscription.LastBillingDate = DateTime.UtcNow;
}
```

#### 9. Status History Record

```csharp
var statusHistory = new SubscriptionStatusHistory {
    Id = Guid.NewGuid(),
    SubscriptionId = subscription.Id,
    FromStatus = null,
    ToStatus = subscription.Status,
    Reason = "Initial subscription creation",
    ChangedAt = DateTime.UtcNow,
    ChangedByUserId = tokenModel.UserID
};

await _statusHistoryRepository.AddAsync(statusHistory);
```

#### 10. Send Notifications

```csharp
// Welcome email
await _subscriptionNotificationService.SendSubscriptionCreatedNotificationAsync(
    subscription.Id, tokenModel);

// Send invoice
if (billingRecord.Status == BillingStatus.Paid) {
    await _invoiceService.SendInvoiceEmailAsync(billingRecord.Id, tokenModel);
}
```

### Creation Flow Diagram

```
User Action
    │
    ▼
┌─────────────────────────┐
│ Validate Plan & User    │
│ - Check plan exists     │
│ - Enforce latest version│
│ - Check duplicates      │
└────────┬────────────────┘
         │
         ▼
┌─────────────────────────┐
│ Create Stripe Customer  │
│ - Ensure exists         │
│ - Link payment method   │
└────────┬────────────────┘
         │
         ▼
┌─────────────────────────┐
│ Create Subscription     │
│ - Set status (Pending)  │
│ - Calculate dates       │
│ - Set pricing           │
└────────┬────────────────┘
         │
         ▼
┌─────────────────────────┐
│ Create Stripe Subscription
│ - Link to Stripe        │
│ - Store IDs             │
└────────┬────────────────┘
         │
         ▼
┌─────────────────────────┐
│ Save to Database        │
│ (Transaction)           │
└────────┬────────────────┘
         │
         ▼
┌─────────────────────────┐
│ Allocate Privileges     │
│ - For each plan privilege│
│ - Create usage tracker  │
└────────┬────────────────┘
         │
         ▼
┌─────────────────────────┐
│ Create Billing Record   │
│ - Initial subscription  │
│ - Status: Pending       │
└────────┬────────────────┘
         │
         ▼
┌─────────────────────────┐
│ Process Payment         │
│ - Create payment record │
│ - Charge via Stripe     │
│ - Update statuses       │
└────────┬────────────────┘
         │
         ▼
┌─────────────────────────┐
│ Activate Subscription   │
│ - Status: Active        │
│ - Record history        │
└────────┬────────────────┘
         │
         ▼
┌─────────────────────────┐
│ Send Notifications      │
│ - Welcome email         │
│ - Invoice               │
└─────────────────────────┘
```

---

## Activation Flow

Subscriptions move from `Pending` or `TrialExpired` to `Active`.

### Triggers
1. **Initial payment success** (during creation)
2. **Trial conversion** (trial ends, payment succeeds)
3. **Manual activation** (admin action)

### Process

```csharp
public async Task<JsonModel> ActivateSubscriptionAsync(string subscriptionId, TokenModel tokenModel)
{
    var subscription = await _subscriptionRepository.GetByIdWithDetailsAsync(Guid.Parse(subscriptionId));
    
    // Validation
    if (subscription.Status != SubscriptionStatuses.Pending && 
        subscription.Status != SubscriptionStatuses.TrialExpired) {
        return BadRequest("Cannot activate from current status");
    }
    
    // Update status
    subscription.Status = SubscriptionStatuses.Active;
    subscription.UpdatedBy = tokenModel.UserID;
    subscription.UpdatedDate = DateTime.UtcNow;
    
    // Record history
    var statusHistory = new SubscriptionStatusHistory {
        SubscriptionId = subscription.Id,
        FromStatus = oldStatus,
        ToStatus = SubscriptionStatuses.Active,
        Reason = "Subscription activated",
        ChangedAt = DateTime.UtcNow
    };
    
    await _subscriptionRepository.UpdateSubscriptionAsync(subscription);
    await _statusHistoryRepository.AddAsync(statusHistory);
    
    // Notify user
    await _subscriptionNotificationService.SendSubscriptionActivatedNotificationAsync(
        subscription.Id, tokenModel);
    
    return Success();
}
```

---

## Pause Flow

Temporarily suspends subscription without cancellation.

### Process

**Service**: `SubscriptionLifecycleService.PauseSubscriptionAsync()`

```csharp
public async Task<JsonModel> PauseSubscriptionAsync(
    string subscriptionId, 
    string reason, 
    TokenModel tokenModel)
{
    // 1. Get and validate subscription
    var subscription = await _subscriptionRepository.GetByIdWithDetailsAsync(Guid.Parse(subscriptionId));
    
    if (subscription.Status != SubscriptionStatuses.Active) {
        return BadRequest("Only active subscriptions can be paused");
    }
    
    // 2. Pause in Stripe
    if (!string.IsNullOrEmpty(subscription.StripeSubscriptionId)) {
        await _stripeService.PauseSubscriptionAsync(
            subscription.StripeSubscriptionId, tokenModel);
    }
    
    // 3. Update subscription
    subscription.Status = SubscriptionStatuses.Paused;
    subscription.PausedDate = DateTime.UtcNow;
    subscription.PauseReason = reason;
    subscription.UpdatedBy = tokenModel.UserID;
    subscription.UpdatedDate = DateTime.UtcNow;
    
    await _subscriptionRepository.UpdateSubscriptionAsync(subscription);
    
    // 4. Record history
    var statusHistory = new SubscriptionStatusHistory {
        SubscriptionId = subscription.Id,
        FromStatus = SubscriptionStatuses.Active,
        ToStatus = SubscriptionStatuses.Paused,
        Reason = reason,
        ChangedAt = DateTime.UtcNow,
        ChangedByUserId = tokenModel.UserID
    };
    await _statusHistoryRepository.AddAsync(statusHistory);
    
    // 5. Notify user
    await _subscriptionNotificationService.SendSubscriptionPausedNotificationAsync(
        subscription.Id, tokenModel);
    
    return Success();
}
```

### Business Rules
- Only `Active` subscriptions can be paused
- Billing stops (no new billing records created)
- Privileges remain but usage may be restricted
- Maximum pause duration enforced (`MaxPauseDurationDays`)
- Auto-resume or cancel after max duration

---

## Resume Flow

Reactivates a paused subscription.

### Process

```csharp
public async Task<JsonModel> ResumeSubscriptionAsync(string subscriptionId, TokenModel tokenModel)
{
    // 1. Get and validate
    var subscription = await _subscriptionRepository.GetByIdWithDetailsAsync(Guid.Parse(subscriptionId));
    
    if (subscription.Status != SubscriptionStatuses.Paused) {
        return BadRequest("Only paused subscriptions can be resumed");
    }
    
    // 2. Resume in Stripe
    if (!string.IsNullOrEmpty(subscription.StripeSubscriptionId)) {
        await _stripeService.ResumeSubscriptionAsync(
            subscription.StripeSubscriptionId, tokenModel);
    }
    
    // 3. Recalculate next billing date
    var pauseDuration = (int)(DateTime.UtcNow - subscription.PausedDate.Value).TotalDays;
    subscription.NextBillingDate = subscription.NextBillingDate.AddDays(pauseDuration);
    
    // 4. Update status
    subscription.Status = SubscriptionStatuses.Active;
    subscription.ResumedDate = DateTime.UtcNow;
    subscription.UpdatedBy = tokenModel.UserID;
    subscription.UpdatedDate = DateTime.UtcNow;
    
    await _subscriptionRepository.UpdateSubscriptionAsync(subscription);
    
    // 5. Record history
    await RecordStatusChange(subscription, "Paused", "Active", "Subscription resumed");
    
    // 6. Notify
    await _subscriptionNotificationService.SendSubscriptionResumedNotificationAsync(
        subscription.Id, tokenModel);
    
    return Success();
}
```

---

## Cancellation Flow

Terminates subscription (terminal state).

### Types of Cancellation
1. **User-initiated** - User cancels via portal
2. **Admin-initiated** - Admin cancels subscription
3. **System-initiated** - Failed payments, policy violations
4. **End-of-period** - Cancel at period end (no immediate effect)
5. **Immediate** - Cancel immediately

### Process

```csharp
public async Task<JsonModel> CancelSubscriptionAsync(
    string subscriptionId, 
    string reason, 
    bool immediate, 
    TokenModel tokenModel)
{
    await _unitOfWork.BeginTransactionAsync();
    try {
        // 1. Get subscription
        var subscription = await _subscriptionRepository.GetByIdWithDetailsAsync(Guid.Parse(subscriptionId));
        
        // 2. Cancel in Stripe
        if (!string.IsNullOrEmpty(subscription.StripeSubscriptionId)) {
            await _stripeService.CancelSubscriptionAsync(
                subscription.StripeSubscriptionId, 
                immediate, 
                tokenModel);
        }
        
        // 3. Update subscription
        if (immediate) {
            subscription.Status = SubscriptionStatuses.Cancelled;
            subscription.EndDate = DateTime.UtcNow;
        } else {
            // Cancel at period end
            subscription.AutoRenew = false;
            subscription.EndDate = subscription.NextBillingDate;
        }
        
        subscription.CancelledDate = DateTime.UtcNow;
        subscription.CancellationReason = reason;
        subscription.UpdatedBy = tokenModel.UserID;
        subscription.UpdatedDate = DateTime.UtcNow;
        
        await _subscriptionRepository.UpdateSubscriptionAsync(subscription);
        
        // 4. Handle refunds (if applicable)
        if (immediate && ShouldProcessRefund(subscription)) {
            await ProcessCancellationRefundAsync(subscription, tokenModel);
        }
        
        // 5. Record history
        await RecordStatusChange(subscription, 
            subscription.Status, 
            SubscriptionStatuses.Cancelled, 
            reason);
        
        // 6. Notify user
        await _subscriptionNotificationService.SendSubscriptionCancelledNotificationAsync(
            subscription.Id, immediate, tokenModel);
        
        await _unitOfWork.CommitTransactionAsync();
        return Success();
    }
    catch {
        await _unitOfWork.RollbackTransactionAsync();
        throw;
    }
}
```

### Cancellation with Refund

```csharp
private async Task ProcessCancellationRefundAsync(Subscription subscription, TokenModel tokenModel)
{
    // Get last payment
    var lastPayment = await _subscriptionPaymentRepository
        .GetLastSuccessfulPaymentAsync(subscription.Id);
    
    if (lastPayment == null) return;
    
    // Calculate prorated refund
    var daysInPeriod = (subscription.NextBillingDate - subscription.LastBillingDate.Value).TotalDays;
    var daysRemaining = (subscription.NextBillingDate - DateTime.UtcNow).TotalDays;
    var refundAmount = (lastPayment.Amount / (decimal)daysInPeriod) * (decimal)daysRemaining;
    
    // Process refund
    await _paymentService.ProcessRefundAsync(
        lastPayment.Id, 
        refundAmount, 
        "Prorated refund for cancellation", 
        tokenModel);
}
```

---

## Status Transitions

### Validation

```csharp
public ValidationResult ValidateStatusTransition(string newStatus)
{
    if (!SubscriptionStatuses.ValidStatuses.Contains(newStatus)) {
        return new ValidationResult($"'{newStatus}' is not a valid status");
    }
    
    if (Status == newStatus) {
        return new ValidationResult($"Already in '{newStatus}' status");
    }
    
    var validTransitions = GetValidStatusTransitions();
    if (!validTransitions.Contains(newStatus)) {
        return new ValidationResult($"Cannot transition from '{Status}' to '{newStatus}'");
    }
    
    return ValidationResult.Success;
}

public string[] GetValidStatusTransitions()
{
    return Status switch
    {
        SubscriptionStatuses.Pending => new[] { Active, TrialActive, Cancelled },
        SubscriptionStatuses.Active => new[] { Paused, Cancelled, Expired, PaymentFailed },
        SubscriptionStatuses.Paused => new[] { Active, Cancelled, Expired },
        SubscriptionStatuses.PaymentFailed => new[] { Active, Cancelled, Expired },
        SubscriptionStatuses.TrialActive => new[] { Active, TrialExpired, Cancelled },
        SubscriptionStatuses.TrialExpired => new[] { Active, Cancelled },
        SubscriptionStatuses.Expired => new[] { Active },
        SubscriptionStatuses.Cancelled => Array.Empty<string>(),
        _ => Array.Empty<string>()
    };
}
```

---

## Database Operations

### Transaction Boundaries

**Critical operations that require transactions**:
1. Subscription creation + privilege allocation
2. Payment processing + status update
3. Cancellation + refund processing
4. Plan change + proration

### Rollback Scenarios

```csharp
// Example: Rollback on payment failure
await _unitOfWork.BeginTransactionAsync();
try {
    // 1. Create subscription
    await _subscriptionRepository.AddAsync(subscription);
    
    // 2. Create Stripe subscription
    var stripeResult = await _stripeService.CreateSubscriptionAsync(...);
    
    // 3. Process payment
    var paymentResult = await _paymentService.ProcessPaymentAsync(...);
    
    if (paymentResult.Failed) {
        // Rollback everything including Stripe
        await _stripeService.CancelSubscriptionAsync(stripeResult.Id);
        await _unitOfWork.RollbackTransactionAsync();
        return Error("Payment failed");
    }
    
    await _unitOfWork.CommitTransactionAsync();
}
catch {
    await _unitOfWork.RollbackTransactionAsync();
    throw;
}
```

---

## Summary

The subscription lifecycle is managed through:
- **State machine** with validated transitions
- **Transaction-safe** operations
- **Stripe synchronization** at each step
- **Comprehensive auditing** via status history
- **Privilege allocation** tied to lifecycle
- **Billing integration** throughout lifecycle
- **Notification system** for user communication

**Next**: See [02_BILLING_MECHANISM.md](./02_BILLING_MECHANISM.md) for billing details.

---

*Document Version: 1.0*  
*Last Updated: 2025*



