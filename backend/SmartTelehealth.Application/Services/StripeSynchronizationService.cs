using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;

namespace SmartTelehealth.Application.Services;

/// <summary>
/// Service for handling synchronization between local database and Stripe
/// </summary>
public class StripeSynchronizationService : IStripeSynchronizationService
{
    private readonly IStripeService _stripeService;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<StripeSynchronizationService> _logger;

    public StripeSynchronizationService(
        IStripeService stripeService,
        ISubscriptionRepository subscriptionRepository,
        IUserRepository userRepository,
        ILogger<StripeSynchronizationService> logger)
    {
        _stripeService = stripeService ?? throw new ArgumentNullException(nameof(stripeService));
        _subscriptionRepository = subscriptionRepository ?? throw new ArgumentNullException(nameof(subscriptionRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> SynchronizeSubscriptionPlanAsync(Guid planId, TokenModel tokenModel)
    {
        try
        {
            var plan = await _subscriptionRepository.GetSubscriptionPlanByIdAsync(planId);
            if (plan == null)
            {
                _logger.LogWarning("Subscription plan {PlanId} not found for synchronization", planId);
                return false;
            }

            _logger.LogInformation("Starting Stripe synchronization for subscription plan: {PlanName}", plan.Name);

            // Check if plan already has Stripe integration
            if (!string.IsNullOrEmpty(plan.StripeProductId))
            {
                _logger.LogInformation("Plan {PlanName} already has Stripe integration. Updating existing resources.", plan.Name);
                return await UpdateExistingPlanInStripeAsync(plan, tokenModel);
            }
            else
            {
                _logger.LogInformation("Plan {PlanName} has no Stripe integration. Creating new Stripe resources.", plan.Name);
                return await CreateNewPlanInStripeAsync(plan, tokenModel);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error synchronizing subscription plan {PlanId} with Stripe", planId);
            return false;
        }
    }

    public async Task<bool> SynchronizeSubscriptionPlanDeletionAsync(Guid planId, TokenModel tokenModel)
    {
        try
        {
            var plan = await _subscriptionRepository.GetSubscriptionPlanByIdAsync(planId);
            if (plan == null)
            {
                _logger.LogWarning("Subscription plan {PlanId} not found for deletion synchronization", planId);
                return false;
            }

            if (string.IsNullOrEmpty(plan.StripeProductId))
            {
                _logger.LogInformation("Plan {PlanName} has no Stripe integration. No cleanup needed.", plan.Name);
                return true;
            }

            _logger.LogInformation("Cleaning up Stripe resources for plan: {PlanName}", plan.Name);

            // Deactivate all prices
            if (!string.IsNullOrEmpty(plan.StripeMonthlyPriceId))
            {
                await _stripeService.DeactivatePriceAsync(plan.StripeMonthlyPriceId, tokenModel);
            }
            if (!string.IsNullOrEmpty(plan.StripeQuarterlyPriceId))
            {
                await _stripeService.DeactivatePriceAsync(plan.StripeQuarterlyPriceId, tokenModel);
            }
            if (!string.IsNullOrEmpty(plan.StripeAnnualPriceId))
            {
                await _stripeService.DeactivatePriceAsync(plan.StripeAnnualPriceId, tokenModel);
            }

            // Delete the product
            await _stripeService.DeleteProductAsync(plan.StripeProductId, tokenModel);

            _logger.LogInformation("Successfully cleaned up Stripe resources for plan {PlanName}", plan.Name);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error synchronizing subscription plan deletion {PlanId} with Stripe", planId);
            return false;
        }
    }

    public async Task<bool> SynchronizeSubscriptionStatusAsync(Guid subscriptionId, string newStatus, TokenModel tokenModel)
    {
        try
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found for status synchronization", subscriptionId);
                return false;
            }

            if (string.IsNullOrEmpty(subscription.StripeSubscriptionId))
            {
                _logger.LogInformation("Subscription {SubscriptionId} has no Stripe integration. No sync needed.", subscriptionId);
                return true;
            }

            _logger.LogInformation("Synchronizing subscription {SubscriptionId} status to {Status} with Stripe", subscriptionId, newStatus);

            switch (newStatus.ToLower())
            {
                case "active":
                    await _stripeService.ResumeSubscriptionAsync(subscription.StripeSubscriptionId, tokenModel);
                    break;
                case "paused":
                    await _stripeService.PauseSubscriptionAsync(subscription.StripeSubscriptionId, tokenModel);
                    break;
                case "cancelled":
                    await _stripeService.CancelSubscriptionAsync(subscription.StripeSubscriptionId, tokenModel);
                    break;
                default:
                    _logger.LogWarning("Unknown subscription status {Status} for synchronization", newStatus);
                    return false;
            }

            _logger.LogInformation("Successfully synchronized subscription {SubscriptionId} status to {Status}", subscriptionId, newStatus);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error synchronizing subscription status {SubscriptionId} with Stripe", subscriptionId);
            return false;
        }
    }

    public async Task<bool> SynchronizeCustomerAsync(int userId, TokenModel tokenModel)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found for customer synchronization", userId);
                return false;
            }

            // Check if user already has a Stripe customer ID
            if (!string.IsNullOrEmpty(user.StripeCustomerId))
            {
                _logger.LogInformation("User {UserId} already has Stripe customer ID: {CustomerId}", userId, user.StripeCustomerId);
                return true;
            }

            _logger.LogInformation("Creating Stripe customer for user: {UserId}", userId);

            var stripeCustomerId = await _stripeService.CreateCustomerAsync(user.Email, user.FullName, tokenModel);
            
            // Update user with Stripe customer ID
            user.StripeCustomerId = stripeCustomerId;
            await _userRepository.UpdateAsync(user);

            _logger.LogInformation("Successfully created Stripe customer {CustomerId} for user {UserId}", stripeCustomerId, userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error synchronizing customer for user {UserId} with Stripe", userId);
            return false;
        }
    }

    public async Task<StripeSyncValidationResult> ValidatePlanSynchronizationAsync(Guid planId, TokenModel tokenModel)
    {
        var result = new StripeSyncValidationResult();
        
        try
        {
            var plan = await _subscriptionRepository.GetSubscriptionPlanByIdAsync(planId);
            if (plan == null)
            {
                result.Issues.Add("Subscription plan not found");
                return result;
            }

            // Check if plan has Stripe integration
            if (string.IsNullOrEmpty(plan.StripeProductId))
            {
                result.Issues.Add("Plan has no Stripe product ID");
                result.Recommendations.Add("Run plan synchronization to create Stripe resources");
                return result;
            }

            // Check if all required Stripe prices exist
            if (string.IsNullOrEmpty(plan.StripeMonthlyPriceId))
            {
                result.Issues.Add("Missing Stripe monthly price ID");
            }
            if (string.IsNullOrEmpty(plan.StripeQuarterlyPriceId))
            {
                result.Issues.Add("Missing Stripe quarterly price ID");
            }
            if (string.IsNullOrEmpty(plan.StripeAnnualPriceId))
            {
                result.Issues.Add("Missing Stripe annual price ID");
            }

            if (result.Issues.Count == 0)
            {
                result.IsSynchronized = true;
                result.Recommendations.Add("Plan is fully synchronized with Stripe");
            }
            else
            {
                result.Recommendations.Add("Run plan synchronization repair to fix missing Stripe resources");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating plan synchronization {PlanId}", planId);
            result.Issues.Add($"Validation error: {ex.Message}");
        }

        return result;
    }

    public async Task<StripeSyncValidationResult> ValidateSubscriptionSynchronizationAsync(Guid subscriptionId, TokenModel tokenModel)
    {
        var result = new StripeSyncValidationResult();
        
        try
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
            if (subscription == null)
            {
                result.Issues.Add("Subscription not found");
                return result;
            }

            // Check if subscription has Stripe integration
            if (string.IsNullOrEmpty(subscription.StripeSubscriptionId))
            {
                result.Issues.Add("Subscription has no Stripe subscription ID");
                result.Recommendations.Add("Run subscription synchronization to create Stripe resources");
                return result;
            }

            // Check if subscription plan has Stripe integration
            var plan = await _subscriptionRepository.GetSubscriptionPlanByIdAsync(subscription.SubscriptionPlanId);
            if (plan != null && string.IsNullOrEmpty(plan.StripeProductId))
            {
                result.Issues.Add("Subscription plan has no Stripe integration");
                result.Recommendations.Add("Run plan synchronization first");
            }

            if (result.Issues.Count == 0)
            {
                result.IsSynchronized = true;
                result.Recommendations.Add("Subscription is fully synchronized with Stripe");
            }
            else
            {
                result.Recommendations.Add("Run subscription synchronization repair to fix missing Stripe resources");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating subscription synchronization {SubscriptionId}", subscriptionId);
            result.Issues.Add($"Validation error: {ex.Message}");
        }

        return result;
    }

    public async Task<bool> RepairPlanSynchronizationAsync(Guid planId, TokenModel tokenModel)
    {
        try
        {
            var plan = await _subscriptionRepository.GetSubscriptionPlanByIdAsync(planId);
            if (plan == null)
            {
                _logger.LogWarning("Subscription plan {PlanId} not found for repair", planId);
                return false;
            }

            _logger.LogInformation("Repairing Stripe synchronization for plan: {PlanName}", plan.Name);

            // Force recreation of Stripe resources
            if (!string.IsNullOrEmpty(plan.StripeProductId))
            {
                // Clean up existing resources first
                await SynchronizeSubscriptionPlanDeletionAsync(planId, tokenModel);
            }

            // Create new resources
            return await CreateNewPlanInStripeAsync(plan, tokenModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error repairing plan synchronization {PlanId}", planId);
            return false;
        }
    }

    public async Task<bool> RepairSubscriptionSynchronizationAsync(Guid subscriptionId, TokenModel tokenModel)
    {
        try
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found for repair", subscriptionId);
                return false;
            }

            _logger.LogInformation("Repairing Stripe synchronization for subscription: {SubscriptionId}", subscriptionId);

            // Ensure customer exists in Stripe
            await SynchronizeCustomerAsync(subscription.UserId, tokenModel);

            // Get user to get Stripe customer ID
            var user = await _userRepository.GetByIdAsync(subscription.UserId);
            if (user == null || string.IsNullOrEmpty(user.StripeCustomerId))
            {
                _logger.LogError("User {UserId} not found or has no Stripe customer ID", subscription.UserId);
                return false;
            }

            // Get plan to get Stripe price ID
            var plan = await _subscriptionRepository.GetSubscriptionPlanByIdAsync(subscription.SubscriptionPlanId);
            if (plan == null)
            {
                _logger.LogError("Subscription plan {PlanId} not found", subscription.SubscriptionPlanId);
                return false;
            }

            // Determine appropriate Stripe price ID based on billing cycle
            string stripePriceId = null;
            switch (subscription.BillingCycleId.ToString().ToLower())
            {
                case "monthly":
                    stripePriceId = plan.StripeMonthlyPriceId;
                    break;
                case "quarterly":
                    stripePriceId = plan.StripeQuarterlyPriceId;
                    break;
                case "annual":
                    stripePriceId = plan.StripeAnnualPriceId;
                    break;
            }

            if (string.IsNullOrEmpty(stripePriceId))
            {
                _logger.LogError("No Stripe price ID found for billing cycle {BillingCycleId}", subscription.BillingCycleId);
                return false;
            }

            // Create new Stripe subscription
            var stripeSubscriptionId = await _stripeService.CreateSubscriptionAsync(
                user.StripeCustomerId,
                stripePriceId,
                subscription.PaymentMethodId ?? user.StripeCustomerId,
                tokenModel
            );

            // Update subscription with new Stripe subscription ID
            subscription.StripeSubscriptionId = stripeSubscriptionId;
            await _subscriptionRepository.UpdateAsync(subscription);

            _logger.LogInformation("Successfully repaired subscription {SubscriptionId} synchronization with new Stripe subscription {StripeId}", 
                subscriptionId, stripeSubscriptionId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error repairing subscription synchronization {SubscriptionId}", subscriptionId);
            return false;
        }
    }

    private async Task<bool> CreateNewPlanInStripeAsync(SubscriptionPlan plan, TokenModel tokenModel)
    {
        try
        {
            // Create Stripe product
            var stripeProductId = await _stripeService.CreateProductAsync(plan.Name, plan.Description ?? "", tokenModel);
            plan.StripeProductId = stripeProductId;

            // Get billing cycle details
            var billingCycle = await _subscriptionRepository.GetBillingCycleByIdAsync(plan.BillingCycleId);
            if (billingCycle == null)
            {
                _logger.LogError("Billing cycle {BillingCycleId} not found for plan {PlanName}", plan.BillingCycleId, plan.Name);
                return false;
            }

            // Create Stripe prices for different billing cycles
            var monthlyPriceId = await _stripeService.CreatePriceAsync(
                stripeProductId, plan.Price, "usd", "month", 1, tokenModel);
            plan.StripeMonthlyPriceId = monthlyPriceId;

            var quarterlyPriceId = await _stripeService.CreatePriceAsync(
                stripeProductId, plan.Price * 3, "usd", "month", 3, tokenModel);
            plan.StripeQuarterlyPriceId = quarterlyPriceId;

            var annualPriceId = await _stripeService.CreatePriceAsync(
                stripeProductId, plan.Price * 12, "usd", "month", 12, tokenModel);
            plan.StripeAnnualPriceId = annualPriceId;

            // Update plan with Stripe IDs
            await _subscriptionRepository.UpdateSubscriptionPlanAsync(plan);

            _logger.LogInformation("Successfully created Stripe resources for plan {PlanName}: Product {ProductId}, Prices {MonthlyId}, {QuarterlyId}, {AnnualId}", 
                plan.Name, stripeProductId, monthlyPriceId, quarterlyPriceId, annualPriceId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating new plan in Stripe for plan {PlanName}", plan.Name);
            return false;
        }
    }

    private async Task<bool> UpdateExistingPlanInStripeAsync(SubscriptionPlan plan, TokenModel tokenModel)
    {
        try
        {
            // Update product name and description
            await _stripeService.UpdateProductAsync(plan.StripeProductId, plan.Name, plan.Description ?? "", tokenModel);

            _logger.LogInformation("Successfully updated existing Stripe product for plan {PlanName}", plan.Name);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating existing plan in Stripe for plan {PlanName}", plan.Name);
            return false;
        }
    }
}
