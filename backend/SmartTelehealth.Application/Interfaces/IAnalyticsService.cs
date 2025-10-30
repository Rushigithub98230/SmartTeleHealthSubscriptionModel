using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IAnalyticsService
{
    Task<JsonModel> GetRevenueAnalyticsAsync(DateTime? startDate, DateTime? endDate, TokenModel tokenModel);
    Task<JsonModel> GetUserActivityAnalyticsAsync(DateTime? startDate, DateTime? endDate, TokenModel tokenModel);
    Task<JsonModel> GetAppointmentAnalyticsAsync(DateTime? startDate, DateTime? endDate, TokenModel tokenModel);
    Task<JsonModel> GetSubscriptionAnalyticsAsync(DateTime? startDate, DateTime? endDate, TokenModel tokenModel);
    Task<JsonModel> GetSubscriptionAnalyticsAsync(DateTime? startDate, DateTime? endDate, string? planId, TokenModel tokenModel);
    Task<JsonModel> GetSystemAnalyticsAsync(TokenModel tokenModel);
    
    // Additional Analytics Methods
    Task<JsonModel> GetBillingAnalyticsAsync(DateTime? startDate, DateTime? endDate, TokenModel tokenModel);
    Task<JsonModel> GetUserAnalyticsAsync(DateTime? startDate, DateTime? endDate, TokenModel tokenModel);
    Task<JsonModel> GetProviderAnalyticsAsync(DateTime? startDate, DateTime? endDate, TokenModel tokenModel);
    Task<JsonModel> GetSystemHealthAsync(TokenModel tokenModel);
    
    // Subscription Analytics Methods
    Task<JsonModel> GetSubscriptionDashboardAsync(DateTime? startDate, DateTime? endDate, TokenModel tokenModel);
    Task<JsonModel> GetChurnAnalyticsAsync(DateTime? startDate, DateTime? endDate, TokenModel tokenModel);
    Task<JsonModel> GetPlanAnalyticsAsync(DateTime? startDate, DateTime? endDate, TokenModel tokenModel);
    Task<JsonModel> GetUsageAnalyticsAsync(DateTime? startDate, DateTime? endDate, TokenModel tokenModel);
    
    // Report Generation Methods
    Task<JsonModel> GenerateSubscriptionReportAsync(DateTime? startDate, DateTime? endDate, TokenModel tokenModel);
    Task<JsonModel> GenerateBillingReportAsync(DateTime? startDate, DateTime? endDate, TokenModel tokenModel);
    Task<JsonModel> GenerateUserReportAsync(DateTime? startDate, DateTime? endDate, TokenModel tokenModel);
    Task<JsonModel> GenerateProviderReportAsync(DateTime? startDate, DateTime? endDate, TokenModel tokenModel);
    Task<JsonModel> ExportSubscriptionAnalyticsAsync(DateTime? startDate, DateTime? endDate, TokenModel tokenModel);
    
    // Advanced Analytics Methods
    Task<ChurnAnalyticsDto> GetChurnAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<PrivilegeUsageAnalyticsDto> GetPrivilegeUsageAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<SubscriptionLifecycleAnalyticsDto> GetSubscriptionLifecycleAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    
    // Enhanced Billing Analytics
    Task<EnhancedBillingAnalyticsDto> GetEnhancedBillingAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    
    // Plan Migration Analytics
    Task<JsonModel> GetPlanMigrationAnalyticsAsync(TokenModel tokenModel);
    
    // Core Analytics Methods (for internal use)
    Task<decimal> GetTotalRevenueAsync(DateTime? startDate, DateTime? endDate, TokenModel tokenModel);
    Task<decimal> CalculateAverageRevenuePerUserAsync(TokenModel tokenModel);
    Task<int> GetFailedPaymentsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<int> GetRefundsIssuedAsync(DateTime? startDate, DateTime? endDate, TokenModel tokenModel);
    Task<decimal> CalculatePaymentSuccessRateAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<IEnumerable<CategoryRevenueDto>> GetRevenueByCategoryAsync(DateTime? startDate = null, DateTime? endDate = null, TokenModel tokenModel = null);
    Task<IEnumerable<RevenueTrendDto>> GetRevenueTrendAsync(DateTime? startDate = null, DateTime? endDate = null, TokenModel tokenModel = null);
    Task<decimal> GetMonthlyRecurringRevenueAsync(TokenModel tokenModel);
} 