using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces
{
    public interface IAuditLogRepository
    {
        Task<AuditLog> CreateAsync(AuditLog auditLog);
        Task<AuditLog?> GetByIdAsync(int id);
        Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        
        // Database Audit Querying Methods
        Task<IEnumerable<AuditLog>> GetDatabaseAuditTrailAsync(string tableName, string? entityId = null);
        Task<IEnumerable<AuditLog>> GetUserDatabaseAuditTrailAsync(int userId, DateTime? fromDate = null, DateTime? toDate = null);
        Task<IEnumerable<AuditLog>> GetEntityChangeHistoryAsync(string tableName, string entityId);
        Task<object> GetAuditStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<IEnumerable<AuditLog>> GetRecentDatabaseChangesAsync(int count = 50);
    }
}