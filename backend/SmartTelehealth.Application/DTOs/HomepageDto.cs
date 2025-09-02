using System.Collections.Generic;

namespace SmartTelehealth.Application.DTOs;

public class HomepageDto
{
    public List<CategoryWithSubscriptionsDto> Categories { get; set; } = new();
    public List<FeaturedProviderDto> FeaturedProviders { get; set; } = new();
    public int TotalAppointments { get; set; }
    public int TotalPatients { get; set; }
    public int TotalProviders { get; set; }
    public List<TestimonialDto> Testimonials { get; set; } = new();
    public List<FeatureDto> Features { get; set; } = new();
}

public class TestimonialDto
{
    public Guid Id { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string? PatientPicture { get; set; }
    public string Content { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}

public class FeatureDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public bool IsActive { get; set; }
} 