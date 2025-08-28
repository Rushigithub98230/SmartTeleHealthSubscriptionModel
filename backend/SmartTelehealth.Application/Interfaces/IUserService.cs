using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;
using Microsoft.AspNetCore.Http;

namespace SmartTelehealth.Application.Interfaces
{
    public interface IUserService
    {
        // --- AUTHENTICATION ---
        Task<JsonModel> AuthenticateUserAsync(string email, string password, TokenModel tokenModel);
        Task<JsonModel> GetUserByEmailAsync(string email, TokenModel tokenModel);
        
        // --- USER MANAGEMENT ---
        Task<JsonModel> GetUserByIdAsync(int userId, TokenModel tokenModel);
        Task<JsonModel> UpdateUserAsync(int userId, UpdateUserDto updateDto, TokenModel tokenModel);
        Task<JsonModel> DeleteUserAsync(int userId, TokenModel tokenModel);
        Task<JsonModel> GetUsersByRoleAsync(string role, TokenModel tokenModel);
        Task<JsonModel> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto, TokenModel tokenModel);
        Task<JsonModel> ResetPasswordAsync(string email, TokenModel tokenModel);
        Task<JsonModel> ResetPasswordAsync(ResetPasswordDto resetDto, TokenModel tokenModel);
        Task<JsonModel> ConfirmPasswordResetAsync(string email, string resetToken, string newPassword, TokenModel tokenModel);
        Task<JsonModel> UpdateUserProfileAsync(int userId, UpdateUserProfileDto profileDto, TokenModel tokenModel);
        Task<JsonModel> UpdateUserPreferencesAsync(int userId, UpdateUserPreferencesDto preferencesDto, TokenModel tokenModel);
        Task<JsonModel> GetUserPreferencesAsync(int userId, TokenModel tokenModel);
        
        // --- DOCUMENT MANAGEMENT (Updated to use centralized DocumentService) ---
        Task<JsonModel> UploadProfilePictureAsync(int userId, IFormFile file, TokenModel tokenModel);
        Task<JsonModel> GetUserDocumentsAsync(int userId, string? referenceType, TokenModel tokenModel);
        Task<JsonModel> UploadUserDocumentAsync(int userId, UploadUserDocumentRequest request, TokenModel tokenModel);
        Task<JsonModel> DeleteUserDocumentAsync(Guid documentId, int userId, TokenModel tokenModel);
        
        // --- PROVIDER MANAGEMENT ---
        Task<JsonModel> GetProviderByIdAsync(int providerId, TokenModel tokenModel);
        Task<JsonModel> UpdateProviderAsync(int providerId, UpdateProviderDto updateProviderDto, TokenModel tokenModel);
        Task<JsonModel> GetAllProvidersAsync(TokenModel tokenModel);
        
        // --- MEDICAL HISTORY ---
        Task<JsonModel> GetMedicalHistoryAsync(int userId, TokenModel tokenModel);
        Task<JsonModel> UpdateMedicalHistoryAsync(int userId, UpdateMedicalHistoryDto medicalHistoryDto, TokenModel tokenModel);
        
        // --- PAYMENT METHODS ---
        Task<JsonModel> GetPaymentMethodsAsync(int userId, TokenModel tokenModel);
        Task<JsonModel> AddPaymentMethodAsync(int userId, AddPaymentMethodDto addPaymentMethodDto, TokenModel tokenModel);
        Task<JsonModel> DeletePaymentMethodAsync(int userId, string paymentMethodId, TokenModel tokenModel);
        Task<JsonModel> SetDefaultPaymentMethodAsync(int userId, string paymentMethodId, TokenModel tokenModel);
        
        // --- ADDITIONAL METHODS ---
        Task<JsonModel> GetUserAsync(int userId, TokenModel tokenModel);
        Task<JsonModel> GetAllUsersAsync(TokenModel tokenModel, string? searchText = null, string? role = null, bool? isActive = null, int page = 1, int pageSize = 50);
        Task<JsonModel> CreateUserAsync(CreateUserDto createUserDto, TokenModel tokenModel);
        
        // --- NOTIFICATION METHODS ---
        Task<JsonModel> GetUserNotificationsAsync(int userId, TokenModel tokenModel);
        Task<JsonModel> MarkNotificationAsReadAsync(int userId, Guid notificationId, TokenModel tokenModel);
        Task<JsonModel> MarkAllNotificationsAsReadAsync(int userId, TokenModel tokenModel);
        Task<JsonModel> DeleteNotificationAsync(int userId, Guid notificationId, TokenModel tokenModel);
        
        // --- USER STATS ---
        Task<JsonModel> GetUserStatsAsync(int userId, TokenModel tokenModel);
        
        // --- EMAIL VERIFICATION ---
        Task<JsonModel> SendEmailVerificationAsync(int userId, TokenModel tokenModel);
        Task<JsonModel> ResendEmailVerificationAsync(int userId, TokenModel tokenModel);
        Task<JsonModel> VerifyEmailAsync(int userId, string token, TokenModel tokenModel);
        
        // --- ACCOUNT MANAGEMENT ---
        Task<JsonModel> DeleteAccountAsync(int userId, TokenModel tokenModel);
        
        // --- PROVIDER MANAGEMENT ---
        Task<JsonModel> CreateProviderAsync(int userId, CreateProviderDto providerDto, TokenModel tokenModel);
        Task<JsonModel> DeleteProviderAsync(int userId, TokenModel tokenModel);
        Task<JsonModel> VerifyProviderAsync(int userId, TokenModel tokenModel);
        Task<JsonModel> GetProviderScheduleAsync(int userId, TokenModel tokenModel);
        Task<JsonModel> UpdateProviderScheduleAsync(int userId, UpdateProviderScheduleDto scheduleDto, TokenModel tokenModel);
        Task<JsonModel> GetProviderReviewsAsync(int userId, TokenModel tokenModel);
        Task<JsonModel> AddProviderReviewAsync(int userId, AddReviewDto reviewDto, TokenModel tokenModel);
        
        // --- PATIENT MANAGEMENT ---
        Task<JsonModel> GetPatientByIdAsync(int patientId, TokenModel tokenModel);
        Task<JsonModel> GetAllPatientsAsync(TokenModel tokenModel);
        Task<JsonModel> GetPatientMedicalHistoryAsync(int patientId, TokenModel tokenModel);
        Task<JsonModel> UpdatePatientMedicalHistoryAsync(int patientId, UpdateMedicalHistoryDto medicalHistoryDto, TokenModel tokenModel);
    }
} 