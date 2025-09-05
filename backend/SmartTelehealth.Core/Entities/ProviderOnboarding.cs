using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

/// <summary>
/// Core provider onboarding entity that manages all provider onboarding processes in the system.
/// This entity handles provider registration, credential verification, and approval workflows.
/// It serves as the central hub for provider onboarding management, providing comprehensive
/// provider registration, document verification, and approval tracking capabilities.
/// </summary>
public class ProviderOnboarding : BaseEntity
{
    /// <summary>
    /// Primary key identifier for the provider onboarding record.
    /// Uses Guid for better scalability and security in distributed systems.
    /// Unique identifier for each provider onboarding record in the system.
    /// </summary>
    [Key]
    public Guid Id { get; set; }
    
    /// <summary>
    /// Foreign key reference to the User who is being onboarded as a provider.
    /// Links this onboarding record to the specific user account.
    /// Required for user-onboarding relationship management.
    /// </summary>
    [Required]
    public int UserId { get; set; }
    
    /// <summary>
    /// Navigation property to the User who is being onboarded as a provider.
    /// Provides access to user information for onboarding management.
    /// Used for user-onboarding relationship operations.
    /// </summary>
    public virtual User User { get; set; } = null!;
    
    /// <summary>
    /// First name of the provider being onboarded.
    /// Used for provider identification and display.
    /// Required for provider onboarding and management.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    /// <summary>
    /// Last name of the provider being onboarded.
    /// Used for provider identification and display.
    /// Required for provider onboarding and management.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    /// <summary>
    /// Email address of the provider being onboarded.
    /// Used for provider communication and identification.
    /// Required for provider onboarding and management.
    /// </summary>
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Phone number of the provider being onboarded.
    /// Used for provider communication and identification.
    /// Required for provider onboarding and management.
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Medical specialty of the provider being onboarded.
    /// Used for provider classification and management.
    /// Required for provider onboarding and specialty assignment.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Specialty { get; set; } = string.Empty;
    
    /// <summary>
    /// Sub-specialty of the provider being onboarded.
    /// Used for provider classification and management.
    /// Optional - used for enhanced provider classification.
    /// </summary>
    [MaxLength(500)]
    public string? SubSpecialty { get; set; }
    
    /// <summary>
    /// Medical license number of the provider being onboarded.
    /// Used for provider credential verification and management.
    /// Required for provider onboarding and license validation.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string LicenseNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// State where the medical license was issued.
    /// Used for provider credential verification and management.
    /// Required for provider onboarding and license validation.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string LicenseState { get; set; } = string.Empty;
    
    /// <summary>
    /// National Provider Identifier (NPI) number of the provider.
    /// Used for provider credential verification and management.
    /// Optional - used for enhanced provider identification.
    /// </summary>
    [MaxLength(50)]
    public string? NPINumber { get; set; }
    
    /// <summary>
    /// Drug Enforcement Administration (DEA) number of the provider.
    /// Used for provider credential verification and management.
    /// Optional - used for controlled substance prescription authority.
    /// </summary>
    [MaxLength(50)]
    public string? DEANumber { get; set; }
    
    /// <summary>
    /// Educational background of the provider being onboarded.
    /// Used for provider credential verification and management.
    /// Optional - used for enhanced provider qualification tracking.
    /// </summary>
    [MaxLength(500)]
    public string? Education { get; set; }
    
    /// <summary>
    /// Work history of the provider being onboarded.
    /// Used for provider credential verification and management.
    /// Optional - used for enhanced provider qualification tracking.
    /// </summary>
    [MaxLength(1000)]
    public string? WorkHistory { get; set; }
    
    /// <summary>
    /// Malpractice insurance information of the provider being onboarded.
    /// Used for provider credential verification and management.
    /// Optional - used for enhanced provider qualification tracking.
    /// </summary>
    [MaxLength(500)]
    public string? MalpracticeInsurance { get; set; }
    
    /// <summary>
    /// Professional biography of the provider being onboarded.
    /// Used for provider profile and user communication.
    /// Optional - used for enhanced provider profile management.
    /// </summary>
    [MaxLength(1000)]
    public string? Bio { get; set; }
    
    /// <summary>
    /// URL or path to the provider's profile photo.
    /// Used for provider profile display and management.
    /// Optional - used for enhanced provider profile management.
    /// </summary>
    [MaxLength(255)]
    public string? ProfilePhotoUrl { get; set; }
    
    /// <summary>
    /// URL or path to the provider's government ID document.
    /// Used for provider credential verification and management.
    /// Optional - used for enhanced provider verification.
    /// </summary>
    [MaxLength(255)]
    public string? GovernmentIdUrl { get; set; }
    
    /// <summary>
    /// URL or path to the provider's license document.
    /// Used for provider credential verification and management.
    /// Optional - used for enhanced provider verification.
    /// </summary>
    [MaxLength(255)]
    public string? LicenseDocumentUrl { get; set; }
    
    /// <summary>
    /// URL or path to the provider's certification document.
    /// Used for provider credential verification and management.
    /// Optional - used for enhanced provider verification.
    /// </summary>
    [MaxLength(255)]
    public string? CertificationDocumentUrl { get; set; }
    
    /// <summary>
    /// URL or path to the provider's malpractice insurance document.
    /// Used for provider credential verification and management.
    /// Optional - used for enhanced provider verification.
    /// </summary>
    [MaxLength(255)]
    public string? MalpracticeInsuranceUrl { get; set; }
    
    /// <summary>
    /// Current status of the provider onboarding process.
    /// Used for onboarding status tracking and management.
    /// Defaults to Pending when onboarding record is created.
    /// </summary>
    public OnboardingStatus Status { get; set; } = OnboardingStatus.Pending;
    
    /// <summary>
    /// Administrative remarks or notes about the provider onboarding.
    /// Used for onboarding documentation and management.
    /// Optional - used for enhanced onboarding management and communication.
    /// </summary>
    [MaxLength(1000)]
    public string? AdminRemarks { get; set; }
    
    /// <summary>
    /// Date and time when the provider onboarding was submitted.
    /// Used for onboarding timing tracking and management.
    /// Set when the provider submits their onboarding information.
    /// </summary>
    public DateTime? SubmittedAt { get; set; }
    
    /// <summary>
    /// Date and time when the provider onboarding was reviewed.
    /// Used for onboarding timing tracking and management.
    /// Set when the onboarding is reviewed by an administrator.
    /// </summary>
    public DateTime? ReviewedAt { get; set; }
    
    /// <summary>
    /// Foreign key reference to the User who reviewed this onboarding.
    /// Links this onboarding to the specific administrator who reviewed it.
    /// Optional - used for reviewer tracking and management.
    /// </summary>
    public int? ReviewedByUserId { get; set; }
    
    /// <summary>
    /// Navigation property to the User who reviewed this onboarding.
    /// Provides access to reviewer information for onboarding management.
    /// Used for reviewer-onboarding relationship operations.
    /// </summary>
    public virtual User? ReviewedByUser { get; set; }
    
    // Alias properties for backward compatibility
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
}

/// <summary>
/// Enumeration defining the possible statuses of provider onboarding.
/// Used for onboarding status tracking and management.
/// </summary>
public enum OnboardingStatus
{
    /// <summary>Onboarding is pending submission or review.</summary>
    Pending = 0,
    /// <summary>Onboarding is under administrative review.</summary>
    UnderReview = 1,
    /// <summary>Onboarding has been approved by administrators.</summary>
    Approved = 2,
    /// <summary>Onboarding has been rejected by administrators.</summary>
    Rejected = 3,
    /// <summary>Onboarding requires additional information from the provider.</summary>
    RequiresMoreInfo = 4
} 