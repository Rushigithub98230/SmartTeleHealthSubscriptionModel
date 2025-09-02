using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

public class ProviderPayout : BaseEntity
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public int ProviderId { get; set; }
    public virtual User Provider { get; set; } = null!;
    
    [Required]
    public Guid PayoutPeriodId { get; set; }
    public virtual PayoutPeriod PayoutPeriod { get; set; } = null!;
    
    [Required]
    [Range(0, 1000000)]
    public decimal TotalEarnings { get; set; }
    
    [Required]
    [Range(0, 1000000)]
    public decimal PlatformCommission { get; set; }
    
    [Required]
    [Range(0, 1000000)]
    public decimal NetPayout { get; set; }
    
    public int TotalConsultations { get; set; }
    
    public int TotalOneTimeConsultations { get; set; }
    
    public int TotalSubscriptionConsultations { get; set; }
    
    public PayoutStatus Status { get; set; } = PayoutStatus.Pending;
    
    [MaxLength(1000)]
    public string? AdminRemarks { get; set; }
    
    public DateTime? ProcessedAt { get; set; }
    
    public int? ProcessedByUserId { get; set; }
    public virtual User? ProcessedByUser { get; set; }
    
    [MaxLength(255)]
    public string? TransactionId { get; set; }
    
    [MaxLength(255)]
    public string? PaymentMethodId { get; set; }
    
    // Navigation properties for detailed breakdown
    public virtual ICollection<PayoutDetail> PayoutDetails { get; set; } = new List<PayoutDetail>();
}

public class PayoutPeriod : BaseEntity
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    public DateTime StartDate { get; set; }
    
    public DateTime EndDate { get; set; }
    
    public PayoutPeriodStatus Status { get; set; } = PayoutPeriodStatus.Open;
    
    public DateTime? ProcessedAt { get; set; }
    
    public int? ProcessedByUserId { get; set; }
    public virtual User? ProcessedByUser { get; set; }
    
    // Alias properties for backward compatibility
    public DateTime? CreatedDate { get => CreatedDate; set => CreatedDate = value; }
    public DateTime? UpdatedDate { get => UpdatedDate; set => UpdatedDate = value; }
    
    // Navigation properties
    public virtual ICollection<ProviderPayout> ProviderPayouts { get; set; } = new List<ProviderPayout>();
}

public class PayoutDetail : BaseEntity
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public Guid PayoutId { get; set; }
    public virtual ProviderPayout Payout { get; set; } = null!;
    
    [Required]
    public Guid AppointmentId { get; set; }
    public virtual Appointment Appointment { get; set; } = null!;
    
    [Required]
    [Range(0, 10000)]
    public decimal ConsultationFee { get; set; }
    
    [Required]
    [Range(0, 10000)]
    public decimal PlatformCommission { get; set; }
    
    [Required]
    [Range(0, 10000)]
    public decimal ProviderEarnings { get; set; }
    
    public ConsultationType ConsultationType { get; set; }
    
    // Alias properties for backward compatibility
    public DateTime? CreatedDate { get => CreatedDate; set => CreatedDate = value; }
    public DateTime? UpdatedDate { get => UpdatedDate; set => UpdatedDate = value; }
    
    // Navigation properties
    public virtual ICollection<PayoutDetail> PayoutDetails { get; set; } = new List<PayoutDetail>();
}

public enum PayoutStatus
{
    Pending = 0,
    UnderReview = 1,
    Approved = 2,
    Processed = 3,
    OnHold = 4,
    Cancelled = 5
}

public enum PayoutPeriodStatus
{
    Open = 0,
    Processing = 1,
    Completed = 2,
    Cancelled = 3
}

public enum ConsultationType
{
    OneTime = 0,
    Subscription = 1
} 