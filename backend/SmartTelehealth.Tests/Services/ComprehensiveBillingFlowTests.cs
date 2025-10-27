using Microsoft.Extensions.Logging;
using Moq;
using SmartTelehealth.Application.Services;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Tests.Infrastructure;
using Xunit;

namespace SmartTelehealth.Tests.Services;

/// <summary>
/// Comprehensive end-to-end billing flow tests that verify the complete payment journey
/// from plan creation to subscription lifecycle events.
/// </summary>
public class ComprehensiveBillingFlowTests : TestBase
{
    private readonly ISubscriptionPlanService _planService;
    private readonly ISubscriptionService _subscriptionService;
    private readonly ISubscriptionBillingService _billingService;
    private readonly IAutomatedBillingService _automatedBillingService;
    private readonly ISubscriptionLifecycleService _lifecycleService;
    private readonly IPlanPricingService _pricingService;

    public ComprehensiveBillingFlowTests()
    {
        _planService = GetService<ISubscriptionPlanService>();
        _subscriptionService = GetService<ISubscriptionService>();
        _billingService = GetService<ISubscriptionBillingService>();
        _automatedBillingService = GetService<IAutomatedBillingService>();
        _lifecycleService = GetService<ISubscriptionLifecycleService>();
        _pricingService = GetService<IPlanPricingService>();
    }

    #region Complete User Journey Tests

    [Fact]
    public async Task CompleteUserJourney_PlanPurchaseToRenewal_ShouldWorkEndToEnd()
    {
        // Arrange
        var adminToken = CreateTestToken();
        var userToken = CreateTestToken();

        // Step 1: Admin creates a subscription plan with auto-calculated pricing
        var createPlanDto = new CreateSubscriptionPlanDto
        {
            Name = "Premium Healthcare Plan",
            Description = "Comprehensive healthcare plan with video consultations and prescriptions",
            BasePrice = 0m, // Will be auto-calculated
            BillingCycleId = MasterData.BillingCycles.First(bc => bc.Name == "monthly").Id,
            CurrencyId = MasterData.Currencies.First(c => c.Code == "USD").Id,
            CategoryId = MasterData.Categories.First().Id,
            IsAutoCalculatedPrice = true,
            AdminCommissionPercent = 15.0m,
            IsActive = true,
            Privileges = new List<PlanPrivilegeDto>
            {
                new PlanPrivilegeDto
                {
                    PrivilegeId = MasterData.Privileges.First(p => p.Name == "Video Consultation").Id,
                    Value = 10, // 10 consultations
                    PrivilegeBaseCost = 25.00m,
                    UnitCost = 30.00m
                },
                new PlanPrivilegeDto
                {
                    PrivilegeId = MasterData.Privileges.First(p => p.Name == "Prescription Management").Id,
                    Value = 5, // 5 prescriptions
                    PrivilegeBaseCost = 15.00m,
                    UnitCost = 20.00m
                },
                new PlanPrivilegeDto
                {
                    PrivilegeId = MasterData.Privileges.First(p => p.Name == "Text Messaging").Id,
                    Value = -1, // Unlimited
                    PrivilegeBaseCost = 5.00m,
                    UnitCost = 0.50m
                }
            }
        };

        var planResult = await _planService.CreatePlanAsync(createPlanDto, adminToken);
        Assert.Equal(200, planResult.StatusCode);
        var plan = planResult.data as SubscriptionPlan;

        // Verify auto-calculated pricing
        // Expected: (10×25) + (5×15) + (1×5) = 250 + 75 + 5 = 330
        // Commission: 330 × 15% = 49.5
        // Total: 330 + 49.5 = 379.5
        Assert.Equal(379.50m, plan.BasePrice);
        Assert.Equal(330.00m, plan.PrivilegesTotalCost);

        // Step 2: User creates a subscription
        var user = await TestData.User().BuildAsync();
        var createSubscriptionDto = new CreateSubscriptionDto
        {
            PlanId = plan.Id.ToString(),
            PaymentMethodId = "pm_card_visa",
            UserId = user.Id
        };

        var subscriptionResult = await _subscriptionService.CreateSubscriptionAsync(createSubscriptionDto, userToken);
        Assert.Equal(200, subscriptionResult.StatusCode);
        var subscription = subscriptionResult.data as Subscription;

        // Verify subscription is created with correct pricing
        Assert.Equal(Subscription.SubscriptionStatuses.Active, subscription.Status);
        Assert.Equal(379.50m, subscription.CurrentPrice);

        // Step 3: Create initial billing record
        var billingResult = await _billingService.CreateBillingRecordAsync(subscription.Id, userToken);
        Assert.Equal(200, billingResult.StatusCode);
        var billingRecord = billingResult.data as BillingRecord;

        Assert.Equal(379.50m, billingRecord.Amount);
        Assert.Equal(BillingRecord.BillingStatus.Pending, billingRecord.Status);

        // Step 4: Process payment
        var paymentResult = await _billingService.ProcessPaymentAsync(billingRecord.Id, userToken);
        Assert.Equal(200, paymentResult.StatusCode);

        // Verify billing record is marked as paid
        var updatedBillingRecord = await _billingService.GetBillingRecordByIdAsync(billingRecord.Id, userToken);
        var paidRecord = updatedBillingRecord.data as BillingRecord;
        Assert.Equal(BillingRecord.BillingStatus.Paid, paidRecord.Status);

        // Step 5: Test renewal process
        var renewalResult = await _billingService.ProcessSubscriptionRenewalAsync(subscription.Id, userToken);
        Assert.Equal(200, renewalResult.StatusCode);

        var renewalData = renewalResult.data as SubscriptionRenewalResultDto;
        Assert.True(renewalData.IsSuccess);
        Assert.Equal(379.50m, renewalData.Amount);

        // Step 6: Verify subscription is renewed
        var renewedSubscription = await _subscriptionService.GetSubscriptionByIdAsync(subscription.Id.ToString(), userToken);
        var renewedSub = renewedSubscription.data as Subscription;
        Assert.True(renewedSub.NextBillingDate > subscription.NextBillingDate);
    }

    [Fact]
    public async Task PlanUpgradeWithProration_ShouldCalculateCorrectAmounts()
    {
        // Arrange
        var adminToken = CreateTestToken();
        var userToken = CreateTestToken();

        // Create basic plan
        var basicPlan = await TestData.SubscriptionPlan()
            .WithName("Basic Plan")
            .WithBasePrice(99.99m)
            .WithBillingCycle("monthly")
            .WithCurrency("USD")
            .WithCategory(MasterData.Categories.First().Id)
            .BuildAsync();

        // Create premium plan
        var premiumPlan = await TestData.SubscriptionPlan()
            .WithName("Premium Plan")
            .WithBasePrice(199.99m)
            .WithBillingCycle("monthly")
            .WithCurrency("USD")
            .WithCategory(MasterData.Categories.First().Id)
            .BuildAsync();

        // Create user and subscription
        var user = await TestData.User().BuildAsync();
        var subscription = await TestData.Subscription()
            .ForUser(user)
            .WithPlan(basicPlan)
            .WithBillingCycle("monthly")
            .BuildAsync();

        // Set subscription to be in the middle of billing cycle
        subscription.StartDate = DateTime.UtcNow.AddDays(-15); // 15 days ago
        subscription.NextBillingDate = DateTime.UtcNow.AddDays(15); // 15 days remaining
        await SaveChangesAsync();

        // Act: Upgrade to premium plan
        var upgradeRequest = new ChangePlanRequestDto
        {
            NewPlanId = premiumPlan.Id.ToString(),
            EffectiveDate = DateTime.UtcNow
        };

        var upgradeResult = await _lifecycleService.ChangePlanAsync(subscription.Id.ToString(), upgradeRequest, userToken);

        // Assert
        Assert.Equal(200, upgradeResult.StatusCode);
        var upgradeData = upgradeResult.data as SubscriptionDto;
        Assert.Equal(premiumPlan.Id, upgradeData.SubscriptionPlanId);

        // Verify proration calculation
        // Expected: (199.99 - 99.99) × (15/30) = 100 × 0.5 = 50
        // The upgrade should charge the prorated difference
        var updatedSubscription = await _subscriptionService.GetSubscriptionByIdAsync(subscription.Id.ToString(), userToken);
        var updatedSub = updatedSubscription.data as Subscription;
        Assert.Equal(premiumPlan.BasePrice, updatedSub.CurrentPrice);
    }

    [Fact]
    public async Task OverageBilling_ShouldChargeCorrectAmounts()
    {
        // Arrange
        var userToken = CreateTestToken();

        // Create plan with limited privileges
        var plan = await TestData.SubscriptionPlan()
            .WithName("Limited Plan")
            .WithBasePrice(99.99m)
            .WithBillingCycle("monthly")
            .WithCurrency("USD")
            .WithCategory(MasterData.Categories.First().Id)
            .BuildAsync();

        // Add privilege to plan
        var videoConsultation = MasterData.Privileges.First(p => p.Name == "Video Consultation");
        await TestData.SubscriptionPlanPrivilege()
            .ForPlan(plan)
            .WithPrivilege(videoConsultation)
            .WithValue(5) // 5 consultations included
            .WithBaseCost(25.00m)
            .WithUnitCost(30.00m) // $30 per overage consultation
            .BuildAsync();

        // Create user and subscription
        var user = await TestData.User().BuildAsync();
        var subscription = await TestData.Subscription()
            .ForUser(user)
            .WithPlan(plan)
            .WithBillingCycle("monthly")
            .BuildAsync();

        // Act: Create overage billing for 3 extra consultations
        var overageResult = await _billingService.CreateHealthcareOverageBillingAsync(
            subscription.Id,
            videoConsultation.Id,
            3, // 3 overage consultations
            userToken);

        // Assert
        Assert.Equal(200, overageResult.StatusCode);
        var overageBilling = overageResult.data as BillingRecord;

        // Expected: 3 consultations × $30 = $90
        Assert.Equal(90.00m, overageBilling.Amount);
        Assert.Equal(BillingRecord.BillingType.Overage, overageBilling.Type);
    }

    [Fact]
    public async Task FailedPaymentRetry_ShouldHandleCorrectly()
    {
        // Arrange
        var userToken = CreateTestToken();

        // Create subscription
        var (user, plan, subscription) = await TestData.CreateCompleteSubscriptionAsync();

        // Create billing record
        var billingResult = await _billingService.CreateBillingRecordAsync(subscription.Id, userToken);
        var billingRecord = billingResult.data as BillingRecord;

        // Mock Stripe service to return failed payment
        var mockStripeService = GetMockService<IStripeService>();
        mockStripeService.Setup(x => x.ProcessPaymentAsync(
            It.IsAny<string>(), 
            It.IsAny<decimal>(), 
            It.IsAny<string>(), 
            It.IsAny<TokenModel>()))
            .ReturnsAsync(new PaymentResultDto
            {
                Status = "failed",
                ErrorMessage = "Card declined",
                TransactionId = "txn_failed"
            });

        // Act: Process payment (should fail)
        var paymentResult = await _billingService.ProcessPaymentAsync(billingRecord.Id, userToken);
        Assert.Equal(400, paymentResult.StatusCode); // Payment failed

        // Verify billing record status
        var updatedBillingRecord = await _billingService.GetBillingRecordByIdAsync(billingRecord.Id, userToken);
        var failedRecord = updatedBillingRecord.data as BillingRecord;
        Assert.Equal(BillingRecord.BillingStatus.Failed, failedRecord.Status);

        // Act: Retry payment with successful result
        mockStripeService.Setup(x => x.ProcessPaymentAsync(
            It.IsAny<string>(), 
            It.IsAny<decimal>(), 
            It.IsAny<string>(), 
            It.IsAny<TokenModel>()))
            .ReturnsAsync(new PaymentResultDto
            {
                Status = "succeeded",
                TransactionId = "txn_success"
            });

        var retryResult = await _billingService.ProcessPaymentAsync(billingRecord.Id, userToken);
        Assert.Equal(200, retryResult.StatusCode); // Payment succeeded

        // Verify billing record is now paid
        var finalBillingRecord = await _billingService.GetBillingRecordByIdAsync(billingRecord.Id, userToken);
        var paidRecord = finalBillingRecord.data as BillingRecord;
        Assert.Equal(BillingRecord.BillingStatus.Paid, paidRecord.Status);
    }

    [Fact]
    public async Task SubscriptionPauseAndResume_ShouldHandleBillingCorrectly()
    {
        // Arrange
        var userToken = CreateTestToken();
        var (user, plan, subscription) = await TestData.CreateCompleteSubscriptionAsync();

        // Act: Pause subscription
        var pauseResult = await _lifecycleService.PauseSubscriptionAsync(subscription.Id.ToString(), userToken);
        Assert.Equal(200, pauseResult.StatusCode);

        // Verify subscription is paused
        var pausedSubscription = await _subscriptionService.GetSubscriptionByIdAsync(subscription.Id.ToString(), userToken);
        var pausedSub = pausedSubscription.data as Subscription;
        Assert.Equal(Subscription.SubscriptionStatuses.Paused, pausedSub.Status);

        // Act: Resume subscription
        var resumeResult = await _lifecycleService.ResumeSubscriptionAsync(subscription.Id.ToString(), userToken);
        Assert.Equal(200, resumeResult.StatusCode);

        // Verify subscription is active again
        var resumedSubscription = await _subscriptionService.GetSubscriptionByIdAsync(subscription.Id.ToString(), userToken);
        var resumedSub = resumedSubscription.data as Subscription;
        Assert.Equal(Subscription.SubscriptionStatuses.Active, resumedSub.Status);
    }

    [Fact]
    public async Task SubscriptionCancellation_ShouldHandleCorrectly()
    {
        // Arrange
        var userToken = CreateTestToken();
        var (user, plan, subscription) = await TestData.CreateCompleteSubscriptionAsync();

        // Act: Cancel subscription
        var cancelResult = await _lifecycleService.CancelSubscriptionAsync(subscription.Id.ToString(), userToken);
        Assert.Equal(200, cancelResult.StatusCode);

        // Verify subscription is cancelled
        var cancelledSubscription = await _subscriptionService.GetSubscriptionByIdAsync(subscription.Id.ToString(), userToken);
        var cancelledSub = cancelledSubscription.data as Subscription;
        Assert.Equal(Subscription.SubscriptionStatuses.Cancelled, cancelledSub.Status);
        Assert.NotNull(cancelledSub.EndDate);
    }

    [Fact]
    public async Task BillingAdjustment_ShouldApplyCorrectly()
    {
        // Arrange
        var userToken = CreateTestToken();
        var (user, plan, subscription) = await TestData.CreateCompleteSubscriptionAsync();

        // Create billing record
        var billingResult = await _billingService.CreateBillingRecordAsync(subscription.Id, userToken);
        var billingRecord = billingResult.data as BillingRecord;

        // Act: Apply 10% discount adjustment
        var adjustmentDto = new CreateBillingAdjustmentDto
        {
            Type = BillingAdjustment.AdjustmentType.Discount,
            IsPercentage = true,
            Percentage = 10,
            Description = "10% promotional discount",
            Reason = "Customer loyalty",
            IsApproved = true
        };

        var adjustmentResult = await _billingService.ApplyBillingAdjustmentAsync(billingRecord.Id, adjustmentDto, userToken);
        Assert.Equal(200, adjustmentResult.StatusCode);

        // Verify adjustment is applied
        var updatedBillingRecord = await _billingService.GetBillingRecordByIdAsync(billingRecord.Id, userToken);
        var updatedRecord = updatedBillingRecord.data as BillingRecord;

        // Expected: Original amount - (Original amount × 10%) = 99.99 - 9.999 = 89.991
        var expectedAmount = billingRecord.Amount * 0.9m;
        Assert.Equal(expectedAmount, updatedRecord.TotalAmount);
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public async Task ZeroAmountBilling_ShouldHandleCorrectly()
    {
        // Arrange
        var userToken = CreateTestToken();

        // Create plan with zero base price
        var plan = await TestData.SubscriptionPlan()
            .WithName("Free Plan")
            .WithBasePrice(0m)
            .WithBillingCycle("monthly")
            .WithCurrency("USD")
            .WithCategory(MasterData.Categories.First().Id)
            .BuildAsync();

        var (user, _, subscription) = await TestData.CreateCompleteSubscriptionAsync(
            planName: "Free Plan",
            basePrice: 0m,
            billingCycle: "monthly");

        // Act: Create billing record
        var billingResult = await _billingService.CreateBillingRecordAsync(subscription.Id, userToken);
        Assert.Equal(200, billingResult.StatusCode);

        var billingRecord = billingResult.data as BillingRecord;
        Assert.Equal(0.01m, billingRecord.Amount); // Should be minimum amount
    }

    [Fact]
    public async Task ExcessiveDiscount_ShouldBeCapped()
    {
        // Arrange
        var userToken = CreateTestToken();
        var (user, plan, subscription) = await TestData.CreateCompleteSubscriptionAsync();

        // Create billing record
        var billingResult = await _billingService.CreateBillingRecordAsync(subscription.Id, userToken);
        var billingRecord = billingResult.data as BillingRecord;

        // Act: Apply 150% discount (should be capped)
        var adjustmentDto = new CreateBillingAdjustmentDto
        {
            Type = BillingAdjustment.AdjustmentType.Discount,
            IsPercentage = true,
            Percentage = 150, // Excessive discount
            Description = "Excessive discount test",
            Reason = "Test case",
            IsApproved = true
        };

        var adjustmentResult = await _billingService.ApplyBillingAdjustmentAsync(billingRecord.Id, adjustmentDto, userToken);
        Assert.Equal(200, adjustmentResult.StatusCode);

        // Verify discount is capped
        var updatedBillingRecord = await _billingService.GetBillingRecordByIdAsync(billingRecord.Id, userToken);
        var updatedRecord = updatedBillingRecord.data as BillingRecord;

        // Should be capped at 50% discount (0.01m minimum)
        var expectedAmount = Math.Max(billingRecord.Amount * 0.5m, 0.01m);
        Assert.Equal(expectedAmount, updatedRecord.TotalAmount);
    }

    #endregion
}
