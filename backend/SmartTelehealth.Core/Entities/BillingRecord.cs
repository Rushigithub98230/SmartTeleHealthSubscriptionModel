using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

/// <summary>
/// Core billing record entity that manages all billing and payment records in the system.
/// This entity handles billing management including payment tracking, invoice management, and financial records.
/// It serves as the central hub for all billing operations including subscriptions, consultations, medications,
/// and other services. The entity includes comprehensive payment status tracking, Stripe integration,
/// and financial record management with support for various billing types and statuses.
/// </summary>
public class BillingRecord : BaseEntity
{
    /// <summary>
    /// Primary key identifier for the billing record.
    /// Uses Guid for better scalability and security in distributed systems.
    /// Unique identifier for each billing record in the system.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Enumeration of all possible billing statuses for billing records.
    /// Defines the current state of a billing record in the payment process.
    /// Used for billing status tracking and payment management.
    /// </summary>
    public enum BillingStatus
    {
        /// <summary>Billing record is created but payment has not been processed</summary>
        Pending,
        /// <summary>Payment has been successfully processed and confirmed</summary>
        Paid,
        /// <summary>Payment attempt failed and needs to be retried</summary>
        Failed,
        /// <summary>Billing record has been cancelled and will not be processed</summary>
        Cancelled,
        /// <summary>Payment has been refunded to the customer</summary>
        Refunded,
        /// <summary>Payment is past due date and requires attention</summary>
        Overdue,
        /// <summary>Billing record is scheduled for future processing</summary>
        Upcoming
    }
    
    /// <summary>
    /// Enumeration of all possible billing types for billing records.
    /// Defines the category and purpose of the billing record.
    /// Used for billing categorization and management.
    /// </summary>
    public enum BillingType
    {
        /// <summary>Billing for subscription services</summary>
        Subscription,
        /// <summary>Billing for consultation services</summary>
        Consultation,
        /// <summary>Billing for medication delivery services</summary>
        Medication,
        /// <summary>Billing for late payment fees</summary>
        LateFee,
        /// <summary>Billing for refunds and credits</summary>
        Refund,
        /// <summary>Billing for recurring services</summary>
        Recurring,
        /// <summary>Billing for upfront payments</summary>
        Upfront,
        /// <summary>Billing for bundled services</summary>
        Bundle,
        /// <summary>Billing for invoice-based services</summary>
        Invoice,
        /// <summary>Billing for billing cycle services</summary>
        Cycle
    }
    
    // Foreign keys
    /// <summary>
    /// Foreign key reference to the User who is being billed.
    /// Links this billing record to the specific user account.
    /// Required for user-specific billing management and payment tracking.
    /// </summary>
    public int UserId { get; set; }
    
    /// <summary>
    /// Navigation property to the User who is being billed.
    /// Provides access to user information for billing management.
    /// Used for user-specific billing operations and payment tracking.
    /// </summary>
    public virtual User User { get; set; } = null!;
    
    /// <summary>
    /// Foreign key reference to the Subscription that this billing record is for.
    /// Links this billing record to the specific subscription.
    /// Optional - used for subscription-based billing.
    /// </summary>
    public Guid? SubscriptionId { get; set; }
    
    /// <summary>
    /// Navigation property to the Subscription that this billing record is for.
    /// Provides access to subscription information for billing management.
    /// Used for subscription-based billing operations.
    /// </summary>
    public virtual Subscription? Subscription { get; set; }
    
    /// <summary>
    /// Foreign key reference to the Consultation that this billing record is for.
    /// Links this billing record to the specific consultation.
    /// Optional - used for consultation-based billing.
    /// </summary>
    public Guid? ConsultationId { get; set; }
    
    /// <summary>
    /// Navigation property to the Consultation that this billing record is for.
    /// Provides access to consultation information for billing management.
    /// Used for consultation-based billing operations.
    /// </summary>
    public virtual Consultation? Consultation { get; set; }
    
    /// <summary>
    /// Foreign key reference to the MedicationDelivery that this billing record is for.
    /// Links this billing record to the specific medication delivery.
    /// Optional - used for medication delivery billing.
    /// </summary>
    public Guid? MedicationDeliveryId { get; set; }
    
    /// <summary>
    /// Navigation property to the MedicationDelivery that this billing record is for.
    /// Provides access to medication delivery information for billing management.
    /// Used for medication delivery billing operations.
    /// </summary>
    public virtual MedicationDelivery? MedicationDelivery { get; set; }
    
    /// <summary>
    /// Foreign key reference to the BillingCycle that this billing record belongs to.
    /// Links this billing record to the specific billing cycle.
    /// Optional - used for billing cycle management.
    /// </summary>
    public Guid? BillingCycleId { get; set; }
    
    /// <summary>
    /// Foreign key reference to the Currency for this billing record.
    /// Determines the currency for billing and payment processing.
    /// Required for international billing and currency management.
    /// </summary>
    public Guid CurrencyId { get; set; }
    
    /// <summary>
    /// Navigation property to the Currency for this billing record.
    /// Provides access to currency information and exchange rates.
    /// Used for international billing and currency management.
    /// </summary>
    public virtual MasterCurrency Currency { get; set; } = null!;
    
    // Billing details
    /// <summary>
    /// Current status of the billing record in the payment process.
    /// Determines the billing record's state and payment status.
    /// Used for billing status tracking and payment management.
    /// </summary>
    public BillingStatus Status { get; set; } = BillingStatus.Pending;
    
    /// <summary>
    /// Type of billing record indicating the category and purpose.
    /// Determines the billing record's category and management approach.
    /// Used for billing categorization and management.
    /// </summary>
    public BillingType Type { get; set; } = BillingType.Subscription;
    
    /// <summary>
    /// Base amount for this billing record before taxes and fees.
    /// Used for billing calculations and payment processing.
    /// This is the core amount being billed for the service.
    /// </summary>
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Tax amount for this billing record.
    /// Used for tax calculations and compliance.
    /// Added to the base amount for total billing calculation.
    /// </summary>
    public decimal TaxAmount { get; set; }
    
    /// <summary>
    /// Shipping amount for this billing record.
    /// Used for shipping cost calculations and delivery billing.
    /// Added to the base amount for total billing calculation.
    /// </summary>
    public decimal ShippingAmount { get; set; }
    
    /// <summary>
    /// Total amount for this billing record including all fees and taxes.
    /// Used for final billing calculations and payment processing.
    /// This is the final amount that will be charged to the customer.
    /// </summary>
    public decimal TotalAmount { get; set; }
    
    /// <summary>
    /// Date when this billing record was created.
    /// Used for billing history tracking and payment scheduling.
    /// Set when the billing record is first created.
    /// </summary>
    public DateTime BillingDate { get; set; }
    
    /// <summary>
    /// Date and time when payment was successfully processed.
    /// Used for payment history tracking and billing analytics.
    /// Set when payment is successfully confirmed.
    /// </summary>
    public DateTime? PaidAt { get; set; }
    
    /// <summary>
    /// Date when payment is due for this billing record.
    /// Used for payment scheduling and overdue tracking.
    /// Set when the billing record is created or payment is scheduled.
    /// </summary>
    public DateTime? DueDate { get; set; }
    
    /// <summary>
    /// Unique invoice number for this billing record.
    /// Used for invoice tracking and customer reference.
    /// Generated when the billing record is created or processed.
    /// </summary>
    [MaxLength(100)]
    public string? InvoiceNumber { get; set; }
    
    /// <summary>
    /// Stripe payment intent ID for this billing record.
    /// Links this billing record to the corresponding Stripe payment intent.
    /// Used for Stripe integration and payment processing.
    /// </summary>
    [MaxLength(100)]
    public string? StripePaymentIntentId { get; set; }
    
    /// <summary>
    /// Stripe invoice ID for this billing record.
    /// Links this billing record to the corresponding Stripe invoice.
    /// Used for Stripe integration and invoice management.
    /// </summary>
    [MaxLength(100)]
    public string? StripeInvoiceId { get; set; }
    
    /// <summary>
    /// Description of this billing record and what it covers.
    /// Used for billing information display and customer communication.
    /// Provides context about what the billing record is for.
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Reason for payment failure if applicable.
    /// Used for payment failure analysis and customer support.
    /// Captured when payment attempt fails.
    /// </summary>
    [MaxLength(500)]
    public string? FailureReason { get; set; }
    
    /// <summary>
    /// Payment method used for this billing record.
    /// Used for payment method tracking and customer communication.
    /// Captured when payment is processed.
    /// </summary>
    [MaxLength(100)]
    public string? PaymentMethod { get; set; }
    
    /// <summary>
    /// Transaction ID for this billing record.
    /// Used for transaction tracking and payment processing.
    /// Generated when payment is processed.
    /// </summary>
    [MaxLength(100)]
    public string? TransactionId { get; set; }
    
    /// <summary>
    /// Error message for this billing record if applicable.
    /// Used for error tracking and customer support.
    /// Captured when errors occur during billing processing.
    /// </summary>
    [MaxLength(500)]
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Date and time when this billing record was processed.
    /// Used for billing processing tracking and analytics.
    /// Set when the billing record is successfully processed.
    /// </summary>
    public DateTime? ProcessedAt { get; set; }
    
    /// <summary>
    /// Indicates whether this billing record is for recurring services.
    /// Used for recurring billing management and payment scheduling.
    /// Set to true for subscription-based or recurring service billing.
    /// </summary>
    public bool IsRecurring { get; set; } = false;
    
    /// <summary>
    /// Date when the next billing is scheduled for recurring services.
    /// Used for recurring billing management and payment scheduling.
    /// Set for recurring billing records to schedule next billing.
    /// </summary>
    public DateTime? NextBillingDate { get; set; }
    
    /// <summary>
    /// Payment intent ID for this billing record.
    /// Used for payment processing and transaction tracking.
    /// Generated when payment intent is created.
    /// </summary>
    public string? PaymentIntentId { get; set; }
    
    /// <summary>
    /// Accrued amount for this billing record if applicable.
    /// Used for accrual-based billing and financial calculations.
    /// Set when billing is based on accrued usage or time.
    /// </summary>
    public decimal? AccruedAmount { get; set; }
    
    /// <summary>
    /// Start date for accrual period if applicable.
    /// Used for accrual-based billing and financial calculations.
    /// Set when billing is based on accrued usage or time.
    /// </summary>
    public DateTime? AccrualStartDate { get; set; }
    
    /// <summary>
    /// End date for accrual period if applicable.
    /// Used for accrual-based billing and financial calculations.
    /// Set when billing is based on accrued usage or time.
    /// </summary>
    public DateTime? AccrualEndDate { get; set; }
    
    // Navigation properties
    /// <summary>
    /// Collection of all billing adjustments associated with this billing record.
    /// Represents the complete history of billing adjustments and modifications.
    /// Used for billing adjustment tracking and financial record management.
    /// </summary>
    public virtual ICollection<BillingAdjustment> Adjustments { get; set; } = new List<BillingAdjustment>();
    
    // Computed Properties
    /// <summary>
    /// Computed property that indicates whether this billing record has been paid.
    /// Returns true if status is "Paid", indicating successful payment.
    /// Used for payment status checking and billing management.
    /// </summary>
    [NotMapped]
    public bool IsPaid => Status == BillingStatus.Paid;
    
    /// <summary>
    /// Computed property that indicates whether this billing record payment has failed.
    /// Returns true if status is "Failed", indicating payment failure.
    /// Used for payment failure checking and billing management.
    /// </summary>
    [NotMapped]
    public bool IsFailed => Status == BillingStatus.Failed;
    
    /// <summary>
    /// Computed property that indicates whether this billing record has been refunded.
    /// Returns true if status is "Refunded", indicating payment refund.
    /// Used for refund status checking and billing management.
    /// </summary>
    [NotMapped]
    public bool IsRefunded => Status == BillingStatus.Refunded;
    
    /// <summary>
    /// Computed property that indicates whether this billing record is overdue.
    /// Returns true if due date has passed and payment has not been made.
    /// Used for overdue checking and billing management.
    /// </summary>
    [NotMapped]
    public bool IsOverdue => DueDate.HasValue && DateTime.UtcNow > DueDate.Value && !IsPaid;
} 