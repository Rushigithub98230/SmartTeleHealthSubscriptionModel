using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace SmartTelehealth.API.Controllers;

/// <summary>
/// Controller responsible for comprehensive message management and communication functionality.
/// This controller provides extensive functionality for sending, receiving, managing, and processing
/// messages including message operations, reactions, attachments, encryption, and search capabilities
/// for healthcare communication and messaging systems.
/// </summary>
[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class MessageController : BaseController
{
    private readonly IMessagingService _messagingService;

    /// <summary>
    /// Initializes a new instance of the MessageController with the required messaging service.
    /// </summary>
    /// <param name="messagingService">Service for handling messaging-related business logic</param>
    public MessageController(IMessagingService messagingService)
    {
        _messagingService = messagingService;
    }

    /// <summary>
    /// Retrieves detailed information about a specific message by its ID.
    /// This endpoint provides comprehensive message details including content, metadata,
    /// sender information, and message status for authorized users.
    /// </summary>
    /// <param name="messageId">The unique identifier of the message</param>
    /// <returns>JsonModel containing the message details</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns detailed message information by ID
    /// - Includes message content, metadata, and sender information
    /// - Shows message status and delivery information
    /// - Access restricted to message participants and authorized users
    /// - Used for message details and communication management
    /// - Includes comprehensive message information and metadata
    /// - Provides secure access to message information
    /// - Handles authorization validation and error responses
    /// </remarks>
    [HttpGet("{messageId}")]
    public async Task<JsonModel> GetMessage(Guid messageId)
    {
        return await _messagingService.GetMessageAsync(messageId.ToString(), GetToken(HttpContext));
    }

    /// <summary>
    /// Sends a new message to a chat room or recipient.
    /// This endpoint handles message creation and delivery including content validation,
    /// recipient verification, and message processing for healthcare communication.
    /// </summary>
    /// <param name="createDto">DTO containing message creation details</param>
    /// <returns>JsonModel containing the sent message information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Sends a new message with content validation
    /// - Validates recipient and chat room access
    /// - Processes message delivery and notifications
    /// - Access restricted to authenticated users
    /// - Used for message sending and communication
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on message sending
    /// - Maintains message delivery audit trails
    /// </remarks>
    [HttpPost]
    public async Task<JsonModel> SendMessage([FromBody] CreateMessageDto createDto)
    {
        var userId = GetCurrentUserId();
        return await _messagingService.SendMessageAsync(createDto, userId, GetToken(HttpContext));
    }

    /// <summary>
    /// Updates an existing message with new content or metadata.
    /// This endpoint allows authorized users to modify message content, edit messages,
    /// and update message metadata while maintaining data integrity and audit trails.
    /// </summary>
    /// <param name="messageId">The unique identifier of the message to update</param>
    /// <param name="updateDto">DTO containing the updated message information</param>
    /// <returns>JsonModel containing the update result</returns>
    /// <remarks>
    /// This endpoint:
    /// - Updates message content and metadata with validation
    /// - Ensures data integrity and consistency
    /// - Validates message editing permissions and business rules
    /// - Access restricted to message senders and authorized users
    /// - Used for message editing and content management
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on update operations
    /// - Maintains message edit audit trails and change history
    /// </remarks>
    [HttpPut("{messageId}")]
    public async Task<JsonModel> UpdateMessage(Guid messageId, [FromBody] UpdateMessageDto updateDto)
    {
        var userId = GetCurrentUserId();
        return await _messagingService.UpdateMessageAsync(messageId.ToString(), updateDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Deletes a message from the system.
    /// This endpoint handles message deletion including cleanup of related data,
    /// validation of deletion permissions, and message history management.
    /// </summary>
    /// <param name="messageId">The unique identifier of the message to delete</param>
    /// <returns>JsonModel containing the deletion result</returns>
    /// <remarks>
    /// This endpoint:
    /// - Deletes message with cleanup of related data
    /// - Validates deletion permissions and message status
    /// - Handles message cleanup and data integrity
    /// - Access restricted to message senders and administrators
    /// - Used for message management and cleanup
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on deletion operations
    /// - Maintains message deletion audit trails
    /// </remarks>
    [HttpDelete("{messageId}")]
    public async Task<JsonModel> DeleteMessage(Guid messageId)
    {
        var userId = GetCurrentUserId();
        return await _messagingService.DeleteMessageAsync(messageId.ToString(), GetToken(HttpContext));
    }

    [HttpPost("{messageId}/read")]
    public async Task<JsonModel> MarkMessageAsRead(Guid messageId)
    {
        var userId = GetCurrentUserId();
        return await _messagingService.MarkMessageAsReadAsync(messageId.ToString(), userId, GetToken(HttpContext));
    }

    [HttpPost("{messageId}/reactions")]
    public async Task<JsonModel> AddReaction(Guid messageId, [FromQuery] string reactionType)
    {
        var userId = GetCurrentUserId();
        return await _messagingService.AddReactionAsync(messageId.ToString(), userId, reactionType, GetToken(HttpContext));
    }

    [HttpDelete("{messageId}/reactions")]
    public async Task<JsonModel> RemoveReaction(Guid messageId, [FromQuery] string reactionType)
    {
        var userId = GetCurrentUserId();
        return await _messagingService.RemoveReactionAsync(messageId.ToString(), userId, reactionType, GetToken(HttpContext));
    }

    [HttpGet("{messageId}/reactions")]
    public async Task<JsonModel> GetMessageReactions(Guid messageId)
    {
        return await _messagingService.GetMessageReactionsAsync(messageId.ToString(), GetToken(HttpContext));
    }

    [HttpPost("search")]
    public async Task<JsonModel> SearchMessages([FromQuery] string chatRoomId, [FromQuery] string searchTerm)
    {
        return await _messagingService.SearchMessagesAsync(chatRoomId, searchTerm, GetToken(HttpContext));
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

 