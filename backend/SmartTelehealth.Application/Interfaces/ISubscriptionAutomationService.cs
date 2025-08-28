using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface ISubscriptionAutomationService
{
    Task<JsonModel> TriggerBillingAsync(TokenModel tokenModel);
    Task<JsonModel> RenewSubscriptionAsync(string subscriptionId, TokenModel tokenModel);
    Task<JsonModel> ChangePlanAsync(string subscriptionId, ChangePlanRequest request, TokenModel tokenModel);
    Task<JsonModel> ProcessAutomatedRenewalsAsync(TokenModel tokenModel);
    Task<JsonModel> ProcessExpiredSubscriptionsAsync(TokenModel tokenModel);
    
    // Automation status and logging methods
    Task<JsonModel> GetAutomationStatusAsync(TokenModel tokenModel);
    Task<JsonModel> GetAutomationLogsAsync(int page = 1, int pageSize = 50, TokenModel tokenModel = null);
}
