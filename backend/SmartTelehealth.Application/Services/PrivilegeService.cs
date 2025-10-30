using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Utilities;
using SmartTelehealth.Core.DTOs;

namespace SmartTelehealth.Application.Services;

/// <summary>
/// Privilege management service that handles all privilege-related operations including:
/// - Privilege usage validation and enforcement
/// - Time-based limit checking (daily, weekly, monthly)
/// - Usage tracking and increment operations
/// - Remaining privilege calculation
/// - Privilege history and audit trails
/// - Subscription plan privilege management
/// - Access control based on subscription status
/// </summary>
public class PrivilegeService : IPrivilegeService
{
    private readonly IPrivilegeRepository _privilegeRepo;
    private readonly ISubscriptionPlanPrivilegeRepository _planPrivilegeRepo;
    private readonly IUserSubscriptionPrivilegeUsageRepository _usageRepo;
    private readonly IPrivilegeUsageHistoryRepository _usageHistoryRepo;
    private readonly ISubscriptionRepository _subscriptionRepo;
    private readonly ILogger<PrivilegeService> _logger;

    /// <summary>
    /// Initializes a new instance of the PrivilegeService with all required dependencies
    /// </summary>
    /// <param name="privilegeRepo">Repository for privilege data access operations</param>
    /// <param name="planPrivilegeRepo">Repository for subscription plan privilege data access</param>
    /// <param name="usageRepo">Repository for user subscription privilege usage tracking</param>
    /// <param name="usageHistoryRepo">Repository for privilege usage history data access</param>
    /// <param name="subscriptionRepo">Repository for subscription data access operations</param>
    /// <param name="logger">Logger instance for logging operations and errors</param>
    public PrivilegeService(
        IPrivilegeRepository privilegeRepo,
        ISubscriptionPlanPrivilegeRepository planPrivilegeRepo,
        IUserSubscriptionPrivilegeUsageRepository usageRepo,
        IPrivilegeUsageHistoryRepository usageHistoryRepo,
        ISubscriptionRepository subscriptionRepo,
        ILogger<PrivilegeService> logger)
    {
        _privilegeRepo = privilegeRepo;
        _planPrivilegeRepo = planPrivilegeRepo;
        _usageRepo = usageRepo;
        _usageHistoryRepo = usageHistoryRepo;
        _subscriptionRepo = subscriptionRepo;
        _logger = logger;
    }

    #region Private Helper Methods

    /// <summary>
    /// Helper method to get SubscriptionPlanPrivilege by subscription ID and privilege name
    /// </summary>
    /// <param name="subscriptionId">The subscription ID to get privileges for</param>
    /// <param name="privilegeName">The name of the privilege to retrieve</param>
    /// <returns>SubscriptionPlanPrivilege if found and subscription is active, null otherwise</returns>
    /// <remarks>
    /// This method:
    /// - Retrieves the subscription to get the plan ID
    /// - Validates that the subscription is active and not deleted
    /// - Checks that the subscription status allows privilege usage (Active or Trial)
    /// - Retrieves plan privileges and finds the matching privilege by name
    /// </remarks>
    private async Task<SubscriptionPlanPrivilege?> GetPlanPrivilegeAsync(Guid subscriptionId, string privilegeName)
    {
        // Fetch the subscription to get the planId
        var subscription = await _subscriptionRepo.GetByIdWithDetailsAsync(subscriptionId);
        if (subscription == null) return null;
        
        // Check if subscription is active and allows privilege usage
        if (!subscription.IsActive || subscription.IsDeleted || 
            subscription.Status != "Active" && subscription.Status != "Trial")
        {
            return null;
        }
        
        // Get plan privileges and find the matching privilege by name
        var planPrivileges = await _planPrivilegeRepo.GetByPlanIdAsync(subscription.SubscriptionPlanId);
        return planPrivileges.FirstOrDefault(pp => pp.Privilege.Name == privilegeName);
    }
    #endregion

    #region Public Methods

    /// <summary>
    /// Gets the remaining usage count for a specific privilege in a subscription
    /// </summary>
    /// <param name="subscriptionId">The subscription ID to check privileges for</param>
    /// <param name="privilegeName">The name of the privilege to check</param>
    /// <param name="tokenModel">Token containing user authentication information for audit purposes</param>
    /// <returns>The remaining usage count for the privilege (0 if disabled, int.MaxValue if unlimited)</returns>
    /// <remarks>
    /// This method:
    /// - Retrieves the plan privilege configuration
    /// - Returns 0 if privilege is disabled or subscription is inactive
    /// - Returns int.MaxValue if privilege is unlimited (-1)
    /// - Calculates remaining usage by subtracting used amount from allowed amount
    /// - Logs the remaining count for audit purposes
    /// - Returns 0 on any error for safety
    /// </remarks>
    public async Task<int> GetRemainingPrivilegeAsync(Guid subscriptionId, string privilegeName, TokenModel tokenModel)
    {
        try
        {
            // Get the plan privilege configuration
            var planPrivilege = await GetPlanPrivilegeAsync(subscriptionId, privilegeName);
            if (planPrivilege == null) return 0;
            
            // Check if privilege is disabled
            if (planPrivilege.Value == 0) return 0;
            
            // Check if privilege is unlimited
            if (planPrivilege.Value == -1) return int.MaxValue;
            
            // Get current usage and calculate remaining
            var usage = (await _usageRepo.GetBySubscriptionIdAsync(subscriptionId))
                .FirstOrDefault(u => u.SubscriptionPlanPrivilegeId == planPrivilege.Id);
            
            // If no usage record exists yet, use plan's Value as initial limit
            if (usage == null)
            {
                var initialLimit = planPrivilege.Value == -1 ? int.MaxValue : Math.Max(0, planPrivilege.Value);
                _logger.LogInformation("No usage record yet for privilege '{PrivilegeName}' for subscription {SubscriptionId}. Using plan limit: {InitialLimit}", 
                    privilegeName, subscriptionId, initialLimit);
                return initialLimit;
            }
            
            // Use the DYNAMIC AllowedValue (can increase with credit purchases) not the static plan Value!
            var used = usage.UsedValue;
            var allowed = usage.AllowedValue;
            var remaining = allowed == -1 ? int.MaxValue : Math.Max(0, allowed - used);
            
            _logger.LogInformation("Remaining privilege '{PrivilegeName}' for subscription {SubscriptionId} by user {UserId}: {Remaining} (Allowed: {Allowed}, Used: {Used})", 
                privilegeName, subscriptionId, tokenModel.UserID, remaining, allowed, used);
            return remaining;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting remaining privilege '{PrivilegeName}' for subscription {SubscriptionId} by user {UserId}", 
                privilegeName, subscriptionId, tokenModel.UserID);
            return 0;
        }
    }

    // Check time-based limits for a privilege
    // Time-based limit checking method removed - overage now only applies when total AllowedValue is exhausted

    /// <summary>
    /// Uses a privilege by incrementing the usage count with comprehensive validation
    /// </summary>
    /// <param name="subscriptionId">The subscription ID to use the privilege for</param>
    /// <param name="privilegeName">The name of the privilege to use</param>
    /// <param name="amount">The amount of privilege usage to consume</param>
    /// <param name="tokenModel">Token containing user authentication information for audit purposes</param>
    /// <returns>True if privilege usage was successful, false otherwise</returns>
    /// <remarks>
    /// This method performs comprehensive validation and usage tracking:
    /// 1. Validates input parameters (amount must be positive)
    /// 2. Retrieves and validates the plan privilege configuration
    /// 3. Checks if privilege is disabled (returns false if so)
    /// 4. Validates time-based limits (daily, weekly, monthly)
    /// 5. Handles unlimited privileges (-1) by allowing usage without limit checks
    /// 6. For limited privileges, checks remaining usage before allowing
    /// 7. Creates or updates usage records with audit information
    /// 8. Records usage history for detailed tracking
    /// 9. Logs all operations for audit purposes
    /// 
    /// Business Rules:
    /// - Only active subscriptions can use privileges
    /// - Time-based limits are enforced before quantity limits
    /// - Usage is tracked with timestamps and user information
    /// - Failed operations are logged but don't throw exceptions
    /// </remarks>
    public async Task<bool> UsePrivilegeAsync(Guid subscriptionId, string privilegeName, int amount, TokenModel tokenModel)
    {
        try
        {
            // Validate input parameters - amount must be positive
            if (amount <= 0) return false;
            
            // Get the plan privilege configuration
            var planPrivilege = await GetPlanPrivilegeAsync(subscriptionId, privilegeName);
            if (planPrivilege == null) return false;
            
            // Check if privilege is disabled
            if (planPrivilege.Value == 0) return false;
            
            // Time-based limits removed - only check total AllowedValue
            
            // Handle unlimited privileges
            if (planPrivilege.Value == -1)
            {
                // For unlimited privileges, we can always use them
                var unlimitedUsage = (await _usageRepo.GetBySubscriptionIdAsync(subscriptionId))
                    .FirstOrDefault(u => u.SubscriptionPlanPrivilegeId == planPrivilege.Id);
                if (unlimitedUsage == null)
                {
                    var (allowedValue, periodStart, periodEnd) = await CalculatePrivilegeAllocationAsync(subscriptionId, planPrivilege);
                    unlimitedUsage = new UserSubscriptionPrivilegeUsage
                    {
                        SubscriptionId = subscriptionId,
                        SubscriptionPlanPrivilegeId = planPrivilege.Id,
                        UsedValue = amount,
                        AllowedValue = allowedValue,
                        UsagePeriodStart = periodStart,
                        UsagePeriodEnd = periodEnd,
                        LastUsedAt = DateTime.UtcNow,
                        // Set audit properties for creation
                        IsActive = true,
                        CreatedBy = tokenModel.UserID,
                        CreatedDate = DateTime.UtcNow
                    };
                    await _usageRepo.AddAsync(unlimitedUsage);
                }
                else
                {
                    unlimitedUsage.UsedValue += amount;
                    unlimitedUsage.LastUsedAt = DateTime.UtcNow;
                    unlimitedUsage.UpdatedBy = tokenModel.UserID;
                    unlimitedUsage.UpdatedDate = DateTime.UtcNow;
                    await _usageRepo.UpdateUsageAsync(unlimitedUsage);
                }

                // Add usage history
                await AddUsageHistoryAsync(unlimitedUsage.Id, amount, tokenModel);
                
                _logger.LogInformation("Unlimited privilege '{PrivilegeName}' used for subscription {SubscriptionId} by user {UserId}: amount {Amount}", 
                    privilegeName, subscriptionId, tokenModel.UserID, amount);
                return true;
            }
            
            // For limited privileges, check remaining amount
            var remaining = await GetRemainingPrivilegeAsync(subscriptionId, privilegeName, tokenModel);
            if (remaining < amount) return false;
            
            var limitedUsage = (await _usageRepo.GetBySubscriptionIdAsync(subscriptionId))
                .FirstOrDefault(u => u.SubscriptionPlanPrivilegeId == planPrivilege.Id);
            if (limitedUsage == null)
            {
                var (allowedValue, periodStart, periodEnd) = await CalculatePrivilegeAllocationAsync(subscriptionId, planPrivilege);
                limitedUsage = new UserSubscriptionPrivilegeUsage
                {
                    SubscriptionId = subscriptionId,
                    SubscriptionPlanPrivilegeId = planPrivilege.Id,
                    UsedValue = amount,
                    AllowedValue = allowedValue,
                    UsagePeriodStart = periodStart,
                    UsagePeriodEnd = periodEnd,
                    LastUsedAt = DateTime.UtcNow,
                    // Set audit properties for creation
                    IsActive = true,
                    CreatedBy = tokenModel.UserID,
                    CreatedDate = DateTime.UtcNow
                };
                await _usageRepo.AddAsync(limitedUsage);
            }
            else
            {
                limitedUsage.UsedValue += amount;
                limitedUsage.LastUsedAt = DateTime.UtcNow;
                limitedUsage.UpdatedBy = tokenModel.UserID;
                limitedUsage.UpdatedDate = DateTime.UtcNow;
                await _usageRepo.UpdateUsageAsync(limitedUsage);
            }

            // Add usage history
            await AddUsageHistoryAsync(limitedUsage.Id, amount, tokenModel);
                
            _logger.LogInformation("Privilege '{PrivilegeName}' used for subscription {SubscriptionId} by user {UserId}: amount {Amount}", 
                privilegeName, subscriptionId, tokenModel.UserID, amount);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error using privilege '{PrivilegeName}' for subscription {SubscriptionId} by user {UserId}", 
                privilegeName, subscriptionId, tokenModel.UserID);
            return false;
        }
    }

    // Add usage history record
    private async Task AddUsageHistoryAsync(Guid userSubscriptionPrivilegeUsageId, int amount, TokenModel tokenModel)
    {
        try
        {
            var now = DateTime.UtcNow;
            var today = now.Date;
            var weekStart = today.AddDays(-(int)today.DayOfWeek);
            var monthStart = new DateTime(today.Year, today.Month, 1);

            var usageHistory = new PrivilegeUsageHistory
            {
                UserSubscriptionPrivilegeUsageId = userSubscriptionPrivilegeUsageId,
                UsedValue = amount,
                UsedAt = now,
                UsageDate = today,
                UsageWeek = $"{weekStart:yyyy}-{GetWeekNumber(weekStart):D2}",
                UsageMonth = $"{monthStart:yyyy-MM}",
                Notes = $"Used by user {tokenModel.UserID}",
                CreatedBy = tokenModel.UserID,
                CreatedDate = now
            };

            await _usageHistoryRepo.AddAsync(usageHistory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding usage history for user subscription privilege usage {Id}", userSubscriptionPrivilegeUsageId);
        }
    }

    private static int GetWeekNumber(DateTime date)
    {
        var calendar = System.Globalization.CultureInfo.InvariantCulture.Calendar;
        return calendar.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
    }

    // Get all privileges for a plan
    /// <summary>
    /// Retrieves all privileges associated with a specific subscription plan
    /// </summary>
    /// <param name="planId">The unique identifier of the subscription plan to get privileges for</param>
    /// <param name="tokenModel">Token containing user authentication information for audit purposes</param>
    /// <returns>Collection of Privilege entities associated with the plan</returns>
    /// <remarks>
    /// This method:
    /// - Retrieves subscription plan privileges from the repository
    /// - Extracts the privilege entities from plan privilege relationships
    /// - Returns all privileges available in the specified plan
    /// - Used for plan privilege management and display
    /// - Logs successful operations and errors
    /// - Returns empty collection on error for safety
    /// </remarks>
    public async Task<IEnumerable<Privilege>> GetPrivilegesForPlanAsync(Guid planId, TokenModel tokenModel)
    {
        try
        {
            // Retrieve plan privileges from repository
            var planPrivileges = await _planPrivilegeRepo.GetByPlanIdAsync(planId);
            
            // Extract privilege entities from plan privilege relationships
            var privileges = planPrivileges.Select(pp => pp.Privilege);
            
            _logger.LogInformation("Privileges retrieved for plan {PlanId} by user {UserId}: {PrivilegeCount} privileges", 
                planId, tokenModel.UserID, privileges.Count());
            return privileges;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting privileges for plan {PlanId} by user {UserId}", planId, tokenModel.UserID);
            return Enumerable.Empty<Privilege>();
        }
    }

    /// <summary>
    /// Retrieves all privileges with advanced filtering, searching, and pagination
    /// </summary>
    /// <param name="page">Page number for pagination (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="search">Search term to filter privileges by name or description</param>
    /// <param name="category">Category filter (currently not implemented)</param>
    /// <param name="status">Status filter (active/inactive)</param>
    /// <param name="tokenModel">Token containing user authentication and authorization information</param>
    /// <returns>JsonModel containing paginated and filtered privileges or error information</returns>
    /// <remarks>
    /// This method:
    /// - Retrieves all privileges from the repository
    /// - Applies search filter on name and description (case-insensitive)
    /// - Applies status filter if specified
    /// - Implements pagination for large result sets
    /// - Maps entities to DTOs for response
    /// - Used for administrative privilege management with advanced filtering
    /// - Logs errors for troubleshooting
    /// 
    /// Note: Category filtering is currently not implemented
    /// </remarks>
    public async Task<JsonModel> GetAllPrivilegesAsync(int page, int pageSize, string? search, string? category, string? status, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Retrieving privileges with database-level filtering - Page: {Page}, PageSize: {PageSize}, Search: {Search}, Category: {Category}, Status: {Status}", 
                page, pageSize, search, category, status);

            // Use database-level filtering, pagination, and sorting
            var (privileges, totalCount) = await _privilegeRepo.GetPrivilegesWithFilteringAsync(page, pageSize, search, category, status);

            var paginationMeta = new Meta
            {
                TotalRecords = totalCount,
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                HasNextPage = page < (int)Math.Ceiling((double)totalCount / pageSize),
                HasPreviousPage = page > 1
            };
            
            _logger.LogInformation("Privileges retrieved by user {UserId}: {PrivilegeCount} privileges (page {Page} of {TotalPages})", 
                tokenModel.UserID, privileges.Count(), page, paginationMeta.TotalPages);
            return new JsonModel 
            { 
                data = privileges, 
                meta = paginationMeta,
                Message = "Privileges retrieved successfully with database-level filtering", 
                StatusCode = 200 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving privileges by user {UserId}", tokenModel.UserID);
            return new JsonModel 
            { 
                data = new object(), 
                Message = "Error retrieving privileges", 
                StatusCode = 500 
            };
        }
    }

    // Get privilege by ID
    /// <summary>
    /// Retrieves a specific privilege by its ID
    /// </summary>
    /// <param name="id">The unique identifier of the privilege to retrieve</param>
    /// <param name="token">Token containing user authentication and authorization information</param>
    /// <returns>JsonModel containing the privilege data or error information</returns>
    /// <remarks>
    /// This method:
    /// - Validates the privilege ID format (must be valid GUID)
    /// - Retrieves the privilege from the repository
    /// - Maps entity to DTO for response
    /// - Returns 400 Bad Request for invalid ID format
    /// - Returns 404 Not Found if privilege doesn't exist
    /// - Used for detailed privilege information display
    /// - Logs errors for troubleshooting
    /// </remarks>
    public async Task<JsonModel> GetPrivilegeByIdAsync(string id, TokenModel token)
    {
        try
        {
            // Validate privilege ID format
            if (!Guid.TryParse(id, out var privilegeId))
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Invalid privilege ID format",
                    StatusCode = 400
                };
            }

            // Retrieve privilege from repository
            var privilege = await _privilegeRepo.GetByIdAsync(privilegeId);
            if (privilege == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Privilege not found",
                    StatusCode = 404
                };
            }

            return new JsonModel
            {
                data = privilege,
                Message = "Privilege retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving privilege {PrivilegeId} by user {UserId}", id, token.UserID);
            return new JsonModel
            {
                data = new object(),
                Message = "Error retrieving privilege",
                StatusCode = 500
            };
        }
    }

    // Create new privilege
    /// <summary>
    /// Creates a new privilege with proper validation and audit trail
    /// </summary>
    /// <param name="createDto">DTO containing privilege creation details</param>
    /// <param name="token">Token containing user authentication information for audit purposes</param>
    /// <returns>JsonModel containing the created privilege data or error information</returns>
    /// <remarks>
    /// This method:
    /// - Creates a new Privilege entity from the DTO
    /// - Sets audit properties (CreatedBy, CreatedDate)
    /// - Adds the privilege to the repository
    /// - Maps the created entity to DTO for response
    /// - Used for privilege management and administration
    /// - Logs errors for troubleshooting
    /// 
    /// Business Rules:
    /// - All privileges are created with audit fields
    /// - Created privileges are immediately available for use
    /// - Privilege type must be valid and exist
    /// </remarks>
    public async Task<JsonModel> CreatePrivilegeAsync(CreatePrivilegeDto createDto, TokenModel token)
    {
        try
        {
            // Create new privilege entity with audit fields
            var privilege = new Privilege
            {
                Name = createDto.Name,
                Description = createDto.Description,
                PrivilegeTypeId = createDto.PrivilegeTypeId,
                IsActive = createDto.IsActive,
                CreatedBy = token.UserID,
                CreatedDate = DateTime.UtcNow
            };

            // Add privilege to repository
            await _privilegeRepo.AddAsync(privilege);

            return new JsonModel
            {
                data = privilege.Id,
                Message = "Privilege created successfully",
                StatusCode = 201
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating privilege by user {UserId}", token.UserID);
            return new JsonModel
            {
                data = new object(),
                Message = "Error creating privilege",
                StatusCode = 500
            };
        }
    }

    // Update privilege
    public async Task<JsonModel> UpdatePrivilegeAsync(string id, UpdatePrivilegeDto updateDto, TokenModel token)
    {
        try
        {
            if (!Guid.TryParse(id, out var privilegeId))
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Invalid privilege ID format",
                    StatusCode = 400
                };
            }

            var privilege = await _privilegeRepo.GetByIdAsync(privilegeId);
            if (privilege == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Privilege not found",
                    StatusCode = 404
                };
            }

            privilege.Name = updateDto.Name;
            privilege.Description = updateDto.Description;
            privilege.PrivilegeTypeId = updateDto.PrivilegeTypeId;
            privilege.IsActive = updateDto.IsActive;
            privilege.UpdatedBy = token.UserID;
            privilege.UpdatedDate = DateTime.UtcNow;

            await _privilegeRepo.UpdateAsync(privilege);

            return new JsonModel
            {
                data = privilege.Id,
                Message = "Privilege updated successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating privilege {PrivilegeId} by user {UserId}", id, token.UserID);
            return new JsonModel
            {
                data = new object(),
                Message = "Error updating privilege",
                StatusCode = 500
            };
        }
    }

    // Delete privilege
    public async Task<JsonModel> DeletePrivilegeAsync(string id, TokenModel token)
    {
        try
        {
            if (!Guid.TryParse(id, out var privilegeId))
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Invalid privilege ID format",
                    StatusCode = 400
                };
            }

            var privilege = await _privilegeRepo.GetByIdAsync(privilegeId);
            if (privilege == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Privilege not found",
                    StatusCode = 404
                };
            }

            // Soft delete - set audit properties
            privilege.IsDeleted = true;
            privilege.DeletedBy = token.UserID;
            privilege.DeletedDate = DateTime.UtcNow;
            privilege.UpdatedBy = token.UserID;
            privilege.UpdatedDate = DateTime.UtcNow;
            
            await _privilegeRepo.UpdateAsync(privilege);

            return new JsonModel
            {
                data = privilegeId,
                Message = "Privilege deleted successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting privilege {PrivilegeId} by user {UserId}", id, token.UserID);
            return new JsonModel
            {
                data = new object(),
                Message = "Error deleting privilege",
                StatusCode = 500
            };
        }
    }

    // Get privilege categories
    public async Task<JsonModel> GetPrivilegeCategoriesAsync(TokenModel token)
    {
        try
        {
            // For now, return hardcoded categories. In a real implementation, 
            // you might want to store these in a separate table
            var categories = new[]
            {
                new { Name = "Medical", Description = "Medical-related privileges" },
                new { Name = "Communication", Description = "Communication privileges" },
                new { Name = "Administrative", Description = "Administrative privileges" },
                new { Name = "Technical", Description = "Technical privileges" },
                new { Name = "Financial", Description = "Financial privileges" }
            };

            return new JsonModel
            {
                data = categories,
                Message = "Privilege categories retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving privilege categories by user {UserId}", token.UserID);
            return new JsonModel
            {
                data = new object(),
                Message = "Error retrieving privilege categories",
                StatusCode = 500
            };
        }
    }

    // Get privilege types
    public async Task<JsonModel> GetPrivilegeTypesAsync(TokenModel token)
    {
        try
        {
            // This would typically come from the MasterPrivilegeTypes table
            // For now, return a basic structure
            var privilegeTypes = new[]
            {
                new { Id = Guid.NewGuid(), Name = "Basic", Description = "Basic privileges" },
                new { Id = Guid.NewGuid(), Name = "Premium", Description = "Premium privileges" },
                new { Id = Guid.NewGuid(), Name = "Enterprise", Description = "Enterprise privileges" }
            };

            return new JsonModel
            {
                data = privilegeTypes,
                Message = "Privilege types retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving privilege types by user {UserId}", token.UserID);
            return new JsonModel
            {
                data = new object(),
                Message = "Error retrieving privilege types",
                StatusCode = 500
            };
        }
    }

    // Export privileges
    public async Task<JsonModel> ExportPrivilegesAsync(string? search, string? category, string? status, string format, TokenModel token)
    {
        try
        {
            var privileges = await _privilegeRepo.GetAllAsync();
            
            // Apply filters
            if (!string.IsNullOrEmpty(search))
            {
                privileges = privileges.Where(p => p.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                                                 p.Description?.Contains(search, StringComparison.OrdinalIgnoreCase) == true);
            }

            if (!string.IsNullOrEmpty(category))
            {
                // Filter by category logic would go here
            }

            if (!string.IsNullOrEmpty(status))
            {
                if (bool.TryParse(status, out var isActive))
                {
                    privileges = privileges.Where(p => p.IsActive == isActive);
                }
            }

            var exportData = privileges.Select(p => new
            {
                p.Name,
                p.Description,
                p.IsActive,
                p.CreatedDate
            });

            return new JsonModel
            {
                data = exportData,
                Message = $"Privileges exported successfully in {format.ToUpper()} format",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting privileges by user {UserId}", token.UserID);
            return new JsonModel
            {
                data = new object(),
                Message = "Error exporting privileges",
                StatusCode = 500
            };
        }
    }

    // Get usage history
    public async Task<JsonModel> GetUsageHistoryAsync(int page, int pageSize, string? privilegeId, string? userId, string? subscriptionId, DateTime? startDate, DateTime? endDate, string? sortBy, string? sortOrder, TokenModel token)
    {
        try
        {
            _logger.LogInformation("Retrieving privilege usage history with database-level filtering - Page: {Page}, PageSize: {PageSize}, PrivilegeId: {PrivilegeId}, UserId: {UserId}, SubscriptionId: {SubscriptionId}", 
                page, pageSize, privilegeId, userId, subscriptionId);

            // Use database-level filtering, pagination, and sorting
            var (history, totalCount) = await _usageHistoryRepo.GetUsageHistoryWithFilteringAsync(
                page, pageSize, privilegeId, userId, subscriptionId, startDate, endDate, sortBy, sortOrder);

            var paginationMeta = new Meta
            {
                TotalRecords = totalCount,
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                HasNextPage = page < (int)Math.Ceiling((double)totalCount / pageSize),
                HasPreviousPage = page > 1
            };

            return new JsonModel
            {
                data = history,
                meta = paginationMeta,
                Message = "Usage history retrieved successfully with database-level filtering",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving usage history by user {UserId}", token.UserID);
            return new JsonModel
            {
                data = new object(),
                Message = "Error retrieving usage history",
                StatusCode = 500
            };
        }
    }

    // Get usage summary
    public async Task<JsonModel> GetUsageSummaryAsync(string? privilegeId, string? userId, string? subscriptionId, DateTime? startDate, DateTime? endDate, TokenModel token)
    {
        try
        {
            _logger.LogInformation("Retrieving privilege usage summary with database-level aggregation - PrivilegeId: {PrivilegeId}, UserId: {UserId}, SubscriptionId: {SubscriptionId}", 
                privilegeId, userId, subscriptionId);

            // Use database-level aggregation
            var summary = await _usageHistoryRepo.GetUsageSummaryAsync(
                privilegeId, userId, subscriptionId, startDate, endDate);

            return new JsonModel
            {
                data = summary,
                Message = "Usage summary retrieved successfully with database-level aggregation",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving usage summary by user {UserId}", token.UserID);
            return new JsonModel
            {
                data = new object(),
                Message = "Error retrieving usage summary",
                StatusCode = 500
            };
        }
    }

    // Export usage data
    public async Task<JsonModel> ExportUsageDataAsync(string format, string? privilegeId, string? userId, string? subscriptionId, DateTime? startDate, DateTime? endDate, TokenModel token)
    {
        try
        {
            // This would typically query and format data from PrivilegeUsageHistory table
            // For now, return a placeholder response
            var exportData = new[]
            {
                new { 
                    PrivilegeName = "Teleconsultation",
                    UserName = "John Doe",
                    UsedValue = 1,
                    UsedAt = DateTime.UtcNow.AddDays(-1),
                    UsageDate = DateTime.UtcNow.AddDays(-1).Date
                }
            };

            return new JsonModel
            {
                data = exportData,
                Message = $"Usage data exported successfully in {format.ToUpper()} format",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting usage data by user {UserId}", token.UserID);
            return new JsonModel
            {
                data = new object(),
                Message = "Error exporting usage data",
                StatusCode = 500
            };
        }
    }

    /// <summary>
    /// Updates time-based usage limits for a subscription plan privilege.
    /// This method allows administrators to configure daily, weekly, and monthly usage limits
    /// for specific privileges within subscription plans.
    /// </summary>
    /// <param name="updateDto">DTO containing time-based limit configuration details</param>
    /// <param name="token">Token model for authentication and authorization</param>
    /// <returns>JsonModel containing the update result</returns>
    public async Task<JsonModel> UpdateTimeBasedLimitsAsync(UpdateTimeBasedLimitsDto updateDto, TokenModel token)
    {
        try
        {
            // Validate admin access
            if (token.RoleID != (int)SmartTelehealth.Core.Enums.RoleId.Admin)
            {
                return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };
            }

            _logger.LogInformation("Updating time-based limits for privilege {PrivilegeId} by user {UserId}", 
                updateDto.PrivilegeId, token.UserID);

            // Find the plan privilege to update (repository returns IEnumerable, so get first)
            var planPrivileges = await _planPrivilegeRepo.GetByPrivilegeIdAsync(updateDto.PrivilegeId);
            var planPrivilege = planPrivileges.FirstOrDefault();
            if (planPrivilege == null)
            {
                return new JsonModel { data = new object(), Message = "Plan privilege not found", StatusCode = 404 };
            }

            // Update the time-based limits
            // Time-based limits removed
            // planPrivilege.UsagePeriodId = updateDto.UsagePeriodId; // REMOVED - not used
            planPrivilege.DurationMonths = updateDto.DurationMonths;
            planPrivilege.UpdatedBy = token.UserID;
            planPrivilege.UpdatedDate = DateTime.UtcNow;

            // Update in repository
            await _planPrivilegeRepo.UpdateAsync(planPrivilege);

            _logger.LogInformation("Successfully updated time-based limits for privilege {PrivilegeId}", updateDto.PrivilegeId);

            return new JsonModel
            {
                data = new
                {
                    PrivilegeId = updateDto.PrivilegeId,
                    // UsagePeriodId = updateDto.UsagePeriodId, // REMOVED - not used
                    DurationMonths = updateDto.DurationMonths,
                    UpdatedDate = DateTime.UtcNow
                },
                Message = "Time-based limits updated successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating time-based limits for privilege {PrivilegeId} by user {UserId}", 
                updateDto.PrivilegeId, token.UserID);
            return new JsonModel
            {
                data = new object(),
                Message = "Error updating time-based limits",
                StatusCode = 500
            };
        }
    }
    #endregion

    #region Privilege Availability Check with Purchase Information

    /// <summary>
    /// Checks if a privilege can be used and returns detailed information for purchasing if limit exceeded.
    /// This method implements the requirement:
    /// "Once a user has used all their included privileges, any additional usage 
    /// would require upfront payment."
    /// </summary>
    /// <param name="subscriptionId">The subscription ID to check</param>
    /// <param name="privilegeName">The name of the privilege to check</param>
    /// <param name="requestedAmount">The amount of privilege usage requested</param>
    /// <param name="tokenModel">Token containing user authentication information</param>
    /// <returns>JsonModel with availability status and purchase details if needed</returns>
    /// <remarks>
    /// Returns:
    /// - 200 OK if privilege is available (user can proceed)
    /// - 402 Payment Required if limit exceeded (includes purchase information)
    /// - 403 Forbidden if privilege is disabled
    /// - 404 Not Found if subscription or privilege not found
    /// </remarks>
    public async Task<JsonModel> CheckPrivilegeAvailabilityAsync(
        Guid subscriptionId,
        string privilegeName,
        int requestedAmount,
        TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation(
                "Checking privilege '{PrivilegeName}' availability for subscription {SubscriptionId}, requested amount: {Amount}",
                privilegeName, subscriptionId, requestedAmount
            );

            // Validate input
            if (requestedAmount <= 0)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Requested amount must be greater than zero",
                    StatusCode = 400
                };
            }

            // Get the plan privilege configuration
            var planPrivilege = await GetPlanPrivilegeAsync(subscriptionId, privilegeName);
            
            if (planPrivilege == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = $"Privilege '{privilegeName}' not found in subscription plan",
                    StatusCode = 404
                };
            }

            // Check if privilege is disabled
            if (planPrivilege.Value == 0)
            {
                return new JsonModel
                {
                    data = new
                    {
                        available = false,
                        disabled = true,
                        message = $"Privilege '{privilegeName}' is not included in your subscription plan"
                    },
                    Message = $"Privilege '{privilegeName}' is not available in your plan",
                    StatusCode = 403
                };
            }

            // Handle unlimited privileges
            if (planPrivilege.Value == -1)
            {
                return new JsonModel
                {
                    data = new
                    {
                        available = true,
                        unlimited = true,
                        privilegeName = privilegeName,
                        message = "You have unlimited access to this privilege"
                    },
                    Message = "Privilege is available (unlimited)",
                    StatusCode = 200
                };
            }

            // Time-based limits removed - only check total AllowedValue below

            // Get remaining privilege amount
            var remaining = await GetRemainingPrivilegeAsync(subscriptionId, privilegeName, tokenModel);

            // Check if user has enough remaining credits
            if (remaining >= requestedAmount)
            {
                // User has sufficient credits - allow usage
                return new JsonModel
                {
                    data = new
                    {
                        available = true,
                        privilegeName = privilegeName,
                        remaining = remaining,
                        requested = requestedAmount,
                        afterUse = remaining - requestedAmount,
                        message = "Privilege is available"
                    },
                    Message = "Privilege is available",
                    StatusCode = 200
                };
            }

            // LIMIT EXCEEDED - Return 402 Payment Required with purchase details
            var shortfall = requestedAmount - remaining;
            var requiredPayment = shortfall * planPrivilege.UnitCost;

            _logger.LogWarning(
                "Privilege '{PrivilegeName}' limit exceeded for subscription {SubscriptionId}. " +
                "Remaining: {Remaining}, Requested: {Requested}, Shortfall: {Shortfall}, Cost: ${Cost}",
                privilegeName, subscriptionId, remaining, requestedAmount, shortfall, requiredPayment
            );

            return new JsonModel
            {
                data = new
                {
                    available = false,
                    limitExceeded = true,
                    privilegeName = privilegeName,
                    remaining = remaining,
                    requested = requestedAmount,
                    shortfall = shortfall,
                    unitCost = planPrivilege.UnitCost,
                    requiredPayment = requiredPayment,
                    message = $"You've used all your included {privilegeName} credits. Purchase {shortfall} additional credit{(shortfall > 1 ? "s" : "")} for ${requiredPayment:F2} to continue.",
                    purchaseEndpoint = $"/api/subscriptions/{subscriptionId}/purchase-credits",
                    purchaseDetails = new
                    {
                        privilegeName = privilegeName,
                        quantity = shortfall,
                        unitCost = planPrivilege.UnitCost,
                        totalCost = requiredPayment
                    }
                },
                Message = $"Insufficient {privilegeName} credits. {remaining} remaining, {requestedAmount} requested. Purchase {shortfall} additional credit{(shortfall > 1 ? "s" : "")} for ${requiredPayment:F2}.",
                StatusCode = 402 // Payment Required
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error checking privilege availability for '{PrivilegeName}' in subscription {SubscriptionId}",
                privilegeName, subscriptionId
            );

            return new JsonModel
            {
                data = new object(),
                Message = "Error checking privilege availability",
                StatusCode = 500
            };
        }
    }

    #endregion
    
    #region Privilege Allocation (Corrected Implementation)
    
    /// <summary>
    /// Gets privilege allocation using admin-set Value directly (no calculation).
    /// CORRECTED: The Value field contains the total privilege count set by admin.
    /// Monthly/Weekly/Daily limits are optional rate limiters checked separately.
    /// </summary>
    private async Task<(int allowedValue, DateTime periodStart, DateTime periodEnd)> CalculatePrivilegeAllocationAsync(
        Guid subscriptionId, 
        SubscriptionPlanPrivilege planPrivilege)
    {
        var subscription = await _subscriptionRepo.GetByIdWithDetailsAsync(subscriptionId);
        if (subscription == null)
            throw new InvalidOperationException($"Subscription {subscriptionId} not found");
        
        // CORRECTED: Use centralized calculator (now returns Value directly without calculation)
        var (allowedValue, periodStart, periodEnd) = PrivilegeAllocationCalculator.CalculatePrivilegeAllocation(
            subscription, 
            planPrivilege);
        
        _logger.LogDebug("Retrieved privilege allocation for subscription {SubscriptionId}: " +
            "AllowedValue={AllowedValue} (admin-set total), Period={Start:yyyy-MM-dd} to {End:yyyy-MM-dd}",
            subscriptionId, allowedValue, periodStart, periodEnd);
        
        return (allowedValue, periodStart, periodEnd);
    }
    
    #endregion

    #region Privilege Initialization

    /// <summary>
    /// Initializes privileges for a new subscription by creating privilege usage records
    /// for all privileges in the subscription plan
    /// </summary>
    public async Task<JsonModel> InitializeSubscriptionPrivilegesAsync(
        Guid subscriptionId, 
        Guid planId, 
        TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("🎁 Initializing privileges for subscription {SubscriptionId}", subscriptionId);
            
            // 1. Get plan with privileges
            var plan = await _planPrivilegeRepo.GetByPlanIdAsync(planId);
            if (plan == null || !plan.Any())
            {
                _logger.LogWarning("⚠️ No privileges found for plan {PlanId}", planId);
                return new JsonModel { StatusCode = 200, Message = "No privileges to initialize" };
            }
            
            _logger.LogInformation("📦 Found {Count} privileges for plan {PlanId}", 
                plan.Count(), planId);
            
            // 2. Check if already initialized (idempotency)
            var existing = await _usageRepo.GetBySubscriptionIdAsync(subscriptionId);
            if (existing != null && existing.Any())
            {
                _logger.LogInformation("ℹ️ Privileges already initialized for subscription {SubscriptionId} (count: {Count})", 
                    subscriptionId, existing.Count());
                return new JsonModel { StatusCode = 200, Message = "Privileges already initialized" };
            }
            
            // 3. Create privilege usage records
            var usageRecords = new List<UserSubscriptionPrivilegeUsage>();
            
            foreach (var pp in plan)
            {
                var privilege = await _privilegeRepo.GetByIdAsync(pp.PrivilegeId);
                if (privilege == null)
                {
                    _logger.LogWarning("⚠️ Privilege {PrivilegeId} not found, skipping", pp.PrivilegeId);
                    continue;
                }
                
                var usageRecord = new UserSubscriptionPrivilegeUsage
                {
                    Id = Guid.NewGuid(),
                    SubscriptionId = subscriptionId,
                    SubscriptionPlanPrivilegeId = pp.Id,
                    PrivilegeId = pp.PrivilegeId,
                    UsedValue = 0,
                    AllowedValue = pp.Value, // Value from SubscriptionPlanPrivilege
                    UsagePeriodStart = DateTime.UtcNow,
                    UsagePeriodEnd = DateTime.UtcNow.AddMonths(1), // Will be updated based on billing cycle
                    ResetAt = null,
                    CreatedBy = tokenModel.UserID,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedBy = tokenModel.UserID,
                    UpdatedDate = DateTime.UtcNow
                };
                
                usageRecords.Add(usageRecord);
                
                _logger.LogInformation("  ✅ Prepared privilege: {PrivilegeName} (Limit: {Limit})", 
                    privilege.Name, pp.Value);
            }
            
            // 4. Save all records
            foreach (var record in usageRecords)
            {
                await _usageRepo.CreateAsync(record);
            }
            await _usageRepo.SaveChangesAsync();
            
            _logger.LogInformation("🎉 Successfully initialized {Count} privileges for subscription {SubscriptionId}", 
                usageRecords.Count, subscriptionId);
            
            return new JsonModel 
            { 
                data = usageRecords.Count, 
                StatusCode = 201, 
                Message = $"Initialized {usageRecords.Count} privileges successfully" 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error initializing privileges for subscription {SubscriptionId}", subscriptionId);
            return new JsonModel { StatusCode = 500, Message = $"Failed to initialize privileges: {ex.Message}" };
        }
    }

    #endregion
} 