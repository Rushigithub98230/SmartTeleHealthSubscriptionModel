using SmartTelehealth.Core.Entities;
using SmartTelehealth.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace SmartTelehealth.Tests.Integration.Billing;

/// <summary>
/// Builder class for creating consistent test data across billing tests
/// </summary>
public class TestDataBuilder
{
    private readonly ApplicationDbContext _context;
    private readonly int _adminUserId;

    public TestDataBuilder(ApplicationDbContext context, int adminUserId = 1)
    {
        _context = context;
        _adminUserId = adminUserId;
    }

    /// <summary>
    /// Creates a complete test environment with all required master data
    /// </summary>
    public async Task<TestEnvironment> CreateCompleteTestEnvironmentAsync()
    {
        var masterData = await CreateMasterDataAsync();
        var privileges = await CreateStandardPrivilegesAsync(masterData.PrivilegeTypes);
        var plans = await CreateStandardPlansAsync(masterData, privileges);
        
        return new TestEnvironment
        {
            MasterData = masterData,
            Privileges = privileges,
            Plans = plans
        };
    }

    /// <summary>
    /// Creates master data required for billing tests
    /// </summary>
    public async Task<MasterData> CreateMasterDataAsync()
    {
        var billingCycles = new List<MasterBillingCycle>
        {
            new MasterBillingCycle
            {
                Id = Guid.NewGuid(),
                Name = "Daily",
                Description = "Daily billing cycle",
                DurationInDays = 1,
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = _adminUserId
            },
            new MasterBillingCycle
            {
                Id = Guid.NewGuid(),
                Name = "Weekly",
                Description = "Weekly billing cycle",
                DurationInDays = 7,
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = _adminUserId
            },
            new MasterBillingCycle
            {
                Id = Guid.NewGuid(),
                Name = "Monthly",
                Description = "Monthly billing cycle",
                DurationInDays = 30,
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = _adminUserId
            },
            new MasterBillingCycle
            {
                Id = Guid.NewGuid(),
                Name = "Quarterly",
                Description = "Quarterly billing cycle",
                DurationInDays = 90,
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = _adminUserId
            },
            new MasterBillingCycle
            {
                Id = Guid.NewGuid(),
                Name = "Yearly",
                Description = "Yearly billing cycle",
                DurationInDays = 365,
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = _adminUserId
            }
        };

        var currencies = new List<MasterCurrency>
        {
            new MasterCurrency
            {
                Id = Guid.NewGuid(),
                Name = "USD",
                Symbol = "$",
                Code = "USD",
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = _adminUserId
            },
            new MasterCurrency
            {
                Id = Guid.NewGuid(),
                Name = "EUR",
                Symbol = "€",
                Code = "EUR",
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = _adminUserId
            },
            new MasterCurrency
            {
                Id = Guid.NewGuid(),
                Name = "GBP",
                Symbol = "£",
                Code = "GBP",
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = _adminUserId
            }
        };

        var privilegeTypes = new List<MasterPrivilegeType>
        {
            new MasterPrivilegeType
            {
                Id = Guid.NewGuid(),
                Name = "Consultation",
                Description = "Medical consultation services",
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = _adminUserId
            },
            new MasterPrivilegeType
            {
                Id = Guid.NewGuid(),
                Name = "Medication",
                Description = "Medication delivery services",
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = _adminUserId
            },
            new MasterPrivilegeType
            {
                Id = Guid.NewGuid(),
                Name = "Follow-up",
                Description = "Follow-up care services",
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = _adminUserId
            }
        };

        _context.MasterBillingCycles.AddRange(billingCycles);
        _context.MasterCurrencies.AddRange(currencies);
        _context.MasterPrivilegeTypes.AddRange(privilegeTypes);
        await _context.SaveChangesAsync();

        return new MasterData
        {
            BillingCycles = billingCycles,
            Currencies = currencies,
            PrivilegeTypes = privilegeTypes
        };
    }

    /// <summary>
    /// Creates standard privileges for testing
    /// </summary>
    public async Task<List<Privilege>> CreateStandardPrivilegesAsync(List<MasterPrivilegeType> privilegeTypes)
    {
        var privileges = new List<Privilege>
        {
            // Consultation privileges
            new Privilege
            {
                Id = Guid.NewGuid(),
                Name = "Basic Consultation",
                Description = "Basic medical consultation (15 minutes)",
                PrivilegeTypeId = privilegeTypes[0].Id,
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = _adminUserId
            },
            new Privilege
            {
                Id = Guid.NewGuid(),
                Name = "Extended Consultation",
                Description = "Extended medical consultation (30 minutes)",
                PrivilegeTypeId = privilegeTypes[0].Id,
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = _adminUserId
            },
            new Privilege
            {
                Id = Guid.NewGuid(),
                Name = "Specialist Consultation",
                Description = "Specialist medical consultation (45 minutes)",
                PrivilegeTypeId = privilegeTypes[0].Id,
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = _adminUserId
            },

            // Medication privileges
            new Privilege
            {
                Id = Guid.NewGuid(),
                Name = "Standard Medication Delivery",
                Description = "Standard medication delivery service",
                PrivilegeTypeId = privilegeTypes[1].Id,
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = _adminUserId
            },
            new Privilege
            {
                Id = Guid.NewGuid(),
                Name = "Express Medication Delivery",
                Description = "Express medication delivery service",
                PrivilegeTypeId = privilegeTypes[1].Id,
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = _adminUserId
            },

            // Follow-up privileges
            new Privilege
            {
                Id = Guid.NewGuid(),
                Name = "Follow-up Care",
                Description = "Follow-up medical care service",
                PrivilegeTypeId = privilegeTypes[2].Id,
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = _adminUserId
            }
        };

        _context.Privileges.AddRange(privileges);
        await _context.SaveChangesAsync();

        return privileges;
    }

    /// <summary>
    /// Creates standard subscription plans for testing
    /// </summary>
    public async Task<List<SubscriptionPlan>> CreateStandardPlansAsync(MasterData masterData, List<Privilege> privileges)
    {
        var plans = new List<SubscriptionPlan>
        {
            // Basic Plan
            new SubscriptionPlan
            {
                Id = Guid.NewGuid(),
                Name = "Basic Health Plan",
                Description = "Basic health plan with essential services",
                ShortDescription = "Essential health services",
                Price = 29.99m,
                BillingCycleId = masterData.BillingCycles[2].Id, // Monthly
                CurrencyId = masterData.Currencies[0].Id, // USD
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = _adminUserId
            },

            // Premium Plan
            new SubscriptionPlan
            {
                Id = Guid.NewGuid(),
                Name = "Premium Health Plan",
                Description = "Premium health plan with comprehensive services",
                ShortDescription = "Comprehensive health services",
                Price = 99.99m,
                BillingCycleId = masterData.BillingCycles[2].Id, // Monthly
                CurrencyId = masterData.Currencies[0].Id, // USD
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = _adminUserId
            },

            // Professional Plan
            new SubscriptionPlan
            {
                Id = Guid.NewGuid(),
                Name = "Professional Health Plan",
                Description = "Professional health plan with high limits and premium services",
                ShortDescription = "High-limit professional services",
                Price = 199.99m,
                BillingCycleId = masterData.BillingCycles[4].Id, // Yearly
                CurrencyId = masterData.Currencies[0].Id, // USD
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = _adminUserId
            }
        };

        _context.SubscriptionPlans.AddRange(plans);

        // Add plan privileges
        var planPrivileges = new List<SubscriptionPlanPrivilege>();

        // Basic Plan privileges
        planPrivileges.AddRange(new[]
        {
            new SubscriptionPlanPrivilege
            {
                Id = Guid.NewGuid(),
                SubscriptionPlanId = plans[0].Id,
                PrivilegeId = privileges[0].Id, // Basic Consultation
                DailyLimit = 2,
                UnitCost = 15.00m,
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = _adminUserId
            },
            new SubscriptionPlanPrivilege
            {
                Id = Guid.NewGuid(),
                SubscriptionPlanId = plans[0].Id,
                PrivilegeId = privileges[3].Id, // Standard Medication Delivery
                DailyLimit = 1,
                UnitCost = 25.00m,
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = _adminUserId
            }
        });

        // Premium Plan privileges
        planPrivileges.AddRange(new[]
        {
            new SubscriptionPlanPrivilege
            {
                Id = Guid.NewGuid(),
                SubscriptionPlanId = plans[1].Id,
                PrivilegeId = privileges[1].Id, // Extended Consultation
                DailyLimit = 5,
                UnitCost = 20.00m,
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = _adminUserId
            },
            new SubscriptionPlanPrivilege
            {
                Id = Guid.NewGuid(),
                SubscriptionPlanId = plans[1].Id,
                PrivilegeId = privileges[4].Id, // Express Medication Delivery
                DailyLimit = 3,
                UnitCost = 35.00m,
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = _adminUserId
            },
            new SubscriptionPlanPrivilege
            {
                Id = Guid.NewGuid(),
                SubscriptionPlanId = plans[1].Id,
                PrivilegeId = privileges[5].Id, // Follow-up Care
                DailyLimit = 2,
                UnitCost = 30.00m,
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = _adminUserId
            }
        });

        // Professional Plan privileges (high limits with overage charges - no unlimited privileges)
        planPrivileges.AddRange(new[]
        {
            new SubscriptionPlanPrivilege
            {
                Id = Guid.NewGuid(),
                SubscriptionPlanId = plans[2].Id,
                PrivilegeId = privileges[2].Id, // Specialist Consultation
                DailyLimit = 20, // High limit but not unlimited
                UnitCost = 50.00m,
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = _adminUserId
            },
            new SubscriptionPlanPrivilege
            {
                Id = Guid.NewGuid(),
                SubscriptionPlanId = plans[2].Id,
                PrivilegeId = privileges[4].Id, // Express Medication Delivery
                DailyLimit = 15, // High limit but not unlimited
                UnitCost = 35.00m,
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = _adminUserId
            },
            new SubscriptionPlanPrivilege
            {
                Id = Guid.NewGuid(),
                SubscriptionPlanId = plans[2].Id,
                PrivilegeId = privileges[5].Id, // Follow-up Care
                DailyLimit = 10, // High limit but not unlimited
                UnitCost = 30.00m,
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = _adminUserId
            }
        });

        _context.SubscriptionPlanPrivileges.AddRange(planPrivileges);
        await _context.SaveChangesAsync();

        return plans;
    }

    /// <summary>
    /// Creates test users with different roles
    /// </summary>
    public async Task<List<User>> CreateTestUsersAsync(int count = 5)
    {
        var users = new List<User>();
        
        for (int i = 1; i <= count; i++)
        {
            var user = new User
            {
                Id = 100 + i,
                FirstName = $"Test",
                LastName = $"User{i}",
                Email = $"testuser{i}@example.com",
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = _adminUserId
            };
            users.Add(user);
        }

        _context.Users.AddRange(users);
        await _context.SaveChangesAsync();

        return users;
    }

    /// <summary>
    /// Creates a subscription for a user
    /// </summary>
    public async Task<Subscription> CreateUserSubscriptionAsync(User user, SubscriptionPlan plan, MasterBillingCycle billingCycle)
    {
        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            SubscriptionPlanId = plan.Id,
            BillingCycleId = billingCycle.Id,
            Status = Subscription.SubscriptionStatuses.Active,
            StartDate = DateTime.UtcNow,
            NextBillingDate = DateTime.UtcNow.AddDays(billingCycle.DurationInDays),
            CurrentPrice = plan.Price,
            IsActive = true,
            IsDeleted = false,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = _adminUserId
        };

        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        return subscription;
    }

    /// <summary>
    /// Creates privilege usage tracking for a subscription
    /// </summary>
    public async Task<List<UserSubscriptionPrivilegeUsage>> CreatePrivilegeUsageTrackingAsync(Subscription subscription)
    {
        var planPrivileges = await _context.SubscriptionPlanPrivileges
            .Where(pp => pp.SubscriptionPlanId == subscription.SubscriptionPlanId)
            .ToListAsync();

        var usages = new List<UserSubscriptionPrivilegeUsage>();

        foreach (var planPrivilege in planPrivileges)
        {
            var usage = new UserSubscriptionPrivilegeUsage
            {
                Id = Guid.NewGuid(),
                SubscriptionId = subscription.Id,
                SubscriptionPlanPrivilegeId = planPrivilege.Id,
                PrivilegeId = planPrivilege.PrivilegeId,
                UsedValue = 0,
                ResetAt = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = _adminUserId
            };
            usages.Add(usage);
        }

        _context.UserSubscriptionPrivilegeUsages.AddRange(usages);
        await _context.SaveChangesAsync();

        return usages;
    }

    /// <summary>
    /// Creates billing records for testing
    /// </summary>
    public async Task<List<BillingRecord>> CreateTestBillingRecordsAsync(int userId, int count = 3)
    {
        var billingRecords = new List<BillingRecord>();

        for (int i = 0; i < count; i++)
        {
            var billingRecord = new BillingRecord
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Amount = 50.00m + (i * 25.00m),
                TotalAmount = 50.00m + (i * 25.00m),
                TaxAmount = 0,
                ShippingAmount = 0,
                Status = i == 0 ? BillingRecord.BillingStatus.Paid : BillingRecord.BillingStatus.Pending,
                Type = BillingRecord.BillingType.Subscription,
                Description = $"Test billing record {i + 1}",
                BillingDate = DateTime.UtcNow.AddDays(-i),
                DueDate = DateTime.UtcNow.AddDays(30 - i),
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = _adminUserId
            };

            if (i == 0)
            {
                billingRecord.PaidAt = DateTime.UtcNow.AddDays(-i);
                billingRecord.ProcessedAt = DateTime.UtcNow.AddDays(-i);
            }

            billingRecords.Add(billingRecord);
        }

        _context.BillingRecords.AddRange(billingRecords);
        await _context.SaveChangesAsync();

        return billingRecords;
    }
}

/// <summary>
/// Container for test environment data
/// </summary>
public class TestEnvironment
{
    public MasterData MasterData { get; set; } = null!;
    public List<Privilege> Privileges { get; set; } = new();
    public List<SubscriptionPlan> Plans { get; set; } = new();
}

/// <summary>
/// Container for master data
/// </summary>
public class MasterData
{
    public List<MasterBillingCycle> BillingCycles { get; set; } = new();
    public List<MasterCurrency> Currencies { get; set; } = new();
    public List<MasterPrivilegeType> PrivilegeTypes { get; set; } = new();
}
