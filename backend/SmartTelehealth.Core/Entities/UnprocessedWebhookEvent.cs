using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities
{
    /// <summary>
    /// Entity for tracking webhook events that couldn't be processed immediately
    /// due to missing related entities (e.g., invoice events before subscription sync)
    /// </summary>
    public class UnprocessedWebhookEvent : BaseEntity
    {
        /// <summary>
        /// Primary key identifier for the unprocessed webhook event
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Stripe webhook event ID for deduplication
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string StripeEventId { get; set; } = string.Empty;

        /// <summary>
        /// Type of Stripe webhook event (e.g., invoice.created, invoice.finalized)
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string EventType { get; set; } = string.Empty;

        /// <summary>
        /// Raw JSON payload of the webhook event
        /// </summary>
        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public string EventData { get; set; } = string.Empty;

        /// <summary>
        /// Stripe subscription ID that the event relates to
        /// </summary>
        [MaxLength(255)]
        public string? StripeSubscriptionId { get; set; }

        /// <summary>
        /// Stripe invoice ID (for invoice events)
        /// </summary>
        [MaxLength(255)]
        public string? StripeInvoiceId { get; set; }

        /// <summary>
        /// Stripe customer ID
        /// </summary>
        [MaxLength(255)]
        public string? StripeCustomerId { get; set; }

        /// <summary>
        /// Reason why the event couldn't be processed
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string FailureReason { get; set; } = string.Empty;

        /// <summary>
        /// Number of retry attempts made
        /// </summary>
        public int RetryCount { get; set; } = 0;

        /// <summary>
        /// Maximum number of retry attempts allowed
        /// </summary>
        public int MaxRetries { get; set; } = 48; // 24 hours with 5-minute intervals

        /// <summary>
        /// When the next retry should be attempted
        /// </summary>
        public DateTime NextRetryAt { get; set; }

        /// <summary>
        /// Status of the unprocessed event
        /// </summary>
        public enum ProcessingStatus
        {
            Pending,
            Processing,
            Completed,
            Failed,
            Expired
        }

        /// <summary>
        /// Current processing status
        /// </summary>
        public ProcessingStatus Status { get; set; } = ProcessingStatus.Pending;

        /// <summary>
        /// When the event was originally received
        /// </summary>
        public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When the event was last processed (successfully or failed)
        /// </summary>
        public DateTime? LastProcessedAt { get; set; }

        /// <summary>
        /// Error message from the last processing attempt
        /// </summary>
        [MaxLength(1000)]
        public string? LastError { get; set; }

        /// <summary>
        /// Whether this event is still active (not expired or permanently failed)
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}

