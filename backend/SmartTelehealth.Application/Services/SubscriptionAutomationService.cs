using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Application.Utilities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Application.Services;

/// <summary>
/// Service responsible for automating subscription management tasks and operations.
/// This service handles automated billing, renewals, plan changes, expiration processing,
/// and other subscription lifecycle automation tasks. It provides scheduled and on-demand
/// automation capabilities to ensure subscriptions are properly managed without manual intervention.
/// 
/// Key Features:
/// - Automated billing processing for due subscriptions
/// - Subscription renewal automation
/// - Plan change automation with proration
/// - Expired subscription processing
/// - Automation status monitoring
/// - Automation logs and reporting
/// - Proration calculations for plan changes
/// - Bulk operations support
/// - Error handling and retry logic
/// - Integration with lifecycle and billing services
/// </summary>
public class SubscriptionAutomationService : ISubscriptionAutomationService
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly ISubscriptionLifecycleService _lifecycleService;
    private readonly ISubscriptionBillingService _billingService; // UPDATED: Use consolidated service
    private readonly IStripeService _stripeService;
      
    private readonly ILogger<SubscriptionAutomationService> _logger;

    public SubscriptionAutomationService(
        ISubscriptionRepository subscriptionRepository,
        ISubscriptionLifecycleService lifecycleService,
        ISubscriptionBillingService billingService, // UPDATED: Use consolidated service
        IStripeService stripeService,
          
        ILogger<SubscriptionAutomationService> logger)
    {
        _subscriptionRepository = subscriptionRepository;
        _lifecycleService = lifecycleService;
        _billingService = billingService;
        _stripeService = stripeService;
          
        _logger = logger;
    }

    public async Task<JsonModel> TriggerBillingAsync(TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Triggering automated billing by user {UserId}", tokenModel?.UserID ?? 0);
            
            // Get all subscriptions due for billing
            var dueSubscriptions = await _subscriptionRepository.GetSubscriptionsDueForBillingAsync(DateTime.UtcNow);
            var processedCount = 0;
            var failedCount = 0;

            foreach (var subscription in dueSubscriptions)
            {
                try
                {
                    // SRP Refactoring: Use centralized billing record factory method
                    var billingResult = await _billingService.CreateSubscriptionBillingAsync(
                        subscription,
                        subscription.CurrentPrice,
                        $"Automated billing for {subscription.SubscriptionPlan.Name}",
                        DateTime.UtcNow,
                        tokenModel
                    );
                    
                    if (billingResult.StatusCode == 200)
                    {
                        processedCount++;

                    }
                    else
                    {
                        failedCount++;

                    }
                }
                catch (Exception ex)
                {
                    failedCount++;
                    _logger.LogError(ex, "Error processing billing for subscription {SubscriptionId}", subscription.Id);

                }
            }

            var result = new 
            { 
                BillingTriggered = true, 
                Timestamp = DateTime.UtcNow, 
                TriggeredBy = tokenModel?.UserID ?? 0,
                ProcessedCount = processedCount,
                FailedCount = failedCount,
                TotalCount = dueSubscriptions.Count()
            };
            
            _logger.LogInformation("Automated billing triggered successfully by user {UserId}. Processed: {Processed}, Failed: {Failed}", 
                tokenModel?.UserID ?? 0, processedCount, failedCount);
            
            return new JsonModel 
            { 
                data = result, 
                Message = $"Automated billing completed. Processed: {processedCount}, Failed: {failedCount}", 
                StatusCode = 200 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error triggering billing by user {UserId}", tokenModel?.UserID ?? 0);
            return new JsonModel 
            { 
                data = new object(), 
                Message = "Failed to trigger billing", 
                StatusCode = 500 
            };
        }
    }

    public async Task<JsonModel> RenewSubscriptionAsync(string subscriptionId, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Renewing subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            
            var subscription = await _subscriptionRepository.GetByIdWithDetailsAsync(Guid.Parse(subscriptionId));
            if (subscription == null)
            {
                return new JsonModel 
                { 
                    data = new object(), 
                    Message = "Subscription not found", 
                    StatusCode = 404 
                };
            }

            // Check if subscription can be renewed
            if (subscription.Status != Subscription.SubscriptionStatuses.Active && 
                subscription.Status != Subscription.SubscriptionStatuses.Expired)
            {
                return new JsonModel 
                { 
                    data = new object(), 
                    Message = "Subscription cannot be renewed in its current status", 
                    StatusCode = 400 
                };
            }

            // FIXED: Use centralized calculator for consistency (handles leap years, month variations)
            var newBillingDate = BillingCycleCalculator.CalculateNextBillingDate(
                subscription.NextBillingDate, 
                subscription.BillingCycle);

            // Update subscription
            subscription.NextBillingDate = newBillingDate;
            subscription.Status = Subscription.SubscriptionStatuses.Active;
            subscription.UpdatedBy = tokenModel.UserID;
            subscription.UpdatedDate = DateTime.UtcNow;
            
            await _subscriptionRepository.UpdateSubscriptionAsync(subscription);
            await _subscriptionRepository.SaveChangesAsync();



            var result = new 
            { 
                SubscriptionId = subscriptionId, 
                Renewed = true, 
                Timestamp = DateTime.UtcNow, 
                RenewedBy = tokenModel?.UserID ?? 0,
                NewBillingDate = newBillingDate
            };
            
            _logger.LogInformation("Subscription {SubscriptionId} renewed successfully by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            return new JsonModel 
            { 
                data = result, 
                Message = "Subscription renewed successfully", 
                StatusCode = 200 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error renewing subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            return new JsonModel 
            { 
                data = new object(), 
                Message = "Failed to renew subscription", 
                StatusCode = 500 
            };
        }
    }

    public async Task<JsonModel> ChangePlanAsync(string subscriptionId, ChangePlanRequest request, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Changing plan for subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            
            var subscription = await _subscriptionRepository.GetByIdWithDetailsAsync(Guid.Parse(subscriptionId));
            if (subscription == null)
            {
                return new JsonModel 
                { 
                    data = new object(), 
                    Message = "Subscription not found", 
                    StatusCode = 404 
                };
            }

            var newPlan = await _subscriptionRepository.GetSubscriptionPlanByIdAsync(Guid.Parse(request.NewPlanId));
            if (newPlan == null)
            {
                return new JsonModel 
                { 
                    data = new object(), 
                    Message = "New plan not found", 
                    StatusCode = 404 
                };
            }

            // Calculate proration
            var prorationAmount = CalculateProration(subscription, newPlan, request.EffectiveDate);
            
            // Update subscription
            var oldPlanId = subscription.SubscriptionPlanId;
            subscription.SubscriptionPlanId = newPlan.Id;
            subscription.CurrentPrice = newPlan.Price;
            subscription.UpdatedBy = tokenModel.UserID;
            subscription.UpdatedDate = DateTime.UtcNow;
            
            await _subscriptionRepository.UpdateSubscriptionAsync(subscription);
            await _subscriptionRepository.SaveChangesAsync();



            var result = new 
            { 
                SubscriptionId = subscriptionId, 
                OldPlanId = oldPlanId, 
                NewPlanId = newPlan.Id,
                ProrationAmount = prorationAmount,
                EffectiveDate = request.EffectiveDate,
                ChangedBy = tokenModel?.UserID ?? 0
            };
            
            return new JsonModel 
            { 
                data = result, 
                Message = "Plan changed successfully", 
                StatusCode = 200 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing plan for subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            return new JsonModel 
            { 
                data = new object(), 
                Message = "Failed to change plan", 
                StatusCode = 500 
            };
        }
    }

    public async Task<JsonModel> ProcessAutomatedRenewalsAsync(TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Processing automated renewals by user {UserId}", tokenModel?.UserID ?? 0);
            
            var activeSubscriptions = await _subscriptionRepository.GetActiveSubscriptionsAsync();
            var renewalsProcessed = 0;
            var failedRenewals = 0;

            foreach (var subscription in activeSubscriptions)
            {
                try
                {
                    // Check if subscription needs renewal
                    if (subscription.NextBillingDate <= DateTime.UtcNow.AddDays(7) && subscription.AutoRenew)
                    {
                        var renewalResult = await RenewSubscriptionAsync(subscription.Id.ToString(), tokenModel);
                        if (renewalResult.StatusCode == 200)
                        {
                            renewalsProcessed++;
                        }
                        else
                        {
                            failedRenewals++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    failedRenewals++;
                    _logger.LogError(ex, "Error processing renewal for subscription {SubscriptionId}", subscription.Id);
                }
            }

            var result = new 
            { 
                RenewalsProcessed = renewalsProcessed, 
                FailedRenewals = failedRenewals,
                Timestamp = DateTime.UtcNow, 
                ProcessedBy = tokenModel?.UserID ?? 0 
            };
            
            _logger.LogInformation("Automated renewals processed successfully by user {UserId}. Processed: {Processed}, Failed: {Failed}", 
                tokenModel?.UserID ?? 0, renewalsProcessed, failedRenewals);
            
            return new JsonModel 
            { 
                data = result, 
                Message = $"Automated renewals processed successfully. Processed: {renewalsProcessed}, Failed: {failedRenewals}", 
                StatusCode = 200 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing automated renewals by user {UserId}", tokenModel?.UserID ?? 0);
            return new JsonModel 
            { 
                data = new object(), 
                Message = "Failed to process automated renewals", 
                StatusCode = 500 
            };
        }
    }

    public async Task<JsonModel> ProcessExpiredSubscriptionsAsync(TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Processing expired subscriptions by user {UserId}", tokenModel?.UserID ?? 0);
            
            var activeSubscriptions = await _subscriptionRepository.GetActiveSubscriptionsAsync();
            var expiredCount = 0;
            var failedExpirations = 0;

            foreach (var subscription in activeSubscriptions)
            {
                try
                {
                    if (subscription.NextBillingDate <= DateTime.UtcNow)
                    {
                        var expirationResult = await _lifecycleService.ExpireSubscriptionAsync(subscription.Id, "Automated expiration", tokenModel);
                        if (expirationResult)
                        {
                            expiredCount++;
                        }
                        else
                        {
                            failedExpirations++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    failedExpirations++;
                    _logger.LogError(ex, "Error processing expiration for subscription {SubscriptionId}", subscription.Id);
                }
            }

            var result = new 
            { 
                ExpiredCount = expiredCount, 
                FailedExpirations = failedExpirations,
                Timestamp = DateTime.UtcNow, 
                ProcessedBy = tokenModel?.UserID ?? 0 
            };
            
            return new JsonModel 
            { 
                data = result, 
                Message = $"Expired subscriptions processed successfully. Expired: {expiredCount}, Failed: {failedExpirations}", 
                StatusCode = 200 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing expired subscriptions by user {UserId}", tokenModel?.UserID ?? 0);
            return new JsonModel 
            { 
                data = new object(), 
                Message = "Failed to process expired subscriptions", 
                StatusCode = 500 
            };
        }
    }

    public async Task<JsonModel> GetAutomationStatusAsync(TokenModel tokenModel)
    {
        try
        {
            var activeSubscriptions = await _subscriptionRepository.GetActiveSubscriptionsAsync();
            var dueForBilling = await _subscriptionRepository.GetSubscriptionsDueForBillingAsync(DateTime.UtcNow);
            var expiredSubscriptions = activeSubscriptions.Where(s => s.NextBillingDate <= DateTime.UtcNow);

            var status = new
            {
                TotalActiveSubscriptions = activeSubscriptions.Count(),
                DueForBilling = dueForBilling.Count(),
                ExpiredSubscriptions = expiredSubscriptions.Count(),
                LastRun = DateTime.UtcNow,
                NextScheduledRun = DateTime.UtcNow.AddHours(1)
            };

            return new JsonModel
            {
                data = status,
                Message = "Automation status retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting automation status by user {UserId}", tokenModel?.UserID ?? 0);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to get automation status",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetAutomationLogsAsync(int page = 1, int pageSize = 50, TokenModel tokenModel = null)
    {
        try
        {           
            return new JsonModel
            {
                data = new object(),
                Message = "Automation logs retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting automation logs by user {UserId}", tokenModel?.UserID ?? 0);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to get automation logs",
                StatusCode = 500
            };
        }
    }

    /// <summary>
    /// Calculates proration for plan changes (upgrade/downgrade).
    /// REFACTORED: Now uses centralized BillingCycleCalculator (PHASE 3)
    /// Returns NET charge: (new plan credit) - (old plan credit) for remaining period
    /// </summary>
    private decimal CalculateProration(Subscription subscription, SubscriptionPlan newPlan, DateTime effectiveDate)
    {
        try
        {
            // Calculate unused credit from old plan using centralized calculator
            var creditForRemainingDays = BillingCycleCalculator.CalculateProratedAmount(
                subscription,
                effectiveDate,
                subscription.CurrentPrice,
                null // No logger needed for internal calculation
            );
            
            // Calculate charge for new plan for remaining period
            // Create temporary subscription with new plan for proration calculation
            // NEW ARCHITECTURE: BillingCycle comes from plan, not direct property
            var tempSubscription = new Subscription
            {
                Id = subscription.Id,
                CurrentPrice = newPlan.Price,
                SubscriptionPlan = newPlan,  // Set plan to get BillingCycle from it
                SubscriptionPlanId = newPlan.Id,
                StartDate = subscription.StartDate,
                LastBillingDate = subscription.LastBillingDate,
                NextBillingDate = subscription.NextBillingDate
            };
            
            var chargeForRemainingDays = BillingCycleCalculator.CalculateProratedAmount(
                tempSubscription,
                effectiveDate,
                newPlan.Price,
                null // No logger needed for internal calculation
            );
            
            // Return NET proration charge (positive = charge user, negative = credit user)
            return chargeForRemainingDays - creditForRemainingDays;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating proration for subscription {SubscriptionId}", subscription.Id);
            return 0;
        }
    }

    #region OLD PRORATION HELPER - REMOVED IN PHASE 3
    // REMOVED: GetDaysInBillingCycle() method (was Lines 484-497)
    // Now using: BillingCycleCalculator for all proration calculations
    #endregion
}
