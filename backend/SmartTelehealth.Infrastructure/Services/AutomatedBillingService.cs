using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Infrastructure.Services;

public class AutomatedBillingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AutomatedBillingService> _logger;
    private readonly TimeSpan _billingInterval = TimeSpan.FromHours(1); // Run every hour
    private readonly int _maxRetryAttempts = 3;
    private readonly TimeSpan _retryDelay = TimeSpan.FromHours(6);

    public AutomatedBillingService(
        IServiceProvider serviceProvider,
        ILogger<AutomatedBillingService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Automated billing service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBillingCycleAsync();
                await Task.Delay(_billingInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Automated billing service stopped");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in automated billing cycle");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Wait before retry
            }
        }
    }

    private async Task ProcessBillingCycleAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var subscriptionRepository = scope.ServiceProvider.GetRequiredService<ISubscriptionRepository>();
        var billingService = scope.ServiceProvider.GetRequiredService<IBillingService>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        var auditService = scope.ServiceProvider.GetRequiredService<IAuditService>();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        try
        {
            _logger.LogInformation("Starting automated billing cycle at {Time}", DateTime.UtcNow);

            // 1. Process subscriptions due for billing
            await ProcessDueSubscriptionsAsync(subscriptionRepository, billingService, notificationService, auditService, userService);

            // 2. Process failed payment retries
            await ProcessFailedPaymentRetriesAsync(subscriptionRepository, billingService, notificationService, auditService, userService);

            // 3. Reset usage counters for new billing cycles
            await ResetUsageCountersAsync(subscriptionRepository, auditService);

            _logger.LogInformation("Completed automated billing cycle at {Time}", DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in automated billing cycle");
            throw;
        }
    }

    private async Task ProcessDueSubscriptionsAsync(
        ISubscriptionRepository subscriptionRepository,
        IBillingService billingService,
        INotificationService notificationService,
        IAuditService auditService,
        IUserService userService)
    {
        try
        {
            // Create system-level token for background service operations
            var systemToken = new TokenModel
            {
                UserID = 0, // System user ID
                RoleID = 1  // Admin role
            };
            
            var dueSubscriptions = await subscriptionRepository.GetSubscriptionsDueForBillingAsync(DateTime.UtcNow);
            _logger.LogInformation("Found {Count} subscriptions due for billing", dueSubscriptions.Count());

            foreach (var subscription in dueSubscriptions)
            {
                try
                {
                    await ProcessSubscriptionBillingAsync(subscription, subscriptionRepository, billingService, notificationService, auditService, userService);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing billing for subscription {SubscriptionId}", subscription.Id);
                    await auditService.LogPaymentEventAsync(
                        subscription.UserId,
                        "BillingError",
                        subscription.Id.ToString(),
                        "Error",
                        ex.Message,
                        systemToken
                    );
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing due subscriptions");
            throw;
        }
    }

    private async Task ProcessSubscriptionBillingAsync(
        Subscription subscription,
        ISubscriptionRepository subscriptionRepository,
        IBillingService billingService,
        INotificationService notificationService,
        IAuditService auditService,
        IUserService userService)
    {
        try
        {
            // Create system-level token for background service operations
            var systemToken = new TokenModel
            {
                UserID = 0, // System user ID
                RoleID = 1  // Admin role
            };
            
            // Create billing record
            var billingRecord = new CreateBillingRecordDto
            {
                UserId = subscription.UserId,
                SubscriptionId = subscription.Id.ToString(),
                Amount = subscription.CurrentPrice,
                Description = $"Subscription billing for {subscription.SubscriptionPlan.Name}",
                DueDate = DateTime.UtcNow
            };

            var billingResult = await billingService.CreateBillingRecordAsync(billingRecord, systemToken);
            if (billingResult.StatusCode != 200)
            {
                _logger.LogError("Failed to create billing record for subscription {SubscriptionId}", subscription.Id);
                return;
            }

            // Process payment with retry logic
            var billingData2 = billingResult.data as dynamic;
            if (billingData2?.Id == null)
            {
                _logger.LogError("Invalid billing result data for subscription {SubscriptionId}", subscription.Id);
                return;
            }
            var paymentResult = await ProcessPaymentWithRetryAsync(Guid.Parse(billingData2.Id.ToString()), billingService, auditService);

            if (paymentResult.StatusCode == 200)
            {
                // Update subscription billing date
                subscription.NextBillingDate = subscription.NextBillingDate.AddMonths(1);
                subscription.LastBillingDate = DateTime.UtcNow;
                subscription.FailedPaymentAttempts = 0;
                subscription.LastPaymentError = null;
                await subscriptionRepository.UpdateAsync(subscription);

                // Send success notification
                var userResult = await userService.GetUserByIdAsync(subscription.UserId, systemToken);
                if (userResult.StatusCode == 200 && userResult.data != null)
                {
                    var userData = userResult.data as dynamic;
                    var billingData = billingResult.data as dynamic;
                    if (userData?.Email != null && userData?.FullName != null)
                    {
                                            await notificationService.SendPaymentSuccessEmailAsync(
                        userData.Email.ToString(),
                        userData.FullName.ToString(),
                        billingData,
                        systemToken
                    );
                    }
                }

                await auditService.LogPaymentEventAsync(
                    subscription.UserId,
                    "PaymentSuccess",
                    subscription.Id.ToString(),
                    "Success",
                    null,
                    systemToken
                );

                _logger.LogInformation("Successfully processed billing for subscription {SubscriptionId}", subscription.Id);
            }
            else
            {
                // Handle failed payment with immediate suspension
                var errorMessage = paymentResult.Message?.ToString() ?? "Unknown payment error";
                await HandleFailedPaymentAsync(subscription, errorMessage, subscriptionRepository, billingService, notificationService, auditService, userService);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing subscription billing for {SubscriptionId}", subscription.Id);
            throw;
        }
    }

    private async Task<JsonModel> ProcessPaymentWithRetryAsync(
        Guid billingRecordId,
        IBillingService billingService,
        IAuditService auditService)
    {
        // Create system-level token for background service operations
        var systemToken = new TokenModel
        {
            UserID = 0, // System user ID
            RoleID = 1  // Admin role
        };
        
        for (int attempt = 1; attempt <= _maxRetryAttempts; attempt++)
        {
            try
            {
                var paymentResult = await billingService.ProcessPaymentAsync(billingRecordId, systemToken);
                
                if (paymentResult.StatusCode == 200)
                {
                    return paymentResult;
                }

                if (attempt < _maxRetryAttempts)
                {
                    _logger.LogWarning("Payment attempt {Attempt} failed for billing record {BillingRecordId}, retrying in {Delay} hours", 
                        attempt, billingRecordId, _retryDelay.TotalHours);
                    
                    await Task.Delay(_retryDelay);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in payment attempt {Attempt} for billing record {BillingRecordId}", attempt, billingRecordId);
                
                if (attempt == _maxRetryAttempts)
                {
                    return new JsonModel
                    {
                        data = new object(),
                        Message = $"Payment failed after {_maxRetryAttempts} attempts: {ex.Message}",
                        StatusCode = 500
                    };
                }
                
                await Task.Delay(_retryDelay);
            }
        }

        return new JsonModel
        {
            data = new object(),
            Message = $"Payment failed after {_maxRetryAttempts} attempts",
            StatusCode = 500
        };
    }

    private async Task HandleFailedPaymentAsync(
        Subscription subscription,
        string errorMessage,
        ISubscriptionRepository subscriptionRepository,
        IBillingService billingService,
        INotificationService notificationService,
        IAuditService auditService,
        IUserService userService)
    {
        try
        {
            // Update subscription status to suspended immediately
            subscription.Status = Subscription.SubscriptionStatuses.Suspended;
            subscription.FailedPaymentAttempts += 1;
            subscription.LastPaymentError = errorMessage;
            subscription.LastPaymentFailedDate = DateTime.UtcNow;
            subscription.SuspendedDate = DateTime.UtcNow;
            await subscriptionRepository.UpdateAsync(subscription);

            // Send immediate suspension notification
            var userResult = await userService.GetUserByIdAsync(subscription.UserId, new TokenModel { UserID = 0, RoleID = 1 });
            if (userResult.StatusCode == 200 && userResult.data != null)
            {
                var userData = userResult.data as dynamic;
                var billingRecordDto = new CreateBillingRecordDto
                {
                    UserId = subscription.UserId,
                    SubscriptionId = subscription.Id.ToString(),
                    Amount = subscription.CurrentPrice,
                    Description = $"Subscription billing for {subscription.SubscriptionPlan.Name}",
                    DueDate = DateTime.UtcNow
                };
                var tokenModel = new TokenModel { UserID = 0, RoleID = 1 };
                
                // Create the billing record first
                var billingResult = await billingService.CreateBillingRecordAsync(billingRecordDto, tokenModel);
                if (billingResult.StatusCode == 200 && billingResult.data != null)
                {
                    var billingRecord = billingResult.data as dynamic;
                    
                    // Send payment failed notification
                    await notificationService.SendPaymentFailedEmailAsync(
                        userData.Email,
                        userData.FullName,
                        billingRecord,
                        tokenModel
                    );
                    _logger.LogInformation("Payment failed notification sent to user {UserId}", subscription.UserId);
                }

                // Send immediate suspension notification
                await notificationService.SendSubscriptionSuspendedNotificationAsync(
                    subscription.UserId,
                    subscription.Id.ToString(),
                    tokenModel
                );
                _logger.LogInformation("Subscription suspended notification sent to user {UserId}", subscription.UserId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling failed payment for subscription {SubscriptionId}", subscription.Id);
        }
    }

    private async Task ProcessFailedPaymentRetriesAsync(
        ISubscriptionRepository subscriptionRepository,
        IBillingService billingService,
        INotificationService notificationService,
        IAuditService auditService,
        IUserService userService)
    {
        try
        {
            // Create system-level token for background service operations
            var systemToken = new TokenModel
            {
                UserID = 0, // System user ID
                RoleID = 1  // Admin role
            };
            
            var suspendedSubscriptions = await subscriptionRepository.GetSuspendedSubscriptionsAsync();
            _logger.LogInformation("Found {Count} suspended subscriptions", suspendedSubscriptions.Count());

            foreach (var subscription in suspendedSubscriptions)
            {
                                    // Extract all subscription values to avoid dynamic dispatch issues
                    var subscriptionId = subscription.Id.ToString();
                    var userId = subscription.UserId.ToString();
                    var userIdInt = int.TryParse(userId, out int parsedUserId) ? parsedUserId : 0;
                    var currentPrice = subscription.CurrentPrice;
                    var subscriptionPlanName = subscription.SubscriptionPlan?.Name?.ToString() ?? "unknown";
                    var suspendedDate = subscription.SuspendedDate;
                    var failedPaymentAttempts = subscription.FailedPaymentAttempts;
                    var nextBillingDate = subscription.NextBillingDate;
                    var lastBillingDate = subscription.LastBillingDate;
                    var lastPaymentError = subscription.LastPaymentError;
                    var lastPaymentFailedDate = subscription.LastPaymentFailedDate;

                try
                {

                    // Check if enough time has passed since suspension
                    if (suspendedDate.HasValue &&
                        DateTime.UtcNow - suspendedDate.Value < _retryDelay)
                    {
                        continue; // Skip if not enough time has passed
                    }

                    // Retry payment for suspended subscription
                    var billingRecord = new CreateBillingRecordDto
                    {
                        UserId = userIdInt,
                        SubscriptionId = subscriptionId,
                        Amount = currentPrice,
                        Description = $"Retry payment for {subscriptionPlanName}",
                        DueDate = DateTime.UtcNow
                    };

                    var billingResult = await billingService.CreateBillingRecordAsync(billingRecord, systemToken);
                    if (billingResult.StatusCode != 200)
                    {
                        continue;
                    }

                    var billingData = billingResult.data as dynamic;
                    if (billingData?.Id == null)
                    {
                        continue;
                    }
                    var paymentResult = await ProcessPaymentWithRetryAsync(Guid.Parse(billingData.Id.ToString()), billingService, auditService);

                    if (paymentResult.StatusCode == 200)
                    {
                        // Reactivate subscription
                        subscription.Status = Subscription.SubscriptionStatuses.Active;
                        subscription.NextBillingDate = nextBillingDate.AddMonths(1);
                        subscription.LastBillingDate = DateTime.UtcNow;
                        subscription.FailedPaymentAttempts = 0;
                        subscription.LastPaymentError = null;
                        subscription.SuspendedDate = null;
                        await subscriptionRepository.UpdateAsync(subscription);

                        // Send reactivation notification
                        var userResult = await userService.GetUserByIdAsync(userIdInt, systemToken);
                        if (userResult.StatusCode == 200 && userResult.data != null)
                        {
                            // Send subscription reactivated notification
                            await notificationService.SendSubscriptionReactivatedNotificationAsync(
                                userIdInt,
                                subscriptionId,
                                systemToken
                            );
                            _logger.LogInformation("Subscription reactivated notification sent to user {UserId}", userId);
                        }

                        await auditService.LogPaymentEventAsync(
                            userIdInt,
                            "PaymentRetrySuccess",
                            subscriptionId,
                            "Success",
                            null,
                            systemToken
                        );

                        _logger.LogInformation("Successfully retried payment and reactivated subscription");
                    }
                    else
                    {
                        // Update failure count but keep suspended
                        subscription.FailedPaymentAttempts = failedPaymentAttempts + 1;
                        subscription.LastPaymentError = paymentResult.Message;
                        subscription.LastPaymentFailedDate = DateTime.UtcNow;
                        await subscriptionRepository.UpdateAsync(subscription);

                        var errorMessage = paymentResult.Message?.ToString() ?? "Unknown error";
                        await auditService.LogPaymentEventAsync(
                            userIdInt,
                            "PaymentRetryFailed",
                            subscriptionId,
                            "Failed",
                            errorMessage,
                            systemToken
                        );

                        _logger.LogWarning("Payment retry failed for suspended subscription");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing payment retry for suspended subscription");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing failed payment retries");
            throw;
        }
    }

    private async Task ResetUsageCountersAsync(ISubscriptionRepository subscriptionRepository, IAuditService auditService)
    {
        try
        {
            // Create system-level token for background service operations
            var systemToken = new TokenModel
            {
                UserID = 0, // System user ID
                RoleID = 1  // Admin role
            };
            
            var subscriptionsWithResetUsage = await subscriptionRepository.GetSubscriptionsWithResetUsageAsync();
            _logger.LogInformation("Found {Count} subscriptions with usage counters to reset", subscriptionsWithResetUsage.Count());

            foreach (var subscription in subscriptionsWithResetUsage)
            {
                try
                {
                    // Reset usage counters for new billing cycle
                    await subscriptionRepository.ResetUsageCountersAsync();

                    await auditService.LogUserActionAsync(
                        subscription.UserId,
                        "UsageReset",
                        "Subscription",
                        subscription.Id.ToString(),
                        "Usage counters reset for new billing cycle",
                        systemToken
                    );

                    _logger.LogInformation("Reset usage counters for subscription {SubscriptionId}", subscription.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error resetting usage counters for subscription {SubscriptionId}", subscription.Id);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting usage counters");
            throw;
        }
    }

    public async Task<JsonModel> TriggerManualBillingCycleAsync()
    {
        try
        {
            _logger.LogInformation("Manual billing cycle triggered");
            await ProcessBillingCycleAsync();
            return new JsonModel
            {
                data = true,
                Message = "Manual billing cycle completed successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in manual billing cycle");
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to complete manual billing cycle",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetBillingCycleReportAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var subscriptionRepository = scope.ServiceProvider.GetRequiredService<ISubscriptionRepository>();
            var billingService = scope.ServiceProvider.GetRequiredService<IBillingService>();
            
            // Create system-level token for background service operations
            var systemToken = new TokenModel
            {
                UserID = 0, // System user ID
                RoleID = 1  // Admin role
            };

            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            var report = new BillingCycleReportDto
            {
                StartDate = start,
                EndDate = end,
                TotalSubscriptionsProcessed = 0,
                SuccessfulPayments = 0,
                FailedPayments = 0,
                TotalRevenue = 0,
                SuspendedSubscriptions = 0,
                PaymentRetries = 0,
                UsageResets = 0
            };

            // Get billing statistics
            var billingHistory = await billingService.GetPaymentAnalyticsAsync(start, end, systemToken);
            var analyticsData = billingHistory.data as dynamic;
            if (analyticsData != null)
            {
                report.SuccessfulPayments = (int)(analyticsData.SuccessfulTransactions ?? 0);
                report.FailedPayments = (int)(analyticsData.FailedTransactions ?? 0);
                report.TotalRevenue = (decimal)(analyticsData.TotalPayments ?? 0);
            }

            // Get subscription statistics
            var subscriptions = await subscriptionRepository.GetSubscriptionsInDateRangeAsync(start, end);
            report.TotalSubscriptionsProcessed = subscriptions.Count();

            // Get suspended subscription statistics
            var suspendedSubscriptions = await subscriptionRepository.GetSuspendedSubscriptionsAsync();
            report.SuspendedSubscriptions = suspendedSubscriptions.Count();

            var failedPayments = await subscriptionRepository.GetSubscriptionsWithFailedPaymentsAsync();
            report.PaymentRetries = failedPayments.Count(s => s.FailedPaymentAttempts > 0);

            return new JsonModel
            {
                data = report,
                Message = "Billing cycle report generated successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating billing cycle report");
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to generate billing cycle report",
                StatusCode = 500
            };
        }
    }
}

// DTOs for automated billing service
public class BillingCycleReportDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalSubscriptionsProcessed { get; set; }
    public int SuccessfulPayments { get; set; }
    public int FailedPayments { get; set; }
    public decimal TotalRevenue { get; set; }
    public int SuspendedSubscriptions { get; set; }
    public int PaymentRetries { get; set; }
    public int UsageResets { get; set; }
    public decimal SuccessRate => TotalSubscriptionsProcessed > 0 ? (decimal)SuccessfulPayments / TotalSubscriptionsProcessed * 100 : 0;
    public decimal FailureRate => TotalSubscriptionsProcessed > 0 ? (decimal)FailedPayments / TotalSubscriptionsProcessed * 100 : 0;
} 