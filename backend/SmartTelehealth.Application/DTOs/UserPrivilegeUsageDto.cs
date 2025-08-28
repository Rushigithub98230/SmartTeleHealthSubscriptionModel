namespace SmartTelehealth.Application.DTOs;

public class UserPrivilegeUsageDto
{
    public Guid SubscriptionId { get; set; }
    public string PrivilegeName { get; set; } = string.Empty;
    public int Remaining { get; set; }
} 