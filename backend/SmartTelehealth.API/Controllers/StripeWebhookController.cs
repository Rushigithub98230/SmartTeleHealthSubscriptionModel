using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using Stripe;
using SmartTelehealth.Application.DTOs;
using Stripe.Events;
using Microsoft.Extensions.Logging;


namespace SmartTelehealth.API.Controllers
{
[ApiController]
[Route("api/[controller]")]
public class StripeWebhookController : BaseController
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly IBillingService _billingService;
    private readonly IBillingRepository _billingRepository;
    private readonly INotificationService _notificationService;
    private readonly IAuditService _auditService;
    private readonly IStripeService _stripeService;
    private readonly ISubscriptionLifecycleService _subscriptionLifecycleService;
    private readonly ILogger<StripeWebhookController> _logger;
    private readonly IConfiguration _configuration;
    private readonly int _maxRetries;
    private readonly int _retryDelaySeconds;

    public StripeWebhookController(
        ISubscriptionService subscriptionService,
        IBillingService billingService,
        IBillingRepository billingRepository,
        INotificationService notificationService,
        IAuditService auditService,
        IStripeService stripeService,
        ISubscriptionLifecycleService subscriptionLifecycleService,
        ILogger<StripeWebhookController> logger,
        IConfiguration configuration)
    {
        _subscriptionService = subscriptionService;
        _billingService = billingService;
        _billingRepository = billingRepository;
        _notificationService = notificationService;
        _auditService = auditService;
        _stripeService = stripeService;
        _subscriptionLifecycleService = subscriptionLifecycleService;
        _logger = logger;
        _configuration = configuration;
        _maxRetries = configuration.GetValue<int>("Stripe:WebhookRetryAttempts", 3);
        _retryDelaySeconds = configuration.GetValue<int>("Stripe:WebhookRetryDelaySeconds", 5);
    }

    [HttpPost]
    public async Task<JsonModel> HandleWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var webhookSecret = _configuration["Stripe:WebhookSecret"];

        if (string.IsNullOrEmpty(webhookSecret) || webhookSecret == "whsec_test_webhook_secret_replace_in_production")
        {
            return new JsonModel { data = new object(), Message = "Webhook secret not configured", StatusCode = 400 };
        }

        var stripeEvent = EventUtility.ConstructEvent(
            json,
            Request.Headers["Stripe-Signature"],
            webhookSecret
        );

        // CRITICAL FIX: Implement webhook idempotency
        try
        {
            // Check if this event has already been processed
            var eventId = stripeEvent.Id;
            var isProcessed = await _auditService.IsEventProcessedAsync(eventId);
            if (isProcessed)
            {
                _logger.LogInformation("Webhook event {EventId} already processed, skipping", eventId);
                return new JsonModel { data = new object(), Message = "Event already processed", StatusCode = 200 };
            }

            // Mark event as being processed
            await _auditService.LogActionAsync("Webhook", "Processing", eventId, 
                $"Processing Stripe webhook event {stripeEvent.Type}", GetToken(HttpContext));

            // Process webhook with retry logic
            await ProcessWebhookWithRetryAsync(stripeEvent);

            // Mark event as successfully processed
            await _auditService.LogActionAsync("Webhook", "Processed", eventId, 
                $"Successfully processed Stripe webhook event {stripeEvent.Type}", GetToken(HttpContext));

            return new JsonModel { data = new object(), Message = "Webhook processed successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook event {EventId} of type {EventType}", stripeEvent.Id, stripeEvent.Type);
            
            // Mark event as failed
            await _auditService.LogActionAsync("Webhook", "Failed", stripeEvent.Id, 
                $"Failed to process Stripe webhook event {stripeEvent.Type}: {ex.Message}", GetToken(HttpContext));
            
            throw; // Re-throw to trigger retry mechanism
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
                if (attempt == _maxRetries)
                {
                    // Log final failure
                    throw;
                }
                
                // Wait before retry
                await Task.Delay(_retryDelaySeconds * 1000);
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
            await _subscriptionService.UpdateSubscriptionAsync(localSubscription.data.ToString(), updateDto, GetToken(HttpContext));
        }
    }

    private async Task HandleSubscriptionUpdated(Event stripeEvent)
    {
        var subscription = stripeEvent.Data.Object as Stripe.Subscription;
        if (subscription == null) return;

        var localSubscription = await _subscriptionService.GetByStripeSubscriptionIdAsync(subscription.Id, GetToken(HttpContext));
        if (localSubscription.StatusCode == 200)
        {
            var updateDto = new UpdateSubscriptionDto
            {
                Status = MapStripeStatusToLocal(subscription.Status),
                NextBillingDate = GetNextBillingDateFromSubscription(subscription),
                CurrentPrice = subscription.Items.Data.FirstOrDefault()?.Price.UnitAmount / 100m ?? 0
            };
            await _subscriptionService.UpdateSubscriptionAsync(localSubscription.data.ToString(), updateDto, GetToken(HttpContext));
        }
    }

    private async Task HandleSubscriptionDeleted(Event stripeEvent)
    {
        var subscription = stripeEvent.Data.Object as Stripe.Subscription;
        if (subscription == null) return;

        var localSubscription = await _subscriptionService.GetByStripeSubscriptionIdAsync(subscription.Id, GetToken(HttpContext));
        if (localSubscription.StatusCode == 200)
        {
            await _subscriptionService.CancelSubscriptionAsync(localSubscription.data.ToString(), "Cancelled via Stripe", GetToken(HttpContext));
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

                        await _subscriptionService.UpdateSubscriptionAsync(localSubscription.data.ToString(), updateDto, GetToken(HttpContext));

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

                        // Log payment success
                        await _auditService.LogActionAsync("Subscription", "PaymentSucceeded", 
                            subscriptionData.Id?.ToString(), 
                            $"Payment succeeded for subscription {subscriptionId}. Invoice: {invoice.Number}", 
                            GetToken(HttpContext));

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
                        
                        await _subscriptionService.UpdateSubscriptionAsync(localSubscription.data.ToString(), updateDto, GetToken(HttpContext));

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

                        // Log payment failure
                        await _auditService.LogActionAsync("Subscription", "PaymentFailed", 
                            subscriptionData.Id?.ToString(), 
                            $"Payment failed for subscription {subscriptionId}. Invoice: {invoice.Number}", 
                            GetToken(HttpContext));

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

                    // Log trial ending event
                    await _auditService.LogActionAsync("Subscription", "TrialEnding", 
                        subscriptionData.Id?.ToString(), 
                        $"Trial ending for subscription {subscription.Id} on {subscription.TrialEnd}", 
                        GetToken(HttpContext));

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
                await _subscriptionService.UpdateSubscriptionAsync(localSubscription.data.ToString(), updateDto, GetToken(HttpContext));
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
        // In Stripe.net 48.4.0, PaymentIntentId is removed from Invoice
        // We need to extract it from the Invoice.Payments array
        if (invoice.Payments?.Data?.Count > 0)
        {
            var invoicePayment = invoice.Payments.Data.FirstOrDefault();
            if (invoicePayment != null)
            {
                // The payment intent ID might be available through the payment object
                // For now, return empty string as we need additional API calls to get this
                return string.Empty;
            }
        }
        return string.Empty;
    }

    private DateTime GetNextBillingDateFromSubscription(Stripe.Subscription subscription)
    {
        try
        {
            // In Stripe.net 48.4.0, CurrentPeriodEnd is moved to subscription items
            var firstItem = subscription.Items.Data.FirstOrDefault();
            if (firstItem?.CurrentPeriodEnd != null)
            {
                // Convert Unix timestamp to DateTime
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
        // In Stripe.net 48.4.0, SubscriptionId is removed from Invoice
        // Try to get it from different possible locations
        if (invoice.Parent != null)
        {
            // Try to convert Parent to string - it might be the subscription ID directly
            var parentString = invoice.Parent.ToString();
            if (!string.IsNullOrEmpty(parentString) && parentString != "null")
            {
                return parentString;
            }
        }
        
        // Fallback: try to get from metadata or other fields
        if (invoice.Metadata?.ContainsKey("subscription_id") == true)
        {
            return invoice.Metadata["subscription_id"];
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
            _ => "Pending"
        };
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
            await _subscriptionService.UpdateSubscriptionAsync(localSubscription.data.ToString(), updateDto, GetToken(HttpContext));
            
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
            await _subscriptionService.UpdateSubscriptionAsync(localSubscription.data.ToString(), updateDto, GetToken(HttpContext));
            
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
            await _subscriptionService.UpdateSubscriptionAsync(localSubscription.data.ToString(), updateDto, GetToken(HttpContext));
            
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
            await _subscriptionService.UpdateSubscriptionAsync(localSubscription.data.ToString(), updateDto, GetToken(HttpContext));
            
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

                    await _subscriptionService.UpdateSubscriptionAsync(subscriptionId, updateDto, GetToken(HttpContext));

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

                    // Log trial expiration
                    await _auditService.LogActionAsync("Subscription", "TrialExpired", 
                        subscriptionId, 
                        $"Trial expired for subscription {subscriptionId} due to payment failure. Invoice: {invoiceNumber}", 
                        GetToken(HttpContext));

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
            await _auditService.LogActionAsync("PaymentMethod", "SetupSucceeded", setupIntent.Id, 
                $"Payment method setup succeeded for customer {setupIntent.CustomerId}", GetToken(HttpContext));
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

            // Log failed payment method setup
            await _auditService.LogActionAsync("PaymentMethod", "SetupFailed", setupIntent.Id, 
                $"Payment method setup failed for customer {setupIntent.CustomerId}: {setupIntent.LastSetupError?.Message}", GetToken(HttpContext));
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

            // Log invoice creation
            await _auditService.LogActionAsync("Invoice", "Created", invoice.Id, 
                $"Invoice {invoice.Number} created for customer {invoice.CustomerId}", GetToken(HttpContext));
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

            // Log invoice voiding
            await _auditService.LogActionAsync("Invoice", "Voided", invoice.Id, 
                $"Invoice {invoice.Number} voided for customer {invoice.CustomerId}", GetToken(HttpContext));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling invoice voided webhook for {InvoiceId}", invoice.Id);
        }
    }
}
} 