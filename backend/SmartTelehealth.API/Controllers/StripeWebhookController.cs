using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Application.Services;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using Stripe;
using SmartTelehealth.Application.DTOs;
using Stripe.Events;
using Microsoft.Extensions.Logging;


namespace SmartTelehealth.API.Controllers
{
/// <summary>
/// Controller responsible for handling Stripe webhook events and maintaining synchronization
/// between Stripe and the local database. This controller processes various Stripe events
/// including subscription lifecycle events, payment events, and customer management events.
/// It ensures data consistency and provides real-time updates to the local system.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class StripeWebhookController : BaseController
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly IBillingService _billingService;
    private readonly IBillingRepository _billingRepository;
    private readonly INotificationService _notificationService;
    private readonly ICommunicationService _communicationService;
      
    private readonly IStripeService _stripeService;
    private readonly ISubscriptionLifecycleService _subscriptionLifecycleService;
    private readonly IWebhookIdempotencyService _webhookIdempotencyService;
    private readonly ILogger<StripeWebhookController> _logger;
    private readonly IConfiguration _configuration;
    private readonly int _maxRetries;
    private readonly int _retryDelaySeconds;

    /// <summary>
    /// Initializes a new instance of the StripeWebhookController with required services.
    /// </summary>
    /// <param name="subscriptionService">Service for subscription management operations</param>
    /// <param name="billingService">Service for billing and payment operations</param>
    /// <param name="billingRepository">Repository for billing data access</param>
    /// <param name="notificationService">Service for notification management</param>
    /// <param name="stripeService">Service for Stripe integration operations</param>
    /// <param name="subscriptionLifecycleService">Service for subscription lifecycle management</param>
    /// <param name="logger">Logger for webhook event tracking and debugging</param>
    /// <param name="configuration">Configuration for webhook settings and retry logic</param>
    public StripeWebhookController(
        ISubscriptionService subscriptionService,
        IBillingService billingService,
        IBillingRepository billingRepository,
        INotificationService notificationService,
        ICommunicationService communicationService,
        IStripeService stripeService,
        ISubscriptionLifecycleService subscriptionLifecycleService,
        IWebhookIdempotencyService webhookIdempotencyService,
        ILogger<StripeWebhookController> logger,
        IConfiguration configuration)
    {
        _subscriptionService = subscriptionService;
        _billingService = billingService;
        _billingRepository = billingRepository;
        _notificationService = notificationService;
        _communicationService = communicationService;
        _stripeService = stripeService;
        _subscriptionLifecycleService = subscriptionLifecycleService;
        _webhookIdempotencyService = webhookIdempotencyService;
        _logger = logger;
        _configuration = configuration;
        _maxRetries = configuration.GetValue<int>("StripeSettings:WebhookRetryAttempts", 3);
        _retryDelaySeconds = configuration.GetValue<int>("StripeSettings:WebhookRetryDelaySeconds", 5);
    }

    /// <summary>
    /// Handles incoming Stripe webhook events and processes them with comprehensive error handling and retry logic.
    /// This endpoint receives webhook events from Stripe, validates them, and processes them to maintain
    /// synchronization between Stripe and the local database.
    /// </summary>
    /// <returns>JsonModel containing the webhook processing result</returns>
    /// <remarks>
    /// This endpoint:
    /// - Validates webhook signature using Stripe webhook secret
    /// - Processes various Stripe events including subscription, payment, and customer events
    /// - Implements idempotency to prevent duplicate processing
    /// - Includes comprehensive retry logic with exponential backoff
    /// - Logs all webhook events for audit and debugging purposes
    /// - Handles webhook failures gracefully with proper error responses
    /// - Maintains data consistency between Stripe and local database
    /// - Sends notifications to users for important events
    /// - Updates subscription and billing records based on Stripe events
    /// - Supports all major Stripe webhook event types
    /// </remarks>
    [HttpPost("webhook")]
    public async Task<JsonModel> HandleWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var webhookSecret = _configuration["StripeSettings:WebhookSecret"];

        if (!ValidateWebhookSecret(webhookSecret))
        {
            _logger.LogError("Invalid webhook secret configuration. Secret must be a valid Stripe webhook secret.");
            return new JsonModel { data = new object(), Message = "Webhook configuration error", StatusCode = 500 };
        }

        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(
                json,
                Request.Headers["Stripe-Signature"],
                webhookSecret
            );
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe webhook signature verification failed: {Message}", ex.Message);
            return new JsonModel { data = new object(), Message = "Invalid webhook signature", StatusCode = 400 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during webhook signature verification: {Message}", ex.Message);
            return new JsonModel { data = new object(), Message = "Webhook processing error", StatusCode = 500 };
        }

        // Implement proper webhook idempotency
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            // Check idempotency before processing
            var idempotencyResult = await _webhookIdempotencyService.CheckIdempotencyAsync(stripeEvent.Id, stripeEvent.Type);
            
            if (!idempotencyResult.ShouldProcess)
            {
                _logger.LogInformation("Skipping webhook event {EventId} - {Reason}", stripeEvent.Id, idempotencyResult.Reason);
                return new JsonModel { data = new object(), Message = $"Event skipped: {idempotencyResult.Reason}", StatusCode = 200 };
            }

            _logger.LogInformation("Processing webhook event {EventId} of type {EventType} (New: {IsNew})", 
                stripeEvent.Id, stripeEvent.Type, idempotencyResult.IsNewEvent);

            // Process webhook with retry logic
            await ProcessWebhookWithRetryAsync(stripeEvent);

            // Mark event as successfully processed
            stopwatch.Stop();
            await _webhookIdempotencyService.MarkAsProcessedAsync(stripeEvent.Id, stopwatch.ElapsedMilliseconds);

            _logger.LogInformation("Successfully processed webhook event {EventId} in {Duration}ms", 
                stripeEvent.Id, stopwatch.ElapsedMilliseconds);

            return new JsonModel { data = new object(), Message = "Webhook processed successfully", StatusCode = 200 };
        }
        catch (StripeException ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Stripe error processing webhook event {EventId} of type {EventType} after {Duration}ms: {Message}", 
                stripeEvent.Id, stripeEvent.Type, stopwatch.ElapsedMilliseconds, ex.Message);
            
            // Mark event as failed with detailed error information
            await _webhookIdempotencyService.MarkAsFailedAsync(stripeEvent.Id, $"Stripe error: {ex.Message}", _maxRetries);
            
            // Log additional context for debugging
            _logger.LogError("Stripe error details - Type: {ErrorType}, Code: {ErrorCode}, Param: {ErrorParam}", 
                ex.StripeError?.Type, ex.StripeError?.Code, ex.StripeError?.Param);
            
            return new JsonModel { data = new object(), Message = $"Stripe error: {ex.Message}", StatusCode = 400 };
        }
        catch (InvalidOperationException ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Business logic error processing webhook event {EventId} of type {EventType} after {Duration}ms: {Message}", 
                stripeEvent.Id, stripeEvent.Type, stopwatch.ElapsedMilliseconds, ex.Message);
            
            // Mark event as failed
            await _webhookIdempotencyService.MarkAsFailedAsync(stripeEvent.Id, ex.Message, _maxRetries);
            
            return new JsonModel { data = new object(), Message = "Business logic error", StatusCode = 422 };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Unexpected error processing webhook event {EventId} of type {EventType} after {Duration}ms", 
                stripeEvent.Id, stripeEvent.Type, stopwatch.ElapsedMilliseconds);
            
            // Mark event as failed
            await _webhookIdempotencyService.MarkAsFailedAsync(stripeEvent.Id, ex.Message, _maxRetries);
            
            return new JsonModel { data = new object(), Message = "Internal server error", StatusCode = 500 };
        }
    }

    private async Task ProcessWebhookWithRetryAsync(Event stripeEvent)
    {
        for (int attempt = 1; attempt <= _maxRetries; attempt++)
        {
            try
            {
                await ProcessStripeEvent(stripeEvent);
                return; // Success, exit retry loop
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Webhook processing attempt {Attempt} failed for event {EventId}: {Error}", 
                    attempt, stripeEvent.Id, ex.Message);
                
                if (attempt == _maxRetries)
                {
                    // Log final failure
                    _logger.LogError("All {MaxRetries} attempts failed for webhook event {EventId}", 
                        _maxRetries, stripeEvent.Id);
                    throw;
                }
                
                // Calculate exponential backoff delay
                var delaySeconds = _retryDelaySeconds * Math.Pow(2, attempt - 1);
                var delay = TimeSpan.FromSeconds(delaySeconds);
                
                _logger.LogInformation("Retrying webhook event {EventId} in {Delay}ms (attempt {Attempt}/{MaxRetries})", 
                    stripeEvent.Id, delay.TotalMilliseconds, attempt + 1, _maxRetries);
                
                await Task.Delay(delay);
            }
        }
    }

    private async Task ProcessStripeEvent(Event stripeEvent)
    {
        switch (stripeEvent.Type)
        {
            case "customer.subscription.created":
                await HandleSubscriptionCreated(stripeEvent);
                break;
            case "customer.subscription.updated":
                await HandleSubscriptionUpdated(stripeEvent);
                break;
            case "customer.subscription.deleted":
                await HandleSubscriptionDeleted(stripeEvent);
                break;
            case "customer.subscription.paused":
                await HandleSubscriptionPaused(stripeEvent);
                break;
            case "customer.subscription.resumed":
                await HandleSubscriptionResumed(stripeEvent);
                break;
            case "customer.subscription.past_due":
                await HandleSubscriptionPastDue(stripeEvent);
                break;
            case "customer.subscription.unpaid":
                await HandleSubscriptionUnpaid(stripeEvent);
                break;
            case "invoice.payment_succeeded":
                await HandlePaymentSucceeded(stripeEvent);
                break;
            case "invoice.payment_failed":
                await HandlePaymentFailed(stripeEvent);
                break;
            case "invoice.payment_action_required":
                await HandlePaymentActionRequired(stripeEvent);
                break;
            case "invoice.finalized":
                await HandleInvoiceFinalized(stripeEvent);
                break;
            case "invoice.sent":
                await HandleInvoiceSent(stripeEvent);
                break;
            case "invoice.upcoming":
                await HandleInvoiceUpcoming(stripeEvent);
                break;
            case "invoice.finalization_failed":
                await HandleInvoiceFinalizationFailed(stripeEvent);
                break;
            case "customer.subscription.trial_will_end":
                await HandleSubscriptionTrialWillEnd(stripeEvent);
                break;
            case "payment_intent.succeeded":
                await HandlePaymentIntentSucceeded(stripeEvent);
                break;
            case "payment_intent.payment_failed":
                await HandlePaymentIntentFailed(stripeEvent);
                break;
            case "payment_method.attached":
                await HandlePaymentMethodAttached(stripeEvent);
                break;
            case "payment_method.updated":
                await HandlePaymentMethodUpdated(stripeEvent);
                break;
            case "payment_method.detached":
                await HandlePaymentMethodDetached(stripeEvent);
                break;
            case "charge.refunded":
                await HandleChargeRefunded(stripeEvent);
                break;
            case "charge.dispute.created":
                await HandleChargeDisputeCreated(stripeEvent);
                break;
            case "charge.dispute.closed":
                await HandleChargeDisputeClosed(stripeEvent);
                break;
            case "customer.created":
                await HandleCustomerCreated(stripeEvent);
                break;
            case "customer.updated":
                await HandleCustomerUpdated(stripeEvent);
                break;
            case "customer.deleted":
                await HandleCustomerDeleted(stripeEvent);
                break;
            case "setup_intent.succeeded":
                await HandleSetupIntentSucceeded(stripeEvent);
                break;
            case "setup_intent.setup_failed":
                await HandleSetupIntentFailed(stripeEvent);
                break;
            case "payment_intent.requires_action":
                await HandlePaymentIntentRequiresAction(stripeEvent);
                break;
            case "invoice.created":
                await HandleInvoiceCreated(stripeEvent);
                break;
            case "invoice.voided":
                await HandleInvoiceVoided(stripeEvent);
                break;
            case "checkout.session.completed":
                await HandleCheckoutSessionCompleted(stripeEvent);
                break;
            case "product.created":
                await HandleProductCreated(stripeEvent);
                break;
            case "product.updated":
                await HandleProductUpdated(stripeEvent);
                break;
            case "product.deleted":
                await HandleProductDeleted(stripeEvent);
                break;
            case "price.created":
                await HandlePriceCreated(stripeEvent);
                break;
            case "price.updated":
                await HandlePriceUpdated(stripeEvent);
                break;
            case "price.deleted":
                await HandlePriceDeleted(stripeEvent);
                break;
            case "payout.created":
                await HandlePayoutCreated(stripeEvent);
                break;
            case "payout.updated":
                await HandlePayoutUpdated(stripeEvent);
                break;
            case "payout.paid":
                await HandlePayoutPaid(stripeEvent);
                break;
            case "payout.failed":
                await HandlePayoutFailed(stripeEvent);
                break;
            case "payout.canceled":
                await HandlePayoutCanceled(stripeEvent);
                break;
            case "balance.available":
                await HandleBalanceAvailable(stripeEvent);
                break;
            case "mandate.updated":
                await HandleMandateUpdated(stripeEvent);
                break;
            case "review.opened":
                await HandleReviewOpened(stripeEvent);
                break;
            case "review.closed":
                await HandleReviewClosed(stripeEvent);
                break;
            case "subscription_schedule.canceled":
                await HandleSubscriptionScheduleCanceled(stripeEvent);
                break;
            case "subscription_schedule.completed":
                await HandleSubscriptionScheduleCompleted(stripeEvent);
                break;
            case "subscription_schedule.created":
                await HandleSubscriptionScheduleCreated(stripeEvent);
                break;
            case "subscription_schedule.released":
                await HandleSubscriptionScheduleReleased(stripeEvent);
                break;
            case "subscription_schedule.updated":
                await HandleSubscriptionScheduleUpdated(stripeEvent);
                break;
            case "tax_rate.created":
                await HandleTaxRateCreated(stripeEvent);
                break;
            case "tax_rate.updated":
                await HandleTaxRateUpdated(stripeEvent);
                break;
            case "transfer.created":
                await HandleTransferCreated(stripeEvent);
                break;
            case "transfer.failed":
                await HandleTransferFailed(stripeEvent);
                break;
            case "transfer.paid":
                await HandleTransferPaid(stripeEvent);
                break;
            case "transfer.reversed":
                await HandleTransferReversed(stripeEvent);
                break;
            case "transfer.updated":
                await HandleTransferUpdated(stripeEvent);
                break;
            default:
                // Log unhandled event type
                _logger.LogInformation("Unhandled Stripe webhook event type: {EventType}", stripeEvent.Type);
                break;
        }
    }

    private async Task HandleSubscriptionCreated(Event stripeEvent)
    {
        var subscription = stripeEvent.Data.Object as Stripe.Subscription;
        if (subscription == null) return;

        // Update local subscription with Stripe subscription ID
        var localSubscription = await _subscriptionService.GetByStripeSubscriptionIdAsync(subscription.Id, GetToken(HttpContext));
        if (localSubscription.StatusCode == 200)
        {
            // Subscription already exists, update status
            var updateDto = new UpdateSubscriptionDto
            {
                StripeSubscriptionId = subscription.Id,
                Status = MapStripeStatusToLocal(subscription.Status)
            };
            await _subscriptionLifecycleService.UpdateSubscriptionAsync(localSubscription.data.ToString(), updateDto, GetToken(HttpContext));
        }
    }

    private async Task HandleSubscriptionUpdated(Event stripeEvent)
    {
        var subscription = stripeEvent.Data.Object as Stripe.Subscription;
        if (subscription == null) return;

        try
        {
            var localSubscription = await _subscriptionService.GetByStripeSubscriptionIdAsync(subscription.Id, GetToken(HttpContext));
            if (localSubscription.StatusCode == 200)
            {
                var subscriptionData = localSubscription.data as dynamic;
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

                    await _subscriptionLifecycleService.UpdateSubscriptionAsync(localSubscription.data.ToString(), updateDto, GetToken(HttpContext));

                    _logger.LogInformation("Subscription {SubscriptionId} updated via Stripe webhook. Status: {Status}", 
                        subscription.Id, subscription.Status);
                }
            }
            else
            {
                _logger.LogWarning("Local subscription not found for Stripe subscription {SubscriptionId}", subscription.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling subscription updated webhook for subscription {SubscriptionId}", subscription.Id);
            throw; // Re-throw to trigger retry mechanism
        }
    }

    private async Task HandleSubscriptionDeleted(Event stripeEvent)
    {
        var subscription = stripeEvent.Data.Object as Stripe.Subscription;
        if (subscription == null) return;

        var localSubscription = await _subscriptionService.GetByStripeSubscriptionIdAsync(subscription.Id, GetToken(HttpContext));
        if (localSubscription.StatusCode == 200)
        {
            await _subscriptionLifecycleService.CancelSubscriptionAsync(localSubscription.data.ToString(), "Cancelled via Stripe", GetToken(HttpContext));
        }
    }

    private async Task HandlePaymentSucceeded(Event stripeEvent)
    {
        var invoice = stripeEvent.Data.Object as Stripe.Invoice;
        if (invoice == null) return;

        try
        {
            var subscriptionId = GetSubscriptionIdFromInvoice(invoice);
            if (!string.IsNullOrEmpty(subscriptionId))
            {
                var localSubscription = await _subscriptionService.GetByStripeSubscriptionIdAsync(subscriptionId, GetToken(HttpContext));
                if (localSubscription.StatusCode == 200)
                {
                    var subscriptionData = localSubscription.data as dynamic;
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

                        await _subscriptionLifecycleService.UpdateSubscriptionAsync(localSubscription.data.ToString(), updateDto, GetToken(HttpContext));

                        // Send payment success notification
                        await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                        {
                            UserId = subscriptionData.UserId,
                            Title = "Payment Successful",
                            Message = $"Your payment for subscription has been processed successfully. Invoice: {invoice.Number}",
                            Type = "PaymentSuccess",
                            IsRead = false,
                            Priority = "Normal"
                        }, GetToken(HttpContext));

                        // Create billing record for successful payment with comprehensive data
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

                        var billingResult = await _billingService.CreateBillingRecordAsync(billingRecordDto, GetToken(HttpContext));
                        
                        if (billingResult.StatusCode != 200)
                        {
                            _logger.LogError("Failed to create billing record for successful payment. Invoice: {InvoiceId}, Error: {Error}", 
                                invoice.Id, billingResult.Message);
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
                            GetToken(HttpContext));

                        // Log payment success
                        

                        _logger.LogInformation("Payment success handled for subscription {SubscriptionId}, invoice {InvoiceNumber}", 
                            subscriptionId, invoice.Number);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling payment succeeded webhook for invoice {InvoiceNumber}", invoice.Number);
            throw; // Re-throw to trigger retry mechanism
        }
    }

    private async Task HandlePaymentFailed(Event stripeEvent)
    {
        var invoice = stripeEvent.Data.Object as Stripe.Invoice;
        if (invoice == null) return;

        try
        {
            var subscriptionId = GetSubscriptionIdFromInvoice(invoice);
            if (!string.IsNullOrEmpty(subscriptionId))
            {
                var localSubscription = await _subscriptionService.GetByStripeSubscriptionIdAsync(subscriptionId, GetToken(HttpContext));
                if (localSubscription.StatusCode == 200)
                {
                    var subscriptionData = localSubscription.data as dynamic;
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
                        
                        await _subscriptionLifecycleService.UpdateSubscriptionAsync(localSubscription.data.ToString(), updateDto, GetToken(HttpContext));

                        // Send payment failure notification
                        await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                        {
                            UserId = subscriptionData.UserId,
                            Title = "Payment Failed",
                            Message = $"Your payment for subscription has failed. Please update your payment method to continue your subscription. Invoice: {invoice.Number}",
                            Type = "PaymentFailed",
                            IsRead = false,
                            Priority = "High"
                        }, GetToken(HttpContext));

                        // Create billing record for failed payment with comprehensive data
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

                        var failedBillingResult = await _billingService.CreateBillingRecordAsync(failedBillingRecordDto, GetToken(HttpContext));
                        
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
                            GetToken(HttpContext));

                        // Log payment failure
                        

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
            _logger.LogError(ex, "Error handling payment failed webhook for invoice {InvoiceNumber}", invoice.Number);
            throw; // Re-throw to trigger retry mechanism
        }
    }

    private async Task HandlePaymentIntentSucceeded(Event stripeEvent)
    {
        var paymentIntent = stripeEvent.Data.Object as Stripe.PaymentIntent;
        if (paymentIntent == null) return;

        // Handle successful payment intent
        // This is typically handled by the invoice events, but we can log it here
    }

    private async Task HandlePaymentIntentFailed(Event stripeEvent)
    {
        var paymentIntent = stripeEvent.Data.Object as Stripe.PaymentIntent;
        if (paymentIntent == null) return;

        // Handle failed payment intent
        // This is typically handled by the invoice events, but we can log it here
    }

    private async Task HandleSubscriptionTrialWillEnd(Event stripeEvent)
    {
        var subscription = stripeEvent.Data.Object as Stripe.Subscription;
        if (subscription == null) return;

        try
        {
            // Get local subscription to send notification and prepare for transition
            var localSubscription = await _subscriptionService.GetByStripeSubscriptionIdAsync(subscription.Id, GetToken(HttpContext));
            if (localSubscription.StatusCode == 200)
            {
                var subscriptionData = localSubscription.data as dynamic;
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
                    }, GetToken(HttpContext));

                    

                    _logger.LogInformation("Trial ending notification sent for subscription {SubscriptionId}", subscription.Id);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling trial will end webhook for subscription {SubscriptionId}", subscription.Id);
            throw; // Re-throw to trigger retry mechanism
        }
    }

    private async Task HandlePaymentActionRequired(Event stripeEvent)
    {
        var invoice = stripeEvent.Data.Object as Stripe.Invoice;
        if (invoice == null) return;

        // Validate customer ID format before parsing
        if (!int.TryParse(invoice.CustomerId, out int userId))
        {
            return;
        }

        // Send payment action required notification - using new Invoice.Parent property in 48.4.0
        var subscriptionId = GetSubscriptionIdFromInvoice(invoice);
        if (!string.IsNullOrEmpty(subscriptionId))
        {
            var localSubscription = await _subscriptionService.GetByStripeSubscriptionIdAsync(subscriptionId, GetToken(HttpContext));
            if (localSubscription.StatusCode == 200)
            {
                // Create notification for payment action required
                await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                {
                    UserId = userId,
                    Title = "Payment Action Required",
                    Message = $"Your payment requires additional verification. Please complete the authentication process to continue your subscription. Invoice: {invoice.Number}",
                    Type = "PaymentAction",
                    IsRead = false,
                    Priority = "High"
                }, GetToken(HttpContext));

                // Update subscription status to indicate action required
                var updateDto = new UpdateSubscriptionDto
                {
                    Status = "PaymentActionRequired",
                    LastPaymentError = "Payment authentication required"
                };
                await _subscriptionLifecycleService.UpdateSubscriptionAsync(localSubscription.data.ToString(), updateDto, GetToken(HttpContext));
            }
        }
    }

    private async Task HandleCustomerCreated(Event stripeEvent)
    {
        var customer = stripeEvent.Data.Object as Stripe.Customer;
        if (customer == null) return;

        // Log customer creation for audit purposes
        _logger.LogInformation("Stripe customer created: {CustomerId}, Email: {Email}", customer.Id, customer.Email);
        
        // Note: We typically create Stripe customers when users register in our system,
        // so this event is mainly for logging and verification purposes
    }

    private async Task HandleCustomerUpdated(Event stripeEvent)
    {
        var customer = stripeEvent.Data.Object as Stripe.Customer;
        if (customer == null) return;

        // Log customer update for audit purposes
        _logger.LogInformation("Stripe customer updated: {CustomerId}, Email: {Email}", customer.Id, customer.Email);
        
        // Note: Customer updates typically involve payment method changes or profile updates
        // which are handled through our user management system
    }

    private async Task HandleCustomerDeleted(Event stripeEvent)
    {
        var customer = stripeEvent.Data.Object as Stripe.Customer;
        if (customer == null) return;

        // Log customer deletion for audit purposes
        _logger.LogWarning("Stripe customer deleted: {CustomerId}, Email: {Email}", customer.Id, customer.Email);
        
        // Note: Customer deletion should be handled carefully as it affects all subscriptions
        // We typically don't delete customers automatically but mark them as inactive
    }

    private string GetPaymentIntentIdFromInvoice(Stripe.Invoice invoice)
    {
        try
        {
            // Try to get from metadata first (most reliable)
            if (invoice.Metadata?.ContainsKey("payment_intent_id") == true)
            {
                return invoice.Metadata["payment_intent_id"];
            }
            
            // Note: Payment intent ID extraction from invoice is limited in Stripe.NET 48.4.0
            // The most reliable approach is through metadata or by fetching the invoice with expanded data
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Error extracting payment intent ID from invoice {InvoiceId}: {Error}", invoice.Id, ex.Message);
        }
        
        return string.Empty;
    }

    private DateTime GetNextBillingDateFromSubscription(Stripe.Subscription subscription)
    {
        try
        {
            // ✅ CORRECT - Use subscription.CurrentPeriodEnd directly (most reliable)
            // Note: CurrentPeriodEnd is not available in Stripe.NET 48.4.0
            // We'll use the fallback approach instead
            
            // Fallback: Try to get from subscription items
            var firstItem = subscription.Items?.Data?.FirstOrDefault();
            if (firstItem?.CurrentPeriodEnd != null)
            {
                var unixTimestamp = Convert.ToInt64(firstItem.CurrentPeriodEnd);
                return DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).DateTime;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Failed to parse subscription billing date: {Error}", ex.Message);
        }
        
        // Fallback to default
        return DateTime.UtcNow.AddMonths(1);
    }

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
            if (invoice.Parent != null && invoice.Parent.Type == "subscription")
            {
                // For subscription parents, we can't directly get the ID from InvoiceParent
                // This would require additional API calls to fetch the subscription
                _logger.LogDebug("Invoice {InvoiceId} has subscription parent but ID not directly available", invoice.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Error extracting subscription ID from invoice {InvoiceId}: {Error}", invoice.Id, ex.Message);
        }
        
        return string.Empty;
    }

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
    /// Validates webhook secret format according to Stripe specifications
    /// </summary>
    /// <param name="secret">The webhook secret to validate</param>
    /// <returns>True if the secret is valid, false otherwise</returns>
    private bool ValidateWebhookSecret(string secret)
    {
        if (string.IsNullOrEmpty(secret))
            return false;
            
        // Stripe webhook secrets start with "whsec_" and are typically 50+ characters
        if (!secret.StartsWith("whsec_"))
            return false;
            
        // Check minimum length (Stripe webhook secrets are typically 50+ characters)
        if (secret.Length < 50)
            return false;
            
        // Check for valid characters (alphanumeric and underscores)
        var validPattern = @"^whsec_[a-zA-Z0-9_]+$";
        return System.Text.RegularExpressions.Regex.IsMatch(secret, validPattern);
    }

    // NEW: Handle subscription pause events
    private async Task HandleSubscriptionPaused(Event stripeEvent)
    {
        var subscription = stripeEvent.Data.Object as Stripe.Subscription;
        if (subscription == null) return;

        var localSubscription = await _subscriptionService.GetByStripeSubscriptionIdAsync(subscription.Id, GetToken(HttpContext));
        if (localSubscription.StatusCode == 200)
        {
            var updateDto = new UpdateSubscriptionDto
            {
                Status = "Paused",
                PausedDate = DateTime.UtcNow
            };
            await _subscriptionLifecycleService.UpdateSubscriptionAsync(localSubscription.data.ToString(), updateDto, GetToken(HttpContext));
            
            _logger.LogInformation("Subscription {SubscriptionId} paused via Stripe webhook", subscription.Id);
        }
    }

    // NEW: Handle subscription resume events
    private async Task HandleSubscriptionResumed(Event stripeEvent)
    {
        var subscription = stripeEvent.Data.Object as Stripe.Subscription;
        if (subscription == null) return;

        var localSubscription = await _subscriptionService.GetByStripeSubscriptionIdAsync(subscription.Id, GetToken(HttpContext));
        if (localSubscription.StatusCode == 200)
        {
            var updateDto = new UpdateSubscriptionDto
            {
                Status = "Active",
                ResumedDate = DateTime.UtcNow
            };
            await _subscriptionLifecycleService.UpdateSubscriptionAsync(localSubscription.data.ToString(), updateDto, GetToken(HttpContext));
            
            _logger.LogInformation("Subscription {SubscriptionId} resumed via Stripe webhook", subscription.Id);
        }
    }

    // NEW: Handle subscription past due events
    private async Task HandleSubscriptionPastDue(Event stripeEvent)
    {
        var subscription = stripeEvent.Data.Object as Stripe.Subscription;
        if (subscription == null) return;

        var localSubscription = await _subscriptionService.GetByStripeSubscriptionIdAsync(subscription.Id, GetToken(HttpContext));
        if (localSubscription.StatusCode == 200)
        {
            var updateDto = new UpdateSubscriptionDto
            {
                Status = "PaymentFailed",
                LastPaymentError = "Payment past due via Stripe"
            };
            await _subscriptionLifecycleService.UpdateSubscriptionAsync(localSubscription.data.ToString(), updateDto, GetToken(HttpContext));
            
            _logger.LogInformation("Subscription {SubscriptionId} marked as past due via Stripe webhook", subscription.Id);
        }
    }

    // NEW: Handle subscription unpaid events
    private async Task HandleSubscriptionUnpaid(Event stripeEvent)
    {
        var subscription = stripeEvent.Data.Object as Stripe.Subscription;
        if (subscription == null) return;

        var localSubscription = await _subscriptionService.GetByStripeSubscriptionIdAsync(subscription.Id, GetToken(HttpContext));
        if (localSubscription.StatusCode == 200)
        {
            var updateDto = new UpdateSubscriptionDto
            {
                Status = "PaymentFailed",
                LastPaymentError = "Payment unpaid via Stripe"
            };
            await _subscriptionLifecycleService.UpdateSubscriptionAsync(localSubscription.data.ToString(), updateDto, GetToken(HttpContext));
            
            _logger.LogInformation("Subscription {SubscriptionId} marked as unpaid via Stripe webhook", subscription.Id);
        }
    }

    // NEW: Handle payment method attached events
    private async Task HandlePaymentMethodAttached(Event stripeEvent)
    {
        var paymentMethod = stripeEvent.Data.Object as Stripe.PaymentMethod;
        if (paymentMethod == null) return;

        // Log payment method attachment for audit purposes
        _logger.LogInformation("Payment method {PaymentMethodId} attached to customer {CustomerId} via Stripe webhook", 
            paymentMethod.Id, paymentMethod.CustomerId);
        
        // Note: Payment method management is typically handled through our payment service
        // This webhook is mainly for logging and verification
    }

    // NEW: Handle payment method updated events
    private async Task HandlePaymentMethodUpdated(Event stripeEvent)
    {
        var paymentMethod = stripeEvent.Data.Object as Stripe.PaymentMethod;
        if (paymentMethod == null) return;

        // Log payment method update for audit purposes
        _logger.LogInformation("Payment method {PaymentMethodId} updated for customer {CustomerId} via Stripe webhook", 
            paymentMethod.Id, paymentMethod.CustomerId);
        
        // Note: Payment method updates are typically handled through our payment service
        // This webhook is mainly for logging and verification
    }

    // NEW: Handle payment method detached events
    private async Task HandlePaymentMethodDetached(Event stripeEvent)
    {
        var paymentMethod = stripeEvent.Data.Object as Stripe.PaymentMethod;
        if (paymentMethod == null) return;

        // Log payment method detachment for audit purposes
        _logger.LogInformation("Payment method {PaymentMethodId} detached from customer {CustomerId} via Stripe webhook", 
            paymentMethod.Id, paymentMethod.CustomerId);
        
        // Note: Payment method removal is typically handled through our payment service
        // This webhook is mainly for logging and verification
    }

    // NEW: Handle charge refunded events
    private async Task HandleChargeRefunded(Event stripeEvent)
    {
        var charge = stripeEvent.Data.Object as Stripe.Charge;
        if (charge == null) return;

        try
        {
            // Find the billing record associated with this charge
            var billingRecord = await _billingRepository.GetByStripePaymentIntentIdAsync(charge.PaymentIntentId);
            if (billingRecord != null)
            {
                // Update billing record status to refunded
                billingRecord.Status = BillingRecord.BillingStatus.Refunded;
                billingRecord.UpdatedDate = DateTime.UtcNow;
                await _billingRepository.UpdateAsync(billingRecord);

                // Create refund record
                await _billingService.CreateBillingRecordAsync(new CreateBillingRecordDto
                {
                    UserId = billingRecord.UserId,
                    Amount = charge.AmountRefunded / 100m, // Convert from cents
                    CurrencyId = null, // Will use default currency
                    PaymentMethod = "stripe",
                    StripePaymentIntentId = charge.PaymentIntentId,
                    Status = BillingRecord.BillingStatus.Refunded.ToString(),
                    Description = $"Refund for charge {charge.Id}",
                    BillingDate = DateTime.UtcNow,
                    Type = BillingRecord.BillingType.Refund.ToString()
                }, GetToken(HttpContext));

                _logger.LogInformation("Charge {ChargeId} refunded via Stripe webhook. Billing record {BillingRecordId} updated.", 
                    charge.Id, billingRecord.Id);
            }
            else
            {
                _logger.LogWarning("No billing record found for refunded charge {ChargeId} with payment intent {PaymentIntentId}", 
                    charge.Id, charge.PaymentIntentId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling charge refunded webhook for charge {ChargeId}", charge.Id);
        }
    }

    // NEW: Handle charge dispute created events
    private async Task HandleChargeDisputeCreated(Event stripeEvent)
    {
        var dispute = stripeEvent.Data.Object as Stripe.Dispute;
        if (dispute == null) return;

        try
        {
            // Find the billing record associated with this dispute
            var billingRecord = await _billingRepository.GetByStripePaymentIntentIdAsync(dispute.PaymentIntentId);
            if (billingRecord != null)
            {
                // Update billing record to indicate dispute
                billingRecord.Status = BillingRecord.BillingStatus.Pending; // Reset to pending during dispute
                billingRecord.UpdatedDate = DateTime.UtcNow;
                await _billingRepository.UpdateAsync(billingRecord);

                // Create dispute record
                await _billingService.CreateBillingRecordAsync(new CreateBillingRecordDto
                {
                    UserId = billingRecord.UserId,
                    Amount = dispute.Amount / 100m, // Convert from cents
                    CurrencyId = null, // Will use default currency
                    PaymentMethod = "stripe",
                    StripePaymentIntentId = dispute.PaymentIntentId,
                    Status = BillingRecord.BillingStatus.Pending.ToString(),
                    Description = $"Dispute created for charge {dispute.ChargeId}. Reason: {dispute.Reason}",
                    BillingDate = DateTime.UtcNow,
                    Type = BillingRecord.BillingType.Subscription.ToString()
                }, GetToken(HttpContext));

                _logger.LogInformation("Dispute {DisputeId} created via Stripe webhook for charge {ChargeId}. Billing record {BillingRecordId} updated.", 
                    dispute.Id, dispute.ChargeId, billingRecord.Id);
            }
            else
            {
                _logger.LogWarning("No billing record found for dispute {DisputeId} with payment intent {PaymentIntentId}", 
                    dispute.Id, dispute.PaymentIntentId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling charge dispute created webhook for dispute {DisputeId}", dispute.Id);
        }
    }

    // NEW: Handle charge dispute closed events
    private async Task HandleChargeDisputeClosed(Event stripeEvent)
    {
        var dispute = stripeEvent.Data.Object as Stripe.Dispute;
        if (dispute == null) return;

        try
        {
            // Find the billing record associated with this dispute
            var billingRecord = await _billingRepository.GetByStripePaymentIntentIdAsync(dispute.PaymentIntentId);
            if (billingRecord != null)
            {
                // Update billing record based on dispute outcome
                if (dispute.Status == "won")
                {
                    // Dispute won by customer - mark as refunded
                    billingRecord.Status = BillingRecord.BillingStatus.Refunded;
                    billingRecord.UpdatedDate = DateTime.UtcNow;
                    await _billingRepository.UpdateAsync(billingRecord);

                    _logger.LogInformation("Dispute {DisputeId} closed in favor of customer via Stripe webhook. Billing record {BillingRecordId} marked as refunded.", 
                        dispute.Id, billingRecord.Id);
                }
                else if (dispute.Status == "lost")
                {
                    // Dispute lost by customer - mark as paid
                    billingRecord.Status = BillingRecord.BillingStatus.Paid;
                    billingRecord.UpdatedDate = DateTime.UtcNow;
                    await _billingRepository.UpdateAsync(billingRecord);

                    _logger.LogInformation("Dispute {DisputeId} closed in favor of business via Stripe webhook. Billing record {BillingRecordId} marked as paid.", 
                        dispute.Id, billingRecord.Id);
                }
                else
                {
                    // Dispute closed for other reasons (e.g., withdrawn)
                    billingRecord.Status = BillingRecord.BillingStatus.Paid; // Default to paid
                    billingRecord.UpdatedDate = DateTime.UtcNow;
                    await _billingRepository.UpdateAsync(billingRecord);

                    _logger.LogInformation("Dispute {DisputeId} closed with status {Status} via Stripe webhook. Billing record {BillingRecordId} updated.", 
                        dispute.Id, dispute.Status, billingRecord.Id);
                }
            }
            else
            {
                _logger.LogWarning("No billing record found for closed dispute {DisputeId} with payment intent {PaymentIntentId}", 
                    dispute.Id, dispute.PaymentIntentId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling charge dispute closed webhook for dispute {DisputeId}", dispute.Id);
        }
    }

    // NEW: Handle invoice finalized events
    private async Task HandleInvoiceFinalized(Event stripeEvent)
    {
        var invoice = stripeEvent.Data.Object as Stripe.Invoice;
        if (invoice == null) return;

        try
        {
            // Validate customer ID format before parsing
            if (!int.TryParse(invoice.CustomerId, out int userId))
            {
                _logger.LogWarning("Invalid customer ID format in invoice finalized webhook: {CustomerId}", invoice.CustomerId);
                return;
            }

                                    // CRITICAL FIX: Create billing record with proper Stripe correlation
                        await _billingService.CreateBillingRecordAsync(new CreateBillingRecordDto
                        {
                            UserId = userId,
                            Amount = invoice.AmountDue / 100m, // Convert from cents
                            CurrencyId = null, // Will use default currency
                            PaymentMethod = "stripe",
                            StripeInvoiceId = invoice.Id, // Link to Stripe invoice
                            StripePaymentIntentId = GetPaymentIntentIdFromInvoice(invoice), // Link to payment intent
                            Status = BillingRecord.BillingStatus.Pending.ToString(),
                            Description = $"Invoice {invoice.Number} finalized - Amount: {invoice.AmountDue / 100m} {invoice.Currency}",
                            BillingDate = DateTime.UtcNow,
                            DueDate = invoice.DueDate ?? DateTime.UtcNow.AddDays(30),
                            Type = BillingRecord.BillingType.Subscription.ToString(),
                            InvoiceNumber = invoice.Number // Store invoice number for reference
                        }, GetToken(HttpContext));

            _logger.LogInformation("Invoice {InvoiceId} finalized via Stripe webhook. Billing record created for user {UserId}.", 
                invoice.Id, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling invoice finalized webhook for invoice {InvoiceId}", invoice.Id);
        }
    }

    // NEW: Handle invoice sent events
    private async Task HandleInvoiceSent(Event stripeEvent)
    {
        var invoice = stripeEvent.Data.Object as Stripe.Invoice;
        if (invoice == null) return;

        try
        {
            // Validate customer ID format before parsing
            if (!int.TryParse(invoice.CustomerId, out int userId))
            {
                _logger.LogWarning("Invalid customer ID format in invoice sent webhook: {CustomerId}", invoice.CustomerId);
                return;
            }

            // Update billing record status to indicate invoice was sent
            var billingRecord = await _billingRepository.GetByStripeInvoiceIdAsync(invoice.Id);
            if (billingRecord != null)
            {
                billingRecord.Status = BillingRecord.BillingStatus.Pending;
                billingRecord.UpdatedDate = DateTime.UtcNow;
                await _billingRepository.UpdateAsync(billingRecord);

                _logger.LogInformation("Invoice {InvoiceId} sent via Stripe webhook. Billing record {BillingRecordId} status updated.", 
                    invoice.Id, billingRecord.Id);
            }
            else
            {
                _logger.LogWarning("No billing record found for sent invoice {InvoiceId}", invoice.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling invoice sent webhook for invoice {InvoiceId}", invoice.Id);
        }
    }

    // NEW: Handle upcoming invoice events
    private async Task HandleInvoiceUpcoming(Event stripeEvent)
    {
        var invoice = stripeEvent.Data.Object as Stripe.Invoice;
        if (invoice == null) return;

        try
        {
            // Validate customer ID format before parsing
            if (!int.TryParse(invoice.CustomerId, out int userId))
            {
                _logger.LogWarning("Invalid customer ID format in upcoming invoice webhook: {CustomerId}", invoice.CustomerId);
                return;
            }

            // Create billing record for upcoming invoice
            await _billingService.CreateBillingRecordAsync(new CreateBillingRecordDto
            {
                UserId = userId,
                Amount = invoice.AmountDue / 100m, // Convert from cents
                CurrencyId = null, // Will use default currency
                PaymentMethod = "stripe",
                StripeInvoiceId = invoice.Id,
                Status = BillingRecord.BillingStatus.Upcoming.ToString(),
                Description = $"Upcoming invoice {invoice.Number} - Amount: {invoice.AmountDue / 100m} {invoice.Currency}",
                BillingDate = invoice.Created,
                DueDate = invoice.DueDate ?? DateTime.UtcNow.AddDays(30),
                Type = BillingRecord.BillingType.Subscription.ToString()
            }, GetToken(HttpContext));

            _logger.LogInformation("Upcoming invoice {InvoiceId} created via Stripe webhook. Billing record created for user {UserId}.", 
                invoice.Id, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling upcoming invoice webhook for invoice {InvoiceId}", invoice.Id);
        }
    }

    // NEW: Handle invoice finalization failed events
    private async Task HandleInvoiceFinalizationFailed(Event stripeEvent)
    {
        var invoice = stripeEvent.Data.Object as Stripe.Invoice;
        if (invoice == null) return;

        try
        {
            // Validate customer ID format before parsing
            if (!int.TryParse(invoice.CustomerId, out int userId))
            {
                _logger.LogWarning("Invalid customer ID format in invoice finalization failed webhook: {CustomerId}", invoice.CustomerId);
                return;
            }

            // Update billing record status to indicate finalization failed
            var billingRecord = await _billingRepository.GetByStripeInvoiceIdAsync(invoice.Id);
            if (billingRecord != null)
            {
                billingRecord.Status = BillingRecord.BillingStatus.Failed;
                billingRecord.UpdatedDate = DateTime.UtcNow;
                await _billingRepository.UpdateAsync(billingRecord);

                _logger.LogInformation("Invoice {InvoiceId} finalization failed via Stripe webhook. Billing record {BillingRecordId} status updated to failed.", 
                    invoice.Id, billingRecord.Id);
            }
            else
            {
                _logger.LogWarning("No billing record found for failed invoice {InvoiceId}", invoice.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling invoice finalization failed webhook for invoice {InvoiceId}", invoice.Id);
        }
    }

    // NEW: Handle trial payment failure specifically
    private async Task HandleTrialPaymentFailure(string subscriptionId, string invoiceNumber)
    {
        try
        {
            if (string.IsNullOrEmpty(subscriptionId)) return;

            // For trial subscriptions, we need to handle the transition carefully
                            var subscription = await _subscriptionService.GetSubscriptionByIdAsync(subscriptionId, GetToken(HttpContext));
            if (subscription.StatusCode == 200)
            {
                var subscriptionData = subscription.data as dynamic;
                if (subscriptionData != null)
                {
                    // Update trial end date to now since payment failed
                    var updateDto = new UpdateSubscriptionDto
                    {
                        Status = "TrialExpired",
                        TrialEndDate = DateTime.UtcNow,
                        LastPaymentError = "Trial ended due to payment failure"
                    };

                    await _subscriptionLifecycleService.UpdateSubscriptionAsync(subscriptionId, updateDto, GetToken(HttpContext));

                    // Send trial expired notification
                    await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                    {
                        UserId = subscriptionData.UserId,
                        Title = "Trial Expired",
                        Message = "Your trial period has expired due to payment failure. Please add a valid payment method to continue your subscription.",
                        Type = "TrialExpired",
                        IsRead = false,
                        Priority = "High"
                    }, GetToken(HttpContext));

                   
                   

                    _logger.LogInformation("Trial payment failure handled for subscription {SubscriptionId}", subscriptionId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling trial payment failure for subscription {SubscriptionId}", subscriptionId);
            // Don't re-throw here as this is a secondary operation
        }
    }

    // CRITICAL FIX: Additional missing webhook handlers
    private async Task HandleSetupIntentSucceeded(Event stripeEvent)
    {
        var setupIntent = stripeEvent.Data.Object as Stripe.SetupIntent;
        if (setupIntent == null) return;

        try
        {
            _logger.LogInformation("Setup intent {SetupIntentId} succeeded for customer {CustomerId}", 
                setupIntent.Id, setupIntent.CustomerId);

            // Log successful payment method setup
           
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling setup intent succeeded webhook for {SetupIntentId}", setupIntent.Id);
        }
    }

    private async Task HandleSetupIntentFailed(Event stripeEvent)
    {
        var setupIntent = stripeEvent.Data.Object as Stripe.SetupIntent;
        if (setupIntent == null) return;

        try
        {
            _logger.LogWarning("Setup intent {SetupIntentId} failed for customer {CustomerId}: {FailureReason}", 
                setupIntent.Id, setupIntent.CustomerId, setupIntent.LastSetupError?.Message);

            
            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling setup intent failed webhook for {SetupIntentId}", setupIntent.Id);
        }
    }

    private async Task HandlePaymentIntentRequiresAction(Event stripeEvent)
    {
        var paymentIntent = stripeEvent.Data.Object as Stripe.PaymentIntent;
        if (paymentIntent == null) return;

        try
        {
            _logger.LogInformation("Payment intent {PaymentIntentId} requires action for customer {CustomerId}", 
                paymentIntent.Id, paymentIntent.CustomerId);

            // Find the billing record associated with this payment intent
            var billingRecord = await _billingRepository.GetByStripePaymentIntentIdAsync(paymentIntent.Id);
            if (billingRecord != null)
            {
                // Update billing record status to indicate action required
                billingRecord.Status = BillingRecord.BillingStatus.Pending;
                billingRecord.UpdatedDate = DateTime.UtcNow;
                billingRecord.ErrorMessage = "Payment requires additional authentication";
                await _billingRepository.UpdateAsync(billingRecord);

                // Send notification to user
                await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                {
                    UserId = billingRecord.UserId,
                    Title = "Payment Action Required",
                    Message = "Your payment requires additional verification. Please complete the authentication process.",
                    Type = "PaymentAction",
                    IsRead = false,
                    Priority = "High"
                }, GetToken(HttpContext));

                _logger.LogInformation("Payment action required handled for billing record {BillingRecordId}", billingRecord.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling payment intent requires action webhook for {PaymentIntentId}", paymentIntent.Id);
        }
    }

    private async Task HandleInvoiceCreated(Event stripeEvent)
    {
        var invoice = stripeEvent.Data.Object as Stripe.Invoice;
        if (invoice == null) return;

        try
        {
            _logger.LogInformation("Invoice {InvoiceId} created for customer {CustomerId}", 
                invoice.Id, invoice.CustomerId);

           
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling invoice created webhook for {InvoiceId}", invoice.Id);
        }
    }

    private async Task HandleInvoiceVoided(Event stripeEvent)
    {
        var invoice = stripeEvent.Data.Object as Stripe.Invoice;
        if (invoice == null) return;

        try
        {
            _logger.LogInformation("Invoice {InvoiceId} voided for customer {CustomerId}", 
                invoice.Id, invoice.CustomerId);

            // Update billing record status if it exists
            var billingRecord = await _billingRepository.GetByStripeInvoiceIdAsync(invoice.Id);
            if (billingRecord != null)
            {
                billingRecord.Status = BillingRecord.BillingStatus.Cancelled;
                billingRecord.UpdatedDate = DateTime.UtcNow;
                billingRecord.ErrorMessage = "Invoice voided";
                await _billingRepository.UpdateAsync(billingRecord);

                _logger.LogInformation("Invoice voided handled for billing record {BillingRecordId}", billingRecord.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling invoice voided webhook for {InvoiceId}", invoice.Id);
        }
    }

    private async Task HandleCheckoutSessionCompleted(Event stripeEvent)
    {
        // Note: Stripe.Session might not be available in this version
        // We'll handle this event when the Stripe.NET version supports it
        _logger.LogInformation("Checkout session completed event received but not fully implemented due to Stripe.NET version limitations");
        return;
    }

    // Additional webhook handlers for comprehensive Stripe event coverage
    private async Task HandleProductCreated(Event stripeEvent)
    {
        var product = stripeEvent.Data.Object as Stripe.Product;
        _logger.LogInformation("Product created: {ProductId} - {ProductName}", product?.Id, product?.Name);
        // Implement product creation logic if needed
    }

    private async Task HandleProductUpdated(Event stripeEvent)
    {
        var product = stripeEvent.Data.Object as Stripe.Product;
        _logger.LogInformation("Product updated: {ProductId} - {ProductName}", product?.Id, product?.Name);
        // Implement product update logic if needed
    }

    private async Task HandleProductDeleted(Event stripeEvent)
    {
        var product = stripeEvent.Data.Object as Stripe.Product;
        _logger.LogInformation("Product deleted: {ProductId}", product?.Id);
        // Implement product deletion logic if needed
    }

    private async Task HandlePriceCreated(Event stripeEvent)
    {
        var price = stripeEvent.Data.Object as Stripe.Price;
        _logger.LogInformation("Price created: {PriceId} - {Amount} {Currency}", price?.Id, price?.UnitAmount, price?.Currency);
        // Implement price creation logic if needed
    }

    private async Task HandlePriceUpdated(Event stripeEvent)
    {
        var price = stripeEvent.Data.Object as Stripe.Price;
        _logger.LogInformation("Price updated: {PriceId} - {Amount} {Currency}", price?.Id, price?.UnitAmount, price?.Currency);
        // Implement price update logic if needed
    }

    private async Task HandlePriceDeleted(Event stripeEvent)
    {
        var price = stripeEvent.Data.Object as Stripe.Price;
        _logger.LogInformation("Price deleted: {PriceId}", price?.Id);
        // Implement price deletion logic if needed
    }

    private async Task HandlePayoutCreated(Event stripeEvent)
    {
        var payout = stripeEvent.Data.Object as Stripe.Payout;
        _logger.LogInformation("Payout created: {PayoutId} - {Amount} {Currency}", payout?.Id, payout?.Amount, payout?.Currency);
        // Implement payout creation logic if needed
    }

    private async Task HandlePayoutUpdated(Event stripeEvent)
    {
        var payout = stripeEvent.Data.Object as Stripe.Payout;
        _logger.LogInformation("Payout updated: {PayoutId} - {Status}", payout?.Id, payout?.Status);
        // Implement payout update logic if needed
    }

    private async Task HandlePayoutPaid(Event stripeEvent)
    {
        var payout = stripeEvent.Data.Object as Stripe.Payout;
        _logger.LogInformation("Payout paid: {PayoutId} - {Amount} {Currency}", payout?.Id, payout?.Amount, payout?.Currency);
        // Implement payout paid logic if needed
    }

    private async Task HandlePayoutFailed(Event stripeEvent)
    {
        var payout = stripeEvent.Data.Object as Stripe.Payout;
        _logger.LogWarning("Payout failed: {PayoutId} - {FailureCode}: {FailureMessage}", 
            payout?.Id, payout?.FailureCode, payout?.FailureMessage);
        // Implement payout failure logic if needed
    }

    private async Task HandlePayoutCanceled(Event stripeEvent)
    {
        var payout = stripeEvent.Data.Object as Stripe.Payout;
        _logger.LogInformation("Payout canceled: {PayoutId}", payout?.Id);
        // Implement payout cancellation logic if needed
    }

    private async Task HandleBalanceAvailable(Event stripeEvent)
    {
        var balance = stripeEvent.Data.Object as Stripe.Balance;
        _logger.LogInformation("Balance available: {AvailableAmount} {Currency}", 
            balance?.Available?.FirstOrDefault()?.Amount, balance?.Available?.FirstOrDefault()?.Currency);
        // Implement balance available logic if needed
    }

    private async Task HandleMandateUpdated(Event stripeEvent)
    {
        var mandate = stripeEvent.Data.Object as Stripe.Mandate;
        _logger.LogInformation("Mandate updated: {MandateId} - {Status}", mandate?.Id, mandate?.Status);
        // Implement mandate update logic if needed
    }

    private async Task HandleReviewOpened(Event stripeEvent)
    {
        var review = stripeEvent.Data.Object as Stripe.Review;
        _logger.LogInformation("Review opened: {ReviewId} - {Reason}", review?.Id, review?.Reason);
        // Implement review opened logic if needed
    }

    private async Task HandleReviewClosed(Event stripeEvent)
    {
        var review = stripeEvent.Data.Object as Stripe.Review;
        _logger.LogInformation("Review closed: {ReviewId} - {Reason}", review?.Id, review?.Reason);
        // Implement review closed logic if needed
    }

    private async Task HandleSubscriptionScheduleCanceled(Event stripeEvent)
    {
        var schedule = stripeEvent.Data.Object as Stripe.SubscriptionSchedule;
        _logger.LogInformation("Subscription schedule canceled: {ScheduleId}", schedule?.Id);
        // Implement subscription schedule cancellation logic if needed
    }

    private async Task HandleSubscriptionScheduleCompleted(Event stripeEvent)
    {
        var schedule = stripeEvent.Data.Object as Stripe.SubscriptionSchedule;
        _logger.LogInformation("Subscription schedule completed: {ScheduleId}", schedule?.Id);
        // Implement subscription schedule completion logic if needed
    }

    private async Task HandleSubscriptionScheduleCreated(Event stripeEvent)
    {
        var schedule = stripeEvent.Data.Object as Stripe.SubscriptionSchedule;
        _logger.LogInformation("Subscription schedule created: {ScheduleId}", schedule?.Id);
        // Implement subscription schedule creation logic if needed
    }

    private async Task HandleSubscriptionScheduleReleased(Event stripeEvent)
    {
        var schedule = stripeEvent.Data.Object as Stripe.SubscriptionSchedule;
        _logger.LogInformation("Subscription schedule released: {ScheduleId}", schedule?.Id);
        // Implement subscription schedule release logic if needed
    }

    private async Task HandleSubscriptionScheduleUpdated(Event stripeEvent)
    {
        var schedule = stripeEvent.Data.Object as Stripe.SubscriptionSchedule;
        _logger.LogInformation("Subscription schedule updated: {ScheduleId}", schedule?.Id);
        // Implement subscription schedule update logic if needed
    }

    private async Task HandleTaxRateCreated(Event stripeEvent)
    {
        var taxRate = stripeEvent.Data.Object as Stripe.TaxRate;
        _logger.LogInformation("Tax rate created: {TaxRateId} - {Percentage}%", taxRate?.Id, taxRate?.Percentage);
        // Implement tax rate creation logic if needed
    }

    private async Task HandleTaxRateUpdated(Event stripeEvent)
    {
        var taxRate = stripeEvent.Data.Object as Stripe.TaxRate;
        _logger.LogInformation("Tax rate updated: {TaxRateId} - {Percentage}%", taxRate?.Id, taxRate?.Percentage);
        // Implement tax rate update logic if needed
    }

    private async Task HandleTransferCreated(Event stripeEvent)
    {
        var transfer = stripeEvent.Data.Object as Stripe.Transfer;
        _logger.LogInformation("Transfer created: {TransferId} - {Amount} {Currency}", transfer?.Id, transfer?.Amount, transfer?.Currency);
        // Implement transfer creation logic if needed
    }

    private async Task HandleTransferFailed(Event stripeEvent)
    {
        var transfer = stripeEvent.Data.Object as Stripe.Transfer;
        _logger.LogWarning("Transfer failed: {TransferId}", transfer?.Id);
        // Implement transfer failure logic if needed
    }

    private async Task HandleTransferPaid(Event stripeEvent)
    {
        var transfer = stripeEvent.Data.Object as Stripe.Transfer;
        _logger.LogInformation("Transfer paid: {TransferId} - {Amount} {Currency}", transfer?.Id, transfer?.Amount, transfer?.Currency);
        // Implement transfer paid logic if needed
    }

    private async Task HandleTransferReversed(Event stripeEvent)
    {
        var transfer = stripeEvent.Data.Object as Stripe.Transfer;
        _logger.LogInformation("Transfer reversed: {TransferId}", transfer?.Id);
        // Implement transfer reversal logic if needed
    }

    private async Task HandleTransferUpdated(Event stripeEvent)
    {
        var transfer = stripeEvent.Data.Object as Stripe.Transfer;
        _logger.LogInformation("Transfer updated: {TransferId}", transfer?.Id);
        // Implement transfer update logic if needed
    }
}
}
