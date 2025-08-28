using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IChatRoomRepository
{
    Task<ChatRoom?> GetByIdAsync(Guid id);
    Task<IEnumerable<ChatRoom>> GetByUserIdAsync(int userId);
    Task<IEnumerable<ChatRoom>> GetByProviderIdAsync(int providerId);
    Task<IEnumerable<ChatRoom>> GetBySubscriptionIdAsync(Guid subscriptionId);
    Task<IEnumerable<ChatRoom>> GetByConsultationIdAsync(Guid consultationId);
    Task<IEnumerable<ChatRoom>> GetActiveChatRoomsAsync();
    Task<ChatRoom> CreateAsync(ChatRoom chatRoom);
    Task<ChatRoom> UpdateAsync(ChatRoom chatRoom);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<int> GetCountAsync();
    Task<IEnumerable<ChatRoom>> SearchAsync(string searchTerm);
} 