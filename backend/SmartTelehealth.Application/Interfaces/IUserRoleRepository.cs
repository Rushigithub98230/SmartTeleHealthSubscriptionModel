using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;

namespace SmartTelehealth.Application.Interfaces;

public interface IUserRoleRepository : IRepositoryBase<UserRole>
{
    Task<UserRole?> GetByIdAsync(int id);
    Task<UserRole?> GetByNameAsync(string name);
    Task<IEnumerable<UserRole>> GetAllAsync();
}
