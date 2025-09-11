/*
 * ========================================================================================
 * MIGRATION-ONLY SERVICE - DO NOT USE FOR CURRENT COMMUNICATION
 * ========================================================================================
 * 
 * This service is ONLY for backward compatibility during SMTP to Twilio migration.
 * It is NOT currently in use and should NOT be used for regular email communication.
 * 
 * Purpose: Provides drop-in replacement for existing SMTP implementations when migrating
 *          subscription management functionality to projects using Twilio.
 * 
 * Status: COMMENTED OUT - Uncomment only when needed for migration purposes.
 * 
 * ========================================================================================
 */

/*
using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.Interfaces;

namespace SmartTelehealth.Infrastructure.Services;

/// <summary>
/// Legacy email service implementation for backward compatibility during SMTP to Twilio migration.
/// This service provides a drop-in replacement for existing SMTP-based email implementations,
/// ensuring minimal code changes during the migration process.
/// 
/// The service delegates to the modern ICommunicationService (TwilioService) while maintaining
/// the legacy interface for seamless migration.
/// </summary>
public class EmailService : IEmailService
{
    private readonly ICommunicationService _communicationService;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        ICommunicationService communicationService,
        ILogger<EmailService> logger)
    {
        _communicationService = communicationService ?? throw new ArgumentNullException(nameof(communicationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Sends an email asynchronously with organization branding.
    /// This method provides backward compatibility for existing SMTP-based implementations.
    /// </summary>
    public async Task SendEmailAsync(string email, string subject, string message, string organizationName)
    {
        try
        {
            _logger.LogInformation("Sending legacy email to {Email} with subject '{Subject}' for organization '{Organization}'", 
                email, subject, organizationName);

            // Delegate to the modern communication service
            var result = await _communicationService.SendEmailAsync(email, subject, message, true, null);
            
            if (result.StatusCode != 200)
            {
                _logger.LogError("Legacy email failed for {Email}: {Message}", email, result.Message);
                throw new InvalidOperationException($"Failed to send email: {result.Message}");
            }

            _logger.LogInformation("Legacy email sent successfully to {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending legacy email to {Email}", email);
            throw;
        }
    }

    /// <summary>
    /// Sends an email synchronously with organization branding.
    /// This method provides backward compatibility for existing SMTP-based implementations.
    /// </summary>
    public bool SendEmail(string email, string subject, string bodyHtml, string organizationName, string toEmail)
    {
        try
        {
            _logger.LogInformation("Sending legacy synchronous email to {Email} with subject '{Subject}' for organization '{Organization}'", 
                email, subject, organizationName);

            // Use the primary email address, fallback to toEmail if needed
            var primaryEmail = !string.IsNullOrEmpty(email) ? email : toEmail;
            
            // Delegate to the modern communication service synchronously
            var result = _communicationService.SendEmailAsync(primaryEmail, subject, bodyHtml, true, null).GetAwaiter().GetResult();
            
            if (result.StatusCode == 200)
            {
                _logger.LogInformation("Legacy synchronous email sent successfully to {Email}", primaryEmail);
                return true;
            }
            else
            {
                _logger.LogError("Legacy synchronous email failed for {Email}: {Message}", primaryEmail, result.Message);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending legacy synchronous email to {Email}", email);
            return false;
        }
    }

    /// <summary>
    /// Sends emails and returns a result identifier.
    /// This method provides backward compatibility for existing SMTP-based implementations.
    /// </summary>
    public async Task<string> SendEmails(string email, string subject, string message, string organizationName)
    {
        try
        {
            _logger.LogInformation("Sending legacy email with identifier to {Email} with subject '{Subject}' for organization '{Organization}'", 
                email, subject, organizationName);

            // Delegate to the modern communication service
            var result = await _communicationService.SendEmailAsync(email, subject, message, true, null);
            
            if (result.StatusCode != 200)
            {
                _logger.LogError("Legacy email with identifier failed for {Email}: {Message}", email, result.Message);
                throw new InvalidOperationException($"Failed to send email: {result.Message}");
            }

            // Generate a unique identifier for the sent email
            var emailId = $"legacy_{Guid.NewGuid():N}_{DateTime.UtcNow:yyyyMMddHHmmss}";
            
            _logger.LogInformation("Legacy email with identifier sent successfully to {Email} with ID {EmailId}", email, emailId);
            return emailId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending legacy email with identifier to {Email}", email);
            throw;
        }
    }
}
*/
