using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Application.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using SmartTelehealth.Core.Interfaces;

namespace SmartTelehealth.Infrastructure.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    private readonly UserManager<User> _userManager;
    private readonly IUserRoleRepository _userRoleRepository;
    
    public JwtService(IConfiguration configuration, UserManager<User> userManager, IUserRoleRepository userRoleRepository)
    {
        _configuration = configuration;
        _userManager = userManager;
        _userRoleRepository = userRoleRepository;
    }
    
    public async Task<string> GenerateTokenAsync(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"] ?? "default-secret-key-for-development");
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email ?? ""),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim("UserId", user.Id.ToString())
        };
        
        // Get user roles from the database
        var userRoles = await _userManager.GetRolesAsync(user);
        foreach (var role in userRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
        
        // If no Identity roles found, check the UserRole via repository (for backward compatibility)
        if (!userRoles.Any() && user.UserRoleId != 0)
        {
            var userRole = await _userRoleRepository.GetByIdAsync(user.UserRoleId);
            if (userRole != null && !string.IsNullOrEmpty(userRole.Name))
            {
                claims.Add(new Claim(ClaimTypes.Role, userRole.Name));
                // Create a new list with the found role for isAdmin check
                var userRolesList = new List<string> { userRole.Name };
                
                // Add isAdmin claim for policy-based authorization
                if (userRolesList.Contains("Admin"))
                {
                    claims.Add(new Claim("isAdmin", "true"));
                }
            }
        }
        else
        {
            // Add isAdmin claim for policy-based authorization (when Identity roles are found)
            if (userRoles.Contains("Admin"))
            {
                claims.Add(new Claim("isAdmin", "true"));
            }
        }
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(24),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            ),
            Issuer = _configuration["JwtSettings:Issuer"] ?? "SmartTelehealth",
            Audience = _configuration["JwtSettings:Audience"] ?? "SmartTelehealth"
        };
        
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public string GenerateToken(User user)
    {
        // Synchronous wrapper for the async method
        return GenerateTokenAsync(user).GetAwaiter().GetResult();
    }
    
    public string GenerateRefreshToken()
    {
        return Guid.NewGuid().ToString();
    }
    
    public ClaimsPrincipal? GetPrincipalFromToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"] ?? "default-secret-key-for-development");
        
        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["JwtSettings:Issuer"] ?? "SmartTelehealth",
                ValidateAudience = true,
                ValidAudience = _configuration["JwtSettings:Audience"] ?? "SmartTelehealth",
                ValidateLifetime = false // We want to validate the token even if it's expired
            }, out SecurityToken validatedToken);
            
            return principal;
        }
        catch
        {
            return null;
        }
    }
    
    public bool ValidateToken(string token)
    {
        var principal = GetPrincipalFromToken(token);
        return principal != null;
    }
    
    public string? GetUserIdFromToken(string token)
    {
        var principal = GetPrincipalFromToken(token);
        return principal?.FindFirst("UserId")?.Value;
    }
    
    public string? GetEmailFromToken(string token)
    {
        var principal = GetPrincipalFromToken(token);
        return principal?.FindFirst(ClaimTypes.Email)?.Value;
    }
} 