using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using System.Security.Claims;

namespace SmartTelehealth.API.Controllers;

/// <summary>
/// Controller responsible for managing healthcare consultations and consultation lifecycle.
/// This controller provides comprehensive functionality for creating, managing, and tracking
/// healthcare consultations between patients and providers, including consultation scheduling,
/// status management, and consultation completion workflows.
/// </summary>
[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class ConsultationsController : BaseController
{
    private readonly IConsultationService _consultationService;

    /// <summary>
    /// Initializes a new instance of the ConsultationsController with the required consultation service.
    /// </summary>
    /// <param name="consultationService">Service for handling consultation-related business logic</param>
    public ConsultationsController(IConsultationService consultationService)
    {
        _consultationService = consultationService;
    }

    /// <summary>
    /// Retrieves all consultations for the current authenticated user.
    /// This endpoint returns a comprehensive list of consultations associated with the user,
    /// including both patient and provider consultations based on the user's role.
    /// </summary>
    /// <returns>JsonModel containing the list of user consultations</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns all consultations for the authenticated user
    /// - Includes consultation details, status, and participant information
    /// - Shows both patient and provider consultations based on user role
    /// - Access restricted to authenticated users only
    /// - Used for consultation history and management
    /// - Includes comprehensive consultation information and status
    /// - Provides secure access to user's consultation data
    /// - Handles authentication validation and error responses
    /// </remarks>
    [HttpGet]
    public async Task<JsonModel> GetUserConsultations()
    {
        var userId = GetCurrentUserId();
        return await _consultationService.GetUserConsultationsAsync(userId, GetToken(HttpContext));
    }

    /// <summary>
    /// Retrieves detailed information about a specific consultation by its ID.
    /// This endpoint provides comprehensive consultation details including participant information,
    /// consultation history, and current status for authorized users.
    /// </summary>
    /// <param name="id">The unique identifier of the consultation to retrieve</param>
    /// <returns>JsonModel containing the consultation details</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns detailed consultation information by ID
    /// - Includes participant details, consultation history, and status
    /// - Shows consultation notes, outcomes, and related information
    /// - Access restricted to consultation participants and administrators
    /// - Used for consultation details and management
    /// - Includes comprehensive consultation data and participant information
    /// - Provides secure access to consultation information
    /// - Handles authorization validation and error responses
    /// </remarks>
    [HttpGet("{id}")]
    public async Task<JsonModel> GetConsultation(Guid id)
    {
        return await _consultationService.GetConsultationByIdAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Creates a new healthcare consultation between patients and providers.
    /// This endpoint handles the consultation creation process including participant validation,
    /// scheduling, and initial consultation setup.
    /// </summary>
    /// <param name="createDto">DTO containing consultation creation details</param>
    /// <returns>JsonModel containing the created consultation information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Creates a new consultation with participant validation
    /// - Handles consultation scheduling and setup
    /// - Validates participant availability and permissions
    /// - Access restricted to authenticated users
    /// - Used for consultation booking and creation
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on consultation creation
    /// - Maintains consultation audit trails and participant records
    /// </remarks>
    [HttpPost]
    public async Task<JsonModel> CreateConsultation(CreateConsultationDto createDto)
    {
        return await _consultationService.CreateConsultationAsync(createDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Updates an existing consultation with new information.
    /// This endpoint allows authorized users to modify consultation details,
    /// participant information, and consultation settings.
    /// </summary>
    /// <param name="id">The unique identifier of the consultation to update</param>
    /// <param name="updateDto">DTO containing the updated consultation information</param>
    /// <returns>JsonModel containing the update result</returns>
    /// <remarks>
    /// This endpoint:
    /// - Updates consultation information with validation
    /// - Ensures data integrity and consistency
    /// - Validates participant permissions and consultation status
    /// - Access restricted to consultation participants and administrators
    /// - Used for consultation editing and information updates
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on update operations
    /// - Maintains consultation audit trails and change history
    /// </remarks>
    [HttpPut("{id}")]
    public async Task<JsonModel> UpdateConsultation(Guid id, UpdateConsultationDto updateDto)
    {
        return await _consultationService.UpdateConsultationAsync(id, updateDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Cancels an existing consultation with a specified reason.
    /// This endpoint handles consultation cancellation including participant notifications,
    /// status updates, and cancellation reason tracking.
    /// </summary>
    /// <param name="id">The unique identifier of the consultation to cancel</param>
    /// <param name="reason">The reason for consultation cancellation</param>
    /// <returns>JsonModel containing the cancellation result</returns>
    /// <remarks>
    /// This endpoint:
    /// - Cancels consultation with reason tracking
    /// - Sends notifications to all participants
    /// - Updates consultation status and cancellation information
    /// - Access restricted to consultation participants and administrators
    /// - Used for consultation cancellation and management
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on cancellation operations
    /// - Maintains consultation audit trails and cancellation history
    /// </remarks>
    [HttpPost("{id}/cancel")]
    public async Task<JsonModel> CancelConsultation(Guid id, [FromBody] string reason)
    {
        return await _consultationService.CancelConsultationAsync(id, reason, GetToken(HttpContext));
    }

    /// <summary>
    /// Starts an existing consultation session.
    /// This endpoint initiates a consultation session, updates the consultation status,
    /// and prepares the consultation for active participation.
    /// </summary>
    /// <param name="id">The unique identifier of the consultation to start</param>
    /// <returns>JsonModel containing the consultation start result</returns>
    /// <remarks>
    /// This endpoint:
    /// - Starts consultation session with status updates
    /// - Validates consultation readiness and participant availability
    /// - Updates consultation status and session information
    /// - Access restricted to consultation participants and administrators
    /// - Used for consultation session initiation
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on consultation start operations
    /// - Maintains consultation audit trails and session history
    /// </remarks>
    [HttpPost("{id}/start")]
    public async Task<JsonModel> StartConsultation(Guid id)
    {
        return await _consultationService.StartConsultationAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Completes an existing consultation session with notes and outcomes.
    /// This endpoint finalizes a consultation session, records consultation outcomes,
    /// and updates the consultation status to completed.
    /// </summary>
    /// <param name="id">The unique identifier of the consultation to complete</param>
    /// <param name="notes">Consultation notes and outcomes</param>
    /// <returns>JsonModel containing the consultation completion result</returns>
    /// <remarks>
    /// This endpoint:
    /// - Completes consultation session with notes and outcomes
    /// - Records consultation results and participant feedback
    /// - Updates consultation status and completion information
    /// - Access restricted to consultation participants and administrators
    /// - Used for consultation completion and outcome recording
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on consultation completion
    /// - Maintains consultation audit trails and outcome history
    /// </remarks>
    [HttpPost("{id}/complete")]
    public async Task<JsonModel> CompleteConsultation(Guid id, [FromBody] string notes)
    {
        return await _consultationService.CompleteConsultationAsync(id, notes, GetToken(HttpContext));
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
} 