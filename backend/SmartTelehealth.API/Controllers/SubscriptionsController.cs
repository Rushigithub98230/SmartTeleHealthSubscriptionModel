using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;

namespace SmartTelehealth.API.Controllers;

/// <summary>
/// Controller responsible for managing user subscriptions and subscription plans.
/// This controller provides comprehensive subscription management functionality including
/// creating, updating, canceling, pausing, and resuming subscriptions, as well as managing
/// subscription plans and handling payment-related operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class SubscriptionsController : BaseController
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly ISubscriptionLifecycleService _subscriptionLifecycleService;
    

    /// <summary>
    /// Initializes a new instance of the SubscriptionsController with the required services.
    /// </summary>
    /// <param name="subscriptionService">Service for handling subscription-related business logic</param>
    /// <param name="subscriptionLifecycleService">Service for handling subscription lifecycle operations</param>
    /// <param name="subscriptionPlanService">Service for handling subscription plan-related business logic</param>
    public SubscriptionsController(ISubscriptionService subscriptionService, ISubscriptionLifecycleService subscriptionLifecycleService, ISubscriptionPlanService subscriptionPlanService)
    {
        _subscriptionService = subscriptionService;
        _subscriptionLifecycleService = subscriptionLifecycleService;
       
    }

    /// <summary>
    /// Retrieves a specific subscription by its unique identifier.
    /// This endpoint allows users to get detailed information about a particular subscription,
    /// including its status, plan details, billing information, and usage statistics.
    /// </summary>
    /// <param name="id">The unique identifier (GUID) of the subscription to retrieve</param>
    /// <returns>JsonModel containing the subscription details or error information</returns>
    /// <remarks>
    /// Access Control:
    /// - Admins can access any subscription
    /// - Regular users can only access their own subscriptions
    /// - Returns 403 Forbidden if user doesn't have access
    /// - Returns 404 Not Found if subscription doesn't exist
    /// </remarks>
    [HttpGet("{id}")]
    public async Task<JsonModel> GetSubscription(string id)
    {
        return await _subscriptionService.GetSubscriptionAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Retrieves all subscriptions for a specific user.
    /// This endpoint returns a list of all subscriptions (active, paused, cancelled, etc.)
    /// associated with the specified user, including their current status and plan information.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose subscriptions to retrieve</param>
    /// <returns>JsonModel containing the list of user subscriptions or error information</returns>
    /// <remarks>
    /// Access Control:
    /// - Admins can access any user's subscriptions
    /// - Regular users can only access their own subscriptions
    /// - Returns 403 Forbidden if user doesn't have access
    /// - Returns all subscription statuses (active, paused, cancelled, expired, etc.)
    /// </remarks>
    [HttpGet("user/{userId}")]
    public async Task<JsonModel> GetUserSubscriptions(int userId)
    {
        return await _subscriptionService.GetUserSubscriptionsAsync(userId, GetToken(HttpContext));
    }

    /// <summary>
    /// Creates a new subscription for a user.
    /// This endpoint handles the complete subscription creation process including Stripe integration,
    /// payment method validation, trial setup (if applicable), and initial privilege allocation.
    /// </summary>
    /// <param name="createDto">DTO containing subscription creation details (userId, planId, billingCycleId, paymentMethodId)</param>
    /// <returns>JsonModel containing the created subscription details or error information</returns>
    /// <remarks>
    /// This endpoint performs the following operations:
    /// 1. Validates subscription plan exists and is active
    /// 2. Prevents duplicate active/paused subscriptions for the same user and plan
    /// 3. Creates Stripe customer if not exists
    /// 4. Creates Stripe subscription with proper billing cycle
    /// 5. Handles trial subscriptions if plan allows trials
    /// 6. Sets up billing dates and audit fields
    /// 7. Creates initial privilege usage records
    /// 8. Sends welcome notifications
    /// </remarks>
    [HttpPost]
    public async Task<JsonModel> CreateSubscription([FromBody] CreateSubscriptionDto createDto)
    {
        return await _subscriptionLifecycleService.CreateSubscriptionAsync(createDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Cancels an existing subscription.
    /// This endpoint cancels a subscription immediately or at the end of the current billing period,
    /// updates the subscription status, and handles Stripe synchronization.
    /// </summary>
    /// <param name="id">The unique identifier of the subscription to cancel</param>
    /// <param name="reason">Optional reason for cancellation (for audit and analytics purposes)</param>
    /// <returns>JsonModel containing the cancellation result or error information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Validates the subscription exists and can be cancelled
    /// - Updates subscription status to "Cancelled"
    /// - Cancels the corresponding Stripe subscription
    /// - Records the cancellation reason and timestamp
    /// - Sends cancellation notification to the user
    /// - Prevents further billing for the subscription
    /// </remarks>
    [HttpPost("{id}/cancel")]
    public async Task<JsonModel> CancelSubscription(string id, [FromBody] string reason)
    {
        return await _subscriptionLifecycleService.CancelSubscriptionAsync(id, reason, GetToken(HttpContext));
    }

    /// <summary>
    /// Pauses an active subscription temporarily.
    /// This endpoint allows users to temporarily pause their subscription, which stops billing
    /// but preserves the subscription for future reactivation.
    /// </summary>
    /// <param name="id">The unique identifier of the subscription to pause</param>
    /// <returns>JsonModel containing the pause result or error information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Validates the subscription is active and can be paused
    /// - Updates subscription status to "Paused"
    /// - Pauses the corresponding Stripe subscription
    /// - Records the pause date and reason
    /// - Stops billing while preserving subscription data
    /// - Allows reactivation at any time
    /// </remarks>
    [HttpPost("{id}/pause")]
    public async Task<JsonModel> PauseSubscription(string id)
    {
        return await _subscriptionLifecycleService.PauseSubscriptionAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Resumes a paused subscription.
    /// This endpoint reactivates a previously paused subscription, restoring billing
    /// and access to subscription benefits.
    /// </summary>
    /// <param name="id">The unique identifier of the subscription to resume</param>
    /// <returns>JsonModel containing the resume result or error information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Validates the subscription is paused and can be resumed
    /// - Updates subscription status to "Active"
    /// - Resumes the corresponding Stripe subscription
    /// - Records the resume date
    /// - Restores billing and access to subscription benefits
    /// - Calculates new billing dates based on pause duration
    /// </remarks>
    [HttpPost("{id}/resume")]
    public async Task<JsonModel> ResumeSubscription(string id)
    {
        return await _subscriptionLifecycleService.ResumeSubscriptionAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Upgrades a subscription to a different plan.
    /// This endpoint handles plan upgrades, including prorated billing, privilege changes,
    /// and Stripe subscription updates.
    /// </summary>
    /// <param name="id">The unique identifier of the subscription to upgrade</param>
    /// <param name="newPlanId">The unique identifier of the new subscription plan</param>
    /// <returns>JsonModel containing the upgrade result or error information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Validates both current and new subscription plans exist
    /// - Calculates prorated billing for the upgrade
    /// - Updates Stripe subscription with new plan
    /// - Adjusts privilege limits based on new plan
    /// - Handles billing adjustments and refunds if applicable
    /// - Sends upgrade confirmation notification
    /// </remarks>
    [HttpPost("{id}/upgrade")]
    public async Task<JsonModel> UpgradeSubscription(string id, [FromBody] string newPlanId)
    {
        return await _subscriptionLifecycleService.UpgradeSubscriptionAsync(id, newPlanId, GetToken(HttpContext));
    }

    /// <summary>
    /// Reactivates a cancelled or expired subscription.
    /// This endpoint allows users to reactivate their subscription after cancellation
    /// or expiration, subject to business rules and plan availability.
    /// </summary>
    /// <param name="id">The unique identifier of the subscription to reactivate</param>
    /// <returns>JsonModel containing the reactivation result or error information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Validates the subscription can be reactivated
    /// - Checks if the original plan is still available
    /// - Creates a new Stripe subscription if needed
    /// - Updates subscription status to "Active"
    /// - Restores access to subscription benefits
    /// - Sends reactivation confirmation notification
    /// </remarks>
    [HttpPost("{id}/reactivate")]
    public async Task<JsonModel> ReactivateSubscription(string id)
    {
        return await _subscriptionLifecycleService.ReactivateSubscriptionAsync(id, GetToken(HttpContext));
    }



    /// <summary>
    /// Retrieves the billing history for a specific subscription.
    /// This endpoint returns a chronological list of all billing records, payments,
    /// and financial transactions associated with the subscription.
    /// </summary>
    /// <param name="id">The unique identifier of the subscription</param>
    /// <returns>JsonModel containing the billing history or error information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns all billing records for the subscription
    /// - Includes payment status, amounts, dates, and transaction IDs
    /// - Shows failed payments, refunds, and adjustments
    /// - Provides financial audit trail for the subscription
    /// - Access restricted to subscription owner or admins
    /// </remarks>
    [HttpGet("{id}/billing-history")]
    public async Task<JsonModel> GetBillingHistory(string id)
    {
        return await _subscriptionService.GetBillingHistoryAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Retrieves all payment methods associated with a user.
    /// This endpoint returns a list of all payment methods (credit cards, bank accounts, etc.)
    /// that the user has added to their account for subscription billing.
    /// </summary>
    /// <param name="userId">The unique identifier of the user</param>
    /// <returns>JsonModel containing the list of payment methods or error information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns all payment methods associated with the user
    /// - Includes payment method details (masked card numbers, expiry dates)
    /// - Shows which payment method is set as default
    /// - Access restricted to the user themselves or admins
    /// - Used for payment method management in the frontend
    /// </remarks>
    [HttpGet("user/{userId}/payment-methods")]
    public async Task<JsonModel> GetPaymentMethods(int userId)
    {
        return await _subscriptionService.GetPaymentMethodsAsync(userId, GetToken(HttpContext));
    }

    /// <summary>
    /// Adds a new payment method to a user's account.
    /// This endpoint allows users to add additional payment methods (credit cards, bank accounts)
    /// to their account for subscription billing and payment processing.
    /// </summary>
    /// <param name="userId">The unique identifier of the user</param>
    /// <param name="paymentMethodId">The Stripe payment method ID to add</param>
    /// <returns>JsonModel containing the result of adding the payment method</returns>
    /// <remarks>
    /// This endpoint:
    /// - Validates the payment method with Stripe
    /// - Associates the payment method with the user's account
    /// - Sets up the payment method for future billing
    /// - Access restricted to the user themselves or admins
    /// - Used when users want to add backup payment methods
    /// </remarks>
    [HttpPost("user/{userId}/payment-methods")]
    public async Task<JsonModel> AddPaymentMethod(int userId, [FromBody] string paymentMethodId)
    {
        return await _subscriptionService.AddPaymentMethodAsync(userId, paymentMethodId, GetToken(HttpContext));
    }

    /// <summary>
    /// Retrieves subscriptions associated with a specific plan.
    /// This endpoint returns all subscriptions that are currently using the specified
    /// subscription plan, useful for plan analytics and management.
    /// </summary>
    /// <param name="planId">The unique identifier of the subscription plan</param>
    /// <returns>JsonModel containing the list of subscriptions using the plan</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns all subscriptions currently using the specified plan
    /// - Includes subscription status and user information
    /// - Used for plan analytics and usage tracking
    /// - Access restricted to admins only
    /// - Helps understand plan popularity and usage patterns
    /// </remarks>
    [HttpGet("plan/{planId}")]
    public async Task<JsonModel> GetSubscriptionByPlanId(string planId)
    {
        return await _subscriptionService.GetSubscriptionByPlanIdAsync(planId, GetToken(HttpContext));
    }

    /// <summary>
    /// Retrieves all currently active subscriptions.
    /// This endpoint returns a list of all subscriptions that are currently in "Active"
    /// status, providing an overview of active subscriptions in the system.
    /// </summary>
    /// <returns>JsonModel containing the list of active subscriptions</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns all subscriptions with "Active" status
    /// - Includes subscription details and user information
    /// - Used for system monitoring and analytics
    /// - Access restricted to admins only
    /// - Provides real-time view of active subscriptions
    /// </remarks>
    [HttpGet("active")]
    public async Task<JsonModel> GetActiveSubscriptions()
    {
        return await _subscriptionService.GetActiveSubscriptionsAsync(GetToken(HttpContext));
    }

    /// <summary>
    /// Updates an existing subscription with new information.
    /// This endpoint allows modification of subscription details such as billing cycle,
    /// payment method, or other subscription properties.
    /// </summary>
    /// <param name="id">The unique identifier of the subscription to update</param>
    /// <param name="updateDto">DTO containing the updated subscription information</param>
    /// <returns>JsonModel containing the update result or error information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Validates the subscription exists and can be updated
    /// - Updates subscription properties as specified
    /// - Synchronizes changes with Stripe if applicable
    /// - Records audit trail of changes
    /// - Access restricted to subscription owner or admins
    /// - Used for subscription management and modifications
    /// </remarks>
    [HttpPut("{id}")]
    public async Task<JsonModel> UpdateSubscription(string id, [FromBody] UpdateSubscriptionDto updateDto)
    {
        return await _subscriptionLifecycleService.UpdateSubscriptionAsync(id, updateDto, GetToken(HttpContext));
    }

    /// <summary>
    /// Processes a payment for a specific subscription.
    /// This endpoint handles manual payment processing for subscriptions, including
    /// one-time payments, payment retries, and payment method updates.
    /// </summary>
    /// <param name="id">The unique identifier of the subscription</param>
    /// <param name="paymentRequest">DTO containing payment processing details</param>
    /// <returns>JsonModel containing the payment processing result</returns>
    /// <remarks>
    /// This endpoint:
    /// - Processes payments through Stripe
    /// - Handles payment failures and retries
    /// - Updates subscription status based on payment result
    /// - Records payment history and audit trail
    /// - Sends payment confirmation notifications
    /// - Access restricted to subscription owner or admins
    /// </remarks>
    [HttpPost("{id}/process-payment")]
    public async Task<JsonModel> ProcessPayment(string id, [FromBody] PaymentRequestDto paymentRequest)
    {
        return await _subscriptionService.ProcessPaymentAsync(id, paymentRequest, GetToken(HttpContext));
    }

    /// <summary>
    /// Retrieves usage statistics for a specific subscription.
    /// This endpoint returns detailed usage analytics including privilege consumption,
    /// usage patterns, and remaining limits for the subscription.
    /// </summary>
    /// <param name="id">The unique identifier of the subscription</param>
    /// <returns>JsonModel containing usage statistics and analytics</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns privilege usage statistics
    /// - Shows usage patterns over time
    /// - Displays remaining limits and quotas
    /// - Provides usage analytics and insights
    /// - Access restricted to subscription owner or admins
    /// - Used for usage monitoring and optimization
    /// </remarks>
    [HttpGet("{id}/usage-statistics")]
    public async Task<JsonModel> GetUsageStatistics(string id)
    {
        return await _subscriptionService.GetUsageStatisticsAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Retrieves all subscriptions in the system (admin only).
    /// This endpoint returns a comprehensive list of all subscriptions across all users,
    /// providing system-wide subscription management capabilities.
    /// </summary>
    /// <returns>JsonModel containing all subscriptions in the system</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns all subscriptions regardless of status
    /// - Includes user and plan information
    /// - Provides system-wide subscription overview
    /// - Access restricted to admins only
    /// - Used for administrative management and reporting
    /// - Supports pagination for large datasets
    /// </remarks>
    [HttpGet]
    public async Task<JsonModel> GetAllSubscriptions()
    {
        return await _subscriptionService.GetAllSubscriptionsAsync(GetToken(HttpContext));
    }

    /// <summary>
    /// Retrieves detailed analytics for a specific subscription.
    /// This endpoint provides comprehensive analytics including revenue, usage patterns,
    /// churn risk, and performance metrics for the subscription.
    /// </summary>
    /// <param name="id">The unique identifier of the subscription</param>
    /// <returns>JsonModel containing detailed subscription analytics</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns comprehensive subscription analytics
    /// - Includes revenue and billing analytics
    /// - Shows usage patterns and trends
    /// - Provides churn risk assessment
    /// - Access restricted to subscription owner or admins
    /// - Used for subscription optimization and insights
    /// </remarks>
    [HttpGet("{id}/analytics")]
    public async Task<JsonModel> GetSubscriptionAnalytics(string id)
    {
        return await _subscriptionService.GetSubscriptionAnalyticsAsync(id, null, null, GetToken(HttpContext));
    }

    /// <summary>
    /// Creates a new subscription plan for the system.
    /// This endpoint handles subscription plan creation including validation, configuration,
    /// and initial setup for administrative management and user subscription options.
    /// </summary>
    /// <param name="createPlanDto">DTO containing subscription plan creation details</param>
    /// <returns>JsonModel containing the created subscription plan information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Creates a new subscription plan with comprehensive validation
    /// - Validates plan configuration and pricing information
    /// - Sets up plan for administrative management
    /// - Access restricted to administrators only
    /// - Used for subscription plan creation and management
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on plan creation
    /// - Maintains plan creation audit trails and history
    /// </remarks>

    /// <summary>
    /// Updates an existing subscription plan with new configuration.
    /// This endpoint handles subscription plan updates including validation, configuration changes,
    /// and plan modification for administrative management and user subscription options.
    /// </summary>
    /// <param name="planId">The unique identifier of the subscription plan to update</param>
    /// <param name="updatePlanDto">DTO containing subscription plan update details</param>
    /// <returns>JsonModel containing the updated subscription plan information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Updates an existing subscription plan with comprehensive validation
    /// - Validates plan configuration and pricing changes
    /// - Ensures plan modification consistency and integrity
    /// - Access restricted to administrators only
    /// - Used for subscription plan modification and management
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on plan updates
    /// - Maintains plan update audit trails and change history
    /// </remarks>

    /// <summary>
    /// Activates a subscription plan to make it available for user subscriptions.
    /// This endpoint handles subscription plan activation including validation, status updates,
    /// and plan availability for user subscription options.
    /// </summary>
    /// <param name="planId">The unique identifier of the subscription plan to activate</param>
    /// <returns>JsonModel containing the activation result</returns>
    /// <remarks>
    /// This endpoint:
    /// - Activates a subscription plan for user availability
    /// - Validates plan configuration and readiness
    /// - Updates plan status to active
    /// - Access restricted to administrators only
    /// - Used for subscription plan activation and management
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on plan activation
    /// - Maintains plan activation audit trails and status history
    /// </remarks>

    /// <summary>
    /// Deactivates a subscription plan to prevent new user subscriptions.
    /// This endpoint handles subscription plan deactivation including validation, status updates,
    /// and plan unavailability for new user subscription options.
    /// </summary>
    /// <param name="planId">The unique identifier of the subscription plan to deactivate</param>
    /// <returns>JsonModel containing the deactivation result</returns>
    /// <remarks>
    /// This endpoint:
    /// - Deactivates a subscription plan to prevent new subscriptions
    /// - Validates plan status and existing subscriptions
    /// - Updates plan status to inactive
    /// - Access restricted to administrators only
    /// - Used for subscription plan deactivation and management
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on plan deactivation
    /// - Maintains plan deactivation audit trails and status history
    /// </remarks>

    /// <summary>
    /// Deletes a subscription plan from the system.
    /// This endpoint handles subscription plan deletion including validation, dependency checking,
    /// and plan removal for administrative management and system cleanup.
    /// </summary>
    /// <param name="planId">The unique identifier of the subscription plan to delete</param>
    /// <returns>JsonModel containing the deletion result</returns>
    /// <remarks>
    /// This endpoint:
    /// - Deletes a subscription plan with comprehensive validation
    /// - Checks for existing subscriptions and dependencies
    /// - Ensures safe plan removal without data integrity issues
    /// - Access restricted to administrators only
    /// - Used for subscription plan removal and system cleanup
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on plan deletion
    /// - Maintains plan deletion audit trails and removal history
    /// </remarks>

    /// <summary>
    /// Retrieves a subscription by its Stripe subscription ID.
    /// This endpoint allows lookup of local subscription records using the Stripe subscription ID,
    /// useful for webhook processing and Stripe integration synchronization.
    /// </summary>
    /// <param name="stripeSubscriptionId">The Stripe subscription ID to search for</param>
    /// <returns>JsonModel containing the subscription details or error information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Looks up subscription by Stripe subscription ID
    /// - Returns local subscription record details
    /// - Used for Stripe webhook processing and synchronization
    /// - Access restricted to administrators and system processes
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on subscription lookup
    /// - Maintains subscription lookup audit trails
    /// </remarks>
    [HttpGet("stripe/{stripeSubscriptionId}")]
    public async Task<JsonModel> GetByStripeSubscriptionId(string stripeSubscriptionId)
    {
        return await _subscriptionService.GetByStripeSubscriptionIdAsync(stripeSubscriptionId, GetToken(HttpContext));
    }

    /// <summary>
    /// Retrieves all user subscriptions with comprehensive filtering and pagination for administrative management.
    /// This endpoint provides administrators with access to all user subscriptions in the system with advanced filtering
    /// capabilities including status, plan, user, date range, and sorting options for effective subscription oversight.
    /// </summary>
    /// <param name="page">Page number for pagination (default: 1)</param>
    /// <param name="pageSize">Number of records per page (default: 10)</param>
    /// <param name="searchTerm">Search term for filtering subscriptions</param>
    /// <param name="status">Filter subscriptions by status array</param>
    /// <param name="planId">Filter subscriptions by plan ID array</param>
    /// <param name="userId">Filter subscriptions by user ID array</param>
    /// <param name="startDate">Start date for date range filtering</param>
    /// <param name="endDate">End date for date range filtering</param>
    /// <param name="sortBy">Field to sort by</param>
    /// <param name="sortOrder">Sort order (asc/desc)</param>
    /// <returns>JsonModel containing paginated user subscriptions with filtering applied</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns all user subscriptions with comprehensive filtering options
    /// - Supports pagination for large datasets
    /// - Includes advanced filtering by status, plan, user, and date range
    /// - Provides sorting capabilities for data organization
    /// - Access restricted to administrators only
    /// - Used for administrative subscription oversight and management
    /// - Includes comprehensive subscription information and metadata
    /// - Supports advanced filtering for subscription analysis
    /// - Provides comprehensive subscription overview for administrators
    /// </remarks>
    [HttpGet("admin/user-subscriptions")]
    public async Task<JsonModel> GetAllUserSubscriptions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string[]? status = null,
        [FromQuery] string[]? planId = null,
        [FromQuery] string[]? userId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortOrder = null)
    {
        return await _subscriptionService.GetAllUserSubscriptionsAsync(page, pageSize, searchTerm, status, planId, userId, startDate, endDate, sortBy, sortOrder, GetToken(HttpContext));
    }

    /// <summary>
    /// Cancels a user subscription with optional reason for administrative management.
    /// This endpoint handles subscription cancellation including validation, status updates,
    /// and cancellation processing for administrative subscription management.
    /// </summary>
    /// <param name="id">The unique identifier of the subscription to cancel</param>
    /// <param name="reason">Optional reason for cancellation</param>
    /// <returns>JsonModel containing the cancellation result</returns>
    /// <remarks>
    /// This endpoint:
    /// - Cancels a user subscription with administrative authority
    /// - Validates subscription status and cancellation eligibility
    /// - Records cancellation reason for audit purposes
    /// - Access restricted to administrators only
    /// - Used for administrative subscription cancellation and management
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on cancellation operations
    /// - Maintains subscription cancellation audit trails
    /// </remarks>
    [HttpPost("admin/{id}/cancel")]
    public async Task<JsonModel> CancelUserSubscription(string id, [FromBody] string? reason)
    {
        return await _subscriptionLifecycleService.CancelSubscriptionAsync(id, reason, GetToken(HttpContext));
    }

    /// <summary>
    /// Pauses a user subscription for administrative management.
    /// This endpoint handles subscription pausing including validation, status updates,
    /// and pause processing for administrative subscription management.
    /// </summary>
    /// <param name="id">The unique identifier of the subscription to pause</param>
    /// <returns>JsonModel containing the pause result</returns>
    /// <remarks>
    /// This endpoint:
    /// - Pauses a user subscription with administrative authority
    /// - Validates subscription status and pause eligibility
    /// - Updates subscription status to paused
    /// - Access restricted to administrators only
    /// - Used for administrative subscription pause and management
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on pause operations
    /// - Maintains subscription pause audit trails
    /// </remarks>
    [HttpPost("admin/{id}/pause")]
    public async Task<JsonModel> PauseUserSubscription(string id)
    {
        return await _subscriptionLifecycleService.PauseSubscriptionAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Resumes a paused user subscription for administrative management.
    /// This endpoint handles subscription resumption including validation, status updates,
    /// and resume processing for administrative subscription management.
    /// </summary>
    /// <param name="id">The unique identifier of the subscription to resume</param>
    /// <returns>JsonModel containing the resume result</returns>
    /// <remarks>
    /// This endpoint:
    /// - Resumes a paused user subscription with administrative authority
    /// - Validates subscription status and resume eligibility
    /// - Updates subscription status to active
    /// - Access restricted to administrators only
    /// - Used for administrative subscription resume and management
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on resume operations
    /// - Maintains subscription resume audit trails
    /// </remarks>
    [HttpPost("admin/{id}/resume")]
    public async Task<JsonModel> ResumeUserSubscription(string id)
    {
        return await _subscriptionLifecycleService.ResumeSubscriptionAsync(id, GetToken(HttpContext));
    }

    /// <summary>
    /// Extends a user subscription duration for administrative management.
    /// This endpoint handles subscription extension including validation, duration calculation,
    /// and extension processing for administrative subscription management.
    /// </summary>
    /// <param name="id">The unique identifier of the subscription to extend</param>
    /// <param name="additionalDays">Number of additional days to extend the subscription</param>
    /// <returns>JsonModel containing the extension result</returns>
    /// <remarks>
    /// This endpoint:
    /// - Extends a user subscription duration with administrative authority
    /// - Validates subscription status and extension eligibility
    /// - Calculates new end date based on additional days
    /// - Access restricted to administrators only
    /// - Used for administrative subscription extension and management
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on extension operations
    /// - Maintains subscription extension audit trails
    /// </remarks>
    [HttpPost("admin/{id}/extend")]
    public async Task<JsonModel> ExtendUserSubscription(string id, [FromBody] int additionalDays)
    {
        return await _subscriptionLifecycleService.ExtendUserSubscriptionAsync(id, additionalDays, GetToken(HttpContext));
    }

    /// <summary>
    /// Performs bulk operations on multiple subscriptions for administrative management.
    /// This endpoint handles bulk actions including status updates, cancellations, and other
    /// administrative operations on multiple subscriptions simultaneously for efficient management.
    /// </summary>
    /// <param name="actions">List of bulk action request DTOs containing action details</param>
    /// <returns>JsonModel containing the bulk operation results</returns>
    /// <remarks>
    /// This endpoint:
    /// - Performs bulk operations on multiple subscriptions
    /// - Supports various administrative actions and status updates
    /// - Provides comprehensive result tracking and reporting
    /// - Includes success and failure counts for operation monitoring
    /// - Access restricted to administrators only
    /// - Used for efficient bulk subscription management
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on bulk operations
    /// - Maintains bulk operation audit trails and execution history
    /// </remarks>
    [HttpPost("admin/bulk-action")]
    public async Task<JsonModel> PerformBulkAction([FromBody] List<BulkActionRequestDto> actions)
    {
        return await _subscriptionLifecycleService.PerformBulkActionAsync(actions, GetToken(HttpContext));
    }

    /// <summary>
    /// Retrieves all subscription plans with comprehensive filtering and export capabilities for administrative management.
    /// This endpoint provides administrators with access to all subscription plans in the system with advanced filtering
    /// capabilities including search, category filtering, status filtering, and data export functionality.
    /// </summary>
    /// <param name="searchTerm">Search term for filtering plans by name or description</param>
    /// <param name="categoryId">Filter plans by specific category ID</param>
    /// <param name="isActive">Filter plans by active status (true/false/null for all)</param>
    /// <param name="page">Page number for pagination (default: 1)</param>
    /// <param name="pageSize">Number of records per page (default: 50)</param>
    /// <param name="format">Export format (csv/excel) for data export</param>
    /// <returns>JsonModel containing subscription plans or exported data</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns all subscription plans with comprehensive filtering options
    /// - Supports pagination for large datasets
    /// - Includes search functionality for plan names and descriptions
    /// - Filters by category and active status
    /// - Supports data export in CSV and Excel formats
    /// - Access restricted to administrators only
    /// - Used for administrative plan management and data export
    /// - Includes comprehensive plan information and metadata
    /// - Supports advanced filtering for plan analysis
    /// - Provides comprehensive plan overview for administrators
    /// </remarks>

    /// <summary>
    /// Retrieves all active subscription plans for administrative management.
    /// This endpoint provides administrators with access to all currently active subscription plans
    /// in the system for administrative oversight and management.
    /// </summary>
    /// <returns>JsonModel containing all active subscription plans</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns all subscription plans with active status
    /// - Includes plan details, pricing, and configuration information
    /// - Shows active plan availability and settings
    /// - Access restricted to administrators only
    /// - Used for active plan management and oversight
    /// - Includes comprehensive plan information and metadata
    /// - Provides data for active plan analysis and management
    /// - Handles plan validation and error responses
    /// </remarks>

    /// <summary>
    /// Retrieves subscription plans by specific category for administrative management.
    /// This endpoint provides administrators with access to subscription plans filtered by category
    /// for category-specific plan management and analysis.
    /// </summary>
    /// <param name="category">The category name to filter plans by</param>
    /// <returns>JsonModel containing subscription plans in the specified category</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns subscription plans filtered by specific category
    /// - Includes plan details, pricing, and category information
    /// - Shows category-specific plan availability and settings
    /// - Access restricted to administrators only
    /// - Used for category-specific plan management and analysis
    /// - Includes comprehensive plan information and metadata
    /// - Provides data for category plan analysis and management
    /// - Handles category validation and error responses
    /// </remarks>

    /// <summary>
    /// Retrieves detailed information about a specific subscription plan for administrative management.
    /// This endpoint provides administrators with comprehensive details about a specific subscription plan
    /// including configuration, pricing, and administrative information.
    /// </summary>
    /// <param name="planId">The unique identifier of the subscription plan</param>
    /// <returns>JsonModel containing the subscription plan details</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns detailed subscription plan information by ID
    /// - Includes plan configuration, pricing, and administrative details
    /// - Shows plan status and availability information
    /// - Access restricted to administrators only
    /// - Used for detailed plan management and configuration
    /// - Includes comprehensive plan information and metadata
    /// - Provides secure access to plan information
    /// - Handles plan validation and error responses
    /// </remarks>

    /// <summary>
    /// Creates a new subscription plan for administrative management.
    /// This endpoint handles subscription plan creation including validation, configuration,
    /// and initial setup for administrative management and user subscription options.
    /// </summary>
    /// <param name="createDto">DTO containing subscription plan creation details</param>
    /// <returns>JsonModel containing the created subscription plan information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Creates a new subscription plan with comprehensive validation
    /// - Validates plan configuration and pricing information
    /// - Sets up plan for administrative management
    /// - Access restricted to administrators only
    /// - Used for subscription plan creation and management
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on plan creation
    /// - Maintains plan creation audit trails and history
    /// </remarks>

    /// <summary>
    /// Updates an existing subscription plan for administrative management.
    /// This endpoint handles subscription plan updates including validation, configuration changes,
    /// and plan modification for administrative management and user subscription options.
    /// </summary>
    /// <param name="planId">The unique identifier of the subscription plan to update</param>
    /// <param name="updateDto">DTO containing subscription plan update details</param>
    /// <returns>JsonModel containing the updated subscription plan information</returns>
    /// <remarks>
    /// This endpoint:
    /// - Updates an existing subscription plan with comprehensive validation
    /// - Validates plan configuration and pricing changes
    /// - Ensures plan modification consistency and integrity
    /// - Access restricted to administrators only
    /// - Used for subscription plan modification and management
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on plan updates
    /// - Maintains plan update audit trails and change history
    /// </remarks>

    /// <summary>
    /// Deletes a subscription plan from the system for administrative management.
    /// This endpoint handles subscription plan deletion including validation, dependency checking,
    /// and plan removal for administrative management and system cleanup.
    /// </summary>
    /// <param name="planId">The unique identifier of the subscription plan to delete</param>
    /// <returns>JsonModel containing the deletion result</returns>
    /// <remarks>
    /// This endpoint:
    /// - Deletes a subscription plan with comprehensive validation
    /// - Checks for existing subscriptions and dependencies
    /// - Ensures safe plan removal without data integrity issues
    /// - Access restricted to administrators only
    /// - Used for subscription plan removal and system cleanup
    /// - Includes comprehensive validation and error handling
    /// - Provides detailed feedback on plan deletion
    /// - Maintains plan deletion audit trails and removal history
    /// </remarks>

    /// <summary>
    /// Retrieves all categories with comprehensive filtering and export capabilities for administrative management.
    /// This endpoint provides administrators with access to all categories in the system with advanced filtering
    /// capabilities including search, status filtering, and data export functionality.
    /// </summary>
    /// <param name="page">Page number for pagination (default: 1)</param>
    /// <param name="pageSize">Number of records per page (default: 10)</param>
    /// <param name="searchTerm">Search term for filtering categories by name or description</param>
    /// <param name="isActive">Filter categories by active status (true/false/null for all)</param>
    /// <param name="format">Export format (csv/excel) for data export</param>
    /// <returns>JsonModel containing categories or exported data</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns all categories with comprehensive filtering options
    /// - Supports pagination for large datasets
    /// - Includes search functionality for category names and descriptions
    /// - Filters by active status
    /// - Supports data export in CSV and Excel formats
    /// - Access restricted to administrators only
    /// - Used for administrative category management and data export
    /// - Includes comprehensive category information and metadata
    /// - Supports advanced filtering for category analysis
    /// - Provides comprehensive category overview for administrators
    /// </remarks>
    [HttpGet("admin/categories")]
    public async Task<JsonModel> GetAllCategories(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? format = null)
    {
        // If format is specified, return export data
        if (!string.IsNullOrEmpty(format) && (format.ToLower() == "csv" || format.ToLower() == "excel"))
        {
            // This method should be moved to a dedicated category service
            return new JsonModel 
            { 
                data = new object(), 
                Message = "Category export not available - use category service", 
                StatusCode = 501 
            };
        }
        
        return await _subscriptionService.GetAllCategoriesAsync(page, pageSize, searchTerm, isActive, GetToken(HttpContext));
    }

    /// <summary>
    /// Retrieves subscription plans with comprehensive filtering, pagination, export, and analytics capabilities for administrative management.
    /// This endpoint provides administrators with advanced functionality including pagination, filtering, data export,
    /// and analytics integration for comprehensive subscription plan management.
    /// </summary>
    /// <param name="page">Page number for pagination (default: 1)</param>
    /// <param name="pageSize">Number of records per page (default: 10)</param>
    /// <param name="searchTerm">Search term for filtering plans by name or description</param>
    /// <param name="categoryId">Filter plans by specific category ID</param>
    /// <param name="isActive">Filter plans by active status (true/false/null for all)</param>
    /// <param name="format">Export format (csv/excel) for data export</param>
    /// <param name="includeAnalytics">Include analytics data in the response</param>
    /// <returns>JsonModel containing subscription plans, exported data, or analytics</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns subscription plans with comprehensive filtering options
    /// - Supports pagination for large datasets
    /// - Includes search functionality for plan names and descriptions
    /// - Filters by category and active status
    /// - Supports data export in CSV and Excel formats
    /// - Includes analytics data when requested
    /// - Access restricted to administrators only
    /// - Used for comprehensive plan management, data export, and analytics
    /// - Includes comprehensive plan information and metadata
    /// - Supports advanced filtering for plan analysis
    /// - Provides comprehensive plan overview for administrators
    /// </remarks>

    /// <summary>
    /// Retrieves public subscription plans for homepage display (no authentication required).
    /// This endpoint returns a curated list of subscription plans that are suitable for
    /// public display on the homepage, including only active plans with public visibility.
    /// </summary>
    /// <returns>JsonModel containing the list of public subscription plans</returns>
    /// <remarks>
    /// This endpoint:
    /// - Returns only active subscription plans marked for public display
    /// - Includes plan pricing, features, and marketing information
    /// - No authentication required - accessible to all visitors
    /// - Used for homepage plan comparison and marketing
    /// - Optimized for public consumption with limited sensitive information
    /// </remarks>
} 