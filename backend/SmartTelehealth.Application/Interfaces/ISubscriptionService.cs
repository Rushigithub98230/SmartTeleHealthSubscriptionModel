using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;

namespace SmartTelehealth.Application.Interfaces
{
    public interface ISubscriptionService
    {
        Task<JsonModel> GetSubscriptionAsync(string subscriptionId, TokenModel tokenModel);
        Task<JsonModel> GetUserSubscriptionsAsync(int userId, TokenModel tokenModel);
        Task<JsonModel> CreateSubscriptionAsync(CreateSubscriptionDto createDto, TokenModel tokenModel);
        Task<JsonModel> CancelSubscriptionAsync(string subscriptionId, string? reason, TokenModel tokenModel);
        Task<JsonModel> PauseSubscriptionAsync(string subscriptionId, TokenModel tokenModel);
        Task<JsonModel> ResumeSubscriptionAsync(string subscriptionId, TokenModel tokenModel);
        Task<JsonModel> GetSubscriptionByPlanIdAsync(string planId, TokenModel tokenModel);
        Task<JsonModel> GetPaymentMethodsAsync(int userId, TokenModel tokenModel);
        Task<JsonModel> AddPaymentMethodAsync(int userId, string paymentMethodId, TokenModel tokenModel);
        
        // Missing methods from controllers
        Task<JsonModel> GetActiveSubscriptionsAsync(TokenModel tokenModel);
        Task<JsonModel> GetAllPlansAsync(TokenModel tokenModel);
        Task<JsonModel> GetAllPlansAsync(int page, int pageSize, string? searchTerm, string? categoryId, bool? isActive, TokenModel tokenModel);
        Task<JsonModel> GetPublicPlansAsync();
        Task<JsonModel> GetPlanByIdAsync(string planId, TokenModel tokenModel);
        Task<JsonModel> GetSubscriptionByIdAsync(string subscriptionId, TokenModel tokenModel);
        Task<JsonModel> UpdateSubscriptionAsync(string subscriptionId, UpdateSubscriptionDto updateDto, TokenModel tokenModel);
        Task<JsonModel> ReactivateSubscriptionAsync(string subscriptionId, TokenModel tokenModel);
        Task<JsonModel> UpgradeSubscriptionAsync(string subscriptionId, string newPlanId, TokenModel tokenModel);
        Task<JsonModel> GetBillingHistoryAsync(string subscriptionId, TokenModel tokenModel);
        Task<JsonModel> ProcessPaymentAsync(string subscriptionId, PaymentRequestDto paymentRequest, TokenModel tokenModel);
        Task<JsonModel> GetUsageStatisticsAsync(string subscriptionId, TokenModel tokenModel);
        Task<JsonModel> GetAllSubscriptionsAsync(TokenModel tokenModel);
        Task<JsonModel> GetSubscriptionAnalyticsAsync(string subscriptionId, TokenModel tokenModel);
        Task<JsonModel> CreatePlanAsync(CreateSubscriptionPlanDto createPlanDto, TokenModel tokenModel);
        Task<JsonModel> UpdatePlanAsync(string planId, UpdateSubscriptionPlanDto updatePlanDto, TokenModel tokenModel);
        Task<JsonModel> ActivatePlanAsync(string planId, TokenModel tokenModel);
        Task<JsonModel> DeactivatePlanAsync(string planId, TokenModel tokenModel);
        Task<JsonModel> DeletePlanAsync(string planId, TokenModel tokenModel);
        
        // Admin management methods
        Task<JsonModel> GetAllUserSubscriptionsAsync(int page, int pageSize, string? searchTerm, string[]? status, string[]? planId, string[]? userId, DateTime? startDate, DateTime? endDate, string? sortBy, string? sortOrder, TokenModel tokenModel);
        Task<JsonModel> CancelUserSubscriptionAsync(string subscriptionId, string? reason, TokenModel tokenModel);
        Task<JsonModel> PauseUserSubscriptionAsync(string subscriptionId, TokenModel tokenModel);
        Task<JsonModel> ResumeUserSubscriptionAsync(string subscriptionId, TokenModel tokenModel);
        Task<JsonModel> ExtendUserSubscriptionAsync(string subscriptionId, int additionalDays, TokenModel tokenModel);
        Task<JsonModel> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId, TokenModel tokenModel);
        
        // Bulk operations
        Task<JsonModel> PerformBulkActionAsync(List<BulkActionRequestDto> actions, TokenModel tokenModel);
        Task<JsonModel> GetAllSubscriptionPlansAsync(TokenModel tokenModel, string? searchTerm = null, string? categoryId = null, bool? isActive = null, int page = 1, int pageSize = 50);
        Task<JsonModel> GetActiveSubscriptionPlansAsync(TokenModel tokenModel);
        Task<JsonModel> GetSubscriptionPlansByCategoryAsync(string category, TokenModel tokenModel);
        Task<JsonModel> GetSubscriptionPlanAsync(string planId, TokenModel tokenModel);
        
        // Category management
        Task<JsonModel> GetAllCategoriesAsync(int page, int pageSize, string? searchTerm, bool? isActive, TokenModel tokenModel);

        
        // Export and enhanced analytics methods
        Task<JsonModel> ExportSubscriptionPlansAsync(TokenModel tokenModel, string? searchTerm = null, string? categoryId = null, bool? isActive = null, string format = "csv");
        Task<JsonModel> ExportCategoriesAsync(TokenModel tokenModel, string? searchTerm = null, bool? isActive = null, string format = "csv");
        Task<JsonModel> GetSubscriptionAnalyticsAsync(TokenModel tokenModel, string? searchTerm = null, string? categoryId = null, bool? isActive = null);
        
        // Analytics methods
        Task<JsonModel> GetSubscriptionAnalyticsAsync(string subscriptionId, DateTime? startDate, DateTime? endDate, TokenModel tokenModel);
        Task<JsonModel> UpdateSubscriptionPlanAsync(string planId, UpdateSubscriptionPlanDto updateDto, TokenModel tokenModel);
        Task<JsonModel> DeleteSubscriptionPlanAsync(string planId, TokenModel tokenModel);
        Task<JsonModel> HandleFailedPaymentAsync(string subscriptionId, string reason, TokenModel tokenModel);
        Task<JsonModel> CanUsePrivilegeAsync(string subscriptionId, string privilegeName, TokenModel tokenModel);
        Task<JsonModel> DeactivatePlanAsync(string planId, string adminUserId, TokenModel tokenModel);
        Task<JsonModel> HandlePaymentProviderWebhookAsync(string eventType, string subscriptionId, string? errorMessage, TokenModel tokenModel);
        
        // Privilege Management Methods
        Task<JsonModel> AssignPrivilegesToPlanAsync(Guid planId, List<PlanPrivilegeDto> privileges, TokenModel tokenModel);
        Task<JsonModel> RemovePrivilegeFromPlanAsync(Guid planId, Guid privilegeId, TokenModel tokenModel);
        Task<JsonModel> UpdatePlanPrivilegeAsync(Guid planId, Guid privilegeId, PlanPrivilegeDto privilegeDto, TokenModel tokenModel);
        Task<JsonModel> GetPlanPrivilegesAsync(Guid planId, TokenModel tokenModel);
    }
} 