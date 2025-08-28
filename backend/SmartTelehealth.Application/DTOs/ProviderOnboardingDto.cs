namespace SmartTelehealth.Application.DTOs;

public class CreateProviderOnboardingDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public string? SubSpecialty { get; set; }
    public string LicenseNumber { get; set; } = string.Empty;
    public string LicenseState { get; set; } = string.Empty;
    public string? NPINumber { get; set; }
    public string? DEANumber { get; set; }
    public string? Education { get; set; }
    public string? WorkHistory { get; set; }
    public string? MalpracticeInsurance { get; set; }
    public string? Bio { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public string? GovernmentIdUrl { get; set; }
    public string? LicenseDocumentUrl { get; set; }
    public string? CertificationDocumentUrl { get; set; }
    public string? MalpracticeInsuranceUrl { get; set; }
}

public class UpdateProviderOnboardingDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Specialty { get; set; }
    public string? SubSpecialty { get; set; }
    public string? LicenseNumber { get; set; }
    public string? LicenseState { get; set; }
    public string? NPINumber { get; set; }
    public string? DEANumber { get; set; }
    public string? Education { get; set; }
    public string? WorkHistory { get; set; }
    public string? MalpracticeInsurance { get; set; }
    public string? Bio { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public string? GovernmentIdUrl { get; set; }
    public string? LicenseDocumentUrl { get; set; }
    public string? CertificationDocumentUrl { get; set; }
    public string? MalpracticeInsuranceUrl { get; set; }
}

public class ProviderOnboardingDto
{
    public Guid Id { get; set; }
    public int UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public string? SubSpecialty { get; set; }
    public string LicenseNumber { get; set; } = string.Empty;
    public string LicenseState { get; set; } = string.Empty;
    public string? NPINumber { get; set; }
    public string? DEANumber { get; set; }
    public string? Education { get; set; }
    public string? WorkHistory { get; set; }
    public string? MalpracticeInsurance { get; set; }
    public string? Bio { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public string? GovernmentIdUrl { get; set; }
    public string? LicenseDocumentUrl { get; set; }
    public string? CertificationDocumentUrl { get; set; }
    public string? MalpracticeInsuranceUrl { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? AdminRemarks { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public int? ReviewedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ReviewProviderOnboardingDto
{
    public string Status { get; set; } = string.Empty;
    public string? AdminRemarks { get; set; }
} 