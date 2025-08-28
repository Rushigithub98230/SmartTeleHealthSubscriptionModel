using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using AutoMapper;

namespace SmartTelehealth.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CategoryService> _logger;
    
    public CategoryService(
        ICategoryRepository categoryRepository,
        ISubscriptionRepository subscriptionRepository,
        IMapper mapper,
        ILogger<CategoryService> logger)
    {
        _categoryRepository = categoryRepository;
        _subscriptionRepository = subscriptionRepository;
        _mapper = mapper;
        _logger = logger;
    }
    
    public async Task<JsonModel> GetCategoryAsync(Guid id, TokenModel tokenModel)
    {
        try
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                return new JsonModel { data = new object(), Message = "Category not found", StatusCode = 404 };
            
            var categoryDto = _mapper.Map<CategoryDto>(category);
            _logger.LogInformation("Category {CategoryId} retrieved by user {UserId}", id, tokenModel.UserID);
            return new JsonModel { data = categoryDto, Message = "Category retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category {Id} by user {UserId}", id, tokenModel.UserID);
            return new JsonModel { data = new object(), Message = "An error occurred while retrieving the category", StatusCode = 500 };
        }
    }
    
    public async Task<JsonModel> GetAllCategoriesAsync(TokenModel tokenModel)
    {
        try
        {
            var categories = await _categoryRepository.GetAllActiveAsync();
            var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);
            _logger.LogInformation("All categories retrieved by user {UserId}", tokenModel.UserID);
            return new JsonModel { data = categoryDtos, Message = "Categories retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all categories by user {UserId}", tokenModel.UserID);
            return new JsonModel { data = new object(), Message = "An error occurred while retrieving categories", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetAllCategoriesAsync(int page, int pageSize, string? searchTerm, bool? isActive, TokenModel tokenModel)
    {
        try
        {
            var categories = await _categoryRepository.GetAllActiveAsync();
            var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);
            
            // Apply search filter if provided
            if (!string.IsNullOrEmpty(searchTerm))
            {
                categoryDtos = categoryDtos.Where(c => c.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            }
            
            // Apply active filter if provided
            if (isActive.HasValue)
            {
                categoryDtos = categoryDtos.Where(c => c.IsActive == isActive.Value);
            }
            
            // Apply pagination
            var totalCount = categoryDtos.Count();
            var pagedCategories = categoryDtos
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            
            var result = new
            {
                Categories = pagedCategories,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };
            
            _logger.LogInformation("Paginated categories retrieved by user {UserId}: page {Page}, pageSize {PageSize}", 
                tokenModel.UserID, page, pageSize);
            return new JsonModel { data = result, Message = "Categories retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paginated categories by user {UserId}: page {Page}, pageSize {PageSize}", 
                tokenModel.UserID, page, pageSize);
            return new JsonModel { data = new object(), Message = "An error occurred while retrieving categories", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetActiveCategoriesAsync(TokenModel tokenModel)
    {
        try
        {
            var categories = await _categoryRepository.GetAllActiveAsync();
            var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);
            _logger.LogInformation("Active categories retrieved by user {UserId}", tokenModel.UserID);
            return new JsonModel { data = categoryDtos, Message = "Active categories retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active categories by user {UserId}", tokenModel.UserID);
            return new JsonModel { data = new object(), Message = "An error occurred while retrieving active categories", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> CreateCategoryAsync(CreateCategoryDto createDto, TokenModel tokenModel)
    {
        try
        {
            var category = _mapper.Map<Category>(createDto);
            category.CreatedDate = DateTime.UtcNow;
            category.IsActive = true;
            
            var createdCategory = await _categoryRepository.CreateAsync(category);
            var categoryDto = _mapper.Map<CategoryDto>(createdCategory);
            
            _logger.LogInformation("Category '{CategoryName}' created by user {UserId}", createDto.Name, tokenModel.UserID);
            return new JsonModel { data = categoryDto, Message = "Category created successfully", StatusCode = 201 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category '{CategoryName}' by user {UserId}", createDto.Name, tokenModel.UserID);
            return new JsonModel { data = new object(), Message = "An error occurred while creating the category", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> UpdateCategoryAsync(Guid id, UpdateCategoryDto updateDto, TokenModel tokenModel)
    {
        try
        {
            var existingCategory = await _categoryRepository.GetByIdAsync(id);
            if (existingCategory == null)
                return new JsonModel { data = new object(), Message = "Category not found", StatusCode = 404 };
            
            _mapper.Map(updateDto, existingCategory);
            existingCategory.UpdatedDate = DateTime.UtcNow;
            
            var updatedCategory = await _categoryRepository.UpdateAsync(existingCategory);
            var categoryDto = _mapper.Map<CategoryDto>(updatedCategory);
            
            _logger.LogInformation("Category {CategoryId} updated by user {UserId}", id, tokenModel.UserID);
            return new JsonModel { data = categoryDto, Message = "Category updated successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category {CategoryId} by user {UserId}", id, tokenModel.UserID);
            return new JsonModel { data = new object(), Message = "An error occurred while updating the category", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> DeleteCategoryAsync(Guid id, TokenModel tokenModel)
    {
        try
        {
            var existingCategory = await _categoryRepository.GetByIdAsync(id);
            if (existingCategory == null)
                return new JsonModel { data = new object(), Message = "Category not found", StatusCode = 404 };
            
            // Check if category is being used by any subscriptions
            var subscriptionsUsingCategory = await _subscriptionRepository.GetByCategoryIdAsync(id);
            if (subscriptionsUsingCategory.Any())
            {
                return new JsonModel { data = new object(), Message = "Cannot delete category as it is being used by subscriptions", StatusCode = 400 };
            }
            
            existingCategory.IsActive = false;
            existingCategory.DeletedDate = DateTime.UtcNow;
            
            await _categoryRepository.UpdateAsync(existingCategory);
            
            _logger.LogInformation("Category {CategoryId} deleted by user {UserId}", id, tokenModel.UserID);
            return new JsonModel { data = new object(), Message = "Category deleted successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category {CategoryId} by user {UserId}", id, tokenModel.UserID);
            return new JsonModel { data = new object(), Message = "An error occurred while deleting the category", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> ExistsAsync(Guid id, TokenModel tokenModel)
    {
        try
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            var exists = category != null && category.IsActive;
            
            _logger.LogInformation("Category existence check for {CategoryId} by user {UserId}: {Exists}", id, tokenModel.UserID, exists);
            return new JsonModel { data = exists, Message = "Category existence checked successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking category existence for {CategoryId} by user {UserId}", id, tokenModel.UserID);
            return new JsonModel { data = new object(), Message = "An error occurred while checking category existence", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> SearchCategoriesAsync(string searchTerm, TokenModel tokenModel)
    {
        try
        {
            var categories = await _categoryRepository.GetAllActiveAsync();
            var searchResults = categories
                .Where(c => c.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                           c.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();
            
            var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(searchResults);
            
            _logger.LogInformation("Categories searched for term '{SearchTerm}' by user {UserId}: {ResultCount} results", 
                searchTerm, tokenModel.UserID, searchResults.Count);
            return new JsonModel { data = categoryDtos, Message = "Categories searched successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching categories for term '{SearchTerm}' by user {UserId}", searchTerm, tokenModel.UserID);
            return new JsonModel { data = new object(), Message = "An error occurred while searching categories", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetCategoryPlansAsync(Guid categoryId, TokenModel tokenModel)
    {
        try
        {
            var subscriptions = await _subscriptionRepository.GetByCategoryIdAsync(categoryId);
            var subscriptionDtos = _mapper.Map<IEnumerable<SubscriptionDto>>(subscriptions);
            
            _logger.LogInformation("Category plans retrieved for category {CategoryId} by user {UserId}: {PlanCount} plans", 
                categoryId, tokenModel.UserID, subscriptions.Count());
            return new JsonModel { data = subscriptionDtos, Message = "Category plans retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category plans for category {CategoryId} by user {UserId}", categoryId, tokenModel.UserID);
            return new JsonModel { data = new object(), Message = "An error occurred while retrieving category plans", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetActiveCategoryCountAsync(TokenModel tokenModel)
    {
        try
        {
            var categories = await _categoryRepository.GetAllActiveAsync();
            var activeCount = categories.Count(c => c.IsActive);
            
            _logger.LogInformation("Active category count retrieved by user {UserId}: {Count}", tokenModel.UserID, activeCount);
            return new JsonModel { data = activeCount, Message = "Active category count retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active category count by user {UserId}", tokenModel.UserID);
            return new JsonModel { data = new object(), Message = "An error occurred while retrieving active category count", StatusCode = 500 };
        }
    }
} 