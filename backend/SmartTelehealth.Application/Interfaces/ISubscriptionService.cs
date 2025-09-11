using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;

namespace SmartTelehealth.Application.Interfaces
{
    public interface ISubscriptionService
    {
        Task<JsonModel> GetSubscriptionAsync(string subscriptionId, TokenModel tokenModel);
        Task<JsonModel> GetUserSubscriptionsAsync(int userId, TokenModel tokenModel);
        Task<JsonModel> GetSubscriptionByPlanIdAsync(string planId, TokenModel tokenModel);
        Task<JsonModel> GetPaymentMethodsAsync(int userId, TokenModel tokenModel);
        Task<JsonModel> AddPaymentMethodAsync(int userId, string paymentMethodId, TokenModel tokenModel);
        
        // Missing methods from controllers
        Task<JsonModel> GetActiveSubscriptionsAsync(TokenModel tokenModel);
        Task<JsonModel> GetSubscriptionByIdAsync(string subscriptionId, TokenModel tokenModel);
        Task<JsonModel> GetBillingHistoryAsync(string subscriptionId, TokenModel tokenModel);
        Task<JsonModel> ProcessPaymentAsync(string subscriptionId, PaymentRequestDto paymentRequest, TokenModel tokenModel);
        Task<JsonModel> GetUsageStatisticsAsync(string subscriptionId, TokenModel tokenModel);
        Task<JsonModel> GetAllSubscriptionsAsync(TokenModel tokenModel);
        // Admin management methods
        Task<JsonModel> GetAllUserSubscriptionsAsync(int page, int pageSize, string? searchTerm, string[]? status, string[]? planId, string[]? userId, DateTime? startDate, DateTime? endDate, string? sortBy, string? sortOrder, TokenModel tokenModel);
        Task<JsonModel> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId, TokenModel tokenModel);
        
        
        // Category management
        Task<JsonModel> GetAllCategoriesAsync(int page, int pageSize, string? searchTerm, bool? isActive, TokenModel tokenModel);

        
        // Export and enhanced analytics methods
        
        // Analytics methods
        Task<JsonModel> GetSubscriptionAnalyticsAsync(string subscriptionId, DateTime? startDate = null, DateTime? endDate = null, TokenModel tokenModel = null);
        Task<JsonModel> HandleFailedPaymentAsync(string subscriptionId, string reason, TokenModel tokenModel);
        Task<JsonModel> CanUsePrivilegeAsync(string subscriptionId, string privilegeName, TokenModel tokenModel);
        
        // Additional service methods
        Task<JsonModel> BookConsultationAsync(int userId, Guid subscriptionId, TokenModel tokenModel);
        Task<JsonModel> RequestMedicationSupplyAsync(int userId, Guid subscriptionId, TokenModel tokenModel);
        Task<JsonModel> RetryPaymentAsync(string subscriptionId, PaymentRequestDto paymentRequest, TokenModel tokenModel);
        
        // Privilege usage tracking
        Task IncrementPrivilegeUsageAsync(string subscriptionId, string privilegeName);
    }
} 