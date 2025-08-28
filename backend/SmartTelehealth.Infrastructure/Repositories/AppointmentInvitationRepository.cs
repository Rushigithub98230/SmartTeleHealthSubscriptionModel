using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories;

public class AppointmentInvitationRepository : IAppointmentInvitationRepository
{
    private readonly ApplicationDbContext _context;
    public AppointmentInvitationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AppointmentInvitation>> GetAllAsync()
    {
        return await _context.AppointmentInvitations.ToListAsync();
    }

    public async Task<AppointmentInvitation?> GetByIdAsync(Guid id)
    {
        return await _context.AppointmentInvitations.FindAsync(id);
    }

    public async Task<AppointmentInvitation> CreateAsync(AppointmentInvitation entity)
    {
        _context.AppointmentInvitations.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<AppointmentInvitation> UpdateAsync(AppointmentInvitation entity)
    {
        _context.AppointmentInvitations.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _context.AppointmentInvitations.FindAsync(id);
        if (entity != null)
        {
            _context.AppointmentInvitations.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
        return false;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.AppointmentInvitations.AnyAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<AppointmentInvitation>> GetByAppointmentAsync(Guid appointmentId)
    {
        return await _context.AppointmentInvitations
            .Include(i => i.InvitedByUser)
            .Include(i => i.InvitedUser)
            .Where(i => i.AppointmentId == appointmentId)
            .ToListAsync();
    }

    public async Task<AppointmentInvitation?> FindByEmailAndAppointmentAsync(string email, Guid appointmentId)
    {
        return await _context.AppointmentInvitations
            .Include(i => i.InvitedByUser)
            .Include(i => i.InvitedUser)
            .FirstOrDefaultAsync(i => i.InvitedEmail == email && i.AppointmentId == appointmentId);
    }

    public async Task<AppointmentInvitation?> FindByPhoneAndAppointmentAsync(string phone, Guid appointmentId)
    {
        return await _context.AppointmentInvitations
            .Include(i => i.InvitedByUser)
            .Include(i => i.InvitedUser)
            .FirstOrDefaultAsync(i => i.InvitedPhone == phone && i.AppointmentId == appointmentId);
    }

    public async Task<IEnumerable<AppointmentInvitation>> GetPendingInvitationsAsync(Guid appointmentId)
    {
        // Fix for async lambda in LINQ: fetch statusId before query
        var pendingStatusId = await _context.InvitationStatuses
            .Where(s => s.Name == "Pending")
            .Select(s => s.Id)
            .FirstOrDefaultAsync();
        var invitations = await _context.AppointmentInvitations
            .Include(i => i.InvitedByUser)
            .Include(i => i.InvitedUser)
            .Where(i => i.AppointmentId == appointmentId &&
                         i.InvitationStatusId == pendingStatusId &&
                         i.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();
        return invitations;
    }

    public async Task<IEnumerable<AppointmentInvitation>> GetExpiredInvitationsAsync()
    {
        var expiredStatusId = await _context.InvitationStatuses
            .Where(s => s.Name == "Pending")
            .Select(s => s.Id)
            .FirstOrDefaultAsync();
        var expiredInvitations = await _context.AppointmentInvitations
            .Include(i => i.InvitedByUser)
            .Include(i => i.InvitedUser)
            .Where(i => i.InvitationStatusId == expiredStatusId &&
                         i.ExpiresAt <= DateTime.UtcNow)
            .ToListAsync();
        return expiredInvitations;
    }

    public async Task<IEnumerable<AppointmentInvitation>> GetByInviteeAsync(int inviteeId)
    {
        return await _context.AppointmentInvitations
            .Include(i => i.InvitedByUser)
            .Include(i => i.InvitedUser)
            .Where(i => i.InvitedUserId == inviteeId)
            .ToListAsync();
    }

    public async Task<AppointmentInvitation?> GetByTokenAsync(string token)
    {
        // Since the entity doesn't have a Token property, return null
        // This method would need to be implemented if Token property is added to the entity
        return null;
    }

    public async Task<Guid> GetStatusIdByNameAsync(string name)
    {
        return await _context.InvitationStatuses
            .Where(s => s.Name == name)
            .Select(s => s.Id)
            .FirstOrDefaultAsync();
    }
} 