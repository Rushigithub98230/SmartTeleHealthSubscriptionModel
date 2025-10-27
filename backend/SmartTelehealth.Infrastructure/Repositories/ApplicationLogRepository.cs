using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for ApplicationLog entity with specialized query methods.
/// </summary>
public class ApplicationLogRepository : GenericRepository<ApplicationLog>, IApplicationLogRepository
{
    public ApplicationLogRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<(List<ApplicationLog> logs, int totalCount)> GetLogsAsync(
        DateTime? startDate,
        DateTime? endDate,
        string? logLevel,
        string? source,
        int? userId,
        string? searchText,
        int page,
        int pageSize)
    {
        var query = _context.ApplicationLogs.AsQueryable();

        // Apply filters
        if (startDate.HasValue)
            query = query.Where(log => log.Timestamp >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(log => log.Timestamp <= endDate.Value);

        if (!string.IsNullOrEmpty(logLevel))
            query = query.Where(log => log.LogLevel == logLevel);

        if (!string.IsNullOrEmpty(source))
            query = query.Where(log => log.Source.Contains(source));

        if (userId.HasValue)
            query = query.Where(log => log.UserId == userId.Value);

        if (!string.IsNullOrEmpty(searchText))
            query = query.Where(log => log.Message.Contains(searchText));

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply pagination and ordering
        var logs = await query
            .OrderByDescending(log => log.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(log => log.User)
            .ToListAsync();

        return (logs, totalCount);
    }

    public async Task<List<ApplicationLog>> GetRecentLogsAsync(int count)
    {
        return await _context.ApplicationLogs
            .OrderByDescending(log => log.Timestamp)
            .Take(count)
            .Include(log => log.User)
            .ToListAsync();
    }

    public async Task<ApplicationLog?> GetLogByIdAsync(long id)
    {
        return await _context.ApplicationLogs
            .Include(log => log.User)
            .FirstOrDefaultAsync(log => log.Id == id);
    }
}

