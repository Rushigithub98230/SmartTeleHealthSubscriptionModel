using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;

namespace SmartTelehealth.Application.Services;

/// <summary>
/// Service responsible for automated billing operations and payment processing.
/// This service handles recurring billing, subscription renewals, failed payment retries,
/// plan changes, manual billing, and payment validation. It provides comprehensive billing
/// automation with Stripe integration, proration calculations, and billing cycle management.
/// 
/// Key Features:
/// - Automated recurring billing processing
/// - Subscription renewal automation
/// - Failed payment retry mechanisms
/// - Plan change processing with proration
/// - Manual billing capabilities
/// - Payment processing through Stripe
/// - Billing cycle validation
/// - Next billing date calculations
/// - Prorated amount calculations
/// - Comprehensive error handling and logging
/// - Integration with subscription and billing services
/// </summary>
public class AutomatedBillingService : IAutomatedBillingService
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IBillingService _billingService;
    private readonly IStripeService _stripeService;
      
    private readonly ILogger<AutomatedBillingService> _logger;

    /// <summary>
    /// Initializes a new instance of the AutomatedBillingService
    /// </summary>
    /// <param name="subscriptionRepository">Repository for subscription data access operations</param>
    /// <param name="billingService">Service for billing record management and processing</param>
    /// <param name="stripeService">Service for Stripe payment processing integration</param>
    /// <param name="logger">Logger instance for recording service operations and errors</param>
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

            var billingCycle = subscription.BillingCycle;
            if (billingCycle == null)
            {
                _logger.LogWarning("No billing cycle found for subscription {SubscriptionId}", subscriptionId);
                return subscription.CurrentPrice;
            }

            // Calculate prorated amount based on billing cycle
            var proratedAmount = billingCycle.Name.ToLower() switch
            {
                "monthly" => CalculateMonthlyProration(subscription, effectiveDate),
                "quarterly" => CalculateQuarterlyProration(subscription, effectiveDate),
                "yearly" => CalculateYearlyProration(subscription, effectiveDate),
                "weekly" => CalculateWeeklyProration(subscription, effectiveDate),
                "daily" => CalculateDailyProration(subscription, effectiveDate),
                _ => CalculateMonthlyProration(subscription, effectiveDate)
            };

            // Ensure minimum amount
            proratedAmount = Math.Max(proratedAmount, 0.01m);

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

    /// <summary>
    /// Calculates monthly proration
    /// </summary>
    private decimal CalculateMonthlyProration(Subscription subscription, DateTime effectiveDate)
    {
        var daysInMonth = DateTime.DaysInMonth(effectiveDate.Year, effectiveDate.Month);
        var daysRemaining = daysInMonth - effectiveDate.Day + 1;
        return (subscription.CurrentPrice / daysInMonth) * daysRemaining;
    }

    /// <summary>
    /// Calculates quarterly proration
    /// </summary>
    private decimal CalculateQuarterlyProration(Subscription subscription, DateTime effectiveDate)
    {
        var quarterStart = new DateTime(effectiveDate.Year, ((effectiveDate.Month - 1) / 3) * 3 + 1, 1);
        var quarterEnd = quarterStart.AddMonths(3).AddDays(-1);
        var totalDaysInQuarter = (quarterEnd - quarterStart).Days + 1;
        var daysRemaining = (quarterEnd - effectiveDate).Days + 1;
        return (subscription.CurrentPrice / totalDaysInQuarter) * daysRemaining;
    }

    /// <summary>
    /// Calculates yearly proration
    /// </summary>
    private decimal CalculateYearlyProration(Subscription subscription, DateTime effectiveDate)
    {
        var yearStart = new DateTime(effectiveDate.Year, 1, 1);
        var yearEnd = new DateTime(effectiveDate.Year, 12, 31);
        var totalDaysInYear = (yearEnd - yearStart).Days + 1;
        var daysRemaining = (yearEnd - effectiveDate).Days + 1;
        return (subscription.CurrentPrice / totalDaysInYear) * daysRemaining;
    }

    /// <summary>
    /// Calculates weekly proration
    /// </summary>
    private decimal CalculateWeeklyProration(Subscription subscription, DateTime effectiveDate)
    {
        var weekStart = effectiveDate.AddDays(-(int)effectiveDate.DayOfWeek);
        var weekEnd = weekStart.AddDays(6);
        var totalDaysInWeek = 7;
        var daysRemaining = (weekEnd - effectiveDate).Days + 1;
        return (subscription.CurrentPrice / totalDaysInWeek) * daysRemaining;
    }

    /// <summary>
    /// Calculates daily proration
    /// </summary>
    private decimal CalculateDailyProration(Subscription subscription, DateTime effectiveDate)
    {
        // For daily billing, return the full amount
        return subscription.CurrentPrice;
    }

    // Helper methods
    private async Task ProcessSubscriptionBillingAsync(Subscription subscription, TokenModel tokenModel)
    {
        try
        {
        _logger.LogInformation("Processing billing for subscription {SubscriptionId} by user {UserId}", 
            subscription.Id, tokenModel?.UserID ?? 0);
        
            // Step 1: Validate subscription is eligible for billing
            if (!await ValidateSubscriptionForBillingAsync(subscription, tokenModel))
            {
                _logger.LogWarning("Subscription {SubscriptionId} is not eligible for billing", subscription.Id);
                return;
            }

            // Step 2: Calculate billing amount (including proration if needed)
            var billingAmount = await CalculateBillingAmountAsync(subscription, tokenModel);
            if (billingAmount <= 0)
            {
                _logger.LogWarning("Billing amount is zero or negative for subscription {SubscriptionId}", subscription.Id);
                return;
            }

            // Step 3: Create billing record
            var billingRecordDto = new CreateBillingRecordDto
            {
                UserId = subscription.UserId,
                SubscriptionId = subscription.Id.ToString(),
                Amount = billingAmount,
                CurrencyId = null, // Currency is hardcoded to USD in Subscription entity
                PaymentMethod = "stripe",
                Status = BillingRecord.BillingStatus.Pending.ToString(),
                Description = $"Automated billing for {subscription.SubscriptionPlan?.Name ?? "subscription"} - {subscription.BillingCycle?.Name ?? "monthly"}",
                BillingDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(7), // 7-day grace period
                Type = BillingRecord.BillingType.Subscription.ToString(),
                // StripeSubscriptionId is not available in CreateBillingRecordDto
            };

            var billingResult = await _billingService.CreateBillingRecordAsync(billingRecordDto, tokenModel);
            if (billingResult.StatusCode != 200)
            {
                _logger.LogError("Failed to create billing record for subscription {SubscriptionId}: {Error}", 
                    subscription.Id, billingResult.Message);
                return;
            }

            // Step 4: Process payment through Stripe
            var paymentResult = await ProcessPaymentThroughStripeAsync(subscription, billingAmount, tokenModel);
            
            // Step 5: Update subscription and billing record based on payment result
            await UpdateSubscriptionAfterBillingAsync(subscription, paymentResult, billingRecordDto, tokenModel);

            _logger.LogInformation("Successfully processed billing for subscription {SubscriptionId} with amount {Amount}", 
                subscription.Id, billingAmount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing billing for subscription {SubscriptionId}", subscription.Id);
            throw;
        }
    }

    private async Task ProcessSubscriptionRenewalAsync(Subscription subscription, TokenModel tokenModel)
    {
        try
        {
        _logger.LogInformation("Processing renewal for subscription {SubscriptionId} by user {UserId}", 
            subscription.Id, tokenModel?.UserID ?? 0);
        
            // Step 1: Validate subscription is eligible for renewal
            if (!await ValidateSubscriptionForRenewalAsync(subscription, tokenModel))
            {
                _logger.LogWarning("Subscription {SubscriptionId} is not eligible for renewal", subscription.Id);
                return;
            }

            // Step 2: Calculate renewal amount
            var renewalAmount = await CalculateRenewalAmountAsync(subscription, tokenModel);
            if (renewalAmount <= 0)
            {
                _logger.LogWarning("Renewal amount is zero or negative for subscription {SubscriptionId}", subscription.Id);
                return;
            }

            // Step 3: Process renewal payment
            var paymentResult = await ProcessPaymentThroughStripeAsync(subscription, renewalAmount, tokenModel);
            
            if (paymentResult.Status == "succeeded")
            {
                // Step 4: Update subscription for renewal
                await UpdateSubscriptionForRenewalAsync(subscription, tokenModel);
                
                // Step 5: Create renewal billing record
                var renewalBillingDto = new CreateBillingRecordDto
                {
                    UserId = subscription.UserId,
                    SubscriptionId = subscription.Id.ToString(),
                    Amount = renewalAmount,
                    CurrencyId = null, // Currency is hardcoded to USD in Subscription entity
                    PaymentMethod = "stripe",
                    Status = BillingRecord.BillingStatus.Paid.ToString(),
                    Description = $"Subscription renewal for {subscription.SubscriptionPlan?.Name ?? "subscription"}",
                    BillingDate = DateTime.UtcNow,
                    PaidDate = DateTime.UtcNow,
                    Type = BillingRecord.BillingType.Subscription.ToString(),
                    // StripeSubscriptionId is not available in CreateBillingRecordDto,
                    StripePaymentIntentId = paymentResult.PaymentIntentId
                };

                await _billingService.CreateBillingRecordAsync(renewalBillingDto, tokenModel);
                
                _logger.LogInformation("Successfully renewed subscription {SubscriptionId} with amount {Amount}", 
                    subscription.Id, renewalAmount);
            }
            else
            {
                // Step 4: Handle renewal failure
                await HandleRenewalFailureAsync(subscription, paymentResult, tokenModel);
                
                _logger.LogWarning("Failed to renew subscription {SubscriptionId}: {Error}", 
                    subscription.Id, paymentResult.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing renewal for subscription {SubscriptionId}", subscription.Id);
            throw;
        }
    }

    private async Task ProcessFailedPaymentRetryAsync(Subscription subscription, TokenModel tokenModel)
    {
        try
        {
        _logger.LogInformation("Processing failed payment retry for subscription {SubscriptionId} by user {UserId}", 
            subscription.Id, tokenModel?.UserID ?? 0);
        
            // Step 1: Check if subscription is eligible for retry
            if (!await ValidateSubscriptionForRetryAsync(subscription, tokenModel))
            {
                _logger.LogWarning("Subscription {SubscriptionId} is not eligible for payment retry", subscription.Id);
                return;
            }

            // Step 2: Get the most recent failed billing record
            var failedBillingRecord = await GetMostRecentFailedBillingRecordAsync(subscription.Id, tokenModel);
            if (failedBillingRecord == null)
            {
                _logger.LogWarning("No failed billing record found for subscription {SubscriptionId}", subscription.Id);
                return;
            }

            // Step 3: Calculate retry amount (may include late fees)
            var retryAmount = await CalculateRetryAmountAsync(subscription, failedBillingRecord, tokenModel);
            
            // Step 4: Process retry payment
            var paymentResult = await ProcessPaymentThroughStripeAsync(subscription, retryAmount, tokenModel);
            
            if (paymentResult.Status == "succeeded")
            {
                // Step 5: Update subscription and billing record for successful retry
                await UpdateSubscriptionAfterSuccessfulRetryAsync(subscription, paymentResult, failedBillingRecord, tokenModel);
                
                _logger.LogInformation("Successfully retried payment for subscription {SubscriptionId} with amount {Amount}", 
                    subscription.Id, retryAmount);
            }
            else
            {
                // Step 5: Handle retry failure
                await HandleRetryFailureAsync(subscription, paymentResult, failedBillingRecord, tokenModel);
                
                _logger.LogWarning("Failed to retry payment for subscription {SubscriptionId}: {Error}", 
                    subscription.Id, paymentResult.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing failed payment retry for subscription {SubscriptionId}", subscription.Id);
            throw;
        }
    }

    #region Helper Methods for Automated Billing

    /// <summary>
    /// Validates if a subscription is eligible for billing
    /// </summary>
    private async Task<bool> ValidateSubscriptionForBillingAsync(Subscription subscription, TokenModel tokenModel)
    {
        try
        {
            // Check if subscription is active
            if (subscription.Status != Subscription.SubscriptionStatuses.Active)
            {
                _logger.LogDebug("Subscription {SubscriptionId} is not active (status: {Status})", 
                    subscription.Id, subscription.Status);
                return false;
            }

            // Check if subscription has a valid payment method
            if (string.IsNullOrEmpty(subscription.PaymentMethodId))
            {
                _logger.LogDebug("Subscription {SubscriptionId} has no payment method", subscription.Id);
                return false;
            }

            // Check if subscription is not paused
            if (subscription.IsPaused)
            {
                _logger.LogDebug("Subscription {SubscriptionId} is paused", subscription.Id);
                return false;
            }

            // Check if billing date is due
            if (subscription.NextBillingDate > DateTime.UtcNow)
            {
                _logger.LogDebug("Subscription {SubscriptionId} billing date is not due yet", subscription.Id);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating subscription {SubscriptionId} for billing", subscription.Id);
            return false;
        }
    }

    /// <summary>
    /// Validates if a subscription is eligible for renewal
    /// </summary>
    private async Task<bool> ValidateSubscriptionForRenewalAsync(Subscription subscription, TokenModel tokenModel)
    {
        try
        {
            // Check if subscription is active or trial
            if (subscription.Status != Subscription.SubscriptionStatuses.Active && 
                subscription.Status != Subscription.SubscriptionStatuses.TrialActive)
            {
                _logger.LogDebug("Subscription {SubscriptionId} is not eligible for renewal (status: {Status})", 
                    subscription.Id, subscription.Status);
                return false;
            }

            // Check if subscription is near expiration
            if (subscription.EndDate.HasValue && subscription.EndDate.Value > DateTime.UtcNow.AddDays(7))
            {
                _logger.LogDebug("Subscription {SubscriptionId} is not near expiration", subscription.Id);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating subscription {SubscriptionId} for renewal", subscription.Id);
            return false;
        }
    }

    /// <summary>
    /// Validates if a subscription is eligible for payment retry
    /// </summary>
    private async Task<bool> ValidateSubscriptionForRetryAsync(Subscription subscription, TokenModel tokenModel)
    {
        try
        {
            // Check if subscription has failed payments
            if (subscription.Status != Subscription.SubscriptionStatuses.PaymentFailed)
            {
                _logger.LogDebug("Subscription {SubscriptionId} does not have failed payments", subscription.Id);
                return false;
            }

            // Check retry count (max 3 retries)
            if (subscription.FailedPaymentAttempts >= 3)
            {
                _logger.LogDebug("Subscription {SubscriptionId} has exceeded max retry attempts", subscription.Id);
                return false;
            }

            // Check if enough time has passed since last retry (24 hours)
            if (subscription.LastPaymentFailedDate.HasValue && 
                subscription.LastPaymentFailedDate.Value > DateTime.UtcNow.AddHours(-24))
            {
                _logger.LogDebug("Subscription {SubscriptionId} retry too soon", subscription.Id);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating subscription {SubscriptionId} for retry", subscription.Id);
            return false;
        }
    }

    /// <summary>
    /// Calculates the billing amount for a subscription
    /// </summary>
    private async Task<decimal> CalculateBillingAmountAsync(Subscription subscription, TokenModel tokenModel)
    {
        try
        {
            var baseAmount = subscription.CurrentPrice;
            
            // Apply any discounts or adjustments
            var discountAmount = await CalculateDiscountAmountAsync(subscription, tokenModel);
            var adjustmentAmount = await CalculateAdjustmentAmountAsync(subscription, tokenModel);
            
            var totalAmount = baseAmount - discountAmount + adjustmentAmount;
            
            // Ensure minimum amount
            return Math.Max(totalAmount, 0.01m);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating billing amount for subscription {SubscriptionId}", subscription.Id);
            return subscription.CurrentPrice;
        }
    }

    /// <summary>
    /// Calculates the renewal amount for a subscription
    /// </summary>
    private async Task<decimal> CalculateRenewalAmountAsync(Subscription subscription, TokenModel tokenModel)
    {
        try
        {
            // For renewals, use the current plan price
            var renewalAmount = subscription.SubscriptionPlan?.Price ?? subscription.CurrentPrice;
            
            // Apply any renewal discounts
            var renewalDiscount = await CalculateRenewalDiscountAsync(subscription, tokenModel);
            
            return Math.Max(renewalAmount - renewalDiscount, 0.01m);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating renewal amount for subscription {SubscriptionId}", subscription.Id);
            return subscription.CurrentPrice;
        }
    }

    /// <summary>
    /// Calculates the retry amount for a failed payment
    /// </summary>
    private async Task<decimal> CalculateRetryAmountAsync(Subscription subscription, BillingRecord failedRecord, TokenModel tokenModel)
    {
        try
        {
            var baseAmount = failedRecord.Amount;
            
            // Add late fees if applicable
            var lateFee = await CalculateLateFeeAsync(subscription, failedRecord, tokenModel);
            
            return baseAmount + lateFee;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating retry amount for subscription {SubscriptionId}", subscription.Id);
            return failedRecord.Amount;
        }
    }

    /// <summary>
    /// Processes payment through Stripe
    /// </summary>
    private async Task<PaymentResultDto> ProcessPaymentThroughStripeAsync(Subscription subscription, decimal amount, TokenModel tokenModel)
    {
        try
        {
            return await _stripeService.ProcessPaymentAsync(
                subscription.PaymentMethodId,
                amount,
                subscription.Currency ?? "usd",
                tokenModel
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment through Stripe for subscription {SubscriptionId}", subscription.Id);
            return new PaymentResultDto
            {
                Status = "failed",
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Updates subscription after successful billing
    /// </summary>
    private async Task UpdateSubscriptionAfterBillingAsync(Subscription subscription, PaymentResultDto paymentResult, CreateBillingRecordDto billingRecord, TokenModel tokenModel)
    {
        try
        {
            if (paymentResult.Status == "succeeded")
            {
                // Update subscription status and next billing date
                subscription.Status = Subscription.SubscriptionStatuses.Active;
                subscription.LastPaymentDate = DateTime.UtcNow;
                subscription.FailedPaymentAttempts = 0;
                subscription.LastPaymentError = null;
                subscription.NextBillingDate = CalculateNextBillingDate(subscription);
                subscription.UpdatedDate = DateTime.UtcNow;
                
                await _subscriptionRepository.UpdateAsync(subscription);
                
                _logger.LogInformation("Updated subscription {SubscriptionId} after successful billing", subscription.Id);
            }
            else
            {
                // Handle payment failure
                subscription.Status = Subscription.SubscriptionStatuses.PaymentFailed;
                subscription.FailedPaymentAttempts++;
                subscription.LastPaymentFailedDate = DateTime.UtcNow;
                subscription.LastPaymentError = paymentResult.ErrorMessage;
                subscription.UpdatedDate = DateTime.UtcNow;
                
                await _subscriptionRepository.UpdateAsync(subscription);
                
                _logger.LogWarning("Updated subscription {SubscriptionId} after failed billing", subscription.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating subscription {SubscriptionId} after billing", subscription.Id);
            throw;
        }
    }

    /// <summary>
    /// Updates subscription for renewal
    /// </summary>
    private async Task UpdateSubscriptionForRenewalAsync(Subscription subscription, TokenModel tokenModel)
    {
        try
        {
            subscription.Status = Subscription.SubscriptionStatuses.Active;
            subscription.LastPaymentDate = DateTime.UtcNow;
            subscription.FailedPaymentAttempts = 0;
            subscription.LastPaymentError = null;
            subscription.NextBillingDate = CalculateNextBillingDate(subscription);
            subscription.UpdatedDate = DateTime.UtcNow;
            
            // Extend subscription end date
            if (subscription.EndDate.HasValue)
            {
                subscription.EndDate = subscription.EndDate.Value.AddMonths(1); // Assuming monthly billing
            }
            
            await _subscriptionRepository.UpdateAsync(subscription);
            
            _logger.LogInformation("Updated subscription {SubscriptionId} for renewal", subscription.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating subscription {SubscriptionId} for renewal", subscription.Id);
            throw;
        }
    }

    /// <summary>
    /// Handles renewal failure
    /// </summary>
    private async Task HandleRenewalFailureAsync(Subscription subscription, PaymentResultDto paymentResult, TokenModel tokenModel)
    {
        try
        {
            subscription.Status = Subscription.SubscriptionStatuses.PaymentFailed;
            subscription.FailedPaymentAttempts++;
            subscription.LastPaymentFailedDate = DateTime.UtcNow;
            subscription.LastPaymentError = paymentResult.ErrorMessage;
            subscription.UpdatedDate = DateTime.UtcNow;
            
            await _subscriptionRepository.UpdateAsync(subscription);
            
            _logger.LogWarning("Handled renewal failure for subscription {SubscriptionId}", subscription.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling renewal failure for subscription {SubscriptionId}", subscription.Id);
            throw;
        }
    }

    /// <summary>
    /// Updates subscription after successful retry
    /// </summary>
    private async Task UpdateSubscriptionAfterSuccessfulRetryAsync(Subscription subscription, PaymentResultDto paymentResult, BillingRecord failedRecord, TokenModel tokenModel)
    {
        try
        {
            subscription.Status = Subscription.SubscriptionStatuses.Active;
            subscription.LastPaymentDate = DateTime.UtcNow;
            subscription.FailedPaymentAttempts = 0;
            subscription.LastPaymentError = null;
            subscription.NextBillingDate = CalculateNextBillingDate(subscription);
            subscription.UpdatedDate = DateTime.UtcNow;
            
            // Update the failed billing record
            failedRecord.Status = BillingRecord.BillingStatus.Paid;
            failedRecord.PaidAt = DateTime.UtcNow;
            failedRecord.ProcessedAt = DateTime.UtcNow;
            failedRecord.UpdatedDate = DateTime.UtcNow;
            failedRecord.StripePaymentIntentId = paymentResult.PaymentIntentId;
            failedRecord.FailureReason = null;
            
            await _subscriptionRepository.UpdateAsync(subscription);
            
            _logger.LogInformation("Updated subscription {SubscriptionId} after successful retry", subscription.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating subscription {SubscriptionId} after successful retry", subscription.Id);
            throw;
        }
    }

    /// <summary>
    /// Handles retry failure
    /// </summary>
    private async Task HandleRetryFailureAsync(Subscription subscription, PaymentResultDto paymentResult, BillingRecord failedRecord, TokenModel tokenModel)
    {
        try
        {
            subscription.FailedPaymentAttempts++;
            subscription.LastPaymentFailedDate = DateTime.UtcNow;
            subscription.LastPaymentError = paymentResult.ErrorMessage;
            subscription.UpdatedDate = DateTime.UtcNow;
            
            // If max retries reached, suspend subscription
            if (subscription.FailedPaymentAttempts >= 3)
            {
                subscription.Status = Subscription.SubscriptionStatuses.Suspended;
                _logger.LogWarning("Suspended subscription {SubscriptionId} after max retry attempts", subscription.Id);
            }
            
            await _subscriptionRepository.UpdateAsync(subscription);
            
            _logger.LogWarning("Handled retry failure for subscription {SubscriptionId}", subscription.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling retry failure for subscription {SubscriptionId}", subscription.Id);
            throw;
        }
    }

    /// <summary>
    /// Gets the most recent failed billing record for a subscription
    /// </summary>
    private async Task<BillingRecord?> GetMostRecentFailedBillingRecordAsync(Guid subscriptionId, TokenModel tokenModel)
    {
        try
        {
            var billingResult = await _billingService.GetSubscriptionBillingHistoryAsync(subscriptionId, tokenModel);
            if (billingResult.StatusCode != 200 || billingResult.data == null)
            {
                return null;
            }
            var billingRecords = (IEnumerable<BillingRecord>)billingResult.data;
            return billingRecords
                .Where(br => br.Status == BillingRecord.BillingStatus.Failed)
                .OrderByDescending(br => br.CreatedDate)
                .FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting failed billing record for subscription {SubscriptionId}", subscriptionId);
            return null;
        }
    }

    /// <summary>
    /// Calculates the next billing date for a subscription
    /// </summary>
    private DateTime CalculateNextBillingDate(Subscription subscription)
    {
        try
        {
            var billingCycle = subscription.BillingCycle;
            if (billingCycle == null)
            {
                // Default to monthly if no billing cycle specified
                return DateTime.UtcNow.AddMonths(1);
            }

            return billingCycle.Name.ToLower() switch
            {
                "monthly" => DateTime.UtcNow.AddMonths(1),
                "quarterly" => DateTime.UtcNow.AddMonths(3),
                "yearly" => DateTime.UtcNow.AddYears(1),
                "weekly" => DateTime.UtcNow.AddDays(7),
                "daily" => DateTime.UtcNow.AddDays(1),
                _ => DateTime.UtcNow.AddMonths(1)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating next billing date for subscription {SubscriptionId}", subscription.Id);
            return DateTime.UtcNow.AddMonths(1);
        }
    }

    /// <summary>
    /// Calculates discount amount for a subscription
    /// </summary>
    private async Task<decimal> CalculateDiscountAmountAsync(Subscription subscription, TokenModel tokenModel)
    {
        try
        {
            decimal totalDiscount = 0;
            
            // Check for subscription plan discounts
            if (subscription.SubscriptionPlan != null)
            {
                // Early bird discount for new subscriptions (first 30 days)
                if (subscription.CreatedDate > DateTime.UtcNow.AddDays(-30))
                {
                    var earlyBirdDiscount = subscription.CurrentPrice * 0.1m; // 10% early bird discount
                    totalDiscount += earlyBirdDiscount;
                    _logger.LogInformation("Applied early bird discount of {Discount} for subscription {SubscriptionId}", 
                        earlyBirdDiscount, subscription.Id);
                }
                
                // Volume discount for annual plans
                if (subscription.SubscriptionPlan.BillingCycle?.Name == "annual")
                {
                    var volumeDiscount = subscription.CurrentPrice * 0.15m; // 15% annual discount
                    totalDiscount += volumeDiscount;
                    _logger.LogInformation("Applied annual volume discount of {Discount} for subscription {SubscriptionId}", 
                        volumeDiscount, subscription.Id);
                }
                
                // Loyalty discount for long-term subscribers (6+ months)
                if (subscription.CreatedDate < DateTime.UtcNow.AddMonths(-6))
                {
                    var loyaltyDiscount = subscription.CurrentPrice * 0.05m; // 5% loyalty discount
                    totalDiscount += loyaltyDiscount;
                    _logger.LogInformation("Applied loyalty discount of {Discount} for subscription {SubscriptionId}", 
                        loyaltyDiscount, subscription.Id);
                }
            }
            
            // Check for promotional codes in subscription plan features (if available)
            if (!string.IsNullOrEmpty(subscription.SubscriptionPlan?.Features))
            {
                try
                {
                    var features = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(subscription.SubscriptionPlan.Features);
                    if (features != null && features.ContainsKey("promo_code"))
                    {
                        var promoCode = features["promo_code"].ToString();
                        if (!string.IsNullOrEmpty(promoCode))
                        {
                            // Apply promotional discount based on code
                            var promoDiscount = ApplyPromotionalDiscount(promoCode, subscription.CurrentPrice);
                            totalDiscount += promoDiscount;
                            _logger.LogInformation("Applied promotional discount of {Discount} for code {PromoCode} on subscription {SubscriptionId}", 
                                promoDiscount, promoCode, subscription.Id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse subscription plan features for promotional codes");
                }
            }
            
            // Ensure discount doesn't exceed the base amount
            return Math.Min(totalDiscount, subscription.CurrentPrice);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating discount for subscription {SubscriptionId}", subscription.Id);
            return 0;
        }
    }
    
    private decimal ApplyPromotionalDiscount(string promoCode, decimal baseAmount)
    {
        // Define promotional codes and their discounts
        var promoDiscounts = new Dictionary<string, decimal>
        {
            { "WELCOME10", 0.10m },    // 10% off
            { "SAVE20", 0.20m },       // 20% off
            { "FIRST50", 0.50m },      // 50% off first month
            { "STUDENT15", 0.15m },    // 15% off for students
            { "SENIOR20", 0.20m }      // 20% off for seniors
        };
        
        if (promoDiscounts.TryGetValue(promoCode.ToUpper(), out var discountPercentage))
        {
            return baseAmount * discountPercentage;
        }
        
        return 0;
    }

    /// <summary>
    /// Calculates adjustment amount for a subscription
    /// </summary>
    private async Task<decimal> CalculateAdjustmentAmountAsync(Subscription subscription, TokenModel tokenModel)
    {
        try
        {
            decimal totalAdjustment = 0;
            
            // Check for subscription plan adjustments
            if (subscription.SubscriptionPlan != null)
            {
                // Overage charges for usage-based plans
                // Note: PlanType property doesn't exist in SubscriptionPlan entity, using a placeholder for future implementation
                // TODO: Add PlanType property to SubscriptionPlan entity when needed
                /*
                if (subscription.SubscriptionPlan.PlanType == "usage_based")
                {
                    var overageCharge = await CalculateOverageChargeAsync(subscription);
                    totalAdjustment += overageCharge;
                    if (overageCharge > 0)
                    {
                        _logger.LogInformation("Applied overage charge of {Charge} for subscription {SubscriptionId}", 
                            overageCharge, subscription.Id);
                    }
                }
                */
                
                // Late payment fees
                if (subscription.Status == Subscription.SubscriptionStatuses.PaymentFailed)
                {
                    var lateFee = subscription.CurrentPrice * 0.05m; // 5% late fee
                    totalAdjustment += lateFee;
                    _logger.LogInformation("Applied late payment fee of {Fee} for subscription {SubscriptionId}", 
                        lateFee, subscription.Id);
                }
                
                // Service charges for premium features
                // Note: PlanType property doesn't exist in SubscriptionPlan entity, using a placeholder for future implementation
                // TODO: Add PlanType property to SubscriptionPlan entity when needed
                /*
                if (subscription.SubscriptionPlan.PlanType == "premium")
                {
                    var serviceCharge = subscription.CurrentPrice * 0.02m; // 2% service charge
                    totalAdjustment += serviceCharge;
                    _logger.LogInformation("Applied service charge of {Charge} for premium subscription {SubscriptionId}", 
                        serviceCharge, subscription.Id);
                }
                */
            }
            
            // Check for manual adjustments in subscription plan features (if available)
            if (!string.IsNullOrEmpty(subscription.SubscriptionPlan?.Features))
            {
                try
                {
                    var features = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(subscription.SubscriptionPlan.Features);
                    if (features != null && features.ContainsKey("manual_adjustment"))
                    {
                        if (decimal.TryParse(features["manual_adjustment"].ToString(), out var manualAdjustment))
                        {
                            totalAdjustment += manualAdjustment;
                            _logger.LogInformation("Applied manual adjustment of {Adjustment} for subscription {SubscriptionId}", 
                                manualAdjustment, subscription.Id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error parsing subscription plan features for manual adjustment on subscription {SubscriptionId}", subscription.Id);
                }
            }
            
            return totalAdjustment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating adjustment for subscription {SubscriptionId}", subscription.Id);
            return 0;
        }
    }
    
    private async Task<decimal> CalculateOverageChargeAsync(Subscription subscription)
    {
        try
        {
            // This is a simplified overage calculation
            // In a real implementation, you would check actual usage against limits
            
            // For now, return 0 as we don't have usage tracking entities yet
            // TODO: Implement actual usage tracking when entities are available
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating overage charge for subscription {SubscriptionId}", subscription.Id);
            return 0;
        }
    }

    /// <summary>
    /// Calculates renewal discount for a subscription
    /// </summary>
    private async Task<decimal> CalculateRenewalDiscountAsync(Subscription subscription, TokenModel tokenModel)
    {
        // TODO: Implement renewal discount calculation logic
        return 0;
    }

    /// <summary>
    /// Calculates late fee for a failed payment
    /// </summary>
    private async Task<decimal> CalculateLateFeeAsync(Subscription subscription, BillingRecord failedRecord, TokenModel tokenModel)
    {
        try
        {
            // Calculate days overdue
            var daysOverdue = failedRecord.DueDate.HasValue ? (DateTime.UtcNow - failedRecord.DueDate.Value).Days : 0;
            if (daysOverdue <= 0) return 0;

            // Apply late fee (e.g., $5 per week overdue)
            var lateFeePerWeek = 5.00m;
            var weeksOverdue = (int)Math.Ceiling(daysOverdue / 7.0);
            
            return Math.Min(lateFeePerWeek * weeksOverdue, 50.00m); // Cap at $50
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating late fee for subscription {SubscriptionId}", subscription.Id);
            return 0;
        }
    }

    #endregion

    #region Billing Cycle Management

    /// <summary>
    /// Processes all subscriptions in a specific billing cycle
    /// </summary>
    public async Task ProcessBillingCycleAsync(Guid billingCycleId, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Processing billing cycle {BillingCycleId} by user {UserId}", 
                billingCycleId, tokenModel?.UserID ?? 0);

            // Get all subscriptions for this billing cycle
            var subscriptions = await _subscriptionRepository.GetAllSubscriptionsAsync();
            subscriptions = subscriptions.Where(s => s.BillingCycleId == billingCycleId);
            var activeSubscriptions = subscriptions.Where(s => s.Status == Subscription.SubscriptionStatuses.Active).ToList();

            var processedCount = 0;
            var failedCount = 0;

            foreach (var subscription in activeSubscriptions)
            {
                try
                {
                    await ProcessSubscriptionBillingAsync(subscription, tokenModel);
                    processedCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing subscription {SubscriptionId} in billing cycle {BillingCycleId}", 
                        subscription.Id, billingCycleId);
                    failedCount++;
                }
            }

            _logger.LogInformation("Completed billing cycle {BillingCycleId}: {ProcessedCount} processed, {FailedCount} failed", 
                billingCycleId, processedCount, failedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing billing cycle {BillingCycleId} by user {UserId}", 
                billingCycleId, tokenModel?.UserID ?? 0);
            throw;
        }
    }

    /// <summary>
    /// Processes all subscriptions due for billing on a specific date
    /// </summary>
    public async Task ProcessBillingForDateAsync(DateTime billingDate, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Processing billing for date {BillingDate} by user {UserId}", 
                billingDate, tokenModel?.UserID ?? 0);

            // Get all subscriptions due for billing on this date
            var dueSubscriptions = await _subscriptionRepository.GetSubscriptionsDueForBillingAsync(billingDate);
            var activeSubscriptions = dueSubscriptions.Where(s => s.Status == Subscription.SubscriptionStatuses.Active).ToList();

            var processedCount = 0;
            var failedCount = 0;

            foreach (var subscription in activeSubscriptions)
            {
                try
                {
                    await ProcessSubscriptionBillingAsync(subscription, tokenModel);
                    processedCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing subscription {SubscriptionId} for billing date {BillingDate}", 
                        subscription.Id, billingDate);
                    failedCount++;
                }
            }

            _logger.LogInformation("Completed billing for date {BillingDate}: {ProcessedCount} processed, {FailedCount} failed", 
                billingDate, processedCount, failedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing billing for date {BillingDate} by user {UserId}", 
                billingDate, tokenModel?.UserID ?? 0);
            throw;
        }
    }

    /// <summary>
    /// Processes all failed payment retries
    /// </summary>
    public async Task ProcessAllFailedPaymentRetriesAsync(TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Processing all failed payment retries by user {UserId}", tokenModel?.UserID ?? 0);

            // Get all subscriptions with failed payments
            var failedSubscriptions = await _subscriptionRepository.GetSubscriptionsWithFailedPaymentsAsync();
            var retryableSubscriptions = failedSubscriptions.Where(s => s.Status == Subscription.SubscriptionStatuses.PaymentFailed).ToList();

            var processedCount = 0;
            var failedCount = 0;

            foreach (var subscription in retryableSubscriptions)
            {
                try
                {
                    await ProcessFailedPaymentRetryAsync(subscription, tokenModel);
                    processedCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing failed payment retry for subscription {SubscriptionId}", subscription.Id);
                    failedCount++;
                }
            }

            _logger.LogInformation("Completed failed payment retries: {ProcessedCount} processed, {FailedCount} failed", 
                processedCount, failedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing all failed payment retries by user {UserId}", tokenModel?.UserID ?? 0);
            throw;
        }
    }

    /// <summary>
    /// Processes all subscription renewals
    /// </summary>
    public async Task ProcessAllSubscriptionRenewalsAsync(TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Processing all subscription renewals by user {UserId}", tokenModel?.UserID ?? 0);

            // Get all subscriptions that need renewal
            var allSubscriptions = await _subscriptionRepository.GetAllSubscriptionsAsync();
            var renewalSubscriptions = allSubscriptions.Where(s => 
                s.Status == Subscription.SubscriptionStatuses.Active && 
                s.EndDate.HasValue && 
                s.EndDate.Value <= DateTime.UtcNow.AddDays(7)).ToList();

            var processedCount = 0;
            var failedCount = 0;

            foreach (var subscription in renewalSubscriptions)
            {
                try
                {
                    await ProcessSubscriptionRenewalAsync(subscription, tokenModel);
                    processedCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing renewal for subscription {SubscriptionId}", subscription.Id);
                    failedCount++;
                }
            }

            _logger.LogInformation("Completed subscription renewals: {ProcessedCount} processed, {FailedCount} failed", 
                processedCount, failedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing all subscription renewals by user {UserId}", tokenModel?.UserID ?? 0);
            throw;
        }
    }

    /// <summary>
    /// Gets billing statistics for a date range
    /// </summary>
    public async Task<BillingStatistics> GetBillingStatisticsAsync(DateTime startDate, DateTime endDate, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Getting billing statistics from {StartDate} to {EndDate} by user {UserId}", 
                startDate, endDate, tokenModel?.UserID ?? 0);

            var subscriptions = await _subscriptionRepository.GetSubscriptionsByDateRangeAsync(startDate, endDate);
            var activeSubscriptions = subscriptions.Where(s => s.Status == Subscription.SubscriptionStatuses.Active).ToList();

            var statistics = new BillingStatistics
            {
                TotalSubscriptions = subscriptions.Count(),
                ActiveSubscriptions = activeSubscriptions.Count(),
                TotalRevenue = activeSubscriptions.Sum(s => s.CurrentPrice),
                AverageRevenuePerSubscription = activeSubscriptions.Any() ? activeSubscriptions.Average(s => s.CurrentPrice) : 0,
                StartDate = startDate,
                EndDate = endDate,
                GeneratedAt = DateTime.UtcNow
            };

            _logger.LogInformation("Generated billing statistics: {TotalSubscriptions} total, {ActiveSubscriptions} active, {TotalRevenue} revenue", 
                statistics.TotalSubscriptions, statistics.ActiveSubscriptions, statistics.TotalRevenue);

            return statistics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting billing statistics by user {UserId}", tokenModel?.UserID ?? 0);
            throw;
        }
    }

    #endregion
}
