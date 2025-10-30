using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces
{
public interface IAuditLogRepository : IRepositoryBase<AuditLog>
{
    // Basic CRUD methods are inherited from IRepositoryBase<AuditLog>
        Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        
        // Comprehensive filtering method with pagination
        Task<(List<AuditLog> logs, int totalCount)> GetAuditLogsAsync(
            DateTime? startDate,
            DateTime? endDate,
            List<string>? types,
            List<string>? tableNames,
            int? userId,
            string? searchText,
            int page,
            int pageSize);
        
        // Helper methods for filter options
        Task<List<string>> GetAvailableTablesAsync();
        Task<List<string>> GetAvailableTypesAsync();
        
        // Database Audit Querying Methods
        Task<IEnumerable<AuditLog>> GetDatabaseAuditTrailAsync(string tableName, string? entityId = null);
        Task<IEnumerable<AuditLog>> GetUserDatabaseAuditTrailAsync(int userId, DateTime? fromDate = null, DateTime? toDate = null);
        Task<IEnumerable<AuditLog>> GetEntityChangeHistoryAsync(string tableName, string entityId);
        Task<object> GetAuditStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<IEnumerable<AuditLog>> GetRecentDatabaseChangesAsync(int count = 50);
    }
}