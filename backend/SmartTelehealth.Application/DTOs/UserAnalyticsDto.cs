using System;
using System.Collections.Generic;

namespace SmartTelehealth.Application.DTOs
{
    /// <summary>
    /// User analytics DTO for admin portal comprehensive user monitoring
    /// </summary>
    public class UserAnalyticsDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        
        // Subscription Analytics
        public int TotalSubscriptions { get; set; }
        public int ActiveSubscriptions { get; set; }
        public int PastSubscriptions { get; set; }
        public int CancelledSubscriptions { get; set; }
        public decimal AverageSubscriptionDurationDays { get; set; }
        public string? CurrentPlan { get; set; }
        public DateTime? CurrentSubscriptionStartDate { get; set; }
        public DateTime? NextBillingDate { get; set; }
        
        // Financial Analytics
        public decimal TotalRevenue { get; set; }
        public decimal AverageMonthlySpend { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalRefunded { get; set; }
        
        // Payment Analytics
        public int TotalPayments { get; set; }
        public int SuccessfulPayments { get; set; }
        public int FailedPayments { get; set; }
        public decimal PaymentSuccessRate { get; set; }
        
        // Privilege Analytics
        public int ActivePrivileges { get; set; }
        public decimal PrivilegeUsageRate { get; set; }
        public bool HasOverageCharges { get; set; }
        
        // Account Analytics
        public DateTime AccountCreatedDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public DateTime? LastActivityDate { get; set; }
        public int AccountAgeDays { get; set; }
        public bool IsActiveAccount { get; set; }
        
        // Enhanced Analytics - Detailed Timeline and History
        public List<SubscriptionHistoryDto> SubscriptionHistory { get; set; } = new();
        public List<PrivilegeUsageHistoryDto> PrivilegeUsageHistory { get; set; } = new();
        public List<UpcomingRenewalDto> UpcomingRenewals { get; set; } = new();
        public List<InvoiceSummaryDto> Invoices { get; set; } = new();
    }
    
    /// <summary>
    /// Detailed subscription analytics for charts
    /// </summary>
    public class SubscriptionAnalyticsDetailDto
    {
        public List<SubscriptionTimelineDto> SubscriptionTimeline { get; set; } = new();
        public List<MonthlyRevenueDto> MonthlyRevenue { get; set; } = new();
        public List<PlanDistributionDto> PlanDistribution { get; set; } = new();
    }
    
    public class SubscriptionTimelineDto
    {
        public DateTime Date { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
    
    public class MonthlyRevenueDto
    {
        public string Month { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int PaymentCount { get; set; }
    }
    
    public class PlanDistributionDto
    {
        public string PlanName { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal TotalRevenue { get; set; }
    }
    
    /// <summary>
    /// Detailed subscription history for user analytics
    /// </summary>
    public class SubscriptionHistoryDto
    {
        public Guid Id { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string BillingCycle { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
    }
    
    /// <summary>
    /// Privilege usage history for user analytics
    /// </summary>
    public class PrivilegeUsageHistoryDto
    {
        public Guid PrivilegeId { get; set; }
        public string PrivilegeName { get; set; } = string.Empty;
        public int UsedValue { get; set; }
        public int LimitValue { get; set; }
        public decimal UsagePercentage { get; set; }
        public DateTime UsedAt { get; set; }
        public bool IsOverage { get; set; }
        public decimal? OverageAmount { get; set; }
    }
    
    /// <summary>
    /// Upcoming renewal information for user analytics
    /// </summary>
    public class UpcomingRenewalDto
    {
        public Guid SubscriptionId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public DateTime RenewalDate { get; set; }
        public decimal Amount { get; set; }
        public bool AutoRenew { get; set; }
        public int DaysUntilRenewal { get; set; }
    }
    
    /// <summary>
    /// Invoice summary for user analytics
    /// </summary>
    public class InvoiceSummaryDto
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public DateTime? PaidAt { get; set; }
    }
}

