using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartTelehealth.Infrastructure.Repositories
{
    public class MedicationShipmentRepository : IMedicationShipmentRepository
    {
        private readonly ApplicationDbContext _context;
        
        public MedicationShipmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public async Task<MedicationDelivery?> GetByIdAsync(Guid id)
        {
            return await _context.MedicationDeliveries
                .Include(m => m.User)
                .Include(m => m.Subscription)
                .Include(m => m.Provider)
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);
        }
        
        public async Task<IEnumerable<MedicationDelivery>> GetByUserIdAsync(int userId)
        {
            return await _context.MedicationDeliveries
                .Include(m => m.Subscription)
                .Include(m => m.Provider)
                .Where(m => m.UserId == userId && !m.IsDeleted)
                .OrderByDescending(m => m.CreatedDate)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<MedicationDelivery>> GetByStatusAsync(string status)
        {
            // Parse the string status to enum
            if (!Enum.TryParse<MedicationDelivery.DeliveryStatus>(status, true, out var statusEnum))
            {
                return new List<MedicationDelivery>();
            }
            
            return await _context.MedicationDeliveries
                .Include(m => m.User)
                .Include(m => m.Subscription)
                .Include(m => m.Provider)
                .Where(m => m.Status == statusEnum && !m.IsDeleted)
                .OrderByDescending(m => m.CreatedDate)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<MedicationDelivery>> GetByTrackingNumberAsync(string trackingNumber)
        {
            return await _context.MedicationDeliveries
                .Include(m => m.User)
                .Include(m => m.Subscription)
                .Include(m => m.Provider)
                .Where(m => m.TrackingNumber == trackingNumber && !m.IsDeleted)
                .OrderByDescending(m => m.CreatedDate)
                .ToListAsync();
        }
        
        public async Task<MedicationDelivery> CreateAsync(MedicationDelivery shipment)
        {
            shipment.CreatedDate = DateTime.UtcNow;
            shipment.UpdatedDate = DateTime.UtcNow;
            
            if (shipment.Status == 0) // Default enum value
                shipment.Status = MedicationDelivery.DeliveryStatus.Pending;
                
            _context.MedicationDeliveries.Add(shipment);
            await _context.SaveChangesAsync();
            return shipment;
        }
        
        public async Task<MedicationDelivery> UpdateAsync(MedicationDelivery shipment)
        {
            shipment.UpdatedDate = DateTime.UtcNow;
            _context.MedicationDeliveries.Update(shipment);
            await _context.SaveChangesAsync();
            return shipment;
        }
        
        public async Task<bool> DeleteAsync(Guid id)
        {
            var shipment = await _context.MedicationDeliveries.FindAsync(id);
            if (shipment == null) return false;

            shipment.IsDeleted = true;
            shipment.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<IEnumerable<MedicationDelivery>> GetOverdueShipmentsAsync()
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-7); // Shipments older than 7 days
            return await _context.MedicationDeliveries
                .Include(m => m.User)
                .Include(m => m.Subscription)
                .Include(m => m.Provider)
                .Where(m => m.Status == MedicationDelivery.DeliveryStatus.Pending && m.CreatedDate < cutoffDate && !m.IsDeleted)
                .OrderBy(m => m.CreatedDate)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<MedicationDelivery>> GetShipmentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.MedicationDeliveries
                .Include(m => m.User)
                .Include(m => m.Subscription)
                .Include(m => m.Provider)
                .Where(m => m.CreatedDate >= startDate && m.CreatedDate <= endDate && !m.IsDeleted)
                .OrderByDescending(m => m.CreatedDate)
                .ToListAsync();
        }
        
        public async Task<int> GetShipmentCountAsync(int userId)
        {
            return await _context.MedicationDeliveries
                .CountAsync(m => m.UserId == userId && !m.IsDeleted);
        }

        public async Task<decimal> GetShipmentTotalAsync(int userId)
        {
            // This would typically calculate the total cost of shipments
            // For now, return a placeholder value
            return await _context.MedicationDeliveries
                .Where(m => m.UserId == userId && !m.IsDeleted)
                .CountAsync() * 25.0m; // Placeholder calculation
        }
    }
} 