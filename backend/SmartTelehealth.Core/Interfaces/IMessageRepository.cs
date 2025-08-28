using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IMessageRepository
{
    // Message operations
    Task<Message> CreateMessageAsync(Message message);
    Task<Message?> GetMessageByIdAsync(Guid messageId);
    Task<IEnumerable<Message>> GetMessagesByChatRoomAsync(Guid chatRoomId, int skip = 0, int take = 50);
    Task<Message> UpdateMessageAsync(Message message);
    Task<bool> DeleteMessageAsync(Guid messageId);
    Task<bool> SoftDeleteMessageAsync(Guid messageId);

    // Message search
    Task<IEnumerable<Message>> SearchMessagesAsync(Guid chatRoomId, string searchTerm);
    Task<IEnumerable<Message>> GetMessagesByDateRangeAsync(Guid chatRoomId, DateTime startDate, DateTime endDate);

    // Message reactions
    Task<MessageReaction> AddReactionAsync(MessageReaction reaction);
    Task<bool> RemoveReactionAsync(Guid messageId, int userId, string emoji);
    Task<IEnumerable<MessageReaction>> GetMessageReactionsAsync(Guid messageId);

    // Read receipts
    Task<MessageReadReceipt> MarkMessageAsReadAsync(Guid messageId, int userId);
    Task<IEnumerable<MessageReadReceipt>> GetMessageReadReceiptsAsync(Guid messageId);
    Task<int> GetUnreadMessageCountAsync(Guid chatRoomId, int userId);

    // Threaded conversations
    Task<IEnumerable<Message>> GetMessageThreadAsync(Guid messageId);
    Task<IEnumerable<Message>> GetRepliesToMessageAsync(Guid messageId);

    // Message statistics
    Task<int> GetMessageCountAsync(Guid chatRoomId);
    Task<DateTime?> GetLastMessageTimeAsync(Guid chatRoomId);
    
    // Additional methods referenced in services
    Task<IEnumerable<Message>> GetByChatRoomIdAsync(Guid chatRoomId);
    Task<IEnumerable<Message>> GetRepliesAsync(Guid messageId);
} 