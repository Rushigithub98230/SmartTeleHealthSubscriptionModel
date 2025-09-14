using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;

namespace SmartTelehealth.Application.Interfaces;

public interface IUserRoleRepository : IRepositoryBase<UserRole>
{
    // Basic CRUD methods are inherited from IRepositoryBase<UserRole>
    // GetByIdAsync, GetAllAsync, CreateAsync, UpdateAsync, DeleteAsync, ExistsAsync

    // Specialized methods for UserRole entity
    Task<UserRole?> GetByNameAsync(string name);
    Task<bool> ExistsByNameAsync(string name);
}
