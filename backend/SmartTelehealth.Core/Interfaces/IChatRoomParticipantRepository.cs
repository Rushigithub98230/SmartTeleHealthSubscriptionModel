using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IChatRoomParticipantRepository : IRepositoryBase<ChatRoomParticipant>
{
    // Basic CRUD methods are inherited from IRepositoryBase<ChatRoomParticipant>
    
    Task<ChatRoomParticipant?> GetByChatRoomAndUserAsync(Guid chatRoomId, int userId);
    Task<IEnumerable<ChatRoomParticipant>> GetByChatRoomIdAsync(Guid chatRoomId);
    Task<IEnumerable<ChatRoomParticipant>> GetByUserIdAsync(int userId);
    Task<IEnumerable<ChatRoomParticipant>> GetActiveParticipantsAsync(Guid chatRoomId);
    Task<bool> RemoveParticipantAsync(Guid chatRoomId, int userId);
    Task<int> GetParticipantCountAsync(Guid chatRoomId);
    Task<bool> IsUserParticipantAsync(Guid chatRoomId, int userId);
} 