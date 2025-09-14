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
        payment.CreatedDate = DateTime.UtcNow;
        payment.UpdatedDate = DateTime.UtcNow;
        return await base.CreateAsync(payment);
    }

    /// <summary>
    /// Updates an existing subscription payment
    /// </summary>
    public override async Task<SubscriptionPayment> UpdateAsync(SubscriptionPayment payment)
    {
        payment.UpdatedDate = DateTime.UtcNow;
        return await base.UpdateAsync(payment);
    }

    /// <summary>
    /// Deletes a subscription payment by its unique identifier
    /// </summary>
    public override async Task<bool> DeleteAsync(object id)
    {
        if (id is not Guid paymentId)
            return false;

        var payment = await _context.SubscriptionPayments.FindAsync(paymentId);
        if (payment == null)
            return false;

        _context.SubscriptionPayments.Remove(payment);
        await _context.SaveChangesAsync();
        return true;
    }

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
} 