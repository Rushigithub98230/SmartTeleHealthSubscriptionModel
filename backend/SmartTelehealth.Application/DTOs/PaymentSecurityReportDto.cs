namespace SmartTelehealth.Application.DTOs
{
    public class PaymentSecurityReportDto
    {
        public int UserId { get; set; }
        public DateTime ReportDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalPaymentAttempts { get; set; }
        public int SuccessfulPayments { get; set; }
        public int FailedPayments { get; set; }
        public int SuspiciousActivities { get; set; }
        public int RateLimitViolations { get; set; }
        public int AmountLimitViolations { get; set; }
        public decimal TotalAmountProcessed { get; set; }
        public decimal AverageAmountPerPayment { get; set; }
        public decimal AverageAmount { get; set; }
        public List<PaymentSecurityEventDto> SecurityEvents { get; set; } = new();
        public List<string> RiskFactors { get; set; } = new();
        public string RiskScore { get; set; } = "LOW";
        public string Recommendation { get; set; } = string.Empty;
        
        // Additional properties for PaymentSecurityService
        public int TotalAttempts { get; set; }
        public int SuccessfulAttempts { get; set; }
        public int FailedAttempts { get; set; }
        public int GeneratedBy { get; set; }
        public DateTime GeneratedAt { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class PaymentSecurityEventDto
    {
        public DateTime Timestamp { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public decimal? Amount { get; set; }
        public string Status { get; set; } = string.Empty;
    }
} 