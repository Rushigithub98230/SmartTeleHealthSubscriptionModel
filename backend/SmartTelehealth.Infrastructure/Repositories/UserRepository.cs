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

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);
    }

    public async Task<User?> GetByUserNameAsync(string userName)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.UserName == userName && !u.IsDeleted);
    }

    public async Task<User?> GetByRefreshTokenAsync(string refreshToken)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken && 
                                     u.RefreshTokenExpiry > DateTime.UtcNow && 
                                     !u.IsDeleted);
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users
            .Where(u => !u.IsDeleted)
            .OrderBy(u => u.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        return await _context.Users
            .Where(u => u.IsActive && !u.IsDeleted)
            .OrderBy(u => u.CreatedDate)
            .ToListAsync();
    }

    public async Task<User> CreateAsync(User user)
    {
        user.CreatedDate = DateTime.UtcNow;
        Create(user);
        await SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        user.UpdatedDate = DateTime.UtcNow;
        Update(user);
        await SaveChangesAsync();
        return user;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return false;

        user.IsDeleted = true;
        user.UpdatedDate = DateTime.UtcNow;
        Update(user);
        await SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
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
} 