using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IAuthService
{
    Task<JsonModel> LoginAsync(LoginDto loginDto);
    Task<JsonModel> RegisterAsync(RegisterDto registerDto);
    Task<JsonModel> RefreshTokenAsync(RefreshTokenDto refreshTokenDto);
    Task<JsonModel> ChangePasswordAsync(ChangePasswordDto changePasswordDto);
    Task<JsonModel> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);
    Task<JsonModel> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
    Task<JsonModel> LogoutAsync();
    Task<JsonModel> ValidateTokenAsync(string token);
} 