using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DocumentsController : BaseController
{
    private readonly IDocumentService _documentService;

    public DocumentsController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    /// <summary>
    /// Upload a document
    /// </summary>
    [HttpPost("upload")]
    public async Task<JsonModel> UploadDocument([FromBody] UploadDocumentRequest request)
    {
        return await _documentService.UploadDocumentAsync(request, GetToken(HttpContext));
    }

    /// <summary>
    /// Upload a user document
    /// </summary>
    [HttpPost("user/upload")]
    public async Task<JsonModel> UploadUserDocument([FromBody] UploadUserDocumentRequest request)
    {
        return await _documentService.UploadUserDocumentAsync(request, GetToken(HttpContext));
    }

    /// <summary>
    /// Get a document by ID
    /// </summary>
    [HttpGet("{documentId}")]
    public async Task<JsonModel> GetDocument(Guid documentId, [FromQuery] int? userId = null)
    {
        return await _documentService.GetDocumentAsync(documentId, userId, GetToken(HttpContext));
    }

    /// <summary>
    /// Get a document with content by ID
    /// </summary>
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
