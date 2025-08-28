using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.Services;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.DTOs;

namespace SmartTelehealth.Application.Services.BackgroundServices;

public class SubscriptionBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SubscriptionBackgroundService> _logger;
    private readonly TimeSpan _billingInterval = TimeSpan.FromHours(1); // Run every hour
    private readonly TimeSpan _lifecycleInterval = TimeSpan.FromHours(6); // Run every 6 hours

    public SubscriptionBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<SubscriptionBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Subscription Background Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Process automated billing
                await ProcessAutomatedBillingAsync(stoppingToken);

                // Process lifecycle management
                await ProcessLifecycleManagementAsync(stoppingToken);

                // Wait for next cycle
                await Task.Delay(_billingInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Subscription Background Service cancelled");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Subscription Background Service");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Wait 5 minutes before retry
            }
        }

        _logger.LogInformation("Subscription Background Service stopped");
    }

    private async Task ProcessAutomatedBillingAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var automatedBillingService = scope.ServiceProvider.GetRequiredService<AutomatedBillingService>();

            _logger.LogInformation("Starting automated billing process");
            
            // Create system-level token for background service operations
            var systemToken = new TokenModel
            {
                UserID = 0, // System user ID
                RoleID = 1  // Admin role
            };
            
            await automatedBillingService.ProcessRecurringBillingAsync(systemToken);

            _logger.LogInformation("Automated billing completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in automated billing process");
        }
    }

    private async Task ProcessLifecycleManagementAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var lifecycleService = scope.ServiceProvider.GetRequiredService<ISubscriptionLifecycleService>();
            var subscriptionRepository = scope.ServiceProvider.GetRequiredService<ISubscriptionRepository>();

            _logger.LogInformation("Starting lifecycle management process");

            // Process subscription expirations
            var activeSubscriptions = await subscriptionRepository.GetAllSubscriptionsAsync();
            activeSubscriptions = activeSubscriptions.Where(s => s.Status == "Active");
            var expiredCount = 0;

            foreach (var subscription in activeSubscriptions)
            {
                if (subscription.NextBillingDate <= DateTime.UtcNow)
                {
                    var result = await lifecycleService.ExpireSubscriptionAsync(subscription.Id);
                    if (result)
                    {
                        expiredCount++;
                    }
                }
            }

            // Process trial expirations
            var trialSubscriptions = await subscriptionRepository.GetAllSubscriptionsAsync();
            trialSubscriptions = trialSubscriptions.Where(s => s.Status == "TrialActive");
            var trialExpiredCount = 0;

            foreach (var subscription in trialSubscriptions)
            {
                if (subscription.TrialEndDate <= DateTime.UtcNow)
                {
                    var result = await lifecycleService.ExpireSubscriptionAsync(subscription.Id);
                    if (result)
                    {
                        trialExpiredCount++;
                    }
                }
            }

            _logger.LogInformation("Lifecycle management completed. Expired: {Expired}, Trial Expired: {TrialExpired}",
                expiredCount, trialExpiredCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in lifecycle management process");
        }
    }
}
