using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories;

public class AppointmentPaymentLogRepository : RepositoryBase<AppointmentPaymentLog>, IAppointmentPaymentLogRepository
{
    private readonly ApplicationDbContext _context;
    public AppointmentPaymentLogRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves all appointment payment logs
    /// </summary>
    public override async Task<IEnumerable<AppointmentPaymentLog>> GetAllAsync()
    {
        return await _context.AppointmentPaymentLogs.ToListAsync();
    }

    /// <summary>
    /// Retrieves an appointment payment log by its unique identifier
    /// </summary>
    public override async Task<AppointmentPaymentLog?> GetByIdAsync(object id)
    {
        if (id is not Guid logId)
            return null;

        return await _context.AppointmentPaymentLogs.FindAsync(logId);
    }

    /// <summary>
    /// Creates a new appointment payment log
    /// </summary>
    public override async Task<AppointmentPaymentLog> CreateAsync(AppointmentPaymentLog entity)
    {
        return await base.CreateAsync(entity);
    }

    /// <summary>
    /// Updates an existing appointment payment log
    /// </summary>
    public override async Task<AppointmentPaymentLog> UpdateAsync(AppointmentPaymentLog entity)
    {
        return await base.UpdateAsync(entity);
    }

    /// <summary>
    /// Deletes an appointment payment log by its unique identifier (hard delete)
    /// </summary>
    public override async Task<bool> DeleteAsync(object id)
    {
        if (id is not Guid logId)
            return false;

        var entity = await _context.AppointmentPaymentLogs.FindAsync(logId);
        if (entity != null)
        {
            _context.AppointmentPaymentLogs.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Checks if an appointment payment log exists
    /// </summary>
    public override async Task<bool> ExistsAsync(object id)
    {
        if (id is not Guid logId)
            return false;

        return await _context.AppointmentPaymentLogs.AnyAsync(x => x.Id == logId);
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.AppointmentPaymentLogs.AnyAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<AppointmentPaymentLog>> GetByAppointmentAsync(Guid appointmentId)
    {
        return await _context.AppointmentPaymentLogs
            .Include(p => p.User)
            .Where(p => p.AppointmentId == appointmentId && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedDate)
            .ToListAsync();
    }

    public async Task<AppointmentPaymentLog?> GetLatestByAppointmentAsync(Guid appointmentId)
    {
        return await _context.AppointmentPaymentLogs
            .Include(p => p.User)
            .Where(p => p.AppointmentId == appointmentId && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedDate)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<AppointmentPaymentLog>> GetByPaymentStatusAsync(Guid statusId)
    {
        return await _context.AppointmentPaymentLogs
            .Include(p => p.User)
            .Where(p => p.PaymentStatusId == statusId && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<AppointmentPaymentLog>> GetByRefundStatusAsync(Guid statusId)
    {
        return await _context.AppointmentPaymentLogs
            .Include(p => p.User)
            .Where(p => p.RefundStatusId == statusId && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedDate)
            .ToListAsync();
    }

    public async Task<AppointmentPaymentLog?> FindByPaymentIntentIdAsync(string paymentIntentId)
    {
        return await _context.AppointmentPaymentLogs
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.PaymentIntentId == paymentIntentId && !p.IsDeleted);
    }

    public async Task<AppointmentPaymentLog?> FindByRefundIdAsync(string refundId)
    {
        return await _context.AppointmentPaymentLogs
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.RefundId == refundId && !p.IsDeleted);
    }

    public async Task<Guid> GetStatusIdByNameAsync(string name)
    {
        return await _context.PaymentStatuses
            .Where(s => s.Name == name)
            .Select(s => s.Id)
            .FirstOrDefaultAsync();
    }
} 