using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using System.Security.Claims;

namespace SmartTelehealth.API.Controllers;

/// <summary>
/// Controller responsible for comprehensive health assessment management and processing.
/// This controller provides extensive functionality for creating, managing, and processing
/// health assessments including assessment templates, provider reviews, assessment reports,
/// and assessment lifecycle management. It handles the complete health assessment workflow
/// from creation to completion and reporting.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthAssessmentsController : BaseController
{
    private readonly IHealthAssessmentService _healthAssessmentService;

    /// <summary>
    /// Initializes a new instance of the HealthAssessmentsController with the required health assessment service.
    /// </summary>
    /// <param name="healthAssessmentService">Service for handling health assessment-related business logic</param>
    public HealthAssessmentsController(IHealthAssessmentService healthAssessmentService)
    {
        _healthAssessmentService = healthAssessmentService;
    }

    /// <summary>
    /// Creates a new health assessment for a user.
    /// This endpoint handles health assessment creation including validation, template selection,
    /// and initial assessment setup for provider review and processing.
    /// </summary>
    /// <param name="createDto">DTO containing health assessment creation details</param>
    /// <returns>JsonModel containing the created assessment information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Creates a new health assessment with validation
    /// - Validates assessment template and user eligibility
    /// - Sets up assessment for provider review
    /// - Access restricted to authenticated users
    /// - Used for health assessment creation and management
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on assessment creation
    /// - Maintains assessment audit trails and creation history
    /// </remarks>
    [HttpPost]
    public async Task<JsonModel> CreateAssessment([FromBody] CreateHealthAssessmentDto createDto)
    {
        return await _healthAssessmentService.CreateAssessmentAsync(createDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Retrieves all health assessments for a specific user.
    /// This endpoint provides a comprehensive list of health assessments associated with a user,
    /// including assessment status, completion details, and assessment history.
    /// </summary>
    /// <param name="userId">The unique identifier of the user</param>
    /// <returns>JsonModel containing the user's health assessments</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns all health assessments for the specified user
    /// - Includes assessment status, completion details, and history
    /// - Shows assessment results and provider feedback
    /// - Access restricted to assessment owners and authorized users
    /// - Used for user assessment history and management
    /// - Includes comprehensive assessment information and metadata
    /// - Provides secure access to user assessment data
    /// - Handles authorization validation and error responses
    /// </remarks>
    [HttpGet("user/{userId}")]
    public async Task<JsonModel> GetUserAssessments(int userId)
    {
        return await _healthAssessmentService.GetUserAssessmentsAsync(userId, GetToken(HttpContext));
    }

    /// <summary>
    /// Retrieves pending health assessments assigned to a specific provider.
    /// This endpoint provides a list of health assessments that are awaiting review
    /// by the specified provider, including assessment details and priority information.
    /// </summary>
    /// <param name="providerId">The unique identifier of the provider</param>
    /// <returns>JsonModel containing the provider's pending assessments</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns pending health assessments for the specified provider
    /// - Includes assessment details and priority information
    /// - Shows assessment status and review requirements
    /// - Access restricted to providers and administrators
    /// - Used for provider assessment queue and management
    /// - Includes comprehensive assessment information and metadata
    /// - Provides secure access to provider assessment data
    /// - Handles authorization validation and error responses
    /// </remarks>
    [HttpGet("provider/{providerId}/pending")]
    
    public async Task<JsonModel> GetProviderPendingAssessments(int providerId)
    {
        return await _healthAssessmentService.GetProviderPendingAssessmentsAsync(providerId, GetToken(HttpContext));
    }

    /// <summary>
    /// Retrieves reviewed health assessments completed by a specific provider.
    /// This endpoint provides a list of health assessments that have been reviewed
    /// and completed by the specified provider, including review details and outcomes.
    /// </summary>
    /// <param name="providerId">The unique identifier of the provider</param>
    /// <returns>JsonModel containing the provider's reviewed assessments</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns reviewed health assessments for the specified provider
    /// - Includes review details, outcomes, and completion information
    /// - Shows assessment results and provider feedback
    /// - Access restricted to providers and administrators
    /// - Used for provider assessment history and management
    /// - Includes comprehensive assessment information and metadata
    /// - Provides secure access to provider assessment data
    /// - Handles authorization validation and error responses
    /// </remarks>
    [HttpGet("provider/{providerId}/reviewed")]
    //[Authorize]
    public async Task<JsonModel> GetProviderReviewedAssessments(int providerId)
    {
        return await _healthAssessmentService.GetProviderReviewedAssessmentsAsync(providerId, GetToken(HttpContext));
    }

    /// <summary>
    /// Retrieves all pending health assessments in the system (Provider only).
    /// This endpoint provides a comprehensive list of all health assessments that are
    /// awaiting provider review, including assessment details and assignment information.
    /// </summary>
    /// <returns>JsonModel containing all pending health assessments</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns all pending health assessments in the system
    /// - Includes assessment details and assignment information
    /// - Shows assessment priority and review requirements
    /// - Access restricted to providers and administrators only
    /// - Used for system-wide assessment queue management
    /// - Includes comprehensive assessment information and metadata
    /// - Provides secure access to pending assessment data
    /// - Handles authorization validation and error responses
    /// </remarks>
    [HttpGet("pending")]
    
    public async Task<JsonModel> GetPendingAssessments()
    {
        return await _healthAssessmentService.GetPendingAssessmentsAsync(GetToken(HttpContext));
    }

    /// <summary>
    /// Retrieves detailed information about a specific health assessment by its ID.
    /// This endpoint provides comprehensive assessment details including assessment content,
    /// provider reviews, assessment status, and related information for authorized users.
    /// </summary>
    /// <param name="assessmentId">The unique identifier of the health assessment</param>
    /// <returns>JsonModel containing the assessment details</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns detailed health assessment information by ID
    /// - Includes assessment content, reviews, and status information
    /// - Shows assessment results and provider feedback
    /// - Access restricted to assessment participants and authorized users
    /// - Used for assessment details and management
    /// - Includes comprehensive assessment information and metadata
    /// - Provides secure access to assessment information
    /// - Handles authorization validation and error responses
    /// </remarks>
    [HttpGet("{assessmentId}")]
    public async Task<JsonModel> GetAssessment(Guid assessmentId)
    {
        return await _healthAssessmentService.GetAssessmentByIdAsync(assessmentId, GetToken(HttpContext));
    }

    /// <summary>
    /// Updates an existing health assessment with new information.
    /// This endpoint allows authorized users to modify assessment details,
    /// content, and settings while maintaining data integrity and validation.
    /// </summary>
    /// <param name="assessmentId">The unique identifier of the assessment to update</param>
    /// <param name="updateDto">DTO containing the updated assessment information</param>
    /// <returns>JsonModel containing the update result</returns>
    /// <remarks>
    /// This endpoint:
    /// - Updates health assessment information with validation
    /// - Ensures data integrity and consistency
    /// - Validates assessment content and business rules
    /// - Access restricted to assessment owners and authorized users
    /// - Used for assessment editing and management
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on update operations
    /// - Maintains assessment audit trails and change history
    /// </remarks>
    [HttpPut("{assessmentId}")]
    public async Task<JsonModel> UpdateAssessment(Guid assessmentId, [FromBody] UpdateHealthAssessmentDto updateDto)
    {
        return await _healthAssessmentService.UpdateAssessmentAsync(assessmentId, updateDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Deletes a health assessment from the system.
    /// This endpoint handles assessment deletion including cleanup of related data,
    /// validation of assessment status, and assessment history management.
    /// </summary>
    /// <param name="assessmentId">The unique identifier of the assessment to delete</param>
    /// <returns>JsonModel containing the deletion result</returns>
    /// <remarks>
    /// This endpoint:
    /// - Deletes health assessment with cleanup of related data
    /// - Validates assessment status and deletion eligibility
    /// - Handles assessment cleanup and data integrity
    /// - Access restricted to assessment owners and administrators
    /// - Used for assessment management and cleanup
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on deletion operations
    /// - Maintains assessment audit trails and deletion history
    /// </remarks>
    [HttpDelete("{assessmentId}")]
    public async Task<JsonModel> DeleteAssessment(Guid assessmentId)
    {
        return await _healthAssessmentService.DeleteAssessmentAsync(assessmentId, GetToken(HttpContext));
    }

    /// <summary>
    /// Reviews a health assessment and provides provider feedback (Provider only).
    /// This endpoint allows providers to review health assessments, provide feedback,
    /// and determine eligibility with detailed review comments and recommendations.
    /// </summary>
    /// <param name="assessmentId">The unique identifier of the assessment to review</param>
    /// <param name="reviewDto">DTO containing review details and provider feedback</param>
    /// <returns>JsonModel containing the review result</returns>
    /// <remarks>
    /// This endpoint:
    /// - Reviews health assessment with provider feedback
    /// - Determines assessment eligibility and recommendations
    /// - Updates assessment status and review information
    /// - Access restricted to providers only
    /// - Used for assessment review and provider feedback
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on review operations
    /// - Maintains assessment audit trails and review history
    /// </remarks>
    [HttpPost("{assessmentId}/review")]
    //[Authorize]
    public async Task<JsonModel> ReviewAssessment(Guid assessmentId, [FromBody] ReviewAssessmentDto reviewDto)
    {
        var userId = GetCurrentUserId();
        return await _healthAssessmentService.ReviewAssessmentAsync(assessmentId, userId, reviewDto.IsEligible, reviewDto.Notes, GetToken(HttpContext));
    }

    /// <summary>
    /// Completes a health assessment and finalizes the assessment process.
    /// This endpoint marks the assessment as completed, updates the assessment status,
    /// and triggers any necessary follow-up actions or notifications.
    /// </summary>
    /// <param name="assessmentId">The unique identifier of the assessment to complete</param>
    /// <returns>JsonModel containing the completion result</returns>
    /// <remarks>
    /// This endpoint:
    /// - Completes health assessment and finalizes process
    /// - Updates assessment status and completion information
    /// - Triggers follow-up actions and notifications
    /// - Access restricted to assessment participants and authorized users
    /// - Used for assessment completion and finalization
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on completion operations
    /// - Maintains assessment audit trails and completion history
    /// </remarks>
    [HttpPost("{assessmentId}/complete")]
    public async Task<JsonModel> CompleteAssessment(Guid assessmentId)
    {
        return await _healthAssessmentService.CompleteAssessmentAsync(assessmentId, GetToken(HttpContext));
    }

    /// <summary>
    /// Cancels a health assessment with a specified reason.
    /// This endpoint handles assessment cancellation including status updates,
    /// reason tracking, and notification of relevant parties.
    /// </summary>
    /// <param name="assessmentId">The unique identifier of the assessment to cancel</param>
    /// <param name="cancelDto">DTO containing cancellation reason and details</param>
    /// <returns>JsonModel containing the cancellation result</returns>
    /// <remarks>
    /// This endpoint:
    /// - Cancels health assessment with reason tracking
    /// - Updates assessment status and cancellation information
    /// - Notifies relevant parties of cancellation
    /// - Access restricted to assessment participants and authorized users
    /// - Used for assessment cancellation and management
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on cancellation operations
    /// - Maintains assessment audit trails and cancellation history
    /// </remarks>
    [HttpPost("{assessmentId}/cancel")]
    public async Task<JsonModel> CancelAssessment(Guid assessmentId, [FromBody] CancelAssessmentDto cancelDto)
    {
        return await _healthAssessmentService.CancelAssessmentAsync(assessmentId, cancelDto.Reason, GetToken(HttpContext));
    }

    /// <summary>
    /// Create assessment template (Admin only)
    /// </summary>
    [HttpPost("templates")]
    
    public async Task<JsonModel> CreateAssessmentTemplate([FromBody] CreateAssessmentTemplateDto createDto)
    {
        return await _healthAssessmentService.CreateAssessmentTemplateAsync(createDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Get assessment template by ID
    /// </summary>
    [HttpGet("templates/{id}")]
    public async Task<JsonModel> GetAssessmentTemplate(Guid id)
    {
        return await _healthAssessmentService.GetAssessmentTemplateAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Get assessment templates by category
    /// </summary>
    [HttpGet("templates/category/{categoryId}")]
    public async Task<JsonModel> GetAssessmentTemplatesByCategory(Guid categoryId)
    {
        return await _healthAssessmentService.GetAssessmentTemplatesByCategoryAsync(categoryId, GetToken(HttpContext));
    }

    /// <summary>
    /// Update assessment template (Admin only)
    /// </summary>
    [HttpPut("templates/{id}")]
    
    public async Task<JsonModel> UpdateAssessmentTemplate(Guid id, [FromBody] UpdateAssessmentTemplateDto updateDto)
    {
        return await _healthAssessmentService.UpdateAssessmentTemplateAsync(id, updateDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Delete assessment template (Admin only)
    /// </summary>
    [HttpDelete("templates/{id}")]
    
    public async Task<JsonModel> DeleteAssessmentTemplate(Guid id)
    {
        return await _healthAssessmentService.DeleteAssessmentTemplateAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Generate assessment report
    /// </summary>
    [HttpGet("{assessmentId}/report")]
    public async Task<JsonModel> GenerateAssessmentReport(Guid assessmentId)
    {
        return await _healthAssessmentService.GenerateAssessmentReportAsync(assessmentId, GetToken(HttpContext));
    }

    /// <summary>
    /// Export assessment report
    /// </summary>
    [HttpGet("{assessmentId}/export")]
    public async Task<JsonModel> ExportAssessmentReport(Guid assessmentId, [FromQuery] string format = "pdf")
    {
        return await _healthAssessmentService.ExportAssessmentReportAsync(assessmentId, format, GetToken(HttpContext));
    }

    /// <summary>
    /// Get assessment reports for user
    /// </summary>
    [HttpGet("reports/{userId}")]
    public async Task<JsonModel> GetAssessmentReports(int userId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        return await _healthAssessmentService.GetAssessmentReportsAsync(userId, startDate, endDate, GetToken(HttpContext));
    }

    /// <summary>
    /// Assign assessment to provider (Admin only)
    /// </summary>
    [HttpPost("{assessmentId}/assign")]
    
    public async Task<JsonModel> AssignAssessmentToProvider(Guid assessmentId, [FromBody] AssignAssessmentDto assignDto)
    {
        return await _healthAssessmentService.AssignAssessmentToProviderAsync(assessmentId, assignDto.ProviderId, GetToken(HttpContext));
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
}

// Supporting DTOs
public class ReviewAssessmentDto
{
    public int ProviderId { get; set; }
    public bool IsEligible { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class CancelAssessmentDto
{
    public string Reason { get; set; } = string.Empty;
}

public class AssignAssessmentDto
{
    public int ProviderId { get; set; }
} 