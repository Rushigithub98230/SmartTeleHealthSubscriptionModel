# Payment Processing & Stripe Integration

## Table of Contents
1. [Payment Architecture](#payment-architecture)
2. [Payment Processing Flow](#payment-processing-flow)
3. [Stripe Integration](#stripe-integration)
4. [Webhook Processing](#webhook-processing)
5. [Refund Management](#refund-management)
6. [Payment Security](#payment-security)

---

## Payment Architecture

### Key Components

```
┌──────────────────────────┐
│   PaymentService         │  ← Payment orchestration
└────────┬────────────────-┘
         │
         ▼
┌──────────────────────────┐
│ StripeBillingService     │  ← Stripe-specific logic
└────────┬─────────────────┘
         │
         ▼
┌──────────────────────────┐
│ StripeService            │  ← Stripe API calls
└──────────────────────────┘
```

### Payment Entities

**SubscriptionPayment**:
```csharp
public class SubscriptionPayment : BaseEntity
{
    public Guid Id { get; set; }
    public Guid SubscriptionId { get; set; }
    public Guid BillingRecordId { get; set; }
    public Guid CurrencyId { get; set; }
    
    // Amounts
    public decimal Amount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal NetAmount { get; set; }
    
    // Status & Type
    public PaymentStatus Status { get; set; }  // Pending, Succeeded, Failed, Refunded
    public PaymentType Type { get; set; }      // Subscription, Trial, Upgrade, etc.
    
    // Dates
    public DateTime DueDate { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? FailedAt { get; set; }
    public DateTime BillingPeriodStart { get; set; }
    public DateTime BillingPeriodEnd { get; set; }
    
    // Stripe Integration
    public string? StripePaymentIntentId { get; set; }
    public string? StripeInvoiceId { get; set; }
    public string? ReceiptUrl { get; set; }
    
    // Retry Logic
    public int AttemptCount { get; set; }
    public DateTime? NextRetryAt { get; set; }
    
    // Refunds
    public decimal RefundedAmount { get; set; }
    public virtual ICollection<PaymentRefund> Refunds { get; set; }
}
```

---

## Payment Processing Flow

### Manual Payment Processing

**Endpoint**: `POST /api/billing/{billingRecordId}/process-payment`

**Service**: `PaymentService.ProcessPaymentAsync()`

#### Flow Diagram

```
User/Admin Trigger
    │
    ▼
┌─────────────────────────┐
│ Validate Billing Record │
│ - Exists?               │
│ - Already paid?         │
└────────┬────────────────┘
         │
         ▼
┌─────────────────────────┐
│ Get/Create              │
│ SubscriptionPayment     │
│ - Link to billing       │
│ - Set initial status    │
└────────┬────────────────┘
         │
         ▼
┌─────────────────────────┐
│ Process Stripe Payment  │
│ - Get payment methods   │
│ - Validate method       │
│ - Create payment intent │
│ - Confirm payment       │
└────────┬────────────────┘
         │
         ▼
     ┌───┴───┐
     │Success│
     └───┬───┘
         │
         ▼
┌─────────────────────────┐
│ Update Records          │
│ - Payment: Succeeded    │
│ - Billing: Paid         │
│ - Subscription: Update  │
│   dates & status        │
└────────┬────────────────┘
         │
         ▼
┌─────────────────────────┐
│ Reset Privileges        │
│ - New billing period    │
│ - Reset usage counters  │
└────────┬────────────────┘
         │
         ▼
┌─────────────────────────┐
│ Send Notifications      │
│ - Payment confirmation  │
│ - Invoice email         │
└─────────────────────────┘
```

#### Code Implementation

```csharp
public async Task<JsonModel> ProcessPaymentAsync(
    Guid billingRecordId, 
    TokenModel tokenModel)
{
    try {
        // 1. Validate billing record
        var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
        if (billingRecord == null) {
            return NotFound("Billing record not found");
        }
        
        if (billingRecord.Status == BillingRecord.BillingStatus.Paid) {
            return BadRequest("Billing record already paid");
        }
        
        // 2. Get or create SubscriptionPayment
        SubscriptionPayment subscriptionPayment = null;
        
        if (billingRecord.Type == BillingRecord.BillingType.Subscription || 
            billingRecord.Type == BillingRecord.BillingType.Overage ||
            billingRecord.Type == BillingRecord.BillingType.Recurring) {
            
            subscriptionPayment = await GetOrCreateSubscriptionPaymentAsync(
                billingRecord, tokenModel);
        }
        
        // 3. Process payment through Stripe
        var stripeResult = await _stripeBillingService.ProcessStripePaymentAsync(
            billingRecordId, tokenModel);
        
        // 4. Update records based on result
        await UpdatePaymentRecordsAsync(
            billingRecord, subscriptionPayment, stripeResult, tokenModel);
        
        if (stripeResult.StatusCode == 200) {
            _logger.LogInformation(
                "Payment processed successfully for billing record {BillingRecordId}", 
                billingRecordId);
        }
        
        return stripeResult;
    }
    catch (Exception ex) {
        _logger.LogError(ex, 
            "Error processing payment for billing record {BillingRecordId}", 
            billingRecordId);
        return Error("Error processing payment");
    }
}
```

### Get or Create SubscriptionPayment

```csharp
private async Task<SubscriptionPayment> GetOrCreateSubscriptionPaymentAsync(
    BillingRecord billingRecord, 
    TokenModel tokenModel)
{
    // Check if payment already exists
    var existingPayment = await _subscriptionPaymentRepository
        .GetByBillingRecordIdAsync(billingRecord.Id);
    
    if (existingPayment != null) {
        return existingPayment;
    }
    
    // Create new payment record
    var subscription = await _subscriptionRepository
        .GetByIdAsync(billingRecord.SubscriptionId.Value);
    
    var payment = new SubscriptionPayment {
        Id = Guid.NewGuid(),
        SubscriptionId = billingRecord.SubscriptionId.Value,
        BillingRecordId = billingRecord.Id,
        CurrencyId = billingRecord.CurrencyId,
        
        Amount = billingRecord.TotalAmount,
        TaxAmount = billingRecord.TaxAmount,
        NetAmount = billingRecord.TotalAmount,
        
        Status = SubscriptionPayment.PaymentStatus.Pending,
        Type = MapBillingTypeToPaymentType(billingRecord.Type),
        
        DueDate = billingRecord.DueDate ?? DateTime.UtcNow.AddDays(7),
        BillingPeriodStart = subscription.LastBillingDate ?? subscription.StartDate,
        BillingPeriodEnd = subscription.NextBillingDate,
        
        Description = billingRecord.Description,
        
        CreatedBy = tokenModel.UserID,
        CreatedDate = DateTime.UtcNow
    };
    
    await _subscriptionPaymentRepository.AddAsync(payment);
    await _subscriptionPaymentRepository.SaveChangesAsync();
    
    return payment;
}
```

### Update Payment Records

```csharp
private async Task UpdatePaymentRecordsAsync(
    BillingRecord billingRecord,
    SubscriptionPayment? subscriptionPayment,
    JsonModel stripeResult,
    TokenModel tokenModel)
{
    await _unitOfWork.BeginTransactionAsync();
    try {
        if (stripeResult.StatusCode == 200) {
            // SUCCESS CASE
            var paymentData = stripeResult.data as dynamic;
            
            // Update billing record
            billingRecord.Status = BillingRecord.BillingStatus.Paid;
            billingRecord.PaidAt = DateTime.UtcNow;
            billingRecord.StripePaymentIntentId = paymentData?.PaymentIntentId;
            billingRecord.ProcessedAt = DateTime.UtcNow;
            
            await _billingRepository.UpdateAsync(billingRecord);
            
            // Update subscription payment
            if (subscriptionPayment != null) {
                subscriptionPayment.Status = SubscriptionPayment.PaymentStatus.Succeeded;
                subscriptionPayment.PaidAt = DateTime.UtcNow;
                subscriptionPayment.StripePaymentIntentId = paymentData?.PaymentIntentId;
                subscriptionPayment.ReceiptUrl = paymentData?.ReceiptUrl;
                
                await _subscriptionPaymentRepository.UpdateAsync(subscriptionPayment);
                
                // Update subscription billing dates
                await UpdateSubscriptionBillingDatesAsync(
                    subscriptionPayment.SubscriptionId, tokenModel);
                
                // Reset privileges for new billing period
                await ResetPrivilegesAsync(
                    subscriptionPayment.SubscriptionId, tokenModel);
            }
        }
        else {
            // FAILURE CASE
            billingRecord.Status = BillingRecord.BillingStatus.Failed;
            billingRecord.FailureReason = stripeResult.Message;
            await _billingRepository.UpdateAsync(billingRecord);
            
            if (subscriptionPayment != null) {
                subscriptionPayment.Status = SubscriptionPayment.PaymentStatus.Failed;
                subscriptionPayment.FailedAt = DateTime.UtcNow;
                subscriptionPayment.FailureReason = stripeResult.Message;
                subscriptionPayment.AttemptCount++;
                
                // Schedule retry
                subscriptionPayment.NextRetryAt = DateTime.UtcNow.AddHours(24);
                
                await _subscriptionPaymentRepository.UpdateAsync(subscriptionPayment);
            }
        }
        
        await _unitOfWork.CommitTransactionAsync();
    }
    catch {
        await _unitOfWork.RollbackTransactionAsync();
        throw;
    }
}
```

---

## Stripe Integration

### Stripe Service Architecture

**StripeService** (Infrastructure layer):
- Manages Stripe API calls
- Customer management
- Payment method handling
- Subscription lifecycle in Stripe
- Payment intents and charges

### Customer Management

#### Ensure Stripe Customer Exists

```csharp
public async Task<string> EnsureStripeCustomerAsync(UserDto user, TokenModel tokenModel)
{
    // Check if user already has Stripe customer ID
    if (!string.IsNullOrEmpty(user.StripeCustomerId)) {
        try {
            // Verify customer still exists in Stripe
            var customer = await _stripeService.GetCustomerAsync(
                user.StripeCustomerId, tokenModel);
            return user.StripeCustomerId;
        }
        catch {
            // Customer doesn't exist, create new one
        }
    }
    
    // Create new Stripe customer
    var stripeCustomerId = await _stripeService.CreateCustomerAsync(
        user.Email,
        $"{user.FirstName} {user.LastName}",
        tokenModel
    );
    
    // Update user record
    user.StripeCustomerId = stripeCustomerId;
    await _userService.UpdateUserAsync(user, tokenModel);
    
    return stripeCustomerId;
}
```

#### Create Stripe Customer

```csharp
public async Task<string> CreateCustomerAsync(
    string email, 
    string name, 
    TokenModel tokenModel)
{
    return await ExecuteWithRetryAsync(async () => {
        var customerCreateOptions = new CustomerCreateOptions {
            Email = email,
            Name = name,
            Metadata = new Dictionary<string, string> {
                { "created_at", DateTime.UtcNow.ToString("O") },
                { "source", "smart_telehealth" },
                { "user_id", tokenModel.UserID.ToString() }
            }
        };
        
        var customerService = new CustomerService();
        var customer = await customerService.CreateAsync(customerCreateOptions);
        
        _logger.LogInformation(
            "Created Stripe customer {CustomerId} for email {Email}", 
            customer.Id, email);
        
        return customer.Id;
    });
}
```

### Payment Method Management

#### Attach Payment Method

```csharp
public async Task<bool> AttachPaymentMethodAsync(
    string paymentMethodId, 
    string customerId, 
    TokenModel tokenModel)
{
    try {
        var paymentMethodService = new PaymentMethodService();
        
        // Attach payment method to customer
        var options = new PaymentMethodAttachOptions {
            Customer = customerId
        };
        
        await paymentMethodService.AttachAsync(paymentMethodId, options);
        
        // Set as default payment method
        var customerService = new CustomerService();
        var customerUpdateOptions = new CustomerUpdateOptions {
            InvoiceSettings = new CustomerInvoiceSettingsOptions {
                DefaultPaymentMethod = paymentMethodId
            }
        };
        
        await customerService.UpdateAsync(customerId, customerUpdateOptions);
        
        _logger.LogInformation(
            "Attached payment method {PaymentMethodId} to customer {CustomerId}",
            paymentMethodId, customerId);
        
        return true;
    }
    catch (StripeException ex) {
        _logger.LogError(ex, 
            "Error attaching payment method {PaymentMethodId} to customer {CustomerId}",
            paymentMethodId, customerId);
        return false;
    }
}
```

### Payment Processing

#### Process Stripe Payment

**Service**: `StripeBillingService.ProcessStripePaymentAsync()`

```csharp
public async Task<JsonModel> ProcessStripePaymentAsync(
    Guid billingRecordId, 
    TokenModel tokenModel)
{
    var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
    if (billingRecord == null) {
        return NotFound("Billing record not found");
    }
    
    // 1. Get customer payment methods
    var paymentMethods = await _stripeService.GetCustomerPaymentMethodsAsync(
        billingRecord.User.StripeCustomerId, tokenModel);
    
    if (!paymentMethods.Any()) {
        return BadRequest("No payment methods found");
    }
    
    // 2. Validate payment method
    var paymentMethod = paymentMethods.First();
    var isValid = await _stripeService.ValidatePaymentMethodAsync(
        paymentMethod.Id, tokenModel);
    
    if (!isValid) {
        return BadRequest("Payment method invalid or expired");
    }
    
    // 3. Process payment
    var paymentResult = await _stripeService.ProcessPaymentAsync(
        paymentMethod.Id,
        billingRecord.TotalAmount,
        billingRecord.Currency.Code,
        tokenModel
    );
    
    if (paymentResult.Success) {
        // 4. Update billing record in transaction
        await _unitOfWork.BeginTransactionAsync();
        try {
            billingRecord.Status = BillingRecord.BillingStatus.Paid;
            billingRecord.PaidAt = DateTime.UtcNow;
            billingRecord.PaymentMethod = paymentMethod.Type;
            billingRecord.StripePaymentIntentId = paymentResult.PaymentIntentId;
            billingRecord.TransactionId = $"txn_{paymentResult.PaymentIntentId}";
            billingRecord.ProcessedAt = DateTime.UtcNow;
            
            await _billingRepository.UpdateAsync(billingRecord);
            await _unitOfWork.CommitTransactionAsync();
            
            return Success(new {
                BillingRecordId = billingRecord.Id,
                PaymentIntentId = paymentResult.PaymentIntentId,
                Amount = billingRecord.TotalAmount,
                Status = "Paid"
            });
        }
        catch {
            await _unitOfWork.RollbackTransactionAsync();
            
            // CRITICAL: Refund Stripe payment if database update fails
            await _stripeService.ProcessRefundAsync(
                paymentResult.PaymentIntentId,
                billingRecord.TotalAmount,
                tokenModel
            );
            
            throw;
        }
    }
    
    return Error("Payment processing failed");
}
```

#### Create Payment Intent

```csharp
public async Task<PaymentResult> ProcessPaymentAsync(
    string paymentMethodId,
    decimal amount,
    string currency,
    TokenModel tokenModel)
{
    try {
        var paymentIntentService = new PaymentIntentService();
        
        var options = new PaymentIntentCreateOptions {
            Amount = (long)(amount * 100), // Convert to cents
            Currency = currency.ToLower(),
            PaymentMethod = paymentMethodId,
            Confirm = true,
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions {
                Enabled = true,
                AllowRedirects = "never"
            },
            Metadata = new Dictionary<string, string> {
                { "user_id", tokenModel.UserID.ToString() },
                { "processed_at", DateTime.UtcNow.ToString("O") }
            }
        };
        
        var paymentIntent = await paymentIntentService.CreateAsync(options);
        
        return new PaymentResult {
            Success = paymentIntent.Status == "succeeded",
            PaymentIntentId = paymentIntent.Id,
            Status = paymentIntent.Status,
            ReceiptUrl = paymentIntent.Charges?.Data?[0]?.ReceiptUrl
        };
    }
    catch (StripeException ex) {
        _logger.LogError(ex, "Stripe payment failed");
        return new PaymentResult {
            Success = false,
            ErrorMessage = ex.Message
        };
    }
}
```

---

## Webhook Processing

### Webhook Controller

**Endpoint**: `POST /api/stripewebhook/webhook`

**Controller**: `StripeWebhookController`

### Webhook Event Types Handled

| Event Type | Handler Method | Purpose |
|------------|---------------|---------|
| `customer.subscription.created` | `HandleSubscriptionCreated` | Sync new subscription |
| `customer.subscription.updated` | `HandleSubscriptionUpdated` | Sync subscription changes |
| `customer.subscription.deleted` | `HandleSubscriptionDeleted` | Handle cancellation |
| `invoice.payment_succeeded` | `HandlePaymentSucceeded` | Record successful payment |
| `invoice.payment_failed` | `HandlePaymentFailed` | Handle payment failure |
| `charge.refunded` | `HandleChargeRefunded` | Process refund |

### Webhook Processing Flow

```csharp
[HttpPost("webhook")]
public async Task<JsonModel> HandleWebhook()
{
    // 1. Read webhook body
    var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
    var webhookSecret = _configuration["StripeSettings:WebhookSecret"];
    
    // 2. Verify signature
    Event stripeEvent;
    try {
        stripeEvent = EventUtility.ConstructEvent(
            json,
            Request.Headers["Stripe-Signature"],
            webhookSecret
        );
    }
    catch (StripeException ex) {
        _logger.LogError(ex, "Webhook signature verification failed");
        return BadRequest("Invalid signature");
    }
    
    // 3. Check idempotency
    var idempotencyResult = await _webhookIdempotencyService
        .CheckIdempotencyAsync(stripeEvent.Id, stripeEvent.Type);
    
    if (!idempotencyResult.ShouldProcess) {
        return Success($"Event skipped: {idempotencyResult.Reason}");
    }
    
    // 4. Process event with retry logic
    try {
        await ProcessWebhookWithRetryAsync(stripeEvent);
        await _webhookIdempotencyService.MarkAsProcessedAsync(stripeEvent.Id);
        return Success("Webhook processed successfully");
    }
    catch (Exception ex) {
        _logger.LogError(ex, "Error processing webhook {EventId}", stripeEvent.Id);
        await _webhookIdempotencyService.MarkAsFailedAsync(stripeEvent.Id, ex.Message);
        return Error("Webhook processing failed");
    }
}
```

### Payment Success Webhook

```csharp
private async Task HandlePaymentSucceeded(Event stripeEvent)
{
    var invoice = stripeEvent.Data.Object as Invoice;
    if (invoice == null) return;
    
    _logger.LogInformation(
        "Processing payment success for invoice {InvoiceId}", 
        invoice.Id);
    
    // 1. Find subscription by Stripe subscription ID
    var subscription = await _subscriptionRepository
        .GetByStripeSubscriptionIdAsync(invoice.SubscriptionId, SystemToken);
    
    if (subscription == null) {
        _logger.LogWarning(
            "Subscription not found for Stripe subscription {SubscriptionId}",
            invoice.SubscriptionId);
        return;
    }
    
    // 2. Find or create billing record
    var billingRecord = await _billingRepository
        .GetByStripeInvoiceIdAsync(invoice.Id);
    
    if (billingRecord == null) {
        billingRecord = await CreateBillingRecordFromInvoiceAsync(
            subscription, invoice, SystemToken);
    }
    
    // 3. Update billing record status
    billingRecord.Status = BillingRecord.BillingStatus.Paid;
    billingRecord.PaidAt = DateTime.UtcNow;
    billingRecord.StripeInvoiceId = invoice.Id;
    billingRecord.StripePaymentIntentId = invoice.PaymentIntentId;
    
    await _billingRepository.UpdateAsync(billingRecord);
    
    // 4. Record external payment (creates SubscriptionPayment)
    await _paymentService.RecordExternalPaymentAsync(
        billingRecord.Id, SystemToken);
    
    // 5. Update subscription status
    if (subscription.Status != Subscription.SubscriptionStatuses.Active) {
        subscription.Status = Subscription.SubscriptionStatuses.Active;
        subscription.FailedPaymentAttempts = 0;
        subscription.LastPaymentError = null;
        
        await _subscriptionRepository.UpdateSubscriptionAsync(subscription);
    }
    
    // 6. Send notification
    await _subscriptionNotificationService
        .SendPaymentSuccessNotificationAsync(subscription.Id, SystemToken);
    
    _logger.LogInformation(
        "Successfully processed payment for subscription {SubscriptionId}",
        subscription.Id);
}
```

### Payment Failure Webhook

```csharp
private async Task HandlePaymentFailed(Event stripeEvent)
{
    var invoice = stripeEvent.Data.Object as Invoice;
    if (invoice == null) return;
    
    var subscription = await _subscriptionRepository
        .GetByStripeSubscriptionIdAsync(invoice.SubscriptionId, SystemToken);
    
    if (subscription == null) return;
    
    // Increment failed attempts
    subscription.FailedPaymentAttempts++;
    subscription.LastPaymentFailedDate = DateTime.UtcNow;
    subscription.LastPaymentError = invoice.LastFinalizationError?.Message ?? "Payment failed";
    
    // Update status if max retries exceeded
    if (subscription.FailedPaymentAttempts >= 3) {
        subscription.Status = Subscription.SubscriptionStatuses.PaymentFailed;
        
        await _subscriptionNotificationService
            .SendPaymentFailedFinalNotificationAsync(subscription.Id, SystemToken);
    }
    else {
        await _subscriptionNotificationService
            .SendPaymentFailedRetryNotificationAsync(
                subscription.Id, 
                subscription.FailedPaymentAttempts, 
                SystemToken);
    }
    
    await _subscriptionRepository.UpdateSubscriptionAsync(subscription);
}
```

---

## Refund Management

### Refund Process

**Service**: `PaymentService.ProcessRefundAsync()`

```csharp
public async Task<JsonModel> ProcessRefundAsync(
    Guid paymentId,
    decimal refundAmount,
    string reason,
    TokenModel tokenModel)
{
    await _unitOfWork.BeginTransactionAsync();
    try {
        // 1. Get payment record
        var payment = await _subscriptionPaymentRepository.GetByIdAsync(paymentId);
        if (payment == null) {
            return NotFound("Payment not found");
        }
        
        // 2. Validate refund amount
        var maxRefund = payment.Amount - payment.RefundedAmount;
        if (refundAmount > maxRefund) {
            return BadRequest($"Refund amount exceeds available amount: {maxRefund}");
        }
        
        // 3. Process refund in Stripe
        var stripeRefundResult = await _stripeService.ProcessRefundAsync(
            payment.StripePaymentIntentId,
            refundAmount,
            tokenModel
        );
        
        if (!stripeRefundResult) {
            return Error("Stripe refund failed");
        }
        
        // 4. Create refund record
        var refund = new PaymentRefund {
            Id = Guid.NewGuid(),
            SubscriptionPaymentId = payment.Id,
            Amount = refundAmount,
            Reason = reason,
            RefundedAt = DateTime.UtcNow,
            ProcessedByUserId = tokenModel.UserID,
            StripeRefundId = stripeRefundResult.RefundId,
            CreatedBy = tokenModel.UserID,
            CreatedDate = DateTime.UtcNow
        };
        
        await _subscriptionPaymentRepository.AddRefundAsync(refund);
        
        // 5. Update payment record
        payment.RefundedAmount += refundAmount;
        
        if (payment.RefundedAmount >= payment.Amount) {
            payment.Status = SubscriptionPayment.PaymentStatus.Refunded;
        } else {
            payment.Status = SubscriptionPayment.PaymentStatus.PartiallyRefunded;
        }
        
        await _subscriptionPaymentRepository.UpdateAsync(payment);
        
        // 6. Update billing record
        var billingRecord = await _billingRepository.GetByIdAsync(payment.BillingRecordId);
        if (billingRecord != null && payment.RefundedAmount >= payment.Amount) {
            billingRecord.Status = BillingRecord.BillingStatus.Refunded;
            await _billingRepository.UpdateAsync(billingRecord);
        }
        
        await _unitOfWork.CommitTransactionAsync();
        
        return Success(refund);
    }
    catch {
        await _unitOfWork.RollbackTransactionAsync();
        throw;
    }
}
```

---

## Payment Security

### Security Measures

1. **Stripe Webhook Signature Verification**
   - Validates all webhook requests
   - Prevents unauthorized webhook calls

2. **Idempotency Checking**
   - Prevents duplicate processing
   - Tracks processed events

3. **Payment Method Validation**
   - Verifies card is valid before charging
   - Checks expiration dates

4. **Transaction Rollback**
   - Rolls back database on Stripe failure
   - Refunds Stripe on database failure

5. **Secure Token Handling**
   - Never stores credit card details
   - Uses Stripe payment method tokens

6. **Audit Logging**
   - Logs all payment operations
   - Tracks user actions

---

## Summary

The payment system provides:
- **Dual-service architecture** (PaymentService + StripeBillingService)
- **Complete Stripe integration** (customers, payments, subscriptions)
- **Webhook processing** with idempotency and retries
- **Comprehensive refund handling**
- **Transaction safety** with rollback mechanisms
- **Security best practices** throughout

**Next**: See [04_PRIVILEGE_MANAGEMENT.md](./04_PRIVILEGE_MANAGEMENT.md) for privilege system details.

---

*Document Version: 1.0*  
*Last Updated: 2025*



