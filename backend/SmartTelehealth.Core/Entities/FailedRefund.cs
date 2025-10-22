using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities
{
    /// <summary>
    /// Entity representing a failed compensating refund that requires manual intervention.
    /// When Stripe successfully charges a user but our database transaction fails,
    /// we attempt to issue a compensating refund. If that refund also fails,
    /// we track it here for automated retry and manual review.
    /// </summary>
    [Table("FailedRefunds")]
    public class FailedRefund
    {
        /// <summary>
        /// Unique identifier for the failed refund record
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// The billing record ID associated with this failed refund
        /// </summary>
        [Required]
        public Guid BillingRecordId { get; set; }

        /// <summary>
        /// Navigation property to the associated billing record
        /// </summary>
        [ForeignKey("BillingRecordId")]
        public BillingRecord BillingRecord { get; set; }

        /// <summary>
        /// The Stripe Payment Intent ID that was charged
        /// </summary>
        [Required]
        [StringLength(255)]
        public string StripePaymentIntentId { get; set; } = string.Empty;

        /// <summary>
        /// The Stripe Invoice ID associated with the payment
        /// </summary>
        [StringLength(255)]
        public string? StripeInvoiceId { get; set; }

        /// <summary>
        /// The amount that was charged and needs to be refunded (in dollars)
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        /// <summary>
        /// User ID who was charged
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// Navigation property to the user
        /// </summary>
        [ForeignKey("UserId")]
        public User User { get; set; }

        /// <summary>
        /// Timestamp when the original charge succeeded
        /// </summary>
        [Required]
        public DateTime ChargedAt { get; set; }

        /// <summary>
        /// Timestamp when the database transaction failed
        /// </summary>
        [Required]
        public DateTime DatabaseFailedAt { get; set; }

        /// <summary>
        /// Timestamp when the refund was first attempted and failed
        /// </summary>
        [Required]
        public DateTime FirstAttemptAt { get; set; }

        /// <summary>
        /// Timestamp of the most recent retry attempt
        /// </summary>
        public DateTime? LastAttemptAt { get; set; }

        /// <summary>
        /// Number of retry attempts made
        /// </summary>
        public int RetryCount { get; set; } = 0;

        /// <summary>
        /// Maximum number of retries allowed
        /// </summary>
        public int MaxRetries { get; set; } = 5;

        /// <summary>
        /// Current status of the failed refund
        /// </summary>
        [Required]
        [StringLength(50)]
        public FailedRefundStatus Status { get; set; } = FailedRefundStatus.Pending;

        /// <summary>
        /// Error message from the most recent failed refund attempt
        /// </summary>
        [StringLength(2000)]
        public string? LastErrorMessage { get; set; }

        /// <summary>
        /// Stack trace or detailed error information
        /// </summary>
        [Column(TypeName = "text")]
        public string? ErrorDetails { get; set; }

        /// <summary>
        /// Reason for the database failure (if known)
        /// </summary>
        [StringLength(2000)]
        public string? DatabaseFailureReason { get; set; }

        /// <summary>
        /// Indicates if an admin has been notified about this failed refund
        /// </summary>
        public bool AdminNotified { get; set; } = false;

        /// <summary>
        /// Timestamp when admin was notified
        /// </summary>
        public DateTime? AdminNotifiedAt { get; set; }

        /// <summary>
        /// Timestamp when the refund was successfully processed (if resolved)
        /// </summary>
        public DateTime? ResolvedAt { get; set; }

        /// <summary>
        /// Admin user ID who manually resolved this (if applicable)
        /// </summary>
        public int? ResolvedBy { get; set; }

        /// <summary>
        /// Resolution notes from admin
        /// </summary>
        [StringLength(2000)]
        public string? ResolutionNotes { get; set; }

        /// <summary>
        /// Priority level for manual review
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Priority { get; set; } = "High"; // High, Critical

        /// <summary>
        /// Indicates if this should be retried by the background service
        /// </summary>
        public bool ShouldRetry => Status == FailedRefundStatus.Pending && RetryCount < MaxRetries;

        /// <summary>
        /// Indicates if this has exceeded maximum retries and needs manual intervention
        /// </summary>
        public bool RequiresManualIntervention => Status == FailedRefundStatus.Pending && RetryCount >= MaxRetries;

        /// <summary>
        /// Audit: Created date
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Audit: Created by user ID (0 for system)
        /// </summary>
        public int CreatedBy { get; set; }

        /// <summary>
        /// Audit: Last updated date
        /// </summary>
        public DateTime? UpdatedDate { get; set; }

        /// <summary>
        /// Audit: Last updated by user ID
        /// </summary>
        public int? UpdatedBy { get; set; }
    }

    /// <summary>
    /// Status of a failed refund
    /// </summary>
    public enum FailedRefundStatus
    {
        /// <summary>
        /// Pending retry
        /// </summary>
        Pending,

        /// <summary>
        /// Currently being retried by background service
        /// </summary>
        Retrying,

        /// <summary>
        /// Successfully refunded
        /// </summary>
        Refunded,

        /// <summary>
        /// Manually resolved by admin (refunded outside system)
        /// </summary>
        ManuallyResolved,

        /// <summary>
        /// Cancelled (e.g., customer dispute won, manual decision not to refund)
        /// </summary>
        Cancelled
    }
}

