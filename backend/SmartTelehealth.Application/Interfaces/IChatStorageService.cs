using SmartTelehealth.Application.DTOs;
using Microsoft.AspNetCore.Http;
using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Application.Interfaces
{
    public interface IChatStorageService
    {
        // Chat Room Management
        Task<ChatRoomDto> CreateChatRoomAsync(CreateChatRoomDto createDto);
        Task<ChatRoomDto?> GetChatRoomAsync(string chatRoomId);
        Task<ChatRoomDto?> UpdateChatRoomAsync(string chatRoomId, UpdateChatRoomDto updateDto);
        Task<bool> DeleteChatRoomAsync(string chatRoomId);
        Task<IEnumerable<ChatRoomDto>> GetUserChatRoomsAsync(int userId);

        // Chat Room Participants
        Task<bool> AddParticipantAsync(string chatRoomId, int userId, string role = "Member");
        Task<bool> RemoveParticipantAsync(string chatRoomId, int userId);
        Task<IEnumerable<ChatRoomParticipantDto>> GetChatRoomParticipantsAsync(string chatRoomId);
        Task<bool> UpdateParticipantRoleAsync(string chatRoomId, int userId, string newRole);

        // Messages
        Task<MessageDto> StoreMessageAsync(CreateMessageDto createDto);
        Task<MessageDto?> GetMessageAsync(string messageId);
        Task<IEnumerable<MessageDto>> GetChatRoomMessagesAsync(string chatRoomId, int page = 1, int pageSize = 50);
        Task<bool> UpdateMessageAsync(string messageId, UpdateMessageDto updateDto, int userId);
        Task<bool> DeleteMessageAsync(string messageId, int userId);
        Task<IEnumerable<MessageDto>> GetUnreadMessagesAsync(int userId, string chatRoomId);
        Task<bool> MarkMessageAsReadAsync(string messageId, int userId);

        // Message Reactions
        Task<bool> AddReactionAsync(string messageId, int userId, string reactionType);
        Task<bool> RemoveReactionAsync(string messageId, int userId, string reactionType);
        Task<IEnumerable<MessageReactionDto>> GetMessageReactionsAsync(string messageId);

        // Message Attachments
        Task<string> UploadMessageAttachmentAsync(string messageId, Stream fileStream, string fileName, string contentType);
        Task<Stream> DownloadMessageAttachmentAsync(string attachmentId);
        Task<bool> DeleteMessageAttachmentAsync(string attachmentId);

        // Search and Validation
        Task<IEnumerable<MessageDto>> SearchMessagesAsync(string chatRoomId, string searchTerm);
        Task<bool> ValidateChatAccessAsync(int userId, string chatRoomId);

        // Statistics
        Task<ChatStatisticsDto> GetChatStatisticsAsync(string chatRoomId);
    }
}

public class ChatActivityDto
{
    public Guid Id { get; set; }
    public Guid ChatRoomId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string ActivityType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? IpAddress { get; set; }
    public string? DeviceInfo { get; set; }
} 