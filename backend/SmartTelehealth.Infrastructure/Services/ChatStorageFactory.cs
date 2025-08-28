using SmartTelehealth.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SmartTelehealth.Infrastructure.Services;

public class ChatStorageFactory : IChatStorageFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    public ChatStorageFactory(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    public IChatStorageService CreateStorageService(string storageType)
    {
        storageType = storageType?.ToLowerInvariant() ?? "local";
        return storageType switch
        {
            "cloud" => _serviceProvider.GetRequiredService<CloudChatStorageService>(),
            "local" => _serviceProvider.GetRequiredService<LocalChatStorageService>(),
            _ => _serviceProvider.GetRequiredService<LocalChatStorageService>()
        };
    }

    public string GetCurrentStorageType()
    {
        return _configuration["ChatStorage:Type"] ?? "Local";
    }
} 