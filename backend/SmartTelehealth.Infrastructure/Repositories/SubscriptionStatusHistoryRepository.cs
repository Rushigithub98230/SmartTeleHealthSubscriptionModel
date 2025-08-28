using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories;

public class SubscriptionStatusHistoryRepository : ISubscriptionStatusHistoryRepository
{
    private readonly ApplicationDbContext _context;

    public SubscriptionStatusHistoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SubscriptionStatusHistory?> GetByIdAsync(Guid id)
    {
        return await _context.SubscriptionStatusHistories
            .Include(h => h.Subscription)
            .FirstOrDefaultAsync(h => h.Id == id && !h.IsDeleted);
    }

    public async Task<IEnumerable<SubscriptionStatusHistory>> GetBySubscriptionIdAsync(Guid subscriptionId)
    {
        return await _context.SubscriptionStatusHistories
            .Include(h => h.Subscription)
            .Where(h => h.SubscriptionId == subscriptionId && !h.IsDeleted)
            .OrderByDescending(h => h.ChangedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<SubscriptionStatusHistory>> GetByStatusAsync(string status)
    {
        return await _context.SubscriptionStatusHistories
            .Include(h => h.Subscription)
            .Where(h => h.ToStatus == status && !h.IsDeleted)
            .OrderByDescending(h => h.ChangedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<SubscriptionStatusHistory>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.SubscriptionStatusHistories
            .Include(h => h.Subscription)
            .Where(h => h.ChangedAt >= startDate && h.ChangedAt <= endDate && !h.IsDeleted)
            .OrderByDescending(h => h.ChangedAt)
            .ToListAsync();
    }

    public async Task<SubscriptionStatusHistory> CreateAsync(SubscriptionStatusHistory history)
    {
        history.CreatedDate = DateTime.UtcNow;
        _context.SubscriptionStatusHistories.Add(history);
        await _context.SaveChangesAsync();
        return history;
    }

    public async Task<SubscriptionStatusHistory> UpdateAsync(SubscriptionStatusHistory history)
    {
        history.UpdatedDate = DateTime.UtcNow;
        _context.SubscriptionStatusHistories.Update(history);
        await _context.SaveChangesAsync();
        return history;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var history = await _context.SubscriptionStatusHistories.FindAsync(id);
        if (history == null) return false;

        history.IsDeleted = true;
        history.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.SubscriptionStatusHistories
            .AnyAsync(h => h.Id == id && !h.IsDeleted);
    }

    public async Task<int> GetCountBySubscriptionIdAsync(Guid subscriptionId)
    {
        return await _context.SubscriptionStatusHistories
            .CountAsync(h => h.SubscriptionId == subscriptionId && !h.IsDeleted);
    }

    public async Task<SubscriptionStatusHistory?> GetLatestBySubscriptionIdAsync(Guid subscriptionId)
    {
        return await _context.SubscriptionStatusHistories
            .Include(h => h.Subscription)
            .Where(h => h.SubscriptionId == subscriptionId && !h.IsDeleted)
            .OrderByDescending(h => h.ChangedAt)
            .FirstOrDefaultAsync();
    }
} 