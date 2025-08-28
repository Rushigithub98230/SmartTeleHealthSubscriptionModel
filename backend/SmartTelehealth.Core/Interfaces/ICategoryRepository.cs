using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid id);
    Task<IEnumerable<Category>> GetAllAsync();
    Task<IEnumerable<Category>> GetAllActiveAsync();
    Task<IEnumerable<Category>> GetByDisplayOrderAsync();
    Task<Category> CreateAsync(Category category);
    Task<Category> UpdateAsync(Category category);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> ExistsByNameAsync(string name);
    Task<int> GetActiveCategoryCountAsync();
    Task<IEnumerable<Category>> SearchCategoriesAsync(string searchTerm);
} 