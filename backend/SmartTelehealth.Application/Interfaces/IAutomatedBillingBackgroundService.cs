using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces;

/// <summary>
/// Interface for the automated billing background service.
/// This service handles scheduled billing operations, background processing,
/// and automated billing cycle management.
/// </summary>
public interface IAutomatedBillingBackgroundService
{
    /// <summary>
    /// Triggers a manual billing cycle execution
    /// </summary>
    /// <returns>Result of the manual billing cycle execution</returns>
    Task<JsonModel> TriggerManualBillingCycleAsync();

    /// <summary>
    /// Gets a billing cycle report for the specified date range
    /// </summary>
    /// <param name="startDate">Start date for the report (optional)</param>
    /// <param name="endDate">End date for the report (optional)</param>
    /// <returns>Billing cycle report data</returns>
    Task<JsonModel> GetBillingCycleReportAsync(DateTime? startDate = null, DateTime? endDate = null);
}
