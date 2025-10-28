using AutoMapper;
using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Application.Utilities;

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
    private readonly IStripeSynchronizationService _stripeSyncService;

    public PlanPricingService(
        ISubscriptionPlanRepository subscriptionPlanRepository,
        ISystemSettingsRepository systemSettingsRepository,
        ISubscriptionRepository subscriptionRepository,
        ISubscriptionPlanPrivilegeRepository planPrivilegeRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<PlanPricingService> logger,
        IStripeSynchronizationService stripeSyncService)
    {
        _subscriptionPlanRepository = subscriptionPlanRepository ?? throw new ArgumentNullException(nameof(subscriptionPlanRepository));
        _systemSettingsRepository = systemSettingsRepository ?? throw new ArgumentNullException(nameof(systemSettingsRepository));
        _subscriptionRepository = subscriptionRepository ?? throw new ArgumentNullException(nameof(subscriptionRepository));
        _planPrivilegeRepository = planPrivilegeRepository ?? throw new ArgumentNullException(nameof(planPrivilegeRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _stripeSyncService = stripeSyncService ?? throw new ArgumentNullException(nameof(stripeSyncService));
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
                _logger.LogInformation("Using manual price ${Price} for plan {PlanId}", plan.BasePrice, planId);
                return plan.BasePrice;
            }
            
            // Auto-calculate from privileges
            var planPrivileges = plan.PlanPrivileges.Where(pp => pp.IsActive).ToList();
            
            if (!planPrivileges.Any())
            {
                _logger.LogInformation("Plan {PlanId} has no active privileges, calculating base price + commission", planId);
                // CRITICAL FIX: Use centralized commission calculation for empty plans
                var emptyPlanSettings = await _systemSettingsRepository.GetSettingsAsync();
                var emptyDefaultCommissionPercent = emptyPlanSettings?.DefaultAdminCommissionPercent ?? 0;
                
                var (emptyFinalPrice, emptyCommission, emptyCommissionPercent) = BillingCalculationService.CalculateFinalPlanPrice(
                    0, // No privileges cost
                    plan.AdminCommissionPercent,
                    emptyDefaultCommissionPercent,
                    _logger);
                
                return emptyFinalPrice; // Base price is 0, but commission still applies
            }
            
            decimal privilegesTotalCost = 0;
            
            foreach (var planPrivilege in planPrivileges)
            {
                // CRITICAL FIX: Use centralized privilege cost calculation
                // This ensures consistent pricing across all services
                var privilegeCost = BillingCalculationService.CalculatePrivilegeCost(planPrivilege, _logger);
                
                // Only add to total if there's a cost
                if (privilegeCost > 0)
                {
                    privilegesTotalCost += privilegeCost;
                }
                
                _logger.LogDebug(
                    "Privilege {Name}: {Qty} × ${Base} = ${Total}",
                    planPrivilege.Privilege.Name, planPrivilege.Value,
                    planPrivilege.PrivilegeBaseCost, privilegeCost);
            }
            
            // CRITICAL FIX: Use centralized commission calculation
            var systemSettings = await _systemSettingsRepository.GetSettingsAsync();
            var defaultCommissionPercent = systemSettings?.DefaultAdminCommissionPercent ?? 0;
            
            var (finalPrice, commission, commissionPercent) = BillingCalculationService.CalculateFinalPlanPrice(
                privilegesTotalCost,
                plan.AdminCommissionPercent,
                defaultCommissionPercent,
                _logger);
            
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
                    data = new { planId, price = plan.BasePrice },
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
            plan.BasePrice = calculatedPrice;
            plan.UpdatedBy = tokenModel.UserID;
            plan.UpdatedDate = DateTime.UtcNow;
            
            await _subscriptionPlanRepository.UpdateAsync(plan);
            await _unitOfWork.CommitTransactionAsync();
            
            _logger.LogInformation(
                "Updated plan {PlanId} price to ${Price} (Privileges: ${Priv}, Commission: ${Comm})",
                planId, calculatedPrice, breakdown.PrivilegesTotalCost, breakdown.CommissionAmount);
            
            // CRITICAL: Synchronize with Stripe after price calculation update
            _logger.LogInformation("Synchronizing plan {PlanId} with Stripe after price calculation update", planId);
            var syncSuccess = await _stripeSyncService.SynchronizeSubscriptionPlanAsync(planId, tokenModel);
            
            if (!syncSuccess)
            {
                _logger.LogWarning("Failed to synchronize plan {PlanId} with Stripe after price calculation update", planId);
            }
            else
            {
                _logger.LogInformation("Successfully synchronized plan {PlanId} with Stripe after price calculation update", planId);
            }
            
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
                // If ParentPlanId is null, this IS the parent plan
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
            // CRITICAL FIX: Use centralized privilege cost calculation
            var cost = BillingCalculationService.CalculatePrivilegeCost(pp, _logger);
            
            string quantityDescription;
            if (pp.Value > 0) // Limited privileges
            {
                quantityDescription = pp.Value.ToString();
            }
            else if (pp.Value == -1) // Unlimited privileges
            {
                quantityDescription = "Unlimited";
            }
            else // Disabled (0)
            {
                quantityDescription = "Disabled";
            }
            
            if (cost > 0) // Only add to breakdown if there's a cost
            {
                privilegesTotalCost += cost;
                
                privilegeBreakdown.Add(new PrivilegeBreakdownItem
                {
                    PrivilegeName = pp.Privilege.Name,
                    Quantity = pp.Value, // Keep original value for reference
                    UnitBaseCost = pp.PrivilegeBaseCost,
                    TotalCost = cost,
                    OverageUnitCost = pp.UnitCost
                });
            }
        }
        
        // CRITICAL FIX: Use centralized commission calculation
        var systemSettings = await _systemSettingsRepository.GetSettingsAsync();
        var defaultCommissionPercent = systemSettings?.DefaultAdminCommissionPercent ?? 0;
        
        var (basePrice, commission, commissionPercent) = BillingCalculationService.CalculateFinalPlanPrice(
            privilegesTotalCost,
            plan.AdminCommissionPercent,
            defaultCommissionPercent,
            _logger);
        
        // CRITICAL FIX: Use centralized effective price calculation
        // This ensures consistent discount application across all services
        decimal finalPrice = BillingCalculationService.GetEffectivePlanPrice(plan, null, _logger);
        
        // CRITICAL: Only admin-set discount percentages are used
        // No automatic promotional codes or discounts are applied
        // Only plan.DiscountPercentage and plan.BillingDiscountPercentage are used
        
        // Calculate discount amounts for breakdown display (using centralized logic)
        decimal? promotionalDiscountAmount = null;
        decimal? billingDiscountAmount = null;
        
        if (plan.DiscountPercentage.HasValue && plan.DiscountPercentage.Value > 0 &&
            (!plan.DiscountValidUntil.HasValue || plan.DiscountValidUntil.Value >= DateTime.UtcNow))
        {
            promotionalDiscountAmount = basePrice * (plan.DiscountPercentage.Value / 100);
        }
        
        if (plan.BillingDiscountPercentage.HasValue && plan.BillingDiscountPercentage.Value > 0)
        {
            var afterPromotionalDiscount = basePrice - (promotionalDiscountAmount ?? 0);
            billingDiscountAmount = afterPromotionalDiscount * (plan.BillingDiscountPercentage.Value / 100);
        }
        
        return new PricingBreakdown
        {
            PlanId = planId,
            PlanName = plan.Name,
            IsAutoCalculated = plan.IsAutoCalculatedPrice,
            PrivilegeBreakdown = privilegeBreakdown,
            PrivilegesTotalCost = privilegesTotalCost,
            CommissionPercent = commissionPercent,
            CommissionAmount = commission,
            IsFixedCommission = false, // Always percentage-based now
            BasePrice = basePrice,
            PromotionalDiscountPercent = plan.DiscountPercentage,
            PromotionalDiscountAmount = promotionalDiscountAmount,
            BillingDiscountPercent = plan.BillingDiscountPercentage,
            BillingDiscountAmount = billingDiscountAmount,
            FinalPrice = finalPrice,
            ManualPrice = !plan.IsAutoCalculatedPrice ? plan.BasePrice : null
        };
    }

    /// <summary>
    /// Calculates the effective price for a subscription plan, considering all discounts.
    /// This is the price that should be used for billing and Stripe synchronization.
    /// CORRECTED: Removed billing cycle multiplier - BillingDiscountPercentage already handles cycle discounts.
    /// </summary>
    public async Task<decimal> CalculateEffectivePriceAsync(Guid planId, string billingCycle = "monthly")
    {
        var plan = await _subscriptionPlanRepository.GetByIdWithDetailsAsync(planId);
        if (plan == null)
            throw new ArgumentException($"Plan {planId} not found");

        // Get the pricing breakdown (already includes BillingDiscountPercentage)
        var breakdown = await CalculatePricingBreakdownAsync(planId);
        
        // CORRECTED: Return the final price as-is (no multiplier needed)
        // BillingDiscountPercentage already handles billing cycle discounts
        // Each plan has a fixed billing cycle, no multiplication needed
        return breakdown.FinalPrice;
    }

    /// <summary>
    /// CRITICAL FIX: Calculates the effective price for a plan after applying all discounts.
    /// This ensures consistent pricing calculations across frontend and backend.
    /// </summary>
    public async Task<decimal> GetEffectivePriceAsync(Guid planId)
    {
        try
        {
            _logger.LogInformation("Calculating effective price for plan {PlanId}", planId);
            
            // Get the plan details
            var plan = await _subscriptionPlanRepository.GetByIdAsync(planId);
            if (plan == null)
            {
                _logger.LogError("Plan {PlanId} not found", planId);
                throw new ArgumentException($"Plan {planId} not found");
            }
            
            // CRITICAL FIX: Use centralized effective price calculation
            // This ensures consistent discount application across all services
            var finalPrice = BillingCalculationService.GetEffectivePlanPrice(plan, null, _logger);
            
            _logger.LogInformation("Final effective price for plan {PlanId}: ${FinalPrice}", planId, finalPrice);
            
            return finalPrice;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating effective price for plan {PlanId}", planId);
            throw;
        }
    }

    #endregion
}


