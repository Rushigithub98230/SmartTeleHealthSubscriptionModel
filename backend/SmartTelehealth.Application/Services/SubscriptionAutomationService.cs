using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Application.Services;

public class SubscriptionAutomationService : ISubscriptionAutomationService
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly ISubscriptionLifecycleService _lifecycleService;
    private readonly IBillingService _billingService;
    private readonly IStripeService _stripeService;
    private readonly IAuditService _auditService;
    private readonly ILogger<SubscriptionAutomationService> _logger;

    public SubscriptionAutomationService(
        ISubscriptionRepository subscriptionRepository,
        ISubscriptionLifecycleService lifecycleService,
        IBillingService billingService,
        IStripeService stripeService,
        IAuditService auditService,
        ILogger<SubscriptionAutomationService> logger)
    {
        _subscriptionRepository = subscriptionRepository;
        _lifecycleService = lifecycleService;
        _billingService = billingService;
        _stripeService = stripeService;
        _auditService = auditService;
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
                    // Create billing record
                    var billingRecord = new CreateBillingRecordDto
                    {
                        UserId = subscription.UserId,
                        SubscriptionId = subscription.Id.ToString(),
                        Amount = subscription.CurrentPrice,
                        Description = $"Automated billing for {subscription.SubscriptionPlan.Name}",
                        DueDate = DateTime.UtcNow,
                        Type = BillingRecord.BillingType.Subscription.ToString()
                    };

                    var billingResult = await _billingService.CreateBillingRecordAsync(billingRecord, tokenModel);
                    if (billingResult.StatusCode == 200)
                    {
                        processedCount++;
                        await _auditService.LogPaymentEventAsync(
                            subscription.UserId,
                            "AutomatedBilling",
                            subscription.Id.ToString(),
                            "Success",
                            "Billing record created successfully",
                            tokenModel
                        );
                    }
                    else
                    {
                        failedCount++;
                        await _auditService.LogPaymentEventAsync(
                            subscription.UserId,
                            "AutomatedBilling",
                            subscription.Id.ToString(),
                            "Failed",
                            billingResult.Message,
                            tokenModel
                        );
                    }
                }
                catch (Exception ex)
                {
                    failedCount++;
                    _logger.LogError(ex, "Error processing billing for subscription {SubscriptionId}", subscription.Id);
                    await _auditService.LogPaymentEventAsync(
                        subscription.UserId,
                        "AutomatedBilling",
                        subscription.Id.ToString(),
                        "Error",
                        ex.Message,
                        tokenModel
                    );
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
            
            var subscription = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
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

            // Calculate new billing date based on billing cycle
            var newBillingDate = subscription.NextBillingDate;
            switch (subscription.BillingCycle.Name.ToLower())
            {
                case "monthly":
                    newBillingDate = newBillingDate.AddMonths(1);
                    break;
                case "quarterly":
                    newBillingDate = newBillingDate.AddMonths(3);
                    break;
                case "annually":
                    newBillingDate = newBillingDate.AddYears(1);
                    break;
                default:
                    newBillingDate = newBillingDate.AddMonths(1);
                    break;
            }

            // Update subscription
            subscription.NextBillingDate = newBillingDate;
            subscription.Status = Subscription.SubscriptionStatuses.Active;
            subscription.UpdatedDate = DateTime.UtcNow;
            
            await _subscriptionRepository.UpdateAsync(subscription);
            await _subscriptionRepository.SaveChangesAsync();

            // Log renewal
            await _auditService.LogPaymentEventAsync(
                subscription.UserId,
                "SubscriptionRenewal",
                subscription.Id.ToString(),
                "Success",
                $"Subscription renewed until {newBillingDate:yyyy-MM-dd}",
                tokenModel
            );

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
            
            var subscription = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
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
            subscription.UpdatedDate = DateTime.UtcNow;
            
            await _subscriptionRepository.UpdateAsync(subscription);
            await _subscriptionRepository.SaveChangesAsync();

            // Log plan change
            await _auditService.LogPaymentEventAsync(
                subscription.UserId,
                "PlanChange",
                subscription.Id.ToString(),
                "Success",
                $"Plan changed from {oldPlanId} to {newPlan.Id}. Proration: {prorationAmount:C}",
                tokenModel
            );

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
            // Get audit logs related to automation
            var logs = await _auditService.GetAuditLogsAsync(
                null, // action filter (will search for multiple actions in post-processing)
                null, // userId 
                null, // startDate
                null, // endDate
                page, 
                pageSize, 
                tokenModel
            );

            return new JsonModel
            {
                data = logs,
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

    private decimal CalculateProration(Subscription subscription, SubscriptionPlan newPlan, DateTime effectiveDate)
    {
        try
        {
            var daysRemaining = (subscription.NextBillingDate - effectiveDate).Days;
            var totalDaysInCycle = GetDaysInBillingCycle(subscription.BillingCycle);
            
            if (daysRemaining <= 0 || totalDaysInCycle <= 0)
                return 0;

            var dailyRateOld = subscription.CurrentPrice / totalDaysInCycle;
            var dailyRateNew = newPlan.Price / totalDaysInCycle;
            
            var creditForRemainingDays = dailyRateOld * daysRemaining;
            var chargeForRemainingDays = dailyRateNew * daysRemaining;
            
            return chargeForRemainingDays - creditForRemainingDays;
        }
        catch
        {
            return 0;
        }
    }

    private int GetDaysInBillingCycle(MasterBillingCycle billingCycle)
    {
        switch (billingCycle.Name.ToLower())
        {
            case "monthly":
                return 30;
            case "quarterly":
                return 90;
            case "annually":
                return 365;
            default:
                return 30;
        }
    }
}
