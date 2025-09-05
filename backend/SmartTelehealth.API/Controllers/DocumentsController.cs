using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;

namespace SmartTelehealth.API.Controllers;

/// <summary>
/// Controller responsible for comprehensive document management and file handling.
/// This controller provides extensive functionality for document upload, storage, retrieval,
/// metadata management, access control, and document lifecycle operations. It supports
/// various document types, file formats, and secure document access with proper authorization.
/// </summary>
[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class DocumentsController : BaseController
{
    private readonly IDocumentService _documentService;

    /// <summary>
    /// Initializes a new instance of the DocumentsController with the required document service.
    /// </summary>
    /// <param name="documentService">Service for handling document-related business logic</param>
    public DocumentsController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    /// <summary>
    /// Uploads a new document to the system with metadata and access control.
    /// This endpoint handles document upload including file validation, metadata extraction,
    /// access control setup, and secure storage in the document management system.
    /// </summary>
    /// <param name="request">DTO containing document upload details and metadata</param>
    /// <returns>JsonModel containing the uploaded document information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Uploads document with metadata and access control
    /// - Validates file format, size, and content
    /// - Sets up document access permissions and security
    /// - Access restricted to authenticated users
    /// - Used for document upload and management
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on upload operations
    /// - Maintains document audit trails and upload history
    /// </remarks>
    [HttpPost("upload")]
    public async Task<JsonModel> UploadDocument([FromBody] UploadDocumentRequest request)
    {
        return await _documentService.UploadDocumentAsync(request, GetToken(HttpContext));
    }

    /// <summary>
    /// Uploads a document specifically associated with a user account.
    /// This endpoint handles user-specific document upload including user association,
    /// access control setup, and document categorization for user document management.
    /// </summary>
    /// <param name="request">DTO containing user document upload details</param>
    /// <returns>JsonModel containing the uploaded user document information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Uploads document associated with specific user account
    /// - Sets up user-specific access control and permissions
    /// - Handles document categorization and user association
    /// - Access restricted to authenticated users
    /// - Used for user document upload and management
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on upload operations
    /// - Maintains user document audit trails and upload history
    /// </remarks>
    [HttpPost("user/upload")]
    public async Task<JsonModel> UploadUserDocument([FromBody] UploadUserDocumentRequest request)
    {
        return await _documentService.UploadUserDocumentAsync(request, GetToken(HttpContext));
    }

    /// <summary>
    /// Retrieves document information by its unique identifier.
    /// This endpoint provides document metadata, access information, and basic details
    /// without returning the actual document content for efficient data retrieval.
    /// </summary>
    /// <param name="documentId">The unique identifier of the document to retrieve</param>
    /// <param name="userId">Optional user ID for access validation</param>
    /// <returns>JsonModel containing the document information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns document metadata and access information
    /// - Validates user access permissions for the document
    /// - Provides document details without content for efficiency
    /// - Access restricted to document owners and authorized users
    /// - Used for document information retrieval and access validation
    /// - Includes comprehensive access control and authorization
    /// - Provides secure access to document information
    /// - Handles authorization validation and error responses
    /// </remarks>
    [HttpGet("{documentId}")]
    public async Task<JsonModel> GetDocument(Guid documentId, [FromQuery] int? userId = null)
    {
        return await _documentService.GetDocumentAsync(documentId, userId, GetToken(HttpContext));
    }

    /// <summary>
    /// Retrieves document information along with its actual content.
    /// This endpoint provides complete document data including metadata and file content
    /// for document viewing, downloading, and content processing operations.
    /// </summary>
    /// <param name="documentId">The unique identifier of the document to retrieve</param>
    /// <param name="userId">Optional user ID for access validation</param>
    /// <returns>JsonModel containing the document information and content</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns document metadata and actual file content
    /// - Validates user access permissions for the document
    /// - Provides complete document data for viewing and processing
    /// - Access restricted to document owners and authorized users
    /// - Used for document viewing, downloading, and content processing
    /// - Includes comprehensive access control and authorization
    /// - Provides secure access to document content
    /// - Handles authorization validation and error responses
    /// </remarks>
    [HttpGet("{documentId}/content")]
    public async Task<JsonModel> GetDocumentWithContent(Guid documentId, [FromQuery] int? userId = null)
    {
        return await _documentService.GetDocumentWithContentAsync(documentId, userId, GetToken(HttpContext));
    }

    /// <summary>
    /// Get user documents
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<JsonModel> GetUserDocuments(int userId, [FromQuery] string? referenceType = null)
    {
        return await _documentService.GetUserDocumentsAsync(userId, referenceType, GetToken(HttpContext));
    }

    /// <summary>
    /// Search documents
    /// </summary>
    [HttpPost("search")]
    public async Task<JsonModel> SearchDocuments([FromBody] DocumentSearchRequest request, [FromQuery] int? userId = null)
    {
        return await _documentService.SearchDocumentsAsync(request, userId, GetToken(HttpContext));
    }

    /// <summary>
    /// Update document metadata
    /// </summary>
    [HttpPut("{documentId}/metadata")]
    public async Task<JsonModel> UpdateDocumentMetadata(Guid documentId, [FromBody] UpdateDocumentMetadataRequest request)
    {
        var tokenModel = GetToken(HttpContext);
        return await _documentService.UpdateDocumentMetadataAsync(documentId, request.Description, request.IsPublic, tokenModel.UserID, tokenModel);
    }

    /// <summary>
    /// Delete a document
    /// </summary>
    [HttpDelete("{documentId}")]
    public async Task<JsonModel> DeleteDocument(Guid documentId, [FromQuery] int userId)
    {
        return await _documentService.DeleteDocumentAsync(documentId, userId, GetToken(HttpContext));
    }

    /// <summary>
    /// Soft delete a document
    /// </summary>
    [HttpDelete("{documentId}/soft")]
    public async Task<JsonModel> SoftDeleteDocument(Guid documentId, [FromQuery] int userId)
    {
        return await _documentService.SoftDeleteDocumentAsync(documentId, userId, GetToken(HttpContext));
    }

    /// <summary>
    /// Validate document access
    /// </summary>
    [HttpGet("{documentId}/access")]
    public async Task<JsonModel> ValidateDocumentAccess(Guid documentId, [FromQuery] int userId)
    {
        return await _documentService.ValidateDocumentAccessAsync(documentId, userId, GetToken(HttpContext));
    }
}

public class UpdateDocumentMetadataRequest
{
    public string? Description { get; set; }
    public bool? IsPublic { get; set; }
}
