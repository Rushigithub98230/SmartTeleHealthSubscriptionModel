using AutoMapper;
using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Core.Enums;

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
        IPlanPricingService pricingService)
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

            if (createDto.Price <= 0)
            {
                return new JsonModel { data = new object(), Message = "Price must be greater than 0", StatusCode = 400 };
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

            // Check if plan with same name already exists
            var existingPlans = await _subscriptionPlanRepository.GetAllWithDetailsAsync();
            if (existingPlans.Any(p => p.Name.Equals(createDto.Name, StringComparison.OrdinalIgnoreCase)))
            {
                return new JsonModel { data = new object(), Message = "A plan with this name already exists", StatusCode = 400 };
            }

            // BEGIN TRANSACTION - Single atomic operation for all changes
            await _unitOfWork.BeginTransactionAsync();
            
            SubscriptionPlan createdPlan = null;
            string stripeProductId = null;
            string monthlyPriceId = null;
            string quarterlyPriceId = null;
            string annualPriceId = null;
            var invalidPrivileges = new List<Guid>();
            var assignedPrivilegesCount = 0;
            
            try
            {
                // STEP 1: Create plan entity in database first
                var plan = new SubscriptionPlan
                {
                    Name = createDto.Name,
                    Description = createDto.Description,
                    Price = createDto.Price,
                    BillingCycleId = createDto.BillingCycleId,
                    CurrencyId = createDto.CurrencyId,
                    CategoryId = createDto.CategoryId,
                    IsActive = createDto.IsActive,
                    DisplayOrder = createDto.DisplayOrder,
                    // Trial configuration
                    IsTrialAllowed = createDto.IsTrialAllowed,
                    TrialDurationInDays = createDto.TrialDurationInDays,
                    
                    // ═══════════════════════════════════════════════════════════
                    // HEALTHCARE PRICING MODEL (Choices 1c, 2c, 4d)
                    // ═══════════════════════════════════════════════════════════
                    VersionNumber = 1,  // Choice 3a: First version
                    IsLatestVersion = true,
                    ParentPlanId = null,
                    VersionCreatedDate = DateTime.UtcNow,
                    IsAutoCalculatedPrice = createDto.IsAutoCalculatedPrice,
                    AdminCommissionPercent = createDto.AdminCommissionPercent,
                    AdminCommissionFixed = createDto.AdminCommissionFixed,
                    PriceChangeNoticeDays = createDto.PriceChangeNoticeDays,
                    PrivilegesTotalCost = 0,  // Will be calculated if auto-pricing
                    
                    // Set audit properties for creation
                    CreatedBy = tokenModel.UserID,
                    CreatedDate = DateTime.UtcNow
                };

                createdPlan = await _subscriptionPlanRepository.CreatePlanAsync(plan);

                // STEP 2: Create Stripe resources
                _logger.LogInformation("Creating Stripe resources for plan {PlanName}", createdPlan.Name);
                
                // Create Stripe product
                stripeProductId = await _stripeService.CreateProductAsync(createdPlan.Name, createdPlan.Description ?? "", tokenModel);
                createdPlan.StripeProductId = stripeProductId;

                // Create Stripe prices for different billing cycles
                monthlyPriceId = await _stripeService.CreatePriceAsync(
                    stripeProductId, createdPlan.Price, "usd", "month", 1, tokenModel);
                createdPlan.StripeMonthlyPriceId = monthlyPriceId;

                quarterlyPriceId = await _stripeService.CreatePriceAsync(
                    stripeProductId, createdPlan.Price * 3, "usd", "month", 3, tokenModel);
                createdPlan.StripeQuarterlyPriceId = quarterlyPriceId;

                annualPriceId = await _stripeService.CreatePriceAsync(
                    stripeProductId, createdPlan.Price * 12, "usd", "month", 12, tokenModel);
                createdPlan.StripeAnnualPriceId = annualPriceId;

                // STEP 3: Update plan with Stripe IDs (CRITICAL STEP)
                await _subscriptionPlanRepository.UpdatePlanAsync(createdPlan);

                _logger.LogInformation("Successfully created Stripe resources for plan {PlanName}: Product {ProductId}, Prices {MonthlyId}, {QuarterlyId}, {AnnualId}", 
                    createdPlan.Name, stripeProductId, monthlyPriceId, quarterlyPriceId, annualPriceId);

                // STEP 4: Process privileges if provided (SAME TRANSACTION - NO NESTED!)
                if (createDto.Privileges != null && createDto.Privileges.Any())
                {
                    foreach (var privilege in createDto.Privileges)
                    {
                        // Validate privilege exists
                        var privilegeEntity = await _privilegeRepository.GetByIdAsync(privilege.PrivilegeId);
                        if (privilegeEntity == null)
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
                            UsagePeriodId = privilege.UsagePeriodId,
                            DurationMonths = privilege.DurationMonths,
                            ExpirationDate = privilege.ExpirationDate,
                            DailyLimit = privilege.DailyLimit,
                            WeeklyLimit = privilege.WeeklyLimit,
                            MonthlyLimit = privilege.MonthlyLimit,
                            
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
                if (createdPlan.IsAutoCalculatedPrice && assignedPrivilegesCount > 0)
                {
                    _logger.LogInformation("Auto-calculating price for plan {PlanId} based on privileges", createdPlan.Id);
                    
                    // Get pricing breakdown (includes privilegesTotalCost)
                    var breakdown = await _pricingService.CalculatePricingBreakdownAsync(createdPlan.Id);
                    
                    // Update plan with calculated price
                    createdPlan.Price = breakdown.FinalPrice;
                    createdPlan.PrivilegesTotalCost = breakdown.PrivilegesTotalCost;
                    
                    await _subscriptionPlanRepository.UpdatePlanAsync(createdPlan);
                    
                    _logger.LogInformation(
                        "Auto-calculated price for plan {PlanName}: ${Price} (Privileges: ${PrivTotal}, Commission: ${Comm})",
                        createdPlan.Name, breakdown.FinalPrice, breakdown.PrivilegesTotalCost, breakdown.CommissionAmount);
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
                        
                        // Deactivate all prices
                        if (!string.IsNullOrEmpty(monthlyPriceId))
                            await _stripeService.DeactivatePriceAsync(monthlyPriceId, tokenModel);
                        if (!string.IsNullOrEmpty(quarterlyPriceId))
                            await _stripeService.DeactivatePriceAsync(quarterlyPriceId, tokenModel);
                        if (!string.IsNullOrEmpty(annualPriceId))
                            await _stripeService.DeactivatePriceAsync(annualPriceId, tokenModel);
                        
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

            var plan = await _subscriptionPlanRepository.GetByIdWithDetailsAsync(Guid.Parse(planId));
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
            await _subscriptionPlanRepository.UpdatePlanAsync(plan);
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

            // Check if plan exists
            var plan = await _subscriptionPlanRepository.GetByIdWithDetailsAsync(planId);
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
                // Validate privilege exists
                var privilegeEntity = await _privilegeRepository.GetByIdAsync(privilege.PrivilegeId);
                if (privilegeEntity == null)
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
                    UsagePeriodId = privilege.UsagePeriodId,
                    DurationMonths = privilege.DurationMonths,
                    ExpirationDate = privilege.ExpirationDate,
                    DailyLimit = privilege.DailyLimit,
                    WeeklyLimit = privilege.WeeklyLimit,
                    MonthlyLimit = privilege.MonthlyLimit,
                    PrivilegeBaseCost = privilege.PrivilegeBaseCost,
                    UnitCost = privilege.UnitCost,
                    IsActive = true,
                    CreatedBy = tokenModel.UserID,
                    CreatedDate = DateTime.UtcNow
                };

                await _planPrivilegeRepository.AddAsync(planPrivilege);
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
                plan.Price = breakdown.FinalPrice;
                plan.PrivilegesTotalCost = breakdown.PrivilegesTotalCost;
                plan.UpdatedBy = tokenModel.UserID;
                plan.UpdatedDate = DateTime.UtcNow;
                
                await _subscriptionPlanRepository.UpdatePlanAsync(plan);
                
                _logger.LogInformation("Recalculated price for plan {PlanName}: ${Price}", plan.Name, breakdown.FinalPrice);
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

            // Check if plan exists
            var plan = await _subscriptionPlanRepository.GetByIdWithDetailsAsync(planId);
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
            
            await _planPrivilegeRepository.UpdatePlanPrivilegeAsync(planPrivilege);
            
            // If plan has auto-calculated pricing, recalculate price
            if (plan.IsAutoCalculatedPrice)
            {
                _logger.LogInformation("Recalculating price for auto-priced plan {PlanId} after privilege removal", planId);
                
                var breakdown = await _pricingService.CalculatePricingBreakdownAsync(planId);
                plan.Price = breakdown.FinalPrice;
                plan.PrivilegesTotalCost = breakdown.PrivilegesTotalCost;
                plan.UpdatedBy = tokenModel.UserID;
                plan.UpdatedDate = DateTime.UtcNow;
                
                await _subscriptionPlanRepository.UpdatePlanAsync(plan);
                
                _logger.LogInformation("Recalculated price for plan {PlanName}: ${OldPrice} → ${NewPrice}", 
                    plan.Name, plan.Price, breakdown.FinalPrice);
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

            // Check if plan exists
            var plan = await _subscriptionPlanRepository.GetByIdWithDetailsAsync(planId);
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
            planPrivilege.UsagePeriodId = updatedPrivilegeDto.UsagePeriodId;
            planPrivilege.DurationMonths = updatedPrivilegeDto.DurationMonths;
            planPrivilege.ExpirationDate = updatedPrivilegeDto.ExpirationDate;
            planPrivilege.DailyLimit = updatedPrivilegeDto.DailyLimit;
            planPrivilege.WeeklyLimit = updatedPrivilegeDto.WeeklyLimit;
            planPrivilege.MonthlyLimit = updatedPrivilegeDto.MonthlyLimit;
            planPrivilege.PrivilegeBaseCost = updatedPrivilegeDto.PrivilegeBaseCost;
            planPrivilege.UnitCost = updatedPrivilegeDto.UnitCost;  // Update unit cost for overage billing
            planPrivilege.UpdatedBy = tokenModel.UserID;
            planPrivilege.UpdatedDate = DateTime.UtcNow;

            await _planPrivilegeRepository.UpdatePlanPrivilegeAsync(planPrivilege);
            
            // If plan has auto-calculated pricing, recalculate price
            if (plan.IsAutoCalculatedPrice)
            {
                _logger.LogInformation("Recalculating price for auto-priced plan {PlanId} after privilege update", planId);
                
                var breakdown = await _pricingService.CalculatePricingBreakdownAsync(planId);
                plan.Price = breakdown.FinalPrice;
                plan.PrivilegesTotalCost = breakdown.PrivilegesTotalCost;
                plan.UpdatedBy = tokenModel.UserID;
                plan.UpdatedDate = DateTime.UtcNow;
                
                await _subscriptionPlanRepository.UpdatePlanAsync(plan);
                
                _logger.LogInformation("Recalculated price for plan {PlanName}: ${NewPrice}", plan.Name, breakdown.FinalPrice);
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

            // Check if plan exists
            var plan = await _subscriptionPlanRepository.GetByIdWithDetailsAsync(planId);
            if (plan == null)
                return new JsonModel { data = new object(), Message = "Subscription plan not found", StatusCode = 404 };

            // Get plan privileges
            var planPrivileges = await _planPrivilegeRepository.GetByPlanIdAsync(planId);
            var privilegeDtos = planPrivileges.Select(pp => new PlanPrivilegeDto
            {
                PrivilegeId = pp.PrivilegeId,
                Value = pp.Value,
                UsagePeriodId = pp.UsagePeriodId,
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
    public async Task<JsonModel> UpdatePlanAsync(string planId, UpdateSubscriptionPlanDto updateDto, TokenModel tokenModel)
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

            var originalPrice = existingPlan.Price;
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

            // NEW: Handle price updates with Stripe synchronization
            if (updateDto.Price > 0 && updateDto.Price != originalPrice)
            {
                existingPlan.Price = updateDto.Price;
                
                // Sync price changes to Stripe if Stripe integration exists
                if (!string.IsNullOrEmpty(existingPlan.StripeProductId))
                {
                    try
                    {
                        _logger.LogInformation("Updating Stripe prices for plan {PlanName} from {OldPrice} to {NewPrice}", 
                            existingPlan.Name, originalPrice, updateDto.Price);
                        
                        // Update monthly price
                        if (!string.IsNullOrEmpty(existingPlan.StripeMonthlyPriceId))
                        {
                            newMonthlyPriceId = await _stripeService.UpdatePriceWithNewPriceAsync(
                                existingPlan.StripeMonthlyPriceId, 
                                existingPlan.StripeProductId, 
                                updateDto.Price, 
                                "usd", 
                                "month", 
                                1, 
                                tokenModel
                            );
                            existingPlan.StripeMonthlyPriceId = newMonthlyPriceId;
                        }
                        
                        // Update quarterly price (3x monthly)
                        if (!string.IsNullOrEmpty(existingPlan.StripeQuarterlyPriceId))
                        {
                            newQuarterlyPriceId = await _stripeService.UpdatePriceWithNewPriceAsync(
                                existingPlan.StripeQuarterlyPriceId, 
                                existingPlan.StripeProductId, 
                                updateDto.Price * 3, 
                                "usd", 
                                "month", 
                                3, 
                                tokenModel
                            );
                            existingPlan.StripeQuarterlyPriceId = newQuarterlyPriceId;
                        }
                        
                        // Update annual price (12x monthly)
                        if (!string.IsNullOrEmpty(existingPlan.StripeAnnualPriceId))
                        {
                            newAnnualPriceId = await _stripeService.UpdatePriceWithNewPriceAsync(
                                existingPlan.StripeAnnualPriceId, 
                                existingPlan.StripeProductId, 
                                updateDto.Price * 12, 
                                "usd", 
                                "month", 
                                12, 
                                tokenModel
                            );
                            existingPlan.StripeAnnualPriceId = newAnnualPriceId;
                        }
                        
                        _logger.LogInformation("Successfully updated Stripe prices for plan {PlanName}", existingPlan.Name);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error updating Stripe prices for plan {PlanName}. Failing operation to maintain DB-Stripe consistency.", existingPlan.Name);
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

                var updatedPlan = await _subscriptionPlanRepository.UpdatePlanAsync(existingPlan);
                
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

            // Check if plan has active subscriptions
            var activeSubscriptions = await _subscriptionRepository.GetActiveSubscriptionsAsync();
            if (activeSubscriptions.Any(s => s.SubscriptionPlanId == existingPlan.Id))
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
                        // Deactivate all prices
                        if (!string.IsNullOrEmpty(existingPlan.StripeMonthlyPriceId))
                        {
                            await _stripeService.DeactivatePriceAsync(existingPlan.StripeMonthlyPriceId, tokenModel);
                        }
                        if (!string.IsNullOrEmpty(existingPlan.StripeQuarterlyPriceId))
                        {
                            await _stripeService.DeactivatePriceAsync(existingPlan.StripeQuarterlyPriceId, tokenModel);
                        }
                        if (!string.IsNullOrEmpty(existingPlan.StripeAnnualPriceId))
                        {
                            await _stripeService.DeactivatePriceAsync(existingPlan.StripeAnnualPriceId, tokenModel);
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
                
                var result = await _subscriptionPlanRepository.UpdatePlanAsync(existingPlan);
                if (result == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return new JsonModel { data = new object(), Message = "Failed to deactivate subscription plan", StatusCode = 500 };
                }

                await _unitOfWork.CommitTransactionAsync();
                
                _logger.LogInformation("Successfully deactivated subscription plan {PlanName} by user {UserId}", existingPlan.Name, tokenModel?.UserID ?? 0);
                
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
                
                var result = await _subscriptionPlanRepository.UpdatePlanAsync(existingPlan);
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

            // Check if plan has active subscriptions
            var activeSubscriptions = await _subscriptionRepository.GetActiveSubscriptionsAsync();
            if (activeSubscriptions.Any(s => s.SubscriptionPlanId == existingPlan.Id))
            {
                return new JsonModel { data = new object(), Message = "Cannot deactivate plan with active subscriptions. Please wait for all subscriptions to end or cancel them first.", StatusCode = 400 };
            }

            // BEGIN TRANSACTION - Ensure database and Stripe operations are atomic
            await _unitOfWork.BeginTransactionAsync();
            
            // Track Stripe cleanup for potential recovery
            bool stripeCleanedUp = false;
            string originalProductId = existingPlan.StripeProductId;
            string originalMonthlyPriceId = existingPlan.StripeMonthlyPriceId;
            string originalQuarterlyPriceId = existingPlan.StripeQuarterlyPriceId;
            string originalAnnualPriceId = existingPlan.StripeAnnualPriceId;
            
            try
            {
                // Clean up Stripe resources before deleting the plan
                if (!string.IsNullOrEmpty(existingPlan.StripeProductId))
                {
                    _logger.LogInformation("Cleaning up Stripe resources for plan {PlanName}", existingPlan.Name);
                    
                    try
                    {
                        // First, deactivate all prices
                        if (!string.IsNullOrEmpty(existingPlan.StripeMonthlyPriceId))
                        {
                            await _stripeService.DeactivatePriceAsync(existingPlan.StripeMonthlyPriceId, tokenModel);
                        }
                        if (!string.IsNullOrEmpty(existingPlan.StripeQuarterlyPriceId))
                        {
                            await _stripeService.DeactivatePriceAsync(existingPlan.StripeQuarterlyPriceId, tokenModel);
                        }
                        if (!string.IsNullOrEmpty(existingPlan.StripeAnnualPriceId))
                        {
                            await _stripeService.DeactivatePriceAsync(existingPlan.StripeAnnualPriceId, tokenModel);
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

                // Set audit properties for deletion
                existingPlan.DeletedBy = tokenModel.UserID;
                existingPlan.DeletedDate = DateTime.UtcNow;
                existingPlan.UpdatedBy = tokenModel.UserID;
                existingPlan.UpdatedDate = DateTime.UtcNow;

                var result = await _subscriptionPlanRepository.DeletePlanAsync(planGuid);
                if (!result)
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
                            
                            // Recreate the prices
                            var recoveredMonthlyPriceId = await _stripeService.CreatePriceAsync(
                                recoveredProductId, existingPlan.Price, "usd", "month", 1, tokenModel);
                            var recoveredQuarterlyPriceId = await _stripeService.CreatePriceAsync(
                                recoveredProductId, existingPlan.Price * 3, "usd", "month", 3, tokenModel);
                            var recoveredAnnualPriceId = await _stripeService.CreatePriceAsync(
                                recoveredProductId, existingPlan.Price * 12, "usd", "month", 12, tokenModel);
                            
                            // Update the plan with recovered Stripe IDs
                            existingPlan.StripeProductId = recoveredProductId;
                            existingPlan.StripeMonthlyPriceId = recoveredMonthlyPriceId;
                            existingPlan.StripeQuarterlyPriceId = recoveredQuarterlyPriceId;
                            existingPlan.StripeAnnualPriceId = recoveredAnnualPriceId;
                            
                            await _subscriptionPlanRepository.UpdatePlanAsync(existingPlan);
                            
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
                        
                        // Recreate the prices
                        var recoveredMonthlyPriceId = await _stripeService.CreatePriceAsync(
                            recoveredProductId, existingPlan.Price, "usd", "month", 1, tokenModel);
                        var recoveredQuarterlyPriceId = await _stripeService.CreatePriceAsync(
                            recoveredProductId, existingPlan.Price * 3, "usd", "month", 3, tokenModel);
                        var recoveredAnnualPriceId = await _stripeService.CreatePriceAsync(
                            recoveredProductId, existingPlan.Price * 12, "usd", "month", 12, tokenModel);
                        
                        // Update the plan with recovered Stripe IDs
                        existingPlan.StripeProductId = recoveredProductId;
                        existingPlan.StripeMonthlyPriceId = recoveredMonthlyPriceId;
                        existingPlan.StripeQuarterlyPriceId = recoveredQuarterlyPriceId;
                        existingPlan.StripeAnnualPriceId = recoveredAnnualPriceId;
                        
                        await _subscriptionPlanRepository.UpdatePlanAsync(existingPlan);
                        
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
            
            csv.AppendLine($"{plan.Id},{name},{description},{plan.Price},{currency},{billingCycle},{category},{plan.IsActive},{plan.IsTrialAllowed},{trialDuration},{displayOrder},{features},{terms},{plan.CreatedDate:yyyy-MM-dd HH:mm:ss},{plan.UpdatedDate:yyyy-MM-dd HH:mm:ss},{totalSubscriptions}");
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
                AveragePrice = plans.Any() ? plans.Average(p => p.Price) : 0,
                TotalSubscriptions = 0, // Not available in DTO
                ExportDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC")
            },
            Plans = plans.Select(plan => new
            {
                PlanId = plan.Id,
                Name = plan.Name,
                Description = plan.Description,
                Price = plan.Price,
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

    #endregion
}
