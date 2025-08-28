 
public class UpdateBillingRecordDto
{
    public int? BillingStatusId { get; set; }
    public DateTime? PaidDate { get; set; }
    public string? PaymentMethodId { get; set; }
    public string? StripePaymentIntentId { get; set; }
    public string? StripeSessionId { get; set; }
    public string? FailureReason { get; set; }
    public decimal? RefundAmount { get; set; }
    public string? RefundReason { get; set; }
    public DateTime? RefundDate { get; set; }
} 

public class BillingHistoryDto
{
    public string Id { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string BillingType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string Status { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Description { get; set; }
    public string? InvoiceNumber { get; set; }
    public string? ReceiptUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    // Added properties
    public string? SubscriptionId { get; set; }
    public DateTime? BillingDate { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? StripeInvoiceId { get; set; }
    public string? StripePaymentIntentId { get; set; }
}

public class CreateBillingAdjustmentDto
{
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string AdjustmentType { get; set; } = string.Empty;
    public string? Notes { get; set; }
} 