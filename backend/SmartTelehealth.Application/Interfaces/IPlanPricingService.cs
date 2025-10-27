using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;

namespace SmartTelehealth.Application.Interfaces;

/// <summary>
/// Service interface for plan pricing calculations.
/// Healthcare Feature: Privilege-based pricing with abuse prevention.
/// </summary>
public interface IPlanPricingService
{
    /// <summary>
    /// Calculates plan price based on privilege costs + admin commission.
    /// Healthcare Model: Total Price = Σ(Privilege Costs) + Commission.
    /// Choices 1c & 2c: Supports both manual and auto-calculated pricing.
    /// </summary>
    /// <param name="planId">The subscription plan ID</param>
    /// <param name="useAutoCalculation">Whether to use auto-calculation (default: true)</param>
    /// <returns>Calculated plan price</returns>
    Task<decimal> CalculatePlanPriceAsync(Guid planId, bool useAutoCalculation = true);
    
    /// <summary>
    /// Calculates and updates plan price, saving to database.
    /// </summary>
    /// <param name="planId">The subscription plan ID</param>
    /// <param name="tokenModel">User token for audit</param>
    /// <returns>JsonModel with updated plan</returns>
    Task<JsonModel> CalculateAndUpdatePlanPriceAsync(Guid planId, TokenModel tokenModel);
    
    /// <summary>
    /// HEALTHCARE RULE: Overage uses LATEST plan pricing to prevent abuse.
    /// Even if user is on old plan (v1 at $10/mo), overage charges use new plan pricing (v2).
    /// </summary>
    /// <param name="subscriptionId">The subscription ID</param>
    /// <param name="privilegeId">The privilege ID</param>
    /// <param name="quantity">Number of overage units</param>
    /// <returns>Total overage cost</returns>
    Task<decimal> CalculateOverageCostForSubscriptionAsync(
        Guid subscriptionId, 
        Guid privilegeId, 
        int quantity);
    
    /// <summary>
    /// Gets pricing breakdown for a plan (for display/transparency).
    /// </summary>
    /// <param name="planId">The subscription plan ID</param>
    /// <returns>JsonModel with detailed pricing breakdown</returns>
    Task<JsonModel> GetPlanPricingBreakdownAsync(Guid planId);
    
    /// <summary>
    /// Calculates detailed pricing breakdown for a plan (internal use by services).
    /// Returns structured breakdown object for service-to-service communication.
    /// </summary>
    /// <param name="planId">The subscription plan ID</param>
    /// <returns>PricingBreakdown object with detailed cost information</returns>
    Task<PricingBreakdown> CalculatePricingBreakdownAsync(Guid planId);
    
    /// <summary>
    /// CRITICAL FIX: Calculates the effective price for a plan after applying all discounts.
    /// This ensures consistent pricing calculations across frontend and backend.
    /// </summary>
    /// <param name="planId">The subscription plan ID</param>
    /// <returns>The effective price after applying promotional and billing discounts</returns>
    Task<decimal> GetEffectivePriceAsync(Guid planId);
}

