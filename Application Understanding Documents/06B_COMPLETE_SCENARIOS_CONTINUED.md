# 📘 Complete End-to-End Scenarios (Continued)

## SCENARIO 2: User Consumes Privileges & Overage

### Complete Flow from Service Usage to Overage Payment

```
═══════════════════════════════════════════════════════════════
PHASE 1: USER USES INCLUDED PRIVILEGES
═══════════════════════════════════════════════════════════════

Current State:
├─ Subscription: Active
├─ Teleconsultations: 5 remaining
└─ Medications: 3 remaining

┌─────────────────────────────────────────┐
│ DAY 3: User books first consultation     │
│ User Portal → Book Appointment            │
└────────────────┬────────────────────────┘
                 │ HTTP POST
                 ↓
┌─────────────────────────────────────────┐
│ AppointmentsController.BookAsync()       │
└────────────────┬────────────────────────┘
                 │ Must check privilege first
                 ↓
┌─────────────────────────────────────────────────┐
│ PrivilegeService.CheckPrivilegeAvailabilityAsync│
│ File: Application/Services/PrivilegeService.cs  │
│ Line: 126-219                                   │
│                                                  │
│ ┌────────────────────────────────────────────┐ │
│ │ [1] Get Active Subscription                │ │
│ │ subscription = await _subscriptionRepository││
│ │   .GetActiveSubscriptionByUserIdAsync(456);││
│ │                                            │ │
│ │ Found: sub_111 (Status: Active) ✅          │ │
│ └────────────────────────────────────────────┘ │
│                 ↓                                │
│ ┌────────────────────────────────────────────┐ │
│ │ [2] Get Current Usage                      │ │
│ │ usage = await _privilegeUsageRepository    │ │
│ │   .GetByUserAndPrivilegeAsync(             │ │
│ │     userId: 456,                           │ │
│ │     privilegeId: teleconsultation-guid     │ │
│ │   );                                       │ │
│ │                                            │ │
│ │ Current State:                             │ │
│ │   AllocatedLimit: 5                        │ │
│ │   UsedValue: 0                             │ │
│ │   AllowedValue: 5 ← Has credits!           │ │
│ └────────────────────────────────────────────┘ │
│                 ↓                                │
│ ┌────────────────────────────────────────────┐ │
│ │ [3] Check Availability                     │ │
│ │ if (usage.AllowedValue >= 1) {             │ │
│ │   // ✅ HAS CREDITS                        │ │
│ │   return new JsonModel {                   │ │
│ │     StatusCode = 200,                      │ │
│ │     Message = "Privilege available",       │ │
│ │     data = new {                           │ │
│ │       Available = true,                    │ │
│ │       RemainingCredits = 5                 │ │
│ │     }                                      │ │
│ │   };                                       │ │
│ │ }                                          │ │
│ └────────────────────────────────────────────┘ │
└────────────────┬────────────────────────────────┘
                 │ returns 200 OK
                 ↓
┌─────────────────────────────────────────┐
│ AppointmentsController (continued)       │
│ Availability check passed ✅              │
│ Proceed with booking                     │
└────────────────┬────────────────────────┘
                 │ calls
                 ↓
┌─────────────────────────────────────────────────┐
│ PrivilegeService.UsePrivilegeAsync()            │
│ Line: 220-327                                   │
│                                                  │
│ ┌────────────────────────────────────────────┐ │
│ │ BEGIN TRANSACTION                          │ │
│ │ _unitOfWork.BeginTransactionAsync()        │ │
│ └────────────────────────────────────────────┘ │
│                 ↓                                │
│ ┌────────────────────────────────────────────┐ │
│ │ [1] Update Usage Counter                   │ │
│ │ usage.UsedValue += 1;  // 0 → 1            │ │
│ │ usage.AllowedValue = usage.AllocatedLimit  │ │
│ │   - usage.UsedValue;  // 5 - 1 = 4         │ │
│ │ usage.LastUsedAt = DateTime.UtcNow;        │ │
│ │                                            │ │
│ │ await _privilegeUsageRepository            │ │
│ │   .UpdateAsync(usage);                     │ │
│ │                                            │ │
│ │ DATABASE: UserSubscriptionPrivilegeUsage   │ │
│ │ UPDATE:                                    │ │
│ │   UsedValue: 0 → 1 ✅                       │ │
│ │   AllowedValue: 5 → 4 ✅                    │ │
│ │   LastUsedAt: 2025-10-19 10:30:00          │ │
│ └────────────────────────────────────────────┘ │
│                 ↓                                │
│ ┌────────────────────────────────────────────┐ │
│ │ [2] Record in Usage History                │ │
│ │ await _privilegeUsageHistoryRepository     │ │
│ │   .CreateAsync(                            │ │
│ │     new PrivilegeUsageHistory {            │ │
│ │       UserId: 456,                         │ │
│ │       SubscriptionId: sub_111,             │ │
│ │       PrivilegeId: teleconsultation-guid,  │ │
│ │       UsageDate: 2025-10-19,               │ │
│ │       QuantityUsed: 1,                     │ │
│ │       RemainingAfterUse: 4,                │ │
│ │       UsageType: "Included",  ← Not overage│ │
│ │       Cost: 0.00,  ← Free (included)       │ │
│ │       RelatedEntityId: appt-123,           │ │
│ │       Notes: "Video consultation booked"   │ │
│ │     }                                      │ │
│ │   );                                       │ │
│ │                                            │ │
│ │ DATABASE: PrivilegeUsageHistory            │ │
│ │ INSERT: New history record                 │ │
│ └────────────────────────────────────────────┘ │
│                 ↓                                │
│ ┌────────────────────────────────────────────┐ │
│ │ COMMIT TRANSACTION ✅                       │ │
│ │ _unitOfWork.CommitTransactionAsync()       │ │
│ └────────────────────────────────────────────┘ │
│                                                  │
│ return JsonModel {                              │
│   StatusCode = 200,                             │
│   Message = "Privilege used",                   │
│   data = new {                                  │
│     RemainingCredits = 4,                       │
│     UsedTotal = 1                               │
│   }                                             │
│ };                                              │
└─────────────────────────────────────────────────┘
                 ↓
┌─────────────────────────────────────────┐
│ AppointmentsController (continued)       │
│ Privilege used successfully ✅            │
│ Create appointment record                │
│ Return success to user                   │
└─────────────────────────────────────────┘

✅ RESULT:
   Consultation booked
   Counter: 5 → 4 remaining
   History recorded
   User notified

DAYS 7, 10, 15, 22: User continues using privileges
├─ Each use: Check availability → Use privilege → Record history
└─ Counters: 4 → 3 → 2 → 1 → 0

═══════════════════════════════════════════════════════════════
PHASE 2: USER EXCEEDS LIMITS (OVERAGE)
═══════════════════════════════════════════════════════════════

Current State:
├─ Teleconsultations: 0 remaining (all 5 used)
├─ Medications: 2 remaining
└─ User wants 6th consultation

┌─────────────────────────────────────────┐
│ DAY 25: User tries to book 6th consultation│
└────────────────┬────────────────────────┘
                 ↓
┌─────────────────────────────────────────────────┐
│ PrivilegeService.CheckPrivilegeAvailabilityAsync│
│                                                  │
│ ┌────────────────────────────────────────────┐ │
│ │ Get Current Usage                          │ │
│ │   AllocatedLimit: 5                        │ │
│ │   UsedValue: 5                             │ │
│ │   AllowedValue: 0 ← NO CREDITS!            │ │
│ └────────────────────────────────────────────┘ │
│                 ↓                                │
│ ┌────────────────────────────────────────────┐ │
│ │ Check: AllowedValue >= 1?                  │ │
│ │ 0 >= 1? ❌ NO                               │ │
│ │                                            │ │
│ │ INSUFFICIENT CREDITS!                      │ │
│ └────────────────────────────────────────────┘ │
│                 ↓                                │
│ ┌────────────────────────────────────────────┐ │
│ │ Get Latest Plan for Overage Pricing       │ │
│ │ (Abuse Prevention)                         │ │
│ │                                            │ │
│ │ latestPlan = await _subscriptionPlanRepository││
│ │   .GetLatestVersionByParentIdAsync(...);   │ │
│ │                                            │ │
│ │ planPrivilege = latestPlan.PlanPrivileges  │ │
│ │   .Find(p => p.PrivilegeId ==              │ │
│ │     teleconsultation-guid);                │ │
│ │                                            │ │
│ │ unitCost = planPrivilege.UnitCost;         │ │
│ │   // $25.00                                │ │
│ └────────────────────────────────────────────┘ │
│                 ↓                                │
│ ┌────────────────────────────────────────────┐ │
│ │ Return 402 PAYMENT REQUIRED                │ │
│ │                                            │ │
│ │ return new JsonModel {                     │ │
│ │   StatusCode = 402,                        │ │
│ │   Message = "Insufficient credits",        │ │
│ │   data = new {                             │ │
│ │     Available = false,                     │ │
│ │     AvailableCredits = 0,                  │ │
│ │     RequiredCredits = 1,                   │ │
│ │     CostPerUnit = 25.00,                   │ │
│ │     TotalRequired = 25.00                  │ │
│ │   }                                        │ │
│ │ };                                         │ │
│ └────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────┘
                 ↓
┌─────────────────────────────────────────────────┐
│ Frontend (User Portal)                           │
│ Receives 402 response                           │
│                                                  │
│ Displays modal:                                  │
│ ┌────────────────────────────────────────────┐ │
│ │  ⚠️ Additional Credits Required             │ │
│ │                                            │ │
│ │  You've used all 5 consultations.         │ │
│ │  Additional consultations: $25 each        │ │
│ │                                            │ │
│ │  Quantity: [1] ▼                           │ │
│ │  Total: $25.00                             │ │
│ │                                            │ │
│ │  Payment Method: Visa ****1234             │ │
│ │                                            │ │
│ │  [Cancel] [Pay $25 & Continue] ←───────   │ │
│ └────────────────────────────────────────────┘ │
│                                                  │
│ User clicks: "Pay $25 & Continue"               │
└────────────────┬────────────────────────────────┘
                 │ HTTP POST
                 ↓
┌─────────────────────────────────────────┐
│ SubscriptionsController                  │
│   .PurchaseAdditionalCreditsAsync()      │
│ File: API/Controllers/                   │
│   SubscriptionsController.cs             │
│ Line: 450-460                            │
└────────────────┬────────────────────────┘
                 │ calls
                 ↓
┌─────────────────────────────────────────────────┐
│ SubscriptionService.PurchaseAdditionalCreditsAsync│
│ File: Application/Services/SubscriptionService.cs│
│ Line: 1762-2030                                 │
│                                                  │
│ ┌────────────────────────────────────────────┐ │
│ │ [1] Validate Subscription                  │ │
│ │ subscription = await _subscriptionRepository││
│ │   .GetByIdWithDetailsAsync(sub_111);       │ │
│ │                                            │ │
│ │ if (subscription.Status != Active)         │ │
│ │   return 400 "Not active";                 │ │
│ │                                            │ │
│ │ if (tokenModel.UserID != subscription.UserId)│ │
│ │   return 403 "Access denied";              │ │
│ └────────────────────────────────────────────┘ │
│                 ↓                                │
│ ┌────────────────────────────────────────────┐ │
│ │ [2] Get Latest Plan & Calculate Cost      │ │
│ │ latestPlan = await _subscriptionPlanRepository││
│ │   .GetLatestVersionAsync(...);             │ │
│ │                                            │ │
│ │ planPrivilege = latestPlan.PlanPrivileges  │ │
│ │   .Find(p => p.Name == "Teleconsultation");│ │
│ │                                            │ │
│ │ unitCost = planPrivilege.UnitCost;  // $25 │ │
│ │ quantity = 1;                              │ │
│ │ totalCost = 1 × $25 = $25.00               │ │
│ └────────────────────────────────────────────┘ │
│                 ↓                                │
│ ┌────────────────────────────────────────────┐ │
│ │ BEGIN TRANSACTION (Critical!)              │ │
│ │ _unitOfWork.BeginTransactionAsync()        │ │
│ └────────────────────────────────────────────┘ │
│                 ↓                                │
│ ┌────────────────────────────────────────────┐ │
│ │ [3] Create Overage Billing Record          │ │
│ │ var createBillingDto = new                 │ │
│ │   CreateBillingRecordDto {                 │ │
│ │   UserId: 456,                             │ │
│ │   SubscriptionId: sub_111,                 │ │
│ │   Amount: 25.00,                           │ │
│ │   Type: "Overage",                         │ │
│ │   Status: "Pending",                       │ │
│ │   DueDate: DateTime.UtcNow,  // PAY NOW!   │ │
│ │   Description: "1 extra teleconsultation"  │ │
│ │ };                                         │ │
│ │                                            │ │
│ │ billingResult = await _billingService      │ │
│ │   .CreateBillingRecordAsync(               │ │
│ │     createBillingDto, tokenModel           │ │
│ │   );                                       │ │
│ │                                            │ │
│ │ DATABASE: BillingRecords                   │ │
│ │ INSERT:                                    │ │
│ │   Id: bill_002                             │ │
│ │   Type: Overage ⚠️                         │ │
│ │   Amount: $25.00                           │ │
│ │   Status: Pending                          │ │
│ └────────────────────────────────────────────┘ │
│                 ↓                                │
│ ┌────────────────────────────────────────────┐ │
│ │ [4] Process Payment IMMEDIATELY            │ │
│ │ _billingService.ProcessPaymentAsync(       │ │
│ │   billingRecordId: bill_002,               │ │
│ │   tokenModel                               │ │
│ │ )                                          │ │
│ └────────────────────────────────────────────┘ │
└────────────────┬────────────────────────────────┘
                 │ calls
                 ↓
┌─────────────────────────────────────────────────┐
│ PaymentService.ProcessPaymentAsync()            │
│ File: Application/Services/PaymentService.cs    │
│ Line: 400-500                                   │
│                                                  │
│ ┌────────────────────────────────────────────┐ │
│ │ [1] Get Billing Record                     │ │
│ │ billingRecord = await _billingRepository   │ │
│ │   .GetByIdAsync(bill_002);                 │ │
│ └────────────────────────────────────────────┘ │
│                 ↓                                │
│ ┌────────────────────────────────────────────┐ │
│ │ [2] Create Payment Intent in Stripe        │ │
│ │ _stripeService.CreatePaymentIntentAsync(   │ │
│ │   amount: 2500,  // $25 in cents           │ │
│ │   customerId: "cus_XYZ789",                │ │
│ │   description: "1 extra teleconsultation"  │ │
│ │ )                                          │ │
│ └────────────────────────────────────────────┘ │
└────────────────┬────────────────────────────────┘
                 │ API Call
                 ↓
┌─────────────────────────────────────────────────┐
│ StripeService.CreatePaymentIntentAsync()        │
│ Line: 850-900                                   │
│                                                  │
│ var paymentIntentOptions = new                  │
│   PaymentIntentCreateOptions                    │
│ {                                               │
│   Amount = 2500,  // Cents                      │
│   Currency = "usd",                             │
│   Customer = "cus_XYZ789",                      │
│   Description = "1 extra teleconsultation",     │
│   AutomaticPaymentMethods = new {               │
│     Enabled = true  // Use customer's default   │
│   },                                            │
│   Metadata = new Dictionary {                   │
│     { "billingRecordId", "bill_002" },          │
│     { "type", "overage" }                       │
│   }                                             │
│ };                                              │
│                                                  │
│ var service = new PaymentIntentService();       │
│ var paymentIntent = await service               │
│   .CreateAsync(paymentIntentOptions);           │
│                                                  │
│ STRIPE:                                          │
│ ├─ Creates PaymentIntent: pi_OVERAGE123         │
│ ├─ Charges default payment method               │
│ ├─ Result: succeeded ✅                          │
│ └─ Creates Invoice: in_stripe_CCC               │
│                                                  │
│ return paymentIntent;                           │
│   // ID: pi_OVERAGE123                          │
│   // Status: succeeded                          │
└────────────────┬────────────────────────────────┘
                 │ returns to
                 ↓
┌─────────────────────────────────────────────────┐
│ PaymentService (continued)                       │
│                                                  │
│ paymentIntent received ✅                        │
│   Status: succeeded                             │
│                                                  │
│ ┌────────────────────────────────────────────┐ │
│ │ [3] Update Billing Record                  │ │
│ │ billingRecord.Status = "Paid";             │ │
│ │ billingRecord.PaidDate = DateTime.UtcNow;  │ │
│ │ billingRecord.StripePaymentIntentId =      │ │
│ │   "pi_OVERAGE123";                         │ │
│ │                                            │ │
│ │ await _billingRepository.UpdateAsync();    │ │
│ │                                            │ │
│ │ DATABASE: BillingRecords                   │ │
│ │ UPDATE bill_002:                           │ │
│ │   Status: Pending → Paid ✅                 │ │
│ │   PaidDate: 2025-11-10                     │ │
│ └────────────────────────────────────────────┘ │
│                 ↓                                │
│ ┌────────────────────────────────────────────┐ │
│ │ [4] Create Payment Record                  │ │
│ │ payment = new SubscriptionPayment {        │ │
│ │   SubscriptionId: sub_111,                 │ │
│ │   BillingRecordId: bill_002,               │ │
│ │   Amount: 25.00,                           │ │
│ │   Status: "Success",                       │ │
│ │   PaymentDate: 2025-11-10                  │ │
│ │ };                                         │ │
│ │                                            │ │
│ │ await _paymentRepository.CreateAsync();    │ │
│ │                                            │ │
│ │ DATABASE: SubscriptionPayments             │ │
│ │ INSERT: pay_002                            │ │
│ └────────────────────────────────────────────┘ │
│                                                  │
│ return JsonModel {                              │
│   StatusCode = 200,                             │
│   Message = "Payment successful"                │
│ };                                              │
└────────────────┬────────────────────────────────┘
                 │ returns to
                 ↓
┌─────────────────────────────────────────────────┐
│ SubscriptionService (continued)                  │
│ Payment successful ✅                            │
│                                                  │
│ ┌────────────────────────────────────────────┐ │
│ │ [5] Add Credit ONLY AFTER Payment Succeeds│ │
│ │ usage = await _privilegeUsageRepository    │ │
│ │   .GetByUserAndPrivilegeAsync(...);        │ │
│ │                                            │ │
│ │ usage.AllocatedLimit += 1;  // 5 → 6       │ │
│ │ usage.AllowedValue += 1;  // 0 → 1         │ │
│ │                                            │ │
│ │ await _privilegeUsageRepository            │ │
│ │   .UpdateAsync(usage);                     │ │
│ │                                            │ │
│ │ DATABASE: UserSubscriptionPrivilegeUsage   │ │
│ │ UPDATE:                                    │ │
│ │   AllocatedLimit: 5 → 6 ✅                  │ │
│ │   AllowedValue: 0 → 1 ✅                    │ │
│ └────────────────────────────────────────────┘ │
│                 ↓                                │
│ ┌────────────────────────────────────────────┐ │
│ │ [6] Immediately Use the Credit             │ │
│ │ await _privilegeService.UsePrivilegeAsync( │ │
│ │   userId: 456,                             │ │
│ │   privilegeId: teleconsultation-guid,      │ │
│ │   quantity: 1                              │ │
│ │ );                                         │ │
│ │                                            │ │
│ │ // This calls the standard UsePrivilege    │ │
│ │ // logic (increments UsedValue, etc.)      │ │
│ │                                            │ │
│ │ DATABASE: UserSubscriptionPrivilegeUsage   │ │
│ │ UPDATE:                                    │ │
│ │   UsedValue: 5 → 6 ✅                       │ │
│ │   AllowedValue: 1 → 0 ✅                    │ │
│ └────────────────────────────────────────────┘ │
│                 ↓                                │
│ ┌────────────────────────────────────────────┐ │
│ │ [7] Record as Overage in History           │ │
│ │ (Done inside UsePrivilegeAsync, but with   │ │
│ │  special marking)                          │ │
│ │                                            │ │
│ │ DATABASE: PrivilegeUsageHistory            │ │
│ │ INSERT:                                    │ │
│ │   UsageType: "Overage" ⚠️                  │ │
│ │   Cost: $25.00 ✅                           │ │
│ │   QuantityUsed: 1                          │ │
│ │   Notes: "Purchased & used extra credit"   │ │
│ └────────────────────────────────────────────┘ │
│                 ↓                                │
│ ┌────────────────────────────────────────────┐ │
│ │ COMMIT TRANSACTION ✅                       │ │
│ │ _unitOfWork.CommitTransactionAsync()       │ │
│ │                                            │ │
│ │ ALL CHANGES SAVED:                         │ │
│ │ ✅ Billing record: Paid                     │ │
│ │ ✅ Payment record: Created                  │ │
│ │ ✅ Credit added: 6 total                    │ │
│ │ ✅ Credit used: 6 used                      │ │
│ │ ✅ History: Marked as overage               │ │
│ └────────────────────────────────────────────┘ │
│                                                  │
│ return JsonModel {                              │
│   StatusCode = 200,                             │
│   Message = "Purchase successful",              │
│   data = new {                                  │
│     BillingRecordId = bill_002,                 │
│     AmountCharged = 25.00,                      │
│     CreditsAdded = 1,                           │
│     NewBalance = 0                              │
│   }                                             │
│ };                                              │
└─────────────────────────────────────────────────┘

✅ OVERAGE COMPLETE:
   💳 User paid $25 upfront
   ✅ Credit added to account
   ✅ Credit used for consultation
   📧 Confirmation sent
   🔒 No risk of non-payment

DATABASE STATE:
┌──────────────────────────────────────┐
│ BillingRecords:                       │
│ [bill_001] Subscription: $275 (Paid)  │
│ [bill_002] Overage: $25 (Paid) ←NEW  │
├──────────────────────────────────────┤
│ SubscriptionPayments:                 │
│ [pay_001] $275 (Success)              │
│ [pay_002] $25 (Success) ←NEW          │
├──────────────────────────────────────┤
│ PrivilegeUsageHistory:                │
│ [history_1-5] Type: Included, Cost: $0│
│ [history_6] Type: Overage, Cost: $25 ←NEW│
└──────────────────────────────────────┘

═══════════════════════════════════════════════════════════════
PHASE 3: MONTHLY RENEWAL (Automated)
═══════════════════════════════════════════════════════════════

┌─────────────────────────────────────────┐
│ DAY 30: Renewal Date (Nov 17, 2025)     │
│ Time: 00:00:00 UTC (Midnight)           │
└─────────────────────────────────────────┘
                 ↓
┌─────────────────────────────────────────────────┐
│ STRIPE Automatic Billing (Stripe's System)      │
│                                                  │
│ [1] Stripe Scheduler Detects Renewal            │
│     Subscription: sub_stripe_AAA                │
│     Current_period_end: 2025-11-17              │
│     Status: active                              │
│     Auto-renew: true                            │
│                                                  │
│ [2] Create New Invoice                          │
│     Invoice: in_stripe_DDD                      │
│     Amount: $275.00                             │
│     Period: 2025-11-17 to 2025-12-17            │
│     Status: open                                │
│                                                  │
│ [3] Charge Customer's Default Payment Method    │
│     Customer: cus_XYZ789                        │
│     Payment Method: pm_card_visa                │
│     Amount: $275.00                             │
│     Result: SUCCESS ✅                           │
│                                                  │
│ [4] Update Invoice                              │
│     Status: open → paid                         │
│     Payment Intent: pi_RENEWAL456               │
│                                                  │
│ [5] Update Subscription                         │
│     Current_period_start: 2025-11-17            │
│     Current_period_end: 2025-12-17              │
│                                                  │
│ [6] Send Webhook to Your System                 │
│     Event Type: "invoice.payment_succeeded"     │
│     Event ID: evt_renewal_XYZ                   │
│     POST https://yourapi.com/api/webhooks/stripe│
│     Body: {                                     │
│       type: "invoice.payment_succeeded",        │
│       data: {                                   │
│         object: {                               │
│           id: "in_stripe_DDD",                  │
│           subscription: "sub_stripe_AAA",       │
│           amount_paid: 27500,                   │
│           ...                                   │
│         }                                       │
│       }                                         │
│     }                                           │
└────────────────┬────────────────────────────────┘
                 │ HTTP POST Webhook
                 ↓
┌─────────────────────────────────────────────────┐
│ StripeWebhookController.HandleWebhook()         │
│                                                  │
│ [1] Validate Webhook Signature                  │
│     stripeEvent = EventUtility.ConstructEvent(  │
│       json, signature, webhookSecret            │
│     );                                          │
│     ✅ Valid signature                          │
│                                                  │
│ [2] Check Idempotency                           │
│     result = await _webhookIdempotencyService   │
│       .CheckIdempotencyAsync("evt_renewal_XYZ");│
│                                                  │
│     if (result.ShouldProcess) ✅                 │
│       Continue processing                       │
│                                                  │
│ [3] Route to Handler                            │
│     Event type: "invoice.payment_succeeded"     │
│     → await HandlePaymentSucceeded(stripeEvent);│
└────────────────┬────────────────────────────────┘
                 │ calls
                 ↓
┌─────────────────────────────────────────────────┐
│ StripeWebhookController.HandlePaymentSucceeded()│
│ Line: 540-650                                   │
│                                                  │
│ invoice = stripeEvent.Data.Object as Invoice;   │
│                                                  │
│ ┌────────────────────────────────────────────┐ │
│ │ [1] Extract Data from Webhook              │ │
│ │ invoiceId = "in_stripe_DDD"                │ │
│ │ subscriptionId = invoice.Metadata          │ │
│ │   ["subscriptionId"];  // "sub_111"        │ │
│ │ amountPaid = 27500;  // $275 in cents      │ │
│ └────────────────────────────────────────────┘ │
│                 ↓                                │
│ ┌────────────────────────────────────────────┐ │
│ │ [2] Find Local Subscription                │ │
│ │ subscription = await _subscriptionRepository││
│ │   .GetByIdWithDetailsAsync(sub_111);       │ │
│ └────────────────────────────────────────────┘ │
│                 ↓                                │
│ ┌────────────────────────────────────────────┐ │
│ │ [3] Determine if Renewal                   │ │
│ │ // Check if this is renewal or first payment││
│ │ isRenewal = subscription.LastPaymentDate   │ │
│ │   != null;  // true (had previous payment) │ │
│ └────────────────────────────────────────────┘ │
│                 ↓                                │
│ ┌────────────────────────────────────────────┐ │
│ │ BEGIN TRANSACTION                          │ │
│ │ _unitOfWork.BeginTransactionAsync()        │ │
│ └────────────────────────────────────────────┘ │
│                 ↓                                │
│ ┌────────────────────────────────────────────┐ │
│ │ [4] Create New Billing Record (Renewal)    │ │
│ │ billingRecord = new BillingRecord {        │ │
│ │   Id: bill_003,                            │ │
│ │   SubscriptionId: sub_111,                 │ │
│ │   Type: "Subscription",  ← Renewal         │ │
│ │   Status: "Paid",  ← Already paid          │ │
│ │   Amount: 275.00,                          │ │
│ │   PaidDate: 2025-11-17,                    │ │
│ │   StripeInvoiceId: "in_stripe_DDD",        │ │
│ │   BillingPeriodStart: 2025-11-17,          │ │
│ │   BillingPeriodEnd: 2025-12-17             │ │
│ │ };                                         │ │
│ │                                            │ │
│ │ await _billingRepository.CreateAsync();    │ │
│ │                                            │ │
│ │ DATABASE: BillingRecords                   │ │
│ │ INSERT: bill_003                           │ │
│ └────────────────────────────────────────────┘ │
│                 ↓                                │
│ ┌────────────────────────────────────────────┐ │
│ │ [5] Create Payment Record                  │ │
│ │ DATABASE: SubscriptionPayments             │ │
│ │ INSERT: pay_003 ($275, Success)            │ │
│ └────────────────────────────────────────────┘ │
│                 ↓                                │
│ ┌────────────────────────────────────────────┐ │
│ │ [6] Update Subscription Dates              │ │
│ │ subscription.EndDate = 2025-12-17;         │ │
│ │   // Extended by 1 month                   │ │
│ │ subscription.NextBillingDate = 2025-12-17; │ │
│ │ subscription.LastPaymentDate = 2025-11-17; │ │
│ │                                            │ │
│ │ await _subscriptionRepository.UpdateAsync();│ │
│ │                                            │ │
│ │ DATABASE: Subscriptions                    │ │
│ │ UPDATE sub_111:                            │ │
│ │   EndDate: 2025-11-17 → 2025-12-17 ✅       │ │
│ │   NextBillingDate: 2025-12-17              │ │
│ └────────────────────────────────────────────┘ │
│                 ↓                                │
│ ┌────────────────────────────────────────────┐ │
│ │ [7] ⚡ RESET PRIVILEGE COUNTERS ⚡         │ │
│ │ (Only for renewals, not first payment)     │ │
│ │                                            │ │
│ │ if (isRenewal) {                           │ │
│ │   await ResetPrivilegeUsageAsync(sub_111); │ │
│ │ }                                          │ │
│ └────────────────────────────────────────────┘ │
└────────────────┬────────────────────────────────┘
                 │ calls
                 ↓
┌─────────────────────────────────────────────────┐
│ SubscriptionBillingService                       │
│   .ResetPrivilegeUsageAsync()                    │
│ (Called during renewal processing)              │
│                                                  │
│ usages = await _privilegeUsageRepository        │
│   .GetBySubscriptionIdAsync(sub_111);           │
│                                                  │
│ foreach (usage in usages) {                     │
│   // Get original plan limit                    │
│   planPrivilege = await _planPrivilegeRepository│
│     .GetByPlanAndPrivilegeAsync(...);           │
│                                                  │
│   // RESET to original limits                   │
│   usage.AllocatedLimit = planPrivilege.Value;   │
│     // Back to 5 (not 6)                        │
│   usage.UsedValue = 0;  // Reset to zero        │
│   usage.AllowedValue = planPrivilege.Value;     │
│     // Back to 5                                │
│   usage.ResetAt = DateTime.UtcNow;              │
│   usage.LastResetDate = DateTime.UtcNow;        │
│   usage.NextResetDate =                         │
│     DateTime.UtcNow.AddMonths(1);               │
│                                                  │
│   await _privilegeUsageRepository               │
│     .UpdateAsync(usage);                        │
│ }                                               │
│                                                  │
│ DATABASE: UserSubscriptionPrivilegeUsage        │
│ UPDATE (2 records):                             │
│                                                  │
│ Teleconsultation:                               │
│   BEFORE: Allocated=6, Used=6, Allowed=0        │
│   AFTER:  Allocated=5, Used=0, Allowed=5 ✅     │
│                                                  │
│ Medication:                                     │
│   BEFORE: Allocated=3, Used=2, Allowed=1        │
│   AFTER:  Allocated=3, Used=0, Allowed=3 ✅     │
└────────────────┬────────────────────────────────┘
                 │ returns to
                 ↓
┌─────────────────────────────────────────────────┐
│ StripeWebhookController (continued)              │
│                                                  │
│ ┌────────────────────────────────────────────┐ │
│ │ [8] Record Status History                  │ │
│ │ await _statusHistoryRepository.CreateAsync(│ │
│ │   new SubscriptionStatusHistory {          │ │
│ │     FromStatus: "Active",                  │ │
│ │     ToStatus: "Active",  // Still active   │ │
│ │     Reason: "Subscription renewed",        │ │
│ │     ChangedAt: 2025-11-17                  │ │
│ │   }                                        │ │
│ │ );                                         │ │
│ └────────────────────────────────────────────┘ │
│                 ↓                                │
│ ┌────────────────────────────────────────────┐ │
│ │ COMMIT TRANSACTION ✅                       │ │
│ │ _unitOfWork.CommitTransactionAsync()       │ │
│ └────────────────────────────────────────────┘ │
│                 ↓                                │
│ ┌────────────────────────────────────────────┐ │
│ │ [9] Mark Webhook as Processed              │ │
│ │ await _webhookIdempotencyService           │ │
│ │   .MarkAsProcessedAsync("evt_renewal_XYZ");│ │
│ └────────────────────────────────────────────┘ │
│                 ↓                                │
│ ┌────────────────────────────────────────────┐ │
│ │ [10] Send Renewal Notification             │ │
│ │ await _notificationService.Send(           │ │
│ │   "Your subscription has been renewed!"    │ │
│ │ );                                         │ │
│ └────────────────────────────────────────────┘ │
│                                                  │
│ return new JsonModel {                          │
│   StatusCode = 200,                             │
│   Message = "Webhook processed"                 │
│ };                                              │
└─────────────────────────────────────────────────┘

✅ RENEWAL COMPLETE:
   💳 Payment: $275 auto-charged
   📅 Dates: Extended to Dec 17
   🔄 Credits: Reset to 5 consultations, 3 medications
   📧 User notified
   📊 Status: Active (continued)

MONTHLY SUMMARY:
┌──────────────────────────────────────┐
│ Month 1 (Oct 17 - Nov 17):          │
│ ├─ Base Plan: $275.00               │
│ ├─ Overage: $25.00                  │
│ └─ TOTAL: $300.00                   │
│                                     │
│ Month 2 (Nov 17 - Dec 17):          │
│ ├─ Base Plan: $275.00               │
│ ├─ Fresh credits: 5 consult, 3 meds │
│ └─ Cycle continues...               │
└──────────────────────────────────────┘
```

---

## SCENARIO 3: Payment Failure & Recovery

### Complete Flow from Failure to Resolution

```
═══════════════════════════════════════════════════════════════
PAYMENT FAILURE SCENARIO
═══════════════════════════════════════════════════════════════

┌─────────────────────────────────────────┐
│ DAY 30: Renewal attempt                  │
│ User's card: EXPIRED                     │
└─────────────────────────────────────────┘
                 ↓
┌─────────────────────────────────────────────────┐
│ STRIPE Attempts Payment                          │
│                                                  │
│ [1] Create invoice: in_stripe_DDD               │
│ [2] Try to charge: pm_card_visa                 │
│ [3] Result: FAILED ❌                            │
│     Error: "Your card has expired"              │
│ [4] Update invoice status: payment_failed       │
│ [5] Send webhook: "invoice.payment_failed"      │
└────────────────┬────────────────────────────────┘
                 │ Webhook
                 ↓
┌─────────────────────────────────────────────────┐
│ StripeWebhookController.HandlePaymentFailed()   │
│ Line: 610-680                                   │
│                                                  │
│ ┌────────────────────────────────────────────┐ │
│ │ [1] Extract Invoice Data                   │ │
│ │ invoice = stripeEvent.Data.Object;         │ │
│ │ error = invoice.LastFinalizationError      │ │
│ │   .Message;  // "Card expired"             │ │
│ └────────────────────────────────────────────┘ │
│                 ↓                                │
│ ┌────────────────────────────────────────────┐ │
│ │ [2] Find Local Subscription                │ │
│ │ subscriptionId = GetSubscriptionIdFromInvoice│
│ │   (invoice);  // "sub_111"                 │ │
│ │                                            │ │
│ │ subscription = await _subscriptionRepository││
│ │   .GetByIdAsync(sub_111);                  │ │
│ └────────────────────────────────────────────┘ │
│                 ↓                                │
│ ┌────────────────────────────────────────────┐ │
│ │ [3] Update Subscription Status             │ │
│ │ subscription.Status = "PaymentFailed";     │ │
│ │ subscription.FailedPaymentAttempts++;      │ │
│ │   // 0 → 1                                 │ │
│ │ subscription.LastPaymentFailedDate =       │ │
│ │   DateTime.UtcNow;                         │ │
│ │ subscription.LastPaymentError =            │ │
│ │   "Your card has expired";                 │ │
│ │                                            │ │
│ │ await _subscriptionRepository.UpdateAsync();│ │
│ │                                            │ │
│ │ DATABASE: Subscriptions                    │ │
│ │ UPDATE sub_111:                            │ │
│ │   Status: Active → PaymentFailed ⚠️        │ │
│ │   FailedPaymentAttempts: 1                 │ │
│ │   LastPaymentError: "Card expired"         │ │
│ └────────────────────────────────────────────┘ │
│                 ↓                                │
│ ┌────────────────────────────────────────────┐ │
│ │ [4] Create Failed Billing Record           │ │
│ │ billingRecord = new BillingRecord {        │ │
│ │   Type: "Subscription",                    │ │
│ │   Status: "Failed",                        │ │
│ │   Amount: 275.00,                          │ │
│ │   FailedReason: "Card expired"             │ │
│ │ };                                         │ │
│ │                                            │ │
│ │ await _billingRepository.CreateAsync();    │ │
│ │                                            │ │
│ │ DATABASE: BillingRecords                   │ │
│ │ INSERT: bill_004 (Status: Failed)          │ │
│ └────────────────────────────────────────────┘ │
│                 ↓                                │
│ ┌────────────────────────────────────────────┐ │
│ │ [5] Send URGENT Notification               │ │
│ │ await _notificationService                 │ │
│ │   .SendPaymentFailureNotificationAsync(    │ │
│ │     email: "johndoe@example.com",          │ │
│ │     name: "John Doe",                      │ │
│ │     error: "Your card has expired"         │ │
│ │   );                                       │ │
│ │                                            │ │
│ │ Email sent:                                │ │
│ │ ┌──────────────────────────────────────┐  │ │
│ │ │ Subject: URGENT: Payment Failed      │  │ │
│ │ │                                      │  │ │
│ │ │ Your payment of $275 failed.         │  │ │
│ │ │ Reason: Your card has expired        │  │ │
│ │ │                                      │  │ │
│ │ │ Please update your payment method    │  │ │
│ │ │ within 7 days to avoid suspension.   │  │ │
│ │ │                                      │  │ │
│ │ │ [Update Payment Method]              │  │ │
│ │ └──────────────────────────────────────┘  │ │
│ └────────────────────────────────────────────┘ │
│                 ↓                                │
│ ┌────────────────────────────────────────────┐ │
│ │ COMMIT TRANSACTION                         │ │
│ │ _unitOfWork.CommitTransactionAsync()       │ │
│ └────────────────────────────────────────────┘ │
│                                                  │
│ return 200 OK to Stripe                         │
└─────────────────────────────────────────────────┘

USER EXPERIENCE:
├─ Receives urgent email
├─ Dashboard shows: "Payment Failed - Update Card"
└─ Still has access (grace period)

RETRY SCHEDULE AUTOMATICALLY CREATED:
├─ Retry #1: In 2 days (Nov 19)
├─ Retry #2: In 5 days (Nov 22)
└─ Retry #3: In 7 days (Nov 24) - FINAL

═══════════════════════════════════════════════════════════════
RETRY PROCESS (Automated Background Job)
═══════════════════════════════════════════════════════════════

┌─────────────────────────────────────────┐
│ DAY 32: Retry #1 (Nov 19)               │
└─────────────────────────────────────────┘
                 ↓
┌─────────────────────────────────────────────────┐
│ AutomatedBillingBackgroundService (Runs Daily)  │
│ File: Infrastructure/Services/                  │
│   AutomatedBillingBackgroundService.cs          │
│ Line: 50-120                                    │
│                                                  │
│ ┌────────────────────────────────────────────┐ │
│ │ [1] Find Failed Subscriptions              │ │
│ │ failedSubs = await _subscriptionRepository │ │
│ │   .GetByStatusAsync("PaymentFailed");      │ │
│ │                                            │ │
│ │ WHERE Status = 'PaymentFailed'             │ │
│ │   AND FailedPaymentAttempts < 3            │ │
│ │   AND LastPaymentFailedDate <=             │ │
│ │     NOW - RetryDelay                       │ │
│ │                                            │ │
│ │ Found: sub_111 (attempts: 1, delay: 2 days)│ │
│ └────────────────────────────────────────────┘ │
│                 ↓                                │
│ ┌────────────────────────────────────────────┐ │
│ │ [2] Get Failed Billing Record              │ │
│ │ billingRecord = await _billingRepository   │ │
│ │   .GetPendingBySubscriptionIdAsync(sub_111);│
│ │                                            │ │
│ │ Found: bill_004 (Status: Failed)           │ │
│ └────────────────────────────────────────────┘ │
│                 ↓                                │
│ ┌────────────────────────────────────────────┐ │
│ │ [3] Retry Payment                          │ │
│ │ paymentResult = await _paymentService      │ │
│ │   .ProcessPaymentAsync(bill_004, token);   │ │
│ └────────────────────────────────────────────┘ │
└────────────────┬────────────────────────────────┘
                 │ calls
                 ↓
┌─────────────────────────────────────────────────┐
│ PaymentService.ProcessPaymentAsync()            │
│ Attempts to charge again via Stripe             │
│                                                  │
│ Result: STILL FAILED ❌                          │
│   (Card still expired, user hasn't updated)     │
└────────────────┬────────────────────────────────┘
                 │ returns failure
                 ↓
┌─────────────────────────────────────────────────┐
│ AutomatedBillingBackgroundService (continued)    │
│                                                  │
│ Payment retry failed ❌                          │
│                                                  │
│ ┌────────────────────────────────────────────┐ │
│ │ [4] Update Attempt Counter                 │ │
│ │ subscription.FailedPaymentAttempts++;      │ │
│ │   // 1 → 2                                 │ │
│ │                                            │ │
│ │ await _subscriptionRepository.UpdateAsync();│ │
│ │                                            │ │
│ │ DATABASE: Subscriptions                    │ │
│ │ UPDATE sub_111:                            │ │
│ │   FailedPaymentAttempts: 2                 │ │
│ └────────────────────────────────────────────┘ │
│                 ↓                                │
│ ┌────────────────────────────────────────────┐ │
│ │ [5] Send Notification                      │ │
│ │ "Retry 1 of 3 failed. Please update        │ │
│ │  payment method. Next retry in 3 days."    │ │
│ └────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────┘

USER EXPERIENCE:
├─ Receives another email reminder
├─ Still has access (grace period continues)
└─ Dashboard: "Update card - 2 retry attempts remaining"

═══════════════════════════════════════════════════════════════
USER UPDATES PAYMENT METHOD & RECOVERY
═══════════════════════════════════════════════════════════════

┌─────────────────────────────────────────┐
│ DAY 34: User adds new card (Nov 21)     │
│ User Portal → Payment Methods → Add Card │
└────────────────┬────────────────────────┘
                 │ HTTP POST
                 ↓
┌─────────────────────────────────────────┐
│ PaymentController.AddPaymentMethodAsync()│
└────────────────┬────────────────────────┘
                 │ calls
                 ↓
┌─────────────────────────────────────────────────┐
│ PaymentService.AddPaymentMethodAsync()          │
│                                                  │
│ await _stripeService.AttachPaymentMethodAsync(  │
│   paymentMethodId: pm_card_mastercard,  // New card│
│   customerId: "cus_XYZ789"                      │
│ );                                              │
│                                                  │
│ await _stripeService.SetDefaultPaymentMethodAsync│
│   customerId: "cus_XYZ789",                     │
│   paymentMethodId: pm_card_mastercard           │
│ );                                              │
│                                                  │
│ STRIPE:                                          │
│ ✅ New card attached                             │
│ ✅ Set as default payment method                 │
└─────────────────────────────────────────────────┘

┌─────────────────────────────────────────┐
│ DAY 35: Retry #2 (Nov 22) - Scheduled   │
└─────────────────────────────────────────┘
                 ↓
┌─────────────────────────────────────────────────┐
│ AutomatedBillingBackgroundService                │
│ Retry #2 triggered                              │
│                                                  │
│ [1] Attempt payment again                       │
│     paymentResult = await _paymentService       │
│       .ProcessPaymentAsync(bill_004);           │
│                                                  │
│     This time: NEW CARD (MasterCard)            │
│     Result: SUCCESS ✅                           │
│                                                  │
│ [2] Update Subscription                         │
│     subscription.Status = "Active";             │
│     subscription.FailedPaymentAttempts = 0;     │
│     subscription.LastPaymentDate = UtcNow;      │
│                                                  │
│     DATABASE: Subscriptions                     │
│     UPDATE sub_111:                             │
│       Status: PaymentFailed → Active ✅          │
│       FailedPaymentAttempts: 0 (reset)          │
│                                                  │
│ [3] Update Billing Record                       │
│     billingRecord.Status = "Paid";              │
│     billingRecord.PaidDate = UtcNow;            │
│                                                  │
│     DATABASE: BillingRecords                    │
│     UPDATE bill_004:                            │
│       Status: Failed → Paid ✅                   │
│                                                  │
│ [4] Process Renewal (Reset Privileges)          │
│     await ResetPrivilegeUsageAsync(sub_111);    │
│                                                  │
│ [5] Send Success Notification                   │
│     "Payment received! Your subscription is     │
│      now active. Thank you for updating your    │
│      payment method."                           │
└─────────────────────────────────────────────────┘

✅ RECOVERY SUCCESSFUL:
   💳 New payment method worked
   ✅ Subscription reactivated
   🔄 Privileges reset
   📧 User notified

═══════════════════════════════════════════════════════════════
ALTERNATIVE: MAX RETRIES REACHED → SUSPENSION
═══════════════════════════════════════════════════════════════

If user NEVER updates card:

┌─────────────────────────────────────────┐
│ DAY 37: Retry #3 (Nov 24) - FINAL      │
└─────────────────────────────────────────┘
                 ↓
┌─────────────────────────────────────────────────┐
│ AutomatedBillingBackgroundService                │
│                                                  │
│ [1] Attempt payment (3rd time)                  │
│     Result: FAILED ❌ (still expired)            │
│                                                  │
│ [2] Check: attempts == 3 (MAX REACHED)          │
│     if (subscription.FailedPaymentAttempts >= 3)│
│     {                                           │
│       // SUSPEND SUBSCRIPTION                   │
│       subscription.Status = "Suspended";        │
│       subscription.SuspendedDate = UtcNow;      │
│       subscription.Notes =                      │
│         "Suspended due to payment failure";     │
│                                                  │
│       DATABASE: Subscriptions                   │
│       UPDATE sub_111:                           │
│         Status: PaymentFailed → Suspended ⛔     │
│         SuspendedDate: 2025-11-24               │
│     }                                           │
│                                                  │
│ [3] Disable Access                              │
│     // User can no longer use services          │
│     // Privilege checks will fail               │
│                                                  │
│ [4] Send Final Warning                          │
│     Email: "ACCOUNT SUSPENDED"                  │
│     "Your subscription has been suspended due   │
│      to payment failure. Pay $275 now to        │
│      reactivate your account."                  │
│                                                  │
│     SMS: "Account suspended. Pay now."          │
└─────────────────────────────────────────────────┘

USER EXPERIENCE:
├─ Dashboard: "Account Suspended"
├─ Cannot book appointments
├─ Cannot order medications
└─ Prominent banner: "Pay $275 to Reactivate"

TO REACTIVATE:
1. User clicks "Pay Now"
2. User adds new payment method
3. System processes payment
4. If successful: Status → Active
5. Full access restored
```

---

Continue to next file for more scenarios...

**Document Version:** 1.0  
**Last Updated:** October 17, 2025

