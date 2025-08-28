// Temporarily commented out to fix build errors - will be implemented after LocalFileStorageService testing
/*
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using System.Security.Cryptography;
using System.Text;

namespace SmartTelehealth.Infrastructure.Services;

public class AwsS3StorageService : IFileStorageService
{
    // Implementation will be added after LocalFileStorageService testing
    public Task<JsonModel> UploadFileAsync(byte[] fileData, string fileName, string contentType) => throw new NotImplementedException();
    public Task<JsonModel> DownloadFileAsync(string filePath) => throw new NotImplementedException();
    public Task<JsonModel> DeleteFileAsync(string filePath) => throw new NotImplementedException();
    public Task<JsonModel> FileExistsAsync(string filePath) => throw new NotImplementedException();
    public Task<JsonModel> GetFileSizeAsync(string filePath) => throw new NotImplementedException();
    public Task<JsonModel> GetFileUrlAsync(string filePath) => throw new NotImplementedException();
    public Task<JsonModel> GetFileInfoAsync(string filePath) => throw new NotImplementedException();
    public Task<JsonModel> GetSecureUrlAsync(string filePath, TimeSpan? expiration = null) => throw new NotImplementedException();
    public Task<JsonModel> CreateDirectoryAsync(string directoryPath) => throw new NotImplementedException();
    public Task<JsonModel> DeleteDirectoryAsync(string directoryPath) => throw new NotImplementedException();
    public Task<JsonModel> ListFilesAsync(string directoryPath, string? searchPattern = null) => throw new NotImplementedException();
    public Task<JsonModel> ValidateFileAccessAsync(string filePath, Guid userId) => throw new NotImplementedException();
    public Task<JsonModel> SetFilePermissionsAsync(string filePath, FilePermissions permissions) => throw new NotImplementedException();
    public Task<JsonModel> EncryptFileAsync(byte[] fileData, string encryptionKey) => throw new NotImplementedException();
    public Task<JsonModel> DecryptFileAsync(string encryptedFilePath, string encryptionKey) => throw new NotImplementedException();
    public Task<JsonModel> UploadMultipleFilesAsync(IEnumerable<FileUploadDto> files) => throw new NotImplementedException();
    public Task<JsonModel> DeleteMultipleFilesAsync(IEnumerable<string> filePaths) => throw new NotImplementedException();
    public Task<JsonModel> GetStorageInfoAsync() => throw new NotImplementedException();
    public Task<JsonModel> CleanupExpiredFilesAsync() => throw new NotImplementedException();
    public Task<JsonModel> ArchiveOldFilesAsync(string sourcePath, string archivePath, TimeSpan ageThreshold) => throw new NotImplementedException();
}
*/ 