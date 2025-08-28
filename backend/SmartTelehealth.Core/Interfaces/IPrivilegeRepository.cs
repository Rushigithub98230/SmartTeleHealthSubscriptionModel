using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IPrivilegeRepository
{
    Task<Privilege?> GetByIdAsync(Guid id);
    Task<IEnumerable<Privilege>> GetAllAsync();
    Task AddAsync(Privilege privilege);
    Task UpdateAsync(Privilege privilege);
    Task DeleteAsync(Guid id);
} 