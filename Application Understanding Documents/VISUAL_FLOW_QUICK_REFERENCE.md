# 📊 Visual Flow Quick Reference Guide
## All Major Flows on One Page

> **✨ CURRENT IMPLEMENTATION** | Updated October 18, 2025
> 
> **Note:** Flows show monthly examples. Current system supports Monthly/Quarterly/Annual billing cycles.
> **Formulas:** See **CURRENT_IMPLEMENTATION_QUICK_REFERENCE.md** for billing cycle calculations

---

**Use this for:** Quick visualization of system flows during development

---

## FLOW 1: ADMIN CREATES PLAN (30 seconds)

```
Admin Portal → POST /api/subscription-plans/admin
    ↓
SubscriptionPlanService.CreatePlanAsync()
    ↓
BEGIN TRANSACTION
    ↓
    ├─ Create Plan in DB (SubscriptionPlans)
    ├─ Create Product in Stripe → prod_ABC123
    ├─ Create 3 Prices in Stripe → price_1Month_XYZ, etc.
    ├─ Update Plan with Stripe IDs
    ├─ Assign Privileges (SubscriptionPlanPrivileges)
    └─ Auto-calculate Price: $275
    ↓
COMMIT TRANSACTION
    ↓
✅ Plan Created & Synced to Stripe
```

---

## FLOW 2: USER SUBSCRIBES (60 seconds)

```
User Portal → POST /api/subscriptions
    ↓
SubscriptionLifecycleService.CreateSubscriptionAsync()
    ↓
Validate Plan → Check Duplicates
    ↓
EnsureStripeCustomerAsync() → cus_XYZ789
    ↓
BEGIN TRANSACTION
    ↓
    ├─ Create Subscription (Status: Pending)
    ├─ CreateSubscriptionAsync() in Stripe → sub_stripe_AAA
    ├─ Update with StripeSubscriptionId
    ├─ Initialize Privileges (5 consult, 3 meds)
    ├─ Record Status History (null → Pending)
    └─ Create Billing Record ($275, Pending)
    ↓
COMMIT TRANSACTION
    ↓
Stripe Auto-Charges → Invoice: in_stripe_BBB
    ↓
Webhook: invoice.payment_succeeded
    ↓
HandlePaymentSucceeded()
    ↓
    ├─ Update Subscription (Status: Active)
    ├─ Update Billing Record (Status: Paid)
    ├─ Create Payment Record
    └─ Record Status History (Pending → Active)
    ↓
✅ Subscription Active! User can use services
```

---

## FLOW 3: USER USES PRIVILEGE (15 seconds)

```
User Portal → Book Consultation
    ↓
CheckPrivilegeAvailabilityAsync()
    ↓
Query: UserSubscriptionPrivilegeUsage
    AllocatedLimit: 5
    UsedValue: 2
    AllowedValue: 3 ✅
    ↓
Return 200 OK (Has Credits)
    ↓
UsePrivilegeAsync()
    ↓
BEGIN TRANSACTION
    ↓
    ├─ Update: UsedValue: 2 → 3
    ├─ Update: AllowedValue: 3 → 2
    └─ Insert History: Type=Included, Cost=$0
    ↓
COMMIT TRANSACTION
    ↓
✅ Service Used, Counter Decremented
```

---

## FLOW 4: OVERAGE PURCHASE (45 seconds)

```
User tries service (no credits left)
    ↓
CheckPrivilegeAvailabilityAsync()
    ↓
Query: AllowedValue = 0 ❌
    ↓
Get Latest Plan → UnitCost: $25
    ↓
Return 402 Payment Required
    data: { CostPerUnit: $25, TotalRequired: $25 }
    ↓
Frontend: Show Payment Modal
    ↓
User Clicks: "Pay $25 & Continue"
    ↓
POST /api/subscriptions/{id}/credits
    ↓
PurchaseAdditionalCreditsAsync()
    ↓
BEGIN TRANSACTION
    ↓
    ├─ Create Billing Record (Type: Overage, $25, Pending)
    ├─ ProcessPaymentAsync() → Stripe charges $25
    ├─ Update Billing (Status: Paid)
    ├─ Add Credit: AllocatedLimit: 5 → 6, AllowedValue: 0 → 1
    ├─ Use Credit Immediately: UsedValue: 5 → 6, AllowedValue: 1 → 0
    └─ Insert History: Type=Overage, Cost=$25
    ↓
COMMIT TRANSACTION
    ↓
✅ Paid $25, Credit Added & Used
```

---

## FLOW 5: MONTHLY RENEWAL (90 seconds)

```
DAY 30: Renewal Date
    ↓
STRIPE (Automatic):
    ├─ Create Invoice: in_stripe_DDD, $275
    ├─ Charge Payment Method
    └─ Send Webhook: invoice.payment_succeeded
    ↓
HandlePaymentSucceeded()
    ↓
BEGIN TRANSACTION
    ↓
    ├─ Create Billing Record (Type: Subscription, Status: Paid)
    ├─ Create Payment Record ($275, Success)
    ├─ Update Subscription Dates (EndDate: +1 month)
    ├─ RESET PRIVILEGES:
    │  ├─ Teleconsultation: Allocated=5, Used=0, Allowed=5
    │  └─ Medication: Allocated=3, Used=0, Allowed=3
    └─ Record Status History (Active → Active, renewed)
    ↓
COMMIT TRANSACTION
    ↓
Send Confirmation Email
    ↓
✅ Renewed! Fresh Credits for New Month
```

---

## FLOW 6: PAYMENT FAILURE (120 seconds)

```
Renewal Date → Stripe Attempts Charge
    ↓
Card Expired ❌
    ↓
Webhook: invoice.payment_failed
    ↓
HandlePaymentFailed()
    ↓
    ├─ Update: Status → PaymentFailed
    ├─ Update: FailedPaymentAttempts++
    ├─ Create Billing Record (Status: Failed)
    └─ Send URGENT Email: "Payment Failed"
    ↓
USER: Still has access (grace period)
    ↓
DAY +2: Retry #1 (AutomatedBillingService)
    ├─ Attempt payment
    └─ Still fails (card not updated)
    ↓
DAY +5: Retry #2
    ├─ User updated card in meantime
    ├─ Attempt payment → SUCCESS ✅
    ├─ Status: PaymentFailed → Active
    ├─ FailedPaymentAttempts: 0
    └─ Reset privileges
    ↓
✅ Recovered! Service Continues

ALTERNATIVE: Never updates card
    ↓
DAY +7: Retry #3 (Final)
    ├─ Still fails
    ├─ Status → Suspended ⛔
    └─ Access: BLOCKED
```

---

## DATABASE STATE TRACKING

### Example: Complete Lifecycle in Database

```sql
-- DAY 1: Subscription Created
Subscriptions:
  Id: sub_111
  Status: 'Pending'
  StripeSubscriptionId: 'sub_stripe_AAA'

UserSubscriptionPrivilegeUsage:
  [Teleconsult] Allocated: 5, Used: 0, Allowed: 5
  [Medication]  Allocated: 3, Used: 0, Allowed: 3

BillingRecords:
  [bill_001] Type: Subscription, Amount: $275, Status: Pending

-- DAY 1 (30 min later): Payment Succeeds
Subscriptions:
  Status: 'Pending' → 'Active' ✅

BillingRecords:
  [bill_001] Status: Pending → Paid ✅

SubscriptionPayments:
  [pay_001] Amount: $275, Status: Success ✅

-- DAY 5: User Uses Consultation
UserSubscriptionPrivilegeUsage:
  [Teleconsult] Allocated: 5, Used: 1, Allowed: 4 ✅

PrivilegeUsageHistory:
  [history_1] Type: Included, Cost: $0, Remaining: 4 ✅

-- DAY 25: User Purchases Overage
BillingRecords:
  [bill_002] Type: Overage, Amount: $25, Status: Paid ✅

UserSubscriptionPrivilegeUsage:
  [Teleconsult] Allocated: 6, Used: 6, Allowed: 0 ✅

PrivilegeUsageHistory:
  [history_6] Type: Overage, Cost: $25, Remaining: 0 ✅

-- DAY 30: Monthly Renewal
BillingRecords:
  [bill_003] Type: Subscription, Amount: $275, Status: Paid ✅

UserSubscriptionPrivilegeUsage:
  [Teleconsult] Allocated: 5 (reset), Used: 0, Allowed: 5 ✅
  [Medication]  Allocated: 3 (reset), Used: 0, Allowed: 3 ✅

Subscriptions:
  EndDate: 2025-11-17 → 2025-12-17 ✅
  NextBillingDate: 2025-12-17 ✅
```

---

## SERVICE CALL PATTERNS

### Pattern A: CRUD with Stripe Sync

```
Controller → Service → Repository + StripeService
                     ↓
            Begin Transaction
                     ↓
            Create/Update in DB
                     ↓
            Create/Update in Stripe
                     ↓
            Link IDs
                     ↓
            Commit Transaction
```

### Pattern B: Privilege Usage

```
Controller → CheckAvailability → Query Usage
                     ↓
            If Available: UsePrivilege
                     ↓
            Begin Transaction
                     ↓
            Update Counters
                     ↓
            Record History
                     ↓
            Commit Transaction
```

### Pattern C: Webhook Processing

```
Stripe → Webhook → Validate Signature
                     ↓
            Check Idempotency
                     ↓
            Route to Handler
                     ↓
            Begin Transaction
                     ↓
            Update Database
                     ↓
            Commit Transaction
                     ↓
            Mark as Processed
                     ↓
            Return 200 OK
```

---

## QUICK DEBUGGING CHECKLIST

### Subscription Not Created?
```
□ Check logs for errors
□ Check if plan exists and is active
□ Check for duplicate subscriptions
□ Check Stripe dashboard for customer creation
□ Check transaction rollback in logs
□ Check WebhookEvents for payment webhook
```

### Payment Not Processing?
```
□ Check billing record status
□ Check Stripe payment intent status
□ Check user's StripeCustomerId
□ Check payment method attached
□ Check webhook signature validation
□ Check idempotency table
```

### Privileges Not Working?
```
□ Check UserSubscriptionPrivilegeUsage table
□ Check subscription status is Active
□ Check AllocatedLimit, UsedValue, AllowedValue
□ Check privilege initialization
□ Check PrivilegeUsageHistory for records
□ Check if reset occurred on renewal
```

---

## CRITICAL VALIDATION CHECKLIST

### Before Deploying a Change

□ Does it use transactions for multi-step operations?  
□ Does it sync with Stripe where applicable?  
□ Does it record in history/audit tables?  
□ Does it handle errors and rollback?  
□ Does it validate user permissions?  
□ Does it log important operations?  
□ Does it send appropriate notifications?  
□ Does it follow SRP (single responsibility)?  

---

## KEY FORMULAS

### Plan Price Calculation
```
Plan Price = Σ(Privilege.Value × PrivilegeBaseCost) + Commission

Example:
  (5 consultations × $20) + (3 medications × $50) + $25 commission
  = $100 + $150 + $25
  = $275
```

### Overage Cost Calculation
```
Overage Cost = Quantity × UnitCost (from LATEST plan version)

Example:
  1 extra consultation × $25 = $25
```

### Remaining Credits Calculation
```
AllowedValue = AllocatedLimit - UsedValue

Example:
  5 (allocated) - 3 (used) = 2 (remaining)
```

---

## FILE LOCATIONS QUICK REFERENCE

| What | File Path | Line |
|------|-----------|------|
| **Subscription creation** | Application/Services/SubscriptionLifecycleService.cs | 110-290 |
| **Plan creation** | Application/Services/SubscriptionPlanService.cs | 165-410 |
| **Privilege check** | Application/Services/PrivilegeService.cs | 126-219 |
| **Privilege usage** | Application/Services/PrivilegeService.cs | 220-327 |
| **Overage purchase** | Application/Services/SubscriptionService.cs | 1762-2030 |
| **Billing creation** | Application/Services/SubscriptionBillingService.cs | 85-150 |
| **Payment processing** | Application/Services/PaymentService.cs | 400-500 |
| **Stripe customer** | Infrastructure/Services/StripeService.cs | 150-200 |
| **Webhook handler** | API/Controllers/StripeWebhookController.cs | 97-160 |
| **Payment success** | API/Controllers/StripeWebhookController.cs | 540-650 |
| **Payment failed** | API/Controllers/StripeWebhookController.cs | 610-680 |
| **Automated billing** | Infrastructure/Services/AutomatedBillingBackgroundService.cs | 50-120 |

---

## TESTING SCENARIOS

### Manual Test Cases

**Test 1: Create Plan**
```
1. Login as admin
2. POST /api/subscription-plans/admin
3. Verify: Plan in DB, Product in Stripe
4. Verify: Price auto-calculated correctly
5. Verify: Privileges assigned
```

**Test 2: Subscribe**
```
1. Login as user
2. POST /api/subscriptions
3. Verify: Subscription created (Pending)
4. Wait for webhook
5. Verify: Status → Active
6. Verify: Privileges initialized
7. Verify: Billing record created & paid
```

**Test 3: Use Privilege**
```
1. Login as user with active subscription
2. Book consultation
3. Verify: Counter decremented
4. Verify: History recorded
5. Repeat until 0 credits
```

**Test 4: Overage**
```
1. Use all credits (0 remaining)
2. Try to book another consultation
3. Verify: 402 Payment Required
4. Purchase additional credit
5. Verify: Payment processed
6. Verify: Credit added & used
7. Verify: History marked as "Overage"
```

**Test 5: Renewal**
```
1. Wait for renewal date OR use Stripe test clock
2. Verify: Stripe charges automatically
3. Verify: Webhook received
4. Verify: Billing record created
5. Verify: Privileges reset
6. Verify: Dates extended
```

---

## POSTMAN COLLECTION OUTLINE

### Authentication
```
POST /api/auth/login
Body: { email, password }
→ Save token for subsequent requests
```

### Admin Actions
```
POST /api/subscription-plans/admin
Header: Authorization: Bearer {token}
Body: {
  "name": "Test Plan",
  "isAutoCalculatedPrice": true,
  "adminCommissionPercent": 10,
  "privileges": [...]
}
```

### User Actions
```
POST /api/subscriptions
Header: Authorization: Bearer {token}
Body: {
  "planId": "{plan-guid}",
  "billingCycleId": "{cycle-guid}",
  "paymentMethodId": "pm_card_visa"
}
```

### Privilege Usage
```
POST /api/privileges/check-availability
Body: {
  "privilegeId": "{privilege-guid}",
  "quantity": 1
}

POST /api/privileges/use
Body: {
  "privilegeId": "{privilege-guid}",
  "quantity": 1,
  "relatedEntityId": "appt-123"
}
```

### Overage Purchase
```
POST /api/subscriptions/{id}/credits
Body: {
  "privilegeName": "Teleconsultation",
  "quantity": 1,
  "paymentMethodId": "pm_card_visa"
}
```

---

## SQL QUERIES FOR DEBUGGING

### Check User's Subscription Status
```sql
SELECT 
    s.Id,
    s.Status,
    s.StartDate,
    s.EndDate,
    s.NextBillingDate,
    s.CurrentPrice,
    sp.Name as PlanName,
    s.StripeSubscriptionId,
    s.FailedPaymentAttempts
FROM Subscriptions s
JOIN SubscriptionPlans sp ON s.SubscriptionPlanId = sp.Id
WHERE s.UserId = 456
ORDER BY s.CreatedDate DESC;
```

### Check Privilege Usage
```sql
SELECT 
    p.Name as Privilege,
    u.AllocatedLimit,
    u.UsedValue,
    u.AllowedValue,
    u.LastUsedAt,
    u.ResetAt
FROM UserSubscriptionPrivilegeUsage u
JOIN Privileges p ON u.PrivilegeId = p.Id
WHERE u.SubscriptionId = 'sub_111';
```

### Check Billing History
```sql
SELECT 
    Type,
    Status,
    Amount,
    BillingDate,
    PaidDate,
    InvoiceNumber,
    Description
FROM BillingRecords
WHERE SubscriptionId = 'sub_111'
ORDER BY BillingDate DESC;
```

### Check Usage History
```sql
SELECT 
    UsageDate,
    p.Name as Privilege,
    QuantityUsed,
    RemainingAfterUse,
    UsageType,
    Cost,
    Notes
FROM PrivilegeUsageHistory h
JOIN Privileges p ON h.PrivilegeId = p.Id
WHERE h.UserId = 456
ORDER BY UsageDate DESC;
```

### Check Status Changes
```sql
SELECT 
    FromStatus,
    ToStatus,
    Reason,
    ChangedAt,
    ChangedByUserId
FROM SubscriptionStatusHistory
WHERE SubscriptionId = 'sub_111'
ORDER BY ChangedAt DESC;
```

---

## STRIPE DASHBOARD CHECKS

### Verify Customer
```
Stripe Dashboard → Customers → Search: johndoe@example.com
Check:
  ✓ Customer ID matches Users.StripeCustomerId
  ✓ Payment methods attached
  ✓ Default payment method set
  ✓ Metadata contains userId
```

### Verify Subscription
```
Stripe Dashboard → Subscriptions → Search: sub_stripe_AAA
Check:
  ✓ Status: active
  ✓ Current period matches your DB
  ✓ Metadata contains subscriptionId, planId
  ✓ Next billing date correct
```

### Verify Invoices
```
Stripe Dashboard → Invoices
Check:
  ✓ Invoice ID matches BillingRecords.StripeInvoiceId
  ✓ Amount matches BillingRecords.Amount
  ✓ Status matches (paid, open, etc.)
  ✓ Metadata contains billingRecordId
```

---

## CRITICAL CODE SNIPPETS

### Check if User Has Credits
```csharp
var usage = await _privilegeUsageRepository
    .GetByUserAndPrivilegeAsync(userId, privilegeId);

if (usage.AllowedValue >= requestedQuantity)
{
    // Has credits
}
else
{
    // Need to pay for overage
}
```

### Get Latest Plan (Abuse Prevention)
```csharp
var parentPlanId = subscription.SubscriptionPlan.ParentPlanId 
    ?? subscription.SubscriptionPlan.Id;

var latestPlan = await _subscriptionPlanRepository
    .GetLatestVersionByParentIdAsync(parentPlanId);

var unitCost = latestPlan.PlanPrivileges
    .FirstOrDefault(p => p.PrivilegeId == privilegeId)
    ?.UnitCost ?? 0;
```

### Reset Privileges on Renewal
```csharp
var usages = await _privilegeUsageRepository
    .GetBySubscriptionIdAsync(subscriptionId);

foreach (var usage in usages)
{
    var planPrivilege = await _planPrivilegeRepository
        .GetByPlanAndPrivilegeAsync(planId, usage.PrivilegeId);
    
    usage.AllocatedLimit = planPrivilege.Value;
    usage.UsedValue = 0;
    usage.AllowedValue = planPrivilege.Value;
    usage.ResetAt = DateTime.UtcNow;
    
    await _privilegeUsageRepository.UpdateAsync(usage);
}
```

---

## PRINT & POST THIS REFERENCE!

Keep this guide handy while developing. It provides quick access to:
- ✅ All major flows
- ✅ Database state changes
- ✅ Service call patterns
- ✅ SQL debugging queries
- ✅ Critical code snippets

---

**Document Version:** 1.0  
**Last Updated:** October 17, 2025  
**Purpose:** Quick reference during active development

