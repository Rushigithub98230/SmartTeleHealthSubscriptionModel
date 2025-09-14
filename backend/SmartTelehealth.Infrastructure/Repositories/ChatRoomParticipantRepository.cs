using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories;

public class ChatRoomParticipantRepository : RepositoryBase<ChatRoomParticipant>, IChatRoomParticipantRepository
{
    private readonly ApplicationDbContext _context;

    public ChatRoomParticipantRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves a chat room participant by its unique identifier with related entities
    /// </summary>
    public override async Task<ChatRoomParticipant?> GetByIdAsync(object id)
    {
        if (id is not Guid participantId)
            return null;

        return await _context.ChatRoomParticipants
            .Include(crp => crp.ChatRoom)
            .Include(crp => crp.User)
            .Include(crp => crp.Provider)
            .FirstOrDefaultAsync(crp => crp.Id == participantId && !crp.IsDeleted);
    }

    public async Task<ChatRoomParticipant?> GetByChatRoomAndUserAsync(Guid chatRoomId, int userId)
    {
        return await _context.ChatRoomParticipants
            .Include(crp => crp.ChatRoom)
            .Include(crp => crp.User)
            .Include(crp => crp.Provider)
            .FirstOrDefaultAsync(crp => crp.ChatRoomId == chatRoomId && 
                                      crp.UserId == userId && 
                                      !crp.IsDeleted);
    }

    public async Task<IEnumerable<ChatRoomParticipant>> GetByChatRoomIdAsync(Guid chatRoomId)
    {
        return await _context.ChatRoomParticipants
            .Include(crp => crp.ChatRoom)
            .Include(crp => crp.User)
            .Include(crp => crp.Provider)
            .Where(crp => crp.ChatRoomId == chatRoomId && !crp.IsDeleted)
            .OrderBy(crp => crp.JoinedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ChatRoomParticipant>> GetByUserIdAsync(int userId)
    {
        return await _context.ChatRoomParticipants
            .Include(crp => crp.ChatRoom)
            .Include(crp => crp.User)
            .Include(crp => crp.Provider)
            .Where(crp => crp.UserId == userId && !crp.IsDeleted)
            .OrderByDescending(crp => crp.JoinedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ChatRoomParticipant>> GetActiveParticipantsAsync(Guid chatRoomId)
    {
        return await _context.ChatRoomParticipants
            .Include(crp => crp.ChatRoom)
            .Include(crp => crp.User)
            .Include(crp => crp.Provider)
            .Where(crp => crp.ChatRoomId == chatRoomId && 
                         crp.Status == ChatRoomParticipant.ParticipantStatus.Active && 
                         !crp.IsDeleted)
            .OrderBy(crp => crp.JoinedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves all chat room participants with related entities
    /// </summary>
    public override async Task<IEnumerable<ChatRoomParticipant>> GetAllAsync()
    {
        return await _context.ChatRoomParticipants
            .Include(crp => crp.ChatRoom)
            .Include(crp => crp.User)
            .Include(crp => crp.Provider)
            .Where(crp => !crp.IsDeleted)
            .OrderByDescending(crp => crp.JoinedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Creates a new chat room participant
    /// </summary>
    public override async Task<ChatRoomParticipant> CreateAsync(ChatRoomParticipant participant)
    {
        participant.CreatedDate = DateTime.UtcNow;
        participant.JoinedAt = DateTime.UtcNow;
        return await base.CreateAsync(participant);
    }

    /// <summary>
    /// Updates an existing chat room participant
    /// </summary>
    public override async Task<ChatRoomParticipant> UpdateAsync(ChatRoomParticipant participant)
    {
        participant.UpdatedDate = DateTime.UtcNow;
        return await base.UpdateAsync(participant);
    }

    public async Task<bool> RemoveParticipantAsync(Guid chatRoomId, int userId)
    {
        var participant = await _context.ChatRoomParticipants
            .FirstOrDefaultAsync(crp => crp.ChatRoomId == chatRoomId && 
                                      crp.UserId == userId && 
                                      !crp.IsDeleted);

        if (participant == null)
            return false;

        participant.Status = ChatRoomParticipant.ParticipantStatus.Left;
        participant.LeftAt = DateTime.UtcNow;
        participant.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Deletes a chat room participant by its unique identifier (soft delete)
    /// </summary>
    public override async Task<bool> DeleteAsync(object id)
    {
        if (id is not Guid participantId)
            return false;

        var participant = await _context.ChatRoomParticipants.FindAsync(participantId);
        if (participant == null)
            return false;

        participant.IsDeleted = true;
        participant.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Checks if a chat room participant exists
    /// </summary>
    public override async Task<bool> ExistsAsync(object id)
    {
        if (id is not Guid participantId)
            return false;

        return await _context.ChatRoomParticipants.AnyAsync(crp => crp.Id == participantId && !crp.IsDeleted);
    }

    public async Task<int> GetParticipantCountAsync(Guid chatRoomId)
    {
        return await _context.ChatRoomParticipants
            .CountAsync(crp => crp.ChatRoomId == chatRoomId && 
                              crp.Status == ChatRoomParticipant.ParticipantStatus.Active && 
                              !crp.IsDeleted);
    }

    public async Task<bool> IsUserParticipantAsync(Guid chatRoomId, int userId)
    {
        return await _context.ChatRoomParticipants
            .AnyAsync(crp => crp.ChatRoomId == chatRoomId && 
                           crp.UserId == userId && 
                           crp.Status == ChatRoomParticipant.ParticipantStatus.Active && 
                           !crp.IsDeleted);
    }
} 