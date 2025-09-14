using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories;

public class UserRoleRepository : RepositoryBase<UserRole>, IUserRoleRepository
{
    private readonly ApplicationDbContext _context;

    public UserRoleRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    // Use base class methods for basic CRUD operations
    // GetByIdAsync, GetAllAsync, CreateAsync, UpdateAsync, DeleteAsync, ExistsAsync are inherited from RepositoryBase

    // Specialized methods for UserRole entity
    public async Task<UserRole?> GetByNameAsync(string name)
    {
        return await _context.UserRoles
            .FirstOrDefaultAsync(ur => ur.Name.ToLower() == name.ToLower());
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await _context.UserRoles
            .AnyAsync(ur => ur.Name.ToLower() == name.ToLower());
    }

    // Override GetByIdAsync to handle int ID type (base class expects object)
    public override async Task<UserRole?> GetByIdAsync(object id)
    {
        if (id is int intId)
        {
            return await _context.UserRoles.FindAsync(intId);
        }
        return null;
    }
}
