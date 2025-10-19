using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Application.Utilities;

/// <summary>
/// Centralized utility class for privilege allocation calculations.
/// Ensures consistent privilege allocation logic across the entire application.
/// </summary>
public static class PrivilegeAllocationCalculator
{
    /// <summary>
    /// Returns the privilege value as-is without any calculation.
    /// The Value field represents the TOTAL allowed count set by the admin.
    /// 
    /// IMPORTANT: Monthly/Weekly/Daily limits are OPTIONAL rate limiters, not for calculating total.
    /// - Value field = Total privilege count for the billing cycle (set by admin)
    /// - MonthlyLimit = Optional rate limiter (max per month)
    /// - WeeklyLimit = Optional rate limiter (max per week)
    /// - DailyLimit = Optional rate limiter (max per day)
    /// 
    /// Example: Value = 152 means user gets 152 total for the billing cycle.
    /// </summary>
    /// <param name="privilegeValue">The total privilege count (or -1 for unlimited)</param>
    /// <returns>The privilege value as-is (-1 for unlimited, 0 for disabled)</returns>
    [Obsolete("This method is no longer needed. Use planPrivilege.Value directly instead.")]
    public static int CalculateAllowedForCycle(int privilegeValue, int billingCycleDays)
    {
        // Simply return the value as-is
        // Unlimited privileges
        if (privilegeValue == -1)
            return -1;

        // Disabled privileges
        if (privilegeValue == 0)
            return 0;

        // Return the admin-set value directly (no calculation needed)
        return privilegeValue;
    }

    /// <summary>
    /// Calculates the usage period for a subscription's privilege allocation.
    /// Period aligns with the subscription's billing period.
    /// </summary>
    /// <param name="subscription">The subscription</param>
    /// <returns>Tuple of (periodStart, periodEnd)</returns>
    public static (DateTime periodStart, DateTime periodEnd) CalculateUsagePeriod(Subscription subscription)
    {
        // Period starts at LastBillingDate (start of current billing period)
        // For new subscriptions (no LastBillingDate), use StartDate
        var periodStart = subscription.LastBillingDate ?? subscription.StartDate;
        
        // Period ends at NextBillingDate (when next billing occurs)
        var periodEnd = subscription.NextBillingDate;

        return (periodStart, periodEnd);
    }

    /// <summary>
    /// Gets the privilege allocation using the admin-set Value directly.
    /// CORRECTED: Now uses planPrivilege.Value as the total allowed count (no calculation).
    /// 
    /// The Value field represents the TOTAL privilege count set by admin for the billing cycle.
    /// Monthly/Weekly/Daily limits are optional rate limiters checked separately.
    /// </summary>
    /// <param name="subscription">The subscription</param>
    /// <param name="planPrivilege">The plan privilege configuration</param>
    /// <returns>Tuple of (allowedValue, periodStart, periodEnd)</returns>
    public static (int allowedValue, DateTime periodStart, DateTime periodEnd) CalculatePrivilegeAllocation(
        Subscription subscription,
        SubscriptionPlanPrivilege planPrivilege)
    {
        // CORRECTED: Use the admin-set Value directly (total privilege count)
        // No calculation needed - the admin explicitly sets the total allowed count
        var allowedValue = planPrivilege.Value;

        // Calculate usage period (aligns with subscription billing cycle)
        var (periodStart, periodEnd) = CalculateUsagePeriod(subscription);

        return (allowedValue, periodStart, periodEnd);
    }

    /// <summary>
    /// Validates if a privilege can be used based on current usage.
    /// </summary>
    /// <param name="usedValue">Current usage count</param>
    /// <param name="allowedValue">Allowed usage count (-1 for unlimited)</param>
    /// <param name="requestedAmount">Amount requested to use</param>
    /// <returns>True if privilege can be used, false otherwise</returns>
    public static bool CanUsePrivilege(int usedValue, int allowedValue, int requestedAmount)
    {
        // Unlimited privileges
        if (allowedValue == -1)
            return true;

        // Disabled privileges
        if (allowedValue == 0)
            return false;

        // Check if enough remaining
        var remaining = allowedValue - usedValue;
        return remaining >= requestedAmount;
    }

    /// <summary>
    /// Calculates remaining privilege usage.
    /// </summary>
    /// <param name="usedValue">Current usage count</param>
    /// <param name="allowedValue">Allowed usage count (-1 for unlimited)</param>
    /// <returns>Remaining usage count (int.MaxValue for unlimited)</returns>
    public static int CalculateRemainingValue(int usedValue, int allowedValue)
    {
        if (allowedValue == -1)
            return int.MaxValue;  // Unlimited

        return Math.Max(0, allowedValue - usedValue);
    }

    /// <summary>
    /// Determines if a privilege is unlimited.
    /// </summary>
    /// <param name="allowedValue">The allowed value</param>
    /// <returns>True if unlimited, false otherwise</returns>
    public static bool IsUnlimitedPrivilege(int allowedValue)
    {
        return allowedValue == -1;
    }

    /// <summary>
    /// Determines if a privilege is disabled.
    /// </summary>
    /// <param name="allowedValue">The allowed value</param>
    /// <returns>True if disabled, false otherwise</returns>
    public static bool IsDisabledPrivilege(int allowedValue)
    {
        return allowedValue == 0;
    }
}


