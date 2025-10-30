using AutoMapper;
using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Core.Enums;
using SmartTelehealth.Application.Utilities;

namespace SmartTelehealth.Application.Services;

/// <summary>
/// Service for managing subscription plans including CRUD operations,
/// plan administration, analytics, and privilege management.
/// This service handles all subscription plan-related functionality that was
/// previously managed by the SubscriptionService, following the Single Responsibility Principle.
/// </summary>
public class SubscriptionPlanService : ISubscriptionPlanService
{
    private readonly ISubscriptionPlanRepository _subscriptionPlanRepository;
    private readonly ISubscriptionPlanPrivilegeRepository _planPrivilegeRepository;
    private readonly ICategoryService _categoryService;
    private readonly IMapper _mapper;
    private readonly ILogger<SubscriptionPlanService> _logger;
    private readonly IStripeService _stripeService;
    private readonly IPrivilegeRepository _privilegeRepository;
    private readonly INotificationService _notificationService;
    private readonly IUserService _userService;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPlanPricingService _pricingService;
    private readonly IStripeSynchronizationService _stripeSyncService;
    private readonly IPlanVersioningService _planVersioningService;
    private readonly ISystemSettingsRepository _systemSettingsRepository;

    /// <summary>
    /// Initializes a new instance of the SubscriptionPlanService with required dependencies
    /// </summary>
    /// <param name="subscriptionPlanRepository">Repository for subscription plan data access operations</param>
    /// <param name="planPrivilegeRepository">Repository for subscription plan privilege data access</param>
    /// <param name="categoryService">Service for category management operations</param>
    /// <param name="mapper">AutoMapper instance for entity-DTO mapping</param>
    /// <param name="logger">Logger instance for logging operations and errors</param>
    /// <param name="stripeService">Service for Stripe integration operations</param>
    /// <param name="privilegeRepository">Repository for privilege data access</param>
    /// <param name="notificationService">Service for sending notifications</param>
    /// <param name="userService">Service for user management operations</param>
    /// <param name="subscriptionRepository">Repository for subscription data access</param>
    /// <param name="unitOfWork">Unit of work for transaction management</param>
    /// <param name="pricingService">Service for healthcare pricing calculations</param>
    /// <param name="stripeSyncService">Service for Stripe synchronization</param>
    /// <param name="planVersioningService">Service for plan versioning and migration</param>
    /// <param name="systemSettingsRepository">Repository for system settings</param>
    public SubscriptionPlanService(
        ISubscriptionPlanRepository subscriptionPlanRepository,
        ISubscriptionPlanPrivilegeRepository planPrivilegeRepository,
        ICategoryService categoryService,
        IMapper mapper,
        ILogger<SubscriptionPlanService> logger,
        IStripeService stripeService,
        IPrivilegeRepository privilegeRepository,
        INotificationService notificationService,
        IUserService userService,
        ISubscriptionRepository subscriptionRepository,
        IUnitOfWork unitOfWork,
        IPlanPricingService pricingService,
        IStripeSynchronizationService stripeSyncService,
        IPlanVersioningService planVersioningService,
        ISystemSettingsRepository systemSettingsRepository)
    {
        _subscriptionPlanRepository = subscriptionPlanRepository ?? throw new ArgumentNullException(nameof(subscriptionPlanRepository));
        _planPrivilegeRepository = planPrivilegeRepository ?? throw new ArgumentNullException(nameof(planPrivilegeRepository));
        _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _stripeService = stripeService ?? throw new ArgumentNullException(nameof(stripeService));
        _privilegeRepository = privilegeRepository ?? throw new ArgumentNullException(nameof(privilegeRepository));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _subscriptionRepository = subscriptionRepository ?? throw new ArgumentNullException(nameof(subscriptionRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _pricingService = pricingService ?? throw new ArgumentNullException(nameof(pricingService));
        _stripeSyncService = stripeSyncService ?? throw new ArgumentNullException(nameof(stripeSyncService));
        _planVersioningService = planVersioningService ?? throw new ArgumentNullException(nameof(planVersioningService));
        _systemSettingsRepository = systemSettingsRepository ?? throw new ArgumentNullException(nameof(systemSettingsRepository));
    }

    #region Core Plan Management

    /// <summary>
    /// Retrieves a specific subscription plan by its unique identifier
    /// </summary>
    public async Task<JsonModel> GetPlanByIdAsync(string planId, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Retrieving subscription plan {PlanId} by user {UserId}", planId, tokenModel?.UserID ?? 0);

            if (!Guid.TryParse(planId, out var planGuid))
            {
                return new JsonModel { data = new object(), Message = "Invalid plan ID format", StatusCode = 400 };
            }

            var plan = await _subscriptionPlanRepository.GetByIdWithDetailsAsync(planGuid);
            if (plan == null)
            {
                return new JsonModel { data = new object(), Message = "Subscription plan not found", StatusCode = 404 };
            }

            var planDto = _mapper.Map<SubscriptionPlanDto>(plan);
            return new JsonModel { data = planDto, Message = "Subscription plan retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving subscription plan {PlanId} by user {UserId}", planId, tokenModel?.UserID ?? 0);
            return new JsonModel { data = new object(), Message = "Error retrieving subscription plan", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Retrieves subscription plans with comprehensive filtering using filter DTO
    /// This is the main method that consolidates all filtering capabilities
    /// </summary>
    public async Task<JsonModel> GetSubscriptionPlansWithFilteringAsync(SubscriptionPlanFilterDto filter, TokenModel? tokenModel = null, bool adminOnly = false)
    {
        try
        {
            // Validate admin access if required
            if (adminOnly && (tokenModel?.RoleID != (int)RoleId.Admin))
            {
                return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };
            }

            // Validate filter parameters
            if (!filter.IsValid())
            {
                var errors = filter.GetValidationErrors();
                return new JsonModel 
                { 
                    data = new object(), 
                    Message = $"Invalid filter parameters: {string.Join(", ", errors)}", 
                    StatusCode = 400 
                };
            }

            _logger.LogInformation("Retrieving subscription plans with advanced filtering - Page: {Page}, PageSize: {PageSize}, SearchTerm: {SearchTerm}, CategoryId: {CategoryId}, IsActive: {IsActive}", 
                filter.Page, filter.PageSize, filter.SearchTerm, filter.CategoryId, filter.IsActive);

            // Use the advanced repository method with comprehensive filtering
            var (plans, totalCount) = await _subscriptionPlanRepository.GetPlansWithAdvancedFilteringAsync(filter);

            var planDtos = _mapper.Map<IEnumerable<SubscriptionPlanDto>>(plans);

            // Create comprehensive pagination metadata
            var paginationMeta = new Meta
            {
                TotalRecords = totalCount,
                PageSize = filter.PageSize,
                CurrentPage = filter.Page,
                TotalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize),
                DefaultPageSize = filter.PageSize,
                HasNextPage = filter.Page < (int)Math.Ceiling((double)totalCount / filter.PageSize),
                HasPreviousPage = filter.Page > 1
            };

            return new JsonModel 
            { 
                data = planDtos,
                meta = paginationMeta,
                Message = "Subscription plans retrieved successfully with advanced filtering", 
                StatusCode = 200 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving subscription plans with advanced filtering");
            return new JsonModel { data = new object(), Message = "Error retrieving subscription plans", StatusCode = 500 };
        }
    }


    /// <summary>
    /// Creates a new subscription plan
    /// </summary>
    public async Task<JsonModel> CreatePlanAsync(CreateSubscriptionPlanDto createDto, TokenModel tokenModel)
    {
        try
        {
            // Admin only method - validate admin role
            if (tokenModel.RoleID != (int)RoleId.Admin)
            {
                return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };
            }

            _logger.LogInformation("Creating subscription plan '{PlanName}' by user {UserId}", createDto.Name, tokenModel?.UserID ?? 0);

            // Validate required fields
            if (string.IsNullOrWhiteSpace(createDto.Name))
            {
                return new JsonModel { data = new object(), Message = "Plan name is required", StatusCode = 400 };
            }

            // ✅ NEW: Always calculate base price from privileges if auto-calculation is enabled
            // This ensures pricing consistency and eliminates manual input errors
            if (createDto.IsAutoCalculatedPrice)
            {
                _logger.LogInformation("Auto-calculating base price from privileges for plan '{PlanName}'", createDto.Name);
                
                // Calculate base price from privileges + commission
                decimal calculatedBasePrice = 0;
                
                if (createDto.Privileges != null && createDto.Privileges.Any())
                {
                    foreach (var privilege in createDto.Privileges)
                    {
                        // Calculate privilege cost: Value × PrivilegeBaseCost
                        decimal privilegeCost = privilege.Value > 0 
                            ? privilege.Value * privilege.PrivilegeBaseCost
                            : (privilege.Value == -1 ? privilege.PrivilegeBaseCost : 0);
                        
                        calculatedBasePrice += privilegeCost;
                    }
                    
                    // Add admin commission
                    decimal commissionPercent = createDto.AdminCommissionPercent ?? 10; // Default 10%
                    decimal commission = calculatedBasePrice * (commissionPercent / 100);
                    calculatedBasePrice += commission;
                }
                
                // Override the provided base price with calculated value
                createDto.BasePrice = calculatedBasePrice;
                
                _logger.LogInformation("Calculated base price for plan '{PlanName}': ${CalculatedPrice} (from {PrivilegeCount} privileges + {CommissionPercent}% commission)", 
                    createDto.Name, calculatedBasePrice, createDto.Privileges?.Count ?? 0, createDto.AdminCommissionPercent ?? 10);
            }

            // Allow 0 base price if auto-calculation is enabled (will be calculated)
            if (createDto.BasePrice <= 0 && !createDto.IsAutoCalculatedPrice)
            {
                return new JsonModel { data = new object(), Message = "Base price must be greater than 0 for manual pricing", StatusCode = 400 };
            }

            if (createDto.IsTrialAllowed && createDto.TrialDurationInDays <= 0)
            {
                return new JsonModel { data = new object(), Message = "Trial duration must be greater than 0 when trial is allowed", StatusCode = 400 };
            }

            // Validate category exists if provided
            if (createDto.CategoryId != Guid.Empty)
            {
                var categoryResult = await _categoryService.GetCategoryAsync(createDto.CategoryId, tokenModel);
                if (categoryResult.StatusCode != 200)
                {
                    return new JsonModel { data = new object(), Message = "Invalid category ID", StatusCode = 400 };
                }
            }

            // Check if plan with same name already exists (database-level check)
            if (!await _subscriptionPlanRepository.IsNameUniqueAsync(createDto.Name))
            {
                return new JsonModel { data = new object(), Message = "A plan with this name already exists", StatusCode = 400 };
            }

            // Validate discount data
            var discountValidation = ValidatePlanDiscount(createDto);
            if (!discountValidation.IsValid)
            {
                return new JsonModel { data = new object(), Message = discountValidation.ErrorMessage, StatusCode = 400 };
            }

            // BEGIN TRANSACTION - Single atomic operation for all changes
            await _unitOfWork.BeginTransactionAsync();
            
            SubscriptionPlan createdPlan = null;
            string stripeProductId = null;
            string stripePriceId = null;  // NEW ARCHITECTURE: Only ONE price per plan
            var invalidPrivileges = new List<Guid>();
            var assignedPrivilegesCount = 0;
            
            try
            {
                // STEP 1: Create plan entity in database first
                var plan = new SubscriptionPlan
                {
                    Name = createDto.Name,
                    Description = createDto.Description,
                    ShortDescription = createDto.ShortDescription,
                    BasePrice = createDto.BasePrice,
                    DiscountPercentage = createDto.DiscountPercentage,
                    DiscountValidUntil = createDto.DiscountValidUntil,
                    BillingCycleId = createDto.BillingCycleId,
                    CurrencyId = createDto.CurrencyId,
                    CategoryId = createDto.CategoryId,
                    IsActive = createDto.IsActive,
                    DisplayOrder = createDto.DisplayOrder,
                    // Trial configuration
                    IsTrialAllowed = createDto.IsTrialAllowed,
                    TrialDurationInDays = createDto.TrialDurationInDays,
                    // Marketing properties
                    IsFeatured = createDto.IsFeatured,
                    IsMostPopular = createDto.IsMostPopular,
                    IsTrending = createDto.IsTrending,
                    // Plan features
                    MessagingCount = createDto.MessagingCount,
                    IncludesMedicationDelivery = createDto.IncludesMedicationDelivery,
                    IncludesFollowUpCare = createDto.IncludesFollowUpCare,
                    DeliveryFrequencyDays = createDto.DeliveryFrequencyDays,
                    MaxPauseDurationDays = createDto.MaxPauseDurationDays,
                    // Note: MaxConcurrentUsers and GracePeriodDays are in DTO but not in entity
                    // These are subscription-level properties, not plan-level
                    // Metadata
                    Features = createDto.Features,
                    Terms = createDto.Terms,
                    EffectiveDate = createDto.EffectiveDate,
                    ExpirationDate = createDto.ExpirationDate,
                    // Stripe IDs (if provided)
                    StripeProductId = createDto.StripeProductId,
                    StripePriceId = createDto.StripePriceId,
                    
                    // ═══════════════════════════════════════════════════════════
                    // HEALTHCARE PRICING MODEL (Choices 1c, 2c, 4d)
                    // ═══════════════════════════════════════════════════════════
                    VersionNumber = 1,  // Choice 3a: First version
                    IsLatestVersion = true,
                    ParentPlanId = null,
                    VersionCreatedDate = DateTime.UtcNow,
                    IsAutoCalculatedPrice = createDto.IsAutoCalculatedPrice,
                    AdminCommissionPercent = createDto.AdminCommissionPercent,
                    PriceChangeNoticeDays = createDto.PriceChangeNoticeDays,
                    PrivilegesTotalCost = 0,  // Will be calculated if auto-pricing
                    BillingDiscountPercentage = createDto.BillingDiscountPercentage,
                    
                    // NEW ARCHITECTURE: Discounts are now explicit in the plan price
                    // No billing cycle discount fields needed
                    
                    // Set audit properties for creation
                    CreatedBy = tokenModel.UserID,
                    CreatedDate = DateTime.UtcNow
                };

                createdPlan = await _subscriptionPlanRepository.CreateAsync(plan);

                // STEP 2: Load billing cycle first to prevent null references
                var billingCycle = await _subscriptionRepository.GetBillingCycleByIdAsync(createdPlan.BillingCycleId);
                if (billingCycle == null)
                {
                    throw new Exception($"Billing cycle {createdPlan.BillingCycleId} not found for plan {createdPlan.Name}");
                }
                
                // Create Stripe resources
                _logger.LogInformation("Creating Stripe resources for plan {PlanName} with billing cycle {BillingCycle}", 
                    createdPlan.Name, billingCycle.Name);
                
                // Create Stripe product
                stripeProductId = await _stripeService.CreateProductAsync(createdPlan.Name, createdPlan.Description ?? "", tokenModel);
                createdPlan.StripeProductId = stripeProductId;

                // NEW ARCHITECTURE: Create only ONE Stripe price matching the plan's fixed billing cycle
                // Each plan (Monthly, Quarterly, Annual) has its own explicit price
                
                // Determine Stripe recurring interval based on billing cycle
                var (interval, intervalCount) = billingCycle.Name?.ToLower() switch
                {
                    "monthly" => ("month", 1),
                    "quarterly" => ("month", 3),
                    "annual" => ("year", 1),
                    "weekly" => ("week", 1),
                    "daily" => ("day", 1),
                    _ => ("month", 1) // Default to monthly
                };
                
                // Get currency code for Stripe integration
                var currency = await _subscriptionRepository.GetCurrencyByIdAsync(createdPlan.CurrencyId);
                var currencyCode = currency?.Code?.ToLower() ?? "usd"; // Fallback to USD if not found
                
                // Create single Stripe price for this plan's billing cycle
                stripePriceId = await _stripeService.CreatePriceAsync(
                    stripeProductId, 
                    createdPlan.BasePrice,  // Use plan's base price
                    currencyCode, 
                    interval, 
                    intervalCount, 
                    tokenModel);
                
                // NEW ARCHITECTURE: Simply set the single StripePriceId
                createdPlan.StripePriceId = stripePriceId;

                // STEP 3: Update plan with Stripe IDs (CRITICAL STEP)
                await _subscriptionPlanRepository.UpdateAsync(createdPlan);

                _logger.LogInformation("Successfully created Stripe resources for plan {PlanName}: Product {ProductId}, Price {PriceId} ({Cycle})", 
                    createdPlan.Name, stripeProductId, stripePriceId, billingCycle.Name);

                // STEP 4: Process privileges if provided (SAME TRANSACTION - NO NESTED!)
                if (createDto.Privileges != null && createDto.Privileges.Any())
                {
                    foreach (var privilege in createDto.Privileges)
                    {
                        // Validate privilege exists (use ExistsAsync for efficiency)
                        if (!await _privilegeRepository.ExistsAsync(privilege.PrivilegeId))
                        {
                            _logger.LogWarning("Privilege {PrivilegeId} not found, skipping privilege assignment", privilege.PrivilegeId);
                            invalidPrivileges.Add(privilege.PrivilegeId);
                            continue; // Skip this privilege and continue with others
                        }

                        // Create plan privilege
                        var planPrivilege = new SubscriptionPlanPrivilege
                        {
                            Id = Guid.NewGuid(),
                            SubscriptionPlanId = createdPlan.Id,
                            PrivilegeId = privilege.PrivilegeId,
                            Value = privilege.Value,
                            // UsagePeriodId = privilege.UsagePeriodId, // REMOVED - not used
                            DurationMonths = privilege.DurationMonths,
                            ExpirationDate = privilege.ExpirationDate,
                            
                            // Healthcare Pricing Model
                            PrivilegeBaseCost = privilege.PrivilegeBaseCost,  // For plan price calculation
                            UnitCost = privilege.UnitCost,  // For overage billing
                            
                            // Set audit properties for creation
                            IsActive = true,
                            CreatedBy = tokenModel.UserID,
                            CreatedDate = DateTime.UtcNow
                        };

                        await _planPrivilegeRepository.CreateAsync(planPrivilege);
                        assignedPrivilegesCount++;
                    }
                    
                    _logger.LogInformation("Successfully assigned {PrivilegeCount} privileges to plan {PlanName}", 
                        assignedPrivilegesCount, createdPlan.Name);
                }
                
                // STEP 5: Auto-calculate price if enabled (STILL IN SAME TRANSACTION!)
                // Allow auto-calculation even with 0 privileges (will result in base price + commission)
                if (createdPlan.IsAutoCalculatedPrice)
                {
                    _logger.LogInformation("Auto-calculating price for plan {PlanId} based on privileges", createdPlan.Id);
                    
                    // Get pricing breakdown (includes privilegesTotalCost)
                    var breakdown = await _pricingService.CalculatePricingBreakdownAsync(createdPlan.Id);
                    
                    // ✅ CRITICAL FIX: Store original base price before updating
                    var originalBasePrice = createdPlan.BasePrice;
                    
                    // Update plan with calculated base price
                    createdPlan.BasePrice = breakdown.BasePrice;
                    createdPlan.PrivilegesTotalCost = breakdown.PrivilegesTotalCost;
                    
                    await _subscriptionPlanRepository.UpdateAsync(createdPlan);
                    
                    // ✅ CRITICAL FIX: Update Stripe price to match the auto-calculated base price
                    if (breakdown.BasePrice != originalBasePrice)
                    {
                        _logger.LogInformation("Updating Stripe price from ${OldPrice} to ${NewPrice} for plan {PlanName}", 
                            originalBasePrice, breakdown.BasePrice, createdPlan.Name);
                        
                        // Deactivate old price and create new one with correct base price
                        var newStripePriceId = await _stripeService.UpdatePriceWithNewPriceAsync(
                            stripePriceId,
                            stripeProductId,
                            breakdown.BasePrice,
                            currencyCode,
                            interval,
                            intervalCount,
                            tokenModel);
                        
                        // Update plan with new Stripe price ID
                        createdPlan.StripePriceId = newStripePriceId;
                        await _subscriptionPlanRepository.UpdateAsync(createdPlan);
                        
                        _logger.LogInformation("Successfully updated Stripe price to match auto-calculated base price for plan {PlanName}", 
                            createdPlan.Name);
                    }
                    
                    _logger.LogInformation(
                        "Auto-calculated price for plan {PlanName}: BasePrice=${BasePrice}, FinalPrice=${FinalPrice} (Privileges: ${PrivTotal}, Commission: ${Comm})",
                        createdPlan.Name, breakdown.BasePrice, breakdown.FinalPrice, breakdown.PrivilegesTotalCost, breakdown.CommissionAmount);
                }

                // COMMIT SINGLE TRANSACTION - All operations successful (atomic)
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                // ROLLBACK SINGLE TRANSACTION - Ensure all-or-nothing consistency
                await _unitOfWork.RollbackTransactionAsync();
                
                // CRITICAL: Clean up Stripe resources if they were created but database failed
                if (!string.IsNullOrEmpty(stripeProductId))
                {
                    try
                    {
                        _logger.LogWarning("Cleaning up Stripe resources due to database failure for plan {PlanName}", createDto.Name);
                        
                        // NEW ARCHITECTURE: Deactivate the single price that was created
                        if (!string.IsNullOrEmpty(stripePriceId))
                            await _stripeService.DeactivatePriceAsync(stripePriceId, tokenModel);
                        
                        // Delete the product
                        await _stripeService.DeleteProductAsync(stripeProductId, tokenModel);
                        
                        _logger.LogInformation("Successfully cleaned up Stripe resources for failed plan {PlanName}", createDto.Name);
                    }
                    catch (Exception cleanupEx)
                    {
                        _logger.LogError(cleanupEx, "Failed to cleanup Stripe resources for plan {PlanName}. Manual cleanup may be required.", createDto.Name);
                    }
                }
                
                _logger.LogError(ex, "Failed to create subscription plan {PlanName}. Database and Stripe operations rolled back.", createDto.Name);
                return new JsonModel { data = new object(), Message = $"Failed to create plan: {ex.Message}", StatusCode = 500 };
            }

            var planDto = _mapper.Map<SubscriptionPlanDto>(createdPlan);

            // Build success message with privilege assignment info
            var successMessage = invalidPrivileges.Any()
                ? $"Plan created with {assignedPrivilegesCount} privileges. {invalidPrivileges.Count} invalid privileges skipped."
                : $"Plan created successfully with {assignedPrivilegesCount} privileges";

            _logger.LogInformation("Successfully created subscription plan {PlanId} by user {UserId}", createdPlan.Id, tokenModel?.UserID ?? 0);
            return new JsonModel { data = planDto, Message = successMessage, StatusCode = 201 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating subscription plan by user {UserId}", tokenModel?.UserID ?? 0);
            return new JsonModel { data = new object(), Message = $"Failed to create plan: {ex.Message}", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Updates an existing subscription plan
    /// </summary>

    /// <summary>
    /// Deletes a subscription plan
    /// </summary>

    /// <summary>
    /// Activates a subscription plan
    /// </summary>
    public async Task<JsonModel> ActivatePlanAsync(string planId, TokenModel tokenModel)
    {
        try
        {
            // Admin only method - validate admin role
            if (tokenModel.RoleID != (int)RoleId.Admin)
            {
                return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };
            }

            _logger.LogInformation("Activating subscription plan {PlanId} by user {UserId}", planId, tokenModel?.UserID ?? 0);

            var plan = await _subscriptionPlanRepository.GetByIdAsync(Guid.Parse(planId));
            if (plan == null)
                return new JsonModel { data = new object(), Message = "Plan not found", StatusCode = 404 };
            
            // Check if plan is already active
            if (plan.IsActive)
            {
                return new JsonModel { data = new object(), Message = "Plan is already active", StatusCode = 400 };
            }
            
            plan.IsActive = true;
            plan.UpdatedBy = tokenModel.UserID;
            plan.UpdatedDate = DateTime.UtcNow;
            await _subscriptionPlanRepository.UpdateAsync(plan);
            
            // TASK 3.2: Notify users when plan is reactivated
            // Note: Plan activation allows new purchases, but doesn't automatically activate existing subscriptions
            // Only notify if there are subscriptions waiting for this plan to become available
            try
            {
                var subscriptionsOnPlan = await _subscriptionRepository.GetByPlanIdAsync(plan.Id);
                var affectedSubscriptions = subscriptionsOnPlan
                    .Where(s => s.Status == Subscription.SubscriptionStatuses.Paused || 
                               s.Status == Subscription.SubscriptionStatuses.PaymentFailed)
                    .ToList();
                
                if (affectedSubscriptions.Any())
                {
                    _logger.LogInformation(
                        "Plan {PlanName} activated - notifying {Count} affected subscriptions",
                        plan.Name, affectedSubscriptions.Count);
                    
                    foreach (var subscription in affectedSubscriptions)
                    {
                        try
                        {
                            var userResult = await _userService.GetUserByIdAsync(subscription.UserId, tokenModel);
                            if (userResult.StatusCode == 200 && userResult.data != null)
                            {
                                var userDto = userResult.data as UserDto;
                                if (userDto != null)
                                {
                                    var notificationMessage = $@"Your subscription plan '{plan.Name}' has been reactivated.

You can now resume your subscription if it was previously paused, or update your payment method if there were payment issues.

Visit your subscription dashboard to take action.

Best regards,
SmartTelehealth Team";
                                    
                                    await _notificationService.SendNotificationAsync(
                                        subscription.UserId,
                                        $"Plan '{plan.Name}' Reactivated",
                                        notificationMessage,
                                        tokenModel);
                                    
                                    _logger.LogInformation(
                                        "Sent plan reactivation notification to user {UserId} for subscription {SubscriptionId}",
                                        subscription.UserId, subscription.Id);
                                }
                            }
                        }
                        catch (Exception notifEx)
                        {
                            _logger.LogWarning(notifEx,
                                "Failed to send plan activation notification for subscription {SubscriptionId}",
                                subscription.Id);
                            // Don't fail the entire operation if notification fails
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error notifying users about plan {PlanName} activation", plan.Name);
                // Don't fail the entire operation if notification fails
            }
            
            return new JsonModel { data = true, Message = "Plan activated", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating subscription plan {PlanId} by user {UserId}", planId, tokenModel?.UserID ?? 0);
            return new JsonModel { data = new object(), Message = $"Failed to activate plan: {ex.Message}", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Deactivates a subscription plan
    /// </summary>

    #endregion

    #region Plan Search and Filtering


    #endregion

    #region Plan Analytics and Reporting

    /// <summary>
    /// Retrieves analytics data for subscription plans
    /// </summary>

    /// <summary>
    /// Exports subscription plans to specified format
    /// </summary>
    public async Task<JsonModel> ExportSubscriptionPlansAsync(TokenModel tokenModel, string? searchTerm = null, string? categoryId = null, bool? isActive = null, string format = "csv")
    {
        try
        {
            // Admin only method - validate admin role
            if (tokenModel.RoleID != (int)RoleId.Admin && tokenModel.RoleID != (int)RoleId.Provider)
            {
                return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };
            }

            _logger.LogInformation("Exporting subscription plans in {Format} format by user {UserId}", format, tokenModel?.UserID ?? 0);

            // Get filtered plans using the consolidated method
            var filter = new SubscriptionPlanFilterDto
            {
                Page = 1,
                PageSize = int.MaxValue,
                SearchTerm = searchTerm,
                CategoryId = !string.IsNullOrEmpty(categoryId) && Guid.TryParse(categoryId, out var catId) ? catId : null,
                IsActive = isActive
            };
            var plansResult = await GetSubscriptionPlansWithFilteringAsync(filter, tokenModel, adminOnly: true);
            
            if (plansResult.StatusCode != 200)
            {
                return plansResult;
            }

            // Extract plans from the result
            var plansData = plansResult.data as dynamic;
            var plans = plansData?.plans as IEnumerable<SubscriptionPlanDto>;
            
            if (plans == null)
            {
                return new JsonModel { data = new object(), Message = "No plans found for export", StatusCode = 404 };
            }

            // Generate export data based on format
            var exportData = format.ToLower() == "csv" 
                ? GenerateSubscriptionPlansCsv(plans)
                : GenerateSubscriptionPlansExcel(plans);

            return new JsonModel 
            { 
                data = new { exportData, format, fileName = $"subscription_plans_{DateTime.UtcNow:yyyyMMdd}.{format}" }, 
                Message = "Export data generated successfully", 
                StatusCode = 200 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting subscription plans");
            return new JsonModel { data = new object(), Message = "Failed to export subscription plans", StatusCode = 500 };
        }
    }

    #endregion

    #region Plan Privilege Management

    /// <summary>
    /// Assigns privileges to a subscription plan
    /// </summary>
    public async Task<JsonModel> AssignPrivilegesToPlanAsync(Guid planId, List<PlanPrivilegeDto> privileges, TokenModel tokenModel)
    {
        // BEGIN TRANSACTION - Ensure atomic privilege assignment
        await _unitOfWork.BeginTransactionAsync();
        
        try
        {
            _logger.LogInformation("Assigning privileges to plan {PlanId} by user {UserId}", planId, tokenModel?.UserID ?? 0);

            // Check admin access
            if (tokenModel?.RoleID != (int)RoleId.Admin && tokenModel?.RoleID != (int)RoleId.Provider)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };
            }

            // Check if plan exists (lightweight query - we need the plan object for auto-pricing later)
            var plan = await _subscriptionPlanRepository.GetByIdAsync(planId);
            if (plan == null)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return new JsonModel { data = new object(), Message = "Subscription plan not found", StatusCode = 404 };
            }

            // Validate and assign privileges
            int assignedCount = 0;
            var invalidPrivileges = new List<Guid>();
            
            foreach (var privilege in privileges)
            {
                // Validate privilege exists (use ExistsAsync for efficiency)
                if (!await _privilegeRepository.ExistsAsync(privilege.PrivilegeId))
                {
                    _logger.LogWarning("Privilege {PrivilegeId} not found, skipping", privilege.PrivilegeId);
                    invalidPrivileges.Add(privilege.PrivilegeId);
                    continue;
                }

                // Create plan privilege
                var planPrivilege = new SubscriptionPlanPrivilege
                {
                    Id = Guid.NewGuid(),
                    SubscriptionPlanId = planId,
                    PrivilegeId = privilege.PrivilegeId,
                    Value = privilege.Value,
                    // UsagePeriodId = privilege.UsagePeriodId, // REMOVED - not used
                    DurationMonths = privilege.DurationMonths,
                    ExpirationDate = privilege.ExpirationDate,
                    PrivilegeBaseCost = privilege.PrivilegeBaseCost,
                    UnitCost = privilege.UnitCost,
                    IsActive = true,
                    CreatedBy = tokenModel.UserID,
                    CreatedDate = DateTime.UtcNow
                };

                await _planPrivilegeRepository.CreateAsync(planPrivilege);
                assignedCount++;
            }
            
            // If ALL privileges were invalid, fail
            if (assignedCount == 0 && privileges.Any())
            {
                await _unitOfWork.RollbackTransactionAsync();
                return new JsonModel 
                { 
                    data = new object(), 
                    Message = "All provided privileges are invalid. No privileges assigned.", 
                    StatusCode = 400 
                };
            }
            
            // If plan has auto-calculated pricing, recalculate price
            if (plan.IsAutoCalculatedPrice && assignedCount > 0)
            {
                _logger.LogInformation("Recalculating price for auto-priced plan {PlanId} after privilege assignment", planId);
                
                var breakdown = await _pricingService.CalculatePricingBreakdownAsync(planId);
                plan.BasePrice = breakdown.BasePrice;
                plan.PrivilegesTotalCost = breakdown.PrivilegesTotalCost;
                plan.UpdatedBy = tokenModel.UserID;
                plan.UpdatedDate = DateTime.UtcNow;
                
                await _subscriptionPlanRepository.UpdateAsync(plan);
                
                _logger.LogInformation("Recalculated price for plan {PlanName}: ${Price}", plan.Name, breakdown.FinalPrice);
                
                // CRITICAL: Synchronize with Stripe after privilege changes affect pricing
                _logger.LogInformation("Synchronizing plan {PlanName} with Stripe after privilege assignment", plan.Name);
                var syncSuccess = await _stripeSyncService.SynchronizeSubscriptionPlanAsync(plan.Id, tokenModel);
                
                if (!syncSuccess)
                {
                    _logger.LogWarning("Failed to synchronize plan {PlanName} with Stripe after privilege assignment", plan.Name);
                }
                else
                {
                    _logger.LogInformation("Successfully synchronized plan {PlanName} with Stripe after privilege assignment", plan.Name);
                }
            }

            // COMMIT TRANSACTION
            await _unitOfWork.CommitTransactionAsync();
            
            var message = invalidPrivileges.Any()
                ? $"Successfully assigned {assignedCount} privileges to plan. {invalidPrivileges.Count} invalid privileges skipped."
                : $"Successfully assigned {assignedCount} privileges to plan";

            return new JsonModel 
            { 
                data = new { assignedCount, skippedCount = invalidPrivileges.Count, invalidPrivileges }, 
                Message = message, 
                StatusCode = 200 
            };
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error assigning privileges to plan {PlanId}", planId);
            return new JsonModel { data = new object(), Message = $"Failed to assign privileges to plan: {ex.Message}", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Removes a privilege from a subscription plan
    /// </summary>
    public async Task<JsonModel> RemovePrivilegeFromPlanAsync(Guid planId, Guid privilegeId, TokenModel tokenModel)
    {
        // BEGIN TRANSACTION - Ensure atomic privilege removal and price recalculation
        await _unitOfWork.BeginTransactionAsync();
        
        try
        {
            _logger.LogInformation("Removing privilege {PrivilegeId} from plan {PlanId} by user {UserId}", privilegeId, planId, tokenModel?.UserID ?? 0);

            // Check admin access
            if (tokenModel?.RoleID != (int)RoleId.Admin && tokenModel?.RoleID != (int)RoleId.Provider)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };
            }

            // Check if plan exists (lightweight query - we need the plan object for auto-pricing later)
            var plan = await _subscriptionPlanRepository.GetByIdAsync(planId);
            if (plan == null)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return new JsonModel { data = new object(), Message = "Subscription plan not found", StatusCode = 404 };
            }

            // Find and remove the privilege
            var planPrivileges = await _planPrivilegeRepository.GetByPlanIdAsync(planId);
            var planPrivilege = planPrivileges.FirstOrDefault(pp => pp.PrivilegeId == privilegeId);
            
            if (planPrivilege == null)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return new JsonModel { data = new object(), Message = "Privilege not found in plan", StatusCode = 404 };
            }

            // Soft delete - set audit properties
            planPrivilege.IsDeleted = true;
            planPrivilege.DeletedBy = tokenModel.UserID;
            planPrivilege.DeletedDate = DateTime.UtcNow;
            planPrivilege.UpdatedBy = tokenModel.UserID;
            planPrivilege.UpdatedDate = DateTime.UtcNow;
            
            // Use UpdateAsync for soft delete
            await _planPrivilegeRepository.UpdateAsync(planPrivilege);
            
            // If plan has auto-calculated pricing, recalculate price
            if (plan.IsAutoCalculatedPrice)
            {
                _logger.LogInformation("Recalculating price for auto-priced plan {PlanId} after privilege removal", planId);
                
                var breakdown = await _pricingService.CalculatePricingBreakdownAsync(planId);
                plan.BasePrice = breakdown.BasePrice;
                plan.PrivilegesTotalCost = breakdown.PrivilegesTotalCost;
                plan.UpdatedBy = tokenModel.UserID;
                plan.UpdatedDate = DateTime.UtcNow;
                
                await _subscriptionPlanRepository.UpdateAsync(plan);
                
                _logger.LogInformation("Recalculated price for plan {PlanName}: ${OldPrice} → ${NewPrice}", 
                    plan.Name, plan.BasePrice, breakdown.FinalPrice);
                
                // CRITICAL: Synchronize with Stripe after privilege removal affects pricing
                _logger.LogInformation("Synchronizing plan {PlanName} with Stripe after privilege removal", plan.Name);
                var syncSuccess = await _stripeSyncService.SynchronizeSubscriptionPlanAsync(plan.Id, tokenModel);
                
                if (!syncSuccess)
                {
                    _logger.LogWarning("Failed to synchronize plan {PlanName} with Stripe after privilege removal", plan.Name);
                }
                else
                {
                    _logger.LogInformation("Successfully synchronized plan {PlanName} with Stripe after privilege removal", plan.Name);
                }
            }

            // COMMIT TRANSACTION
            await _unitOfWork.CommitTransactionAsync();

            return new JsonModel { data = true, Message = "Privilege removed from plan successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error removing privilege {PrivilegeId} from plan {PlanId}", privilegeId, planId);
            return new JsonModel { data = new object(), Message = $"Failed to remove privilege from plan: {ex.Message}", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Updates a privilege assignment for a subscription plan
    /// </summary>
    public async Task<JsonModel> UpdatePlanPrivilegeAsync(Guid planId, Guid privilegeId, PlanPrivilegeDto updatedPrivilegeDto, TokenModel tokenModel)
    {
        // BEGIN TRANSACTION - Ensure atomic privilege update and price recalculation
        await _unitOfWork.BeginTransactionAsync();
        
        try
        {
            _logger.LogInformation("Updating privilege {PrivilegeId} in plan {PlanId} by user {UserId}", privilegeId, planId, tokenModel?.UserID ?? 0);

            // Check admin access
            if (tokenModel?.RoleID != (int)RoleId.Admin && tokenModel?.RoleID != (int)RoleId.Provider)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };
            }

            // Check if plan exists (lightweight query - we need the plan object for auto-pricing later)
            var plan = await _subscriptionPlanRepository.GetByIdAsync(planId);
            if (plan == null)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return new JsonModel { data = new object(), Message = "Subscription plan not found", StatusCode = 404 };
            }

            // Find the privilege
            var planPrivileges = await _planPrivilegeRepository.GetByPlanIdAsync(planId);
            var planPrivilege = planPrivileges.FirstOrDefault(pp => pp.PrivilegeId == privilegeId);
            
            if (planPrivilege == null)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return new JsonModel { data = new object(), Message = "Privilege not found in plan", StatusCode = 404 };
            }

            // Update the privilege
            planPrivilege.Value = updatedPrivilegeDto.Value;
            // planPrivilege.UsagePeriodId = updatedPrivilegeDto.UsagePeriodId; // REMOVED - not used
            planPrivilege.DurationMonths = updatedPrivilegeDto.DurationMonths;
            planPrivilege.ExpirationDate = updatedPrivilegeDto.ExpirationDate;
            // Time-based limits removed
            planPrivilege.PrivilegeBaseCost = updatedPrivilegeDto.PrivilegeBaseCost;
            planPrivilege.UnitCost = updatedPrivilegeDto.UnitCost;  // Update unit cost for overage billing
            planPrivilege.UpdatedBy = tokenModel.UserID;
            planPrivilege.UpdatedDate = DateTime.UtcNow;

                await _planPrivilegeRepository.UpdateAsync(planPrivilege);
            
            // If plan has auto-calculated pricing, recalculate price
            if (plan.IsAutoCalculatedPrice)
            {
                _logger.LogInformation("Recalculating price for auto-priced plan {PlanId} after privilege update", planId);
                
                var breakdown = await _pricingService.CalculatePricingBreakdownAsync(planId);
                plan.BasePrice = breakdown.BasePrice;
                plan.PrivilegesTotalCost = breakdown.PrivilegesTotalCost;
                plan.UpdatedBy = tokenModel.UserID;
                plan.UpdatedDate = DateTime.UtcNow;
                
                await _subscriptionPlanRepository.UpdateAsync(plan);
                
                _logger.LogInformation("Recalculated price for plan {PlanName}: ${NewPrice}", plan.Name, breakdown.FinalPrice);
                
                // CRITICAL: Synchronize with Stripe after privilege update affects pricing
                _logger.LogInformation("Synchronizing plan {PlanName} with Stripe after privilege update", plan.Name);
                var syncSuccess = await _stripeSyncService.SynchronizeSubscriptionPlanAsync(plan.Id, tokenModel);
                
                if (!syncSuccess)
                {
                    _logger.LogWarning("Failed to synchronize plan {PlanName} with Stripe after privilege update", plan.Name);
                }
                else
                {
                    _logger.LogInformation("Successfully synchronized plan {PlanName} with Stripe after privilege update", plan.Name);
                }
            }

            // COMMIT TRANSACTION
            await _unitOfWork.CommitTransactionAsync();

            return new JsonModel { data = true, Message = "Plan privilege updated successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error updating privilege {PrivilegeId} in plan {PlanId}", privilegeId, planId);
            return new JsonModel { data = new object(), Message = $"Failed to update plan privilege: {ex.Message}", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Retrieves all privileges assigned to a subscription plan
    /// </summary>
    public async Task<JsonModel> GetPlanPrivilegesAsync(Guid planId, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Getting privileges for plan {PlanId} by user {UserId}", planId, tokenModel?.UserID ?? 0);

            // Check if plan exists (existence check only - we don't use the plan object)
            if (!await _subscriptionPlanRepository.ExistsAsync(planId))
                return new JsonModel { data = new object(), Message = "Subscription plan not found", StatusCode = 404 };

            // Get plan privileges
            var planPrivileges = await _planPrivilegeRepository.GetByPlanIdAsync(planId);
            var privilegeDtos = planPrivileges.Select(pp => new PlanPrivilegeDto
            {
                PrivilegeId = pp.PrivilegeId,
                Value = pp.Value,
                // UsagePeriodId = pp.UsagePeriodId, // REMOVED - not used
                DurationMonths = pp.DurationMonths,
                ExpirationDate = pp.ExpirationDate
            }).ToList();

            return new JsonModel { data = privilegeDtos, Message = "Plan privileges retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting privileges for plan {PlanId}", planId);
            return new JsonModel { data = new object(), Message = "Failed to get plan privileges", StatusCode = 500 };
        }
    }

    #endregion
    
    #region Additional Plan Methods (for backward compatibility)
    
    
    /// <summary>
    /// Updates a subscription plan with comprehensive validation (for backward compatibility)
    /// </summary>
    public async Task<JsonModel> UpdateAsync(string planId, UpdateSubscriptionPlanDto updateDto, TokenModel tokenModel)
    {
        try
        {
            // Admin only method - validate admin role
            if (tokenModel.RoleID != (int)RoleId.Admin && tokenModel.RoleID != (int)RoleId.Provider)
            {
                return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };
            }

            _logger.LogInformation("Updating subscription plan {PlanId} by user {UserId}", planId, tokenModel?.UserID ?? 0);

            if (!Guid.TryParse(planId, out var planGuid))
            {
                return new JsonModel { data = new object(), Message = "Invalid plan ID format", StatusCode = 400 };
            }

            var existingPlan = await _subscriptionPlanRepository.GetByIdWithDetailsAsync(planGuid);
            if (existingPlan == null)
            {
                return new JsonModel { data = new object(), Message = "Subscription plan not found", StatusCode = 404 };
            }

            var originalBasePrice = existingPlan.BasePrice;
            var originalName = existingPlan.Name;
            var originalDescription = existingPlan.Description;

            // BEGIN TRANSACTION - Ensure database and Stripe operations are atomic
            await _unitOfWork.BeginTransactionAsync();
            
            // Track Stripe changes for potential cleanup
            string newMonthlyPriceId = null;
            string newQuarterlyPriceId = null;
            string newAnnualPriceId = null;
            bool stripeProductUpdated = false;
            
            try
            {
                // Update plan properties
                if (!string.IsNullOrEmpty(updateDto.Name))
                    existingPlan.Name = updateDto.Name;
                
                if (!string.IsNullOrEmpty(updateDto.Description))
                    existingPlan.Description = updateDto.Description;
                
                if (updateDto.CategoryId != Guid.Empty)
                    existingPlan.CategoryId = updateDto.CategoryId;
                
                existingPlan.IsActive = updateDto.IsActive;
                
                if (updateDto.DisplayOrder.HasValue)
                    existingPlan.DisplayOrder = updateDto.DisplayOrder.Value;

            // NEW: Handle base price updates with Stripe synchronization
            if (updateDto.BasePrice > 0 && updateDto.BasePrice != originalBasePrice)
            {
                existingPlan.BasePrice = updateDto.BasePrice;
                
                // Sync price changes to Stripe if Stripe integration exists
                if (!string.IsNullOrEmpty(existingPlan.StripeProductId))
                {
                    try
                    {
                        _logger.LogInformation("Updating Stripe price for plan {PlanName} from {OldPrice} to {NewPrice}", 
                            existingPlan.Name, originalBasePrice, updateDto.BasePrice);
                        
                        // NEW ARCHITECTURE: Each plan has only ONE Stripe price (matching its billing cycle)
                        // Update only the price that exists for this plan's billing cycle
                        
                        // Get billing cycle to determine interval
                        var billingCycle = await _subscriptionRepository.GetBillingCycleByIdAsync(existingPlan.BillingCycleId);
                        if (billingCycle == null)
                        {
                            throw new Exception($"Billing cycle {existingPlan.BillingCycleId} not found for plan {existingPlan.Name}");
                        }
                        
                        var (interval, intervalCount) = billingCycle.Name?.ToLower() switch
                        {
                            "monthly" => ("month", 1),
                            "quarterly" => ("month", 3),
                            "annual" => ("year", 1),
                            "weekly" => ("week", 1),
                            "daily" => ("day", 1),
                            _ => ("month", 1)
                        };
                        
                        // Get currency code for Stripe integration
                        var currency = await _subscriptionRepository.GetCurrencyByIdAsync(existingPlan.CurrencyId);
                        var currencyCode = currency?.Code?.ToLower() ?? "usd"; // Fallback to USD if not found
                        
                        // NEW ARCHITECTURE: Simply update the single Stripe price
                        if (!string.IsNullOrEmpty(existingPlan.StripePriceId))
                        {
                            var newPriceId = await _stripeService.UpdatePriceWithNewPriceAsync(
                                existingPlan.StripePriceId,
                                existingPlan.StripeProductId,
                                updateDto.BasePrice,  // Use base price
                                currencyCode,
                                interval,
                                intervalCount,
                                tokenModel
                            );
                            existingPlan.StripePriceId = newPriceId;
                            _logger.LogInformation("Updated Stripe price for plan {PlanName} ({Cycle}) to ${Price}", 
                                existingPlan.Name, billingCycle.Name, updateDto.BasePrice);
                        }
                        else
                        {
                            _logger.LogWarning("No Stripe price ID found for plan {PlanName}", existingPlan.Name);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error updating Stripe price for plan {PlanName}. Failing operation to maintain DB-Stripe consistency.", existingPlan.Name);
                        // CRITICAL FIX: Don't proceed with database-only update - throw to trigger rollback
                        throw new InvalidOperationException($"Failed to synchronize price changes with Stripe. Update aborted to maintain consistency. Error: {ex.Message}", ex);
                    }
                }
                else
                {
                    _logger.LogWarning("Plan {PlanName} has no Stripe product ID. Cannot sync price changes to Stripe.", existingPlan.Name);
                }
            }

            // NEW: Handle name/description updates with Stripe synchronization
            if ((!string.IsNullOrEmpty(updateDto.Name) && updateDto.Name != originalName) ||
                (updateDto.Description != null && updateDto.Description != originalDescription))
            {
                if (!string.IsNullOrEmpty(existingPlan.StripeProductId))
                {
                    try
                    {
                        _logger.LogInformation("Updating Stripe product for plan {PlanName}", existingPlan.Name);
                        
                        await _stripeService.UpdateProductAsync(
                            existingPlan.StripeProductId, 
                            existingPlan.Name, 
                            existingPlan.Description ?? "", 
                            tokenModel
                        );
                        
                        stripeProductUpdated = true;
                        _logger.LogInformation("Successfully updated Stripe product for plan {PlanName}", existingPlan.Name);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error updating Stripe product for plan {PlanName}. Failing operation to maintain DB-Stripe consistency.", existingPlan.Name);
                        // CRITICAL FIX: Don't revert entity - throw to trigger rollback and fail operation
                        throw new InvalidOperationException($"Failed to synchronize product changes with Stripe. Update aborted to maintain consistency. Error: {ex.Message}", ex);
                    }
                }
            }

                existingPlan.UpdatedBy = tokenModel?.UserID ?? 0;
                existingPlan.UpdatedDate = DateTime.UtcNow;

                var updatedPlan = await _subscriptionPlanRepository.UpdateAsync(existingPlan);
                
                // COMMIT TRANSACTION - All operations successful
                await _unitOfWork.CommitTransactionAsync();
                
                var planDto = _mapper.Map<SubscriptionPlanDto>(updatedPlan);

                _logger.LogInformation("Successfully updated subscription plan {PlanId} by user {UserId}", planId, tokenModel?.UserID ?? 0);
                return new JsonModel { data = planDto, Message = "Subscription plan updated successfully with Stripe synchronization", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                // ROLLBACK TRANSACTION - Something failed, ensure data consistency
                await _unitOfWork.RollbackTransactionAsync();
                
                // CRITICAL: Clean up Stripe changes if they were made but database failed
                if (!string.IsNullOrEmpty(existingPlan.StripeProductId))
                {
                    try
                    {
                        _logger.LogWarning("Cleaning up Stripe changes due to database failure for plan {PlanName}", existingPlan.Name);
                        
                        // Revert product changes if they were made
                        if (stripeProductUpdated)
                        {
                            await _stripeService.UpdateProductAsync(
                                existingPlan.StripeProductId, 
                                originalName, 
                                originalDescription ?? "", 
                                tokenModel
                            );
                        }
                        
                        // Clean up new prices if they were created
                        if (!string.IsNullOrEmpty(newMonthlyPriceId))
                            await _stripeService.DeactivatePriceAsync(newMonthlyPriceId, tokenModel);
                        if (!string.IsNullOrEmpty(newQuarterlyPriceId))
                            await _stripeService.DeactivatePriceAsync(newQuarterlyPriceId, tokenModel);
                        if (!string.IsNullOrEmpty(newAnnualPriceId))
                            await _stripeService.DeactivatePriceAsync(newAnnualPriceId, tokenModel);
                        
                        _logger.LogInformation("Successfully cleaned up Stripe changes for failed plan update {PlanName}", existingPlan.Name);
                    }
                    catch (Exception cleanupEx)
                    {
                        _logger.LogError(cleanupEx, "Failed to cleanup Stripe changes for plan {PlanName}. Manual cleanup may be required.", existingPlan.Name);
                    }
                }
                
                _logger.LogError(ex, "Failed to update subscription plan {PlanId}. Database and Stripe operations rolled back.", planId);
                return new JsonModel { data = new object(), Message = "Failed to update subscription plan", StatusCode = 500 };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating subscription plan {PlanId} by user {UserId}", planId, tokenModel?.UserID ?? 0);
            return new JsonModel { data = new object(), Message = "Failed to update subscription plan", StatusCode = 500 };
        }
    }
    
    /// <summary>
    /// Deactivates a subscription plan (soft delete) - RECOMMENDED APPROACH
    /// </summary>
    public async Task<JsonModel> DeactivatePlanAsync(string planId, TokenModel tokenModel)
    {
        try
        {
            // Admin only method - validate admin role
            if (tokenModel.RoleID != (int)RoleId.Admin)
            {
                return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };
            }

            _logger.LogInformation("Deactivating subscription plan {PlanId} by user {UserId}", planId, tokenModel?.UserID ?? 0);

            if (!Guid.TryParse(planId, out var planGuid))
            {
                return new JsonModel { data = new object(), Message = "Invalid plan ID format", StatusCode = 400 };
            }

            var existingPlan = await _subscriptionPlanRepository.GetByIdWithDetailsAsync(planGuid);
            if (existingPlan == null)
            {
                return new JsonModel { data = new object(), Message = "Subscription plan not found", StatusCode = 404 };
            }

            // Check if plan is already deactivated
            if (!existingPlan.IsActive)
            {
                return new JsonModel { data = new object(), Message = "Plan is already deactivated", StatusCode = 400 };
            }

            // Check if plan has active subscriptions (database-level check)
            if (await _subscriptionPlanRepository.HasActiveSubscriptionsAsync(existingPlan.Id))
            {
                return new JsonModel { data = new object(), Message = "Cannot deactivate plan with active subscriptions. Please wait for all subscriptions to end or cancel them first.", StatusCode = 400 };
            }

            // BEGIN TRANSACTION - Ensure database and Stripe operations are atomic
            await _unitOfWork.BeginTransactionAsync();
            
            try
            {
                // Deactivate Stripe resources instead of deleting them
                if (!string.IsNullOrEmpty(existingPlan.StripeProductId))
                {
                    _logger.LogInformation("Deactivating Stripe resources for plan {PlanName}", existingPlan.Name);
                    
                    try
                    {
                        // NEW ARCHITECTURE: Deactivate the single price
                        if (!string.IsNullOrEmpty(existingPlan.StripePriceId))
                        {
                            await _stripeService.DeactivatePriceAsync(existingPlan.StripePriceId, tokenModel);
                        }
                        
                        // Archive the product instead of deleting it
                        await _stripeService.ArchiveProductAsync(existingPlan.StripeProductId, existingPlan.Name, existingPlan.Description ?? "", tokenModel);
                        
                        _logger.LogInformation("Successfully deactivated Stripe resources for plan {PlanName}", existingPlan.Name);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error deactivating Stripe resources for plan {PlanName}: {Message}", existingPlan.Name, ex.Message);
                        // Continue with database deactivation even if Stripe operations fail
                    }
                }

                // Soft delete: Deactivate the plan instead of hard delete
                existingPlan.IsActive = false;
                existingPlan.UpdatedDate = DateTime.UtcNow;
                existingPlan.UpdatedBy = tokenModel?.UserID ?? 0;
                
                var result = await _subscriptionPlanRepository.UpdateAsync(existingPlan);
                if (result == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return new JsonModel { data = new object(), Message = "Failed to deactivate subscription plan", StatusCode = 500 };
                }

                await _unitOfWork.CommitTransactionAsync();
                
                _logger.LogInformation("Successfully deactivated subscription plan {PlanName} by user {UserId}", existingPlan.Name, tokenModel?.UserID ?? 0);
                
                // TASK 3.2: Notify all affected users when plan is deactivated mid-cycle
                // Notify users with paused, cancelled, or expired subscriptions that the plan is no longer available
                var allPlanSubscriptions = await _subscriptionRepository.GetByPlanIdAsync(existingPlan.Id);
                var affectedSubscriptions = allPlanSubscriptions
                    .Where(s => s.Status != Subscription.SubscriptionStatuses.Cancelled || 
                               s.EndDate > DateTime.UtcNow) // Notify if subscription hasn't fully ended yet
                    .ToList();
                
                if (affectedSubscriptions.Any())
                {
                    _logger.LogInformation(
                        "Plan {PlanName} deactivated - notifying {Count} affected users",
                        existingPlan.Name, affectedSubscriptions.Count);
                    
                    foreach (var subscription in affectedSubscriptions)
                    {
                        try
                        {
                            var userResult = await _userService.GetUserByIdAsync(subscription.UserId, tokenModel);
                            if (userResult.StatusCode == 200 && userResult.data != null)
                            {
                                var userDto = userResult.data as UserDto;
                                if (userDto != null)
                                {
                                    var subscriptionEndDate = subscription.EndDate?.ToString("MMMM dd, yyyy") ?? "the end of your current billing period";
                                    var notificationMessage = $@"Important: Your subscription plan '{existingPlan.Name}' has been deactivated.

This means the plan is no longer available for new purchases. Your existing subscription will continue until {subscriptionEndDate}.

What this means for you:
• Your current subscription will remain active until it expires
• You will not be automatically renewed to this plan after it expires
• You can switch to a different active plan at any time

We recommend choosing a new plan before your current subscription expires to avoid service interruption.

Visit your subscription dashboard to explore available plans and make changes.

If you have any questions, please contact our support team.

Best regards,
SmartTelehealth Team";
                                    
                                    await _notificationService.SendNotificationAsync(
                                        subscription.UserId,
                                        $"Important: Plan '{existingPlan.Name}' Deactivated",
                                        notificationMessage,
                                        tokenModel);
                                    
                                    _logger.LogInformation(
                                        "Sent plan deactivation notification to user {UserId} for subscription {SubscriptionId}",
                                        subscription.UserId, subscription.Id);
                                }
                            }
                        }
                        catch (Exception notifEx)
                        {
                            _logger.LogWarning(notifEx,
                                "Failed to send plan deactivation notification for subscription {SubscriptionId}",
                                subscription.Id);
                            // Don't fail the entire operation if notification fails
                        }
                    }
                }
                
                return new JsonModel 
                { 
                    data = new { planId = planId, planName = existingPlan.Name, isActive = false }, 
                    Message = "Subscription plan deactivated successfully", 
                    StatusCode = 200 
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error deactivating subscription plan {PlanId}: {Message}", planId, ex.Message);
                
                return new JsonModel { data = new object(), Message = "An error occurred while deactivating the subscription plan", StatusCode = 500 };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in DeactivatePlanAsync for plan {PlanId}: {Message}", planId, ex.Message);
            return new JsonModel { data = new object(), Message = "An unexpected error occurred", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Reactivates a deactivated subscription plan
    /// </summary>
    public async Task<JsonModel> ReactivatePlanAsync(string planId, TokenModel tokenModel)
    {
        try
        {
            // Admin only method - validate admin role
            if (tokenModel.RoleID != (int)RoleId.Admin)
            {
                return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };
            }

            _logger.LogInformation("Reactivating subscription plan {PlanId} by user {UserId}", planId, tokenModel?.UserID ?? 0);

            if (!Guid.TryParse(planId, out var planGuid))
            {
                return new JsonModel { data = new object(), Message = "Invalid plan ID format", StatusCode = 400 };
            }

            var existingPlan = await _subscriptionPlanRepository.GetByIdWithDetailsAsync(planGuid);
            if (existingPlan == null)
            {
                return new JsonModel { data = new object(), Message = "Subscription plan not found", StatusCode = 404 };
            }

            // Check if plan is already active
            if (existingPlan.IsActive)
            {
                return new JsonModel { data = new object(), Message = "Plan is already active", StatusCode = 400 };
            }

            // BEGIN TRANSACTION
            await _unitOfWork.BeginTransactionAsync();
            
            try
            {
                // Reactivate the plan
                existingPlan.IsActive = true;
                existingPlan.UpdatedDate = DateTime.UtcNow;
                existingPlan.UpdatedBy = tokenModel?.UserID ?? 0;
                
                var result = await _subscriptionPlanRepository.UpdateAsync(existingPlan);
                if (result == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return new JsonModel { data = new object(), Message = "Failed to reactivate subscription plan", StatusCode = 500 };
                }

                await _unitOfWork.CommitTransactionAsync();
                
                _logger.LogInformation("Successfully reactivated subscription plan {PlanName} by user {UserId}", existingPlan.Name, tokenModel?.UserID ?? 0);
                
                return new JsonModel 
                { 
                    data = new { planId = planId, planName = existingPlan.Name, isActive = true }, 
                    Message = "Subscription plan reactivated successfully", 
                    StatusCode = 200 
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error reactivating subscription plan {PlanId}: {Message}", planId, ex.Message);
                
                return new JsonModel { data = new object(), Message = "An error occurred while reactivating the subscription plan", StatusCode = 500 };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in ReactivatePlanAsync for plan {PlanId}: {Message}", planId, ex.Message);
            return new JsonModel { data = new object(), Message = "An unexpected error occurred", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Deletes a subscription plan with comprehensive validation (DEPRECATED - Use DeactivatePlanAsync instead)
    /// </summary>
    [Obsolete("Use DeactivatePlanAsync instead for better data integrity and business continuity")]
    public async Task<JsonModel> DeletePlanAsync(string planId, TokenModel tokenModel)
    {
        try
        {
            // Admin only method - validate admin role
            if (tokenModel.RoleID != (int)RoleId.Admin)
            {
                return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };
            }

            _logger.LogInformation("Deactivating subscription plan {PlanId} by user {UserId}", planId, tokenModel?.UserID ?? 0);

            if (!Guid.TryParse(planId, out var planGuid))
            {
                return new JsonModel { data = new object(), Message = "Invalid plan ID format", StatusCode = 400 };
            }

            var existingPlan = await _subscriptionPlanRepository.GetByIdWithDetailsAsync(planGuid);
            if (existingPlan == null)
            {
                return new JsonModel { data = new object(), Message = "Subscription plan not found", StatusCode = 404 };
            }

            // Check if plan is already deactivated
            if (!existingPlan.IsActive)
            {
                return new JsonModel { data = new object(), Message = "Plan is already deactivated", StatusCode = 400 };
            }

            // Check if plan has active subscriptions (database-level check)
            if (await _subscriptionPlanRepository.HasActiveSubscriptionsAsync(existingPlan.Id))
            {
                return new JsonModel { data = new object(), Message = "Cannot deactivate plan with active subscriptions. Please wait for all subscriptions to end or cancel them first.", StatusCode = 400 };
            }

            // BEGIN TRANSACTION - Ensure database and Stripe operations are atomic
            await _unitOfWork.BeginTransactionAsync();
            
            // Track Stripe cleanup for potential recovery
            bool stripeCleanedUp = false;
            string originalProductId = existingPlan.StripeProductId;
            string originalPriceId = existingPlan.StripePriceId;
            
            try
            {
                // Clean up Stripe resources before deleting the plan
                if (!string.IsNullOrEmpty(existingPlan.StripeProductId))
                {
                    _logger.LogInformation("Cleaning up Stripe resources for plan {PlanName}", existingPlan.Name);
                    
                    try
                    {
                        // NEW ARCHITECTURE: Deactivate the single price
                        if (!string.IsNullOrEmpty(existingPlan.StripePriceId))
                        {
                            await _stripeService.DeactivatePriceAsync(existingPlan.StripePriceId, tokenModel);
                        }
                        
                        // Wait a moment for Stripe to process the deactivations
                        await Task.Delay(1000);
                        
                        // Try to delete the product
                        await _stripeService.DeleteProductAsync(existingPlan.StripeProductId, tokenModel);
                        
                        stripeCleanedUp = true;
                        _logger.LogInformation("Successfully cleaned up Stripe resources for plan {PlanName}", existingPlan.Name);
                    }
                    catch (Exception ex) when (ex.Message.Contains("cannot be deleted because it has one or more user-created prices"))
                    {
                        _logger.LogWarning("Cannot delete Stripe product {ProductId} due to active prices. Product will be archived in Stripe instead.", existingPlan.StripeProductId);
                        
                        // Instead of deleting, we'll archive the product
                        try
                        {
                            await _stripeService.ArchiveProductAsync(existingPlan.StripeProductId, existingPlan.Name, existingPlan.Description ?? "", tokenModel);
                            stripeCleanedUp = true;
                            _logger.LogInformation("Archived Stripe product {ProductId} instead of deleting it", existingPlan.StripeProductId);
                        }
                        catch (Exception archiveEx)
                        {
                            _logger.LogError(archiveEx, "Failed to archive Stripe product {ProductId} after deletion attempt", existingPlan.StripeProductId);
                            // Continue with database deletion even if Stripe archiving fails
                            stripeCleanedUp = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error cleaning up Stripe resources for plan {PlanName}: {Message}", existingPlan.Name, ex.Message);
                        // Continue with database deletion even if Stripe cleanup fails
                        stripeCleanedUp = true;
                    }
                }

                // Set audit properties for soft deletion
                existingPlan.IsDeleted = true;
                existingPlan.DeletedBy = tokenModel.UserID;
                existingPlan.DeletedDate = DateTime.UtcNow;
                existingPlan.UpdatedBy = tokenModel.UserID;
                existingPlan.UpdatedDate = DateTime.UtcNow;

                // Use UpdateAsync instead of DeleteAsync for soft delete
                var result = await _subscriptionPlanRepository.UpdateAsync(existingPlan);
                if (result == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    
                    // CRITICAL: If Stripe was cleaned up but database deletion failed, we need to recover
                    if (stripeCleanedUp && !string.IsNullOrEmpty(originalProductId))
                    {
                        try
                        {
                            _logger.LogWarning("Attempting to recover Stripe resources for plan {PlanName} due to database deletion failure", existingPlan.Name);
                            
                            // Recreate the Stripe product
                            var recoveredProductId = await _stripeService.CreateProductAsync(existingPlan.Name, existingPlan.Description ?? "", tokenModel);
                            existingPlan.StripeProductId = recoveredProductId;
                            
                            // NEW ARCHITECTURE: Recreate the single price for this plan
                            var billingCycle = await _subscriptionRepository.GetBillingCycleByIdAsync(existingPlan.BillingCycleId);
                            var (interval, intervalCount) = billingCycle.Name?.ToLower() switch
                            {
                                "monthly" => ("month", 1),
                                "quarterly" => ("month", 3),
                                "annual" => ("year", 1),
                                _ => ("month", 1)
                            };
                            
                            // Get currency code for Stripe integration
                            var currency = await _subscriptionRepository.GetCurrencyByIdAsync(existingPlan.CurrencyId);
                            var currencyCode = currency?.Code?.ToLower() ?? "usd"; // Fallback to USD if not found
                            
                            var recoveredPriceId = await _stripeService.CreatePriceAsync(
                                recoveredProductId,
                                existingPlan.BasePrice,
                                currencyCode,
                                interval,
                                intervalCount,
                                tokenModel);
                            
                            existingPlan.StripePriceId = recoveredPriceId;
                            await _subscriptionPlanRepository.UpdateAsync(existingPlan);
                            
                            _logger.LogInformation("Successfully recovered Stripe resources for plan {PlanName}", existingPlan.Name);
                        }
                        catch (Exception recoveryEx)
                        {
                            _logger.LogError(recoveryEx, "Failed to recover Stripe resources for plan {PlanName}. Manual recovery may be required.", existingPlan.Name);
                        }
                    }
                    
                    return new JsonModel { data = new object(), Message = "Failed to delete subscription plan", StatusCode = 500 };
                }

                // COMMIT TRANSACTION - All operations successful
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Successfully deleted subscription plan {PlanId} by user {UserId}", planId, tokenModel?.UserID ?? 0);
                return new JsonModel { data = true, Message = "Subscription plan deleted successfully", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                // ROLLBACK TRANSACTION - Something failed, ensure data consistency
                await _unitOfWork.RollbackTransactionAsync();
                
                // CRITICAL: If Stripe was cleaned up but database deletion failed, we need to recover
                if (stripeCleanedUp && !string.IsNullOrEmpty(originalProductId))
                {
                    try
                    {
                        _logger.LogWarning("Attempting to recover Stripe resources for plan {PlanName} due to deletion failure", existingPlan.Name);
                        
                        // Recreate the Stripe product
                        var recoveredProductId = await _stripeService.CreateProductAsync(existingPlan.Name, existingPlan.Description ?? "", tokenModel);
                        existingPlan.StripeProductId = recoveredProductId;
                        
                        // NEW ARCHITECTURE: Recreate the single price for this plan
                        var billingCycle = await _subscriptionRepository.GetBillingCycleByIdAsync(existingPlan.BillingCycleId);
                        var (interval, intervalCount) = billingCycle.Name?.ToLower() switch
                        {
                            "monthly" => ("month", 1),
                            "quarterly" => ("month", 3),
                            "annual" => ("year", 1),
                            _ => ("month", 1)
                        };
                        
                        // Get currency code for Stripe integration
                        var currency = await _subscriptionRepository.GetCurrencyByIdAsync(existingPlan.CurrencyId);
                        var currencyCode = currency?.Code?.ToLower() ?? "usd"; // Fallback to USD if not found
                        
                        var recoveredPriceId = await _stripeService.CreatePriceAsync(
                            recoveredProductId,
                            existingPlan.BasePrice,
                            currencyCode,
                            interval,
                            intervalCount,
                            tokenModel);
                        
                        existingPlan.StripePriceId = recoveredPriceId;
                        await _subscriptionPlanRepository.UpdateAsync(existingPlan);
                        
                        _logger.LogInformation("Successfully recovered Stripe resources for plan {PlanName}", existingPlan.Name);
                    }
                    catch (Exception recoveryEx)
                    {
                        _logger.LogError(recoveryEx, "Failed to recover Stripe resources for plan {PlanName}. Manual recovery may be required.", existingPlan.Name);
                    }
                }
                
                _logger.LogError(ex, "Failed to delete subscription plan {PlanId}. Database and Stripe operations rolled back.", planId);
                return new JsonModel { data = new object(), Message = "Error deleting subscription plan", StatusCode = 500 };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting subscription plan {PlanId} by user {UserId}", planId, tokenModel?.UserID ?? 0);
            return new JsonModel { data = new object(), Message = "Error deleting subscription plan", StatusCode = 500 };
        }
    }
    
    
    #endregion

    #region Helper Methods

    /// <summary>
    /// Generates CSV data for subscription plans export
    /// </summary>
    private string GenerateSubscriptionPlansCsv(IEnumerable<SubscriptionPlanDto> plans)
    {
        var csv = new System.Text.StringBuilder();
        
        // Enhanced CSV header with more comprehensive fields
        csv.AppendLine("Plan ID,Name,Description,Price,Currency,Billing Cycle,Category,Is Active,Is Trial Allowed,Trial Duration (Days),Display Order,Features,Terms,Created Date,Updated Date,Total Subscriptions");
        
        foreach (var plan in plans)
        {
            // Escape CSV values properly and handle null values
            var name = EscapeCsvValue(plan.Name);
            var description = EscapeCsvValue(plan.Description);
            var features = EscapeCsvValue(plan.Features);
            var terms = EscapeCsvValue(plan.Terms);
            var currency = plan.CurrencyId.ToString();
            var billingCycle = plan.BillingCycleId.ToString();
            var category = plan.CategoryId.ToString();
            var trialDuration = plan.TrialDurationInDays.ToString();
            var displayOrder = plan.DisplayOrder.ToString();
            var totalSubscriptions = "0"; // Not available in DTO
            
            csv.AppendLine($"{plan.Id},{name},{description},{plan.BasePrice},{currency},{billingCycle},{category},{plan.IsActive},{plan.IsTrialAllowed},{trialDuration},{displayOrder},{features},{terms},{plan.CreatedDate:yyyy-MM-dd HH:mm:ss},{plan.UpdatedDate:yyyy-MM-dd HH:mm:ss},{totalSubscriptions}");
        }
        
        return csv.ToString();
    }
    
    /// <summary>
    /// Escapes CSV values to handle commas, quotes, and newlines
    /// </summary>
    private string EscapeCsvValue(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return "";
            
        // If value contains comma, quote, or newline, wrap in quotes and escape internal quotes
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        
        return value;
    }

    /// <summary>
    /// Generates Excel-compatible data for subscription plans export
    /// Returns structured data that can be easily imported into Excel
    /// </summary>
    private object GenerateSubscriptionPlansExcel(IEnumerable<SubscriptionPlanDto> plans)
    {
        // Create structured data for Excel import
        var excelData = new
        {
            Summary = new
            {
                TotalPlans = plans.Count(),
                ActivePlans = plans.Count(p => p.IsActive),
                InactivePlans = plans.Count(p => !p.IsActive),
                PlansWithTrial = plans.Count(p => p.IsTrialAllowed),
                AveragePrice = plans.Any() ? plans.Average(p => p.BasePrice) : 0,
                TotalSubscriptions = 0, // Not available in DTO
                ExportDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC")
            },
            Plans = plans.Select(plan => new
            {
                PlanId = plan.Id,
                Name = plan.Name,
                Description = plan.Description,
                Price = plan.BasePrice,
                Currency = plan.CurrencyId.ToString(),
                BillingCycle = plan.BillingCycleId.ToString(),
                Category = plan.CategoryId.ToString(),
                IsActive = plan.IsActive ? "Yes" : "No",
                IsTrialAllowed = plan.IsTrialAllowed ? "Yes" : "No",
                TrialDurationDays = plan.TrialDurationInDays,
                DisplayOrder = plan.DisplayOrder,
                Features = plan.Features ?? "",
                Terms = plan.Terms ?? "",
                CreatedDate = plan.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss"),
                UpdatedDate = plan.UpdatedDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "",
                TotalSubscriptions = 0 // Not available in DTO
            }).ToList(),
            // Also include CSV format for compatibility
            CsvData = GenerateSubscriptionPlansCsv(plans)
        };
        
        return excelData;
    }

    /// <summary>
    /// Gets plans for a category with comparison details.
    /// NEW ARCHITECTURE: Returns Monthly, Quarterly, Annual plans with value comparison metrics.
    /// Helps users understand the value proposition of each billing cycle.
    /// </summary>
    /// <param name="categoryId">The category ID</param>
    /// <param name="tokenModel">Token for authentication</param>
    /// <returns>JsonModel with plans and comparison data</returns>
    public async Task<JsonModel> GetPlansForComparisonAsync(Guid categoryId, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Getting plans for comparison - category {CategoryId}", categoryId);
            
            // Get all plans for this category using new repository method
            var plans = await _subscriptionPlanRepository.GetPlansByCategoryAsync(categoryId);
            
            if (!plans.Any())
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "No plans found for this category",
                    StatusCode = 404
                };
            }
            
            // Calculate comparison metrics for each plan
            var comparisonData = plans.Select(plan => new
            {
                PlanId = plan.Id,
                PlanName = plan.Name,
                BillingCycle = plan.BillingCycle.Name,
                BillingCycleDays = plan.BillingCycle.DurationInDays,
                Price = plan.BasePrice,
                // Calculate effective monthly price for comparison
                PricePerMonth = plan.BillingCycle.DurationInDays > 0 
                    ? Math.Round(plan.BasePrice / (plan.BillingCycle.DurationInDays / 30.0m), 2)
                    : plan.BasePrice,
                Privileges = plan.PlanPrivileges.Select(pp => new
                {
                    PrivilegeId = pp.PrivilegeId,
                    PrivilegeName = pp.Privilege?.Name ?? "Unknown",
                    Value = pp.Value,
                    IsUnlimited = pp.IsUnlimited,
                    UnitCost = pp.UnitCost,
                    TotalCost = BillingCalculationService.CalculatePrivilegeCost(pp, _logger)
                }).ToList(),
                TotalPrivilegesValue = BillingCalculationService.CalculatePlanBasePrice(plan.PlanPrivileges, _logger),
                AdminCommission = BillingCalculationService.CalculateAdminCommission(
                    BillingCalculationService.CalculatePlanBasePrice(plan.PlanPrivileges, _logger),
                    plan.AdminCommissionPercent,
                    0, // Default commission percent - will be overridden by system settings in actual calculations
                    _logger),
                IsFeatured = plan.IsFeatured,
                IsMostPopular = plan.IsMostPopular,
                Description = plan.Description,
                ShortDescription = plan.ShortDescription
            })
            .OrderBy(p => p.BillingCycleDays)
            .ToList();
            
            // Calculate savings compared to monthly
            var monthlyPlan = comparisonData.FirstOrDefault(p => p.BillingCycle.ToLower() == "monthly");
            if (monthlyPlan != null)
            {
                foreach (var plan in comparisonData)
                {
                    var monthlyEquivalent = monthlyPlan.PricePerMonth * (plan.BillingCycleDays / 30.0m);
                    var savings = monthlyEquivalent - plan.Price;
                    
                    // Add savings data
                    plan.GetType().GetProperty("AnnualSavings")?.SetValue(plan, 
                        plan.BillingCycle.ToLower() == "annual" ? savings : savings * (365.0m / plan.BillingCycleDays));
                }
            }
            
            _logger.LogInformation("Retrieved {Count} plans for category {CategoryId} comparison", 
                comparisonData.Count, categoryId);
            
            return new JsonModel
            {
                data = new
                {
                    CategoryId = categoryId,
                    Plans = comparisonData,
                    ComparisonGenerated = DateTime.UtcNow
                },
                Message = $"Retrieved {comparisonData.Count} plans for comparison",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting plans for comparison - category {CategoryId}", categoryId);
            return new JsonModel
            {
                data = new object(),
                Message = "Error retrieving plans for comparison",
                StatusCode = 500
            };
        }
    }

    /// <summary>
    /// Validates plan discount data
    /// </summary>
    /// <param name="createDto">The plan creation DTO</param>
    /// <returns>Validation result</returns>
    private (bool IsValid, string ErrorMessage) ValidatePlanDiscount(CreateSubscriptionPlanDto createDto)
    {
        try
        {
            // If no discount percentage is set, validation passes
            if (!createDto.DiscountPercentage.HasValue || createDto.DiscountPercentage.Value <= 0)
            {
                return (true, string.Empty);
            }

            var basePrice = createDto.BasePrice;
            var discountPercentage = createDto.DiscountPercentage.Value;

            // Validate discount percentage is reasonable (not more than 100%)
            if (discountPercentage >= 100)
            {
                return (false, "Discount percentage must be less than 100%");
            }

            // Validate that discount doesn't make price negative
            var finalPrice = basePrice * (1 - (discountPercentage / 100));
            if (finalPrice <= 0)
            {
                return (false, "Discount percentage is too high - would result in zero or negative price");
            }

            // Validate discount is not more than 90% (business rule)
            if (discountPercentage > 90)
            {
                return (false, "Discount percentage cannot exceed 90%");
            }

            // If discount valid until is set, validate it's in the future
            if (createDto.DiscountValidUntil.HasValue)
            {
                if (createDto.DiscountValidUntil.Value <= DateTime.UtcNow)
                {
                    return (false, "Discount valid until date must be in the future");
                }

                // Validate discount doesn't last more than 1 year
                if (createDto.DiscountValidUntil.Value > DateTime.UtcNow.AddYears(1))
                {
                    return (false, "Discount cannot be valid for more than 1 year");
                }
            }

            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating plan discount");
            return (false, "Error validating discount data");
        }
    }

    #endregion

    #region Additional Plan Methods (for backward compatibility)

    /// <summary>
    /// Updates a subscription plan with comprehensive validation (for backward compatibility)
    /// </summary>
    /// <param name="planId">The unique identifier of the subscription plan to update</param>
    /// <param name="updateDto">DTO containing subscription plan update details</param>
    /// <param name="tokenModel">Token containing user authentication information</param>
    /// <returns>JsonModel containing the updated subscription plan or error information</returns>
    public async Task<JsonModel> UpdatePlanAsync(string planId, UpdateSubscriptionPlanDto updateDto, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Updating subscription plan {PlanId} by user {UserId}", planId, tokenModel.UserID);

            if (!Guid.TryParse(planId, out var planGuid))
            {
                return new JsonModel { data = new object(), Message = "Invalid plan ID format", StatusCode = 400 };
            }

            var existingPlan = await _subscriptionPlanRepository.GetByIdWithDetailsAsync(planGuid);
            if (existingPlan == null)
            {
                return new JsonModel { data = new object(), Message = "Subscription plan not found", StatusCode = 404 };
            }

            // CRITICAL: Check for active subscriptions
            var activeSubscriptionsCount = await _subscriptionPlanRepository
                .GetActiveSubscriptionsCountAsync(planGuid);

            // Decision: Create version if active subscriptions exist
            if (activeSubscriptionsCount > 0)
            {
                _logger.LogInformation(
                    "Plan {PlanId} has {Count} active subscriptions. Creating new version instead of updating.",
                    planGuid, activeSubscriptionsCount);

                // Use plan versioning service to create new version
                return await _planVersioningService.CreateNewPlanVersionAsync(
                    planGuid,
                    updateDto,
                    tokenModel);
            }

            // No active subscriptions - safe to update in-place
            _logger.LogInformation("Plan {PlanId} has no active subscriptions. Updating in-place.", planGuid);

            // Update plan properties
            existingPlan.Name = updateDto.Name;
            existingPlan.Description = updateDto.Description;
            existingPlan.BasePrice = updateDto.BasePrice;
            existingPlan.BillingCycleId = updateDto.BillingCycleId;
            existingPlan.CurrencyId = updateDto.CurrencyId;
            existingPlan.CategoryId = updateDto.CategoryId;
            existingPlan.IsActive = updateDto.IsActive;
            existingPlan.IsMostPopular = updateDto.IsMostPopular;
            existingPlan.IsTrending = updateDto.IsTrending;
            existingPlan.DisplayOrder = updateDto.DisplayOrder ?? existingPlan.DisplayOrder;
            
            existingPlan.IsAutoCalculatedPrice = updateDto.IsAutoCalculatedPrice;
            existingPlan.AdminCommissionPercent = updateDto.AdminCommissionPercent;
            existingPlan.PriceChangeNoticeDays = updateDto.PriceChangeNoticeDays;
            existingPlan.BillingDiscountPercentage = updateDto.BillingDiscountPercentage;
            existingPlan.DiscountPercentage = updateDto.DiscountPercentage;
            existingPlan.DiscountValidUntil = updateDto.DiscountValidUntil;
            
            // Auto-recalculate BasePrice if using auto-calculation
            if (existingPlan.IsAutoCalculatedPrice)
            {
                var privilegesTotalCost = await CalculatePrivilegesTotalCostAsync(existingPlan);
                var systemSettings = await _systemSettingsRepository.GetSettingsAsync();
                var defaultCommission = systemSettings?.DefaultAdminCommissionPercent ?? 0;
                
                var (calculatedPrice, _, _) = BillingCalculationService.CalculateFinalPlanPrice(
                    privilegesTotalCost,
                    existingPlan.AdminCommissionPercent,
                    defaultCommission,
                    _logger);
                
                existingPlan.BasePrice = calculatedPrice;
                existingPlan.PrivilegesTotalCost = privilegesTotalCost;
                
                _logger.LogInformation("Auto-recalculated BasePrice for plan {PlanId}: ${BasePrice}", 
                    planGuid, calculatedPrice);
            }
            
            existingPlan.UpdatedBy = tokenModel.UserID;
            existingPlan.UpdatedDate = DateTime.UtcNow;

            await _subscriptionPlanRepository.UpdateAsync(existingPlan);
            await _unitOfWork.SaveChangesAsync();

            // Synchronize with Stripe
            _logger.LogInformation("Synchronizing updated plan {PlanName} with Stripe", existingPlan.Name);
            var syncSuccess = await _stripeSyncService.SynchronizeSubscriptionPlanAsync(existingPlan.Id, tokenModel);
            
            if (!syncSuccess)
            {
                _logger.LogWarning("Failed to synchronize plan {PlanName} with Stripe", existingPlan.Name);
            }

            var updatedPlanDto = _mapper.Map<SubscriptionPlanDto>(existingPlan);
            return new JsonModel 
            { 
                data = updatedPlanDto, 
                Message = "Subscription plan updated successfully", 
                StatusCode = 200 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating subscription plan {PlanId}", planId);
            return new JsonModel { data = new object(), Message = "Error updating subscription plan", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Gets the effective price for a subscription plan with all discounts applied
    /// </summary>
    public async Task<JsonModel> GetEffectivePriceAsync(string planId, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Calculating effective price for plan {PlanId} by user {UserId}", planId, tokenModel?.UserID ?? 0);

            if (!Guid.TryParse(planId, out var planGuid))
            {
                return new JsonModel { data = new object(), Message = "Invalid plan ID format", StatusCode = 400 };
            }

            var plan = await _subscriptionPlanRepository.GetByIdWithDetailsAsync(planGuid);
            if (plan == null)
            {
                return new JsonModel { data = new object(), Message = "Plan not found", StatusCode = 404 };
            }

            var effectivePrice = BillingCalculationService.GetEffectivePlanPrice(plan, null, _logger);
            
            return new JsonModel 
            { 
                data = new 
                {
                    PlanId = planId,
                    BasePrice = plan.BasePrice,
                    EffectivePrice = effectivePrice,
                    DiscountPercentage = plan.DiscountPercentage,
                    BillingDiscountPercentage = plan.BillingDiscountPercentage,
                    DiscountValidUntil = plan.DiscountValidUntil,
                    CurrencyCode = plan.Currency?.Code ?? "USD",
                    CalculatedAt = DateTime.UtcNow
                }, 
                Message = "Effective price calculated successfully", 
                StatusCode = 200 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating effective price for plan {PlanId}", planId);
            return new JsonModel { data = new object(), Message = "Error calculating effective price", StatusCode = 500 };
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Calculates the total cost of all privileges associated with a plan.
    /// Used for BasePrice auto-recalculation when privileges change.
    /// </summary>
    /// <param name="plan">The subscription plan</param>
    /// <returns>Total cost of all privileges</returns>
    private async Task<decimal> CalculatePrivilegesTotalCostAsync(SubscriptionPlan plan)
    {
        var planPrivileges = await _planPrivilegeRepository.GetByPlanIdAsync(plan.Id);
        
        decimal totalCost = 0;
        foreach (var pp in planPrivileges)
        {
            totalCost += pp.Value * pp.PrivilegeBaseCost;
        }
        
        _logger.LogDebug("Calculated privileges total cost for plan {PlanId}: ${Cost}", 
            plan.Id, totalCost);
        
        return totalCost;
    }

    #endregion
}
