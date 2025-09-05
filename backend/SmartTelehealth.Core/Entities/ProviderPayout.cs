using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

/// <summary>
/// Core provider payout entity that manages all provider payouts in the system.
/// This entity handles provider payout creation, management, and processing for healthcare providers.
/// It serves as the central hub for provider payout management, providing payout creation,
/// commission calculation, and payment processing capabilities.
/// </summary>
public class ProviderPayout : BaseEntity
{
    /// <summary>
    /// Primary key identifier for the provider payout.
    /// Uses Guid for better scalability and security in distributed systems.
    /// Unique identifier for each provider payout in the system.
    /// </summary>
    [Key]
    public Guid Id { get; set; }
    
    /// <summary>
    /// ID of the provider (User) who is receiving this payout.
    /// Used for provider identification and payout management.
    /// Required for payout processing and provider identification.
    /// </summary>
    [Required]
    public int ProviderId { get; set; }
    
    /// <summary>
    /// Navigation property to the Provider (User) who is receiving this payout.
    /// Provides access to provider information for payout management.
    /// Used for provider-payout relationship operations.
    /// </summary>
    public virtual User Provider { get; set; } = null!;
    
    /// <summary>
    /// ID of the payout period this payout belongs to.
    /// Used for payout period identification and management.
    /// Required for payout period tracking and management.
    /// </summary>
    [Required]
    public Guid PayoutPeriodId { get; set; }
    
    /// <summary>
    /// Navigation property to the PayoutPeriod this payout belongs to.
    /// Provides access to payout period information for payout management.
    /// Used for payout-period relationship operations.
    /// </summary>
    public virtual PayoutPeriod PayoutPeriod { get; set; } = null!;
    
    /// <summary>
    /// Total earnings for this payout period.
    /// Used for payout calculation and provider earnings tracking.
    /// Required for payout processing and financial management.
    /// </summary>
    [Required]
    [Range(0, 1000000)]
    public decimal TotalEarnings { get; set; }
    
    /// <summary>
    /// Platform commission for this payout period.
    /// Used for commission calculation and platform revenue tracking.
    /// Required for payout processing and financial management.
    /// </summary>
    [Required]
    [Range(0, 1000000)]
    public decimal PlatformCommission { get; set; }
    
    /// <summary>
    /// Net payout amount after commission deduction.
    /// Used for payout calculation and provider payment processing.
    /// Required for payout processing and financial management.
    /// </summary>
    [Required]
    [Range(0, 1000000)]
    public decimal NetPayout { get; set; }
    
    /// <summary>
    /// Total number of consultations in this payout period.
    /// Used for payout calculation and provider performance tracking.
    /// Required for payout processing and analytics.
    /// </summary>
    public int TotalConsultations { get; set; }
    
    /// <summary>
    /// Total number of one-time consultations in this payout period.
    /// Used for payout calculation and consultation type tracking.
    /// Required for payout processing and analytics.
    /// </summary>
    public int TotalOneTimeConsultations { get; set; }
    
    /// <summary>
    /// Total number of subscription consultations in this payout period.
    /// Used for payout calculation and consultation type tracking.
    /// Required for payout processing and analytics.
    /// </summary>
    public int TotalSubscriptionConsultations { get; set; }
    
    /// <summary>
    /// Current status of this payout.
    /// Used for payout status tracking and management.
    /// Defaults to Pending for new payouts.
    /// </summary>
    public PayoutStatus Status { get; set; } = PayoutStatus.Pending;
    
    /// <summary>
    /// Admin remarks for this payout.
    /// Used for payout documentation and admin communication.
    /// Optional - used for enhanced payout management and documentation.
    /// </summary>
    [MaxLength(1000)]
    public string? AdminRemarks { get; set; }
    
    /// <summary>
    /// Date and time when this payout was processed.
    /// Used for payout processing tracking and management.
    /// Set when the payout is processed by administrators.
    /// </summary>
    public DateTime? ProcessedAt { get; set; }
    
    /// <summary>
    /// ID of the user who processed this payout.
    /// Used for payout processing tracking and audit trails.
    /// Optional - used for enhanced payout management and audit capabilities.
    /// </summary>
    public int? ProcessedByUserId { get; set; }
    
    /// <summary>
    /// Navigation property to the User who processed this payout.
    /// Provides access to user information for payout processing management.
    /// Used for user-payout relationship operations.
    /// </summary>
    public virtual User? ProcessedByUser { get; set; }
    
    /// <summary>
    /// Transaction ID for this payout.
    /// Used for payout transaction tracking and management.
    /// Optional - used for enhanced payout management and transaction tracking.
    /// </summary>
    [MaxLength(255)]
    public string? TransactionId { get; set; }
    
    /// <summary>
    /// Payment method ID for this payout.
    /// Used for payout payment method tracking and management.
    /// Optional - used for enhanced payout management and payment tracking.
    /// </summary>
    [MaxLength(255)]
    public string? PaymentMethodId { get; set; }
    
    /// <summary>
    /// Navigation property to the PayoutDetails that belong to this payout.
    /// Provides access to payout detail information for payout management.
    /// Used for payout-detail relationship operations.
    /// </summary>
    public virtual ICollection<PayoutDetail> PayoutDetails { get; set; } = new List<PayoutDetail>();
}

/// <summary>
/// Core payout period entity that manages all payout periods in the system.
/// This entity handles payout period creation, management, and processing for provider payouts.
/// It serves as the central hub for payout period management, providing period creation,
/// status tracking, and processing capabilities.
/// </summary>
public class PayoutPeriod : BaseEntity
{
    /// <summary>
    /// Primary key identifier for the payout period.
    /// Uses Guid for better scalability and security in distributed systems.
    /// Unique identifier for each payout period in the system.
    /// </summary>
    [Key]
    public Guid Id { get; set; }
    
    /// <summary>
    /// Name of this payout period.
    /// Used for payout period identification and management.
    /// Required for payout period creation and management.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Start date of this payout period.
    /// Used for payout period definition and management.
    /// Required for payout period creation and management.
    /// </summary>
    public DateTime StartDate { get; set; }
    
    /// <summary>
    /// End date of this payout period.
    /// Used for payout period definition and management.
    /// Required for payout period creation and management.
    /// </summary>
    public DateTime EndDate { get; set; }
    
    /// <summary>
    /// Current status of this payout period.
    /// Used for payout period status tracking and management.
    /// Defaults to Open for new payout periods.
    /// </summary>
    public PayoutPeriodStatus Status { get; set; } = PayoutPeriodStatus.Open;
    
    /// <summary>
    /// Date and time when this payout period was processed.
    /// Used for payout period processing tracking and management.
    /// Set when the payout period is processed by administrators.
    /// </summary>
    public DateTime? ProcessedAt { get; set; }
    
    /// <summary>
    /// ID of the user who processed this payout period.
    /// Used for payout period processing tracking and audit trails.
    /// Optional - used for enhanced payout period management and audit capabilities.
    /// </summary>
    public int? ProcessedByUserId { get; set; }
    
    /// <summary>
    /// Navigation property to the User who processed this payout period.
    /// Provides access to user information for payout period processing management.
    /// Used for user-payout period relationship operations.
    /// </summary>
    public virtual User? ProcessedByUser { get; set; }
    
    /// <summary>
    /// Alias property for CreatedDate from BaseEntity.
    /// Used for backward compatibility and legacy system integration.
    /// </summary>
    public DateTime? CreatedDate { get => CreatedDate; set => CreatedDate = value; }
    
    /// <summary>
    /// Alias property for UpdatedDate from BaseEntity.
    /// Used for backward compatibility and legacy system integration.
    /// </summary>
    public DateTime? UpdatedDate { get => UpdatedDate; set => UpdatedDate = value; }
    
    /// <summary>
    /// Navigation property to the ProviderPayouts that belong to this period.
    /// Provides access to provider payout information for period management.
    /// Used for period-payout relationship operations.
    /// </summary>
    public virtual ICollection<ProviderPayout> ProviderPayouts { get; set; } = new List<ProviderPayout>();
}

/// <summary>
/// Core payout detail entity that manages all payout details in the system.
/// This entity handles payout detail creation, management, and tracking for provider payouts.
/// It serves as the central hub for payout detail management, providing detail creation,
/// fee tracking, and commission calculation capabilities.
/// </summary>
public class PayoutDetail : BaseEntity
{
    /// <summary>
    /// Primary key identifier for the payout detail.
    /// Uses Guid for better scalability and security in distributed systems.
    /// Unique identifier for each payout detail in the system.
    /// </summary>
    [Key]
    public Guid Id { get; set; }
    
    /// <summary>
    /// ID of the payout this detail belongs to.
    /// Used for payout identification and detail management.
    /// Required for payout-detail relationship management.
    /// </summary>
    [Required]
    public Guid PayoutId { get; set; }
    
    /// <summary>
    /// Navigation property to the ProviderPayout this detail belongs to.
    /// Provides access to payout information for detail management.
    /// Used for payout-detail relationship operations.
    /// </summary>
    public virtual ProviderPayout Payout { get; set; } = null!;
    
    /// <summary>
    /// ID of the appointment this detail is for.
    /// Used for appointment identification and detail management.
    /// Required for appointment-detail relationship management.
    /// </summary>
    [Required]
    public Guid AppointmentId { get; set; }
    
    /// <summary>
    /// Navigation property to the Appointment this detail is for.
    /// Provides access to appointment information for detail management.
    /// Used for appointment-detail relationship operations.
    /// </summary>
    public virtual Appointment Appointment { get; set; } = null!;
    
    /// <summary>
    /// Consultation fee for this appointment.
    /// Used for payout calculation and fee tracking.
    /// Required for payout processing and financial management.
    /// </summary>
    [Required]
    [Range(0, 10000)]
    public decimal ConsultationFee { get; set; }
    
    /// <summary>
    /// Platform commission for this appointment.
    /// Used for commission calculation and platform revenue tracking.
    /// Required for payout processing and financial management.
    /// </summary>
    [Required]
    [Range(0, 10000)]
    public decimal PlatformCommission { get; set; }
    
    /// <summary>
    /// Provider earnings for this appointment.
    /// Used for payout calculation and provider earnings tracking.
    /// Required for payout processing and financial management.
    /// </summary>
    [Required]
    [Range(0, 10000)]
    public decimal ProviderEarnings { get; set; }
    
    /// <summary>
    /// Type of consultation for this appointment.
    /// Used for consultation type tracking and payout calculation.
    /// Required for payout processing and analytics.
    /// </summary>
    public ConsultationType ConsultationType { get; set; }
    
    /// <summary>
    /// Alias property for CreatedDate from BaseEntity.
    /// Used for backward compatibility and legacy system integration.
    /// </summary>
    public DateTime? CreatedDate { get => CreatedDate; set => CreatedDate = value; }
    
    /// <summary>
    /// Alias property for UpdatedDate from BaseEntity.
    /// Used for backward compatibility and legacy system integration.
    /// </summary>
    public DateTime? UpdatedDate { get => UpdatedDate; set => UpdatedDate = value; }
    
    /// <summary>
    /// Navigation property to the PayoutDetails that belong to this detail.
    /// Provides access to payout detail information for detail management.
    /// Used for detail-detail relationship operations.
    /// </summary>
    public virtual ICollection<PayoutDetail> PayoutDetails { get; set; } = new List<PayoutDetail>();
}

/// <summary>
/// Enumeration defining the possible statuses of provider payouts in the system.
/// Used for payout status tracking and management.
/// </summary>
public enum PayoutStatus
{
    /// <summary>Payout is pending processing.</summary>
    Pending = 0,
    /// <summary>Payout is under review by administrators.</summary>
    UnderReview = 1,
    /// <summary>Payout has been approved for processing.</summary>
    Approved = 2,
    /// <summary>Payout has been processed and paid.</summary>
    Processed = 3,
    /// <summary>Payout is on hold and cannot be processed.</summary>
    OnHold = 4,
    /// <summary>Payout has been cancelled.</summary>
    Cancelled = 5
}

/// <summary>
/// Enumeration defining the possible statuses of payout periods in the system.
/// Used for payout period status tracking and management.
/// </summary>
public enum PayoutPeriodStatus
{
    /// <summary>Payout period is open for processing.</summary>
    Open = 0,
    /// <summary>Payout period is currently being processed.</summary>
    Processing = 1,
    /// <summary>Payout period has been completed.</summary>
    Completed = 2,
    /// <summary>Payout period has been cancelled.</summary>
    Cancelled = 3
}

/// <summary>
/// Enumeration defining the possible types of consultations in the system.
/// Used for consultation type tracking and payout calculation.
/// </summary>
public enum ConsultationType
{
    /// <summary>One-time consultation.</summary>
    OneTime = 0,
    /// <summary>Subscription-based consultation.</summary>
    Subscription = 1
} 