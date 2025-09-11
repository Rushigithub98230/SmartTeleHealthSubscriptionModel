using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities
{
    /// <summary>
    /// Entity representing a processed webhook event to ensure idempotency
    /// and prevent duplicate processing of Stripe webhook events.
    /// </summary>
    [Table("ProcessedWebhookEvents")]
    public class ProcessedWebhookEvent
    {
        /// <summary>
        /// Unique identifier for the processed webhook event record
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// The Stripe event ID from the webhook payload
        /// </summary>
        [Required]
        [StringLength(255)]
        public string StripeEventId { get; set; } = string.Empty;

        /// <summary>
        /// The type of Stripe event (e.g., subscription.created, payment_intent.succeeded)
        /// </summary>
        [Required]
        [StringLength(100)]
        public string EventType { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp when the event was first received
        /// </summary>
        public DateTime ReceivedAt { get; set; }

        /// <summary>
        /// Timestamp when the event was successfully processed
        /// </summary>
        public DateTime? ProcessedAt { get; set; }

        /// <summary>
        /// Indicates whether the event was processed successfully
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Error message if processing failed
        /// </summary>
        [StringLength(2000)]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Number of retry attempts made for this event
        /// </summary>
        public int RetryCount { get; set; }

        /// <summary>
        /// Maximum number of retries allowed for this event
        /// </summary>
        public int MaxRetries { get; set; } = 3;

        /// <summary>
        /// Timestamp when the event was last attempted
        /// </summary>
        public DateTime? LastAttemptAt { get; set; }

        /// <summary>
        /// Additional metadata about the event (JSON format)
        /// </summary>
        [StringLength(4000)]
        public string? Metadata { get; set; }

        /// <summary>
        /// Processing duration in milliseconds
        /// </summary>
        public long? ProcessingDurationMs { get; set; }

        /// <summary>
        /// Indicates if this event should be retried
        /// </summary>
        public bool ShouldRetry => !IsSuccess && RetryCount < MaxRetries;

        /// <summary>
        /// Indicates if this event has exceeded maximum retries
        /// </summary>
        public bool IsPermanentlyFailed => !IsSuccess && RetryCount >= MaxRetries;
    }
}
