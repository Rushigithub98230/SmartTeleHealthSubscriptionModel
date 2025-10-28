using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Application.Utilities;

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
                        unitOfWork,
                        scope.ServiceProvider);
                    
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
        IUnitOfWork unitOfWork,
        IServiceProvider serviceProvider)
    {
        await unitOfWork.BeginTransactionAsync();
        
        try
        {
            var subscription = await subscriptionRepository.GetByIdWithDetailsAsync(migration.SubscriptionId);
            if (subscription == null)
            {
                throw new InvalidOperationException($"Subscription {migration.SubscriptionId} not found");
            }
            
            // Check user decision - handle cancellation
            if (migration.UserDecision == "Cancel")
            {
                _logger.LogInformation(
                    "User rejected migration for subscription {SubId}. Marking for cancellation at renewal.",
                    subscription.Id);
                
                // Mark subscription for auto-cancel at renewal
                subscription.PendingCancellationAtRenewal = true;
                subscription.PendingCancellationReason = "User rejected plan version migration";
                
                // Update migration status
                migration.Status = "UserOptedOut";
                migration.CompletedDate = DateTime.UtcNow;
                
                await subscriptionRepository.UpdateAsync(subscription);
                await unitOfWork.CommitTransactionAsync();
                
                _logger.LogInformation(
                    "Subscription {SubId} marked for cancellation at next renewal",
                    subscription.Id);
                
                return; // Don't proceed with migration
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
            
            // Get system default commission for price calculation
            var systemSettingsRepo = serviceProvider.GetRequiredService<ISystemSettingsRepository>();
            var systemSettings = await systemSettingsRepo.GetSettingsAsync();
            var defaultCommission = systemSettings?.DefaultAdminCommissionPercent ?? 0;
            
            // Update subscription to new plan
            subscription.SubscriptionPlanId = targetPlan.Id;
            
            // Use calculated effective price instead of stored BasePrice
            subscription.CurrentPrice = BillingCalculationService.GetEffectivePlanPrice(
                targetPlan, 
                defaultCommission,
                _logger);
            
            _logger.LogInformation(
                "Migration: Calculated effective price for subscription {SubId}: ${Price}",
                subscription.Id, subscription.CurrentPrice);
            
            subscription.UpdatedBy = 0; // System automated
            subscription.UpdatedDate = DateTime.UtcNow;
            
            // Update in Stripe if Stripe subscription exists
            if (!string.IsNullOrEmpty(subscription.StripeSubscriptionId))
            {
                try
                {
                    // Create a system token for Stripe operations
                    var systemToken = new TokenModel { UserID = 0, RoleID = 1 };
                    
                    // NEW ARCHITECTURE: Simply use the plan's single Stripe price ID
                    var stripePriceId = targetPlan.StripePriceId;
                    if (!string.IsNullOrEmpty(stripePriceId))
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
                    else
                    {
                        _logger.LogWarning("No Stripe price ID configured for target plan {PlanId}. Skipping Stripe update.", targetPlan.Id);
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
            
            await subscriptionRepository.UpdateAsync(subscription);
            
            // CRITICAL FIX (Issue #13): Synchronize privileges from new plan version
            // This ensures users get NEW privileges added in the updated plan version
            await SyncPrivilegesToNewPlanAsync(subscription, targetPlan, serviceProvider);
            
            await unitOfWork.CommitTransactionAsync();
            
            _logger.LogInformation(
                "Successfully migrated subscription {SubId} to plan {PlanId} with privilege synchronization",
                subscription.Id, targetPlan.Id);
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error processing migration {MigrationId}", migration.Id);
            throw;
        }
    }
    
    /// <summary>
    /// Synchronizes user privileges to new plan version during migration.
    /// Creates new privilege usage records for privileges added in the new plan version,
    /// and updates existing privilege allocations to match new plan values.
    /// </summary>
    private async Task SyncPrivilegesToNewPlanAsync(
        Subscription subscription,
        SubscriptionPlan newPlan,
        IServiceProvider serviceProvider)
    {
        var privilegeUsageRepository = serviceProvider
            .GetRequiredService<IUserSubscriptionPrivilegeUsageRepository>();
        
        _logger.LogInformation("Synchronizing privileges for subscription {SubId} to plan {PlanName} v{Version}",
            subscription.Id, newPlan.Name, newPlan.VersionNumber);
        
        // Get user's current privilege usages
        var currentUsages = await privilegeUsageRepository.GetBySubscriptionIdAsync(subscription.Id);
        
        // Get new plan's active privileges
        var newPlanPrivileges = newPlan.PlanPrivileges.Where(pp => pp.IsActive && !pp.IsDeleted);
        
        var newPrivilegesAdded = 0;
        var existingPrivilegesUpdated = 0;
        
        foreach (var newPlanPrivilege in newPlanPrivileges)
        {
            // Check if user already has usage record for this plan privilege
            var existingUsage = currentUsages
                .FirstOrDefault(u => u.SubscriptionPlanPrivilegeId == newPlanPrivilege.Id);
            
            if (existingUsage == null)
            {
                // NEW PRIVILEGE - Create usage record
                // Calculate allocation using current subscription dates
                var periodStart = subscription.LastBillingDate ?? subscription.StartDate;
                var periodEnd = subscription.NextBillingDate;
                var allowedValue = newPlanPrivilege.Value;
                
                var newUsage = new UserSubscriptionPrivilegeUsage
                {
                    Id = Guid.NewGuid(),
                    SubscriptionId = subscription.Id,
                    SubscriptionPlanPrivilegeId = newPlanPrivilege.Id,
                    UsedValue = 0,  // Start fresh
                    AllowedValue = allowedValue,
                    UsagePeriodStart = periodStart,
                    UsagePeriodEnd = periodEnd,
                    ResetAt = DateTime.UtcNow,
                    IsActive = true,
                    CreatedBy = 0,  // System automated
                    CreatedDate = DateTime.UtcNow,
                    UpdatedBy = 0,
                    UpdatedDate = DateTime.UtcNow
                };
                
                await privilegeUsageRepository.AddAsync(newUsage);
                newPrivilegesAdded++;
                
                _logger.LogInformation("Created new privilege usage for {PrivilegeName} (Value: {Value}) during migration",
                    newPlanPrivilege.Privilege?.Name ?? "Unknown", allowedValue);
            }
            else
            {
                // EXISTING PRIVILEGE - Update allocation from new plan version
                var allowedValue = newPlanPrivilege.Value;
                
                existingUsage.AllowedValue = allowedValue;
                existingUsage.SubscriptionPlanPrivilegeId = newPlanPrivilege.Id;  // Update FK to new version
                existingUsage.UpdatedBy = 0;  // System automated
                existingUsage.UpdatedDate = DateTime.UtcNow;
                
                await privilegeUsageRepository.UpdateUsageAsync(existingUsage);
                existingPrivilegesUpdated++;
                
                _logger.LogInformation("Updated privilege usage for {PrivilegeName} to new value {Value} during migration",
                    newPlanPrivilege.Privilege?.Name ?? "Unknown", allowedValue);
            }
        }
        
        _logger.LogInformation("Privilege synchronization complete for subscription {SubId}: " +
            "{NewCount} new privileges added, {UpdatedCount} existing privileges updated",
            subscription.Id, newPrivilegesAdded, existingPrivilegesUpdated);
    }
}

