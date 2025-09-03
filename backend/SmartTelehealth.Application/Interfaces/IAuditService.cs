using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces
{
    public interface IAuditService
    {
        // Core audit log retrieval
        Task<JsonModel> GetAuditLogByIdAsync(int id, TokenModel tokenModel);
        
        // Database audit querying methods
        Task<JsonModel> GetDatabaseAuditTrailAsync(string tableName, string? entityId = null, TokenModel tokenModel = null);
        Task<JsonModel> GetUserDatabaseAuditTrailAsync(int userId, DateTime? fromDate = null, DateTime? toDate = null, TokenModel tokenModel = null);
        Task<JsonModel> GetEntityChangeHistoryAsync(string tableName, string entityId, TokenModel tokenModel = null);
        Task<JsonModel> GetAuditStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null, TokenModel tokenModel = null);
        Task<JsonModel> GetRecentDatabaseChangesAsync(int count = 50, TokenModel tokenModel = null);
        Task<JsonModel> GetAuditLogsByDateRangeAsync(DateTime startDate, DateTime endDate, TokenModel tokenModel = null);
    }
}