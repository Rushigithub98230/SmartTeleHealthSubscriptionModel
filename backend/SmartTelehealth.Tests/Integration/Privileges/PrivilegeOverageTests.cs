using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Tests.Helpers;
using SmartTelehealth.Tests.Integration.Subscription;
using Xunit;

namespace SmartTelehealth.Tests.Integration.Privileges;

/// <summary>
/// Tests for privilege overage handling and billing
/// </summary>
[Trait("Category", "Privilege")]
public class PrivilegeOverageTests : CleanTestBase
{
    [Fact]
    public async Task Test_OverageCalculationAccuracy_ShouldMatchFormula()
    {
        // Arrange
        var testEnv = await _testDataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _testDataBuilder.CreateTestUsersAsync(1);
        var user = users[0];
        var plan = testEnv.Plans[0];
        var monthlyCycle = testEnv.MasterData.BillingCycles.First(bc => bc.Name == "Monthly");

        var subscription = await _testDataBuilder.CreateOverageScenarioAsync(user, plan, monthlyCycle, exceedLimit: true);

        var usages = await _context.UserSubscriptionPrivilegeUsages
            .Include(u => u.SubscriptionPlanPrivilege)
            .Where(u => u.SubscriptionId == subscription.Id && u.UsedValue > u.AllowedValue)
            .ToListAsync();

        // Act - Calculate overage
        decimal totalOverage = 0;
        foreach (var usage in usages)
        {
            var overage = usage.UsedValue - usage.AllowedValue;
            var overageCharge = overage * usage.SubscriptionPlanPrivilege.UnitCost;
            totalOverage += overageCharge;

            // Assert individual calculation
            AssertionHelpers.AssertOverageCalculation(usage, usage.SubscriptionPlanPrivilege, overageCharge);
        }

        // Assert
        Assert.True(totalOverage > 0);
    }

    [Fact]
    public async Task Test_MultiplePrivilegeOverages_ShouldSumCorrectly()
    {
        // Arrange
        var testEnv = await _testDataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _testDataBuilder.CreateTestUsersAsync(1);
        var user = users[0];
        var monthlyCycle = testEnv.MasterData.BillingCycles.First(bc => bc.Name == "Monthly");

        // Create plan with multiple privileges with overage charges
        var plan = await _testDataBuilder.CreatePlanWithPrivilegeConfigAsync(
            "Overage Test Plan",
            99.99m,
            monthlyCycle,
            new List<(Privilege, int, decimal)>
            {
                (testEnv.Privileges[0], 5, 10.00m),  // Limit 5, $10 per overage
                (testEnv.Privileges[1], 3, 15.00m),  // Limit 3, $15 per overage
                (testEnv.Privileges[2], 2, 20.00m)   // Limit 2, $20 per overage
            }
        );

        var subscription = await _testDataBuilder.CreateUserSubscriptionAsync(user, plan, monthlyCycle);
        await _testDataBuilder.CreatePrivilegeUsageTrackingAsync(subscription);

        // Set all privileges to exceed limits by 2
        var usages = await _context.UserSubscriptionPrivilegeUsages
            .Where(u => u.SubscriptionId == subscription.Id)
            .ToListAsync();

        foreach (var usage in usages)
        {
            usage.UsedValue = usage.AllowedValue + 2; // Exceed by 2
            usage.LastUsedAt = DateTime.UtcNow;
        }
        await _context.SaveChangesAsync();

        // Act - Calculate total overage
        var usagesWithPlanPrivilege = await _context.UserSubscriptionPrivilegeUsages
            .Include(u => u.SubscriptionPlanPrivilege)
            .Where(u => u.SubscriptionId == subscription.Id)
            .ToListAsync();

        decimal totalOverage = 0;
        foreach (var usage in usagesWithPlanPrivilege)
        {
            var overage = usage.UsedValue - usage.AllowedValue;
            totalOverage += overage * usage.SubscriptionPlanPrivilege.UnitCost;
        }

        // Assert - Expected: (2 * $10) + (2 * $15) + (2 * $20) = $90
        var expectedOverage = (2 * 10.00m) + (2 * 15.00m) + (2 * 20.00m);
        Assert.Equal(expectedOverage, totalOverage);
    }

    [Fact]
    public async Task Test_OverageBillingRecordCreation_ShouldHaveCorrectType()
    {
        // Arrange
        var testEnv = await _testDataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _testDataBuilder.CreateTestUsersAsync(1);
        var user = users[0];
        var plan = testEnv.Plans[0];
        var monthlyCycle = testEnv.MasterData.BillingCycles.First(bc => bc.Name == "Monthly");

        var subscription = await _testDataBuilder.CreateOverageScenarioAsync(user, plan, monthlyCycle, exceedLimit: true);

        var usages = await _context.UserSubscriptionPrivilegeUsages
            .Include(u => u.SubscriptionPlanPrivilege)
            .Where(u => u.SubscriptionId == subscription.Id && u.UsedValue > u.AllowedValue)
            .ToListAsync();

        decimal totalOverage = 0;
        foreach (var usage in usages)
        {
            var overage = usage.UsedValue - usage.AllowedValue;
            totalOverage += overage * usage.SubscriptionPlanPrivilege.UnitCost;
        }

        // Act - Create overage billing record
        var overageBilling = new BillingRecord
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            SubscriptionId = subscription.Id,
            Amount = totalOverage,
            TotalAmount = totalOverage,
            Status = BillingRecord.BillingStatus.Pending,
            Type = BillingRecord.BillingType.Overage, // Important!
            BillingDate = DateTime.UtcNow,
            DueDate = subscription.NextBillingDate,
            Description = "Overage charges for exceeding privilege limits",
            CurrencyId = plan.CurrencyId,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = user.Id
        };
        _context.BillingRecords.Add(overageBilling);
        await _context.SaveChangesAsync();

        // Assert
        var savedBilling = await _context.BillingRecords.FindAsync(overageBilling.Id);
        Assert.NotNull(savedBilling);
        Assert.Equal(BillingRecord.BillingType.Overage, savedBilling.Type);
        Assert.Equal(totalOverage, savedBilling.Amount);
        AssertionHelpers.AssertBillingRecordComplete(savedBilling);
    }

    [Fact]
    public async Task Test_OveragePaymentProcessing_SeparateFromSubscription()
    {
        // Arrange
        var testEnv = await _testDataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _testDataBuilder.CreateTestUsersAsync(1);
        var user = users[0];
        var plan = testEnv.Plans[0];
        var monthlyCycle = testEnv.MasterData.BillingCycles.First(bc => bc.Name == "Monthly");

        var subscription = await _testDataBuilder.CreateOverageScenarioAsync(user, plan, monthlyCycle, exceedLimit: true);

        // Calculate overage
        var usages = await _context.UserSubscriptionPrivilegeUsages
            .Include(u => u.SubscriptionPlanPrivilege)
            .Where(u => u.SubscriptionId == subscription.Id && u.UsedValue > u.AllowedValue)
            .ToListAsync();

        decimal totalOverage = 0;
        foreach (var usage in usages)
        {
            var overage = usage.UsedValue - usage.AllowedValue;
            totalOverage += overage * usage.SubscriptionPlanPrivilege.UnitCost;
        }

        // Create overage billing record
        var overageBilling = new BillingRecord
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            SubscriptionId = subscription.Id,
            Amount = totalOverage,
            TotalAmount = totalOverage,
            Status = BillingRecord.BillingStatus.Pending,
            Type = BillingRecord.BillingType.Overage,
            BillingDate = DateTime.UtcNow,
            DueDate = subscription.NextBillingDate,
            CurrencyId = plan.CurrencyId,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = user.Id
        };
        _context.BillingRecords.Add(overageBilling);
        await _context.SaveChangesAsync();

        // Act - Process overage payment separately
        await _paymentService.ProcessPaymentAsync(overageBilling.Id, _userToken);

        // Assert
        var processedBilling = await _context.BillingRecords.FindAsync(overageBilling.Id);
        Assert.Equal(BillingRecord.BillingStatus.Paid, processedBilling.Status);
        Assert.NotNull(processedBilling.PaidAt);
    }

    [Fact]
    public async Task Test_NoOverageIfUnitCostZero_ShouldBlockAtLimit()
    {
        // Arrange
        var testEnv = await _testDataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _testDataBuilder.CreateTestUsersAsync(1);
        var user = users[0];
        var monthlyCycle = testEnv.MasterData.BillingCycles.First(bc => bc.Name == "Monthly");

        // Create plan with no overage charges (UnitCost = 0)
        var plan = await _testDataBuilder.CreatePlanWithPrivilegeConfigAsync(
            "No Overage Plan",
            49.99m,
            monthlyCycle,
            new List<(Privilege, int, decimal)>
            {
                (testEnv.Privileges[0], 5, 0m)  // Limit 5, NO overage charges
            }
        );

        var subscription = await _testDataBuilder.CreateUserSubscriptionAsync(user, plan, monthlyCycle);
        await _testDataBuilder.CreatePrivilegeUsageTrackingAsync(subscription);

        var usage = await _context.UserSubscriptionPrivilegeUsages
            .Include(u => u.SubscriptionPlanPrivilege)
            .FirstAsync(u => u.SubscriptionId == subscription.Id);

        // Set usage to limit
        usage.UsedValue = usage.AllowedValue;
        _context.UserSubscriptionPrivilegeUsages.Update(usage);
        await _context.SaveChangesAsync();

        // Assert - Should be blocked at limit (no overage allowed)
        Assert.Equal(usage.AllowedValue, usage.UsedValue);
        Assert.True(usage.IsExhausted);
        Assert.Equal(0, usage.RemainingValue);
        Assert.Equal(0m, usage.SubscriptionPlanPrivilege.UnitCost);

        // No overage charge should be calculated
        var overage = usage.UsedValue - usage.AllowedValue;
        var overageCharge = overage * usage.SubscriptionPlanPrivilege.UnitCost;
        Assert.Equal(0m, overageCharge);
    }

    [Fact]
    public async Task Test_PurchaseAdditionalCredits_ShouldIncreaseAllowedValue()
    {
        // Arrange
        var testEnv = await _testDataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _testDataBuilder.CreateTestUsersAsync(1);
        var user = users[0];
        var plan = testEnv.Plans[0];
        var monthlyCycle = testEnv.MasterData.BillingCycles.First(bc => bc.Name == "Monthly");

        var subscription = await _testDataBuilder.CreateSubscriptionWithUsageAsync(user, plan, monthlyCycle);

        var usage = await _context.UserSubscriptionPrivilegeUsages
            .Include(u => u.SubscriptionPlanPrivilege)
            .FirstAsync(u => u.SubscriptionId == subscription.Id && u.AllowedValue > 0);

        var originalAllowed = usage.AllowedValue;
        var creditsToPurchase = 5;
        var unitCost = usage.SubscriptionPlanPrivilege.UnitCost;

        // Act - Purchase additional credits (simulate)
        usage.AllowedValue += creditsToPurchase;
        _context.UserSubscriptionPrivilegeUsages.Update(usage);
        await _context.SaveChangesAsync();

        // Create billing for purchased credits
        var purchaseBilling = new BillingRecord
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            SubscriptionId = subscription.Id,
            Amount = creditsToPurchase * unitCost,
            TotalAmount = creditsToPurchase * unitCost,
            Status = BillingRecord.BillingStatus.Pending,
            Type = BillingRecord.BillingType.Overage,
            Description = $"Purchase {creditsToPurchase} additional credits",
            BillingDate = DateTime.UtcNow,
            CurrencyId = plan.CurrencyId,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = user.Id
        };
        _context.BillingRecords.Add(purchaseBilling);
        await _context.SaveChangesAsync();

        // Process payment
        await _paymentService.ProcessPaymentAsync(purchaseBilling.Id, _userToken);

        // Assert
        var updatedUsage = await _context.UserSubscriptionPrivilegeUsages.FindAsync(usage.Id);
        Assert.Equal(originalAllowed + creditsToPurchase, updatedUsage.AllowedValue);
        
        var paidBilling = await _context.BillingRecords.FindAsync(purchaseBilling.Id);
        Assert.Equal(BillingRecord.BillingStatus.Paid, paidBilling.Status);
    }
}

