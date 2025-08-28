namespace SmartTelehealth.Core.Interfaces;

public interface IAnalyticsRepository
{
    Task<object> GetAppointmentAnalyticsAsync();
    Task<object> GetRevenueAnalyticsAsync();
    Task<object> GetUserActivityAnalyticsAsync();
    Task<object> GetSubscriptionAnalyticsAsync();
    Task<object> GetProviderAnalyticsAsync();
} 