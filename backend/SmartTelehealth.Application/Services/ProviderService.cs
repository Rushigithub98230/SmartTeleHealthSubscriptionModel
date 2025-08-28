using AutoMapper;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartTelehealth.Application.Services
{
    public class ProviderService : IProviderService
    {
        private readonly IProviderRepository _providerRepository;
        private readonly IMapper _mapper;
        private readonly IAuditService _auditService;

        public ProviderService(IProviderRepository providerRepository, IMapper mapper, IAuditService auditService)
        {
            _providerRepository = providerRepository;
            _mapper = mapper;
            _auditService = auditService;
        }

        public async Task<JsonModel> GetAllProvidersAsync(TokenModel tokenModel)
        {
            var providers = await _providerRepository.GetAllAsync();
            var dtos = _mapper.Map<List<ProviderDto>>(providers);
            return new JsonModel
            {
                data = dtos,
                Message = "Providers retrieved successfully",
                StatusCode = 200
            };
        }

        public async Task<JsonModel> GetProviderByIdAsync(int id, TokenModel tokenModel)
        {
            var provider = await _providerRepository.GetByIdAsync(id);
            if (provider == null)
                return new JsonModel
                {
                    data = new object(),
                    Message = "Provider not found",
                    StatusCode = 404
                };
            var dto = _mapper.Map<ProviderDto>(provider);
            return new JsonModel
            {
                data = dto,
                Message = "Provider retrieved successfully",
                StatusCode = 200
            };
        }

        public async Task<JsonModel> CreateProviderAsync(CreateProviderDto createProviderDto, TokenModel tokenModel)
        {
            var provider = _mapper.Map<Provider>(createProviderDto);
            var created = await _providerRepository.CreateAsync(provider);
            var dto = _mapper.Map<ProviderDto>(created);
            await _auditService.LogUserActionAsync(dto.Id, "ProviderCreated", "Provider", dto.Id.ToString(), $"Provider {dto.FullName} created", tokenModel);
            return new JsonModel
            {
                data = dto,
                Message = "Provider created",
                StatusCode = 201
            };
        }

        public async Task<JsonModel> UpdateProviderAsync(int id, UpdateProviderDto updateProviderDto, TokenModel tokenModel)
        {
            var existing = await _providerRepository.GetByIdAsync(id);
            if (existing == null)
                return new JsonModel
                {
                    data = new object(),
                    Message = "Provider not found",
                    StatusCode = 404
                };
            _mapper.Map(updateProviderDto, existing);
            var updated = await _providerRepository.UpdateAsync(existing);
            var dto = _mapper.Map<ProviderDto>(updated);
            await _auditService.LogUserActionAsync(dto.Id, "ProviderUpdated", "Provider", dto.Id.ToString(), $"Provider {dto.FullName} updated", tokenModel);
            return new JsonModel
            {
                data = dto,
                Message = "Provider updated",
                StatusCode = 200
            };
        }

        public async Task<JsonModel> DeleteProviderAsync(int id, TokenModel tokenModel)
        {
            var result = await _providerRepository.DeleteAsync(id);
            if (!result)
                return new JsonModel
                {
                    data = new object(),
                    Message = "Provider not found",
                    StatusCode = 404
                };
            await _auditService.LogUserActionAsync(id, "ProviderDeleted", "Provider", id.ToString(), $"Provider {id} deleted", tokenModel);
            return new JsonModel
            {
                data = true,
                Message = "Provider deleted",
                StatusCode = 200
            };
        }
    }
} 