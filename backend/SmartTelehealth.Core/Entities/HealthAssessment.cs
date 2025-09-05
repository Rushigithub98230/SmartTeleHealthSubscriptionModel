using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

/// <summary>
/// Core health assessment entity that manages all health assessments in the system.
/// This entity handles health assessment data collection, evaluation, and provider review.
/// It serves as the central hub for health assessment management, integrating with users,
/// providers, categories, consultations, and subscriptions. The entity includes comprehensive
/// health data tracking, assessment status management, and eligibility determination capabilities.
/// </summary>
public class HealthAssessment : BaseEntity
{
    /// <summary>
    /// Primary key identifier for the health assessment.
    /// Uses Guid for better scalability and security in distributed systems.
    /// Unique identifier for each health assessment in the system.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Enumeration defining the possible statuses of a health assessment.
    /// Used for assessment status tracking and workflow management.
    /// </summary>
    public enum AssessmentStatus
    {
        /// <summary>Assessment is pending and not yet started.</summary>
        Pending,
        /// <summary>Assessment is currently in progress.</summary>
        InProgress,
        /// <summary>Assessment has been completed by the user.</summary>
        Completed,
        /// <summary>Assessment has been reviewed by a provider.</summary>
        Reviewed,
        /// <summary>Assessment has been cancelled.</summary>
        Cancelled
    }
    
    // Foreign keys
    /// <summary>
    /// Foreign key reference to the User who is taking this health assessment.
    /// Links this assessment to the specific user account.
    /// Required for user-specific assessment management and data collection.
    /// </summary>
    public int UserId { get; set; }
    
    /// <summary>
    /// Navigation property to the User who is taking this health assessment.
    /// Provides access to user information for assessment management.
    /// Used for user-specific assessment operations and data collection.
    /// </summary>
    public virtual User User { get; set; } = null!;
    
    /// <summary>
    /// Foreign key reference to the Category that this assessment belongs to.
    /// Links this assessment to the specific health category or specialty.
    /// Required for assessment categorization and provider matching.
    /// </summary>
    public Guid CategoryId { get; set; }
    
    /// <summary>
    /// Navigation property to the Category that this assessment belongs to.
    /// Provides access to category information for assessment management.
    /// Used for assessment categorization and provider matching.
    /// </summary>
    public virtual Category Category { get; set; } = null!;
    
    /// <summary>
    /// Foreign key reference to the Provider who reviewed this assessment.
    /// Links this assessment to the specific healthcare provider.
    /// Optional - used for provider review and assessment evaluation.
    /// </summary>
    public int? ProviderId { get; set; }
    
    /// <summary>
    /// Navigation property to the Provider who reviewed this assessment.
    /// Provides access to provider information for assessment management.
    /// Used for provider review and assessment evaluation operations.
    /// </summary>
    public virtual Provider? Provider { get; set; }
    
    // Assessment details
    /// <summary>
    /// Current status of this health assessment.
    /// Used for assessment status tracking and workflow management.
    /// Defaults to InProgress when assessment is first created.
    /// </summary>
    public AssessmentStatus Status { get; set; } = AssessmentStatus.InProgress;
    
    /// <summary>
    /// Patient's reported symptoms and health concerns.
    /// Used for health data collection and assessment evaluation.
    /// Set by the user during assessment completion.
    /// </summary>
    [MaxLength(1000)]
    public string? Symptoms { get; set; }
    
    /// <summary>
    /// Patient's medical history and past health conditions.
    /// Used for health data collection and assessment evaluation.
    /// Set by the user during assessment completion.
    /// </summary>
    [MaxLength(1000)]
    public string? MedicalHistory { get; set; }
    
    /// <summary>
    /// Patient's current medications and treatments.
    /// Used for health data collection and assessment evaluation.
    /// Set by the user during assessment completion.
    /// </summary>
    [MaxLength(1000)]
    public string? CurrentMedications { get; set; }
    
    /// <summary>
    /// Patient's known allergies and adverse reactions.
    /// Used for health data collection and safety assessment.
    /// Set by the user during assessment completion.
    /// </summary>
    [MaxLength(1000)]
    public string? Allergies { get; set; }
    
    /// <summary>
    /// Patient's lifestyle factors and habits.
    /// Used for health data collection and assessment evaluation.
    /// Set by the user during assessment completion.
    /// </summary>
    [MaxLength(1000)]
    public string? LifestyleFactors { get; set; }
    
    /// <summary>
    /// Patient's family medical history.
    /// Used for health data collection and assessment evaluation.
    /// Set by the user during assessment completion.
    /// </summary>
    [MaxLength(1000)]
    public string? FamilyHistory { get; set; }
    
    /// <summary>
    /// Date and time when this assessment was completed by the user.
    /// Used for assessment completion tracking and workflow management.
    /// Set when the user completes the assessment.
    /// </summary>
    public DateTime? CompletedAt { get; set; }
    
    /// <summary>
    /// Date and time when this assessment was reviewed by a provider.
    /// Used for provider review tracking and workflow management.
    /// Set when a provider reviews the assessment.
    /// </summary>
    public DateTime? ReviewedAt { get; set; }
    
    /// <summary>
    /// Notes and comments from the provider who reviewed this assessment.
    /// Used for provider feedback and assessment documentation.
    /// Set by the provider during assessment review.
    /// </summary>
    [MaxLength(1000)]
    public string? ProviderNotes { get; set; }
    
    /// <summary>
    /// Reason for assessment rejection if applicable.
    /// Used for rejection tracking and assessment management.
    /// Set when the assessment is rejected by a provider.
    /// </summary>
    [MaxLength(500)]
    public string? RejectionReason { get; set; }
    
    /// <summary>
    /// Indicates whether the patient is eligible for treatment based on this assessment.
    /// Used for treatment eligibility determination and care planning.
    /// Set by the provider during assessment review.
    /// </summary>
    public bool IsEligibleForTreatment { get; set; }
    
    /// <summary>
    /// Indicates whether this assessment requires follow-up care.
    /// Used for follow-up care planning and patient management.
    /// Set by the provider during assessment review.
    /// </summary>
    public bool RequiresFollowUp { get; set; }
    
    // Navigation properties
    /// <summary>
    /// Collection of consultations based on this health assessment.
    /// Used for consultation management and assessment-based care.
    /// Includes all consultations that reference this assessment.
    /// </summary>
    public virtual ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();
    
    /// <summary>
    /// Collection of subscriptions based on this health assessment.
    /// Used for subscription management and assessment-based access.
    /// Includes all subscriptions that reference this assessment.
    /// </summary>
    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
} 