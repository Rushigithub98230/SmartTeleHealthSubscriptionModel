using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Application.DTOs;

public class CreatePrivilegeDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public Guid PrivilegeTypeId { get; set; }

    public bool IsActive { get; set; } = true;
}

public class UpdatePrivilegeDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public Guid PrivilegeTypeId { get; set; }

    public bool IsActive { get; set; }
}

public class PrivilegeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid PrivilegeTypeId { get; set; }
    public string PrivilegeTypeName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}
