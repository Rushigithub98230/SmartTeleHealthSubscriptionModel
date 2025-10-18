using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Application.DTOs;

/// <summary>
/// DTO for a single plan version.
/// </summary>
public class PlanVersionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int VersionNumber { get; set; }
    public bool IsLatestVersion { get; set; }
    public decimal Price { get; set; }
    public decimal CalculatedPrice { get; set; }
    public DateTime VersionCreatedDate { get; set; }
    public int ActiveSubscriptionsCount { get; set; }
    public bool IsAutoCalculatedPrice { get; set; }
}

/// <summary>
/// DTO for plan version history.
/// </summary>
public class PlanVersionHistoryDto
{
    public Guid ParentPlanId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public List<PlanVersionDto> Versions { get; set; } = new();
    public int TotalVersions { get; set; }
    public int TotalActiveSubscriptions { get; set; }
}

/// <summary>
/// DTO for user response to scheduled migration.
/// Healthcare Workflow: User can accept, downgrade, or cancel.
/// </summary>
public class MigrationResponseDto
{
    [Required]
    public Guid SubscriptionId { get; set; }
    
    /// <summary>
    /// User's decision: "Accept", "Downgrade", "Cancel"
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Decision { get; set; } = string.Empty;
    
    /// <summary>
    /// If user chose "Downgrade", the plan they want to downgrade to.
    /// </summary>
    public Guid? DowngradeToPlanId { get; set; }
    
    /// <summary>
    /// Optional reason for the decision.
    /// </summary>
    [MaxLength(500)]
    public string? Reason { get; set; }
}

