using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly ApplicationDbContext _context;

        public AuditLogRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AuditLog> CreateAsync(AuditLog auditLog)
        {
            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
            return auditLog;
        }

        public async Task<IEnumerable<AuditLog>> GetByUserIdAsync(int userId)
        {
            return await _context.AuditLogs
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetByEntityTypeAsync(string entityType)
        {
            return await _context.AuditLogs
                .Where(a => a.EntityType == entityType)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.AuditLogs
                .Where(a => a.Timestamp >= startDate && a.Timestamp <= endDate)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetByActionAsync(string action)
        {
            return await _context.AuditLogs
                .Where(a => a.Action == action)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetByStatusAsync(string status)
        {
            return await _context.AuditLogs
                .Where(a => a.Status == status)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<int> GetCountByUserIdAsync(int userId)
        {
            return await _context.AuditLogs
                .CountAsync(a => a.UserId == userId);
        }

        public async Task<IEnumerable<AuditLog>> GetRecentAsync(int count = 100)
        {
            return await _context.AuditLogs
                .OrderByDescending(a => a.Timestamp)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> SearchAsync(string searchTerm)
        {
            return await _context.AuditLogs
                .Where(a => a.Action.Contains(searchTerm) || 
                           a.EntityType.Contains(searchTerm) || 
                           a.Description.Contains(searchTerm) ||
                           a.UserEmail.Contains(searchTerm))
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<AuditLog?> GetByIdAsync(Guid id)
        {
            return await _context.AuditLogs.FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<AuditLog>> GetWithFiltersAsync(string? action, int? userId, DateTime? startDate, DateTime? endDate, int page, int pageSize)
        {
            var query = _context.AuditLogs.AsQueryable();

            if (!string.IsNullOrEmpty(action))
                query = query.Where(a => a.Action == action);

            if (userId.HasValue)
                query = query.Where(a => a.UserId == userId.Value);

            if (startDate.HasValue)
                query = query.Where(a => a.Timestamp >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(a => a.Timestamp <= endDate.Value);

            return await query
                .OrderByDescending(a => a.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
} 