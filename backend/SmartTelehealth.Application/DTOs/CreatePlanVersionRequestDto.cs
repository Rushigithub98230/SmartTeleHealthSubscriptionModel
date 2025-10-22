namespace SmartTelehealth.Application.DTOs;

/// <summary>
/// Request DTO for creating a new plan version
/// Phase 6: Plan Version History
/// </summary>
public class CreatePlanVersionRequestDto
{
    /// <summary>
    /// List of changes made in this version
    /// Example: ["Increased consultations from 10 to 15", "Added premium support"]
    /// </summary>
    public List<string> Changes { get; set; } = new();

    /// <summary>
    /// Reason for creating new version
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Whether to grandfather existing subscribers
    /// Default: true (existing users keep old version)
    /// </summary>
    public bool GrandfatherExistingSubscribers { get; set; } = true;
}

