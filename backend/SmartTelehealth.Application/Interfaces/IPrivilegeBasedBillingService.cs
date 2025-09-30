using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;

namespace SmartTelehealth.Application.Interfaces;

/// <summary>
/// Interface for privilege-based billing service
/// </summary>
public interface IPrivilegeBasedBillingService
{
    /// <summary>
    /// Calculates the base price for a subscription plan based on privileges and their unit costs
    /// </summary>
    /// <param name="calculateDto">Plan price calculation details</param>
    /// <param name="tokenModel">Token containing user authentication information</param>
    /// <returns>JsonModel containing calculated base price and breakdown</returns>
    Task<JsonModel> CalculatePlanBasePriceAsync(CalculatePlanPriceDto calculateDto, TokenModel tokenModel);

    /// <summary>
    /// Processes privilege usage and calculates extra charges if limits are exceeded
    /// </summary>
    /// <param name="usageDto">Privilege usage details</param>
    /// <param name="tokenModel">Token containing user authentication information</param>
    /// <returns>JsonModel containing usage processing results</returns>
    Task<JsonModel> ProcessPrivilegeUsageAsync(ProcessPrivilegeUsageDto usageDto, TokenModel tokenModel);

    /// <summary>
    /// Processes subscription renewal and resets privilege usage
    /// </summary>
    /// <param name="subscriptionId">Subscription ID to renew</param>
    /// <param name="tokenModel">Token containing user authentication information</param>
    /// <returns>JsonModel containing renewal processing results</returns>
    Task<JsonModel> ProcessSubscriptionRenewalAsync(Guid subscriptionId, TokenModel tokenModel);

    /// <summary>
    /// Gets privilege usage summary for a user
    /// </summary>
    /// <param name="userId">User ID to get usage summary for</param>
    /// <param name="tokenModel">Token containing user authentication information</param>
    /// <returns>JsonModel containing privilege usage summary</returns>
    Task<JsonModel> GetPrivilegeUsageSummaryAsync(int userId, TokenModel tokenModel);
}
