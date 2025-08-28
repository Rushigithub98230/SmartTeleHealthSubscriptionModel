namespace SmartTelehealth.Application.DTOs
{
    public class PrivilegeUsageDto
    {
        public string PrivilegeName { get; set; }
        public int UsedValue { get; set; }
        public int AllowedValue { get; set; }
        public int RemainingValue { get; set; }
        public decimal UsagePercentage { get; set; }
    }
} 