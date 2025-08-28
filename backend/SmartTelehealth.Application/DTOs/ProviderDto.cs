using System;

namespace SmartTelehealth.Application.DTOs
{
    public class ProviderDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string CategoryId { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public string? Education { get; set; }
        public string? Experience { get; set; }
        public string? Certifications { get; set; }
        public int ConsultationDurationMinutes { get; set; }
        public bool IsAvailable { get; set; }
        public string? ProfilePicture { get; set; }
        public string? Languages { get; set; }
        public string? TimeZone { get; set; }
        public decimal Rating { get; set; }
        public int ReviewCount { get; set; }
        public bool IsVerified { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? StripeAccountId { get; set; }
        public string? StripeCustomerId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string UserType { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsEmailVerified { get; set; }
        public bool IsPhoneVerified { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }

    public class UpdateProviderDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? LicenseNumber { get; set; }
        public string? Specialization { get; set; }
        public string? Bio { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsVerified { get; set; } = false;
    }
} 