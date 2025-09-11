/*
 * ========================================================================================
 * MIGRATION-ONLY EXAMPLES - DO NOT USE FOR CURRENT COMMUNICATION
 * ========================================================================================
 * 
 * This file contains examples for backward compatibility during SMTP to Twilio migration.
 * It is NOT currently in use and should NOT be used for regular email communication.
 * 
 * Purpose: Provides usage examples for migrating existing SMTP implementations when
 *          migrating subscription management functionality to projects using Twilio.
 * 
 * Status: COMMENTED OUT - Uncomment only when needed for migration purposes.
 * 
 * ========================================================================================
 */

/*
using SmartTelehealth.Application.Interfaces;

namespace SmartTelehealth.Examples;

/// <summary>
/// Example demonstrating how to use the IEmailService for backward compatibility
/// during SMTP to Twilio migration. This shows how existing code can be migrated
/// with minimal changes.
/// </summary>
public class LegacyEmailUsageExample
{
    private readonly IEmailService _emailService;

    public LegacyEmailUsageExample(IEmailService emailService)
    {
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
    }

    /// <summary>
    /// Example 1: User Registration Email
    /// This method shows how to send a welcome email using the legacy interface.
    /// The method signature and usage remain exactly the same as with SMTP.
    /// </summary>
    public async Task SendUserRegistrationEmailAsync(string userEmail, string userName, string organizationName)
    {
        try
        {
            string subject = "Welcome to Our Platform";
            string message = $@"
                <html>
                <body>
                    <h2>Welcome {userName}!</h2>
                    <p>Thank you for registering with {organizationName}.</p>
                    <p>Your account has been successfully created.</p>
                    <p>Best regards,<br>{organizationName} Team</p>
                </body>
                </html>";

            // This call works exactly the same as with SMTP - no changes needed!
            await _emailService.SendEmailAsync(userEmail, subject, message, organizationName);
        }
        catch (Exception ex)
        {
            // Handle email sending errors
            throw new InvalidOperationException($"Failed to send registration email to {userEmail}", ex);
        }
    }

    /// <summary>
    /// Example 2: Password Reset Email (Synchronous)
    /// This method shows how to send a password reset email using the synchronous method.
    /// </summary>
    public bool SendPasswordResetEmail(string userEmail, string resetLink, string organizationName)
    {
        try
        {
            string subject = "Password Reset Request";
            string htmlBody = $@"
                <html>
                <body>
                    <h2>Password Reset</h2>
                    <p>You requested a password reset for your {organizationName} account.</p>
                    <p>Click the link below to reset your password:</p>
                    <a href='{resetLink}'>Reset Password</a>
                    <p>This link will expire in 24 hours.</p>
                    <p>If you didn't request this, please ignore this email.</p>
                    <p>Best regards,<br>{organizationName} Team</p>
                </body>
                </html>";

            // Synchronous call - works exactly the same as with SMTP
            return _emailService.SendEmail(userEmail, subject, htmlBody, organizationName, userEmail);
        }
        catch (Exception ex)
        {
            // Log error and return false
            Console.WriteLine($"Failed to send password reset email: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Example 3: Bulk Email with Tracking
    /// This method shows how to send emails and track them using the identifier.
    /// </summary>
    public async Task<List<string>> SendBulkNotificationEmailsAsync(List<string> userEmails, string notificationMessage, string organizationName)
    {
        var emailIds = new List<string>();

        foreach (var email in userEmails)
        {
            try
            {
                string subject = "Important Notification";
                string message = $@"
                    <html>
                    <body>
                        <h2>Important Update</h2>
                        <p>{notificationMessage}</p>
                        <p>Best regards,<br>{organizationName} Team</p>
                    </body>
                    </html>";

                // Send email and get tracking ID
                string emailId = await _emailService.SendEmails(email, subject, message, organizationName);
                emailIds.Add(emailId);

                // Optional: Store emailId in database for tracking
                // await _emailTrackingRepository.StoreEmailIdAsync(email, emailId);
            }
            catch (Exception ex)
            {
                // Log error but continue with other emails
                Console.WriteLine($"Failed to send email to {email}: {ex.Message}");
            }
        }

        return emailIds;
    }

    /// <summary>
    /// Example 4: Migration from SMTP Service
    /// This shows how to replace an existing SMTP service with minimal changes.
    /// </summary>
    public class MigrationExample
    {
        // OLD: Using SMTP service
        // private readonly ISmtpEmailService _smtpService;

        // NEW: Using legacy-compatible email service
        private readonly IEmailService _emailService;

        public MigrationExample(IEmailService emailService)
        {
            _emailService = emailService;
        }

        // This method requires NO CHANGES - it works exactly the same!
        public async Task SendOrderConfirmationAsync(string customerEmail, string orderNumber, string organizationName)
        {
            string subject = "Order Confirmation";
            string message = $@"
                <html>
                <body>
                    <h2>Order Confirmed</h2>
                    <p>Your order #{orderNumber} has been confirmed.</p>
                    <p>Thank you for choosing {organizationName}!</p>
                </body>
                </html>";

            // This line works exactly the same with both SMTP and Twilio
            await _emailService.SendEmailAsync(customerEmail, subject, message, organizationName);
        }
    }
}

/// <summary>
/// Example showing how to configure dependency injection for the migration
/// </summary>
public static class EmailServiceConfiguration
{
    /// <summary>
    /// Configuration for migrating from SMTP to Twilio
    /// </summary>
    public static void ConfigureEmailServices(IServiceCollection services)
    {
        // Register the legacy-compatible email service
        services.AddScoped<IEmailService, EmailService>();

        // The EmailService will automatically use ICommunicationService (TwilioService)
        // which is already registered in the DI container
    }

    /// <summary>
    /// Alternative configuration if you want to use a different email provider
    /// </summary>
    public static void ConfigureCustomEmailServices(IServiceCollection services)
    {
        // You can create a custom implementation that uses a different provider
        services.AddScoped<IEmailService, CustomEmailService>();
    }
}

/// <summary>
/// Example of a custom email service implementation
/// </summary>
public class CustomEmailService : IEmailService
{
    private readonly ILogger<CustomEmailService> _logger;

    public CustomEmailService(ILogger<CustomEmailService> logger)
    {
        _logger = logger;
    }

    public async Task SendEmailAsync(string email, string subject, string message, string organizationName)
    {
        // Implement using your preferred email provider
        _logger.LogInformation("Sending email to {Email} with subject '{Subject}' for {Organization}", 
            email, subject, organizationName);
        
        // Your custom email sending logic here
        await Task.CompletedTask;
    }

    public bool SendEmail(string email, string subject, string bodyHtml, string organizationName, string toEmail)
    {
        // Implement synchronous email sending
        _logger.LogInformation("Sending synchronous email to {Email}", email);
        
        // Your custom email sending logic here
        return true;
    }

    public async Task<string> SendEmails(string email, string subject, string message, string organizationName)
    {
        // Implement email sending with tracking
        _logger.LogInformation("Sending tracked email to {Email}", email);
        
        // Your custom email sending logic here
        await Task.CompletedTask;
        
        return $"custom_{Guid.NewGuid()}";
    }
}
*/
