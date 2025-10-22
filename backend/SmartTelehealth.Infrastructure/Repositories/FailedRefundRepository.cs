using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for managing failed compensating refunds
    /// </summary>
    public class FailedRefundRepository : RepositoryBase<FailedRefund>, IFailedRefundRepository
    {
        private readonly ApplicationDbContext _context;

        public FailedRefundRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets all failed refunds that should be retried (pending and not exceeded max retries)
        /// </summary>
        public async Task<IEnumerable<FailedRefund>> GetPendingRetryAsync()
        {
            return await _context.FailedRefunds
                .Include(f => f.BillingRecord)
                .Include(f => f.User)
                .Where(f => f.Status == FailedRefundStatus.Pending && f.RetryCount < f.MaxRetries)
                .OrderBy(f => f.FirstAttemptAt) // Oldest first
                .ToListAsync();
        }

        /// <summary>
        /// Gets all failed refunds that require manual intervention (exceeded max retries)
        /// </summary>
        public async Task<IEnumerable<FailedRefund>> GetRequiringManualInterventionAsync()
        {
            return await _context.FailedRefunds
                .Include(f => f.BillingRecord)
                .Include(f => f.User)
                .Where(f => f.Status == FailedRefundStatus.Pending && f.RetryCount >= f.MaxRetries)
                .OrderByDescending(f => f.LastAttemptAt)
                .ToListAsync();
        }

        /// <summary>
        /// Gets failed refunds by user ID
        /// </summary>
        public async Task<IEnumerable<FailedRefund>> GetByUserIdAsync(int userId)
        {
            return await _context.FailedRefunds
                .Include(f => f.BillingRecord)
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.CreatedDate)
                .ToListAsync();
        }

        /// <summary>
        /// Gets failed refund by billing record ID
        /// </summary>
        public async Task<FailedRefund?> GetByBillingRecordIdAsync(Guid billingRecordId)
        {
            return await _context.FailedRefunds
                .Include(f => f.BillingRecord)
                .Include(f => f.User)
                .FirstOrDefaultAsync(f => f.BillingRecordId == billingRecordId);
        }

        /// <summary>
        /// Gets failed refund by Stripe payment intent ID
        /// </summary>
        public async Task<FailedRefund?> GetByStripePaymentIntentIdAsync(string stripePaymentIntentId)
        {
            return await _context.FailedRefunds
                .Include(f => f.BillingRecord)
                .Include(f => f.User)
                .FirstOrDefaultAsync(f => f.StripePaymentIntentId == stripePaymentIntentId);
        }

        /// <summary>
        /// Marks a failed refund as successfully refunded
        /// </summary>
        public async Task<bool> MarkAsRefundedAsync(Guid failedRefundId, string? notes = null, int? resolvedBy = null)
        {
            var failedRefund = await GetByIdAsync(failedRefundId);
            if (failedRefund == null)
                return false;

            failedRefund.Status = FailedRefundStatus.Refunded;
            failedRefund.ResolvedAt = DateTime.UtcNow;
            failedRefund.ResolvedBy = resolvedBy;
            failedRefund.ResolutionNotes = notes;
            failedRefund.UpdatedDate = DateTime.UtcNow;
            failedRefund.UpdatedBy = resolvedBy;

            await UpdateAsync(failedRefund);
            return true;
        }

        /// <summary>
        /// Marks a failed refund as manually resolved by admin
        /// </summary>
        public async Task<bool> MarkAsManuallyResolvedAsync(Guid failedRefundId, string resolutionNotes, int resolvedBy)
        {
            var failedRefund = await GetByIdAsync(failedRefundId);
            if (failedRefund == null)
                return false;

            failedRefund.Status = FailedRefundStatus.ManuallyResolved;
            failedRefund.ResolvedAt = DateTime.UtcNow;
            failedRefund.ResolvedBy = resolvedBy;
            failedRefund.ResolutionNotes = resolutionNotes;
            failedRefund.UpdatedDate = DateTime.UtcNow;
            failedRefund.UpdatedBy = resolvedBy;

            await UpdateAsync(failedRefund);
            return true;
        }

        /// <summary>
        /// Increments retry count for a failed refund and updates error message
        /// </summary>
        public async Task<bool> IncrementRetryCountAsync(Guid failedRefundId, string errorMessage)
        {
            var failedRefund = await GetByIdAsync(failedRefundId);
            if (failedRefund == null)
                return false;

            failedRefund.RetryCount++;
            failedRefund.LastAttemptAt = DateTime.UtcNow;
            failedRefund.LastErrorMessage = errorMessage;
            failedRefund.Status = failedRefund.RetryCount < failedRefund.MaxRetries 
                ? FailedRefundStatus.Pending 
                : FailedRefundStatus.Pending; // Keep as Pending but now requires manual intervention
            failedRefund.UpdatedDate = DateTime.UtcNow;

            await UpdateAsync(failedRefund);
            return true;
        }

        /// <summary>
        /// Marks admin as notified for a failed refund
        /// </summary>
        public async Task<bool> MarkAdminNotifiedAsync(Guid failedRefundId)
        {
            var failedRefund = await GetByIdAsync(failedRefundId);
            if (failedRefund == null)
                return false;

            failedRefund.AdminNotified = true;
            failedRefund.AdminNotifiedAt = DateTime.UtcNow;
            failedRefund.UpdatedDate = DateTime.UtcNow;

            await UpdateAsync(failedRefund);
            return true;
        }

        /// <summary>
        /// Gets statistics about failed refunds
        /// </summary>
        public async Task<FailedRefundStats> GetStatsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.FailedRefunds.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(f => f.CreatedDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(f => f.CreatedDate <= endDate.Value);

            var failedRefunds = await query.ToListAsync();

            return new FailedRefundStats
            {
                TotalPending = failedRefunds.Count(f => f.Status == FailedRefundStatus.Pending),
                TotalRetrying = failedRefunds.Count(f => f.Status == FailedRefundStatus.Retrying),
                TotalRefunded = failedRefunds.Count(f => f.Status == FailedRefundStatus.Refunded),
                TotalManuallyResolved = failedRefunds.Count(f => f.Status == FailedRefundStatus.ManuallyResolved),
                TotalRequiringIntervention = failedRefunds.Count(f => f.Status == FailedRefundStatus.Pending && f.RetryCount >= f.MaxRetries),
                TotalAmountPending = failedRefunds.Where(f => f.Status == FailedRefundStatus.Pending).Sum(f => f.Amount),
                TotalAmountResolved = failedRefunds.Where(f => f.Status == FailedRefundStatus.Refunded || f.Status == FailedRefundStatus.ManuallyResolved).Sum(f => f.Amount),
                AverageRetryCount = failedRefunds.Any() ? (int)failedRefunds.Average(f => f.RetryCount) : 0
            };
        }
    }
}

