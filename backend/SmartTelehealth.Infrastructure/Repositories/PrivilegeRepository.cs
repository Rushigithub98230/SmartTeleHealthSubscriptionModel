using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories;

public class PrivilegeRepository : RepositoryBase<Privilege>, IPrivilegeRepository
{
    private readonly ApplicationDbContext _context;
    public PrivilegeRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    // Use base class methods for basic CRUD operations
    // GetByIdAsync, GetAllAsync, CreateAsync, UpdateAsync, DeleteAsync are inherited from RepositoryBase

    // Keep only specialized methods that add value
    public async Task<Privilege?> GetByNameAsync(string name)
    {
        return await _context.Privileges
            .FirstOrDefaultAsync(p => p.Name.ToLower() == name.ToLower());
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await _context.Privileges
            .AnyAsync(p => p.Name.ToLower() == name.ToLower());
    }

    // Legacy method for backward compatibility - delegates to base CreateAsync
    public async Task AddAsync(Privilege privilege)
    {
        await CreateAsync(privilege);
    }
} 