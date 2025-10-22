# Service-Level Test Pattern Guide

## Principle: Test Business Logic Through Services, Not Database

### ❌ WRONG Pattern - Direct Database Operations
```csharp
[Fact]
public async Task Test_CancelSubscription_Wrong()
{
    // Creating subscription directly in DB
    var subscription = new Subscription { Status = "Active", ... };
    _context.Subscriptions.Add(subscription);
    await _context.SaveChangesAsync();
    
    // Manually changing status
    subscription.Status = "Cancelled";
    _context.Update(subscription);
    await _context.SaveChangesAsync();
    
    // Only testing database state, NOT business logic
    Assert.Equal("Cancelled", subscription.Status);
}
```

**Problems:**
- Doesn't test actual service methods
- Misses validation logic
- Misses side effects (status history, notifications, Stripe cancellation)
- Doesn't verify business rules

### ✅ CORRECT Pattern - Service Method Testing
```csharp
[Fact]
public async Task Test_CancelSubscription_Correct()
{
    // Arrange - Setup test environment
    var testEnv = await _testDataBuilder.CreateCompleteTestEnvironmentAsync();
    var users = await _testDataBuilder.CreateTestUsersAsync(1);
    
    // Create subscription through service (tests creation logic)
    var createDto = new CreateSubscriptionDto
    {
        UserId = users[0].Id,
        PlanId = testEnv.Plans[0].Id,
        BillingCycleId = testEnv.MasterData.BillingCycles[2].Id,
        PaymentMethodId = "pm_test_123"
    };
    var createResult = await _subscriptionLifecycleService.CreateSubscriptionAsync(createDto, _userToken);
    
    // Assert creation succeeded
    Assert.Equal(201, createResult.StatusCode); // Or 200
    var subscriptionId = ((dynamic)createResult.data).Id;
    
    // Act - Cancel through service (tests cancellation logic)
    var cancelResult = await _subscriptionLifecycleService.CancelSubscriptionAsync(
        subscriptionId,
        "Testing cancellation",
        _userToken
    );
    
    // Assert - Verify service response
    Assert.Equal(200, cancelResult.StatusCode);
    
    // Assert - Verify database state was correctly updated
    var subscription = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
    Assert.Equal("Cancelled", subscription.Status);
    Assert.NotNull(subscription.CancelledDate);
    Assert.Equal("Testing cancellation", subscription.CancellationReason);
    
    // Assert - Verify side effects (business logic validation)
    var statusHistory = await _context.SubscriptionStatusHistories
        .Where(sh => sh.SubscriptionId == subscription.Id)
        .OrderByDescending(sh => sh.ChangedAt)
        .FirstOrDefaultAsync();
    Assert.NotNull(statusHistory);
    Assert.Equal("Cancelled", statusHistory.ToStatus);
}
```

**Benefits:**
- Tests actual service method
- Validates business rules
- Verifies all side effects
- Tests complete workflow

## Key Test Patterns

### 1. Subscription Creation
```csharp
// Use SubscriptionLifecycleService.CreateSubscriptionAsync()
var dto = new CreateSubscriptionDto { ... };
var result = await _subscriptionLifecycleService.CreateSubscriptionAsync(dto, token);

// Verify:
// - Service response
// - Subscription created in DB
// - Privileges allocated
// - Billing record created
// - Status history recorded
```

### 2. Privilege Usage
```csharp
// Use PrivilegeService.UsePrivilegeAsync()
var result = await _privilegeService.UsePrivilegeAsync(
    subscriptionId,
    "TeleConsultation",
    1,
    token
);

// Verify:
// - Service response (success/failure)
// - UsedValue incremented
// - Usage history created
// - Limit enforcement
```

### 3. Payment Processing
```csharp
// Use PaymentService.ProcessPaymentAsync()
var result = await _paymentService.ProcessPaymentAsync(billingRecordId, token);

// Verify:
// - Payment status updated
// - Subscription dates updated (if renewal)
// - Privileges reset (if renewal)
// - SubscriptionPayment record created
```

### 4. Billing Renewal
```csharp
// Use AutomatedBillingService.ProcessAutomatedBillingAsync()
var result = await _automatedBillingService.ProcessAutomatedBillingAsync(token);

// Verify:
// - Billing records created
// - Payments processed
// - Privileges reset
// - Subscriptions updated
```

## Test Data Setup

### Use TestDataBuilder for Test Environment Only
```csharp
// Good: Setup test environment
var testEnv = await _testDataBuilder.CreateCompleteTestEnvironmentAsync();
var users = await _testDataBuilder.CreateTestUsersAsync(1);

// Then use SERVICES for actual operations
var result = await _subscriptionLifecycleService.CreateSubscriptionAsync(dto, token);
```

### Don't Use TestDataBuilder for Business Operations
```csharp
// ❌ WRONG: TestDataBuilder creates subscription directly
var subscription = await _testDataBuilder.CreateUserSubscriptionAsync(user, plan, cycle);

// ✅ RIGHT: Service creates subscription
var dto = new CreateSubscriptionDto { UserId = user.Id, PlanId = plan.Id, ... };
var result = await _subscriptionLifecycleService.CreateSubscriptionAsync(dto, token);
```

## Updated Test Structure

```csharp
[Fact]
public async Task Test_Feature_Scenario_ExpectedResult()
{
    // ============================================
    // ARRANGE: Setup test environment
    // ============================================
    var testEnv = await _testDataBuilder.CreateCompleteTestEnvironmentAsync();
    var users = await _testDataBuilder.CreateTestUsersAsync(1);
    var dto = new ServiceDto { ... };
    
    // ============================================
    // ACT: Call REAL service method
    // ============================================
    var result = await _actualService.ActualMethod(dto, token);
    
    // ============================================
    // ASSERT: Verify service response
    // ============================================
    Assert.Equal(expectedStatusCode, result.StatusCode);
    Assert.NotNull(result.data);
    
    // ============================================
    // ASSERT: Verify database state (side effects)
    // ============================================
    var entity = await _repository.GetByIdAsync(id);
    Assert.Equal(expectedValue, entity.Property);
    
    // ============================================
    // ASSERT: Verify business rules were enforced
    // ============================================
    // Check related entities, history records, etc.
}
```

## Services That MUST Be Real (Not Mocked)

- ✅ SubscriptionLifecycleService
- ✅ SubscriptionBillingService
- ✅ AutomatedBillingService
- ✅ PaymentService
- ✅ PrivilegeService
- ✅ SubscriptionService

## Services That CAN Be Mocked (External/Infrastructure)

- ✅ IStripeService (external Stripe API)
- ✅ INotificationService (external email/SMS)
- ✅ IUserService (simplified for testing)
- ✅ IUnitOfWork (transaction control)

