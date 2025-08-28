using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartTelehealth.Application.Interfaces
{
    public interface IProviderService
    {
        Task<JsonModel> GetAllProvidersAsync(TokenModel tokenModel);
        Task<JsonModel> GetProviderByIdAsync(int id, TokenModel tokenModel);
        Task<JsonModel> CreateProviderAsync(CreateProviderDto createProviderDto, TokenModel tokenModel);
        Task<JsonModel> UpdateProviderAsync(int id, UpdateProviderDto updateProviderDto, TokenModel tokenModel);
        Task<JsonModel> DeleteProviderAsync(int id, TokenModel tokenModel);
    }
} 