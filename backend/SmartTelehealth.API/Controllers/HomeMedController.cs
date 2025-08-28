using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HomeMedController : BaseController
{
    private readonly IHomeMedService _homeMedService;

    public HomeMedController(IHomeMedService homeMedService)
    {
        _homeMedService = homeMedService;
    }

    // Prescription Management
    [HttpPost("prescriptions")]
    public async Task<JsonModel> CreatePrescription([FromBody] CreatePrescriptionDto createDto)
    {
        return await _homeMedService.CreatePrescriptionAsync(createDto, GetToken(HttpContext));
    }

    [HttpGet("prescriptions/{id}")]
    public async Task<JsonModel> GetPrescription(Guid id)
    {
        return await _homeMedService.GetPrescriptionAsync(id, GetToken(HttpContext));
    }

    [HttpGet("users/{userId}/prescriptions")]
    public async Task<JsonModel> GetUserPrescriptions(Guid userId)
    {
        return await _homeMedService.GetUserPrescriptionsAsync(userId, GetToken(HttpContext));
    }

    [HttpGet("providers/{providerId}/prescriptions")]
    public async Task<JsonModel> GetProviderPrescriptions(Guid providerId)
    {
        return await _homeMedService.GetProviderPrescriptionsAsync(providerId, GetToken(HttpContext));
    }

    [HttpPut("prescriptions/{id}")]
    public async Task<JsonModel> UpdatePrescription(Guid id, [FromBody] UpdatePrescriptionDto updateDto)
    {
        return await _homeMedService.UpdatePrescriptionAsync(id, updateDto, GetToken(HttpContext));
    }

    [HttpDelete("prescriptions/{id}")]
    public async Task<JsonModel> DeletePrescription(Guid id)
    {
        return await _homeMedService.DeletePrescriptionAsync(id, GetToken(HttpContext));
    }

    // Prescription Workflow
    [HttpPost("prescriptions/{id}/send-to-pharmacy")]
    public async Task<JsonModel> SendPrescriptionToPharmacy(Guid id)
    {
        return await _homeMedService.SendPrescriptionToPharmacyAsync(id, GetToken(HttpContext));
    }

    [HttpPost("prescriptions/{id}/confirm")]
    public async Task<JsonModel> ConfirmPrescription(Guid id, [FromQuery] string pharmacyReference)
    {
        return await _homeMedService.ConfirmPrescriptionAsync(id, pharmacyReference, GetToken(HttpContext));
    }

    [HttpPost("prescriptions/{id}/dispense")]
    public async Task<JsonModel> DispensePrescription(Guid id)
    {
        return await _homeMedService.DispensePrescriptionAsync(id, GetToken(HttpContext));
    }

    [HttpPost("prescriptions/{id}/ship")]
    public async Task<JsonModel> ShipPrescription(Guid id, [FromQuery] string trackingNumber)
    {
        return await _homeMedService.ShipPrescriptionAsync(id, trackingNumber, GetToken(HttpContext));
    }

    [HttpPost("prescriptions/{id}/deliver")]
    public async Task<JsonModel> DeliverPrescription(Guid id)
    {
        return await _homeMedService.DeliverPrescriptionAsync(id, GetToken(HttpContext));
    }

    // Medication Shipment Management
    [HttpPost("shipments")]
    public async Task<JsonModel> CreateShipment([FromBody] CreateMedicationShipmentDto createDto)
    {
        return await _homeMedService.CreateShipmentAsync(createDto, GetToken(HttpContext));
    }

    [HttpGet("shipments/{id}")]
    public async Task<JsonModel> GetShipment(Guid id)
    {
        return await _homeMedService.GetShipmentAsync(id, GetToken(HttpContext));
    }

    [HttpGet("users/{userId}/shipments")]
    public async Task<JsonModel> GetUserShipments(Guid userId)
    {
        return await _homeMedService.GetUserShipmentsAsync(userId, GetToken(HttpContext));
    }

    [HttpPut("shipments/{id}")]
    public async Task<JsonModel> UpdateShipment(Guid id, [FromBody] UpdateMedicationShipmentDto updateDto)
    {
        return await _homeMedService.UpdateShipmentAsync(id, updateDto, GetToken(HttpContext));
    }

    [HttpDelete("shipments/{id}")]
    public async Task<JsonModel> DeleteShipment(Guid id)
    {
        return await _homeMedService.DeleteShipmentAsync(id, GetToken(HttpContext));
    }

    // Shipment Workflow
    [HttpPost("shipments/{id}/process")]
    public async Task<JsonModel> ProcessShipment(Guid id)
    {
        return await _homeMedService.ProcessShipmentAsync(id, GetToken(HttpContext));
    }

    [HttpPost("shipments/{id}/ship")]
    public async Task<JsonModel> ShipMedication(Guid id, [FromQuery] string trackingNumber, [FromQuery] string carrier)
    {
        return await _homeMedService.ShipMedicationAsync(id, trackingNumber, carrier, GetToken(HttpContext));
    }

    [HttpPost("shipments/{id}/deliver")]
    public async Task<JsonModel> DeliverMedication(Guid id)
    {
        return await _homeMedService.DeliverMedicationAsync(id, GetToken(HttpContext));
    }

    [HttpPost("shipments/{id}/return")]
    public async Task<JsonModel> ReturnShipment(Guid id, [FromQuery] string reason)
    {
        return await _homeMedService.ReturnShipmentAsync(id, reason, GetToken(HttpContext));
    }

    // Tracking and Status
    [HttpGet("tracking/{trackingNumber}")]
    public async Task<JsonModel> GetTrackingStatus(string trackingNumber)
    {
        return await _homeMedService.GetTrackingStatusAsync(trackingNumber, GetToken(HttpContext));
    }

    [HttpGet("shipments/{id}/estimated-delivery")]
    public async Task<JsonModel> GetEstimatedDelivery(Guid id)
    {
        return await _homeMedService.GetEstimatedDeliveryAsync(id, GetToken(HttpContext));
    }

    [HttpPut("shipments/{id}/tracking")]
    public async Task<JsonModel> UpdateTrackingInfo(Guid id, [FromQuery] string trackingNumber, [FromQuery] string status)
    {
        return await _homeMedService.UpdateTrackingInfoAsync(id, trackingNumber, status, GetToken(HttpContext));
    }

    // Analytics and Reporting
    [HttpGet("analytics/prescriptions")]
    public async Task<JsonModel> GetPrescriptionAnalytics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        return await _homeMedService.GetPrescriptionAnalyticsAsync(startDate, endDate, GetToken(HttpContext));
    }

    [HttpGet("analytics/shipments")]
    public async Task<JsonModel> GetShipmentAnalytics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        return await _homeMedService.GetShipmentAnalyticsAsync(startDate, endDate, GetToken(HttpContext));
    }

    [HttpGet("reports/prescriptions")]
    public async Task<JsonModel> GeneratePrescriptionReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] string format = "pdf")
    {
        return await _homeMedService.GeneratePrescriptionReportAsync(startDate, endDate, format, GetToken(HttpContext));
    }

    [HttpGet("reports/shipments")]
    public async Task<JsonModel> GenerateShipmentReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] string format = "pdf")
    {
        return await _homeMedService.GenerateShipmentReportAsync(startDate, endDate, format, GetToken(HttpContext));
    }

    // Pharmacy Integration
    [HttpGet("pharmacy/integration")]
    public async Task<JsonModel> GetPharmacyIntegration()
    {
        return await _homeMedService.GetPharmacyIntegrationAsync(GetToken(HttpContext));
    }

    [HttpPost("pharmacy/test-connection")]
    public async Task<JsonModel> TestPharmacyConnection()
    {
        return await _homeMedService.TestPharmacyConnectionAsync(GetToken(HttpContext));
    }

    [HttpPost("pharmacy/sync-prescriptions")]
    public async Task<JsonModel> SyncPrescriptions()
    {
        return await _homeMedService.SyncPrescriptionsAsync(GetToken(HttpContext));
    }

    [HttpPost("pharmacy/sync-shipments")]
    public async Task<JsonModel> SyncShipments()
    {
        return await _homeMedService.SyncShipmentsAsync(GetToken(HttpContext));
    }

    // Refill Management
    [HttpPost("prescriptions/{id}/refill-request")]
    public async Task<JsonModel> CreateRefillRequest(Guid id)
    {
        return await _homeMedService.CreateRefillRequestAsync(id, GetToken(HttpContext));
    }

    [HttpGet("users/{userId}/refill-requests")]
    public async Task<JsonModel> GetRefillRequests(Guid userId)
    {
        return await _homeMedService.GetRefillRequestsAsync(userId, GetToken(HttpContext));
    }

    [HttpPost("prescriptions/{id}/approve-refill")]
    public async Task<JsonModel> ApproveRefillRequest(Guid id)
    {
        return await _homeMedService.ApproveRefillRequestAsync(id, GetToken(HttpContext));
    }

    [HttpPost("prescriptions/{id}/deny-refill")]
    public async Task<JsonModel> DenyRefillRequest(Guid id, [FromQuery] string reason)
    {
        return await _homeMedService.DenyRefillRequestAsync(id, reason, GetToken(HttpContext));
    }

    // Inventory Management
    [HttpGet("medications/availability")]
    public async Task<JsonModel> CheckMedicationAvailability([FromQuery] string medicationName, [FromQuery] string dosage)
    {
        return await _homeMedService.CheckMedicationAvailabilityAsync(medicationName, dosage, GetToken(HttpContext));
    }

    [HttpGet("medications/price")]
    public async Task<JsonModel> GetMedicationPrice([FromQuery] string medicationName, [FromQuery] string dosage)
    {
        return await _homeMedService.GetMedicationPriceAsync(medicationName, dosage, GetToken(HttpContext));
    }

    [HttpPost("medications/reserve")]
    public async Task<JsonModel> ReserveMedication([FromQuery] string medicationName, [FromQuery] string dosage, [FromQuery] int quantity)
    {
        return await _homeMedService.ReserveMedicationAsync(medicationName, dosage, quantity, GetToken(HttpContext));
    }

    // Auto-dispatch
    [HttpPost("prescriptions/{id}/auto-dispatch")]
    public async Task<JsonModel> TriggerAutoDispatch(Guid id)
    {
        return await _homeMedService.TriggerAutoDispatchAsync(id, GetToken(HttpContext));
    }

    [HttpPost("auto-dispatch/process-queue")]
    public async Task<JsonModel> ProcessAutoDispatchQueue()
    {
        return await _homeMedService.ProcessAutoDispatchQueueAsync(GetToken(HttpContext));
    }

    [HttpGet("auto-dispatch/queue")]
    public async Task<JsonModel> GetAutoDispatchQueue()
    {
        return await _homeMedService.GetAutoDispatchQueueAsync(GetToken(HttpContext));
    }
} 