using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Application.Interfaces;

public interface IUserRoleRepository
{
    Task<UserRole?> GetByIdAsync(int id);
    Task<UserRole?> GetByNameAsync(string name);
    Task<IEnumerable<UserRole>> GetAllAsync();
}
