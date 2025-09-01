using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace SmartTelehealth.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : BaseController
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Get current user profile
        /// </summary>
        [HttpGet("profile")]
        //[Authorize]
        public async Task<JsonModel> GetCurrentUserProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            {
                return new JsonModel 
                { 
                    data = new object(), 
                    Message = "User not authenticated",
                    StatusCode = 401
                };
            }

            return await _userService.GetUserByIdAsync(userIdInt, GetToken(HttpContext));
        }

        /// <summary>
        /// Update current user profile
        /// </summary>
        [HttpPut("profile")]
        //[Authorize]
        public async Task<JsonModel> UpdateProfile([FromBody] UpdateUserDto updateDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            {
                return new JsonModel 
                { 
                    data = new object(), 
                    Message = "User not authenticated",
                    StatusCode = 401
                };
            }

            updateDto.Id = userIdInt;
            return await _userService.UpdateUserAsync(userIdInt, updateDto, GetToken(HttpContext));
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        [HttpGet("{userId}")]
        //[Authorize]
        public async Task<JsonModel> GetUser(int userId)
        {
            return await _userService.GetUserByIdAsync(userId, GetToken(HttpContext));
        }

        /// <summary>
        /// Get all users with optional filtering and search
        /// </summary>
        [HttpGet]
        //[Authorize]
        public async Task<JsonModel> GetAllUsers(
            [FromQuery] string? searchText = null,
            [FromQuery] string? role = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            return await _userService.GetAllUsersAsync(GetToken(HttpContext), searchText, role, isActive, page, pageSize);
        }

        /// <summary>
        /// Create new user
        /// </summary>
        [HttpPost]
        //[Authorize]
        public async Task<JsonModel> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            return await _userService.CreateUserAsync(createUserDto, GetToken(HttpContext));
        }

        /// <summary>
        /// Update user
        /// </summary>
        [HttpPut("{userId}")]
        //[Authorize]
        public async Task<JsonModel> UpdateUser(int userId, [FromBody] UpdateUserDto updateDto)
        {
            return await _userService.UpdateUserAsync(userId, updateDto, GetToken(HttpContext));
        }

        /// <summary>
        /// Delete user
        /// </summary>
        [HttpDelete("{userId}")]
        //[Authorize]
        public async Task<JsonModel> DeleteUser(int userId)
        {
            return await _userService.DeleteUserAsync(userId, GetToken(HttpContext));
        }

        /// <summary>
        /// Get users by role
        /// </summary>
        [HttpGet("role/{role}")]
        //[Authorize]
        public async Task<JsonModel> GetUsersByRole(string role)
        {
            return await _userService.GetUsersByRoleAsync(role, GetToken(HttpContext));
        }

        /// <summary>
        /// Change password
        /// </summary>
        [HttpPost("change-password")]
        //[Authorize]
        public async Task<JsonModel> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            {
                return new JsonModel 
                { 
                    data = new object(), 
                    Message = "User not authenticated",
                    StatusCode = 401
                };
            }

            return await _userService.ChangePasswordAsync(userIdInt, changePasswordDto, GetToken(HttpContext));
        }

        /// <summary>
        /// Request password reset
        /// </summary>
        [HttpPost("request-password-reset")]
        public async Task<JsonModel> RequestPasswordReset([FromBody] string email)
        {
            return await _userService.ResetPasswordAsync(email, GetToken(HttpContext));
        }

        /// <summary>
        /// Reset password
        /// </summary>
        [HttpPost("reset-password")]
        public async Task<JsonModel> ResetPassword([FromBody] ResetPasswordDto resetDto)
        {
            return await _userService.ResetPasswordAsync(resetDto, GetToken(HttpContext));
        }

        /// <summary>
        /// Confirm password reset
        /// </summary>
        [HttpPost("confirm-password-reset")]
        public async Task<JsonModel> ConfirmPasswordReset([FromBody] ConfirmPasswordResetDto confirmDto)
        {
            return await _userService.ConfirmPasswordResetAsync(confirmDto.Email, confirmDto.ResetToken, confirmDto.NewPassword, GetToken(HttpContext));
        }

        /// <summary>
        /// Upload profile picture
        /// </summary>
        [HttpPost("profile-picture")]
        //[Authorize]
        public async Task<JsonModel> UploadProfilePicture(IFormFile file)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            {
                return new JsonModel 
                { 
                    data = new object(), 
                    Message = "User not authenticated",
                    StatusCode = 401
                };
            }

            return await _userService.UploadProfilePictureAsync(userIdInt, file, GetToken(HttpContext));
        }

        /// <summary>
        /// Get user documents
        /// </summary>
        [HttpGet("documents")]
        //[Authorize]
        public async Task<JsonModel> GetUserDocuments([FromQuery] string? referenceType = null)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            {
                return new JsonModel 
                { 
                    data = new object(), 
                    Message = "User not authenticated",
                    StatusCode = 401
                };
            }

            return await _userService.GetUserDocumentsAsync(userIdInt, referenceType, GetToken(HttpContext));
        }

        /// <summary>
        /// Upload user document
        /// </summary>
        [HttpPost("documents")]
        //[Authorize]
        public async Task<JsonModel> UploadUserDocument([FromBody] UploadUserDocumentRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            {
                return new JsonModel 
                { 
                    data = new object(), 
                    Message = "User not authenticated",
                    StatusCode = 401
                };
            }

            return await _userService.UploadUserDocumentAsync(userIdInt, request, GetToken(HttpContext));
        }

        /// <summary>
        /// Delete user document
        /// </summary>
        [HttpDelete("documents/{documentId}")]
        //[Authorize]
        public async Task<JsonModel> DeleteUserDocument(Guid documentId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            {
                return new JsonModel 
                { 
                    data = new object(), 
                    Message = "User not authenticated",
                    StatusCode = 401
                };
            }

            return await _userService.DeleteUserDocumentAsync(documentId, userIdInt, GetToken(HttpContext));
        }

        /// <summary>
        /// Get user medical history
        /// </summary>
        [HttpGet("medical-history")]
        //[Authorize]
        public async Task<JsonModel> GetMedicalHistory()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            {
                return new JsonModel 
                { 
                    data = new object(), 
                    Message = "User not authenticated",
                    StatusCode = 401
                };
            }

            return await _userService.GetMedicalHistoryAsync(userIdInt, GetToken(HttpContext));
        }

        /// <summary>
        /// Update user medical history
        /// </summary>
        [HttpPut("medical-history")]
        //[Authorize]
        public async Task<JsonModel> UpdateMedicalHistory([FromBody] UpdateMedicalHistoryDto medicalHistoryDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            {
                return new JsonModel 
                { 
                    data = new object(), 
                    Message = "User not authenticated",
                    StatusCode = 401
                };
            }

            return await _userService.UpdateMedicalHistoryAsync(userIdInt, medicalHistoryDto, GetToken(HttpContext));
        }

        /// <summary>
        /// Get user preferences
        /// </summary>
        [HttpGet("preferences")]
        //[Authorize]
        public async Task<JsonModel> GetUserPreferences()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            {
                return new JsonModel 
                { 
                    data = new object(), 
                    Message = "User not authenticated",
                    StatusCode = 401
                };
            }

            return await _userService.GetUserPreferencesAsync(userIdInt, GetToken(HttpContext));
        }

        /// <summary>
        /// Update user preferences
        /// </summary>
        [HttpPut("preferences")]
        //[Authorize]
        public async Task<JsonModel> UpdateUserPreferences([FromBody] UpdateUserPreferencesDto preferencesDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            {
                return new JsonModel 
                { 
                    data = new object(), 
                    Message = "User not authenticated",
                    StatusCode = 401
                };
            }

            return await _userService.UpdateUserPreferencesAsync(userIdInt, preferencesDto, GetToken(HttpContext));
        }

        /// <summary>
        /// Get user notifications
        /// </summary>
        [HttpGet("notifications")]
        //[Authorize]
        public async Task<JsonModel> GetUserNotifications()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            {
                return new JsonModel 
                { 
                    data = new object(), 
                    Message = "User not authenticated",
                    StatusCode = 401
                };
            }

            return await _userService.GetUserNotificationsAsync(userIdInt, GetToken(HttpContext));
        }

        /// <summary>
        /// Mark notification as read
        /// </summary>
        [HttpPut("notifications/{notificationId}/read")]
        //[Authorize]
        public async Task<JsonModel> MarkNotificationAsRead(Guid notificationId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            {
                return new JsonModel 
                { 
                    data = new object(), 
                    Message = "User not authenticated",
                    StatusCode = 401
                };
            }
            return await _userService.MarkNotificationAsReadAsync(userIdInt, notificationId, GetToken(HttpContext));
        }

        /// <summary>
        /// Mark all notifications as read
        /// </summary>
        [HttpPut("notifications/read-all")]
        //[Authorize]
        public async Task<JsonModel> MarkAllNotificationsAsRead()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            {
                return new JsonModel 
                { 
                    data = new object(), 
                    Message = "User not authenticated",
                    StatusCode = 401
                };
            }

            return await _userService.MarkAllNotificationsAsReadAsync(userIdInt, GetToken(HttpContext));
        }

        /// <summary>
        /// Delete notification
        /// </summary>
        [HttpDelete("notifications/{notificationId}")]
        //[Authorize]
        public async Task<JsonModel> DeleteNotification(Guid notificationId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            {
                return new JsonModel 
                { 
                    data = new object(), 
                    Message = "User not authenticated",
                    StatusCode = 401
                };
            }
            return await _userService.DeleteNotificationAsync(userIdInt, notificationId, GetToken(HttpContext));
        }

        /// <summary>
        /// Get user stats
        /// </summary>
        [HttpGet("stats")]
        //[Authorize]
        public async Task<JsonModel> GetUserStats()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            {
                return new JsonModel 
                { 
                    data = new object(), 
                    Message = "User not authenticated",
                    StatusCode = 401
                };
            }

            return await _userService.GetUserStatsAsync(userIdInt, GetToken(HttpContext));
        }

        /// <summary>
        /// Get payment methods
        /// </summary>
        [HttpGet("payment-methods")]
        //[Authorize]
        public async Task<JsonModel> GetPaymentMethods()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            {
                return new JsonModel 
                { 
                    data = new object(), 
                    Message = "User not authenticated",
                    StatusCode = 401
                };
            }

            return await _userService.GetPaymentMethodsAsync(userIdInt, GetToken(HttpContext));
        }

        /// <summary>
        /// Add payment method
        /// </summary>
        [HttpPost("payment-methods")]
        //[Authorize]
        public async Task<JsonModel> AddPaymentMethod([FromBody] AddPaymentMethodDto addPaymentMethodDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            {
                return new JsonModel 
                { 
                    data = new object(), 
                    Message = "User not authenticated",
                    StatusCode = 401
                };
            }

            return await _userService.AddPaymentMethodAsync(userIdInt, addPaymentMethodDto, GetToken(HttpContext));
        }

        /// <summary>
        /// Delete payment method
        /// </summary>
        [HttpDelete("payment-methods/{paymentMethodId}")]
        //[Authorize]
        public async Task<JsonModel> DeletePaymentMethod(string paymentMethodId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            {
                return new JsonModel 
                { 
                    data = new object(), 
                    Message = "User not authenticated",
                    StatusCode = 401
                };
            }

            return await _userService.DeletePaymentMethodAsync(userIdInt, paymentMethodId, GetToken(HttpContext));
        }

        /// <summary>
        /// Set default payment method
        /// </summary>
        [HttpPut("payment-methods/{paymentMethodId}/default")]
        //[Authorize]
        public async Task<JsonModel> SetDefaultPaymentMethod(string paymentMethodId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            {
                return new JsonModel 
                { 
                    data = new object(), 
                    Message = "User not authenticated",
                    StatusCode = 401
                };
            }

            return await _userService.SetDefaultPaymentMethodAsync(userIdInt, paymentMethodId, GetToken(HttpContext));
        }

        /// <summary>
        /// Send email verification
        /// </summary>
        [HttpPost("send-email-verification")]
        //[Authorize]
        public async Task<JsonModel> SendEmailVerification()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            {
                return new JsonModel 
                { 
                    data = new object(), 
                    Message = "User not authenticated",
                    StatusCode = 401
                };
            }

            return await _userService.SendEmailVerificationAsync(userIdInt, GetToken(HttpContext));
        }

        /// <summary>
        /// Resend email verification
        /// </summary>
        [HttpPost("resend-email-verification")]
        //[Authorize]
        public async Task<JsonModel> ResendEmailVerification()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            {
                return new JsonModel 
                { 
                    data = new object(), 
                    Message = "User not authenticated",
                    StatusCode = 401
                };
            }

            return await _userService.ResendEmailVerificationAsync(userIdInt, GetToken(HttpContext));
        }

        /// <summary>
        /// Verify email
        /// </summary>
        [HttpPost("verify-email")]
        public async Task<JsonModel> VerifyEmail([FromBody] string token)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            {
                return new JsonModel 
                { 
                    data = new object(), 
                    Message = "User not authenticated",
                    StatusCode = 401
                };
            }
            return await _userService.VerifyEmailAsync(userIdInt, token, GetToken(HttpContext));
        }

        /// <summary>
        /// Delete account
        /// </summary>
        [HttpDelete("account")]
        //[Authorize]
        public async Task<JsonModel> DeleteAccount([FromBody] string reason)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            {
                return new JsonModel 
                { 
                    data = new object(), 
                    Message = "User not authenticated",
                    StatusCode = 401
                };
            }

            return await _userService.DeleteAccountAsync(userIdInt, GetToken(HttpContext));
        }

        // === PROVIDER OPERATIONS ===

        /// <summary>
        /// Get provider by ID
        /// </summary>
        [HttpGet("providers/{providerId}")]
        //[Authorize]
        public async Task<JsonModel> GetProvider(int providerId)
        {
            return await _userService.GetProviderByIdAsync(providerId, GetToken(HttpContext));
        }

        /// <summary>
        /// Get all providers
        /// </summary>
        [HttpGet("providers")]
        //[Authorize]
        public async Task<JsonModel> GetAllProviders()
        {
            return await _userService.GetAllProvidersAsync(GetToken(HttpContext));
        }

        /// <summary>
        /// Create provider
        /// </summary>
        [HttpPost("providers")]
        //[Authorize]
        public async Task<JsonModel> CreateProvider([FromBody] CreateProviderDto createDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            {
                return new JsonModel 
                { 
                    data = new object(), 
                    Message = "User not authenticated",
                    StatusCode = 401
                };
            }
            return await _userService.CreateProviderAsync(userIdInt, createDto, GetToken(HttpContext));
        }

        /// <summary>
        /// Update provider
        /// </summary>
        [HttpPut("providers/{providerId}")]
        //[Authorize]
        public async Task<JsonModel> UpdateProvider(int providerId, [FromBody] UpdateProviderDto updateDto)
        {
            return await _userService.UpdateProviderAsync(providerId, updateDto, GetToken(HttpContext));
        }

        /// <summary>
        /// Delete provider
        /// </summary>
        [HttpDelete("providers/{providerId}")]
        //[Authorize]
        public async Task<JsonModel> DeleteProvider(int providerId)
        {
            return await _userService.DeleteProviderAsync(providerId, GetToken(HttpContext));
        }

        /// <summary>
        /// Verify provider
        /// </summary>
        [HttpPut("providers/{providerId}/verify")]
        //[Authorize]
        public async Task<JsonModel> VerifyProvider(int providerId)
        {
            return await _userService.VerifyProviderAsync(providerId, GetToken(HttpContext));
        }

        /// <summary>
        /// Get provider schedule
        /// </summary>
        [HttpGet("providers/{providerId}/schedule")]
        //[Authorize]
        public async Task<JsonModel> GetProviderSchedule(int providerId)
        {
            return await _userService.GetProviderScheduleAsync(providerId, GetToken(HttpContext));
        }

        /// <summary>
        /// Update provider schedule
        /// </summary>
        [HttpPut("providers/{providerId}/schedule")]
        //[Authorize]
        public async Task<JsonModel> UpdateProviderSchedule(int providerId, [FromBody] UpdateProviderScheduleDto scheduleDto)
        {
            return await _userService.UpdateProviderScheduleAsync(providerId, scheduleDto, GetToken(HttpContext));
        }

        /// <summary>
        /// Get provider reviews
        /// </summary>
        [HttpGet("providers/{providerId}/reviews")]
        //[Authorize]
        public async Task<JsonModel> GetProviderReviews(int providerId)
        {
            return await _userService.GetProviderReviewsAsync(providerId, GetToken(HttpContext));
        }

        /// <summary>
        /// Add provider review
        /// </summary>
        [HttpPost("providers/{providerId}/reviews")]
        //[Authorize]
        public async Task<JsonModel> AddProviderReview(int providerId, [FromBody] AddReviewDto reviewDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            {
                return new JsonModel 
                { 
                    data = new object(), 
                    Message = "User not authenticated",
                    StatusCode = 401
                };
            }

            return await _userService.AddProviderReviewAsync(userIdInt, reviewDto, GetToken(HttpContext));
        }

        // === PATIENT OPERATIONS ===

        /// <summary>
        /// Get patient by ID
        /// </summary>
        [HttpGet("patients/{patientId}")]
        //[Authorize]
        public async Task<JsonModel> GetPatient(int patientId)
        {
            return await _userService.GetPatientByIdAsync(patientId, GetToken(HttpContext));
        }

        /// <summary>
        /// Get all patients
        /// </summary>
        [HttpGet("patients")]
        //[Authorize]
        public async Task<JsonModel> GetAllPatients()
        {
            return await _userService.GetAllPatientsAsync(GetToken(HttpContext));
        }

        /// <summary>
        /// Get patient medical history
        /// </summary>
        [HttpGet("patients/{patientId}/medical-history")]
        //[Authorize]
        public async Task<JsonModel> GetPatientMedicalHistory(int patientId)
        {
            return await _userService.GetPatientMedicalHistoryAsync(patientId, GetToken(HttpContext));
        }

        /// <summary>
        /// Update patient medical history
        /// </summary>
        [HttpPut("patients/{patientId}/medical-history")]
        //[Authorize]
        public async Task<JsonModel> UpdatePatientMedicalHistory(int patientId, [FromBody] UpdateMedicalHistoryDto medicalHistoryDto)
        {
            return await _userService.UpdatePatientMedicalHistoryAsync(patientId, medicalHistoryDto, GetToken(HttpContext));
        }
    }
} 