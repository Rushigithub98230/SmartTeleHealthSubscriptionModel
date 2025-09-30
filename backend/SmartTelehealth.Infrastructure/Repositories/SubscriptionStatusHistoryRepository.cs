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
        return await base.CreateAsync(history);
    }

    /// <summary>
    /// Updates an existing subscription status history
    /// </summary>
    public override async Task<SubscriptionStatusHistory> UpdateAsync(SubscriptionStatusHistory history)
    {
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

    /// <summary>
    /// Retrieves subscription status history with database-level filtering, pagination, and sorting
    /// </summary>
    public async Task<(IEnumerable<SubscriptionStatusHistory> History, int TotalCount)> GetHistoryWithFilteringAsync(
        int page, int pageSize, Guid? subscriptionId = null, string? status = null, 
        string? search = null, DateTime? startDate = null, DateTime? endDate = null, 
        string? sortBy = "ChangedAt", string? sortOrder = "desc")
    {
        var query = _context.SubscriptionStatusHistories
            .Include(h => h.Subscription)
                .ThenInclude(s => s.User)
            .Where(h => !h.IsDeleted)
            .AsQueryable();

        // Apply filters
        if (subscriptionId.HasValue)
        {
            query = query.Where(h => h.SubscriptionId == subscriptionId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(h => h.ToStatus == status);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(h =>
                h.FromStatus.ToLower().Contains(term) ||
                h.ToStatus.ToLower().Contains(term) ||
                h.Reason.ToLower().Contains(term) ||
                h.Subscription.User.Email.ToLower().Contains(term));
        }

        if (startDate.HasValue)
        {
            query = query.Where(h => h.ChangedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(h => h.ChangedAt <= endDate.Value);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply sorting
        query = ApplySorting(query, sortBy, sortOrder);

        // Apply pagination
        var skip = (page - 1) * pageSize;
        var history = await query
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();

        return (history, totalCount);
    }

    private static IQueryable<SubscriptionStatusHistory> ApplySorting(IQueryable<SubscriptionStatusHistory> query, string sortBy, string sortOrder)
    {
        return sortBy.ToLower() switch
        {
            "fromstatus" => sortOrder.ToLower() == "desc"
                ? query.OrderByDescending(h => h.FromStatus)
                : query.OrderBy(h => h.FromStatus),
            "tostatus" => sortOrder.ToLower() == "desc"
                ? query.OrderByDescending(h => h.ToStatus)
                : query.OrderBy(h => h.ToStatus),
            "changedat" => sortOrder.ToLower() == "desc"
                ? query.OrderByDescending(h => h.ChangedAt)
                : query.OrderBy(h => h.ChangedAt),
            "createddate" => sortOrder.ToLower() == "desc"
                ? query.OrderByDescending(h => h.CreatedDate)
                : query.OrderBy(h => h.CreatedDate),
            "updateddate" => sortOrder.ToLower() == "desc"
                ? query.OrderByDescending(h => h.UpdatedDate)
                : query.OrderBy(h => h.UpdatedDate),
            _ => query.OrderByDescending(h => h.ChangedAt)
        };
    }
} 