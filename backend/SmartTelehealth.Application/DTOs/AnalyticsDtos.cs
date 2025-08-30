using System;
using System.Collections.Generic;

namespace SmartTelehealth.Application.DTOs
{
    public class RevenueAnalyticsDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal AnnualRevenue { get; set; }
        public int TotalSubscriptions { get; set; }
        public int ActiveSubscriptions { get; set; }
        public int NewSubscriptionsThisMonth { get; set; }
        public int CancelledSubscriptionsThisMonth { get; set; }
        public decimal AverageRevenuePerSubscription { get; set; }
        public List<MonthlyRevenueData> MonthlyRevenueBreakdown { get; set; } = new List<MonthlyRevenueData>();
        public List<CategoryRevenueData> RevenueByCategory { get; set; } = new List<CategoryRevenueData>();
        public decimal TotalRefunds { get; set; }
        public decimal MonthlyRecurringRevenue { get; set; }
        public decimal AverageRevenuePerUser { get; set; }
        public decimal RevenueGrowth { get; set; }
        public List<PlanRevenueDto> RevenueByPlan { get; set; } = new List<PlanRevenueDto>();
    }

    public class MonthlyRevenueData
    {
        public string Month { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int Subscriptions { get; set; }
    }

    public class CategoryRevenueData
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int Subscriptions { get; set; }
    }

    public class UserActivityAnalyticsDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int NewUsersThisMonth { get; set; }
        public int UsersWithActiveSubscriptions { get; set; }
        public decimal AverageConsultationsPerUser { get; set; }
        public decimal AverageMessagesPerUser { get; set; }
        public List<UserActivityData> UserActivityBreakdown { get; set; } = new List<UserActivityData>();
        public List<UserTypeData> UsersByType { get; set; } = new List<UserTypeData>();
        public int TotalLogins { get; set; }
    }

    public class UserActivityData
    {
        public string Date { get; set; } = string.Empty;
        public int ActiveUsers { get; set; }
        public int Consultations { get; set; }
        public int Messages { get; set; }
    }

    public class UserTypeData
    {
        public string UserType { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    public class AppointmentAnalyticsDto
    {
        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public int PendingAppointments { get; set; }
        public decimal CompletionRate { get; set; }
        public decimal AverageAppointmentDuration { get; set; }
        public List<AppointmentData> AppointmentBreakdown { get; set; } = new List<AppointmentData>();
        public List<ProviderAppointmentData> AppointmentsByProvider { get; set; } = new List<ProviderAppointmentData>();
    }

    public class AppointmentData
    {
        public string Date { get; set; } = string.Empty;
        public int Total { get; set; }
        public int Completed { get; set; }
        public int Cancelled { get; set; }
    }

    public class ProviderAppointmentData
    {
        public string ProviderName { get; set; } = string.Empty;
        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public decimal CompletionRate { get; set; }
    }

    public class SubscriptionAnalyticsDto
    {
        public int TotalSubscriptions { get; set; }
        public int ActiveSubscriptions { get; set; }
        public int PausedSubscriptions { get; set; }
        public int CancelledSubscriptions { get; set; }
        public int NewSubscriptionsThisMonth { get; set; }
        public decimal ChurnRate { get; set; }
        public decimal AverageSubscriptionValue { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal YearlyRevenue { get; set; }
        public IEnumerable<CategoryAnalyticsDto> TopCategories { get; set; } = new List<CategoryAnalyticsDto>();
        public decimal MonthlyGrowth { get; set; }
        public Dictionary<string, int> SubscriptionsByPlan { get; set; } = new();
        public Dictionary<string, int> SubscriptionsByStatus { get; set; } = new();
        
        // Additional properties for individual subscription analytics
        public string SubscriptionId { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime NextBillingDate { get; set; }
        public decimal TotalAmountPaid { get; set; }
        public int PaymentCount { get; set; }
        public decimal AveragePaymentAmount { get; set; }
        public UsageStatisticsDto UsageStatistics { get; set; } = new();
        public List<PaymentHistoryDto> PaymentHistory { get; set; } = new();
    }

    public class BillingAnalyticsDto
    {
        public int TotalBillingRecords { get; set; } // Added
        public int PendingBillingRecords { get; set; } // Added
        public int PaidBillingRecords { get; set; } // Added
        public int FailedBillingRecords { get; set; } // Added
        public decimal TotalRevenue { get; set; }
        public decimal AverageBillingAmount { get; set; } // Added
        public List<MonthlyRevenueDto> MonthlyRevenue { get; set; } = new(); // Added
        public List<BillingStatusDto> BillingStatuses { get; set; } = new(); // Added
        public List<PaymentMethodDto> PaymentMethods { get; set; } = new(); // Added
        public decimal OutstandingAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public int TotalInvoices { get; set; }
        public int PaidInvoices { get; set; }
        public int PendingInvoices { get; set; } // Added missing property
        public int FailedInvoices { get; set; } // Added missing property
        public int OverdueInvoices { get; set; }
        public decimal AverageInvoiceAmount { get; set; } // Added missing property
        public decimal AveragePaymentTime { get; set; }
        public List<RevenueSourceDto> TopRevenueSources { get; set; } = new List<RevenueSourceDto>();
        public decimal MonthlyRecurringRevenue { get; set; }
        public decimal AverageRevenuePerUser { get; set; }
        public int FailedPayments { get; set; }
        public int RefundsIssued { get; set; }
        public decimal PaymentSuccessRate { get; set; }
        public IEnumerable<CategoryRevenueDto> RevenueByCategory { get; set; } = new List<CategoryRevenueDto>();
        public IEnumerable<RevenueTrendDto> RevenueTrend { get; set; } = new List<RevenueTrendDto>();
    }

    public class RevenueSourceDto
    {
        public string Source { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal Percentage { get; set; }
    }

    public class UserAnalyticsDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int NewUsersThisMonth { get; set; }
        public decimal UserRetentionRate { get; set; }
        public TimeSpan AverageUserLifetime { get; set; }
        public IEnumerable<CategoryAnalyticsDto> TopUserCategories { get; set; } = new List<CategoryAnalyticsDto>();
    }

    public class SystemHealthDto
    {
        public string DatabaseStatus { get; set; } = string.Empty;
        public string ApiStatus { get; set; } = string.Empty;
        public string PaymentGatewayStatus { get; set; } = string.Empty;
        public string EmailServiceStatus { get; set; } = string.Empty;
        public DateTime LastBackup { get; set; }
        public TimeSpan SystemUptime { get; set; }
        public int ActiveConnections { get; set; }
        public double MemoryUsage { get; set; }
        public double CpuUsage { get; set; }
    }

    public class CategoryAnalyticsDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public int SubscriptionCount { get; set; }
        public decimal Revenue { get; set; }
        public decimal GrowthRate { get; set; }
        
        // Added missing properties to fix build errors
        public Guid CategoryId { get; set; }
        public int TotalSubscriptions { get; set; }
        public int ActiveSubscriptions { get; set; }
    }

    public class CategoryRevenueDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        
        // Added missing properties to fix build errors
        public Guid CategoryId { get; set; }
        public int SubscriptionCount { get; set; }
    }

    public class RevenueTrendDto
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public decimal Growth { get; set; }
        
        // Added missing properties to fix build errors
        public string Period { get; set; } = string.Empty;
        public int Month { get; set; }
        public int Year { get; set; }
    }

    public class ProviderAnalyticsDto
    {
        public int TotalProviders { get; set; }
        public int ActiveProviders { get; set; }
        public decimal AverageProviderRating { get; set; }
        public int TotalConsultations { get; set; }
        public decimal AverageConsultationDuration { get; set; }
        public IEnumerable<ProviderPerformanceDto> TopPerformingProviders { get; set; } = new List<ProviderPerformanceDto>();
        public IEnumerable<ProviderWorkloadDto> ProviderWorkload { get; set; } = new List<ProviderWorkloadDto>();
    }

    public class ProviderPerformanceDto
    {
        public Guid ProviderId { get; set; }
        public string ProviderName { get; set; } = string.Empty;
        public int ConsultationsCompleted { get; set; }
        public decimal AverageRating { get; set; }
        public decimal Revenue { get; set; }
        public int PatientCount { get; set; }
    }

    public class ProviderWorkloadDto
    {
        public Guid ProviderId { get; set; }
        public string ProviderName { get; set; } = string.Empty;
        public int ScheduledConsultations { get; set; }
        public int CompletedConsultations { get; set; }
        public int PendingConsultations { get; set; }
        public decimal UtilizationRate { get; set; }
    }

    public class SystemAnalyticsDto
    {
        public SystemHealthDto SystemHealth { get; set; } = new();
        public int TotalApiCalls { get; set; }
        public int SuccessfulApiCalls { get; set; }
        public int FailedApiCalls { get; set; }
        public double AverageResponseTime { get; set; }
        public int ActiveConnections { get; set; }
        public double MemoryUsage { get; set; }
        public double CpuUsage { get; set; }
        public IEnumerable<ApiUsageDto> ApiUsage { get; set; } = new List<ApiUsageDto>();
        public IEnumerable<ErrorLogDto> ErrorLogs { get; set; } = new List<ErrorLogDto>();
    }

    public class ApiUsageDto
    {
        public string Endpoint { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public int CallCount { get; set; }
        public double AverageResponseTime { get; set; }
        public int ErrorCount { get; set; }
        public DateTime Date { get; set; }
    }

    public class ErrorLogDto
    {
        public string ErrorType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string StackTrace { get; set; } = string.Empty;
        public int UserId { get; set; }
        public DateTime Timestamp { get; set; }
        public string Endpoint { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
    }

    // Added missing DTOs for BillingAnalyticsDto
    public class MonthlyRevenueDto
    {
        public string Month { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int BillingCount { get; set; }
        public int PaymentCount { get; set; }
        public int InvoiceCount { get; set; } // Added for invoice analytics
    }

    public class BillingStatusDto
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
    }

    // Added missing DTOs for SubscriptionAnalyticsController
    public class UsageDistributionDto
    {
        public string Range { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class PeakUsageTimeDto
    {
        public int Hour { get; set; }
        public int UsageCount { get; set; }
    }

    public class UsageTrendDto
    {
        public DateTime Date { get; set; }
        public decimal AverageUsage { get; set; }
    }

    public class ForecastDto
    {
        public decimal NextMonthRevenue { get; set; }
        public int NextMonthSubscriptions { get; set; }
        public decimal GrowthRate { get; set; }
        public decimal Confidence { get; set; }
    }

    // Added missing DTO for RevenueAnalyticsDto
    public class PlanRevenueDto
    {
        public string PlanName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int SubscriptionCount { get; set; }
    }


} 