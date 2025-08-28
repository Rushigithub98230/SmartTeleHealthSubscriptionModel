using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface ICategoryService
{
    Task<JsonModel> GetCategoryAsync(Guid id, TokenModel tokenModel);
    Task<JsonModel> GetAllCategoriesAsync(TokenModel tokenModel);
    Task<JsonModel> GetAllCategoriesAsync(int page, int pageSize, string? searchTerm, bool? isActive, TokenModel tokenModel);
    Task<JsonModel> GetActiveCategoriesAsync(TokenModel tokenModel);
    Task<JsonModel> CreateCategoryAsync(CreateCategoryDto createDto, TokenModel tokenModel);
    Task<JsonModel> UpdateCategoryAsync(Guid id, UpdateCategoryDto updateDto, TokenModel tokenModel);
    Task<JsonModel> DeleteCategoryAsync(Guid id, TokenModel tokenModel);
    Task<JsonModel> ExistsAsync(Guid id, TokenModel tokenModel);
    Task<JsonModel> SearchCategoriesAsync(string searchTerm, TokenModel tokenModel);
    Task<JsonModel> GetCategoryPlansAsync(Guid categoryId, TokenModel tokenModel);
    Task<JsonModel> GetActiveCategoryCountAsync(TokenModel tokenModel);
} 