using SmartTelehealth.Core.Entities;
using SmartTelehealth.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace SmartTelehealth.Tests.Infrastructure;

/// <summary>
/// Builder pattern for creating test data entities with realistic defaults.
/// Provides fluent API for building test entities with proper relationships.
/// </summary>
public class TestDataBuilder
{
    private readonly ApplicationDbContext _context;
    private readonly MasterData _masterData;
    private readonly Random _random = new Random();

    public TestDataBuilder(ApplicationDbContext context, MasterData masterData)
    {
        _context = context;
        _masterData = masterData;
    }

    #region User Builder

    public UserBuilder User()
    {
        return new UserBuilder(_context);
    }

    public class UserBuilder
    {
        private readonly ApplicationDbContext _context;
        private readonly User _user;

        public UserBuilder(ApplicationDbContext context)
        {
            _context = context;
            _user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                PhoneNumber = "+1234567890",
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };
        }

        public UserBuilder WithEmail(string email)
        {
            _user.Email = email;
            return this;
        }

        public UserBuilder WithName(string firstName, string lastName)
        {
            _user.FirstName = firstName;
            _user.LastName = lastName;
            return this;
        }

        public UserBuilder WithPhone(string phoneNumber)
        {
            _user.PhoneNumber = phoneNumber;
            return this;
        }

        public UserBuilder WithRole(int roleId)
        {
            _user.RoleId = roleId;
            return this;
        }

        public UserBuilder AsInactive()
        {
            _user.IsActive = false;
            return this;
        }

        public UserBuilder AsDeleted()
        {
            _user.IsDeleted = true;
            _user.DeletedDate = DateTime.UtcNow;
            return this;
        }

        public async Task<User> BuildAsync()
        {
            _context.Users.Add(_user);
            await _context.SaveChangesAsync();
            return _user;
        }

        public User Build()
        {
            _context.Users.Add(_user);
            _context.SaveChanges();
            return _user;
        }
    }

    #endregion

    #region Subscription Plan Builder

    public SubscriptionPlanBuilder SubscriptionPlan()
    {
        return new SubscriptionPlanBuilder(_context);
    }

    public class SubscriptionPlanBuilder
    {
        private readonly ApplicationDbContext _context;
        private readonly SubscriptionPlan _plan;

        public SubscriptionPlanBuilder(ApplicationDbContext context)
        {
            _context = context;
            _plan = new SubscriptionPlan
            {
                Id = Guid.NewGuid(),
                Name = "Test Plan",
                Description = "Test subscription plan",
                BasePrice = 99.99m,
                IsActive = true,
                IsDeleted = false,
                IsAutoCalculatedPrice = false,
                AdminCommissionPercent = 10.0m,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };
        }

        public SubscriptionPlanBuilder WithName(string name)
        {
            _plan.Name = name;
            return this;
        }

        public SubscriptionPlanBuilder WithDescription(string description)
        {
            _plan.Description = description;
            return this;
        }

        public SubscriptionPlanBuilder WithBasePrice(decimal basePrice)
        {
            _plan.BasePrice = basePrice;
            return this;
        }

        public SubscriptionPlanBuilder WithAutoCalculatedPrice(bool isAutoCalculated)
        {
            _plan.IsAutoCalculatedPrice = isAutoCalculated;
            return this;
        }

        public SubscriptionPlanBuilder WithAdminCommissionPercent(decimal commissionPercent)
        {
            _plan.AdminCommissionPercent = commissionPercent;
            return this;
        }

        public SubscriptionPlanBuilder WithCategory(Guid categoryId)
        {
            _plan.CategoryId = categoryId;
            return this;
        }

        public SubscriptionPlanBuilder WithDiscountPercentage(decimal discountPercentage, DateTime? validUntil = null)
        {
            _plan.DiscountPercentage = discountPercentage;
            _plan.DiscountValidUntil = validUntil ?? DateTime.UtcNow.AddDays(30);
            return this;
        }

        public SubscriptionPlanBuilder WithBillingCycle(string cycleName)
        {
            var cycle = _context.MasterBillingCycles.FirstOrDefault(c => c.Name == cycleName);
            if (cycle != null)
            {
                _plan.BillingCycleId = cycle.Id;
            }
            return this;
        }

        public SubscriptionPlanBuilder WithCurrency(string currencyCode)
        {
            var currency = _context.MasterCurrencies.FirstOrDefault(c => c.Code == currencyCode);
            if (currency != null)
            {
                _plan.CurrencyId = currency.Id;
            }
            return this;
        }

        public SubscriptionPlanBuilder WithTrial(int trialDays)
        {
            _plan.IsTrialAllowed = true;
            _plan.TrialDurationInDays = trialDays;
            return this;
        }

        public SubscriptionPlanBuilder WithStripeIntegration(string productId, string priceId)
        {
            _plan.StripeProductId = productId;
            _plan.StripePriceId = priceId;
            return this;
        }

        public SubscriptionPlanBuilder AsInactive()
        {
            _plan.IsActive = false;
            return this;
        }

        public SubscriptionPlanBuilder AsDeleted()
        {
            _plan.IsDeleted = true;
            _plan.DeletedDate = DateTime.UtcNow;
            return this;
        }

        public async Task<SubscriptionPlan> BuildAsync()
        {
            _context.SubscriptionPlans.Add(_plan);
            await _context.SaveChangesAsync();
            return _plan;
        }

        public SubscriptionPlan Build()
        {
            _context.SubscriptionPlans.Add(_plan);
            _context.SaveChanges();
            return _plan;
        }
    }

    #endregion

    #region Subscription Builder

    public SubscriptionBuilder Subscription()
    {
        return new SubscriptionBuilder(_context);
    }

    public class SubscriptionBuilder
    {
        private readonly ApplicationDbContext _context;
        private readonly Subscription _subscription;

        public SubscriptionBuilder(ApplicationDbContext context)
        {
            _context = context;
            _subscription = new Subscription
            {
                Id = Guid.NewGuid(),
                Status = Subscription.SubscriptionStatuses.Active,
                CurrentPrice = 99.99m,
                StartDate = DateTime.UtcNow,
                NextBillingDate = DateTime.UtcNow.AddDays(30),
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };
        }

        public SubscriptionBuilder ForUser(User user)
        {
            _subscription.UserId = user.Id;
            return this;
        }

        public SubscriptionBuilder WithPlan(SubscriptionPlan plan)
        {
            _subscription.SubscriptionPlanId = plan.Id;
            _subscription.SubscriptionPlan = plan;
            _subscription.CurrentPrice = plan.Price;
            return this;
        }

        public SubscriptionBuilder WithStatus(Subscription.SubscriptionStatuses status)
        {
            _subscription.Status = status;
            return this;
        }

        public SubscriptionBuilder WithPrice(decimal price)
        {
            _subscription.CurrentPrice = price;
            return this;
        }

        public SubscriptionBuilder WithBillingCycle(string cycleName)
        {
            var cycle = _context.MasterBillingCycles.FirstOrDefault(c => c.Name == cycleName);
            if (cycle != null)
            {
                _subscription.BillingCycleId = cycle.Id;
                _subscription.BillingCycle = cycle;
            }
            return this;
        }

        public SubscriptionBuilder WithTrial(DateTime startDate, DateTime endDate)
        {
            _subscription.IsTrialSubscription = true;
            _subscription.TrialStartDate = startDate;
            _subscription.TrialEndDate = endDate;
            _subscription.TrialDurationInDays = (int)(endDate - startDate).TotalDays;
            _subscription.Status = Subscription.SubscriptionStatuses.TrialActive;
            return this;
        }

        public SubscriptionBuilder WithStripeIntegration(string customerId, string subscriptionId, string priceId)
        {
            _subscription.StripeCustomerId = customerId;
            _subscription.StripeSubscriptionId = subscriptionId;
            _subscription.StripePriceId = priceId;
            return this;
        }

        public SubscriptionBuilder WithNextBillingDate(DateTime nextBillingDate)
        {
            _subscription.NextBillingDate = nextBillingDate;
            return this;
        }

        public SubscriptionBuilder AsInactive()
        {
            _subscription.IsActive = false;
            return this;
        }

        public SubscriptionBuilder AsDeleted()
        {
            _subscription.IsDeleted = true;
            _subscription.DeletedDate = DateTime.UtcNow;
            return this;
        }

        public async Task<Subscription> BuildAsync()
        {
            _context.Subscriptions.Add(_subscription);
            await _context.SaveChangesAsync();
            return _subscription;
        }

        public Subscription Build()
        {
            _context.Subscriptions.Add(_subscription);
            _context.SaveChanges();
            return _subscription;
        }
    }

    #endregion

    #region Billing Record Builder

    public BillingRecordBuilder BillingRecord()
    {
        return new BillingRecordBuilder(_context);
    }

    public class BillingRecordBuilder
    {
        private readonly ApplicationDbContext _context;
        private readonly BillingRecord _billingRecord;

        public BillingRecordBuilder(ApplicationDbContext context)
        {
            _context = context;
            _billingRecord = new BillingRecord
            {
                Id = Guid.NewGuid(),
                Amount = 99.99m,
                TotalAmount = 99.99m,
                Status = BillingRecord.BillingStatus.Pending,
                BillingDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(7),
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };
        }

        public BillingRecordBuilder ForSubscription(Subscription subscription)
        {
            _billingRecord.SubscriptionId = subscription.Id;
            _billingRecord.Subscription = subscription;
            return this;
        }

        public BillingRecordBuilder WithAmount(decimal amount)
        {
            _billingRecord.Amount = amount;
            _billingRecord.TotalAmount = amount;
            return this;
        }

        public BillingRecordBuilder WithStatus(BillingRecord.BillingStatus status)
        {
            _billingRecord.Status = status;
            return this;
        }

        public BillingRecordBuilder WithBillingDate(DateTime billingDate)
        {
            _billingRecord.BillingDate = billingDate;
            return this;
        }

        public BillingRecordBuilder WithDueDate(DateTime dueDate)
        {
            _billingRecord.DueDate = dueDate;
            return this;
        }

        public BillingRecordBuilder AsPaid()
        {
            _billingRecord.Status = BillingRecord.BillingStatus.Paid;
            _billingRecord.ProcessedAt = DateTime.UtcNow;
            return this;
        }

        public BillingRecordBuilder AsFailed()
        {
            _billingRecord.Status = BillingRecord.BillingStatus.Failed;
            _billingRecord.ProcessedAt = DateTime.UtcNow;
            return this;
        }

        public BillingRecordBuilder AsInactive()
        {
            _billingRecord.IsActive = false;
            return this;
        }

        public BillingRecordBuilder AsDeleted()
        {
            _billingRecord.IsDeleted = true;
            _billingRecord.DeletedDate = DateTime.UtcNow;
            return this;
        }

        public async Task<BillingRecord> BuildAsync()
        {
            _context.BillingRecords.Add(_billingRecord);
            await _context.SaveChangesAsync();
            return _billingRecord;
        }

        public BillingRecord Build()
        {
            _context.BillingRecords.Add(_billingRecord);
            _context.SaveChanges();
            return _billingRecord;
        }
    }

    #endregion

    #region Privilege Builder

    public PrivilegeBuilder Privilege()
    {
        return new PrivilegeBuilder(_context);
    }

    public class PrivilegeBuilder
    {
        private readonly ApplicationDbContext _context;
        private readonly Privilege _privilege;

        public PrivilegeBuilder(ApplicationDbContext context)
        {
            _context = context;
            _privilege = new Privilege
            {
                Id = Guid.NewGuid(),
                Name = "Test Privilege",
                Description = "Test privilege description",
                BaseCost = 10.00m,
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };
        }

        public PrivilegeBuilder WithName(string name)
        {
            _privilege.Name = name;
            return this;
        }

        public PrivilegeBuilder WithDescription(string description)
        {
            _privilege.Description = description;
            return this;
        }

        public PrivilegeBuilder WithBaseCost(decimal baseCost)
        {
            _privilege.BaseCost = baseCost;
            return this;
        }

        public PrivilegeBuilder WithType(string typeName)
        {
            var type = _context.MasterPrivilegeTypes.FirstOrDefault(t => t.Name == typeName);
            if (type != null)
            {
                _privilege.PrivilegeTypeId = type.Id;
            }
            return this;
        }

        public PrivilegeBuilder AsInactive()
        {
            _privilege.IsActive = false;
            return this;
        }

        public PrivilegeBuilder AsDeleted()
        {
            _privilege.IsDeleted = true;
            _privilege.DeletedDate = DateTime.UtcNow;
            return this;
        }

        public async Task<Privilege> BuildAsync()
        {
            _context.Privileges.Add(_privilege);
            await _context.SaveChangesAsync();
            return _privilege;
        }

        public Privilege Build()
        {
            _context.Privileges.Add(_privilege);
            _context.SaveChanges();
            return _privilege;
        }
    }

    #endregion

    #region Subscription Plan Privilege Builder

    public SubscriptionPlanPrivilegeBuilder SubscriptionPlanPrivilege()
    {
        return new SubscriptionPlanPrivilegeBuilder(_context);
    }

    public class SubscriptionPlanPrivilegeBuilder
    {
        private readonly ApplicationDbContext _context;
        private readonly SubscriptionPlanPrivilege _planPrivilege;

        public SubscriptionPlanPrivilegeBuilder(ApplicationDbContext context)
        {
            _context = context;
            _planPrivilege = new SubscriptionPlanPrivilege
            {
                Id = Guid.NewGuid(),
                Value = 10,
                PrivilegeBaseCost = 5.00m,
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };
        }

        public SubscriptionPlanPrivilegeBuilder ForPlan(SubscriptionPlan plan)
        {
            _planPrivilege.SubscriptionPlanId = plan.Id;
            _planPrivilege.SubscriptionPlan = plan;
            return this;
        }

        public SubscriptionPlanPrivilegeBuilder WithPrivilege(Privilege privilege)
        {
            _planPrivilege.PrivilegeId = privilege.Id;
            _planPrivilege.Privilege = privilege;
            return this;
        }

        public SubscriptionPlanPrivilegeBuilder WithValue(int value)
        {
            _planPrivilege.Value = value;
            return this;
        }

        public SubscriptionPlanPrivilegeBuilder WithBaseCost(decimal baseCost)
        {
            _planPrivilege.PrivilegeBaseCost = baseCost;
            return this;
        }

        public SubscriptionPlanPrivilegeBuilder AsUnlimited()
        {
            _planPrivilege.Value = -1;
            return this;
        }

        public SubscriptionPlanPrivilegeBuilder AsDisabled()
        {
            _planPrivilege.Value = 0;
            return this;
        }

        public SubscriptionPlanPrivilegeBuilder AsInactive()
        {
            _planPrivilege.IsActive = false;
            return this;
        }

        public SubscriptionPlanPrivilegeBuilder AsDeleted()
        {
            _planPrivilege.IsDeleted = true;
            _planPrivilege.DeletedDate = DateTime.UtcNow;
            return this;
        }

        public async Task<SubscriptionPlanPrivilege> BuildAsync()
        {
            _context.SubscriptionPlanPrivileges.Add(_planPrivilege);
            await _context.SaveChangesAsync();
            return _planPrivilege;
        }

        public SubscriptionPlanPrivilege Build()
        {
            _context.SubscriptionPlanPrivileges.Add(_planPrivilege);
            _context.SaveChanges();
            return _planPrivilege;
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Gets a random billing cycle
    /// </summary>
    public MasterBillingCycle GetRandomBillingCycle()
    {
        return _masterData.BillingCycles[_random.Next(_masterData.BillingCycles.Count)];
    }

    /// <summary>
    /// Gets a random currency
    /// </summary>
    public MasterCurrency GetRandomCurrency()
    {
        return _masterData.Currencies[_random.Next(_masterData.Currencies.Count)];
    }

    /// <summary>
    /// Gets a random privilege type
    /// </summary>
    public MasterPrivilegeType GetRandomPrivilegeType()
    {
        return _masterData.PrivilegeTypes[_random.Next(_masterData.PrivilegeTypes.Count)];
    }

    /// <summary>
    /// Gets a random privilege
    /// </summary>
    public Privilege GetRandomPrivilege()
    {
        return _masterData.Privileges[_random.Next(_masterData.Privileges.Count)];
    }

    /// <summary>
    /// Gets a random category
    /// </summary>
    public Category GetRandomCategory()
    {
        return _masterData.Categories[_random.Next(_masterData.Categories.Count)];
    }

    /// <summary>
    /// Creates a complete subscription setup with user, plan, and subscription
    /// </summary>
    public async Task<(User user, SubscriptionPlan plan, Subscription subscription)> CreateCompleteSubscriptionAsync(
        string planName = "Test Plan",
        decimal basePrice = 99.99m,
        string billingCycle = "monthly",
        bool isAutoCalculated = false)
    {
        var user = await User().BuildAsync();
        
        var billingCycleEntity = _masterData.BillingCycles.First(bc => bc.Name == billingCycle);
        var currency = _masterData.Currencies.First(c => c.Code == "USD");
        var category = _masterData.Categories.First();
        
        var plan = await SubscriptionPlan()
            .WithName(planName)
            .WithBasePrice(basePrice)
            .WithBillingCycle(billingCycle)
            .WithCurrency("USD")
            .WithCategory(category.Id)
            .WithAutoCalculatedPrice(isAutoCalculated)
            .WithAdminCommissionPercent(10.0m)
            .BuildAsync();
        
        var subscription = await Subscription()
            .ForUser(user)
            .WithPlan(plan)
            .WithBillingCycle(billingCycle)
            .BuildAsync();

        return (user, plan, subscription);
    }

    #endregion
}

