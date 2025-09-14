using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories;

public class AppointmentParticipantRepository : RepositoryBase<AppointmentParticipant>, IAppointmentParticipantRepository
{
    private readonly ApplicationDbContext _context;
    public AppointmentParticipantRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    // Use base class methods for basic CRUD operations
    // GetByIdAsync, GetAllAsync, CreateAsync, UpdateAsync, DeleteAsync, ExistsAsync are inherited from RepositoryBase

    // Override GetByIdAsync to handle Guid ID type
    public override async Task<AppointmentParticipant?> GetByIdAsync(object id)
    {
        if (id is Guid guidId)
        {
            return await _context.AppointmentParticipants.FindAsync(guidId);
        }
        return null;
    }

    // Override DeleteAsync to handle Guid ID type
    public override async Task<bool> DeleteAsync(object id)
    {
        if (id is Guid guidId)
        {
            var entity = await _context.AppointmentParticipants.FindAsync(guidId);
            if (entity != null)
            {
                _context.AppointmentParticipants.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
        }
        return false;
    }

    // Override ExistsAsync to handle Guid ID type
    public override async Task<bool> ExistsAsync(object id)
    {
        if (id is Guid guidId)
        {
            return await _context.AppointmentParticipants.AnyAsync(e => e.Id == guidId);
        }
        return false;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.AppointmentParticipants.AnyAsync(e => e.Id == id);
    }

    public async Task<AppointmentParticipant?> FindByAppointmentAndUserOrEmailAsync(Guid appointmentId, int? userId, string? email)
    {
        return await _context.AppointmentParticipants
            .Include(p => p.User)
            .Include(p => p.InvitedByUser)
            .FirstOrDefaultAsync(p => p.AppointmentId == appointmentId && 
                                    (p.UserId == userId || p.ExternalEmail == email));
    }

    public async Task<IEnumerable<AppointmentParticipant>> GetByAppointmentAsync(Guid appointmentId)
    {
        return await _context.AppointmentParticipants
            .Include(p => p.User)
            .Include(p => p.InvitedByUser)
            .Where(p => p.AppointmentId == appointmentId && !p.IsDeleted)
            .ToListAsync();
    }

    public async Task<int> CountByAppointmentAsync(Guid appointmentId)
    {
        return await _context.AppointmentParticipants
            .CountAsync(p => p.AppointmentId == appointmentId && !p.IsDeleted);
    }

    public async Task<IEnumerable<AppointmentParticipant>> GetActiveParticipantsAsync(Guid appointmentId)
    {
        return await _context.AppointmentParticipants
            .Include(p => p.User)
            .Where(p => p.AppointmentId == appointmentId && 
                       p.ParticipantStatusId == _context.ParticipantStatuses.First(s => s.Name == "Joined").Id && // Confirmed status ID
                       !p.IsDeleted)
            .ToListAsync();
    }

    public async Task<AppointmentParticipant?> GetByUserAndAppointmentAsync(int userId, Guid appointmentId)
    {
        return await _context.AppointmentParticipants
            .Include(p => p.User)
            .Include(p => p.InvitedByUser)
            .FirstOrDefaultAsync(p => p.UserId == userId && 
                                    p.AppointmentId == appointmentId && 
                                    !p.IsDeleted);
    }

    public async Task<IEnumerable<AppointmentParticipant>> GetByUserAsync(int userId)
    {
        return await _context.AppointmentParticipants
            .Include(p => p.User)
            .Where(p => p.UserId == userId)
            .ToListAsync();
    }

    public async Task<AppointmentParticipant?> GetByAppointmentAndUserAsync(Guid appointmentId, int? userId)
    {
        return await _context.AppointmentParticipants
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.AppointmentId == appointmentId && p.UserId == userId);
    }

    public async Task<Guid> GetStatusIdByNameAsync(string name)
    {
        return await _context.ParticipantStatuses
            .Where(s => s.Name == name)
            .Select(s => s.Id)
            .FirstOrDefaultAsync();
    }
} 