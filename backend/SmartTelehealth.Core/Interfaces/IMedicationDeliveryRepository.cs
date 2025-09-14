using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IMedicationDeliveryRepository : IRepositoryBase<MedicationDelivery>
{
    // Basic CRUD methods are inherited from IRepositoryBase<MedicationDelivery>
    
    Task<IEnumerable<MedicationDelivery>> GetByUserIdAsync(int userId);
    Task<IEnumerable<MedicationDelivery>> GetBySubscriptionIdAsync(Guid subscriptionId);
    Task<IEnumerable<MedicationDelivery>> GetByStatusAsync(MedicationDelivery.DeliveryStatus status);
    Task<IEnumerable<MedicationDelivery>> GetPendingDeliveriesAsync();
    Task<IEnumerable<MedicationDelivery>> GetShippedDeliveriesAsync();
    Task<IEnumerable<MedicationDelivery>> GetDeliveriesByStatusAsync(MedicationDelivery.DeliveryStatus status);
    Task<bool> UpdateDeliveryStatusAsync(Guid id, MedicationDelivery.DeliveryStatus status, string trackingNumber = null);
    Task<int> GetPendingDeliveryCountAsync();
    Task<IEnumerable<MedicationDelivery>> GetDeliveriesByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<MedicationDelivery?> GetByTrackingNumberAsync(string trackingNumber);
} 