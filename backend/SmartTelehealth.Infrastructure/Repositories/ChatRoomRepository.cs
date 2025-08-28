using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories;

public class ChatRoomRepository : IChatRoomRepository
{
    private readonly ApplicationDbContext _context;

    public ChatRoomRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ChatRoom?> GetByIdAsync(Guid id)
    {
        return await _context.ChatRooms
            .Include(cr => cr.Patient)
            .Include(cr => cr.Provider)
            .Include(cr => cr.Subscription)
            .Include(cr => cr.Consultation)
            .Include(cr => cr.Participants)
            .Include(cr => cr.Messages)
            .FirstOrDefaultAsync(cr => cr.Id == id && !cr.IsDeleted);
    }

    public async Task<IEnumerable<ChatRoom>> GetByUserIdAsync(int userId)
    {
        return await _context.ChatRooms
            .Include(cr => cr.Patient)
            .Include(cr => cr.Provider)
            .Include(cr => cr.Subscription)
            .Include(cr => cr.Consultation)
            .Include(cr => cr.Participants)
            .Where(cr => !cr.IsDeleted && 
                        (cr.PatientId == userId || 
                         cr.ProviderId == userId || 
                         cr.Participants.Any(p => p.UserId == userId && p.Status == ChatRoomParticipant.ParticipantStatus.Active)))
            .OrderByDescending(cr => cr.LastActivityAt ?? cr.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ChatRoom>> GetByProviderIdAsync(int providerId)
    {
        return await _context.ChatRooms
            .Include(cr => cr.Patient)
            .Include(cr => cr.Provider)
            .Include(cr => cr.Subscription)
            .Include(cr => cr.Consultation)
            .Where(cr => cr.ProviderId == providerId && !cr.IsDeleted)
            .OrderByDescending(cr => cr.LastActivityAt ?? cr.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ChatRoom>> GetBySubscriptionIdAsync(Guid subscriptionId)
    {
        return await _context.ChatRooms
            .Include(cr => cr.Patient)
            .Include(cr => cr.Provider)
            .Include(cr => cr.Subscription)
            .Include(cr => cr.Consultation)
            .Where(cr => cr.SubscriptionId == subscriptionId && !cr.IsDeleted)
            .OrderByDescending(cr => cr.LastActivityAt ?? cr.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ChatRoom>> GetByConsultationIdAsync(Guid consultationId)
    {
        return await _context.ChatRooms
            .Include(cr => cr.Patient)
            .Include(cr => cr.Provider)
            .Include(cr => cr.Subscription)
            .Include(cr => cr.Consultation)
            .Where(cr => cr.ConsultationId == consultationId && !cr.IsDeleted)
            .OrderByDescending(cr => cr.LastActivityAt ?? cr.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ChatRoom>> GetActiveChatRoomsAsync()
    {
        return await _context.ChatRooms
            .Include(cr => cr.Patient)
            .Include(cr => cr.Provider)
            .Include(cr => cr.Subscription)
            .Include(cr => cr.Consultation)
            .Where(cr => cr.Status == ChatRoom.ChatRoomStatus.Active && !cr.IsDeleted)
            .OrderByDescending(cr => cr.LastActivityAt ?? cr.CreatedDate)
            .ToListAsync();
    }

    public async Task<ChatRoom> CreateAsync(ChatRoom chatRoom)
    {
        chatRoom.CreatedDate = DateTime.UtcNow;
        _context.ChatRooms.Add(chatRoom);
        await _context.SaveChangesAsync();
        return chatRoom;
    }

    public async Task<ChatRoom> UpdateAsync(ChatRoom chatRoom)
    {
        chatRoom.UpdatedDate = DateTime.UtcNow;
        _context.ChatRooms.Update(chatRoom);
        await _context.SaveChangesAsync();
        return chatRoom;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var chatRoom = await _context.ChatRooms.FindAsync(id);
        if (chatRoom == null)
            return false;

        chatRoom.IsDeleted = true;
        chatRoom.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.ChatRooms.AnyAsync(cr => cr.Id == id && !cr.IsDeleted);
    }

    public async Task<int> GetCountAsync()
    {
        return await _context.ChatRooms.CountAsync(cr => !cr.IsDeleted);
    }

    public async Task<IEnumerable<ChatRoom>> SearchAsync(string searchTerm)
    {
        return await _context.ChatRooms
            .Include(cr => cr.Patient)
            .Include(cr => cr.Provider)
            .Include(cr => cr.Subscription)
            .Include(cr => cr.Consultation)
            .Where(cr => !cr.IsDeleted && 
                        (cr.Name.Contains(searchTerm) || 
                         cr.Description != null && cr.Description.Contains(searchTerm)))
            .OrderByDescending(cr => cr.LastActivityAt ?? cr.CreatedDate)
            .ToListAsync();
    }
} 