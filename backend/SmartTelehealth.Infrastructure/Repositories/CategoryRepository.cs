using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _context;

    public CategoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Category?> GetByIdAsync(Guid id)
    {
        return await _context.Categories
            .Include(c => c.SubscriptionPlans.Where(sp => sp.IsActive))
            .Include(c => c.ProviderCategories.Where(pc => pc.IsAvailable))
                .ThenInclude(pc => pc.Provider)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        return await _context.Categories
            .Include(c => c.SubscriptionPlans.Where(sp => sp.IsActive))
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();
    }

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

    public async Task<Category> CreateAsync(Category category)
    {
        category.CreatedDate = DateTime.UtcNow;
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return category;
    }

    public async Task<Category> UpdateAsync(Category category)
    {
        category.UpdatedDate = DateTime.UtcNow;
        _context.Categories.Update(category);
        await _context.SaveChangesAsync();
        return category;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return false;

        category.IsDeleted = true;
        category.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Categories
            .AnyAsync(c => c.Id == id && !c.IsDeleted);
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