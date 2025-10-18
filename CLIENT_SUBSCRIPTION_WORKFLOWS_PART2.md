# SmartTelehealth Subscription Management - Part 2
## Detailed Workflows & Scenarios

---

## WORKFLOW 1: ADMIN CREATES SUBSCRIPTION PLAN

### Step-by-Step Process

**Step 1: Admin Accesses Plan Creation Form**
```
Admin Portal → Subscription Plans → Create New Plan
```

**Step 2: Admin Fills Out Plan Details**
```json
{
  "name": "Basic Health Plan",
  "description": "Essential healthcare services for individuals",
  "billingCycleId": "monthly-cycle-guid",
  "isAutoCalculatedPrice": true,
  "adminCommissionPercent": 10,
  "privileges": [
    {
      "privilegeId": "teleconsultation-guid",
      "value": 5,
      "privilegeBaseCost": 20.00,
      "unitCost": 25.00,
      "monthlyLimit": 5
    },
    {
      "privilegeId": "medication-guid",
      "value": 3,
      "privilegeBaseCost": 50.00,
      "unitCost": 60.00,
      "monthlyLimit": 3
    }
  ]
}
```

**Step 3: System Processing (Single Transaction)**

```
┌─────────────────────────────────────────────┐
│ BEGIN TRANSACTION                            │
├─────────────────────────────────────────────┤
│                                             │
│ 1. Validate Admin Authorization             │
│    ├─ Check: tokenModel.RoleID == Admin    │
│    └─ Result: ✅ Authorized                 │
│                                             │
│ 2. Create Plan Entity in Database           │
│    ├─ SubscriptionPlan.Id = [NEW GUID]     │
│    ├─ Name = "Basic Health Plan"           │
│    ├─ Price = 0 (will be calculated)       │
│    ├─ IsAutoCalculatedPrice = true         │
│    ├─ AdminCommissionPercent = 10          │
│    └─ Status: Creating...                  │
│                                             │
│ 3. Create Stripe Product                    │
│    ├─ API Call: POST /v1/products          │
│    ├─ Name: "Basic Health Plan"            │
│    ├─ Description: "..."                   │
│    └─ Returns: prod_ABC123                 │
│                                             │
│ 4. Update Plan with Stripe Product ID       │
│    └─ StripeProductId = "prod_ABC123"      │
│                                             │
│ 5. Create Stripe Prices (3 tiers)           │
│    ├─ Monthly: price_1Month_XYZ            │
│    ├─ Quarterly: price_3Month_XYZ          │
│    └─ Annual: price_12Month_XYZ            │
│                                             │
│ 6. Assign Privileges to Plan                │
│    ├─ Teleconsultation:                    │
│    │  ├─ Value: 5                          │
│    │  ├─ PrivilegeBaseCost: $20            │
│    │  └─ UnitCost: $25                     │
│    └─ Medication:                          │
│       ├─ Value: 3                          │
│       ├─ PrivilegeBaseCost: $50            │
│       └─ UnitCost: $60                     │
│                                             │
│ 7. Auto-Calculate Plan Price                │
│    ├─ Teleconsultation: 5 × $20 = $100    │
│    ├─ Medication: 3 × $50 = $150           │
│    ├─ Subtotal: $250                       │
│    ├─ Commission: $250 × 10% = $25         │
│    └─ FINAL PRICE: $275                    │
│                                             │
│ 8. Update Plan with Final Price             │
│    ├─ Price = $275                         │
│    ├─ PrivilegesTotalCost = $250           │
│    └─ Status: Active                       │
│                                             │
│ COMMIT TRANSACTION                          │
└─────────────────────────────────────────────┘

✅ SUCCESS: Plan Created
   Plan ID: f3a1b2c3-...
   Stripe Product ID: prod_ABC123
   Final Price: $275/month
   Privileges: 2 assigned
```

**Step 4: Error Handling**

If ANY step fails (e.g., Stripe API error):
```
ROLLBACK TRANSACTION
├─ Database changes reverted
└─ Cleanup Stripe resources:
   ├─ Delete Product: prod_ABC123
   └─ Delete Prices: price_1Month_XYZ, etc.

Return Error to Admin:
"Failed to create plan: [Error message]"
```

---

## WORKFLOW 2: USER SUBSCRIBES TO PLAN

### Complete Subscription Creation Flow

**Step 1: User Selects Plan**
```
User Portal → Browse Plans → "Basic Health Plan" → Subscribe
```

**Step 2: Payment Method Selection**
```javascript
// User provides payment details
{
  "userId": 456,
  "planId": "f3a1b2c3-...",
  "billingCycleId": "monthly-cycle-guid",
  "paymentMethodId": "pm_card_visa"  // Stripe payment method token
}
```

**Step 3: System Processing**

```
┌───────────────────────────────────────────────────────┐
│ SUBSCRIPTION CREATION PROCESS                          │
├───────────────────────────────────────────────────────┤
│                                                        │
│ [1] VALIDATE PLAN                                      │
│     ├─ Check plan exists and is active                │
│     └─ Result: ✅ Plan found (Price: $275)             │
│                                                        │
│ [2] CHECK FOR EXISTING SUBSCRIPTIONS                   │
│     ├─ Query: Active subscriptions for user+plan      │
│     └─ Result: None found (OK to proceed)             │
│                                                        │
│ [3] ENSURE STRIPE CUSTOMER EXISTS                      │
│     User Record:                                       │
│     ├─ UserId: 456                                    │
│     ├─ Email: johndoe@example.com                     │
│     └─ StripeCustomerId: NULL (not created yet)       │
│                                                        │
│     Create Stripe Customer:                            │
│     ├─ API Call: POST /v1/customers                   │
│     ├─ Email: johndoe@example.com                     │
│     ├─ Name: John Doe                                 │
│     ├─ Metadata: { userId: "456" }                    │
│     └─ Returns: cus_XYZ789                            │
│                                                        │
│     Update User Record:                                │
│     └─ StripeCustomerId = "cus_XYZ789" ✅             │
│                                                        │
│ [4] ATTACH PAYMENT METHOD                              │
│     ├─ API Call: POST /v1/payment_methods/attach      │
│     ├─ PaymentMethod: pm_card_visa                    │
│     ├─ Customer: cus_XYZ789                           │
│     └─ Set as default: true                           │
│                                                        │
│ [5] BEGIN TRANSACTION (All or Nothing)                 │
│                                                        │
│     [5a] CREATE SUBSCRIPTION IN DATABASE               │
│          ┌──────────────────────────────────┐         │
│          │ Subscriptions Table:             │         │
│          │  Id: sub_111                     │         │
│          │  UserId: 456                     │         │
│          │  PlanId: f3a1b2c3-...            │         │
│          │  Status: Pending                 │         │
│          │  StartDate: 2025-10-17           │         │
│          │  EndDate: 2025-11-17             │         │
│          │  NextBillingDate: 2025-11-17     │         │
│          │  CurrentPrice: 275.00            │         │
│          │  StripeCustomerId: cus_XYZ789    │         │
│          │  StripeSubscriptionId: NULL      │         │
│          └──────────────────────────────────┘         │
│                                                        │
│     [5b] CREATE STRIPE SUBSCRIPTION                    │
│          ├─ API Call: POST /v1/subscriptions          │
│          ├─ Customer: cus_XYZ789                      │
│          ├─ Items: [{ price: price_1Month_XYZ }]      │
│          ├─ Metadata: { subscriptionId: "sub_111" }   │
│          └─ Returns: sub_stripe_AAA                   │
│                                                        │
│     [5c] UPDATE SUBSCRIPTION WITH STRIPE ID            │
│          └─ StripeSubscriptionId = "sub_stripe_AAA"   │
│                                                        │
│     [5d] INITIALIZE PRIVILEGE USAGE RECORDS            │
│          ┌───────────────────────────────────────┐    │
│          │ UserSubscriptionPrivilegeUsage:       │    │
│          │                                       │    │
│          │ [Record 1 - Teleconsultation]         │    │
│          │  SubscriptionId: sub_111              │    │
│          │  PrivilegeId: teleconsultation-guid   │    │
│          │  AllocatedLimit: 5                    │    │
│          │  UsedCount: 0                         │    │
│          │  RemainingLimit: 5                    │    │
│          │  LastResetDate: 2025-10-17            │    │
│          │  NextResetDate: 2025-11-17            │    │
│          │                                       │    │
│          │ [Record 2 - Medication]               │    │
│          │  SubscriptionId: sub_111              │    │
│          │  PrivilegeId: medication-guid         │    │
│          │  AllocatedLimit: 3                    │    │
│          │  UsedCount: 0                         │    │
│          │  RemainingLimit: 3                    │    │
│          └───────────────────────────────────────┘    │
│                                                        │
│     [5e] CREATE INITIAL BILLING RECORD                 │
│          ┌──────────────────────────────────┐         │
│          │ BillingRecords Table:            │         │
│          │  Id: bill_001                    │         │
│          │  SubscriptionId: sub_111         │         │
│          │  UserId: 456                     │         │
│          │  Amount: 275.00                  │         │
│          │  Type: Subscription              │         │
│          │  Status: Pending                 │         │
│          │  DueDate: 2025-10-17 (today)     │         │
│          │  InvoiceNumber: INV-2025-001     │         │
│          └──────────────────────────────────┘         │
│                                                        │
│     [5f] RECORD STATUS HISTORY                         │
│          ┌──────────────────────────────────┐         │
│          │ SubscriptionStatusHistory:       │         │
│          │  SubscriptionId: sub_111         │         │
│          │  OldStatus: NULL                 │         │
│          │  NewStatus: Pending              │         │
│          │  ChangedDate: 2025-10-17         │         │
│          │  Reason: "Subscription created"  │         │
│          └──────────────────────────────────┘         │
│                                                        │
│     COMMIT TRANSACTION ✅                              │
│                                                        │
└───────────────────────────────────────────────────────┘
```

**Step 4: Stripe Processes Payment**

```
┌───────────────────────────────────────────────────┐
│ STRIPE AUTOMATIC PAYMENT PROCESSING                │
├───────────────────────────────────────────────────┤
│                                                    │
│ 1. Stripe creates invoice for new subscription    │
│    ┌────────────────────────────────────┐        │
│    │ Stripe Invoice:                    │        │
│    │  Id: in_stripe_BBB                 │        │
│    │  Subscription: sub_stripe_AAA      │        │
│    │  Customer: cus_XYZ789              │        │
│    │  Amount: $275.00                   │        │
│    │  Status: open                      │        │
│    └────────────────────────────────────┘        │
│                                                    │
│ 2. Stripe charges payment method                  │
│    ├─ Card: pm_card_visa                         │
│    ├─ Amount: $275.00                            │
│    └─ Result: ✅ SUCCESS                          │
│                                                    │
│ 3. Stripe updates invoice status                  │
│    └─ Status: paid                                │
│                                                    │
│ 4. Stripe sends webhook event                     │
│    ├─ Event: "invoice.payment_succeeded"         │
│    ├─ Target: https://yourapi.com/api/webhooks/stripe │
│    └─ Payload: { invoice: {...}, subscription: {...} } │
│                                                    │
└───────────────────────────────────────────────────┘
```

**Step 5: Webhook Handler Updates Your Database**

```
┌───────────────────────────────────────────────────┐
│ WEBHOOK PROCESSING                                 │
├───────────────────────────────────────────────────┤
│                                                    │
│ 1. SECURITY: Validate webhook signature           │
│    ├─ Extract signature from headers              │
│    ├─ Verify using webhook secret                 │
│    └─ Result: ✅ Valid (from Stripe)               │
│                                                    │
│ 2. IDEMPOTENCY: Check if already processed        │
│    ├─ Event ID: evt_ABC123                        │
│    ├─ Query: WebhookEvents table                  │
│    └─ Result: Not processed (proceed)             │
│                                                    │
│ 3. EXTRACT DATA from webhook payload              │
│    ├─ Invoice ID: in_stripe_BBB                   │
│    ├─ Subscription ID: sub_stripe_AAA             │
│    ├─ Amount: $275.00                             │
│    └─ Status: paid                                │
│                                                    │
│ 4. FIND LOCAL SUBSCRIPTION                        │
│    ├─ Query by: StripeSubscriptionId              │
│    └─ Found: sub_111                              │
│                                                    │
│ 5. UPDATE DATABASE (Transaction)                  │
│                                                    │
│    [5a] Update Subscription                       │
│         ├─ Status: Active ✅                       │
│         ├─ StripeSubscriptionId: sub_stripe_AAA   │
│         └─ LastPaymentDate: 2025-10-17            │
│                                                    │
│    [5b] Update Billing Record                     │
│         ├─ Status: Paid ✅                         │
│         ├─ PaidDate: 2025-10-17                   │
│         ├─ StripeInvoiceId: in_stripe_BBB         │
│         └─ StripePaymentIntentId: pi_...          │
│                                                    │
│    [5c] Create Payment Record                     │
│         ┌─────────────────────────────┐           │
│         │ SubscriptionPayments:       │           │
│         │  SubscriptionId: sub_111    │           │
│         │  BillingRecordId: bill_001  │           │
│         │  Amount: 275.00             │           │
│         │  Status: Success            │           │
│         │  PaymentDate: 2025-10-17    │           │
│         └─────────────────────────────┘           │
│                                                    │
│    [5d] Record Status Change                      │
│         ├─ OldStatus: Pending                     │
│         ├─ NewStatus: Active                      │
│         └─ Reason: "Payment successful"           │
│                                                    │
│ 6. MARK WEBHOOK AS PROCESSED                      │
│    ├─ Event ID: evt_ABC123                        │
│    ├─ Processed At: 2025-10-17 10:30:15          │
│    └─ Processing Time: 245ms                      │
│                                                    │
│ 7. SEND CONFIRMATION EMAIL                        │
│    ├─ To: johndoe@example.com                     │
│    ├─ Subject: "Welcome to Basic Health Plan!"    │
│    └─ Body: [Subscription details, privileges]    │
│                                                    │
│ 8. RETURN 200 OK TO STRIPE                        │
│    └─ Confirms webhook processed successfully     │
│                                                    │
└───────────────────────────────────────────────────┘
```

**Final State:**

```
YOUR DATABASE:
✅ Subscription Status: Active
✅ Billing Record Status: Paid
✅ Payment Record: Created
✅ Privileges Initialized: 5 consultations, 3 medications
✅ Status History: Recorded

STRIPE DATABASE:
✅ Customer: cus_XYZ789 (linked)
✅ Subscription: sub_stripe_AAA (active)
✅ Invoice: in_stripe_BBB (paid)

USER:
✅ Can now use subscription privileges
✅ Received confirmation email
✅ Dashboard shows: "5 consultations remaining, 3 medications remaining"
```

---

## WORKFLOW 3: USER CONSUMES PRIVILEGES

### Privilege Consumption Flow

**Scenario: User Books a Teleconsultation**

```
┌────────────────────────────────────────────────────┐
│ PRIVILEGE CONSUMPTION PROCESS                       │
├────────────────────────────────────────────────────┤
│                                                     │
│ USER ACTION:                                        │
│ User Portal → Book Appointment → Select Doctor     │
│                                                     │
│ SYSTEM PROCESSING:                                  │
│                                                     │
│ [1] CHECK PRIVILEGE AVAILABILITY                    │
│     ├─ Service: PrivilegeService                   │
│     ├─ Method: CheckPrivilegeAvailabilityAsync()   │
│     └─ Input: userId=456, privilegeId=telecon...   │
│                                                     │
│     Query Current Usage:                            │
│     ┌──────────────────────────────────┐           │
│     │ UserSubscriptionPrivilegeUsage:  │           │
│     │  SubscriptionId: sub_111         │           │
│     │  PrivilegeId: teleconsultation   │           │
│     │  AllocatedLimit: 5               │           │
│     │  UsedCount: 2 (already used 2)   │           │
│     │  RemainingLimit: 3 (has 3 left)  │           │
│     └──────────────────────────────────┘           │
│                                                     │
│     Validation:                                     │
│     ├─ Check: RemainingLimit >= RequestedQty       │
│     ├─ 3 >= 1? ✅ YES                               │
│     └─ Result: ALLOWED (has credits)               │
│                                                     │
│ [2] USE PRIVILEGE (DECREMENT)                       │
│     ├─ Service: PrivilegeService                   │
│     ├─ Method: UsePrivilegeAsync()                 │
│     └─ BEGIN TRANSACTION                           │
│                                                     │
│     [2a] Update Usage Counter                      │
│          ┌──────────────────────────────┐          │
│          │ BEFORE:                      │          │
│          │  UsedCount: 2                │          │
│          │  RemainingLimit: 3           │          │
│          ├──────────────────────────────┤          │
│          │ AFTER:                       │          │
│          │  UsedCount: 3 ✅             │          │
│          │  RemainingLimit: 2 ✅        │          │
│          │  LastUsedDate: 2025-10-20    │          │
│          └──────────────────────────────┘          │
│                                                     │
│     [2b] Record in Usage History                   │
│          ┌──────────────────────────────┐          │
│          │ PrivilegeUsageHistory:       │          │
│          │  UserId: 456                 │          │
│          │  SubscriptionId: sub_111     │          │
│          │  PrivilegeId: teleconsultation│         │
│          │  UsageDate: 2025-10-20       │          │
│          │  QuantityUsed: 1             │          │
│          │  RemainingAfterUse: 2        │          │
│          │  UsageType: Included ✅      │          │
│          │  Cost: $0.00 (included)      │          │
│          │  RelatedEntityId: appt_123   │          │
│          └──────────────────────────────┘          │
│                                                     │
│     COMMIT TRANSACTION ✅                           │
│                                                     │
│ [3] ALLOW BOOKING TO PROCEED                        │
│     └─ Create appointment record                   │
│                                                     │
└────────────────────────────────────────────────────┘

✅ SUCCESS
   Consultation booked
   Credits: 5 → 4 → 3 → 2 remaining
   User notified: "Booking confirmed! You have 2 consultations left."
```

### Usage Timeline Example

```
MONTH 1: October 17 - November 17

DAY 1 (Oct 17):
  ┌─────────────────────────────────────┐
  │ Subscription Started                 │
  │ Teleconsultations: 5 remaining       │
  │ Medications: 3 remaining             │
  └─────────────────────────────────────┘

DAY 3 (Oct 19): User books consultation
  ┌─────────────────────────────────────┐
  │ Teleconsultations: 4 remaining ▼    │
  │ Medications: 3 remaining             │
  └─────────────────────────────────────┘
  Usage History: +1 record (Included, $0)

DAY 7 (Oct 23): User books consultation
  ┌─────────────────────────────────────┐
  │ Teleconsultations: 3 remaining ▼    │
  │ Medications: 3 remaining             │
  └─────────────────────────────────────┘

DAY 10 (Oct 26): User orders medication
  ┌─────────────────────────────────────┐
  │ Teleconsultations: 3 remaining       │
  │ Medications: 2 remaining ▼          │
  └─────────────────────────────────────┘

DAY 15 (Oct 31): User books consultation
  ┌─────────────────────────────────────┐
  │ Teleconsultations: 2 remaining ▼    │
  │ Medications: 2 remaining             │
  └─────────────────────────────────────┘

DAY 18 (Nov 3): User books consultation
  ┌─────────────────────────────────────┐
  │ Teleconsultations: 1 remaining ▼    │
  │ Medications: 2 remaining             │
  └─────────────────────────────────────┘

DAY 22 (Nov 7): User books consultation
  ┌─────────────────────────────────────┐
  │ Teleconsultations: 0 remaining ▼    │
  │ Medications: 2 remaining             │
  │                                      │
  │ ⚠️ ALL INCLUDED CREDITS USED         │
  └─────────────────────────────────────┘

DAY 25 (Nov 10): User tries to book 6th consultation
  🚫 BLOCKED → See WORKFLOW 4 (Overage)
```

---


