using Microsoft.AspNetCore.Http;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IFileStorageService
{
    // Core file operations
    Task<JsonModel> UploadFileAsync(byte[] fileData, string fileName, string contentType, TokenModel tokenModel);
    Task<JsonModel> DownloadFileAsync(string filePath, TokenModel tokenModel);
    Task<JsonModel> DeleteFileAsync(string filePath, TokenModel tokenModel);
    Task<JsonModel> FileExistsAsync(string filePath, TokenModel tokenModel);
    Task<JsonModel> GetFileSizeAsync(string filePath, TokenModel tokenModel);
    Task<JsonModel> GetFileUrlAsync(string filePath, TokenModel tokenModel);
    
    // File metadata
    Task<JsonModel> GetFileInfoAsync(string filePath, TokenModel tokenModel);
    Task<JsonModel> GetSecureUrlAsync(string filePath, TimeSpan? expiration, TokenModel tokenModel);
    
    // Directory operations
    Task<JsonModel> CreateDirectoryAsync(string directoryPath, TokenModel tokenModel);
    Task<JsonModel> DeleteDirectoryAsync(string directoryPath, TokenModel tokenModel);
    Task<JsonModel> ListFilesAsync(string directoryPath, string? searchPattern, TokenModel tokenModel);
    
    // Security and access control
    Task<JsonModel> ValidateFileAccessAsync(string filePath, Guid userId, TokenModel tokenModel);
    Task<JsonModel> SetFilePermissionsAsync(string filePath, FilePermissions permissions, TokenModel tokenModel);
    
    // Encryption
    Task<JsonModel> EncryptFileAsync(byte[] fileData, string encryptionKey, TokenModel tokenModel);
    Task<JsonModel> DecryptFileAsync(string encryptedFilePath, string encryptionKey, TokenModel tokenModel);
    
    // Batch operations
    Task<JsonModel> UploadMultipleFilesAsync(IEnumerable<FileUploadDto> files, TokenModel tokenModel);
    Task<JsonModel> DeleteMultipleFilesAsync(IEnumerable<string> filePaths, TokenModel tokenModel);
    
    // Storage management
    Task<JsonModel> GetStorageInfoAsync(TokenModel tokenModel);
    Task<JsonModel> CleanupExpiredFilesAsync(TokenModel tokenModel);
    Task<JsonModel> ArchiveOldFilesAsync(string sourcePath, string archivePath, TimeSpan ageThreshold, TokenModel tokenModel);
} 