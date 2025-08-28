using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController : BaseController
{
    private readonly ICategoryService _categoryService;
    
    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<JsonModel> GetAllCategories()
    {
        return await _categoryService.GetAllCategoriesAsync(GetToken(HttpContext));
    }

    [HttpGet("{id}")]
    public async Task<JsonModel> GetCategory(Guid id)
    {
        return await _categoryService.GetCategoryAsync(id, GetToken(HttpContext));
    }

    [HttpPost]
    public async Task<JsonModel> CreateCategory([FromBody] CreateCategoryDto createCategoryDto)
    {
        return await _categoryService.CreateCategoryAsync(createCategoryDto, GetToken(HttpContext));
    }

    [HttpPut("{id}")]
    public async Task<JsonModel> UpdateCategory(Guid id, [FromBody] UpdateCategoryDto updateCategoryDto)
    {
        if (!Guid.TryParse(updateCategoryDto.Id, out var dtoId) || id != dtoId)
            return new JsonModel { data = new object(), Message = "ID mismatch", StatusCode = 400 };
        return await _categoryService.UpdateCategoryAsync(id, updateCategoryDto, GetToken(HttpContext));
    }

    [HttpDelete("{id}")]
    public async Task<JsonModel> DeleteCategory(Guid id)
    {
        return await _categoryService.DeleteCategoryAsync(id, GetToken(HttpContext));
    }

    [HttpGet("active")]
    public async Task<JsonModel> GetActiveCategories()
    {
        return await _categoryService.GetActiveCategoriesAsync(GetToken(HttpContext));
    }

    [HttpGet("search")]
    public async Task<JsonModel> SearchCategories([FromQuery] string searchTerm)
    {
        return await _categoryService.SearchCategoriesAsync(searchTerm, GetToken(HttpContext));
    }

    [HttpGet("{id}/plans")]
    public async Task<JsonModel> GetCategoryPlans(Guid id)
    {
        return await _categoryService.GetCategoryPlansAsync(id, GetToken(HttpContext));
    }

    [HttpGet("count/active")]
    public async Task<JsonModel> GetActiveCategoryCount()
    {
        return await _categoryService.GetActiveCategoryCountAsync(GetToken(HttpContext));
    }

    [HttpGet("paged")]
    public async Task<JsonModel> GetAllCategoriesPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] bool? isActive = null)
    {
        return await _categoryService.GetAllCategoriesAsync(page, pageSize, searchTerm, isActive, GetToken(HttpContext));
    }
} 