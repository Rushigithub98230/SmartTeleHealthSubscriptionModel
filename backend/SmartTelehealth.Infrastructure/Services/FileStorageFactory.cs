using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartTelehealth.Application.Interfaces;

namespace SmartTelehealth.Infrastructure.Services;

public class FileStorageFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    public FileStorageFactory(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    public IFileStorageService CreateFileStorageService()
    {
        var provider = _configuration["FileStorage:Provider"]?.ToLowerInvariant() ?? "local";
        return CreateFileStorageService(provider);
    }

    public IFileStorageService CreateFileStorageService(string provider)
    {
        return provider.ToLowerInvariant() switch
        {
            "local" => _serviceProvider.GetRequiredService<LocalFileStorageService>(),
            // "azure" => _serviceProvider.GetRequiredService<AzureBlobStorageService>(),
            // "aws" => _serviceProvider.GetRequiredService<AwsS3StorageService>(),
            _ => _serviceProvider.GetRequiredService<LocalFileStorageService>() // Default to local
        };
    }
} 