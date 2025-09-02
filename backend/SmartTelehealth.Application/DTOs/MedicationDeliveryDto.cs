using System;

namespace SmartTelehealth.Application.DTOs;

public class MedicationDeliveryDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public Guid SubscriptionId { get; set; }
    public string SubscriptionName { get; set; } = string.Empty;
    public string MedicationName { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime EstimatedDeliveryDate { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}

public class CreateMedicationDeliveryDto
{
    public string PatientId { get; set; } = string.Empty;
    public string? SubscriptionId { get; set; }
    public string MedicationName { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string DeliveryAddress { get; set; } = string.Empty;
    public DateTime ScheduledDeliveryDate { get; set; }
    public string? PrescriptionNotes { get; set; }
    public decimal DeliveryFee { get; set; }
}

public class UpdateMedicationDeliveryDto
{
    public string? MedicationName { get; set; }
    public string? Dosage { get; set; }
    public string? Instructions { get; set; }
    public int? Quantity { get; set; }
    public string? DeliveryAddress { get; set; }
    public DateTime? ScheduledDeliveryDate { get; set; }
    public string? PrescriptionNotes { get; set; }
    public decimal? DeliveryFee { get; set; }
    public string? DeliveryStatus { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string? DeliveryNotes { get; set; }
    public string? TrackingNumber { get; set; }
} 