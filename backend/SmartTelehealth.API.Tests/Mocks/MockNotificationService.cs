using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.API.Tests.Mocks
{
    public class MockNotificationService : INotificationService
    {
        private readonly List<string> _sentEmails;
        private readonly List<string> _sentSms;
        private bool _shouldFail;
        private string? _failureReason;

        public MockNotificationService(bool shouldFail = false, string failureReason = null)
        {
            _sentEmails = new List<string>();
            _sentSms = new List<string>();
            _shouldFail = shouldFail;
            _failureReason = failureReason;
        }

        public async Task<JsonModel> SendEmailAsync(string to, string subject, string body, string from = null)
        {
            if (_shouldFail)
            {
                return new JsonModel
                {
                    StatusCode = 400,
                    Message = _failureReason ?? "Mock email sending failed",
                    data = null
                };
            }

            var emailInfo = $"To: {to}, Subject: {subject}, From: {from ?? "noreply@smarttelehealth.com"}";
            _sentEmails.Add(emailInfo);

            return new JsonModel
            {
                StatusCode = 200,
                Message = "Mock email sent successfully",
                data = new { emailId = Guid.NewGuid().ToString(), to, subject }
            };
        }

        public async Task<JsonModel> SendSmsAsync(string to, string message, string from = null)
        {
            if (_shouldFail)
            {
                return new JsonModel
                {
                    StatusCode = 400,
                    Message = _failureReason ?? "Mock SMS sending failed",
                    data = null
                };
            }

            var smsInfo = $"To: {to}, Message: {message}, From: {from ?? "+1234567890"}";
            _sentSms.Add(smsInfo);

            return new JsonModel
            {
                StatusCode = 200,
                Message = "Mock SMS sent successfully",
                data = new { smsId = Guid.NewGuid().ToString(), to, message }
            };
        }

        public async Task<JsonModel> SendSmsAsync(string phoneNumber, string message, TokenModel tokenModel)
        {
            return await SendSmsAsync(phoneNumber, message);
        }

        // Implement all required interface methods
        public async Task<JsonModel> GetNotificationsAsync(TokenModel tokenModel)
        {
            return new JsonModel { StatusCode = 200, Message = "Mock notifications retrieved", data = new List<object>() };
        }

        public async Task<JsonModel> GetNotificationAsync(Guid id, TokenModel tokenModel)
        {
            return new JsonModel { StatusCode = 200, Message = "Mock notification retrieved", data = new { id } };
        }

        public async Task<JsonModel> CreateNotificationAsync(CreateNotificationDto createNotificationDto, TokenModel tokenModel)
        {
            return new JsonModel { StatusCode = 200, Message = "Mock notification created", data = new { id = Guid.NewGuid() } };
        }

        public async Task<JsonModel> UpdateNotificationAsync(Guid id, object updateNotificationDto, TokenModel tokenModel)
        {
            return new JsonModel { StatusCode = 200, Message = "Mock notification updated", data = new { id } };
        }

        public async Task<JsonModel> DeleteNotificationAsync(Guid id, TokenModel tokenModel)
        {
            return new JsonModel { StatusCode = 200, Message = "Mock notification deleted", data = true };
        }

        public async Task<JsonModel> SendWelcomeEmailAsync(string email, string userName, TokenModel tokenModel)
        {
            return await SendEmailAsync(email, "Welcome", $"Welcome {userName}!");
        }

        public async Task<JsonModel> SendEmailVerificationAsync(string email, string userName, string verificationToken, TokenModel tokenModel)
        {
            return await SendEmailAsync(email, "Email Verification", $"Verify your email with token: {verificationToken}");
        }

        public async Task<JsonModel> SendSubscriptionConfirmationAsync(string email, string userName, SubscriptionDto subscription, TokenModel tokenModel)
        {
            return await SendEmailAsync(email, "Subscription Confirmed", $"Your subscription is confirmed, {userName}!");
        }

        public async Task<JsonModel> SendSubscriptionWelcomeEmailAsync(string email, string userName, SubscriptionDto subscription, TokenModel tokenModel)
        {
            return await SendEmailAsync(email, "Welcome to Your Subscription", $"Welcome to your subscription, {userName}!");
        }

        public async Task<JsonModel> SendSubscriptionCancellationAsync(string email, string userName, SubscriptionDto subscription, TokenModel tokenModel)
        {
            return await SendEmailAsync(email, "Subscription Cancelled", $"Your subscription has been cancelled, {userName}.");
        }

        public async Task<JsonModel> SendSubscriptionSuspensionAsync(string email, string userName, SubscriptionDto subscription, TokenModel tokenModel)
        {
            return await SendEmailAsync(email, "Subscription Suspended", $"Your subscription has been suspended, {userName}.");
        }

        public async Task<JsonModel> SendPaymentReminderAsync(string email, string userName, BillingRecordDto billingRecord, TokenModel tokenModel)
        {
            return await SendEmailAsync(email, "Payment Reminder", $"Please complete your payment, {userName}.");
        }

        public async Task<JsonModel> SendConsultationReminderAsync(string email, string userName, ConsultationDto consultation, TokenModel tokenModel)
        {
            return await SendEmailAsync(email, "Consultation Reminder", $"Reminder about your consultation, {userName}.");
        }

        public async Task<JsonModel> SendPasswordResetEmailAsync(string email, string resetToken, TokenModel tokenModel)
        {
            return await SendEmailAsync(email, "Password Reset", $"Reset your password with token: {resetToken}");
        }

        public async Task<JsonModel> SendDeliveryNotificationAsync(string email, string userName, MedicationDeliveryDto delivery, TokenModel tokenModel)
        {
            return await SendEmailAsync(email, "Medication Delivery", $"Your medication has been delivered, {userName}.");
        }

        public async Task<JsonModel> SendSubscriptionPausedNotificationAsync(string email, string userName, SubscriptionDto subscription, TokenModel tokenModel)
        {
            return await SendEmailAsync(email, "Subscription Paused", $"Your subscription has been paused, {userName}.");
        }

        public async Task<JsonModel> SendSubscriptionResumedNotificationAsync(string email, string userName, SubscriptionDto subscription, TokenModel tokenModel)
        {
            return await SendEmailAsync(email, "Subscription Resumed", $"Your subscription has been resumed, {userName}.");
        }

        public async Task<JsonModel> SendSubscriptionCancelledNotificationAsync(string email, string userName, SubscriptionDto subscription, TokenModel tokenModel)
        {
            return await SendEmailAsync(email, "Subscription Cancelled", $"Your subscription has been cancelled, {userName}.");
        }

        public async Task<JsonModel> SendProviderMessageNotificationAsync(string email, string userName, MessageDto message, TokenModel tokenModel)
        {
            return await SendEmailAsync(email, "New Message", $"You have a new message, {userName}.");
        }

        public async Task<JsonModel> SendPaymentSuccessEmailAsync(string email, string userName, BillingRecordDto billingRecord, TokenModel tokenModel)
        {
            return await SendEmailAsync(email, "Payment Successful", $"Your payment was successful, {userName}.");
        }

        public async Task<JsonModel> SendPaymentFailedEmailAsync(string email, string userName, BillingRecordDto billingRecord, TokenModel tokenModel)
        {
            return await SendEmailAsync(email, "Payment Failed", $"Your payment failed, {userName}.");
        }

        public async Task<JsonModel> SendRefundProcessedEmailAsync(string email, string userName, BillingRecordDto billingRecord, decimal refundAmount, TokenModel tokenModel)
        {
            return await SendEmailAsync(email, "Refund Processed", $"Your refund of ${refundAmount} has been processed, {userName}.");
        }

        public async Task<JsonModel> SendOverduePaymentEmailAsync(string email, string userName, BillingRecordDto billingRecord, TokenModel tokenModel)
        {
            return await SendEmailAsync(email, "Payment Overdue", $"Your payment is overdue, {userName}.");
        }

        public async Task<JsonModel> CreateInAppNotificationAsync(int userId, string title, string message, TokenModel tokenModel)
        {
            return new JsonModel { StatusCode = 200, Message = "Mock in-app notification created", data = new { userId, title, message } };
        }

        public async Task<JsonModel> GetUserNotificationsAsync(int userId, TokenModel tokenModel)
        {
            return new JsonModel { StatusCode = 200, Message = "Mock user notifications retrieved", data = new List<object>() };
        }

        public async Task<JsonModel> MarkNotificationAsReadAsync(Guid notificationId, TokenModel tokenModel)
        {
            return new JsonModel { StatusCode = 200, Message = "Mock notification marked as read", data = true };
        }

        public async Task<JsonModel> GetUnreadNotificationCountAsync(int userId, TokenModel tokenModel)
        {
            return new JsonModel { StatusCode = 200, Message = "Mock unread count retrieved", data = 0 };
        }

        public async Task<JsonModel> IsEmailValidAsync(string email, TokenModel tokenModel)
        {
            return new JsonModel { StatusCode = 200, Message = "Mock email validation", data = true };
        }

        public async Task<JsonModel> SendNotificationAsync(int userId, string title, string message, TokenModel tokenModel)
        {
            return new JsonModel { StatusCode = 200, Message = "Mock notification sent", data = new { userId, title, message } };
        }

        public async Task<JsonModel> SendSubscriptionSuspendedNotificationAsync(int userId, string subscriptionId, TokenModel tokenModel)
        {
            return new JsonModel { StatusCode = 200, Message = "Mock subscription suspended notification sent", data = new { userId, subscriptionId } };
        }

        public async Task<JsonModel> SendRefundNotificationAsync(int userId, decimal amount, string billingRecordId, TokenModel tokenModel)
        {
            return new JsonModel { StatusCode = 200, Message = "Mock refund notification sent", data = new { userId, amount, billingRecordId } };
        }

        public async Task<JsonModel> SendSubscriptionReactivatedNotificationAsync(int userId, string subscriptionId, TokenModel tokenModel)
        {
            return new JsonModel { StatusCode = 200, Message = "Mock subscription reactivated notification sent", data = new { userId, subscriptionId } };
        }

        // Helper methods for testing
        public List<string> GetSentEmails() => new List<string>(_sentEmails);
        public List<string> GetSentSms() => new List<string>(_sentSms);
        public void ClearSentNotifications() { _sentEmails.Clear(); _sentSms.Clear(); }
        public void SetFailureMode(bool shouldFail, string failureReason = null) { _shouldFail = shouldFail; _failureReason = failureReason; }
        public bool HasSentEmailTo(string email) => _sentEmails.Any(e => e.Contains($"To: {email}"));
        public bool HasSentSmsTo(string phone) => _sentSms.Any(s => s.Contains($"To: {phone}"));
    }
}
