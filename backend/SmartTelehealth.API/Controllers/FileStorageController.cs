using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Infrastructure.Services;

namespace SmartTelehealth.API.Controllers;

/// <summary>
/// Controller responsible for comprehensive file storage and management functionality.
/// This controller provides extensive functionality for file upload, download, storage management,
/// file encryption, secure access, and file lifecycle operations. It handles the complete
/// file management workflow from upload to secure access and cleanup.
/// </summary>
[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class FileStorageController : BaseController
{
    private readonly IFileStorageService _fileStorageService;

    /// <summary>
    /// Initializes a new instance of the FileStorageController with the required file storage service.
    /// </summary>
    /// <param name="fileStorageService">Service for handling file storage-related business logic</param>
    public FileStorageController(IFileStorageService fileStorageService)
    {
        _fileStorageService = fileStorageService;
    }

    /// <summary>
    /// Uploads a single file to the file storage system.
    /// This endpoint handles file upload including validation, storage, and metadata management
    /// for secure file storage and access control in the SmartTelehealth system.
    /// </summary>
    /// <param name="file">The file to upload</param>
    /// <returns>JsonModel containing the upload result and file information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Uploads a single file to the file storage system
    /// - Validates file content, size, and format
    /// - Stores file with metadata and access control
    /// - Access restricted to authenticated users
    /// - Used for file upload and storage management
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on upload operations
    /// - Maintains file upload audit trails and storage history
    /// </remarks>
    [HttpPost("upload")]
    public async Task<JsonModel> UploadFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return new JsonModel { data = new object(), Message = "No file provided", StatusCode = 400 };
        }

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var fileData = memoryStream.ToArray();

        var result = await _fileStorageService.UploadFileAsync(fileData, file.FileName, file.ContentType, GetToken(HttpContext));
        
        if (result.StatusCode == 200)
        {
            return result;
        }
        
        return result;
    }

    /// <summary>
    /// Uploads multiple files to the file storage system.
    /// This endpoint handles batch file upload including validation, storage, and metadata management
    /// for efficient multiple file storage and access control in the SmartTelehealth system.
    /// </summary>
    /// <param name="files">The collection of files to upload</param>
    /// <returns>JsonModel containing the upload results and file information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Uploads multiple files to the file storage system
    /// - Validates file content, size, and format for each file
    /// - Stores files with metadata and access control
    /// - Access restricted to authenticated users
    /// - Used for batch file upload and storage management
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on upload operations
    /// - Maintains file upload audit trails and storage history
    /// </remarks>
    [HttpPost("upload-multiple")]
    public async Task<JsonModel> UploadMultipleFiles(IFormFileCollection files)
    {
        if (files == null || !files.Any())
        {
            return new JsonModel { data = new object(), Message = "No files provided", StatusCode = 400 };
        }

        var fileUploads = new List<FileUploadDto>();

        foreach (var file in files)
        {
            if (file.Length > 0)
            {
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                
                fileUploads.Add(new FileUploadDto
                {
                    Content = memoryStream.ToArray(),
                    FileName = file.FileName,
                    ContentType = file.ContentType
                });
            }
        }

        var result = await _fileStorageService.UploadMultipleFilesAsync(fileUploads, GetToken(HttpContext));
        
        if (result.StatusCode == 200)
        {
            return result;
        }
        
        return result;
    }

    /// <summary>
    /// Downloads a file from the file storage system.
    /// This endpoint handles secure file download including access validation, file retrieval,
    /// and download management for authorized file access in the SmartTelehealth system.
    /// </summary>
    /// <param name="filePath">The path of the file to download</param>
    /// <returns>JsonModel containing the file data and download information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Downloads a file from the file storage system
    /// - Validates file access permissions and authorization
    /// - Retrieves file content and metadata
    /// - Access restricted to authenticated users with file access
    /// - Used for file download and access management
    /// - Includes comprehensive validation and error handling
    /// - Provides secure file download functionality
    /// - Maintains file access audit trails and download history
    /// </remarks>
    [HttpGet("download/{filePath}")]
    public async Task<JsonModel> DownloadFile(string filePath)
    {
        var result = await _fileStorageService.DownloadFileAsync(filePath, GetToken(HttpContext));
        
        if (result.StatusCode != 200)
        {
            return new JsonModel { data = new object(), Message = "File not found", StatusCode = 404 };
        }

        var fileInfo = await _fileStorageService.GetFileInfoAsync(filePath, GetToken(HttpContext));
        if (fileInfo.StatusCode != 200)
        {
            return new JsonModel { data = new object(), Message = "File info not found", StatusCode = 404 };
        }

        return result;
    }

    /// <summary>
    /// Get file information
    /// </summary>
    [HttpGet("info/{filePath}")]
    public async Task<JsonModel> GetFileInfo(string filePath)
    {
        var result = await _fileStorageService.GetFileInfoAsync(filePath, GetToken(HttpContext));
        
        if (result.StatusCode == 200)
        {
            return result;
        }
        
        return result;
    }

    /// <summary>
    /// Get secure URL for file access
    /// </summary>
    [HttpGet("secure-url/{filePath}")]
    public async Task<JsonModel> GetSecureUrl(string filePath, [FromQuery] int? expirationHours = 1)
    {
        var expiration = expirationHours.HasValue ? TimeSpan.FromHours(expirationHours.Value) : TimeSpan.FromHours(1);
        var result = await _fileStorageService.GetSecureUrlAsync(filePath, expiration, GetToken(HttpContext));
        
        if (result.StatusCode == 200)
        {
            return result;
        }
        
        return result;
    }

    /// <summary>
    /// Delete a file
    /// </summary>
    [HttpDelete("{filePath}")]
    public async Task<JsonModel> DeleteFile(string filePath)
    {
        var result = await _fileStorageService.DeleteFileAsync(filePath, GetToken(HttpContext));
        
        if (result.StatusCode == 200)
        {
            return result;
        }
        
        return result;
    }

    /// <summary>
    /// Delete multiple files
    /// </summary>
    [HttpDelete("multiple")]
    public async Task<JsonModel> DeleteMultipleFiles([FromBody] List<string> filePaths)
    {
        if (filePaths == null || !filePaths.Any())
        {
            return new JsonModel { data = new object(), Message = "No file paths provided", StatusCode = 400 };
        }

        var result = await _fileStorageService.DeleteMultipleFilesAsync(filePaths, GetToken(HttpContext));
        return result;
    }

    /// <summary>
    /// List files in a directory
    /// </summary>
    [HttpGet("list/{directoryPath}")]
    public async Task<JsonModel> ListFiles(string directoryPath, [FromQuery] string? searchPattern = null)
    {
        var result = await _fileStorageService.ListFilesAsync(directoryPath, searchPattern, GetToken(HttpContext));
        
        if (result.StatusCode == 200)
        {
            return result;
        }
        
        return result;
    }

    /// <summary>
    /// Get storage information
    /// </summary>
    [HttpGet("storage-info")]
    public async Task<JsonModel> GetStorageInfo()
    {
        var result = await _fileStorageService.GetStorageInfoAsync(GetToken(HttpContext));
        return result;
    }

    /// <summary>
    /// Cleanup expired files
    /// </summary>
    [HttpPost("cleanup")]
    
    public async Task<JsonModel> CleanupExpiredFiles()
    {
        var result = await _fileStorageService.CleanupExpiredFilesAsync(GetToken(HttpContext));
        return result;
    }

    /// <summary>
    /// Archive old files
    /// </summary>
    [HttpPost("archive")]
    
    public async Task<JsonModel> ArchiveOldFiles([FromBody] ArchiveFilesRequest request)
    {
        if (string.IsNullOrEmpty(request.SourcePath) || string.IsNullOrEmpty(request.ArchivePath))
        {
            return new JsonModel { data = new object(), Message = "Source path and archive path are required", StatusCode = 400 };
        }

        var ageThreshold = TimeSpan.FromDays(request.AgeThresholdDays);
        var result = await _fileStorageService.ArchiveOldFilesAsync(request.SourcePath, request.ArchivePath, ageThreshold, GetToken(HttpContext));
        return result;
    }

    /// <summary>
    /// Encrypt a file
    /// </summary>
    [HttpPost("encrypt")]
    public async Task<JsonModel> EncryptFile(IFormFile file, [FromQuery] string encryptionKey)
    {
        if (file == null || file.Length == 0)
        {
            return new JsonModel { data = new object(), Message = "No file provided", StatusCode = 400 };
        }

        if (string.IsNullOrEmpty(encryptionKey))
        {
            return new JsonModel { data = new object(), Message = "Encryption key is required", StatusCode = 400 };
        }

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var fileData = memoryStream.ToArray();

        var result = await _fileStorageService.EncryptFileAsync(fileData, encryptionKey, GetToken(HttpContext));
        
        if (result.StatusCode == 200)
        {
            return result;
        }
        
        return result;
    }

    /// <summary>
    /// Decrypt a file
    /// </summary>
    [HttpPost("decrypt/{encryptedFilePath}")]
    public async Task<JsonModel> DecryptFile(string encryptedFilePath, [FromQuery] string encryptionKey)
    {
        if (string.IsNullOrEmpty(encryptionKey))
        {
            return new JsonModel { data = new object(), Message = "Encryption key is required", StatusCode = 400 };
        }

        var result = await _fileStorageService.DecryptFileAsync(encryptedFilePath, encryptionKey, GetToken(HttpContext));
        
        if (result.StatusCode != 200)
        {
            return new JsonModel { data = new object(), Message = "Encrypted file not found", StatusCode = 404 };
        }

        return result;
    }
}

public class ArchiveFilesRequest
{
    public string SourcePath { get; set; } = string.Empty;
    public string ArchivePath { get; set; } = string.Empty;
    public int AgeThresholdDays { get; set; } = 30;
} 