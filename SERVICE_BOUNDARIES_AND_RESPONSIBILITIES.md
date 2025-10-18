# Service Boundaries & Responsibilities Guide

**Purpose:** Definitive guide for developers on which service to use for what operation  
**Status:** ✅ Current as of SRP Refactoring (93% compliance)  
**Last Updated:** October 15, 2025

---

## 📋 QUICK REFERENCE: WHICH SERVICE TO USE

```
┌─────────────────────────────────────────────────────────────────┐
│                    OPERATION → SERVICE MAPPING                  │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  💳 BILLING OPERATIONS        → BillingService                 │
│  💰 PAYMENT OPERATIONS        → PaymentService                 │
│  📊 SUBSCRIPTION LIFECYCLE    → SubscriptionLifecycleService   │
│  📝 SUBSCRIPTION BUSINESS     → SubscriptionService            │
│  🎫 PRIVILEGE MANAGEMENT      → PrivilegeService               │
│  🔄 AUTOMATED TASKS           → SubscriptionAutomationService  │
│  💳 STRIPE INTEGRATION        → StripeService                  │
│  📧 NOTIFICATIONS             → SubscriptionNotificationService│
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🎯 CORE SERVICES (Application Layer)

### **1. BillingService** 📊
**Responsibility:** Billing record management, calculations, and billing history

#### **Use This Service When You Need To:**
- ✅ Create billing records (subscription, overage, consultation, medication)
- ✅ Calculate next billing dates
- ✅ Retrieve billing history
- ✅ Process payments
- ✅ Handle refunds
- ✅ Apply billing adjustments
- ✅ Generate invoices

#### **Key Methods:**

**Centralized Factory Methods (SRP Refactoring):**
```csharp
// Subscription billing
await billingService.CreateSubscriptionBillingAsync(
    subscription, amount, description, dueDate, tokenModel
);

// Overage/extra usage billing
await billingService.CreateOverageBillingAsync(
    subscription, privilegeName, amount, tokenModel
);

// Consultation billing
await billingService.CreateConsultationBillingAsync(
    userId, consultationId, amount, description, tokenModel
);

// Medication billing
await billingService.CreateMedicationBillingAsync(
    subscription, amount, description, tokenModel
);
```

**Billing Date Calculation (SRP Refactoring):**
```csharp
// Calculate next billing date
var nextDate = billingService.CalculateNextBillingDate(
    currentDate, billingCycle
);

// Calculate for specific subscription
var nextDate = await billingService.CalculateNextBillingDateForSubscriptionAsync(
    subscriptionId, tokenModel
);
```

**Payment Processing:**
```csharp
// Process payment for billing record
await billingService.ProcessPaymentAsync(billingRecordId, tokenModel);

// Process refund
await billingService.ProcessRefundAsync(billingRecordId, amount, reason, tokenModel);
```

**Billing History:**
```csharp
// Get subscription billing history
await billingService.GetSubscriptionBillingHistoryAsync(subscriptionId, tokenModel);

// Get user billing history
await billingService.GetUserBillingHistoryAsync(userId, tokenModel);
```

#### **When NOT to Use:**
- ❌ Don't use for payment method management → Use **PaymentService**
- ❌ Don't use for subscription lifecycle → Use **SubscriptionLifecycleService**
- ❌ Don't use for Stripe customer creation → Use **StripeService**

---

### **2. PaymentService** 💰
**Responsibility:** Payment processing and payment method management

#### **Use This Service When You Need To:**
- ✅ Get user's payment methods
- ✅ Add payment methods
- ✅ Process payments
- ✅ Handle refunds
- ✅ Retry failed payments
- ✅ Validate payment methods

#### **Key Methods:**

**Payment Method Management (SRP Refactoring - Moved from SubscriptionService):**
```csharp
// Get user's payment methods
await paymentService.GetPaymentMethodsAsync(userId, tokenModel);

// Add payment method
await paymentService.AddPaymentMethodAsync(userId, paymentMethodId, tokenModel);

// Update payment method for billing record
await paymentService.UpdatePaymentMethodAsync(billingRecordId, paymentMethodId, tokenModel);
```

**Payment Processing:**
```csharp
// Process payment
await paymentService.ProcessPaymentAsync(billingRecordId, tokenModel);

// Retry failed payment
await paymentService.RetryFailedPaymentAsync(billingRecordId, tokenModel);

// Process partial payment
await paymentService.ProcessPartialPaymentAsync(billingRecordId, amount, tokenModel);
```

**Payment History:**
```csharp
// Get payment history
await paymentService.GetPaymentHistoryAsync(userId, startDate, endDate, tokenModel);

// Get payment analytics
await paymentService.GetPaymentAnalyticsAsync(startDate, endDate, tokenModel);
```

#### **When NOT to Use:**
- ❌ Don't use for billing record creation → Use **BillingService**
- ❌ Don't use for subscription operations → Use **SubscriptionService**

---

### **3. SubscriptionService** 📝
**Responsibility:** Subscription business logic and coordination

#### **Use This Service When You Need To:**
- ✅ Get subscription details
- ✅ Get user's subscriptions
- ✅ Purchase additional credits/privileges
- ✅ Check privilege availability
- ✅ Coordinate subscription operations

#### **Key Methods:**

**Subscription Queries:**
```csharp
// Get subscription by ID
await subscriptionService.GetSubscriptionAsync(subscriptionId, tokenModel);

// Get user's subscriptions
await subscriptionService.GetUserSubscriptionsAsync(userId, tokenModel);

// Get all subscriptions (admin)
await subscriptionService.GetAllSubscriptionsAsync(page, pageSize, tokenModel);
```

**Privilege Operations:**
```csharp
// Purchase additional credits (SRP: Upfront Payment Feature)
await subscriptionService.PurchaseAdditionalCreditsAsync(
    subscriptionId, 
    new PurchaseAdditionalCreditsDto {
        PrivilegeName = "Teleconsultation",
        Quantity = 5,
        PaymentMethodId = "pm_xxx"
    }, 
    tokenModel
);

// Check privilege availability
await subscriptionService.CheckPrivilegeAvailabilityAsync(
    subscriptionId, privilegeName, requestedAmount, tokenModel
);
```

#### **Deprecated Methods (Use Alternative Services):**
```csharp
// ❌ DEPRECATED - Use PaymentService instead
await subscriptionService.GetPaymentMethodsAsync(userId, tokenModel);
await subscriptionService.AddPaymentMethodAsync(userId, paymentMethodId, tokenModel);

// ❌ DEPRECATED - Use BillingService instead
await subscriptionService.GetBillingHistoryAsync(subscriptionId, tokenModel);

// ❌ DEPRECATED - Use CategoryService instead
await subscriptionService.GetAllCategoriesAsync(page, pageSize, searchTerm, isActive, tokenModel);

// ❌ DEPRECATED - Move to ConsultationService (future)
await subscriptionService.BookConsultationAsync(userId, subscriptionId, tokenModel);

// ❌ DEPRECATED - Move to MedicationService (future)
await subscriptionService.RequestMedicationSupplyAsync(userId, subscriptionId, tokenModel);
```

#### **When NOT to Use:**
- ❌ Don't use for payment methods → Use **PaymentService**
- ❌ Don't use for billing history → Use **BillingService**
- ❌ Don't use for lifecycle operations → Use **SubscriptionLifecycleService**

---

### **4. SubscriptionLifecycleService** 📊
**Responsibility:** Subscription state management and lifecycle transitions

#### **Use This Service When You Need To:**
- ✅ Create subscriptions
- ✅ Cancel subscriptions
- ✅ Pause/resume subscriptions
- ✅ Renew subscriptions
- ✅ Activate/suspend subscriptions
- ✅ Handle trial subscriptions
- ✅ Manage subscription status transitions

#### **Key Methods:**

**Subscription Lifecycle:**
```csharp
// Create new subscription
await lifecycleService.CreateSubscriptionAsync(createDto, tokenModel);

// Cancel subscription
await lifecycleService.CancelSubscriptionAsync(subscriptionId, reason, tokenModel);

// Pause subscription
await lifecycleService.PauseSubscriptionAsync(subscriptionId, tokenModel);

// Resume subscription
await lifecycleService.ResumeSubscriptionAsync(subscriptionId, tokenModel);

// Renew subscription
await lifecycleService.RenewSubscriptionAsync(subscriptionId, tokenModel);
```

**Status Management:**
```csharp
// Activate subscription
await lifecycleService.ActivateSubscriptionAsync(subscriptionId, reason, tokenModel);

// Suspend subscription
await lifecycleService.SuspendSubscriptionAsync(subscriptionId, reason, tokenModel);

// Expire subscription
await lifecycleService.ExpireSubscriptionAsync(subscriptionId, reason, tokenModel);
```

**Trial Management:**
```csharp
// Extend trial
await lifecycleService.ExtendTrialAsync(subscriptionId, additionalDays, reason, tokenModel);

// Convert trial to paid
await lifecycleService.ConvertTrialToPaidAsync(subscriptionId, tokenModel);
```

#### **Internal Helpers (SRP Refactoring):**
The service uses centralized helpers:
- `RecordStatusChangeAsync()` - Consolidated 20+ duplicate status history creations
- `CalculateNextBillingDate()` - Delegates to BillingService
- `EnsureStripeCustomerAsync()` - Delegates to StripeService

#### **When NOT to Use:**
- ❌ Don't use for billing operations → Use **BillingService**
- ❌ Don't use for payment processing → Use **PaymentService**

---

### **5. PrivilegeService** 🎫
**Responsibility:** Privilege management, usage tracking, and validation

#### **Use This Service When You Need To:**
- ✅ Check privilege availability
- ✅ Use/consume privileges
- ✅ Get remaining privileges
- ✅ Reset privilege usage
- ✅ Validate privilege access

#### **Key Methods:**

**Privilege Availability (SRP Refactoring - Enhanced):**
```csharp
// Check if privilege is available (with payment required response)
await privilegeService.CheckPrivilegeAvailabilityAsync(
    subscriptionId, privilegeName, requestedAmount, tokenModel
);
// Returns 402 Payment Required if limit exceeded, with purchase details
```

**Privilege Usage:**
```csharp
// Use privilege
await privilegeService.UsePrivilegeAsync(
    subscriptionId, privilegeName, amount, tokenModel
);

// Get remaining privilege
var remaining = await privilegeService.GetRemainingPrivilegeAsync(
    subscriptionId, privilegeName, tokenModel
);

// Reset privilege usage (for new billing cycle)
await privilegeService.ResetPrivilegeUsageAsync(
    subscriptionId, privilegeName, tokenModel
);
```

#### **When NOT to Use:**
- ❌ Don't use for purchasing additional privileges → Use **SubscriptionService.PurchaseAdditionalCreditsAsync()**
- ❌ Don't use for billing overage → Use **BillingService.CreateOverageBillingAsync()**

---

### **6. SubscriptionAutomationService** 🔄
**Responsibility:** Automated subscription tasks and scheduled jobs

#### **Use This Service When You Need To:**
- ✅ Process subscription renewals (automated)
- ✅ Handle trial expirations
- ✅ Retry failed payments
- ✅ Send billing reminders
- ✅ Reset privilege usage on cycle change

#### **Key Methods:**

**Automated Operations:**
```csharp
// Process renewals
await automationService.ProcessSubscriptionRenewalsAsync(tokenModel);

// Handle trial expirations
await automationService.ProcessTrialExpirationsAsync(tokenModel);

// Retry failed payments
await automationService.RetryFailedPaymentsAsync(tokenModel);

// Send billing reminders
await automationService.SendBillingRemindersAsync(tokenModel);
```

#### **When NOT to Use:**
- ❌ Don't use for manual subscription operations → Use **SubscriptionLifecycleService**
- ❌ Don't use for billing operations → Use **BillingService**

---

### **7. PrivilegeBasedBillingService** 💳
**Responsibility:** Privilege-based billing calculations and overage management

#### **Use This Service When You Need To:**
- ✅ Calculate overage charges
- ✅ Process subscription renewal with overage
- ✅ Handle privilege usage billing

#### **Key Methods:**

```csharp
// Calculate overage for subscription
await privilegeBillingService.CalculateOverageChargesAsync(subscriptionId, tokenModel);

// Process renewal with overage handling
await privilegeBillingService.ProcessSubscriptionRenewalAsync(subscriptionId, tokenModel);
```

#### **Note:** This service uses **BillingService.CreateOverageBillingAsync()** for billing record creation (SRP refactoring)

---

## 🔧 INFRASTRUCTURE SERVICES

### **8. StripeService** 💳
**Responsibility:** Core Stripe API operations

**Location:** `SmartTelehealth.Infrastructure/Services/StripeService.cs`

#### **Use This Service When You Need To:**
- ✅ Create Stripe customers
- ✅ Manage Stripe subscriptions
- ✅ Create products and prices
- ✅ Process Stripe payments
- ✅ Handle payment methods in Stripe
- ✅ Ensure Stripe customer exists (SRP Refactoring - Centralized)

#### **Key Methods:**

**Customer Management (SRP Refactoring - Centralized):**
```csharp
// Ensure Stripe customer exists (CENTRALIZED - was duplicated in 3 services)
await stripeService.EnsureStripeCustomerAsync(
    userId, email, fullName, existingStripeCustomerId, tokenModel
);

// Create Stripe customer
await stripeService.CreateCustomerAsync(email, name, tokenModel);

// Get customer details
await stripeService.GetCustomerAsync(customerId, tokenModel);
```

**Subscription Management:**
```csharp
// Create Stripe subscription
await stripeService.CreateSubscriptionAsync(customerId, priceId, paymentMethodId, tokenModel);

// Cancel Stripe subscription
await stripeService.CancelSubscriptionAsync(subscriptionId, tokenModel);

// Update subscription
await stripeService.UpdateSubscriptionAsync(subscriptionId, priceId, tokenModel);
```

**Product & Price Management:**
```csharp
// Create product
await stripeService.CreateProductAsync(name, description, tokenModel);

// Create price
await stripeService.CreatePriceAsync(productId, amount, currency, interval, intervalCount, tokenModel);
```

#### **When NOT to Use:**
- ❌ Don't use for business logic → Use application layer services

---

### **9. StripeBillingService** 💳
**Responsibility:** Stripe-specific billing operations

**Location:** `SmartTelehealth.Infrastructure/Services/StripeBillingService.cs`

#### **Use This Service When You Need To:**
- ✅ Create Stripe payment intents
- ✅ Capture Stripe payments
- ✅ Handle Stripe invoices
- ✅ Process Stripe refunds

#### **Key Methods:**

```csharp
// Create payment intent
await stripeBillingService.CreatePaymentIntentAsync(amount, currency, customerId, tokenModel);

// Capture payment
await stripeBillingService.CapturePaymentAsync(paymentIntentId, tokenModel);

// Create invoice
await stripeBillingService.CreateInvoiceAsync(customerId, items, tokenModel);
```

---

### **10. StripeSynchronizationService** 🔄
**Responsibility:** Stripe data synchronization

#### **Use This Service When You Need To:**
- ✅ Sync local subscriptions with Stripe
- ✅ Handle Stripe webhook events
- ✅ Reconcile payment data

---

## 📋 SERVICE DEPENDENCY GRAPH

```
┌─────────────────────────────────────────────────────────────┐
│                    SERVICE DEPENDENCIES                     │
│                    (who calls whom)                         │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  SubscriptionService                                        │
│  ├─→ BillingService (billing operations)                   │
│  ├─→ PaymentService (payment methods)                      │
│  ├─→ PrivilegeService (privilege checks)                   │
│  ├─→ StripeService (Stripe operations)                     │
│  └─→ NotificationService (notifications)                   │
│                                                             │
│  SubscriptionLifecycleService                               │
│  ├─→ BillingService (billing records, date calculation)    │
│  ├─→ StripeService (customer creation, subscriptions)      │
│  ├─→ PrivilegeService (privilege setup)                    │
│  └─→ NotificationService (lifecycle notifications)         │
│                                                             │
│  BillingService                                             │
│  ├─→ PaymentService (payment processing)                   │
│  └─→ StripeService (payment intents)                       │
│                                                             │
│  PaymentService                                             │
│  ├─→ StripeBillingService (Stripe payment operations)      │
│  └─→ StripeService (payment methods)                       │
│                                                             │
│  PrivilegeBasedBillingService                               │
│  ├─→ BillingService (billing record creation)              │
│  ├─→ PrivilegeService (privilege usage)                    │
│  └─→ SubscriptionRepository (subscription data)            │
│                                                             │
│  SubscriptionAutomationService                              │
│  ├─→ BillingService (automated billing)                    │
│  ├─→ SubscriptionLifecycleService (renewals, expirations)  │
│  └─→ NotificationService (automated notifications)         │
│                                                             │
└─────────────────────────────────────────────────────────────┘

✅ CLEAN ARCHITECTURE: No circular dependencies
✅ CLEAR LAYERING: Application → Infrastructure
✅ SRP COMPLIANCE: Each service has single responsibility
```

---

## 🚫 ANTI-PATTERNS TO AVOID

### **❌ Don't Do This:**

```csharp
// ❌ Creating billing records manually
var billingRecord = new BillingRecord { ... };
await billingRepository.CreateAsync(billingRecord);

// ✅ Use centralized factory method instead
await billingService.CreateSubscriptionBillingAsync(
    subscription, amount, description, dueDate, tokenModel
);
```

```csharp
// ❌ Calculating billing dates manually
var nextDate = subscription.BillingCycle.Name == "Monthly" 
    ? DateTime.UtcNow.AddMonths(1) 
    : DateTime.UtcNow.AddMonths(3);

// ✅ Use centralized calculation instead
var nextDate = billingService.CalculateNextBillingDate(
    DateTime.UtcNow, subscription.BillingCycle
);
```

```csharp
// ❌ Creating status history manually
var history = new SubscriptionStatusHistory {
    SubscriptionId = id,
    FromStatus = oldStatus,
    ToStatus = newStatus,
    ...
};
await statusHistoryRepository.CreateAsync(history);

// ✅ Use centralized helper instead (in SubscriptionLifecycleService)
await RecordStatusChangeAsync(id, oldStatus, newStatus, reason, tokenModel);
```

```csharp
// ❌ Using deprecated methods
await subscriptionService.GetPaymentMethodsAsync(userId, tokenModel);

// ✅ Use correct service instead
await paymentService.GetPaymentMethodsAsync(userId, tokenModel);
```

---

## 📊 DECISION FLOWCHART

```
START: What do you need to do?
│
├─ Need to create billing record?
│  └─→ Use BillingService.Create*BillingAsync()
│
├─ Need to process payment?
│  └─→ Use PaymentService.ProcessPaymentAsync()
│
├─ Need to manage payment methods?
│  └─→ Use PaymentService (GetPaymentMethods, AddPaymentMethod)
│
├─ Need to change subscription status?
│  └─→ Use SubscriptionLifecycleService (Cancel, Pause, Resume, etc.)
│
├─ Need to create new subscription?
│  └─→ Use SubscriptionLifecycleService.CreateSubscriptionAsync()
│
├─ Need to check privilege availability?
│  └─→ Use PrivilegeService.CheckPrivilegeAvailabilityAsync()
│
├─ Need to purchase additional credits?
│  └─→ Use SubscriptionService.PurchaseAdditionalCreditsAsync()
│
├─ Need to interact with Stripe?
│  └─→ Use StripeService (customers, products, subscriptions)
│
├─ Need to calculate billing date?
│  └─→ Use BillingService.CalculateNextBillingDate()
│
└─ Need automated/scheduled task?
   └─→ Use SubscriptionAutomationService
```

---

## 🎯 COMMON SCENARIOS

### **Scenario 1: User Subscribes to a Plan**

```csharp
// 1. Create subscription (lifecycle)
var subscription = await subscriptionLifecycleService.CreateSubscriptionAsync(
    new CreateSubscriptionDto {
        UserId = userId,
        SubscriptionPlanId = planId,
        BillingCycleId = billingCycleId
    },
    tokenModel
);

// 2. Initial billing record created automatically by lifecycle service
// (Uses BillingService.CreateSubscriptionBillingAsync internally)

// 3. Process initial payment
await paymentService.ProcessPaymentAsync(billingRecordId, tokenModel);
```

### **Scenario 2: User Exceeds Privilege Limit**

```csharp
// 1. Check privilege availability
var availabilityResult = await privilegeService.CheckPrivilegeAvailabilityAsync(
    subscriptionId, "Teleconsultation", 1, tokenModel
);

if (availabilityResult.StatusCode == 402) // Payment Required
{
    // 2. Purchase additional credits
    await subscriptionService.PurchaseAdditionalCreditsAsync(
        subscriptionId,
        new PurchaseAdditionalCreditsDto {
            PrivilegeName = "Teleconsultation",
            Quantity = 5,
            PaymentMethodId = paymentMethodId
        },
        tokenModel
    );
}

// 3. Use privilege
await privilegeService.UsePrivilegeAsync(
    subscriptionId, "Teleconsultation", 1, tokenModel
);
```

### **Scenario 3: Monthly Billing Automation**

```csharp
// Run automated job (scheduled task)
await subscriptionAutomationService.ProcessSubscriptionRenewalsAsync(tokenModel);

// This internally:
// - Gets subscriptions due for renewal
// - Creates billing records (via BillingService)
// - Processes payments (via PaymentService)
// - Updates subscription status (via SubscriptionLifecycleService)
// - Resets privilege usage (via PrivilegeService)
```

---

## 📚 MIGRATION GUIDE

### **From Old Patterns to New Patterns:**

#### **Old: Billing Record Creation (Pre-Refactoring)**
```csharp
// ❌ OLD - Duplicated in 7 services
var billingRecord = new CreateBillingRecordDto {
    UserId = subscription.UserId,
    SubscriptionId = subscription.Id.ToString(),
    Amount = amount,
    ...
};
await billingService.CreateBillingRecordAsync(billingRecord, tokenModel);
```

#### **New: Centralized Factory Methods**
```csharp
// ✅ NEW - Use factory method
await billingService.CreateSubscriptionBillingAsync(
    subscription, amount, description, dueDate, tokenModel
);
```

---

#### **Old: Payment Methods in SubscriptionService**
```csharp
// ❌ DEPRECATED - SRP violation
await subscriptionService.GetPaymentMethodsAsync(userId, tokenModel);
```

#### **New: Payment Methods in PaymentService**
```csharp
// ✅ NEW - Correct service
await paymentService.GetPaymentMethodsAsync(userId, tokenModel);
```

---

#### **Old: Manual Stripe Customer Creation**
```csharp
// ❌ OLD - Duplicated in 3 services
if (string.IsNullOrEmpty(user.StripeCustomerId)) {
    var customerId = await stripeService.CreateCustomerAsync(email, name, token);
    await userService.UpdateUserAsync(userId, new UpdateUserDto {
        StripeCustomerId = customerId
    }, token);
    return customerId;
}
return user.StripeCustomerId;
```

#### **New: Centralized EnsureStripeCustomer**
```csharp
// ✅ NEW - Single centralized method
var customerId = await stripeService.EnsureStripeCustomerAsync(
    userId, email, fullName, existingStripeCustomerId, tokenModel
);
```

---

## ✅ BEST PRACTICES

### **1. Always Use the Right Service**
- Don't bypass service boundaries
- Don't create billing records directly
- Don't duplicate logic

### **2. Use Factory Methods**
- For billing records: Use `BillingService.Create*BillingAsync()`
- For status history: Use `RecordStatusChangeAsync()` (in LifecycleService)
- For Stripe customers: Use `StripeService.EnsureStripeCustomerAsync()`

### **3. Follow the Dependency Graph**
- Application services → Infrastructure services (✅ Correct)
- Infrastructure services → Application services (❌ Wrong)

### **4. Monitor Deprecated Methods**
- Watch deprecation logs
- Migrate gradually
- Update to new patterns

---

## 📞 QUICK REFERENCE CHEAT SHEET

| Operation | Service | Method |
|-----------|---------|--------|
| Create billing record | BillingService | `CreateSubscriptionBillingAsync()` |
| Calculate billing date | BillingService | `CalculateNextBillingDate()` |
| Get payment methods | PaymentService | `GetPaymentMethodsAsync()` |
| Add payment method | PaymentService | `AddPaymentMethodAsync()` |
| Process payment | PaymentService | `ProcessPaymentAsync()` |
| Create subscription | SubscriptionLifecycleService | `CreateSubscriptionAsync()` |
| Cancel subscription | SubscriptionLifecycleService | `CancelSubscriptionAsync()` |
| Check privilege | PrivilegeService | `CheckPrivilegeAvailabilityAsync()` |
| Purchase credits | SubscriptionService | `PurchaseAdditionalCreditsAsync()` |
| Ensure Stripe customer | StripeService | `EnsureStripeCustomerAsync()` |

---

**End of Service Boundaries Guide**


