using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Services;

/// <summary>
/// Background service that monitors for expired privilege usage periods
/// Logs warnings for admin review when privileges have expired but not been reset
/// Actual resets happen on billing success in PaymentService
/// </summary>
public class PrivilegeResetBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PrivilegeResetBackgroundService> _logger;
    
    public PrivilegeResetBackgroundService(
        IServiceProvider serviceProvider, 
        ILogger<PrivilegeResetBackgroundService> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Privilege Reset Background Service started");
        
        // Wait 1 minute before first run to allow app to fully start
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckExpiredPrivilegeUsagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in privilege reset background service");
            }
            
            // Run daily
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
        
        _logger.LogInformation("Privilege Reset Background Service stopped");
    }
    
    private async Task CheckExpiredPrivilegeUsagesAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var now = DateTime.UtcNow;
        
        // Find usage records where period has expired but usage hasn't been reset
        var expiredUsages = await context.UserSubscriptionPrivilegeUsages
            .Include(u => u.SubscriptionPlanPrivilege)
                .ThenInclude(p => p.Privilege)
            .Where(u => u.UsagePeriodEnd < now && u.UsedValue > 0)
            .Take(100)
            .ToListAsync(stoppingToken);
        
        if (expiredUsages.Any())
        {
            _logger.LogWarning(
                "Found {Count} expired privilege usages that need attention. " +
                "These should reset automatically on next successful billing. " +
                "If billing is delayed, privileges may be locked until payment succeeds.",
                expiredUsages.Count);
            
            // Log details for admin review
            foreach (var usage in expiredUsages.Take(10)) // Log first 10
            {
                _logger.LogInformation(
                    "Expired privilege: SubscriptionId={SubscriptionId}, Privilege={PrivilegeName}, " +
                    "Expired={ExpiredDate}, DaysSinceExpiry={Days}",
                    usage.SubscriptionId,
                    usage.SubscriptionPlanPrivilege?.Privilege?.Name ?? "Unknown",
                    usage.UsagePeriodEnd,
                    (now - usage.UsagePeriodEnd).Days);
            }
        }
        else
        {
            _logger.LogDebug("No expired privilege usages found - all privileges are current");
        }
    }
}

