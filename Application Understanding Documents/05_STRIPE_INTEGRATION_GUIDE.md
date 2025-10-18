# 📘 Stripe Integration - Developer Guide

## Table of Contents
1. [Overview](#overview)
2. [Stripe Resources](#stripe-resources)
3. [Service Architecture](#service-architecture)
4. [Synchronization Strategy](#synchronization-strategy)
5. [Webhook Handling](#webhook-handling)
6. [Complete Workflows](#complete-workflows)
7. [Error Handling](#error-handling)
8. [Code Examples](#code-examples)

---

## 1. Overview

### What is Stripe Integration?

Stripe is our payment processing provider. The integration handles all financial transactions, recurring billing, and payment method management through Stripe's API while maintaining a synchronized copy of data in our local database.

### Key Responsibilities

- ✅ **Payment Processing**: Charge customers through Stripe
- ✅ **Subscription Management**: Create and manage recurring subscriptions in Stripe
- ✅ **Product/Price Management**: Sync plans as products and prices in Stripe
- ✅ **Customer Management**: Create and update customer records in Stripe
- ✅ **Webhook Processing**: Handle real-time events from Stripe
- ✅ **Data Synchronization**: Keep local DB in sync with Stripe

---

## 2. Stripe Resources

### 2.1 Stripe Resource Mapping

| Our Entity | Stripe Resource | Link Field | Example |
|------------|-----------------|------------|---------|
| **User** | Customer | User.StripeCustomerId | "cus_XYZ789" |
| **SubscriptionPlan** | Product | Plan.StripeProductId | "prod_ABC123" |
| **SubscriptionPlan** | Price (3x) | Plan.StripeMonthlyPriceId | "price_1Month_XYZ" |
| **Subscription** | Subscription | Subscription.StripeSubscriptionId | "sub_stripe_AAA" |
| **BillingRecord** | Invoice | BillingRecord.StripeInvoiceId | "in_stripe_BBB" |
| **BillingRecord** | PaymentIntent | BillingRecord.StripePaymentIntentId | "pi_DEF456" |

### 2.2 Stripe Data Structures

#### **Customer**
```json
{
  "id": "cus_XYZ789",
  "object": "customer",
  "email": "johndoe@example.com",
  "name": "John Doe",
  "description": "SmartTelehealth User",
  "metadata": {
    "userId": "456",
    "platform": "SmartTelehealth"
  },
  "created": 1697544000
}
```

#### **Product**
```json
{
  "id": "prod_ABC123",
  "object": "product",
  "name": "Basic Health Plan",
  "description": "Essential healthcare services for individuals",
  "active": true,
  "metadata": {
    "planId": "f3a1b2c3-...",
    "source": "SmartTelehealth"
  }
}
```

#### **Price**
```json
{
  "id": "price_1Month_XYZ",
  "object": "price",
  "product": "prod_ABC123",
  "unit_amount": 27500,  // $275.00 in cents
  "currency": "usd",
  "recurring": {
    "interval": "month",
    "interval_count": 1
  },
  "active": true
}
```

#### **Subscription**
```json
{
  "id": "sub_stripe_AAA",
  "object": "subscription",
  "customer": "cus_XYZ789",
  "status": "active",
  "items": {
    "data": [
      {
        "id": "si_...",
        "price": "price_1Month_XYZ"
      }
    ]
  },
  "current_period_start": 1697544000,
  "current_period_end": 1700136000,
  "metadata": {
    "subscriptionId": "sub_111",
    "planId": "f3a1b2c3-..."
  }
}
```

#### **Invoice**
```json
{
  "id": "in_stripe_BBB",
  "object": "invoice",
  "customer": "cus_XYZ789",
  "subscription": "sub_stripe_AAA",
  "amount_due": 27500,
  "amount_paid": 27500,
  "status": "paid",
  "number": "12345678",
  "metadata": {
    "billingRecordId": "bill_001",
    "subscriptionId": "sub_111"
  }
}
```

---

## 3. Service Architecture

### 3.1 Primary Services

#### **StripeService** (Core Stripe Operations)
**Location:** `SmartTelehealth.Infrastructure/Services/StripeService.cs`

**Responsibilities:**
- All direct Stripe API calls
- Customer management (create, update, retrieve)
- Product/Price management
- Subscription management
- Payment processing
- Invoice handling

**Key Dependencies:**
```csharp
IConfiguration _configuration  // For Stripe API keys
ILogger<StripeService> _logger
IUserRepository _userRepository  // For EnsureStripeCustomerAsync
```

**Key Methods:**
```csharp
// Customer Management
Task<string> CreateCustomerAsync(string email, string name, TokenModel tokenModel)
Task<string> EnsureStripeCustomerAsync(int userId, TokenModel tokenModel)
Task UpdateCustomerAsync(string customerId, string email, string name, TokenModel tokenModel)

// Product/Price Management
Task<string> CreateProductAsync(string name, string description, TokenModel tokenModel)
Task<string> CreatePriceAsync(string productId, decimal amount, string currency, string interval, int intervalCount, TokenModel tokenModel)
Task UpdateProductAsync(string productId, string name, string description, TokenModel tokenModel)
Task DeactivatePriceAsync(string priceId, TokenModel tokenModel)

// Subscription Management
Task<string> CreateSubscriptionAsync(string customerId, string priceId, Dictionary<string, string> metadata, DateTime? trialEnd, TokenModel tokenModel)
Task CancelSubscriptionAsync(string subscriptionId, TokenModel tokenModel)
Task<Stripe.Subscription> GetSubscriptionAsync(string subscriptionId)

// Payment Processing
Task<Stripe.PaymentIntent> CreatePaymentIntentAsync(long amount, string customerId, string description, Dictionary<string, string> metadata, TokenModel tokenModel)
Task<Stripe.Refund> CreateRefundAsync(string paymentIntentId, long? amount, TokenModel tokenModel)

// Invoice Management
Task<Stripe.Invoice> GetInvoiceAsync(string invoiceId)
Task<Stripe.Invoice> FinalizeInvoiceAsync(string invoiceId)
```

#### **StripeSynchronizationService**
**Location:** `SmartTelehealth.Application/Services/StripeSynchronizationService.cs`

**Responsibilities:**
- Sync plans to Stripe products
- Handle plan deletions
- Sync subscription statuses
- Resolve discrepancies

#### **WebhookIdempotencyService**
**Location:** `SmartTelehealth.Application/Services/WebhookIdempotencyService.cs`

**Responsibilities:**
- Track processed webhook events
- Prevent duplicate processing
- Ensure idempotency

---

## 4. Synchronization Strategy

### 4.1 Bidirectional Sync

```
┌──────────────────────┐              ┌──────────────────────┐
│   YOUR DATABASE      │              │      STRIPE          │
│                      │              │                      │
│  - Subscriptions     │              │  - Customers         │
│  - Plans             │              │  - Products          │
│  - Billing Records   │              │  - Prices            │
│  - Payments          │              │  - Subscriptions     │
│  - Users             │              │  - Invoices          │
└──────────┬───────────┘              └──────────┬───────────┘
           │                                     │
           │ ←─────── PUSH (API Calls) ─────────│
           │                                     │
           │ ─────────→ PULL (Webhooks) ────────│
           │                                     │
```

### 4.2 PUSH Synchronization (Your System → Stripe)

**When:** Admin creates/updates resources

**Examples:**

```
SCENARIO 1: Admin Creates Plan
┌──────────────────────────────────┐
│ YOUR DATABASE                     │
│ SubscriptionPlan created          │
│   Name: "Basic Health"            │
│   Price: $275                     │
└────────────┬─────────────────────┘
             │
             │ API Call: POST /v1/products
             ↓
┌──────────────────────────────────┐
│ STRIPE                            │
│ Product created                   │
│   ID: "prod_ABC123"               │
│   Name: "Basic Health"            │
└────────────┬─────────────────────┘
             │
             │ Returns: prod_ABC123
             ↓
┌──────────────────────────────────┐
│ YOUR DATABASE                     │
│ SubscriptionPlan updated          │
│   StripeProductId: "prod_ABC123"  │
└──────────────────────────────────┘
```

```
SCENARIO 2: User Subscribes
┌──────────────────────────────────┐
│ YOUR DATABASE                     │
│ Subscription created              │
│   Status: Pending                 │
│   UserId: 456                     │
└────────────┬─────────────────────┘
             │
             │ API Call: POST /v1/subscriptions
             ↓
┌──────────────────────────────────┐
│ STRIPE                            │
│ Subscription created              │
│   ID: "sub_stripe_AAA"            │
│   Customer: "cus_XYZ789"          │
│   Status: active                  │
└────────────┬─────────────────────┘
             │
             │ Returns: sub_stripe_AAA
             ↓
┌──────────────────────────────────┐
│ YOUR DATABASE                     │
│ Subscription updated              │
│   StripeSubscriptionId:           │
│     "sub_stripe_AAA"              │
└──────────────────────────────────┘
```

### 4.3 PULL Synchronization (Stripe → Your System)

**When:** Stripe events occur (payment, renewal, etc.)

**Flow:**

```
┌──────────────────────────────────┐
│ STRIPE                            │
│ Event occurs:                     │
│   - Payment successful            │
│   - Payment failed                │
│   - Subscription renewed          │
│   - etc.                          │
└────────────┬─────────────────────┘
             │
             │ HTTP POST (Webhook)
             ↓
┌──────────────────────────────────┐
│ YOUR SYSTEM                       │
│ Endpoint: /api/webhooks/stripe    │
│                                   │
│ 1. Validate signature ✅          │
│ 2. Check idempotency ✅           │
│ 3. Process event ✅               │
│ 4. Update database ✅             │
│ 5. Return 200 OK ✅               │
└──────────────────────────────────┘
```

---

## 5. Webhook Handling

### 5.1 Webhook Event Types (51 Handled)

**Customer Events:**
- `customer.created`
- `customer.updated`
- `customer.deleted`

**Subscription Events:**
- `customer.subscription.created`
- `customer.subscription.updated`
- `customer.subscription.deleted`
- `customer.subscription.paused`
- `customer.subscription.resumed`
- `customer.subscription.past_due`
- `customer.subscription.unpaid`

**Invoice Events (Most Critical):**
- `invoice.created`
- `invoice.finalized`
- `invoice.payment_succeeded` ⚡ **Most Important**
- `invoice.payment_failed`
- `invoice.payment_action_required`
- `invoice.sent`
- `invoice.upcoming`

**Payment Events:**
- `payment_intent.succeeded`
- `payment_intent.payment_failed`
- `payment_intent.canceled`

### 5.2 Webhook Processing Flow

```
┌─────────────────────────────────────────────────┐
│ STRIPE sends webhook POST                        │
│ Event ID: evt_ABC123                             │
│ Type: invoice.payment_succeeded                  │
└─────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────┐
│ YOUR SYSTEM: StripeWebhookController             │
│ Endpoint: POST /api/webhooks/stripe              │
│                                                  │
│ [STEP 1] SECURITY - Validate Signature          │
│   ┌────────────────────────────────┐            │
│   │ Get signature from headers     │            │
│   │ Get webhook secret from config │            │
│   │ Verify using Stripe SDK:       │            │
│   │   EventUtility.ConstructEvent( │            │
│   │     json, signature, secret    │            │
│   │   )                            │            │
│   └────────────────────────────────┘            │
│   If invalid → Return 400 Bad Request           │
│   If valid → Continue ✅                         │
│                                                  │
│ [STEP 2] IDEMPOTENCY - Prevent Duplicates       │
│   ┌────────────────────────────────┐            │
│   │ Query WebhookEvents table:     │            │
│   │ WHERE EventId = 'evt_ABC123'   │            │
│   │                                │            │
│   │ If found:                      │            │
│   │   Already processed            │            │
│   │   Return 200 OK (skip)         │            │
│   │                                │            │
│   │ If not found:                  │            │
│   │   New event, proceed           │            │
│   └────────────────────────────────┘            │
│                                                  │
│ [STEP 3] PROCESS - Handle Event Type            │
│   switch (event.Type) {                         │
│     case "invoice.payment_succeeded":           │
│       await HandlePaymentSucceeded(event);      │
│       break;                                    │
│     case "invoice.payment_failed":              │
│       await HandlePaymentFailed(event);         │
│       break;                                    │
│     // ... 51 event types total                 │
│   }                                             │
│                                                  │
│ [STEP 4] MARK AS PROCESSED                      │
│   await _webhookIdempotencyService              │
│     .MarkAsProcessedAsync(                      │
│       eventId: "evt_ABC123",                    │
│       processingTime: 245ms                     │
│     );                                          │
│                                                  │
│ [STEP 5] RETURN 200 OK                          │
│   Stripe receives confirmation                  │
│   Won't retry this event                        │
└─────────────────────────────────────────────────┘
```

### 5.3 Key Webhook Handlers

#### **invoice.payment_succeeded**

**Most Critical**: This webhook activates subscriptions and processes renewals

```csharp
private async Task HandlePaymentSucceeded(Event stripeEvent)
{
    var invoice = stripeEvent.Data.Object as Stripe.Invoice;
    
    // Extract metadata
    var subscriptionId = GetSubscriptionIdFromInvoice(invoice);
    
    // Find local subscription
    var subscription = await _subscriptionRepository
        .GetByStripeSubscriptionIdAsync(subscriptionId);
    
    // Begin transaction
    await _unitOfWork.BeginTransactionAsync();
    
    try
    {
        // Update subscription status
        subscription.Status = Subscription.SubscriptionStatuses.Active;
        subscription.LastPaymentDate = DateTime.UtcNow;
        subscription.FailedPaymentAttempts = 0;
        
        await _subscriptionRepository.UpdateAsync(subscription);
        
        // Create/update billing record
        var billingRecord = await _billingRepository
            .GetBySubscriptionIdAsync(subscription.Id);
        
        if (billingRecord != null)
        {
            billingRecord.Status = BillingRecord.BillingStatus.Paid;
            billingRecord.PaidDate = DateTime.UtcNow;
            billingRecord.StripeInvoiceId = invoice.Id;
            billingRecord.StripePaymentIntentId = invoice.PaymentIntentId;
            
            await _billingRepository.UpdateAsync(billingRecord);
        }
        
        // Create payment record
        var payment = new SubscriptionPayment
        {
            SubscriptionId = subscription.Id,
            BillingRecordId = billingRecord?.Id,
            Amount = invoice.AmountPaid / 100m,  // Convert from cents
            PaymentMethod = "Stripe",
            Status = "Success",
            TransactionId = invoice.PaymentIntentId,
            PaymentDate = DateTime.UtcNow
        };
        
        await _paymentRepository.CreateAsync(payment);
        
        // Record status change
        await _statusHistoryRepository.CreateAsync(
            new SubscriptionStatusHistory
            {
                SubscriptionId = subscription.Id,
                FromStatus = "Pending",
                ToStatus = "Active",
                Reason = "Payment successful",
                ChangedAt = DateTime.UtcNow
            }
        );
        
        // If this is a renewal, reset privileges
        if (IsRenewal(invoice))
        {
            await ResetPrivilegeUsageAsync(subscription.Id);
        }
        
        // Commit transaction
        await _unitOfWork.CommitTransactionAsync();
        
        // Send notification
        await _notificationService.SendPaymentSuccessNotificationAsync(
            subscription.User.Email,
            subscription.User.FullName,
            _mapper.Map<BillingRecordDto>(billingRecord),
            tokenModel: null
        );
        
        _logger.LogInformation(
            "Successfully processed payment for subscription {SubId}",
            subscription.Id
        );
    }
    catch (Exception ex)
    {
        await _unitOfWork.RollbackTransactionAsync();
        _logger.LogError(ex, "Failed to process payment success webhook");
        throw;  // Stripe will retry
    }
}
```

#### **invoice.payment_failed**

**Critical**: Handles payment failures and triggers retries

```csharp
private async Task HandlePaymentFailed(Event stripeEvent)
{
    var invoice = stripeEvent.Data.Object as Stripe.Invoice;
    
    var subscriptionId = GetSubscriptionIdFromInvoice(invoice);
    var subscription = await _subscriptionRepository
        .GetByStripeSubscriptionIdAsync(subscriptionId);
    
    // Update subscription
    subscription.Status = Subscription.SubscriptionStatuses.PaymentFailed;
    subscription.FailedPaymentAttempts++;
    subscription.LastPaymentFailedDate = DateTime.UtcNow;
    subscription.LastPaymentError = invoice.LastFinalizationError?.Message 
        ?? "Payment failed";
    
    await _subscriptionRepository.UpdateAsync(subscription);
    
    // Update billing record
    var billingRecord = await _billingRepository
        .GetBySubscriptionIdAsync(subscription.Id);
    
    if (billingRecord != null)
    {
        billingRecord.Status = BillingRecord.BillingStatus.Failed;
        billingRecord.FailedReason = subscription.LastPaymentError;
        
        await _billingRepository.UpdateAsync(billingRecord);
    }
    
    // Send urgent notification
    await _notificationService.SendPaymentFailureNotificationAsync(
        subscription.User.Email,
        subscription.User.FullName,
        subscription.LastPaymentError,
        tokenModel: null
    );
    
    _logger.LogWarning(
        "Payment failed for subscription {SubId}. Attempts: {Attempts}",
        subscription.Id, subscription.FailedPaymentAttempts
    );
}
```

---

## 6. Complete Workflows

### 6.1 Workflow: Creating Stripe Customer

```
┌─────────────────────────────────────────────────┐
│ TRIGGER: User subscribes for first time         │
└─────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────┐
│ StripeService.EnsureStripeCustomerAsync()       │
│                                                  │
│ [1] Check if user already has Stripe ID         │
│     user = await _userRepository                │
│       .GetByIdAsync(userId);                    │
│                                                  │
│     if (!string.IsNullOrEmpty(                  │
│          user.StripeCustomerId)) {              │
│       // Already exists                         │
│       return user.StripeCustomerId;             │
│     }                                           │
│                                                  │
│ [2] Create Customer in Stripe                   │
│     var customerOptions = new CustomerCreateOptions│
│     {                                           │
│       Email = user.Email,                       │
│       Name = user.FullName,                     │
│       Description = "SmartTelehealth User",     │
│       Metadata = new Dictionary<string, string> │
│       {                                         │
│         { "userId", userId.ToString() },        │
│         { "platform", "SmartTelehealth" }       │
│       }                                         │
│     };                                          │
│                                                  │
│     var customerService = new CustomerService();│
│     var customer = await customerService        │
│       .CreateAsync(customerOptions);            │
│                                                  │
│     // Returns: cus_XYZ789                      │
│                                                  │
│ [3] Update User in YOUR Database                │
│     user.StripeCustomerId = customer.Id;        │
│     await _userRepository.UpdateAsync(user);    │
│                                                  │
│ [4] Return Customer ID                          │
│     return customer.Id;  // "cus_XYZ789"        │
└─────────────────────────────────────────────────┘
```

### 6.2 Workflow: Processing Payment via Stripe

```
┌─────────────────────────────────────────────────┐
│ TRIGGER: Billing record needs payment           │
└─────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────┐
│ PaymentService.ProcessPaymentAsync()            │
│                                                  │
│ [1] Get Billing Record                          │
│     billingRecord = await _billingRepository    │
│       .GetByIdAsync(billingRecordId);           │
│                                                  │
│ [2] Get Stripe Customer ID                      │
│     subscription = await _subscriptionRepository│
│       .GetByIdAsync(                            │
│         billingRecord.SubscriptionId            │
│       );                                        │
│                                                  │
│     stripeCustomerId =                          │
│       subscription.StripeCustomerId;            │
│       // "cus_XYZ789"                           │
│                                                  │
│ [3] Create Payment Intent in Stripe             │
│     var paymentIntentOptions = new              │
│       PaymentIntentCreateOptions                │
│     {                                           │
│       Amount = (long)(billingRecord.TotalAmount │
│         * 100),  // $275 → 27500 cents          │
│       Currency = "usd",                         │
│       Customer = stripeCustomerId,              │
│       Description = billingRecord.Description,  │
│       AutomaticPaymentMethods = new             │
│         PaymentIntentAutomaticPaymentMethodsOptions│
│       {                                         │
│         Enabled = true  // Use customer's       │
│                         // default payment method│
│       },                                        │
│       Metadata = new Dictionary<string, string> │
│       {                                         │
│         { "billingRecordId",                    │
│            billingRecord.Id.ToString() },       │
│         { "subscriptionId",                     │
│            subscription.Id.ToString() }         │
│       }                                         │
│     };                                          │
│                                                  │
│     var paymentIntentService =                  │
│       new PaymentIntentService();               │
│                                                  │
│     var paymentIntent = await paymentIntentService│
│       .CreateAsync(paymentIntentOptions);       │
│                                                  │
│     // Stripe auto-charges the customer's       │
│     // default payment method                   │
│     // Returns: pi_DEF456 (with status: succeeded)│
│                                                  │
│ [4] Update Billing Record                       │
│     billingRecord.Status =                      │
│       BillingRecord.BillingStatus.Paid;         │
│     billingRecord.PaidDate = DateTime.UtcNow;   │
│     billingRecord.StripePaymentIntentId =       │
│       paymentIntent.Id;                         │
│                                                  │
│     await _billingRepository.UpdateAsync(       │
│       billingRecord                             │
│     );                                          │
│                                                  │
│ [5] Return Success                              │
│     return new JsonModel {                      │
│       StatusCode = 200,                         │
│       Message = "Payment successful",           │
│       data = paymentDto                         │
│     };                                          │
└─────────────────────────────────────────────────┘
```

---

## 7. Error Handling

### 7.1 Retry Logic

**For API Calls:**
```csharp
private async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation)
{
    int maxRetries = 3;
    TimeSpan retryDelay = TimeSpan.FromSeconds(2);
    
    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            return await operation();
        }
        catch (Exception ex) when (attempt < maxRetries && IsRetryableException(ex))
        {
            _logger.LogWarning(
                ex,
                "Attempt {Attempt} failed, retrying in {Delay}ms",
                attempt,
                retryDelay.TotalMilliseconds
            );
            
            await Task.Delay(retryDelay);
        }
    }
    
    throw new InvalidOperationException(
        $"Operation failed after {maxRetries} attempts"
    );
}

private bool IsRetryableException(Exception ex)
{
    return ex is StripeException stripeEx && 
           (stripeEx.StripeError?.Type == "rate_limit_error" || 
            stripeEx.StripeError?.Type == "api_connection_error");
}
```

**For Webhooks:**
- Stripe automatically retries failed webhooks
- Up to 3 days of retries
- Exponential backoff
- Your system must return 200 OK to stop retries

### 7.2 Cleanup on Failure

**If subscription creation fails, clean up Stripe resources:**

```csharp
try
{
    // Create DB subscription
    var subscription = await _subscriptionRepository.CreateAsync(...);
    
    // Create Stripe subscription
    var stripeSubId = await _stripeService.CreateSubscriptionAsync(...);
    
    subscription.StripeSubscriptionId = stripeSubId;
    await _subscriptionRepository.UpdateAsync(subscription);
    
    await _unitOfWork.CommitTransactionAsync();
}
catch (Exception ex)
{
    await _unitOfWork.RollbackTransactionAsync();
    
    // CLEANUP: Cancel Stripe subscription if it was created
    if (!string.IsNullOrEmpty(stripeSubId))
    {
        try
        {
            await _stripeService.CancelSubscriptionAsync(stripeSubId, tokenModel);
            _logger.LogInformation("Cleaned up Stripe subscription {Id}", stripeSubId);
        }
        catch (Exception cleanupEx)
        {
            _logger.LogError(cleanupEx, "Failed to cleanup Stripe resources");
            // Manual cleanup may be required
        }
    }
    
    throw;
}
```

---

## 8. Code Examples

### 8.1 Complete Stripe Customer Creation

```csharp
public async Task<string> EnsureStripeCustomerAsync(
    int userId,
    TokenModel tokenModel)
{
    // 1. Check if user already has Stripe customer
    var user = await _userRepository.GetByIdAsync(userId);
    
    if (!string.IsNullOrEmpty(user.StripeCustomerId))
    {
        _logger.LogInformation(
            "User {UserId} already has Stripe customer {CustomerId}",
            userId, user.StripeCustomerId
        );
        return user.StripeCustomerId;
    }
    
    // 2. Create customer in Stripe
    try
    {
        var customerOptions = new CustomerCreateOptions
        {
            Email = user.Email,
            Name = $"{user.FirstName} {user.LastName}",
            Description = "SmartTelehealth Patient",
            Metadata = new Dictionary<string, string>
            {
                { "userId", userId.ToString() },
                { "platform", "SmartTelehealth" },
                { "createdBy", tokenModel?.UserID.ToString() ?? "System" }
            }
        };
        
        var customerService = new CustomerService();
        var customer = await customerService.CreateAsync(customerOptions);
        
        _logger.LogInformation(
            "Created Stripe customer {CustomerId} for user {UserId}",
            customer.Id, userId
        );
        
        // 3. Update user with Stripe customer ID
        user.StripeCustomerId = customer.Id;
        await _userRepository.UpdateAsync(user);
        
        return customer.Id;
    }
    catch (StripeException ex)
    {
        _logger.LogError(ex, 
            "Failed to create Stripe customer for user {UserId}", userId
        );
        throw new InvalidOperationException(
            $"Failed to create Stripe customer: {ex.Message}", ex
        );
    }
}
```

### 8.2 Creating Stripe Subscription

```csharp
public async Task<string> CreateSubscriptionAsync(
    string customerId,
    string priceId,
    Dictionary<string, string> metadata,
    DateTime? trialEnd,
    TokenModel tokenModel)
{
    try
    {
        var subscriptionOptions = new SubscriptionCreateOptions
        {
            Customer = customerId,
            Items = new List<SubscriptionItemOptions>
            {
                new SubscriptionItemOptions
                {
                    Price = priceId
                }
            },
            Metadata = metadata,
            PaymentBehavior = "default_incomplete",  // Requires payment method
            PaymentSettings = new SubscriptionPaymentSettingsOptions
            {
                SaveDefaultPaymentMethod = "on_subscription"
            },
            TrialEnd = trialEnd,  // null if no trial
            Expand = new List<string> { "latest_invoice.payment_intent" }
        };
        
        var subscriptionService = new SubscriptionService();
        var subscription = await subscriptionService.CreateAsync(subscriptionOptions);
        
        _logger.LogInformation(
            "Created Stripe subscription {SubId} for customer {CustId}",
            subscription.Id, customerId
        );
        
        return subscription.Id;
    }
    catch (StripeException ex)
    {
        _logger.LogError(ex, 
            "Failed to create Stripe subscription for customer {CustomerId}",
            customerId
        );
        throw new InvalidOperationException(
            $"Failed to create subscription in Stripe: {ex.Message}", ex
        );
    }
}
```

---

## Key Takeaways

### ✅ Critical Concepts

1. **Resource Mapping**: Each of our entities maps to a Stripe resource
2. **Bidirectional Sync**: PUSH (API calls) + PULL (webhooks)
3. **Webhook Security**: Always validate signatures
4. **Idempotency**: Prevent duplicate processing of webhooks
5. **Error Handling**: Retry API calls, clean up on failures
6. **Metadata**: Use to link Stripe resources to local entities

### 🔍 Common Patterns

| Operation | Direction | Method | When |
|-----------|-----------|--------|------|
| Create customer | Push | API Call | First subscription |
| Create product | Push | API Call | Plan creation |
| Create subscription | Push | API Call | User subscribes |
| Payment success | Pull | Webhook | Payment succeeds |
| Payment failure | Pull | Webhook | Payment fails |
| Renewal | Pull | Webhook | Billing date |

---

**Document Version:** 1.0  
**Last Updated:** October 17, 2025

