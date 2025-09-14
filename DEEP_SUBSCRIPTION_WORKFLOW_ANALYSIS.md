# Deep Dive: SmartTeleHealth Subscription Management Workflow Analysis

## Table of Contents
1. [System Architecture Overview](#system-architecture-overview)
2. [Subscription Lifecycle Management](#subscription-lifecycle-management)
3. [Subscription Plan Management](#subscription-plan-management)
4. [Privilege System Architecture](#privilege-system-architecture)
5. [Billing and Payment Processing](#billing-and-payment-processing)
6. [Stripe Integration Deep Dive](#stripe-integration-deep-dive)
7. [Database Relationships and Data Flow](#database-relationships-and-data-flow)
8. [Business Logic and Validation](#business-logic-and-validation)
9. [Error Handling and Resilience](#error-handling-and-resilience)
10. [Real-World Workflow Examples](#real-world-workflow-examples)

---

## System Architecture Overview

### Core Components Integration

The subscription management system follows a sophisticated multi-layered architecture:

```
┌─────────────────────────────────────────────────────────────┐
│                    API Controllers Layer                    │
├─────────────────────────────────────────────────────────────┤
│  SubscriptionsController  │  SubscriptionPlansController   │
│  StripeController        │  StripeWebhookController       │
│  BillingController       │  PrivilegesController          │
└─────────────────────────────────────────────────────────────┘
                                │
┌─────────────────────────────────────────────────────────────┐
│                   Service Layer                            │
├─────────────────────────────────────────────────────────────┤
│  SubscriptionService      │  SubscriptionLifecycleService  │
│  SubscriptionPlanService  │  BillingService               │
│  PrivilegeService         │  StripeService                │
│  NotificationService      │  UserService                  │
└─────────────────────────────────────────────────────────────┘
                                │
┌─────────────────────────────────────────────────────────────┐
│                  Repository Layer                          │
├─────────────────────────────────────────────────────────────┤
│  SubscriptionRepository   │  SubscriptionPlanRepository    │
│  BillingRepository        │  PrivilegeRepository           │
│  UserSubscriptionPrivilegeUsageRepository                  │
└─────────────────────────────────────────────────────────────┘
                                │
┌─────────────────────────────────────────────────────────────┐
│                    Entity Layer                            │
├─────────────────────────────────────────────────────────────┤
│  Subscription            │  SubscriptionPlan              │
│  User                   │  Privilege                     │
│  BillingRecord          │  UserSubscriptionPrivilegeUsage │
└─────────────────────────────────────────────────────────────┘
```

---

## Subscription Lifecycle Management

### 1. Subscription Creation Workflow

The subscription creation process is handled by `SubscriptionLifecycleService.CreateSubscriptionAsync()`:

#### Step-by-Step Process:

**Phase 1: Validation and Preparation**
```csharp
// 1. Validate subscription plan exists and is active
var plan = await _subscriptionRepository.GetSubscriptionPlanByIdAsync(Guid.Parse(createDto.PlanId));
if (plan == null || !plan.IsActive) return Error;

// 2. Prevent duplicate subscriptions
var userSubscriptions = await _subscriptionRepository.GetByUserIdAsync(createDto.UserId);
if (userSubscriptions.Any(s => s.SubscriptionPlanId == plan.Id && 
    (s.Status == "Active" || s.Status == "Paused"))) return Error;

// 3. Get user details for Stripe integration
var userResult = await _userService.GetUserByIdAsync(createDto.UserId, tokenModel);
```

**Phase 2: Stripe Integration Setup**
```csharp
// 4. Ensure Stripe Customer exists
string stripeCustomerId = await EnsureStripeCustomerAsync(user, tokenModel);

// 5. Validate Payment Method
if (!string.IsNullOrEmpty(createDto.PaymentMethodId))
{
    var isValid = await _stripeService.ValidatePaymentMethodAsync(createDto.PaymentMethodId, tokenModel);
}

// 6. Create Stripe Subscription
string stripePriceId = await GetStripePriceIdForBillingCycleAsync(plan, createDto.BillingCycleId);
string stripeSubscriptionId = await _stripeService.CreateSubscriptionAsync(
    stripeCustomerId, stripePriceId, createDto.PaymentMethodId, tokenModel);
```

**Phase 3: Local Subscription Creation**
```csharp
// 7. Create local subscription entity
var entity = _mapper.Map<Subscription>(createDto);
entity.StripeCustomerId = stripeCustomerId;
entity.StripeSubscriptionId = stripeSubscriptionId;
entity.StripePriceId = stripePriceId;
entity.PaymentMethodId = createDto.PaymentMethodId;
entity.CurrentPrice = plan.Price;
entity.Status = plan.IsTrialAllowed ? "TrialActive" : "Active";
entity.StartDate = DateTime.UtcNow;
entity.NextBillingDate = await CalculateNextBillingDateAsync(DateTime.UtcNow, createDto.BillingCycleId);
```

**Phase 4: Privilege Setup and Notifications**
```csharp
// 8. Initialize privilege usage records
await InitializePrivilegeUsageAsync(entity.Id, plan.Id, tokenModel);

// 9. Send welcome notification
await _subscriptionNotificationService.SendSubscriptionCreatedNotificationAsync(
    entity.Id.ToString(), tokenModel);

// 10. Create audit trail
await _subscriptionRepository.AddStatusHistoryAsync(new SubscriptionStatusHistory {
    SubscriptionId = entity.Id,
    FromStatus = "Pending",
    ToStatus = entity.Status,
    Reason = "Subscription created",
    ChangedAt = DateTime.UtcNow
});
```

### 2. Subscription Status Transitions

The system supports comprehensive status management:

#### Valid Status Transitions:
```
Pending → Active, TrialActive, Cancelled
Active → Paused, Cancelled, Expired, PaymentFailed
Paused → Active, Cancelled, Expired
TrialActive → Active, TrialExpired, Cancelled
TrialExpired → Active, Cancelled
PaymentFailed → Active, Cancelled, Expired
```

#### Status Transition Logic:
```csharp
public ValidationResult ValidateStatusTransition(string newStatus)
{
    var validTransitions = GetValidStatusTransitions();
    if (!validTransitions.Contains(newStatus))
        return new ValidationResult($"Cannot transition from '{Status}' to '{newStatus}'");
    
    return ValidationResult.Success;
}
```

---

## Subscription Plan Management

### 1. Plan Creation and Configuration

The `SubscriptionPlanService` handles comprehensive plan management:

#### Plan Entity Structure:
```csharp
public class SubscriptionPlan : BaseEntity
{
    // Core Properties
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountedPrice { get; set; }
    
    // Billing Configuration
    public Guid BillingCycleId { get; set; }
    public Guid CurrencyId { get; set; }
    public Guid? CategoryId { get; set; }
    
    // Trial Configuration
    public bool IsTrialAllowed { get; set; }
    public int TrialDurationInDays { get; set; }
    
    // Stripe Integration
    public string? StripeProductId { get; set; }
    public string? StripeMonthlyPriceId { get; set; }
    public string? StripeQuarterlyPriceId { get; set; }
    public string? StripeAnnualPriceId { get; set; }
    
    // Plan Features
    public int MessagingCount { get; set; }
    public bool IncludesMedicationDelivery { get; set; }
    public bool IncludesFollowUpCare { get; set; }
    public int DeliveryFrequencyDays { get; set; }
    public int MaxPauseDurationDays { get; set; }
}
```

#### Plan Creation Workflow:
```csharp
public async Task<JsonModel> CreatePlanAsync(CreateSubscriptionPlanDto createDto, TokenModel tokenModel)
{
    // 1. Admin validation
    if (tokenModel.RoleID != 1 && tokenModel.RoleID != 3) return AccessDenied;
    
    // 2. Validate required fields
    if (string.IsNullOrWhiteSpace(createDto.Name)) return ValidationError;
    
    // 3. Check for duplicate names
    var existingPlans = await _subscriptionPlanRepository.GetAllAsync();
    if (existingPlans.Any(p => p.Name.Equals(createDto.Name, StringComparison.OrdinalIgnoreCase)))
        return DuplicateError;
    
    // 4. Create Stripe product and prices
    string stripeProductId = await _stripeService.CreateProductAsync(createDto.Name, createDto.Description, tokenModel);
    string monthlyPriceId = await _stripeService.CreatePriceAsync(stripeProductId, createDto.Price, "usd", "month", 1, tokenModel);
    
    // 5. Create local plan entity
    var plan = new SubscriptionPlan
    {
        Name = createDto.Name,
        Description = createDto.Description,
        Price = createDto.Price,
        BillingCycleId = createDto.BillingCycleId,
        CurrencyId = createDto.CurrencyId,
        StripeProductId = stripeProductId,
        StripeMonthlyPriceId = monthlyPriceId,
        // ... other properties
    };
    
    // 6. Save and return
    await _subscriptionPlanRepository.AddAsync(plan);
    return Success;
}
```

### 2. Plan-Privilege Relationship Management

The system uses a sophisticated privilege management system:

#### SubscriptionPlanPrivilege Junction Entity:
```csharp
public class SubscriptionPlanPrivilege : BaseEntity
{
    public Guid SubscriptionPlanId { get; set; }
    public Guid PrivilegeId { get; set; }
    public int Value { get; set; } // -1 = unlimited, 0 = disabled, >0 = limited
    public Guid UsagePeriodId { get; set; }
    public int DurationMonths { get; set; }
    
    // Time-based limits
    public int? DailyLimit { get; set; }
    public int? WeeklyLimit { get; set; }
    public int? MonthlyLimit { get; set; }
    
    // Navigation properties
    public virtual SubscriptionPlan SubscriptionPlan { get; set; }
    public virtual Privilege Privilege { get; set; }
    public virtual MasterBillingCycle UsagePeriod { get; set; }
}
```

---

## Privilege System Architecture

### 1. Privilege Usage Tracking

The system implements comprehensive privilege usage tracking:

#### UserSubscriptionPrivilegeUsage Entity:
```csharp
public class UserSubscriptionPrivilegeUsage : BaseEntity
{
    public Guid SubscriptionId { get; set; }
    public Guid SubscriptionPlanPrivilegeId { get; set; }
    public int UsedValue { get; set; }
    public int AllowedValue { get; set; }
    public DateTime UsagePeriodStart { get; set; }
    public DateTime UsagePeriodEnd { get; set; }
    public DateTime? LastUsedAt { get; set; }
    
    // Navigation properties
    public virtual Subscription Subscription { get; set; }
    public virtual SubscriptionPlanPrivilege SubscriptionPlanPrivilege { get; set; }
    public virtual ICollection<PrivilegeUsageHistory> UsageHistory { get; set; }
}
```

### 2. Privilege Usage Validation

The `PrivilegeService` implements sophisticated usage validation:

#### Usage Validation Workflow:
```csharp
public async Task<bool> UsePrivilegeAsync(Guid subscriptionId, string privilegeName, int amount, TokenModel tokenModel)
{
    // 1. Validate input parameters
    if (amount <= 0) return false;
    
    // 2. Get plan privilege configuration
    var planPrivilege = await GetPlanPrivilegeAsync(subscriptionId, privilegeName);
    if (planPrivilege == null) return false;
    
    // 3. Check if privilege is disabled
    if (planPrivilege.Value == 0) return false;
    
    // 4. Check time-based limits (daily, weekly, monthly)
    if (!await CheckTimeBasedLimitsAsync(subscriptionId, planPrivilege, amount))
        return false;
    
    // 5. Handle unlimited privileges
    if (planPrivilege.Value == -1)
    {
        await UpdateUnlimitedUsageAsync(subscriptionId, planPrivilege, amount, tokenModel);
        return true;
    }
    
    // 6. Check remaining amount for limited privileges
    var remaining = await GetRemainingPrivilegeAsync(subscriptionId, privilegeName, tokenModel);
    if (remaining < amount) return false;
    
    // 7. Update usage records
    await UpdateLimitedUsageAsync(subscriptionId, planPrivilege, amount, tokenModel);
    
    // 8. Record usage history
    await AddUsageHistoryAsync(usageRecord.Id, amount, tokenModel);
    
    return true;
}
```

### 3. Time-Based Limit Enforcement

The system enforces multiple levels of usage limits:

```csharp
private async Task<bool> CheckTimeBasedLimitsAsync(Guid subscriptionId, SubscriptionPlanPrivilege planPrivilege, int amount)
{
    var now = DateTime.UtcNow;
    var today = now.Date;
    var weekStart = today.AddDays(-(int)today.DayOfWeek);
    var monthStart = new DateTime(today.Year, today.Month, 1);

    // Check daily limit
    if (planPrivilege.DailyLimit.HasValue)
    {
        var dailyUsage = await _usageHistoryRepo.GetDailyUsageAsync(subscriptionId, planPrivilege.Id, today);
        if (dailyUsage + amount > planPrivilege.DailyLimit.Value) return false;
    }

    // Check weekly limit
    if (planPrivilege.WeeklyLimit.HasValue)
    {
        var weeklyUsage = await _usageHistoryRepo.GetWeeklyUsageAsync(subscriptionId, planPrivilege.Id, weekStart);
        if (weeklyUsage + amount > planPrivilege.WeeklyLimit.Value) return false;
    }

    // Check monthly limit
    if (planPrivilege.MonthlyLimit.HasValue)
    {
        var monthlyUsage = await _usageHistoryRepo.GetMonthlyUsageAsync(subscriptionId, planPrivilege.Id, monthStart);
        if (monthlyUsage + amount > planPrivilege.MonthlyLimit.Value) return false;
    }

    return true;
}
```

---

## Billing and Payment Processing

### 1. Billing Record Management

The `BillingService` handles comprehensive billing operations:

#### BillingRecord Entity Structure:
```csharp
public class BillingRecord : BaseEntity
{
    // Core Properties
    public int UserId { get; set; }
    public Guid? SubscriptionId { get; set; }
    public BillingStatus Status { get; set; }
    public BillingType Type { get; set; }
    
    // Financial Details
    public decimal Amount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal TotalAmount { get; set; }
    
    // Dates
    public DateTime BillingDate { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? DueDate { get; set; }
    
    // Stripe Integration
    public string? StripePaymentIntentId { get; set; }
    public string? StripeInvoiceId { get; set; }
    
    // Payment Details
    public string? PaymentMethod { get; set; }
    public string? TransactionId { get; set; }
    public string? FailureReason { get; set; }
    
    // Recurring Billing
    public bool IsRecurring { get; set; }
    public DateTime? NextBillingDate { get; set; }
}
```

### 2. Payment Processing Workflow

#### Payment Processing Steps:
```csharp
public async Task<JsonModel> ProcessPaymentAsync(string subscriptionId, PaymentRequestDto paymentRequest, TokenModel tokenModel)
{
    // 1. Validate input parameters
    if (string.IsNullOrEmpty(subscriptionId) || !Guid.TryParse(subscriptionId, out _)) return ValidationError;
    if (paymentRequest == null || paymentRequest.Amount <= 0) return ValidationError;
    
    // 2. Validate access permissions
    if (tokenModel.RoleID != 1 && !await HasAccessToSubscription(tokenModel.UserID, subscriptionId))
        return AccessDenied;
    
    // 3. Get subscription details
    var subscription = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
    if (subscription == null) return NotFound;
    
    // 4. Process payment through Stripe
    var paymentResult = await _stripeService.ProcessPaymentAsync(
        paymentRequest.PaymentMethodId,
        paymentRequest.Amount,
        paymentRequest.Currency ?? "usd",
        tokenModel
    );
    
    // 5. Handle payment result
    if (paymentResult.Status == "succeeded")
    {
        // Update subscription status
        if (subscription.Status == "PaymentFailed")
        {
            subscription.Status = "Active";
            subscription.FailedPaymentAttempts = 0;
            subscription.LastPaymentError = null;
            await _subscriptionRepository.UpdateAsync(subscription);
        }
        
        // Create billing record
        var billingRecordDto = new CreateBillingRecordDto
        {
            UserId = subscription.UserId,
            SubscriptionId = subscription.Id.ToString(),
            Amount = paymentRequest.Amount,
            Status = "Paid",
            Type = "Subscription",
            Description = $"Payment for subscription {subscription.Id}",
            BillingDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow
        };
        
        await _billingService.CreateBillingRecordAsync(billingRecordDto, tokenModel);
        
        return Success;
    }
    else
    {
        return PaymentFailed;
    }
}
```

---

## Stripe Integration Deep Dive

### 1. Stripe Service Interface

The system implements comprehensive Stripe integration through `IStripeService`:

#### Key Stripe Operations:
```csharp
public interface IStripeService
{
    // Customer Management
    Task<string> CreateCustomerAsync(string email, string name, TokenModel tokenModel);
    Task<CustomerDto> GetCustomerAsync(string customerId, TokenModel tokenModel);
    
    // Payment Methods
    Task<string> AddPaymentMethodAsync(string customerId, string paymentMethodId, TokenModel tokenModel);
    Task<bool> ValidatePaymentMethodAsync(string paymentMethodId, TokenModel tokenModel);
    
    // Subscription Management
    Task<string> CreateSubscriptionAsync(string customerId, string priceId, string paymentMethodId, TokenModel tokenModel);
    Task<bool> CancelSubscriptionAsync(string subscriptionId, TokenModel tokenModel);
    Task<bool> UpdateSubscriptionAsync(string subscriptionId, string priceId, TokenModel tokenModel);
    
    // Payment Processing
    Task<PaymentResultDto> ProcessPaymentAsync(string paymentMethodId, decimal amount, string currency, TokenModel tokenModel);
    Task<bool> ProcessRefundAsync(string paymentIntentId, decimal amount, TokenModel tokenModel);
    
    // Checkout Sessions
    Task<string> CreateCheckoutSessionAsync(string priceId, string successUrl, string cancelUrl, TokenModel tokenModel);
}
```

### 2. Webhook Processing

The `StripeWebhookController` handles real-time Stripe events:

#### Webhook Event Processing:
```csharp
[HttpPost]
public async Task<JsonModel> HandleWebhook()
{
    var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
    var webhookSecret = _configuration["Stripe:WebhookSecret"];
    
    try
    {
        // Verify webhook signature
        var stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], webhookSecret);
        
        // Process event based on type
        switch (stripeEvent.Type)
        {
            case "customer.subscription.created":
                await HandleSubscriptionCreated(stripeEvent);
                break;
            case "customer.subscription.updated":
                await HandleSubscriptionUpdated(stripeEvent);
                break;
            case "customer.subscription.deleted":
                await HandleSubscriptionDeleted(stripeEvent);
                break;
            case "invoice.payment_succeeded":
                await HandlePaymentSucceeded(stripeEvent);
                break;
            case "invoice.payment_failed":
                await HandlePaymentFailed(stripeEvent);
                break;
            // ... other event types
        }
        
        return Success;
    }
    catch (StripeException ex)
    {
        _logger.LogError(ex, "Stripe webhook error");
        return Error;
    }
}
```

#### Payment Success Handling:
```csharp
private async Task HandlePaymentSucceeded(Event stripeEvent)
{
    var invoice = stripeEvent.Data.Object as Stripe.Invoice;
    if (invoice == null) return;

    var subscriptionId = GetSubscriptionIdFromInvoice(invoice);
    if (!string.IsNullOrEmpty(subscriptionId))
    {
        var localSubscription = await _subscriptionService.GetByStripeSubscriptionIdAsync(subscriptionId, GetToken(HttpContext));
        if (localSubscription.StatusCode == 200)
        {
            // Determine new status based on current state
            string newStatus = "Active";
            string reason = "Payment succeeded via Stripe";

            // Handle trial conversion
            if (subscriptionData.Status == "TrialActive")
            {
                newStatus = "Active";
                reason = "Trial converted to active subscription via payment";
            }
            // Handle payment failure recovery
            else if (subscriptionData.Status == "PaymentFailed")
            {
                newStatus = "Active";
                reason = "Subscription reactivated after successful payment";
            }

            // Update subscription
            var updateDto = new UpdateSubscriptionDto
            {
                Status = newStatus,
                LastPaymentDate = DateTime.UtcNow,
                FailedPaymentAttempts = 0,
                LastPaymentError = null
            };

            await _subscriptionLifecycleService.UpdateSubscriptionAsync(
                localSubscription.data.ToString(), updateDto, GetToken(HttpContext));

            // Send notifications
            await _notificationService.SendPaymentSuccessEmailAsync(/*...*/);
        }
    }
}
```

---

## Database Relationships and Data Flow

### 1. Core Entity Relationships

```
User (1) ──────────── (N) Subscription
  │                        │
  │                        │
  └── (N) BillingRecord    │
                           │
SubscriptionPlan (1) ──── (N) Subscription
  │
  │
  └── (N) SubscriptionPlanPrivilege (N) ──── Privilege
                    │
                    │
                    └── (N) UserSubscriptionPrivilegeUsage
                                │
                                │
                                └── (N) PrivilegeUsageHistory
```

### 2. Data Flow Patterns

#### Subscription Creation Data Flow:
```
1. User Request → Controller
2. Controller → SubscriptionLifecycleService
3. Service → UserService (get user details)
4. Service → StripeService (create customer/subscription)
5. Service → SubscriptionRepository (save local subscription)
6. Service → PrivilegeService (initialize privileges)
7. Service → NotificationService (send welcome email)
8. Service → AuditService (log creation)
```

#### Payment Processing Data Flow:
```
1. Payment Request → Controller
2. Controller → SubscriptionService
3. Service → StripeService (process payment)
4. Service → BillingService (create billing record)
5. Service → SubscriptionRepository (update status)
6. Service → NotificationService (send confirmation)
7. Webhook → StripeWebhookController (sync status)
```

---

## Business Logic and Validation

### 1. Subscription Validation Rules

#### Plan Validation:
```csharp
// Plan must exist and be active
if (plan == null || !plan.IsActive) return ValidationError;

// No duplicate active subscriptions
if (userSubscriptions.Any(s => s.SubscriptionPlanId == plan.Id && 
    (s.Status == "Active" || s.Status == "Paused"))) return DuplicateError;
```

#### Payment Method Validation:
```csharp
// Payment method must be valid
var isValid = await _stripeService.ValidatePaymentMethodAsync(paymentMethodId, tokenModel);
if (!isValid) return ValidationError;
```

### 2. Privilege Usage Validation

#### Usage Limit Validation:
```csharp
// Check remaining usage
var remaining = await GetRemainingPrivilegeAsync(subscriptionId, privilegeName, tokenModel);
if (remaining < amount) return LimitExceeded;

// Check time-based limits
if (!await CheckTimeBasedLimitsAsync(subscriptionId, planPrivilege, amount))
    return TimeLimitExceeded;
```

### 3. Status Transition Validation

#### Business Rules:
```csharp
public ValidationResult ValidateStatusTransition(string newStatus)
{
    // Check if status is valid
    if (!SubscriptionStatuses.ValidStatuses.Contains(newStatus))
        return new ValidationResult($"'{newStatus}' is not a valid subscription status.");
    
    // Check if transition is allowed
    var validTransitions = GetValidStatusTransitions();
    if (!validTransitions.Contains(newStatus))
        return new ValidationResult($"Cannot transition from '{Status}' to '{newStatus}'.");
    
    return ValidationResult.Success;
}
```

---

## Error Handling and Resilience

### 1. Exception Management

#### Global Exception Handling:
```csharp
public class GlobalExceptionMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }
}
```

### 2. Retry Logic

#### Payment Retry Mechanism:
```csharp
public async Task<JsonModel> RetryPaymentAsync(string subscriptionId, PaymentRequestDto paymentRequest, TokenModel tokenModel)
{
    // Get subscription
    var subscription = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
    
    // Process payment retry
    var paymentResult = await _stripeService.ProcessPaymentAsync(
        paymentMethodId, amount, currency, tokenModel);
    
    if (paymentResult.Status == "succeeded")
    {
        // Reactivate subscription
        subscription.Status = "Active";
        subscription.FailedPaymentAttempts = 0;
        await _subscriptionRepository.UpdateAsync(subscription);
        
        return Success;
    }
    else
    {
        return PaymentFailed;
    }
}
```

### 3. Data Consistency

#### Transaction Management:
```csharp
public async Task<JsonModel> CreateSubscriptionAsync(CreateSubscriptionDto createDto, TokenModel tokenModel)
{
    using var transaction = await _unitOfWork.BeginTransactionAsync();
    try
    {
        // Create Stripe subscription
        var stripeSubscriptionId = await _stripeService.CreateSubscriptionAsync(/*...*/);
        
        // Create local subscription
        var subscription = new Subscription(/*...*/);
        await _subscriptionRepository.AddAsync(subscription);
        
        // Initialize privileges
        await InitializePrivilegeUsageAsync(subscription.Id, plan.Id, tokenModel);
        
        // Commit transaction
        await _unitOfWork.CommitAsync();
        
        return Success;
    }
    catch (Exception ex)
    {
        await _unitOfWork.RollbackAsync();
        throw;
    }
}
```

---

## Real-World Workflow Examples

### 1. Complete Subscription Creation Flow

#### User Journey:
```
1. User visits subscription plans page
2. User selects "Premium Plan" with monthly billing
3. User provides payment method (card ending in 1234)
4. System validates plan availability
5. System creates Stripe customer
6. System validates payment method
7. System creates Stripe subscription
8. System creates local subscription record
9. System initializes privilege usage records
10. System sends welcome email
11. User receives confirmation and can start using services
```

#### System Processing:
```csharp
// API Call: POST /api/subscriptions
{
    "userId": 123,
    "planId": "premium-plan-guid",
    "billingCycleId": "monthly-guid",
    "paymentMethodId": "pm_1234567890"
}

// System Response:
{
    "data": {
        "id": "subscription-guid",
        "status": "Active",
        "planName": "Premium Plan",
        "nextBillingDate": "2024-02-01T00:00:00Z",
        "privileges": [
            {
                "name": "Teleconsultation",
                "allowedValue": 10,
                "usedValue": 0,
                "remainingValue": 10
            },
            {
                "name": "MedicationDelivery",
                "allowedValue": 2,
                "usedValue": 0,
                "remainingValue": 2
            }
        ]
    },
    "message": "Subscription created successfully",
    "statusCode": 200
}
```

### 2. Privilege Usage Flow

#### User Books Consultation:
```csharp
// API Call: POST /api/subscriptions/{id}/book-consultation
{
    "userId": 123,
    "subscriptionId": "subscription-guid"
}

// System Processing:
1. Validate subscription is active
2. Check remaining teleconsultation privileges
3. Validate time-based limits (daily/weekly/monthly)
4. Record privilege usage
5. Create consultation booking
6. Send confirmation notification
7. Update usage statistics
```

### 3. Payment Failure and Recovery

#### Payment Failure Scenario:
```
1. Stripe attempts recurring payment
2. Payment fails (insufficient funds)
3. Stripe sends webhook: invoice.payment_failed
4. System updates subscription status to "PaymentFailed"
5. System increments failed payment attempts
6. System sends payment failure notification
7. User updates payment method
8. User retries payment
9. Payment succeeds
10. System updates status to "Active"
11. System resets failed payment attempts
12. System sends payment success notification
```

### 4. Subscription Cancellation Flow

#### User Cancels Subscription:
```csharp
// API Call: DELETE /api/subscriptions/{id}
{
    "reason": "No longer needed",
    "effectiveDate": "end_of_billing_period"
}

// System Processing:
1. Validate cancellation request
2. Cancel Stripe subscription
3. Update local subscription status to "Cancelled"
4. Set cancellation date and reason
5. Create status history record
6. Send cancellation confirmation
7. Schedule end-of-period deactivation
```

---

## Conclusion

The SmartTeleHealth subscription management system represents a sophisticated, enterprise-grade solution that handles complex business requirements while maintaining data integrity and providing excellent user experience. The system's architecture supports:

- **Scalability**: Clean separation of concerns and modular design
- **Reliability**: Comprehensive error handling and retry mechanisms
- **Flexibility**: Configurable privilege system and billing cycles
- **Integration**: Seamless Stripe integration with real-time synchronization
- **Auditability**: Complete audit trails and usage tracking
- **User Experience**: Intuitive APIs and comprehensive notifications

The workflow analysis demonstrates how each component works together to provide a robust subscription management platform that can handle real-world business scenarios while maintaining data consistency and providing excellent user experience.


