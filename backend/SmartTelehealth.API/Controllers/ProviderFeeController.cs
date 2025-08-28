using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProviderFeeController : BaseController
{
    private readonly IProviderFeeService _feeService;
    private readonly ICategoryFeeRangeService _feeRangeService;
    public ProviderFeeController(
        IProviderFeeService feeService,
        ICategoryFeeRangeService feeRangeService)
    {
        _feeService = feeService;
        _feeRangeService = feeRangeService;
    }

    /// <summary>
    /// Create a new provider fee proposal
    /// </summary>
    [HttpPost]
    public async Task<JsonModel> CreateFee([FromBody] CreateProviderFeeDto createDto)
    {
        return await _feeService.CreateFeeAsync(createDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Get fee by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<JsonModel> GetFee(Guid id)
    {
        return await _feeService.GetFeeAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Get fee by provider and category
    /// </summary>
    [HttpGet("provider/{providerId}/category/{categoryId}")]
    public async Task<JsonModel> GetFeeByProviderAndCategory(int providerId, Guid categoryId)
    {
        return await _feeService.GetFeeByProviderAndCategoryAsync(providerId, categoryId, GetToken(HttpContext));
    }

    /// <summary>
    /// Update fee proposal
    /// </summary>
    [HttpPut("{id}")]
    public async Task<JsonModel> UpdateFee(Guid id, [FromBody] UpdateProviderFeeDto updateDto)
    {
        return await _feeService.UpdateFeeAsync(id, updateDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Submit fee proposal for review
    /// </summary>
    [HttpPost("{id}/propose")]
    public async Task<JsonModel> ProposeFee(Guid id)
    {
        return await _feeService.ProposeFeeAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Review fee proposal (Admin only)
    /// </summary>
    [HttpPost("{id}/review")]
    
    public async Task<JsonModel> ReviewFee(Guid id, [FromBody] ReviewProviderFeeDto reviewDto)
    {
        return await _feeService.ReviewFeeAsync(id, reviewDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Get fees by provider
    /// </summary>
    [HttpGet("provider/{providerId}")]
    public async Task<JsonModel> GetFeesByProvider(int providerId)
    {
        return await _feeService.GetFeesByProviderAsync(providerId, GetToken(HttpContext));
    }

    /// <summary>
    /// Get fees by category
    /// </summary>
    [HttpGet("category/{categoryId}")]
    public async Task<JsonModel> GetFeesByCategory(Guid categoryId)
    {
        var result = await _feeService.GetFeesByCategoryAsync(categoryId, GetToken(HttpContext));
        return result;
    }

    /// <summary>
    /// Get all fees with optional filtering
    /// </summary>
    [HttpGet]
    
    public async Task<JsonModel> GetAllFees(
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        return await _feeService.GetAllFeesAsync(status, page, pageSize, GetToken(HttpContext));
    }

    /// <summary>
    /// Get pending fees
    /// </summary>
    [HttpGet("pending")]
    
    public async Task<JsonModel> GetPendingFees()
    {
        return await _feeService.GetPendingFeesAsync(GetToken(HttpContext));
    }

    /// <summary>
    /// Get fees by status
    /// </summary>
    [HttpGet("status/{status}")]
    
    public async Task<JsonModel> GetFeesByStatus(string status)
    {
        return await _feeService.GetFeesByStatusAsync(status, GetToken(HttpContext));
    }

    /// <summary>
    /// Delete fee
    /// </summary>
    [HttpDelete("{id}")]
    
    public async Task<JsonModel> DeleteFee(Guid id)
    {
        return await _feeService.DeleteFeeAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Get fee statistics
    /// </summary>
    [HttpGet("statistics")]
    
    public async Task<JsonModel> GetFeeStatistics()
    {
        return await _feeService.GetFeeStatisticsAsync(GetToken(HttpContext));
    }

    // Category Fee Range endpoints

    /// <summary>
    /// Create a new category fee range
    /// </summary>
    [HttpPost("ranges")]
    
    public async Task<JsonModel> CreateFeeRange([FromBody] CreateCategoryFeeRangeDto createDto)
    {
        return await _feeRangeService.CreateFeeRangeAsync(createDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Get fee range by ID
    /// </summary>
    [HttpGet("ranges/{id}")]
    public async Task<JsonModel> GetFeeRange(Guid id)
    {
        return await _feeRangeService.GetFeeRangeAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Get fee range by category
    /// </summary>
    [HttpGet("ranges/category/{categoryId}")]
    public async Task<JsonModel> GetFeeRangeByCategory(Guid categoryId)
    {
        return await _feeRangeService.GetFeeRangeByCategoryAsync(categoryId, GetToken(HttpContext));
    }

    /// <summary>
    /// Update fee range
    /// </summary>
    [HttpPut("ranges/{id}")]
    
    public async Task<JsonModel> UpdateFeeRange(Guid id, [FromBody] UpdateCategoryFeeRangeDto updateDto)
    {
        return await _feeRangeService.UpdateFeeRangeAsync(id, updateDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Get all fee ranges
    /// </summary>
    [HttpGet("ranges")]
    public async Task<JsonModel> GetAllFeeRanges()
    {
        return await _feeRangeService.GetAllFeeRangesAsync(GetToken(HttpContext));
    }

    /// <summary>
    /// Delete fee range
    /// </summary>
    [HttpDelete("ranges/{id}")]
    
    public async Task<JsonModel> DeleteFeeRange(Guid id)
    {
        return await _feeRangeService.DeleteFeeRangeAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Get fee range statistics
    /// </summary>
    [HttpGet("ranges/statistics")]
    
    public async Task<JsonModel> GetFeeRangeStatistics()
    {
        return await _feeRangeService.GetFeeRangeStatisticsAsync(GetToken(HttpContext));
    }
} 