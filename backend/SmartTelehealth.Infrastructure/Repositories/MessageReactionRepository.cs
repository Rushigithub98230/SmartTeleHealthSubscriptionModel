using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories;

public class MessageReactionRepository : IMessageReactionRepository
{
    private readonly ApplicationDbContext _context;

    public MessageReactionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MessageReaction?> GetByIdAsync(Guid id)
    {
        return await _context.MessageReactions
            .Include(mr => mr.Message)
            .Include(mr => mr.User)
            .FirstOrDefaultAsync(mr => mr.Id == id && !mr.IsDeleted);
    }

    public async Task<IEnumerable<MessageReaction>> GetByMessageIdAsync(Guid messageId)
    {
        return await _context.MessageReactions
            .Include(mr => mr.Message)
            .Include(mr => mr.User)
            .Where(mr => mr.MessageId == messageId && !mr.IsDeleted)
            .OrderBy(mr => mr.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<MessageReaction>> GetByUserIdAsync(int userId)
    {
        return await _context.MessageReactions
            .Include(mr => mr.Message)
            .Include(mr => mr.User)
            .Where(mr => mr.UserId == userId && !mr.IsDeleted)
            .OrderByDescending(mr => mr.CreatedDate)
            .ToListAsync();
    }

    public async Task<MessageReaction?> GetByMessageAndUserAsync(Guid messageId, int userId)
    {
        return await _context.MessageReactions
            .Include(mr => mr.Message)
            .Include(mr => mr.User)
            .FirstOrDefaultAsync(mr => mr.MessageId == messageId && 
                                     mr.UserId == userId && 
                                     !mr.IsDeleted);
    }

    public async Task<MessageReaction> CreateAsync(MessageReaction reaction)
    {
        reaction.CreatedDate = DateTime.UtcNow;
        _context.MessageReactions.Add(reaction);
        await _context.SaveChangesAsync();
        return reaction;
    }

    public async Task<MessageReaction> UpdateAsync(MessageReaction reaction)
    {
        reaction.UpdatedDate = DateTime.UtcNow;
        _context.MessageReactions.Update(reaction);
        await _context.SaveChangesAsync();
        return reaction;
    }

    public async Task<bool> RemoveReactionAsync(Guid messageId, string emoji, int userId)
    {
        var reaction = await _context.MessageReactions
            .FirstOrDefaultAsync(mr => mr.MessageId == messageId && 
                                     mr.UserId == userId && 
                                     mr.Emoji == emoji && 
                                     !mr.IsDeleted);

        if (reaction == null)
            return false;

        reaction.IsDeleted = true;
        reaction.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var reaction = await _context.MessageReactions.FindAsync(id);
        if (reaction == null)
            return false;

        reaction.IsDeleted = true;
        reaction.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.MessageReactions.AnyAsync(mr => mr.Id == id && !mr.IsDeleted);
    }

    public async Task<int> GetReactionCountAsync(Guid messageId)
    {
        return await _context.MessageReactions
            .CountAsync(mr => mr.MessageId == messageId && !mr.IsDeleted);
    }

    public async Task<bool> HasUserReactedAsync(Guid messageId, int userId)
    {
        return await _context.MessageReactions
            .AnyAsync(mr => mr.MessageId == messageId && 
                           mr.UserId == userId && 
                           !mr.IsDeleted);
    }
} 