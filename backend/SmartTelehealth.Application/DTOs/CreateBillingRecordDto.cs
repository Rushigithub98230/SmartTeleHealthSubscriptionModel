using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Application.DTOs;

public class CreateBillingRecordDto
{
    [Required]
    public int UserId { get; set; }
    
    public string? SubscriptionId { get; set; }
    
    [Required]
    public decimal Amount { get; set; }
    
    public decimal TaxAmount { get; set; } = 0;
    
    public decimal TotalAmount { get; set; }
    
    [Required]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public DateTime BillingDate { get; set; } = DateTime.UtcNow;
    
    [Required]
    public DateTime DueDate { get; set; }
    
    public string? PaymentMethod { get; set; }
    
    public string? StripeInvoiceId { get; set; }
    
    public string? StripePaymentIntentId { get; set; }
    
    [Required]
    public string Status { get; set; } = "Pending";
    
    [Required]
    public string Type { get; set; } = "Subscription";
    
    public Guid? CurrencyId { get; set; }
    
    public DateTime? PaidAt { get; set; }
    
    public string? InvoiceNumber { get; set; }
    
    public bool IsRecurring { get; set; } = false;
    
    public decimal ShippingAmount { get; set; } = 0;
    
    public bool IsPaid { get; set; } = false;
    
    public string? FailureReason { get; set; }
    
    public string? ConsultationId { get; set; }
}


