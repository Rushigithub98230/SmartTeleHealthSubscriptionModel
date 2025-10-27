using System;
using System.Collections.Generic;

namespace SmartTelehealth.Application.DTOs
{
    public class RevenueAnalyticsDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal AnnualRevenue { get; set; }
        public decimal AnnualRecurringRevenue { get; set; } // Added missing property
        public int TotalSubscriptions { get; set; }
        public int ActiveSubscriptions { get; set; }
        public int NewSubscriptionsThisMonth { get; set; }
        public int CancelledSubscriptionsThisMonth { get; set; }
        public decimal AverageRevenuePerSubscription { get; set; }
        public List<SmartTelehealth.Core.DTOs.MonthlyRevenueData> MonthlyRevenueBreakdown { get; set; } = new List<SmartTelehealth.Core.DTOs.MonthlyRevenueData>();
        public List<SmartTelehealth.Core.DTOs.CategoryRevenueData> RevenueByCategory { get; set; } = new List<SmartTelehealth.Core.DTOs.CategoryRevenueData>();
        public List<RevenueTrendDto> RevenueTrend { get; set; } = new List<RevenueTrendDto>(); // Added missing property
        public decimal TotalRefunds { get; set; }
        public decimal MonthlyRecurringRevenue { get; set; }
        public decimal AverageRevenuePerUser { get; set; }
        public decimal RevenueGrowth { get; set; }
        public List<PlanRevenueDto> RevenueByPlan { get; set; } = new List<PlanRevenueDto>();
        public DateRangeDto Period { get; set; } = new DateRangeDto(); // Added missing property
        public DateTime GeneratedAt { get; set; } // Added missing property
    }

    // MonthlyRevenueData and CategoryRevenueData moved to Core.DTOs to avoid duplication

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
        public int TrialSubscriptions { get; set; } // Added missing property
        public int NewSubscriptionsThisMonth { get; set; }
        public int NewSubscriptionsThisPeriod { get; set; } // Added missing property
        public int CancelledSubscriptionsThisPeriod { get; set; } // Added missing property
        public decimal ChurnRate { get; set; }
        public decimal AverageSubscriptionValue { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal YearlyRevenue { get; set; }
        public IEnumerable<CategoryAnalyticsDto> TopCategories { get; set; } = new List<CategoryAnalyticsDto>();
        public decimal MonthlyGrowth { get; set; }
        public Dictionary<string, int> SubscriptionsByPlan { get; set; } = new();
        public Dictionary<string, int> SubscriptionsByStatus { get; set; } = new();
        public DateRangeDto Period { get; set; } = new DateRangeDto(); // Added missing property
        public DateTime GeneratedAt { get; set; } // Added missing property
        
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
        public int TotalPayments { get; set; } // Added missing property
        public int SuccessfulPayments { get; set; } // Added missing property
        public int PendingPayments { get; set; } // Added missing property
        public decimal TotalRevenue { get; set; }
        public decimal AverageBillingAmount { get; set; } // Added
        public decimal AveragePaymentAmount { get; set; } // Added missing property
        public List<MonthlyBillingRevenueDto> MonthlyRevenue { get; set; } = new(); // Added
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
        public DateRangeDto Period { get; set; } = new DateRangeDto(); // Added missing property
        public DateTime GeneratedAt { get; set; } // Added missing property
    }

    public class RevenueSourceDto
    {
        public string Source { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal Percentage { get; set; }
    }

    public class AggregateUserAnalyticsDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; } // Added missing property
        public int NewUsersThisMonth { get; set; }
        public int NewUsersThisPeriod { get; set; } // Added missing property
        public decimal UserRetentionRate { get; set; }
        public decimal UserGrowthRate { get; set; } // Added missing property
        public decimal AverageUsage { get; set; } // Added missing property
        public TimeSpan AverageUserLifetime { get; set; }
        public IEnumerable<CategoryAnalyticsDto> TopUserCategories { get; set; } = new List<CategoryAnalyticsDto>();
        public DateRangeDto Period { get; set; } = new DateRangeDto(); // Added missing property
        public DateTime GeneratedAt { get; set; } // Added missing property
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
    public class MonthlyBillingRevenueDto
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

    /// <summary>
    /// Comprehensive churn analytics data transfer object
    /// </summary>
    public class ChurnAnalyticsDto
    {
        public int TotalChurnedSubscriptions { get; set; }
        public decimal ChurnRate { get; set; }
        public List<ChurnByPlanDto> ChurnByPlan { get; set; } = new();
        public List<ChurnByReasonDto> ChurnByReason { get; set; } = new();
        public List<ChurnTrendDto> ChurnTrend { get; set; } = new();
        public decimal RevenueLostToChurn { get; set; }
        public decimal AverageChurnTime { get; set; }
    }

    public class ChurnByPlanDto
    {
        public string PlanName { get; set; } = string.Empty;
        public int ChurnedCount { get; set; }
        public decimal ChurnRate { get; set; }
    }

    public class ChurnByReasonDto
    {
        public string Reason { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    public class ChurnTrendDto
    {
        public string Month { get; set; } = string.Empty;
        public int ChurnedCount { get; set; }
        public decimal ChurnRate { get; set; }
    }

    /// <summary>
    /// Comprehensive privilege usage analytics data transfer object
    /// </summary>
    public class PrivilegeUsageAnalyticsDto
    {
        public int TotalPrivilegeUsage { get; set; }
        public List<PrivilegeUsageDto> MostUsedPrivileges { get; set; } = new();
        public List<PrivilegeUsageDto> LeastUsedPrivileges { get; set; } = new();
        public List<UsageByPlanDto> UsageByPlan { get; set; } = new();
        public List<UsageTrendDto> UsageTrend { get; set; } = new();
        public OverageChargesDto OverageCharges { get; set; } = new();
        public decimal AverageUsagePerUser { get; set; }
    }

    public class PrivilegeUsageDto
    {
        public string PrivilegeName { get; set; } = string.Empty;
        public int UsageCount { get; set; }
        public decimal UsagePercentage { get; set; }
        public decimal AverageUsagePerUser { get; set; }
    }

    public class UsageByPlanDto
    {
        public string PlanName { get; set; } = string.Empty;
        public int TotalUsage { get; set; }
        public decimal AverageUsagePerSubscription { get; set; }
        public int SubscriptionCount { get; set; }
    }

    public class OverageChargesDto
    {
        public decimal TotalOverageCharges { get; set; }
        public int OverageCount { get; set; }
        public decimal AverageOverageAmount { get; set; }
        public List<SmartTelehealth.Core.DTOs.OverageByPlanDto> OverageByPlan { get; set; } = new();
    }

    public class OverageByPlanDto
    {
        public string PlanName { get; set; } = string.Empty;
        public decimal TotalCharges { get; set; }
        public int Count { get; set; }
    }

    /// <summary>
    /// Comprehensive subscription lifecycle analytics data transfer object
    /// </summary>
    public class SubscriptionLifecycleAnalyticsDto
    {
        public int TotalSubscriptions { get; set; }
        public List<StatusDistributionDto> StatusDistribution { get; set; } = new();
        public decimal AverageSubscriptionDuration { get; set; }
        public ConversionRatesDto ConversionRates { get; set; } = new();
        public List<LifecycleEventDto> LifecycleEvents { get; set; } = new();
        public List<RetentionRateDto> RetentionRates { get; set; } = new();
        public UpgradeDowngradeRatesDto UpgradeDowngradeRates { get; set; } = new();
    }

    public class StatusDistributionDto
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    public class ConversionRatesDto
    {
        public decimal TrialToActiveRate { get; set; }
        public decimal OverallActivationRate { get; set; }
        public decimal CancellationRate { get; set; }
    }

    public class LifecycleEventDto
    {
        public string EventType { get; set; } = string.Empty;
        public int Count { get; set; }
        public DateTime Date { get; set; }
        public decimal Percentage { get; set; }
    }

    public class RetentionRateDto
    {
        public string Period { get; set; } = string.Empty;
        public decimal RetentionRate { get; set; }
        public int RetainedUsers { get; set; }
        public int TotalUsers { get; set; }
    }

    public class UpgradeDowngradeRatesDto
    {
        public decimal UpgradeRate { get; set; }
        public decimal DowngradeRate { get; set; }
        public int TotalUpgrades { get; set; }
        public int TotalDowngrades { get; set; }
        public decimal AverageUpgradeValue { get; set; }
        public decimal AverageDowngradeValue { get; set; }
    }

    /// <summary>
    /// Enhanced billing analytics with comprehensive metrics
    /// </summary>
    public class EnhancedBillingAnalyticsDto : BillingAnalyticsDto
    {
        public List<MonthlyBillingTrendDto> MonthlyBillingTrend { get; set; } = new();
        public List<BillingMethodAnalyticsDto> BillingMethodAnalytics { get; set; } = new();
        public List<BillingFailureReasonDto> BillingFailureReasons { get; set; } = new();
        public decimal AverageBillingCycleTime { get; set; }
        public decimal BillingEfficiency { get; set; }
        public List<RevenueForecastDto> RevenueForecast { get; set; } = new();
    }

    public class MonthlyBillingTrendDto
    {
        public string Month { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public int SuccessfulBills { get; set; }
        public int FailedBills { get; set; }
        public decimal SuccessRate { get; set; }
    }

    public class BillingMethodAnalyticsDto
    {
        public string PaymentMethod { get; set; } = string.Empty;
        public int UsageCount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal SuccessRate { get; set; }
        public decimal AverageAmount { get; set; }
    }

    public class BillingFailureReasonDto
    {
        public string Reason { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Percentage { get; set; }
        public decimal LostRevenue { get; set; }
    }

    public class RevenueForecastDto
    {
        public string Period { get; set; } = string.Empty;
        public decimal ForecastedRevenue { get; set; }
        public decimal ConfidenceLevel { get; set; }
        public decimal GrowthRate { get; set; }
    }

    // === DATABASE-LEVEL ANALYTICS DTOs ===

    public class PaymentMethodAnalytics
    {
        public string PaymentMethod { get; set; } = string.Empty;
        public int UsageCount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal SuccessRate { get; set; }
        public decimal AverageAmount { get; set; }
    }

    public class BillingStatusAnalytics
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Percentage { get; set; }
    }

    public class RevenueTrendData
    {
        public string Period { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int BillingCount { get; set; }
        public decimal GrowthRate { get; set; }
    }

    public class OverageChargesAnalytics
    {
        public decimal TotalOverageCharges { get; set; }
        public int OverageCount { get; set; }
        public decimal AverageOverageAmount { get; set; }
        public List<SmartTelehealth.Core.DTOs.OverageByPlanDto> OverageByPlan { get; set; } = new();
        public List<SmartTelehealth.Core.DTOs.OverageTrendDto> OverageTrend { get; set; } = new();
    }

    public class OverageTrendDto
    {
        public string Period { get; set; } = string.Empty;
        public decimal OverageAmount { get; set; }
        public int OverageCount { get; set; }
    }

    public class BillingEfficiencyMetrics
    {
        public decimal OverallEfficiency { get; set; }
        public decimal PaymentSuccessRate { get; set; }
        public decimal AverageBillingCycleTime { get; set; }
        public decimal RevenueRecoveryRate { get; set; }
        public List<BillingEfficiencyByMethodDto> EfficiencyByMethod { get; set; } = new();
    }

    public class BillingEfficiencyByMethodDto
    {
        public string PaymentMethod { get; set; } = string.Empty;
        public decimal Efficiency { get; set; }
        public decimal SuccessRate { get; set; }
        public decimal AverageProcessingTime { get; set; }
    }


    // === END DATABASE-LEVEL ANALYTICS DTOs ===

    // === END ADVANCED ANALYTICS DTOs ===

    // === MISSING DTOs FOR ANALYTICS SERVICE ===

    /// <summary>
    /// System health DTO with subscription metrics
    /// </summary>
    public class SystemHealthAnalyticsDto
    {
        public int ActiveSubscriptions { get; set; }
        public decimal RevenueToday { get; set; }
        public int NewSubscriptionsToday { get; set; }
        public int TrialsEndingThisWeek { get; set; }
        public int PendingPayments { get; set; }
        public string SystemStatus { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// Subscription dashboard DTO
    /// </summary>
    public class SubscriptionDashboardAnalyticsDto
    {
        public int TotalSubscriptions { get; set; }
        public int ActiveSubscriptions { get; set; }
        public int CancelledSubscriptions { get; set; }
        public int PausedSubscriptions { get; set; }
        public int TrialSubscriptions { get; set; }
        public int NewSubscriptionsThisPeriod { get; set; }
        public int CancelledSubscriptionsThisPeriod { get; set; }
        public decimal AverageSubscriptionValue { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal MonthlyRecurringRevenue { get; set; }
        public DateRangeDto Period { get; set; } = new();
        public DateTime GeneratedAt { get; set; }
    }

    /// <summary>
    /// Enhanced privilege usage analytics DTO
    /// </summary>
    public class EnhancedPrivilegeUsageAnalyticsDto
    {
        public decimal AveragePrivilegeUsage { get; set; }
        public List<PrivilegeUsageDto> PrivilegeUsage { get; set; } = new();
        public DateRangeDto Period { get; set; } = new();
        public DateTime GeneratedAt { get; set; }
    }

    /// <summary>
    /// Enhanced privilege usage DTO
    /// </summary>
    public class EnhancedPrivilegeUsageDto
    {
        public Guid PrivilegeId { get; set; }
        public string PrivilegeName { get; set; } = string.Empty;
        public decimal TotalUsage { get; set; }
        public decimal AverageUsage { get; set; }
    }

    /// <summary>
    /// Enhanced subscription lifecycle analytics DTO
    /// </summary>
    public class EnhancedSubscriptionLifecycleAnalyticsDto
    {
        public int TotalLifecycleEvents { get; set; }
        public List<LifecycleEventDto> LifecycleEvents { get; set; } = new();
        public decimal RetentionRate { get; set; }
        public DateRangeDto Period { get; set; } = new();
        public DateTime GeneratedAt { get; set; }
    }

    public class BillingTrendDto
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int PaymentCount { get; set; }
    }

    // === MISSING DTOs FOR ANALYTICS SERVICE ===

    /// <summary>
    /// Plan analytics DTO
    /// </summary>
    public class PlanAnalyticsDto
    {
        public List<PlanPerformanceDto> PlanPerformance { get; set; } = new();
        public DateRangeDto Period { get; set; } = new DateRangeDto();
        public DateTime GeneratedAt { get; set; }
    }

    /// <summary>
    /// Plan performance DTO
    /// </summary>
    public class PlanPerformanceDto
    {
        public string PlanName { get; set; } = string.Empty;
        public int SubscriptionCount { get; set; }
        public decimal Revenue { get; set; }
        public decimal AverageValue { get; set; }
    }

    /// <summary>
    /// Usage analytics DTO
    /// </summary>
    public class UsageAnalyticsDto
    {
        public decimal TotalUsage { get; set; }
        public decimal AverageUsage { get; set; }
        public List<UsageByPrivilegeDto> UsageByPrivilege { get; set; } = new();
        public DateRangeDto Period { get; set; } = new DateRangeDto();
        public DateTime GeneratedAt { get; set; }
    }

    /// <summary>
    /// Usage by privilege DTO
    /// </summary>
    public class UsageByPrivilegeDto
    {
        public string PrivilegeName { get; set; } = string.Empty;
        public decimal Usage { get; set; }
        public decimal Percentage { get; set; }
    }


    /// <summary>
    /// Date range DTO
    /// </summary>
    public class DateRangeDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

} 