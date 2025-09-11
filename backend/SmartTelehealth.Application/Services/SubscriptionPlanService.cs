using AutoMapper;
using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;

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
        ISubscriptionRepository subscriptionRepository)
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

            var plan = await _subscriptionPlanRepository.GetByIdAsync(planGuid);
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
    /// Retrieves subscription plans with comprehensive filtering, pagination, and sorting
    /// </summary>
    public async Task<JsonModel> GetSubscriptionPlansAsync(
        TokenModel? tokenModel = null,
        int page = 1,
        int pageSize = 50,
        string? searchTerm = null,
        string? categoryId = null,
        bool? isActive = null,
        string? sortColumn = "DisplayOrder",
        string? sortOrder = "asc",
        bool adminOnly = false)
    {
        try
        {
            // Validate admin access if required
            if (adminOnly && (tokenModel?.RoleID != 1 && tokenModel?.RoleID != 3))
            {
                return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };
            }

            _logger.LogInformation("Retrieving subscription plans with filters - Page: {Page}, PageSize: {PageSize}, SearchTerm: {SearchTerm}, CategoryId: {CategoryId}, IsActive: {IsActive}, SortColumn: {SortColumn}, SortOrder: {SortOrder}", 
                page, pageSize, searchTerm, categoryId, isActive, sortColumn, sortOrder);

            // Use the improved repository method with database-level operations
            var (plans, totalCount) = await _subscriptionPlanRepository.GetPlansWithPaginationAsync(
                page, pageSize, searchTerm, categoryId, isActive, sortColumn, sortOrder);

            var planDtos = _mapper.Map<IEnumerable<SubscriptionPlanDto>>(plans);

            // Create pagination metadata
            var paginationMeta = new Meta
            {
                TotalRecords = totalCount,
                PageSize = pageSize,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                DefaultPageSize = pageSize
            };

            return new JsonModel 
            { 
                data = planDtos,
                meta = paginationMeta,
                Message = "Subscription plans retrieved successfully", 
                StatusCode = 200 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving subscription plans with filters");
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
            if (tokenModel.RoleID != 1 && tokenModel.RoleID != 3)
            {
                return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };
            }

            _logger.LogInformation("Creating subscription plan '{PlanName}' by user {UserId}", createDto.Name, tokenModel?.UserID ?? 0);

            // Validate required fields
            if (string.IsNullOrWhiteSpace(createDto.Name))
            {
                return new JsonModel { data = new object(), Message = "Plan name is required", StatusCode = 400 };
            }

            // Check if plan with same name already exists
            var existingPlans = await _subscriptionPlanRepository.GetAllAsync();
            if (existingPlans.Any(p => p.Name.Equals(createDto.Name, StringComparison.OrdinalIgnoreCase)))
            {
                return new JsonModel { data = new object(), Message = "A plan with this name already exists", StatusCode = 400 };
            }

            // Create plan entity with all properties
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
                // Set audit properties for creation
                CreatedBy = tokenModel.UserID,
                CreatedDate = DateTime.UtcNow
            };

            var createdPlan = await _subscriptionPlanRepository.CreateAsync(plan);

            // Create Stripe product and prices for the plan
            try
            {
                // Create Stripe product
                var stripeProductId = await _stripeService.CreateProductAsync(createdPlan.Name, createdPlan.Description ?? "", tokenModel);
                createdPlan.StripeProductId = stripeProductId;

                // Create Stripe prices for different billing cycles
                var monthlyPriceId = await _stripeService.CreatePriceAsync(
                    stripeProductId, createdPlan.Price, "usd", "month", 1, tokenModel);
                createdPlan.StripeMonthlyPriceId = monthlyPriceId;

                var quarterlyPriceId = await _stripeService.CreatePriceAsync(
                    stripeProductId, createdPlan.Price * 3, "usd", "month", 3, tokenModel);
                createdPlan.StripeQuarterlyPriceId = quarterlyPriceId;

                var annualPriceId = await _stripeService.CreatePriceAsync(
                    stripeProductId, createdPlan.Price * 12, "usd", "month", 12, tokenModel);
                createdPlan.StripeAnnualPriceId = annualPriceId;

                // Update plan with Stripe IDs
                await _subscriptionPlanRepository.UpdateAsync(createdPlan);

                _logger.LogInformation("Successfully created Stripe resources for plan {PlanName}: Product {ProductId}, Prices {MonthlyId}, {QuarterlyId}, {AnnualId}", 
                    createdPlan.Name, stripeProductId, monthlyPriceId, quarterlyPriceId, annualPriceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create Stripe resources for plan {PlanName}. Plan created but Stripe integration failed.", createdPlan.Name);
                // Don't fail the entire operation, just log the error
            }

            // Process privileges if provided
            if (createDto.Privileges != null && createDto.Privileges.Any())
            {
                foreach (var privilege in createDto.Privileges)
                {
                    // Validate privilege exists
                    var privilegeEntity = await _privilegeRepository.GetByIdAsync(privilege.PrivilegeId);
                    if (privilegeEntity == null)
                    {
                        _logger.LogWarning("Privilege {PrivilegeId} not found, skipping privilege assignment", privilege.PrivilegeId);
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
                        // Set audit properties for creation
                        IsActive = true,
                        CreatedBy = tokenModel.UserID,
                        CreatedDate = DateTime.UtcNow
                    };

                    await _planPrivilegeRepository.AddAsync(planPrivilege);
                }
            }

            var planDto = _mapper.Map<SubscriptionPlanDto>(createdPlan);

            _logger.LogInformation("Successfully created subscription plan {PlanId} by user {UserId}", createdPlan.Id, tokenModel?.UserID ?? 0);
            return new JsonModel { data = planDto, Message = "Plan created successfully with privileges", StatusCode = 201 };
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
            if (tokenModel.RoleID != 1 && tokenModel.RoleID != 3)
            {
                return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };
            }

            _logger.LogInformation("Activating subscription plan {PlanId} by user {UserId}", planId, tokenModel?.UserID ?? 0);

            var plan = await _subscriptionPlanRepository.GetByIdAsync(Guid.Parse(planId));
            if (plan == null)
                return new JsonModel { data = new object(), Message = "Plan not found", StatusCode = 404 };
            
            plan.IsActive = true;
            await _subscriptionPlanRepository.UpdateAsync(plan);
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

    /// <summary>
    /// Retrieves all active subscription plans (convenience method)
    /// </summary>
    public async Task<JsonModel> GetActiveSubscriptionPlansAsync(TokenModel tokenModel)
    {
        return await GetSubscriptionPlansAsync(tokenModel, isActive: true, adminOnly: true);
    }

    /// <summary>
    /// Retrieves subscription plans by category (convenience method)
    /// </summary>
    public async Task<JsonModel> GetSubscriptionPlansByCategoryAsync(string category, TokenModel tokenModel)
    {
        if (!Guid.TryParse(category, out var categoryGuid))
        {
            return new JsonModel { data = new object(), Message = "Invalid category ID format", StatusCode = 400 };
        }
        return await GetSubscriptionPlansAsync(tokenModel, categoryId: category);
    }

    /// <summary>
    /// Retrieves a specific subscription plan by ID (convenience method)
    /// </summary>
    public async Task<JsonModel> GetSubscriptionPlanAsync(string planId, TokenModel tokenModel)
    {
        return await GetPlanByIdAsync(planId, tokenModel);
    }

    /// <summary>
    /// Retrieves all subscription plans with advanced filtering and pagination (convenience method)
    /// </summary>
    public async Task<JsonModel> GetAllSubscriptionPlansAsync(TokenModel tokenModel, string? searchTerm = null, string? categoryId = null, bool? isActive = null, int page = 1, int pageSize = 50)
    {
        return await GetSubscriptionPlansAsync(tokenModel, page, pageSize, searchTerm, categoryId, isActive, adminOnly: true);
    }


    /// <summary>
    /// Retrieves all subscription plans with pagination and filtering (convenience method)
    /// </summary>
    public async Task<JsonModel> GetAllPlansAsync(int page, int pageSize, string? searchTerm, string? categoryId, bool? isActive, TokenModel tokenModel)
    {
        return await GetSubscriptionPlansAsync(tokenModel, page, pageSize, searchTerm, categoryId, isActive, adminOnly: false);
    }

    /// <summary>
    /// Retrieves all public subscription plans (convenience method)
    /// </summary>
    public async Task<JsonModel> GetPublicPlansAsync()
    {
        return await GetSubscriptionPlansAsync(null, isActive: true, adminOnly: false);
    }

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
            if (tokenModel.RoleID != 1 && tokenModel.RoleID != 3)
            {
                return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };
            }

            _logger.LogInformation("Exporting subscription plans in {Format} format by user {UserId}", format, tokenModel?.UserID ?? 0);

            // Get filtered plans using the existing method
            var plansResult = await GetAllSubscriptionPlansAsync(tokenModel, searchTerm, categoryId, isActive, 1, int.MaxValue);
            
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
        try
        {
            _logger.LogInformation("Assigning privileges to plan {PlanId} by user {UserId}", planId, tokenModel?.UserID ?? 0);

            // Check admin access
            if (tokenModel?.RoleID != 1)
                return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };

            // Check if plan exists
            var plan = await _subscriptionPlanRepository.GetByIdAsync(planId);
            if (plan == null)
                return new JsonModel { data = new object(), Message = "Subscription plan not found", StatusCode = 404 };

            // Validate and assign privileges
            int assignedCount = 0;
            foreach (var privilege in privileges)
            {
                // Validate privilege exists
                var privilegeEntity = await _privilegeRepository.GetByIdAsync(privilege.PrivilegeId);
                if (privilegeEntity == null)
                {
                    _logger.LogWarning("Privilege {PrivilegeId} not found, skipping", privilege.PrivilegeId);
                    continue;
                }

                // Create plan privilege
                var planPrivilege = new SubscriptionPlanPrivilege
                {
                    SubscriptionPlanId = planId,
                    PrivilegeId = privilege.PrivilegeId,
                    Value = privilege.Value,
                    UsagePeriodId = privilege.UsagePeriodId,
                    DurationMonths = privilege.DurationMonths,
                    ExpirationDate = privilege.ExpirationDate,
                    CreatedDate = DateTime.UtcNow
                };

                await _planPrivilegeRepository.AddAsync(planPrivilege);
                assignedCount++;
            }

            return new JsonModel { data = new object(), Message = $"Successfully assigned {assignedCount} privileges to plan", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning privileges to plan {PlanId}", planId);
            return new JsonModel { data = new object(), Message = "Failed to assign privileges to plan", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Removes a privilege from a subscription plan
    /// </summary>
    public async Task<JsonModel> RemovePrivilegeFromPlanAsync(Guid planId, Guid privilegeId, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Removing privilege {PrivilegeId} from plan {PlanId} by user {UserId}", privilegeId, planId, tokenModel?.UserID ?? 0);

            // Check admin access
            if (tokenModel?.RoleID != 1)
                return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };

            // Check if plan exists
            var plan = await _subscriptionPlanRepository.GetByIdAsync(planId);
            if (plan == null)
                return new JsonModel { data = new object(), Message = "Subscription plan not found", StatusCode = 404 };

            // Find and remove the privilege
            var planPrivileges = await _planPrivilegeRepository.GetByPlanIdAsync(planId);
            var planPrivilege = planPrivileges.FirstOrDefault(pp => pp.PrivilegeId == privilegeId);
            
            if (planPrivilege == null)
                return new JsonModel { data = new object(), Message = "Privilege not found in plan", StatusCode = 404 };

            // Soft delete - set audit properties
            planPrivilege.IsDeleted = true;
            planPrivilege.DeletedBy = tokenModel.UserID;
            planPrivilege.DeletedDate = DateTime.UtcNow;
            planPrivilege.UpdatedBy = tokenModel.UserID;
            planPrivilege.UpdatedDate = DateTime.UtcNow;
            
            await _planPrivilegeRepository.UpdateAsync(planPrivilege);

            return new JsonModel { data = true, Message = "Privilege removed from plan successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing privilege {PrivilegeId} from plan {PlanId}", privilegeId, planId);
            return new JsonModel { data = new object(), Message = "Failed to remove privilege from plan", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Updates a privilege assignment for a subscription plan
    /// </summary>
    public async Task<JsonModel> UpdatePlanPrivilegeAsync(Guid planId, Guid privilegeId, PlanPrivilegeDto updatedPrivilegeDto, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Updating privilege {PrivilegeId} in plan {PlanId} by user {UserId}", privilegeId, planId, tokenModel?.UserID ?? 0);

            // Check admin access
            if (tokenModel?.RoleID != 1)
                return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };

            // Check if plan exists
            var plan = await _subscriptionPlanRepository.GetByIdAsync(planId);
            if (plan == null)
                return new JsonModel { data = new object(), Message = "Subscription plan not found", StatusCode = 404 };

            // Find the privilege
            var planPrivileges = await _planPrivilegeRepository.GetByPlanIdAsync(planId);
            var planPrivilege = planPrivileges.FirstOrDefault(pp => pp.PrivilegeId == privilegeId);
            
            if (planPrivilege == null)
                return new JsonModel { data = new object(), Message = "Privilege not found in plan", StatusCode = 404 };

            // Update the privilege
            planPrivilege.Value = updatedPrivilegeDto.Value;
            planPrivilege.UsagePeriodId = updatedPrivilegeDto.UsagePeriodId;
            planPrivilege.DurationMonths = updatedPrivilegeDto.DurationMonths;
            planPrivilege.ExpirationDate = updatedPrivilegeDto.ExpirationDate;
            planPrivilege.UpdatedBy = tokenModel.UserID;
            planPrivilege.UpdatedDate = DateTime.UtcNow;

            await _planPrivilegeRepository.UpdateAsync(planPrivilege);

            return new JsonModel { data = true, Message = "Plan privilege updated successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating privilege {PrivilegeId} in plan {PlanId}", privilegeId, planId);
            return new JsonModel { data = new object(), Message = "Failed to update plan privilege", StatusCode = 500 };
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
            var plan = await _subscriptionPlanRepository.GetByIdAsync(planId);
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
            if (tokenModel.RoleID != 1 && tokenModel.RoleID != 3)
            {
                return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };
            }

            _logger.LogInformation("Updating subscription plan {PlanId} by user {UserId}", planId, tokenModel?.UserID ?? 0);

            if (!Guid.TryParse(planId, out var planGuid))
            {
                return new JsonModel { data = new object(), Message = "Invalid plan ID format", StatusCode = 400 };
            }

            var existingPlan = await _subscriptionPlanRepository.GetByIdAsync(planGuid);
            if (existingPlan == null)
            {
                return new JsonModel { data = new object(), Message = "Subscription plan not found", StatusCode = 404 };
            }

            var originalPrice = existingPlan.Price;
            var originalName = existingPlan.Name;
            var originalDescription = existingPlan.Description;

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
                            var newMonthlyPriceId = await _stripeService.UpdatePriceWithNewPriceAsync(
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
                            var newQuarterlyPriceId = await _stripeService.UpdatePriceWithNewPriceAsync(
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
                            var newAnnualPriceId = await _stripeService.UpdatePriceWithNewPriceAsync(
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
                        _logger.LogError(ex, "Error updating Stripe prices for plan {PlanName}. Proceeding with local update only.", existingPlan.Name);
                        // Don't fail the entire operation if Stripe update fails
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
                        
                        _logger.LogInformation("Successfully updated Stripe product for plan {PlanName}", existingPlan.Name);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error updating Stripe product for plan {PlanName}. Proceeding with local update only.", existingPlan.Name);
                        existingPlan.Name = originalName;
                        existingPlan.Description = originalDescription;
                    }
                }
            }

            existingPlan.UpdatedBy = tokenModel?.UserID ?? 0;
            existingPlan.UpdatedDate = DateTime.UtcNow;

            var updatedPlan = await _subscriptionPlanRepository.UpdateAsync(existingPlan);
            var planDto = _mapper.Map<SubscriptionPlanDto>(updatedPlan);

            _logger.LogInformation("Successfully updated subscription plan {PlanId} by user {UserId}", planId, tokenModel?.UserID ?? 0);
            return new JsonModel { data = planDto, Message = "Subscription plan updated successfully with Stripe synchronization", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating subscription plan {PlanId} by user {UserId}", planId, tokenModel?.UserID ?? 0);
            return new JsonModel { data = new object(), Message = "Failed to update subscription plan", StatusCode = 500 };
        }
    }
    
    /// <summary>
    /// Deletes a subscription plan with comprehensive validation (for backward compatibility)
    /// </summary>
    public async Task<JsonModel> DeletePlanAsync(string planId, TokenModel tokenModel)
    {
        try
        {
            // Admin only method - validate admin role
            if (tokenModel.RoleID != 1 && tokenModel.RoleID != 3)
            {
                return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };
            }

            _logger.LogInformation("Deleting subscription plan {PlanId} by user {UserId}", planId, tokenModel?.UserID ?? 0);

            if (!Guid.TryParse(planId, out var planGuid))
            {
                return new JsonModel { data = new object(), Message = "Invalid plan ID format", StatusCode = 400 };
            }

            var existingPlan = await _subscriptionPlanRepository.GetByIdAsync(planGuid);
            if (existingPlan == null)
            {
                return new JsonModel { data = new object(), Message = "Subscription plan not found", StatusCode = 404 };
            }

            // Check if plan has active subscriptions
            var activeSubscriptions = await _subscriptionRepository.GetActiveSubscriptionsAsync();
            if (activeSubscriptions.Any(s => s.SubscriptionPlanId == existingPlan.Id))
            {
                return new JsonModel { data = new object(), Message = "Cannot delete plan with active subscriptions", StatusCode = 400 };
            }

            // NEW: Clean up Stripe resources before deleting the plan
            if (!string.IsNullOrEmpty(existingPlan.StripeProductId))
            {
                try
                {
                    _logger.LogInformation("Cleaning up Stripe resources for plan {PlanName}", existingPlan.Name);
                    
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
                    
                    // Delete the product (this will also deactivate all associated prices)
                    await _stripeService.DeleteProductAsync(existingPlan.StripeProductId, tokenModel);
                    
                    _logger.LogInformation("Successfully cleaned up Stripe resources for plan {PlanName}", existingPlan.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error cleaning up Stripe resources for plan {PlanName}. Proceeding with local deletion.", existingPlan.Name);
                    // Don't fail the entire operation if Stripe cleanup fails
                }
            }

            var result = await _subscriptionPlanRepository.DeleteAsync(planGuid);
            if (!result)
            {
                return new JsonModel { data = new object(), Message = "Failed to delete subscription plan", StatusCode = 500 };
            }

            _logger.LogInformation("Successfully deleted subscription plan {PlanId} by user {UserId}", planId, tokenModel?.UserID ?? 0);
            return new JsonModel { data = true, Message = "Subscription plan deleted successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting subscription plan {PlanId} by user {UserId}", planId, tokenModel?.UserID ?? 0);
            return new JsonModel { data = new object(), Message = "Error deleting subscription plan", StatusCode = 500 };
        }
    }
    
    /// <summary>
    /// Deactivates a subscription plan with admin user tracking (for backward compatibility)
    /// </summary>
    public async Task<JsonModel> DeactivatePlanAsync(string planId, string adminUserId, TokenModel tokenModel)
    {
        try
        {
            // Admin only method - validate admin role
            if (tokenModel.RoleID != 1 && tokenModel.RoleID != 3)
            {
                return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };
            }

            _logger.LogInformation("Deactivating subscription plan {PlanId} by admin {AdminUserId}", planId, adminUserId);

            if (!Guid.TryParse(planId, out var planGuid))
            {
                return new JsonModel { data = new object(), Message = "Invalid plan ID format", StatusCode = 400 };
            }

            var result = await _subscriptionPlanRepository.DeactivateAsync(planGuid);
            if (result)
            {
                _logger.LogInformation("Successfully deactivated subscription plan {PlanId} by admin {AdminUserId}", planId, adminUserId);
                return new JsonModel { data = true, Message = "Subscription plan deactivated successfully", StatusCode = 200 };
            }
            else
            {
                return new JsonModel { data = false, Message = "Failed to deactivate subscription plan", StatusCode = 500 };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating subscription plan {PlanId} by admin {AdminUserId}", planId, adminUserId);
            return new JsonModel { data = new object(), Message = "Error deactivating subscription plan", StatusCode = 500 };
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
        csv.AppendLine("Name,Description,Price,BillingCycleId,IsActive,Features,Terms,CreatedDate");
        
        foreach (var plan in plans)
        {
            csv.AppendLine($"\"{plan.Name}\",\"{plan.Description}\",{plan.Price},{plan.BillingCycleId},{plan.IsActive},\"{plan.Features ?? ""}\",\"{plan.Terms ?? ""}\",{plan.CreatedDate:yyyy-MM-dd}");
        }
        
        return csv.ToString();
    }

    /// <summary>
    /// Generates Excel data for subscription plans export
    /// </summary>
    private string GenerateSubscriptionPlansExcel(IEnumerable<SubscriptionPlanDto> plans)
    {
        // For now, return CSV format as Excel generation would require additional libraries
        // In a real implementation, you'd use EPPlus or similar library
        return GenerateSubscriptionPlansCsv(plans);
    }

    #endregion
}
