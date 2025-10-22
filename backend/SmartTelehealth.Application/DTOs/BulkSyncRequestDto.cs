namespace SmartTelehealth.Application.DTOs;

/// <summary>
/// Request DTO for bulk Stripe synchronization
/// Phase 5: Stripe Sync Dashboard
/// </summary>
public class BulkSyncRequestDto
{
    /// <summary>
    /// Type of entities to sync (plans, subscriptions, customers)
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// List of entity IDs to synchronize
    /// For plans: Guid strings
    /// For subscriptions: Guid strings
    /// For customers: User IDs
    /// </summary>
    public List<string> Ids { get; set; } = new();

    /// <summary>
    /// Direction of sync (LocalToStripe, StripeToLocal, Both)
    /// </summary>
    public string SyncDirection { get; set; } = "LocalToStripe";

    /// <summary>
    /// Whether to continue syncing if one fails
    /// </summary>
    public bool ContinueOnError { get; set; } = true;

    /// <summary>
    /// Delay between sync operations in milliseconds
    /// Prevents API rate limiting
    /// Default: 200ms
    /// </summary>
    public int DelayBetweenSyncsMs { get; set; } = 200;
}

