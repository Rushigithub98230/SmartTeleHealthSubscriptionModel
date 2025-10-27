using AutoMapper;
using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Core.DTOs;

namespace SmartTelehealth.Application.Services;

/// <summary>
/// Service for managing billing adjustments including discounts, credits, refunds, and manual adjustments
/// </summary>
public class BillingAdjustmentService : IBillingAdjustmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBillingAdjustmentRepository _billingAdjustmentRepository;
    private readonly IBillingRepository _billingRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<BillingAdjustmentService> _logger;

    public BillingAdjustmentService(
        IUnitOfWork unitOfWork,
        IBillingAdjustmentRepository billingAdjustmentRepository,
        IBillingRepository billingRepository,
        ISubscriptionRepository subscriptionRepository,
        IMapper mapper,
        ILogger<BillingAdjustmentService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _billingAdjustmentRepository = billingAdjustmentRepository ?? throw new ArgumentNullException(nameof(billingAdjustmentRepository));
        _billingRepository = billingRepository ?? throw new ArgumentNullException(nameof(billingRepository));
        _subscriptionRepository = subscriptionRepository ?? throw new ArgumentNullException(nameof(subscriptionRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a billing adjustment for a subscription
    /// </summary>
    public async Task<JsonModel> CreateBillingAdjustmentAsync(CreateBillingAdjustmentDto createDto, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Creating billing adjustment for billing record {BillingRecordId}, type: {Type}, amount: {Amount}",
                createDto.BillingRecordId, createDto.Type, createDto.Amount);

            // Validate billing record exists
            var billingRecord = await _billingRepository.GetByIdAsync(createDto.BillingRecordId);
            if (billingRecord == null)
            {
                return new JsonModel { data = new object(), Message = "Billing record not found", StatusCode = 404 };
            }

            // Validate amount
            if (createDto.Amount == 0)
            {
                return new JsonModel { data = new object(), Message = "Adjustment amount cannot be zero", StatusCode = 400 };
            }

            // Validate adjustment type
            if (!Enum.IsDefined(typeof(BillingAdjustment.AdjustmentType), createDto.Type))
            {
                return new JsonModel { data = new object(), Message = "Invalid adjustment type", StatusCode = 400 };
            }

            var adjustment = new BillingAdjustment
            {
                Id = Guid.NewGuid(),
                BillingRecordId = createDto.BillingRecordId,
                Type = createDto.Type,
                Amount = createDto.Amount,
                Description = createDto.Description,
                Reason = createDto.Reason,
                AppliedAt = DateTime.UtcNow,
                AppliedBy = tokenModel.UserID,
                IsApproved = createDto.IsApproved,
                ApprovalNotes = createDto.ApprovalNotes,
                IsActive = true,
                CreatedBy = tokenModel.UserID,
                CreatedDate = DateTime.UtcNow,
                UpdatedBy = tokenModel.UserID,
                UpdatedDate = DateTime.UtcNow
            };

            var createdAdjustment = await _billingAdjustmentRepository.CreateAsync(adjustment);
            var adjustmentDto = _mapper.Map<BillingAdjustmentDto>(createdAdjustment);

            _logger.LogInformation("Created billing adjustment {AdjustmentId} for billing record {BillingRecordId}",
                createdAdjustment.Id, createDto.BillingRecordId);

            return new JsonModel { data = adjustmentDto, Message = "Billing adjustment created successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating billing adjustment for billing record {BillingRecordId}", createDto.BillingRecordId);
            return new JsonModel { data = new object(), Message = "Error creating billing adjustment", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Applies a billing adjustment to a billing record
    /// </summary>
    public async Task<JsonModel> ApplyBillingAdjustmentAsync(Guid adjustmentId, Guid billingRecordId, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Applying billing adjustment {AdjustmentId} to billing record {BillingRecordId}",
                adjustmentId, billingRecordId);

            // Get the adjustment
            var adjustment = await _billingAdjustmentRepository.GetByIdAsync(adjustmentId);
            if (adjustment == null)
            {
                return new JsonModel { data = new object(), Message = "Billing adjustment not found", StatusCode = 404 };
            }

            // Get the billing record
            var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
            if (billingRecord == null)
            {
                return new JsonModel { data = new object(), Message = "Billing record not found", StatusCode = 404 };
            }

            // Check if adjustment is already applied
            if (adjustment.AppliedAt != default(DateTime))
            {
                return new JsonModel { data = new object(), Message = "Billing adjustment already applied", StatusCode = 400 };
            }

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Apply the adjustment to the billing record
                billingRecord.TotalAmount += adjustment.Amount;
                billingRecord.UpdatedBy = tokenModel.UserID;
                billingRecord.UpdatedDate = DateTime.UtcNow;

                await _billingRepository.UpdateBillingRecordAsync(billingRecord);

                // Update the adjustment to mark it as applied
                adjustment.AppliedAt = DateTime.UtcNow;
                adjustment.AppliedBy = tokenModel.UserID;
                adjustment.UpdatedBy = tokenModel.UserID;
                adjustment.UpdatedDate = DateTime.UtcNow;

                await _billingAdjustmentRepository.UpdateAsync(adjustment);

                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Successfully applied billing adjustment {AdjustmentId} to billing record {BillingRecordId}",
                    adjustmentId, billingRecordId);

                return new JsonModel { data = new object(), Message = "Billing adjustment applied successfully", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying billing adjustment {AdjustmentId} to billing record {BillingRecordId}",
                adjustmentId, billingRecordId);
            return new JsonModel { data = new object(), Message = "Error applying billing adjustment", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Gets billing adjustments for a billing record
    /// </summary>
    public async Task<JsonModel> GetBillingAdjustmentsAsync(Guid billingRecordId, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Getting billing adjustments for billing record {BillingRecordId}", billingRecordId);

            // TODO: Implement GetByBillingRecordIdAsync method in repository
            var adjustments = new List<BillingAdjustment>(); // Placeholder
            var adjustmentDtos = _mapper.Map<IEnumerable<BillingAdjustmentDto>>(adjustments);

            return new JsonModel { data = adjustmentDtos, Message = "Billing adjustments retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting billing adjustments for billing record {BillingRecordId}", billingRecordId);
            return new JsonModel { data = new object(), Message = "Error retrieving billing adjustments", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Updates a billing adjustment
    /// </summary>
    public async Task<JsonModel> UpdateBillingAdjustmentAsync(Guid adjustmentId, UpdateBillingAdjustmentDto updateDto, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Updating billing adjustment {AdjustmentId}", adjustmentId);

            var adjustment = await _billingAdjustmentRepository.GetByIdAsync(adjustmentId);
            if (adjustment == null)
            {
                return new JsonModel { data = new object(), Message = "Billing adjustment not found", StatusCode = 404 };
            }

            // Check if adjustment is already applied
            if (adjustment.AppliedAt != default(DateTime))
            {
                return new JsonModel { data = new object(), Message = "Cannot update applied billing adjustment", StatusCode = 400 };
            }

            // Update properties
            if (!string.IsNullOrEmpty(updateDto.Description))
                adjustment.Description = updateDto.Description;
            
            if (!string.IsNullOrEmpty(updateDto.Reason))
                adjustment.Reason = updateDto.Reason;
            
            if (updateDto.Amount.HasValue)
                adjustment.Amount = updateDto.Amount.Value;

            adjustment.UpdatedBy = tokenModel.UserID;
            adjustment.UpdatedDate = DateTime.UtcNow;

            var updatedAdjustment = await _billingAdjustmentRepository.UpdateAsync(adjustment);
            var adjustmentDto = _mapper.Map<BillingAdjustmentDto>(updatedAdjustment);

            _logger.LogInformation("Updated billing adjustment {AdjustmentId}", adjustmentId);

            return new JsonModel { data = adjustmentDto, Message = "Billing adjustment updated successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating billing adjustment {AdjustmentId}", adjustmentId);
            return new JsonModel { data = new object(), Message = "Error updating billing adjustment", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Deletes a billing adjustment (soft delete)
    /// </summary>
    public async Task<JsonModel> DeleteBillingAdjustmentAsync(Guid adjustmentId, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Deleting billing adjustment {AdjustmentId}", adjustmentId);

            var adjustment = await _billingAdjustmentRepository.GetByIdAsync(adjustmentId);
            if (adjustment == null)
            {
                return new JsonModel { data = new object(), Message = "Billing adjustment not found", StatusCode = 404 };
            }

            // Check if adjustment is already applied
            if (adjustment.AppliedAt != default(DateTime))
            {
                return new JsonModel { data = new object(), Message = "Cannot delete applied billing adjustment", StatusCode = 400 };
            }

            // Soft delete
            adjustment.IsDeleted = true;
            adjustment.DeletedBy = tokenModel.UserID;
            adjustment.DeletedDate = DateTime.UtcNow;
            adjustment.UpdatedBy = tokenModel.UserID;
            adjustment.UpdatedDate = DateTime.UtcNow;

            await _billingAdjustmentRepository.UpdateAsync(adjustment);

            _logger.LogInformation("Deleted billing adjustment {AdjustmentId}", adjustmentId);

            return new JsonModel { data = new object(), Message = "Billing adjustment deleted successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting billing adjustment {AdjustmentId}", adjustmentId);
            return new JsonModel { data = new object(), Message = "Error deleting billing adjustment", StatusCode = 500 };
        }
    }
}
