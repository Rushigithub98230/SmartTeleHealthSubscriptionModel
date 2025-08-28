using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;

namespace SmartTelehealth.Application.Interfaces
{
    public interface IMessagingService
    {
        Task<JsonModel> SendMessageAsync(CreateMessageDto createDto, int senderId, TokenModel tokenModel);
        Task<JsonModel> GetMessageAsync(string messageId, TokenModel tokenModel);
        Task<JsonModel> GetChatRoomMessagesAsync(string chatRoomId, int page, int pageSize, TokenModel tokenModel);
        Task<JsonModel> UpdateMessageAsync(string messageId, UpdateMessageDto updateDto, TokenModel tokenModel);
        Task<JsonModel> DeleteMessageAsync(string messageId, TokenModel tokenModel);
        Task<JsonModel> GetChatRoomAsync(string chatRoomId, TokenModel tokenModel);
        Task<JsonModel> GetUserChatRoomsAsync(int userId, TokenModel tokenModel);
        Task<JsonModel> UpdateChatRoomAsync(string chatRoomId, UpdateChatRoomDto updateDto, TokenModel tokenModel);
        Task<JsonModel> DeleteChatRoomAsync(string chatRoomId, TokenModel tokenModel);
        Task<JsonModel> AddParticipantAsync(string chatRoomId, int userId, string role, TokenModel tokenModel);
        Task<JsonModel> RemoveParticipantAsync(string chatRoomId, int userId, TokenModel tokenModel);
        Task<JsonModel> GetChatRoomParticipantsAsync(string chatRoomId, TokenModel tokenModel);
        Task<JsonModel> UpdateParticipantRoleAsync(string chatRoomId, int userId, string newRole, TokenModel tokenModel);
        Task<JsonModel> MarkMessageAsReadAsync(string messageId, int userId, TokenModel tokenModel);
        Task<JsonModel> AddReactionAsync(string messageId, int userId, string reactionType, TokenModel tokenModel);
        Task<JsonModel> RemoveReactionAsync(string messageId, int userId, string reactionType, TokenModel tokenModel);
        Task<JsonModel> GetMessageReactionsAsync(string messageId, TokenModel tokenModel);
        Task<JsonModel> SearchMessagesAsync(string chatRoomId, string searchTerm, TokenModel tokenModel);
        Task<JsonModel> ValidateChatRoomAccessAsync(string chatRoomId, int userId, TokenModel tokenModel);
        Task<JsonModel> GetUnreadMessagesAsync(string chatRoomId, int userId, TokenModel tokenModel);
        Task<JsonModel> SendNotificationToUserAsync(int userId, string title, string message, string? data, TokenModel tokenModel);
        Task<JsonModel> SendNotificationToChatRoomAsync(string chatRoomId, string title, string message, TokenModel tokenModel);
        Task<JsonModel> SendTypingIndicatorAsync(string chatRoomId, int userId, bool isTyping, TokenModel tokenModel);
        Task<JsonModel> UploadMessageAttachmentAsync(byte[] fileData, string fileName, string contentType, TokenModel tokenModel);
        Task<JsonModel> DownloadMessageAttachmentAsync(string attachmentId, TokenModel tokenModel);
        Task<JsonModel> DeleteMessageAttachmentAsync(string attachmentId, TokenModel tokenModel);
        Task<JsonModel> EncryptMessageAsync(string message, string key, TokenModel tokenModel);
        Task<JsonModel> DecryptMessageAsync(string encryptedMessage, string key, TokenModel tokenModel);
        
        // Missing methods from controllers
        Task<JsonModel> CreateChatRoomAsync(CreateChatRoomDto createChatRoomDto, TokenModel tokenModel);
    }
} 