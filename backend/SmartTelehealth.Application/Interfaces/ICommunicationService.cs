using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface ICommunicationService
{
    #region Generic SMS Methods
    Task<JsonModel> SendSmsAsync(string phoneNumber, string message, TokenModel tokenModel = null);
    Task<JsonModel> SendBulkSmsAsync(List<string> phoneNumbers, string message, TokenModel tokenModel = null);
    #endregion

    #region Generic Email Methods
    Task<JsonModel> SendEmailAsync(string to, string subject, string body, bool isHtml = true, TokenModel tokenModel = null);
    Task<JsonModel> SendEmailAsync(string to, string from, string subject, string body, bool isHtml = true, TokenModel tokenModel = null);
    Task<JsonModel> SendBulkEmailAsync(List<string> toEmails, string subject, string body, bool isHtml = true, TokenModel tokenModel = null);
    Task<JsonModel> SendEmailWithAttachmentAsync(string to, string subject, string body, string attachmentPath, bool isHtml = true, TokenModel tokenModel = null);
    Task<JsonModel> SendEmailWithAttachmentsAsync(string to, string subject, string body, List<string> attachmentPaths, bool isHtml = true, TokenModel tokenModel = null);
    #endregion

    #region Utility Methods
    Task<JsonModel> ValidatePhoneNumberAsync(string phoneNumber, TokenModel tokenModel = null);
    Task<JsonModel> ValidateEmailAsync(string email, TokenModel tokenModel = null);
    Task<JsonModel> GetSmsDeliveryStatusAsync(string messageSid, TokenModel tokenModel = null);
    Task<JsonModel> GetEmailDeliveryStatusAsync(string messageId, TokenModel tokenModel = null);
    Task<JsonModel> GetSmsHistoryAsync(string phoneNumber, DateTime startDate, DateTime endDate, TokenModel tokenModel = null);
    Task<JsonModel> GetEmailHistoryAsync(string email, DateTime startDate, DateTime endDate, TokenModel tokenModel = null);
    #endregion
}
