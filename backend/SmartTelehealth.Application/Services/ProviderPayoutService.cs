using AutoMapper;
using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartTelehealth.Application.Services
{
    public class ProviderPayoutService : IProviderPayoutService
    {
        private readonly IProviderPayoutRepository _providerPayoutRepository;
        private readonly IProviderRepository _providerRepository;
          
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;
        private readonly ILogger<ProviderPayoutService> _logger;
        public ProviderPayoutService(
            IProviderPayoutRepository providerPayoutRepository,
            IProviderRepository providerRepository,
              
            INotificationService notificationService,
            IMapper mapper,
            ILogger<ProviderPayoutService> logger)
        {
            _providerPayoutRepository = providerPayoutRepository;
            _providerRepository = providerRepository;
              
            _notificationService = notificationService;
            _mapper = mapper;
            _logger = logger;
        }
        public async Task<JsonModel> GetPayoutByIdAsync(Guid id, TokenModel tokenModel)
        {
            try
            {
                var payout = await _providerPayoutRepository.GetByIdAsync(id);
                if (payout == null)
                {
                    return new JsonModel
                    {
                        data = new object(),
                        Message = "Payout not found",
                        StatusCode = 404
                    };
                }
                var payoutDto = _mapper.Map<ProviderPayoutDto>(payout);
                return new JsonModel
                {
                    data = payoutDto,
                    Message = "Payout retrieved successfully",
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving provider payout");
                return new JsonModel
                {
                    data = new object(),
                    Message = "Failed to retrieve payout",
                    StatusCode = 500
                };
            }
        }
        public async Task<JsonModel> GetPayoutAsync(Guid id, TokenModel tokenModel) => throw new NotImplementedException();
        public async Task<JsonModel> ProcessPayoutAsync(Guid id, ProcessPayoutDto processDto, TokenModel tokenModel) => throw new NotImplementedException();
        public async Task<JsonModel> GetPayoutsByProviderAsync(int providerId, TokenModel tokenModel) => throw new NotImplementedException();
        public async Task<JsonModel> GetPayoutsByPeriodAsync(Guid periodId, TokenModel tokenModel) => throw new NotImplementedException();
        public async Task<JsonModel> GetAllPayoutsAsync(string status, int page, int pageSize, TokenModel tokenModel)
        {
            try
            {
                IEnumerable<ProviderPayout> payouts;
                if (!string.IsNullOrEmpty(status))
                {
                    payouts = await _providerPayoutRepository.GetByStatusWithPaginationAsync(status, page, pageSize);
                }
                else
                {
                    payouts = await _providerPayoutRepository.GetAllAsync();
                    // Optionally page manually if needed
                    payouts = payouts.Skip((page - 1) * pageSize).Take(pageSize);
                }
                var payoutDtos = _mapper.Map<IEnumerable<ProviderPayoutDto>>(payouts);
                return new JsonModel
                {
                    data = payoutDtos,
                    Message = "Payouts retrieved successfully",
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payouts");
                return new JsonModel
                {
                    data = new object(),
                    Message = "Failed to retrieve payouts",
                    StatusCode = 500
                };
            }
        }
        public async Task<JsonModel> GetPendingPayoutsAsync(TokenModel tokenModel)
        {
            try
            {
                var payouts = await _providerPayoutRepository.GetPendingPayoutsAsync();
                var payoutDtos = _mapper.Map<IEnumerable<ProviderPayoutDto>>(payouts);
                return new JsonModel
                {
                    data = payoutDtos,
                    Message = "Pending payouts retrieved successfully",
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending payouts");
                return new JsonModel
                {
                    data = new object(),
                    Message = "Failed to retrieve pending payouts",
                    StatusCode = 500
                };
            }
        }
        public async Task<JsonModel> GetPayoutsByStatusAsync(string status, TokenModel tokenModel)
        {
            try
            {
                var payouts = await _providerPayoutRepository.GetByStatusAsync(status);
                var payoutDtos = _mapper.Map<IEnumerable<ProviderPayoutDto>>(payouts);
                return new JsonModel
                {
                    data = payoutDtos,
                    Message = $"Payouts with status {status} retrieved successfully",
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payouts by status");
                return new JsonModel
                {
                    data = new object(),
                    Message = "Failed to retrieve payouts by status",
                    StatusCode = 500
                };
            }
        }
        public async Task<JsonModel> GetProviderEarningsAsync(int providerId, TokenModel tokenModel)
        {
            try
            {
                var provider = await _providerRepository.GetByIdAsync(providerId);
                if (provider == null)
                {
                    return new JsonModel
                    {
                        data = new object(),
                        Message = "Provider not found",
                        StatusCode = 404
                    };
                }
                var totalEarnings = await _providerPayoutRepository.GetTotalPayoutAmountByProviderAsync(providerId);
                var pendingEarnings = await _providerPayoutRepository.GetPendingPayoutAmountByProviderAsync(providerId);
                var payoutCount = await _providerPayoutRepository.GetPayoutCountByProviderAsync(providerId);
                var earnings = new ProviderEarningsDto
                {
                    ProviderId = providerId,
                    ProviderName = $"{provider.FirstName} {provider.LastName}",
                    TotalEarnings = totalEarnings,
                    PendingEarnings = pendingEarnings,
                    ProcessedEarnings = totalEarnings,
                    TotalConsultations = 0, // TODO: Get from consultation service
                    AveragePerConsultation = payoutCount > 0 ? totalEarnings / payoutCount : 0,
                    LastPayoutDate = null, // TODO: Get from payout history
                    NextPayoutDate = null // TODO: Calculate based on payout schedule
                };
                return new JsonModel
                {
                    data = earnings,
                    Message = "Provider earnings retrieved successfully",
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving provider earnings");
                return new JsonModel
                {
                    data = new object(),
                    Message = "Failed to retrieve provider earnings",
                    StatusCode = 500
                };
            }
        }
        public async Task<JsonModel> GetPayoutStatisticsAsync(TokenModel tokenModel)
        {
            try
            {
                var statistics = await _providerPayoutRepository.GetPayoutStatisticsAsync();
                var statisticsDto = _mapper.Map<PayoutStatisticsDto>(statistics);
                return new JsonModel
                {
                    data = statisticsDto,
                    Message = "Payout statistics retrieved successfully",
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payout statistics");
                return new JsonModel
                {
                    data = new object(),
                    Message = "Failed to retrieve payout statistics",
                    StatusCode = 500
                };
            }
        }
        public async Task<JsonModel> ProcessAllPendingPayoutsAsync(TokenModel tokenModel)
        {
            try
            {
                var pendingPayouts = await _providerPayoutRepository.GetPendingPayoutsAsync();
                int processedCount = 0;
                foreach (var payout in pendingPayouts)
                {
                    try
                    {
                        var processDto = new ProcessPayoutDto
                        {
                            Status = "Completed",
                            PaymentReference = $"PAY-{DateTime.UtcNow:yyyyMMdd}-{payout.Id}",
                            Notes = "Automatically processed"
                        };
                        await ProcessPayoutAsync(payout.Id, processDto, tokenModel);
                        processedCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error processing payout {payout.Id}");
                    }
                }
                return new JsonModel
                {
                    data = true,
                    Message = $"Processed {processedCount} pending payouts",
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing all pending payouts");
                return new JsonModel
                {
                    data = new object(),
                    Message = "Failed to process pending payouts",
                    StatusCode = 500
                };
            }
        }
        public async Task<JsonModel> GeneratePayoutsForPeriodAsync(Guid periodId, TokenModel tokenModel) => throw new NotImplementedException();
        public async Task<JsonModel> CreatePeriodAsync(CreatePayoutPeriodDto createDto, TokenModel tokenModel)
        {
            try
            {
                var period = new PayoutPeriod
                {
                    Name = createDto.Name,
                    StartDate = createDto.StartDate,
                    EndDate = createDto.EndDate,
                    Status = PayoutPeriodStatus.Open,
                    // Set audit properties for creation
                    IsActive = true,
                    CreatedBy = tokenModel.UserID,
                    CreatedDate = DateTime.UtcNow
                };
                var createdPeriod = await _providerPayoutRepository.AddPeriodAsync();
                var periodDto = _mapper.Map<PayoutPeriodDto>(createdPeriod);
                return new JsonModel
                {
                    data = periodDto,
                    Message = "Payout period created successfully",
                    StatusCode = 201
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payout period");
                return new JsonModel
                {
                    data = new object(),
                    Message = "Failed to create payout period",
                    StatusCode = 500
                };
            }
        }
        public async Task<JsonModel> GetAllPeriodsAsync(TokenModel tokenModel)
        {
            try
            {
                var periods = await _providerPayoutRepository.GetAllPeriodsAsync();
                var periodDtos = _mapper.Map<IEnumerable<PayoutPeriodDto>>(periods);
                return new JsonModel
                {
                    data = periodDtos,
                    Message = "Payout periods retrieved successfully",
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payout periods");
                return new JsonModel
                {
                    data = new object(),
                    Message = "Failed to retrieve payout periods",
                    StatusCode = 500
                };
            }
        }
    }
} 