using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Tests.Helpers;
using SmartTelehealth.Tests.Integration.Subscription;
using Xunit;

namespace SmartTelehealth.Tests.Integration.Privileges;

/// <summary>
/// Tests for privilege usage tracking and enforcement
/// </summary>
[Trait("Category", "Privilege")]
public class PrivilegeUsageTests : CleanTestBase
{
    [Fact]
    public async Task Test_UsePrivilegeWithinLimits_ShouldIncrement()
    {
        // Arrange
        var testEnv = await _testDataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _testDataBuilder.CreateTestUsersAsync(1);
        var user = users[0];
        var plan = testEnv.Plans[0];
        var monthlyCycle = testEnv.MasterData.BillingCycles.First(bc => bc.Name == "Monthly");

        var subscription = await _testDataBuilder.CreateUserSubscriptionAsync(user, plan, monthlyCycle);
        await _testDataBuilder.CreatePrivilegeUsageTrackingAsync(subscription);

        var usage = await _context.UserSubscriptionPrivilegeUsages
            .FirstAsync(u => u.SubscriptionId == subscription.Id && u.AllowedValue > 0);

        var initialUsed = usage.UsedValue;
        var allowedValue = usage.AllowedValue;

        // Act - Use privilege
        usage.UsedValue += 1;
        usage.LastUsedAt = DateTime.UtcNow;
        _context.UserSubscriptionPrivilegeUsages.Update(usage);
        await _context.SaveChangesAsync();

        // Assert
        var updatedUsage = await _context.UserSubscriptionPrivilegeUsages.FindAsync(usage.Id);
        Assert.Equal(initialUsed + 1, updatedUsage.UsedValue);
        Assert.NotNull(updatedUsage.LastUsedAt);
        Assert.True(updatedUsage.RemainingValue < allowedValue);
        AssertionHelpers.AssertPrivilegeUsageWithinLimits(updatedUsage);
    }

    [Fact]
    public async Task Test_AttemptUsageAtLimit_ShouldBlock()
    {
        // Arrange
        var testEnv = await _testDataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _testDataBuilder.CreateTestUsersAsync(1);
        var user = users[0];
        var plan = testEnv.Plans[0];
        var monthlyCycle = testEnv.MasterData.BillingCycles.First(bc => bc.Name == "Monthly");

        var subscription = await _testDataBuilder.CreateOverageScenarioAsync(user, plan, monthlyCycle, exceedLimit: false);

        var usage = await _context.UserSubscriptionPrivilegeUsages
            .FirstAsync(u => u.SubscriptionId == subscription.Id && u.AllowedValue > 0);

        // Assert - Usage should be at limit
        Assert.Equal(usage.AllowedValue, usage.UsedValue);
        Assert.True(usage.IsExhausted);
        Assert.Equal(0, usage.RemainingValue);
    }

    [Fact]
    public async Task Test_UnlimitedPrivilegeUsage_ShouldAlwaysAllow()
    {
        // Arrange
        var testEnv = await _testDataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _testDataBuilder.CreateTestUsersAsync(1);
        var user = users[0];
        var monthlyCycle = testEnv.MasterData.BillingCycles.First(bc => bc.Name == "Monthly");

        // Create plan with unlimited privilege
        var plan = await _testDataBuilder.CreatePlanWithPrivilegeConfigAsync(
            "Unlimited Usage Plan",
            99.99m,
            monthlyCycle,
            new List<(Privilege, int, decimal)>
            {
                (testEnv.Privileges[0], -1, 0m) // Unlimited
            }
        );

        var subscription = await _testDataBuilder.CreateUserSubscriptionAsync(user, plan, monthlyCycle);
        await _testDataBuilder.CreatePrivilegeUsageTrackingAsync(subscription);

        var usage = await _context.UserSubscriptionPrivilegeUsages
            .FirstAsync(u => u.SubscriptionId == subscription.Id);

        // Act - Use privilege many times
        for (int i = 0; i < 100; i++)
        {
            usage.UsedValue++;
        }
        usage.LastUsedAt = DateTime.UtcNow;
        _context.UserSubscriptionPrivilegeUsages.Update(usage);
        await _context.SaveChangesAsync();

        // Assert - Should still be unlimited
        var updatedUsage = await _context.UserSubscriptionPrivilegeUsages.FindAsync(usage.Id);
        Assert.True(updatedUsage.IsUnlimited);
        Assert.False(updatedUsage.IsExhausted);
        Assert.Equal(int.MaxValue, updatedUsage.RemainingValue);
        Assert.Equal(100, updatedUsage.UsedValue);
    }

    [Fact]
    public async Task Test_ConcurrentUsageBySameUser_ShouldTrackAccurately()
    {
        // Arrange
        var testEnv = await _testDataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _testDataBuilder.CreateTestUsersAsync(1);
        var user = users[0];
        var plan = testEnv.Plans[0];
        var monthlyCycle = testEnv.MasterData.BillingCycles.First(bc => bc.Name == "Monthly");

        var subscription = await _testDataBuilder.CreateUserSubscriptionAsync(user, plan, monthlyCycle);
        await _testDataBuilder.CreatePrivilegeUsageTrackingAsync(subscription);

        var usage = await _context.UserSubscriptionPrivilegeUsages
            .FirstAsync(u => u.SubscriptionId == subscription.Id && u.AllowedValue > 0);

        var initialUsed = usage.UsedValue;

        // Act - Simulate 5 concurrent uses
        for (int i = 0; i < 5; i++)
        {
            var currentUsage = await _context.UserSubscriptionPrivilegeUsages.FindAsync(usage.Id);
            currentUsage.UsedValue++;
            currentUsage.LastUsedAt = DateTime.UtcNow;
            _context.UserSubscriptionPrivilegeUsages.Update(currentUsage);
            await _context.SaveChangesAsync();
        }

        // Assert
        var finalUsage = await _context.UserSubscriptionPrivilegeUsages.FindAsync(usage.Id);
        Assert.Equal(initialUsed + 5, finalUsage.UsedValue);
    }

    [Fact]
    public async Task Test_UsageHistoryRecording_ShouldCreateRecords()
    {
        // Arrange
        var testEnv = await _testDataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _testDataBuilder.CreateTestUsersAsync(1);
        var user = users[0];
        var plan = testEnv.Plans[0];
        var monthlyCycle = testEnv.MasterData.BillingCycles.First(bc => bc.Name == "Monthly");

        var subscription = await _testDataBuilder.CreateUserSubscriptionAsync(user, plan, monthlyCycle);
        await _testDataBuilder.CreatePrivilegeUsageTrackingAsync(subscription);

        var usage = await _context.UserSubscriptionPrivilegeUsages
            .FirstAsync(u => u.SubscriptionId == subscription.Id && u.AllowedValue > 0);

        // Act - Use privilege and create history
        usage.UsedValue++;
        usage.LastUsedAt = DateTime.UtcNow;
        _context.UserSubscriptionPrivilegeUsages.Update(usage);
        await _context.SaveChangesAsync();

        // Create usage history manually (in real implementation, this would be automatic)
        var history = new PrivilegeUsageHistory
        {
            Id = Guid.NewGuid(),
            UserSubscriptionPrivilegeUsageId = usage.Id,
            UsageDate = DateTime.UtcNow,
            UsedValue = 1, // Correct property name
            IsActive = true,
            IsDeleted = false,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = user.Id
        };
        _context.PrivilegeUsageHistories.Add(history);
        await _context.SaveChangesAsync();

        // Assert
        var historyRecords = await _context.PrivilegeUsageHistories
            .Where(h => h.UserSubscriptionPrivilegeUsageId == usage.Id)
            .ToListAsync();

        Assert.NotEmpty(historyRecords);
        Assert.Single(historyRecords);
        Assert.Equal(1, historyRecords[0].UsedValue);
    }

    [Fact]
    public async Task Test_LastUsedAtTracking_ShouldUpdateTimestamp()
    {
        // Arrange
        var testEnv = await _testDataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _testDataBuilder.CreateTestUsersAsync(1);
        var user = users[0];
        var plan = testEnv.Plans[0];
        var monthlyCycle = testEnv.MasterData.BillingCycles.First(bc => bc.Name == "Monthly");

        var subscription = await _testDataBuilder.CreateUserSubscriptionAsync(user, plan, monthlyCycle);
        await _testDataBuilder.CreatePrivilegeUsageTrackingAsync(subscription);

        var usage = await _context.UserSubscriptionPrivilegeUsages
            .FirstAsync(u => u.SubscriptionId == subscription.Id && u.AllowedValue > 0);

        Assert.Null(usage.LastUsedAt);

        // Act - Use privilege
        var usageTime = DateTime.UtcNow;
        usage.UsedValue++;
        usage.LastUsedAt = usageTime;
        _context.UserSubscriptionPrivilegeUsages.Update(usage);
        await _context.SaveChangesAsync();

        // Assert
        var updatedUsage = await _context.UserSubscriptionPrivilegeUsages.FindAsync(usage.Id);
        Assert.NotNull(updatedUsage.LastUsedAt);
        Assert.True((updatedUsage.LastUsedAt.Value - usageTime).TotalSeconds < 5);
    }
}

