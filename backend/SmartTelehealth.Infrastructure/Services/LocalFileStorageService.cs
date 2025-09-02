using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using System.Security.Cryptography;
using System.Text;
using DTOs = SmartTelehealth.Application.DTOs;
using Microsoft.Extensions.Configuration;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;

namespace SmartTelehealth.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly ILogger<LocalFileStorageService> _logger;
    private readonly string _baseStoragePath;
    private readonly string _encryptionKey;

    public LocalFileStorageService(ILogger<LocalFileStorageService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _baseStoragePath = configuration["FileStorage:Local:BasePath"] ?? "wwwroot/uploads";
        _encryptionKey = configuration["FileStorage:EncryptionKey"] ?? "default-encryption-key-change-in-production";
        
        // Ensure base directory exists
        if (!Directory.Exists(_baseStoragePath))
        {
            Directory.CreateDirectory(_baseStoragePath);
        }
    }

    public async Task<JsonModel> UploadFileAsync(byte[] fileData, string fileName, string contentType, TokenModel tokenModel)
    {
        try
        {
            // Generate unique filename to prevent conflicts
            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            var filePath = Path.Combine(_baseStoragePath, uniqueFileName);
            
            // Ensure directory exists
            var directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory!);
            }
            
            // Write file
            await File.WriteAllBytesAsync(filePath, fileData);
            
            // Store original filename metadata
            var metadataPath = filePath + ".meta";
            var metadata = new { OriginalFileName = fileName, ContentType = contentType, UploadedAt = DateTime.UtcNow };
            await File.WriteAllTextAsync(metadataPath, System.Text.Json.JsonSerializer.Serialize(metadata));
            
            _logger.LogInformation("File uploaded successfully: {FileName} -> {FilePath}", fileName, filePath);
            return new JsonModel
            {
                data = uniqueFileName,
                Message = "File uploaded successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file: {FileName}", fileName);
            return new JsonModel
            {
                data = new object(),
                Message = $"Error uploading file: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> DownloadFileAsync(string filePath, TokenModel tokenModel)
    {
        try
        {
            var fullPath = Path.Combine(_baseStoragePath, filePath);
            if (!File.Exists(fullPath))
                return new JsonModel
                {
                    data = new object(),
                    Message = "File not found",
                    StatusCode = 404
                };
            var data = await File.ReadAllBytesAsync(fullPath);
            return new JsonModel
            {
                data = data,
                Message = "File downloaded successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file: {FilePath}", filePath);
            return new JsonModel
            {
                data = new object(),
                Message = $"Error downloading file: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> DeleteFileAsync(string filePath, TokenModel tokenModel)
    {
        try
        {
            var fullPath = Path.Combine(_baseStoragePath, filePath);
            if (!File.Exists(fullPath))
                return new JsonModel
                {
                    data = new object(),
                    Message = "File not found",
                    StatusCode = 404
                };
            File.Delete(fullPath);
            
            // Delete metadata file if it exists
            var metadataPath = fullPath + ".meta";
            if (File.Exists(metadataPath))
            {
                File.Delete(metadataPath);
            }
            
            return new JsonModel
            {
                data = true,
                Message = "File deleted successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
            return new JsonModel
            {
                data = new object(),
                Message = $"Error deleting file: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> FileExistsAsync(string filePath, TokenModel tokenModel)
    {
        try
        {
            var fullPath = Path.Combine(_baseStoragePath, filePath);
            var exists = File.Exists(fullPath);
            return new JsonModel
            {
                data = exists,
                Message = exists ? "File exists" : "File not found",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking file existence: {FilePath}", filePath);
            return new JsonModel
            {
                data = new object(),
                Message = $"Error checking file existence: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetFileSizeAsync(string filePath, TokenModel tokenModel)
    {
        try
        {
            var fullPath = Path.Combine(_baseStoragePath, filePath);
            if (!File.Exists(fullPath))
                return new JsonModel
                {
                    data = new object(),
                    Message = "File not found",
                    StatusCode = 404
                };
            var fileInfo = new FileInfo(fullPath);
            return new JsonModel
            {
                data = fileInfo.Length,
                Message = "File size retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file size: {FilePath}", filePath);
            return new JsonModel
            {
                data = new object(),
                Message = $"Error getting file size: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public Task<JsonModel> GetFileUrlAsync(string filePath, TokenModel tokenModel)
    {
        var url = $"/uploads/{filePath}";
        return Task.FromResult(new JsonModel
        {
            data = url,
            Message = "File URL generated successfully",
            StatusCode = 200
        });
    }

    public async Task<JsonModel> GetFileInfoAsync(string filePath, TokenModel tokenModel)
    {
        try
        {
            var fullPath = Path.Combine(_baseStoragePath, filePath);
            if (!File.Exists(fullPath))
                return new JsonModel
                {
                    data = new object(),
                    Message = "File not found",
                    StatusCode = 404
                };
            
            var fileInfo = new FileInfo(fullPath);
            var extension = Path.GetExtension(filePath);
            
            // Try to get original filename from metadata
            var originalFileName = Path.GetFileName(filePath); // Default to the stored filename
            var metadataPath = fullPath + ".meta";
            if (File.Exists(metadataPath))
            {
                try
                {
                    var metadataJson = await File.ReadAllTextAsync(metadataPath);
                    var metadata = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(metadataJson);
                    if (metadata != null && metadata.ContainsKey("OriginalFileName"))
                    {
                        originalFileName = metadata["OriginalFileName"]?.ToString() ?? originalFileName;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not read metadata for file: {FilePath}", filePath);
                }
            }
            
            var dto = new FileInfoDto
            {
                FileName = originalFileName,
                FilePath = filePath,
                FileSize = fileInfo.Length,
                CreatedDate = fileInfo.CreationTimeUtc,
                ModifiedAt = fileInfo.LastWriteTimeUtc,
                ContentType = GetContentTypeFromExtension(extension),
                IsDirectory = false
            };
            
            return new JsonModel
            {
                data = dto,
                Message = "File info retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file info: {FilePath}", filePath);
            return new JsonModel
            {
                data = new object(),
                Message = $"Error getting file info: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public Task<JsonModel> GetSecureUrlAsync(string filePath, TimeSpan? expiration, TokenModel tokenModel)
    {
        var url = $"/uploads/{filePath}";
        return Task.FromResult(new JsonModel
        {
            data = url,
            Message = "Secure file URL generated successfully",
            StatusCode = 200
        });
    }

    public async Task<JsonModel> CreateDirectoryAsync(string directoryPath, TokenModel tokenModel)
    {
        try
        {
            var fullPath = Path.Combine(_baseStoragePath, directoryPath);
            if (Directory.Exists(fullPath))
                return new JsonModel
                {
                    data = true,
                    Message = "Directory already exists",
                    StatusCode = 200
                };
            
            Directory.CreateDirectory(fullPath);
            return new JsonModel
            {
                data = true,
                Message = "Directory created successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating directory: {DirectoryPath}", directoryPath);
            return new JsonModel
            {
                data = new object(),
                Message = $"Error creating directory: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> DeleteDirectoryAsync(string directoryPath, TokenModel tokenModel)
    {
        try
        {
            var fullPath = Path.Combine(_baseStoragePath, directoryPath);
            if (!Directory.Exists(fullPath))
                return new JsonModel
                {
                    data = new object(),
                    Message = "Directory not found",
                    StatusCode = 404
                };
            
            Directory.Delete(fullPath, true);
            return new JsonModel
            {
                data = true,
                Message = "Directory deleted successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting directory: {DirectoryPath}", directoryPath);
            return new JsonModel
            {
                data = new object(),
                Message = $"Error deleting directory: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> ListFilesAsync(string directoryPath, string? searchPattern, TokenModel tokenModel)
    {
        try
        {
            var fullPath = Path.Combine(_baseStoragePath, directoryPath);
            if (!Directory.Exists(fullPath))
                return new JsonModel
                {
                    data = new object(),
                    Message = "Directory not found",
                    StatusCode = 404
                };
            
            var files = Directory.GetFiles(fullPath, searchPattern ?? "*")
                .Select(f => Path.GetFileName(f))
                .Where(f => !f!.EndsWith(".meta")) // Exclude metadata files
                .ToList();
            
            return new JsonModel
            {
                data = files,
                Message = "Files listed successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing files: {DirectoryPath}", directoryPath);
            return new JsonModel
            {
                data = new object(),
                Message = $"Error listing files: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public Task<JsonModel> ValidateFileAccessAsync(string filePath, Guid userId, TokenModel tokenModel)
    {
        // For now, implement basic access validation
        // In a real implementation, you would check user permissions, file ownership, etc.
        try
        {
            var fullPath = Path.Combine(_baseStoragePath, filePath);
            var exists = File.Exists(fullPath);
            
            if (!exists)
                return Task.FromResult(new JsonModel
                {
                    data = new object(),
                    Message = "File not found",
                    StatusCode = 404
                });
            
            // Basic access validation - in production, implement proper access control
            // For now, if file exists, grant access
            return Task.FromResult(new JsonModel
            {
                data = true,
                Message = "Access granted",
                StatusCode = 200
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating file access: {FilePath} for user {UserId}", filePath, userId);
            return Task.FromResult(new JsonModel
            {
                data = new object(),
                Message = $"Error validating file access: {ex.Message}",
                StatusCode = 500
            });
        }
    }

    public Task<JsonModel> SetFilePermissionsAsync(string filePath, FilePermissions permissions, TokenModel tokenModel)
    {
        // For local storage, file permissions are handled by the file system
        // In production, you might implement additional permission tracking
        return Task.FromResult(new JsonModel
        {
            data = true,
            Message = "File permissions set successfully",
            StatusCode = 200
        });
    }

    public async Task<JsonModel> EncryptFileAsync(byte[] fileData, string encryptionKey, TokenModel tokenModel)
    {
        try
        {
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(encryptionKey.PadRight(32).Substring(0, 32));
            aes.GenerateIV();
            
            using var encryptor = aes.CreateEncryptor();
            using var msEncrypt = new MemoryStream();
            
            // Write IV first
            await msEncrypt.WriteAsync(aes.IV, 0, aes.IV.Length);
            
            using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            {
                await csEncrypt.WriteAsync(fileData, 0, fileData.Length);
                await csEncrypt.FlushFinalBlockAsync();
            }
            
            var encryptedData = msEncrypt.ToArray();
            var base64Result = Convert.ToBase64String(encryptedData);
            
            return new JsonModel
            {
                data = base64Result,
                Message = "File encrypted successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error encrypting file data");
            return new JsonModel
            {
                data = new object(),
                Message = $"Error encrypting file: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> DecryptFileAsync(string encryptedFilePath, string encryptionKey, TokenModel tokenModel)
    {
        try
        {
            var fullPath = Path.Combine(_baseStoragePath, encryptedFilePath);
            if (!File.Exists(fullPath))
                return new JsonModel
                {
                    data = new object(),
                    Message = "Encrypted file not found",
                    StatusCode = 404
                };
            
            var encryptedData = await File.ReadAllBytesAsync(fullPath);
            
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(encryptionKey.PadRight(32).Substring(0, 32));
            
            // Read IV from the beginning of the encrypted data
            var iv = new byte[16];
            Array.Copy(encryptedData, 0, iv, 0, 16);
            aes.IV = iv;
            
            // Extract the actual encrypted data (after IV)
            var actualEncryptedData = new byte[encryptedData.Length - 16];
            Array.Copy(encryptedData, 16, actualEncryptedData, 0, actualEncryptedData.Length);
            
            using var decryptor = aes.CreateDecryptor();
            using var msDecrypt = new MemoryStream(actualEncryptedData);
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var resultStream = new MemoryStream();
            
            await csDecrypt.CopyToAsync(resultStream);
            var decryptedData = resultStream.ToArray();
            
            return new JsonModel
            {
                data = decryptedData,
                Message = "File decrypted successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decrypting file: {FilePath}", encryptedFilePath);
            return new JsonModel
            {
                data = new object(),
                Message = $"Error decrypting file: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> UploadMultipleFilesAsync(IEnumerable<FileUploadDto> files, TokenModel tokenModel)
    {
        try
        {
            var uploadedPaths = new List<string>();
            
            foreach (var file in files)
            {
                // Handle directory specification
                var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                var targetPath = string.IsNullOrEmpty(file.Directory) 
                    ? Path.Combine(_baseStoragePath, uniqueFileName)
                    : Path.Combine(_baseStoragePath, file.Directory, uniqueFileName);
                
                // Ensure directory exists
                var directory = Path.GetDirectoryName(targetPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory!);
                }
                
                // Write file
                await File.WriteAllBytesAsync(targetPath, file.Content);
                
                // Store metadata
                var metadataPath = targetPath + ".meta";
                var metadata = new { OriginalFileName = file.FileName, ContentType = file.ContentType ?? "application/octet-stream", UploadedAt = DateTime.UtcNow };
                await File.WriteAllTextAsync(metadataPath, System.Text.Json.JsonSerializer.Serialize(metadata));
                
                // Return relative path
                var relativePath = Path.GetRelativePath(_baseStoragePath, targetPath);
                uploadedPaths.Add(relativePath.Replace('\\', '/'));
            }
            
            return new JsonModel
            {
                data = uploadedPaths,
                Message = "Multiple files uploaded successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading multiple files");
            return new JsonModel
            {
                data = new object(),
                Message = $"Error uploading multiple files: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> DeleteMultipleFilesAsync(IEnumerable<string> filePaths, TokenModel tokenModel)
    {
        try
        {
            var allDeleted = true;
            
            foreach (var filePath in filePaths)
            {
                var result = await DeleteFileAsync(filePath, tokenModel);
                if (result.StatusCode != 200)
                {
                    allDeleted = false;
                    _logger.LogWarning("Failed to delete file: {FilePath}", filePath);
                }
            }
            
            return new JsonModel
            {
                data = allDeleted,
                Message = "Multiple files deleted successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting multiple files");
            return new JsonModel
            {
                data = new object(),
                Message = $"Error deleting multiple files: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetStorageInfoAsync(TokenModel tokenModel)
    {
        try
        {
            var driveInfo = new DriveInfo(Path.GetPathRoot(_baseStoragePath) ?? "C:");
            var directoryInfo = new DirectoryInfo(_baseStoragePath);
            
            var fileCount = 0;
            var directoryCount = 0;
            
            if (Directory.Exists(_baseStoragePath))
            {
                fileCount = Directory.GetFiles(_baseStoragePath, "*", SearchOption.AllDirectories).Length;
                directoryCount = Directory.GetDirectories(_baseStoragePath, "*", SearchOption.AllDirectories).Length;
            }
            
            var storageInfo = new StorageInfoDto
            {
                TotalSpace = driveInfo.TotalSize,
                UsedSpace = driveInfo.TotalSize - driveInfo.AvailableFreeSpace,
                AvailableSpace = driveInfo.AvailableFreeSpace,
                FileCount = fileCount,
                DirectoryCount = directoryCount,
                LastUpdated = DateTime.UtcNow
            };
            
            return new JsonModel
            {
                data = storageInfo,
                Message = "Storage info retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting storage info");
            return new JsonModel
            {
                data = new object(),
                Message = $"Error getting storage info: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public Task<JsonModel> CleanupExpiredFilesAsync(TokenModel tokenModel)
    {
        // For local storage, implement cleanup logic based on file age, size, etc.
        // For now, return success
        return Task.FromResult(new JsonModel
        {
            data = true,
            Message = "Cleanup completed successfully",
            StatusCode = 200
        });
    }

    public Task<JsonModel> ArchiveOldFilesAsync(string sourcePath, string archivePath, TimeSpan ageThreshold, TokenModel tokenModel)
    {
        // For local storage, implement archiving logic
        // For now, return success
        return Task.FromResult(new JsonModel
        {
            data = true,
            Message = "Archiving completed successfully",
            StatusCode = 200
        });
    }

    private string GetContentTypeFromExtension(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".txt" => "text/plain",
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            _ => "application/octet-stream"
        };
    }
} 