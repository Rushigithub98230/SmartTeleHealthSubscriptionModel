using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class AppointmentsController : BaseController
{
    private readonly IAppointmentService _appointmentService;
    private readonly IOpenTokService _openTokService;

    public AppointmentsController(
        IAppointmentService appointmentService,
        IOpenTokService openTokService)
    {
        _appointmentService = appointmentService;
        _openTokService = openTokService;
    }

    // Homepage endpoints
    [HttpGet("homepage")]
    [AllowAnonymous]
    public async Task<JsonModel> GetHomepageData()
    {
        return await _appointmentService.GetHomepageDataAsync(GetToken(HttpContext));
    }

    [HttpGet("categories")]
    public async Task<JsonModel> GetCategories()
    {
        return await _appointmentService.GetCategoriesWithSubscriptionsAsync(GetToken(HttpContext));
    }

    [HttpGet("providers/featured")]
    public async Task<JsonModel> GetFeaturedProviders()
    {
        return await _appointmentService.GetFeaturedProvidersAsync(GetToken(HttpContext));
    }

    [HttpGet("home-data")]
    public async Task<JsonModel> GetHomeData()
    {
        return await _appointmentService.GetHomeDataAsync(GetToken(HttpContext));
    }

    // Appointment booking flow
    [HttpPost("book")]
    public async Task<JsonModel> BookAppointment([FromBody] AppointmentBookingDto bookingDto)
    {
        return await _appointmentService.BookAppointmentAsync(bookingDto, GetToken(HttpContext));
    }

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