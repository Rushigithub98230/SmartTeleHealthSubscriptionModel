using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace SmartTelehealth.Infrastructure.Repositories
{
    public class BillingRepository : RepositoryBase<BillingRecord>, IBillingRepository
    {
        private readonly ApplicationDbContext _context;

        public BillingRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<BillingRecord?> GetByIdAsync(Guid id)
        {
            return await _context.BillingRecords
                .Include(b => b.User)
                .Include(b => b.Subscription)
                .Include(b => b.Currency)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<IEnumerable<BillingRecord>> GetByUserIdAsync(int userId)
        {
            return await _context.BillingRecords
                .Include(b => b.Subscription)
                .Include(b => b.Currency)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.BillingDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<BillingRecord>> GetBySubscriptionIdAsync(Guid subscriptionId)
        {
            return await _context.BillingRecords
                .Include(b => b.User)
                .Include(b => b.Currency)
                .Where(b => b.SubscriptionId == subscriptionId)
                .OrderByDescending(b => b.BillingDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<BillingRecord>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.BillingRecords
                .Include(b => b.User)
                .Include(b => b.Subscription)
                .Include(b => b.Currency)
                .Where(b => b.BillingDate >= startDate && b.BillingDate <= endDate)
                .OrderByDescending(b => b.BillingDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<BillingRecord>> GetByStatusAsync(BillingRecord.BillingStatus status)
        {
            return await _context.BillingRecords
                .Include(b => b.User)
                .Include(b => b.Subscription)
                .Include(b => b.Currency)
                .Where(b => b.Status == status)
                .OrderByDescending(b => b.BillingDate)
                .ToListAsync();
        }

        public async Task<BillingRecord> CreateAsync(BillingRecord billingRecord)
        {
            _context.BillingRecords.Add(billingRecord);
            await _context.SaveChangesAsync();
            return billingRecord;
        }

        public async Task<BillingRecord> UpdateAsync(BillingRecord billingRecord)
        {
            _context.BillingRecords.Update(billingRecord);
            await _context.SaveChangesAsync();
            return billingRecord;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var billingRecord = await _context.BillingRecords.FindAsync(id);
            if (billingRecord == null)
                return false;

            _context.BillingRecords.Remove(billingRecord);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.BillingRecords.AnyAsync(b => b.Id == id);
        }

        public async Task<BillingRecord?> GetByInvoiceNumberAsync(string invoiceNumber)
        {
            return await _context.BillingRecords
                .Include(b => b.User)
                .Include(b => b.Subscription)
                .Include(b => b.Currency)
                .FirstOrDefaultAsync(b => b.InvoiceNumber == invoiceNumber);
        }

        public async Task<IEnumerable<BillingRecord>> GetInvoicesByUserIdAsync(int userId, int page, int pageSize)
        {
            return await _context.BillingRecords
                .Include(b => b.Subscription)
                .Include(b => b.Currency)
                .Where(b => b.UserId == userId && !string.IsNullOrEmpty(b.InvoiceNumber))
                .OrderByDescending(b => b.BillingDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetInvoiceCountByUserIdAsync(int userId)
        {
            return await _context.BillingRecords
                .Where(b => b.UserId == userId && !string.IsNullOrEmpty(b.InvoiceNumber))
                .CountAsync();
        }

        public async Task<IEnumerable<BillingRecord>> GetBillingRecordsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.BillingRecords
                .Include(b => b.User)
                .Include(b => b.Subscription)
                .Include(b => b.Currency)
                .Where(b => b.BillingDate >= startDate && b.BillingDate <= endDate)
                .OrderByDescending(b => b.BillingDate)
                .ToListAsync();
        }

        // Additional methods needed by BillingService
        public async Task<IEnumerable<BillingAdjustment>> GetAdjustmentsByBillingRecordIdAsync(Guid billingRecordId)
        {
            return await _context.BillingAdjustments
                .Where(ba => ba.BillingRecordId == billingRecordId)
                .ToListAsync();
        }

        public async Task<IEnumerable<BillingRecord>> GetByBillingCycleIdAsync(Guid billingCycleId)
        {
            return await _context.BillingRecords
                .Include(b => b.User)
                .Include(b => b.Subscription)
                .Include(b => b.Currency)
                .Where(b => b.BillingCycleId == billingCycleId)
                .OrderByDescending(b => b.BillingDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<BillingRecord>> GetOverdueRecordsAsync()
        {
            return await _context.BillingRecords
                .Include(b => b.User)
                .Include(b => b.Subscription)
                .Include(b => b.Currency)
                .Where(b => b.Status == BillingRecord.BillingStatus.Pending && b.DueDate < DateTime.UtcNow)
                .OrderBy(b => b.DueDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<BillingRecord>> GetPendingRecordsAsync()
        {
            return await _context.BillingRecords
                .Include(b => b.User)
                .Include(b => b.Subscription)
                .Include(b => b.Currency)
                .Where(b => b.Status == BillingRecord.BillingStatus.Pending)
                .OrderBy(b => b.DueDate)
                .ToListAsync();
        }
    }
} 