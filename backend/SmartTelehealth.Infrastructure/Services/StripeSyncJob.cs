using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Core.Enums;

namespace SmartTelehealth.Infrastructure.Services;

/// <summary>
/// Background service that performs hourly Stripe synchronization to ensure data consistency
/// between Stripe and local database.
/// 
/// Phase 3: Background Sync & Reconciliation
/// 
/// Operations:
/// - Syncs all subscriptions from Stripe to local database
/// - Updates subscription statuses, billing dates, and cancellation flags
/// - Detects orphaned Stripe subscriptions
/// - Runs every hour to catch any missed webhook events
/// - Provides reconciliation for webhook failures
/// </summary>
public class StripeSyncJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<StripeSyncJob> _logger;
    private readonly TimeSpan _syncInterval = TimeSpan.FromHours(1); // Run every hour
    private readonly bool _isEnabled = true; // Can be configured via appsettings

    public StripeSyncJob(
        IServiceProvider serviceProvider,
        ILogger<StripeSyncJob> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🚀 Stripe Sync Job started - Running every {Interval}", _syncInterval);

        if (!_isEnabled)
        {
            _logger.LogWarning("⚠️ Stripe Sync Job is disabled. Exiting.");
            return;
        }

        // Wait 2 minutes after startup before first sync (allow app to fully initialize)
        await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("🔄 Starting scheduled Stripe synchronization at {Time}", DateTime.UtcNow);

                using (var scope = _serviceProvider.CreateScope())
                {
                    var stripeSyncService = scope.ServiceProvider
                        .GetRequiredService<IStripeSynchronizationService>();
                    
                    // Use admin token for system operations
                    var tokenModel = new TokenModel 
                    { 
                        UserID = 1, // System user
                        RoleID = (int)RoleId.Admin 
                    };

                    // Perform subscription sync
                    var syncResult = await stripeSyncService.SyncAllSubscriptionsFromStripeAsync(tokenModel);

                    if (syncResult.StatusCode == 200)
                    {
                        _logger.LogInformation(
                            "✅ Scheduled Stripe sync completed successfully: {Message}", 
                            syncResult.Message);
                    }
                    else
                    {
                        _logger.LogWarning("⚠️ Scheduled Stripe sync completed with warnings: {Message}", 
                            syncResult.Message);
                    }

                    // Optionally run customer ID consistency check (once per day)
                    if (DateTime.UtcNow.Hour == 3) // 3 AM
                    {
                        _logger.LogInformation("🔍 Running daily customer ID consistency check...");
                        
                        var consistencyResult = await stripeSyncService
                            .CheckCustomerIdConsistencyAsync(tokenModel);
                        
                        if (consistencyResult.StatusCode == 200)
                        {
                            _logger.LogInformation("✅ Customer ID consistency check completed: {Message}", 
                                consistencyResult.Message);
                        }
                        else
                        {
                            _logger.LogWarning("⚠️ Customer ID consistency check had issues: {Message}", 
                                consistencyResult.Message);
                        }
                    }
                }

                // Wait for next sync interval
                _logger.LogInformation("⏰ Next Stripe sync scheduled in {Interval} at {NextRun}", 
                    _syncInterval, DateTime.UtcNow.Add(_syncInterval));
                
                await Task.Delay(_syncInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("🛑 Stripe Sync Job is stopping (cancellation requested)");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ CRITICAL ERROR in Stripe Sync Job. Retrying in 5 minutes.");
                
                // On error, retry after 5 minutes instead of 1 hour
                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("🛑 Stripe Sync Job stopping during error recovery");
                    break;
                }
            }
        }

        _logger.LogInformation("✅ Stripe Sync Job stopped gracefully");
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("⏹️ Stripe Sync Job stop requested");
        await base.StopAsync(cancellationToken);
    }
}

