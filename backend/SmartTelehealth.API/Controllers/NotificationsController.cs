using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class NotificationsController : BaseController
{
    private readonly INotificationService _notificationService;
    
    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<JsonModel> GetAllNotifications()
    {
        return await _notificationService.GetNotificationsAsync(GetToken(HttpContext));
    }

    [HttpGet("{id}")]
    public async Task<JsonModel> GetNotification(Guid id)
    {
        return await _notificationService.GetNotificationAsync(id, GetToken(HttpContext));
    }

    [HttpPost]
    public async Task<JsonModel> CreateNotification([FromBody] CreateNotificationDto createNotificationDto)
    {
        return await _notificationService.CreateNotificationAsync(createNotificationDto, GetToken(HttpContext));
    }

    [HttpPut("{id}")]
    public async Task<JsonModel> UpdateNotification(Guid id, [FromBody] UpdateNotificationDto updateNotificationDto)
    {
        if (id != updateNotificationDto.Id)
            return new JsonModel { data = new object(), Message = "ID mismatch", StatusCode = 400 };
        return await _notificationService.UpdateNotificationAsync(id, updateNotificationDto, GetToken(HttpContext));
    }

    [HttpDelete("{id}")]
    public async Task<JsonModel> DeleteNotification(Guid id)
    {
        return await _notificationService.DeleteNotificationAsync(id, GetToken(HttpContext));
    }

    [HttpGet("user/{userId}")]
    public async Task<JsonModel> GetUserNotifications(int userId)
    {
        return await _notificationService.GetUserNotificationsAsync(userId, GetToken(HttpContext));
    }

    [HttpPost("mark-read/{id}")]
    public async Task<JsonModel> MarkNotificationAsRead(Guid id)
    {
        return await _notificationService.MarkNotificationAsReadAsync(id, GetToken(HttpContext));
    }

    [HttpGet("unread-count/{userId}")]
    public async Task<JsonModel> GetUnreadNotificationCount(int userId)
    {
        return await _notificationService.GetUnreadNotificationCountAsync(userId, GetToken(HttpContext));
    }

    [HttpPost("in-app")]
    public async Task<JsonModel> CreateInAppNotification([FromBody] CreateInAppNotificationDto createDto)
    {
        return await _notificationService.CreateInAppNotificationAsync(createDto.UserId, createDto.Title, createDto.Message, GetToken(HttpContext));
    }

    [HttpPost("email/welcome")]
    public async Task<JsonModel> SendWelcomeEmail([FromBody] SendEmailDto emailDto)
    {
        return await _notificationService.SendWelcomeEmailAsync(emailDto.Email, emailDto.UserName, GetToken(HttpContext));
    }

    [HttpPost("email/verification")]
    public async Task<JsonModel> SendEmailVerification([FromBody] SendEmailVerificationDto emailDto)
    {
        return await _notificationService.SendEmailVerificationAsync(emailDto.Email, emailDto.UserName, emailDto.VerificationToken, GetToken(HttpContext));
    }

    [HttpPost("email/subscription-confirmation")]
    public async Task<JsonModel> SendSubscriptionConfirmation([FromBody] SendSubscriptionEmailDto emailDto)
    {
        return await _notificationService.SendSubscriptionConfirmationAsync(emailDto.Email, emailDto.UserName, emailDto.Subscription, GetToken(HttpContext));
    }

    [HttpPost("email/payment-reminder")]
    public async Task<JsonModel> SendPaymentReminder([FromBody] SendPaymentReminderDto emailDto)
    {
        return await _notificationService.SendPaymentReminderAsync(emailDto.Email, emailDto.UserName, emailDto.BillingRecord, GetToken(HttpContext));
    }

    [HttpPost("email/consultation-reminder")]
    public async Task<JsonModel> SendConsultationReminder([FromBody] SendConsultationReminderDto emailDto)
    {
        return await _notificationService.SendConsultationReminderAsync(emailDto.Email, emailDto.UserName, emailDto.Consultation, GetToken(HttpContext));
    }

    [HttpPost("email/password-reset")]
    public async Task<JsonModel> SendPasswordResetEmail([FromBody] SendPasswordResetDto emailDto)
    {
        return await _notificationService.SendPasswordResetEmailAsync(emailDto.Email, emailDto.ResetToken, GetToken(HttpContext));
    }

    [HttpPost("email/delivery-notification")]
    public async Task<JsonModel> SendDeliveryNotification([FromBody] SendDeliveryNotificationDto emailDto)
    {
        return await _notificationService.SendDeliveryNotificationAsync(emailDto.Email, emailDto.UserName, emailDto.Delivery, GetToken(HttpContext));
    }

    [HttpPost("email/payment-success")]
    public async Task<JsonModel> SendPaymentSuccessEmail([FromBody] SendPaymentEmailDto emailDto)
    {
        return await _notificationService.SendPaymentSuccessEmailAsync(emailDto.Email, emailDto.UserName, emailDto.BillingRecord, GetToken(HttpContext));
    }

    [HttpPost("email/payment-failed")]
    public async Task<JsonModel> SendPaymentFailedEmail([FromBody] SendPaymentEmailDto emailDto)
    {
        return await _notificationService.SendPaymentFailedEmailAsync(emailDto.Email, emailDto.UserName, emailDto.BillingRecord, GetToken(HttpContext));
    }

    [HttpPost("email/refund-processed")]
    public async Task<JsonModel> SendRefundProcessedEmail([FromBody] SendRefundEmailDto emailDto)
    {
        return await _notificationService.SendRefundProcessedEmailAsync(emailDto.Email, emailDto.UserName, emailDto.BillingRecord, emailDto.RefundAmount, GetToken(HttpContext));
    }

    [HttpPost("email/overdue-payment")]
    public async Task<JsonModel> SendOverduePaymentEmail([FromBody] SendPaymentEmailDto emailDto)
    {
        return await _notificationService.SendOverduePaymentEmailAsync(emailDto.Email, emailDto.UserName, emailDto.BillingRecord, GetToken(HttpContext));
    }

    [HttpPost("sms")]
    public async Task<JsonModel> SendSms([FromBody] SendSmsDto smsDto)
    {
        return await _notificationService.SendSmsAsync(smsDto.PhoneNumber, smsDto.Message, GetToken(HttpContext));
    }

    [HttpPost("validate-email")]
    public async Task<JsonModel> ValidateEmail([FromBody] ValidateEmailDto emailDto)
    {
        return await _notificationService.IsEmailValidAsync(emailDto.Email, GetToken(HttpContext));
    }

    [HttpPost("subscription-suspended")]
    public async Task<JsonModel> SendSubscriptionSuspendedNotification([FromBody] SendSubscriptionNotificationDto notificationDto)
    {
        return await _notificationService.SendSubscriptionSuspendedNotificationAsync(notificationDto.UserId, notificationDto.SubscriptionId, GetToken(HttpContext));
    }

    [HttpPost("refund")]
    public async Task<JsonModel> SendRefundNotification([FromBody] SendRefundNotificationDto notificationDto)
    {
        return await _notificationService.SendRefundNotificationAsync(notificationDto.UserId, notificationDto.Amount, notificationDto.BillingRecordId, GetToken(HttpContext));
    }

    [HttpPost("subscription-reactivated")]
    public async Task<JsonModel> SendSubscriptionReactivatedNotification([FromBody] SendSubscriptionNotificationDto notificationDto)
    {
        return await _notificationService.SendSubscriptionReactivatedNotificationAsync(notificationDto.UserId, notificationDto.SubscriptionId, GetToken(HttpContext));
    }
}

// DTOs for the controller
public class CreateInAppNotificationDto
{
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class SendEmailDto
{
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
}

public class SendEmailVerificationDto
{
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string VerificationToken { get; set; } = string.Empty;
}

public class SendSubscriptionEmailDto
{
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public SubscriptionDto Subscription { get; set; } = new();
}

public class SendPaymentReminderDto
{
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public BillingRecordDto BillingRecord { get; set; } = new();
}

public class SendConsultationReminderDto
{
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public ConsultationDto Consultation { get; set; } = new();
}

public class SendPasswordResetDto
{
    public string Email { get; set; } = string.Empty;
    public string ResetToken { get; set; } = string.Empty;
}

public class SendDeliveryNotificationDto
{
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public MedicationDeliveryDto Delivery { get; set; } = new();
}

public class SendPaymentEmailDto
{
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public BillingRecordDto BillingRecord { get; set; } = new();
}

public class SendRefundEmailDto
{
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public BillingRecordDto BillingRecord { get; set; } = new();
    public decimal RefundAmount { get; set; }
}

public class SendSmsDto
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class ValidateEmailDto
{
    public string Email { get; set; } = string.Empty;
}

public class SendSubscriptionNotificationDto
{
    public int UserId { get; set; }
    public string SubscriptionId { get; set; } = string.Empty;
}

public class SendRefundNotificationDto
{
    public int UserId { get; set; }
    public decimal Amount { get; set; }
    public string BillingRecordId { get; set; } = string.Empty;
}

// DTO for creating test notifications
public class CreateTestNotificationDto
{
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
} 