# Complete User Plan Purchase Flow - Step-by-Step Process

## Flow Overview

```
User Journey: Marketing Portal → Plan Selection → Purchase Flow → Payment → Subscription Creation → Confirmation
```

## Detailed Step-by-Step Process

### STEP 1: Plan Selection (Frontend)
**Location**: `frontend/smarttelehealth-app/src/app/features/marketing/plans/plan-list/plan-list.component.ts`

**Process**:
1. User visits `/plans` (Marketing Portal)
2. `PlanListComponent` loads and displays available plans
3. User clicks "Purchase" button on a plan
4. System checks if user is authenticated

**Code Flow**:
```typescript
purchasePlan(planId: string): void {
  if (this.authService.isAuthenticated()) {
    // User is authenticated - go directly to purchase flow
    this.router.navigate(['/web/subscriptions/purchase', planId]);
  } else {
    // User is not authenticated - redirect to registration
    this.router.navigate(['/web/register'], { 
      queryParams: { planId: planId, redirect: 'purchase' } 
    });
  }
}
```

**APIs Triggered**: None (Frontend routing only)

---

### STEP 2: Purchase Component Initialization (Frontend)
**Location**: `frontend/smarttelehealth-app/src/app/features/user/subscriptions/purchase-plan/purchase-plan.component.ts`

**Process**:
1. User navigates to `/web/subscriptions/purchase/:planId`
2. `PurchasePlanComponent` loads
3. Component initializes 4-step checkout stepper
4. Loads plan details, billing cycles, payment methods, and user data

**Code Flow**:
```typescript
ngOnInit(): void {
  this.planId = this.route.snapshot.paramMap.get('planId')!;
  
  // Load all required data
  this.loadPlanDetails();
  this.loadBillingCycles();
  this.loadPaymentMethods();
  this.loadCurrentUser();
  this.loadPrivileges();
}
```

**APIs Triggered**:
- `GET /api/SubscriptionPlans/{planId}` - Load plan details
- `GET /api/Billing/billing-cycles` - Load billing cycles
- `GET /api/Payment/methods` - Load user's payment methods
- `GET /api/Users/current` - Load current user data
- `GET /api/SubscriptionPlans/admin/privileges` - Load available privileges

---

### STEP 3: Payment Method Decision (Frontend)
**Location**: `frontend/smarttelehealth-app/src/app/features/user/subscriptions/purchase-plan/purchase-plan.component.ts`

**Process**:
1. User reaches Step 2 (Payment Method Selection)
2. System checks if user has saved payment methods
3. Two different flows based on payment method availability

**Code Flow**:
```typescript
nextStep(): void {
  if (this.currentStep === 2) {
    // If user has no payment methods, redirect to Stripe checkout
    if (this.paymentMethods.length === 0) {
      this.submitPurchaseWithStripeCheckout();
      return;
    }
    
    // If user has payment methods but hasn't selected one, show error
    const selectedPaymentMethod = this.billingForm.get('paymentMethodId')?.value;
    if (!selectedPaymentMethod || selectedPaymentMethod.trim() === '') {
      this.error = 'Please select a payment method';
      return;
    }
  }
}
```

**APIs Triggered**: None (Frontend logic only)

---

### STEP 4A: Existing Payment Method Flow (Frontend → Backend)
**Location**: `frontend/smarttelehealth-app/src/app/features/user/subscriptions/purchase-plan/purchase-plan.component.ts`

**Process**:
1. User has saved payment methods
2. User selects a payment method and clicks "Complete Purchase"
3. Frontend calls `submitPurchase()` method
4. Creates `CreateSubscriptionDto` and calls subscription API

**Code Flow**:
```typescript
submitPurchase(): void {
  const dto: CreateSubscriptionDto = {
    userId: this.currentUser!.id,
    planId: this.planId,
    price: this.calculateFinalPrice(),
    currencyId: this.plan!.currencyId,
    paymentMethodId: this.billingForm.value.paymentMethodId,
    autoRenew: true,
    startImmediately: true,
    isActive: true
  };

  this.subscriptionService.createSubscription(dto).subscribe({
    next: (response) => {
      if (response.statusCode === 200 || response.statusCode === 201) {
        // Success - redirect to subscriptions page
        this.router.navigate(['/web/subscriptions'], {
          queryParams: { success: 'true', newSubscription: 'true' }
        });
      }
    }
  });
}
```

**APIs Triggered**:
- `POST /api/Subscriptions` - Create subscription directly

---

### STEP 4B: Stripe Checkout Flow (Frontend → Backend → Stripe)
**Location**: `frontend/smarttelehealth-app/src/app/features/user/subscriptions/purchase-plan/purchase-plan.component.ts`

**Process**:
1. User has no saved payment methods
2. User clicks "Continue to Secure Checkout"
3. Frontend calls `submitPurchaseWithStripeCheckout()` method
4. Creates checkout session and redirects to Stripe

**Code Flow**:
```typescript
submitPurchaseWithStripeCheckout(): void {
  const request = {
    planId: this.planId,
    successUrl: `${window.location.origin}/web/subscriptions/success?session_id={CHECKOUT_SESSION_ID}`,
    cancelUrl: `${window.location.origin}/web/subscriptions/purchase/${this.planId}?cancelled=true`
  };

  this.stripeCheckoutService.createCheckoutSession(request).subscribe({
    next: (response) => {
      if (response.statusCode === 200 && response.data?.url) {
        // Redirect to Stripe checkout
        this.stripeCheckoutService.redirectToCheckout(response.data.url);
      }
    }
  });
}
```

**APIs Triggered**:
- `POST /api/stripe/create-checkout-session` - Create Stripe checkout session

---

### STEP 5: Backend Subscription Creation (Backend)
**Location**: `backend/SmartTelehealth.Application/Services/SubscriptionLifecycleService.cs`

**Process**:
1. `SubscriptionsController.CreateSubscription()` receives request
2. Calls `SubscriptionLifecycleService.CreateSubscriptionAsync()`
3. Comprehensive validation and processing
4. Creates Stripe customer and subscription
5. Creates local subscription entity
6. Sets up billing and privileges

**Code Flow**:
```csharp
public async Task<JsonModel> CreateSubscriptionAsync(CreateSubscriptionDto createDto, TokenModel tokenModel)
{
    // Step 1: Validate subscription plan exists and is active
    var requestedPlan = await _subscriptionRepository.GetSubscriptionPlanByIdAsync(Guid.Parse(createDto.PlanId));
    
    // Step 2: Use latest plan version for new subscriptions
    if (!requestedPlan.IsLatestVersion)
    {
        var latestVersion = await _subscriptionPlanRepository.GetAllVersionsOfPlanAsync(parentPlanId);
        plan = latestVersion.FirstOrDefault(v => v.IsLatestVersion && v.IsActive);
    }
    
    // Step 3: Check for duplicate subscriptions
    var existingSubscription = await _subscriptionRepository.GetActiveSubscriptionByUserAndPlanAsync(createDto.UserId, plan.Id);
    if (existingSubscription != null)
    {
        return new JsonModel { Message = "User already has an active subscription for this plan", StatusCode = 400 };
    }
    
    // Step 4: Create Stripe customer if not exists
    var stripeCustomerId = await _stripeService.GetOrCreateCustomerAsync(createDto.UserId, tokenModel);
    
    // Step 5: Create Stripe subscription
    var stripeSubscriptionId = await _stripeService.CreateSubscriptionAsync(
        stripeCustomerId,
        stripePriceId,
        createDto.PaymentMethodId,
        tokenModel
    );
    
    // Step 6: Create local subscription entity
    var entity = _mapper.Map<Subscription>(createDto);
    entity.StripeCustomerId = stripeCustomerId;
    entity.StripeSubscriptionId = stripeSubscriptionId;
    entity.StripePriceId = stripePriceId;
    entity.PaymentMethodId = createDto.PaymentMethodId;
    entity.CurrentPrice = BillingCalculationService.GetEffectivePlanPrice(plan, _logger);
    
    // Step 7: Save subscription and related data
    created = await _subscriptionRepository.CreateAsync(entity);
    await CreateInitialBillingRecordAsync(created, plan, tokenModel);
    await AllocateInitialPrivilegesAsync(created, plan, tokenModel);
    await _unitOfWork.CommitTransactionAsync();
}
```

**APIs Triggered**:
- Internal service calls to Stripe API
- Database operations for subscription creation

---

### STEP 6: Stripe Checkout Session Creation (Backend)
**Location**: `backend/SmartTelehealth.Infrastructure/Services/StripeService.cs`

**Process**:
1. `StripeController.CreateCheckoutSession()` receives request
2. Calls `StripeService.CreateCheckoutSessionWithCustomerAsync()`
3. Creates Stripe customer if needed
4. Creates Stripe checkout session
5. Returns checkout URL

**Code Flow**:
```csharp
public async Task<string> CreateCheckoutSessionWithCustomerAsync(string customerId, string priceId, string successUrl, string cancelUrl, TokenModel tokenModel, string? planId = null)
{
    var checkoutSessionCreateOptions = new SessionCreateOptions
    {
        Customer = customerId,
        PaymentMethodTypes = new List<string> { "card" },
        LineItems = new List<SessionLineItemOptions>
        {
            new SessionLineItemOptions
            {
                Price = priceId,
                Quantity = 1
            }
        },
        Mode = "subscription",
        SuccessUrl = successUrl,
        CancelUrl = cancelUrl,
        Metadata = new Dictionary<string, string>
        {
            { "customer_id", customerId },
            { "price_id", priceId },
            { "plan_id", planId },
            { "created_by_user_id", tokenModel.UserID.ToString() },
            { "created_by_role_id", tokenModel.RoleID.ToString() },
            { "created_at", DateTime.UtcNow.ToString("O") }
        }
    };
    
    var sessionService = new SessionService();
    var session = await sessionService.CreateAsync(checkoutSessionCreateOptions);
    return session.Url;
}
```

**APIs Triggered**:
- Stripe API: `POST /v1/checkout/sessions` - Create checkout session

---

### STEP 7: Stripe Payment Processing (Stripe)
**Process**:
1. User is redirected to Stripe Checkout
2. User enters payment information
3. Stripe processes payment
4. Stripe creates subscription
5. Stripe sends webhook events

**APIs Triggered**:
- Stripe API: Payment processing
- Stripe API: Subscription creation

---

### STEP 8: Webhook Processing (Backend)
**Location**: `backend/SmartTelehealth.Application/Services/WebhookService.cs`

**Process**:
1. `StripeWebhookController` receives webhook events
2. Validates webhook signature
3. Processes different event types
4. For `checkout.session.completed`, creates subscription

**Code Flow**:
```csharp
public async Task HandleCheckoutSessionCompletedAsync(Event stripeEvent)
{
    var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
    
    // Extract user ID and plan ID from session metadata
    var userId = session.Metadata["created_by_user_id"];
    var planId = session.Metadata["plan_id"];
    
    // Create subscription using existing lifecycle service
    var createDto = new CreateSubscriptionDto
    {
        UserId = userId,
        PlanId = planId,
        Price = plan.BasePrice,
        CurrencyId = plan.CurrencyId,
        PaymentMethodId = null, // Will be set from Stripe subscription
        AutoRenew = true,
        StartImmediately = true,
        IsActive = true
    };
    
    var result = await _lifecycleService.CreateSubscriptionAsync(createDto, tokenModel);
    
    // Update subscription with Stripe IDs
    if (result.StatusCode == 200)
    {
        var subscriptionEntity = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscription.Id));
        subscriptionEntity.StripeSubscriptionId = session.SubscriptionId;
        subscriptionEntity.StripeCustomerId = session.CustomerId;
        subscriptionEntity.StripePriceId = session.Metadata["price_id"];
        await _subscriptionRepository.UpdateAsync(subscriptionEntity);
    }
}
```

**APIs Triggered**:
- Internal service calls to create subscription
- Database operations for subscription creation

---

### STEP 9: Success Confirmation (Frontend)
**Process**:
1. User is redirected to success page
2. Success page displays confirmation
3. User can navigate to subscriptions page

**APIs Triggered**: None (Frontend routing only)

---

## Complete API Flow Summary

### For Existing Payment Methods:
```
Frontend → POST /api/Subscriptions → Backend Subscription Creation → Stripe API → Database
```

### For New Payment Methods (Stripe Checkout):
```
Frontend → POST /api/stripe/create-checkout-session → Stripe Checkout → Stripe API → Webhook → Backend Subscription Creation → Database
```

## Key APIs and Their Purposes

1. **`GET /api/SubscriptionPlans/{planId}`** - Load plan details
2. **`GET /api/Billing/billing-cycles`** - Load billing cycles
3. **`GET /api/Payment/methods`** - Load user's payment methods
4. **`POST /api/Subscriptions`** - Create subscription (existing payment methods)
5. **`POST /api/stripe/create-checkout-session`** - Create Stripe checkout session
6. **`POST /api/StripeWebhook`** - Handle Stripe webhook events
7. **Stripe API: `POST /v1/checkout/sessions`** - Create checkout session
8. **Stripe API: `POST /v1/subscriptions`** - Create subscription
9. **Stripe API: `POST /v1/customers`** - Create customer

## Payment Processing Flow

### Direct Payment Method:
1. User selects existing payment method
2. Frontend calls subscription creation API
3. Backend creates Stripe subscription with existing payment method
4. Subscription is immediately active

### Stripe Checkout:
1. User has no payment methods
2. Frontend creates checkout session
3. User is redirected to Stripe Checkout
4. User enters payment information
5. Stripe processes payment and creates subscription
6. Stripe sends webhook to backend
7. Backend creates local subscription record
8. User is redirected to success page

## Subscription Creation Initialization

The subscription creation is initialized through:

1. **Frontend Trigger**: User clicks "Complete Purchase" or "Continue to Secure Checkout"
2. **API Call**: Either `POST /api/Subscriptions` or `POST /api/stripe/create-checkout-session`
3. **Backend Processing**: `SubscriptionLifecycleService.CreateSubscriptionAsync()`
4. **Stripe Integration**: Customer creation, subscription creation, payment processing
5. **Database Operations**: Subscription entity creation, billing record creation, privilege allocation
6. **Confirmation**: Success response and user redirection

This complete flow ensures that subscriptions are created properly with all necessary data, payment processing, and Stripe synchronization.


