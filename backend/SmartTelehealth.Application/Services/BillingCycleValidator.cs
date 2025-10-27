using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Application.Services;

/// <summary>
/// Validates billing cycle selections for subscription plans
/// Prevents inappropriate billing cycle combinations
/// </summary>
public static class BillingCycleValidator
{
    /// <summary>
    /// Determines if a billing cycle is valid for a given subscription plan
    /// </summary>
    /// <param name="plan">The subscription plan</param>
    /// <param name="billingCycle">The billing cycle to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValidBillingCycleForPlan(SubscriptionPlan plan, MasterBillingCycle billingCycle)
    {
        // Validation rules
        var planMonthlyPrice = plan.BasePrice;
        var billingCycleMonths = billingCycle.DurationInDays / 30.0m;
        
        // Prevent billing cycles that don't make business sense
        // Example: Don't allow daily billing for expensive plans (creates too many transactions)
        if (billingCycle.Name.Equals("Daily", StringComparison.OrdinalIgnoreCase) && planMonthlyPrice > 50)
            return false;
        
        // Allow common combinations
        return billingCycle.Name.ToLower() switch
        {
            "monthly" => true,                          // Always allowed
            "quarterly" => true,                        // Always allowed
            "annual" => true,                           // Always allowed (ONLY "annual" - database standard)
            "weekly" => planMonthlyPrice <= 100,       // Only for lower-cost plans
            "daily" => planMonthlyPrice <= 50,         // Only for very low-cost plans
            _ => false
        };
    }
}

