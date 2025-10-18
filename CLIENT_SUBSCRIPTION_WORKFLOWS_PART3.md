# SmartTelehealth Subscription Management - Part 3
## Overage, Renewal, Payment Failures & Advanced Scenarios

---

## WORKFLOW 4: OVERAGE HANDLING (CRITICAL CLIENT REQUIREMENT)

### Scenario: User Exceeds Privilege Limits

**Context:**
- User has used all 5 included teleconsultations
- Tries to book 6th consultation
- **REQUIREMENT: Must pay UPFRONT before service is provided**

```
┌────────────────────────────────────────────────────┐
│ OVERAGE PREVENTION & UPFRONT PAYMENT FLOW          │
├────────────────────────────────────────────────────┤
│                                                     │
│ DAY 25: User tries to book 6th consultation        │
│                                                     │
│ [1] USER ACTION                                     │
│     User Portal → Book Consultation → Select Doctor│
│                                                     │
│ [2] CHECK AVAILABILITY                              │
│     ├─ Service: PrivilegeService                   │
│     └─ Method: CheckPrivilegeAvailabilityAsync()   │
│                                                     │
│     Current State:                                  │
│     ┌──────────────────────────────────┐           │
│     │ UserSubscriptionPrivilegeUsage:  │           │
│     │  AllocatedLimit: 5               │           │
│     │  UsedCount: 5                    │           │
│     │  RemainingLimit: 0 ❌            │           │
│     └──────────────────────────────────┘           │
│                                                     │
│     Validation:                                     │
│     ├─ Check: RemainingLimit >= 1?                 │
│     ├─ 0 >= 1? ❌ NO                                │
│     └─ Result: INSUFFICIENT CREDITS                │
│                                                     │
│ [3] GET OVERAGE PRICING                             │
│     ├─ Get latest plan version (abuse prevention)  │
│     ├─ Find privilege: Teleconsultation            │
│     └─ UnitCost: $25.00 (overage price)            │
│                                                     │
│     💡 NOTE: Uses LATEST pricing, not user's plan  │
│        If admin changed price from $25 to $30,     │
│        user pays $30 (prevents price gaming)       │
│                                                     │
│ [4] RETURN 402 PAYMENT REQUIRED                     │
│     ├─ Status: 402                                 │
│     ├─ Message: "Insufficient credits"             │
│     ├─ AvailableCredits: 0                         │
│     ├─ RequiredCredits: 1                          │
│     ├─ CostPerUnit: $25.00                         │
│     └─ TotalRequired: $25.00                       │
│                                                     │
└────────────────────────────────────────────────────┘

┌────────────────────────────────────────────────────┐
│ FRONTEND DISPLAYS PAYMENT MODAL                    │
├────────────────────────────────────────────────────┤
│                                                     │
│  ┌─────────────────────────────────────────┐      │
│  │  ⚠️ Additional Credits Required           │      │
│  │                                          │      │
│  │  You've used all 5 included              │      │
│  │  consultations in your plan.             │      │
│  │                                          │      │
│  │  Additional consultations:               │      │
│  │  $25.00 each                             │      │
│  │                                          │      │
│  │  Quantity: [▼] 1                         │      │
│  │  Total: $25.00                           │      │
│  │                                          │      │
│  │  Payment Method: [●] Visa ****1234       │      │
│  │                                          │      │
│  │  [Cancel]  [Pay $25 & Continue] ─────►  │      │
│  └─────────────────────────────────────────┘      │
│                                                     │
│  User clicks: "Pay $25 & Continue"                 │
│                                                     │
└────────────────────────────────────────────────────┘
```

### Upfront Payment Processing

```
┌────────────────────────────────────────────────────┐
│ PURCHASE ADDITIONAL CREDITS FLOW                    │
├────────────────────────────────────────────────────┤
│                                                     │
│ API Call: POST /api/subscriptions/{id}/credits     │
│ Body:                                               │
│ {                                                   │
│   "privilegeName": "Teleconsultation",              │
│   "quantity": 1,                                    │
│   "paymentMethodId": "pm_card_visa"                 │
│ }                                                   │
│                                                     │
│ [1] VALIDATE SUBSCRIPTION                           │
│     ├─ Get subscription by ID                      │
│     ├─ Check status: Active ✅                      │
│     └─ Verify user owns subscription ✅             │
│                                                     │
│ [2] GET PRIVILEGE CONFIGURATION                     │
│     ├─ Find privilege: Teleconsultation            │
│     ├─ Get latest plan version                     │
│     └─ UnitCost: $25.00                            │
│                                                     │
│ [3] CALCULATE TOTAL COST                            │
│     ├─ Quantity: 1                                 │
│     ├─ Unit Cost: $25.00                           │
│     └─ Total: 1 × $25.00 = $25.00                  │
│                                                     │
│ [4] BEGIN TRANSACTION (Critical!)                   │
│                                                     │
│     [4a] CREATE BILLING RECORD                     │
│          ┌──────────────────────────────┐          │
│          │ BillingRecords:              │          │
│          │  Id: bill_002                │          │
│          │  SubscriptionId: sub_111     │          │
│          │  UserId: 456                 │          │
│          │  Amount: $25.00              │          │
│          │  Type: Overage ⚠️            │          │
│          │  Status: Pending             │          │
│          │  DueDate: NOW (immediate)    │          │
│          │  Description: "1 extra       │          │
│          │   teleconsultation"          │          │
│          └──────────────────────────────┘          │
│                                                     │
│     [4b] PROCESS PAYMENT VIA STRIPE (IMMEDIATE)    │
│          ├─ Create Stripe Invoice                  │
│          ├─ Customer: cus_XYZ789                   │
│          ├─ Amount: $25.00                         │
│          ├─ Collection method: charge_automatically│
│          └─ Charge NOW (not later)                 │
│                                                     │
│          Stripe Response:                          │
│          ├─ Invoice: in_stripe_CCC                 │
│          ├─ Status: paid ✅                         │
│          └─ Payment Intent: pi_DEF456              │
│                                                     │
│     [4c] UPDATE BILLING RECORD                     │
│          ├─ Status: Paid ✅                         │
│          ├─ PaidDate: 2025-11-10                   │
│          ├─ StripeInvoiceId: in_stripe_CCC         │
│          └─ StripePaymentIntentId: pi_DEF456       │
│                                                     │
│     [4d] ADD CREDIT TO USER ACCOUNT                │
│          (ONLY AFTER PAYMENT SUCCEEDS!)            │
│          ┌──────────────────────────────┐          │
│          │ BEFORE:                      │          │
│          │  AllocatedLimit: 5           │          │
│          │  UsedCount: 5                │          │
│          │  RemainingLimit: 0           │          │
│          ├──────────────────────────────┤          │
│          │ AFTER:                       │          │
│          │  AllocatedLimit: 6 ✅        │          │
│          │  UsedCount: 5 (unchanged)    │          │
│          │  RemainingLimit: 1 ✅        │          │
│          └──────────────────────────────┘          │
│                                                     │
│     [4e] IMMEDIATELY USE THE CREDIT                │
│          (For the triggering booking)              │
│          ├─ UsedCount: 6                           │
│          └─ RemainingLimit: 0                      │
│                                                     │
│     [4f] RECORD IN USAGE HISTORY                   │
│          ┌──────────────────────────────┐          │
│          │ PrivilegeUsageHistory:       │          │
│          │  UsageDate: 2025-11-10       │          │
│          │  QuantityUsed: 1             │          │
│          │  UsageType: Overage ⚠️       │          │
│          │  Cost: $25.00 ✅             │          │
│          │  Notes: "Purchased & used"   │          │
│          └──────────────────────────────┘          │
│                                                     │
│     COMMIT TRANSACTION ✅                           │
│                                                     │
│ [5] SEND NOTIFICATIONS                              │
│     ├─ Email: "Payment of $25.00 processed"        │
│     └─ SMS: "Consultation booked. $25 charged."    │
│                                                     │
│ [6] RETURN SUCCESS TO USER                          │
│     ├─ BillingRecordId: bill_002                   │
│     ├─ AmountCharged: $25.00                       │
│     ├─ NewBalance: "0 consultations remaining"     │
│     └─ Message: "Payment successful!"              │
│                                                     │
└────────────────────────────────────────────────────┘

✅ RESULT:
   💳 User paid $25.00 immediately
   ✅ Credit added to account
   ✅ Consultation booked
   📧 Confirmation sent
   🔒 No risk of non-payment
```

### Payment Failure During Overage

```
If payment fails at [4b]:
  ├─ ROLLBACK TRANSACTION
  ├─ Billing record marked as Failed
  ├─ NO credit added to account
  └─ Return error to user:
     "Payment failed. Please update payment method."

User CANNOT use service until payment succeeds.
```

---

## WORKFLOW 5: MONTHLY RENEWAL (AUTOMATED)

### Automatic Subscription Renewal Process

```
┌────────────────────────────────────────────────────┐
│ AUTOMATED MONTHLY RENEWAL                           │
├────────────────────────────────────────────────────┤
│                                                     │
│ TIMELINE:                                           │
│                                                     │
│ DAY 27 (3 days before renewal):                    │
│   ┌─────────────────────────────────────┐          │
│   │ BACKGROUND SERVICE RUNS              │          │
│   │ (AutomatedBillingBackgroundService)  │          │
│   ├─────────────────────────────────────┤          │
│   │ 1. Query subscriptions expiring soon │          │
│   │    WHERE NextBillingDate <= NOW + 3d │          │
│   │                                      │          │
│   │ 2. Found: sub_111 (John Doe)         │          │
│   │    NextBillingDate: 2025-11-17       │          │
│   │                                      │          │
│   │ 3. Send reminder email:              │          │
│   │    To: johndoe@example.com           │          │
│   │    Subject: "Renewal in 3 days"      │          │
│   │    Body: "Your Basic Health Plan     │          │
│   │     will renew on Nov 17 for $275"   │          │
│   └─────────────────────────────────────┘          │
│                                                     │
│ DAY 30 (Renewal day - 2025-11-17):                 │
│                                                     │
│   ┌──────────────────────────────────────┐         │
│   │ STRIPE AUTOMATIC BILLING             │         │
│   │ (Stripe's internal scheduler)        │         │
│   ├──────────────────────────────────────┤         │
│   │                                      │         │
│   │ Stripe detects subscription renewal: │         │
│   │ sub_stripe_AAA                       │         │
│   │                                      │         │
│   │ [1] Create Invoice                   │         │
│   │     ┌────────────────────────┐       │         │
│   │     │ Stripe Invoice:        │       │         │
│   │     │  Id: in_stripe_DDD     │       │         │
│   │     │  Subscription: sub_... │       │         │
│   │     │  Amount: $275.00       │       │         │
│   │     │  Period:               │       │         │
│   │     │   Start: 2025-11-17    │       │         │
│   │     │   End: 2025-12-17      │       │         │
│   │     │  Status: open          │       │         │
│   │     └────────────────────────┘       │         │
│   │                                      │         │
│   │ [2] Charge Payment Method            │         │
│   │     ├─ Customer: cus_XYZ789          │         │
│   │     ├─ Payment Method: pm_card_visa  │         │
│   │     ├─ Amount: $275.00               │         │
│   │     └─ Result: ✅ SUCCESS             │         │
│   │                                      │         │
│   │ [3] Update Invoice                   │         │
│   │     └─ Status: paid                  │         │
│   │                                      │         │
│   │ [4] Update Subscription              │         │
│   │     ├─ Current period start: 11-17   │         │
│   │     └─ Current period end: 12-17     │         │
│   │                                      │         │
│   │ [5] Send Webhook                     │         │
│   │     ├─ Event: invoice.payment_       │         │
│   │     │   succeeded                    │         │
│   │     └─ POST to your webhook endpoint │         │
│   │                                      │         │
│   └──────────────────────────────────────┘         │
│                                                     │
└────────────────────────────────────────────────────┘
```

### Webhook Processing for Renewal

```
┌────────────────────────────────────────────────────┐
│ YOUR SYSTEM RECEIVES RENEWAL WEBHOOK                │
├────────────────────────────────────────────────────┤
│                                                     │
│ [1] Validate & Check Idempotency                   │
│     └─ Event: evt_renewal_XYZ (new) ✅              │
│                                                     │
│ [2] Extract Webhook Data                           │
│     ├─ Invoice: in_stripe_DDD                      │
│     ├─ Subscription: sub_stripe_AAA                │
│     ├─ Amount: $275.00                             │
│     └─ Period: 11-17 to 12-17                      │
│                                                     │
│ [3] Find Local Subscription                        │
│     └─ Query by StripeSubscriptionId: sub_111      │
│                                                     │
│ [4] BEGIN TRANSACTION                               │
│                                                     │
│     [4a] CREATE NEW BILLING RECORD                 │
│          ┌──────────────────────────────┐          │
│          │ BillingRecords:              │          │
│          │  Id: bill_003                │          │
│          │  SubscriptionId: sub_111     │          │
│          │  Amount: $275.00             │          │
│          │  Type: Subscription (renewal)│          │
│          │  Status: Paid                │          │
│          │  PaidDate: 2025-11-17        │          │
│          │  BillingPeriodStart: 11-17   │          │
│          │  BillingPeriodEnd: 12-17     │          │
│          │  StripeInvoiceId: in_stripe_DDD│        │
│          └──────────────────────────────┘          │
│                                                     │
│     [4b] CREATE PAYMENT RECORD                     │
│          ┌──────────────────────────────┐          │
│          │ SubscriptionPayments:        │          │
│          │  SubscriptionId: sub_111     │          │
│          │  BillingRecordId: bill_003   │          │
│          │  Amount: $275.00             │          │
│          │  Status: Success             │          │
│          │  PaymentDate: 2025-11-17     │          │
│          └──────────────────────────────┘          │
│                                                     │
│     [4c] UPDATE SUBSCRIPTION DATES                 │
│          ┌──────────────────────────────┐          │
│          │ Subscriptions:               │          │
│          │  StartDate: 2025-10-17       │          │
│          │    (unchanged - original)    │          │
│          │  EndDate: 2025-12-17 ✅      │          │
│          │    (extended by 1 month)     │          │
│          │  NextBillingDate: 2025-12-17 │          │
│          │    (next cycle)              │          │
│          │  Status: Active              │          │
│          └──────────────────────────────┘          │
│                                                     │
│     [4d] ⚡ RESET PRIVILEGE COUNTERS ⚡            │
│          ┌────────────────────────────────┐        │
│          │ UserSubscriptionPrivilegeUsage:│        │
│          │                                │        │
│          │ [Teleconsultation]             │        │
│          │  BEFORE:                       │        │
│          │   AllocatedLimit: 6 (5+1 extra)│        │
│          │   UsedCount: 6 (all used)      │        │
│          │   RemainingLimit: 0            │        │
│          │  ───────────────────────       │        │
│          │  AFTER RESET:                  │        │
│          │   AllocatedLimit: 5 ✅         │        │
│          │     (back to plan limit)       │        │
│          │   UsedCount: 0 ✅              │        │
│          │     (reset to zero)            │        │
│          │   RemainingLimit: 5 ✅         │        │
│          │     (fresh credits!)           │        │
│          │   LastResetDate: 2025-11-17    │        │
│          │   NextResetDate: 2025-12-17    │        │
│          │                                │        │
│          │ [Medication]                   │        │
│          │  AllocatedLimit: 3 ✅          │        │
│          │  UsedCount: 0 ✅               │        │
│          │  RemainingLimit: 3 ✅          │        │
│          └────────────────────────────────┘        │
│                                                     │
│     [4e] RECORD STATUS IN HISTORY                  │
│          ├─ OldStatus: Active                      │
│          ├─ NewStatus: Active (renewed)            │
│          └─ Reason: "Subscription renewed"         │
│                                                     │
│     COMMIT TRANSACTION ✅                           │
│                                                     │
│ [5] SEND RENEWAL CONFIRMATION                      │
│     ├─ Email: "Subscription Renewed!"              │
│     ├─ Body: "Your plan has been renewed for       │
│     │   another month. You now have fresh          │
│     │   credits: 5 consultations, 3 medications."  │
│     └─ Invoice attached: INV-2025-003              │
│                                                     │
│ [6] RETURN 200 OK TO STRIPE                        │
│                                                     │
└────────────────────────────────────────────────────┘

✅ RENEWAL COMPLETE:
   💳 Payment: $275.00 charged automatically
   🔄 Credits: Reset to original limits
   📅 Next billing: December 17, 2025
   📧 User notified
   📊 Status: Active (continued)
```

### Monthly Billing Summary

```
MONTH 1 (Oct 17 - Nov 17):
├─ Initial Payment: $275.00 (base plan)
├─ Overage Payment: $25.00 (1 extra consultation)
└─ TOTAL: $300.00

MONTH 2 (Nov 17 - Dec 17):
├─ Renewal Payment: $275.00 (auto-charged)
├─ Credits Reset: ✅ 5 consultations, 3 medications
└─ Cycle continues...
```

---

## WORKFLOW 6: PAYMENT FAILURE HANDLING

### Failed Payment Scenarios

**Context:** User's card expires or has insufficient funds

```
┌────────────────────────────────────────────────────┐
│ PAYMENT FAILURE & RECOVERY PROCESS                  │
├────────────────────────────────────────────────────┤
│                                                     │
│ DAY 30: Renewal date (2025-11-17)                  │
│                                                     │
│ [1] STRIPE ATTEMPTS PAYMENT                         │
│     ├─ Subscription: sub_stripe_AAA                │
│     ├─ Customer: cus_XYZ789                        │
│     ├─ Payment Method: pm_card_visa (EXPIRED)      │
│     └─ Result: ❌ PAYMENT FAILED                    │
│                                                     │
│ [2] STRIPE SENDS WEBHOOK                            │
│     Event: "invoice.payment_failed"                │
│                                                     │
└────────────────────────────────────────────────────┘

┌────────────────────────────────────────────────────┐
│ YOUR SYSTEM HANDLES FAILURE                         │
├────────────────────────────────────────────────────┤
│                                                     │
│ [1] Receive Webhook Event                          │
│     └─ invoice.payment_failed                      │
│                                                     │
│ [2] Find Local Subscription                        │
│     └─ sub_111 (John Doe)                          │
│                                                     │
│ [3] UPDATE SUBSCRIPTION STATUS                      │
│     ┌──────────────────────────────────┐           │
│     │ Subscriptions:                   │           │
│     │  Status: PaymentFailed ⚠️        │           │
│     │  FailedPaymentAttempts: 1        │           │
│     │  LastPaymentFailedDate: 11-17    │           │
│     │  LastPaymentError: "Card expired"│           │
│     └──────────────────────────────────┘           │
│                                                     │
│ [4] CREATE BILLING RECORD (Failed)                  │
│     ┌──────────────────────────────────┐           │
│     │ BillingRecords:                  │           │
│     │  Amount: $275.00                 │           │
│     │  Status: Failed ❌               │           │
│     │  Type: Subscription              │           │
│     │  FailedReason: "Card expired"    │           │
│     └──────────────────────────────────┘           │
│                                                     │
│ [5] SEND URGENT NOTIFICATION                        │
│     ├─ Email: "URGENT: Payment Failed"             │
│     ├─ Subject: "Update payment method now"        │
│     ├─ Body: "Your payment of $275 failed.         │
│     │   Please update your payment method          │
│     │   within 3 days to avoid service             │
│     │   suspension."                               │
│     └─ SMS: "Payment failed. Update card now."     │
│                                                     │
│ [6] SCHEDULE RETRY                                  │
│     ├─ Retry #1: In 2 days (Nov 19)                │
│     ├─ Retry #2: In 5 days (Nov 22)                │
│     └─ Retry #3: In 7 days (Nov 24)                │
│                                                     │
└────────────────────────────────────────────────────┘
```

### Retry Mechanism

```
┌────────────────────────────────────────────────────┐
│ AUTOMATIC PAYMENT RETRY LOGIC                       │
├────────────────────────────────────────────────────┤
│                                                     │
│ RETRY #1 (2 days later - Nov 19):                  │
│   ┌──────────────────────────────────┐             │
│   │ AutomatedBillingService runs     │             │
│   ├──────────────────────────────────┤             │
│   │ 1. Find failed billing records   │             │
│   │ 2. Check retry attempts: 1 of 3  │             │
│   │ 3. Attempt payment via Stripe    │             │
│   │    └─ Result: ❌ STILL FAILED     │             │
│   │ 4. Update:                       │             │
│   │    FailedPaymentAttempts: 2      │             │
│   │ 5. Send notification: "Retry 1   │             │
│   │    failed. Update payment."      │             │
│   └──────────────────────────────────┘             │
│                                                     │
│   User still has access (grace period)             │
│                                                     │
│ RETRY #2 (5 days later - Nov 22):                  │
│   Same process...                                   │
│   └─ Result: ❌ FAILED (attempts: 3 of 3)           │
│                                                     │
│ RETRY #3 (7 days later - Nov 24):                  │
│   ┌──────────────────────────────────┐             │
│   │ FINAL RETRY ATTEMPT              │             │
│   ├──────────────────────────────────┤             │
│   │ Check: attempts == 3 (MAX)       │             │
│   │                                  │             │
│   │ Scenario A: User updated card    │             │
│   │   ├─ Retry payment               │             │
│   │   ├─ Result: ✅ SUCCESS!          │             │
│   │   ├─ Status: Active              │             │
│   │   └─ Send: "Payment received!"   │             │
│   │                                  │             │
│   │ Scenario B: Still failed         │             │
│   │   ├─ Result: ❌ FAILED            │             │
│   │   ├─ Status: Suspended ⛔        │             │
│   │   ├─ Disable access to services  │             │
│   │   └─ Send: "Subscription         │             │
│   │       suspended. Pay now."       │             │
│   └──────────────────────────────────┘             │
│                                                     │
└────────────────────────────────────────────────────┘
```

### Suspension & Recovery

```
SUSPENDED STATE:
┌────────────────────────────────────┐
│ Subscription Suspended             │
├────────────────────────────────────┤
│ Status: Suspended                  │
│ Reason: "Payment failure"          │
│ SuspendedDate: 2025-11-24          │
│                                    │
│ USER EXPERIENCE:                   │
│ ├─ Cannot book appointments        │
│ ├─ Cannot order medications        │
│ ├─ Dashboard shows: "Payment       │
│ │   required to reactivate"        │
│ └─ CTA Button: "Update Payment"    │
└────────────────────────────────────┘

USER UPDATES PAYMENT METHOD:
┌────────────────────────────────────┐
│ 1. User adds new card              │
│ 2. System retries payment          │
│ 3. Payment succeeds ✅             │
│ 4. Subscription reactivated        │
│ 5. Status: Active                  │
│ 6. Full access restored            │
└────────────────────────────────────┘
```

---

## WORKFLOW 7: TRIAL SUBSCRIPTIONS

### Trial-to-Paid Conversion

```
┌────────────────────────────────────────────────────┐
│ TRIAL SUBSCRIPTION LIFECYCLE                        │
├────────────────────────────────────────────────────┤
│                                                     │
│ DAY 1: User starts trial                           │
│   ┌──────────────────────────────────┐             │
│   │ Subscription Created:            │             │
│   │  Status: TrialActive             │             │
│   │  IsTrialSubscription: true       │             │
│   │  TrialStartDate: 2025-10-17      │             │
│   │  TrialEndDate: 2025-11-01        │             │
│   │    (15 days trial)               │             │
│   │  NextBillingDate: 2025-11-01     │             │
│   │  CurrentPrice: $275.00           │             │
│   └──────────────────────────────────┘             │
│                                                     │
│   Stripe Subscription:                              │
│   ├─ Created with trial_end parameter              │
│   └─ No immediate charge                           │
│                                                     │
│   User Experience:                                  │
│   ├─ Full access to all privileges                 │
│   ├─ 5 consultations, 3 medications                │
│   └─ Banner: "Trial ends in 14 days"               │
│                                                     │
│ DAY 13 (2 days before trial ends):                 │
│   ├─ Send reminder: "Trial ending soon"            │
│   └─ Prompt: "Add payment method to continue"      │
│                                                     │
│ DAY 15 (Trial end date - 2025-11-01):              │
│   ┌──────────────────────────────────┐             │
│   │ AUTOMATIC CONVERSION ATTEMPT     │             │
│   ├──────────────────────────────────┤             │
│   │                                  │             │
│   │ Scenario A: Payment method exists│             │
│   │   ├─ Stripe charges $275         │             │
│   │   ├─ Status: Active ✅           │             │
│   │   ├─ Credits reset               │             │
│   │   └─ Notification: "Trial        │             │
│   │       converted! Charged $275"   │             │
│   │                                  │             │
│   │ Scenario B: No payment method    │             │
│   │   ├─ Status: TrialExpired ⚠️     │             │
│   │   ├─ Access disabled             │             │
│   │   └─ Prompt: "Add card to        │             │
│   │       continue service"          │             │
│   └──────────────────────────────────┘             │
│                                                     │
└────────────────────────────────────────────────────┘
```

---


