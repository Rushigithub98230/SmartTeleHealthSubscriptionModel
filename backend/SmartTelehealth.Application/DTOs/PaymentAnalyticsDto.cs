namespace SmartTelehealth.Application.DTOs
{
    public class PaymentAnalyticsDto
    {
        public decimal TotalPayments { get; set; }
        public decimal SuccessfulPayments { get; set; }
        public decimal FailedPayments { get; set; }
        public decimal PaymentSuccessRate { get; set; }
        public decimal AveragePaymentAmount { get; set; }
        public decimal TotalRefunds { get; set; }
        public decimal TotalSpent { get; set; }
        public int TotalTransactions { get; set; }
        public int SuccessfulTransactions { get; set; }
        public int FailedTransactions { get; set; }
        public List<MonthlyPaymentDto> MonthlyPayments { get; set; } = new();
        public List<PaymentMethodAnalyticsDto> PaymentMethods { get; set; } = new();
        public List<PaymentStatusAnalyticsDto> PaymentStatuses { get; set; } = new();
    }

    public class MonthlyPaymentDto
    {
        public string Month { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int TransactionCount { get; set; }
        public int SuccessfulCount { get; set; }
        public int FailedCount { get; set; }
        public decimal Amount { get; set; }
        public int Count { get; set; }
    }

    public class PaymentMethodAnalyticsDto
    {
        public string Method { get; set; } = string.Empty;
        public int UsageCount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal SuccessRate { get; set; }
    }

    public class PaymentStatusAnalyticsDto
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
    }
} 