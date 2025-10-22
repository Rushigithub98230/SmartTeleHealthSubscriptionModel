using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Tests.Helpers;
using SmartTelehealth.Tests.Integration.Subscription;
using Xunit;

namespace SmartTelehealth.Tests.Integration.Privileges;

/// <summary>
/// Tests for privilege reset during billing cycle renewal
/// </summary>
[Trait("Category", "Privilege")]
public class PrivilegeResetTests : CleanTestBase
{
    [Fact]
    public async Task Test_ResetOnSuccessfulRenewalPayment_ShouldResetUsage()
    {
        // Arrange
        var testEnv = await _testDataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _testDataBuilder.CreateTestUsersAsync(1);
        var user = users[0];
        var plan = testEnv.Plans[0];
        var monthlyCycle = testEnv.MasterData.BillingCycles.First(bc => bc.Name == "Monthly");

        var subscription = await _testDataBuilder.CreateSubscriptionWithUsageAsync(user, plan, monthlyCycle, usedAmount: 5);
        
        var usagesBefore = await _context.UserSubscriptionPrivilegeUsages
            .Where(u => u.SubscriptionId == subscription.Id)
            .ToListAsync();

        // Verify there's usage before reset
        var usedUsage = usagesBefore.First(u => u.AllowedValue > 0);
        Assert.True(usedUsage.UsedValue > 0);

        // Act - Advance billing cycle and process payment
        await AdvanceTimeToBillingDateAsync(subscription);

        var billingRecord = new BillingRecord
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            SubscriptionId = subscription.Id,
            Amount = plan.Price,
            TotalAmount = plan.Price,
            Status = BillingRecord.BillingStatus.Pending,
            Type = BillingRecord.BillingType.Recurring,
            BillingDate = DateTime.UtcNow,
            DueDate = subscription.NextBillingDate,
            CurrencyId = plan.CurrencyId,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = user.Id
        };
        _context.BillingRecords.Add(billingRecord);
        await _context.SaveChangesAsync();

        // Process payment (this should trigger privilege reset)
        await _paymentService.ProcessPaymentAsync(billingRecord.Id, _userToken);

        // Assert
        var usagesAfter = await _context.UserSubscriptionPrivilegeUsages
            .Where(u => u.SubscriptionId == subscription.Id)
            .ToListAsync();

        var updatedSubscription = await _context.Subscriptions.FindAsync(subscription.Id);
        AssertionHelpers.AssertPrivilegesReset(usagesAfter, updatedSubscription);
    }

    [Fact]
    public async Task Test_ResetTiming_OnlyAfterPaymentSuccess_NotBefore()
    {
        // Arrange
        var testEnv = await _testDataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _testDataBuilder.CreateTestUsersAsync(1);
        var user = users[0];
        var plan = testEnv.Plans[0];
        var monthlyCycle = testEnv.MasterData.BillingCycles.First(bc => bc.Name == "Monthly");

        var subscription = await _testDataBuilder.CreateSubscriptionWithUsageAsync(user, plan, monthlyCycle, usedAmount: 3);
        await AdvanceTimeToBillingDateAsync(subscription);

        var usageBefore = await _context.UserSubscriptionPrivilegeUsages
            .FirstAsync(u => u.SubscriptionId == subscription.Id && u.AllowedValue > 0);
        var usedValueBefore = usageBefore.UsedValue;

        // Act - Billing date arrives but payment not processed yet
        // Assert - Usage should NOT be reset yet
        var usageAfterBillingDate = await _context.UserSubscriptionPrivilegeUsages.FindAsync(usageBefore.Id);
        Assert.Equal(usedValueBefore, usageAfterBillingDate.UsedValue);
        
        // Now process payment
        var billingRecord = new BillingRecord
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            SubscriptionId = subscription.Id,
            Amount = plan.Price,
            TotalAmount = plan.Price,
            Status = BillingRecord.BillingStatus.Pending,
            Type = BillingRecord.BillingType.Recurring,
            BillingDate = DateTime.UtcNow,
            DueDate = subscription.NextBillingDate,
            CurrencyId = plan.CurrencyId,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = user.Id
        };
        _context.BillingRecords.Add(billingRecord);
        await _context.SaveChangesAsync();

        await _paymentService.ProcessPaymentAsync(billingRecord.Id, _userToken);

        // Assert - Now usage should be reset
        var usageAfterPayment = await _context.UserSubscriptionPrivilegeUsages.FindAsync(usageBefore.Id);
        Assert.True(usageAfterPayment.UsedValue == 0 || usageAfterPayment.ResetAt.HasValue);
    }

    [Fact]
    public async Task Test_UsagePeriodUpdate_ShouldAlignWithNewBillingDates()
    {
        // Arrange
        var testEnv = await _testDataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _testDataBuilder.CreateTestUsersAsync(1);
        var user = users[0];
        var plan = testEnv.Plans[0];
        var monthlyCycle = testEnv.MasterData.BillingCycles.First(bc => bc.Name == "Monthly");

        var subscription = await _testDataBuilder.CreateSubscriptionWithUsageAsync(user, plan, monthlyCycle);
        
        var usageBefore = await _context.UserSubscriptionPrivilegeUsages
            .FirstAsync(u => u.SubscriptionId == subscription.Id);

        var oldPeriodStart = usageBefore.UsagePeriodStart;
        var oldPeriodEnd = usageBefore.UsagePeriodEnd;

        // Act - Process renewal
        await AdvanceTimeToBillingDateAsync(subscription);

        var billingRecord = new BillingRecord
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            SubscriptionId = subscription.Id,
            Amount = plan.Price,
            TotalAmount = plan.Price,
            Status = BillingRecord.BillingStatus.Pending,
            Type = BillingRecord.BillingType.Recurring,
            BillingDate = DateTime.UtcNow,
            DueDate = subscription.NextBillingDate,
            CurrencyId = plan.CurrencyId,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = user.Id
        };
        _context.BillingRecords.Add(billingRecord);
        await _context.SaveChangesAsync();

        await _paymentService.ProcessPaymentAsync(billingRecord.Id, _userToken);

        // Assert
        var usageAfter = await _context.UserSubscriptionPrivilegeUsages.FindAsync(usageBefore.Id);
        var updatedSubscription = await _context.Subscriptions.FindAsync(subscription.Id);

        // Period should be updated to match new billing dates
        Assert.True(usageAfter.UsagePeriodStart > oldPeriodStart || usageAfter.UsagePeriodStart == updatedSubscription.LastBillingDate);
        Assert.True(usageAfter.UsagePeriodEnd > oldPeriodEnd);
        Assert.Equal(updatedSubscription.NextBillingDate, usageAfter.UsagePeriodEnd);
    }

    [Fact]
    public async Task Test_ResetAtTimestamp_ShouldBeRecorded()
    {
        // Arrange
        var testEnv = await _testDataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _testDataBuilder.CreateTestUsersAsync(1);
        var user = users[0];
        var plan = testEnv.Plans[0];
        var monthlyCycle = testEnv.MasterData.BillingCycles.First(bc => bc.Name == "Monthly");

        var subscription = await _testDataBuilder.CreateSubscriptionWithUsageAsync(user, plan, monthlyCycle);
        
        var usageBefore = await _context.UserSubscriptionPrivilegeUsages
            .FirstAsync(u => u.SubscriptionId == subscription.Id);

        Assert.Null(usageBefore.ResetAt);

        // Act - Process renewal
        await AdvanceTimeToBillingDateAsync(subscription);

        var billingRecord = new BillingRecord
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            SubscriptionId = subscription.Id,
            Amount = plan.Price,
            TotalAmount = plan.Price,
            Status = BillingRecord.BillingStatus.Pending,
            Type = BillingRecord.BillingType.Recurring,
            BillingDate = DateTime.UtcNow,
            DueDate = subscription.NextBillingDate,
            CurrencyId = plan.CurrencyId,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = user.Id
        };
        _context.BillingRecords.Add(billingRecord);
        await _context.SaveChangesAsync();

        await _paymentService.ProcessPaymentAsync(billingRecord.Id, _userToken);

        // Assert
        var usageAfter = await _context.UserSubscriptionPrivilegeUsages.FindAsync(usageBefore.Id);
        Assert.NotNull(usageAfter.ResetAt);
        Assert.True((DateTime.UtcNow - usageAfter.ResetAt.Value).TotalMinutes < 5);
    }

    [Fact]
    public async Task Test_PurchasedExtraCreditsLost_OnReset()
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
        
        // Simulate purchasing extra credits
        usage.AllowedValue = originalAllowed + 5; // Purchased 5 more
        _context.UserSubscriptionPrivilegeUsages.Update(usage);
        await _context.SaveChangesAsync();

        Assert.Equal(originalAllowed + 5, usage.AllowedValue);

        // Act - Process renewal (reset)
        await AdvanceTimeToBillingDateAsync(subscription);

        var billingRecord = new BillingRecord
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            SubscriptionId = subscription.Id,
            Amount = plan.Price,
            TotalAmount = plan.Price,
            Status = BillingRecord.BillingStatus.Pending,
            Type = BillingRecord.BillingType.Recurring,
            BillingDate = DateTime.UtcNow,
            DueDate = subscription.NextBillingDate,
            CurrencyId = plan.CurrencyId,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = user.Id
        };
        _context.BillingRecords.Add(billingRecord);
        await _context.SaveChangesAsync();

        await _paymentService.ProcessPaymentAsync(billingRecord.Id, _userToken);

        // Assert - AllowedValue should reset to plan's Value (purchased credits lost)
        var usageAfterReset = await _context.UserSubscriptionPrivilegeUsages.FindAsync(usage.Id);
        Assert.Equal(usage.SubscriptionPlanPrivilege.Value, usageAfterReset.AllowedValue);
        Assert.NotEqual(originalAllowed + 5, usageAfterReset.AllowedValue);
    }

    [Fact]
    public async Task Test_ResetAcrossDifferentBillingCycles_ShouldWork()
    {
        // Arrange
        var testEnv = await _testDataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _testDataBuilder.CreateTestUsersAsync(3);
        
        var monthlyCycle = testEnv.MasterData.BillingCycles.First(bc => bc.Name == "Monthly");
        var quarterlyCycle = testEnv.MasterData.BillingCycles.First(bc => bc.Name == "Quarterly");
        var annualCycle = testEnv.MasterData.BillingCycles.First(bc => bc.Name == "Yearly");

        var subscriptions = new List<SmartTelehealth.Core.Entities.Subscription>
        {
            await _testDataBuilder.CreateSubscriptionWithUsageAsync(users[0], testEnv.Plans[0], monthlyCycle),
            await _testDataBuilder.CreateSubscriptionWithUsageAsync(users[1], testEnv.Plans[1], quarterlyCycle),
            await _testDataBuilder.CreateSubscriptionWithUsageAsync(users[2], testEnv.Plans[2], annualCycle)
        };

        // Act & Assert - Process renewal for each
        foreach (var subscription in subscriptions)
        {
            await AdvanceTimeToBillingDateAsync(subscription);

            var billingRecord = new BillingRecord
            {
                Id = Guid.NewGuid(),
                UserId = subscription.UserId,
                SubscriptionId = subscription.Id,
                Amount = subscription.CurrentPrice,
                TotalAmount = subscription.CurrentPrice,
                Status = BillingRecord.BillingStatus.Pending,
                Type = BillingRecord.BillingType.Recurring,
                BillingDate = DateTime.UtcNow,
                DueDate = subscription.NextBillingDate,
                CurrencyId = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow,
                CreatedBy = subscription.UserId
            };
            _context.BillingRecords.Add(billingRecord);
            await _context.SaveChangesAsync();

            await _paymentService.ProcessPaymentAsync(billingRecord.Id, _adminToken);

            // Verify reset worked for this cycle
            var usages = await _context.UserSubscriptionPrivilegeUsages
                .Where(u => u.SubscriptionId == subscription.Id)
                .ToListAsync();

            Assert.All(usages, usage =>
            {
                if (usage.AllowedValue > 0)
                {
                    Assert.True(usage.UsedValue == 0 || usage.ResetAt.HasValue);
                }
            });
        }
    }
}

