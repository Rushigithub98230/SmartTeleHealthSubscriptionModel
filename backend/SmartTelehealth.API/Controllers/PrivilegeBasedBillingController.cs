using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;

namespace SmartTelehealth.API.Controllers;

/// <summary>
/// Controller for managing privilege-based billing operations
/// UPDATED: Now uses consolidated ISubscriptionBillingService
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PrivilegeBasedBillingController : BaseController
{
    private readonly ISubscriptionBillingService _subscriptionBillingService;
    private readonly ILogger<PrivilegeBasedBillingController> _logger;

    public PrivilegeBasedBillingController(
        ISubscriptionBillingService subscriptionBillingService,
        ILogger<PrivilegeBasedBillingController> logger)
    {
        _subscriptionBillingService = subscriptionBillingService;
        _logger = logger;
    }

    /// <summary>
    /// Calculates the base price for a subscription plan based on privileges and their unit costs
    /// </summary>
    /// <param name="calculateDto">Plan price calculation details</param>
    /// <returns>Calculated base price and breakdown</returns>
    [HttpPost("calculate-plan-price")]
    public async Task<JsonModel> CalculatePlanBasePrice([FromBody] CalculatePlanPriceDto calculateDto)
    {
        var token = GetToken(HttpContext);
        return await _subscriptionBillingService.CalculatePlanBasePriceAsync(calculateDto, token);
    }

    /// <summary>
    /// Processes privilege usage and calculates extra charges if limits are exceeded
    /// </summary>
    /// <param name="usageDto">Privilege usage details</param>
    /// <returns>Usage processing results</returns>
    [HttpPost("process-usage")]
    public async Task<JsonModel> ProcessPrivilegeUsage([FromBody] ProcessPrivilegeUsageDto usageDto)
    {
        var token = GetToken(HttpContext);
        return await _subscriptionBillingService.ProcessPrivilegeUsageAsync(usageDto, token);
    }

    /// <summary>
    /// Resets subscription for new billing period (dates + privileges only).
    /// 
    /// ⚠️ DEPRECATION WARNING (PHASE 4):
    /// This endpoint does NOT create billing records or process payments.
    /// It ONLY updates billing dates and resets privileges.
    /// 
    /// For production use, this endpoint should:
    /// 1. Create billing record for renewal amount
    /// 2. Process payment through Stripe
    /// 3. Update dates and reset privileges (on payment success)
    /// 
    /// Current Implementation: Partial (step 3 only)
    /// 
    /// Recommended: Use automated billing service or implement complete renewal flow
    /// 
    /// TODO: Implement complete renewal flow with billing + payment
    /// See: AutomatedBillingService.ProcessSubscriptionRenewalAsync() for reference
    /// </summary>
    /// <param name="subscriptionId">Subscription ID to reset for new billing period</param>
    /// <returns>Reset results (dates + privileges updated)</returns>
    [HttpPost("renew-subscription/{subscriptionId}")]
    public async Task<JsonModel> ProcessSubscriptionRenewal(Guid subscriptionId)
    {
        var token = GetToken(HttpContext);
        
        // PHASE 4: This now calls the renamed method with clear warning in response
        var result = await _subscriptionBillingService.ResetSubscriptionForNewBillingPeriodAsync(subscriptionId, token);
        
        // Add warning to response about incomplete implementation
        if (result.StatusCode == 200)
        {
            _logger.LogWarning(
                "⚠️ Manual renewal endpoint called for subscription {SubscriptionId}. " +
                "This does NOT create billing records or process payments. " +
                "Consider using automated billing service for complete renewal.",
                subscriptionId);
        }
        
        return result;
    }

    /// <summary>
    /// Gets privilege usage summary for a user
    /// </summary>
    /// <param name="userId">User ID to get usage summary for</param>
    /// <returns>Privilege usage summary</returns>
    [HttpGet("usage-summary/{userId}")]
    public async Task<JsonModel> GetPrivilegeUsageSummary(int userId)
    {
        var token = GetToken(HttpContext);
        return await _subscriptionBillingService.GetPrivilegeUsageSummaryAsync(userId, token);
    }

    /// <summary>
    /// Gets privilege usage summary for the current user
    /// </summary>
    /// <returns>Current user's privilege usage summary</returns>
    [HttpGet("my-usage-summary")]
    public async Task<JsonModel> GetMyPrivilegeUsageSummary()
    {
        var token = GetToken(HttpContext);
        return await _subscriptionBillingService.GetPrivilegeUsageSummaryAsync(token.UserID, token);
    }

}
