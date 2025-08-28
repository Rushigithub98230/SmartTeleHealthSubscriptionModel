using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces
{
    public interface IChatRoomService
    {
        Task<JsonModel> CreateChatRoomAsync(CreateChatRoomDto createDto);
        Task<JsonModel> CreatePatientProviderChatRoomAsync(string patientId, string providerId, string? subscriptionId = null);
        Task<JsonModel> CreateGroupChatRoomAsync(string name, string? description, List<string> participantIds, string creatorId);
        Task<JsonModel> GetChatRoomAsync(string chatRoomId);
        Task<JsonModel> GetUserChatRoomsAsync(int userId);
        Task<JsonModel> GetUnreadMessagesAsync(int userId, string chatRoomId);
        Task<JsonModel> UpdateChatRoomAsync(string chatRoomId, UpdateChatRoomDto updateDto);
        Task<JsonModel> RemoveParticipantAsync(string chatRoomId, int userId);
        Task<JsonModel> GetChatRoomParticipantsAsync(string chatRoomId);
        Task<JsonModel> AddParticipantAsync(string chatRoomId, int userId, string role = "Member");
        Task<JsonModel> UpdateParticipantRoleAsync(string chatRoomId, int userId, string newRole);
        Task<JsonModel> ValidateChatAccessAsync(int userId, string chatRoomId);
        Task<JsonModel> GetChatRoomAsync(string chatRoomId, int userId);
        Task<JsonModel> GetChatRoomMessagesAsync(string chatRoomId, int userId);
    }
} 