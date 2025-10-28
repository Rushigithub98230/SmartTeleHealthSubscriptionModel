using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Application.Utilities;
using SmartTelehealth.Application.Constants;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Application.Services;

/// <summary>
/// Service for managing subscription lifecycle operations including:
/// - Subscription creation, cancellation, pausing, resumption
/// - Subscription upgrades, renewals, and billing cycle changes
/// - Bulk lifecycle operations
/// - Status transitions and validation
/// - Trial management
/// </summary>
public class SubscriptionLifecycleService : ISubscriptionLifecycleService
{
    #region Constants
    
    // Note: Using Subscription.SubscriptionStatuses from Core.Entities for consistency
    // This ensures all status constants are centralized and consistent across the system
    
    #endregion

    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly ISubscriptionStatusHistoryRepository _statusHistoryRepository;
    private readonly ISubscriptionPlanRepository _subscriptionPlanRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<SubscriptionLifecycleService> _logger;
    private readonly IStripeService _stripeService;
    private readonly IPrivilegeService _privilegeService;
    private readonly INotificationService _notificationService;
    private readonly IUserService _userService;
    private readonly ISubscriptionPlanPrivilegeRepository _planPrivilegeRepo;
    private readonly IUserSubscriptionPrivilegeUsageRepository _usageRepo;
    private readonly ISubscriptionBillingService _billingService; // UPDATED: Use consolidated service
    private readonly ISubscriptionNotificationService _subscriptionNotificationService;
    private readonly IPrivilegeRepository _privilegeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IServiceProvider _serviceProvider;

    public SubscriptionLifecycleService(
        ISubscriptionRepository subscriptionRepository,
        ISubscriptionStatusHistoryRepository statusHistoryRepository,
        ISubscriptionPlanRepository subscriptionPlanRepository,
        IMapper mapper,
        ILogger<SubscriptionLifecycleService> logger,
        IStripeService stripeService,
        IPrivilegeService privilegeService,
        INotificationService notificationService,
        IUserService userService,
        ISubscriptionPlanPrivilegeRepository planPrivilegeRepo,
        IUserSubscriptionPrivilegeUsageRepository usageRepo,
        ISubscriptionBillingService billingService, // UPDATED: Use consolidated service
        ISubscriptionNotificationService subscriptionNotificationService,
        IPrivilegeRepository privilegeRepository,
        IUnitOfWork unitOfWork,
        IServiceProvider serviceProvider)
    {
        _subscriptionRepository = subscriptionRepository ?? throw new ArgumentNullException(nameof(subscriptionRepository));
        _statusHistoryRepository = statusHistoryRepository ?? throw new ArgumentNullException(nameof(statusHistoryRepository));
        _subscriptionPlanRepository = subscriptionPlanRepository ?? throw new ArgumentNullException(nameof(subscriptionPlanRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _stripeService = stripeService ?? throw new ArgumentNullException(nameof(stripeService));
        _privilegeService = privilegeService ?? throw new ArgumentNullException(nameof(privilegeService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _planPrivilegeRepo = planPrivilegeRepo ?? throw new ArgumentNullException(nameof(planPrivilegeRepo));
        _usageRepo = usageRepo ?? throw new ArgumentNullException(nameof(usageRepo));
        _billingService = billingService ?? throw new ArgumentNullException(nameof(billingService));
        _subscriptionNotificationService = subscriptionNotificationService ?? throw new ArgumentNullException(nameof(subscriptionNotificationService));
        _privilegeRepository = privilegeRepository ?? throw new ArgumentNullException(nameof(privilegeRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    #region Core Lifecycle Methods

    /// <summary>
    /// Creates a new subscription with proper validation and Stripe integration
    /// </summary>
    public async Task<JsonModel> CreateSubscriptionAsync(CreateSubscriptionDto createDto, TokenModel tokenModel)
    {
        try
        {
            // Step 1: Validate subscription plan exists and is active
            var requestedPlan = await _subscriptionRepository.GetSubscriptionPlanByIdAsync(Guid.Parse(createDto.PlanId));
            if (requestedPlan == null)
                return new JsonModel { data = new object(), Message = "Subscription plan does not exist", StatusCode = 404 };
            
            // CRITICAL FIX (Issue #12): Ensure new subscriptions always use the LATEST plan version
            // This ensures new users get current pricing and features, not outdated versions
            SubscriptionPlan plan;
            
            if (!requestedPlan.IsLatestVersion)
            {
                _logger.LogInformation("Plan {PlanId} (v{Version}) is not latest version. Finding latest version for new subscription.",
                    requestedPlan.Id, requestedPlan.VersionNumber);
                
                // Get parent plan ID (could be this plan itself if it's the original, or its parent if it's a version)
                var parentPlanId = requestedPlan.ParentPlanId ?? requestedPlan.Id;
                
                // Get all versions of this plan and find the latest active version
                var allVersions = await _subscriptionPlanRepository.GetAllVersionsOfPlanAsync(parentPlanId);
                var latestVersion = allVersions.FirstOrDefault(v => v.IsLatestVersion && v.IsActive);
                
                if (latestVersion != null && latestVersion.Id != requestedPlan.Id)
                {
                    _logger.LogInformation(
                        "Redirecting new subscription from plan {OldId} v{OldVer} (${OldPrice}) to latest version {NewId} v{NewVer} (${NewPrice})",
                        requestedPlan.Id, requestedPlan.VersionNumber, requestedPlan.BasePrice,
                        latestVersion.Id, latestVersion.VersionNumber, latestVersion.BasePrice);
                    
                    plan = latestVersion;  // Use latest version for new subscription
                }
                else
                {
                    _logger.LogWarning("Latest version not found for plan {PlanId}. Using requested plan. " +
                        "This may indicate a versioning configuration issue.",
                        requestedPlan.Id);
                    plan = requestedPlan;
                }
            }
            else
            {
                // Already the latest version or no versioning applied
                plan = requestedPlan;
                _logger.LogInformation("Plan {PlanId} is latest version (v{Version}), proceeding with subscription creation",
                    plan.Id, plan.VersionNumber);
            }
            
            if (!plan.IsActive)
                return new JsonModel { data = new object(), Message = "Subscription plan is not active", StatusCode = 400 };

            // Step 2: Prevent duplicate subscriptions for the same user and plan (active or paused)
            var userSubscriptions = await _subscriptionRepository.GetByUserIdAsync(createDto.UserId);
            if (userSubscriptions.Any(s => s.SubscriptionPlanId == plan.Id && (s.Status == Subscription.SubscriptionStatuses.Active || s.Status == Subscription.SubscriptionStatuses.Paused)))
                return new JsonModel { data = new object(), Message = "User already has an active or paused subscription for this plan", StatusCode = 400 };

            // Step 3: Get user details for Stripe integration
            var userResult = await _userService.GetUserByIdAsync(createDto.UserId, tokenModel);
            UserDto? user = null;
            if (userResult.StatusCode == 200 && userResult.data != null)
            {
                user = (UserDto)userResult.data;
            }
            else
            {
                _logger.LogWarning("Failed to get user {UserId} for subscription creation by user {TokenUserId}. Proceeding without user details.", 
                    createDto.UserId, tokenModel?.UserID ?? 0);
            }

            // Step 4: Ensure Stripe Customer exists for payment processing
            string stripeCustomerId;
            try
            {
                if (user != null)
                {
                    // Create or retrieve existing Stripe customer
                    stripeCustomerId = await EnsureStripeCustomerAsync(user, tokenModel);
                }
                else
                {
                    // For test environments or when user service is not available, use a default customer ID
                    stripeCustomerId = $"test_customer_{createDto.UserId}";
                    _logger.LogInformation("Using test customer ID {CustomerId} for user {UserId}", stripeCustomerId, createDto.UserId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create Stripe customer for user {UserId}", createDto.UserId);
                return new JsonModel { data = new object(), Message = "Failed to create payment customer", StatusCode = 500 };
            }

            // Step 5: Validate Payment Method if provided
            if (!string.IsNullOrEmpty(createDto.PaymentMethodId))
            {
                try
                {
                    // Validate the payment method with Stripe to ensure it's valid and can be used
                    var isValid = await _stripeService.ValidatePaymentMethodAsync(createDto.PaymentMethodId, tokenModel);
                    if (!isValid)
                    {
                        return new JsonModel { data = new object(), Message = "Invalid payment method", StatusCode = 400 };
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to validate payment method {PaymentMethodId} for user {UserId}", createDto.PaymentMethodId, createDto.UserId);
                    return new JsonModel { data = new object(), Message = "Payment method validation failed", StatusCode = 400 };
                }
            }

            // NEW ARCHITECTURE: Billing cycle comes from the plan, no separate validation needed
            // Each plan has a fixed billing cycle
            _logger.LogInformation("Using plan's fixed billing cycle: {BillingCycle}", plan.BillingCycle.Name);
            
            // Step 7: Create Stripe Subscription with plan's billing cycle
            string stripeSubscriptionId = null;
            // NEW ARCHITECTURE: Get or create Stripe price ID for effective price
            string stripePriceId = await GetOrCreateStripePriceForPlan(plan, tokenModel);
            
            try
            {
                _logger.LogInformation("Creating Stripe subscription for user {UserId} with plan {PlanName} (billing cycle: {BillingCycle}) using price ID {StripePriceId}", 
                    createDto.UserId, plan.Name, plan.BillingCycle.Name, stripePriceId);
                
                // Create the actual Stripe subscription
                stripeSubscriptionId = await _stripeService.CreateSubscriptionAsync(
                    stripeCustomerId,
                    stripePriceId,
                    createDto.PaymentMethodId,
                    tokenModel
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create Stripe subscription for user {UserId} with plan {PlanId}", createDto.UserId, createDto.PlanId);
                return new JsonModel { data = new object(), Message = "Failed to create payment subscription", StatusCode = 500 };
            }

            // Step 8: Create local subscription entity with Stripe IDs
            var entity = _mapper.Map<Subscription>(createDto);
            
            // NEW: Set Stripe integration fields
            entity.StripeCustomerId = stripeCustomerId;
            entity.StripeSubscriptionId = stripeSubscriptionId;
            entity.StripePriceId = stripePriceId;
            entity.PaymentMethodId = createDto.PaymentMethodId;
            
            // CRITICAL FIX: Use centralized effective price calculation
            entity.CurrentPrice = BillingCalculationService.GetEffectivePlanPrice(plan, null, _logger);
            
            _logger.LogInformation("Using effective plan price for subscription: " +
                "PlanName={PlanName}, BillingCycle={BillingCycle}, BasePrice=${BasePrice}, EffectivePrice=${EffectivePrice}",
                plan.Name, plan.BillingCycle.Name, plan.BasePrice, entity.CurrentPrice);
            
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
            // NEW ARCHITECTURE: Use plan's billing cycle (not from DTO)
            entity.NextBillingDate = BillingCycleCalculator.CalculateNextBillingDate(DateTime.UtcNow, plan.BillingCycle);
            
            // Set EndDate based on plan's billing cycle
            entity.EndDate = BillingCycleCalculator.CalculateEndDateForCycle(DateTime.UtcNow, plan.BillingCycle);
            
            // Set audit properties for creation
            entity.IsActive = true;
            entity.CreatedBy = tokenModel.UserID;
            entity.CreatedDate = DateTime.UtcNow;
            
            // BEGIN TRANSACTION - Ensure subscription, status history, billing record, and privileges are created atomically
            await _unitOfWork.BeginTransactionAsync();
            
            Subscription created;
            try
            {
                created = await _subscriptionRepository.CreateAsync(entity);
                
                // SRP Refactoring: Use centralized status history helper method
                await RecordStatusChangeAsync(
                    created.Id,
                    null,
                    created.Status,
                    "Subscription created",
                    tokenModel
                );
                
                // CRITICAL FIX: Create initial billing record within the same transaction
                await CreateInitialBillingRecordAsync(created, plan, tokenModel);
                
                // CRITICAL FIX: Allocate initial privileges within the same transaction
                await AllocateInitialPrivilegesAsync(created, plan, tokenModel);
                
                // COMMIT TRANSACTION
                await _unitOfWork.CommitTransactionAsync();
                
                _logger.LogInformation("Successfully created subscription {SubscriptionId} with status history, billing record, and privileges in transaction", created.Id);
            }
            catch (Exception ex)
            {
                // ROLLBACK TRANSACTION on any error
                await _unitOfWork.RollbackTransactionAsync();
                
                // CRITICAL: Clean up Stripe subscription if it was created but database failed
                if (!string.IsNullOrEmpty(stripeSubscriptionId))
                {
                    try
                    {
                        _logger.LogWarning("Cleaning up Stripe subscription {StripeSubscriptionId} due to database failure for user {UserId}", 
                            stripeSubscriptionId, createDto.UserId);
                        
                        // Cancel the Stripe subscription
                        await _stripeService.CancelSubscriptionAsync(stripeSubscriptionId, tokenModel);
                        
                        _logger.LogInformation("Successfully cleaned up Stripe subscription {StripeSubscriptionId} for failed subscription creation", 
                            stripeSubscriptionId);
                    }
                    catch (Exception cleanupEx)
                    {
                        _logger.LogError(cleanupEx, "Failed to cleanup Stripe subscription {StripeSubscriptionId} for user {UserId}. Manual cleanup may be required.", 
                            stripeSubscriptionId, createDto.UserId);
                    }
                }
                
                _logger.LogError(ex, "Failed to create subscription in transaction, rolling back");
                throw;
            }
            
            var dto = _mapper.Map<SubscriptionDto>(created);
            
            // Send confirmation and welcome emails
            if (user != null)
            {
                // Send subscription confirmation and welcome emails
                await _notificationService.SendSubscriptionConfirmationAsync(user.Email, user.FullName, dto, tokenModel);
                await _notificationService.SendSubscriptionWelcomeEmailAsync(user.Email, user.FullName, dto, tokenModel);
                
                // Send subscription created notification via the subscription notification service
                await _subscriptionNotificationService.SendSubscriptionCreatedNotificationAsync(created.Id.ToString(), tokenModel);
                
                _logger.LogInformation("Subscription confirmation, welcome emails, and created notification sent to {Email}", user.Email);
            }
            
            _logger.LogInformation("Successfully created subscription {SubscriptionId} for user {UserId} with Stripe subscription {StripeSubscriptionId}", 
                created.Id, createDto.UserId, stripeSubscriptionId);
            
            return new JsonModel { data = dto, Message = "Subscription created successfully with payment integration", StatusCode = 201 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating subscription for user {UserId}", createDto.UserId);
            return new JsonModel { data = new object(), Message = "Failed to create subscription", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Cancels a subscription with proper validation and Stripe synchronization
    /// </summary>
    public async Task<JsonModel> CancelSubscriptionAsync(string subscriptionId, string? reason, TokenModel tokenModel)
    {
        try
        {
            // Validate token permissions - ensure user has access to this subscription
            if (tokenModel.RoleID != (int)RoleId.Admin && !await HasAccessToSubscription(tokenModel.UserID, subscriptionId))
            {
                return new JsonModel { data = new object(), Message = "Access denied", StatusCode = 403 };
            }

            // Retrieve subscription entity from repository
            var entity = await _subscriptionRepository.GetByIdWithDetailsAsync(Guid.Parse(subscriptionId));
            if (entity == null)
                return new JsonModel { data = new object(), Message = "Subscription not found", StatusCode = 404 };
            
            // Prevent cancelling an already cancelled subscription
            if (entity.IsCancelled)
                return new JsonModel { data = new object(), Message = "Subscription is already cancelled", StatusCode = 400 };
            
            // Validate status transition - ensure cancellation is allowed from current status
            var validation = entity.ValidateStatusTransition(Subscription.SubscriptionStatuses.Cancelled);
            if (validation != ValidationResult.Success)
                return new JsonModel { data = new object(), Message = validation.ErrorMessage, StatusCode = 400 };
            
            var oldStatus = entity.Status;
            
            // Track Stripe cancellation for potential recovery
            bool stripeCancelled = false;
            string originalStripeSubscriptionId = entity.StripeSubscriptionId;
            
            // NEW: Cancel Stripe subscription first
            if (!string.IsNullOrEmpty(entity.StripeSubscriptionId))
            {
                try
                {
                    var stripeCancelResult = await _stripeService.CancelSubscriptionAsync(
                        entity.StripeSubscriptionId,
                        tokenModel
                    );
                    
                    if (stripeCancelResult)
                    {
                        stripeCancelled = true;
                        _logger.LogInformation("Successfully cancelled Stripe subscription {StripeSubscriptionId} for subscription {SubscriptionId}", 
                            entity.StripeSubscriptionId, subscriptionId);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to cancel Stripe subscription {StripeSubscriptionId} for subscription {SubscriptionId}. Proceeding with local cancellation only.", 
                            entity.StripeSubscriptionId, subscriptionId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error cancelling Stripe subscription {StripeSubscriptionId} for subscription {SubscriptionId}. Proceeding with local cancellation only.", 
                        entity.StripeSubscriptionId, subscriptionId);
                    // Don't fail the entire operation if Stripe cancellation fails
                }
            }
            else
            {
                _logger.LogWarning("Subscription {SubscriptionId} has no Stripe subscription ID. Cannot cancel Stripe subscription.", subscriptionId);
            }
            
            // Update local subscription
            entity.Status = Subscription.SubscriptionStatuses.Cancelled;
            entity.CancellationReason = reason;
            entity.CancelledDate = DateTime.UtcNow;
            
            // BEGIN TRANSACTION - Ensure subscription update and status history are atomic
            await _unitOfWork.BeginTransactionAsync();
            
            Subscription updated;
            try
            {
                updated = await _subscriptionRepository.UpdateAsync(entity);
                
                // SRP Refactoring: Use centralized status history helper method
                await RecordStatusChangeAsync(updated.Id, oldStatus, updated.Status, reason, tokenModel);
                
                // COMMIT TRANSACTION
                await _unitOfWork.CommitTransactionAsync();
                
                _logger.LogInformation("Successfully cancelled subscription {SubscriptionId} with status history in transaction", updated.Id);
                
                // REMOVED: Automatic refunds on cancellation
                // Refunds for mid-cycle cancellations should be processed manually by admin through the admin portal
                // Admin has full control to determine and initiate any applicable refund
                // NOTE: To process refund manually, admin should use:
                //       POST /api/Billing/{billingRecordId}/process-refund
                // await ProcessCancellationRefundsAsync(updated, tokenModel);
            }
            catch (Exception ex)
            {
                // ROLLBACK TRANSACTION on any error
                await _unitOfWork.RollbackTransactionAsync();
                
                // CRITICAL: If Stripe was cancelled but database update failed, we need to recover
                if (stripeCancelled && !string.IsNullOrEmpty(originalStripeSubscriptionId))
                {
                    try
                    {
                        _logger.LogWarning("Attempting to recover Stripe subscription {StripeSubscriptionId} due to database cancellation failure for subscription {SubscriptionId}", 
                            originalStripeSubscriptionId, subscriptionId);
                        
                        // Reactivate the Stripe subscription by updating to active price
                        // Note: This is a simplified recovery - in production you might need more sophisticated logic
                        var reactivateResult = await _stripeService.UpdateSubscriptionAsync(
                            originalStripeSubscriptionId,
                            entity.StripePriceId ?? "", // Use the original price ID
                            tokenModel
                        );
                        
                        if (reactivateResult)
                        {
                            _logger.LogInformation("Successfully recovered Stripe subscription {StripeSubscriptionId} for subscription {SubscriptionId}", 
                                originalStripeSubscriptionId, subscriptionId);
                        }
                        else
                        {
                            _logger.LogWarning("Failed to recover Stripe subscription {StripeSubscriptionId} for subscription {SubscriptionId}. Manual recovery may be required.", 
                                originalStripeSubscriptionId, subscriptionId);
                        }
                    }
                    catch (Exception recoveryEx)
                    {
                        _logger.LogError(recoveryEx, "Failed to recover Stripe subscription {StripeSubscriptionId} for subscription {SubscriptionId}. Manual recovery may be required.", 
                            originalStripeSubscriptionId, subscriptionId);
                    }
                }
                
                _logger.LogError(ex, "Failed to cancel subscription in transaction, rolling back");
                throw;
            }
            
            var dto = _mapper.Map<SubscriptionDto>(updated);
            
            // Send cancellation email
            var userResult = await _userService.GetUserByIdAsync(entity.UserId, tokenModel);
            if (userResult.StatusCode == 200 && userResult.data != null)
            {
                // Send subscription cancellation email
                await _notificationService.SendSubscriptionCancelledNotificationAsync(((UserDto)userResult.data).Email, ((UserDto)userResult.data).FullName, dto, tokenModel);
                _logger.LogInformation("Subscription cancellation email sent to {Email}", ((UserDto)userResult.data).Email);
            }
            
            return new JsonModel { data = dto, Message = "Subscription cancelled successfully with Stripe synchronization", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling subscription {SubscriptionId}", subscriptionId);
            return new JsonModel { data = new object(), Message = "Failed to cancel subscription", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Pauses a subscription with proper validation and Stripe synchronization
    /// </summary>
    public async Task<JsonModel> PauseSubscriptionAsync(string subscriptionId, TokenModel tokenModel)
    {
        try
        {
            // Validate token permissions - ensure user has access to this subscription
            if (tokenModel.RoleID != (int)RoleId.Admin && !await HasAccessToSubscription(tokenModel.UserID, subscriptionId))
            {
                return new JsonModel { data = new object(), Message = "Access denied", StatusCode = 403 };
            }

            // Retrieve subscription entity from repository
            var entity = await _subscriptionRepository.GetByIdWithDetailsAsync(Guid.Parse(subscriptionId));
            if (entity == null)
                return new JsonModel { data = new object(), Message = "Subscription not found", StatusCode = 404 };
            
            // Check if subscription is already paused
            if (entity.IsPaused)
                return new JsonModel { data = new object(), Message = "Subscription is already paused", StatusCode = 400 };
            
            // Check if subscription is cancelled (cannot pause cancelled subscriptions)
            if (entity.IsCancelled)
                return new JsonModel { data = new object(), Message = "Cannot pause a cancelled subscription", StatusCode = 400 };
            
            // Validate status transition - ensure pause is allowed from current status
            var validation = entity.ValidateStatusTransition(Subscription.SubscriptionStatuses.Paused);
            if (validation != ValidationResult.Success)
                return new JsonModel { data = new object(), Message = validation.ErrorMessage, StatusCode = 400 };
            
            var oldStatus = entity.Status;
            
            // NEW: Pause Stripe subscription first
            if (!string.IsNullOrEmpty(entity.StripeSubscriptionId))
            {
                try
                {
                    var stripePauseResult = await _stripeService.PauseSubscriptionAsync(
                        entity.StripeSubscriptionId,
                        tokenModel
                    );
                    
                    if (stripePauseResult)
                    {
                        _logger.LogInformation("Successfully paused Stripe subscription {StripeSubscriptionId} for subscription {SubscriptionId}", 
                            entity.StripeSubscriptionId, subscriptionId);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to pause Stripe subscription {StripeSubscriptionId} for subscription {SubscriptionId}. Proceeding with local pause only.", 
                            entity.StripeSubscriptionId, subscriptionId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error pausing Stripe subscription {StripeSubscriptionId} for subscription {SubscriptionId}. Proceeding with local pause only.", 
                        entity.StripeSubscriptionId, subscriptionId);
                    // Don't fail the entire operation if Stripe pause fails
                }
            }
            else
            {
                _logger.LogWarning("Subscription {SubscriptionId} has no Stripe subscription ID. Cannot pause Stripe subscription.", subscriptionId);
            }
            
            // Update local subscription
            entity.Status = Subscription.SubscriptionStatuses.Paused;
            entity.PausedDate = DateTime.UtcNow;
            
            // BEGIN TRANSACTION - Ensure subscription update and status history are atomic
            await _unitOfWork.BeginTransactionAsync();
            
            Subscription updated;
            try
            {
                updated = await _subscriptionRepository.UpdateAsync(entity);
                
                // SRP Refactoring: Use centralized status history helper method
                await RecordStatusChangeAsync(updated.Id, oldStatus, updated.Status, "Subscription paused", tokenModel);
                
                // COMMIT TRANSACTION
                await _unitOfWork.CommitTransactionAsync();
                
                _logger.LogInformation("Successfully paused subscription {SubscriptionId} with status history in transaction", updated.Id);
            }
            catch (Exception ex)
            {
                // ROLLBACK TRANSACTION on any error
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Failed to pause subscription in transaction, rolling back");
                throw;
            }
            
            var dto = _mapper.Map<SubscriptionDto>(updated);
            
            // Send pause notification email
            var userResult = await _userService.GetUserByIdAsync(entity.UserId, tokenModel);
            if (userResult.StatusCode == 200 && userResult.data != null)
            {
                // Send subscription pause notification email
                await _notificationService.SendSubscriptionPausedNotificationAsync(((UserDto)userResult.data).Email, ((UserDto)userResult.data).FullName, dto, tokenModel);
                _logger.LogInformation("Subscription pause notification email sent to {Email}", ((UserDto)userResult.data).Email);
            }
            
            return new JsonModel { data = dto, Message = "Subscription paused successfully with Stripe synchronization", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pausing subscription {SubscriptionId}", subscriptionId);
            return new JsonModel { data = new object(), Message = "Failed to pause subscription", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Resumes a paused subscription with proper validation and Stripe synchronization
    /// </summary>
    public async Task<JsonModel> ResumeSubscriptionAsync(string subscriptionId, TokenModel tokenModel)
    {
        try
        {
            // Validate token permissions - ensure user has access to this subscription
            if (tokenModel.RoleID != (int)RoleId.Admin && !await HasAccessToSubscription(tokenModel.UserID, subscriptionId))
            {
                return new JsonModel { data = new object(), Message = "Access denied", StatusCode = 403 };
            }

            // Retrieve subscription entity from repository
            var entity = await _subscriptionRepository.GetByIdWithDetailsAsync(Guid.Parse(subscriptionId));
            if (entity == null)
                return new JsonModel { data = new object(), Message = "Subscription not found", StatusCode = 404 };
            
            // Check if subscription is currently paused
            if (!entity.IsPaused)
                return new JsonModel { data = new object(), Message = "Subscription is not paused", StatusCode = 400 };
            
            // Validate status transition
            var validation = entity.ValidateStatusTransition(Subscription.SubscriptionStatuses.Active);
            if (validation != ValidationResult.Success)
                return new JsonModel { data = new object(), Message = validation.ErrorMessage, StatusCode = 400 };
            
            var oldStatus = entity.Status;
            
            // NEW: Resume Stripe subscription first
            if (!string.IsNullOrEmpty(entity.StripeSubscriptionId))
            {
                try
                {
                    var stripeResumeResult = await _stripeService.ResumeSubscriptionAsync(
                        entity.StripeSubscriptionId,
                        tokenModel
                    );
                    
                    if (stripeResumeResult)
                    {
                        _logger.LogInformation("Successfully resumed Stripe subscription {StripeSubscriptionId} for subscription {SubscriptionId}", 
                            entity.StripeSubscriptionId, subscriptionId);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to resume Stripe subscription {StripeSubscriptionId} for subscription {SubscriptionId}. Proceeding with local resume only.", 
                            entity.StripeSubscriptionId, subscriptionId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error resuming Stripe subscription {StripeSubscriptionId} for subscription {SubscriptionId}. Proceeding with local resume only.", 
                        entity.StripeSubscriptionId, subscriptionId);
                    // Don't fail the entire operation if Stripe resume fails
                }
            }
            else
            {
                _logger.LogWarning("Subscription {SubscriptionId} has no Stripe subscription ID. Cannot resume Stripe subscription.", subscriptionId);
            }
            
            // Update local subscription
            entity.Status = Subscription.SubscriptionStatuses.Active;
            entity.ResumedDate = DateTime.UtcNow;
            
            // Recalculate next billing date based on pause duration
            if (entity.PausedDate.HasValue)
            {
                var pauseDuration = DateTime.UtcNow - entity.PausedDate.Value;
                entity.NextBillingDate = entity.NextBillingDate.Add(pauseDuration);
            }
            
            // BEGIN TRANSACTION - Ensure subscription update and status history are atomic
            await _unitOfWork.BeginTransactionAsync();
            
            Subscription updated;
            try
            {
                updated = await _subscriptionRepository.UpdateAsync(entity);
                
                // SRP Refactoring: Use centralized status history helper method
                await RecordStatusChangeAsync(updated.Id, oldStatus, updated.Status, "Subscription resumed", tokenModel);
                
                // COMMIT TRANSACTION
                await _unitOfWork.CommitTransactionAsync();
                
                _logger.LogInformation("Successfully resumed subscription {SubscriptionId} with status history in transaction", updated.Id);
            }
            catch (Exception ex)
            {
                // ROLLBACK TRANSACTION on any error
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Failed to resume subscription in transaction, rolling back");
                throw;
            }
            
            var dto = _mapper.Map<SubscriptionDto>(updated);
            
            // Send resume notification email
            var userResult = await _userService.GetUserByIdAsync(entity.UserId, tokenModel);
            if (userResult.StatusCode == 200 && userResult.data != null)
            {
                // Send subscription resume notification email
                await _notificationService.SendSubscriptionResumedNotificationAsync(((UserDto)userResult.data).Email, ((UserDto)userResult.data).FullName, dto, tokenModel);
                _logger.LogInformation("Subscription resume notification email sent to {Email}", ((UserDto)userResult.data).Email);
            }
            
            return new JsonModel { data = dto, Message = "Subscription resumed successfully with Stripe synchronization", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resuming subscription {SubscriptionId}", subscriptionId);
            return new JsonModel { data = new object(), Message = "Failed to resume subscription", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Reactivates a cancelled or expired subscription
    /// </summary>
    public async Task<JsonModel> ReactivateSubscriptionAsync(string subscriptionId, TokenModel tokenModel)
    {
        try
        {
            // Validate token permissions
            if (tokenModel.RoleID != (int)RoleId.Admin && !await HasAccessToSubscription(tokenModel.UserID, subscriptionId))
            {
                return new JsonModel { data = new object(), Message = "Access denied", StatusCode = 403 };
            }

            var entity = await _subscriptionRepository.GetByIdWithDetailsAsync(Guid.Parse(subscriptionId));
            if (entity == null)
                return new JsonModel { data = new object(), Message = "Subscription not found", StatusCode = 404 };
            
            // Validate status transition
            var validation = entity.ValidateStatusTransition(Subscription.SubscriptionStatuses.Active);
            if (validation != ValidationResult.Success)
                return new JsonModel { data = new object(), Message = validation.ErrorMessage, StatusCode = 400 };
            
            var oldStatus = entity.Status;
            
            // NEW: Reactivate Stripe subscription first
            if (!string.IsNullOrEmpty(entity.StripeSubscriptionId))
            {
                try
                {
                    // For reactivation, we need to create a new Stripe subscription since the old one was cancelled
                    // Get the current plan details
                    var currentPlan = await _subscriptionRepository.GetSubscriptionPlanByIdAsync(entity.SubscriptionPlanId);
                    if (currentPlan != null)
                    {
                        // NEW ARCHITECTURE: Get the plan's single Stripe price ID
                        string stripePriceId = GetStripePriceIdForPlan(currentPlan);
                        
                        var stripeSubscriptionResult = await _stripeService.CreateSubscriptionAsync(
                            entity.StripeCustomerId,
                            stripePriceId,
                            entity.PaymentMethodId,
                            tokenModel
                        );
                        
                        if (!string.IsNullOrEmpty(stripeSubscriptionResult))
                        {
                            _logger.LogInformation("Successfully reactivated Stripe subscription {NewStripeSubscriptionId} for subscription {SubscriptionId}", 
                                stripeSubscriptionResult, subscriptionId);
                            
                            // Update local subscription with new Stripe subscription ID
                            entity.StripeSubscriptionId = stripeSubscriptionResult;
                            entity.StripePriceId = stripePriceId;
                        }
                        else
                        {
                            _logger.LogWarning("Failed to reactivate Stripe subscription for subscription {SubscriptionId}. Proceeding with local reactivation only.", 
                                subscriptionId);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error reactivating Stripe subscription for subscription {SubscriptionId}. Proceeding with local reactivation only.", 
                        subscriptionId);
                    // Don't fail the entire operation if Stripe reactivation fails
                }
            }
            else
            {
                _logger.LogWarning("Subscription {SubscriptionId} has no Stripe customer ID. Cannot reactivate Stripe subscription.", subscriptionId);
            }
            
            // BEGIN TRANSACTION - Ensure atomic update and status history
            await _unitOfWork.BeginTransactionAsync();
            
            try
            {
                // Update local subscription
                entity.Status = Subscription.SubscriptionStatuses.Active;
                entity.UpdatedBy = tokenModel.UserID;
                entity.UpdatedDate = DateTime.UtcNow;
                
                var updated = await _subscriptionRepository.UpdateAsync(entity);
                
                // SRP Refactoring: Use centralized status history helper method
                await RecordStatusChangeAsync(updated.Id, oldStatus, updated.Status, "Subscription reactivated", tokenModel);
                
                await _unitOfWork.CommitTransactionAsync();
                
                // Send reactivation notification AFTER successful commit
                var userResult = await _userService.GetUserByIdAsync(updated.UserId, tokenModel);
                if (userResult.StatusCode == 200 && userResult.data != null)
                {
                    var dto = _mapper.Map<SubscriptionDto>(updated);
                    await _notificationService.SendSubscriptionWelcomeEmailAsync(((UserDto)userResult.data).Email, ((UserDto)userResult.data).FullName, dto, tokenModel);
                    _logger.LogInformation("Subscription reactivation notification sent to {Email}", ((UserDto)userResult.data).Email);
                }
                
                return new JsonModel { data = _mapper.Map<SubscriptionDto>(updated), Message = "Subscription reactivated successfully with Stripe synchronization", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error reactivating subscription {SubscriptionId} in transaction", subscriptionId);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reactivating subscription {SubscriptionId}", subscriptionId);
            return new JsonModel { data = new object(), Message = "Failed to reactivate subscription", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Upgrades a subscription to a new plan with proper validation and Stripe synchronization
    /// </summary>
    public async Task<JsonModel> UpgradeSubscriptionAsync(string subscriptionId, string newPlanId, TokenModel tokenModel)
    {
        try
        {
            // Validate token permissions - ensure user has access to this subscription
            if (tokenModel.RoleID != (int)RoleId.Admin && !await HasAccessToSubscription(tokenModel.UserID, subscriptionId))
            {
                return new JsonModel { data = new object(), Message = "Access denied", StatusCode = 403 };
            }

            // Retrieve subscription entity from repository
            var entity = await _subscriptionRepository.GetByIdWithDetailsAsync(Guid.Parse(subscriptionId));
            if (entity == null)
                return new JsonModel { data = new object(), Message = "Subscription not found", StatusCode = 404 };
            
            // Prevent upgrading to the same plan
            if (entity.SubscriptionPlanId == Guid.Parse(newPlanId))
                return new JsonModel { data = new object(), Message = "Subscription is already on this plan", StatusCode = 400 };
            
            // Get the new plan details
            var newPlan = await _subscriptionRepository.GetSubscriptionPlanByIdAsync(Guid.Parse(newPlanId));
            if (newPlan == null)
                return new JsonModel { data = new object(), Message = "New plan not found", StatusCode = 404 };

            var oldPlanId = entity.SubscriptionPlanId;
            
            // NEW: Update Stripe subscription with new price ID
            if (!string.IsNullOrEmpty(entity.StripeSubscriptionId))
            {
                try
                {
                    // NEW ARCHITECTURE: Get the plan's single Stripe price ID
                    string newStripePriceId = GetStripePriceIdForPlan(newPlan);
                    
                    var stripeUpdateResult = await _stripeService.UpdateSubscriptionAsync(
                        entity.StripeSubscriptionId,
                        newStripePriceId,
                        tokenModel
                    );
                    
                    if (stripeUpdateResult)
                    {
                        _logger.LogInformation("Successfully updated Stripe subscription {StripeSubscriptionId} for subscription {SubscriptionId} from plan {OldPlanId} to {NewPlanId}", 
                            entity.StripeSubscriptionId, subscriptionId, oldPlanId, newPlanId);
                        
                        // Update local subscription with new Stripe price ID
                        entity.StripePriceId = newStripePriceId;
                    }
                    else
                    {
                        _logger.LogWarning("Failed to update Stripe subscription {StripeSubscriptionId} for subscription {SubscriptionId}. Proceeding with local update only.", 
                            entity.StripeSubscriptionId, subscriptionId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating Stripe subscription {StripeSubscriptionId} for subscription {SubscriptionId}. Proceeding with local update only.", 
                        entity.StripeSubscriptionId, subscriptionId);
                    // Don't fail the entire operation if Stripe update fails
                }
            }
            else
            {
                _logger.LogWarning("Subscription {SubscriptionId} has no Stripe subscription ID. Cannot update Stripe.", subscriptionId);
            }
            
            // BEGIN TRANSACTION - Ensure atomic plan update
            await _unitOfWork.BeginTransactionAsync();
            
            try
            {
                // Update local subscription
                entity.SubscriptionPlanId = Guid.Parse(newPlanId);
                entity.UpdatedBy = tokenModel.UserID;
                entity.UpdatedDate = DateTime.UtcNow;
                
                var updated = await _subscriptionRepository.UpdateAsync(entity);
                
                await _unitOfWork.CommitTransactionAsync();
                
                return new JsonModel { data = _mapper.Map<SubscriptionDto>(updated), Message = "Subscription upgraded successfully with Stripe synchronization", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error updating subscription plan for {SubscriptionId} in transaction", subscriptionId);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error upgrading subscription {SubscriptionId}", subscriptionId);
            return new JsonModel { data = new object(), Message = "Failed to upgrade subscription", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Updates a subscription with proper validation
    /// </summary>
    public async Task<JsonModel> UpdateAsync(string subscriptionId, UpdateSubscriptionDto updateDto, TokenModel tokenModel)
    {
        try
        {
            // Validate token permissions - ensure user has access to this subscription
            if (tokenModel.RoleID != (int)RoleId.Admin && !await HasAccessToSubscription(tokenModel.UserID, subscriptionId))
            {
                return new JsonModel { data = new object(), Message = "Access denied", StatusCode = 403 };
            }

            // Retrieve subscription entity from repository
            var subscription = await _subscriptionRepository.GetByIdWithDetailsAsync(Guid.Parse(subscriptionId));
            if (subscription == null)
                return new JsonModel { data = new object(), Message = "Subscription not found", StatusCode = 404 };

            // BEGIN TRANSACTION - Ensure atomic update
            await _unitOfWork.BeginTransactionAsync();
            
            try
            {
                // Update subscription properties from DTO
                if (!string.IsNullOrEmpty(updateDto.Status))
                    subscription.Status = updateDto.Status;
                
                if (updateDto.AutoRenew.HasValue)
                    subscription.AutoRenew = updateDto.AutoRenew.Value;
                
                if (updateDto.NextBillingDate.HasValue)
                    subscription.NextBillingDate = updateDto.NextBillingDate.Value;

                subscription.UpdatedBy = tokenModel.UserID;
                subscription.UpdatedDate = DateTime.UtcNow;
                
                var updatedSubscription = await _subscriptionRepository.UpdateAsync(subscription);
                
                await _unitOfWork.CommitTransactionAsync();
                
                return new JsonModel { data = _mapper.Map<SubscriptionDto>(updatedSubscription), Message = "Subscription updated successfully", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error updating subscription {SubscriptionId} in transaction", subscriptionId);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating subscription {SubscriptionId}", subscriptionId);
            return new JsonModel { data = new object(), Message = "Failed to update subscription", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Bulk cancel subscriptions (admin action)
    /// </summary>
    public async Task<JsonModel> BulkCancelSubscriptionsAsync(IEnumerable<string> subscriptionIds, string adminUserId, TokenModel tokenModel, string? reason = null)
    {
        int cancelled = 0;
        var cancelledSubscriptions = new List<Subscription>();
        
        // BEGIN TRANSACTION - All-or-nothing bulk operation
        await _unitOfWork.BeginTransactionAsync();
        
        try
        {
            foreach (var id in subscriptionIds)
            {
                var sub = await _subscriptionRepository.GetByIdWithDetailsAsync(Guid.Parse(id));
                if (sub != null && sub.Status == Subscription.SubscriptionStatuses.Active)
                {
                    sub.Status = Subscription.SubscriptionStatuses.Cancelled;
                    sub.CancellationReason = reason ?? "Bulk admin cancel";
                    sub.CancelledDate = DateTime.UtcNow;
                    sub.UpdatedBy = int.Parse(adminUserId); // FIX: Add audit property
                    sub.UpdatedDate = DateTime.UtcNow;      // FIX: Add audit property
                    await _subscriptionRepository.UpdateAsync(sub);
                    cancelledSubscriptions.Add(sub);
                    cancelled++;
                }
            }
            
            await _unitOfWork.CommitTransactionAsync();
            
            // Send notifications AFTER successful commit
            foreach (var sub in cancelledSubscriptions)
            {
                var userResult = await _userService.GetUserByIdAsync(sub.UserId, tokenModel);
                if (userResult.StatusCode == 200 && userResult.data != null)
                {
                    try
                    {
                        await _notificationService.SendSubscriptionCancelledNotificationAsync(
                            ((UserDto)userResult.data).Email, 
                            ((UserDto)userResult.data).FullName, 
                            _mapper.Map<SubscriptionDto>(sub), 
                            tokenModel);
                        _logger.LogInformation("Subscription cancellation email sent to {Email}", ((UserDto)userResult.data).Email);
                    }
                    catch (Exception notifEx)
                    {
                        _logger.LogWarning(notifEx, "Failed to send cancellation notification for subscription {SubscriptionId}", sub.Id);
                        // Don't fail entire operation if notification fails
                    }
                }
            }
            
            return new JsonModel { data = cancelled, Message = $"{cancelled} subscriptions cancelled.", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error in bulk cancel operation, rolling back all changes");
            return new JsonModel { data = 0, Message = "Bulk cancel failed, no subscriptions were cancelled", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Bulk upgrade subscriptions (admin action)
    /// </summary>
    public async Task<JsonModel> BulkUpgradeSubscriptionsAsync(IEnumerable<string> subscriptionIds, string newPlanId, string adminUserId, TokenModel tokenModel)
    {
        int upgraded = 0;
        var upgradedSubscriptions = new List<Subscription>();
        
        // BEGIN TRANSACTION - All-or-nothing bulk operation
        await _unitOfWork.BeginTransactionAsync();
        
        try
        {
            foreach (var id in subscriptionIds)
            {
                var sub = await _subscriptionRepository.GetByIdWithDetailsAsync(Guid.Parse(id));
                if (sub != null && sub.Status == Subscription.SubscriptionStatuses.Active && sub.SubscriptionPlanId != Guid.Parse(newPlanId))
                {
                    sub.SubscriptionPlanId = Guid.Parse(newPlanId);
                    sub.UpdatedBy = tokenModel.UserID;
                    sub.UpdatedDate = DateTime.UtcNow;
                    await _subscriptionRepository.UpdateAsync(sub);
                    upgradedSubscriptions.Add(sub);
                    upgraded++;
                }
            }
            
            await _unitOfWork.CommitTransactionAsync();
            
            // Send notifications AFTER successful commit
            foreach (var sub in upgradedSubscriptions)
            {
                var userResult = await _userService.GetUserByIdAsync(sub.UserId, tokenModel);
                if (userResult.StatusCode == 200 && userResult.data != null)
                {
                    try
                    {
                        await _notificationService.SendSubscriptionConfirmationAsync(
                            ((UserDto)userResult.data).Email, 
                            ((UserDto)userResult.data).FullName, 
                            _mapper.Map<SubscriptionDto>(sub), 
                            tokenModel);
                        _logger.LogInformation("Subscription confirmation email sent to {Email}", ((UserDto)userResult.data).Email);
                    }
                    catch (Exception notifEx)
                    {
                        _logger.LogWarning(notifEx, "Failed to send upgrade notification for subscription {SubscriptionId}", sub.Id);
                        // Don't fail entire operation if notification fails
                    }
                }
            }
            
            return new JsonModel { data = upgraded, Message = $"{upgraded} subscriptions upgraded.", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error in bulk upgrade operation, rolling back all changes");
            return new JsonModel { data = 0, Message = "Bulk upgrade failed, no subscriptions were upgraded", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Performs bulk actions on subscriptions (admin only)
    /// </summary>
    public async Task<JsonModel> PerformBulkActionAsync(List<BulkActionRequestDto> actions, TokenModel tokenModel)
    {
        try
        {
            // Admin only method - validate admin role
            if (tokenModel.RoleID != (int)RoleId.Admin && tokenModel.RoleID != (int)RoleId.Provider)
            {
                return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };
            }

            var results = new List<BulkActionResultDto>();
            
            foreach (var action in actions)
            {
                try
                {
                    // Pre-validate subscription exists and action is appropriate
                    var subscription = await _subscriptionRepository.GetByIdAsync(Guid.Parse(action.SubscriptionId));
                    if (subscription == null)
                    {
                        results.Add(new BulkActionResultDto
                        {
                            SubscriptionId = action.SubscriptionId,
                            Action = action.Action,
                            Success = false,
                            Message = "Subscription not found"
                        });
                        continue;
                    }

                    // Validate if action is appropriate for current status
                    var isValidAction = await ValidateBulkActionAsync(subscription.Status, action.Action.ToLower());
                    if (!isValidAction)
                    {
                        results.Add(new BulkActionResultDto
                        {
                            SubscriptionId = action.SubscriptionId,
                            Action = action.Action,
                            Success = false,
                            Message = $"Action '{action.Action}' is not valid for subscription with status '{subscription.Status}'"
                        });
                        continue;
                    }

                    JsonModel result = action.Action.ToLower() switch
                    {
                        "cancel" => await CancelSubscriptionAsync(action.SubscriptionId, action.Reason, tokenModel),
                        "pause" => await PauseSubscriptionAsync(action.SubscriptionId, tokenModel),
                        "resume" => await ResumeSubscriptionAsync(action.SubscriptionId, tokenModel),
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

    /// <summary>
    /// Extends a user subscription by additional days
    /// </summary>
    public async Task<JsonModel> ExtendUserSubscriptionAsync(string subscriptionId, int additionalDays, TokenModel tokenModel)
    {
        try
        {
            // Validate token permissions
            if (tokenModel.RoleID != (int)RoleId.Admin && !await HasAccessToSubscription(tokenModel.UserID, subscriptionId))
            {
                return new JsonModel { data = new object(), Message = "Access denied", StatusCode = 403 };
            }

            var entity = await _subscriptionRepository.GetByIdWithDetailsAsync(Guid.Parse(subscriptionId));
            if (entity == null)
                return new JsonModel { data = new object(), Message = "Subscription not found", StatusCode = 404 };

            // BEGIN TRANSACTION - Ensure atomic update of EndDate and NextBillingDate
            await _unitOfWork.BeginTransactionAsync();
            
            try
            {
                // Extend the subscription
                entity.EndDate = entity.EndDate?.AddDays(additionalDays) ?? DateTime.UtcNow.AddDays(additionalDays);
                entity.NextBillingDate = entity.NextBillingDate.AddDays(additionalDays);
                entity.UpdatedBy = tokenModel.UserID;
                entity.UpdatedDate = DateTime.UtcNow;

                var updated = await _subscriptionRepository.UpdateAsync(entity);
                
                await _unitOfWork.CommitTransactionAsync();

                return new JsonModel { data = _mapper.Map<SubscriptionDto>(updated), Message = $"Subscription extended by {additionalDays} days", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error extending subscription {SubscriptionId} in transaction", subscriptionId);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extending subscription {SubscriptionId}", subscriptionId);
            return new JsonModel { data = new object(), Message = "Failed to extend subscription", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Auto-renews a subscription with payment processing
    /// </summary>
    public async Task<JsonModel> AutoRenewSubscriptionAsync(string subscriptionId, TokenModel tokenModel)
    {
        var entity = await _subscriptionRepository.GetByIdWithDetailsAsync(Guid.Parse(subscriptionId));
        if (entity == null)
            return new JsonModel { data = new object(), Message = "Subscription not found", StatusCode = 404 };
        if (entity.Status != Subscription.SubscriptionStatuses.Active)
            return new JsonModel { data = new object(), Message = "Only active subscriptions can be auto-renewed", StatusCode = 400 };
        
        // NEW: Process payment through Stripe with proper subscription renewal
        PaymentResultDto paymentResult;
        
        if (!string.IsNullOrEmpty(entity.StripeSubscriptionId))
        {
            try
            {
                // For Stripe auto-renewal, we should use the subscription's payment method
                // and process the renewal through Stripe's subscription renewal mechanism
                _logger.LogInformation("Processing auto-renewal for Stripe subscription {StripeSubscriptionId} for subscription {SubscriptionId}", 
                    entity.StripeSubscriptionId, subscriptionId);
                
                // Use Stripe service to process the renewal payment
                paymentResult = await _stripeService.ProcessPaymentAsync(
                    entity.PaymentMethodId ?? entity.UserId.ToString(), 
                    entity.CurrentPrice, 
                    "USD", 
                    tokenModel
                );
                
                if (paymentResult.Status == "succeeded")
                {
                    _logger.LogInformation("Successfully processed Stripe auto-renewal payment for subscription {SubscriptionId}", subscriptionId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Stripe auto-renewal for subscription {SubscriptionId}. Falling back to local payment processing.", subscriptionId);
                // Fallback to local payment processing
                paymentResult = await _stripeService.ProcessPaymentAsync(entity.UserId.ToString(), entity.CurrentPrice, "USD", tokenModel);
            }
        }
        else
        {
            // Fallback for subscriptions without Stripe integration
            paymentResult = await _stripeService.ProcessPaymentAsync(entity.UserId.ToString(), entity.CurrentPrice, "USD", tokenModel);
        }
        
        if (paymentResult.Status == "succeeded")
        {
            // Send renewal confirmation email
            var userResult = await _userService.GetUserByIdAsync(entity.UserId, tokenModel);
            if (userResult.StatusCode == 200 && userResult.data != null)
            {
                var billingRecord = new BillingRecordDto { Amount = entity.CurrentPrice, PaidDate = DateTime.UtcNow, Description = "Auto-Renewal" };
                await _notificationService.SendPaymentSuccessEmailAsync(((UserDto)userResult.data).Email, ((UserDto)userResult.data).FullName, billingRecord, tokenModel);
            }
            
            // FIXED: Use centralized calculator for consistency (handles all billing cycles correctly)
            entity.NextBillingDate = BillingCycleCalculator.CalculateNextBillingDate(
                entity.NextBillingDate, 
                entity.BillingCycle);
            entity.UpdatedBy = tokenModel.UserID;
            entity.UpdatedDate = DateTime.UtcNow;
            
            // Add status history for renewal
            await _subscriptionRepository.AddStatusHistoryAsync(new SubscriptionStatusHistory {
                SubscriptionId = entity.Id,
                FromStatus = entity.Status,
                ToStatus = entity.Status, // Same status, but renewed
                Reason = "Auto-renewal successful",
                ChangedAt = DateTime.UtcNow
            });
            
            await _subscriptionRepository.UpdateAsync(entity);
            
            return new JsonModel { data = _mapper.Map<SubscriptionDto>(entity), Message = "Subscription auto-renewed successfully with Stripe synchronization", StatusCode = 200 };
        }
        else
        {
            return new JsonModel { data = new object(), Message = $"Auto-renewal payment failed: {paymentResult.ErrorMessage}", StatusCode = 400 };
        }
    }

    /// <summary>
    /// Performs a prorated upgrade/downgrade of a subscription
    /// </summary>
    public async Task<JsonModel> ProrateUpgradeAsync(string subscriptionId, string newPlanId, TokenModel tokenModel)
    {
        var entity = await _subscriptionRepository.GetByIdWithDetailsAsync(Guid.Parse(subscriptionId));
        if (entity == null)
            return new JsonModel { data = new object(), Message = "Subscription not found", StatusCode = 404 };
        if (entity.SubscriptionPlanId == Guid.Parse(newPlanId))
            return new JsonModel { data = new object(), Message = "Already on this plan", StatusCode = 400 };
        
        // REFACTORED: Use centralized BillingCycleCalculator for accurate proration (PHASE 3)
        // This properly handles all billing cycles (monthly, quarterly, annual) and edge cases
        var oldPlan = await _subscriptionRepository.GetSubscriptionPlanByIdAsync(entity.SubscriptionPlanId);
        var newPlan = await _subscriptionRepository.GetSubscriptionPlanByIdAsync(Guid.Parse(newPlanId));
        if (oldPlan == null || newPlan == null)
            return new JsonModel { data = new object(), Message = "Plan not found", StatusCode = 404 };
        
        // Calculate unused credit from old plan using centralized proration
        var credit = BillingCycleCalculator.CalculateProratedAmount(
            entity,
            DateTime.UtcNow,
            entity.CurrentPrice,
            _logger
        );
        
        // Calculate charge for new plan (difference between new plan effective price and unused credit)
        var newPlanEffectivePrice = BillingCalculationService.GetEffectivePlanPrice(newPlan, null, _logger);
        var charge = newPlanEffectivePrice - credit;
        
        _logger.LogInformation(
            "Proration calculated for subscription {SubscriptionId}: OldPlanPrice={OldPrice}, " +
            "UnusedCredit={Credit}, NewPlanEffectivePrice={NewPrice}, ChargeAmount={Charge}",
            subscriptionId, entity.CurrentPrice, credit, newPlanEffectivePrice, charge);
        
        // NEW: Process prorated payment through Stripe with subscription upgrade
        if (!string.IsNullOrEmpty(entity.StripeSubscriptionId))
        {
            try
            {
                // For Stripe prorated upgrades, we should update the subscription with the new price
                // and let Stripe handle the proration calculation
                _logger.LogInformation("Processing prorated upgrade for Stripe subscription {StripeSubscriptionId} for subscription {SubscriptionId}", 
                    entity.StripeSubscriptionId, subscriptionId);
                
                // NEW ARCHITECTURE: Use Stripe service to update the subscription with new price
                var stripeUpdateResult = await _stripeService.UpdateSubscriptionAsync(
                    entity.StripeSubscriptionId,
                    GetStripePriceIdForPlan(newPlan),
                    tokenModel
                );
                
                if (stripeUpdateResult)
                {
                    _logger.LogInformation("Successfully updated Stripe subscription for prorated upgrade {SubscriptionId}", subscriptionId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Stripe prorated upgrade for subscription {SubscriptionId}. Proceeding with local update only.", subscriptionId);
                // Don't fail the entire operation if Stripe update fails
            }
        }
        
        // BEGIN TRANSACTION - Ensure atomic prorated upgrade
        await _unitOfWork.BeginTransactionAsync();
        
        try
        {
            // Update local subscription
            entity.SubscriptionPlanId = Guid.Parse(newPlanId);
            entity.CurrentPrice = BillingCalculationService.GetEffectivePlanPrice(newPlan, null, _logger);
            entity.UpdatedBy = tokenModel.UserID;
            entity.UpdatedDate = DateTime.UtcNow;
            
            var updated = await _subscriptionRepository.UpdateAsync(entity);
            
            await _unitOfWork.CommitTransactionAsync();
            
            return new JsonModel { data = _mapper.Map<SubscriptionDto>(updated), Message = "Subscription prorated upgrade completed successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error processing prorated upgrade for {SubscriptionId} in transaction", subscriptionId);
            throw;
        }
    }

    /// <summary>
    /// Changes the billing cycle of a subscription.
    /// NEW ARCHITECTURE: This operation is no longer supported.
    /// Billing cycles are now fixed per plan. To change billing frequency, users must switch to a different plan.
    /// </summary>
    [Obsolete("DEPRECATED: Billing cycles are now fixed per plan. Use plan upgrade/downgrade instead.")]
    public async Task<JsonModel> ChangeBillingCycleAsync(string subscriptionId, string newBillingCycleId, TokenModel tokenModel)
    {
        await Task.CompletedTask; // Suppress async warning
        
        _logger.LogWarning("ChangeBillingCycleAsync called for subscription {SubscriptionId} - operation no longer supported in new architecture", subscriptionId);
        
        return new JsonModel
        {
            data = new object(),
            Message = "Billing cycle cannot be changed directly. Each plan has a fixed billing cycle. Please upgrade/downgrade to a plan with your desired billing cycle instead.",
            StatusCode = 400
        };
    }

    #endregion

    public async Task<bool> ActivateSubscriptionAsync(Guid subscriptionId, string? reason = null, TokenModel tokenModel = null)
    {
        try
        {
            _logger.LogInformation("Activating subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            
            var subscription = await _subscriptionRepository.GetByIdWithDetailsAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            if (!await ValidateStatusTransitionAsync(subscription.Status, Subscription.SubscriptionStatuses.Active, tokenModel))
            {
                _logger.LogWarning("Invalid status transition from {CurrentStatus} to Active for subscription {SubscriptionId} by user {UserId}", 
                    subscription.Status, subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            var oldStatus = subscription.Status;
            subscription.Status = Subscription.SubscriptionStatuses.Active;
            subscription.UpdatedBy = tokenModel?.UserID;
            subscription.UpdatedDate = DateTime.UtcNow;

            // SRP Refactoring: Use centralized status history helper method
            await RecordStatusChangeAsync(subscriptionId, oldStatus, Subscription.SubscriptionStatuses.Active, reason ?? "Subscription activated", tokenModel);

            await _subscriptionRepository.UpdateAsync(subscription);
            
            // Send activation notification
            var userResult = await _userService.GetUserByIdAsync(subscription.UserId, tokenModel);
            if (userResult.StatusCode == 200 && userResult.data != null)
            {
                var dto = _mapper.Map<SubscriptionDto>(subscription);
                await _notificationService.SendSubscriptionConfirmationAsync(((UserDto)userResult.data).Email, ((UserDto)userResult.data).FullName, dto, tokenModel);
                _logger.LogInformation("Subscription activation notification sent to {Email}", ((UserDto)userResult.data).Email);
            }
            
            _logger.LogInformation("Successfully activated subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            return false;
        }
    }

    public async Task<bool> PauseSubscriptionAsync(Guid subscriptionId, string? reason = null, TokenModel tokenModel = null)
    {
        try
        {
            _logger.LogInformation("Pausing subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            
            var subscription = await _subscriptionRepository.GetByIdWithDetailsAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            if (!await ValidateStatusTransitionAsync(subscription.Status, Subscription.SubscriptionStatuses.Paused, tokenModel))
            {
                _logger.LogWarning("Invalid status transition from {CurrentStatus} to Paused for subscription {SubscriptionId} by user {UserId}", 
                    subscription.Status, subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            var oldStatus = subscription.Status;
            subscription.Status = Subscription.SubscriptionStatuses.Paused;
            subscription.UpdatedBy = tokenModel?.UserID;
            subscription.UpdatedDate = DateTime.UtcNow;

            // SRP Refactoring: Use centralized status history helper method
            await RecordStatusChangeAsync(subscriptionId, oldStatus, Subscription.SubscriptionStatuses.Paused, reason ?? "Subscription paused", tokenModel);

            await _subscriptionRepository.UpdateAsync(subscription);
            
            
            
            _logger.LogInformation("Successfully paused subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pausing subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            return false;
        }
    }

    public async Task<bool> ResumeSubscriptionAsync(Guid subscriptionId, string? reason = null, TokenModel tokenModel = null)
    {
        try
        {
            _logger.LogInformation("Resuming subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            
            var subscription = await _subscriptionRepository.GetByIdWithDetailsAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            if (!await ValidateStatusTransitionAsync(subscription.Status, Subscription.SubscriptionStatuses.Active, tokenModel))
            {
                _logger.LogWarning("Invalid status transition from {CurrentStatus} to Active for subscription {SubscriptionId} by user {UserId}", 
                    subscription.Status, subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            var oldStatus = subscription.Status;
            subscription.Status = Subscription.SubscriptionStatuses.Active;
            subscription.UpdatedBy = tokenModel?.UserID;
            subscription.UpdatedDate = DateTime.UtcNow;

            // SRP Refactoring: Use centralized status history helper method
            await RecordStatusChangeAsync(subscriptionId, oldStatus, Subscription.SubscriptionStatuses.Active, reason ?? "Subscription resumed", tokenModel);

            await _subscriptionRepository.UpdateAsync(subscription);
            
            
            
            _logger.LogInformation("Successfully resumed subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resuming subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            return false;
        }
    }

    public async Task<bool> CancelSubscriptionAsync(Guid subscriptionId, string? reason = null, TokenModel tokenModel = null)
    {
        try
        {
            _logger.LogInformation("Cancelling subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            
            var subscription = await _subscriptionRepository.GetByIdWithDetailsAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            if (!await ValidateStatusTransitionAsync(subscription.Status, Subscription.SubscriptionStatuses.Cancelled, tokenModel))
            {
                _logger.LogWarning("Invalid status transition from {CurrentStatus} to Cancelled for subscription {SubscriptionId} by user {UserId}", 
                    subscription.Status, subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            var oldStatus = subscription.Status;
            subscription.Status = Subscription.SubscriptionStatuses.Cancelled;
            subscription.UpdatedBy = tokenModel?.UserID;
            subscription.UpdatedDate = DateTime.UtcNow;
            subscription.CancelledAt = DateTime.UtcNow;

            // SRP Refactoring: Use centralized status history helper method
            await RecordStatusChangeAsync(subscriptionId, oldStatus, Subscription.SubscriptionStatuses.Cancelled, reason ?? "Subscription cancelled", tokenModel);

            await _subscriptionRepository.UpdateAsync(subscription);
            
            
            _logger.LogInformation("Successfully cancelled subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            return false;
        }
    }

    public async Task<bool> SuspendSubscriptionAsync(Guid subscriptionId, string? reason = null, TokenModel tokenModel = null)
    {
        try
        {
            _logger.LogInformation("Suspending subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            
            var subscription = await _subscriptionRepository.GetByIdWithDetailsAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            if (!await ValidateStatusTransitionAsync(subscription.Status, Subscription.SubscriptionStatuses.Suspended, tokenModel))
            {
                _logger.LogWarning("Invalid status transition from {CurrentStatus} to Suspended for subscription {SubscriptionId} by user {UserId}", 
                    subscription.Status, subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            var oldStatus = subscription.Status;
            subscription.Status = Subscription.SubscriptionStatuses.Suspended;
            subscription.UpdatedBy = tokenModel?.UserID;
            subscription.UpdatedDate = DateTime.UtcNow;

            // SRP Refactoring: Use centralized status history helper method
            await RecordStatusChangeAsync(subscriptionId, oldStatus, Subscription.SubscriptionStatuses.Suspended, reason ?? "Subscription suspended", tokenModel);

            await _subscriptionRepository.UpdateAsync(subscription);
            
            // Send suspension notification
            var userResult = await _userService.GetUserByIdAsync(subscription.UserId, tokenModel);
            if (userResult.StatusCode == 200 && userResult.data != null)
            {
                var dto = _mapper.Map<SubscriptionDto>(subscription);
                await _notificationService.SendSubscriptionSuspensionAsync(((UserDto)userResult.data).Email, ((UserDto)userResult.data).FullName, dto, tokenModel);
                _logger.LogInformation("Subscription suspension notification sent to {Email}", ((UserDto)userResult.data).Email);
            }
            
            _logger.LogInformation("Successfully suspended subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error suspending subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            return false;
        }
    }

    public async Task<bool> RenewSubscriptionAsync(Guid subscriptionId, string? reason = null, TokenModel tokenModel = null)
    {
        try
        {
            _logger.LogInformation("Renewing subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            
            var subscription = await _subscriptionRepository.GetByIdWithDetailsAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            if (!await ValidateStatusTransitionAsync(subscription.Status, Subscription.SubscriptionStatuses.Active, tokenModel))
            {
                _logger.LogWarning("Invalid status transition from {CurrentStatus} to Active for subscription {SubscriptionId} by user {UserId}", 
                    subscription.Status, subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            var oldStatus = subscription.Status;
            subscription.Status = Subscription.SubscriptionStatuses.Active;
            subscription.UpdatedBy = tokenModel?.UserID;
            subscription.UpdatedDate = DateTime.UtcNow;
            subscription.RenewedAt = DateTime.UtcNow;

            // SRP Refactoring: Use centralized status history helper method
            await RecordStatusChangeAsync(subscriptionId, oldStatus, Subscription.SubscriptionStatuses.Active, reason ?? "Subscription renewed", tokenModel);

            await _subscriptionRepository.UpdateAsync(subscription);
            
            
            
            _logger.LogInformation("Successfully renewed subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error renewing subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            return false;
        }
    }

    public async Task<bool> ExpireSubscriptionAsync(Guid subscriptionId, string? reason = null, TokenModel tokenModel = null)
    {
        try
        {
            _logger.LogInformation("Expiring subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            
            var subscription = await _subscriptionRepository.GetByIdWithDetailsAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            if (!await ValidateStatusTransitionAsync(subscription.Status, Subscription.SubscriptionStatuses.Expired, tokenModel))
            {
                _logger.LogWarning("Invalid status transition from {CurrentStatus} to Expired for subscription {SubscriptionId} by user {UserId}", 
                    subscription.Status, subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            var oldStatus = subscription.Status;
            subscription.Status = Subscription.SubscriptionStatuses.Expired;
            subscription.UpdatedBy = tokenModel?.UserID;
            subscription.UpdatedDate = DateTime.UtcNow;
            subscription.ExpiredAt = DateTime.UtcNow;

            // SRP Refactoring: Use centralized status history helper method
            await RecordStatusChangeAsync(subscriptionId, oldStatus, Subscription.SubscriptionStatuses.Expired, reason ?? "Subscription expired", tokenModel);

            await _subscriptionRepository.UpdateAsync(subscription);
            
            // Send expiration notification
            var userResult = await _userService.GetUserByIdAsync(subscription.UserId, tokenModel);
            if (userResult.StatusCode == 200 && userResult.data != null)
            {
                var dto = _mapper.Map<SubscriptionDto>(subscription);
                await _subscriptionNotificationService.SendSubscriptionExpiredNotificationAsync(subscriptionId.ToString(), tokenModel);
                _logger.LogInformation("Subscription expiration notification sent to {Email}", ((UserDto)userResult.data).Email);
            }
           
            _logger.LogInformation("Successfully expired subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error expiring subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            return false;
        }
    }

    public async Task<bool> MarkPaymentFailedAsync(Guid subscriptionId, string? reason = null, TokenModel tokenModel = null)
    {
        try
        {
            _logger.LogInformation("Marking payment failed for subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            
            var subscription = await _subscriptionRepository.GetByIdWithDetailsAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            if (!await ValidateStatusTransitionAsync(subscription.Status, Subscription.SubscriptionStatuses.PaymentFailed, tokenModel))
            {
                _logger.LogWarning("Invalid status transition from {CurrentStatus} to PaymentFailed for subscription {SubscriptionId} by user {UserId}", 
                    subscription.Status, subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            var oldStatus = subscription.Status;
            subscription.Status = Subscription.SubscriptionStatuses.PaymentFailed;
            subscription.UpdatedBy = tokenModel?.UserID;
            subscription.UpdatedDate = DateTime.UtcNow;

            // SRP Refactoring: Use centralized status history helper method
            await RecordStatusChangeAsync(subscriptionId, oldStatus, Subscription.SubscriptionStatuses.PaymentFailed, reason ?? "Payment failed", tokenModel);

            await _subscriptionRepository.UpdateAsync(subscription);
            
            // Send payment failed notification
            var userResult = await _userService.GetUserByIdAsync(subscription.UserId, tokenModel);
            if (userResult.StatusCode == 200 && userResult.data != null)
            {
                var billingRecord = new BillingRecordDto 
                { 
                    Amount = subscription.CurrentPrice, 
                    PaidDate = DateTime.UtcNow, 
                    Description = "Payment Failed" 
                };
                await _notificationService.SendPaymentFailedEmailAsync(((UserDto)userResult.data).Email, ((UserDto)userResult.data).FullName, billingRecord, tokenModel);
                _logger.LogInformation("Payment failed notification sent to {Email}", ((UserDto)userResult.data).Email);
            }
            
            _logger.LogInformation("Successfully marked payment failed for subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking payment failed for subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            return false;
        }
    }

    public async Task<bool> MarkPaymentSucceededAsync(Guid subscriptionId, string? reason = null, TokenModel tokenModel = null)
    {
        try
        {
            _logger.LogInformation("Marking payment succeeded for subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            
            var subscription = await _subscriptionRepository.GetByIdWithDetailsAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            if (!await ValidateStatusTransitionAsync(subscription.Status, Subscription.SubscriptionStatuses.Active, tokenModel))
            {
                _logger.LogWarning("Invalid status transition from {CurrentStatus} to Active for subscription {SubscriptionId} by user {UserId}", 
                    subscription.Status, subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            var oldStatus = subscription.Status;
            subscription.Status = Subscription.SubscriptionStatuses.Active;
            subscription.UpdatedBy = tokenModel?.UserID;
            subscription.UpdatedDate = DateTime.UtcNow;

            // SRP Refactoring: Use centralized status history helper method
            await RecordStatusChangeAsync(subscriptionId, oldStatus, Subscription.SubscriptionStatuses.Active, reason ?? "Payment succeeded", tokenModel);

            await _subscriptionRepository.UpdateAsync(subscription);
            
            // Send payment success notification
            var userResult = await _userService.GetUserByIdAsync(subscription.UserId, tokenModel);
            if (userResult.StatusCode == 200 && userResult.data != null)
            {
                var billingRecord = new BillingRecordDto 
                { 
                    Amount = subscription.CurrentPrice, 
                    PaidDate = DateTime.UtcNow, 
                    Description = "Payment Succeeded" 
                };
                await _notificationService.SendPaymentSuccessEmailAsync(((UserDto)userResult.data).Email, ((UserDto)userResult.data).FullName, billingRecord, tokenModel);
                _logger.LogInformation("Payment success notification sent to {Email}", ((UserDto)userResult.data).Email);
            }
            
            _logger.LogInformation("Successfully marked payment succeeded for subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking payment succeeded for subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            return false;
        }
    }

    public async Task<bool> UpdateSubscriptionStatusAsync(Guid subscriptionId, string newStatus, string? reason = null, TokenModel tokenModel = null)
    {
        try
        {
            _logger.LogInformation("Updating subscription {SubscriptionId} status to {NewStatus} by user {UserId}", 
                subscriptionId, newStatus, tokenModel?.UserID ?? 0);
            
            var subscription = await _subscriptionRepository.GetByIdWithDetailsAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            if (!await ValidateStatusTransitionAsync(subscription.Status, newStatus, tokenModel))
            {
                _logger.LogWarning("Invalid status transition from {CurrentStatus} to {NewStatus} for subscription {SubscriptionId} by user {UserId}", 
                    subscription.Status, newStatus, subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            var oldStatus = subscription.Status;
            subscription.Status = newStatus;
            subscription.UpdatedBy = tokenModel?.UserID;
            subscription.UpdatedDate = DateTime.UtcNow;

            // SRP Refactoring: Use centralized status history helper method
            await RecordStatusChangeAsync(subscriptionId, oldStatus, newStatus, reason ?? $"Status updated to {newStatus}", tokenModel);

            await _subscriptionRepository.UpdateAsync(subscription);
            
            
            _logger.LogInformation("Successfully updated subscription {SubscriptionId} status to {NewStatus} by user {UserId}", 
                subscriptionId, newStatus, tokenModel?.UserID ?? 0);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating subscription {SubscriptionId} status to {NewStatus} by user {UserId}", 
                subscriptionId, newStatus, tokenModel?.UserID ?? 0);
            return false;
        }
    }

    public async Task<JsonModel> UpdateSubscriptionStatusAsync(string subscriptionId, string newStatus, TokenModel tokenModel)
    {
        try
        {
            if (!Guid.TryParse(subscriptionId, out var subscriptionGuid))
            {
                return new JsonModel 
                { 
                    data = new object(), 
                    Message = "Invalid subscription ID format", 
                    StatusCode = 400 
                };
            }

            var success = await UpdateSubscriptionStatusAsync(subscriptionGuid, newStatus, null, tokenModel);
            
            if (success)
            {
                return new JsonModel 
                { 
                    data = new { subscriptionId, newStatus }, 
                    Message = "Subscription status updated successfully", 
                    StatusCode = 200 
                };
            }
            else
            {
                return new JsonModel 
                { 
                    data = new object(), 
                    Message = "Failed to update subscription status", 
                    StatusCode = 400 
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating subscription {SubscriptionId} status to {NewStatus}", subscriptionId, newStatus);
            return new JsonModel 
            { 
                data = new object(), 
                Message = $"Error updating subscription status: {ex.Message}", 
                StatusCode = 500 
            };
        }
    }

    public async Task<IEnumerable<SubscriptionStatusHistory>> GetStatusHistoryAsync(Guid subscriptionId, TokenModel tokenModel = null)
    {
        try
        {
            var history = await _statusHistoryRepository.GetBySubscriptionIdAsync(subscriptionId);
            
            _logger.LogInformation("Status history retrieved for subscription {SubscriptionId} by user {UserId}: {HistoryCount} records", 
                subscriptionId, tokenModel?.UserID ?? 0, history.Count());
            return history;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving status history for subscription {SubscriptionId} by user {UserId}", 
                subscriptionId, tokenModel?.UserID ?? 0);
            return Enumerable.Empty<SubscriptionStatusHistory>();
        }
    }

    public async Task<bool> ValidateStatusTransitionAsync(string currentStatus, string newStatus, TokenModel tokenModel = null)
    {
        try
        {
            // CONSISTENT FIX: Use single source of truth for status transitions
            var allowedTransitions = GetAllowedTransitions();
            
            if (allowedTransitions.TryGetValue(currentStatus, out var allowedStates))
            {
                if (allowedStates.Contains(newStatus))
                {
                    _logger.LogInformation("Status transition from {CurrentStatus} to {NewStatus} validated by user {UserId}", 
                        currentStatus, newStatus, tokenModel?.UserID ?? 0);
                    return true;
                }
            }

            _logger.LogWarning("Invalid status transition from {CurrentStatus} to {NewStatus} by user {UserId}", 
                currentStatus, newStatus, tokenModel?.UserID ?? 0);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating status transition from {CurrentStatus} to {NewStatus} by user {UserId}", 
                currentStatus, newStatus, tokenModel?.UserID ?? 0);
            return false;
        }
    }

    public async Task<string> GetNextValidStatusAsync(string currentStatus, TokenModel tokenModel = null)
    {
        try
        {
            // CONSISTENT FIX: Use single source of truth for status transitions
            var allowedTransitions = GetAllowedTransitions();
            
            if (allowedTransitions.TryGetValue(currentStatus, out var allowedStates))
            {
                var nextStatus = allowedStates.FirstOrDefault() ?? "No valid next status";
                _logger.LogInformation("Next valid status for {CurrentStatus} determined by user {UserId}: {NextStatus}", 
                    currentStatus, tokenModel?.UserID ?? 0, nextStatus);
                return nextStatus;
            }

            _logger.LogWarning("No valid next status found for {CurrentStatus} by user {UserId}", 
                currentStatus, tokenModel?.UserID ?? 0);
            return "No valid next status";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error determining next valid status for {CurrentStatus} by user {UserId}", 
                currentStatus, tokenModel?.UserID ?? 0);
            return "Error determining next status";
        }
    }

    /// <summary>
    /// Process subscription lifecycle state transitions
    /// </summary>
    public async Task<JsonModel> ProcessStateTransitionAsync(string subscriptionId, string newStatus, string reason = null, string changedByUserId = null, TokenModel tokenModel = null)
    {
        try
        {
            var subscription = await _subscriptionRepository.GetByIdWithDetailsAsync(Guid.Parse(subscriptionId));
            if (subscription == null)
                return new JsonModel
                {
                    data = new object(),
                    Message = "Subscription not found",
                    StatusCode = 404
                };

            var oldStatus = subscription.Status;

            // Validate state transition
            var validationResult = ValidateStateTransition(oldStatus, newStatus);
            if (!validationResult.IsValid)
                return new JsonModel
                {
                    data = new object(),
                    Message = validationResult.ErrorMessage,
                    StatusCode = 400
                };

            // BEGIN TRANSACTION - Ensure atomic state transition with status history
            await _unitOfWork.BeginTransactionAsync();
            
            try
            {
                // Update subscription status
                subscription.Status = newStatus;
                subscription.UpdatedBy = tokenModel?.UserID;
                subscription.UpdatedDate = DateTime.UtcNow;

                // Update status-specific properties
                await UpdateStatusSpecificPropertiesAsync(subscription, newStatus, reason);

                // Add status history
                await _subscriptionRepository.AddStatusHistoryAsync(new SubscriptionStatusHistory
                {
                    SubscriptionId = subscription.Id,
                    FromStatus = oldStatus,
                    ToStatus = newStatus,
                    Reason = reason,
                    ChangedByUserId = !string.IsNullOrEmpty(changedByUserId) ? int.Parse(changedByUserId) : null,
                    ChangedAt = DateTime.UtcNow,
                    // Set audit properties for creation
                    IsActive = true,
                    CreatedBy = !string.IsNullOrEmpty(changedByUserId) ? int.Parse(changedByUserId) : null,
                    CreatedDate = DateTime.UtcNow
                });

                await _subscriptionRepository.UpdateAsync(subscription);
                
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Subscription {SubscriptionId} state changed from {OldStatus} to {NewStatus}", 
                    subscriptionId, oldStatus, newStatus);

                return new JsonModel
                {
                    data = true,
                    Message = "State transition processed successfully",
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error processing state transition for {SubscriptionId} in transaction", subscriptionId);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing state transition for subscription {SubscriptionId}", subscriptionId);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to process state transition",
                StatusCode = 500
            };
        }
    }

    /// <summary>
    /// Validate if a state transition is allowed
    /// </summary>
    private StateTransitionValidation ValidateStateTransition(string currentStatus, string newStatus)
    {
        var allowedTransitions = GetAllowedTransitions();
        
        if (allowedTransitions.TryGetValue(currentStatus, out var allowedStates))
        {
            if (allowedStates.Contains(newStatus))
            {
                return new StateTransitionValidation { IsValid = true };
            }
        }

        return new StateTransitionValidation 
        { 
            IsValid = false, 
            ErrorMessage = $"Invalid state transition from {currentStatus} to {newStatus}" 
        };
    }

    /// <summary>
    /// Get allowed state transitions
    /// </summary>
    private Dictionary<string, HashSet<string>> GetAllowedTransitions()
    {
        return new Dictionary<string, HashSet<string>>
        {
            [Subscription.SubscriptionStatuses.Pending] = new HashSet<string>
            {
                Subscription.SubscriptionStatuses.Active,
                Subscription.SubscriptionStatuses.TrialActive,
                Subscription.SubscriptionStatuses.Cancelled
            },
            [Subscription.SubscriptionStatuses.Active] = new HashSet<string>
            {
                Subscription.SubscriptionStatuses.Paused,
                Subscription.SubscriptionStatuses.Cancelled,
                Subscription.SubscriptionStatuses.PaymentFailed,
                Subscription.SubscriptionStatuses.Expired
            },
            [Subscription.SubscriptionStatuses.Paused] = new HashSet<string>
            {
                Subscription.SubscriptionStatuses.Active,
                Subscription.SubscriptionStatuses.Cancelled,
                Subscription.SubscriptionStatuses.Expired
            },
            [Subscription.SubscriptionStatuses.PaymentFailed] = new HashSet<string>
            {
                Subscription.SubscriptionStatuses.Active,
                Subscription.SubscriptionStatuses.Cancelled,
                Subscription.SubscriptionStatuses.Suspended
            },
            [Subscription.SubscriptionStatuses.Suspended] = new HashSet<string>
            {
                Subscription.SubscriptionStatuses.Active,
                Subscription.SubscriptionStatuses.Cancelled
            },
            [Subscription.SubscriptionStatuses.TrialActive] = new HashSet<string>
            {
                Subscription.SubscriptionStatuses.Active,
                Subscription.SubscriptionStatuses.TrialExpired,
                Subscription.SubscriptionStatuses.Cancelled
            },
            [Subscription.SubscriptionStatuses.TrialExpired] = new HashSet<string>
            {
                Subscription.SubscriptionStatuses.Active,
                Subscription.SubscriptionStatuses.Cancelled
            },
            [Subscription.SubscriptionStatuses.Cancelled] = new HashSet<string>
            {
                // No valid transitions from Cancelled (matches Subscription entity behavior)
            },
            [Subscription.SubscriptionStatuses.Expired] = new HashSet<string>
            {
                Subscription.SubscriptionStatuses.Active // Allow reactivation
            }
        };
    }

    /// <summary>
    /// Update status-specific properties
    /// </summary>
    private async Task UpdateStatusSpecificPropertiesAsync(Subscription subscription, string newStatus, string reason)
    {
        switch (newStatus)
        {
            case Subscription.SubscriptionStatuses.Active:
                subscription.ResumedDate = DateTime.UtcNow;
                subscription.PauseReason = null;
                subscription.CancellationReason = null;
                
                // CONSISTENT FIX: Always ensure current price is set correctly during trial-to-active conversion
                if (subscription.SubscriptionPlan != null)
                {
                    var effectivePrice = BillingCalculationService.GetEffectivePlanPrice(subscription.SubscriptionPlan, null, _logger);
                    if (subscription.CurrentPrice != effectivePrice)
                    {
                        _logger.LogInformation("Updating subscription {SubscriptionId} price from ${OldPrice} to ${NewPrice} during trial conversion", 
                            subscription.Id, subscription.CurrentPrice, effectivePrice);
                        subscription.CurrentPrice = effectivePrice;
                    }
                }
                break;

            case Subscription.SubscriptionStatuses.Paused:
                subscription.PausedDate = DateTime.UtcNow;
                subscription.PauseReason = reason;
                break;

            case Subscription.SubscriptionStatuses.Cancelled:
                subscription.CancelledDate = DateTime.UtcNow;
                subscription.CancellationReason = reason;
                subscription.AutoRenew = false;
                break;

            case Subscription.SubscriptionStatuses.PaymentFailed:
                subscription.LastPaymentFailedDate = DateTime.UtcNow;
                subscription.LastPaymentError = reason;
                break;

            case Subscription.SubscriptionStatuses.Suspended:
                subscription.SuspendedDate = DateTime.UtcNow;
                break;

            case Subscription.SubscriptionStatuses.Expired:
                subscription.ExpirationDate = DateTime.UtcNow;
                break;

            case Subscription.SubscriptionStatuses.TrialExpired:
                subscription.TrialEndDate = DateTime.UtcNow;
                break;
        }
    }

    /// <summary>
    /// Process subscription expiration
    /// </summary>
    public async Task<JsonModel> ProcessSubscriptionExpirationAsync(string subscriptionId)
    {
        try
        {
            var subscription = await _subscriptionRepository.GetByIdWithDetailsAsync(Guid.Parse(subscriptionId));
            if (subscription == null)
                return new JsonModel
                {
                    data = new object(),
                    Message = "Subscription not found",
                    StatusCode = 404
                };

            if (subscription.Status == Subscription.SubscriptionStatuses.Active && 
                subscription.NextBillingDate <= DateTime.UtcNow)
            {
                return await ProcessStateTransitionAsync(
                    subscriptionId, 
                    Subscription.SubscriptionStatuses.Expired, 
                    "Subscription expired due to non-payment"
                );
            }

            return new JsonModel
            {
                data = true,
                Message = "Subscription is not due for expiration",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing subscription expiration for {SubscriptionId}", subscriptionId);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to process subscription expiration",
                StatusCode = 500
            };
        }
    }

    /// <summary>
    /// Process trial expiration with enhanced logic
    /// </summary>
    public async Task<JsonModel> ProcessTrialExpirationAsync(string subscriptionId)
    {
        try
        {
            var subscription = await _subscriptionRepository.GetByIdWithDetailsAsync(Guid.Parse(subscriptionId));
            if (subscription == null)
                return new JsonModel
                {
                    data = new object(),
                    Message = "Subscription not found",
                    StatusCode = 404
                };

            // Enhanced trial expiration logic
            if (subscription.Status == Subscription.SubscriptionStatuses.TrialActive)
            {
                // Check if trial has actually ended
                if (subscription.TrialEndDate <= DateTime.UtcNow)
                {
                    // Check if there's a valid payment method and attempt to charge
                    var hasValidPaymentMethod = await CheckPaymentMethodValidityAsync(subscription);
                    
                    if (hasValidPaymentMethod)
                    {
                        // Attempt to process first payment
                        var paymentResult = await AttemptFirstPaymentAsync(subscription);
                        
                        if (paymentResult.IsSuccessful)
                        {
                            // Convert trial to active subscription
                            return await ProcessStateTransitionAsync(
                                subscriptionId, 
                                Subscription.SubscriptionStatuses.Active, 
                                "Trial converted to active subscription via successful payment"
                            );
                        }
                        else
                        {
                            // Payment failed, expire trial
                            return await ProcessStateTransitionAsync(
                                subscriptionId, 
                                Subscription.SubscriptionStatuses.TrialExpired, 
                                $"Trial expired due to payment failure: {paymentResult.ErrorMessage}"
                            );
                        }
                    }
                    else
                    {
                        // No valid payment method, expire trial
                        return await ProcessStateTransitionAsync(
                            subscriptionId, 
                            Subscription.SubscriptionStatuses.TrialExpired, 
                            "Trial expired - no valid payment method"
                        );
                    }
                }
                else
                {
                    return new JsonModel
                    {
                        data = true,
                        Message = $"Trial is not due for expiration. Ends on {subscription.TrialEndDate:MMM dd, yyyy}",
                        StatusCode = 200
                    };
                }
            }
            else if (subscription.Status == Subscription.SubscriptionStatuses.TrialExpired)
            {
                return new JsonModel
                {
                    data = true,
                    Message = "Trial has already expired",
                    StatusCode = 200
                };
            }
            else
            {
                return new JsonModel
                {
                    data = true,
                    Message = $"Subscription is not in trial state. Current status: {subscription.Status}",
                    StatusCode = 200
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing trial expiration for {SubscriptionId}", subscriptionId);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to process trial expiration",
                StatusCode = 500
            };
        }
    }

    /// <summary>
    /// Enhanced trial management - convert trial to active subscription
    /// </summary>
    public async Task<JsonModel> ConvertTrialToActiveAsync(string subscriptionId, string paymentMethodId = null)
    {
        try
        {
            var subscription = await _subscriptionRepository.GetByIdWithDetailsAsync(Guid.Parse(subscriptionId));
            if (subscription == null)
                return new JsonModel
                {
                    data = new object(),
                    Message = "Subscription not found",
                    StatusCode = 404
                };

            if (subscription.Status != Subscription.SubscriptionStatuses.TrialActive)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = $"Cannot convert subscription from {subscription.Status} to Active. Only TrialActive subscriptions can be converted.",
                    StatusCode = 400
                };
            }

            // Validate trial hasn't expired
            if (subscription.TrialEndDate <= DateTime.UtcNow)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Cannot convert expired trial to active subscription",
                    StatusCode = 400
                };
            }

            // If payment method provided, attempt to charge
            if (!string.IsNullOrEmpty(paymentMethodId))
            {
                var paymentResult = await AttemptFirstPaymentAsync(subscription, paymentMethodId);
                
                if (paymentResult.IsSuccessful)
                {
                    // Convert to active with successful payment
                    return await ProcessStateTransitionAsync(
                        subscriptionId, 
                        Subscription.SubscriptionStatuses.Active, 
                        "Trial converted to active subscription via successful payment"
                    );
                }
                else
                {
                    return new JsonModel
                    {
                        data = new object(),
                        Message = $"Payment failed: {paymentResult.ErrorMessage}",
                        StatusCode = 400
                    };
                }
            }
            else
            {
                // Convert to active without immediate payment (user will be charged later)
                return await ProcessStateTransitionAsync(
                    subscriptionId, 
                    Subscription.SubscriptionStatuses.Active, 
                    "Trial converted to active subscription"
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting trial to active for {SubscriptionId}", subscriptionId);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to convert trial to active subscription",
                StatusCode = 500
            };
        }
    }

    /// <summary>
    /// Extend trial period for a subscription
    /// </summary>
    public async Task<JsonModel> ExtendTrialAsync(string subscriptionId, int additionalDays, string reason = null)
    {
        try
        {
            var subscription = await _subscriptionRepository.GetByIdWithDetailsAsync(Guid.Parse(subscriptionId));
            if (subscription == null)
                return new JsonModel
                {
                    data = new object(),
                    Message = "Subscription not found",
                    StatusCode = 404
                };

            if (subscription.Status != Subscription.SubscriptionStatuses.TrialActive)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = $"Cannot extend trial for subscription in {subscription.Status} state",
                    StatusCode = 400
                };
            }

            // BEGIN TRANSACTION - Ensure atomic update of trial end date and status history
            await _unitOfWork.BeginTransactionAsync();
            
            try
            {
                // Calculate new trial end date
                var newTrialEndDate = subscription.TrialEndDate?.AddDays(additionalDays) ?? DateTime.UtcNow.AddDays(additionalDays);
                
                // Update trial end date
                subscription.TrialEndDate = newTrialEndDate;
                subscription.UpdatedBy = 0; // 0 for system actions
                subscription.UpdatedDate = DateTime.UtcNow;

                await _subscriptionRepository.UpdateAsync(subscription);

                // SRP Refactoring: Use centralized status history helper method
                await RecordStatusChangeAsync(subscription.Id, subscription.Status, subscription.Status, 
                    $"Trial extended by {additionalDays} days. {reason}", null);
                
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Trial extended for subscription {SubscriptionId} by {AdditionalDays} days", 
                    subscriptionId, additionalDays);

                return new JsonModel
                {
                    data = new { NewTrialEndDate = newTrialEndDate },
                    Message = $"Trial extended by {additionalDays} days. New end date: {newTrialEndDate:MMM dd, yyyy}",
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error extending trial for subscription {SubscriptionId} in transaction", subscriptionId);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extending trial for {SubscriptionId}", subscriptionId);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to extend trial",
                StatusCode = 500
            };
        }
    }

    /// <summary>
    /// Check if subscription has a valid payment method
    /// </summary>
    private async Task<bool> CheckPaymentMethodValidityAsync(Subscription subscription)
    {
        try
        {
            // This would typically call your payment service to validate payment methods
            // For now, we'll assume true if the subscription has a Stripe customer ID
            return !string.IsNullOrEmpty(subscription.StripeCustomerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking payment method validity for subscription {SubscriptionId}", subscription.Id);
            return false;
        }
    }

    /// <summary>
    /// Attempt to process first payment for trial conversion
    /// </summary>
    private async Task<PaymentAttemptResult> AttemptFirstPaymentAsync(Subscription subscription, string paymentMethodId = null)
    {
        try
        {
            // This would typically call your payment service to process the payment
            // For now, we'll return a mock successful result
            _logger.LogInformation("Attempting first payment for trial subscription {SubscriptionId}", subscription.Id);
            
            // Simulate payment processing
            await Task.Delay(100); // Simulate processing time
            
            return new PaymentAttemptResult
            {
                IsSuccessful = true,
                TransactionId = $"txn_{Guid.NewGuid():N}",
                Amount = BillingCalculationService.GetEffectivePlanPrice(subscription.SubscriptionPlan, null, _logger),
                Currency = subscription.SubscriptionPlan?.Currency?.Code ?? "USD"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error attempting first payment for subscription {SubscriptionId}", subscription.Id);
            return new PaymentAttemptResult
            {
                IsSuccessful = false,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Reactivate a cancelled or expired subscription
    /// </summary>

    /// <summary>
    /// Calculate next billing date based on billing cycle
    /// </summary>
    // SRP Refactoring: Removed duplicate CalculateNextBillingDate - now uses BillingService.CalculateNextBillingDate()
    private DateTime CalculateNextBillingDate(Subscription subscription)
    {
        return _billingService.CalculateNextBillingDate(DateTime.UtcNow, subscription.BillingCycle);
    }

    /// <summary>
    /// Get subscription lifecycle status
    /// </summary>
    public async Task<JsonModel> GetSubscriptionLifecycleStatusAsync(string subscriptionId, TokenModel tokenModel = null)
    {
        try
        {
            var subscription = await _subscriptionRepository.GetByIdWithDetailsAsync(Guid.Parse(subscriptionId));
            if (subscription == null)
                return new JsonModel
                {
                    data = new object(),
                    Message = "Subscription not found",
                    StatusCode = 404
                };

            var status = new SubscriptionLifecycleStatus
            {
                SubscriptionId = subscriptionId,
                CurrentStatus = subscription.Status,
                DaysUntilNextBilling = (subscription.NextBillingDate - DateTime.UtcNow).Days,
                IsActive = subscription.Status == Subscription.SubscriptionStatuses.Active,
                IsInTrial = subscription.Status == Subscription.SubscriptionStatuses.TrialActive,
                IsExpired = subscription.Status == Subscription.SubscriptionStatuses.Expired,
                IsCancelled = subscription.Status == Subscription.SubscriptionStatuses.Cancelled,
                IsPaused = subscription.Status == Subscription.SubscriptionStatuses.Paused,
                IsPaymentFailed = subscription.Status == Subscription.SubscriptionStatuses.PaymentFailed,
                CanBeReactivated = subscription.Status == Subscription.SubscriptionStatuses.Cancelled || 
                                  subscription.Status == Subscription.SubscriptionStatuses.Expired,
                CanBePaused = subscription.Status == Subscription.SubscriptionStatuses.Active,
                CanBeCancelled = subscription.Status == Subscription.SubscriptionStatuses.Active || 
                                subscription.Status == Subscription.SubscriptionStatuses.Paused
            };

            return new JsonModel
            {
                data = status,
                Message = "Subscription lifecycle status retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting lifecycle status for subscription {SubscriptionId}", subscriptionId);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to get lifecycle status",
                StatusCode = 500
            };
        }
    }

    /// <summary>
    /// Process bulk state transitions
    /// </summary>
    public async Task<JsonModel> ProcessBulkStateTransitionsAsync(
        IEnumerable<string> subscriptionIds, string newStatus, string reason = null, string changedByUserId = null, TokenModel tokenModel = null)
    {
        var result = new BulkStateTransitionResult
        {
            ProcessedAt = DateTime.UtcNow,
            TotalSubscriptions = 0,
            SuccessfulTransitions = 0,
            FailedTransitions = 0,
            Errors = new List<string>()
        };

        foreach (var subscriptionId in subscriptionIds)
        {
            result.TotalSubscriptions++;
            try
            {
                var transitionResult = await ProcessStateTransitionAsync(subscriptionId, newStatus, reason, changedByUserId);
                if (transitionResult.StatusCode == 200)
                {
                    result.SuccessfulTransitions++;
                }
                else
                {
                    result.FailedTransitions++;
                    result.Errors.Add($"Subscription {subscriptionId}: {transitionResult.Message}");
                }
            }
            catch (Exception ex)
            {
                result.FailedTransitions++;
                result.Errors.Add($"Subscription {subscriptionId}: {ex.Message}");
            }
        }

        return new JsonModel
        {
            data = result,
            Message = "Bulk state transitions processed successfully",
            StatusCode = 200
        };
    }

    public async Task<bool> ProcessSubscriptionExpirationAsync(Guid subscriptionId, TokenModel tokenModel = null)
    {
        try
        {
            _logger.LogInformation("Processing subscription expiration for {SubscriptionId} by user {UserId}", 
                subscriptionId, tokenModel?.UserID ?? 0);
            
            var subscription = await _subscriptionRepository.GetByIdWithDetailsAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found for expiration processing by user {UserId}", 
                    subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            // Check if subscription has expired
            if (subscription.ExpiryDate.HasValue && subscription.ExpiryDate.Value < DateTime.UtcNow)
            {
                var result = await ExpireSubscriptionAsync(subscriptionId, "Subscription expired automatically", tokenModel);
                
                _logger.LogInformation("Subscription expiration processed for {SubscriptionId} by user {UserId}: {Result}", 
                    subscriptionId, tokenModel?.UserID ?? 0, result);
                return result;
            }

            _logger.LogInformation("Subscription {SubscriptionId} has not expired yet, no processing needed by user {UserId}", 
                subscriptionId, tokenModel?.UserID ?? 0);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing subscription expiration for {SubscriptionId} by user {UserId}", 
                subscriptionId, tokenModel?.UserID ?? 0);
            return false;
        }
    }

    public async Task<bool> ProcessSubscriptionSuspensionAsync(Guid subscriptionId, string reason, TokenModel tokenModel = null)
    {
        try
        {
            _logger.LogInformation("Processing subscription suspension for {SubscriptionId} by user {UserId}", 
                subscriptionId, tokenModel?.UserID ?? 0);
            
            var subscription = await _subscriptionRepository.GetByIdWithDetailsAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found for suspension processing by user {UserId}", 
                    subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            // Check if subscription should be suspended (e.g., payment issues, policy violations)
            var shouldSuspend = await DetermineIfShouldSuspendAsync(subscription, reason);
            if (shouldSuspend)
            {
                var result = await SuspendSubscriptionAsync(subscriptionId, reason, tokenModel);
                
                _logger.LogInformation("Subscription suspension processed for {SubscriptionId} by user {UserId}: {Result}", 
                    subscriptionId, tokenModel?.UserID ?? 0, result);
                return result;
            }

            _logger.LogInformation("Subscription {SubscriptionId} does not need suspension, no processing needed by user {UserId}", 
                subscriptionId, tokenModel?.UserID ?? 0);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing subscription suspension for {SubscriptionId} by user {UserId}", 
                subscriptionId, tokenModel?.UserID ?? 0);
            return false;
        }
    }

    // Helper method to determine if subscription should be suspended
    private async Task<bool> DetermineIfShouldSuspendAsync(Subscription subscription, string reason)
    {
        // Implement business logic to determine if suspension is needed
        // This could include checking payment history, policy violations, etc.
        return reason?.Contains("payment") == true || reason?.Contains("violation") == true;
    }

    #region Helper Methods

    /// <summary>
    /// Checks if a user has access to a specific subscription
    /// </summary>
    private async Task<bool> HasAccessToSubscription(int userId, string subscriptionId)
    {
        try
        {
            var subscription = await _subscriptionRepository.GetByIdWithDetailsAsync(Guid.Parse(subscriptionId));
            return subscription != null && subscription.UserId == userId;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// SRP Refactoring: Centralized helper method to record subscription status changes.
    /// This eliminates the 20+ duplicate status history creation blocks throughout this service.
    /// </summary>
    /// <param name="subscriptionId">The subscription ID</param>
    /// <param name="fromStatus">The previous status</param>
    /// <param name="toStatus">The new status</param>
    /// <param name="reason">Reason for the status change</param>
    /// <param name="tokenModel">Token for audit trail</param>
    private async Task RecordStatusChangeAsync(
        Guid subscriptionId,
        string fromStatus,
        string toStatus,
        string? reason,
        TokenModel tokenModel)
    {
        var historyEntry = new SubscriptionStatusHistory
        {
            SubscriptionId = subscriptionId,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            Reason = reason ?? $"Status changed to {toStatus}",
            ChangedAt = DateTime.UtcNow,
            ChangedByUserId = tokenModel?.UserID,
            IsActive = true,
            CreatedBy = tokenModel?.UserID,
            CreatedDate = DateTime.UtcNow
        };
        
        await _statusHistoryRepository.CreateAsync(historyEntry);
        
        _logger.LogDebug(
            "Status history recorded for subscription {SubscriptionId}: {FromStatus} → {ToStatus}",
            subscriptionId, fromStatus, toStatus
        );
    }

    /// <summary>
    /// Validates if a bulk action is appropriate for a subscription's current status
    /// </summary>
    private async Task<bool> ValidateBulkActionAsync(string currentStatus, string action)
    {
        try
        {
            var validActions = new Dictionary<string, List<string>>
            {
                [Subscription.SubscriptionStatuses.Pending] = new List<string> { "cancel" },
                [Subscription.SubscriptionStatuses.Active] = new List<string> { "cancel", "pause", "extend" },
                [Subscription.SubscriptionStatuses.Paused] = new List<string> { "cancel", "resume", "extend" },
                [Subscription.SubscriptionStatuses.Suspended] = new List<string> { "cancel", "resume", "extend" },
                [Subscription.SubscriptionStatuses.PaymentFailed] = new List<string> { "cancel", "extend" },
                [Subscription.SubscriptionStatuses.Expired] = new List<string> { "cancel" },
                [Subscription.SubscriptionStatuses.Cancelled] = new List<string> { }, // No actions allowed on cancelled subscriptions
                [Subscription.SubscriptionStatuses.TrialActive] = new List<string> { "cancel", "extend" }
            };

            if (validActions.ContainsKey(currentStatus))
            {
                return validActions[currentStatus].Contains(action);
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating bulk action {Action} for status {CurrentStatus}", action, currentStatus);
            return false;
        }
    }

    /// <summary>
    /// Ensures a Stripe customer exists for the user, creating one if necessary
    /// </summary>
    // SRP Refactoring: Removed duplicate EnsureStripeCustomerAsync - now using centralized method in StripeService
    private async Task<string> EnsureStripeCustomerAsync(UserDto user, TokenModel tokenModel)
    {
        return await _stripeService.EnsureStripeCustomerAsync(
            user.Id,
            user.Email,
            user.FullName,
            user.StripeCustomerId,
            tokenModel
        );
    }

    /// <summary>
    /// Gets the Stripe price ID for the plan
    /// NEW ARCHITECTURE: Each plan has ONE billing cycle, therefore ONE Stripe price
    /// </summary>
    private string GetStripePriceIdForPlan(SubscriptionPlan plan)
    {
        if (string.IsNullOrEmpty(plan.StripePriceId))
        {
            throw new Exception($"No Stripe price ID configured for plan {plan.Name}");
        }
        return plan.StripePriceId;
    }

    /// <summary>
    /// Gets or creates a Stripe price ID for the plan's effective price (considering discounts).
    /// If the plan has a valid discount, creates a new Stripe price with the discounted amount.
    /// Otherwise, uses the plan's existing Stripe price ID.
    /// </summary>
    /// <param name="plan">The subscription plan</param>
    /// <param name="tokenModel">Token containing user authentication information</param>
    /// <returns>The Stripe price ID to use for the subscription</returns>
    private async Task<string> GetOrCreateStripePriceForPlan(SubscriptionPlan plan, TokenModel tokenModel)
    {
        try
        {
            var effectivePrice = BillingCalculationService.GetEffectivePlanPrice(plan, null, _logger);
            
            // If using base price, use existing Stripe price ID
            if (effectivePrice == plan.BasePrice)
            {
                return GetStripePriceIdForPlan(plan);
            }
            
            // If using discounted price, create a new Stripe price
            _logger.LogInformation("Creating new Stripe price for discounted plan {PlanName}: Base=${BasePrice}, Discounted=${DiscountedPrice}",
                plan.Name, plan.BasePrice, effectivePrice);
            
            // Get billing cycle interval for Stripe
            var (interval, intervalCount) = GetStripeIntervalForBillingCycle(plan.BillingCycle);
            
            // CRITICAL FIX: Use centralized currency handling
            var currencyLogger = _serviceProvider.GetRequiredService<ILogger<CurrencyService>>();
            var currencyService = new CurrencyService(_subscriptionRepository, currencyLogger);
            var currencyCode = await currencyService.GetCurrencyCodeAsync(plan.CurrencyId);
            
            // Create new Stripe price with discounted amount
            var discountedStripePriceId = await _stripeService.CreatePriceAsync(
                plan.StripeProductId,
                effectivePrice,
                currencyCode,
                interval,
                intervalCount,
                tokenModel);
            
            _logger.LogInformation("Created discounted Stripe price {PriceId} for plan {PlanName} with amount ${Amount}",
                discountedStripePriceId, plan.Name, effectivePrice);
            
            return discountedStripePriceId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting or creating Stripe price for plan {PlanName}, falling back to base price", plan.Name);
            return GetStripePriceIdForPlan(plan);
        }
    }

    /// <summary>
    /// Gets the Stripe interval and interval count for a billing cycle
    /// </summary>
    /// <param name="billingCycle">The billing cycle</param>
    /// <returns>Tuple of (interval, intervalCount)</returns>
    private (string interval, int intervalCount) GetStripeIntervalForBillingCycle(MasterBillingCycle billingCycle)
    {
        return billingCycle.Name.ToLower() switch
        {
            "monthly" => ("month", 1),
            "quarterly" => ("month", 3),
            "semi-annual" => ("month", 6),
            "annual" => ("year", 1),
            "weekly" => ("week", 1),
            "daily" => ("day", 1),
            _ => ("month", 1)
        };
    }

    /// <summary>
    /// Calculates the next billing date based on billing cycle ID
    /// </summary>
    // SRP Refactoring: Removed duplicate CalculateNextBillingDateAsync - now uses BillingService.CalculateNextBillingDate()
    private async Task<DateTime> CalculateNextBillingDateAsync(DateTime startDate, Guid billingCycleId)
    {
        try
        {
            // Get the billing cycle from the database
            var billingCycle = await _subscriptionRepository.GetBillingCycleByIdAsync(billingCycleId);
            
            // Use centralized calculation from BillingService
            return _billingService.CalculateNextBillingDate(startDate, billingCycle);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating next billing date for billing cycle {BillingCycleId}", billingCycleId);
            // CONSISTENT FIX: Use centralized billing cycle calculator instead of manual AddMonths
            return BillingCycleCalculator.CalculateNextBillingDate(startDate, 
                new MasterBillingCycle { Name = "monthly", DurationInDays = 30 }); // 30 days = 1 month
        }
    }

    // SRP Refactoring: Removed duplicate CalculateEndDateAsync - now uses BillingService.CalculateNextBillingDate()
    private async Task<DateTime> CalculateEndDateAsync(DateTime startDate, Guid billingCycleId)
    {
        try
        {
            // Get the billing cycle from the database
            var billingCycle = await _subscriptionRepository.GetBillingCycleByIdAsync(billingCycleId);
            
            // Use centralized calculation from BillingService
            return _billingService.CalculateNextBillingDate(startDate, billingCycle);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating end date for billing cycle {BillingCycleId}", billingCycleId);
            // CONSISTENT FIX: Use centralized billing cycle calculator instead of manual AddMonths
            return BillingCycleCalculator.CalculateNextBillingDate(startDate, 
                new MasterBillingCycle { Name = "monthly", DurationInDays = 30 }); // 30 days = 1 month
        }
    }

    /// <summary>
    /// Creates an initial billing record for a newly created subscription
    /// </summary>
    /// <param name="subscription">The subscription entity</param>
    /// <param name="plan">The subscription plan</param>
    /// <param name="tokenModel">Token containing user authentication information</param>
    /// <returns>Task representing the asynchronous operation</returns>
    private async Task CreateInitialBillingRecordAsync(Subscription subscription, SubscriptionPlan plan, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Creating initial billing record for subscription {SubscriptionId}", subscription.Id);

            // SRP Refactoring: Use centralized billing record factory method
            var billingResult = await _billingService.CreateSubscriptionBillingAsync(
                subscription,
                BillingCalculationService.GetEffectivePlanPrice(plan, null, _logger),
                $"Initial billing for {plan.Name} subscription",
                subscription.NextBillingDate,
                tokenModel
            );
            
            if (billingResult.StatusCode == 200)
            {
                _logger.LogInformation("Successfully created initial billing record for subscription {SubscriptionId}", subscription.Id);
            }
            else
            {
                _logger.LogWarning("Failed to create initial billing record for subscription {SubscriptionId}: {Error}", 
                    subscription.Id, billingResult.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating initial billing record for subscription {SubscriptionId}", subscription.Id);
            // Don't throw - this is not critical for subscription creation
        }
    }

    /// <summary>
    /// Allocates initial privileges for a new subscription.
    /// NEW ARCHITECTURE: Creates UserSubscriptionPrivilegeUsage records based on plan's explicit privilege values.
    /// Each plan (Monthly, Quarterly, Annual) has its own explicit privilege limits.
    /// </summary>
    /// <param name="subscription">The newly created subscription</param>
    /// <param name="plan">The subscription plan with privileges</param>
    /// <param name="tokenModel">Token containing user authentication information</param>
    /// <returns>Task representing the asynchronous operation</returns>
    private async Task AllocateInitialPrivilegesAsync(Subscription subscription, SubscriptionPlan plan, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Allocating initial privileges for subscription {SubscriptionId} from plan {PlanName}", 
                subscription.Id, plan.Name);
            
            if (plan.PlanPrivileges == null || !plan.PlanPrivileges.Any())
            {
                _logger.LogWarning("No privileges found for plan {PlanId}. Skipping privilege allocation.", plan.Id);
                return;
            }
            
            int allocatedCount = 0;
            
            // For each plan privilege, create initial usage record
            foreach (var planPrivilege in plan.PlanPrivileges)
            {
                try
                {
                    // Use PrivilegeAllocationCalculator for consistent allocation logic
                    var (allowedValue, periodStart, periodEnd) = 
                        PrivilegeAllocationCalculator.CalculatePrivilegeAllocation(subscription, planPrivilege);
                    
                    var usage = new UserSubscriptionPrivilegeUsage
                    {
                        Id = Guid.NewGuid(),
                        SubscriptionId = subscription.Id,
                        SubscriptionPlanPrivilegeId = planPrivilege.Id,
                        PrivilegeId = planPrivilege.PrivilegeId,
                        UsedValue = 0,
                        AllowedValue = allowedValue,  // Explicit from plan (e.g., 10/month, 150/year)
                        UsagePeriodStart = periodStart,
                        UsagePeriodEnd = periodEnd,
                        LastUsedAt = null,
                        ResetAt = null,
                        CreatedBy = tokenModel.UserID,
                        CreatedDate = DateTime.UtcNow,
                        IsActive = true,
                        IsDeleted = false
                    };
                    
                    await _usageRepo.CreateUsageAsync(usage);
                    allocatedCount++;
                    
                    _logger.LogDebug("Allocated privilege {PrivilegeName}: AllowedValue={AllowedValue}, Period={Start} to {End}", 
                        planPrivilege.Privilege?.Name ?? "Unknown",
                        allowedValue,
                        periodStart,
                        periodEnd);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error allocating privilege {PrivilegeId} for subscription {SubscriptionId}", 
                        planPrivilege.PrivilegeId, subscription.Id);
                    // Continue with other privileges
                }
            }
            
            _logger.LogInformation("Successfully allocated {Count} privileges for subscription {SubscriptionId}", 
                allocatedCount, subscription.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error allocating initial privileges for subscription {SubscriptionId}", subscription.Id);
            // Don't throw - this can be handled later via admin tool or webhook
        }
    }

    /// <summary>
    /// Processes any pending refunds for a cancelled subscription
    /// </summary>
    /// <param name="subscription">The cancelled subscription</param>
    /// <param name="tokenModel">Token containing user authentication information</param>
    /// <returns>Task representing the asynchronous operation</returns>
    private async Task ProcessCancellationRefundsAsync(Subscription subscription, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Processing cancellation refunds for subscription {SubscriptionId}", subscription.Id);

            // Get pending billing records for this subscription
            var billingHistoryResult = await _billingService.GetSubscriptionBillingHistoryAsync(subscription.Id, tokenModel);
            
            if (billingHistoryResult.StatusCode == 200 && billingHistoryResult.data != null)
            {
                var billingRecords = (IEnumerable<BillingRecord>)billingHistoryResult.data;
                var pendingRecords = billingRecords.Where(b => b.Status == BillingRecord.BillingStatus.Pending).ToList();

                foreach (var billingRecord in pendingRecords)
                {
                    try
                    {
                        // Process refund for pending billing record
                        var refundResult = await _billingService.ProcessRefundAsync(billingRecord.Id, billingRecord.TotalAmount, tokenModel);
                        
                        if (refundResult.StatusCode == 200)
                        {
                            _logger.LogInformation("Successfully processed refund for billing record {BillingRecordId} during subscription cancellation", 
                                billingRecord.Id);
                        }
                        else
                        {
                            _logger.LogWarning("Failed to process refund for billing record {BillingRecordId} during subscription cancellation: {Error}", 
                                billingRecord.Id, refundResult.Message);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing refund for billing record {BillingRecordId} during subscription cancellation", 
                            billingRecord.Id);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing cancellation refunds for subscription {SubscriptionId}", subscription.Id);
            // Don't throw - this is not critical for subscription cancellation
        }
    }


    #endregion

    /// <summary>
    /// Updates a subscription with the provided details
    /// </summary>
    /// <param name="subscriptionId">The subscription ID to update</param>
    /// <param name="updateDto">The update details</param>
    /// <param name="tokenModel">Token containing user authentication information</param>
    /// <returns>JsonModel containing the update result</returns>
    public async Task<JsonModel> UpdateSubscriptionAsync(string subscriptionId, UpdateSubscriptionDto updateDto, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Updating subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel.UserID);

            if (!Guid.TryParse(subscriptionId, out var subscriptionGuid))
            {
                return new JsonModel { data = new object(), Message = "Invalid subscription ID format", StatusCode = 400 };
            }

            var existingSubscription = await _subscriptionRepository.GetByIdWithDetailsAsync(subscriptionGuid);
            if (existingSubscription == null)
            {
                return new JsonModel { data = new object(), Message = "Subscription not found", StatusCode = 404 };
            }

            // Update subscription properties
            if (updateDto.AutoRenew.HasValue)
                existingSubscription.AutoRenew = updateDto.AutoRenew.Value;
            existingSubscription.UpdatedBy = tokenModel.UserID;
            existingSubscription.UpdatedDate = DateTime.UtcNow;

            // Update in database
            await _subscriptionRepository.UpdateAsync(existingSubscription);
            await _unitOfWork.SaveChangesAsync();

            var updatedSubscriptionDto = _mapper.Map<SubscriptionDto>(existingSubscription);
            return new JsonModel { data = updatedSubscriptionDto, Message = "Subscription updated successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating subscription {SubscriptionId}", subscriptionId);
            return new JsonModel { data = new object(), Message = "Error updating subscription", StatusCode = 500 };
        }
    }
}

public class StateTransitionValidation
{
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}

public class SubscriptionLifecycleStatus
{
    public string SubscriptionId { get; set; } = string.Empty;
    public string CurrentStatus { get; set; } = string.Empty;
    public int DaysUntilNextBilling { get; set; }
    public bool IsActive { get; set; }
    public bool IsInTrial { get; set; }
    public bool IsExpired { get; set; }
    public bool IsCancelled { get; set; }
    public bool IsPaused { get; set; }
    public bool IsPaymentFailed { get; set; }
    public bool CanBeReactivated { get; set; }
    public bool CanBePaused { get; set; }
    public bool CanBeCancelled { get; set; }
}

public class BulkStateTransitionResult
{
    public DateTime ProcessedAt { get; set; }
    public int TotalSubscriptions { get; set; }
    public int SuccessfulTransitions { get; set; }
    public int FailedTransitions { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class PaymentAttemptResult
{
    public bool IsSuccessful { get; set; }
    public string TransactionId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public string ErrorMessage { get; set; }
}
