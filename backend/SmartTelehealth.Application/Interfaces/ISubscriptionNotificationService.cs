using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface ISubscriptionNotificationService
{
    /// <summary>
    /// Send notification when trial is ending soon
    /// </summary>
    Task<JsonModel> SendTrialEndingNotificationAsync(string subscriptionId, int daysRemaining, TokenModel tokenModel);

    /// <summary>
    /// Send notification when payment fails
    /// </summary>
    Task<JsonModel> SendPaymentFailedNotificationAsync(string subscriptionId, string errorMessage, TokenModel tokenModel);

    /// <summary>
    /// Send notification when subscription expires
    /// </summary>
    Task<JsonModel> SendSubscriptionExpiredNotificationAsync(string subscriptionId, TokenModel tokenModel);

    /// <summary>
    /// Send notification when subscription is renewed
    /// </summary>
    Task<JsonModel> SendSubscriptionRenewedNotificationAsync(string subscriptionId, DateTime newEndDate, TokenModel tokenModel);

    /// <summary>
    /// Send notification when plan is changed
    /// </summary>
    Task<JsonModel> SendPlanChangeNotificationAsync(string subscriptionId, string oldPlanName, string newPlanName, TokenModel tokenModel);

    /// <summary>
    /// Send bulk notifications to multiple subscriptions
    /// </summary>
    Task<JsonModel> SendBulkNotificationAsync(List<string> subscriptionIds, string title, string message, string type, TokenModel tokenModel);

    /// <summary>
    /// Send notification when subscription is created
    /// </summary>
    Task<JsonModel> SendSubscriptionCreatedNotificationAsync(string subscriptionId, TokenModel tokenModel);

    /// <summary>
    /// Send notification when subscription is upgraded
    /// </summary>
    Task<JsonModel> SendSubscriptionUpgradedNotificationAsync(string subscriptionId, string oldPlanName, string newPlanName, TokenModel tokenModel);

    /// <summary>
    /// Send notification when subscription is downgraded
    /// </summary>
    Task<JsonModel> SendSubscriptionDowngradedNotificationAsync(string subscriptionId, string oldPlanName, string newPlanName, TokenModel tokenModel);

    /// <summary>
    /// Send billing reminder notification
    /// </summary>
    Task<JsonModel> SendBillingReminderNotificationAsync(string subscriptionId, DateTime dueDate, decimal amount, TokenModel tokenModel);
}
