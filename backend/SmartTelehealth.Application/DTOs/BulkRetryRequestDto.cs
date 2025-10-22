namespace SmartTelehealth.Application.DTOs;

/// <summary>
/// Request DTO for bulk retrying failed payments
/// Phase 3: Failed Payment Management
/// </summary>
public class BulkRetryRequestDto
{
    /// <summary>
    /// List of billing record IDs to retry
    /// </summary>
    public List<Guid> BillingRecordIds { get; set; } = new();

    /// <summary>
    /// Delay between retry attempts in milliseconds
    /// Prevents overwhelming payment processor
    /// Default: 1000ms (1 second)
    /// </summary>
    public int DelayBetweenRetriesMs { get; set; } = 1000;

    /// <summary>
    /// Whether to send notifications for successful retries
    /// </summary>
    public bool NotifyOnSuccess { get; set; } = true;

    /// <summary>
    /// Whether to continue on individual failures
    /// If false, stops at first failure
    /// </summary>
    public bool ContinueOnError { get; set; } = true;
}

