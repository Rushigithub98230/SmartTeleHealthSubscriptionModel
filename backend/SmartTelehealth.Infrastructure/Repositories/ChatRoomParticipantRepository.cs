using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories;

public class ChatRoomParticipantRepository : IChatRoomParticipantRepository
{
    private readonly ApplicationDbContext _context;

    public ChatRoomParticipantRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ChatRoomParticipant?> GetByIdAsync(Guid id)
    {
        return await _context.ChatRoomParticipants
            .Include(crp => crp.ChatRoom)
            .Include(crp => crp.User)
            .Include(crp => crp.Provider)
            .FirstOrDefaultAsync(crp => crp.Id == id && !crp.IsDeleted);
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

    public async Task<ChatRoomParticipant> CreateAsync(ChatRoomParticipant participant)
    {
        participant.CreatedDate = DateTime.UtcNow;
        participant.JoinedAt = DateTime.UtcNow;
        _context.ChatRoomParticipants.Add(participant);
        await _context.SaveChangesAsync();
        return participant;
    }

    public async Task<ChatRoomParticipant> UpdateAsync(ChatRoomParticipant participant)
    {
        participant.UpdatedDate = DateTime.UtcNow;
        _context.ChatRoomParticipants.Update(participant);
        await _context.SaveChangesAsync();
        return participant;
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

    public async Task<bool> DeleteAsync(Guid id)
    {
        var participant = await _context.ChatRoomParticipants.FindAsync(id);
        if (participant == null)
            return false;

        participant.IsDeleted = true;
        participant.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.ChatRoomParticipants.AnyAsync(crp => crp.Id == id && !crp.IsDeleted);
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