using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces;

/// <summary>
/// Service for detecting and reporting data inconsistencies in subscription management system
/// Includes subscriptions without billing, orphaned billing records, and Stripe/local status mismatches
/// </summary>
public interface IReconciliationService
{
    /// <summary>
    /// Detects all reconciliation issues across the subscription domain
    /// </summary>
    /// <returns>JsonModel containing detected issues with details and counts</returns>
    Task<JsonModel> DetectReconciliationIssuesAsync();

    /// <summary>
    /// Detects subscriptions that are missing expected billing records
    /// </summary>
    Task<ReconciliationReport> DetectSubscriptionsWithoutBillingAsync();

    /// <summary>
    /// Detects billing records that reference non-existent subscriptions
    /// </summary>
    Task<ReconciliationReport> DetectOrphanedBillingRecordsAsync();

    /// <summary>
    /// Detects mismatches between Stripe subscription status and local database status
    /// </summary>
    Task<ReconciliationReport> DetectStatusMismatchesAsync();

    /// <summary>
    /// Generates a comprehensive reconciliation report with all detected issues
    /// </summary>
    Task<ReconciliationReport> GenerateFullReportAsync();
}

/// <summary>
/// Detailed report of reconciliation issues detected
/// </summary>
public class ReconciliationReport
{
    public DateTime GeneratedAt { get; set; }
    public int TotalIssuesFound { get; set; }
    
    // Subscriptions without billing
    public List<SubscriptionWithoutBillingIssue> SubscriptionsWithoutBilling { get; set; } = new();
    public int SubscriptionsWithoutBillingCount => SubscriptionsWithoutBilling.Count;
    
    // Orphaned billing records
    public List<OrphanedBillingIssue> OrphanedBillingRecords { get; set; } = new();
    public int OrphanedBillingRecordsCount => OrphanedBillingRecords.Count;
    
    // Status mismatches
    public List<StatusMismatchIssue> StatusMismatches { get; set; } = new();
    public int StatusMismatchesCount => StatusMismatches.Count;

    public bool HasIssues => TotalIssuesFound > 0;
}

public class SubscriptionWithoutBillingIssue
{
    public Guid SubscriptionId { get; set; }
    public int UserId { get; set; }
    public string? UserEmail { get; set; }
    public Guid PlanId { get; set; }
    public string? PlanName { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? NextBillingDate { get; set; }
    public string? StripeSubscriptionId { get; set; }
    public string IssueDescription { get; set; } = string.Empty;
}

public class OrphanedBillingIssue
{
    public Guid BillingRecordId { get; set; }
    public string? SubscriptionId { get; set; }
    public int UserId { get; set; }
    public string? UserEmail { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime BillingDate { get; set; }
    public string IssueDescription { get; set; } = string.Empty;
}

public class StatusMismatchIssue
{
    public Guid SubscriptionId { get; set; }
    public int UserId { get; set; }
    public string? UserEmail { get; set; }
    public string LocalStatus { get; set; } = string.Empty;
    public string StripeStatus { get; set; } = string.Empty;
    public string? StripeSubscriptionId { get; set; }
    public DateTime? LastUpdated { get; set; }
    public string IssueDescription { get; set; } = string.Empty;
}

