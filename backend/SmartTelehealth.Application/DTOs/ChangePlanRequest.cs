using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Application.DTOs;

public class ChangePlanRequest
{
    [Required]
    public string NewPlanId { get; set; } = string.Empty;
    
    public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;
    
    public string? Reason { get; set; }
    
    public bool Prorate { get; set; } = true;
}
