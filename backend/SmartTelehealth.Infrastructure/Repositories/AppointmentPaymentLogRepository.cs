using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories;

public class AppointmentPaymentLogRepository : IAppointmentPaymentLogRepository
{
    private readonly ApplicationDbContext _context;
    public AppointmentPaymentLogRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AppointmentPaymentLog>> GetAllAsync()
    {
        return await _context.AppointmentPaymentLogs.ToListAsync();
    }

    public async Task<AppointmentPaymentLog?> GetByIdAsync(Guid id)
    {
        return await _context.AppointmentPaymentLogs.FindAsync(id);
    }

    public async Task<AppointmentPaymentLog> CreateAsync(AppointmentPaymentLog entity)
    {
        _context.AppointmentPaymentLogs.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<AppointmentPaymentLog> UpdateAsync(AppointmentPaymentLog entity)
    {
        _context.AppointmentPaymentLogs.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _context.AppointmentPaymentLogs.FindAsync(id);
        if (entity != null)
        {
            _context.AppointmentPaymentLogs.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
        return false;
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