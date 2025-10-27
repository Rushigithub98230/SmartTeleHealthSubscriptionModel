using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using Stripe;
using System;
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
        ILogger<WebhookService> logger)
    {
        _subscriptionRepository = subscriptionRepository;
        _billingService = billingService;
        _lifecycleService = lifecycleService;
        _notificationService = notificationService;
        _userRepository = userRepository;
        _billingRepository = billingRepository;
        _paymentService = paymentService;
        _subscriptionService = subscriptionService;
        _logger = logger;
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

            var tokenModel = new TokenModel { UserID = 1 }; // System user for webhook processing
            var localSubscriptionResult = await _subscriptionService.GetByStripeSubscriptionIdAsync(subscription.Id, tokenModel);
            
            if (localSubscriptionResult.StatusCode == 200)
            {
                var subscriptionData = localSubscriptionResult.data as dynamic;
                if (subscriptionData != null)
                {
                    var updateDto = new UpdateSubscriptionDto
                    {
                        Status = MapStripeStatusToLocal(subscription.Status),
                        NextBillingDate = GetNextBillingDateFromSubscription(subscription),
                        CurrentPrice = subscription.Items.Data.FirstOrDefault()?.Price.UnitAmount / 100m ?? 0,
                        StripeSubscriptionId = subscription.Id,
                        UpdatedDate = DateTime.UtcNow
                    };

                    // Add trial information if available
                    if (subscription.TrialEnd.HasValue)
                    {
                        updateDto.TrialEndDate = subscription.TrialEnd.Value;
                    }

                    // Add pause information if subscription is paused
                    if (subscription.PauseCollection != null)
                    {
                        updateDto.PausedDate = subscription.PauseCollection.ResumesAt;
                    }

                    await _lifecycleService.UpdateSubscriptionAsync(localSubscriptionResult.data.ToString(), updateDto, tokenModel);

                    _logger.LogInformation("Successfully updated subscription {SubscriptionId} via webhook. Status: {Status}", 
                        subscription.Id, subscription.Status);
                }
            }
            else
            {
                _logger.LogWarning("Local subscription not found for Stripe subscription {StripeSubscriptionId}", subscription.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing subscription updated event");
            throw;
        }
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

    #endregion

    #region Payment Events

    /// <summary>
    /// Handles payment succeeded event from Stripe
    /// </summary>
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

                        var updateDto = new UpdateSubscriptionDto
                        {
                            Status = newStatus,
                            LastPaymentDate = DateTime.UtcNow,
                            FailedPaymentAttempts = 0, // Reset failed attempts
                            LastPaymentError = null // Clear error
                        };

                        await _lifecycleService.UpdateSubscriptionAsync(localSubscriptionResult.data.ToString(), updateDto, tokenModel);

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
                        // Update subscription status to PaymentFailed
                        var updateDto = new UpdateSubscriptionDto
                        {
                            Status = "PaymentFailed",
                            LastPaymentFailedDate = DateTime.UtcNow,
                            LastPaymentError = "Payment failed via Stripe",
                            FailedPaymentAttempts = 1 // Increment failed attempts
                        };
                        
                        await _lifecycleService.UpdateSubscriptionAsync(localSubscriptionResult.data.ToString(), updateDto, tokenModel);

                        // Send payment failure notification
                        await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                        {
                            UserId = subscriptionData.UserId,
                            Title = "Payment Failed",
                            Message = $"Your payment for subscription has failed. Please update your payment method to continue your subscription. Invoice: {invoice.Number}",
                            Type = "PaymentFailed",
                            IsRead = false,
                            Priority = "High"
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

            _logger.LogInformation("Processing payment intent requires action event for payment intent {PaymentIntentId}", paymentIntent.Id);
            
            // Log that additional action is required
            _logger.LogInformation("Payment intent {PaymentIntentId} requires additional action: {NextAction}", 
                paymentIntent.Id, paymentIntent.NextAction?.Type);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment intent requires action event");
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
            var invoice = stripeEvent.Data.Object as Stripe.Invoice;
            if (invoice == null)
            {
                _logger.LogWarning("Invoice created event received but no invoice data found");
                return;
            }

            _logger.LogInformation("Processing invoice created event for invoice {InvoiceId}", invoice.Id);
            
            // Log invoice creation
            _logger.LogInformation("Invoice {InvoiceId} created for amount {Amount} with status {Status}", 
                invoice.Id, invoice.AmountDue, invoice.Status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing invoice created event");
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
            var invoice = stripeEvent.Data.Object as Stripe.Invoice;
            if (invoice == null)
            {
                _logger.LogWarning("Invoice finalized event received but no invoice data found");
                return;
            }

            _logger.LogInformation("Processing invoice finalized event for invoice {InvoiceId}", invoice.Id);
            
            // Log invoice finalization
            _logger.LogInformation("Invoice {InvoiceId} finalized for amount {Amount}", 
                invoice.Id, invoice.AmountDue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing invoice finalized event");
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
            var invoice = stripeEvent.Data.Object as Stripe.Invoice;
            if (invoice == null)
            {
                _logger.LogWarning("Invoice voided event received but no invoice data found");
                return;
            }

            _logger.LogInformation("Processing invoice voided event for invoice {InvoiceId}", invoice.Id);
            
            // Log invoice voided
            _logger.LogInformation("Invoice {InvoiceId} voided", invoice.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing invoice voided event");
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

            _logger.LogInformation("Processing invoice payment action required event for invoice {InvoiceId}", invoice.Id);
            
            // Log action required
            _logger.LogInformation("Invoice {InvoiceId} payment requires additional action", invoice.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing invoice payment action required event");
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
            
            // Log customer update
            _logger.LogInformation("Customer {CustomerId} updated", customer.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing customer updated event");
            throw;
        }
    }

    /// <summary>
    /// Handles customer deleted event from Stripe
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

            _logger.LogInformation("Processing customer deleted event for customer {CustomerId}", customer.Id);
            
            // Log customer deletion
            _logger.LogInformation("Customer {CustomerId} deleted", customer.Id);
            
            // Note: Customer deletion should be handled carefully as it affects all subscriptions
            // We typically don't delete customers automatically but mark them as inactive
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
            var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
            if (session == null)
            {
                _logger.LogWarning("Checkout session completed event received but no session data found");
                return;
            }

            _logger.LogInformation("Processing checkout session completed event for session {SessionId}", session.Id);

            // Log checkout session completion
            _logger.LogInformation("Checkout session {SessionId} completed for customer {CustomerId}", 
                session.Id, session.CustomerId);

            // Extract user ID from session metadata
            if (!session.Metadata.TryGetValue("created_by_user_id", out var userIdStr) || 
                !int.TryParse(userIdStr, out var userId))
            {
                _logger.LogWarning("No valid user ID found in checkout session {SessionId} metadata", session.Id);
                return;
            }

            // Get the subscription from Stripe
            if (string.IsNullOrEmpty(session.SubscriptionId))
            {
                _logger.LogWarning("No subscription ID found in checkout session {SessionId}", session.Id);
                return;
            }

            // Extract plan ID from session metadata (much better approach!)
            if (!session.Metadata.TryGetValue("plan_id", out var planIdStr) || 
                !Guid.TryParse(planIdStr, out var planId))
            {
                _logger.LogError("No valid plan ID found in checkout session {SessionId} metadata", session.Id);
                return;
            }

            var tokenModel = new TokenModel { UserID = userId, RoleID = 1 }; // Default role for system operations

            // Get the subscription plan using the plan ID
            var plan = await _subscriptionRepository.GetSubscriptionPlanByIdAsync(planId);
            if (plan == null)
            {
                _logger.LogError("Failed to get subscription plan {PlanId} for session {SessionId}", planId, session.Id);
                return;
            }

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

            // Create subscription using the existing lifecycle service method
            var result = await _lifecycleService.CreateSubscriptionAsync(createDto, tokenModel);

            if (result.StatusCode == 200)
            {
                // CRITICAL FIX: Update the subscription with Stripe IDs after creation
                var subscription = result.data as SubscriptionDto;
                if (subscription != null)
                {
                    try
                    {
                        // Get the subscription entity from database
                        var subscriptionEntity = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscription.Id));
                        if (subscriptionEntity != null)
                        {
                            // Update Stripe integration fields
                            subscriptionEntity.StripeSubscriptionId = session.SubscriptionId;
                            subscriptionEntity.StripeCustomerId = session.CustomerId;
                            subscriptionEntity.StripePriceId = session.Metadata["price_id"];
                            subscriptionEntity.UpdatedBy = userId;
                            subscriptionEntity.UpdatedDate = DateTime.UtcNow;
                            
                            // Save the updated subscription
                            await _subscriptionRepository.UpdateAsync(subscriptionEntity);
                            await _subscriptionRepository.SaveChangesAsync();
                            
                            _logger.LogInformation("Successfully updated subscription {SubscriptionId} with Stripe IDs from checkout session {SessionId} for user {UserId}", 
                                subscription.Id, session.Id, userId);
                        }
                        else
                        {
                            _logger.LogError("Failed to find subscription entity {SubscriptionId} for Stripe ID update", subscription.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to update subscription {SubscriptionId} with Stripe IDs from checkout session {SessionId}", 
                            subscription.Id, session.Id);
                        // Don't throw - subscription was created successfully, just Stripe ID sync failed
                    }
                }
            }
            else
            {
                _logger.LogError("Failed to create subscription from checkout session {SessionId} for user {UserId}: {Message}", 
                    session.Id, userId, result.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing checkout session completed event");
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
}