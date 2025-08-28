using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IMedicationDeliveryRepository
{
    Task<MedicationDelivery?> GetByIdAsync(Guid id);
    Task<IEnumerable<MedicationDelivery>> GetByUserIdAsync(int userId);
    Task<IEnumerable<MedicationDelivery>> GetBySubscriptionIdAsync(Guid subscriptionId);
    Task<IEnumerable<MedicationDelivery>> GetByStatusAsync(MedicationDelivery.DeliveryStatus status);
    Task<IEnumerable<MedicationDelivery>> GetPendingDeliveriesAsync();
    Task<MedicationDelivery> CreateAsync(MedicationDelivery delivery);
    Task<MedicationDelivery> UpdateAsync(MedicationDelivery delivery);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<int> GetPendingDeliveryCountAsync();
    Task<IEnumerable<MedicationDelivery>> GetDeliveriesByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<MedicationDelivery?> GetByTrackingNumberAsync(string trackingNumber);
    Task<IEnumerable<MedicationDelivery>> GetAllAsync();
} 