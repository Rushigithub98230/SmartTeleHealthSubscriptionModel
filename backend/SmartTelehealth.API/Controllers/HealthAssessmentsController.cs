using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using System.Security.Claims;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthAssessmentsController : BaseController
{
    private readonly IHealthAssessmentService _healthAssessmentService;

    public HealthAssessmentsController(IHealthAssessmentService healthAssessmentService)
    {
        _healthAssessmentService = healthAssessmentService;
    }

    /// <summary>
    /// Create a new health assessment
    /// </summary>
    [HttpPost]
    public async Task<JsonModel> CreateAssessment([FromBody] CreateHealthAssessmentDto createDto)
    {
        return await _healthAssessmentService.CreateAssessmentAsync(createDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Get user's health assessments
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<JsonModel> GetUserAssessments(int userId)
    {
        return await _healthAssessmentService.GetUserAssessmentsAsync(userId, GetToken(HttpContext));
    }

    /// <summary>
    /// Get provider's pending assessments
    /// </summary>
    [HttpGet("provider/{providerId}/pending")]
    
    public async Task<JsonModel> GetProviderPendingAssessments(int providerId)
    {
        return await _healthAssessmentService.GetProviderPendingAssessmentsAsync(providerId, GetToken(HttpContext));
    }

    /// <summary>
    /// Get provider's reviewed assessments
    /// </summary>
    [HttpGet("provider/{providerId}/reviewed")]
    [Authorize]
    public async Task<JsonModel> GetProviderReviewedAssessments(int providerId)
    {
        return await _healthAssessmentService.GetProviderReviewedAssessmentsAsync(providerId, GetToken(HttpContext));
    }

    /// <summary>
    /// Get pending health assessments (Provider only)
    /// </summary>
    [HttpGet("pending")]
    
    public async Task<JsonModel> GetPendingAssessments()
    {
        return await _healthAssessmentService.GetPendingAssessmentsAsync(GetToken(HttpContext));
    }

    /// <summary>
    /// Get health assessment by ID
    /// </summary>
    [HttpGet("{assessmentId}")]
    public async Task<JsonModel> GetAssessment(Guid assessmentId)
    {
        return await _healthAssessmentService.GetAssessmentByIdAsync(assessmentId, GetToken(HttpContext));
    }

    /// <summary>
    /// Update health assessment
    /// </summary>
    [HttpPut("{assessmentId}")]
    public async Task<JsonModel> UpdateAssessment(Guid assessmentId, [FromBody] UpdateHealthAssessmentDto updateDto)
    {
        return await _healthAssessmentService.UpdateAssessmentAsync(assessmentId, updateDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Delete health assessment
    /// </summary>
    [HttpDelete("{assessmentId}")]
    public async Task<JsonModel> DeleteAssessment(Guid assessmentId)
    {
        return await _healthAssessmentService.DeleteAssessmentAsync(assessmentId, GetToken(HttpContext));
    }

    /// <summary>
    /// Review health assessment (Provider only)
    /// </summary>
    [HttpPost("{assessmentId}/review")]
    [Authorize]
    public async Task<JsonModel> ReviewAssessment(Guid assessmentId, [FromBody] ReviewAssessmentDto reviewDto)
    {
        var userId = GetCurrentUserId();
        return await _healthAssessmentService.ReviewAssessmentAsync(assessmentId, userId, reviewDto.IsEligible, reviewDto.Notes, GetToken(HttpContext));
    }

    /// <summary>
    /// Complete health assessment
    /// </summary>
    [HttpPost("{assessmentId}/complete")]
    public async Task<JsonModel> CompleteAssessment(Guid assessmentId)
    {
        return await _healthAssessmentService.CompleteAssessmentAsync(assessmentId, GetToken(HttpContext));
    }

    /// <summary>
    /// Cancel health assessment
    /// </summary>
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