# 🏗️ COMPLETE SUBSCRIPTION MANAGEMENT ARCHITECTURE GUIDE
## Part 4: Complete Workflow Diagrams

---

## 🔄 WORKFLOW 1: SUBSCRIPTION CREATION (User Subscribes)

```
┌──────────────────────────────────────────────────────────────────┐
│                   USER SUBSCRIBES TO PLAN                         │
│                      ($280 Base Price)                            │
└──────────────────────────────────────────────────────────────────┘

[CLIENT] User clicks "Subscribe to Standard Plan ($280)"
   │
   ↓
[API] POST /api/subscriptions
   Body: {
     userId: 789,
     planId: "plan-guid",
     billingCycleId: "monthly-guid",
     paymentMethodId: "pm_xxxxx"
   }
   │
   ↓
[CONTROLLER] SubscriptionsController.CreateSubscription()
   │ • Extracts token (authentication)
   │ • Validates request
   ↓
[SERVICE] SubscriptionLifecycleService.CreateSubscriptionAsync()
   │
   ├─→ [VALIDATION PHASE]
   │   ├─→ Get plan from SubscriptionPlanRepository
   │   │   IF not found OR not active → RETURN 404/400
   │   │
   │   ├─→ Check for duplicate subscriptions
   │   │   IF user already has active/paused sub for this plan → RETURN 400
   │   │
   │   ├─→ Get user details from UserService
   │   │
   │   └─→ Validate billing cycle exists
   │
   ├─→ [STRIPE CUSTOMER PHASE]
   │   └─→ StripeService.EnsureStripeCustomerAsync(user)
   │       IF user.StripeCustomerId exists:
   │           RETURN existing ID
   │       ELSE:
   │           ├─→ Stripe API: Customer.Create()
   │           ├─→ Update user.StripeCustomerId
   │           └─→ RETURN new customer ID
   │       Result: "cus_xxxxxxxxxxxxx"
   │
   ├─→ [PAYMENT METHOD VALIDATION]
   │   └─→ StripeService.ValidatePaymentMethodAsync(paymentMethodId)
   │       └─→ Stripe API: PaymentMethod.Get()
   │           IF valid → RETURN true
   │           IF invalid → RETURN false, ABORT
   │
   ├─→ [CREATE STRIPE SUBSCRIPTION] ⭐⭐⭐
   │   └─→ StripeService.CreateSubscriptionAsync(customerId, priceId, paymentMethodId)
   │       └─→ Stripe API: Subscription.Create({
   │             customer: "cus_xxxxx",
   │             items: [{ price: "price_monthly_xxxxx" }],
   │             default_payment_method: "pm_xxxxx"
   │           })
   │           
   │           STRIPE CHARGES PAYMENT METHOD $280! 💳💳💳
   │           
   │           Result: {
   │             id: "sub_xxxxxxxxxxxxx",
   │             status: "active",
   │             current_period_start: 1697500800,
   │             current_period_end: 1700179200
   │           }
   │
   ├─→ [LOCAL DATABASE PHASE]
   │   ├─→ UnitOfWork.BeginTransactionAsync() ⭐
   │   │
   │   ├─→ Create Subscription Entity:
   │   │   {
   │   │     Id: New Guid,
   │   │     UserId: 789,
   │   │     SubscriptionPlanId: plan-guid,
   │   │     Status: "Active" (or "TrialActive"),
   │   │     CurrentPrice: $280,
   │   │     StartDate: Now,
   │   │     NextBillingDate: Now + 30 days,
   │   │     StripeSubscriptionId: "sub_xxxxx", ⭐
   │   │     StripeCustomerId: "cus_xxxxx", ⭐
   │   │     StripePriceId: "price_xxxxx", ⭐
   │   │     PaymentMethodId: "pm_xxxxx",
   │   │     IsTrialSubscription: false,
   │   │     AutoRenew: true,
   │   │     CreatedBy: 789,
   │   │     CreatedDate: Now
   │   │   }
   │   │
   │   ├─→ SubscriptionRepository.CreateSubscriptionAsync()
   │   │   └─→ EF Core: INSERT INTO Subscriptions
   │   │
   │   ├─→ Record Status Change:
   │   │   └─→ StatusHistoryRepository.AddAsync({
   │   │         FromStatus: null,
   │   │         ToStatus: "Active",
   │   │         Reason: "Subscription created",
   │   │         ChangedAt: Now
   │   │       })
   │   │
   │   └─→ UnitOfWork.CommitTransactionAsync() ⭐
   │       └─→ EF Core: COMMIT
   │
   ├─→ [INITIAL BILLING]
   │   └─→ SubscriptionBillingService.CreateSubscriptionBillingAsync()
   │       └─→ BillingRepository.CreateAsync({
   │             Type: BillingRecord.BillingType.Subscription,
   │             Amount: $280,
   │             Status: Pending → Paid (Stripe already charged),
   │             Description: "Initial billing for Standard Health Plan"
   │           })
   │
   └─→ [NOTIFICATIONS]
       ├─→ NotificationService.SendSubscriptionConfirmationAsync()
       └─→ NotificationService.SendWelcomeEmailAsync()

[RESULT] User subscribed successfully!
   • Local subscription created
   • Stripe subscription active
   • $280 charged
   • Privileges initialized (on first use)
   • Status: "Active"
   • NextBillingDate: 30 days from now
```

---

## 🔄 WORKFLOW 2: USE INCLUDED PRIVILEGE (FREE)

```
┌──────────────────────────────────────────────────────────────────┐
│              USER USES INCLUDED PRIVILEGE                         │
│                  (1st, 2nd, 3rd, 4th, 5th Consultation)          │
│                        NO CHARGE                                  │
└──────────────────────────────────────────────────────────────────┘

[APPLICATION] User books consultation
   │
   ↓
[BUSINESS LOGIC] PrivilegeService.UsePrivilegeAsync(subscriptionId, "Teleconsultation", 1)
   │
   ├─→ [GET PLAN PRIVILEGE]
   │   └─→ GetPlanPrivilegeAsync(subscriptionId, "Teleconsultation")
   │       ├─→ SubscriptionRepository.GetByIdAsync(subscriptionId)
   │       │   └─→ Get subscription.SubscriptionPlanId
   │       │
   │       └─→ SubscriptionPlanPrivilegeRepository.GetByPlanIdAsync()
   │           └─→ Find privilege where Privilege.Name = "Teleconsultation"
   │               Result: {
   │                 Value: 5 (limit),
   │                 UnitCost: $20.00,
   │                 DailyLimit: null,
   │                 WeeklyLimit: null
   │               }
   │
   ├─→ [CHECK IF DISABLED]
   │   IF planPrivilege.Value == 0:
   │       RETURN FALSE (disabled privilege)
   │
   ├─→ [CHECK TIME-BASED LIMITS]
   │   └─→ CheckTimeBasedLimitsAsync()
   │       IF daily/weekly/monthly exceeded:
   │           RETURN FALSE (time limit exceeded)
   │
   ├─→ [CHECK QUANTITY LIMIT] ⭐⭐⭐
   │   └─→ GetRemainingPrivilegeAsync(subscriptionId, "Teleconsultation")
   │       ├─→ UserSubscriptionPrivilegeUsageRepository.GetBySubscriptionIdAsync()
   │       │   └─→ Get usage record for this privilege
   │       │       Result: {
   │       │         UsedValue: 4,  // Used 4 so far
   │       │         AllowedValue: 5  // Limit is 5
   │       │       }
   │       │
   │       └─→ remaining = Math.Max(0, AllowedValue - UsedValue)
   │           = Math.Max(0, 5 - 4) = 1
   │
   │   CHECK: remaining >= amount?
   │   1 >= 1 → TRUE ✓ ALLOWED
   │
   ├─→ [UPDATE USAGE] ⭐
   │   └─→ Get or create UserSubscriptionPrivilegeUsage
   │       IF first use:
   │           Create new record {
   │             SubscriptionId: subscriptionId,
   │             SubscriptionPlanPrivilegeId: planPrivilege.Id,
   │             UsedValue: 1,  // Start with 1
   │             AllowedValue: 5,  // From plan
   │             UsagePeriodStart: Now,
   │             UsagePeriodEnd: Now + 30 days,
   │             LastUsedAt: Now
   │           }
   │       ELSE:
   │           Update existing {
   │             UsedValue: 4 → 5 ⭐
   │             LastUsedAt: Now
   │           }
   │       └─→ UserSubscriptionPrivilegeUsageRepository.UpdateAsync()
   │           └─→ EF Core: UPDATE UserSubscriptionPrivilegeUsages
   │
   ├─→ [RECORD USAGE HISTORY]
   │   └─→ AddUsageHistoryAsync(usageId, amount=1)
   │       └─→ PrivilegeUsageHistoryRepository.AddAsync({
   │             UserSubscriptionPrivilegeUsageId: usage.Id,
   │             UsageAmount: 1,
   │             UsageTimestamp: Now,
   │             CreatedBy: userId
   │           })
   │
   └─→ RETURN TRUE ✓

[RESULT] Privilege used successfully
   • UsedValue incremented: 4 → 5
   • RemainingValue: 0 (computed: 5-5)
   • NO BILLING RECORD CREATED ⭐
   • NO PAYMENT CHARGED ⭐
   • Only usage tracking updated
```

---

## 🔄 WORKFLOW 3: EXCEED LIMIT & PURCHASE CREDITS (UPFRONT PAYMENT)

```
┌──────────────────────────────────────────────────────────────────┐
│         USER TRIES 6TH CONSULTATION (LIMIT IS 5)                 │
│                  REQUIRES UPFRONT PAYMENT                         │
└──────────────────────────────────────────────────────────────────┘

[APPLICATION] User tries to book 6th consultation
   │
   ↓
[BUSINESS LOGIC] PrivilegeService.UsePrivilegeAsync(subscriptionId, "Teleconsultation", 1)
   │
   ├─→ GetRemainingPrivilegeAsync()
   │   └─→ AllowedValue = 5, UsedValue = 5
   │       remaining = 5 - 5 = 0
   │
   ├─→ CHECK: remaining >= amount?
   │   0 >= 1 → FALSE ❌
   │
   └─→ RETURN FALSE ⭐⭐⭐
       (Privilege denied - insufficient credits!)

[APPLICATION] Receives false, checks availability
   │
   ↓
[API] GET /api/subscriptions/{id}/check-privilege/Teleconsultation?requestedAmount=1
   │
   ↓
[SERVICE] PrivilegeService.CheckPrivilegeAvailabilityAsync()
   │
   ├─→ GetRemainingPrivilegeAsync() → remaining = 0
   │
   ├─→ IF remaining >= requested (0 >= 1):
   │     RETURN 200 OK { available: true }
   │
   └─→ ELSE (LIMIT EXCEEDED):
       shortfall = requested - remaining = 1 - 0 = 1
       requiredPayment = shortfall × unitCost = 1 × $20 = $20
       
       RETURN 402 Payment Required ⭐⭐⭐
       {
         "data": {
           "available": false,
           "limitExceeded": true,
           "privilegeName": "Teleconsultation",
           "remaining": 0,
           "requested": 1,
           "shortfall": 1,
           "unitCost": 20.00,
           "requiredPayment": 20.00,
           "message": "Purchase 1 additional credit for $20.00",
           "purchaseEndpoint": "/api/subscriptions/{id}/purchase-credits"
         },
         "statusCode": 402
       }

[CLIENT] Displays payment modal:
   ╔════════════════════════════════════╗
   ║  Insufficient Credits              ║
   ║                                    ║
   ║  You need 1 more Teleconsultation  ║
   ║  Cost: $20.00                      ║
   ║                                    ║
   ║  [Cancel]  [Pay $20.00 Now]        ║
   ╚════════════════════════════════════╝
   │
   ↓
[CLIENT] User clicks "Pay $20.00 Now"
   │
   ↓
[API] POST /api/subscriptions/{id}/purchase-credits
   Body: {
     privilegeName: "Teleconsultation",
     quantity: 1,
     paymentMethodId: "pm_xxxxx"
   }
   │
   ↓
[CONTROLLER] SubscriptionsController.PurchaseAdditionalCredits()
   │ • Validates subscription ID
   │ • Validates DTO
   ↓
[SERVICE] SubscriptionService.PurchaseAdditionalCreditsAsync()
   │
   ├─→ [VALIDATION PHASE]
   │   ├─→ Get subscription → Verify exists
   │   ├─→ Verify status is Active/TrialActive
   │   ├─→ Verify user is owner or admin
   │   ├─→ Get privilege config from plan
   │   ├─→ Verify privilege not disabled
   │   └─→ Get current usage record
   │
   ├─→ [COST CALCULATION]
   │   └─→ totalCost = quantity × unitCost
   │       = 1 × $20 = $20
   │       IF totalCost <= 0 → RETURN error
   │
   ├─→ [PAYMENT METHOD VALIDATION]
   │   └─→ StripeService.ValidatePaymentMethodAsync("pm_xxxxx")
   │       IF invalid → RETURN error
   │
   ├─→ [TRANSACTION PHASE] ⭐⭐⭐
   │   │
   │   ├─→ UnitOfWork.BeginTransactionAsync() 🔒
   │   │   └─→ EF Core: BEGIN TRANSACTION
   │   │
   │   ├─→ [CREATE BILLING RECORD]
   │   │   └─→ SubscriptionBillingService.CreateBillingRecordAsync({
   │   │         UserId: 789,
   │   │         SubscriptionId: sub-guid,
   │   │         Amount: $20,
   │   │         TotalAmount: $20,
   │   │         Type: BillingRecord.BillingType.Overage, ⭐
   │   │         Status: BillingRecord.BillingStatus.Pending,
   │   │         Description: "Purchase 1 additional Teleconsultation credits @ $20.00 each",
   │   │         BillingDate: Now,
   │   │         DueDate: Now,  // Immediate!
   │   │         IsRecurring: false
   │   │       })
   │   │       └─→ BillingRepository.CreateAsync()
   │   │           └─→ EF Core: INSERT INTO BillingRecords
   │   │               Result: billingRecordId
   │   │
   │   ├─→ [PROCESS PAYMENT IMMEDIATELY] ⭐⭐⭐💳
   │   │   └─→ SubscriptionBillingService.ProcessPaymentAsync(billingRecordId)
   │   │       └─→ PaymentService.ProcessPaymentAsync(billingRecordId)
   │   │           └─→ StripeBillingService.ProcessStripePaymentAsync(billingRecordId)
   │   │               │
   │   │               ├─→ Get billing record
   │   │               ├─→ Create SubscriptionPayment record
   │   │               │
   │   │               ├─→ Stripe API: PaymentIntent.Create({
   │   │               │     amount: 2000,  // $20.00 in cents
   │   │               │     currency: "usd",
   │   │               │     customer: "cus_xxxxx",
   │   │               │     payment_method: "pm_xxxxx",
   │   │               │     off_session: true,
   │   │               │     confirm: true
   │   │               │   })
   │   │               │
   │   │               │   STRIPE CHARGES CARD $20! 💳
   │   │               │
   │   │               ├─→ IF Stripe returns SUCCESS:
   │   │               │     └─→ Update billing: Status = "Paid", PaidAt = Now
   │   │               │         Update payment: Status = "Succeeded"
   │   │               │         RETURN 200 OK
   │   │               │
   │   │               └─→ IF Stripe returns FAILURE:
   │   │                     └─→ Update billing: Status = "Failed"
   │   │                         Update payment: Status = "Failed"
   │   │                         RETURN 400 Error
   │   │
   │   ├─→ [CHECK PAYMENT RESULT]
   │   │   IF paymentResult.StatusCode != 200:
   │   │       ├─→ UnitOfWork.RollbackTransactionAsync() ⭐
   │   │       │   └─→ EF Core: ROLLBACK
   │   │       │       (Billing record removed!)
   │   │       │       (Payment record removed!)
   │   │       │
   │   │       └─→ RETURN {
   │   │             paymentFailed: true,
   │   │             creditsAdded: 0,
   │   │             message: "Payment failed. Credits NOT added."
   │   │           }
   │   │           EXIT ❌
   │   │
   │   ├─→ [PAYMENT SUCCESSFUL - ADD CREDITS] ⭐⭐⭐
   │   │   └─→ Get usage record
   │   │       previousAllowedValue = 5
   │   │       usage.AllowedValue += quantity  // 5 + 1 = 6 ⭐
   │   │       usage.UpdatedBy = userId
   │   │       usage.UpdatedDate = Now
   │   │       
   │   │       UserSubscriptionPrivilegeUsageRepository.UpdateAsync(usage)
   │   │       └─→ EF Core: UPDATE UserSubscriptionPrivilegeUsages
   │   │           SET AllowedValue = 6, UpdatedDate = Now
   │   │           WHERE Id = usage.Id
   │   │
   │   │       New state:
   │   │         UsedValue: 5
   │   │         AllowedValue: 6 ⭐
   │   │         RemainingValue: 1 (computed: 6-5)
   │   │
   │   ├─→ UnitOfWork.CommitTransactionAsync() ⭐
   │   │   └─→ EF Core: COMMIT
   │   │       • Billing record persisted
   │   │       • Payment record persisted
   │   │       • Usage record persisted
   │   │
   │   └─→ [SEND NOTIFICATION]
   │       └─→ SubscriptionNotificationService.SendBulkNotificationAsync()
   │           "You've successfully purchased 1 additional Teleconsultation credit for $20"
   │
   └─→ RETURN SUCCESS {
         creditsAdded: 1,
         totalPaid: $20.00,
         newLimit: 6,
         newRemaining: 1,
         billingRecordId: bill-guid
       }

[RESULT] Credits added successfully!
   • $20 charged via Stripe
   • BillingRecord created (Type=Overage, Status=Paid)
   • SubscriptionPayment created (Type=Upfront, Status=Succeeded)
   • AllowedValue increased: 5 → 6
   • User can now use 6th consultation

[APPLICATION] Now try to use 6th consultation again
   │
   ↓
[BUSINESS LOGIC] PrivilegeService.UsePrivilegeAsync(subscriptionId, "Teleconsultation", 1)
   │
   ├─→ GetRemainingPrivilegeAsync()
   │   → AllowedValue = 6, UsedValue = 5
   │   → remaining = 6 - 5 = 1
   │
   ├─→ CHECK: remaining >= amount?
   │   1 >= 1 → TRUE ✓
   │
   ├─→ UPDATE USAGE:
   │   UsedValue: 5 → 6
   │   Save to database
   │
   └─→ RETURN TRUE ✓

[RESULT] 6th consultation allowed!
   • UsedValue: 6
   • AllowedValue: 6
   • RemainingValue: 0
   • NO additional billing (already paid)
```

---

## 🔄 WORKFLOW 4: MONTHLY BILLING & RENEWAL

```
┌──────────────────────────────────────────────────────────────────┐
│               AUTOMATED MONTHLY BILLING & RENEWAL                 │
│                    (Runs Daily at 2:00 AM)                        │
└──────────────────────────────────────────────────────────────────┘

[BACKGROUND SERVICE] AutomatedBillingBackgroundService
   │ • Runs at scheduled time (configured in appsettings.json)
   │ • Executes: AutomatedBillingService.ProcessRecurringBillingAsync()
   ↓
[SERVICE] AutomatedBillingService.ProcessRecurringBillingAsync()
   │
   ├─→ [GET DUE SUBSCRIPTIONS]
   │   └─→ SubscriptionRepository.GetActiveSubscriptionsAsync()
   │       Filter: NextBillingDate <= Today
   │       Result: List of subscriptions needing billing
   │
   └─→ FOR EACH subscription:
       │
       ├─→ [CALCULATE OVERAGE] ⭐
       │   └─→ CalculateOverageChargeAsync(subscription)
       │       FOR EACH privilege in plan:
       │         Get actual usage from UserSubscriptionPrivilegeUsage
       │         IF usage > limit:
       │             overage = usage - limit
       │             charge = overage × unitCost
       │             totalOverage += charge
       │       
       │       Example:
       │         Consultations: Used=7, Limit=5
       │           Overage: (7-5) × $20 = $40
       │         Medications: Used=4, Limit=3
       │           Overage: (4-3) × $50 = $50
       │         Total: $90
       │       
       │       BUT WITH UPFRONT PAYMENT:
       │         All overage already paid when purchasing credits!
       │         Pending overage = $0 ⭐
       │
       ├─→ [CALCULATE BILLING AMOUNT]
       │   totalAmount = basePlantPrice + pendingOverage
       │   = $280 + $0 = $280 ⭐
       │
       ├─→ [CREATE BILLING RECORD]
       │   └─→ SubscriptionBillingService.CreateSubscriptionBillingAsync({
       │         Amount: $280,
       │         Type: BillingRecord.BillingType.Subscription,
       │         Description: "Monthly billing for Standard Health Plan"
       │       })
       │
       ├─→ [PROCESS PAYMENT]
       │   └─→ PaymentService.ProcessPaymentAsync()
       │       └─→ Stripe charges $280
       │
       ├─→ [RENEWAL & RESET] ⭐
       │   └─→ SubscriptionBillingService.ProcessSubscriptionRenewalAsync()
       │       │
       │       ├─→ BEGIN TRANSACTION
       │       │
       │       ├─→ FOR EACH privilege usage:
       │       │     UsedValue = 0 ⭐⭐⭐ (RESET!)
       │       │     AllowedValue = plan default (5, 3)
       │       │     ResetAt = Now
       │       │     Save
       │       │
       │       ├─→ Update subscription:
       │       │     NextBillingDate = NextBillingDate + 30 days
       │       │     LastBillingDate = Now
       │       │
       │       └─→ COMMIT TRANSACTION
       │
       └─→ [SEND NOTIFICATION]
           └─→ Send billing email: "Your subscription has been renewed"

[RESULT] Monthly billing complete
   • Base subscription: $280 charged
   • Overage: $0 (already paid upfront!)
   • All privilege usage reset to 0
   • NextBillingDate: 30 days later
   • User starts fresh month
```

---

## 🔄 WORKFLOW 5: STRIPE WEBHOOK SYNCHRONIZATION

```
┌──────────────────────────────────────────────────────────────────┐
│              STRIPE WEBHOOK EVENT PROCESSING                      │
│                (Keeps Local DB in Sync with Stripe)               │
└──────────────────────────────────────────────────────────────────┘

[STRIPE] Event occurs (e.g., payment succeeds)
   │
   ↓
[STRIPE] Sends webhook POST to:
   https://yourdomain.com/api/stripewebhook/webhook
   Headers: {
     Stripe-Signature: "t=timestamp,v1=signature"
   }
   Body: {
     id: "evt_xxxxxxxxxxxxx",
     type: "payment_intent.succeeded",
     data: { object: { ... payment intent details ... } }
   }
   │
   ↓
[API] StripeWebhookController.HandleWebhook()
   │
   ├─→ [SIGNATURE VALIDATION] 🔒
   │   └─→ EventUtility.ConstructEvent(json, signature, secret)
   │       IF signature invalid → RETURN 400 (reject webhook)
   │       IF signature valid → Continue
   │
   ├─→ [IDEMPOTENCY CHECK] ⭐
   │   └─→ WebhookIdempotencyService.CheckIdempotencyAsync(eventId)
   │       └─→ ProcessedWebhookEventRepository.GetByEventIdAsync(eventId)
   │           IF event already processed:
   │               RETURN 200 "Already processed" (prevent duplicate)
   │           ELSE:
   │               Mark as processing, continue
   │
   ├─→ [EVENT ROUTING]
   │   SWITCH (stripeEvent.Type):
   │   
   │   CASE "customer.subscription.created":
   │       └─→ HandleSubscriptionCreated(event)
   │           ├─→ Extract subscription data
   │           ├─→ Find local subscription by StripeSubscriptionId
   │           ├─→ IF not found, create local subscription
   │           └─→ Update status, sync data
   │   
   │   CASE "customer.subscription.updated":
   │       └─→ HandleSubscriptionUpdated(event)
   │           ├─→ Find local subscription
   │           ├─→ Update status from Stripe
   │           ├─→ Update price if changed
   │           └─→ Sync all fields
   │   
   │   CASE "invoice.payment_succeeded": ⭐
   │       └─→ HandleInvoicePaymentSucceeded(event)
   │           ├─→ Find billing record by StripeInvoiceId
   │           ├─→ Update billing status: "Paid"
   │           ├─→ Update PaidAt: Now
   │           ├─→ Update subscription: LastPaymentDate = Now
   │           └─→ Send payment confirmation email
   │   
   │   CASE "payment_intent.succeeded": ⭐
   │       └─→ HandlePaymentIntentSucceeded(event)
   │           ├─→ Find billing record by StripePaymentIntentId
   │           ├─→ Update billing status: "Paid"
   │           ├─→ Update payment status: "Succeeded"
   │           └─→ Log success
   │   
   │   CASE "invoice.payment_failed":
   │       └─→ HandleInvoicePaymentFailed(event)
   │           ├─→ Find subscription
   │           ├─→ Increment FailedPaymentAttempts
   │           ├─→ Update subscription status: "PaymentFailed"
   │           └─→ Send payment failure notification
   │   
   │   CASE "payment_intent.payment_failed":
   │       └─→ HandlePaymentIntentFailed(event)
   │           └─→ Update billing/payment status: "Failed"
   │   
   │   DEFAULT:
   │       └─→ Log unhandled event type
   │
   ├─→ [MARK AS PROCESSED]
   │   └─→ WebhookIdempotencyService.MarkAsProcessedAsync(eventId, duration)
   │       └─→ ProcessedWebhookEventRepository.UpdateAsync({
   │             EventId: "evt_xxxxx",
   │             ProcessedAt: Now,
   │             ProcessingTimeMs: 234,
   │             Status: "Processed"
   │           })
   │
   └─→ RETURN 200 OK

[RESULT] Local database synchronized with Stripe
   • Payment status updated
   • Subscription status synchronized
   • User notified if needed
   • Idempotency prevents duplicate processing
```

---

## 📊 BUSINESS LOGIC RULES

### **Rule 1: Payment Before Access** ⭐

**Where:** `SubscriptionService.PurchaseAdditionalCreditsAsync()`

```
BEGIN TRANSACTION
  Create billing record
  Process payment ← HAPPENS FIRST (Line 1938)
  IF payment succeeds:
      Add credits ← ONLY IF PAID (Line 1973)
      COMMIT
  ELSE:
      ROLLBACK ← NO credits if not paid
END TRANSACTION
```

**Guarantee:** User CANNOT get credits without paying

---

### **Rule 2: No Billing for Included Privileges** ⭐

**Where:** `PrivilegeService.UsePrivilegeAsync()`

```
IF remaining >= amount:
    Update UsedValue
    Record history
    // NO billing service calls!
    // NO payment service calls!
    RETURN true
```

**Guarantee:** User NEVER charged for included privileges

---

### **Rule 3: Block Unauthorized Usage** ⭐

**Where:** `PrivilegeService.UsePrivilegeAsync()`

```
remaining = AllowedValue - UsedValue
IF remaining < amount:
    RETURN FALSE  ← BLOCKS immediately
    // Never reaches database update!
```

**Guarantee:** User CANNOT exceed limits without paying

---

### **Rule 4: Renewal Resets Usage** ⭐

**Where:** `SubscriptionBillingService.ProcessSubscriptionRenewalAsync()`

```
BEGIN TRANSACTION
FOR EACH privilege:
    UsedValue = 0  ← RESET
    ResetAt = Now
COMMIT TRANSACTION
```

**Guarantee:** Fresh start each billing period

---

### **Rule 5: Plan Versioning** ⭐

**Where:** `SubscriptionPlanService.UpdatePlanAsync()`

```
IF plan.Price changed:
    oldPlan.IsLatestVersion = false
    newPlan = Clone(oldPlan)
    newPlan.VersionNumber = oldPlan.VersionNumber + 1
    newPlan.IsLatestVersion = true
    newPlan.ParentPlanId = oldPlan.Id
    CREATE newPlan
    
    Existing users stay on oldPlan
    New users get newPlan
```

**Guarantee:** Existing users not affected by price changes

---

## 🎯 KEY TAKEAWAYS - PART 4

1. **8 workflows** cover complete subscription management
2. **SubscriptionLifecycleService** handles all state transitions
3. **SubscriptionService.PurchaseAdditionalCreditsAsync()** is 297-line transaction-safe implementation
4. **PrivilegeService.UsePrivilegeAsync()** NEVER creates billing records
5. **Payment happens BEFORE credits** (Line 1938 → 1973)
6. **Stripe webhooks** keep local DB synchronized
7. **Idempotency** prevents duplicate processing
8. **Automated billing** runs daily for renewals

---

**Continue to Part 5 for Stripe integration details...**

