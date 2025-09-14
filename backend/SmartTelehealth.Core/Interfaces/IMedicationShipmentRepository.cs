using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IMedicationShipmentRepository : IRepositoryBase<MedicationDelivery>
{
    // Basic CRUD methods are inherited from IRepositoryBase<MedicationDelivery>
    
    Task<IEnumerable<MedicationDelivery>> GetByUserIdAsync(int userId);
    Task<IEnumerable<MedicationDelivery>> GetByStatusAsync(string status);
    Task<IEnumerable<MedicationDelivery>> GetByTrackingNumberAsync(string trackingNumber);
    Task<IEnumerable<MedicationDelivery>> GetOverdueShipmentsAsync();
    Task<IEnumerable<MedicationDelivery>> GetShipmentsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<int> GetShipmentCountAsync(int userId);
    Task<decimal> GetShipmentTotalAsync(int userId);
} 