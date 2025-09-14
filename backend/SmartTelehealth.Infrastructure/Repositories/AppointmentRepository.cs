using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories;

public class AppointmentRepository : RepositoryBase<Appointment>, IAppointmentRepository
{
    private readonly ApplicationDbContext _context;

    public AppointmentRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    // Use base class methods for basic CRUD operations
    // GetByIdAsync, GetAllAsync, CreateAsync, UpdateAsync, DeleteAsync, ExistsAsync are inherited from RepositoryBase

    // Override GetByIdAsync to include related data
    public override async Task<Appointment?> GetByIdAsync(object id)
    {
        if (id is Guid guidId)
        {
            return await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Provider)
                .Include(a => a.Consultation)
                .Include(a => a.Participants)
                .FirstOrDefaultAsync(a => a.Id == guidId);
        }
        return null;
    }

    // Override GetAllAsync to include related data
    public override async Task<IEnumerable<Appointment>> GetAllAsync()
    {
        return await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Provider)
            .Include(a => a.Consultation)
            .ToListAsync();
    }

    // Override DeleteAsync to implement soft delete
    public override async Task<bool> DeleteAsync(object id)
    {
        if (id is Guid guidId)
        {
            var appointment = await _context.Appointments.FindAsync(guidId);
            if (appointment == null)
                return false;

            appointment.IsDeleted = true;
            appointment.UpdatedDate = DateTime.UtcNow;
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
            return await _context.Appointments.AnyAsync(a => a.Id == guidId && !a.IsDeleted);
        }
        return false;
    }

    // Specialized methods for Appointment entity
    public async Task<IEnumerable<Appointment>> GetByPatientAsync(int patientId)
    {
        return await _context.Appointments
            .Include(a => a.Category)
            .Include(a => a.Patient)
            .Include(a => a.Provider)
            .Include(a => a.Consultation)
            .Where(a => a.PatientId == patientId && !a.IsDeleted)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetByProviderAsync(int providerId)
    {
        return await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Provider)
            .Include(a => a.Consultation)
            .Where(a => a.ProviderId == providerId && !a.IsDeleted)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetByStatusAsync(Guid appointmentStatusId)
    {
        return await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Provider)
            .Include(a => a.Consultation)
            .Include(a => a.AppointmentStatus)
            .Where(a => a.AppointmentStatusId == appointmentStatusId && !a.IsDeleted)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Provider)
            .Include(a => a.Consultation)
            .Where(a => a.ScheduledAt >= startDate && a.ScheduledAt <= endDate && !a.IsDeleted)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetUpcomingAsync()
    {
        return await GetUpcomingAppointmentsAsync(DateTime.UtcNow);
    }

    public async Task<IEnumerable<Appointment>> GetUpcomingAppointmentsAsync(DateTime fromDate)
    {
        var scheduledStatusId = await GetStatusIdByNameAsync("Scheduled");
        return await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Provider)
            .Include(a => a.Consultation)
            .Include(a => a.AppointmentStatus)
            .Where(a => a.ScheduledAt >= fromDate && a.AppointmentStatusId == scheduledStatusId && !a.IsDeleted)
            .OrderBy(a => a.ScheduledAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetOverdueAppointmentsAsync()
    {
        var now = DateTime.UtcNow;
        var scheduledStatusId = await GetStatusIdByNameAsync("Scheduled");
        return await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Provider)
            .Include(a => a.Consultation)
            .Include(a => a.AppointmentStatus)
            .Where(a => a.ScheduledAt < now && a.AppointmentStatusId == scheduledStatusId && !a.IsDeleted)
            .ToListAsync();
    }

    public async Task<int> GetCountByStatusAsync(Guid statusId)
    {
        return await _context.Appointments
            .CountAsync(a => a.AppointmentStatusId == statusId && !a.IsDeleted);
    }

    public async Task<decimal> GetTotalRevenueAsync(DateTime startDate, DateTime endDate)
    {
        var completedStatusId = await GetStatusIdByNameAsync("Completed");
        return await _context.Appointments
            .Where(a => a.ScheduledAt >= startDate && 
                       a.ScheduledAt <= endDate && 
                       a.AppointmentStatusId == completedStatusId &&
                       !a.IsDeleted)
            .SumAsync(a => a.Fee);
    }

    public async Task<Guid> GetStatusIdByNameAsync(string statusName)
    {
        return await _context.AppointmentStatuses
            .Where(s => s.Name == statusName)
            .Select(s => s.Id)
            .FirstAsync();
    }

    // Legacy methods for backward compatibility
    public async Task<Appointment> AddAsync(Appointment appointment)
    {
        return await CreateAsync(appointment);
    }

    public async Task<int> GetCountAsync()
    {
        return await _context.Appointments.CountAsync(a => !a.IsDeleted);
    }
} 