using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MasterDataController : BaseController
{
    private readonly IMasterDataService _masterDataService;

    public MasterDataController(IMasterDataService masterDataService)
    {
        _masterDataService = masterDataService;
    }

    /// <summary>
    /// Get all billing cycles
    /// </summary>
    [HttpGet("billing-cycles")]
    public async Task<JsonModel> GetBillingCycles()
    {
        return await _masterDataService.GetBillingCyclesAsync(GetToken(HttpContext));
    }

    /// <summary>
    /// Get all currencies
    /// </summary>
    [HttpGet("currencies")]
    public async Task<JsonModel> GetCurrencies()
    {
        return await _masterDataService.GetCurrenciesAsync(GetToken(HttpContext));
    }

    /// <summary>
    /// Get all privilege types
    /// </summary>
    [HttpGet("privilege-types")]
    public async Task<JsonModel> GetPrivilegeTypes()
    {
        return await _masterDataService.GetPrivilegeTypesAsync(GetToken(HttpContext));
    }
}
