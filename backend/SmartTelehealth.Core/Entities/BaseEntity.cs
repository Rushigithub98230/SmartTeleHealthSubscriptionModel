using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTelehealth.Core.Entities;

public abstract class BaseEntity
{
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;

    public int? CreatedBy { get; set; }
    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }

    public int? DeletedBy { get; set; }
    public DateTime? DeletedDate { get; set; }

    [NotMapped]
    [ForeignKey(nameof(CreatedBy))]
    public virtual User CreatedByUser { get; set; }

    [NotMapped]
    [ForeignKey(nameof(UpdatedBy))]
    public virtual User UpdatedByUser { get; set; }

    [NotMapped]
    [ForeignKey(nameof(DeletedBy))]
    public virtual User DeletedByUser { get; set; }
} 