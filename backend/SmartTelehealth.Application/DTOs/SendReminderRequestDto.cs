namespace SmartTelehealth.Application.DTOs;

/// <summary>
/// Request DTO for sending payment reminder emails
/// Phase 3: Failed Payment Management
/// </summary>
public class SendReminderRequestDto
{
    /// <summary>
    /// Custom message to include in the reminder email (optional)
    /// If not provided, uses default template
    /// </summary>
    public string? CustomMessage { get; set; }

    /// <summary>
    /// Urgency level of the reminder (Normal, Urgent, Final)
    /// Affects email template and tone
    /// </summary>
    public string ReminderType { get; set; } = "Normal";

    /// <summary>
    /// Include payment link in the email for easy payment
    /// </summary>
    public bool IncludePaymentLink { get; set; } = true;

    /// <summary>
    /// CC admin on the reminder email
    /// </summary>
    public bool CopyAdmin { get; set; } = false;
}

