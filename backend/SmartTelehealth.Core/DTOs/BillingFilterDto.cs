using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Core.DTOs;

/// <summary>
/// DTO for comprehensive filtering of billing records with all possible filter parameters.
/// This DTO consolidates all filtering options into a single, reusable structure.
/// </summary>
public class BillingFilterDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
    public int Page { get; set; } = 1;

    [Range(1, 200, ErrorMessage = "PageSize must be between 1 and 200")]
    public int PageSize { get; set; } = 50;

    public string? SearchTerm { get; set; }
    public Guid? BillingRecordId { get; set; }
    public Guid? SubscriptionId { get; set; }
    public int? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string? Status { get; set; }
    public List<string>? Statuses { get; set; }
    public string? Type { get; set; }
    public List<string>? Types { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsPaid { get; set; }
    public bool? IsOverdue { get; set; }
    public bool? IsPending { get; set; }
    public bool? IsFailed { get; set; }
    public bool? IsRefunded { get; set; }

    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public decimal? ExactAmount { get; set; }
    public Guid? CurrencyId { get; set; }

    public DateTime? CreatedDateFrom { get; set; }
    public DateTime? CreatedDateTo { get; set; }
    public DateTime? UpdatedDateFrom { get; set; }
    public DateTime? UpdatedDateTo { get; set; }
    public DateTime? DueDateFrom { get; set; }
    public DateTime? DueDateTo { get; set; }
    public DateTime? PaidDateFrom { get; set; }
    public DateTime? PaidDateTo { get; set; }
    public DateTime? ProcessedDateFrom { get; set; }
    public DateTime? ProcessedDateTo { get; set; }

    public string? PaymentMethod { get; set; }
    public string? PaymentStatus { get; set; }
    public string? PaymentMethodType { get; set; }
    public string? TransactionId { get; set; }
    public string? StripeInvoiceId { get; set; }
    public string? StripePaymentIntentId { get; set; }
    public string? StripeChargeId { get; set; }

    public bool? HasStripeIntegration { get; set; }
    public bool? HasPaymentMethod { get; set; }
    public bool? HasTransactionId { get; set; }

    public List<Guid>? BillingRecordIds { get; set; }
    public List<Guid>? ExcludeBillingRecordIds { get; set; }
    public List<Guid>? SubscriptionIds { get; set; }
    public List<int>? UserIds { get; set; }

    public int? MinRetryCount { get; set; }
    public int? MaxRetryCount { get; set; }
    public int? MinFailureCount { get; set; }
    public int? MaxFailureCount { get; set; }

    public string? FailureReason { get; set; }
    public string? Notes { get; set; }
    public string? Description { get; set; }

    public bool? IsRecurring { get; set; }
    public bool? IsOneTime { get; set; }
    public bool? IsAdjustment { get; set; }
    public bool? IsRefund { get; set; }

    public string? BillingCycle { get; set; }
    public Guid? BillingCycleId { get; set; }

    public DateTime? LastRetryDateFrom { get; set; }
    public DateTime? LastRetryDateTo { get; set; }
    public DateTime? NextRetryDateFrom { get; set; }
    public DateTime? NextRetryDateTo { get; set; }

    public string SortColumn { get; set; } = "CreatedDate";
    public string SortOrder { get; set; } = "desc"; // "asc" or "desc"

    public bool IsValid()
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(this);
        return Validator.TryValidateObject(this, validationContext, validationResults, true);
    }

    public IEnumerable<string> GetValidationErrors()
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(this);
        Validator.TryValidateObject(this, validationContext, validationResults, true);
        return validationResults.Select(r => r.ErrorMessage ?? string.Empty);
    }
}
