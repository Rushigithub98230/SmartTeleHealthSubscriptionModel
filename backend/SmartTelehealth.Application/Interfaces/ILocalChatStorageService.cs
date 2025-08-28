using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface ILocalChatStorageService : IChatStorageService
{
    Task<JsonModel> CreateChatRoomAsync(CreateChatRoomDto createDto);
    Task<JsonModel> UpdateChatRoomAsync(Guid chatRoomId, UpdateChatRoomDto updateDto);
    Task<JsonModel> DeleteChatRoomAsync(Guid chatRoomId);
    Task<JsonModel> CreateMessageAsync(CreateMessageDto createDto);
    Task<JsonModel> UpdateMessageAsync(Guid messageId, UpdateMessageDto updateDto);
    Task<JsonModel> DeleteMessageAsync(Guid messageId);
    Task<JsonModel> GetMessageByIdAsync(Guid messageId);
    Task<JsonModel> GetMessagesByChatRoomAsync(Guid chatRoomId);
} 