using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IChatRoomParticipantRepository
{
    Task<ChatRoomParticipant?> GetByIdAsync(Guid id);
    Task<ChatRoomParticipant?> GetByChatRoomAndUserAsync(Guid chatRoomId, int userId);
    Task<IEnumerable<ChatRoomParticipant>> GetByChatRoomIdAsync(Guid chatRoomId);
    Task<IEnumerable<ChatRoomParticipant>> GetByUserIdAsync(int userId);
    Task<IEnumerable<ChatRoomParticipant>> GetActiveParticipantsAsync(Guid chatRoomId);
    Task<ChatRoomParticipant> CreateAsync(ChatRoomParticipant participant);
    Task<ChatRoomParticipant> UpdateAsync(ChatRoomParticipant participant);
    Task<bool> RemoveParticipantAsync(Guid chatRoomId, int userId);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<int> GetParticipantCountAsync(Guid chatRoomId);
    Task<bool> IsUserParticipantAsync(Guid chatRoomId, int userId);
} 