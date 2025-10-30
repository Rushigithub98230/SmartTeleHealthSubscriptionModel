using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories
{
    public class AuditLogRepository : RepositoryBase<AuditLog>, IAuditLogRepository
    {
        private readonly ApplicationDbContext _context;

        public AuditLogRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public override async Task<AuditLog> CreateAsync(AuditLog auditLog)
        {
            return await base.CreateAsync(auditLog);
        }

        public override async Task<AuditLog?> GetByIdAsync(object id)
        {
            if (id is not int auditId)
                return null;

            return await _context.AuditLogs.FirstOrDefaultAsync(a => a.Id == auditId);
        }

        public override async Task<IEnumerable<AuditLog>> GetAllAsync()
        {
            return await _context.AuditLogs
                .OrderByDescending(a => a.DateTime)
                .ToListAsync();
        }

        public override async Task<AuditLog> UpdateAsync(AuditLog auditLog)
        {
            return await base.UpdateAsync(auditLog);
        }

        public override async Task<bool> DeleteAsync(object id)
        {
            if (id is not int auditId)
                return false;

            var auditLog = await _context.AuditLogs.FindAsync(auditId);
            if (auditLog == null) return false;

            _context.AuditLogs.Remove(auditLog);
            await _context.SaveChangesAsync();
            return true;
        }

        public override async Task<bool> ExistsAsync(object id)
        {
            if (id is not int auditId)
                return false;

            return await _context.AuditLogs.AnyAsync(a => a.Id == auditId);
        }

        public async Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.AuditLogs
                .Where(a => a.DateTime >= startDate && a.DateTime <= endDate)
                .OrderByDescending(a => a.DateTime)
                .ToListAsync();
        }

        public async Task<(List<AuditLog> logs, int totalCount)> GetAuditLogsAsync(
            DateTime? startDate,
            DateTime? endDate,
            List<string>? types,
            List<string>? tableNames,
            int? userId,
            string? searchText,
            int page,
            int pageSize)
        {
            var query = _context.AuditLogs.AsQueryable();

            // Apply date range filter
            if (startDate.HasValue)
                query = query.Where(log => log.DateTime >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(log => log.DateTime <= endDate.Value);

            // Apply type filter
            if (types != null && types.Any())
                query = query.Where(log => types.Contains(log.Type));

            // Apply table name filter
            if (tableNames != null && tableNames.Any())
                query = query.Where(log => tableNames.Contains(log.TableName));

            // Apply user filter
            if (userId.HasValue)
                query = query.Where(log => log.UserId == userId.Value);

            // Apply search text filter (searches in OldValues, NewValues, TableName, and PrimaryKey)
            if (!string.IsNullOrEmpty(searchText))
            {
                query = query.Where(log =>
                    (log.OldValues != null && log.OldValues.Contains(searchText)) ||
                    (log.NewValues != null && log.NewValues.Contains(searchText)) ||
                    (log.TableName != null && log.TableName.Contains(searchText)) ||
                    (log.PrimaryKey != null && log.PrimaryKey.Contains(searchText)));
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply pagination and ordering
            var logs = await query
                .OrderByDescending(log => log.DateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (logs, totalCount);
        }

        public async Task<List<string>> GetAvailableTablesAsync()
        {
            return await _context.AuditLogs
                .Select(a => a.TableName)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();
        }

        public async Task<List<string>> GetAvailableTypesAsync()
        {
            return await _context.AuditLogs
                .Select(a => a.Type)
                .Distinct()
                .OrderBy(t => t)
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