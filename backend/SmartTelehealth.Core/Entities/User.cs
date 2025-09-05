using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

/// <summary>
/// Core user entity that extends ASP.NET Core Identity to provide comprehensive user management for the telehealth platform.
/// This entity handles all user-related data including personal information, authentication, Stripe integration, and role-based access control.
/// It serves as the central hub for all user interactions within the system including subscriptions, consultations, appointments, and messaging.
/// </summary>
public class User : IdentityUser<int>
{
    /// <summary>
    /// Primary key identifier for the user.
    /// Inherited from IdentityUser<int> and serves as the unique identifier across the entire system.
    /// Used for all foreign key relationships and user identification.
    /// </summary>
    [Key]
    public override int Id { get; set; }

    /// <summary>
    /// User's first name for personal identification and display purposes.
    /// Required field for user registration and profile management.
    /// Used in greetings, notifications, and user interface display.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    /// <summary>
    /// User's last name for personal identification and display purposes.
    /// Required field for user registration and profile management.
    /// Used in greetings, notifications, and user interface display.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    /// <summary>
    /// User's phone number for contact and verification purposes.
    /// Overrides IdentityUser's PhoneNumber to allow null values.
    /// Used for SMS notifications, two-factor authentication, and emergency contact.
    /// </summary>
    public new string? PhoneNumber { get; set; }
    
    /// <summary>
    /// User's date of birth for age verification and medical record management.
    /// Required for healthcare compliance and age-appropriate service delivery.
    /// Used for calculating age, determining service eligibility, and medical record keeping.
    /// </summary>
    public DateTime DateOfBirth { get; set; }
    
    /// <summary>
    /// User's gender for demographic tracking and personalized healthcare services.
    /// Optional field that can be used for health assessments and treatment recommendations.
    /// Stored as string to accommodate various gender identities and preferences.
    /// </summary>
    [MaxLength(10)]
    public string? Gender { get; set; }
    
    /// <summary>
    /// User's complete address for location-based services and emergency contact.
    /// Used for medication delivery, emergency services, and location-based provider matching.
    /// Can be used for compliance with healthcare regulations requiring patient location.
    /// </summary>
    [MaxLength(500)]
    public string? Address { get; set; }
    
    /// <summary>
    /// User's city for location-based services and demographic analysis.
    /// Used for local provider matching, regional service availability, and analytics.
    /// Helps in determining service coverage and local healthcare regulations.
    /// </summary>
    [MaxLength(100)]
    public string? City { get; set; }
    
    /// <summary>
    /// User's state/province for location-based services and regulatory compliance.
    /// Used for state-specific healthcare regulations, provider licensing, and service availability.
    /// Important for compliance with state healthcare laws and regulations.
    /// </summary>
    [MaxLength(50)]
    public string? State { get; set; }
    
    /// <summary>
    /// User's postal/zip code for precise location-based services and delivery.
    /// Used for medication delivery, local provider matching, and service area determination.
    /// Critical for accurate delivery scheduling and local service availability.
    /// </summary>
    [MaxLength(20)]
    public string? ZipCode { get; set; }
    
    /// <summary>
    /// User's country for international compliance and service availability.
    /// Used for determining applicable healthcare regulations and service coverage.
    /// Important for international users and cross-border healthcare services.
    /// </summary>
    [MaxLength(100)]
    public string? Country { get; set; }
    
    /// <summary>
    /// Name of emergency contact person for critical situations and medical emergencies.
    /// Required for healthcare compliance and emergency response procedures.
    /// Used when user is unable to communicate or make decisions during medical emergencies.
    /// </summary>
    [MaxLength(100)]
    public string? EmergencyContactName { get; set; }
    
    /// <summary>
    /// Phone number of emergency contact for critical situations and medical emergencies.
    /// Required for healthcare compliance and emergency response procedures.
    /// Used for immediate contact during medical emergencies or critical situations.
    /// </summary>
    [MaxLength(20)]
    public string? EmergencyContactPhone { get; set; }
    
    /// <summary>
    /// Legacy emergency contact field for backward compatibility with existing services.
    /// Maintained to ensure compatibility with older system components.
    /// Can be used as fallback when EmergencyContactName is not available.
    /// </summary>
    public string? EmergencyContact { get; set; }
    
    /// <summary>
    /// Legacy emergency phone field for backward compatibility with existing services.
    /// Maintained to ensure compatibility with older system components.
    /// Can be used as fallback when EmergencyContactPhone is not available.
    /// </summary>
    public string? EmergencyPhone { get; set; }
    
    /// <summary>
    /// JWT refresh token for maintaining user authentication sessions.
    /// Used for secure token refresh without requiring user to log in again.
    /// Stored securely and used for extending authentication sessions.
    /// </summary>
    [MaxLength(500)]
    public string? RefreshToken { get; set; }
    
    /// <summary>
    /// Expiration date and time for the refresh token.
    /// Used to determine when refresh token expires and needs renewal.
    /// Security measure to limit token lifetime and prevent unauthorized access.
    /// </summary>
    public DateTime? RefreshTokenExpiry { get; set; }
    
    /// <summary>
    /// Stripe customer ID for payment processing and subscription management.
    /// Links user to their Stripe customer record for payment operations.
    /// Used for creating subscriptions, processing payments, and managing billing.
    /// </summary>
    [MaxLength(100)]
    public string? StripeCustomerId { get; set; }
    
    /// <summary>
    /// Professional license number for healthcare providers.
    /// Required for provider verification and regulatory compliance.
    /// Used for provider credential verification and regulatory reporting.
    /// </summary>
    [MaxLength(100)]
    public string? LicenseNumber { get; set; }
    
    /// <summary>
    /// Type of user (Client, Provider, Admin) for role-based access control.
    /// Determines user permissions and available features in the system.
    /// Used for UI customization, feature access, and business logic routing.
    /// </summary>
    public string UserType { get; set; } = string.Empty;
    
    /// <summary>
    /// URL or path to user's profile picture for personalization and identification.
    /// Used in user interface, messaging, and profile displays.
    /// Can be stored locally or in cloud storage (AWS S3, Azure Blob).
    /// </summary>
    public string? ProfilePicture { get; set; }
    
    /// <summary>
    /// Indicates whether user's email address has been verified.
    /// Required for account security and communication reliability.
    /// Used to determine if user can receive important notifications and communications.
    /// </summary>
    public bool IsEmailVerified { get; set; } = false;
    
    /// <summary>
    /// Indicates whether user's phone number has been verified.
    /// Required for SMS notifications and two-factor authentication.
    /// Used to determine if user can receive SMS-based communications and security codes.
    /// </summary>
    public bool IsPhoneVerified { get; set; } = false;
    
    /// <summary>
    /// Timestamp of user's last login for security monitoring and analytics.
    /// Used for security audits, user activity tracking, and session management.
    /// Helps identify inactive accounts and potential security issues.
    /// </summary>
    public DateTime? LastLoginAt { get; set; }
    
    /// <summary>
    /// Alias property for PhoneNumber to maintain backward compatibility.
    /// Provides consistent access to phone number across different service layers.
    /// Used to ensure compatibility with existing code that expects 'Phone' property.
    /// </summary>
    public string? Phone { get => PhoneNumber; set => PhoneNumber = value; }
    
    /// <summary>
    /// Token for password reset functionality.
    /// Generated when user requests password reset and used for secure password reset process.
    /// Expires after a set time period for security purposes.
    /// </summary>
    public string? PasswordResetToken { get; set; }
    
    /// <summary>
    /// Expiration date and time for the password reset token.
    /// Used to determine when password reset token expires and becomes invalid.
    /// Security measure to limit password reset window and prevent unauthorized access.
    /// </summary>
    public DateTime? ResetTokenExpires { get; set; }
    
    /// <summary>
    /// Alias property for ResetTokenExpires to maintain backward compatibility.
    /// Provides consistent access to reset token expiration across different service layers.
    /// Used to ensure compatibility with existing code that expects 'PasswordResetTokenExpires' property.
    /// </summary>
    public DateTime? PasswordResetTokenExpires => ResetTokenExpires;
    
    /// <summary>
    /// User's notification preferences stored as JSON string.
    /// Contains settings for email, SMS, push notifications, and notification types.
    /// Used to customize notification delivery based on user preferences.
    /// </summary>
    public string? NotificationPreferences { get; set; }
    
    /// <summary>
    /// User's preferred language for internationalization and localization.
    /// Used to display user interface and content in user's preferred language.
    /// Supports multi-language healthcare content and user experience.
    /// </summary>
    public string? LanguagePreference { get; set; }
    
    /// <summary>
    /// User's preferred timezone for scheduling and time display.
    /// Used for appointment scheduling, medication reminders, and time-sensitive notifications.
    /// Ensures all time-based features work correctly for users in different timezones.
    /// </summary>
    public string? TimeZonePreference { get; set; }
    
    /// <summary>
    /// Foreign key reference to UserRole for role-based access control.
    /// Determines user permissions, available features, and system access levels.
    /// Used for authorization, UI customization, and business logic routing.
    /// </summary>
    public int UserRoleId { get; set; }
    
    /// <summary>
    /// Navigation property to UserRole entity.
    /// Provides access to role information including permissions and role details.
    /// Used for role-based authorization and user interface customization.
    /// </summary>
    public virtual UserRole UserRole { get; set; } = null!;
    
    // Audit properties (since User can't inherit from BaseEntity due to IdentityUser<int>)
    
    /// <summary>
    /// Indicates whether the user account is currently active.
    /// Used for soft activation/deactivation without deleting user records.
    /// Inactive users cannot log in or access system features.
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Indicates whether the user account has been soft deleted.
    /// Used for soft delete functionality to maintain data integrity and audit trails.
    /// Deleted users are marked as deleted rather than physically removed from database.
    /// </summary>
    public bool IsDeleted { get; set; } = false;
    
    /// <summary>
    /// Foreign key reference to the User who created this user account.
    /// Used for audit trail and tracking who created the user account.
    /// Typically set to admin user ID for system-created accounts.
    /// </summary>
    public int? CreatedBy { get; set; }
    
    /// <summary>
    /// Timestamp when the user account was created.
    /// Used for audit trail and tracking when user accounts were created.
    /// Automatically set when user account is first saved to database.
    /// </summary>
    public DateTime? CreatedDate { get; set; }
    
    /// <summary>
    /// Foreign key reference to the User who last updated this user account.
    /// Used for audit trail and tracking who made the most recent changes.
    /// Updated every time the user account is modified.
    /// </summary>
    public int? UpdatedBy { get; set; }
    
    /// <summary>
    /// Timestamp when the user account was last updated.
    /// Used for audit trail and tracking when user accounts were last modified.
    /// Automatically updated when user account is saved to database.
    /// </summary>
    public DateTime? UpdatedDate { get; set; }
    
    /// <summary>
    /// Foreign key reference to the User who deleted this user account.
    /// Used for audit trail and tracking who performed the soft delete operation.
    /// Only set when IsDeleted is changed to true.
    /// </summary>
    public int? DeletedBy { get; set; }
    
    /// <summary>
    /// Timestamp when the user account was soft deleted.
    /// Used for audit trail and tracking when user accounts were deleted.
    /// Set when IsDeleted is changed to true.
    /// </summary>
    public DateTime? DeletedDate { get; set; }
    
    /// <summary>
    /// Navigation property to the User who created this user account.
    /// Not mapped to database - used for eager loading user details when needed.
    /// Provides access to creator's information for audit and display purposes.
    /// </summary>
    [NotMapped]
    [ForeignKey(nameof(CreatedBy))]
    public virtual User? CreatedByUser { get; set; }
    
    /// <summary>
    /// Navigation property to the User who last updated this user account.
    /// Not mapped to database - used for eager loading user details when needed.
    /// Provides access to updater's information for audit and display purposes.
    /// </summary>
    [NotMapped]
    [ForeignKey(nameof(UpdatedBy))]
    public virtual User? UpdatedByUser { get; set; }
    
    /// <summary>
    /// Navigation property to the User who deleted this user account.
    /// Not mapped to database - used for eager loading user details when needed.
    /// Provides access to deleter's information for audit purposes.
    /// </summary>
    [NotMapped]
    [ForeignKey(nameof(DeletedBy))]
    public virtual User? DeletedByUser { get; set; }
    
    // Navigation properties - These establish relationships with other entities
    
    /// <summary>
    /// Collection of all subscriptions associated with this user.
    /// Represents the user's subscription history and current active subscriptions.
    /// Used for subscription management, billing, and service access control.
    /// </summary>
    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    
    /// <summary>
    /// Collection of all consultations this user has participated in.
    /// Represents the user's consultation history as either patient or provider.
    /// Used for consultation management, history tracking, and medical record keeping.
    /// </summary>
    public virtual ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();
    
    /// <summary>
    /// Collection of all health assessments completed by this user.
    /// Represents the user's health assessment history and results.
    /// Used for health tracking, assessment management, and medical record keeping.
    /// </summary>
    public virtual ICollection<HealthAssessment> HealthAssessments { get; set; } = new List<HealthAssessment>();
    
    /// <summary>
    /// Collection of all messages sent or received by this user.
    /// Represents the user's messaging history and communication records.
    /// Used for messaging functionality, communication tracking, and support.
    /// </summary>
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
    
    /// <summary>
    /// Collection of all appointments where this user is the patient.
    /// Represents the user's appointment history as a patient.
    /// Used for appointment management, scheduling, and medical record keeping.
    /// </summary>
    public virtual ICollection<Appointment> PatientAppointments { get; set; } = new List<Appointment>();
    
    /// <summary>
    /// Collection of all appointment participations for this user.
    /// Represents the user's participation in appointments (as patient, provider, or observer).
    /// Used for appointment management and participation tracking.
    /// </summary>
    public virtual ICollection<AppointmentParticipant> AppointmentParticipants { get; set; } = new List<AppointmentParticipant>();
    
    /// <summary>
    /// Collection of all payment logs associated with this user.
    /// Represents the user's payment history and transaction records.
    /// Used for payment tracking, billing management, and financial records.
    /// </summary>
    public virtual ICollection<AppointmentPaymentLog> PaymentLogs { get; set; } = new List<AppointmentPaymentLog>();
    
    /// <summary>
    /// Collection of all documents uploaded by this user.
    /// Represents the user's document upload history and file management.
    /// Used for document management, file storage, and medical record keeping.
    /// </summary>
    public virtual ICollection<AppointmentDocument> UploadedDocuments { get; set; } = new List<AppointmentDocument>();
    
    /// <summary>
    /// Collection of all appointment events associated with this user.
    /// Represents the user's appointment event history and activity tracking.
    /// Used for appointment tracking, event logging, and audit trails.
    /// </summary>
    public virtual ICollection<AppointmentEvent> AppointmentEvents { get; set; } = new List<AppointmentEvent>();
    
    /// <summary>
    /// Computed property that returns the user's full name.
    /// Combines FirstName and LastName for display purposes.
    /// Used in user interface, notifications, and user identification.
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();
    
    /// <summary>
    /// Computed property that returns the user's role name.
    /// Provides easy access to role name for display and authorization purposes.
    /// Used in user interface, authorization logic, and role-based feature access.
    /// </summary>
    public string RoleName => UserRole?.Name ?? "Unknown";
} 