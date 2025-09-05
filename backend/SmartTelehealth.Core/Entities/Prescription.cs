using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

/// <summary>
/// Core prescription entity that manages all prescriptions in the system.
/// This entity handles prescription creation, tracking, and fulfillment management.
/// It serves as the central hub for prescription management, integrating with consultations,
/// providers, users, and pharmacy systems. The entity includes comprehensive prescription
/// tracking, status management, and fulfillment capabilities.
/// </summary>
public class Prescription : BaseEntity
{
    /// <summary>
    /// Primary key identifier for the prescription.
    /// Uses Guid for better scalability and security in distributed systems.
    /// Unique identifier for each prescription in the system.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key reference to the Consultation that resulted in this prescription.
    /// Links this prescription to the specific consultation session.
    /// Required for consultation-prescription relationship management.
    /// </summary>
    public Guid ConsultationId { get; set; }
    
    /// <summary>
    /// Foreign key reference to the Provider who prescribed this medication.
    /// Links this prescription to the specific healthcare provider.
    /// Required for provider-prescription relationship management.
    /// </summary>
    public int ProviderId { get; set; }
    
    /// <summary>
    /// Foreign key reference to the User who is receiving this prescription.
    /// Links this prescription to the specific patient user account.
    /// Required for user-prescription relationship management.
    /// </summary>
    public int UserId { get; set; }
    
    /// <summary>
    /// Current status of this prescription.
    /// Used for prescription status tracking and workflow management.
    /// Possible values: pending, sent, confirmed, dispensed, shipped, delivered.
    /// </summary>
    public string Status { get; set; } = string.Empty; // pending, sent, confirmed, dispensed, shipped, delivered
    
    /// <summary>
    /// Date and time when this prescription was prescribed.
    /// Used for prescription timing tracking and management.
    /// Set when the prescription is created by the provider.
    /// </summary>
    public DateTime PrescribedAt { get; set; }
    
    /// <summary>
    /// Date and time when this prescription was sent to the pharmacy.
    /// Used for prescription workflow tracking and management.
    /// Set when the prescription is transmitted to the pharmacy.
    /// </summary>
    public DateTime? SentToPharmacyAt { get; set; }
    
    /// <summary>
    /// Date and time when this prescription was confirmed by the pharmacy.
    /// Used for prescription workflow tracking and management.
    /// Set when the pharmacy confirms receipt of the prescription.
    /// </summary>
    public DateTime? ConfirmedAt { get; set; }
    
    /// <summary>
    /// Date and time when this prescription was dispensed by the pharmacy.
    /// Used for prescription workflow tracking and management.
    /// Set when the pharmacy dispenses the medication.
    /// </summary>
    public DateTime? DispensedAt { get; set; }
    
    /// <summary>
    /// Date and time when this prescription was shipped.
    /// Used for prescription workflow tracking and management.
    /// Set when the medication is shipped for delivery.
    /// </summary>
    public DateTime? ShippedAt { get; set; }
    
    /// <summary>
    /// Date and time when this prescription was delivered to the patient.
    /// Used for prescription workflow tracking and management.
    /// Set when the medication is successfully delivered.
    /// </summary>
    public DateTime? DeliveredAt { get; set; }
    
    /// <summary>
    /// Reference number or identifier from the pharmacy.
    /// Used for pharmacy integration and prescription tracking.
    /// Set when the prescription is sent to the pharmacy.
    /// </summary>
    public string? PharmacyReference { get; set; }
    
    /// <summary>
    /// Tracking number for prescription delivery.
    /// Used for delivery tracking and shipment monitoring.
    /// Set when the prescription is shipped for delivery.
    /// </summary>
    public string? TrackingNumber { get; set; }
    
    /// <summary>
    /// Additional notes or instructions for this prescription.
    /// Used for prescription documentation and special instructions.
    /// Set when the prescription is created or when notes are added.
    /// </summary>
    public string? Notes { get; set; }
    
    // Navigation properties
    /// <summary>
    /// Navigation property to the Consultation that resulted in this prescription.
    /// Provides access to consultation information for prescription management.
    /// Used for consultation-prescription relationship operations.
    /// </summary>
    public virtual Consultation Consultation { get; set; } = null!;
    
    /// <summary>
    /// Navigation property to the Provider who prescribed this medication.
    /// Provides access to provider information for prescription management.
    /// Used for provider-prescription relationship operations.
    /// </summary>
    public virtual Provider Provider { get; set; } = null!;
    
    /// <summary>
    /// Navigation property to the User who is receiving this prescription.
    /// Provides access to user information for prescription management.
    /// Used for user-prescription relationship operations.
    /// </summary>
    public virtual User User { get; set; } = null!;
    
    /// <summary>
    /// Collection of prescription items for this prescription.
    /// Used for prescription item management and medication tracking.
    /// Includes all medications and items included in this prescription.
    /// </summary>
    public virtual ICollection<PrescriptionItem> Items { get; set; } = new List<PrescriptionItem>();
}

/// <summary>
/// Entity for managing individual prescription items within a prescription.
/// This entity handles medication details, dosage, instructions, and fulfillment tracking.
/// It serves as a detailed breakdown of prescription contents, providing granular tracking
/// for each medication item within a prescription.
/// </summary>
public class PrescriptionItem : BaseEntity
{
    /// <summary>
    /// Primary key identifier for the prescription item.
    /// Uses Guid for better scalability and security in distributed systems.
    /// Unique identifier for each prescription item in the system.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key reference to the Prescription that this item belongs to.
    /// Links this prescription item to the specific prescription.
    /// Required for prescription-item relationship management.
    /// </summary>
    public Guid PrescriptionId { get; set; }
    
    /// <summary>
    /// Name of the medication for this prescription item.
    /// Used for medication identification and prescription management.
    /// Required for prescription item creation and medication tracking.
    /// </summary>
    public string MedicationName { get; set; } = string.Empty;
    
    /// <summary>
    /// Dosage information for this prescription item.
    /// Used for medication dosage tracking and prescription management.
    /// Required for prescription item creation and dosage management.
    /// </summary>
    public string Dosage { get; set; } = string.Empty;
    
    /// <summary>
    /// Instructions for taking this medication.
    /// Used for medication instruction tracking and patient guidance.
    /// Required for prescription item creation and patient care.
    /// </summary>
    public string Instructions { get; set; } = string.Empty;
    
    /// <summary>
    /// Quantity of medication prescribed for this item.
    /// Used for medication quantity tracking and prescription management.
    /// Set when the prescription item is created.
    /// </summary>
    public int Quantity { get; set; }
    
    /// <summary>
    /// Number of refills allowed for this prescription item.
    /// Used for refill tracking and prescription management.
    /// Set when the prescription item is created.
    /// </summary>
    public int Refills { get; set; }
    
    /// <summary>
    /// Current status of this prescription item.
    /// Used for prescription item status tracking and workflow management.
    /// Possible values: pending, dispensed, shipped, delivered.
    /// </summary>
    public string Status { get; set; } = string.Empty; // pending, dispensed, shipped, delivered
    
    /// <summary>
    /// Date and time when this prescription item was dispensed.
    /// Used for prescription item workflow tracking and management.
    /// Set when the medication is dispensed by the pharmacy.
    /// </summary>
    public DateTime? DispensedAt { get; set; }
    
    /// <summary>
    /// Date and time when this prescription item was shipped.
    /// Used for prescription item workflow tracking and management.
    /// Set when the medication is shipped for delivery.
    /// </summary>
    public DateTime? ShippedAt { get; set; }
    
    /// <summary>
    /// Date and time when this prescription item was delivered.
    /// Used for prescription item workflow tracking and management.
    /// Set when the medication is successfully delivered.
    /// </summary>
    public DateTime? DeliveredAt { get; set; }
    
    /// <summary>
    /// Tracking number for this prescription item delivery.
    /// Used for delivery tracking and shipment monitoring.
    /// Set when the prescription item is shipped for delivery.
    /// </summary>
    public string? TrackingNumber { get; set; }
    
    /// <summary>
    /// Additional notes or instructions for this prescription item.
    /// Used for prescription item documentation and special instructions.
    /// Set when the prescription item is created or when notes are added.
    /// </summary>
    public string? Notes { get; set; }
    
    // Navigation property
    /// <summary>
    /// Navigation property to the Prescription that this item belongs to.
    /// Provides access to prescription information for prescription item management.
    /// Used for prescription-item relationship operations.
    /// </summary>
    public virtual Prescription Prescription { get; set; } = null!;
}

/// <summary>
/// Entity for managing pharmacy integrations and API connections.
/// This entity handles pharmacy system integration, API configuration, and sync management.
/// It serves as the central hub for pharmacy integration management, providing configuration
/// and monitoring capabilities for external pharmacy system connections.
/// </summary>
public class PharmacyIntegration : BaseEntity
{
    /// <summary>
    /// Primary key identifier for the pharmacy integration.
    /// Uses Guid for better scalability and security in distributed systems.
    /// Unique identifier for each pharmacy integration in the system.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Name of the pharmacy for this integration.
    /// Used for pharmacy identification and integration management.
    /// Required for pharmacy integration creation and management.
    /// </summary>
    public string PharmacyName { get; set; } = string.Empty;
    
    /// <summary>
    /// API endpoint URL for the pharmacy integration.
    /// Used for pharmacy API communication and integration management.
    /// Required for pharmacy integration configuration and API calls.
    /// </summary>
    public string ApiEndpoint { get; set; } = string.Empty;
    
    /// <summary>
    /// API key for authenticating with the pharmacy system.
    /// Used for pharmacy API authentication and secure communication.
    /// Required for pharmacy integration authentication and API access.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
    
    // IsActive is inherited from BaseEntity
    
    /// <summary>
    /// Current status of this pharmacy integration.
    /// Used for integration status tracking and management.
    /// Possible values: active, inactive, error.
    /// </summary>
    public string Status { get; set; } = string.Empty; // active, inactive, error
    
    /// <summary>
    /// Date and time of the last successful sync with the pharmacy.
    /// Used for integration monitoring and sync tracking.
    /// Updated when successful sync operations are completed.
    /// </summary>
    public DateTime LastSyncAt { get; set; }
    
    /// <summary>
    /// Last error message from the pharmacy integration.
    /// Used for error tracking and troubleshooting.
    /// Set when integration errors occur.
    /// </summary>
    public string? LastError { get; set; }
    
    /// <summary>
    /// JSON string containing additional settings for the pharmacy integration.
    /// Used for integration configuration and custom settings.
    /// Set when integration is configured with additional settings.
    /// </summary>
    public string Settings { get; set; } = string.Empty; // JSON string for settings
} 