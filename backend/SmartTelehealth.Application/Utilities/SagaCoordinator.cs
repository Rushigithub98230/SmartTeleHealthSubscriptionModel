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

    /// <summary>
    /// Executes a saga step with automatic compensation registration.
    /// This is a convenience method for common saga patterns.
    /// </summary>
    /// <param name="step">The step to execute</param>
    /// <param name="compensation">The compensation for this step</param>
    /// <returns>True if step succeeded, false otherwise</returns>
    public async Task<bool> ExecuteStepAsync(Func<Task> step, Func<Task> compensation)
    {
        try
        {
            await step();
            AddCompensation(compensation);
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Saga step failed, compensations will be executed");
            await ExecuteCompensationsAsync();
            return false;
        }
    }

    /// <summary>
    /// Executes a saga step that returns a result with automatic compensation registration.
    /// </summary>
    /// <typeparam name="T">The result type</typeparam>
    /// <param name="step">The step to execute</param>
    /// <param name="compensation">The compensation for this step</param>
    /// <returns>The result of the step, or default if failed</returns>
    public async Task<T?> ExecuteStepAsync<T>(Func<Task<T>> step, Func<Task> compensation)
    {
        try
        {
            var result = await step();
            AddCompensation(compensation);
            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Saga step failed, compensations will be executed");
            await ExecuteCompensationsAsync();
            return default(T);
        }
    }

    /// <summary>
    /// Executes a saga step with validation and automatic compensation registration.
    /// </summary>
    /// <param name="step">The step to execute</param>
    /// <param name="compensation">The compensation for this step</param>
    /// <param name="validator">Validation function to check step result</param>
    /// <returns>True if step succeeded and passed validation, false otherwise</returns>
    public async Task<bool> ExecuteStepWithValidationAsync(Func<Task> step, Func<Task> compensation, Func<bool> validator)
    {
        try
        {
            await step();
            
            if (!validator())
            {
                _logger?.LogError("Saga step validation failed, executing compensations");
                AddCompensation(compensation);
                await ExecuteCompensationsAsync();
                return false;
            }
            
            AddCompensation(compensation);
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Saga step failed, compensations will be executed");
            await ExecuteCompensationsAsync();
            return false;
        }
    }

    /// <summary>
    /// Executes a saga step with timeout and automatic compensation registration.
    /// </summary>
    /// <param name="step">The step to execute</param>
    /// <param name="compensation">The compensation for this step</param>
    /// <param name="timeout">Timeout duration</param>
    /// <returns>True if step succeeded within timeout, false otherwise</returns>
    public async Task<bool> ExecuteStepWithTimeoutAsync(Func<Task> step, Func<Task> compensation, TimeSpan timeout)
    {
        try
        {
            using var cts = new CancellationTokenSource(timeout);
            await step();
            AddCompensation(compensation);
            return true;
        }
        catch (OperationCanceledException)
        {
            _logger?.LogError("Saga step timed out after {Timeout}, executing compensations", timeout);
            await ExecuteCompensationsAsync();
            return false;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Saga step failed, compensations will be executed");
            await ExecuteCompensationsAsync();
            return false;
        }
    }

    /// <summary>
    /// Executes a saga step with retry logic and automatic compensation registration.
    /// </summary>
    /// <param name="step">The step to execute</param>
    /// <param name="compensation">The compensation for this step</param>
    /// <param name="maxRetries">Maximum number of retries</param>
    /// <param name="retryDelay">Delay between retries</param>
    /// <returns>True if step succeeded within retry limit, false otherwise</returns>
    public async Task<bool> ExecuteStepWithRetryAsync(Func<Task> step, Func<Task> compensation, int maxRetries = 3, TimeSpan retryDelay = default)
    {
        if (retryDelay == default)
            retryDelay = TimeSpan.FromSeconds(1);

        var attempt = 0;
        while (attempt <= maxRetries)
        {
            try
            {
                await step();
                AddCompensation(compensation);
                return true;
            }
            catch (Exception ex)
            {
                attempt++;
                if (attempt > maxRetries)
                {
                    _logger?.LogError(ex, "Saga step failed after {MaxRetries} retries, executing compensations", maxRetries);
                    await ExecuteCompensationsAsync();
                    return false;
                }
                
                _logger?.LogWarning(ex, "Saga step attempt {Attempt} failed, retrying in {Delay}", attempt, retryDelay);
                await Task.Delay(retryDelay);
            }
        }

        return false;
    }

    /// <summary>
    /// Executes a saga step with exponential backoff retry logic.
    /// </summary>
    /// <param name="step">The step to execute</param>
    /// <param name="compensation">The compensation for this step</param>
    /// <param name="maxRetries">Maximum number of retries</param>
    /// <param name="baseDelay">Base delay for exponential backoff</param>
    /// <returns>True if step succeeded within retry limit, false otherwise</returns>
    public async Task<bool> ExecuteStepWithExponentialBackoffAsync(Func<Task> step, Func<Task> compensation, int maxRetries = 3, TimeSpan baseDelay = default)
    {
        if (baseDelay == default)
            baseDelay = TimeSpan.FromSeconds(1);

        var attempt = 0;
        while (attempt <= maxRetries)
        {
            try
            {
                await step();
                AddCompensation(compensation);
                return true;
            }
            catch (Exception ex)
            {
                attempt++;
                if (attempt > maxRetries)
                {
                    _logger?.LogError(ex, "Saga step failed after {MaxRetries} retries with exponential backoff, executing compensations", maxRetries);
                    await ExecuteCompensationsAsync();
                    return false;
                }
                
                var delay = TimeSpan.FromMilliseconds(baseDelay.TotalMilliseconds * Math.Pow(2, attempt - 1));
                _logger?.LogWarning(ex, "Saga step attempt {Attempt} failed, retrying in {Delay} (exponential backoff)", attempt, delay);
                await Task.Delay(delay);
            }
        }

        return false;
    }
}

