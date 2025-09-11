using AutoMapper;
using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
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
    
    /// <summary>
    /// Subscription status constants to avoid hard-coded strings
    /// </summary>
    public static class SubscriptionStatus
    {
        public const string Pending = "Pending";
        public const string Active = "Active";
        public const string Paused = "Paused";
        public const string Suspended = "Suspended";
        public const string Cancelled = "Cancelled";
        public const string Expired = "Expired";
        public const string PaymentFailed = "PaymentFailed";
        public const string TrialActive = "TrialActive";
    }
    
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
    private readonly IBillingService _billingService;
    private readonly ISubscriptionNotificationService _subscriptionNotificationService;
    private readonly IPrivilegeRepository _privilegeRepository;
    private readonly IUnitOfWork _unitOfWork;

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
        IBillingService billingService,
        ISubscriptionNotificationService subscriptionNotificationService,
        IPrivilegeRepository privilegeRepository,
        IUnitOfWork unitOfWork)
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
            var plan = await _subscriptionRepository.GetSubscriptionPlanByIdAsync(Guid.Parse(createDto.PlanId));
            if (plan == null)
                return new JsonModel { data = new object(), Message = "Subscription plan does not exist", StatusCode = 404 };
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

            // Step 6: Create Stripe Subscription with proper billing cycle logic
            string stripeSubscriptionId;
            // Get the appropriate Stripe price ID based on the selected billing cycle
            string stripePriceId = await GetStripePriceIdForBillingCycleAsync(plan, createDto.BillingCycleId);
            
            try
            {
                _logger.LogInformation("Creating Stripe subscription for user {UserId} with billing cycle ID {BillingCycleId} using price ID {StripePriceId}", 
                    createDto.UserId, createDto.BillingCycleId, stripePriceId);
                
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

            // 7. Create local subscription entity with Stripe IDs
            var entity = _mapper.Map<Subscription>(createDto);
            
            // NEW: Set Stripe integration fields
            entity.StripeCustomerId = stripeCustomerId;
            entity.StripeSubscriptionId = stripeSubscriptionId;
            entity.StripePriceId = stripePriceId;
            entity.PaymentMethodId = createDto.PaymentMethodId;
            
            // Set the current price from the plan
            entity.CurrentPrice = plan.Price;
            
            // Trial logic
            if (plan.IsTrialAllowed && plan.TrialDurationInDays > 0)
            {
                entity.IsTrialSubscription = true;
                entity.TrialStartDate = DateTime.UtcNow;
                entity.TrialEndDate = DateTime.UtcNow.AddDays(plan.TrialDurationInDays);
                entity.TrialDurationInDays = plan.TrialDurationInDays;
                entity.Status = SubscriptionStatus.TrialActive;
            }
            else
            {
                entity.Status = SubscriptionStatus.Active;
            }
            
            entity.StartDate = DateTime.UtcNow;
            entity.NextBillingDate = await CalculateNextBillingDateAsync(DateTime.UtcNow, createDto.BillingCycleId);
            
            // Set EndDate based on billing cycle
            entity.EndDate = await CalculateEndDateAsync(DateTime.UtcNow, createDto.BillingCycleId);
            
            // Set audit properties for creation
            entity.IsActive = true;
            entity.CreatedBy = tokenModel.UserID;
            entity.CreatedDate = DateTime.UtcNow;
            
            // BEGIN TRANSACTION - Ensure subscription and status history are created atomically
            await _unitOfWork.BeginTransactionAsync();
            
            Subscription created;
            try
            {
                created = await _subscriptionRepository.CreateAsync(entity);
                
                // Add status history
                await _subscriptionRepository.AddStatusHistoryAsync(new SubscriptionStatusHistory {
                    SubscriptionId = created.Id,
                    FromStatus = null,
                    ToStatus = created.Status,
                    ChangedAt = DateTime.UtcNow,
                    ChangedByUserId = tokenModel.UserID,
                    // Set audit properties for creation
                    IsActive = true,
                    CreatedBy = tokenModel.UserID,
                    CreatedDate = DateTime.UtcNow
                });
                
                // COMMIT TRANSACTION
                await _unitOfWork.CommitTransactionAsync();
                
                _logger.LogInformation("Successfully created subscription {SubscriptionId} with status history in transaction", created.Id);
            }
            catch (Exception ex)
            {
                // ROLLBACK TRANSACTION on any error
                await _unitOfWork.RollbackTransactionAsync();
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
            if (tokenModel.RoleID != 1 && !await HasAccessToSubscription(tokenModel.UserID, subscriptionId))
            {
                return new JsonModel { data = new object(), Message = "Access denied", StatusCode = 403 };
            }

            // Retrieve subscription entity from repository
            var entity = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
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
                
                // Add status history
                await _subscriptionRepository.AddStatusHistoryAsync(new SubscriptionStatusHistory {
                    SubscriptionId = updated.Id,
                    FromStatus = oldStatus,
                    ToStatus = updated.Status,
                    Reason = reason,
                    ChangedAt = DateTime.UtcNow
                });
                
                // COMMIT TRANSACTION
                await _unitOfWork.CommitTransactionAsync();
                
                _logger.LogInformation("Successfully cancelled subscription {SubscriptionId} with status history in transaction", updated.Id);
            }
            catch (Exception ex)
            {
                // ROLLBACK TRANSACTION on any error
                await _unitOfWork.RollbackTransactionAsync();
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
            if (tokenModel.RoleID != 1 && !await HasAccessToSubscription(tokenModel.UserID, subscriptionId))
            {
                return new JsonModel { data = new object(), Message = "Access denied", StatusCode = 403 };
            }

            // Retrieve subscription entity from repository
            var entity = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
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
                
                // Add status history
                await _subscriptionRepository.AddStatusHistoryAsync(new SubscriptionStatusHistory {
                    SubscriptionId = updated.Id,
                    FromStatus = oldStatus,
                    ToStatus = updated.Status,
                    ChangedAt = DateTime.UtcNow
                });
                
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
            if (tokenModel.RoleID != 1 && !await HasAccessToSubscription(tokenModel.UserID, subscriptionId))
            {
                return new JsonModel { data = new object(), Message = "Access denied", StatusCode = 403 };
            }

            // Retrieve subscription entity from repository
            var entity = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
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
                
                // Add status history
                await _subscriptionRepository.AddStatusHistoryAsync(new SubscriptionStatusHistory {
                    SubscriptionId = updated.Id,
                    FromStatus = oldStatus,
                    ToStatus = updated.Status,
                    ChangedAt = DateTime.UtcNow
                });
                
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
                        // Determine which Stripe price ID to use based on billing cycle
                        string stripePriceId = currentPlan.StripeMonthlyPriceId; // Default to monthly
                        
                        // You can add logic here to determine the correct price ID based on billing cycle
                        // For now, using monthly as default
                        
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
            
            // Update local subscription
            entity.Status = Subscription.SubscriptionStatuses.Active;
            entity.UpdatedBy = tokenModel.UserID;
            entity.UpdatedDate = DateTime.UtcNow;
            
            var updated = await _subscriptionRepository.UpdateAsync(entity);
            
            // Add status history
            await _subscriptionRepository.AddStatusHistoryAsync(new SubscriptionStatusHistory {
                SubscriptionId = updated.Id,
                FromStatus = oldStatus,
                ToStatus = updated.Status,
                ChangedAt = DateTime.UtcNow
            });
            
            // Send reactivation notification
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
            if (tokenModel.RoleID != 1 && !await HasAccessToSubscription(tokenModel.UserID, subscriptionId))
            {
                return new JsonModel { data = new object(), Message = "Access denied", StatusCode = 403 };
            }

            // Retrieve subscription entity from repository
            var entity = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
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
                    // Determine which Stripe price ID to use based on billing cycle
                    string newStripePriceId = newPlan.StripeMonthlyPriceId; // Default to monthly
                    
                    // You can add logic here to determine the correct price ID based on billing cycle
                    // For now, using monthly as default
                    
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
            
            // Update local subscription
            entity.SubscriptionPlanId = Guid.Parse(newPlanId);
            entity.UpdatedBy = tokenModel.UserID;
            entity.UpdatedDate = DateTime.UtcNow;
            
            var updated = await _subscriptionRepository.UpdateAsync(entity);
            
            return new JsonModel { data = _mapper.Map<SubscriptionDto>(updated), Message = "Subscription upgraded successfully with Stripe synchronization", StatusCode = 200 };
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
    public async Task<JsonModel> UpdateSubscriptionAsync(string subscriptionId, UpdateSubscriptionDto updateDto, TokenModel tokenModel)
    {
        try
        {
            // Validate token permissions - ensure user has access to this subscription
            if (tokenModel.RoleID != 1 && !await HasAccessToSubscription(tokenModel.UserID, subscriptionId))
            {
                return new JsonModel { data = new object(), Message = "Access denied", StatusCode = 403 };
            }

            // Retrieve subscription entity from repository
            var subscription = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (subscription == null)
                return new JsonModel { data = new object(), Message = "Subscription not found", StatusCode = 404 };

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
            
            return new JsonModel { data = _mapper.Map<SubscriptionDto>(updatedSubscription), Message = "Subscription updated successfully", StatusCode = 200 };
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
                    await _notificationService.SendSubscriptionCancelledNotificationAsync(((UserDto)userResult.data).Email, ((UserDto)userResult.data).FullName, _mapper.Map<SubscriptionDto>(sub), tokenModel);
                    _logger.LogInformation("Subscription cancellation email sent to {Email}", ((UserDto)userResult.data).Email);
                }
                cancelled++;
            }
        }
        return new JsonModel { data = cancelled, Message = $"{cancelled} subscriptions cancelled.", StatusCode = 200 };
    }

    /// <summary>
    /// Bulk upgrade subscriptions (admin action)
    /// </summary>
    public async Task<JsonModel> BulkUpgradeSubscriptionsAsync(IEnumerable<string> subscriptionIds, string newPlanId, string adminUserId, TokenModel tokenModel)
    {
        int upgraded = 0;
        foreach (var id in subscriptionIds)
        {
            var sub = await _subscriptionRepository.GetByIdAsync(Guid.Parse(id));
            if (sub != null && sub.Status == Subscription.SubscriptionStatuses.Active && sub.SubscriptionPlanId != Guid.Parse(newPlanId))
            {
                sub.SubscriptionPlanId = Guid.Parse(newPlanId);
                sub.UpdatedBy = tokenModel.UserID;
                sub.UpdatedDate = DateTime.UtcNow;
                await _subscriptionRepository.UpdateAsync(sub);
                var userResult = await _userService.GetUserByIdAsync(sub.UserId, tokenModel);
                if (userResult.StatusCode == 200 && userResult.data != null)
                {
                    // Send subscription confirmation email
                    await _notificationService.SendSubscriptionConfirmationAsync(((UserDto)userResult.data).Email, ((UserDto)userResult.data).FullName, _mapper.Map<SubscriptionDto>(sub), tokenModel);
                    _logger.LogInformation("Subscription confirmation email sent to {Email}", ((UserDto)userResult.data).Email);
                }
                upgraded++;
            }
        }
        return new JsonModel { data = upgraded, Message = $"{upgraded} subscriptions upgraded.", StatusCode = 200 };
    }

    /// <summary>
    /// Performs bulk actions on subscriptions (admin only)
    /// </summary>
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
            if (tokenModel.RoleID != 1 && !await HasAccessToSubscription(tokenModel.UserID, subscriptionId))
            {
                return new JsonModel { data = new object(), Message = "Access denied", StatusCode = 403 };
            }

            var entity = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (entity == null)
                return new JsonModel { data = new object(), Message = "Subscription not found", StatusCode = 404 };

            // Extend the subscription
            entity.EndDate = entity.EndDate?.AddDays(additionalDays) ?? DateTime.UtcNow.AddDays(additionalDays);
            entity.NextBillingDate = entity.NextBillingDate.AddDays(additionalDays);
            entity.UpdatedBy = tokenModel.UserID;
            entity.UpdatedDate = DateTime.UtcNow;

            var updated = await _subscriptionRepository.UpdateAsync(entity);

            return new JsonModel { data = _mapper.Map<SubscriptionDto>(updated), Message = $"Subscription extended by {additionalDays} days", StatusCode = 200 };
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
        var entity = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
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
            
            // Update subscription with new billing date
            entity.NextBillingDate = entity.NextBillingDate.AddMonths(1);
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
        
        // NEW: Process prorated payment through Stripe with subscription upgrade
        if (!string.IsNullOrEmpty(entity.StripeSubscriptionId))
        {
            try
            {
                // For Stripe prorated upgrades, we should update the subscription with the new price
                // and let Stripe handle the proration calculation
                _logger.LogInformation("Processing prorated upgrade for Stripe subscription {StripeSubscriptionId} for subscription {SubscriptionId}", 
                    entity.StripeSubscriptionId, subscriptionId);
                
                // Use Stripe service to update the subscription with new price
                var stripeUpdateResult = await _stripeService.UpdateSubscriptionAsync(
                    entity.StripeSubscriptionId,
                    newPlan.StripeMonthlyPriceId, // Assuming monthly for simplicity
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
        
        // Update local subscription
        entity.SubscriptionPlanId = Guid.Parse(newPlanId);
        entity.CurrentPrice = newPlan.Price;
        entity.UpdatedBy = tokenModel.UserID;
        entity.UpdatedDate = DateTime.UtcNow;
        
        var updated = await _subscriptionRepository.UpdateAsync(entity);
        
        return new JsonModel { data = _mapper.Map<SubscriptionDto>(updated), Message = "Subscription prorated upgrade completed successfully", StatusCode = 200 };
    }

    /// <summary>
    /// Changes the billing cycle of a subscription
    /// </summary>
    public async Task<JsonModel> ChangeBillingCycleAsync(string subscriptionId, string newBillingCycleId, TokenModel tokenModel)
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

            // Get the new billing cycle
            var newBillingCycle = await _subscriptionRepository.GetBillingCycleByIdAsync(Guid.Parse(newBillingCycleId));
            if (newBillingCycle == null)
                return new JsonModel { data = new object(), Message = "Billing cycle not found", StatusCode = 404 };

            // Update billing cycle
            entity.BillingCycleId = Guid.Parse(newBillingCycleId);
            entity.NextBillingDate = await CalculateNextBillingDateAsync(DateTime.UtcNow, Guid.Parse(newBillingCycleId));
            entity.UpdatedBy = tokenModel.UserID;
            entity.UpdatedDate = DateTime.UtcNow;

            var updated = await _subscriptionRepository.UpdateAsync(entity);

            return new JsonModel { data = _mapper.Map<SubscriptionDto>(updated), Message = "Billing cycle changed successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing billing cycle for subscription {SubscriptionId}", subscriptionId);
            return new JsonModel { data = new object(), Message = "Failed to change billing cycle", StatusCode = 500 };
        }
    }

    #endregion

    public async Task<bool> ActivateSubscriptionAsync(Guid subscriptionId, string? reason = null, TokenModel tokenModel = null)
    {
        try
        {
            _logger.LogInformation("Activating subscription {SubscriptionId} by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
            
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            if (!await ValidateStatusTransitionAsync(subscription.Status, SubscriptionStatus.Active, tokenModel))
            {
                _logger.LogWarning("Invalid status transition from {CurrentStatus} to Active for subscription {SubscriptionId} by user {UserId}", 
                    subscription.Status, subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            var oldStatus = subscription.Status;
            subscription.Status = SubscriptionStatus.Active;
            subscription.UpdatedBy = tokenModel?.UserID;
            subscription.UpdatedDate = DateTime.UtcNow;

            // Add status history
            await _statusHistoryRepository.CreateAsync(new SubscriptionStatusHistory
            {
                SubscriptionId = subscriptionId,
                FromStatus = oldStatus,
                ToStatus = SubscriptionStatus.Active,
                Reason = reason ?? "Subscription activated",
                ChangedAt = DateTime.UtcNow,
                ChangedByUserId = tokenModel?.UserID,
                // Set audit properties for creation
                IsActive = true,
                CreatedBy = tokenModel?.UserID,
                CreatedDate = DateTime.UtcNow
            });

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
            
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            if (!await ValidateStatusTransitionAsync(subscription.Status, SubscriptionStatus.Paused, tokenModel))
            {
                _logger.LogWarning("Invalid status transition from {CurrentStatus} to Paused for subscription {SubscriptionId} by user {UserId}", 
                    subscription.Status, subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            var oldStatus = subscription.Status;
            subscription.Status = SubscriptionStatus.Paused;
            subscription.UpdatedBy = tokenModel?.UserID;
            subscription.UpdatedDate = DateTime.UtcNow;

            // Add status history
            await _statusHistoryRepository.CreateAsync(new SubscriptionStatusHistory
            {
                SubscriptionId = subscriptionId,
                FromStatus = oldStatus,
                ToStatus = SubscriptionStatus.Paused,
                Reason = reason ?? "Subscription paused",
                ChangedAt = DateTime.UtcNow,
                ChangedByUserId = tokenModel?.UserID,
                // Set audit properties for creation
                IsActive = true,
                CreatedBy = tokenModel?.UserID,
                CreatedDate = DateTime.UtcNow
            });

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
            
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            if (!await ValidateStatusTransitionAsync(subscription.Status, SubscriptionStatus.Active, tokenModel))
            {
                _logger.LogWarning("Invalid status transition from {CurrentStatus} to Active for subscription {SubscriptionId} by user {UserId}", 
                    subscription.Status, subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            var oldStatus = subscription.Status;
            subscription.Status = SubscriptionStatus.Active;
            subscription.UpdatedBy = tokenModel?.UserID;
            subscription.UpdatedDate = DateTime.UtcNow;

            // Add status history
            await _statusHistoryRepository.CreateAsync(new SubscriptionStatusHistory
            {
                SubscriptionId = subscriptionId,
                FromStatus = oldStatus,
                ToStatus = SubscriptionStatus.Active,
                Reason = reason ?? "Subscription resumed",
                ChangedAt = DateTime.UtcNow,
                ChangedByUserId = tokenModel?.UserID,
                // Set audit properties for creation
                IsActive = true,
                CreatedBy = tokenModel?.UserID,
                CreatedDate = DateTime.UtcNow
            });

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
            
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            if (!await ValidateStatusTransitionAsync(subscription.Status, SubscriptionStatus.Cancelled, tokenModel))
            {
                _logger.LogWarning("Invalid status transition from {CurrentStatus} to Cancelled for subscription {SubscriptionId} by user {UserId}", 
                    subscription.Status, subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            var oldStatus = subscription.Status;
            subscription.Status = SubscriptionStatus.Cancelled;
            subscription.UpdatedBy = tokenModel?.UserID;
            subscription.UpdatedDate = DateTime.UtcNow;
            subscription.CancelledAt = DateTime.UtcNow;

            // Add status history
            await _statusHistoryRepository.CreateAsync(new SubscriptionStatusHistory
            {
                SubscriptionId = subscriptionId,
                FromStatus = oldStatus,
                ToStatus = SubscriptionStatus.Cancelled,
                Reason = reason ?? "Subscription cancelled",
                ChangedAt = DateTime.UtcNow,
                ChangedByUserId = tokenModel?.UserID,
                // Set audit properties for creation
                IsActive = true,
                CreatedBy = tokenModel?.UserID,
                CreatedDate = DateTime.UtcNow
            });

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
            
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            if (!await ValidateStatusTransitionAsync(subscription.Status, SubscriptionStatus.Suspended, tokenModel))
            {
                _logger.LogWarning("Invalid status transition from {CurrentStatus} to Suspended for subscription {SubscriptionId} by user {UserId}", 
                    subscription.Status, subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            var oldStatus = subscription.Status;
            subscription.Status = SubscriptionStatus.Suspended;
            subscription.UpdatedBy = tokenModel?.UserID;
            subscription.UpdatedDate = DateTime.UtcNow;

            // Add status history
            await _statusHistoryRepository.CreateAsync(new SubscriptionStatusHistory
            {
                SubscriptionId = subscriptionId,
                FromStatus = oldStatus,
                ToStatus = SubscriptionStatus.Suspended,
                Reason = reason ?? "Subscription suspended",
                ChangedAt = DateTime.UtcNow,
                ChangedByUserId = tokenModel?.UserID,
                // Set audit properties for creation
                IsActive = true,
                CreatedBy = tokenModel?.UserID,
                CreatedDate = DateTime.UtcNow
            });

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
            
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            if (!await ValidateStatusTransitionAsync(subscription.Status, SubscriptionStatus.Active, tokenModel))
            {
                _logger.LogWarning("Invalid status transition from {CurrentStatus} to Active for subscription {SubscriptionId} by user {UserId}", 
                    subscription.Status, subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            var oldStatus = subscription.Status;
            subscription.Status = SubscriptionStatus.Active;
            subscription.UpdatedBy = tokenModel?.UserID;
            subscription.UpdatedDate = DateTime.UtcNow;
            subscription.RenewedAt = DateTime.UtcNow;

            // Add status history
            await _statusHistoryRepository.CreateAsync(new SubscriptionStatusHistory
            {
                SubscriptionId = subscriptionId,
                FromStatus = oldStatus,
                ToStatus = SubscriptionStatus.Active,
                Reason = reason ?? "Subscription renewed",
                ChangedAt = DateTime.UtcNow,
                ChangedByUserId = tokenModel?.UserID,
                // Set audit properties for creation
                IsActive = true,
                CreatedBy = tokenModel?.UserID,
                CreatedDate = DateTime.UtcNow
            });

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
            
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            if (!await ValidateStatusTransitionAsync(subscription.Status, SubscriptionStatus.Expired, tokenModel))
            {
                _logger.LogWarning("Invalid status transition from {CurrentStatus} to Expired for subscription {SubscriptionId} by user {UserId}", 
                    subscription.Status, subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            var oldStatus = subscription.Status;
            subscription.Status = SubscriptionStatus.Expired;
            subscription.UpdatedBy = tokenModel?.UserID;
            subscription.UpdatedDate = DateTime.UtcNow;
            subscription.ExpiredAt = DateTime.UtcNow;

            // Add status history
            await _statusHistoryRepository.CreateAsync(new SubscriptionStatusHistory
            {
                SubscriptionId = subscriptionId,
                FromStatus = oldStatus,
                ToStatus = SubscriptionStatus.Expired,
                Reason = reason ?? "Subscription expired",
                ChangedAt = DateTime.UtcNow,
                ChangedByUserId = tokenModel?.UserID,
                // Set audit properties for creation
                IsActive = true,
                CreatedBy = tokenModel?.UserID,
                CreatedDate = DateTime.UtcNow
            });

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
            
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            if (!await ValidateStatusTransitionAsync(subscription.Status, SubscriptionStatus.PaymentFailed, tokenModel))
            {
                _logger.LogWarning("Invalid status transition from {CurrentStatus} to PaymentFailed for subscription {SubscriptionId} by user {UserId}", 
                    subscription.Status, subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            var oldStatus = subscription.Status;
            subscription.Status = SubscriptionStatus.PaymentFailed;
            subscription.UpdatedBy = tokenModel?.UserID;
            subscription.UpdatedDate = DateTime.UtcNow;

            // Add status history
            await _statusHistoryRepository.CreateAsync(new SubscriptionStatusHistory
            {
                SubscriptionId = subscriptionId,
                FromStatus = oldStatus,
                ToStatus = SubscriptionStatus.PaymentFailed,
                Reason = reason ?? "Payment failed",
                ChangedAt = DateTime.UtcNow,
                ChangedByUserId = tokenModel?.UserID,
                // Set audit properties for creation
                IsActive = true,
                CreatedBy = tokenModel?.UserID,
                CreatedDate = DateTime.UtcNow
            });

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
            
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found by user {UserId}", subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            if (!await ValidateStatusTransitionAsync(subscription.Status, SubscriptionStatus.Active, tokenModel))
            {
                _logger.LogWarning("Invalid status transition from {CurrentStatus} to Active for subscription {SubscriptionId} by user {UserId}", 
                    subscription.Status, subscriptionId, tokenModel?.UserID ?? 0);
                return false;
            }

            var oldStatus = subscription.Status;
            subscription.Status = SubscriptionStatus.Active;
            subscription.UpdatedBy = tokenModel?.UserID;
            subscription.UpdatedDate = DateTime.UtcNow;

            // Add status history
            await _statusHistoryRepository.CreateAsync(new SubscriptionStatusHistory
            {
                SubscriptionId = subscriptionId,
                FromStatus = oldStatus,
                ToStatus = SubscriptionStatus.Active,
                Reason = reason ?? "Payment succeeded",
                ChangedAt = DateTime.UtcNow,
                ChangedByUserId = tokenModel?.UserID,
                // Set audit properties for creation
                IsActive = true,
                CreatedBy = tokenModel?.UserID,
                CreatedDate = DateTime.UtcNow
            });

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
            
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
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

            // Add status history
            await _statusHistoryRepository.CreateAsync(new SubscriptionStatusHistory
            {
                SubscriptionId = subscriptionId,
                FromStatus = oldStatus,
                ToStatus = newStatus,
                Reason = reason ?? $"Status updated to {newStatus}",
                ChangedAt = DateTime.UtcNow,
                ChangedByUserId = tokenModel?.UserID,
                // Set audit properties for creation
                IsActive = true,
                CreatedBy = tokenModel?.UserID,
                CreatedDate = DateTime.UtcNow
            });

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
            // Define valid status transitions
            var validTransitions = new Dictionary<string, List<string>>
            {
                [SubscriptionStatus.Pending] = new List<string> { SubscriptionStatus.Active, SubscriptionStatus.Cancelled, SubscriptionStatus.Expired },
                [SubscriptionStatus.Active] = new List<string> { SubscriptionStatus.Paused, SubscriptionStatus.Suspended, SubscriptionStatus.Cancelled, SubscriptionStatus.Expired, SubscriptionStatus.PaymentFailed },
                [SubscriptionStatus.Paused] = new List<string> { SubscriptionStatus.Active, SubscriptionStatus.Cancelled, SubscriptionStatus.Expired },
                [SubscriptionStatus.Suspended] = new List<string> { SubscriptionStatus.Active, SubscriptionStatus.Cancelled, SubscriptionStatus.Expired },
                [SubscriptionStatus.PaymentFailed] = new List<string> { SubscriptionStatus.Active, SubscriptionStatus.Cancelled, SubscriptionStatus.Expired },
                [SubscriptionStatus.Expired] = new List<string> { SubscriptionStatus.Active, SubscriptionStatus.Cancelled },
                [SubscriptionStatus.Cancelled] = new List<string> { SubscriptionStatus.Active }, // Reactivation allowed
                [SubscriptionStatus.TrialActive] = new List<string> { SubscriptionStatus.Active, SubscriptionStatus.Cancelled, SubscriptionStatus.Expired }
            };

            if (validTransitions.ContainsKey(currentStatus) && validTransitions[currentStatus].Contains(newStatus))
            {
                _logger.LogInformation("Status transition from {CurrentStatus} to {NewStatus} validated by user {UserId}", 
                    currentStatus, newStatus, tokenModel?.UserID ?? 0);
                return true;
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
            // Define valid next statuses for each current status
            var nextStatuses = new Dictionary<string, List<string>>
            {
                [SubscriptionStatus.Pending] = new List<string> { SubscriptionStatus.Active, SubscriptionStatus.Cancelled, SubscriptionStatus.Expired },
                [SubscriptionStatus.Active] = new List<string> { SubscriptionStatus.Paused, SubscriptionStatus.Suspended, SubscriptionStatus.Cancelled, SubscriptionStatus.Expired, SubscriptionStatus.PaymentFailed },
                [SubscriptionStatus.Paused] = new List<string> { SubscriptionStatus.Active, SubscriptionStatus.Cancelled, SubscriptionStatus.Expired },
                [SubscriptionStatus.Suspended] = new List<string> { SubscriptionStatus.Active, SubscriptionStatus.Cancelled, SubscriptionStatus.Expired },
                [SubscriptionStatus.PaymentFailed] = new List<string> { SubscriptionStatus.Active, SubscriptionStatus.Cancelled, SubscriptionStatus.Expired },
                [SubscriptionStatus.Expired] = new List<string> { SubscriptionStatus.Active, SubscriptionStatus.Cancelled },
                [SubscriptionStatus.Cancelled] = new List<string> { SubscriptionStatus.Active },
                [SubscriptionStatus.TrialActive] = new List<string> { SubscriptionStatus.Active, SubscriptionStatus.Cancelled, SubscriptionStatus.Expired }
            };

            if (nextStatuses.ContainsKey(currentStatus))
            {
                var nextStatus = nextStatuses[currentStatus].FirstOrDefault() ?? "No valid next status";
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
            var subscription = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
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
                
                // If converting from trial, set the real price from the plan
                if (subscription.CurrentPrice == 0 && subscription.SubscriptionPlan != null)
                {
                    subscription.CurrentPrice = subscription.SubscriptionPlan.Price;
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
            var subscription = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
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
            var subscription = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
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
            var subscription = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
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
            var subscription = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
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

            // Calculate new trial end date
            var newTrialEndDate = subscription.TrialEndDate?.AddDays(additionalDays) ?? DateTime.UtcNow.AddDays(additionalDays);
            
            // Update trial end date
            subscription.TrialEndDate = newTrialEndDate;
            subscription.UpdatedBy = null; // System action
            subscription.UpdatedDate = DateTime.UtcNow;

            // Add status history
            await _statusHistoryRepository.CreateAsync(new SubscriptionStatusHistory
            {
                SubscriptionId = subscription.Id,
                FromStatus = subscription.Status,
                ToStatus = subscription.Status, // Same status, but trial extended
                Reason = $"Trial extended by {additionalDays} days. {reason}",
                ChangedAt = DateTime.UtcNow,

                ChangedByUserId = null, // System action
                // Set audit properties for creation
                IsActive = true,
                CreatedBy = null, // System action
                CreatedDate = DateTime.UtcNow
            });

            await _subscriptionRepository.UpdateAsync(subscription);

            // Log audit trail
           

            _logger.LogInformation("Trial extended for subscription {SubscriptionId} by {AdditionalDays} days", subscriptionId, additionalDays);

            return new JsonModel
            {
                data = new { NewTrialEndDate = newTrialEndDate },
                Message = $"Trial extended by {additionalDays} days. New end date: {newTrialEndDate:MMM dd, yyyy}",
                StatusCode = 200
            };
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
                Amount = subscription.CurrentPrice,
                Currency = "usd"
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
    private DateTime CalculateNextBillingDate(Subscription subscription)
    {
        var billingCycle = subscription.BillingCycle;
        
        return billingCycle.Name.ToLower() switch
        {
            "monthly" => DateTime.UtcNow.AddMonths(1),
            "quarterly" => DateTime.UtcNow.AddMonths(3),
            "annual" => DateTime.UtcNow.AddYears(1),
            "weekly" => DateTime.UtcNow.AddDays(7),
            "daily" => DateTime.UtcNow.AddDays(1),
            _ => DateTime.UtcNow.AddMonths(1) // Default to monthly
        };
    }

    /// <summary>
    /// Get subscription lifecycle status
    /// </summary>
    public async Task<JsonModel> GetSubscriptionLifecycleStatusAsync(string subscriptionId, TokenModel tokenModel = null)
    {
        try
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
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
            
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
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
            
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
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
            var subscription = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            return subscription != null && subscription.UserId == userId;
        }
        catch
        {
            return false;
        }
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
                [SubscriptionStatus.Pending] = new List<string> { "cancel" },
                [SubscriptionStatus.Active] = new List<string> { "cancel", "pause", "extend" },
                [SubscriptionStatus.Paused] = new List<string> { "cancel", "resume", "extend" },
                [SubscriptionStatus.Suspended] = new List<string> { "cancel", "resume", "extend" },
                [SubscriptionStatus.PaymentFailed] = new List<string> { "cancel", "extend" },
                [SubscriptionStatus.Expired] = new List<string> { "cancel" },
                [SubscriptionStatus.Cancelled] = new List<string> { }, // No actions allowed on cancelled subscriptions
                [SubscriptionStatus.TrialActive] = new List<string> { "cancel", "extend" }
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
    private async Task<string> EnsureStripeCustomerAsync(UserDto user, TokenModel tokenModel)
    {
        // If user already has Stripe customer ID, return it
        if (!string.IsNullOrEmpty(user.StripeCustomerId))
        {
            _logger.LogInformation("User {UserId} already has Stripe customer ID: {StripeCustomerId}", user.Id, user.StripeCustomerId);
            return user.StripeCustomerId;
        }
        
        // Create new Stripe customer
        _logger.LogInformation("Creating new Stripe customer for user {UserId} with email {Email}", user.Id, user.Email);
        
        var stripeCustomerId = await _stripeService.CreateCustomerAsync(
            user.Email, 
            user.FullName, 
            tokenModel
        );
        
        // Update user with Stripe customer ID
        try
        {
            // Create update DTO with Stripe customer ID
            var updateUserDto = new UpdateUserDto
            {
                StripeCustomerId = stripeCustomerId
            };
            
            await _userService.UpdateUserAsync(user.Id, updateUserDto, tokenModel);
            
            _logger.LogInformation("Successfully updated user {UserId} with Stripe customer ID: {StripeCustomerId}", user.Id, stripeCustomerId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to update user {UserId} with Stripe customer ID {StripeCustomerId}. Customer created but user not updated.", user.Id, stripeCustomerId);
            // Don't fail the entire operation if user update fails
        }
        
        return stripeCustomerId;
    }

    /// <summary>
    /// Gets the appropriate Stripe price ID based on billing cycle ID
    /// </summary>
    private async Task<string> GetStripePriceIdForBillingCycleAsync(SubscriptionPlan plan, Guid billingCycleId)
    {
        try
        {
            // Get the billing cycle name from the database
            var billingCycle = await _subscriptionRepository.GetBillingCycleByIdAsync(billingCycleId);
            if (billingCycle == null)
            {
                _logger.LogWarning("Billing cycle {BillingCycleId} not found, using default monthly price", billingCycleId);
                return plan.StripeMonthlyPriceId;
            }

            var billingCycleName = billingCycle.Name.ToLower();
            return billingCycleName switch
            {
                "monthly" => plan.StripeMonthlyPriceId,
                "quarterly" => plan.StripeQuarterlyPriceId,
                "annual" => plan.StripeAnnualPriceId,
                _ => plan.StripeMonthlyPriceId // Default fallback
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting billing cycle {BillingCycleId}, using default monthly price", billingCycleId);
            return plan.StripeMonthlyPriceId;
        }
    }

    /// <summary>
    /// Calculates the next billing date based on billing cycle ID
    /// </summary>
    private async Task<DateTime> CalculateNextBillingDateAsync(DateTime startDate, Guid billingCycleId)
    {
        try
        {
            // Get the billing cycle from the database
            var billingCycle = await _subscriptionRepository.GetBillingCycleByIdAsync(billingCycleId);
            if (billingCycle == null)
            {
                _logger.LogWarning("Billing cycle {BillingCycleId} not found, using default monthly calculation", billingCycleId);
                return startDate.AddMonths(1);
            }

            var billingCycleName = billingCycle.Name.ToLower();
            return billingCycleName switch
            {
                "monthly" => startDate.AddMonths(1),
                "quarterly" => startDate.AddMonths(3),
                "annual" => startDate.AddYears(1),
                _ => startDate.AddMonths(1) // Default fallback
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting billing cycle {BillingCycleId}, using default monthly calculation", billingCycleId);
            return startDate.AddMonths(1);
        }
    }

    private async Task<DateTime> CalculateEndDateAsync(DateTime startDate, Guid billingCycleId)
    {
        try
        {
            // Get the billing cycle from the database
            var billingCycle = await _subscriptionRepository.GetBillingCycleByIdAsync(billingCycleId);
            if (billingCycle == null)
            {
                _logger.LogWarning("Billing cycle {BillingCycleId} not found, using default monthly calculation", billingCycleId);
                return startDate.AddMonths(1);
            }

            var billingCycleName = billingCycle.Name.ToLower();
            return billingCycleName switch
            {
                "monthly" => startDate.AddMonths(1),
                "quarterly" => startDate.AddMonths(3),
                "annual" => startDate.AddYears(1),
                _ => startDate.AddMonths(1) // Default fallback
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating end date for billing cycle {BillingCycleId}", billingCycleId);
            return startDate.AddMonths(1); // Default fallback
        }
    }

    #endregion
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
