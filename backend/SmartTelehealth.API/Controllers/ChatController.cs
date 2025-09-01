using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Application.Services;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class ChatController : BaseController
{
    private readonly IMessagingService _messagingService;
    private readonly ChatService _chatService;
    private readonly ChatRoomService _chatRoomService;

    public ChatController(
        IMessagingService messagingService,
        ChatService chatService,
        ChatRoomService chatRoomService)
    {
        _messagingService = messagingService;
        _chatService = chatService;
        _chatRoomService = chatRoomService;
    }

    [HttpPost("messages")]
    public async Task<JsonModel> SendMessage([FromBody] CreateMessageDto createDto)
    {
        var userId = GetCurrentUserId();
        return await _messagingService.SendMessageAsync(createDto, userId, GetToken(HttpContext));
    }

    [HttpPost("messages/with-notification")]
    public async Task<JsonModel> SendMessageWithNotification([FromBody] CreateMessageDto createDto)
    {
        var userId = GetCurrentUserId();
        return await _messagingService.SendMessageAsync(createDto, userId, GetToken(HttpContext));
    }

    [HttpGet("messages/{messageId}")]
    public async Task<JsonModel> GetMessage(Guid messageId)
    {
        return await _messagingService.GetMessageAsync(messageId.ToString(), GetToken(HttpContext));
    }

    [HttpGet("rooms/{chatRoomId}/messages")]
    public async Task<JsonModel> GetChatRoomMessages(
        Guid chatRoomId, 
        [FromQuery] int skip = 0, 
        [FromQuery] int take = 50)
    {
        var userId = GetCurrentUserId();
        return await _messagingService.GetChatRoomMessagesAsync(chatRoomId.ToString(), skip, take, GetToken(HttpContext));
    }

    [HttpPut("messages/{messageId}")]
    public async Task<JsonModel> UpdateMessage(Guid messageId, [FromBody] UpdateMessageDto updateDto)
    {
        var userId = GetCurrentUserId();
        return await _messagingService.UpdateMessageAsync(messageId.ToString(), updateDto, GetToken(HttpContext));
    }

    [HttpDelete("messages/{messageId}")]
    public async Task<JsonModel> DeleteMessage(Guid messageId)
    {
        var userId = GetCurrentUserId();
        return await _messagingService.DeleteMessageAsync(messageId.ToString(), GetToken(HttpContext));
    }

    [HttpPost("messages/{messageId}/read")]
    public async Task<JsonModel> MarkMessageAsRead(Guid messageId)
    {
        var userId = GetCurrentUserId();
        return await _messagingService.MarkMessageAsReadAsync(messageId.ToString(), userId, GetToken(HttpContext));
    }

    [HttpPost("messages/{messageId}/reactions")]
    public async Task<JsonModel> AddReaction(Guid messageId, [FromQuery] string reactionType)
    {
        var userId = GetCurrentUserId();
        return await _messagingService.AddReactionAsync(messageId.ToString(), userId, reactionType, GetToken(HttpContext));
    }

    [HttpDelete("messages/{messageId}/reactions")]
    public async Task<JsonModel> RemoveReaction(Guid messageId, [FromQuery] string reactionType)
    {
        var userId = GetCurrentUserId();
        return await _messagingService.RemoveReactionAsync(messageId.ToString(), userId, reactionType, GetToken(HttpContext));
    }

    [HttpGet("messages/{messageId}/reactions")]
    public async Task<JsonModel> GetMessageReactions(Guid messageId)
    {
        return await _messagingService.GetMessageReactionsAsync(messageId.ToString(), GetToken(HttpContext));
    }

    [HttpPost("search")]
    public async Task<JsonModel> SearchMessages([FromQuery] string chatRoomId, [FromQuery] string searchTerm)
    {
        return await _messagingService.SearchMessagesAsync(chatRoomId, searchTerm, GetToken(HttpContext));
    }

    [HttpPost("rooms")]
    public async Task<JsonModel> CreateChatRoom([FromBody] CreateChatRoomDto createDto)
    {
        return await _messagingService.CreateChatRoomAsync(createDto, GetToken(HttpContext));
    }

    [HttpGet("rooms/{chatRoomId}")]
    public async Task<JsonModel> GetChatRoom(Guid chatRoomId)
    {
        return await _messagingService.GetChatRoomAsync(chatRoomId.ToString(), GetToken(HttpContext));
    }

    [HttpGet("users/{userId}/rooms")]
    public async Task<JsonModel> GetUserChatRooms(string userId)
    {
        if (!int.TryParse(userId, out int parsedUserId))
        {
            return new JsonModel { data = new object(), Message = "Invalid user ID format", StatusCode = 400 };
        }
        return await _messagingService.GetUserChatRoomsAsync(parsedUserId, GetToken(HttpContext));
    }

    [HttpPut("rooms/{chatRoomId}")]
    public async Task<JsonModel> UpdateChatRoom(Guid chatRoomId, [FromBody] UpdateChatRoomDto updateDto)
    {
        return await _messagingService.UpdateChatRoomAsync(chatRoomId.ToString(), updateDto, GetToken(HttpContext));
    }

    [HttpDelete("rooms/{chatRoomId}")]
    public async Task<JsonModel> DeleteChatRoom(Guid chatRoomId)
    {
        return await _messagingService.DeleteChatRoomAsync(chatRoomId.ToString(), GetToken(HttpContext));
    }

    [HttpPost("rooms/{chatRoomId}/participants")]
    public async Task<JsonModel> AddParticipant(Guid chatRoomId, [FromQuery] string userId, [FromQuery] string role)
    {
        if (!int.TryParse(userId, out int parsedUserId))
        {
            return new JsonModel { data = new object(), Message = "Invalid user ID format", StatusCode = 400 };
        }
        return await _messagingService.AddParticipantAsync(chatRoomId.ToString(), parsedUserId, role, GetToken(HttpContext));
    }

    [HttpDelete("rooms/{chatRoomId}/participants/{userId}")]
    public async Task<JsonModel> RemoveParticipant(Guid chatRoomId, string userId)
    {
        if (!int.TryParse(userId, out int parsedUserId))
        {
            return new JsonModel { data = new object(), Message = "Invalid user ID format", StatusCode = 400 };
        }
        return await _messagingService.RemoveParticipantAsync(chatRoomId.ToString(), parsedUserId, GetToken(HttpContext));
    }

    [HttpGet("rooms/{chatRoomId}/participants")]
    public async Task<JsonModel> GetChatRoomParticipants(Guid chatRoomId)
    {
        return await _messagingService.GetChatRoomParticipantsAsync(chatRoomId.ToString(), GetToken(HttpContext));
    }

    [HttpPut("rooms/{chatRoomId}/participants/{userId}/role")]
    public async Task<JsonModel> UpdateParticipantRole(Guid chatRoomId, string userId, [FromQuery] string newRole)
    {
        if (!int.TryParse(userId, out int parsedUserId))
        {
            return new JsonModel { data = new object(), Message = "Invalid user ID format", StatusCode = 400 };
        }
        return await _messagingService.UpdateParticipantRoleAsync(chatRoomId.ToString(), parsedUserId, newRole, GetToken(HttpContext));
    }

    [HttpGet("rooms/{chatRoomId}/unread")]
    public async Task<JsonModel> GetUnreadMessages(Guid chatRoomId)
    {
        var userId = GetCurrentUserId();
        return await _messagingService.GetUnreadMessagesAsync(chatRoomId.ToString(), userId, GetToken(HttpContext));
    }

    [HttpPost("rooms/{chatRoomId}/validate-access")]
    public async Task<JsonModel> ValidateChatRoomAccess(Guid chatRoomId)
    {
        var userId = GetCurrentUserId();
        return await _messagingService.ValidateChatRoomAccessAsync(chatRoomId.ToString(), userId, GetToken(HttpContext));
    }

    [HttpPost("rooms/{chatRoomId}/typing")]
    public async Task<JsonModel> SendTypingIndicator(Guid chatRoomId, [FromQuery] bool isTyping)
    {
        var userId = GetCurrentUserId();
        return await _messagingService.SendTypingIndicatorAsync(chatRoomId.ToString(), userId, isTyping, GetToken(HttpContext));
    }

    [HttpPost("notifications/user/{userId}")]
    public async Task<JsonModel> SendNotificationToUser(string userId, [FromBody] SendNotificationRequest request)
    {
        if (!int.TryParse(userId, out int parsedUserId))
        {
            return new JsonModel { data = new object(), Message = "Invalid user ID format", StatusCode = 400 };
        }
        return await _messagingService.SendNotificationToUserAsync(parsedUserId, request.Title, request.Message, request.Data, GetToken(HttpContext));
    }

    [HttpPost("rooms/{chatRoomId}/notifications")]
    public async Task<JsonModel> SendNotificationToChatRoom(Guid chatRoomId, [FromBody] SendNotificationRequest request)
    {
        return await _messagingService.SendNotificationToChatRoomAsync(chatRoomId.ToString(), request.Title, request.Message, GetToken(HttpContext));
    }

    [HttpPost("attachments/upload")]
    public async Task<JsonModel> UploadAttachment(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return new JsonModel { data = new object(), Message = "No file provided", StatusCode = 400 };
        }

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var fileData = memoryStream.ToArray();

        return await _messagingService.UploadMessageAttachmentAsync(fileData, file.FileName, file.ContentType, GetToken(HttpContext));
    }

    [HttpGet("attachments/{attachmentId}")]
    public async Task<JsonModel> DownloadAttachment(string attachmentId)
    {
        return await _messagingService.DownloadMessageAttachmentAsync(attachmentId, GetToken(HttpContext));
    }

    [HttpDelete("attachments/{attachmentId}")]
    public async Task<JsonModel> DeleteAttachment(string attachmentId)
    {
        return await _messagingService.DeleteMessageAttachmentAsync(attachmentId, GetToken(HttpContext));
    }

    [HttpPost("encrypt")]
    public async Task<JsonModel> EncryptMessage([FromBody] EncryptMessageRequest request)
    {
        return await _messagingService.EncryptMessageAsync(request.Message, request.Key, GetToken(HttpContext));
    }

    [HttpPost("decrypt")]
    public async Task<JsonModel> DecryptMessage([FromBody] DecryptMessageRequest request)
    {
        return await _messagingService.DecryptMessageAsync(request.EncryptedMessage, request.Key, GetToken(HttpContext));
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
}

// Supporting DTOs
public class SendNotificationRequest
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Data { get; set; }
}

public class EncryptMessageRequest
{
    public string Message { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
}

public class DecryptMessageRequest
{
    public string EncryptedMessage { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
} 