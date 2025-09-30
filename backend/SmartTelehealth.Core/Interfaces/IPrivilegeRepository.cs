using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IPrivilegeRepository : IRepositoryBase<Privilege>
{
    // Basic CRUD methods are inherited from IRepositoryBase<Privilege>
    // GetByIdAsync, GetAllAsync, CreateAsync, UpdateAsync, DeleteAsync, ExistsAsync

    // Specialized methods for Privilege entity
    Task<Privilege?> GetByNameAsync(string name);
    Task<bool> ExistsByNameAsync(string name);
    Task<IEnumerable<Privilege>> GetByIdsAsync(IEnumerable<Guid> privilegeIds);
    
    // Legacy method for backward compatibility
    Task AddAsync(Privilege privilege);
    
    // Database-level filtering, pagination, and sorting
    Task<(IEnumerable<Privilege> Privileges, int TotalCount)> GetPrivilegesWithFilteringAsync(
        int page, int pageSize, string? search, string? category, string? status, string? sortBy = "Name", string? sortOrder = "asc");
} 