using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;

namespace SmartTelehealth.API.Controllers;

/// <summary>
/// Controller responsible for comprehensive provider payout management and processing.
/// This controller provides extensive functionality for managing provider payouts, payout periods,
/// earnings calculation, payout processing, and financial reporting. It handles the complete
/// payout lifecycle from earnings calculation to payment processing and financial reconciliation.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProviderPayoutController : BaseController
{
    private readonly IProviderPayoutService _providerPayoutService;
    private readonly IPayoutPeriodService _periodService;

    /// <summary>
    /// Initializes a new instance of the ProviderPayoutController with required services.
    /// </summary>
    /// <param name="providerPayoutService">Service for handling provider payout-related business logic</param>
    /// <param name="periodService">Service for handling payout period operations</param>
    public ProviderPayoutController(
        IProviderPayoutService providerPayoutService,
        IPayoutPeriodService periodService)
    {
        _providerPayoutService = providerPayoutService;
        _periodService = periodService;
    }

    /// <summary>
    /// Get payout by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<JsonModel> GetPayout(Guid id)
    {
        return await _providerPayoutService.GetPayoutAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Process payout (Admin only)
    /// </summary>
    [HttpPost("{id}/process")]
    
    public async Task<JsonModel> ProcessPayout(Guid id, [FromBody] ProcessPayoutDto processDto)
    {
        return await _providerPayoutService.ProcessPayoutAsync(id, processDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Get payouts by provider
    /// </summary>
    [HttpGet("provider/{providerId}")]
    public async Task<JsonModel> GetPayoutsByProvider(int providerId)
    {
        return await _providerPayoutService.GetPayoutsByProviderAsync(providerId, GetToken(HttpContext));
    }

    /// <summary>
    /// Get payouts by period
    /// </summary>
    [HttpGet("period/{periodId}")]
    public async Task<JsonModel> GetPayoutsByPeriod(Guid periodId)
    {
        return await _providerPayoutService.GetPayoutsByPeriodAsync(periodId, GetToken(HttpContext));
    }

    /// <summary>
    /// Get all payouts with optional filtering
    /// </summary>
    [HttpGet]
    
    public async Task<JsonModel> GetAllPayouts(
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        return await _providerPayoutService.GetAllPayoutsAsync(status, page, pageSize, GetToken(HttpContext));
    }

    /// <summary>
    /// Get pending payouts
    /// </summary>
    [HttpGet("pending")]
    
    public async Task<JsonModel> GetPendingPayouts()
    {
        return await _providerPayoutService.GetPendingPayoutsAsync(GetToken(HttpContext));
    }

    /// <summary>
    /// Get payouts by status
    /// </summary>
    [HttpGet("status/{status}")]
    
    public async Task<JsonModel> GetPayoutsByStatus(string status)
    {
        return await _providerPayoutService.GetPayoutsByStatusAsync(status, GetToken(HttpContext));
    }

    /// <summary>
    /// Get provider earnings
    /// </summary>
    [HttpGet("provider/{providerId}/earnings")]
    public async Task<JsonModel> GetProviderEarnings(int providerId)
    {
        return await _providerPayoutService.GetProviderEarningsAsync(providerId, GetToken(HttpContext));
    }

    /// <summary>
    /// Get payout statistics
    /// </summary>
    [HttpGet("statistics")]
    
    public async Task<JsonModel> GetPayoutStatistics()
    {
        return await _providerPayoutService.GetPayoutStatisticsAsync(GetToken(HttpContext));
    }

    /// <summary>
    /// Generate payouts for a period
    /// </summary>
    [HttpPost("period/{periodId}/generate")]
    
    public async Task<JsonModel> GeneratePayoutsForPeriod(Guid periodId)
    {
        return await _providerPayoutService.GeneratePayoutsForPeriodAsync(periodId, GetToken(HttpContext));
    }

    /// <summary>
    /// Process all pending payouts
    /// </summary>
    [HttpPost("process-all-pending")]
    
    public async Task<JsonModel> ProcessAllPendingPayouts()
    {
        return await _providerPayoutService.ProcessAllPendingPayoutsAsync(GetToken(HttpContext));
    }

    // Payout Period Management

    /// <summary>
    /// Create a new payout period
    /// </summary>
    [HttpPost("periods")]
    
    public async Task<JsonModel> CreatePayoutPeriod([FromBody] CreatePayoutPeriodDto createDto)
    {
        return await _periodService.CreatePeriodAsync(createDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Get payout period by ID
    /// </summary>
    [HttpGet("periods/{id}")]
    public async Task<JsonModel> GetPayoutPeriod(Guid id)
    {
        return await _periodService.GetPeriodAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Update payout period
    /// </summary>
    [HttpPut("periods/{id}")]
    
    public async Task<JsonModel> UpdatePayoutPeriod(Guid id, [FromBody] CreatePayoutPeriodDto updateDto)
    {
        return await _periodService.UpdatePeriodAsync(id, updateDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Get all payout periods
    /// </summary>
    [HttpGet("periods")]
    public async Task<JsonModel> GetAllPayoutPeriods()
    {
        return await _periodService.GetAllPeriodsAsync(GetToken(HttpContext));
    }

    /// <summary>
    /// Get active payout periods
    /// </summary>
    [HttpGet("periods/active")]
    public async Task<JsonModel> GetActivePayoutPeriods()
    {
        return await _periodService.GetActivePeriodsAsync(GetToken(HttpContext));
    }

    /// <summary>
    /// Delete payout period
    /// </summary>
    [HttpDelete("periods/{id}")]
    
    public async Task<JsonModel> DeletePayoutPeriod(Guid id)
    {
        return await _periodService.DeletePeriodAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Process payout period
    /// </summary>
    [HttpPost("periods/{id}/process")]
    
    public async Task<JsonModel> ProcessPayoutPeriod(Guid id)
    {
        return await _periodService.ProcessPeriodAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Get payout period statistics
    /// </summary>
    [HttpGet("periods/statistics")]
    
    public async Task<JsonModel> GetPayoutPeriodStatistics()
    {
        return await _periodService.GetPeriodStatisticsAsync(GetToken(HttpContext));
    }
} 