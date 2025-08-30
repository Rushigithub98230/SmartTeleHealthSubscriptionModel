using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Application.Interfaces;

public interface ISubscriptionLifecycleService
{
    Task<bool> ActivateSubscriptionAsync(Guid subscriptionId, string? reason = null, TokenModel tokenModel = null);
    Task<bool> PauseSubscriptionAsync(Guid subscriptionId, string? reason = null, TokenModel tokenModel = null);
    Task<bool> ResumeSubscriptionAsync(Guid subscriptionId, string? reason = null, TokenModel tokenModel = null);
    Task<bool> CancelSubscriptionAsync(Guid subscriptionId, string? reason = null, TokenModel tokenModel = null);
    Task<bool> SuspendSubscriptionAsync(Guid subscriptionId, string? reason = null, TokenModel tokenModel = null);
    Task<bool> RenewSubscriptionAsync(Guid subscriptionId, string? reason = null, TokenModel tokenModel = null);
    Task<bool> ExpireSubscriptionAsync(Guid subscriptionId, string? reason = null, TokenModel tokenModel = null);
    Task<bool> MarkPaymentFailedAsync(Guid subscriptionId, string? reason = null, TokenModel tokenModel = null);
    Task<bool> MarkPaymentSucceededAsync(Guid subscriptionId, string? reason = null, TokenModel tokenModel = null);
    Task<bool> UpdateSubscriptionStatusAsync(Guid subscriptionId, string newStatus, string? reason = null, TokenModel tokenModel = null);
    Task<IEnumerable<SubscriptionStatusHistory>> GetStatusHistoryAsync(Guid subscriptionId, TokenModel tokenModel = null);
    Task<bool> ValidateStatusTransitionAsync(string currentStatus, string newStatus, TokenModel tokenModel = null);
    Task<string> GetNextValidStatusAsync(string currentStatus, TokenModel tokenModel = null);
    
    // Process methods for automation
    Task<bool> ProcessSubscriptionExpirationAsync(Guid subscriptionId, TokenModel tokenModel = null);
    Task<bool> ProcessSubscriptionSuspensionAsync(Guid subscriptionId, string reason, TokenModel tokenModel = null);
    
    // Trial management methods
    Task<JsonModel> ExtendTrialAsync(string subscriptionId, int additionalDays, string reason = null);
    Task<JsonModel> ConvertTrialToActiveAsync(string subscriptionId, string paymentMethodId = null);
    Task<JsonModel> ProcessTrialExpirationAsync(string subscriptionId);
}
