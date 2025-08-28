using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;

namespace SmartTelehealth.Application.Services;

public class SubscriptionLifecycleService : ISubscriptionLifecycleService
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly ISubscriptionStatusHistoryRepository _statusHistoryRepository;
    private readonly IAuditService _auditService;
    private readonly ILogger<SubscriptionLifecycleService> _logger;

    public SubscriptionLifecycleService(
        ISubscriptionRepository subscriptionRepository,
        ISubscriptionStatusHistoryRepository statusHistoryRepository,
        IAuditService auditService,
        ILogger<SubscriptionLifecycleService> logger)
    {
        _subscriptionRepository = subscriptionRepository;
        _statusHistoryRepository = statusHistoryRepository;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<bool> ActivateSubscriptionAsync(Guid subscriptionId, string? reason = null, TokenModel tokenModel = null)
    {
        try
        {
            _logger.LogInformation("Activating subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            if (!await ValidateStatusTransitionAsync(subscription.Status, "Active", tokenModel))
            {
                _logger.LogWarning("Invalid status transition from {CurrentStatus} to Active for subscription {SubscriptionId} by user {UserId}", 
                    subscription.Status, subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            var oldStatus = subscription.Status;
            subscription.Status = "Active";
            subscription.UpdatedAt = DateTime.UtcNow;

            // Add status history
            await _statusHistoryRepository.CreateAsync(new SubscriptionStatusHistory
            {
                SubscriptionId = subscriptionId,
                FromStatus = oldStatus,
                ToStatus = "Active",
                Reason = reason ?? "Subscription activated",
                ChangedAt = DateTime.UtcNow,
                ChangedByUserId = tokenModel?.UserID
            });

            await _subscriptionRepository.UpdateAsync(subscription);
            
            // Log audit trail
            if (tokenModel != null)
            {
                await _auditService.LogActionAsync("Subscription", "Activate", subscriptionId.ToString(), 
                    $"Subscription activated from {oldStatus} to Active. Reason: {reason}", tokenModel);
            }
            
            _logger.LogInformation("Successfully activated subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            return false;
        }
    }

    public async Task<bool> PauseSubscriptionAsync(Guid subscriptionId, string? reason = null, TokenModel tokenModel = null)
    {
        try
        {
            _logger.LogInformation("Pausing subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            if (!await ValidateStatusTransitionAsync(subscription.Status, "Paused", tokenModel))
            {
                _logger.LogWarning("Invalid status transition from {CurrentStatus} to Paused for subscription {SubscriptionId} by user {UserId}", 
                    subscription.Status, subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            var oldStatus = subscription.Status;
            subscription.Status = "Paused";
            subscription.UpdatedAt = DateTime.UtcNow;

            // Add status history
            await _statusHistoryRepository.CreateAsync(new SubscriptionStatusHistory
            {
                SubscriptionId = subscriptionId,
                FromStatus = oldStatus,
                ToStatus = "Paused",
                Reason = reason ?? "Subscription paused",
                ChangedAt = DateTime.UtcNow,
                ChangedByUserId = tokenModel?.UserID
            });

            await _subscriptionRepository.UpdateAsync(subscription);
            
            // Log audit trail
            if (tokenModel != null)
            {
                await _auditService.LogActionAsync("Subscription", "Pause", subscriptionId.ToString(), 
                    $"Subscription paused from {oldStatus} to Paused. Reason: {reason}", tokenModel);
            }
            
            _logger.LogInformation("Successfully paused subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pausing subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            return false;
        }
    }

    public async Task<bool> ResumeSubscriptionAsync(Guid subscriptionId, string? reason = null, TokenModel tokenModel = null)
    {
        try
        {
            _logger.LogInformation("Resuming subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            if (!await ValidateStatusTransitionAsync(subscription.Status, "Active", tokenModel))
            {
                _logger.LogWarning("Invalid status transition from {CurrentStatus} to Active for subscription {SubscriptionId} by user {UserId}", 
                    subscription.Status, subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            var oldStatus = subscription.Status;
            subscription.Status = "Active";
            subscription.UpdatedAt = DateTime.UtcNow;

            // Add status history
            await _statusHistoryRepository.CreateAsync(new SubscriptionStatusHistory
            {
                SubscriptionId = subscriptionId,
                FromStatus = oldStatus,
                ToStatus = "Active",
                Reason = reason ?? "Subscription resumed",
                ChangedAt = DateTime.UtcNow,
                ChangedByUserId = tokenModel?.UserID
            });

            await _subscriptionRepository.UpdateAsync(subscription);
            
            // Log audit trail
            if (tokenModel != null)
            {
                await _auditService.LogActionAsync("Subscription", "Resume", subscriptionId.ToString(), 
                    $"Subscription resumed from {oldStatus} to Active. Reason: {reason}", tokenModel);
            }
            
            _logger.LogInformation("Successfully resumed subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resuming subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            return false;
        }
    }

    public async Task<bool> CancelSubscriptionAsync(Guid subscriptionId, string? reason = null, TokenModel tokenModel = null)
    {
        try
        {
            _logger.LogInformation("Cancelling subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            if (!await ValidateStatusTransitionAsync(subscription.Status, "Cancelled", tokenModel))
            {
                _logger.LogWarning("Invalid status transition from {CurrentStatus} to Cancelled for subscription {SubscriptionId} by user {UserId}", 
                    subscription.Status, subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            var oldStatus = subscription.Status;
            subscription.Status = "Cancelled";
            subscription.UpdatedAt = DateTime.UtcNow;
            subscription.CancelledAt = DateTime.UtcNow;

            // Add status history
            await _statusHistoryRepository.CreateAsync(new SubscriptionStatusHistory
            {
                SubscriptionId = subscriptionId,
                FromStatus = oldStatus,
                ToStatus = "Cancelled",
                Reason = reason ?? "Subscription cancelled",
                ChangedAt = DateTime.UtcNow,
                ChangedByUserId = tokenModel?.UserID
            });

            await _subscriptionRepository.UpdateAsync(subscription);
            
            // Log audit trail
            if (tokenModel != null)
            {
                await _auditService.LogActionAsync("Subscription", "Cancel", subscriptionId.ToString(), 
                    $"Subscription cancelled from {oldStatus} to Cancelled. Reason: {reason}", tokenModel);
            }
            
            _logger.LogInformation("Successfully cancelled subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            return false;
        }
    }

    public async Task<bool> SuspendSubscriptionAsync(Guid subscriptionId, string? reason = null, TokenModel tokenModel = null)
    {
        try
        {
            _logger.LogInformation("Suspending subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            if (!await ValidateStatusTransitionAsync(subscription.Status, "Suspended", tokenModel))
            {
                _logger.LogWarning("Invalid status transition from {CurrentStatus} to Suspended for subscription {SubscriptionId} by user {UserId}", 
                    subscription.Status, subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            var oldStatus = subscription.Status;
            subscription.Status = "Suspended";
            subscription.UpdatedAt = DateTime.UtcNow;

            // Add status history
            await _statusHistoryRepository.CreateAsync(new SubscriptionStatusHistory
            {
                SubscriptionId = subscriptionId,
                FromStatus = oldStatus,
                ToStatus = "Suspended",
                Reason = reason ?? "Subscription suspended",
                ChangedAt = DateTime.UtcNow,
                ChangedByUserId = tokenModel?.UserID
            });

            await _subscriptionRepository.UpdateAsync(subscription);
            
            // Log audit trail
            if (tokenModel != null)
            {
                await _auditService.LogActionAsync("Subscription", "Suspend", subscriptionId.ToString(), 
                    $"Subscription suspended from {oldStatus} to Suspended. Reason: {reason}", tokenModel);
            }
            
            _logger.LogInformation("Successfully suspended subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error suspending subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            return false;
        }
    }

    public async Task<bool> RenewSubscriptionAsync(Guid subscriptionId, string? reason = null, TokenModel tokenModel = null)
    {
        try
        {
            _logger.LogInformation("Renewing subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            if (!await ValidateStatusTransitionAsync(subscription.Status, "Active", tokenModel))
            {
                _logger.LogWarning("Invalid status transition from {CurrentStatus} to Active for subscription {SubscriptionId} by user {UserId}", 
                    subscription.Status, subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            var oldStatus = subscription.Status;
            subscription.Status = "Active";
            subscription.UpdatedAt = DateTime.UtcNow;
            subscription.RenewedAt = DateTime.UtcNow;

            // Add status history
            await _statusHistoryRepository.CreateAsync(new SubscriptionStatusHistory
            {
                SubscriptionId = subscriptionId,
                FromStatus = oldStatus,
                ToStatus = "Active",
                Reason = reason ?? "Subscription renewed",
                ChangedAt = DateTime.UtcNow,
                ChangedByUserId = tokenModel?.UserID
            });

            await _subscriptionRepository.UpdateAsync(subscription);
            
            // Log audit trail
            if (tokenModel != null)
            {
                await _auditService.LogActionAsync("Subscription", "Renew", subscriptionId.ToString(), 
                    $"Subscription renewed from {oldStatus} to Active. Reason: {reason}", tokenModel);
            }
            
            _logger.LogInformation("Successfully renewed subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error renewing subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            return false;
        }
    }

    public async Task<bool> ExpireSubscriptionAsync(Guid subscriptionId, string? reason = null, TokenModel tokenModel = null)
    {
        try
        {
            _logger.LogInformation("Expiring subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            if (!await ValidateStatusTransitionAsync(subscription.Status, "Expired", tokenModel))
            {
                _logger.LogWarning("Invalid status transition from {CurrentStatus} to Expired for subscription {SubscriptionId} by user {UserId}", 
                    subscription.Status, subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            var oldStatus = subscription.Status;
            subscription.Status = "Expired";
            subscription.UpdatedAt = DateTime.UtcNow;
            subscription.ExpiredAt = DateTime.UtcNow;

            // Add status history
            await _statusHistoryRepository.CreateAsync(new SubscriptionStatusHistory
            {
                SubscriptionId = subscriptionId,
                FromStatus = oldStatus,
                ToStatus = "Expired",
                Reason = reason ?? "Subscription expired",
                ChangedAt = DateTime.UtcNow,
                ChangedByUserId = tokenModel?.UserID
            });

            await _subscriptionRepository.UpdateAsync(subscription);
            
            // Log audit trail
            if (tokenModel != null)
            {
                await _auditService.LogActionAsync("Subscription", "Expire", subscriptionId.ToString(), 
                    $"Subscription expired from {oldStatus} to Expired. Reason: {reason}", tokenModel);
            }
            
            _logger.LogInformation("Successfully expired subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error expiring subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            return false;
        }
    }

    public async Task<bool> MarkPaymentFailedAsync(Guid subscriptionId, string? reason = null, TokenModel tokenModel = null)
    {
        try
        {
            _logger.LogInformation("Marking payment failed for subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            if (!await ValidateStatusTransitionAsync(subscription.Status, "PaymentFailed", tokenModel))
            {
                _logger.LogWarning("Invalid status transition from {CurrentStatus} to PaymentFailed for subscription {SubscriptionId} by user {UserId}", 
                    subscription.Status, subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            var oldStatus = subscription.Status;
            subscription.Status = "PaymentFailed";
            subscription.UpdatedAt = DateTime.UtcNow;

            // Add status history
            await _statusHistoryRepository.CreateAsync(new SubscriptionStatusHistory
            {
                SubscriptionId = subscriptionId,
                FromStatus = oldStatus,
                ToStatus = "PaymentFailed",
                Reason = reason ?? "Payment failed",
                ChangedAt = DateTime.UtcNow,
                ChangedByUserId = tokenModel?.UserID
            });

            await _subscriptionRepository.UpdateAsync(subscription);
            
            // Log audit trail
            if (tokenModel != null)
            {
                await _auditService.LogActionAsync("Subscription", "PaymentFailed", subscriptionId.ToString(), 
                    $"Subscription payment failed from {oldStatus} to PaymentFailed. Reason: {reason}", tokenModel);
            }
            
            _logger.LogInformation("Successfully marked payment failed for subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking payment failed for subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            return false;
        }
    }

    public async Task<bool> MarkPaymentSucceededAsync(Guid subscriptionId, string? reason = null, TokenModel tokenModel = null)
    {
        try
        {
            _logger.LogInformation("Marking payment succeeded for subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            if (!await ValidateStatusTransitionAsync(subscription.Status, "Active", tokenModel))
            {
                _logger.LogWarning("Invalid status transition from {CurrentStatus} to Active for subscription {SubscriptionId} by user {UserId}", 
                    subscription.Status, subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            var oldStatus = subscription.Status;
            subscription.Status = "Active";
            subscription.UpdatedAt = DateTime.UtcNow;

            // Add status history
            await _statusHistoryRepository.CreateAsync(new SubscriptionStatusHistory
            {
                SubscriptionId = subscriptionId,
                FromStatus = oldStatus,
                ToStatus = "Active",
                Reason = reason ?? "Payment succeeded",
                ChangedAt = DateTime.UtcNow,
                ChangedByUserId = tokenModel?.UserID
            });

            await _subscriptionRepository.UpdateAsync(subscription);
            
            // Log audit trail
            if (tokenModel != null)
            {
                await _auditService.LogActionAsync("Subscription", "PaymentSucceeded", subscriptionId.ToString(), 
                    $"Subscription payment succeeded from {oldStatus} to Active. Reason: {reason}", tokenModel);
            }
            
            _logger.LogInformation("Successfully marked payment succeeded for subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking payment succeeded for subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            return false;
        }
    }

    public async Task<bool> UpdateSubscriptionStatusAsync(Guid subscriptionId, string newStatus, string? reason = null, TokenModel tokenModel = null)
    {
        try
        {
            _logger.LogInformation("Updating subscription {SubscriptionId} status to {NewStatus} by user {UserId}", 
                subscriptionId, newStatus, tokenModel?.UserID ?? 0);
            
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            if (!await ValidateStatusTransitionAsync(subscription.Status, newStatus, tokenModel))
            {
                _logger.LogWarning("Invalid status transition from {CurrentStatus} to {NewStatus} for subscription {SubscriptionId} by user {UserId}", 
                    subscription.Status, newStatus, subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            var oldStatus = subscription.Status;
            subscription.Status = newStatus;
            subscription.UpdatedAt = DateTime.UtcNow;

            // Add status history
            await _statusHistoryRepository.CreateAsync(new SubscriptionStatusHistory
            {
                SubscriptionId = subscriptionId,
                FromStatus = oldStatus,
                ToStatus = newStatus,
                Reason = reason ?? $"Status updated to {newStatus}",
                ChangedAt = DateTime.UtcNow,
                ChangedByUserId = tokenModel?.UserID
            });

            await _subscriptionRepository.UpdateAsync(subscription);
            
            // Log audit trail
            if (tokenModel != null)
            {
                await _auditService.LogActionAsync("Subscription", "StatusUpdate", subscriptionId.ToString(), 
                    $"Subscription status updated from {oldStatus} to {newStatus}. Reason: {reason}", tokenModel);
            }
            
            _logger.LogInformation("Successfully updated subscription {SubscriptionId} status to {NewStatus} by user {UserId}", 
                subscriptionId, newStatus, tokenModel?.UserID ?? 0);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating subscription {SubscriptionId} status to {NewStatus} by user {UserId}", 
                subscriptionId, newStatus, tokenModel?.UserID ?? 0);
            return false;
        }
    }

    public async Task<IEnumerable<SubscriptionStatusHistory>> GetStatusHistoryAsync(Guid subscriptionId, TokenModel tokenModel = null)
    {
        try
        {
            var history = await _statusHistoryRepository.GetBySubscriptionIdAsync(subscriptionId);
            
            _logger.LogInformation("Status history retrieved for subscription {SubscriptionId} by user {UserId}: {HistoryCount} records", 
                subscriptionId, tokenModel?.UserID ?? 0, history.Count());
            return history;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving status history for subscription {SubscriptionId} by user {UserId}", 
                subscriptionId, tokenModel?.UserID ?? 0);
            return Enumerable.Empty<SubscriptionStatusHistory>();
        }
    }

    public async Task<bool> ValidateStatusTransitionAsync(string currentStatus, string newStatus, TokenModel tokenModel = null)
    {
        try
        {
            // Define valid status transitions
            var validTransitions = new Dictionary<string, List<string>>
            {
                ["Pending"] = new List<string> { "Active", "Cancelled", "Expired" },
                ["Active"] = new List<string> { "Paused", "Suspended", "Cancelled", "Expired", "PaymentFailed" },
                ["Paused"] = new List<string> { "Active", "Cancelled", "Expired" },
                ["Suspended"] = new List<string> { "Active", "Cancelled", "Expired" },
                ["PaymentFailed"] = new List<string> { "Active", "Cancelled", "Expired" },
                ["Expired"] = new List<string> { "Active", "Cancelled" },
                ["Cancelled"] = new List<string> { "Active" } // Reactivation
            };

            if (validTransitions.ContainsKey(currentStatus) && validTransitions[currentStatus].Contains(newStatus))
            {
                _logger.LogInformation("Status transition from {CurrentStatus} to {NewStatus} validated by user {UserId}", 
                    currentStatus, newStatus, tokenModel?.UserID ?? 0);
                return true;
            }

            _logger.LogWarning("Invalid status transition from {CurrentStatus} to {NewStatus} by user {UserId}", 
                currentStatus, newStatus, tokenModel?.UserID ?? 0);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating status transition from {CurrentStatus} to {NewStatus} by user {UserId}", 
                currentStatus, newStatus, tokenModel?.UserID ?? 0);
            return false;
        }
    }

    public async Task<string> GetNextValidStatusAsync(string currentStatus, TokenModel tokenModel = null)
    {
        try
        {
            // Define valid next statuses for each current status
            var nextStatuses = new Dictionary<string, List<string>>
            {
                ["Pending"] = new List<string> { "Active", "Cancelled", "Expired" },
                ["Active"] = new List<string> { "Paused", "Suspended", "Cancelled", "Expired", "PaymentFailed" },
                ["Paused"] = new List<string> { "Active", "Cancelled", "Expired" },
                ["Suspended"] = new List<string> { "Active", "Cancelled", "Expired" },
                ["PaymentFailed"] = new List<string> { "Active", "Cancelled", "Expired" },
                ["Expired"] = new List<string> { "Active", "Cancelled" },
                ["Cancelled"] = new List<string> { "Active" }
            };

            if (nextStatuses.ContainsKey(currentStatus))
            {
                var nextStatus = nextStatuses[currentStatus].FirstOrDefault() ?? "No valid next status";
                _logger.LogInformation("Next valid status for {CurrentStatus} determined by user {UserId}: {NextStatus}", 
                    currentStatus, tokenModel?.UserID ?? 0, nextStatus);
                return nextStatus;
            }

            _logger.LogWarning("No valid next status found for {CurrentStatus} by user {UserId}", 
                currentStatus, tokenModel?.UserID ?? 0);
            return "No valid next status";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error determining next valid status for {CurrentStatus} by user {UserId}", 
                currentStatus, tokenModel?.UserID ?? 0);
            return "Error determining next status";
        }
    }

    /// <summary>
    /// Process subscription lifecycle state transitions
    /// </summary>
    public async Task<JsonModel> ProcessStateTransitionAsync(string subscriptionId, string newStatus, string reason = null, string changedByUserId = null, TokenModel tokenModel = null)
    {
        try
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (subscription == null)
                return new JsonModel
                {
                    data = new object(),
                    Message = "Subscription not found",
                    StatusCode = 404
                };

            var oldStatus = subscription.Status;

            // Validate state transition
            var validationResult = ValidateStateTransition(oldStatus, newStatus);
            if (!validationResult.IsValid)
                return new JsonModel
                {
                    data = new object(),
                    Message = validationResult.ErrorMessage,
                    StatusCode = 400
                };

            // Update subscription status
            subscription.Status = newStatus;
            subscription.UpdatedDate = DateTime.UtcNow;

            // Update status-specific properties
            await UpdateStatusSpecificPropertiesAsync(subscription, newStatus, reason);

            // Add status history
            await _subscriptionRepository.AddStatusHistoryAsync(new SubscriptionStatusHistory
            {
                SubscriptionId = subscription.Id,
                FromStatus = oldStatus,
                ToStatus = newStatus,
                Reason = reason,
                ChangedByUserId = !string.IsNullOrEmpty(changedByUserId) ? int.Parse(changedByUserId) : null,
                ChangedAt = DateTime.UtcNow
            });

            await _subscriptionRepository.UpdateAsync(subscription);

            // Audit log
            await _auditService.LogUserActionAsync(
                !string.IsNullOrEmpty(changedByUserId) ? int.Parse(changedByUserId) : 0,
                "SubscriptionStateChange",
                "Subscription",
                subscriptionId,
                $"Status changed from {oldStatus} to {newStatus}: {reason}",
                tokenModel
            );

            _logger.LogInformation("Subscription {SubscriptionId} state changed from {OldStatus} to {NewStatus}", 
                subscriptionId, oldStatus, newStatus);

            return new JsonModel
            {
                data = true,
                Message = "State transition processed successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing state transition for subscription {SubscriptionId}", subscriptionId);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to process state transition",
                StatusCode = 500
            };
        }
    }

    /// <summary>
    /// Validate if a state transition is allowed
    /// </summary>
    private StateTransitionValidation ValidateStateTransition(string currentStatus, string newStatus)
    {
        var allowedTransitions = GetAllowedTransitions();
        
        if (allowedTransitions.TryGetValue(currentStatus, out var allowedStates))
        {
            if (allowedStates.Contains(newStatus))
            {
                return new StateTransitionValidation { IsValid = true };
            }
        }

        return new StateTransitionValidation 
        { 
            IsValid = false, 
            ErrorMessage = $"Invalid state transition from {currentStatus} to {newStatus}" 
        };
    }

    /// <summary>
    /// Get allowed state transitions
    /// </summary>
    private Dictionary<string, HashSet<string>> GetAllowedTransitions()
    {
        return new Dictionary<string, HashSet<string>>
        {
            [Subscription.SubscriptionStatuses.Pending] = new HashSet<string>
            {
                Subscription.SubscriptionStatuses.Active,
                Subscription.SubscriptionStatuses.TrialActive,
                Subscription.SubscriptionStatuses.Cancelled
            },
            [Subscription.SubscriptionStatuses.Active] = new HashSet<string>
            {
                Subscription.SubscriptionStatuses.Paused,
                Subscription.SubscriptionStatuses.Cancelled,
                Subscription.SubscriptionStatuses.PaymentFailed,
                Subscription.SubscriptionStatuses.Expired
            },
            [Subscription.SubscriptionStatuses.Paused] = new HashSet<string>
            {
                Subscription.SubscriptionStatuses.Active,
                Subscription.SubscriptionStatuses.Cancelled,
                Subscription.SubscriptionStatuses.Expired
            },
            [Subscription.SubscriptionStatuses.PaymentFailed] = new HashSet<string>
            {
                Subscription.SubscriptionStatuses.Active,
                Subscription.SubscriptionStatuses.Cancelled,
                Subscription.SubscriptionStatuses.Suspended
            },
            [Subscription.SubscriptionStatuses.Suspended] = new HashSet<string>
            {
                Subscription.SubscriptionStatuses.Active,
                Subscription.SubscriptionStatuses.Cancelled
            },
            [Subscription.SubscriptionStatuses.TrialActive] = new HashSet<string>
            {
                Subscription.SubscriptionStatuses.Active,
                Subscription.SubscriptionStatuses.TrialExpired,
                Subscription.SubscriptionStatuses.Cancelled
            },
            [Subscription.SubscriptionStatuses.TrialExpired] = new HashSet<string>
            {
                Subscription.SubscriptionStatuses.Active,
                Subscription.SubscriptionStatuses.Cancelled
            },
            [Subscription.SubscriptionStatuses.Cancelled] = new HashSet<string>
            {
                // No valid transitions from Cancelled (matches Subscription entity behavior)
            },
            [Subscription.SubscriptionStatuses.Expired] = new HashSet<string>
            {
                Subscription.SubscriptionStatuses.Active // Allow reactivation
            }
        };
    }

    /// <summary>
    /// Update status-specific properties
    /// </summary>
    private async Task UpdateStatusSpecificPropertiesAsync(Subscription subscription, string newStatus, string reason)
    {
        switch (newStatus)
        {
            case Subscription.SubscriptionStatuses.Active:
                subscription.ResumedDate = DateTime.UtcNow;
                subscription.PauseReason = null;
                subscription.CancellationReason = null;
                break;

            case Subscription.SubscriptionStatuses.Paused:
                subscription.PausedDate = DateTime.UtcNow;
                subscription.PauseReason = reason;
                break;

            case Subscription.SubscriptionStatuses.Cancelled:
                subscription.CancelledDate = DateTime.UtcNow;
                subscription.CancellationReason = reason;
                subscription.AutoRenew = false;
                break;

            case Subscription.SubscriptionStatuses.PaymentFailed:
                subscription.LastPaymentFailedDate = DateTime.UtcNow;
                subscription.LastPaymentError = reason;
                break;

            case Subscription.SubscriptionStatuses.Suspended:
                subscription.SuspendedDate = DateTime.UtcNow;
                break;

            case Subscription.SubscriptionStatuses.Expired:
                subscription.ExpirationDate = DateTime.UtcNow;
                break;

            case Subscription.SubscriptionStatuses.TrialExpired:
                subscription.TrialEndDate = DateTime.UtcNow;
                break;
        }
    }

    /// <summary>
    /// Process subscription expiration
    /// </summary>
    public async Task<JsonModel> ProcessSubscriptionExpirationAsync(string subscriptionId)
    {
        try
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (subscription == null)
                return new JsonModel
                {
                    data = new object(),
                    Message = "Subscription not found",
                    StatusCode = 404
                };

            if (subscription.Status == Subscription.SubscriptionStatuses.Active && 
                subscription.NextBillingDate <= DateTime.UtcNow)
            {
                return await ProcessStateTransitionAsync(
                    subscriptionId, 
                    Subscription.SubscriptionStatuses.Expired, 
                    "Subscription expired due to non-payment"
                );
            }

            return new JsonModel
            {
                data = true,
                Message = "Subscription is not due for expiration",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing subscription expiration for {SubscriptionId}", subscriptionId);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to process subscription expiration",
                StatusCode = 500
            };
        }
    }

    /// <summary>
    /// Process trial expiration
    /// </summary>
    public async Task<JsonModel> ProcessTrialExpirationAsync(string subscriptionId)
    {
        try
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (subscription == null)
                return new JsonModel
                {
                    data = new object(),
                    Message = "Subscription not found",
                    StatusCode = 404
                };

            if (subscription.Status == Subscription.SubscriptionStatuses.TrialActive && 
                subscription.TrialEndDate <= DateTime.UtcNow)
            {
                return await ProcessStateTransitionAsync(
                    subscriptionId, 
                    Subscription.SubscriptionStatuses.TrialExpired, 
                    "Trial period expired"
                );
            }

            return new JsonModel
            {
                data = true,
                Message = "Trial is not due for expiration",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing trial expiration for {SubscriptionId}", subscriptionId);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to process trial expiration",
                StatusCode = 500
            };
        }
    }

    /// <summary>
    /// Reactivate a cancelled or expired subscription
    /// </summary>
    public async Task<JsonModel> ReactivateSubscriptionAsync(string subscriptionId, string reason = null)
    {
        try
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (subscription == null)
                return new JsonModel
                {
                    data = new object(),
                    Message = "Subscription not found",
                    StatusCode = 404
                };

            if (subscription.Status != Subscription.SubscriptionStatuses.Cancelled && 
                subscription.Status != Subscription.SubscriptionStatuses.Expired)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Subscription is not in a reactivatable state",
                    StatusCode = 400
                };
            }

            // Reset subscription dates
            subscription.StartDate = DateTime.UtcNow;
            subscription.NextBillingDate = CalculateNextBillingDate(subscription);
            subscription.CancelledDate = null;
            subscription.ExpirationDate = null;
            subscription.CancellationReason = null;
            subscription.AutoRenew = true;

            return await ProcessStateTransitionAsync(
                subscriptionId, 
                Subscription.SubscriptionStatuses.Active, 
                reason ?? "Subscription reactivated"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reactivating subscription {SubscriptionId}", subscriptionId);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to reactivate subscription",
                StatusCode = 500
            };
        }
    }

    /// <summary>
    /// Calculate next billing date based on billing cycle
    /// </summary>
    private DateTime CalculateNextBillingDate(Subscription subscription)
    {
        var billingCycle = subscription.BillingCycle;
        
        return billingCycle.Name.ToLower() switch
        {
            "monthly" => DateTime.UtcNow.AddMonths(1),
            "quarterly" => DateTime.UtcNow.AddMonths(3),
            "annual" => DateTime.UtcNow.AddYears(1),
            "weekly" => DateTime.UtcNow.AddDays(7),
            "daily" => DateTime.UtcNow.AddDays(1),
            _ => DateTime.UtcNow.AddMonths(1) // Default to monthly
        };
    }

    /// <summary>
    /// Get subscription lifecycle status
    /// </summary>
    public async Task<JsonModel> GetSubscriptionLifecycleStatusAsync(string subscriptionId)
    {
        try
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (subscription == null)
                return new JsonModel
                {
                    data = new object(),
                    Message = "Subscription not found",
                    StatusCode = 404
                };

            var status = new SubscriptionLifecycleStatus
            {
                SubscriptionId = subscriptionId,
                CurrentStatus = subscription.Status,
                DaysUntilNextBilling = (subscription.NextBillingDate - DateTime.UtcNow).Days,
                IsActive = subscription.Status == Subscription.SubscriptionStatuses.Active,
                IsInTrial = subscription.Status == Subscription.SubscriptionStatuses.TrialActive,
                IsExpired = subscription.Status == Subscription.SubscriptionStatuses.Expired,
                IsCancelled = subscription.Status == Subscription.SubscriptionStatuses.Cancelled,
                IsPaused = subscription.Status == Subscription.SubscriptionStatuses.Paused,
                IsPaymentFailed = subscription.Status == Subscription.SubscriptionStatuses.PaymentFailed,
                CanBeReactivated = subscription.Status == Subscription.SubscriptionStatuses.Cancelled || 
                                  subscription.Status == Subscription.SubscriptionStatuses.Expired,
                CanBePaused = subscription.Status == Subscription.SubscriptionStatuses.Active,
                CanBeCancelled = subscription.Status == Subscription.SubscriptionStatuses.Active || 
                                subscription.Status == Subscription.SubscriptionStatuses.Paused
            };

            return new JsonModel
            {
                data = status,
                Message = "Subscription lifecycle status retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting lifecycle status for subscription {SubscriptionId}", subscriptionId);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to get lifecycle status",
                StatusCode = 500
            };
        }
    }

    /// <summary>
    /// Process bulk state transitions
    /// </summary>
    public async Task<JsonModel> ProcessBulkStateTransitionsAsync(
        IEnumerable<string> subscriptionIds, string newStatus, string reason, string changedByUserId = null)
    {
        var result = new BulkStateTransitionResult
        {
            ProcessedAt = DateTime.UtcNow,
            TotalSubscriptions = 0,
            SuccessfulTransitions = 0,
            FailedTransitions = 0,
            Errors = new List<string>()
        };

        foreach (var subscriptionId in subscriptionIds)
        {
            result.TotalSubscriptions++;
            try
            {
                var transitionResult = await ProcessStateTransitionAsync(subscriptionId, newStatus, reason, changedByUserId);
                if (transitionResult.StatusCode == 200)
                {
                    result.SuccessfulTransitions++;
                }
                else
                {
                    result.FailedTransitions++;
                    result.Errors.Add($"Subscription {subscriptionId}: {transitionResult.Message}");
                }
            }
            catch (Exception ex)
            {
                result.FailedTransitions++;
                result.Errors.Add($"Subscription {subscriptionId}: {ex.Message}");
            }
        }

        return new JsonModel
        {
            data = result,
            Message = "Bulk state transitions processed successfully",
            StatusCode = 200
        };
    }

    public async Task<bool> ProcessSubscriptionExpirationAsync(Guid subscriptionId, TokenModel tokenModel = null)
    {
        try
        {
            _logger.LogInformation("Processing subscription expiration for {SubscriptionId} by user {UserId}", 
                subscriptionId, tokenModel?.UserID ?? 0);
            
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found for expiration processing by user {UserId}", 
                    subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            // Check if subscription has expired
            if (subscription.ExpiryDate.HasValue && subscription.ExpiryDate.Value < DateTime.UtcNow)
            {
                var result = await ExpireSubscriptionAsync(subscriptionId, "Subscription expired automatically", tokenModel);
                
                _logger.LogInformation("Subscription expiration processed for {SubscriptionId} by user {UserId}: {Result}", 
                    subscriptionId, tokenModel?.UserID ?? 0, result);
                return result;
            }

            _logger.LogInformation("Subscription {SubscriptionId} has not expired yet, no processing needed by user {UserId}", 
                subscriptionId, tokenModel?.UserID ?? 0);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing subscription expiration for {SubscriptionId} by user {UserId}", 
                subscriptionId, tokenModel?.UserID ?? 0);
            return false;
        }
    }

    public async Task<bool> ProcessSubscriptionSuspensionAsync(Guid subscriptionId, string reason, TokenModel tokenModel = null)
    {
        try
        {
            _logger.LogInformation("Processing subscription suspension for {SubscriptionId} by user {UserId}", 
                subscriptionId, tokenModel?.UserID ?? 0);
            
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found for suspension processing by user {UserId}", 
                    subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            // Check if subscription should be suspended (e.g., payment issues, policy violations)
            var shouldSuspend = await DetermineIfShouldSuspendAsync(subscription, reason);
            if (shouldSuspend)
            {
                var result = await SuspendSubscriptionAsync(subscriptionId, reason, tokenModel);
                
                _logger.LogInformation("Subscription suspension processed for {SubscriptionId} by user {UserId}: {Result}", 
                    subscriptionId, tokenModel?.UserID ?? 0, result);
                return result;
            }

            _logger.LogInformation("Subscription {SubscriptionId} does not need suspension, no processing needed by user {UserId}", 
                subscriptionId, tokenModel?.UserID ?? 0);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing subscription suspension for {SubscriptionId} by user {UserId}", 
                subscriptionId, tokenModel?.UserID ?? 0);
            return false;
        }
    }

    // Helper method to determine if subscription should be suspended
    private async Task<bool> DetermineIfShouldSuspendAsync(Subscription subscription, string reason)
    {
        // Implement business logic to determine if suspension is needed
        // This could include checking payment history, policy violations, etc.
        return reason?.Contains("payment") == true || reason?.Contains("violation") == true;
    }
}

public class StateTransitionValidation
{
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}

public class SubscriptionLifecycleStatus
{
    public string SubscriptionId { get; set; } = string.Empty;
    public string CurrentStatus { get; set; } = string.Empty;
    public int DaysUntilNextBilling { get; set; }
    public bool IsActive { get; set; }
    public bool IsInTrial { get; set; }
    public bool IsExpired { get; set; }
    public bool IsCancelled { get; set; }
    public bool IsPaused { get; set; }
    public bool IsPaymentFailed { get; set; }
    public bool CanBeReactivated { get; set; }
    public bool CanBePaused { get; set; }
    public bool CanBeCancelled { get; set; }
}

public class BulkStateTransitionResult
{
    public DateTime ProcessedAt { get; set; }
    public int TotalSubscriptions { get; set; }
    public int SuccessfulTransitions { get; set; }
    public int FailedTransitions { get; set; }
    public List<string> Errors { get; set; } = new();
}
