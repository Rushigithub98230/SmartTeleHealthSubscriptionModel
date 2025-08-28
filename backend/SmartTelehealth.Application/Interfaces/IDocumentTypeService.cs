using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IDocumentTypeService
{
    // Core document type operations
    Task<JsonModel> CreateDocumentTypeAsync(CreateDocumentTypeRequest request);
    Task<JsonModel> GetDocumentTypeAsync(Guid documentTypeId);
    Task<JsonModel> GetByNameAsync(string name);
    Task<JsonModel> UpdateDocumentTypeAsync(Guid documentTypeId, UpdateDocumentTypeRequest request);
    Task<JsonModel> DeleteDocumentTypeAsync(Guid documentTypeId, Guid userId);
    Task<JsonModel> SoftDeleteDocumentTypeAsync(Guid documentTypeId, Guid userId);
    
    // Document type listing and search
    Task<JsonModel> GetAllDocumentTypesAsync(bool? isActive = null);
    Task<JsonModel> SearchDocumentTypesAsync(DocumentTypeSearchRequest request);
    Task<JsonModel> GetActiveDocumentTypesAsync();
    Task<JsonModel> GetSystemDocumentTypesAsync();
    Task<JsonModel> GetAdminDocumentTypesAsync();
    
    // Document type statistics
    Task<JsonModel> GetDocumentTypeWithStatsAsync(Guid documentTypeId);
    Task<JsonModel> GetPopularDocumentTypesAsync(int limit = 10);
    
    // Document type validation
    Task<JsonModel> ValidateDocumentTypeAsync(Guid documentTypeId);
    Task<JsonModel> IsDocumentTypeActiveAsync(Guid documentTypeId);
    Task<JsonModel> ValidateFileAgainstDocumentTypeAsync(DocumentTypeValidationRequest request);
    Task<JsonModel> ValidateFileExtensionAsync(Guid documentTypeId, string fileName);
    Task<JsonModel> ValidateFileSizeAsync(Guid documentTypeId, long fileSizeBytes);
    
    // Bulk operations
    Task<JsonModel> ActivateDocumentTypeAsync(Guid documentTypeId, Guid userId);
    Task<JsonModel> DeactivateDocumentTypeAsync(Guid documentTypeId, Guid userId);
    Task<JsonModel> ActivateMultipleDocumentTypesAsync(List<Guid> documentTypeIds, Guid userId);
    Task<JsonModel> DeactivateMultipleDocumentTypesAsync(List<Guid> documentTypeIds, Guid userId);
    
    // Usage tracking
    Task<JsonModel> IncrementUsageCountAsync(Guid documentTypeId);
    Task<JsonModel> UpdateLastUsedAtAsync(Guid documentTypeId);
    
    // System document type management
    Task<JsonModel> CreateSystemDocumentTypeAsync(CreateDocumentTypeRequest request);
    Task<JsonModel> GetDocumentTypesForEntityAsync(string entityType, bool? isActive = null);
} 