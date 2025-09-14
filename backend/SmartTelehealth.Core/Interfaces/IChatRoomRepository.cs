using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IChatRoomRepository : IRepositoryBase<ChatRoom>
{
    // Basic CRUD methods are inherited from IRepositoryBase<ChatRoom>
    // GetByIdAsync, GetAllAsync, CreateAsync, UpdateAsync, DeleteAsync, ExistsAsync

    // Specialized methods
    Task<IEnumerable<ChatRoom>> GetByUserIdAsync(int userId);
    Task<IEnumerable<ChatRoom>> GetByProviderIdAsync(int providerId);
    Task<IEnumerable<ChatRoom>> GetBySubscriptionIdAsync(Guid subscriptionId);
    Task<IEnumerable<ChatRoom>> GetByConsultationIdAsync(Guid consultationId);
    Task<IEnumerable<ChatRoom>> GetActiveChatRoomsAsync();
    Task<int> GetCountAsync();
    Task<IEnumerable<ChatRoom>> SearchAsync(string searchTerm);
} 