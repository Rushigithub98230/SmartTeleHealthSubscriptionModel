namespace SmartTelehealth.Application.DTOs
{
    public class UsageStatisticsDto
    {
        public string SubscriptionId { get; set; }
        public string PlanName { get; set; }
        public DateTime CurrentPeriodStart { get; set; }
        public DateTime CurrentPeriodEnd { get; set; }
        public int TotalPrivileges { get; set; }
        public int UsedPrivileges { get; set; }
        public List<PrivilegeUsageDto> PrivilegeUsage { get; set; } = new();
    }
} 