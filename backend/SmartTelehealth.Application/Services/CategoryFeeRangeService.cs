using AutoMapper;
using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;

namespace SmartTelehealth.Application.Services;

/// <summary>
/// Service responsible for managing category fee ranges.
/// This service handles fee range creation, updates, and retrieval for different healthcare categories.
/// </summary>
public class CategoryFeeRangeService : ICategoryFeeRangeService
{
    private readonly ICategoryFeeRangeRepository _feeRangeRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CategoryFeeRangeService> _logger;

    public CategoryFeeRangeService(
        ICategoryFeeRangeRepository feeRangeRepository,
        ICategoryRepository categoryRepository,
        IMapper mapper,
        ILogger<CategoryFeeRangeService> logger)
    {
        _feeRangeRepository = feeRangeRepository;
        _categoryRepository = categoryRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<JsonModel> CreateFeeRangeAsync(CreateCategoryFeeRangeDto createDto, TokenModel tokenModel)
    {
        try
        {
            // Validate category exists
            var category = await _categoryRepository.GetByIdAsync(createDto.CategoryId);
            if (category == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Category not found",
                    StatusCode = 404
                };
            }

            // Check if fee range already exists for this category
            var existingFeeRange = await _feeRangeRepository.GetByCategoryAsync(createDto.CategoryId);
            if (existingFeeRange != null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Fee range already exists for this category",
                    StatusCode = 400
                };
            }

            // Validate min/max fee
            if (createDto.MinimumFee >= createDto.MaximumFee)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Minimum fee must be less than maximum fee",
                    StatusCode = 400
                };
            }

            var feeRange = new CategoryFeeRange
            {
                Id = Guid.NewGuid(),
                CategoryId = createDto.CategoryId,
                MinimumFee = createDto.MinimumFee,
                MaximumFee = createDto.MaximumFee,
                PlatformCommission = createDto.PlatformCommission,
                Description = createDto.Description,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = tokenModel.UserID
            };

            await _feeRangeRepository.AddAsync(feeRange);

            return new JsonModel
            {
                data = _mapper.Map<CategoryFeeRangeDto>(feeRange),
                Message = "Fee range created successfully",
                StatusCode = 201
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category fee range");
            return new JsonModel
            {
                data = new object(),
                Message = "Error creating fee range",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetFeeRangeAsync(Guid id, TokenModel tokenModel)
    {
        try
        {
            var feeRange = await _feeRangeRepository.GetByIdAsync(id);
            if (feeRange == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Fee range not found",
                    StatusCode = 404
                };
            }

            return new JsonModel
            {
                data = _mapper.Map<CategoryFeeRangeDto>(feeRange),
                Message = "Fee range retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving fee range {Id}", id);
            return new JsonModel
            {
                data = new object(),
                Message = "Error retrieving fee range",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetFeeRangeByCategoryAsync(Guid categoryId, TokenModel tokenModel)
    {
        try
        {
            var feeRange = await _feeRangeRepository.GetByCategoryAsync(categoryId);
            if (feeRange == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Fee range not found for this category",
                    StatusCode = 404
                };
            }

            return new JsonModel
            {
                data = _mapper.Map<CategoryFeeRangeDto>(feeRange),
                Message = "Fee range retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving fee range for category {CategoryId}", categoryId);
            return new JsonModel
            {
                data = new object(),
                Message = "Error retrieving fee range",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> UpdateFeeRangeAsync(Guid id, UpdateCategoryFeeRangeDto updateDto, TokenModel tokenModel)
    {
        try
        {
            var feeRange = await _feeRangeRepository.GetByIdAsync(id);
            if (feeRange == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Fee range not found",
                    StatusCode = 404
                };
            }

            // Validate min/max fee
            if (updateDto.MinimumFee >= updateDto.MaximumFee)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Minimum fee must be less than maximum fee",
                    StatusCode = 400
                };
            }

            feeRange.MinimumFee = updateDto.MinimumFee;
            feeRange.MaximumFee = updateDto.MaximumFee;
            feeRange.PlatformCommission = updateDto.PlatformCommission;
            feeRange.Description = updateDto.Description;
            feeRange.UpdatedDate = DateTime.UtcNow;
            feeRange.UpdatedBy = tokenModel.UserID;

            await _feeRangeRepository.UpdateAsync(feeRange);

            return new JsonModel
            {
                data = _mapper.Map<CategoryFeeRangeDto>(feeRange),
                Message = "Fee range updated successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating fee range {Id}", id);
            return new JsonModel
            {
                data = new object(),
                Message = "Error updating fee range",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetAllFeeRangesAsync(TokenModel tokenModel)
    {
        try
        {
            var feeRanges = await _feeRangeRepository.GetAllAsync();
            var feeRangeDtos = _mapper.Map<IEnumerable<CategoryFeeRangeDto>>(feeRanges);

            return new JsonModel
            {
                data = feeRangeDtos,
                Message = "Fee ranges retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all fee ranges");
            return new JsonModel
            {
                data = new object(),
                Message = "Error retrieving fee ranges",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> DeleteFeeRangeAsync(Guid id, TokenModel tokenModel)
    {
        try
        {
            var feeRange = await _feeRangeRepository.GetByIdAsync(id);
            if (feeRange == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Fee range not found",
                    StatusCode = 404
                };
            }

            await _feeRangeRepository.DeleteAsync(feeRange);

            return new JsonModel
            {
                data = new object(),
                Message = "Fee range deleted successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting fee range {Id}", id);
            return new JsonModel
            {
                data = new object(),
                Message = "Error deleting fee range",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetFeeRangeStatisticsAsync(TokenModel tokenModel)
    {
        try
        {
            var feeRanges = await _feeRangeRepository.GetAllAsync();
            var feeRangesList = feeRanges.ToList();

            var statistics = new FeeRangeStatisticsDto
            {
                TotalFeeRanges = feeRangesList.Count,
                ActiveFeeRanges = feeRangesList.Count(fr => !fr.IsDeleted),
                AverageMinimumFee = feeRangesList.Any() ? feeRangesList.Average(fr => fr.MinimumFee) : 0,
                AverageMaximumFee = feeRangesList.Any() ? feeRangesList.Average(fr => fr.MaximumFee) : 0,
                AveragePlatformCommission = feeRangesList.Any() ? feeRangesList.Average(fr => fr.PlatformCommission) : 0
            };

            return new JsonModel
            {
                data = statistics,
                Message = "Fee range statistics retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving fee range statistics");
            return new JsonModel
            {
                data = new object(),
                Message = "Error retrieving fee range statistics",
                StatusCode = 500
            };
        }
    }
}

