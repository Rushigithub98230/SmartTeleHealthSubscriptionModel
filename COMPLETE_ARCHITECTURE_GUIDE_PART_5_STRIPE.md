# 🏗️ COMPLETE SUBSCRIPTION MANAGEMENT ARCHITECTURE GUIDE
## Part 5: Stripe Integration & Synchronization

---

## 💳 STRIPE INTEGRATION OVERVIEW

Your system has **complete bidirectional synchronization** with Stripe for:
- Customer management
- Subscription lifecycle
- Payment processing
- Product & pricing
- Real-time event handling

---

## 🔗 STRIPE DATA MAPPING

### **Local Entity → Stripe Object:**

| Local Entity | Stripe Object | Sync Field | Example |
|--------------|---------------|------------|---------|
| **User** | Customer | `StripeCustomerId` | "cus_xxxxx" |
| **SubscriptionPlan** | Product | `StripeProductId` | "prod_xxxxx" |
| **SubscriptionPlan** | Price (Monthly) | `StripeMonthlyPriceId` | "price_month_xxxxx" |
| **SubscriptionPlan** | Price (Quarterly) | `StripeQuarterlyPriceId` | "price_quarter_xxxxx" |
| **SubscriptionPlan** | Price (Annual) | `StripeAnnualPriceId` | "price_annual_xxxxx" |
| **Subscription** | Subscription | `StripeSubscriptionId` | "sub_xxxxx" |
| **Subscription** | Customer | `StripeCustomerId` | "cus_xxxxx" |
| **Subscription** | Price | `StripePriceId` | "price_xxxxx" |
| **Subscription** | PaymentMethod | `PaymentMethodId` | "pm_xxxxx" |
| **BillingRecord** | Invoice | `StripeInvoiceId` | "in_xxxxx" |
| **BillingRecord** | PaymentIntent | `StripePaymentIntentId` | "pi_xxxxx" |
| **SubscriptionPayment** | PaymentIntent | `StripePaymentIntentId` | "pi_xxxxx" |
| **SubscriptionPayment** | Invoice | `StripeInvoiceId` | "in_xxxxx" |

---

## 🔄 STRIPE INTEGRATION FLOWS

### **FLOW 1: Create Subscription Plan in Stripe**

```
Admin creates plan in system
   ↓
SubscriptionPlanService.CreatePlanAsync()
   │
   ├─→ [CREATE STRIPE PRODUCT]
   │   └─→ StripeService.CreateProductAsync("Standard Health Plan", description)
   │       └─→ Stripe API: Product.Create({
   │             name: "Standard Health Plan",
   │             description: "5 consultations, 3 medications",
   │             metadata: {
   │               plan_id: "plan-guid",
   │               created_at: "2025-10-16",
   │               source: "smart_telehealth"
   │             }
   │           })
   │           Result: "prod_xxxxxxxxxxxxx"
   │
   ├─→ [CREATE STRIPE PRICES]
   │   │
   │   ├─→ Monthly Price:
   │   │   └─→ StripeService.CreatePriceAsync("prod_xxx", $280, "usd", "month", 1)
   │   │       └─→ Stripe API: Price.Create({
   │   │             product: "prod_xxx",
   │   │             unit_amount: 28000,  // $280 in cents
   │   │             currency: "usd",
   │   │             recurring: { interval: "month", interval_count: 1 }
   │   │           })
   │   │           Result: "price_monthly_xxxxx"
   │   │
   │   ├─→ Quarterly Price:
   │   │   └─→ StripeService.CreatePriceAsync("prod_xxx", $252, "usd", "month", 3)
   │   │       Result: "price_quarterly_xxxxx" (10% discount)
   │   │
   │   └─→ Annual Price:
   │       └─→ StripeService.CreatePriceAsync("prod_xxx", $224, "usd", "year", 1)
   │           Result: "price_annual_xxxxx" (20% discount)
   │
   └─→ [SAVE TO DATABASE]
       └─→ SubscriptionPlanRepository.CreateAsync({
             Name: "Standard Health Plan",
             Price: $280,
             StripeProductId: "prod_xxx", ⭐
             StripeMonthlyPriceId: "price_monthly_xxx", ⭐
             StripeQuarterlyPriceId: "price_quarterly_xxx", ⭐
             StripeAnnualPriceId: "price_annual_xxx" ⭐
           })

[RESULT]
   • Stripe Product created
   • Stripe Prices created (3 intervals)
   • Local plan has Stripe IDs for sync
```

---

### **FLOW 2: User Subscribes (Stripe Charges Base Price)**

```
User subscribes to plan
   ↓
SubscriptionLifecycleService.CreateSubscriptionAsync()
   │
   ├─→ [ENSURE STRIPE CUSTOMER]
   │   └─→ StripeService.EnsureStripeCustomerAsync(user)
   │       IF user.StripeCustomerId exists:
   │           RETURN "cus_xxxxx"
   │       ELSE:
   │           └─→ Stripe API: Customer.Create({
   │                 email: user.Email,
   │                 name: user.FullName,
   │                 metadata: { user_id: 789 }
   │               })
   │               Result: "cus_xxxxx"
   │               Update user.StripeCustomerId = "cus_xxxxx"
   │               RETURN "cus_xxxxx"
   │
   ├─→ [VALIDATE PAYMENT METHOD]
   │   └─→ StripeService.ValidatePaymentMethodAsync("pm_xxxxx")
   │       └─→ Stripe API: PaymentMethod.Get("pm_xxxxx")
   │           IF exists and usable → RETURN true
   │           ELSE → RETURN false, ABORT
   │
   ├─→ [CREATE STRIPE SUBSCRIPTION] ⭐⭐⭐
   │   └─→ StripeService.CreateSubscriptionAsync(
   │         customerId: "cus_xxxxx",
   │         priceId: "price_monthly_xxxxx",
   │         paymentMethodId: "pm_xxxxx"
   │       )
   │       └─→ Stripe API: Subscription.Create({
   │             customer: "cus_xxxxx",
   │             items: [{
   │               price: "price_monthly_xxxxx"  // $280/month
   │             }],
   │             default_payment_method: "pm_xxxxx",
   │             payment_behavior: "default_incomplete",
   │             expand: ["latest_invoice.payment_intent"]
   │           })
   │           
   │           🔥 STRIPE AUTOMATICALLY CHARGES $280 🔥
   │           
   │           Result: {
   │             id: "sub_xxxxxxxxxxxxx",
   │             status: "active",
   │             customer: "cus_xxxxx",
   │             current_period_start: 1697500800,
   │             current_period_end: 1700179200,
   │             latest_invoice: {
   │               id: "in_xxxxx",
   │               payment_intent: {
   │                 id: "pi_xxxxx",
   │                 status: "succeeded",
   │                 amount: 28000  // $280.00
   │               }
   │             }
   │           }
   │
   ├─→ [SAVE LOCAL SUBSCRIPTION]
   │   └─→ SubscriptionRepository.CreateAsync({
   │         UserId: 789,
   │         SubscriptionPlanId: plan-guid,
   │         Status: "Active",
   │         CurrentPrice: $280,
   │         StripeSubscriptionId: "sub_xxxxx", ⭐
   │         StripeCustomerId: "cus_xxxxx", ⭐
   │         StripePriceId: "price_monthly_xxxxx", ⭐
   │         PaymentMethodId: "pm_xxxxx"
   │       })
   │
   └─→ [STRIPE SENDS WEBHOOK]
       Later (1-5 seconds):
       └─→ Webhook: "customer.subscription.created"
           └─→ StripeWebhookController confirms creation

[RESULT]
   • Stripe customer created/confirmed
   • Stripe subscription created
   • $280 charged via Stripe
   • Local subscription created
   • Synchronized via StripeSubscriptionId
```

---

### **FLOW 3: Purchase Extra Credits (Upfront Stripe Charge)**

```
User purchases 1 extra consultation for $20
   ↓
SubscriptionService.PurchaseAdditionalCreditsAsync()
   │
   ├─→ [VALIDATE PAYMENT METHOD]
   │   └─→ StripeService.ValidatePaymentMethodAsync("pm_xxxxx")
   │       └─→ Stripe API: PaymentMethod.Get()
   │           IF invalid → ABORT
   │
   ├─→ [CREATE LOCAL BILLING]
   │   └─→ BillingRepository.CreateAsync({
   │         Type: Overage,
   │         Amount: $20,
   │         Status: Pending
   │       })
   │       Result: billing record created
   │
   ├─→ [PROCESS PAYMENT VIA STRIPE] ⭐⭐⭐
   │   └─→ PaymentService.ProcessPaymentAsync(billingRecordId)
   │       └─→ StripeBillingService.ProcessStripePaymentAsync()
   │           │
   │           ├─→ Stripe API: PaymentIntent.Create({
   │           │     amount: 2000,  // $20.00 in cents
   │           │     currency: "usd",
   │           │     customer: "cus_xxxxx",
   │           │     payment_method: "pm_xxxxx",
   │           │     off_session: true,  // Charge without user present
   │           │     confirm: true,  // Charge immediately
   │           │     metadata: {
   │           │       billing_record_id: "bill-guid",
   │           │       privilege_name: "Teleconsultation",
   │           │       quantity: 1
   │           │     }
   │           │   })
   │           │   
   │           │   🔥 STRIPE CHARGES CARD $20 IMMEDIATELY 🔥
   │           │   
   │           │   Result: {
   │           │     id: "pi_xxxxxxxxxxxxx",
   │           │     status: "succeeded",
   │           │     amount: 2000,
   │           │     amount_received: 2000,
   │           │     charges: {
   │           │       data: [{
   │           │         id: "ch_xxxxx",
   │           │         amount: 2000,
   │           │         paid: true,
   │           │         receipt_url: "https://..."
   │           │       }]
   │           │     }
   │           │   }
   │           │
   │           ├─→ Update BillingRecord:
   │           │     Status: "Paid"
   │           │     PaidAt: Now
   │           │     StripePaymentIntentId: "pi_xxxxx"
   │           │
   │           ├─→ Create SubscriptionPayment:
   │           │     Amount: $20
   │           │     Status: "Succeeded"
   │           │     Type: "Upfront"
   │           │     StripePaymentIntentId: "pi_xxxxx"
   │           │
   │           └─→ RETURN 200 OK
   │
   ├─→ [ADD CREDITS AFTER PAYMENT]
   │   └─→ usage.AllowedValue += 1  // 5 → 6
   │       UserSubscriptionPrivilegeUsageRepository.UpdateAsync()
   │
   └─→ [STRIPE SENDS WEBHOOK]
       Later:
       └─→ Webhook: "payment_intent.succeeded"
           └─→ Confirms payment in local DB

[RESULT]
   • $20 charged via Stripe
   • Local billing record: Type=Overage, Status=Paid
   • Credits added: AllowedValue 5 → 6
   • User can now use 6th consultation
```

---

### **FLOW 4: Stripe Webhook Synchronization**

```
┌────────────────────────────────────────────────────────────┐
│           STRIPE → LOCAL DATABASE SYNCHRONIZATION           │
│              (Real-time Event Processing)                   │
└────────────────────────────────────────────────────────────┘

[STRIPE] Event occurs (payment succeeds, subscription updates, etc.)
   │
   ↓
[STRIPE] HTTP POST → https://yourdomain.com/api/stripewebhook/webhook
   Headers:
     Stripe-Signature: "t=1697500800,v1=abc123..."
   Body:
     {
       "id": "evt_xxxxxxxxxxxxx",
       "type": "payment_intent.succeeded",
       "data": {
         "object": {
           "id": "pi_xxxxx",
           "amount": 2000,
           "status": "succeeded",
           "metadata": {
             "billing_record_id": "bill-guid"
           }
         }
       }
     }
   │
   ↓
[API] StripeWebhookController.HandleWebhook()
   │
   ├─→ [VERIFY SIGNATURE] 🔒
   │   └─→ Stripe SDK: EventUtility.ConstructEvent(
   │         json,
   │         signature,
   │         webhookSecret
   │       )
   │       Uses: HMAC-SHA256 verification
   │       IF signature invalid → Reject (return 400)
   │       IF signature valid → Continue
   │
   ├─→ [CHECK IDEMPOTENCY]
   │   └─→ WebhookIdempotencyService.CheckIdempotencyAsync(eventId)
   │       └─→ Query ProcessedWebhookEvents table
   │           IF eventId exists and processed:
   │               RETURN 200 "Already processed"
   │           ELSE:
   │               INSERT { EventId, ReceivedAt, Status: "Processing" }
   │               Continue
   │
   ├─→ [ROUTE TO HANDLER]
   │   SWITCH (event.type):
   │
   │   CASE "payment_intent.succeeded":
   │       └─→ HandlePaymentIntentSucceeded(event)
   │           ├─→ Extract: paymentIntent = event.data.object
   │           ├─→ Get billing_record_id from metadata
   │           ├─→ BillingRepository.GetByIdAsync(billingRecordId)
   │           ├─→ IF billing found:
   │           │     Update:
   │           │       Status: "Paid"
   │           │       PaidAt: Now
   │           │       StripePaymentIntentId: "pi_xxxxx"
   │           │     Save
   │           ├─→ SubscriptionPaymentRepository.FindByPaymentIntentId()
   │           └─→ IF payment found:
   │                 Update: Status: "Succeeded"
   │
   │   CASE "invoice.payment_succeeded":
   │       └─→ HandleInvoicePaymentSucceeded(event)
   │           ├─→ Extract invoice details
   │           ├─→ Find billing by StripeInvoiceId
   │           ├─→ Update billing status: "Paid"
   │           ├─→ Find subscription
   │           └─→ Update: LastPaymentDate = Now
   │
   │   CASE "customer.subscription.updated":
   │       └─→ HandleSubscriptionUpdated(event)
   │           ├─→ Find local subscription by StripeSubscriptionId
   │           ├─→ Sync status: active/paused/cancelled
   │           ├─→ Sync price if changed
   │           └─→ Sync next billing date
   │
   ├─→ [MARK AS PROCESSED]
   │   └─→ WebhookIdempotencyService.MarkAsProcessedAsync(
   │         eventId,
   │         processingTimeMs
   │       )
   │       └─→ UPDATE ProcessedWebhookEvents
   │           SET Status = "Processed", ProcessedAt = Now
   │
   └─→ RETURN 200 OK

[RESULT] Local database synchronized with Stripe
```

---

## 📊 STRIPE OBJECT LIFECYCLE

### **Customer Lifecycle:**

```
1. CREATE:
   Local: User registered
   ↓
   StripeService.EnsureStripeCustomerAsync()
   ↓
   Stripe API: Customer.Create()
   ↓
   Stripe: Customer created ("cus_xxxxx")
   ↓
   Local: user.StripeCustomerId = "cus_xxxxx"

2. UPDATE:
   Local: User updates email/name
   ↓
   StripeService.UpdateCustomerAsync()
   ↓
   Stripe API: Customer.Update()
   ↓
   Stripe: Customer updated
   ↓
   Webhook: "customer.updated"
   ↓
   Local: Sync confirmed

3. DELETE:
   Local: User account deleted
   ↓
   StripeService.DeleteCustomerAsync()
   ↓
   Stripe API: Customer.Delete()
   ↓
   Stripe: Customer deleted
```

---

### **Subscription Lifecycle:**

```
1. CREATE:
   Local: CreateSubscriptionAsync()
   ↓
   StripeService.CreateSubscriptionAsync()
   ↓
   Stripe API: Subscription.Create() 
   💳 CHARGES $280
   ↓
   Stripe: Subscription active ("sub_xxxxx")
   ↓
   Local: StripeSubscriptionId = "sub_xxxxx"
   ↓
   Webhook: "customer.subscription.created"
   ↓
   Local: Confirmation

2. CANCEL:
   Local: CancelSubscriptionAsync()
   ↓
   StripeService.CancelSubscriptionAsync()
   ↓
   Stripe API: Subscription.Cancel()
   ↓
   Stripe: Subscription cancelled
   ↓
   Webhook: "customer.subscription.deleted"
   ↓
   Local: Status = "Cancelled"

3. PAUSE:
   Local: PauseSubscriptionAsync()
   ↓
   StripeService.PauseSubscriptionAsync()
   ↓
   Stripe API: Subscription.Update({ pause_collection: { behavior: "void" } })
   ↓
   Stripe: Subscription paused
   ↓
   Webhook: "customer.subscription.updated"
   ↓
   Local: Status = "Paused"

4. RESUME:
   Local: ResumeSubscriptionAsync()
   ↓
   StripeService.ResumeSubscriptionAsync()
   ↓
   Stripe API: Subscription.Update({ pause_collection: null })
   ↓
   Stripe: Subscription resumed
   ↓
   Webhook: "customer.subscription.updated"
   ↓
   Local: Status = "Active"
```

---

### **Payment Lifecycle:**

```
1. INITIAL PAYMENT (Subscription):
   StripeService.CreateSubscriptionAsync()
   ↓
   Stripe: Auto-charges first payment
   ↓
   Stripe creates: Invoice, PaymentIntent
   ↓
   Webhook: "invoice.payment_succeeded"
   ↓
   Local: BillingRecord status = "Paid"

2. UPFRONT PAYMENT (Extra Credits):
   PaymentService.ProcessPaymentAsync()
   ↓
   StripeBillingService.ProcessStripePaymentAsync()
   ↓
   Stripe API: PaymentIntent.Create({ confirm: true })
   💳 CHARGES $20 IMMEDIATELY
   ↓
   Stripe returns: { status: "succeeded" }
   ↓
   Local: BillingRecord status = "Paid"
   ↓
   Webhook: "payment_intent.succeeded"
   ↓
   Local: Confirmation

3. RECURRING PAYMENT (Monthly):
   Stripe: Auto-bills on renewal date
   ↓
   Stripe creates: Invoice, PaymentIntent
   ↓
   Stripe charges: $280
   ↓
   Webhook: "invoice.payment_succeeded"
   ↓
   Local: Create/update billing record

4. FAILED PAYMENT:
   Stripe: Payment attempt fails
   ↓
   Webhook: "invoice.payment_failed"
   ↓
   Local:
     • FailedPaymentAttempts++
     • Status = "PaymentFailed"
     • Send notification
   ↓
   Stripe: Retries based on settings
```

---

## 🔐 SECURITY & VALIDATION

### **Webhook Signature Validation:**

```csharp
// In StripeWebhookController:
var json = await ReadBodyAsync();
var signature = Request.Headers["Stripe-Signature"];
var secret = Configuration["StripeSettings:WebhookSecret"];

Event stripeEvent = EventUtility.ConstructEvent(
    json,
    signature,
    secret  // "whsec_xxxxxxxxxxxxx"
);

// If signature doesn't match → StripeException → Return 400
```

**Security:** Prevents unauthorized webhook calls ✅

---

### **Idempotency (Prevents Duplicate Processing):**

```csharp
// Check before processing:
var processed = await _webhookIdempotencyService.CheckIdempotencyAsync(eventId);
if (processed.ShouldProcess == false)
{
    return 200 "Already processed";
}

// Process event...

// Mark as processed:
await _webhookIdempotencyService.MarkAsProcessedAsync(eventId);
```

**Benefit:** If Stripe resends webhook, we don't process twice ✅

---

## 📊 STRIPE WEBHOOK EVENTS HANDLED

| Stripe Event | Handler Method | Local Action |
|--------------|----------------|--------------|
| `customer.subscription.created` | HandleSubscriptionCreated | Create/confirm subscription |
| `customer.subscription.updated` | HandleSubscriptionUpdated | Sync status, price, dates |
| `customer.subscription.deleted` | HandleSubscriptionDeleted | Cancel local subscription |
| `invoice.payment_succeeded` | HandleInvoicePaymentSucceeded | Mark billing as paid |
| `invoice.payment_failed` | HandleInvoicePaymentFailed | Handle payment failure |
| `payment_intent.succeeded` | HandlePaymentIntentSucceeded | Confirm payment |
| `payment_intent.payment_failed` | HandlePaymentIntentFailed | Mark payment failed |
| `checkout.session.completed` | HandleCheckoutCompleted | Create subscription |

---

## 🔄 SYNCHRONIZATION POINTS

### **Outbound (Local → Stripe):**

```
Local Action                        Stripe API Call
──────────────────────────────────────────────────────────────
Create plan                    →    Product.Create(), Price.Create()
Create subscription            →    Subscription.Create() 💳 Charges
Cancel subscription            →    Subscription.Cancel()
Pause subscription             →    Subscription.Update(pause)
Resume subscription            →    Subscription.Update(resume)
Upgrade subscription           →    Subscription.Update(price)
Process payment                →    PaymentIntent.Create() 💳 Charges
Refund payment                 →    Refund.Create()
Update customer                →    Customer.Update()
```

---

### **Inbound (Stripe → Local):**

```
Stripe Event                         Local Update
──────────────────────────────────────────────────────────────
subscription.created            →    Confirm local subscription
subscription.updated            →    Sync status, price, dates
subscription.deleted            →    Cancel local subscription
invoice.payment_succeeded       →    Mark billing as paid
invoice.payment_failed          →    Handle payment failure
payment_intent.succeeded        →    Confirm payment success
payment_intent.payment_failed   →    Mark payment failed
```

---

## 🎯 STRIPE INTEGRATION BEST PRACTICES

### **1. Always Use Metadata** ✅

```csharp
// Store local IDs in Stripe objects:
metadata: {
    "user_id": "789",
    "subscription_id": "sub-guid",
    "billing_record_id": "bill-guid",
    "privilege_name": "Teleconsultation"
}
```

**Benefit:** Easy correlation between Stripe and local records

---

### **2. Store Stripe IDs Locally** ✅

```csharp
// SubscriptionPlan:
public string StripeProductId { get; set; }
public string StripeMonthlyPriceId { get; set; }

// Subscription:
public string StripeSubscriptionId { get; set; }
public string StripeCustomerId { get; set; }
public string StripePriceId { get; set; }

// BillingRecord:
public string StripeInvoiceId { get; set; }
public string StripePaymentIntentId { get; set; }
```

**Benefit:** Can query Stripe using these IDs for reconciliation

---

### **3. Handle Webhooks Idempotently** ✅

```csharp
// ProcessedWebhookEvent table tracks:
- EventId
- EventType
- ProcessedAt
- Status (Processing/Processed/Failed)
- AttemptCount
```

**Benefit:** Safe webhook retries

---

### **4. Use Retry Logic** ✅

```csharp
await ExecuteWithRetryAsync(async () =>
{
    return await stripe.Customers.CreateAsync(options);
});

// Retries up to 3 times with 1-second delay
```

**Benefit:** Handles transient Stripe API errors

---

## 🎯 KEY TAKEAWAYS - PART 5

1. **Complete Stripe integration** across all operations
2. **Bidirectional sync** via API calls and webhooks
3. **Payment processing** via PaymentIntent API
4. **Webhook idempotency** prevents duplicate processing
5. **Signature validation** ensures security
6. **Metadata** links Stripe objects to local records
7. **Local IDs** stored in Stripe, Stripe IDs stored locally
8. **Real-time synchronization** via webhook events

---

**See MASTER_ARCHITECTURE_INDEX.md for complete guide navigation...**

