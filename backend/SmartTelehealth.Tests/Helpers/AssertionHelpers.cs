using SmartTelehealth.Core.Entities;
using Xunit;

namespace SmartTelehealth.Tests.Helpers;

/// <summary>
/// Custom assertion helpers for subscription testing
/// </summary>
public static class AssertionHelpers
{
    /// <summary>
    /// Asserts that a subscription is in the expected state
    /// </summary>
    public static void AssertSubscriptionInState(Subscription subscription, string expectedStatus)
    {
        Assert.NotNull(subscription);
        Assert.Equal(expectedStatus, subscription.Status);
    }

    /// <summary>
    /// Asserts that all privileges have been reset correctly
    /// </summary>
    public static void AssertPrivilegesReset(List<UserSubscriptionPrivilegeUsage> usages, Subscription subscription)
    {
        Assert.NotNull(usages);
        Assert.NotEmpty(usages);

        foreach (var usage in usages)
        {
            // Check UsedValue is reset to 0 (except for unlimited)
            if (usage.AllowedValue != -1)
            {
                Assert.Equal(0, usage.UsedValue);
            }

            // Check usage period aligns with billing period
            Assert.Equal(subscription.LastBillingDate ?? subscription.StartDate, usage.UsagePeriodStart);
            Assert.Equal(subscription.NextBillingDate, usage.UsagePeriodEnd);

            // Check ResetAt is populated
            Assert.NotNull(usage.ResetAt);
        }
    }

    /// <summary>
    /// Asserts that a billing record is complete and valid
    /// </summary>
    public static void AssertBillingRecordComplete(BillingRecord billingRecord)
    {
        Assert.NotNull(billingRecord);
        Assert.NotEqual(Guid.Empty, billingRecord.Id);
        Assert.True(billingRecord.Amount > 0);
        Assert.True(billingRecord.TotalAmount > 0);
        Assert.NotNull(billingRecord.InvoiceNumber);
        Assert.NotEqual(default(DateTime), billingRecord.BillingDate);
    }

    /// <summary>
    /// Asserts that a payment has been processed correctly
    /// </summary>
    public static void AssertPaymentProcessed(BillingRecord billingRecord, Subscription subscription)
    {
        Assert.NotNull(billingRecord);
        Assert.Equal(BillingRecord.BillingStatus.Paid, billingRecord.Status);
        Assert.NotNull(billingRecord.PaidAt);
        
        // Subscription should be active after successful payment
        Assert.Equal("Active", subscription.Status);
        
        // Subscription dates should be updated
        Assert.NotNull(subscription.LastBillingDate);
        Assert.True(subscription.NextBillingDate > subscription.LastBillingDate);
    }

    /// <summary>
    /// Asserts that privilege allocation is correct
    /// </summary>
    public static void AssertPrivilegeAllocation(
        UserSubscriptionPrivilegeUsage usage,
        SubscriptionPlanPrivilege planPrivilege,
        Subscription subscription)
    {
        Assert.NotNull(usage);
        Assert.Equal(subscription.Id, usage.SubscriptionId);
        Assert.Equal(planPrivilege.Id, usage.SubscriptionPlanPrivilegeId);
        Assert.Equal(planPrivilege.PrivilegeId, usage.PrivilegeId);
        
        // Check allowed value matches plan configuration
        Assert.Equal(planPrivilege.Value, usage.AllowedValue);
        
        // Check initial state
        Assert.Equal(0, usage.UsedValue);
        
        // Check period alignment
        Assert.Equal(subscription.LastBillingDate ?? subscription.StartDate, usage.UsagePeriodStart);
        Assert.Equal(subscription.NextBillingDate, usage.UsagePeriodEnd);
    }

    /// <summary>
    /// Asserts that privilege usage is within limits
    /// </summary>
    public static void AssertPrivilegeUsageWithinLimits(UserSubscriptionPrivilegeUsage usage)
    {
        Assert.NotNull(usage);
        
        if (usage.AllowedValue == -1) // Unlimited
        {
            Assert.True(usage.IsUnlimited);
            return;
        }

        Assert.True(usage.UsedValue <= usage.AllowedValue, 
            $"Usage ({usage.UsedValue}) should not exceed allowed value ({usage.AllowedValue})");
        Assert.False(usage.IsExhausted);
    }

    /// <summary>
    /// Asserts that a privilege is exhausted
    /// </summary>
    public static void AssertPrivilegeExhausted(UserSubscriptionPrivilegeUsage usage)
    {
        Assert.NotNull(usage);
        Assert.False(usage.IsUnlimited);
        Assert.True(usage.IsExhausted);
        Assert.True(usage.UsedValue >= usage.AllowedValue);
        Assert.Equal(0, usage.RemainingValue);
    }

    /// <summary>
    /// Asserts that subscription dates are consistent
    /// </summary>
    public static void AssertSubscriptionDatesConsistent(Subscription subscription)
    {
        Assert.NotNull(subscription);
        Assert.NotEqual(default(DateTime), subscription.StartDate);
        Assert.NotEqual(default(DateTime), subscription.NextBillingDate);
        
        // NextBillingDate should be after StartDate
        Assert.True(subscription.NextBillingDate > subscription.StartDate);
        
        // If LastBillingDate exists, it should be before NextBillingDate
        if (subscription.LastBillingDate.HasValue)
        {
            Assert.True(subscription.NextBillingDate > subscription.LastBillingDate.Value);
        }
    }

    /// <summary>
    /// Asserts that a subscription payment record is valid
    /// </summary>
    public static void AssertSubscriptionPaymentValid(SubscriptionPayment payment, Subscription subscription, BillingRecord billingRecord)
    {
        Assert.NotNull(payment);
        Assert.Equal(subscription.Id, payment.SubscriptionId);
        Assert.Equal(billingRecord.Id, payment.BillingRecordId);
        Assert.Equal(billingRecord.Amount, payment.Amount);
        Assert.Equal(billingRecord.TotalAmount, payment.NetAmount);
        Assert.NotNull(payment.StripePaymentIntentId);
        
        // Check billing period
        Assert.Equal(subscription.LastBillingDate ?? subscription.StartDate, payment.BillingPeriodStart);
        Assert.Equal(subscription.NextBillingDate, payment.BillingPeriodEnd);
    }

    /// <summary>
    /// Asserts that overage charges are calculated correctly
    /// </summary>
    public static void AssertOverageCalculation(
        UserSubscriptionPrivilegeUsage usage,
        SubscriptionPlanPrivilege planPrivilege,
        decimal expectedCharge)
    {
        Assert.NotNull(usage);
        Assert.False(usage.IsUnlimited);
        Assert.True(usage.UsedValue > usage.AllowedValue, "Usage must exceed allowed value for overage");
        
        var overage = usage.UsedValue - usage.AllowedValue;
        var calculatedCharge = overage * planPrivilege.UnitCost;
        
        Assert.Equal(expectedCharge, calculatedCharge);
    }

    /// <summary>
    /// Asserts that billing record amounts are calculated correctly
    /// </summary>
    public static void AssertBillingAmountsCorrect(BillingRecord billingRecord)
    {
        Assert.NotNull(billingRecord);
        
        // Total should equal Amount + TaxAmount + ShippingAmount
        var expectedTotal = billingRecord.Amount + billingRecord.TaxAmount + billingRecord.ShippingAmount;
        Assert.Equal(expectedTotal, billingRecord.TotalAmount);
        
        // All amounts should be non-negative
        Assert.True(billingRecord.Amount >= 0);
        Assert.True(billingRecord.TaxAmount >= 0);
        Assert.True(billingRecord.ShippingAmount >= 0);
        Assert.True(billingRecord.TotalAmount >= 0);
    }

    /// <summary>
    /// Asserts that a subscription status history entry is valid
    /// </summary>
    public static void AssertStatusHistoryValid(SubscriptionStatusHistory history, Subscription subscription)
    {
        Assert.NotNull(history);
        Assert.Equal(subscription.Id, history.SubscriptionId);
        Assert.NotNull(history.FromStatus);
        Assert.NotNull(history.ToStatus);
        Assert.NotEqual(default(DateTime), history.ChangedAt);
    }
}

