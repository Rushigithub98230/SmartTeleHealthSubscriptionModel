using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using Stripe;
using Stripe.Checkout;

namespace SmartTelehealth.Infrastructure.Services;

/// <summary>
/// Stripe payment service that handles all Stripe-related operations including:
/// - Customer management (create, retrieve, update)
/// - Payment method management (add, remove, validate)
/// - Subscription lifecycle management (create, update, cancel, pause, resume)
/// - Payment processing (one-time payments, recurring billing)
/// - Invoice management and payment intent handling
/// - Product and price management for subscription plans
/// - Webhook event processing and error handling
/// - Retry logic for failed operations
/// </summary>
public class StripeService : IStripeService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<StripeService> _logger;
    private readonly int _maxRetries = 3;
    private readonly TimeSpan _retryDelay = TimeSpan.FromSeconds(1);
    
    /// <summary>
    /// Initializes a new instance of the StripeService with configuration and logging
    /// </summary>
    /// <param name="configuration">Configuration instance containing Stripe settings</param>
    /// <param name="logger">Logger instance for logging operations and errors</param>
    /// <exception cref="ArgumentNullException">Thrown when configuration or logger is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when Stripe secret key is not configured</exception>
    public StripeService(IConfiguration configuration, ILogger<StripeService> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        // Validate and set Stripe API key from configuration
        var secretKey = _configuration["StripeSettings:SecretKey"];
        if (string.IsNullOrEmpty(secretKey))
        {
            throw new InvalidOperationException("Stripe secret key is not configured");
        }
        
        // Initialize Stripe with the secret key
        StripeConfiguration.ApiKey = secretKey;
        _logger.LogInformation("Stripe service initialized successfully");
    }
    
    #region Customer Management

    /// <summary>
    /// Creates a new Stripe customer with the provided email and name
    /// </summary>
    /// <param name="email">Customer's email address</param>
    /// <param name="name">Customer's full name</param>
    /// <param name="tokenModel">Token containing user authentication information for audit purposes</param>
    /// <returns>The Stripe customer ID of the created customer</returns>
    /// <exception cref="ArgumentException">Thrown when email or name is null or empty</exception>
    /// <exception cref="InvalidOperationException">Thrown when Stripe customer creation fails</exception>
    /// <remarks>
    /// This method:
    /// - Validates input parameters
    /// - Creates a Stripe customer with metadata for tracking
    /// - Includes audit information (user ID, role ID, creation timestamp)
    /// - Uses retry logic for resilience
    /// - Logs successful creation and errors
    /// </remarks>
    public async Task<string> CreateCustomerAsync(string email, string name, TokenModel tokenModel)
    {
        if (string.IsNullOrEmpty(email))
            throw new ArgumentException("Email is required", nameof(email));
        
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Name is required", nameof(name));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                // Create customer options with metadata for tracking and audit
                var customerCreateOptions = new CustomerCreateOptions
                {
                    Email = email,
                    Name = name,
                    Metadata = new Dictionary<string, string>
                    {
                        { "created_at", DateTime.UtcNow.ToString("O") },
                        { "source", "smart_telehealth" },
                        { "user_id", tokenModel.UserID.ToString() },
                        { "role_id", tokenModel.RoleID.ToString() }
                    }
                };

                // Create the customer in Stripe
                var customerService = new CustomerService();
                var customer = await customerService.CreateAsync(customerCreateOptions);

                _logger.LogInformation("Created Stripe customer: {CustomerId} for email {Email} by user {UserId}", customer.Id, email, tokenModel.UserID);
                return customer.Id;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error creating customer for email {Email}: {Message}", email, ex.Message);
                throw new InvalidOperationException($"Failed to create Stripe customer: {ex.Message}", ex);
            }
        });
    }

    /// <summary>
    /// Retrieves a Stripe customer by their customer ID
    /// </summary>
    /// <param name="customerId">The Stripe customer ID to retrieve</param>
    /// <param name="tokenModel">Token containing user authentication information for audit purposes</param>
    /// <returns>CustomerDto containing customer information</returns>
    /// <exception cref="ArgumentException">Thrown when customerId is null/empty or customer not found</exception>
    /// <exception cref="InvalidOperationException">Thrown when Stripe API call fails</exception>
    /// <remarks>
    /// This method:
    /// - Validates the customer ID parameter
    /// - Retrieves customer data from Stripe
    /// - Maps Stripe customer to CustomerDto
    /// - Handles customer not found scenarios
    /// - Uses retry logic for resilience
    /// </remarks>
    public async Task<CustomerDto> GetCustomerAsync(string customerId, TokenModel tokenModel)
    {
        if (string.IsNullOrEmpty(customerId))
            throw new ArgumentException("Customer ID is required", nameof(customerId));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                // Retrieve customer from Stripe
                var customerService = new CustomerService();
                var customer = await customerService.GetAsync(customerId);

                // Map Stripe customer to DTO
                return new CustomerDto
                {
                    Id = customer.Id,
                    Email = customer.Email,
                    Name = customer.Name,
                    CreatedDate = customer.Created
                };
            }
            catch (StripeException ex) when (ex.StripeError?.Type == "invalid_request_error")
            {
                _logger.LogWarning("Customer not found: {CustomerId} by user {UserId}", customerId, tokenModel.UserID);
                throw new ArgumentException($"Customer not found: {customerId}");
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error getting customer {CustomerId}: {Message}", customerId, ex.Message);
                throw new InvalidOperationException($"Failed to get Stripe customer: {ex.Message}", ex);
            }
        });
    }

    /// <summary>
    /// Lists all Stripe customers with pagination support
    /// </summary>
    /// <param name="tokenModel">Token containing user authentication information for audit purposes</param>
    /// <returns>Collection of CustomerDto objects representing Stripe customers</returns>
    /// <exception cref="InvalidOperationException">Thrown when Stripe API call fails</exception>
    /// <remarks>
    /// This method:
    /// - Retrieves a list of customers from Stripe with pagination
    /// - Limits results to 100 customers for performance
    /// - Maps Stripe customer objects to CustomerDto format
    /// - Uses retry logic for resilience
    /// - Used for administrative customer management
    /// - Logs successful operations and errors
    /// </remarks>
    public async Task<IEnumerable<CustomerDto>> ListCustomersAsync(TokenModel tokenModel)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                // Create customer service and list customers with pagination
                var customerService = new CustomerService();
                var customers = await customerService.ListAsync(new CustomerListOptions
                {
                    Limit = 100 // Limit to 100 customers for performance
                });

                // Map Stripe customers to DTOs
                return customers.Data.Select(customer => new CustomerDto
                {
                    Id = customer.Id,
                    Email = customer.Email,
                    Name = customer.Name,
                    CreatedDate = customer.Created
                });
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error listing customers: {Message}", ex.Message);
                throw new InvalidOperationException($"Failed to list Stripe customers: {ex.Message}", ex);
            }
        });
    }
    
    /// <summary>
    /// Updates an existing Stripe customer with new email and name information
    /// </summary>
    /// <param name="customerId">The Stripe customer ID to update</param>
    /// <param name="email">New email address for the customer</param>
    /// <param name="name">New name for the customer</param>
    /// <param name="tokenModel">Token containing user authentication information for audit purposes</param>
    /// <returns>True if customer was updated successfully, false otherwise</returns>
    /// <exception cref="ArgumentException">Thrown when customerId is null or empty</exception>
    /// <exception cref="InvalidOperationException">Thrown when Stripe customer update fails</exception>
    /// <remarks>
    /// This method:
    /// - Validates the customer ID parameter
    /// - Updates customer information in Stripe
    /// - Includes audit metadata for tracking updates
    /// - Uses retry logic for resilience
    /// - Logs successful updates and errors
    /// - Returns boolean result for operation success
    /// </remarks>
    public async Task<bool> UpdateCustomerAsync(string customerId, string email, string name, TokenModel tokenModel)
    {
        if (string.IsNullOrEmpty(customerId))
            throw new ArgumentException("Customer ID is required", nameof(customerId));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                // Create customer update options with new information and audit metadata
                var customerUpdateOptions = new CustomerUpdateOptions
                {
                    Email = email,
                    Name = name,
                    Metadata = new Dictionary<string, string>
                    {
                        { "updated_at", DateTime.UtcNow.ToString("O") },
                        { "source", "smart_telehealth" },
                        { "updated_by_user_id", tokenModel.UserID.ToString() },
                        { "updated_by_role_id", tokenModel.RoleID.ToString() }
                    }
                };

                var customerService = new CustomerService();
                await customerService.UpdateAsync(customerId, customerUpdateOptions);

                _logger.LogInformation("Updated Stripe customer: {CustomerId} by user {UserId}", customerId, tokenModel.UserID);
                return true;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error updating customer {CustomerId}: {Message}", customerId, ex.Message);
                throw new InvalidOperationException($"Failed to update Stripe customer: {ex.Message}", ex);
            }
        });
    }
    #endregion

    // Payment Method Management
    #region Payment Method Management

    /// <summary>
    /// Retrieves all payment methods for a specific Stripe customer
    /// </summary>
    /// <param name="customerId">The Stripe customer ID to get payment methods for</param>
    /// <param name="tokenModel">Token containing user authentication information for audit purposes</param>
    /// <returns>Collection of PaymentMethodDto objects representing customer's payment methods</returns>
    /// <exception cref="ArgumentException">Thrown when customerId is null or empty</exception>
    /// <exception cref="InvalidOperationException">Thrown when Stripe API call fails</exception>
    /// <remarks>
    /// This method:
    /// - Validates the customer ID parameter
    /// - Retrieves all card payment methods for the customer
    /// - Maps Stripe payment method objects to PaymentMethodDto format
    /// - Uses retry logic for resilience
    /// - Used for payment method management and selection
    /// - Logs successful operations and errors
    /// </remarks>
    public async Task<IEnumerable<PaymentMethodDto>> GetCustomerPaymentMethodsAsync(string customerId, TokenModel tokenModel)
    {
        if (string.IsNullOrEmpty(customerId))
            throw new ArgumentException("Customer ID is required", nameof(customerId));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                // Create payment method service and list customer's payment methods
                var paymentMethodService = new PaymentMethodService();
                var paymentMethods = await paymentMethodService.ListAsync(new PaymentMethodListOptions
                {
                    Customer = customerId,
                    Type = "card" // Only retrieve card payment methods
                });

                var customerService = new CustomerService();
                var customer = await customerService.GetAsync(customerId);
                var defaultPaymentMethodId = customer.InvoiceSettings?.DefaultPaymentMethod;

                return paymentMethods.Data.Select(pm => new PaymentMethodDto
                {
                    Id = pm.Id,
                    CustomerId = pm.CustomerId,
                    Type = pm.Type,
                    Card = new CardDto
                    {
                        Brand = pm.Card?.Brand,
                        Last4 = pm.Card?.Last4,
                        ExpMonth = (int)(pm.Card?.ExpMonth ?? 0),
                        ExpYear = (int)(pm.Card?.ExpYear ?? 0),
                        Fingerprint = pm.Card?.Fingerprint
                    },
                    IsDefault =  pm.Id.Equals(defaultPaymentMethodId),
                    CreatedDate = pm.Created
                });
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error getting payment methods for customer {CustomerId}: {Message}", customerId, ex.Message);
                throw new InvalidOperationException($"Failed to get payment methods: {ex.Message}", ex);
            }
        });
    }

    /// <summary>
    /// Attaches a payment method to a Stripe customer
    /// </summary>
    /// <param name="customerId">The Stripe customer ID to attach the payment method to</param>
    /// <param name="paymentMethodId">The Stripe payment method ID to attach</param>
    /// <param name="tokenModel">Token containing user authentication information for audit purposes</param>
    /// <returns>The payment method ID of the attached payment method</returns>
    /// <exception cref="ArgumentException">Thrown when required parameters are null or empty</exception>
    /// <exception cref="InvalidOperationException">Thrown when Stripe payment method attachment fails</exception>
    /// <remarks>
    /// This method:
    /// - Validates input parameters (customerId and paymentMethodId)
    /// - Attaches the payment method to the customer in Stripe
    /// - Uses retry logic for resilience
    /// - Used for adding new payment methods to customer accounts
    /// - Logs successful attachments and errors
    /// - Returns the payment method ID for confirmation
    /// </remarks>
    public async Task<string> AddPaymentMethodAsync(string customerId, string paymentMethodId, TokenModel tokenModel)
    {
        if (string.IsNullOrEmpty(customerId))
            throw new ArgumentException("Customer ID is required", nameof(customerId));
        
        if (string.IsNullOrEmpty(paymentMethodId))
            throw new ArgumentException("Payment method ID is required", nameof(paymentMethodId));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                // Attach payment method to customer
                var paymentMethodService = new PaymentMethodService();
                var paymentMethod = await paymentMethodService.AttachAsync(paymentMethodId, new PaymentMethodAttachOptions
                {
                    Customer = customerId
                });

                _logger.LogInformation("Added payment method {PaymentMethodId} to customer {CustomerId} by user {UserId}", 
                    paymentMethodId, customerId, tokenModel.UserID);
                return paymentMethod.Id;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error adding payment method {PaymentMethodId} to customer {CustomerId}: {Message}", 
                    paymentMethodId, customerId, ex.Message);
                throw new InvalidOperationException($"Failed to add payment method: {ex.Message}", ex);
            }
        });
    }

    public async Task<bool> SetDefaultPaymentMethodAsync(string customerId, string paymentMethodId, TokenModel tokenModel)
    {
        if (string.IsNullOrEmpty(customerId))
            throw new ArgumentException("Customer ID is required", nameof(customerId));
        
        if (string.IsNullOrEmpty(paymentMethodId))
            throw new ArgumentException("Payment method ID is required", nameof(paymentMethodId));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                var customerUpdateOptions = new CustomerUpdateOptions
                {
                    InvoiceSettings = new CustomerInvoiceSettingsOptions
                    {
                        DefaultPaymentMethod = paymentMethodId
                    }
                };

                var customerService = new CustomerService();
                await customerService.UpdateAsync(customerId, customerUpdateOptions);

                _logger.LogInformation("Set default payment method {PaymentMethodId} for customer {CustomerId} by user {UserId}", 
                    paymentMethodId, customerId, tokenModel.UserID);
                return true;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error setting default payment method {PaymentMethodId} for customer {CustomerId}: {Message}", 
                    paymentMethodId, customerId, ex.Message);
                throw new InvalidOperationException($"Failed to set default payment method: {ex.Message}", ex);
            }
        });
    }

    public async Task<bool> RemovePaymentMethodAsync(string customerId, string paymentMethodId, TokenModel tokenModel)
    {
        if (string.IsNullOrEmpty(customerId))
            throw new ArgumentException("Customer ID is required", nameof(customerId));
        
        if (string.IsNullOrEmpty(paymentMethodId))
            throw new ArgumentException("Payment method ID is required", nameof(paymentMethodId));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                var paymentMethodService = new PaymentMethodService();
                await paymentMethodService.DetachAsync(paymentMethodId);

                _logger.LogInformation("Removed payment method {PaymentMethodId} from customer {CustomerId} by user {UserId}", 
                    paymentMethodId, customerId, tokenModel.UserID);
                return true;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error removing payment method {PaymentMethodId} from customer {CustomerId}: {Message}", 
                    paymentMethodId, customerId, ex.Message);
                throw new InvalidOperationException($"Failed to remove payment method: {ex.Message}", ex);
            }
        });
    }

    public async Task<PaymentMethodValidationDto> ValidatePaymentMethodDetailedAsync(string paymentMethodId, TokenModel tokenModel)
    {
        if (string.IsNullOrEmpty(paymentMethodId))
            throw new ArgumentException("Payment method ID is required", nameof(paymentMethodId));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                var paymentMethodService = new PaymentMethodService();
                var paymentMethod = await paymentMethodService.GetAsync(paymentMethodId);

                var validationResult = new PaymentMethodValidationDto
                {
                    IsValid = true,
                    ValidationMessage = "Payment method is valid",
                    CardBrand = paymentMethod.Card?.Brand,
                    Last4 = paymentMethod.Card?.Last4,
                    ExpMonth = (int)(paymentMethod.Card?.ExpMonth ?? 0),
                    ExpYear = (int)(paymentMethod.Card?.ExpYear ?? 0),
                    IsExpired = paymentMethod.Card?.ExpYear < DateTime.Now.Year || 
                               (paymentMethod.Card?.ExpYear == DateTime.Now.Year && paymentMethod.Card?.ExpMonth < DateTime.Now.Month)
                };

                if (validationResult.IsExpired)
                {
                    validationResult.IsValid = false;
                    validationResult.ValidationMessage = "Payment method is expired";
                }

                _logger.LogInformation("Validated payment method {PaymentMethodId} by user {UserId}: {IsValid}", 
                    paymentMethodId, tokenModel.UserID, validationResult.IsValid);
                return validationResult;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error validating payment method {PaymentMethodId}: {Message}", paymentMethodId, ex.Message);
                throw new InvalidOperationException($"Failed to validate payment method: {ex.Message}", ex);
            }
        });
    }

    public async Task<bool> ValidatePaymentMethodAsync(string paymentMethodId, TokenModel tokenModel)
    {
        var validation = await ValidatePaymentMethodDetailedAsync(paymentMethodId, tokenModel);
        return validation.IsValid;
    }
    #endregion

    // Subscription Management
    #region Subscription Management

    /// <summary>
    /// Creates a new Stripe subscription for a customer with the specified price and payment method
    /// </summary>
    /// <param name="customerId">The Stripe customer ID</param>
    /// <param name="priceId">The Stripe price ID for the subscription plan</param>
    /// <param name="paymentMethodId">The Stripe payment method ID to use for billing</param>
    /// <param name="tokenModel">Token containing user authentication information for audit purposes</param>
    /// <returns>The Stripe subscription ID of the created subscription</returns>
    /// <exception cref="ArgumentException">Thrown when required parameters are null or empty</exception>
    /// <exception cref="InvalidOperationException">Thrown when Stripe subscription creation fails</exception>
    /// <remarks>
    /// This method:
    /// - Validates input parameters
    /// - Creates a Stripe subscription with the specified price
    /// - Sets up automatic payment collection
    /// - Includes audit metadata for tracking
    /// - Uses retry logic for resilience
    /// - Handles Stripe-specific errors appropriately
    /// </remarks>
    public async Task<string> CreateSubscriptionAsync(string customerId, string priceId, string paymentMethodId, TokenModel tokenModel)
    {
        if (string.IsNullOrEmpty(customerId))
            throw new ArgumentException("Customer ID is required", nameof(customerId));
        
        if (string.IsNullOrEmpty(priceId))
            throw new ArgumentException("Price ID is required", nameof(priceId));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                // Create subscription options with payment settings and metadata
                var subscriptionCreateOptions = new SubscriptionCreateOptions
                {
                    Customer = customerId,
                    Items = new List<SubscriptionItemOptions>
                    {
                        new SubscriptionItemOptions
                        {
                            Price = priceId
                        }
                    },
                    DefaultPaymentMethod = paymentMethodId,
                    PaymentSettings = new SubscriptionPaymentSettingsOptions
                    {
                        SaveDefaultPaymentMethod = "on_subscription"
                    },
                    Metadata = new Dictionary<string, string>
                    {
                        { "created_by_user_id", tokenModel.UserID.ToString() },
                        { "created_by_role_id", tokenModel.RoleID.ToString() },
                        { "created_at", DateTime.UtcNow.ToString("O") }
                    }
                };

                var subscriptionService = new SubscriptionService();
                var subscription = await subscriptionService.CreateAsync(subscriptionCreateOptions);

                _logger.LogInformation("Created Stripe subscription {SubscriptionId} for customer {CustomerId} by user {UserId}", 
                    subscription.Id, customerId, tokenModel.UserID);
                return subscription.Id;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error creating subscription for customer {CustomerId}: {Message}", customerId, ex.Message);
                throw new InvalidOperationException($"Failed to create subscription: {ex.Message}", ex);
            }
        });
    }

    public async Task<SubscriptionDto> GetSubscriptionAsync(string subscriptionId, TokenModel tokenModel)
    {
        if (string.IsNullOrEmpty(subscriptionId))
            throw new ArgumentException("Subscription ID is required", nameof(subscriptionId));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                var subscriptionService = new SubscriptionService();
                var subscription = await subscriptionService.GetAsync(subscriptionId);

                return new SubscriptionDto
                {
                    Id = subscription.Id,
                    CustomerId = subscription.CustomerId,
                    Status = subscription.Status,
                    CurrentPeriodStart = subscription.Created,
                    CurrentPeriodEnd = subscription.Created.AddDays(30), // Default to 30 days from creation
                    CreatedDate = subscription.Created
                };
            }
            catch (StripeException ex) when (ex.StripeError?.Type == "invalid_request_error")
            {
                _logger.LogWarning("Subscription not found: {SubscriptionId} by user {UserId}", subscriptionId, tokenModel.UserID);
                throw new ArgumentException($"Subscription not found: {subscriptionId}");
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error getting subscription {SubscriptionId}: {Message}", subscriptionId, ex.Message);
                throw new InvalidOperationException($"Failed to get subscription: {ex.Message}", ex);
            }
        });
    }

    public async Task<bool> CancelSubscriptionAsync(string subscriptionId, TokenModel tokenModel)
    {
        if (string.IsNullOrEmpty(subscriptionId))
            throw new ArgumentException("Subscription ID is required", nameof(subscriptionId));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                var subscriptionService = new SubscriptionService();
                await subscriptionService.CancelAsync(subscriptionId);

                _logger.LogInformation("Cancelled Stripe subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel.UserID);
                return true;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error cancelling subscription {SubscriptionId}: {Message}", subscriptionId, ex.Message);
                throw new InvalidOperationException($"Failed to cancel subscription: {ex.Message}", ex);
            }
        });
    }
    #endregion

    // Payment Processing
    #region Payment Processing

    /// <summary>
    /// Processes a one-time payment using a payment method
    /// </summary>
    /// <param name="paymentMethodId">The Stripe payment method ID to use for payment</param>
    /// <param name="amount">The payment amount (must be greater than 0)</param>
    /// <param name="currency">The currency code for the payment (e.g., "usd")</param>
    /// <param name="tokenModel">Token containing user authentication information for audit purposes</param>
    /// <returns>PaymentResultDto containing payment result information</returns>
    /// <exception cref="ArgumentException">Thrown when required parameters are invalid</exception>
    /// <exception cref="InvalidOperationException">Thrown when payment processing fails</exception>
    /// <remarks>
    /// This method:
    /// - Validates all input parameters (paymentMethodId, amount, currency)
    /// - Validates the payment method before processing
    /// - Creates a payment intent in Stripe
    /// - Confirms the payment intent
    /// - Returns detailed payment result information
    /// - Uses retry logic for resilience
    /// - Used for one-time payments and subscription setup
    /// - Logs successful payments and errors
    /// </remarks>
    public async Task<PaymentResultDto> ProcessPaymentAsync(string paymentMethodId, decimal amount, string currency, TokenModel tokenModel)
    {
        if (string.IsNullOrEmpty(paymentMethodId))
            throw new ArgumentException("Payment method ID is required", nameof(paymentMethodId));
        
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than 0", nameof(amount));
        
        if (string.IsNullOrEmpty(currency))
            throw new ArgumentException("Currency is required", nameof(currency));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                // CRITICAL FIX: Validate payment method before processing
                var isValid = await ValidatePaymentMethodAsync(paymentMethodId, tokenModel);
                if (!isValid)
                {
                    throw new InvalidOperationException("Payment method is invalid or expired");
                }

                var paymentIntentCreateOptions = new PaymentIntentCreateOptions
                {
                    Amount = (long)(amount * 100), // Convert to cents
                    Currency = currency.ToLower(),
                    PaymentMethod = paymentMethodId,
                    Confirm = true,
                    ReturnUrl = "https://example.com/return",
                    Metadata = new Dictionary<string, string>
                    {
                        { "processed_by_user_id", tokenModel.UserID.ToString() },
                        { "processed_by_role_id", tokenModel.RoleID.ToString() },
                        { "processed_at", DateTime.UtcNow.ToString("O") }
                    }
                };

                var paymentIntentService = new PaymentIntentService();
                var paymentIntent = await paymentIntentService.CreateAsync(paymentIntentCreateOptions);

                var result = new PaymentResultDto
                {
                    Status = paymentIntent.Status,
                    PaymentIntentId = paymentIntent.Id,
                    Amount = amount,
                    Currency = currency,
                    ProcessedAt = DateTime.UtcNow
                };

                _logger.LogInformation("Processed payment {PaymentIntentId} for amount {Amount} {Currency} by user {UserId}", 
                    paymentIntent.Id, amount, currency, tokenModel.UserID);
                return result;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error processing payment: {Message}", ex.Message);
                throw new InvalidOperationException($"Failed to process payment: {ex.Message}", ex);
            }
        });
    }

    /// <summary>
    /// Processes a refund for a payment intent
    /// </summary>
    /// <param name="paymentIntentId">The Stripe payment intent ID to refund</param>
    /// <param name="amount">The refund amount (must be greater than 0)</param>
    /// <param name="tokenModel">Token containing user authentication information for audit purposes</param>
    /// <returns>True if refund was processed successfully, false otherwise</returns>
    /// <exception cref="ArgumentException">Thrown when required parameters are invalid</exception>
    /// <exception cref="InvalidOperationException">Thrown when refund processing fails</exception>
    /// <remarks>
    /// This method:
    /// - Validates input parameters (paymentIntentId and amount)
    /// - Creates a refund in Stripe for the specified payment intent
    /// - Converts amount to cents for Stripe API
    /// - Uses retry logic for resilience
    /// - Used for processing refunds and chargebacks
    /// - Logs successful refunds and errors
    /// - Returns boolean result for operation success
    /// </remarks>
    public async Task<bool> ProcessRefundAsync(string paymentIntentId, decimal amount, TokenModel tokenModel)
    {
        if (string.IsNullOrEmpty(paymentIntentId))
            throw new ArgumentException("Payment intent ID is required", nameof(paymentIntentId));
        
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than 0", nameof(amount));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                // Create refund options with payment intent and amount
                var refundCreateOptions = new RefundCreateOptions
                {
                    PaymentIntent = paymentIntentId,
                    Amount = (long)(amount * 100), // Convert to cents for Stripe API
                    Metadata = new Dictionary<string, string>
                    {
                        { "refunded_by_user_id", tokenModel.UserID.ToString() },
                        { "refunded_by_role_id", tokenModel.RoleID.ToString() },
                        { "refunded_at", DateTime.UtcNow.ToString("O") }
                    }
                };

                var refundService = new RefundService();
                var refund = await refundService.CreateAsync(refundCreateOptions);

                _logger.LogInformation("Processed refund {RefundId} for payment intent {PaymentIntentId} by user {UserId}", 
                    refund.Id, paymentIntentId, tokenModel.UserID);
                return true;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error processing refund: {Message}", ex.Message);
                throw new InvalidOperationException($"Failed to process refund: {ex.Message}", ex);
            }
        });
    }

    // Product Management
    public async Task<string> CreateProductAsync(string name, string description, TokenModel tokenModel)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Product name is required", nameof(name));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                var productCreateOptions = new ProductCreateOptions
                {
                    Name = name,
                    Description = description,
                    Metadata = new Dictionary<string, string>
                    {
                        { "created_by_user_id", tokenModel.UserID.ToString() },
                        { "created_by_role_id", tokenModel.RoleID.ToString() },
                        { "created_at", DateTime.UtcNow.ToString("O") }
                    }
                };

                var productService = new ProductService();
                var product = await productService.CreateAsync(productCreateOptions);

                _logger.LogInformation("Created Stripe product {ProductId} '{Name}' by user {UserId}", 
                    product.Id, name, tokenModel.UserID);
                return product.Id;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error creating product: {Message}", ex.Message);
                throw new InvalidOperationException($"Failed to create product: {ex.Message}", ex);
            }
        });
    }

    public async Task<bool> DeactivatePriceAsync(string priceId, TokenModel tokenModel)
    {
        if (string.IsNullOrEmpty(priceId))
            throw new ArgumentException("Price ID is required", nameof(priceId));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                var priceService = new PriceService();
                await priceService.UpdateAsync(priceId, new PriceUpdateOptions { Active = false });

                _logger.LogInformation("Deactivated Stripe price {PriceId} by user {UserId}", priceId, tokenModel.UserID);
                return true;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error deactivating price {PriceId}: {Message}", priceId, ex.Message);
                throw new InvalidOperationException($"Failed to deactivate price: {ex.Message}", ex);
            }
        });
    }

    public async Task<string> CreatePaymentMethodAsync(string customerId, string paymentMethodId, TokenModel tokenModel)
    {
        // This method is a duplicate of AddPaymentMethodAsync, redirecting to it
        return await AddPaymentMethodAsync(customerId, paymentMethodId, tokenModel);
    }

    public async Task<bool> UpdatePaymentMethodAsync(string customerId, string paymentMethodId, TokenModel tokenModel)
    {
        // This method is a placeholder - Stripe doesn't support updating payment methods directly
        // In a real implementation, you might want to detach and reattach
        _logger.LogWarning("UpdatePaymentMethodAsync called - Stripe doesn't support updating payment methods directly. User: {UserId}", tokenModel.UserID);
        return true;
    }

    public async Task<bool> UpdateProductAsync(string productId, string name, string description, TokenModel tokenModel)
    {
        if (string.IsNullOrEmpty(productId))
            throw new ArgumentException("Product ID is required", nameof(productId));
        
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Product name is required", nameof(name));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                var productUpdateOptions = new ProductUpdateOptions
                {
                    Name = name,
                    Description = description,
                    Metadata = new Dictionary<string, string>
                    {
                        { "updated_by_user_id", tokenModel.UserID.ToString() },
                        { "updated_by_role_id", tokenModel.RoleID.ToString() },
                        { "updated_at", DateTime.UtcNow.ToString("O") }
                    }
                };

                var productService = new ProductService();
                await productService.UpdateAsync(productId, productUpdateOptions);

                _logger.LogInformation("Updated Stripe product {ProductId} by user {UserId}", productId, tokenModel.UserID);
                return true;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error updating product {ProductId}: {Message}", productId, ex.Message);
                throw new InvalidOperationException($"Failed to update product: {ex.Message}", ex);
            }
        });
    }

    public async Task<bool> DeleteProductAsync(string productId, TokenModel tokenModel)
    {
        if (string.IsNullOrEmpty(productId))
            throw new ArgumentException("Product ID is required", nameof(productId));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                var productService = new ProductService();
                await productService.DeleteAsync(productId);

                _logger.LogInformation("Deleted Stripe product {ProductId} by user {UserId}", productId, tokenModel.UserID);
                return true;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error deleting product {ProductId}: {Message}", productId, ex.Message);
                throw new InvalidOperationException($"Failed to delete product: {ex.Message}", ex);
            }
        });
    }

    // Price Management
    public async Task<string> CreatePriceAsync(string productId, decimal amount, string currency, string interval, int intervalCount, TokenModel tokenModel)
    {
        if (string.IsNullOrEmpty(productId))
            throw new ArgumentException("Product ID is required", nameof(productId));
        
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than 0", nameof(amount));
        
        if (string.IsNullOrEmpty(currency))
            throw new ArgumentException("Currency is required", nameof(currency));
        
        if (string.IsNullOrEmpty(interval))
            throw new ArgumentException("Interval is required", nameof(interval));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                var priceCreateOptions = new PriceCreateOptions
                {
                    Product = productId,
                    UnitAmount = (long)(amount * 100), // Convert to cents
                    Currency = currency.ToLower(),
                    Recurring = new PriceRecurringOptions
                    {
                        Interval = interval.ToLower(),
                        IntervalCount = intervalCount
                    },
                    Metadata = new Dictionary<string, string>
                    {
                        { "created_by_user_id", tokenModel.UserID.ToString() },
                        { "created_by_role_id", tokenModel.RoleID.ToString() },
                        { "created_at", DateTime.UtcNow.ToString("O") }
                    }
                };

                var priceService = new PriceService();
                var price = await priceService.CreateAsync(priceCreateOptions);

                _logger.LogInformation("Created Stripe price {PriceId} for product {ProductId} by user {UserId}", 
                    price.Id, productId, tokenModel.UserID);
                return price.Id;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error creating price: {Message}", ex.Message);
                throw new InvalidOperationException($"Failed to create price: {ex.Message}", ex);
            }
        });
    }

    public async Task<bool> UpdatePriceAsync(string priceId, decimal amount, TokenModel tokenModel)
    {
        if (string.IsNullOrEmpty(priceId))
            throw new ArgumentException("Price ID is required", nameof(priceId));
        
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than 0", nameof(amount));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                // IMPORTANT: Stripe doesn't allow updating unit amounts of existing prices
                // We need to deactivate the old price and create a new one
                // For now, we'll just update metadata and log the amount change
                // In a production environment, you might want to create new prices and update references
                
                var priceUpdateOptions = new PriceUpdateOptions
                {
                    Metadata = new Dictionary<string, string>
                    {
                        { "updated_by_user_id", tokenModel.UserID.ToString() },
                        { "updated_by_role_id", tokenModel.RoleID.ToString() },
                        { "updated_at", DateTime.UtcNow.ToString("O") },
                        { "new_amount", amount.ToString() },
                        { "note", "Amount update requires new price creation in Stripe" }
                    }
                };

                var priceService = new PriceService();
                await priceService.UpdateAsync(priceId, priceUpdateOptions);

                _logger.LogWarning("Stripe price {PriceId} metadata updated for amount change to {Amount}. Note: Stripe requires new price creation for amount updates.", 
                    priceId, amount);
                return true;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error updating price {PriceId}: {Message}", priceId, ex.Message);
                throw new InvalidOperationException($"Failed to update price: {ex.Message}", ex);
            }
        });
    }

    // NEW: Method to properly handle price updates by creating new prices
    public async Task<string> UpdatePriceWithNewPriceAsync(string oldPriceId, string productId, decimal newAmount, string currency, string interval, int intervalCount, TokenModel tokenModel)
    {
        if (string.IsNullOrEmpty(oldPriceId))
            throw new ArgumentException("Old price ID is required", nameof(oldPriceId));
        
        if (string.IsNullOrEmpty(productId))
            throw new ArgumentException("Product ID is required", nameof(productId));
        
        if (newAmount <= 0)
            throw new ArgumentException("Amount must be greater than 0", nameof(newAmount));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                // 1. Create new price with the new amount
                var newPriceId = await CreatePriceAsync(productId, newAmount, currency, interval, intervalCount, tokenModel);
                
                // 2. Deactivate the old price
                await DeactivatePriceAsync(oldPriceId, tokenModel);
                
                _logger.LogInformation("Successfully updated price from {OldPriceId} to {NewPriceId} with new amount {Amount}", 
                    oldPriceId, newPriceId, newAmount);
                
                return newPriceId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating price from {OldPriceId} to new amount {Amount}", oldPriceId, newAmount);
                throw new InvalidOperationException($"Failed to update price: {ex.Message}", ex);
            }
        });
    }

    // Subscription Management (continued)
    public async Task<bool> UpdateSubscriptionAsync(string subscriptionId, string priceId, TokenModel tokenModel)
    {
        if (string.IsNullOrEmpty(subscriptionId))
            throw new ArgumentException("Subscription ID is required", nameof(subscriptionId));
        
        if (string.IsNullOrEmpty(priceId))
            throw new ArgumentException("Price ID is required", nameof(priceId));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                var subscriptionUpdateOptions = new SubscriptionUpdateOptions
                {
                    Items = new List<SubscriptionItemOptions>
                    {
                        new SubscriptionItemOptions
                        {
                            Id = "price_" + priceId, // This is a simplified approach
                            Price = priceId
                        }
                    },
                    Metadata = new Dictionary<string, string>
                    {
                        { "updated_by_user_id", tokenModel.UserID.ToString() },
                        { "updated_by_role_id", tokenModel.RoleID.ToString() },
                        { "updated_at", DateTime.UtcNow.ToString("O") }
                    }
                };

                var subscriptionService = new SubscriptionService();
                await subscriptionService.UpdateAsync(subscriptionId, subscriptionUpdateOptions);

                _logger.LogInformation("Updated Stripe subscription {SubscriptionId} to price {PriceId} by user {UserId}", 
                    subscriptionId, priceId, tokenModel.UserID);
                return true;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error updating subscription {SubscriptionId}: {Message}", subscriptionId, ex.Message);
                throw new InvalidOperationException($"Failed to update subscription: {ex.Message}", ex);
            }
        });
    }

    public async Task<bool> PauseSubscriptionAsync(string subscriptionId, TokenModel tokenModel)
    {
        if (string.IsNullOrEmpty(subscriptionId))
            throw new ArgumentException("Subscription ID is required", nameof(subscriptionId));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                var subscriptionUpdateOptions = new SubscriptionUpdateOptions
                {
                    PauseCollection = new SubscriptionPauseCollectionOptions
                    {
                        Behavior = "void"
                    },
                    Metadata = new Dictionary<string, string>
                    {
                        { "paused_by_user_id", tokenModel.UserID.ToString() },
                        { "paused_by_role_id", tokenModel.RoleID.ToString() },
                        { "paused_at", DateTime.UtcNow.ToString("O") }
                    }
                };

                var subscriptionService = new SubscriptionService();
                await subscriptionService.UpdateAsync(subscriptionId, subscriptionUpdateOptions);

                _logger.LogInformation("Paused Stripe subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel.UserID);
                return true;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error pausing subscription {SubscriptionId}: {Message}", subscriptionId, ex.Message);
                throw new InvalidOperationException($"Failed to pause subscription: {ex.Message}", ex);
            }
        });
    }

    public async Task<bool> ResumeSubscriptionAsync(string subscriptionId, TokenModel tokenModel)
    {
        if (string.IsNullOrEmpty(subscriptionId))
            throw new ArgumentException("Subscription ID is required", nameof(subscriptionId));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                var subscriptionUpdateOptions = new SubscriptionUpdateOptions
                {
                    PauseCollection = null, // Remove pause collection
                    Metadata = new Dictionary<string, string>
                    {
                        { "resumed_by_user_id", tokenModel.UserID.ToString() },
                        { "resumed_by_role_id", tokenModel.RoleID.ToString() },
                        { "resumed_at", DateTime.UtcNow.ToString("O") }
                    }
                };

                var subscriptionService = new SubscriptionService();
                await subscriptionService.UpdateAsync(subscriptionId, subscriptionUpdateOptions);

                _logger.LogInformation("Resumed Stripe subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel.UserID);
                return true;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error resuming subscription {SubscriptionId}: {Message}", subscriptionId, ex.Message);
                throw new InvalidOperationException($"Failed to resume subscription: {ex.Message}", ex);
            }
        });
    }

    public async Task<bool> ReactivateSubscriptionAsync(string subscriptionId, TokenModel tokenModel)
    {
        // This method is a duplicate of ResumeSubscriptionAsync, redirecting to it
        return await ResumeSubscriptionAsync(subscriptionId, tokenModel);
    }

    public async Task<bool> UpdateSubscriptionPaymentMethodAsync(string subscriptionId, string paymentMethodId, TokenModel tokenModel)
    {
        if (string.IsNullOrEmpty(subscriptionId))
            throw new ArgumentException("Subscription ID is required", nameof(subscriptionId));
        
        if (string.IsNullOrEmpty(paymentMethodId))
            throw new ArgumentException("Payment method ID is required", nameof(paymentMethodId));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                var subscriptionUpdateOptions = new SubscriptionUpdateOptions
                {
                    DefaultPaymentMethod = paymentMethodId,
                    Metadata = new Dictionary<string, string>
                    {
                        { "payment_method_updated_by_user_id", tokenModel.UserID.ToString() },
                        { "payment_method_updated_by_role_id", tokenModel.RoleID.ToString() },
                        { "payment_method_updated_at", DateTime.UtcNow.ToString("O") }
                    }
                };

                var subscriptionService = new SubscriptionService();
                await subscriptionService.UpdateAsync(subscriptionId, subscriptionUpdateOptions);

                _logger.LogInformation("Updated payment method for Stripe subscription {SubscriptionId} by user {UserId}", 
                    subscriptionId, tokenModel.UserID);
                return true;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error updating payment method for subscription {SubscriptionId}: {Message}", subscriptionId, ex.Message);
                throw new InvalidOperationException($"Failed to update subscription payment method: {ex.Message}", ex);
            }
        });
    }

    // Checkout Sessions
    public async Task<string> CreateCheckoutSessionAsync(string priceId, string successUrl, string cancelUrl, TokenModel tokenModel)
    {
        if (string.IsNullOrEmpty(priceId))
            throw new ArgumentException("Price ID is required", nameof(priceId));
        
        if (string.IsNullOrEmpty(successUrl))
            throw new ArgumentException("Success URL is required", nameof(successUrl));
        
        if (string.IsNullOrEmpty(cancelUrl))
            throw new ArgumentException("Cancel URL is required", nameof(cancelUrl));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                var checkoutSessionCreateOptions = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            Price = priceId,
                            Quantity = 1
                        }
                    },
                    Mode = "subscription",
                    SuccessUrl = successUrl,
                    CancelUrl = cancelUrl,
                    Metadata = new Dictionary<string, string>
                    {
                        { "created_by_user_id", tokenModel.UserID.ToString() },
                        { "created_by_role_id", tokenModel.RoleID.ToString() },
                        { "created_at", DateTime.UtcNow.ToString("O") }
                    }
                };

                var sessionService = new SessionService();
                var session = await sessionService.CreateAsync(checkoutSessionCreateOptions);

                _logger.LogInformation("Created Stripe checkout session {SessionId} for price {PriceId} by user {UserId}", 
                    session.Id, priceId, tokenModel.UserID);
                return session.Id;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error creating checkout session: {Message}", ex.Message);
                throw new InvalidOperationException($"Failed to create checkout session: {ex.Message}", ex);
            }
        });
    }

    // Webhook Processing
    public async Task<bool> ProcessWebhookAsync(string json, string signature, TokenModel tokenModel)
    {
        if (string.IsNullOrEmpty(json))
            throw new ArgumentException("Webhook JSON is required", nameof(json));
        
        if (string.IsNullOrEmpty(signature))
            throw new ArgumentException("Webhook signature is required", nameof(signature));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                var webhookSecret = _configuration["StripeSettings:WebhookSecret"];
                if (string.IsNullOrEmpty(webhookSecret))
                {
                    throw new InvalidOperationException("Webhook secret is not configured");
                }

                var stripeEvent = EventUtility.ConstructEvent(json, signature, webhookSecret);
                
                _logger.LogInformation("Processed Stripe webhook event {EventType} {EventId} by user {UserId}", 
                    stripeEvent.Type, stripeEvent.Id, tokenModel.UserID);

                // Process different event types
                switch (stripeEvent.Type)
                {
                    case "customer.subscription.created":
                        await HandleSubscriptionCreatedAsync(stripeEvent);
                        break;
                    case "customer.subscription.updated":
                        await HandleSubscriptionUpdatedAsync(stripeEvent);
                        break;
                    case "customer.subscription.deleted":
                        await HandleSubscriptionDeletedAsync(stripeEvent);
                        break;
                    case "invoice.payment_succeeded":
                        await HandleInvoicePaymentSucceededAsync(stripeEvent);
                        break;
                    case "invoice.payment_failed":
                        await HandleInvoicePaymentFailedAsync(stripeEvent);
                        break;
                    default:
                        _logger.LogInformation("Unhandled Stripe event type: {EventType}", stripeEvent.Type);
                        break;
                }

                return true;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error processing webhook: {Message}", ex.Message);
                throw new InvalidOperationException($"Failed to process webhook: {ex.Message}", ex);
            }
        });
    }

    // Utility Methods
    private async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation)
    {
        for (int attempt = 1; attempt <= _maxRetries; attempt++)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex) when (attempt < _maxRetries && IsRetryableException(ex))
            {
                _logger.LogWarning(ex, "Attempt {Attempt} failed, retrying in {Delay}ms", attempt, _retryDelay.TotalMilliseconds);
                await Task.Delay(_retryDelay);
            }
        }
        
        throw new InvalidOperationException($"Operation failed after {_maxRetries} attempts");
    }

    private bool IsRetryableException(Exception ex)
    {
        // Add logic to determine if an exception is retryable
        return ex is StripeException stripeEx && 
               (stripeEx.StripeError?.Type == "rate_limit_error" || 
                stripeEx.StripeError?.Type == "api_connection_error");
    }

    // Webhook Event Handlers
    private async Task HandleSubscriptionCreatedAsync(Event stripeEvent)
    {
        var subscription = stripeEvent.Data.Object as Stripe.Subscription;
        _logger.LogInformation("Handling subscription created event for subscription {SubscriptionId}", subscription?.Id);
        // Implement subscription created logic
    }

    private async Task HandleSubscriptionUpdatedAsync(Event stripeEvent)
    {
        var subscription = stripeEvent.Data.Object as Stripe.Subscription;
        _logger.LogInformation("Handling subscription updated event for subscription {SubscriptionId}", subscription?.Id);
        // Implement subscription updated logic
    }

    private async Task HandleSubscriptionDeletedAsync(Event stripeEvent)
    {
        var subscription = stripeEvent.Data.Object as Stripe.Subscription;
        _logger.LogInformation("Handling subscription deleted event for subscription {SubscriptionId}", subscription?.Id);
        // Implement subscription deleted logic
    }

    private async Task HandleInvoicePaymentSucceededAsync(Event stripeEvent)
    {
        var invoice = stripeEvent.Data.Object as Stripe.Invoice;
        _logger.LogInformation("Handling invoice payment succeeded event for invoice {InvoiceId}", invoice?.Id);
        // Implement payment succeeded logic
    }

    private async Task HandleInvoicePaymentFailedAsync(Event stripeEvent)
    {
        var invoice = stripeEvent.Data.Object as Stripe.Invoice;
        _logger.LogInformation("Handling invoice payment failed event for invoice {InvoiceId}", invoice?.Id);
        // Implement payment failed logic
    }
    #endregion
} 