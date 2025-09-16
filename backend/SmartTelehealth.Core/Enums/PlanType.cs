namespace SmartTelehealth.Core.Enums;

/// <summary>
/// Enumeration defining the different types of subscription plans and their billing models.
/// This enum is used to categorize subscription plans and determine appropriate billing logic.
/// </summary>
public enum PlanType
{
    /// <summary>
    /// Standard fixed-price subscription plan with predefined limits and features.
    /// Users pay a fixed amount regardless of usage within the plan limits.
    /// </summary>
    Standard = 0,

    /// <summary>
    /// Usage-based subscription plan where users pay based on actual consumption.
    /// Includes base features with additional charges for overages beyond plan limits.
    /// </summary>
    UsageBased = 1,

    /// <summary>
    /// Premium subscription plan with additional service charges and premium features.
    /// Includes enhanced services and may have percentage-based service fees.
    /// </summary>
    Premium = 2,

    /// <summary>
    /// Enterprise subscription plan with custom pricing and features.
    /// Typically used for large organizations with specific requirements.
    /// </summary>
    Enterprise = 3
}

