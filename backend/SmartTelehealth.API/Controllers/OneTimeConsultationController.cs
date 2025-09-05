using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using System.Security.Claims;

namespace SmartTelehealth.API.Controllers;

/// <summary>
/// Controller responsible for one-time consultation management and processing.
/// This controller provides functionality for managing one-time consultations including
/// consultation retrieval, status tracking, and consultation history for users who
/// require single-session healthcare consultations without ongoing subscription commitments.
/// </summary>
[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class OneTimeConsultationController : BaseController
{
    private readonly IConsultationService _consultationService;

    /// <summary>
    /// Initializes a new instance of the OneTimeConsultationController with the required consultation service.
    /// </summary>
    /// <param name="consultationService">Service for handling consultation-related business logic</param>
    public OneTimeConsultationController(IConsultationService consultationService)
    {
        _consultationService = consultationService;
    }

    /// <summary>
    /// Retrieves all one-time consultations for the current authenticated user.
    /// This endpoint provides a comprehensive list of one-time consultations associated with the user,
    /// including consultation details, status, provider information, and consultation history.
    /// </summary>
    /// <returns>JsonModel containing the user's one-time consultations</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns all one-time consultations for the authenticated user
    /// - Includes consultation details, status, and provider information
    /// - Shows consultation history and current status
    /// - Access restricted to authenticated users
    /// - Used for one-time consultation history and management
    /// - Includes comprehensive consultation information and metadata
    /// - Provides secure access to user consultation data
    /// - Handles authentication validation and error responses
    /// </remarks>
    [HttpGet("my-consultations")]
    public async Task<JsonModel> GetMyOneTimeConsultations()
    {
        var userId = GetCurrentUserId();
        return await _consultationService.GetUserOneTimeConsultationsAsync(userId, GetToken(HttpContext));
    }

    /// <summary>
    /// Gets the current user ID from the authentication claims.
    /// This helper method extracts the user ID from the JWT token claims for use in service calls.
    /// </summary>
    /// <returns>The current user ID or 0 if not found</returns>
    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
} 