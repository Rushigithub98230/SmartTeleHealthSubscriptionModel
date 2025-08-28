using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using System.Linq.Expressions;

namespace SmartTelehealth.Infrastructure.Services;

public class DocumentTypeService : IDocumentTypeService
{
    private readonly IGenericRepository<DocumentType> _documentTypeRepository;
    private readonly IGenericRepository<Document> _documentRepository;
    private readonly ILogger<DocumentTypeService> _logger;

    public DocumentTypeService(
        IGenericRepository<DocumentType> documentTypeRepository,
        IGenericRepository<Document> documentRepository,
        ILogger<DocumentTypeService> logger)
    {
        _documentTypeRepository = documentTypeRepository;
        _documentRepository = documentRepository;
        _logger = logger;
    }

    public async Task<JsonModel> CreateDocumentTypeAsync(CreateDocumentTypeRequest request)
    {
        try
        {
            // Check if document type with same name already exists
            var existingType = await _documentTypeRepository.FindAsync(dt =>
                dt.Name.ToLower() == request.Name.ToLower() && !dt.IsDeleted);
            
            if (existingType.Any())
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Document type with this name already exists",
                    StatusCode = 400
                };
            }

            var documentType = new DocumentType
            {
                Name = request.Name,
                Description = request.Description,
                IsActive = request.IsActive,
                IsSystemDefined = request.IsSystemDefined,
                AllowedExtensions = request.AllowedExtensions,
                MaxFileSizeBytes = request.MaxFileSizeBytes,
                RequireFileValidation = request.RequireFileValidation,
                Icon = request.Icon,
                Color = request.Color,
                DisplayOrder = request.DisplayOrder,
                CreatedBy = (int?)request.CreatedById.GetHashCode(), // Temporary fix - convert Guid to int
                CreatedDate = DateTime.UtcNow
            };

            await _documentTypeRepository.AddAsync(documentType);
            await _documentTypeRepository.SaveChangesAsync();

            return await GetDocumentTypeAsync(documentType.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating document type {Name}", request.Name);
            return new JsonModel
            {
                data = new object(),
                Message = "Internal server error",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetDocumentTypeAsync(Guid documentTypeId)
    {
        try
        {
            var documentType = await _documentTypeRepository.GetByIdAsync(documentTypeId);
            if (documentType == null || documentType.IsDeleted)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Document type not found",
                    StatusCode = 404
                };
            }

            // Get document count for this type
            var documentCount = await _documentRepository.FindAsync(d =>
                d.DocumentTypeId == documentTypeId && !d.IsDeleted);
            var count = documentCount.Count();

            return new JsonModel
            {
                data = MapToDto(documentType, count),
                Message = "Document type retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving document type {DocumentTypeId}", documentTypeId);
            return new JsonModel
            {
                data = new object(),
                Message = "Internal server error",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetByNameAsync(string name)
    {
        try
        {
            var documentType = await _documentTypeRepository.FindAsync(dt =>
                dt.Name.ToLower() == name.ToLower() && !dt.IsDeleted);
            
            var firstType = documentType.FirstOrDefault();
            if (firstType == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Document type not found",
                    StatusCode = 404
                };
            }

            // Get document count for this type
            var documentCount = await _documentRepository.FindAsync(d =>
                d.DocumentTypeId == firstType.Id && !d.IsDeleted);
            var count = documentCount.Count();

            return new JsonModel
            {
                data = MapToDto(firstType, count),
                Message = "Document type retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving document type by name {Name}", name);
            return new JsonModel
            {
                data = new object(),
                Message = "Internal server error",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> UpdateDocumentTypeAsync(Guid documentTypeId, UpdateDocumentTypeRequest request)
    {
        try
        {
            var documentType = await _documentTypeRepository.GetByIdAsync(documentTypeId);
            if (documentType == null || documentType.IsDeleted)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Document type not found",
                    StatusCode = 404
                };
            }

            // Prevent modification of system-defined types
            if (documentType.IsSystemDefined)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Cannot modify system-defined document types",
                    StatusCode = 403
                };
            }

            // Check if name is being changed and if it conflicts with existing types
            if (!string.IsNullOrEmpty(request.Name) && request.Name.ToLower() != documentType.Name.ToLower())
            {
                var existingType = await _documentTypeRepository.FindAsync(dt => 
                    dt.Name.ToLower() == request.Name.ToLower() && 
                    dt.Id != documentTypeId && 
                    !dt.IsDeleted);
                
                if (existingType.Any())
                {
                    return new JsonModel
                    {
                        data = new object(),
                        Message = "Document type with this name already exists",
                        StatusCode = 400
                    };
                }
            }

            // Update properties
            if (!string.IsNullOrEmpty(request.Name))
                documentType.Name = request.Name;
            
            if (request.Description != null)
                documentType.Description = request.Description;
            
            if (request.IsActive.HasValue)
                documentType.IsActive = request.IsActive.Value;
            
            if (request.AllowedExtensions != null)
                documentType.AllowedExtensions = request.AllowedExtensions;
            
            if (request.MaxFileSizeBytes.HasValue)
                documentType.MaxFileSizeBytes = request.MaxFileSizeBytes;
            
            if (request.RequireFileValidation.HasValue)
                documentType.RequireFileValidation = request.RequireFileValidation.Value;
            
            if (request.Icon != null)
                documentType.Icon = request.Icon;
            
            if (request.Color != null)
                documentType.Color = request.Color;
            
            if (request.DisplayOrder.HasValue)
                documentType.DisplayOrder = request.DisplayOrder.Value;
            
            documentType.UpdatedBy = (int?)request.UpdatedById.GetHashCode(); // Temporary fix - convert Guid to int
            documentType.UpdatedDate = DateTime.UtcNow;

            await _documentTypeRepository.SaveChangesAsync();

            return await GetDocumentTypeAsync(documentTypeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating document type {DocumentTypeId}", documentTypeId);
            return new JsonModel
            {
                data = new object(),
                Message = "Internal server error",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> DeleteDocumentTypeAsync(Guid documentTypeId, Guid userId)
    {
        try
        {
            var documentType = await _documentTypeRepository.GetByIdAsync(documentTypeId);
            if (documentType == null || documentType.IsDeleted)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Document type not found",
                    StatusCode = 404
                };
            }

            // Prevent deletion of system-defined types
            if (documentType.IsSystemDefined)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Cannot delete system-defined document types",
                    StatusCode = 403
                };
            }

            // Check if there are documents using this type
            var documentsUsingType = await _documentRepository.FindAsync(d => 
                d.DocumentTypeId == documentTypeId && !d.IsDeleted);
            
            if (documentsUsingType.Any())
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Cannot delete document type that has associated documents",
                    StatusCode = 400
                };
            }

            await _documentTypeRepository.DeleteAsync(documentType);
            await _documentTypeRepository.SaveChangesAsync();

            return new JsonModel
            {
                data = true,
                Message = "Document type deleted successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document type {DocumentTypeId}", documentTypeId);
            return new JsonModel
            {
                data = new object(),
                Message = "Internal server error",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> SoftDeleteDocumentTypeAsync(Guid documentTypeId, Guid userId)
    {
        try
        {
            var documentType = await _documentTypeRepository.GetByIdAsync(documentTypeId);
            if (documentType == null || documentType.IsDeleted)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Document type not found",
                    StatusCode = 404
                };
            }

            // Prevent soft deletion of system-defined types
            if (documentType.IsSystemDefined)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Cannot delete system-defined document types",
                    StatusCode = 403
                };
            }

            documentType.IsDeleted = true;
            documentType.DeletedBy = (int?)userId.GetHashCode(); // Temporary fix - convert Guid to int
            documentType.DeletedDate = DateTime.UtcNow;
            documentType.UpdatedDate = DateTime.UtcNow;

            await _documentTypeRepository.SaveChangesAsync();

            return new JsonModel
            {
                data = true,
                Message = "Document type soft deleted successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error soft deleting document type {DocumentTypeId}", documentTypeId);
            return new JsonModel
            {
                data = new object(),
                Message = "Internal server error",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetAllDocumentTypesAsync(bool? isActive = null)
    {
        try
        {
            Expression<Func<DocumentType, bool>> filter = dt => !dt.IsDeleted;
            
            if (isActive.HasValue)
            {
                filter = dt => !dt.IsDeleted && dt.IsActive == isActive.Value;
            }

            var documentTypes = await _documentTypeRepository.FindAsync(filter);
            var documentTypeDtos = new List<DocumentTypeDto>();

            foreach (var docType in documentTypes.OrderBy(dt => dt.DisplayOrder).ThenBy(dt => dt.Name))
            {
                var documentCount = await _documentRepository.FindAsync(d =>
                    d.DocumentTypeId == docType.Id && !d.IsDeleted);
                var count = documentCount.Count();

                documentTypeDtos.Add(MapToDto(docType, count));
            }

            return new JsonModel
            {
                data = documentTypeDtos,
                Message = "Document types retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all document types");
            return new JsonModel
            {
                data = new object(),
                Message = "Internal server error",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> SearchDocumentTypesAsync(DocumentTypeSearchRequest request)
    {
        try
        {
            Expression<Func<DocumentType, bool>> filter = dt => !dt.IsDeleted;

            if (!string.IsNullOrEmpty(request.Name))
            {
                filter = dt => !dt.IsDeleted && dt.Name.ToLower().Contains(request.Name.ToLower());
            }

            if (request.IsActive.HasValue)
            {
                filter = dt => !dt.IsDeleted && dt.IsActive == request.IsActive.Value;
            }

            if (request.IsSystemDefined.HasValue)
            {
                filter = dt => !dt.IsDeleted && dt.IsSystemDefined == request.IsSystemDefined.Value;
            }

            if (request.CreatedFrom.HasValue)
            {
                filter = dt => !dt.IsDeleted && dt.CreatedDate >= request.CreatedFrom.Value;
            }

            if (request.CreatedTo.HasValue)
            {
                filter = dt => !dt.IsDeleted && dt.CreatedDate <= request.CreatedTo.Value;
            }

            var documentTypes = await _documentTypeRepository.FindAsync(filter);
            var documentTypeDtos = new List<DocumentTypeDto>();

            foreach (var docType in documentTypes.OrderBy(dt => dt.DisplayOrder).ThenBy(dt => dt.Name))
            {
                var documentCount = await _documentRepository.FindAsync(d =>
                    d.DocumentTypeId == docType.Id && !d.IsDeleted);
                var count = documentCount.Count();

                documentTypeDtos.Add(MapToDto(docType, count));
            }

            // Apply pagination
            var pagedResults = documentTypeDtos
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return new JsonModel
            {
                data = pagedResults,
                Message = "Document types search completed successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching document types");
            return new JsonModel
            {
                data = new object(),
                Message = "Internal server error",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetActiveDocumentTypesAsync()
    {
        return await GetAllDocumentTypesAsync(true);
    }

    public async Task<JsonModel> GetSystemDocumentTypesAsync()
    {
        try
        {
            var systemTypes = await _documentTypeRepository.FindAsync(dt => 
                !dt.IsDeleted && dt.IsSystemDefined && dt.IsActive);
            
            var documentTypeDtos = new List<DocumentTypeDto>();
            foreach (var docType in systemTypes.OrderBy(dt => dt.DisplayOrder).ThenBy(dt => dt.Name))
            {
                var documentCount = await _documentRepository.FindAsync(d =>
                    d.DocumentTypeId == docType.Id && !d.IsDeleted);
                var count = documentCount.Count();
                documentTypeDtos.Add(MapToDto(docType, count));
            }

            return new JsonModel
            {
                data = documentTypeDtos,
                Message = "System document types retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting system document types");
            return new JsonModel
            {
                data = new object(),
                Message = "Internal server error",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetAdminDocumentTypesAsync()
    {
        try
        {
            var adminTypes = await _documentTypeRepository.FindAsync(dt => 
                !dt.IsDeleted && !dt.IsSystemDefined && dt.IsActive);
            
            var documentTypeDtos = new List<DocumentTypeDto>();
            foreach (var docType in adminTypes.OrderBy(dt => dt.DisplayOrder).ThenBy(dt => dt.Name))
            {
                var documentCount = await _documentRepository.FindAsync(d =>
                    d.DocumentTypeId == docType.Id && !d.IsDeleted);
                var count = documentCount.Count();
                documentTypeDtos.Add(MapToDto(docType, count));
            }

            return new JsonModel
            {
                data = documentTypeDtos,
                Message = "Admin document types retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting admin document types");
            return new JsonModel
            {
                data = new object(),
                Message = "Internal server error",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetDocumentTypeWithStatsAsync(Guid documentTypeId)
    {
        return await GetDocumentTypeAsync(documentTypeId);
    }

    public async Task<JsonModel> GetPopularDocumentTypesAsync(int limit = 10)
    {
        try
        {
            var documentTypes = await _documentTypeRepository.FindAsync(dt => !dt.IsDeleted && dt.IsActive);
            var documentTypeStats = new List<(DocumentType Type, int Count)>();

            foreach (var docType in documentTypes)
            {
                var documentCount = await _documentRepository.FindAsync(d =>
                    d.DocumentTypeId == docType.Id && !d.IsDeleted);
                
                documentTypeStats.Add((docType, documentCount.Count()));
            }

            // Sort by document count and take top limit
            var popularTypes = documentTypeStats
                .OrderByDescending(x => x.Count)
                .Take(limit)
                .Select(x => MapToDto(x.Type, x.Count))
                .ToList();

            return new JsonModel
            {
                data = popularTypes,
                Message = "Popular document types retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting popular document types");
            return new JsonModel
            {
                data = new object(),
                Message = "Internal server error",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> ValidateDocumentTypeAsync(Guid documentTypeId)
    {
        try
        {
            var documentType = await _documentTypeRepository.GetByIdAsync(documentTypeId);
            return new JsonModel
            {
                data = documentType != null && !documentType.IsDeleted,
                Message = "Document type validation completed successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating document type {DocumentTypeId}", documentTypeId);
            return new JsonModel
            {
                data = new object(),
                Message = "Internal server error",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> IsDocumentTypeActiveAsync(Guid documentTypeId)
    {
        try
        {
            var documentType = await _documentTypeRepository.GetByIdAsync(documentTypeId);
            return new JsonModel
            {
                data = documentType != null && !documentType.IsDeleted && documentType.IsActive,
                Message = "Document type active status checked successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if document type is active {DocumentTypeId}", documentTypeId);
            return new JsonModel
            {
                data = new object(),
                Message = "Internal server error",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> ValidateFileAgainstDocumentTypeAsync(DocumentTypeValidationRequest request)
    {
        try
        {
            var documentType = await _documentTypeRepository.GetByIdAsync(request.DocumentTypeId);
            if (documentType == null || documentType.IsDeleted || !documentType.IsActive)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Document type not found or inactive",
                    StatusCode = 404
                };
            }

            var validationErrors = new List<string>();

            // Validate file extension
            if (documentType.RequireFileValidation && !string.IsNullOrEmpty(documentType.AllowedExtensions))
            {
                if (!documentType.IsValidFileExtension(request.FileName))
                {
                    validationErrors.Add($"File extension not allowed. Allowed extensions: {documentType.AllowedExtensions}");
                }
            }

            // Validate file size
            if (documentType.RequireFileValidation && documentType.MaxFileSizeBytes.HasValue)
            {
                if (!documentType.IsValidFileSize(request.FileSizeBytes))
                {
                    validationErrors.Add($"File size exceeds maximum allowed size of {documentType.GetMaxFileSizeDisplay()}");
                }
            }

            var response = new DocumentTypeValidationResponse
            {
                IsValid = validationErrors.Count == 0,
                ValidationErrors = validationErrors,
                MaxFileSizeDisplay = documentType.GetMaxFileSizeDisplay(),
                AllowedExtensions = documentType.GetAllowedExtensionsList()
            };

            return new JsonModel
            {
                data = response,
                Message = "File validation completed successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating file against document type {DocumentTypeId}", request.DocumentTypeId);
            return new JsonModel
            {
                data = new object(),
                Message = "Internal server error",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> ValidateFileExtensionAsync(Guid documentTypeId, string fileName)
    {
        try
        {
            var documentType = await _documentTypeRepository.GetByIdAsync(documentTypeId);
            if (documentType == null || documentType.IsDeleted || !documentType.IsActive)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Document type not found or inactive",
                    StatusCode = 404
                };
            }

            var isValid = documentType.IsValidFileExtension(fileName);
            return new JsonModel
            {
                data = isValid,
                Message = "File extension validation completed successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating file extension for document type {DocumentTypeId}", documentTypeId);
            return new JsonModel
            {
                data = new object(),
                Message = "Internal server error",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> ValidateFileSizeAsync(Guid documentTypeId, long fileSizeBytes)
    {
        try
        {
            var documentType = await _documentTypeRepository.GetByIdAsync(documentTypeId);
            if (documentType == null || documentType.IsDeleted || !documentType.IsActive)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Document type not found or inactive",
                    StatusCode = 404
                };
            }

            var isValid = documentType.IsValidFileSize(fileSizeBytes);
            return new JsonModel
            {
                data = isValid,
                Message = "File size validation completed successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating file size for document type {DocumentTypeId}", documentTypeId);
            return new JsonModel
            {
                data = new object(),
                Message = "Internal server error",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> ActivateDocumentTypeAsync(Guid documentTypeId, Guid userId)
    {
        try
        {
            var documentType = await _documentTypeRepository.GetByIdAsync(documentTypeId);
            if (documentType == null || documentType.IsDeleted)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Document type not found",
                    StatusCode = 404
                };
            }

            documentType.IsActive = true;
            documentType.UpdatedBy = (int?)userId.GetHashCode(); // Temporary fix - convert Guid to int
            documentType.UpdatedDate = DateTime.UtcNow;

            await _documentTypeRepository.SaveChangesAsync();

            return new JsonModel
            {
                data = true,
                Message = "Document type activated successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating document type {DocumentTypeId}", documentTypeId);
            return new JsonModel
            {
                data = new object(),
                Message = "Internal server error",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> DeactivateDocumentTypeAsync(Guid documentTypeId, Guid userId)
    {
        try
        {
            var documentType = await _documentTypeRepository.GetByIdAsync(documentTypeId);
            if (documentType == null || documentType.IsDeleted)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Document type not found",
                    StatusCode = 404
                };
            }

            documentType.IsActive = false;
            documentType.UpdatedBy = (int?)userId.GetHashCode(); // Temporary fix - convert Guid to int
            documentType.UpdatedDate = DateTime.UtcNow;

            await _documentTypeRepository.SaveChangesAsync();

            return new JsonModel
            {
                data = true,
                Message = "Document type deactivated successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating document type {DocumentTypeId}", documentTypeId);
            return new JsonModel
            {
                data = new object(),
                Message = "Internal server error",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> ActivateMultipleDocumentTypesAsync(List<Guid> documentTypeIds, Guid userId)
    {
        try
        {
            foreach (var documentTypeId in documentTypeIds)
            {
                await ActivateDocumentTypeAsync(documentTypeId, userId);
            }

            return new JsonModel
            {
                data = true,
                Message = "Multiple document types activated successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating multiple document types");
            return new JsonModel
            {
                data = new object(),
                Message = "Internal server error",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> DeactivateMultipleDocumentTypesAsync(List<Guid> documentTypeIds, Guid userId)
    {
        try
        {
            foreach (var documentTypeId in documentTypeIds)
            {
                await DeactivateDocumentTypeAsync(documentTypeId, userId);
            }

            return new JsonModel
            {
                data = true,
                Message = "Multiple document types deactivated successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating multiple document types");
            return new JsonModel
            {
                data = new object(),
                Message = "Internal server error",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> IncrementUsageCountAsync(Guid documentTypeId)
    {
        try
        {
            var documentType = await _documentTypeRepository.GetByIdAsync(documentTypeId);
            if (documentType == null || documentType.IsDeleted)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Document type not found",
                    StatusCode = 404
                };
            }

            documentType.UsageCount++;
            documentType.LastUsedAt = DateTime.UtcNow;
            documentType.UpdatedDate = DateTime.UtcNow;

            await _documentTypeRepository.SaveChangesAsync();

            return new JsonModel
            {
                data = true,
                Message = "Usage count incremented successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing usage count for document type {DocumentTypeId}", documentTypeId);
            return new JsonModel
            {
                data = new object(),
                Message = "Internal server error",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> UpdateLastUsedAtAsync(Guid documentTypeId)
    {
        try
        {
            var documentType = await _documentTypeRepository.GetByIdAsync(documentTypeId);
            if (documentType == null || documentType.IsDeleted)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Document type not found",
                    StatusCode = 404
                };
            }

            documentType.LastUsedAt = DateTime.UtcNow;
            documentType.UpdatedDate = DateTime.UtcNow;

            await _documentTypeRepository.SaveChangesAsync();

            return new JsonModel
            {
                data = true,
                Message = "Last used at updated successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating last used at for document type {DocumentTypeId}", documentTypeId);
            return new JsonModel
            {
                data = new object(),
                Message = "Internal server error",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> CreateSystemDocumentTypeAsync(CreateDocumentTypeRequest request)
    {
        try
        {
            // Force system-defined flag
            request.IsSystemDefined = true;
            request.CreatedById = Guid.Empty; // System user

            var result = await CreateDocumentTypeAsync(request);
            return new JsonModel
            {
                data = result.StatusCode == 200,
                Message = "System document type created successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating system document type {Name}", request.Name);
            return new JsonModel
            {
                data = new object(),
                Message = "Internal server error",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetDocumentTypesForEntityAsync(string entityType, bool? isActive = null)
    {
        try
        {
            // Get document types based on entity type
            var documentTypes = await _documentTypeRepository.FindAsync(dt => 
                !dt.IsDeleted && dt.IsActive && 
                (isActive == null || dt.IsActive == isActive.Value));

            var filteredTypes = new List<DocumentTypeDto>();

            foreach (var docType in documentTypes.OrderBy(dt => dt.DisplayOrder).ThenBy(dt => dt.Name))
            {
                var documentCount = await _documentRepository.FindAsync(d =>
                    d.DocumentTypeId == docType.Id && !d.IsDeleted);
                var count = documentCount.Count();

                filteredTypes.Add(MapToDto(docType, count));
            }

            return new JsonModel
            {
                data = filteredTypes,
                Message = "Document types for entity retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting document types for entity {EntityType}", entityType);
            return new JsonModel
            {
                data = new object(),
                Message = "Internal server error",
                StatusCode = 500
            };
        }
    }

    private DocumentTypeDto MapToDto(DocumentType documentType, int documentCount = 0)
    {
        return new DocumentTypeDto
        {
            DocumentTypeId = documentType.Id,
            Name = documentType.Name,
            Description = documentType.Description,
            IsSystemDefined = documentType.IsSystemDefined,
            IsActive = documentType.IsActive,
            IsDeleted = documentType.IsDeleted,
            AllowedExtensions = documentType.AllowedExtensions,
            MaxFileSizeBytes = documentType.MaxFileSizeBytes,
            RequireFileValidation = documentType.RequireFileValidation,
            Icon = documentType.Icon,
            Color = documentType.Color,
            DisplayOrder = documentType.DisplayOrder,
            UsageCount = documentType.UsageCount,
            LastUsedAt = documentType.LastUsedAt,
                            CreatedById = documentType.CreatedBy,
                            CreatedAt = documentType.CreatedDate ?? DateTime.UtcNow,
                            UpdatedAt = documentType.UpdatedDate,
                            DeletedAt = documentType.DeletedDate,
            DocumentCount = documentCount,
            MaxFileSizeDisplay = documentType.GetMaxFileSizeDisplay(),
            AllowedExtensionsList = documentType.GetAllowedExtensionsList()
        };
    }
} 