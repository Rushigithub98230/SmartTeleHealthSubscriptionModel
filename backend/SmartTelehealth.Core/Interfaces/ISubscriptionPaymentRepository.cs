using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface ISubscriptionPaymentRepository
{
    Task<SubscriptionPayment> GetByIdAsync(Guid id);
    Task<IEnumerable<SubscriptionPayment>> GetBySubscriptionIdAsync(Guid subscriptionId);
    Task<IEnumerable<SubscriptionPayment>> GetByUserIdAsync(int userId);
    Task<IEnumerable<SubscriptionPayment>> GetByStatusAsync(SubscriptionPayment.PaymentStatus status);
    Task<SubscriptionPayment> CreateAsync(SubscriptionPayment payment);
    Task<SubscriptionPayment> UpdateAsync(SubscriptionPayment payment);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<IEnumerable<SubscriptionPayment>> GetPendingPaymentsAsync();
    Task<IEnumerable<SubscriptionPayment>> GetFailedPaymentsAsync();
    Task<SubscriptionPayment?> GetByPaymentIntentIdAsync(string paymentIntentId);
} 