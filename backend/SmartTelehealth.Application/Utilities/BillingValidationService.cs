using SmartTelehealth.Core.Entities;
using Microsoft.Extensions.Logging;

namespace SmartTelehealth.Application.Utilities;

/// <summary>
/// Centralized billing validation service to ensure consistency and prevent revenue loss.
/// SINGLE SOURCE OF TRUTH for all billing validation rules.
/// </summary>
public static class BillingValidationService
{
    /// <summary>
    /// Validates that total discounts don't exceed maximum allowed percentage.
    /// Prevents revenue loss from excessive discount stacking.
    /// </summary>
    /// <param name="baseAmount">The base amount before discounts</param>
    /// <param name="totalDiscounts">Total discount amount to validate</param>
    /// <param name="maxDiscountPercentage">Maximum allowed discount percentage (default: 50%)</param>
    /// <returns>Validated discount amount (capped at maximum)</returns>
    public static decimal ValidateAndCapDiscounts(decimal baseAmount, decimal totalDiscounts, decimal maxDiscountPercentage = 50m)
    {
        if (baseAmount <= 0)
            return 0;

        var maxAllowedDiscount = baseAmount * (maxDiscountPercentage / 100);
        return Math.Min(totalDiscounts, maxAllowedDiscount);
    }

    /// <summary>
    /// Validates that a billing adjustment doesn't result in negative amounts.
    /// </summary>
    /// <param name="currentAmount">Current billing record amount</param>
    /// <param name="adjustmentAmount">Adjustment amount (positive for charges, negative for credits)</param>
    /// <param name="adjustmentType">Type of adjustment</param>
    /// <returns>Validated adjustment amount</returns>
    public static decimal ValidateAdjustmentAmount(decimal currentAmount, decimal adjustmentAmount, BillingAdjustment.AdjustmentType adjustmentType)
    {
        // Determine if this is a deduction (credit, discount, refund) or addition (fee, tax)
        bool isDeduction = adjustmentType == BillingAdjustment.AdjustmentType.Discount ||
                          adjustmentType == BillingAdjustment.AdjustmentType.Credit ||
                          adjustmentType == BillingAdjustment.AdjustmentType.Refund;

        if (isDeduction)
        {
            // Ensure deduction doesn't exceed current amount
            return Math.Min(Math.Abs(adjustmentAmount), currentAmount);
        }
        else
        {
            // For additions, return the amount as-is (no cap needed)
            return Math.Abs(adjustmentAmount);
        }
    }

    /// <summary>
    /// Validates currency code format and provides consistent fallback.
    /// </summary>
    /// <param name="currencyCode">Currency code to validate</param>
    /// <returns>Validated currency code in lowercase</returns>
    public static string ValidateCurrencyCode(string? currencyCode)
    {
        if (string.IsNullOrWhiteSpace(currencyCode))
            return "usd";

        // Normalize to lowercase for Stripe compatibility
        var normalized = currencyCode.Trim().ToLower();

        // Validate against common currency codes
        var validCurrencies = new[] { "usd", "eur", "gbp", "cad", "aud", "jpy", "chf", "sek", "nok", "dkk" };
        
        return validCurrencies.Contains(normalized) ? normalized : "usd";
    }

    /// <summary>
    /// Validates that a price amount is within reasonable bounds.
    /// </summary>
    /// <param name="amount">Amount to validate</param>
    /// <param name="minAmount">Minimum allowed amount (default: $0.01)</param>
    /// <param name="maxAmount">Maximum allowed amount (default: $100,000)</param>
    /// <returns>Validated amount</returns>
    public static decimal ValidatePriceAmount(decimal amount, decimal minAmount = 0.01m, decimal maxAmount = 100000m)
    {
        if (amount < minAmount)
            return minAmount;
        
        if (amount > maxAmount)
            return maxAmount;
        
        return amount;
    }

    /// <summary>
    /// Validates billing cycle name against supported cycles.
    /// </summary>
    /// <param name="billingCycleName">Billing cycle name to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValidBillingCycle(string? billingCycleName)
    {
        if (string.IsNullOrWhiteSpace(billingCycleName))
            return false;

        var normalized = billingCycleName.ToLower();
        var validCycles = new[] { "monthly", "quarterly", "annual", "weekly", "daily" };
        
        return validCycles.Contains(normalized);
    }

    /// <summary>
    /// Validates that a date is in the future (for discount expiry, etc.).
    /// </summary>
    /// <param name="date">Date to validate</param>
    /// <param name="allowPast">Whether to allow past dates (default: false)</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValidFutureDate(DateTime date, bool allowPast = false)
    {
        if (allowPast)
            return true;

        return date > DateTime.UtcNow;
    }

    /// <summary>
    /// Validates that a percentage is within valid range (0-100).
    /// </summary>
    /// <param name="percentage">Percentage to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValidPercentage(decimal percentage)
    {
        return percentage >= 0 && percentage <= 100;
    }

    /// <summary>
    /// Validates that a subscription is in a valid state for billing operations.
    /// </summary>
    /// <param name="subscription">Subscription to validate</param>
    /// <returns>Validation result with error message if invalid</returns>
    public static (bool IsValid, string ErrorMessage) ValidateSubscriptionForBilling(Subscription subscription)
    {
        if (subscription == null)
            return (false, "Subscription is null");

        if (subscription.Status == Subscription.SubscriptionStatuses.Cancelled)
            return (false, "Cannot bill cancelled subscription");

        if (subscription.Status == Subscription.SubscriptionStatuses.Expired)
            return (false, "Cannot bill expired subscription");

        if (subscription.SubscriptionPlan == null)
            return (false, "Subscription has no associated plan");

        if (subscription.CurrentPrice <= 0)
            return (false, "Subscription has invalid price");

        return (true, string.Empty);
    }

    /// <summary>
    /// Validates that a billing record is in a valid state for adjustments.
    /// </summary>
    /// <param name="billingRecord">Billing record to validate</param>
    /// <param name="adjustmentType">Type of adjustment being applied</param>
    /// <returns>Validation result with error message if invalid</returns>
    public static (bool IsValid, string ErrorMessage) ValidateBillingRecordForAdjustment(
        BillingRecord billingRecord, 
        BillingAdjustment.AdjustmentType adjustmentType)
    {
        if (billingRecord == null)
            return (false, "Billing record is null");

        if (billingRecord.Status == BillingRecord.BillingStatus.Paid && 
            (adjustmentType == BillingAdjustment.AdjustmentType.Discount || 
             adjustmentType == BillingAdjustment.AdjustmentType.Credit))
        {
            return (false, "Cannot apply discounts or credits to already paid billing records");
        }

        if (adjustmentType == BillingAdjustment.AdjustmentType.Refund && 
            billingRecord.Status != BillingRecord.BillingStatus.Paid)
        {
            return (false, "Refunds can only be applied to paid billing records");
        }

        return (true, string.Empty);
    }

    // REMOVED: ValidatePromotionalCode method - promotional codes are no longer supported
    // Only admin-set discount percentages on plans are used

    /// <summary>
    /// Validates that a billing cycle is compatible with a subscription plan.
    /// </summary>
    /// <param name="billingCycleName">Billing cycle name to validate</param>
    /// <param name="plan">Subscription plan to validate against</param>
    /// <returns>Validation result with error message if invalid</returns>
    public static (bool IsValid, string ErrorMessage) ValidateBillingCycleCompatibility(string billingCycleName, SubscriptionPlan plan)
    {
        if (string.IsNullOrWhiteSpace(billingCycleName))
            return (false, "Billing cycle name cannot be empty");

        if (plan == null)
            return (false, "Subscription plan is required");

        // Check if billing cycle is supported
        if (!IsValidBillingCycle(billingCycleName))
            return (false, $"Billing cycle '{billingCycleName}' is not supported");

        // Check if plan supports this billing cycle
        if (plan.BillingCycleId == null)
            return (false, "Plan does not have an associated billing cycle");

        return (true, string.Empty);
    }

    /// <summary>
    /// Validates that a payment method is valid for processing.
    /// </summary>
    /// <param name="paymentMethodId">Payment method ID to validate</param>
    /// <param name="customerId">Customer ID to validate against</param>
    /// <returns>Validation result with error message if invalid</returns>
    public static (bool IsValid, string ErrorMessage) ValidatePaymentMethod(string paymentMethodId, string customerId)
    {
        if (string.IsNullOrWhiteSpace(paymentMethodId))
            return (false, "Payment method ID cannot be empty");

        if (string.IsNullOrWhiteSpace(customerId))
            return (false, "Customer ID cannot be empty");

        // Basic format validation for Stripe payment method IDs
        if (!paymentMethodId.StartsWith("pm_"))
            return (false, "Invalid payment method ID format");

        // Basic format validation for Stripe customer IDs
        if (!customerId.StartsWith("cus_"))
            return (false, "Invalid customer ID format");

        return (true, string.Empty);
    }

    /// <summary>
    /// Validates that a refund amount is valid for a billing record.
    /// </summary>
    /// <param name="refundAmount">Refund amount to validate</param>
    /// <param name="billingRecord">Billing record to validate against</param>
    /// <returns>Validation result with error message if invalid</returns>
    public static (bool IsValid, string ErrorMessage) ValidateRefundAmount(decimal refundAmount, BillingRecord billingRecord)
    {
        if (billingRecord == null)
            return (false, "Billing record is required");

        if (refundAmount <= 0)
            return (false, "Refund amount must be greater than zero");

        if (refundAmount > billingRecord.TotalAmount)
            return (false, "Refund amount cannot exceed the total amount paid");

        if (billingRecord.Status != BillingRecord.BillingStatus.Paid)
            return (false, "Refunds can only be processed for paid billing records");

        return (true, string.Empty);
    }

    /// <summary>
    /// Validates that a subscription plan is valid for creation or updates.
    /// </summary>
    /// <param name="plan">Subscription plan to validate</param>
    /// <returns>Validation result with error message if invalid</returns>
    public static (bool IsValid, string ErrorMessage) ValidateSubscriptionPlan(SubscriptionPlan plan)
    {
        if (plan == null)
            return (false, "Subscription plan is required");

        if (string.IsNullOrWhiteSpace(plan.Name))
            return (false, "Plan name is required");

        if (plan.Name.Length > 100)
            return (false, "Plan name cannot exceed 100 characters");

        if (plan.BasePrice < 0)
            return (false, "Base price cannot be negative");

        if (plan.BasePrice > 100000)
            return (false, "Base price cannot exceed $100,000");

        if (plan.DiscountPercentage.HasValue && !IsValidPercentage(plan.DiscountPercentage.Value))
            return (false, "Discount percentage must be between 0 and 100");

        if (plan.BillingDiscountPercentage.HasValue && !IsValidPercentage(plan.BillingDiscountPercentage.Value))
            return (false, "Billing discount percentage must be between 0 and 100");

        if (plan.AdminCommissionPercent.HasValue && !IsValidPercentage(plan.AdminCommissionPercent.Value))
            return (false, "Admin commission percentage must be between 0 and 100");

        if (plan.DiscountValidUntil.HasValue && plan.DiscountValidUntil.Value <= DateTime.UtcNow)
            return (false, "Discount valid until date must be in the future");

        return (true, string.Empty);
    }

    /// <summary>
    /// Validates that a user is eligible for a subscription plan.
    /// </summary>
    /// <param name="userId">User ID to validate</param>
    /// <param name="plan">Subscription plan to validate against</param>
    /// <returns>Validation result with error message if invalid</returns>
    public static (bool IsValid, string ErrorMessage) ValidateUserEligibility(Guid userId, SubscriptionPlan plan)
    {
        if (userId == Guid.Empty)
            return (false, "User ID is required");

        if (plan == null)
            return (false, "Subscription plan is required");

        if (!plan.IsActive)
            return (false, "Subscription plan is not active");

        // Add additional eligibility checks here (e.g., user status, existing subscriptions, etc.)
        return (true, string.Empty);
    }

    /// <summary>
    /// Validates that a billing adjustment is within acceptable limits.
    /// </summary>
    /// <param name="adjustmentAmount">Adjustment amount to validate</param>
    /// <param name="adjustmentType">Type of adjustment</param>
    /// <param name="currentAmount">Current billing amount</param>
    /// <returns>Validation result with error message if invalid</returns>
    public static (bool IsValid, string ErrorMessage) ValidateBillingAdjustment(decimal adjustmentAmount, BillingAdjustment.AdjustmentType adjustmentType, decimal currentAmount)
    {
        if (adjustmentAmount == 0)
            return (false, "Adjustment amount cannot be zero");

        if (currentAmount <= 0)
            return (false, "Current amount must be greater than zero");

        // Validate adjustment limits based on type
        switch (adjustmentType)
        {
            case BillingAdjustment.AdjustmentType.Discount:
            case BillingAdjustment.AdjustmentType.Credit:
                if (Math.Abs(adjustmentAmount) > currentAmount)
                    return (false, "Discount/credit cannot exceed current amount");
                break;

            case BillingAdjustment.AdjustmentType.ServiceFee:
            case BillingAdjustment.AdjustmentType.TaxAdjustment:
                if (adjustmentAmount < 0)
                    return (false, "Fees and taxes cannot be negative");
                break;

            case BillingAdjustment.AdjustmentType.Refund:
                if (adjustmentAmount <= 0)
                    return (false, "Refund amount must be positive");
                if (adjustmentAmount > currentAmount)
                    return (false, "Refund cannot exceed current amount");
                break;
        }

        return (true, string.Empty);
    }
}

