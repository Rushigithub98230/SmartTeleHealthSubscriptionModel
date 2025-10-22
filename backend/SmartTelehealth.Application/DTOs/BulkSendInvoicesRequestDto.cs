namespace SmartTelehealth.Application.DTOs;

/// <summary>
/// Request DTO for bulk sending invoices
/// Phase 4: Invoice Management
/// </summary>
public class BulkSendInvoicesRequestDto
{
    /// <summary>
    /// List of invoice numbers to send
    /// </summary>
    public List<string> InvoiceNumbers { get; set; } = new();

    /// <summary>
    /// Custom email template to use (optional)
    /// If not provided, uses default invoice email template
    /// </summary>
    public string? EmailTemplate { get; set; }

    /// <summary>
    /// Custom subject line for the email (optional)
    /// </summary>
    public string? CustomSubject { get; set; }

    /// <summary>
    /// Delay between emails in milliseconds
    /// Prevents email service rate limiting
    /// Default: 500ms
    /// </summary>
    public int DelayBetweenEmailsMs { get; set; } = 500;

    /// <summary>
    /// Whether to continue sending if one fails
    /// </summary>
    public bool ContinueOnError { get; set; } = true;
}

