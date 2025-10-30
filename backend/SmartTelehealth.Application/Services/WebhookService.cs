using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Application.Utilities;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Enums;
using SmartTelehealth.Core.Interfaces;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SmartTelehealth.Application.Services;

/// <summary>
/// Service for handling Stripe webhook events and maintaining data consistency
/// between Stripe and the local database.
/// </summary>
public class WebhookService : IWebhookService
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly ISubscriptionBillingService _billingService;
    private readonly ISubscriptionLifecycleService _lifecycleService;
    private readonly INotificationService _notificationService;
    private readonly IUserRepository _userRepository;
    private readonly IBillingRepository _billingRepository;
    private readonly IPaymentService _paymentService;
    private readonly ISubscriptionService _subscriptionService;
    private readonly IUserSubscriptionPrivilegeUsageRepository _privilegeUsageRepository;
    private readonly ISubscriptionPlanRepository _subscriptionPlanRepository;
    private readonly IUnprocessedWebhookEventRepository _unprocessedWebhookEventRepository;
    private readonly ILogger<WebhookService> _logger;

    public WebhookService(
        ISubscriptionRepository subscriptionRepository,
        ISubscriptionBillingService billingService,
        ISubscriptionLifecycleService lifecycleService,
        INotificationService notificationService,
        IUserRepository userRepository,
        IBillingRepository billingRepository,
        IPaymentService paymentService,
        ISubscriptionService subscriptionService,
        IUserSubscriptionPrivilegeUsageRepository privilegeUsageRepository,
        ISubscriptionPlanRepository subscriptionPlanRepository,
        IUnprocessedWebhookEventRepository unprocessedWebhookEventRepository,
        ILogger<WebhookService> logger)
    {
        _subscriptionRepository = subscriptionRepository ?? throw new ArgumentNullException(nameof(subscriptionRepository));
        _billingService = billingService ?? throw new ArgumentNullException(nameof(billingService));
        _lifecycleService = lifecycleService ?? throw new ArgumentNullException(nameof(lifecycleService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _billingRepository = billingRepository ?? throw new ArgumentNullException(nameof(billingRepository));
        _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
        _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
        _privilegeUsageRepository = privilegeUsageRepository ?? throw new ArgumentNullException(nameof(privilegeUsageRepository));
        _subscriptionPlanRepository = subscriptionPlanRepository ?? throw new ArgumentNullException(nameof(subscriptionPlanRepository));
        _unprocessedWebhookEventRepository = unprocessedWebhookEventRepository ?? throw new ArgumentNullException(nameof(unprocessedWebhookEventRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Core Subscription Events

    /// <summary>
    /// Handles subscription created event from Stripe
    /// </summary>
    public async Task HandleSubscriptionCreatedAsync(Event stripeEvent)
    {
        try
        {
            var subscription = stripeEvent.Data.Object as Stripe.Subscription;
            if (subscription == null)
            {
                _logger.LogWarning("Subscription created event received but no subscription data found");
                return;
            }

            _logger.LogInformation("Processing subscription created event for Stripe subscription {StripeSubscriptionId}", subscription.Id);

            // Find local subscription by Stripe subscription ID
            var tokenModel = new TokenModel { UserID = 1 }; // System user for webhook processing
            var localSubscriptionResult = await _subscriptionService.GetByStripeSubscriptionIdAsync(subscription.Id, tokenModel);
            
            if (localSubscriptionResult.StatusCode == 200)
            {
                // Subscription already exists, update status
                var updateDto = new UpdateSubscriptionDto
                {
                    StripeSubscriptionId = subscription.Id,
                    Status = MapStripeStatusToLocal(subscription.Status)
                };
                await _lifecycleService.UpdateSubscriptionAsync(localSubscriptionResult.data.ToString(), updateDto, tokenModel);
                
                _logger.LogInformation("Successfully updated existing subscription {SubscriptionId} via webhook", localSubscriptionResult.data);
            }
            else
            {
                _logger.LogWarning("Local subscription not found for Stripe subscription {StripeSubscriptionId}", subscription.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing subscription created event");
            throw;
        }
    }

    /// <summary>
    /// Handles subscription updated event from Stripe
    /// </summary>
    public async Task HandleSubscriptionUpdatedAsync(Event stripeEvent)
    {
        try
        {
            var subscription = stripeEvent.Data.Object as Stripe.Subscription;
            if (subscription == null)
            {
                _logger.LogWarning("Subscription updated event received but no subscription data found");
                return;
            }

            _logger.LogInformation("Processing subscription updated event for Stripe subscription {StripeSubscriptionId}", subscription.Id);

            var tokenModel = new TokenModel { UserID = 1, RoleID = 1 }; // System user for webhook processing
            var localSubscriptionResult = await _subscriptionService.GetByStripeSubscriptionIdAsync(subscription.Id, tokenModel);
            
            if (localSubscriptionResult.StatusCode == 200)
            {
                var subscriptionDataObj = localSubscriptionResult.data;
                if (subscriptionDataObj != null)
                {
                    Guid subscriptionId;
                    
                    // Try to extract subscription ID safely
                    if (subscriptionDataObj is SubscriptionDto dto)
                    {
                        if (!Guid.TryParse(dto.Id, out subscriptionId))
                        {
                            _logger.LogError("Failed to parse subscription ID from DTO");
                            return;
                        }
                    }
                    else
                    {
                        dynamic subscriptionData = subscriptionDataObj;
                        if (!Guid.TryParse(subscriptionData.id.ToString(), out subscriptionId))
                        {
                            _logger.LogError("Failed to parse subscription ID from dynamic object");
                            return;
                        }
                    }

                    var newPrice = subscription.Items.Data.FirstOrDefault()?.Price.UnitAmount / 100m ?? 0;
                    var localSubscription = await _subscriptionRepository.GetByIdWithDetailsAsync(subscriptionId);
                    
                    if (localSubscription == null)
                    {
                        _logger.LogWarning("Local subscription {SubscriptionId} not found for Stripe subscription {StripeSubscriptionId}", 
                            subscriptionId, subscription.Id);
                        return;
                    }

                    // ENHANCED: Sync all subscription fields from Stripe
                    var updateDto = new UpdateSubscriptionDto
                    {
                        Status = MapStripeStatusToLocal(subscription.Status),
                        NextBillingDate = GetNextBillingDateFromSubscription(subscription),
                        CurrentPrice = newPrice,
                        StripeSubscriptionId = subscription.Id,
                        UpdatedDate = DateTime.UtcNow
                    };

                    // Sync status changes (active/paused/past_due/canceled from Stripe dashboard)
                    var newStatus = MapStripeStatusToLocal(subscription.Status);
                    if (localSubscription.Status != newStatus)
                    {
                        _logger.LogInformation("Status change detected for subscription {SubscriptionId}: {OldStatus} -> {NewStatus}", 
                            subscriptionId, localSubscription.Status, newStatus);
                        
                        // Handle status-specific updates
                        if (newStatus == Core.Entities.Subscription.SubscriptionStatuses.Paused && 
                            subscription.PauseCollection != null && 
                            subscription.PauseCollection.ResumesAt.HasValue)
                        {
                            updateDto.PausedDate = subscription.PauseCollection.ResumesAt.Value;
                        }
                        else if (newStatus == Core.Entities.Subscription.SubscriptionStatuses.Cancelled)
                        {
                            updateDto.CancelledDate = DateTime.UtcNow;
                            updateDto.CancellationReason = "Cancelled via Stripe dashboard";
                        }
                    }

                    // Sync current_period_end (billing period end date) - update directly on entity
                    if (subscription.CurrentPeriodEnd != default)
                    {
                        var newEndDate = subscription.CurrentPeriodEnd.ToUniversalTime();
                        if (localSubscription.EndDate != newEndDate)
                        {
                            _logger.LogInformation("End date updated for subscription {SubscriptionId}: {OldDate} -> {NewDate}", 
                                subscriptionId, localSubscription.EndDate, newEndDate);
                            localSubscription.EndDate = newEndDate;
                        }
                    }

                    // Sync trial_end (trial expiration date)
                    if (subscription.TrialEnd.HasValue)
                    {
                        var newTrialEnd = subscription.TrialEnd.Value.ToUniversalTime();
                        if (localSubscription.TrialEndDate != newTrialEnd)
                        {
                            _logger.LogInformation("Trial end date updated for subscription {SubscriptionId}: {OldDate} -> {NewDate}", 
                                subscriptionId, localSubscription.TrialEndDate, newTrialEnd);
                            updateDto.TrialEndDate = newTrialEnd;
                        }
                    }
                    else if (localSubscription.TrialEndDate.HasValue && !subscription.TrialEnd.HasValue)
                    {
                        // Trial ended - clear trial end date
                        updateDto.TrialEndDate = null;
                    }

                    // Sync price updates
                    if (Math.Abs(localSubscription.CurrentPrice - newPrice) > 0.01m)
                    {
                        _logger.LogInformation("Price updated for subscription {SubscriptionId}: ${OldPrice} -> ${NewPrice}", 
                            subscriptionId, localSubscription.CurrentPrice, newPrice);
                        updateDto.CurrentPrice = newPrice;
                    }

                    // Sync collection_method changes (if supported)
                    if (!string.IsNullOrEmpty(subscription.CollectionMethod))
                    {
                        _logger.LogDebug("Collection method for subscription {SubscriptionId}: {Method}", 
                            subscriptionId, subscription.CollectionMethod);
                    }

                    // 🎯 DETECT SCHEDULED PLAN CHANGE EXECUTION
                    // Check if this subscription has a pending plan change that Stripe just executed
                    bool planChangeExecuted = false;
                    
                    if (localSubscription != null && 
                        localSubscription.PendingPlanChangeId.HasValue && 
                        !string.IsNullOrEmpty(subscription.Items.Data.FirstOrDefault()?.Price.Id))
                    {
                        var newStripePriceId = subscription.Items.Data.FirstOrDefault()?.Price.Id;
                        var pendingPlan = await _subscriptionRepository.GetSubscriptionPlanByIdAsync(localSubscription.PendingPlanChangeId.Value);
                        
                        if (pendingPlan != null)
                        {
                            // Check if the new Stripe price matches the pending plan's price
                            var pendingPlanStripePriceId = GetStripePriceIdForPlan(pendingPlan);
                            
                            if (pendingPlanStripePriceId == newStripePriceId || 
                                Math.Abs(newPrice - pendingPlan.BasePrice) < 0.01m)
                            {
                                _logger.LogInformation("🎉 SCHEDULED PLAN CHANGE EXECUTED! Subscription {SubscriptionId} switched from {OldPlan} to {NewPlan}", 
                                    localSubscription.Id, localSubscription.SubscriptionPlan?.Name, pendingPlan.Name);
                                
                                planChangeExecuted = true;
                                
                                // Apply the plan change locally
                                localSubscription.SubscriptionPlanId = localSubscription.PendingPlanChangeId.Value;
                                localSubscription.CurrentPrice = pendingPlan.BasePrice;
                                
                                // Also sync status and date changes from Stripe
                                localSubscription.Status = MapStripeStatusToLocal(subscription.Status);
                                if (subscription.CurrentPeriodEnd != default)
                                {
                                    localSubscription.EndDate = subscription.CurrentPeriodEnd.ToUniversalTime();
                                }
                                if (subscription.TrialEnd.HasValue)
                                {
                                    localSubscription.TrialEndDate = subscription.TrialEnd.Value.ToUniversalTime();
                                }
                                else if (localSubscription.TrialEndDate.HasValue && !subscription.TrialEnd.HasValue)
                                {
                                    localSubscription.TrialEndDate = null;
                                }
                                
                                // Clear pending change fields
                                localSubscription.PendingPlanChangeId = null;
                                localSubscription.PlanChangeEffectiveDate = null;
                                localSubscription.PendingChangeType = null;
                                
                                localSubscription.UpdatedBy = tokenModel.UserID;
                                localSubscription.UpdatedDate = DateTime.UtcNow;
                                
                                await _subscriptionRepository.UpdateAsync(localSubscription);
                                await _subscriptionRepository.SaveChangesAsync();
                                
                                _logger.LogInformation("✅ Updated local subscription to new plan {PlanId}", pendingPlan.Id);
                                
                                // Reset privileges to new plan's limits
                                try
                                {
                                    _logger.LogInformation("🔄 Resetting privileges to new plan limits for subscription {SubscriptionId}", 
                                        localSubscription.Id);
                                    
                                    // Get all privilege usage records for this subscription
                                    var usageRecords = await _privilegeUsageRepository.GetBySubscriptionIdAsync(localSubscription.Id);
                                    
                                    if (usageRecords != null && usageRecords.Any())
                                    {
                                        // Reload subscription with plan details for privilege reset
                                        var subWithPlan = await _subscriptionRepository.GetByIdWithDetailsAsync(localSubscription.Id);
                                        
                                        if (subWithPlan != null && subWithPlan.SubscriptionPlan?.PlanPrivileges != null)
                                        {
                                            // Reset each privilege to new plan's limits
                                            foreach (var usage in usageRecords)
                                            {
                                                var planPrivilege = subWithPlan.SubscriptionPlan.PlanPrivileges
                                                    .FirstOrDefault(pp => pp.PrivilegeId == usage.PrivilegeId);
                                                
                                                if (planPrivilege != null)
                                                {
                                                    usage.UsedValue = 0;
                                                    usage.ResetAt = DateTime.UtcNow;
                                                    usage.UpdatedBy = tokenModel.UserID;
                                                    usage.UpdatedDate = DateTime.UtcNow;
                                                    
                                                    await _privilegeUsageRepository.UpdateAsync(usage);
                                                }
                                            }
                                            
                                            await _privilegeUsageRepository.SaveChangesAsync();
                                            
                                            _logger.LogInformation("✅ Successfully reset {Count} privileges for subscription {SubscriptionId}", 
                                                usageRecords.Count(), localSubscription.Id);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, "❌ Error resetting privileges after plan change for subscription {SubscriptionId}", 
                                        localSubscription.Id);
                                    // Don't fail webhook processing if privilege reset fails
                                }
                                
                                // Send notification to user about completed plan change
                                try
                                {
                                    var user = await _userRepository.GetByIdAsync(localSubscription.UserId);
                                    if (user != null)
                                    {
                                        _logger.LogInformation("📧 User {UserId} will be notified about completed plan change", user.Id);
                                        // TODO: Implement SendPlanChangeCompletedNotificationAsync in INotificationService
                                        // await _notificationService.SendPlanChangeCompletedNotificationAsync(
                                        //     user.Email,
                                        //     user.FirstName,
                                        //     localSubscription.SubscriptionPlan?.Name ?? "your old plan",
                                        //     pendingPlan.Name,
                                        //     pendingPlan.BasePrice
                                        // );
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogWarning(ex, "Failed to send plan change notification to user {UserId}", 
                                        localSubscription.UserId);
                                }
                            }
                        }
                    }

                    // Standard subscription update (if plan change wasn't executed)
                    if (!planChangeExecuted)
                    {
                        // Save any direct entity changes (like EndDate)
                        await _subscriptionRepository.UpdateAsync(localSubscription);
                        await _subscriptionRepository.SaveChangesAsync();
                        
                        // Apply other updates via lifecycle service
                        await _lifecycleService.UpdateSubscriptionAsync(subscriptionId.ToString(), updateDto, tokenModel);

                        _logger.LogInformation("Successfully updated subscription {SubscriptionId} via webhook. Status: {Status}", 
                            subscription.Id, subscription.Status);
                    }
                }
            }
            else
            {
                _logger.LogWarning("Local subscription not found for Stripe subscription {StripeSubscriptionId}", subscription.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error processing subscription updated event");
            throw;
        }
    }
    
    /// <summary>
    /// Helper method to get the Stripe price ID for a plan
    /// </summary>
    private string GetStripePriceIdForPlan(SubscriptionPlan plan)
    {
        if (string.IsNullOrEmpty(plan.StripePriceId))
        {
            throw new Exception($"No Stripe price ID configured for plan {plan.Name}");
        }
        return plan.StripePriceId;
    }

    /// <summary>
    /// Handles subscription deleted event from Stripe
    /// </summary>
    public async Task HandleSubscriptionDeletedAsync(Event stripeEvent)
    {
        try
        {
            var subscription = stripeEvent.Data.Object as Stripe.Subscription;
            if (subscription == null)
            {
                _logger.LogWarning("Subscription deleted event received but no subscription data found");
                return;
            }

            _logger.LogInformation("Processing subscription deleted event for Stripe subscription {StripeSubscriptionId}", subscription.Id);

            var tokenModel = new TokenModel { UserID = 1 }; // System user for webhook processing
            var localSubscriptionResult = await _subscriptionService.GetByStripeSubscriptionIdAsync(subscription.Id, tokenModel);
            
            if (localSubscriptionResult.StatusCode == 200)
            {
                await _lifecycleService.CancelSubscriptionAsync(localSubscriptionResult.data.ToString(), "Cancelled via Stripe webhook", tokenModel);
                _logger.LogInformation("Successfully cancelled subscription {SubscriptionId} via webhook", localSubscriptionResult.data);
            }
            else
            {
                _logger.LogWarning("Local subscription not found for Stripe subscription {StripeSubscriptionId}", subscription.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing subscription deleted event");
            throw;
        }
    }

    /// <summary>
    /// Handles subscription paused event from Stripe
    /// </summary>
    public async Task HandleSubscriptionPausedAsync(Event stripeEvent)
    {
        try
        {
            var subscription = stripeEvent.Data.Object as Stripe.Subscription;
            if (subscription == null)
            {
                _logger.LogWarning("Subscription paused event received but no subscription data found");
                return;
            }

            _logger.LogInformation("Processing subscription paused event for Stripe subscription {StripeSubscriptionId}", subscription.Id);

            var tokenModel = new TokenModel { UserID = 1 };
            var localSubscriptionResult = await _subscriptionService.GetByStripeSubscriptionIdAsync(subscription.Id, tokenModel);
            
            if (localSubscriptionResult.StatusCode == 200)
            {
                var updateDto = new UpdateSubscriptionDto
                {
                    Status = "Paused",
                    PausedDate = subscription.PauseCollection?.ResumesAt ?? DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                };

                await _lifecycleService.UpdateSubscriptionAsync(localSubscriptionResult.data.ToString(), updateDto, tokenModel);
                
                _logger.LogInformation("Successfully paused subscription {SubscriptionId} via webhook", localSubscriptionResult.data);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing subscription paused event");
            throw;
        }
    }

    /// <summary>
    /// Handles subscription resumed event from Stripe
    /// </summary>
    public async Task HandleSubscriptionResumedAsync(Event stripeEvent)
    {
        try
        {
            var subscription = stripeEvent.Data.Object as Stripe.Subscription;
            if (subscription == null)
            {
                _logger.LogWarning("Subscription resumed event received but no subscription data found");
                return;
            }

            _logger.LogInformation("Processing subscription resumed event for Stripe subscription {StripeSubscriptionId}", subscription.Id);

            var tokenModel = new TokenModel { UserID = 1 };
            var localSubscriptionResult = await _subscriptionService.GetByStripeSubscriptionIdAsync(subscription.Id, tokenModel);
            
            if (localSubscriptionResult.StatusCode == 200)
            {
                var updateDto = new UpdateSubscriptionDto
                {
                    Status = "Active",
                    PausedDate = null,
                    UpdatedDate = DateTime.UtcNow
                };

                await _lifecycleService.UpdateSubscriptionAsync(localSubscriptionResult.data.ToString(), updateDto, tokenModel);
                
                _logger.LogInformation("Successfully resumed subscription {SubscriptionId} via webhook", localSubscriptionResult.data);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing subscription resumed event");
            throw;
        }
    }

    /// <summary>
    /// Handles subscription past due event from Stripe
    /// </summary>
    public async Task HandleSubscriptionPastDueAsync(Event stripeEvent)
    {
        try
        {
            var subscription = stripeEvent.Data.Object as Stripe.Subscription;
            if (subscription == null)
            {
                _logger.LogWarning("Subscription past due event received but no subscription data found");
                return;
            }

            _logger.LogInformation("Processing subscription past due event for Stripe subscription {StripeSubscriptionId}", subscription.Id);

            var tokenModel = new TokenModel { UserID = 1 };
            var localSubscriptionResult = await _subscriptionService.GetByStripeSubscriptionIdAsync(subscription.Id, tokenModel);
            
            if (localSubscriptionResult.StatusCode == 200)
            {
                var subscriptionData = localSubscriptionResult.data as dynamic;
                if (subscriptionData != null)
                {
                    var updateDto = new UpdateSubscriptionDto
                    {
                        Status = "PaymentFailed",
                        LastPaymentFailedDate = DateTime.UtcNow,
                        LastPaymentError = "Payment past due via Stripe",
                        FailedPaymentAttempts = 1
                    };

                    await _lifecycleService.UpdateSubscriptionAsync(localSubscriptionResult.data.ToString(), updateDto, tokenModel);

                    // Send past due notification
                    await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                    {
                        UserId = subscriptionData.UserId,
                        Title = "Payment Past Due",
                        Message = "Your subscription payment is past due. Please update your payment method to avoid service interruption.",
                        Type = "PaymentPastDue",
                        IsRead = false,
                        Priority = "High"
                    }, tokenModel);

                    _logger.LogInformation("Successfully updated subscription {SubscriptionId} to past due status via webhook", localSubscriptionResult.data);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing subscription past due event");
            throw;
        }
    }

    /// <summary>
    /// Handles subscription unpaid event from Stripe
    /// </summary>
    public async Task HandleSubscriptionUnpaidAsync(Event stripeEvent)
    {
        try
        {
            var subscription = stripeEvent.Data.Object as Stripe.Subscription;
            if (subscription == null)
            {
                _logger.LogWarning("Subscription unpaid event received but no subscription data found");
                return;
            }

            _logger.LogInformation("Processing subscription unpaid event for Stripe subscription {StripeSubscriptionId}", subscription.Id);

            var tokenModel = new TokenModel { UserID = 1 };
            var localSubscriptionResult = await _subscriptionService.GetByStripeSubscriptionIdAsync(subscription.Id, tokenModel);
            
            if (localSubscriptionResult.StatusCode == 200)
            {
                var subscriptionData = localSubscriptionResult.data as dynamic;
                if (subscriptionData != null)
                {
                    var updateDto = new UpdateSubscriptionDto
                    {
                        Status = "PaymentFailed",
                        LastPaymentFailedDate = DateTime.UtcNow,
                        LastPaymentError = "Payment unpaid via Stripe",
                        FailedPaymentAttempts = 1
                    };

                    await _lifecycleService.UpdateSubscriptionAsync(localSubscriptionResult.data.ToString(), updateDto, tokenModel);

                    // Send unpaid notification
                    await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                    {
                        UserId = subscriptionData.UserId,
                        Title = "Payment Unpaid",
                        Message = "Your subscription payment could not be processed. Please update your payment method immediately.",
                        Type = "PaymentUnpaid",
                        IsRead = false,
                        Priority = "High"
                    }, tokenModel);

                    _logger.LogInformation("Successfully updated subscription {SubscriptionId} to unpaid status via webhook", localSubscriptionResult.data);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing subscription unpaid event");
            throw;
        }
    }

    /// <summary>
    /// Handles subscription trial will end event from Stripe
    /// </summary>
    public async Task HandleSubscriptionTrialWillEndAsync(Event stripeEvent)
    {
        try
        {
            var subscription = stripeEvent.Data.Object as Stripe.Subscription;
            if (subscription == null)
            {
                _logger.LogWarning("Subscription trial will end event received but no subscription data found");
                return;
            }

            _logger.LogInformation("Processing subscription trial will end event for Stripe subscription {StripeSubscriptionId}", subscription.Id);

            var tokenModel = new TokenModel { UserID = 1 };
            var localSubscriptionResult = await _subscriptionService.GetByStripeSubscriptionIdAsync(subscription.Id, tokenModel);
            
            if (localSubscriptionResult.StatusCode == 200)
            {
                var subscriptionData = localSubscriptionResult.data as dynamic;
                if (subscriptionData != null)
                {
                    // Send trial ending notification
                    await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                    {
                        UserId = subscriptionData.UserId,
                        Title = "Trial Ending Soon",
                        Message = $"Your trial for subscription plan will end on {subscription.TrialEnd?.ToString("MMM dd, yyyy")}. Please add a payment method to continue your subscription.",
                        Type = "TrialWarning",
                        IsRead = false,
                        Priority = "High"
                    }, tokenModel);

                    _logger.LogInformation("Successfully sent trial ending notification for subscription {SubscriptionId}", localSubscriptionResult.data);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing subscription trial will end event");
            throw;
        }
    }

    /// <summary>
    /// Handles subscription trial started event from Stripe
    /// </summary>
    public async Task HandleCustomerSubscriptionTrialStartedAsync(Event stripeEvent)
    {
        try
        {
            var subscription = stripeEvent.Data.Object as Stripe.Subscription;
            if (subscription == null)
            {
                _logger.LogWarning("Subscription trial started event received but no subscription data found");
                return;
            }

            _logger.LogInformation("Processing trial started for subscription {StripeSubscriptionId}", subscription.Id);

            var tokenModel = new TokenModel { UserID = 1 };
            var localSubscriptionResult = await _subscriptionService.GetByStripeSubscriptionIdAsync(subscription.Id, tokenModel);
            
            if (localSubscriptionResult.StatusCode == 200)
            {
                var subscriptionData = localSubscriptionResult.data as dynamic;
                if (subscriptionData != null)
                {
                    // Update subscription to TrialActive
                    var updateDto = new UpdateSubscriptionDto
                    {
                        Status = "TrialActive",
                        TrialEndDate = subscription.TrialEnd,
                        UpdatedDate = DateTime.UtcNow
                    };

                    await _lifecycleService.UpdateSubscriptionAsync(localSubscriptionResult.data.ToString(), updateDto, tokenModel);
                    
                    // Send trial started notification
                    await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                    {
                        UserId = subscriptionData.UserId,
                        Title = "Trial Started",
                        Message = "Your free trial has begun! Enjoy full access to all features.",
                        Type = "TrialStarted",
                        IsRead = false,
                        Priority = "Normal"
                    }, tokenModel);

                    _logger.LogInformation("Successfully updated subscription to trial started for {SubscriptionId}", localSubscriptionResult.data);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing trial started event");
            throw;
        }
    }

    /// <summary>
    /// Handles subscription trial ended event from Stripe
    /// </summary>
    public async Task HandleCustomerSubscriptionTrialEndedAsync(Event stripeEvent)
    {
        try
        {
            var subscription = stripeEvent.Data.Object as Stripe.Subscription;
            if (subscription == null)
            {
                _logger.LogWarning("Subscription trial ended event received but no subscription data found");
                return;
            }

            _logger.LogInformation("Processing trial ended for subscription {StripeSubscriptionId}", subscription.Id);

            var tokenModel = new TokenModel { UserID = 1 };
            var localSubscriptionResult = await _subscriptionService.GetByStripeSubscriptionIdAsync(subscription.Id, tokenModel);
            
            if (localSubscriptionResult.StatusCode == 200)
            {
                var subscriptionData = localSubscriptionResult.data as dynamic;
                if (subscriptionData != null)
                {
                    // Update subscription status based on trial outcome
                    string newStatus = subscription.Status == "active" ? "Active" : "TrialExpired";
                    
                    var updateDto = new UpdateSubscriptionDto
                    {
                        Status = newStatus,
                        UpdatedDate = DateTime.UtcNow
                    };

                    await _lifecycleService.UpdateSubscriptionAsync(localSubscriptionResult.data.ToString(), updateDto, tokenModel);
                    
                    // Send trial ended notification
                    await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                    {
                        UserId = subscriptionData.UserId,
                        Title = "Trial Ended",
                        Message = newStatus == "Active" 
                            ? "Your trial has ended and your subscription is now active!" 
                            : "Your trial has ended. Please add a payment method to continue.",
                        Type = "TrialEnded",
                        IsRead = false,
                        Priority = "High"
                    }, tokenModel);

                    _logger.LogInformation("Successfully updated subscription trial ended for {SubscriptionId}", localSubscriptionResult.data);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing trial ended event");
            throw;
        }
    }

    /// <summary>
    /// Handles invoice payment action required event from Stripe
    /// </summary>
    public async Task HandleInvoicePaymentActionRequiredAsync(Event stripeEvent)
    {
        try
        {
            var invoice = stripeEvent.Data.Object as Stripe.Invoice;
            if (invoice == null)
            {
                _logger.LogWarning("Invoice payment action required event received but no invoice data found");
                return;
            }

            _logger.LogInformation("Processing payment action required for invoice {InvoiceId}", invoice.Id);

            var subscriptionId = GetSubscriptionIdFromInvoice(invoice);
            if (!string.IsNullOrEmpty(subscriptionId))
            {
                var tokenModel = new TokenModel { UserID = 1 };
                var localSubscriptionResult = await _subscriptionService.GetByStripeSubscriptionIdAsync(subscriptionId, tokenModel);
                
                if (localSubscriptionResult.StatusCode == 200)
                {
                    var subscriptionData = localSubscriptionResult.data as dynamic;
                    if (subscriptionData != null)
                    {
                        // Send payment action required notification
                        await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                        {
                            UserId = subscriptionData.UserId,
                            Title = "Payment Action Required",
                            Message = "Your payment requires additional authentication. Please complete the payment process.",
                            Type = "PaymentActionRequired",
                            IsRead = false,
                            Priority = "High"
                        }, tokenModel);

                        _logger.LogInformation("Successfully sent payment action required notification for subscription {SubscriptionId}", localSubscriptionResult.data);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment action required event");
            throw;
        }
    }

    /// <summary>
    /// Handles payment intent requires action event from Stripe
    /// </summary>
    public async Task HandlePaymentIntentRequiresActionAsync(Event stripeEvent)
    {
        try
        {
            var paymentIntent = stripeEvent.Data.Object as Stripe.PaymentIntent;
            if (paymentIntent == null)
            {
                _logger.LogWarning("Payment intent requires action event received but no payment intent data found");
                return;
            }

            _logger.LogInformation("Processing payment intent requires action for payment intent {PaymentIntentId}", paymentIntent.Id);

            // Extract subscription ID from metadata if available
            if (paymentIntent.Metadata.TryGetValue("subscription_id", out var subscriptionIdStr))
            {
                var tokenModel = new TokenModel { UserID = 1 };
                var localSubscriptionResult = await _subscriptionService.GetByStripeSubscriptionIdAsync(subscriptionIdStr, tokenModel);
                
                if (localSubscriptionResult.StatusCode == 200)
                {
                    var subscriptionData = localSubscriptionResult.data as dynamic;
                    if (subscriptionData != null)
                    {
                        // Send payment action required notification
                        await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                        {
                            UserId = subscriptionData.UserId,
                            Title = "Payment Authentication Required",
                            Message = "Your payment requires additional authentication. Please complete the authentication process.",
                            Type = "PaymentAuthenticationRequired",
                            IsRead = false,
                            Priority = "High"
                        }, tokenModel);

                        _logger.LogInformation("Successfully sent payment authentication required notification for subscription {SubscriptionId}", localSubscriptionResult.data);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment intent requires action event");
            throw;
        }
    }

    /// <summary>
    /// Handles charge succeeded event from Stripe
    /// </summary>
    public async Task HandleChargeSucceededAsync(Event stripeEvent)
    {
        try
        {
            var charge = stripeEvent.Data.Object as Stripe.Charge;
            if (charge == null)
            {
                _logger.LogWarning("Charge succeeded event received but no charge data found");
                return;
            }

            _logger.LogInformation("Processing charge succeeded event for charge {ChargeId}", charge.Id);

            // Extract subscription ID from metadata if available
            if (charge.Metadata.TryGetValue("subscription_id", out var subscriptionIdStr))
            {
                var tokenModel = new TokenModel { UserID = 1 };
                var localSubscriptionResult = await _subscriptionService.GetByStripeSubscriptionIdAsync(subscriptionIdStr, tokenModel);
                
                if (localSubscriptionResult.StatusCode == 200)
                {
                    var subscriptionData = localSubscriptionResult.data as dynamic;
                    if (subscriptionData != null)
                    {
                        // Log successful charge for audit purposes
                        _logger.LogInformation("Charge {ChargeId} succeeded for subscription {SubscriptionId}, amount: {Amount}", 
                            charge.Id, localSubscriptionResult.data, charge.Amount);

                        // Update subscription last payment date
                        var updateDto = new UpdateSubscriptionDto
                        {
                            LastPaymentDate = DateTime.UtcNow,
                            UpdatedDate = DateTime.UtcNow
                        };

                        await _lifecycleService.UpdateSubscriptionAsync(localSubscriptionResult.data.ToString(), updateDto, tokenModel);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing charge succeeded event");
            throw;
        }
    }

    /// <summary>
    /// Handles charge failed event from Stripe
    /// </summary>
    public async Task HandleChargeFailedAsync(Event stripeEvent)
    {
        try
        {
            var charge = stripeEvent.Data.Object as Stripe.Charge;
            if (charge == null)
            {
                _logger.LogWarning("Charge failed event received but no charge data found");
                return;
            }

            _logger.LogInformation("Processing charge failed event for charge {ChargeId}", charge.Id);

            // Extract subscription ID from metadata if available
            if (charge.Metadata.TryGetValue("subscription_id", out var subscriptionIdStr))
            {
                var tokenModel = new TokenModel { UserID = 1 };
                var localSubscriptionResult = await _subscriptionService.GetByStripeSubscriptionIdAsync(subscriptionIdStr, tokenModel);
                
                if (localSubscriptionResult.StatusCode == 200)
                {
                    var subscriptionData = localSubscriptionResult.data as dynamic;
                    if (subscriptionData != null)
                    {
                        // Log failed charge for audit purposes
                        _logger.LogWarning("Charge {ChargeId} failed for subscription {SubscriptionId}, amount: {Amount}, failure code: {FailureCode}", 
                            charge.Id, localSubscriptionResult.data, charge.Amount, charge.FailureCode);

                        // Send payment failed notification
                        await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                        {
                            UserId = subscriptionData.UserId,
                            Title = "Payment Failed",
                            Message = $"Your payment of ${charge.Amount / 100m:F2} failed. Please update your payment method.",
                            Type = "PaymentFailed",
                            IsRead = false,
                            Priority = "High"
                        }, tokenModel);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing charge failed event");
            throw;
        }
    }

    /// <summary>
    /// Handles charge captured event from Stripe
    /// </summary>
    public async Task HandleChargeCapturedAsync(Event stripeEvent)
    {
        try
        {
            var charge = stripeEvent.Data.Object as Stripe.Charge;
            if (charge == null)
            {
                _logger.LogWarning("Charge captured event received but no charge data found");
                return;
            }

            _logger.LogInformation("Processing charge captured event for charge {ChargeId}", charge.Id);

            // Extract subscription ID from metadata if available
            if (charge.Metadata.TryGetValue("subscription_id", out var subscriptionIdStr))
            {
                var tokenModel = new TokenModel { UserID = 1 };
                var localSubscriptionResult = await _subscriptionService.GetByStripeSubscriptionIdAsync(subscriptionIdStr, tokenModel);
                
                if (localSubscriptionResult.StatusCode == 200)
                {
                    var subscriptionData = localSubscriptionResult.data as dynamic;
                    if (subscriptionData != null)
                    {
                        // Log captured charge for audit purposes
                        _logger.LogInformation("Charge {ChargeId} captured for subscription {SubscriptionId}, amount: {Amount}", 
                            charge.Id, localSubscriptionResult.data, charge.Amount);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing charge captured event");
            throw;
        }
    }
    public async Task HandlePaymentSucceededAsync(Event stripeEvent)
    {
        Stripe.Invoice? invoice = null;
        try
        {
            invoice = stripeEvent.Data.Object as Stripe.Invoice;
            if (invoice == null)
            {
                _logger.LogWarning("Payment succeeded event received but no invoice data found");
                return;
            }

            _logger.LogInformation("Processing payment succeeded event for invoice {InvoiceId}", invoice.Id);

            var subscriptionId = GetSubscriptionIdFromInvoice(invoice);
            if (!string.IsNullOrEmpty(subscriptionId))
            {
                var tokenModel = new TokenModel { UserID = 1 };
                var localSubscriptionResult = await _subscriptionService.GetByStripeSubscriptionIdAsync(subscriptionId, tokenModel);
                
                if (localSubscriptionResult.StatusCode == 200)
                {
                    var subscriptionData = localSubscriptionResult.data as dynamic;
                    if (subscriptionData != null)
                    {
                        // Determine new status based on current state
                        string newStatus = "Active";
                        string reason = "Payment succeeded via Stripe";

                        // If this was a trial subscription, transition to Active
                        if (subscriptionData.Status == "TrialActive")
                        {
                            newStatus = "Active";
                            reason = "Trial converted to active subscription via payment";
                        }
                        // If this was a failed payment, reactivate
                        else if (subscriptionData.Status == "PaymentFailed")
                        {
                            newStatus = "Active";
                            reason = "Subscription reactivated after successful payment";
                        }

                        // CRITICAL: Sync billing dates from Stripe invoice to ensure accuracy
                        var updateDto = new UpdateSubscriptionDto
                        {
                            Status = newStatus,
                            LastPaymentDate = DateTime.UtcNow,
                            FailedPaymentAttempts = 0, // Reset failed attempts
                            LastPaymentError = null, // Clear error
                            NextBillingDate = invoice.PeriodEnd.ToUniversalTime()
                        };

                        await _lifecycleService.UpdateSubscriptionAsync(localSubscriptionResult.data.ToString(), updateDto, tokenModel);
                        
                        _logger.LogInformation("💳 Updated subscription billing dates - NextBillingDate: {NextBilling}, BillingReason: {BillingReason}", 
                            invoice.PeriodEnd.ToUniversalTime(), invoice.BillingReason ?? "N/A");

                        // Send payment success notification
                        await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                        {
                            UserId = subscriptionData.UserId,
                            Title = "Payment Successful",
                            Message = $"Your payment for subscription has been processed successfully. Invoice: {invoice.Number}",
                            Type = "PaymentSuccess",
                            IsRead = false,
                            Priority = "Normal"
                        }, tokenModel);

                        // Check if billing record already exists before creating new one
                        var existingBillingRecord = await _billingRepository.GetByStripeInvoiceIdAsync(invoice.Id);
                        
                        if (existingBillingRecord != null)
                        {
                            // Update existing billing record instead of creating duplicate
                            _logger.LogInformation("Found existing billing record {BillingRecordId} for invoice {InvoiceId}. Updating instead of creating new.", 
                                existingBillingRecord.Id, invoice.Id);
                            
                            existingBillingRecord.Status = BillingRecord.BillingStatus.Paid;
                            existingBillingRecord.PaidAt = DateTime.UtcNow;
                            existingBillingRecord.StripePaymentIntentId = GetPaymentIntentIdFromInvoice(invoice);
                            existingBillingRecord.ProcessedAt = DateTime.UtcNow;
                            existingBillingRecord.UpdatedBy = 0; // System
                            existingBillingRecord.UpdatedDate = DateTime.UtcNow;
                            
                            await _billingRepository.UpdateAsync(existingBillingRecord);
                            
                            // Record external payment to create SubscriptionPayment, update billing dates, and reset privileges
                            var paymentRecordingResult = await _paymentService.RecordExternalPaymentAsync(existingBillingRecord.Id, tokenModel);
                            
                            if (paymentRecordingResult.StatusCode != 200)
                            {
                                _logger.LogError("Failed to record external payment for existing billing record {BillingRecordId}. Error: {Error}", 
                                    existingBillingRecord.Id, paymentRecordingResult.Message);
                                
                                throw new InvalidOperationException(
                                    $"Failed to record external payment for billing record {existingBillingRecord.Id}. " +
                                    $"This is critical as it prevents privilege reset and billing date updates. Error: {paymentRecordingResult.Message}");
                            }
                            
                            _logger.LogInformation("Successfully updated existing billing record {BillingRecordId} and recorded external payment", 
                                existingBillingRecord.Id);
                        }
                        else
                        {
                            // No existing record - create new billing record
                            _logger.LogInformation("No existing billing record found for invoice {InvoiceId}. Creating new billing record.", invoice.Id);
                            
                            var billingRecordDto = new CreateBillingRecordDto
                            {
                                UserId = subscriptionData.UserId,
                                Amount = (decimal)(invoice.AmountPaid / 100),
                                CurrencyId = null, // Will use default currency
                                PaymentMethod = "stripe",
                                StripeInvoiceId = invoice.Id,
                                StripePaymentIntentId = GetPaymentIntentIdFromInvoice(invoice),
                                Status = BillingRecord.BillingStatus.Paid.ToString(),
                                Description = $"Payment for subscription - Invoice: {invoice.Number}",
                                BillingDate = invoice.Created,
                                PaidDate = DateTime.UtcNow,
                                Type = BillingRecord.BillingType.Subscription.ToString(),
                                InvoiceNumber = invoice.Number,
                                SubscriptionId = subscriptionId
                            };

                            var billingResult = await _billingService.CreateBillingRecordAsync(billingRecordDto, tokenModel);
                            
                            if (billingResult.StatusCode != 200)
                            {
                                _logger.LogError("Failed to create billing record for successful payment. Invoice: {InvoiceId}, Error: {Error}", 
                                    invoice.Id, billingResult.Message);
                            }
                            else
                            {
                                // Record external payment to create SubscriptionPayment, update billing dates, and reset privileges
                                var billingRecordId = ExtractBillingRecordId(billingResult);
                                if (billingRecordId.HasValue)
                                {
                                    var paymentRecordingResult = await _paymentService.RecordExternalPaymentAsync(billingRecordId.Value, tokenModel);
                                    
                                    if (paymentRecordingResult.StatusCode != 200)
                                    {
                                        _logger.LogError("Failed to record external payment for billing record {BillingRecordId}. Error: {Error}", 
                                            billingRecordId.Value, paymentRecordingResult.Message);
                                        
                                        throw new InvalidOperationException(
                                            $"Failed to record external payment for billing record {billingRecordId.Value}. " +
                                            $"This is critical as it prevents privilege reset and billing date updates. Error: {paymentRecordingResult.Message}");
                                    }
                                    
                                    _logger.LogInformation("Successfully created new billing record {BillingRecordId} and recorded external payment", 
                                        billingRecordId.Value);
                                }
                                else
                                {
                                    _logger.LogError("Failed to extract billing record ID from billing result for invoice {InvoiceId}", invoice.Id);
                                    
                                    throw new InvalidOperationException(
                                        $"Failed to extract billing record ID from billing result for invoice {invoice.Id}. " +
                                        $"Cannot record external payment without billing record ID.");
                                }
                            }
                        }

                        // Send payment success email
                        var billingRecord = new BillingRecordDto 
                        { 
                            Amount = (decimal)(invoice.AmountPaid / 100), 
                            PaidDate = DateTime.UtcNow, 
                            Description = $"Payment for subscription - Invoice: {invoice.Number}" 
                        };
                        await _notificationService.SendPaymentSuccessEmailAsync(
                            subscriptionData.UserEmail, 
                            subscriptionData.UserName, 
                            billingRecord, 
                            tokenModel);

                        _logger.LogInformation("Payment success handled for subscription {SubscriptionId}, invoice {InvoiceNumber}", 
                            subscriptionId, invoice.Number);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment succeeded event for invoice {InvoiceNumber}", invoice?.Number ?? "Unknown");
            throw;
        }
    }

    /// <summary>
    /// Handles payment failed event from Stripe
    /// </summary>
    public async Task HandlePaymentFailedAsync(Event stripeEvent)
    {
        Stripe.Invoice? invoice = null;
        try
        {
            invoice = stripeEvent.Data.Object as Stripe.Invoice;
            if (invoice == null)
            {
                _logger.LogWarning("Payment failed event received but no invoice data found");
                return;
            }

            _logger.LogInformation("Processing payment failed event for invoice {InvoiceId}", invoice.Id);

            var subscriptionId = GetSubscriptionIdFromInvoice(invoice);
            if (!string.IsNullOrEmpty(subscriptionId))
            {
                var tokenModel = new TokenModel { UserID = 1 };
                var localSubscriptionResult = await _subscriptionService.GetByStripeSubscriptionIdAsync(subscriptionId, tokenModel);
                
                if (localSubscriptionResult.StatusCode == 200)
                {
                    var subscriptionData = localSubscriptionResult.data as dynamic;
                    if (subscriptionData != null)
                    {
                        // Get current failed attempts count
                        int currentFailedAttempts = subscriptionData.FailedPaymentAttempts ?? 0;
                        int newFailedAttempts = currentFailedAttempts + 1;
                        
                        _logger.LogWarning("⚠️ Payment failed for subscription {SubscriptionId} - Attempt #{Attempts}", 
                            subscriptionId, newFailedAttempts);
                        
                        // Determine status based on failed attempts
                        string newStatus = "PaymentFailed";
                        if (newFailedAttempts >= 3)
                        {
                            newStatus = "Suspended";
                            _logger.LogError("🚫 Suspending subscription {SubscriptionId} after {Attempts} failed payment attempts", 
                                subscriptionId, newFailedAttempts);
                        }
                        
                        // Update subscription status to PaymentFailed or Suspended
                        var updateDto = new UpdateSubscriptionDto
                        {
                            Status = newStatus,
                            LastPaymentFailedDate = DateTime.UtcNow,
                            LastPaymentError = $"Payment failed via Stripe - Invoice: {invoice.Number}",
                            FailedPaymentAttempts = newFailedAttempts
                        };
                        
                        await _lifecycleService.UpdateSubscriptionAsync(localSubscriptionResult.data.ToString(), updateDto, tokenModel);

                        // Send payment failure notification with escalated priority for suspensions
                        string notificationTitle = newStatus == "Suspended" 
                            ? "Subscription Suspended - Action Required" 
                            : "Payment Failed";
                        string notificationMessage = newStatus == "Suspended"
                            ? $"Your subscription has been suspended after {newFailedAttempts} failed payment attempts. Please update your payment method immediately to restore access. Invoice: {invoice.Number}"
                            : $"Your payment for subscription has failed (Attempt #{newFailedAttempts}). Please update your payment method to continue your subscription. Invoice: {invoice.Number}";
                        string notificationPriority = newStatus == "Suspended" ? "Critical" : "High";
                        
                        await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                        {
                            UserId = subscriptionData.UserId,
                            Title = notificationTitle,
                            Message = notificationMessage,
                            Type = "PaymentFailed",
                            IsRead = false,
                            Priority = notificationPriority
                        }, tokenModel);

                        // Create billing record for failed payment
                        var failedBillingRecordDto = new CreateBillingRecordDto
                        {
                            UserId = subscriptionData.UserId,
                            Amount = (decimal)(invoice.AmountDue / 100),
                            CurrencyId = null, // Will use default currency
                            PaymentMethod = "stripe",
                            StripeInvoiceId = invoice.Id,
                            StripePaymentIntentId = GetPaymentIntentIdFromInvoice(invoice),
                            Status = BillingRecord.BillingStatus.Failed.ToString(),
                            Description = $"Failed payment for subscription - Invoice: {invoice.Number}",
                            BillingDate = invoice.Created,
                            Type = BillingRecord.BillingType.Subscription.ToString(),
                            InvoiceNumber = invoice.Number,
                            ErrorMessage = "Payment failed via Stripe",
                            SubscriptionId = subscriptionId
                        };

                        var failedBillingResult = await _billingService.CreateBillingRecordAsync(failedBillingRecordDto, tokenModel);
                        
                        if (failedBillingResult.StatusCode != 200)
                        {
                            _logger.LogError("Failed to create billing record for failed payment. Invoice: {InvoiceId}, Error: {Error}", 
                                invoice.Id, failedBillingResult.Message);
                        }

                        // Send payment failed email
                        var billingRecord = new BillingRecordDto 
                        { 
                            Amount = (decimal)(invoice.AmountDue / 100), 
                            PaidDate = DateTime.UtcNow, 
                            Description = $"Failed payment for subscription - Invoice: {invoice.Number}" 
                        };
                        await _notificationService.SendPaymentFailedEmailAsync(
                            subscriptionData.UserEmail, 
                            subscriptionData.UserName, 
                            billingRecord, 
                            tokenModel);

                        // Check if this is a trial subscription that needs special handling
                        if (subscriptionData.Status == "TrialActive")
                        {
                            await HandleTrialPaymentFailure(subscriptionData.Id?.ToString(), invoice.Number);
                        }

                        _logger.LogInformation("Payment failure handled for subscription {SubscriptionId}, invoice {InvoiceNumber}", 
                            subscriptionId, invoice.Number);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment failed event for invoice {InvoiceNumber}", invoice?.Number ?? "Unknown");
            throw;
        }
    }

    /// <summary>
    /// Handles payment intent succeeded event from Stripe
    /// </summary>
    public async Task HandlePaymentIntentSucceededAsync(Event stripeEvent)
    {
        try
        {
            var paymentIntent = stripeEvent.Data.Object as Stripe.PaymentIntent;
            if (paymentIntent == null)
            {
                _logger.LogWarning("Payment intent succeeded event received but no payment intent data found");
                return;
            }

            _logger.LogInformation("Processing payment intent succeeded event for payment intent {PaymentIntentId}", paymentIntent.Id);
            
            // This is typically handled by the invoice events, but we can log it here
            _logger.LogInformation("Payment intent {PaymentIntentId} succeeded with amount {Amount}", 
                paymentIntent.Id, paymentIntent.Amount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment intent succeeded event");
            throw;
        }
    }

    /// <summary>
    /// Handles payment intent failed event from Stripe
    /// </summary>
    public async Task HandlePaymentIntentFailedAsync(Event stripeEvent)
    {
        try
        {
            var paymentIntent = stripeEvent.Data.Object as Stripe.PaymentIntent;
            if (paymentIntent == null)
            {
                _logger.LogWarning("Payment intent failed event received but no payment intent data found");
                return;
            }

            _logger.LogInformation("Processing payment intent failed event for payment intent {PaymentIntentId}", paymentIntent.Id);
            
            // This is typically handled by the invoice events, but we can log it here
            _logger.LogInformation("Payment intent {PaymentIntentId} failed with error: {Error}", 
                paymentIntent.Id, paymentIntent.LastPaymentError?.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment intent failed event");
            throw;
        }
    }


    #endregion

    #region Invoice Events

    /// <summary>
    /// Handles invoice created event from Stripe
    /// </summary>
    public async Task HandleInvoiceCreatedAsync(Event stripeEvent)
    {
        try
        {
            _logger.LogInformation("💰 WEBHOOK: invoice.created - Event ID: {EventId}", stripeEvent.Id);
            
            var invoice = stripeEvent.Data.Object as Stripe.Invoice;
            if (invoice == null)
            {
                _logger.LogWarning("❌ Invoice created event received but no invoice data found");
                return;
            }

            _logger.LogInformation("📋 Processing invoice created for invoice {InvoiceId}, subscription {SubscriptionId}", 
                invoice.Id, invoice.SubscriptionId);
            _logger.LogInformation("💵 Amount: ${Amount}, Status: {Status}, Number: {Number}", 
                invoice.AmountDue / 100m, invoice.Status, invoice.Number);
            
            // Skip if not related to subscription
            if (string.IsNullOrEmpty(invoice.SubscriptionId))
            {
                _logger.LogInformation("ℹ️ Invoice {InvoiceId} is not associated with a subscription, skipping", invoice.Id);
                return;
            }
            
            var tokenModel = new TokenModel { UserID = 1, RoleID = (int)Core.Enums.RoleId.Admin };
            
            // Find local subscription
            var subscription = await _subscriptionRepository
                .GetByStripeSubscriptionIdAsync(invoice.SubscriptionId, tokenModel);
            
            if (subscription == null)
            {
                _logger.LogWarning("⚠️ Local subscription not found for Stripe subscription {StripeSubscriptionId}, queueing for retry", 
                    invoice.SubscriptionId);
                
                // Queue the event for retry processing
                await QueueEventForRetryAsync(stripeEvent, "Local subscription not found yet");
                return;
            }
            
            _logger.LogInformation("✅ Found local subscription {SubscriptionId} for user {UserId}", 
                subscription.Id, subscription.UserId);
            
            // Check if billing record already exists (idempotency)
            var existingBillingRecord = await _billingRepository
                .GetByStripeInvoiceIdAsync(invoice.Id);
            
            if (existingBillingRecord != null)
            {
                _logger.LogInformation("ℹ️ BillingRecord already exists for invoice {InvoiceId}, updating...", 
                    invoice.Id);
                
                // Update existing record
                existingBillingRecord.InvoiceNumber = invoice.Number;
                existingBillingRecord.Amount = invoice.AmountDue / 100m;
                existingBillingRecord.TotalAmount = invoice.AmountDue / 100m;
                existingBillingRecord.TaxAmount = (invoice.Tax ?? 0) / 100m;
                existingBillingRecord.Status = MapStripeInvoiceStatusToBillingStatus(invoice.Status);
                existingBillingRecord.Description = $"Subscription Invoice - {subscription.SubscriptionPlan?.Name ?? "Plan"}";
                existingBillingRecord.DueDate = invoice.DueDate?.ToUniversalTime();
                existingBillingRecord.UpdatedDate = DateTime.UtcNow;
                existingBillingRecord.UpdatedBy = 1;
                
                await _billingRepository.UpdateBillingRecordAsync(existingBillingRecord);
                
                _logger.LogInformation("✅ Updated BillingRecord {BillingRecordId} for invoice {InvoiceId}", 
                    existingBillingRecord.Id, invoice.Id);
            }
            else
            {
                _logger.LogInformation("🆕 Creating new BillingRecord for invoice {InvoiceId}", invoice.Id);
                
                // Create new billing record
                var billingRecord = new BillingRecord
                {
                    Id = Guid.NewGuid(),
                    SubscriptionId = subscription.Id,
                    UserId = subscription.UserId,
                    CurrencyId = subscription.SubscriptionPlan?.CurrencyId ?? Guid.Empty,
                    StripeInvoiceId = invoice.Id,
                    InvoiceNumber = invoice.Number,
                    Amount = invoice.AmountDue / 100m,
                    TotalAmount = invoice.AmountDue / 100m,
                    TaxAmount = (invoice.Tax ?? 0) / 100m,
                    Status = MapStripeInvoiceStatusToBillingStatus(invoice.Status),
                    Type = BillingRecord.BillingType.Subscription,
                    BillingDate = invoice.Created.ToUniversalTime(),
                    DueDate = invoice.DueDate?.ToUniversalTime(),
                    Description = $"Subscription Invoice - {subscription.SubscriptionPlan?.Name ?? "Plan"}",
                    IsRecurring = true,
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedBy = 1,
                    UpdatedDate = DateTime.UtcNow
                };
                
                await _billingRepository.CreateBillingRecordAsync(billingRecord);
                
                _logger.LogInformation("✅ Created BillingRecord {BillingRecordId} for invoice {InvoiceId}", 
                    billingRecord.Id, invoice.Id);
            }
            
            _logger.LogInformation("🎉 Successfully processed invoice.created for {InvoiceId}", invoice.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ CRITICAL ERROR processing invoice created event");
            throw;
        }
    }

    /// <summary>
    /// Handles invoice finalized event from Stripe
    /// </summary>
    public async Task HandleInvoiceFinalizedAsync(Event stripeEvent)
    {
        try
        {
            _logger.LogInformation("📄 WEBHOOK: invoice.finalized - Event ID: {EventId}", stripeEvent.Id);
            
            var invoice = stripeEvent.Data.Object as Stripe.Invoice;
            if (invoice == null)
            {
                _logger.LogWarning("❌ Invoice finalized event received but no invoice data found");
                return;
            }

            _logger.LogInformation("📋 Processing invoice finalized for invoice {InvoiceId}", invoice.Id);
            _logger.LogInformation("💵 Finalized Amount: ${Amount}, Status: {Status}", 
                invoice.AmountDue / 100m, invoice.Status);
            
            // Skip if not related to subscription
            if (string.IsNullOrEmpty(invoice.SubscriptionId))
            {
                _logger.LogInformation("ℹ️ Invoice {InvoiceId} is not associated with a subscription, skipping", invoice.Id);
                return;
            }
            
            var tokenModel = new TokenModel { UserID = 1, RoleID = (int)Core.Enums.RoleId.Admin };
            
            // Find local subscription
            var subscription = await _subscriptionRepository
                .GetByStripeSubscriptionIdAsync(invoice.SubscriptionId, tokenModel);
            
            if (subscription == null)
            {
                _logger.LogWarning("⚠️ Local subscription not found for Stripe subscription {StripeSubscriptionId}, queueing for retry", 
                    invoice.SubscriptionId);
                
                // Queue the event for retry processing
                await QueueEventForRetryAsync(stripeEvent, "Local subscription not found yet");
                return;
            }
            
            // Find or create billing record
            var billingRecord = await _billingRepository.GetByStripeInvoiceIdAsync(invoice.Id);
            
            if (billingRecord == null)
            {
                _logger.LogInformation("🆕 BillingRecord not found, creating for finalized invoice {InvoiceId}", invoice.Id);
                
                // Create new billing record (idempotency - webhook may arrive out of order)
                billingRecord = new BillingRecord
                {
                    Id = Guid.NewGuid(),
                    SubscriptionId = subscription.Id,
                    UserId = subscription.UserId,
                    CurrencyId = subscription.SubscriptionPlan?.CurrencyId ?? Guid.Empty,
                    StripeInvoiceId = invoice.Id,
                    InvoiceNumber = invoice.Number,
                    Amount = invoice.AmountDue / 100m,
                    TotalAmount = invoice.AmountDue / 100m,
                    TaxAmount = (invoice.Tax ?? 0) / 100m,
                    Status = MapStripeInvoiceStatusToBillingStatus(invoice.Status),
                    Type = BillingRecord.BillingType.Subscription,
                    BillingDate = invoice.Created.ToUniversalTime(),
                    DueDate = invoice.DueDate?.ToUniversalTime(),
                    Description = $"Subscription Invoice - {subscription.SubscriptionPlan?.Name ?? "Plan"}",
                    IsRecurring = true,
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedBy = 1,
                    UpdatedDate = DateTime.UtcNow
                };
                
                await _billingRepository.CreateBillingRecordAsync(billingRecord);
                
                _logger.LogInformation("✅ Created BillingRecord {BillingRecordId} for finalized invoice {InvoiceId}", 
                    billingRecord.Id, invoice.Id);
            }
            else
            {
                _logger.LogInformation("🔄 Updating existing BillingRecord for finalized invoice {InvoiceId}", invoice.Id);
                
                // Update existing record with finalized details
                billingRecord.InvoiceNumber = invoice.Number ?? billingRecord.InvoiceNumber;
                billingRecord.Amount = invoice.AmountDue / 100m;
                billingRecord.TotalAmount = invoice.AmountDue / 100m;
                billingRecord.TaxAmount = (invoice.Tax ?? 0) / 100m;
                billingRecord.Status = MapStripeInvoiceStatusToBillingStatus(invoice.Status);
                billingRecord.DueDate = invoice.DueDate?.ToUniversalTime();
                billingRecord.UpdatedDate = DateTime.UtcNow;
                billingRecord.UpdatedBy = 1;
                
                await _billingRepository.UpdateBillingRecordAsync(billingRecord);
                
                _logger.LogInformation("✅ Updated BillingRecord {BillingRecordId} for finalized invoice {InvoiceId}", 
                    billingRecord.Id, invoice.Id);
            }
            
            _logger.LogInformation("🎉 Successfully processed invoice.finalized for {InvoiceId}", invoice.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ CRITICAL ERROR processing invoice finalized event");
            throw;
        }
    }

    /// <summary>
    /// Handles invoice sent event from Stripe
    /// </summary>
    public async Task HandleInvoiceSentAsync(Event stripeEvent)
    {
        try
        {
            var invoice = stripeEvent.Data.Object as Stripe.Invoice;
            if (invoice == null)
            {
                _logger.LogWarning("Invoice sent event received but no invoice data found");
                return;
            }

            _logger.LogInformation("Processing invoice sent event for invoice {InvoiceId}", invoice.Id);
            
            // Log invoice sent
            _logger.LogInformation("Invoice {InvoiceId} sent to customer", invoice.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing invoice sent event");
            throw;
        }
    }

    /// <summary>
    /// Handles invoice upcoming event from Stripe
    /// </summary>
    public async Task HandleInvoiceUpcomingAsync(Event stripeEvent)
    {
        try
        {
            var invoice = stripeEvent.Data.Object as Stripe.Invoice;
            if (invoice == null)
            {
                _logger.LogWarning("Invoice upcoming event received but no invoice data found");
                return;
            }

            _logger.LogInformation("Processing invoice upcoming event for invoice {InvoiceId}", invoice.Id);
            
            // Log upcoming invoice
            _logger.LogInformation("Upcoming invoice {InvoiceId} for amount {Amount}", 
                invoice.Id, invoice.AmountDue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing invoice upcoming event");
            throw;
        }
    }

    /// <summary>
    /// Handles invoice finalization failed event from Stripe
    /// </summary>
    public async Task HandleInvoiceFinalizationFailedAsync(Event stripeEvent)
    {
        try
        {
            var invoice = stripeEvent.Data.Object as Stripe.Invoice;
            if (invoice == null)
            {
                _logger.LogWarning("Invoice finalization failed event received but no invoice data found");
                return;
            }

            _logger.LogInformation("Processing invoice finalization failed event for invoice {InvoiceId}", invoice.Id);
            
            // Log finalization failure
            _logger.LogWarning("Invoice {InvoiceId} finalization failed", invoice.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing invoice finalization failed event");
            throw;
        }
    }

    /// <summary>
    /// Handles invoice voided event from Stripe
    /// </summary>
    public async Task HandleInvoiceVoidedAsync(Event stripeEvent)
    {
        try
        {
            _logger.LogInformation("🚫 WEBHOOK: invoice.voided - Event ID: {EventId}", stripeEvent.Id);
            
            var invoice = stripeEvent.Data.Object as Stripe.Invoice;
            if (invoice == null)
            {
                _logger.LogWarning("❌ Invoice voided event received but no invoice data found");
                return;
            }

            _logger.LogInformation("📋 Processing invoice voided for invoice {InvoiceId}", invoice.Id);
            
            // Skip if not related to subscription
            if (string.IsNullOrEmpty(invoice.SubscriptionId))
            {
                _logger.LogInformation("ℹ️ Invoice {InvoiceId} is not associated with a subscription, skipping", invoice.Id);
                return;
            }
            
            // Find billing record
            var billingRecord = await _billingRepository.GetByStripeInvoiceIdAsync(invoice.Id);
            
            if (billingRecord == null)
            {
                _logger.LogWarning("⚠️ BillingRecord not found for voided invoice {InvoiceId}", invoice.Id);
                return;
            }
            
            _logger.LogInformation("🔄 Updating BillingRecord {BillingRecordId} to Cancelled status", billingRecord.Id);
            
            // Update billing record status to Cancelled
            billingRecord.Status = BillingRecord.BillingStatus.Cancelled;
            billingRecord.ErrorMessage = "Invoice voided in Stripe";
            billingRecord.UpdatedDate = DateTime.UtcNow;
            billingRecord.UpdatedBy = 1;
            
            await _billingRepository.UpdateBillingRecordAsync(billingRecord);
            
            _logger.LogInformation("✅ Updated BillingRecord {BillingRecordId} for voided invoice {InvoiceId}", 
                billingRecord.Id, invoice.Id);
            _logger.LogInformation("🎉 Successfully processed invoice.voided for {InvoiceId}", invoice.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ CRITICAL ERROR processing invoice voided event");
            throw;
        }
    }


    #endregion

    #region Payment Method Events

    /// <summary>
    /// Handles payment method attached event from Stripe
    /// </summary>
    public async Task HandlePaymentMethodAttachedAsync(Event stripeEvent)
    {
        try
        {
            var paymentMethod = stripeEvent.Data.Object as Stripe.PaymentMethod;
            if (paymentMethod == null)
            {
                _logger.LogWarning("Payment method attached event received but no payment method data found");
                return;
            }

            _logger.LogInformation("Processing payment method attached event for payment method {PaymentMethodId}", paymentMethod.Id);
            Console.WriteLine($"🔗 [WEBHOOK] Payment method attached: {paymentMethod.Id} for customer: {paymentMethod.CustomerId}");

            // Find user by Stripe customer ID
            var user = await _userRepository.GetUserByStripeCustomerIdAsync(paymentMethod.CustomerId);
            if (user == null)
            {
                _logger.LogWarning("User not found for Stripe customer {CustomerId}", paymentMethod.CustomerId);
                Console.WriteLine($"⚠️ [WEBHOOK] User not found for Stripe customer: {paymentMethod.CustomerId}");
                return;
            }

            Console.WriteLine($"👤 [WEBHOOK] Found user: {user.Id} ({user.Email}) for payment method: {paymentMethod.Id}");

            // Save payment method to database
            var tokenModel = new TokenModel { UserID = user.Id };
            await SavePaymentMethodToDatabaseAsync(paymentMethod, user.Id, tokenModel);

            _logger.LogInformation("Successfully saved payment method {PaymentMethodId} for user {UserId}", 
                paymentMethod.Id, user.Id);
            Console.WriteLine($"✅ [WEBHOOK] Successfully saved payment method {paymentMethod.Id} for user {user.Id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment method attached event");
            Console.WriteLine($"❌ [WEBHOOK] Error processing payment method attached: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Handles payment method updated event from Stripe
    /// </summary>
    public async Task HandlePaymentMethodUpdatedAsync(Event stripeEvent)
    {
        try
        {
            var paymentMethod = stripeEvent.Data.Object as Stripe.PaymentMethod;
            if (paymentMethod == null)
            {
                _logger.LogWarning("Payment method updated event received but no payment method data found");
                return;
            }

            _logger.LogInformation("Processing payment method updated event for payment method {PaymentMethodId}", paymentMethod.Id);
            
            // Log payment method update
            _logger.LogInformation("Payment method {PaymentMethodId} updated", paymentMethod.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment method updated event");
            throw;
        }
    }

    /// <summary>
    /// Handles payment method detached event from Stripe
    /// </summary>
    public async Task HandlePaymentMethodDetachedAsync(Event stripeEvent)
    {
        try
        {
            var paymentMethod = stripeEvent.Data.Object as Stripe.PaymentMethod;
            if (paymentMethod == null)
            {
                _logger.LogWarning("Payment method detached event received but no payment method data found");
                return;
            }

            _logger.LogInformation("Processing payment method detached event for payment method {PaymentMethodId}", paymentMethod.Id);
            
            // Log payment method detachment
            _logger.LogInformation("Payment method {PaymentMethodId} detached from customer {CustomerId}", 
                paymentMethod.Id, paymentMethod.CustomerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment method detached event");
            throw;
        }
    }

    #endregion

    #region Charge Events

    /// <summary>
    /// Handles charge refunded event from Stripe
    /// </summary>
    public async Task HandleChargeRefundedAsync(Event stripeEvent)
    {
        try
        {
            var charge = stripeEvent.Data.Object as Stripe.Charge;
            if (charge == null)
            {
                _logger.LogWarning("Charge refunded event received but no charge data found");
                return;
            }

            _logger.LogInformation("Processing charge refunded event for charge {ChargeId}", charge.Id);
            
            // Log charge refund
            _logger.LogInformation("Charge {ChargeId} refunded for amount {Amount}", 
                charge.Id, charge.Refunded);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing charge refunded event");
            throw;
        }
    }

    /// <summary>
    /// Handles charge dispute created event from Stripe
    /// </summary>
    public async Task HandleChargeDisputeCreatedAsync(Event stripeEvent)
    {
        try
        {
            var dispute = stripeEvent.Data.Object as Stripe.Dispute;
            if (dispute == null)
            {
                _logger.LogWarning("Charge dispute created event received but no dispute data found");
                return;
            }

            _logger.LogInformation("Processing charge dispute created event for dispute {DisputeId}", dispute.Id);
            
            // Log dispute creation
            _logger.LogWarning("Dispute {DisputeId} created for charge {ChargeId} with amount {Amount}", 
                dispute.Id, dispute.ChargeId, dispute.Amount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing charge dispute created event");
            throw;
        }
    }

    /// <summary>
    /// Handles charge dispute closed event from Stripe
    /// </summary>
    public async Task HandleChargeDisputeClosedAsync(Event stripeEvent)
    {
        try
        {
            var dispute = stripeEvent.Data.Object as Stripe.Dispute;
            if (dispute == null)
            {
                _logger.LogWarning("Charge dispute closed event received but no dispute data found");
                return;
            }

            _logger.LogInformation("Processing charge dispute closed event for dispute {DisputeId}", dispute.Id);
            
            // Log dispute closure
            _logger.LogInformation("Dispute {DisputeId} closed with status {Status}", 
                dispute.Id, dispute.Status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing charge dispute closed event");
            throw;
        }
    }

    #endregion

    #region Customer Events

    /// <summary>
    /// Handles customer created event from Stripe
    /// </summary>
    public async Task HandleCustomerCreatedAsync(Event stripeEvent)
    {
        try
        {
            var customer = stripeEvent.Data.Object as Stripe.Customer;
            if (customer == null)
            {
                _logger.LogWarning("Customer created event received but no customer data found");
                return;
            }

            _logger.LogInformation("Processing customer created event for customer {CustomerId}", customer.Id);
            
            // Log customer creation
            _logger.LogInformation("Customer {CustomerId} created with email {Email}", 
                customer.Id, customer.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing customer created event");
            throw;
        }
    }

    /// <summary>
    /// Handles customer updated event from Stripe
    /// Syncs customer data changes (email, metadata) to local User entity
    /// </summary>
    public async Task HandleCustomerUpdatedAsync(Event stripeEvent)
    {
        try
        {
            var customer = stripeEvent.Data.Object as Stripe.Customer;
            if (customer == null)
            {
                _logger.LogWarning("Customer updated event received but no customer data found");
                return;
            }

            _logger.LogInformation("Processing customer updated event for customer {CustomerId}", customer.Id);
            
            // Find local user by Stripe customer ID
            var user = await _userRepository.GetUserByStripeCustomerIdAsync(customer.Id);
            if (user == null)
            {
                _logger.LogWarning("Local user not found for Stripe customer {CustomerId}, skipping sync", customer.Id);
                return;
            }

            _logger.LogInformation("Found local user {UserId} for Stripe customer {CustomerId}", user.Id, customer.Id);

            bool hasChanges = false;

            // Sync email if changed
            if (!string.IsNullOrEmpty(customer.Email) && user.Email != customer.Email)
            {
                _logger.LogInformation("Email changed for user {UserId}: {OldEmail} -> {NewEmail}", 
                    user.Id, user.Email, customer.Email);
                user.Email = customer.Email;
                hasChanges = true;
            }

            // Sync name if changed
            if (!string.IsNullOrEmpty(customer.Name))
            {
                // Split name into first and last if possible
                var nameParts = customer.Name.Split(' ', 2);
                if (nameParts.Length > 0 && user.FirstName != nameParts[0])
                {
                    _logger.LogInformation("First name changed for user {UserId}: {OldName} -> {NewName}", 
                        user.Id, user.FirstName, nameParts[0]);
                    user.FirstName = nameParts[0];
                    hasChanges = true;
                }
                if (nameParts.Length > 1 && user.LastName != nameParts[1])
                {
                    _logger.LogInformation("Last name changed for user {UserId}: {OldName} -> {NewName}", 
                        user.Id, user.LastName, nameParts[1]);
                    user.LastName = nameParts[1];
                    hasChanges = true;
                }
            }

            // Sync phone if changed
            if (!string.IsNullOrEmpty(customer.Phone) && user.PhoneNumber != customer.Phone)
            {
                _logger.LogInformation("Phone changed for user {UserId}: {OldPhone} -> {NewPhone}", 
                    user.Id, user.PhoneNumber, customer.Phone);
                user.PhoneNumber = customer.Phone;
                hasChanges = true;
            }

            // Sync metadata if available
            if (customer.Metadata != null && customer.Metadata.Any())
            {
                foreach (var metadata in customer.Metadata)
                {
                    _logger.LogDebug("Customer metadata: {Key} = {Value}", metadata.Key, metadata.Value);
                    // Store important metadata in user notes or custom fields if needed
                }
            }

            if (hasChanges)
            {
                user.UpdatedDate = DateTime.UtcNow;
                await _userRepository.UpdateUserAsync(user);
                _logger.LogInformation("Successfully synced customer data for user {UserId}", user.Id);
            }
            else
            {
                _logger.LogInformation("No changes detected for user {UserId}", user.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing customer updated event");
            throw;
        }
    }

    /// <summary>
    /// Handles customer deleted event from Stripe
    /// Marks user inactive and cancels all subscriptions to maintain data integrity
    /// </summary>
    public async Task HandleCustomerDeletedAsync(Event stripeEvent)
    {
        try
        {
            var customer = stripeEvent.Data.Object as Stripe.Customer;
            if (customer == null)
            {
                _logger.LogWarning("Customer deleted event received but no customer data found");
                return;
            }

            _logger.LogWarning("⚠️ CRITICAL: Processing customer deleted event for customer {CustomerId}", customer.Id);
            
            // Find local user by Stripe customer ID
            var user = await _userRepository.GetUserByStripeCustomerIdAsync(customer.Id);
            if (user == null)
            {
                _logger.LogWarning("Local user not found for Stripe customer {CustomerId}, nothing to cleanup", customer.Id);
                return;
            }

            _logger.LogWarning("⚠️ Found local user {UserId} for deleted Stripe customer {CustomerId}", user.Id, customer.Id);

            var tokenModel = new TokenModel { UserID = 1, RoleID = (int)Core.Enums.RoleId.Admin };

            // Get all subscriptions for this user and filter for active ones
            var allSubscriptions = await _subscriptionRepository.GetByUserIdAsync(user.Id);
            var subscriptions = allSubscriptions.Where(s => 
                s.Status == Core.Entities.Subscription.SubscriptionStatuses.Active || 
                s.Status == Core.Entities.Subscription.SubscriptionStatuses.Paused).ToList();
            
            _logger.LogWarning("⚠️ Found {Count} active subscriptions for user {UserId}, cancelling all", 
                subscriptions.Count(), user.Id);
            
            // Cancel all subscriptions
            int cancelledCount = 0;
            foreach (var subscription in subscriptions)
            {
                try
                {
                    var cancelResult = await _lifecycleService.CancelSubscriptionAsync(
                        subscription.Id.ToString(), 
                        "Customer deleted from Stripe", 
                        tokenModel);
                    
                    if (cancelResult.StatusCode == 200)
                    {
                        cancelledCount++;
                        _logger.LogInformation("✅ Cancelled subscription {SubscriptionId} for deleted customer", subscription.Id);
                    }
                    else
                    {
                        _logger.LogError("❌ Failed to cancel subscription {SubscriptionId}: {Error}", 
                            subscription.Id, cancelResult.Message);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error cancelling subscription {SubscriptionId} for deleted customer", subscription.Id);
                }
            }

            // Mark user as inactive (don't delete - preserve audit trail)
            user.IsActive = false;
            user.UpdatedDate = DateTime.UtcNow;
            await _userRepository.UpdateUserAsync(user);

            _logger.LogWarning(
                "⚠️ CRITICAL ACTION COMPLETED: User {UserId} marked inactive, {CancelledCount}/{TotalCount} subscriptions cancelled " +
                "due to Stripe customer deletion {CustomerId}",
                user.Id, cancelledCount, subscriptions.Count(), customer.Id);

            // Send critical alert notification (if notification service supports admin alerts)
            try
            {
                await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                {
                    UserId = 1, // Admin user
                    Title = "CRITICAL: Customer Deleted from Stripe",
                    Message = $"Customer {customer.Id} was deleted from Stripe. User {user.Id} ({user.Email}) marked inactive. " +
                             $"{cancelledCount} subscriptions cancelled.",
                    Type = "SystemAlert",
                    IsRead = false,
                    Priority = "Critical"
                }, tokenModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send critical alert notification for customer deletion");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing customer deleted event");
            throw;
        }
    }

    #endregion

    #region Setup Intent Events

    /// <summary>
    /// Handles setup intent succeeded event from Stripe
    /// </summary>
    public async Task HandleSetupIntentSucceededAsync(Event stripeEvent)
    {
        try
        {
            var setupIntent = stripeEvent.Data.Object as Stripe.SetupIntent;
            if (setupIntent == null)
            {
                _logger.LogWarning("Setup intent succeeded event received but no setup intent data found");
                return;
            }

            _logger.LogInformation("Processing setup intent succeeded event for setup intent {SetupIntentId}", setupIntent.Id);
            
            // Log setup intent success
            _logger.LogInformation("Setup intent {SetupIntentId} succeeded for customer {CustomerId}", 
                setupIntent.Id, setupIntent.CustomerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing setup intent succeeded event");
            throw;
        }
    }

    /// <summary>
    /// Handles setup intent failed event from Stripe
    /// </summary>
    public async Task HandleSetupIntentFailedAsync(Event stripeEvent)
    {
        try
        {
            var setupIntent = stripeEvent.Data.Object as Stripe.SetupIntent;
            if (setupIntent == null)
            {
                _logger.LogWarning("Setup intent failed event received but no setup intent data found");
                return;
            }

            _logger.LogInformation("Processing setup intent failed event for setup intent {SetupIntentId}", setupIntent.Id);
            
            // Log setup intent failure
            _logger.LogWarning("Setup intent {SetupIntentId} failed for customer {CustomerId} with error: {Error}", 
                setupIntent.Id, setupIntent.CustomerId, setupIntent.LastSetupError?.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing setup intent failed event");
            throw;
        }
    }

    #endregion

    #region Checkout Events

    /// <summary>
    /// Handles checkout session completed event from Stripe
    /// This is triggered when a user completes payment through Stripe Checkout
    /// </summary>
    public async Task HandleCheckoutSessionCompletedAsync(Event stripeEvent)
    {
        try
        {
            _logger.LogInformation("🔔 WEBHOOK RECEIVED: checkout.session.completed - Event ID: {EventId}", stripeEvent.Id);
            
            var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
            if (session == null)
            {
                _logger.LogError("❌ Checkout session completed event received but no session data found - Event ID: {EventId}", stripeEvent.Id);
                return;
            }

            _logger.LogInformation("✅ Processing checkout session completed event for session {SessionId}", session.Id);
            _logger.LogInformation("📋 Session Details - Customer: {CustomerId}, Payment Status: {PaymentStatus}, Amount: {Amount}", 
                session.CustomerId, session.PaymentStatus, session.AmountTotal);
            _logger.LogInformation("📦 Session Metadata: {Metadata}", string.Join(", ", session.Metadata.Select(x => $"{x.Key}={x.Value}")));

            // Extract user ID from session metadata
            if (!session.Metadata.TryGetValue("created_by_user_id", out var userIdStr) || 
                !int.TryParse(userIdStr, out var userId))
            {
                _logger.LogError("❌ No valid user ID found in checkout session {SessionId} metadata. Metadata keys: {MetadataKeys}", 
                    session.Id, string.Join(", ", session.Metadata.Keys));
                return;
            }
            
            _logger.LogInformation("👤 Extracted User ID: {UserId} from checkout session {SessionId}", userId, session.Id);

            // Get the subscription from Stripe
            if (string.IsNullOrEmpty(session.SubscriptionId))
            {
                _logger.LogError("❌ No subscription ID found in checkout session {SessionId}", session.Id);
                return;
            }
            
            _logger.LogInformation("💳 Stripe Subscription ID: {StripeSubscriptionId}", session.SubscriptionId);

            // Extract plan ID from session metadata (much better approach!)
            if (!session.Metadata.TryGetValue("plan_id", out var planIdStr) || 
                !Guid.TryParse(planIdStr, out var planId))
            {
                _logger.LogError("❌ No valid plan ID found in checkout session {SessionId} metadata. Metadata keys: {MetadataKeys}", 
                    session.Id, string.Join(", ", session.Metadata.Keys));
                return;
            }
            
            _logger.LogInformation("📦 Extracted Plan ID: {PlanId} from checkout session {SessionId}", planId, session.Id);

            var tokenModel = new TokenModel { UserID = userId, RoleID = 1 }; // Default role for system operations

            // Get the subscription plan using the plan ID
            _logger.LogInformation("🔍 Fetching subscription plan {PlanId} from database...", planId);
            var plan = await _subscriptionRepository.GetSubscriptionPlanByIdAsync(planId);
            if (plan == null)
            {
                _logger.LogError("❌ Failed to get subscription plan {PlanId} for session {SessionId}", planId, session.Id);
                return;
            }
            
            _logger.LogInformation("✅ Found subscription plan: {PlanName} (ID: {PlanId})", plan.Name, plan.Id);

            // Create subscription in our database using existing method
            var createDto = new CreateSubscriptionDto
            {
                UserId = userId,
                PlanId = plan.Id.ToString(),
                Price = plan.BasePrice,
                CurrencyId = plan.CurrencyId,
                PaymentMethodId = null, // Will be set from Stripe subscription
                AutoRenew = true,
                StartImmediately = true,
                IsActive = true
            };
            
            _logger.LogInformation("🚀 Syncing subscription for User {UserId} with Plan {PlanId}...", userId, plan.Id);

            // Use the dedicated lifecycle service method for syncing checkout subscriptions
            // This ensures proper privilege initialization, notifications, and follows separation of concerns
            var stripePriceId = session.Metadata.ContainsKey("price_id") ? session.Metadata["price_id"] : null;
            
            var result = await _lifecycleService.SyncSubscriptionFromCheckoutAsync(
                userId,
                plan.Id,
                session.SubscriptionId,
                session.CustomerId,
                stripePriceId,
                tokenModel
            );
            
            if (result.StatusCode == 201 || result.StatusCode == 200)
            {
                _logger.LogInformation("🎉 SUBSCRIPTION SYNCED SUCCESSFULLY! User {UserId} now has access to Plan {PlanName}", 
                    userId, plan.Name);
            }
            else
            {
                _logger.LogError("❌ Failed to sync subscription for user {UserId}: {Message}", 
                    userId, result.Message);
                throw new Exception($"Failed to sync subscription: {result.Message}");
            }
            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ CRITICAL ERROR processing checkout session completed event for session {SessionId}", stripeEvent.Data.Object.GetType().GetProperty("Id")?.GetValue(stripeEvent.Data.Object)?.ToString() ?? "Unknown");
            throw;
        }
    }

    #endregion

    #region Payment Method Helper Methods

    /// <summary>
    /// Saves a Stripe payment method to the local database
    /// </summary>
    private async Task SavePaymentMethodToDatabaseAsync(Stripe.PaymentMethod stripePaymentMethod, int userId, TokenModel tokenModel)
    {
        try
        {
            // Check if payment method already exists
            var existingPaymentMethod = await _paymentService.GetPaymentMethodByStripeIdAsync(stripePaymentMethod.Id, tokenModel);
            if (existingPaymentMethod != null)
            {
                _logger.LogInformation("Payment method {PaymentMethodId} already exists in database", stripePaymentMethod.Id);
                return;
            }

            // Create payment method DTO
            var paymentMethodDto = new AddPaymentMethodDto
            {
                PaymentMethodId = stripePaymentMethod.Id,
                UserId = userId,
                IsDefault = false, // Will be set as default if it's the user's first payment method
                CreatedBy = userId,
                CreatedDate = DateTime.UtcNow
            };

            // Add payment method to database
            var result = await _paymentService.AddPaymentMethodAsync(paymentMethodDto, tokenModel);
            if (result.StatusCode == 200)
            {
                _logger.LogInformation("Successfully saved payment method {PaymentMethodId} to database for user {UserId}", 
                    stripePaymentMethod.Id, userId);

                // If this is the user's first payment method, set it as default
                var userPaymentMethods = await _paymentService.GetUserPaymentMethodsAsync(userId, tokenModel);
                if (userPaymentMethods.StatusCode == 200 && userPaymentMethods.data is IEnumerable<object> paymentMethodsList && paymentMethodsList.Count() == 1)
                {
                    await _paymentService.SetDefaultPaymentMethodAsync(stripePaymentMethod.Id, tokenModel);
                    _logger.LogInformation("Set payment method {PaymentMethodId} as default for user {UserId}", 
                        stripePaymentMethod.Id, userId);
                }
            }
            else
            {
                _logger.LogError("Failed to save payment method {PaymentMethodId} to database: {Message}", 
                    stripePaymentMethod.Id, result.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving payment method {PaymentMethodId} to database for user {UserId}", 
                stripePaymentMethod.Id, userId);
            throw;
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Maps Stripe subscription status to local status
    /// </summary>
    private string MapStripeStatusToLocal(string stripeStatus)
    {
        return stripeStatus switch
        {
            "active" => "Active",
            "canceled" => "Cancelled",
            "incomplete" => "Pending",
            "incomplete_expired" => "Expired",
            "past_due" => "PaymentFailed",
            "trialing" => "TrialActive",
            "unpaid" => "PaymentFailed",
            "paused" => "Paused",
            _ => "Pending"
        };
    }

    /// <summary>
    /// Gets the next billing date from Stripe subscription
    /// </summary>
    private DateTime GetNextBillingDateFromSubscription(Stripe.Subscription subscription)
    {
        try
        {
            // Use subscription.CurrentPeriodEnd directly (most reliable)
            if (subscription.CurrentPeriodEnd != default(DateTime))
            {
                return subscription.CurrentPeriodEnd;
            }
            
            // Fallback: Try to get from subscription items
            var firstItem = subscription.Items?.Data?.FirstOrDefault();
            // Note: SubscriptionItem doesn't have CurrentPeriodEnd in Stripe.NET 45.0.0
            // This fallback is not available in this version
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Failed to parse subscription billing date: {Error}", ex.Message);
        }
        
        // Fallback to default
        return DateTime.UtcNow.AddMonths(1);
    }

    /// <summary>
    /// Gets subscription ID from Stripe invoice
    /// </summary>
    private string GetSubscriptionIdFromInvoice(Stripe.Invoice invoice)
    {
        try
        {
            // Try to get from metadata first (most reliable)
            if (invoice.Metadata?.ContainsKey("subscription_id") == true)
            {
                return invoice.Metadata["subscription_id"];
            }
            
            // Additional fallback: check if Parent is a subscription
            // Note: Invoice.Parent is not available in Stripe.NET 45.0.0
            // This fallback is not available in this version
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Error extracting subscription ID from invoice {InvoiceId}: {Error}", invoice.Id, ex.Message);
        }
        
        return string.Empty;
    }

    /// <summary>
    /// Gets payment intent ID from Stripe invoice
    /// </summary>
    private string GetPaymentIntentIdFromInvoice(Stripe.Invoice invoice)
    {
        try
        {
            // Try to get from metadata first (most reliable)
            if (invoice.Metadata?.ContainsKey("payment_intent_id") == true)
            {
                return invoice.Metadata["payment_intent_id"];
            }
            
            // Note: Payment intent ID extraction from invoice is limited in Stripe.NET 45.0.0
            // The most reliable approach is through metadata or by fetching the invoice with expanded data
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Error extracting payment intent ID from invoice {InvoiceId}: {Error}", invoice.Id, ex.Message);
        }
        
        return string.Empty;
    }

    /// <summary>
    /// Extracts billing record ID from billing result
    /// </summary>
    private Guid? ExtractBillingRecordId(JsonModel billingResult)
    {
        try
        {
            if (billingResult.data == null) return null;
            
            // Try to get ID from different possible formats
            var dataType = billingResult.data.GetType();
            
            // Check if data is a JObject or dynamic object
            if (billingResult.data is Newtonsoft.Json.Linq.JObject jObject)
            {
                if (jObject["id"] != null && Guid.TryParse(jObject["id"].ToString(), out var idFromJObject))
                    return idFromJObject;
            }
            else
            {
                // Try to get Id property via reflection
                var idProperty = dataType.GetProperty("Id") ?? dataType.GetProperty("id");
                if (idProperty != null)
                {
                    var idValue = idProperty.GetValue(billingResult.data);
                    if (idValue != null && Guid.TryParse(idValue.ToString(), out var idFromProperty))
                        return idFromProperty;
                }
            }
            
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting billing record ID from result");
            return null;
        }
    }

    /// <summary>
    /// Handles trial payment failure
    /// </summary>
    private async Task HandleTrialPaymentFailure(string subscriptionId, string invoiceNumber)
    {
        try
        {
            if (string.IsNullOrEmpty(subscriptionId)) return;

            _logger.LogInformation("Handling trial payment failure for subscription {SubscriptionId}, invoice {InvoiceNumber}", 
                subscriptionId, invoiceNumber);

            // Additional logic for trial payment failures can be added here
            // For example, extending trial period, sending special notifications, etc.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling trial payment failure for subscription {SubscriptionId}", subscriptionId);
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Maps Stripe invoice status to local BillingRecord.BillingStatus enum
    /// </summary>
    private BillingRecord.BillingStatus MapStripeInvoiceStatusToBillingStatus(string? stripeStatus)
    {
        return stripeStatus?.ToLower() switch
        {
            "draft" => BillingRecord.BillingStatus.Pending,
            "open" => BillingRecord.BillingStatus.Pending,
            "paid" => BillingRecord.BillingStatus.Paid,
            "uncollectible" => BillingRecord.BillingStatus.Failed,
            "void" => BillingRecord.BillingStatus.Cancelled,
            _ => BillingRecord.BillingStatus.Pending
        };
    }

    /// <summary>
    /// Gets the appropriate Stripe price ID for a subscription plan based on its billing cycle.
    /// NEW ARCHITECTURE: Each plan has ONE billing cycle, therefore ONE Stripe price.
    /// </summary>
    private string GetStripePriceIdForPlan(SubscriptionPlan plan, int billingCycleDays)
    {
        if (string.IsNullOrEmpty(plan.StripePriceId))
        {
            _logger.LogError("Plan {PlanId} ({PlanName}) has no StripePriceId configured", plan.Id, plan.Name);
            throw new InvalidOperationException($"Plan {plan.Name} does not have a Stripe price ID configured");
        }

        _logger.LogInformation("Using Stripe price {StripePriceId} for plan {PlanName} (Billing Cycle: {Days} days)", 
            plan.StripePriceId, plan.Name, billingCycleDays);
        
        return plan.StripePriceId;
    }

    /// <summary>
    /// Queues a webhook event for retry processing when related entities are not found
    /// </summary>
    private async Task QueueEventForRetryAsync(Event stripeEvent, string failureReason)
    {
        try
        {
            // Check if event is already queued to avoid duplicates
            var existingEvent = await _unprocessedWebhookEventRepository.GetByStripeEventIdAsync(stripeEvent.Id);
            if (existingEvent != null)
            {
                _logger.LogInformation("Event {EventId} already queued for retry, skipping", stripeEvent.Id);
                return;
            }

            var unprocessedEvent = new UnprocessedWebhookEvent
            {
                Id = Guid.NewGuid(),
                StripeEventId = stripeEvent.Id,
                EventType = stripeEvent.Type,
                EventData = stripeEvent.ToJson(),
                StripeSubscriptionId = ExtractStripeSubscriptionId(stripeEvent),
                StripeInvoiceId = ExtractStripeInvoiceId(stripeEvent),
                StripeCustomerId = ExtractStripeCustomerId(stripeEvent),
                FailureReason = failureReason,
                RetryCount = 0,
                MaxRetries = 48, // 24 hours with 5-minute intervals
                NextRetryAt = DateTime.UtcNow.AddMinutes(5),
                Status = UnprocessedWebhookEvent.ProcessingStatus.Pending,
                ReceivedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _unprocessedWebhookEventRepository.CreateAsync(unprocessedEvent);
            _logger.LogInformation("Queued webhook event {EventId} for retry processing", stripeEvent.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error queueing webhook event {EventId} for retry", stripeEvent.Id);
        }
    }

    /// <summary>
    /// Extracts Stripe subscription ID from various event types
    /// </summary>
    private string? ExtractStripeSubscriptionId(Event stripeEvent)
    {
        return stripeEvent.Type switch
        {
            "invoice.created" or "invoice.finalized" or "invoice.payment_succeeded" or "invoice.payment_failed" =>
                (stripeEvent.Data.Object as Stripe.Invoice)?.SubscriptionId,
            "customer.subscription.updated" or "customer.subscription.deleted" =>
                (stripeEvent.Data.Object as Stripe.Subscription)?.Id,
            _ => null
        };
    }

    /// <summary>
    /// Extracts Stripe invoice ID from invoice events
    /// </summary>
    private string? ExtractStripeInvoiceId(Event stripeEvent)
    {
        return stripeEvent.Type.StartsWith("invoice.") 
            ? (stripeEvent.Data.Object as Stripe.Invoice)?.Id 
            : null;
    }

    /// <summary>
    /// Extracts Stripe customer ID from various event types
    /// </summary>
    private string? ExtractStripeCustomerId(Event stripeEvent)
    {
        return stripeEvent.Type switch
        {
            "invoice.created" or "invoice.finalized" or "invoice.payment_succeeded" or "invoice.payment_failed" =>
                (stripeEvent.Data.Object as Stripe.Invoice)?.CustomerId,
            "customer.subscription.updated" or "customer.subscription.deleted" =>
                (stripeEvent.Data.Object as Stripe.Subscription)?.CustomerId,
            "customer.updated" or "customer.deleted" =>
                (stripeEvent.Data.Object as Stripe.Customer)?.Id,
            _ => null
        };
    }

    #endregion
}