using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories;

public class BillingAdjustmentRepository : RepositoryBase<BillingAdjustment>, IBillingAdjustmentRepository
{
    private readonly ApplicationDbContext _context;

    public BillingAdjustmentRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves a billing adjustment by its unique identifier with related entities
    /// </summary>
    public override async Task<BillingAdjustment?> GetByIdAsync(object id)
    {
        if (id is not Guid adjustmentId)
            return null;

        return await _context.BillingAdjustments
            .Include(ba => ba.BillingRecord)
            .FirstOrDefaultAsync(ba => ba.Id == adjustmentId);
    }

    public async Task<IEnumerable<BillingAdjustment>> GetByBillingRecordIdAsync(Guid billingRecordId)
    {
        return await _context.BillingAdjustments
            .Include(ba => ba.BillingRecord)
            .Where(ba => ba.BillingRecordId == billingRecordId)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves all billing adjustments with related entities
    /// </summary>
    public override async Task<IEnumerable<BillingAdjustment>> GetAllAsync()
    {
        return await _context.BillingAdjustments
            .Include(ba => ba.BillingRecord)
            .ToListAsync();
    }

    /// <summary>
    /// Creates a new billing adjustment
    /// </summary>
    public override async Task<BillingAdjustment> CreateAsync(BillingAdjustment billingAdjustment)
    {
        billingAdjustment.CreatedDate = DateTime.UtcNow;
        return await base.CreateAsync(billingAdjustment);
    }

    /// <summary>
    /// Updates an existing billing adjustment
    /// </summary>
    public override async Task<BillingAdjustment> UpdateAsync(BillingAdjustment billingAdjustment)
    {
        billingAdjustment.UpdatedDate = DateTime.UtcNow;
        return await base.UpdateAsync(billingAdjustment);
    }

    /// <summary>
    /// Deletes a billing adjustment by its unique identifier (hard delete)
    /// </summary>
    public override async Task<bool> DeleteAsync(object id)
    {
        if (id is not Guid adjustmentId)
            return false;

        var billingAdjustment = await GetByIdAsync(adjustmentId);
        if (billingAdjustment == null)
            return false;

        _context.BillingAdjustments.Remove(billingAdjustment);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Checks if a billing adjustment exists
    /// </summary>
    public override async Task<bool> ExistsAsync(object id)
    {
        if (id is not Guid adjustmentId)
            return false;

        return await _context.BillingAdjustments.AnyAsync(ba => ba.Id == adjustmentId);
    }

    /// <summary>
    /// Retrieves billing adjustments with database-level filtering, pagination, and sorting
    /// </summary>
    public async Task<(IEnumerable<BillingAdjustment> Adjustments, int TotalCount)> GetAdjustmentsWithFilteringAsync(
        int page, int pageSize, Guid? billingRecordId = null, string? type = null, 
        string? search = null, DateTime? startDate = null, DateTime? endDate = null, 
        string? sortBy = "CreatedDate", string? sortOrder = "desc")
    {
        var query = _context.BillingAdjustments
            .Include(ba => ba.BillingRecord)
                .ThenInclude(br => br.User)
            .AsQueryable();

        // Apply filters
        if (billingRecordId.HasValue)
        {
            query = query.Where(ba => ba.BillingRecordId == billingRecordId.Value);
        }

           if (!string.IsNullOrWhiteSpace(type))
           {
               if (Enum.TryParse<BillingAdjustment.AdjustmentType>(type, out var adjustmentType))
               {
                   query = query.Where(ba => ba.Type == adjustmentType);
               }
           }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(ba =>
                (ba.Reason != null && ba.Reason.ToLower().Contains(term)) ||
                ba.Type.ToString().ToLower().Contains(term) ||
                (ba.ApprovalNotes != null && ba.ApprovalNotes.ToLower().Contains(term)) ||
                ba.BillingRecord.User.Email.ToLower().Contains(term));
        }

        if (startDate.HasValue)
        {
            query = query.Where(ba => ba.CreatedDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(ba => ba.CreatedDate <= endDate.Value);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply sorting
        query = ApplySorting(query, sortBy, sortOrder);

        // Apply pagination
        var skip = (page - 1) * pageSize;
        var adjustments = await query
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();

        return (adjustments, totalCount);
    }

    private static IQueryable<BillingAdjustment> ApplySorting(IQueryable<BillingAdjustment> query, string sortBy, string sortOrder)
    {
        return sortBy.ToLower() switch
        {
            "amount" => sortOrder.ToLower() == "desc"
                ? query.OrderByDescending(ba => ba.Amount)
                : query.OrderBy(ba => ba.Amount),
            "type" => sortOrder.ToLower() == "desc"
                ? query.OrderByDescending(ba => ba.Type)
                : query.OrderBy(ba => ba.Type),
            "reason" => sortOrder.ToLower() == "desc"
                ? query.OrderByDescending(ba => ba.Reason)
                : query.OrderBy(ba => ba.Reason),
            "createddate" => sortOrder.ToLower() == "desc"
                ? query.OrderByDescending(ba => ba.CreatedDate)
                : query.OrderBy(ba => ba.CreatedDate),
            "updateddate" => sortOrder.ToLower() == "desc"
                ? query.OrderByDescending(ba => ba.UpdatedDate)
                : query.OrderBy(ba => ba.UpdatedDate),
            _ => query.OrderByDescending(ba => ba.CreatedDate)
        };
    }
} 