using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories;

public class CategoryRepository : RepositoryBase<Category>, ICategoryRepository
{
    private readonly ApplicationDbContext _context;

    public CategoryRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    // Use base class methods for basic CRUD operations
    // GetByIdAsync, GetAllAsync, CreateAsync, UpdateAsync, DeleteAsync, ExistsAsync are inherited from RepositoryBase

    // Custom method to get category by ID with related data and business logic
    public async Task<Category?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _context.Categories
            .Include(c => c.SubscriptionPlans.Where(sp => sp.IsActive))
            .Include(c => c.ProviderCategories.Where(pc => pc.IsAvailable))
                .ThenInclude(pc => pc.Provider)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
    }

    // Custom method to get all categories with related data and business logic
    public async Task<IEnumerable<Category>> GetAllWithDetailsAsync()
    {
        return await _context.Categories
            .Include(c => c.SubscriptionPlans.Where(sp => sp.IsActive))
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();
    }

    // Custom method to create category with audit fields
    public async Task<Category> CreateCategoryAsync(Category category)
    {
        return await base.CreateAsync(category);
    }

    // Custom method to update category with audit fields
    public async Task<Category> UpdateCategoryAsync(Category category)
    {
        return await base.UpdateAsync(category);
    }

    // Custom method to soft delete category
    public async Task<bool> DeleteCategoryAsync(Guid id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return false;

        category.IsActive = false;
        category.IsDeleted = true;
        await _context.SaveChangesAsync();
        return true;
    }

    // Custom method to check if category exists with business logic
    public async Task<bool> ExistsCategoryAsync(Guid id)
    {
        return await _context.Categories
            .AnyAsync(c => c.Id == id && !c.IsDeleted);
    }

    // Specialized methods for Category entity
    public async Task<IEnumerable<Category>> GetAllActiveAsync()
    {
        return await _context.Categories
            .Include(c => c.SubscriptionPlans.Where(sp => sp.IsActive))
            .Where(c => c.IsActive && !c.IsDeleted)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();
    }

    public async Task<IEnumerable<Category>> GetByDisplayOrderAsync()
    {
        return await _context.Categories
            .Include(c => c.SubscriptionPlans.Where(sp => sp.IsActive))
            .Where(c => c.IsActive && !c.IsDeleted)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await _context.Categories
            .AnyAsync(c => c.Name.ToLower() == name.ToLower() && !c.IsDeleted);
    }

    public async Task<int> GetActiveCategoryCountAsync()
    {
        return await _context.Categories
            .CountAsync(c => c.IsActive && !c.IsDeleted);
    }

    public async Task<IEnumerable<Category>> SearchCategoriesAsync(string searchTerm)
    {
        return await _context.Categories
            .Include(c => c.SubscriptionPlans.Where(sp => sp.IsActive))
            .Where(c => (c.Name.ToLower().Contains(searchTerm.ToLower()) || 
                        c.Description.ToLower().Contains(searchTerm.ToLower())) && 
                       c.IsActive && !c.IsDeleted)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves categories with database-level filtering, pagination, and sorting
    /// </summary>
    public async Task<(IEnumerable<Category> Categories, int TotalCount)> GetCategoriesWithFilteringAsync(
        int page, int pageSize, string? search, bool? isActive, string? sortBy = "DisplayOrder", string? sortOrder = "asc")
    {
        var query = _context.Categories
            .Include(c => c.SubscriptionPlans.Where(sp => sp.IsActive))
            .Where(c => !c.IsDeleted)
            .AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(term) ||
                (c.Description != null && c.Description.ToLower().Contains(term)));
        }

        // Apply active filter
        if (isActive.HasValue)
        {
            query = query.Where(c => c.IsActive == isActive.Value);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply sorting
        query = ApplySorting(query, sortBy, sortOrder);

        // Apply pagination
        var skip = (page - 1) * pageSize;
        var categories = await query
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();

        return (categories, totalCount);
    }

    private static IQueryable<Category> ApplySorting(IQueryable<Category> query, string? sortBy, string? sortOrder)
    {
        // Default sorting if parameters are null or empty
        if (string.IsNullOrEmpty(sortBy) || string.IsNullOrEmpty(sortOrder))
        {
            return query.OrderBy(c => c.Name);
        }

        return sortBy.ToLower() switch
        {
            "name" => sortOrder.ToLower() == "desc"
                ? query.OrderByDescending(c => c.Name)
                : query.OrderBy(c => c.Name),
            "description" => sortOrder.ToLower() == "desc"
                ? query.OrderByDescending(c => c.Description)
                : query.OrderBy(c => c.Description),
            "displayorder" => sortOrder.ToLower() == "desc"
                ? query.OrderByDescending(c => c.DisplayOrder)
                : query.OrderBy(c => c.DisplayOrder),
            "createddate" => sortOrder.ToLower() == "desc"
                ? query.OrderByDescending(c => c.CreatedDate)
                : query.OrderBy(c => c.CreatedDate),
            "updateddate" => sortOrder.ToLower() == "desc"
                ? query.OrderByDescending(c => c.UpdatedDate)
                : query.OrderBy(c => c.UpdatedDate),
            "isactive" => sortOrder.ToLower() == "desc"
                ? query.OrderByDescending(c => c.IsActive)
                : query.OrderBy(c => c.IsActive),
            _ => query.OrderBy(c => c.DisplayOrder)
        };
    }
} 