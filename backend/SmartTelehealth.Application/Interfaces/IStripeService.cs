using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IStripeService
{
    // Customer Management
    Task<string> CreateCustomerAsync(string email, string name, TokenModel tokenModel);
    Task<CustomerDto> GetCustomerAsync(string customerId, TokenModel tokenModel);
    Task<IEnumerable<CustomerDto>> ListCustomersAsync(TokenModel tokenModel);
    
    // Payment Methods
    Task<string> CreatePaymentMethodAsync(string customerId, string paymentMethodId, TokenModel tokenModel);
    Task<bool> UpdatePaymentMethodAsync(string customerId, string paymentMethodId, TokenModel tokenModel);
    Task<string> AddPaymentMethodAsync(string customerId, string paymentMethodId, TokenModel tokenModel);
    Task<bool> SetDefaultPaymentMethodAsync(string customerId, string paymentMethodId, TokenModel tokenModel);
    Task<bool> RemovePaymentMethodAsync(string customerId, string paymentMethodId, TokenModel tokenModel);
    Task<IEnumerable<PaymentMethodDto>> GetCustomerPaymentMethodsAsync(string customerId, TokenModel tokenModel);
    Task<bool> ValidatePaymentMethodAsync(string paymentMethodId, TokenModel tokenModel);
    Task<PaymentMethodValidationDto> ValidatePaymentMethodDetailedAsync(string paymentMethodId, TokenModel tokenModel);
    
    // Product Management
    Task<string> CreateProductAsync(string name, string description, TokenModel tokenModel);
    Task<bool> UpdateProductAsync(string productId, string name, string description, TokenModel tokenModel);
    Task<bool> DeleteProductAsync(string productId, TokenModel tokenModel);
    
    // Price Management
    Task<string> CreatePriceAsync(string productId, decimal amount, string currency, string interval, int intervalCount, TokenModel tokenModel);
    Task<bool> UpdatePriceAsync(string priceId, decimal amount, TokenModel tokenModel);
    Task<bool> DeactivatePriceAsync(string priceId, TokenModel tokenModel);
    
    // Subscription Management
    Task<string> CreateSubscriptionAsync(string customerId, string priceId, string paymentMethodId, TokenModel tokenModel);
    Task<bool> CancelSubscriptionAsync(string subscriptionId, TokenModel tokenModel);
    Task<SubscriptionDto> GetSubscriptionAsync(string subscriptionId, TokenModel tokenModel);
    Task<bool> UpdateSubscriptionAsync(string subscriptionId, string priceId, TokenModel tokenModel);
    Task<bool> PauseSubscriptionAsync(string subscriptionId, TokenModel tokenModel);
    Task<bool> ResumeSubscriptionAsync(string subscriptionId, TokenModel tokenModel);
    
    // Payment Processing
    Task<PaymentResultDto> ProcessPaymentAsync(string paymentMethodId, decimal amount, string currency, TokenModel tokenModel);
    Task<bool> ProcessRefundAsync(string paymentIntentId, decimal amount, TokenModel tokenModel);
    
    // Checkout Sessions
    Task<string> CreateCheckoutSessionAsync(string priceId, string successUrl, string cancelUrl, TokenModel tokenModel);
    
    // Webhook Processing
    Task<bool> ProcessWebhookAsync(string json, string signature, TokenModel tokenModel);
} 