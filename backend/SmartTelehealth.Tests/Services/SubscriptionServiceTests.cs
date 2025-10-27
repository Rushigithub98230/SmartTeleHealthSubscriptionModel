using Microsoft.Extensions.Logging;
using Moq;
using SmartTelehealth.Application.Services;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Tests.Infrastructure;
using Xunit;

namespace SmartTelehealth.Tests.Services;

/// <summary>
/// Comprehensive integration tests for subscription services using real implementations
/// with mocked third-party services
/// </summary>
public class SubscriptionServiceTests : TestBase
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly ISubscriptionLifecycleService _lifecycleService;
    private readonly ISubscriptionPlanService _planService;
    private readonly IPrivilegeService _privilegeService;

    public SubscriptionServiceTests()
    {
        _subscriptionService = GetService<ISubscriptionService>();
        _lifecycleService = GetService<ISubscriptionLifecycleService>();
        _planService = GetService<ISubscriptionPlanService>();
        _privilegeService = GetService<IPrivilegeService>();
    }

    #region Subscription Service Tests

    [Fact]
    public async Task GetSubscriptionsByUserIdAsync_ShouldReturnUserSubscriptions()
    {
        // Arrange
        var user = await TestData.User().BuildAsync();
        var plan = await TestData.SubscriptionPlan()
            .WithName("Test Plan")
            .WithPrice(99.99m)
            .WithBillingCycle("monthly")
            .WithCurrency("USD")
            .BuildAsync();

        var subscription = await TestData.Subscription()
            .ForUser(user)
            .WithPlan(plan)
            .WithBillingCycle("monthly")
            .BuildAsync();

        var tokenModel = CreateTestToken(user.Id);

        // Act
        var result = await _subscriptionService.GetSubscriptionsByUserIdAsync(user.Id, tokenModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.data);

        var subscriptions = result.data as IEnumerable<Subscription>;
        Assert.NotNull(subscriptions);
        Assert.Single(subscriptions);
        Assert.Equal(subscription.Id, subscriptions.First().Id);
    }

    [Fact]
    public async Task GetSubscriptionByIdAsync_ShouldReturnSubscription()
    {
        // Arrange
        var (user, plan, subscription) = await TestData.CreateCompleteSubscriptionAsync();
        var tokenModel = CreateTestToken(user.Id);

        // Act
        var result = await _subscriptionService.GetSubscriptionByIdAsync(subscription.Id, tokenModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.data);

        var subscriptionData = result.data as Subscription;
        Assert.NotNull(subscriptionData);
        Assert.Equal(subscription.Id, subscriptionData.Id);
        Assert.Equal(user.Id, subscriptionData.UserId);
        Assert.Equal(plan.Id, subscriptionData.SubscriptionPlanId);
    }

    [Fact]
    public async Task GetSubscriptionByIdAsync_ShouldReturnNotFoundForInvalidId()
    {
        // Arrange
        var invalidId = Guid.NewGuid();
        var tokenModel = CreateTestToken();

        // Act
        var result = await _subscriptionService.GetSubscriptionByIdAsync(invalidId, tokenModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("not found", result.Message);
    }

    [Fact]
    public async Task UpdateSubscriptionAsync_ShouldUpdateSubscription()
    {
        // Arrange
        var (user, plan, subscription) = await TestData.CreateCompleteSubscriptionAsync();
        var tokenModel = CreateTestToken(user.Id);

        var updateDto = new UpdateSubscriptionDto
        {
            Id = subscription.Id,
            Status = Subscription.SubscriptionStatuses.Paused,
            Notes = "Updated subscription"
        };

        // Act
        var result = await _subscriptionService.UpdateSubscriptionAsync(updateDto, tokenModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);

        // Verify subscription was updated
        var updatedSubscription = await _subscriptionService.GetSubscriptionByIdAsync(subscription.Id, tokenModel);
        var subscriptionData = updatedSubscription.data as Subscription;
        Assert.Equal(Subscription.SubscriptionStatuses.Paused, subscriptionData.Status);
    }

    #endregion

    #region Subscription Lifecycle Service Tests

    [Fact]
    public async Task CreateSubscriptionAsync_ShouldCreateSubscriptionWithTrial()
    {
        // Arrange
        var user = await TestData.User().BuildAsync();
        var plan = await TestData.SubscriptionPlan()
            .WithName("Trial Plan")
            .WithPrice(99.99m)
            .WithBillingCycle("monthly")
            .WithCurrency("USD")
            .WithTrial(14)
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
        Assert.True(subscription.IsTrialSubscription);
        Assert.Equal(Subscription.SubscriptionStatuses.TrialActive, subscription.Status);
        Assert.NotNull(subscription.TrialStartDate);
        Assert.NotNull(subscription.TrialEndDate);
        Assert.Equal(14, subscription.TrialDurationInDays);
    }

    [Fact]
    public async Task CreateSubscriptionAsync_ShouldCreateActiveSubscriptionWithoutTrial()
    {
        // Arrange
        var user = await TestData.User().BuildAsync();
        var plan = await TestData.SubscriptionPlan()
            .WithName("No Trial Plan")
            .WithPrice(99.99m)
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
        Assert.False(subscription.IsTrialSubscription);
        Assert.Equal(Subscription.SubscriptionStatuses.Active, subscription.Status);
    }

    [Fact]
    public async Task UpgradeSubscriptionAsync_ShouldUpgradeSubscription()
    {
        // Arrange
        var user = await TestData.User().BuildAsync();
        
        var basicPlan = await TestData.SubscriptionPlan()
            .WithName("Basic Plan")
            .WithPrice(49.99m)
            .WithBillingCycle("monthly")
            .WithCurrency("USD")
            .BuildAsync();

        var premiumPlan = await TestData.SubscriptionPlan()
            .WithName("Premium Plan")
            .WithPrice(99.99m)
            .WithBillingCycle("monthly")
            .WithCurrency("USD")
            .BuildAsync();

        var subscription = await TestData.Subscription()
            .ForUser(user)
            .WithPlan(basicPlan)
            .WithBillingCycle("monthly")
            .BuildAsync();

        var tokenModel = CreateTestToken(user.Id);

        // Act
        var result = await _lifecycleService.UpgradeSubscriptionAsync(subscription.Id, premiumPlan.Id, tokenModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);

        // Verify subscription was upgraded
        var updatedSubscription = await _subscriptionService.GetSubscriptionByIdAsync(subscription.Id, tokenModel);
        var subscriptionData = updatedSubscription.data as Subscription;
        Assert.Equal(premiumPlan.Id, subscriptionData.SubscriptionPlanId);
        Assert.Equal(premiumPlan.Price, subscriptionData.CurrentPrice);
    }

    [Fact]
    public async Task DowngradeSubscriptionAsync_ShouldDowngradeSubscription()
    {
        // Arrange
        var user = await TestData.User().BuildAsync();
        
        var basicPlan = await TestData.SubscriptionPlan()
            .WithName("Basic Plan")
            .WithPrice(49.99m)
            .WithBillingCycle("monthly")
            .WithCurrency("USD")
            .BuildAsync();

        var premiumPlan = await TestData.SubscriptionPlan()
            .WithName("Premium Plan")
            .WithPrice(99.99m)
            .WithBillingCycle("monthly")
            .WithCurrency("USD")
            .BuildAsync();

        var subscription = await TestData.Subscription()
            .ForUser(user)
            .WithPlan(premiumPlan)
            .WithBillingCycle("monthly")
            .BuildAsync();

        var tokenModel = CreateTestToken(user.Id);

        // Act
        var result = await _lifecycleService.DowngradeSubscriptionAsync(subscription.Id, basicPlan.Id, tokenModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);

        // Verify subscription was downgraded
        var updatedSubscription = await _subscriptionService.GetSubscriptionByIdAsync(subscription.Id, tokenModel);
        var subscriptionData = updatedSubscription.data as Subscription;
        Assert.Equal(basicPlan.Id, subscriptionData.SubscriptionPlanId);
        Assert.Equal(basicPlan.Price, subscriptionData.CurrentPrice);
    }

    [Fact]
    public async Task PauseSubscriptionAsync_ShouldPauseSubscription()
    {
        // Arrange
        var (user, plan, subscription) = await TestData.CreateCompleteSubscriptionAsync();
        var tokenModel = CreateTestToken(user.Id);

        // Act
        var result = await _lifecycleService.PauseSubscriptionAsync(subscription.Id, tokenModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);

        // Verify subscription was paused
        var updatedSubscription = await _subscriptionService.GetSubscriptionByIdAsync(subscription.Id, tokenModel);
        var subscriptionData = updatedSubscription.data as Subscription;
        Assert.Equal(Subscription.SubscriptionStatuses.Paused, subscriptionData.Status);
    }

    [Fact]
    public async Task ResumeSubscriptionAsync_ShouldResumeSubscription()
    {
        // Arrange
        var (user, plan, subscription) = await TestData.CreateCompleteSubscriptionAsync();
        var tokenModel = CreateTestToken(user.Id);

        // First pause the subscription
        await _lifecycleService.PauseSubscriptionAsync(subscription.Id, tokenModel);

        // Act
        var result = await _lifecycleService.ResumeSubscriptionAsync(subscription.Id, tokenModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);

        // Verify subscription was resumed
        var updatedSubscription = await _subscriptionService.GetSubscriptionByIdAsync(subscription.Id, tokenModel);
        var subscriptionData = updatedSubscription.data as Subscription;
        Assert.Equal(Subscription.SubscriptionStatuses.Active, subscriptionData.Status);
    }

    #endregion

    #region Subscription Plan Service Tests

    [Fact]
    public async Task GetSubscriptionPlansAsync_ShouldReturnAllActivePlans()
    {
        // Arrange
        var plan1 = await TestData.SubscriptionPlan()
            .WithName("Plan 1")
            .WithPrice(49.99m)
            .WithBillingCycle("monthly")
            .WithCurrency("USD")
            .BuildAsync();

        var plan2 = await TestData.SubscriptionPlan()
            .WithName("Plan 2")
            .WithPrice(99.99m)
            .WithBillingCycle("monthly")
            .WithCurrency("USD")
            .BuildAsync();

        var tokenModel = CreateTestToken();

        // Act
        var result = await _planService.GetSubscriptionPlansAsync(tokenModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.data);

        var plans = result.data as IEnumerable<SubscriptionPlan>;
        Assert.NotNull(plans);
        Assert.True(plans.Count() >= 2);
        Assert.Contains(plans, p => p.Id == plan1.Id);
        Assert.Contains(plans, p => p.Id == plan2.Id);
    }

    [Fact]
    public async Task GetSubscriptionPlanByIdAsync_ShouldReturnPlan()
    {
        // Arrange
        var plan = await TestData.SubscriptionPlan()
            .WithName("Test Plan")
            .WithPrice(99.99m)
            .WithBillingCycle("monthly")
            .WithCurrency("USD")
            .BuildAsync();

        var tokenModel = CreateTestToken();

        // Act
        var result = await _planService.GetSubscriptionPlanByIdAsync(plan.Id, tokenModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.data);

        var planData = result.data as SubscriptionPlan;
        Assert.NotNull(planData);
        Assert.Equal(plan.Id, planData.Id);
        Assert.Equal(plan.Name, planData.Name);
        Assert.Equal(plan.BasePrice, planData.BasePrice);
    }

    [Fact]
    public async Task UpdateSubscriptionPlanAsync_ShouldUpdatePlan()
    {
        // Arrange
        var plan = await TestData.SubscriptionPlan()
            .WithName("Original Plan")
            .WithPrice(99.99m)
            .WithBillingCycle("monthly")
            .WithCurrency("USD")
            .BuildAsync();

        var updateDto = new UpdateSubscriptionPlanDto
        {
            Id = plan.Id,
            Name = "Updated Plan",
            Description = "Updated description",
            Price = 149.99m
        };

        var tokenModel = CreateTestToken();

        // Act
        var result = await _planService.UpdateSubscriptionPlanAsync(updateDto, tokenModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);

        // Verify plan was updated
        var updatedPlan = await _planService.GetSubscriptionPlanByIdAsync(plan.Id, tokenModel);
        var planData = updatedPlan.data as SubscriptionPlan;
        Assert.Equal("Updated Plan", planData.Name);
        Assert.Equal("Updated description", planData.Description);
        Assert.Equal(149.99m, planData.Price);
    }

    [Fact]
    public async Task DeleteSubscriptionPlanAsync_ShouldSoftDeletePlan()
    {
        // Arrange
        var plan = await TestData.SubscriptionPlan()
            .WithName("Plan to Delete")
            .WithPrice(99.99m)
            .WithBillingCycle("monthly")
            .WithCurrency("USD")
            .BuildAsync();

        var tokenModel = CreateTestToken();

        // Act
        var result = await _planService.DeleteSubscriptionPlanAsync(plan.Id, tokenModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);

        // Verify plan was soft deleted
        var deletedPlan = await _planService.GetSubscriptionPlanByIdAsync(plan.Id, tokenModel);
        Assert.Equal(404, deletedPlan.StatusCode);
    }

    #endregion

    #region Privilege Service Tests

    [Fact]
    public async Task GetPrivilegesAsync_ShouldReturnAllActivePrivileges()
    {
        // Arrange
        var privilege1 = await TestData.Privilege()
            .WithName("Video Call")
            .WithDescription("Video call privilege")
            .WithBaseCost(10.00m)
            .WithType("Video Call")
            .BuildAsync();

        var privilege2 = await TestData.Privilege()
            .WithName("Message")
            .WithDescription("Message privilege")
            .WithBaseCost(5.00m)
            .WithType("Message")
            .BuildAsync();

        var tokenModel = CreateTestToken();

        // Act
        var result = await _privilegeService.GetPrivilegesAsync(tokenModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.data);

        var privileges = result.data as IEnumerable<Privilege>;
        Assert.NotNull(privileges);
        Assert.True(privileges.Count() >= 2);
        Assert.Contains(privileges, p => p.Id == privilege1.Id);
        Assert.Contains(privileges, p => p.Id == privilege2.Id);
    }

    [Fact]
    public async Task CreatePrivilegeAsync_ShouldCreatePrivilege()
    {
        // Arrange
        var createDto = new CreatePrivilegeDto
        {
            Name = "Test Privilege",
            Description = "Test privilege description",
            BaseCost = 15.00m,
            PrivilegeTypeId = (await TestData.GetRandomPrivilegeType()).Id
        };

        var tokenModel = CreateTestToken();

        // Act
        var result = await _privilegeService.CreatePrivilegeAsync(createDto, tokenModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.data);

        var privilege = result.data as Privilege;
        Assert.NotNull(privilege);
        Assert.Equal(createDto.Name, privilege.Name);
        Assert.Equal(createDto.Description, privilege.Description);
        Assert.Equal(createDto.BaseCost, privilege.BaseCost);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task CompleteSubscriptionLifecycle_ShouldWorkEndToEnd()
    {
        // Arrange
        var user = await TestData.User().BuildAsync();
        var tokenModel = CreateTestToken(user.Id);

        // Step 1: Create subscription plan
        var createPlanDto = new CreateSubscriptionPlanDto
        {
            Name = "Lifecycle Test Plan",
            Description = "Plan for testing complete lifecycle",
            Price = 99.99m,
            BillingCycleId = (await TestData.GetRandomBillingCycle()).Id,
            CurrencyId = (await TestData.GetRandomCurrency()).Id,
            IsTrialAllowed = true,
            TrialDurationInDays = 7
        };

        var planResult = await _planService.CreateSubscriptionPlanAsync(createPlanDto, tokenModel);
        Assert.Equal(200, planResult.StatusCode);
        var plan = planResult.data as SubscriptionPlan;

        // Step 2: Create subscription with trial
        var createSubscriptionDto = new CreateSubscriptionDto
        {
            UserId = user.Id,
            SubscriptionPlanId = plan.Id,
            PaymentMethodId = "pm_test_payment_method"
        };

        var subscriptionResult = await _lifecycleService.CreateSubscriptionAsync(createSubscriptionDto, tokenModel);
        Assert.Equal(200, subscriptionResult.StatusCode);
        var subscription = subscriptionResult.data as Subscription;

        // Step 3: Verify trial is active
        Assert.True(subscription.IsTrialSubscription);
        Assert.Equal(Subscription.SubscriptionStatuses.TrialActive, subscription.Status);

        // Step 4: Pause subscription
        var pauseResult = await _lifecycleService.PauseSubscriptionAsync(subscription.Id, tokenModel);
        Assert.Equal(200, pauseResult.StatusCode);

        // Step 5: Resume subscription
        var resumeResult = await _lifecycleService.ResumeSubscriptionAsync(subscription.Id, tokenModel);
        Assert.Equal(200, resumeResult.StatusCode);

        // Step 6: Upgrade subscription
        var upgradePlanDto = new CreateSubscriptionPlanDto
        {
            Name = "Upgraded Plan",
            Description = "Upgraded plan for testing",
            Price = 199.99m,
            BillingCycleId = (await TestData.GetRandomBillingCycle()).Id,
            CurrencyId = (await TestData.GetRandomCurrency()).Id
        };

        var upgradePlanResult = await _planService.CreateSubscriptionPlanAsync(upgradePlanDto, tokenModel);
        Assert.Equal(200, upgradePlanResult.StatusCode);
        var upgradePlan = upgradePlanResult.data as SubscriptionPlan;

        var upgradeResult = await _lifecycleService.UpgradeSubscriptionAsync(subscription.Id, upgradePlan.Id, tokenModel);
        Assert.Equal(200, upgradeResult.StatusCode);

        // Step 7: Cancel subscription
        var cancelResult = await _lifecycleService.CancelSubscriptionAsync(subscription.Id, tokenModel);
        Assert.Equal(200, cancelResult.StatusCode);

        // Step 8: Verify final state
        var finalSubscription = await _subscriptionService.GetSubscriptionByIdAsync(subscription.Id, tokenModel);
        var finalData = finalSubscription.data as Subscription;
        Assert.Equal(Subscription.SubscriptionStatuses.Cancelled, finalData.Status);
    }

    #endregion
}

