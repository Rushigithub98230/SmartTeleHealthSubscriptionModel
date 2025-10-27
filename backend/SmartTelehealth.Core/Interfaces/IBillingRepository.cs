using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.DTOs;

namespace SmartTelehealth.Core.Interfaces;

public interface IBillingRepository : IRepositoryBase<BillingRecord>
{
    
    
    // Custom methods with business logic
    Task<BillingRecord?> GetByIdWithDetailsAsync(Guid billingId);
    Task<IEnumerable<BillingRecord>> GetAllWithDetailsAsync();
    Task<BillingRecord> CreateBillingRecordAsync(BillingRecord billingRecord);
    Task<BillingRecord> UpdateBillingRecordAsync(BillingRecord billingRecord);
    Task<bool> DeleteBillingRecordAsync(Guid billingId);
    Task<bool> ExistsBillingRecordAsync(Guid billingId);
    
    Task<IEnumerable<BillingRecord>> GetByUserIdAsync(int userId);
    Task<IEnumerable<BillingRecord>> GetBySubscriptionIdAsync(Guid subscriptionId);
    Task<IEnumerable<BillingRecord>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<BillingRecord>> GetByStatusAsync(BillingRecord.BillingStatus status);
    
    // New methods for InvoiceService
    Task<BillingRecord?> GetByInvoiceNumberAsync(string invoiceNumber);
    Task<IEnumerable<BillingRecord>> GetInvoicesByUserIdAsync(int userId, int page, int pageSize);
    Task<int> GetInvoiceCountByUserIdAsync(int userId);
    Task<IEnumerable<BillingRecord>> GetBillingRecordsByDateRangeAsync(DateTime startDate, DateTime endDate);
    
    // Additional methods needed by BillingService
    Task<IEnumerable<BillingAdjustment>> GetAdjustmentsByBillingRecordIdAsync(Guid billingRecordId);
    Task<BillingAdjustment?> GetAdjustmentByIdAsync(Guid adjustmentId);
    Task<BillingAdjustment> CreateAdjustmentAsync(BillingAdjustment adjustment);
    Task<BillingAdjustment> UpdateAdjustmentAsync(BillingAdjustment adjustment);
    Task<IEnumerable<BillingRecord>> GetByBillingCycleIdAsync(Guid billingCycleId);
    Task<IEnumerable<BillingRecord>> GetOverdueBillingRecordsAsync();
    Task<IEnumerable<BillingRecord>> GetPendingBillingRecordsAsync();
    
    // Webhook support methods
    Task<BillingRecord?> GetByStripePaymentIntentIdAsync(string stripePaymentIntentId);
    Task<BillingRecord?> GetByStripeInvoiceIdAsync(string stripeInvoiceId);
    Task<BillingRecord?> GetByTransactionIdAsync(string transactionId);
    
    // Admin dashboard methods (Phase 2)
    /// <summary>
    /// Gets all billing records for admin dashboard aggregation.
    /// WARNING: Can return large datasets - use with caution or implement pagination
    /// </summary>
    Task<IEnumerable<BillingRecord>> GetAllBillingRecordsAsync();
    
    // Comprehensive filtering method
    Task<(IEnumerable<BillingRecord> BillingRecords, int TotalCount)> GetBillingRecordsWithAdvancedFilteringAsync(BillingFilterDto filter);
    
    // === DATABASE-LEVEL ANALYTICS AGGREGATION METHODS ===
    
    /// <summary>
    /// Gets failed payments count using database-level aggregation for better performance
    /// </summary>
    Task<int> GetFailedPaymentsCountAsync(DateTime startDate, DateTime endDate);
    
    /// <summary>
    /// Gets total revenue using database-level aggregation for better performance
    /// </summary>
    Task<decimal> GetTotalRevenueAsync(DateTime startDate, DateTime endDate);
    
    /// <summary>
    /// Gets monthly revenue breakdown using database-level aggregation
    /// </summary>
    Task<List<MonthlyRevenueData>> GetMonthlyRevenueBreakdownAsync(DateTime startDate, DateTime endDate);
    
    /// <summary>
    /// Gets revenue by category using database-level aggregation
    /// </summary>
    Task<List<CategoryRevenueData>> GetRevenueByCategoryAsync(DateTime startDate, DateTime endDate);
    
    /// <summary>
    /// Gets average revenue per user using database-level aggregation
    /// </summary>
    Task<decimal> GetAverageRevenuePerUserAsync(DateTime startDate, DateTime endDate);
    
    /// <summary>
    /// Gets payment method analytics using database-level aggregation
    /// </summary>
    Task<List<PaymentMethodAnalytics>> GetPaymentMethodAnalyticsAsync(DateTime startDate, DateTime endDate);
    
    /// <summary>
    /// Gets billing status analytics using database-level aggregation
    /// </summary>
    Task<List<BillingStatusAnalytics>> GetBillingStatusAnalyticsAsync(DateTime startDate, DateTime endDate);
    
    /// <summary>
    /// Gets payment success rate using database-level aggregation
    /// </summary>
    Task<decimal> GetPaymentSuccessRateAsync(DateTime startDate, DateTime endDate);
    
    /// <summary>
    /// Gets revenue trend using database-level aggregation
    /// </summary>
    Task<List<RevenueTrendData>> GetRevenueTrendAsync(DateTime startDate, DateTime endDate);
    
    /// <summary>
    /// Gets overage charges analytics using database-level aggregation
    /// </summary>
    Task<OverageChargesAnalytics> GetOverageChargesAnalyticsAsync(DateTime startDate, DateTime endDate);
    
    /// <summary>
    /// Gets billing efficiency metrics using database-level aggregation
    /// </summary>
    Task<BillingEfficiencyMetrics> GetBillingEfficiencyMetricsAsync(DateTime startDate, DateTime endDate);
    
    // Additional analytics methods
    Task<decimal> GetRevenueForDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<int> GetPendingPaymentsCountAsync();
} 