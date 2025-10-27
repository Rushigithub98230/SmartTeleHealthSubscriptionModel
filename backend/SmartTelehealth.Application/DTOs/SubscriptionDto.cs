using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Application.DTOs
{
    public class SubscriptionDto
    {
        public string Id { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string PlanId { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public string PlanDescription { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? CustomerId { get; set; }
        public DateTime? CurrentPeriodStart { get; set; }
        public DateTime? CurrentPeriodEnd { get; set; }
        public string? StatusReason { get; set; }
        public decimal CurrentPrice { get; set; }
        public bool AutoRenew { get; set; }
        public string? Notes { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime NextBillingDate { get; set; }
        public DateTime? PausedDate { get; set; }
        public DateTime? ResumedDate { get; set; }
        public DateTime? CancelledDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string? CancellationReason { get; set; }
        public string? PauseReason { get; set; }
        public string? StripeSubscriptionId { get; set; }
        public string? StripeCustomerId { get; set; }
        public string? PaymentMethodId { get; set; }
        public DateTime? LastPaymentDate { get; set; }
        public DateTime? LastPaymentFailedDate { get; set; }
        public string? LastPaymentError { get; set; }
        public int FailedPaymentAttempts { get; set; }
        public bool IsTrialSubscription { get; set; }
        public DateTime? TrialStartDate { get; set; }
        public DateTime? TrialEndDate { get; set; }
        public int TrialDurationInDays { get; set; }
        public DateTime? LastUsedDate { get; set; }
        public int TotalUsageCount { get; set; }
        public List<SubscriptionStatusHistoryDto> StatusHistory { get; set; } = new();
        public List<SubscriptionPaymentDto> Payments { get; set; } = new();
        public bool IsActive { get; set; }
        public bool IsPaused { get; set; }
        public bool IsCancelled { get; set; }
        public bool IsExpired { get; set; }
        public bool HasPaymentIssues { get; set; }
        public bool IsInTrial { get; set; }
        public int DaysUntilNextBilling { get; set; }
        public bool IsNearExpiration { get; set; }
        public bool CanPause { get; set; }
        public bool CanResume { get; set; }
        public bool CanCancel { get; set; }
        public bool CanRenew { get; set; }
        public decimal UsagePercentage { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public Guid BillingCycleId { get; set; }
        public Guid CurrencyId { get; set; }
    }

    public class CreateSubscriptionDto
    {
        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }
        
        public string SubscriptionId { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Plan ID is required")]
        public string PlanId { get; set; } = string.Empty;
        
        public string? Name { get; set; }
        public string? Description { get; set; }
        
        [Range(0, double.MaxValue, ErrorMessage = "Price must be 0 or positive")]
        public decimal Price { get; set; }
        
        // NEW ARCHITECTURE: BillingCycleId removed - billing cycle comes from the selected plan
        // Each plan has a fixed billing cycle. Users select a complete plan (e.g., "Basic - Monthly")
        // OLD: public Guid BillingCycleId { get; set; }
        
        [Required(ErrorMessage = "Currency ID is required")]
        public Guid CurrencyId { get; set; }
        
        public bool IsActive { get; set; } = true;
        public DateTime? StartDate { get; set; }
        public bool StartImmediately { get; set; } = true;
        
        [Required(ErrorMessage = "Payment method ID is required")]
        public string? PaymentMethodId { get; set; }
        
        public bool AutoRenew { get; set; } = true;
        // Removed: MonthlyPrice, QuarterlyPrice, AnnualPrice, BillingCycle
    }

    public class UpgradeSubscriptionDto
    {
        [Required(ErrorMessage = "Subscription ID is required")]
        public string SubscriptionId { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }
        
        [Required(ErrorMessage = "New plan ID is required")]
        public string NewPlanId { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Payment method ID is required")]
        public string PaymentMethodId { get; set; } = string.Empty;
        
        public bool Prorate { get; set; } = true;
    }

    public class DowngradeSubscriptionDto
    {
        [Required(ErrorMessage = "Subscription ID is required")]
        public string SubscriptionId { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }
        
        [Required(ErrorMessage = "New plan ID is required")]
        public string NewPlanId { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Payment method ID is required")]
        public string PaymentMethodId { get; set; } = string.Empty;
        
        public bool Prorate { get; set; } = true;
    }

    public class ExtendSubscriptionDto
    {
        [Required]
        public DateTime NewEndDate { get; set; }
        public string? Reason { get; set; }
    }

    public class BillingHistoryDto
    {
        public string Id { get; set; } = string.Empty;
        public string SubscriptionId { get; set; } = string.Empty;
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime BillingDate { get; set; }
        public DateTime? PaidAt { get; set; }
        public string? TransactionId { get; set; }
        public string? FailureReason { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public string? StripeInvoiceId { get; set; }
        public string? StripePaymentIntentId { get; set; }
        public DateTime CreatedDate { get; set; }
    }


    public class ProcessPaymentRequestDto
    {
        public Guid BillingRecordId { get; set; }
        public string? PaymentMethodId { get; set; }
    }

    public class ValidatePaymentMethodDto
    {
        public string PaymentMethodId { get; set; } = string.Empty;
    }

    public class SubscriptionBenefitDto
    {
        public string Id { get; set; } = string.Empty;
        public string SubscriptionId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string BenefitName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string BenefitType { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int Limit { get; set; }
        public int UsedQuantity { get; set; }
        public int Used { get; set; }
        public int RemainingQuantity { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class PauseSubscriptionDto
    {
        [Required]
        public string Reason { get; set; } = string.Empty;
        public DateTime? ResumeDate { get; set; }
        public DateTime? PauseDate { get; set; }
    }

    public class SubscriptionReminderDto
    {
        public string Id { get; set; } = string.Empty;
        public string SubscriptionId { get; set; } = string.Empty;
        public string ReminderType { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTime ScheduledAt { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsSent { get; set; }
        public DateTime? SentAt { get; set; }
        public string? RecipientEmail { get; set; }
        public string? RecipientPhone { get; set; }
    }

    public class UpdateSubscriptionPlanDto
    {
        [Required(ErrorMessage = "Plan ID is required")]
        public string Id { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Plan name is required")]
        [MaxLength(100, ErrorMessage = "Plan name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }
        
        [Required(ErrorMessage = "Base price is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Base price must be 0 or positive")]
        public decimal BasePrice { get; set; }
        
        [Required(ErrorMessage = "Billing cycle ID is required")]
        public Guid BillingCycleId { get; set; }
        
        [Required(ErrorMessage = "Currency ID is required")]
        public Guid CurrencyId { get; set; }
        
        [Required(ErrorMessage = "Category ID is required")]
        public Guid CategoryId { get; set; }
        
        public bool IsActive { get; set; }
        
        // Marketing and display properties
        public bool IsMostPopular { get; set; } = false;
        public bool IsTrending { get; set; } = false;
        
        [Range(0, int.MaxValue, ErrorMessage = "Display order must be non-negative")]
        public int? DisplayOrder { get; set; }
        
        // Healthcare pricing model
        public bool IsAutoCalculatedPrice { get; set; } = true;
        
        [Range(0, 100, ErrorMessage = "Commission must be between 0 and 100%")]
        public decimal? AdminCommissionPercent { get; set; }
        
        [Range(7, 365, ErrorMessage = "Notice period must be between 7 and 365 days")]
        public int PriceChangeNoticeDays { get; set; } = 10;
        
        [Range(0, 100, ErrorMessage = "Billing discount must be between 0 and 100%")]
        public decimal? BillingDiscountPercentage { get; set; }
    
    /// <summary>
    /// Promotional discount percentage (0-100)
    /// </summary>
    public decimal? DiscountPercentage { get; set; }
    
    /// <summary>
    /// Promotional discount valid until date
    /// </summary>
    public DateTime? DiscountValidUntil { get; set; }
    }

    public class SubscriptionStatusHistoryDto
    {
        public string Id { get; set; } = string.Empty;
        public string SubscriptionId { get; set; } = string.Empty;
        public string FromStatus { get; set; } = string.Empty;
        public string ToStatus { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public string? ChangedByUserId { get; set; }
        public DateTime ChangedAt { get; set; }
        public string? Metadata { get; set; }
        
        // Backward compatibility properties
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
    public class SubscriptionPaymentDto
    {
        public string Id { get; set; } = string.Empty;
        public string SubscriptionId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal NetAmount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? FailureReason { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? FailedAt { get; set; }
        public DateTime BillingPeriodStart { get; set; }
        public DateTime BillingPeriodEnd { get; set; }
        public string? StripePaymentIntentId { get; set; }
        public string? StripeInvoiceId { get; set; }
        public string? ReceiptUrl { get; set; }
        public string? PaymentIntentId { get; set; }
        public string? InvoiceId { get; set; }
        public int AttemptCount { get; set; }
        public DateTime? NextRetryAt { get; set; }
        public decimal RefundedAmount { get; set; }
        public List<PaymentRefundDto> Refunds { get; set; } = new();
        public bool IsPaid { get; set; }
        public bool IsFailed { get; set; }
        public bool IsRefunded { get; set; }
        public bool IsOverdue { get; set; }
        public decimal RemainingAmount { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
    public class PaymentRefundDto
    {
        public string Id { get; set; } = string.Empty;
        public string SubscriptionPaymentId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? StripeRefundId { get; set; }
        public DateTime RefundedAt { get; set; }
        public string? ProcessedByUserId { get; set; }
    }
} 