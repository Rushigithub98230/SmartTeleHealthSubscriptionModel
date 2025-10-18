# 📘 Service & Method Interaction Map - Developer Guide

> **✨ CURRENT IMPLEMENTATION** | Updated October 18, 2025
> 
> **⚠️ Important:** Some legacy methods (IncrementPrivilegeUsageAsync, ResetAllUsageCountersAsync) have been removed.
> 
> **New Methods Added:**
> - PrivilegeService.CalculatePrivilegeAllocationAsync() - Line 1207
> - PaymentService.ResetPrivilegesForNewBillingPeriodAsync() - Line 1197
> - AutomatedBillingService.CalculateBillingAmountAsync() - Line 932
> 
> **See:** **CURRENT_IMPLEMENTATION_QUICK_REFERENCE.md** for current method list and line numbers

---

## Complete Method-to-Method Call Chains

This document shows you **exactly** which methods call which other methods, so you can trace the execution flow through the codebase.

---

## 1. SUBSCRIPTION CREATION FLOW

### Complete Call Chain

```
USER SUBSCRIBES
    ↓
[1] SubscriptionsController.CreateSubscription()
    Location: API/Controllers/SubscriptionsController.cs:100
    ↓
[2] SubscriptionLifecycleService.CreateSubscriptionAsync()
    Location: Application/Services/SubscriptionLifecycleService.cs:110
    ↓
    ├─[2a] _subscriptionPlanRepository.GetByIdWithDetailsAsync()
    │      Returns: SubscriptionPlan entity
    │
    ├─[2b] _stripeService.EnsureStripeCustomerAsync()
    │      Location: Infrastructure/Services/StripeService.cs:150
    │      ↓
    │      ├─ _userRepository.GetByIdAsync()
    │      ├─ new CustomerService().CreateAsync()  ← Stripe API
    │      └─ _userRepository.UpdateAsync()
    │      Returns: "cus_XYZ789"
    │
    ├─[2c] _unitOfWork.BeginTransactionAsync()
    │
    ├─[2d] _subscriptionRepository.CreateAsync()
    │      Inserts: Subscription entity (Status: Pending)
    │
    ├─[2e] _stripeService.CreateSubscriptionAsync()
    │      Location: Infrastructure/Services/StripeService.cs:600
    │      ↓
    │      └─ new SubscriptionService().CreateAsync()  ← Stripe API
    │      Returns: "sub_stripe_AAA"
    │
    ├─[2f] _subscriptionRepository.UpdateAsync()
    │      Updates: StripeSubscriptionId
    │
    ├─[2g] InitializePrivilegeUsageAsync()  (private method)
    │      Location: Same file, Line: 3055
    │      ↓
    │      ├─ _planPrivilegeRepo.GetByPlanIdAsync()
    │      └─ FOR EACH privilege:
    │          └─ _usageRepo.CreateAsync()
    │             Inserts: UserSubscriptionPrivilegeUsage
    │
    ├─[2h] RecordStatusChangeAsync()  (private method)
    │      Location: Same file, Line: 2657
    │      ↓
    │      └─ _statusHistoryRepository.CreateAsync()
    │         Inserts: SubscriptionStatusHistory
    │
    ├─[2i] _billingService.CreateSubscriptionBillingAsync()
    │      Location: Application/Services/SubscriptionBillingService.cs:85
    │      ↓
    │      └─ _billingRepository.CreateAsync()
    │         Inserts: BillingRecord (Type: Subscription)
    │
    └─[2j] _unitOfWork.CommitTransactionAsync()
           All changes saved atomically

Returns to controller:
    ↓
[3] SubscriptionsController returns JsonModel
    StatusCode: 201 Created
    data: SubscriptionDto
```

---

## 2. PRIVILEGE USAGE FLOW

### Complete Call Chain

```
USER BOOKS CONSULTATION
    ↓
[1] AppointmentsController.BookConsultationAsync()
    Location: API/Controllers/AppointmentsController.cs
    ↓
[2] PrivilegeService.CheckPrivilegeAvailabilityAsync()
    Location: Application/Services/PrivilegeService.cs:126
    ↓
    ├─[2a] _subscriptionRepository.GetActiveSubscriptionByUserIdAsync()
    │      Returns: Active subscription or null
    │
    ├─[2b] _privilegeUsageRepository.GetByUserAndPrivilegeAsync()
    │      Returns: UserSubscriptionPrivilegeUsage entity
    │
    ├─[2c] Check: usage.AllowedValue >= requestedQuantity
    │      If YES → Return 200 OK
    │      If NO → Get overage pricing:
    │          ├─ _subscriptionPlanRepository.GetLatestVersionAsync()
    │          ├─ Extract: planPrivilege.UnitCost
    │          └─ Return 402 Payment Required
    │
    └─ Returns: JsonModel (200 or 402)

If 200 OK (Has credits):
    ↓
[3] PrivilegeService.UsePrivilegeAsync()
    Location: Application/Services/PrivilegeService.cs:220
    ↓
    ├─[3a] _unitOfWork.BeginTransactionAsync()
    │
    ├─[3b] _privilegeUsageRepository.GetByUserAndPrivilegeAsync()
    │      Gets current usage (with row lock)
    │
    ├─[3c] Update counters:
    │      usage.UsedValue++
    │      usage.AllowedValue = AllocatedLimit - UsedValue
    │      usage.LastUsedAt = UtcNow
    │
    ├─[3d] _privilegeUsageRepository.UpdateAsync()
    │      Updates: Usage counters
    │
    ├─[3e] AddUsageHistoryAsync()  (private method)
    │      Location: Same file, Line: 330
    │      ↓
    │      └─ _privilegeUsageHistoryRepository.CreateAsync()
    │         Inserts: PrivilegeUsageHistory
    │            (UsageType: "Included", Cost: 0)
    │
    └─[3f] _unitOfWork.CommitTransactionAsync()

Returns to controller:
    ↓
[4] AppointmentsController proceeds with booking
    Creates appointment record
    Returns success to user
```

---

## 3. OVERAGE PURCHASE FLOW

### Complete Call Chain

```
USER PAYS FOR OVERAGE
    ↓
[1] SubscriptionsController.PurchaseAdditionalCreditsAsync()
    Location: API/Controllers/SubscriptionsController.cs:450
    ↓
[2] SubscriptionService.PurchaseAdditionalCreditsAsync()
    Location: Application/Services/SubscriptionService.cs:1762
    ↓
    ├─[2a] _subscriptionRepository.GetByIdWithDetailsAsync()
    │
    ├─[2b] Validate subscription status & ownership
    │
    ├─[2c] _subscriptionPlanRepository.GetLatestVersionAsync()
    │      Gets latest plan for abuse prevention
    │
    ├─[2d] Calculate: quantity × unitCost
    │
    ├─[2e] _unitOfWork.BeginTransactionAsync()
    │
    ├─[2f] _billingService.CreateBillingRecordAsync()
    │      Location: Application/Services/SubscriptionBillingService.cs:125
    │      ↓
    │      └─ _billingRepository.CreateAsync()
    │         Inserts: BillingRecord (Type: Overage, Status: Pending)
    │
    ├─[2g] _billingService.ProcessPaymentAsync()
    │      Location: Application/Services/SubscriptionBillingService.cs:1078
    │      ↓
    │      └─ _paymentService.ProcessPaymentAsync()
    │         Location: Application/Services/PaymentService.cs:400
    │         ↓
    │         ├─ _stripeService.CreatePaymentIntentAsync()
    │         │  Location: Infrastructure/Services/StripeService.cs:850
    │         │  ↓
    │         │  └─ new PaymentIntentService().CreateAsync()
    │         │     Stripe API: Charges customer
    │         │     Returns: PaymentIntent (Status: succeeded)
    │         │
    │         ├─ _billingRepository.UpdateAsync()
    │         │  Updates: Status → Paid
    │         │
    │         └─ _paymentRepository.CreateAsync()
    │            Inserts: SubscriptionPayment
    │
    ├─[2h] _privilegeUsageRepository.GetByUserAndPrivilegeAsync()
    │
    ├─[2i] Update usage:
    │      usage.AllocatedLimit++
    │      usage.AllowedValue++
    │
    ├─[2j] _privilegeUsageRepository.UpdateAsync()
    │      Updates: Credit added
    │
    ├─[2k] _privilegeService.UsePrivilegeAsync()
    │      (Immediately uses the purchased credit)
    │      ↓
    │      ├─ Updates: UsedValue++, AllowedValue--
    │      └─ Records in history (UsageType: "Overage", Cost: $25)
    │
    └─[2l] _unitOfWork.CommitTransactionAsync()

Returns to controller:
    ↓
[3] SubscriptionsController returns success
    data: { BillingRecordId, AmountCharged, NewBalance }
```

---

## 4. AUTOMATED RENEWAL FLOW

### Complete Call Chain

```
RENEWAL DATE ARRIVES
    ↓
[STRIPE SIDE] Stripe's Internal Scheduler
    ↓
    ├─ Detects: Subscription due for renewal
    ├─ Creates: Invoice (in_stripe_DDD)
    ├─ Charges: Customer's payment method
    └─ Sends: Webhook "invoice.payment_succeeded"
    ↓
[YOUR SYSTEM] Webhook Received
    ↓
[1] StripeWebhookController.HandleWebhook()
    Location: API/Controllers/StripeWebhookController.cs:97
    ↓
    ├─[1a] EventUtility.ConstructEvent()  ← Stripe SDK
    │      Validates webhook signature
    │
    ├─[1b] _webhookIdempotencyService.CheckIdempotencyAsync()
    │      Location: Application/Services/WebhookIdempotencyService.cs:50
    │      ↓
    │      └─ Checks if event already processed
    │         Returns: { ShouldProcess: true }
    │
    ├─[1c] ProcessWebhookWithRetryAsync()  (private method)
    │      Location: Same file, Line: 195
    │      ↓
    │      └─ ProcessStripeEvent()  (private method)
    │         Location: Same file, Line: 230
    │         ↓
    │         └─ Routes by event type:
    │            case "invoice.payment_succeeded":
    │              → HandlePaymentSucceeded()
    │
    └─[1d] _webhookIdempotencyService.MarkAsProcessedAsync()
           Records: Event processed successfully

    ↓
[2] HandlePaymentSucceeded()  (private method)
    Location: Same file, Line: 540
    ↓
    ├─[2a] Extract invoice from event
    │
    ├─[2b] GetSubscriptionIdFromInvoice()  (private method)
    │      Location: Same file, Line: 887
    │      Returns: "sub_111" (from metadata)
    │
    ├─[2c] _subscriptionRepository.GetByIdWithDetailsAsync()
    │
    ├─[2d] IsRenewal()  (private method, checks if first payment or renewal)
    │      Returns: true (has previous payment)
    │
    ├─[2e] _unitOfWork.BeginTransactionAsync()
    │
    ├─[2f] _billingRepository.CreateAsync()
    │      Inserts: BillingRecord (renewal)
    │
    ├─[2g] _paymentRepository.CreateAsync()
    │      Inserts: SubscriptionPayment
    │
    ├─[2h] _subscriptionRepository.UpdateAsync()
    │      Updates: EndDate, NextBillingDate
    │
    ├─[2i] IF isRenewal:
    │      └─ ResetPrivilegeUsageAsync()  (private method)
    │         ↓
    │         ├─ _privilegeUsageRepository.GetBySubscriptionIdAsync()
    │         └─ FOR EACH usage:
    │             ├─ Reset: AllocatedLimit, UsedValue, AllowedValue
    │             └─ _privilegeUsageRepository.UpdateAsync()
    │
    ├─[2j] _statusHistoryRepository.CreateAsync()
    │      Inserts: Status history (Active → Active, renewed)
    │
    ├─[2k] _unitOfWork.CommitTransactionAsync()
    │
    └─[2l] _notificationService.SendRenewalConfirmationAsync()

Returns to Stripe:
    ↓
[3] StripeWebhookController returns 200 OK
    Stripe stops retrying this webhook
```

---

## 5. PAYMENT FAILURE FLOW

### Complete Call Chain

```
PAYMENT FAILS IN STRIPE
    ↓
[STRIPE] Sends webhook: "invoice.payment_failed"
    ↓
[1] StripeWebhookController.HandleWebhook()
    ↓
    └─ Routes to: HandlePaymentFailed()
       Location: API/Controllers/StripeWebhookController.cs:610
    ↓
[2] HandlePaymentFailed()
    ↓
    ├─[2a] Extract invoice and error from event
    │
    ├─[2b] GetSubscriptionIdFromInvoice()
    │
    ├─[2c] _subscriptionRepository.GetByIdAsync()
    │
    ├─[2d] Update subscription:
    │      subscription.Status = "PaymentFailed"
    │      subscription.FailedPaymentAttempts++
    │      subscription.LastPaymentError = error message
    │
    ├─[2e] _subscriptionRepository.UpdateAsync()
    │
    ├─[2f] _billingRepository.CreateAsync()
    │      Inserts: Failed billing record
    │
    └─[2g] _notificationService.SendPaymentFailureNotificationAsync()

THEN (Automated):
    ↓
[3] AutomatedBillingBackgroundService (Daily Job)
    Location: Infrastructure/Services/AutomatedBillingBackgroundService.cs
    Method: ExecuteAsync()
    ↓
    └─[3a] ProcessFailedPaymentsAsync()
       Location: Same file, Line: 200
       ↓
       ├─ _subscriptionRepository.GetByStatusAsync("PaymentFailed")
       │
       └─ FOR EACH failed subscription:
           ↓
          [3b] ProcessPaymentWithRetryAsync()
               Location: Same file, Line: 220
               ↓
               ├─ _billingRepository.GetPendingBillingRecordsAsync()
               ├─ _billingService.ProcessPaymentAsync()
               │  ↓
               │  └─ _paymentService.ProcessPaymentAsync()
               │     ↓
               │     └─ _stripeService.CreatePaymentIntentAsync()
               │        Tries to charge again
               │
               ├─ IF SUCCESS:
               │  ├─ subscription.Status = "Active"
               │  ├─ subscription.FailedPaymentAttempts = 0
               │  └─ _notificationService.SendPaymentSuccessNotificationAsync()
               │
               └─ IF FAILED (and attempts == 3):
                  ├─ subscription.Status = "Suspended"
                  ├─ subscription.SuspendedDate = UtcNow
                  └─ _notificationService.SendSuspensionNotificationAsync()
```

---

## 6. ADMIN CREATES PLAN FLOW

### Complete Call Chain

```
ADMIN CREATES PLAN
    ↓
[1] SubscriptionPlansController.CreatePlan()
    Location: API/Controllers/SubscriptionPlansController.cs:50
    ↓
[2] SubscriptionPlanService.CreatePlanAsync()
    Location: Application/Services/SubscriptionPlanService.cs:165
    ↓
    ├─[2a] Validate admin authorization
    │      if (tokenModel.RoleID != Admin) → 403
    │
    ├─[2b] _unitOfWork.BeginTransactionAsync()
    │
    ├─[2c] _subscriptionPlanRepository.CreatePlanAsync()
    │      Inserts: SubscriptionPlan (Price: 0, temporary)
    │
    ├─[2d] _stripeService.CreateProductAsync()
    │      Location: Infrastructure/Services/StripeService.cs:420
    │      ↓
    │      └─ new ProductService().CreateAsync()  ← Stripe API
    │         Returns: "prod_ABC123"
    │
    ├─[2e] _stripeService.CreatePriceAsync()  × 3 times
    │      Location: Infrastructure/Services/StripeService.cs:480
    │      ↓
    │      └─ new PriceService().CreateAsync()  ← Stripe API
    │         Returns: "price_1Month_XYZ", "price_3Month_XYZ", "price_12Month_XYZ"
    │
    ├─[2f] _subscriptionPlanRepository.UpdatePlanAsync()
    │      Updates: StripeProductId, Price IDs
    │
    ├─[2g] FOR EACH privilege in createDto.Privileges:
    │      ├─ _privilegeRepository.GetByIdAsync()
    │      │  Validates privilege exists
    │      └─ _planPrivilegeRepository.CreateAsync()
    │         Inserts: SubscriptionPlanPrivilege
    │
    ├─[2h] IF IsAutoCalculatedPrice:
    │      └─ _pricingService.CalculatePricingBreakdownAsync()
    │         Location: Application/Services/PlanPricingService.cs:54
    │         ↓
    │         ├─ _subscriptionPlanRepository.GetPlanPrivilegesAsync()
    │         ├─ Calculate: Σ(Value × PrivilegeBaseCost)
    │         ├─ Calculate: Commission
    │         ├─ Calculate: FinalPrice = Total + Commission
    │         └─ Returns: PricingBreakdown
    │              { FinalPrice: 275, PrivilegesTotalCost: 250, CommissionAmount: 25 }
    │
    ├─[2i] _subscriptionPlanRepository.UpdatePlanAsync()
    │      Updates: Price = 275, PrivilegesTotalCost = 250
    │
    └─[2j] _unitOfWork.CommitTransactionAsync()

Returns to controller:
    ↓
[3] SubscriptionPlansController returns JsonModel
    StatusCode: 201 Created
    data: SubscriptionPlanDto
```

---

## 7. BILLING RECORD CREATION FLOW

### Complete Call Chain

```
CREATE BILLING RECORD
    ↓
[1] SubscriptionBillingService.CreateSubscriptionBillingAsync()
    Location: Application/Services/SubscriptionBillingService.cs:85
    Parameters:
      - subscription: Subscription entity
      - amount: decimal (e.g., 275.00)
      - description: string
      - dueDate: DateTime
      - tokenModel: TokenModel
    ↓
    ├─[1a] Create BillingRecord entity:
    │      var billingRecord = new BillingRecord {
    │        Id = Guid.NewGuid(),
    │        UserId = subscription.UserId,
    │        SubscriptionId = subscription.Id,
    │        Type = BillingRecord.BillingType.Subscription,
    │        Status = BillingRecord.BillingStatus.Pending,
    │        Amount = amount,
    │        TotalAmount = amount,
    │        BillingDate = DateTime.UtcNow,
    │        DueDate = dueDate,
    │        InvoiceNumber = GenerateInvoiceNumber(),
    │        Description = description,
    │        PaymentMethod = "stripe",
    │        CreatedBy = tokenModel.UserID,
    │        CreatedDate = DateTime.UtcNow
    │      };
    │
    ├─[1b] _billingRepository.CreateAsync()
    │      Location: Infrastructure/Repositories/BillingRepository.cs
    │      ↓
    │      └─ _dbContext.BillingRecords.Add(billingRecord)
    │         await _dbContext.SaveChangesAsync()
    │         Inserts into SQL Server
    │
    ├─[1c] _mapper.Map<BillingRecordDto>()
    │      Converts entity to DTO
    │
    └─ Returns: JsonModel { data: BillingRecordDto, StatusCode: 200 }

THEN (If payment needed):
    ↓
[2] SubscriptionBillingService.ProcessPaymentAsync()
    Location: Same file, Line: 1078
    ↓
    └─ Delegates to PaymentService (see Payment Processing Flow)
```

---

## 8. REPOSITORY LAYER METHODS

### Common Repository Operations

#### **Create Operations**
```csharp
// Pattern used across all repositories
public async Task<TEntity> CreateAsync(TEntity entity)
{
    _dbContext.Set<TEntity>().Add(entity);
    await _dbContext.SaveChangesAsync();
    return entity;
}
```

**Used by:**
- `SubscriptionRepository.CreateAsync()` → Creates Subscription
- `BillingRepository.CreateAsync()` → Creates BillingRecord
- `PrivilegeUsageRepository.CreateAsync()` → Creates UserSubscriptionPrivilegeUsage
- `PrivilegeUsageHistoryRepository.CreateAsync()` → Creates PrivilegeUsageHistory

#### **Update Operations**
```csharp
public async Task<TEntity> UpdateAsync(TEntity entity)
{
    entity.UpdatedDate = DateTime.UtcNow;
    _dbContext.Set<TEntity>().Update(entity);
    await _dbContext.SaveChangesAsync();
    return entity;
}
```

#### **Query Operations**
```csharp
// Get by ID with related entities
public async Task<Subscription> GetByIdWithDetailsAsync(Guid id)
{
    return await _dbContext.Subscriptions
        .Include(s => s.User)
        .Include(s => s.SubscriptionPlan)
            .ThenInclude(sp => sp.PlanPrivileges)
                .ThenInclude(pp => pp.Privilege)
        .Include(s => s.BillingCycle)
        .FirstOrDefaultAsync(s => s.Id == id);
}
```

---

## 9. SERVICE INTERACTION MATRIX

### Who Calls Whom?

| Calling Service | Called Service | Common Methods Called | Purpose |
|-----------------|----------------|----------------------|---------|
| **SubscriptionLifecycleService** | SubscriptionBillingService | CreateSubscriptionBillingAsync() | Create billing on subscription creation |
| **SubscriptionLifecycleService** | StripeService | EnsureStripeCustomerAsync(), CreateSubscriptionAsync() | Stripe customer & subscription setup |
| **SubscriptionLifecycleService** | PrivilegeService | InitializePrivilegesAsync() | Set up privilege tracking |
| **SubscriptionService** | SubscriptionBillingService | CreateBillingRecordAsync(), ProcessPaymentAsync() | Overage billing |
| **SubscriptionService** | PrivilegeService | CheckPrivilegeAvailabilityAsync(), UsePrivilegeAsync() | Usage validation & tracking |
| **SubscriptionBillingService** | PaymentService | ProcessPaymentAsync(), ProcessRefundAsync() | Payment operations |
| **SubscriptionBillingService** | StripeService | CreatePaymentIntentAsync() | Stripe payment processing |
| **PaymentService** | StripeService | CreatePaymentIntentAsync(), CreateRefundAsync() | All Stripe payment operations |
| **SubscriptionPlanService** | PlanPricingService | CalculatePricingBreakdownAsync() | Auto-calculate plan prices |
| **SubscriptionPlanService** | StripeService | CreateProductAsync(), CreatePriceAsync() | Sync plans to Stripe |
| **AutomatedBillingService** | SubscriptionBillingService | CreateSubscriptionBillingAsync() | Automated billing |
| **StripeWebhookController** | SubscriptionRepository | GetByStripeSubscriptionIdAsync() | Find local subscription |
| **StripeWebhookController** | BillingRepository | CreateAsync(), UpdateAsync() | Update billing records |
| **All Services** | UnitOfWork | BeginTransactionAsync(), CommitTransactionAsync() | Transaction management |

---

## 10. TRANSACTION BOUNDARIES

### Where Transactions Start & End

#### **Scenario 1: Create Subscription**
```
Transaction Start: SubscriptionLifecycleService.CreateSubscriptionAsync(), Line: 125
├─ Create subscription entity
├─ Update with Stripe ID
├─ Initialize privilege usage
├─ Record status history
└─ Create billing record
Transaction End: Same method, Line: 234 (Commit)
```

#### **Scenario 2: Purchase Overage**
```
Transaction Start: SubscriptionService.PurchaseAdditionalCreditsAsync(), Line: 1880
├─ Create billing record
├─ Process payment
├─ Add credit to usage
├─ Use privilege
└─ Record in history
Transaction End: Same method, Line: 2015 (Commit)
```

#### **Scenario 3: Use Privilege**
```
Transaction Start: PrivilegeService.UsePrivilegeAsync(), Line: 225
├─ Update usage counters
└─ Record in usage history
Transaction End: Same method, Line: 315 (Commit)
```

#### **Scenario 4: Process Renewal (Webhook)**
```
Transaction Start: StripeWebhookController.HandlePaymentSucceeded(), Line: 550
├─ Create billing record
├─ Create payment record
├─ Update subscription dates
├─ Reset privilege usage
└─ Record status history
Transaction End: Same method, Line: 630 (Commit)
```

---

## Key Takeaways

### ✅ Understanding Call Chains

1. **Controllers** call **Services** (never repositories directly)
2. **Services** call **Repositories** for data access
3. **Services** call **other Services** for cross-cutting concerns
4. **Transactions** managed by **UnitOfWork** (injected into services)
5. **Stripe calls** wrapped in **StripeService** (single point of integration)

### 🔍 Tracing Execution

**To trace a bug:**
1. Start at controller endpoint
2. Follow service method call
3. Check which repository methods are called
4. Look for transaction boundaries
5. Review error handling (catch blocks)
6. Check logs for execution path

**Example:**
```
Bug: "Payment not reflecting"
Trace:
  1. Check SubscriptionsController.PurchaseAdditionalCreditsAsync()
  2. Follow to SubscriptionService.PurchaseAdditionalCreditsAsync()
  3. Check if _billingService.ProcessPaymentAsync() was called
  4. Check if _paymentService.ProcessPaymentAsync() was called
  5. Check if transaction was committed
  6. Check Stripe dashboard for actual charge
  7. Review webhook logs for confirmation event
```

---

**Document Version:** 1.0  
**Last Updated:** October 17, 2025

