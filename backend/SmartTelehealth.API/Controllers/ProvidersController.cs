using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System;
using System.Threading.Tasks;

namespace SmartTelehealth.API.Controllers;

/// <summary>
/// Controller responsible for managing healthcare providers and their information.
/// This controller provides comprehensive provider management functionality including
/// creating, updating, and deleting provider records, as well as managing provider
/// profiles, services, and associated healthcare information.
/// </summary>
[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class ProvidersController : BaseController
{
    private readonly IProviderService _providerService;

    /// <summary>
    /// Initializes a new instance of the ProvidersController with the required provider service.
    /// </summary>
    /// <param name="providerService">Service for handling provider-related business logic</param>
    public ProvidersController(IProviderService providerService)
    {
        _providerService = providerService;
    }

    /// <summary>
    /// Retrieves all healthcare providers in the system.
    /// This endpoint returns a comprehensive list of all healthcare providers
    /// including their profiles, services, and associated information.
    /// </summary>
    /// <returns>JsonModel containing the list of all healthcare providers</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns all healthcare providers in the system
    /// - Includes provider profiles, services, and contact information
    /// - Shows provider status and availability
    /// - Access restricted to administrators and authorized users
    /// - Used for provider management and system administration
    /// - Provides comprehensive provider information for healthcare services
    /// - Includes provider ratings and service categories
    /// </remarks>
    [HttpGet]
    public async Task<JsonModel> GetAllProviders()
    {
        return await _providerService.GetAllProvidersAsync(GetToken(HttpContext));
    }

    /// <summary>
    /// Retrieves detailed information about a specific healthcare provider.
    /// This endpoint returns comprehensive provider details including profile information,
    /// services offered, availability, and associated healthcare data.
    /// </summary>
    /// <param name="id">The unique identifier of the healthcare provider</param>
    /// <returns>JsonModel containing the provider details or error information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns detailed provider information including profile and services
    /// - Shows provider availability and contact information
    /// - Includes provider ratings and service categories
    /// - Access restricted to administrators and authorized users
    /// - Used for provider details and healthcare service management
    /// - Provides complete provider profile and service information
    /// - Includes provider credentials and specializations
    /// </remarks>
    [HttpGet("{id}")]
    public async Task<JsonModel> GetProvider(int id)
    {
        return await _providerService.GetProviderByIdAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Creates a new healthcare provider in the system.
    /// This endpoint allows administrators to add new healthcare providers
    /// with their profiles, services, and associated information.
    /// </summary>
    /// <param name="createProviderDto">DTO containing the provider creation details</param>
    /// <returns>JsonModel containing the creation result and new provider information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Creates a new healthcare provider with specified details
    /// - Sets up provider profile, services, and contact information
    /// - Access restricted to administrators only
    /// - Used for adding new healthcare providers to the system
    /// - Includes validation of provider information and credentials
    /// - Sets up audit trails and administrative tracking
    /// - Ensures provider uniqueness and proper categorization
    /// </remarks>
    [HttpPost]
    public async Task<JsonModel> CreateProvider([FromBody] CreateProviderDto createProviderDto)
    {
        return await _providerService.CreateProviderAsync(createProviderDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Updates an existing healthcare provider with new information.
    /// This endpoint allows administrators to modify provider details including
    /// profile information, services, and associated healthcare data.
    /// </summary>
    /// <param name="id">The unique identifier of the provider to update</param>
    /// <param name="updateProviderDto">DTO containing the updated provider information</param>
    /// <returns>JsonModel containing the update result and updated provider information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Updates existing provider with new information
    /// - Modifies provider profile, services, and contact details
    /// - Access restricted to administrators only
    /// - Used for provider maintenance and information updates
    /// - Includes validation of provider changes and business impact
    /// - Maintains audit trails of all provider modifications
    /// - Handles impact on existing healthcare services and appointments
    /// </remarks>
    [HttpPut("{id}")]
    public async Task<JsonModel> UpdateProvider(int id, [FromBody] UpdateProviderDto updateProviderDto)
    {
        if (id != updateProviderDto.Id)
            return new JsonModel { data = new object(), Message = "ID mismatch", StatusCode = 400 };
        return await _providerService.UpdateProviderAsync(id, updateProviderDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Deletes a healthcare provider from the system.
    /// This endpoint removes a provider from the system, handling cleanup
    /// of associated data and ensuring no active services are affected.
    /// </summary>
    /// <param name="id">The unique identifier of the provider to delete</param>
    /// <returns>JsonModel containing the deletion result</returns>
    /// <remarks>
    /// This endpoint:
    /// - Removes the healthcare provider from the system
    /// - Validates that no active services are using the provider
    /// - Access restricted to administrators only
    /// - Used for removing obsolete or discontinued providers
    /// - Includes safety checks to prevent data loss
    /// - Maintains audit trails of provider deletion
    /// - Handles cleanup of related healthcare services and appointments
    /// </remarks>
    [HttpDelete("{id}")]
    public async Task<JsonModel> DeleteProvider(int id)
    {
        return await _providerService.DeleteProviderAsync(id, GetToken(HttpContext));
    }
} 