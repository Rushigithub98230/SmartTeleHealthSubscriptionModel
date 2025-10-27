using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Application.Utilities;
using SmartTelehealth.Application.Constants;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Core.Enums;

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
    private readonly ISubscriptionPlanRepository _subscriptionPlanRepository;
    private readonly ISubscriptionBillingService _billingService; // UPDATED: Use consolidated service
    private readonly IStripeService _stripeService;
    private readonly IPrivilegeUsageHistoryRepository _privilegeUsageHistoryRepository;
    private readonly IUserSubscriptionPrivilegeUsageRepository _userSubscriptionPrivilegeUsageRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AutomatedBillingService> _logger;
    private readonly INotificationService _notificationService;
    private readonly IUserRepository _userRepository;
    private readonly IBillingRepository _billingRepository;
    private readonly ISubscriptionPaymentRepository _subscriptionPaymentRepository;

    /// <summary>
    /// Initializes a new instance of the AutomatedBillingService
    /// UPDATED: Now uses consolidated ISubscriptionBillingService
    /// </summary>
    /// <param name="subscriptionRepository">Repository for subscription data access operations</param>
    /// <param name="subscriptionPlanRepository">Repository for subscription plan data access operations</param>
    /// <param name="billingService">Service for billing record management and processing (consolidated)</param>
    /// <param name="stripeService">Service for Stripe payment processing integration</param>
    /// <param name="privilegeUsageHistoryRepository">Repository for privilege usage history tracking</param>
    /// <param name="userSubscriptionPrivilegeUsageRepository">Repository for user subscription privilege usage tracking</param>
    /// <param name="unitOfWork">Unit of work for transaction management</param>
    /// <param name="logger">Logger instance for recording service operations and errors</param>
    /// <param name="notificationService">Service for sending notifications to users</param>
    /// <param name="userRepository">Repository for user data access operations</param>
    /// <param name="billingRepository">Repository for billing record data access operations</param>
    /// <param name="subscriptionPaymentRepository">Repository for subscription payment data access operations</param>
    public AutomatedBillingService(
        ISubscriptionRepository subscriptionRepository,
        ISubscriptionPlanRepository subscriptionPlanRepository,
        ISubscriptionBillingService billingService, // UPDATED: Use consolidated service
        IStripeService stripeService,
        IPrivilegeUsageHistoryRepository privilegeUsageHistoryRepository,
        IUserSubscriptionPrivilegeUsageRepository userSubscriptionPrivilegeUsageRepository,
        IUnitOfWork unitOfWork,
        ILogger<AutomatedBillingService> logger,
        INotificationService notificationService,
        IUserRepository userRepository,
        IBillingRepository billingRepository,
        ISubscriptionPaymentRepository subscriptionPaymentRepository)
    {
        _subscriptionRepository = subscriptionRepository;
        _subscriptionPlanRepository = subscriptionPlanRepository;
        _billingService = billingService;
        _stripeService = stripeService;
        _privilegeUsageHistoryRepository = privilegeUsageHistoryRepository;
        _userSubscriptionPrivilegeUsageRepository = userSubscriptionPrivilegeUsageRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _notificationService = notificationService;
        _userRepository = userRepository;
        _billingRepository = billingRepository;
        _subscriptionPaymentRepository = subscriptionPaymentRepository;
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
                                          s.EndDate.Value <= DateTime.UtcNow.AddDays(SubscriptionConstants.DEFAULT_BILLING_GRACE_PERIOD_DAYS));
            
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
            
            // Get failed payments that are due for retry using SubscriptionPayment
            var paymentsToRetry = await _subscriptionPaymentRepository.GetFailedPaymentsDueForRetryAsync(DateTime.UtcNow, 100);
            
            foreach (var payment in paymentsToRetry)
            {
                try
                {
                    if (payment.AttemptCount >= 3)
                    {
                        await HandleMaxRetriesExceededAsync(payment, tokenModel);
                        continue;
                    }
                    
                    // Process retry payment
                    var result = await _billingService.ProcessPaymentAsync(payment.BillingRecordId, tokenModel);
                    
                    if (result.StatusCode == 200)
                    {
                        _logger.LogInformation("Successfully retried payment {PaymentId} for subscription {SubscriptionId}", 
                            payment.Id, payment.SubscriptionId);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to retry payment {PaymentId} for subscription {SubscriptionId}: {Message}", 
                            payment.Id, payment.SubscriptionId, result.Message);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing failed payment retry for payment {PaymentId} by user {UserId}", 
                        payment.Id, tokenModel?.UserID ?? 0);
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
            
            var subscription = await _subscriptionRepository.GetByIdWithDetailsAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found for plan change by user {UserId}", 
                    subscriptionId, tokenModel?.UserID ?? 0);
                return;
            }

            var oldPlan = subscription.SubscriptionPlan;
            if (oldPlan == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} has no plan loaded. Cannot process plan change.", subscriptionId);
                return;
            }

            var newPlan = await _subscriptionPlanRepository.GetByIdWithDetailsAsync(newPlanId);
            if (newPlan == null)
            {
                _logger.LogWarning("New plan {NewPlanId} not found for subscription {SubscriptionId}", 
                    newPlanId, subscriptionId);
                return;
            }

            // CRITICAL FIX (Issue #4): Implement complete plan change proration logic
            // This ensures users are charged/credited fairly for mid-cycle plan changes
            
            // Begin transaction for plan change to ensure atomicity
            await _unitOfWork.BeginTransactionAsync();
            
            try
            {
                // Calculate proration amounts based on remaining time in billing period
                var now = DateTime.UtcNow;
                var periodStart = subscription.LastBillingDate ?? subscription.StartDate;
                var periodEnd = subscription.NextBillingDate;
                var totalDays = (periodEnd - periodStart).Days;
                var remainingDays = Math.Max(0, (periodEnd - now).Days);

                _logger.LogInformation(
                    "Plan change proration calculation for subscription {SubscriptionId}: " +
                    "PeriodStart={PeriodStart:yyyy-MM-dd}, PeriodEnd={PeriodEnd:yyyy-MM-dd}, " +
                    "TotalDays={TotalDays}, RemainingDays={RemainingDays}",
                    subscriptionId, periodStart, periodEnd, totalDays, remainingDays);

                // Only prorate if there's significant time remaining (more than 1 day)
                // and we have valid period calculation
                if (remainingDays > 1 && totalDays > 0)
                {
                    // Calculate prorated credit from old plan (unused portion)
                    var oldPlanDailyRate = subscription.CurrentPrice / totalDays;
                    var proratedCredit = Math.Round(oldPlanDailyRate * remainingDays, 2);

                    // Calculate prorated charge for new plan (remaining portion)
                    var newPlanDailyRate = newPlan.BasePrice / totalDays;
                    var proratedCharge = Math.Round(newPlanDailyRate * remainingDays, 2);

                    // Calculate net amount to charge or refund
                    var netAmount = proratedCharge - proratedCredit;

                    _logger.LogInformation(
                        "Plan change proration for subscription {SubscriptionId}: " +
                        "OldPlan={OldPlan} (${OldPrice}), NewPlan={NewPlan} (${NewPrice}), " +
                        "RemainingDays={RemainingDays}/{TotalDays}, " +
                        "Credit=${Credit}, Charge=${Charge}, Net=${Net}",
                        subscriptionId, oldPlan.Name, subscription.CurrentPrice, newPlan.Name, newPlan.BasePrice,
                        remainingDays, totalDays, proratedCredit, proratedCharge, netAmount);

                    // Process financial adjustment (if significant - ignore < 10 cents to avoid micro-transactions)
                    if (Math.Abs(netAmount) >= 0.10m)
                    {
                        if (netAmount > 0)
                        {
                            // Upgrade: Charge the difference immediately (per requirement 5a - immediate charging)
                            _logger.LogInformation("Plan upgrade requires immediate charge of ${Amount} for subscription {SubscriptionId}", 
                                netAmount, subscriptionId);
                            
                            var billingResult = await _billingService.CreateSubscriptionBillingAsync(
                                subscription,
                                netAmount,
                                $"Plan upgrade from {oldPlan.Name} to {newPlan.Name} (prorated for {remainingDays} days of {totalDays} total)",
                                DateTime.UtcNow.AddDays(SubscriptionConstants.DEFAULT_BILLING_GRACE_PERIOD_DAYS), // Consistent grace period
                                tokenModel);

                            if (billingResult.StatusCode == 200)
                            {
                                var billingRecordDto = billingResult.data as BillingRecordDto;
                                if (billingRecordDto != null && Guid.TryParse(billingRecordDto.Id, out var billingRecordId))
                                {
                                    // Process payment immediately for upgrade
                                    var paymentResult = await _billingService.ProcessPaymentAsync(billingRecordId, tokenModel);
                                    
                                    if (paymentResult.StatusCode != 200)
                                    {
                                        await _unitOfWork.RollbackTransactionAsync();
                                        _logger.LogError("Failed to process upgrade payment for subscription {SubscriptionId}: {Error}. Plan change cancelled.", 
                                            subscriptionId, paymentResult.Message);
                                        return;
                                    }
                                    
                                    _logger.LogInformation("Successfully charged upgrade difference of ${Amount} for subscription {SubscriptionId}", 
                                        netAmount, subscriptionId);
                                }
                                else
                                {
                                    await _unitOfWork.RollbackTransactionAsync();
                                    _logger.LogError("Failed to extract billing record ID from upgrade billing result for subscription {SubscriptionId}. Plan change cancelled.", 
                                        subscriptionId);
                                    return;
                                }
                            }
                            else
                            {
                                await _unitOfWork.RollbackTransactionAsync();
                                _logger.LogError("Failed to create upgrade billing for subscription {SubscriptionId}: {Error}. Plan change cancelled.", 
                                    subscriptionId, billingResult.Message);
                                return;
                            }
                        }
                        else
                        {
                            // Downgrade: Issue credit for next billing
                            _logger.LogInformation("Plan downgrade issues credit of ${Amount} for next billing on subscription {SubscriptionId}", 
                                Math.Abs(netAmount), subscriptionId);
                            
                            // Store credit in subscription notes for application in next billing
                            // Format: [YYYY-MM-DD] Downgrade credit: $XX.XX - From PlanA to PlanB
                            var creditNote = $"\n[{DateTime.UtcNow:yyyy-MM-dd HH:mm}] Downgrade credit: ${Math.Abs(netAmount):F2} - " +
                                           $"From {oldPlan.Name} to {newPlan.Name} (prorated for {remainingDays} days)";
                            
                            subscription.Notes = (subscription.Notes ?? "") + creditNote;
                            
                            _logger.LogInformation("Credit of ${Amount} stored in subscription notes for application in next billing cycle", 
                                Math.Abs(netAmount));
                        }
                    }
                    else
                    {
                        _logger.LogInformation("Plan change proration amount ${Amount} is negligible (< $0.10), skipping adjustment for subscription {SubscriptionId}", 
                            Math.Abs(netAmount), subscriptionId);
                    }
                }
                else
                {
                    _logger.LogInformation("Less than 2 days remaining in billing period for subscription {SubscriptionId}, skipping proration. " +
                        "Plan change will take effect at next billing.", subscriptionId);
                }

                // Update subscription to new plan
            subscription.SubscriptionPlanId = newPlanId;
                subscription.CurrentPrice = newPlan.BasePrice;
            subscription.UpdatedBy = tokenModel.UserID;
            subscription.UpdatedDate = DateTime.UtcNow;
            
                await _subscriptionRepository.UpdateAsync(subscription);
                
                _logger.LogInformation("Updated subscription {SubscriptionId} to new plan {NewPlanId} with price ${NewPrice}", 
                    subscriptionId, newPlanId, newPlan.BasePrice);

                // Update Stripe subscription if it exists and has valid price ID
                if (!string.IsNullOrEmpty(subscription.StripeSubscriptionId) && !string.IsNullOrEmpty(newPlan.StripePriceId))
                {
                    try
                    {
                        _logger.LogInformation("Updating Stripe subscription {StripeSubId} to new price {PriceId} for subscription {SubscriptionId}", 
                            subscription.StripeSubscriptionId, newPlan.StripePriceId, subscriptionId);
                        
                        var stripeUpdateResult = await _stripeService.UpdateSubscriptionAsync(
                            subscription.StripeSubscriptionId,
                            newPlan.StripePriceId,
                            tokenModel);
                        
                        if (stripeUpdateResult)
                        {
                            subscription.StripePriceId = newPlan.StripePriceId;
                            await _subscriptionRepository.UpdateAsync(subscription);
                            
                            _logger.LogInformation("Successfully updated Stripe subscription {StripeSubId} to new plan price {PriceId}", 
                                subscription.StripeSubscriptionId, newPlan.StripePriceId);
                        }
                        else
                        {
                            _logger.LogWarning("Failed to update Stripe subscription {StripeSubId} for subscription {SubscriptionId}. " +
                                "Local plan change will proceed but Stripe may be out of sync. Manual reconciliation may be required.",
                                subscription.StripeSubscriptionId, subscriptionId);
                        }
                    }
                    catch (Exception stripeEx)
                    {
                        _logger.LogError(stripeEx, "Error updating Stripe subscription {StripeSubId} for subscription {SubscriptionId}. " +
                            "Continuing with local plan change only. Manual Stripe update may be required.",
                            subscription.StripeSubscriptionId, subscriptionId);
                        // Don't fail the entire operation if Stripe update fails
                        // Local plan change is more critical; Stripe can be manually reconciled
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(subscription.StripeSubscriptionId))
                    {
                        _logger.LogInformation("Subscription {SubscriptionId} has no Stripe subscription ID. Skipping Stripe update.", 
                            subscriptionId);
                    }
                    else
                    {
                        _logger.LogWarning("New plan {NewPlanId} has no Stripe price ID configured. Cannot update Stripe subscription {StripeSubId}. " +
                            "Manual Stripe update may be required.",
                            newPlanId, subscription.StripeSubscriptionId);
                    }
                }

                // Commit transaction - all plan change operations completed successfully
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Successfully processed plan change for subscription {SubscriptionId} from {OldPlan} to {NewPlan} by user {UserId}", 
                    subscriptionId, oldPlan.Name, newPlan.Name, tokenModel?.UserID ?? 0);
            }
            catch (Exception ex)
            {
                // Rollback transaction on any error to ensure data consistency
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error in plan change transaction for subscription {SubscriptionId}. All changes rolled back.", 
                    subscriptionId);
                throw;
            }
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
            
            var subscription = await _subscriptionRepository.GetByIdWithDetailsAsync(subscriptionId);
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

    public async Task<bool> ValidateBillingCycleAsync(Guid subscriptionId, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Validating billing cycle for subscription {SubscriptionId} by user {UserId}", 
                subscriptionId, tokenModel?.UserID ?? 0);
            
            var subscription = await _subscriptionRepository.GetByIdWithDetailsAsync(subscriptionId);
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

    // SRP Refactoring: Replaced duplicate logic with centralized BillingService method
    public async Task<DateTime> CalculateNextBillingDateAsync(Guid subscriptionId, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Calculating next billing date for subscription {SubscriptionId} by user {UserId}", 
                subscriptionId, tokenModel?.UserID ?? 0);
            
            var subscription = await _subscriptionRepository.GetByIdWithDetailsAsync(subscriptionId);
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

            // SRP Refactoring: Use centralized billing date calculation from BillingService
            var nextBillingDate = _billingService.CalculateNextBillingDate(DateTime.UtcNow, subscription.BillingCycle);

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
            
            var subscription = await _subscriptionRepository.GetByIdWithDetailsAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found for prorated amount calculation by user {UserId}", 
                    subscriptionId, tokenModel?.UserID ?? 0);
                return 0;
            }

            // REFACTORED: Delegate to centralized BillingCycleCalculator (PHASE 3)
            // This ensures consistent proration logic across ALL services
            var proratedAmount = BillingCycleCalculator.CalculateProratedAmount(
                subscription,
                effectiveDate,
                subscription.CurrentPrice,
                _logger
            );

            _logger.LogInformation(
                "Prorated amount calculated for subscription {SubscriptionId} by user {UserId}: {ProratedAmount}",
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

    #region OLD PRORATION METHODS - REMOVED IN PHASE 3
    // All proration methods moved to BillingCycleCalculator utility
    // REMOVED: CalculateMonthlyProration (was Lines 397-431)
    // REMOVED: CalculateQuarterlyProration (was Lines 437-473)
    // REMOVED: CalculateAnnualProration (was Lines 483-519)
    // REMOVED: CalculateWeeklyProration (was Lines 525-558)
    // REMOVED: CalculateDailyProration (was Lines 563-567)
    // Now using: BillingCycleCalculator.CalculateProratedAmount()
    #endregion
    
    /// <summary>
    /// Migrates existing subscription pricing to align with billing cycle
    /// </summary>
    private async Task MigrateSubscriptionPricingIfNeededAsync(Subscription subscription, TokenModel tokenModel)
    {
        try
        {
            var plan = subscription.SubscriptionPlan;
            var monthlyPrice = plan.BasePrice;
            var billingCycleDays = subscription.BillingCycle.DurationInDays;
            var monthsInCycle = billingCycleDays / 30.0m;
            // NEW ARCHITECTURE: Each plan has explicit price, no calculation needed
            // The plan's Price already reflects the correct amount for its billing cycle
            var correctPrice = plan.BasePrice;
            
            // If CurrentPrice is wrong, update it
            if (Math.Abs(subscription.CurrentPrice - correctPrice) > 0.01m)
            {
                _logger.LogWarning("Migrating subscription {SubscriptionId} price from {OldPrice} to {NewPrice} for billing cycle {BillingCycle}",
                    subscription.Id, subscription.CurrentPrice, correctPrice, subscription.BillingCycle.Name);
                
                subscription.CurrentPrice = correctPrice;
                subscription.UpdatedBy = tokenModel.UserID;
                subscription.UpdatedDate = DateTime.UtcNow;
                await _subscriptionRepository.UpdateAsync(subscription);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error migrating subscription pricing for subscription {SubscriptionId}", subscription.Id);
            // Don't throw - allow billing to continue with current price
        }
    }

    // Helper methods
    private async Task ProcessSubscriptionBillingAsync(Subscription subscription, TokenModel tokenModel)
    {
        try
        {
        _logger.LogInformation("Processing billing for subscription {SubscriptionId} by user {UserId}", 
            subscription.Id, tokenModel?.UserID ?? 0);
        
            // Migrate existing subscription pricing if needed (for existing subscriptions)
            await MigrateSubscriptionPricingIfNeededAsync(subscription, tokenModel);
        
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

            // Step 3: Create billing record using centralized factory method (SRP Refactoring)
            var billingResult = await _billingService.CreateSubscriptionBillingAsync(
                subscription,
                billingAmount,
                $"Automated billing for {subscription.SubscriptionPlan?.Name ?? "subscription"} - {subscription.BillingCycle?.Name ?? "monthly"}",
                DateTime.UtcNow.AddDays(SubscriptionConstants.DEFAULT_BILLING_GRACE_PERIOD_DAYS), // Consistent grace period
                tokenModel
            );
            
            if (billingResult.StatusCode != 200)
            {
                _logger.LogError("Failed to create billing record for subscription {SubscriptionId}: {Error}", 
                    subscription.Id, billingResult.Message);
                return;
            }

            // Extract billing record ID from result
            var billingRecordDto = billingResult.data as BillingRecordDto;
            if (billingRecordDto == null || !Guid.TryParse(billingRecordDto.Id, out var billingRecordId))
            {
                _logger.LogError("Failed to extract billing record ID from result for subscription {SubscriptionId}", subscription.Id);
                return;
            }

            // Step 4: FIXED - Process payment through PaymentService to enable SubscriptionPayment tracking
            // PaymentService.ProcessPaymentAsync handles:
            // - Creates SubscriptionPayment with billing period
            // - Processes payment through Stripe
            // - Updates BillingRecord, SubscriptionPayment, and Subscription in transaction
            var paymentResult = await _billingService.ProcessPaymentAsync(billingRecordId, tokenModel);
            
            if (paymentResult.StatusCode == 200)
            {
                _logger.LogInformation("Successfully processed billing and payment for subscription {SubscriptionId} with amount {Amount}", 
                    subscription.Id, billingAmount);
                
                // CRITICAL FIX (Issue #2): Process overage charges immediately after successful billing
                // This ensures users are charged for exceeding their privilege limits in real-time
                try
                {
                    _logger.LogInformation("Checking for overage charges for subscription {SubscriptionId}", subscription.Id);
                    
                    var overageProcessed = await ProcessOverageChargesAsync(subscription, tokenModel);
                    
                    if (overageProcessed)
                    {
                        _logger.LogInformation("Successfully processed overage charges for subscription {SubscriptionId}", subscription.Id);
                    }
                    else
                    {
                        _logger.LogWarning("No overage charges or overage processing failed for subscription {SubscriptionId}. " +
                            "If there were overage charges, they will be carried over to next billing.", subscription.Id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing overage charges for subscription {SubscriptionId}. " +
                        "Overage charges will be carried over to next billing period. Main billing completed successfully.", 
                        subscription.Id);
                    // Don't fail the main billing operation if overage processing fails
                    // Overage can be retried or processed in next billing cycle
                }
            }
            else
            {
                _logger.LogWarning("Billing created but payment failed for subscription {SubscriptionId}: {Error}. Will retry automatically.", 
                    subscription.Id, paymentResult.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing billing for subscription {SubscriptionId}", subscription.Id);
            throw;
        }
    }

    /// <summary>
    /// CRITICAL FIX: Now delegates to centralized renewal method in SubscriptionBillingService.
    /// This ensures complete renewal with ALL operations (billing, payment, dates, privileges).
    /// 
    /// Previous implementation: Created billing + processed payment only (incomplete)
    /// New implementation: Calls SubscriptionBillingService.ProcessSubscriptionRenewalAsync()
    ///                     which performs COMPLETE renewal with Saga pattern.
    /// </summary>
    private async Task ProcessSubscriptionRenewalAsync(Subscription subscription, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Delegating renewal for subscription {SubscriptionId} to centralized billing service", 
                subscription.Id);
        
            // CRITICAL FIX: Use centralized renewal method that does EVERYTHING
            // - Updates billing dates
            // - Creates billing record
            // - Processes payment
            // - Resets privilege usage
            // - Uses Saga pattern for safety
            var renewalResult = await _billingService.ProcessSubscriptionRenewalAsync(
                subscription.Id, 
                tokenModel);
            
            if (renewalResult.StatusCode == 200)
            {
                _logger.LogInformation("✅ Successfully renewed subscription {SubscriptionId}", subscription.Id);
            }
            else if (renewalResult.StatusCode == 402)
            {
                _logger.LogWarning("⚠️ Renewal payment failed for subscription {SubscriptionId}: {Error}. Will retry automatically.", 
                    subscription.Id, renewalResult.Message);
            }
            else
            {
                _logger.LogError("❌ Renewal failed for subscription {SubscriptionId}: {Error}", 
                    subscription.Id, renewalResult.Message);
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
            if (subscription.EndDate.HasValue && subscription.EndDate.Value > DateTime.UtcNow.AddDays(SubscriptionConstants.DEFAULT_BILLING_GRACE_PERIOD_DAYS))
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
    /// Calculates the billing amount for a subscription with proper validation.
    /// CRITICAL FIX: Uses centralized billing calculation to prevent double discounting and ensure consistency.
    /// </summary>
    private async Task<decimal> CalculateBillingAmountAsync(Subscription subscription, TokenModel tokenModel)
    {
        try
        {
            var plan = subscription.SubscriptionPlan;
            
            // CRITICAL FIX: Use centralized effective price calculation
            var basePrice = BillingCalculationService.GetEffectivePlanPrice(plan, _logger);
            
            _logger.LogDebug(
                "Using effective plan price for {SubscriptionId}: " +
                "BasePrice={BasePrice}, EffectivePrice={EffectivePrice}, Cycle={Cycle}",
                subscription.Id, plan.BasePrice, basePrice, subscription.BillingCycle?.Name);
            
            // Apply additional discounts or adjustments (subscription-specific)
            var additionalDiscounts = await CalculateDiscountAmountAsync(subscription, tokenModel);
            var adjustmentAmount = await CalculateAdjustmentAmountAsync(subscription, tokenModel);
            
            // CRITICAL FIX: Use centralized billing calculation with proper validation
            var finalPrice = BillingCalculationService.CalculateFinalBillingAmount(
                subscription, basePrice, additionalDiscounts, adjustmentAmount, _logger);
            
            // Validate the calculation is logically correct
            var isValid = BillingCalculationService.ValidateBillingCalculation(
                subscription, basePrice, additionalDiscounts, adjustmentAmount, finalPrice, _logger);
            
            if (!isValid)
            {
                _logger.LogError("Billing calculation validation failed for subscription {SubscriptionId}, using fallback price", subscription.Id);
                return Math.Max(subscription.CurrentPrice, 0.01m);
            }
            
            return finalPrice;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating billing amount for subscription {SubscriptionId}", subscription.Id);
            return Math.Max(subscription.CurrentPrice, 0.01m);
        }
    }
    
    /// <summary>
    /// Calculates billing cycle discount based on plan configuration.
    /// DEPRECATED: Use BillingCycleCalculator.CalculateBillingCycleDiscount() instead.
    /// Kept for backward compatibility during transition.
    /// </summary>
    [Obsolete("Use BillingCycleCalculator.CalculateBillingCycleDiscount() instead")]
    private decimal CalculateBillingCycleDiscount(SubscriptionPlan plan, MasterBillingCycle billingCycle, decimal basePrice)
    {
        // Delegate to centralized calculator
        return BillingCycleCalculator.CalculateBillingCycleDiscount(plan, billingCycle, basePrice);
    }

    /// <summary>
    /// Calculates the renewal amount for a subscription.
    /// CRITICAL FIX: Uses centralized billing calculation to ensure consistency with regular billing.
    /// </summary>
    private async Task<decimal> CalculateRenewalAmountAsync(Subscription subscription, TokenModel tokenModel)
    {
        try
        {
            var plan = subscription.SubscriptionPlan;
            if (plan == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} has no plan, using CurrentPrice", subscription.Id);
                return Math.Max(subscription.CurrentPrice, 0.01m);
            }
            
            // CRITICAL FIX: Use the same billing calculation logic as regular billing
            // This ensures consistency between regular billing and renewal billing
            var renewalAmount = await CalculateBillingAmountAsync(subscription, tokenModel);
            
            _logger.LogInformation("Renewal amount calculated for subscription {SubscriptionId}: " +
                "PlanPrice=${PlanPrice}, Cycle={Cycle}, RenewalAmount=${Amount}",
                subscription.Id, plan.BasePrice, subscription.BillingCycle?.Name, renewalAmount);
            
            return renewalAmount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating renewal amount for subscription {SubscriptionId}", subscription.Id);
            return Math.Max(subscription.CurrentPrice, 0.01m);
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
    /// Processes payment through Stripe with comprehensive retry logic
    /// </summary>
    private async Task<PaymentResultDto> ProcessPaymentThroughStripeAsync(Subscription subscription, decimal amount, TokenModel tokenModel)
    {
        const int maxRetries = SubscriptionConstants.MAX_PAYMENT_RETRY_ATTEMPTS;
        const int baseDelayMs = SubscriptionConstants.PAYMENT_RETRY_BASE_DELAY_MS;
        
        for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
                _logger.LogInformation("Processing payment attempt {Attempt}/{MaxRetries} for subscription {SubscriptionId} amount {Amount}", 
                    attempt, maxRetries, subscription.Id, amount);

                var result = await _stripeService.ProcessPaymentAsync(
                subscription.PaymentMethodId,
                amount,
                subscription.Currency ?? "usd",
                tokenModel
            );

                if (result.Status == "succeeded")
                {
                    _logger.LogInformation("Payment succeeded on attempt {Attempt} for subscription {SubscriptionId}", 
                        attempt, subscription.Id);
                    return result;
                }

                // Check if this is a retryable error
                if (IsRetryablePaymentError(result.Status) && attempt < maxRetries)
                {
                    var delay = baseDelayMs * (int)Math.Pow(2, attempt - 1); // Exponential backoff
                    _logger.LogWarning("Payment failed with retryable error on attempt {Attempt} for subscription {SubscriptionId}. Retrying in {Delay}ms. Status: {Status}", 
                        attempt, subscription.Id, delay, result.Status);
                    
                    await Task.Delay(delay);
                    continue;
                }

                // Non-retryable error or max retries reached
                _logger.LogError("Payment failed permanently for subscription {SubscriptionId} after {Attempt} attempts. Status: {Status}, Error: {Error}", 
                    subscription.Id, attempt, result.Status, result.ErrorMessage);
                
                return result;
        }
        catch (Exception ex)
        {
                if (attempt < maxRetries && IsRetryableException(ex))
                {
                    var delay = baseDelayMs * (int)Math.Pow(2, attempt - 1); // Exponential backoff
                    _logger.LogWarning(ex, "Payment processing exception on attempt {Attempt} for subscription {SubscriptionId}. Retrying in {Delay}ms", 
                        attempt, subscription.Id, delay);
                    
                    await Task.Delay(delay);
                    continue;
                }

                _logger.LogError(ex, "Payment processing failed permanently for subscription {SubscriptionId} after {Attempt} attempts", 
                    subscription.Id, attempt);
                
            return new PaymentResultDto
            {
                Status = "failed",
                ErrorMessage = ex.Message
            };
        }
        }

        // This should never be reached, but just in case
        return new PaymentResultDto
        {
            Status = "failed",
            ErrorMessage = "Payment processing failed after all retry attempts"
        };
    }

    /// <summary>
    /// Determines if a payment error is retryable
    /// </summary>
    private static bool IsRetryablePaymentError(string status)
    {
        var retryableStatuses = new[]
        {
            "requires_payment_method",
            "requires_confirmation",
            "requires_action",
            "processing",
            "canceled" // Sometimes canceled payments can be retried
        };

        return retryableStatuses.Contains(status?.ToLower());
    }

    /// <summary>
    /// Determines if an exception is retryable
    /// </summary>
    private static bool IsRetryableException(Exception ex)
    {
        // Network-related exceptions are typically retryable
        if (ex is HttpRequestException || ex is TaskCanceledException || ex is TimeoutException)
            return true;

        // Stripe rate limiting is retryable
        if (ex.Message.Contains("rate_limit") || ex.Message.Contains("too_many_requests"))
            return true;

        // Temporary Stripe service issues are retryable
        if (ex.Message.Contains("service_unavailable") || ex.Message.Contains("internal_error"))
            return true;

        return false;
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
                subscription.UpdatedBy = tokenModel.UserID;
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
                subscription.UpdatedBy = tokenModel.UserID;
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
            subscription.UpdatedBy = tokenModel.UserID;
            subscription.UpdatedDate = DateTime.UtcNow;
            
            // REFACTORED: Extend subscription end date using centralized calculator
            if (subscription.EndDate.HasValue && subscription.BillingCycle != null)
            {
                var oldEndDate = subscription.EndDate.Value;
                
                // Use centralized calculator for consistency
                subscription.EndDate = BillingCycleCalculator.ExtendByBillingCycle(
                    subscription.EndDate.Value, 
                    subscription.BillingCycle);
                
                _logger.LogInformation("Extended subscription {SubscriptionId} EndDate by {Cycle}: {OldDate:yyyy-MM-dd} → {NewDate:yyyy-MM-dd}",
                    subscription.Id, subscription.BillingCycle.Name, oldEndDate, subscription.EndDate.Value);
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
            subscription.UpdatedBy = tokenModel.UserID;
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
            subscription.UpdatedBy = tokenModel.UserID;
            subscription.UpdatedDate = DateTime.UtcNow;
            
            // Update the failed billing record
            failedRecord.Status = BillingRecord.BillingStatus.Paid;
            failedRecord.PaidAt = DateTime.UtcNow;
            failedRecord.ProcessedAt = DateTime.UtcNow;
            failedRecord.UpdatedBy = tokenModel.UserID;
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
            subscription.UpdatedBy = tokenModel.UserID;
            subscription.UpdatedDate = DateTime.UtcNow;
            
            // If max retries reached, suspend subscription
            if (subscription.FailedPaymentAttempts >= SubscriptionConstants.MAX_FAILED_PAYMENT_ATTEMPTS)
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
    /// <summary>
    /// Calculates the next billing date for a subscription based on billing cycle.
    /// REFACTORED: Now uses centralized BillingCycleCalculator for consistency.
    /// </summary>
    private DateTime CalculateNextBillingDate(Subscription subscription)
    {
        // CONSISTENT FIX: Use centralized billing service instead of duplicate implementation
        return _billingService.CalculateNextBillingDate(DateTime.UtcNow, subscription.BillingCycle);
    }

    /// <summary>
    /// Calculates discount amount for a subscription with proper validation and capping.
    /// FIXED: Prevents excessive discount stacking that could cause revenue loss.
    /// </summary>
    private async Task<decimal> CalculateDiscountAmountAsync(Subscription subscription, TokenModel tokenModel)
    {
        try
        {
            decimal totalDiscount = 0;
            var basePrice = subscription.CurrentPrice;
            
            // Check for subscription plan discounts
            if (subscription.SubscriptionPlan != null)
            {
                // Early bird discount for new subscriptions (first 30 days)
                if (subscription.CreatedDate > DateTime.UtcNow.AddDays(-30))
                {
                    var earlyBirdDiscount = basePrice * 0.1m; // 10% early bird discount
                    totalDiscount += earlyBirdDiscount;
                    _logger.LogInformation("Applied early bird discount of {Discount} for subscription {SubscriptionId}", 
                        earlyBirdDiscount, subscription.Id);
                }
                
                // Volume discount for annual plans
                if (subscription.SubscriptionPlan.BillingCycle?.Name == "annual")
                {
                    var volumeDiscount = basePrice * 0.15m; // 15% annual discount
                    totalDiscount += volumeDiscount;
                    _logger.LogInformation("Applied annual volume discount of {Discount} for subscription {SubscriptionId}", 
                        volumeDiscount, subscription.Id);
                }
                
                // Loyalty discount for long-term subscribers (6+ months)
                if (subscription.CreatedDate < DateTime.UtcNow.AddMonths(-6))
                {
                    var loyaltyDiscount = basePrice * 0.05m; // 5% loyalty discount
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
                            var promoDiscount = ApplyPromotionalDiscount(promoCode, basePrice);
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
            
            // CRITICAL FIX: Validate and cap total discounts to prevent revenue loss
            var validatedDiscount = BillingValidationService.ValidateAndCapDiscounts(basePrice, totalDiscount, 50m);
            
            if (validatedDiscount != totalDiscount)
            {
                _logger.LogWarning("Total discount {TotalDiscount} exceeded maximum allowed for subscription {SubscriptionId}, capped to {CappedDiscount}",
                    totalDiscount, subscription.Id, validatedDiscount);
            }
            
            return validatedDiscount;
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
    /// Gets the effective price for a subscription plan, considering discounts and validity periods.
    /// Returns the discounted price if valid, otherwise returns the base price.
    /// </summary>
    /// <param name="plan">The subscription plan to get the effective price for</param>
    /// <returns>The effective price to use for billing</returns>
    private decimal GetEffectivePlanPrice(SubscriptionPlan plan)
    {
        try
        {
            // CRITICAL FIX: Use centralized effective price calculation
            return BillingCalculationService.GetEffectivePlanPrice(plan, _logger);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating effective price for plan {PlanName}, using base price", plan.Name);
            return plan.BasePrice;
        }
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
                // Overage charges for plans with usage-based privileges
                var overageCharge = await CalculateOverageChargeAsync(subscription);
                totalAdjustment += overageCharge;
                if (overageCharge > 0)
                {
                    _logger.LogInformation("Applied overage charge of {Charge} for subscription {SubscriptionId}", 
                        overageCharge, subscription.Id);
                }
                
                // Late payment fees
                if (subscription.Status == Subscription.SubscriptionStatuses.PaymentFailed)
                {
                    var lateFee = subscription.CurrentPrice * 0.05m; // 5% late fee
                    totalAdjustment += lateFee;
                    _logger.LogInformation("Applied late payment fee of {Fee} for subscription {SubscriptionId}", 
                        lateFee, subscription.Id);
                }
                
                // Service charges for plans with premium features
                if (subscription.SubscriptionPlan.IsFeatured || subscription.SubscriptionPlan.IsMostPopular)
                {
                    var serviceCharge = subscription.CurrentPrice * 0.02m; // 2% service charge
                    totalAdjustment += serviceCharge;
                    _logger.LogInformation("Applied service charge of {Charge} for premium subscription {SubscriptionId}", 
                        serviceCharge, subscription.Id);
                }
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
    
    public async Task<decimal> CalculateOverageChargeAsync(Subscription subscription)
    {
        try
        {
            // Get plan privileges to determine limits
            var planPrivileges = subscription.SubscriptionPlan?.PlanPrivileges;
            if (planPrivileges == null || !planPrivileges.Any())
            {
                return 0;
            }

            decimal totalOverageCharge = 0;

            foreach (var privilege in planPrivileges)
            {
                if (!privilege.HasOverageCharges)
                {
                    continue; // No overage charges for unlimited privileges or privileges without unit costs
                }

                // Get actual usage for this privilege
                var actualUsage = await GetActualUsageForPrivilegeAsync(subscription.Id, privilege.PrivilegeId);
                var totalLimit = privilege.Value; // Total privilege limit

                // Skip overage calculation for unlimited privileges (Value = -1)
                if (totalLimit == SubscriptionConstants.UNLIMITED_PRIVILEGE_VALUE)
                {
                    _logger.LogDebug("Skipping overage calculation for unlimited privilege {PrivilegeId}", privilege.PrivilegeId);
                    continue;
                }

                if (actualUsage > totalLimit)
                {
                    var overage = actualUsage - totalLimit;
                    var unitCost = privilege.UnitCost;
                    var overageCharge = overage * unitCost;
                    totalOverageCharge += overageCharge;

                    _logger.LogInformation("Overage charge for privilege {PrivilegeId}: {Overage} units × ${UnitCost} = ${Charge}", 
                        privilege.PrivilegeId, overage, unitCost, overageCharge);
                }
            }

            return totalOverageCharge;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating overage charge for subscription {SubscriptionId}", subscription.Id);
            return 0;
        }
    }

    /// <summary>
    /// Creates a billing record for overage charges
    /// </summary>
    /// <param name="subscription">The subscription with overage charges</param>
    /// <param name="overageAmount">The total overage amount</param>
    /// <param name="tokenModel">Token containing user authentication information</param>
    /// <returns>Created billing record ID</returns>
    public async Task<Guid?> CreateOverageBillingRecordAsync(Subscription subscription, decimal overageAmount, TokenModel tokenModel)
    {
        try
        {
            if (overageAmount <= 0)
            {
                _logger.LogInformation("No overage charges to bill for subscription {SubscriptionId}", subscription.Id);
                return null;
            }

            // Get subscription plan to get CurrencyId
            var subscriptionPlan = await _subscriptionPlanRepository.GetByIdWithDetailsAsync(subscription.SubscriptionPlanId);
            if (subscriptionPlan == null)
            {
                _logger.LogError("Subscription plan {SubscriptionPlanId} not found for subscription {SubscriptionId}", 
                    subscription.SubscriptionPlanId, subscription.Id);
                return null;
            }

            // Create billing record for overage charges
            var billingRecord = new BillingRecord
            {
                Id = Guid.NewGuid(),
                UserId = subscription.UserId,
                SubscriptionId = subscription.Id,
                CurrencyId = subscriptionPlan.CurrencyId,
                Status = BillingRecord.BillingStatus.Pending,
                Type = BillingRecord.BillingType.Overage,  // FIXED: Use Overage type, not Subscription
                Amount = overageAmount,
                TaxAmount = 0, // Calculate tax if needed
                ShippingAmount = 0,
                TotalAmount = overageAmount,
                BillingDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(SubscriptionConstants.DEFAULT_BILLING_GRACE_PERIOD_DAYS), // Consistent grace period
                Description = $"Overage charges for subscription {subscription.Id}",
                IsRecurring = false,
                NextBillingDate = null
            };

            // Save billing record
            await _billingRepository.CreateAsync(billingRecord);

            _logger.LogInformation("Created overage billing record {BillingRecordId} for subscription {SubscriptionId} with amount {Amount}", 
                billingRecord.Id, subscription.Id, overageAmount);

            // Send notification to user about overage charges
            try
            {
                var user = await _userRepository.GetByIdAsync(subscription.UserId);
                if (user != null)
                {
                    var billingRecordDto = new BillingRecordDto
                    {
                        Id = billingRecord.Id.ToString(),
                        Amount = overageAmount,
                        DueDate = billingRecord.DueDate,
                        Description = billingRecord.Description
                    };
                    
                    await _notificationService.SendOverageChargeEmailAsync(user.Email, user.FullName, billingRecordDto, overageAmount, tokenModel);
                    _logger.LogInformation("Overage charge notification sent to user {UserId} for subscription {SubscriptionId}", user.Id, subscription.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send overage charge notification for subscription {SubscriptionId}", subscription.Id);
                // Don't fail the main operation if notification fails
            }

            return billingRecord.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating overage billing record for subscription {SubscriptionId}", subscription.Id);
            return null;
        }
    }

    /// <summary>
    /// Processes overage charges for a subscription
    /// </summary>
    /// <param name="subscription">The subscription to process overage charges for</param>
    /// <param name="tokenModel">Token containing user authentication information</param>
    /// <returns>True if overage charges were processed successfully</returns>
    public async Task<bool> ProcessOverageChargesAsync(Subscription subscription, TokenModel tokenModel)
    {
        try
        {
            // Calculate overage charges
            var overageAmount = await CalculateOverageChargeAsync(subscription);
            
            if (overageAmount <= 0)
            {
                _logger.LogInformation("No overage charges for subscription {SubscriptionId}", subscription.Id);
                return true;
            }

            // Create billing record for overage
            var billingRecordId = await CreateOverageBillingRecordAsync(subscription, overageAmount, tokenModel);
            
            if (billingRecordId == null)
            {
                _logger.LogError("Failed to create overage billing record for subscription {SubscriptionId}", subscription.Id);
                return false;
            }

            // FIXED: Process payment through PaymentService to enable SubscriptionPayment tracking and retry logic
            var paymentResult = await _billingService.ProcessPaymentAsync(billingRecordId.Value, tokenModel);
            
            if (paymentResult.StatusCode == 200)
            {
                _logger.LogInformation("Successfully processed overage charges of {Amount} for subscription {SubscriptionId}", 
                    overageAmount, subscription.Id);
                return true;
            }
            else
            {
                _logger.LogWarning("Failed to process overage charges for subscription {SubscriptionId}: {Error}", 
                    subscription.Id, paymentResult.Message);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing overage charges for subscription {SubscriptionId}", subscription.Id);
            return false;
        }
    }

    /// <summary>
    /// Gets the actual usage count for a specific privilege in the current billing period
    /// </summary>
    /// <param name="subscriptionId">The subscription ID</param>
    /// <param name="privilegeId">The privilege ID</param>
    /// <returns>Total usage count for the privilege in the current billing period</returns>
    public async Task<int> GetActualUsageForPrivilegeAsync(Guid subscriptionId, Guid privilegeId)
    {
        try
        {
            // Get subscription details
            var subscription = await _subscriptionRepository.GetByIdWithDetailsAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found for usage calculation", subscriptionId);
                return 0;
            }

            // Get current usage records for this subscription
            var usageRecords = await _userSubscriptionPrivilegeUsageRepository.GetBySubscriptionIdAsync(subscriptionId);
            
            // Find the usage record for the specific privilege
            var privilegeUsage = usageRecords.FirstOrDefault(u => u.PrivilegeId == privilegeId);
            
            if (privilegeUsage == null)
            {
                _logger.LogInformation("No usage found for subscription {SubscriptionId}, privilege {PrivilegeId}", 
                    subscriptionId, privilegeId);
                return 0;
            }

            // Return the current used value
            var totalUsage = privilegeUsage.UsedValue;
            
            _logger.LogInformation("Actual usage for subscription {SubscriptionId}, privilege {PrivilegeId}: {Usage} units", 
                subscriptionId, privilegeId, totalUsage);

            return totalUsage;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting actual usage for subscription {SubscriptionId}, privilege {PrivilegeId}", 
                subscriptionId, privilegeId);
            return 0;
        }
    }

    private DateTime GetBillingPeriodStart(Subscription subscription)
    {
        // Calculate the start of the current billing period
        var currentDate = DateTime.UtcNow;
        var subscriptionStart = subscription.StartDate;
        
        // If subscription started this month, use subscription start date
        if (subscriptionStart.Month == currentDate.Month && subscriptionStart.Year == currentDate.Year)
        {
            return subscriptionStart;
        }
        
        // Otherwise, use the first day of current month
        return new DateTime(currentDate.Year, currentDate.Month, 1);
    }

    private DateTime GetBillingPeriodEnd(Subscription subscription)
    {
        // Calculate the end of the current billing period
        var currentDate = DateTime.UtcNow;
        
        // Use the last day of current month
        return new DateTime(currentDate.Year, currentDate.Month, 
            DateTime.DaysInMonth(currentDate.Year, currentDate.Month));
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
                s.EndDate.Value <= DateTime.UtcNow.AddDays(SubscriptionConstants.DEFAULT_BILLING_GRACE_PERIOD_DAYS)).ToList();

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

    #region Helper Methods

    /// <summary>
    /// Handles subscription suspension when maximum retry attempts are exceeded
    /// </summary>
    private async Task HandleMaxRetriesExceededAsync(SubscriptionPayment payment, TokenModel tokenModel)
    {
        try
        {
            _logger.LogWarning("Maximum retry attempts exceeded for payment {PaymentId}. Suspending subscription {SubscriptionId}", 
                payment.Id, payment.SubscriptionId);

            await _unitOfWork.BeginTransactionAsync();
            
            try
            {
                // Get subscription
                var subscription = await _subscriptionRepository.GetByIdAsync(payment.SubscriptionId);
                if (subscription == null)
                {
                    _logger.LogError("Subscription {SubscriptionId} not found for payment {PaymentId}", 
                        payment.SubscriptionId, payment.Id);
                    return;
                }

                // Suspend subscription
                subscription.Status = Subscription.SubscriptionStatuses.Suspended;
                subscription.Notes = "Maximum payment retry attempts exceeded";
                // SuspensionReason and SuspendedAt properties don't exist - using Notes instead
                subscription.UpdatedBy = tokenModel.UserID;
                subscription.UpdatedDate = DateTime.UtcNow;

                await _subscriptionRepository.UpdateAsync(subscription);

                // Update payment status to indicate max retries exceeded
                payment.Status = SubscriptionPayment.PaymentStatus.Failed;
                payment.FailureReason = "Maximum retry attempts exceeded (3)";
                payment.UpdatedBy = tokenModel.UserID;
                payment.UpdatedDate = DateTime.UtcNow;

                await _subscriptionPaymentRepository.UpdateAsync(payment);

                // Send notification to user
                var user = await _userRepository.GetByIdAsync(subscription.UserId);
                if (user != null)
                {
                    await _notificationService.SendNotificationAsync(
                        user.Id,
                        "Subscription Suspended",
                        "Your subscription has been suspended due to failed payment attempts. Please update your payment method to reactivate your subscription.",
                        tokenModel);
                }

                await _unitOfWork.CommitTransactionAsync();
                
                _logger.LogInformation("Successfully suspended subscription {SubscriptionId} after max retry attempts exceeded", 
                    subscription.Id);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error suspending subscription {SubscriptionId} after max retry attempts", 
                    payment.SubscriptionId);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling max retries exceeded for payment {PaymentId}", payment.Id);
            throw;
        }
    }

    #endregion
}
