using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IMessageReactionRepository
{
    Task<MessageReaction?> GetByIdAsync(Guid id);
    Task<IEnumerable<MessageReaction>> GetByMessageIdAsync(Guid messageId);
    Task<IEnumerable<MessageReaction>> GetByUserIdAsync(int userId);
    Task<MessageReaction?> GetByMessageAndUserAsync(Guid messageId, int userId);
    Task<MessageReaction> CreateAsync(MessageReaction reaction);
    Task<MessageReaction> UpdateAsync(MessageReaction reaction);
    Task<bool> RemoveReactionAsync(Guid messageId, string emoji, int userId);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<int> GetReactionCountAsync(Guid messageId);
    Task<bool> HasUserReactedAsync(Guid messageId, int userId);
} 