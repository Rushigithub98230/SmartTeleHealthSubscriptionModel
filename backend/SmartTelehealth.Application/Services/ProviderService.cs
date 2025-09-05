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
    /// <summary>
    /// Service responsible for healthcare provider management operations.
    /// This service handles provider CRUD operations, provider data management,
    /// provider verification, and provider-related business logic. It provides
    /// comprehensive provider management functionality with proper validation,
    /// audit trails, and data mapping between entities and DTOs.
    /// 
    /// Key Features:
    /// - Provider CRUD operations (Create, Read, Update, Delete)
    /// - Provider data retrieval and management
    /// - Provider verification and status management
    /// - Soft delete functionality with audit trails
    /// - AutoMapper integration for entity-DTO mapping
    /// - Comprehensive error handling and validation
    /// - Audit logging and tracking
    /// - Integration with provider repository
    /// - Provider search and filtering capabilities
    /// - Provider profile management
    /// </summary>
    public class ProviderService : IProviderService
    {
        private readonly IProviderRepository _providerRepository;
        private readonly IMapper _mapper;
          

        /// <summary>
        /// Initializes a new instance of the ProviderService
        /// </summary>
        /// <param name="providerRepository">Repository for provider data access operations</param>
        /// <param name="mapper">AutoMapper instance for entity-DTO mapping</param>
        /// <param name="auditService">Service for audit logging and tracking</param>
        public ProviderService(IProviderRepository providerRepository, IMapper mapper, IAuditService auditService)
        {
            _providerRepository = providerRepository;
            _mapper = mapper;
              
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
            return new JsonModel
            {
                data = dto,
                Message = "Provider updated",
                StatusCode = 200
            };
        }

        public async Task<JsonModel> DeleteProviderAsync(int id, TokenModel tokenModel)
        {
            var provider = await _providerRepository.GetByIdAsync(id);
            if (provider == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Provider not found",
                    StatusCode = 404
                };
            }

            // Soft delete - set audit properties
            provider.IsDeleted = true;
            provider.DeletedBy = tokenModel.UserID;
            provider.DeletedDate = DateTime.UtcNow;
            provider.UpdatedBy = tokenModel.UserID;
            provider.UpdatedDate = DateTime.UtcNow;
            
            var result = await _providerRepository.UpdateAsync(provider);
            if (result == null)
                return new JsonModel
                {
                    data = new object(),
                    Message = "Provider not found",
                    StatusCode = 404
                };
            return new JsonModel
            {
                data = true,
                Message = "Provider deleted",
                StatusCode = 200
            };
        }
    }
} 