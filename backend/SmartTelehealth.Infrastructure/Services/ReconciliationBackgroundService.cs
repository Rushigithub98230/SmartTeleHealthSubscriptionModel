using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.Interfaces;

namespace SmartTelehealth.Infrastructure.Services;

/// <summary>
/// Background service that performs nightly reconciliation checks to detect data inconsistencies
/// Runs daily at 2 AM to avoid peak load times
/// 
/// Detects:
/// - Subscriptions without billing records
/// - Orphaned billing records (billing without subscriptions)
/// - Status mismatches between Stripe and local database
/// 
/// Generates reports and logs critical issues for admin attention
/// </summary>
public class ReconciliationBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ReconciliationBackgroundService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(24); // Run daily
    private readonly bool _isEnabled = true; // Can be configured via appsettings

    public ReconciliationBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<ReconciliationBackgroundService> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🚀 Reconciliation Background Service started - Running daily at 2 AM");

        if (!_isEnabled)
        {
            _logger.LogWarning("⚠️ Reconciliation Background Service is disabled. Exiting.");
            return;
        }

        // Wait 5 minutes after startup before first check (allow app to fully initialize)
        await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Run reconciliation at 2 AM UTC (adjust for your timezone if needed)
                var now = DateTime.UtcNow;
                var nextRun = new DateTime(now.Year, now.Month, now.Day, 2, 0, 0, DateTimeKind.Utc);
                
                // If it's already past 2 AM today, schedule for tomorrow
                if (now >= nextRun)
                {
                    nextRun = nextRun.AddDays(1);
                }

                var delayUntilNextRun = nextRun - now;
                
                _logger.LogInformation(
                    "⏰ Next reconciliation check scheduled for {NextRun} (in {Hours} hours, {Minutes} minutes)",
                    nextRun, delayUntilNextRun.TotalHours.ToString("F1"), delayUntilNextRun.Minutes);

                await Task.Delay(delayUntilNextRun, stoppingToken);

                // Perform reconciliation check
                _logger.LogInformation("🔍 Starting nightly reconciliation check at {Time}", DateTime.UtcNow);

                using (var scope = _serviceProvider.CreateScope())
                {
                    var reconciliationService = scope.ServiceProvider
                        .GetRequiredService<IReconciliationService>();

                    // Run comprehensive reconciliation check
                    var reportResult = await reconciliationService.DetectReconciliationIssuesAsync();

                    if (reportResult.StatusCode == 200 && reportResult.data is ReconciliationReport report)
                    {
                        if (report.HasIssues)
                        {
                            _logger.LogWarning(
                                "⚠️ RECONCILIATION ISSUES DETECTED: {TotalIssues} issue(s) found. " +
                                "Subscriptions without billing: {SubsWithoutBilling}, " +
                                "Orphaned billing records: {OrphanedBilling}, " +
                                "Status mismatches: {StatusMismatches}",
                                report.TotalIssuesFound,
                                report.SubscriptionsWithoutBillingCount,
                                report.OrphanedBillingRecordsCount,
                                report.StatusMismatchesCount);

                            // Log detailed issues for admin review
                            LogDetailedIssues(report);
                        }
                        else
                        {
                            _logger.LogInformation(
                                "✅ Reconciliation check completed: No issues detected. All systems in sync.");
                        }
                    }
                    else
                    {
                        _logger.LogError(
                            "❌ Reconciliation check failed with status {StatusCode}: {Message}",
                            reportResult.StatusCode, reportResult.Message);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("🛑 Reconciliation Background Service is stopping (cancellation requested)");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ CRITICAL ERROR in Reconciliation Background Service. Will retry on next scheduled run.");
                
                // On error, wait until next scheduled run (24 hours)
                try
                {
                    var now = DateTime.UtcNow;
                    var nextRun = new DateTime(now.Year, now.Month, now.Day, 2, 0, 0, DateTimeKind.Utc).AddDays(1);
                    var delayUntilNextRun = nextRun - now;
                    
                    await Task.Delay(delayUntilNextRun, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("🛑 Reconciliation Background Service stopping during error recovery");
                    break;
                }
            }
        }

        _logger.LogInformation("✅ Reconciliation Background Service stopped gracefully");
    }

    /// <summary>
    /// Logs detailed reconciliation issues for admin review
    /// </summary>
    private void LogDetailedIssues(ReconciliationReport report)
    {
        // Log subscriptions without billing
        if (report.SubscriptionsWithoutBilling.Any())
        {
            _logger.LogWarning("📋 SUBSCRIPTIONS WITHOUT BILLING ({Count}):", report.SubscriptionsWithoutBillingCount);
            foreach (var issue in report.SubscriptionsWithoutBilling.Take(10)) // Log first 10 to avoid log spam
            {
                _logger.LogWarning(
                    "  - Subscription {SubscriptionId} (User: {UserId}, Plan: {PlanName}, Status: {Status}) - {Description}",
                    issue.SubscriptionId, issue.UserId, issue.PlanName, issue.Status, issue.IssueDescription);
            }
            if (report.SubscriptionsWithoutBillingCount > 10)
            {
                _logger.LogWarning("  ... and {MoreCount} more", report.SubscriptionsWithoutBillingCount - 10);
            }
        }

        // Log orphaned billing records
        if (report.OrphanedBillingRecords.Any())
        {
            _logger.LogWarning("💰 ORPHANED BILLING RECORDS ({Count}):", report.OrphanedBillingRecordsCount);
            foreach (var issue in report.OrphanedBillingRecords.Take(10))
            {
                _logger.LogWarning(
                    "  - Billing Record {BillingId} (SubscriptionId: {SubscriptionId}, User: {UserId}, Amount: ${Amount}) - {Description}",
                    issue.BillingRecordId, issue.SubscriptionId, issue.UserId, issue.Amount, issue.IssueDescription);
            }
            if (report.OrphanedBillingRecordsCount > 10)
            {
                _logger.LogWarning("  ... and {MoreCount} more", report.OrphanedBillingRecordsCount - 10);
            }
        }

        // Log status mismatches
        if (report.StatusMismatches.Any())
        {
            _logger.LogWarning("🔄 STATUS MISMATCHES ({Count}):", report.StatusMismatchesCount);
            foreach (var issue in report.StatusMismatches.Take(10))
            {
                _logger.LogWarning(
                    "  - Subscription {SubscriptionId} (User: {UserId}): Local={LocalStatus}, Stripe={StripeStatus} - {Description}",
                    issue.SubscriptionId, issue.UserId, issue.LocalStatus, issue.StripeStatus, issue.IssueDescription);
            }
            if (report.StatusMismatchesCount > 10)
            {
                _logger.LogWarning("  ... and {MoreCount} more", report.StatusMismatchesCount - 10);
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("⏹️ Reconciliation Background Service stop requested");
        await base.StopAsync(cancellationToken);
    }
}

