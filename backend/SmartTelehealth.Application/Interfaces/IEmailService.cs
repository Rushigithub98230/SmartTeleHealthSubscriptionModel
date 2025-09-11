/*
 * ========================================================================================
 * MIGRATION-ONLY INTERFACE - DO NOT USE FOR CURRENT COMMUNICATION
 * ========================================================================================
 * 
 * This interface is ONLY for backward compatibility during SMTP to Twilio migration.
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
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;

namespace SmartTelehealth.Application.Interfaces;

/// <summary>
/// Legacy email service interface for backward compatibility during SMTP to Twilio migration.
/// This interface preserves the existing SMTP-based email methods to ensure a smooth transition
/// when migrating subscription management functionality to projects using Twilio.
/// 
/// These methods are designed to be drop-in replacements for existing SMTP implementations,
/// allowing minimal code changes during the migration process.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email asynchronously with organization branding.
    /// This method provides backward compatibility for existing SMTP-based implementations.
    /// </summary>
    /// <param name="email">Recipient email address</param>
    /// <param name="subject">Email subject line</param>
    /// <param name="message">Email message content (HTML or plain text)</param>
    /// <param name="organizationName">Organization name for branding</param>
    /// <returns>Task representing the asynchronous operation</returns>
    Task SendEmailAsync(
        string email, 
        string subject, 
        string message,
        string organizationName
    );

    /// <summary>
    /// Sends an email synchronously with organization branding.
    /// This method provides backward compatibility for existing SMTP-based implementations.
    /// </summary>
    /// <param name="email">Recipient email address</param>
    /// <param name="subject">Email subject line</param>
    /// <param name="bodyHtml">Email body content in HTML format</param>
    /// <param name="organizationName">Organization name for branding</param>
    /// <param name="toEmail">Additional recipient email (for CC/BCC scenarios)</param>
    /// <returns>True if email was sent successfully, false otherwise</returns>
    bool SendEmail(
        string email, 
        string subject, 
        string bodyHtml, 
        string organizationName, 
        string toEmail
    );

    /// <summary>
    /// Sends emails and returns a result identifier.
    /// This method provides backward compatibility for existing SMTP-based implementations.
    /// </summary>
    /// <param name="email">Recipient email address</param>
    /// <param name="subject">Email subject line</param>
    /// <param name="message">Email message content (HTML or plain text)</param>
    /// <param name="organizationName">Organization name for branding</param>
    /// <returns>Task containing a string identifier for the sent email</returns>
    Task<string> SendEmails(
        string email, 
        string subject, 
        string message, 
        string organizationName
    );
}
*/
