using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly ApplicationDbContext _context;

    public AppointmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Appointment?> GetByIdAsync(Guid id)
    {
        return await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Provider)
            .Include(a => a.Consultation)
            .Include(a => a.Participants)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<Appointment>> GetAllAsync()
    {
        return await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Provider)
            .Include(a => a.Consultation)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetByPatientIdAsync(int patientId)
    {
        return await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Provider)
            .Include(a => a.Consultation)
            .Where(a => a.PatientId == patientId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetByProviderIdAsync(int providerId)
    {
        return await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Provider)
            .Include(a => a.Consultation)
            .Where(a => a.ProviderId == providerId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Provider)
            .Include(a => a.Consultation)
            .Where(a => a.ScheduledAt >= startDate && a.ScheduledAt <= endDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetByStatusAsync(Guid appointmentStatusId)
    {
        return await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Provider)
            .Include(a => a.Consultation)
            .Include(a => a.AppointmentStatus)
            .Where(a => a.AppointmentStatusId == appointmentStatusId)
            .ToListAsync();
    }

    public async Task<Appointment> AddAsync(Appointment appointment)
    {
        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();
        return appointment;
    }

    public async Task<Appointment> UpdateAsync(Appointment appointment)
    {
        _context.Appointments.Update(appointment);
        await _context.SaveChangesAsync();
        return appointment;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment == null)
            return false;

        _context.Appointments.Remove(appointment);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Appointments.AnyAsync(a => a.Id == id);
    }

    public async Task<int> GetCountAsync()
    {
        return await _context.Appointments.CountAsync();
    }

    public async Task<IEnumerable<Appointment>> GetUpcomingAppointmentsAsync(DateTime fromDate)
    {
        var scheduledStatusId = await GetStatusIdByNameAsync("Scheduled");
        return await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Provider)
            .Include(a => a.Consultation)
            .Include(a => a.AppointmentStatus)
            .Where(a => a.ScheduledAt >= fromDate && a.AppointmentStatusId == scheduledStatusId)
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
            .Where(a => a.ScheduledAt < now && a.AppointmentStatusId == scheduledStatusId)
            .ToListAsync();
    }

    // Additional interface methods
    public async Task<Appointment> CreateAsync(Appointment appointment)
    {
        return await AddAsync(appointment);
    }

    public async Task<IEnumerable<Appointment>> GetByPatientAsync(int patientId)
    {
        return await _context.Appointments
            .Include(a => a.Category)
            .Include(a => a.Patient)
            .Include(a => a.Provider)
            .Include(a => a.Consultation)
            .Where(a => a.PatientId == patientId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetByProviderAsync(int providerId)
    {
        return await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Provider)
            .Include(a => a.Consultation)
            .Where(a => a.ProviderId == providerId)
            .ToListAsync();
    }



    public async Task<IEnumerable<Appointment>> GetUpcomingAsync()
    {
        return await GetUpcomingAppointmentsAsync(DateTime.UtcNow);
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
                       a.AppointmentStatusId == completedStatusId && // Completed status
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
} 