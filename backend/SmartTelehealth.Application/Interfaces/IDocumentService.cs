using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IDocumentService
{
    // Core document operations
    Task<JsonModel> UploadDocumentAsync(UploadDocumentRequest request, TokenModel tokenModel);
    Task<JsonModel> UploadUserDocumentAsync(UploadUserDocumentRequest request, TokenModel tokenModel);
    Task<JsonModel> DeleteUserDocumentAsync(Guid documentId, int userId, TokenModel tokenModel);
    Task<JsonModel> GetDocumentAsync(Guid documentId, int? userId, TokenModel tokenModel);
    Task<JsonModel> GetDocumentWithContentAsync(Guid documentId, int? userId, TokenModel tokenModel);
    Task<JsonModel> DeleteDocumentAsync(Guid documentId, int userId, TokenModel tokenModel);
    Task<JsonModel> SoftDeleteDocumentAsync(Guid documentId, int userId, TokenModel tokenModel);
    
    // Document search and listing
    Task<JsonModel> GetDocumentsByEntityAsync(string entityType, Guid entityId, int? userId, TokenModel tokenModel);
    Task<JsonModel> GetDocumentsByReferenceTypeAsync(string entityType, Guid entityId, string referenceType, int? userId, TokenModel tokenModel);
    Task<JsonModel> GetUserDocumentsAsync(int userId, string? referenceType, TokenModel tokenModel);
    Task<JsonModel> SearchDocumentsAsync(DocumentSearchRequest request, int? userId, TokenModel tokenModel);
    
    // Document references
    Task<JsonModel> AddDocumentReferenceAsync(Guid documentId, string entityType, Guid entityId, string? referenceType, int? createdById, TokenModel tokenModel);
    Task<JsonModel> RemoveDocumentReferenceAsync(Guid documentId, string entityType, Guid entityId, TokenModel tokenModel);
    Task<JsonModel> GetDocumentReferencesAsync(Guid documentId, TokenModel tokenModel);
    
    // Access control
    Task<JsonModel> ValidateDocumentAccessAsync(Guid documentId, int userId, TokenModel tokenModel);
    Task<JsonModel> UpdateDocumentAccessAsync(Guid documentId, bool isPublic, int userId, TokenModel tokenModel);
    
    // Batch operations
    Task<JsonModel> UploadMultipleDocumentsAsync(List<UploadDocumentRequest> requests, TokenModel tokenModel);
    Task<JsonModel> DeleteMultipleDocumentsAsync(List<Guid> documentIds, int userId, TokenModel tokenModel);
    
    // Document metadata
    Task<JsonModel> UpdateDocumentMetadataAsync(Guid documentId, string? description, bool? isPublic, int userId, TokenModel tokenModel);
    Task<JsonModel> SetDocumentExpirationAsync(Guid documentId, DateTime? expiresAt, int userId, TokenModel tokenModel);
} 