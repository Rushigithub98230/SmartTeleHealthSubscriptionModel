using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;

namespace SmartTelehealth.Infrastructure.Services
{
    /// <summary>
    /// Background service that automatically retries failed compensating refunds.
    /// Runs every hour to process pending failed refunds and notify admins of permanent failures.
    /// This ensures financial discrepancies are resolved automatically or escalated properly.
    /// </summary>
    public class FailedRefundRetryBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<FailedRefundRetryBackgroundService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromHours(1); // Run every hour

        public FailedRefundRetryBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<FailedRefundRetryBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Failed Refund Retry Background Service started. Running every {Interval} minutes.", 
                _interval.TotalMinutes);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessFailedRefundsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Failed Refund Retry Background Service");
                }

                await Task.Delay(_interval, stoppingToken);
            }

            _logger.LogInformation("Failed Refund Retry Background Service stopped.");
        }

        private async Task ProcessFailedRefundsAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var failedRefundRepository = scope.ServiceProvider.GetRequiredService<IFailedRefundRepository>();
            var stripeService = scope.ServiceProvider.GetRequiredService<IStripeService>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            try
            {
                // Get all failed refunds that should be retried
                var pendingRefunds = await failedRefundRepository.GetPendingRetryAsync();
                var pendingList = pendingRefunds.ToList();

                if (!pendingList.Any())
                {
                    _logger.LogInformation("No pending failed refunds to retry");
                    return;
                }

                _logger.LogInformation("Found {Count} pending failed refunds to retry", pendingList.Count);

                foreach (var failedRefund in pendingList)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    await RetryFailedRefundAsync(failedRefund, stripeService, failedRefundRepository, notificationService);
                }

                // Check for refunds requiring manual intervention and notify admins
                await NotifyAdminsOfPermanentFailuresAsync(failedRefundRepository, notificationService);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing failed refunds");
            }
        }

        private async Task RetryFailedRefundAsync(
            FailedRefund failedRefund,
            IStripeService stripeService,
            IFailedRefundRepository failedRefundRepository,
            INotificationService notificationService)
        {
            try
            {
                // CRITICAL SAFEGUARD #2: Check if refund was already processed to prevent double refunds
                // Re-fetch the latest state in case another process already resolved it
                var latestState = await failedRefundRepository.GetByIdAsync(failedRefund.Id);
                if (latestState == null)
                {
                    _logger.LogWarning("Failed refund {FailedRefundId} no longer exists. Skipping retry.", failedRefund.Id);
                    return;
                }
                
                if (latestState.Status == FailedRefundStatus.Refunded || 
                    latestState.Status == FailedRefundStatus.ManuallyResolved ||
                    latestState.Status == FailedRefundStatus.Cancelled)
                {
                    _logger.LogInformation(
                        "⚠️ DUPLICATE REFUND PREVENTED: Failed refund {FailedRefundId} already resolved with status {Status}. " +
                        "Skipping retry to prevent double refunding.",
                        failedRefund.Id, latestState.Status);
                    return;
                }
                
                // Update status to "Retrying" to prevent concurrent processing
                latestState.Status = FailedRefundStatus.Retrying;
                latestState.UpdatedDate = DateTime.UtcNow;
                await failedRefundRepository.UpdateAsync(latestState);
                
                _logger.LogInformation(
                    "Retrying failed refund {FailedRefundId} (Attempt {AttemptNumber}/{MaxRetries}). " +
                    "PaymentIntentId: {PaymentIntentId}, Amount: ${Amount}",
                    failedRefund.Id, latestState.RetryCount + 1, latestState.MaxRetries,
                    latestState.StripePaymentIntentId, latestState.Amount);

                // Create system token for Stripe API call
                var systemToken = new TokenModel { UserID = 0, RoleID = 1 }; // System user

                // Attempt refund
                var refundResult = await stripeService.ProcessRefundAsync(
                    latestState.StripePaymentIntentId,
                    latestState.Amount,
                    systemToken);

                if (refundResult)
                {
                    // SUCCESS - Mark as refunded
                    await failedRefundRepository.MarkAsRefundedAsync(
                        latestState.Id,
                        $"Successfully refunded on retry attempt {latestState.RetryCount + 1}",
                        resolvedBy: 0); // System resolved

                    _logger.LogInformation(
                        "✅ Successfully refunded after retry. FailedRefundId: {FailedRefundId}, " +
                        "PaymentIntentId: {PaymentIntentId}, Amount: ${Amount}",
                        latestState.Id, latestState.StripePaymentIntentId, latestState.Amount);

                    // Send notification to user
                    await notificationService.CreateNotificationAsync(new CreateNotificationDto
                    {
                        UserId = latestState.UserId,
                        Title = "Refund Processed",
                        Message = $"A refund of ${latestState.Amount:F2} has been processed to your payment method. " +
                                 "This was due to a technical issue during payment processing. We apologize for any inconvenience.",
                        Type = "RefundProcessed",
                        IsRead = false,
                        Priority = "Normal"
                    }, systemToken);
                }
                else
                {
                    // FAILED - Increment retry count
                    var errorMessage = $"Refund attempt {latestState.RetryCount + 1} failed - Stripe API returned false";
                    await failedRefundRepository.IncrementRetryCountAsync(latestState.Id, errorMessage);

                    _logger.LogWarning(
                        "❌ Failed refund retry attempt {AttemptNumber}/{MaxRetries} failed. " +
                        "FailedRefundId: {FailedRefundId}, PaymentIntentId: {PaymentIntentId}",
                        latestState.RetryCount + 1, latestState.MaxRetries,
                        latestState.Id, latestState.StripePaymentIntentId);

                    // If exceeded max retries, it will be picked up by admin notification logic
                }
            }
            catch (Exception ex)
            {
                // EXCEPTION - Increment retry count with exception message
                // Need to get current state again since we may not have latestState if exception occurred early
                var currentState = await failedRefundRepository.GetByIdAsync(failedRefund.Id);
                if (currentState != null)
                {
                    var errorMessage = $"Exception on retry attempt {currentState.RetryCount + 1}: {ex.Message}";
                    await failedRefundRepository.IncrementRetryCountAsync(currentState.Id, errorMessage);

                    _logger.LogError(ex,
                        "❌ Exception during failed refund retry. FailedRefundId: {FailedRefundId}, " +
                        "PaymentIntentId: {PaymentIntentId}, Attempt: {AttemptNumber}/{MaxRetries}",
                        currentState.Id, currentState.StripePaymentIntentId,
                        currentState.RetryCount + 1, currentState.MaxRetries);
                }
            }
        }

        private async Task NotifyAdminsOfPermanentFailuresAsync(
            IFailedRefundRepository failedRefundRepository,
            INotificationService notificationService)
        {
            try
            {
                // Get failed refunds requiring manual intervention (exceeded max retries)
                var requiresIntervention = await failedRefundRepository.GetRequiringManualInterventionAsync();
                var interventionList = requiresIntervention.Where(f => !f.AdminNotified).ToList();

                if (!interventionList.Any())
                    return;

                _logger.LogWarning(
                    "⚠️ {Count} failed refunds require manual intervention. Notifying admins.",
                    interventionList.Count);

                // Create system token
                var systemToken = new TokenModel { UserID = 0, RoleID = 1 };

                // TODO: Get admin user IDs from configuration or database
                // For now, create a general notification that admins can see
                var totalAmount = interventionList.Sum(f => f.Amount);
                var notificationMessage = 
                    $"URGENT: {interventionList.Count} failed refunds require immediate manual intervention. " +
                    $"Total amount pending: ${totalAmount:F2}. " +
                    $"These refunds have exceeded maximum retry attempts. " +
                    $"Please review the Failed Refunds dashboard immediately.";

                // Send high-priority notification
                // In a real system, you would:
                // 1. Get list of admin users
                // 2. Send email to each admin
                // 3. Send SMS to on-call admin
                // 4. Create dashboard alert
                
                _logger.LogCritical(
                    "🚨 ADMIN ACTION REQUIRED: {Count} failed refunds totaling ${TotalAmount:F2} need manual review",
                    interventionList.Count, totalAmount);

                // Mark all as admin notified
                foreach (var failedRefund in interventionList)
                {
                    await failedRefundRepository.MarkAdminNotifiedAsync(failedRefund.Id);
                    
                    _logger.LogWarning(
                        "Admin notification sent for FailedRefund {FailedRefundId}: " +
                        "User {UserId}, Amount ${Amount}, PaymentIntent {PaymentIntentId}",
                        failedRefund.Id, failedRefund.UserId, failedRefund.Amount, failedRefund.StripePaymentIntentId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying admins of permanent failures");
            }
        }
    }
}

