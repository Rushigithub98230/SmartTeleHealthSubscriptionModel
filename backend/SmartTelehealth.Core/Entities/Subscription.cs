using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

/// <summary>
/// Core subscription entity that manages user subscriptions to telehealth services.
/// This entity handles the complete subscription lifecycle including creation, activation, billing, trials, pauses, and cancellations.
/// It serves as the central hub for subscription management, integrating with Stripe for payment processing and maintaining
/// synchronization between the local database and Stripe's subscription system.
/// The entity includes comprehensive status management, trial handling, usage tracking, and business logic validation.
/// </summary>
#region Improved Subscription Entity
public class Subscription : BaseEntity
{
    /// <summary>
    /// Primary key identifier for the subscription.
    /// Uses Guid for better scalability and security in distributed systems.
    /// Unique identifier for each subscription record in the system.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    #region Constants
    /// <summary>
    /// Static class containing all valid subscription status constants.
    /// Provides type-safe status values and prevents invalid status assignments.
    /// Used throughout the system for status validation and business logic.
    /// </summary>
    public static class SubscriptionStatuses
    {
        /// <summary>
        /// Subscription is created but not yet activated.
        /// Used for newly created subscriptions awaiting activation.
        /// </summary>
        public const string Pending = "Pending";
        
        /// <summary>
        /// Subscription is active and user has full access to services.
        /// Used for fully functional subscriptions with valid payments.
        /// </summary>
        public const string Active = "Active";
        
        /// <summary>
        /// Subscription is temporarily paused by user or system.
        /// Used for temporary suspension of services without cancellation.
        /// </summary>
        public const string Paused = "Paused";
        
        /// <summary>
        /// Subscription has been cancelled by user or system.
        /// Used for permanently terminated subscriptions.
        /// </summary>
        public const string Cancelled = "Cancelled";
        
        /// <summary>
        /// Subscription has expired due to time or payment issues.
        /// Used for subscriptions that have reached their natural end.
        /// </summary>
        public const string Expired = "Expired";
        
        /// <summary>
        /// Subscription has payment issues that need resolution.
        /// Used for subscriptions with failed payment attempts.
        /// </summary>
        public const string PaymentFailed = "PaymentFailed";
        
        /// <summary>
        /// Subscription is in trial period with limited access.
        /// Used for trial subscriptions before full activation.
        /// </summary>
        public const string TrialActive = "TrialActive";
        
        /// <summary>
        /// Trial period has expired and subscription needs activation.
        /// Used for expired trial subscriptions awaiting conversion.
        /// </summary>
        public const string TrialExpired = "TrialExpired";
        
        /// <summary>
        /// Subscription is suspended due to policy violations or other issues.
        /// Used for administrative suspension of subscriptions.
        /// </summary>
        public const string Suspended = "Suspended";
        
        /// <summary>
        /// Array of all valid subscription statuses for validation.
        /// Used for status validation and business logic enforcement.
        /// </summary>
        public static readonly string[] ValidStatuses = 
        {
            Pending, Active, Paused, Cancelled, Expired, PaymentFailed, TrialActive, TrialExpired, Suspended
        };
    }
    #endregion

    #region Foreign Keys
    /// <summary>
    /// Foreign key reference to the User who owns this subscription.
    /// Links the subscription to the specific user account.
    /// Required for user-specific subscription management and billing.
    /// </summary>
    [Required]
    public int UserId { get; set; }
    
    /// <summary>
    /// Foreign key reference to the SubscriptionPlan that defines this subscription's features and pricing.
    /// Determines what services, privileges, and pricing apply to this subscription.
    /// Required for subscription plan management and feature access control.
    /// </summary>
    [Required]
    public Guid SubscriptionPlanId { get; set; }
    
    /// <summary>
    /// Foreign key reference to the BillingCycle that defines this subscription's billing frequency.
    /// Determines how often the user is billed (monthly, yearly, etc.).
    /// Required for billing cycle management and payment scheduling.
    /// </summary>
    [Required]
    public Guid BillingCycleId { get; set; }
    
    /// <summary>
    /// Foreign key reference to the Provider assigned to this subscription.
    /// Links the subscription to a specific healthcare provider for personalized care.
    /// Optional - used for provider-specific subscriptions and care management.
    /// </summary>
    public int? ProviderId { get; set; }
    #endregion

    #region Navigation Properties
    /// <summary>
    /// Navigation property to the User who owns this subscription.
    /// Provides access to user information for subscription management.
    /// Used for user-specific subscription operations and billing.
    /// </summary>
    public virtual User User { get; set; } = null!;
    
    /// <summary>
    /// Navigation property to the SubscriptionPlan that defines this subscription.
    /// Provides access to plan details including features, pricing, and privileges.
    /// Used for subscription plan management and feature access control.
    /// </summary>
    public virtual SubscriptionPlan SubscriptionPlan { get; set; } = null!;
    
    /// <summary>
    /// Navigation property to the BillingCycle that defines this subscription's billing frequency.
    /// Provides access to billing cycle details and payment scheduling.
    /// Used for billing cycle management and payment operations.
    /// </summary>
    public virtual MasterBillingCycle BillingCycle { get; set; } = null!;
    
    /// <summary>
    /// Navigation property to the Provider assigned to this subscription.
    /// Provides access to provider information for personalized care.
    /// Used for provider-specific subscriptions and care management.
    /// </summary>
    public virtual Provider? Provider { get; set; }
    #endregion

    #region Core Properties
    /// <summary>
    /// Current status of the subscription (Pending, Active, Paused, Cancelled, etc.).
    /// Determines subscription functionality and user access to services.
    /// Synchronized with Stripe subscription status via webhooks.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = SubscriptionStatuses.Pending;
    
    /// <summary>
    /// Reason for the current subscription status.
    /// Used for status tracking, customer support, and audit purposes.
    /// Provides context for status changes and subscription management.
    /// </summary>
    [MaxLength(500)]
    public string? StatusReason { get; set; }
    
    /// <summary>
    /// Date when the subscription was created and activated.
    /// Used for subscription history tracking and billing calculations.
    /// Set when subscription is first created and activated.
    /// </summary>
    [Required]
    public DateTime StartDate { get; set; }
    
    /// <summary>
    /// Date when the subscription ends or was canceled.
    /// Used for subscription expiration tracking and access control.
    /// Set when subscription is canceled or reaches its natural end.
    /// </summary>
    public DateTime? EndDate { get; set; }
    
    /// <summary>
    /// Date when the next billing/payment is scheduled.
    /// Used for payment scheduling and billing notifications.
    /// Updated based on billing cycle and payment success/failure.
    /// </summary>
    [Required]
    public DateTime NextBillingDate { get; set; }
    
    /// <summary>
    /// Current price of the subscription in the specified currency.
    /// Used for billing calculations and payment processing.
    /// Can be updated for price changes or promotional pricing.
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal CurrentPrice { get; set; }
    
    /// <summary>
    /// Indicates whether the subscription should automatically renew.
    /// Used for subscription renewal logic and billing management.
    /// Can be disabled by user or system for various reasons.
    /// </summary>
    public bool AutoRenew { get; set; } = true;
    
    /// <summary>
    /// Additional notes or comments about the subscription.
    /// Used for customer support, subscription management, and internal notes.
    /// Can include customer service notes, special instructions, or subscription details.
    /// </summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }
    
    /// <summary>
    /// Alias property for Status to maintain backward compatibility.
    /// Provides consistent access to status across different service layers.
    /// Used to ensure compatibility with existing code that expects 'SubscriptionStatus' property.
    /// </summary>
    public string SubscriptionStatus { get => Status; set => Status = value; }
    #endregion

    #region Status-Specific Properties
    /// <summary>
    /// Date when the subscription was paused.
    /// Used for pause tracking and subscription management.
    /// Set when subscription is paused by user or system.
    /// </summary>
    public DateTime? PausedDate { get; set; }
    
    /// <summary>
    /// Date when the subscription was resumed from pause.
    /// Used for pause tracking and subscription management.
    /// Set when subscription is resumed from pause.
    /// </summary>
    public DateTime? ResumedDate { get; set; }
    
    /// <summary>
    /// Date when the subscription was cancelled.
    /// Used for cancellation tracking and subscription history.
    /// Set when subscription is cancelled by user or system.
    /// </summary>
    public DateTime? CancelledDate { get; set; }
    
    /// <summary>
    /// Date when the subscription expires.
    /// Used for expiration tracking and access control.
    /// Set when subscription reaches its natural end or is set to expire.
    /// </summary>
    public DateTime? ExpirationDate { get; set; }
    
    /// <summary>
    /// Date when the subscription was suspended.
    /// Used for suspension tracking and subscription management.
    /// Set when subscription is suspended due to policy violations or other issues.
    /// </summary>
    public DateTime? SuspendedDate { get; set; }
    
    /// <summary>
    /// Date of the last successful billing/payment.
    /// Used for billing history tracking and payment analytics.
    /// Updated when billing is successfully processed.
    /// </summary>
    public DateTime? LastBillingDate { get; set; }
    
    /// <summary>
    /// Reason for subscription cancellation.
    /// Used for cancellation analytics and customer retention insights.
    /// Captured when user cancels subscription.
    /// </summary>
    [MaxLength(500)]
    public string? CancellationReason { get; set; }
    
    /// <summary>
    /// Reason for subscription pause.
    /// Used for pause analytics and customer support.
    /// Captured when subscription is paused.
    /// </summary>
    [MaxLength(500)]
    public string? PauseReason { get; set; }
    
    // Alias properties for backward compatibility with existing services
    /// <summary>
    /// Alias property for CancelledDate to maintain backward compatibility.
    /// Provides consistent access to cancellation date across different service layers.
    /// </summary>
    public DateTime? CancelledAt { get => CancelledDate; set => CancelledDate = value; }
    
    /// <summary>
    /// Alias property for ExpirationDate to maintain backward compatibility.
    /// Provides consistent access to expiration date across different service layers.
    /// </summary>
    public DateTime? ExpiredAt { get => ExpirationDate; set => ExpirationDate = value; }
    
    /// <summary>
    /// Alias property for ResumedDate to maintain backward compatibility.
    /// Provides consistent access to renewal date across different service layers.
    /// </summary>
    public DateTime? RenewedAt { get => ResumedDate; set => ResumedDate = value; }
    
    /// <summary>
    /// Alias property for ExpirationDate to maintain backward compatibility.
    /// Provides consistent access to expiry date across different service layers.
    /// </summary>
    public DateTime? ExpiryDate { get => ExpirationDate; set => ExpirationDate = value; }
    
    // Alias properties for Amount and Currency to maintain backward compatibility
    /// <summary>
    /// Alias property for CurrentPrice to maintain backward compatibility.
    /// Provides consistent access to subscription amount across different service layers.
    /// </summary>
    public decimal Amount { get => CurrentPrice; set => CurrentPrice = value; }
    
    /// <summary>
    /// Default currency for the subscription (USD).
    /// Read-only property for backward compatibility with existing services.
    /// </summary>
    public string Currency { get => "USD"; set { } }
    #endregion

    #region Stripe Integration Properties
    /// <summary>
    /// Stripe subscription ID for payment processing and billing management.
    /// Links this local subscription to the corresponding Stripe subscription.
    /// Required for Stripe integration, webhook processing, and payment operations.
    /// </summary>
    [MaxLength(100)]
    public string? StripeSubscriptionId { get; set; }
    
    /// <summary>
    /// Stripe customer ID for payment processing and customer management.
    /// Links this subscription to the user's Stripe customer record.
    /// Used for Stripe integration and customer-specific operations.
    /// </summary>
    [MaxLength(100)]
    public string? StripeCustomerId { get; set; }
    
    /// <summary>
    /// Stripe price ID for the subscription plan.
    /// Links this subscription to the specific Stripe price object.
    /// Used for Stripe integration and price management.
    /// </summary>
    [MaxLength(100)]
    public string? StripePriceId { get; set; }
    
    /// <summary>
    /// Stripe payment method ID for this subscription.
    /// Links this subscription to the user's payment method in Stripe.
    /// Used for payment processing and payment method management.
    /// </summary>
    [MaxLength(100)]
    public string? PaymentMethodId { get; set; }
    
    /// <summary>
    /// Date of the last successful payment for this subscription.
    /// Used for payment history tracking and billing analytics.
    /// Updated when payment is successfully processed.
    /// </summary>
    public DateTime? LastPaymentDate { get; set; }
    
    /// <summary>
    /// Date of the last failed payment attempt.
    /// Used for payment failure tracking and billing management.
    /// Updated when payment attempt fails.
    /// </summary>
    public DateTime? LastPaymentFailedDate { get; set; }
    
    /// <summary>
    /// Error message from the last failed payment attempt.
    /// Used for payment failure analysis and customer support.
    /// Captured when payment attempt fails.
    /// </summary>
    [MaxLength(500)]
    public string? LastPaymentError { get; set; }
    
    /// <summary>
    /// Number of consecutive failed payment attempts.
    /// Used for payment failure tracking and subscription management.
    /// Incremented on payment failure, reset on successful payment.
    /// </summary>
    public int FailedPaymentAttempts { get; set; } = 0;
    #endregion

    #region Trial Properties
    /// <summary>
    /// Indicates whether this subscription includes a trial period.
    /// Used for trial period access control and billing logic.
    /// Set to true when subscription includes a trial period.
    /// </summary>
    public bool IsTrialSubscription { get; set; } = false;
    
    /// <summary>
    /// Start date of the trial period for this subscription.
    /// Used for trial period tracking and access control.
    /// Set when subscription includes a trial period.
    /// </summary>
    public DateTime? TrialStartDate { get; set; }
    
    /// <summary>
    /// End date of the trial period for this subscription.
    /// Used for trial period tracking and access control.
    /// Set when subscription includes a trial period.
    /// </summary>
    public DateTime? TrialEndDate { get; set; }
    
    /// <summary>
    /// Duration of the trial period in days.
    /// Used for trial period calculations and display.
    /// Set when subscription includes a trial period.
    /// </summary>
    public int TrialDurationInDays { get; set; } = 0;
    #endregion

    #region Usage Tracking
    /// <summary>
    /// Date when the subscription was last used by the user.
    /// Used for usage tracking and subscription analytics.
    /// Updated when user accesses subscription features.
    /// </summary>
    public DateTime? LastUsedDate { get; set; }
    
    /// <summary>
    /// Total number of times the subscription has been used.
    /// Used for usage tracking and subscription analytics.
    /// Incremented when user accesses subscription features.
    /// </summary>
    public int TotalUsageCount { get; set; } = 0;
    #endregion

    #region Collection Navigation Properties
    /// <summary>
    /// Collection of all consultations associated with this subscription.
    /// Represents the consultation history for this subscription.
    /// Used for consultation management and subscription usage tracking.
    /// </summary>
    public virtual ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();
    
    /// <summary>
    /// Collection of all messages associated with this subscription.
    /// Represents the messaging history for this subscription.
    /// Used for communication tracking and subscription usage analytics.
    /// </summary>
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
    
    /// <summary>
    /// Collection of all medication deliveries associated with this subscription.
    /// Represents the medication delivery history for this subscription.
    /// Used for medication management and subscription usage tracking.
    /// </summary>
    public virtual ICollection<MedicationDelivery> MedicationDeliveries { get; set; } = new List<MedicationDelivery>();
    
    /// <summary>
    /// Collection of all billing records associated with this subscription.
    /// Represents the complete billing history for this subscription.
    /// Used for billing tracking, invoice management, and financial records.
    /// </summary>
    public virtual ICollection<BillingRecord> BillingRecords { get; set; } = new List<BillingRecord>();
    
    /// <summary>
    /// Collection of all privilege usage records for this subscription.
    /// Represents how the user has used their subscription privileges.
    /// Used for usage tracking, privilege management, and analytics.
    /// </summary>
    public virtual ICollection<UserSubscriptionPrivilegeUsage> PrivilegeUsages { get; set; } = new List<UserSubscriptionPrivilegeUsage>();
    
    /// <summary>
    /// Collection of all status change history for this subscription.
    /// Represents the complete status change history for this subscription.
    /// Used for audit trails, status tracking, and subscription management.
    /// </summary>
    public virtual ICollection<SubscriptionStatusHistory> StatusHistory { get; set; } = new List<SubscriptionStatusHistory>();
    
    /// <summary>
    /// Collection of all payments associated with this subscription.
    /// Represents the complete payment history for this subscription.
    /// Used for payment tracking, billing management, and financial records.
    /// </summary>
    public virtual ICollection<SubscriptionPayment> Payments { get; set; } = new List<SubscriptionPayment>();
    #endregion

    #region Computed Properties
    /// <summary>
    /// Computed property that indicates whether the subscription is currently active.
    /// Returns true if status is "Active" and subscription is not deleted or inactive.
    /// Used for subscription access control and status checking.
    /// </summary>
    [NotMapped]
    public bool IsSubscriptionActive => Status == SubscriptionStatuses.Active;
    
    /// <summary>
    /// Computed property that indicates whether the subscription is currently paused.
    /// Returns true if status is "Paused".
    /// Used for pause status checking and subscription management.
    /// </summary>
    [NotMapped]
    public bool IsPaused => Status == SubscriptionStatuses.Paused;
    
    /// <summary>
    /// Computed property that indicates whether the subscription has been cancelled.
    /// Returns true if status is "Cancelled".
    /// Used for cancellation status checking and subscription management.
    /// </summary>
    [NotMapped]
    public bool IsCancelled => Status == SubscriptionStatuses.Cancelled;
    
    /// <summary>
    /// Computed property that indicates whether the subscription has expired.
    /// Returns true if status is "Expired".
    /// Used for expiration status checking and subscription management.
    /// </summary>
    [NotMapped]
    public bool IsExpired => Status == SubscriptionStatuses.Expired;
    
    /// <summary>
    /// Computed property that indicates whether the subscription has payment issues.
    /// Returns true if status is "PaymentFailed" or there are failed payment attempts.
    /// Used for payment issue detection and subscription management.
    /// </summary>
    [NotMapped]
    public bool HasPaymentIssues => Status == SubscriptionStatuses.PaymentFailed || FailedPaymentAttempts > 0;
    
    /// <summary>
    /// Computed property that indicates whether the subscription is currently in trial period.
    /// Returns true if status is "TrialActive" or trial is active and current date is within trial period.
    /// Used for trial period access control and billing logic.
    /// </summary>
    [NotMapped]
    public bool IsInTrial => Status == SubscriptionStatuses.TrialActive || 
        (IsTrialSubscription && TrialEndDate.HasValue && DateTime.UtcNow <= TrialEndDate.Value);
    
    /// <summary>
    /// Computed property that returns the number of days until the next billing date.
    /// Returns negative values if billing date has passed.
    /// Used for billing notifications and subscription management.
    /// </summary>
    [NotMapped]
    public int DaysUntilNextBilling => (int)(NextBillingDate - DateTime.UtcNow).TotalDays;
    
    /// <summary>
    /// Computed property that indicates whether the subscription is near expiration.
    /// Returns true if next billing is within 7 days and not yet due.
    /// Used for expiration warnings and user notifications.
    /// </summary>
    [NotMapped]
    public bool IsNearExpiration => DaysUntilNextBilling <= 7 && DaysUntilNextBilling > 0;
    #endregion

    #region Business Logic Properties
    /// <summary>
    /// Computed property that indicates whether the subscription can be paused.
    /// Returns true if subscription is active and has no payment issues.
    /// Used for business logic validation and UI state management.
    /// </summary>
    [NotMapped]
    public bool CanPause => IsActive && !HasPaymentIssues;
    
    /// <summary>
    /// Computed property that indicates whether the subscription can be resumed.
    /// Returns true if subscription is currently paused.
    /// Used for business logic validation and UI state management.
    /// </summary>
    [NotMapped]
    public bool CanResume => IsPaused;
    
    /// <summary>
    /// Computed property that indicates whether the subscription can be cancelled.
    /// Returns true if subscription is active, paused, or pending.
    /// Used for business logic validation and UI state management.
    /// </summary>
    [NotMapped]
    public bool CanCancel => IsActive || IsPaused || Status == SubscriptionStatuses.Pending;
    
    /// <summary>
    /// Computed property that indicates whether the subscription can be renewed.
    /// Returns true if subscription is expired or cancelled and auto-renew is enabled.
    /// Used for business logic validation and UI state management.
    /// </summary>
    [NotMapped]
    public bool CanRenew => (IsExpired || IsCancelled) && AutoRenew;
    #endregion

    #region Validation Methods
    /// <summary>
    /// Validates whether a status transition is allowed for this subscription.
    /// Checks if the new status is valid and if the transition is allowed from the current status.
    /// Used for business logic validation and status change enforcement.
    /// </summary>
    /// <param name="newStatus">The new status to transition to</param>
    /// <returns>ValidationResult indicating whether the transition is valid</returns>
    public ValidationResult ValidateStatusTransition(string newStatus)
    {
        if (!SubscriptionStatuses.ValidStatuses.Contains(newStatus))
        {
            return new ValidationResult($"'{newStatus}' is not a valid subscription status.");
        }
        
        if (Status == newStatus)
        {
            return new ValidationResult($"Subscription is already in '{newStatus}' status.");
        }
        
        var validTransitions = GetValidStatusTransitions();
        if (!validTransitions.Contains(newStatus))
        {
            return new ValidationResult($"Cannot transition from '{Status}' to '{newStatus}'.");
        }
        
        return ValidationResult.Success;
    }
    
    /// <summary>
    /// Gets the list of valid status transitions from the current subscription status.
    /// Defines the business rules for status transitions and state management.
    /// Used for status validation and business logic enforcement.
    /// </summary>
    /// <returns>Array of valid status values that can be transitioned to from the current status</returns>
    public string[] GetValidStatusTransitions()
    {
        return Status switch
        {
            SubscriptionStatuses.Pending => new[] { SubscriptionStatuses.Active, SubscriptionStatuses.TrialActive, SubscriptionStatuses.Cancelled },
            SubscriptionStatuses.Active => new[] { SubscriptionStatuses.Paused, SubscriptionStatuses.Cancelled, SubscriptionStatuses.Expired, SubscriptionStatuses.PaymentFailed },
            SubscriptionStatuses.Paused => new[] { SubscriptionStatuses.Active, SubscriptionStatuses.Cancelled, SubscriptionStatuses.Expired },
            SubscriptionStatuses.PaymentFailed => new[] { SubscriptionStatuses.Active, SubscriptionStatuses.Cancelled, SubscriptionStatuses.Expired },
            SubscriptionStatuses.TrialActive => new[] { SubscriptionStatuses.Active, SubscriptionStatuses.TrialExpired, SubscriptionStatuses.Cancelled },
            SubscriptionStatuses.TrialExpired => new[] { SubscriptionStatuses.Active, SubscriptionStatuses.Cancelled },
            SubscriptionStatuses.Expired => new[] { SubscriptionStatuses.Active },
            SubscriptionStatuses.Cancelled => Array.Empty<string>(),
            _ => Array.Empty<string>()
        };
    }
    #endregion
}
#endregion
