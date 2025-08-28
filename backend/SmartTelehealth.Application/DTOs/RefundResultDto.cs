namespace SmartTelehealth.Application.DTOs
{
    public class RefundResultDto
    {
        public bool Success { get; set; }
        public string RefundId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public string Status { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public string? ErrorMessage { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime ProcessedAt { get; set; }
        public string PaymentIntentId { get; set; } = string.Empty;
        public Guid BillingRecordId { get; set; }
        public decimal RefundAmount { get; set; }
    }
} 