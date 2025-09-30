using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface ICategoryRepository : IRepositoryBase<Category>
{
    // Basic CRUD methods are inherited from IRepositoryBase<Category>
    // GetByIdAsync, GetAllAsync, CreateAsync, UpdateAsync, DeleteAsync, ExistsAsync

    // Custom methods with business logic
    Task<Category?> GetByIdWithDetailsAsync(Guid id);
    Task<IEnumerable<Category>> GetAllWithDetailsAsync();
    Task<Category> CreateCategoryAsync(Category category);
    Task<Category> UpdateCategoryAsync(Category category);
    Task<bool> DeleteCategoryAsync(Guid id);
    Task<bool> ExistsCategoryAsync(Guid id);

    // Specialized methods for Category entity
    Task<IEnumerable<Category>> GetAllActiveAsync();
    Task<IEnumerable<Category>> GetByDisplayOrderAsync();
    Task<bool> ExistsByNameAsync(string name);
    Task<int> GetActiveCategoryCountAsync();
    Task<IEnumerable<Category>> SearchCategoriesAsync(string searchTerm);
    
    // Database-level filtering, pagination, and sorting
    Task<(IEnumerable<Category> Categories, int TotalCount)> GetCategoriesWithFilteringAsync(
        int page, int pageSize, string? search, bool? isActive, string? sortBy = "DisplayOrder", string? sortOrder = "asc");
} 