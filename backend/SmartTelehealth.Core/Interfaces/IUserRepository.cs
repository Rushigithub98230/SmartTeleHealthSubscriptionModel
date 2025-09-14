using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IUserRepository : IRepositoryBase<User>
{
    // Basic CRUD methods are inherited from IRepositoryBase<User>
    // GetByIdAsync, GetAllAsync, CreateAsync, UpdateAsync, DeleteAsync, ExistsAsync

    // Specialized methods
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUserNameAsync(string userName);
    Task<User?> GetByRefreshTokenAsync(string refreshToken);
    Task<IEnumerable<User>> GetActiveUsersAsync();
    Task<bool> ExistsByEmailAsync(string email);
    Task<int> GetActiveUserCountAsync();
    Task<IEnumerable<User>> SearchUsersAsync(string searchTerm);
    Task<IEnumerable<User>> GetUsersBySubscriptionStatusAsync(string status);
    Task<object> GetUserAnalyticsAsync();
    Task<IEnumerable<User>> GetByUserTypeAsync(string userType);
    Task<User?> GetByLicenseNumberAsync(string licenseNumber);
    Task<IEnumerable<User>> GetByRoleAsync(string role);
} 