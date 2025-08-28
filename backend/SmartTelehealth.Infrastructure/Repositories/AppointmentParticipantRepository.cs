using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories;

public class AppointmentParticipantRepository : IAppointmentParticipantRepository
{
    private readonly ApplicationDbContext _context;
    public AppointmentParticipantRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AppointmentParticipant>> GetAllAsync()
    {
        return await _context.AppointmentParticipants.ToListAsync();
    }

    public async Task<AppointmentParticipant?> GetByIdAsync(Guid id)
    {
        return await _context.AppointmentParticipants.FindAsync(id);
    }

    public async Task<AppointmentParticipant> CreateAsync(AppointmentParticipant entity)
    {
        _context.AppointmentParticipants.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<AppointmentParticipant> UpdateAsync(AppointmentParticipant entity)
    {
        _context.AppointmentParticipants.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _context.AppointmentParticipants.FindAsync(id);
        if (entity != null)
        {
            _context.AppointmentParticipants.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
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