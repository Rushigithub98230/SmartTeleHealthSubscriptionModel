using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace SmartTelehealth.API.Controllers;

/// <summary>
/// Controller responsible for comprehensive provider onboarding management and processing.
/// This controller provides extensive functionality for managing provider onboarding applications,
/// including application creation, review processes, status management, and onboarding workflow
/// administration. It handles the complete provider onboarding lifecycle from application to approval.
/// </summary>
[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class ProviderOnboardingController : BaseController
{
    private readonly IProviderOnboardingService _onboardingService;

    /// <summary>
    /// Initializes a new instance of the ProviderOnboardingController with the required onboarding service.
    /// </summary>
    /// <param name="onboardingService">Service for handling provider onboarding-related business logic</param>
    public ProviderOnboardingController(
        IProviderOnboardingService onboardingService)
    {
        _onboardingService = onboardingService;
    }

    /// <summary>
    /// Creates a new provider onboarding application.
    /// This endpoint handles provider onboarding application creation including validation,
    /// document collection, and initial application setup for administrative review.
    /// </summary>
    /// <param name="createDto">DTO containing provider onboarding application details</param>
    /// <returns>JsonModel containing the created onboarding application information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Creates a new provider onboarding application with validation
    /// - Validates provider information and required documents
    /// - Sets up application for administrative review
    /// - Access restricted to authenticated users
    /// - Used for provider onboarding application creation
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on application creation
    /// - Maintains onboarding application audit trails and creation history
    /// </remarks>
    [HttpPost]
    public async Task<JsonModel> CreateOnboarding([FromBody] CreateProviderOnboardingDto createDto)
    {
        return await _onboardingService.CreateOnboardingAsync(createDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Retrieves detailed information about a specific provider onboarding application by its ID.
    /// This endpoint provides comprehensive onboarding application details including application status,
    /// review information, document status, and application progress for authorized users.
    /// </summary>
    /// <param name="id">The unique identifier of the onboarding application</param>
    /// <returns>JsonModel containing the onboarding application details</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns detailed onboarding application information by ID
    /// - Includes application status, review information, and document status
    /// - Shows application progress and review history
    /// - Access restricted to application owners and administrators
    /// - Used for onboarding application details and management
    /// - Includes comprehensive application information and metadata
    /// - Provides secure access to application information
    /// - Handles authorization validation and error responses
    /// </remarks>
    [HttpGet("{id}")]
    public async Task<JsonModel> GetOnboarding(Guid id)
    {
        return await _onboardingService.GetOnboardingAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Retrieves provider onboarding application information for a specific user.
    /// This endpoint provides onboarding application details associated with a user,
    /// including application status, progress, and review information.
    /// </summary>
    /// <param name="userId">The unique identifier of the user</param>
    /// <returns>JsonModel containing the user's onboarding application information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns onboarding application information for the specified user
    /// - Includes application status, progress, and review information
    /// - Shows application history and current status
    /// - Access restricted to application owners and authorized users
    /// - Used for user onboarding application management
    /// - Includes comprehensive application information and metadata
    /// - Provides secure access to user application data
    /// - Handles authorization validation and error responses
    /// </remarks>
    [HttpGet("user/{userId}")]
    public async Task<JsonModel> GetOnboardingByUser(int userId)
    {
        return await _onboardingService.GetOnboardingByUserIdAsync(userId, GetToken(HttpContext));
    }

    /// <summary>
    /// Updates an existing provider onboarding application with new information.
    /// This endpoint allows authorized users to modify onboarding application details,
    /// documents, and information while maintaining data integrity and validation.
    /// </summary>
    /// <param name="id">The unique identifier of the onboarding application to update</param>
    /// <param name="updateDto">DTO containing the updated onboarding application information</param>
    /// <returns>JsonModel containing the update result</returns>
    /// <remarks>
    /// This endpoint:
    /// - Updates onboarding application information with validation
    /// - Ensures data integrity and consistency
    /// - Validates application information and business rules
    /// - Access restricted to application owners and authorized users
    /// - Used for onboarding application editing and management
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on update operations
    /// - Maintains application audit trails and change history
    /// </remarks>
    [HttpPut("{id}")]
    public async Task<JsonModel> UpdateOnboarding(Guid id, [FromBody] UpdateProviderOnboardingDto updateDto)
    {
        return await _onboardingService.UpdateOnboardingAsync(id, updateDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Submits a provider onboarding application for administrative review.
    /// This endpoint changes the application status to "Under Review" and notifies
    /// administrators for review and approval processing.
    /// </summary>
    /// <param name="id">The unique identifier of the onboarding application to submit</param>
    /// <returns>JsonModel containing the submission result</returns>
    /// <remarks>
    /// This endpoint:
    /// - Submits onboarding application for administrative review
    /// - Changes application status to "Under Review"
    /// - Notifies administrators for review processing
    /// - Access restricted to application owners
    /// - Used for onboarding application submission and review workflow
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on submission operations
    /// - Maintains application audit trails and submission history
    /// </remarks>
    [HttpPost("{id}/submit")]
    public async Task<JsonModel> SubmitOnboarding(Guid id)
    {
        return await _onboardingService.SubmitOnboardingAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Reviews and approves or rejects a provider onboarding application (Admin only).
    /// This endpoint allows administrators to review onboarding applications, provide feedback,
    /// and approve or reject applications with detailed review comments and decisions.
    /// </summary>
    /// <param name="id">The unique identifier of the onboarding application to review</param>
    /// <param name="reviewDto">DTO containing review details and administrative decision</param>
    /// <returns>JsonModel containing the review result</returns>
    /// <remarks>
    /// This endpoint:
    /// - Reviews onboarding application with administrative decision
    /// - Approves or rejects application with feedback
    /// - Updates application status and implementation
    /// - Access restricted to administrators only
    /// - Used for onboarding application review and approval workflow
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on review operations
    /// - Maintains application audit trails and review history
    /// </remarks>
    [HttpPost("{id}/review")]
    
    public async Task<JsonModel> ReviewOnboarding(Guid id, [FromBody] ReviewProviderOnboardingDto reviewDto)
    {
        return await _onboardingService.ReviewOnboardingAsync(id, reviewDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Get all onboarding applications with optional filtering
    /// </summary>
    [HttpGet]
    
    public async Task<JsonModel> GetAllOnboardings(
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        return await _onboardingService.GetAllOnboardingsAsync(status, page, pageSize, GetToken(HttpContext));
    }

    /// <summary>
    /// Get pending onboarding applications
    /// </summary>
    [HttpGet("pending")]
    
    public async Task<JsonModel> GetPendingOnboardings()
    {
        return await _onboardingService.GetPendingOnboardingsAsync(GetToken(HttpContext));
    }

    /// <summary>
    /// Get onboarding applications by status
    /// </summary>
    [HttpGet("status/{status}")]
    
    public async Task<JsonModel> GetOnboardingsByStatus(string status)
    {
        return await _onboardingService.GetOnboardingsByStatusAsync(status, GetToken(HttpContext));
    }

    /// <summary>
    /// Delete onboarding application
    /// </summary>
    [HttpDelete("{id}")]
    
    public async Task<JsonModel> DeleteOnboarding(Guid id)
    {
        return await _onboardingService.DeleteOnboardingAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Get onboarding statistics
    /// </summary>
    [HttpGet("statistics")]
    
    public async Task<JsonModel> GetOnboardingStatistics()
    {
        return await _onboardingService.GetOnboardingStatisticsAsync(GetToken(HttpContext));
    }
} 