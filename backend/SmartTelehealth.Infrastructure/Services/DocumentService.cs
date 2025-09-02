using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Application.DTOs;
using System.Linq.Expressions;

namespace SmartTelehealth.Infrastructure.Services;

public class DocumentService : IDocumentService
{
    private readonly IFileStorageService _fileStorageService;
    private readonly IGenericRepository<Document> _documentRepository;
    private readonly IGenericRepository<DocumentReference> _referenceRepository;
    private readonly IGenericRepository<DocumentType> _documentTypeRepository;
    private readonly ILogger<DocumentService> _logger;
    private readonly IDocumentTypeService _documentTypeService;

    public DocumentService(
        IFileStorageService fileStorageService,
        IGenericRepository<Document> documentRepository,
        IGenericRepository<DocumentReference> referenceRepository,
        IGenericRepository<DocumentType> documentTypeRepository,
        ILogger<DocumentService> logger,
        IDocumentTypeService documentTypeService)
    {
        _fileStorageService = fileStorageService;
        _documentRepository = documentRepository;
        _referenceRepository = referenceRepository;
        _documentTypeRepository = documentTypeRepository;
        _logger = logger;
        _documentTypeService = documentTypeService;
    }

    public async Task<JsonModel> UploadDocumentAsync(UploadDocumentRequest request, TokenModel tokenModel)
    {
        try
        {
            // 1. Validate document type
            var documentType = await _documentTypeRepository.GetByIdAsync(request.DocumentTypeId);
            if (documentType == null || documentType.IsDeleted || !documentType.IsActive)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Invalid or inactive document type",
                    StatusCode = 400
                };
            }

            // 2. Validate file against document type rules
            var validationRequest = new DocumentTypeValidationRequest
            {
                DocumentTypeId = request.DocumentTypeId,
                FileName = request.FileName,
                FileSizeBytes = request.FileData.Length
            };

            var validationResult = await _documentTypeService.ValidateFileAgainstDocumentTypeAsync(validationRequest);
            if (validationResult.StatusCode != 200)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Document type validation failed",
                    StatusCode = 400
                };
            }

            if (validationResult.data is not DocumentTypeValidationResponse validationData || !validationData.IsValid)
            {
                var errorMessage = validationResult.data is DocumentTypeValidationResponse vd ? 
                    string.Join("; ", vd.ValidationErrors) : "Unknown validation error";
                return new JsonModel
                {
                    data = new object(),
                    Message = $"File validation failed: {errorMessage}",
                    StatusCode = 400
                };
            }

            // 3. Upload file to storage
            var uploadResult = await _fileStorageService.UploadFileAsync(request.FileData, request.FileName, request.ContentType, tokenModel);
            if (uploadResult.StatusCode != 200)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = uploadResult.Message ?? "File upload failed",
                    StatusCode = uploadResult.StatusCode
                };
            }

            // 4. Create document record
            var uploadPath = uploadResult.data?.ToString() ?? string.Empty;
            var document = new Document
            {
                OriginalName = request.FileName,
                UniqueName = Path.GetFileName(uploadPath), // Assuming UniqueName is the uploaded file name
                FilePath = uploadPath,
                FolderPath = Path.GetDirectoryName(uploadPath) ?? "",
                ContentType = request.ContentType,
                FileSize = request.FileData.Length,
                DocumentTypeId = request.DocumentTypeId, // Storing DocumentTypeId
                DocumentCategory = request.EntityType.ToLower(), // For backward compatibility
                Description = request.Description,
                IsEncrypted = request.IsEncrypted,
                IsPublic = request.IsPublic,
                CreatedBy = request.CreatedById ?? 0, // Ensure CreatedBy is set
                CreatedDate = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false
            };

            await _documentRepository.AddAsync(document);
            await _documentRepository.SaveChangesAsync();

            // 5. Create document reference
            var reference = new DocumentReference
            {
                DocumentId = document.Id,
                EntityType = request.EntityType,
                EntityId = request.EntityId,
                ReferenceType = request.ReferenceType,
                IsPublic = request.IsPublic,
                ExpiresAt = request.ExpiresAt,
                CreatedBy = request.CreatedById ?? 0,
                CreatedDate = DateTime.UtcNow
            };

            await _referenceRepository.AddAsync(reference);
            await _referenceRepository.SaveChangesAsync();

            // 6. Update document type usage statistics
            await _documentTypeService.IncrementUsageCountAsync(request.DocumentTypeId);

            // 7. Return document DTO with document type information
            return new JsonModel
            {
                data = new DocumentDto
            {
                DocumentId = document.Id,
                OriginalName = document.OriginalName,
                UniqueName = document.UniqueName,
                FilePath = document.FilePath,
                FolderPath = document.FolderPath,
                ContentType = document.ContentType,
                FileSize = document.FileSize,
                Description = document.Description,
                DocumentTypeId = document.DocumentTypeId,
                DocumentType = new DocumentTypeDto // Include DocumentTypeDto
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
                     CreatedDate = documentType.CreatedDate ?? DateTime.UtcNow,
                    UpdatedDate = documentType.UpdatedDate,
                    DeletedDate = documentType.DeletedDate,
                    MaxFileSizeDisplay = documentType.GetMaxFileSizeDisplay(),
                    AllowedExtensionsList = documentType.GetAllowedExtensionsList()
                },
                DocumentCategory = document.DocumentCategory,
                IsEncrypted = document.IsEncrypted,
                IsPublic = document.IsPublic,
                CreatedById = document.CreatedBy,
                CreatedDate = document.CreatedDate ?? DateTime.UtcNow,
                DeletedDate = document.DeletedDate,
                IsActive = document.IsActive,
                IsDeleted = document.IsDeleted,
                DownloadUrl = (await _fileStorageService.GetFileUrlAsync(document.FilePath, tokenModel)).data?.ToString() ?? "",
                SecureUrl = (await _fileStorageService.GetSecureUrlAsync(document.FilePath, null, tokenModel)).data?.ToString() ?? "",
                References = new List<DocumentReferenceDto> { new DocumentReferenceDto { DocumentId = reference.DocumentId, EntityType = reference.EntityType, EntityId = reference.EntityId, ReferenceType = reference.ReferenceType } }
            },
            Message = "Document uploaded successfully",
            StatusCode = 200
        };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading document {FileName}", request.FileName);
            return new JsonModel
            {
                data = new object(),
                Message = "Internal server error",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetDocumentAsync(Guid documentId, int? userId, TokenModel tokenModel)
    {
        try
        {
            // 1. Get document
            var document = await _documentRepository.GetByIdAsync(documentId);
            if (document == null || document.IsDeleted)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Document not found",
                    StatusCode = 404
                };
            }

            // 2. Check access permissions
            if (!document.IsPublic && userId.HasValue)
            {
                var hasAccess = await ValidateDocumentAccessAsync(documentId, userId.Value, tokenModel);
                if (hasAccess.StatusCode != 200 || !(hasAccess.data is bool accessGranted && accessGranted))
                {
                    return new JsonModel
                    {
                        data = new object(),
                        Message = "Access denied",
                        StatusCode = 403
                    };
                }
            }

            // 3. Get document type
            var documentType = await _documentTypeRepository.GetByIdAsync(document.DocumentTypeId);
            var documentTypeDto = documentType != null ? new DocumentTypeDto
            {
                DocumentTypeId = documentType.Id,
                Name = documentType.Name,
                Description = documentType.Description,
                IsActive = documentType.IsActive,
                IsDeleted = documentType.IsDeleted,
                CreatedById = documentType.CreatedBy,
                CreatedDate = documentType.CreatedDate ?? DateTime.UtcNow,
                UpdatedDate = documentType.UpdatedDate,
                DeletedDate = documentType.DeletedDate
            } : null;

            // 4. Get references
            var references = await _referenceRepository.FindAsync(r => r.DocumentId == documentId && !r.IsDeleted);

            // 5. Return document DTO
            return new JsonModel
            {
                data = new DocumentDto
            {
                DocumentId = document.Id,
                OriginalName = document.OriginalName,
                UniqueName = document.UniqueName,
                FilePath = document.FilePath,
                FolderPath = document.FolderPath,
                ContentType = document.ContentType,
                FileSize = document.FileSize,
                Description = document.Description,
                DocumentTypeId = document.DocumentTypeId,
                DocumentType = documentTypeDto,
                DocumentCategory = document.DocumentCategory,
                IsEncrypted = document.IsEncrypted,
                IsPublic = document.IsPublic,
                CreatedById = document.CreatedBy,
                CreatedDate = document.CreatedDate ?? DateTime.UtcNow,
                DeletedDate = document.DeletedDate,
                IsActive = document.IsActive,
                IsDeleted = document.IsDeleted,
                References = references.Select(r => new DocumentReferenceDto
                {
                    Id = r.Id,
                    DocumentId = r.DocumentId,
                    EntityType = r.EntityType,
                    EntityId = r.EntityId,
                    ReferenceType = r.ReferenceType,
                    Description = r.Description,
                    IsPublic = r.IsPublic,
                    ExpiresAt = r.ExpiresAt,
                    CreatedById = r.CreatedBy ?? 0,
                    CreatedDate = r.CreatedDate ?? DateTime.UtcNow
                }).ToList()
            },
            Message = "Document retrieved successfully",
            StatusCode = 200
        };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving document {DocumentId}", documentId);
            return new JsonModel
            {
                data = new object(),
                Message = "Internal server error",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetDocumentWithContentAsync(Guid documentId, int? userId, TokenModel tokenModel)
    {
        try
        {
            // 1. Get document metadata
            var documentResult = await GetDocumentAsync(documentId, userId, tokenModel);
            if (documentResult.StatusCode != 200)
            {
                return documentResult;
            }

            // 2. Get file content
            var documentDto = documentResult.data as DocumentDto;
            if (documentDto == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Invalid document data",
                    StatusCode = 500
                };
            }

            var fileBytes = await _fileStorageService.DownloadFileAsync(documentDto.FilePath, tokenModel);
            if (fileBytes.StatusCode != 200)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Failed to retrieve file content",
                    StatusCode = 500
                };
            }

            // 3. Add content to DTO
            documentDto.Content = fileBytes.data as byte[] ?? new byte[0];
            return documentResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving document content {DocumentId}", documentId);
            return new JsonModel
            {
                data = new object(),
                Message = "Internal server error",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> DeleteDocumentAsync(Guid documentId, int userId, TokenModel tokenModel)
    {
        try
        {
            // 1. Get document
            var document = await _documentRepository.GetByIdAsync(documentId);
            if (document == null || document.IsDeleted)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Document not found",
                    StatusCode = 404
                };
            }

            // 2. Check access permissions
            var hasAccess = await ValidateDocumentAccessAsync(documentId, userId, tokenModel);
            if (hasAccess.StatusCode != 200 || !(hasAccess.data is bool accessGranted && accessGranted))
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Access denied",
                    StatusCode = 403
                };
            }

            // 3. Delete from storage
            var deleteResult = await _fileStorageService.DeleteFileAsync(document.FilePath, tokenModel);
            if (deleteResult.StatusCode != 200)
            {
                _logger.LogWarning("Failed to delete file from storage: {FilePath}", document.FilePath);
            }

            // 4. Delete references
            var references = await _referenceRepository.FindAsync(r => r.DocumentId == documentId);
            foreach (var reference in references)
            {
                reference.IsDeleted = true;
                reference.UpdatedDate = DateTime.UtcNow;
            }
            await _referenceRepository.SaveChangesAsync();

            // 5. Delete document
            await _documentRepository.DeleteAsync(document);
            await _referenceRepository.SaveChangesAsync();

            return new JsonModel
            {
                data = true,
                Message = "Document deleted successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document {DocumentId}", documentId);
            return new JsonModel
            {
                data = new object(),
                Message = "Internal server error",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> SoftDeleteDocumentAsync(Guid documentId, int userId, TokenModel tokenModel)
    {
        try
        {
            // 1. Get document
            var document = await _documentRepository.GetByIdAsync(documentId);
            if (document == null || document.IsDeleted)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Document not found",
                    StatusCode = 404
                };
            }

            // 2. Check access permissions
            var hasAccess = await ValidateDocumentAccessAsync(documentId, userId, tokenModel);
            if (hasAccess.StatusCode != 200 || !(hasAccess.data is bool accessGranted && accessGranted))
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Access denied",
                    StatusCode = 403
                };
            }

            // 3. Soft delete document
            document.IsDeleted = true;
            document.DeletedBy = userId;
            document.DeletedDate = DateTime.UtcNow;
            document.UpdatedDate = DateTime.UtcNow;

            await _documentRepository.SaveChangesAsync();

            return new JsonModel
            {
                data = true,
                Message = "Document soft deleted successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error soft deleting document {DocumentId}", documentId);
            return new JsonModel
            {
                data = new object(),
                Message = "Internal server error",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetDocumentsByEntityAsync(string entityType, Guid entityId, int? userId, TokenModel tokenModel)
    {
        try
        {
            var references = await _referenceRepository.FindAsync(r => 
                r.EntityType == entityType && 
                r.EntityId == entityId && 
                !r.IsDeleted);

            var documents = new List<DocumentDto>();
            foreach (var reference in references)
            {
                var documentResult = await GetDocumentAsync(reference.DocumentId, userId, tokenModel);
                if (documentResult.StatusCode == 200)
                {
                    documents.Add((DocumentDto)documentResult.data);
                }
            }

            return new JsonModel
            {
                data = documents,
                Message = "Documents retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting documents for entity {EntityType} {EntityId}", entityType, entityId);
            return new JsonModel
            {
                data = new object(),
                Message = "Internal server error",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetDocumentsByReferenceTypeAsync(string entityType, Guid entityId, string referenceType, int? userId, TokenModel tokenModel)
    {
        try
        {
            var references = await _referenceRepository.FindAsync(r => 
                r.EntityType == entityType && 
                r.EntityId == entityId && 
                r.ReferenceType == referenceType && 
                !r.IsDeleted);

            var documents = new List<DocumentDto>();
            foreach (var reference in references)
            {
                var documentResult = await GetDocumentAsync(reference.DocumentId, userId, tokenModel);
                if (documentResult.StatusCode == 200)
                {
                    documents.Add((DocumentDto)documentResult.data);
                }
            }

            return new JsonModel
            {
                data = documents,
                Message = "Documents retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting documents for entity {EntityType} {EntityId} with reference type {ReferenceType}", entityType, entityId, referenceType);
            return new JsonModel
            {
                data = new object(),
                Message = "Internal server error",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> SearchDocumentsAsync(DocumentSearchRequest request, int? userId, TokenModel tokenModel)
    {
        try
        {
            // Build search expression
            Expression<Func<Document, bool>> searchExpression = d => !d.IsDeleted;

            if (!string.IsNullOrEmpty(request.EntityType))
            {
                searchExpression = d => d.DocumentCategory == request.EntityType.ToLower() && !d.IsDeleted;
            }

            if (request.DocumentTypeId.HasValue)
            {
                searchExpression = d => d.DocumentTypeId == request.DocumentTypeId.Value && !d.IsDeleted;
            }

            if (!string.IsNullOrEmpty(request.DocumentTypeName))
            {
                // This would require a join with DocumentType table
                // For now, we'll filter by document type name in memory
                var documentTypes = await _documentTypeRepository.FindAsync(dt => 
                    dt.Name.ToLower().Contains(request.DocumentTypeName.ToLower()) && !dt.IsDeleted);
                var documentTypeIds = documentTypes.Select(dt => dt.Id).ToList();
                searchExpression = d => documentTypeIds.Contains(d.DocumentTypeId) && !d.IsDeleted;
            }

            if (request.IsPublic.HasValue)
            {
                searchExpression = d => d.IsPublic == request.IsPublic.Value && !d.IsDeleted;
            }

            if (request.CreatedFrom.HasValue)
            {
                searchExpression = d => d.CreatedDate >= request.CreatedFrom.Value && !d.IsDeleted;
            }

            if (request.CreatedTo.HasValue)
            {
                searchExpression = d => d.CreatedDate <= request.CreatedTo.Value && !d.IsDeleted;
            }

            var documents = await _documentRepository.FindAsync(searchExpression);
            var documentDtos = new List<DocumentDto>();

            foreach (var document in documents)
            {
                var documentResult = await GetDocumentAsync(document.Id, userId, tokenModel);
                if (documentResult.StatusCode == 200)
                {
                    documentDtos.Add((DocumentDto)documentResult.data);
                }
            }

            // Apply pagination
            var pagedDocuments = documentDtos
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return new JsonModel
            {
                data = pagedDocuments,
                Message = "Documents searched successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching documents");
            return new JsonModel
            {
                data = new object(),
                Message = "Internal server error",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> AddDocumentReferenceAsync(Guid documentId, string entityType, Guid entityId, string? referenceType, int? createdById, TokenModel tokenModel)
    {
        try
        {
            var reference = new DocumentReference
            {
                DocumentId = documentId,
                EntityType = entityType,
                EntityId = entityId,
                ReferenceType = referenceType,
                CreatedBy = createdById ?? 0,
                CreatedDate = DateTime.UtcNow
            };

            await _referenceRepository.AddAsync(reference);
            await _referenceRepository.SaveChangesAsync();

            return new JsonModel
            {
                data = new DocumentReferenceDto
                {
                    Id = reference.Id,
                    DocumentId = reference.DocumentId,
                    EntityType = reference.EntityType,
                    EntityId = reference.EntityId,
                    ReferenceType = reference.ReferenceType,
                    Description = reference.Description,
                    IsPublic = reference.IsPublic,
                    ExpiresAt = reference.ExpiresAt,
                    CreatedById = reference.CreatedBy ?? 0,
                    CreatedDate = reference.CreatedDate ?? DateTime.UtcNow
                },
                Message = "Document reference added successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding document reference for document {DocumentId}", documentId);
            return new JsonModel
            {
                data = new object(),
                Message = "Internal server error",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> RemoveDocumentReferenceAsync(Guid documentId, string entityType, Guid entityId, TokenModel tokenModel)
    {
        try
        {
            var reference = await _referenceRepository.FindAsync(r => 
                r.DocumentId == documentId && 
                r.EntityType == entityType && 
                r.EntityId == entityId && 
                !r.IsDeleted);

            var refToDelete = reference.FirstOrDefault();
            if (refToDelete == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Document reference not found",
                    StatusCode = 404
                };
            }

            refToDelete.IsDeleted = true;
            refToDelete.UpdatedDate = DateTime.UtcNow;

            await _referenceRepository.SaveChangesAsync();

            return new JsonModel
            {
                data = true,
                Message = "Document reference removed successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing document reference for document {DocumentId}", documentId);
            return new JsonModel
            {
                data = new object(),
                Message = "Internal server error",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetDocumentReferencesAsync(Guid documentId, TokenModel tokenModel)
    {
        try
        {
            var references = await _referenceRepository.FindAsync(r => r.DocumentId == documentId && !r.IsDeleted);

            var referenceDtos = references.Select(r => new DocumentReferenceDto
            {
                Id = r.Id,
                DocumentId = r.DocumentId,
                EntityType = r.EntityType,
                EntityId = r.EntityId,
                ReferenceType = r.ReferenceType,
                Description = r.Description,
                IsPublic = r.IsPublic,
                ExpiresAt = r.ExpiresAt,
                CreatedById = r.CreatedBy ?? 0,
                CreatedDate = r.CreatedDate ?? DateTime.UtcNow
            }).ToList();

            return new JsonModel
            {
                data = referenceDtos,
                Message = "Document references retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting document references for document {DocumentId}", documentId);
            return new JsonModel
            {
                data = new object(),
                Message = "Internal server error",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> ValidateDocumentAccessAsync(Guid documentId, int userId, TokenModel tokenModel)
    {
        try
        {
            var document = await _documentRepository.GetByIdAsync(documentId);
            if (document == null || document.IsDeleted)
            {
                return new JsonModel
                {
                    data = false,
                    Message = "Document not found",
                    StatusCode = 200
                };
            }

            // Public documents
            if (document.IsPublic)
            {
                return new JsonModel
                {
                    data = true,
                    Message = "Access granted (public document)",
                    StatusCode = 200
                };
            }

            // Check if user created the document
            if (document.CreatedBy.HasValue && document.CreatedBy.Value == userId)
            {
                return new JsonModel
                {
                    data = true,
                    Message = "Access granted (document owner)",
                    StatusCode = 200
                };
            }

            // Check references for access
            var references = await _referenceRepository.FindAsync(r => 
                r.DocumentId == documentId && 
                !r.IsDeleted);

            foreach (var reference in references)
            {
                if (await ValidateEntityAccessAsync(reference.EntityType, reference.EntityId, userId))
                {
                    return new JsonModel
                    {
                        data = true,
                        Message = "Access granted (entity access)",
                        StatusCode = 200
                    };
                }
            }

            return new JsonModel
            {
                data = false,
                Message = "Access denied",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating document access for document {DocumentId}", documentId);
            return new JsonModel
            {
                data = new object(),
                Message = "Internal server error",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> UpdateDocumentAccessAsync(Guid documentId, bool isPublic, int userId, TokenModel tokenModel)
    {
        try
        {
            var document = await _documentRepository.GetByIdAsync(documentId);
            if (document == null || document.IsDeleted)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Document not found",
                    StatusCode = 404
                };
            }

            // Check if user has permission to update
            var hasAccess = await ValidateDocumentAccessAsync(documentId, userId, tokenModel);
            if (hasAccess.StatusCode != 200 || !(hasAccess.data is bool accessGranted && accessGranted))
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Access denied",
                    StatusCode = 403
                };
            }

            document.IsPublic = isPublic;
            document.UpdatedDate = DateTime.UtcNow;

            await _documentRepository.SaveChangesAsync();

            return new JsonModel
            {
                data = true,
                Message = "Document access updated successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating document access for document {DocumentId}", documentId);
            return new JsonModel
            {
                data = new object(),
                Message = "Internal server error",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> UploadMultipleDocumentsAsync(List<UploadDocumentRequest> requests, TokenModel tokenModel)
    {
        try
        {
            var results = new List<DocumentDto>();

            foreach (var request in requests)
            {
                var result = await UploadDocumentAsync(request, tokenModel);
                if (result.StatusCode == 200)
                {
                    var documentDto = result.data as DocumentDto;
                    if (documentDto != null)
                    {
                        results.Add(documentDto);
                    }
                }
            }

            return new JsonModel
            {
                data = results,
                Message = "Multiple documents uploaded successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading multiple documents");
            return new JsonModel
            {
                data = new object(),
                Message = "Internal server error",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> DeleteMultipleDocumentsAsync(List<Guid> documentIds, int userId, TokenModel tokenModel)
    {
        try
        {
            foreach (var documentId in documentIds)
            {
                await DeleteDocumentAsync(documentId, userId, tokenModel);
            }

            return new JsonModel
            {
                data = true,
                Message = "Multiple documents deleted successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting multiple documents");
            return new JsonModel
            {
                data = new object(),
                Message = "Internal server error",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> UpdateDocumentMetadataAsync(Guid documentId, string? description, bool? isPublic, int userId, TokenModel tokenModel)
    {
        try
        {
            var document = await _documentRepository.GetByIdAsync(documentId);
            if (document == null || document.IsDeleted)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Document not found",
                    StatusCode = 404
                };
            }

            // Check if user has permission to update
            var hasAccess = await ValidateDocumentAccessAsync(documentId, userId, tokenModel);
            if (hasAccess.StatusCode != 200 || !(hasAccess.data is bool accessGranted && accessGranted))
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Access denied",
                    StatusCode = 403
                };
            }

            if (description != null)
            {
                document.Description = description;
            }

            if (isPublic.HasValue)
            {
                document.IsPublic = isPublic.Value;
            }

            document.UpdatedDate = DateTime.UtcNow;

            await _documentRepository.SaveChangesAsync();

            var result = await GetDocumentAsync(documentId, userId, tokenModel);
            if (result.StatusCode == 200)
            {
                return new JsonModel
                {
                    data = result.data,
                    Message = "Document metadata updated successfully",
                    StatusCode = 200
                };
            }
            else
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = result.Message,
                    StatusCode = result.StatusCode
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating document metadata for document {DocumentId}", documentId);
            return new JsonModel
            {
                data = new object(),
                Message = "Internal server error",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> SetDocumentExpirationAsync(Guid documentId, DateTime? expiresAt, int userId, TokenModel tokenModel)
    {
        try
        {
            var references = await _referenceRepository.FindAsync(r => r.DocumentId == documentId && !r.IsDeleted);
            var reference = references.FirstOrDefault();

            if (reference == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Document reference not found",
                    StatusCode = 404
                };
            }

            // Check if user has permission to update
            var hasAccess = await ValidateDocumentAccessAsync(documentId, userId, tokenModel);
            if (hasAccess.StatusCode != 200 || !(hasAccess.data is bool accessGranted && accessGranted))
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Access denied",
                    StatusCode = 403
                };
            }

            reference.ExpiresAt = expiresAt;
            reference.UpdatedDate = DateTime.UtcNow;

            await _referenceRepository.SaveChangesAsync();

            return new JsonModel
            {
                data = true,
                Message = "Document expiration set successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting document expiration for document {DocumentId}", documentId);
            return new JsonModel
            {
                data = new object(),
                Message = "Internal server error",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> UploadUserDocumentAsync(UploadUserDocumentRequest request, TokenModel tokenModel)
    {
        try
        {
            // Convert UploadUserDocumentRequest to UploadDocumentRequest
            var uploadRequest = new UploadDocumentRequest
            {
                FileData = request.FileData,
                FileName = request.FileName,
                ContentType = request.ContentType,
                DocumentTypeId = request.DocumentTypeId,
                EntityType = "User",
                EntityId = Guid.Empty, // We'll handle this in the document creation
                Description = request.Description,
                IsEncrypted = request.IsEncrypted,
                IsPublic = request.IsPublic,
                CreatedById = request.CreatedById
            };

            var result = await UploadDocumentAsync(uploadRequest, tokenModel);
            if (result.StatusCode == 200)
            {
                return new JsonModel
                {
                    data = result.data,
                    Message = "User document uploaded successfully",
                    StatusCode = 200
                };
            }
            else
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = result.Message,
                    StatusCode = result.StatusCode
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading user document");
            return new JsonModel
            {
                data = new object(),
                Message = "Internal server error",
                StatusCode = 500
            };
        }
    }













    public async Task<JsonModel> GetUserDocumentsAsync(int userId, string? referenceType, TokenModel tokenModel)
    {
        try
        {
            var references = await _referenceRepository.FindAsync(r => 
                r.CreatedBy == userId && 
                !r.IsDeleted);

            if (!string.IsNullOrEmpty(referenceType))
            {
                references = references.Where(r => r.ReferenceType == referenceType);
            }

            var documentIds = references.Select(r => r.DocumentId).ToList();
            var documents = new List<DocumentDto>();

            foreach (var documentId in documentIds)
            {
                var document = await _documentRepository.GetByIdAsync(documentId);
                if (document != null && !document.IsDeleted)
                {
                    documents.Add(MapToDocumentDto(document));
                }
            }

            return new JsonModel
            {
                data = documents,
                Message = "User documents retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user documents for user {UserId}", userId);
            return new JsonModel
            {
                data = new object(),
                Message = "Internal server error",
                StatusCode = 500
            };
        }
    }











    private async Task<bool> ValidateEntityAccessAsync(string entityType, Guid entityId, int userId)
    {
        // This is a simplified implementation
        // In a real application, you would implement specific access control logic for each entity type
        switch (entityType)
        {
            case "Appointment":
                // Implement appointment access validation
                return true; // Placeholder
            case "User":
                // For User entity, we need to handle the case where EntityId is a Guid but userId is int
                // This is a temporary fix - in production, you might want to store user IDs consistently
                return entityId.ToString() == userId.ToString(); // Users can only access their own documents
            case "ChatRoom":
                // Implement chat room access validation
                return true; // Placeholder
            default:
                return false;
        }
    }

    public async Task<JsonModel> DeleteUserDocumentAsync(Guid documentId, int userId, TokenModel tokenModel)
    {
        try
        {
            // Validate that the user owns this document
            var document = await _documentRepository.GetByIdAsync(documentId);
            if (document == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Document not found",
                    StatusCode = 404
                };
            }

            // Check if user has access to this document
            var hasAccess = await ValidateEntityAccessAsync("User", documentId, userId);
            if (!hasAccess)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Access denied",
                    StatusCode = 403
                };
            }

            // Soft delete the document
            document.IsDeleted = true;
            document.DeletedDate = DateTime.UtcNow;
            await _documentRepository.UpdateAsync(document);

            // Also soft delete any references
            var references = await _referenceRepository.FindAsync(r => r.DocumentId == documentId);
            foreach (var reference in references)
            {
                reference.IsDeleted = true;
                await _referenceRepository.UpdateAsync(reference);
            }

            return new JsonModel
            {
                data = new object(),
                Message = "Document deleted successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user document {DocumentId} for user {UserId}", documentId, userId);
            return new JsonModel
            {
                data = new object(),
                Message = "Internal server error",
                StatusCode = 500
            };
        }
    }

    private DocumentDto MapToDocumentDto(Document document)
    {
        return new DocumentDto
        {
            DocumentId = document.Id,
            OriginalName = document.OriginalName,
            UniqueName = document.UniqueName,
            FilePath = document.FilePath,
            FolderPath = document.FolderPath,
            ContentType = document.ContentType,
            FileSize = document.FileSize,
            DocumentTypeId = document.DocumentTypeId,
            DocumentCategory = document.DocumentCategory,
            Description = document.Description,
            IsEncrypted = document.IsEncrypted,
            IsPublic = document.IsPublic,
            CreatedById = document.CreatedBy ?? 0,
            CreatedDate = document.CreatedDate ?? DateTime.UtcNow,
            DeletedDate = document.DeletedDate,
            IsActive = document.IsActive,
            IsDeleted = document.IsDeleted
        };
    }

    private DocumentReferenceDto MapToDocumentReferenceDto(DocumentReference reference)
    {
        return new DocumentReferenceDto
        {
            Id = reference.Id,
            DocumentId = reference.DocumentId,
            EntityType = reference.EntityType,
            EntityId = reference.EntityId,
            ReferenceType = reference.ReferenceType,
            CreatedById = reference.CreatedBy ?? 0,
            CreatedDate = reference.CreatedDate ?? DateTime.UtcNow,
            ExpiresAt = reference.ExpiresAt
        };
    }
} 