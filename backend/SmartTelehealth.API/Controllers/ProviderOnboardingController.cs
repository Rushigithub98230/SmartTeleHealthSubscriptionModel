using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProviderOnboardingController : BaseController
{
    private readonly IProviderOnboardingService _onboardingService;

    public ProviderOnboardingController(
        IProviderOnboardingService onboardingService)
    {
        _onboardingService = onboardingService;
    }

    /// <summary>
    /// Create a new provider onboarding application
    /// </summary>
    [HttpPost]
    public async Task<JsonModel> CreateOnboarding([FromBody] CreateProviderOnboardingDto createDto)
    {
        return await _onboardingService.CreateOnboardingAsync(createDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Get onboarding by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<JsonModel> GetOnboarding(Guid id)
    {
        return await _onboardingService.GetOnboardingAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Get onboarding by user ID
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<JsonModel> GetOnboardingByUser(int userId)
    {
        return await _onboardingService.GetOnboardingByUserIdAsync(userId, GetToken(HttpContext));
    }

    /// <summary>
    /// Update onboarding application
    /// </summary>
    [HttpPut("{id}")]
    public async Task<JsonModel> UpdateOnboarding(Guid id, [FromBody] UpdateProviderOnboardingDto updateDto)
    {
        return await _onboardingService.UpdateOnboardingAsync(id, updateDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Submit onboarding application for review
    /// </summary>
    [HttpPost("{id}/submit")]
    public async Task<JsonModel> SubmitOnboarding(Guid id)
    {
        return await _onboardingService.SubmitOnboardingAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Review onboarding application (Admin only)
    /// </summary>
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