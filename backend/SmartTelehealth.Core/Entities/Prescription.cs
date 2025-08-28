using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

public class Prescription : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    public Guid ConsultationId { get; set; }
    public int ProviderId { get; set; }
    public int UserId { get; set; }
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
    
    // Navigation properties
    public virtual Consultation Consultation { get; set; } = null!;
    public virtual Provider Provider { get; set; } = null!;
    public virtual User User { get; set; } = null!;
    public virtual ICollection<PrescriptionItem> Items { get; set; } = new List<PrescriptionItem>();
}

public class PrescriptionItem : BaseEntity
{
    [Key]
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
    
    // Navigation property
    public virtual Prescription Prescription { get; set; } = null!;
}

public class PharmacyIntegration : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    public string PharmacyName { get; set; } = string.Empty;
    public string ApiEndpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    // IsActive is inherited from BaseEntity
    public string Status { get; set; } = string.Empty; // active, inactive, error
    public DateTime LastSyncAt { get; set; }
    public string? LastError { get; set; }
    public string Settings { get; set; } = string.Empty; // JSON string for settings
} 