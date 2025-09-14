using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;

namespace SmartTelehealth.Application.Interfaces;

/// <summary>
/// Interface for Stripe-specific billing operations.
/// This interface defines the contract for Stripe payment processing, webhook handling,
/// and Stripe-specific billing operations within the application.
/// </summary>
public interface IStripeBillingService
{
    // Stripe Payment Processing
    Task<JsonModel> ProcessStripePaymentAsync(Guid billingRecordId, TokenModel tokenModel);
    Task<JsonModel> ProcessStripeRefundAsync(Guid billingRecordId, decimal amount, TokenModel tokenModel);
    Task<JsonModel> RetryStripePaymentAsync(Guid billingRecordId, TokenModel tokenModel);
    Task<JsonModel> ProcessStripePartialPaymentAsync(Guid billingRecordId, decimal amount, TokenModel tokenModel);
    
    // Stripe Customer Management
    Task<JsonModel> GetStripeCustomerPaymentMethodsAsync(int userId, TokenModel tokenModel);
    Task<JsonModel> UpdateStripePaymentMethodAsync(Guid billingRecordId, string paymentMethodId, TokenModel tokenModel);
    
    // Stripe Recurring Billing
    Task<JsonModel> CreateStripeRecurringBillingAsync(CreateRecurringBillingDto createDto, TokenModel tokenModel);
    Task<JsonModel> ProcessStripeRecurringPaymentAsync(Guid subscriptionId, TokenModel tokenModel);
    Task<JsonModel> CancelStripeRecurringBillingAsync(Guid subscriptionId, TokenModel tokenModel);
    
    // Stripe Payment Types
    Task<JsonModel> CreateStripeUpfrontPaymentAsync(CreateUpfrontPaymentDto createDto, TokenModel tokenModel);
    Task<JsonModel> ProcessStripeBundlePaymentAsync(CreateBundlePaymentDto createDto, TokenModel tokenModel);
    
    // Stripe Invoice Management
    Task<JsonModel> CreateStripeInvoiceAsync(CreateInvoiceDto createDto, TokenModel tokenModel);
    Task<JsonModel> GenerateStripeInvoicePdfAsync(Guid billingRecordId, TokenModel tokenModel);
    
    // Stripe Webhook Processing
    Task<JsonModel> ProcessStripeWebhookAsync(string webhookPayload, string signature, TokenModel tokenModel);
    Task<JsonModel> HandleStripePaymentSucceededAsync(string paymentIntentId, TokenModel tokenModel);
    Task<JsonModel> HandleStripePaymentFailedAsync(string paymentIntentId, TokenModel tokenModel);
    
    // Stripe Analytics
    Task<JsonModel> GetStripePaymentAnalyticsAsync(int userId, DateTime? startDate = null, DateTime? endDate = null, TokenModel tokenModel = null);
    Task<JsonModel> GetStripeRevenueSummaryAsync(DateTime? from = null, DateTime? to = null, string? planId = null, TokenModel tokenModel = null);
}
