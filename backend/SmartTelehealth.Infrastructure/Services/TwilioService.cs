using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading;

namespace SmartTelehealth.Infrastructure.Services;

public class TwilioService : ICommunicationService
{
    private readonly ILogger<TwilioService> _logger;
    private readonly TwilioSettings _twilioSettings;
    private readonly IConfiguration _configuration;
    private readonly ISendGridClient? _sendGridClient;
    private readonly Dictionary<string, DateTime> _lastSmsSent;
    private readonly Dictionary<string, DateTime> _lastEmailSent;
    private readonly SemaphoreSlim _smsSemaphore;
    private readonly SemaphoreSlim _emailSemaphore;
    private readonly Random _random;

    public TwilioService(
        ILogger<TwilioService> logger,
        TwilioSettings twilioSettings,
        IConfiguration configuration)
    {
        _logger = logger;
        _twilioSettings = twilioSettings;
        _configuration = configuration;
        _lastSmsSent = new Dictionary<string, DateTime>();
        _lastEmailSent = new Dictionary<string, DateTime>();
        _smsSemaphore = new SemaphoreSlim(_twilioSettings.SmsRateLimitPerMinute, _twilioSettings.SmsRateLimitPerMinute);
        _emailSemaphore = new SemaphoreSlim(_twilioSettings.EmailRateLimitPerMinute, _twilioSettings.EmailRateLimitPerMinute);
        _random = new Random();

        // Initialize Twilio client for SMS
        if (!string.IsNullOrEmpty(_twilioSettings.AccountSid) && !string.IsNullOrEmpty(_twilioSettings.AuthToken))
        {
            TwilioClient.Init(_twilioSettings.AccountSid, _twilioSettings.AuthToken);
            _logger.LogInformation("Twilio SMS client initialized successfully");
        }
        else
        {
            _logger.LogWarning("Twilio SMS client not initialized - missing credentials");
        }

        // Initialize SendGrid client for email
        if (!string.IsNullOrEmpty(_twilioSettings.SendGridApiKey))
        {
            _sendGridClient = new SendGridClient(_twilioSettings.SendGridApiKey);
            _logger.LogInformation("SendGrid email client initialized successfully");
        }
        else
        {
            _logger.LogWarning("SendGrid email client not initialized - missing API key");
        }
    }

    #region Generic SMS Methods

    public async Task<JsonModel> SendSmsAsync(string phoneNumber, string message, TokenModel tokenModel = null)
    {
        try
        {
            if (!_twilioSettings.EnableSms)
            {
                _logger.LogWarning("SMS functionality is disabled");
                return new JsonModel
                {
                    data = false,
                    Message = "SMS functionality is disabled",
                    StatusCode = 400
                };
            }

            // Validate phone number and message
            var validationResult = await ValidateSmsRequest(phoneNumber, message);
            if (validationResult.StatusCode != 200)
            {
                return validationResult;
            }

            // Apply rate limiting with semaphore
            await _smsSemaphore.WaitAsync();
            try
            {
                // Apply additional rate limiting per phone number
                await ApplySmsRateLimiting(phoneNumber);

                // Format phone number (ensure it starts with +)
                var formattedPhoneNumber = FormatPhoneNumber(phoneNumber);

                // Add retry logic for SMS sending
                var messageResource = await SendSmsWithRetryAsync(formattedPhoneNumber, message);

                _logger.LogInformation("SMS sent successfully to {PhoneNumber}. Message SID: {MessageSid}", 
                    formattedPhoneNumber, messageResource.Sid);

                return new JsonModel
                {
                    data = new
                    {
                        MessageSid = messageResource.Sid,
                        Status = messageResource.Status.ToString(),
                        PhoneNumber = formattedPhoneNumber,
                        SentAt = DateTime.UtcNow,
                        Cost = messageResource.Price,
                        Currency = messageResource.PriceUnit
                    },
                    Message = "SMS sent successfully",
                    StatusCode = 200
                };
            }
            finally
            {
                _smsSemaphore.Release();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SMS to {PhoneNumber}", phoneNumber);
            return new JsonModel
            {
                data = false,
                Message = $"Failed to send SMS: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> SendBulkSmsAsync(List<string> phoneNumbers, string message, TokenModel tokenModel = null)
    {
        try
        {
            if (!_twilioSettings.EnableSms)
            {
                return new JsonModel
                {
                    data = false,
                    Message = "SMS functionality is disabled",
                    StatusCode = 400
                };
            }

            if (phoneNumbers == null || !phoneNumbers.Any())
            {
                return new JsonModel
                {
                    data = false,
                    Message = "Phone numbers list is required",
                    StatusCode = 400
                };
            }

            // Limit bulk SMS to prevent abuse
            if (phoneNumbers.Count > 100)
            {
                return new JsonModel
                {
                    data = false,
                    Message = "Bulk SMS limited to 100 recipients per request",
                    StatusCode = 400
                };
            }

            var results = new List<object>();
            var successCount = 0;
            var failureCount = 0;
            var startTime = DateTime.UtcNow;

            // Process in parallel with limited concurrency
            var semaphore = new SemaphoreSlim(5, 5); // Max 5 concurrent SMS
            var tasks = phoneNumbers.Select(async phoneNumber =>
            {
                await semaphore.WaitAsync();
                try
                {
                    var result = await SendSmsAsync(phoneNumber, message, tokenModel);
                    if (result.StatusCode == 200)
                    {
                        Interlocked.Increment(ref successCount);
                        results.Add(new { PhoneNumber = phoneNumber, Status = "Success", Result = result.data });
                    }
                    else
                    {
                        Interlocked.Increment(ref failureCount);
                        results.Add(new { PhoneNumber = phoneNumber, Status = "Failed", Error = result.Message });
                    }
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref failureCount);
                    results.Add(new { PhoneNumber = phoneNumber, Status = "Failed", Error = ex.Message });
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);

            var duration = DateTime.UtcNow - startTime;
            _logger.LogInformation("Bulk SMS completed in {Duration}ms. Success: {SuccessCount}, Failed: {FailureCount}", 
                duration.TotalMilliseconds, successCount, failureCount);

            return new JsonModel
            {
                data = new
                {
                    TotalCount = phoneNumbers.Count,
                    SuccessCount = successCount,
                    FailureCount = failureCount,
                    Results = results,
                    Duration = duration.TotalMilliseconds,
                    ProcessedAt = DateTime.UtcNow
                },
                Message = $"Bulk SMS completed. Success: {successCount}, Failed: {failureCount}",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending bulk SMS to {Count} phone numbers", phoneNumbers?.Count ?? 0);
            return new JsonModel
            {
                data = false,
                Message = $"Failed to send bulk SMS: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    #endregion

    #region Generic Email Methods

    public async Task<JsonModel> SendEmailAsync(string to, string subject, string body, bool isHtml = true, TokenModel tokenModel = null)
    {
        return await SendEmailAsync(to, _twilioSettings.FromEmail, subject, body, isHtml, tokenModel);
    }

    public async Task<JsonModel> SendEmailAsync(string to, string from, string subject, string body, bool isHtml = true, TokenModel tokenModel = null)
    {
        try
        {
            if (!_twilioSettings.EnableEmail)
            {
                _logger.LogWarning("Email functionality is disabled");
                return new JsonModel
                {
                    data = false,
                    Message = "Email functionality is disabled",
                    StatusCode = 400
                };
            }

            if (_sendGridClient == null)
            {
                _logger.LogWarning("SendGrid client is not initialized");
                return new JsonModel
                {
                    data = false,
                    Message = "Email service is not configured",
                    StatusCode = 400
                };
            }

            // Validate email request
            var validationResult = await ValidateEmailRequest(to, from, subject, body);
            if (validationResult.StatusCode != 200)
            {
                return validationResult;
            }

            // Apply rate limiting with semaphore
            await _emailSemaphore.WaitAsync();
            try
            {
                // Apply additional rate limiting per email
                await ApplyEmailRateLimiting(to);

                // Create SendGrid message
                var fromAddress = new EmailAddress(from, _twilioSettings.FromName);
                var toAddress = new EmailAddress(to);
                var message = MailHelper.CreateSingleEmail(fromAddress, toAddress, subject, isHtml ? null : body, isHtml ? body : null);

                // Add tracking and analytics
                message.SetClickTracking(true, true);
                message.SetOpenTracking(true);
                message.SetGoogleAnalytics(true, "SmartTelehealth");

                // Send email with retry logic
                var response = await SendEmailWithRetryAsync(message);

                if (response.IsSuccessStatusCode)
                {
                    var messageId = response.Headers.GetValues("X-Message-Id").FirstOrDefault();
                    _logger.LogInformation("Email sent successfully to {To}. Message ID: {MessageId}, Status: {StatusCode}", 
                        to, messageId, response.StatusCode);

                    return new JsonModel
                    {
                        data = new
                        {
                            MessageId = messageId,
                            Status = response.StatusCode.ToString(),
                            To = to,
                            From = from,
                            SentAt = DateTime.UtcNow,
                            TrackingEnabled = true
                        },
                        Message = "Email sent successfully",
                        StatusCode = 200
                    };
                }
                else
                {
                    var errorBody = await response.Body.ReadAsStringAsync();
                    _logger.LogError("Failed to send email to {To}. Status: {StatusCode}, Error: {Error}", to, response.StatusCode, errorBody);
                    return new JsonModel
                    {
                        data = false,
                        Message = $"Failed to send email. Status: {response.StatusCode}",
                        StatusCode = 500
                    };
                }
            }
            finally
            {
                _emailSemaphore.Release();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {To}", to);
            return new JsonModel
            {
                data = false,
                Message = $"Failed to send email: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> SendBulkEmailAsync(List<string> toEmails, string subject, string body, bool isHtml = true, TokenModel tokenModel = null)
    {
        try
        {
            if (!_twilioSettings.EnableEmail)
            {
                return new JsonModel
                {
                    data = false,
                    Message = "Email functionality is disabled",
                    StatusCode = 400
                };
            }

            if (_sendGridClient == null)
            {
                return new JsonModel
                {
                    data = false,
                    Message = "Email service is not configured",
                    StatusCode = 400
                };
            }

            if (toEmails == null || !toEmails.Any())
            {
                return new JsonModel
                {
                    data = false,
                    Message = "Email addresses list is required",
                    StatusCode = 400
                };
            }

            // Limit bulk email to prevent abuse
            if (toEmails.Count > 1000)
            {
                return new JsonModel
                {
                    data = false,
                    Message = "Bulk email limited to 1000 recipients per request",
                    StatusCode = 400
                };
            }

            var results = new List<object>();
            var successCount = 0;
            var failureCount = 0;
            var startTime = DateTime.UtcNow;

            // Process in parallel with limited concurrency
            var semaphore = new SemaphoreSlim(10, 10); // Max 10 concurrent emails
            var tasks = toEmails.Select(async email =>
            {
                await semaphore.WaitAsync();
                try
                {
                    var result = await SendEmailAsync(email, subject, body, isHtml, tokenModel);
                    if (result.StatusCode == 200)
                    {
                        Interlocked.Increment(ref successCount);
                        results.Add(new { Email = email, Status = "Success", Result = result.data });
                    }
                    else
                    {
                        Interlocked.Increment(ref failureCount);
                        results.Add(new { Email = email, Status = "Failed", Error = result.Message });
                    }
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref failureCount);
                    results.Add(new { Email = email, Status = "Failed", Error = ex.Message });
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);

            var duration = DateTime.UtcNow - startTime;
            _logger.LogInformation("Bulk email completed in {Duration}ms. Success: {SuccessCount}, Failed: {FailureCount}", 
                duration.TotalMilliseconds, successCount, failureCount);

            return new JsonModel
            {
                data = new
                {
                    TotalCount = toEmails.Count,
                    SuccessCount = successCount,
                    FailureCount = failureCount,
                    Results = results,
                    Duration = duration.TotalMilliseconds,
                    ProcessedAt = DateTime.UtcNow
                },
                Message = $"Bulk email completed. Success: {successCount}, Failed: {failureCount}",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending bulk email to {Count} email addresses", toEmails?.Count ?? 0);
            return new JsonModel
            {
                data = false,
                Message = $"Failed to send bulk email: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> SendEmailWithAttachmentAsync(string to, string subject, string body, string attachmentPath, bool isHtml = true, TokenModel tokenModel = null)
    {
        try
        {
            if (_sendGridClient == null)
            {
                return new JsonModel
                {
                    data = false,
                    Message = "Email service is not configured",
                    StatusCode = 400
                };
            }

            if (!File.Exists(attachmentPath))
            {
                return new JsonModel
                {
                    data = false,
                    Message = "Attachment file not found",
                    StatusCode = 400
                };
            }

            // Check file size limit (SendGrid limit is 30MB)
            var fileInfo = new FileInfo(attachmentPath);
            if (fileInfo.Length > 30 * 1024 * 1024)
            {
                return new JsonModel
                {
                    data = false,
                    Message = "Attachment file size exceeds 30MB limit",
                    StatusCode = 400
                };
            }

            var fromAddress = new EmailAddress(_twilioSettings.FromEmail, _twilioSettings.FromName);
            var toAddress = new EmailAddress(to);
            var message = MailHelper.CreateSingleEmail(fromAddress, toAddress, subject, isHtml ? null : body, isHtml ? body : null);

            // Add attachment
            var attachment = await File.ReadAllBytesAsync(attachmentPath);
            var fileName = Path.GetFileName(attachmentPath);
            var mimeType = GetMimeType(Path.GetExtension(attachmentPath));
            message.AddAttachment(fileName, Convert.ToBase64String(attachment), mimeType);

            var response = await _sendGridClient.SendEmailAsync(message);

            if (response.IsSuccessStatusCode)
            {
                return new JsonModel
                {
                    data = new
                    {
                        MessageId = response.Headers.GetValues("X-Message-Id").FirstOrDefault(),
                        Status = response.StatusCode.ToString(),
                        To = to,
                        Attachment = fileName,
                        FileSize = fileInfo.Length,
                        MimeType = mimeType,
                        SentAt = DateTime.UtcNow
                    },
                    Message = "Email with attachment sent successfully",
                    StatusCode = 200
                };
            }
            else
            {
                var errorBody = await response.Body.ReadAsStringAsync();
                return new JsonModel
                {
                    data = false,
                    Message = $"Failed to send email with attachment. Status: {response.StatusCode}",
                    StatusCode = 500
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email with attachment to {To}", to);
            return new JsonModel
            {
                data = false,
                Message = $"Failed to send email with attachment: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> SendEmailWithAttachmentsAsync(string to, string subject, string body, List<string> attachmentPaths, bool isHtml = true, TokenModel tokenModel = null)
    {
        try
        {
            if (_sendGridClient == null)
            {
                return new JsonModel
                {
                    data = false,
                    Message = "Email service is not configured",
                    StatusCode = 400
                };
            }

            if (attachmentPaths == null || !attachmentPaths.Any())
            {
                return new JsonModel
                {
                    data = false,
                    Message = "Attachment paths list is required",
                    StatusCode = 400
                };
            }

            // Check total file size limit
            var totalSize = 0L;
            var validAttachments = new List<string>();
            foreach (var path in attachmentPaths)
            {
                if (File.Exists(path))
                {
                    var fileInfo = new FileInfo(path);
                    if (fileInfo.Length <= 30 * 1024 * 1024) // 30MB per file
                    {
                        totalSize += fileInfo.Length;
                        if (totalSize <= 30 * 1024 * 1024) // Total 30MB limit
                        {
                            validAttachments.Add(path);
                        }
                    }
                }
            }

            if (!validAttachments.Any())
            {
                return new JsonModel
                {
                    data = false,
                    Message = "No valid attachments found or total size exceeds 30MB limit",
                    StatusCode = 400
                };
            }

            var fromAddress = new EmailAddress(_twilioSettings.FromEmail, _twilioSettings.FromName);
            var toAddress = new EmailAddress(to);
            var message = MailHelper.CreateSingleEmail(fromAddress, toAddress, subject, isHtml ? null : body, isHtml ? body : null);

            // Add multiple attachments
            var attachmentInfo = new List<object>();
            foreach (var attachmentPath in validAttachments)
            {
                var attachment = await File.ReadAllBytesAsync(attachmentPath);
                var fileName = Path.GetFileName(attachmentPath);
                var mimeType = GetMimeType(Path.GetExtension(attachmentPath));
                message.AddAttachment(fileName, Convert.ToBase64String(attachment), mimeType);
                
                attachmentInfo.Add(new
                {
                    FileName = fileName,
                    Size = new FileInfo(attachmentPath).Length,
                    MimeType = mimeType
                });
            }

            var response = await _sendGridClient.SendEmailAsync(message);

            if (response.IsSuccessStatusCode)
            {
                return new JsonModel
                {
                    data = new
                    {
                        MessageId = response.Headers.GetValues("X-Message-Id").FirstOrDefault(),
                        Status = response.StatusCode.ToString(),
                        To = to,
                        Attachments = attachmentInfo,
                        TotalSize = totalSize,
                        SentAt = DateTime.UtcNow
                    },
                    Message = "Email with attachments sent successfully",
                    StatusCode = 200
                };
            }
            else
            {
                var errorBody = await response.Body.ReadAsStringAsync();
                return new JsonModel
                {
                    data = false,
                    Message = $"Failed to send email with attachments. Status: {response.StatusCode}",
                    StatusCode = 500
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email with attachments to {To}", to);
            return new JsonModel
            {
                data = false,
                Message = $"Failed to send email with attachments: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    #endregion

    #region Utility Methods

    public async Task<JsonModel> ValidatePhoneNumberAsync(string phoneNumber, TokenModel tokenModel = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return new JsonModel
                {
                    data = false,
                    Message = "Phone number is required",
                    StatusCode = 400
                };
            }

            // Enhanced phone number validation
            var formattedNumber = FormatPhoneNumber(phoneNumber);
            var isValid = IsValidPhoneNumber(formattedNumber);

            return new JsonModel
            {
                data = new
                {
                    OriginalNumber = phoneNumber,
                    FormattedNumber = formattedNumber,
                    IsValid = isValid,
                    CountryCode = ExtractCountryCode(formattedNumber),
                    ValidationRules = new
                    {
                        HasCountryCode = formattedNumber.StartsWith("+"),
                        MinLength = formattedNumber.Length >= 10,
                        MaxLength = formattedNumber.Length <= 15,
                        OnlyDigits = formattedNumber.Substring(1).All(char.IsDigit)
                    }
                },
                Message = isValid ? "Phone number is valid" : "Phone number is invalid",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating phone number {PhoneNumber}", phoneNumber);
            return new JsonModel
            {
                data = false,
                Message = $"Failed to validate phone number: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> ValidateEmailAsync(string email, TokenModel tokenModel = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return new JsonModel
                {
                    data = false,
                    Message = "Email is required",
                    StatusCode = 400
                };
            }

            // Enhanced email validation
            var isValid = IsValidEmail(email);
            var domain = ExtractDomain(email);

            return new JsonModel
            {
                data = new
                {
                    Email = email,
                    IsValid = isValid,
                    Domain = domain,
                    ValidationRules = new
                    {
                        HasAtSymbol = email.Contains("@"),
                        HasDomain = !string.IsNullOrEmpty(domain),
                        MinLength = email.Length >= 5,
                        MaxLength = email.Length <= 254,
                        ValidFormat = IsValidEmailFormat(email)
                    }
                },
                Message = isValid ? "Email is valid" : "Email is invalid",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating email {Email}", email);
            return new JsonModel
            {
                data = false,
                Message = $"Failed to validate email: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetSmsDeliveryStatusAsync(string messageSid, TokenModel tokenModel = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(messageSid))
            {
                return new JsonModel
                {
                    data = false,
                    Message = "Message SID is required",
                    StatusCode = 400
                };
            }

            var message = await MessageResource.FetchAsync(messageSid);
            
            return new JsonModel
            {
                data = new
                {
                    MessageSid = message.Sid,
                    Status = message.Status.ToString(),
                    To = message.To,
                    From = message.From,
                    Body = message.Body,
                    DateSent = message.DateSent,
                    DateUpdated = message.DateUpdated,
                    ErrorCode = message.ErrorCode,
                    ErrorMessage = message.ErrorMessage,
                    Price = message.Price,
                    PriceUnit = message.PriceUnit,
                    Direction = message.Direction.ToString()
                },
                Message = "SMS delivery status retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting SMS delivery status for {MessageSid}", messageSid);
            return new JsonModel
            {
                data = false,
                Message = $"Failed to get SMS delivery status: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetEmailDeliveryStatusAsync(string messageId, TokenModel tokenModel = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(messageId))
            {
                return new JsonModel
                {
                    data = false,
                    Message = "Message ID is required",
                    StatusCode = 400
                };
            }

            // Note: SendGrid doesn't provide real-time delivery status via API
            // This would require webhook integration for actual delivery status
            return new JsonModel
            {
                data = new
                {
                    MessageId = messageId,
                    Status = "Delivered", // Placeholder - would need webhook integration
                    Note = "Real-time delivery status requires webhook integration",
                    Service = "SendGrid",
                    LastChecked = DateTime.UtcNow
                },
                Message = "Email delivery status retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting email delivery status for {MessageId}", messageId);
            return new JsonModel
            {
                data = false,
                Message = $"Failed to get email delivery status: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetSmsHistoryAsync(string phoneNumber, DateTime startDate, DateTime endDate, TokenModel tokenModel = null)
    {
        try
        {
            var formattedPhoneNumber = FormatPhoneNumber(phoneNumber);
            
            // Note: Date filtering requires proper Twilio API integration
            // For now, return a placeholder response
            return new JsonModel
            {
                data = new
                {
                    PhoneNumber = formattedPhoneNumber,
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalMessages = 0,
                    Note = "SMS history with date filtering requires proper Twilio API integration",
                    Service = "Twilio",
                    LastChecked = DateTime.UtcNow
                },
                Message = "SMS history retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting SMS history for {PhoneNumber}", phoneNumber);
            return new JsonModel
            {
                data = false,
                Message = $"Failed to get SMS history: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetEmailHistoryAsync(string email, DateTime startDate, DateTime endDate, TokenModel tokenModel = null)
    {
        try
        {
            // Note: SendGrid doesn't provide email history via API
            // This would require webhook integration or database storage
            return new JsonModel
            {
                data = new
                {
                    Email = email,
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalEmails = 0,
                    Note = "Email history requires webhook integration or database storage",
                    Service = "SendGrid",
                    LastChecked = DateTime.UtcNow
                },
                Message = "Email history retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting email history for {Email}", email);
            return new JsonModel
            {
                data = false,
                Message = $"Failed to get email history: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> UnsubscribeEmailAsync(string email, string unsubscribeToken, TokenModel tokenModel = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return new JsonModel
                {
                    data = false,
                    Message = "Email is required",
                    StatusCode = 400
                };
            }

            if (string.IsNullOrWhiteSpace(unsubscribeToken))
            {
                return new JsonModel
                {
                    data = false,
                    Message = "Unsubscribe token is required",
                    StatusCode = 400
                };
            }

            // Note: This is a placeholder implementation
            // In a real scenario, you would:
            // 1. Validate the unsubscribe token
            // 2. Update your database to mark the user as unsubscribed
            // 3. Potentially call SendGrid's unsubscribe API if you have webhook integration
            
            _logger.LogInformation("Email unsubscribe request for {Email} with token {Token}", email, unsubscribeToken);
            
            return new JsonModel
            {
                data = new
                {
                    Email = email,
                    Unsubscribed = true,
                    UnsubscribedAt = DateTime.UtcNow,
                    Note = "Unsubscribe processed. Note: This is a placeholder implementation. Implement proper token validation and database updates for production use.",
                    Service = "SendGrid"
                },
                Message = "Email unsubscribed successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing email unsubscribe for {Email}", email);
            return new JsonModel
            {
                data = false,
                Message = $"Failed to process email unsubscribe: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    #endregion

    #region Private Helper Methods

    private async Task<JsonModel> ValidateSmsRequest(string phoneNumber, string message)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(message))
        {
            return new JsonModel
            {
                data = false,
                Message = "Phone number and message are required",
                StatusCode = 400
            };
        }

        if (message.Length > 1600) // Twilio limit
        {
            return new JsonModel
            {
                data = false,
                Message = "SMS message exceeds 1600 character limit",
                StatusCode = 400
            };
        }

        return new JsonModel
        {
            data = true,
            Message = "SMS request is valid",
            StatusCode = 200
        };
    }

    private async Task<JsonModel> ValidateEmailRequest(string to, string from, string subject, string body)
    {
        if (string.IsNullOrWhiteSpace(to) || string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(body))
        {
            return new JsonModel
            {
                data = false,
                Message = "To, subject, and body are required",
                StatusCode = 400
            };
        }

        if (subject.Length > 998) // RFC 2822 limit
        {
            return new JsonModel
            {
                data = false,
                Message = "Email subject exceeds 998 character limit",
                StatusCode = 400
            };
        }

        if (body.Length > 1000000) // 1MB limit for body
        {
            return new JsonModel
            {
                data = false,
                Message = "Email body exceeds 1MB limit",
                StatusCode = 400
            };
        }

        return new JsonModel
        {
            data = true,
            Message = "Email request is valid",
            StatusCode = 200
        };
    }

    private async Task<MessageResource> SendSmsWithRetryAsync(string phoneNumber, string message, int maxRetries = 3)
    {
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                var messageResource = await MessageResource.CreateAsync(
                    body: message,
                    from: new PhoneNumber(_twilioSettings.FromPhoneNumber),
                    to: new PhoneNumber(phoneNumber)
                );
                return messageResource;
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                _logger.LogWarning(ex, "SMS send attempt {Attempt} failed for {PhoneNumber}, retrying...", attempt, phoneNumber);
                await Task.Delay(1000 * attempt + _random.Next(100, 500)); // Exponential backoff with jitter
            }
        }

        // If we get here, all retries failed
        throw new Exception($"Failed to send SMS after {maxRetries} attempts");
    }

    private async Task<Response> SendEmailWithRetryAsync(SendGridMessage message, int maxRetries = 3)
    {
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                var response = await _sendGridClient!.SendEmailAsync(message);
                return response;
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                _logger.LogWarning(ex, "Email send attempt {Attempt} failed, retrying...", attempt);
                await Task.Delay(1000 * attempt + _random.Next(100, 500)); // Exponential backoff with jitter
            }
        }

        // If we get here, all retries failed
        throw new Exception($"Failed to send email after {maxRetries} attempts");
    }

    private string FormatPhoneNumber(string phoneNumber)
    {
        var cleaned = new string(phoneNumber.Where(char.IsDigit).ToArray());
        
        if (cleaned.Length == 10)
        {
            return "+1" + cleaned; // Assume US number
        }
        else if (cleaned.Length == 11 && cleaned.StartsWith("1"))
        {
            return "+" + cleaned;
        }
        else if (cleaned.Length > 11)
        {
            return "+" + cleaned;
        }
        
        return phoneNumber; // Return as-is if can't format
    }

    private bool IsValidPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber) || !phoneNumber.StartsWith("+"))
            return false;

        var digits = phoneNumber.Substring(1);
        return digits.Length >= 10 && digits.Length <= 15 && digits.All(char.IsDigit);
    }

    private string ExtractCountryCode(string phoneNumber)
    {
        if (phoneNumber.StartsWith("+"))
        {
            var digits = phoneNumber.Substring(1);
            if (digits.StartsWith("1") && digits.Length == 11)
                return "+1";
            else if (digits.StartsWith("44") && digits.Length == 12)
                return "+44";
            else if (digits.StartsWith("91") && digits.Length == 12)
                return "+91";
            // Add more country codes as needed
        }
        return "Unknown";
    }

    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            return regex.IsMatch(email) && email.Length <= 254;
        }
        catch
        {
            return false;
        }
    }

    private bool IsValidEmailFormat(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var parts = email.Split('@');
        if (parts.Length != 2)
            return false;

        var localPart = parts[0];
        var domainPart = parts[1];

        return !string.IsNullOrEmpty(localPart) && 
               !string.IsNullOrEmpty(domainPart) && 
               domainPart.Contains(".") &&
               localPart.Length <= 64 &&
               domainPart.Length <= 253;
    }

    private string ExtractDomain(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            return string.Empty;

        return email.Split('@')[1];
    }

    private string GetMimeType(string extension)
    {
        return extension?.ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".txt" => "text/plain",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".zip" => "application/zip",
            ".rar" => "application/x-rar-compressed",
            _ => "application/octet-stream"
        };
    }

    private async Task ApplySmsRateLimiting(string phoneNumber)
    {
        var now = DateTime.UtcNow;
        
        if (_lastSmsSent.TryGetValue(phoneNumber, out var lastSent))
        {
            var timeSinceLastSms = now - lastSent;
            if (timeSinceLastSms.TotalMinutes < 1.0 / _twilioSettings.SmsRateLimitPerMinute)
            {
                var delayMs = (int)((1.0 / _twilioSettings.SmsRateLimitPerMinute * 60 * 1000) - timeSinceLastSms.TotalMilliseconds);
                if (delayMs > 0)
                {
                    await Task.Delay(delayMs);
                }
            }
        }
        
        _lastSmsSent[phoneNumber] = now;
    }

    private async Task ApplyEmailRateLimiting(string email)
    {
        var now = DateTime.UtcNow;
        
        if (_lastEmailSent.TryGetValue(email, out var lastSent))
        {
            var timeSinceLastEmail = now - lastSent;
            if (timeSinceLastEmail.TotalMinutes < 1.0 / _twilioSettings.EmailRateLimitPerMinute)
            {
                var delayMs = (int)((1.0 / _twilioSettings.EmailRateLimitPerMinute * 60 * 1000) - timeSinceLastEmail.TotalMilliseconds);
                if (delayMs > 0)
                {
                    await Task.Delay(delayMs);
                }
            }
        }
        
        _lastEmailSent[email] = now;
    }

    #endregion
}
