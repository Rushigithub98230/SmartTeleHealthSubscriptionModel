namespace SmartTelehealth.Application.DTOs;

public class UpdateBillingRecordDto
{
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? PaidAt { get; set; }
    public string? PaymentIntentId { get; set; }
} 