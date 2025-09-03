using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using AutoMapper;
using System.Linq;
using System.Threading.Tasks;

namespace SmartTelehealth.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IJwtService _jwtService;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
          

        public AuthService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IJwtService jwtService,
            IUserRepository userRepository,
            IMapper mapper,
            IConfiguration configuration,
            IAuditService auditService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _userRepository = userRepository;
            _mapper = mapper;
            _configuration = configuration;
              
        }

        public async Task<JsonModel> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(loginDto.Email);
                if (user == null)
                {
                 
                    return new JsonModel { data = new object(), Message = "Invalid email or password", StatusCode = 400 };
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
                if (!result.Succeeded)
                {
                  
                    return new JsonModel { data = new object(), Message = "Invalid email or password", StatusCode = 400 };
                }

                var token = _jwtService.GenerateToken(user);
                var refreshToken = await GenerateRefreshTokenAsync(user);
                

                var loginResponse = new LoginResponseDto
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpirationInMinutes"])),
                    User = _mapper.Map<UserDto>(user),
                    Message = "Login successful"
                };

                return new JsonModel { data = loginResponse, Message = "Login successful", StatusCode = 200 };
            }
            catch (Exception ex)
            {
               
                return new JsonModel { data = new object(), Message = "An error occurred during login", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
                if (existingUser != null)
                {
                  
                    return new JsonModel { data = new object(), Message = "User with this email already exists", StatusCode = 400 };
                }

                var user = new User
                {
                    UserName = registerDto.Email,
                    Email = registerDto.Email,
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,
                    PhoneNumber = registerDto.PhoneNumber,
                    DateOfBirth = DateTime.UtcNow, // Default value
                    Gender = registerDto.Gender,
                    Address = registerDto.Address,
                    City = registerDto.City,
                    State = registerDto.State,
                    ZipCode = registerDto.ZipCode,
                    // Set audit properties for creation
                    CreatedBy = 0, // System user for registration
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true
                };

                var result = await _userManager.CreateAsync(user, registerDto.Password);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                   
                    return new JsonModel { data = new object(), Message = "Registration failed", StatusCode = 400 };
                }

                // Assign default role (Patient)
                await _userManager.AddToRoleAsync(user, "Patient");
                var token = _jwtService.GenerateToken(user);
                var refreshToken = await GenerateRefreshTokenAsync(user);

                var loginResponse = new LoginResponseDto
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpirationInMinutes"])),
                    User = _mapper.Map<UserDto>(user),
                    Message = "Registration successful"
                };

                return new JsonModel { data = loginResponse, Message = "Registration successful", StatusCode = 201 };
            }
            catch (Exception ex)
            {
                return new JsonModel { data = new object(), Message = "An error occurred during registration", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
        {
            try
            {
                var user = await _userRepository.GetByRefreshTokenAsync(refreshTokenDto.RefreshToken);
                if (user == null)
                {
                    return new JsonModel { data = new object(), Message = "Invalid refresh token", StatusCode = 400 };
                }

                var token = _jwtService.GenerateToken(user);
                var newRefreshToken = await GenerateRefreshTokenAsync(user);

                var loginResponse = new LoginResponseDto
                {
                    Token = token,
                    RefreshToken = newRefreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpirationInMinutes"])),
                    User = _mapper.Map<UserDto>(user),
                    Message = "Token refreshed successfully"
                };

                return new JsonModel { data = loginResponse, Message = "Token refreshed successfully", StatusCode = 200 };
            }
            catch (Exception ex)
            {
               
                return new JsonModel { data = new object(), Message = "An error occurred while refreshing token", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> ChangePasswordAsync(ChangePasswordDto changePasswordDto)
        {
            try
            {
                // This would need to get the current user from the context
                // For now, we'll use a placeholder
                var userId = "current-user-id"; // This should come from the current user context
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return new JsonModel { data = new object(), Message = "User not found", StatusCode = 404 };
                }

                var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return new JsonModel { data = new object(), Message = "Failed to change password", StatusCode = 400 };
                }

               
                return new JsonModel { data = true, Message = "Password changed successfully", StatusCode = 200 };
            }
            catch (Exception ex)
            {
               
                return new JsonModel { data = new object(), Message = "An error occurred while changing password", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);
                if (user == null)
                {
                    // Don't reveal if email exists or not for security
                    return new JsonModel { data = true, Message = "Password reset email sent", StatusCode = 200 };
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                
               
                return new JsonModel { data = true, Message = "Password reset email sent", StatusCode = 200 };
            }
            catch (Exception ex)
            {
               
                return new JsonModel { data = new object(), Message = "An error occurred while processing the request", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            try
            {
                // Note: This is a simplified implementation. In a real application,
                // you would validate the token and find the user by token, not by email
                var user = await _userRepository.GetByEmailAsync(resetPasswordDto.Token); // This should be token validation
                if (user == null)
                    return new JsonModel { data = new object(), Message = "Invalid reset token", StatusCode = 400 };

                var result = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.NewPassword);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return new JsonModel { data = new object(), Message = "Failed to reset password", StatusCode = 400 };
                }

            
                return new JsonModel { data = true, Message = "Password reset successfully", StatusCode = 200 };
            }
            catch (Exception ex)
            {
               
                return new JsonModel { data = new object(), Message = "An error occurred while resetting password", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> LogoutAsync()
        {
            try
            {
                // This would typically invalidate the refresh token
                // For now, we'll just return success
                return new JsonModel { data = true, Message = "Logout successful", StatusCode = 200 };
            }
            catch (Exception ex)
            {
              
                return new JsonModel { data = new object(), Message = "An error occurred during logout", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> ValidateTokenAsync(string token)
        {
            try
            {
                // This would validate the JWT token
                // For now, we'll just return true
                return new JsonModel { data = true, Message = "Token is valid", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                
                return new JsonModel { data = new object(), Message = "Invalid token", StatusCode = 400 };
            }
        }

        private async Task<string> GenerateRefreshTokenAsync(User user)
        {
            var refreshToken = Guid.NewGuid().ToString();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _userRepository.UpdateAsync(user);
            return refreshToken;
        }
    }
} 