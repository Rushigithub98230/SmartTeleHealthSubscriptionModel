using AutoMapper;
using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;

namespace SmartTelehealth.Application.Services;

/// <summary>
/// Service for calculating plan prices based on privilege costs and commission.
/// Healthcare Model: Total Price = Σ(Privilege Costs) + Commission.
/// Implements abuse prevention by using latest plan pricing for overages.
/// </summary>
public class PlanPricingService : IPlanPricingService
{
    private readonly ISubscriptionPlanRepository _subscriptionPlanRepository;
    private readonly ISystemSettingsRepository _systemSettingsRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly ISubscriptionPlanPrivilegeRepository _planPrivilegeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<PlanPricingService> _logger;

    public PlanPricingService(
        ISubscriptionPlanRepository subscriptionPlanRepository,
        ISystemSettingsRepository systemSettingsRepository,
        ISubscriptionRepository subscriptionRepository,
        ISubscriptionPlanPrivilegeRepository planPrivilegeRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<PlanPricingService> logger)
    {
        _subscriptionPlanRepository = subscriptionPlanRepository ?? throw new ArgumentNullException(nameof(subscriptionPlanRepository));
        _systemSettingsRepository = systemSettingsRepository ?? throw new ArgumentNullException(nameof(systemSettingsRepository));
        _subscriptionRepository = subscriptionRepository ?? throw new ArgumentNullException(nameof(subscriptionRepository));
        _planPrivilegeRepository = planPrivilegeRepository ?? throw new ArgumentNullException(nameof(planPrivilegeRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Calculates plan price based on privilege costs + admin commission.
    /// Healthcare Model: Total Price = Σ(Privilege Costs) + Commission.
    /// Choices 1c & 2c: Supports both manual and auto-calculated pricing.
    /// </summary>
    public async Task<decimal> CalculatePlanPriceAsync(Guid planId, bool useAutoCalculation = true)
    {
        try
        {
            _logger.LogInformation("Calculating price for plan {PlanId}, AutoCalculation: {Auto}", planId, useAutoCalculation);
            
            var plan = await _subscriptionPlanRepository.GetByIdWithDetailsAsync(planId);
            if (plan == null)
            {
                _logger.LogError("Plan {PlanId} not found", planId);
                throw new ArgumentException($"Plan {planId} not found");
            }
            
            // Choice 1c: Return manual price if not auto-calculating
            if (!useAutoCalculation || !plan.IsAutoCalculatedPrice)
            {
                _logger.LogInformation("Using manual price ${Price} for plan {PlanId}", plan.Price, planId);
                return plan.Price;
            }
            
            // Auto-calculate from privileges
            var planPrivileges = plan.PlanPrivileges.Where(pp => pp.IsActive).ToList();
            
            if (!planPrivileges.Any())
            {
                _logger.LogWarning("Plan {PlanId} has no active privileges for pricing calculation", planId);
                return 0;
            }
            
            decimal privilegesTotalCost = 0;
            
            foreach (var planPrivilege in planPrivileges)
            {
                // Formula: (Quantity included) × (Base cost per unit)
                // Disabled (0) or unlimited (-1) = $0 contribution to plan price
                if (planPrivilege.Value > 0)
                {
                    var privilegeCost = planPrivilege.Value * planPrivilege.PrivilegeBaseCost;
                    privilegesTotalCost += privilegeCost;
                    
                    _logger.LogDebug(
                        "Privilege {Name}: {Qty} × ${Base} = ${Total}",
                        planPrivilege.Privilege.Name, planPrivilege.Value,
                        planPrivilege.PrivilegeBaseCost, privilegeCost);
                }
            }
            
            // Choice 2c: Get commission (per-plan or global default)
            var settings = await _systemSettingsRepository.GetSettingsAsync();
            decimal commissionPercent = plan.AdminCommissionPercent ?? settings.DefaultAdminCommissionPercent;
            
            decimal commission = plan.AdminCommissionFixed 
                ?? (privilegesTotalCost * (commissionPercent / 100));
            
            decimal finalPrice = privilegesTotalCost + commission;
            
            _logger.LogInformation(
                "Plan {PlanId} auto-calculated price: Privileges ${Priv} + Commission ${Comm} ({Pct}%) = ${Final}",
                planId, privilegesTotalCost, commission, commissionPercent, finalPrice);
            
            return finalPrice;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating price for plan {PlanId}", planId);
            throw;
        }
    }

    /// <summary>
    /// Calculates and updates plan price, saving to database.
    /// </summary>
    public async Task<JsonModel> CalculateAndUpdatePlanPriceAsync(Guid planId, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Calculating and updating price for plan {PlanId} by user {UserId}", 
                planId, tokenModel.UserID);
            
            var plan = await _subscriptionPlanRepository.GetByIdWithDetailsAsync(planId);
            if (plan == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Plan not found",
                    StatusCode = 404
                };
            }
            
            if (!plan.IsAutoCalculatedPrice)
            {
                return new JsonModel
                {
                    data = new { planId, price = plan.Price },
                    Message = "Plan uses manual pricing. Auto-calculation not enabled.",
                    StatusCode = 400
                };
            }
            
            // Calculate the price
            var calculatedPrice = await CalculatePlanPriceAsync(planId, useAutoCalculation: true);
            
            // Get breakdown for transparency
            var breakdown = await CalculatePricingBreakdownAsync(planId);
            
            // Update plan
            await _unitOfWork.BeginTransactionAsync();
            
            plan.PrivilegesTotalCost = breakdown.PrivilegesTotalCost;
            plan.Price = calculatedPrice;
            plan.UpdatedBy = tokenModel.UserID;
            plan.UpdatedDate = DateTime.UtcNow;
            
            await _subscriptionPlanRepository.UpdatePlanAsync(plan);
            await _unitOfWork.CommitTransactionAsync();
            
            _logger.LogInformation(
                "Updated plan {PlanId} price to ${Price} (Privileges: ${Priv}, Commission: ${Comm})",
                planId, calculatedPrice, breakdown.PrivilegesTotalCost, breakdown.CommissionAmount);
            
            return new JsonModel
            {
                data = new
                {
                    planId,
                    price = calculatedPrice,
                    breakdown
                },
                Message = "Plan price calculated and updated successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error calculating and updating price for plan {PlanId}", planId);
            return new JsonModel
            {
                data = new object(),
                Message = $"Error: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    /// <summary>
    /// HEALTHCARE RULE: Overage uses LATEST plan pricing to prevent abuse.
    /// Even if user is on old plan (v1 at $10/mo), overage charges use new plan pricing (v2).
    /// This prevents users from staying on old plans just to get cheaper overages.
    /// </summary>
    public async Task<decimal> CalculateOverageCostForSubscriptionAsync(
        Guid subscriptionId, 
        Guid privilegeId, 
        int quantity)
    {
        try
        {
            _logger.LogInformation(
                "Calculating overage cost for subscription {SubId}, privilege {PrivId}, quantity {Qty}",
                subscriptionId, privilegeId, quantity);
            
            var subscription = await _subscriptionRepository.GetByIdWithDetailsAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogError("Subscription {SubId} not found", subscriptionId);
                throw new ArgumentException($"Subscription {subscriptionId} not found");
            }
            
            var currentPlan = subscription.SubscriptionPlan;
            
            // ✅ KEY HEALTHCARE LOGIC: Get LATEST version for overage pricing
            SubscriptionPlan pricingPlan;
            
            if (!currentPlan.IsLatestVersion)
            {
                var parentPlanId = currentPlan.ParentPlanId ?? currentPlan.Id;
                pricingPlan = await _subscriptionPlanRepository
                    .GetLatestVersionOfPlanAsync(parentPlanId);
                
                if (pricingPlan == null)
                {
                    _logger.LogWarning(
                        "Could not find latest version for plan {PlanId}. Using current plan for overage.",
                        parentPlanId);
                    pricingPlan = currentPlan;
                }
                else
                {
                    _logger.LogInformation(
                        "Subscription {SubId} is on plan v{Old}. Using v{New} pricing for overage (abuse prevention).",
                        subscriptionId, currentPlan.VersionNumber, pricingPlan.VersionNumber);
                }
            }
            else
            {
                pricingPlan = currentPlan;
                _logger.LogDebug("Subscription {SubId} is on latest plan version v{Ver}", 
                    subscriptionId, pricingPlan.VersionNumber);
            }
            
            // Get overage unit cost from LATEST plan
            var privilegeConfig = pricingPlan.PlanPrivileges
                .FirstOrDefault(pp => pp.PrivilegeId == privilegeId && pp.IsActive);
            
            if (privilegeConfig == null)
            {
                _logger.LogError(
                    "Privilege {PrivId} not found or not active in plan {PlanId} v{Ver}",
                    privilegeId, pricingPlan.Id, pricingPlan.VersionNumber);
                throw new InvalidOperationException(
                    $"Privilege {privilegeId} not found in plan {pricingPlan.Name} v{pricingPlan.VersionNumber}");
            }
            
            var unitCost = privilegeConfig.UnitCost;
            var totalCost = quantity * unitCost;
            
            _logger.LogInformation(
                "Overage cost for subscription {SubId}: {Qty} × ${Unit} (from plan v{Ver}) = ${Total}",
                subscriptionId, quantity, unitCost, pricingPlan.VersionNumber, totalCost);
            
            return totalCost;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error calculating overage cost for subscription {SubId}, privilege {PrivId}",
                subscriptionId, privilegeId);
            throw;
        }
    }

    /// <summary>
    /// Gets pricing breakdown for a plan (for display/transparency).
    /// </summary>
    public async Task<JsonModel> GetPlanPricingBreakdownAsync(Guid planId)
    {
        try
        {
            _logger.LogInformation("Getting pricing breakdown for plan {PlanId}", planId);
            
            var breakdown = await CalculatePricingBreakdownAsync(planId);
            
            return new JsonModel
            {
                data = breakdown,
                Message = "Pricing breakdown retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pricing breakdown for plan {PlanId}", planId);
            return new JsonModel
            {
                data = new object(),
                Message = $"Error: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    #region Private Helper Methods

    /// <summary>
    /// Calculates detailed pricing breakdown for a plan.
    /// Used internally and by other services for comprehensive pricing information.
    /// </summary>
    public async Task<PricingBreakdown> CalculatePricingBreakdownAsync(Guid planId)
    {
        var plan = await _subscriptionPlanRepository.GetByIdWithDetailsAsync(planId);
        if (plan == null)
            throw new ArgumentException($"Plan {planId} not found");
        
        var settings = await _systemSettingsRepository.GetSettingsAsync();
        var planPrivileges = plan.PlanPrivileges.Where(pp => pp.IsActive).ToList();
        
        var privilegeBreakdown = new List<PrivilegeBreakdownItem>();
        decimal privilegesTotalCost = 0;
        
        foreach (var pp in planPrivileges)
        {
            if (pp.Value > 0) // Only count limited privileges
            {
                var cost = pp.Value * pp.PrivilegeBaseCost;
                privilegesTotalCost += cost;
                
                privilegeBreakdown.Add(new PrivilegeBreakdownItem
                {
                    PrivilegeName = pp.Privilege.Name,
                    Quantity = pp.Value,
                    UnitBaseCost = pp.PrivilegeBaseCost,
                    TotalCost = cost,
                    OverageUnitCost = pp.UnitCost
                });
            }
        }
        
        decimal commissionPercent = plan.AdminCommissionPercent ?? settings.DefaultAdminCommissionPercent;
        decimal commission = plan.AdminCommissionFixed ?? (privilegesTotalCost * (commissionPercent / 100));
        decimal finalPrice = privilegesTotalCost + commission;
        
        return new PricingBreakdown
        {
            PlanId = planId,
            PlanName = plan.Name,
            IsAutoCalculated = plan.IsAutoCalculatedPrice,
            PrivilegeBreakdown = privilegeBreakdown,
            PrivilegesTotalCost = privilegesTotalCost,
            CommissionPercent = commissionPercent,
            CommissionAmount = commission,
            IsFixedCommission = plan.AdminCommissionFixed.HasValue,
            FinalPrice = finalPrice,
            ManualPrice = !plan.IsAutoCalculatedPrice ? plan.Price : null
        };
    }

    #endregion
}


