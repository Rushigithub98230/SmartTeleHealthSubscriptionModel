using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces
{
    public interface IAuditLogRepository
    {
        Task<AuditLog> CreateAsync(AuditLog auditLog);
        Task<IEnumerable<AuditLog>> GetByUserIdAsync(int userId);
        Task<IEnumerable<AuditLog>> GetByEntityTypeAsync(string entityType);
        Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<AuditLog>> GetByActionAsync(string action);
        Task<IEnumerable<AuditLog>> GetByStatusAsync(string status);
        Task<int> GetCountByUserIdAsync(int userId);
        Task<IEnumerable<AuditLog>> GetRecentAsync(int count = 100);
        Task<IEnumerable<AuditLog>> SearchAsync(string searchTerm);
        Task<IEnumerable<AuditLog>> GetWithFiltersAsync(string? action, int? userId, DateTime? startDate, DateTime? endDate, int page, int pageSize);
        Task<AuditLog?> GetByIdAsync(Guid id);
    }
} 