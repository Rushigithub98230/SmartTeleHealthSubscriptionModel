using SmartTelehealth.Core.Entities;
using Microsoft.Extensions.Logging;

namespace SmartTelehealth.Application.Utilities;

/// <summary>
/// Centralized utility class for all billing cycle calculations.
/// This ensures consistency across the entire application and eliminates duplicate logic.
/// 
/// STANDARD BILLING CYCLE KEYWORDS (Must match MasterBillingCycle.Name in database):
/// - "monthly" (30 days)
/// - "quarterly" (90 days)
/// - "annual" (365 days) ← ONLY THIS TERM (Database uses "Annual")
/// - "weekly" (7 days)
/// - "daily" (1 day)
/// 
/// NOTE: Database MasterBillingCycle table uses "Annual" (capital A).
/// This code handles case-insensitive matching via .ToLower().
/// </summary>
public static class BillingCycleCalculator
{
    /// <summary>
    /// Calculates the next billing date from a base date and billing cycle.
    /// This is the SINGLE SOURCE OF TRUTH for all next billing date calculations.
    /// IMPORTANT: Only "annual" is supported (database standard). No "yearly" or "annually".
    /// </summary>
    /// <param name="baseDate">The base date to calculate from (typically LastBillingDate or StartDate)</param>
    /// <param name="billingCycle">The billing cycle configuration</param>
    /// <returns>The next billing date</returns>
    public static DateTime CalculateNextBillingDate(DateTime baseDate, MasterBillingCycle? billingCycle)
    {
        if (billingCycle == null)
        {
            return baseDate.AddMonths(1); // Default to monthly
        }

        return billingCycle.Name?.ToLower() switch
        {
            "monthly" => baseDate.AddMonths(1),           // 30 days
            "quarterly" => baseDate.AddMonths(3),         // 90 days
            "annual" => baseDate.AddYears(1),             // 365 days (ONLY VALID TERM)
            "weekly" => baseDate.AddDays(7),              // 7 days
            "daily" => baseDate.AddDays(1),               // 1 day
            _ => baseDate.AddDays(billingCycle.DurationInDays)  // Fallback to duration
        };
    }

    /// <summary>
    /// Calculates the end date of a billing period from a start date and billing cycle.
    /// Returns the last day of the period (inclusive).
    /// IMPORTANT: Only "annual" is supported (database standard).
    /// </summary>
    /// <param name="startDate">The start date of the billing period</param>
    /// <param name="billingCycle">The billing cycle configuration</param>
    /// <returns>The end date of the billing period</returns>
    public static DateTime CalculateEndDateForCycle(DateTime startDate, MasterBillingCycle? billingCycle)
    {
        if (billingCycle == null)
        {
            return startDate.AddMonths(1).AddDays(-1);
        }

        return billingCycle.Name?.ToLower() switch
        {
            "monthly" => startDate.AddMonths(1).AddDays(-1),      // Last day of 30-day period
            "quarterly" => startDate.AddMonths(3).AddDays(-1),    // Last day of 90-day period
            "annual" => startDate.AddYears(1).AddDays(-1),        // Last day of 365-day period (ONLY VALID TERM)
            "weekly" => startDate.AddDays(7).AddDays(-1),         // Last day of 7-day period
            "daily" => startDate,                                  // Same day
            _ => startDate.AddDays(billingCycle.DurationInDays).AddDays(-1)
        };
    }

    /// <summary>
    /// Calculates the number of months in a billing cycle.
    /// Uses standard 30-day month approximation for consistency.
    /// </summary>
    /// <param name="billingCycleDays">Number of days in the billing cycle</param>
    /// <returns>Number of months (decimal) in the billing cycle</returns>
    public static decimal CalculateMonthsInCycle(int billingCycleDays)
    {
        return billingCycleDays / 30.0m;
    }

    /// <summary>
    /// Scales a monthly price to a billing cycle duration with proper rounding.
    /// </summary>
    /// <param name="monthlyPrice">The base monthly price</param>
    /// <param name="billingCycle">The billing cycle configuration</param>
    /// <returns>The price scaled to the billing cycle duration</returns>
    public static decimal ScalePriceToBillingCycle(decimal monthlyPrice, MasterBillingCycle billingCycle)
    {
        var monthsInCycle = CalculateMonthsInCycle(billingCycle.DurationInDays);
        return monthlyPrice * monthsInCycle;
    }

    /// <summary>
    /// Calculates the billing cycle discount amount based on plan configuration.
    /// IMPORTANT: Only "annual" is supported (database standard).
    /// </summary>
    /// <param name="plan">The subscription plan with discount percentages</param>
    /// <param name="billingCycle">The billing cycle being used</param>
    /// <param name="basePrice">The base price before discount</param>
    /// <returns>The discount amount to subtract from base price</returns>
    public static decimal CalculateBillingCycleDiscount(
        SubscriptionPlan plan, 
        MasterBillingCycle billingCycle, 
        decimal basePrice)
    {
        var discountPercent = billingCycle.Name?.ToLower() switch
        {
            "annual" => plan.AnnualBillingDiscount,       // ONLY VALID TERM (matches database)
            "quarterly" => plan.QuarterlyBillingDiscount,
            "monthly" => plan.MonthlyBillingDiscount,
            _ => 0m
        };

        return basePrice * (discountPercent / 100);
    }

    /// <summary>
    /// Calculates the final subscription price including scaling and discount.
    /// This is the complete price calculation in one method.
    /// </summary>
    /// <param name="plan">The subscription plan</param>
    /// <param name="billingCycle">The billing cycle</param>
    /// <returns>The final price to charge</returns>
    public static decimal CalculateSubscriptionPrice(
        SubscriptionPlan plan, 
        MasterBillingCycle billingCycle)
    {
        var basePrice = ScalePriceToBillingCycle(plan.Price, billingCycle);
        var discount = CalculateBillingCycleDiscount(plan, billingCycle, basePrice);
        return basePrice - discount;
    }

    /// <summary>
    /// Extends a date by the duration of a billing cycle.
    /// Used for extending EndDate during renewals.
    /// </summary>
    /// <param name="currentDate">The current date to extend from</param>
    /// <param name="billingCycle">The billing cycle configuration</param>
    /// <returns>The extended date</returns>
    public static DateTime ExtendByBillingCycle(DateTime currentDate, MasterBillingCycle billingCycle)
    {
        return CalculateNextBillingDate(currentDate, billingCycle);
    }

    /// <summary>
    /// Validates if a billing cycle name is valid.
    /// IMPORTANT: Only accepts database-standard terms (annual, quarterly, monthly, weekly, daily).
    /// </summary>
    /// <param name="billingCycleName">The billing cycle name to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValidBillingCycleName(string? billingCycleName)
    {
        if (string.IsNullOrWhiteSpace(billingCycleName))
            return false;

        var normalized = billingCycleName.ToLower();
        return normalized is "monthly" or "quarterly" or "annual" or "weekly" or "daily";
    }

    /// <summary>
    /// Normalizes billing cycle name to lowercase standard format.
    /// Database uses "Annual" (capital A), this ensures case-insensitive matching.
    /// </summary>
    /// <param name="billingCycleName">The billing cycle name to normalize</param>
    /// <returns>Normalized billing cycle name (lowercase)</returns>
    public static string NormalizeBillingCycleName(string billingCycleName)
    {
        return billingCycleName?.ToLower() ?? "monthly";
    }
}

