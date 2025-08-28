using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;

namespace SmartTelehealth.Application.Services;

public class VideoCallSubscriptionService : IVideoCallSubscriptionService
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IConsultationRepository _consultationRepository;
    private readonly IOpenTokService _openTokService;
    private readonly IBillingService _billingService;
    private readonly ILogger<VideoCallSubscriptionService> _logger;
    private readonly PrivilegeService _privilegeService;

    public VideoCallSubscriptionService(
        ISubscriptionRepository subscriptionRepository,
        IConsultationRepository consultationRepository,
        IOpenTokService openTokService,
        IBillingService billingService,
        ILogger<VideoCallSubscriptionService> logger,
        PrivilegeService privilegeService)
    {
        _subscriptionRepository = subscriptionRepository;
        _consultationRepository = consultationRepository;
        _openTokService = openTokService;
        _billingService = billingService;
        _logger = logger;
        _privilegeService = privilegeService;
    }

    public async Task<JsonModel> CheckVideoCallAccessAsync(int userId, Guid? consultationId = null, TokenModel tokenModel = null)
    {
        try
        {
            // Get active subscription for user
            var subscription = await _subscriptionRepository.GetActiveSubscriptionByUserIdAsync(userId);
            if (subscription == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "No active subscription found",
                    StatusCode = 403
                };
            }
            // Check if subscription plan includes video calls (privilege)
            var videoCallRemaining = await _privilegeService.GetRemainingPrivilegeAsync(subscription.Id, "VideoCall", tokenModel);
            if (videoCallRemaining <= 0)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Video calls not included in current plan or limit reached",
                    StatusCode = 403
                };
            }
            // Check consultation limits (privilege)
            var consultationRemaining = await _privilegeService.GetRemainingPrivilegeAsync(subscription.Id, "Teleconsultation", tokenModel);
            if (consultationRemaining == 0)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Consultation limit reached for current billing period",
                    StatusCode = 403
                };
            }
            // Check if this is a one-time consultation
            if (consultationId.HasValue)
            {
                var consultation = await _consultationRepository.GetByIdAsync(consultationId.Value);
                if (consultation != null && consultation.IsOneTime)
                {
                    return await CheckOneTimeVideoCallAccessAsync(consultation);
                }
            }
            var accessDto = new VideoCallAccessDto
            {
                HasAccess = true,
                SubscriptionId = subscription.Id,
                PlanName = subscription.SubscriptionPlan.Name,
                // Remove ConsultationsUsed, ConsultationLimit, RemainingConsultations
                // Use privilege system for privilege usage reporting if needed
                MaxDurationMinutes = 60, // Default duration
                CanRecord = await _privilegeService.GetRemainingPrivilegeAsync(subscription.Id, "PrioritySupport", tokenModel) > 0,
                CanBroadcast = false // Only for premium plans, add as privilege if needed
            };
            return new JsonModel
            {
                data = accessDto,
                Message = "Video call access granted",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking video call access for user: {UserId}", userId);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to check video call access",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> CreateVideoCallSessionAsync(int userId, Guid consultationId, string sessionName, TokenModel tokenModel)
    {
        try
        {
            var accessResult = await CheckVideoCallAccessAsync(userId, consultationId, tokenModel);
            if (accessResult.StatusCode != 200)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = accessResult.Message,
                    StatusCode = accessResult.StatusCode
                };
            }
            var consultation = await _consultationRepository.GetByIdAsync(consultationId);
            if (consultation == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Consultation not found",
                    StatusCode = 404
                };
            }
            var sessionResult = await _openTokService.CreateSessionAsync(sessionName, false, tokenModel);
            if (sessionResult.StatusCode != 200)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Failed to create video session",
                    StatusCode = 500
                };
            }
            
            // Extract all values before any logger calls to avoid dynamic dispatch issues
            string sessionId = "unknown";
            try
            {
                var dynamicData = sessionResult.data as dynamic;
                if (dynamicData != null)
                {
                    sessionId = dynamicData.SessionId?.ToString() ?? "unknown";
                    consultation.MeetingUrl = $"/video-call/{sessionId}";
                    consultation.MeetingId = sessionId;
                    await _consultationRepository.UpdateAsync(consultation);
                    
                    // Increment consultation usage (privilege)
                    var subscription = await _subscriptionRepository.GetActiveSubscriptionByUserIdAsync(userId);
                    if (subscription != null && !consultation.IsOneTime)
                    {
                        await _privilegeService.UsePrivilegeAsync(subscription.Id, "Teleconsultation", 1, tokenModel);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing session data for consultation: {ConsultationId}", consultationId);
                // Continue execution even if dynamic access fails
            }
            
            // Now log with extracted values (no dynamic dispatch)
            _logger.LogInformation("Created video call session for consultation: {0}, session: {1}", consultationId, sessionId);
            return sessionResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating video call session for user: {UserId}, consultation: {ConsultationId}", userId, consultationId);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to create video call session",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GenerateVideoCallTokenAsync(int userId, string sessionId, OpenTokRole role, TokenModel tokenModel)
    {
        try
        {
            // Check access
            var accessResult = await CheckVideoCallAccessAsync(userId, null, tokenModel);
            if (accessResult.StatusCode != 200)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = accessResult.Message,
                    StatusCode = accessResult.StatusCode
                };
            }

            // Generate token
            var tokenResult = await _openTokService.GenerateTokenAsync(sessionId, userId.ToString(), "User", role, tokenModel);
            
            if (tokenResult.StatusCode != 200)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Failed to generate video call token",
                    StatusCode = 500
                };
            }

            _logger.LogInformation("Generated video call token for user: {UserId}, session: {SessionId}", userId, sessionId);
            return tokenResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating video call token for user: {UserId}, session: {SessionId}", userId, sessionId);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to generate video call token",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> ProcessVideoCallBillingAsync(int userId, Guid consultationId, int durationMinutes, TokenModel tokenModel)
    {
        try
        {
            var consultation = await _consultationRepository.GetByIdAsync(consultationId);
            if (consultation == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Consultation not found",
                    StatusCode = 404
                };
            }
            var subscription = await _subscriptionRepository.GetActiveSubscriptionByUserIdAsync(userId);
            decimal billingAmount = 0;
            if (consultation.IsOneTime)
            {
                billingAmount = consultation.Fee;
            }
            else if (subscription != null)
            {
                // Use privilege system to determine if included
                var consultationRemaining = await _privilegeService.GetRemainingPrivilegeAsync(subscription.Id, "Teleconsultation", tokenModel);
                if (consultationRemaining > 0)
                {
                    billingAmount = 0; // Included in subscription
                }
                else
                {
                    // Additional consultation fee (could be a privilege value or plan property)
                    billingAmount = 0; // Set as needed
                }
            }
            if (billingAmount > 0)
            {
                var billingDto = new CreateBillingRecordDto
                {
                    UserId = userId,
                    SubscriptionId = subscription?.Id.ToString() ?? string.Empty,
                    ConsultationId = consultationId.ToString(),
                    Amount = billingAmount,
                    Type = BillingRecord.BillingType.Consultation.ToString(),
                    Description = $"Video consultation - {durationMinutes} minutes",
                    DueDate = DateTime.UtcNow.AddDays(30)
                };
                var billingResult = await _billingService.CreateBillingRecordAsync(billingDto, tokenModel);
                if (billingResult.StatusCode != 200)
                {
                    return new JsonModel
                    {
                        data = new object(),
                        Message = "Failed to create billing record",
                        StatusCode = 500
                    };
                }
                var billingResponse = new VideoCallBillingDto
                {
                    BillingRecordId = GetBillingRecordId(billingResult.data),
                    Amount = billingAmount,
                    DurationMinutes = durationMinutes,
                    IsIncludedInSubscription = billingAmount == 0,
                    ConsultationId = consultationId,
                    Description = $"Video consultation - {durationMinutes} minutes"
                };
                return new JsonModel
                {
                    data = billingResponse,
                    Message = "Billing processed successfully",
                    StatusCode = 200
                };
            }
            var noBillingResponse = new VideoCallBillingDto
            {
                Amount = 0,
                DurationMinutes = durationMinutes,
                IsIncludedInSubscription = true,
                ConsultationId = consultationId
            };
            return new JsonModel
            {
                data = noBillingResponse,
                Message = "No billing required",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing video call billing for user: {UserId}, consultation: {ConsultationId}", userId, consultationId);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to process video call billing",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetVideoCallUsageAsync(int userId)
    {
        try
        {
            var subscription = await _subscriptionRepository.GetActiveSubscriptionByUserIdAsync(userId);
            if (subscription == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "No active subscription found",
                    StatusCode = 404
                };
            }

            // Get consultations for current billing period
            var billingStartDate = subscription.NextBillingDate.AddMonths(-1);
            var consultations = await _consultationRepository.GetByUserIdAsync(userId);
            var currentPeriodConsultations = consultations
                .Where(c => c.CreatedDate >= billingStartDate && c.Status == Consultation.ConsultationStatus.Completed)
                .ToList();

            var usageDto = new VideoCallUsageDto
            {
                SubscriptionId = subscription.Id,
                PlanName = subscription.SubscriptionPlan.Name,
                // Remove ConsultationsUsed, ConsultationLimit, RemainingConsultations from VideoCallUsageDto
                // Use privilege system for reporting if needed
                CurrentBillingPeriodStart = billingStartDate,
                CurrentBillingPeriodEnd = subscription.NextBillingDate,
                TotalVideoCallsThisPeriod = currentPeriodConsultations.Count(),
                TotalDurationThisPeriod = currentPeriodConsultations.Sum(c => c.DurationMinutes),
                AverageDurationMinutes = currentPeriodConsultations.Any() ? 
                    currentPeriodConsultations.Average(c => c.DurationMinutes) : 0
            };

            return new JsonModel
            {
                data = usageDto,
                Message = "Usage data retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting video call usage for user: {UserId}", userId);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to get usage data",
                StatusCode = 500
            };
        }
    }

    private async Task<JsonModel> CheckOneTimeVideoCallAccessAsync(Consultation consultation)
    {
        try
        {
            // Check if consultation is within allowed time window
            var timeWindow = TimeSpan.FromHours(2); // 2 hours before/after scheduled time
            var now = DateTime.UtcNow;
            var scheduledTime = consultation.ScheduledAt;
            
            if (now < scheduledTime.Subtract(timeWindow) || now > scheduledTime.Add(timeWindow))
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Video call access outside allowed time window",
                    StatusCode = 403
                };
            }

            // Check if consultation is paid
            if (consultation.Fee > 0)
            {
                // Check if payment is processed
                // This would typically check against billing records
                var isPaid = true; // Placeholder - implement actual payment check
                
                if (!isPaid)
                {
                    return new JsonModel
                    {
                        data = new object(),
                        Message = "Consultation payment required",
                        StatusCode = 402
                    };
                }
            }

            var accessDto = new VideoCallAccessDto
            {
                HasAccess = true,
                ConsultationId = consultation.Id,
                PlanName = "One-time Consultation",
                // Remove ConsultationsUsed, ConsultationLimit, RemainingConsultations
                // Use privilege system for privilege usage reporting if needed
                MaxDurationMinutes = consultation.DurationMinutes,
                CanRecord = false,
                CanBroadcast = false,
                IsOneTime = true
            };

            return new JsonModel
            {
                data = accessDto,
                Message = "One-time video call access granted",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking one-time video call access for consultation: {ConsultationId}", consultation.Id);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to check one-time video call access",
                StatusCode = 500
            };
        }
    }

    private Guid? GetBillingRecordId(object billingData)
    {
        try
        {
            var dynamicData = billingData as dynamic;
            if (dynamicData != null)
            {
                if (Guid.TryParse(dynamicData.Id?.ToString(), out Guid billId))
                {
                    return billId;
                }
            }
            return null;
        }
        catch
        {
            return null;
        }
    }
} 