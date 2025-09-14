using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories;

public class SubscriptionStatusHistoryRepository : RepositoryBase<SubscriptionStatusHistory>, ISubscriptionStatusHistoryRepository
{
    private readonly ApplicationDbContext _context;

    public SubscriptionStatusHistoryRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves a subscription status history by its unique identifier with related entities
    /// </summary>
    public override async Task<SubscriptionStatusHistory?> GetByIdAsync(object id)
    {
        if (id is not Guid historyId)
            return null;

        return await _context.SubscriptionStatusHistories
            .Include(h => h.Subscription)
            .FirstOrDefaultAsync(h => h.Id == historyId && !h.IsDeleted);
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

    /// <summary>
    /// Retrieves all subscription status histories with related entities
    /// </summary>
    public override async Task<IEnumerable<SubscriptionStatusHistory>> GetAllAsync()
    {
        return await _context.SubscriptionStatusHistories
            .Include(h => h.Subscription)
            .Where(h => !h.IsDeleted)
            .OrderByDescending(h => h.ChangedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Creates a new subscription status history
    /// </summary>
    public override async Task<SubscriptionStatusHistory> CreateAsync(SubscriptionStatusHistory history)
    {
        history.CreatedDate = DateTime.UtcNow;
        return await base.CreateAsync(history);
    }

    /// <summary>
    /// Updates an existing subscription status history
    /// </summary>
    public override async Task<SubscriptionStatusHistory> UpdateAsync(SubscriptionStatusHistory history)
    {
        history.UpdatedDate = DateTime.UtcNow;
        return await base.UpdateAsync(history);
    }

    /// <summary>
    /// Deletes a subscription status history by its unique identifier (soft delete)
    /// </summary>
    public override async Task<bool> DeleteAsync(object id)
    {
        if (id is not Guid historyId)
            return false;

        var history = await _context.SubscriptionStatusHistories.FindAsync(historyId);
        if (history == null) return false;

        history.IsDeleted = true;
        history.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Checks if a subscription status history exists
    /// </summary>
    public override async Task<bool> ExistsAsync(object id)
    {
        if (id is not Guid historyId)
            return false;

        return await _context.SubscriptionStatusHistories
            .AnyAsync(h => h.Id == historyId && !h.IsDeleted);
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