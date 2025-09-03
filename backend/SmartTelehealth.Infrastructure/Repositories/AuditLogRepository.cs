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

        public async Task<AuditLog?> GetByIdAsync(int id)
        {
            return await _context.AuditLogs.FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.AuditLogs
                .Where(a => a.DateTime >= startDate && a.DateTime <= endDate)
                .OrderByDescending(a => a.DateTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetDatabaseAuditTrailAsync(string tableName, string? entityId = null)
        {
            var query = _context.AuditLogs
                .Where(a => a.TableName == tableName);

            if (!string.IsNullOrEmpty(entityId))
            {
                query = query.Where(a => a.PrimaryKey.Contains(entityId));
            }

            return await query
                .OrderByDescending(a => a.DateTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetUserDatabaseAuditTrailAsync(int userId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.AuditLogs
                .Where(a => a.UserId == userId);

            if (fromDate.HasValue)
                query = query.Where(a => a.DateTime >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(a => a.DateTime <= toDate.Value);

            return await query
                .OrderByDescending(a => a.DateTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetEntityChangeHistoryAsync(string tableName, string entityId)
        {
            return await _context.AuditLogs
                .Where(a => a.TableName == tableName && 
                           a.PrimaryKey.Contains(entityId))
                .OrderByDescending(a => a.DateTime)
                .ToListAsync();
        }

        public async Task<object> GetAuditStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.AuditLogs.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(a => a.DateTime >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(a => a.DateTime <= toDate.Value);

            var statistics = new
            {
                TotalChanges = await query.CountAsync(),
                CreateCount = await query.CountAsync(a => a.Type == "Create"),
                UpdateCount = await query.CountAsync(a => a.Type == "Update"),
                DeleteCount = await query.CountAsync(a => a.Type == "Delete"),
                MostChangedTables = await query
                    .GroupBy(a => a.TableName)
                    .Select(g => new { TableName = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(10)
                    .ToListAsync(),
                MostActiveUsers = await query
                    .Where(a => a.UserId.HasValue)
                    .GroupBy(a => a.UserId)
                    .Select(g => new { UserId = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(10)
                    .ToListAsync()
            };

            return statistics;
        }

        public async Task<IEnumerable<AuditLog>> GetRecentDatabaseChangesAsync(int count = 50)
        {
            return await _context.AuditLogs
                .OrderByDescending(a => a.DateTime)
                .Take(count)
                .ToListAsync();
        }
    }
}