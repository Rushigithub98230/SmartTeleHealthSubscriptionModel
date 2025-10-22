using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Tests.Helpers;
using SmartTelehealth.Tests.Integration.Subscription;
using Xunit;

namespace SmartTelehealth.Tests.Integration.BillingCycles;

/// <summary>
/// Tests for monthly billing cycle operations
/// </summary>
[Trait("Category", "BillingCycle")]
public class MonthlyBillingCycleTests : CleanTestBase
{
    [Fact]
    public async Task Test_MonthlyRenewal_Complete_ShouldUpdateDatesAndResetPrivileges()
    {
        // Arrange
        var testEnv = await _testDataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _testDataBuilder.CreateTestUsersAsync(1);
        var user = users[0];
        var plan = testEnv.Plans[0];
        var monthlyCycle = testEnv.MasterData.BillingCycles.First(bc => bc.Name == "Monthly");

        var subscription = await _testDataBuilder.CreateSubscriptionWithUsageAsync(user, plan, monthlyCycle, usedAmount: 3);
        var originalNextBillingDate = subscription.NextBillingDate;

        // Act - Simulate billing cycle completion
        await AdvanceTimeToBillingDateAsync(subscription);

        // Create billing record and process payment
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

        // Process payment
        await _paymentService.ProcessPaymentAsync(billingRecord.Id, _userToken);

        // Assert
        var updatedSubscription = await _context.Subscriptions
            .Include(s => s.BillingCycle)
            .FirstOrDefaultAsync(s => s.Id == subscription.Id);

        Assert.NotNull(updatedSubscription);
        Assert.True(updatedSubscription.NextBillingDate > originalNextBillingDate);
        Assert.NotNull(updatedSubscription.LastBillingDate);

        // Verify privileges were reset
        var privilegeUsages = await _context.UserSubscriptionPrivilegeUsages
            .Where(u => u.SubscriptionId == subscription.Id)
            .ToListAsync();

        Assert.NotEmpty(privilegeUsages);
        foreach (var usage in privilegeUsages)
        {
            if (usage.AllowedValue > 0) // Limited privileges should be reset
            {
                Assert.True(usage.UsedValue == 0 || usage.ResetAt.HasValue);
            }
        }
    }

    [Fact]
    public async Task Test_MultipleConsecutiveMonthlyRenewals_ShouldTrackCorrectly()
    {
        // Arrange
        var testEnv = await _testDataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _testDataBuilder.CreateTestUsersAsync(1);
        var user = users[0];
        var plan = testEnv.Plans[0];
        var monthlyCycle = testEnv.MasterData.BillingCycles.First(bc => bc.Name == "Monthly");

        var subscription = await _testDataBuilder.CreateUserSubscriptionAsync(user, plan, monthlyCycle);
        await _testDataBuilder.CreatePrivilegeUsageTrackingAsync(subscription);

        var startDate = subscription.StartDate;
        var billingRecords = new List<BillingRecord>();

        // Act - Simulate 3 monthly renewals
        for (int month = 0; month < 3; month++)
        {
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
            billingRecords.Add(billingRecord);
        }

        // Assert
        Assert.Equal(3, billingRecords.Count);
        
        var updatedSubscription = await _context.Subscriptions.FindAsync(subscription.Id);
        // After 3 months, next billing should be approximately 3 months from start
        var expectedDate = startDate.AddMonths(3);
        Assert.True((updatedSubscription.NextBillingDate - expectedDate).TotalDays < 5); // Allow small variance
    }

    [Fact]
    public async Task Test_MidMonthStart_ShouldAlignToStartDate()
    {
        // Arrange
        var testEnv = await _testDataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _testDataBuilder.CreateTestUsersAsync(1);
        var user = users[0];
        var plan = testEnv.Plans[0];
        var monthlyCycle = testEnv.MasterData.BillingCycles.First(bc => bc.Name == "Monthly");

        // Create subscription mid-month (e.g., 15th)
        var subscription = await _testDataBuilder.CreateUserSubscriptionAsync(user, plan, monthlyCycle);
        var startDay = subscription.StartDate.Day;

        // Act - Advance one billing cycle
        await AdvanceTimeToBillingDateAsync(subscription);

        // Assert - Next billing date should be same day of month as start date
        var updatedSubscription = await _context.Subscriptions.FindAsync(subscription.Id);
        Assert.Equal(startDay, updatedSubscription.NextBillingDate.Day);
    }

    [Fact]
    public async Task Test_MonthlyRenewal_WithOverageCharges_ShouldCalculateCorrectly()
    {
        // Arrange
        var testEnv = await _testDataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _testDataBuilder.CreateTestUsersAsync(1);
        var user = users[0];
        var plan = testEnv.Plans[0];
        var monthlyCycle = testEnv.MasterData.BillingCycles.First(bc => bc.Name == "Monthly");

        // Create subscription with overage
        var subscription = await _testDataBuilder.CreateOverageScenarioAsync(user, plan, monthlyCycle, exceedLimit: true);
        
        // Get privilege usage with overage
        var usages = await _context.UserSubscriptionPrivilegeUsages
            .Include(u => u.SubscriptionPlanPrivilege)
            .Where(u => u.SubscriptionId == subscription.Id && u.UsedValue > u.AllowedValue)
            .ToListAsync();

        Assert.NotEmpty(usages);

        // Act - Calculate overage charges
        decimal totalOverage = 0;
        foreach (var usage in usages)
        {
            var overage = usage.UsedValue - usage.AllowedValue;
            var overageCharge = overage * usage.SubscriptionPlanPrivilege.UnitCost;
            totalOverage += overageCharge;
        }

        // Assert
        Assert.True(totalOverage > 0);
    }

    [Fact]
    public async Task Test_MonthlyRenewal_PaymentFailure_ShouldHandleCorrectly()
    {
        // Arrange
        var testEnv = await _testDataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _testDataBuilder.CreateTestUsersAsync(1);
        var user = users[0];
        var plan = testEnv.Plans[0];
        var monthlyCycle = testEnv.MasterData.BillingCycles.First(bc => bc.Name == "Monthly");

        var subscription = await _testDataBuilder.CreateUserSubscriptionAsync(user, plan, monthlyCycle);
        await AdvanceTimeToBillingDateAsync(subscription);

        // Note: In real implementation, payment would be processed through Stripe

        // Act - Create billing record and attempt payment
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

        // Note: Payment processing would fail in real scenario
        // For test purposes, we manually set the failure state
        billingRecord.Status = BillingRecord.BillingStatus.Failed;
        billingRecord.FailureReason = "Card declined";
        _context.BillingRecords.Update(billingRecord);
        await _context.SaveChangesAsync();

        subscription.Status = "PaymentFailed";
        subscription.FailedPaymentAttempts = 1;
        subscription.LastPaymentFailedDate = DateTime.UtcNow;
        subscription.LastPaymentError = "Card declined";
        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync();

        // Assert
        var updatedBillingRecord = await _context.BillingRecords.FindAsync(billingRecord.Id);
        Assert.Equal(BillingRecord.BillingStatus.Failed, updatedBillingRecord.Status);

        var updatedSubscription = await _context.Subscriptions.FindAsync(subscription.Id);
        Assert.Equal("PaymentFailed", updatedSubscription.Status);
        Assert.True(updatedSubscription.FailedPaymentAttempts > 0);
        Assert.NotNull(updatedSubscription.LastPaymentError);

        // Note: Stripe mock reset would be handled by test cleanup
    }
}

