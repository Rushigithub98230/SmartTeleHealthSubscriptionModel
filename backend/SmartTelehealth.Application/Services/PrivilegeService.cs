using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;

namespace SmartTelehealth.Application.Services;

public class PrivilegeService : IPrivilegeService
{
    private readonly IPrivilegeRepository _privilegeRepo;
    private readonly ISubscriptionPlanPrivilegeRepository _planPrivilegeRepo;
    private readonly IUserSubscriptionPrivilegeUsageRepository _usageRepo;
    private readonly IPrivilegeUsageHistoryRepository _usageHistoryRepo;
    private readonly ISubscriptionRepository _subscriptionRepo;
    private readonly ILogger<PrivilegeService> _logger;

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

    // Helper to get SubscriptionPlanPrivilege by subscription and privilege name
    private async Task<SubscriptionPlanPrivilege?> GetPlanPrivilegeAsync(Guid subscriptionId, string privilegeName)
    {
        // Fetch the subscription to get the planId
        var subscription = await _subscriptionRepo.GetByIdAsync(subscriptionId);
        if (subscription == null) return null;
        
        // Check if subscription is active
        if (!subscription.IsActive || subscription.IsDeleted || 
            subscription.Status != "Active" && subscription.Status != "Trial")
        {
            return null;
        }
        
        var planPrivileges = await _planPrivilegeRepo.GetByPlanIdAsync(subscription.SubscriptionPlanId);
        return planPrivileges.FirstOrDefault(pp => pp.Privilege.Name == privilegeName);
    }

    // Check if a user has a privilege and how much is left
    public async Task<int> GetRemainingPrivilegeAsync(Guid subscriptionId, string privilegeName, TokenModel tokenModel)
    {
        try
        {
            var planPrivilege = await GetPlanPrivilegeAsync(subscriptionId, privilegeName);
            if (planPrivilege == null) return 0;
            
            // Check if privilege is disabled
            if (planPrivilege.Value == 0) return 0;
            
            // Check if privilege is unlimited
            if (planPrivilege.Value == -1) return int.MaxValue;
            
            var usage = (await _usageRepo.GetBySubscriptionIdAsync(subscriptionId))
                .FirstOrDefault(u => u.SubscriptionPlanPrivilegeId == planPrivilege.Id);
            var used = usage?.UsedValue ?? 0;
            var remaining = Math.Max(0, planPrivilege.Value - used);
            
            _logger.LogInformation("Remaining privilege '{PrivilegeName}' for subscription {SubscriptionId} by user {UserId}: {Remaining}", 
                privilegeName, subscriptionId, tokenModel.UserID, remaining);
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
    private async Task<bool> CheckTimeBasedLimitsAsync(Guid subscriptionId, SubscriptionPlanPrivilege planPrivilege, int amount)
    {
        try
        {
            var now = DateTime.UtcNow;
            var today = now.Date;
            var weekStart = today.AddDays(-(int)today.DayOfWeek);
            var monthStart = new DateTime(today.Year, today.Month, 1);

            // Check daily limit
            if (planPrivilege.DailyLimit.HasValue)
            {
                var dailyUsage = await _usageHistoryRepo.GetDailyUsageAsync(subscriptionId, planPrivilege.Id, today);
                if (dailyUsage + amount > planPrivilege.DailyLimit.Value)
                {
                    _logger.LogWarning("Daily limit exceeded for privilege {PrivilegeId} on {Date}. Used: {Used}, Limit: {Limit}, Requested: {Requested}", 
                        planPrivilege.Id, today, dailyUsage, planPrivilege.DailyLimit.Value, amount);
                    return false;
                }
            }

            // Check weekly limit
            if (planPrivilege.WeeklyLimit.HasValue)
            {
                var weeklyUsage = await _usageHistoryRepo.GetWeeklyUsageAsync(subscriptionId, planPrivilege.Id, weekStart);
                if (weeklyUsage + amount > planPrivilege.WeeklyLimit.Value)
                {
                    _logger.LogWarning("Weekly limit exceeded for privilege {PrivilegeId} for week starting {WeekStart}. Used: {Used}, Limit: {Limit}, Requested: {Requested}", 
                        planPrivilege.Id, weekStart, weeklyUsage, planPrivilege.WeeklyLimit.Value, amount);
                    return false;
                }
            }

            // Check monthly limit
            if (planPrivilege.MonthlyLimit.HasValue)
            {
                var monthlyUsage = await _usageHistoryRepo.GetMonthlyUsageAsync(subscriptionId, planPrivilege.Id, monthStart);
                if (monthlyUsage + amount > planPrivilege.MonthlyLimit.Value)
                {
                    _logger.LogWarning("Monthly limit exceeded for privilege {PrivilegeId} for month starting {MonthStart}. Used: {Used}, Limit: {Limit}, Requested: {Requested}", 
                        planPrivilege.Id, monthStart, monthlyUsage, planPrivilege.MonthlyLimit.Value, amount);
                    return false;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking time-based limits for privilege {PrivilegeId} in subscription {SubscriptionId}", 
                planPrivilege.Id, subscriptionId);
            return false;
        }
    }

    // Use a privilege (e.g., book a consult)
    public async Task<bool> UsePrivilegeAsync(Guid subscriptionId, string privilegeName, int amount, TokenModel tokenModel)
    {
        try
        {
            // Validate input parameters
            if (amount <= 0) return false;
            
            var planPrivilege = await GetPlanPrivilegeAsync(subscriptionId, privilegeName);
            if (planPrivilege == null) return false;
            
            // Check if privilege is disabled
            if (planPrivilege.Value == 0) return false;
            
            // Check time-based limits first
            if (!await CheckTimeBasedLimitsAsync(subscriptionId, planPrivilege, amount))
            {
                return false;
            }
            
            // Check if privilege is unlimited
            if (planPrivilege.Value == -1)
            {
                // For unlimited privileges, we can always use them
                var unlimitedUsage = (await _usageRepo.GetBySubscriptionIdAsync(subscriptionId))
                    .FirstOrDefault(u => u.SubscriptionPlanPrivilegeId == planPrivilege.Id);
                if (unlimitedUsage == null)
                {
                    unlimitedUsage = new UserSubscriptionPrivilegeUsage
                    {
                        SubscriptionId = subscriptionId,
                        SubscriptionPlanPrivilegeId = planPrivilege.Id,
                        UsedValue = amount,
                        AllowedValue = -1,
                        UsagePeriodStart = DateTime.UtcNow,
                        UsagePeriodEnd = DateTime.UtcNow.AddMonths(1),
                        LastUsedAt = DateTime.UtcNow
                    };
                    await _usageRepo.AddAsync(unlimitedUsage);
                }
                else
                {
                    unlimitedUsage.UsedValue += amount;
                    unlimitedUsage.LastUsedAt = DateTime.UtcNow;
                    await _usageRepo.UpdateAsync(unlimitedUsage);
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
                limitedUsage = new UserSubscriptionPrivilegeUsage
                {
                    SubscriptionId = subscriptionId,
                    SubscriptionPlanPrivilegeId = planPrivilege.Id,
                    UsedValue = amount,
                    AllowedValue = planPrivilege.Value,
                    UsagePeriodStart = DateTime.UtcNow,
                    UsagePeriodEnd = DateTime.UtcNow.AddMonths(1),
                    LastUsedAt = DateTime.UtcNow
                };
                await _usageRepo.AddAsync(limitedUsage);
            }
            else
            {
                limitedUsage.UsedValue += amount;
                limitedUsage.LastUsedAt = DateTime.UtcNow;
                await _usageRepo.UpdateAsync(limitedUsage);
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
    public async Task<IEnumerable<Privilege>> GetPrivilegesForPlanAsync(Guid planId, TokenModel tokenModel)
    {
        try
        {
            var planPrivileges = await _planPrivilegeRepo.GetByPlanIdAsync(planId);
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

    // Get all privileges with pagination and filtering
    public async Task<JsonModel> GetAllPrivilegesAsync(int page, int pageSize, string? search, string? category, string? status, TokenModel tokenModel)
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

            // Apply pagination
            var totalCount = privileges.Count();
            var pagedPrivileges = privileges
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var result = new
            {
                Privileges = pagedPrivileges,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };
            
            _logger.LogInformation("Privileges retrieved by user {UserId}: {PrivilegeCount} privileges (page {Page} of {TotalPages})", 
                tokenModel.UserID, pagedPrivileges.Count, page, result.TotalPages);
            return new JsonModel 
            { 
                data = result, 
                Message = "Privileges retrieved successfully", 
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
    public async Task<JsonModel> GetPrivilegeByIdAsync(string id, TokenModel token)
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
    public async Task<JsonModel> CreatePrivilegeAsync(CreatePrivilegeDto createDto, TokenModel token)
    {
        try
        {
            var privilege = new Privilege
            {
                Name = createDto.Name,
                Description = createDto.Description,
                PrivilegeTypeId = createDto.PrivilegeTypeId,
                IsActive = createDto.IsActive,
                CreatedBy = token.UserID,
                CreatedDate = DateTime.UtcNow
            };

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

            await _privilegeRepo.DeleteAsync(privilegeId);

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
            // This would typically query the PrivilegeUsageHistory table
            // For now, return a placeholder response
            var usageHistory = new[]
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
                data = usageHistory,
                Message = "Usage history retrieved successfully",
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
            // This would typically aggregate data from PrivilegeUsageHistory table
            // For now, return a placeholder response
            var summary = new
            {
                TotalUsage = 150,
                AverageDailyUsage = 5,
                MostUsedPrivilege = "Teleconsultation",
                UsageTrend = "Increasing"
            };

            return new JsonModel
            {
                data = summary,
                Message = "Usage summary retrieved successfully",
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
} 