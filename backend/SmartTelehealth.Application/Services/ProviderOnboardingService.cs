using AutoMapper;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Application.Services;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace SmartTelehealth.Application.Services;

public class ProviderOnboardingService : IProviderOnboardingService
{
    private readonly IProviderOnboardingRepository _onboardingRepository;
    private readonly IUserService _userService;
    private readonly IAuditService _auditService;
    private readonly INotificationService _notificationService;
    private readonly IMapper _mapper;
    private readonly ILogger<ProviderOnboardingService> _logger;

    public ProviderOnboardingService(
        IProviderOnboardingRepository onboardingRepository,
        IUserService userService,
        IAuditService auditService,
        INotificationService notificationService,
        IMapper mapper,
        ILogger<ProviderOnboardingService> logger)
    {
        _onboardingRepository = onboardingRepository;
        _userService = userService;
        _auditService = auditService;
        _notificationService = notificationService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<JsonModel> CreateOnboardingAsync(CreateProviderOnboardingDto createDto, TokenModel tokenModel)
    {
        try
        {
            // TODO: Check if user already has an onboarding application by email
            // This will be implemented when we have proper user management

            var onboarding = new ProviderOnboarding
            {
                Id = Guid.NewGuid(),
                UserId = 0, // TODO: This should be set to the actual user ID after user creation
                FirstName = createDto.FirstName,
                LastName = createDto.LastName,
                Email = createDto.Email,
                PhoneNumber = createDto.PhoneNumber,
                Specialty = createDto.Specialty,
                SubSpecialty = createDto.SubSpecialty,
                LicenseNumber = createDto.LicenseNumber,
                LicenseState = createDto.LicenseState,
                NPINumber = createDto.NPINumber,
                DEANumber = createDto.DEANumber,
                Education = createDto.Education,
                WorkHistory = createDto.WorkHistory,
                MalpracticeInsurance = createDto.MalpracticeInsurance,
                Bio = createDto.Bio,
                ProfilePhotoUrl = createDto.ProfilePhotoUrl,
                GovernmentIdUrl = createDto.GovernmentIdUrl,
                LicenseDocumentUrl = createDto.LicenseDocumentUrl,
                CertificationDocumentUrl = createDto.CertificationDocumentUrl,
                MalpracticeInsuranceUrl = createDto.MalpracticeInsuranceUrl,
                Status = OnboardingStatus.Pending,
                // Set audit properties for creation
                IsActive = true,
                CreatedBy = tokenModel.UserID,
                CreatedDate = DateTime.UtcNow
            };

            var savedOnboarding = await _onboardingRepository.AddAsync(onboarding);
            var dto = _mapper.Map<ProviderOnboardingDto>(savedOnboarding);

            // TODO: Implement audit logging
            // await _auditService.LogActionAsync("ProviderOnboarding", "Create", savedOnboarding.Id.ToString(), "Onboarding application created");

            return new JsonModel
            {
                data = dto,
                Message = "Onboarding application created successfully",
                StatusCode = 201
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating onboarding application");
            return new JsonModel
            {
                data = new object(),
                Message = "Error creating onboarding application",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetOnboardingAsync(Guid id, TokenModel tokenModel)
    {
        try
        {
            var onboarding = await _onboardingRepository.GetByIdAsync(id);
            if (onboarding == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Onboarding application not found",
                    StatusCode = 404
                };
            }

            var dto = _mapper.Map<ProviderOnboardingDto>(onboarding);
            return new JsonModel
            {
                data = dto,
                Message = "Onboarding application retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving onboarding application");
            return new JsonModel
            {
                data = new object(),
                Message = "Error retrieving onboarding application",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetOnboardingByUserIdAsync(int userId, TokenModel tokenModel)
    {
        try
        {
            var onboarding = await _onboardingRepository.GetByUserIdAsync(userId);
            if (onboarding == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Onboarding application not found",
                    StatusCode = 404
                };
            }

            var dto = _mapper.Map<ProviderOnboardingDto>(onboarding);
            return new JsonModel
            {
                data = dto,
                Message = "Onboarding application retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving onboarding application by user ID");
            return new JsonModel
            {
                data = new object(),
                Message = "Error retrieving onboarding application",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> UpdateOnboardingAsync(Guid id, UpdateProviderOnboardingDto updateDto, TokenModel tokenModel)
    {
        try
        {
            var onboarding = await _onboardingRepository.GetByIdAsync(id);
            if (onboarding == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Onboarding application not found",
                    StatusCode = 404
                };
            }

            // Update fields if provided
            if (!string.IsNullOrEmpty(updateDto.FirstName))
                onboarding.FirstName = updateDto.FirstName;
            if (!string.IsNullOrEmpty(updateDto.LastName))
                onboarding.LastName = updateDto.LastName;
            if (!string.IsNullOrEmpty(updateDto.PhoneNumber))
                onboarding.PhoneNumber = updateDto.PhoneNumber;
            if (!string.IsNullOrEmpty(updateDto.Specialty))
                onboarding.Specialty = updateDto.Specialty;
            if (!string.IsNullOrEmpty(updateDto.SubSpecialty))
                onboarding.SubSpecialty = updateDto.SubSpecialty;
            if (!string.IsNullOrEmpty(updateDto.LicenseNumber))
                onboarding.LicenseNumber = updateDto.LicenseNumber;
            if (!string.IsNullOrEmpty(updateDto.LicenseState))
                onboarding.LicenseState = updateDto.LicenseState;
            if (!string.IsNullOrEmpty(updateDto.NPINumber))
                onboarding.NPINumber = updateDto.NPINumber;
            if (!string.IsNullOrEmpty(updateDto.DEANumber))
                onboarding.DEANumber = updateDto.DEANumber;
            if (!string.IsNullOrEmpty(updateDto.Education))
                onboarding.Education = updateDto.Education;
            if (!string.IsNullOrEmpty(updateDto.WorkHistory))
                onboarding.WorkHistory = updateDto.WorkHistory;
            if (!string.IsNullOrEmpty(updateDto.MalpracticeInsurance))
                onboarding.MalpracticeInsurance = updateDto.MalpracticeInsurance;
            if (!string.IsNullOrEmpty(updateDto.Bio))
                onboarding.Bio = updateDto.Bio;
            if (!string.IsNullOrEmpty(updateDto.ProfilePhotoUrl))
                onboarding.ProfilePhotoUrl = updateDto.ProfilePhotoUrl;
            if (!string.IsNullOrEmpty(updateDto.GovernmentIdUrl))
                onboarding.GovernmentIdUrl = updateDto.GovernmentIdUrl;
            if (!string.IsNullOrEmpty(updateDto.LicenseDocumentUrl))
                onboarding.LicenseDocumentUrl = updateDto.LicenseDocumentUrl;
            if (!string.IsNullOrEmpty(updateDto.CertificationDocumentUrl))
                onboarding.CertificationDocumentUrl = updateDto.CertificationDocumentUrl;
            if (!string.IsNullOrEmpty(updateDto.MalpracticeInsuranceUrl))
                onboarding.MalpracticeInsuranceUrl = updateDto.MalpracticeInsuranceUrl;

            onboarding.UpdatedBy = tokenModel.UserID;
            onboarding.UpdatedDate = DateTime.UtcNow;

            var updatedOnboarding = await _onboardingRepository.UpdateAsync(onboarding);
            var dto = _mapper.Map<ProviderOnboardingDto>(updatedOnboarding);

            // TODO: Implement audit logging
            // await _auditService.LogActionAsync("ProviderOnboarding", "Update", id.ToString(), "Onboarding application updated");

            return new JsonModel
            {
                data = dto,
                Message = "Onboarding application updated successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating onboarding application");
            return new JsonModel
            {
                data = new object(),
                Message = "Error updating onboarding application",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> SubmitOnboardingAsync(Guid id, TokenModel tokenModel)
    {
        try
        {
            var onboarding = await _onboardingRepository.GetByIdAsync(id);
            if (onboarding == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Onboarding application not found",
                    StatusCode = 404
                };
            }

            if (onboarding.Status != OnboardingStatus.Pending)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Onboarding application cannot be submitted in current status",
                    StatusCode = 400
                };
            }

            onboarding.Status = OnboardingStatus.UnderReview;
            onboarding.SubmittedAt = DateTime.UtcNow;
            onboarding.UpdatedBy = tokenModel.UserID;
            onboarding.UpdatedDate = DateTime.UtcNow;

            var updatedOnboarding = await _onboardingRepository.UpdateAsync(onboarding);
            var dto = _mapper.Map<ProviderOnboardingDto>(updatedOnboarding);

            // Log audit
            // TODO: Implement audit logging
            // await _auditService.LogActionAsync("ProviderOnboarding", "Submit", id.ToString(), "Onboarding application submitted for review");

            // TODO: Implement notification service
            // await _notificationService.SendNotificationAsync(
            //     "New Onboarding Application",
            //     $"New provider onboarding application submitted by {onboarding.FirstName} {onboarding.LastName}",
            //     "Admin"
            // );

            return new JsonModel
            {
                data = dto,
                Message = "Onboarding application submitted successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting onboarding application");
            return new JsonModel
            {
                data = new object(),
                Message = "Error submitting onboarding application",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> ReviewOnboardingAsync(Guid id, ReviewProviderOnboardingDto reviewDto, TokenModel tokenModel)
    {
        try
        {
            var onboarding = await _onboardingRepository.GetByIdAsync(id);
            if (onboarding == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Onboarding application not found",
                    StatusCode = 404
                };
            }

            if (onboarding.Status != OnboardingStatus.UnderReview && onboarding.Status != OnboardingStatus.RequiresMoreInfo)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Onboarding application cannot be reviewed in current status",
                    StatusCode = 400
                };
            }

            // Update status based on review
            onboarding.Status = Enum.Parse<OnboardingStatus>(reviewDto.Status);
            onboarding.AdminRemarks = reviewDto.AdminRemarks;
            onboarding.ReviewedAt = DateTime.UtcNow;
            onboarding.UpdatedBy = tokenModel.UserID;
            onboarding.UpdatedDate = DateTime.UtcNow;

            var updatedOnboarding = await _onboardingRepository.UpdateAsync(onboarding);
            var dto = _mapper.Map<ProviderOnboardingDto>(updatedOnboarding);

            // Log audit
            // TODO: Implement audit logging
            // await _auditService.LogActionAsync("ProviderOnboarding", "Review", id.ToString(), $"Onboarding application reviewed: {reviewDto.Status}");

            // Send notification to provider
            var notificationMessage = reviewDto.Status == "Approved" 
                ? "Your onboarding application has been approved! You can now access the provider portal."
                : $"Your onboarding application has been {reviewDto.Status.ToLower()}. {reviewDto.AdminRemarks}";

            // TODO: Implement notification service
            // await _notificationService.SendNotificationAsync(
            //     "Onboarding Application Update",
            //     notificationMessage,
            //     onboarding.UserId.ToString()
            // );

            return new JsonModel
            {
                data = dto,
                Message = "Onboarding application reviewed successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reviewing onboarding application");
            return new JsonModel
            {
                data = new object(),
                Message = "Error reviewing onboarding application",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetAllOnboardingsAsync(string? status, int page, int pageSize, TokenModel tokenModel)
    {
        try
        {
            IEnumerable<ProviderOnboarding> onboardings;
            
            if (!string.IsNullOrEmpty(status))
            {
                onboardings = await _onboardingRepository.GetByStatusWithPaginationAsync(status, page, pageSize);
            }
            else
            {
                onboardings = await _onboardingRepository.GetAllAsync();
            }

            var dtos = _mapper.Map<IEnumerable<ProviderOnboardingDto>>(onboardings);
            return new JsonModel
            {
                data = dtos,
                Message = "Onboarding applications retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving onboarding applications");
            return new JsonModel
            {
                data = new object(),
                Message = "Error retrieving onboarding applications",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetPendingOnboardingsAsync(TokenModel tokenModel)
    {
        try
        {
            var onboardings = await _onboardingRepository.GetPendingAsync();
            var dtos = _mapper.Map<IEnumerable<ProviderOnboardingDto>>(onboardings);
            return new JsonModel
            {
                data = dtos,
                Message = "Pending onboarding applications retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending onboarding applications");
            return new JsonModel
            {
                data = new object(),
                Message = "Error retrieving pending onboarding applications",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetOnboardingsByStatusAsync(string status, TokenModel tokenModel)
    {
        try
        {
            var onboardings = await _onboardingRepository.GetByStatusAsync(status);
            var dtos = _mapper.Map<IEnumerable<ProviderOnboardingDto>>(onboardings);
            return new JsonModel
            {
                data = dtos,
                Message = "Onboarding applications by status retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving onboarding applications by status");
            return new JsonModel
            {
                data = new object(),
                Message = "Error retrieving onboarding applications by status",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> DeleteOnboardingAsync(Guid id, TokenModel tokenModel)
    {
        try
        {
            var onboarding = await _onboardingRepository.GetByIdAsync(id);
            if (onboarding == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Onboarding application not found",
                    StatusCode = 404
                };
            }

            // Soft delete - set audit properties
            onboarding.IsDeleted = true;
            onboarding.DeletedBy = tokenModel.UserID;
            onboarding.DeletedDate = DateTime.UtcNow;
            onboarding.UpdatedBy = tokenModel.UserID;
            onboarding.UpdatedDate = DateTime.UtcNow;
            
            var result = await _onboardingRepository.UpdateAsync(onboarding);
            if (result == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Onboarding application not found",
                    StatusCode = 404
                };
            }

            // Log audit
            // TODO: Implement audit logging
            // await _auditService.LogActionAsync("ProviderOnboarding", "Delete", id.ToString(), "Onboarding application deleted");

            return new JsonModel
            {
                data = true,
                Message = "Onboarding application deleted successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting onboarding application");
            return new JsonModel
            {
                data = new object(),
                Message = "Error deleting onboarding application",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetOnboardingStatisticsAsync(TokenModel tokenModel)
    {
        try
        {
            var totalOnboardings = await _onboardingRepository.GetTotalCountAsync();
            var pendingOnboardings = await _onboardingRepository.GetCountByStatusAsync("Pending");
            var underReviewOnboardings = await _onboardingRepository.GetCountByStatusAsync("UnderReview");
            var approvedOnboardings = await _onboardingRepository.GetCountByStatusAsync("Approved");
            var rejectedOnboardings = await _onboardingRepository.GetCountByStatusAsync("Rejected");
            var requiresMoreInfoOnboardings = await _onboardingRepository.GetCountByStatusAsync("RequiresMoreInfo");

            var approvalRate = totalOnboardings > 0 
                ? (decimal)(approvedOnboardings + rejectedOnboardings) > 0 
                    ? (decimal)approvedOnboardings / (decimal)(approvedOnboardings + rejectedOnboardings) * 100 
                    : 0 
                : 0;

            var statistics = new OnboardingStatisticsDto
            {
                TotalOnboardings = totalOnboardings,
                PendingOnboardings = pendingOnboardings,
                UnderReviewOnboardings = underReviewOnboardings,
                ApprovedOnboardings = approvedOnboardings,
                RejectedOnboardings = rejectedOnboardings,
                RequiresMoreInfoOnboardings = requiresMoreInfoOnboardings,
                ApprovalRate = approvalRate,
                AverageProcessingTimeDays = 3 // TODO: Calculate actual average
            };

            return new JsonModel
            {
                data = statistics,
                Message = "Onboarding statistics retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving onboarding statistics");
            return new JsonModel
            {
                data = new object(),
                Message = "Error retrieving onboarding statistics",
                StatusCode = 500
            };
        }
    }
} 