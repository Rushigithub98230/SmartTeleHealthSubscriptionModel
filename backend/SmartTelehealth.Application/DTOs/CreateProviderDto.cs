using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Application.DTOs;

public class CreateProviderDto
{
    [Required]
    public string FirstName { get; set; } = string.Empty;
    [Required]
    public string LastName { get; set; } = string.Empty;
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    [Required]
    public string PhoneNumber { get; set; } = string.Empty;
    [Required]
    public string LicenseNumber { get; set; } = string.Empty;
    [Required]
    public string State { get; set; } = string.Empty;
    [Required]
    public string Specialty { get; set; } = string.Empty;
    [Required]
    public string Bio { get; set; } = string.Empty;
    public string? ProfilePicture { get; set; }
    // public decimal ConsultationFee { get; set; }
    public bool IsAvailable { get; set; } = true;
    public string? StripeAccountId { get; set; }
} 