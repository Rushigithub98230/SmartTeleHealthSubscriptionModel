using AutoMapper;
using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;

namespace SmartTelehealth.Application.Services;

/// <summary>
/// Service for managing subscription plan versioning.
/// Healthcare Feature: Create plan versions instead of modifying existing plans.
/// Issue #1 Fix: Preserves existing subscriptions when plan changes.
/// </summary>
public class PlanVersioningService : IPlanVersioningService
{
    private readonly ISubscriptionPlanRepository _subscriptionPlanRepository;
    private readonly ISubscriptionPlanPrivilegeRepository _planPrivilegeRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IScheduledPlanMigrationRepository _scheduledMigrationRepository;
    private readonly ISystemSettingsRepository _systemSettingsRepository;
    private readonly IStripeService _stripeService;
    private readonly IPlanPricingService _pricingService;
    private readonly INotificationService _notificationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<PlanVersioningService> _logger;

    public PlanVersioningService(
        ISubscriptionPlanRepository subscriptionPlanRepository,
        ISubscriptionPlanPrivilegeRepository planPrivilegeRepository,
        ISubscriptionRepository subscriptionRepository,
        IScheduledPlanMigrationRepository scheduledMigrationRepository,
        ISystemSettingsRepository systemSettingsRepository,
        IStripeService stripeService,
        IPlanPricingService pricingService,
        INotificationService notificationService,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<PlanVersioningService> logger)
    {
        _subscriptionPlanRepository = subscriptionPlanRepository ?? throw new ArgumentNullException(nameof(subscriptionPlanRepository));
        _planPrivilegeRepository = planPrivilegeRepository ?? throw new ArgumentNullException(nameof(planPrivilegeRepository));
        _subscriptionRepository = subscriptionRepository ?? throw new ArgumentNullException(nameof(subscriptionRepository));
        _scheduledMigrationRepository = scheduledMigrationRepository ?? throw new ArgumentNullException(nameof(scheduledMigrationRepository));
        _systemSettingsRepository = systemSettingsRepository ?? throw new ArgumentNullException(nameof(systemSettingsRepository));
        _stripeService = stripeService ?? throw new ArgumentNullException(nameof(stripeService));
        _pricingService = pricingService ?? throw new ArgumentNullException(nameof(pricingService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new version of a plan instead of modifying existing.
    /// Issue #1 Fix: Preserves existing subscriptions on old version.
    /// Choice 3a: Auto-versions existing plans as v1.0.
    /// </summary>
    public async Task<JsonModel> CreateNewPlanVersionAsync(
        Guid existingPlanId,
        UpdateSubscriptionPlanDto updateDto,
        TokenModel tokenModel)
    {
        await _unitOfWork.BeginTransactionAsync();
        
        try
        {
            _logger.LogInformation(
                "Creating new version of plan {PlanId} by user {UserId}",
                existingPlanId, tokenModel.UserID);
            
            // Get existing plan
            var existingPlan = await _subscriptionPlanRepository
                .GetByIdWithDetailsAsync(existingPlanId);
            
            if (existingPlan == null)
            {
                return new JsonModel 
                { 
                    data = new object(),
                    Message = "Plan not found", 
                    StatusCode = 404 
                };
            }
            
            // Issue #2: Check for active subscriptions
            var activeSubsCount = await _subscriptionPlanRepository
                .GetActiveSubscriptionsCountAsync(existingPlanId);
            
            if (activeSubsCount > 0)
            {
                _logger.LogWarning(
                    "Plan {PlanId} has {Count} active subscriptions. Creating new version to preserve them.",
                    existingPlanId, activeSubsCount);
            }
            
            // Determine parent plan ID
            var parentPlanId = existingPlan.ParentPlanId ?? existingPlan.Id;
            
            // Get max version number
            var allVersions = await _subscriptionPlanRepository
                .GetAllVersionsOfPlanAsync(parentPlanId);
            var newVersionNumber = allVersions.Max(v => v.VersionNumber) + 1;
            
            // Create new version entity
            var newVersion = new SubscriptionPlan
            {
                Id = Guid.NewGuid(),
                
                // Copy base properties
                Name = updateDto.Name ?? existingPlan.Name,
                Description = updateDto.Description ?? existingPlan.Description,
                ShortDescription = existingPlan.ShortDescription,
                BillingCycleId = updateDto.BillingCycleId != Guid.Empty 
                    ? updateDto.BillingCycleId : existingPlan.BillingCycleId,
                CurrencyId = updateDto.CurrencyId != Guid.Empty
                    ? updateDto.CurrencyId : existingPlan.CurrencyId,
                CategoryId = updateDto.CategoryId != Guid.Empty
                    ? updateDto.CategoryId : existingPlan.CategoryId,
                
                // Versioning fields
                ParentPlanId = parentPlanId,
                VersionNumber = newVersionNumber,
                IsLatestVersion = true,
                VersionCreatedDate = DateTime.UtcNow,
                
                // Pricing fields
                Price = updateDto.Price,
                IsAutoCalculatedPrice = updateDto.IsAutoCalculatedPrice,
                AdminCommissionPercent = updateDto.AdminCommissionPercent,
                AdminCommissionFixed = updateDto.AdminCommissionFixed,
                
                // Marketing properties
                IsFeatured = existingPlan.IsFeatured,
                IsMostPopular = updateDto.IsMostPopular,
                IsTrending = updateDto.IsTrending,
                DisplayOrder = updateDto.DisplayOrder ?? existingPlan.DisplayOrder,
                
                // Trial configuration
                IsTrialAllowed = existingPlan.IsTrialAllowed,
                TrialDurationInDays = existingPlan.TrialDurationInDays,
                
                // Plan features
                MessagingCount = existingPlan.MessagingCount,
                IncludesMedicationDelivery = existingPlan.IncludesMedicationDelivery,
                IncludesFollowUpCare = existingPlan.IncludesFollowUpCare,
                DeliveryFrequencyDays = existingPlan.DeliveryFrequencyDays,
                MaxPauseDurationDays = existingPlan.MaxPauseDurationDays,
                
                // Choice 4d: Configurable notice period
                PriceChangeNoticeDays = updateDto.PriceChangeNoticeDays,
                
                // Audit
                CreatedBy = tokenModel.UserID,
                CreatedDate = DateTime.UtcNow,
                IsActive = updateDto.IsActive
            };
            
            // Copy privileges from old version to new version
            await CopyPrivilegesToNewVersionAsync(existingPlan, newVersion, tokenModel);
            
            // Save new version (marks old version as not latest)
            var createdVersion = await _subscriptionPlanRepository
                .CreateNewPlanVersionAsync(newVersion);
            
            // Create Stripe resources for new version
            await CreateStripeResourcesForPlanAsync(createdVersion, tokenModel);
            
            // If auto-calculating price, calculate and update
            if (createdVersion.IsAutoCalculatedPrice)
            {
                var calculatedPrice = await _pricingService.CalculatePlanPriceAsync(createdVersion.Id, true);
                createdVersion.Price = calculatedPrice;
                await _subscriptionPlanRepository.UpdatePlanAsync(createdVersion);
                
                _logger.LogInformation(
                    "Auto-calculated price for plan v{Version}: ${Price}",
                    newVersionNumber, calculatedPrice);
            }
            
            // Schedule migrations for existing subscribers (if any)
            if (activeSubsCount > 0)
            {
                await ScheduleMigrationsForActiveSubscribersAsync(
                    existingPlanId, createdVersion.Id, tokenModel);
            }
            
            await _unitOfWork.CommitTransactionAsync();
            
            _logger.LogInformation(
                "Created plan version {Version} for {PlanName}. {Count} subscribers scheduled for migration.",
                newVersionNumber, createdVersion.Name, activeSubsCount);
            
            return new JsonModel
            {
                data = _mapper.Map<SubscriptionPlanDto>(createdVersion),
                Message = activeSubsCount > 0 
                    ? $"Plan version {newVersionNumber} created. {activeSubsCount} users will migrate at their next renewal."
                    : $"Plan version {newVersionNumber} created successfully.",
                StatusCode = 201
            };
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Failed to create new plan version for plan {PlanId}", existingPlanId);
            return new JsonModel 
            { 
                data = new object(),
                Message = $"Failed to create plan version: {ex.Message}", 
                StatusCode = 500 
            };
        }
    }

    /// <summary>
    /// Gets version history for a plan.
    /// </summary>
    public async Task<JsonModel> GetPlanVersionHistoryAsync(Guid planId)
    {
        try
        {
            _logger.LogInformation("Getting version history for plan {PlanId}", planId);
            
            var versions = await _subscriptionPlanRepository.GetAllVersionsOfPlanAsync(planId);
            
            if (!versions.Any())
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Plan not found",
                    StatusCode = 404
                };
            }
            
            var versionDtos = new List<PlanVersionDto>();
            var totalActiveSubscriptions = 0;
            
            foreach (var version in versions)
            {
                var activeCount = await _subscriptionPlanRepository
                    .GetActiveSubscriptionsCountAsync(version.Id);
                totalActiveSubscriptions += activeCount;
                
                versionDtos.Add(new PlanVersionDto
                {
                    Id = version.Id,
                    Name = version.Name,
                    VersionNumber = version.VersionNumber,
                    IsLatestVersion = version.IsLatestVersion,
                    Price = version.Price,
                    CalculatedPrice = version.CalculatedPrice,
                    VersionCreatedDate = version.VersionCreatedDate,
                    ActiveSubscriptionsCount = activeCount,
                    IsAutoCalculatedPrice = version.IsAutoCalculatedPrice
                });
            }
            
            var firstVersion = versions.First();
            var parentPlanId = firstVersion.ParentPlanId ?? firstVersion.Id;
            
            var history = new PlanVersionHistoryDto
            {
                ParentPlanId = parentPlanId,
                PlanName = firstVersion.Name,
                Versions = versionDtos,
                TotalVersions = versionDtos.Count,
                TotalActiveSubscriptions = totalActiveSubscriptions
            };
            
            return new JsonModel
            {
                data = history,
                Message = "Plan version history retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting version history for plan {PlanId}", planId);
            return new JsonModel
            {
                data = new object(),
                Message = $"Error: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    /// <summary>
    /// Schedules migrations for active subscribers when a new plan version is created.
    /// Healthcare Workflow: Each user migrates at their next individual renewal date.
    /// </summary>
    public async Task<JsonModel> ScheduleMigrationsForPlanVersionAsync(
        Guid oldPlanId,
        Guid newPlanId,
        TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation(
                "Scheduling migrations from plan {OldId} to {NewId} by user {UserId}",
                oldPlanId, newPlanId, tokenModel.UserID);
            
            await ScheduleMigrationsForActiveSubscribersAsync(oldPlanId, newPlanId, tokenModel);
            
            return new JsonModel
            {
                data = new { oldPlanId, newPlanId },
                Message = "Migrations scheduled successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling migrations from plan {OldId} to {NewId}", oldPlanId, newPlanId);
            return new JsonModel
            {
                data = new object(),
                Message = $"Error: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    /// <summary>
    /// Processes user response to scheduled migration.
    /// Healthcare Workflow: User can accept, downgrade, or cancel.
    /// </summary>
    public async Task<JsonModel> ProcessUserMigrationResponseAsync(
        MigrationResponseDto response,
        TokenModel tokenModel)
    {
        await _unitOfWork.BeginTransactionAsync();
        
        try
        {
            _logger.LogInformation(
                "Processing migration response for subscription {SubId}: {Decision}",
                response.SubscriptionId, response.Decision);
            
            var migration = await _scheduledMigrationRepository
                .GetBySubscriptionIdAsync(response.SubscriptionId);
            
            if (migration == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "No pending migration found for this subscription",
                    StatusCode = 404
                };
            }
            
            // Validate subscription belongs to user
            var subscription = await _subscriptionRepository
                .GetByIdWithDetailsAsync(response.SubscriptionId);
            
            if (subscription.UserId != tokenModel.UserID)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Access denied",
                    StatusCode = 403
                };
            }
            
            // Process decision
            migration.UserDecision = response.Decision;
            migration.UserDecisionDate = DateTime.UtcNow;
            migration.UpdatedBy = tokenModel.UserID;
            migration.UpdatedDate = DateTime.UtcNow;
            
            switch (response.Decision.ToLower())
            {
                case "accept":
                    // User accepts the price change - migration will proceed at scheduled date
                    _logger.LogInformation(
                        "User {UserId} accepted migration for subscription {SubId}",
                        tokenModel.UserID, response.SubscriptionId);
                    break;
                
                case "downgrade":
                    // User wants to downgrade to a different plan
                    if (!response.DowngradeToPlanId.HasValue)
                    {
                        return new JsonModel
                        {
                            data = new object(),
                            Message = "Downgrade plan ID is required when choosing to downgrade",
                            StatusCode = 400
                        };
                    }
                    
                    migration.DowngradeToPlanId = response.DowngradeToPlanId.Value;
                    migration.ToPlanId = response.DowngradeToPlanId.Value; // Change target plan
                    
                    _logger.LogInformation(
                        "User {UserId} chose to downgrade subscription {SubId} to plan {PlanId}",
                        tokenModel.UserID, response.SubscriptionId, response.DowngradeToPlanId.Value);
                    break;
                
                case "cancel":
                    // User wants to cancel subscription instead of accepting price change
                    migration.Status = "UserOptedOut";
                    subscription.AutoRenew = false; // Disable auto-renewal
                    subscription.Notes = $"User cancelled due to price change: {response.Reason}";
                    
                    await _subscriptionRepository.UpdateSubscriptionAsync(subscription);
                    
                    _logger.LogInformation(
                        "User {UserId} opted to cancel subscription {SubId} due to price change",
                        tokenModel.UserID, response.SubscriptionId);
                    break;
                
                default:
                    return new JsonModel
                    {
                        data = new object(),
                        Message = "Invalid decision. Must be 'Accept', 'Downgrade', or 'Cancel'",
                        StatusCode = 400
                    };
            }
            
            if (!string.IsNullOrEmpty(response.Reason))
            {
                migration.Notes = response.Reason;
            }
            
            await _scheduledMigrationRepository.UpdateAsync(migration);
            await _unitOfWork.CommitTransactionAsync();
            
            return new JsonModel
            {
                data = migration,
                Message = $"Migration response '{response.Decision}' processed successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error processing migration response for subscription {SubId}", 
                response.SubscriptionId);
            return new JsonModel
            {
                data = new object(),
                Message = $"Error: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    #region Private Helper Methods

    /// <summary>
    /// Copies privileges from existing plan to new version.
    /// Preserves all privilege configurations.
    /// </summary>
    private async Task CopyPrivilegesToNewVersionAsync(
        SubscriptionPlan existingPlan,
        SubscriptionPlan newVersion,
        TokenModel tokenModel)
    {
        _logger.LogInformation(
            "Copying {Count} privileges from plan {OldId} to new version {NewId}",
            existingPlan.PlanPrivileges.Count, existingPlan.Id, newVersion.Id);
        
        foreach (var oldPrivilege in existingPlan.PlanPrivileges.Where(pp => pp.IsActive))
        {
            var newPrivilege = new SubscriptionPlanPrivilege
            {
                Id = Guid.NewGuid(),
                SubscriptionPlanId = newVersion.Id,
                PrivilegeId = oldPrivilege.PrivilegeId,
                Value = oldPrivilege.Value,
                UsagePeriodId = oldPrivilege.UsagePeriodId,
                DurationMonths = oldPrivilege.DurationMonths,
                Description = oldPrivilege.Description,
                EffectiveDate = oldPrivilege.EffectiveDate,
                ExpirationDate = oldPrivilege.ExpirationDate,
                DailyLimit = oldPrivilege.DailyLimit,
                WeeklyLimit = oldPrivilege.WeeklyLimit,
                MonthlyLimit = oldPrivilege.MonthlyLimit,
                
                // Healthcare pricing fields
                PrivilegeBaseCost = oldPrivilege.PrivilegeBaseCost,
                UnitCost = oldPrivilege.UnitCost,
                
                // Audit
                IsActive = true,
                CreatedBy = tokenModel.UserID,
                CreatedDate = DateTime.UtcNow
            };
            
            await _planPrivilegeRepository.AddAsync(newPrivilege);
        }
        
        _logger.LogInformation("Privileges copied successfully to new plan version");
    }

    /// <summary>
    /// Creates Stripe resources for a new plan version.
    /// </summary>
    private async Task CreateStripeResourcesForPlanAsync(
        SubscriptionPlan plan,
        TokenModel tokenModel)
    {
        _logger.LogInformation(
            "Creating Stripe resources for plan {PlanName} v{Version}",
            plan.Name, plan.VersionNumber);
        
        try
        {
            // Create Stripe product
            var productName = $"{plan.Name} v{plan.VersionNumber}";
            var stripeProductId = await _stripeService.CreateProductAsync(
                productName, 
                plan.Description ?? "", 
                tokenModel);
            
            plan.StripeProductId = stripeProductId;
            
            // Create Stripe prices for different billing cycles
            var monthlyPriceId = await _stripeService.CreatePriceAsync(
                stripeProductId, plan.Price, "usd", "month", 1, tokenModel);
            plan.StripeMonthlyPriceId = monthlyPriceId;
            
            var quarterlyPriceId = await _stripeService.CreatePriceAsync(
                stripeProductId, plan.Price * 3, "usd", "month", 3, tokenModel);
            plan.StripeQuarterlyPriceId = quarterlyPriceId;
            
            var annualPriceId = await _stripeService.CreatePriceAsync(
                stripeProductId, plan.Price * 12, "usd", "month", 12, tokenModel);
            plan.StripeAnnualPriceId = annualPriceId;
            
            await _subscriptionPlanRepository.UpdatePlanAsync(plan);
            
            _logger.LogInformation(
                "Stripe resources created for plan v{Version}: Product {ProductId}",
                plan.VersionNumber, stripeProductId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Failed to create Stripe resources for plan {PlanName} v{Version}",
                plan.Name, plan.VersionNumber);
            throw;
        }
    }

    /// <summary>
    /// Healthcare Workflow: Schedule migrations at each user's next renewal date.
    /// Not a fixed grace period - prevents service abuse.
    /// </summary>
    private async Task ScheduleMigrationsForActiveSubscribersAsync(
        Guid oldPlanId,
        Guid newPlanId,
        TokenModel tokenModel)
    {
        var activeSubscriptions = await _subscriptionRepository
            .GetActiveSubscriptionsByPlanIdAsync(oldPlanId);
        
        var newPlan = await _subscriptionPlanRepository.GetByIdWithDetailsAsync(newPlanId);
        if (newPlan == null)
        {
            _logger.LogError("New plan {NewPlanId} not found for scheduling migrations", newPlanId);
            throw new ArgumentException($"New plan {newPlanId} not found");
        }
        
        var noticeDays = newPlan.PriceChangeNoticeDays;
        var migrationsScheduled = 0;
        
        foreach (var subscription in activeSubscriptions)
        {
            try
            {
                // Calculate migration date: user's next renewal
                var migrationDate = subscription.NextBillingDate;
                
                // Ensure minimum notice period (Choice 4d)
                var earliestMigrationDate = DateTime.UtcNow.AddDays(noticeDays);
                if (migrationDate < earliestMigrationDate)
                {
                    // If renewal is too soon, push to next billing cycle
                    migrationDate = CalculateNextBillingDate(subscription, earliestMigrationDate);
                    
                    _logger.LogInformation(
                        "Subscription {SubId} renewal on {Original} is too soon. " +
                        "Pushed migration to {New} to ensure {Days} days notice.",
                        subscription.Id, subscription.NextBillingDate, migrationDate, noticeDays);
                }
                
                var migration = new ScheduledPlanMigration
                {
                    Id = Guid.NewGuid(),
                    SubscriptionId = subscription.Id,
                    FromPlanId = oldPlanId,
                    ToPlanId = newPlanId,
                    NotificationDate = DateTime.UtcNow,
                    ScheduledMigrationDate = migrationDate,
                    Status = "Pending",
                    CreatedBy = tokenModel.UserID,
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true
                };
                
                await _scheduledMigrationRepository.CreateAsync(migration);
                migrationsScheduled++;
                
                // Send notification to user
                await SendPriceChangeNotificationAsync(subscription, newPlan, migrationDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Failed to schedule migration for subscription {SubId}",
                    subscription.Id);
                // Continue with other subscriptions
            }
        }
        
        _logger.LogInformation(
            "Scheduled {Count} migrations from plan {OldId} to {NewId}",
            migrationsScheduled, oldPlanId, newPlanId);
    }

    /// <summary>
    /// Calculates the next billing date after a minimum date.
    /// Ensures proper notice period by pushing to next billing cycle if needed.
    /// </summary>
    private DateTime CalculateNextBillingDate(Subscription subscription, DateTime minimumDate)
    {
        var billingCycleName = subscription.BillingCycle.Name.ToLower();
        var currentBillingDate = subscription.NextBillingDate;
        
        while (currentBillingDate < minimumDate)
        {
            currentBillingDate = billingCycleName switch
            {
                "monthly" => currentBillingDate.AddMonths(1),
                "quarterly" => currentBillingDate.AddMonths(3),
                "annually" or "annual" => currentBillingDate.AddYears(1),
                _ => currentBillingDate.AddMonths(1) // Default to monthly
            };
        }
        
        return currentBillingDate;
    }

    /// <summary>
    /// Sends price change notification to user.
    /// Healthcare Compliance: Inform users with adequate notice.
    /// </summary>
    private async Task SendPriceChangeNotificationAsync(
        Subscription subscription,
        SubscriptionPlan newPlan,
        DateTime migrationDate)
    {
        try
        {
            var oldPlan = subscription.SubscriptionPlan;
            var noticeDays = (migrationDate - DateTime.UtcNow).Days;
            
            var notificationMessage = $@"
Important Update to Your Subscription Plan

Dear {subscription.User.FirstName},

We are updating the pricing for your subscription plan '{oldPlan.Name}'.

Current Plan: {oldPlan.Name} v{oldPlan.VersionNumber} - ${oldPlan.Price}/month
New Plan: {newPlan.Name} v{newPlan.VersionNumber} - ${newPlan.Price}/month

Migration Date: {migrationDate:MMMM dd, yyyy} (Your next renewal date)
Notice Period: {noticeDays} days

What This Means:
- You will continue to enjoy your current plan at ${oldPlan.Price}/month until {migrationDate:MMMM dd, yyyy}
- On {migrationDate:MMMM dd, yyyy}, you will automatically migrate to the new plan at ${newPlan.Price}/month
- Any additional privileges you purchase before migration will be billed at current market rates

Your Options:
1. Accept: Continue with the automatic migration (no action needed)
2. Downgrade: Switch to a different plan that better fits your needs
3. Cancel: Cancel your subscription before the migration date

Please note: If you purchase additional privileges during this period, they will be charged at our current pricing to ensure fairness.

To review your options or respond to this change, please visit your account dashboard.

Best regards,
SmartTelehealth Team
";
            
            // Create system token for automated notification
            var systemToken = new TokenModel { UserID = 0, RoleID = 1 };
            
            await _notificationService.SendNotificationAsync(
                subscription.UserId,
                "Price Change Notification",
                notificationMessage,
                systemToken);
            
            _logger.LogInformation(
                "Sent price change notification to user {UserId} for subscription {SubId}",
                subscription.UserId, subscription.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Failed to send price change notification for subscription {SubId}",
                subscription.Id);
            // Don't throw - notification failure shouldn't break migration scheduling
        }
    }

    #endregion
}

