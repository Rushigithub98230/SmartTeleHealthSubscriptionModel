using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

public class Appointment : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    // Foreign keys
    public int PatientId { get; set; }
    public virtual User Patient { get; set; } = null!;

    public int ProviderId { get; set; }
    public virtual Provider Provider { get; set; } = null!;

    public Guid CategoryId { get; set; }
    public virtual Category Category { get; set; } = null!;

    public Guid? SubscriptionId { get; set; }
    public virtual Subscription? Subscription { get; set; }

    public Guid? ConsultationId { get; set; }
    public virtual Consultation? Consultation { get; set; }

    // Status Foreign Keys
    public Guid AppointmentStatusId { get; set; }
    public virtual AppointmentStatus AppointmentStatus { get; set; } = null!;

    public Guid AppointmentTypeId { get; set; }
    public virtual AppointmentType AppointmentType { get; set; } = null!;

    public Guid ConsultationModeId { get; set; }
    public virtual ConsultationMode ConsultationMode { get; set; } = null!;

    public Guid PaymentStatusId { get; set; }
    public virtual PaymentStatus? PaymentStatus { get; set; }

    // Appointment details
    public DateTime ScheduledAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime? RejectedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public int DurationMinutes { get; set; } = 30;

    // Patient information
    [Required]
    [MaxLength(1000)]
    public string ReasonForVisit { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Symptoms { get; set; }

    [MaxLength(1000)]
    public string? PatientNotes { get; set; }

    // Provider information
    [MaxLength(1000)]
    public string? Diagnosis { get; set; }

    [MaxLength(1000)]
    public string? Prescription { get; set; }

    [MaxLength(1000)]
    public string? ProviderNotes { get; set; }

    [MaxLength(1000)]
    public string? FollowUpInstructions { get; set; }

    [MaxLength(1000)]
    public string? CancellationReason { get; set; }

    public DateTime? FollowUpDate { get; set; }

    // Payment information
    public decimal Fee { get; set; }
    public string? StripePaymentIntentId { get; set; }
    public string? StripeSessionId { get; set; }
    public bool IsPaymentCaptured { get; set; } = false;
    public bool IsRefunded { get; set; } = false;
    public decimal? RefundAmount { get; set; }

    // Video call information
    public string? OpenTokSessionId { get; set; }
    public string? MeetingUrl { get; set; }
    public string? MeetingId { get; set; }
    public bool IsVideoCallStarted { get; set; } = false;
    public bool IsVideoCallEnded { get; set; } = false;

    // Recording information
    public string? RecordingId { get; set; }
    public string? RecordingUrl { get; set; }
    public bool IsRecordingEnabled { get; set; } = true;

    // Notifications
    public bool IsPatientNotified { get; set; } = false;
    public bool IsProviderNotified { get; set; } = false;
    public DateTime? LastReminderSent { get; set; }

    // Timeout and expiration
    public DateTime? ExpiresAt { get; set; }
    public DateTime? AutoCancellationAt { get; set; }

    // Audit properties are inherited from BaseEntity

    // Navigation properties
    public virtual ICollection<AppointmentDocument> Documents { get; set; } = new List<AppointmentDocument>();
    public virtual ICollection<AppointmentReminder> Reminders { get; set; } = new List<AppointmentReminder>();
    public virtual ICollection<AppointmentEvent> Events { get; set; } = new List<AppointmentEvent>();
    public virtual ICollection<AppointmentParticipant> Participants { get; set; } = new List<AppointmentParticipant>();
    public virtual ICollection<AppointmentPaymentLog> PaymentLogs { get; set; } = new List<AppointmentPaymentLog>();

    // Computed properties
    [NotMapped]
    public bool IsAppointmentActive => AppointmentStatus?.Name == "Pending" || AppointmentStatus?.Name == "Approved" || 
                           AppointmentStatus?.Name == "Scheduled" || AppointmentStatus?.Name == "InMeeting";
    [NotMapped]
    public bool IsCompleted => AppointmentStatus?.Name == "Completed";
    [NotMapped]
    public bool IsCancelled => AppointmentStatus?.Name == "Cancelled" || AppointmentStatus?.Name == "Rejected" || 
                              AppointmentStatus?.Name == "Expired";
    [NotMapped]
    public TimeSpan? Duration => StartedAt.HasValue && EndedAt.HasValue ? EndedAt.Value - StartedAt.Value : null;
    [NotMapped]
    public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;
}

public class AppointmentDocument : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    // Foreign keys
    public Guid AppointmentId { get; set; }
    public virtual Appointment Appointment { get; set; } = null!;

    public int? UploadedById { get; set; }
    public virtual User? UploadedBy { get; set; }

    public int? ProviderId { get; set; }
    public virtual Provider? Provider { get; set; }

    public Guid DocumentTypeId { get; set; }
    public virtual DocumentType DocumentType { get; set; } = null!;

    // Document details
    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;

    [MaxLength(100)]
    public string FileType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }
}

public class AppointmentReminder : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    // Foreign keys
    public Guid AppointmentId { get; set; }
    public virtual Appointment Appointment { get; set; } = null!;

    public Guid ReminderTypeId { get; set; }
    public Guid ReminderTimingId { get; set; }
    public virtual ReminderType ReminderType { get; set; } = null!;
    public virtual ReminderTiming ReminderTiming { get; set; } = null!;

    // Reminder details
    public DateTime ScheduledAt { get; set; }
    public DateTime? SentAt { get; set; }
    public bool IsSent { get; set; } = false;
    public bool IsDelivered { get; set; } = false;

    [MaxLength(1000)]
    public string? Message { get; set; }

    [MaxLength(100)]
    public string? RecipientEmail { get; set; }

    [MaxLength(20)]
    public string? RecipientPhone { get; set; }
}

public class AppointmentEvent : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    // Foreign keys
    public Guid AppointmentId { get; set; }
    public virtual Appointment Appointment { get; set; } = null!;

    public int? UserId { get; set; }
    public virtual User? User { get; set; }

    public int? ProviderId { get; set; }
    public virtual Provider? Provider { get; set; }

    public Guid EventTypeId { get; set; }
    public virtual EventType EventType { get; set; } = null!;

    // Event details
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(500)]
    public string? Metadata { get; set; } // JSON data for additional event info
} 