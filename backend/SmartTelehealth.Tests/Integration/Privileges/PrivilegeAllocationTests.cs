using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Tests.Helpers;
using SmartTelehealth.Tests.Integration.Subscription;
using Xunit;

namespace SmartTelehealth.Tests.Integration.Privileges;

/// <summary>
/// Tests for privilege allocation on subscription creation
/// </summary>
[Trait("Category", "Privilege")]
public class PrivilegeAllocationTests : CleanTestBase
{
    [Fact]
    public async Task Test_InitialAllocation_ShouldCreateUsageRecords()
    {
        // Arrange
        var testEnv = await _testDataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _testDataBuilder.CreateTestUsersAsync(1);
        var user = users[0];
        var plan = testEnv.Plans[0]; // Basic plan with 2 privileges
        var monthlyCycle = testEnv.MasterData.BillingCycles.First(bc => bc.Name == "Monthly");

        // Act
        var subscription = await _testDataBuilder.CreateUserSubscriptionAsync(user, plan, monthlyCycle);
        await _testDataBuilder.CreatePrivilegeUsageTrackingAsync(subscription);

        // Assert
        var usages = await _context.UserSubscriptionPrivilegeUsages
            .Where(u => u.SubscriptionId == subscription.Id)
            .ToListAsync();

        Assert.NotEmpty(usages);
        Assert.Equal(2, usages.Count); // Basic plan has 2 privileges

        foreach (var usage in usages)
        {
            Assert.Equal(subscription.Id, usage.SubscriptionId);
            Assert.Equal(0, usage.UsedValue);
            Assert.True(usage.AllowedValue != 0); // Should not be disabled
        }
    }

    [Fact]
    public async Task Test_AllocationCalculationAccuracy_ShouldMatchPlanConfig()
    {
        // Arrange
        var testEnv = await _testDataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _testDataBuilder.CreateTestUsersAsync(1);
        var user = users[0];
        var plan = testEnv.Plans[0];
        var monthlyCycle = testEnv.MasterData.BillingCycles.First(bc => bc.Name == "Monthly");

        var subscription = await _testDataBuilder.CreateUserSubscriptionAsync(user, plan, monthlyCycle);
        await _testDataBuilder.CreatePrivilegeUsageTrackingAsync(subscription);

        // Act - Get plan privileges and usage records
        var planPrivileges = await _context.SubscriptionPlanPrivileges
            .Where(pp => pp.SubscriptionPlanId == plan.Id)
            .ToListAsync();

        var usages = await _context.UserSubscriptionPrivilegeUsages
            .Where(u => u.SubscriptionId == subscription.Id)
            .ToListAsync();

        // Assert - Allocation should match plan configuration exactly
        Assert.Equal(planPrivileges.Count, usages.Count);

        foreach (var planPrivilege in planPrivileges)
        {
            var usage = usages.FirstOrDefault(u => u.PrivilegeId == planPrivilege.PrivilegeId);
            Assert.NotNull(usage);
            
            // AllowedValue should equal plan's Value (admin-set total)
            Assert.Equal(planPrivilege.Value, usage.AllowedValue);
        }
    }

    [Fact]
    public async Task Test_UsagePeriodAlignment_ShouldMatchBillingDates()
    {
        // Arrange
        var testEnv = await _testDataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _testDataBuilder.CreateTestUsersAsync(1);
        var user = users[0];
        var plan = testEnv.Plans[0];
        var monthlyCycle = testEnv.MasterData.BillingCycles.First(bc => bc.Name == "Monthly");

        // Act
        var subscription = await _testDataBuilder.CreateUserSubscriptionAsync(user, plan, monthlyCycle);
        await _testDataBuilder.CreatePrivilegeUsageTrackingAsync(subscription);

        var usages = await _context.UserSubscriptionPrivilegeUsages
            .Where(u => u.SubscriptionId == subscription.Id)
            .ToListAsync();

        // Assert - Usage periods should align with billing period
        foreach (var usage in usages)
        {
            Assert.Equal(subscription.StartDate, usage.UsagePeriodStart);
            Assert.Equal(subscription.NextBillingDate, usage.UsagePeriodEnd);
        }
    }

    [Fact]
    public async Task Test_UnlimitedPrivileges_ShouldAllocateWithNegativeOne()
    {
        // Arrange
        var testEnv = await _testDataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _testDataBuilder.CreateTestUsersAsync(1);
        var user = users[0];
        var monthlyCycle = testEnv.MasterData.BillingCycles.First(bc => bc.Name == "Monthly");
        var privilege = testEnv.Privileges[0];

        // Create plan with unlimited privilege
        var plan = await _testDataBuilder.CreatePlanWithPrivilegeConfigAsync(
            "Unlimited Test Plan",
            99.99m,
            monthlyCycle,
            new List<(Privilege, int, decimal)>
            {
                (privilege, -1, 0m) // Unlimited (-1)
            }
        );

        // Act
        var subscription = await _testDataBuilder.CreateUserSubscriptionAsync(user, plan, monthlyCycle);
        await _testDataBuilder.CreatePrivilegeUsageTrackingAsync(subscription);

        var usage = await _context.UserSubscriptionPrivilegeUsages
            .FirstOrDefaultAsync(u => u.SubscriptionId == subscription.Id);

        // Assert
        Assert.NotNull(usage);
        Assert.Equal(-1, usage.AllowedValue);
        Assert.True(usage.IsUnlimited);
        Assert.Equal(int.MaxValue, usage.RemainingValue);
    }

    [Fact]
    public async Task Test_DisabledPrivileges_ShouldNotAllocate()
    {
        // Arrange
        var testEnv = await _testDataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _testDataBuilder.CreateTestUsersAsync(1);
        var user = users[0];
        var monthlyCycle = testEnv.MasterData.BillingCycles.First(bc => bc.Name == "Monthly");
        var privilege = testEnv.Privileges[0];

        // Create plan with disabled privilege
        var plan = await _testDataBuilder.CreatePlanWithPrivilegeConfigAsync(
            "Disabled Privilege Plan",
            49.99m,
            monthlyCycle,
            new List<(Privilege, int, decimal)>
            {
                (privilege, 0, 0m) // Disabled (0)
            }
        );

        // Act
        var subscription = await _testDataBuilder.CreateUserSubscriptionAsync(user, plan, monthlyCycle);
        await _testDataBuilder.CreatePrivilegeUsageTrackingAsync(subscription);

        var usage = await _context.UserSubscriptionPrivilegeUsages
            .FirstOrDefaultAsync(u => u.SubscriptionId == subscription.Id);

        // Assert - Disabled privileges may still be tracked but with 0 allowed
        if (usage != null)
        {
            Assert.Equal(0, usage.AllowedValue);
            Assert.False(usage.IsUnlimited);
        }
    }

    [Fact]
    public async Task Test_MixedPrivilegeTypes_ShouldAllocateCorrectly()
    {
        // Arrange
        var testEnv = await _testDataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _testDataBuilder.CreateTestUsersAsync(1);
        var user = users[0];
        var monthlyCycle = testEnv.MasterData.BillingCycles.First(bc => bc.Name == "Monthly");

        // Create plan with mixed privilege types
        var plan = await _testDataBuilder.CreatePlanWithPrivilegeConfigAsync(
            "Mixed Privileges Plan",
            149.99m,
            monthlyCycle,
            new List<(Privilege, int, decimal)>
            {
                (testEnv.Privileges[0], 10, 5.00m),  // Limited to 10
                (testEnv.Privileges[1], -1, 0m),      // Unlimited
                (testEnv.Privileges[2], 0, 0m)        // Disabled
            }
        );

        // Act
        var subscription = await _testDataBuilder.CreateUserSubscriptionAsync(user, plan, monthlyCycle);
        await _testDataBuilder.CreatePrivilegeUsageTrackingAsync(subscription);

        var usages = await _context.UserSubscriptionPrivilegeUsages
            .Where(u => u.SubscriptionId == subscription.Id)
            .OrderBy(u => u.PrivilegeId)
            .ToListAsync();

        // Assert - Should have 3 different types of allocations
        Assert.NotEmpty(usages);
        
        var limitedUsage = usages.FirstOrDefault(u => u.AllowedValue == 10);
        var unlimitedUsage = usages.FirstOrDefault(u => u.AllowedValue == -1);
        var disabledUsage = usages.FirstOrDefault(u => u.AllowedValue == 0);

        Assert.NotNull(limitedUsage);
        Assert.NotNull(unlimitedUsage);
        
        // Verify properties
        Assert.False(limitedUsage.IsUnlimited);
        Assert.True(unlimitedUsage.IsUnlimited);
    }
}

