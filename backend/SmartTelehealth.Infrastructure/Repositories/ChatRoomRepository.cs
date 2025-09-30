using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories;

public class ChatRoomRepository : RepositoryBase<ChatRoom>, IChatRoomRepository
{
    private readonly ApplicationDbContext _context;

    public ChatRoomRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    // Use base class methods for basic CRUD operations
    // GetByIdAsync, GetAllAsync, CreateAsync, UpdateAsync, DeleteAsync, ExistsAsync are inherited from RepositoryBase

    // Override GetByIdAsync to include related data and apply business logic
    public override async Task<ChatRoom?> GetByIdAsync(object id)
    {
        if (id is Guid guidId)
        {
            return await _context.ChatRooms
                .Include(cr => cr.Patient)
                .Include(cr => cr.Provider)
                .Include(cr => cr.Subscription)
                .Include(cr => cr.Consultation)
                .Include(cr => cr.Participants)
                .Include(cr => cr.Messages)
                .FirstOrDefaultAsync(cr => cr.Id == guidId && !cr.IsDeleted);
        }
        return null;
    }

    // Override GetAllAsync to include related data and apply business logic
    public override async Task<IEnumerable<ChatRoom>> GetAllAsync()
    {
        return await _context.ChatRooms
            .Include(cr => cr.Patient)
            .Include(cr => cr.Provider)
            .Include(cr => cr.Subscription)
            .Include(cr => cr.Consultation)
            .Where(cr => !cr.IsDeleted)
            .ToListAsync();
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

    // Override CreateAsync to add audit fields
    public override async Task<ChatRoom> CreateAsync(ChatRoom chatRoom)
    {
        return await base.CreateAsync(chatRoom);
    }

    // Override UpdateAsync to add audit fields
    public override async Task<ChatRoom> UpdateAsync(ChatRoom chatRoom)
    {
        return await base.UpdateAsync(chatRoom);
    }

    // Override DeleteAsync to implement soft delete
    public override async Task<bool> DeleteAsync(object id)
    {
        if (id is Guid guidId)
        {
            var chatRoom = await _context.ChatRooms.FindAsync(guidId);
            if (chatRoom == null)
                return false;

            chatRoom.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }
        return false;
    }

    // Override ExistsAsync to apply business logic
    public override async Task<bool> ExistsAsync(object id)
    {
        if (id is Guid guidId)
        {
            return await _context.ChatRooms.AnyAsync(cr => cr.Id == guidId && !cr.IsDeleted);
        }
        return false;
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