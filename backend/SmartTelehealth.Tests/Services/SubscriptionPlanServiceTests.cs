using Microsoft.Extensions.Logging;
using Moq;
using SmartTelehealth.Application.Services;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Tests.Infrastructure;
using Xunit;

namespace SmartTelehealth.Tests.Services;

/// <summary>
/// Comprehensive integration tests for SubscriptionPlanService using real implementations
/// with mocked third-party services and real SQL Server database.
/// Tests the updated pricing architecture with AdminCommissionPercent only.
/// </summary>
public class SubscriptionPlanServiceTests : TestBase
{
    private readonly ISubscriptionPlanService _planService;
    private readonly IPlanPricingService _pricingService;

    public SubscriptionPlanServiceTests()
    {
        _planService = GetService<ISubscriptionPlanService>();
        _pricingService = GetService<IPlanPricingService>();
    }

    #region Plan Creation Tests

    [Fact]
    public async Task CreatePlanAsync_ShouldCreatePlanWithManualPricing()
    {
        // Arrange
        var createDto = new CreateSubscriptionPlanDto
        {
            Name = "Manual Pricing Plan",
            Description = "Plan with manual pricing",
            BasePrice = 99.99m,
            BillingCycleId = MasterData.BillingCycles.First(bc => bc.Name == "monthly").Id,
            CurrencyId = MasterData.Currencies.First(c => c.Code == "USD").Id,
            CategoryId = MasterData.Categories.First().Id,
            IsAutoCalculatedPrice = false,
            AdminCommissionPercent = 15.0m,
            IsActive = true
        };

        var tokenModel = CreateTestToken();

        // Act
        var result = await _planService.CreatePlanAsync(createDto, tokenModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.data);

        var plan = result.data as SubscriptionPlan;
        Assert.NotNull(plan);
        Assert.Equal(createDto.Name, plan.Name);
        Assert.Equal(createDto.BasePrice, plan.BasePrice);
        Assert.Equal(createDto.AdminCommissionPercent, plan.AdminCommissionPercent);
        Assert.False(plan.IsAutoCalculatedPrice);
        Assert.NotNull(plan.StripeProductId);
        Assert.NotNull(plan.StripePriceId);

        // Verify Stripe service was called
        var mockStripeService = GetMockService<IStripeService>();
        mockStripeService.Verify(x => x.CreateProductAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TokenModel>()), Times.Once);
        mockStripeService.Verify(x => x.CreatePriceAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<TokenModel>()), Times.Once);
    }

    [Fact]
    public async Task CreatePlanAsync_ShouldCreatePlanWithAutoCalculatedPricing()
    {
        // Arrange
        var createDto = new CreateSubscriptionPlanDto
        {
            Name = "Auto Calculated Plan",
            Description = "Plan with auto-calculated pricing",
            BasePrice = 0m, // Will be calculated from privileges
            BillingCycleId = MasterData.BillingCycles.First(bc => bc.Name == "monthly").Id,
            CurrencyId = MasterData.Currencies.First(c => c.Code == "USD").Id,
            CategoryId = MasterData.Categories.First().Id,
            IsAutoCalculatedPrice = true,
            AdminCommissionPercent = 10.0m,
            IsActive = true,
            Privileges = new List<PlanPrivilegeDto>
            {
                new PlanPrivilegeDto
                {
                    PrivilegeId = MasterData.Privileges.First(p => p.Name == "Video Consultation").Id,
                    Value = 5, // 5 consultations
                    PrivilegeBaseCost = 25.00m,
                    UnitCost = 30.00m
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

        var tokenModel = CreateTestToken();

        // Act
        var result = await _planService.CreatePlanAsync(createDto, tokenModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.data);

        var plan = result.data as SubscriptionPlan;
        Assert.NotNull(plan);
        Assert.Equal(createDto.Name, plan.Name);
        Assert.True(plan.IsAutoCalculatedPrice);
        Assert.Equal(createDto.AdminCommissionPercent, plan.AdminCommissionPercent);

        // Verify auto-calculated pricing
        // Expected: (5 consultations × $25) + (1 unlimited messaging × $5) = $130
        // Commission: $130 × 10% = $13
        // Total: $130 + $13 = $143
        Assert.Equal(143.00m, plan.BasePrice);
        Assert.Equal(130.00m, plan.PrivilegesTotalCost);

        // Verify Stripe price was updated with calculated amount
        var mockStripeService = GetMockService<IStripeService>();
        mockStripeService.Verify(x => x.UpdatePriceWithNewPriceAsync(
            It.IsAny<string>(), It.IsAny<string>(), 143.00m, It.IsAny<string>(), 
            It.IsAny<string>(), It.IsAny<int>(), It.IsAny<TokenModel>()), Times.Once);
    }

    [Fact]
    public async Task CreatePlanAsync_ShouldValidateDiscountData()
    {
        // Arrange
        var createDto = new CreateSubscriptionPlanDto
        {
            Name = "Invalid Discount Plan",
            Description = "Plan with invalid discount",
            BasePrice = 100.00m,
            DiscountPercentage = 150.0m, // Invalid: > 100%
            DiscountValidUntil = DateTime.UtcNow.AddDays(30),
            BillingCycleId = MasterData.BillingCycles.First().Id,
            CurrencyId = MasterData.Currencies.First().Id,
            CategoryId = MasterData.Categories.First().Id
        };

        var tokenModel = CreateTestToken();

        // Act
        var result = await _planService.CreatePlanAsync(createDto, tokenModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("Discount percentage must be between 0 and 100%", result.Message);
    }

    [Fact]
    public async Task CreatePlanAsync_ShouldValidateCommissionPercentage()
    {
        // Arrange
        var createDto = new CreateSubscriptionPlanDto
        {
            Name = "Invalid Commission Plan",
            Description = "Plan with invalid commission",
            BasePrice = 100.00m,
            AdminCommissionPercent = 150.0m, // Invalid: > 100%
            BillingCycleId = MasterData.BillingCycles.First().Id,
            CurrencyId = MasterData.Currencies.First().Id,
            CategoryId = MasterData.Categories.First().Id
        };

        var tokenModel = CreateTestToken();

        // Act
        var result = await _planService.CreatePlanAsync(createDto, tokenModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("Commission must be between 0 and 100%", result.Message);
    }

    #endregion

    #region Plan Pricing Tests

    [Fact]
    public async Task CalculatePlanPriceAsync_ShouldCalculateManualPrice()
    {
        // Arrange
        var plan = await TestData.SubscriptionPlan()
            .WithName("Manual Plan")
            .WithBasePrice(99.99m)
            .WithAutoCalculatedPrice(false)
            .WithAdminCommissionPercent(10.0m)
            .BuildAsync();

        // Act
        var result = await _pricingService.CalculatePlanPriceAsync(plan.Id, useAutoCalculation: false);

        // Assert
        Assert.Equal(99.99m, result);
    }

    [Fact]
    public async Task CalculatePlanPriceAsync_ShouldCalculateAutoPriceFromPrivileges()
    {
        // Arrange
        var plan = await TestData.SubscriptionPlan()
            .WithName("Auto Plan")
            .WithBasePrice(0m)
            .WithAutoCalculatedPrice(true)
            .WithAdminCommissionPercent(10.0m)
            .BuildAsync();

        // Add privileges to the plan
        var videoConsultation = MasterData.Privileges.First(p => p.Name == "Video Consultation");
        var textMessaging = MasterData.Privileges.First(p => p.Name == "Text Messaging");

        await TestData.SubscriptionPlanPrivilege()
            .ForPlan(plan)
            .WithPrivilege(videoConsultation)
            .WithValue(5) // 5 consultations
            .WithBaseCost(25.00m)
            .BuildAsync();

        await TestData.SubscriptionPlanPrivilege()
            .ForPlan(plan)
            .WithPrivilege(textMessaging)
            .AsUnlimited() // -1 for unlimited
            .WithBaseCost(5.00m)
            .BuildAsync();

        // Act
        var result = await _pricingService.CalculatePlanPriceAsync(plan.Id, useAutoCalculation: true);

        // Assert
        // Expected: (5 consultations × $25) + (1 unlimited messaging × $5) = $130
        // Commission: $130 × 10% = $13
        // Total: $130 + $13 = $143
        Assert.Equal(143.00m, result);
    }

    [Fact]
    public async Task CalculatePlanPriceAsync_ShouldUseSystemDefaultCommission()
    {
        // Arrange
        var plan = await TestData.SubscriptionPlan()
            .WithName("Default Commission Plan")
            .WithBasePrice(0m)
            .WithAutoCalculatedPrice(true)
            .WithAdminCommissionPercent(null) // Use system default
            .BuildAsync();

        // Add privilege to the plan
        var videoConsultation = MasterData.Privileges.First(p => p.Name == "Video Consultation");
        await TestData.SubscriptionPlanPrivilege()
            .ForPlan(plan)
            .WithPrivilege(videoConsultation)
            .WithValue(4) // 4 consultations
            .WithBaseCost(25.00m)
            .BuildAsync();

        // Act
        var result = await _pricingService.CalculatePlanPriceAsync(plan.Id, useAutoCalculation: true);

        // Assert
        // Expected: (4 consultations × $25) = $100
        // Commission: $100 × 10% (system default) = $10
        // Total: $100 + $10 = $110
        Assert.Equal(110.00m, result);
    }

    [Fact]
    public async Task CalculatePricingBreakdownAsync_ShouldReturnDetailedBreakdown()
    {
        // Arrange
        var plan = await TestData.SubscriptionPlan()
            .WithName("Breakdown Plan")
            .WithBasePrice(0m)
            .WithAutoCalculatedPrice(true)
            .WithAdminCommissionPercent(15.0m)
            .BuildAsync();

        // Add privileges to the plan
        var videoConsultation = MasterData.Privileges.First(p => p.Name == "Video Consultation");
        var prescription = MasterData.Privileges.First(p => p.Name == "Prescription Management");

        await TestData.SubscriptionPlanPrivilege()
            .ForPlan(plan)
            .WithPrivilege(videoConsultation)
            .WithValue(3) // 3 consultations
            .WithBaseCost(25.00m)
            .BuildAsync();

        await TestData.SubscriptionPlanPrivilege()
            .ForPlan(plan)
            .WithPrivilege(prescription)
            .WithValue(2) // 2 prescriptions
            .WithBaseCost(15.00m)
            .BuildAsync();

        // Act
        var result = await _pricingService.GetPricingBreakdownAsync(plan.Id, CreateTestToken());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.data);

        var breakdown = result.data as PricingBreakdown;
        Assert.NotNull(breakdown);
        Assert.Equal(plan.Id, breakdown.PlanId);
        Assert.Equal(plan.Name, breakdown.PlanName);
        Assert.True(breakdown.IsAutoCalculated);
        Assert.Equal(105.00m, breakdown.PrivilegesTotalCost); // (3×25) + (2×15) = 75 + 30 = 105
        Assert.Equal(15.0m, breakdown.CommissionPercent);
        Assert.Equal(15.75m, breakdown.CommissionAmount); // 105 × 15% = 15.75
        Assert.Equal(120.75m, breakdown.BasePrice); // 105 + 15.75 = 120.75
        Assert.Equal(120.75m, breakdown.FinalPrice); // No discounts applied
        Assert.Equal(2, breakdown.PrivilegeBreakdown.Count);
    }

    #endregion

    #region Plan Management Tests

    [Fact]
    public async Task GetPlansAsync_ShouldReturnAllActivePlans()
    {
        // Arrange
        var plan1 = await TestData.SubscriptionPlan()
            .WithName("Plan 1")
            .WithBasePrice(49.99m)
            .BuildAsync();

        var plan2 = await TestData.SubscriptionPlan()
            .WithName("Plan 2")
            .WithBasePrice(99.99m)
            .BuildAsync();

        var tokenModel = CreateTestToken();

        // Act
        var result = await _planService.GetPlansAsync(tokenModel);

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
    public async Task GetPlanByIdAsync_ShouldReturnPlan()
    {
        // Arrange
        var plan = await TestData.SubscriptionPlan()
            .WithName("Test Plan")
            .WithBasePrice(99.99m)
            .WithAdminCommissionPercent(12.5m)
            .BuildAsync();

        var tokenModel = CreateTestToken();

        // Act
        var result = await _planService.GetPlanByIdAsync(plan.Id.ToString(), tokenModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.data);

        var planData = result.data as SubscriptionPlan;
        Assert.NotNull(planData);
        Assert.Equal(plan.Id, planData.Id);
        Assert.Equal(plan.Name, planData.Name);
        Assert.Equal(plan.BasePrice, planData.BasePrice);
        Assert.Equal(plan.AdminCommissionPercent, planData.AdminCommissionPercent);
    }

    [Fact]
    public async Task UpdatePlanAsync_ShouldUpdatePlan()
    {
        // Arrange
        var plan = await TestData.SubscriptionPlan()
            .WithName("Original Plan")
            .WithBasePrice(99.99m)
            .WithAdminCommissionPercent(10.0m)
            .BuildAsync();

        var updateDto = new UpdateSubscriptionPlanDto
        {
            Id = plan.Id,
            Name = "Updated Plan",
            Description = "Updated description",
            BasePrice = 149.99m,
            AdminCommissionPercent = 15.0m
        };

        var tokenModel = CreateTestToken();

        // Act
        var result = await _planService.UpdatePlanAsync(updateDto, tokenModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);

        // Verify plan was updated
        var updatedPlan = await _planService.GetPlanByIdAsync(plan.Id.ToString(), tokenModel);
        var planData = updatedPlan.data as SubscriptionPlan;
        Assert.Equal("Updated Plan", planData.Name);
        Assert.Equal("Updated description", planData.Description);
        Assert.Equal(149.99m, planData.BasePrice);
        Assert.Equal(15.0m, planData.AdminCommissionPercent);
    }

    [Fact]
    public async Task DeletePlanAsync_ShouldSoftDeletePlan()
    {
        // Arrange
        var plan = await TestData.SubscriptionPlan()
            .WithName("Plan to Delete")
            .WithBasePrice(99.99m)
            .BuildAsync();

        var tokenModel = CreateTestToken();

        // Act
        var result = await _planService.DeletePlanAsync(plan.Id, tokenModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);

        // Verify plan was soft deleted
        var deletedPlan = await _planService.GetPlanByIdAsync(plan.Id.ToString(), tokenModel);
        Assert.Equal(404, deletedPlan.StatusCode);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task CompletePlanLifecycle_ShouldWorkEndToEnd()
    {
        // Arrange
        var tokenModel = CreateTestToken();

        // Step 1: Create plan with auto-calculated pricing
        var createDto = new CreateSubscriptionPlanDto
        {
            Name = "Lifecycle Test Plan",
            Description = "Plan for testing complete lifecycle",
            BasePrice = 0m, // Will be calculated
            BillingCycleId = MasterData.BillingCycles.First(bc => bc.Name == "monthly").Id,
            CurrencyId = MasterData.Currencies.First(c => c.Code == "USD").Id,
            CategoryId = MasterData.Categories.First().Id,
            IsAutoCalculatedPrice = true,
            AdminCommissionPercent = 12.0m,
            IsActive = true,
            Privileges = new List<PlanPrivilegeDto>
            {
                new PlanPrivilegeDto
                {
                    PrivilegeId = MasterData.Privileges.First(p => p.Name == "Video Consultation").Id,
                    Value = 4,
                    PrivilegeBaseCost = 25.00m,
                    UnitCost = 30.00m
                },
                new PlanPrivilegeDto
                {
                    PrivilegeId = MasterData.Privileges.First(p => p.Name == "Prescription Management").Id,
                    Value = 2,
                    PrivilegeBaseCost = 15.00m,
                    UnitCost = 20.00m
                }
            }
        };

        var createResult = await _planService.CreatePlanAsync(createDto, tokenModel);
        Assert.Equal(200, createResult.StatusCode);
        var plan = createResult.data as SubscriptionPlan;

        // Step 2: Verify auto-calculated pricing
        // Expected: (4×25) + (2×15) = 100 + 30 = 130
        // Commission: 130 × 12% = 15.6
        // Total: 130 + 15.6 = 145.6
        Assert.Equal(145.60m, plan.BasePrice);
        Assert.Equal(130.00m, plan.PrivilegesTotalCost);

        // Step 3: Get pricing breakdown
        var breakdownResult = await _pricingService.GetPricingBreakdownAsync(plan.Id, tokenModel);
        Assert.Equal(200, breakdownResult.StatusCode);
        var breakdown = breakdownResult.data as PricingBreakdown;
        Assert.Equal(145.60m, breakdown.FinalPrice);

        // Step 4: Update plan
        var updateDto = new UpdateSubscriptionPlanDto
        {
            Id = plan.Id,
            Name = "Updated Lifecycle Plan",
            BasePrice = 200.00m,
            AdminCommissionPercent = 15.0m
        };

        var updateResult = await _planService.UpdatePlanAsync(updateDto, tokenModel);
        Assert.Equal(200, updateResult.StatusCode);

        // Step 5: Verify update
        var updatedPlanResult = await _planService.GetPlanByIdAsync(plan.Id.ToString(), tokenModel);
        var updatedPlan = updatedPlanResult.data as SubscriptionPlan;
        Assert.Equal("Updated Lifecycle Plan", updatedPlan.Name);
        Assert.Equal(200.00m, updatedPlan.BasePrice);
        Assert.Equal(15.0m, updatedPlan.AdminCommissionPercent);

        // Step 6: Delete plan
        var deleteResult = await _planService.DeletePlanAsync(plan.Id, tokenModel);
        Assert.Equal(200, deleteResult.StatusCode);

        // Step 7: Verify deletion
        var deletedPlanResult = await _planService.GetPlanByIdAsync(plan.Id.ToString(), tokenModel);
        Assert.Equal(404, deletedPlanResult.StatusCode);
    }

    #endregion
}
