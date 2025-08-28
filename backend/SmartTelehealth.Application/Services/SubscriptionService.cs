using AutoMapper;
using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Application.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<SubscriptionService> _logger;
    private readonly IStripeService _stripeService;
    private readonly IPrivilegeService _privilegeService;
    private readonly INotificationService _notificationService;
    private readonly IAuditService _auditService;
    private readonly IUserService _userService;
    private readonly ISubscriptionPlanPrivilegeRepository _planPrivilegeRepo;
    private readonly IUserSubscriptionPrivilegeUsageRepository _usageRepo;
    private readonly IBillingService _billingService;
    private readonly ISubscriptionNotificationService _subscriptionNotificationService;

    public SubscriptionService(
        ISubscriptionRepository subscriptionRepository,
        IMapper mapper,
        ILogger<SubscriptionService> logger,
        IStripeService stripeService,
        IPrivilegeService privilegeService,
        INotificationService notificationService,
        IAuditService auditService,
        IUserService userService,
        ISubscriptionPlanPrivilegeRepository planPrivilegeRepo,
        IUserSubscriptionPrivilegeUsageRepository usageRepo,
        IBillingService billingService,
        ISubscriptionNotificationService subscriptionNotificationService)
    {
        _subscriptionRepository = subscriptionRepository ?? throw new ArgumentNullException(nameof(subscriptionRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _stripeService = stripeService ?? throw new ArgumentNullException(nameof(stripeService));
        _privilegeService = privilegeService ?? throw new ArgumentNullException(nameof(privilegeService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _planPrivilegeRepo = planPrivilegeRepo ?? throw new ArgumentNullException(nameof(planPrivilegeRepo));
        _usageRepo = usageRepo ?? throw new ArgumentNullException(nameof(usageRepo));
        _billingService = billingService ?? throw new ArgumentNullException(nameof(billingService));
        _subscriptionNotificationService = subscriptionNotificationService ?? throw new ArgumentNullException(nameof(subscriptionNotificationService));
    }

    public async Task<JsonModel> GetSubscriptionAsync(string subscriptionId, TokenModel tokenModel)
    {
        try
        {
            // Validate token permissions
            if (tokenModel.RoleID != 1 && !await HasAccessToSubscription(tokenModel.UserID, subscriptionId))
            {
                return new JsonModel { data = new object(), Message = "Access denied", StatusCode = 403 };
            }

            var entity = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (entity == null)
                return new JsonModel { data = new object(), Message = "Subscription not found", StatusCode = 404 };
            return new JsonModel { data = _mapper.Map<SubscriptionDto>(entity), Message = "Subscription retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription {SubscriptionId}", subscriptionId);
            return new JsonModel { data = new object(), Message = "Failed to retrieve subscription", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetUserSubscriptionsAsync(int userId, TokenModel tokenModel)
    {
        try
        {
            // Validate token permissions - user can only access their own subscriptions unless admin
            if (tokenModel.RoleID != 1 && tokenModel.UserID != userId)
            {
                return new JsonModel { data = new object(), Message = "Access denied", StatusCode = 403 };
            }

            var entities = await _subscriptionRepository.GetByUserIdAsync(userId);
            var dtos = _mapper.Map<IEnumerable<SubscriptionDto>>(entities);
            return new JsonModel { data = dtos, Message = "User subscriptions retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscriptions for user {UserId}", userId);
            return new JsonModel { data = new object(), Message = "Failed to retrieve user subscriptions", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> CreateSubscriptionAsync(CreateSubscriptionDto createDto, TokenModel tokenModel)
    {
        try
        {
            // 1. Check if plan exists and is active
            var plan = await _subscriptionRepository.GetSubscriptionPlanByIdAsync(Guid.Parse(createDto.PlanId));
            if (plan == null)
                return new JsonModel { data = new object(), Message = "Subscription plan does not exist", StatusCode = 404 };
            if (!plan.IsActive)
                return new JsonModel { data = new object(), Message = "Subscription plan is not active", StatusCode = 400 };

            // 2. Prevent duplicate subscriptions for the same user and plan (active or paused)
            var userSubscriptions = await _subscriptionRepository.GetByUserIdAsync(createDto.UserId);
            if (userSubscriptions.Any(s => s.SubscriptionPlanId == plan.Id && (s.Status == Subscription.SubscriptionStatuses.Active || s.Status == Subscription.SubscriptionStatuses.Paused)))
                return new JsonModel { data = new object(), Message = "User already has an active or paused subscription for this plan", StatusCode = 400 };

            var entity = _mapper.Map<Subscription>(createDto);
            
            // Trial logic
            if (plan.IsTrialAllowed && plan.TrialDurationInDays > 0)
            {
                entity.IsTrialSubscription = true;
                entity.TrialStartDate = DateTime.UtcNow;
                entity.TrialEndDate = DateTime.UtcNow.AddDays(plan.TrialDurationInDays);
                entity.TrialDurationInDays = plan.TrialDurationInDays;
                entity.Status = Subscription.SubscriptionStatuses.TrialActive;
            }
            else
            {
                entity.Status = Subscription.SubscriptionStatuses.Active;
            }
            
            entity.StartDate = DateTime.UtcNow;
            entity.NextBillingDate = DateTime.UtcNow.AddMonths(1);
            
            var created = await _subscriptionRepository.CreateAsync(entity);
            
            // Add status history
            await _subscriptionRepository.AddStatusHistoryAsync(new SubscriptionStatusHistory {
                SubscriptionId = created.Id,
                FromStatus = null,
                ToStatus = created.Status,
                ChangedAt = DateTime.UtcNow
            });
            
            var dto = _mapper.Map<SubscriptionDto>(created);
            
            // Send confirmation and welcome emails
            var userResult = await _userService.GetUserByIdAsync(createDto.UserId, tokenModel);
            if (userResult.StatusCode != 200)
            {
                _logger.LogWarning("Failed to get user {UserId} for subscription creation by user {TokenUserId}", 
                    createDto.UserId, tokenModel?.UserID ?? 0);
                return new JsonModel { data = new object(), Message = "User not found", StatusCode = 404 };
            }
            if (userResult.data != null)
            {
                // Send subscription confirmation and welcome emails
                await _notificationService.SendSubscriptionConfirmationAsync(((UserDto)userResult.data).Email, ((UserDto)userResult.data).FullName, dto, tokenModel);
                await _notificationService.SendSubscriptionWelcomeEmailAsync(((UserDto)userResult.data).Email, ((UserDto)userResult.data).FullName, dto, tokenModel);
                
                // Send subscription created notification via the subscription notification service
                await _subscriptionNotificationService.SendSubscriptionCreatedNotificationAsync(created.Id.ToString(), tokenModel);
                
                _logger.LogInformation("Subscription confirmation, welcome emails, and created notification sent to {Email}", ((UserDto)userResult.data).Email);
            }
            
            // Audit log
                            await _auditService.LogUserActionAsync(createDto.UserId, "CreateSubscription", "Subscription", created.Id.ToString(), "Subscription created successfully", tokenModel);
            
            return new JsonModel { data = dto, Message = "Subscription created", StatusCode = 201 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating subscription for user {UserId}", createDto.UserId);
            return new JsonModel { data = new object(), Message = "Failed to create subscription", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> CancelSubscriptionAsync(string subscriptionId, string? reason, TokenModel tokenModel)
    {
        try
        {
            // Validate token permissions
            if (tokenModel.RoleID != 1 && !await HasAccessToSubscription(tokenModel.UserID, subscriptionId))
            {
                return new JsonModel { data = new object(), Message = "Access denied", StatusCode = 403 };
            }

            var entity = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (entity == null)
                return new JsonModel { data = new object(), Message = "Subscription not found", StatusCode = 404 };
            
            // Prevent cancelling an already cancelled subscription
            if (entity.IsCancelled)
                return new JsonModel { data = new object(), Message = "Subscription is already cancelled", StatusCode = 400 };
            
            // Validate status transition
            var validation = entity.ValidateStatusTransition(Subscription.SubscriptionStatuses.Cancelled);
            if (validation != ValidationResult.Success)
                return new JsonModel { data = new object(), Message = validation.ErrorMessage, StatusCode = 400 };
            
            var oldStatus = entity.Status;
            entity.Status = Subscription.SubscriptionStatuses.Cancelled;
            entity.CancellationReason = reason;
            entity.CancelledDate = DateTime.UtcNow;
            
            var updated = await _subscriptionRepository.UpdateAsync(entity);
            
            // Add status history
            await _subscriptionRepository.AddStatusHistoryAsync(new SubscriptionStatusHistory {
                SubscriptionId = updated.Id,
                FromStatus = oldStatus,
                ToStatus = updated.Status,
                Reason = reason,
                ChangedAt = DateTime.UtcNow
            });
            
            var dto = _mapper.Map<SubscriptionDto>(updated);
            
            // Send cancellation email
            var userResult = await _userService.GetUserByIdAsync(entity.UserId, tokenModel);
            if (userResult.StatusCode == 200 && userResult.data != null)
            {
                // Send subscription cancellation email
                await _notificationService.SendSubscriptionCancellationAsync(((UserDto)userResult.data).Email, ((UserDto)userResult.data).FullName, dto, tokenModel);
                _logger.LogInformation("Subscription cancellation email sent to {Email}", ((UserDto)userResult.data).Email);
            }
            
            // Audit log
            await _auditService.LogUserActionAsync(entity.UserId, "CancelSubscription", "Subscription", subscriptionId, reason ?? "Subscription cancelled", tokenModel);
            
            return new JsonModel { data = dto, Message = "Subscription cancelled", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling subscription {SubscriptionId}", subscriptionId);
            return new JsonModel { data = new object(), Message = "Failed to cancel subscription", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> PauseSubscriptionAsync(string subscriptionId, TokenModel tokenModel)
    {
        try
        {
            // Validate token permissions
            if (tokenModel.RoleID != 1 && !await HasAccessToSubscription(tokenModel.UserID, subscriptionId))
            {
                return new JsonModel { data = new object(), Message = "Access denied", StatusCode = 403 };
            }

            var entity = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (entity == null)
                return new JsonModel { data = new object(), Message = "Subscription not found", StatusCode = 404 };
            
            if (entity.IsPaused)
                return new JsonModel { data = new object(), Message = "Subscription is already paused", StatusCode = 400 };
            
            if (entity.IsCancelled)
                return new JsonModel { data = new object(), Message = "Cannot pause a cancelled subscription", StatusCode = 400 };
            
            // Validate status transition
            var validation = entity.ValidateStatusTransition(Subscription.SubscriptionStatuses.Paused);
            if (validation != ValidationResult.Success)
                return new JsonModel { data = new object(), Message = validation.ErrorMessage, StatusCode = 400 };
            
            var oldStatus = entity.Status;
            entity.Status = Subscription.SubscriptionStatuses.Paused;
            entity.PausedDate = DateTime.UtcNow;
            
            var updated = await _subscriptionRepository.UpdateAsync(entity);
            
            // Add status history
            await _subscriptionRepository.AddStatusHistoryAsync(new SubscriptionStatusHistory {
                SubscriptionId = updated.Id,
                FromStatus = oldStatus,
                ToStatus = updated.Status,
                ChangedAt = DateTime.UtcNow
            });
            
            var dto = _mapper.Map<SubscriptionDto>(updated);
            
            // Send pause notification email
            var userResult = await _userService.GetUserByIdAsync(entity.UserId, tokenModel);
            if (userResult.StatusCode == 200 && userResult.data != null)
            {
                // Send subscription pause notification email
                await _notificationService.SendSubscriptionPausedNotificationAsync(((UserDto)userResult.data).Email, ((UserDto)userResult.data).FullName, dto, tokenModel);
                _logger.LogInformation("Subscription pause notification email sent to {Email}", ((UserDto)userResult.data).Email);
            }
            
            // Audit log
            await _auditService.LogUserActionAsync(entity.UserId, "PauseSubscription", "Subscription", subscriptionId, "Subscription paused", tokenModel);
            
            return new JsonModel { data = dto, Message = "Subscription paused", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pausing subscription {SubscriptionId}", subscriptionId);
            return new JsonModel { data = new object(), Message = "Failed to pause subscription", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> ResumeSubscriptionAsync(string subscriptionId, TokenModel tokenModel)
    {
        try
        {
            // Validate token permissions
            if (tokenModel.RoleID != 1 && !await HasAccessToSubscription(tokenModel.UserID, subscriptionId))
            {
                return new JsonModel { data = new object(), Message = "Access denied", StatusCode = 403 };
            }

            var entity = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (entity == null)
                return new JsonModel { data = new object(), Message = "Subscription not found", StatusCode = 404 };
            
            if (!entity.IsPaused)
                return new JsonModel { data = new object(), Message = "Subscription is not paused", StatusCode = 400 };
            
            // Validate status transition
            var validation = entity.ValidateStatusTransition(Subscription.SubscriptionStatuses.Active);
            if (validation != ValidationResult.Success)
                return new JsonModel { data = new object(), Message = validation.ErrorMessage, StatusCode = 400 };
            
            var oldStatus = entity.Status;
            entity.Status = Subscription.SubscriptionStatuses.Active;
            entity.ResumedDate = DateTime.UtcNow;
            
            var updated = await _subscriptionRepository.UpdateAsync(entity);
            
            // Add status history
            await _subscriptionRepository.AddStatusHistoryAsync(new SubscriptionStatusHistory {
                SubscriptionId = updated.Id,
                FromStatus = oldStatus,
                ToStatus = updated.Status,
                ChangedAt = DateTime.UtcNow
            });
            
            var dto = _mapper.Map<SubscriptionDto>(updated);
            
            // Send resume email
            var userResult = await _userService.GetUserByIdAsync(entity.UserId, tokenModel);
            if (userResult.StatusCode == 200 && userResult.data != null)
            {
                // Send subscription resume email
                await _notificationService.SendSubscriptionResumedNotificationAsync(((UserDto)userResult.data).Email, ((UserDto)userResult.data).FullName, dto, tokenModel);
                _logger.LogInformation("Subscription resume email sent to {Email}", ((UserDto)userResult.data).Email);
            }
            
            // Audit log
            await _auditService.LogUserActionAsync(entity.UserId, "ResumeSubscription", "Subscription", subscriptionId, "Subscription resumed", tokenModel);
            
            return new JsonModel { data = dto, Message = "Subscription resumed", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resuming subscription {SubscriptionId}", subscriptionId);
            return new JsonModel { data = new object(), Message = "Failed to resume subscription", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> UpgradeSubscriptionAsync(string subscriptionId, string newPlanId, TokenModel tokenModel)
    {
        try
        {
            // Validate token permissions
            if (tokenModel.RoleID != 1 && !await HasAccessToSubscription(tokenModel.UserID, subscriptionId))
            {
                return new JsonModel { data = new object(), Message = "Access denied", StatusCode = 403 };
            }

            var entity = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (entity == null)
                return new JsonModel { data = new object(), Message = "Subscription not found", StatusCode = 404 };
            
            // Prevent upgrading to the same plan
            if (entity.SubscriptionPlanId == Guid.Parse(newPlanId))
                return new JsonModel { data = new object(), Message = "Subscription is already on this plan", StatusCode = 400 };
            
            var oldPlanId = entity.SubscriptionPlanId;
            entity.SubscriptionPlanId = Guid.Parse(newPlanId);
            entity.UpdatedAt = DateTime.UtcNow;
            
            var updated = await _subscriptionRepository.UpdateAsync(entity);
            
            // Audit log
            await _auditService.LogUserActionAsync(entity.UserId, "UpgradeSubscription", "Subscription", subscriptionId, $"Upgraded from plan {oldPlanId} to {newPlanId}", tokenModel);
            
            return new JsonModel { data = _mapper.Map<SubscriptionDto>(updated), Message = "Subscription upgraded", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error upgrading subscription {SubscriptionId}", subscriptionId);
            return new JsonModel { data = new object(), Message = "Failed to upgrade subscription", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> ReactivateSubscriptionAsync(string subscriptionId, TokenModel tokenModel)
    {
        try
        {
            // Validate token permissions
            if (tokenModel.RoleID != 1 && !await HasAccessToSubscription(tokenModel.UserID, subscriptionId))
            {
                return new JsonModel { data = new object(), Message = "Access denied", StatusCode = 403 };
            }

            var entity = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (entity == null)
                return new JsonModel { data = new object(), Message = "Subscription not found", StatusCode = 404 };
            
            // Validate status transition
            var validation = entity.ValidateStatusTransition(Subscription.SubscriptionStatuses.Active);
            if (validation != ValidationResult.Success)
                return new JsonModel { data = new object(), Message = validation.ErrorMessage, StatusCode = 400 };
            
            var oldStatus = entity.Status;
            entity.Status = Subscription.SubscriptionStatuses.Active;
            entity.UpdatedAt = DateTime.UtcNow;
            
            var updated = await _subscriptionRepository.UpdateAsync(entity);
            
            // Add status history
            await _subscriptionRepository.AddStatusHistoryAsync(new SubscriptionStatusHistory {
                SubscriptionId = updated.Id,
                FromStatus = oldStatus,
                ToStatus = updated.Status,
                ChangedAt = DateTime.UtcNow
            });
            
            // Audit log
            await _auditService.LogUserActionAsync(entity.UserId, "ReactivateSubscription", "Subscription", subscriptionId, "Subscription reactivated", tokenModel);
            
            return new JsonModel { data = _mapper.Map<SubscriptionDto>(updated), Message = "Subscription reactivated", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reactivating subscription {SubscriptionId}", subscriptionId);
            return new JsonModel { data = new object(), Message = "Failed to reactivate subscription", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetAllPlansAsync(TokenModel tokenModel)
    {
        try
        {
            var plans = await _subscriptionRepository.GetAllSubscriptionPlansAsync();
            var dtos = _mapper.Map<IEnumerable<SubscriptionPlanDto>>(plans);
            return new JsonModel { data = dtos, Message = "Subscription plans retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all subscription plans");
            return new JsonModel { data = new object(), Message = "Failed to retrieve subscription plans", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetPublicPlansAsync()
    {
        try
        {
            var plans = await _subscriptionRepository.GetAllSubscriptionPlansAsync();
            // Only return active plans for public display
            var activePlans = plans.Where(p => p.IsActive);
            var dtos = _mapper.Map<IEnumerable<SubscriptionPlanDto>>(activePlans);
            return new JsonModel { data = dtos, Message = "Public subscription plans retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting public subscription plans");
            return new JsonModel { data = new object(), Message = "Failed to retrieve public subscription plans", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetAllPlansAsync(int page, int pageSize, string? searchTerm, string? categoryId, bool? isActive, TokenModel tokenModel)
    {
        try
        {
            var allPlans = await _subscriptionRepository.GetAllSubscriptionPlansAsync();
            
            // Apply filters
            var filteredPlans = allPlans.AsQueryable();
            
            if (!string.IsNullOrEmpty(searchTerm))
            {
                filteredPlans = filteredPlans.Where(p => p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) || 
                                                       p.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            }
            
            if (!string.IsNullOrEmpty(categoryId) && Guid.TryParse(categoryId, out var categoryGuid))
            {
                // Note: SubscriptionPlan doesn't have CategoryId property, so this filter is disabled
                // filteredPlans = filteredPlans.Where(p => p.CategoryId == categoryGuid);
            }
            
            if (isActive.HasValue)
            {
                filteredPlans = filteredPlans.Where(p => p.IsActive == isActive.Value);
            }
            
            // Apply pagination
            var totalCount = filteredPlans.Count();
            var plans = filteredPlans
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            
            var dtos = _mapper.Map<IEnumerable<SubscriptionPlanDto>>(plans);
            return new JsonModel { data = dtos, Message = "Subscription plans retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting filtered subscription plans");
            return new JsonModel { data = new object(), Message = "Failed to retrieve subscription plans", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetPlanByIdAsync(string planId, TokenModel tokenModel)
    {
        var plan = await _subscriptionRepository.GetSubscriptionPlanByIdAsync(Guid.Parse(planId));
        if (plan == null)
            return new JsonModel { data = new object(), Message = "Plan not found", StatusCode = 404 };
        return new JsonModel { data = _mapper.Map<SubscriptionPlanDto>(plan), Message = "Plan retrieved successfully", StatusCode = 200 };
    }

    public async Task<JsonModel> GetBillingHistoryAsync(string subscriptionId, TokenModel tokenModel)
    {
        try
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (subscription == null)
                return new JsonModel { data = new object(), Message = "Subscription not found", StatusCode = 404 };

            // Get billing records for this subscription
            var billingRecords = await _billingService.GetSubscriptionBillingHistoryAsync(subscription.Id, tokenModel);
            
            if (billingRecords.StatusCode != 200)
                return new JsonModel { data = new object(), Message = "Failed to retrieve billing history", StatusCode = 500 };

            var billingHistory = ((IEnumerable<BillingRecordDto>)billingRecords.data).Select(br => new BillingHistoryDto
            {
                Id = br.Id.ToString(),
                Amount = br.Amount,
                Status = br.Status,
                BillingDate = br.BillingDate,
                PaidDate = br.PaidAt,
                Description = br.Description,
                InvoiceNumber = br.InvoiceNumber,
                PaymentMethod = br.PaymentMethod
            });

            return new JsonModel { data = billingHistory, Message = "Billing history retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting billing history for subscription {SubscriptionId}", subscriptionId);
            return new JsonModel { data = new object(), Message = "Failed to retrieve billing history", StatusCode = 500 };
        }
    }

            public async Task<JsonModel> GetPaymentMethodsAsync(int userId, TokenModel tokenModel)
    {
        // Validate token permissions - user can only access their own payment methods unless admin
        if (tokenModel.RoleID != 1 && tokenModel.UserID != userId)
        {
            return new JsonModel { data = new object(), Message = "Access denied", StatusCode = 403 };
        }

        var methods = await _stripeService.GetCustomerPaymentMethodsAsync(userId.ToString(), tokenModel);
        return new JsonModel { data = methods, Message = "Payment methods retrieved successfully", StatusCode = 200 };
    }

    public async Task<JsonModel> AddPaymentMethodAsync(int userId, string paymentMethodId, TokenModel tokenModel)
    {
        // Validate token permissions - user can only add payment methods to their own account unless admin
        if (tokenModel.RoleID != 1 && tokenModel.UserID != userId)
        {
            return new JsonModel { data = new object(), Message = "Access denied", StatusCode = 403 };
        }

        var methodId = await _stripeService.AddPaymentMethodAsync(userId.ToString(), paymentMethodId, tokenModel);
        var method = new PaymentMethodDto { Id = methodId };
        return new JsonModel { data = method, Message = "Payment method added", StatusCode = 200 };
    }

    public async Task<JsonModel> GetSubscriptionByPlanIdAsync(string planId, TokenModel tokenModel)
    {
        try
        {
            // Admin only method - validate admin role
            if (tokenModel.RoleID != 1 && tokenModel.RoleID != 3)
            {
                return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };
            }

            var subscription = await _subscriptionRepository.GetByPlanIdAsync(Guid.Parse(planId));
            if (subscription == null)
                return new JsonModel { data = new object(), Message = "Subscription not found for this plan", StatusCode = 404 };
            return new JsonModel { data = _mapper.Map<SubscriptionDto>(subscription), Message = "Subscription retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription by plan ID {PlanId}", planId);
            return new JsonModel { data = new object(), Message = "Failed to retrieve subscription", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetActiveSubscriptionsAsync(TokenModel tokenModel)
    {
        try
        {
            // Admin only method - validate admin role
            if (tokenModel.RoleID != 1 && tokenModel.RoleID != 3)
            {
                return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };
            }

            var activeSubscriptions = await _subscriptionRepository.GetActiveSubscriptionsAsync();
            var dtos = _mapper.Map<IEnumerable<SubscriptionDto>>(activeSubscriptions);
            return new JsonModel { data = dtos, Message = "Active subscriptions retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active subscriptions");
            return new JsonModel { data = new object(), Message = "Failed to retrieve active subscriptions", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetSubscriptionByIdAsync(string subscriptionId, TokenModel tokenModel)
    {
        return await GetSubscriptionAsync(subscriptionId, tokenModel);
    }

    public async Task<JsonModel> UpdateSubscriptionAsync(string subscriptionId, UpdateSubscriptionDto updateDto, TokenModel tokenModel)
    {
        try
        {
            // Validate token permissions
            if (tokenModel.RoleID != 1 && !await HasAccessToSubscription(tokenModel.UserID, subscriptionId))
            {
                return new JsonModel { data = new object(), Message = "Access denied", StatusCode = 403 };
            }

            var subscription = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (subscription == null)
                return new JsonModel { data = new object(), Message = "Subscription not found", StatusCode = 404 };

            // Update subscription properties
            if (!string.IsNullOrEmpty(updateDto.Status))
                subscription.Status = updateDto.Status;
            
            if (updateDto.AutoRenew.HasValue)
                subscription.AutoRenew = updateDto.AutoRenew.Value;
            
            if (updateDto.NextBillingDate.HasValue)
                subscription.NextBillingDate = updateDto.NextBillingDate.Value;

            subscription.UpdatedAt = DateTime.UtcNow;
            
            var updatedSubscription = await _subscriptionRepository.UpdateAsync(subscription);
            
            // Audit log
                            await _auditService.LogUserActionAsync(subscription.UserId, "UpdateSubscription", "Subscription", subscriptionId, "Subscription updated", tokenModel);
            
            return new JsonModel { data = _mapper.Map<SubscriptionDto>(updatedSubscription), Message = "Subscription updated successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating subscription {SubscriptionId}", subscriptionId);
            return new JsonModel { data = new object(), Message = "Failed to update subscription", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> ProcessPaymentAsync(string subscriptionId, PaymentRequestDto paymentRequest, TokenModel tokenModel)
    {
        try
        {
            // Validate token permissions
            if (tokenModel.RoleID != 1 && !await HasAccessToSubscription(tokenModel.UserID, subscriptionId))
            {
                return new JsonModel { data = new object(), Message = "Access denied", StatusCode = 403 };
            }

            var subscription = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (subscription == null)
                return new JsonModel { data = new object(), Message = "Subscription not found", StatusCode = 404 };

            // Process payment through Stripe
            var paymentResult = await _stripeService.ProcessPaymentAsync(
                paymentRequest.PaymentMethodId,
                paymentRequest.Amount,
                paymentRequest.Currency ?? "usd",
                tokenModel
            );

            if (paymentResult.Status == "succeeded")
            {
                // Update subscription status if needed
                if (subscription.Status == Subscription.SubscriptionStatuses.PaymentFailed)
                {
                    subscription.Status = Subscription.SubscriptionStatuses.Active;
                    await _subscriptionRepository.UpdateAsync(subscription);
                }

                return new JsonModel { data = paymentResult, Message = "Payment processed successfully", StatusCode = 200 };
            }
            else
            {
                return new JsonModel { data = new object(), Message = $"Payment failed: {paymentResult.ErrorMessage}", StatusCode = 400 };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment for subscription {SubscriptionId}", subscriptionId);
            return new JsonModel { data = new object(), Message = "Failed to process payment", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetUsageStatisticsAsync(string subscriptionId, TokenModel tokenModel)
    {
        try
        {
            // Validate token permissions
            if (tokenModel.RoleID != 1 && !await HasAccessToSubscription(tokenModel.UserID, subscriptionId))
            {
                return new JsonModel { data = new object(), Message = "Access denied", StatusCode = 403 };
            }

            var subscription = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (subscription == null)
                return new JsonModel { data = new object(), Message = "Subscription not found", StatusCode = 404 };

            var usages = await _usageRepo.GetBySubscriptionIdAsync(subscription.Id);
            var planPrivileges = await _planPrivilegeRepo.GetByPlanIdAsync(subscription.SubscriptionPlanId);

            var usageStats = new UsageStatisticsDto
            {
                SubscriptionId = subscriptionId,
                PlanName = subscription.SubscriptionPlan.Name,
                CurrentPeriodStart = subscription.StartDate,
                CurrentPeriodEnd = subscription.NextBillingDate,
                TotalPrivileges = planPrivileges.Count(),
                UsedPrivileges = usages.Count(),
                PrivilegeUsage = new List<PrivilegeUsageDto>()
            };

            foreach (var usage in usages)
            {
                var planPrivilege = planPrivileges.FirstOrDefault(pp => pp.Id == usage.SubscriptionPlanPrivilegeId);
                if (planPrivilege != null)
                {
                    usageStats.PrivilegeUsage.Add(new PrivilegeUsageDto
                    {
                        PrivilegeName = planPrivilege.Privilege.Name,
                        UsedValue = usage.UsedValue,
                        AllowedValue = planPrivilege.Value,
                        RemainingValue = planPrivilege.Value == -1 ? int.MaxValue : Math.Max(0, planPrivilege.Value - usage.UsedValue),
                        UsagePercentage = planPrivilege.Value == -1 ? 0 : (decimal)usage.UsedValue / planPrivilege.Value * 100
                    });
                }
            }

            return new JsonModel { data = usageStats, Message = "Usage statistics retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting usage statistics for subscription {SubscriptionId}", subscriptionId);
            return new JsonModel { data = new object(), Message = "Failed to retrieve usage statistics", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetAllSubscriptionsAsync(TokenModel tokenModel)
    {
        try
        {
            // Admin only method - validate admin role
            if (tokenModel.RoleID != 1 && tokenModel.RoleID != 3)
            {
                return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };
            }

            var allSubscriptions = await _subscriptionRepository.GetAllSubscriptionsAsync();
            var dtos = _mapper.Map<IEnumerable<SubscriptionDto>>(allSubscriptions);
            return new JsonModel { data = dtos, Message = "All subscriptions retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all subscriptions");
            return new JsonModel { data = new object(), Message = "Failed to retrieve subscriptions", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetSubscriptionAnalyticsAsync(string subscriptionId, TokenModel tokenModel)
    {
        try
        {
            // Validate token permissions
            if (tokenModel.RoleID != 1 && !await HasAccessToSubscription(tokenModel.UserID, subscriptionId))
            {
                return new JsonModel { data = new object(), Message = "Access denied", StatusCode = 403 };
            }

            var subscription = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (subscription == null)
                return new JsonModel { data = new object(), Message = "Subscription not found", StatusCode = 404 };

            // Get usage statistics
            var usageStats = await GetUsageStatisticsAsync(subscriptionId, tokenModel);
            if (usageStats.StatusCode != 200)
                return new JsonModel { data = new object(), Message = "Failed to get usage statistics", StatusCode = 500 };

            // Get billing history
            var billingHistory = await _billingService.GetPaymentHistoryAsync(subscription.UserId, null, null, tokenModel);

            var analytics = new SubscriptionAnalyticsDto
            {
                SubscriptionId = subscriptionId,
                PlanName = subscription.SubscriptionPlan.Name,
                Status = subscription.Status,
                StartDate = subscription.StartDate,
                NextBillingDate = subscription.NextBillingDate,
                TotalAmountPaid = billingHistory.data is IEnumerable<BillingRecordDto> billingData1 ? billingData1.Sum(bh => bh.Amount) : 0,
                PaymentCount = billingHistory.data is IEnumerable<BillingRecordDto> billingData2 ? billingData2.Count() : 0,
                AveragePaymentAmount = billingHistory.data is IEnumerable<BillingRecordDto> billingData3 && billingData3.Any() ? billingData3.Average(bh => bh.Amount) : 0,
                UsageStatistics = usageStats.data is UsageStatisticsDto usageData ? usageData : null,
                PaymentHistory = billingHistory.data is IEnumerable<BillingRecordDto> billingData4 ? billingData4.Select(bh => new PaymentHistoryDto
                {
                    Id = Guid.TryParse(bh.Id, out Guid id) ? id : Guid.Empty,
                    UserId = bh.UserId,
                    SubscriptionId = bh.SubscriptionId ?? string.Empty,
                    Amount = bh.Amount,
                    Currency = bh.Currency,
                    PaymentMethod = bh.PaymentMethod,
                    Status = bh.Status,
                    TransactionId = bh.StripePaymentIntentId,
                    ErrorMessage = bh.FailureReason,
                    CreatedAt = bh.CreatedAt,
                    ProcessedAt = bh.PaidAt,
                    PaymentDate = bh.CreatedAt,
                    Description = bh.Description,
                    PaymentMethodId = bh.PaymentMethodId
                }).ToList() : new List<PaymentHistoryDto>()
            };

            return new JsonModel { data = analytics, Message = "Subscription analytics retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription analytics for subscription {SubscriptionId}", subscriptionId);
            return new JsonModel { data = new object(), Message = "Failed to retrieve subscription analytics", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> CreatePlanAsync(CreateSubscriptionPlanDto createPlanDto, TokenModel tokenModel)
    {
        try
        {
            // Admin only method - validate admin role
            if (tokenModel.RoleID != 1 && tokenModel.RoleID != 3)
            {
                return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };
            }

            var plan = new SubscriptionPlan
            {
                Name = createPlanDto.Name,
                Description = createPlanDto.Description,
                Price = createPlanDto.Price,
                BillingCycleId = createPlanDto.BillingCycleId,
                CurrencyId = createPlanDto.CurrencyId,
                IsActive = createPlanDto.IsActive,
                DisplayOrder = createPlanDto.DisplayOrder,
                // PlanPrivileges will be added later or via a separate call
            };
            var created = await _subscriptionRepository.CreateSubscriptionPlanAsync(plan);
            return new JsonModel { data = _mapper.Map<SubscriptionPlanDto>(created), Message = "Plan created", StatusCode = 201 };
        }
        catch (Exception ex)
        {
            return new JsonModel { data = new object(), Message = $"Failed to create plan: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> UpdatePlanAsync(string planId, UpdateSubscriptionPlanDto updatePlanDto, TokenModel tokenModel)
    {
        try
        {
            // Admin only method - validate admin role
            if (tokenModel.RoleID != 1 && tokenModel.RoleID != 3)
            {
                return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };
            }

            var plan = await _subscriptionRepository.GetSubscriptionPlanByIdAsync(Guid.Parse(planId));
            if (plan == null)
                return new JsonModel { data = new object(), Message = "Plan not found", StatusCode = 404 };
            plan.Name = updatePlanDto.Name;
            plan.Description = updatePlanDto.Description;
            plan.IsActive = updatePlanDto.IsActive;
            // Remove updates to Price, BillingCycleId, CurrencyId, etc., as they are not present in the DTO
            var updated = await _subscriptionRepository.UpdateSubscriptionPlanAsync(plan);
            return new JsonModel { data = _mapper.Map<SubscriptionPlanDto>(updated), Message = "Plan updated", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            return new JsonModel { data = new object(), Message = $"Failed to update plan: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> ActivatePlanAsync(string planId, TokenModel tokenModel)
    {
        try
        {
            // Admin only method - validate admin role
            if (tokenModel.RoleID != 1 && tokenModel.RoleID != 3)
            {
                return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };
            }

            var plan = await _subscriptionRepository.GetSubscriptionPlanByIdAsync(Guid.Parse(planId));
            if (plan == null)
                return new JsonModel { data = new object(), Message = "Plan not found", StatusCode = 404 };
            plan.IsActive = true;
            await _subscriptionRepository.UpdateSubscriptionPlanAsync(plan);
            return new JsonModel { data = true, Message = "Plan activated", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            return new JsonModel { data = new object(), Message = $"Failed to activate plan: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> DeactivatePlanAsync(string planId, TokenModel tokenModel)
    {
        try
        {
            // Admin only method - validate admin role
            if (tokenModel.RoleID != 1 && tokenModel.RoleID != 3)
            {
                return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };
            }

            var plan = await _subscriptionRepository.GetSubscriptionPlanByIdAsync(Guid.Parse(planId));
            if (plan == null)
                return new JsonModel { data = new object(), Message = "Plan not found", StatusCode = 404 };
            plan.IsActive = false;
            await _subscriptionRepository.UpdateSubscriptionPlanAsync(plan);
            return new JsonModel { data = true, Message = "Plan deactivated", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            return new JsonModel { data = new object(), Message = $"Failed to deactivate plan: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> DeletePlanAsync(string planId, TokenModel tokenModel)
    {
        try
        {
            // Admin only method - validate admin role
            if (tokenModel.RoleID != 1 && tokenModel.RoleID != 3)
            {
                return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };
            }

            var result = await _subscriptionRepository.DeleteSubscriptionPlanAsync(Guid.Parse(planId));
            if (!result)
                return new JsonModel { data = new object(), Message = "Plan not found or could not be deleted", StatusCode = 404 };
            return new JsonModel { data = true, Message = "Plan deleted", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            return new JsonModel { data = new object(), Message = $"Failed to delete plan: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId, TokenModel tokenModel)
    {
        try
        {
            // Admin only method - validate admin role
            if (tokenModel.RoleID != 1 && tokenModel.RoleID != 3)
            {
                return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };
            }

            var subscription = await _subscriptionRepository.GetByStripeSubscriptionIdAsync(stripeSubscriptionId, tokenModel);
            if (subscription == null)
                return new JsonModel { data = new object(), Message = "Subscription not found for this Stripe ID", StatusCode = 404 };
            return new JsonModel { data = _mapper.Map<SubscriptionDto>(subscription), Message = "Subscription retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription by Stripe ID {StripeSubscriptionId}", stripeSubscriptionId);
            return new JsonModel { data = new object(), Message = "Failed to retrieve subscription", StatusCode = 500 };
        }
    }

    // Admin management methods
    public async Task<JsonModel> GetAllUserSubscriptionsAsync(int page, int pageSize, string? searchTerm, string[]? status, string[]? planId, string[]? userId, DateTime? startDate, DateTime? endDate, string? sortBy, string? sortOrder, TokenModel tokenModel)
    {
        try
        {
            // Admin only method - validate admin role
            if (tokenModel.RoleID != 1 && tokenModel.RoleID != 3)
            {
                return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };
            }

            var allSubscriptions = await _subscriptionRepository.GetAllSubscriptionsAsync();
            var filteredSubscriptions = allSubscriptions.AsQueryable();
            
            // Apply search term filter
            if (!string.IsNullOrEmpty(searchTerm))
            {
                filteredSubscriptions = filteredSubscriptions.Where(s => 
                    s.User.UserName.Contains(searchTerm) || 
                    s.SubscriptionPlan.Name.Contains(searchTerm) ||
                    s.Id.ToString().Contains(searchTerm));
            }
            
            // Apply status filter (array)
            if (status != null && status.Length > 0)
            {
                filteredSubscriptions = filteredSubscriptions.Where(s => status.Contains(s.Status));
            }
            
            // Apply plan ID filter (array)
            if (planId != null && planId.Length > 0)
            {
                var planIds = planId.Where(id => Guid.TryParse(id, out _)).Select(id => Guid.Parse(id)).ToList();
                if (planIds.Any())
                {
                    filteredSubscriptions = filteredSubscriptions.Where(s => planIds.Contains(s.SubscriptionPlanId));
                }
            }
            
            // Apply user ID filter (array)
            if (userId != null && userId.Length > 0)
            {
                var userIds = userId.Where(id => int.TryParse(id, out _)).Select(id => int.Parse(id)).ToList();
                if (userIds.Any())
                {
                    filteredSubscriptions = filteredSubscriptions.Where(s => userIds.Contains(s.UserId));
                }
            }
            
            if (startDate.HasValue)
            {
                filteredSubscriptions = filteredSubscriptions.Where(s => s.CreatedAt >= startDate.Value);
            }
            
            if (endDate.HasValue)
            {
                filteredSubscriptions = filteredSubscriptions.Where(s => s.CreatedAt <= endDate.Value);
            }
            
            // Apply sorting
            if (!string.IsNullOrEmpty(sortBy))
            {
                filteredSubscriptions = sortBy.ToLower() switch
                {
                    "createdat" => sortOrder?.ToLower() == "desc" 
                        ? filteredSubscriptions.OrderByDescending(s => s.CreatedAt)
                        : filteredSubscriptions.OrderBy(s => s.CreatedAt),
                    "status" => sortOrder?.ToLower() == "desc" 
                        ? filteredSubscriptions.OrderByDescending(s => s.Status)
                        : filteredSubscriptions.OrderBy(s => s.Status),
                    "userid" => sortOrder?.ToLower() == "desc" 
                        ? filteredSubscriptions.OrderByDescending(s => s.UserId)
                        : filteredSubscriptions.OrderBy(s => s.UserId),
                    _ => filteredSubscriptions.OrderByDescending(s => s.CreatedAt)
                };
            }
            else
            {
                filteredSubscriptions = filteredSubscriptions.OrderByDescending(s => s.CreatedAt);
            }
            
            var totalCount = filteredSubscriptions.Count();
            var subscriptions = filteredSubscriptions
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            
            var dtos = _mapper.Map<IEnumerable<SubscriptionDto>>(subscriptions);
            
            // Return with pagination metadata
            return new JsonModel 
            { 
                data = new
                {
                    data = dtos,
                    meta = new
                    {
                        totalRecords = totalCount,
                        pageSize = pageSize,
                        currentPage = page,
                        totalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                        defaultPageSize = pageSize
                    }
                },
                Message = "User subscriptions retrieved successfully", 
                StatusCode = 200 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all user subscriptions");
            return new JsonModel { data = new object(), Message = "Failed to retrieve user subscriptions", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> CancelUserSubscriptionAsync(string subscriptionId, string? reason, TokenModel tokenModel)
    {
        try
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (subscription == null)
                return new JsonModel { data = new object(), Message = "Subscription not found", StatusCode = 404 };
            
            subscription.Status = "Cancelled";
            // subscription.CancelledAt = DateTime.UtcNow; // Property doesn't exist
            // subscription.CancellationReason = reason; // Property doesn't exist
            subscription.UpdatedAt = DateTime.UtcNow;
            
            await _subscriptionRepository.UpdateAsync(subscription);
            return new JsonModel { data = true, Message = "Subscription cancelled successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling user subscription {SubscriptionId}", subscriptionId);
            return new JsonModel { data = new object(), Message = "Failed to cancel subscription", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> PauseUserSubscriptionAsync(string subscriptionId, TokenModel tokenModel)
    {
        try
        {
            // Admin only method - validate admin role
            if (tokenModel.RoleID != 1 && tokenModel.RoleID != 3)
            {
                return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };
            }

            var subscription = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (subscription == null)
                return new JsonModel { data = new object(), Message = "Subscription not found", StatusCode = 404 };
            
            subscription.Status = "Paused";
            // subscription.PausedAt = DateTime.UtcNow; // Property doesn't exist
            subscription.UpdatedAt = DateTime.UtcNow;
            
            await _subscriptionRepository.UpdateAsync(subscription);
            return new JsonModel { data = true, Message = "Subscription paused successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pausing user subscription {SubscriptionId}", subscriptionId);
            return new JsonModel { data = new object(), Message = "Failed to pause subscription", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> ResumeUserSubscriptionAsync(string subscriptionId, TokenModel tokenModel)
    {
        try
        {
            // Admin only method - validate admin role
            if (tokenModel.RoleID != 1 && tokenModel.RoleID != 3)
            {
                return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };
            }

            var subscription = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (subscription == null)
                return new JsonModel { data = new object(), Message = "Subscription not found", StatusCode = 404 };
            
            subscription.Status = "Active";
            // subscription.ResumedAt = DateTime.UtcNow; // Property doesn't exist
            subscription.UpdatedAt = DateTime.UtcNow;
            
            await _subscriptionRepository.UpdateAsync(subscription);
            return new JsonModel { data = true, Message = "Subscription resumed successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resuming user subscription {SubscriptionId}", subscriptionId);
            return new JsonModel { data = new object(), Message = "Failed to resume subscription", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> ExtendUserSubscriptionAsync(string subscriptionId, int additionalDays, TokenModel tokenModel)
    {
        try
        {
            // Admin only method - validate admin role
            if (tokenModel.RoleID != 1 && tokenModel.RoleID != 3)
            {
                return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };
            }

            var subscription = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (subscription == null)
                return new JsonModel { data = new object(), Message = "Subscription not found", StatusCode = 404 };
            
            if (subscription.EndDate.HasValue)
            {
                subscription.EndDate = subscription.EndDate.Value.AddDays(additionalDays);
            }
            subscription.UpdatedAt = DateTime.UtcNow;
            
            await _subscriptionRepository.UpdateAsync(subscription);
            return new JsonModel { data = true, Message = "Subscription extended successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extending user subscription {SubscriptionId}", subscriptionId);
            return new JsonModel { data = new object(), Message = "Failed to extend subscription", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> PerformBulkActionAsync(List<BulkActionRequestDto> actions, TokenModel tokenModel)
    {
        try
        {
            // Admin only method - validate admin role
            if (tokenModel.RoleID != 1 && tokenModel.RoleID != 3)
            {
                return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };
            }

            var results = new List<BulkActionResultDto>();
            
            foreach (var action in actions)
            {
                try
                {
                    JsonModel result = action.Action.ToLower() switch
                    {
                        "cancel" => await CancelUserSubscriptionAsync(action.SubscriptionId, action.Reason, tokenModel),
                        "pause" => await PauseUserSubscriptionAsync(action.SubscriptionId, tokenModel),
                        "resume" => await ResumeUserSubscriptionAsync(action.SubscriptionId, tokenModel),
                        "extend" => await ExtendUserSubscriptionAsync(action.SubscriptionId, action.AdditionalDays ?? 30, tokenModel),
                        _ => new JsonModel { data = new object(), Message = $"Unknown action: {action.Action}", StatusCode = 400 }
                    };
                    
                    results.Add(new BulkActionResultDto
                    {
                        SubscriptionId = action.SubscriptionId,
                        Action = action.Action,
                        Success = result.StatusCode == 200,
                        Message = result.Message
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error performing bulk action {Action} on subscription {SubscriptionId}", action.Action, action.SubscriptionId);
                    results.Add(new BulkActionResultDto
                    {
                        SubscriptionId = action.SubscriptionId,
                        Action = action.Action,
                        Success = false,
                        Message = "Internal error occurred"
                    });
                }
            }
            
            return new JsonModel { data = results, Message = "Bulk actions completed", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing bulk actions");
            return new JsonModel { data = new object(), Message = "Failed to perform bulk actions", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetAllSubscriptionPlansAsync(TokenModel tokenModel, string? searchTerm = null, string? categoryId = null, bool? isActive = null, int page = 1, int pageSize = 50)
    {
        try
        {
            // Admin only method - validate admin role
            if (tokenModel.RoleID != 1 && tokenModel.RoleID != 3)
            {
                return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };
            }

            // Get all plans first
            var allPlans = await _subscriptionRepository.GetAllSubscriptionPlansAsync();
            var plans = allPlans.AsEnumerable();

            // Apply search filter
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                plans = plans.Where(p => 
                    p.Name.ToLower().Contains(searchTerm) || 
                    p.Description.ToLower().Contains(searchTerm) ||
                    (p.ShortDescription != null && p.ShortDescription.ToLower().Contains(searchTerm)));
            }

            // Apply category filter - using BillingCycleId as a proxy for category for now
            if (!string.IsNullOrEmpty(categoryId) && Guid.TryParse(categoryId, out Guid categoryGuid))
            {
                plans = plans.Where(p => p.BillingCycleId == categoryGuid);
            }

            // Apply active filter
            if (isActive.HasValue)
            {
                plans = plans.Where(p => p.IsActive == isActive.Value);
            }

            // Apply pagination
            var totalCount = plans.Count();
            var pagedPlans = plans
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var dtos = _mapper.Map<IEnumerable<SubscriptionPlanDto>>(pagedPlans);
            
            return new JsonModel 
            { 
                data = new
                {
                    plans = dtos,
                    pagination = new
                    {
                        totalCount,
                        page,
                        pageSize,
                        totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                    }
                },
                Message = "Subscription plans retrieved successfully", 
                StatusCode = 200 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all subscription plans");
            return new JsonModel { data = new object(), Message = "Failed to retrieve subscription plans", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetActiveSubscriptionPlansAsync(TokenModel tokenModel)
    {
        try
        {
            // Admin only method - validate admin role
            if (tokenModel.RoleID != 1 && tokenModel.RoleID != 3)
            {
                return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };
            }

            var plans = await _subscriptionRepository.GetActiveSubscriptionPlansAsync();
            var dtos = _mapper.Map<IEnumerable<SubscriptionPlanDto>>(plans);
            return new JsonModel { data = dtos, Message = "Active subscription plans retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active subscription plans");
            return new JsonModel { data = new object(), Message = "Failed to retrieve active subscription plans", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetSubscriptionPlansByCategoryAsync(string category, TokenModel tokenModel)
    {
        try
        {
            // Admin only method - validate admin role
            if (tokenModel.RoleID != 1 && tokenModel.RoleID != 3)
            {
                return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };
            }

            // First get the category by name
            var categoryEntity = await _subscriptionRepository.GetCategoryByNameAsync(category);
            if (categoryEntity == null)
                return new JsonModel { data = new object(), Message = "Category not found", StatusCode = 404 };

            var plans = await _subscriptionRepository.GetSubscriptionPlansByCategoryAsync(categoryEntity.Id);
            var dtos = _mapper.Map<IEnumerable<SubscriptionPlanDto>>(plans);
            return new JsonModel { data = dtos, Message = "Subscription plans by category retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription plans by category {Category}", category);
            return new JsonModel { data = new object(), Message = "Failed to retrieve subscription plans by category", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetAllCategoriesAsync(int page, int pageSize, string? searchTerm, bool? isActive, TokenModel tokenModel)
    {
        try
        {
            // Admin only method - validate admin role
            if (tokenModel.RoleID != 1 && tokenModel.RoleID != 3)
            {
                return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };
            }

            // This method should be implemented in the CategoryService, not here
            // For now, return a placeholder response
            return new JsonModel { data = new object(), Message = "Categories service not implemented", StatusCode = 501 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all categories");
            return new JsonModel { data = new object(), Message = "Failed to retrieve categories", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetSubscriptionPlanAsync(string planId, TokenModel tokenModel)
    {
        try
        {
            // Admin only method - validate admin role
            if (tokenModel.RoleID != 1 && tokenModel.RoleID != 3)
            {
                return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };
            }

            var plan = await _subscriptionRepository.GetSubscriptionPlanByIdAsync(Guid.Parse(planId));
            if (plan == null)
                return new JsonModel { data = new object(), Message = "Subscription plan not found", StatusCode = 404 };
            return new JsonModel { data = _mapper.Map<SubscriptionPlanDto>(plan), Message = "Subscription plan retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription plan {PlanId}", planId);
            return new JsonModel { data = new object(), Message = "Failed to retrieve subscription plan", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> CreateSubscriptionPlanAsync(CreateSubscriptionDto createDto, TokenModel tokenModel)
    {
        try
        {
            // Admin only method - validate admin role
            if (tokenModel.RoleID != 1 && tokenModel.RoleID != 3)
            {
                return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };
            }

            var plan = _mapper.Map<SubscriptionPlan>(createDto);
            plan.CreatedAt = DateTime.UtcNow;
            plan.IsActive = true;

            var createdPlan = await _subscriptionRepository.CreateSubscriptionPlanAsync(plan);
            var dto = _mapper.Map<SubscriptionPlanDto>(createdPlan);

            await _auditService.LogActionAsync(
                "SubscriptionPlan",
                "SubscriptionPlanCreated",
                createdPlan.Id.ToString(),
                $"Created plan: {createdPlan.Name}",
                tokenModel
            );

            return new JsonModel { data = dto, Message = "Subscription plan created successfully", StatusCode = 201 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating subscription plan");
            return new JsonModel { data = new object(), Message = "Failed to create subscription plan", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> UpdateSubscriptionPlanAsync(string planId, UpdateSubscriptionPlanDto updateDto, TokenModel tokenModel)
    {
        try
        {
            // Admin only method - validate admin role
            if (tokenModel.RoleID != 1 && tokenModel.RoleID != 3)
            {
                return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };
            }

            var plan = await _subscriptionRepository.GetSubscriptionPlanByIdAsync(Guid.Parse(planId));
            if (plan == null)
                return new JsonModel { data = new object(), Message = "Subscription plan not found", StatusCode = 404 };

            // Update plan properties
            if (!string.IsNullOrEmpty(updateDto.Name))
                plan.Name = updateDto.Name;
            
            if (!string.IsNullOrEmpty(updateDto.Description))
                plan.Description = updateDto.Description;
            
            if (updateDto.IsActive)
                plan.IsActive = updateDto.IsActive;
            
            if (updateDto.DisplayOrder.HasValue)
                plan.DisplayOrder = updateDto.DisplayOrder.Value;

            plan.UpdatedAt = DateTime.UtcNow;
            
            var updatedPlan = await _subscriptionRepository.UpdateSubscriptionPlanAsync(plan);
            var dto = _mapper.Map<SubscriptionPlanDto>(updatedPlan);

            await _auditService.LogActionAsync(
                "SubscriptionPlan",
                "SubscriptionPlanUpdated",
                planId,
                $"Updated plan: {updatedPlan.Name}",
                tokenModel
            );

            return new JsonModel { data = dto, Message = "Subscription plan updated successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating subscription plan {PlanId}", planId);
            return new JsonModel { data = new object(), Message = "Failed to update subscription plan", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> DeleteSubscriptionPlanAsync(string planId, TokenModel tokenModel)
    {
        try
        {
            // Admin only method - validate admin role
            if (tokenModel.RoleID != 1 && tokenModel.RoleID != 3)
            {
                return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };
            }

            var plan = await _subscriptionRepository.GetSubscriptionPlanByIdAsync(Guid.Parse(planId));
            if (plan == null)
                return new JsonModel { data = new object(), Message = "Subscription plan not found", StatusCode = 404 };

            // Check if plan has active subscriptions
            var activeSubscriptions = await _subscriptionRepository.GetActiveSubscriptionsAsync();
            if (activeSubscriptions.Any(s => s.SubscriptionPlanId == plan.Id))
                return new JsonModel { data = new object(), Message = "Cannot delete plan with active subscriptions", StatusCode = 400 };

            var result = await _subscriptionRepository.DeleteSubscriptionPlanAsync(plan.Id);
            if (!result)
                return new JsonModel { data = new object(), Message = "Failed to delete subscription plan", StatusCode = 500 };

            await _auditService.LogActionAsync(
                "SubscriptionPlan",
                "SubscriptionPlanDeleted",
                planId,
                $"Deleted plan: {plan.Name}",
                tokenModel
            );

            return new JsonModel { data = true, Message = "Subscription plan deleted successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting subscription plan {PlanId}", planId);
            return new JsonModel { data = new object(), Message = "Failed to delete subscription plan", StatusCode = 500 };
        }
    }

    // Example: Booking a consultation using privilege system
    public async Task<JsonModel> BookConsultationAsync(Guid userId, Guid subscriptionId, TokenModel tokenModel)
    {
        // Check if user has remaining consult privileges
        var remaining = await _privilegeService.GetRemainingPrivilegeAsync(subscriptionId, "Teleconsultation", tokenModel);
        if (remaining <= 0)
            return new JsonModel { data = new object(), Message = "No teleconsultations remaining in your plan.", StatusCode = 400 };
        var used = await _privilegeService.UsePrivilegeAsync(subscriptionId, "Teleconsultation", 1, tokenModel);
        if (!used)
            return new JsonModel { data = new object(), Message = "Failed to use teleconsultation privilege.", StatusCode = 500 };
        // Proceed with booking logic (not shown)
        return new JsonModel { data = true, Message = "Consultation booked.", StatusCode = 200 };
    }

    // Example: Medication supply using privilege system
    public async Task<JsonModel> RequestMedicationSupplyAsync(Guid userId, Guid subscriptionId, TokenModel tokenModel)
    {
        var remaining = await _privilegeService.GetRemainingPrivilegeAsync(subscriptionId, "MedicationSupply", tokenModel);
        if (remaining <= 0)
            return new JsonModel { data = new object(), Message = "No medication supply privilege remaining in your plan.", StatusCode = 400 };
        var used = await _privilegeService.UsePrivilegeAsync(subscriptionId, "MedicationSupply", 1, tokenModel);
        if (!used)
            return new JsonModel { data = new object(), Message = "Failed to use medication supply privilege.", StatusCode = 500 };
        // Proceed with medication supply logic (not shown)
        return new JsonModel { data = true, Message = "Medication supply requested.", StatusCode = 200 };
    }

    // --- PAYMENT & BILLING EDGE CASES ---

    // 1. Handle failed payment and update subscription status
    public async Task<JsonModel> HandleFailedPaymentAsync(string subscriptionId, string reason, TokenModel tokenModel)
    {
        try
        {
            // Admin only method - validate admin role
            if (tokenModel.RoleID != 1 && tokenModel.RoleID != 3)
            {
                return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };
            }

            var entity = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (entity == null)
                return new JsonModel { data = new object(), Message = "Subscription not found", StatusCode = 404 };

            entity.Status = Subscription.SubscriptionStatuses.PaymentFailed;
            entity.LastPaymentError = reason;
            entity.FailedPaymentAttempts += 1;
            entity.LastPaymentFailedDate = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;

            await _subscriptionRepository.UpdateAsync(entity);

            // Add status history
            await _subscriptionRepository.AddStatusHistoryAsync(new SubscriptionStatusHistory {
                SubscriptionId = entity.Id,
                FromStatus = entity.Status,
                ToStatus = Subscription.SubscriptionStatuses.PaymentFailed,
                Reason = reason,
                ChangedAt = DateTime.UtcNow
            });

            // Send payment failed notification
            var userResult = await _userService.GetUserByIdAsync(entity.UserId, tokenModel);
            if (userResult.StatusCode == 200 && userResult.data != null)
            {
                var billingRecord = new BillingRecordDto { Amount = entity.CurrentPrice, DueDate = DateTime.UtcNow, Description = reason };
                await _notificationService.SendPaymentFailedEmailAsync(((UserDto)userResult.data).Email, ((UserDto)userResult.data).FullName, billingRecord, tokenModel);
            }

            // Audit log
            await _auditService.LogPaymentEventAsync(entity.UserId, "PaymentFailed", subscriptionId, "Failed", reason, tokenModel);

            return new JsonModel { data = new object(), Message = $"Payment failed: {reason}", StatusCode = 400 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling failed payment for subscription {SubscriptionId}", subscriptionId);
            return new JsonModel { data = new object(), Message = "Failed to handle payment failure", StatusCode = 500 };
        }
    }

    // 2. Retry payment and reactivate subscription if successful
    public async Task<JsonModel> RetryPaymentAsync(string subscriptionId, PaymentRequestDto paymentRequest, TokenModel tokenModel)
    {
        var entity = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
        if (entity == null)
            return new JsonModel { data = new object(), Message = "Subscription not found", StatusCode = 404 };
        // Simulate payment retry logic
        var paymentResult = await _stripeService.ProcessPaymentAsync(entity.UserId.ToString(), paymentRequest.Amount, "USD", tokenModel);
        if (paymentResult.Status == "succeeded")
        {
            // Send payment success email
            var userResult = await _userService.GetUserByIdAsync(entity.UserId, tokenModel);
            if (userResult.StatusCode == 200 && userResult.data != null)
            {
                var billingRecord = new BillingRecordDto { Amount = paymentRequest.Amount, PaidDate = DateTime.UtcNow, Description = "Retry Payment" };
                await _notificationService.SendPaymentSuccessEmailAsync(((UserDto)userResult.data).Email, ((UserDto)userResult.data).FullName, billingRecord, tokenModel);
            }
            entity.Status = Subscription.SubscriptionStatuses.Active;
            entity.UpdatedAt = DateTime.UtcNow;
            await _subscriptionRepository.UpdateAsync(entity);
            // TODO: Trigger notification to user
            return new JsonModel { data = paymentResult, Message = "Payment retried and subscription reactivated", StatusCode = 200 };
        }
        else
        {
            return new JsonModel { data = new object(), Message = $"Payment retry failed: {paymentResult.ErrorMessage}", StatusCode = 400 };
        }
    }

    // 3. Auto-renewal logic (to be called by a scheduler/cron job)
    public async Task<JsonModel> AutoRenewSubscriptionAsync(string subscriptionId, TokenModel tokenModel)
    {
        var entity = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
        if (entity == null)
            return new JsonModel { data = new object(), Message = "Subscription not found", StatusCode = 404 };
        if (entity.Status != Subscription.SubscriptionStatuses.Active)
            return new JsonModel { data = new object(), Message = "Only active subscriptions can be auto-renewed", StatusCode = 400 };
        // Simulate payment
        var paymentResult = await _stripeService.ProcessPaymentAsync(entity.UserId.ToString(), entity.CurrentPrice, "USD", tokenModel);
        if (paymentResult.Status == "succeeded")
        {
            // Send renewal confirmation email
            var userResult = await _userService.GetUserByIdAsync(entity.UserId, tokenModel);
            if (userResult.StatusCode == 200 && userResult.data != null)
            {
                var billingRecord = new BillingRecordDto { Amount = entity.CurrentPrice, PaidDate = DateTime.UtcNow, Description = "Auto-Renewal" };
                await _notificationService.SendPaymentSuccessEmailAsync(((UserDto)userResult.data).Email, ((UserDto)userResult.data).FullName, billingRecord, tokenModel);
            }
            entity.NextBillingDate = entity.NextBillingDate.AddMonths(1);
            entity.UpdatedAt = DateTime.UtcNow;
            await _subscriptionRepository.UpdateAsync(entity);
            // TODO: Add billing history record
            return new JsonModel { data = _mapper.Map<SubscriptionDto>(entity), Message = "Subscription auto-renewed", StatusCode = 200 };
        }
        else
        {
                            await HandleFailedPaymentAsync(subscriptionId, paymentResult.ErrorMessage ?? "Auto-renewal payment failed", tokenModel);
            return new JsonModel { data = new object(), Message = $"Auto-renewal payment failed: {paymentResult.ErrorMessage}", StatusCode = 400 };
        }
    }

    // 4. Prorated upgrades/downgrades
    public async Task<JsonModel> ProrateUpgradeAsync(string subscriptionId, string newPlanId, TokenModel tokenModel)
    {
        var entity = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
        if (entity == null)
            return new JsonModel { data = new object(), Message = "Subscription not found", StatusCode = 404 };
        if (entity.SubscriptionPlanId == Guid.Parse(newPlanId))
            return new JsonModel { data = new object(), Message = "Already on this plan", StatusCode = 400 };
        // Simulate proration calculation
        var daysLeft = (entity.NextBillingDate - DateTime.UtcNow).TotalDays;
        var oldPlan = await _subscriptionRepository.GetSubscriptionPlanByIdAsync(entity.SubscriptionPlanId);
        var newPlan = await _subscriptionRepository.GetSubscriptionPlanByIdAsync(Guid.Parse(newPlanId));
        if (oldPlan == null || newPlan == null)
            return new JsonModel { data = new object(), Message = "Plan not found", StatusCode = 404 };
        // In proration, use Price and BillingCycleId
        var credit = (decimal)(daysLeft / 30.0) * oldPlan.Price; // Assuming Price is the monthly price
        var charge = newPlan.Price - credit;
        // Simulate payment for the difference
        var paymentResult = await _stripeService.ProcessPaymentAsync(entity.UserId.ToString(), charge, "USD", tokenModel);
        if (paymentResult.Status == "succeeded")
        {
            entity.SubscriptionPlanId = newPlan.Id;
            entity.UpdatedAt = DateTime.UtcNow;
            await _subscriptionRepository.UpdateAsync(entity);
            // TODO: Add billing history record
            return new JsonModel { data = _mapper.Map<SubscriptionDto>(entity), Message = "Subscription upgraded with proration", StatusCode = 200 };
        }
        else
        {
            return new JsonModel { data = new object(), Message = $"Prorated upgrade payment failed: {paymentResult.ErrorMessage}", StatusCode = 400 };
        }
    }

    // --- USAGE & LIMITS ---

    // Check if user can use a privilege (e.g., book a consult)
    public async Task<JsonModel> CanUsePrivilegeAsync(string subscriptionId, string privilegeName, TokenModel tokenModel)
    {
        try
        {
            // Validate token permissions
            if (tokenModel.RoleID != 1 && !await HasAccessToSubscription(tokenModel.UserID, subscriptionId))
            {
                return new JsonModel { data = new object(), Message = "Access denied", StatusCode = 403 };
            }

            var subscription = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (subscription == null || subscription.Status != Subscription.SubscriptionStatuses.Active)
                return new JsonModel { data = new object(), Message = "Subscription not active", StatusCode = 400 };

            var planPrivileges = await _planPrivilegeRepo.GetByPlanIdAsync(subscription.SubscriptionPlanId);
            var planPrivilege = planPrivileges.FirstOrDefault(p => p.Privilege.Name == privilegeName);
            if (planPrivilege == null)
                return new JsonModel { data = new object(), Message = "Privilege not included in plan", StatusCode = 400 };

            var usages = await _usageRepo.GetBySubscriptionIdAsync(subscription.Id);
            var usage = usages.FirstOrDefault(u => u.SubscriptionPlanPrivilegeId == planPrivilege.Id);
            int used = usage?.UsedValue ?? 0;
            int allowed = planPrivilege.Value;

            if (used >= allowed)
                return new JsonModel { data = new object(), Message = $"Usage limit reached for {privilegeName}", StatusCode = 400 };

            return new JsonModel { data = true, Message = "Privilege can be used", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking privilege usage for subscription {SubscriptionId}", subscriptionId);
            return new JsonModel { data = new object(), Message = "Failed to check privilege usage", StatusCode = 500 };
        }
    }

    // Increment privilege usage (to be called after successful action)
    public async Task IncrementPrivilegeUsageAsync(string subscriptionId, string privilegeName)
    {
        var subscription = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
        if (subscription == null) return;
        var planPrivileges = await _planPrivilegeRepo.GetByPlanIdAsync(subscription.SubscriptionPlanId);
        var planPrivilege = planPrivileges.FirstOrDefault(p => p.Privilege.Name == privilegeName);
        if (planPrivilege == null) return;
        var usages = await _usageRepo.GetBySubscriptionIdAsync(subscription.Id);
        var usage = usages.FirstOrDefault(u => u.SubscriptionPlanPrivilegeId == planPrivilege.Id);
        if (usage == null)
        {
            usage = new UserSubscriptionPrivilegeUsage
            {
                SubscriptionId = subscription.Id,
                SubscriptionPlanPrivilegeId = planPrivilege.Id,
                UsedValue = 1,
                UsagePeriodStart = DateTime.UtcNow,
                UsagePeriodEnd = DateTime.UtcNow.AddMonths(1)
            };
            await _usageRepo.AddAsync(usage);
        }
        else
        {
            usage.UsedValue += 1;
            await _usageRepo.UpdateAsync(usage);
        }
    }

    // Reset usage counters for all active subscriptions (to be called by a scheduler/cron job at billing cycle start)
    public async Task ResetAllUsageCountersAsync()
    {
        var activeSubscriptions = await _subscriptionRepository.GetActiveSubscriptionsAsync();
        foreach (var subscription in activeSubscriptions)
        {
            var usages = await _usageRepo.GetBySubscriptionIdAsync(subscription.Id);
            foreach (var usage in usages)
            {
                usage.UsedValue = 0;
                await _usageRepo.UpdateAsync(usage);
            }
        }
    }

    // Expire unused benefits (e.g., free consults) if not used within the period
    public async Task ExpireUnusedBenefitsAsync()
    {
        var activeSubscriptions = await _subscriptionRepository.GetActiveSubscriptionsAsync();
        foreach (var subscription in activeSubscriptions)
        {
            var usages = await _usageRepo.GetBySubscriptionIdAsync(subscription.Id);
            foreach (var usage in usages)
            {
                // For now, just reset to 0 at expiry; can be extended for carry-over logic
                usage.UsedValue = 0;
                await _usageRepo.UpdateAsync(usage);
            }
        }
    }

    // --- ADMIN OPERATIONS ---

    // Deactivate a plan (admin action)
    public async Task<JsonModel> DeactivatePlanAsync(string planId, string adminUserId, TokenModel tokenModel)
    {
        // Admin only method - validate admin role
        if (tokenModel.RoleID != 1 && tokenModel.RoleID != 3)
        {
            return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };
        }

        // Example: deactivate plan and notify/pause all subscribers
        var plan = await _subscriptionRepository.GetSubscriptionPlanByIdAsync(Guid.Parse(planId));
        if (plan == null)
            return new JsonModel { data = new object(), Message = "Plan not found", StatusCode = 404 };
        plan.IsActive = false;
        await _subscriptionRepository.UpdateSubscriptionPlanAsync(plan);
        // TODO: Pause all subscribers and notify them
        return new JsonModel { data = true, Message = "Plan deactivated and subscribers notified/paused.", StatusCode = 200 };
    }

    // Bulk cancel subscriptions (admin action)
    public async Task<JsonModel> BulkCancelSubscriptionsAsync(IEnumerable<string> subscriptionIds, string adminUserId, TokenModel tokenModel, string? reason = null)
    {
        int cancelled = 0;
        foreach (var id in subscriptionIds)
        {
            var sub = await _subscriptionRepository.GetByIdAsync(Guid.Parse(id));
            if (sub != null && sub.Status == Subscription.SubscriptionStatuses.Active)
            {
                sub.Status = Subscription.SubscriptionStatuses.Cancelled;
                sub.CancellationReason = reason ?? "Bulk admin cancel";
                sub.CancelledDate = DateTime.UtcNow;
                await _subscriptionRepository.UpdateAsync(sub);
                var userResult = await _userService.GetUserByIdAsync(sub.UserId, tokenModel);
                if (userResult.StatusCode == 200 && userResult.data != null)
                {
                    // Send subscription cancellation email
                    await _notificationService.SendSubscriptionCancellationAsync(((UserDto)userResult.data).Email, ((UserDto)userResult.data).FullName, _mapper.Map<SubscriptionDto>(sub), tokenModel);
                    _logger.LogInformation("Subscription cancellation email sent to {Email}", ((UserDto)userResult.data).Email);
                }
                await _auditService.LogUserActionAsync(int.Parse(adminUserId), "BulkCancelSubscription", "Subscription", id, "Cancelled by admin", tokenModel);
                cancelled++;
            }
        }
        return new JsonModel { data = cancelled, Message = $"{cancelled} subscriptions cancelled.", StatusCode = 200 };
    }

    // Bulk upgrade subscriptions (admin action)
    public async Task<JsonModel> BulkUpgradeSubscriptionsAsync(IEnumerable<string> subscriptionIds, string newPlanId, string adminUserId, TokenModel tokenModel)
    {
        int upgraded = 0;
        foreach (var id in subscriptionIds)
        {
            var sub = await _subscriptionRepository.GetByIdAsync(Guid.Parse(id));
            if (sub != null && sub.Status == Subscription.SubscriptionStatuses.Active && sub.SubscriptionPlanId != Guid.Parse(newPlanId))
            {
                sub.SubscriptionPlanId = Guid.Parse(newPlanId);
                sub.UpdatedAt = DateTime.UtcNow;
                await _subscriptionRepository.UpdateAsync(sub);
                var userResult = await _userService.GetUserByIdAsync(sub.UserId, tokenModel);
                if (userResult.StatusCode == 200 && userResult.data != null)
                {
                    // Send subscription confirmation email
                    await _notificationService.SendSubscriptionConfirmationAsync(((UserDto)userResult.data).Email, ((UserDto)userResult.data).FullName, _mapper.Map<SubscriptionDto>(sub), tokenModel);
                    _logger.LogInformation("Subscription confirmation email sent to {Email}", ((UserDto)userResult.data).Email);
                }
                await _auditService.LogUserActionAsync(int.Parse(adminUserId), "BulkUpgradeSubscription", "Subscription", id, $"Upgraded to plan {newPlanId}", tokenModel);
                upgraded++;
            }
        }
        return new JsonModel { data = upgraded, Message = $"{upgraded} subscriptions upgraded.", StatusCode = 200 };
    }

    public async Task<JsonModel> HandlePaymentProviderWebhookAsync(string eventType, string subscriptionId, string? errorMessage, TokenModel tokenModel)
    {
        try
        {
            // Admin only method - validate admin role
            if (tokenModel.RoleID != 1 && tokenModel.RoleID != 3)
            {
                return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };
            }

            if (eventType == "payment_failed")
            {
                await HandleFailedPaymentAsync(subscriptionId, errorMessage ?? "Unknown error", tokenModel);
                return new JsonModel { data = true, Message = "Payment failure handled", StatusCode = 200 };
            }
            if (eventType == "payment_succeeded")
            {
                var sub = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
                if (sub != null)
                {
                    sub.Status = Subscription.SubscriptionStatuses.Active;
                    await _subscriptionRepository.UpdateAsync(sub);
                    
                    var userResult = await _userService.GetUserByIdAsync(sub.UserId, tokenModel);
                    if (userResult.StatusCode == 200 && userResult.data != null)
                    {
                        var billingRecord = new BillingRecordDto { Amount = sub.CurrentPrice, PaidDate = DateTime.UtcNow, Description = "Webhook Payment Success" };
                        // Send payment success email
                        await _notificationService.SendPaymentSuccessEmailAsync(((UserDto)userResult.data).Email, ((UserDto)userResult.data).FullName, billingRecord, tokenModel);
                        _logger.LogInformation("Payment success email sent to {Email}", ((UserDto)userResult.data).Email);
                    }
                    await _auditService.LogPaymentEventAsync(sub.UserId, "PaymentSucceeded", subscriptionId, "Succeeded", null, tokenModel);
                }
                return new JsonModel { data = true, Message = "Payment success handled", StatusCode = 200 };
            }
            
            return new JsonModel { data = new object(), Message = "Unhandled webhook event type", StatusCode = 400 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling payment provider webhook for subscription {SubscriptionId}", subscriptionId);
            return new JsonModel { data = new object(), Message = "Failed to handle webhook event", StatusCode = 500 };
        }
    }

    // Additional methods for comprehensive subscription management
    public async Task<JsonModel> GetSubscriptionAnalyticsAsync(string subscriptionId, DateTime? startDate, DateTime? endDate, TokenModel tokenModel)
    {
        try
        {
            // Validate token permissions
            if (tokenModel.RoleID != 1 && !await HasAccessToSubscription(tokenModel.UserID, subscriptionId))
            {
                return new JsonModel { data = new object(), Message = "Access denied", StatusCode = 403 };
            }

            var subscription = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (subscription == null)
                return new JsonModel { data = new object(), Message = "Subscription not found", StatusCode = 404 };

            // Get usage statistics
            var usageStats = await GetUsageStatisticsAsync(subscriptionId, tokenModel);
            if (usageStats.StatusCode != 200)
                return new JsonModel { data = new object(), Message = "Failed to get usage statistics", StatusCode = 500 };

            // Get billing history with date range
            var billingHistory = await _billingService.GetPaymentHistoryAsync(subscription.UserId, startDate, endDate, tokenModel);

            var analytics = new SubscriptionAnalyticsDto
            {
                SubscriptionId = subscriptionId,
                PlanName = subscription.SubscriptionPlan.Name,
                Status = subscription.Status,
                StartDate = subscription.StartDate,
                NextBillingDate = subscription.NextBillingDate,
                TotalAmountPaid = billingHistory.data is IEnumerable<BillingRecordDto> billingData5 ? billingData5.Sum(bh => bh.Amount) : 0,
                PaymentCount = billingHistory.data is IEnumerable<BillingRecordDto> billingData6 ? billingData6.Count() : 0,
                AveragePaymentAmount = billingHistory.data is IEnumerable<BillingRecordDto> billingData7 && billingData7.Any() ? billingData7.Average(bh => bh.Amount) : 0,
                UsageStatistics = usageStats.data is UsageStatisticsDto usageData ? usageData : null,
                PaymentHistory = billingHistory.data is IEnumerable<BillingRecordDto> billingData8 ? billingData8.Select(bh => new PaymentHistoryDto
                {
                    Id = Guid.TryParse(bh.Id, out Guid id) ? id : Guid.Empty,
                    UserId = bh.UserId,
                    SubscriptionId = bh.SubscriptionId ?? string.Empty,
                    Amount = bh.Amount,
                    Currency = bh.Currency,
                    PaymentMethod = bh.PaymentMethod,
                    Status = bh.Status,
                    TransactionId = bh.StripePaymentIntentId,
                    ErrorMessage = bh.FailureReason,
                    CreatedAt = bh.CreatedAt,
                    ProcessedAt = bh.PaidAt,
                    PaymentDate = bh.CreatedAt,
                    Description = bh.Description,
                    PaymentMethodId = bh.PaymentMethodId
                }).ToList() : new List<PaymentHistoryDto>()
            };

            return new JsonModel { data = analytics, Message = "Subscription analytics retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription analytics for subscription {SubscriptionId}", subscriptionId);
            return new JsonModel { data = new object(), Message = "Failed to retrieve subscription analytics", StatusCode = 500 };
        }
    }

    // Helper method to check if user has access to subscription
    private async Task<bool> HasAccessToSubscription(int userId, string subscriptionId)
    {
        try
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            return subscription != null && subscription.UserId == userId;
        }
        catch
        {
            return false;
        }
    }

    // Export methods
    public async Task<JsonModel> ExportSubscriptionPlansAsync(TokenModel tokenModel, string? searchTerm = null, string? categoryId = null, bool? isActive = null, string format = "csv")
    {
        try
        {
            // Admin only method - validate admin role
            if (tokenModel.RoleID != 1 && tokenModel.RoleID != 3)
            {
                return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };
            }

            // Get filtered plans using the existing method
            var plansResult = await GetAllSubscriptionPlansAsync(tokenModel, searchTerm, categoryId, isActive, 1, int.MaxValue);
            
            if (plansResult.StatusCode != 200)
            {
                return plansResult;
            }

            // Extract plans from the result
            var plansData = plansResult.data as dynamic;
            var plans = plansData?.plans as IEnumerable<SubscriptionPlanDto>;
            
            if (plans == null)
            {
                return new JsonModel { data = new object(), Message = "No plans found for export", StatusCode = 404 };
            }

            // Generate export data based on format
            var exportData = format.ToLower() == "csv" 
                ? GenerateSubscriptionPlansCsv(plans)
                : GenerateSubscriptionPlansExcel(plans);

            return new JsonModel 
            { 
                data = new { exportData, format, fileName = $"subscription_plans_{DateTime.UtcNow:yyyyMMdd}.{format}" }, 
                Message = "Export data generated successfully", 
                StatusCode = 200 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting subscription plans");
            return new JsonModel { data = new object(), Message = "Failed to export subscription plans", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> ExportCategoriesAsync(TokenModel tokenModel, string? searchTerm = null, bool? isActive = null, string format = "csv")
    {
        try
        {
            // Admin only method - validate admin role
            if (tokenModel.RoleID != 1 && tokenModel.RoleID != 3)
            {
                return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };
            }

            // For now, return a placeholder since categories service is not fully implemented
            return new JsonModel 
            { 
                data = new { exportData = "Categories export not yet implemented", format, fileName = $"categories_{DateTime.UtcNow:yyyyMMdd}.{format}" }, 
                Message = "Categories export not yet implemented", 
                StatusCode = 501 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting categories");
            return new JsonModel { data = new object(), Message = "Failed to export categories", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetSubscriptionAnalyticsAsync(TokenModel tokenModel, string? searchTerm = null, string? categoryId = null, bool? isActive = null)
    {
        try
        {
            // Admin only method - validate admin role
            if (tokenModel.RoleID != 1 && tokenModel.RoleID != 3)
            {
                return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };
            }

            // Get filtered plans
            var plansResult = await GetAllSubscriptionPlansAsync(tokenModel, searchTerm, categoryId, isActive, 1, int.MaxValue);
            
            if (plansResult.StatusCode != 200)
            {
                return plansResult;
            }

            // Extract plans from the result
            var plansData = plansResult.data as dynamic;
            var plans = plansData?.plans as IEnumerable<SubscriptionPlanDto>;
            
            if (plans == null)
            {
                return new JsonModel { data = new object(), Message = "No plans found for analytics", StatusCode = 404 };
            }

            // Generate analytics data
            var analytics = new
            {
                totalPlans = plans.Count(),
                activePlans = plans.Count(p => p.IsActive),
                inactivePlans = plans.Count(p => !p.IsActive),
                averagePrice = plans.Any() ? plans.Average(p => p.Price) : 0,
                totalSubscriptions = 0, // SubscriberCount not available in current DTO
                plansByCategory = plans.GroupBy(p => p.BillingCycleId).Select(g => new { category = g.Key.ToString(), count = g.Count() }),
                priceRange = new
                {
                    min = plans.Any() ? plans.Min(p => p.Price) : 0,
                    max = plans.Any() ? plans.Max(p => p.Price) : 0
                }
            };

            return new JsonModel { data = analytics, Message = "Subscription analytics retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription analytics");
            return new JsonModel { data = new object(), Message = "Failed to retrieve subscription analytics", StatusCode = 500 };
        }
    }

    // Helper methods for export generation
    private string GenerateSubscriptionPlansCsv(IEnumerable<SubscriptionPlanDto> plans)
    {
        var csv = new System.Text.StringBuilder();
        csv.AppendLine("Name,Description,Price,BillingCycleId,IsActive,Features,Terms,CreatedAt");
        
        foreach (var plan in plans)
        {
            csv.AppendLine($"\"{plan.Name}\",\"{plan.Description}\",{plan.Price},{plan.BillingCycleId},{plan.IsActive},\"{plan.Features ?? ""}\",\"{plan.Terms ?? ""}\",{plan.CreatedAt:yyyy-MM-dd}");
        }
        
        return csv.ToString();
    }

    private string GenerateSubscriptionPlansExcel(IEnumerable<SubscriptionPlanDto> plans)
    {
        // For now, return CSV format as Excel generation would require additional libraries
        // In a real implementation, you'd use EPPlus or similar library
        return GenerateSubscriptionPlansCsv(plans);
    }
} 
