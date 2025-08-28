using System.Linq.Expressions;

namespace SmartTelehealth.Core.Interfaces;

public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task<bool> ExistsAsync(Guid id);
    Task<int> CountAsync();
    Task<int> CountAsync(Expression<Func<T, bool>> predicate);
    Task SaveChangesAsync();
} 