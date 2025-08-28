using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IChatStorageFactory
{
    IChatStorageService CreateStorageService(string storageType);
} 