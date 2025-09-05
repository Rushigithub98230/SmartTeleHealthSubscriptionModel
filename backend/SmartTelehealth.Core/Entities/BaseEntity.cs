using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

/// <summary>
/// Base entity class that provides common audit and soft delete functionality for all entities in the system.
/// This class ensures consistent data tracking, audit trails, and soft delete capabilities across the entire application.
/// All entities inherit from this base class to maintain data integrity and compliance requirements.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Indicates whether the entity is currently active in the system.
    /// Used for soft activation/deactivation without deleting records.
    /// Defaults to true for new entities.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Indicates whether the entity has been soft deleted.
    /// Used for soft delete functionality to maintain data integrity and audit trails.
    /// Records are marked as deleted rather than physically removed from the database.
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Foreign key reference to the User who created this entity.
    /// Used for audit trail and tracking who created each record.
    /// Nullable to handle system-generated records or legacy data.
    /// </summary>
    public int? CreatedBy { get; set; }

    /// <summary>
    /// Timestamp when the entity was created.
    /// Used for audit trail and tracking when records were created.
    /// Automatically set when entity is first saved to database.
    /// </summary>
    public DateTime? CreatedDate { get; set; }

    /// <summary>
    /// Foreign key reference to the User who last updated this entity.
    /// Used for audit trail and tracking who made the most recent changes.
    /// Updated every time the entity is modified.
    /// </summary>
    public int? UpdatedBy { get; set; }

    /// <summary>
    /// Timestamp when the entity was last updated.
    /// Used for audit trail and tracking when records were last modified.
    /// Automatically updated when entity is saved to database.
    /// </summary>
    public DateTime? UpdatedDate { get; set; }

    /// <summary>
    /// Foreign key reference to the User who deleted this entity.
    /// Used for audit trail and tracking who performed the soft delete operation.
    /// Only set when IsDeleted is changed to true.
    /// </summary>
    public int? DeletedBy { get; set; }

    /// <summary>
    /// Timestamp when the entity was soft deleted.
    /// Used for audit trail and tracking when records were deleted.
    /// Set when IsDeleted is changed to true.
    /// </summary>
    public DateTime? DeletedDate { get; set; }

    /// <summary>
    /// Navigation property to the User who created this entity.
    /// Not mapped to database - used for eager loading user details when needed.
    /// Provides access to creator's information for display purposes.
    /// </summary>
    [NotMapped]
    [ForeignKey(nameof(CreatedBy))]
    public virtual User CreatedByUser { get; set; }

    /// <summary>
    /// Navigation property to the User who last updated this entity.
    /// Not mapped to database - used for eager loading user details when needed.
    /// Provides access to updater's information for display purposes.
    /// </summary>
    [NotMapped]
    [ForeignKey(nameof(UpdatedBy))]
    public virtual User UpdatedByUser { get; set; }

    /// <summary>
    /// Navigation property to the User who deleted this entity.
    /// Not mapped to database - used for eager loading user details when needed.
    /// Provides access to deleter's information for audit purposes.
    /// </summary>
    [NotMapped]
    [ForeignKey(nameof(DeletedBy))]
    public virtual User DeletedByUser { get; set; }
} 