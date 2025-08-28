using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Infrastructure.Services;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FileStorageController : BaseController
{
    private readonly IFileStorageService _fileStorageService;

    public FileStorageController(IFileStorageService fileStorageService)
    {
        _fileStorageService = fileStorageService;
    }

    /// <summary>
    /// Upload a single file
    /// </summary>
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
    /// Upload multiple files
    /// </summary>
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
    /// Download a file
    /// </summary>
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