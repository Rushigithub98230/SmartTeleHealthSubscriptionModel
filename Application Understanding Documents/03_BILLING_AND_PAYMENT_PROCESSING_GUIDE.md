# 📘 Billing and Payment Processing - Developer Guide

> **✨ CURRENT IMPLEMENTATION** | Updated October 18, 2025
> 
> **Key Updates:**
> - ✅ Billing cycle-based recurring charges (not monthly-only)
> - ✅ Price scaling formula: `monthlyPrice × (billingCycleDays / 30) - discount`
> - ✅ Billing cycle discounts: MonthlyBillingDiscount, QuarterlyBillingDiscount, AnnualBillingDiscount
> - ✅ See **CURRENT_IMPLEMENTATION_QUICK_REFERENCE.md** for formulas and examples

---

## Table of Contents
1. [Overview](#overview)
2. [Billing Types](#billing-types)
3. [Database Schema](#database-schema)
4. [Service Architecture](#service-architecture)
5. [Subscription Billing Workflow](#subscription-billing-workflow)
6. [Overage Billing Workflow](#overage-billing-workflow)
7. [Automated Renewal](#automated-renewal)
8. [Payment Failure Handling](#payment-failure-handling)
9. [Code Examples](#code-examples)

---

## 1. Overview

### What is Billing and Payment Processing?

This module handles all financial transactions related to subscriptions, including:
- Creating billing records for subscription fees
- Processing overage charges (when users exceed limits)
- Handling automated recurring renewals (billing cycle-based: monthly/quarterly/annual)
- Managing payment failures and retries
- Processing refunds and adjustments
- Integrating with Stripe for payment processing

### Key Responsibilities

- ✅ **Billing Record Management**: Create and track all billing events
- ✅ **Payment Processing**: Handle payments through Stripe with transaction safety
- ✅ **Overage Billing**: Charge for usage beyond plan limits
- ✅ **Automated Renewals**: Process recurring charges (billing cycle-aware: monthly/quarterly/annual)
- ✅ **Payment Retry Logic**: Handle failed payments gracefully (3 attempts with exponential backoff)
- ✅ **Privilege Reset**: Reset privileges when payment succeeds
- ✅ **Refund Processing**: Manage refunds and credits
- ✅ **Invoice Generation**: Create and track invoices

---

## 2. Billing Types

### 2.1 All Billing Types in the System

| Type | Description | When Created | Example Amount |
|------|-------------|--------------|----------------|
| **Subscription** | Monthly/quarterly/annual plan fees | At subscription creation and renewal | $275.00 |
| **Overage** | Charges for exceeding privilege limits | When user exceeds limits | $25.00 (1 extra consultation) |
| **Consultation** | One-time consultation fees | When consultation booked outside plan | $50.00 |
| **Medication** | Medication fees | When medication ordered | $30.00 |
| **Adjustment** | Credits or debits (admin corrections) | Manual adjustment | -$10.00 (credit) |

### 2.2 Billing Status Flow

```
┌──────────┐
│ PENDING  │  ← Billing record created, awaiting payment
└────┬─────┘
     │
     ├─→ Payment succeeds ─────→ ┌──────┐
     │                            │ PAID │  ← Final success state
     │                            └──────┘
     │
     ├─→ Payment fails ─────────→ ┌─────────┐
     │                             │ FAILED  │ ──→ Retry ──→ PAID or FAILED
     │                             └─────────┘
     │
     ├─→ Partial payment ───────→ ┌──────────────┐
     │                             │ PARTIALLY    │
     │                             │ PAID         │
     │                             └──────────────┘
     │
     └─→ Refund processed ──────→ ┌───────────┐
                                   │ REFUNDED  │  ← Money returned
                                   └───────────┘
```

---

## 3. Database Schema

### 3.1 Table: BillingRecords (Core Billing Entity)

| Column | Type | Description | Example |
|--------|------|-------------|---------|
| Id | UNIQUEIDENTIFIER | Primary key | bill_001 |
| UserId | INT | FK to Users | 456 |
| SubscriptionId | UNIQUEIDENTIFIER | FK to Subscriptions (if applicable) | sub_111 |
| Type | NVARCHAR(50) | Billing type | "Subscription" |
| Status | NVARCHAR(50) | Payment status | "Paid" |
| Amount | DECIMAL(18,2) | Base amount | 275.00 |
| TotalAmount | DECIMAL(18,2) | Final amount (with tax, discount) | 275.00 |
| TaxAmount | DECIMAL(18,2) | Tax | 0.00 |
| DiscountAmount | DECIMAL(18,2) | Discounts applied | 0.00 |
| BillingDate | DATETIME2 | When billed | 2025-10-17 |
| DueDate | DATETIME2 | Payment due date | 2025-10-24 |
| PaidDate | DATETIME2 | When paid | 2025-10-17 |
| InvoiceNumber | NVARCHAR(100) | Unique invoice # | "INV-2025-001" |
| Description | NVARCHAR(MAX) | Billing details | "Monthly subscription..." |
| PaymentMethod | NVARCHAR(50) | Payment method | "stripe" |
| StripeInvoiceId | NVARCHAR(255) | Stripe invoice link | "in_stripe_BBB" |
| StripePaymentIntentId | NVARCHAR(255) | Stripe payment link | "pi_DEF456" |
| ConsultationId | NVARCHAR(255) | Related consultation (if applicable) | NULL |
| Notes | NVARCHAR(MAX) | Additional notes | NULL |

### 3.2 Table: SubscriptionPayments (Payment Records)

| Column | Type | Description | Example |
|--------|------|-------------|---------|
| Id | UNIQUEIDENTIFIER | Primary key | pay_001 |
| SubscriptionId | UNIQUEIDENTIFIER | FK to Subscriptions | sub_111 |
| BillingRecordId | UNIQUEIDENTIFIER | FK to BillingRecords | bill_001 |
| Amount | DECIMAL(18,2) | Payment amount | 275.00 |
| PaymentMethod | NVARCHAR(50) | Method used | "Stripe" |
| Status | NVARCHAR(50) | Payment status | "Success" |
| TransactionId | NVARCHAR(255) | Stripe transaction ID | "pi_..." |
| PaymentDate | DATETIME2 | When paid | 2025-10-17 |
| FailureReason | NVARCHAR(MAX) | If failed, why | NULL |

### 3.3 Table: BillingAdjustments (Credits/Debits)

| Column | Type | Description | Example |
|--------|------|-------------|---------|
| Id | UNIQUEIDENTIFIER | Primary key | adj_001 |
| BillingRecordId | UNIQUEIDENTIFIER | FK to BillingRecords | bill_001 |
| AdjustmentType | NVARCHAR(50) | "Credit" or "Debit" | "Credit" |
| Amount | DECIMAL(18,2) | Adjustment amount | -10.00 |
| Reason | NVARCHAR(MAX) | Why adjusted | "Customer service gesture" |
| AdjustedBy | INT | Admin user ID | 1 |
| AdjustedDate | DATETIME2 | When adjusted | 2025-10-18 |

### 3.4 Relationships

```
Subscriptions
    ↓
    │ One-to-Many
    ↓
BillingRecords
    ↓
    │ One-to-Many
    ↓
SubscriptionPayments

BillingRecords
    ↓
    │ One-to-Many
    ↓
BillingAdjustments
```

---

## 4. Service Architecture

### 4.1 Primary Services

#### **SubscriptionBillingService** (Consolidated Billing Service)
**Location:** `SmartTelehealth.Application/Services/SubscriptionBillingService.cs`

**Responsibilities:**
- Create all types of billing records
- Calculate billing amounts
- Process overage charges
- Handle privilege-based billing
- Generate invoices
- Manage billing adjustments

**Key Dependencies:**
```csharp
IBillingRepository _billingRepository
ISubscriptionRepository _subscriptionRepository
ISubscriptionPlanRepository _subscriptionPlanRepository
IPrivilegeUsageRepository _privilegeUsageRepository
IPaymentService _paymentService
IStripeService _stripeService
IUnitOfWork _unitOfWork
IMapper _mapper
ILogger<SubscriptionBillingService> _logger
```

#### **PaymentService**
**Location:** `SmartTelehealth.Application/Services/PaymentService.cs`

**Responsibilities:**
- Process payments through Stripe
- Manage payment methods
- Handle refunds
- Process payment retries

#### **AutomatedBillingService**
**Location:** `SmartTelehealth.Application/Services/AutomatedBillingService.cs`

**Responsibilities:**
- Automated recurring billing (billing cycle-aware: monthly/quarterly/annual)
- Price migration for existing subscriptions
- Billing amount calculation with scaling and discounts
- Overage charge processing
- Failed payment retry logic
- Subscription suspension after max retries

**Key Methods (Current):**
- ProcessRecurringBillingAsync() - Daily job
- MigrateSubscriptionPricingIfNeededAsync() - Line 577
- CalculateBillingAmountAsync() - Line 932
- CalculateBillingCycleDiscount() - Line 969
- ProcessOverageChargesAsync() - Line 1667

---

## 5. Subscription Billing Workflow

### 5.1 Creating Subscription Billing

**When:** At subscription creation or renewal

```
┌─────────────────────────────────────────────────┐
│ TRIGGER: User subscribes OR renewal date        │
└─────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────┐
│ SubscriptionBillingService                       │
│ Method: CreateSubscriptionBillingAsync()         │
│                                                  │
│ [STEP 1] Extract Subscription Details           │
│   subscription = [passed as parameter]          │
│   amount = subscription.CurrentPrice  // $275   │
│   description = "Monthly billing for..."        │
│   dueDate = DateTime.UtcNow                     │
│                                                  │
│ [STEP 2] Create Billing Record                  │
│   var billingRecord = new BillingRecord {       │
│     Id = Guid.NewGuid(),                        │
│     UserId = subscription.UserId,               │
│     SubscriptionId = subscription.Id,           │
│     Type = BillingType.Subscription,            │
│     Status = BillingStatus.Pending,             │
│     Amount = amount,  // 275.00                 │
│     TotalAmount = amount,  // 275.00            │
│     BillingDate = DateTime.UtcNow,              │
│     DueDate = dueDate,                          │
│     InvoiceNumber = GenerateInvoiceNumber(),    │
│       // "INV-2025-001"                         │
│     Description = description,                  │
│     PaymentMethod = "stripe",                   │
│     CreatedBy = tokenModel.UserID,              │
│     CreatedDate = DateTime.UtcNow               │
│   };                                            │
│                                                  │
│ [STEP 3] Save to Database                       │
│   created = await _billingRepository            │
│     .CreateAsync(billingRecord);                │
│                                                  │
│ [STEP 4] Log and Return                         │
│   _logger.LogInformation(                       │
│     "Created billing record {Id} for ${Amount}",│
│     created.Id, amount                          │
│   );                                            │
│                                                  │
│   return JsonModel {                            │
│     data = billingRecordDto,                    │
│     Message = "Billing record created",         │
│     StatusCode = 200                            │
│   };                                            │
└─────────────────────────────────────────────────┘
```

### 5.2 Processing Payment

**After billing record is created:**

```
┌─────────────────────────────────────────────────┐
│ PaymentService                                   │
│ Method: ProcessPaymentAsync()                    │
│                                                  │
│ [STEP 1] Get Billing Record                     │
│   billingRecord = await _billingRepository      │
│     .GetByIdAsync(billingRecordId);             │
│                                                  │
│ [STEP 2] Validate                               │
│   if (billingRecord.Status != Pending)          │
│     return "Already processed";                 │
│                                                  │
│ [STEP 3] Get Stripe Customer ID                 │
│   subscription = await _subscriptionRepository  │
│     .GetByIdAsync(billingRecord.SubscriptionId);│
│   stripeCustomerId = subscription               │
│     .StripeCustomerId;  // "cus_XYZ789"         │
│                                                  │
│ [STEP 4] Process Payment in Stripe              │
│   paymentIntent = await _stripeService          │
│     .CreatePaymentIntentAsync(                  │
│       amount: billingRecord.TotalAmount * 100,  │
│         // $275 → 27500 cents                   │
│       customerId: stripeCustomerId,             │
│       description: billingRecord.Description,   │
│       metadata: {                               │
│         billingRecordId: billingRecord.Id,      │
│         subscriptionId: subscription.Id         │
│       }                                         │
│     );                                          │
│                                                  │
│   // Stripe auto-charges default payment method │
│   // Returns: pi_DEF456                         │
│                                                  │
│ [STEP 5] Update Billing Record                  │
│   billingRecord.Status = BillingStatus.Paid;    │
│   billingRecord.PaidDate = DateTime.UtcNow;     │
│   billingRecord.StripePaymentIntentId =         │
│     paymentIntent.Id;  // "pi_DEF456"           │
│   billingRecord.StripeInvoiceId =               │
│     paymentIntent.InvoiceId;                    │
│                                                  │
│   await _billingRepository.UpdateAsync(         │
│     billingRecord                               │
│   );                                            │
│                                                  │
│ [STEP 6] Create Payment Record                  │
│   var payment = new SubscriptionPayment {       │
│     SubscriptionId = subscription.Id,           │
│     BillingRecordId = billingRecord.Id,         │
│     Amount = billingRecord.TotalAmount,         │
│     PaymentMethod = "Stripe",                   │
│     Status = "Success",                         │
│     TransactionId = paymentIntent.Id,           │
│     PaymentDate = DateTime.UtcNow               │
│   };                                            │
│                                                  │
│   await _paymentRepository.CreateAsync(payment);│
│                                                  │
│ [STEP 7] Update Subscription                    │
│   subscription.LastPaymentDate = DateTime.UtcNow;│
│   subscription.FailedPaymentAttempts = 0;       │
│   await _subscriptionRepository.UpdateAsync(    │
│     subscription                                │
│   );                                            │
│                                                  │
│ [STEP 8] Return Success                         │
│   return JsonModel {                            │
│     data = paymentDto,                          │
│     Message = "Payment successful",             │
│     StatusCode = 200                            │
│   };                                            │
└─────────────────────────────────────────────────┘
```

---

## 6. Overage Billing Workflow

### 6.1 Overage Scenario

**Context:** User has used all 5 included teleconsultations and wants a 6th

```
┌─────────────────────────────────────────────────┐
│ USER ACTION: Book 6th consultation              │
│ System detects: User has 0 remaining credits    │
└─────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────┐
│ PrivilegeService                                 │
│ Method: CheckPrivilegeAvailabilityAsync()       │
│                                                  │
│ [1] Query Current Usage                         │
│   usage = await _usageRepository                │
│     .GetByUserAndPrivilegeAsync(                │
│       userId, privilegeId                       │
│     );                                          │
│                                                  │
│   usage.AllocatedLimit = 5                      │
│   usage.UsedValue = 5                           │
│   usage.AllowedValue = 0  ← NO CREDITS LEFT     │
│                                                  │
│ [2] Check Availability                          │
│   if (usage.AllowedValue < requestedQuantity) { │
│     // INSUFFICIENT CREDITS                     │
│                                                  │
│     // Get overage cost from latest plan        │
│     latestPlan = await GetLatestPlanVersion();  │
│     privilege = latestPlan.PlanPrivileges       │
│       .Find(p => p.PrivilegeId == privilegeId); │
│                                                  │
│     unitCost = privilege.UnitCost;  // $25      │
│                                                  │
│     return new JsonModel {                      │
│       StatusCode = 402,  // Payment Required    │
│       Message = "Insufficient credits",         │
│       data = new {                              │
│         AvailableCredits = 0,                   │
│         RequiredCredits = 1,                    │
│         CostPerUnit = 25.00,                    │
│         TotalRequired = 25.00                   │
│       }                                         │
│     };                                          │
│   }                                             │
└─────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────┐
│ FRONTEND: Display Payment Required Modal        │
│ "You need to pay $25 for 1 extra consultation"  │
│ [Cancel] [Pay $25 & Continue]                   │
└─────────────────────────────────────────────────┘
                    ↓
        User clicks "Pay $25 & Continue"
                    ↓
┌─────────────────────────────────────────────────┐
│ SubscriptionService                              │
│ Method: PurchaseAdditionalCreditsAsync()         │
│                                                  │
│ [STEP 1] Validate Subscription                  │
│   subscription = await _subscriptionRepository  │
│     .GetByIdWithDetailsAsync(subscriptionId);   │
│                                                  │
│   if (subscription.Status != Active)            │
│     return "Subscription not active";           │
│                                                  │
│ [STEP 2] Calculate Cost                         │
│   latestPlan = await GetLatestPlanVersion();    │
│   privilege = latestPlan.PlanPrivileges         │
│     .Find(p => p.Name == "Teleconsultation");   │
│                                                  │
│   unitCost = privilege.UnitCost;  // $25        │
│   quantity = 1;                                 │
│   totalCost = quantity * unitCost;  // $25      │
│                                                  │
│ [STEP 3] BEGIN TRANSACTION                      │
│   await _unitOfWork.BeginTransactionAsync();    │
│                                                  │
│ [STEP 4] Create Overage Billing Record          │
│   var billingRecord = new BillingRecord {       │
│     Id = Guid.NewGuid(),                        │
│     UserId = subscription.UserId,               │
│     SubscriptionId = subscription.Id,           │
│     Type = BillingType.Overage,  ← OVERAGE      │
│     Status = BillingStatus.Pending,             │
│     Amount = totalCost,  // 25.00               │
│     TotalAmount = totalCost,                    │
│     BillingDate = DateTime.UtcNow,              │
│     DueDate = DateTime.UtcNow,  // Pay NOW      │
│     Description = "1 extra teleconsultation",   │
│     InvoiceNumber = GenerateInvoiceNumber()     │
│   };                                            │
│                                                  │
│   created = await _billingRepository            │
│     .CreateAsync(billingRecord);                │
│                                                  │
│ [STEP 5] Process Payment IMMEDIATELY            │
│   paymentResult = await _paymentService         │
│     .ProcessPaymentAsync(                       │
│       billingRecordId: created.Id,              │
│       tokenModel                                │
│     );                                          │
│                                                  │
│   if (paymentResult.StatusCode != 200) {        │
│     await _unitOfWork.RollbackTransactionAsync();│
│     return "Payment failed";                    │
│   }                                             │
│                                                  │
│ [STEP 6] Add Credit ONLY AFTER Payment Succeeds │
│   usage = await _usageRepository                │
│     .GetByUserAndPrivilegeAsync(...);           │
│                                                  │
│   usage.AllocatedLimit += quantity;  // 5 → 6   │
│   usage.AllowedValue += quantity;  // 0 → 1     │
│                                                  │
│   await _usageRepository.UpdateAsync(usage);    │
│                                                  │
│ [STEP 7] Immediately Use the Credit             │
│   await _privilegeService.UsePrivilegeAsync(    │
│     userId, privilegeId, quantity               │
│   );                                            │
│   // UsedValue: 5 → 6                           │
│   // AllowedValue: 1 → 0                        │
│                                                  │
│ [STEP 8] Record in Usage History                │
│   await _privilegeUsageHistoryRepository        │
│     .CreateAsync(new PrivilegeUsageHistory {    │
│       UserId = userId,                          │
│       SubscriptionId = subscription.Id,         │
│       PrivilegeId = privilegeId,                │
│       UsageDate = DateTime.UtcNow,              │
│       QuantityUsed = quantity,                  │
│       UsageType = "Overage",  ← MARKED AS EXTRA │
│       Cost = totalCost,  // $25                 │
│       RelatedEntityId = appointmentId           │
│     });                                         │
│                                                  │
│ [STEP 9] COMMIT TRANSACTION                     │
│   await _unitOfWork.CommitTransactionAsync();   │
│                                                  │
│ [STEP 10] Send Notifications                    │
│   await _notificationService.Send(              │
│     "Payment of $25 processed successfully"     │
│   );                                            │
│                                                  │
│ [STEP 11] Return Success                        │
│   return JsonModel {                            │
│     data = new {                                │
│       BillingRecordId = created.Id,             │
│       AmountCharged = 25.00,                    │
│       NewBalance = "0 consultations remaining"  │
│     },                                          │
│     Message = "Payment successful",             │
│     StatusCode = 200                            │
│   };                                            │
└─────────────────────────────────────────────────┘
```

### 6.2 Why Upfront Payment is Critical

```
❌ BAD (Old Way - Deferred Billing):
   User exceeds limit → Add to next month's bill → User may not pay → Lost revenue

✅ GOOD (Current Way - Upfront Payment):
   User exceeds limit → Block usage → Require immediate payment → 
   Payment succeeds → Add credit → Allow usage → Zero non-payment risk
```

---

## 7. Automated Renewal

### 7.1 Monthly Renewal Process

**Trigger:** Stripe detects subscription renewal date

```
┌─────────────────────────────────────────────────┐
│ DAY 30: Renewal Date (2025-11-17)              │
└─────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────┐
│ STRIPE AUTOMATIC PROCESS                         │
│                                                  │
│ [1] Stripe's billing engine detects renewal     │
│     Subscription: sub_stripe_AAA                │
│     Current period end: 2025-11-17              │
│                                                  │
│ [2] Create Invoice                              │
│     Invoice ID: in_stripe_DDD                   │
│     Amount: $275.00                             │
│     Period: 2025-11-17 to 2025-12-17            │
│                                                  │
│ [3] Charge Payment Method                       │
│     Customer: cus_XYZ789                        │
│     Payment Method: pm_card_visa                │
│     Result: SUCCESS ✅                           │
│                                                  │
│ [4] Send Webhook Event                          │
│     Event: "invoice.payment_succeeded"          │
│     POST to: yourapi.com/api/webhooks/stripe    │
└─────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────┐
│ YOUR SYSTEM (Webhook Handler)                   │
│ File: StripeWebhookController.cs                │
│                                                  │
│ [1] Validate Signature                          │
│     Verify webhook came from Stripe ✅           │
│                                                  │
│ [2] Check Idempotency                           │
│     Event ID: evt_renewal_XYZ                   │
│     Check if already processed: NO ✅            │
│                                                  │
│ [3] Extract Data                                │
│     Invoice ID: in_stripe_DDD                   │
│     Subscription ID: sub_stripe_AAA             │
│     Amount: $275.00                             │
│                                                  │
│ [4] Find Local Subscription                     │
│     subscription = await _subscriptionRepository│
│       .GetByStripeSubscriptionIdAsync(          │
│         "sub_stripe_AAA"                        │
│       );                                        │
│                                                  │
│ [5] BEGIN TRANSACTION                           │
│     await _unitOfWork.BeginTransactionAsync();  │
│                                                  │
│ [6] Create Billing Record                       │
│     var billingRecord = new BillingRecord {     │
│       Type = BillingType.Subscription,          │
│       Status = BillingStatus.Paid,  ← Already paid│
│       Amount = 275.00,                          │
│       PaidDate = DateTime.UtcNow,               │
│       StripeInvoiceId = "in_stripe_DDD",        │
│       BillingPeriodStart = DateTime.Parse(      │
│         "2025-11-17"),                          │
│       BillingPeriodEnd = DateTime.Parse(        │
│         "2025-12-17")                           │
│     };                                          │
│                                                  │
│     await _billingRepository.CreateAsync(       │
│       billingRecord                             │
│     );                                          │
│                                                  │
│ [7] Create Payment Record                       │
│     var payment = new SubscriptionPayment {     │
│       SubscriptionId = subscription.Id,         │
│       BillingRecordId = billingRecord.Id,       │
│       Amount = 275.00,                          │
│       Status = "Success",                       │
│       PaymentDate = DateTime.UtcNow             │
│     };                                          │
│                                                  │
│     await _paymentRepository.CreateAsync(payment);│
│                                                  │
│ [8] Update Subscription Dates                   │
│     subscription.EndDate =                      │
│       DateTime.Parse("2025-12-17");             │
│     subscription.NextBillingDate =              │
│       DateTime.Parse("2025-12-17");             │
│                                                  │
│     await _subscriptionRepository.UpdateAsync(  │
│       subscription                              │
│     );                                          │
│                                                  │
│ [9] ⚡ RESET PRIVILEGE COUNTERS ⚡              │
│     usages = await _usageRepository             │
│       .GetByUserIdAsync(subscription.UserId);   │
│                                                  │
│     foreach (usage in usages) {                 │
│       // Get original limit from plan           │
│       planPrivilege = await _planPrivilegeRepo  │
│         .GetByPlanAndPrivilegeAsync(...);       │
│                                                  │
│       usage.AllocatedLimit =                    │
│         planPrivilege.Value;  // Back to 5      │
│       usage.UsedValue = 0;  // Reset to zero    │
│       usage.AllowedValue =                      │
│         planPrivilege.Value;  // Back to 5      │
│       usage.ResetAt = DateTime.UtcNow;          │
│       usage.LastResetDate = DateTime.UtcNow;    │
│       usage.NextResetDate =                     │
│         DateTime.UtcNow.AddMonths(1);           │
│                                                  │
│       await _usageRepository.UpdateAsync(usage);│
│     }                                           │
│                                                  │
│ [10] Record Status History                      │
│     await _statusHistoryRepository.CreateAsync( │
│       new SubscriptionStatusHistory {           │
│         SubscriptionId = subscription.Id,       │
│         FromStatus = "Active",                  │
│         ToStatus = "Active",  // Still active   │
│         Reason = "Subscription renewed",        │
│         ChangedAt = DateTime.UtcNow             │
│       }                                         │
│     );                                          │
│                                                  │
│ [11] COMMIT TRANSACTION                         │
│     await _unitOfWork.CommitTransactionAsync(); │
│                                                  │
│ [12] Send Confirmation                          │
│     await _notificationService.Send(            │
│       "Your subscription has been renewed!"     │
│     );                                          │
│                                                  │
│ [13] Return 200 OK to Stripe                    │
│     (Confirms webhook processed successfully)   │
└─────────────────────────────────────────────────┘
```

### 7.2 Privilege Reset Logic

**Before Renewal:**
```
Teleconsultation:
  AllocatedLimit: 6 (5 + 1 purchased extra)
  UsedValue: 6 (all used)
  AllowedValue: 0

Medication:
  AllocatedLimit: 3
  UsedValue: 2
  AllowedValue: 1
```

**After Renewal:**
```
Teleconsultation:
  AllocatedLimit: 5 ← RESET to plan limit
  UsedValue: 0 ← RESET to zero
  AllowedValue: 5 ← Fresh credits!
  LastResetDate: 2025-11-17

Medication:
  AllocatedLimit: 3 ← RESET to plan limit
  UsedValue: 0 ← RESET to zero
  AllowedValue: 3 ← Fresh credits!
  LastResetDate: 2025-11-17
```

---

## 8. Payment Failure Handling

### 8.1 Failed Payment Flow

```
┌─────────────────────────────────────────────────┐
│ DAY 30: Renewal Date                            │
│ Stripe attempts to charge: $275                 │
│ Result: FAILED (card expired)                   │
└─────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────┐
│ STRIPE sends webhook:                           │
│ Event: "invoice.payment_failed"                 │
└─────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────┐
│ YOUR SYSTEM (Webhook Handler)                   │
│                                                  │
│ [1] Extract Invoice Data                        │
│     Invoice ID: in_stripe_DDD                   │
│     Subscription: sub_stripe_AAA                │
│     Error: "Your card has expired"              │
│                                                  │
│ [2] Find Local Subscription                     │
│     subscription = await _subscriptionRepository│
│       .GetByStripeSubscriptionIdAsync(...);     │
│                                                  │
│ [3] Update Subscription Status                  │
│     subscription.Status = "PaymentFailed";      │
│     subscription.FailedPaymentAttempts++;       │
│       // Now: 1                                 │
│     subscription.LastPaymentFailedDate =        │
│       DateTime.UtcNow;                          │
│     subscription.LastPaymentError =             │
│       "Your card has expired";                  │
│                                                  │
│     await _subscriptionRepository.UpdateAsync(  │
│       subscription                              │
│     );                                          │
│                                                  │
│ [4] Create Failed Billing Record                │
│     var billingRecord = new BillingRecord {     │
│       Type = BillingType.Subscription,          │
│       Status = BillingStatus.Failed,  ← FAILED  │
│       Amount = 275.00,                          │
│       FailedReason = "Card expired"             │
│     };                                          │
│                                                  │
│     await _billingRepository.CreateAsync(       │
│       billingRecord                             │
│     );                                          │
│                                                  │
│ [5] Send URGENT Notification                    │
│     await _notificationService.Send(            │
│       to: user.Email,                           │
│       subject: "URGENT: Payment Failed",        │
│       body: "Your payment of $275 failed. Your  │
│         card has expired. Please update your    │
│         payment method within 3 days to avoid   │
│         service suspension."                    │
│     );                                          │
│                                                  │
│     await _smsService.Send(                     │
│       "Payment failed. Update card now."        │
│     );                                          │
│                                                  │
│ [6] Schedule Retry Attempts                     │
│     Retry #1: In 2 days (Nov 19)                │
│     Retry #2: In 5 days (Nov 22)                │
│     Retry #3: In 7 days (Nov 24) ← FINAL        │
└─────────────────────────────────────────────────┘
```

### 8.2 Retry Mechanism

**Retry Configuration:**
```csharp
private const int MaxRetryAttempts = 3;
private static readonly int[] RetryDelaysInDays = { 2, 5, 7 };
```

**Retry Logic:**
```
RETRY #1 (2 days later - Nov 19):
┌─────────────────────────────────────────────────┐
│ AutomatedBillingService (Background Job)        │
│                                                  │
│ [1] Find subscriptions with failed payments     │
│     WHERE Status = "PaymentFailed"              │
│       AND FailedPaymentAttempts < 3             │
│                                                  │
│ [2] Attempt payment for each                    │
│     foreach (subscription in failedSubs) {      │
│       result = await _paymentService            │
│         .RetryFailedPaymentAsync(               │
│           billingRecordId                       │
│         );                                      │
│                                                  │
│       if (result.StatusCode == 200) {           │
│         // SUCCESS!                             │
│         subscription.Status = "Active";         │
│         subscription.FailedPaymentAttempts = 0; │
│         Send: "Payment received! Service        │
│           restored."                            │
│       } else {                                  │
│         // STILL FAILED                         │
│         subscription.FailedPaymentAttempts++;   │
│           // Now: 2                             │
│         Send: "Retry 1 failed. Please update    │
│           payment method."                      │
│       }                                         │
│     }                                           │
└─────────────────────────────────────────────────┘

RETRY #2 (5 days later - Nov 22):
  Same process... attempts: 2 → 3

RETRY #3 (7 days later - Nov 24) - FINAL ATTEMPT:
┌─────────────────────────────────────────────────┐
│ If payment SUCCEEDS:                            │
│   ✅ Status: Active                              │
│   ✅ FailedPaymentAttempts: 0                    │
│   ✅ Full access restored                        │
│                                                  │
│ If payment STILL FAILS:                         │
│   ⛔ Status: Suspended                           │
│   ⛔ Access: BLOCKED                             │
│   ⛔ Message: "Maximum retry attempts exceeded.  │
│       Subscription suspended. Pay now to        │
│       reactivate."                              │
└─────────────────────────────────────────────────┘
```

---

## 9. Code Examples

### 9.1 Creating Subscription Billing (Full Code)

```csharp
public async Task<JsonModel> CreateSubscriptionBillingAsync(
    Subscription subscription,
    decimal amount,
    string description,
    DateTime dueDate,
    TokenModel tokenModel)
{
    try
    {
        var billingRecord = new BillingRecord
        {
            Id = Guid.NewGuid(),
            UserId = subscription.UserId,
            SubscriptionId = subscription.Id,
            Type = BillingRecord.BillingType.Subscription,
            Status = BillingRecord.BillingStatus.Pending,
            Amount = amount,
            TotalAmount = amount,  // Can add tax/discount later
            TaxAmount = 0,
            DiscountAmount = 0,
            BillingDate = DateTime.UtcNow,
            DueDate = dueDate,
            InvoiceNumber = GenerateInvoiceNumber(),
            Description = description,
            PaymentMethod = "stripe",
            CurrencyId = subscription.SubscriptionPlan?.CurrencyId,
            CreatedBy = tokenModel?.UserID ?? 0,
            CreatedDate = DateTime.UtcNow
        };
        
        var created = await _billingRepository.CreateAsync(billingRecord);
        
        _logger.LogInformation(
            "Created billing record {Id} for subscription {SubId}: ${Amount}",
            created.Id, subscription.Id, amount
        );
        
        var billingRecordDto = _mapper.Map<BillingRecordDto>(created);
        
        return new JsonModel
        {
            data = billingRecordDto,
            Message = "Billing record created successfully",
            StatusCode = 200
        };
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error creating subscription billing");
        return new JsonModel
        {
            data = new object(),
            Message = $"Error: {ex.Message}",
            StatusCode = 500
        };
    }
}

private string GenerateInvoiceNumber()
{
    var year = DateTime.UtcNow.Year;
    var random = new Random().Next(10000, 99999);
    return $"INV-{year}-{random}";
}
```

### 9.2 Processing Overage Charge (Simplified)

```csharp
public async Task<JsonModel> ChargeOverageAsync(
    Guid subscriptionId,
    Guid privilegeId,
    int quantity,
    TokenModel tokenModel)
{
    await _unitOfWork.BeginTransactionAsync();
    
    try
    {
        // 1. Get subscription
        var subscription = await _subscriptionRepository
            .GetByIdWithDetailsAsync(subscriptionId);
        
        // 2. Get latest plan for pricing (abuse prevention)
        var latestPlan = await GetLatestPlanVersionAsync(
            subscription.SubscriptionPlanId
        );
        
        var planPrivilege = latestPlan.PlanPrivileges
            .FirstOrDefault(p => p.PrivilegeId == privilegeId);
        
        // 3. Calculate cost
        var unitCost = planPrivilege.UnitCost;
        var totalCost = quantity * unitCost;
        
        // 4. Create overage billing record
        var billingRecord = new BillingRecord
        {
            Type = BillingRecord.BillingType.Overage,
            Status = BillingRecord.BillingStatus.Pending,
            Amount = totalCost,
            TotalAmount = totalCost,
            Description = $"{quantity} extra {planPrivilege.Privilege.Name}",
            DueDate = DateTime.UtcNow  // Immediate payment required
        };
        
        var created = await _billingRepository.CreateAsync(billingRecord);
        
        // 5. Process payment IMMEDIATELY
        var paymentResult = await _paymentService.ProcessPaymentAsync(
            created.Id,
            tokenModel
        );
        
        if (paymentResult.StatusCode != 200)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return new JsonModel
            {
                StatusCode = 402,
                Message = "Payment failed"
            };
        }
        
        // 6. Add credits ONLY after payment succeeds
        var usage = await _privilegeUsageRepository
            .GetByUserAndPrivilegeAsync(subscription.UserId, privilegeId);
        
        usage.AllocatedLimit += quantity;
        usage.AllowedValue += quantity;
        
        await _privilegeUsageRepository.UpdateAsync(usage);
        
        // 7. Record in history
        await _privilegeUsageHistoryRepository.CreateAsync(
            new PrivilegeUsageHistory
            {
                UsageType = "Overage",
                Cost = totalCost,
                QuantityUsed = quantity
            }
        );
        
        // 8. Commit transaction
        await _unitOfWork.CommitTransactionAsync();
        
        return new JsonModel
        {
            data = new { BillingRecordId = created.Id, AmountCharged = totalCost },
            Message = "Overage charged successfully",
            StatusCode = 200
        };
    }
    catch (Exception ex)
    {
        await _unitOfWork.RollbackTransactionAsync();
        _logger.LogError(ex, "Failed to charge overage");
        return new JsonModel { StatusCode = 500, Message = ex.Message };
    }
}
```

---

## Key Takeaways

### ✅ Critical Concepts

1. **Billing Types**: Subscription, Overage, Consultation, Medication, Adjustment
2. **Payment States**: Pending → Paid/Failed → Refunded
3. **Upfront Payment**: Overage requires immediate payment before service
4. **Automated Renewal**: Stripe handles recurring billing, webhooks update DB
5. **Retry Logic**: 3 attempts over 7 days before suspension
6. **Transaction Safety**: Always use Unit of Work for atomic operations

### 🔍 Common Patterns

| Operation | Transaction? | Stripe Call? | Immediate? |
|-----------|--------------|--------------|------------|
| Create billing record | No | No | Yes |
| Process payment | Yes | Yes | Yes |
| Charge overage | Yes | Yes | Yes (upfront) |
| Process renewal | Yes | Via webhook | On renewal date |
| Retry failed payment | No | Yes | Scheduled |

---

## Next Steps

Continue to:
- **Guide 04**: Privilege Usage and Tracking
- **Guide 05**: Stripe Integration Deep Dive
- **Guide 06**: Automated Background Jobs

---

**Document Version:** 1.0  
**Last Updated:** October 17, 2025



