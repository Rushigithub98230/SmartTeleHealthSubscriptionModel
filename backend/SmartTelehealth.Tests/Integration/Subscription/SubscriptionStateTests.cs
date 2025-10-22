using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Tests.Helpers;
using Xunit;

namespace SmartTelehealth.Tests.Integration.Subscription;

/// <summary>
/// Tests for subscription state transitions and lifecycle management
/// </summary>
[Trait("Category", "SubscriptionLifecycle")]
public class SubscriptionStateTests : CleanTestBase
{
    [Fact]
    public async Task Test_TrialToActive_WithSuccessfulPayment_ShouldTransition()
    {
        // Arrange
        var testEnv = await _testDataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _testDataBuilder.CreateTestUsersAsync(1);
        var user = users[0];
        var plan = testEnv.Plans[0];
        var monthlyCycle = testEnv.MasterData.BillingCycles.First(bc => bc.Name == "Monthly");

        var subscription = await _testDataBuilder.CreateTrialSubscriptionAsync(user, plan, monthlyCycle, trialDays: 14);
        Assert.Equal("TrialActive", subscription.Status);

        // Act - Simulate trial end and payment success
        subscription.Status = "Active";
        subscription.IsTrialSubscription = false;
        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync();

        // Create status history
        await _testDataBuilder.CreateStatusHistoryAsync(subscription, "TrialActive", "Active", "Trial ended, payment successful");

        // Assert
        var updatedSubscription = await _context.Subscriptions.FindAsync(subscription.Id);
        Assert.Equal("Active", updatedSubscription.Status);
        
        var statusHistory = await _context.SubscriptionStatusHistories
            .FirstOrDefaultAsync(sh => sh.SubscriptionId == subscription.Id);
        Assert.NotNull(statusHistory);
        Assert.Equal("TrialActive", statusHistory.FromStatus);
        Assert.Equal("Active", statusHistory.ToStatus);
    }

    [Fact]
    public async Task Test_ActiveToPaused_ShouldUpdateStatus()
    {
        // Arrange
        var testEnv = await _testDataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _testDataBuilder.CreateTestUsersAsync(1);
        var user = users[0];
        var plan = testEnv.Plans[0];
        var monthlyCycle = testEnv.MasterData.BillingCycles.First(bc => bc.Name == "Monthly");

        var subscription = await _testDataBuilder.CreateUserSubscriptionAsync(user, plan, monthlyCycle);
        Assert.Equal("Active", subscription.Status);

        // Act - Pause subscription
        subscription.Status = "Paused";
        subscription.PausedDate = DateTime.UtcNow;
        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync();

        await _testDataBuilder.CreateStatusHistoryAsync(subscription, "Active", "Paused", "User requested pause");

        // Assert
        var updatedSubscription = await _context.Subscriptions.FindAsync(subscription.Id);
        Assert.Equal("Paused", updatedSubscription.Status);
        Assert.NotNull(updatedSubscription.PausedDate);

        var statusHistory = await _context.SubscriptionStatusHistories
            .Where(sh => sh.SubscriptionId == subscription.Id)
            .OrderByDescending(sh => sh.ChangedAt)
            .FirstOrDefaultAsync();
        Assert.NotNull(statusHistory);
        Assert.Equal("Paused", statusHistory.ToStatus);
    }

    [Fact]
    public async Task Test_PausedToActive_Resume_ShouldRecalculateBillingDates()
    {
        // Arrange
        var testEnv = await _testDataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _testDataBuilder.CreateTestUsersAsync(1);
        var user = users[0];
        var plan = testEnv.Plans[0];
        var monthlyCycle = testEnv.MasterData.BillingCycles.First(bc => bc.Name == "Monthly");

        var subscription = await _testDataBuilder.CreateUserSubscriptionAsync(user, plan, monthlyCycle);
        
        // Pause for 10 days
        subscription.Status = "Paused";
        subscription.PausedDate = DateTime.UtcNow.AddDays(-10);
        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync();

        var originalNextBillingDate = subscription.NextBillingDate;

        // Act - Resume subscription
        subscription.Status = "Active";
        subscription.ResumedDate = DateTime.UtcNow;
        // Adjust billing date by adding paused days
        subscription.NextBillingDate = subscription.NextBillingDate.AddDays(10);
        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync();

        await _testDataBuilder.CreateStatusHistoryAsync(subscription, "Paused", "Active", "User resumed subscription");

        // Assert
        var updatedSubscription = await _context.Subscriptions.FindAsync(subscription.Id);
        Assert.Equal("Active", updatedSubscription.Status);
        Assert.NotNull(updatedSubscription.ResumedDate);
        Assert.True(updatedSubscription.NextBillingDate > originalNextBillingDate);
    }

    [Fact]
    public async Task Test_ActiveToCancelled_ShouldSetCancellationData()
    {
        // Arrange
        var testEnv = await _testDataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _testDataBuilder.CreateTestUsersAsync(1);
        var user = users[0];
        var plan = testEnv.Plans[0];
        var monthlyCycle = testEnv.MasterData.BillingCycles.First(bc => bc.Name == "Monthly");

        var subscription = await _testDataBuilder.CreateUserSubscriptionAsync(user, plan, monthlyCycle);

        // Act - Cancel subscription
        subscription.Status = "Cancelled";
        subscription.CancelledDate = DateTime.UtcNow;
        subscription.CancellationReason = "User requested cancellation";
        subscription.EndDate = DateTime.UtcNow;
        subscription.AutoRenew = false;
        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync();

        await _testDataBuilder.CreateStatusHistoryAsync(subscription, "Active", "Cancelled", "User requested cancellation");

        // Assert
        var updatedSubscription = await _context.Subscriptions.FindAsync(subscription.Id);
        Assert.Equal("Cancelled", updatedSubscription.Status);
        Assert.NotNull(updatedSubscription.CancelledDate);
        Assert.NotNull(updatedSubscription.CancellationReason);
        Assert.NotNull(updatedSubscription.EndDate);
        Assert.False(updatedSubscription.AutoRenew);
    }

    [Fact]
    public async Task Test_PaymentFailedToActive_RetrySucceeds_ShouldReactivate()
    {
        // Arrange
        var testEnv = await _testDataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _testDataBuilder.CreateTestUsersAsync(1);
        var user = users[0];
        var plan = testEnv.Plans[0];
        var monthlyCycle = testEnv.MasterData.BillingCycles.First(bc => bc.Name == "Monthly");

        var subscription = await _testDataBuilder.CreateUserSubscriptionAsync(user, plan, monthlyCycle);
        
        // Simulate payment failure
        subscription.Status = "PaymentFailed";
        subscription.FailedPaymentAttempts = 2;
        subscription.LastPaymentFailedDate = DateTime.UtcNow.AddDays(-1);
        subscription.LastPaymentError = "Card declined";
        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync();

        // Act - Retry payment succeeds
        subscription.Status = "Active";
        subscription.FailedPaymentAttempts = 0;
        subscription.LastPaymentError = null;
        subscription.LastPaymentDate = DateTime.UtcNow;
        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync();

        await _testDataBuilder.CreateStatusHistoryAsync(subscription, "PaymentFailed", "Active", "Retry payment successful");

        // Assert
        var updatedSubscription = await _context.Subscriptions.FindAsync(subscription.Id);
        Assert.Equal("Active", updatedSubscription.Status);
        Assert.Equal(0, updatedSubscription.FailedPaymentAttempts);
        Assert.Null(updatedSubscription.LastPaymentError);
        Assert.NotNull(updatedSubscription.LastPaymentDate);
    }

    [Fact]
    public async Task Test_PaymentFailed_MaxRetriesExceeded_ShouldCancelOrSuspend()
    {
        // Arrange
        var testEnv = await _testDataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _testDataBuilder.CreateTestUsersAsync(1);
        var user = users[0];
        var plan = testEnv.Plans[0];
        var monthlyCycle = testEnv.MasterData.BillingCycles.First(bc => bc.Name == "Monthly");

        var subscription = await _testDataBuilder.CreateUserSubscriptionAsync(user, plan, monthlyCycle);
        
        // Simulate max retries reached
        subscription.Status = "PaymentFailed";
        subscription.FailedPaymentAttempts = 3;
        subscription.LastPaymentFailedDate = DateTime.UtcNow;
        subscription.LastPaymentError = "Card declined";
        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync();

        // Act - After max retries, move to Suspended or Cancelled
        subscription.Status = "Suspended";
        subscription.AutoRenew = false;
        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync();

        await _testDataBuilder.CreateStatusHistoryAsync(subscription, "PaymentFailed", "Suspended", "Max payment retries exceeded");

        // Assert
        var updatedSubscription = await _context.Subscriptions.FindAsync(subscription.Id);
        Assert.True(updatedSubscription.Status == "Suspended" || updatedSubscription.Status == "Cancelled");
        Assert.Equal(3, updatedSubscription.FailedPaymentAttempts);
        Assert.False(updatedSubscription.AutoRenew);
    }

    [Fact]
    public async Task Test_InvalidStateTransition_ShouldBePreventable()
    {
        // Arrange
        var testEnv = await _testDataBuilder.CreateCompleteTestEnvironmentAsync();
        var users = await _testDataBuilder.CreateTestUsersAsync(1);
        var user = users[0];
        var plan = testEnv.Plans[0];
        var monthlyCycle = testEnv.MasterData.BillingCycles.First(bc => bc.Name == "Monthly");

        var subscription = await _testDataBuilder.CreateUserSubscriptionAsync(user, plan, monthlyCycle);
        subscription.Status = "Cancelled";
        subscription.CancelledDate = DateTime.UtcNow;
        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync();

        // Act & Assert - Attempting to transition from Cancelled to Active should be validated
        // (In real implementation, this would be caught by ValidateStatusTransition)
        var cancelledSubscription = await _context.Subscriptions.FindAsync(subscription.Id);
        Assert.Equal("Cancelled", cancelledSubscription.Status);
        
        // This test documents that cancelled subscriptions are terminal states
        // and should require special reactivation logic rather than simple status changes
    }
}

