using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

/// <summary>
/// Core consultation entity that manages all healthcare consultations in the system.
/// This entity handles the complete consultation lifecycle including scheduling, execution, medical records,
/// and follow-up care. It serves as the central hub for consultation management, integrating with users,
/// providers, subscriptions, health assessments, and payment systems. The entity includes comprehensive
/// consultation tracking, medical record management, and treatment planning capabilities.
/// </summary>
public class Consultation : BaseEntity
{
    /// <summary>
    /// Primary key identifier for the consultation.
    /// Uses Guid for better scalability and security in distributed systems.
    /// Unique identifier for each consultation in the system.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Enumeration defining the possible statuses of a consultation.
    /// Used for consultation status tracking and workflow management.
    /// </summary>
    public enum ConsultationStatus
    {
        /// <summary>Scheduled consultation that has not yet started.</summary>
        Scheduled,
        /// <summary>Consultation that is currently in progress.</summary>
        InProgress,
        /// <summary>Consultation that has been completed successfully.</summary>
        Completed,
        /// <summary>Consultation that has been cancelled.</summary>
        Cancelled,
        /// <summary>Consultation where the patient did not show up.</summary>
        NoShow
    }
    
    /// <summary>
    /// Enumeration defining the possible types of consultations.
    /// Used for consultation type management and categorization.
    /// </summary>
    public enum ConsultationType
    {
        /// <summary>Initial consultation for a new patient or condition.</summary>
        Initial,
        /// <summary>Follow-up consultation for ongoing care.</summary>
        FollowUp,
        /// <summary>Emergency consultation for urgent medical needs.</summary>
        Emergency,
        /// <summary>Routine consultation for regular check-ups.</summary>
        Routine
    }
    
    // Foreign keys
    /// <summary>
    /// Foreign key reference to the User who is the patient for this consultation.
    /// Links this consultation to the specific patient user account.
    /// Required for patient-specific consultation management and medical record keeping.
    /// </summary>
    public int UserId { get; set; }
    
    /// <summary>
    /// Navigation property to the User who is the patient for this consultation.
    /// Provides access to patient information for consultation management.
    /// Used for patient-specific consultation operations and medical record management.
    /// </summary>
    public virtual User User { get; set; } = null!;
    
    /// <summary>
    /// Foreign key reference to the Provider who will conduct this consultation.
    /// Links this consultation to the specific healthcare provider.
    /// Required for provider-specific consultation management and scheduling.
    /// </summary>
    public int ProviderId { get; set; }
    
    /// <summary>
    /// Navigation property to the Provider who will conduct this consultation.
    /// Provides access to provider information for consultation management.
    /// Used for provider-specific consultation operations and scheduling.
    /// </summary>
    public virtual Provider Provider { get; set; } = null!;
    
    /// <summary>
    /// Foreign key reference to the Category that this consultation belongs to.
    /// Links this consultation to the specific medical category or specialty.
    /// Required for consultation categorization and provider matching.
    /// </summary>
    public Guid CategoryId { get; set; }
    
    /// <summary>
    /// Navigation property to the Category that this consultation belongs to.
    /// Provides access to category information for consultation management.
    /// Used for consultation categorization and provider matching.
    /// </summary>
    public virtual Category Category { get; set; } = null!;
    
    /// <summary>
    /// Foreign key reference to the Subscription that covers this consultation.
    /// Links this consultation to the specific user subscription for billing and access control.
    /// Optional - used for subscription-based consultation billing and privilege management.
    /// </summary>
    public Guid? SubscriptionId { get; set; }
    
    /// <summary>
    /// Navigation property to the Subscription that covers this consultation.
    /// Provides access to subscription information for consultation billing and access control.
    /// Used for subscription-based consultation operations and billing management.
    /// </summary>
    public virtual Subscription? Subscription { get; set; }
    
    /// <summary>
    /// Foreign key reference to the HealthAssessment that this consultation is based on.
    /// Links this consultation to the specific health assessment.
    /// Optional - used for assessment-based consultation management and medical record keeping.
    /// </summary>
    public Guid? HealthAssessmentId { get; set; }
    
    /// <summary>
    /// Navigation property to the HealthAssessment that this consultation is based on.
    /// Provides access to health assessment information for consultation management.
    /// Used for assessment-based consultation operations and medical record management.
    /// </summary>
    public virtual HealthAssessment? HealthAssessment { get; set; }
    
    // Consultation details
    /// <summary>
    /// Current status of this consultation.
    /// Used for consultation status tracking and workflow management.
    /// Defaults to Scheduled when consultation is first created.
    /// </summary>
    public ConsultationStatus Status { get; set; } = ConsultationStatus.Scheduled;
    
    /// <summary>
    /// Type of this consultation.
    /// Used for consultation type management and categorization.
    /// Defaults to Initial when consultation is first created.
    /// </summary>
    public ConsultationType Type { get; set; } = ConsultationType.Initial;
    
    /// <summary>
    /// Date and time when this consultation is scheduled to take place.
    /// Used for consultation scheduling and calendar management.
    /// Set when the consultation is first scheduled or rescheduled.
    /// </summary>
    public DateTime ScheduledAt { get; set; }
    
    /// <summary>
    /// Date and time when this consultation actually started.
    /// Used for consultation tracking and duration calculations.
    /// Set when the consultation begins (e.g., when video call starts).
    /// </summary>
    public DateTime? StartedAt { get; set; }
    
    /// <summary>
    /// Date and time when this consultation actually ended.
    /// Used for consultation tracking and duration calculations.
    /// Set when the consultation concludes (e.g., when video call ends).
    /// </summary>
    public DateTime? EndedAt { get; set; }
    
    /// <summary>
    /// Expected duration of this consultation in minutes.
    /// Used for consultation scheduling and time management.
    /// Defaults to 30 minutes for standard consultations.
    /// </summary>
    public int DurationMinutes { get; set; } = 30;
    
    /// <summary>
    /// Fee amount for this consultation in the specified currency.
    /// Used for consultation billing and payment processing.
    /// Set when the consultation is created or when pricing is determined.
    /// </summary>
    public decimal Fee { get; set; }
    
    /// <summary>
    /// Meeting URL for video consultation access.
    /// Used for video consultation access and user interface.
    /// Generated when video consultation is set up for this consultation.
    /// </summary>
    [MaxLength(500)]
    public string? MeetingUrl { get; set; }
    
    /// <summary>
    /// Meeting ID for video consultation identification.
    /// Used for video consultation identification and management.
    /// Generated when video consultation is set up for this consultation.
    /// </summary>
    [MaxLength(100)]
    public string? MeetingId { get; set; }
    
    /// <summary>
    /// General notes about this consultation.
    /// Used for consultation documentation and medical record keeping.
    /// Can include consultation context, observations, or additional information.
    /// </summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }
    
    /// <summary>
    /// Provider's diagnosis or assessment of the patient's condition.
    /// Used for medical record keeping and follow-up care.
    /// Set by the provider during or after the consultation.
    /// </summary>
    [MaxLength(1000)]
    public string? Diagnosis { get; set; }
    
    /// <summary>
    /// Treatment plan provided by the provider.
    /// Used for medical record keeping and follow-up care.
    /// Set by the provider during or after the consultation.
    /// </summary>
    [MaxLength(1000)]
    public string? TreatmentPlan { get; set; }
    
    /// <summary>
    /// Prescriptions or medication recommendations provided by the provider.
    /// Used for medical record keeping and follow-up care.
    /// Set by the provider during or after the consultation.
    /// </summary>
    [MaxLength(1000)]
    public string? Prescriptions { get; set; }
    
    /// <summary>
    /// Indicates whether this consultation requires a follow-up.
    /// Used for follow-up consultation scheduling and patient care.
    /// Set by the provider during or after the consultation.
    /// </summary>
    public bool RequiresFollowUp { get; set; }
    
    /// <summary>
    /// Date for follow-up consultation if recommended by the provider.
    /// Used for follow-up consultation scheduling and patient care.
    /// Set by the provider during or after the consultation.
    /// </summary>
    public DateTime? FollowUpDate { get; set; }
    
    /// <summary>
    /// Indicates whether this is a one-time consultation.
    /// Used for consultation type management and billing.
    /// Set when consultation is created or when type is determined.
    /// </summary>
    public bool IsOneTime { get; set; } = false;
    
    /// <summary>
    /// Reason for consultation cancellation if applicable.
    /// Used for cancellation tracking and consultation management.
    /// Set when the consultation is cancelled by either party.
    /// </summary>
    [MaxLength(500)]
    public string? CancellationReason { get; set; }
    
    // Navigation properties
    /// <summary>
    /// Collection of messages associated with this consultation.
    /// Used for consultation communication and message management.
    /// Includes consultation-related messages, notes, and communications.
    /// </summary>
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
    
    /// <summary>
    /// Collection of medication deliveries associated with this consultation.
    /// Used for medication delivery management and prescription fulfillment.
    /// Includes consultation-related medication deliveries and prescriptions.
    /// </summary>
    public virtual ICollection<MedicationDelivery> MedicationDeliveries { get; set; } = new List<MedicationDelivery>();
    
    // Computed Properties
    /// <summary>
    /// Indicates whether this consultation has been completed.
    /// Returns true if consultation status is Completed.
    /// Used for consultation completion checking and workflow management.
    /// </summary>
    [NotMapped]
    public bool IsCompleted => Status == ConsultationStatus.Completed;
    
    /// <summary>
    /// Indicates whether this consultation has been cancelled.
    /// Returns true if consultation status is Cancelled.
    /// Used for consultation cancellation checking and workflow management.
    /// </summary>
    [NotMapped]
    public bool IsCancelled => Status == ConsultationStatus.Cancelled;
    
    /// <summary>
    /// Indicates whether this consultation was a no-show.
    /// Returns true if consultation status is NoShow.
    /// Used for no-show checking and workflow management.
    /// </summary>
    [NotMapped]
    public bool IsNoShow => Status == ConsultationStatus.NoShow;
    
    // Alias properties for backward compatibility
    /// <summary>
    /// Alias property for CreatedDate from BaseEntity.
    /// Used for backward compatibility with existing code.
    /// </summary>
    public DateTime? CreatedDate { get => CreatedDate; set => CreatedDate = value; }
    
    /// <summary>
    /// Alias property for UpdatedDate from BaseEntity.
    /// Used for backward compatibility with existing code.
    /// </summary>
    public DateTime? UpdatedDate { get => UpdatedDate; set => UpdatedDate = value; }
    
    /// <summary>
    /// Calculates the actual duration of this consultation.
    /// Returns the time difference between StartedAt and EndedAt if both are available.
    /// Used for consultation duration tracking and billing calculations.
    /// </summary>
    [NotMapped]
    public TimeSpan? ActualDuration => StartedAt.HasValue && EndedAt.HasValue 
        ? EndedAt.Value - StartedAt.Value 
        : null;
} 