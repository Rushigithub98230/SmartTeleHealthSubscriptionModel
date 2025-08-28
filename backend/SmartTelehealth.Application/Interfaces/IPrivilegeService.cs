using SmartTelehealth.Core.Entities;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IPrivilegeService
{
    // Existing methods
    Task<int> GetRemainingPrivilegeAsync(Guid subscriptionId, string privilegeName, TokenModel tokenModel);
    Task<bool> UsePrivilegeAsync(Guid subscriptionId, string privilegeName, int amount = 1, TokenModel tokenModel = null);
    Task<IEnumerable<Privilege>> GetPrivilegesForPlanAsync(Guid planId, TokenModel tokenModel);
    Task<JsonModel> GetAllPrivilegesAsync(int page, int pageSize, string? search, string? category, string? status, TokenModel tokenModel);

    // New methods for enhanced privilege management
    Task<JsonModel> GetPrivilegeByIdAsync(string id, TokenModel token);
    Task<JsonModel> CreatePrivilegeAsync(CreatePrivilegeDto createDto, TokenModel token);
    Task<JsonModel> UpdatePrivilegeAsync(string id, UpdatePrivilegeDto updateDto, TokenModel token);
    Task<JsonModel> DeletePrivilegeAsync(string id, TokenModel token);
    Task<JsonModel> GetPrivilegeCategoriesAsync(TokenModel token);
    Task<JsonModel> GetPrivilegeTypesAsync(TokenModel token);
    Task<JsonModel> ExportPrivilegesAsync(string? search, string? category, string? status, string format, TokenModel token);
    Task<JsonModel> GetUsageHistoryAsync(int page, int pageSize, string? privilegeId, string? userId, string? subscriptionId, DateTime? startDate, DateTime? endDate, string? sortBy, string? sortOrder, TokenModel token);
    Task<JsonModel> GetUsageSummaryAsync(string? privilegeId, string? userId, string? subscriptionId, DateTime? startDate, DateTime? endDate, TokenModel token);
    Task<JsonModel> ExportUsageDataAsync(string format, string? privilegeId, string? userId, string? subscriptionId, DateTime? startDate, DateTime? endDate, TokenModel token);
}
