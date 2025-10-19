using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Core.DTOs;

namespace SmartTelehealth.Infrastructure.Services;

/// <summary>
/// Background service that processes scheduled plan migrations at user renewal dates.
/// Healthcare Feature: Migrates users at their individual renewal dates, not a fixed grace period.
/// Runs daily at 2 AM to check for due migrations.
/// </summary>
public class ScheduledMigrationBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ScheduledMigrationBackgroundService> _logger;
    private readonly TimeSpan _runInterval = TimeSpan.FromHours(24); // Run once per day
    private readonly TimeSpan _targetRunTime = new TimeSpan(2, 0, 0); // 2 AM

    public ScheduledMigrationBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<ScheduledMigrationBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Scheduled Migration Background Service started");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Calculate delay until next 2 AM
                var now = DateTime.Now;
                var nextRun = now.Date.Add(_targetRunTime);
                
                if (now.TimeOfDay > _targetRunTime)
                {
                    // If past 2 AM today, schedule for 2 AM tomorrow
                    nextRun = nextRun.AddDays(1);
                }
                
                var delay = nextRun - now;
                
                _logger.LogInformation(
                    "Scheduled Migration Background Service: Next run at {NextRun} (in {Hours}h {Minutes}m)",
                    nextRun, delay.Hours, delay.Minutes);
                
                // Wait until 2 AM
                await Task.Delay(delay, stoppingToken);
                
                // Process migrations
                await ProcessDueMigrationsAsync();
                
                // After processing, wait a bit to avoid running twice
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Scheduled Migration Background Service is stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in scheduled migration processor. Retrying in 5 minutes.");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
        
        _logger.LogInformation("Scheduled Migration Background Service stopped");
    }

    private async Task ProcessDueMigrationsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        
        var migrationRepository = scope.ServiceProvider
            .GetRequiredService<IScheduledPlanMigrationRepository>();
        var subscriptionRepository = scope.ServiceProvider
            .GetRequiredService<ISubscriptionRepository>();
        var subscriptionPlanRepository = scope.ServiceProvider
            .GetRequiredService<ISubscriptionPlanRepository>();
        var stripeService = scope.ServiceProvider
            .GetRequiredService<SmartTelehealth.Application.Interfaces.IStripeService>();
        var unitOfWork = scope.ServiceProvider
            .GetRequiredService<IUnitOfWork>();
        
        try
        {
            _logger.LogInformation("Processing scheduled migrations for {Date}", DateTime.UtcNow.Date);
            
            var dueMigrations = await migrationRepository.GetMigrationsDueByDateAsync(DateTime.UtcNow);
            var pendingMigrations = dueMigrations.Where(m => m.Status == "Pending").ToList();
            
            _logger.LogInformation("Found {Count} migrations due for processing", pendingMigrations.Count);
            
            var successCount = 0;
            var failureCount = 0;
            
            foreach (var migration in pendingMigrations)
            {
                try
                {
                    await ProcessSingleMigrationAsync(
                        migration,
                        subscriptionRepository,
                        subscriptionPlanRepository,
                        stripeService,
                        unitOfWork);
                    
                    migration.Status = "Completed";
                    migration.CompletedDate = DateTime.UtcNow;
                    await migrationRepository.UpdateAsync(migration);
                    
                    successCount++;
                    
                    _logger.LogInformation(
                        "✅ Completed migration {MigrationId} for subscription {SubId}",
                        migration.Id, migration.SubscriptionId);
                }
                catch (Exception ex)
                {
                    migration.Status = "Failed";
                    migration.Notes = $"Error: {ex.Message}";
                    await migrationRepository.UpdateAsync(migration);
                    
                    failureCount++;
                    
                    _logger.LogError(ex,
                        "❌ Failed migration {MigrationId} for subscription {SubId}",
                        migration.Id, migration.SubscriptionId);
                }
            }
            
            _logger.LogInformation(
                "Migration processing complete. Success: {Success}, Failed: {Failed}",
                successCount, failureCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing scheduled migrations");
        }
    }

    private async Task ProcessSingleMigrationAsync(
        ScheduledPlanMigration migration,
        ISubscriptionRepository subscriptionRepository,
        ISubscriptionPlanRepository subscriptionPlanRepository,
        SmartTelehealth.Application.Interfaces.IStripeService stripeService,
        IUnitOfWork unitOfWork)
    {
        await unitOfWork.BeginTransactionAsync();
        
        try
        {
            var subscription = await subscriptionRepository.GetByIdWithDetailsAsync(migration.SubscriptionId);
            if (subscription == null)
            {
                throw new InvalidOperationException($"Subscription {migration.SubscriptionId} not found");
            }
            
            var targetPlanId = migration.DowngradeToPlanId ?? migration.ToPlanId;
            var targetPlan = await subscriptionPlanRepository.GetByIdWithDetailsAsync(targetPlanId);
            
            if (targetPlan == null)
            {
                throw new InvalidOperationException($"Target plan {targetPlanId} not found");
            }
            
            _logger.LogInformation(
                "Migrating subscription {SubId} from plan {OldPlan} v{OldVer} to {NewPlan} v{NewVer}",
                subscription.Id, migration.FromPlan.Name, migration.FromPlan.VersionNumber,
                targetPlan.Name, targetPlan.VersionNumber);
            
            // Update subscription to new plan
            subscription.SubscriptionPlanId = targetPlan.Id;
            subscription.CurrentPrice = targetPlan.Price;
            subscription.UpdatedBy = 0; // System automated
            subscription.UpdatedDate = DateTime.UtcNow;
            
            // Update in Stripe if Stripe subscription exists
            if (!string.IsNullOrEmpty(subscription.StripeSubscriptionId))
            {
                try
                {
                    // Create a system token for Stripe operations
                    var systemToken = new TokenModel { UserID = 0, RoleID = 1 };
                    
                    // Get the appropriate Stripe price ID for the billing cycle
                    var stripePriceId = subscription.BillingCycle.Name.ToLower() switch
                    {
                        "monthly" => targetPlan.StripeMonthlyPriceId,
                        "quarterly" => targetPlan.StripeQuarterlyPriceId,
                        "annual" => targetPlan.StripeAnnualPriceId,               // ONLY "annual" (database standard)
                        _ => targetPlan.StripeMonthlyPriceId
                    };
                    
                    if (string.IsNullOrEmpty(stripePriceId))
                    {
                        _logger.LogWarning(
                            "No Stripe price ID found for plan {PlanId} and billing cycle {Cycle}. Skipping Stripe update.",
                            targetPlan.Id, subscription.BillingCycle.Name);
                    }
                    else
                    {
                        // Update Stripe subscription to new plan/price
                        await stripeService.UpdateSubscriptionAsync(
                            subscription.StripeSubscriptionId,
                            stripePriceId,
                            systemToken);
                        
                        subscription.StripePriceId = stripePriceId;
                        
                        _logger.LogInformation(
                            "Updated Stripe subscription {StripeSubId} to price {PriceId}",
                            subscription.StripeSubscriptionId, stripePriceId);
                    }
                }
                catch (Exception stripeEx)
                {
                    _logger.LogError(stripeEx,
                        "Failed to update Stripe subscription {StripeSubId}. Migration will continue with local update.",
                        subscription.StripeSubscriptionId);
                    // Continue with local update even if Stripe fails
                }
            }
            
            await subscriptionRepository.UpdateSubscriptionAsync(subscription);
            await unitOfWork.CommitTransactionAsync();
            
            _logger.LogInformation(
                "Successfully migrated subscription {SubId} to plan {PlanId}",
                subscription.Id, targetPlan.Id);
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error processing migration {MigrationId}", migration.Id);
            throw;
        }
    }
}

