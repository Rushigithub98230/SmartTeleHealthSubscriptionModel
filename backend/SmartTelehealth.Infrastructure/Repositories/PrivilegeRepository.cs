using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories;

public class PrivilegeRepository : RepositoryBase<Privilege>, IPrivilegeRepository
{
    private readonly ApplicationDbContext _context;
    public PrivilegeRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    // Use base class methods for basic CRUD operations
    // GetByIdAsync, GetAllAsync, CreateAsync, UpdateAsync, DeleteAsync are inherited from RepositoryBase

    // Keep only specialized methods that add value
    public async Task<Privilege?> GetByNameAsync(string name)
    {
        return await _context.Privileges
            .FirstOrDefaultAsync(p => p.Name.ToLower() == name.ToLower());
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await _context.Privileges
            .AnyAsync(p => p.Name.ToLower() == name.ToLower());
    }

    public async Task<IEnumerable<Privilege>> GetByIdsAsync(IEnumerable<Guid> privilegeIds)
    {
        var ids = privilegeIds.ToList();
        return await _context.Privileges
            .Where(p => ids.Contains(p.Id))
            .ToListAsync();
    }

    // Legacy method for backward compatibility - delegates to base CreateAsync
    public async Task AddAsync(Privilege privilege)
    {
        await CreateAsync(privilege);
    }

    /// <summary>
    /// Retrieves privileges with database-level filtering, pagination, and sorting
    /// </summary>
    public async Task<(IEnumerable<Privilege> Privileges, int TotalCount)> GetPrivilegesWithFilteringAsync(
        int page, int pageSize, string? search, string? category, string? status, string? sortBy = "Name", string? sortOrder = "asc")
    {
        var query = _context.Privileges
            .Include(p => p.PrivilegeType)
            .AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(p => 
                p.Name.ToLower().Contains(term) || 
                (p.Description != null && p.Description.ToLower().Contains(term)));
        }

        // Apply category filter (if PrivilegeType is considered as category)
        if (!string.IsNullOrWhiteSpace(category))
        {
            var categoryTerm = category.ToLower();
            query = query.Where(p => p.PrivilegeType != null && p.PrivilegeType.Name.ToLower().Contains(categoryTerm));
        }

        // Apply status filter
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (bool.TryParse(status, out var isActive))
            {
                query = query.Where(p => p.IsActive == isActive);
            }
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply sorting
        query = ApplySorting(query, sortBy, sortOrder);

        // Apply pagination
        var skip = (page - 1) * pageSize;
        var privileges = await query
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();

        return (privileges, totalCount);
    }

    /// <summary>
    /// Applies dynamic sorting to the query
    /// </summary>
    private static IQueryable<Privilege> ApplySorting(IQueryable<Privilege> query, string? sortBy, string? sortOrder)
    {
        // Default sorting if parameters are null or empty
        if (string.IsNullOrEmpty(sortBy) || string.IsNullOrEmpty(sortOrder))
        {
            return query.OrderBy(p => p.Name);
        }

        return sortBy.ToLower() switch
        {
            "name" => sortOrder.ToLower() == "desc" 
                ? query.OrderByDescending(p => p.Name)
                : query.OrderBy(p => p.Name),
            "description" => sortOrder.ToLower() == "desc" 
                ? query.OrderByDescending(p => p.Description)
                : query.OrderBy(p => p.Description),
            "createddate" => sortOrder.ToLower() == "desc" 
                ? query.OrderByDescending(p => p.CreatedDate)
                : query.OrderBy(p => p.CreatedDate),
            "updateddate" => sortOrder.ToLower() == "desc" 
                ? query.OrderByDescending(p => p.UpdatedDate)
                : query.OrderBy(p => p.UpdatedDate),
            "isactive" => sortOrder.ToLower() == "desc" 
                ? query.OrderByDescending(p => p.IsActive)
                : query.OrderBy(p => p.IsActive),
            _ => query.OrderBy(p => p.Name)
        };
    }
} 