using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Application.DTOs;

public class CreateSubscriptionPlanDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public decimal Price { get; set; }
    public Guid BillingCycleId { get; set; }
    public Guid CurrencyId { get; set; }
    public int MessagingCount { get; set; } = 10;
    public bool IncludesMedicationDelivery { get; set; } = true;
    public bool IncludesFollowUpCare { get; set; } = true;
    public int DeliveryFrequencyDays { get; set; } = 30;
    public int MaxPauseDurationDays { get; set; } = 90;
    public bool IsActive { get; set; } = true;
    
    // Marketing and display properties
    public bool IsMostPopular { get; set; } = false;
    public bool IsTrending { get; set; } = false;
    
    public int DisplayOrder { get; set; }
    [MaxLength(1000)]
    public string? Features { get; set; }
} 