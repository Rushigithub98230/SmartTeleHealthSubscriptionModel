using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Application.DTOs;

public class LoginDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty;
}

public class RegisterDto
{
    [Required]
    public string FirstName { get; set; } = string.Empty;
    [Required]
    public string LastName { get; set; } = string.Empty;
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
    [Required]
    [Compare("Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
    [Required]
    public string PhoneNumber { get; set; } = string.Empty;
    [Required]
    public string DateOfBirth { get; set; } = string.Empty;
    [Required]
    public string Gender { get; set; } = string.Empty;
    [Required]
    public string Address { get; set; } = string.Empty;
    [Required]
    public string City { get; set; } = string.Empty;
    [Required]
    public string State { get; set; } = string.Empty;
    [Required]
    public string ZipCode { get; set; } = string.Empty;
            public string? Role { get; set; } = "Client"; // Default to Client, can be Admin, Provider, or Client
}

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserDto User { get; set; } = new();
    public string Message { get; set; } = string.Empty;
}

public class RefreshTokenDto
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}

public class ForgotPasswordDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordDto
{
    [Required]
    public string Token { get; set; } = string.Empty;
    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; } = string.Empty;
    [Required]
    [Compare("NewPassword")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}

public class ChangePasswordDto
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;
    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; } = string.Empty;
    [Required]
    [Compare("NewPassword")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
} 

public class LoginRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

public class RefreshTokenRequestDto
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class ChangePasswordRequestDto
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class ForgotPasswordRequestDto
{
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class ConfirmPasswordResetDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    [Required]
    public string ResetToken { get; set; } = string.Empty;
    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; } = string.Empty;
    [Required]
    [Compare("NewPassword")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
} 