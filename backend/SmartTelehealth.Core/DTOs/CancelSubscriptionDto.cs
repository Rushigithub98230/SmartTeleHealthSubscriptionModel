using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Core.DTOs
{
    /// <summary>
    /// DTO for cancelling a subscription with optional reason
    /// </summary>
    public class CancelSubscriptionDto
    {
        /// <summary>
        /// The unique identifier of the subscription to cancel
        /// </summary>
        public Guid SubscriptionId { get; set; }

        /// <summary>
        /// Optional reason for cancelling the subscription
        /// </summary>
        public string? Reason { get; set; }
    }
}
