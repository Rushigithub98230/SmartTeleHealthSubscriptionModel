using AutoMapper;
using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Application.Services;

/// <summary>
/// Core subscription management service that handles all subscription-related operations including:
/// - Subscription lifecycle management (create, update, cancel, pause, resume)
/// - Stripe payment integration and synchronization
/// - Privilege-based access control and usage tracking
/// - Billing and payment processing
/// - Subscription status management and transitions
/// - Trial subscription handling
/// - Automated billing and renewals
/// </summary>
public class SubscriptionService : ISubscriptionService
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<SubscriptionService> _logger;
    private readonly IStripeService _stripeService;
    private readonly IPrivilegeService _privilegeService;
    private readonly INotificationService _notificationService;
      
    private readonly IUserService _userService;
    private readonly ISubscriptionPlanPrivilegeRepository _planPrivilegeRepo;
    private readonly IUserSubscriptionPrivilegeUsageRepository _usageRepo;
    private readonly ISubscriptionBillingService _billingService; // UPDATED: Use consolidated service
    private readonly ISubscriptionNotificationService _subscriptionNotificationService;
    private readonly IPrivilegeRepository _privilegeRepository;
    private readonly ICategoryService _categoryService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymentService _paymentService; // SRP Refactoring: Added for payment method delegation

    /// <summary>
    /// Initializes a new instance of the SubscriptionService with all required dependencies
    /// </summary>
    /// <param name="subscriptionRepository">Repository for subscription data access operations</param>
    /// <param name="mapper">AutoMapper instance for entity-DTO mapping</param>
    /// <param name="logger">Logger instance for logging operations and errors</param>
    /// <param name="stripeService">Stripe payment service for payment processing</param>
    /// <param name="privilegeService">Service for privilege management and usage tracking</param>
    /// <param name="notificationService">Service for sending notifications to users</param>
    /// <param name="userService">Service for user management operations</param>
    /// <param name="planPrivilegeRepo">Repository for subscription plan privilege data access</param>
    /// <param name="usageRepo">Repository for user subscription privilege usage tracking</param>
    /// <param name="billingService">Service for billing and payment record management</param>
    /// <param name="subscriptionNotificationService">Service for subscription-specific notifications</param>
    /// <param name="privilegeRepository">Repository for privilege data access</param>
    /// <param name="categoryService">Service for category management operations</param>
    /// <param name="unitOfWork">Unit of work for transaction management</param>
    /// <param name="paymentService">Service for payment method management (SRP Refactoring)</param>
    public SubscriptionService(
        ISubscriptionRepository subscriptionRepository,
        IMapper mapper,
        ILogger<SubscriptionService> logger,
        IStripeService stripeService,
        IPrivilegeService privilegeService,
        INotificationService notificationService,
          
        IUserService userService,
        ISubscriptionPlanPrivilegeRepository planPrivilegeRepo,
        IUserSubscriptionPrivilegeUsageRepository usageRepo,
        ISubscriptionBillingService billingService, // UPDATED: Use consolidated service
        ISubscriptionNotificationService subscriptionNotificationService,
        IPrivilegeRepository privilegeRepository,
        ICategoryService categoryService,
        IUnitOfWork unitOfWork,
        IPaymentService paymentService)
    {
        _subscriptionRepository = subscriptionRepository ?? throw new ArgumentNullException(nameof(subscriptionRepository));
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
        _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService)); // SRP Refactoring
    }

    /// <summary>
    /// Retrieves a specific subscription by its ID with proper access control validation
    /// </summary>
    /// <param name="subscriptionId">The unique identifier of the subscription to retrieve</param>
    /// <param name="tokenModel">Token containing user authentication and authorization information</param>
    /// <returns>JsonModel containing the subscription data or error information</returns>
    /// <remarks>
    /// Access Control:
    /// - Admins (RoleID = 332) can access any subscription
    /// - Regular users can only access their own subscriptions
    /// - Returns 403 Forbidden if user doesn't have access
    /// - Returns 404 Not Found if subscription doesn't exist
    /// </remarks>
    public async Task<JsonModel> GetSubscriptionAsync(string subscriptionId, TokenModel tokenModel)
    {
        try
        {
            // Validate token permissions - ensure user has access to this subscription
            if (tokenModel.RoleID != (int)RoleId.Admin && !await HasAccessToSubscription(tokenModel.UserID, subscriptionId))
            {
                return new JsonModel { data = new object(), Message = "Access denied", StatusCode = 403 };
            }

            // Retrieve subscription entity from repository with related entities
            var entity = await _subscriptionRepository.GetByIdWithDetailsAsync(Guid.Parse(subscriptionId));
            if (entity == null)
                return new JsonModel { data = new object(), Message = "Subscription not found", StatusCode = 404 };
            
            // Map entity to DTO and return success response
            return new JsonModel { data = _mapper.Map<SubscriptionDto>(entity), Message = "Subscription retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription {SubscriptionId}", subscriptionId);
            return new JsonModel { data = new object(), Message = "Failed to retrieve subscription", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Retrieves all subscriptions for a specific user with access control validation
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose subscriptions to retrieve</param>
    /// <param name="tokenModel">Token containing user authentication and authorization information</param>
    /// <returns>JsonModel containing the list of user subscriptions or error information</returns>
    /// <remarks>
    /// Access Control:
    /// - Admins (RoleID = 332) can access any user's subscriptions
    /// - Regular users can only access their own subscriptions
    /// - Returns 403 Forbidden if user doesn't have access
    /// - Returns all subscriptions (active, paused, cancelled, etc.) for the user
    /// </remarks>
    public async Task<JsonModel> GetUserSubscriptionsAsync(int userId, TokenModel tokenModel)
    {
        try
        {
            // Validate token permissions - user can only access their own subscriptions unless admin
            if (tokenModel.RoleID != (int)RoleId.Admin && tokenModel.UserID != userId)
            {
                return new JsonModel { data = new object(), Message = "Access denied", StatusCode = 403 };
            }

            // Retrieve all subscriptions for the specified user
            var entities = await _subscriptionRepository.GetByUserIdAsync(userId);
            
            // Map entities to DTOs and return success response
            var dtos = _mapper.Map<IEnumerable<SubscriptionDto>>(entities);
            return new JsonModel { data = dtos, Message = "User subscriptions retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscriptions for user {UserId}", userId);
            return new JsonModel { data = new object(), Message = "Failed to retrieve user subscriptions", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetUserSubscriptionsWithFilteringAsync(int page, int pageSize, string? searchTerm, string[]? status, string[]? planId, DateTime? startDate, DateTime? endDate, string? sortBy, string? sortOrder, TokenModel tokenModel)
    {
        try
        {
            var userId = tokenModel.UserID; // Get userId from token

            _logger.LogInformation("Retrieving user subscriptions with database-level filtering - UserId: {UserId}, Page: {Page}, PageSize: {PageSize}, SearchTerm: {SearchTerm}",
                userId, page, pageSize, searchTerm);

            // Create filter DTO for database-level filtering
            var filter = new SubscriptionFilterDto
            {
                Page = page,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                Statuses = status?.ToList(),
                PlanIds = planId?.Where(id => Guid.TryParse(id, out _)).Select(Guid.Parse).ToList(),
                CreatedDateFrom = startDate,
                CreatedDateTo = endDate,
                SortColumn = sortBy ?? "CreatedDate",
                SortOrder = sortOrder ?? "desc"
            };

            // Use database-level filtering, pagination, and sorting
            var (subscriptions, totalCount) = await _subscriptionRepository.GetUserSubscriptionsWithFilteringAsync(userId, filter);
            var dtos = _mapper.Map<IEnumerable<SubscriptionDto>>(subscriptions);

            // Return with pagination metadata
            return new JsonModel
            {
                data = dtos,
                meta = new Meta
                {
                    TotalRecords = totalCount,
                    PageSize = pageSize,
                    CurrentPage = page,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                    HasNextPage = page < (int)Math.Ceiling((double)totalCount / pageSize),
                    HasPreviousPage = page > 1
                },
                Message = "User subscriptions retrieved successfully with database-level filtering",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user subscriptions with filtering for user {UserId}", tokenModel.UserID);
            return new JsonModel { data = new object(), Message = "Failed to retrieve user subscriptions", StatusCode = 500 };
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

    // SRP Refactoring: Removed duplicate CalculateNextBillingDateAsync - now uses BillingService.CalculateNextBillingDate()
    /// <summary>
    /// Calculates the next billing date based on billing cycle ID
    /// </summary>
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
            return startDate.AddMonths(1); // Safe fallback
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
            return startDate.AddMonths(1); // Safe fallback
        }
    }









    /// <summary>
    /// Retrieves a specific subscription plan by its ID
    /// </summary>
    /// <param name="planId">The unique identifier of the subscription plan to retrieve</param>
    /// <param name="tokenModel">Token containing user authentication and authorization information</param>
    /// <returns>JsonModel containing the subscription plan data or error information</returns>
    /// <remarks>
    /// This method:
    /// - Retrieves a specific subscription plan by ID from the repository
    /// - Maps the entity to DTO for response
    /// - Returns 404 if plan is not found
    /// - Used for detailed plan information display
    /// - No additional access control - plan details are generally public
    /// </remarks>

    /// <summary>
    /// Retrieves billing history for a specific subscription with access control validation
    /// </summary>
    /// <param name="subscriptionId">The unique identifier of the subscription to get billing history for</param>
    /// <param name="tokenModel">Token containing user authentication and authorization information</param>
    /// <returns>JsonModel containing the billing history or error information</returns>
    /// <remarks>
    /// This method:
    /// - Validates that the subscription exists
    /// - Checks user access to the subscription (admin or subscription owner)
    /// - Retrieves billing records from the billing service
    /// - Transforms billing records to BillingHistoryDto format
    /// - Includes payment status, amounts, and dates
    /// - Used for subscription billing history display
    /// - Logs errors for troubleshooting
    /// 
    /// Access Control:
    /// - Admins can access any subscription's billing history
    /// - Users can only access their own subscription's billing history
    /// </remarks>
    /// <summary>
    /// SRP Refactoring: This method violates Single Responsibility Principle.
    /// Billing history retrieval should be handled by BillingService directly, not through SubscriptionService.
    /// This method is kept for backward compatibility but will be removed in a future release.
    /// Please use BillingService.GetSubscriptionBillingHistoryAsync directly.
    /// </summary>
    [Obsolete("This method violates SRP. Use BillingService.GetSubscriptionBillingHistoryAsync directly. This will be removed in the next release.")]
    public async Task<JsonModel> GetBillingHistoryAsync(string subscriptionId, TokenModel tokenModel)
    {
        try
        {
            _logger.LogWarning("DEPRECATED: GetBillingHistoryAsync called from SubscriptionService. Use BillingService instead.");
            
            // Retrieve subscription to validate it exists
            var subscription = await _subscriptionRepository.GetByIdWithDetailsAsync(Guid.Parse(subscriptionId));
            if (subscription == null)
                return new JsonModel { data = new object(), Message = "Subscription not found", StatusCode = 404 };

            // Get billing records for this subscription from billing service
            var billingRecords = await _billingService.GetSubscriptionBillingHistoryAsync(subscription.Id, tokenModel);
            
            if (billingRecords.StatusCode != 200)
                return new JsonModel { data = new object(), Message = "Failed to retrieve billing history", StatusCode = 500 };

            // Transform billing records to BillingHistoryDto format
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

    /// <summary>
    /// Retrieves payment methods for a specific user with access control validation
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose payment methods to retrieve</param>
    /// <param name="tokenModel">Token containing user authentication and authorization information</param>
    /// <returns>JsonModel containing the user's payment methods or error information</returns>
    /// <remarks>
    /// This method:
    /// - Validates user access to payment methods (admin or user accessing their own)
    /// - Retrieves payment methods from Stripe service
    /// - Returns payment method details including type, last4 digits, expiry
    /// - Used for payment method management and selection
    /// - Integrates with Stripe for payment method data
    /// 
    /// Access Control:
    /// - Admins can access any user's payment methods
    /// - Users can only access their own payment methods
    /// - Returns 403 Forbidden if access is denied
    /// </remarks>
    /// <summary>
    /// SRP Refactoring: This method has been moved to PaymentService.
    /// This wrapper is kept temporarily for backward compatibility and will be removed in a future release.
    /// Please use PaymentService.GetPaymentMethodsAsync instead.
    /// </summary>
    [Obsolete("This method has been moved to PaymentService. Use PaymentService.GetPaymentMethodsAsync instead. This will be removed in the next release.")]
    public async Task<JsonModel> GetPaymentMethodsAsync(int userId, TokenModel tokenModel)
    {
        _logger.LogWarning("DEPRECATED: GetPaymentMethodsAsync called from SubscriptionService. Use PaymentService instead.");
        return await _paymentService.GetPaymentMethodsAsync(userId, tokenModel);
    }

    /// <summary>
    /// Adds a payment method to a user's account with access control validation
    /// </summary>
    /// <param name="userId">The unique identifier of the user to add the payment method to</param>
    /// <param name="paymentMethodId">The Stripe payment method ID to add</param>
    /// <param name="tokenModel">Token containing user authentication and authorization information</param>
    /// <returns>JsonModel containing the added payment method or error information</returns>
    /// <remarks>
    /// This method:
    /// - Validates user access to add payment methods (admin or user adding to their own account)
    /// - Adds the payment method to the user's Stripe customer account
    /// - Returns the payment method details
    /// - Used for payment method management and setup
    /// - Integrates with Stripe for payment method addition
    /// 
    /// Access Control:
    /// - Admins can add payment methods to any user's account
    /// - Users can only add payment methods to their own account
    /// - Returns 403 Forbidden if access is denied
    /// </remarks>
    /// <summary>
    /// SRP Refactoring: This method has been moved to PaymentService.
    /// This wrapper is kept temporarily for backward compatibility and will be removed in a future release.
    /// Please use PaymentService.AddPaymentMethodAsync instead.
    /// </summary>
    [Obsolete("This method has been moved to PaymentService. Use PaymentService.AddPaymentMethodAsync instead. This will be removed in the next release.")]
    public async Task<JsonModel> AddPaymentMethodAsync(int userId, string paymentMethodId, TokenModel tokenModel)
    {
        _logger.LogWarning("DEPRECATED: AddPaymentMethodAsync called from SubscriptionService. Use PaymentService instead.");
        return await _paymentService.AddPaymentMethodAsync(userId, paymentMethodId, tokenModel);
    }

    /// <summary>
    /// Retrieves a subscription by its plan ID (Admin only method)
    /// </summary>
    /// <param name="planId">The unique identifier of the subscription plan to find subscriptions for</param>
    /// <param name="tokenModel">Token containing user authentication and authorization information</param>
    /// <returns>JsonModel containing the subscription data or error information</returns>
    /// <remarks>
    /// This method:
    /// - Validates admin access (RoleID 332 or 2)
    /// - Retrieves subscription associated with the specified plan ID
    /// - Maps entity to DTO for response
    /// - Used for administrative subscription management
    /// - Returns 404 if no subscription found for the plan
    /// - Logs errors for troubleshooting
    /// 
    /// Access Control:
    /// - Admin only (RoleID = 332 or 2)
    /// - Returns 403 Forbidden for non-admin users
    /// </remarks>
    public async Task<JsonModel> GetSubscriptionByPlanIdAsync(string planId, TokenModel tokenModel)
    {
        try
        {
            // Admin only method - validate admin role
            if (tokenModel.RoleID != (int)RoleId.Admin && tokenModel.RoleID != (int)RoleId.Provider)
            {
                return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };
            }

            // Retrieve subscription by plan ID from repository
            var subscription = await _subscriptionRepository.GetByPlanIdAsync(Guid.Parse(planId));
            if (subscription == null)
                return new JsonModel { data = new object(), Message = "Subscription not found for this plan", StatusCode = 404 };
            
            // Map entity to DTO and return success response
            return new JsonModel { data = _mapper.Map<SubscriptionDto>(subscription), Message = "Subscription retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription by plan ID {PlanId}", planId);
            return new JsonModel { data = new object(), Message = "Failed to retrieve subscription", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Retrieves all active subscriptions in the system (Admin only method)
    /// </summary>
    /// <param name="tokenModel">Token containing user authentication and authorization information</param>
    /// <returns>JsonModel containing the list of active subscriptions or error information</returns>
    /// <remarks>
    /// This method:
    /// - Validates admin access (RoleID 332 or 2)
    /// - Retrieves all active subscriptions from the repository
    /// - Maps entities to DTOs for response
    /// - Used for administrative monitoring and management
    /// - Returns all subscriptions with Active status
    /// - Logs errors for troubleshooting
    /// 
    /// Access Control:
    /// - Admin only (RoleID = 332 or 2)
    /// - Returns 403 Forbidden for non-admin users
    /// </remarks>
    public async Task<JsonModel> GetActiveSubscriptionsAsync(int page, int pageSize, string? searchTerm, string[]? planId, string[]? userId, DateTime? startDate, DateTime? endDate, string? sortBy, string? sortOrder, TokenModel tokenModel)
    {
        try
        {
            // Admin only method - validate admin role
            if (tokenModel.RoleID != (int)RoleId.Admin && tokenModel.RoleID != (int)RoleId.Provider)
            {
                return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };
            }

            _logger.LogInformation("Retrieving active subscriptions with database-level filtering - Page: {Page}, PageSize: {PageSize}, SearchTerm: {SearchTerm}, UserId: {UserId}",
                page, pageSize, searchTerm, tokenModel.UserID);

            // Create filter DTO for database-level filtering
            var filter = new SubscriptionFilterDto
            {
                Page = page,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                PlanIds = planId?.Where(id => Guid.TryParse(id, out _)).Select(Guid.Parse).ToList(),
                UserIds = userId?.Where(id => int.TryParse(id, out _)).Select(int.Parse).ToList(),
                CreatedDateFrom = startDate,
                CreatedDateTo = endDate,
                SortColumn = sortBy ?? "CreatedDate",
                SortOrder = sortOrder ?? "desc"
            };

            // Use database-level filtering, pagination, and sorting
            var (activeSubscriptions, totalCount) = await _subscriptionRepository.GetActiveSubscriptionsWithFilteringAsync(filter);
            var dtos = _mapper.Map<IEnumerable<SubscriptionDto>>(activeSubscriptions);

            // Return with pagination metadata
            return new JsonModel
            {
                data = dtos,
                meta = new Meta
                {
                    TotalRecords = totalCount,
                    PageSize = pageSize,
                    CurrentPage = page,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                    HasNextPage = page < (int)Math.Ceiling((double)totalCount / pageSize),
                    HasPreviousPage = page > 1
                },
                Message = "Active subscriptions retrieved successfully with database-level filtering",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active subscriptions");
            return new JsonModel { data = new object(), Message = "Failed to retrieve active subscriptions", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Retrieves a subscription by its ID (wrapper method for GetSubscriptionAsync)
    /// </summary>
    /// <param name="subscriptionId">The unique identifier of the subscription to retrieve</param>
    /// <param name="tokenModel">Token containing user authentication and authorization information</param>
    /// <returns>JsonModel containing the subscription data or error information</returns>
    /// <remarks>
    /// This method:
    /// - Acts as a wrapper for GetSubscriptionAsync method
    /// - Provides consistent API naming convention
    /// - Delegates to the main GetSubscriptionAsync method
    /// - Used for subscription retrieval by ID
    /// - Maintains same access control and validation as GetSubscriptionAsync
    /// </remarks>
    public async Task<JsonModel> GetSubscriptionByIdAsync(string subscriptionId, TokenModel tokenModel)
    {
        return await GetSubscriptionAsync(subscriptionId, tokenModel);
    }


    public async Task<JsonModel> ProcessPaymentAsync(string subscriptionId, PaymentRequestDto paymentRequest, TokenModel tokenModel)
    {
        try
        {
            // Validate input parameters
            if (string.IsNullOrEmpty(subscriptionId) || !Guid.TryParse(subscriptionId, out _))
            {
                return new JsonModel { data = new object(), Message = "Invalid subscription ID format", StatusCode = 400 };
            }

            if (paymentRequest == null)
            {
                return new JsonModel { data = new object(), Message = "Payment request is required", StatusCode = 400 };
            }

            if (string.IsNullOrEmpty(paymentRequest.PaymentMethodId))
            {
                return new JsonModel { data = new object(), Message = "Payment method ID is required", StatusCode = 400 };
            }

            if (paymentRequest.Amount <= 0)
            {
                return new JsonModel { data = new object(), Message = "Payment amount must be greater than zero", StatusCode = 400 };
            }

            // Validate token permissions
            if (tokenModel.RoleID != (int)RoleId.Admin && !await HasAccessToSubscription(tokenModel.UserID, subscriptionId))
            {
                return new JsonModel { data = new object(), Message = "Access denied", StatusCode = 403 };
            }

            var subscription = await _subscriptionRepository.GetByIdWithDetailsAsync(Guid.Parse(subscriptionId));
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
                    subscription.FailedPaymentAttempts = 0; // Reset failed payment attempts
                    subscription.LastPaymentError = null; // Clear last payment error
                    await _subscriptionRepository.UpdateSubscriptionAsync(subscription);
                }

                // Create billing record for successful payment
                var billingRecordDto = new CreateBillingRecordDto
                {
                    UserId = subscription.UserId,
                    SubscriptionId = subscription.Id.ToString(),
                    Amount = paymentRequest.Amount,
                    CurrencyId = subscription.SubscriptionPlan?.CurrencyId ?? Guid.Empty, // Get currency ID from subscription plan
                    Status = "Paid",
                    Type = "Subscription",
                    Description = $"Payment for subscription {subscription.Id}",
                    BillingDate = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow
                };

                var billingResult = await _billingService.CreateBillingRecordAsync(billingRecordDto, tokenModel);
                if (billingResult.StatusCode != 200)
                {
                    // Log warning but don't fail the payment
                    _logger.LogWarning("Failed to create billing record for payment: {BillingResult}", billingResult.Message);
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
            if (tokenModel.RoleID != (int)RoleId.Admin && !await HasAccessToSubscription(tokenModel.UserID, subscriptionId))
            {
                return new JsonModel { data = new object(), Message = "Access denied", StatusCode = 403 };
            }

            var subscription = await _subscriptionRepository.GetByIdWithDetailsAsync(Guid.Parse(subscriptionId));
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
                        UsageCount = usage.UsedValue,
                        UsagePercentage = planPrivilege.Value == -1 ? 0 : (decimal)usage.UsedValue / planPrivilege.Value * 100,
                        AverageUsagePerUser = usage.UsedValue // For now, using current usage as average
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

    public async Task<JsonModel> GetAllSubscriptionsAsync(int page, int pageSize, string? searchTerm, string[]? status, string[]? planId, string[]? userId, DateTime? startDate, DateTime? endDate, string? sortBy, string? sortOrder, TokenModel tokenModel)
    {
        try
        {
            // Admin only method - validate admin role
            if (tokenModel.RoleID != (int)RoleId.Admin && tokenModel.RoleID != (int)RoleId.Provider)
            {
                return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };
            }

            _logger.LogInformation("Retrieving all subscriptions with database-level filtering - Page: {Page}, PageSize: {PageSize}, SearchTerm: {SearchTerm}, UserId: {UserId}",
                page, pageSize, searchTerm, tokenModel.UserID);

            // Create filter DTO for database-level filtering
            var filter = new SubscriptionFilterDto
            {
                Page = page,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                Statuses = status?.ToList(),
                PlanIds = planId?.Where(id => Guid.TryParse(id, out _)).Select(Guid.Parse).ToList(),
                UserIds = userId?.Where(id => int.TryParse(id, out _)).Select(int.Parse).ToList(),
                CreatedDateFrom = startDate,
                CreatedDateTo = endDate,
                SortColumn = sortBy ?? "CreatedDate",
                SortOrder = sortOrder ?? "desc"
            };

            // Use database-level filtering, pagination, and sorting
            var (subscriptions, totalCount) = await _subscriptionRepository.GetSubscriptionsWithAdvancedFilteringAsync(filter);
            var dtos = _mapper.Map<IEnumerable<SubscriptionDto>>(subscriptions);

            // Return with pagination metadata
            return new JsonModel
            {
                data = dtos,
                meta = new Meta
                {
                    TotalRecords = totalCount,
                    PageSize = pageSize,
                    CurrentPage = page,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                    HasNextPage = page < (int)Math.Ceiling((double)totalCount / pageSize),
                    HasPreviousPage = page > 1
                },
                Message = "All subscriptions retrieved successfully with database-level filtering",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all subscriptions");
            return new JsonModel { data = new object(), Message = "Failed to retrieve subscriptions", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetSubscriptionAnalyticsAsync(string subscriptionId, DateTime? startDate = null, DateTime? endDate = null, TokenModel tokenModel = null)
    {
        try
        {
            // Validate token permissions
            if (tokenModel.RoleID != (int)RoleId.Admin && !await HasAccessToSubscription(tokenModel.UserID, subscriptionId))
            {
                return new JsonModel { data = new object(), Message = "Access denied", StatusCode = 403 };
            }

            var subscription = await _subscriptionRepository.GetByIdWithDetailsAsync(Guid.Parse(subscriptionId));
            if (subscription == null)
                return new JsonModel { data = new object(), Message = "Subscription not found", StatusCode = 404 };

            // Get usage statistics
            var usageStats = await GetUsageStatisticsAsync(subscriptionId, tokenModel);
            if (usageStats.StatusCode != 200)
                return new JsonModel { data = new object(), Message = "Failed to get usage statistics", StatusCode = 500 };

            // Get billing history with optional date filtering
            var billingHistory = await _billingService.GetPaymentHistoryAsync(subscription.UserId, startDate, endDate, tokenModel);

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
                PaymentHistory = billingHistory.data is IEnumerable<BillingRecordDto> billingData ? billingData.Select(bh => new PaymentHistoryDto
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
                    CreatedDate = bh.CreatedDate,
                    ProcessedAt = bh.PaidAt,
                    PaymentDate = bh.CreatedDate,
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






    public async Task<JsonModel> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId, TokenModel tokenModel)
    {
        try
        {
            // Admin only method - validate admin role
            if (tokenModel.RoleID != (int)RoleId.Admin)
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
            if (tokenModel.RoleID != (int)RoleId.Admin && tokenModel.RoleID != (int)RoleId.Provider)
            {
                return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };
            }

            _logger.LogInformation("Retrieving user subscriptions with database-level filtering - Page: {Page}, PageSize: {PageSize}, SearchTerm: {SearchTerm}",
                page, pageSize, searchTerm);

            // Create filter DTO for database-level filtering
            var filter = new SubscriptionFilterDto
            {
                Page = page,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                Statuses = status?.ToList(),
                PlanIds = planId?.Where(id => Guid.TryParse(id, out _)).Select(Guid.Parse).ToList(),
                UserIds = userId?.Where(id => int.TryParse(id, out _)).Select(int.Parse).ToList(),
                CreatedDateFrom = startDate,
                CreatedDateTo = endDate,
                SortColumn = sortBy ?? "CreatedDate",
                SortOrder = sortOrder ?? "desc"
            };

            // Use database-level filtering, pagination, and sorting
            var (subscriptions, totalCount) = await _subscriptionRepository.GetSubscriptionsWithAdvancedFilteringAsync(filter);
            
            var dtos = _mapper.Map<IEnumerable<SubscriptionDto>>(subscriptions);

            // Return with pagination metadata
            return new JsonModel
            {

                data = dtos,
                meta = new Meta
                {
                    TotalRecords = totalCount,
                    PageSize = pageSize,
                    CurrentPage = page,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                    DefaultPageSize = pageSize

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







    /// <summary>
    /// SRP Refactoring: This method violates Single Responsibility Principle.
    /// Category management should be handled by CategoryService directly, not exposed through SubscriptionService.
    /// This method is kept for backward compatibility but will be removed in a future release.
    /// Please use CategoryService.GetAllCategoriesAsync directly.
    /// </summary>
    [Obsolete("This method violates SRP. Use CategoryService.GetAllCategoriesAsync directly. This will be removed in the next release.")]
    public async Task<JsonModel> GetAllCategoriesAsync(int page, int pageSize, string? searchTerm, bool? isActive, TokenModel tokenModel)
    {
        try
        {
            _logger.LogWarning("DEPRECATED: GetAllCategoriesAsync called from SubscriptionService. Use CategoryService instead.");
            
            // Admin only method - validate admin role
            if (tokenModel.RoleID != (int)RoleId.Admin && tokenModel.RoleID != (int)RoleId.Provider)
            {
                return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };
            }

            // Delegate to CategoryService for actual implementation
            return await _categoryService.GetAllCategoriesAsync(page, pageSize, searchTerm, isActive, tokenModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all categories");
            return new JsonModel { data = new object(), Message = "Failed to retrieve categories", StatusCode = 500 };
        }
    }






    // Example: Booking a consultation using privilege system
    /// <summary>
    /// SRP Refactoring: This method violates Single Responsibility Principle.
    /// Consultation booking is healthcare domain logic and should be in ConsultationService, not SubscriptionService.
    /// This method is kept for backward compatibility but will be removed in a future release.
    /// Please implement consultation booking in ConsultationService.
    /// </summary>
    [Obsolete("This method violates SRP. Consultation booking should be in ConsultationService. This will be removed in the next release.")]
    public async Task<JsonModel> BookConsultationAsync(int userId, Guid subscriptionId, TokenModel tokenModel)
    {
        try
        {
            _logger.LogWarning("DEPRECATED: BookConsultationAsync called from SubscriptionService. This should be in ConsultationService.");
            
            // Validate input parameters
            if (userId <= 0)
            {
                return new JsonModel { data = new object(), Message = "Invalid user ID", StatusCode = 400 };
            }

            if (subscriptionId == Guid.Empty)
            {
                return new JsonModel { data = new object(), Message = "Invalid subscription ID", StatusCode = 400 };
            }

            // Validate token permissions
            if (tokenModel.RoleID != (int)RoleId.Admin && !await HasAccessToSubscription(tokenModel.UserID, subscriptionId.ToString()))
            {
                return new JsonModel { data = new object(), Message = "Access denied", StatusCode = 403 };
            }

            // Check if user has remaining consult privileges
            var remaining = await _privilegeService.GetRemainingPrivilegeAsync(subscriptionId, "Teleconsultation", tokenModel);
            if (remaining <= 0)
                return new JsonModel { data = new object(), Message = "No teleconsultations remaining in your plan.", StatusCode = 400 };

            var used = await _privilegeService.UsePrivilegeAsync(subscriptionId, "Teleconsultation", 1, tokenModel);
            if (!used)
                return new JsonModel { data = new object(), Message = "Failed to use teleconsultation privilege.", StatusCode = 500 };

            // Get subscription details
            var subscription = await _subscriptionRepository.GetByIdWithDetailsAsync(subscriptionId);
            if (subscription == null)
                return new JsonModel { data = new object(), Message = "Subscription not found", StatusCode = 404 };

            // Create consultation booking
            var consultation = new
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                SubscriptionId = subscriptionId,
                ScheduledDate = DateTime.UtcNow.AddHours(1), // Default to 1 hour from now
                Status = "Scheduled",
                CreatedDate = DateTime.UtcNow,
                PlanName = subscription.SubscriptionPlan?.Name ?? "Unknown Plan"
            };

            // TODO: Save consultation to database when consultation entity is available
            // await _consultationRepository.AddAsync(consultation);

            // Send confirmation notification
            var userResult = await _userService.GetUserByIdAsync(userId, tokenModel);
            if (userResult.StatusCode == 200 && userResult.data != null)
            {
                var user = (UserDto)userResult.data;
                
                // Send consultation booking confirmation notification
                
                try
                {
                    // Use the subscription notification service for proper business logic
                    var emailResult = await _subscriptionNotificationService.SendSubscriptionCreatedNotificationAsync(subscriptionId.ToString(), tokenModel);
                    if (emailResult.StatusCode == 200)
                    {
                        _logger.LogInformation("Consultation booking confirmation notification sent to {Email} for user {UserId}", user.Email, userId);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to send consultation booking confirmation notification to {Email} for user {UserId}: {Message}", 
                            user.Email, userId, emailResult.Message);
                    }
                }
                catch (Exception emailEx)
                {
                    _logger.LogError(emailEx, "Failed to send consultation booking confirmation notification to {Email} for user {UserId}", user.Email, userId);
                    // Don't fail the entire operation if email fails
                }
            }

            return new JsonModel { data = consultation, Message = "Consultation booked successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error booking consultation for user {UserId} with subscription {SubscriptionId}", userId, subscriptionId);
            return new JsonModel { data = new object(), Message = "Failed to book consultation", StatusCode = 500 };
        }
    }

    /// <summary>
    /// SRP Refactoring: This method violates Single Responsibility Principle.
    /// Medication supply/pharmacy logic should be in MedicationService, not SubscriptionService.
    /// This method is kept for backward compatibility but will be removed in a future release.
    /// Please implement medication supply requests in MedicationService.
    /// </summary>
    [Obsolete("This method violates SRP. Medication supply should be in MedicationService. This will be removed in the next release.")]
    public async Task<JsonModel> RequestMedicationSupplyAsync(int userId, Guid subscriptionId, TokenModel tokenModel)
    {
        try
        {
            _logger.LogWarning("DEPRECATED: RequestMedicationSupplyAsync called from SubscriptionService. This should be in MedicationService.");
            
            // Validate input parameters
            if (userId <= 0)
            {
                return new JsonModel { data = new object(), Message = "Invalid user ID", StatusCode = 400 };
            }

            if (subscriptionId == Guid.Empty)
            {
                return new JsonModel { data = new object(), Message = "Invalid subscription ID", StatusCode = 400 };
            }

            // Validate token permissions
            if (tokenModel.RoleID != (int)RoleId.Admin && !await HasAccessToSubscription(tokenModel.UserID, subscriptionId.ToString()))
            {
                return new JsonModel { data = new object(), Message = "Access denied", StatusCode = 403 };
            }

            // Check if user has remaining medication supply privileges
            var remaining = await _privilegeService.GetRemainingPrivilegeAsync(subscriptionId, "MedicationSupply", tokenModel);
            if (remaining <= 0)
                return new JsonModel { data = new object(), Message = "No medication supply privilege remaining in your plan.", StatusCode = 400 };

            var used = await _privilegeService.UsePrivilegeAsync(subscriptionId, "MedicationSupply", 1, tokenModel);
            if (!used)
                return new JsonModel { data = new object(), Message = "Failed to use medication supply privilege.", StatusCode = 500 };

            // Get subscription details
            var subscription = await _subscriptionRepository.GetByIdWithDetailsAsync(subscriptionId);
            if (subscription == null)
                return new JsonModel { data = new object(), Message = "Subscription not found", StatusCode = 404 };

            // Create medication supply request
            var medicationRequest = new
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                SubscriptionId = subscriptionId,
                RequestDate = DateTime.UtcNow,
                Status = "Pending",
                Priority = "Normal",
                PlanName = subscription.SubscriptionPlan?.Name ?? "Unknown Plan",
                EstimatedDelivery = DateTime.UtcNow.AddDays(3) // Default 3 days
            };

            // TODO: Save medication request to database when medication entity is available
            // await _medicationRepository.AddAsync(medicationRequest);

            // Send confirmation notification
            var userResult = await _userService.GetUserByIdAsync(userId, tokenModel);
            if (userResult.StatusCode == 200 && userResult.data != null)
            {
                var user = (UserDto)userResult.data;
                
                // Send medication supply request confirmation notification
                
                try
                {
                    // Use the subscription notification service for proper business logic
                    var emailResult = await _subscriptionNotificationService.SendSubscriptionCreatedNotificationAsync(subscriptionId.ToString(), tokenModel);
                    if (emailResult.StatusCode == 200)
                    {
                        _logger.LogInformation("Medication supply request confirmation notification sent to {Email} for user {UserId}", user.Email, userId);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to send medication supply request confirmation notification to {Email} for user {UserId}: {Message}", 
                            user.Email, userId, emailResult.Message);
                    }
                }
                catch (Exception emailEx)
                {
                    _logger.LogError(emailEx, "Failed to send medication supply request confirmation notification to {Email} for user {UserId}", user.Email, userId);
                    // Don't fail the entire operation if email fails
                }
            }

            return new JsonModel { data = medicationRequest, Message = "Medication supply requested successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting medication supply for user {UserId} with subscription {SubscriptionId}", userId, subscriptionId);
            return new JsonModel { data = new object(), Message = "Failed to request medication supply", StatusCode = 500 };
        }
    }

    // --- PAYMENT & BILLING EDGE CASES ---

    // 1. Handle failed payment and update subscription status
    public async Task<JsonModel> HandleFailedPaymentAsync(string subscriptionId, string reason, TokenModel tokenModel)
    {
        try
        {
            // Admin only method - validate admin role
            if (tokenModel.RoleID != (int)RoleId.Admin && tokenModel.RoleID != (int)RoleId.Provider)
            {
                return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };
            }

            var entity = await _subscriptionRepository.GetByIdWithDetailsAsync(Guid.Parse(subscriptionId));
            if (entity == null)
                return new JsonModel { data = new object(), Message = "Subscription not found", StatusCode = 404 };

            // NEW: Handle Stripe payment failure if subscription exists
            if (!string.IsNullOrEmpty(entity.StripeSubscriptionId))
            {
                try
                {
                    _logger.LogInformation("Handling failed payment for Stripe subscription {StripeSubscriptionId} for subscription {SubscriptionId}. Reason: {Reason}", 
                        entity.StripeSubscriptionId, subscriptionId, reason);
                    
                    // For Stripe, failed payments are typically handled automatically through webhooks
                    // But we can log the failure and ensure our local state is consistent
                    // TODO: Implement additional Stripe-specific failed payment handling if needed
                    // This might involve updating Stripe subscription metadata or status
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error handling Stripe payment failure for subscription {SubscriptionId}. Proceeding with local failure handling only.", 
                        subscriptionId);
                    // Don't fail the entire operation if Stripe handling fails
                }
            }
            else
            {
                _logger.LogWarning("Subscription {SubscriptionId} has no Stripe subscription ID. Cannot handle Stripe payment failure.", subscriptionId);
            }
            
            // Update local subscription status
            entity.Status = Subscription.SubscriptionStatuses.PaymentFailed;
            entity.LastPaymentError = reason;
            entity.FailedPaymentAttempts += 1;
            entity.LastPaymentFailedDate = DateTime.UtcNow;
            entity.UpdatedBy = tokenModel.UserID;
            entity.UpdatedDate = DateTime.UtcNow;

            await _subscriptionRepository.UpdateSubscriptionAsync(entity);

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

            return new JsonModel { data = new object(), Message = $"Payment failed with Stripe synchronization: {reason}", StatusCode = 400 };
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
        try
        {
            // Validate input parameters
            if (string.IsNullOrEmpty(subscriptionId) || !Guid.TryParse(subscriptionId, out _))
            {
                return new JsonModel { data = new object(), Message = "Invalid subscription ID format", StatusCode = 400 };
            }

            if (paymentRequest == null)
            {
                return new JsonModel { data = new object(), Message = "Payment request is required", StatusCode = 400 };
            }

            if (paymentRequest.Amount <= 0)
            {
                return new JsonModel { data = new object(), Message = "Payment amount must be greater than zero", StatusCode = 400 };
            }

            // Validate token permissions
            if (tokenModel.RoleID != (int)RoleId.Admin && !await HasAccessToSubscription(tokenModel.UserID, subscriptionId))
            {
                return new JsonModel { data = new object(), Message = "Access denied", StatusCode = 403 };
            }

        var entity = await _subscriptionRepository.GetByIdWithDetailsAsync(Guid.Parse(subscriptionId));
        if (entity == null)
            return new JsonModel { data = new object(), Message = "Subscription not found", StatusCode = 404 };

            // Process payment retry through Stripe with proper subscription reactivation
        PaymentResultDto paymentResult;
        
        if (!string.IsNullOrEmpty(entity.StripeSubscriptionId))
        {
            try
            {
                _logger.LogInformation("Processing payment retry for Stripe subscription {StripeSubscriptionId} for subscription {SubscriptionId}", 
                    entity.StripeSubscriptionId, subscriptionId);
                
                // For Stripe payment retries, we should use the subscription's payment method
                // and process the payment through Stripe
                    var paymentMethodId = !string.IsNullOrEmpty(entity.PaymentMethodId) 
                        ? entity.PaymentMethodId 
                        : await GetDefaultPaymentMethodAsync(entity.UserId, tokenModel);
                    
                    if (string.IsNullOrEmpty(paymentMethodId))
                    {
                        return new JsonModel { data = new object(), Message = "No payment method available for retry", StatusCode = 400 };
                    }
                    
                paymentResult = await _stripeService.ProcessPaymentAsync(
                        paymentMethodId, 
                    paymentRequest.Amount, 
                    "USD", 
                    tokenModel
                );
                
                if (paymentResult.Status == "succeeded")
                {
                    _logger.LogInformation("Successfully processed Stripe payment retry for subscription {SubscriptionId}", subscriptionId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Stripe payment retry for subscription {SubscriptionId}. Falling back to local payment processing.", subscriptionId);
                // Fallback to local payment processing
                paymentResult = await _stripeService.ProcessPaymentAsync(entity.UserId.ToString(), paymentRequest.Amount, "USD", tokenModel);
            }
        }
        else
        {
            // Fallback for subscriptions without Stripe integration
            paymentResult = await _stripeService.ProcessPaymentAsync(entity.UserId.ToString(), paymentRequest.Amount, "USD", tokenModel);
        }
        
        if (paymentResult.Status == "succeeded")
        {
            // Send payment success email
            var userResult = await _userService.GetUserByIdAsync(entity.UserId, tokenModel);
            if (userResult.StatusCode == 200 && userResult.data != null)
            {
                var billingRecord = new BillingRecordDto { Amount = paymentRequest.Amount, PaidDate = DateTime.UtcNow, Description = "Retry Payment" };
                await _notificationService.SendPaymentSuccessEmailAsync(((UserDto)userResult.data).Email, ((UserDto)userResult.data).FullName, billingRecord, tokenModel);
            }
            
            // Reactivate subscription
            entity.Status = Subscription.SubscriptionStatuses.Active;
            entity.UpdatedBy = tokenModel.UserID;
            entity.UpdatedDate = DateTime.UtcNow;
            
            // Add status history for reactivation
            await _subscriptionRepository.AddStatusHistoryAsync(new SubscriptionStatusHistory {
                SubscriptionId = entity.Id,
                FromStatus = Subscription.SubscriptionStatuses.PaymentFailed,
                ToStatus = Subscription.SubscriptionStatuses.Active,
                Reason = "Payment retry successful",
                ChangedAt = DateTime.UtcNow
            });
            
            await _subscriptionRepository.UpdateSubscriptionAsync(entity);
            
            return new JsonModel { data = paymentResult, Message = "Payment retried and subscription reactivated successfully with Stripe synchronization", StatusCode = 200 };
        }
        else
        {
            return new JsonModel { data = new object(), Message = $"Payment retry failed: {paymentResult.ErrorMessage}", StatusCode = 400 };
                }
            }
            catch (Exception ex)
            {
            _logger.LogError(ex, "Error processing payment retry for subscription {SubscriptionId}", subscriptionId);
            return new JsonModel { data = new object(), Message = "Failed to process payment retry", StatusCode = 500 };
        }
    }

    // 3. Auto-renewal logic (to be called by a scheduler/cron job)

    // 4. Prorated upgrades/downgrades

    // --- USAGE & LIMITS ---

    // Check if user can use a privilege (e.g., book a consult)
    public async Task<JsonModel> CanUsePrivilegeAsync(string subscriptionId, string privilegeName, TokenModel tokenModel)
    {
        try
        {
            // Validate token permissions
            if (tokenModel.RoleID != (int)RoleId.Admin && !await HasAccessToSubscription(tokenModel.UserID, subscriptionId))
            {
                return new JsonModel { data = new object(), Message = "Access denied", StatusCode = 403 };
            }

            var subscription = await _subscriptionRepository.GetByIdWithDetailsAsync(Guid.Parse(subscriptionId));
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
        var subscription = await _subscriptionRepository.GetByIdWithDetailsAsync(Guid.Parse(subscriptionId));
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
            await _usageRepo.UpdateUsageAsync(usage);
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
                await _usageRepo.UpdateUsageAsync(usage);
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
                await _usageRepo.UpdateUsageAsync(usage);
            }
        }
    }

    // --- ADMIN OPERATIONS ---

    // Deactivate a plan (admin action)

    // Bulk cancel subscriptions (admin action)

    // Bulk upgrade subscriptions (admin action)


    // Additional methods for comprehensive subscription management

    // Helper method to check if user has access to subscription
    private async Task<bool> HasAccessToSubscription(int userId, string subscriptionId)
    {
        try
        {
            var subscription = await _subscriptionRepository.GetByIdWithDetailsAsync(Guid.Parse(subscriptionId));
            return subscription != null && subscription.UserId == userId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking subscription access for user {UserId}, subscription {SubscriptionId}", userId, subscriptionId);
            return false;
        }
    }

    // Helper method to get default payment method for a user
    private async Task<string?> GetDefaultPaymentMethodAsync(int userId, TokenModel tokenModel)
    {
        try
        {
            var paymentMethods = await _stripeService.GetCustomerPaymentMethodsAsync(userId.ToString(), tokenModel);
            var defaultMethod = paymentMethods.FirstOrDefault(pm => pm.IsDefault);
            return defaultMethod?.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting default payment method for user {UserId}", userId);
            return null;
        }
    }

    // Export methods



    // Helper methods for export generation
    private string GenerateSubscriptionPlansCsv(IEnumerable<SubscriptionPlanDto> plans)
    {
        var csv = new System.Text.StringBuilder();
        csv.AppendLine("Name,Description,Price,BillingCycleId,IsActive,Features,Terms,CreatedDate");
        
        foreach (var plan in plans)
        {
            csv.AppendLine($"\"{plan.Name}\",\"{plan.Description}\",{plan.Price},{plan.BillingCycleId},{plan.IsActive},\"{plan.Features ?? ""}\",\"{plan.Terms ?? ""}\",{plan.CreatedDate:yyyy-MM-dd}");
        }
        
        return csv.ToString();
    }

    private string GenerateSubscriptionPlansExcel(IEnumerable<SubscriptionPlanDto> plans)
    {
        // For now, return CSV format as Excel generation would require additional libraries
        // In a real implementation, you'd use EPPlus or similar library
        return GenerateSubscriptionPlansCsv(plans);
    }

    /// <summary>
    /// Changes the billing cycle of a subscription with Stripe synchronization
    /// </summary>

    public async Task<JsonModel> GetSubscriptionBillingHistoryAsync(string subscriptionId, TokenModel tokenModel)
    {
        try
        {
            if (!Guid.TryParse(subscriptionId, out var subscriptionGuid))
            {
                return new JsonModel { data = new object(), Message = "Invalid subscription ID format", StatusCode = 400 };
            }

            var billingHistory = await _billingService.GetSubscriptionBillingHistoryAsync(subscriptionGuid, tokenModel);
            return billingHistory;
        }
        catch (Exception ex)
        {
            return new JsonModel { data = new object(), Message = $"Error retrieving billing history: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetSubscriptionPrivilegeUsageAsync(string subscriptionId, TokenModel tokenModel)
    {
        try
        {
            if (!Guid.TryParse(subscriptionId, out var subscriptionGuid))
            {
                return new JsonModel { data = new object(), Message = "Invalid subscription ID format", StatusCode = 400 };
            }

            // Get subscription details
            var subscriptionResult = await GetSubscriptionAsync(subscriptionId, tokenModel);
            if (subscriptionResult.StatusCode != 200)
            {
                return subscriptionResult;
            }

            // Get privilege usage data
            var privilegeUsage = await _usageRepo.GetBySubscriptionIdAsync(subscriptionGuid);
            
            var usageData = privilegeUsage.Select(usage => new
            {
                PrivilegeId = usage.SubscriptionPlanPrivilegeId,
                PrivilegeName = usage.SubscriptionPlanPrivilege?.Privilege?.Name ?? "Unknown",
                UsedCount = usage.UsedValue,
                Limit = usage.AllowedValue,
                UsagePeriod = $"{usage.UsagePeriodStart:yyyy-MM-dd} to {usage.UsagePeriodEnd:yyyy-MM-dd}",
                LastUsed = usage.LastUsedAt,
                CreatedAt = usage.CreatedDate
            }).ToList();

            return new JsonModel
            {
                data = usageData,
                Message = "Privilege usage retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            return new JsonModel { data = new object(), Message = $"Error retrieving privilege usage: {ex.Message}", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Retrieves subscription privilege usage with comprehensive filtering, pagination, and sorting
    /// </summary>
    public async Task<JsonModel> GetSubscriptionPrivilegeUsageWithFilteringAsync(
        string subscriptionId, int page, int pageSize, string? search = null, 
        bool? isActive = null, DateTime? startDate = null, DateTime? endDate = null, 
        string? sortBy = "LastUsedAt", string? sortOrder = "desc", TokenModel? tokenModel = null)
    {
        try
        {
            if (!Guid.TryParse(subscriptionId, out var subscriptionGuid))
            {
                return new JsonModel { data = new object(), Message = "Invalid subscription ID format", StatusCode = 400 };
            }

            // Validate access
            if (tokenModel != null)
            {
                var subscriptionResult = await GetSubscriptionAsync(subscriptionId, tokenModel);
                if (subscriptionResult.StatusCode != 200)
                {
                    return subscriptionResult;
                }
            }

            _logger.LogInformation("Retrieving subscription privilege usage with database-level filtering - SubscriptionId: {SubscriptionId}, Page: {Page}, PageSize: {PageSize}", 
                subscriptionId, page, pageSize);

            // Use database-level filtering, pagination, and sorting
            var (usages, totalCount) = await _usageRepo.GetUsagesWithFilteringAsync(
                page, pageSize, subscriptionGuid, null, null, search, isActive, startDate, endDate, sortBy, sortOrder);

            var usageData = usages.Select(usage => new
            {
                Id = usage.Id,
                PrivilegeId = usage.PrivilegeId,
                PrivilegeName = usage.Privilege?.Name ?? "Unknown",
                UsedCount = usage.UsedValue,
                Limit = usage.AllowedValue,
                UsagePeriod = $"{usage.UsagePeriodStart:yyyy-MM-dd} to {usage.UsagePeriodEnd:yyyy-MM-dd}",
                LastUsed = usage.LastUsedAt,
                CreatedAt = usage.CreatedDate,
                IsActive = usage.IsActive
            }).ToList();

            var paginationMeta = new Meta
            {
                TotalRecords = totalCount,
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                HasNextPage = page < (int)Math.Ceiling((double)totalCount / pageSize),
                HasPreviousPage = page > 1
            };

            return new JsonModel
            {
                data = usageData,
                meta = paginationMeta,
                Message = "Privilege usage retrieved successfully with database-level filtering",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving subscription privilege usage with filtering for subscription {SubscriptionId}", subscriptionId);
            return new JsonModel { data = new object(), Message = $"Error retrieving privilege usage: {ex.Message}", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Retrieves subscriptions with comprehensive filtering, pagination, and sorting
    /// </summary>
    public async Task<JsonModel> GetSubscriptionsWithFilteringAsync(SubscriptionFilterDto filter, TokenModel? tokenModel = null, bool adminOnly = false)
    {
        try
        {
            // Validate admin access if required
            if (adminOnly && (tokenModel?.RoleID != (int)RoleId.Admin && tokenModel?.RoleID != (int)RoleId.Provider))
            {
                return new JsonModel { data = new object(), Message = "Access denied - Admin only", StatusCode = 403 };
            }

            // Validate filter parameters
            if (!filter.IsValid())
            {
                var errors = filter.GetValidationErrors();
                return new JsonModel 
                { 
                    data = new object(), 
                    Message = $"Invalid filter parameters: {string.Join(", ", errors)}", 
                    StatusCode = 400 
                };
            }

            _logger.LogInformation("Retrieving subscriptions with advanced filtering - Page: {Page}, PageSize: {PageSize}, SearchTerm: {SearchTerm}, Status: {Status}", 
                filter.Page, filter.PageSize, filter.SearchTerm, filter.Status);

            // Use the advanced repository method with comprehensive filtering
            var (subscriptions, totalCount) = await _subscriptionRepository.GetSubscriptionsWithAdvancedFilteringAsync(filter);

            var subscriptionDtos = _mapper.Map<IEnumerable<SubscriptionDto>>(subscriptions);

            // Create comprehensive pagination metadata
            var paginationMeta = new Meta
            {
                TotalRecords = totalCount,
                PageSize = filter.PageSize,
                CurrentPage = filter.Page,
                TotalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize),
                DefaultPageSize = filter.PageSize,
                HasNextPage = filter.Page < (int)Math.Ceiling((double)totalCount / filter.PageSize),
                HasPreviousPage = filter.Page > 1
            };

            return new JsonModel 
            { 
                data = subscriptionDtos,
                meta = paginationMeta,
                Message = "Subscriptions retrieved successfully with advanced filtering", 
                StatusCode = 200 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving subscriptions with advanced filtering");
            return new JsonModel { data = new object(), Message = "Error retrieving subscriptions", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Allows users to purchase additional privilege credits with immediate upfront payment.
    /// This implements the workflow requirement:
    /// "Once a user has used all their included privileges, any additional usage 
    /// would require upfront payment. Only after this payment would the extra 
    /// privilege be added to their account, allowing them to continue using the service."
    /// </summary>
    /// <param name="subscriptionId">The subscription ID to add credits to</param>
    /// <param name="dto">DTO containing privilege name, quantity, and payment method</param>
    /// <param name="tokenModel">Token containing user authentication information</param>
    /// <returns>JsonModel with purchase details or error information</returns>
    /// <remarks>
    /// Workflow:
    /// 1. Validate subscription is active
    /// 2. Get privilege configuration and calculate cost (quantity × unitCost)
    /// 3. Create billing record for upfront payment
    /// 4. Process payment IMMEDIATELY (not deferred)
    /// 5. If payment succeeds: Add credits to AllowedValue
    /// 6. If payment fails: Return error, no credits added
    /// 7. Send confirmation notification
    /// 
    /// This ensures payment is received BEFORE credits are added to the account.
    /// </remarks>
    public async Task<JsonModel> PurchaseAdditionalCreditsAsync(
        Guid subscriptionId,
        PurchaseAdditionalCreditsDto dto,
        TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation(
                "User {UserId} purchasing {Quantity} {PrivilegeName} credits for subscription {SubscriptionId}",
                tokenModel.UserID, dto.Quantity, dto.PrivilegeName, subscriptionId
            );

            // STEP 1: Get and validate subscription
            var subscription = await _subscriptionRepository.GetByIdWithDetailsAsync(subscriptionId);
            
            if (subscription == null)
            {
                return new JsonModel 
                { 
                    data = new object(),
                    Message = "Subscription not found",
                    StatusCode = 404
                };
            }

            // STEP 2: Validate subscription is active
            if (subscription.Status != Subscription.SubscriptionStatuses.Active &&
                subscription.Status != Subscription.SubscriptionStatuses.TrialActive)
            {
                return new JsonModel 
                { 
                    data = new object(),
                    Message = $"Cannot purchase credits. Subscription status is {subscription.Status}. Only active subscriptions can purchase additional credits.",
                    StatusCode = 400
                };
            }

            // STEP 3: Validate user access (must be subscription owner or admin)
            if (tokenModel.RoleID != (int)RoleId.Admin && 
                tokenModel.UserID != subscription.UserId)
            {
                return new JsonModel 
                { 
                    data = new object(),
                    Message = "Access denied. You can only purchase credits for your own subscription.",
                    StatusCode = 403
                };
            }

            // STEP 4: Get plan privilege configuration
            var planPrivileges = await _planPrivilegeRepo.GetByPlanIdAsync(subscription.SubscriptionPlanId);
            
            var planPrivilege = planPrivileges
                .FirstOrDefault(pp => pp.Privilege != null && pp.Privilege.Name == dto.PrivilegeName);
            
            if (planPrivilege == null)
            {
                return new JsonModel 
                { 
                    data = new object(),
                    Message = $"Privilege '{dto.PrivilegeName}' not found in your subscription plan",
                    StatusCode = 404
                };
            }

            // STEP 5: Check if privilege is disabled in the plan
            if (planPrivilege.IsDisabled)
            {
                return new JsonModel 
                { 
                    data = new object(),
                    Message = $"Privilege '{dto.PrivilegeName}' is not available in your plan",
                    StatusCode = 400
                };
            }

            // STEP 6: Get current privilege usage
            var usages = await _usageRepo.GetBySubscriptionIdAsync(subscriptionId);
            
            var usage = usages
                .FirstOrDefault(u => u.SubscriptionPlanPrivilegeId == planPrivilege.Id);
            
            if (usage == null)
            {
                return new JsonModel 
                { 
                    data = new object(),
                    Message = "Privilege usage record not found. Please contact support.",
                    StatusCode = 500
                };
            }

            // STEP 7: Calculate cost (quantity × unit cost)
            decimal totalCost = dto.Quantity * planPrivilege.UnitCost;

            if (totalCost <= 0)
            {
                return new JsonModel 
                { 
                    data = new object(),
                    Message = "Invalid cost calculated. Unit cost must be greater than zero.",
                    StatusCode = 400
                };
            }

            _logger.LogInformation(
                "Calculated cost: {Quantity} credits × ${UnitCost} = ${TotalCost}",
                dto.Quantity, planPrivilege.UnitCost, totalCost
            );

            // STEP 8: Validate payment method
            var isValidPaymentMethod = await _stripeService.ValidatePaymentMethodAsync(dto.PaymentMethodId, tokenModel);
            if (!isValidPaymentMethod)
            {
                return new JsonModel 
                { 
                    data = new object(),
                    Message = "Invalid payment method. Please add a valid payment method.",
                    StatusCode = 400
                };
            }

            // STEP 9: BEGIN TRANSACTION for data consistency
            await _unitOfWork.BeginTransactionAsync();
            
            try
            {
                // STEP 10: Create billing record for upfront payment
                var billingRecord = new BillingRecord
                {
                    Id = Guid.NewGuid(),
                    UserId = subscription.UserId,
                    SubscriptionId = subscription.Id,
                    CurrencyId = subscription.SubscriptionPlan.CurrencyId,
                    Amount = totalCost,
                    TaxAmount = 0,
                    ShippingAmount = 0,
                    TotalAmount = totalCost,
                    Status = BillingRecord.BillingStatus.Pending,
                    Type = BillingRecord.BillingType.Overage,
                    Description = $"Purchase {dto.Quantity} additional {dto.PrivilegeName} credits @ ${planPrivilege.UnitCost} each",
                    BillingDate = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow, // Due immediately for upfront payment
                    IsRecurring = false,
                    PaymentMethod = dto.PaymentMethodId,
                    IsActive = true,
                    CreatedBy = tokenModel.UserID,
                    CreatedDate = DateTime.UtcNow
                };

                var createdBilling = await _billingService.CreateBillingRecordAsync(
                    _mapper.Map<CreateBillingRecordDto>(billingRecord),
                    tokenModel
                );

                if (createdBilling.StatusCode != 200 || createdBilling.data == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return new JsonModel 
                    { 
                        data = new object(),
                        Message = "Failed to create billing record",
                        StatusCode = 500
                    };
                }

                var billingRecordDto = (BillingRecordDto)createdBilling.data;

                _logger.LogInformation(
                    "Created billing record {BillingRecordId} for ${Amount}",
                    billingRecordDto.Id, totalCost
                );

                // STEP 11: PROCESS UPFRONT PAYMENT IMMEDIATELY (CRITICAL!)
                // This is where we enforce "payment before credits" requirement
                var billingRecordId = Guid.Parse(billingRecordDto.Id);
                var paymentResult = await _billingService.ProcessPaymentAsync(
                    billingRecordId,
                    tokenModel
                );

                // STEP 12: Check if payment succeeded
                if (paymentResult.StatusCode != 200)
                {
                    // PAYMENT FAILED - Rollback entire transaction
                    await _unitOfWork.RollbackTransactionAsync();
                    
                    _logger.LogWarning(
                        "Payment failed for billing record {BillingRecordId}: {Message}. Credits NOT added.",
                        billingRecordDto.Id, paymentResult.Message
                    );

                    return new JsonModel
                    {
                        data = new
                        {
                            paymentFailed = true,
                            reason = paymentResult.Message,
                            creditsAdded = 0,
                            amountCharged = 0
                        },
                        Message = $"Payment failed: {paymentResult.Message}. Additional credits were not added to your account.",
                        StatusCode = 400
                    };
                }

                // STEP 13: PAYMENT SUCCESSFUL - Add credits to AllowedValue
                // This is the KEY operation that adds credits after successful payment
                var previousAllowedValue = usage.AllowedValue;
                var previousRemaining = usage.RemainingValue;

                usage.AllowedValue += dto.Quantity; // ADD CREDITS HERE!
                usage.UpdatedBy = tokenModel.UserID;
                usage.UpdatedDate = DateTime.UtcNow;

                await _usageRepo.UpdateAsync(usage);

                _logger.LogInformation(
                    "✓ Payment successful! Updated AllowedValue from {PreviousValue} to {NewValue} for privilege {PrivilegeName}",
                    previousAllowedValue, usage.AllowedValue, dto.PrivilegeName
                );

                // STEP 14: COMMIT TRANSACTION
                // Only commit if payment successful and credits added
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation(
                    "✓ Transaction committed. Successfully purchased {Quantity} {PrivilegeName} credits for user {UserId}",
                    dto.Quantity, dto.PrivilegeName, tokenModel.UserID
                );

                // STEP 15: Send confirmation notification
                try
                {
                    await _subscriptionNotificationService.SendBulkNotificationAsync(
                        new List<string> { subscriptionId.ToString() },
                        "Additional Credits Purchased",
                        $"You've successfully purchased {dto.Quantity} additional {dto.PrivilegeName} credits for ${totalCost}. " +
                        $"Your new limit is {usage.AllowedValue} (previously {previousAllowedValue}). " +
                        $"You have {usage.RemainingValue} credits remaining.",
                        "credits_purchased",
                        tokenModel
                    );
                }
                catch (Exception notificationEx)
                {
                    // Don't fail the entire operation if notification fails
                    _logger.LogWarning(notificationEx, 
                        "Failed to send notification for credit purchase, but purchase was successful");
                }

                // STEP 16: Return success response with complete details
                return new JsonModel
                {
                    data = new PurchaseCreditsResponseDto
                    {
                        SubscriptionId = subscriptionId,
                        PrivilegeName = dto.PrivilegeName,
                        CreditsAdded = dto.Quantity,
                        UnitCost = planPrivilege.UnitCost,
                        TotalPaid = totalCost,
                        PreviousLimit = previousAllowedValue,
                        NewLimit = usage.AllowedValue,
                        CurrentUsed = usage.UsedValue,
                        NewRemaining = usage.RemainingValue,
                        BillingRecordId = billingRecordId,
                        PurchasedAt = DateTime.UtcNow
                    },
                    Message = $"Successfully purchased {dto.Quantity} additional {dto.PrivilegeName} credits for ${totalCost}. Your new limit is {usage.AllowedValue}.",
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                // ROLLBACK on any error
                await _unitOfWork.RollbackTransactionAsync();
                
                _logger.LogError(ex, 
                    "Error in transaction while purchasing credits for subscription {SubscriptionId}. Transaction rolled back.",
                    subscriptionId
                );
                
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error purchasing {Quantity} {PrivilegeName} credits for subscription {SubscriptionId}",
                dto.Quantity, dto.PrivilegeName, subscriptionId
            );

            return new JsonModel
            {
                data = new object(),
                Message = "Error processing credit purchase. Please try again or contact support.",
                StatusCode = 500
            };
        }
    }

}