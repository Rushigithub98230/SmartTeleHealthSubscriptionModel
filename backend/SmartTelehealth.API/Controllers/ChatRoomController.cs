using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Application.Services;

namespace SmartTelehealth.API.Controllers;

/// <summary>
/// Controller responsible for comprehensive chat room management and communication functionality.
/// This controller provides extensive functionality for creating, managing, and controlling chat rooms
/// including patient-provider chat rooms, group chat rooms, direct messaging, participant management,
/// access control, and chat room lifecycle operations for healthcare communication.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ChatRoomController : BaseController
{
    private readonly IMessagingService _messagingService;
    private readonly ChatRoomService _chatRoomService;

    /// <summary>
    /// Initializes a new instance of the ChatRoomController with required services.
    /// </summary>
    /// <param name="messagingService">Service for handling messaging-related business logic</param>
    /// <param name="chatRoomService">Service for handling chat room operations</param>
    public ChatRoomController(
        IMessagingService messagingService,
        ChatRoomService chatRoomService)
    {
        _messagingService = messagingService;
        _chatRoomService = chatRoomService;
    }

    /// <summary>
    /// Creates a new chat room with specified configuration.
    /// This endpoint handles general chat room creation including room setup, configuration,
    /// and initial participant management for healthcare communication.
    /// </summary>
    /// <param name="createDto">DTO containing chat room creation details</param>
    /// <returns>JsonModel containing the created chat room information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Creates a new chat room with specified configuration
    /// - Sets up chat room settings and initial participants
    /// - Configures chat room for healthcare communication
    /// - Access restricted to authenticated users
    /// - Used for general chat room creation and management
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on chat room creation
    /// - Maintains chat room creation audit trails
    /// </remarks>
    [HttpPost]
    public async Task<JsonModel> CreateChatRoom([FromBody] CreateChatRoomDto createDto)
    {
        return await _messagingService.CreateChatRoomAsync(createDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Creates a specialized patient-provider chat room for healthcare communication.
    /// This endpoint creates a dedicated chat room between a patient and healthcare provider
    /// with subscription-based access control and healthcare-specific communication features.
    /// </summary>
    /// <param name="createDto">DTO containing patient-provider chat room creation details</param>
    /// <returns>JsonModel containing the created patient-provider chat room information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Creates a patient-provider specific chat room
    /// - Sets up healthcare communication features and access control
    /// - Links chat room to subscription for privilege management
    /// - Access restricted to authenticated users
    /// - Used for patient-provider communication setup
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on chat room creation
    /// - Maintains healthcare communication audit trails
    /// </remarks>
    [HttpPost("patient-provider")]
    public async Task<JsonModel> CreatePatientProviderChatRoom(
        [FromBody] CreatePatientProviderChatRoomDto createDto)
    {
        return await _chatRoomService.CreatePatientProviderChatRoomAsync(
            createDto.PatientId.ToString(), 
            createDto.ProviderId.ToString(), 
            createDto.SubscriptionId?.ToString());
    }

    /// <summary>
    /// Creates a group chat room with multiple participants.
    /// This endpoint creates a group chat room for multiple users including group setup,
    /// participant management, and group communication features for healthcare collaboration.
    /// </summary>
    /// <param name="createDto">DTO containing group chat room creation details</param>
    /// <returns>JsonModel containing the created group chat room information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Creates a group chat room with multiple participants
    /// - Sets up group communication features and participant management
    /// - Configures group settings and access control
    /// - Access restricted to authenticated users
    /// - Used for group healthcare communication and collaboration
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on group chat room creation
    /// - Maintains group communication audit trails
    /// </remarks>
    [HttpPost("group")]
    public async Task<JsonModel> CreateGroupChatRoom([FromBody] CreateGroupChatRoomDto createDto)
    {
        var userId = GetCurrentUserId();
        return await _chatRoomService.CreateGroupChatRoomAsync(
            createDto.Name,
            createDto.Description,
            createDto.ParticipantIds.Select(id => id.ToString()).ToList(),
            userId.ToString());
    }

    /// <summary>
    /// Creates a direct chat room between two users.
    /// This endpoint creates a private one-on-one chat room between two specific users
    /// for direct healthcare communication and private messaging.
    /// </summary>
    /// <param name="createDto">DTO containing direct chat room creation details</param>
    /// <returns>JsonModel containing the created direct chat room information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Creates a direct chat room between two users
    /// - Sets up private one-on-one communication
    /// - Configures direct messaging features and access control
    /// - Access restricted to authenticated users
    /// - Used for direct healthcare communication and private messaging
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on direct chat room creation
    /// - Maintains direct communication audit trails
    /// </remarks>
    [HttpPost("direct")]
    public async Task<JsonModel> CreateDirectChatRoom([FromBody] CreateDirectChatRoomDto createDto)
    {
        return await _chatRoomService.CreateGroupChatRoomAsync(
            createDto.Name ?? "Direct Chat",
            null,
            new List<string> { createDto.User1Id.ToString(), createDto.User2Id.ToString() },
            createDto.User1Id.ToString());
    }

    [HttpGet("{chatRoomId}")]
    public async Task<JsonModel> GetChatRoom(Guid chatRoomId)
    {
        return await _messagingService.GetChatRoomAsync(chatRoomId.ToString(), GetToken(HttpContext));
    }

    [HttpGet("users/{userId}")]
    public async Task<JsonModel> GetUserChatRooms(string userId)
    {
        if (!int.TryParse(userId, out int parsedUserId))
        {
            return new JsonModel { data = new object(), Message = "Invalid user ID format", StatusCode = 400 };
        }
        return await _messagingService.GetUserChatRoomsAsync(parsedUserId, GetToken(HttpContext));
    }

    [HttpPut("{chatRoomId}")]
    public async Task<JsonModel> UpdateChatRoom(Guid chatRoomId, [FromBody] UpdateChatRoomDto updateDto)
    {
        return await _messagingService.UpdateChatRoomAsync(chatRoomId.ToString(), updateDto, GetToken(HttpContext));
    }

    [HttpDelete("{chatRoomId}")]
    public async Task<JsonModel> DeleteChatRoom(Guid chatRoomId)
    {
        return await _messagingService.DeleteChatRoomAsync(chatRoomId.ToString(), GetToken(HttpContext));
    }

    [HttpPost("{chatRoomId}/participants")]
    public async Task<JsonModel> AddParticipant(Guid chatRoomId, [FromQuery] string userId, [FromQuery] string role)
    {
        if (!int.TryParse(userId, out int parsedUserId))
        {
            return new JsonModel { data = new object(), Message = "Invalid user ID format", StatusCode = 400 };
        }
        return await _messagingService.AddParticipantAsync(chatRoomId.ToString(), parsedUserId, role, GetToken(HttpContext));
    }

    [HttpDelete("{chatRoomId}/participants/{userId}")]
    public async Task<JsonModel> RemoveParticipant(Guid chatRoomId, string userId)
    {
        if (!int.TryParse(userId, out int parsedUserId))
        {
            return new JsonModel { data = new object(), Message = "Invalid user ID format", StatusCode = 400 };
        }
        return await _messagingService.RemoveParticipantAsync(chatRoomId.ToString(), parsedUserId, GetToken(HttpContext));
    }

    [HttpGet("{chatRoomId}/participants")]
    public async Task<JsonModel> GetChatRoomParticipants(Guid chatRoomId)
    {
        return await _messagingService.GetChatRoomParticipantsAsync(chatRoomId.ToString(), GetToken(HttpContext));
    }

    [HttpPut("{chatRoomId}/participants/{userId}/role")]
    public async Task<JsonModel> UpdateParticipantRole(Guid chatRoomId, string userId, [FromQuery] string newRole)
    {
        if (!int.TryParse(userId, out int parsedUserId))
        {
            return new JsonModel { data = new object(), Message = "Invalid user ID format", StatusCode = 400 };
        }
        return await _messagingService.UpdateParticipantRoleAsync(chatRoomId.ToString(), parsedUserId, newRole, GetToken(HttpContext));
    }

    [HttpPost("{chatRoomId}/validate-access")]
    public async Task<JsonModel> ValidateChatRoomAccess(Guid chatRoomId)
    {
        var userId = GetCurrentUserId();
        return await _messagingService.ValidateChatRoomAccessAsync(chatRoomId.ToString(), userId, GetToken(HttpContext));
    }

    [HttpPost("{chatRoomId}/archive")]
    public async Task<JsonModel> ArchiveChatRoom(Guid chatRoomId)
    {
        // TODO: Implement archive functionality
        return new JsonModel { data = new object(), Message = "Archive functionality not implemented yet", StatusCode = 501 };
    }

    [HttpPost("{chatRoomId}/unarchive")]
    public async Task<JsonModel> UnarchiveChatRoom(Guid chatRoomId)
    {
        // TODO: Implement unarchive functionality
        return new JsonModel { data = new object(), Message = "Unarchive functionality not implemented yet", StatusCode = 501 };
    }

    [HttpPost("{chatRoomId}/participants/{participantId}/mute")]
    public async Task<JsonModel> MuteParticipant(Guid chatRoomId, Guid participantId, [FromQuery] DateTime? muteUntil = null, [FromQuery] string? reason = null)
    {
        // TODO: Implement mute participant functionality
        return new JsonModel { data = new object(), Message = "Mute participant functionality not implemented yet", StatusCode = 501 };
    }

    [HttpPost("{chatRoomId}/participants/{participantId}/unmute")]
    public async Task<JsonModel> UnmuteParticipant(Guid chatRoomId, Guid participantId)
    {
        // TODO: Implement unmute participant functionality
        return new JsonModel { data = new object(), Message = "Unmute participant functionality not implemented yet", StatusCode = 501 };
    }

    [HttpGet("{chatRoomId}/statistics")]
    public async Task<JsonModel> GetChatRoomStatistics(Guid chatRoomId)
    {
        // TODO: Implement chat room statistics functionality
        return new JsonModel { data = new object(), Message = "Chat room statistics functionality not implemented yet", StatusCode = 501 };
    }

    [HttpGet("{chatRoomId}/history")]
    public async Task<JsonModel> GetChatHistory(Guid chatRoomId, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        // TODO: Implement chat history functionality
        return new JsonModel { data = new object(), Message = "Chat history functionality not implemented yet", StatusCode = 501 };
    }

    [HttpPost("{chatRoomId}/invite")]
    public async Task<JsonModel> InviteToChatRoom(Guid chatRoomId, [FromBody] InviteToChatRoomDto inviteDto)
    {
        // TODO: Implement invite to chat room functionality
        return new JsonModel { data = new object(), Message = "Invite to chat room functionality not implemented yet", StatusCode = 501 };
    }

    [HttpPost("{chatRoomId}/invite/accept")]
    public async Task<JsonModel> AcceptChatRoomInvite(Guid chatRoomId, [FromQuery] string inviteId)
    {
        // TODO: Implement accept chat room invite functionality
        return new JsonModel { data = new object(), Message = "Accept chat room invite functionality not implemented yet", StatusCode = 501 };
    }

    [HttpPost("{chatRoomId}/invite/decline")]
    public async Task<JsonModel> DeclineChatRoomInvite(Guid chatRoomId, [FromQuery] string inviteId)
    {
        // TODO: Implement decline chat room invite functionality
        return new JsonModel { data = new object(), Message = "Decline chat room invite functionality not implemented yet", StatusCode = 501 };
    }

    [HttpGet("{chatRoomId}/invites")]
    public async Task<JsonModel> GetChatRoomInvites(Guid chatRoomId)
    {
        // TODO: Implement get chat room invites functionality
        return new JsonModel { data = new object(), Message = "Get chat room invites functionality not implemented yet", StatusCode = 501 };
    }

    [HttpPost("{chatRoomId}/leave")]
    public async Task<JsonModel> LeaveChatRoom(Guid chatRoomId)
    {
        // TODO: Implement leave chat room functionality
        return new JsonModel { data = new object(), Message = "Leave chat room functionality not implemented yet", StatusCode = 501 };
    }

    [HttpPost("{chatRoomId}/join")]
    public async Task<JsonModel> JoinChatRoom(Guid chatRoomId)
    {
        // TODO: Implement join chat room functionality
        return new JsonModel { data = new object(), Message = "Join chat room functionality not implemented yet", StatusCode = 501 };
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
}

// Additional DTOs for specific operations
public class CreatePatientProviderChatRoomDto
{
    public Guid PatientId { get; set; }
    public Guid ProviderId { get; set; }
    public Guid? SubscriptionId { get; set; }
}

public class CreateGroupChatRoomDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<Guid> ParticipantIds { get; set; } = new();
}

public class CreateDirectChatRoomDto
{
    public Guid User1Id { get; set; }
    public Guid User2Id { get; set; }
    public string? Name { get; set; }
}

public class CreateSupportChatRoomDto
{
    public Guid UserId { get; set; }
    public string? Issue { get; set; }
}

public class ChatRoomAddParticipantDto
{
    public Guid UserId { get; set; }
    public string Role { get; set; } = "Member";
}

public class UpdateParticipantRoleDto
{
    public string NewRole { get; set; } = "Member";
}

public class InviteUserDto
{
    public Guid InviteeId { get; set; }
} 