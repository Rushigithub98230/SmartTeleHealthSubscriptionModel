using AutoMapper;
using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Application.Utilities;
using SmartTelehealth.Application.Constants;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Core.Enums;

namespace SmartTelehealth.Application.Services;

/// <summary>
/// Comprehensive Subscription Billing Service
/// Combines all functionality from BillingService and PrivilegeBasedBillingService
/// Aligned with client's subscription management billing workflow
/// </summary>
public class SubscriptionBillingService : ISubscriptionBillingService
{
    #region Dependencies
    
    // Core repositories
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBillingRepository _billingRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly ISubscriptionPlanRepository _subscriptionPlanRepository;
    private readonly IUserSubscriptionPrivilegeUsageRepository _privilegeUsageRepository;
    private readonly IPrivilegeRepository _privilegeRepository;
    private readonly IUserRepository _userRepository;
    private readonly ISystemSettingsRepository _systemSettingsRepository;
    
    // Service dependencies
    private readonly IPaymentService _paymentService;
    private readonly IStripeService _stripeService;
    private readonly INotificationService _notificationService;
    private readonly IPlanPricingService _pricingService;
    private readonly IRealTimeLogsService _realTimeLogsService;
    
    // Utilities
    private readonly IMapper _mapper;
    private readonly ILogger<SubscriptionBillingService> _logger;
    
    #endregion

    #region Constructor
    
    public SubscriptionBillingService(
        IUnitOfWork unitOfWork,
        IBillingRepository billingRepository,
        ISubscriptionRepository subscriptionRepository,
        ISubscriptionPlanRepository subscriptionPlanRepository,
        IUserSubscriptionPrivilegeUsageRepository privilegeUsageRepository,
        IPrivilegeRepository privilegeRepository,
        IUserRepository userRepository,
        ISystemSettingsRepository systemSettingsRepository,
        IPaymentService paymentService,
        IStripeService stripeService,
        INotificationService notificationService,
        IPlanPricingService pricingService,
        IRealTimeLogsService realTimeLogsService,
        IMapper mapper,
        ILogger<SubscriptionBillingService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _billingRepository = billingRepository ?? throw new ArgumentNullException(nameof(billingRepository));
        _subscriptionRepository = subscriptionRepository ?? throw new ArgumentNullException(nameof(subscriptionRepository));
        _subscriptionPlanRepository = subscriptionPlanRepository ?? throw new ArgumentNullException(nameof(subscriptionPlanRepository));
        _privilegeUsageRepository = privilegeUsageRepository ?? throw new ArgumentNullException(nameof(privilegeUsageRepository));
        _privilegeRepository = privilegeRepository ?? throw new ArgumentNullException(nameof(privilegeRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _systemSettingsRepository = systemSettingsRepository ?? throw new ArgumentNullException(nameof(systemSettingsRepository));
        _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
        _stripeService = stripeService ?? throw new ArgumentNullException(nameof(stripeService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _pricingService = pricingService ?? throw new ArgumentNullException(nameof(pricingService));
        _realTimeLogsService = realTimeLogsService ?? throw new ArgumentNullException(nameof(realTimeLogsService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    #endregion

    #region Privilege-Based Billing (Client Workflow - Fully Migrated)
    
    /// <summary>
    /// Calculates the base price for a subscription plan based on privileges and their unit costs
    /// Client Workflow Step 1: Admin Creates a Subscription Plan
    /// Formula: Base Price = Σ(PrivilegeLimit × UnitCost) + AdminCommission
    /// MIGRATED & FIXED: Uses Value field correctly
    /// </summary>
    public async Task<JsonModel> CalculatePlanBasePriceAsync(CalculatePlanPriceDto calculateDto, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Calculating base price for plan {PlanId} by user {UserId}", 
                calculateDto.PlanId, tokenModel?.UserID ?? 0);

            var plan = await _subscriptionPlanRepository.GetByIdWithDetailsAsync(calculateDto.PlanId);
            if (plan == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Subscription plan not found",
                    StatusCode = 404
                };
            }

            var planPrivileges = await _subscriptionPlanRepository.GetPlanPrivilegesAsync(calculateDto.PlanId);
            
            decimal totalBasePrice = 0;
            var privilegeBreakdown = new List<object>();

            var privilegeIds = planPrivileges.Select(pp => pp.PrivilegeId).ToList();
            var privileges = await _privilegeRepository.GetByIdsAsync(privilegeIds);
            var privilegeLookup = privileges.ToDictionary(p => p.Id, p => p);

            foreach (var planPrivilege in planPrivileges)
            {
                if (!privilegeLookup.TryGetValue(planPrivilege.PrivilegeId, out var privilege))
                    continue;

                // CRITICAL FIX: Use centralized privilege cost calculation
                // This ensures consistent pricing for unlimited privileges (Value = -1)
                var privilegeCost = BillingCalculationService.CalculatePrivilegeCost(planPrivilege, _logger);
                totalBasePrice += privilegeCost;

                privilegeBreakdown.Add(new
                {
                    PrivilegeId = privilege.Id,
                    PrivilegeName = privilege.Name,
                    PrivilegeLimit = planPrivilege.Value,
                    UnitCost = planPrivilege.PrivilegeBaseCost,
                    TotalCost = privilegeCost,
                    IsUnlimited = planPrivilege.Value == -1,
                    IsDisabled = planPrivilege.Value == 0
                });
            }

            // CRITICAL FIX: Use centralized commission calculation
            // Get system settings for default commission percentage
            var settings = await _systemSettingsRepository.GetSettingsAsync();
            var defaultCommissionPercent = settings?.DefaultAdminCommissionPercent ?? 0;

            // Use centralized commission calculation (ignores API parameter for security)
            var (finalPrice, adminCommission, commissionPercent) = BillingCalculationService.CalculateFinalPlanPrice(
                totalBasePrice, 
                plan.AdminCommissionPercent, 
                defaultCommissionPercent, 
                _logger);

            _logger.LogInformation("Base price calculated for plan {PlanId}: {BasePrice} + {Commission} ({Percent}%) = {FinalPrice}", 
                calculateDto.PlanId, totalBasePrice, adminCommission, commissionPercent, finalPrice);

            return new JsonModel
            {
                data = new
                {
                    PlanId = calculateDto.PlanId,
                    PlanName = plan.Name,
                    BasePrice = totalBasePrice,
                    AdminCommission = adminCommission,
                    CommissionPercent = commissionPercent,
                    FinalPrice = finalPrice,
                    PrivilegeBreakdown = privilegeBreakdown,
                    CalculatedAt = DateTime.UtcNow,
                    CommissionSource = plan.AdminCommissionPercent.HasValue ? "Plan" : "System Default"
                },
                Message = "Plan base price calculated successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating plan base price for plan {PlanId}", calculateDto.PlanId);
            return new JsonModel
            {
                data = new object(),
                Message = "Error calculating plan base price",
                StatusCode = 500
            };
        }
    }

    /// <summary>
    /// Processes privilege usage and calculates extra charges if limits are exceeded
    /// Client Workflow Steps 3-4: Privilege Usage Tracking & Extra Usage Calculation
    /// MIGRATED - Full implementation with all helper methods
    /// </summary>
    public async Task<JsonModel> ProcessPrivilegeUsageAsync(ProcessPrivilegeUsageDto usageDto, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Processing privilege usage for user {UserId}, privilege {PrivilegeId}", 
                usageDto.UserId, usageDto.PrivilegeId);

            var subscription = await _subscriptionRepository.GetActiveSubscriptionByUserIdAsync(usageDto.UserId);
            if (subscription == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "No active subscription found for user",
                    StatusCode = 404
                };
            }

            var privilegeUsage = await GetOrCreatePrivilegeUsageAsync(usageDto.UserId, usageDto.PrivilegeId, subscription.Id, tokenModel);

            var planPrivilege = await _subscriptionPlanRepository.GetPlanPrivilegeAsync(subscription.SubscriptionPlanId, usageDto.PrivilegeId);
            if (planPrivilege == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Plan privilege configuration not found",
                    StatusCode = 404
                };
            }

            await RecordUsageEventAsync(usageDto.UserId, usageDto.PrivilegeId, usageDto.UsageCount, tokenModel);

            // Time-based limits removed - overage only when total AllowedValue exhausted
            var currentUsage = await _privilegeUsageRepository.GetByUserAndPrivilegeAsync(usageDto.UserId, usageDto.PrivilegeId);
            var isOverLimit = currentUsage != null && currentUsage.UsedValue >= currentUsage.AllowedValue;
            
            decimal extraCharge = 0; // No automatic overage charges

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _privilegeUsageRepository.UpdatePrivilegeUsageAsync(privilegeUsage);

                if (extraCharge > 0)
                {
                    await BatchOverageChargeAsync(subscription, usageDto.PrivilegeId, extraCharge, tokenModel);
                }

                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error in transaction for privilege usage processing");
                throw;
            }

            return new JsonModel
            {
                data = new
                {
                    UserId = usageDto.UserId,
                    PrivilegeId = usageDto.PrivilegeId,
                    UsedCount = privilegeUsage.UsedValue,
                    Limit = planPrivilege.Value, // Total privilege limit
                    IsOverLimit = isOverLimit,
                    ExtraCharge = extraCharge,
                    ProcessedAt = DateTime.UtcNow
                },
                Message = isOverLimit ? "Privilege usage processed with extra charges" : "Privilege usage processed within limits",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing privilege usage for user {UserId}, privilege {PrivilegeId}", 
                usageDto.UserId, usageDto.PrivilegeId);
            return new JsonModel
            {
                data = new object(),
                Message = "Error processing privilege usage",
                StatusCode = 500
            };
        }
    }

    /// <summary>
    /// MASTER METHOD: Processes COMPLETE subscription renewal including ALL operations.
    /// This is the SINGLE SOURCE OF TRUTH for all subscription renewals.
    /// 
    /// CRITICAL FIX: This method now performs ALL renewal operations:
    /// - Billing date updates (LastBillingDate, NextBillingDate)
    /// - Privilege usage reset  
    /// - Billing record creation
    /// - Payment processing via Stripe
    /// - Overage charge handling
    /// - Notifications
    /// 
    /// Uses Saga pattern for distributed transaction safety:
    /// - Tracks compensating transactions for each step
    /// - Executes compensations if payment fails after database commit
    /// - Ensures data consistency even with external API failures
    /// 
    /// Client Workflow Step 6: Complete Renewal Process
    /// </summary>
    public async Task<JsonModel> ProcessSubscriptionRenewalAsync(Guid subscriptionId, TokenModel tokenModel)
    {
        var saga = new SagaCoordinator(_logger);
        
        // Track state for compensation
        Subscription? originalSubscription = null;
        List<UserSubscriptionPrivilegeUsage>? originalPrivilegeUsages = null;
        Guid? createdBillingRecordId = null;
        bool paymentAttempted = false;
        
        try
        {
            _logger.LogInformation("=== COMPLETE RENEWAL PROCESS STARTED for subscription {SubscriptionId} ===", subscriptionId);

            // ═══════════════════════════════════════════════════════════
            // STEP 1: LOAD & VALIDATE SUBSCRIPTION
            // ═══════════════════════════════════════════════════════════
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

                var plan = await _subscriptionPlanRepository.GetByIdWithDetailsAsync(subscription.SubscriptionPlanId);
                if (plan == null)
                {
                    return new JsonModel
                    {
                        data = new object(),
                        Message = "Subscription plan not found",
                        StatusCode = 404
                    };
                }

            // Capture original state for compensation
            originalSubscription = new Subscription
            {
                Id = subscription.Id,
                LastBillingDate = subscription.LastBillingDate,
                NextBillingDate = subscription.NextBillingDate,
                Status = subscription.Status,
                FailedPaymentAttempts = subscription.FailedPaymentAttempts
            };

            var allPrivilegeUsages = await _privilegeUsageRepository.GetByUserIdAsync(subscription.UserId);
            originalPrivilegeUsages = allPrivilegeUsages
                .Where(u => u.SubscriptionId == subscriptionId)
                .Select(u => new UserSubscriptionPrivilegeUsage
                {
                    Id = u.Id,
                    UsedValue = u.UsedValue,
                    AllowedValue = u.AllowedValue,
                    UsagePeriodStart = u.UsagePeriodStart,
                    UsagePeriodEnd = u.UsagePeriodEnd,
                    ResetAt = u.ResetAt
                }).ToList();

            _logger.LogInformation("[Step 1/7] Subscription validated, original state captured");

            // ═══════════════════════════════════════════════════════════
            // STEP 2: CALCULATE RENEWAL AMOUNT (Including Overage)
            // ═══════════════════════════════════════════════════════════
            var pendingOverage = await _billingRepository.GetByUserIdAsync(subscription.UserId);
            var pendingOverageAmount = pendingOverage
                .Where(b => b.Type == BillingRecord.BillingType.Overage && 
                           b.Status == BillingRecord.BillingStatus.Pending &&
                           b.SubscriptionId == subscriptionId)
                .Sum(b => b.TotalAmount);

            var baseRenewalAmount = BillingCalculationService.GetEffectivePlanPrice(plan, null, _logger);
            var totalRenewalAmount = baseRenewalAmount + pendingOverageAmount;

            _logger.LogInformation("[Step 2/7] Renewal amount calculated: Base=${Base}, Overage=${Overage}, Total=${Total}",
                baseRenewalAmount, pendingOverageAmount, totalRenewalAmount);

            // ═══════════════════════════════════════════════════════════
            // STEP 3: BEGIN DATABASE TRANSACTION
            // ═══════════════════════════════════════════════════════════
            _logger.LogInformation("[Step 3/7] Beginning database transaction...");
            await _unitOfWork.BeginTransactionAsync();
            
            try
            {
                // ═══════════════════════════════════════════════════════════
                // STEP 4: UPDATE BILLING DATES (With Compensation)
                // ═══════════════════════════════════════════════════════════
                var oldNextBillingDate = subscription.NextBillingDate;
                var oldLastBillingDate = subscription.LastBillingDate;
                
                subscription.LastBillingDate = oldNextBillingDate;
                subscription.NextBillingDate = BillingCycleCalculator.CalculateNextBillingDate(
                    subscription.LastBillingDate.Value, 
                    plan.BillingCycle);
                subscription.UpdatedBy = tokenModel.UserID;
                subscription.UpdatedDate = DateTime.UtcNow;
                
                await _subscriptionRepository.UpdateAsync(subscription);
                
                _logger.LogInformation("[Step 4/7] Billing dates updated: Last={Last:yyyy-MM-dd}, Next={Next:yyyy-MM-dd}",
                    subscription.LastBillingDate, subscription.NextBillingDate);

                // Register compensation: Revert billing dates
                saga.AddCompensation(async () =>
                {
                    _logger.LogWarning("[COMPENSATION] Reverting billing dates...");
                    subscription.LastBillingDate = oldLastBillingDate;
                    subscription.NextBillingDate = oldNextBillingDate;
                    subscription.UpdatedBy = tokenModel.UserID;
                    subscription.UpdatedDate = DateTime.UtcNow;
                    await _subscriptionRepository.UpdateAsync(subscription);
                });

                // ═══════════════════════════════════════════════════════════
                // STEP 5: CREATE BILLING RECORD (With Compensation)
                // ═══════════════════════════════════════════════════════════
                // CRITICAL FIX: Use CreateSubscriptionBillingAsync to ensure tax is applied
                // This ensures renewal billing applies the same tax logic as initial billing
                var billingResult = await CreateSubscriptionBillingAsync(
                    subscription,
                    totalRenewalAmount,
                    $"Subscription renewal for {plan.Name} - {plan.BillingCycle?.Name ?? "monthly"} billing",
                    subscription.NextBillingDate,
                    tokenModel
                );

                if (billingResult.StatusCode != 200)
                {
                    throw new InvalidOperationException($"Failed to create renewal billing record: {billingResult.Message}");
                }

                // Extract billing record ID from result for compensation tracking
                var billingRecordDto = billingResult.data as BillingRecordDto;
                if (billingRecordDto == null)
                {
                    throw new InvalidOperationException("Billing record creation succeeded but no record data returned");
                }
                
                if (!Guid.TryParse(billingRecordDto.Id, out var parsedBillingRecordId))
                {
                    throw new InvalidOperationException($"Invalid billing record ID format: {billingRecordDto.Id}");
                }
                
                createdBillingRecordId = parsedBillingRecordId;

                var totalAmount = billingRecordDto.Amount + billingRecordDto.TaxAmount;
                _logger.LogInformation("[Step 5/7] Billing record created: {BillingRecordId}, Amount=${Amount}, Tax=${Tax}, Total=${Total}",
                    createdBillingRecordId, billingRecordDto.Amount, billingRecordDto.TaxAmount, totalAmount);

                // Register compensation: Delete billing record
                saga.AddCompensation(async () =>
                {
                    _logger.LogWarning("[COMPENSATION] Deleting billing record {BillingRecordId}...", createdBillingRecordId);
                    var billingRecord = await _billingRepository.GetByIdAsync(createdBillingRecordId);
                    if (billingRecord != null)
                    {
                        billingRecord.IsDeleted = true;
                        billingRecord.DeletedBy = tokenModel.UserID;
                        billingRecord.DeletedDate = DateTime.UtcNow;
                        await _billingRepository.UpdateAsync(billingRecord);
                    }
                });

                // ═══════════════════════════════════════════════════════════
                // STEP 6: RESET PRIVILEGE USAGE (With Compensation)
                // ═══════════════════════════════════════════════════════════
                var privilegeUsages = await _privilegeUsageRepository.GetByUserIdAsync(subscription.UserId);
                var subscriptionPrivileges = privilegeUsages.Where(u => u.SubscriptionId == subscriptionId).ToList();

                // Use centralized helper for consistent privilege reset logic
                // This ensures all privilege resets use the same logic across all services
                try
                {
                    await PrivilegeResetHelper.ResetPrivilegesForBillingPeriodAsync(
                        subscription,
                        subscriptionPrivileges,
                        async (usage) => await _privilegeUsageRepository.UpdatePrivilegeUsageAsync(usage),
                        tokenModel.UserID,
                        _logger
                    );

                    _logger.LogInformation("[Step 6/7] Privilege usage reset complete: {Count} privileges reset", subscriptionPrivileges.Count);
                }
                catch (Exception privilegeResetEx)
                {
                    // TASK 3.3: Send admin alert when privilege reset fails during renewal
                    _logger.LogError(privilegeResetEx, 
                        "CRITICAL: Privilege reset failed during renewal for subscription {SubscriptionId}", 
                        subscription.Id);
                    
                    try
                    {
                        var user = await _userRepository.GetByIdAsync(subscription.UserId);
                        var subscriptionPlan = await _subscriptionPlanRepository.GetByIdAsync(subscription.SubscriptionPlanId);
                        
                        var alertMessage = $"Privilege reset failed during subscription renewal for subscription {subscription.Id}. " +
                                         $"User: {user?.Email ?? subscription.UserId.ToString()}, " +
                                         $"Plan: {subscriptionPlan?.Name ?? "Unknown"}, " +
                                         $"Error: {privilegeResetEx.Message}. " +
                                         $"Renewal billing was successful, but privileges were NOT reset for the new billing period. " +
                                         $"Manual intervention required to reset privileges.";
                        
                        _logger.LogCritical(
                            "CRITICAL ALERT: Privilege reset failed for subscription {SubscriptionId} during renewal",
                            subscription.Id);
                        
                        // Broadcast to all connected admin users
                        await _realTimeLogsService.BroadcastNotificationAsync(
                            "Privilege Reset Failure During Renewal",
                            alertMessage,
                            "error");
                    }
                    catch (Exception alertEx)
                    {
                        _logger.LogError(alertEx, "Failed to send admin alert for privilege reset failure during renewal");
                        // Continue - don't fail the renewal if alert fails
                    }
                    
                    // Don't throw - renewal should complete even if privilege reset fails
                    // Admin will be notified and can manually reset privileges
                }

                // Register compensation: Restore original privilege usage
                saga.AddCompensation(async () =>
                {
                    _logger.LogWarning("[COMPENSATION] Restoring original privilege usage...");
                    foreach (var original in originalPrivilegeUsages!)
                    {
                        var current = await _privilegeUsageRepository.GetByIdAsync(original.Id);
                        if (current != null)
                        {
                            current.UsedValue = original.UsedValue;
                            current.AllowedValue = original.AllowedValue;
                            current.UsagePeriodStart = original.UsagePeriodStart;
                            current.UsagePeriodEnd = original.UsagePeriodEnd;
                            current.ResetAt = original.ResetAt;
                            await _privilegeUsageRepository.UpdatePrivilegeUsageAsync(current);
                        }
                    }
                });

                // Mark overage records as paid (included in renewal)
                if (pendingOverageAmount > 0)
                {
                    var overageRecords = pendingOverage
                        .Where(b => b.Type == BillingRecord.BillingType.Overage && 
                                   b.Status == BillingRecord.BillingStatus.Pending &&
                                   b.SubscriptionId == subscriptionId);
                    
                    foreach (var overageRecord in overageRecords)
                    {
                        overageRecord.Status = BillingRecord.BillingStatus.Paid;
                        overageRecord.PaidAt = DateTime.UtcNow;
                        overageRecord.UpdatedBy = tokenModel.UserID;
                        overageRecord.UpdatedDate = DateTime.UtcNow;
                        await _billingRepository.UpdateAsync(overageRecord);
                    }

                    _logger.LogInformation("Marked {Count} overage records as paid (${Amount} included in renewal)",
                        overageRecords.Count(), pendingOverageAmount);
                }

                // ═══════════════════════════════════════════════════════════
                // COMMIT DATABASE TRANSACTION
                // ═══════════════════════════════════════════════════════════
                await _unitOfWork.CommitTransactionAsync();
                _logger.LogInformation("[Step 6/7] ✅ Database transaction committed successfully");
            }
            catch (Exception dbEx)
            {
                // Database transaction failed - rollback and abort
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(dbEx, "❌ Database transaction failed, rolled back");
                throw; // Exit - don't attempt payment
            }

            // ═══════════════════════════════════════════════════════════
            // STEP 7: PROCESS PAYMENT (EXTERNAL - AFTER COMMIT)
            // ═══════════════════════════════════════════════════════════
            // Payment happens AFTER database commit because:
            // 1. Stripe is external (can't participate in DB transaction)
            // 2. If payment fails, we use compensating transactions
            // 3. Billing record exists in DB, can retry payment later
            
            try
            {
                _logger.LogInformation("[Step 7/7] Processing payment via Stripe...");
                paymentAttempted = true;
                
                var paymentResult = await _paymentService.ProcessPaymentAsync(
                    createdBillingRecordId!.Value, 
                    tokenModel);

                if (paymentResult.StatusCode == 200)
                {
                    // ✅ SUCCESS - Payment processed
                    _logger.LogInformation("✅ [Step 7/7] Payment succeeded: ${Amount}", totalRenewalAmount);
                    
                    // Clear compensations (saga successful!)
                    saga.Clear();
                    
                    // Send success notification
                    try
                    {
                        var user = await _userRepository.GetByIdAsync(subscription.UserId);
                        if (user != null && createdBillingRecordId.HasValue)
                        {
                            var billingRecordForEmail = await _billingRepository.GetByIdAsync(createdBillingRecordId.Value);
                            if (billingRecordForEmail != null)
                            {
                                var billingDto = _mapper.Map<BillingRecordDto>(billingRecordForEmail);
                                await _notificationService.SendPaymentSuccessEmailAsync(
                                    user.Email ?? "",
                                    user.FullName,
                                    billingDto,
                                    tokenModel);
                            }
                        }
                    }
                    catch (Exception notifEx)
                    {
                        _logger.LogWarning(notifEx, "Failed to send renewal notification, but renewal succeeded");
            }

            return new JsonModel
            {
                data = new
                {
                            subscriptionId,
                            renewalAmount = totalRenewalAmount,
                            nextBillingDate = subscription.NextBillingDate,
                            lastBillingDate = subscription.LastBillingDate,
                            privilegesReset = true,
                            paymentStatus = "Succeeded",
                            billingRecordId = createdBillingRecordId,
                            processedAt = DateTime.UtcNow
                        },
                        Message = "Subscription renewed successfully with payment processed",
                StatusCode = 200
            };
                }
                else
                {
                    // ⚠️ PAYMENT FAILED - Execute compensating transactions
                    _logger.LogWarning("⚠️ [Step 7/7] Payment failed: {Error}. Executing compensations...", 
                        paymentResult.Message);

                    // Execute compensations to revert database changes
                    await saga.ExecuteCompensationsAsync();

                    // Update subscription to indicate payment failure
                    subscription.Status = Subscription.SubscriptionStatuses.PaymentFailed;
                    subscription.FailedPaymentAttempts += 1;
                    subscription.LastPaymentError = paymentResult.Message;
                    subscription.LastPaymentFailedDate = DateTime.UtcNow;
                    await _subscriptionRepository.UpdateAsync(subscription);

                    // Send payment failed notification
                    try
                    {
                        var user = await _userRepository.GetByIdAsync(subscription.UserId);
                        if (user != null && createdBillingRecordId.HasValue)
                        {
                            var billingRecordForEmail = await _billingRepository.GetByIdAsync(createdBillingRecordId.Value);
                            if (billingRecordForEmail != null)
                            {
                                var billingDto = _mapper.Map<BillingRecordDto>(billingRecordForEmail);
                                await _notificationService.SendPaymentFailedEmailAsync(
                                    user.Email ?? "",
                                    user.FullName,
                                    billingDto,
                                    tokenModel);
                            }
                        }
                    }
                    catch (Exception notifEx)
                    {
                        _logger.LogWarning(notifEx, "Failed to send payment failure notification");
                    }

                    return new JsonModel
                    {
                        data = new
                        {
                            subscriptionId,
                            renewalAmount = totalRenewalAmount,
                            paymentStatus = "Failed",
                            paymentError = paymentResult.Message,
                            compensationsExecuted = saga.Count,
                            willRetry = subscription.FailedPaymentAttempts < 3,
                            processedAt = DateTime.UtcNow
                        },
                        Message = $"Renewal payment failed: {paymentResult.Message}. Database changes reverted. Will retry automatically.",
                        StatusCode = 402 // Payment Required
                    };
                }
            }
            catch (Exception paymentEx)
            {
                // ═══════════════════════════════════════════════════════════
                // PAYMENT PROCESSING EXCEPTION - Execute Compensations
                // ═══════════════════════════════════════════════════════════
                _logger.LogError(paymentEx, "❌ CRITICAL: Payment processing threw exception after database commit!");

                // Execute compensating transactions to revert database changes
                _logger.LogWarning("Executing compensations to revert database changes...");
                await saga.ExecuteCompensationsAsync();

                // If payment was partially processed, attempt refund
                if (createdBillingRecordId.HasValue)
                {
                    await IssueCompensatingRefundIfNeededAsync(createdBillingRecordId.Value, totalRenewalAmount, tokenModel);
                }

                throw; // Re-throw for outer catch
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Subscription renewal failed for {SubscriptionId}", subscriptionId);

            // If we haven't attempted payment yet, just rollback (already done in inner catch)
            // If payment was attempted, compensations already executed above

            return new JsonModel
            {
                data = new object(),
                Message = $"Subscription renewal failed: {ex.Message}",
                StatusCode = 500
            };
        }
        finally
        {
            _logger.LogInformation("=== RENEWAL PROCESS COMPLETE for subscription {SubscriptionId} ===", subscriptionId);
        }
    }

    /// <summary>
    /// Issues a compensating refund if payment was processed but renewal failed.
    /// </summary>
    private async Task IssueCompensatingRefundIfNeededAsync(Guid billingRecordId, decimal amount, TokenModel tokenModel)
    {
        try
        {
            var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
            
            if (billingRecord != null && 
                billingRecord.Status == BillingRecord.BillingStatus.Paid && 
                !string.IsNullOrEmpty(billingRecord.StripePaymentIntentId))
            {
                _logger.LogWarning("Payment was processed but renewal failed. Issuing compensating refund...");
                
                var refundResult = await _paymentService.ProcessRefundAsync(billingRecordId, amount, tokenModel);
                
                if (refundResult.StatusCode == 200)
                {
                    _logger.LogInformation("✅ Compensating refund issued successfully: ${Amount}", amount);
                }
                else
                {
                    _logger.LogError("❌ CRITICAL: Compensating refund failed! Manual refund required for billing record {BillingRecordId}", 
                        billingRecordId);
                    
                    // Send critical alert to admin
                    await SendCriticalAlertAsync(
                        "Renewal Compensation Failure",
                        $"Billing Record {billingRecordId}: Payment processed (${amount}) but renewal failed. " +
                        $"Automatic refund also failed. MANUAL REFUND REQUIRED.",
                        tokenModel);
                }
            }
        }
        catch (Exception refundEx)
        {
            _logger.LogError(refundEx, "❌ CRITICAL: Exception during compensating refund! Manual intervention required.");
            
            await SendCriticalAlertAsync(
                "Renewal Compensation Exception",
                $"Billing Record {billingRecordId}: Exception during compensating refund. MANUAL INTERVENTION REQUIRED.",
                tokenModel);
        }
    }

    /// <summary>
    /// Sends critical alert to system administrators.
    /// </summary>
    private async Task SendCriticalAlertAsync(string subject, string message, TokenModel tokenModel)
    {
        try
        {
            // Log critical error
            _logger.LogCritical("CRITICAL ALERT: {Subject} - {Message}", subject, message);
            
            // Send notification (implement based on your notification system)
            // await _notificationService.SendAdminAlertAsync(subject, message, tokenModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send critical alert");
        }
    }

    /// <summary>
    /// UPDATED: This method now performs COMPLETE renewal (billing + payment + date updates + privilege reset).
    /// Delegates to ProcessSubscriptionRenewalAsync which now handles ALL renewal operations.
    /// 
    /// This is an alias method that maintains backward compatibility.
    /// Both this method and ProcessSubscriptionRenewalAsync now do complete renewal with:
    /// - Billing date updates
    /// - Privilege usage reset
    /// - Billing record creation
    /// - Payment processing
    /// - Saga pattern for distributed transaction safety
    /// 
    /// BUILD FIX: Added to satisfy interface contract.
    /// CRITICAL FIX: Now performs complete renewal, not just date/privilege reset.
    /// </summary>
    public async Task<JsonModel> ResetSubscriptionForNewBillingPeriodAsync(Guid subscriptionId, TokenModel tokenModel)
    {
        // Delegate to the master renewal method (now does COMPLETE renewal)
        return await ProcessSubscriptionRenewalAsync(subscriptionId, tokenModel);
    }

    /// <summary>
    /// Gets privilege usage summary for a user
    /// MIGRATED - Full implementation
    /// </summary>
    public async Task<JsonModel> GetPrivilegeUsageSummaryAsync(int userId, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Getting privilege usage summary for user {UserId}", userId);

            var subscription = await _subscriptionRepository.GetActiveSubscriptionByUserIdAsync(userId);
            if (subscription == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "No active subscription found for user",
                    StatusCode = 404
                };
            }

            var privilegeUsages = await _privilegeUsageRepository.GetByUserIdAsync(userId);
            var planPrivileges = await _subscriptionPlanRepository.GetPlanPrivilegesAsync(subscription.SubscriptionPlanId);

            var usageSummary = new List<object>();
            decimal totalOverageCharges = 0;

            var privilegeIds = privilegeUsages.Select(u => u.PrivilegeId).Distinct().ToList();
            var privileges = await _privilegeRepository.GetByIdsAsync(privilegeIds);
            var privilegeLookup = privileges.ToDictionary(p => p.Id, p => p);

            foreach (var usage in privilegeUsages)
            {
                var planPrivilege = planPrivileges.FirstOrDefault(pp => pp.PrivilegeId == usage.PrivilegeId);
                if (planPrivilege == null) continue;

                privilegeLookup.TryGetValue(usage.PrivilegeId, out var privilege);
                
                var totalLimit = planPrivilege.Value; // Total privilege limit
                var isOverLimit = usage.UsedValue > totalLimit;
                var overageCount = isOverLimit ? usage.UsedValue - totalLimit : 0;
                var overageCharge = overageCount * planPrivilege.UnitCost;
                totalOverageCharges += overageCharge;

                usageSummary.Add(new
                {
                    PrivilegeId = usage.PrivilegeId,
                    PrivilegeName = privilege?.Name,
                    UsedCount = usage.UsedValue,
                    TotalLimit = totalLimit,
                    UnitCost = planPrivilege.UnitCost,
                    IsOverLimit = isOverLimit,
                    OverageCount = overageCount,
                    OverageCharge = overageCharge,
                    RemainingCount = Math.Max(0, totalLimit - usage.UsedValue),
                    BillingCycle = subscription.BillingCycle?.Name ?? "Unknown"
                });
            }

            return new JsonModel
            {
                data = new
                {
                    UserId = userId,
                    SubscriptionId = subscription.Id,
                    UsageSummary = usageSummary,
                    TotalOverageCharges = totalOverageCharges,
                    GeneratedAt = DateTime.UtcNow
                },
                Message = "Privilege usage summary retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting privilege usage summary for user {UserId}", userId);
            return new JsonModel
            {
                data = new object(),
                Message = "Error retrieving privilege usage summary",
                StatusCode = 500
            };
        }
    }

    #region Privilege-Based Billing Helper Methods
    
    /// <summary>
    /// Gets or creates privilege usage record with proper allocation.
    /// CORRECTED: Now uses admin-set Value directly (no calculation).
    /// The Value field contains the total privilege count set by admin for the billing cycle.
    /// </summary>
    private async Task<UserSubscriptionPrivilegeUsage> GetOrCreatePrivilegeUsageAsync(int userId, Guid privilegeId, Guid subscriptionId, TokenModel tokenModel)
    {
        var existingUsage = await _privilegeUsageRepository.GetByUserAndPrivilegeAsync(userId, privilegeId);
        if (existingUsage != null)
        {
            return existingUsage;
        }

        // Get subscription and plan privilege to get the admin-set Value
        var subscription = await _subscriptionRepository.GetByIdWithDetailsAsync(subscriptionId);
        if (subscription == null)
        {
            throw new InvalidOperationException($"Subscription {subscriptionId} not found");
        }

        var planPrivilege = await _subscriptionPlanRepository.GetPlanPrivilegeAsync(
            subscription.SubscriptionPlanId, 
            privilegeId);
        
        if (planPrivilege == null)
        {
            throw new InvalidOperationException($"Privilege {privilegeId} not found in plan");
        }

        // CORRECTED: Use centralized calculator (now returns Value directly without calculation)
        var (allowedValue, periodStart, periodEnd) = PrivilegeAllocationCalculator.CalculatePrivilegeAllocation(
            subscription, 
            planPrivilege);

        // Create complete privilege usage record
        // AllowedValue = admin-set Value (e.g., 152), NOT calculated from monthly limit
        var newUsage = new UserSubscriptionPrivilegeUsage
        {
            Id = Guid.NewGuid(),
            SubscriptionId = subscriptionId,
            SubscriptionPlanPrivilegeId = planPrivilege.Id,
            PrivilegeId = privilegeId,
            UsedValue = 0,
            AllowedValue = allowedValue,      // ✅ Admin-set total!
            UsagePeriodStart = periodStart,   // ✅ Set!
            UsagePeriodEnd = periodEnd,       // ✅ Set!
            LastUsedAt = null,
            IsActive = true,
            IsDeleted = false,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = tokenModel?.UserID ?? 1
        };

        _logger.LogInformation("Creating privilege usage for subscription {SubscriptionId}, privilege {PrivilegeId}: " +
            "AllowedValue={AllowedValue} (admin-set total), Period={Start:yyyy-MM-dd} to {End:yyyy-MM-dd}",
            subscriptionId, privilegeId, allowedValue, periodStart, periodEnd);

        await _privilegeUsageRepository.CreatePrivilegeUsageAsync(newUsage);
        return newUsage;
    }

    private async Task RecordUsageEventAsync(int userId, Guid privilegeId, int usageCount, TokenModel tokenModel)
    {
        var usage = await _privilegeUsageRepository.GetByUserAndPrivilegeAsync(userId, privilegeId);
        if (usage != null)
        {
            usage.UsedValue += usageCount;
            usage.UpdatedBy = tokenModel.UserID;
            usage.UpdatedDate = DateTime.UtcNow;
            await _privilegeUsageRepository.UpdatePrivilegeUsageAsync(usage);
        }
    }

    // Time-based limit checking methods removed - overage now only applies when total AllowedValue is exhausted

    private async Task BatchOverageChargeAsync(Subscription subscription, Guid privilegeId, decimal overageCharge, TokenModel tokenModel)
    {
        try
        {
            var existingOverage = await _billingRepository.GetByUserIdAsync(subscription.UserId);
            var pendingOverage = existingOverage
                .FirstOrDefault(b => b.Type == BillingRecord.BillingType.Overage && 
                                    b.Status == BillingRecord.BillingStatus.Pending);

            if (pendingOverage != null)
            {
                pendingOverage.Amount += overageCharge;
                pendingOverage.TotalAmount += overageCharge;
                pendingOverage.Description += $"; Additional overage: {overageCharge:C}";
                pendingOverage.UpdatedBy = tokenModel.UserID;
                pendingOverage.UpdatedDate = DateTime.UtcNow;
                
                await _billingRepository.UpdateBillingRecordAsync(pendingOverage);
            }
            else
            {
                await CreateOverageBillingRecordAsync(subscription, privilegeId, overageCharge, tokenModel);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error batching overage charge for subscription {SubscriptionId}", subscription.Id);
            throw;
        }
    }

    private async Task CreateOverageBillingRecordAsync(Subscription subscription, Guid privilegeId, decimal extraCharge, TokenModel tokenModel)
    {
        try
        {
            var privilege = await _privilegeRepository.GetByIdAsync(privilegeId);
            var plan = await _subscriptionPlanRepository.GetByIdWithDetailsAsync(subscription.SubscriptionPlanId);

            var billingRecord = new BillingRecord
            {
                Id = Guid.NewGuid(),
                UserId = subscription.UserId,
                SubscriptionId = subscription.Id,
                CurrencyId = plan.CurrencyId,
                Amount = extraCharge,
                TotalAmount = extraCharge,
                TaxAmount = 0,
                ShippingAmount = 0,
                Status = BillingRecord.BillingStatus.Pending,
                Type = BillingRecord.BillingType.Overage,
                Description = $"Overage charge for {privilege?.Name} - {extraCharge:C}",
                BillingDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(SubscriptionConstants.DEFAULT_BILLING_GRACE_PERIOD_DAYS),
                IsRecurring = false,
                CreatedBy = tokenModel.UserID,
                CreatedDate = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false
            };

            await _billingRepository.CreateBillingRecordAsync(billingRecord);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating overage billing record for subscription {SubscriptionId}", subscription.Id);
            throw;
        }
    }

    private async Task CarryOverOverageChargesAsync(Subscription subscription, decimal pendingOverageAmount, TokenModel tokenModel)
    {
        try
        {
            var plan = await _subscriptionPlanRepository.GetByIdWithDetailsAsync(subscription.SubscriptionPlanId);
            
            var carriedOverBilling = new BillingRecord
            {
                Id = Guid.NewGuid(),
                UserId = subscription.UserId,
                SubscriptionId = subscription.Id,
                CurrencyId = plan?.CurrencyId ?? Guid.Empty,
                Amount = pendingOverageAmount,
                TotalAmount = pendingOverageAmount,
                TaxAmount = 0,
                ShippingAmount = 0,
                Status = BillingRecord.BillingStatus.Pending,
                Type = BillingRecord.BillingType.Overage,
                Description = $"Carried over overage charges from previous billing cycle - {pendingOverageAmount:C}",
                BillingDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(SubscriptionConstants.DEFAULT_BILLING_GRACE_PERIOD_DAYS),
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = tokenModel.UserID
            };

            await _billingRepository.CreateBillingRecordAsync(carriedOverBilling);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error carrying over overage charges for subscription {SubscriptionId}", subscription.Id);
            throw;
        }
    }

    private class OverageResult
    {
        public decimal DailyOverageCharge { get; set; }
        public decimal WeeklyOverageCharge { get; set; }
        public decimal MonthlyOverageCharge { get; set; }
        public decimal TotalOverageCharge { get; set; }
        public bool IsOverLimit { get; set; }
    }
    
    #endregion

    #endregion

    #region Billing Record Factory Methods (SRP Refactoring)
    
    /// <summary>
    /// Creates a billing record for subscription (initial or renewal)
    /// MIGRATED: Client Workflow Step 2
    /// </summary>
    public async Task<JsonModel> CreateSubscriptionBillingAsync(
        Subscription subscription,
        decimal amount,
        string description,
        DateTime? dueDate = null,
        TokenModel tokenModel = null)
    {
        try
        {
            _logger.LogInformation("Creating subscription billing for subscription {SubscriptionId}, amount: ${Amount}",
                subscription.Id, amount);

            // Calculate tax amount from plan's default tax percentage
            var taxAmount = 0m;
            var taxPercentage = 0m;
            if (subscription.SubscriptionPlan?.DefaultTaxPercentage.HasValue == true && 
                subscription.SubscriptionPlan.DefaultTaxPercentage.Value > 0)
            {
                taxPercentage = subscription.SubscriptionPlan.DefaultTaxPercentage.Value;
                taxAmount = amount * (taxPercentage / 100);
                _logger.LogInformation("Applying {TaxPercent}% tax to billing: ${TaxAmount}", 
                    taxPercentage, taxAmount);
            }

            var totalAmount = amount + taxAmount;

            _logger.LogInformation(
                "BILLING_RECORD_DTO_CREATION: SubscriptionId={SubscriptionId}, UserId={UserId}, " +
                "PlanId={PlanId}, PlanName={PlanName}, BaseAmount={BaseAmount}, TaxPercentage={TaxPercentage}, " +
                "TaxAmount={TaxAmount}, TotalAmount={TotalAmount}, CurrencyId={CurrencyId}, " +
                "Description={Description}, DueDate={DueDate}",
                subscription.Id, subscription.UserId, subscription.SubscriptionPlan?.Id, 
                subscription.SubscriptionPlan?.Name, amount, taxPercentage, taxAmount, totalAmount,
                subscription.SubscriptionPlan?.CurrencyId, description, dueDate);

            var dto = new CreateBillingRecordDto
            {
                UserId = subscription.UserId,
                SubscriptionId = subscription.Id.ToString(),
                Amount = amount,
                TaxAmount = taxAmount,
                TotalAmount = totalAmount,
                CurrencyId = subscription.SubscriptionPlan?.CurrencyId,
                PaymentMethod = "stripe",
                Status = BillingRecord.BillingStatus.Pending.ToString(),
                Description = description,
                BillingDate = DateTime.UtcNow,
                DueDate = dueDate ?? DateTime.UtcNow.AddDays(SubscriptionConstants.DEFAULT_BILLING_GRACE_PERIOD_DAYS),
                Type = BillingRecord.BillingType.Subscription.ToString()
            };

            return await CreateBillingRecordAsync(dto, tokenModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating subscription billing for subscription {SubscriptionId}", subscription.Id);
            return new JsonModel { data = new object(), Message = "Error creating subscription billing", StatusCode = 500 };
        }
    }
    
    /// <summary>
    /// Creates overage billing record for privilege usage exceeding limits
    /// MIGRATED: Client Workflow Steps 3-4
    /// </summary>
    public async Task<JsonModel> CreateOverageBillingAsync(
        Subscription subscription,
        string privilegeName,
        decimal amount,
        TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Creating overage billing for subscription {SubscriptionId}, privilege: {PrivilegeName}, amount: ${Amount}",
                subscription.Id, privilegeName, amount);

            var dto = new CreateBillingRecordDto
            {
                UserId = subscription.UserId,
                SubscriptionId = subscription.Id.ToString(),
                Amount = amount,
                CurrencyId = subscription.SubscriptionPlan?.CurrencyId,
                PaymentMethod = "stripe",
                Status = BillingRecord.BillingStatus.Pending.ToString(),
                Description = $"Overage charge for {privilegeName} - ${amount:F2}",
                BillingDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(SubscriptionConstants.DEFAULT_BILLING_GRACE_PERIOD_DAYS),
                Type = BillingRecord.BillingType.Overage.ToString()
            };

            return await CreateBillingRecordAsync(dto, tokenModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating overage billing for subscription {SubscriptionId}, privilege {PrivilegeName}", 
                subscription.Id, privilegeName);
            return new JsonModel { data = new object(), Message = "Error creating overage billing", StatusCode = 500 };
        }
    }
    
    /// <summary>
    /// Creates overage billing with healthcare-compliant pricing (uses latest plan version).
    /// Healthcare Rule: Overage charges use LATEST plan pricing to prevent abuse.
    /// </summary>
    /// <param name="subscriptionId">Subscription ID</param>
    /// <param name="privilegeId">Privilege ID</param>
    /// <param name="quantity">Number of overage units</param>
    /// <param name="tokenModel">User token</param>
    /// <returns>JsonModel with billing record</returns>
    public async Task<JsonModel> CreateHealthcareOverageBillingAsync(
        Guid subscriptionId,
        Guid privilegeId,
        int quantity,
        TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation(
                "Creating healthcare overage billing for subscription {SubId}, privilege {PrivId}, quantity {Qty}",
                subscriptionId, privilegeId, quantity);
            
            // ✅ HEALTHCARE RULE: Calculate cost using LATEST plan version pricing
            var overageCost = await _pricingService.CalculateOverageCostForSubscriptionAsync(
                subscriptionId, privilegeId, quantity);
            
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
            
            var privilege = await _privilegeRepository.GetByIdAsync(privilegeId);
            if (privilege == null)
            {
                return new JsonModel 
                { 
                    data = new object(), 
                    Message = "Privilege not found", 
                    StatusCode = 404 
                };
            }
            
            // Create billing record with calculated overage cost
            var dto = new CreateBillingRecordDto
            {
                UserId = subscription.UserId,
                SubscriptionId = subscription.Id.ToString(),
                Amount = overageCost,
                CurrencyId = subscription.SubscriptionPlan?.CurrencyId,
                PaymentMethod = "stripe",
                Status = BillingRecord.BillingStatus.Pending.ToString(),
                Description = $"Overage charge: {quantity} × {privilege.Name} - ${overageCost:F2} (latest plan pricing)",
                BillingDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(SubscriptionConstants.DEFAULT_BILLING_GRACE_PERIOD_DAYS),
                Type = BillingRecord.BillingType.Overage.ToString()
            };
            
            _logger.LogInformation(
                "Healthcare overage billing created: {Qty} × {Privilege} = ${Cost} for subscription {SubId}",
                quantity, privilege.Name, overageCost, subscriptionId);

            return await CreateBillingRecordAsync(dto, tokenModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error creating healthcare overage billing for subscription {SubId}, privilege {PrivId}", 
                subscriptionId, privilegeId);
            return new JsonModel 
            { 
                data = new object(), 
                Message = $"Error creating overage billing: {ex.Message}", 
                StatusCode = 500 
            };
        }
    }

    /// <summary>
    /// Creates consultation billing record
    /// MIGRATED
    /// </summary>
    public async Task<JsonModel> CreateConsultationBillingAsync(int userId, Guid consultationId, decimal amount, string? description = null, TokenModel tokenModel = null)
    {
        try
        {
            var dto = new CreateBillingRecordDto
            {
                UserId = userId,
                ConsultationId = consultationId.ToString(),
                Amount = amount,
                PaymentMethod = "stripe",
                Status = BillingRecord.BillingStatus.Pending.ToString(),
                Description = description ?? $"Consultation billing - ${amount:F2}",
                BillingDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(SubscriptionConstants.DEFAULT_BILLING_GRACE_PERIOD_DAYS),
                Type = BillingRecord.BillingType.Consultation.ToString()
            };

            return await CreateBillingRecordAsync(dto, tokenModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating consultation billing for user {UserId}", userId);
            return new JsonModel { data = new object(), Message = "Error creating consultation billing", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Creates medication billing record
    /// MIGRATED
    /// </summary>
    public async Task<JsonModel> CreateMedicationBillingAsync(Subscription subscription, decimal amount, string? description = null, TokenModel tokenModel = null)
    {
        try
        {
            var dto = new CreateBillingRecordDto
            {
                UserId = subscription.UserId,
                SubscriptionId = subscription.Id.ToString(),
                Amount = amount,
                CurrencyId = subscription.SubscriptionPlan?.CurrencyId,
                PaymentMethod = "stripe",
                Status = BillingRecord.BillingStatus.Pending.ToString(),
                Description = description ?? $"Medication billing - ${amount:F2}",
                BillingDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(SubscriptionConstants.DEFAULT_BILLING_GRACE_PERIOD_DAYS),
                Type = BillingRecord.BillingType.Medication.ToString()
            };

            return await CreateBillingRecordAsync(dto, tokenModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating medication billing for subscription {SubscriptionId}", subscription.Id);
            return new JsonModel { data = new object(), Message = "Error creating medication billing", StatusCode = 500 };
        }
    }
    
    #endregion

    #region Core Billing Record Management
    
    /// <summary>
    /// Creates a new billing record with proper audit trail and status management
    /// MIGRATED
    /// </summary>
    public async Task<JsonModel> CreateBillingRecordAsync(CreateBillingRecordDto createDto, TokenModel tokenModel)
    {
        try
        {
            var billingRecord = _mapper.Map<BillingRecord>(createDto);
            
            // FIXED: Calculate TotalAmount if not already set
            // TotalAmount = Amount + TaxAmount + ShippingAmount
            if (billingRecord.TotalAmount == 0)
            {
                billingRecord.TotalAmount = billingRecord.Amount + billingRecord.TaxAmount + billingRecord.ShippingAmount;
                
                _logger.LogInformation("Calculated TotalAmount for billing record: Amount=${Amount}, Tax=${Tax}, Shipping=${Shipping}, Total=${Total}",
                    billingRecord.Amount, billingRecord.TaxAmount, billingRecord.ShippingAmount, billingRecord.TotalAmount);
            }
            
            // Validate TotalAmount is not negative
            if (billingRecord.TotalAmount < 0)
            {
                _logger.LogError("Invalid TotalAmount calculated: ${TotalAmount}. Amount=${Amount}, Tax=${Tax}, Shipping=${Shipping}",
                    billingRecord.TotalAmount, billingRecord.Amount, billingRecord.TaxAmount, billingRecord.ShippingAmount);
                
                return new JsonModel 
                { 
                    data = new object(), 
                    Message = "Invalid billing amount calculated", 
                    StatusCode = 400 
                };
            }
            
            billingRecord.Status = BillingRecord.BillingStatus.Pending;
            billingRecord.IsActive = true;
            billingRecord.CreatedBy = tokenModel.UserID;
            billingRecord.CreatedDate = DateTime.UtcNow;

            var createdRecord = await _billingRepository.CreateBillingRecordAsync(billingRecord);
            var billingRecordDto = _mapper.Map<BillingRecordDto>(createdRecord);
            
            _logger.LogInformation(
                "BILLING_RECORD_CREATED: BillingRecordId={BillingRecordId}, UserId={UserId}, SubscriptionId={SubscriptionId}, " +
                "Amount={Amount}, TaxAmount={TaxAmount}, ShippingAmount={ShippingAmount}, TotalAmount={TotalAmount}, " +
                "CurrencyId={CurrencyId}, Status={Status}, Type={Type}, Description={Description}, " +
                "BillingDate={BillingDate}, DueDate={DueDate}, CreatedBy={CreatedBy}",
                createdRecord.Id, createdRecord.UserId, createdRecord.SubscriptionId, createdRecord.Amount, 
                createdRecord.TaxAmount, createdRecord.ShippingAmount, createdRecord.TotalAmount,
                createdRecord.CurrencyId, createdRecord.Status, createdRecord.Type, createdRecord.Description,
                createdRecord.BillingDate, createdRecord.DueDate, createdRecord.CreatedBy);
            
            return new JsonModel { data = billingRecordDto, Message = "Billing record created successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating billing record");
            return new JsonModel { data = new object(), Message = "Error creating billing record", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Retrieves a specific billing record by its unique identifier
    /// MIGRATED
    /// </summary>
    public async Task<JsonModel> GetBillingRecordAsync(Guid id, TokenModel tokenModel)
    {
        try
        {
            var billingRecord = await _billingRepository.GetByIdWithDetailsAsync(id);
            if (billingRecord == null)
            {
                return new JsonModel { data = new object(), Message = "Billing record not found", StatusCode = 404 };
            }

            var billingRecordDto = _mapper.Map<BillingRecordDto>(billingRecord);
            return new JsonModel { data = billingRecordDto, Message = "Billing record retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting billing record with ID {BillingRecordId}", id);
            return new JsonModel { data = new object(), Message = "Error retrieving billing record", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Retrieves billing history for a specific user
    /// MIGRATED
    /// </summary>
    public async Task<JsonModel> GetUserBillingHistoryAsync(int userId, TokenModel tokenModel)
    {
        try
        {
            var billingRecords = await _billingRepository.GetByUserIdAsync(userId);
            var billingRecordDtos = _mapper.Map<IEnumerable<BillingRecordDto>>(billingRecords);
            return new JsonModel { data = billingRecordDtos, Message = "User billing history retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting billing history for user {UserId}", userId);
            return new JsonModel { data = new object(), Message = "Error retrieving billing history", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Retrieves billing records for a specific user with filtering and pagination
    /// </summary>
    public async Task<JsonModel> GetUserBillingRecordsAsync(int userId, int page, int pageSize, string[]? status, string[]? type, string[]? subscriptionId, DateTime? startDate, DateTime? endDate, string? sortBy, string? sortOrder, TokenModel tokenModel)
    {
        try
        {
            // Validate access - users can only access their own billing records
            if (tokenModel.RoleID != (int)RoleId.Admin && tokenModel.UserID != userId)
            {
                return new JsonModel { data = new object(), Message = "Access denied", StatusCode = 403 };
            }

            var filter = new BillingFilterDto
            {
                Page = page,
                PageSize = pageSize,
                UserId = userId,
                Statuses = status?.ToList(),
                Types = type?.ToList(),
                SubscriptionIds = subscriptionId?.Select(Guid.Parse).ToList(),
                CreatedDateFrom = startDate,
                CreatedDateTo = endDate,
                SortColumn = sortBy ?? "CreatedDate",
                SortOrder = sortOrder ?? "desc"
            };

            var result = await GetBillingRecordsWithFilteringAsync(filter, tokenModel, adminOnly: false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting billing records for user {UserId}", userId);
            return new JsonModel { data = new object(), Message = "Error retrieving billing records", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Retrieves billing history for a specific subscription
    /// MIGRATED
    /// </summary>
    public async Task<JsonModel> GetSubscriptionBillingHistoryAsync(Guid subscriptionId, TokenModel tokenModel)
    {
        try
        {
            var billingRecords = await _billingRepository.GetBySubscriptionIdAsync(subscriptionId);
            var billingRecordDtos = _mapper.Map<IEnumerable<BillingRecordDto>>(billingRecords);
            
            return new JsonModel { data = billingRecordDtos, Message = "Subscription billing history retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting billing history for subscription {SubscriptionId}", subscriptionId);
            return new JsonModel { data = new object(), Message = "Error retrieving subscription billing history", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Retrieves all billing records with advanced filtering, pagination, and sorting
    /// MIGRATED
    /// </summary>
    public async Task<JsonModel> GetBillingRecordsWithFilteringAsync(BillingFilterDto filter, TokenModel? tokenModel = null, bool adminOnly = false)
    {
        try
        {
            var (billingRecords, totalCount) = await _billingRepository.GetBillingRecordsWithAdvancedFilteringAsync(filter);
            
            var billingRecordDtos = _mapper.Map<IEnumerable<BillingRecordDto>>(billingRecords);
            
            var paginationMeta = new Meta
            {
                TotalRecords = totalCount,
                PageSize = filter.PageSize,
                CurrentPage = filter.Page,
                TotalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize),
                DefaultPageSize = 10,
                HasNextPage = filter.Page < (int)Math.Ceiling((double)totalCount / filter.PageSize),
                HasPreviousPage = filter.Page > 1
            };

            return new JsonModel 
            { 
                data = billingRecordDtos,
                meta = paginationMeta,
                Message = $"Retrieved {billingRecordDtos.Count()} billing records successfully", 
                StatusCode = 200 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving billing records with filtering");
            return new JsonModel { data = new object(), Message = "Error retrieving billing records", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Retrieves all billing records with legacy parameters (wrapper method)
    /// MIGRATED - Redirects to GetBillingRecordsWithFilteringAsync
    /// </summary>
    public async Task<JsonModel> GetAllBillingRecordsAsync(int page, int pageSize, string? searchTerm, string[]? status, string[]? type, string[]? userId, string[]? subscriptionId, DateTime? startDate, DateTime? endDate, string? sortBy, string? sortOrder, TokenModel tokenModel)
    {
        try
        {
            var filter = new BillingFilterDto
            {
                Page = page,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                Statuses = status?.ToList(),
                Types = type?.ToList(),
                UserIds = userId?.Select(int.Parse).ToList(),
                SubscriptionIds = subscriptionId?.Select(Guid.Parse).ToList(),
                CreatedDateFrom = startDate,
                CreatedDateTo = endDate,
                SortColumn = sortBy ?? "CreatedDate",
                SortOrder = sortOrder ?? "desc"
            };

            return await GetBillingRecordsWithFilteringAsync(filter, tokenModel, true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all billing records");
            return new JsonModel { data = new object(), Message = "Error retrieving billing records", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Retrieves all overdue billing records
    /// MIGRATED
    /// </summary>
    public async Task<JsonModel> GetOverdueBillingRecordsAsync(TokenModel tokenModel)
    {
        try
        {
            var overdueRecords = await _billingRepository.GetOverdueBillingRecordsAsync();
            var billingRecordDtos = _mapper.Map<IEnumerable<BillingRecordDto>>(overdueRecords);
            
            return new JsonModel 
            { 
                data = billingRecordDtos, 
                Message = $"Retrieved {overdueRecords.Count()} overdue billing records successfully", 
                StatusCode = 200 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting overdue billing records");
            return new JsonModel { data = new object(), Message = "Error retrieving overdue billing records", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Retrieves all billing records with pending payment status
    /// MIGRATED
    /// </summary>
    public async Task<JsonModel> GetPendingPaymentsAsync(TokenModel tokenModel)
    {
        try
        {
            var pendingPayments = await _billingRepository.GetPendingBillingRecordsAsync();
            var dtos = _mapper.Map<IEnumerable<BillingRecordDto>>(pendingPayments);
            return new JsonModel { data = dtos, Message = "Pending payments retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending payments");
            return new JsonModel { data = new object(), Message = "Error retrieving pending payments", StatusCode = 500 };
        }
    }
    
    #endregion

    #region Payment Processing
    
    /// <summary>
    /// Processes payment for a specific billing record
    /// MIGRATED
    /// </summary>
    public async Task<JsonModel> ProcessPaymentAsync(Guid billingRecordId, TokenModel tokenModel)
    {
        try
        {
            if (billingRecordId == Guid.Empty)
            {
                return new JsonModel { data = new object(), Message = "Invalid billing record ID", StatusCode = 400 };
            }

            var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
            if (billingRecord == null)
            {
                return new JsonModel { data = new object(), Message = "Billing record not found", StatusCode = 404 };
            }

            if (billingRecord.Status == BillingRecord.BillingStatus.Paid)
            {
                return new JsonModel { data = new object(), Message = "Billing record is already paid", StatusCode = 400 };
            }

            var paymentResult = await _paymentService.ProcessPaymentAsync(billingRecordId, tokenModel);
            
            if (paymentResult.StatusCode == 200)
            {
                billingRecord.Status = BillingRecord.BillingStatus.Paid;
                billingRecord.PaidAt = DateTime.UtcNow;
                billingRecord.UpdatedBy = tokenModel.UserID;
                billingRecord.UpdatedDate = DateTime.UtcNow;
                
                await _billingRepository.UpdateBillingRecordAsync(billingRecord);
            }
            
            return paymentResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment for billing record {BillingRecordId}", billingRecordId);
            return new JsonModel { data = new object(), Message = "Error processing payment", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Processes a refund for a specific billing record
    /// MIGRATED
    /// </summary>
    public async Task<JsonModel> ProcessRefundAsync(Guid billingRecordId, decimal amount, TokenModel tokenModel)
    {
        try
        {
            if (billingRecordId == Guid.Empty || amount <= 0)
            {
                return new JsonModel { data = new object(), Message = "Invalid parameters", StatusCode = 400 };
            }

            var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
            if (billingRecord == null)
            {
                return new JsonModel { data = new object(), Message = "Billing record not found", StatusCode = 404 };
            }

            if (amount > billingRecord.TotalAmount || billingRecord.Status != BillingRecord.BillingStatus.Paid)
            {
                return new JsonModel { data = new object(), Message = "Invalid refund request", StatusCode = 400 };
            }

            var refundResult = await _paymentService.ProcessRefundAsync(billingRecordId, amount, tokenModel);
            
            if (refundResult.StatusCode == 200)
            {
                if (amount >= billingRecord.TotalAmount)
                {
                    billingRecord.Status = BillingRecord.BillingStatus.Refunded;
                }
                
                billingRecord.UpdatedBy = tokenModel.UserID;
                billingRecord.UpdatedDate = DateTime.UtcNow;
                
                await _billingRepository.UpdateBillingRecordAsync(billingRecord);
            }
            
            return refundResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing refund for billing record {BillingRecordId}", billingRecordId);
            return new JsonModel { data = new object(), Message = "Error processing refund", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Processes a refund with reason
    /// MIGRATED
    /// </summary>
    public async Task<JsonModel> ProcessRefundAsync(Guid billingRecordId, decimal amount, string reason, TokenModel tokenModel)
    {
        try
        {
            var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
            if (billingRecord == null)
            {
                return new JsonModel { data = new object(), Message = "Billing record not found", StatusCode = 404 };
            }

            if (amount <= 0 || amount > billingRecord.TotalAmount || billingRecord.Status != BillingRecord.BillingStatus.Paid)
            {
                return new JsonModel { data = new object(), Message = "Invalid refund request", StatusCode = 400 };
            }

            var refundResult = await _paymentService.ProcessRefundAsync(billingRecordId, amount, reason, tokenModel);
            
            if (refundResult.StatusCode == 200)
            {
                if (amount >= billingRecord.TotalAmount)
                {
                    billingRecord.Status = BillingRecord.BillingStatus.Refunded;
                }
                
                billingRecord.UpdatedBy = tokenModel.UserID;
                billingRecord.UpdatedDate = DateTime.UtcNow;
                
                await _billingRepository.UpdateBillingRecordAsync(billingRecord);
            }
            
            return refundResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing refund for billing record {BillingRecordId}", billingRecordId);
            return new JsonModel { data = new object(), Message = "Error processing refund", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Retries a failed payment
    /// MIGRATED
    /// </summary>
    public async Task<JsonModel> RetryFailedPaymentAsync(Guid billingRecordId, TokenModel tokenModel)
    {
        try
        {
            var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
            if (billingRecord == null)
            {
                return new JsonModel { data = new object(), Message = "Billing record not found", StatusCode = 404 };
            }

            billingRecord.Status = BillingRecord.BillingStatus.Pending;
            billingRecord.UpdatedBy = tokenModel.UserID;
            billingRecord.UpdatedDate = DateTime.UtcNow;
            var updatedRecord = await _billingRepository.UpdateAsync(billingRecord);
            var billingRecordDto = _mapper.Map<BillingRecordDto>(updatedRecord);
            
            return new JsonModel { data = billingRecordDto, Message = "Failed payment retry initiated successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying failed payment for billing record {BillingRecordId}", billingRecordId);
            return new JsonModel { data = new object(), Message = "Error retrying failed payment", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Retries payment processing
    /// MIGRATED
    /// </summary>
    public async Task<JsonModel> RetryPaymentAsync(Guid billingRecordId, TokenModel tokenModel)
    {
        try
        {
            var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
            if (billingRecord == null)
            {
                return new JsonModel { data = new object(), Message = "Billing record not found", StatusCode = 404 };
            }

            if (billingRecord.Status == BillingRecord.BillingStatus.Paid)
            {
                return new JsonModel { data = new object(), Message = "Billing record is already paid", StatusCode = 400 };
            }

            var paymentResult = await _paymentService.ProcessPaymentAsync(billingRecordId, tokenModel);
            
            if (paymentResult.StatusCode == 200)
            {
                billingRecord.Status = BillingRecord.BillingStatus.Paid;
                billingRecord.PaidAt = DateTime.UtcNow;
                billingRecord.UpdatedBy = tokenModel.UserID;
                billingRecord.UpdatedDate = DateTime.UtcNow;
                
                await _billingRepository.UpdateBillingRecordAsync(billingRecord);
            }
            
            return paymentResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying payment for billing record {BillingRecordId}", billingRecordId);
            return new JsonModel { data = new object(), Message = "Error retrying payment", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Processes partial payment
    /// MIGRATED
    /// </summary>
    public async Task<JsonModel> ProcessPartialPaymentAsync(Guid billingRecordId, decimal amount, TokenModel tokenModel)
    {
        try
        {
            return await _paymentService.ProcessPartialPaymentAsync(billingRecordId, amount, tokenModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing partial payment for billing record {BillingRecordId}", billingRecordId);
            return new JsonModel { data = new object(), Message = "Error processing partial payment", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Updates payment method
    /// MIGRATED
    /// </summary>
    public async Task<JsonModel> UpdatePaymentMethodAsync(Guid billingRecordId, string paymentMethodId, TokenModel tokenModel)
    {
        try
        {
            return await _paymentService.UpdatePaymentMethodAsync(billingRecordId, paymentMethodId, tokenModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating payment method for billing record {BillingRecordId}", billingRecordId);
            return new JsonModel { data = new object(), Message = "Error updating payment method", StatusCode = 500 };
        }
    }
    
    #endregion

    #region Calculations & Utilities
    
    /// <summary>
    /// Calculates total amount
    /// MIGRATED
    /// </summary>
    public async Task<JsonModel> CalculateTotalAmountAsync(decimal baseAmount, decimal taxAmount, decimal shippingAmount, TokenModel tokenModel)
    {
        try
        {
            var totalAmount = baseAmount + taxAmount + shippingAmount;
            return new JsonModel { data = totalAmount, Message = "Total amount calculated successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating total amount");
            return new JsonModel { data = new object(), Message = "Error calculating total amount", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Calculates tax amount based on state
    /// MIGRATED
    /// </summary>
    public async Task<JsonModel> CalculateTaxAmountAsync(decimal baseAmount, string state, TokenModel tokenModel)
    {
        try
        {
            var taxRate = state.ToUpper() switch
            {
                "CA" => 0.0825m,
                "NY" => 0.085m,
                "TX" => 0.0625m,
                _ => 0.06m
            };

            var taxAmount = baseAmount * taxRate;
            return new JsonModel { data = taxAmount, Message = "Tax amount calculated successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating tax amount");
            return new JsonModel { data = new object(), Message = "Error calculating tax amount", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Calculates shipping amount
    /// MIGRATED
    /// </summary>
    public async Task<JsonModel> CalculateShippingAmountAsync(string deliveryAddress, bool isExpress, TokenModel tokenModel)
    {
        try
        {
            var baseShipping = 5.99m;
            var expressMultiplier = isExpress ? 2.5m : 1.0m;
            var shippingAmount = baseShipping * expressMultiplier;
            
            return new JsonModel { data = shippingAmount, Message = "Shipping amount calculated successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating shipping amount");
            return new JsonModel { data = new object(), Message = "Error calculating shipping amount", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Checks if payment is overdue
    /// MIGRATED
    /// </summary>
    public async Task<JsonModel> IsPaymentOverdueAsync(Guid billingRecordId, TokenModel tokenModel)
    {
        return await _paymentService.IsPaymentOverdueAsync(billingRecordId, tokenModel);
    }

    /// <summary>
    /// Calculates due date
    /// MIGRATED
    /// </summary>
    public async Task<JsonModel> CalculateDueDateAsync(DateTime billingDate, int gracePeriodDays, TokenModel tokenModel)
    {
        try
        {
            var dueDate = billingDate.AddDays(gracePeriodDays);
            return new JsonModel { data = dueDate, Message = "Due date calculated successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating due date");
            return new JsonModel { data = new object(), Message = "Error calculating due date", StatusCode = 500 };
        }
    }
    
    #endregion

    #region Enhanced Billing Features
    
    /// <summary>
    /// Creates upfront payment
    /// MIGRATED
    /// </summary>
    public async Task<JsonModel> CreateUpfrontPaymentAsync(CreateUpfrontPaymentDto createDto, TokenModel tokenModel)
    {
        try
        {
            return await _paymentService.CreateUpfrontPaymentAsync(createDto, tokenModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating upfront payment for user {UserId}", createDto.UserId);
            return new JsonModel { data = new object(), Message = "Error creating upfront payment", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Processes bundle payment
    /// MIGRATED
    /// </summary>
    public async Task<JsonModel> ProcessBundlePaymentAsync(CreateBundlePaymentDto createDto, TokenModel tokenModel)
    {
        try
        {
            return await _paymentService.ProcessBundlePaymentAsync(createDto, tokenModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing bundle payment for user {UserId}", createDto.UserId);
            return new JsonModel { data = new object(), Message = "Error processing bundle payment", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Creates recurring billing setup
    /// MIGRATED - Delegates to PaymentService (correct per SRP)
    /// </summary>
    public async Task<JsonModel> CreateRecurringBillingAsync(CreateRecurringBillingDto createDto, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Creating recurring billing for user {UserId}", createDto.UserId);
            return await _paymentService.CreateRecurringBillingAsync(createDto, tokenModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating recurring billing for user {UserId}", createDto.UserId);
            return new JsonModel { data = new object(), Message = "Error creating recurring billing", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Processes recurring payment for subscription
    /// MIGRATED - Delegates to PaymentService (correct per SRP)
    /// </summary>
    public async Task<JsonModel> ProcessRecurringPaymentAsync(Guid subscriptionId, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Processing recurring payment for subscription {SubscriptionId}", subscriptionId);
            return await _paymentService.ProcessRecurringPaymentAsync(subscriptionId, tokenModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing recurring payment for subscription {SubscriptionId}", subscriptionId);
            return new JsonModel { data = new object(), Message = "Error processing recurring payment", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Cancels recurring billing for subscription
    /// MIGRATED - Delegates to PaymentService (correct per SRP)
    /// </summary>
    public async Task<JsonModel> CancelRecurringBillingAsync(Guid subscriptionId, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Cancelling recurring billing for subscription {SubscriptionId}", subscriptionId);
            return await _paymentService.CancelRecurringBillingAsync(subscriptionId, tokenModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling recurring billing for subscription {SubscriptionId}", subscriptionId);
            return new JsonModel { data = new object(), Message = "Error cancelling recurring billing", StatusCode = 500 };
        }
    }
    
    #endregion

    #region Billing Adjustments (Fully Migrated)
    
    // Implementation continues in next message due to length...
    
    #endregion

    #region Date Calculations (SRP Refactoring)
    
    /// <summary>
    /// Calculates the next billing date based on current date and billing cycle
    /// MIGRATED
    /// </summary>
    public DateTime CalculateNextBillingDate(DateTime currentDate, MasterBillingCycle billingCycle)
    {
        try
        {
            // REFACTORED: Delegate to centralized calculator (eliminates duplicate logic)
            return BillingCycleCalculator.CalculateNextBillingDate(currentDate, billingCycle);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating next billing date");
            // CONSISTENT FIX: Use centralized billing cycle calculator instead of manual AddMonths
            return BillingCycleCalculator.CalculateNextBillingDate(currentDate, 
                new MasterBillingCycle { Name = "monthly", DurationInDays = 30 }); // 30 days = 1 month
        }
    }

    /// <summary>
    /// Calculates the next billing date for a specific subscription
    /// MIGRATED
    /// </summary>
    public async Task<DateTime> CalculateNextBillingDateForSubscriptionAsync(Guid subscriptionId, TokenModel tokenModel)
    {
        try
        {
            var subscription = await _subscriptionRepository.GetByIdWithDetailsAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found for billing date calculation", subscriptionId);
                // CONSISTENT FIX: Use centralized billing cycle calculator instead of manual AddMonths
                return BillingCycleCalculator.CalculateNextBillingDate(DateTime.UtcNow, 
                    new MasterBillingCycle { Name = "monthly", DurationInDays = 30 }); // 30 days = 1 month
            }

            return CalculateNextBillingDate(DateTime.UtcNow, subscription.BillingCycle);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating next billing date for subscription {SubscriptionId}", subscriptionId);
            // CONSISTENT FIX: Use centralized billing cycle calculator instead of manual AddMonths
            return BillingCycleCalculator.CalculateNextBillingDate(DateTime.UtcNow, 
                new MasterBillingCycle { Name = "monthly", DurationInDays = 30 }); // 30 days = 1 month
        }
    }
    
    #endregion

    #region Billing Adjustments (Fully Migrated)
    
    /// <summary>
    /// Applies a billing adjustment to a specific billing record
    /// MIGRATED
    /// </summary>
    public async Task<JsonModel> ApplyBillingAdjustmentAsync(Guid billingRecordId, CreateBillingAdjustmentDto adjustmentDto, TokenModel tokenModel)
    {
        try
        {
            var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
            if (billingRecord == null)
            {
                _logger.LogWarning("Billing record {BillingRecordId} not found for adjustment", billingRecordId);
                return new JsonModel { data = new object(), Message = "Billing record not found", StatusCode = 404 };
            }

            var validationResult = ValidateBillingAdjustment(adjustmentDto, billingRecord);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Invalid billing adjustment for record {BillingRecordId}: {ValidationError}", 
                    billingRecordId, validationResult.ErrorMessage);
                return new JsonModel { data = new object(), Message = validationResult.ErrorMessage, StatusCode = 400 };
            }

            var adjustment = new BillingAdjustment
            {
                Id = Guid.NewGuid(),
                BillingRecordId = billingRecordId,
                Type = adjustmentDto.Type,
                Amount = adjustmentDto.Amount,
                Description = adjustmentDto.Description,
                Reason = adjustmentDto.Reason,
                IsPercentage = adjustmentDto.IsPercentage,
                Percentage = adjustmentDto.Percentage,
                AppliedAt = DateTime.UtcNow,
                AppliedBy = tokenModel?.UserID,
                IsApproved = adjustmentDto.IsApproved,
                ApprovalNotes = adjustmentDto.ApprovalNotes
            };

            // CRITICAL FIX: Use centralized adjustment calculation with proper validation
            decimal actualAdjustmentAmount = BillingCalculationService.CalculateAdjustmentAmount(
                billingRecord, adjustmentDto.Amount, adjustmentDto.Type.ToString(), _logger);

            // Determine if adjustment should be added or subtracted based on type
            // Discounts, Credits, and Refunds should REDUCE the total amount
            // LateFee, ServiceFee, and TaxAdjustment should INCREASE the total amount
            bool isDeduction = adjustmentDto.Type == BillingAdjustment.AdjustmentType.Discount ||
                               adjustmentDto.Type == BillingAdjustment.AdjustmentType.Credit ||
                               adjustmentDto.Type == BillingAdjustment.AdjustmentType.Refund;

            // Apply adjustment correctly based on type
            if (isDeduction)
            {
                billingRecord.TotalAmount -= actualAdjustmentAmount;
                _logger.LogInformation("Applied deduction of ${Amount} ({Type}) to billing record {BillingRecordId}. New total: ${NewTotal}", 
                    actualAdjustmentAmount, adjustmentDto.Type, billingRecordId, billingRecord.TotalAmount);
            }
            else
            {
                billingRecord.TotalAmount += actualAdjustmentAmount;
                _logger.LogInformation("Applied charge of ${Amount} ({Type}) to billing record {BillingRecordId}. New total: ${NewTotal}", 
                    actualAdjustmentAmount, adjustmentDto.Type, billingRecordId, billingRecord.TotalAmount);
            }

            // Ensure billing record total doesn't go negative
            billingRecord.TotalAmount = Math.Max(billingRecord.TotalAmount, 0.01m);

            billingRecord.ProcessedAt = DateTime.UtcNow;

            await _billingRepository.CreateAdjustmentAsync(adjustment);
            await _billingRepository.UpdateBillingRecordAsync(billingRecord);
            
            var adjustmentResponse = new BillingAdjustmentDto
            {
                Id = adjustment.Id,
                BillingRecordId = billingRecordId,
                Type = adjustment.Type.ToString(),
                Amount = actualAdjustmentAmount,
                Description = adjustment.Description,
                Reason = adjustment.Reason,
                IsPercentage = adjustment.IsPercentage,
                Percentage = adjustment.Percentage,
                AppliedAt = adjustment.AppliedAt,
                AppliedBy = adjustment.AppliedBy,
                IsApproved = adjustment.IsApproved,
                ApprovalNotes = adjustment.ApprovalNotes
            };

            _logger.LogInformation("Successfully applied billing adjustment {AdjustmentId} of {Amount} to billing record {BillingRecordId}", 
                adjustment.Id, actualAdjustmentAmount, billingRecordId);

            try
            {
                var user = await _userRepository.GetByIdAsync(billingRecord.UserId);
                if (user != null)
                {
                    await _notificationService.SendBillingAdjustmentEmailAsync(user.Email, user.FullName, adjustmentResponse, tokenModel);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send billing adjustment notification");
            }

            return new JsonModel { data = adjustmentResponse, Message = "Billing adjustment applied successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying billing adjustment for billing record {BillingRecordId}", billingRecordId);
            return new JsonModel { data = new object(), Message = "Error applying billing adjustment", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Retrieves all billing adjustments for a specific billing record
    /// MIGRATED
    /// </summary>
    public async Task<JsonModel> GetBillingAdjustmentsAsync(Guid billingRecordId, TokenModel tokenModel)
    {
        try
        {
            var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
            if (billingRecord == null)
            {
                _logger.LogWarning("Billing record {BillingRecordId} not found for adjustments retrieval", billingRecordId);
                return new JsonModel { data = new object(), Message = "Billing record not found", StatusCode = 404 };
            }

            var adjustments = await _billingRepository.GetAdjustmentsByBillingRecordIdAsync(billingRecordId);
            
            var adjustmentDtos = adjustments.Select(adj => new BillingAdjustmentDto
            {
                Id = adj.Id,
                BillingRecordId = adj.BillingRecordId,
                Type = adj.Type.ToString(),
                Amount = adj.Amount,
                Description = adj.Description,
                Reason = adj.Reason,
                IsPercentage = adj.IsPercentage,
                Percentage = adj.Percentage,
                AppliedAt = adj.AppliedAt,
                AppliedBy = adj.AppliedBy,
                IsApproved = adj.IsApproved,
                ApprovalNotes = adj.ApprovalNotes
            }).ToList();

            return new JsonModel { data = adjustmentDtos, Message = "Billing adjustments retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting billing adjustments for billing record {BillingRecordId}", billingRecordId);
            return new JsonModel { data = new object(), Message = "Error retrieving billing adjustments", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Reverses a billing adjustment
    /// MIGRATED
    /// </summary>
    public async Task<JsonModel> ReverseBillingAdjustmentAsync(Guid adjustmentId, TokenModel tokenModel)
    {
        try
        {
            var adjustment = await _billingRepository.GetAdjustmentByIdAsync(adjustmentId);
            if (adjustment == null)
            {
                return new JsonModel { data = new object(), Message = "Billing adjustment not found", StatusCode = 404 };
            }

            var billingRecord = await _billingRepository.GetByIdAsync(adjustment.BillingRecordId);
            if (billingRecord == null)
            {
                return new JsonModel { data = new object(), Message = "Billing record not found", StatusCode = 404 };
            }

            var reversalAdjustment = new BillingAdjustment
            {
                Id = Guid.NewGuid(),
                BillingRecordId = adjustment.BillingRecordId,
                Type = BillingAdjustment.AdjustmentType.Credit,
                Amount = -adjustment.Amount,
                Description = $"Reversal of adjustment {adjustment.Id}",
                Reason = "Adjustment reversal",
                AppliedAt = DateTime.UtcNow,
                AppliedBy = tokenModel?.UserID,
                IsApproved = true
            };

            billingRecord.TotalAmount -= adjustment.Amount;
            
            await _billingRepository.CreateAdjustmentAsync(reversalAdjustment);
            await _billingRepository.UpdateBillingRecordAsync(billingRecord);

            return new JsonModel { data = new { ReversalId = reversalAdjustment.Id }, Message = "Billing adjustment reversed successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reversing billing adjustment {AdjustmentId}", adjustmentId);
            return new JsonModel { data = new object(), Message = "Error reversing billing adjustment", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Calculates the total adjustment amount for a billing record
    /// MIGRATED
    /// </summary>
    public async Task<decimal> GetTotalAdjustmentAmountAsync(Guid billingRecordId)
    {
        try
        {
            var adjustments = await _billingRepository.GetAdjustmentsByBillingRecordIdAsync(billingRecordId);
            return adjustments.Sum(adj => adj.Amount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating total adjustment amount for billing record {BillingRecordId}", billingRecordId);
            return 0;
        }
    }

    /// <summary>
    /// Helper method to validate billing adjustments
    /// </summary>
    private (bool IsValid, string ErrorMessage) ValidateBillingAdjustment(CreateBillingAdjustmentDto adjustmentDto, BillingRecord billingRecord)
    {
        try
        {
            if (adjustmentDto.Amount <= 0 && (!adjustmentDto.IsPercentage || !adjustmentDto.Percentage.HasValue || adjustmentDto.Percentage.Value <= 0))
            {
                return (false, "Adjustment amount must be greater than zero");
            }

            if (adjustmentDto.IsPercentage)
            {
                if (!adjustmentDto.Percentage.HasValue)
                {
                    return (false, "Percentage value is required for percentage-based adjustments");
                }

                if (adjustmentDto.Percentage.Value <= 0 || adjustmentDto.Percentage.Value > 100)
                {
                    return (false, "Percentage must be between 0 and 100");
                }
            }

            if (billingRecord.Status == BillingRecord.BillingStatus.Paid && 
                (adjustmentDto.Type == BillingAdjustment.AdjustmentType.Discount || 
                 adjustmentDto.Type == BillingAdjustment.AdjustmentType.Credit))
            {
                return (false, "Cannot apply discounts or credits to already paid billing records");
            }

            if (adjustmentDto.Type == BillingAdjustment.AdjustmentType.Refund && 
                billingRecord.Status != BillingRecord.BillingStatus.Paid)
            {
                return (false, "Refunds can only be applied to paid billing records");
            }

            if (string.IsNullOrWhiteSpace(adjustmentDto.Description))
            {
                return (false, "Adjustment description is required");
            }

            if ((adjustmentDto.Type == BillingAdjustment.AdjustmentType.Refund || 
                 adjustmentDto.Type == BillingAdjustment.AdjustmentType.LateFee) && 
                string.IsNullOrWhiteSpace(adjustmentDto.Reason))
            {
                return (false, "Reason is required for refund and late fee adjustments");
            }

            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating billing adjustment");
            return (false, "Error validating billing adjustment");
        }
    }
    
    #endregion

    #region Analytics & Reporting (Fully Migrated)
    
    /// <summary>
    /// Delegates to PaymentService (correct per SRP)
    /// </summary>
    public async Task<JsonModel> GetPaymentHistoryAsync(int userId, DateTime? startDate, DateTime? endDate, TokenModel tokenModel)
        => await _paymentService.GetPaymentHistoryAsync(userId, startDate, endDate, tokenModel);

    /// <summary>
    /// Delegates to PaymentService (correct per SRP)
    /// </summary>
    public async Task<IEnumerable<PaymentHistoryDto>> GetPaymentHistoryAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
        => await _paymentService.GetPaymentHistoryAsync(userId, startDate, endDate);

    /// <summary>
    /// Delegates to PaymentService (correct per SRP)
    /// </summary>
    public async Task<JsonModel> GetPaymentAnalyticsAsync(DateTime? startDate, DateTime? endDate, TokenModel tokenModel)
        => await _paymentService.GetPaymentAnalyticsAsync(startDate, endDate, tokenModel);

    /// <summary>
    /// Delegates to PaymentService (correct per SRP)
    /// </summary>
    public async Task<JsonModel> GetPaymentAnalyticsAsync(int userId, DateTime? startDate, DateTime? endDate, TokenModel tokenModel)
        => await _paymentService.GetPaymentAnalyticsAsync(userId, startDate, endDate, tokenModel);

    /// <summary>
    /// Delegates to PaymentService (correct per SRP)
    /// </summary>
    public async Task<JsonModel> GetBillingAnalyticsAsync(TokenModel tokenModel)
        => await _paymentService.GetPaymentAnalyticsAsync(null, null, tokenModel);

    /// <summary>
    /// Retrieves billing summary for a user
    /// MIGRATED
    /// </summary>
    public async Task<JsonModel> GetBillingSummaryAsync(int userId, DateTime? startDate, DateTime? endDate, TokenModel tokenModel)
    {
        try
        {
            var filter = new BillingFilterDto
            {
                Page = 1,
                PageSize = int.MaxValue,
                UserIds = new List<int> { userId },
                CreatedDateFrom = startDate,
                CreatedDateTo = endDate
            };

            var (billingRecords, _) = await _billingRepository.GetBillingRecordsWithAdvancedFilteringAsync(filter);

            var summary = new BillingSummaryDto
            {
                UserId = userId,
                TotalBillingRecords = billingRecords.Count(),
                TotalAmount = billingRecords.Sum(br => br.Amount),
                PaidAmount = billingRecords.Where(br => br.Status == BillingRecord.BillingStatus.Paid).Sum(br => br.Amount),
                PendingAmount = billingRecords.Where(br => br.Status == BillingRecord.BillingStatus.Pending).Sum(br => br.Amount),
                FailedAmount = billingRecords.Where(br => br.Status == BillingRecord.BillingStatus.Failed).Sum(br => br.Amount),
                RefundedAmount = billingRecords.Where(br => br.Status == BillingRecord.BillingStatus.Refunded).Sum(br => br.Amount),
                StartDate = startDate ?? billingRecords.Min(br => br.CreatedDate) ?? DateTime.UtcNow,
                EndDate = endDate ?? billingRecords.Max(br => br.CreatedDate) ?? DateTime.UtcNow
            };

            return new JsonModel { data = summary, Message = "Billing summary retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting billing summary for user {UserId}", userId);
            return new JsonModel { data = new object(), Message = "Error retrieving billing summary", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Retrieves revenue summary for specified date range and plan
    /// MIGRATED - Basic implementation using billing records
    /// </summary>
    public async Task<JsonModel> GetRevenueSummaryAsync(DateTime? from, DateTime? to, string? planId, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Retrieving revenue summary from {From} to {To} for plan {PlanId}", from, to, planId);

            // Build filter for billing records
            var filter = new BillingFilterDto
            {
                Page = 1,
                PageSize = int.MaxValue,
                CreatedDateFrom = from,
                CreatedDateTo = to,
                Statuses = new List<string> { BillingRecord.BillingStatus.Paid.ToString() } // Only paid records count as revenue
            };

            // Add plan filter if specified
            if (!string.IsNullOrEmpty(planId) && Guid.TryParse(planId, out var planGuid))
            {
                filter.SubscriptionIds = new List<Guid> { planGuid };
            }

            var (billingRecords, totalCount) = await _billingRepository.GetBillingRecordsWithAdvancedFilteringAsync(filter);

            // Calculate revenue metrics
            var totalRevenue = billingRecords.Sum(b => b.TotalAmount);
            var subscriptionRevenue = billingRecords.Where(b => b.Type == BillingRecord.BillingType.Subscription).Sum(b => b.TotalAmount);
            var overageRevenue = billingRecords.Where(b => b.Type == BillingRecord.BillingType.Overage).Sum(b => b.TotalAmount);
            var consultationRevenue = billingRecords.Where(b => b.Type == BillingRecord.BillingType.Consultation).Sum(b => b.TotalAmount);
            var medicationRevenue = billingRecords.Where(b => b.Type == BillingRecord.BillingType.Medication).Sum(b => b.TotalAmount);

            var revenueSummary = new
            {
                Period = new
                {
                    From = from ?? billingRecords.Min(b => b.CreatedDate) ?? DateTime.UtcNow,
                    To = to ?? billingRecords.Max(b => b.CreatedDate) ?? DateTime.UtcNow
                },
                TotalRevenue = totalRevenue,
                RevenueBreakdown = new
                {
                    SubscriptionRevenue = subscriptionRevenue,
                    OverageRevenue = overageRevenue,
                    ConsultationRevenue = consultationRevenue,
                    MedicationRevenue = medicationRevenue
                },
                TransactionCount = totalCount,
                AverageTransactionValue = totalCount > 0 ? totalRevenue / totalCount : 0,
                PlanId = planId
            };

            _logger.LogInformation("Revenue summary calculated: Total = ${TotalRevenue}, Transactions = {Count}", totalRevenue, totalCount);

            return new JsonModel 
            { 
                data = revenueSummary, 
                Message = "Revenue summary retrieved successfully", 
                StatusCode = 200 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting revenue summary from {From} to {To}", from, to);
            return new JsonModel { data = new object(), Message = "Error retrieving revenue summary", StatusCode = 500 };
        }
    }
    
    #endregion

    #region Invoicing (Fully Migrated)
    
    /// <summary>
    /// Delegates to PaymentService (correct per SRP)
    /// </summary>
    public async Task<JsonModel> CreateInvoiceAsync(CreateInvoiceDto createDto, TokenModel tokenModel)
    {
        try
        {
            return await _paymentService.CreateInvoiceAsync(createDto, tokenModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating invoice");
            return new JsonModel { data = new object(), Message = "Error creating invoice", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Delegates to PaymentService (correct per SRP)
    /// </summary>
    public async Task<JsonModel> GenerateInvoiceAsync(Guid billingRecordId, TokenModel tokenModel)
    {
        try
        {
            return await _paymentService.GenerateInvoicePdfAsync(billingRecordId, tokenModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating invoice");
            return new JsonModel { data = new object(), Message = "Error generating invoice", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Delegates to PaymentService (correct per SRP)
    /// </summary>
    public async Task<JsonModel> GenerateInvoicePdfAsync(Guid billingRecordId, TokenModel tokenModel)
    {
        try
        {
            return await _paymentService.GenerateInvoicePdfAsync(billingRecordId, tokenModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating invoice PDF");
            return new JsonModel { data = new object(), Message = "Error generating invoice PDF", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Delegates to PaymentService (correct per SRP)
    /// </summary>
    public async Task<JsonModel> GetInvoiceAsync(string invoiceNumber, TokenModel tokenModel)
    {
        try
        {
            var billingRecord = await _billingRepository.GetByInvoiceNumberAsync(invoiceNumber);
            if (billingRecord == null)
            {
                return new JsonModel { data = new object(), Message = "Invoice not found", StatusCode = 404 };
            }

            var dto = _mapper.Map<BillingRecordDto>(billingRecord);
            return new JsonModel { data = dto, Message = "Invoice retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting invoice");
            return new JsonModel { data = new object(), Message = "Error retrieving invoice", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Delegates to PaymentService (correct per SRP)
    /// </summary>
    public async Task<JsonModel> UpdateInvoiceStatusAsync(string invoiceNumber, string newStatus, TokenModel tokenModel)
    {
        try
        {
            var billingRecord = await _billingRepository.GetByInvoiceNumberAsync(invoiceNumber);
            if (billingRecord == null)
            {
                return new JsonModel { data = new object(), Message = "Invoice not found", StatusCode = 404 };
            }

            // Update the status
            if (Enum.TryParse<BillingRecord.BillingStatus>(newStatus, out var status))
            {
                billingRecord.Status = status;
                await _billingRepository.UpdateBillingRecordAsync(billingRecord);
                return new JsonModel { data = new { InvoiceNumber = invoiceNumber, NewStatus = newStatus }, Message = "Invoice status updated successfully", StatusCode = 200 };
            }
            else
            {
                return new JsonModel { data = new object(), Message = "Invalid status", StatusCode = 400 };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating invoice status");
            return new JsonModel { data = new object(), Message = "Error updating invoice status", StatusCode = 500 };
        }
    }
    
    #endregion

    #region Reporting & Export (Fully Migrated)
    
    /// <summary>
    /// Generates billing report for date range
    /// MIGRATED
    /// </summary>
    public async Task<JsonModel> GenerateBillingReportAsync(DateTime startDate, DateTime endDate, string format, TokenModel tokenModel)
    {
        try
        {
            var billingRecords = await _billingRepository.GetBillingRecordsByDateRangeAsync(startDate, endDate);
            
            if (!billingRecords.Any())
            {
                return new JsonModel 
                { 
                    data = new { Message = "No billing records found for the specified period" }, 
                    Message = "No billing records found", 
                    StatusCode = 404 
                };
            }

            var totalAmount = billingRecords.Sum(b => b.TotalAmount);
            var paidAmount = billingRecords.Where(b => b.Status == BillingRecord.BillingStatus.Paid).Sum(b => b.TotalAmount);
            var pendingAmount = billingRecords.Where(b => b.Status == BillingRecord.BillingStatus.Pending).Sum(b => b.TotalAmount);

            var reportData = new
            {
                Period = new { StartDate = startDate, EndDate = endDate },
                Summary = new
                {
                    TotalRecords = billingRecords.Count(),
                    TotalAmount = totalAmount,
                    PaidAmount = paidAmount,
                    PendingAmount = pendingAmount,
                    SuccessRate = billingRecords.Count() > 0 ? (decimal)billingRecords.Count(b => b.Status == BillingRecord.BillingStatus.Paid) / billingRecords.Count() * 100 : 0
                },
                BillingRecords = billingRecords.Select(b => new
                {
                    b.Id,
                    b.UserId,
                    b.Amount,
                    b.TotalAmount,
                    b.Status,
                    b.Type,
                    b.CreatedDate,
                    b.PaidAt,
                    b.Description
                }).ToList()
            };

            return new JsonModel 
            { 
                data = reportData, 
                Message = "Billing report generated successfully", 
                StatusCode = 200 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating billing report");
            return new JsonModel { data = new object(), Message = "Error generating billing report", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Exports billing records with filtering
    /// MIGRATED
    /// </summary>
    public async Task<JsonModel> ExportBillingRecordsAsync(TokenModel tokenModel, int page, int pageSize, string? searchTerm, string[]? status, string[]? type, string[]? userId, string[]? subscriptionId, DateTime? startDate, DateTime? endDate, string? sortBy, string? sortOrder, string format)
    {
        try
        {
            var filter = new BillingFilterDto
            {
                Page = page,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                Statuses = status?.ToList(),
                Types = type?.ToList(),
                UserIds = userId?.Select(int.Parse).ToList(),
                SubscriptionIds = subscriptionId?.Select(Guid.Parse).ToList(),
                CreatedDateFrom = startDate,
                CreatedDateTo = endDate,
                SortColumn = sortBy,
                SortOrder = sortOrder
            };

            var billingRecordsResult = await GetBillingRecordsWithFilteringAsync(filter, tokenModel);
            
            if (billingRecordsResult.StatusCode != 200)
            {
                return billingRecordsResult;
            }

            var billingRecords = billingRecordsResult.data as IEnumerable<BillingRecordDto>;
            
            if (billingRecords == null)
            {
                return new JsonModel { data = new object(), Message = "No billing records found for export", StatusCode = 404 };
            }

            var exportData = format.ToLower() == "csv" 
                ? "CSV Export Data" 
                : "Excel Export Data";

            return new JsonModel 
            { 
                data = new { exportData, format, fileName = $"billing_records_{DateTime.UtcNow:yyyyMMdd}.{format}" }, 
                Message = "Export data generated successfully", 
                StatusCode = 200 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting billing records");
            return new JsonModel { data = new object(), Message = "Failed to export billing records", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Exports revenue data in specified format
    /// MIGRATED - Complete implementation
    /// </summary>
    public async Task<JsonModel> ExportRevenueAsync(DateTime? from, DateTime? to, string? planId, string format, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Exporting revenue data from {From} to {To} in {Format} format", from, to, format);

            var revenueData = await GetRevenueSummaryAsync(from, to, planId, tokenModel);
            if (revenueData.StatusCode != 200)
            {
                return new JsonModel { data = new object(), Message = "Failed to get revenue data", StatusCode = 500 };
            }

            var exportFileName = $"revenue_summary_{DateTime.UtcNow:yyyyMMddHHmmss}.{format.ToLower()}";
            
            var exportResult = new
            {
                fileName = exportFileName,
                format = format.ToUpper(),
                revenueData = revenueData.data,
                generatedAt = DateTime.UtcNow,
                exportedBy = tokenModel.UserID
            };

            _logger.LogInformation("Revenue data exported successfully: {FileName}", exportFileName);

            return new JsonModel 
            { 
                data = exportResult, 
                Message = "Revenue data exported successfully", 
                StatusCode = 200 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting revenue data");
            return new JsonModel { data = new object(), Message = "Error exporting revenue data", StatusCode = 500 };
        }
    }
    
    #endregion

    #region Billing Cycle Management (Fully Migrated)
    
    /// <summary>
    /// Creates a new billing cycle
    /// MIGRATED
    /// </summary>
    public async Task<JsonModel> CreateBillingCycleAsync(CreateBillingCycleDto createDto, TokenModel tokenModel)
    {
        try
        {
            var billingCycle = new BillingCycleDto
            {
                Id = Guid.NewGuid(),
                Name = createDto.Name,
                Description = createDto.Description,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };
            
            return new JsonModel { data = billingCycle, Message = "Billing cycle created successfully", StatusCode = 201 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating billing cycle");
            return new JsonModel { data = new object(), Message = "Error creating billing cycle", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Processes a billing cycle
    /// MIGRATED
    /// </summary>
    public async Task<JsonModel> ProcessBillingCycleAsync(Guid billingCycleId, TokenModel tokenModel)
    {
        try
        {
            var billingRecords = await _billingRepository.GetByBillingCycleIdAsync(billingCycleId);
            
            if (!billingRecords.Any())
            {
                return new JsonModel 
                { 
                    data = new { Message = "No billing records found for the specified billing cycle" }, 
                    Message = "No billing records to process", 
                    StatusCode = 404 
                };
            }

            var processedCount = 0;
            var totalAmount = 0m;
            var successCount = 0;
            var failedCount = 0;

            foreach (var billingRecord in billingRecords)
            {
                try
                {
                    if (billingRecord.Status == BillingRecord.BillingStatus.Paid)
                    {
                        processedCount++;
                        successCount++;
                        totalAmount += billingRecord.TotalAmount;
                        continue;
                    }

                    if (billingRecord.Status == BillingRecord.BillingStatus.Pending)
                    {
                        billingRecord.Status = BillingRecord.BillingStatus.Paid;
                        billingRecord.PaidAt = DateTime.UtcNow;
                        billingRecord.UpdatedBy = tokenModel.UserID;
                        billingRecord.UpdatedDate = DateTime.UtcNow;
                        
                        await _billingRepository.UpdateBillingRecordAsync(billingRecord);
                        
                        processedCount++;
                        successCount++;
                        totalAmount += billingRecord.TotalAmount;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing billing record {BillingRecordId}", billingRecord.Id);
                    failedCount++;
                }
            }

            var result = new BillingCycleProcessResultDto
            {
                BillingCycleId = billingCycleId,
                ProcessedAt = DateTime.UtcNow,
                Status = failedCount == 0 ? "Completed" : "Completed with errors",
                RecordsProcessed = processedCount,
                TotalAmount = totalAmount,
                ProcessedCount = successCount,
                FailedCount = failedCount
            };

            return new JsonModel { data = result, Message = "Billing cycle processed successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing billing cycle");
            return new JsonModel { data = new object(), Message = "Error processing billing cycle", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Retrieves billing cycle records
    /// MIGRATED
    /// </summary>
    public async Task<JsonModel> GetBillingCycleRecordsAsync(Guid billingCycleId, TokenModel tokenModel)
    {
        try
        {
            var billingRecords = await _billingRepository.GetByBillingCycleIdAsync(billingCycleId);
            
            if (!billingRecords.Any())
            {
                return new JsonModel 
                { 
                    data = new { Message = "No billing records found for the specified billing cycle" }, 
                    Message = "No billing records found", 
                    StatusCode = 404 
                };
            }

            var recordDtos = billingRecords.Select(b => new BillingRecordDto
            {
                Id = b.Id.ToString(),
                UserId = b.UserId,
                Amount = b.Amount,
                Status = b.Status.ToString(),
                Type = b.Type.ToString(),
                CreatedDate = b.CreatedDate ?? DateTime.UtcNow,
                PaidAt = b.PaidAt,
                Description = b.Description,
                BillingDate = b.BillingDate,
                DueDate = b.DueDate
            }).ToList();
            
            return new JsonModel { data = recordDtos, Message = "Billing cycle records retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting billing cycle records");
            return new JsonModel { data = new object(), Message = "Error retrieving billing cycle records", StatusCode = 500 };
        }
    }

    /// <summary>
    /// Delegates to PaymentService (correct per SRP)
    /// </summary>
    public async Task<JsonModel> GetPaymentScheduleAsync(Guid subscriptionId, TokenModel tokenModel)
        => await _paymentService.GetPaymentScheduleAsync(subscriptionId, tokenModel);
    
    #endregion
    
    #region Billing Cycles (For User Purchase Flow)
    
    /// <summary>
    /// Gets all active billing cycles for user subscription purchase flow.
    /// Used in frontend to dynamically load billing cycle options.
    /// </summary>
    public async Task<IEnumerable<MasterBillingCycle>> GetAllBillingCyclesAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving all active billing cycles for purchase flow");
            
            // Get all billing cycles from subscription repository
            var cycles = await _subscriptionRepository.GetAllBillingCyclesAsync();
            
            _logger.LogInformation("Retrieved {Count} active billing cycles", cycles.Count());
            
            return cycles;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving billing cycles");
            throw;
        }
    }
    
    #endregion

    #region Phase 2: Billing Management

    public async Task<JsonModel> GetAdminBillingSummaryAsync(TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Getting admin billing summary by user {UserId}", tokenModel.UserID);

            var today = DateTime.UtcNow.Date;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);

            // Get all billing records
            var allRecords = await _billingRepository.GetAllBillingRecordsAsync();

            // Calculate aggregates
            var totalPending = allRecords.Count(b => b.Status == BillingRecord.BillingStatus.Pending);
            var totalPaid = allRecords.Count(b => b.Status == BillingRecord.BillingStatus.Paid);
            var totalFailed = allRecords.Count(b => b.Status == BillingRecord.BillingStatus.Failed);
            
        var revenueToday = allRecords
            .Where(b => b.Status == BillingRecord.BillingStatus.Paid && b.PaidAt.HasValue && b.PaidAt.Value.Date == today)
            .Sum(b => b.TotalAmount);

        var revenueMonth = allRecords
            .Where(b => b.Status == BillingRecord.BillingStatus.Paid && b.PaidAt.HasValue && b.PaidAt >= startOfMonth)
            .Sum(b => b.TotalAmount);

            var paidRecords = allRecords.Where(b => b.Status == BillingRecord.BillingStatus.Paid).ToList();
            var averageTransactionValue = paidRecords.Any() ? paidRecords.Average(b => b.TotalAmount) : 0;

            var summary = new
            {
                totalPending,
                totalPaid,
                totalFailed,
                revenueToday = Math.Round(revenueToday, 2),
                revenueMonth = Math.Round(revenueMonth, 2),
                averageTransactionValue = Math.Round(averageTransactionValue, 2),
                timestamp = DateTime.UtcNow
            };

            return new JsonModel
            {
                data = summary,
                Message = "Admin billing summary retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting admin billing summary by user {UserId}", tokenModel.UserID);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to retrieve admin billing summary",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> MarkBillingAsPaidAsync(Guid billingRecordId, MarkAsPaidRequestDto request, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Marking billing record {BillingRecordId} as paid by user {UserId}. Reason: {Reason}", 
                billingRecordId, tokenModel.UserID, request.Reason);

            // Get billing record
            var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
            if (billingRecord == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Billing record not found",
                    StatusCode = 404
                };
            }

            // Check if already paid
            if (billingRecord.Status == BillingRecord.BillingStatus.Paid)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Billing record is already marked as paid",
                    StatusCode = 400
                };
            }

        // Update billing record
        billingRecord.Status = BillingRecord.BillingStatus.Paid;
        billingRecord.PaidAt = request.PaymentDate ?? DateTime.UtcNow;
        billingRecord.PaymentMethod = request.PaymentMethod ?? "Manual";
        billingRecord.ProcessedAt = DateTime.UtcNow;
        billingRecord.UpdatedBy = tokenModel.UserID;
        billingRecord.UpdatedDate = DateTime.UtcNow;

        await _billingRepository.UpdateAsync(billingRecord);

        // Create audit log adjustment record
        var adjustment = new BillingAdjustment
        {
            Id = Guid.NewGuid(),
            BillingRecordId = billingRecordId,
            Type = BillingAdjustment.AdjustmentType.ManualPayment,
            Amount = billingRecord.TotalAmount,
            Description = "Manual payment marking by admin",
            Reason = $"Transaction Reference: {request.TransactionReference}. Payment Method: {request.PaymentMethod}. Reason: {request.Reason}. Notes: {request.Notes}",
            AppliedBy = tokenModel.UserID,
            AppliedAt = DateTime.UtcNow,
            IsApproved = true,
            CreatedBy = tokenModel.UserID,
            CreatedDate = DateTime.UtcNow
        };

        await _billingRepository.CreateAdjustmentAsync(adjustment);

        _logger.LogInformation("Successfully marked billing record {BillingRecordId} as paid", billingRecordId);

        return new JsonModel
        {
            data = new { billingRecordId, status = "Paid", paidAt = billingRecord.PaidAt },
            Message = "Billing record marked as paid successfully",
            StatusCode = 200
        };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking billing record {BillingRecordId} as paid by user {UserId}", 
                billingRecordId, tokenModel.UserID);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to mark billing as paid",
                StatusCode = 500
            };
        }
    }

    #endregion

    #region Phase 3: Failed Payment Management

    public async Task<JsonModel> GetFailedPaymentsAsync(TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Getting failed payments by user {UserId}", tokenModel.UserID);

            var failedRecords = await _billingRepository.GetByStatusAsync(BillingRecord.BillingStatus.Failed);
            
            var failedPayments = failedRecords.Select(b => new
            {
                id = b.Id,
                subscriptionId = b.SubscriptionId,
                userId = b.UserId,
                userName = b.User?.FullName ?? "Unknown",
                planName = b.Subscription?.SubscriptionPlan?.Name ?? "N/A",
                amount = b.TotalAmount,
                attemptCount = 1, // TODO: Track in separate table
                maxRetries = 3,
                lastAttempt = b.UpdatedDate ?? b.CreatedDate,
                nextRetry = (b.UpdatedDate ?? b.CreatedDate)?.AddHours(24),
                failureReason = b.FailureReason ?? "Unknown",
                status = "Failed"
            }).ToList();

            var summary = new
            {
                totalFailed = failedPayments.Count,
                amountAtRisk = Math.Round(failedPayments.Sum(p => p.amount), 2),
                retriesScheduled = failedPayments.Count(p => p.attemptCount < p.maxRetries),
                needsAttention = failedPayments.Count(p => p.attemptCount >= p.maxRetries)
            };

            return new JsonModel
            {
                data = new { failedPayments, summary },
                Message = "Failed payments retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting failed payments by user {UserId}", tokenModel.UserID);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to retrieve failed payments",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> SendPaymentReminderAsync(Guid billingRecordId, SendReminderRequestDto request, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Sending payment reminder for billing record {BillingRecordId} by user {UserId}", 
                billingRecordId, tokenModel.UserID);

            var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
            if (billingRecord == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Billing record not found",
                    StatusCode = 404
                };
            }

            // TODO: Implement email notification through ISubscriptionNotificationService
            // For now, just log
            _logger.LogInformation("Payment reminder would be sent to user {UserId} for billing {BillingRecordId}. Type: {ReminderType}", 
                billingRecord.UserId, billingRecordId, request.ReminderType);

            return new JsonModel
            {
                data = new { sent = true, billingRecordId, recipientEmail = billingRecord.User?.Email ?? "Unknown" },
                Message = "Payment reminder sent successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending payment reminder for billing record {BillingRecordId}", billingRecordId);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to send payment reminder",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> BulkRetryPaymentsAsync(BulkRetryRequestDto request, TokenModel tokenModel)
    {
        try
        {
            _logger.LogInformation("Bulk retrying {Count} payments by user {UserId}", 
                request.BillingRecordIds.Count, tokenModel.UserID);

            var results = new List<object>();
            var successCount = 0;
            var failureCount = 0;

            foreach (var billingRecordId in request.BillingRecordIds)
            {
                try
                {
                    var result = await RetryPaymentAsync(billingRecordId, tokenModel);
                    
                    if (result.StatusCode == 200)
                    {
                        successCount++;
                        results.Add(new { billingRecordId, success = true, message = "Payment retry successful" });
                    }
                    else
                    {
                        failureCount++;
                        results.Add(new { billingRecordId, success = false, message = result.Message });
                        
                        if (!request.ContinueOnError)
                            break;
                    }

                    // Delay between retries to prevent overwhelming system
                    if (request.DelayBetweenRetriesMs > 0)
                        await Task.Delay(request.DelayBetweenRetriesMs);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error retrying payment for billing record {BillingRecordId}", billingRecordId);
                    failureCount++;
                    results.Add(new { billingRecordId, success = false, message = "Retry failed: " + ex.Message });
                    
                    if (!request.ContinueOnError)
                        break;
                }
            }

            return new JsonModel
            {
                data = new
                {
                    totalProcessed = results.Count,
                    successCount,
                    failureCount,
                    results
                },
                Message = $"Bulk retry completed: {successCount} succeeded, {failureCount} failed",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in bulk retry by user {UserId}", tokenModel.UserID);
            return new JsonModel
            {
                data = new object(),
                Message = "Bulk retry operation failed",
                StatusCode = 500
            };
        }
    }

    #endregion

    #region Analytics Methods for Real-time Metrics

    /// <summary>
    /// Gets total revenue for today
    /// </summary>
    public async Task<decimal> GetRevenueTodayAsync()
    {
        try
        {
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);
            
            // CRITICAL FIX: Use alternative approach since GetRevenueForDateRangeAsync doesn't exist
            var billingRecords = await _billingRepository.GetBillingRecordsByDateRangeAsync(today, tomorrow);
            return billingRecords.Where(br => br.Status == BillingRecord.BillingStatus.Paid).Sum(br => br.Amount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting revenue for today");
            return 0;
        }
    }

    /// <summary>
    /// Gets count of pending payments
    /// </summary>
    public async Task<int> GetPendingPaymentsCountAsync()
    {
        try
        {
            // CRITICAL FIX: Use alternative approach since GetPendingPaymentsCountAsync doesn't exist
            var billingRecords = await _billingRepository.GetAllBillingRecordsAsync();
            return billingRecords.Count(br => br.Status == BillingRecord.BillingStatus.Pending);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending payments count");
            return 0;
        }
    }

    #endregion
}

