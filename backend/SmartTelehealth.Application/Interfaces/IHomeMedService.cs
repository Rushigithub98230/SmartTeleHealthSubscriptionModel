using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IHomeMedService
{
    // Prescription Management
    Task<JsonModel> CreatePrescriptionAsync(CreatePrescriptionDto createDto, TokenModel tokenModel);
    Task<JsonModel> GetPrescriptionAsync(Guid id, TokenModel tokenModel);
    Task<JsonModel> GetUserPrescriptionsAsync(Guid userId, TokenModel tokenModel);
    Task<JsonModel> GetProviderPrescriptionsAsync(Guid providerId, TokenModel tokenModel);
    Task<JsonModel> UpdatePrescriptionAsync(Guid id, UpdatePrescriptionDto updateDto, TokenModel tokenModel);
    Task<JsonModel> DeletePrescriptionAsync(Guid id, TokenModel tokenModel);
    
    // Prescription Workflow
    Task<JsonModel> SendPrescriptionToPharmacyAsync(Guid prescriptionId, TokenModel tokenModel);
    Task<JsonModel> ConfirmPrescriptionAsync(Guid prescriptionId, string pharmacyReference, TokenModel tokenModel);
    Task<JsonModel> DispensePrescriptionAsync(Guid prescriptionId, TokenModel tokenModel);
    Task<JsonModel> ShipPrescriptionAsync(Guid prescriptionId, string trackingNumber, TokenModel tokenModel);
    Task<JsonModel> DeliverPrescriptionAsync(Guid prescriptionId, TokenModel tokenModel);
    
    // Medication Shipment Management
    Task<JsonModel> CreateShipmentAsync(CreateMedicationShipmentDto createDto, TokenModel tokenModel);
    Task<JsonModel> GetShipmentAsync(Guid id, TokenModel tokenModel);
    Task<JsonModel> GetUserShipmentsAsync(Guid userId, TokenModel tokenModel);
    Task<JsonModel> UpdateShipmentAsync(Guid id, UpdateMedicationShipmentDto updateDto, TokenModel tokenModel);
    Task<JsonModel> DeleteShipmentAsync(Guid id, TokenModel tokenModel);
    
    // Shipment Workflow
    Task<JsonModel> ProcessShipmentAsync(Guid shipmentId, TokenModel tokenModel);
    Task<JsonModel> ShipMedicationAsync(Guid shipmentId, string trackingNumber, string carrier, TokenModel tokenModel);
    Task<JsonModel> DeliverMedicationAsync(Guid shipmentId, TokenModel tokenModel);
    Task<JsonModel> ReturnShipmentAsync(Guid shipmentId, string reason, TokenModel tokenModel);
    
    // Tracking and Status
    Task<JsonModel> GetTrackingStatusAsync(string trackingNumber, TokenModel tokenModel);
    Task<JsonModel> GetEstimatedDeliveryAsync(Guid shipmentId, TokenModel tokenModel);
    Task<JsonModel> UpdateTrackingInfoAsync(Guid shipmentId, string trackingNumber, string status, TokenModel tokenModel);
    
    // Analytics and Reporting
    Task<JsonModel> GetPrescriptionAnalyticsAsync(DateTime? startDate, DateTime? endDate, TokenModel tokenModel);
    Task<JsonModel> GetShipmentAnalyticsAsync(DateTime? startDate, DateTime? endDate, TokenModel tokenModel);
    Task<JsonModel> GeneratePrescriptionReportAsync(DateTime startDate, DateTime endDate, string format, TokenModel tokenModel);
    Task<JsonModel> GenerateShipmentReportAsync(DateTime startDate, DateTime endDate, string format, TokenModel tokenModel);
    
    // Pharmacy Integration (Database Level)
    Task<JsonModel> GetPharmacyIntegrationAsync(TokenModel tokenModel);
    Task<JsonModel> TestPharmacyConnectionAsync(TokenModel tokenModel);
    Task<JsonModel> SyncPrescriptionsAsync(TokenModel tokenModel);
    Task<JsonModel> SyncShipmentsAsync(TokenModel tokenModel);
    
    // Refill Management
    Task<JsonModel> CreateRefillRequestAsync(Guid prescriptionId, TokenModel tokenModel);
    Task<JsonModel> GetRefillRequestsAsync(Guid userId, TokenModel tokenModel);
    Task<JsonModel> ApproveRefillRequestAsync(Guid prescriptionId, TokenModel tokenModel);
    Task<JsonModel> DenyRefillRequestAsync(Guid prescriptionId, string reason, TokenModel tokenModel);
    
    // Inventory Management (Placeholder for HomeMed API)
    Task<JsonModel> CheckMedicationAvailabilityAsync(string medicationName, string dosage, TokenModel tokenModel);
    Task<JsonModel> GetMedicationPriceAsync(string medicationName, string dosage, TokenModel tokenModel);
    Task<JsonModel> ReserveMedicationAsync(string medicationName, string dosage, int quantity, TokenModel tokenModel);
    
    // Auto-dispatch (Placeholder for HomeMed API)
    Task<JsonModel> TriggerAutoDispatchAsync(Guid prescriptionId, TokenModel tokenModel);
    Task<JsonModel> ProcessAutoDispatchQueueAsync(TokenModel tokenModel);
    Task<JsonModel> GetAutoDispatchQueueAsync(TokenModel tokenModel);
} 