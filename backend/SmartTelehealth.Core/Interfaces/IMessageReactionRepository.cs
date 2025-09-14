using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IMessageReactionRepository : IRepositoryBase<MessageReaction>
{
    // Basic CRUD methods are inherited from IRepositoryBase<MessageReaction>
    
    Task<IEnumerable<MessageReaction>> GetByMessageIdAsync(Guid messageId);
    Task<IEnumerable<MessageReaction>> GetByUserIdAsync(int userId);
    Task<MessageReaction?> GetByMessageAndUserAsync(Guid messageId, int userId);
    Task<bool> RemoveReactionAsync(Guid messageId, string emoji, int userId);
    Task<int> GetReactionCountAsync(Guid messageId);
    Task<bool> HasUserReactedAsync(Guid messageId, int userId);
} 