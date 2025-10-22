using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces
{
    /// <summary>
    /// Repository interface for managing failed compensating refunds
    /// </summary>
    public interface IFailedRefundRepository : IRepositoryBase<FailedRefund>
    {
        /// <summary>
        /// Gets all failed refunds that should be retried
        /// </summary>
        Task<IEnumerable<FailedRefund>> GetPendingRetryAsync();

        /// <summary>
        /// Gets all failed refunds that require manual intervention
        /// </summary>
        Task<IEnumerable<FailedRefund>> GetRequiringManualInterventionAsync();

        /// <summary>
        /// Gets failed refunds by user ID
        /// </summary>
        Task<IEnumerable<FailedRefund>> GetByUserIdAsync(int userId);

        /// <summary>
        /// Gets failed refund by billing record ID
        /// </summary>
        Task<FailedRefund?> GetByBillingRecordIdAsync(Guid billingRecordId);

        /// <summary>
        /// Gets failed refund by Stripe payment intent ID
        /// </summary>
        Task<FailedRefund?> GetByStripePaymentIntentIdAsync(string stripePaymentIntentId);

        /// <summary>
        /// Marks a failed refund as successfully refunded
        /// </summary>
        Task<bool> MarkAsRefundedAsync(Guid failedRefundId, string? notes = null, int? resolvedBy = null);

        /// <summary>
        /// Marks a failed refund as manually resolved
        /// </summary>
        Task<bool> MarkAsManuallyResolvedAsync(Guid failedRefundId, string resolutionNotes, int resolvedBy);

        /// <summary>
        /// Increments retry count for a failed refund
        /// </summary>
        Task<bool> IncrementRetryCountAsync(Guid failedRefundId, string errorMessage);

        /// <summary>
        /// Marks admin as notified for a failed refund
        /// </summary>
        Task<bool> MarkAdminNotifiedAsync(Guid failedRefundId);

        /// <summary>
        /// Gets statistics about failed refunds
        /// </summary>
        Task<FailedRefundStats> GetStatsAsync(DateTime? startDate = null, DateTime? endDate = null);
    }

    /// <summary>
    /// Statistics about failed refunds
    /// </summary>
    public class FailedRefundStats
    {
        public int TotalPending { get; set; }
        public int TotalRetrying { get; set; }
        public int TotalRefunded { get; set; }
        public int TotalManuallyResolved { get; set; }
        public int TotalRequiringIntervention { get; set; }
        public decimal TotalAmountPending { get; set; }
        public decimal TotalAmountResolved { get; set; }
        public int AverageRetryCount { get; set; }
    }
}

