using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

/// <summary>
/// Core provider entity that manages all healthcare providers in the system.
/// This entity handles provider information, credentials, availability, and professional details.
/// It serves as the central hub for provider management, integrating with consultations, messages,
/// categories, and appointment scheduling. The entity includes comprehensive provider tracking,
/// credential management, and availability scheduling capabilities.
/// </summary>
public class Provider : BaseEntity
{
    /// <summary>
    /// Primary key identifier for the provider.
    /// Uses integer for provider identification and management.
    /// Unique identifier for each provider in the system.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Provider's first name.
    /// Used for provider identification and user interface display.
    /// Required for provider registration and management.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    /// <summary>
    /// Provider's last name.
    /// Used for provider identification and user interface display.
    /// Required for provider registration and management.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    /// <summary>
    /// Provider's email address.
    /// Used for provider communication and account management.
    /// Required for provider registration and authentication.
    /// </summary>
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Provider's phone number.
    /// Used for provider communication and contact management.
    /// Optional - set when provider provides phone number.
    /// </summary>
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }
    
    /// <summary>
    /// Provider's medical license number.
    /// Used for provider credential verification and compliance.
    /// Required for provider registration and credential management.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string LicenseNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// State where the provider is licensed to practice.
    /// Used for provider credential verification and compliance.
    /// Required for provider registration and credential management.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string State { get; set; } = string.Empty;
    
    /// <summary>
    /// Provider's medical specialty or area of expertise.
    /// Used for provider categorization and patient matching.
    /// Required for provider registration and specialty management.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Specialty { get; set; } = string.Empty;
    
    /// <summary>
    /// Provider's professional biography or description.
    /// Used for provider profile display and patient information.
    /// Optional - set when provider provides biography information.
    /// </summary>
    [MaxLength(500)]
    public string? Bio { get; set; }
    
    /// <summary>
    /// Provider's educational background and qualifications.
    /// Used for provider credential display and verification.
    /// Optional - set when provider provides education information.
    /// </summary>
    [MaxLength(100)]
    public string? Education { get; set; }
    
    /// <summary>
    /// Provider's professional certifications and credentials.
    /// Used for provider credential display and verification.
    /// Optional - set when provider provides certification information.
    /// </summary>
    [MaxLength(500)]
    public string? Certifications { get; set; }
    
    /// <summary>
    /// Indicates whether this provider is currently available for consultations.
    /// Used for provider availability management and appointment scheduling.
    /// Defaults to true when provider is registered.
    /// </summary>
    public bool IsAvailable { get; set; } = true;
    
    /// <summary>
    /// Time when this provider is available to start consultations.
    /// Used for provider availability management and appointment scheduling.
    /// Optional - set when provider specifies availability hours.
    /// </summary>
    public TimeSpan? AvailableFrom { get; set; }
    
    /// <summary>
    /// Time when this provider stops being available for consultations.
    /// Used for provider availability management and appointment scheduling.
    /// Optional - set when provider specifies availability hours.
    /// </summary>
    public TimeSpan? AvailableTo { get; set; }
    
    /// <summary>
    /// Fee amount for consultations with this provider.
    /// Used for consultation pricing and billing management.
    /// Set when provider is registered or when pricing is determined.
    /// </summary>
    public decimal ConsultationFee { get; set; }
    
    // Navigation properties
    /// <summary>
    /// Collection of consultations conducted by this provider.
    /// Used for consultation management and provider performance tracking.
    /// Includes all consultations where this provider is the assigned provider.
    /// </summary>
    public virtual ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();
    
    /// <summary>
    /// Collection of messages sent by this provider.
    /// Used for message management and provider communication tracking.
    /// Includes all messages where this provider is the sender.
    /// </summary>
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
    
    /// <summary>
    /// Collection of categories that this provider belongs to.
    /// Used for provider categorization and category management.
    /// Includes all categories where this provider is assigned.
    /// </summary>
    public virtual ICollection<ProviderCategory> ProviderCategories { get; set; } = new List<ProviderCategory>();
    
    /// <summary>
    /// Computed property that returns the provider's full name.
    /// Combines FirstName and LastName with proper trimming.
    /// Used for display purposes and user interface.
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();
} 