using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Application.Interfaces;

/// <summary>
/// Service interface for managing subscription lifecycle operations including:
/// - Subscription creation, cancellation, pausing, resumption
/// - Subscription upgrades, renewals, and billing cycle changes
/// - Bulk lifecycle operations
/// - Status transitions and validation
/// - Trial management
/// </summary>
public interface ISubscriptionLifecycleService
{
    // Core Lifecycle Methods
    Task<JsonModel> CreateSubscriptionAsync(CreateSubscriptionDto createDto, TokenModel tokenModel);
    Task<JsonModel> CancelSubscriptionAsync(string subscriptionId, string? reason, TokenModel tokenModel);
    Task<JsonModel> PauseSubscriptionAsync(string subscriptionId, TokenModel tokenModel);
    Task<JsonModel> ResumeSubscriptionAsync(string subscriptionId, TokenModel tokenModel);
    Task<JsonModel> ReactivateSubscriptionAsync(string subscriptionId, TokenModel tokenModel);
    Task<JsonModel> UpgradeSubscriptionAsync(string subscriptionId, string newPlanId, TokenModel tokenModel);
    Task<JsonModel> AutoRenewSubscriptionAsync(string subscriptionId, TokenModel tokenModel);
    Task<JsonModel> ProrateUpgradeAsync(string subscriptionId, string newPlanId, TokenModel tokenModel);
    Task<JsonModel> ChangeBillingCycleAsync(string subscriptionId, string newBillingCycleId, TokenModel tokenModel);
    Task<JsonModel> ExtendUserSubscriptionAsync(string subscriptionId, int additionalDays, TokenModel tokenModel);
    Task<JsonModel> UpdateSubscriptionAsync(string subscriptionId, UpdateSubscriptionDto updateDto, TokenModel tokenModel);

    // Bulk Lifecycle Methods
    Task<JsonModel> BulkCancelSubscriptionsAsync(IEnumerable<string> subscriptionIds, string adminUserId, TokenModel tokenModel, string? reason = null);
    Task<JsonModel> BulkUpgradeSubscriptionsAsync(IEnumerable<string> subscriptionIds, string newPlanId, string adminUserId, TokenModel tokenModel);
    Task<JsonModel> PerformBulkActionAsync(List<BulkActionRequestDto> actions, TokenModel tokenModel);

    // Status Management Methods
    Task<bool> ActivateSubscriptionAsync(Guid subscriptionId, string? reason = null, TokenModel tokenModel = null);
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
    Task<JsonModel> ProcessSubscriptionExpirationAsync(string subscriptionId);
    
    // Trial management methods
    Task<JsonModel> ExtendTrialAsync(string subscriptionId, int additionalDays, string reason = null);
    Task<JsonModel> ConvertTrialToActiveAsync(string subscriptionId, string paymentMethodId = null);
    Task<JsonModel> ProcessTrialExpirationAsync(string subscriptionId);
    
    // Additional lifecycle methods
    Task<JsonModel> ProcessStateTransitionAsync(string subscriptionId, string newStatus, string reason = null, string changedByUserId = null, TokenModel tokenModel = null);
    Task<JsonModel> GetSubscriptionLifecycleStatusAsync(string subscriptionId, TokenModel tokenModel = null);
    Task<JsonModel> ProcessBulkStateTransitionsAsync(IEnumerable<string> subscriptionIds, string newStatus, string reason = null, string changedByUserId = null, TokenModel tokenModel = null);
}
