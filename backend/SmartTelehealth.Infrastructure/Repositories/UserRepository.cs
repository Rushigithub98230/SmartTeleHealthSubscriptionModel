using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories;

public class UserRepository : RepositoryBase<User>, IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    // Use base class methods for basic CRUD operations
    // GetByIdAsync, GetAllAsync, CreateAsync, UpdateAsync, DeleteAsync, ExistsAsync are inherited from RepositoryBase

    // Custom method to get user by ID with related data and business logic
    public async Task<User?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.Users
            .Include(u => u.UserRole)
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.UserRole)
            .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);
    }

    public async Task<User?> GetByUserNameAsync(string userName)
    {
        return await _context.Users
            .Include(u => u.UserRole)
            .FirstOrDefaultAsync(u => u.UserName == userName && !u.IsDeleted);
    }

    public async Task<User?> GetByRefreshTokenAsync(string refreshToken)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken && 
                                     u.RefreshTokenExpiry > DateTime.UtcNow && 
                                     !u.IsDeleted);
    }

    public async Task<User?> GetUserByStripeCustomerIdAsync(string stripeCustomerId)
    {
        return await _context.Users
            .Include(u => u.UserRole)
            .FirstOrDefaultAsync(u => u.StripeCustomerId == stripeCustomerId && !u.IsDeleted);
    }

    // Custom method to get all users with related data and business logic
    public async Task<IEnumerable<User>> GetAllWithDetailsAsync()
    {
        return await _context.Users
            .Include(u => u.UserRole)
            .Where(u => !u.IsDeleted)
            .OrderBy(u => u.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        return await _context.Users
            .Include(u => u.UserRole)
            .Where(u => u.IsActive && !u.IsDeleted)
            .OrderBy(u => u.CreatedDate)
            .ToListAsync();
    }

    // Custom method to create user with audit fields
    public async Task<User> CreateUserAsync(User user)
    {
        return await base.CreateAsync(user);
    }

    // Custom method to update user with audit fields
    public async Task<User> UpdateUserAsync(User user)
    {
        return await base.UpdateAsync(user);
    }

    // Custom method to soft delete user
    public async Task<bool> DeleteUserAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return false;

        user.IsActive = false;
        user.IsDeleted = true;
        await _context.SaveChangesAsync();
        return true;
    }

    // Custom method to check if user exists with business logic
    public async Task<bool> ExistsUserAsync(int id)
    {
        return await _context.Users
            .AnyAsync(u => u.Id == id && !u.IsDeleted);
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _context.Users
            .AnyAsync(u => u.Email == email && !u.IsDeleted);
    }

    public async Task<int> GetActiveUserCountAsync()
    {
        return await _context.Users
            .CountAsync(u => u.IsActive && !u.IsDeleted);
    }

    public async Task<IEnumerable<User>> SearchUsersAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return new List<User>();

        return await _context.Users
            .Where(u => !u.IsDeleted && 
                       (u.FirstName.Contains(searchTerm) || 
                        u.LastName.Contains(searchTerm) || 
                        u.Email.Contains(searchTerm) || 
                        u.UserName.Contains(searchTerm)))
            .OrderBy(u => u.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetUsersBySubscriptionStatusAsync(string status)
    {
        return await _context.Users
            .Include(u => u.UserRole)
            .Include(u => u.Subscriptions)
            .Where(u => u.Subscriptions.Any(s => s.Status == status))
            .ToListAsync();
    }

    public async Task<object> GetUserAnalyticsAsync()
    {
        var totalUsers = await _context.Users.CountAsync(u => !u.IsDeleted);
        var activeUsers = await _context.Users.CountAsync(u => u.IsActive && !u.IsDeleted);
        var newUsersThisMonth = await _context.Users
            .CountAsync(u => !u.IsDeleted && u.CreatedDate >= DateTime.UtcNow.AddDays(-30));

        return new
        {
            TotalUsers = totalUsers,
            ActiveUsers = activeUsers,
            NewUsersThisMonth = newUsersThisMonth,
            InactiveUsers = totalUsers - activeUsers
        };
    }

    public async Task<IEnumerable<User>> GetByUserTypeAsync(string userType)
    {
        return await _context.Users
            .Where(u => !u.IsDeleted && u.UserType == userType)
            .OrderBy(u => u.CreatedDate)
            .ToListAsync();
    }

    public async Task<User?> GetByLicenseNumberAsync(string licenseNumber)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.LicenseNumber == licenseNumber && !u.IsDeleted);
    }

    public async Task<IEnumerable<User>> GetUsersByRoleAsync(string roleName)
    {
        return await _context.Users
            .Include(u => u.UserRole)
            .Include(u => u.Subscriptions)
            .Where(u => u.UserRole.Name == roleName)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetByRoleAsync(string role)
    {
        return await _context.Users
            .Include(u => u.UserRole)
            .Include(u => u.Subscriptions)
            .Where(u => u.UserRole.Name == role)
            .ToListAsync();
    }

    // Analytics methods
    public async Task<int> GetTotalUsersCountAsync()
    {
        return await _context.Users
            .Where(u => !u.IsDeleted)
            .CountAsync();
    }

    public async Task<int> GetActiveUsersCountAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Users
            .Where(u => !u.IsDeleted && 
                       u.LastLoginAt.HasValue && 
                       u.LastLoginAt.Value >= startDate && 
                       u.LastLoginAt.Value <= endDate)
            .CountAsync();
    }

    public async Task<int> GetNewUsersCountAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Users
            .Where(u => !u.IsDeleted && 
                       u.CreatedDate >= startDate && 
                       u.CreatedDate <= endDate)
            .CountAsync();
    }

    public async Task<int> GetTotalLoginsCountAsync(DateTime startDate, DateTime endDate)
    {
        // This is a placeholder implementation since we don't have a separate login tracking table
        // In a real implementation, you would have a UserLoginLogs table
        return await _context.Users
            .Where(u => !u.IsDeleted && 
                       u.LastLoginAt.HasValue && 
                       u.LastLoginAt.Value >= startDate && 
                       u.LastLoginAt.Value <= endDate)
            .CountAsync();
    }
} 