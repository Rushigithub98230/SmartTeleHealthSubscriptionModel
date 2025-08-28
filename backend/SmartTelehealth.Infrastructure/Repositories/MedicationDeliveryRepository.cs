using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories;

public class MedicationDeliveryRepository : IMedicationDeliveryRepository
{
    private readonly ApplicationDbContext _context;
    
    public MedicationDeliveryRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<MedicationDelivery> GetByIdAsync(Guid id)
    {
        return await _context.MedicationDeliveries
            .Include(m => m.User)
            .Include(m => m.Subscription)
            .Include(m => m.TrackingEvents)
            .FirstOrDefaultAsync(m => m.Id == id);
    }
    
    public async Task<IEnumerable<MedicationDelivery>> GetByUserIdAsync(int userId)
    {
        return await _context.MedicationDeliveries
            .Include(m => m.Subscription)
            .Include(m => m.TrackingEvents)
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.CreatedDate)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<MedicationDelivery>> GetBySubscriptionIdAsync(Guid subscriptionId)
    {
        return await _context.MedicationDeliveries
            .Include(m => m.User)
            .Include(m => m.TrackingEvents)
            .Where(m => m.SubscriptionId == subscriptionId)
            .OrderByDescending(m => m.CreatedDate)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<MedicationDelivery>> GetPendingDeliveriesAsync()
    {
        return await _context.MedicationDeliveries
            .Include(m => m.User)
            .Include(m => m.Subscription)
            .Where(m => m.Status == MedicationDelivery.DeliveryStatus.Pending)
            .OrderBy(m => m.CreatedDate)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<MedicationDelivery>> GetShippedDeliveriesAsync()
    {
        return await _context.MedicationDeliveries
            .Include(m => m.User)
            .Include(m => m.Subscription)
            .Include(m => m.TrackingEvents)
            .Where(m => m.Status == MedicationDelivery.DeliveryStatus.Shipped)
            .OrderByDescending(m => m.ShippedAt)
            .ToListAsync();
    }
    
    public async Task<MedicationDelivery> CreateAsync(MedicationDelivery delivery)
    {
        _context.MedicationDeliveries.Add(delivery);
        await _context.SaveChangesAsync();
        return delivery;
    }
    
    public async Task<MedicationDelivery> UpdateAsync(MedicationDelivery delivery)
    {
        _context.MedicationDeliveries.Update(delivery);
        await _context.SaveChangesAsync();
        return delivery;
    }
    
    public async Task<bool> DeleteAsync(Guid id)
    {
        var delivery = await _context.MedicationDeliveries.FindAsync(id);
        if (delivery == null)
            return false;
            
        _context.MedicationDeliveries.Remove(delivery);
        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task<IEnumerable<MedicationDelivery>> GetDeliveriesByStatusAsync(MedicationDelivery.DeliveryStatus status)
    {
        return await _context.MedicationDeliveries
            .Include(m => m.User)
            .Include(m => m.Subscription)
            .Include(m => m.TrackingEvents)
            .Where(m => m.Status == status)
            .OrderByDescending(m => m.CreatedDate)
            .ToListAsync();
    }
    
    public async Task<bool> UpdateDeliveryStatusAsync(Guid id, MedicationDelivery.DeliveryStatus status, string trackingNumber = null)
    {
        var delivery = await _context.MedicationDeliveries.FindAsync(id);
        if (delivery == null)
            return false;
        
        delivery.Status = status;
        delivery.UpdatedDate = DateTime.UtcNow;
        
        if (status == MedicationDelivery.DeliveryStatus.Shipped)
        {
            delivery.ShippedAt = DateTime.UtcNow;
            delivery.TrackingNumber = trackingNumber;
        }
        else if (status == MedicationDelivery.DeliveryStatus.Delivered)
        {
            delivery.DeliveredAt = DateTime.UtcNow;
        }
        
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.MedicationDeliveries.AnyAsync(m => m.Id == id);
    }
    
    public async Task<int> GetPendingDeliveryCountAsync()
    {
        return await _context.MedicationDeliveries
            .CountAsync(m => m.Status == MedicationDelivery.DeliveryStatus.Pending);
    }
    
    public async Task<IEnumerable<MedicationDelivery>> GetDeliveriesByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.MedicationDeliveries
            .Include(m => m.User)
            .Include(m => m.Subscription)
            .Where(m => m.CreatedDate >= startDate && m.CreatedDate <= endDate)
            .OrderByDescending(m => m.CreatedDate)
            .ToListAsync();
    }
    
    public async Task<MedicationDelivery?> GetByTrackingNumberAsync(string trackingNumber)
    {
        return await _context.MedicationDeliveries
            .Include(m => m.User)
            .Include(m => m.Subscription)
            .Include(m => m.TrackingEvents)
            .FirstOrDefaultAsync(m => m.TrackingNumber == trackingNumber);
    }

    public async Task<IEnumerable<MedicationDelivery>> GetByStatusAsync(MedicationDelivery.DeliveryStatus status)
    {
        return await _context.MedicationDeliveries
            .Include(m => m.User)
            .Include(m => m.Subscription)
            .Include(m => m.TrackingEvents)
            .Where(m => m.Status == status)
            .OrderByDescending(m => m.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<MedicationDelivery>> GetAllAsync()
    {
        return await _context.MedicationDeliveries
            .Include(m => m.User)
            .Include(m => m.Subscription)
            .Include(m => m.TrackingEvents)
            .OrderByDescending(m => m.CreatedDate)
            .ToListAsync();
    }
} 