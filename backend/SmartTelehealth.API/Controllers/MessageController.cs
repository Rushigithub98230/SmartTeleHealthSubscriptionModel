using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class MessageController : BaseController
{
    private readonly IMessagingService _messagingService;

    public MessageController(IMessagingService messagingService)
    {
        _messagingService = messagingService;
    }

    [HttpGet("{messageId}")]
    public async Task<JsonModel> GetMessage(Guid messageId)
    {
        return await _messagingService.GetMessageAsync(messageId.ToString(), GetToken(HttpContext));
    }

    [HttpPost]
    public async Task<JsonModel> SendMessage([FromBody] CreateMessageDto createDto)
    {
        var userId = GetCurrentUserId();
        return await _messagingService.SendMessageAsync(createDto, userId, GetToken(HttpContext));
    }

    [HttpPut("{messageId}")]
    public async Task<JsonModel> UpdateMessage(Guid messageId, [FromBody] UpdateMessageDto updateDto)
    {
        var userId = GetCurrentUserId();
        return await _messagingService.UpdateMessageAsync(messageId.ToString(), updateDto, GetToken(HttpContext));
    }

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

 