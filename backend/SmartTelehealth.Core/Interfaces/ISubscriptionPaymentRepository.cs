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
} 