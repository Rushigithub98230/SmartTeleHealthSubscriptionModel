using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Application.DTOs;

public class UpdateSubscriptionDto
{
    public string? Status { get; set; }
    public decimal? CurrentPrice { get; set; }
    public DateTime? NextBillingDate { get; set; }
    public DateTime? LastPaymentDate { get; set; }
    public DateTime? LastPaymentFailedDate { get; set; }
    public string? LastPaymentError { get; set; }
    public int? FailedPaymentAttempts { get; set; }
    public string? StripeSubscriptionId { get; set; }
    public string? StripeCustomerId { get; set; }
    public string? PaymentMethodId { get; set; }
    public DateTime? CancelledDate { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime? PausedDate { get; set; }
    public string? PauseReason { get; set; }
    public DateTime? ResumedDate { get; set; }
    public DateTime? ExpiredDate { get; set; }
    public DateTime? RenewedAt { get; set; }
    public DateTime? LastUsedDate { get; set; }
    public int? TotalUsageCount { get; set; }
    public bool? AutoRenew { get; set; }
    public Guid? SubscriptionPlanId { get; set; }
    public DateTime? TrialEndDate { get; set; }
}
