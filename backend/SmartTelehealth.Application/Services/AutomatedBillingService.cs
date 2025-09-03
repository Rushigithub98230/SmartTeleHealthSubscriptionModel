using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;

namespace SmartTelehealth.Application.Services;

public class AutomatedBillingService : IAutomatedBillingService
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IBillingService _billingService;
    private readonly IStripeService _stripeService;
      
    private readonly ILogger<AutomatedBillingService> _logger;

    public AutomatedBillingService(
        ISubscriptionRepository subscriptionRepository,
        IBillingService billingService,
        IStripeService stripeService,
          
        ILogger<AutomatedBillingService> logger)
    {
        _subscriptionRepository = subscriptionRepository;
        _billingService = billingService;
        _stripeService = stripeService;
          
        _logger = logger;
    }

    public async Task ProcessRecurringBillingAsync(TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Starting recurring billing process by user {UserId}", tokenModel?.UserID ?? 0);
            
            // Get all active subscriptions that are due for billing
            var dueSubscriptions = await _subscriptionRepository.GetSubscriptionsDueForBillingAsync(DateTime.UtcNow);
            
            foreach (var subscription in dueSubscriptions)
            {
                try
                {
                    await ProcessSubscriptionBillingAsync(subscription, tokenModel);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing billing for subscription {SubscriptionId} by user {UserId}", 
                        subscription.Id, tokenModel?.UserID ?? 0);
                }
            }
            
            _logger.LogInformation("Completed recurring billing process by user {UserId}", tokenModel?.UserID ?? 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in recurring billing process by user {UserId}", tokenModel?.UserID ?? 0);
            throw;
        }
    }

    public async Task ProcessSubscriptionRenewalAsync(TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Starting subscription renewal process by user {UserId}", tokenModel?.UserID ?? 0);
            
            // Get subscriptions that need renewal
            var renewals = await _subscriptionRepository.GetAllSubscriptionsAsync();
            renewals = renewals.Where(s => s.Status == Subscription.SubscriptionStatuses.Active && 
                                          s.EndDate.HasValue && 
                                          s.EndDate.Value <= DateTime.UtcNow.AddDays(7));
            
            foreach (var subscription in renewals)
            {
                try
                {
                    await ProcessSubscriptionRenewalAsync(subscription, tokenModel);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing renewal for subscription {SubscriptionId} by user {UserId}", 
                        subscription.Id, tokenModel?.UserID ?? 0);
                }
            }
            
            _logger.LogInformation("Completed subscription renewal process by user {UserId}", tokenModel?.UserID ?? 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in subscription renewal process by user {UserId}", tokenModel?.UserID ?? 0);
            throw;
        }
    }

    public async Task ProcessFailedPaymentRetryAsync(TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Starting failed payment retry process by user {UserId}", tokenModel?.UserID ?? 0);
            
            // Get subscriptions with failed payments that can be retried
            var failedSubscriptions = await _subscriptionRepository.GetSubscriptionsWithFailedPaymentsAsync();
            
            foreach (var subscription in failedSubscriptions)
            {
                try
                {
                    await ProcessFailedPaymentRetryAsync(subscription, tokenModel);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing failed payment retry for subscription {SubscriptionId} by user {UserId}", 
                        subscription.Id, tokenModel?.UserID ?? 0);
                }
            }
            
            _logger.LogInformation("Completed failed payment retry process by user {UserId}", tokenModel?.UserID ?? 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in failed payment retry process by user {UserId}", tokenModel?.UserID ?? 0);
            throw;
        }
    }

    public async Task ProcessPlanChangeAsync(Guid subscriptionId, Guid newPlanId, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Processing plan change for subscription {SubscriptionId} to plan {NewPlanId} by user {UserId}", 
                subscriptionId, newPlanId, tokenModel?.UserID ?? 0);
            
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found for plan change by user {UserId}", 
                    subscriptionId, tokenModel?.UserID ?? 0);
                return;
            }

            // Calculate prorated amount for the plan change
            var proratedAmount = await CalculateProratedAmountAsync(subscriptionId, DateTime.UtcNow, tokenModel);
            
            // Process the plan change
            subscription.SubscriptionPlanId = newPlanId;
            subscription.UpdatedDate = DateTime.UtcNow;
            
            await _subscriptionRepository.UpdateAsync(subscription);
            
            
            
            _logger.LogInformation("Successfully processed plan change for subscription {SubscriptionId} by user {UserId}", 
                subscriptionId, tokenModel?.UserID ?? 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing plan change for subscription {SubscriptionId} by user {UserId}", 
                subscriptionId, tokenModel?.UserID ?? 0);
            throw;
        }
    }

    public async Task ProcessManualBillingAsync(Guid subscriptionId, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Processing manual billing for subscription {SubscriptionId} by user {UserId}", 
                subscriptionId, tokenModel?.UserID ?? 0);
            
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found for manual billing by user {UserId}", 
                    subscriptionId, tokenModel?.UserID ?? 0);
                return;
            }

            // Process manual billing
            await ProcessSubscriptionBillingAsync(subscription, tokenModel);
            
            _logger.LogInformation("Successfully processed manual billing for subscription {SubscriptionId} by user {UserId}", 
                subscriptionId, tokenModel?.UserID ?? 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing manual billing for subscription {SubscriptionId} by user {UserId}", 
                subscriptionId, tokenModel?.UserID ?? 0);
            throw;
        }
    }

    public async Task<PaymentResultDto> ProcessPaymentAsync(Guid subscriptionId, decimal amount, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Processing payment for subscription {SubscriptionId} amount {Amount} by user {UserId}", 
                subscriptionId, amount, tokenModel?.UserID ?? 0);
            
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found for payment processing by user {UserId}", 
                    subscriptionId, tokenModel?.UserID ?? 0);
                return new PaymentResultDto { Status = "failed", ErrorMessage = "Subscription not found" };
            }

            // Validate billing cycle
            if (!await ValidateBillingCycleAsync(subscriptionId, tokenModel))
            {
                _logger.LogWarning("Invalid billing cycle for subscription {SubscriptionId} by user {UserId}", 
                    subscriptionId, tokenModel?.UserID ?? 0);
                return new PaymentResultDto { Status = "failed", ErrorMessage = "Invalid billing cycle" };
            }

            // Process payment through Stripe
            var paymentResult = await _stripeService.ProcessPaymentAsync(
                subscription.PaymentMethodId, 
                amount, 
                subscription.Currency, 
                tokenModel);

            if (paymentResult.Status == "succeeded")
            {
                // Update subscription status
                subscription.Status = Subscription.SubscriptionStatuses.Active;
                subscription.UpdatedDate = DateTime.UtcNow;
                await _subscriptionRepository.UpdateAsync(subscription);
                
                // Log audit trail
                if (tokenModel != null)
                {
                   
                }
            }
            else
            {
                // Log failed payment
                if (tokenModel != null)
                {
                    
                }
            }
            
            _logger.LogInformation("Payment processing completed for subscription {SubscriptionId} by user {UserId}: {Status}", 
                subscriptionId, tokenModel?.UserID ?? 0, paymentResult.Status);
            return paymentResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment for subscription {SubscriptionId} by user {UserId}", 
                subscriptionId, tokenModel?.UserID ?? 0);
            return new PaymentResultDto { Status = "failed", ErrorMessage = ex.Message };
        }
    }

    public async Task<bool> ValidateBillingCycleAsync(Guid subscriptionId, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Validating billing cycle for subscription {SubscriptionId} by user {UserId}", 
                subscriptionId, tokenModel?.UserID ?? 0);
            
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found for billing cycle validation by user {UserId}", 
                    subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            // Check if subscription is active and has a valid billing cycle
            var isValid = subscription.Status == Subscription.SubscriptionStatuses.Active && 
                         subscription.BillingCycle != null &&
                         subscription.BillingCycle.IsActive;

            _logger.LogInformation("Billing cycle validation for subscription {SubscriptionId} by user {UserId}: {IsValid}", 
                subscriptionId, tokenModel?.UserID ?? 0, isValid);
            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating billing cycle for subscription {SubscriptionId} by user {UserId}", 
                subscriptionId, tokenModel?.UserID ?? 0);
            return false;
        }
    }

    public async Task<DateTime> CalculateNextBillingDateAsync(Guid subscriptionId, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Calculating next billing date for subscription {SubscriptionId} by user {UserId}", 
                subscriptionId, tokenModel?.UserID ?? 0);
            
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found for next billing date calculation by user {UserId}", 
                    subscriptionId, tokenModel?.UserID ?? 0);
                return DateTime.UtcNow;
            }

            // CRITICAL FIX: Sync with Stripe subscription's current_period_end if available
            if (!string.IsNullOrEmpty(subscription.StripeSubscriptionId))
            {
                try
                {
                    var stripeSubscription = await _stripeService.GetSubscriptionAsync(subscription.StripeSubscriptionId, tokenModel);
                    if (stripeSubscription != null && stripeSubscription.CurrentPeriodEnd.HasValue)
                    {
                        _logger.LogInformation("Using Stripe subscription period end for subscription {SubscriptionId}: {PeriodEnd}", 
                            subscriptionId, stripeSubscription.CurrentPeriodEnd.Value);
                        return stripeSubscription.CurrentPeriodEnd.Value;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get Stripe subscription period end for {SubscriptionId}, falling back to local calculation", 
                        subscriptionId);
                }
            }

            var nextBillingDate = DateTime.UtcNow;
            
            // Calculate next billing date based on billing cycle
            if (subscription.BillingCycle != null)
            {
                switch (subscription.BillingCycle.Name)
                {
                    case "Monthly":
                        nextBillingDate = DateTime.UtcNow.AddMonths(1);
                        break;
                    case "Quarterly":
                        nextBillingDate = DateTime.UtcNow.AddMonths(3);
                        break;
                    case "Annually":
                        nextBillingDate = DateTime.UtcNow.AddYears(1);
                        break;
                    default:
                        nextBillingDate = DateTime.UtcNow.AddMonths(1);
                        break;
                }
            }

            _logger.LogInformation("Next billing date calculated for subscription {SubscriptionId} by user {UserId}: {NextBillingDate}", 
                subscriptionId, tokenModel?.UserID ?? 0, nextBillingDate);
            return nextBillingDate;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating next billing date for subscription {SubscriptionId} by user {UserId}", 
                subscriptionId, tokenModel?.UserID ?? 0);
            return DateTime.UtcNow;
        }
    }

    public async Task<decimal> CalculateProratedAmountAsync(Guid subscriptionId, DateTime effectiveDate, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Calculating prorated amount for subscription {SubscriptionId} effective {EffectiveDate} by user {UserId}", 
                subscriptionId, effectiveDate, tokenModel?.UserID ?? 0);
            
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found for prorated amount calculation by user {UserId}", 
                    subscriptionId, tokenModel?.UserID ?? 0);
                return 0;
            }

            // Calculate prorated amount based on effective date
            var daysInMonth = DateTime.DaysInMonth(effectiveDate.Year, effectiveDate.Month);
            var daysRemaining = daysInMonth - effectiveDate.Day + 1;
            var proratedAmount = (subscription.Amount / daysInMonth) * daysRemaining;

            _logger.LogInformation("Prorated amount calculated for subscription {SubscriptionId} by user {UserId}: {ProratedAmount}", 
                subscriptionId, tokenModel?.UserID ?? 0, proratedAmount);
            return proratedAmount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating prorated amount for subscription {SubscriptionId} by user {UserId}", 
                subscriptionId, tokenModel?.UserID ?? 0);
            return 0;
        }
    }

    // Helper methods
    private async Task ProcessSubscriptionBillingAsync(Subscription subscription, TokenModel tokenModel)
    {
        // Implementation for processing individual subscription billing
        _logger.LogInformation("Processing billing for subscription {SubscriptionId} by user {UserId}", 
            subscription.Id, tokenModel?.UserID ?? 0);
        
        // TODO: Implement billing logic
    }

    private async Task ProcessSubscriptionRenewalAsync(Subscription subscription, TokenModel tokenModel)
    {
        // Implementation for processing individual subscription renewal
        _logger.LogInformation("Processing renewal for subscription {SubscriptionId} by user {UserId}", 
            subscription.Id, tokenModel?.UserID ?? 0);
        
        // TODO: Implement renewal logic
    }

    private async Task ProcessFailedPaymentRetryAsync(Subscription subscription, TokenModel tokenModel)
    {
        // Implementation for processing failed payment retry
        _logger.LogInformation("Processing failed payment retry for subscription {SubscriptionId} by user {UserId}", 
            subscription.Id, tokenModel?.UserID ?? 0);
        
        // TODO: Implement retry logic
    }
} 