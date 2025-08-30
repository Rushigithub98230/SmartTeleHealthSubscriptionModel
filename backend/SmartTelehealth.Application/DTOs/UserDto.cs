using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Application.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string UserType { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
        public int UserRoleId { get; set; }
        public bool IsActive { get; set; }
        public bool IsVerified { get; set; }
        public bool IsEmailVerified { get; set; }
        public bool IsPhoneVerified { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public string? ProfilePicture { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public string? Country { get; set; }
        public string? EmergencyContact { get; set; }
        public string? EmergencyPhone { get; set; }
        public string? StripeCustomerId { get; set; }
    }

    public class PatientDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string UserType { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsVerified { get; set; }
        public bool IsEmailVerified { get; set; }
        public bool IsPhoneVerified { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public string? ProfilePicture { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public string? Country { get; set; }
        public string? EmergencyContact { get; set; }
        public string? EmergencyPhone { get; set; }
        public string? StripeCustomerId { get; set; }
        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public decimal TotalSpent { get; set; }
    }

    public class CreateUserDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public string? Country { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string UserType { get; set; } = "Patient";
        public string Role { get; set; } = "User";
        public int UserRoleId { get; set; }
    }

    public class UpdateUserDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public string? Country { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public bool IsActive { get; set; } = true;
        
        // Stripe Integration
        public string? StripeCustomerId { get; set; }
    }

    public class UpdateUserProfileDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Phone { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public string? Country { get; set; }
        public string? EmergencyContact { get; set; }
        public string? EmergencyPhone { get; set; }
    }

    public class UpdateUserPreferencesDto
    {
        public bool EmailNotifications { get; set; } = true;
        public bool SmsNotifications { get; set; } = true;
        public bool PushNotifications { get; set; } = true;
        public string? Language { get; set; } = "en";
        public string? TimeZone { get; set; } = "UTC";
        public bool MarketingEmails { get; set; } = false;
        public Dictionary<string, object> Preferences { get; set; } = new();
        public string? NotificationPreferences { get; set; }
        public string? LanguagePreference { get; set; }
        public string? TimeZonePreference { get; set; }
    }

    public class UpdateProviderScheduleDto
    {
        public List<SmartTelehealth.Application.DTOs.ProviderAvailabilityDto> Availability { get; set; } = new();
        public bool IsAvailable { get; set; } = true;
        public string? Notes { get; set; }
        public WeeklyScheduleDto WeeklySchedule { get; set; } = new();
        public List<DateTime> AvailableDates { get; set; } = new();
        public List<DateTime> UnavailableDates { get; set; } = new();
        public int DefaultDurationMinutes { get; set; } = 30;
        public bool IsActive { get; set; } = true;
    }

    public class ReviewDto
    {
        public string Id { get; set; } = string.Empty;
        public string ProviderId { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public decimal Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsVerified { get; set; }
    }

    public class AddReviewDto
    {
        [Required]
        [Range(1, 5)]
        public decimal Rating { get; set; }
        
        [Required]
        [MaxLength(1000)]
        public string Comment { get; set; } = string.Empty;
    }

    public class MedicalHistoryDto
    {
        public int UserId { get; set; }
        public List<string> Allergies { get; set; } = new();
        public List<string> Medications { get; set; } = new();
        public List<string> Conditions { get; set; } = new();
        public List<string> Surgeries { get; set; } = new();
        public string? FamilyHistory { get; set; }
        public string? Lifestyle { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class UpdateMedicalHistoryDto
    {
        public int UserId { get; set; }
        public List<string>? Allergies { get; set; }
        public List<string>? Medications { get; set; }
        public List<string>? CurrentMedications { get; set; }
        public List<string>? Conditions { get; set; }
        public List<string>? Surgeries { get; set; }
        public string? FamilyHistory { get; set; }
        public string? Lifestyle { get; set; }
        public string? MedicalHistory { get; set; }
    }



    public class CreatePaymentMethodDto
    {
        public int UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
    }

    public class PagedResult<T>
    {
        public List<T> Data { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }
} 