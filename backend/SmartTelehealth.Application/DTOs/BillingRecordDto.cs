namespace SmartTelehealth.Application.DTOs;

public class BillingRecordDto
{
    public string Id { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string? SubscriptionId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime? PaidAt { get; set; }
    public string? PaymentIntentId { get; set; }
    public string? SubscriptionName { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Additional properties for Infrastructure layer compatibility
    public string? ConsultationId { get; set; }
    public string? MedicationDeliveryId { get; set; }
    public string Currency { get; set; } = "USD";
    public string PaymentMethod { get; set; } = string.Empty;
    public string StripeInvoiceId { get; set; } = string.Empty;
    public string StripePaymentIntentId { get; set; } = string.Empty;
    public DateTime BillingDate { get; set; }
    public bool IsRecurring { get; set; } = false;
    public decimal TaxAmount { get; set; } = 0;
    public decimal ShippingAmount { get; set; } = 0;
    public string? InvoiceNumber { get; set; }
    public bool IsPaid { get; set; } = false;
    public string? FailureReason { get; set; }
    public decimal? RefundAmount { get; set; }
    public string? RefundReason { get; set; }
    public DateTime? RefundDate { get; set; }
    public int? BillingStatusId { get; set; }
    public string? BillingStatusName { get; set; }
    public DateTime? PaidDate { get; set; }
    public string? PaymentMethodId { get; set; }
    public string? StripeSessionId { get; set; }
    public decimal? AccruedAmount { get; set; }
    public DateTime? AccrualStartDate { get; set; }
    public DateTime? AccrualEndDate { get; set; }
} 