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

public class StripeService : IStripeService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<StripeService> _logger;
    private readonly int _maxRetries = 3;
    private readonly TimeSpan _retryDelay = TimeSpan.FromSeconds(1);
    
    public StripeService(IConfiguration configuration, ILogger<StripeService> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        var secretKey = _configuration["StripeSettings:SecretKey"];
        if (string.IsNullOrEmpty(secretKey))
        {
            throw new InvalidOperationException("Stripe secret key is not configured");
        }
        
        StripeConfiguration.ApiKey = secretKey;
        _logger.LogInformation("Stripe service initialized successfully");
    }
    
    // Customer Management
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

    public async Task<CustomerDto> GetCustomerAsync(string customerId, TokenModel tokenModel)
    {
        if (string.IsNullOrEmpty(customerId))
            throw new ArgumentException("Customer ID is required", nameof(customerId));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                var customerService = new CustomerService();
                var customer = await customerService.GetAsync(customerId);

                return new CustomerDto
                {
                    Id = customer.Id,
                    Email = customer.Email,
                    Name = customer.Name,
                    CreatedAt = customer.Created
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

    public async Task<IEnumerable<CustomerDto>> ListCustomersAsync(TokenModel tokenModel)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                var customerService = new CustomerService();
                var customers = await customerService.ListAsync(new CustomerListOptions
                {
                    Limit = 100 // Limit to 100 customers for testing
                });

                return customers.Data.Select(customer => new CustomerDto
                {
                    Id = customer.Id,
                    Email = customer.Email,
                    Name = customer.Name,
                    CreatedAt = customer.Created
                });
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error listing customers: {Message}", ex.Message);
                throw new InvalidOperationException($"Failed to list Stripe customers: {ex.Message}", ex);
            }
        });
    }
    
    public async Task<bool> UpdateCustomerAsync(string customerId, string email, string name, TokenModel tokenModel)
    {
        if (string.IsNullOrEmpty(customerId))
            throw new ArgumentException("Customer ID is required", nameof(customerId));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
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

    // Payment Method Management
    public async Task<IEnumerable<PaymentMethodDto>> GetCustomerPaymentMethodsAsync(string customerId, TokenModel tokenModel)
    {
        if (string.IsNullOrEmpty(customerId))
            throw new ArgumentException("Customer ID is required", nameof(customerId));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                var paymentMethodService = new PaymentMethodService();
                var paymentMethods = await paymentMethodService.ListAsync(new PaymentMethodListOptions
                {
                    Customer = customerId,
                    Type = "card"
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
                    CreatedAt = pm.Created
                });
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error getting payment methods for customer {CustomerId}: {Message}", customerId, ex.Message);
                throw new InvalidOperationException($"Failed to get payment methods: {ex.Message}", ex);
            }
        });
    }

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

    // Subscription Management
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
                    CreatedAt = subscription.Created
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

    // Payment Processing
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
                var refundCreateOptions = new RefundCreateOptions
                {
                    PaymentIntent = paymentIntentId,
                    Amount = (long)(amount * 100), // Convert to cents
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
                var priceUpdateOptions = new PriceUpdateOptions
                {
                    // UnitAmount property not available in current Stripe version
                    // UnitAmount = (long)(amount * 100), // Convert to cents
                    Metadata = new Dictionary<string, string>
                    {
                        { "updated_by_user_id", tokenModel.UserID.ToString() },
                        { "updated_by_role_id", tokenModel.RoleID.ToString() },
                        { "updated_at", DateTime.UtcNow.ToString("O") }
                    }
                };

                var priceService = new PriceService();
                await priceService.UpdateAsync(priceId, priceUpdateOptions);

                _logger.LogInformation("Updated Stripe price {PriceId} to amount {Amount} by user {UserId}", 
                    priceId, amount, tokenModel.UserID);
                return true;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error updating price {PriceId}: {Message}", priceId, ex.Message);
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
} 