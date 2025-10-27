using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;

namespace SmartTelehealth.Application.Interfaces;

/// <summary>
/// Service interface for managing billing adjustments
/// </summary>
public interface IBillingAdjustmentService
{
    /// <summary>
    /// Creates a billing adjustment for a subscription
    /// </summary>
    Task<JsonModel> CreateBillingAdjustmentAsync(CreateBillingAdjustmentDto createDto, TokenModel tokenModel);

    /// <summary>
    /// Applies a billing adjustment to a billing record
    /// </summary>
    Task<JsonModel> ApplyBillingAdjustmentAsync(Guid adjustmentId, Guid billingRecordId, TokenModel tokenModel);

    /// <summary>
    /// Gets billing adjustments for a subscription
    /// </summary>
    Task<JsonModel> GetBillingAdjustmentsAsync(Guid subscriptionId, TokenModel tokenModel);

    /// <summary>
    /// Updates a billing adjustment
    /// </summary>
    Task<JsonModel> UpdateBillingAdjustmentAsync(Guid adjustmentId, UpdateBillingAdjustmentDto updateDto, TokenModel tokenModel);

    /// <summary>
    /// Deletes a billing adjustment (soft delete)
    /// </summary>
    Task<JsonModel> DeleteBillingAdjustmentAsync(Guid adjustmentId, TokenModel tokenModel);
}
