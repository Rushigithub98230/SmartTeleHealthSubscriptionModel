using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartTelehealth.Infrastructure.Repositories
{
    public class PrescriptionRepository : IPrescriptionRepository
    {
        private readonly ApplicationDbContext _context;
        
        public PrescriptionRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public async Task<Prescription> GetByIdAsync(Guid id)
        {
            return await _context.Prescriptions
                .Include(p => p.Items)
                .Include(p => p.Consultation)
                .Include(p => p.Provider)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted) ?? throw new InvalidOperationException($"Prescription with ID {id} not found");
        }
        
        public async Task<IEnumerable<Prescription>> GetByUserIdAsync(int userId)
        {
            return await _context.Prescriptions
                .Include(p => p.Items)
                .Include(p => p.Consultation)
                .Include(p => p.Provider)
                .Where(p => p.UserId == userId && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<Prescription>> GetByProviderIdAsync(int providerId)
        {
            return await _context.Prescriptions
                .Include(p => p.Items)
                .Include(p => p.Consultation)
                .Include(p => p.User)
                .Where(p => p.ProviderId == providerId && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<Prescription>> GetByStatusAsync(string status)
        {
            return await _context.Prescriptions
                .Include(p => p.Items)
                .Include(p => p.Consultation)
                .Include(p => p.Provider)
                .Include(p => p.User)
                .Where(p => p.Status == status && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();
        }
        
        public async Task<Prescription> CreateAsync(Prescription prescription)
        {
            prescription.CreatedDate = DateTime.UtcNow;
            prescription.UpdatedDate = DateTime.UtcNow;
            
            if (string.IsNullOrEmpty(prescription.Status))
                prescription.Status = "pending";
                
            _context.Prescriptions.Add(prescription);
            await _context.SaveChangesAsync();
            return prescription;
        }
        
        public async Task<Prescription> UpdateAsync(Prescription prescription)
        {
            prescription.UpdatedDate = DateTime.UtcNow;
            _context.Prescriptions.Update(prescription);
            await _context.SaveChangesAsync();
            return prescription;
        }
        
        public async Task<bool> DeleteAsync(Guid id)
        {
            var prescription = await _context.Prescriptions.FindAsync(id);
            if (prescription == null) return false;

            prescription.IsDeleted = true;
            prescription.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<IEnumerable<Prescription>> GetOverduePrescriptionsAsync()
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-30); // Prescriptions older than 30 days
            return await _context.Prescriptions
                .Include(p => p.Items)
                .Include(p => p.Consultation)
                .Include(p => p.Provider)
                .Include(p => p.User)
                .Where(p => p.Status == "pending" && p.CreatedDate < cutoffDate && !p.IsDeleted)
                .OrderBy(p => p.CreatedDate)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<Prescription>> GetRefillRequestsAsync(int userId)
        {
            return await _context.Prescriptions
                .Include(p => p.Items)
                .Include(p => p.Consultation)
                .Include(p => p.Provider)
                .Where(p => p.UserId == userId && p.Status == "refill_requested" && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();
        }
        
        public async Task<int> GetPrescriptionCountAsync(int userId)
        {
            return await _context.Prescriptions
                .CountAsync(p => p.UserId == userId && !p.IsDeleted);
        }
        
        public async Task<decimal> GetPrescriptionTotalAsync(int userId)
        {
            // This would typically calculate the total cost of prescriptions
            // For now, return a placeholder value
            return await _context.Prescriptions
                .Where(p => p.UserId == userId && !p.IsDeleted)
                .CountAsync() * 50.0m; // Placeholder calculation
        }
    }
} 