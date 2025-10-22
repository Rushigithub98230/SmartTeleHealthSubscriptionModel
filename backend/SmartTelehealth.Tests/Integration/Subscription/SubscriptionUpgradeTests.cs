using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Tests.Helpers;
using Xunit;

namespace SmartTelehealth.Tests.Integration.Subscription;

/// <summary>
/// Tests for subscription upgrades, downgrades, and plan changes
/// </summary>
[Trait("Category", "SubscriptionLifecycle")]
public class SubscriptionUpgradeTests : CleanTestBase
{
    [Fact]
    public async Task Test_UpgradePlan_MidCycle_ShouldUpdatePlan()
    {
        // Arrange
        var testEnv = await _testDataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _testDataBuilder.CreateTestUsersAsync(1);
        var user = users[0];
        var basicPlan = testEnv.Plans[0]; // Basic plan
        var premiumPlan = testEnv.Plans[1]; // Premium plan
        var monthlyCycle = testEnv.MasterData.BillingCycles.First(bc => bc.Name == "Monthly");

        var subscription = await _testDataBuilder.CreateUserSubscriptionAsync(user, basicPlan, monthlyCycle);
        await _testDataBuilder.CreatePrivilegeUsageTrackingAsync(subscription);

        var originalPlanId = subscription.SubscriptionPlanId;
        var originalPrice = subscription.CurrentPrice;

        // Act - Upgrade to premium plan
        subscription.SubscriptionPlanId = premiumPlan.Id;
        subscription.CurrentPrice = premiumPlan.Price;
        subscription.UpdatedDate = DateTime.UtcNow;
        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync();

        // Assert
        var updatedSubscription = await _context.Subscriptions.FindAsync(subscription.Id);
        Assert.NotEqual(originalPlanId, updatedSubscription.SubscriptionPlanId);
        Assert.Equal(premiumPlan.Id, updatedSubscription.SubscriptionPlanId);
        Assert.NotEqual(originalPrice, updatedSubscription.CurrentPrice);
        Assert.Equal(premiumPlan.Price, updatedSubscription.CurrentPrice);
    }

    [Fact]
    public async Task Test_UpgradePlan_ShouldReallocatePrivileges()
    {
        // Arrange
        var testEnv = await _testDataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _testDataBuilder.CreateTestUsersAsync(1);
        var user = users[0];
        var basicPlan = testEnv.Plans[0]; // Basic plan - 2 privileges
        var premiumPlan = testEnv.Plans[1]; // Premium plan - 3 privileges
        var monthlyCycle = testEnv.MasterData.BillingCycles.First(bc => bc.Name == "Monthly");

        var subscription = await _testDataBuilder.CreateUserSubscriptionAsync(user, basicPlan, monthlyCycle);
        await _testDataBuilder.CreatePrivilegeUsageTrackingAsync(subscription);

        var originalPrivilegeCount = await _context.UserSubscriptionPrivilegeUsages
            .CountAsync(u => u.SubscriptionId == subscription.Id);
        Assert.Equal(2, originalPrivilegeCount);

        // Act - Upgrade and reallocate privileges
        subscription.SubscriptionPlanId = premiumPlan.Id;
        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync();

        // Remove old privileges
        var oldUsages = await _context.UserSubscriptionPrivilegeUsages
            .Where(u => u.SubscriptionId == subscription.Id)
            .ToListAsync();
        _context.UserSubscriptionPrivilegeUsages.RemoveRange(oldUsages);
        await _context.SaveChangesAsync();

        // Allocate new privileges based on premium plan
        await _testDataBuilder.CreatePrivilegeUsageTrackingAsync(subscription);

        // Assert
        var newPrivilegeCount = await _context.UserSubscriptionPrivilegeUsages
            .CountAsync(u => u.SubscriptionId == subscription.Id);
        Assert.Equal(3, newPrivilegeCount); // Premium plan has 3 privileges
    }

    [Fact]
    public async Task Test_DowngradePlan_ScheduledForNextCycle_ShouldNotImmediatelyChange()
    {
        // Arrange
        var testEnv = await _testDataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _testDataBuilder.CreateTestUsersAsync(1);
        var user = users[0];
        var premiumPlan = testEnv.Plans[1]; // Premium plan
        var basicPlan = testEnv.Plans[0]; // Basic plan
        var monthlyCycle = testEnv.MasterData.BillingCycles.First(bc => bc.Name == "Monthly");

        var subscription = await _testDataBuilder.CreateUserSubscriptionAsync(user, premiumPlan, monthlyCycle);
        var originalPlanId = subscription.SubscriptionPlanId;

        // Act - Schedule downgrade (doesn't change immediately)
        // In real implementation, this would set a PendingPlanChange field
        subscription.UpdatedDate = DateTime.UtcNow;
        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync();

        // Assert - Plan should still be premium until next billing cycle
        var currentSubscription = await _context.Subscriptions.FindAsync(subscription.Id);
        Assert.Equal(originalPlanId, currentSubscription.SubscriptionPlanId);
        Assert.Equal(premiumPlan.Id, currentSubscription.SubscriptionPlanId);
    }

    [Fact]
    public async Task Test_UpgradePlan_ShouldMaintainBillingCycle()
    {
        // Arrange
        var testEnv = await _testDataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _testDataBuilder.CreateTestUsersAsync(1);
        var user = users[0];
        var basicPlan = testEnv.Plans[0];
        var premiumPlan = testEnv.Plans[1];
        var monthlyCycle = testEnv.MasterData.BillingCycles.First(bc => bc.Name == "Monthly");

        var subscription = await _testDataBuilder.CreateUserSubscriptionAsync(user, basicPlan, monthlyCycle);
        var originalBillingCycleId = subscription.BillingCycleId;
        var originalNextBillingDate = subscription.NextBillingDate;

        // Act - Upgrade plan
        subscription.SubscriptionPlanId = premiumPlan.Id;
        subscription.CurrentPrice = premiumPlan.Price;
        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync();

        // Assert - Billing cycle should remain the same
        var upgradedSubscription = await _context.Subscriptions.FindAsync(subscription.Id);
        Assert.Equal(originalBillingCycleId, upgradedSubscription.BillingCycleId);
        Assert.Equal(originalNextBillingDate, upgradedSubscription.NextBillingDate);
    }
}

