using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IMasterDataService
{
    Task<JsonModel> GetBillingCyclesAsync(TokenModel token);
    Task<JsonModel> GetCurrenciesAsync(TokenModel token);
    Task<JsonModel> GetPrivilegeTypesAsync(TokenModel token);
}
