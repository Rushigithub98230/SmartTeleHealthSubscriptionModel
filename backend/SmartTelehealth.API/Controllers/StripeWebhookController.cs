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
    private readonly ISubscriptionBillingService _billingService;
    private readonly IBillingRepository _billingRepository;
    private readonly INotificationService _notificationService;
    private readonly ICommunicationService _communicationService;
    private readonly IPaymentService _paymentService;
    private readonly IWebhookService _webhookService;
    private readonly IStripeService _stripeService;
    private readonly ISubscriptionLifecycleService _subscriptionLifecycleService;
    private readonly IWebhookIdempotencyService _webhookIdempotencyService;
    private readonly ILogger<StripeWebhookController> _logger;
    private readonly IConfiguration _configuration;
    private readonly int _maxRetries;
    private readonly int _retryDelaySeconds;

    /// <summary>
    /// Initializes a new instance of the StripeWebhookController with required services.
    /// UPDATED: Now uses consolidated ISubscriptionBillingService
    /// </summary>
    /// <param name="subscriptionService">Service for subscription management operations</param>
    /// <param name="billingService">Service for billing and payment operations (consolidated)</param>
    /// <param name="billingRepository">Repository for billing data access</param>
    /// <param name="notificationService">Service for notification management</param>
    /// <param name="stripeService">Service for Stripe integration operations</param>
    /// <param name="subscriptionLifecycleService">Service for subscription lifecycle management</param>
    /// <param name="logger">Logger for webhook event tracking and debugging</param>
    /// <param name="configuration">Configuration for webhook settings and retry logic</param>
    public StripeWebhookController(
        ISubscriptionService subscriptionService,
        ISubscriptionBillingService billingService,
        IBillingRepository billingRepository,
        INotificationService notificationService,
        ICommunicationService communicationService,
        IPaymentService paymentService,
        IWebhookService webhookService,
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
        _paymentService = paymentService;
        _webhookService = webhookService;
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
                await _webhookService.HandleSubscriptionCreatedAsync(stripeEvent);
                break;
            case "customer.subscription.updated":
                await _webhookService.HandleSubscriptionUpdatedAsync(stripeEvent);
                break;
            case "customer.subscription.deleted":
                await _webhookService.HandleSubscriptionDeletedAsync(stripeEvent);
                break;
            case "customer.subscription.paused":
                await _webhookService.HandleSubscriptionPausedAsync(stripeEvent);
                break;
            case "customer.subscription.resumed":
                await _webhookService.HandleSubscriptionResumedAsync(stripeEvent);
                break;
            case "customer.subscription.past_due":
                await _webhookService.HandleSubscriptionPastDueAsync(stripeEvent);
                break;
            case "customer.subscription.unpaid":
                await _webhookService.HandleSubscriptionUnpaidAsync(stripeEvent);
                break;
            case "invoice.payment_succeeded":
                await _webhookService.HandlePaymentSucceededAsync(stripeEvent);
                break;
            case "invoice.payment_failed":
                await _webhookService.HandlePaymentFailedAsync(stripeEvent);
                break;
            case "invoice.payment_action_required":
                await _webhookService.HandleInvoicePaymentActionRequiredAsync(stripeEvent);
                break;
            case "invoice.finalized":
                await _webhookService.HandleInvoiceFinalizedAsync(stripeEvent);
                break;
            case "invoice.sent":
                await _webhookService.HandleInvoiceSentAsync(stripeEvent);
                break;
            case "invoice.upcoming":
                await _webhookService.HandleInvoiceUpcomingAsync(stripeEvent);
                break;
            case "invoice.finalization_failed":
                await _webhookService.HandleInvoiceFinalizationFailedAsync(stripeEvent);
                break;
            case "customer.subscription.trial_will_end":
                await _webhookService.HandleSubscriptionTrialWillEndAsync(stripeEvent);
                break;
            case "payment_intent.succeeded":
                await _webhookService.HandlePaymentIntentSucceededAsync(stripeEvent);
                break;
            case "payment_intent.payment_failed":
                await _webhookService.HandlePaymentIntentFailedAsync(stripeEvent);
                break;
            case "payment_method.attached":
                await _webhookService.HandlePaymentMethodAttachedAsync(stripeEvent);
                break;
            case "payment_method.updated":
                await _webhookService.HandlePaymentMethodUpdatedAsync(stripeEvent);
                break;
            case "payment_method.detached":
                await _webhookService.HandlePaymentMethodDetachedAsync(stripeEvent);
                break;
            case "charge.refunded":
                await _webhookService.HandleChargeRefundedAsync(stripeEvent);
                break;
            case "charge.dispute.created":
                await _webhookService.HandleChargeDisputeCreatedAsync(stripeEvent);
                break;
            case "charge.dispute.closed":
                await _webhookService.HandleChargeDisputeClosedAsync(stripeEvent);
                break;
            case "customer.created":
                await _webhookService.HandleCustomerCreatedAsync(stripeEvent);
                break;
            case "customer.updated":
                await _webhookService.HandleCustomerUpdatedAsync(stripeEvent);
                break;
            case "customer.deleted":
                await _webhookService.HandleCustomerDeletedAsync(stripeEvent);
                break;
            case "setup_intent.succeeded":
                await _webhookService.HandleSetupIntentSucceededAsync(stripeEvent);
                break;
            case "setup_intent.setup_failed":
                await _webhookService.HandleSetupIntentFailedAsync(stripeEvent);
                break;
            case "payment_intent.requires_action":
                await _webhookService.HandlePaymentIntentRequiresActionAsync(stripeEvent);
                break;
            case "invoice.created":
                await _webhookService.HandleInvoiceCreatedAsync(stripeEvent);
                break;
            case "invoice.voided":
                await _webhookService.HandleInvoiceVoidedAsync(stripeEvent);
                break;
            case "checkout.session.completed":
                await _webhookService.HandleCheckoutSessionCompletedAsync(stripeEvent);
                break;
            default:
                // Log unhandled event type
                _logger.LogInformation("Unhandled Stripe webhook event type: {EventType}", stripeEvent.Type);
                break;
        }
    }

    /// <summary>
    /// Validates the webhook secret configuration
    /// </summary>
    /// <param name="webhookSecret">The webhook secret to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    private bool ValidateWebhookSecret(string webhookSecret)
    {
        return !string.IsNullOrEmpty(webhookSecret) && webhookSecret.StartsWith("whsec_");
    }
}
}