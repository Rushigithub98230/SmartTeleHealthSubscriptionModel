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
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid BillingCycleId { get; set; }
        public Guid CurrencyId { get; set; }
    }

    public class CreateSubscriptionDto
    {
        public int UserId { get; set; }
        public string SubscriptionId { get; set; } = string.Empty;
        public string PlanId { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public Guid BillingCycleId { get; set; }
        public Guid CurrencyId { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? StartDate { get; set; }
        public bool StartImmediately { get; set; } = true;
        public string? PaymentMethodId { get; set; }
        public bool AutoRenew { get; set; } = true;
        // Removed: MonthlyPrice, QuarterlyPrice, AnnualPrice, BillingCycle
    }

    public class UpgradeSubscriptionDto
    {
        public string SubscriptionId { get; set; } = string.Empty;
        public int UserId { get; set; }
        [Required]
        public int NewPlanId { get; set; }
        [Required]
        public string PaymentMethodId { get; set; } = string.Empty;
        public bool Prorate { get; set; } = true;
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
        public DateTime CreatedAt { get; set; }
    }

    public class AddPaymentMethodDto
    {
        [Required]
        public string Token { get; set; } = string.Empty;
        public string PaymentMethodId { get; set; } = string.Empty;
        public bool IsDefault { get; set; } = false;
        public bool SetAsDefault { get; set; } = false;
        
        // Added missing properties to fix build errors
        public string Type { get; set; } = string.Empty;
        public string Last4 { get; set; } = string.Empty;
        public int ExpiryMonth { get; set; }
        public int ExpiryYear { get; set; }
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
        public DateTime CreatedAt { get; set; }
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
        public DateTime CreatedAt { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsSent { get; set; }
        public DateTime? SentAt { get; set; }
        public string? RecipientEmail { get; set; }
        public string? RecipientPhone { get; set; }
    }

    public class UpdateSubscriptionPlanDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public Guid BillingCycleId { get; set; }
        public Guid CurrencyId { get; set; }
        public bool IsActive { get; set; }
        
        // Marketing and display properties
        public bool IsMostPopular { get; set; } = false;
        public bool IsTrending { get; set; } = false;
        
        public int? DisplayOrder { get; set; } // Added property
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
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
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
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
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