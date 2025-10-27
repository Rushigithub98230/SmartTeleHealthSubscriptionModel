using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

/// <summary>
/// Application log entity that stores all ILogger statements from the application.
/// This entity captures informational, warning, error, and critical logs from all services
/// and controllers for monitoring, debugging, and audit purposes.
/// </summary>
public class ApplicationLog : BaseEntity
{
    /// <summary>
    /// Primary key identifier for the application log.
    /// Uses long for scalability with high-volume logging.
    /// </summary>
    [Key]
    public long Id { get; set; }
    
    /// <summary>
    /// Timestamp when the log event occurred.
    /// Stored in UTC for consistency across time zones.
    /// </summary>
    [Required]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Log level indicating the severity of the log event.
    /// Values: Information, Warning, Error, Critical, Debug, Trace
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string LogLevel { get; set; } = string.Empty;
    
    /// <summary>
    /// Source of the log event (service, controller, or component name).
    /// Examples: SubscriptionService, PaymentService, StripeWebhookController
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Source { get; set; } = string.Empty;
    
    /// <summary>
    /// The log message content.
    /// Contains the formatted log message with any placeholders replaced.
    /// </summary>
    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Full exception details if the log is an error or critical.
    /// Contains stack trace, inner exceptions, and other error information.
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? Exception { get; set; }
    
    /// <summary>
    /// ID of the user who triggered the action that generated this log.
    /// Null if the action was system-generated or not user-specific.
    /// </summary>
    public int? UserId { get; set; }
    
    /// <summary>
    /// Type of operation that triggered the log.
    /// Examples: CreateSubscription, ProcessPayment, UpdateUser, etc.
    /// </summary>
    [MaxLength(100)]
    public string? Operation { get; set; }
    
    /// <summary>
    /// Additional contextual data in JSON format.
    /// May contain IDs, parameters, or other relevant information.
    /// </summary>
    [MaxLength(2000)]
    public string? AdditionalData { get; set; }
    
    /// <summary>
    /// Correlation ID for tracking related operations across multiple logs.
    /// Useful for tracing a single operation across multiple services.
    /// </summary>
    [MaxLength(100)]
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Navigation property to the User who triggered the action.
    /// Provides access to user information for log display and analysis.
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public virtual User? User { get; set; }
}

