using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.InMemory;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.API;

public static class DatabaseRegistration
{
    public static void RegisterDatabaseProvider(IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        var isTestEnvironment = IsTestEnvironment(environment, configuration);
        
        if (isTestEnvironment)
        {
            // For tests, use in-memory database
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid().ToString()));
        }
        else
        {
            // For production/development, use SQL Server
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly("SmartTelehealth.Infrastructure")));
        }
    }

    private static bool IsTestEnvironment(IWebHostEnvironment environment, IConfiguration configuration)
    {
        var environmentName = environment.EnvironmentName;
        var aspNetCoreEnvironment = configuration["ASPNETCORE_ENVIRONMENT"];
        
        return string.Equals(environmentName, "Test", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(aspNetCoreEnvironment, "Test", StringComparison.OrdinalIgnoreCase) ||
               environment.IsEnvironment("Test") ||
               environmentName?.Contains("Test", StringComparison.OrdinalIgnoreCase) == true ||
               aspNetCoreEnvironment?.Contains("Test", StringComparison.OrdinalIgnoreCase) == true;
    }
} 