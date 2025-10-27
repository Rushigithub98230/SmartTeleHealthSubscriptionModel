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
    #region Basic Billing Date Calculations

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
    /// Calculates the next billing date directly from a subscription.
    /// Convenience overload that extracts base date and delegates to main calculator.
    /// ADDED IN PHASE 1: Eliminates duplicate methods across services.
    /// </summary>
    /// <param name="subscription">Subscription with billing cycle and date information</param>
    /// <returns>The next billing date</returns>
    public static DateTime CalculateNextBillingDate(Subscription subscription)
    {
        if (subscription == null)
            throw new ArgumentNullException(nameof(subscription));
            
        // Use LastBillingDate if available, otherwise use StartDate
        var baseDate = subscription.LastBillingDate ?? subscription.StartDate;
        return CalculateNextBillingDate(baseDate, subscription.BillingCycle);
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
    /// Calculates the billing period (start and end dates) for a subscription.
    /// Handles first payment vs renewal scenarios correctly.
    /// ADDED IN PHASE 1: Provides complete period calculation in one method.
    /// </summary>
    /// <param name="subscription">Subscription with billing cycle and date information</param>
    /// <param name="isFirstPayment">True if this is the first payment, false for renewals</param>
    /// <returns>Tuple of (periodStart, periodEnd)</returns>
    public static (DateTime periodStart, DateTime periodEnd) CalculateBillingPeriod(
        Subscription subscription,
        bool isFirstPayment = false)
    {
        if (subscription == null)
            throw new ArgumentNullException(nameof(subscription));

        DateTime periodStart;
        DateTime periodEnd;
        
        if (isFirstPayment || !subscription.LastBillingDate.HasValue)
        {
            // First payment: period starts at subscription start date
            periodStart = subscription.StartDate;
            periodEnd = CalculateEndDateForCycle(periodStart, subscription.BillingCycle);
        }
        else
        {
            // Renewal: NEW period starts at NextBillingDate
            periodStart = subscription.NextBillingDate != default(DateTime)
                ? subscription.NextBillingDate
                : CalculateNextBillingDate(subscription);
            periodEnd = CalculateEndDateForCycle(periodStart, subscription.BillingCycle);
        }
        
        return (periodStart, periodEnd);
    }

    #endregion

    #region Price and Discount Calculations

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
    /// DEPRECATED: This method is obsolete in the new architecture.
    /// Each plan now has explicit pricing based on privileges, not multiplication.
    /// </summary>
    [Obsolete("DEPRECATED: Each plan now has explicit pricing. Use plan.Price directly.")]
    public static decimal ScalePriceToBillingCycle(decimal monthlyPrice, MasterBillingCycle billingCycle)
    {
        var monthsInCycle = CalculateMonthsInCycle(billingCycle.DurationInDays);
        return monthlyPrice * monthsInCycle;
    }

    /// <summary>
    /// Calculates the billing cycle discount amount based on plan configuration.
    /// DEPRECATED: This method is obsolete in the new architecture.
    /// Discounts are now explicit per-plan, not calculated based on billing cycle.
    /// </summary>
    [Obsolete("DEPRECATED: Discounts are now explicit per-plan. Billing cycle discount fields removed.")]
    public static decimal CalculateBillingCycleDiscount(
        SubscriptionPlan plan, 
        MasterBillingCycle billingCycle, 
        decimal basePrice)
    {
        // Return 0 as discount fields no longer exist
        // Each plan has its own explicit price
        return 0m;
    }

    /// <summary>
    /// Calculates the final subscription price including scaling and discount.
    /// DEPRECATED: This method is obsolete in the new architecture.
    /// Each plan now has explicit pricing. Use plan.Price directly instead.
    /// </summary>
    [Obsolete("DEPRECATED: Each plan has explicit price. Use plan.Price directly.")]
    public static decimal CalculateSubscriptionPrice(
        SubscriptionPlan plan, 
        MasterBillingCycle billingCycle)
    {
        // Return plan's explicit price - no calculation needed
        return plan.BasePrice;
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

    #endregion

    #region Proration Calculations (PHASE 3 ADDITION)

    /// <summary>
    /// Calculates prorated amount based on billing cycle and effective date.
    /// SINGLE SOURCE OF TRUTH for all proration calculations.
    /// MOVED FROM: AutomatedBillingService (Lines 370-401) - Phase 3
    /// 
    /// Used for:
    /// - Plan upgrades/downgrades
    /// - Mid-cycle plan changes
    /// - Calculating unused credits
    /// - Prorated billing adjustments
    /// </summary>
    /// <param name="subscription">Subscription with billing cycle information</param>
    /// <param name="effectiveDate">Date when proration takes effect</param>
    /// <param name="amount">Amount to prorate</param>
    /// <param name="logger">Optional logger for debugging proration calculations</param>
    /// <returns>Prorated amount based on remaining time in billing period</returns>
    public static decimal CalculateProratedAmount(
        Subscription subscription,
        DateTime effectiveDate,
        decimal amount,
        ILogger? logger = null)
    {
        if (subscription == null)
            throw new ArgumentNullException(nameof(subscription));

        var billingCycle = subscription.BillingCycle;
        if (billingCycle == null)
        {
            logger?.LogWarning("No billing cycle found for subscription {SubscriptionId}", subscription.Id);
            return amount;
        }

        // Calculate prorated amount based on billing cycle type
        var proratedAmount = billingCycle.Name?.ToLower() switch
        {
            "monthly" => CalculateMonthlyProration(subscription, effectiveDate, amount, logger),
            "quarterly" => CalculateQuarterlyProration(subscription, effectiveDate, amount, logger),
            "annual" => CalculateAnnualProration(subscription, effectiveDate, amount, logger),
            "weekly" => CalculateWeeklyProration(subscription, effectiveDate, amount, logger),
            "daily" => CalculateDailyProration(subscription, effectiveDate, amount, logger),
            _ => CalculateMonthlyProration(subscription, effectiveDate, amount, logger) // Default fallback
        };

        // Ensure minimum amount (at least 1 cent)
        proratedAmount = Math.Max(proratedAmount, 0.01m);

        logger?.LogInformation(
            "Prorated amount calculated for subscription {SubscriptionId}: " +
            "Original={Amount}, Prorated={ProratedAmount}, Cycle={Cycle}, EffectiveDate={Date:yyyy-MM-dd}",
            subscription.Id, amount, proratedAmount, billingCycle.Name, effectiveDate);

        return proratedAmount;
    }

    /// <summary>
    /// Calculates monthly proration based on effective date and amount.
    /// Enhanced to handle edge cases like leap years, time zones, and partial days.
    /// MOVED FROM: AutomatedBillingService.CalculateMonthlyProration (Lines 407-441)
    /// </summary>
    private static decimal CalculateMonthlyProration(
        Subscription subscription,
        DateTime effectiveDate,
        decimal amount,
        ILogger? logger = null)
    {
        try
        {
            // Ensure we're working with UTC time to avoid timezone issues
            var utcEffectiveDate = effectiveDate.Kind == DateTimeKind.Utc
                ? effectiveDate
                : effectiveDate.ToUniversalTime();

            // Get the number of days in the month, accounting for leap years
            var daysInMonth = DateTime.DaysInMonth(utcEffectiveDate.Year, utcEffectiveDate.Month);

            // Calculate days remaining from the effective date to the end of the month
            // Include the effective date itself (hence +1)
            var daysRemaining = daysInMonth - utcEffectiveDate.Day + 1;

            // Ensure we don't have negative days or more days than in the month
            daysRemaining = Math.Max(0, Math.Min(daysRemaining, daysInMonth));

            // Calculate daily rate with proper rounding
            var dailyRate = Math.Round(amount / daysInMonth, 4, MidpointRounding.AwayFromZero);

            // Calculate prorated amount with proper rounding
            var proratedAmount = Math.Round(dailyRate * daysRemaining, 2, MidpointRounding.AwayFromZero);

            logger?.LogDebug(
                "Monthly proration: Amount={Amount}, DaysInMonth={DaysInMonth}, " +
                "DaysRemaining={DaysRemaining}, DailyRate={DailyRate}, ProratedAmount={ProratedAmount}",
                amount, daysInMonth, daysRemaining, dailyRate, proratedAmount);

            return proratedAmount;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error calculating monthly proration for subscription {SubscriptionId}", subscription.Id);
            // Fallback to full amount if calculation fails
            return amount;
        }
    }

    /// <summary>
    /// Calculates quarterly proration based on effective date and amount.
    /// Enhanced to handle edge cases and proper rounding.
    /// MOVED FROM: AutomatedBillingService.CalculateQuarterlyProration (Lines 447-483)
    /// </summary>
    private static decimal CalculateQuarterlyProration(
        Subscription subscription,
        DateTime effectiveDate,
        decimal amount,
        ILogger? logger = null)
    {
        try
        {
            // Ensure we're working with UTC time
            var utcEffectiveDate = effectiveDate.Kind == DateTimeKind.Utc
                ? effectiveDate
                : effectiveDate.ToUniversalTime();

            // Calculate quarter boundaries
            var quarterStart = new DateTime(
                utcEffectiveDate.Year,
                ((utcEffectiveDate.Month - 1) / 3) * 3 + 1,
                1);
            var quarterEnd = quarterStart.AddMonths(3).AddDays(-1);

            // Calculate total days in quarter
            var totalDaysInQuarter = (quarterEnd - quarterStart).Days + 1;

            // Calculate days remaining from effective date to end of quarter
            var daysRemaining = (quarterEnd - utcEffectiveDate).Days + 1;

            // Ensure we don't have negative days or more days than in the quarter
            daysRemaining = Math.Max(0, Math.Min(daysRemaining, totalDaysInQuarter));

            // Calculate daily rate with proper rounding
            var dailyRate = Math.Round(amount / totalDaysInQuarter, 4, MidpointRounding.AwayFromZero);

            // Calculate prorated amount with proper rounding
            var proratedAmount = Math.Round(dailyRate * daysRemaining, 2, MidpointRounding.AwayFromZero);

            logger?.LogDebug(
                "Quarterly proration: Amount={Amount}, TotalDaysInQuarter={TotalDaysInQuarter}, " +
                "DaysRemaining={DaysRemaining}, DailyRate={DailyRate}, ProratedAmount={ProratedAmount}",
                amount, totalDaysInQuarter, daysRemaining, dailyRate, proratedAmount);

            return proratedAmount;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error calculating quarterly proration for subscription {SubscriptionId}", subscription.Id);
            return amount;
        }
    }

    /// <summary>
    /// Calculates annual proration based on effective date and amount.
    /// Enhanced to handle leap years and proper rounding.
    /// STANDARDIZED: Uses "annual" term to match database naming.
    /// MOVED FROM: AutomatedBillingService.CalculateAnnualProration (Lines 493-529)
    /// </summary>
    private static decimal CalculateAnnualProration(
        Subscription subscription,
        DateTime effectiveDate,
        decimal amount,
        ILogger? logger = null)
    {
        try
        {
            // Ensure we're working with UTC time
            var utcEffectiveDate = effectiveDate.Kind == DateTimeKind.Utc
                ? effectiveDate
                : effectiveDate.ToUniversalTime();

            // Calculate year boundaries
            var yearStart = new DateTime(utcEffectiveDate.Year, 1, 1);
            var yearEnd = new DateTime(utcEffectiveDate.Year, 12, 31);

            // Calculate total days in year (handles leap years automatically)
            var totalDaysInYear = (yearEnd - yearStart).Days + 1;

            // Calculate days remaining from effective date to end of year
            var daysRemaining = (yearEnd - utcEffectiveDate).Days + 1;

            // Ensure we don't have negative days or more days than in the year
            daysRemaining = Math.Max(0, Math.Min(daysRemaining, totalDaysInYear));

            // Calculate daily rate with proper rounding
            var dailyRate = Math.Round(amount / totalDaysInYear, 4, MidpointRounding.AwayFromZero);

            // Calculate prorated amount with proper rounding
            var proratedAmount = Math.Round(dailyRate * daysRemaining, 2, MidpointRounding.AwayFromZero);

            logger?.LogDebug(
                "Annual proration: Amount={Amount}, TotalDaysInYear={TotalDaysInYear}, " +
                "DaysRemaining={DaysRemaining}, DailyRate={DailyRate}, ProratedAmount={ProratedAmount}",
                amount, totalDaysInYear, daysRemaining, dailyRate, proratedAmount);

            return proratedAmount;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error calculating annual proration for subscription {SubscriptionId}", subscription.Id);
            return amount;
        }
    }

    /// <summary>
    /// Calculates weekly proration based on effective date and amount.
    /// Enhanced to handle proper rounding and edge cases.
    /// MOVED FROM: AutomatedBillingService.CalculateWeeklyProration (Lines 535-566)
    /// </summary>
    private static decimal CalculateWeeklyProration(
        Subscription subscription,
        DateTime effectiveDate,
        decimal amount,
        ILogger? logger = null)
    {
        try
        {
            // Ensure we're working with UTC time
            var utcEffectiveDate = effectiveDate.Kind == DateTimeKind.Utc
                ? effectiveDate
                : effectiveDate.ToUniversalTime();

            // Calculate week boundaries (Sunday to Saturday)
            var weekStart = utcEffectiveDate.AddDays(-(int)utcEffectiveDate.DayOfWeek);
            var weekEnd = weekStart.AddDays(6);

            // Calculate days remaining from effective date to end of week
            var daysRemaining = (weekEnd - utcEffectiveDate).Days + 1;

            // Ensure we don't have negative days or more days than in the week
            daysRemaining = Math.Max(0, Math.Min(daysRemaining, 7));

            // Calculate daily rate with proper rounding
            var dailyRate = Math.Round(amount / 7, 4, MidpointRounding.AwayFromZero);

            // Calculate prorated amount with proper rounding
            var proratedAmount = Math.Round(dailyRate * daysRemaining, 2, MidpointRounding.AwayFromZero);

            logger?.LogDebug(
                "Weekly proration: Amount={Amount}, DaysRemaining={DaysRemaining}, " +
                "DailyRate={DailyRate}, ProratedAmount={ProratedAmount}",
                amount, daysRemaining, dailyRate, proratedAmount);

            return proratedAmount;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error calculating weekly proration for subscription {SubscriptionId}", subscription.Id);
            return amount;
        }
    }

    /// <summary>
    /// Calculates daily proration (typically returns full amount since it's a single day).
    /// Enhanced for consistency with other proration methods.
    /// MOVED FROM: AutomatedBillingService (would be Lines 568+)
    /// </summary>
    private static decimal CalculateDailyProration(
        Subscription subscription,
        DateTime effectiveDate,
        decimal amount,
        ILogger? logger = null)
    {
        // For daily billing, typically no proration needed
        // User gets charged for each day, so return full amount
        logger?.LogDebug("Daily proration: Returning full amount {Amount} (no proration for daily billing)", amount);
        return amount;
    }

    /// <summary>
    /// Convenience method: Calculates proration directly from subscription.
    /// Overload that uses subscription's current billing cycle and current price.
    /// </summary>
    public static decimal CalculateProratedAmount(
        Subscription subscription,
        DateTime effectiveDate,
        ILogger? logger = null)
    {
        return CalculateProratedAmount(subscription, effectiveDate, subscription.CurrentPrice, logger);
    }

    #endregion
}

