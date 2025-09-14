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

    // Override GetByIdAsync to include related data and apply business logic
    public override async Task<User?> GetByIdAsync(object id)
    {
        if (id is int intId)
        {
            return await _context.Users
                .Include(u => u.UserRole)
                .FirstOrDefaultAsync(u => u.Id == intId && !u.IsDeleted);
        }
        return null;
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

    // Override GetAllAsync to include related data and apply business logic
    public override async Task<IEnumerable<User>> GetAllAsync()
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

    // Override CreateAsync to add audit fields
    public override async Task<User> CreateAsync(User user)
    {
        user.CreatedDate = DateTime.UtcNow;
        return await base.CreateAsync(user);
    }

    // Override UpdateAsync to add audit fields
    public override async Task<User> UpdateAsync(User user)
    {
        user.UpdatedDate = DateTime.UtcNow;
        return await base.UpdateAsync(user);
    }

    // Override DeleteAsync to implement soft delete
    public override async Task<bool> DeleteAsync(object id)
    {
        if (id is int intId)
        {
            var user = await _context.Users.FindAsync(intId);
            if (user == null) return false;

            user.IsDeleted = true;
            user.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
        return false;
    }

    // Override ExistsAsync to apply business logic
    public override async Task<bool> ExistsAsync(object id)
    {
        if (id is int intId)
        {
            return await _context.Users
                .AnyAsync(u => u.Id == intId && !u.IsDeleted);
        }
        return false;
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
} 