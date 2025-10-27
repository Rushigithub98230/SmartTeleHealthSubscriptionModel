using SmartTelehealth.Core.Entities;
using Microsoft.Extensions.Logging;

namespace SmartTelehealth.Application.Utilities;

/// <summary>
/// Centralized billing calculation service to ensure consistency across all billing operations.
/// SINGLE SOURCE OF TRUTH for all billing calculations.
/// </summary>
public static class BillingCalculationService
{
    /// <summary>
    /// Calculates the final billing amount for a subscription with proper validation.
    /// This is the SINGLE SOURCE OF TRUTH for all billing calculations.
    /// </summary>
    /// <param name="subscription">The subscription to calculate billing for</param>
    /// <param name="basePrice">The base price (from plan or effective price)</param>
    /// <param name="additionalDiscounts">Additional discounts to apply</param>
    /// <param name="adjustments">Adjustments (fees, taxes, etc.)</param>
    /// <param name="logger">Optional logger for debugging</param>
    /// <returns>Final billing amount with proper validation</returns>
    public static decimal CalculateFinalBillingAmount(
        Subscription subscription,
        decimal basePrice,
        decimal additionalDiscounts,
        decimal adjustments,
        ILogger? logger = null)
    {
        try
        {
            // Validate inputs
            if (subscription == null)
                throw new ArgumentNullException(nameof(subscription));

            if (basePrice < 0)
            {
                logger?.LogWarning("Negative base price {BasePrice} for subscription {SubscriptionId}, using 0", 
                    basePrice, subscription.Id);
                basePrice = 0;
            }

            // CRITICAL FIX: Validate and cap discounts to prevent revenue loss
            var validatedDiscounts = BillingValidationService.ValidateAndCapDiscounts(basePrice, additionalDiscounts, 50m);
            
            if (validatedDiscounts != additionalDiscounts)
            {
                logger?.LogWarning("Discounts capped for subscription {SubscriptionId}: Original={Original}, Capped={Capped}",
                    subscription.Id, additionalDiscounts, validatedDiscounts);
            }

            // Calculate final amount
            var finalAmount = basePrice - validatedDiscounts + adjustments;

            // Ensure minimum amount
            finalAmount = Math.Max(finalAmount, 0.01m);

            logger?.LogInformation(
                "Final billing calculation for subscription {SubscriptionId}: " +
                "BasePrice={BasePrice}, Discounts={Discounts}, Adjustments={Adjustments}, Final={Final}",
                subscription.Id, basePrice, validatedDiscounts, adjustments, finalAmount);

            return finalAmount;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error calculating final billing amount for subscription {SubscriptionId}", subscription.Id);
            // Return base price as fallback
            return Math.Max(basePrice, 0.01m);
        }
    }

    /// <summary>
    /// Calculates the effective price for a subscription plan using sequential discount logic.
    /// Step 1: BasePrice (PrivilegesTotalCost + Commission)
    /// Step 2: Apply DiscountPercentage (if valid)
    /// Step 3: Apply BillingDiscountPercentage
    /// </summary>
    /// <param name="plan">The subscription plan</param>
    /// <param name="logger">Optional logger for debugging</param>
    /// <returns>The effective price to use for billing</returns>
    public static decimal GetEffectivePlanPrice(SubscriptionPlan plan, ILogger? logger = null)
    {
        try
        {
            if (plan == null)
                throw new ArgumentNullException(nameof(plan));

            // Step 1: Start with base price
            decimal price = plan.BasePrice;
            
            logger?.LogInformation("Starting with base price for plan {PlanName}: ${BasePrice}",
                plan.Name, price);

            // Step 2: Apply promotional discount if valid
            if (plan.DiscountPercentage.HasValue && plan.DiscountPercentage.Value > 0 &&
                (!plan.DiscountValidUntil.HasValue || plan.DiscountValidUntil.Value >= DateTime.UtcNow))
            {
                var discountAmount = price * (plan.DiscountPercentage.Value / 100);
                price = price * (1 - (plan.DiscountPercentage.Value / 100));
                
                logger?.LogInformation("Applied promotional discount for plan {PlanName}: Base=${BasePrice}, Discount={Discount}%, After=${AfterPrice}",
                    plan.Name, plan.BasePrice, plan.DiscountPercentage.Value, price);
            }

            // Step 3: Apply billing discount
            if (plan.BillingDiscountPercentage.HasValue && plan.BillingDiscountPercentage.Value > 0)
            {
                var discountAmount = price * (plan.BillingDiscountPercentage.Value / 100);
                price = price * (1 - (plan.BillingDiscountPercentage.Value / 100));
                
                logger?.LogInformation("Applied billing discount for plan {PlanName}: Before=${BeforePrice}, Discount={Discount}%, Final=${FinalPrice}",
                    plan.Name, price / (1 - (plan.BillingDiscountPercentage.Value / 100)), plan.BillingDiscountPercentage.Value, price);
            }
            
            var finalPrice = Math.Max(price, 0); // Ensure price doesn't go negative
            
            logger?.LogInformation("Final effective price for plan {PlanName}: ${FinalPrice}",
                plan.Name, finalPrice);
            
            return finalPrice;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error calculating effective price for plan {PlanName}, using base price", plan.Name);
            return plan.BasePrice;
        }
    }


    /// <summary>
    /// Calculates billing adjustment amount with proper validation.
    /// </summary>
    /// <param name="billingRecord">The billing record being adjusted</param>
    /// <param name="adjustmentAmount">The adjustment amount</param>
    /// <param name="adjustmentType">The type of adjustment</param>
    /// <param name="logger">Optional logger for debugging</param>
    /// <returns>Validated adjustment amount</returns>
    public static decimal CalculateAdjustmentAmount(
        BillingRecord billingRecord,
        decimal adjustmentAmount,
        string adjustmentType,
        ILogger? logger = null)
    {
        try
        {
            if (billingRecord == null)
                throw new ArgumentNullException(nameof(billingRecord));

            if (string.IsNullOrEmpty(adjustmentType))
                throw new ArgumentException("Adjustment type cannot be null or empty", nameof(adjustmentType));

            // For percentage-based adjustments, calculate the actual amount
            if (adjustmentType == "Discount" && adjustmentAmount > 0 && adjustmentAmount <= 100)
            {
                // Treat as percentage discount
                decimal percentageDiscount = billingRecord.TotalAmount * (adjustmentAmount / 100);
                logger?.LogInformation("Calculated percentage discount: {Percentage}% of {BaseAmount} = {DiscountAmount}",
                    adjustmentAmount, billingRecord.TotalAmount, percentageDiscount);
                return percentageDiscount;
            }

            // For fixed amount adjustments
            logger?.LogInformation("Using fixed adjustment amount: {Amount}", adjustmentAmount);
            return adjustmentAmount;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error calculating adjustment amount for billing record {BillingRecordId}", billingRecord.Id);
            return 0;
        }
    }

    /// <summary>
    /// Calculates prorated amount with proper business logic validation.
    /// </summary>
    /// <param name="subscription">The subscription</param>
    /// <param name="effectiveDate">Date when proration takes effect</param>
    /// <param name="amount">Amount to prorate</param>
    /// <param name="prorationType">Type of proration (upgrade, downgrade, cancellation)</param>
    /// <param name="logger">Optional logger for debugging</param>
    /// <returns>Prorated amount with proper business logic</returns>
    public static decimal CalculateProratedAmount(
        Subscription subscription,
        DateTime effectiveDate,
        decimal amount,
        ProrationType prorationType,
        ILogger? logger = null)
    {
        try
        {
            if (subscription == null)
                throw new ArgumentNullException(nameof(subscription));

            // Use centralized proration calculation
            var proratedAmount = BillingCycleCalculator.CalculateProratedAmount(
                subscription, effectiveDate, amount, logger);

            // Apply business logic based on proration type
            switch (prorationType)
            {
                case ProrationType.Upgrade:
                    // For upgrades, user pays the difference for remaining period
                    logger?.LogInformation("Upgrade proration calculated for subscription {SubscriptionId}: {Amount}",
                        subscription.Id, proratedAmount);
                    break;

                case ProrationType.Downgrade:
                    // For downgrades, user gets credit for remaining period
                    logger?.LogInformation("Downgrade proration calculated for subscription {SubscriptionId}: {Amount}",
                        subscription.Id, proratedAmount);
                    break;

                case ProrationType.Cancellation:
                    // For cancellations, user gets refund for remaining period
                    logger?.LogInformation("Cancellation proration calculated for subscription {SubscriptionId}: {Amount}",
                        subscription.Id, proratedAmount);
                    break;

                default:
                    logger?.LogWarning("Unknown proration type {ProrationType} for subscription {SubscriptionId}",
                        prorationType, subscription.Id);
                    break;
            }

            return proratedAmount;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error calculating prorated amount for subscription {SubscriptionId}", subscription.Id);
            return amount; // Fallback to full amount
        }
    }

    /// <summary>
    /// Validates that a billing calculation is logically correct.
    /// </summary>
    /// <param name="subscription">The subscription</param>
    /// <param name="basePrice">Base price</param>
    /// <param name="discounts">Discounts applied</param>
    /// <param name="adjustments">Adjustments applied</param>
    /// <param name="finalAmount">Final calculated amount</param>
    /// <param name="logger">Optional logger for debugging</param>
    /// <returns>True if calculation is valid, false otherwise</returns>
    public static bool ValidateBillingCalculation(
        Subscription subscription,
        decimal basePrice,
        decimal discounts,
        decimal adjustments,
        decimal finalAmount,
        ILogger? logger = null)
    {
        try
        {
            // Validate subscription state
            var (isValid, errorMessage) = BillingValidationService.ValidateSubscriptionForBilling(subscription);
            if (!isValid)
            {
                logger?.LogError("Invalid subscription state for billing: {Error}", errorMessage);
                return false;
            }

            // Validate amounts are non-negative
            if (basePrice < 0 || discounts < 0 || finalAmount < 0)
            {
                logger?.LogError("Negative amounts in billing calculation for subscription {SubscriptionId}: Base={Base}, Discounts={Discounts}, Final={Final}",
                    subscription.Id, basePrice, discounts, finalAmount);
                return false;
            }

            // Validate discount doesn't exceed base price
            if (discounts > basePrice)
            {
                logger?.LogError("Discounts exceed base price for subscription {SubscriptionId}: Base={Base}, Discounts={Discounts}",
                    subscription.Id, basePrice, discounts);
                return false;
            }

            // Validate final amount is reasonable
            var expectedAmount = basePrice - discounts + adjustments;
            var tolerance = 0.01m; // 1 cent tolerance for rounding
            if (Math.Abs(finalAmount - expectedAmount) > tolerance)
            {
                logger?.LogError("Final amount doesn't match expected calculation for subscription {SubscriptionId}: Expected={Expected}, Actual={Actual}",
                    subscription.Id, expectedAmount, finalAmount);
                return false;
            }

            logger?.LogDebug("Billing calculation validated successfully for subscription {SubscriptionId}", subscription.Id);
            return true;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error validating billing calculation for subscription {SubscriptionId}", subscription.Id);
            return false;
        }
    }

    /// <summary>
    /// Calculates the cost contribution of a privilege to the plan's base price.
    /// SINGLE SOURCE OF TRUTH for privilege cost calculations.
    /// 
    /// Business Rules:
    /// - Limited privileges (Value > 0): Cost = Value × PrivilegeBaseCost
    /// - Unlimited privileges (Value = -1): Cost = PrivilegeBaseCost (fixed cost)
    /// - Disabled privileges (Value = 0): Cost = 0
    /// </summary>
    /// <param name="planPrivilege">The subscription plan privilege to calculate cost for</param>
    /// <param name="logger">Optional logger for debugging</param>
    /// <returns>The cost contribution of this privilege to the plan's base price</returns>
    public static decimal CalculatePrivilegeCost(SubscriptionPlanPrivilege planPrivilege, ILogger? logger = null)
    {
        try
        {
            if (planPrivilege == null)
                throw new ArgumentNullException(nameof(planPrivilege));

            decimal privilegeCost = 0;

            if (planPrivilege.Value > 0)
            {
                // Limited privileges: Cost = Quantity × Unit Cost
                privilegeCost = planPrivilege.Value * planPrivilege.PrivilegeBaseCost;
                
                logger?.LogDebug("Limited privilege cost calculation: {Value} × {BaseCost} = {TotalCost}",
                    planPrivilege.Value, planPrivilege.PrivilegeBaseCost, privilegeCost);
            }
            else if (planPrivilege.Value == -1)
            {
                // Unlimited privileges: Use explicit base cost (no multiplication)
                privilegeCost = planPrivilege.PrivilegeBaseCost;
                
                logger?.LogDebug("Unlimited privilege cost calculation: Fixed cost = {BaseCost}",
                    planPrivilege.PrivilegeBaseCost);
            }
            // Disabled privileges (Value = 0): Cost = 0 (no action needed)

            logger?.LogDebug("Privilege cost calculated: {Cost} for privilege value {Value}",
                privilegeCost, planPrivilege.Value);

            return privilegeCost;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error calculating privilege cost for plan privilege {PlanPrivilegeId}", 
                planPrivilege?.Id ?? Guid.Empty);
            return 0; // Safe fallback
        }
    }

    /// <summary>
    /// Calculates the total base price for a subscription plan based on its privileges.
    /// SINGLE SOURCE OF TRUTH for plan base price calculations.
    /// </summary>
    /// <param name="planPrivileges">The privileges included in the plan</param>
    /// <param name="logger">Optional logger for debugging</param>
    /// <returns>The total base price for the plan</returns>
    public static decimal CalculatePlanBasePrice(IEnumerable<SubscriptionPlanPrivilege> planPrivileges, ILogger? logger = null)
    {
        try
        {
            if (planPrivileges == null)
                throw new ArgumentNullException(nameof(planPrivileges));

            decimal totalBasePrice = 0;
            var privilegeBreakdown = new List<object>();

            foreach (var planPrivilege in planPrivileges)
            {
                var privilegeCost = CalculatePrivilegeCost(planPrivilege, logger);
                totalBasePrice += privilegeCost;

                privilegeBreakdown.Add(new
                {
                    PrivilegeId = planPrivilege.PrivilegeId,
                    PrivilegeValue = planPrivilege.Value,
                    PrivilegeBaseCost = planPrivilege.PrivilegeBaseCost,
                    TotalCost = privilegeCost,
                    IsUnlimited = planPrivilege.Value == -1,
                    IsDisabled = planPrivilege.Value == 0
                });
            }

            logger?.LogInformation("Plan base price calculated: {TotalBasePrice} from {PrivilegeCount} privileges",
                totalBasePrice, planPrivileges.Count());

            return totalBasePrice;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error calculating plan base price");
            return 0; // Safe fallback
        }
    }

    /// <summary>
    /// Calculates the admin commission for a plan based on privileges total cost.
    /// SINGLE SOURCE OF TRUTH for commission calculations.
    /// 
    /// Business Rules:
    /// - Use plan.AdminCommissionPercent if available
    /// - Fallback to system default if plan commission is null
    /// - Commission = PrivilegesTotalCost × CommissionPercent / 100
    /// </summary>
    /// <param name="privilegesTotalCost">The total cost of all privileges</param>
    /// <param name="planCommissionPercent">Commission percentage from plan (nullable)</param>
    /// <param name="defaultCommissionPercent">Default commission percentage from system settings</param>
    /// <param name="logger">Optional logger for debugging</param>
    /// <returns>The calculated commission amount</returns>
    public static decimal CalculateAdminCommission(
        decimal privilegesTotalCost,
        decimal? planCommissionPercent,
        decimal defaultCommissionPercent,
        ILogger? logger = null)
    {
        try
        {
            if (privilegesTotalCost < 0)
            {
                logger?.LogWarning("Negative privileges total cost {Cost}, using 0", privilegesTotalCost);
                privilegesTotalCost = 0;
            }

            // Determine commission percentage to use
            decimal commissionPercent = planCommissionPercent ?? defaultCommissionPercent;
            
            // Validate commission percentage
            if (commissionPercent < 0 || commissionPercent > 100)
            {
                logger?.LogWarning("Invalid commission percentage {Percent}%, using 0", commissionPercent);
                commissionPercent = 0;
            }

            // Calculate commission
            decimal commission = privilegesTotalCost * (commissionPercent / 100);

            logger?.LogDebug("Commission calculation: {PrivilegesCost} × {Percent}% = {Commission}",
                privilegesTotalCost, commissionPercent, commission);

            return commission;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error calculating admin commission for privileges cost {Cost}", privilegesTotalCost);
            return 0; // Safe fallback
        }
    }

    /// <summary>
    /// Calculates the final plan price including privileges cost and admin commission.
    /// SINGLE SOURCE OF TRUTH for complete plan price calculations.
    /// </summary>
    /// <param name="privilegesTotalCost">The total cost of all privileges</param>
    /// <param name="planCommissionPercent">Commission percentage from plan (nullable)</param>
    /// <param name="defaultCommissionPercent">Default commission percentage from system settings</param>
    /// <param name="logger">Optional logger for debugging</param>
    /// <returns>Tuple containing (FinalPrice, CommissionAmount, CommissionPercent)</returns>
    public static (decimal FinalPrice, decimal CommissionAmount, decimal CommissionPercent) CalculateFinalPlanPrice(
        decimal privilegesTotalCost,
        decimal? planCommissionPercent,
        decimal defaultCommissionPercent,
        ILogger? logger = null)
    {
        try
        {
            var commissionAmount = CalculateAdminCommission(privilegesTotalCost, planCommissionPercent, defaultCommissionPercent, logger);
            var commissionPercent = planCommissionPercent ?? defaultCommissionPercent;
            var finalPrice = privilegesTotalCost + commissionAmount;

            logger?.LogInformation("Final plan price calculation: Privileges ${Privileges} + Commission ${Commission} ({Percent}%) = ${Final}",
                privilegesTotalCost, commissionAmount, commissionPercent, finalPrice);

            return (finalPrice, commissionAmount, commissionPercent);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error calculating final plan price for privileges cost {Cost}", privilegesTotalCost);
            return (privilegesTotalCost, 0, 0); // Safe fallback - return privileges cost without commission
        }
    }
}

/// <summary>
/// Enum for different types of proration calculations
/// </summary>
public enum ProrationType
{
    Upgrade,
    Downgrade,
    Cancellation,
    MidCycleChange
}









