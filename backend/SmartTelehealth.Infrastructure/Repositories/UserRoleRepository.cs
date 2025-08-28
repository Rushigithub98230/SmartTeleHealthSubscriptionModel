using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories;

public class UserRoleRepository : IUserRoleRepository
{
    private readonly ApplicationDbContext _context;

    public UserRoleRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserRole?> GetByIdAsync(int id)
    {
        return await _context.UserRoles.FindAsync(id);
    }

    public async Task<UserRole?> GetByNameAsync(string name)
    {
        return await _context.UserRoles
            .FirstOrDefaultAsync(ur => ur.Name.ToLower() == name.ToLower());
    }

    public async Task<IEnumerable<UserRole>> GetAllAsync()
    {
        return await _context.UserRoles.ToListAsync();
    }
}
