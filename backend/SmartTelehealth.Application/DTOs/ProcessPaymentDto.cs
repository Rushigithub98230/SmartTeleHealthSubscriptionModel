public class ProcessPaymentDto
{
    public string PaymentMethodId { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string? Description { get; set; }
    public string? PaymentIntentId { get; set; }
    public string? SessionId { get; set; }
    public string SubscriptionId { get; set; } = string.Empty;
    public int UserId { get; set; }
} 