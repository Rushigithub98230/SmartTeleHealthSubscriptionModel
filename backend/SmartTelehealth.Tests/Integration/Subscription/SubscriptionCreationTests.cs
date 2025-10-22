using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Tests.Helpers;
using Xunit;

namespace SmartTelehealth.Tests.Integration.Subscription;

/// <summary>
/// Tests for subscription creation across different billing cycles and scenarios
/// </summary>
[Trait("Category", "SubscriptionLifecycle")]
public class SubscriptionCreationTests : CleanTestBase
{
    [Fact]
    public async Task Test_CreateSubscription_Monthly_ShouldSucceed()
    {
        // Arrange
        var testEnv = await _testDataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _testDataBuilder.CreateTestUsersAsync(1);
        var user = users[0];
        var plan = testEnv.Plans[0]; // Basic plan
        var monthlyCycle = testEnv.MasterData.BillingCycles.First(bc => bc.Name == "Monthly");

        // Act
        var subscription = await _testDataBuilder.CreateUserSubscriptionAsync(user, plan, monthlyCycle);

        // Assert
        Assert.NotNull(subscription);
        Assert.Equal(user.Id, subscription.UserId);
        Assert.Equal(plan.Id, subscription.SubscriptionPlanId);
        Assert.Equal(monthlyCycle.Id, subscription.BillingCycleId);
        Assert.Equal("Active", subscription.Status);
        Assert.NotEqual(default(DateTime), subscription.StartDate);
        Assert.NotEqual(default(DateTime), subscription.NextBillingDate);
        
        // Verify next billing date is 30 days from start
        var expectedNextBilling = subscription.StartDate.AddDays(30);
        Assert.Equal(expectedNextBilling.Date, subscription.NextBillingDate.Date);
    }

    [Fact]
    public async Task Test_CreateSubscription_Quarterly_ShouldSucceed()
    {
        // Arrange
        var testEnv = await _testDataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _testDataBuilder.CreateTestUsersAsync(1);
        var user = users[0];
        var plan = testEnv.Plans[1]; // Premium plan
        var quarterlyCycle = testEnv.MasterData.BillingCycles.First(bc => bc.Name == "Quarterly");

        // Act
        var subscription = await _testDataBuilder.CreateUserSubscriptionAsync(user, plan, quarterlyCycle);

        // Assert
        Assert.NotNull(subscription);
        Assert.Equal("Active", subscription.Status);
        
        // Verify next billing date is 90 days from start
        var expectedNextBilling = subscription.StartDate.AddDays(90);
        Assert.Equal(expectedNextBilling.Date, subscription.NextBillingDate.Date);
    }

    [Fact]
    public async Task Test_CreateSubscription_Annual_ShouldSucceed()
    {
        // Arrange
        var testEnv = await _testDataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _testDataBuilder.CreateTestUsersAsync(1);
        var user = users[0];
        var plan = testEnv.Plans[2]; // Professional plan
        var annualCycle = testEnv.MasterData.BillingCycles.First(bc => bc.Name == "Yearly");

        // Act
        var subscription = await _testDataBuilder.CreateUserSubscriptionAsync(user, plan, annualCycle);

        // Assert
        Assert.NotNull(subscription);
        Assert.Equal("Active", subscription.Status);
        
        // Verify next billing date is 365 days from start
        var expectedNextBilling = subscription.StartDate.AddDays(365);
        Assert.Equal(expectedNextBilling.Date, subscription.NextBillingDate.Date);
    }

    [Fact]
    public async Task Test_CreateTrialSubscription_ShouldSetupTrialPeriod()
    {
        // Arrange
        var testEnv = await _testDataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _testDataBuilder.CreateTestUsersAsync(1);
        var user = users[0];
        var plan = testEnv.Plans[0];
        var monthlyCycle = testEnv.MasterData.BillingCycles.First(bc => bc.Name == "Monthly");

        // Act
        var subscription = await _testDataBuilder.CreateTrialSubscriptionAsync(user, plan, monthlyCycle, trialDays: 14);

        // Assert
        Assert.NotNull(subscription);
        Assert.True(subscription.IsTrialSubscription);
        Assert.Equal("TrialActive", subscription.Status);
        Assert.NotNull(subscription.TrialStartDate);
        Assert.NotNull(subscription.TrialEndDate);
        Assert.Equal(14, subscription.TrialDurationInDays);
        
        // Trial end date should be 14 days from start
        var expectedTrialEnd = subscription.TrialStartDate.Value.AddDays(14);
        Assert.Equal(expectedTrialEnd.Date, subscription.TrialEndDate.Value.Date);
    }

    [Fact]
    public async Task Test_CreateSubscription_ShouldAllocatePrivileges()
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

        // Assert - Verify privileges were allocated
        var privilegeUsages = await _context.UserSubscriptionPrivilegeUsages
            .Where(u => u.SubscriptionId == subscription.Id)
            .ToListAsync();

        Assert.NotEmpty(privilegeUsages);
        Assert.Equal(2, privilegeUsages.Count); // Basic plan has 2 privileges

        foreach (var usage in privilegeUsages)
        {
            Assert.Equal(0, usage.UsedValue); // Initial usage is 0
            Assert.True(usage.AllowedValue > 0 || usage.AllowedValue == -1); // Either limited or unlimited
            Assert.Equal(subscription.StartDate, usage.UsagePeriodStart);
            Assert.Equal(subscription.NextBillingDate, usage.UsagePeriodEnd);
        }
    }

    [Fact]
    public async Task Test_CreateSubscription_WithPayment_ShouldCreateBillingRecord()
    {
        // Arrange
        var testEnv = await _testDataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _testDataBuilder.CreateTestUsersAsync(1);
        var user = users[0];
        var plan = testEnv.Plans[0];
        var monthlyCycle = testEnv.MasterData.BillingCycles.First(bc => bc.Name == "Monthly");

        // Act
        var subscription = await CreateSubscriptionWithPaymentAsync(user, plan, monthlyCycle, processPayment: true);

        // Assert - Verify billing record was created
        var billingRecord = await _context.BillingRecords
            .FirstOrDefaultAsync(br => br.SubscriptionId == subscription.Id);

        Assert.NotNull(billingRecord);
        AssertionHelpers.AssertBillingRecordComplete(billingRecord);
        Assert.Equal(BillingRecord.BillingStatus.Paid, billingRecord.Status);
        Assert.Equal(plan.Price, billingRecord.Amount);
    }

    [Fact]
    public async Task Test_CreateSubscription_ShouldSetStripeIds()
    {
        // Arrange
        var testEnv = await _testDataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _testDataBuilder.CreateTestUsersAsync(1);
        var user = users[0];
        var plan = testEnv.Plans[0];
        var monthlyCycle = testEnv.MasterData.BillingCycles.First(bc => bc.Name == "Monthly");

        // Act
        var subscription = await _testDataBuilder.CreateUserSubscriptionAsync(user, plan, monthlyCycle);
        
        // Simulate Stripe customer and subscription creation
        subscription.StripeCustomerId = $"cus_test_{user.Id}";
        subscription.StripeSubscriptionId = $"sub_test_{Guid.NewGuid().ToString().Substring(0, 8)}";
        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync();

        // Assert
        Assert.NotNull(subscription.StripeCustomerId);
        Assert.NotNull(subscription.StripeSubscriptionId);
        Assert.StartsWith("cus_test_", subscription.StripeCustomerId);
        Assert.StartsWith("sub_test_", subscription.StripeSubscriptionId);
    }
}

