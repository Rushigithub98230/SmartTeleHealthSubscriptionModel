using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories;

public class MessageRepository : IMessageRepository
{
    private readonly ApplicationDbContext _context;

    public MessageRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Message?> GetByIdAsync(Guid id)
    {
        return await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.ChatRoom)
            .Include(m => m.ReplyToMessage)
            .Include(m => m.Replies)
            .Include(m => m.Reactions)
            .Include(m => m.ReadReceipts)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<IEnumerable<Message>> GetAllAsync()
    {
        return await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.ChatRoom)
            .Include(m => m.ReplyToMessage)
            .ToListAsync();
    }

    public async Task<IEnumerable<Message>> GetByChatRoomIdAsync(Guid chatRoomId)
    {
        return await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.ChatRoom)
            .Include(m => m.ReplyToMessage)
            .Include(m => m.Replies)
            .Include(m => m.Reactions)
            .Include(m => m.ReadReceipts)
            .Where(m => m.ChatRoomId == chatRoomId && !m.IsDeleted)
            .OrderBy(m => m.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Message>> GetBySenderIdAsync(int senderId)
    {
        return await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.ChatRoom)
            .Include(m => m.ReplyToMessage)
            .Where(m => m.SenderId == senderId && !m.IsDeleted)
            .OrderByDescending(m => m.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Message>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.ChatRoom)
            .Include(m => m.ReplyToMessage)
            .Where(m => m.CreatedDate >= startDate && m.CreatedDate <= endDate && !m.IsDeleted)
            .OrderBy(m => m.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Message>> GetByTypeAsync(Message.MessageType type)
    {
        return await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.ChatRoom)
            .Include(m => m.ReplyToMessage)
            .Where(m => m.Type == type && !m.IsDeleted)
            .OrderByDescending(m => m.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Message>> GetByStatusAsync(Message.MessageStatus status)
    {
        return await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.ChatRoom)
            .Include(m => m.ReplyToMessage)
            .Where(m => m.Status == status && !m.IsDeleted)
            .OrderByDescending(m => m.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Message>> GetRepliesAsync(Guid messageId)
    {
        return await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.ChatRoom)
            .Include(m => m.ReplyToMessage)
            .Where(m => m.ReplyToMessageId == messageId && !m.IsDeleted)
            .OrderBy(m => m.CreatedDate)
            .ToListAsync();
    }

    public async Task<Message> AddAsync(Message message)
    {
        _context.Messages.Add(message);
        await _context.SaveChangesAsync();
        return message;
    }

    public async Task<Message> UpdateAsync(Message message)
    {
        _context.Messages.Update(message);
        await _context.SaveChangesAsync();
        return message;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var message = await _context.Messages.FindAsync(id);
        if (message == null)
            return false;

        message.IsDeleted = true;
        message.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Messages.AnyAsync(m => m.Id == id && !m.IsDeleted);
    }

    public async Task<int> GetCountAsync()
    {
        return await _context.Messages.CountAsync(m => !m.IsDeleted);
    }

    public async Task<IEnumerable<Message>> SearchAsync(string searchTerm, Guid chatRoomId)
    {
        return await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.ChatRoom)
            .Include(m => m.ReplyToMessage)
            .Where(m => m.ChatRoomId == chatRoomId && 
                       !m.IsDeleted && 
                       m.Content.Contains(searchTerm))
            .OrderByDescending(m => m.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Message>> GetUnreadMessagesAsync(Guid chatRoomId, int userId)
    {
        return await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.ChatRoom)
            .Include(m => m.ReplyToMessage)
            .Where(m => m.ChatRoomId == chatRoomId && 
                       !m.IsDeleted && 
                       m.SenderId != userId &&
                       !m.ReadReceipts.Any(rr => rr.UserId == userId))
            .OrderBy(m => m.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Message>> GetMessagesByDateRangeAsync(Guid chatRoomId, DateTime startDate, DateTime endDate)
    {
        return await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.ChatRoom)
            .Include(m => m.ReplyToMessage)
            .Where(m => m.ChatRoomId == chatRoomId && 
                       !m.IsDeleted && 
                       m.CreatedDate >= startDate && 
                       m.CreatedDate <= endDate)
            .OrderBy(m => m.CreatedDate)
            .ToListAsync();
    }

    // Additional interface methods
    public async Task<Message> CreateMessageAsync(Message message)
    {
        return await AddAsync(message);
    }

    public async Task<Message?> GetMessageByIdAsync(Guid id)
    {
        return await GetByIdAsync(id);
    }

    public async Task<IEnumerable<Message>> GetMessagesByChatRoomAsync(Guid chatRoomId, int skip = 0, int take = 50)
    {
        return await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.ChatRoom)
            .Include(m => m.ReplyToMessage)
            .Where(m => m.ChatRoomId == chatRoomId && !m.IsDeleted)
            .OrderByDescending(m => m.CreatedDate)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<Message> UpdateMessageAsync(Message message)
    {
        return await UpdateAsync(message);
    }

    public async Task<bool> DeleteMessageAsync(Guid id)
    {
        return await DeleteAsync(id);
    }

    public async Task<bool> SoftDeleteMessageAsync(Guid id)
    {
        return await DeleteAsync(id); // Already implements soft delete
    }

    public async Task<IEnumerable<Message>> SearchMessagesAsync(Guid chatRoomId, string searchTerm)
    {
        return await SearchAsync(searchTerm, chatRoomId);
    }

    public async Task<MessageReaction> AddReactionAsync(MessageReaction reaction)
    {
        _context.MessageReactions.Add(reaction);
        await _context.SaveChangesAsync();
        return reaction;
    }

    public async Task<bool> RemoveReactionAsync(Guid messageId, int userId, string emoji)
    {
        var reaction = await _context.MessageReactions
            .FirstOrDefaultAsync(mr => mr.MessageId == messageId && 
                                     mr.UserId == userId && 
                                     mr.Emoji == emoji);
        
        if (reaction == null)
            return false;
        
        _context.MessageReactions.Remove(reaction);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<MessageReaction>> GetMessageReactionsAsync(Guid messageId)
    {
        return await _context.MessageReactions
            .Include(mr => mr.User)
            .Where(mr => mr.MessageId == messageId)
            .ToListAsync();
    }

    public async Task<MessageReadReceipt> MarkMessageAsReadAsync(Guid messageId, int userId)
    {
        var existingReceipt = await _context.MessageReadReceipts
            .FirstOrDefaultAsync(mrr => mrr.MessageId == messageId && mrr.UserId == userId);

        if (existingReceipt != null)
            return existingReceipt;

        var receipt = new MessageReadReceipt
        {
            MessageId = messageId,
            UserId = userId,
            ReadAt = DateTime.UtcNow,
            CreatedDate = DateTime.UtcNow
        };

        _context.MessageReadReceipts.Add(receipt);
        await _context.SaveChangesAsync();
        return receipt;
    }

    public async Task<IEnumerable<MessageReadReceipt>> GetMessageReadReceiptsAsync(Guid messageId)
    {
        return await _context.MessageReadReceipts
            .Include(mrr => mrr.User)
            .Where(mrr => mrr.MessageId == messageId)
            .ToListAsync();
    }

    public async Task<int> GetUnreadMessageCountAsync(Guid chatRoomId, int userId)
    {
        return await _context.Messages
            .CountAsync(m => m.ChatRoomId == chatRoomId && 
                           !m.IsDeleted && 
                           m.SenderId != userId && 
                           !m.ReadReceipts.Any(rr => rr.UserId == userId));
    }

    public async Task<IEnumerable<Message>> GetMessageThreadAsync(Guid messageId)
    {
        var message = await _context.Messages
            .Include(m => m.ReplyToMessage)
            .FirstOrDefaultAsync(m => m.Id == messageId);

        if (message?.ReplyToMessageId == null)
            return new List<Message>();

        var thread = new List<Message>();
        var currentMessage = message;

        while (currentMessage?.ReplyToMessageId != null)
        {
            currentMessage = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.ReplyToMessage)
                .FirstOrDefaultAsync(m => m.Id == currentMessage.ReplyToMessageId);
            
            if (currentMessage != null)
                thread.Insert(0, currentMessage);
        }

        return thread;
    }

    public async Task<IEnumerable<Message>> GetRepliesToMessageAsync(Guid messageId)
    {
        return await GetRepliesAsync(messageId);
    }

    public async Task<int> GetMessageCountAsync(Guid chatRoomId)
    {
        return await _context.Messages
            .CountAsync(m => m.ChatRoomId == chatRoomId && !m.IsDeleted);
    }

    public async Task<DateTime?> GetLastMessageTimeAsync(Guid chatRoomId)
    {
        var lastMessage = await _context.Messages
            .Where(m => m.ChatRoomId == chatRoomId && !m.IsDeleted)
            .OrderByDescending(m => m.CreatedDate)
            .FirstOrDefaultAsync();

        return lastMessage?.CreatedDate;
    }
} 