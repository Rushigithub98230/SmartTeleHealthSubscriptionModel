using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IAuditRepository
{
    Task<AuditLog> GetByIdAsync(Guid id);
    Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<AuditLog>> GetByEntityTypeAsync(string entityType);
    Task<IEnumerable<AuditLog>> GetByEntityIdAsync(string entityId);
    Task<AuditLog> CreateAsync(AuditLog auditLog);
    Task<AuditLog> UpdateAsync(AuditLog auditLog);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<AuditLog>> GetAllAsync();
    Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
} 