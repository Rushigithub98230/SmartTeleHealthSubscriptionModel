namespace SmartTelehealth.Application.DTOs;

public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    public string? Features { get; set; }
    public string? ConsultationDescription { get; set; }
    public decimal BasePrice { get; set; }
    public decimal ConsultationFee { get; set; }
    public int ConsultationDurationMinutes { get; set; }
    public bool RequiresHealthAssessment { get; set; }
    public bool AllowsMedicationDelivery { get; set; }
    public bool AllowsFollowUpMessaging { get; set; }
    public bool AllowsOneTimeConsultation { get; set; }
    public decimal OneTimeConsultationFee { get; set; }
    public int OneTimeConsultationDurationMinutes { get; set; }
    
    // Marketing and display properties
    public bool IsMostPopular { get; set; }
    public bool IsTrending { get; set; }
    
    public List<SubscriptionPlanDto> SubscriptionPlans { get; set; } = new();
}

public class CreateCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; } = 0;
    
    // Marketing and display properties
    public bool IsMostPopular { get; set; } = false;
    public bool IsTrending { get; set; } = false;
}

public class UpdateCategoryDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Marketing and display properties
    public bool IsMostPopular { get; set; } = false;
    public bool IsTrending { get; set; } = false;
}

 