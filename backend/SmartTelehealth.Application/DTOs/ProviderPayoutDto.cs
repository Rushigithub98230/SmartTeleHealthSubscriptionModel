namespace SmartTelehealth.Application.DTOs;

public class ProviderPayoutDto
{
    public Guid Id { get; set; }
    public int ProviderId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public Guid PayoutPeriodId { get; set; }
    public string PayoutPeriodName { get; set; } = string.Empty;
    public DateTime PayoutPeriodStartDate { get; set; }
    public DateTime PayoutPeriodEndDate { get; set; }
    public decimal TotalEarnings { get; set; }
    public decimal PlatformCommission { get; set; }
    public decimal NetPayout { get; set; }
    public int TotalConsultations { get; set; }
    public int TotalOneTimeConsultations { get; set; }
    public int TotalSubscriptionConsultations { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? AdminRemarks { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public Guid? ProcessedByUserId { get; set; }
    public string? ProcessedByUserName { get; set; }
    public string? TransactionId { get; set; }
    public string? PaymentMethodId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<PayoutDetailDto> PayoutDetails { get; set; } = new();
}

public class PayoutDetailDto
{
    public Guid Id { get; set; }
    public Guid PayoutId { get; set; }
    public Guid AppointmentId { get; set; }
    public string AppointmentTitle { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; }
    public decimal ConsultationFee { get; set; }
    public decimal PlatformCommission { get; set; }
    public decimal ProviderEarnings { get; set; }
    public string ConsultationType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class PayoutPeriodDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? ProcessedAt { get; set; }
    public Guid? ProcessedByUserId { get; set; }
    public string? ProcessedByUserName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int TotalPayouts { get; set; }
    public decimal TotalAmount { get; set; }
}

public class CreatePayoutPeriodDto
{
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class ProcessPayoutDto
{
    public string Status { get; set; } = string.Empty;
    public string? AdminRemarks { get; set; }
    public string? TransactionId { get; set; }
    public string? PaymentMethodId { get; set; }
    public string? PaymentReference { get; set; }
    public string? Notes { get; set; }
}

public class ProviderEarningsDto
{
    public int ProviderId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public decimal TotalEarnings { get; set; }
    public decimal PendingEarnings { get; set; }
    public decimal ProcessedEarnings { get; set; }
    public int TotalConsultations { get; set; }
    public int PendingConsultations { get; set; }
    public DateTime? LastPayoutDate { get; set; }
    public List<MonthlyEarningsDto> MonthlyEarnings { get; set; } = new();
    public decimal AveragePerConsultation { get; set; }
    public DateTime? NextPayoutDate { get; set; }
}

public class MonthlyEarningsDto
{
    public string Month { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal Earnings { get; set; }
    public int Consultations { get; set; }
} 