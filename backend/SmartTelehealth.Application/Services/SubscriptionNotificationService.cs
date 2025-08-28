using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;

namespace SmartTelehealth.Application.Services;

public class SubscriptionNotificationService : ISubscriptionNotificationService
{
    private readonly INotificationService _notificationService;
    private readonly ICommunicationService _communicationService;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAuditService _auditService;
    private readonly ILogger<SubscriptionNotificationService> _logger;

    public SubscriptionNotificationService(
        INotificationService notificationService,
        ICommunicationService communicationService,
        ISubscriptionRepository subscriptionRepository,
        IUserRepository userRepository,
        IAuditService auditService,
        ILogger<SubscriptionNotificationService> logger)
    {
        _notificationService = notificationService;
        _communicationService = communicationService;
        _subscriptionRepository = subscriptionRepository;
        _userRepository = userRepository;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<JsonModel> SendTrialEndingNotificationAsync(string subscriptionId, int daysRemaining, TokenModel tokenModel)
    {
        try
        {
            var subscription = await GetSubscriptionWithUserAsync(subscriptionId);
            if (subscription == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Subscription not found",
                    StatusCode = 404
                };
            }

            var user = await _userRepository.GetByIdAsync(subscription.UserId);
            if (user == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "User not found",
                    StatusCode = 404
                };
            }

            // Create in-app notification
            var notification = new CreateNotificationDto
            {
                UserId = subscription.UserId,
                Title = "Trial Ending Soon",
                Message = $"Your trial for {subscription.SubscriptionPlan?.Name ?? "Premium Plan"} ends in {daysRemaining} days. Upgrade now to continue enjoying our services.",
                Type = "SubscriptionTrial",
                Priority = "Medium",
                IsRead = false
            };

            await _notificationService.CreateNotificationAsync(notification, tokenModel);

            // Send email notification
            var emailSubject = "Trial Ending Soon - Action Required";
            var emailBody = GenerateTrialEndingEmailBody(user.FirstName, subscription, daysRemaining);
            await _communicationService.SendEmailAsync(user.Email, emailSubject, emailBody, true, tokenModel);

            // Send SMS notification if phone number exists
            if (!string.IsNullOrEmpty(user.PhoneNumber))
            {
                var smsMessage = $"Hi {user.FirstName}, your trial ends in {daysRemaining} days. Upgrade now to continue using Smart Telehealth services.";
                await _communicationService.SendSmsAsync(user.PhoneNumber, smsMessage, tokenModel);
            }

            // Log the notification
            await _auditService.LogPaymentEventAsync(
                subscription.UserId,
                "TrialEndingNotification",
                subscription.Id.ToString(),
                "Success",
                $"Trial ending notification sent via email and SMS. Days remaining: {daysRemaining}",
                tokenModel
            );

            return new JsonModel
            {
                data = new { NotificationSent = true, DaysRemaining = daysRemaining, EmailSent = true, SmsSent = !string.IsNullOrEmpty(user.PhoneNumber) },
                Message = "Trial ending notification sent successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending trial ending notification for subscription {SubscriptionId}", subscriptionId);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to send trial ending notification",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> SendPaymentFailedNotificationAsync(string subscriptionId, string errorMessage, TokenModel tokenModel)
    {
        try
        {
            var subscription = await GetSubscriptionWithUserAsync(subscriptionId);
            if (subscription == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Subscription not found",
                    StatusCode = 404
                };
            }

            var user = await _userRepository.GetByIdAsync(subscription.UserId);
            if (user == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "User not found",
                    StatusCode = 404
                };
            }

            // Create in-app notification
            var notification = new CreateNotificationDto
            {
                UserId = subscription.UserId,
                Title = "Payment Failed",
                Message = $"Your payment for {subscription.SubscriptionPlan?.Name ?? "Premium Plan"} has failed. Please update your payment method to avoid service interruption.",
                Type = "PaymentFailed",
                Priority = "High",
                IsRead = false
            };

            await _notificationService.CreateNotificationAsync(notification, tokenModel);

            // Send email notification
            var emailSubject = "Payment Failed - Action Required";
            var emailBody = GeneratePaymentFailedEmailBody(user.FirstName, subscription, errorMessage);
            await _communicationService.SendEmailAsync(user.Email, emailSubject, emailBody, true, tokenModel);

            // Send SMS notification if phone number exists
            if (!string.IsNullOrEmpty(user.PhoneNumber))
            {
                var smsMessage = $"Hi {user.FirstName}, your payment failed. Please update your payment method to avoid service interruption. Error: {errorMessage}";
                await _communicationService.SendSmsAsync(user.PhoneNumber, smsMessage, tokenModel);
            }

            // Log the notification
            await _auditService.LogPaymentEventAsync(
                subscription.UserId,
                "PaymentFailedNotification",
                subscription.Id.ToString(),
                "Success",
                $"Payment failed notification sent via email and SMS. Error: {errorMessage}",
                tokenModel
            );

            return new JsonModel
            {
                data = new { NotificationSent = true, ErrorMessage = errorMessage, EmailSent = true, SmsSent = !string.IsNullOrEmpty(user.PhoneNumber) },
                Message = "Payment failed notification sent successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending payment failed notification for subscription {SubscriptionId}", subscriptionId);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to send payment failed notification",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> SendSubscriptionExpiredNotificationAsync(string subscriptionId, TokenModel tokenModel)
    {
        try
        {
            var subscription = await GetSubscriptionWithUserAsync(subscriptionId);
            if (subscription == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Subscription not found",
                    StatusCode = 404
                };
            }

            var user = await _userRepository.GetByIdAsync(subscription.UserId);
            if (user == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "User not found",
                    StatusCode = 404
                };
            }

            // Create in-app notification
            var notification = new CreateNotificationDto
            {
                UserId = subscription.UserId,
                Title = "Subscription Expired",
                Message = $"Your subscription to {subscription.SubscriptionPlan?.Name ?? "Premium Plan"} has expired. Renew now to restore access to our services.",
                Type = "SubscriptionExpired",
                Priority = "High",
                IsRead = false
            };

            await _notificationService.CreateNotificationAsync(notification, tokenModel);

            // Send email notification
            var emailSubject = "Subscription Expired - Renew Now";
            var emailBody = GenerateSubscriptionExpiredEmailBody(user.FirstName, subscription);
            await _communicationService.SendEmailAsync(user.Email, emailSubject, emailBody, true, tokenModel);

            // Send SMS notification if phone number exists
            if (!string.IsNullOrEmpty(user.PhoneNumber))
            {
                var smsMessage = $"Hi {user.FirstName}, your subscription has expired. Renew now to restore access to Smart Telehealth services.";
                await _communicationService.SendSmsAsync(user.PhoneNumber, smsMessage, tokenModel);
            }

            // Log the notification
            await _auditService.LogPaymentEventAsync(
                subscription.UserId,
                "SubscriptionExpiredNotification",
                subscription.Id.ToString(),
                "Success",
                "Subscription expired notification sent via email and SMS",
                tokenModel
            );

            return new JsonModel
            {
                data = new { NotificationSent = true, EmailSent = true, SmsSent = !string.IsNullOrEmpty(user.PhoneNumber) },
                Message = "Subscription expired notification sent successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending subscription expired notification for subscription {SubscriptionId}", subscriptionId);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to send subscription expired notification",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> SendSubscriptionRenewedNotificationAsync(string subscriptionId, DateTime newEndDate, TokenModel tokenModel)
    {
        try
        {
            var subscription = await GetSubscriptionWithUserAsync(subscriptionId);
            if (subscription == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Subscription not found",
                    StatusCode = 404
                };
            }

            var user = await _userRepository.GetByIdAsync(subscription.UserId);
            if (user == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "User not found",
                    StatusCode = 404
                };
            }

            // Create in-app notification
            var notification = new CreateNotificationDto
            {
                UserId = subscription.UserId,
                Title = "Subscription Renewed",
                Message = $"Your subscription to {subscription.SubscriptionPlan?.Name ?? "Premium Plan"} has been successfully renewed until {newEndDate:yyyy-MM-dd}.",
                Type = "SubscriptionRenewed",
                Priority = "Low",
                IsRead = false
            };

            await _notificationService.CreateNotificationAsync(notification, tokenModel);

            // Send email notification
            var emailSubject = "Subscription Renewed Successfully";
            var emailBody = GenerateSubscriptionRenewedEmailBody(user.FirstName, subscription, newEndDate);
            await _communicationService.SendEmailAsync(user.Email, emailSubject, emailBody, true, tokenModel);

            // Send SMS notification if phone number exists
            if (!string.IsNullOrEmpty(user.PhoneNumber))
            {
                var smsMessage = $"Hi {user.FirstName}, your subscription has been renewed successfully until {newEndDate:yyyy-MM-dd}. Thank you!";
                await _communicationService.SendSmsAsync(user.PhoneNumber, smsMessage, tokenModel);
            }

            // Log the notification
            await _auditService.LogPaymentEventAsync(
                subscription.UserId,
                "SubscriptionRenewedNotification",
                subscription.Id.ToString(),
                "Success",
                $"Subscription renewed notification sent via email and SMS. New end date: {newEndDate:yyyy-MM-dd}",
                tokenModel
            );

            return new JsonModel
            {
                data = new { NotificationSent = true, NewEndDate = newEndDate, EmailSent = true, SmsSent = !string.IsNullOrEmpty(user.PhoneNumber) },
                Message = "Subscription renewed notification sent successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending subscription renewed notification for subscription {SubscriptionId}", subscriptionId);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to send subscription renewed notification",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> SendPlanChangeNotificationAsync(string subscriptionId, string oldPlanName, string newPlanName, TokenModel tokenModel)
    {
        try
        {
            var subscription = await GetSubscriptionWithUserAsync(subscriptionId);
            if (subscription == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Subscription not found",
                    StatusCode = 404
                };
            }

            var user = await _userRepository.GetByIdAsync(subscription.UserId);
            if (user == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "User not found",
                    StatusCode = 404
                };
            }

            // Create in-app notification
            var notification = new CreateNotificationDto
            {
                UserId = subscription.UserId,
                Title = "Plan Changed",
                Message = $"Your subscription plan has been changed from {oldPlanName} to {newPlanName}. The changes will take effect immediately.",
                Type = "PlanChanged",
                Priority = "Medium",
                IsRead = false
            };

            await _notificationService.CreateNotificationAsync(notification, tokenModel);

            // Send email notification
            var emailSubject = "Subscription Plan Changed";
            var emailBody = GeneratePlanChangeEmailBody(user.FirstName, oldPlanName, newPlanName, subscription);
            await _communicationService.SendEmailAsync(user.Email, emailSubject, emailBody, true, tokenModel);

            // Send SMS notification if phone number exists
            if (!string.IsNullOrEmpty(user.PhoneNumber))
            {
                var smsMessage = $"Hi {user.FirstName}, your plan has been changed from {oldPlanName} to {newPlanName}. Changes take effect immediately.";
                await _communicationService.SendSmsAsync(user.PhoneNumber, smsMessage, tokenModel);
            }

            // Log the notification
            await _auditService.LogPaymentEventAsync(
                subscription.UserId,
                "PlanChangeNotification",
                subscription.Id.ToString(),
                "Success",
                $"Plan change notification sent via email and SMS. Old plan: {oldPlanName}, New plan: {newPlanName}",
                tokenModel
            );

            return new JsonModel
            {
                data = new { NotificationSent = true, OldPlan = oldPlanName, NewPlan = newPlanName, EmailSent = true, SmsSent = !string.IsNullOrEmpty(user.PhoneNumber) },
                Message = "Plan change notification sent successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending plan change notification for subscription {SubscriptionId}", subscriptionId);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to send plan change notification",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> SendBulkNotificationAsync(List<string> subscriptionIds, string title, string message, string type, TokenModel tokenModel)
    {
        try
        {
            var notificationsSent = 0;
            var failedNotifications = 0;
            var emailsSent = 0;
            var smsSent = 0;

            foreach (var subscriptionId in subscriptionIds)
            {
                try
                {
                    var subscription = await GetSubscriptionWithUserAsync(subscriptionId);
                    if (subscription != null)
                    {
                        var user = await _userRepository.GetByIdAsync(subscription.UserId);
                        if (user != null)
                        {
                            // Create in-app notification
                            var notification = new CreateNotificationDto
                            {
                                UserId = subscription.UserId,
                                Title = title,
                                Message = message,
                                Type = type,
                                Priority = "Medium",
                                IsRead = false
                            };

                            var notificationResult = await _notificationService.CreateNotificationAsync(notification, tokenModel);
                            if (notificationResult.StatusCode == 200)
                            {
                                notificationsSent++;

                                // Send email
                                try
                                {
                                    await _communicationService.SendEmailAsync(user.Email, title, message, true, tokenModel);
                                    emailsSent++;
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogWarning(ex, "Failed to send email to {Email} for subscription {SubscriptionId}", user.Email, subscriptionId);
                                }

                                // Send SMS if phone number exists
                                if (!string.IsNullOrEmpty(user.PhoneNumber))
                                {
                                    try
                                    {
                                        await _communicationService.SendSmsAsync(user.PhoneNumber, message, tokenModel);
                                        smsSent++;
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogWarning(ex, "Failed to send SMS to {PhoneNumber} for subscription {SubscriptionId}", user.PhoneNumber, subscriptionId);
                                    }
                                }
                            }
                            else
                            {
                                failedNotifications++;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    failedNotifications++;
                    _logger.LogError(ex, "Error sending bulk notification for subscription {SubscriptionId}", subscriptionId);
                }
            }

            var result = new
            {
                TotalSubscriptions = subscriptionIds.Count,
                NotificationsSent = notificationsSent,
                FailedNotifications = failedNotifications,
                EmailsSent = emailsSent,
                SmsSent = smsSent,
                SuccessRate = subscriptionIds.Count > 0 ? (double)notificationsSent / subscriptionIds.Count * 100 : 0
            };

            return new JsonModel
            {
                data = result,
                Message = $"Bulk notification completed. Sent: {notificationsSent}, Failed: {failedNotifications}, Emails: {emailsSent}, SMS: {smsSent}",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending bulk notifications to {Count} subscriptions", subscriptionIds.Count);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to send bulk notifications",
                StatusCode = 500
            };
        }
    }

    private async Task<Subscription?> GetSubscriptionWithUserAsync(string subscriptionId)
    {
        try
        {
            if (Guid.TryParse(subscriptionId, out var id))
            {
                return await _subscriptionRepository.GetByIdAsync(id);
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving subscription {SubscriptionId}", subscriptionId);
            return null;
        }
    }

    #region Email Template Generators

    private string GenerateTrialEndingEmailBody(string userName, Subscription subscription, int daysRemaining)
    {
        return $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <h2 style='color: #2c3e50;'>Trial Ending Soon - Action Required</h2>
                <p>Hello {userName},</p>
                <p>Your trial subscription to <strong>{subscription.SubscriptionPlan?.Name ?? "Premium Plan"}</strong> will end in <strong>{daysRemaining} days</strong>.</p>
                
                <div style='background-color: #f8f9fa; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                    <h3 style='color: #e74c3c; margin-top: 0;'>What happens when your trial ends?</h3>
                    <ul>
                        <li>Access to premium features will be restricted</li>
                        <li>Your account will be limited to basic functionality</li>
                        <li>You may lose access to saved data and preferences</li>
                    </ul>
                </div>

                <div style='background-color: #e8f5e8; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                    <h3 style='color: #27ae60; margin-top: 0;'>Upgrade Now to Continue</h3>
                    <p>Don't lose access to your premium features! Upgrade now to continue enjoying:</p>
                    <ul>
                        <li>Unlimited consultations</li>
                        <li>Priority support</li>
                        <li>Advanced health tracking</li>
                        <li>And much more!</li>
                    </ul>
                </div>

                <p style='text-align: center; margin: 30px 0;'>
                    <a href='https://smarttelehealth.com/upgrade' style='background-color: #3498db; color: white; padding: 15px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>
                        Upgrade Now
                    </a>
                </p>

                <p>If you have any questions or need assistance, please don't hesitate to contact our support team.</p>
                
                <p>Best regards,<br><strong>Smart Telehealth Team</strong></p>
                
                <hr style='margin: 30px 0; border: none; border-top: 1px solid #ecf0f1;'>
                <p style='font-size: 12px; color: #7f8c8d;'>
                    This is an automated notification. Please do not reply to this email.
                </p>
            </div>";
    }

    private string GeneratePaymentFailedEmailBody(string userName, Subscription subscription, string errorMessage)
    {
        return $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <h2 style='color: #e74c3c;'>Payment Failed - Action Required</h2>
                <p>Hello {userName},</p>
                <p>We were unable to process your payment for your <strong>{subscription.SubscriptionPlan?.Name ?? "Premium Plan"}</strong> subscription.</p>
                
                <div style='background-color: #fdf2f2; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #e74c3c;'>
                    <h3 style='color: #e74c3c; margin-top: 0;'>Payment Details</h3>
                    <p><strong>Error:</strong> {errorMessage}</p>
                    <p><strong>Amount:</strong> ${subscription.CurrentPrice}</p>
                    <p><strong>Plan:</strong> {subscription.SubscriptionPlan?.Name ?? "Premium Plan"}</p>
                </div>

                <div style='background-color: #fff3cd; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #ffc107;'>
                    <h3 style='color: #856404; margin-top: 0;'>What you need to do:</h3>
                    <ol>
                        <li>Check your payment method for any issues</li>
                        <li>Ensure sufficient funds are available</li>
                        <li>Update your payment information if needed</li>
                        <li>Contact your bank if the issue persists</li>
                    </ol>
                </div>

                <p style='text-align: center; margin: 30px 0;'>
                    <a href='https://smarttelehealth.com/payment' style='background-color: #e74c3c; color: white; padding: 15px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>
                        Update Payment Method
                    </a>
                </p>

                <p><strong>Important:</strong> If payment is not resolved within 7 days, your subscription may be suspended.</p>
                
                <p>Need help? Our support team is here to assist you.</p>
                
                <p>Best regards,<br><strong>Smart Telehealth Team</strong></p>
                
                <hr style='margin: 30px 0; border: none; border-top: 1px solid #ecf0f1;'>
                <p style='font-size: 12px; color: #7f8c8d;'>
                    This is an automated notification. Please do not reply to this email.
                </p>
            </div>";
    }

    private string GenerateSubscriptionExpiredEmailBody(string userName, Subscription subscription)
    {
        return $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <h2 style='color: #e74c3c;'>Subscription Expired</h2>
                <p>Hello {userName},</p>
                <p>Your subscription to <strong>{subscription.SubscriptionPlan?.Name ?? "Premium Plan"}</strong> has expired.</p>
                
                <div style='background-color: #fdf2f2; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #e74c3c;'>
                    <h3 style='color: #e74c3c; margin-top: 0;'>What this means:</h3>
                    <ul>
                        <li>Access to premium features is now restricted</li>
                        <li>Your account is limited to basic functionality</li>
                        <li>You may lose access to saved data and preferences</li>
                    </ul>
                </div>

                <div style='background-color: #e8f5e8; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #27ae60;'>
                    <h3 style='color: #27ae60; margin-top: 0;'>Renew Now to Restore Access</h3>
                    <p>Don't lose your premium features! Renew now to continue enjoying:</p>
                    <ul>
                        <li>Unlimited consultations</li>
                        <li>Priority support</li>
                        <li>Advanced health tracking</li>
                        <li>And much more!</li>
                    </ul>
                </div>

                <p style='text-align: center; margin: 30px 0;'>
                    <a href='https://smarttelehealth.com/renew' style='background-color: #27ae60; color: white; padding: 15px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>
                        Renew Subscription
                    </a>
                </p>

                <p>If you have any questions or need assistance, please don't hesitate to contact our support team.</p>
                
                <p>Best regards,<br><strong>Smart Telehealth Team</strong></p>
                
                <hr style='margin: 30px 0; border: none; border-top: 1px solid #ecf0f1;'>
                <p style='font-size: 12px; color: #7f8c8d;'>
                    This is an automated notification. Please do not reply to this email.
                </p>
            </div>";
    }

    private string GenerateSubscriptionRenewedEmailBody(string userName, Subscription subscription, DateTime newEndDate)
    {
        return $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <h2 style='color: #27ae60;'>Subscription Renewed Successfully!</h2>
                <p>Hello {userName},</p>
                <p>Great news! Your subscription to <strong>{subscription.SubscriptionPlan?.Name ?? "Premium Plan"}</strong> has been successfully renewed.</p>
                
                <div style='background-color: #e8f5e8; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #27ae60;'>
                    <h3 style='color: #27ae60; margin-top: 0;'>Renewal Details</h3>
                    <ul>
                        <li><strong>Plan:</strong> {subscription.SubscriptionPlan?.Name ?? "Premium Plan"}</li>
                        <li><strong>New End Date:</strong> {newEndDate:MMMM dd, yyyy}</li>
                        <li><strong>Amount:</strong> ${subscription.CurrentPrice}</li>
                        <li><strong>Status:</strong> Active</li>
                    </ul>
                </div>

                <div style='background-color: #f0f8ff; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #3498db;'>
                    <h3 style='color: #2980b9; margin-top: 0;'>What's Included:</h3>
                    <ul>
                        <li>Unlimited consultations with healthcare providers</li>
                        <li>Priority customer support</li>
                        <li>Advanced health tracking and analytics</li>
                        <li>Access to premium health resources</li>
                        <li>And much more!</li>
                    </ul>
                </div>

                <p style='text-align: center; margin: 30px 0;'>
                    <a href='https://smarttelehealth.com/dashboard' style='background-color: #3498db; color: white; padding: 15px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>
                        Access Your Dashboard
                    </a>
                </p>

                <p>Thank you for choosing Smart Telehealth! We're committed to providing you with the best healthcare experience.</p>
                
                <p>Best regards,<br><strong>Smart Telehealth Team</strong></p>
                
                <hr style='margin: 30px 0; border: none; border-top: 1px solid #ecf0f1;'>
                <p style='font-size: 12px; color: #7f8c8d;'>
                    This is an automated notification. Please do not reply to this email.
                </p>
            </div>";
    }

    private string GeneratePlanChangeEmailBody(string userName, string oldPlanName, string newPlanName, Subscription subscription)
    {
        return $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <h2 style='color: #3498db;'>Subscription Plan Changed</h2>
                <p>Hello {userName},</p>
                <p>Your subscription plan has been successfully changed from <strong>{oldPlanName}</strong> to <strong>{newPlanName}</strong>.</p>
                
                <div style='background-color: #f0f8ff; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #3498db;'>
                    <h3 style='color: #2980b9; margin-top: 0;'>Plan Change Details</h3>
                    <ul>
                        <li><strong>Previous Plan:</strong> {oldPlanName}</li>
                        <li><strong>New Plan:</strong> {newPlanName}</li>
                        <li><strong>Effective Date:</strong> {DateTime.UtcNow:MMMM dd, yyyy}</li>
                        <li><strong>New Price:</strong> ${subscription.CurrentPrice}</li>
                    </ul>
                </div>

                <div style='background-color: #e8f5e8; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #27ae60;'>
                    <h3 style='color: #27ae60; margin-top: 0;'>What happens next?</h3>
                    <ul>
                        <li>Changes take effect immediately</li>
                        <li>You'll be billed at the new rate on your next billing cycle</li>
                        <li>Any prorated amounts will be applied to your next invoice</li>
                        <li>You'll have access to all features included in your new plan</li>
                    </ul>
                </div>

                <p style='text-align: center; margin: 30px 0;'>
                    <a href='https://smarttelehealth.com/subscription' style='background-color: #3498db; color: white; padding: 15px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>
                        View Subscription Details
                    </a>
                </p>

                <p>If you have any questions about your plan change or need assistance, please don't hesitate to contact our support team.</p>
                
                <p>Best regards,<br><strong>Smart Telehealth Team</strong></p>
                
                <hr style='margin: 30px 0; border: none; border-top: 1px solid #ecf0f1;'>
                <p style='font-size: 12px; color: #7f8c8d;'>
                    This is an automated notification. Please do not reply to this email.
                </p>
            </div>";
    }

    #endregion

    #region Additional Subscription Lifecycle Notifications

    public async Task<JsonModel> SendSubscriptionCreatedNotificationAsync(string subscriptionId, TokenModel tokenModel)
    {
        try
        {
            var subscription = await GetSubscriptionWithUserAsync(subscriptionId);
            if (subscription == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Subscription not found",
                    StatusCode = 404
                };
            }

            var user = await _userRepository.GetByIdAsync(subscription.UserId);
            if (user == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "User not found",
                    StatusCode = 404
                };
            }

            // Create in-app notification
            var notification = new CreateNotificationDto
            {
                UserId = subscription.UserId,
                Title = "Subscription Created",
                Message = $"Your subscription to {subscription.SubscriptionPlan?.Name ?? "Premium Plan"} has been successfully created and activated.",
                Type = "SubscriptionCreated",
                Priority = "Low",
                IsRead = false
            };

            await _notificationService.CreateNotificationAsync(notification, tokenModel);

            // Send email notification
            var emailSubject = "Welcome to Smart Telehealth!";
            var emailBody = GenerateSubscriptionCreatedEmailBody(user.FirstName, subscription);
            await _communicationService.SendEmailAsync(user.Email, emailSubject, emailBody, true, tokenModel);

            // Send SMS notification if phone number exists
            if (!string.IsNullOrEmpty(user.PhoneNumber))
            {
                var smsMessage = $"Hi {user.FirstName}, welcome to Smart Telehealth! Your subscription is now active. Enjoy our premium healthcare services.";
                await _communicationService.SendSmsAsync(user.PhoneNumber, smsMessage, tokenModel);
            }

            // Log the notification
            await _auditService.LogPaymentEventAsync(
                subscription.UserId,
                "SubscriptionCreatedNotification",
                subscription.Id.ToString(),
                "Success",
                "Subscription created notification sent via email and SMS",
                tokenModel
            );

            return new JsonModel
            {
                data = new { NotificationSent = true, EmailSent = true, SmsSent = !string.IsNullOrEmpty(user.PhoneNumber) },
                Message = "Subscription created notification sent successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending subscription created notification for subscription {SubscriptionId}", subscriptionId);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to send subscription created notification",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> SendSubscriptionUpgradedNotificationAsync(string subscriptionId, string oldPlanName, string newPlanName, TokenModel tokenModel)
    {
        try
        {
            var subscription = await GetSubscriptionWithUserAsync(subscriptionId);
            if (subscription == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Subscription not found",
                    StatusCode = 404
                };
            }

            var user = await _userRepository.GetByIdAsync(subscription.UserId);
            if (user == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "User not found",
                    StatusCode = 404
                };
            }

            // Create in-app notification
            var notification = new CreateNotificationDto
            {
                UserId = subscription.UserId,
                Title = "Subscription Upgraded",
                Message = $"Your subscription has been upgraded from {oldPlanName} to {newPlanName}. Enjoy the new features!",
                Type = "SubscriptionUpgraded",
                Priority = "Medium",
                IsRead = false
            };

            await _notificationService.CreateNotificationAsync(notification, tokenModel);

            // Send email notification
            var emailSubject = "Subscription Upgraded Successfully";
            var emailBody = GenerateSubscriptionUpgradedEmailBody(user.FirstName, oldPlanName, newPlanName, subscription);
            await _communicationService.SendEmailAsync(user.Email, emailSubject, emailBody, true, tokenModel);

            // Send SMS notification if phone number exists
            if (!string.IsNullOrEmpty(user.PhoneNumber))
            {
                var smsMessage = $"Hi {user.FirstName}, your subscription has been upgraded from {oldPlanName} to {newPlanName}. Enjoy the new features!";
                await _communicationService.SendSmsAsync(user.PhoneNumber, smsMessage, tokenModel);
            }

            // Log the notification
            await _auditService.LogPaymentEventAsync(
                subscription.UserId,
                "SubscriptionUpgradedNotification",
                subscription.Id.ToString(),
                "Success",
                $"Subscription upgraded notification sent via email and SMS. Old plan: {oldPlanName}, New plan: {newPlanName}",
                tokenModel
            );

            return new JsonModel
            {
                data = new { NotificationSent = true, OldPlan = oldPlanName, NewPlan = newPlanName, EmailSent = true, SmsSent = !string.IsNullOrEmpty(user.PhoneNumber) },
                Message = "Subscription upgraded notification sent successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending subscription upgraded notification for subscription {SubscriptionId}", subscriptionId);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to send subscription upgraded notification",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> SendSubscriptionDowngradedNotificationAsync(string subscriptionId, string oldPlanName, string newPlanName, TokenModel tokenModel)
    {
        try
        {
            var subscription = await GetSubscriptionWithUserAsync(subscriptionId);
            if (subscription == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Subscription not found",
                    StatusCode = 404
                };
            }

            var user = await _userRepository.GetByIdAsync(subscription.UserId);
            if (user == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "User not found",
                    StatusCode = 404
                };
            }

            // Create in-app notification
            var notification = new CreateNotificationDto
            {
                UserId = subscription.UserId,
                Title = "Subscription Plan Changed",
                Message = $"Your subscription has been changed from {oldPlanName} to {newPlanName}. Some features may no longer be available.",
                Type = "SubscriptionDowngraded",
                Priority = "Medium",
                IsRead = false
            };

            await _notificationService.CreateNotificationAsync(notification, tokenModel);

            // Send email notification
            var emailSubject = "Subscription Plan Changed";
            var emailBody = GenerateSubscriptionDowngradedEmailBody(user.FirstName, oldPlanName, newPlanName, subscription);
            await _communicationService.SendEmailAsync(user.Email, emailSubject, emailBody, true, tokenModel);

            // Send SMS notification if phone number exists
            if (!string.IsNullOrEmpty(user.PhoneNumber))
            {
                var smsMessage = $"Hi {user.FirstName}, your subscription has been changed from {oldPlanName} to {newPlanName}. Some features may no longer be available.";
                await _communicationService.SendSmsAsync(user.PhoneNumber, smsMessage, tokenModel);
            }

            // Log the notification
            await _auditService.LogPaymentEventAsync(
                subscription.UserId,
                "SubscriptionDowngradedNotification",
                subscription.Id.ToString(),
                "Success",
                $"Subscription downgraded notification sent via email and SMS. Old plan: {oldPlanName}, New plan: {newPlanName}",
                tokenModel
            );

            return new JsonModel
            {
                data = new { NotificationSent = true, OldPlan = oldPlanName, NewPlan = newPlanName, EmailSent = true, SmsSent = !string.IsNullOrEmpty(user.PhoneNumber) },
                Message = "Subscription downgraded notification sent successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending subscription downgraded notification for subscription {SubscriptionId}", subscriptionId);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to send subscription downgraded notification",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> SendBillingReminderNotificationAsync(string subscriptionId, DateTime dueDate, decimal amount, TokenModel tokenModel)
    {
        try
        {
            var subscription = await GetSubscriptionWithUserAsync(subscriptionId);
            if (subscription == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Subscription not found",
                    StatusCode = 404
                };
            }

            var user = await _userRepository.GetByIdAsync(subscription.UserId);
            if (user == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "User not found",
                    StatusCode = 404
                };
            }

            // Create in-app notification
            var notification = new CreateNotificationDto
            {
                UserId = subscription.UserId,
                Title = "Billing Reminder",
                Message = $"Your subscription payment of ${amount} is due on {dueDate:MM/dd/yyyy}. Please ensure your payment method is up to date.",
                Type = "BillingReminder",
                Priority = "Medium",
                IsRead = false
            };

            await _notificationService.CreateNotificationAsync(notification, tokenModel);

            // Send email notification
            var emailSubject = "Billing Reminder - Action Required";
            var emailBody = GenerateBillingReminderEmailBody(user.FirstName, subscription, dueDate, amount);
            await _communicationService.SendEmailAsync(user.Email, emailSubject, emailBody, true, tokenModel);

            // Send SMS notification if phone number exists
            if (!string.IsNullOrEmpty(user.PhoneNumber))
            {
                var smsMessage = $"Hi {user.FirstName}, your subscription payment of ${amount} is due on {dueDate:MM/dd/yyyy}. Please ensure your payment method is up to date.";
                await _communicationService.SendSmsAsync(user.PhoneNumber, smsMessage, tokenModel);
            }

            // Log the notification
            await _auditService.LogPaymentEventAsync(
                subscription.UserId,
                "BillingReminderNotification",
                subscription.Id.ToString(),
                "Success",
                $"Billing reminder notification sent via email and SMS. Due date: {dueDate:MM/dd/yyyy}, Amount: ${amount}",
                tokenModel
            );

            return new JsonModel
            {
                data = new { NotificationSent = true, DueDate = dueDate, Amount = amount, EmailSent = true, SmsSent = !string.IsNullOrEmpty(user.PhoneNumber) },
                Message = "Billing reminder notification sent successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending billing reminder notification for subscription {SubscriptionId}", subscriptionId);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to send billing reminder notification",
                StatusCode = 500
            };
        }
    }

    #endregion

    #region Additional Email Template Generators

    private string GenerateSubscriptionCreatedEmailBody(string userName, Subscription subscription)
    {
        return $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <h2 style='color: #27ae60;'>Welcome to Smart Telehealth!</h2>
                <p>Hello {userName},</p>
                <p>Congratulations! Your subscription to <strong>{subscription.SubscriptionPlan?.Name ?? "Premium Plan"}</strong> has been successfully created and activated.</p>
                
                <div style='background-color: #e8f5e8; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #27ae60;'>
                    <h3 style='color: #27ae60; margin-top: 0;'>Your Subscription Details</h3>
                    <ul>
                        <li><strong>Plan:</strong> {subscription.SubscriptionPlan?.Name ?? "Premium Plan"}</li>
                        <li><strong>Status:</strong> Active</li>
                        <li><strong>Start Date:</strong> {subscription.StartDate:MMMM dd, yyyy}</li>
                        <li><strong>Next Billing:</strong> {subscription.NextBillingDate:MMMM dd, yyyy}</li>
                        <li><strong>Amount:</strong> ${subscription.CurrentPrice}</li>
                    </ul>
                </div>

                <div style='background-color: #f0f8ff; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #3498db;'>
                    <h3 style='color: #2980b9; margin-top: 0;'>What's Included:</h3>
                    <ul>
                        <li>Unlimited consultations with healthcare providers</li>
                        <li>Priority customer support</li>
                        <li>Advanced health tracking and analytics</li>
                        <li>Access to premium health resources</li>
                        <li>And much more!</li>
                    </ul>
                </div>

                <p style='text-align: center; margin: 30px 0;'>
                    <a href='https://smarttelehealth.com/dashboard' style='background-color: #3498db; color: white; padding: 15px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>
                        Access Your Dashboard
                    </a>
                </p>

                <p>We're excited to have you on board! If you have any questions or need assistance, our support team is here to help.</p>
                
                <p>Best regards,<br><strong>Smart Telehealth Team</strong></p>
                
                <hr style='margin: 30px 0; border: none; border-top: 1px solid #ecf0f1;'>
                <p style='font-size: 12px; color: #7f8c8d;'>
                    This is an automated notification. Please do not reply to this email.
                </p>
            </div>";
    }

    private string GenerateSubscriptionUpgradedEmailBody(string userName, string oldPlanName, string newPlanName, Subscription subscription)
    {
        return $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <h2 style='color: #27ae60;'>Subscription Upgraded Successfully!</h2>
                <p>Hello {userName},</p>
                <p>Great news! Your subscription has been successfully upgraded from <strong>{oldPlanName}</strong> to <strong>{newPlanName}</strong>.</p>
                
                <div style='background-color: #e8f5e8; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #27ae60;'>
                    <h3 style='color: #27ae60; margin-top: 0;'>Upgrade Details</h3>
                    <ul>
                        <li><strong>Previous Plan:</strong> {oldPlanName}</li>
                        <li><strong>New Plan:</strong> {newPlanName}</li>
                        <li><strong>Effective Date:</strong> {DateTime.UtcNow:MMMM dd, yyyy}</li>
                        <li><strong>New Price:</strong> ${subscription.CurrentPrice}</li>
                    </ul>
                </div>

                <div style='background-color: #f0f8ff; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #3498db;'>
                    <h3 style='color: #2980b9; margin-top: 0;'>New Features Available:</h3>
                    <ul>
                        <li>Enhanced consultation options</li>
                        <li>Priority scheduling</li>
                        <li>Advanced health analytics</li>
                        <li>Premium support channels</li>
                        <li>And much more!</li>
                    </ul>
                </div>

                <p style='text-align: center; margin: 30px 0;'>
                    <a href='https://smarttelehealth.com/features' style='background-color: #27ae60; color: white; padding: 15px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>
                        Explore New Features
                    </a>
                </p>

                <p>Thank you for upgrading! We're committed to providing you with the best healthcare experience possible.</p>
                
                <p>Best regards,<br><strong>Smart Telehealth Team</strong></p>
                
                <hr style='margin: 30px 0; border: none; border-top: 1px solid #ecf0f1;'>
                <p style='font-size: 12px; color: #7f8c8d;'>
                    This is an automated notification. Please do not reply to this email.
                </p>
            </div>";
    }

    private string GenerateSubscriptionDowngradedEmailBody(string userName, string oldPlanName, string newPlanName, Subscription subscription)
    {
        return $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <h2 style='color: #f39c12;'>Subscription Plan Changed</h2>
                <p>Hello {userName},</p>
                <p>Your subscription plan has been changed from <strong>{oldPlanName}</strong> to <strong>{newPlanName}</strong>.</p>
                
                <div style='background-color: #fff3cd; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #f39c12;'>
                    <h3 style='color: #856404; margin-top: 0;'>Plan Change Details</h3>
                    <ul>
                        <li><strong>Previous Plan:</strong> {oldPlanName}</li>
                        <li><strong>New Plan:</strong> {newPlanName}</li>
                        <li><strong>Effective Date:</strong> {DateTime.UtcNow:MMMM dd, yyyy}</li>
                        <li><strong>New Price:</strong> ${subscription.CurrentPrice}</li>
                    </ul>
                </div>

                <div style='background-color: #f8f9fa; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #6c757d;'>
                    <h3 style='color: #495057; margin-top: 0;'>Important Notes:</h3>
                    <ul>
                        <li>Some premium features may no longer be available</li>
                        <li>Your current data and settings are preserved</li>
                        <li>You can upgrade again at any time</li>
                        <li>Basic functionality remains unchanged</li>
                    </ul>
                </div>

                <p style='text-align: center; margin: 30px 0;'>
                    <a href='https://smarttelehealth.com/upgrade' style='background-color: #f39c12; color: white; padding: 15px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>
                        Upgrade Again
                    </a>
                </p>

                <p>If you have any questions about your plan change or need assistance, please don't hesitate to contact our support team.</p>
                
                <p>Best regards,<br><strong>Smart Telehealth Team</strong></p>
                
                <hr style='margin: 30px 0; border: none; border-top: 1px solid #ecf0f1;'>
                <p style='font-size: 12px; color: #7f8c8d;'>
                    This is an automated notification. Please do not reply to this email.
                </p>
            </div>";
    }

    private string GenerateBillingReminderEmailBody(string userName, Subscription subscription, DateTime dueDate, decimal amount)
    {
        return $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <h2 style='color: #f39c12;'>Billing Reminder - Action Required</h2>
                <p>Hello {userName},</p>
                <p>This is a friendly reminder that your subscription payment is due soon.</p>
                
                <div style='background-color: #fff3cd; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #f39c12;'>
                    <h3 style='color: #856404; margin-top: 0;'>Payment Details</h3>
                    <ul>
                        <li><strong>Amount Due:</strong> ${amount}</li>
                        <li><strong>Due Date:</strong> {dueDate:MMMM dd, yyyy}</li>
                        <li><strong>Plan:</strong> {subscription.SubscriptionPlan?.Name ?? "Premium Plan"}</li>
                        <li><strong>Days Until Due:</strong> {(int)((dueDate - DateTime.UtcNow).TotalDays)} days</li>
                    </ul>
                </div>

                <div style='background-color: #f8f9fa; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #6c757d;'>
                    <h3 style='color: #495057; margin-top: 0;'>To ensure uninterrupted service:</h3>
                    <ul>
                        <li>Verify your payment method is up to date</li>
                        <li>Ensure sufficient funds are available</li>
                        <li>Check for any billing issues</li>
                        <li>Contact support if you need assistance</li>
                    </ul>
                </div>

                <p style='text-align: center; margin: 30px 0;'>
                    <a href='https://smarttelehealth.com/billing' style='background-color: #f39c12; color: white; padding: 15px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>
                        Update Payment Method
                    </a>
                </p>

                <p><strong>Important:</strong> If payment is not received by the due date, your subscription may be suspended.</p>
                
                <p>Need help? Our support team is here to assist you.</p>
                
                <p>Best regards,<br><strong>Smart Telehealth Team</strong></p>
                
                <hr style='margin: 30px 0; border: none; border-top: 1px solid #ecf0f1;'>
                <p style='font-size: 12px; color: #7f8c8d;'>
                    This is an automated notification. Please do not reply to this email.
                </p>
            </div>";
    }

    #endregion
}
