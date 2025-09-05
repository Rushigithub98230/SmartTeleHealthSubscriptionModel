using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

/// <summary>
/// Core appointment entity that manages all healthcare appointments in the system.
/// This entity handles the complete appointment lifecycle including scheduling, consultation, payment, and video calls.
/// It serves as the central hub for appointment management, integrating with providers, patients, subscriptions,
/// and payment systems. The entity includes comprehensive appointment tracking, medical record management,
/// video call integration, and payment processing with Stripe.
/// </summary>
public class Appointment : BaseEntity
{
    /// <summary>
    /// Primary key identifier for the appointment.
    /// Uses Guid for better scalability and security in distributed systems.
    /// Unique identifier for each appointment in the system.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    // Foreign keys
    /// <summary>
    /// Foreign key reference to the User who is the patient for this appointment.
    /// Links this appointment to the specific patient user account.
    /// Required for patient-specific appointment management and medical record keeping.
    /// </summary>
    public int PatientId { get; set; }
    
    /// <summary>
    /// Navigation property to the User who is the patient for this appointment.
    /// Provides access to patient information for appointment management.
    /// Used for patient-specific appointment operations and medical record management.
    /// </summary>
    public virtual User Patient { get; set; } = null!;

    /// <summary>
    /// Foreign key reference to the Provider who will conduct this appointment.
    /// Links this appointment to the specific healthcare provider.
    /// Required for provider-specific appointment management and scheduling.
    /// </summary>
    public int ProviderId { get; set; }
    
    /// <summary>
    /// Navigation property to the Provider who will conduct this appointment.
    /// Provides access to provider information for appointment management.
    /// Used for provider-specific appointment operations and scheduling.
    /// </summary>
    public virtual Provider Provider { get; set; } = null!;

    /// <summary>
    /// Foreign key reference to the Category that this appointment belongs to.
    /// Links this appointment to the specific medical category or specialty.
    /// Required for appointment categorization and provider matching.
    /// </summary>
    public Guid CategoryId { get; set; }
    
    /// <summary>
    /// Navigation property to the Category that this appointment belongs to.
    /// Provides access to category information for appointment management.
    /// Used for appointment categorization and provider matching.
    /// </summary>
    public virtual Category Category { get; set; } = null!;

    /// <summary>
    /// Foreign key reference to the Subscription that covers this appointment.
    /// Links this appointment to the specific user subscription for billing and access control.
    /// Optional - used for subscription-based appointment billing and privilege management.
    /// </summary>
    public Guid? SubscriptionId { get; set; }
    
    /// <summary>
    /// Navigation property to the Subscription that covers this appointment.
    /// Provides access to subscription information for appointment billing and access control.
    /// Used for subscription-based appointment operations and billing management.
    /// </summary>
    public virtual Subscription? Subscription { get; set; }

    /// <summary>
    /// Foreign key reference to the Consultation that this appointment is part of.
    /// Links this appointment to the specific consultation session.
    /// Optional - used for consultation-based appointment management and medical record keeping.
    /// </summary>
    public Guid? ConsultationId { get; set; }
    
    /// <summary>
    /// Navigation property to the Consultation that this appointment is part of.
    /// Provides access to consultation information for appointment management.
    /// Used for consultation-based appointment operations and medical record management.
    /// </summary>
    public virtual Consultation? Consultation { get; set; }

    // Status Foreign Keys
    /// <summary>
    /// Foreign key reference to the AppointmentStatus that defines this appointment's current status.
    /// Links this appointment to the specific status (Pending, Approved, Completed, etc.).
    /// Required for appointment status tracking and workflow management.
    /// </summary>
    public Guid AppointmentStatusId { get; set; }
    
    /// <summary>
    /// Navigation property to the AppointmentStatus that defines this appointment's current status.
    /// Provides access to status information for appointment management.
    /// Used for appointment status tracking and workflow management.
    /// </summary>
    public virtual AppointmentStatus AppointmentStatus { get; set; } = null!;

    /// <summary>
    /// Foreign key reference to the AppointmentType that defines this appointment's type.
    /// Links this appointment to the specific type (Consultation, Follow-up, Emergency, etc.).
    /// Required for appointment type management and categorization.
    /// </summary>
    public Guid AppointmentTypeId { get; set; }
    
    /// <summary>
    /// Navigation property to the AppointmentType that defines this appointment's type.
    /// Provides access to type information for appointment management.
    /// Used for appointment type management and categorization.
    /// </summary>
    public virtual AppointmentType AppointmentType { get; set; } = null!;

    /// <summary>
    /// Foreign key reference to the ConsultationMode that defines how this appointment is conducted.
    /// Links this appointment to the specific mode (Video, Phone, In-person, etc.).
    /// Required for consultation mode management and appointment setup.
    /// </summary>
    public Guid ConsultationModeId { get; set; }
    
    /// <summary>
    /// Navigation property to the ConsultationMode that defines how this appointment is conducted.
    /// Provides access to mode information for appointment management.
    /// Used for consultation mode management and appointment setup.
    /// </summary>
    public virtual ConsultationMode ConsultationMode { get; set; } = null!;

    /// <summary>
    /// Foreign key reference to the PaymentStatus that defines this appointment's payment status.
    /// Links this appointment to the specific payment status (Pending, Paid, Failed, etc.).
    /// Required for appointment payment tracking and billing management.
    /// </summary>
    public Guid PaymentStatusId { get; set; }
    
    /// <summary>
    /// Navigation property to the PaymentStatus that defines this appointment's payment status.
    /// Provides access to payment status information for appointment management.
    /// Used for appointment payment tracking and billing management.
    /// </summary>
    public virtual PaymentStatus? PaymentStatus { get; set; }

    // Appointment details
    /// <summary>
    /// Date and time when this appointment is scheduled to take place.
    /// Used for appointment scheduling and calendar management.
    /// Set when the appointment is first scheduled or rescheduled.
    /// </summary>
    public DateTime ScheduledAt { get; set; }
    
    /// <summary>
    /// Date and time when this appointment actually started.
    /// Used for appointment tracking and duration calculations.
    /// Set when the appointment begins (e.g., when video call starts).
    /// </summary>
    public DateTime? StartedAt { get; set; }
    
    /// <summary>
    /// Date and time when this appointment actually ended.
    /// Used for appointment tracking and duration calculations.
    /// Set when the appointment concludes (e.g., when video call ends).
    /// </summary>
    public DateTime? EndedAt { get; set; }
    
    /// <summary>
    /// Date and time when this appointment was accepted by the provider.
    /// Used for appointment workflow tracking and provider response management.
    /// Set when the provider accepts the appointment request.
    /// </summary>
    public DateTime? AcceptedAt { get; set; }
    
    /// <summary>
    /// Date and time when this appointment was rejected by the provider.
    /// Used for appointment workflow tracking and provider response management.
    /// Set when the provider rejects the appointment request.
    /// </summary>
    public DateTime? RejectedAt { get; set; }
    
    /// <summary>
    /// Date and time when this appointment was completed.
    /// Used for appointment completion tracking and workflow management.
    /// Set when the appointment is marked as completed by the provider.
    /// </summary>
    public DateTime? CompletedAt { get; set; }
    
    /// <summary>
    /// Date and time when this appointment was cancelled.
    /// Used for appointment cancellation tracking and workflow management.
    /// Set when the appointment is cancelled by either party.
    /// </summary>
    public DateTime? CancelledAt { get; set; }
    
    /// <summary>
    /// Expected duration of this appointment in minutes.
    /// Used for appointment scheduling and time management.
    /// Defaults to 30 minutes for standard consultation appointments.
    /// </summary>
    public int DurationMinutes { get; set; } = 30;

    // Patient information
    /// <summary>
    /// Reason for the patient's visit or consultation request.
    /// Required field for appointment context and medical record keeping.
    /// Used for appointment preparation and provider understanding of patient needs.
    /// </summary>
    [Required]
    [MaxLength(1000)]
    public string ReasonForVisit { get; set; } = string.Empty;

    /// <summary>
    /// Patient's reported symptoms and health concerns.
    /// Used for appointment preparation and medical record keeping.
    /// Provides context for the provider to understand the patient's condition.
    /// </summary>
    [MaxLength(1000)]
    public string? Symptoms { get; set; }

    /// <summary>
    /// Additional notes provided by the patient.
    /// Used for appointment preparation and medical record keeping.
    /// Can include patient concerns, questions, or additional context.
    /// </summary>
    [MaxLength(1000)]
    public string? PatientNotes { get; set; }

    // Provider information
    /// <summary>
    /// Provider's diagnosis or assessment of the patient's condition.
    /// Used for medical record keeping and follow-up care.
    /// Set by the provider during or after the appointment.
    /// </summary>
    [MaxLength(1000)]
    public string? Diagnosis { get; set; }

    /// <summary>
    /// Prescription or treatment recommendations provided by the provider.
    /// Used for medical record keeping and follow-up care.
    /// Set by the provider during or after the appointment.
    /// </summary>
    [MaxLength(1000)]
    public string? Prescription { get; set; }

    /// <summary>
    /// Additional notes provided by the provider.
    /// Used for medical record keeping and follow-up care.
    /// Can include treatment notes, observations, or recommendations.
    /// </summary>
    [MaxLength(1000)]
    public string? ProviderNotes { get; set; }

    /// <summary>
    /// Follow-up instructions provided by the provider to the patient.
    /// Used for patient care and follow-up appointment scheduling.
    /// Set by the provider during or after the appointment.
    /// </summary>
    [MaxLength(1000)]
    public string? FollowUpInstructions { get; set; }

    /// <summary>
    /// Reason for appointment cancellation if applicable.
    /// Used for cancellation tracking and appointment management.
    /// Set when the appointment is cancelled by either party.
    /// </summary>
    [MaxLength(1000)]
    public string? CancellationReason { get; set; }

    /// <summary>
    /// Date for follow-up appointment if recommended by the provider.
    /// Used for follow-up appointment scheduling and patient care.
    /// Set by the provider during or after the appointment.
    /// </summary>
    public DateTime? FollowUpDate { get; set; }

    // Payment information
    /// <summary>
    /// Fee amount for this appointment in the specified currency.
    /// Used for appointment billing and payment processing.
    /// Set when the appointment is created or when pricing is determined.
    /// </summary>
    public decimal Fee { get; set; }
    
    /// <summary>
    /// Stripe payment intent ID for this appointment.
    /// Links this appointment to the corresponding Stripe payment intent.
    /// Used for Stripe integration and payment processing.
    /// </summary>
    public string? StripePaymentIntentId { get; set; }
    
    /// <summary>
    /// Stripe session ID for this appointment.
    /// Links this appointment to the corresponding Stripe checkout session.
    /// Used for Stripe integration and payment processing.
    /// </summary>
    public string? StripeSessionId { get; set; }
    
    /// <summary>
    /// Indicates whether payment has been captured for this appointment.
    /// Used for payment status tracking and billing management.
    /// Set to true when payment is successfully captured.
    /// </summary>
    public bool IsPaymentCaptured { get; set; } = false;
    
    /// <summary>
    /// Indicates whether this appointment has been refunded.
    /// Used for refund tracking and billing management.
    /// Set to true when refund is processed for this appointment.
    /// </summary>
    public bool IsRefunded { get; set; } = false;
    
    /// <summary>
    /// Amount that has been refunded for this appointment.
    /// Used for refund tracking and billing management.
    /// Set when refund is processed for this appointment.
    /// </summary>
    public decimal? RefundAmount { get; set; }

    // Video call information
    /// <summary>
    /// OpenTok session ID for video call integration.
    /// Links this appointment to the corresponding OpenTok video session.
    /// Used for video call management and integration.
    /// </summary>
    public string? OpenTokSessionId { get; set; }
    
    /// <summary>
    /// Meeting URL for video call access.
    /// Used for video call access and user interface.
    /// Generated when video call is set up for this appointment.
    /// </summary>
    public string? MeetingUrl { get; set; }
    
    /// <summary>
    /// Meeting ID for video call identification.
    /// Used for video call identification and management.
    /// Generated when video call is set up for this appointment.
    /// </summary>
    public string? MeetingId { get; set; }
    
    /// <summary>
    /// Indicates whether the video call has been started.
    /// Used for video call status tracking and management.
    /// Set to true when video call begins.
    /// </summary>
    public bool IsVideoCallStarted { get; set; } = false;
    
    /// <summary>
    /// Indicates whether the video call has been ended.
    /// Used for video call status tracking and management.
    /// Set to true when video call concludes.
    /// </summary>
    public bool IsVideoCallEnded { get; set; } = false;

    // Recording information
    /// <summary>
    /// Recording ID for appointment recording.
    /// Links this appointment to the corresponding recording.
    /// Used for recording management and access.
    /// </summary>
    public string? RecordingId { get; set; }
    
    /// <summary>
    /// URL to access the appointment recording.
    /// Used for recording access and user interface.
    /// Generated when recording is available for this appointment.
    /// </summary>
    public string? RecordingUrl { get; set; }
    
    /// <summary>
    /// Indicates whether recording is enabled for this appointment.
    /// Used for recording management and privacy control.
    /// Defaults to true for standard consultation appointments.
    /// </summary>
    public bool IsRecordingEnabled { get; set; } = true;

    // Notifications
    /// <summary>
    /// Indicates whether the patient has been notified about this appointment.
    /// Used for notification tracking and appointment management.
    /// Set to true when patient notification is sent.
    /// </summary>
    public bool IsPatientNotified { get; set; } = false;
    
    /// <summary>
    /// Indicates whether the provider has been notified about this appointment.
    /// Used for notification tracking and appointment management.
    /// Set to true when provider notification is sent.
    /// </summary>
    public bool IsProviderNotified { get; set; } = false;
    
    /// <summary>
    /// Date and time when the last reminder was sent for this appointment.
    /// Used for reminder tracking and appointment management.
    /// Set when reminder notification is sent.
    /// </summary>
    public DateTime? LastReminderSent { get; set; }

    // Timeout and expiration
    /// <summary>
    /// Date and time when this appointment expires.
    /// Used for appointment expiration tracking and management.
    /// Set when appointment is created or when expiration is determined.
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
    
    /// <summary>
    /// Date and time when this appointment will be automatically cancelled.
    /// Used for automatic cancellation tracking and management.
    /// Set when appointment is created or when auto-cancellation is determined.
    /// </summary>
    public DateTime? AutoCancellationAt { get; set; }

    // Audit properties are inherited from BaseEntity

    // Navigation properties
    /// <summary>
    /// Collection of documents associated with this appointment.
    /// Used for document management and medical record keeping.
    /// Includes appointment-related documents, reports, and files.
    /// </summary>
    public virtual ICollection<AppointmentDocument> Documents { get; set; } = new List<AppointmentDocument>();
    
    /// <summary>
    /// Collection of reminders associated with this appointment.
    /// Used for reminder management and appointment tracking.
    /// Includes appointment reminders and notifications.
    /// </summary>
    public virtual ICollection<AppointmentReminder> Reminders { get; set; } = new List<AppointmentReminder>();
    
    /// <summary>
    /// Collection of events associated with this appointment.
    /// Used for event tracking and appointment history.
    /// Includes appointment status changes and important events.
    /// </summary>
    public virtual ICollection<AppointmentEvent> Events { get; set; } = new List<AppointmentEvent>();
    
    /// <summary>
    /// Collection of participants associated with this appointment.
    /// Used for participant management and appointment access control.
    /// Includes appointment participants and their roles.
    /// </summary>
    public virtual ICollection<AppointmentParticipant> Participants { get; set; } = new List<AppointmentParticipant>();
    
    /// <summary>
    /// Collection of payment logs associated with this appointment.
    /// Used for payment tracking and billing management.
    /// Includes appointment payment history and transactions.
    /// </summary>
    public virtual ICollection<AppointmentPaymentLog> PaymentLogs { get; set; } = new List<AppointmentPaymentLog>();

    // Computed properties
    /// <summary>
    /// Indicates whether this appointment is currently active.
    /// Returns true if appointment status is Pending, Approved, Scheduled, or InMeeting.
    /// Used for appointment status checking and workflow management.
    /// </summary>
    [NotMapped]
    public bool IsAppointmentActive => AppointmentStatus?.Name == "Pending" || AppointmentStatus?.Name == "Approved" || 
                           AppointmentStatus?.Name == "Scheduled" || AppointmentStatus?.Name == "InMeeting";
    
    /// <summary>
    /// Indicates whether this appointment has been completed.
    /// Returns true if appointment status is Completed.
    /// Used for appointment completion checking and workflow management.
    /// </summary>
    [NotMapped]
    public bool IsCompleted => AppointmentStatus?.Name == "Completed";
    
    /// <summary>
    /// Indicates whether this appointment has been cancelled.
    /// Returns true if appointment status is Cancelled, Rejected, or Expired.
    /// Used for appointment cancellation checking and workflow management.
    /// </summary>
    [NotMapped]
    public bool IsCancelled => AppointmentStatus?.Name == "Cancelled" || AppointmentStatus?.Name == "Rejected" || 
                              AppointmentStatus?.Name == "Expired";
    
    /// <summary>
    /// Calculates the actual duration of this appointment.
    /// Returns the time difference between StartedAt and EndedAt if both are available.
    /// Used for appointment duration tracking and billing calculations.
    /// </summary>
    [NotMapped]
    public TimeSpan? Duration => StartedAt.HasValue && EndedAt.HasValue ? EndedAt.Value - StartedAt.Value : null;
    
    /// <summary>
    /// Indicates whether this appointment has expired.
    /// Returns true if ExpiresAt is set and current time is past the expiration time.
    /// Used for appointment expiration checking and automatic cleanup.
    /// </summary>
    [NotMapped]
    public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;
}

/// <summary>
/// Entity for managing documents associated with appointments.
/// This entity handles document storage, metadata, and access control for appointment-related files.
/// It serves as a bridge between appointments and their associated documents, providing document
/// management capabilities including file storage, type classification, and upload tracking.
/// </summary>
public class AppointmentDocument : BaseEntity
{
    /// <summary>
    /// Primary key identifier for the appointment document.
    /// Uses Guid for better scalability and security in distributed systems.
    /// Unique identifier for each document in the system.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    // Foreign keys
    /// <summary>
    /// Foreign key reference to the Appointment that this document belongs to.
    /// Links this document to the specific appointment.
    /// Required for appointment-document relationship management.
    /// </summary>
    public Guid AppointmentId { get; set; }
    
    /// <summary>
    /// Navigation property to the Appointment that this document belongs to.
    /// Provides access to appointment information for document management.
    /// Used for appointment-document relationship operations.
    /// </summary>
    public virtual Appointment Appointment { get; set; } = null!;

    /// <summary>
    /// Foreign key reference to the User who uploaded this document.
    /// Links this document to the specific user who uploaded it.
    /// Optional - used for upload tracking and access control.
    /// </summary>
    public int? UploadedById { get; set; }
    
    /// <summary>
    /// Navigation property to the User who uploaded this document.
    /// Provides access to uploader information for document management.
    /// Used for upload tracking and access control.
    /// </summary>
    public virtual User? UploadedBy { get; set; }

    /// <summary>
    /// Foreign key reference to the Provider associated with this document.
    /// Links this document to the specific provider.
    /// Optional - used for provider-specific document management.
    /// </summary>
    public int? ProviderId { get; set; }
    
    /// <summary>
    /// Navigation property to the Provider associated with this document.
    /// Provides access to provider information for document management.
    /// Used for provider-specific document operations.
    /// </summary>
    public virtual Provider? Provider { get; set; }

    /// <summary>
    /// Foreign key reference to the DocumentType that defines this document's type.
    /// Links this document to the specific document type (Report, Image, Prescription, etc.).
    /// Required for document type management and categorization.
    /// </summary>
    public Guid DocumentTypeId { get; set; }
    
    /// <summary>
    /// Navigation property to the DocumentType that defines this document's type.
    /// Provides access to document type information for document management.
    /// Used for document type management and categorization.
    /// </summary>
    public virtual DocumentType DocumentType { get; set; } = null!;

    // Document details
    /// <summary>
    /// Original filename of the uploaded document.
    /// Used for document identification and user interface display.
    /// Required for document management and user experience.
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// File path where the document is stored in the system.
    /// Used for document storage and access management.
    /// Required for document retrieval and file operations.
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// MIME type or file extension of the document.
    /// Used for document type identification and processing.
    /// Helps with file handling and content type management.
    /// </summary>
    [MaxLength(100)]
    public string FileType { get; set; } = string.Empty;

    /// <summary>
    /// Size of the document file in bytes.
    /// Used for storage management and file size validation.
    /// Helps with storage quota management and file size limits.
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// Optional description or notes about the document.
    /// Used for document context and additional information.
    /// Can include document purpose, content summary, or special notes.
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }
}

/// <summary>
/// Entity for managing appointment reminders and notifications.
/// This entity handles reminder scheduling, delivery tracking, and notification management for appointments.
/// It serves as a comprehensive reminder system that supports multiple reminder types, timing options,
/// and delivery methods including email and SMS notifications.
/// </summary>
public class AppointmentReminder : BaseEntity
{
    /// <summary>
    /// Primary key identifier for the appointment reminder.
    /// Uses Guid for better scalability and security in distributed systems.
    /// Unique identifier for each reminder in the system.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    // Foreign keys
    /// <summary>
    /// Foreign key reference to the Appointment that this reminder is for.
    /// Links this reminder to the specific appointment.
    /// Required for appointment-reminder relationship management.
    /// </summary>
    public Guid AppointmentId { get; set; }
    
    /// <summary>
    /// Navigation property to the Appointment that this reminder is for.
    /// Provides access to appointment information for reminder management.
    /// Used for appointment-reminder relationship operations.
    /// </summary>
    public virtual Appointment Appointment { get; set; } = null!;

    /// <summary>
    /// Foreign key reference to the ReminderType that defines this reminder's type.
    /// Links this reminder to the specific reminder type (Email, SMS, Push, etc.).
    /// Required for reminder type management and categorization.
    /// </summary>
    public Guid ReminderTypeId { get; set; }
    
    /// <summary>
    /// Foreign key reference to the ReminderTiming that defines when this reminder should be sent.
    /// Links this reminder to the specific timing (15 minutes before, 1 hour before, etc.).
    /// Required for reminder timing management and scheduling.
    /// </summary>
    public Guid ReminderTimingId { get; set; }
    
    /// <summary>
    /// Navigation property to the ReminderType that defines this reminder's type.
    /// Provides access to reminder type information for reminder management.
    /// Used for reminder type management and categorization.
    /// </summary>
    public virtual ReminderType ReminderType { get; set; } = null!;
    
    /// <summary>
    /// Navigation property to the ReminderTiming that defines when this reminder should be sent.
    /// Provides access to reminder timing information for reminder management.
    /// Used for reminder timing management and scheduling.
    /// </summary>
    public virtual ReminderTiming ReminderTiming { get; set; } = null!;

    // Reminder details
    /// <summary>
    /// Date and time when this reminder is scheduled to be sent.
    /// Used for reminder scheduling and delivery management.
    /// Set when the reminder is created or when scheduling is determined.
    /// </summary>
    public DateTime ScheduledAt { get; set; }
    
    /// <summary>
    /// Date and time when this reminder was actually sent.
    /// Used for reminder delivery tracking and management.
    /// Set when the reminder is successfully sent.
    /// </summary>
    public DateTime? SentAt { get; set; }
    
    /// <summary>
    /// Indicates whether this reminder has been sent.
    /// Used for reminder status tracking and management.
    /// Set to true when reminder is successfully sent.
    /// </summary>
    public bool IsSent { get; set; } = false;
    
    /// <summary>
    /// Indicates whether this reminder has been delivered to the recipient.
    /// Used for reminder delivery tracking and management.
    /// Set to true when reminder is successfully delivered.
    /// </summary>
    public bool IsDelivered { get; set; } = false;

    /// <summary>
    /// Custom message content for this reminder.
    /// Used for reminder personalization and content management.
    /// Can include appointment details, instructions, or custom messages.
    /// </summary>
    [MaxLength(1000)]
    public string? Message { get; set; }

    /// <summary>
    /// Email address of the reminder recipient.
    /// Used for email reminder delivery and recipient management.
    /// Set when reminder is sent via email.
    /// </summary>
    [MaxLength(100)]
    public string? RecipientEmail { get; set; }

    /// <summary>
    /// Phone number of the reminder recipient.
    /// Used for SMS reminder delivery and recipient management.
    /// Set when reminder is sent via SMS.
    /// </summary>
    [MaxLength(20)]
    public string? RecipientPhone { get; set; }
}

/// <summary>
/// Entity for tracking appointment events and status changes.
/// This entity handles event logging, audit trails, and status change tracking for appointments.
/// It serves as a comprehensive event system that records all significant events and changes
/// that occur during the appointment lifecycle, providing detailed audit trails and history.
/// </summary>
public class AppointmentEvent : BaseEntity
{
    /// <summary>
    /// Primary key identifier for the appointment event.
    /// Uses Guid for better scalability and security in distributed systems.
    /// Unique identifier for each event in the system.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    // Foreign keys
    /// <summary>
    /// Foreign key reference to the Appointment that this event is for.
    /// Links this event to the specific appointment.
    /// Required for appointment-event relationship management.
    /// </summary>
    public Guid AppointmentId { get; set; }
    
    /// <summary>
    /// Navigation property to the Appointment that this event is for.
    /// Provides access to appointment information for event management.
    /// Used for appointment-event relationship operations.
    /// </summary>
    public virtual Appointment Appointment { get; set; } = null!;

    /// <summary>
    /// Foreign key reference to the User who triggered this event.
    /// Links this event to the specific user who caused the event.
    /// Optional - used for user action tracking and audit trails.
    /// </summary>
    public int? UserId { get; set; }
    
    /// <summary>
    /// Navigation property to the User who triggered this event.
    /// Provides access to user information for event management.
    /// Used for user action tracking and audit trails.
    /// </summary>
    public virtual User? User { get; set; }

    /// <summary>
    /// Foreign key reference to the Provider who triggered this event.
    /// Links this event to the specific provider who caused the event.
    /// Optional - used for provider action tracking and audit trails.
    /// </summary>
    public int? ProviderId { get; set; }
    
    /// <summary>
    /// Navigation property to the Provider who triggered this event.
    /// Provides access to provider information for event management.
    /// Used for provider action tracking and audit trails.
    /// </summary>
    public virtual Provider? Provider { get; set; }

    /// <summary>
    /// Foreign key reference to the EventType that defines this event's type.
    /// Links this event to the specific event type (StatusChange, Payment, Cancellation, etc.).
    /// Required for event type management and categorization.
    /// </summary>
    public Guid EventTypeId { get; set; }
    
    /// <summary>
    /// Navigation property to the EventType that defines this event's type.
    /// Provides access to event type information for event management.
    /// Used for event type management and categorization.
    /// </summary>
    public virtual EventType EventType { get; set; } = null!;

    // Event details
    /// <summary>
    /// Date and time when this event occurred.
    /// Used for event timing and chronological tracking.
    /// Defaults to current UTC time when event is created.
    /// </summary>
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Description or details about this event.
    /// Used for event documentation and human-readable event information.
    /// Can include event context, reasons, or additional details.
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// JSON metadata containing additional event information.
    /// Used for storing structured event data and additional context.
    /// Can include event-specific data, parameters, or configuration.
    /// </summary>
    [MaxLength(500)]
    public string? Metadata { get; set; } // JSON data for additional event info
} 