using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

#region Improved Subscription Entity
public class Subscription : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    #region Constants
    public static class SubscriptionStatuses
    {
        public const string Pending = "Pending";
        public const string Active = "Active";
        public const string Paused = "Paused";
        public const string Cancelled = "Cancelled";
        public const string Expired = "Expired";
        public const string PaymentFailed = "PaymentFailed";
        public const string TrialActive = "TrialActive";
        public const string TrialExpired = "TrialExpired";
        public const string Suspended = "Suspended"; // Added
        
        public static readonly string[] ValidStatuses = 
        {
            Pending, Active, Paused, Cancelled, Expired, PaymentFailed, TrialActive, TrialExpired, Suspended
        };
    }
    #endregion

    #region Foreign Keys
    [Required]
    public int UserId { get; set; }
    [Required]
    public Guid SubscriptionPlanId { get; set; }
    [Required]
    public Guid BillingCycleId { get; set; }
    public int? ProviderId { get; set; }
    #endregion

    #region Navigation Properties
    public virtual User User { get; set; } = null!;
    public virtual SubscriptionPlan SubscriptionPlan { get; set; } = null!;
    public virtual MasterBillingCycle BillingCycle { get; set; } = null!;
    public virtual Provider? Provider { get; set; }
    #endregion

    #region Core Properties
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = SubscriptionStatuses.Pending;
    
    [MaxLength(500)]
    public string? StatusReason { get; set; }
    
    [Required]
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    [Required]
    public DateTime NextBillingDate { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal CurrentPrice { get; set; }
    
    public bool AutoRenew { get; set; } = true;
    
    [MaxLength(1000)]
    public string? Notes { get; set; }
    
    // Added missing property to fix build errors (alias for Status)
    public string SubscriptionStatus { get => Status; set => Status = value; }
    #endregion

    #region Status-Specific Properties
    public DateTime? PausedDate { get; set; }
    public DateTime? ResumedDate { get; set; }  // Renamed from ResumedAt for consistency
    public DateTime? CancelledDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public DateTime? SuspendedDate { get; set; } // Added
    public DateTime? LastBillingDate { get; set; } // Added
    
    [MaxLength(500)]
    public string? CancellationReason { get; set; }
    
    [MaxLength(500)]
    public string? PauseReason { get; set; }
    
    // Added missing properties to fix build errors (aliases for services)
    public DateTime? CancelledAt { get => CancelledDate; set => CancelledDate = value; }
    public DateTime? ExpiredAt { get => ExpirationDate; set => ExpirationDate = value; }
    public DateTime? RenewedAt { get => ResumedDate; set => ResumedDate = value; }
    public DateTime? ExpiryDate { get => ExpirationDate; set => ExpirationDate = value; }
    
    // Added alias properties for Amount and Currency to fix build errors
    public decimal Amount { get => CurrentPrice; set => CurrentPrice = value; }
    public string Currency { get => "USD"; set { } } // Default currency, read-only setter
    #endregion

    #region Payment Integration
    // Alias properties for backward compatibility
    public DateTime? CreatedAt { get => CreatedDate; set => CreatedDate = value; }
    public DateTime? UpdatedAt { get => UpdatedDate; set => UpdatedDate = value; }
    
    [MaxLength(100)]
    public string? StripeSubscriptionId { get; set; }
    
    [MaxLength(100)]
    public string? StripeCustomerId { get; set; }
    
    [MaxLength(100)]
    public string? PaymentMethodId { get; set; }
    
    public DateTime? LastPaymentDate { get; set; }
    public DateTime? LastPaymentFailedDate { get; set; }
    
    [MaxLength(500)]
    public string? LastPaymentError { get; set; }
    
    public int FailedPaymentAttempts { get; set; } = 0;
    #endregion

    #region Trial Properties
    public bool IsTrialSubscription { get; set; } = false;
    public DateTime? TrialStartDate { get; set; }
    public DateTime? TrialEndDate { get; set; }
    public int TrialDurationInDays { get; set; } = 0;
    #endregion

    #region Usage Tracking
    public DateTime? LastUsedDate { get; set; }
    public int TotalUsageCount { get; set; } = 0;
    #endregion

    #region Collection Navigation Properties
    public virtual ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
    public virtual ICollection<MedicationDelivery> MedicationDeliveries { get; set; } = new List<MedicationDelivery>();
    public virtual ICollection<BillingRecord> BillingRecords { get; set; } = new List<BillingRecord>();
    public virtual ICollection<UserSubscriptionPrivilegeUsage> PrivilegeUsages { get; set; } = new List<UserSubscriptionPrivilegeUsage>();
    public virtual ICollection<SubscriptionStatusHistory> StatusHistory { get; set; } = new List<SubscriptionStatusHistory>();
    public virtual ICollection<SubscriptionPayment> Payments { get; set; } = new List<SubscriptionPayment>();
    #endregion

    #region Computed Properties
    [NotMapped]
    public bool IsSubscriptionActive => Status == SubscriptionStatuses.Active;
    
    [NotMapped]
    public bool IsPaused => Status == SubscriptionStatuses.Paused;
    
    [NotMapped]
    public bool IsCancelled => Status == SubscriptionStatuses.Cancelled;
    
    [NotMapped]
    public bool IsExpired => Status == SubscriptionStatuses.Expired;
    
    [NotMapped]
    public bool HasPaymentIssues => Status == SubscriptionStatuses.PaymentFailed || FailedPaymentAttempts > 0;
    
    [NotMapped]
    public bool IsInTrial => Status == SubscriptionStatuses.TrialActive || 
        (IsTrialSubscription && TrialEndDate.HasValue && DateTime.UtcNow <= TrialEndDate.Value);
    
    [NotMapped]
    public int DaysUntilNextBilling => (int)(NextBillingDate - DateTime.UtcNow).TotalDays;
    
    [NotMapped]
    public bool IsNearExpiration => DaysUntilNextBilling <= 7 && DaysUntilNextBilling > 0;
    #endregion

    #region Business Logic Properties
    [NotMapped]
    public bool CanPause => IsActive && !HasPaymentIssues;
    
    [NotMapped]
    public bool CanResume => IsPaused;
    
    [NotMapped]
    public bool CanCancel => IsActive || IsPaused || Status == SubscriptionStatuses.Pending;
    
    [NotMapped]
    public bool CanRenew => (IsExpired || IsCancelled) && AutoRenew;
    #endregion

    #region Validation Methods
    public ValidationResult ValidateStatusTransition(string newStatus)
    {
        if (!SubscriptionStatuses.ValidStatuses.Contains(newStatus))
        {
            return new ValidationResult($"'{newStatus}' is not a valid subscription status.");
        }
        
        if (Status == newStatus)
        {
            return new ValidationResult($"Subscription is already in '{newStatus}' status.");
        }
        
        var validTransitions = GetValidStatusTransitions();
        if (!validTransitions.Contains(newStatus))
        {
            return new ValidationResult($"Cannot transition from '{Status}' to '{newStatus}'.");
        }
        
        return ValidationResult.Success;
    }
    
    public string[] GetValidStatusTransitions()
    {
        return Status switch
        {
            SubscriptionStatuses.Pending => new[] { SubscriptionStatuses.Active, SubscriptionStatuses.TrialActive, SubscriptionStatuses.Cancelled },
            SubscriptionStatuses.Active => new[] { SubscriptionStatuses.Paused, SubscriptionStatuses.Cancelled, SubscriptionStatuses.Expired, SubscriptionStatuses.PaymentFailed },
            SubscriptionStatuses.Paused => new[] { SubscriptionStatuses.Active, SubscriptionStatuses.Cancelled, SubscriptionStatuses.Expired },
            SubscriptionStatuses.PaymentFailed => new[] { SubscriptionStatuses.Active, SubscriptionStatuses.Cancelled, SubscriptionStatuses.Expired },
            SubscriptionStatuses.TrialActive => new[] { SubscriptionStatuses.Active, SubscriptionStatuses.TrialExpired, SubscriptionStatuses.Cancelled },
            SubscriptionStatuses.TrialExpired => new[] { SubscriptionStatuses.Active, SubscriptionStatuses.Cancelled },
            SubscriptionStatuses.Expired => new[] { SubscriptionStatuses.Active },
            SubscriptionStatuses.Cancelled => Array.Empty<string>(),
            _ => Array.Empty<string>()
        };
    }
    #endregion
}
#endregion 