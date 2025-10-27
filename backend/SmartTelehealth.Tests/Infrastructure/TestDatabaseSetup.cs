using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartTelehealth.Infrastructure.Data;
using SmartTelehealth.Core.Entities;
using System.Data.SqlClient;

namespace SmartTelehealth.Tests.Infrastructure;

/// <summary>
/// Comprehensive test database setup with real SQL Server database,
/// migrations, and proper seed data for testing all business logic.
/// </summary>
public class TestDatabaseSetup : IDisposable
{
    private readonly string _connectionString;
    private readonly ILogger<TestDatabaseSetup> _logger;
    private readonly ApplicationDbContext _context;

    public TestDatabaseSetup(ILogger<TestDatabaseSetup> logger)
    {
        _logger = logger;
        _connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=SmartTelehealth_Test;Trusted_Connection=true;MultipleActiveResultSets=true";
        
        // Create database context
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(_connectionString)
            .Options;
        
        _context = new ApplicationDbContext(options);
    }

    /// <summary>
    /// Sets up the test database with migrations and seed data
    /// </summary>
    public async Task SetupAsync()
    {
        try
        {
            _logger.LogInformation("Setting up test database...");

            // Step 1: Ensure database exists
            await EnsureDatabaseExistsAsync();

            // Step 2: Apply all migrations
            await ApplyMigrationsAsync();

            // Step 3: Clear existing data
            await ClearTestDataAsync();

            // Step 4: Seed master data
            await SeedMasterDataAsync();

            _logger.LogInformation("Test database setup completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting up test database");
            throw;
        }
    }

    /// <summary>
    /// Ensures the test database exists
    /// </summary>
    private async Task EnsureDatabaseExistsAsync()
    {
        try
        {
            using var connection = new SqlConnection(_connectionString.Replace("Database=SmartTelehealth_Test", "Database=master"));
            await connection.OpenAsync();

            var command = new SqlCommand(
                "IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'SmartTelehealth_Test') " +
                "CREATE DATABASE SmartTelehealth_Test", connection);

            await command.ExecuteNonQueryAsync();
            _logger.LogInformation("Test database ensured to exist");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ensuring test database exists");
            throw;
        }
    }

    /// <summary>
    /// Applies all migrations to the test database
    /// </summary>
    private async Task ApplyMigrationsAsync()
    {
        try
        {
            // Ensure database is created
            await _context.Database.EnsureCreatedAsync();
            
            // Apply any pending migrations
            var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                _logger.LogInformation("Applying {Count} pending migrations", pendingMigrations.Count());
                await _context.Database.MigrateAsync();
            }
            else
            {
                _logger.LogInformation("No pending migrations to apply");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying migrations");
            throw;
        }
    }

    /// <summary>
    /// Clears all test data from the database
    /// </summary>
    private async Task ClearTestDataAsync()
    {
        try
        {
            _logger.LogInformation("Clearing test data...");

            // Clear all tables in reverse dependency order
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM UserSubscriptionPrivilegeUsages");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM SubscriptionPlanPrivileges");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM SubscriptionStatusHistories");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM SubscriptionPayments");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM BillingAdjustments");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM BillingRecords");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM PaymentRefunds");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM ProcessedWebhookEvents");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM Subscriptions");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM SubscriptionPlans");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM Users");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM Privileges");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM Categories");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM MasterBillingCycles");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM MasterCurrencies");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM MasterPrivilegeTypes");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM SystemSettings");

            _logger.LogInformation("Test data cleared successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing test data");
            throw;
        }
    }

    /// <summary>
    /// Seeds master data required for testing
    /// </summary>
    private async Task SeedMasterDataAsync()
    {
        try
        {
            _logger.LogInformation("Seeding master data...");

            // Seed billing cycles
            var billingCycles = new[]
            {
                new MasterBillingCycle
                {
                    Id = Guid.NewGuid(),
                    Name = "monthly",
                    DurationInDays = 30,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                },
                new MasterBillingCycle
                {
                    Id = Guid.NewGuid(),
                    Name = "quarterly",
                    DurationInDays = 90,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                },
                new MasterBillingCycle
                {
                    Id = Guid.NewGuid(),
                    Name = "annual",
                    DurationInDays = 365,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                }
            };

            _context.MasterBillingCycles.AddRange(billingCycles);

            // Seed currencies
            var currencies = new[]
            {
                new MasterCurrency
                {
                    Id = Guid.NewGuid(),
                    Code = "USD",
                    Name = "US Dollar",
                    Symbol = "$",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                },
                new MasterCurrency
                {
                    Id = Guid.NewGuid(),
                    Code = "EUR",
                    Name = "Euro",
                    Symbol = "€",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                },
                new MasterCurrency
                {
                    Id = Guid.NewGuid(),
                    Code = "GBP",
                    Name = "British Pound",
                    Symbol = "£",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                }
            };

            _context.MasterCurrencies.AddRange(currencies);

            // Seed privilege types
            var privilegeTypes = new[]
            {
                new MasterPrivilegeType
                {
                    Id = Guid.NewGuid(),
                    Name = "Video Call",
                    Description = "Video call privilege",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                },
                new MasterPrivilegeType
                {
                    Id = Guid.NewGuid(),
                    Name = "Message",
                    Description = "Message privilege",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                },
                new MasterPrivilegeType
                {
                    Id = Guid.NewGuid(),
                    Name = "Prescription",
                    Description = "Prescription privilege",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                },
                new MasterPrivilegeType
                {
                    Id = Guid.NewGuid(),
                    Name = "Consultation",
                    Description = "Medical consultation privilege",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                }
            };

            _context.MasterPrivilegeTypes.AddRange(privilegeTypes);

            // Seed categories
            var categories = new[]
            {
                new Category
                {
                    Id = Guid.NewGuid(),
                    Name = "Mental Health",
                    Description = "Mental health related plans",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                },
                new Category
                {
                    Id = Guid.NewGuid(),
                    Name = "Physical Health",
                    Description = "Physical health related plans",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                },
                new Category
                {
                    Id = Guid.NewGuid(),
                    Name = "General Health",
                    Description = "General health related plans",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                }
            };

            _context.Categories.AddRange(categories);

            // Seed system settings
            var systemSettings = new SystemSettings
            {
                Id = Guid.NewGuid(),
                DefaultAdminCommissionPercent = 10.0m,
                DefaultCurrencyId = currencies[0].Id, // USD
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            _context.SystemSettings.Add(systemSettings);

            // Seed privileges
            var privileges = new[]
            {
                new Privilege
                {
                    Id = Guid.NewGuid(),
                    Name = "Video Consultation",
                    Description = "One-on-one video consultation with healthcare provider",
                    BaseCost = 25.00m,
                    UnitCost = 30.00m, // Overage cost
                    PrivilegeTypeId = privilegeTypes[0].Id,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                },
                new Privilege
                {
                    Id = Guid.NewGuid(),
                    Name = "Text Messaging",
                    Description = "Unlimited text messaging with healthcare provider",
                    BaseCost = 5.00m,
                    UnitCost = 0.50m, // Per message overage
                    PrivilegeTypeId = privilegeTypes[1].Id,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                },
                new Privilege
                {
                    Id = Guid.NewGuid(),
                    Name = "Prescription Management",
                    Description = "Prescription writing and management",
                    BaseCost = 15.00m,
                    UnitCost = 20.00m, // Per prescription overage
                    PrivilegeTypeId = privilegeTypes[2].Id,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                },
                new Privilege
                {
                    Id = Guid.NewGuid(),
                    Name = "General Consultation",
                    Description = "General medical consultation",
                    BaseCost = 20.00m,
                    UnitCost = 25.00m, // Overage cost
                    PrivilegeTypeId = privilegeTypes[3].Id,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                }
            };

            _context.Privileges.AddRange(privileges);

            // Save all changes
            await _context.SaveChangesAsync();

            _logger.LogInformation("Master data seeded successfully: " +
                "{BillingCycles} billing cycles, {Currencies} currencies, {PrivilegeTypes} privilege types, " +
                "{Categories} categories, {Privileges} privileges, 1 system setting",
                billingCycles.Length, currencies.Length, privilegeTypes.Length, 
                categories.Length, privileges.Length);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding master data");
            throw;
        }
    }

    /// <summary>
    /// Gets the database context for testing
    /// </summary>
    public ApplicationDbContext GetContext()
    {
        return _context;
    }

    /// <summary>
    /// Gets master data for testing
    /// </summary>
    public async Task<MasterData> GetMasterDataAsync()
    {
        var billingCycles = await _context.MasterBillingCycles.ToListAsync();
        var currencies = await _context.MasterCurrencies.ToListAsync();
        var privilegeTypes = await _context.MasterPrivilegeTypes.ToListAsync();
        var categories = await _context.Categories.ToListAsync();
        var privileges = await _context.Privileges.ToListAsync();
        var systemSettings = await _context.SystemSettings.FirstAsync();

        return new MasterData
        {
            BillingCycles = billingCycles,
            Currencies = currencies,
            PrivilegeTypes = privilegeTypes,
            Categories = categories,
            Privileges = privileges,
            SystemSettings = systemSettings
        };
    }

    /// <summary>
    /// Disposes resources
    /// </summary>
    public void Dispose()
    {
        _context?.Dispose();
    }
}

/// <summary>
/// Container for master data used in tests
/// </summary>
public class MasterData
{
    public List<MasterBillingCycle> BillingCycles { get; set; } = new();
    public List<MasterCurrency> Currencies { get; set; } = new();
    public List<MasterPrivilegeType> PrivilegeTypes { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public List<Privilege> Privileges { get; set; } = new();
    public SystemSettings SystemSettings { get; set; } = new();
}
