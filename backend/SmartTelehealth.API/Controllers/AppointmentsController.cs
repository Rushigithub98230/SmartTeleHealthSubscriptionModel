using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;

namespace SmartTelehealth.API.Controllers;

/// <summary>
/// Controller responsible for comprehensive appointment management and healthcare scheduling.
/// This controller provides extensive functionality for appointment booking, management,
/// video call integration, payment processing, participant management, and appointment
/// lifecycle operations. It integrates with OpenTok for video conferencing and Stripe for payments.
/// </summary>
[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class AppointmentsController : BaseController
{
    private readonly IAppointmentService _appointmentService;
    private readonly IOpenTokService _openTokService;

    /// <summary>
    /// Initializes a new instance of the AppointmentsController with required services.
    /// </summary>
    /// <param name="appointmentService">Service for handling appointment-related business logic</param>
    /// <param name="openTokService">Service for OpenTok video conferencing integration</param>
    public AppointmentsController(
        IAppointmentService appointmentService,
        IOpenTokService openTokService)
    {
        _appointmentService = appointmentService;
        _openTokService = openTokService;
    }

    /// <summary>
    /// Retrieves homepage data for the appointment system including categories, providers, and statistics.
    /// This endpoint provides comprehensive data for the application homepage including featured providers,
    /// available categories, and system statistics for public display.
    /// </summary>
    /// <returns>JsonModel containing homepage data including categories, providers, and statistics</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns homepage data including categories and featured providers
    /// - Includes system statistics and public information
    /// - Provides data for public homepage display
    /// - Access is anonymous (no authentication required)
    /// - Used for homepage data loading and public information display
    /// - Includes comprehensive homepage information and statistics
    /// - Provides data for public access and homepage rendering
    /// - Handles public data retrieval and error responses
    /// </remarks>
    [HttpGet("homepage")]
    [AllowAnonymous]
    public async Task<JsonModel> GetHomepageData()
    {
        return await _appointmentService.GetHomepageDataAsync(GetToken(HttpContext));
    }

    /// <summary>
    /// Retrieves all available appointment categories with their associated subscription information.
    /// This endpoint provides comprehensive category data including subscription details,
    /// pricing information, and category-specific features for appointment booking.
    /// </summary>
    /// <returns>JsonModel containing categories with subscription information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns all appointment categories with subscription details
    /// - Includes pricing information and category features
    /// - Shows subscription requirements and category-specific information
    /// - Access restricted to authenticated users
    /// - Used for appointment category selection and booking
    /// - Includes comprehensive category information and subscription details
    /// - Provides data for appointment booking and category management
    /// - Handles category data retrieval and error responses
    /// </remarks>
    [HttpGet("categories")]
    public async Task<JsonModel> GetCategories()
    {
        return await _appointmentService.GetCategoriesWithSubscriptionsAsync(GetToken(HttpContext));
    }

    /// <summary>
    /// Retrieves featured healthcare providers for appointment booking.
    /// This endpoint provides a curated list of featured providers including their profiles,
    /// specialties, ratings, and availability information for appointment booking.
    /// </summary>
    /// <returns>JsonModel containing featured providers information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns featured healthcare providers with detailed information
    /// - Includes provider profiles, specialties, and ratings
    /// - Shows provider availability and appointment information
    /// - Access restricted to authenticated users
    /// - Used for provider selection and appointment booking
    /// - Includes comprehensive provider information and availability
    /// - Provides data for provider selection and appointment scheduling
    /// - Handles provider data retrieval and error responses
    /// </remarks>
    [HttpGet("providers/featured")]
    public async Task<JsonModel> GetFeaturedProviders()
    {
        return await _appointmentService.GetFeaturedProvidersAsync(GetToken(HttpContext));
    }

    /// <summary>
    /// Retrieves comprehensive home data for the appointment system.
    /// This endpoint provides detailed home page data including categories, providers,
    /// statistics, and system information for the appointment booking interface.
    /// </summary>
    /// <returns>JsonModel containing comprehensive home data</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns comprehensive home data including categories and providers
    /// - Includes system statistics and appointment information
    /// - Shows available services and booking options
    /// - Access restricted to authenticated users
    /// - Used for home page data loading and appointment interface
    /// - Includes comprehensive home information and booking data
    /// - Provides data for appointment interface and home page rendering
    /// - Handles home data retrieval and error responses
    /// </remarks>
    [HttpGet("home-data")]
    public async Task<JsonModel> GetHomeData()
    {
        return await _appointmentService.GetHomeDataAsync(GetToken(HttpContext));
    }

    /// <summary>
    /// Books a new appointment with a healthcare provider.
    /// This endpoint handles the complete appointment booking process including provider validation,
    /// availability checking, scheduling, and initial appointment setup.
    /// </summary>
    /// <param name="bookingDto">DTO containing appointment booking details</param>
    /// <returns>JsonModel containing the booking result and appointment information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Books new appointment with provider validation
    /// - Checks provider availability and scheduling conflicts
    /// - Handles appointment scheduling and setup
    /// - Validates booking requirements and participant information
    /// - Access restricted to authenticated users
    /// - Used for appointment booking and scheduling
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on booking operations
    /// - Maintains appointment audit trails and booking history
    /// </remarks>
    [HttpPost("book")]
    public async Task<JsonModel> BookAppointment([FromBody] AppointmentBookingDto bookingDto)
    {
        return await _appointmentService.BookAppointmentAsync(bookingDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Processes payment for a specific appointment.
    /// This endpoint handles payment processing for appointments including payment validation,
    /// Stripe integration, and payment confirmation for appointment booking.
    /// </summary>
    /// <param name="appointmentId">The unique identifier of the appointment</param>
    /// <param name="request">DTO containing payment processing details</param>
    /// <returns>JsonModel containing the payment processing result</returns>
    /// <remarks>
    /// This endpoint:
    /// - Processes payment for appointment booking
    /// - Integrates with Stripe for payment processing
    /// - Validates payment information and appointment details
    /// - Confirms payment and updates appointment status
    /// - Access restricted to appointment participants
    /// - Used for appointment payment processing
    /// - Includes comprehensive payment validation and error handling
    /// - Provides detailed feedback on payment operations
    /// - Maintains payment audit trails and transaction history
    /// </remarks>
    [HttpPost("{appointmentId}/payment")]
    public async Task<JsonModel> ProcessPayment(Guid appointmentId, [FromBody] ProcessPaymentDto request)
    {
        return await _appointmentService.ProcessPaymentAsync(appointmentId, request, GetToken(HttpContext));
    }

    // Provider actions
    [HttpPost("{appointmentId}/accept")]
    
    public async Task<JsonModel> AcceptAppointment(Guid appointmentId, [FromBody] ProviderAcceptDto acceptDto)
    {
        return await _appointmentService.ProviderAcceptAppointmentAsync(appointmentId, acceptDto, GetToken(HttpContext));
    }

    [HttpPost("{appointmentId}/reject")]
    
    public async Task<JsonModel> RejectAppointment(Guid appointmentId, [FromBody] ProviderRejectDto rejectDto)
    {
        return await _appointmentService.ProviderRejectAppointmentAsync(appointmentId, rejectDto, GetToken(HttpContext));
    }

    // Meeting management
    [HttpPost("{appointmentId}/start-meeting")]
    public async Task<JsonModel> StartMeeting(Guid appointmentId)
    {
        return await _appointmentService.StartMeetingAsync(appointmentId, GetToken(HttpContext));
    }

    [HttpPost("{appointmentId}/end-meeting")]
    public async Task<JsonModel> EndMeeting(Guid appointmentId)
    {
        return await _appointmentService.EndMeetingAsync(appointmentId, GetToken(HttpContext));
    }

    [HttpPost("{appointmentId}/complete")]
    
    public async Task<JsonModel> CompleteAppointment(Guid appointmentId, [FromBody] CompleteAppointmentDto completeDto)
    {
        return await _appointmentService.CompleteAppointmentAsync(appointmentId, completeDto, GetToken(HttpContext));
    }

    // Video call integration
    [HttpGet("{appointmentId}/meeting-link")]
    public async Task<JsonModel> GetMeetingLink(Guid appointmentId)
    {
        return await _appointmentService.GenerateMeetingLinkAsync(appointmentId, GetToken(HttpContext));
    }

    [HttpGet("{appointmentId}/opentok-token")]
    public async Task<JsonModel> GetOpenTokToken(Guid appointmentId)
    {
        var userId = GetCurrentUserId();
        return await _appointmentService.GetOpenTokTokenAsync(appointmentId, userId, GetToken(HttpContext));
    }

    // CRUD operations
    [HttpGet]
    public async Task<JsonModel> GetUserAppointments()
    {
        var userId = GetCurrentUserId();
        return await _appointmentService.GetPatientAppointmentsAsync(userId, GetToken(HttpContext));
    }

    [HttpGet("{id}")]
    public async Task<JsonModel> GetAppointment(Guid id)
    {
        return await _appointmentService.GetAppointmentByIdAsync(id, GetToken(HttpContext));
    }

    [HttpPut("{id}")]
    public async Task<JsonModel> UpdateAppointment(Guid id, [FromBody] UpdateAppointmentDto updateDto)
    {
        return await _appointmentService.UpdateAppointmentAsync(id, updateDto, GetToken(HttpContext));
    }

    [HttpDelete("{id}")]
    public async Task<JsonModel> CancelAppointment(Guid id, [FromBody] string reason)
    {
        return await _appointmentService.CancelAppointmentAsync(id, reason, GetToken(HttpContext));
    }

    // Provider availability
    [HttpGet("providers/{providerId}/availability")]
    [AllowAnonymous]
    public async Task<JsonModel> GetProviderAvailability(Guid providerId, [FromQuery] DateTime date)
    {
        return await _appointmentService.GetProviderAvailabilityAsync(providerId, date, GetToken(HttpContext));
    }

    // Analytics
    [HttpGet("analytics")]
    
    public async Task<JsonModel> GetAnalytics([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        return await _appointmentService.GetAppointmentAnalyticsAsync(startDate, endDate, GetToken(HttpContext));
    }

    // --- PARTICIPANT MANAGEMENT ---
    [HttpPost("{appointmentId}/participants")]
    public async Task<JsonModel> AddParticipant(Guid appointmentId, [FromBody] AddParticipantDto request)
    {
        return await _appointmentService.AddParticipantAsync(
            appointmentId, 
            request.UserId, 
            request.Email, 
            request.Phone, 
            !string.IsNullOrEmpty(request.Role) ? Guid.Parse(request.Role) : Guid.Empty, 
            !string.IsNullOrEmpty(request.InvitedByUserId) ? int.Parse(request.InvitedByUserId) : 0, 
            GetToken(HttpContext));
    }

    [HttpPost("{appointmentId}/invite-external")]
    public async Task<JsonModel> InviteExternal(Guid appointmentId, [FromBody] InviteExternalDto request)
    {
        if (string.IsNullOrEmpty(request.Email))
        {
            return new JsonModel 
            { 
                data = new object(), 
                Message = "Email is required",
                StatusCode = 400
            };
        }

        var invitedByUserId = request.InvitedByUserId ?? 0;
        return await _appointmentService.InviteExternalAsync(appointmentId, request.Email, request.Phone, request.Message, invitedByUserId, GetToken(HttpContext));
    }

    [HttpPost("{appointmentId}/join")]
    public async Task<JsonModel> JoinAppointment(Guid appointmentId, [FromBody] JoinAppointmentDto request)
    {
        return await _appointmentService.MarkParticipantJoinedAsync(appointmentId, request.UserId, request.Email, GetToken(HttpContext));
    }

    [HttpPost("{appointmentId}/leave")]
    public async Task<JsonModel> LeaveAppointment(Guid appointmentId, [FromBody] LeaveAppointmentDto request)
    {
        return await _appointmentService.MarkParticipantLeftAsync(appointmentId, request.UserId, request.Email, GetToken(HttpContext));
    }

    [HttpGet("{appointmentId}/participants")]
    public async Task<JsonModel> GetParticipants(Guid appointmentId)
    {
        return await _appointmentService.GetParticipantsAsync(appointmentId, GetToken(HttpContext));
    }

    [HttpGet("{appointmentId}/video-token")]
    public async Task<JsonModel> GetVideoToken(Guid appointmentId, [FromQuery] string? userId, [FromQuery] string? email, [FromQuery] Guid? role = null)
    {
        int? userIdInt = userId != null ? int.Parse(userId) : null;
        var token = await _appointmentService.GenerateVideoTokenAsync(appointmentId, userIdInt, email, role ?? Guid.Empty, GetToken(HttpContext));
        return new JsonModel 
        { 
            data = token, 
            Message = "Video token generated successfully", 
            StatusCode = 200 
        };
    }

    // --- PAYMENT MANAGEMENT ---
    [HttpPost("{appointmentId}/confirm-payment")]
    public async Task<JsonModel> ConfirmPayment(Guid appointmentId, [FromBody] ConfirmPaymentDto request)
    {
        return await _appointmentService.ConfirmPaymentAsync(appointmentId, request.PaymentIntentId, GetToken(HttpContext));
    }

    [HttpPost("{appointmentId}/refund")]
    public async Task<JsonModel> ProcessRefund(Guid appointmentId, [FromBody] ProcessRefundDto request)
    {
        if (string.IsNullOrEmpty(request.Reason))
        {
            return new JsonModel 
            { 
                data = new object(), 
                Message = "Reason is required",
                StatusCode = 400
            };
        }

        return await _appointmentService.ProcessRefundAsync(appointmentId, request.RefundAmount, request.Reason, GetToken(HttpContext));
    }

    [HttpGet("{appointmentId}/payment-logs")]
    public async Task<JsonModel> GetPaymentLogs(Guid appointmentId)
    {
        return await _appointmentService.GetPaymentLogsAsync(appointmentId, GetToken(HttpContext));
    }

    // --- PROVIDER ACTIONS ---
    [HttpPost("{appointmentId}/provider-action")]
    public async Task<JsonModel> ProviderAction(Guid appointmentId, [FromBody] ProviderActionDto request)
    {
        return await _appointmentService.ProviderActionAsync(appointmentId, request.Action, request.Notes, GetToken(HttpContext));
    }

    // Health check
    [HttpGet("health")]
    public async Task<JsonModel> HealthCheck()
    {
        return await _appointmentService.IsAppointmentServiceHealthyAsync(GetToken(HttpContext));
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
}

public class HomepageDto
{
    public List<CategoryWithSubscriptionsDto> Categories { get; set; } = new();
    public List<FeaturedProviderDto> FeaturedProviders { get; set; } = new();
    public int TotalAppointments { get; set; }
    public int TotalPatients { get; set; }
    public int TotalProviders { get; set; }
} 