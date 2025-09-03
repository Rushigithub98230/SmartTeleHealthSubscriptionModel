using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Enums;
using SmartTelehealth.Infrastructure.Entities;

namespace SmartTelehealth.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<User, Role, int>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    
    // Current User ID for audit tracking
    public int? CurrentUserId { get; set; }
    
    // Master Tables DbSets
    public DbSet<MasterBillingCycle> MasterBillingCycles { get; set; }
    public DbSet<MasterCurrency> MasterCurrencies { get; set; }
    public DbSet<MasterPrivilegeType> MasterPrivilegeTypes { get; set; }
    public new DbSet<UserRole> UserRoles { get; set; }
    public DbSet<AppointmentStatus> AppointmentStatuses { get; set; }
    public DbSet<PaymentStatus> PaymentStatuses { get; set; }
    public DbSet<RefundStatus> RefundStatuses { get; set; }
    public DbSet<ParticipantStatus> ParticipantStatuses { get; set; }
    public DbSet<ParticipantRole> ParticipantRoles { get; set; }
    public DbSet<InvitationStatus> InvitationStatuses { get; set; }
    public DbSet<AppointmentType> AppointmentTypes { get; set; }
    public DbSet<ConsultationMode> ConsultationModes { get; set; }
    public DbSet<DocumentType> DocumentTypes { get; set; }
    public DbSet<ReminderType> ReminderTypes { get; set; }
    public DbSet<ReminderTiming> ReminderTimings { get; set; }
    public DbSet<EventType> EventTypes { get; set; }
    
    // DbSets
    public DbSet<Provider> Providers { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<HealthAssessment> HealthAssessments { get; set; }
    public DbSet<Consultation> Consultations { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<MessageAttachment> MessageAttachments { get; set; }
    public DbSet<ChatRoom> ChatRooms { get; set; }
    public DbSet<ChatRoomParticipant> ChatRoomParticipants { get; set; }
    public DbSet<MessageReaction> MessageReactions { get; set; }
    public DbSet<MessageReadReceipt> MessageReadReceipts { get; set; }
    public DbSet<MedicationDelivery> MedicationDeliveries { get; set; }
    public DbSet<DeliveryTracking> DeliveryTracking { get; set; }
    public DbSet<Prescription> Prescriptions { get; set; }
    public DbSet<PrescriptionItem> PrescriptionItems { get; set; }
    public DbSet<BillingRecord> BillingRecords { get; set; }
    public DbSet<BillingAdjustment> BillingAdjustments { get; set; }
    public DbSet<ProviderCategory> ProviderCategories { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    
    // Appointment entities
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<AppointmentParticipant> AppointmentParticipants { get; set; }
    public DbSet<AppointmentInvitation> AppointmentInvitations { get; set; }
    public DbSet<AppointmentPaymentLog> AppointmentPaymentLogs { get; set; }
    public DbSet<AppointmentDocument> AppointmentDocuments { get; set; }
    public DbSet<AppointmentReminder> AppointmentReminders { get; set; }
    public DbSet<AppointmentEvent> AppointmentEvents { get; set; }
    
    // Document management
    public DbSet<Document> Documents { get; set; }
    public DbSet<DocumentReference> DocumentReferences { get; set; }
        
    // Video Call entities
    public DbSet<VideoCall> VideoCalls { get; set; }
    public DbSet<VideoCallParticipant> VideoCallParticipants { get; set; }
    public DbSet<VideoCallEvent> VideoCallEvents { get; set; }
    
    public DbSet<SubscriptionPayment> SubscriptionPayments { get; set; }
    public DbSet<Privilege> Privileges { get; set; }
    public DbSet<SubscriptionPlanPrivilege> SubscriptionPlanPrivileges { get; set; }
    public DbSet<UserSubscriptionPrivilegeUsage> UserSubscriptionPrivilegeUsages { get; set; }
    public DbSet<PrivilegeUsageHistory> PrivilegeUsageHistories { get; set; }
    
            // CategoryQuestion and CategoryQuestionAnswer removed - redundant with Questionnaire system
    public DbSet<SubscriptionStatusHistory> SubscriptionStatusHistories { get; set; }
    public DbSet<PaymentRefund> PaymentRefunds { get; set; }
    
    public DbSet<QuestionnaireTemplate> QuestionnaireTemplates { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<QuestionOption> QuestionOptions { get; set; }
    public DbSet<UserResponse> UserResponses { get; set; }
    public DbSet<UserAnswer> UserAnswers { get; set; }
    public DbSet<UserAnswerOption> UserAnswerOptions { get; set; }
    
    public DbSet<ProviderOnboarding> ProviderOnboardings { get; set; }
    public DbSet<ProviderFee> ProviderFees { get; set; }
    public DbSet<CategoryFeeRange> CategoryFeeRanges { get; set; }
    
    // Chat-related entities
    public DbSet<ChatSession> ChatSessions { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }
    public DbSet<ChatAttachment> ChatAttachments { get; set; }
    public DbSet<ServiceConstraint> ServiceConstraints { get; set; }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // Configure master tables
        ConfigureMasterTables(builder);
        
        // Configure entity relationships and constraints
        ConfigureUser(builder);
        ConfigureProvider(builder);
        ConfigureCategory(builder);
        ConfigureSubscriptionPlan(builder);
        ConfigureSubscription(builder);
        ConfigureHealthAssessment(builder);
        ConfigureConsultation(builder);
        ConfigureMessage(builder);
        ConfigureChatRoom(builder);
        ConfigureChatRoomParticipant(builder);
        ConfigureMessageReaction(builder);
        ConfigureMessageReadReceipt(builder);
        ConfigureMedicationDelivery(builder);
        ConfigureBillingRecord(builder);
        ConfigureProviderCategory(builder);
        ConfigureNotification(builder);
        ConfigureAuditLog(builder);
        ConfigureAppointment(builder);
        ConfigureAppointmentParticipant(builder);
        ConfigureAppointmentInvitation(builder);
        ConfigureAppointmentPaymentLog(builder);
        ConfigureAppointmentDocument(builder);
        ConfigureAppointmentReminder(builder);
        ConfigureAppointmentEvent(builder);
        ConfigureVideoCall(builder);
        ConfigureVideoCallParticipant(builder);
        ConfigureVideoCallEvent(builder);
        ConfigureSubscriptionPayment(builder);
        ConfigureSubscriptionStatusHistory(builder);
        ConfigurePaymentRefund(builder);
        ConfigureCategoryFeeRange(builder);
        ConfigureProviderFee(builder);
        ConfigureDocument(builder);
        ConfigureQuestionnaireSystem(builder);
        ConfigurePrivilegeUsageHistory(builder);
    }
    
    private void ConfigureMasterTables(ModelBuilder builder)
    {
        // UserRole
        builder.Entity<UserRole>(entity =>
        {
            entity.ToTable("UserRoles");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);
        });
        
        // AppointmentStatus
        builder.Entity<AppointmentStatus>(entity =>
        {
            entity.ToTable("AppointmentStatuses");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);
            entity.Property(e => e.Color).HasMaxLength(50);
            entity.Property(e => e.Icon).HasMaxLength(50);
        });
        
        // PaymentStatus
        builder.Entity<PaymentStatus>(entity =>
        {
            entity.ToTable("PaymentStatuses");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);
            entity.Property(e => e.Color).HasMaxLength(50);
        });
        
        // RefundStatus
        builder.Entity<RefundStatus>(entity =>
        {
            entity.ToTable("RefundStatuses");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);
            entity.Property(e => e.Color).HasMaxLength(50);
        });
        
        // ParticipantStatus
        builder.Entity<ParticipantStatus>(entity =>
        {
            entity.ToTable("ParticipantStatuses");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);
            entity.Property(e => e.Color).HasMaxLength(50);
        });
        
        // ParticipantRole
        builder.Entity<ParticipantRole>(entity =>
        {
            entity.ToTable("ParticipantRoles");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);
            entity.Property(e => e.Color).HasMaxLength(50);
        });
        
        // InvitationStatus
        builder.Entity<InvitationStatus>(entity =>
        {
            entity.ToTable("InvitationStatuses");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);
            entity.Property(e => e.Color).HasMaxLength(50);
        });
        
        // AppointmentType
        builder.Entity<AppointmentType>(entity =>
        {
            entity.ToTable("AppointmentTypes");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);
            entity.Property(e => e.Color).HasMaxLength(50);
        });
        
        // ConsultationMode
        builder.Entity<ConsultationMode>(entity =>
        {
            entity.ToTable("ConsultationModes");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);
            entity.Property(e => e.Color).HasMaxLength(50);
        });
        
        // DocumentType
        builder.Entity<DocumentType>(entity =>
        {
            entity.ToTable("DocumentTypes");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.DisplayOrder).HasDefaultValue(0);
            entity.Property(e => e.Icon).HasMaxLength(50);
        });
        
        // ReminderType
        builder.Entity<ReminderType>(entity =>
        {
            entity.ToTable("ReminderTypes");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);
        });
        
        // ReminderTiming
        builder.Entity<ReminderTiming>(entity =>
        {
            entity.ToTable("ReminderTimings");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);
            entity.Property(e => e.MinutesBeforeAppointment).HasDefaultValue(0);
        });
        
        // EventType
        builder.Entity<EventType>(entity =>
        {
            entity.ToTable("EventTypes");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);
        });

        // MasterBillingCycle
        builder.Entity<MasterBillingCycle>(entity =>
        {
            entity.ToTable("MasterBillingCycles");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);
        });
        // MasterCurrency
        builder.Entity<MasterCurrency>(entity =>
        {
            entity.ToTable("MasterCurrencies");
            entity.Property(e => e.Code).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Symbol).HasMaxLength(10);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);
        });
        // MasterPrivilegeType
        builder.Entity<MasterPrivilegeType>(entity =>
        {
            entity.ToTable("MasterPrivilegeTypes");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);
        });
    }
    
    private void ConfigureUser(ModelBuilder builder)
    {
        builder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.DateOfBirth).IsRequired();
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            
            // UserRole relationship
            entity.HasOne(e => e.UserRole)
                .WithMany(e => e.Users)
                .HasForeignKey(e => e.UserRoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
    
    private void ConfigureProvider(ModelBuilder builder)
    {
        builder.Entity<Provider>(entity =>
        {
            entity.ToTable("Providers");
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.LicenseNumber).IsRequired().HasMaxLength(100);
            entity.Property(e => e.State).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Specialty).IsRequired().HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsAvailable).HasDefaultValue(true);
            entity.Property(e => e.ConsultationFee).HasPrecision(18, 2);
        });
    }
    
    private void ConfigureCategory(ModelBuilder builder)
    {
        builder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.BasePrice).HasPrecision(18, 2);
            entity.Property(e => e.ConsultationFee).HasPrecision(18, 2);
            entity.Property(e => e.OneTimeConsultationFee).HasPrecision(18, 2);
            entity.Property(e => e.RequiresHealthAssessment).HasDefaultValue(true);
            entity.Property(e => e.AllowsMedicationDelivery).HasDefaultValue(true);
            entity.Property(e => e.AllowsFollowUpMessaging).HasDefaultValue(true);
        });
    }
    
    private void ConfigureSubscriptionPlan(ModelBuilder builder)
    {
        builder.Entity<SubscriptionPlan>(entity =>
        {
            entity.ToTable("SubscriptionPlans");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.DisplayOrder);
            // Remove old price fields and category reference
            // entity.Property(e => e.MonthlyPrice).HasPrecision(18, 2); // Removed
            // entity.Property(e => e.QuarterlyPrice).HasPrecision(18, 2); // Removed
            // entity.Property(e => e.AnnualPrice).HasPrecision(18, 2); // Removed
            // entity.HasOne(e => e.Category).WithMany(e => e.SubscriptionPlans).HasForeignKey(e => e.CategoryId).OnDelete(DeleteBehavior.Restrict); // Removed

            entity.HasOne(e => e.BillingCycle)
                .WithMany()
                .HasForeignKey(e => e.BillingCycleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Currency)
                .WithMany()
                .HasForeignKey(e => e.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
    
    private void ConfigureSubscription(ModelBuilder builder)
    {
        builder.Entity<Subscription>(entity =>
        {
            entity.ToTable("Subscriptions");
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.CurrentPrice).HasPrecision(18, 2);
            entity.Property(e => e.AutoRenew).HasDefaultValue(true);
            // Remove BillingFrequency
            // entity.Property(e => e.BillingFrequency).HasConversion<string>(); // Removed

            entity.HasOne(e => e.User)
                .WithMany(e => e.Subscriptions)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.SubscriptionPlan)
                .WithMany(e => e.Subscriptions)
                .HasForeignKey(e => e.SubscriptionPlanId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Provider)
                .WithMany()
                .HasForeignKey(e => e.ProviderId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.BillingCycle)
                .WithMany()
                .HasForeignKey(e => e.BillingCycleId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
    
    private void ConfigureHealthAssessment(ModelBuilder builder)
    {
        builder.Entity<HealthAssessment>(entity =>
        {
            entity.ToTable("HealthAssessments");
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.IsEligibleForTreatment).HasDefaultValue(false);
            entity.Property(e => e.RequiresFollowUp).HasDefaultValue(false);
            
            entity.HasOne(e => e.User)
                .WithMany(e => e.HealthAssessments)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Category)
                .WithMany(e => e.HealthAssessments)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Provider)
                .WithMany()
                .HasForeignKey(e => e.ProviderId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
    
    private void ConfigureConsultation(ModelBuilder builder)
    {
        builder.Entity<Consultation>(entity =>
        {
            entity.ToTable("Consultations");
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.Type).HasConversion<string>();
            entity.Property(e => e.Fee).HasPrecision(18, 2);
            entity.Property(e => e.RequiresFollowUp).HasDefaultValue(false);
            
            entity.HasOne(e => e.User)
                .WithMany(e => e.Consultations)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Provider)
                .WithMany(e => e.Consultations)
                .HasForeignKey(e => e.ProviderId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Category)
                .WithMany(e => e.Consultations)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Subscription)
                .WithMany(e => e.Consultations)
                .HasForeignKey(e => e.SubscriptionId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasOne(e => e.HealthAssessment)
                .WithMany(e => e.Consultations)
                .HasForeignKey(e => e.HealthAssessmentId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
    
    private void ConfigureMessage(ModelBuilder builder)
    {
        builder.Entity<Message>(entity =>
        {
            entity.ToTable("Messages");
            entity.Property(e => e.Type).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.Content).IsRequired().HasMaxLength(4000);
            entity.Property(e => e.FileName).HasMaxLength(255);
            entity.Property(e => e.FilePath).HasMaxLength(500);
            entity.Property(e => e.FileType).HasMaxLength(50);
            entity.Property(e => e.EncryptionKey).HasMaxLength(255);
            
            entity.HasOne(e => e.Sender)
                .WithMany()
                .HasForeignKey(e => e.SenderId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.ChatRoom)
                .WithMany(cr => cr.Messages)
                .HasForeignKey(e => e.ChatRoomId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.ReplyToMessage)
                .WithMany(e => e.Replies)
                .HasForeignKey(e => e.ReplyToMessageId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasMany(e => e.Reactions)
                .WithOne(r => r.Message)
                .HasForeignKey(r => r.MessageId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasMany(e => e.ReadReceipts)
                .WithOne(rr => rr.Message)
                .HasForeignKey(rr => rr.MessageId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        builder.Entity<MessageAttachment>(entity =>
        {
            entity.ToTable("MessageAttachments");
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FileType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.FileUrl).IsRequired().HasMaxLength(500);
            entity.Property(e => e.IsImage).HasDefaultValue(false);
            entity.Property(e => e.IsDocument).HasDefaultValue(false);
            entity.Property(e => e.IsVideo).HasDefaultValue(false);
            entity.Property(e => e.IsAudio).HasDefaultValue(false);
            
            entity.HasOne(e => e.Message)
                .WithMany(e => e.Attachments)
                .HasForeignKey(e => e.MessageId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
    
    private void ConfigureChatRoom(ModelBuilder builder)
    {
        builder.Entity<ChatRoom>(entity =>
        {
            entity.ToTable("ChatRooms");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Type).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.IsEncrypted).HasDefaultValue(true);
            entity.Property(e => e.AllowFileSharing).HasDefaultValue(true);
            entity.Property(e => e.AllowVoiceCalls).HasDefaultValue(true);
            entity.Property(e => e.AllowVideoCalls).HasDefaultValue(true);
            
            entity.HasOne(e => e.Patient)
                .WithMany()
                .HasForeignKey(e => e.PatientId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasOne(e => e.Provider)
                .WithMany()
                .HasForeignKey(e => e.ProviderId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasOne(e => e.Subscription)
                .WithMany()
                .HasForeignKey(e => e.SubscriptionId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasOne(e => e.Consultation)
                .WithMany()
                .HasForeignKey(e => e.ConsultationId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
    
    private void ConfigureChatRoomParticipant(ModelBuilder builder)
    {
        builder.Entity<ChatRoomParticipant>(entity =>
        {
            entity.ToTable("ChatRoomParticipants");
            entity.Property(e => e.Role).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.CanSendMessages).HasDefaultValue(true);
            entity.Property(e => e.CanSendFiles).HasDefaultValue(true);
            entity.Property(e => e.CanInviteOthers).HasDefaultValue(false);
            entity.Property(e => e.CanModerate).HasDefaultValue(false);
            
            entity.HasOne(e => e.ChatRoom)
                .WithMany(e => e.Participants)
                .HasForeignKey(e => e.ChatRoomId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Provider)
                .WithMany()
                .HasForeignKey(e => e.ProviderId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
    
    private void ConfigureMessageReaction(ModelBuilder builder)
    {
        builder.Entity<MessageReaction>(entity =>
        {
            entity.ToTable("MessageReactions");
            entity.Property(e => e.Emoji).IsRequired().HasMaxLength(10);
            
            entity.HasOne(e => e.Message)
                .WithMany(e => e.Reactions)
                .HasForeignKey(e => e.MessageId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Provider)
                .WithMany()
                .HasForeignKey(e => e.ProviderId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
    
    private void ConfigureMessageReadReceipt(ModelBuilder builder)
    {
        builder.Entity<MessageReadReceipt>(entity =>
        {
            entity.ToTable("MessageReadReceipts");
            entity.Property(e => e.DeviceInfo).HasMaxLength(100);
            entity.Property(e => e.IpAddress).HasMaxLength(50);
            
            entity.HasOne(e => e.Message)
                .WithMany(e => e.ReadReceipts)
                .HasForeignKey(e => e.MessageId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Provider)
                .WithMany()
                .HasForeignKey(e => e.ProviderId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
    
    private void ConfigureMedicationDelivery(ModelBuilder builder)
    {
        builder.Entity<MedicationDelivery>(entity =>
        {
            entity.ToTable("MedicationDeliveries");
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.DeliveryAddress).IsRequired().HasMaxLength(500);
            entity.Property(e => e.ShippingCost).HasPrecision(18, 2);
            entity.Property(e => e.RequiresSignature).HasDefaultValue(false);
            entity.Property(e => e.IsRefrigerated).HasDefaultValue(false);
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Subscription)
                .WithMany(e => e.MedicationDeliveries)
                .HasForeignKey(e => e.SubscriptionId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasOne(e => e.Consultation)
                .WithMany(e => e.MedicationDeliveries)
                .HasForeignKey(e => e.ConsultationId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasOne(e => e.Provider)
                .WithMany()
                .HasForeignKey(e => e.ProviderId)
                .OnDelete(DeleteBehavior.SetNull);
        });
        
        builder.Entity<DeliveryTracking>(entity =>
        {
            entity.ToTable("DeliveryTracking");
            entity.Property(e => e.EventType).HasConversion<string>();
            entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
            
            entity.HasOne(e => e.MedicationDelivery)
                .WithMany(e => e.TrackingEvents)
                .HasForeignKey(e => e.MedicationDeliveryId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
    
    private void ConfigureBillingRecord(ModelBuilder builder)
    {
        builder.Entity<BillingRecord>(entity =>
        {
            entity.ToTable("BillingRecords");
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.Type).HasConversion<string>();
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
            entity.Property(e => e.ShippingAmount).HasPrecision(18, 2);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            entity.Property(e => e.IsRecurring).HasDefaultValue(false);
            entity.Property(e => e.AccruedAmount).HasPrecision(18, 2);
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Subscription)
                .WithMany(e => e.BillingRecords)
                .HasForeignKey(e => e.SubscriptionId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasOne(e => e.Consultation)
                .WithMany()
                .HasForeignKey(e => e.ConsultationId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasOne(e => e.MedicationDelivery)
                .WithMany()
                .HasForeignKey(e => e.MedicationDeliveryId)
                .OnDelete(DeleteBehavior.SetNull);
        });
        
        builder.Entity<BillingAdjustment>(entity =>
        {
            entity.ToTable("BillingAdjustments");
            entity.Property(e => e.Type).HasConversion<string>();
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Percentage).HasPrecision(5, 2);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
            entity.Property(e => e.IsPercentage).HasDefaultValue(false);
            entity.Property(e => e.IsApproved).HasDefaultValue(true);
            
            entity.HasOne(e => e.BillingRecord)
                .WithMany(e => e.Adjustments)
                .HasForeignKey(e => e.BillingRecordId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
    
    private void ConfigureProviderCategory(ModelBuilder builder)
    {
        builder.Entity<ProviderCategory>(entity =>
        {
            entity.ToTable("ProviderCategories");
            entity.Property(e => e.IsPrimary).HasDefaultValue(false);
            entity.Property(e => e.IsAvailable).HasDefaultValue(true);
            entity.Property(e => e.ConsultationFee).HasPrecision(18, 2);
            
            entity.HasOne(e => e.Provider)
                .WithMany(e => e.ProviderCategories)
                .HasForeignKey(e => e.ProviderId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Category)
                .WithMany(e => e.ProviderCategories)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Composite unique constraint
            entity.HasIndex(e => new { e.ProviderId, e.CategoryId }).IsUnique();
        });
    }

    private void ConfigureNotification(ModelBuilder builder)
    {
        builder.Entity<Notification>(entity =>
        {
            entity.ToTable("Notifications");
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Message).IsRequired();
            entity.Property(e => e.IsRead).HasDefaultValue(false);
            entity.Property(e => e.CreatedDate).IsRequired();
        });
    }

    private void ConfigureAuditLog(ModelBuilder builder)
    {
        builder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            entity.Property(e => e.TableName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.DateTime).IsRequired();
            entity.Property(e => e.OldValues);
            entity.Property(e => e.NewValues);
            entity.Property(e => e.AffectedColumns).HasMaxLength(500);
            entity.Property(e => e.PrimaryKey).HasMaxLength(50);
        });
    }

    private void ConfigureVideoCall(ModelBuilder builder)
    {
        builder.Entity<VideoCall>(entity =>
        {
            entity.ToTable("VideoCalls");
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.SessionId).HasMaxLength(100);
            entity.Property(e => e.Token).HasMaxLength(500);
            entity.Property(e => e.RecordingUrl).HasMaxLength(500);
            
            entity.HasOne(e => e.Appointment)
                .WithMany()
                .HasForeignKey(e => e.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureVideoCallParticipant(ModelBuilder builder)
    {
        builder.Entity<VideoCallParticipant>(entity =>
        {
            entity.ToTable("VideoCallParticipants");
            entity.Property(e => e.IsInitiator).HasDefaultValue(false);
            entity.Property(e => e.IsVideoEnabled).HasDefaultValue(true);
            entity.Property(e => e.IsAudioEnabled).HasDefaultValue(true);
            entity.Property(e => e.IsScreenSharingEnabled).HasDefaultValue(false);
            entity.Property(e => e.DeviceInfo).HasMaxLength(100);
            entity.Property(e => e.IpAddress).HasMaxLength(50);
            entity.Property(e => e.UserAgent).HasMaxLength(100);
            
            entity.HasOne(e => e.VideoCall)
                .WithMany(e => e.Participants)
                .HasForeignKey(e => e.VideoCallId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Provider)
                .WithMany()
                .HasForeignKey(e => e.ProviderId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private void ConfigureVideoCallEvent(ModelBuilder builder)
    {
        builder.Entity<VideoCallEvent>(entity =>
        {
            entity.ToTable("VideoCallEvents");
            entity.Property(e => e.Type).HasConversion<string>();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Metadata).HasMaxLength(1000);
            
            entity.HasOne(e => e.VideoCall)
                .WithMany(e => e.Events)
                .HasForeignKey(e => e.VideoCallId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasOne(e => e.Provider)
                .WithMany()
                .HasForeignKey(e => e.ProviderId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private void ConfigureAppointment(ModelBuilder builder)
    {
        builder.Entity<Appointment>(entity =>
        {
            entity.ToTable("Appointments");
            entity.Property(e => e.Fee).HasPrecision(18, 2);
            entity.Property(e => e.StripePaymentIntentId).HasMaxLength(255);
            entity.Property(e => e.StripeSessionId).HasMaxLength(255);
            entity.Property(e => e.OpenTokSessionId).HasMaxLength(255);
            entity.Property(e => e.MeetingUrl).HasMaxLength(500);
            entity.Property(e => e.MeetingId).HasMaxLength(100);
            entity.Property(e => e.RecordingId).HasMaxLength(100);
            entity.Property(e => e.RecordingUrl).HasMaxLength(500);
            entity.Property(e => e.ReasonForVisit).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Symptoms).HasMaxLength(1000);
            entity.Property(e => e.PatientNotes).HasMaxLength(1000);
            entity.Property(e => e.Diagnosis).HasMaxLength(1000);
            entity.Property(e => e.Prescription).HasMaxLength(1000);
            entity.Property(e => e.ProviderNotes).HasMaxLength(1000);
            entity.Property(e => e.FollowUpInstructions).HasMaxLength(1000);
            entity.Property(e => e.IsPaymentCaptured).HasDefaultValue(false);
            entity.Property(e => e.IsRefunded).HasDefaultValue(false);
            entity.Property(e => e.RefundAmount).HasPrecision(18, 2);
            entity.Property(e => e.IsVideoCallStarted).HasDefaultValue(false);
            entity.Property(e => e.IsVideoCallEnded).HasDefaultValue(false);
            entity.Property(e => e.IsRecordingEnabled).HasDefaultValue(true);
            entity.Property(e => e.IsPatientNotified).HasDefaultValue(false);
            entity.Property(e => e.IsProviderNotified).HasDefaultValue(false);
            entity.Property(e => e.DurationMinutes).HasDefaultValue(30);
            entity.Property(e => e.AppointmentStatusId).IsRequired(); // Now Guid
            entity.HasOne(e => e.AppointmentStatus)
                .WithMany(e => e.Appointments)
                .HasForeignKey(e => e.AppointmentStatusId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Foreign key relationships
            entity.HasOne(e => e.Patient)
                .WithMany(e => e.PatientAppointments)
                .HasForeignKey(e => e.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Provider)
                .WithMany()
                .HasForeignKey(e => e.ProviderId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Category)
                .WithMany()
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Subscription)
                .WithMany()
                .HasForeignKey(e => e.SubscriptionId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasOne(e => e.Consultation)
                .WithMany()
                .HasForeignKey(e => e.ConsultationId)
                .OnDelete(DeleteBehavior.SetNull);
                
            // Master table relationships
            entity.HasOne(e => e.AppointmentType)
                .WithMany(e => e.Appointments)
                .HasForeignKey(e => e.AppointmentTypeId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.ConsultationMode)
                .WithMany(e => e.Appointments)
                .HasForeignKey(e => e.ConsultationModeId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.PaymentStatus)
                .WithMany(e => e.Appointments)
                .HasForeignKey(e => e.PaymentStatusId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureAppointmentParticipant(ModelBuilder builder)
    {
        builder.Entity<AppointmentParticipant>(entity =>
        {
            entity.ToTable("AppointmentParticipants");
            entity.Property(e => e.ExternalEmail).HasMaxLength(256);
            entity.Property(e => e.ExternalPhone).HasMaxLength(32);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            
            entity.HasOne(e => e.Appointment)
                .WithMany(e => e.Participants)
                .HasForeignKey(e => e.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.User)
                .WithMany(e => e.AppointmentParticipants)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasOne(e => e.InvitedByUser)
                .WithMany()
                .HasForeignKey(e => e.InvitedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            // Master table relationships
            entity.HasOne(e => e.ParticipantRole)
                .WithMany(r => r.Participants)
                .HasForeignKey(e => e.ParticipantRoleId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.ParticipantStatus)
                .WithMany(s => s.Participants)
                .HasForeignKey(e => e.ParticipantStatusId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureAppointmentInvitation(ModelBuilder builder)
    {
        builder.Entity<AppointmentInvitation>(entity =>
        {
            entity.ToTable("AppointmentInvitations");
            entity.Property(e => e.InvitedEmail).HasMaxLength(256);
            entity.Property(e => e.InvitedPhone).HasMaxLength(32);
            entity.Property(e => e.Message).HasMaxLength(500);
            
            entity.HasOne(e => e.Appointment)
                .WithMany()
                .HasForeignKey(e => e.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.InvitedByUser)
                .WithMany()
                .HasForeignKey(e => e.InvitedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.InvitedUser)
                .WithMany()
                .HasForeignKey(e => e.InvitedUserId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasOne(e => e.InvitationStatus)
                .WithMany(e => e.Invitations)
                .HasForeignKey(e => e.InvitationStatusId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureAppointmentPaymentLog(ModelBuilder builder)
    {
        builder.Entity<AppointmentPaymentLog>(entity =>
        {
            entity.ToTable("AppointmentPaymentLogs");
            entity.Property(e => e.PaymentMethod).HasMaxLength(100);
            entity.Property(e => e.PaymentIntentId).HasMaxLength(255);
            entity.Property(e => e.SessionId).HasMaxLength(255);
            entity.Property(e => e.RefundId).HasMaxLength(255);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.RefundedAmount).HasPrecision(18, 2);
            entity.Property(e => e.Currency).HasMaxLength(10);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.FailureReason).HasMaxLength(1000);
            entity.Property(e => e.RefundReason).HasMaxLength(1000);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            
            entity.HasOne(e => e.Appointment)
                .WithMany(e => e.PaymentLogs)
                .HasForeignKey(e => e.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.User)
                .WithMany(e => e.PaymentLogs)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            // Master table relationships
            entity.HasOne(e => e.PaymentStatus)
                .WithMany(e => e.PaymentLogs)
                .HasForeignKey(e => e.PaymentStatusId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.RefundStatus)
                .WithMany(e => e.PaymentLogs)
                .HasForeignKey(e => e.RefundStatusId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureAppointmentDocument(ModelBuilder builder)
    {
        builder.Entity<AppointmentDocument>(entity =>
        {
            entity.ToTable("AppointmentDocuments");
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FilePath).IsRequired().HasMaxLength(500);
            entity.Property(e => e.FileType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.FileSize).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.DocumentTypeId).IsRequired(); // Now Guid
            
            entity.HasOne(e => e.Appointment)
                .WithMany(e => e.Documents)
                .HasForeignKey(e => e.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.UploadedBy)
                .WithMany(e => e.UploadedDocuments)
                .HasForeignKey(e => e.UploadedById)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasOne(e => e.Provider)
                .WithMany()
                .HasForeignKey(e => e.ProviderId)
                .OnDelete(DeleteBehavior.SetNull);
                
            // Master table relationship
            entity.HasOne(e => e.DocumentType)
                .WithMany()
                .HasForeignKey(e => e.DocumentTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureAppointmentReminder(ModelBuilder builder)
    {
        builder.Entity<AppointmentReminder>(entity =>
        {
            entity.ToTable("AppointmentReminders");
            entity.Property(e => e.ScheduledAt).IsRequired();
            entity.Property(e => e.SentAt).IsRequired(false);
            entity.Property(e => e.IsSent).HasDefaultValue(false);
            entity.Property(e => e.IsDelivered).HasDefaultValue(false);
            entity.Property(e => e.Message).HasMaxLength(1000);
            entity.Property(e => e.RecipientEmail).HasMaxLength(100);
            entity.Property(e => e.RecipientPhone).HasMaxLength(20);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            
            entity.HasOne(e => e.Appointment)
                .WithMany(e => e.Reminders)
                .HasForeignKey(e => e.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Master table relationships
            entity.HasOne(e => e.ReminderType)
                .WithMany(e => e.Reminders)
                .HasForeignKey(e => e.ReminderTypeId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.ReminderTiming)
                .WithMany(e => e.Reminders)
                .HasForeignKey(e => e.ReminderTimingId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureAppointmentEvent(ModelBuilder builder)
    {
        builder.Entity<AppointmentEvent>(entity =>
        {
            entity.ToTable("AppointmentEvents");
            entity.Property(e => e.OccurredAt).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Metadata).HasMaxLength(500);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            
            entity.HasOne(e => e.Appointment)
                .WithMany(e => e.Events)
                .HasForeignKey(e => e.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.User)
                .WithMany(e => e.AppointmentEvents)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasOne(e => e.Provider)
                .WithMany()
                .HasForeignKey(e => e.ProviderId)
                .OnDelete(DeleteBehavior.SetNull);
                
            // Master table relationship
            entity.HasOne(e => e.EventType)
                .WithMany(e => e.Events)
                .HasForeignKey(e => e.EventTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureSubscriptionPayment(ModelBuilder builder)
    {
        builder.Entity<SubscriptionPayment>(entity =>
        {
            entity.ToTable("SubscriptionPayments");
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            // ... existing property and relationship configuration ...
        });
    }

    private void ConfigureSubscriptionStatusHistory(ModelBuilder builder)
    {
        builder.Entity<SubscriptionStatusHistory>(entity =>
        {
            entity.ToTable("SubscriptionStatusHistories");
            entity.Property(e => e.FromStatus).HasMaxLength(50);
            entity.Property(e => e.ToStatus).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Reason).HasMaxLength(500);
            entity.Property(e => e.ChangedByUserId).HasMaxLength(100);
            entity.Property(e => e.Metadata).HasMaxLength(1000);
            entity.HasOne(e => e.Subscription)
                .WithMany(s => s.StatusHistory)
                .HasForeignKey(e => e.SubscriptionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
    private void ConfigurePaymentRefund(ModelBuilder builder)
    {
        builder.Entity<PaymentRefund>(entity =>
        {
            entity.ToTable("PaymentRefunds");
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Reason).IsRequired().HasMaxLength(500);
            entity.Property(e => e.StripeRefundId).HasMaxLength(100);
            entity.HasOne(e => e.SubscriptionPayment)
                .WithMany(p => p.Refunds)
                .HasForeignKey(e => e.SubscriptionPaymentId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.ProcessedByUser)
                .WithMany()
                .HasForeignKey(e => e.ProcessedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private void ConfigureCategoryFeeRange(ModelBuilder builder)
    {
        builder.Entity<CategoryFeeRange>(entity =>
        {
            entity.ToTable("CategoryFeeRanges");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CategoryId).IsRequired();
            entity.Property(e => e.MinimumFee).IsRequired().HasPrecision(18, 2);
            entity.Property(e => e.MaximumFee).IsRequired().HasPrecision(18, 2);
            entity.Property(e => e.PlatformCommission).IsRequired().HasPrecision(18, 2);
            entity.HasOne(e => e.Category)
                .WithMany()
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureProviderFee(ModelBuilder builder)
    {
        builder.Entity<ProviderFee>(entity =>
        {
            entity.ToTable("ProviderFees");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProviderId).IsRequired();
            entity.Property(e => e.CategoryId).IsRequired();
            entity.Property(e => e.ProposedFee).IsRequired().HasPrecision(18, 2);
            entity.Property(e => e.ApprovedFee).IsRequired().HasPrecision(18, 2);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.HasOne(e => e.Provider)
                .WithMany()
                .HasForeignKey(e => e.ProviderId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Category)
                .WithMany()
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureQuestionnaireSystem(ModelBuilder builder)
    {
        builder.Entity<QuestionnaireTemplate>(entity =>
        {
            entity.ToTable("QuestionnaireTemplates");
            entity.HasKey(e => e.Id);
            
            // Properties
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CategoryId).IsRequired();
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.Version).IsRequired();
            entity.Property(e => e.CreatedDate).IsRequired();
            entity.Property(e => e.UpdatedBy).IsRequired(false);
            entity.Property(e => e.IsDeleted).IsRequired();
            
            // Relationships
            entity.HasOne<Category>()
                  .WithMany()
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasMany(q => q.Questions)
                  .WithOne(q => q.Template)
                  .HasForeignKey(q => q.TemplateId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasMany(q => q.UserResponses)
                  .WithOne(r => r.Template)
                  .HasForeignKey(r => r.TemplateId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            // Indexes
            entity.HasIndex(e => e.CategoryId);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.IsDeleted);
            entity.HasIndex(e => new { e.CategoryId, e.IsActive, e.IsDeleted });
        });
        
        builder.Entity<Question>(entity =>
        {
            entity.ToTable("Questions");
            entity.HasKey(e => e.Id);
            
            // Properties
            entity.Property(e => e.TemplateId).IsRequired();
            entity.Property(e => e.Text).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Type).IsRequired().HasConversion<int>();
            entity.Property(e => e.IsRequired).IsRequired();
            entity.Property(e => e.Order).IsRequired();
            entity.Property(e => e.HelpText).HasMaxLength(200);
            entity.Property(e => e.MediaUrl).HasMaxLength(500);
            entity.Property(e => e.MinValue).HasPrecision(18, 2);
            entity.Property(e => e.MaxValue).HasPrecision(18, 2);
            entity.Property(e => e.StepValue).HasPrecision(18, 2);
            entity.Property(e => e.CreatedDate).IsRequired();
            entity.Property(e => e.UpdatedBy).IsRequired(false);
            entity.Property(e => e.IsDeleted).IsRequired();
            
            // Relationships
            entity.HasOne(q => q.Template)
                  .WithMany(t => t.Questions)
                  .HasForeignKey(q => q.TemplateId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasMany(q => q.Options)
                  .WithOne(o => o.Question)
                  .HasForeignKey(o => o.QuestionId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasMany(q => q.UserAnswers)
                  .WithOne(a => a.Question)
                  .HasForeignKey(a => a.QuestionId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            // Indexes
            entity.HasIndex(e => e.TemplateId);
            entity.HasIndex(e => e.Order);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.IsDeleted);
            entity.HasIndex(e => new { e.TemplateId, e.Order });
            entity.HasIndex(e => new { e.TemplateId, e.IsDeleted });
            
            // Constraints
            entity.HasCheckConstraint("CK_Questions_Order_Positive", "[Order] > 0");
            entity.HasCheckConstraint("CK_Questions_Range_Values", 
                "([Type] != 6) OR ([MinValue] IS NULL AND [MaxValue] IS NULL) OR ([MinValue] IS NOT NULL AND [MaxValue] IS NOT NULL AND [MinValue] < [MaxValue])");
        });
        
        builder.Entity<QuestionOption>(entity =>
        {
            entity.ToTable("QuestionOptions");
            entity.HasKey(e => e.Id);
            
            // Properties
            entity.Property(e => e.QuestionId).IsRequired();
            entity.Property(e => e.Text).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Value).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Order).IsRequired();
            entity.Property(e => e.MediaUrl).HasMaxLength(500);
            entity.Property(e => e.IsCorrect).IsRequired();
            entity.Property(e => e.CreatedDate).IsRequired();
            entity.Property(e => e.UpdatedBy).IsRequired(false);
            entity.Property(e => e.IsDeleted).IsRequired();
            
            // Relationships
            entity.HasOne(o => o.Question)
                  .WithMany(q => q.Options)
                  .HasForeignKey(o => o.QuestionId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasMany(o => o.UserAnswerOptions)
                  .WithOne(uao => uao.Option)
                  .HasForeignKey(uao => uao.OptionId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            // Indexes
            entity.HasIndex(e => e.QuestionId);
            entity.HasIndex(e => e.Order);
            entity.HasIndex(e => e.IsCorrect);
            entity.HasIndex(e => e.IsDeleted);
            entity.HasIndex(e => new { e.QuestionId, e.Order });
            entity.HasIndex(e => new { e.QuestionId, e.IsDeleted });
            
            // Constraints
            entity.HasCheckConstraint("CK_QuestionOptions_Order_Positive", "[Order] > 0");
        });
        
        builder.Entity<UserResponse>(entity =>
        {
            entity.ToTable("UserResponses");
            entity.HasKey(e => e.Id);
            
            // Properties
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.CategoryId).IsRequired();
            entity.Property(e => e.TemplateId).IsRequired();
            entity.Property(e => e.Status).IsRequired().HasConversion<int>();
            entity.Property(e => e.CreatedDate).IsRequired();
            entity.Property(e => e.UpdatedBy).IsRequired(false);
            entity.Property(e => e.IsDeleted).IsRequired();
            
            // Relationships
            entity.HasOne<Category>()
                  .WithMany()
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasOne(r => r.Template)
                  .WithMany(t => t.UserResponses)
                  .HasForeignKey(r => r.TemplateId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasMany(r => r.Answers)
                  .WithOne(a => a.Response)
                  .HasForeignKey(a => a.ResponseId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            // Indexes
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.CategoryId);
            entity.HasIndex(e => e.TemplateId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedDate);
            entity.HasIndex(e => e.IsDeleted);
            entity.HasIndex(e => new { e.UserId, e.TemplateId });
            entity.HasIndex(e => new { e.UserId, e.Status, e.IsDeleted });
            
            // Constraints
            entity.HasCheckConstraint("CK_UserResponses_Status_Valid", 
                "[Status] IN (1, 2, 3, 4, 5, 6, 7)");
        });
        
        builder.Entity<UserAnswer>(entity =>
        {
            entity.ToTable("UserAnswers");
            entity.HasKey(e => e.Id);
            
            // Properties
            entity.Property(e => e.ResponseId).IsRequired();
            entity.Property(e => e.QuestionId).IsRequired();
            entity.Property(e => e.AnswerText).HasMaxLength(4000);
            entity.Property(e => e.NumericValue).HasPrecision(18, 2);
            entity.Property(e => e.DateTimeValue).IsRequired(false);
            entity.Property(e => e.CreatedDate).IsRequired();
            entity.Property(e => e.UpdatedBy).IsRequired(false);
            entity.Property(e => e.IsDeleted).IsRequired();
            
            // Relationships
            entity.HasOne(a => a.Response)
                  .WithMany(r => r.Answers)
                  .HasForeignKey(a => a.ResponseId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(a => a.Question)
                  .WithMany(q => q.UserAnswers)
                  .HasForeignKey(a => a.QuestionId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasMany(a => a.SelectedOptions)
                  .WithOne(uao => uao.Answer)
                  .HasForeignKey(uao => uao.AnswerId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            // Indexes
            entity.HasIndex(e => e.ResponseId);
            entity.HasIndex(e => e.QuestionId);
            entity.HasIndex(e => e.CreatedDate);
            entity.HasIndex(e => e.IsDeleted);
            entity.HasIndex(e => new { e.ResponseId, e.QuestionId });
            entity.HasIndex(e => new { e.ResponseId, e.IsDeleted });
            
            // Constraints - Removed complex check constraint that uses subquery
            // entity.HasCheckConstraint("CK_UserAnswers_Answer_Type_Valid", 
            //     "([AnswerText] IS NOT NULL) OR ([NumericValue] IS NOT NULL) OR ([DateTimeValue] IS NOT NULL) OR EXISTS (SELECT 1 FROM UserAnswerOptions WHERE AnswerId = Id)");
        });
        
        builder.Entity<UserAnswerOption>(entity =>
        {
            entity.ToTable("UserAnswerOptions");
            entity.HasKey(e => e.Id);
            
            // Properties
            entity.Property(e => e.AnswerId).IsRequired();
            entity.Property(e => e.OptionId).IsRequired();
            entity.Property(e => e.CreatedDate).IsRequired();
            entity.Property(e => e.UpdatedBy).IsRequired(false);
            entity.Property(e => e.IsDeleted).IsRequired();
            
            // Relationships
            entity.HasOne(uao => uao.Answer)
                  .WithMany(a => a.SelectedOptions)
                  .HasForeignKey(uao => uao.AnswerId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(uao => uao.Option)
                  .WithMany(o => o.UserAnswerOptions)
                  .HasForeignKey(uao => uao.OptionId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            // Indexes
            entity.HasIndex(e => e.AnswerId);
            entity.HasIndex(e => e.OptionId);
            entity.HasIndex(e => e.CreatedDate);
            entity.HasIndex(e => e.IsDeleted);
            entity.HasIndex(e => new { e.AnswerId, e.OptionId });
            
            // Unique constraint to prevent duplicate selections
            entity.HasIndex(e => new { e.AnswerId, e.OptionId, e.IsDeleted })
                  .IsUnique()
                  .HasFilter("[IsDeleted] = 0");
        });
    }
    
    private void ConfigureDocument(ModelBuilder builder)
    {
        builder.Entity<Document>(entity =>
        {
            entity.ToTable("Documents");
            entity.HasKey(e => e.Id);
            
            // Properties
            entity.Property(e => e.Id).IsRequired();
            entity.Property(e => e.OriginalName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.UniqueName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FilePath).IsRequired().HasMaxLength(500);
            entity.Property(e => e.FolderPath).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ContentType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.FileSize).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.DocumentTypeId).IsRequired();
            entity.Property(e => e.DocumentCategory).HasMaxLength(50);
            entity.Property(e => e.IsEncrypted).IsRequired();
            entity.Property(e => e.EncryptionKey).HasMaxLength(100);
            entity.Property(e => e.IsPublic).IsRequired();
            entity.Property(e => e.CreatedBy).IsRequired();
            entity.Property(e => e.DeletedBy).IsRequired(false);
            entity.Property(e => e.DeletedDate).IsRequired(false);
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.IsDeleted).IsRequired();
            entity.Property(e => e.CreatedDate).IsRequired();
            entity.Property(e => e.UpdatedDate).IsRequired(false);
            
            // Relationships - Use NO ACTION to avoid cascade conflicts
            entity.HasOne(e => e.DocumentType)
                .WithMany(dt => dt.Documents)
                .HasForeignKey(e => e.DocumentTypeId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasMany(e => e.References)
                .WithOne(r => r.Document)
                .HasForeignKey(r => r.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Indexes
            entity.HasIndex(e => e.Id).IsUnique();
            entity.HasIndex(e => e.DocumentTypeId);
            entity.HasIndex(e => e.CreatedBy);
            entity.HasIndex(e => e.DeletedBy);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.IsDeleted);
            entity.HasIndex(e => e.CreatedDate);
            entity.HasIndex(e => e.UpdatedDate);
            entity.HasIndex(e => new { e.CreatedBy, e.IsDeleted });
            entity.HasIndex(e => new { e.DocumentTypeId, e.IsDeleted });
        });
        
        builder.Entity<DocumentReference>(entity =>
        {
            entity.ToTable("DocumentReferences");
            entity.HasKey(e => e.Id);
            
            // Properties
            entity.Property(e => e.DocumentId).IsRequired();
            entity.Property(e => e.EntityType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.EntityId).IsRequired();
            entity.Property(e => e.ReferenceType).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsPublic).IsRequired();
            entity.Property(e => e.ExpiresAt).IsRequired(false);
            entity.Property(e => e.CreatedBy).IsRequired();
            entity.Property(e => e.CreatedDate).IsRequired();
            entity.Property(e => e.UpdatedDate).IsRequired(false);
            entity.Property(e => e.IsDeleted).IsRequired();
            
            // Relationships
            entity.HasOne(e => e.Document)
                .WithMany(d => d.References)
                .HasForeignKey(e => e.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Indexes
            entity.HasIndex(e => e.DocumentId);
            entity.HasIndex(e => e.EntityType);
            entity.HasIndex(e => e.EntityId);
            entity.HasIndex(e => e.CreatedBy);
            entity.HasIndex(e => e.IsDeleted);
            entity.HasIndex(e => e.CreatedDate);
            entity.HasIndex(e => new { e.EntityType, e.EntityId });
            entity.HasIndex(e => new { e.DocumentId, e.EntityType, e.EntityId });
            entity.HasIndex(e => new { e.EntityType, e.EntityId, e.IsDeleted });
        });
    }

    private void ConfigurePrivilegeUsageHistory(ModelBuilder builder)
    {
        builder.Entity<PrivilegeUsageHistory>(entity =>
        {
            entity.ToTable("PrivilegeUsageHistories");
            entity.HasKey(e => e.Id);
            
            // Properties
            entity.Property(e => e.Id).IsRequired();
            entity.Property(e => e.UserSubscriptionPrivilegeUsageId).IsRequired();
            entity.Property(e => e.UsedValue).IsRequired();
            entity.Property(e => e.UsedAt).IsRequired();
            entity.Property(e => e.UsageDate).IsRequired();
            entity.Property(e => e.UsageWeek).IsRequired().HasMaxLength(10);
            entity.Property(e => e.UsageMonth).IsRequired().HasMaxLength(7);
            entity.Property(e => e.Notes).HasMaxLength(500);
            
            // Relationships
            entity.HasOne(e => e.UserSubscriptionPrivilegeUsage)
                .WithMany(u => u.UsageHistory)
                .HasForeignKey(e => e.UserSubscriptionPrivilegeUsageId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Indexes for performance
            entity.HasIndex(e => e.UserSubscriptionPrivilegeUsageId);
            entity.HasIndex(e => e.UsageDate);
            entity.HasIndex(e => e.UsageWeek);
            entity.HasIndex(e => e.UsageMonth);
            entity.HasIndex(e => new { e.UserSubscriptionPrivilegeUsageId, e.UsageDate });
        });
    }

    #region Audit Functionality

    public override int SaveChanges()
    {
        var auditEntries = new List<AuditEntry>();

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                continue;

            var auditEntry = new AuditEntry(entry)
            {
                UserId = this.CurrentUserId,
                TableName = entry.Metadata.GetTableName(),
                AuditType = entry.State switch
                {
                    EntityState.Added => AuditType.Create,
                    EntityState.Modified => AuditType.Update,
                    EntityState.Deleted => AuditType.Delete,
                    _ => AuditType.None
                }
            };

            foreach (var property in entry.Properties)
            {
                string propertyName = property.Metadata.Name;
                if (property.Metadata.IsPrimaryKey())
                {
                    auditEntry.KeyValues[propertyName] = property.CurrentValue;
                    continue;
                }

                switch (entry.State)
                {
                    case EntityState.Added:
                        auditEntry.NewValues[propertyName] = property.CurrentValue;
                        auditEntry.ChangedColumns.Add(propertyName);
                        break;
                    case EntityState.Deleted:
                        auditEntry.OldValues[propertyName] = property.OriginalValue;
                        auditEntry.ChangedColumns.Add(propertyName);
                        break;
                    case EntityState.Modified:
                        if (!Equals(property.OriginalValue, property.CurrentValue))
                        {
                            auditEntry.OldValues[propertyName] = property.OriginalValue;
                            auditEntry.NewValues[propertyName] = property.CurrentValue;
                            auditEntry.ChangedColumns.Add(propertyName);
                        }
                        break;
                }
            }

            auditEntries.Add(auditEntry);
        }

        // Save changes to the main entities
        int result = base.SaveChanges();

        // Save audit logs
        if (auditEntries.Count > 0)
        {
            AuditLogs.AddRange(auditEntries.Select(ae => ae.ToAudit()));
            base.SaveChanges();
        }

        return result;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var auditEntries = new List<AuditEntry>();

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                continue;

            var auditEntry = new AuditEntry(entry)
            {
                UserId = this.CurrentUserId,
                TableName = entry.Metadata.GetTableName(),
                AuditType = entry.State switch
                {
                    EntityState.Added => AuditType.Create,
                    EntityState.Modified => AuditType.Update,
                    EntityState.Deleted => AuditType.Delete,
                    _ => AuditType.None
                }
            };

            foreach (var property in entry.Properties)
            {
                string propertyName = property.Metadata.Name;
                if (property.Metadata.IsPrimaryKey())
                {
                    auditEntry.KeyValues[propertyName] = property.CurrentValue;
                    continue;
                }

                switch (entry.State)
                {
                    case EntityState.Added:
                        auditEntry.NewValues[propertyName] = property.CurrentValue;
                        auditEntry.ChangedColumns.Add(propertyName);
                        break;
                    case EntityState.Deleted:
                        auditEntry.OldValues[propertyName] = property.OriginalValue;
                        auditEntry.ChangedColumns.Add(propertyName);
                        break;
                    case EntityState.Modified:
                        if (!Equals(property.OriginalValue, property.CurrentValue))
                        {
                            auditEntry.OldValues[propertyName] = property.OriginalValue;
                            auditEntry.NewValues[propertyName] = property.CurrentValue;
                            auditEntry.ChangedColumns.Add(propertyName);
                        }
                        break;
                }
            }

            auditEntries.Add(auditEntry);
        }

        // Save changes to the main entities
        int result = await base.SaveChangesAsync(cancellationToken);

        // Save audit logs
        if (auditEntries.Count > 0)
        {
            AuditLogs.AddRange(auditEntries.Select(ae => ae.ToAudit()));
            await base.SaveChangesAsync(cancellationToken);
        }

        return result;
    }

    #endregion
} 