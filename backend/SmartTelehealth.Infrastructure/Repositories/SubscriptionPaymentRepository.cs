using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories;

public class SubscriptionPaymentRepository : RepositoryBase<SubscriptionPayment>, ISubscriptionPaymentRepository
{
    private readonly ApplicationDbContext _context;

    public SubscriptionPaymentRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves a subscription payment by its unique identifier with related entities
    /// </summary>
    public override async Task<SubscriptionPayment?> GetByIdAsync(object id)
    {
        if (id is not Guid paymentId)
            return null;

        return await _context.SubscriptionPayments
            .Include(sp => sp.Subscription)
            .FirstOrDefaultAsync(sp => sp.Id == paymentId);
    }

    public async Task<IEnumerable<SubscriptionPayment>> GetBySubscriptionIdAsync(Guid subscriptionId)
    {
        return await _context.SubscriptionPayments
            .Include(sp => sp.Subscription)
            .Where(sp => sp.SubscriptionId == subscriptionId)
            .OrderByDescending(sp => sp.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<SubscriptionPayment>> GetByUserIdAsync(int userId)
    {
        return await _context.SubscriptionPayments
            .Include(sp => sp.Subscription)
            .Where(sp => sp.Subscription.UserId == userId)
            .OrderByDescending(sp => sp.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<SubscriptionPayment>> GetByStatusAsync(SubscriptionPayment.PaymentStatus status)
    {
        return await _context.SubscriptionPayments
            .Include(sp => sp.Subscription)
            .Where(sp => sp.Status == status)
            .OrderByDescending(sp => sp.CreatedDate)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves all subscription payments with related entities
    /// </summary>
    public override async Task<IEnumerable<SubscriptionPayment>> GetAllAsync()
    {
        return await _context.SubscriptionPayments
            .Include(sp => sp.Subscription)
            .OrderByDescending(sp => sp.CreatedDate)
            .ToListAsync();
    }

    /// <summary>
    /// Creates a new subscription payment
    /// </summary>
    public override async Task<SubscriptionPayment> CreateAsync(SubscriptionPayment payment)
    {
        return await base.CreateAsync(payment);
    }

    /// <summary>
    /// Updates an existing subscription payment
    /// </summary>
    public override async Task<SubscriptionPayment> UpdateAsync(SubscriptionPayment payment)
    {
        return await base.UpdateAsync(payment);
    }

    // Note: DeleteAsync is inherited from RepositoryBase<SubscriptionPayment>
    // Service layer should handle audit properties and use UpdateAsync for soft deletes

    /// <summary>
    /// Checks if a subscription payment exists
    /// </summary>
    public override async Task<bool> ExistsAsync(object id)
    {
        if (id is not Guid paymentId)
            return false;

        return await _context.SubscriptionPayments.AnyAsync(sp => sp.Id == paymentId);
    }

    public async Task<IEnumerable<SubscriptionPayment>> GetPendingPaymentsAsync()
    {
        return await _context.SubscriptionPayments
            .Include(sp => sp.Subscription)
            .Where(sp => sp.Status == SubscriptionPayment.PaymentStatus.Pending)
            .OrderBy(sp => sp.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<SubscriptionPayment>> GetFailedPaymentsAsync()
    {
        return await _context.SubscriptionPayments
            .Include(sp => sp.Subscription)
            .Where(sp => sp.Status == SubscriptionPayment.PaymentStatus.Failed)
            .OrderByDescending(sp => sp.CreatedDate)
            .ToListAsync();
    }

    public async Task<SubscriptionPayment?> GetByPaymentIntentIdAsync(string paymentIntentId)
    {
        return await _context.SubscriptionPayments
            .Include(sp => sp.Subscription)
            .FirstOrDefaultAsync(sp => sp.PaymentIntentId == paymentIntentId);
    }

    /// <summary>
    /// Retrieves a subscription payment by its billing record ID with related entities
    /// </summary>
    public async Task<SubscriptionPayment?> GetByBillingRecordIdAsync(Guid billingRecordId)
    {
        return await _context.SubscriptionPayments
            .Include(sp => sp.Subscription)
            .Include(sp => sp.BillingRecord)
            .FirstOrDefaultAsync(sp => sp.BillingRecordId == billingRecordId);
    }

    /// <summary>
    /// Retrieves failed payments that are due for retry with related entities
    /// </summary>
    public async Task<IEnumerable<SubscriptionPayment>> GetFailedPaymentsDueForRetryAsync(
        DateTime now, int maxResults = 100)
    {
        return await _context.SubscriptionPayments
            .Include(sp => sp.Subscription)
            .Include(sp => sp.BillingRecord)
            .Where(sp => sp.Status == SubscriptionPayment.PaymentStatus.Failed)
            .Where(sp => sp.NextRetryAt.HasValue && sp.NextRetryAt.Value <= now)
            .Where(sp => sp.AttemptCount < 3)
            .OrderBy(sp => sp.NextRetryAt)
            .Take(maxResults)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves subscription payments with database-level filtering, pagination, and sorting
    /// </summary>
    public async Task<(IEnumerable<SubscriptionPayment> Payments, int TotalCount)> GetPaymentsWithFilteringAsync(
        int page, int pageSize, Guid? subscriptionId = null, int? userId = null, 
        string? status = null, string? search = null, DateTime? startDate = null, 
        DateTime? endDate = null, string? sortBy = "CreatedDate", string? sortOrder = "desc")
    {
        var query = _context.SubscriptionPayments
            .Include(sp => sp.Subscription)
                .ThenInclude(s => s.User)
            .AsQueryable();

        // Apply filters
        if (subscriptionId.HasValue)
        {
            query = query.Where(sp => sp.SubscriptionId == subscriptionId.Value);
        }

        if (userId.HasValue)
        {
            query = query.Where(sp => sp.Subscription.UserId == userId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (Enum.TryParse<SubscriptionPayment.PaymentStatus>(status, out var paymentStatus))
            {
                query = query.Where(sp => sp.Status == paymentStatus);
            }
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(sp =>
                sp.PaymentIntentId.ToLower().Contains(term) ||
                sp.Subscription.User.Email.ToLower().Contains(term) ||
                sp.StripePaymentIntentId.ToLower().Contains(term));
        }

        if (startDate.HasValue)
        {
            query = query.Where(sp => sp.CreatedDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(sp => sp.CreatedDate <= endDate.Value);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply sorting
        query = ApplySorting(query, sortBy, sortOrder);

        // Apply pagination
        var skip = (page - 1) * pageSize;
        var payments = await query
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();

        return (payments, totalCount);
    }

    private static IQueryable<SubscriptionPayment> ApplySorting(IQueryable<SubscriptionPayment> query, string? sortBy, string? sortOrder)
    {
        // Default sorting if parameters are null or empty
        if (string.IsNullOrEmpty(sortBy) || string.IsNullOrEmpty(sortOrder))
        {
            return query.OrderByDescending(sp => sp.PaidAt);
        }

        return sortBy.ToLower() switch
        {
            "amount" => sortOrder.ToLower() == "desc"
                ? query.OrderByDescending(sp => sp.Amount)
                : query.OrderBy(sp => sp.Amount),
            "status" => sortOrder.ToLower() == "desc"
                ? query.OrderByDescending(sp => sp.Status)
                : query.OrderBy(sp => sp.Status),
            "duedate" => sortOrder.ToLower() == "desc"
                ? query.OrderByDescending(sp => sp.DueDate)
                : query.OrderBy(sp => sp.DueDate),
            "createddate" => sortOrder.ToLower() == "desc"
                ? query.OrderByDescending(sp => sp.CreatedDate)
                : query.OrderBy(sp => sp.CreatedDate),
            "updateddate" => sortOrder.ToLower() == "desc"
                ? query.OrderByDescending(sp => sp.UpdatedDate)
                : query.OrderBy(sp => sp.UpdatedDate),
            _ => query.OrderByDescending(sp => sp.CreatedDate)
        };
    }
} 