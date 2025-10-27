using Microsoft.Extensions.Logging;
using Moq;
using SmartTelehealth.Application.Services;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Tests.Infrastructure;
using Xunit;

namespace SmartTelehealth.Tests.Services;

/// <summary>
/// Comprehensive integration tests for billing services using real implementations
/// with mocked third-party services (Stripe, Email, SMS, etc.)
/// </summary>
public class BillingServiceTests : TestBase
{
    private readonly ISubscriptionBillingService _billingService;
    private readonly IAutomatedBillingService _automatedBillingService;
    private readonly ISubscriptionLifecycleService _lifecycleService;
    private readonly ISubscriptionPlanService _planService;

    public BillingServiceTests()
    {
        _billingService = GetService<ISubscriptionBillingService>();
        _automatedBillingService = GetService<IAutomatedBillingService>();
        _lifecycleService = GetService<ISubscriptionLifecycleService>();
        _planService = GetService<ISubscriptionPlanService>();
    }

    #region Subscription Billing Service Tests

    [Fact]
    public async Task CreateBillingRecordAsync_ShouldCreateValidBillingRecord()
    {
        // Arrange
        var (user, plan, subscription) = await TestData.CreateCompleteSubscriptionAsync();
        var tokenModel = CreateTestToken(user.Id);

        // Act
        var result = await _billingService.CreateBillingRecordAsync(subscription.Id, tokenModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.data);

        var billingRecord = result.data as BillingRecord;
        Assert.NotNull(billingRecord);
        Assert.Equal(subscription.Id, billingRecord.SubscriptionId);
        Assert.Equal(plan.BasePrice, billingRecord.Amount);
        Assert.Equal(BillingRecord.BillingStatus.Pending, billingRecord.Status);
    }

    [Fact]
    public async Task CreateBillingRecordAsync_ShouldUseEffectivePriceForDiscountedPlan()
    {
        // Arrange
        var (user, plan, subscription) = await TestData.CreateCompleteSubscriptionAsync();
        
        // Update plan with discount percentage
        plan.DiscountPercentage = 20.0m; // 20% discount
        plan.DiscountValidUntil = DateTime.UtcNow.AddDays(30);
        await SaveChangesAsync();

        var tokenModel = CreateTestToken(user.Id);

        // Act
        var result = await _billingService.CreateBillingRecordAsync(subscription.Id, tokenModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);

        var billingRecord = result.data as BillingRecord;
        Assert.NotNull(billingRecord);
        
        // Expected: Base price with 20% discount applied
        var expectedAmount = plan.BasePrice * 0.8m; // 20% discount
        Assert.Equal(expectedAmount, billingRecord.Amount);
    }

    [Fact]
    public async Task ApplyBillingAdjustmentAsync_ShouldApplyDiscountCorrectly()
    {
        // Arrange
        var (user, plan, subscription) = await TestData.CreateCompleteSubscriptionAsync();
        var tokenModel = CreateTestToken(user.Id);

        // Create billing record
        var billingResult = await _billingService.CreateBillingRecordAsync(subscription.Id, tokenModel);
        var billingRecord = billingResult.data as BillingRecord;

        var adjustmentDto = new CreateBillingAdjustmentDto
        {
            Type = BillingAdjustment.AdjustmentType.Discount,
            Amount = 10.00m,
            Description = "Test discount",
            Reason = "Promotional discount",
            IsPercentage = false,
            IsApproved = true
        };

        // Act
        var result = await _billingService.ApplyBillingAdjustmentAsync(billingRecord.Id, adjustmentDto, tokenModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);

        // Verify billing record was updated
        var updatedRecord = await _billingService.GetBillingRecordByIdAsync(billingRecord.Id, tokenModel);
        Assert.NotNull(updatedRecord.data);
        
        var updatedBillingRecord = updatedRecord.data as BillingRecord;
        Assert.Equal(plan.BasePrice - 10.00m, updatedBillingRecord.TotalAmount);
    }

    [Fact]
    public async Task ApplyBillingAdjustmentAsync_ShouldApplyPercentageDiscountCorrectly()
    {
        // Arrange
        var (user, plan, subscription) = await TestData.CreateCompleteSubscriptionAsync();
        var tokenModel = CreateTestToken(user.Id);

        // Create billing record
        var billingResult = await _billingService.CreateBillingRecordAsync(subscription.Id, tokenModel);
        var billingRecord = billingResult.data as BillingRecord;

        var adjustmentDto = new CreateBillingAdjustmentDto
        {
            Type = BillingAdjustment.AdjustmentType.Discount,
            Percentage = 15.0m, // 15% discount
            Description = "Percentage discount",
            Reason = "Promotional discount",
            IsPercentage = true,
            IsApproved = true
        };

        // Act
        var result = await _billingService.ApplyBillingAdjustmentAsync(billingRecord.Id, adjustmentDto, tokenModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);

        // Verify billing record was updated
        var updatedRecord = await _billingService.GetBillingRecordByIdAsync(billingRecord.Id, tokenModel);
        Assert.NotNull(updatedRecord.data);
        
        var updatedBillingRecord = updatedRecord.data as BillingRecord;
        var expectedAmount = plan.BasePrice - (plan.BasePrice * 0.15m);
        Assert.Equal(expectedAmount, updatedBillingRecord.TotalAmount);
    }

    [Fact]
    public async Task ApplyBillingAdjustmentAsync_ShouldPreventNegativeAmounts()
    {
        // Arrange
        var (user, plan, subscription) = await TestData.CreateCompleteSubscriptionAsync();
        var tokenModel = CreateTestToken(user.Id);

        // Create billing record
        var billingResult = await _billingService.CreateBillingRecordAsync(subscription.Id, tokenModel);
        var billingRecord = billingResult.data as BillingRecord;

        var adjustmentDto = new CreateBillingAdjustmentDto
        {
            Type = BillingAdjustment.AdjustmentType.Discount,
            Amount = plan.BasePrice + 100.00m, // Discount larger than billing amount
            Description = "Excessive discount",
            Reason = "Test excessive discount",
            IsPercentage = false,
            IsApproved = true
        };

        // Act
        var result = await _billingService.ApplyBillingAdjustmentAsync(billingRecord.Id, adjustmentDto, tokenModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);

        // Verify billing record was updated with minimum amount
        var updatedRecord = await _billingService.GetBillingRecordByIdAsync(billingRecord.Id, tokenModel);
        Assert.NotNull(updatedRecord.data);
        
        var updatedBillingRecord = updatedRecord.data as BillingRecord;
        Assert.Equal(0.01m, updatedBillingRecord.TotalAmount); // Minimum amount
    }

    [Fact]
    public async Task GetSubscriptionBillingHistoryAsync_ShouldReturnBillingHistory()
    {
        // Arrange
        var (user, plan, subscription) = await TestData.CreateCompleteSubscriptionAsync();
        var tokenModel = CreateTestToken(user.Id);

        // Create multiple billing records
        await _billingService.CreateBillingRecordAsync(subscription.Id, tokenModel);
        await _billingService.CreateBillingRecordAsync(subscription.Id, tokenModel);

        // Act
        var result = await _billingService.GetSubscriptionBillingHistoryAsync(subscription.Id, tokenModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.data);

        var billingHistory = result.data as IEnumerable<BillingRecord>;
        Assert.NotNull(billingHistory);
        Assert.Equal(2, billingHistory.Count());
    }

    #endregion

    #region Automated Billing Service Tests

    [Fact]
    public async Task ProcessSubscriptionRenewalAsync_ShouldProcessRenewalSuccessfully()
    {
        // Arrange
        var (user, plan, subscription) = await TestData.CreateCompleteSubscriptionAsync();
        var tokenModel = CreateTestToken(user.Id);

        // Set subscription to be due for renewal
        subscription.NextBillingDate = DateTime.UtcNow.AddDays(-1);
        await SaveChangesAsync();

        // Act
        var result = await _automatedBillingService.ProcessSubscriptionRenewalAsync(subscription.Id, tokenModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.data);

        var renewalResult = result.data as SubscriptionRenewalResultDto;
        Assert.NotNull(renewalResult);
        Assert.True(renewalResult.IsSuccess);
        Assert.Equal(plan.BasePrice, renewalResult.Amount);
    }

    [Fact]
    public async Task ProcessSubscriptionRenewalAsync_ShouldUseEffectivePriceForDiscountedPlan()
    {
        // Arrange
        var (user, plan, subscription) = await TestData.CreateCompleteSubscriptionAsync();
        var discountedPrice = plan.BasePrice * 0.8m; // 20% discount
        
        // Update plan with discount
        plan.DiscountPercentage = 20.0m; // 20% discount
        plan.DiscountValidUntil = DateTime.UtcNow.AddDays(30);
        await SaveChangesAsync();

        var tokenModel = CreateTestToken(user.Id);

        // Set subscription to be due for renewal
        subscription.NextBillingDate = DateTime.UtcNow.AddDays(-1);
        await SaveChangesAsync();

        // Act
        var result = await _automatedBillingService.ProcessSubscriptionRenewalAsync(subscription.Id, tokenModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);

        var renewalResult = result.data as SubscriptionRenewalResultDto;
        Assert.NotNull(renewalResult);
        Assert.True(renewalResult.IsSuccess);
        Assert.Equal(discountedPrice, renewalResult.Amount);
    }

    [Fact]
    public async Task ProcessSubscriptionRenewalAsync_ShouldHandleFailedPayment()
    {
        // Arrange
        var (user, plan, subscription) = await TestData.CreateCompleteSubscriptionAsync();
        var tokenModel = CreateTestToken(user.Id);

        // Set subscription to be due for renewal
        subscription.NextBillingDate = DateTime.UtcNow.AddDays(-1);
        await SaveChangesAsync();

        // Mock Stripe service to return failed payment
        var mockStripeService = GetMockService<IStripeService>();
        mockStripeService.Setup(x => x.ProcessPaymentAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<TokenModel>()))
            .ReturnsAsync(new PaymentResultDto
            {
                Status = "failed",
                TransactionId = null,
                Amount = 0,
                Currency = "usd",
                ErrorMessage = "Card declined"
            });

        // Act
        var result = await _automatedBillingService.ProcessSubscriptionRenewalAsync(subscription.Id, tokenModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);

        var renewalResult = result.data as SubscriptionRenewalResultDto;
        Assert.NotNull(renewalResult);
        Assert.False(renewalResult.IsSuccess);
        Assert.Contains("Card declined", renewalResult.ErrorMessage);
    }

    [Fact]
    public async Task ProcessSubscriptionRenewalAsync_ShouldUpdateNextBillingDate()
    {
        // Arrange
        var (user, plan, subscription) = await TestData.CreateCompleteSubscriptionAsync();
        var tokenModel = CreateTestToken(user.Id);

        var originalNextBillingDate = subscription.NextBillingDate;
        subscription.NextBillingDate = DateTime.UtcNow.AddDays(-1);
        await SaveChangesAsync();

        // Act
        var result = await _automatedBillingService.ProcessSubscriptionRenewalAsync(subscription.Id, tokenModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);

        // Verify next billing date was updated
        var updatedSubscription = await _lifecycleService.GetSubscriptionByIdAsync(subscription.Id, tokenModel);
        Assert.NotNull(updatedSubscription.data);
        
        var subscriptionData = updatedSubscription.data as Subscription;
        Assert.True(subscriptionData.NextBillingDate > originalNextBillingDate);
    }

    #endregion

    #region Subscription Lifecycle Service Tests

    [Fact]
    public async Task CreateSubscriptionAsync_ShouldCreateSubscriptionWithEffectivePrice()
    {
        // Arrange
        var user = await TestData.User().BuildAsync();
        var plan = await TestData.SubscriptionPlan()
            .WithPrice(100.00m)
            .WithDiscountedPrice(80.00m, DateTime.UtcNow.AddDays(30))
            .WithBillingCycle("monthly")
            .WithCurrency("USD")
            .BuildAsync();

        var createDto = new CreateSubscriptionDto
        {
            UserId = user.Id,
            SubscriptionPlanId = plan.Id,
            PaymentMethodId = "pm_test_payment_method"
        };

        var tokenModel = CreateTestToken(user.Id);

        // Act
        var result = await _lifecycleService.CreateSubscriptionAsync(createDto, tokenModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.data);

        var subscription = result.data as Subscription;
        Assert.NotNull(subscription);
        Assert.Equal(user.Id, subscription.UserId);
        Assert.Equal(plan.Id, subscription.SubscriptionPlanId);
        Assert.Equal(80.00m, subscription.CurrentPrice); // Should use discounted price
    }

    [Fact]
    public async Task CreateSubscriptionAsync_ShouldCreateStripeSubscription()
    {
        // Arrange
        var user = await TestData.User().BuildAsync();
        var plan = await TestData.SubscriptionPlan()
            .WithPrice(100.00m)
            .WithBillingCycle("monthly")
            .WithCurrency("USD")
            .WithStripeIntegration("prod_test", "price_test")
            .BuildAsync();

        var createDto = new CreateSubscriptionDto
        {
            UserId = user.Id,
            SubscriptionPlanId = plan.Id,
            PaymentMethodId = "pm_test_payment_method"
        };

        var tokenModel = CreateTestToken(user.Id);

        // Act
        var result = await _lifecycleService.CreateSubscriptionAsync(createDto, tokenModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);

        var subscription = result.data as Subscription;
        Assert.NotNull(subscription);
        Assert.NotNull(subscription.StripeCustomerId);
        Assert.NotNull(subscription.StripeSubscriptionId);
        Assert.NotNull(subscription.StripePriceId);

        // Verify Stripe service was called
        var mockStripeService = GetMockService<IStripeService>();
        mockStripeService.Verify(x => x.CreateCustomerAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TokenModel>()), Times.Once);
        mockStripeService.Verify(x => x.CreateSubscriptionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TokenModel>()), Times.Once);
    }

    [Fact]
    public async Task CancelSubscriptionAsync_ShouldCancelSubscriptionSuccessfully()
    {
        // Arrange
        var (user, plan, subscription) = await TestData.CreateCompleteSubscriptionAsync();
        var tokenModel = CreateTestToken(user.Id);

        // Act
        var result = await _lifecycleService.CancelSubscriptionAsync(subscription.Id, tokenModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);

        // Verify subscription was cancelled
        var updatedSubscription = await _lifecycleService.GetSubscriptionByIdAsync(subscription.Id, tokenModel);
        Assert.NotNull(updatedSubscription.data);
        
        var subscriptionData = updatedSubscription.data as Subscription;
        Assert.Equal(Subscription.SubscriptionStatuses.Cancelled, subscriptionData.Status);
    }

    #endregion

    #region Subscription Plan Service Tests

    [Fact]
    public async Task CreateSubscriptionPlanAsync_ShouldCreatePlanWithValidation()
    {
        // Arrange
        var createDto = new CreateSubscriptionPlanDto
        {
            Name = "Test Premium Plan",
            Description = "Premium subscription plan for testing",
            Price = 199.99m,
            BillingCycleId = (await TestData.GetRandomBillingCycle()).Id,
            CurrencyId = (await TestData.GetRandomCurrency()).Id,
            IsTrialAllowed = true,
            TrialDurationInDays = 14
        };

        var tokenModel = CreateTestToken();

        // Act
        var result = await _planService.CreateSubscriptionPlanAsync(createDto, tokenModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.data);

        var plan = result.data as SubscriptionPlan;
        Assert.NotNull(plan);
        Assert.Equal(createDto.Name, plan.Name);
        Assert.Equal(createDto.BasePrice, plan.BasePrice);
        Assert.Equal(createDto.TrialDurationInDays, plan.TrialDurationInDays);
    }

    [Fact]
    public async Task CreateSubscriptionPlanAsync_ShouldValidateDiscountData()
    {
        // Arrange
        var createDto = new CreateSubscriptionPlanDto
        {
            Name = "Test Plan with Invalid Discount",
            Description = "Plan with invalid discount",
            Price = 100.00m,
            DiscountedPrice = 150.00m, // Invalid: discount price > base price
            DiscountValidUntil = DateTime.UtcNow.AddDays(30),
            BillingCycleId = (await TestData.GetRandomBillingCycle()).Id,
            CurrencyId = (await TestData.GetRandomCurrency()).Id
        };

        var tokenModel = CreateTestToken();

        // Act
        var result = await _planService.CreateSubscriptionPlanAsync(createDto, tokenModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("Discounted price must be less than the base price", result.Message);
    }

    [Fact]
    public async Task CreateSubscriptionPlanAsync_ShouldCreateStripeProductAndPrice()
    {
        // Arrange
        var createDto = new CreateSubscriptionPlanDto
        {
            Name = "Test Stripe Plan",
            Description = "Plan with Stripe integration",
            Price = 99.99m,
            BillingCycleId = (await TestData.GetRandomBillingCycle()).Id,
            CurrencyId = (await TestData.GetRandomCurrency()).Id
        };

        var tokenModel = CreateTestToken();

        // Act
        var result = await _planService.CreateSubscriptionPlanAsync(createDto, tokenModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);

        var plan = result.data as SubscriptionPlan;
        Assert.NotNull(plan);
        Assert.NotNull(plan.StripeProductId);
        Assert.NotNull(plan.StripePriceId);

        // Verify Stripe service was called
        var mockStripeService = GetMockService<IStripeService>();
        mockStripeService.Verify(x => x.CreateProductAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TokenModel>()), Times.Once);
        mockStripeService.Verify(x => x.CreatePriceAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<TokenModel>()), Times.Once);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task CompleteBillingFlow_ShouldWorkEndToEnd()
    {
        // Arrange
        var (user, plan, subscription) = await TestData.CreateCompleteSubscriptionAsync();
        var tokenModel = CreateTestToken(user.Id);

        // Step 1: Create billing record
        var billingResult = await _billingService.CreateBillingRecordAsync(subscription.Id, tokenModel);
        Assert.Equal(200, billingResult.StatusCode);
        var billingRecord = billingResult.data as BillingRecord;

        // Step 2: Apply discount adjustment
        var adjustmentDto = new CreateBillingAdjustmentDto
        {
            Type = BillingAdjustment.AdjustmentType.Discount,
            Amount = 10.00m,
            Description = "Promotional discount",
            Reason = "New customer discount",
            IsPercentage = false,
            IsApproved = true
        };

        var adjustmentResult = await _billingService.ApplyBillingAdjustmentAsync(billingRecord.Id, adjustmentDto, tokenModel);
        Assert.Equal(200, adjustmentResult.StatusCode);

        // Step 3: Process payment
        var paymentResult = await _billingService.ProcessPaymentAsync(billingRecord.Id, tokenModel);
        Assert.Equal(200, paymentResult.StatusCode);

        // Step 4: Verify billing record is paid
        var updatedRecord = await _billingService.GetBillingRecordByIdAsync(billingRecord.Id, tokenModel);
        var paidRecord = updatedRecord.data as BillingRecord;
        Assert.Equal(BillingRecord.BillingStatus.Paid, paidRecord.Status);

        // Step 5: Process renewal
        subscription.NextBillingDate = DateTime.UtcNow.AddDays(-1);
        await SaveChangesAsync();

        var renewalResult = await _automatedBillingService.ProcessSubscriptionRenewalAsync(subscription.Id, tokenModel);
        Assert.Equal(200, renewalResult.StatusCode);

        var renewalData = renewalResult.data as SubscriptionRenewalResultDto;
        Assert.True(renewalData.IsSuccess);
    }

    #endregion
}

