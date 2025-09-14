using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface ICategoryRepository : IRepositoryBase<Category>
{
    // Basic CRUD methods are inherited from IRepositoryBase<Category>
    // GetByIdAsync, GetAllAsync, CreateAsync, UpdateAsync, DeleteAsync, ExistsAsync

    // Specialized methods for Category entity
    Task<IEnumerable<Category>> GetAllActiveAsync();
    Task<IEnumerable<Category>> GetByDisplayOrderAsync();
    Task<bool> ExistsByNameAsync(string name);
    Task<int> GetActiveCategoryCountAsync();
    Task<IEnumerable<Category>> SearchCategoriesAsync(string searchTerm);
} 