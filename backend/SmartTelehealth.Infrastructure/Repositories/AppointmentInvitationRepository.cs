using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories;

public class AppointmentInvitationRepository : RepositoryBase<AppointmentInvitation>, IAppointmentInvitationRepository
{
    private readonly ApplicationDbContext _context;
    public AppointmentInvitationRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves all appointment invitations
    /// </summary>
    public override async Task<IEnumerable<AppointmentInvitation>> GetAllAsync()
    {
        return await _context.AppointmentInvitations.ToListAsync();
    }

    /// <summary>
    /// Retrieves an appointment invitation by its unique identifier
    /// </summary>
    public override async Task<AppointmentInvitation?> GetByIdAsync(object id)
    {
        if (id is not Guid invitationId)
            return null;

        return await _context.AppointmentInvitations.FindAsync(invitationId);
    }

    /// <summary>
    /// Creates a new appointment invitation
    /// </summary>
    public override async Task<AppointmentInvitation> CreateAsync(AppointmentInvitation entity)
    {
        entity.CreatedDate = DateTime.UtcNow;
        return await base.CreateAsync(entity);
    }

    /// <summary>
    /// Updates an existing appointment invitation
    /// </summary>
    public override async Task<AppointmentInvitation> UpdateAsync(AppointmentInvitation entity)
    {
        entity.UpdatedDate = DateTime.UtcNow;
        return await base.UpdateAsync(entity);
    }

    /// <summary>
    /// Deletes an appointment invitation by its unique identifier (hard delete)
    /// </summary>
    public override async Task<bool> DeleteAsync(object id)
    {
        if (id is not Guid invitationId)
            return false;

        var entity = await _context.AppointmentInvitations.FindAsync(invitationId);
        if (entity != null)
        {
            _context.AppointmentInvitations.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Checks if an appointment invitation exists
    /// </summary>
    public override async Task<bool> ExistsAsync(object id)
    {
        if (id is not Guid invitationId)
            return false;

        return await _context.AppointmentInvitations.AnyAsync(x => x.Id == invitationId);
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