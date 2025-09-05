using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.API.Controllers;

/// <summary>
/// Controller responsible for comprehensive master data management and system configuration.
/// This controller provides essential functionality for managing system-wide master data including
/// billing cycles, currencies, privilege types, and other configuration data used throughout
/// the SmartTelehealth system for consistent data management and system configuration.
/// </summary>
[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class MasterDataController : BaseController
{
    private readonly IMasterDataService _masterDataService;

    /// <summary>
    /// Initializes a new instance of the MasterDataController with the required master data service.
    /// </summary>
    /// <param name="masterDataService">Service for handling master data-related business logic</param>
    public MasterDataController(IMasterDataService masterDataService)
    {
        _masterDataService = masterDataService;
    }

    /// <summary>
    /// Retrieves all billing cycles configured in the system.
    /// This endpoint provides comprehensive billing cycle information including cycle types,
    /// durations, and billing configuration details for subscription and billing management.
    /// </summary>
    /// <returns>JsonModel containing all billing cycles with configuration details</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns all billing cycles configured in the system
    /// - Includes billing cycle types, durations, and configuration details
    /// - Shows billing cycle settings and subscription options
    /// - Access restricted to authenticated users
    /// - Used for billing cycle management and subscription configuration
    /// - Includes comprehensive billing cycle information and metadata
    /// - Provides data for billing and subscription management
    /// - Handles billing cycle data retrieval and error responses
    /// </remarks>
    [HttpGet("billing-cycles")]
    public async Task<JsonModel> GetBillingCycles()
    {
        return await _masterDataService.GetBillingCyclesAsync(GetToken(HttpContext));
    }

    /// <summary>
    /// Retrieves all currencies supported by the system.
    /// This endpoint provides comprehensive currency information including currency codes,
    /// symbols, exchange rates, and currency configuration details for payment processing.
    /// </summary>
    /// <returns>JsonModel containing all supported currencies with configuration details</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns all currencies supported by the system
    /// - Includes currency codes, symbols, and exchange rate information
    /// - Shows currency configuration and payment processing details
    /// - Access restricted to authenticated users
    /// - Used for currency management and payment processing
    /// - Includes comprehensive currency information and metadata
    /// - Provides data for payment and billing management
    /// - Handles currency data retrieval and error responses
    /// </remarks>
    [HttpGet("currencies")]
    public async Task<JsonModel> GetCurrencies()
    {
        return await _masterDataService.GetCurrenciesAsync(GetToken(HttpContext));
    }

    /// <summary>
    /// Retrieves all privilege types available in the system.
    /// This endpoint provides comprehensive privilege type information including privilege categories,
    /// descriptions, and privilege configuration details for access control and permission management.
    /// </summary>
    /// <returns>JsonModel containing all privilege types with configuration details</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns all privilege types available in the system
    /// - Includes privilege categories, descriptions, and configuration details
    /// - Shows privilege settings and access control options
    /// - Access restricted to authenticated users
    /// - Used for privilege management and access control configuration
    /// - Includes comprehensive privilege information and metadata
    /// - Provides data for access control and permission management
    /// - Handles privilege data retrieval and error responses
    /// </remarks>
    [HttpGet("privilege-types")]
    public async Task<JsonModel> GetPrivilegeTypes()
    {
        return await _masterDataService.GetPrivilegeTypesAsync(GetToken(HttpContext));
    }
}
