using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories;

public class PrivilegeRepository : IPrivilegeRepository
{
    private readonly ApplicationDbContext _context;
    public PrivilegeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Privilege?> GetByIdAsync(Guid id)
        => await _context.Privileges.FindAsync(id);

    public async Task<IEnumerable<Privilege>> GetAllAsync()
        => await _context.Privileges.ToListAsync();

    public async Task AddAsync(Privilege privilege)
    {
        _context.Privileges.Add(privilege);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Privilege privilege)
    {
        _context.Privileges.Update(privilege);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _context.Privileges.FindAsync(id);
        if (entity != null)
        {
            _context.Privileges.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
} 