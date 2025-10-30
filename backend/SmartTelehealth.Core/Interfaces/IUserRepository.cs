using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IUserRepository : IRepositoryBase<User>
{
    // Basic CRUD methods are inherited from IRepositoryBase<User>
    // GetByIdAsync, GetAllAsync, CreateAsync, UpdateAsync, DeleteAsync, ExistsAsync

    // Custom methods with business logic
    Task<User?> GetByIdWithDetailsAsync(int id);
    Task<IEnumerable<User>> GetAllWithDetailsAsync();
    Task<User> CreateUserAsync(User user);
    Task<User> UpdateUserAsync(User user);
    Task<bool> DeleteUserAsync(int id);
    Task<bool> ExistsUserAsync(int id);

    // Specialized methods
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUserNameAsync(string userName);
    Task<User?> GetByRefreshTokenAsync(string refreshToken);
    Task<User?> GetUserByStripeCustomerIdAsync(string stripeCustomerId);
    Task<bool> UpdateStripeCustomerIdAsync(int userId, string stripeCustomerId);
    Task<IEnumerable<User>> GetActiveUsersAsync();
    Task<bool> ExistsByEmailAsync(string email);
    Task<int> GetActiveUserCountAsync();
    Task<IEnumerable<User>> SearchUsersAsync(string searchTerm);
    Task<IEnumerable<User>> GetUsersBySubscriptionStatusAsync(string status);
    Task<object> GetUserAnalyticsAsync();
    Task<IEnumerable<User>> GetByUserTypeAsync(string userType);
    Task<User?> GetByLicenseNumberAsync(string licenseNumber);
    Task<IEnumerable<User>> GetByRoleAsync(string role);
    
    // Analytics methods
    Task<int> GetTotalUsersCountAsync();
    Task<int> GetActiveUsersCountAsync(DateTime startDate, DateTime endDate);
    Task<int> GetNewUsersCountAsync(DateTime startDate, DateTime endDate);
    Task<int> GetTotalLoginsCountAsync(DateTime startDate, DateTime endDate);
} 