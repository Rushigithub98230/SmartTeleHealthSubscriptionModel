using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface ISubscriptionPaymentRepository : IRepositoryBase<SubscriptionPayment>
{
    // Basic CRUD methods are inherited from IRepositoryBase<SubscriptionPayment>
    
    Task<IEnumerable<SubscriptionPayment>> GetBySubscriptionIdAsync(Guid subscriptionId);
    Task<IEnumerable<SubscriptionPayment>> GetByUserIdAsync(int userId);
    Task<IEnumerable<SubscriptionPayment>> GetByStatusAsync(SubscriptionPayment.PaymentStatus status);
    Task<IEnumerable<SubscriptionPayment>> GetPendingPaymentsAsync();
    Task<IEnumerable<SubscriptionPayment>> GetFailedPaymentsAsync();
    Task<SubscriptionPayment?> GetByPaymentIntentIdAsync(string paymentIntentId);
    Task<SubscriptionPayment?> GetByBillingRecordIdAsync(Guid billingRecordId);
    Task<IEnumerable<SubscriptionPayment>> GetFailedPaymentsDueForRetryAsync(DateTime now, int maxResults = 100);
    
    // Database-level filtering, pagination, and sorting
    Task<(IEnumerable<SubscriptionPayment> Payments, int TotalCount)> GetPaymentsWithFilteringAsync(
        int page, int pageSize, Guid? subscriptionId = null, int? userId = null, 
        string? status = null, string? search = null, DateTime? startDate = null, 
        DateTime? endDate = null, string? sortBy = "CreatedDate", string? sortOrder = "desc");
} 