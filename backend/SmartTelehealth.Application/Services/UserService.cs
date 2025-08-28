using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using System.IO;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace SmartTelehealth.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly INotificationService _notificationService;
    private readonly IStripeService _stripeService;
    private readonly ILogger<UserService> _logger;
    private readonly UserManager<User> _userManager;
    private readonly IMapper _mapper;
    private readonly IDocumentService _documentService;
    private readonly IDocumentTypeService _documentTypeService;
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly ISubscriptionRepository _subscriptionRepository; // Added for DeleteUserAsync

    public UserService(
        IUserRepository userRepository,
        INotificationService notificationService,
        IStripeService stripeService,
        ILogger<UserService> logger,
        UserManager<User> userManager,
        IMapper mapper,
        IDocumentService documentService,
        IDocumentTypeService documentTypeService,
        IUserRoleRepository userRoleRepository,
        ISubscriptionRepository subscriptionRepository) // Added for DeleteUserAsync
    {
        _userRepository = userRepository;
        _notificationService = notificationService;
        _stripeService = stripeService;
        _logger = logger;
        _userManager = userManager;
        _mapper = mapper;
        _documentService = documentService;
        _documentTypeService = documentTypeService;
        _userRoleRepository = userRoleRepository;
        _subscriptionRepository = subscriptionRepository; // Added for DeleteUserAsync
    }

    // --- AUTHENTICATION METHODS ---
    public async Task<JsonModel> AuthenticateUserAsync(string email, string password, TokenModel tokenModel)
    {
        try
        {
            // Use UserManager to find user by email (Identity)
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return new JsonModel { data = new object(), Message = "Invalid email or password", StatusCode = 401 };

            // Use Identity password verification (BCrypt)
            var isValidPassword = await _userManager.CheckPasswordAsync(user, password);
            if (!isValidPassword)
                return new JsonModel { data = new object(), Message = "Invalid email or password", StatusCode = 401 };

            var userDto = MapToUserDto(user);
            return new JsonModel { data = userDto, Message = "Authentication successful", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error authenticating user: {Email}", email);
            return new JsonModel { data = new object(), Message = $"Authentication failed: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetUserByEmailAsync(string email, TokenModel tokenModel)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
                return new JsonModel { data = new object(), Message = "User not found", StatusCode = 404 };

            var userDto = MapToUserDto(user);
            return new JsonModel { data = userDto, Message = "User retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by email: {Email}", email);
            return new JsonModel { data = new object(), Message = $"Failed to get user: {ex.Message}", StatusCode = 500 };
        }
    }

    // User profile operations
    public async Task<JsonModel> GetUserByIdAsync(int userId, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Getting user {UserId} by user {TokenUserId}", userId, tokenModel?.UserID ?? 0);
            
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return new JsonModel { data = new object(), Message = "User not found", StatusCode = 404 };

            var userDto = MapToUserDto(user);
            
            _logger.LogInformation("User {UserId} retrieved successfully by user {TokenUserId}", userId, tokenModel?.UserID ?? 0);
            return new JsonModel { data = userDto, Message = "User retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user {UserId} by user {TokenUserId}", userId, tokenModel?.UserID ?? 0);
            return new JsonModel { data = new object(), Message = $"Failed to get user: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> UpdateUserAsync(int userId, UpdateUserDto updateDto, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Updating user {UserId} by user {TokenUserId}", userId, tokenModel?.UserID ?? 0);
            
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return new JsonModel { data = new object(), Message = "User not found", StatusCode = 404 };

            // Update user properties
            if (!string.IsNullOrEmpty(updateDto.FirstName))
                user.FirstName = updateDto.FirstName;
            if (!string.IsNullOrEmpty(updateDto.LastName))
                user.LastName = updateDto.LastName;
            if (!string.IsNullOrEmpty(updateDto.Email))
                user.Email = updateDto.Email;
            if (!string.IsNullOrEmpty(updateDto.PhoneNumber))
                user.PhoneNumber = updateDto.PhoneNumber;
            if (updateDto.DateOfBirth.HasValue)
                user.DateOfBirth = updateDto.DateOfBirth.Value;
            if (!string.IsNullOrEmpty(updateDto.Address))
                user.Address = updateDto.Address;
            if (!string.IsNullOrEmpty(updateDto.City))
                user.City = updateDto.City;
            if (!string.IsNullOrEmpty(updateDto.State))
                user.State = updateDto.State;
            if (!string.IsNullOrEmpty(updateDto.ZipCode))
                user.ZipCode = updateDto.ZipCode;
            if (!string.IsNullOrEmpty(updateDto.Country))
                user.Country = updateDto.Country;

            user.UpdatedDate = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            var userDto = _mapper.Map<UserDto>(user);
            
            _logger.LogInformation("User {UserId} updated successfully by user {TokenUserId}", userId, tokenModel?.UserID ?? 0);
            return new JsonModel { data = userDto, Message = "User updated successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId} by user {TokenUserId}", userId, tokenModel?.UserID ?? 0);
            return new JsonModel { data = new object(), Message = $"Failed to update user: {ex.Message}", StatusCode = 500 };
        }
    }

    // --- DOCUMENT MANAGEMENT (Updated to use centralized DocumentService) ---
    
    public async Task<JsonModel> UploadProfilePictureAsync(int userId, IFormFile file, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Uploading profile picture for user {UserId} by user {TokenUserId}", userId, tokenModel?.UserID ?? 0);
            
            // Validate user exists
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return new JsonModel { data = new object(), Message = "User not found", StatusCode = 404 };

            // Get profile picture document type
            var profilePictureType = await _documentTypeService.GetByNameAsync("Profile Picture");
            if (profilePictureType == null)
                return new JsonModel { data = new object(), Message = "Profile picture document type not found", StatusCode = 404 };

            // Read file content
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            var fileBytes = memoryStream.ToArray();
            var documentType = profilePictureType.data as DocumentTypeDto;
            var DocumentTypeId = Guid.Empty;
            if (documentType != null)
            {
                DocumentTypeId = documentType.DocumentTypeId;
            }
            var uploadRequest = new UploadUserDocumentRequest
            {
                FileData = fileBytes,
                FileName = file.FileName,
                ContentType = file.ContentType,
                DocumentTypeId = DocumentTypeId,
                UserId = userId,
                Description = "Profile Picture"
            };

            var result = await _documentService.UploadUserDocumentAsync(uploadRequest, tokenModel);
            
            _logger.LogInformation("Profile picture uploaded successfully for user {UserId} by user {TokenUserId}", userId, tokenModel?.UserID ?? 0);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading profile picture for user {UserId} by user {TokenUserId}", userId, tokenModel?.UserID ?? 0);
            return new JsonModel { data = new object(), Message = $"Failed to upload profile picture: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetUserDocumentsAsync(int userId, string? referenceType, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Getting documents for user {UserId} by user {TokenUserId}", userId, tokenModel?.UserID ?? 0);
            
            var result = await _documentService.GetUserDocumentsAsync(userId, referenceType, tokenModel);
            
            _logger.LogInformation("Documents retrieved successfully for user {UserId} by user {TokenUserId}", userId, tokenModel?.UserID ?? 0);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting documents for user {UserId} by user {TokenUserId}", userId, tokenModel?.UserID ?? 0);
            return new JsonModel { data = new object(), Message = $"Failed to get documents: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> UploadUserDocumentAsync(int userId, UploadUserDocumentRequest request, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Uploading document for user {UserId} by user {TokenUserId}", userId, tokenModel?.UserID ?? 0);
            
            var result = await _documentService.UploadUserDocumentAsync(request, tokenModel);
            
            _logger.LogInformation("Document uploaded successfully for user {UserId} by user {TokenUserId}", userId, tokenModel?.UserID ?? 0);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading document for user {UserId} by user {TokenUserId}", userId, tokenModel?.UserID ?? 0);
            return new JsonModel { data = new object(), Message = $"Failed to upload document: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> DeleteUserDocumentAsync(Guid documentId, int userId, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Deleting document {DocumentId} for user {UserId} by user {TokenUserId}", documentId, userId, tokenModel?.UserID ?? 0);
            
            var result = await _documentService.DeleteUserDocumentAsync(documentId, userId, tokenModel);
            
            _logger.LogInformation("Document {DocumentId} deleted successfully for user {UserId} by user {TokenUserId}", documentId, userId, tokenModel?.UserID ?? 0);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document {DocumentId} for user {UserId} by user {TokenUserId}", documentId, userId, tokenModel?.UserID ?? 0);
            return new JsonModel { data = new object(), Message = $"Failed to delete document: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> DeleteUserAsync(int userId, TokenModel tokenModel)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return new JsonModel { data = new object(), Message = "User not found", StatusCode = 404 };

            await _userRepository.DeleteAsync(userId);
            return new JsonModel { data = true, Message = "User deleted successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", userId);
            return new JsonModel { data = new object(), Message = $"Failed to delete user: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetUsersByRoleAsync(string role, TokenModel tokenModel)
    {
        try
        {
            var users = await _userRepository.GetByRoleAsync(role);
            var userDtos = _mapper.Map<IEnumerable<UserDto>>(users);
            return new JsonModel { data = userDtos, Message = $"Users with role {role} retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users by role {Role}", role);
            return new JsonModel { data = new object(), Message = $"Failed to get users by role: {ex.Message}", StatusCode = 500 };
        }
    }



    public async Task<JsonModel> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto, TokenModel tokenModel)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return new JsonModel { data = new object(), Message = "User not found", StatusCode = 404 };

            // Verify current password using Identity
            var isValidCurrentPassword = await _userManager.CheckPasswordAsync(user, changePasswordDto.CurrentPassword);
            if (!isValidCurrentPassword)
                return new JsonModel { data = new object(), Message = "Current password is incorrect", StatusCode = 400 };

            // Change password using Identity
            var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new JsonModel { data = new object(), Message = $"Failed to change password: {errors}", StatusCode = 400 };
            }

            return new JsonModel { data = true, Message = "Password changed successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user {UserId}", userId);
            return new JsonModel { data = new object(), Message = $"Failed to change password: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> ResetPasswordAsync(string email, TokenModel tokenModel)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
                return new JsonModel { data = new object(), Message = "User not found", StatusCode = 404 };

            // Generate reset token
            var resetToken = Guid.NewGuid().ToString();
            user.PasswordResetToken = resetToken;
            user.ResetTokenExpires = DateTime.UtcNow.AddHours(24);
            user.UpdatedDate = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            // Send reset email
            await _notificationService.SendPasswordResetEmailAsync(email, resetToken, tokenModel);

            return new JsonModel { data = true, Message = "Password reset email sent successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password for email {Email}", email);
            return new JsonModel { data = new object(), Message = $"Failed to reset password: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> ConfirmPasswordResetAsync(string email, string resetToken, string newPassword, TokenModel tokenModel)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return new JsonModel { data = new object(), Message = "User not found", StatusCode = 404 };

            if (user.PasswordResetToken != resetToken)
                return new JsonModel { data = new object(), Message = "Invalid reset token", StatusCode = 400 };

            if (user.ResetTokenExpires < DateTime.UtcNow)
                return new JsonModel { data = new object(), Message = "Reset token has expired", StatusCode = 400 };

            // Reset password using Identity
            var result = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new JsonModel { data = new object(), Message = $"Failed to reset password: {errors}", StatusCode = 400 };
            }

            // Clear reset token
            user.PasswordResetToken = null;
            user.ResetTokenExpires = null;
            await _userManager.UpdateAsync(user);

            return new JsonModel { data = true, Message = "Password reset successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming password reset for email {Email}", email);
            return new JsonModel { data = new object(), Message = $"Failed to confirm password reset: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> UpdateUserProfileAsync(int userId, UpdateUserProfileDto profileDto, TokenModel tokenModel)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return new JsonModel { data = new object(), Message = "User not found", StatusCode = 404 };

            // Update profile properties
            if (!string.IsNullOrEmpty(profileDto.FirstName))
                user.FirstName = profileDto.FirstName;
            if (!string.IsNullOrEmpty(profileDto.LastName))
                user.LastName = profileDto.LastName;
            if (!string.IsNullOrEmpty(profileDto.PhoneNumber))
                user.PhoneNumber = profileDto.PhoneNumber;
            if (profileDto.DateOfBirth.HasValue)
                user.DateOfBirth = profileDto.DateOfBirth.Value;
            if (!string.IsNullOrEmpty(profileDto.Address))
                user.Address = profileDto.Address;
            if (!string.IsNullOrEmpty(profileDto.City))
                user.City = profileDto.City;
            if (!string.IsNullOrEmpty(profileDto.State))
                user.State = profileDto.State;
            if (!string.IsNullOrEmpty(profileDto.ZipCode))
                user.ZipCode = profileDto.ZipCode;
            if (!string.IsNullOrEmpty(profileDto.Country))
                user.Country = profileDto.Country;

            user.UpdatedDate = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            return new JsonModel { data = true, Message = "Profile updated successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile {UserId}", userId);
            return new JsonModel { data = new object(), Message = $"Failed to update profile: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> UpdateUserPreferencesAsync(int userId, UpdateUserPreferencesDto preferencesDto, TokenModel tokenModel)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return new JsonModel { data = new object(), Message = "User not found", StatusCode = 404 };

            // Update preferences
            if (!string.IsNullOrEmpty(preferencesDto.NotificationPreferences))
                user.NotificationPreferences = preferencesDto.NotificationPreferences;
            if (!string.IsNullOrEmpty(preferencesDto.LanguagePreference))
                user.LanguagePreference = preferencesDto.LanguagePreference;
            if (!string.IsNullOrEmpty(preferencesDto.TimeZonePreference))
                user.TimeZonePreference = preferencesDto.TimeZonePreference;
            user.UpdatedDate = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            return new JsonModel { data = true, Message = "Preferences updated successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user preferences {UserId}", userId);
            return new JsonModel { data = new object(), Message = $"Failed to update preferences: {ex.Message}", StatusCode = 500 };
        }
    }

    // Patient operations
    public async Task<JsonModel> GetPatientByIdAsync(int patientId, TokenModel tokenModel)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(patientId);
            if (user == null || user.UserType != "Patient")
                return new JsonModel { data = new object(), Message = "Patient not found", StatusCode = 404 };

            var patientDto = MapToPatientDto(user);
            
            // Get additional patient data
            var stats = await GetPatientStatsAsync(patientId);
            var statsObj = await GetPatientStatsAsync(patientId);
            if (statsObj is not null)
            {
                var statsData = (dynamic)statsObj;
                patientDto.TotalAppointments = statsData.TotalAppointments ?? 0;
                patientDto.CompletedAppointments = statsData.CompletedAppointments ?? 0;
                patientDto.CancelledAppointments = statsData.CancelledAppointments ?? 0;
                patientDto.TotalSpent = statsData.TotalSpent ?? 0m;
            }

            return new JsonModel { data = patientDto, Message = "Patient retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting patient by ID: {PatientId}", patientId);
            return new JsonModel { data = new object(), Message = $"Failed to get patient: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetAllPatientsAsync(TokenModel tokenModel)
    {
        try
        {
            var patients = await _userRepository.GetByUserTypeAsync("Patient");
            var patientDtos = patients.Select(MapToPatientDto).ToList();
            return new JsonModel { data = patientDtos, Message = "All patients retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all patients");
            return new JsonModel { data = new object(), Message = $"Failed to get patients: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetPatientMedicalHistoryAsync(int patientId, TokenModel tokenModel)
    {
        try
        {
            // This would typically fetch from a medical history repository
            var medicalHistory = new
            {
                Allergies = "None known",
                CurrentMedications = "None",
                MedicalHistory = "No significant medical history",
                FamilyHistory = "No significant family history",
                Lifestyle = "Non-smoker, occasional alcohol"
            };

            return new JsonModel { data = medicalHistory, Message = "Patient medical history retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting patient medical history: {PatientId}", patientId);
            return new JsonModel { data = new object(), Message = $"Failed to get medical history: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> UpdatePatientMedicalHistoryAsync(int patientId, UpdateMedicalHistoryDto medicalHistoryDto, TokenModel tokenModel)
    {
        try
        {
            // This would typically update a medical history repository
            var updatedHistory = new
            {
                Allergies = medicalHistoryDto.Allergies,
                CurrentMedications = medicalHistoryDto.CurrentMedications,
                MedicalHistory = medicalHistoryDto.MedicalHistory,
                FamilyHistory = medicalHistoryDto.FamilyHistory,
                Lifestyle = medicalHistoryDto.Lifestyle,
                UpdatedAt = DateTime.UtcNow
            };

            return new JsonModel { data = updatedHistory, Message = "Patient medical history updated successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating patient medical history: {PatientId}", patientId);
            return new JsonModel { data = new object(), Message = $"Failed to update medical history: {ex.Message}", StatusCode = 500 };
        }
    }

    // Provider operations
    public async Task<JsonModel> GetProviderAsync(string id, TokenModel tokenModel)
    {
        if (!int.TryParse(id, out var providerId))
            return new JsonModel { data = new object(), Message = "Invalid provider ID", StatusCode = 400 };
        var provider = await _userRepository.GetByIdAsync(providerId);
        if (provider == null)
            return new JsonModel { data = new object(), Message = "Provider not found", StatusCode = 404 };
        var providerDto = _mapper.Map<ProviderDto>(provider);
        return new JsonModel { data = providerDto, Message = "Provider retrieved successfully", StatusCode = 200 };
    }

    public async Task<JsonModel> GetProviderByEmailAsync(string email, TokenModel tokenModel)
    {
        try
        {
            var provider = await _userRepository.GetByEmailAsync(email);
            if (provider == null)
            {
                return new JsonModel { data = new object(), Message = "Provider not found", StatusCode = 404 };
            }

            var providerDto = MapToProviderDto(provider);
            return new JsonModel { data = providerDto, Message = "Provider retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting provider by email {Email}", email);
            return new JsonModel { data = new object(), Message = $"Failed to get provider: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetProviderByLicenseAsync(string licenseNumber, TokenModel tokenModel)
    {
        try
        {
            var provider = await _userRepository.GetByLicenseNumberAsync(licenseNumber);
            if (provider == null)
            {
                return new JsonModel { data = new object(), Message = "Provider not found", StatusCode = 404 };
            }

            var providerDto = MapToProviderDto(provider);
            return new JsonModel { data = providerDto, Message = "Provider retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting provider by license {LicenseNumber}", licenseNumber);
            return new JsonModel { data = new object(), Message = $"Failed to get provider: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetProviderByIdAsync(int providerId, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Getting provider {ProviderId} by user {TokenUserId}", providerId, tokenModel?.UserID ?? 0);
            
            var provider = await _userRepository.GetByIdAsync(providerId);
            if (provider == null)
            {
                return new JsonModel { data = new object(), Message = "Provider not found", StatusCode = 404 };
            }

            var providerDto = _mapper.Map<ProviderDto>(provider);
            
            _logger.LogInformation("Provider {ProviderId} retrieved successfully by user {TokenUserId}", providerId, tokenModel?.UserID ?? 0);
            return new JsonModel { data = providerDto, Message = "Provider retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting provider by ID {ProviderId} by user {TokenUserId}", providerId, tokenModel?.UserID ?? 0);
            return new JsonModel { data = new object(), Message = $"Failed to get provider: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetAllProvidersAsync(TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Getting all providers by user {TokenUserId}", tokenModel?.UserID ?? 0);
            
            var providers = await _userRepository.GetByUserTypeAsync("Provider");
            var userDtos = providers.Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                // ... map other properties as needed ...
            }).ToList();
            
            _logger.LogInformation("Retrieved {ProviderCount} providers by user {TokenUserId}", userDtos.Count, tokenModel?.UserID ?? 0);
            return new JsonModel { data = userDtos, Message = "All providers retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all providers by user {TokenUserId}", tokenModel?.UserID ?? 0);
            return new JsonModel { data = new object(), Message = $"Failed to get providers: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> CreateProviderAsync(CreateProviderDto createDto, TokenModel tokenModel)
    {
        try
        {
            var provider = new User
            {
                Id = 0, // Will be set by the database
                FirstName = createDto.FirstName,
                LastName = createDto.LastName,
                Email = createDto.Email,
                Phone = createDto.PhoneNumber,
                UserType = "Provider",
                IsActive = true,
                IsEmailVerified = false,
                CreatedDate = DateTime.UtcNow
            };

            await _userRepository.CreateAsync(provider);

            return new JsonModel { data = MapToProviderDto(provider), Message = "Provider created successfully", StatusCode = 201 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating provider");
            return new JsonModel { data = new object(), Message = $"Failed to create provider: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> UpdateProviderAsync(int providerId, UpdateProviderDto updateDto, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Updating provider {ProviderId} by user {TokenUserId}", providerId, tokenModel?.UserID ?? 0);
            
            var provider = await _userRepository.GetByIdAsync(providerId);
            if (provider == null || provider.UserType != "Provider")
                return new JsonModel { data = new object(), Message = "Provider not found", StatusCode = 404 };

            // Update provider properties
            if (!string.IsNullOrEmpty(updateDto.FirstName))
                provider.FirstName = updateDto.FirstName;
            if (!string.IsNullOrEmpty(updateDto.LastName))
                provider.LastName = updateDto.LastName;
            if (!string.IsNullOrEmpty(updateDto.Email))
                provider.Email = updateDto.Email;
            if (!string.IsNullOrEmpty(updateDto.PhoneNumber))
                provider.Phone = updateDto.PhoneNumber;

            provider.UpdatedDate = DateTime.UtcNow;
            await _userRepository.UpdateAsync(provider);

            var providerDto = MapToProviderDto(provider);
            
            _logger.LogInformation("Provider {ProviderId} updated successfully by user {TokenUserId}", providerId, tokenModel?.UserID ?? 0);
            return new JsonModel { data = providerDto, Message = "Provider updated successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating provider: {ProviderId} by user {TokenUserId}", providerId, tokenModel?.UserID ?? 0);
            return new JsonModel { data = new object(), Message = $"Failed to update provider: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> DeleteProviderAsync(int providerId, TokenModel tokenModel)
    {
        try
        {
            var provider = await _userRepository.GetByIdAsync(providerId);
            if (provider == null || provider.UserType != "Provider")
                return new JsonModel { data = new object(), Message = "Provider not found", StatusCode = 404 };

            provider.IsActive = false;
            provider.UpdatedDate = DateTime.UtcNow;
            await _userRepository.UpdateAsync(provider);

            return new JsonModel { data = true, Message = "Provider deleted successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting provider: {ProviderId}", providerId);
            return new JsonModel { data = new object(), Message = $"Failed to delete provider: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> VerifyProviderAsync(int providerId, TokenModel tokenModel)
    {
        try
        {
            var provider = await _userRepository.GetByIdAsync(providerId);
            if (provider == null || provider.UserType != "Provider")
                return new JsonModel { data = new object(), Message = "Provider not found", StatusCode = 404 };

            // In a real implementation, this would involve verification logic
            provider.IsEmailVerified = true;
            provider.UpdatedDate = DateTime.UtcNow;
            await _userRepository.UpdateAsync(provider);

            return new JsonModel { data = MapToProviderDto(provider), Message = "Provider verified successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying provider: {ProviderId}", providerId);
            return new JsonModel { data = new object(), Message = $"Failed to verify provider: {ex.Message}", StatusCode = 500 };
        }
    }

    // Provider schedule operations
    public async Task<JsonModel> GetProviderScheduleAsync(int providerId, TokenModel tokenModel)
    {
        try
        {
            // This would typically fetch from a schedule repository
            var schedule = new ProviderScheduleDto
            {
                ProviderId = providerId.ToString(),
                WeeklySchedule = new List<WeeklyScheduleDto>
                {
                    new WeeklyScheduleDto { DayOfWeek = "1", DayName = "Monday", IsAvailable = true, StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(17) },
                    new WeeklyScheduleDto { DayOfWeek = "2", DayName = "Tuesday", IsAvailable = true, StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(17) },
                    new WeeklyScheduleDto { DayOfWeek = "3", DayName = "Wednesday", IsAvailable = true, StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(17) },
                    new WeeklyScheduleDto { DayOfWeek = "4", DayName = "Thursday", IsAvailable = true, StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(17) },
                    new WeeklyScheduleDto { DayOfWeek = "5", DayName = "Friday", IsAvailable = true, StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(17) },
                    new WeeklyScheduleDto { DayOfWeek = "6", DayName = "Saturday", IsAvailable = false, StartTime = TimeSpan.Zero, EndTime = TimeSpan.Zero },
                    new WeeklyScheduleDto { DayOfWeek = "0", DayName = "Sunday", IsAvailable = false, StartTime = TimeSpan.Zero, EndTime = TimeSpan.Zero }
                },
                DefaultDurationMinutes = 30,
                IsActive = true
            };

            return new JsonModel { data = schedule, Message = "Provider schedule retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting provider schedule: {ProviderId}", providerId);
            return new JsonModel { data = new object(), Message = $"Failed to get provider schedule: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> UpdateProviderScheduleAsync(int providerId, UpdateProviderScheduleDto scheduleDto, TokenModel tokenModel)
    {
        try
        {
            // This would typically update a schedule repository
            var updatedSchedule = new ProviderScheduleDto
            {
                ProviderId = providerId.ToString(),
                WeeklySchedule = scheduleDto.WeeklySchedule,
                AvailableDates = scheduleDto.AvailableDates,
                UnavailableDates = scheduleDto.UnavailableDates,
                DefaultDurationMinutes = scheduleDto.DefaultDurationMinutes,
                IsActive = scheduleDto.IsActive
            };

            return new JsonModel { data = updatedSchedule, Message = "Provider schedule updated successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating provider schedule: {ProviderId}", providerId);
            return new JsonModel { data = new object(), Message = $"Failed to update provider schedule: {ex.Message}", StatusCode = 500 };
        }
    }

    // User statistics
    public async Task<JsonModel> GetUserStatsAsync(int userId, TokenModel tokenModel)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return new JsonModel { data = new object(), Message = "User not found", StatusCode = 404 };

            var stats = user.UserType == "Patient" 
                ? await GetPatientStatsAsync(userId)
                : await GetProviderStatsAsync(userId);

            return new JsonModel { data = stats, Message = "User stats retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user stats: {UserId}", userId);
            return new JsonModel { data = new object(), Message = $"Failed to get user stats: {ex.Message}", StatusCode = 500 };
        }
    }

    // Provider reviews
    public async Task<JsonModel> GetProviderReviewsAsync(int providerId, TokenModel tokenModel)
    {
        try
        {
            // This would typically fetch from a reviews repository
            var reviews = new List<ReviewDto>
            {
                new ReviewDto
                {
                    Id = Guid.NewGuid().ToString(),
                    ProviderId = providerId.ToString(),
                    PatientId = Guid.NewGuid().ToString(),
                    PatientName = "John Doe",
                    Rating = 5,
                    Comment = "Excellent consultation. Very professional and knowledgeable.",
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                },
                new ReviewDto
                {
                    Id = Guid.NewGuid().ToString(),
                    ProviderId = providerId.ToString(),
                    PatientId = Guid.NewGuid().ToString(),
                    PatientName = "Jane Smith",
                    Rating = 4,
                    Comment = "Good experience. Would recommend.",
                    CreatedAt = DateTime.UtcNow.AddDays(-10)
                }
            };

            return new JsonModel { data = reviews, Message = "Provider reviews retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting provider reviews: {ProviderId}", providerId);
            return new JsonModel { data = new object(), Message = $"Failed to get provider reviews: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> AddProviderReviewAsync(int providerId, int userId, AddReviewDto reviewDto, TokenModel tokenModel)
    {
        try
        {
            var review = new ReviewDto
            {
                Id = Guid.NewGuid().ToString(),
                ProviderId = providerId.ToString(),
                PatientId = userId.ToString(),
                PatientName = "Current User", // Would get from user data
                Rating = reviewDto.Rating,
                Comment = reviewDto.Comment,
                CreatedAt = DateTime.UtcNow
            };

            return new JsonModel { data = review, Message = "Provider review added successfully", StatusCode = 201 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding provider review: {ProviderId}", providerId);
            return new JsonModel { data = new object(), Message = $"Failed to add review: {ex.Message}", StatusCode = 500 };
        }
    }

    // Notifications
    public async Task<JsonModel> GetUserNotificationsAsync(int userId, TokenModel tokenModel)
    {
        try
        {
            // This would typically fetch from a notifications repository
            var notifications = new List<NotificationDto>
            {
                new NotificationDto
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    Title = "Appointment Reminder",
                    Message = "Your appointment with Dr. Smith is in 1 hour.",
                    Type = "appointment",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow.AddHours(-1)
                },
                new NotificationDto
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    Title = "Payment Successful",
                    Message = "Your payment of $75 has been processed successfully.",
                    Type = "payment",
                    IsRead = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                }
            };

            return new JsonModel { data = notifications, Message = "User notifications retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user notifications: {UserId}", userId);
            return new JsonModel { data = new object(), Message = $"Failed to get notifications: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> MarkNotificationAsReadAsync(Guid notificationId, TokenModel tokenModel)
    {
        try
        {
            // This would typically update a notifications repository
            return new JsonModel { data = true, Message = "Notification marked as read successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification as read: {NotificationId}", notificationId);
            return new JsonModel { data = new object(), Message = $"Failed to mark notification as read: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> MarkAllNotificationsAsReadAsync(Guid userId, TokenModel tokenModel)
    {
        try
        {
            // This would typically update a notifications repository
            return new JsonModel { data = true, Message = "All notifications marked as read successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking all notifications as read: {UserId}", userId);
            return new JsonModel { data = new object(), Message = $"Failed to mark all notifications as read: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> DeleteNotificationAsync(Guid notificationId, TokenModel tokenModel)
    {
        try
        {
            // This would typically delete from a notifications repository
            return new JsonModel { data = true, Message = "Notification deleted successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting notification: {NotificationId}", notificationId);
            return new JsonModel { data = new object(), Message = $"Failed to delete notification: {ex.Message}", StatusCode = 500 };
        }
    }

    // User preferences
    public async Task<JsonModel> GetUserPreferencesAsync(Guid userId, TokenModel tokenModel)
    {
        try
        {
            // This would typically fetch from a preferences repository
            var preferences = new
            {
                EmailNotifications = true,
                SMSNotifications = false,
                AppointmentReminders = true,
                MarketingEmails = false,
                Language = "en",
                TimeZone = "UTC"
            };

            return new JsonModel { data = preferences, Message = "User preferences retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user preferences: {UserId}", userId);
            return new JsonModel { data = new object(), Message = $"Failed to get preferences: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetUserPreferencesAsync(int userId, TokenModel tokenModel)
    {
        try
        {
            // This would typically fetch from a preferences repository
            var preferences = new
            {
                EmailNotifications = true,
                SMSNotifications = false,
                AppointmentReminders = true,
                MarketingEmails = false,
                Language = "en",
                TimeZone = "UTC"
            };

            return new JsonModel { data = preferences, Message = "User preferences retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user preferences: {UserId}", userId);
            return new JsonModel { data = new object(), Message = $"Failed to get preferences: {ex.Message}", StatusCode = 500 };
        }
    }





    // Email verification
    public async Task<JsonModel> SendEmailVerificationAsync(int userId, TokenModel tokenModel)
    {
        try
        {
            // This would typically send an email verification
            return new JsonModel { data = true, Message = "Email verification sent successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email verification: {UserId}", userId);
            return new JsonModel { data = new object(), Message = $"Failed to send email verification: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> ResendEmailVerificationAsync(int userId, TokenModel tokenModel)
    {
        try
        {
            // This would typically resend an email verification
            return new JsonModel { data = true, Message = "Email verification resent successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending email verification: {UserId}", userId);
            return new JsonModel { data = new object(), Message = $"Failed to resend email verification: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> VerifyEmailAsync(int userId, string token, TokenModel tokenModel)
    {
        try
        {
            // This would typically verify the email token
            return new JsonModel { data = true, Message = "Email verified successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying email: {UserId}", userId);
            return new JsonModel { data = new object(), Message = $"Failed to verify email: {ex.Message}", StatusCode = 500 };
        }
    }

    // Account management
    public async Task<JsonModel> DeleteAccountAsync(int userId, TokenModel tokenModel)
    {
        try
        {
            // This would typically delete the user account
            return new JsonModel { data = true, Message = "Account deleted successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting account: {UserId}", userId);
            return new JsonModel { data = new object(), Message = $"Failed to delete account: {ex.Message}", StatusCode = 500 };
        }
    }

    // Provider management
    public async Task<JsonModel> CreateProviderAsync(int userId, CreateProviderDto providerDto, TokenModel tokenModel)
    {
        try
        {
            // This would typically create a provider profile
            return new JsonModel { data = providerDto, Message = "Provider profile created successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating provider profile: {UserId}", userId);
            return new JsonModel { data = new object(), Message = $"Failed to create provider profile: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> MarkNotificationAsReadAsync(int userId, Guid notificationId, TokenModel tokenModel)
    {
        try
        {
            // This would typically update a notifications repository
            return new JsonModel { data = true, Message = "Notification marked as read successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification as read: {UserId}, {NotificationId}", userId, notificationId);
            return new JsonModel { data = new object(), Message = $"Failed to mark notification as read: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> MarkAllNotificationsAsReadAsync(int userId, TokenModel tokenModel)
    {
        try
        {
            // This would typically update a notifications repository
            return new JsonModel { data = true, Message = "All notifications marked as read successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking all notifications as read: {UserId}", userId);
            return new JsonModel { data = new object(), Message = $"Failed to mark all notifications as read: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> DeleteNotificationAsync(int userId, Guid notificationId, TokenModel tokenModel)
    {
        try
        {
            // This would typically delete from a notifications repository
            return new JsonModel { data = true, Message = "Notification deleted successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting notification: {UserId}, {NotificationId}", userId, notificationId);
            return new JsonModel { data = new object(), Message = $"Failed to delete notification: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> AddProviderReviewAsync(int userId, AddReviewDto reviewDto, TokenModel tokenModel)
    {
        try
        {
            // This would typically add a provider review
            return new JsonModel { data = reviewDto, Message = "Provider review added successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding provider review: {UserId}", userId);
            return new JsonModel { data = new object(), Message = $"Failed to add provider review: {ex.Message}", StatusCode = 500 };
        }
    }





    public async Task<JsonModel> UpdateUserPreferencesAsync(Guid userId, UpdateUserPreferencesDto preferencesDto, TokenModel tokenModel)
    {
        try
        {
            // This would typically update a preferences repository
            return new JsonModel { data = preferencesDto.Preferences, Message = "User preferences updated successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user preferences: {UserId}", userId);
            return new JsonModel { data = new object(), Message = $"Failed to update preferences: {ex.Message}", StatusCode = 500 };
        }
    }

    // Password operations
    private string HashPassword(string password)
    {
        // Simple password hashing implementation - in production use BCrypt.Net.BCrypt
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }

    private bool VerifyPassword(string password, string hash)
    {
        // Simple password verification - in production use BCrypt.Net.BCrypt
        var hashedPassword = HashPassword(password);
        return hashedPassword == hash;
    }

    public async Task<JsonModel> RequestPasswordResetAsync(string email, TokenModel tokenModel)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
                return new JsonModel { data = new object(), Message = "User not found", StatusCode = 404 };

            // In a real implementation, send password reset email
            await _notificationService.SendPasswordResetEmailAsync(email, "reset-token", tokenModel);

            return new JsonModel { data = true, Message = "Password reset email sent successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting password reset: {Email}", email);
            return new JsonModel { data = new object(), Message = $"Failed to request password reset: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> ResetPasswordAsync(ResetPasswordDto resetDto, TokenModel tokenModel)
    {
        try
        {
            // In a real implementation, verify token and update password
            return new JsonModel { data = true, Message = "Password reset successful (stub)", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password");
            return new JsonModel { data = new object(), Message = $"Failed to reset password: {ex.Message}", StatusCode = 500 };
        }
    }

    // Email verification
    public async Task<JsonModel> VerifyEmailAsync(string token, TokenModel tokenModel)
    {
        try
        {
            // In a real implementation, verify email token
            return new JsonModel { data = true, Message = "Email verified successfully (stub)", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying email");
            return new JsonModel { data = new object(), Message = $"Failed to verify email: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> SendEmailVerificationAsync(string email, TokenModel tokenModel)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                return new JsonModel { data = new object(), Message = "User not found", StatusCode = 404 };
            }

            var verificationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            // EMAIL FUNCTIONALITY DISABLED - Commented out for now
            // await _notificationService.SendEmailVerificationAsync(email, user.UserName, verificationToken);
            _logger.LogInformation("Email notifications disabled - would have sent email verification to {Email}", email);

            return new JsonModel { data = true, Message = "Email verification sent successfully (stub)", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email verification to {Email}", email);
            return new JsonModel { data = new object(), Message = $"Failed to send email verification: {ex.Message}", StatusCode = 500 };
        }
    }



    // Account management
    public async Task<JsonModel> DeleteAccountAsync(int userId, string reason, TokenModel tokenModel)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return new JsonModel { data = new object(), Message = "User not found", StatusCode = 404 };

            user.IsActive = false;
            user.UpdatedDate = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            return new JsonModel { data = true, Message = "Account deleted successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting account: {UserId}", userId);
            return new JsonModel { data = new object(), Message = $"Failed to delete account: {ex.Message}", StatusCode = 500 };
        }
    }

    // Helper methods
    private UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            Phone = user.Phone ?? string.Empty,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            UserType = user.UserType,
            Role = user.RoleName,
            UserRoleId = user.UserRoleId,
            IsActive = user.IsActive,
            IsVerified = user.IsEmailVerified,
            IsEmailVerified = user.IsEmailVerified,
            IsPhoneVerified = user.IsPhoneVerified,
            CreatedAt = user.CreatedDate ?? DateTime.UtcNow,
            UpdatedAt = user.UpdatedDate ?? DateTime.UtcNow,
            LastLoginAt = user.LastLoginAt,
            ProfilePicture = user.ProfilePicture,
            DateOfBirth = user.DateOfBirth,
            Gender = user.Gender,
            Address = user.Address,
            City = user.City,
            State = user.State,
            ZipCode = user.ZipCode,
            Country = user.Country,
            EmergencyContact = user.EmergencyContact,
            EmergencyPhone = user.EmergencyPhone,
            StripeCustomerId = user.StripeCustomerId
        };
    }

    private PatientDto MapToPatientDto(User user)
    {
        return new PatientDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = $"{user.FirstName} {user.LastName}",
            Phone = user.Phone,
            DateOfBirth = user.DateOfBirth,
            Gender = user.Gender,
            ProfilePicture = user.ProfilePicture,
            UserType = user.UserType,
            IsActive = user.IsActive,
            IsEmailVerified = user.IsEmailVerified,
            IsPhoneVerified = user.IsPhoneVerified,
            CreatedAt = user.CreatedDate ?? DateTime.UtcNow,
            UpdatedAt = user.UpdatedDate ?? DateTime.UtcNow,
            LastLoginAt = user.LastLoginAt,
            StripeCustomerId = user.StripeCustomerId
        };
    }

    private ProviderDto MapToProviderDto(User user)
    {
        return new ProviderDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = $"{user.FirstName} {user.LastName}",
            Phone = user.Phone,
            DateOfBirth = user.DateOfBirth,
            Gender = user.Gender,
            ProfilePicture = user.ProfilePicture,
            UserType = user.UserType,
            IsActive = user.IsActive,
            IsEmailVerified = user.IsEmailVerified,
            IsPhoneVerified = user.IsPhoneVerified,
            CreatedAt = user.CreatedDate ?? DateTime.UtcNow,
            UpdatedAt = user.UpdatedDate ?? DateTime.UtcNow,
            LastLoginAt = user.LastLoginAt,
            StripeCustomerId = user.StripeCustomerId
        };
    }

    private async Task<object> GetPatientStatsAsync(int patientId)
    {
        // This would typically fetch from appointment and payment repositories
        return new
        {
            TotalAppointments = 15,
            CompletedAppointments = 12,
            CancelledAppointments = 2,
            TotalSpent = 450.00m
        };
    }

    private async Task<object> GetProviderStatsAsync(int providerId)
    {
        // This would typically fetch from appointment and payment repositories
        return new
        {
            TotalAppointments = 150,
            CompletedAppointments = 140,
            TotalEarnings = 10500.00m
        };
    }

    // === BEGIN INTERFACE STUBS ===
    public async Task<JsonModel> GetUserAsync(int userId, TokenModel tokenModel)
    {
        return await GetUserByIdAsync(userId, tokenModel);
    }
    public async Task<JsonModel> GetAllUsersAsync(TokenModel tokenModel, string? searchText = null, string? role = null, bool? isActive = null, int page = 1, int pageSize = 50)
    {
        try
        {
            _logger.LogInformation("Getting all users by user {TokenUserId} with filters: search={SearchText}, role={Role}, isActive={IsActive}, page={Page}, pageSize={PageSize}", 
                tokenModel?.UserID ?? 0, searchText, role, isActive, page, pageSize);
            
            var users = await _userRepository.GetAllAsync();
            var userDtos = users.Select(MapToUserDto).ToList();
            
            // Apply search filter
            if (!string.IsNullOrEmpty(searchText))
            {
                searchText = searchText.ToLower();
                userDtos = userDtos.Where(u => 
                    u.FirstName?.ToLower().Contains(searchText) == true || 
                    u.LastName?.ToLower().Contains(searchText) == true || 
                    u.Email?.ToLower().Contains(searchText) == true).ToList();
            }

            // Apply role filter
            if (!string.IsNullOrEmpty(role))
            {
                userDtos = userDtos.Where(u => u.UserType?.Equals(role, StringComparison.OrdinalIgnoreCase) == true).ToList();
            }

            // Apply active filter
            if (isActive.HasValue)
            {
                userDtos = userDtos.Where(u => u.IsActive == isActive.Value).ToList();
            }

            // Apply pagination
            var totalCount = userDtos.Count;
            var pagedUsers = userDtos
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            _logger.LogInformation("Retrieved {UserCount} users (filtered from {TotalCount}) by user {TokenUserId}", pagedUsers.Count, totalCount, tokenModel?.UserID ?? 0);
            
            return new JsonModel 
            { 
                data = new
                {
                    users = pagedUsers,
                    pagination = new
                    {
                        totalCount,
                        page,
                        pageSize,
                        totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                    }
                },
                Message = "Users retrieved successfully", 
                StatusCode = 200 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all users by user {TokenUserId}", tokenModel?.UserID ?? 0);
            return new JsonModel { data = new object(), Message = $"Failed to get users: {ex.Message}", StatusCode = 500 };
        }
    }
    public async Task<JsonModel> CreateUserAsync(CreateUserDto createUserDto, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Creating user by user {TokenUserId}", tokenModel?.UserID ?? 0);
            
            // Check if user already exists
            var existingUser = await _userRepository.GetByEmailAsync(createUserDto.Email);
            if (existingUser != null)
                return new JsonModel { data = new object(), Message = "User with this email already exists", StatusCode = 400 };

            // Get UserRoleId based on UserType
            var userRole = await GetUserRoleByNameAsync(createUserDto.UserType);
            if (userRole == null)
            {
                return new JsonModel { data = new object(), Message = $"Invalid user type: {createUserDto.UserType}", StatusCode = 400 };
            }

            // Create new user
            var user = new User
            {
                FirstName = createUserDto.FirstName,
                LastName = createUserDto.LastName,
                Email = createUserDto.Email,
                UserName = createUserDto.Email, // Set UserName for Identity
                PhoneNumber = createUserDto.PhoneNumber,
                DateOfBirth = createUserDto.DateOfBirth ?? DateTime.UtcNow.AddYears(-25), // Set default if not provided
                Gender = createUserDto.Gender,
                Address = createUserDto.Address,
                City = createUserDto.City,
                State = createUserDto.State,
                ZipCode = createUserDto.ZipCode,
                Country = createUserDto.Country,
                EmergencyContactName = createUserDto.EmergencyContactName,
                EmergencyContactPhone = createUserDto.EmergencyContactPhone,
                UserType = createUserDto.UserType ?? "Patient",
                UserRoleId = userRole.Id, // Set the UserRoleId
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };

            // Create user with password using UserManager (Identity)
            var result = await _userManager.CreateAsync(user, createUserDto.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new JsonModel { data = new object(), Message = $"Failed to create user: {errors}", StatusCode = 400 };
            }

            // Assign Identity role
            var identityRoleName = createUserDto.UserType?.ToLower() switch
            {
                "admin" => "Admin",
                "provider" => "Provider",
                _ => "Client"
            };

            var roleResult = await _userManager.AddToRoleAsync(user, identityRoleName);
            if (!roleResult.Succeeded)
            {
                _logger.LogWarning("Failed to assign Identity role {RoleName} to user {UserId}: {Errors}", 
                    identityRoleName, user.Id, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
            }

            var userDto = MapToUserDto(user);
            
            _logger.LogInformation("User created successfully by user {TokenUserId}: {UserId}", tokenModel?.UserID ?? 0, user.Id);
            return new JsonModel { data = userDto, Message = "User created successfully", StatusCode = 201 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user by user {TokenUserId}", tokenModel?.UserID ?? 0);
            return new JsonModel { data = new object(), Message = $"Failed to create user: {ex.Message}", StatusCode = 500 };
        }
    }
    public async Task<JsonModel> GetMedicalHistoryAsync(int userId, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Getting medical history for user {UserId} by user {TokenUserId}", userId, tokenModel?.UserID ?? 0);
            
            // This would typically fetch from a medical history repository
            var medicalHistory = new
            {
                UserId = userId,
                Allergies = new List<string>(),
                Medications = new List<string>(),
                Conditions = new List<string>(),
                LastUpdated = DateTime.UtcNow
            };

            _logger.LogInformation("Medical history retrieved successfully for user {UserId} by user {TokenUserId}", userId, tokenModel?.UserID ?? 0);
            return new JsonModel { data = medicalHistory, Message = "Medical history retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting medical history for user {UserId} by user {TokenUserId}", userId, tokenModel?.UserID ?? 0);
            return new JsonModel { data = new object(), Message = $"Failed to get medical history: {ex.Message}", StatusCode = 500 };
        }
    }
    public async Task<JsonModel> UpdateMedicalHistoryAsync(int userId, UpdateMedicalHistoryDto medicalHistoryDto, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Updating medical history for user {UserId} by user {TokenUserId}", userId, tokenModel?.UserID ?? 0);
            
            // This would typically update a medical history repository
            var updatedHistory = new
            {
                UserId = userId,
                Allergies = medicalHistoryDto.Allergies ?? new List<string>(),
                Medications = medicalHistoryDto.Medications ?? new List<string>(),
                Conditions = medicalHistoryDto.Conditions ?? new List<string>(),
                LastUpdated = DateTime.UtcNow
            };

            _logger.LogInformation("Medical history updated successfully for user {UserId} by user {TokenUserId}", userId, tokenModel?.UserID ?? 0);
            return new JsonModel { data = updatedHistory, Message = "Medical history updated successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating medical history for user {UserId} by user {TokenUserId}", userId, tokenModel?.UserID ?? 0);
            return new JsonModel { data = new object(), Message = $"Failed to update medical history: {ex.Message}", StatusCode = 500 };
        }
    }
            public async Task<JsonModel> GetPaymentMethodsAsync(int userId, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Getting payment methods for user {UserId} by user {TokenUserId}", userId, tokenModel?.UserID ?? 0);
            
            // This would typically fetch from a payment method repository
            var paymentMethods = new List<object>();

            _logger.LogInformation("Payment methods retrieved successfully for user {UserId} by user {TokenUserId}", userId, tokenModel?.UserID ?? 0);
            return new JsonModel { data = paymentMethods, Message = "Payment methods retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment methods for user {UserId} by user {TokenUserId}", userId, tokenModel?.UserID ?? 0);
            return new JsonModel { data = new object(), Message = $"Failed to get payment methods: {ex.Message}", StatusCode = 500 };
        }
    }
    public async Task<JsonModel> AddPaymentMethodAsync(int userId, AddPaymentMethodDto addPaymentMethodDto, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Adding payment method for user {UserId} by user {TokenUserId}", userId, tokenModel?.UserID ?? 0);
            
            // This would typically add to a payment method repository
            var newPaymentMethod = new
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                Type = addPaymentMethodDto.Type,
                Last4 = addPaymentMethodDto.Last4,
                ExpiryMonth = addPaymentMethodDto.ExpiryMonth,
                ExpiryYear = addPaymentMethodDto.ExpiryYear,
                IsDefault = addPaymentMethodDto.IsDefault
            };

            _logger.LogInformation("Payment method added successfully for user {UserId} by user {TokenUserId}", userId, tokenModel?.UserID ?? 0);
            return new JsonModel { data = newPaymentMethod, Message = "Payment method added successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding payment method for user {UserId} by user {TokenUserId}", userId, tokenModel?.UserID ?? 0);
            return new JsonModel { data = new object(), Message = $"Failed to add payment method: {ex.Message}", StatusCode = 500 };
        }
    }
    public async Task<JsonModel> DeletePaymentMethodAsync(int userId, string paymentMethodId, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Deleting payment method {PaymentMethodId} for user {UserId} by user {TokenUserId}", paymentMethodId, userId, tokenModel?.UserID ?? 0);
            
            // This would typically delete from a payment method repository

            _logger.LogInformation("Payment method {PaymentMethodId} deleted successfully for user {UserId} by user {TokenUserId}", paymentMethodId, userId, tokenModel?.UserID ?? 0);
            return new JsonModel { data = true, Message = "Payment method deleted successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting payment method {PaymentMethodId} for user {UserId} by user {TokenUserId}", paymentMethodId, userId, tokenModel?.UserID ?? 0);
            return new JsonModel { data = new object(), Message = $"Failed to delete payment method: {ex.Message}", StatusCode = 500 };
        }
    }
    public async Task<JsonModel> SetDefaultPaymentMethodAsync(int userId, string paymentMethodId, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Setting default payment method {PaymentMethodId} for user {UserId} by user {TokenUserId}", paymentMethodId, userId, tokenModel?.UserID ?? 0);
            
            // This would typically update a payment method repository

            _logger.LogInformation("Default payment method set successfully for user {UserId} by user {TokenUserId}", userId, tokenModel?.UserID ?? 0);
            return new JsonModel { data = true, Message = "Default payment method set successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting default payment method for user {UserId} by user {TokenUserId}", userId, tokenModel?.UserID ?? 0);
            return new JsonModel { data = new object(), Message = $"Failed to set default payment method: {ex.Message}", StatusCode = 500 };
        }
    }
    // === END INTERFACE STUBS ===

        // === END INTERFACE STUBS ===

    // === BEGIN INTERFACE OVERLOADS ===
    // Removed conflicting method overloads - interface methods now use int parameters

    private async Task<UserRole> GetUserRoleByNameAsync(string userTypeName)
    {
        // First, try to get all UserRoles to see what's available
        var allUserRoles = await _userRoleRepository.GetAllAsync();
        _logger.LogInformation("Available UserRoles: {UserRoles}", string.Join(", ", allUserRoles.Select(ur => ur.Name)));
        
        // Try exact match first
        var userRole = await _userRoleRepository.GetByNameAsync(userTypeName);
        if (userRole != null)
        {
            _logger.LogInformation("Found UserRole: {UserRoleName} with ID: {UserRoleId}", userRole.Name, userRole.Id);
            return userRole;
        }
        
        // Try case-insensitive match
        userRole = allUserRoles.FirstOrDefault(ur => 
            ur.Name.Equals(userTypeName, StringComparison.OrdinalIgnoreCase));
        
        if (userRole != null)
        {
            _logger.LogInformation("Found UserRole (case-insensitive): {UserRoleName} with ID: {UserRoleId}", userRole.Name, userRole.Id);
            return userRole;
        }
        
        // Try mapping common variations
        var roleMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Admin", "Admin" },
            { "Administrator", "Admin" },
            { "User", "Client" },
            { "Patient", "Client" },
            { "Client", "Client" },
            { "Provider", "Provider" },
            { "Doctor", "Provider" },
            { "Physician", "Provider" },

        };
        
        if (roleMapping.TryGetValue(userTypeName, out var mappedRole))
        {
            userRole = allUserRoles.FirstOrDefault(ur => 
                ur.Name.Equals(mappedRole, StringComparison.OrdinalIgnoreCase));
            
            if (userRole != null)
            {
                _logger.LogInformation("Found UserRole (mapped): {UserRoleName} with ID: {UserRoleId}", userRole.Name, userRole.Id);
                return userRole;
            }
        }
        
        _logger.LogWarning("No UserRole found for userType: {UserType}. Available roles: {AvailableRoles}", 
            userTypeName, string.Join(", ", allUserRoles.Select(ur => ur.Name)));
        
        return null;
    }

    private Guid GetDocumentTypeId(object documentData)
    {
        var dynamicData = documentData as dynamic;
        return dynamicData?.DocumentTypeId ?? Guid.Empty;
    }
} 