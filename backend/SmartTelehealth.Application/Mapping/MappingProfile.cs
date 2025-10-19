using AutoMapper;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User mappings
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.PhoneNumber))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
            .ForMember(dest => dest.UserType, opt => opt.MapFrom(src => src.UserType))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.UserRole != null ? src.UserRole.Name : "User"))
            .ForMember(dest => dest.UserRoleId, opt => opt.MapFrom(src => src.UserRoleId))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.IsVerified, opt => opt.MapFrom(src => src.IsEmailVerified && src.IsPhoneVerified))
            .ForMember(dest => dest.IsEmailVerified, opt => opt.MapFrom(src => src.EmailConfirmed))
            .ForMember(dest => dest.IsPhoneVerified, opt => opt.MapFrom(src => src.PhoneNumberConfirmed))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
            .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => src.UpdatedDate))
            .ForMember(dest => dest.LastLoginAt, opt => opt.MapFrom(src => src.LastLoginAt))
            .ForMember(dest => dest.ProfilePicture, opt => opt.MapFrom(src => src.ProfilePicture))
            .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth))
            .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
            .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
            .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.State))
            .ForMember(dest => dest.ZipCode, opt => opt.MapFrom(src => src.ZipCode))
            .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Country))
            .ForMember(dest => dest.EmergencyContact, opt => opt.MapFrom(src => src.EmergencyContact))
            .ForMember(dest => dest.EmergencyPhone, opt => opt.MapFrom(src => src.EmergencyPhone))
            .ForMember(dest => dest.StripeCustomerId, opt => opt.MapFrom(src => src.StripeCustomerId));

        CreateMap<User, PatientDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.PhoneNumber))
            .ForMember(dest => dest.UserType, opt => opt.MapFrom(src => src.UserType))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.IsVerified, opt => opt.MapFrom(src => src.IsEmailVerified && src.IsPhoneVerified))
            .ForMember(dest => dest.IsEmailVerified, opt => opt.MapFrom(src => src.EmailConfirmed))
            .ForMember(dest => dest.IsPhoneVerified, opt => opt.MapFrom(src => src.PhoneNumberConfirmed))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
            .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => src.UpdatedDate))
            .ForMember(dest => dest.LastLoginAt, opt => opt.MapFrom(src => src.LastLoginAt))
            .ForMember(dest => dest.ProfilePicture, opt => opt.MapFrom(src => src.ProfilePicture))
            .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth))
            .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
            .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
            .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.State))
            .ForMember(dest => dest.ZipCode, opt => opt.MapFrom(src => src.ZipCode))
            .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Country))
            .ForMember(dest => dest.EmergencyContact, opt => opt.MapFrom(src => src.EmergencyContact))
            .ForMember(dest => dest.EmergencyPhone, opt => opt.MapFrom(src => src.EmergencyPhone))
            .ForMember(dest => dest.StripeCustomerId, opt => opt.MapFrom(src => src.StripeCustomerId));

        CreateMap<CreateUserDto, User>()
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
            .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth))
            .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
            .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
            .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.State))
            .ForMember(dest => dest.ZipCode, opt => opt.MapFrom(src => src.ZipCode))
            .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Country))
            .ForMember(dest => dest.EmergencyContact, opt => opt.MapFrom(src => src.EmergencyContactName))
            .ForMember(dest => dest.EmergencyPhone, opt => opt.MapFrom(src => src.EmergencyContactPhone))
            .ForMember(dest => dest.UserType, opt => opt.MapFrom(src => src.UserType))
            .ForMember(dest => dest.UserRoleId, opt => opt.MapFrom(src => src.UserRoleId))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.IsEmailVerified, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.IsPhoneVerified, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.EmailConfirmed, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.PhoneNumberConfirmed, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => DateTime.UtcNow));

        CreateMap<UpdateUserDto, User>()
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
            .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
            .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
            .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.State))
            .ForMember(dest => dest.ZipCode, opt => opt.MapFrom(src => src.ZipCode))
            .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Country))
            .ForMember(dest => dest.ProfilePicture, opt => opt.MapFrom(src => src.ProfilePictureUrl))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.StripeCustomerId, opt => opt.MapFrom(src => src.StripeCustomerId))
            .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => DateTime.UtcNow));

        CreateMap<UpdateUserProfileDto, User>()
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
            .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth))
            .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
            .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
            .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.State))
            .ForMember(dest => dest.ZipCode, opt => opt.MapFrom(src => src.ZipCode))
            .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Country))
            .ForMember(dest => dest.EmergencyContact, opt => opt.MapFrom(src => src.EmergencyContact))
            .ForMember(dest => dest.EmergencyPhone, opt => opt.MapFrom(src => src.EmergencyPhone))
            .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => DateTime.UtcNow));

        // Appointment mappings
        CreateMap<Appointment, AppointmentDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.PatientId, opt => opt.MapFrom(src => src.PatientId.ToString()))
            .ForMember(dest => dest.ProviderId, opt => opt.MapFrom(src => src.ProviderId.ToString()))
            .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId.ToString()))
            .ForMember(dest => dest.SubscriptionId, opt => opt.MapFrom(src => src.SubscriptionId.ToString()))
            .ForMember(dest => dest.ConsultationId, opt => opt.MapFrom(src => src.ConsultationId.ToString()))
            .ForMember(dest => dest.AppointmentTypeId, opt => opt.MapFrom(src => src.AppointmentTypeId))
            .ForMember(dest => dest.ConsultationModeId, opt => opt.MapFrom(src => src.ConsultationModeId));

        // Subscription mappings
        CreateMap<CreateSubscriptionDto, Subscription>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.SubscriptionPlanId, opt => opt.MapFrom(src => Guid.Parse(src.PlanId)))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Active"))
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => DateTime.UtcNow));

        CreateMap<Subscription, SubscriptionDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId.ToString()))
            .ForMember(dest => dest.PlanId, opt => opt.MapFrom(src => src.SubscriptionPlanId.ToString()))
            .ForMember(dest => dest.PlanName, opt => opt.MapFrom(src => src.SubscriptionPlan.Name))
            .ForMember(dest => dest.PlanDescription, opt => opt.MapFrom(src => src.SubscriptionPlan.Description))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.StatusReason, opt => opt.MapFrom(src => src.StatusReason))
            .ForMember(dest => dest.CurrentPrice, opt => opt.MapFrom(src => src.CurrentPrice))
            .ForMember(dest => dest.AutoRenew, opt => opt.MapFrom(src => src.AutoRenew))
            .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes))
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
            .ForMember(dest => dest.NextBillingDate, opt => opt.MapFrom(src => src.NextBillingDate))
            .ForMember(dest => dest.PausedDate, opt => opt.MapFrom(src => src.PausedDate))
            .ForMember(dest => dest.ResumedDate, opt => opt.MapFrom(src => src.ResumedDate))
            .ForMember(dest => dest.CancelledDate, opt => opt.MapFrom(src => src.CancelledDate))
            .ForMember(dest => dest.ExpirationDate, opt => opt.MapFrom(src => src.ExpirationDate))
            .ForMember(dest => dest.CancellationReason, opt => opt.MapFrom(src => src.CancellationReason))
            .ForMember(dest => dest.PauseReason, opt => opt.MapFrom(src => src.PauseReason))
            .ForMember(dest => dest.StripeSubscriptionId, opt => opt.MapFrom(src => src.StripeSubscriptionId))
            .ForMember(dest => dest.StripeCustomerId, opt => opt.MapFrom(src => src.StripeCustomerId))
            .ForMember(dest => dest.PaymentMethodId, opt => opt.MapFrom(src => src.PaymentMethodId))
            .ForMember(dest => dest.LastPaymentDate, opt => opt.MapFrom(src => src.LastPaymentDate))
            .ForMember(dest => dest.LastPaymentFailedDate, opt => opt.MapFrom(src => src.LastPaymentFailedDate))
            .ForMember(dest => dest.LastPaymentError, opt => opt.MapFrom(src => src.LastPaymentError))
            .ForMember(dest => dest.FailedPaymentAttempts, opt => opt.MapFrom(src => src.FailedPaymentAttempts))
            .ForMember(dest => dest.IsTrialSubscription, opt => opt.MapFrom(src => src.IsTrialSubscription))
            .ForMember(dest => dest.TrialStartDate, opt => opt.MapFrom(src => src.TrialStartDate))
            .ForMember(dest => dest.TrialEndDate, opt => opt.MapFrom(src => src.TrialEndDate))
            .ForMember(dest => dest.TrialDurationInDays, opt => opt.MapFrom(src => src.TrialDurationInDays))
            .ForMember(dest => dest.LastUsedDate, opt => opt.MapFrom(src => src.LastUsedDate))
            .ForMember(dest => dest.TotalUsageCount, opt => opt.MapFrom(src => src.TotalUsageCount))
            .ForMember(dest => dest.StatusHistory, opt => opt.MapFrom(src => src.StatusHistory))
            .ForMember(dest => dest.Payments, opt => opt.MapFrom(src => src.Payments))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.IsPaused, opt => opt.MapFrom(src => src.IsPaused))
            .ForMember(dest => dest.IsCancelled, opt => opt.MapFrom(src => src.IsCancelled))
            .ForMember(dest => dest.IsExpired, opt => opt.MapFrom(src => src.IsExpired))
            .ForMember(dest => dest.HasPaymentIssues, opt => opt.MapFrom(src => src.HasPaymentIssues))
            .ForMember(dest => dest.IsInTrial, opt => opt.MapFrom(src => src.IsInTrial))
            .ForMember(dest => dest.DaysUntilNextBilling, opt => opt.MapFrom(src => src.DaysUntilNextBilling))
            .ForMember(dest => dest.IsNearExpiration, opt => opt.MapFrom(src => src.IsNearExpiration))
            .ForMember(dest => dest.CanPause, opt => opt.MapFrom(src => src.CanPause))
            .ForMember(dest => dest.CanResume, opt => opt.MapFrom(src => src.CanResume))
            .ForMember(dest => dest.CanCancel, opt => opt.MapFrom(src => src.CanCancel))
            .ForMember(dest => dest.CanRenew, opt => opt.MapFrom(src => src.CanRenew))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
            .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => src.UpdatedDate));
        CreateMap<SubscriptionStatusHistory, SubscriptionStatusHistoryDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.SubscriptionId, opt => opt.MapFrom(src => src.SubscriptionId.ToString()))
            .ForMember(dest => dest.FromStatus, opt => opt.MapFrom(src => src.FromStatus))
            .ForMember(dest => dest.ToStatus, opt => opt.MapFrom(src => src.ToStatus))
            .ForMember(dest => dest.Reason, opt => opt.MapFrom(src => src.Reason))
            .ForMember(dest => dest.ChangedByUserId, opt => opt.MapFrom(src => src.ChangedByUserId))
            .ForMember(dest => dest.ChangedAt, opt => opt.MapFrom(src => src.ChangedAt))
            .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => src.Metadata))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
            .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => src.UpdatedDate));
        CreateMap<SubscriptionPayment, SubscriptionPaymentDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.SubscriptionId, opt => opt.MapFrom(src => src.SubscriptionId.ToString()))
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
            .ForMember(dest => dest.TaxAmount, opt => opt.MapFrom(src => src.TaxAmount))
            .ForMember(dest => dest.NetAmount, opt => opt.MapFrom(src => src.NetAmount))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.FailureReason, opt => opt.MapFrom(src => src.FailureReason))
            .ForMember(dest => dest.DueDate, opt => opt.MapFrom(src => src.DueDate))
            .ForMember(dest => dest.PaidAt, opt => opt.MapFrom(src => src.PaidAt))
            .ForMember(dest => dest.FailedAt, opt => opt.MapFrom(src => src.FailedAt))
            .ForMember(dest => dest.BillingPeriodStart, opt => opt.MapFrom(src => src.BillingPeriodStart))
            .ForMember(dest => dest.BillingPeriodEnd, opt => opt.MapFrom(src => src.BillingPeriodEnd))
            .ForMember(dest => dest.StripePaymentIntentId, opt => opt.MapFrom(src => src.StripePaymentIntentId))
            .ForMember(dest => dest.StripeInvoiceId, opt => opt.MapFrom(src => src.StripeInvoiceId))
            .ForMember(dest => dest.ReceiptUrl, opt => opt.MapFrom(src => src.ReceiptUrl))
            .ForMember(dest => dest.PaymentIntentId, opt => opt.MapFrom(src => src.PaymentIntentId))
            .ForMember(dest => dest.InvoiceId, opt => opt.MapFrom(src => src.InvoiceId))
            .ForMember(dest => dest.AttemptCount, opt => opt.MapFrom(src => src.AttemptCount))
            .ForMember(dest => dest.NextRetryAt, opt => opt.MapFrom(src => src.NextRetryAt))
            .ForMember(dest => dest.RefundedAmount, opt => opt.MapFrom(src => src.RefundedAmount))
            .ForMember(dest => dest.Refunds, opt => opt.MapFrom(src => src.Refunds))
            .ForMember(dest => dest.IsPaid, opt => opt.MapFrom(src => src.IsPaid))
            .ForMember(dest => dest.IsFailed, opt => opt.MapFrom(src => src.IsFailed))
            .ForMember(dest => dest.IsRefunded, opt => opt.MapFrom(src => src.IsRefunded))
            .ForMember(dest => dest.IsOverdue, opt => opt.MapFrom(src => src.IsOverdue))
            .ForMember(dest => dest.RemainingAmount, opt => opt.MapFrom(src => src.RemainingAmount))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
            .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => src.UpdatedDate));
        CreateMap<PaymentRefund, PaymentRefundDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.SubscriptionPaymentId, opt => opt.MapFrom(src => src.SubscriptionPaymentId.ToString()))
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
            .ForMember(dest => dest.Reason, opt => opt.MapFrom(src => src.Reason))
            .ForMember(dest => dest.StripeRefundId, opt => opt.MapFrom(src => src.StripeRefundId))
            .ForMember(dest => dest.RefundedAt, opt => opt.MapFrom(src => src.RefundedAt))
            .ForMember(dest => dest.ProcessedByUserId, opt => opt.MapFrom(src => src.ProcessedByUserId.HasValue ? src.ProcessedByUserId.Value.ToString() : null));
        // Plan mappings
        CreateMap<SubscriptionPlan, SubscriptionPlanDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.ShortDescription, opt => opt.MapFrom(src => src.ShortDescription))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
            .ForMember(dest => dest.DiscountedPrice, opt => opt.MapFrom(src => src.DiscountedPrice))
            .ForMember(dest => dest.DiscountValidUntil, opt => opt.MapFrom(src => src.DiscountValidUntil))
            .ForMember(dest => dest.BillingCycleId, opt => opt.MapFrom(src => src.BillingCycleId))
            .ForMember(dest => dest.CurrencyId, opt => opt.MapFrom(src => src.CurrencyId))
            .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.IsFeatured, opt => opt.MapFrom(src => src.IsFeatured))
            .ForMember(dest => dest.IsTrialAllowed, opt => opt.MapFrom(src => src.IsTrialAllowed))
            .ForMember(dest => dest.TrialDurationInDays, opt => opt.MapFrom(src => src.TrialDurationInDays))
            .ForMember(dest => dest.IsMostPopular, opt => opt.MapFrom(src => src.IsMostPopular))
            .ForMember(dest => dest.IsTrending, opt => opt.MapFrom(src => src.IsTrending))
            .ForMember(dest => dest.DisplayOrder, opt => opt.MapFrom(src => src.DisplayOrder))
            .ForMember(dest => dest.StripeProductId, opt => opt.MapFrom(src => src.StripeProductId))
            .ForMember(dest => dest.StripeMonthlyPriceId, opt => opt.MapFrom(src => src.StripeMonthlyPriceId))
            .ForMember(dest => dest.StripeQuarterlyPriceId, opt => opt.MapFrom(src => src.StripeQuarterlyPriceId))
            .ForMember(dest => dest.StripeAnnualPriceId, opt => opt.MapFrom(src => src.StripeAnnualPriceId))
            .ForMember(dest => dest.Features, opt => opt.MapFrom(src => src.Features))
            .ForMember(dest => dest.Terms, opt => opt.MapFrom(src => src.Terms))
            .ForMember(dest => dest.EffectiveDate, opt => opt.MapFrom(src => src.EffectiveDate))
            .ForMember(dest => dest.ExpirationDate, opt => opt.MapFrom(src => src.ExpirationDate))
            .ForMember(dest => dest.EffectivePrice, opt => opt.MapFrom(src => src.EffectivePrice))
            .ForMember(dest => dest.HasActiveDiscount, opt => opt.MapFrom(src => src.HasActiveDiscount))
            .ForMember(dest => dest.IsCurrentlyAvailable, opt => opt.MapFrom(src => src.IsCurrentlyAvailable))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
            .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => src.UpdatedDate));

        // Create Subscription Plan Mapping
        CreateMap<CreateSubscriptionPlanDto, SubscriptionPlan>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.ShortDescription, opt => opt.MapFrom(src => src.ShortDescription))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
            .ForMember(dest => dest.DiscountedPrice, opt => opt.MapFrom(src => src.DiscountedPrice))
            .ForMember(dest => dest.DiscountValidUntil, opt => opt.MapFrom(src => src.DiscountValidUntil))
            .ForMember(dest => dest.BillingCycleId, opt => opt.MapFrom(src => src.BillingCycleId))
            .ForMember(dest => dest.CurrencyId, opt => opt.MapFrom(src => src.CurrencyId))
            .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.IsFeatured, opt => opt.MapFrom(src => src.IsFeatured))
            .ForMember(dest => dest.IsTrialAllowed, opt => opt.MapFrom(src => src.IsTrialAllowed))
            .ForMember(dest => dest.TrialDurationInDays, opt => opt.MapFrom(src => src.TrialDurationInDays))
            .ForMember(dest => dest.IsMostPopular, opt => opt.MapFrom(src => src.IsMostPopular))
            .ForMember(dest => dest.IsTrending, opt => opt.MapFrom(src => src.IsTrending))
            .ForMember(dest => dest.DisplayOrder, opt => opt.MapFrom(src => src.DisplayOrder))
            .ForMember(dest => dest.Features, opt => opt.MapFrom(src => src.Features))
            .ForMember(dest => dest.Terms, opt => opt.MapFrom(src => src.Terms))
            .ForMember(dest => dest.EffectiveDate, opt => opt.MapFrom(src => src.EffectiveDate))
            .ForMember(dest => dest.ExpirationDate, opt => opt.MapFrom(src => src.ExpirationDate))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => DateTime.UtcNow));

        // Update Subscription Plan Mapping
        CreateMap<UpdateSubscriptionPlanDto, SubscriptionPlan>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
            .ForMember(dest => dest.BillingCycleId, opt => opt.MapFrom(src => src.BillingCycleId))
            .ForMember(dest => dest.CurrencyId, opt => opt.MapFrom(src => src.CurrencyId))
            .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => DateTime.UtcNow));

        // Category mappings
        CreateMap<Category, CategoryDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Icon, opt => opt.MapFrom(src => src.Icon))
            .ForMember(dest => dest.Color, opt => opt.MapFrom(src => src.Color))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.DisplayOrder, opt => opt.MapFrom(src => src.DisplayOrder))
            .ForMember(dest => dest.Features, opt => opt.MapFrom(src => src.Features))
            .ForMember(dest => dest.ConsultationDescription, opt => opt.MapFrom(src => src.ConsultationDescription))
            .ForMember(dest => dest.BasePrice, opt => opt.MapFrom(src => src.BasePrice))
            .ForMember(dest => dest.ConsultationFee, opt => opt.MapFrom(src => src.ConsultationFee))
            .ForMember(dest => dest.ConsultationDurationMinutes, opt => opt.MapFrom(src => src.ConsultationDurationMinutes))
            .ForMember(dest => dest.RequiresHealthAssessment, opt => opt.MapFrom(src => src.RequiresHealthAssessment))
            .ForMember(dest => dest.AllowsMedicationDelivery, opt => opt.MapFrom(src => src.AllowsMedicationDelivery))
            .ForMember(dest => dest.AllowsFollowUpMessaging, opt => opt.MapFrom(src => src.AllowsFollowUpMessaging))
            .ForMember(dest => dest.AllowsOneTimeConsultation, opt => opt.MapFrom(src => src.AllowsOneTimeConsultation))
            .ForMember(dest => dest.OneTimeConsultationFee, opt => opt.MapFrom(src => src.OneTimeConsultationFee))
            .ForMember(dest => dest.OneTimeConsultationDurationMinutes, opt => opt.MapFrom(src => src.OneTimeConsultationDurationMinutes))
            .ForMember(dest => dest.IsMostPopular, opt => opt.MapFrom(src => src.IsMostPopular))
            .ForMember(dest => dest.IsTrending, opt => opt.MapFrom(src => src.IsTrending))
            .ForMember(dest => dest.SubscriptionPlans, opt => opt.MapFrom(src => src.SubscriptionPlans));

        // Privilege mappings
        CreateMap<Privilege, PrivilegeDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.PrivilegeTypeId, opt => opt.MapFrom(src => src.PrivilegeTypeId))
            .ForMember(dest => dest.PrivilegeTypeName, opt => opt.MapFrom(src => src.PrivilegeType.Name))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
            .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => src.UpdatedDate));

        // Create Privilege Mapping
        CreateMap<CreatePrivilegeDto, Privilege>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.PrivilegeTypeId, opt => opt.MapFrom(src => src.PrivilegeTypeId))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => DateTime.UtcNow));

        // Update Privilege Mapping
        CreateMap<UpdatePrivilegeDto, Privilege>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.PrivilegeTypeId, opt => opt.MapFrom(src => src.PrivilegeTypeId))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => DateTime.UtcNow));

        // Subscription Plan Privilege mappings
        CreateMap<SubscriptionPlanPrivilege, PlanPrivilegeDto>()
            .ForMember(dest => dest.PrivilegeId, opt => opt.MapFrom(src => src.PrivilegeId))
            .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value))
            // .ForMember(dest => dest.UsagePeriodId, opt => opt.MapFrom(src => src.UsagePeriodId)) // REMOVED - not used
            .ForMember(dest => dest.DurationMonths, opt => opt.MapFrom(src => src.DurationMonths))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.EffectiveDate, opt => opt.MapFrom(src => src.EffectiveDate))
            .ForMember(dest => dest.ExpirationDate, opt => opt.MapFrom(src => src.ExpirationDate))
            // Time-based limits removed
            .ForMember(dest => dest.UnitCost, opt => opt.MapFrom(src => src.UnitCost));

        CreateMap<PlanPrivilegeDto, SubscriptionPlanPrivilege>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForMember(dest => dest.PrivilegeId, opt => opt.MapFrom(src => src.PrivilegeId))
            .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value))
            // .ForMember(dest => dest.UsagePeriodId, opt => opt.MapFrom(src => src.UsagePeriodId)) // REMOVED - not used
            .ForMember(dest => dest.DurationMonths, opt => opt.MapFrom(src => src.DurationMonths))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.EffectiveDate, opt => opt.MapFrom(src => src.EffectiveDate))
            .ForMember(dest => dest.ExpirationDate, opt => opt.MapFrom(src => src.ExpirationDate))
            // Time-based limits removed
            .ForMember(dest => dest.UnitCost, opt => opt.MapFrom(src => src.UnitCost))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => DateTime.UtcNow));

        // Master Data mappings
        CreateMap<MasterBillingCycle, BillingCycleDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => DateTime.UtcNow.AddDays(30)))
            .ForMember(dest => dest.BillingCycleId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "active"))
            .ForMember(dest => dest.AutoProcess, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.GracePeriodDays, opt => opt.MapFrom(src => 7))
            .ForMember(dest => dest.TotalSubscriptions, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.ProcessedSubscriptions, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow));

        CreateMap<MasterCurrency, BillingCycleDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Code)) // Use Code instead of Description
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => DateTime.UtcNow.AddDays(30)))
            .ForMember(dest => dest.BillingCycleId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "active"))
            .ForMember(dest => dest.AutoProcess, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.GracePeriodDays, opt => opt.MapFrom(src => 7))
            .ForMember(dest => dest.TotalSubscriptions, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.ProcessedSubscriptions, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow));

        // Chat mappings
        CreateMap<ChatRoom, ChatRoomDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()));

        CreateMap<Message, MessageDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.ChatRoomId, opt => opt.MapFrom(src => src.ChatRoomId.ToString()))
            .ForMember(dest => dest.SenderId, opt => opt.MapFrom(src => src.SenderId.ToString()));

        // Provider mappings
        CreateMap<Provider, ProviderDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => 0)) // Provider doesn't have UserId
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => "")) // Provider doesn't have User
            .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => "")) // Provider doesn't have CategoryId
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => "")) // Provider doesn't have Category
            .ForMember(dest => dest.LicenseNumber, opt => opt.MapFrom(src => src.LicenseNumber))
            .ForMember(dest => dest.Specialization, opt => opt.MapFrom(src => src.Specialty)) // Use Specialty instead of Specialization
            .ForMember(dest => dest.Bio, opt => opt.MapFrom(src => src.Bio))
            .ForMember(dest => dest.Education, opt => opt.MapFrom(src => src.Education))
            .ForMember(dest => dest.Experience, opt => opt.MapFrom(src => "")) // Provider doesn't have Experience
            .ForMember(dest => dest.Certifications, opt => opt.MapFrom(src => src.Certifications))
            .ForMember(dest => dest.ConsultationDurationMinutes, opt => opt.MapFrom(src => 30)) // Provider doesn't have this property
            .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(src => src.IsAvailable))
            .ForMember(dest => dest.ProfilePicture, opt => opt.MapFrom(src => "")) // Provider doesn't have ProfilePicture
            .ForMember(dest => dest.Languages, opt => opt.MapFrom(src => "")) // Provider doesn't have Languages
            .ForMember(dest => dest.TimeZone, opt => opt.MapFrom(src => "")) // Provider doesn't have TimeZone
            .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => 0)) // Provider doesn't have Rating
            .ForMember(dest => dest.ReviewCount, opt => opt.MapFrom(src => 0)) // Provider doesn't have ReviewCount
            .ForMember(dest => dest.IsVerified, opt => opt.MapFrom(src => false)) // Provider doesn't have IsVerified
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
            .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => src.UpdatedDate))
            .ForMember(dest => dest.StripeAccountId, opt => opt.MapFrom(src => "")) // Provider doesn't have StripeAccountId
            .ForMember(dest => dest.StripeCustomerId, opt => opt.MapFrom(src => "")) // Provider doesn't have StripeCustomerId
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName)) // Use computed property
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email)) // Use Provider's Email
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.PhoneNumber ?? "")) // Use Provider's PhoneNumber
            .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => (DateTime?)null)) // Provider doesn't have DateOfBirth
            .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => "")) // Provider doesn't have Gender
            .ForMember(dest => dest.UserType, opt => opt.MapFrom(src => "Provider")) // Set as Provider
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.IsEmailVerified, opt => opt.MapFrom(src => true)) // Assume verified
            .ForMember(dest => dest.IsPhoneVerified, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.PhoneNumber)))
            .ForMember(dest => dest.LastLoginAt, opt => opt.MapFrom(src => (DateTime?)null)); // Provider doesn't have LastLoginAt

        CreateMap<CreateProviderDto, Provider>()
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
            .ForMember(dest => dest.LicenseNumber, opt => opt.MapFrom(src => src.LicenseNumber))
            .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.State))
            .ForMember(dest => dest.Specialty, opt => opt.MapFrom(src => src.Specialty))
            .ForMember(dest => dest.Bio, opt => opt.MapFrom(src => src.Bio))
            .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(src => src.IsAvailable))
            .ForMember(dest => dest.ConsultationFee, opt => opt.MapFrom(src => 0)) // Default consultation fee
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => DateTime.UtcNow));

        CreateMap<UpdateProviderDto, Provider>()
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
            .ForMember(dest => dest.LicenseNumber, opt => opt.MapFrom(src => src.LicenseNumber))
            .ForMember(dest => dest.Specialty, opt => opt.MapFrom(src => src.Specialization)) // Map Specialization to Specialty
            .ForMember(dest => dest.Bio, opt => opt.MapFrom(src => src.Bio))
            .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => DateTime.UtcNow));

        // Consultation mappings
        CreateMap<Consultation, ConsultationDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes))
            .ForMember(dest => dest.IsOneTime, opt => opt.MapFrom(src => src.IsOneTime))
            .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId.ToString()))
            .ForMember(dest => dest.ProviderId, opt => opt.MapFrom(src => src.ProviderId.ToString()))
            .ForMember(dest => dest.ProviderName, opt => opt.MapFrom(src => src.Provider != null ? $"{src.Provider.FirstName} {src.Provider.LastName}" : ""))
            .ForMember(dest => dest.ScheduledAt, opt => opt.MapFrom(src => src.ScheduledAt))
            .ForMember(dest => dest.DurationMinutes, opt => opt.MapFrom(src => src.DurationMinutes))
            .ForMember(dest => dest.Fee, opt => opt.MapFrom(src => src.Fee))
            .ForMember(dest => dest.ConsultationMode, opt => opt.MapFrom(src => "Video")) // Default to Video since Consultation doesn't have ConsultationMode
            .ForMember(dest => dest.Reason, opt => opt.MapFrom(src => "")) // Consultation doesn't have Reason
            .ForMember(dest => dest.Symptoms, opt => opt.MapFrom(src => "")) // Consultation doesn't have Symptoms
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
            .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => src.UpdatedDate));

        CreateMap<CreateConsultationDto, Consultation>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => Enum.Parse<Consultation.ConsultationType>(src.Type)))
            .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes))
            .ForMember(dest => dest.IsOneTime, opt => opt.MapFrom(src => src.IsOneTime))
            .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.CategoryId) ? Guid.Empty : Guid.Parse(src.CategoryId)))
            .ForMember(dest => dest.ProviderId, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.ProviderId) ? 0 : int.Parse(src.ProviderId)))
            .ForMember(dest => dest.ScheduledAt, opt => opt.MapFrom(src => src.ScheduledAt ?? DateTime.UtcNow))
            .ForMember(dest => dest.DurationMinutes, opt => opt.MapFrom(src => src.DurationMinutes ?? 30))
            .ForMember(dest => dest.Fee, opt => opt.MapFrom(src => src.Fee ?? 0))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Consultation.ConsultationStatus.Scheduled))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => DateTime.UtcNow));

        CreateMap<UpdateConsultationDto, Consultation>()
            .ForMember(dest => dest.ScheduledAt, opt => opt.MapFrom(src => src.ScheduledAt ?? DateTime.UtcNow))
            .ForMember(dest => dest.DurationMinutes, opt => opt.MapFrom(src => src.DurationMinutes ?? 30))
            .ForMember(dest => dest.Fee, opt => opt.MapFrom(src => src.Fee ?? 0))
            .ForMember(dest => dest.Diagnosis, opt => opt.MapFrom(src => src.Diagnosis))
            .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => DateTime.UtcNow));

        // Notification mappings
        CreateMap<Notification, NotificationDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.IsRead, opt => opt.MapFrom(src => src.IsRead))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
            .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => src.UpdatedDate))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.ReadAt, opt => opt.MapFrom(src => src.ReadAt))
            .ForMember(dest => dest.ScheduledAt, opt => opt.MapFrom(src => src.ScheduledAt));

        // BillingRecord mappings
        CreateMap<CreateBillingRecordDto, BillingRecord>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.SubscriptionId, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.SubscriptionId) ? (Guid?)null : Guid.Parse(src.SubscriptionId)))
            .ForMember(dest => dest.ConsultationId, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.ConsultationId) ? (Guid?)null : Guid.Parse(src.ConsultationId)))
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.DueDate, opt => opt.MapFrom(src => src.DueDate))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
            .ForMember(dest => dest.StripeInvoiceId, opt => opt.MapFrom(src => src.StripeInvoiceId))
            .ForMember(dest => dest.StripePaymentIntentId, opt => opt.MapFrom(src => src.StripePaymentIntentId))
            .ForMember(dest => dest.TaxAmount, opt => opt.MapFrom(src => src.TaxAmount))
            .ForMember(dest => dest.ShippingAmount, opt => opt.MapFrom(src => src.ShippingAmount))
            .ForMember(dest => dest.BillingDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.CurrencyId, opt => opt.MapFrom(src => Guid.Empty)) // Default currency
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => BillingRecord.BillingStatus.Pending));

        CreateMap<BillingRecord, BillingRecordDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.SubscriptionId, opt => opt.MapFrom(src => src.SubscriptionId.HasValue ? src.SubscriptionId.Value.ToString() : null))
            .ForMember(dest => dest.ConsultationId, opt => opt.MapFrom(src => src.ConsultationId.HasValue ? src.ConsultationId.Value.ToString() : null))
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.DueDate, opt => opt.MapFrom(src => src.DueDate))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.StripeInvoiceId, opt => opt.MapFrom(src => src.StripeInvoiceId))
            .ForMember(dest => dest.StripePaymentIntentId, opt => opt.MapFrom(src => src.StripePaymentIntentId))
            .ForMember(dest => dest.TaxAmount, opt => opt.MapFrom(src => src.TaxAmount))
            .ForMember(dest => dest.ShippingAmount, opt => opt.MapFrom(src => src.ShippingAmount))
            .ForMember(dest => dest.BillingDate, opt => opt.MapFrom(src => src.BillingDate))
            .ForMember(dest => dest.PaidAt, opt => opt.MapFrom(src => src.PaidAt))
            .ForMember(dest => dest.InvoiceNumber, opt => opt.MapFrom(src => src.InvoiceNumber))
            .ForMember(dest => dest.FailureReason, opt => opt.MapFrom(src => src.FailureReason))
            .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.PaymentMethod))
            .ForMember(dest => dest.IsRecurring, opt => opt.MapFrom(src => src.IsRecurring))
            .ForMember(dest => dest.PaymentIntentId, opt => opt.MapFrom(src => src.PaymentIntentId))
            .ForMember(dest => dest.AccruedAmount, opt => opt.MapFrom(src => src.AccruedAmount))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
            .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => src.UpdatedDate));

        // UpdateTimeBasedLimitsDto mapping (time-based limits removed)
        CreateMap<UpdateTimeBasedLimitsDto, SubscriptionPlanPrivilege>()
            // Time-based limits removed
            // .ForMember(dest => dest.UsagePeriodId, opt => opt.MapFrom(src => src.UsagePeriodId)) // REMOVED - not used
            .ForMember(dest => dest.DurationMonths, opt => opt.MapFrom(src => src.DurationMonths));

    }
} 