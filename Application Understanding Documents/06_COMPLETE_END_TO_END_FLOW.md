# 📘 Complete End-to-End Subscription Flow - Developer Guide

## Table of Contents
1. [Overview](#overview)
2. [Scenario 1: Happy Path (No Issues)](#scenario-1-happy-path)
3. [Scenario 2: Overage Scenario](#scenario-2-overage-scenario)
4. [Scenario 3: Payment Failure & Recovery](#scenario-3-payment-failure--recovery)
5. [Scenario 4: Trial Subscription](#scenario-4-trial-subscription)
6. [Scenario 5: Plan Upgrade](#scenario-5-plan-upgrade)
7. [Scenario 6: Cancellation](#scenario-6-cancellation)
8. [Service Interaction Map](#service-interaction-map)
9. [Method-to-Method Call Chain](#method-to-method-call-chain)

---

## 1. Overview

This guide shows you the **complete flow** of subscription management from start to finish, with all service interactions, method calls, and database changes visualized.

---

## 2. SCENARIO 1: Happy Path (No Issues)

### Complete Flow from Plan Creation to First Renewal

```
═══════════════════════════════════════════════════════════════
PHASE 1: ADMIN CREATES PLAN
═══════════════════════════════════════════════════════════════

┌─────────────────────────────────────────┐
│ Admin Portal (Frontend)                  │
│ Fills form:                              │
│   Name: "Basic Health"                   │
│   Privileges: 5 consultations, 3 meds    │
│   Auto-price: Yes                        │
│   Commission: 10%                        │
└────────────────┬────────────────────────┘
                 │ HTTP POST
                 ↓
┌─────────────────────────────────────────┐
│ SubscriptionPlansController.CreatePlan() │
│ File: API/Controllers/                   │
│   SubscriptionPlansController.cs         │
└────────────────┬────────────────────────┘
                 │ calls
                 ↓
┌─────────────────────────────────────────────────┐
│ SubscriptionPlanService.CreatePlanAsync()       │
│ File: Application/Services/                     │
│   SubscriptionPlanService.cs                    │
│ Line: 165-410                                   │
│                                                  │
│ ┌────────────────────────────────────────────┐ │
│ │ BEGIN TRANSACTION                          │ │
│ │ _unitOfWork.BeginTransactionAsync()        │ │
│ └────────────────────────────────────────────┘ │
│                 ↓                                │
│ ┌────────────────────────────────────────────┐ │
│ │ [1] Create Plan Entity                     │ │
│ │ _subscriptionPlanRepository.CreatePlanAsync│ │
│ │                                            │ │
│ │ DATABASE: SubscriptionPlans                │ │
│ │ INSERT:                                    │ │
│ │   Id: f3a1b2c3-...                         │ │
│ │   Name: "Basic Health"                     │ │
│ │   Price: 0 (will be calculated)            │ │
│ │   IsAutoCalculatedPrice: true              │ │
│ │   AdminCommissionPercent: 10               │ │
│ │   VersionNumber: 1                         │ │
│ │   IsLatestVersion: true                    │ │
│ └────────────────────────────────────────────┘ │
│                 ↓                                │
│ ┌────────────────────────────────────────────┐ │
│ │ [2] Create Stripe Product                  │ │
│ │ _stripeService.CreateProductAsync()        │ │
│ └────────────────────────────────────────────┘ │
└────────────────┬────────────────────────────────┘
                 │ API Call
                 ↓
┌─────────────────────────────────────────┐
│ StripeService.CreateProductAsync()       │
│ File: Infrastructure/Services/           │
│   StripeService.cs                       │
│ Line: 420-450                            │
│                                          │
│ var productOptions = new                 │
│   ProductCreateOptions                   │
│ {                                        │
│   Name = "Basic Health",                 │
│   Description = "...",                   │
│   Active = true,                         │
│   Metadata = new Dictionary {            │
│     { "planId", "f3a1b2c3-..." }         │
│   }                                      │
│ };                                       │
│                                          │
│ var service = new ProductService();      │
│ var product = await service.CreateAsync();│
│                                          │
│ STRIPE DATABASE:                         │
│ CREATE Product:                          │
│   ID: prod_ABC123                        │
│   Name: "Basic Health"                   │
│   Active: true                           │
│                                          │
│ Returns: "prod_ABC123"                   │
└────────────────┬────────────────────────┘
                 │
                 ↓
┌─────────────────────────────────────────────────┐
│ SubscriptionPlanService (continued)              │
│                                                  │
│ ┌────────────────────────────────────────────┐ │
│ │ [3] Update Plan with Stripe Product ID    │ │
│ │ plan.StripeProductId = "prod_ABC123"       │ │
│ │ _subscriptionPlanRepository.UpdatePlanAsync│ │
│ │                                            │ │
│ │ DATABASE: SubscriptionPlans                │ │
│ │ UPDATE:                                    │ │
│ │   StripeProductId = "prod_ABC123"          │ │
│ └────────────────────────────────────────────┘ │
│                 ↓                                │
│ ┌────────────────────────────────────────────┐ │
│ │ [4] Create Stripe Prices (3x)              │ │
│ │ _stripeService.CreatePriceAsync()          │ │
│ │   × 3 (Monthly, Quarterly, Annual)         │ │
│ │                                            │ │
│ │ Returns:                                   │ │
│ │   Monthly: price_1Month_XYZ                │ │
│ │   Quarterly: price_3Month_XYZ              │ │
│ │   Annual: price_12Month_XYZ                │ │
│ └────────────────────────────────────────────┘ │
│                 ↓                                │
│ ┌────────────────────────────────────────────┐ │
│ │ [5] Assign Privileges                      │ │
│ │ foreach (privilege in createDto.Privileges)│ │
│ │ {                                          │ │
│ │   _planPrivilegeRepository.CreateAsync(    │ │
│ │     new SubscriptionPlanPrivilege {        │ │
│ │       SubscriptionPlanId: f3a1b2c3-...     │ │
│ │       PrivilegeId: teleconsult-guid        │ │
│ │       Value: 5                             │ │
│ │       PrivilegeBaseCost: 20.00             │ │
│ │       UnitCost: 25.00                      │ │
│ │       MonthlyLimit: 5                      │ │
│ │     }                                      │ │
│ │   )                                        │ │
│ │ }                                          │ │
│ │                                            │ │
│ │ DATABASE: SubscriptionPlanPrivileges       │ │
│ │ INSERT (2 records):                        │ │
│ │   [1] Teleconsultation: 5 @ $20 base, $25 ovg│
│ │   [2] Medication: 3 @ $50 base, $60 ovg    │ │
│ └────────────────────────────────────────────┘ │
│                 ↓                                │
│ ┌────────────────────────────────────────────┐ │
│ │ [6] Auto-Calculate Price                   │ │
│ │ _pricingService.CalculatePricingBreakdownAsync│
│ └────────────────────────────────────────────┘ │
└────────────────┬────────────────────────────────┘
                 │ calls
                 ↓
┌─────────────────────────────────────────────────┐
│ PlanPricingService.CalculatePricingBreakdownAsync│
│ File: Application/Services/PlanPricingService.cs│
│ Line: 54-120                                    │
│                                                  │
│ // Get plan privileges                          │
│ planPrivileges = await _planRepo                │
│   .GetPlanPrivilegesAsync(planId);              │
│                                                  │
│ // Calculate privilege total                    │
│ privilegesTotalCost = 0;                        │
│ foreach (var pp in planPrivileges)              │
│ {                                               │
│   if (pp.Value > 0) {                           │
│     cost = pp.Value * pp.PrivilegeBaseCost;     │
│     privilegesTotalCost += cost;                │
│   }                                             │
│ }                                               │
│ // Result: (5×$20) + (3×$50) = $250             │
│                                                  │
│ // Calculate commission                         │
│ commissionPercent = plan.AdminCommissionPercent;│
│   // 10%                                        │
│ commission = privilegesTotalCost *              │
│   (commissionPercent / 100);                    │
│   // $250 × 0.10 = $25                          │
│                                                  │
│ // Calculate final price                        │
│ finalPrice = privilegesTotalCost + commission;  │
│   // $250 + $25 = $275                          │
│                                                  │
│ return new PricingBreakdown {                   │
│   FinalPrice = 275.00,                          │
│   PrivilegesTotalCost = 250.00,                 │
│   CommissionAmount = 25.00                      │
│ };                                              │
└────────────────┬────────────────────────────────┘
                 │ returns breakdown
                 ↓
┌─────────────────────────────────────────────────┐
│ SubscriptionPlanService (continued)              │
│                                                  │
│ ┌────────────────────────────────────────────┐ │
│ │ [7] Update Plan with Calculated Price      │ │
│ │ plan.Price = 275.00                        │ │
│ │ plan.PrivilegesTotalCost = 250.00          │ │
│ │ _subscriptionPlanRepository.UpdatePlanAsync│ │
│ │                                            │ │
│ │ DATABASE: SubscriptionPlans                │ │
│ │ UPDATE:                                    │ │
│ │   Price = 275.00                           │ │
│ │   PrivilegesTotalCost = 250.00             │ │
│ └────────────────────────────────────────────┘ │
│                 ↓                                │
│ ┌────────────────────────────────────────────┐ │
│ │ COMMIT TRANSACTION ✅                       │ │
│ │ _unitOfWork.CommitTransactionAsync()       │ │
│ └────────────────────────────────────────────┘ │
│                                                  │
│ RESULT:                                          │
│ ✅ Plan created in database                      │
│ ✅ Product created in Stripe                     │
│ ✅ Privileges assigned                           │
│ ✅ Price auto-calculated                         │
│ ✅ Fully synchronized                            │
└─────────────────────────────────────────────────┘

═══════════════════════════════════════════════════════════════
PHASE 2: USER SUBSCRIBES TO PLAN
═══════════════════════════════════════════════════════════════

┌─────────────────────────────────────────┐
│ User Portal (Frontend)                   │
│ User: John Doe (ID: 456)                │
│ Selects: "Basic Health" plan            │
│ Clicks: Subscribe                        │
└────────────────┬────────────────────────┘
                 │ HTTP POST
                 ↓
┌─────────────────────────────────────────┐
│ SubscriptionsController.CreateSubscription│
│ File: API/Controllers/                   │
│   SubscriptionsController.cs             │
└────────────────┬────────────────────────┘
                 │ calls
                 ↓
┌─────────────────────────────────────────────────┐
│ SubscriptionLifecycleService                     │
│   .CreateSubscriptionAsync()                     │
│ File: Application/Services/                     │
│   SubscriptionLifecycleService.cs               │
│ Line: 110-290                                   │
│                                                  │
│ ┌────────────────────────────────────────────┐ │
│ │ [1] Validate Plan                          │ │
│ │ plan = await _subscriptionPlanRepository   │ │
│ │   .GetByIdWithDetailsAsync(planId);        │ │
│ │                                            │ │
│ │ if (plan == null || !plan.IsActive)        │ │
│ │   return 400 "Invalid plan";               │ │
│ └────────────────────────────────────────────┘ │
│                 ↓                                │
│ ┌────────────────────────────────────────────┐ │
│ │ [2] Check Duplicate Subscription           │ │
│ │ existing = await _subscriptionRepository   │ │
│ │   .GetActiveSubscriptionByUserAndPlanAsync(│ │
│ │     userId: 456, planId: f3a1b2c3...       │ │
│ │   );                                       │ │
│ │                                            │ │
│ │ if (existing != null)                      │ │
│ │   return 400 "Already subscribed";         │ │
│ └────────────────────────────────────────────┘ │
│                 ↓                                │
│ ┌────────────────────────────────────────────┐ │
│ │ [3] Ensure Stripe Customer                 │ │
│ │ _stripeService.EnsureStripeCustomerAsync(  │ │
│ │   userId: 456                              │ │
│ │ )                                          │ │
│ └────────────────────────────────────────────┘ │
└────────────────┬────────────────────────────────┘
                 │ calls
                 ↓
┌─────────────────────────────────────────────────┐
│ StripeService.EnsureStripeCustomerAsync()       │
│ Line: 150-200                                   │
│                                                  │
│ user = await _userRepository.GetByIdAsync(456); │
│                                                  │
│ if (user.StripeCustomerId != null)              │
│   return user.StripeCustomerId;  // Already exists│
│                                                  │
│ // Create new customer in Stripe                │
│ var customerService = new CustomerService();    │
│ customer = await customerService.CreateAsync(   │
│   new CustomerCreateOptions {                   │
│     Email = "johndoe@example.com",              │
│     Name = "John Doe",                          │
│     Metadata = { { "userId", "456" } }          │
│   }                                             │
│ );                                              │
│                                                  │
│ STRIPE DATABASE:                                 │
│ CREATE Customer:                                 │
│   ID: cus_XYZ789                                │
│   Email: johndoe@example.com                    │
│                                                  │
│ // Update user in YOUR database                 │
│ user.StripeCustomerId = "cus_XYZ789";           │
│ await _userRepository.UpdateAsync(user);        │
│                                                  │
│ YOUR DATABASE: Users                             │
│ UPDATE:                                          │
│   StripeCustomerId = "cus_XYZ789"               │
│                                                  │
│ return "cus_XYZ789";                            │
└────────────────┬────────────────────────────────┘
                 │ returns to
                 ↓
┌─────────────────────────────────────────────────┐
│ SubscriptionLifecycleService (continued)         │
│                                                  │
│ stripeCustomerId = "cus_XYZ789" ✅               │
│                                                  │
│ ┌────────────────────────────────────────────┐ │
│ │ BEGIN TRANSACTION                          │ │
│ │ _unitOfWork.BeginTransactionAsync()        │ │
│ └────────────────────────────────────────────┘ │
│                 ↓                                │
│ ┌────────────────────────────────────────────┐ │
│ │ [4] Create Subscription Entity             │ │
│ │ subscription = new Subscription {          │ │
│ │   Id: sub_111,                             │ │
│ │   UserId: 456,                             │ │
│ │   SubscriptionPlanId: f3a1b2c3-...,        │ │
│ │   Status: "Pending",                       │ │
│ │   StartDate: 2025-10-17,                   │ │
│ │   EndDate: 2025-11-17,                     │ │
│ │   NextBillingDate: 2025-11-17,             │ │
│ │   CurrentPrice: 275.00,                    │ │
│ │   AutoRenew: true,                         │ │
│ │   StripeCustomerId: "cus_XYZ789"           │ │
│ │ };                                         │ │
│ │                                            │ │
│ │ created = await _subscriptionRepository    │ │
│ │   .CreateAsync(subscription);              │ │
│ │                                            │ │
│ │ DATABASE: Subscriptions                    │ │
│ │ INSERT: sub_111                            │ │
│ └────────────────────────────────────────────┘ │
│                 ↓                                │
│ ┌────────────────────────────────────────────┐ │
│ │ [5] Create Stripe Subscription             │ │
│ │ _stripeService.CreateSubscriptionAsync(    │ │
│ │   customerId: "cus_XYZ789",                │ │
│ │   priceId: "price_1Month_XYZ",             │ │
│ │   metadata: {                              │ │
│ │     subscriptionId: "sub_111",             │ │
│ │     planId: "f3a1b2c3-..."                 │ │
│ │   }                                        │ │
│ │ )                                          │ │
│ └────────────────────────────────────────────┘ │
└────────────────┬────────────────────────────────┘
                 │ API Call
                 ↓
┌─────────────────────────────────────────────────┐
│ StripeService.CreateSubscriptionAsync()         │
│ Line: 600-650                                   │
│                                                  │
│ var subscriptionService = new SubscriptionService();│
│ subscription = await subscriptionService        │
│   .CreateAsync(                                 │
│     new SubscriptionCreateOptions {             │
│       Customer = "cus_XYZ789",                  │
│       Items = [{ Price = "price_1Month_XYZ" }], │
│       Metadata = {                              │
│         { "subscriptionId", "sub_111" },        │
│         { "planId", "f3a1b2c3-..." }            │
│       }                                         │
│     }                                           │
│   );                                            │
│                                                  │
│ STRIPE DATABASE:                                 │
│ CREATE Subscription:                             │
│   ID: sub_stripe_AAA                            │
│   Customer: cus_XYZ789                          │
│   Price: price_1Month_XYZ                       │
│   Status: incomplete (awaiting payment)         │
│   Current_period: 2025-10-17 to 2025-11-17      │
│                                                  │
│ CREATE Invoice (automatic):                     │
│   ID: in_stripe_BBB                             │
│   Amount: $275.00                               │
│   Status: open                                  │
│                                                  │
│ CHARGE Payment Method (automatic):              │
│   Customer: cus_XYZ789                          │
│   Amount: $275.00                               │
│   Result: SUCCESS ✅                             │
│                                                  │
│ UPDATE Invoice:                                  │
│   Status: paid                                  │
│                                                  │
│ UPDATE Subscription:                             │
│   Status: active                                │
│                                                  │
│ SEND WEBHOOK:                                    │
│   Event: "invoice.payment_succeeded"            │
│   Payload: { invoice: {...}, subscription: {...} }│
│                                                  │
│ return "sub_stripe_AAA";                        │
└────────────────┬────────────────────────────────┘
                 │ returns to
                 ↓
┌─────────────────────────────────────────────────┐
│ SubscriptionLifecycleService (continued)         │
│                                                  │
│ stripeSubscriptionId = "sub_stripe_AAA" ✅       │
│                                                  │
│ ┌────────────────────────────────────────────┐ │
│ │ [6] Update Subscription with Stripe ID     │ │
│ │ subscription.StripeSubscriptionId =        │ │
│ │   "sub_stripe_AAA";                        │ │
│ │ await _subscriptionRepository.UpdateAsync();│ │
│ │                                            │ │
│ │ DATABASE: Subscriptions                    │ │
│ │ UPDATE:                                    │ │
│ │   StripeSubscriptionId = "sub_stripe_AAA"  │ │
│ └────────────────────────────────────────────┘ │
│                 ↓                                │
│ ┌────────────────────────────────────────────┐ │
│ │ [7] Initialize Privilege Usage             │ │
│ │ await InitializePrivilegeUsageAsync(       │ │
│ │   subscription, plan                       │ │
│ │ );                                         │ │
│ │                                            │ │
│ │ DATABASE: UserSubscriptionPrivilegeUsage   │ │
│ │ INSERT (2 records):                        │ │
│ │                                            │ │
│ │ [Record 1] Teleconsultation:               │ │
│ │   SubscriptionId: sub_111                  │ │
│ │   PrivilegeId: teleconsult-guid            │ │
│ │   AllocatedLimit: 5                        │ │
│ │   UsedValue: 0                             │ │
│ │   AllowedValue: 5                          │ │
│ │   UsagePeriodStart: 2025-10-17             │ │
│ │   UsagePeriodEnd: 2025-11-17               │ │
│ │                                            │ │
│ │ [Record 2] Medication:                     │ │
│ │   AllocatedLimit: 3                        │ │
│ │   UsedValue: 0                             │ │
│ │   AllowedValue: 3                          │ │
│ └────────────────────────────────────────────┘ │
│                 ↓                                │
│ ┌────────────────────────────────────────────┐ │
│ │ [8] Record Status History                  │ │
│ │ await RecordStatusChangeAsync(             │ │
│ │   subscriptionId: sub_111,                 │ │
│ │   oldStatus: null,                         │ │
│ │   newStatus: "Pending",                    │ │
│ │   reason: "Subscription created"           │ │
│ │ );                                         │ │
│ │                                            │ │
│ │ DATABASE: SubscriptionStatusHistory        │ │
│ │ INSERT:                                    │ │
│ │   FromStatus: NULL                         │ │
│ │   ToStatus: "Pending"                      │ │
│ │   Reason: "Subscription created"           │ │
│ └────────────────────────────────────────────┘ │
│                 ↓                                │
│ ┌────────────────────────────────────────────┐ │
│ │ [9] Create Initial Billing Record          │ │
│ │ _billingService.CreateSubscriptionBillingAsync│
│ └────────────────────────────────────────────┘ │
└────────────────┬────────────────────────────────┘
                 │ calls
                 ↓
┌─────────────────────────────────────────────────┐
│ SubscriptionBillingService                       │
│   .CreateSubscriptionBillingAsync()              │
│ Line: 85-150                                    │
│                                                  │
│ billingRecord = new BillingRecord {             │
│   Id: bill_001,                                 │
│   SubscriptionId: sub_111,                      │
│   UserId: 456,                                  │
│   Type: "Subscription",                         │
│   Status: "Pending",                            │
│   Amount: 275.00,                               │
│   TotalAmount: 275.00,                          │
│   DueDate: 2025-10-17,                          │
│   InvoiceNumber: "INV-2025-001"                 │
│ };                                              │
│                                                  │
│ await _billingRepository.CreateAsync(           │
│   billingRecord                                 │
│ );                                              │
│                                                  │
│ DATABASE: BillingRecords                         │
│ INSERT: bill_001                                │
└────────────────┬────────────────────────────────┘
                 │ returns to
                 ↓
┌─────────────────────────────────────────────────┐
│ SubscriptionLifecycleService (continued)         │
│                                                  │
│ ┌────────────────────────────────────────────┐ │
│ │ COMMIT TRANSACTION ✅                       │ │
│ │ _unitOfWork.CommitTransactionAsync()       │ │
│ └────────────────────────────────────────────┘ │
│                                                  │
│ DATABASE STATE NOW:                              │
│ ✅ Subscription: sub_111 (Status: Pending)       │
│ ✅ Privileges: Initialized (5 consult, 3 meds)   │
│ ✅ Billing Record: bill_001 (Status: Pending)    │
│ ✅ Status History: Recorded                      │
│ ✅ Stripe Customer: cus_XYZ789                   │
│ ✅ Stripe Subscription: sub_stripe_AAA (active)  │
└─────────────────────────────────────────────────┘

═══════════════════════════════════════════════════════════════
PHASE 3: STRIPE PROCESSES PAYMENT (Automatic)
═══════════════════════════════════════════════════════════════

(Happens automatically when Stripe subscription created)

┌─────────────────────────────────────────────────┐
│ STRIPE Internal Process                         │
│                                                  │
│ [1] Subscription created → Create invoice       │
│ [2] Invoice created → Charge payment method     │
│ [3] Payment succeeds → Update invoice to "paid" │
│ [4] Send webhook: "invoice.payment_succeeded"   │
└────────────────┬────────────────────────────────┘
                 │ HTTP POST Webhook
                 ↓
┌─────────────────────────────────────────────────┐
│ StripeWebhookController.HandleWebhook()         │
│ File: API/Controllers/StripeWebhookController.cs│
│ Line: 96-160                                    │
│                                                  │
│ [1] Validate Signature ✅                        │
│ [2] Check Idempotency ✅                         │
│ [3] Route to handler based on event type        │
└────────────────┬────────────────────────────────┘
                 │ calls
                 ↓
┌─────────────────────────────────────────────────┐
│ StripeWebhookController                          │
│   .HandlePaymentSucceeded()                      │
│ Line: 540-600                                   │
│                                                  │
│ invoice = stripeEvent.Data.Object;              │
│   // ID: in_stripe_BBB                          │
│   // Amount: $275.00                            │
│   // Subscription: sub_stripe_AAA               │
│                                                  │
│ ┌────────────────────────────────────────────┐ │
│ │ [1] Extract Subscription ID from Metadata  │ │
│ │ subscriptionId = invoice.Metadata           │ │
│ │   ["subscriptionId"];  // "sub_111"        │ │
│ └────────────────────────────────────────────┘ │
│                 ↓                                │
│ ┌────────────────────────────────────────────┐ │
│ │ [2] Find Local Subscription                │ │
│ │ subscription = await _subscriptionRepository││
│ │   .GetByStripeSubscriptionIdAsync(         │ │
│ │     "sub_stripe_AAA"                       │ │
│ │   );                                       │ │
│ └────────────────────────────────────────────┘ │
│                 ↓                                │
│ ┌────────────────────────────────────────────┐ │
│ │ BEGIN TRANSACTION                          │ │
│ │ _unitOfWork.BeginTransactionAsync()        │ │
│ └────────────────────────────────────────────┘ │
│                 ↓                                │
│ ┌────────────────────────────────────────────┐ │
│ │ [3] Update Subscription to Active          │ │
│ │ subscription.Status = "Active";            │ │
│ │ subscription.LastPaymentDate = UtcNow;     │ │
│ │ await _subscriptionRepository.UpdateAsync();│ │
│ │                                            │ │
│ │ DATABASE: Subscriptions                    │ │
│ │ UPDATE sub_111:                            │ │
│ │   Status = "Active" ✅                      │ │
│ │   LastPaymentDate = 2025-10-17             │ │
│ └────────────────────────────────────────────┘ │
│                 ↓                                │
│ ┌────────────────────────────────────────────┐ │
│ │ [4] Update Billing Record                  │ │
│ │ billingRecord.Status = "Paid";             │ │
│ │ billingRecord.PaidDate = UtcNow;           │ │
│ │ billingRecord.StripeInvoiceId =            │ │
│ │   "in_stripe_BBB";                         │ │
│ │ await _billingRepository.UpdateAsync();    │ │
│ │                                            │ │
│ │ DATABASE: BillingRecords                   │ │
│ │ UPDATE bill_001:                           │ │
│ │   Status = "Paid" ✅                        │ │
│ │   PaidDate = 2025-10-17                    │ │
│ │   StripeInvoiceId = "in_stripe_BBB"        │ │
│ └────────────────────────────────────────────┘ │
│                 ↓                                │
│ ┌────────────────────────────────────────────┐ │
│ │ [5] Create Payment Record                  │ │
│ │ payment = new SubscriptionPayment {        │ │
│ │   SubscriptionId: sub_111,                 │ │
│ │   BillingRecordId: bill_001,               │ │
│ │   Amount: 275.00,                          │ │
│ │   Status: "Success",                       │ │
│ │   PaymentDate: 2025-10-17                  │ │
│ │ };                                         │ │
│ │ await _paymentRepository.CreateAsync();    │ │
│ │                                            │ │
│ │ DATABASE: SubscriptionPayments             │ │
│ │ INSERT: pay_001                            │ │
│ └────────────────────────────────────────────┘ │
│                 ↓                                │
│ ┌────────────────────────────────────────────┐ │
│ │ [6] Record Status Change                   │ │
│ │ await _statusHistoryRepository.CreateAsync(│ │
│ │   new SubscriptionStatusHistory {          │ │
│ │     FromStatus: "Pending",                 │ │
│ │     ToStatus: "Active",                    │ │
│ │     Reason: "Payment successful"           │ │
│ │   }                                        │ │
│ │ );                                         │ │
│ │                                            │ │
│ │ DATABASE: SubscriptionStatusHistory        │ │
│ │ INSERT: Pending → Active                   │ │
│ └────────────────────────────────────────────┘ │
│                 ↓                                │
│ ┌────────────────────────────────────────────┐ │
│ │ COMMIT TRANSACTION ✅                       │ │
│ │ _unitOfWork.CommitTransactionAsync()       │ │
│ └────────────────────────────────────────────┘ │
│                 ↓                                │
│ ┌────────────────────────────────────────────┐ │
│ │ [7] Send Confirmation Email                │ │
│ │ _notificationService.SendSubscription      │ │
│ │   ConfirmationAsync(...)                   │ │
│ └────────────────────────────────────────────┘ │
│                                                  │
│ return 200 OK to Stripe ✅                       │
└─────────────────────────────────────────────────┘

✅ SUBSCRIPTION IS NOW ACTIVE!
   User: John Doe
   Plan: Basic Health ($275/month)
   Privileges: 5 consultations, 3 medications
   Next Billing: Nov 17, 2025
```

**Continue reading in the individual guides for more scenarios...**

---

## Next: Read the Guides in Order

1. **[Guide 01: Subscription Plan Management](./01_SUBSCRIPTION_PLAN_MANAGEMENT_GUIDE.md)**
2. **[Guide 02: User Subscription Lifecycle](./02_USER_SUBSCRIPTION_LIFECYCLE_GUIDE.md)**
3. **[Guide 03: Billing and Payment Processing](./03_BILLING_AND_PAYMENT_PROCESSING_GUIDE.md)**
4. **[Guide 04: Privilege Management and Tracking](./04_PRIVILEGE_MANAGEMENT_AND_TRACKING_GUIDE.md)**
5. **[Guide 05: Stripe Integration](./05_STRIPE_INTEGRATION_GUIDE.md)**
6. **[Guide 06: Complete End-to-End Flow](./06_COMPLETE_END_TO_END_FLOW.md)** ← Coming next

---

**Document Version:** 1.0  
**Last Updated:** October 17, 2025

