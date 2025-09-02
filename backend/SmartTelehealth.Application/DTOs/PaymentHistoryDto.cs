namespace SmartTelehealth.Application.DTOs
{
    public class PaymentHistoryDto
    {
        public Guid Id { get; set; }
        public int UserId { get; set; }
        public string SubscriptionId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? TransactionId { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public DateTime PaymentDate { get; set; }
        public string? Description { get; set; }
        public string? PaymentMethodId { get; set; }
    }

    public class PaymentHistorySearchDto
    {
        public string? UserId { get; set; }
        public string? SubscriptionId { get; set; }
        public string? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
} 