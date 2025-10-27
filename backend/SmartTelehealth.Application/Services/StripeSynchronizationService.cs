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
    // TODO: Implement these repositories when needed
    // private readonly IStripeSyncHistoryRepository _syncHistoryRepository;
    // private readonly IWebhookEventRepository _webhookEventRepository;
    private readonly ILogger<StripeSynchronizationService> _logger;

    public StripeSynchronizationService(
        IStripeService stripeService,
        ISubscriptionRepository subscriptionRepository,
        IUserRepository userRepository,
        // IStripeSyncHistoryRepository syncHistoryRepository,
        // IWebhookEventRepository webhookEventRepository,
        ILogger<StripeSynchronizationService> logger)
    {
        _stripeService = stripeService ?? throw new ArgumentNullException(nameof(stripeService));
        _subscriptionRepository = subscriptionRepository ?? throw new ArgumentNullException(nameof(subscriptionRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        // _syncHistoryRepository = syncHistoryRepository ?? throw new ArgumentNullException(nameof(syncHistoryRepository));
        // _webhookEventRepository = webhookEventRepository ?? throw new ArgumentNullException(nameof(webhookEventRepository));
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

            // NEW ARCHITECTURE: Deactivate the single price
            if (!string.IsNullOrEmpty(plan.StripePriceId))
            {
                await _stripeService.DeactivatePriceAsync(plan.StripePriceId, tokenModel);
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
            var subscription = await _subscriptionRepository.GetByIdWithDetailsAsync(subscriptionId);
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
            // NEW ARCHITECTURE: Check for single Stripe price ID
            if (string.IsNullOrEmpty(plan.StripePriceId))
            {
                result.Issues.Add("Missing Stripe price ID");
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
            var subscription = await _subscriptionRepository.GetByIdWithDetailsAsync(subscriptionId);
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
            var subscription = await _subscriptionRepository.GetByIdWithDetailsAsync(subscriptionId);
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

            // NEW ARCHITECTURE: Get the plan's single Stripe price ID
            if (string.IsNullOrEmpty(plan.StripePriceId))
            {
                _logger.LogError("No Stripe price ID configured for plan {PlanId}", plan.Id);
                return false;
            }
            string stripePriceId = plan.StripePriceId;

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

            // NEW ARCHITECTURE: Create only ONE Stripe price for plan's billing cycle
            // Get billing cycle to determine interval
            var billingCycle = await _subscriptionRepository.GetBillingCycleByIdAsync(plan.BillingCycleId);
            if (billingCycle == null)
            {
                _logger.LogError("Billing cycle {BillingCycleId} not found for plan {PlanName}", plan.BillingCycleId, plan.Name);
                return false;
            }
            
            var (interval, intervalCount) = billingCycle.Name?.ToLower() switch
            {
                "monthly" => ("month", 1),
                "quarterly" => ("month", 3),
                "annual" => ("year", 1),
                "weekly" => ("week", 1),
                "daily" => ("day", 1),
                _ => ("month", 1)
            };
            
            // Get currency code for Stripe integration
            var currency = await _subscriptionRepository.GetCurrencyByIdAsync(plan.CurrencyId);
            var currencyCode = currency?.Code?.ToLower() ?? "usd"; // Fallback to USD if not found
            
            // Create single Stripe price for this plan's billing cycle
            var stripePriceId = await _stripeService.CreatePriceAsync(
                stripeProductId,
                plan.BasePrice,  // Use plan's base price
                currencyCode,
                interval,
                intervalCount,
                tokenModel);
            
            // NEW ARCHITECTURE: Simply set the single Stripe price ID
            plan.StripePriceId = stripePriceId;

            // Update plan with Stripe IDs
            await _subscriptionRepository.UpdateSubscriptionPlanAsync(plan);

            _logger.LogInformation("Successfully created Stripe resources for plan {PlanName}: Product {ProductId}, Price {PriceId} ({Cycle})", 
                plan.Name, stripeProductId, stripePriceId, billingCycle.Name);
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

    #region Phase 5: Stripe Sync Dashboard Enhancements

    public async Task<JsonModel> GetAllDiscrepanciesAsync(TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Getting all Stripe sync discrepancies by user {UserId}", tokenModel.UserID);

            var planDiscrepancies = new List<object>();
            var subscriptionDiscrepancies = new List<object>();
            var customerDiscrepancies = new List<object>();

            // Check all plans
            var allPlans = await _subscriptionRepository.GetAllSubscriptionPlansAsync();
            foreach (var plan in allPlans)
            {
                var validation = await ValidatePlanSynchronizationAsync(plan.Id, tokenModel);
                if (!validation.IsSynchronized)
                {
                    planDiscrepancies.Add(new
                    {
                        planId = plan.Id,
                        planName = plan.Name,
                        issues = validation.Issues,
                        recommendations = validation.Recommendations
                    });
                }
            }

            // Check active subscriptions (limit for performance)
            var activeSubscriptions = await _subscriptionRepository.GetAllSubscriptionsAsync();
            var activeSubsList = activeSubscriptions.Where(s => s.Status == Subscription.SubscriptionStatuses.Active).Take(100);
            
            foreach (var subscription in activeSubsList)
            {
                if (!string.IsNullOrEmpty(subscription.StripeSubscriptionId))
                {
                    var validation = await ValidateSubscriptionSynchronizationAsync(subscription.Id, tokenModel);
                    if (!validation.IsSynchronized)
                    {
                        subscriptionDiscrepancies.Add(new
                        {
                            subscriptionId = subscription.Id,
                            userId = subscription.UserId,
                            userName = subscription.User?.FullName ?? "Unknown",
                            issues = validation.Issues
                        });
                    }
                }
            }

            var summary = new
            {
                planDiscrepancies,
                subscriptionDiscrepancies,
                customerDiscrepancies,
                totalIssues = planDiscrepancies.Count + subscriptionDiscrepancies.Count + customerDiscrepancies.Count,
                timestamp = DateTime.UtcNow
            };

            return new JsonModel
            {
                data = summary,
                Message = "Discrepancies retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting discrepancies by user {UserId}", tokenModel.UserID);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to retrieve discrepancies",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> BulkSyncAsync(BulkSyncRequestDto request, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Bulk syncing {Count} {EntityType} entities by user {UserId}", 
                request.Ids.Count, request.EntityType, tokenModel.UserID);

            var results = new List<object>();
            var successCount = 0;
            var failureCount = 0;

            foreach (var id in request.Ids)
            {
                try
                {
                    bool syncResult = false;

                    switch (request.EntityType.ToLower())
                    {
                        case "plans":
                            syncResult = await SynchronizeSubscriptionPlanAsync(Guid.Parse(id), tokenModel);
                            break;
                        case "customers":
                            syncResult = await SynchronizeCustomerAsync(int.Parse(id), tokenModel);
                            break;
                        default:
                            results.Add(new { id, success = false, message = "Invalid entity type" });
                            failureCount++;
                            continue;
                    }

                    if (syncResult)
                    {
                        successCount++;
                        results.Add(new { id, success = true, message = "Synchronized successfully" });
                    }
                    else
                    {
                        failureCount++;
                        results.Add(new { id, success = false, message = "Synchronization failed" });
                    }

                    // Delay to avoid rate limiting
                    if (request.DelayBetweenSyncsMs > 0)
                        await Task.Delay(request.DelayBetweenSyncsMs);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error syncing {EntityType} {Id}", request.EntityType, id);
                    failureCount++;
                    results.Add(new { id, success = false, message = ex.Message });
                    
                    if (!request.ContinueOnError)
                        break;
                }
            }

            return new JsonModel
            {
                data = new
                {
                    totalProcessed = results.Count,
                    successCount,
                    failureCount,
                    results
                },
                Message = $"Bulk sync completed: {successCount} succeeded, {failureCount} failed",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in bulk sync by user {UserId}", tokenModel.UserID);
            return new JsonModel
            {
                data = new object(),
                Message = "Bulk sync operation failed",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetSyncHistoryAsync(int page, int pageSize, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Getting sync history (page {Page}) by user {UserId}", page, tokenModel.UserID);

            // TODO: Implement sync history repository when available
            return new JsonModel
            {
                data = new List<object>(),
                Message = "Sync history feature not yet implemented",
                StatusCode = 501,
                meta = new Meta
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalRecords = 0,
                    TotalPages = 0,
                    HasNextPage = false,
                    HasPreviousPage = false
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sync history by user {UserId}", tokenModel.UserID);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to retrieve sync history",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetWebhookStatusAsync(TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Getting webhook status by user {UserId}", tokenModel.UserID);

            // TODO: Implement webhook event repository when available
            var status = new
            {
                webhookHealthy = true,
                lastWebhookReceived = DateTime.UtcNow.AddHours(-1),
                recentEvents = new List<object>(),
                statusMessage = "Webhook monitoring not yet implemented"
            };

            return new JsonModel
            {
                data = status,
                Message = "Webhook status feature not yet implemented",
                StatusCode = 501
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting webhook status by user {UserId}", tokenModel.UserID);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to retrieve webhook status",
                StatusCode = 500
            };
        }
    }

    #endregion
}
