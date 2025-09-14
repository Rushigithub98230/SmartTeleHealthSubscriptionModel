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

    // Override GetByIdAsync to include related data and apply business logic
    public override async Task<Category?> GetByIdAsync(object id)
    {
        if (id is Guid guidId)
        {
            return await _context.Categories
                .Include(c => c.SubscriptionPlans.Where(sp => sp.IsActive))
                .Include(c => c.ProviderCategories.Where(pc => pc.IsAvailable))
                    .ThenInclude(pc => pc.Provider)
                .FirstOrDefaultAsync(c => c.Id == guidId && !c.IsDeleted);
        }
        return null;
    }

    // Override GetAllAsync to include related data and apply business logic
    public override async Task<IEnumerable<Category>> GetAllAsync()
    {
        return await _context.Categories
            .Include(c => c.SubscriptionPlans.Where(sp => sp.IsActive))
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();
    }

    // Override CreateAsync to set audit fields
    public override async Task<Category> CreateAsync(Category category)
    {
        category.CreatedDate = DateTime.UtcNow;
        return await base.CreateAsync(category);
    }

    // Override UpdateAsync to set audit fields
    public override async Task<Category> UpdateAsync(Category category)
    {
        category.UpdatedDate = DateTime.UtcNow;
        return await base.UpdateAsync(category);
    }

    // Override DeleteAsync to implement soft delete
    public override async Task<bool> DeleteAsync(object id)
    {
        if (id is Guid guidId)
        {
            var category = await _context.Categories.FindAsync(guidId);
            if (category == null) return false;

            category.IsDeleted = true;
            category.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
        return false;
    }

    // Override ExistsAsync to apply business logic
    public override async Task<bool> ExistsAsync(object id)
    {
        if (id is Guid guidId)
        {
            return await _context.Categories
                .AnyAsync(c => c.Id == guidId && !c.IsDeleted);
        }
        return false;
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
} 