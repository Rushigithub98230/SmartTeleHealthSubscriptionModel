using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories;

public class SubscriptionPaymentRepository : ISubscriptionPaymentRepository
{
    private readonly ApplicationDbContext _context;

    public SubscriptionPaymentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SubscriptionPayment> GetByIdAsync(Guid id)
    {
        return await _context.SubscriptionPayments
            .Include(sp => sp.Subscription)
            .FirstOrDefaultAsync(sp => sp.Id == id);
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

    public async Task<SubscriptionPayment> CreateAsync(SubscriptionPayment payment)
    {
        payment.CreatedDate = DateTime.UtcNow;
        payment.UpdatedDate = DateTime.UtcNow;
        
        _context.SubscriptionPayments.Add(payment);
        await _context.SaveChangesAsync();
        return payment;
    }

    public async Task<SubscriptionPayment> UpdateAsync(SubscriptionPayment payment)
    {
        payment.UpdatedDate = DateTime.UtcNow;
        
        _context.SubscriptionPayments.Update(payment);
        await _context.SaveChangesAsync();
        return payment;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var payment = await _context.SubscriptionPayments.FindAsync(id);
        if (payment == null)
            return false;

        _context.SubscriptionPayments.Remove(payment);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.SubscriptionPayments.AnyAsync(sp => sp.Id == id);
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