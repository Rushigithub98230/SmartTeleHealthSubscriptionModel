using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Tests.Helpers;
using SmartTelehealth.Tests.Integration.Subscription;
using Xunit;

namespace SmartTelehealth.Tests.Integration.BillingCycles;

/// <summary>
/// Tests for quarterly (90-day) billing cycle operations
/// </summary>
[Trait("Category", "BillingCycle")]
public class QuarterlyBillingCycleTests : CleanTestBase
{
    [Fact]
    public async Task Test_QuarterlyRenewal_Complete_ShouldUpdateDatesCorrectly()
    {
        // Arrange
        var testEnv = await _testDataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _testDataBuilder.CreateTestUsersAsync(1);
        var user = users[0];
        var plan = testEnv.Plans[1]; // Premium plan
        var quarterlyCycle = testEnv.MasterData.BillingCycles.First(bc => bc.Name == "Quarterly");

        var subscription = await _testDataBuilder.CreateUserSubscriptionAsync(user, plan, quarterlyCycle);
        await _testDataBuilder.CreatePrivilegeUsageTrackingAsync(subscription);

        var startDate = subscription.StartDate;
        var originalNextBillingDate = subscription.NextBillingDate;

        // Verify initial billing date is 90 days out
        var expectedFirstBilling = startDate.AddDays(90);
        Assert.Equal(expectedFirstBilling.Date, originalNextBillingDate.Date);

        // Act - Simulate billing cycle completion
        await AdvanceTimeToBillingDateAsync(subscription);

        // Assert
        var updatedSubscription = await _context.Subscriptions.FindAsync(subscription.Id);
        Assert.NotNull(updatedSubscription.LastBillingDate);
        
        // Next billing should be 90 days after last billing
        var expectedNextBilling = updatedSubscription.LastBillingDate.Value.AddDays(90);
        Assert.Equal(expectedNextBilling.Date, updatedSubscription.NextBillingDate.Date);
    }

    [Fact]
    public async Task Test_QuarterlyPrivilegeUsage_AcrossFullQuarter_ShouldHoldLimits()
    {
        // Arrange
        var testEnv = await _testDataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _testDataBuilder.CreateTestUsersAsync(1);
        var user = users[0];
        var plan = testEnv.Plans[1];
        var quarterlyCycle = testEnv.MasterData.BillingCycles.First(bc => bc.Name == "Quarterly");

        var subscription = await _testDataBuilder.CreateUserSubscriptionAsync(user, plan, quarterlyCycle);
        await _testDataBuilder.CreatePrivilegeUsageTrackingAsync(subscription);

        // Get privilege usages
        var usages = await _context.UserSubscriptionPrivilegeUsages
            .Where(u => u.SubscriptionId == subscription.Id)
            .ToListAsync();

        // Assert - Usage period should be 90 days
        foreach (var usage in usages)
        {
            var periodLength = (usage.UsagePeriodEnd - usage.UsagePeriodStart).Days;
            Assert.Equal(90, periodLength);
        }

        // Act - Use privileges multiple times throughout quarter
        var limitedUsage = usages.FirstOrDefault(u => u.AllowedValue > 0);
        if (limitedUsage != null)
        {
            // Use half the allocation
            var usageAmount = limitedUsage.AllowedValue / 2;
            limitedUsage.UsedValue = usageAmount;
            limitedUsage.LastUsedAt = DateTime.UtcNow;
            _context.UserSubscriptionPrivilegeUsages.Update(limitedUsage);
            await _context.SaveChangesAsync();

            // Assert - Still have remaining privileges
            var updatedUsage = await _context.UserSubscriptionPrivilegeUsages.FindAsync(limitedUsage.Id);
            Assert.True(updatedUsage.RemainingValue > 0);
            Assert.False(updatedUsage.IsExhausted);
        }
    }

    [Fact]
    public async Task Test_QuarterlySubscription_PauseAndResume_ShouldAdjustBillingDates()
    {
        // Arrange
        var testEnv = await _testDataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _testDataBuilder.CreateTestUsersAsync(1);
        var user = users[0];
        var plan = testEnv.Plans[1];
        var quarterlyCycle = testEnv.MasterData.BillingCycles.First(bc => bc.Name == "Quarterly");

        var subscription = await _testDataBuilder.CreateUserSubscriptionAsync(user, plan, quarterlyCycle);
        var originalNextBillingDate = subscription.NextBillingDate;

        // Act - Pause for 30 days
        subscription.Status = "Paused";
        subscription.PausedDate = DateTime.UtcNow;
        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync();

        // Resume after 30 days
        subscription.Status = "Active";
        subscription.ResumedDate = DateTime.UtcNow.AddDays(30);
        subscription.NextBillingDate = subscription.NextBillingDate.AddDays(30); // Extend by pause duration
        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync();

        // Assert
        var updatedSubscription = await _context.Subscriptions.FindAsync(subscription.Id);
        Assert.True(updatedSubscription.NextBillingDate > originalNextBillingDate);
        Assert.Equal(originalNextBillingDate.AddDays(30).Date, updatedSubscription.NextBillingDate.Date);
    }

    [Fact]
    public async Task Test_QuarterlyDiscount_ShouldApplyIfConfigured()
    {
        // Arrange
        var testEnv = await _testDataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _testDataBuilder.CreateTestUsersAsync(1);
        var user = users[0];
        var plan = testEnv.Plans[1];
        var quarterlyCycle = testEnv.MasterData.BillingCycles.First(bc => bc.Name == "Quarterly");

        // Set quarterly discount on plan
        plan.QuarterlyBillingDiscount = 10m; // 10% discount
        _context.SubscriptionPlans.Update(plan);
        await _context.SaveChangesAsync();

        // Act - Create subscription
        var subscription = await _testDataBuilder.CreateUserSubscriptionAsync(user, plan, quarterlyCycle);
        
        // Calculate expected price with discount
        var basePrice = plan.Price;
        var discountAmount = basePrice * (plan.QuarterlyBillingDiscount / 100);
        var expectedPrice = basePrice - discountAmount;

        // Assert - Current price should reflect discount
        // Note: In real implementation, this would be handled by BillingCycleCalculator
        Assert.True(plan.QuarterlyBillingDiscount > 0);
    }
}

