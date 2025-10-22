namespace SmartTelehealth.Application.DTOs;

/// <summary>
/// Request DTO for migrating users to new plan version
/// Phase 6: Plan Version History
/// </summary>
public class MigrateUsersRequestDto
{
    /// <summary>
    /// Migration type (Immediate or Scheduled)
    /// </summary>
    public string MigrationType { get; set; } = "Immediate";

    /// <summary>
    /// Scheduled date for migration (if MigrationType is Scheduled)
    /// </summary>
    public DateTime? ScheduledDate { get; set; }

    /// <summary>
    /// List of specific user IDs to migrate
    /// If empty, migrates all grandfathered users
    /// </summary>
    public List<int> UserIds { get; set; } = new();

    /// <summary>
    /// Whether to send notification to affected users
    /// </summary>
    public bool NotifyUsers { get; set; } = true;

    /// <summary>
    /// Custom notification message
    /// </summary>
    public string? NotificationMessage { get; set; }

    /// <summary>
    /// Reason for migration (for audit trail)
    /// </summary>
    public string? Reason { get; set; }
}

