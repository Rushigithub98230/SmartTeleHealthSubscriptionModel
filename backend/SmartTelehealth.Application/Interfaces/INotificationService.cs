using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface INotificationService
{
    // CRUD operations
    Task<JsonModel> GetNotificationsAsync(TokenModel tokenModel);
    Task<JsonModel> GetNotificationAsync(Guid id, TokenModel tokenModel);
    Task<JsonModel> CreateNotificationAsync(CreateNotificationDto createNotificationDto, TokenModel tokenModel);
    Task<JsonModel> UpdateNotificationAsync(Guid id, object updateNotificationDto, TokenModel tokenModel);
    Task<JsonModel> DeleteNotificationAsync(Guid id, TokenModel tokenModel);
    
    // Email notifications
    Task<JsonModel> SendWelcomeEmailAsync(string email, string userName, TokenModel tokenModel);
    Task<JsonModel> SendEmailVerificationAsync(string email, string userName, string verificationToken, TokenModel tokenModel);
    Task<JsonModel> SendSubscriptionConfirmationAsync(string email, string userName, SubscriptionDto subscription, TokenModel tokenModel);
    Task<JsonModel> SendSubscriptionWelcomeEmailAsync(string email, string userName, SubscriptionDto subscription, TokenModel tokenModel);
    Task<JsonModel> SendSubscriptionCancellationAsync(string email, string userName, SubscriptionDto subscription, TokenModel tokenModel);
    Task<JsonModel> SendSubscriptionSuspensionAsync(string email, string userName, SubscriptionDto subscription, TokenModel tokenModel);
    Task<JsonModel> SendPaymentReminderAsync(string email, string userName, BillingRecordDto billingRecord, TokenModel tokenModel);
    Task<JsonModel> SendConsultationReminderAsync(string email, string userName, ConsultationDto consultation, TokenModel tokenModel);
    Task<JsonModel> SendPasswordResetEmailAsync(string email, string resetToken, TokenModel tokenModel);
    Task<JsonModel> SendDeliveryNotificationAsync(string email, string userName, MedicationDeliveryDto delivery, TokenModel tokenModel);
    Task<JsonModel> SendSubscriptionPausedNotificationAsync(string email, string userName, SubscriptionDto subscription, TokenModel tokenModel);
    Task<JsonModel> SendSubscriptionResumedNotificationAsync(string email, string userName, SubscriptionDto subscription, TokenModel tokenModel);
    Task<JsonModel> SendSubscriptionCancelledNotificationAsync(string email, string userName, SubscriptionDto subscription, TokenModel tokenModel);
    Task<JsonModel> SendProviderMessageNotificationAsync(string email, string userName, MessageDto message, TokenModel tokenModel);
    
    // Billing email notifications
    Task<JsonModel> SendPaymentSuccessEmailAsync(string email, string userName, BillingRecordDto billingRecord, TokenModel tokenModel);
    Task<JsonModel> SendPaymentFailedEmailAsync(string email, string userName, BillingRecordDto billingRecord, TokenModel tokenModel);
    Task<JsonModel> SendRefundProcessedEmailAsync(string email, string userName, BillingRecordDto billingRecord, decimal refundAmount, TokenModel tokenModel);
    Task<JsonModel> SendOverduePaymentEmailAsync(string email, string userName, BillingRecordDto billingRecord, TokenModel tokenModel);
    
    // In-app notifications
    Task<JsonModel> CreateInAppNotificationAsync(int userId, string title, string message, TokenModel tokenModel);
    Task<JsonModel> GetUserNotificationsAsync(int userId, TokenModel tokenModel);
    Task<JsonModel> MarkNotificationAsReadAsync(Guid notificationId, TokenModel tokenModel);
    Task<JsonModel> GetUnreadNotificationCountAsync(int userId, TokenModel tokenModel);
    
    // Utility methods
    Task<JsonModel> IsEmailValidAsync(string email, TokenModel tokenModel);
    Task<JsonModel> SendSmsAsync(string phoneNumber, string message, TokenModel tokenModel);
    Task<JsonModel> SendNotificationAsync(int userId, string title, string message, TokenModel tokenModel);
    
    // Added missing methods for BillingService and AutomatedBillingService
    Task<JsonModel> SendSubscriptionSuspendedNotificationAsync(int userId, string subscriptionId, TokenModel tokenModel);
    Task<JsonModel> SendRefundNotificationAsync(int userId, decimal amount, string billingRecordId, TokenModel tokenModel);
    Task<JsonModel> SendSubscriptionReactivatedNotificationAsync(int userId, string subscriptionId, TokenModel tokenModel);
} 