using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Core.Enums;

namespace SmartTelehealth.Application.Services;

/// <summary>
/// Service for detecting and reporting data inconsistencies in subscription management system
/// Detects: subscriptions without billing, orphaned billing records, Stripe/local status mismatches
/// </summary>
public class ReconciliationService : IReconciliationService
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IBillingRepository _billingRepository;
    private readonly IStripeService _stripeService;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<ReconciliationService> _logger;

    /// <summary>
    /// Creates a system token for background service operations
    /// </summary>
    private TokenModel GetSystemToken() => new TokenModel
    {
        UserID = 1, // System user
        RoleID = (int)RoleId.Admin
    };

    public ReconciliationService(
        ISubscriptionRepository subscriptionRepository,
        IBillingRepository billingRepository,
        IStripeService stripeService,
        IUserRepository userRepository,
        ILogger<ReconciliationService> logger)
    {
        _subscriptionRepository = subscriptionRepository ?? throw new ArgumentNullException(nameof(subscriptionRepository));
        _billingRepository = billingRepository ?? throw new ArgumentNullException(nameof(billingRepository));
        _stripeService = stripeService ?? throw new ArgumentNullException(nameof(stripeService));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Detects all reconciliation issues across the subscription domain
    /// </summary>
    public async Task<JsonModel> DetectReconciliationIssuesAsync()
    {
        try
        {
            _logger.LogInformation("🔍 Starting comprehensive reconciliation check at {Time}", DateTime.UtcNow);

            var report = await GenerateFullReportAsync();

            _logger.LogInformation(
                "✅ Reconciliation check completed. Issues found: {TotalIssues} " +
                "(Subscriptions without billing: {SubsWithoutBilling}, " +
                "Orphaned billing: {OrphanedBilling}, " +
                "Status mismatches: {StatusMismatches})",
                report.TotalIssuesFound,
                report.SubscriptionsWithoutBillingCount,
                report.OrphanedBillingRecordsCount,
                report.StatusMismatchesCount);

            return new JsonModel
            {
                data = report,
                Message = report.HasIssues
                    ? $"Detected {report.TotalIssuesFound} reconciliation issue(s)"
                    : "No reconciliation issues detected",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error during reconciliation check");
            return new JsonModel
            {
                data = new object(),
                Message = $"Error during reconciliation: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    /// <summary>
    /// Detects subscriptions that are missing expected billing records
    /// Active subscriptions should have at least one billing record (initial payment)
    /// Subscriptions with NextBillingDate in the past should have billing records
    /// </summary>
    public async Task<ReconciliationReport> DetectSubscriptionsWithoutBillingAsync()
    {
        var report = new ReconciliationReport { GeneratedAt = DateTime.UtcNow };

        try
        {
            _logger.LogInformation("🔍 Checking for subscriptions without billing records...");

            // Get all active, trial, and paused subscriptions (these should have billing records)
            var activeSubscriptions = await _subscriptionRepository.GetActiveSubscriptionsAsync();
            var allSubscriptions = await _subscriptionRepository.GetAllSubscriptionsAsync();

            // Filter to subscriptions that should have billing records
            var subscriptionsNeedingBilling = allSubscriptions.Where(s =>
                s.Status == Subscription.SubscriptionStatuses.Active ||
                s.Status == Subscription.SubscriptionStatuses.TrialActive ||
                s.Status == Subscription.SubscriptionStatuses.Paused ||
                s.Status == Subscription.SubscriptionStatuses.PaymentFailed
            ).ToList();

            _logger.LogInformation("Checking {Count} subscriptions for billing records", subscriptionsNeedingBilling.Count);

            foreach (var subscription in subscriptionsNeedingBilling)
            {
                try
                {
                    // Check if billing records exist for this subscription
                    var billingRecords = await _billingRepository.GetBySubscriptionIdAsync(subscription.Id);
                    
                    if (!billingRecords.Any())
                    {
                        // This subscription has no billing records - this is an issue
                        var user = await _userRepository.GetByIdAsync(subscription.UserId);
                        var plan = await _subscriptionRepository.GetSubscriptionPlanByIdAsync(subscription.SubscriptionPlanId);

                        var issue = new SubscriptionWithoutBillingIssue
                        {
                            SubscriptionId = subscription.Id,
                            UserId = subscription.UserId,
                            UserEmail = user?.Email,
                            PlanId = subscription.SubscriptionPlanId,
                            PlanName = plan?.Name,
                            Status = subscription.Status,
                            StartDate = subscription.StartDate,
                            NextBillingDate = subscription.NextBillingDate,
                            StripeSubscriptionId = subscription.StripeSubscriptionId,
                            IssueDescription = $"Subscription {subscription.Id} has status '{subscription.Status}' but no billing records found. " +
                                              $"Started on {subscription.StartDate:yyyy-MM-dd}, next billing due {subscription.NextBillingDate:yyyy-MM-dd}"
                        };

                        report.SubscriptionsWithoutBilling.Add(issue);
                        _logger.LogWarning(
                            "⚠️ Subscription {SubscriptionId} (User: {UserId}, Status: {Status}) has no billing records",
                            subscription.Id, subscription.UserId, subscription.Status);
                    }
                    else
                    {
                        // Also check if subscription is past due for billing
                        if (subscription.NextBillingDate < DateTime.UtcNow.AddDays(-7) &&
                            !billingRecords.Any(br => br.BillingDate >= subscription.NextBillingDate.AddDays(-30)))
                        {
                            // Subscription should have a recent billing record but doesn't
                            var user = await _userRepository.GetByIdAsync(subscription.UserId);
                            var plan = await _subscriptionRepository.GetSubscriptionPlanByIdAsync(subscription.SubscriptionPlanId);
                            
                            var issue = new SubscriptionWithoutBillingIssue
                            {
                                SubscriptionId = subscription.Id,
                                UserId = subscription.UserId,
                                UserEmail = user?.Email,
                                PlanId = subscription.SubscriptionPlanId,
                                PlanName = plan?.Name,
                                Status = subscription.Status,
                                StartDate = subscription.StartDate,
                                NextBillingDate = subscription.NextBillingDate,
                                StripeSubscriptionId = subscription.StripeSubscriptionId,
                                IssueDescription = $"Subscription {subscription.Id} is past due for billing. " +
                                                  $"Next billing date was {subscription.NextBillingDate:yyyy-MM-dd}, but no recent billing record found."
                            };

                            report.SubscriptionsWithoutBilling.Add(issue);
                            _logger.LogWarning(
                                "⚠️ Subscription {SubscriptionId} is past due for billing. Last billing: {LastBilling}, Next due: {NextBilling}",
                                subscription.Id,
                                billingRecords.OrderByDescending(br => br.BillingDate).FirstOrDefault()?.BillingDate,
                                subscription.NextBillingDate);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking subscription {SubscriptionId} for billing records", subscription.Id);
                }
            }

            report.TotalIssuesFound = report.SubscriptionsWithoutBillingCount;
            _logger.LogInformation("Found {Count} subscriptions without expected billing records", report.SubscriptionsWithoutBillingCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting subscriptions without billing");
        }

        return report;
    }

    /// <summary>
    /// Detects billing records that reference non-existent subscriptions
    /// </summary>
    public async Task<ReconciliationReport> DetectOrphanedBillingRecordsAsync()
    {
        var report = new ReconciliationReport { GeneratedAt = DateTime.UtcNow };

        try
        {
            _logger.LogInformation("🔍 Checking for orphaned billing records...");

            // Get all billing records
            var allBillingRecords = await _billingRepository.GetAllWithDetailsAsync();
            
            foreach (var billingRecord in allBillingRecords)
            {
                try
                {
                    // Check if SubscriptionId exists and is valid
                    if (billingRecord.SubscriptionId == null || billingRecord.SubscriptionId == Guid.Empty)
                    {
                        // Billing record without subscription ID - could be orphaned or one-time payment
                        continue; // Skip - might be intentional (one-time charges)
                    }

                    var subscriptionId = billingRecord.SubscriptionId.Value;

                    // Check if subscription exists
                    var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
                    if (subscription == null)
                    {
                        // Billing record references non-existent subscription
                        var user = await _userRepository.GetByIdAsync(billingRecord.UserId);
                        
                        var issue = new OrphanedBillingIssue
                        {
                            BillingRecordId = billingRecord.Id,
                            SubscriptionId = subscriptionId.ToString(),
                            UserId = billingRecord.UserId,
                            UserEmail = user?.Email,
                            Amount = billingRecord.TotalAmount,
                            Status = billingRecord.Status.ToString(),
                            BillingDate = billingRecord.BillingDate,
                            IssueDescription = $"Billing record {billingRecord.Id} references non-existent subscription {subscriptionId}"
                        };

                        report.OrphanedBillingRecords.Add(issue);
                        _logger.LogWarning("⚠️ Billing record {BillingId} references non-existent subscription: {SubscriptionId}",
                            billingRecord.Id, subscriptionId);
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking billing record {BillingId}", billingRecord.Id);
                }
            }

            report.TotalIssuesFound = report.OrphanedBillingRecordsCount;
            _logger.LogInformation("Found {Count} orphaned billing records", report.OrphanedBillingRecordsCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting orphaned billing records");
        }

        return report;
    }

    /// <summary>
    /// Detects mismatches between Stripe subscription status and local database status
    /// </summary>
    public async Task<ReconciliationReport> DetectStatusMismatchesAsync()
    {
        var report = new ReconciliationReport { GeneratedAt = DateTime.UtcNow };

        try
        {
            _logger.LogInformation("🔍 Checking for status mismatches between Stripe and local database...");

            // Get all subscriptions with Stripe subscription IDs
            var allSubscriptions = await _subscriptionRepository.GetAllSubscriptionsAsync();
            var subscriptionsWithStripe = allSubscriptions
                .Where(s => !string.IsNullOrEmpty(s.StripeSubscriptionId))
                .ToList();

            _logger.LogInformation("Checking {Count} subscriptions with Stripe IDs for status mismatches", subscriptionsWithStripe.Count);

            foreach (var localSubscription in subscriptionsWithStripe)
            {
                try
                {
                    // Get Stripe subscription status using system token
                    var systemToken = GetSystemToken();
                    var stripeSubscription = await _stripeService.GetSubscriptionAsync(localSubscription.StripeSubscriptionId!, systemToken);
                    
                    if (stripeSubscription == null)
                    {
                        _logger.LogWarning("Stripe subscription {StripeId} not found for local subscription {LocalId}",
                            localSubscription.StripeSubscriptionId, localSubscription.Id);
                        continue;
                    }

                    // Map Stripe status to local status
                    var expectedLocalStatus = MapStripeStatusToLocal(stripeSubscription.Status);
                    var actualLocalStatus = localSubscription.Status;

                    if (expectedLocalStatus != actualLocalStatus)
                    {
                        var user = await _userRepository.GetByIdAsync(localSubscription.UserId);
                        
                        var issue = new StatusMismatchIssue
                        {
                            SubscriptionId = localSubscription.Id,
                            UserId = localSubscription.UserId,
                            UserEmail = user?.Email,
                            LocalStatus = actualLocalStatus,
                            StripeStatus = stripeSubscription.Status,
                            StripeSubscriptionId = localSubscription.StripeSubscriptionId,
                            LastUpdated = localSubscription.UpdatedDate,
                            IssueDescription = $"Status mismatch: Local={actualLocalStatus}, Stripe={stripeSubscription.Status}. " +
                                              $"Local subscription should be updated to match Stripe."
                        };

                        report.StatusMismatches.Add(issue);
                        _logger.LogWarning(
                            "⚠️ Status mismatch for subscription {SubscriptionId}: Local={LocalStatus}, Stripe={StripeStatus}",
                            localSubscription.Id, actualLocalStatus, stripeSubscription.Status);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking status for subscription {SubscriptionId} with Stripe ID {StripeId}",
                        localSubscription.Id, localSubscription.StripeSubscriptionId);
                }
            }

            report.TotalIssuesFound = report.StatusMismatchesCount;
            _logger.LogInformation("Found {Count} status mismatches between Stripe and local database", report.StatusMismatchesCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting status mismatches");
        }

        return report;
    }

    /// <summary>
    /// Generates a comprehensive reconciliation report with all detected issues
    /// </summary>
    public async Task<ReconciliationReport> GenerateFullReportAsync()
    {
        _logger.LogInformation("📊 Generating comprehensive reconciliation report...");

        var report = new ReconciliationReport { GeneratedAt = DateTime.UtcNow };

        // Run all detection methods in parallel for efficiency
        var tasks = new List<Task<ReconciliationReport>>
        {
            DetectSubscriptionsWithoutBillingAsync(),
            DetectOrphanedBillingRecordsAsync(),
            DetectStatusMismatchesAsync()
        };

        var results = await Task.WhenAll(tasks);

        // Combine all results
        report.SubscriptionsWithoutBilling = results[0].SubscriptionsWithoutBilling;
        report.OrphanedBillingRecords = results[1].OrphanedBillingRecords;
        report.StatusMismatches = results[2].StatusMismatches;

        report.TotalIssuesFound = 
            report.SubscriptionsWithoutBillingCount +
            report.OrphanedBillingRecordsCount +
            report.StatusMismatchesCount;

        _logger.LogInformation(
            "📊 Reconciliation report generated: {TotalIssues} total issues " +
            "(Subscriptions without billing: {SubsWithout}, " +
            "Orphaned billing: {Orphaned}, " +
            "Status mismatches: {Mismatches})",
            report.TotalIssuesFound,
            report.SubscriptionsWithoutBillingCount,
            report.OrphanedBillingRecordsCount,
            report.StatusMismatchesCount);

        return report;
    }

    /// <summary>
    /// Maps Stripe subscription status to local subscription status
    /// </summary>
    private string MapStripeStatusToLocal(string stripeStatus)
    {
        return stripeStatus?.ToLower() switch
        {
            "active" => Subscription.SubscriptionStatuses.Active,
            "canceled" => Subscription.SubscriptionStatuses.Cancelled,
            "past_due" => Subscription.SubscriptionStatuses.PaymentFailed,
            "unpaid" => Subscription.SubscriptionStatuses.PaymentFailed,
            "trialing" => Subscription.SubscriptionStatuses.TrialActive,
            "paused" => Subscription.SubscriptionStatuses.Paused,
            _ => Subscription.SubscriptionStatuses.Active
        };
    }
}

