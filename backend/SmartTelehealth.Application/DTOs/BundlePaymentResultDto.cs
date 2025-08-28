namespace SmartTelehealth.Application.DTOs;

/// <summary>
/// DTO for bundle payment processing results
/// </summary>
public class BundlePaymentResultDto
{
    /// <summary>
    /// Payment status (succeeded, failed, pending, etc.)
    /// </summary>
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// Unique transaction identifier
    /// </summary>
    public string TransactionId { get; set; } = string.Empty;
    
    /// <summary>
    /// Total amount processed
    /// </summary>
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Currency code (e.g., USD, EUR)
    /// </summary>
    public string Currency { get; set; } = "USD";
    
    /// <summary>
    /// Error message if payment failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Number of items in the bundle
    /// </summary>
    public int ItemCount { get; set; }
    
    /// <summary>
    /// Processing timestamp
    /// </summary>
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Payment method used
    /// </summary>
    public string PaymentMethodId { get; set; } = string.Empty;
    
    // Added missing properties to fix build errors
    public Guid BundleId { get; set; }
    public decimal TotalAmount { get; set; }
}
