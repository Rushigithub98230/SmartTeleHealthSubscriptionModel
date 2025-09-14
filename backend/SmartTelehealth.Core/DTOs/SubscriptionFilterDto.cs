using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Core.DTOs;

/// <summary>
/// DTO for comprehensive filtering of subscriptions with all possible filter parameters.
/// This DTO consolidates all filtering options into a single, reusable structure.
/// </summary>
public class SubscriptionFilterDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
    public int Page { get; set; } = 1;

    [Range(1, 200, ErrorMessage = "PageSize must be between 1 and 200")]
    public int PageSize { get; set; } = 50;

    public string? SearchTerm { get; set; }
    public Guid? SubscriptionId { get; set; }
    public Guid? PlanId { get; set; }
    public string? PlanName { get; set; }
    public int? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string? Status { get; set; }
    public List<string>? Statuses { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsTrial { get; set; }
    public bool? IsPaused { get; set; }
    public bool? IsCancelled { get; set; }
    public bool? IsExpired { get; set; }

    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public decimal? ExactAmount { get; set; }
    public Guid? CurrencyId { get; set; }

    public Guid? BillingCycleId { get; set; }
    public string? BillingCycleName { get; set; }

    public DateTime? CreatedDateFrom { get; set; }
    public DateTime? CreatedDateTo { get; set; }
    public DateTime? UpdatedDateFrom { get; set; }
    public DateTime? UpdatedDateTo { get; set; }
    public DateTime? StartDateFrom { get; set; }
    public DateTime? StartDateTo { get; set; }
    public DateTime? EndDateFrom { get; set; }
    public DateTime? EndDateTo { get; set; }
    public DateTime? NextBillingDateFrom { get; set; }
    public DateTime? NextBillingDateTo { get; set; }
    public DateTime? LastBillingDateFrom { get; set; }
    public DateTime? LastBillingDateTo { get; set; }

    public int? MinTrialDays { get; set; }
    public int? MaxTrialDays { get; set; }
    public int? MinBillingInterval { get; set; }
    public int? MaxBillingInterval { get; set; }

    public string? StripeSubscriptionId { get; set; }
    public string? StripeCustomerId { get; set; }
    public bool? HasStripeIntegration { get; set; }

    public List<Guid>? SubscriptionIds { get; set; }
    public List<Guid>? ExcludeSubscriptionIds { get; set; }
    public List<Guid>? PlanIds { get; set; }
    public List<int>? UserIds { get; set; }

    public bool? HasActivePayments { get; set; }
    public bool? HasFailedPayments { get; set; }
    public bool? HasPendingPayments { get; set; }
    public bool? HasRefunds { get; set; }

    public string? PaymentMethodType { get; set; }
    public string? PaymentStatus { get; set; }

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
