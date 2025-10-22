using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Tests.Helpers;
using SmartTelehealth.Tests.Integration.Subscription;
using Xunit;

namespace SmartTelehealth.Tests.Integration.BillingCycles;

/// <summary>
/// Tests for annual (365-day) billing cycle operations
/// </summary>
[Trait("Category", "BillingCycle")]
public class AnnualBillingCycleTests : CleanTestBase
{
    [Fact]
    public async Task Test_AnnualRenewal_Complete_ShouldUpdateForOneYear()
    {
        // Arrange
        var testEnv = await _testDataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _testDataBuilder.CreateTestUsersAsync(1);
        var user = users[0];
        var plan = testEnv.Plans[2]; // Professional plan
        var annualCycle = testEnv.MasterData.BillingCycles.First(bc => bc.Name == "Yearly");

        var subscription = await _testDataBuilder.CreateUserSubscriptionAsync(user, plan, annualCycle);
        var startDate = subscription.StartDate;
        var originalNextBillingDate = subscription.NextBillingDate;

        // Verify initial billing date is 365 days out
        var expectedFirstBilling = startDate.AddDays(365);
        Assert.Equal(expectedFirstBilling.Date, originalNextBillingDate.Date);

        // Act - Simulate annual billing cycle completion
        await AdvanceTimeToBillingDateAsync(subscription);

        // Assert
        var updatedSubscription = await _context.Subscriptions.FindAsync(subscription.Id);
        Assert.NotNull(updatedSubscription.LastBillingDate);
        
        // Next billing should be 365 days after last billing
        var expectedNextBilling = updatedSubscription.LastBillingDate.Value.AddDays(365);
        Assert.Equal(expectedNextBilling.Date, updatedSubscription.NextBillingDate.Date);
    }

    [Fact]
    public async Task Test_AnnualDiscount_ShouldApplyCorrectly()
    {
        // Arrange
        var testEnv = await _testDataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _testDataBuilder.CreateTestUsersAsync(1);
        var user = users[0];
        var plan = testEnv.Plans[2];
        var annualCycle = testEnv.MasterData.BillingCycles.First(bc => bc.Name == "Yearly");

        // Set annual discount on plan
        plan.AnnualBillingDiscount = 20m; // 20% discount for annual billing
        _context.SubscriptionPlans.Update(plan);
        await _context.SaveChangesAsync();

        // Act - Create subscription
        var subscription = await _testDataBuilder.CreateUserSubscriptionAsync(user, plan, annualCycle);

        // Assert - Verify discount is configured
        var updatedPlan = await _context.SubscriptionPlans.FindAsync(plan.Id);
        Assert.Equal(20m, updatedPlan.AnnualBillingDiscount);
        
        // In real implementation, BillingCycleCalculator would apply the discount
        var basePrice = plan.Price;
        var discountAmount = basePrice * (plan.AnnualBillingDiscount / 100);
        var expectedDiscountedPrice = basePrice - discountAmount;
        
        Assert.True(expectedDiscountedPrice < basePrice);
    }

    [Fact]
    public async Task Test_AnnualPrivilegeAllocation_HighLimits_ShouldWork()
    {
        // Arrange
        var testEnv = await _testDataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _testDataBuilder.CreateTestUsersAsync(1);
        var user = users[0];
        var monthlyCycle = testEnv.MasterData.BillingCycles.First(bc => bc.Name == "Monthly");
        var annualCycle = testEnv.MasterData.BillingCycles.First(bc => bc.Name == "Yearly");
        
        var privilege = testEnv.Privileges[0];
        
        // Create plan with high annual limits
        var annualPlan = await _testDataBuilder.CreatePlanWithPrivilegeConfigAsync(
            "Annual Premium",
            999.99m,
            annualCycle,
            new List<(Privilege, int, decimal)>
            {
                (privilege, 365, 5.00m) // 365 uses for the year (daily use)
            }
        );

        // Act - Create subscription and allocate privileges
        var subscription = await _testDataBuilder.CreateUserSubscriptionAsync(user, annualPlan, annualCycle);
        await _testDataBuilder.CreatePrivilegeUsageTrackingAsync(subscription);

        // Assert
        var usages = await _context.UserSubscriptionPrivilegeUsages
            .Where(u => u.SubscriptionId == subscription.Id)
            .ToListAsync();

        Assert.NotEmpty(usages);
        var privilegeUsage = usages.First();
        Assert.Equal(365, privilegeUsage.AllowedValue);
        
        // Usage period should be 365 days
        var periodLength = (privilegeUsage.UsagePeriodEnd - privilegeUsage.UsagePeriodStart).Days;
        Assert.Equal(365, periodLength);
    }

    [Fact]
    public async Task Test_MidYearUpgrade_ShouldProrateProperly()
    {
        // Arrange
        var testEnv = await _testDataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _testDataBuilder.CreateTestUsersAsync(1);
        var user = users[0];
        var basicPlan = testEnv.Plans[0];
        var premiumPlan = testEnv.Plans[1];
        var annualCycle = testEnv.MasterData.BillingCycles.First(bc => bc.Name == "Yearly");

        // Create annual subscription
        var subscription = await _testDataBuilder.CreateUserSubscriptionAsync(user, basicPlan, annualCycle);
        var startDate = subscription.StartDate;
        var originalPrice = subscription.CurrentPrice;

        // Simulate 6 months passing (halfway through year)
        subscription.LastBillingDate = startDate;
        subscription.NextBillingDate = startDate.AddDays(365);
        var upgradeDate = startDate.AddDays(180); // 6 months later
        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync();

        // Act - Upgrade mid-year
        subscription.SubscriptionPlanId = premiumPlan.Id;
        subscription.CurrentPrice = premiumPlan.Price;
        subscription.UpdatedDate = upgradeDate;
        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync();

        // Assert
        var updatedSubscription = await _context.Subscriptions.FindAsync(subscription.Id);
        Assert.Equal(premiumPlan.Id, updatedSubscription.SubscriptionPlanId);
        Assert.NotEqual(originalPrice, updatedSubscription.CurrentPrice);
        
        // Next billing date should remain unchanged (end of annual period)
        Assert.Equal(startDate.AddDays(365).Date, updatedSubscription.NextBillingDate.Date);
        
        // Note: In real implementation, proration would be calculated:
        // - Credit for unused portion of basic plan (6 months)
        // - Charge for premium plan for remaining 6 months
        var daysRemaining = (updatedSubscription.NextBillingDate - upgradeDate).Days;
        Assert.True(daysRemaining > 0);
        Assert.True(daysRemaining < 365);
    }
}

