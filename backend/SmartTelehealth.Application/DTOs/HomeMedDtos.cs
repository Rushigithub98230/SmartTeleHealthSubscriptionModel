namespace SmartTelehealth.Application.DTOs;

public class PrescriptionDto
{
    public Guid Id { get; set; }
    public Guid ConsultationId { get; set; }
    public Guid ProviderId { get; set; }
    public Guid UserId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // pending, sent, confirmed, dispensed, shipped, delivered
    public DateTime PrescribedAt { get; set; }
    public DateTime? SentToPharmacyAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? DispensedAt { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string? PharmacyReference { get; set; }
    public string? TrackingNumber { get; set; }
    public string? Notes { get; set; }
    public List<PrescriptionItemDto> Items { get; set; } = new List<PrescriptionItemDto>();
}

public class PrescriptionItemDto
{
    public Guid Id { get; set; }
    public Guid PrescriptionId { get; set; }
    public string MedicationName { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int Refills { get; set; }
    public string Status { get; set; } = string.Empty; // pending, dispensed, shipped, delivered
    public DateTime? DispensedAt { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string? TrackingNumber { get; set; }
    public string? Notes { get; set; }
}

public class CreatePrescriptionDto
{
    public Guid ConsultationId { get; set; }
    public Guid ProviderId { get; set; }
    public Guid UserId { get; set; }
    public List<CreatePrescriptionItemDto> Items { get; set; } = new List<CreatePrescriptionItemDto>();
    public string? Notes { get; set; }
    public bool SendToPharmacy { get; set; } = true;
}

public class CreatePrescriptionItemDto
{
    public string MedicationName { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int Refills { get; set; } = 0;
    public string? Notes { get; set; }
}

public class UpdatePrescriptionDto
{
    public string Status { get; set; } = string.Empty;
    public string? PharmacyReference { get; set; }
    public string? TrackingNumber { get; set; }
    public string? Notes { get; set; }
}

public class MedicationShipmentDto
{
    public Guid Id { get; set; }
    public Guid PrescriptionId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // pending, processing, shipped, delivered, returned
    public string TrackingNumber { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public string ShippingMethod { get; set; } = string.Empty; // standard, express, overnight
    public DateTime CreatedDate { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? EstimatedDelivery { get; set; }
    public string? Carrier { get; set; }
    public string? Notes { get; set; }
    public List<MedicationShipmentItemDto> Items { get; set; } = new List<MedicationShipmentItemDto>();
}

public class MedicationShipmentItemDto
{
    public Guid Id { get; set; }
    public Guid ShipmentId { get; set; }
    public Guid PrescriptionItemId { get; set; }
    public string MedicationName { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Status { get; set; } = string.Empty; // pending, shipped, delivered
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string? Notes { get; set; }
}

public class CreateMedicationShipmentDto
{
    public Guid PrescriptionId { get; set; }
    public Guid UserId { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public string ShippingMethod { get; set; } = "standard";
    public DateTime? EstimatedDelivery { get; set; }
    public string? Notes { get; set; }
}

public class UpdateMedicationShipmentDto
{
    public string Status { get; set; } = string.Empty;
    public string? TrackingNumber { get; set; }
    public string? Carrier { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? EstimatedDelivery { get; set; }
    public string? Notes { get; set; }
}

public class PharmacyIntegrationDto
{
    public Guid Id { get; set; }
    public string PharmacyName { get; set; } = string.Empty;
    public string ApiEndpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string Status { get; set; } = string.Empty; // active, inactive, error
    public DateTime LastSyncAt { get; set; }
    public string? LastError { get; set; }
    public Dictionary<string, string> Settings { get; set; } = new Dictionary<string, string>();
}

public class PrescriptionAnalyticsDto
{
    public int TotalPrescriptions { get; set; }
    public int PendingPrescriptions { get; set; }
    public int SentPrescriptions { get; set; }
    public int ConfirmedPrescriptions { get; set; }
    public int DispensedPrescriptions { get; set; }
    public int ShippedPrescriptions { get; set; }
    public int DeliveredPrescriptions { get; set; }
    public decimal AverageProcessingTime { get; set; }
    public decimal AverageShippingTime { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<PrescriptionTrendDto> Trends { get; set; } = new List<PrescriptionTrendDto>();
}

public class PrescriptionTrendDto
{
    public DateTime Date { get; set; }
    public int PrescriptionsCreated { get; set; }
    public int PrescriptionsSent { get; set; }
    public int PrescriptionsDelivered { get; set; }
    public decimal AverageProcessingTime { get; set; }
}

public class MedicationShipmentAnalyticsDto
{
    public int TotalShipments { get; set; }
    public int PendingShipments { get; set; }
    public int ProcessingShipments { get; set; }
    public int ShippedShipments { get; set; }
    public int DeliveredShipments { get; set; }
    public int ReturnedShipments { get; set; }
    public decimal AverageShippingTime { get; set; }
    public decimal OnTimeDeliveryRate { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<ShipmentTrendDto> Trends { get; set; } = new List<ShipmentTrendDto>();
}

public class ShipmentTrendDto
{
    public DateTime Date { get; set; }
    public int ShipmentsCreated { get; set; }
    public int ShipmentsShipped { get; set; }
    public int ShipmentsDelivered { get; set; }
    public decimal AverageShippingTime { get; set; }
} 