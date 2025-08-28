namespace SmartTelehealth.Application.DTOs;

public class PaymentResultDto
{
    public string Status { get; set; } = string.Empty;
    public string? PaymentIntentId { get; set; }
    public string? CustomerId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "usd";
    public string? ErrorMessage { get; set; }
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    public bool Success => Status == "succeeded";
    public string? ReceiptUrl { get; set; }
    public string? InvoiceId { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public class CustomerDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public string? DefaultPaymentMethodId { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
} 