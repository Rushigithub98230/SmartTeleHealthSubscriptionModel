using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SmartTelehealth.Infrastructure.Data;
using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Infrastructure;

public class SeedDatabase
{
    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        
        using (var scope = host.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            // Ensure database is created
            await context.Database.EnsureCreatedAsync();
            
            // Seed the data
            SeedData.SeedMasterTables(context);
            
            Console.WriteLine("Database seeded successfully!");
        }
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer("Server=SDN-153\\SQLEXPRESS2022;Database=SmartTelehealthDb;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true"));
            });
}
