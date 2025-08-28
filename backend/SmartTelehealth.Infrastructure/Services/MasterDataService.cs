using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Services;

public class MasterDataService : IMasterDataService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<MasterDataService> _logger;

    public MasterDataService(ApplicationDbContext context, ILogger<MasterDataService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<JsonModel> GetBillingCyclesAsync(TokenModel token)
    {
        try
        {
            var billingCycles = await _context.MasterBillingCycles
                .Where(x => x.IsActive)
                .OrderBy(x => x.SortOrder)
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Description,
                    x.IsActive,
                    x.SortOrder
                })
                .ToListAsync();

            return new JsonModel
            {
                data = billingCycles,
                Message = "Billing cycles retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving billing cycles");
            return new JsonModel
            {
                data = new object(),
                Message = "Error retrieving billing cycles",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetCurrenciesAsync(TokenModel token)
    {
        try
        {
            var currencies = await _context.MasterCurrencies
                .Where(x => x.IsActive)
                .OrderBy(x => x.SortOrder)
                .Select(x => new
                {
                    x.Id,
                    x.Code,
                    x.Name,
                    x.Symbol,
                    x.IsActive,
                    x.SortOrder
                })
                .ToListAsync();

            return new JsonModel
            {
                data = currencies,
                Message = "Currencies retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving currencies");
            return new JsonModel
            {
                data = new object(),
                Message = "Error retrieving currencies",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetPrivilegeTypesAsync(TokenModel token)
    {
        try
        {
            var privilegeTypes = await _context.MasterPrivilegeTypes
                .Where(x => x.IsActive)
                .OrderBy(x => x.SortOrder)
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Description,
                    x.IsActive,
                    x.SortOrder
                })
                .ToListAsync();

            return new JsonModel
            {
                data = privilegeTypes,
                Message = "Privilege types retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving privilege types");
            return new JsonModel
            {
                data = new object(),
                Message = "Error retrieving privilege types",
                StatusCode = 500
            };
        }
    }
}
