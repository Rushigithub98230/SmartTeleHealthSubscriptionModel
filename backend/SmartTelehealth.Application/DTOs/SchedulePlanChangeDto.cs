using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Application.DTOs;

/// <summary>
/// DTO for scheduling a plan change (upgrade or downgrade) at the next billing cycle.
/// Used to request a plan change that takes effect without immediate proration.
/// </summary>
public class SchedulePlanChangeDto
{
    /// <summary>
    /// The unique identifier of the new subscription plan to change to.
    /// Must be a valid, active plan ID from the SubscriptionPlans table.
    /// For upgrades: New plan price must be higher than current plan.
    /// For downgrades: New plan price must be lower than current plan.
    /// </summary>
    [Required(ErrorMessage = "New plan ID is required")]
    public Guid NewPlanId { get; set; }
}




