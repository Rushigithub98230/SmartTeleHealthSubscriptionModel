using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IMedicationShipmentRepository
{
    Task<MedicationDelivery> GetByIdAsync(Guid id);
    Task<IEnumerable<MedicationDelivery>> GetByUserIdAsync(int userId);
    Task<IEnumerable<MedicationDelivery>> GetByStatusAsync(string status);
    Task<IEnumerable<MedicationDelivery>> GetByTrackingNumberAsync(string trackingNumber);
    Task<MedicationDelivery> CreateAsync(MedicationDelivery shipment);
    Task<MedicationDelivery> UpdateAsync(MedicationDelivery shipment);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<MedicationDelivery>> GetOverdueShipmentsAsync();
    Task<IEnumerable<MedicationDelivery>> GetShipmentsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<int> GetShipmentCountAsync(int userId);
    Task<decimal> GetShipmentTotalAsync(int userId);
} 