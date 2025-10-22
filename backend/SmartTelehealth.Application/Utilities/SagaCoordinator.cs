using Microsoft.Extensions.Logging;

namespace SmartTelehealth.Application.Utilities;

/// <summary>
/// Lightweight Saga coordinator for managing compensating transactions.
/// Implements the Saga pattern for handling distributed transaction failures.
/// Used within services to coordinate database + external API operations.
/// 
/// Usage:
/// var saga = new SagaCoordinator(logger);
/// // Do Step 1
/// saga.AddCompensation(() => UndoStep1());
/// // Do Step 2
/// saga.AddCompensation(() => UndoStep2());
/// // If failure: await saga.ExecuteCompensationsAsync();
/// // If success: saga.Clear();
/// </summary>
public class SagaCoordinator
{
    private readonly List<Func<Task>> _compensations = new();
    private readonly ILogger? _logger;

    public SagaCoordinator(ILogger? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Registers a compensating transaction to be executed if saga fails.
    /// Call this immediately AFTER each successful step.
    /// Compensations execute in REVERSE order (LIFO - Last In First Out).
    /// </summary>
    /// <param name="compensation">Async function that undoes the step</param>
    public void AddCompensation(Func<Task> compensation)
    {
        _compensations.Add(compensation);
    }

    /// <summary>
    /// Executes all compensating transactions in REVERSE order (LIFO).
    /// Called when saga fails to undo completed steps.
    /// Continues executing remaining compensations even if one fails.
    /// </summary>
    public async Task ExecuteCompensationsAsync()
    {
        if (_compensations.Count == 0)
        {
            _logger?.LogInformation("No compensations to execute");
            return;
        }

        _logger?.LogWarning("Executing {Count} compensating transactions (REVERSE ORDER)...", _compensations.Count);

        var successCount = 0;
        var failureCount = 0;

        // Execute in reverse order (LIFO - undo last step first)
        for (int i = _compensations.Count - 1; i >= 0; i--)
        {
            try
            {
                await _compensations[i]();
                successCount++;
                _logger?.LogInformation("✅ Compensation {Index}/{Total} executed successfully", 
                    _compensations.Count - i, _compensations.Count);
            }
            catch (Exception ex)
            {
                failureCount++;
                _logger?.LogError(ex, "❌ CRITICAL: Compensation {Index}/{Total} failed! Manual intervention may be required.",
                    _compensations.Count - i, _compensations.Count);
                // Continue with remaining compensations even if one fails
            }
        }

        _logger?.LogInformation("Compensation execution complete: {Success} succeeded, {Failed} failed", 
            successCount, failureCount);
    }

    /// <summary>
    /// Clears all registered compensations.
    /// Call this after successful saga completion to prevent accidental compensation execution.
    /// </summary>
    public void Clear()
    {
        _compensations.Clear();
        _logger?.LogDebug("Compensations cleared - saga successful");
    }

    /// <summary>
    /// Returns the number of registered compensations.
    /// </summary>
    public int Count => _compensations.Count;
}

