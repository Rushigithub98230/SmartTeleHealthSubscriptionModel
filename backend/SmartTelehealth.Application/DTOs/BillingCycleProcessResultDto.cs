namespace SmartTelehealth.Application.DTOs;

/// <summary>
/// DTO for billing cycle processing results
/// </summary>
public class BillingCycleProcessResultDto
{
    /// <summary>
    /// Whether the processing was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Human-readable message about the processing result
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Number of subscriptions successfully processed
    /// </summary>
    public int ProcessedCount { get; set; }
    
    /// <summary>
    /// Number of subscriptions that failed processing
    /// </summary>
    public int FailedCount { get; set; }
    
    /// <summary>
    /// List of error messages for failed processing
    /// </summary>
    public List<string> Errors { get; set; } = new List<string>();
    
    /// <summary>
    /// Total amount processed
    /// </summary>
    public decimal TotalAmount { get; set; }
    
    /// <summary>
    /// Processing start time
    /// </summary>
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Processing completion time
    /// </summary>
    public DateTime? CompletedAt { get; set; }
    
    /// <summary>
    /// Processing duration in milliseconds
    /// </summary>
    public long DurationMs => CompletedAt.HasValue ? (long)(CompletedAt.Value - StartedAt).TotalMilliseconds : 0;
    
    /// <summary>
    /// Billing cycle ID that was processed
    /// </summary>
    public Guid BillingCycleId { get; set; }
    
    // Added missing properties to fix build errors
    public DateTime ProcessedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public int RecordsProcessed { get; set; }
}
