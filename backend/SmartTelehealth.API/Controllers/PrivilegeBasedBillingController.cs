using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;

namespace SmartTelehealth.API.Controllers;

/// <summary>
/// Controller for managing privilege-based billing operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PrivilegeBasedBillingController : BaseController
{
    private readonly IPrivilegeBasedBillingService _privilegeBasedBillingService;
    private readonly ILogger<PrivilegeBasedBillingController> _logger;

    public PrivilegeBasedBillingController(
        IPrivilegeBasedBillingService privilegeBasedBillingService,
        ILogger<PrivilegeBasedBillingController> logger)
    {
        _privilegeBasedBillingService = privilegeBasedBillingService;
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
        return await _privilegeBasedBillingService.CalculatePlanBasePriceAsync(calculateDto, token);
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
        return await _privilegeBasedBillingService.ProcessPrivilegeUsageAsync(usageDto, token);
    }

    /// <summary>
    /// Processes subscription renewal and resets privilege usage
    /// </summary>
    /// <param name="subscriptionId">Subscription ID to renew</param>
    /// <returns>Renewal processing results</returns>
    [HttpPost("renew-subscription/{subscriptionId}")]
    public async Task<JsonModel> ProcessSubscriptionRenewal(Guid subscriptionId)
    {
        var token = GetToken(HttpContext);
        return await _privilegeBasedBillingService.ProcessSubscriptionRenewalAsync(subscriptionId, token);
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
        return await _privilegeBasedBillingService.GetPrivilegeUsageSummaryAsync(userId, token);
    }

    /// <summary>
    /// Gets privilege usage summary for the current user
    /// </summary>
    /// <returns>Current user's privilege usage summary</returns>
    [HttpGet("my-usage-summary")]
    public async Task<JsonModel> GetMyPrivilegeUsageSummary()
    {
        var token = GetToken(HttpContext);
        return await _privilegeBasedBillingService.GetPrivilegeUsageSummaryAsync(token.UserID, token);
    }

}
