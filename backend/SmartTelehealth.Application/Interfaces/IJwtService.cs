using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Application.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(User user);
        bool ValidateToken(string token);
        string GetUserIdFromToken(string token);
    }
} 