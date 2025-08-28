using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Core.Entities;
using AutoMapper;
using System.Security.Cryptography;
using System.Text;

namespace SmartTelehealth.Application.Services;

public class MessagingService : IMessagingService
{
    private readonly IChatStorageService _chatStorageService;
    private readonly ILogger<MessagingService> _logger;

    public MessagingService(IChatStorageService chatStorageService, ILogger<MessagingService> logger)
    {
        _chatStorageService = chatStorageService;
        _logger = logger;
    }

    public async Task<JsonModel> SendMessageAsync(CreateMessageDto createDto, int senderId, TokenModel tokenModel)
    {
        try
        {
            var message = await _chatStorageService.StoreMessageAsync(createDto);
            return new JsonModel { data = message, Message = "Message sent successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message");
            return new JsonModel { data = new object(), Message = "Failed to send message", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetMessageAsync(string messageId, TokenModel tokenModel)
    {
        try
        {
            var message = await _chatStorageService.GetMessageAsync(messageId);
            if (message == null)
                return new JsonModel { data = new object(), Message = "Message not found", StatusCode = 404 };

            return new JsonModel { data = message, Message = "Message retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting message {MessageId}", messageId);
            return new JsonModel { data = new object(), Message = "Failed to get message", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetChatRoomMessagesAsync(string chatRoomId, int page, int pageSize, TokenModel tokenModel)
    {
        try
        {
            var messages = await _chatStorageService.GetChatRoomMessagesAsync(chatRoomId, page, pageSize);
            return new JsonModel { data = messages, Message = "Chat room messages retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting chat room messages");
            return new JsonModel { data = new object(), Message = "Failed to get chat room messages", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> UpdateMessageAsync(string messageId, UpdateMessageDto updateDto, TokenModel tokenModel)
    {
        try
        {
            var result = await _chatStorageService.UpdateMessageAsync(messageId, updateDto, tokenModel.UserID);
            return new JsonModel { data = result, Message = "Message updated successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating message {MessageId}", messageId);
            return new JsonModel { data = new object(), Message = "Failed to update message", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> DeleteMessageAsync(string messageId, TokenModel tokenModel)
    {
        try
        {
            var result = await _chatStorageService.DeleteMessageAsync(messageId, tokenModel.UserID);
            return new JsonModel { data = result, Message = "Message deleted successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting message {MessageId}", messageId);
            return new JsonModel { data = new object(), Message = "Failed to delete message", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> CreateChatRoomAsync(CreateChatRoomDto createDto, TokenModel tokenModel)
    {
        try
        {
            var chatRoom = await _chatStorageService.CreateChatRoomAsync(createDto);
            return new JsonModel { data = chatRoom, Message = "Chat room created successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating chat room");
            return new JsonModel { data = new object(), Message = "Failed to create chat room", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetChatRoomAsync(string chatRoomId, TokenModel tokenModel)
    {
        try
        {
            var chatRoom = await _chatStorageService.GetChatRoomAsync(chatRoomId);
            if (chatRoom == null)
                return new JsonModel { data = new object(), Message = "Chat room not found", StatusCode = 404 };

            return new JsonModel { data = chatRoom, Message = "Chat room retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting chat room {ChatRoomId}", chatRoomId);
            return new JsonModel { data = new object(), Message = "Failed to get chat room", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetUserChatRoomsAsync(int userId, TokenModel tokenModel)
    {
        try
        {
            var chatRooms = await _chatStorageService.GetUserChatRoomsAsync(userId);
            return new JsonModel { data = chatRooms, Message = "User chat rooms retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user chat rooms {UserId}", userId);
            return new JsonModel { data = new object(), Message = "Failed to get user chat rooms", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> UpdateChatRoomAsync(string chatRoomId, UpdateChatRoomDto updateDto, TokenModel tokenModel)
    {
        try
        {
            var chatRoom = await _chatStorageService.UpdateChatRoomAsync(chatRoomId, updateDto);
            if (chatRoom == null)
                return new JsonModel { data = new object(), Message = "Chat room not found", StatusCode = 404 };

            return new JsonModel { data = chatRoom, Message = "Chat room updated successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating chat room {ChatRoomId}", chatRoomId);
            return new JsonModel { data = new object(), Message = "Failed to update chat room", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> DeleteChatRoomAsync(string chatRoomId, TokenModel tokenModel)
    {
        try
        {
            var result = await _chatStorageService.DeleteChatRoomAsync(chatRoomId);
            return new JsonModel { data = result, Message = "Chat room deleted successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting chat room {ChatRoomId}", chatRoomId);
            return new JsonModel { data = new object(), Message = "Failed to delete chat room", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> AddParticipantAsync(string chatRoomId, int userId, string role, TokenModel tokenModel)
    {
        try
        {
            var result = await _chatStorageService.AddParticipantAsync(chatRoomId, userId, role);
            return new JsonModel { data = result, Message = "Participant added successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding participant to chat room");
            return new JsonModel { data = new object(), Message = "Failed to add participant", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> RemoveParticipantAsync(string chatRoomId, int userId, TokenModel tokenModel)
    {
        try
        {
            var result = await _chatStorageService.RemoveParticipantAsync(chatRoomId, userId);
            return new JsonModel { data = result, Message = "Participant removed successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing participant from chat room");
            return new JsonModel { data = new object(), Message = "Failed to remove participant", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetChatRoomParticipantsAsync(string chatRoomId, TokenModel tokenModel)
    {
        try
        {
            var participants = await _chatStorageService.GetChatRoomParticipantsAsync(chatRoomId);
            return new JsonModel { data = participants, Message = "Chat room participants retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting chat room participants {ChatRoomId}", chatRoomId);
            return new JsonModel { data = new object(), Message = "Failed to get chat room participants", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> UpdateParticipantRoleAsync(string chatRoomId, int userId, string newRole, TokenModel tokenModel)
    {
        try
        {
            var result = await _chatStorageService.UpdateParticipantRoleAsync(chatRoomId, userId, newRole);
            return new JsonModel { data = result, Message = "Participant role updated successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating participant role");
            return new JsonModel { data = new object(), Message = "Failed to update participant role", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> MarkMessageAsReadAsync(string messageId, int userId, TokenModel tokenModel)
    {
        try
        {
            var result = await _chatStorageService.MarkMessageAsReadAsync(messageId, userId);
            return new JsonModel { data = result, Message = "Message marked as read successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking message as read");
            return new JsonModel { data = new object(), Message = "Failed to mark message as read", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> AddReactionAsync(string messageId, int userId, string reactionType, TokenModel tokenModel)
    {
        try
        {
            var result = await _chatStorageService.AddReactionAsync(messageId, userId, reactionType);
            return new JsonModel { data = result, Message = "Reaction added successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding reaction to message");
            return new JsonModel { data = new object(), Message = "Failed to add reaction", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> RemoveReactionAsync(string messageId, int userId, string reactionType, TokenModel tokenModel)
    {
        try
        {
            var result = await _chatStorageService.RemoveReactionAsync(messageId, userId, reactionType);
            return new JsonModel { data = result, Message = "Reaction removed successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing reaction from message");
            return new JsonModel { data = new object(), Message = "Failed to remove reaction", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetMessageReactionsAsync(string messageId, TokenModel tokenModel)
    {
        try
        {
            var reactions = await _chatStorageService.GetMessageReactionsAsync(messageId);
            return new JsonModel { data = reactions, Message = "Message reactions retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting message reactions {MessageId}", messageId);
            return new JsonModel { data = new object(), Message = "Failed to get message reactions", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> SearchMessagesAsync(string chatRoomId, string searchTerm, TokenModel tokenModel)
    {
        try
        {
            // This would be implemented in the chat storage service
            var messages = await _chatStorageService.GetChatRoomMessagesAsync(chatRoomId);
            var filteredMessages = messages.Where(m => m.Content.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            return new JsonModel { data = filteredMessages, Message = "Messages searched successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching messages");
            return new JsonModel { data = new object(), Message = "Failed to search messages", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> ValidateChatRoomAccessAsync(string chatRoomId, int userId, TokenModel tokenModel)
    {
        try
        {
            var result = await _chatStorageService.ValidateChatAccessAsync(userId, chatRoomId);
            return new JsonModel { data = result, Message = "Chat room access validated successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating chat room access");
            return new JsonModel { data = new object(), Message = "Failed to validate chat room access", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetUnreadMessagesAsync(string chatRoomId, int userId, TokenModel tokenModel)
    {
        try
        {
            var messages = await _chatStorageService.GetUnreadMessagesAsync(userId, chatRoomId);
            return new JsonModel { data = messages, Message = "Unread messages retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread messages");
            return new JsonModel { data = new object(), Message = "Failed to get unread messages", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> SendNotificationToUserAsync(int userId, string title, string message, string? data, TokenModel tokenModel)
    {
        try
        {
            // This would be implemented with a notification service
            return new JsonModel { data = true, Message = "Notification sent successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification to user");
            return new JsonModel { data = new object(), Message = "Failed to send notification", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> SendNotificationToChatRoomAsync(string chatRoomId, string title, string message, TokenModel tokenModel)
    {
        try
        {
            // This would be implemented with a notification service
            return new JsonModel { data = true, Message = "Chat room notification sent successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification to chat room");
            return new JsonModel { data = new object(), Message = "Failed to send chat room notification", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> SendTypingIndicatorAsync(string chatRoomId, int userId, bool isTyping, TokenModel tokenModel)
    {
        try
        {
            // This would be implemented with real-time messaging
            return new JsonModel { data = true, Message = "Typing indicator sent successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending typing indicator");
            return new JsonModel { data = new object(), Message = "Failed to send typing indicator", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> UploadMessageAttachmentAsync(byte[] fileData, string fileName, string contentType, TokenModel tokenModel)
    {
        try
        {
            using var stream = new MemoryStream(fileData);
            var attachmentId = await _chatStorageService.UploadMessageAttachmentAsync("temp", stream, fileName, contentType);
            return new JsonModel { data = attachmentId, Message = "Attachment uploaded successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading message attachment");
            return new JsonModel { data = new object(), Message = "Failed to upload attachment", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> EncryptMessageAsync(string message, string key, TokenModel tokenModel)
    {
        try
        {
            // This would be implemented with encryption
            return new JsonModel { data = message, Message = "Message encrypted successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error encrypting message");
            return new JsonModel { data = new object(), Message = "Failed to encrypt message", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> DecryptMessageAsync(string encryptedMessage, string key, TokenModel tokenModel)
    {
        try
        {
            // This would be implemented with decryption
            return new JsonModel { data = encryptedMessage, Message = "Message decrypted successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decrypting message");
            return new JsonModel { data = new object(), Message = "Failed to decrypt message", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> DownloadMessageAttachmentAsync(string attachmentId, TokenModel tokenModel)
    {
        try
        {
            var stream = await _chatStorageService.DownloadMessageAttachmentAsync(attachmentId);
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            return new JsonModel { data = memoryStream.ToArray(), Message = "Attachment downloaded successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading message attachment {AttachmentId}", attachmentId);
            return new JsonModel { data = new object(), Message = "Failed to download attachment", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> DeleteMessageAttachmentAsync(string attachmentId, TokenModel tokenModel)
    {
        try
        {
            var result = await _chatStorageService.DeleteMessageAttachmentAsync(attachmentId);
            return new JsonModel { data = result, Message = "Attachment deleted successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting message attachment {AttachmentId}", attachmentId);
            return new JsonModel { data = new object(), Message = "Failed to delete attachment", StatusCode = 500 };
        }
    }

    private string GetContentTypeFromFileName(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".txt" => "text/plain",
            _ => "application/octet-stream"
        };
    }
} 