using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartTelehealth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TableName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OldValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AffectedColumns = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PrimaryKey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OrganizationId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProcessedWebhookEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StripeEventId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsSuccess = table.Column<bool>(type: "bit", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    MaxRetries = table.Column<int>(type: "int", nullable: false, defaultValue: 3),
                    LastAttemptAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ProcessingDurationMs = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessedWebhookEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppointmentDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UploadedById = table.Column<int>(type: "int", nullable: true),
                    ProviderId = table.Column<int>(type: "int", nullable: true),
                    DocumentTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FileType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppointmentDocuments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppointmentEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    ProviderId = table.Column<int>(type: "int", nullable: true),
                    EventTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppointmentEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppointmentInvitations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvitedByUserId = table.Column<int>(type: "int", nullable: false),
                    InvitedUserId = table.Column<int>(type: "int", nullable: true),
                    InvitedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    InvitedPhone = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    InvitationStatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RespondedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppointmentInvitations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppointmentParticipants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    ExternalEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ExternalPhone = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    ParticipantRoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParticipantStatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvitedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LeftAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastSeenAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InvitedByUserId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppointmentParticipants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppointmentPaymentLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    PaymentStatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RefundStatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PaymentIntentId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    SessionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    RefundId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RefundedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FailureReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RefundReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RefundDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppointmentPaymentLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppointmentReminders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReminderTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReminderTimingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsSent = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsDelivered = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RecipientEmail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RecipientPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppointmentReminders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    ProviderId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ConsultationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AppointmentStatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppointmentTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConsultationModeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentStatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AcceptedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false, defaultValue: 30),
                    ReasonForVisit = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Symptoms = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PatientNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Diagnosis = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Prescription = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ProviderNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FollowUpInstructions = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FollowUpDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Fee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    StripePaymentIntentId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    StripeSessionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsPaymentCaptured = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsRefunded = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RefundAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    OpenTokSessionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    MeetingUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MeetingId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsVideoCallStarted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsVideoCallEnded = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RecordingId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RecordingUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsRecordingEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsPatientNotified = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsProviderNotified = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    LastReminderSent = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AutoCancellationAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppointmentStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppointmentStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppointmentTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppointmentTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                });

            migrationBuilder.CreateTable(
                name: "BillingAdjustments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BillingRecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsPercentage = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Percentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    AppliedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AppliedBy = table.Column<int>(type: "int", nullable: true),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ApprovalNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillingAdjustments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BillingRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ConsultationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MedicationDeliveryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BillingCycleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CurrencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ShippingAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    BillingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StripePaymentIntentId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StripeInvoiceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FailureReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PaymentMethod = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TransactionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsRecurring = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    NextBillingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PaymentIntentId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AccruedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    AccrualStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AccrualEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MasterCurrencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillingRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Features = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ConsultationDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BasePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ConsultationFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ConsultationDurationMinutes = table.Column<int>(type: "int", nullable: false),
                    RequiresHealthAssessment = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    AllowsMedicationDelivery = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    AllowsFollowUpMessaging = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    AllowsOneTimeConsultation = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    OneTimeConsultationFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    OneTimeConsultationDurationMinutes = table.Column<int>(type: "int", nullable: false, defaultValue: 30),
                    IsMostPopular = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsTrending = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CategoryFeeRanges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MinimumFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MaximumFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PlatformCommission = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryFeeRanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoryFeeRanges_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChatAttachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FileType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UploadedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatAttachments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SenderId = table.Column<int>(type: "int", nullable: false),
                    SenderType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MessageType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsSystemMessage = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChatRoomParticipants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChatRoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CanSendMessages = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CanSendFiles = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CanInviteOthers = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CanModerate = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProviderId = table.Column<int>(type: "int", nullable: true),
                    LeftAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatRoomParticipants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChatRooms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PatientId = table.Column<int>(type: "int", nullable: true),
                    ProviderId = table.Column<int>(type: "int", nullable: true),
                    SubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ConsultationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsEncrypted = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    EncryptionKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastActivityAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ArchivedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AllowFileSharing = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    AllowVoiceCalls = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    AllowVideoCalls = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatRooms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChatSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ProviderId = table.Column<int>(type: "int", nullable: true),
                    SubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    MessageCount = table.Column<int>(type: "int", nullable: false),
                    HasFileSharing = table.Column<bool>(type: "bit", nullable: false),
                    HasVideoChat = table.Column<bool>(type: "bit", nullable: false),
                    SessionNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SessionType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsPriority = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConsultationModes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsultationModes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Consultations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ProviderId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    HealthAssessmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    Fee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MeetingUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MeetingId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Diagnosis = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TreatmentPlan = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Prescriptions = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RequiresFollowUp = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    FollowUpDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsOneTime = table.Column<bool>(type: "bit", nullable: false),
                    CancellationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Consultations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Consultations_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryTracking",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MedicationDeliveryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EventTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrackingNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Carrier = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryTracking", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DocumentReferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferenceType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentReferences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OriginalName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    UniqueName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FolderPath = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DocumentTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentCategory = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsEncrypted = table.Column<bool>(type: "bit", nullable: false),
                    EncryptionKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DocumentTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsSystemDefined = table.Column<bool>(type: "bit", nullable: false),
                    AllowedExtensions = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MaxFileSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    RequireFileValidation = table.Column<bool>(type: "bit", nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    UsageCount = table.Column<int>(type: "int", nullable: false),
                    LastUsedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EventTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HealthAssessments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProviderId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Symptoms = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MedicalHistory = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CurrentMedications = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Allergies = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    LifestyleFactors = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FamilyHistory = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProviderNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsEligibleForTreatment = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RequiresFollowUp = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HealthAssessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HealthAssessments_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InvitationStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvitationStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MasterBillingCycles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DurationInDays = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MasterBillingCycles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MasterCurrencies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MasterCurrencies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MasterPrivilegeTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MasterPrivilegeTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MedicationDeliveries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ConsultationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProviderId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeliveryAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ZipCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TrackingNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Carrier = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ShippedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EstimatedDeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Medications = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Instructions = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ShippingCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RequiresSignature = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsRefrigerated = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    FailureReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicationDeliveries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicationDeliveries_Consultations_ConsultationId",
                        column: x => x.ConsultationId,
                        principalTable: "Consultations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "MessageAttachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FileType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsImage = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsDocument = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsVideo = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsAudio = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageAttachments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MessageReactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ProviderId = table.Column<int>(type: "int", nullable: true),
                    Emoji = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageReactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MessageReadReceipts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ProviderId = table.Column<int>(type: "int", nullable: true),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeviceInfo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageReadReceipts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SenderId = table.Column<int>(type: "int", nullable: false),
                    ChatRoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReplyToMessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FileType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: true),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsEncrypted = table.Column<bool>(type: "bit", nullable: false),
                    EncryptionKey = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ConsultationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProviderId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messages_ChatRooms_ChatRoomId",
                        column: x => x.ChatRoomId,
                        principalTable: "ChatRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Messages_Consultations_ConsultationId",
                        column: x => x.ConsultationId,
                        principalTable: "Consultations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Messages_Messages_ReplyToMessageId",
                        column: x => x.ReplyToMessageId,
                        principalTable: "Messages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ScheduledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ParticipantRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParticipantRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ParticipantStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParticipantStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentRefunds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubscriptionPaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    StripeRefundId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RefundedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedByUserId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentRefunds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Color = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PrescriptionItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PrescriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MedicationName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Dosage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Instructions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Refills = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DispensedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ShippedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TrackingNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrescriptionItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Prescriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConsultationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProviderId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrescribedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SentToPharmacyAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConfirmedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DispensedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ShippedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PharmacyReference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrackingNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prescriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prescriptions_Consultations_ConsultationId",
                        column: x => x.ConsultationId,
                        principalTable: "Consultations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Privileges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PrivilegeTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MasterPrivilegeTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Privileges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Privileges_MasterPrivilegeTypes_MasterPrivilegeTypeId",
                        column: x => x.MasterPrivilegeTypeId,
                        principalTable: "MasterPrivilegeTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Privileges_MasterPrivilegeTypes_PrivilegeTypeId",
                        column: x => x.PrivilegeTypeId,
                        principalTable: "MasterPrivilegeTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PrivilegeUsageHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserSubscriptionPrivilegeUsageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsedValue = table.Column<int>(type: "int", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsageDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsageWeek = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    UsageMonth = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrivilegeUsageHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProviderCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProviderId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    YearsOfExperience = table.Column<int>(type: "int", nullable: false),
                    ConsultationFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    AvailableFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AvailableTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Specialization = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProviderCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProviderFees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProviderId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProposedFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ApprovedFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "int", maxLength: 50, nullable: false),
                    AdminRemarks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ProviderNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ProposedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewedByUserId = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderFees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProviderFees_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProviderOnboardings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Specialty = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SubSpecialty = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LicenseNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LicenseState = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NPINumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DEANumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Education = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    WorkHistory = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MalpracticeInsurance = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Bio = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ProfilePhotoUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    GovernmentIdUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    LicenseDocumentUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CertificationDocumentUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    MalpracticeInsuranceUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AdminRemarks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewedByUserId = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderOnboardings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Providers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    LicenseNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    State = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Specialty = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Bio = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Education = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Certifications = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    AvailableFrom = table.Column<TimeSpan>(type: "time", nullable: true),
                    AvailableTo = table.Column<TimeSpan>(type: "time", nullable: true),
                    ConsultationFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Providers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuestionnaireTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CategoryId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionnaireTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionnaireTemplates_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuestionnaireTemplates_Categories_CategoryId1",
                        column: x => x.CategoryId1,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuestionOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    MediaUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionOptions", x => x.Id);
                    table.CheckConstraint("CK_QuestionOptions_Order_Positive", "[Order] > 0");
                });

            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    HelpText = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MediaUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MinValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    MaxValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    StepValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.Id);
                    table.CheckConstraint("CK_Questions_Order_Positive", "[Order] > 0");
                    table.CheckConstraint("CK_Questions_Range_Values", "([Type] != 6) OR ([MinValue] IS NULL AND [MaxValue] IS NULL) OR ([MinValue] IS NOT NULL AND [MaxValue] IS NOT NULL AND [MinValue] < [MaxValue])");
                    table.ForeignKey(
                        name: "FK_Questions_QuestionnaireTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "QuestionnaireTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefundStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Color = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefundStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReminderTimings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    MinutesBeforeAppointment = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReminderTimings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReminderTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReminderTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScheduledPlanMigrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NotificationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ScheduledMigrationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Pending"),
                    UserDecision = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UserDecisionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DowngradeToPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledPlanMigrations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServiceConstraints",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MaxSessionsPerMonth = table.Column<int>(type: "int", nullable: false),
                    MaxDurationPerSession = table.Column<int>(type: "int", nullable: false),
                    MaxConcurrentSessions = table.Column<int>(type: "int", nullable: false),
                    TotalMinutesPerMonth = table.Column<int>(type: "int", nullable: true),
                    AllowFileSharing = table.Column<bool>(type: "bit", nullable: false),
                    AllowVideoChat = table.Column<bool>(type: "bit", nullable: false),
                    PriorityQueue = table.Column<bool>(type: "bit", nullable: false),
                    MaxMessageLength = table.Column<int>(type: "int", nullable: false),
                    SubscriptionPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceConstraints", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionPayments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BillingRecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NetAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FailureReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BillingPeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BillingPeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StripePaymentIntentId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StripeInvoiceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ReceiptUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PaymentIntentId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    InvoiceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AttemptCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    NextRetryAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RefundedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubscriptionPayments_BillingRecords_BillingRecordId",
                        column: x => x.BillingRecordId,
                        principalTable: "BillingRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubscriptionPayments_MasterCurrencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "MasterCurrencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionPlanPrivileges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubscriptionPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PrivilegeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Value = table.Column<int>(type: "int", nullable: false),
                    UsagePeriodId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DurationMonths = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DailyLimit = table.Column<int>(type: "int", nullable: true),
                    WeeklyLimit = table.Column<int>(type: "int", nullable: true),
                    MonthlyLimit = table.Column<int>(type: "int", nullable: true),
                    PrivilegeBaseCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    UnitCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPlanPrivileges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubscriptionPlanPrivileges_MasterBillingCycles_UsagePeriodId",
                        column: x => x.UsagePeriodId,
                        principalTable: "MasterBillingCycles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubscriptionPlanPrivileges_Privileges_PrivilegeId",
                        column: x => x.PrivilegeId,
                        principalTable: "Privileges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ShortDescription = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsTrialAllowed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    TrialDurationInDays = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsMostPopular = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsTrending = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    PlanType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountedPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    DiscountValidUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MonthlyBillingDiscount = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    QuarterlyBillingDiscount = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    AnnualBillingDiscount = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    VersionNumber = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    IsLatestVersion = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ParentPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsAutoCalculatedPrice = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    PrivilegesTotalCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    AdminCommissionPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    AdminCommissionFixed = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    PriceChangeNoticeDays = table.Column<int>(type: "int", nullable: false, defaultValue: 10),
                    VersionCreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValue: new DateTime(2025, 10, 18, 15, 47, 4, 66, DateTimeKind.Utc).AddTicks(9706)),
                    BillingCycleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StripeProductId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StripeMonthlyPriceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StripeQuarterlyPriceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StripeAnnualPriceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MessagingCount = table.Column<int>(type: "int", nullable: false, defaultValue: 10),
                    IncludesMedicationDelivery = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IncludesFollowUpCare = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    DeliveryFrequencyDays = table.Column<int>(type: "int", nullable: false, defaultValue: 30),
                    MaxPauseDurationDays = table.Column<int>(type: "int", nullable: false, defaultValue: 90),
                    Features = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Terms = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MasterBillingCycleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MasterCurrencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubscriptionPlans_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubscriptionPlans_MasterBillingCycles_BillingCycleId",
                        column: x => x.BillingCycleId,
                        principalTable: "MasterBillingCycles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubscriptionPlans_MasterBillingCycles_MasterBillingCycleId",
                        column: x => x.MasterBillingCycleId,
                        principalTable: "MasterBillingCycles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SubscriptionPlans_MasterCurrencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "MasterCurrencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubscriptionPlans_MasterCurrencies_MasterCurrencyId",
                        column: x => x.MasterCurrencyId,
                        principalTable: "MasterCurrencies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SubscriptionPlans_SubscriptionPlans_ParentPlanId",
                        column: x => x.ParentPlanId,
                        principalTable: "SubscriptionPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    SubscriptionPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BillingCycleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProviderId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StatusReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextBillingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CurrentPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AutoRenew = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SubscriptionStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PausedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResumedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SuspendedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastBillingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PauseReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RenewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StripeSubscriptionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StripeCustomerId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StripePriceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PaymentMethodId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastPaymentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastPaymentFailedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastPaymentError = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FailedPaymentAttempts = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsTrialSubscription = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    TrialStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TrialEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TrialDurationInDays = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    LastUsedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalUsageCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    HealthAssessmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MasterBillingCycleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MasterCurrencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subscriptions_HealthAssessments_HealthAssessmentId",
                        column: x => x.HealthAssessmentId,
                        principalTable: "HealthAssessments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Subscriptions_MasterBillingCycles_BillingCycleId",
                        column: x => x.BillingCycleId,
                        principalTable: "MasterBillingCycles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Subscriptions_MasterBillingCycles_MasterBillingCycleId",
                        column: x => x.MasterBillingCycleId,
                        principalTable: "MasterBillingCycles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Subscriptions_MasterCurrencies_MasterCurrencyId",
                        column: x => x.MasterCurrencyId,
                        principalTable: "MasterCurrencies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Subscriptions_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Subscriptions_SubscriptionPlans_SubscriptionPlanId",
                        column: x => x.SubscriptionPlanId,
                        principalTable: "SubscriptionPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionStatusHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ToStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ChangedByUserId = table.Column<int>(type: "int", nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Metadata = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionStatusHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubscriptionStatusHistories_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DefaultAdminCommissionPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 20m),
                    DefaultPriceChangeNoticeDays = table.Column<int>(type: "int", nullable: false, defaultValue: 10),
                    MaxFailedPaymentAttempts = table.Column<int>(type: "int", nullable: false, defaultValue: 3),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValue: new DateTime(2025, 10, 18, 15, 47, 4, 120, DateTimeKind.Utc).AddTicks(1694)),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserAnswerOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnswerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAnswerOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAnswerOptions_QuestionOptions_OptionId",
                        column: x => x.OptionId,
                        principalTable: "QuestionOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserAnswers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResponseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnswerText = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    NumericValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    DateTimeValue = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAnswers_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserResponses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CategoryId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserResponses", x => x.Id);
                    table.CheckConstraint("CK_UserResponses_Status_Valid", "[Status] IN (1, 2, 3, 4, 5, 6, 7)");
                    table.ForeignKey(
                        name: "FK_UserResponses_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserResponses_Categories_CategoryId1",
                        column: x => x.CategoryId1,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserResponses_QuestionnaireTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "QuestionnaireTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ZipCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EmergencyContactName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EmergencyContactPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    EmergencyContact = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmergencyPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefreshToken = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RefreshTokenExpiry = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StripeCustomerId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LicenseNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UserType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProfilePicture = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsEmailVerified = table.Column<bool>(type: "bit", nullable: false),
                    IsPhoneVerified = table.Column<bool>(type: "bit", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PasswordResetToken = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ResetTokenExpires = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NotificationPreferences = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LanguagePreference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TimeZonePreference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserRoleId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_UserRoles_UserRoleId",
                        column: x => x.UserRoleId,
                        principalTable: "UserRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserSubscriptionPrivilegeUsages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubscriptionPlanPrivilegeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PrivilegeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsedValue = table.Column<int>(type: "int", nullable: false),
                    AllowedValue = table.Column<int>(type: "int", nullable: false),
                    UsagePeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsagePeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUsedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResetAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PrivilegeId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSubscriptionPrivilegeUsages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSubscriptionPrivilegeUsages_Privileges_PrivilegeId",
                        column: x => x.PrivilegeId,
                        principalTable: "Privileges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserSubscriptionPrivilegeUsages_Privileges_PrivilegeId1",
                        column: x => x.PrivilegeId1,
                        principalTable: "Privileges",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserSubscriptionPrivilegeUsages_SubscriptionPlanPrivileges_SubscriptionPlanPrivilegeId",
                        column: x => x.SubscriptionPlanPrivilegeId,
                        principalTable: "SubscriptionPlanPrivileges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserSubscriptionPrivilegeUsages_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserSubscriptionPrivilegeUsages_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserSubscriptionPrivilegeUsages_Users_DeletedBy",
                        column: x => x.DeletedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserSubscriptionPrivilegeUsages_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VideoCalls",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Token = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RecordingUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoCalls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VideoCalls_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VideoCalls_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VideoCalls_Users_DeletedBy",
                        column: x => x.DeletedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VideoCalls_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VideoCallEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VideoCallId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    ProviderId = table.Column<int>(type: "int", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoCallEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VideoCallEvents_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_VideoCallEvents_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VideoCallEvents_Users_DeletedBy",
                        column: x => x.DeletedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VideoCallEvents_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VideoCallEvents_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_VideoCallEvents_VideoCalls_VideoCallId",
                        column: x => x.VideoCallId,
                        principalTable: "VideoCalls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VideoCallParticipants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VideoCallId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ProviderId = table.Column<int>(type: "int", nullable: true),
                    IsInitiator = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LeftAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DurationSeconds = table.Column<int>(type: "int", nullable: false),
                    IsVideoEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsAudioEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsScreenSharingEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AudioQuality = table.Column<int>(type: "int", nullable: true),
                    VideoQuality = table.Column<int>(type: "int", nullable: true),
                    NetworkQuality = table.Column<int>(type: "int", nullable: true),
                    DeviceInfo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoCallParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VideoCallParticipants_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_VideoCallParticipants_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VideoCallParticipants_Users_DeletedBy",
                        column: x => x.DeletedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VideoCallParticipants_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VideoCallParticipants_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VideoCallParticipants_VideoCalls_VideoCallId",
                        column: x => x.VideoCallId,
                        principalTable: "VideoCalls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "SystemSettings",
                columns: new[] { "Id", "CreatedBy", "CreatedDate", "DefaultAdminCommissionPercent", "DefaultPriceChangeNoticeDays", "DeletedBy", "DeletedDate", "IsActive", "LastUpdated", "MaxFailedPaymentAttempts", "UpdatedBy", "UpdatedDate" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000001"), null, new DateTime(2025, 10, 18, 15, 47, 4, 120, DateTimeKind.Utc).AddTicks(2315), 20m, 10, null, null, true, new DateTime(2025, 10, 18, 15, 47, 4, 120, DateTimeKind.Utc).AddTicks(2311), 3, null, null });

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentDocuments_AppointmentId",
                table: "AppointmentDocuments",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentDocuments_CreatedBy",
                table: "AppointmentDocuments",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentDocuments_DeletedBy",
                table: "AppointmentDocuments",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentDocuments_DocumentTypeId",
                table: "AppointmentDocuments",
                column: "DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentDocuments_ProviderId",
                table: "AppointmentDocuments",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentDocuments_UpdatedBy",
                table: "AppointmentDocuments",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentDocuments_UploadedById",
                table: "AppointmentDocuments",
                column: "UploadedById");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentEvents_AppointmentId",
                table: "AppointmentEvents",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentEvents_CreatedBy",
                table: "AppointmentEvents",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentEvents_DeletedBy",
                table: "AppointmentEvents",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentEvents_EventTypeId",
                table: "AppointmentEvents",
                column: "EventTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentEvents_ProviderId",
                table: "AppointmentEvents",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentEvents_UpdatedBy",
                table: "AppointmentEvents",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentEvents_UserId",
                table: "AppointmentEvents",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentInvitations_AppointmentId",
                table: "AppointmentInvitations",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentInvitations_CreatedBy",
                table: "AppointmentInvitations",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentInvitations_DeletedBy",
                table: "AppointmentInvitations",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentInvitations_InvitationStatusId",
                table: "AppointmentInvitations",
                column: "InvitationStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentInvitations_InvitedByUserId",
                table: "AppointmentInvitations",
                column: "InvitedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentInvitations_InvitedUserId",
                table: "AppointmentInvitations",
                column: "InvitedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentInvitations_UpdatedBy",
                table: "AppointmentInvitations",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentParticipants_AppointmentId",
                table: "AppointmentParticipants",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentParticipants_CreatedBy",
                table: "AppointmentParticipants",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentParticipants_DeletedBy",
                table: "AppointmentParticipants",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentParticipants_InvitedByUserId",
                table: "AppointmentParticipants",
                column: "InvitedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentParticipants_ParticipantRoleId",
                table: "AppointmentParticipants",
                column: "ParticipantRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentParticipants_ParticipantStatusId",
                table: "AppointmentParticipants",
                column: "ParticipantStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentParticipants_UpdatedBy",
                table: "AppointmentParticipants",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentParticipants_UserId",
                table: "AppointmentParticipants",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentPaymentLogs_AppointmentId",
                table: "AppointmentPaymentLogs",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentPaymentLogs_CreatedBy",
                table: "AppointmentPaymentLogs",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentPaymentLogs_DeletedBy",
                table: "AppointmentPaymentLogs",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentPaymentLogs_PaymentStatusId",
                table: "AppointmentPaymentLogs",
                column: "PaymentStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentPaymentLogs_RefundStatusId",
                table: "AppointmentPaymentLogs",
                column: "RefundStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentPaymentLogs_UpdatedBy",
                table: "AppointmentPaymentLogs",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentPaymentLogs_UserId",
                table: "AppointmentPaymentLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentReminders_AppointmentId",
                table: "AppointmentReminders",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentReminders_CreatedBy",
                table: "AppointmentReminders",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentReminders_DeletedBy",
                table: "AppointmentReminders",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentReminders_ReminderTimingId",
                table: "AppointmentReminders",
                column: "ReminderTimingId");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentReminders_ReminderTypeId",
                table: "AppointmentReminders",
                column: "ReminderTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentReminders_UpdatedBy",
                table: "AppointmentReminders",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_AppointmentStatusId",
                table: "Appointments",
                column: "AppointmentStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_AppointmentTypeId",
                table: "Appointments",
                column: "AppointmentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_CategoryId",
                table: "Appointments",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ConsultationId",
                table: "Appointments",
                column: "ConsultationId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ConsultationModeId",
                table: "Appointments",
                column: "ConsultationModeId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_CreatedBy",
                table: "Appointments",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_DeletedBy",
                table: "Appointments",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_PatientId",
                table: "Appointments",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_PaymentStatusId",
                table: "Appointments",
                column: "PaymentStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ProviderId",
                table: "Appointments",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_SubscriptionId",
                table: "Appointments",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_UpdatedBy",
                table: "Appointments",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentStatuses_CreatedBy",
                table: "AppointmentStatuses",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentStatuses_DeletedBy",
                table: "AppointmentStatuses",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentStatuses_UpdatedBy",
                table: "AppointmentStatuses",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentTypes_CreatedBy",
                table: "AppointmentTypes",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentTypes_DeletedBy",
                table: "AppointmentTypes",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentTypes_UpdatedBy",
                table: "AppointmentTypes",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_BillingAdjustments_AppliedAt",
                table: "BillingAdjustments",
                column: "AppliedAt");

            migrationBuilder.CreateIndex(
                name: "IX_BillingAdjustments_AppliedBy",
                table: "BillingAdjustments",
                column: "AppliedBy");

            migrationBuilder.CreateIndex(
                name: "IX_BillingAdjustments_BillingRecordId",
                table: "BillingAdjustments",
                column: "BillingRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_BillingAdjustments_CreatedBy",
                table: "BillingAdjustments",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_BillingAdjustments_DeletedBy",
                table: "BillingAdjustments",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_BillingAdjustments_IsApproved",
                table: "BillingAdjustments",
                column: "IsApproved");

            migrationBuilder.CreateIndex(
                name: "IX_BillingAdjustments_IsPercentage",
                table: "BillingAdjustments",
                column: "IsPercentage");

            migrationBuilder.CreateIndex(
                name: "IX_BillingAdjustments_Type",
                table: "BillingAdjustments",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_BillingAdjustments_UpdatedBy",
                table: "BillingAdjustments",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_BillingRecords_BillingCycleId",
                table: "BillingRecords",
                column: "BillingCycleId");

            migrationBuilder.CreateIndex(
                name: "IX_BillingRecords_BillingDate",
                table: "BillingRecords",
                column: "BillingDate");

            migrationBuilder.CreateIndex(
                name: "IX_BillingRecords_ConsultationId",
                table: "BillingRecords",
                column: "ConsultationId");

            migrationBuilder.CreateIndex(
                name: "IX_BillingRecords_CreatedBy",
                table: "BillingRecords",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_BillingRecords_CurrencyId",
                table: "BillingRecords",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_BillingRecords_DeletedBy",
                table: "BillingRecords",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_BillingRecords_DueDate",
                table: "BillingRecords",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_BillingRecords_InvoiceNumber",
                table: "BillingRecords",
                column: "InvoiceNumber");

            migrationBuilder.CreateIndex(
                name: "IX_BillingRecords_IsRecurring",
                table: "BillingRecords",
                column: "IsRecurring");

            migrationBuilder.CreateIndex(
                name: "IX_BillingRecords_MasterCurrencyId",
                table: "BillingRecords",
                column: "MasterCurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_BillingRecords_MedicationDeliveryId",
                table: "BillingRecords",
                column: "MedicationDeliveryId");

            migrationBuilder.CreateIndex(
                name: "IX_BillingRecords_PaidAt",
                table: "BillingRecords",
                column: "PaidAt");

            migrationBuilder.CreateIndex(
                name: "IX_BillingRecords_PaymentIntentId",
                table: "BillingRecords",
                column: "PaymentIntentId");

            migrationBuilder.CreateIndex(
                name: "IX_BillingRecords_Status",
                table: "BillingRecords",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_BillingRecords_StripeInvoiceId",
                table: "BillingRecords",
                column: "StripeInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_BillingRecords_StripePaymentIntentId",
                table: "BillingRecords",
                column: "StripePaymentIntentId");

            migrationBuilder.CreateIndex(
                name: "IX_BillingRecords_SubscriptionId",
                table: "BillingRecords",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_BillingRecords_Type",
                table: "BillingRecords",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_BillingRecords_UpdatedBy",
                table: "BillingRecords",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_BillingRecords_UserId",
                table: "BillingRecords",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_CreatedBy",
                table: "Categories",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_DeletedBy",
                table: "Categories",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_UpdatedBy",
                table: "Categories",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryFeeRanges_CategoryId",
                table: "CategoryFeeRanges",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryFeeRanges_CreatedBy",
                table: "CategoryFeeRanges",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryFeeRanges_DeletedBy",
                table: "CategoryFeeRanges",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryFeeRanges_UpdatedBy",
                table: "CategoryFeeRanges",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ChatAttachments_CreatedBy",
                table: "ChatAttachments",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ChatAttachments_DeletedBy",
                table: "ChatAttachments",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ChatAttachments_SessionId",
                table: "ChatAttachments",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatAttachments_UpdatedBy",
                table: "ChatAttachments",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_CreatedBy",
                table: "ChatMessages",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_DeletedBy",
                table: "ChatMessages",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_SessionId",
                table: "ChatMessages",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_UpdatedBy",
                table: "ChatMessages",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRoomParticipants_ChatRoomId",
                table: "ChatRoomParticipants",
                column: "ChatRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRoomParticipants_CreatedBy",
                table: "ChatRoomParticipants",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRoomParticipants_DeletedBy",
                table: "ChatRoomParticipants",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRoomParticipants_ProviderId",
                table: "ChatRoomParticipants",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRoomParticipants_UpdatedBy",
                table: "ChatRoomParticipants",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRoomParticipants_UserId",
                table: "ChatRoomParticipants",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRooms_ConsultationId",
                table: "ChatRooms",
                column: "ConsultationId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRooms_CreatedBy",
                table: "ChatRooms",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRooms_DeletedBy",
                table: "ChatRooms",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRooms_PatientId",
                table: "ChatRooms",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRooms_ProviderId",
                table: "ChatRooms",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRooms_SubscriptionId",
                table: "ChatRooms",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRooms_UpdatedBy",
                table: "ChatRooms",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ChatSessions_CreatedBy",
                table: "ChatSessions",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ChatSessions_DeletedBy",
                table: "ChatSessions",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ChatSessions_ProviderId",
                table: "ChatSessions",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatSessions_SubscriptionId",
                table: "ChatSessions",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatSessions_UpdatedBy",
                table: "ChatSessions",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ChatSessions_UserId",
                table: "ChatSessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationModes_CreatedBy",
                table: "ConsultationModes",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationModes_DeletedBy",
                table: "ConsultationModes",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationModes_UpdatedBy",
                table: "ConsultationModes",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Consultations_CategoryId",
                table: "Consultations",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Consultations_CreatedBy",
                table: "Consultations",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Consultations_DeletedBy",
                table: "Consultations",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Consultations_HealthAssessmentId",
                table: "Consultations",
                column: "HealthAssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Consultations_ProviderId",
                table: "Consultations",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_Consultations_SubscriptionId",
                table: "Consultations",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Consultations_UpdatedBy",
                table: "Consultations",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Consultations_UserId",
                table: "Consultations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryTracking_CreatedBy",
                table: "DeliveryTracking",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryTracking_DeletedBy",
                table: "DeliveryTracking",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryTracking_MedicationDeliveryId",
                table: "DeliveryTracking",
                column: "MedicationDeliveryId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryTracking_UpdatedBy",
                table: "DeliveryTracking",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentReferences_CreatedBy",
                table: "DocumentReferences",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentReferences_CreatedDate",
                table: "DocumentReferences",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentReferences_DeletedBy",
                table: "DocumentReferences",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentReferences_DocumentId",
                table: "DocumentReferences",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentReferences_DocumentId_EntityType_EntityId",
                table: "DocumentReferences",
                columns: new[] { "DocumentId", "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentReferences_EntityId",
                table: "DocumentReferences",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentReferences_EntityType",
                table: "DocumentReferences",
                column: "EntityType");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentReferences_EntityType_EntityId",
                table: "DocumentReferences",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentReferences_EntityType_EntityId_IsDeleted",
                table: "DocumentReferences",
                columns: new[] { "EntityType", "EntityId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentReferences_IsDeleted",
                table: "DocumentReferences",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentReferences_UpdatedBy",
                table: "DocumentReferences",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_CreatedBy",
                table: "Documents",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_CreatedBy_IsDeleted",
                table: "Documents",
                columns: new[] { "CreatedBy", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_CreatedDate",
                table: "Documents",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_DeletedBy",
                table: "Documents",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_DocumentTypeId",
                table: "Documents",
                column: "DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_DocumentTypeId_IsDeleted",
                table: "Documents",
                columns: new[] { "DocumentTypeId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_Id",
                table: "Documents",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Documents_IsActive",
                table: "Documents",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_IsDeleted",
                table: "Documents",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_UpdatedBy",
                table: "Documents",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_UpdatedDate",
                table: "Documents",
                column: "UpdatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTypes_CreatedBy",
                table: "DocumentTypes",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTypes_DeletedBy",
                table: "DocumentTypes",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTypes_UpdatedBy",
                table: "DocumentTypes",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_EventTypes_CreatedBy",
                table: "EventTypes",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_EventTypes_DeletedBy",
                table: "EventTypes",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_EventTypes_UpdatedBy",
                table: "EventTypes",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_HealthAssessments_CategoryId",
                table: "HealthAssessments",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_HealthAssessments_CreatedBy",
                table: "HealthAssessments",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_HealthAssessments_DeletedBy",
                table: "HealthAssessments",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_HealthAssessments_ProviderId",
                table: "HealthAssessments",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_HealthAssessments_UpdatedBy",
                table: "HealthAssessments",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_HealthAssessments_UserId",
                table: "HealthAssessments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_InvitationStatuses_CreatedBy",
                table: "InvitationStatuses",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_InvitationStatuses_DeletedBy",
                table: "InvitationStatuses",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_InvitationStatuses_UpdatedBy",
                table: "InvitationStatuses",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MasterBillingCycles_CreatedBy",
                table: "MasterBillingCycles",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MasterBillingCycles_DeletedBy",
                table: "MasterBillingCycles",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MasterBillingCycles_DurationInDays",
                table: "MasterBillingCycles",
                column: "DurationInDays");

            migrationBuilder.CreateIndex(
                name: "IX_MasterBillingCycles_Name",
                table: "MasterBillingCycles",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_MasterBillingCycles_SortOrder",
                table: "MasterBillingCycles",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_MasterBillingCycles_UpdatedBy",
                table: "MasterBillingCycles",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MasterCurrencies_Code",
                table: "MasterCurrencies",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MasterCurrencies_CreatedBy",
                table: "MasterCurrencies",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MasterCurrencies_DeletedBy",
                table: "MasterCurrencies",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MasterCurrencies_Name",
                table: "MasterCurrencies",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_MasterCurrencies_SortOrder",
                table: "MasterCurrencies",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_MasterCurrencies_Symbol",
                table: "MasterCurrencies",
                column: "Symbol");

            migrationBuilder.CreateIndex(
                name: "IX_MasterCurrencies_UpdatedBy",
                table: "MasterCurrencies",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MasterPrivilegeTypes_CreatedBy",
                table: "MasterPrivilegeTypes",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MasterPrivilegeTypes_DeletedBy",
                table: "MasterPrivilegeTypes",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MasterPrivilegeTypes_Description",
                table: "MasterPrivilegeTypes",
                column: "Description");

            migrationBuilder.CreateIndex(
                name: "IX_MasterPrivilegeTypes_Name",
                table: "MasterPrivilegeTypes",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_MasterPrivilegeTypes_SortOrder",
                table: "MasterPrivilegeTypes",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_MasterPrivilegeTypes_UpdatedBy",
                table: "MasterPrivilegeTypes",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MedicationDeliveries_ConsultationId",
                table: "MedicationDeliveries",
                column: "ConsultationId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicationDeliveries_CreatedBy",
                table: "MedicationDeliveries",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MedicationDeliveries_DeletedBy",
                table: "MedicationDeliveries",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MedicationDeliveries_ProviderId",
                table: "MedicationDeliveries",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicationDeliveries_SubscriptionId",
                table: "MedicationDeliveries",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicationDeliveries_UpdatedBy",
                table: "MedicationDeliveries",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MedicationDeliveries_UserId",
                table: "MedicationDeliveries",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageAttachments_CreatedBy",
                table: "MessageAttachments",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MessageAttachments_DeletedBy",
                table: "MessageAttachments",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MessageAttachments_MessageId",
                table: "MessageAttachments",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageAttachments_UpdatedBy",
                table: "MessageAttachments",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MessageReactions_CreatedBy",
                table: "MessageReactions",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MessageReactions_DeletedBy",
                table: "MessageReactions",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MessageReactions_MessageId",
                table: "MessageReactions",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageReactions_ProviderId",
                table: "MessageReactions",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageReactions_UpdatedBy",
                table: "MessageReactions",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MessageReactions_UserId",
                table: "MessageReactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageReadReceipts_CreatedBy",
                table: "MessageReadReceipts",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MessageReadReceipts_DeletedBy",
                table: "MessageReadReceipts",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MessageReadReceipts_MessageId",
                table: "MessageReadReceipts",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageReadReceipts_ProviderId",
                table: "MessageReadReceipts",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageReadReceipts_UpdatedBy",
                table: "MessageReadReceipts",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MessageReadReceipts_UserId",
                table: "MessageReadReceipts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ChatRoomId",
                table: "Messages",
                column: "ChatRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ConsultationId",
                table: "Messages",
                column: "ConsultationId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_CreatedBy",
                table: "Messages",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_DeletedBy",
                table: "Messages",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ProviderId",
                table: "Messages",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ReplyToMessageId",
                table: "Messages",
                column: "ReplyToMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SenderId",
                table: "Messages",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_UpdatedBy",
                table: "Messages",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_UserId",
                table: "Messages",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CreatedBy",
                table: "Notifications",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_DeletedBy",
                table: "Notifications",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UpdatedBy",
                table: "Notifications",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ParticipantRoles_CreatedBy",
                table: "ParticipantRoles",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ParticipantRoles_DeletedBy",
                table: "ParticipantRoles",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ParticipantRoles_UpdatedBy",
                table: "ParticipantRoles",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ParticipantStatuses_CreatedBy",
                table: "ParticipantStatuses",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ParticipantStatuses_DeletedBy",
                table: "ParticipantStatuses",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ParticipantStatuses_UpdatedBy",
                table: "ParticipantStatuses",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRefunds_CreatedBy",
                table: "PaymentRefunds",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRefunds_DeletedBy",
                table: "PaymentRefunds",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRefunds_ProcessedByUserId",
                table: "PaymentRefunds",
                column: "ProcessedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRefunds_RefundedAt",
                table: "PaymentRefunds",
                column: "RefundedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRefunds_StripeRefundId",
                table: "PaymentRefunds",
                column: "StripeRefundId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRefunds_SubscriptionPaymentId",
                table: "PaymentRefunds",
                column: "SubscriptionPaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRefunds_UpdatedBy",
                table: "PaymentRefunds",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentStatuses_CreatedBy",
                table: "PaymentStatuses",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentStatuses_DeletedBy",
                table: "PaymentStatuses",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentStatuses_Name",
                table: "PaymentStatuses",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentStatuses_SortOrder",
                table: "PaymentStatuses",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentStatuses_UpdatedBy",
                table: "PaymentStatuses",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PrescriptionItems_CreatedBy",
                table: "PrescriptionItems",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PrescriptionItems_DeletedBy",
                table: "PrescriptionItems",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PrescriptionItems_PrescriptionId",
                table: "PrescriptionItems",
                column: "PrescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_PrescriptionItems_UpdatedBy",
                table: "PrescriptionItems",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_ConsultationId",
                table: "Prescriptions",
                column: "ConsultationId");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_CreatedBy",
                table: "Prescriptions",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_DeletedBy",
                table: "Prescriptions",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_ProviderId",
                table: "Prescriptions",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_UpdatedBy",
                table: "Prescriptions",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_UserId",
                table: "Prescriptions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Privileges_CreatedBy",
                table: "Privileges",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Privileges_DeletedBy",
                table: "Privileges",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Privileges_IsActive",
                table: "Privileges",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Privileges_MasterPrivilegeTypeId",
                table: "Privileges",
                column: "MasterPrivilegeTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Privileges_Name",
                table: "Privileges",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Privileges_PrivilegeTypeId",
                table: "Privileges",
                column: "PrivilegeTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Privileges_UpdatedBy",
                table: "Privileges",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PrivilegeUsageHistories_CreatedBy",
                table: "PrivilegeUsageHistories",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PrivilegeUsageHistories_DeletedBy",
                table: "PrivilegeUsageHistories",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PrivilegeUsageHistories_UpdatedBy",
                table: "PrivilegeUsageHistories",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PrivilegeUsageHistories_UsageDate",
                table: "PrivilegeUsageHistories",
                column: "UsageDate");

            migrationBuilder.CreateIndex(
                name: "IX_PrivilegeUsageHistories_UsageMonth",
                table: "PrivilegeUsageHistories",
                column: "UsageMonth");

            migrationBuilder.CreateIndex(
                name: "IX_PrivilegeUsageHistories_UsageWeek",
                table: "PrivilegeUsageHistories",
                column: "UsageWeek");

            migrationBuilder.CreateIndex(
                name: "IX_PrivilegeUsageHistories_UserSubscriptionPrivilegeUsageId",
                table: "PrivilegeUsageHistories",
                column: "UserSubscriptionPrivilegeUsageId");

            migrationBuilder.CreateIndex(
                name: "IX_PrivilegeUsageHistories_UserSubscriptionPrivilegeUsageId_UsageDate",
                table: "PrivilegeUsageHistories",
                columns: new[] { "UserSubscriptionPrivilegeUsageId", "UsageDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ProcessedWebhookEvents_EventType",
                table: "ProcessedWebhookEvents",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessedWebhookEvents_EventType_IsSuccess",
                table: "ProcessedWebhookEvents",
                columns: new[] { "EventType", "IsSuccess" });

            migrationBuilder.CreateIndex(
                name: "IX_ProcessedWebhookEvents_IsSuccess",
                table: "ProcessedWebhookEvents",
                column: "IsSuccess");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessedWebhookEvents_ProcessedAt",
                table: "ProcessedWebhookEvents",
                column: "ProcessedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessedWebhookEvents_ReceivedAt",
                table: "ProcessedWebhookEvents",
                column: "ReceivedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessedWebhookEvents_StripeEventId",
                table: "ProcessedWebhookEvents",
                column: "StripeEventId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProviderCategories_CategoryId",
                table: "ProviderCategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderCategories_CreatedBy",
                table: "ProviderCategories",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderCategories_DeletedBy",
                table: "ProviderCategories",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderCategories_ProviderId_CategoryId",
                table: "ProviderCategories",
                columns: new[] { "ProviderId", "CategoryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProviderCategories_UpdatedBy",
                table: "ProviderCategories",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderFees_CategoryId",
                table: "ProviderFees",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderFees_CreatedBy",
                table: "ProviderFees",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderFees_DeletedBy",
                table: "ProviderFees",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderFees_ProviderId",
                table: "ProviderFees",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderFees_ReviewedByUserId",
                table: "ProviderFees",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderFees_UpdatedBy",
                table: "ProviderFees",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderOnboardings_CreatedBy",
                table: "ProviderOnboardings",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderOnboardings_DeletedBy",
                table: "ProviderOnboardings",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderOnboardings_ReviewedByUserId",
                table: "ProviderOnboardings",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderOnboardings_UpdatedBy",
                table: "ProviderOnboardings",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderOnboardings_UserId",
                table: "ProviderOnboardings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Providers_CreatedBy",
                table: "Providers",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Providers_DeletedBy",
                table: "Providers",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Providers_UpdatedBy",
                table: "Providers",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireTemplates_CategoryId",
                table: "QuestionnaireTemplates",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireTemplates_CategoryId_IsActive_IsDeleted",
                table: "QuestionnaireTemplates",
                columns: new[] { "CategoryId", "IsActive", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireTemplates_CategoryId1",
                table: "QuestionnaireTemplates",
                column: "CategoryId1");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireTemplates_CreatedBy",
                table: "QuestionnaireTemplates",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireTemplates_DeletedBy",
                table: "QuestionnaireTemplates",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireTemplates_IsActive",
                table: "QuestionnaireTemplates",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireTemplates_IsDeleted",
                table: "QuestionnaireTemplates",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireTemplates_UpdatedBy",
                table: "QuestionnaireTemplates",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionOptions_CreatedBy",
                table: "QuestionOptions",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionOptions_DeletedBy",
                table: "QuestionOptions",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionOptions_IsCorrect",
                table: "QuestionOptions",
                column: "IsCorrect");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionOptions_IsDeleted",
                table: "QuestionOptions",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionOptions_Order",
                table: "QuestionOptions",
                column: "Order");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionOptions_QuestionId",
                table: "QuestionOptions",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionOptions_QuestionId_IsDeleted",
                table: "QuestionOptions",
                columns: new[] { "QuestionId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionOptions_QuestionId_Order",
                table: "QuestionOptions",
                columns: new[] { "QuestionId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionOptions_UpdatedBy",
                table: "QuestionOptions",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_CreatedBy",
                table: "Questions",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_DeletedBy",
                table: "Questions",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_IsDeleted",
                table: "Questions",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_Order",
                table: "Questions",
                column: "Order");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_TemplateId",
                table: "Questions",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_TemplateId_IsDeleted",
                table: "Questions",
                columns: new[] { "TemplateId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Questions_TemplateId_Order",
                table: "Questions",
                columns: new[] { "TemplateId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_Questions_Type",
                table: "Questions",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_UpdatedBy",
                table: "Questions",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RefundStatuses_CreatedBy",
                table: "RefundStatuses",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RefundStatuses_DeletedBy",
                table: "RefundStatuses",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RefundStatuses_Name",
                table: "RefundStatuses",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_RefundStatuses_SortOrder",
                table: "RefundStatuses",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_RefundStatuses_UpdatedBy",
                table: "RefundStatuses",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ReminderTimings_CreatedBy",
                table: "ReminderTimings",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ReminderTimings_DeletedBy",
                table: "ReminderTimings",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ReminderTimings_UpdatedBy",
                table: "ReminderTimings",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ReminderTypes_CreatedBy",
                table: "ReminderTypes",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ReminderTypes_DeletedBy",
                table: "ReminderTypes",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ReminderTypes_UpdatedBy",
                table: "ReminderTypes",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledPlanMigrations_CreatedBy",
                table: "ScheduledPlanMigrations",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledPlanMigrations_DeletedBy",
                table: "ScheduledPlanMigrations",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledPlanMigrations_FromPlanId",
                table: "ScheduledPlanMigrations",
                column: "FromPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledPlanMigrations_ScheduledMigrationDate",
                table: "ScheduledPlanMigrations",
                column: "ScheduledMigrationDate");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledPlanMigrations_Status",
                table: "ScheduledPlanMigrations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledPlanMigrations_Status_ScheduledMigrationDate",
                table: "ScheduledPlanMigrations",
                columns: new[] { "Status", "ScheduledMigrationDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledPlanMigrations_SubscriptionId",
                table: "ScheduledPlanMigrations",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledPlanMigrations_ToPlanId",
                table: "ScheduledPlanMigrations",
                column: "ToPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledPlanMigrations_UpdatedBy",
                table: "ScheduledPlanMigrations",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceConstraints_CreatedBy",
                table: "ServiceConstraints",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceConstraints_DeletedBy",
                table: "ServiceConstraints",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceConstraints_SubscriptionPlanId",
                table: "ServiceConstraints",
                column: "SubscriptionPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceConstraints_UpdatedBy",
                table: "ServiceConstraints",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPayments_BillingRecordId",
                table: "SubscriptionPayments",
                column: "BillingRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPayments_CreatedBy",
                table: "SubscriptionPayments",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPayments_CurrencyId",
                table: "SubscriptionPayments",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPayments_DeletedBy",
                table: "SubscriptionPayments",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPayments_DueDate",
                table: "SubscriptionPayments",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPayments_PaidAt",
                table: "SubscriptionPayments",
                column: "PaidAt");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPayments_Status",
                table: "SubscriptionPayments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPayments_StripeInvoiceId",
                table: "SubscriptionPayments",
                column: "StripeInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPayments_StripePaymentIntentId",
                table: "SubscriptionPayments",
                column: "StripePaymentIntentId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPayments_SubscriptionId",
                table: "SubscriptionPayments",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPayments_Type",
                table: "SubscriptionPayments",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPayments_UpdatedBy",
                table: "SubscriptionPayments",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlanPrivileges_CreatedBy",
                table: "SubscriptionPlanPrivileges",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlanPrivileges_DeletedBy",
                table: "SubscriptionPlanPrivileges",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlanPrivileges_EffectiveDate",
                table: "SubscriptionPlanPrivileges",
                column: "EffectiveDate");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlanPrivileges_ExpirationDate",
                table: "SubscriptionPlanPrivileges",
                column: "ExpirationDate");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlanPrivileges_PrivilegeId",
                table: "SubscriptionPlanPrivileges",
                column: "PrivilegeId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlanPrivileges_SubscriptionPlanId",
                table: "SubscriptionPlanPrivileges",
                column: "SubscriptionPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlanPrivileges_UpdatedBy",
                table: "SubscriptionPlanPrivileges",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlanPrivileges_UsagePeriodId",
                table: "SubscriptionPlanPrivileges",
                column: "UsagePeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlans_BillingCycleId",
                table: "SubscriptionPlans",
                column: "BillingCycleId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlans_CategoryId",
                table: "SubscriptionPlans",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlans_CreatedBy",
                table: "SubscriptionPlans",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlans_CurrencyId",
                table: "SubscriptionPlans",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlans_DeletedBy",
                table: "SubscriptionPlans",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlans_IsActive",
                table: "SubscriptionPlans",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlans_IsFeatured",
                table: "SubscriptionPlans",
                column: "IsFeatured");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlans_IsLatestVersion",
                table: "SubscriptionPlans",
                column: "IsLatestVersion");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlans_MasterBillingCycleId",
                table: "SubscriptionPlans",
                column: "MasterBillingCycleId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlans_MasterCurrencyId",
                table: "SubscriptionPlans",
                column: "MasterCurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlans_Name",
                table: "SubscriptionPlans",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlans_ParentPlanId",
                table: "SubscriptionPlans",
                column: "ParentPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlans_ParentPlanId_VersionNumber",
                table: "SubscriptionPlans",
                columns: new[] { "ParentPlanId", "VersionNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlans_PlanType",
                table: "SubscriptionPlans",
                column: "PlanType");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlans_StripeProductId",
                table: "SubscriptionPlans",
                column: "StripeProductId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlans_UpdatedBy",
                table: "SubscriptionPlans",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_AutoRenew",
                table: "Subscriptions",
                column: "AutoRenew");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_BillingCycleId",
                table: "Subscriptions",
                column: "BillingCycleId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_CreatedBy",
                table: "Subscriptions",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_DeletedBy",
                table: "Subscriptions",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_HealthAssessmentId",
                table: "Subscriptions",
                column: "HealthAssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_IsTrialSubscription",
                table: "Subscriptions",
                column: "IsTrialSubscription");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_MasterBillingCycleId",
                table: "Subscriptions",
                column: "MasterBillingCycleId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_MasterCurrencyId",
                table: "Subscriptions",
                column: "MasterCurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_NextBillingDate",
                table: "Subscriptions",
                column: "NextBillingDate");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_ProviderId",
                table: "Subscriptions",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_StartDate",
                table: "Subscriptions",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_Status",
                table: "Subscriptions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_StripeCustomerId",
                table: "Subscriptions",
                column: "StripeCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_StripeSubscriptionId",
                table: "Subscriptions",
                column: "StripeSubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_SubscriptionPlanId",
                table: "Subscriptions",
                column: "SubscriptionPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_UpdatedBy",
                table: "Subscriptions",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_UserId",
                table: "Subscriptions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionStatusHistories_ChangedAt",
                table: "SubscriptionStatusHistories",
                column: "ChangedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionStatusHistories_ChangedByUserId",
                table: "SubscriptionStatusHistories",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionStatusHistories_CreatedBy",
                table: "SubscriptionStatusHistories",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionStatusHistories_DeletedBy",
                table: "SubscriptionStatusHistories",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionStatusHistories_SubscriptionId",
                table: "SubscriptionStatusHistories",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionStatusHistories_ToStatus",
                table: "SubscriptionStatusHistories",
                column: "ToStatus");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionStatusHistories_UpdatedBy",
                table: "SubscriptionStatusHistories",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SystemSettings_CreatedBy",
                table: "SystemSettings",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SystemSettings_DeletedBy",
                table: "SystemSettings",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SystemSettings_UpdatedBy",
                table: "SystemSettings",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswerOptions_AnswerId",
                table: "UserAnswerOptions",
                column: "AnswerId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswerOptions_AnswerId_OptionId",
                table: "UserAnswerOptions",
                columns: new[] { "AnswerId", "OptionId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswerOptions_AnswerId_OptionId_IsDeleted",
                table: "UserAnswerOptions",
                columns: new[] { "AnswerId", "OptionId", "IsDeleted" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswerOptions_CreatedBy",
                table: "UserAnswerOptions",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswerOptions_CreatedDate",
                table: "UserAnswerOptions",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswerOptions_DeletedBy",
                table: "UserAnswerOptions",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswerOptions_IsDeleted",
                table: "UserAnswerOptions",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswerOptions_OptionId",
                table: "UserAnswerOptions",
                column: "OptionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswerOptions_UpdatedBy",
                table: "UserAnswerOptions",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswers_CreatedBy",
                table: "UserAnswers",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswers_CreatedDate",
                table: "UserAnswers",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswers_DeletedBy",
                table: "UserAnswers",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswers_IsDeleted",
                table: "UserAnswers",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswers_QuestionId",
                table: "UserAnswers",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswers_ResponseId",
                table: "UserAnswers",
                column: "ResponseId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswers_ResponseId_IsDeleted",
                table: "UserAnswers",
                columns: new[] { "ResponseId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswers_ResponseId_QuestionId",
                table: "UserAnswers",
                columns: new[] { "ResponseId", "QuestionId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswers_UpdatedBy",
                table: "UserAnswers",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_UserResponses_CategoryId",
                table: "UserResponses",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_UserResponses_CategoryId1",
                table: "UserResponses",
                column: "CategoryId1");

            migrationBuilder.CreateIndex(
                name: "IX_UserResponses_CreatedBy",
                table: "UserResponses",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_UserResponses_CreatedDate",
                table: "UserResponses",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_UserResponses_DeletedBy",
                table: "UserResponses",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_UserResponses_IsDeleted",
                table: "UserResponses",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_UserResponses_Status",
                table: "UserResponses",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_UserResponses_TemplateId",
                table: "UserResponses",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_UserResponses_UpdatedBy",
                table: "UserResponses",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_UserResponses_UserId",
                table: "UserResponses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserResponses_UserId_Status_IsDeleted",
                table: "UserResponses",
                columns: new[] { "UserId", "Status", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_UserResponses_UserId_TemplateId",
                table: "UserResponses",
                columns: new[] { "UserId", "TemplateId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_CreatedBy",
                table: "UserRoles",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_DeletedBy",
                table: "UserRoles",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UpdatedBy",
                table: "UserRoles",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "Users",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserRoleId",
                table: "Users",
                column: "UserRoleId");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "Users",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptionPrivilegeUsages_CreatedBy",
                table: "UserSubscriptionPrivilegeUsages",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptionPrivilegeUsages_DeletedBy",
                table: "UserSubscriptionPrivilegeUsages",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptionPrivilegeUsages_LastUsedAt",
                table: "UserSubscriptionPrivilegeUsages",
                column: "LastUsedAt");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptionPrivilegeUsages_PrivilegeId",
                table: "UserSubscriptionPrivilegeUsages",
                column: "PrivilegeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptionPrivilegeUsages_PrivilegeId1",
                table: "UserSubscriptionPrivilegeUsages",
                column: "PrivilegeId1");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptionPrivilegeUsages_SubscriptionId",
                table: "UserSubscriptionPrivilegeUsages",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptionPrivilegeUsages_SubscriptionPlanPrivilegeId",
                table: "UserSubscriptionPrivilegeUsages",
                column: "SubscriptionPlanPrivilegeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptionPrivilegeUsages_UpdatedBy",
                table: "UserSubscriptionPrivilegeUsages",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptionPrivilegeUsages_UsagePeriodEnd",
                table: "UserSubscriptionPrivilegeUsages",
                column: "UsagePeriodEnd");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptionPrivilegeUsages_UsagePeriodStart",
                table: "UserSubscriptionPrivilegeUsages",
                column: "UsagePeriodStart");

            migrationBuilder.CreateIndex(
                name: "IX_VideoCallEvents_CreatedBy",
                table: "VideoCallEvents",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_VideoCallEvents_DeletedBy",
                table: "VideoCallEvents",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_VideoCallEvents_ProviderId",
                table: "VideoCallEvents",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoCallEvents_UpdatedBy",
                table: "VideoCallEvents",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_VideoCallEvents_UserId",
                table: "VideoCallEvents",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoCallEvents_VideoCallId",
                table: "VideoCallEvents",
                column: "VideoCallId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoCallParticipants_CreatedBy",
                table: "VideoCallParticipants",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_VideoCallParticipants_DeletedBy",
                table: "VideoCallParticipants",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_VideoCallParticipants_ProviderId",
                table: "VideoCallParticipants",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoCallParticipants_UpdatedBy",
                table: "VideoCallParticipants",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_VideoCallParticipants_UserId",
                table: "VideoCallParticipants",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoCallParticipants_VideoCallId",
                table: "VideoCallParticipants",
                column: "VideoCallId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoCalls_AppointmentId",
                table: "VideoCalls",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoCalls_CreatedBy",
                table: "VideoCalls",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_VideoCalls_DeletedBy",
                table: "VideoCalls",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_VideoCalls_UpdatedBy",
                table: "VideoCalls",
                column: "UpdatedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentDocuments_Appointments_AppointmentId",
                table: "AppointmentDocuments",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentDocuments_DocumentTypes_DocumentTypeId",
                table: "AppointmentDocuments",
                column: "DocumentTypeId",
                principalTable: "DocumentTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentDocuments_Providers_ProviderId",
                table: "AppointmentDocuments",
                column: "ProviderId",
                principalTable: "Providers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentDocuments_Users_CreatedBy",
                table: "AppointmentDocuments",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentDocuments_Users_DeletedBy",
                table: "AppointmentDocuments",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentDocuments_Users_UpdatedBy",
                table: "AppointmentDocuments",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentDocuments_Users_UploadedById",
                table: "AppointmentDocuments",
                column: "UploadedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentEvents_Appointments_AppointmentId",
                table: "AppointmentEvents",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentEvents_EventTypes_EventTypeId",
                table: "AppointmentEvents",
                column: "EventTypeId",
                principalTable: "EventTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentEvents_Providers_ProviderId",
                table: "AppointmentEvents",
                column: "ProviderId",
                principalTable: "Providers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentEvents_Users_CreatedBy",
                table: "AppointmentEvents",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentEvents_Users_DeletedBy",
                table: "AppointmentEvents",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentEvents_Users_UpdatedBy",
                table: "AppointmentEvents",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentEvents_Users_UserId",
                table: "AppointmentEvents",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentInvitations_Appointments_AppointmentId",
                table: "AppointmentInvitations",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentInvitations_InvitationStatuses_InvitationStatusId",
                table: "AppointmentInvitations",
                column: "InvitationStatusId",
                principalTable: "InvitationStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentInvitations_Users_CreatedBy",
                table: "AppointmentInvitations",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentInvitations_Users_DeletedBy",
                table: "AppointmentInvitations",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentInvitations_Users_InvitedByUserId",
                table: "AppointmentInvitations",
                column: "InvitedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentInvitations_Users_InvitedUserId",
                table: "AppointmentInvitations",
                column: "InvitedUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentInvitations_Users_UpdatedBy",
                table: "AppointmentInvitations",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentParticipants_Appointments_AppointmentId",
                table: "AppointmentParticipants",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentParticipants_ParticipantRoles_ParticipantRoleId",
                table: "AppointmentParticipants",
                column: "ParticipantRoleId",
                principalTable: "ParticipantRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentParticipants_ParticipantStatuses_ParticipantStatusId",
                table: "AppointmentParticipants",
                column: "ParticipantStatusId",
                principalTable: "ParticipantStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentParticipants_Users_CreatedBy",
                table: "AppointmentParticipants",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentParticipants_Users_DeletedBy",
                table: "AppointmentParticipants",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentParticipants_Users_InvitedByUserId",
                table: "AppointmentParticipants",
                column: "InvitedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentParticipants_Users_UpdatedBy",
                table: "AppointmentParticipants",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentParticipants_Users_UserId",
                table: "AppointmentParticipants",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentPaymentLogs_Appointments_AppointmentId",
                table: "AppointmentPaymentLogs",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentPaymentLogs_PaymentStatuses_PaymentStatusId",
                table: "AppointmentPaymentLogs",
                column: "PaymentStatusId",
                principalTable: "PaymentStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentPaymentLogs_RefundStatuses_RefundStatusId",
                table: "AppointmentPaymentLogs",
                column: "RefundStatusId",
                principalTable: "RefundStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentPaymentLogs_Users_CreatedBy",
                table: "AppointmentPaymentLogs",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentPaymentLogs_Users_DeletedBy",
                table: "AppointmentPaymentLogs",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentPaymentLogs_Users_UpdatedBy",
                table: "AppointmentPaymentLogs",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentPaymentLogs_Users_UserId",
                table: "AppointmentPaymentLogs",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentReminders_Appointments_AppointmentId",
                table: "AppointmentReminders",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentReminders_ReminderTimings_ReminderTimingId",
                table: "AppointmentReminders",
                column: "ReminderTimingId",
                principalTable: "ReminderTimings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentReminders_ReminderTypes_ReminderTypeId",
                table: "AppointmentReminders",
                column: "ReminderTypeId",
                principalTable: "ReminderTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentReminders_Users_CreatedBy",
                table: "AppointmentReminders",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentReminders_Users_DeletedBy",
                table: "AppointmentReminders",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentReminders_Users_UpdatedBy",
                table: "AppointmentReminders",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_AppointmentStatuses_AppointmentStatusId",
                table: "Appointments",
                column: "AppointmentStatusId",
                principalTable: "AppointmentStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_AppointmentTypes_AppointmentTypeId",
                table: "Appointments",
                column: "AppointmentTypeId",
                principalTable: "AppointmentTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Categories_CategoryId",
                table: "Appointments",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_ConsultationModes_ConsultationModeId",
                table: "Appointments",
                column: "ConsultationModeId",
                principalTable: "ConsultationModes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Consultations_ConsultationId",
                table: "Appointments",
                column: "ConsultationId",
                principalTable: "Consultations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_PaymentStatuses_PaymentStatusId",
                table: "Appointments",
                column: "PaymentStatusId",
                principalTable: "PaymentStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Providers_ProviderId",
                table: "Appointments",
                column: "ProviderId",
                principalTable: "Providers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Subscriptions_SubscriptionId",
                table: "Appointments",
                column: "SubscriptionId",
                principalTable: "Subscriptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Users_CreatedBy",
                table: "Appointments",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Users_DeletedBy",
                table: "Appointments",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Users_PatientId",
                table: "Appointments",
                column: "PatientId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Users_UpdatedBy",
                table: "Appointments",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentStatuses_Users_CreatedBy",
                table: "AppointmentStatuses",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentStatuses_Users_DeletedBy",
                table: "AppointmentStatuses",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentStatuses_Users_UpdatedBy",
                table: "AppointmentStatuses",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentTypes_Users_CreatedBy",
                table: "AppointmentTypes",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentTypes_Users_DeletedBy",
                table: "AppointmentTypes",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentTypes_Users_UpdatedBy",
                table: "AppointmentTypes",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserClaims_Users_UserId",
                table: "AspNetUserClaims",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserLogins_Users_UserId",
                table: "AspNetUserLogins",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_Users_UserId",
                table: "AspNetUserRoles",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserTokens_Users_UserId",
                table: "AspNetUserTokens",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BillingAdjustments_BillingRecords_BillingRecordId",
                table: "BillingAdjustments",
                column: "BillingRecordId",
                principalTable: "BillingRecords",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BillingAdjustments_Users_AppliedBy",
                table: "BillingAdjustments",
                column: "AppliedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_BillingAdjustments_Users_CreatedBy",
                table: "BillingAdjustments",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BillingAdjustments_Users_DeletedBy",
                table: "BillingAdjustments",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BillingAdjustments_Users_UpdatedBy",
                table: "BillingAdjustments",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BillingRecords_Consultations_ConsultationId",
                table: "BillingRecords",
                column: "ConsultationId",
                principalTable: "Consultations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_BillingRecords_MasterCurrencies_CurrencyId",
                table: "BillingRecords",
                column: "CurrencyId",
                principalTable: "MasterCurrencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BillingRecords_MasterCurrencies_MasterCurrencyId",
                table: "BillingRecords",
                column: "MasterCurrencyId",
                principalTable: "MasterCurrencies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BillingRecords_MedicationDeliveries_MedicationDeliveryId",
                table: "BillingRecords",
                column: "MedicationDeliveryId",
                principalTable: "MedicationDeliveries",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_BillingRecords_Subscriptions_SubscriptionId",
                table: "BillingRecords",
                column: "SubscriptionId",
                principalTable: "Subscriptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_BillingRecords_Users_CreatedBy",
                table: "BillingRecords",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BillingRecords_Users_DeletedBy",
                table: "BillingRecords",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BillingRecords_Users_UpdatedBy",
                table: "BillingRecords",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BillingRecords_Users_UserId",
                table: "BillingRecords",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Users_CreatedBy",
                table: "Categories",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Users_DeletedBy",
                table: "Categories",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Users_UpdatedBy",
                table: "Categories",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CategoryFeeRanges_Users_CreatedBy",
                table: "CategoryFeeRanges",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CategoryFeeRanges_Users_DeletedBy",
                table: "CategoryFeeRanges",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CategoryFeeRanges_Users_UpdatedBy",
                table: "CategoryFeeRanges",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatAttachments_ChatSessions_SessionId",
                table: "ChatAttachments",
                column: "SessionId",
                principalTable: "ChatSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatAttachments_Users_CreatedBy",
                table: "ChatAttachments",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatAttachments_Users_DeletedBy",
                table: "ChatAttachments",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatAttachments_Users_UpdatedBy",
                table: "ChatAttachments",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_ChatSessions_SessionId",
                table: "ChatMessages",
                column: "SessionId",
                principalTable: "ChatSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_Users_CreatedBy",
                table: "ChatMessages",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_Users_DeletedBy",
                table: "ChatMessages",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_Users_UpdatedBy",
                table: "ChatMessages",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatRoomParticipants_ChatRooms_ChatRoomId",
                table: "ChatRoomParticipants",
                column: "ChatRoomId",
                principalTable: "ChatRooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatRoomParticipants_Providers_ProviderId",
                table: "ChatRoomParticipants",
                column: "ProviderId",
                principalTable: "Providers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatRoomParticipants_Users_CreatedBy",
                table: "ChatRoomParticipants",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatRoomParticipants_Users_DeletedBy",
                table: "ChatRoomParticipants",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatRoomParticipants_Users_UpdatedBy",
                table: "ChatRoomParticipants",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatRoomParticipants_Users_UserId",
                table: "ChatRoomParticipants",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatRooms_Consultations_ConsultationId",
                table: "ChatRooms",
                column: "ConsultationId",
                principalTable: "Consultations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatRooms_Providers_ProviderId",
                table: "ChatRooms",
                column: "ProviderId",
                principalTable: "Providers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatRooms_Subscriptions_SubscriptionId",
                table: "ChatRooms",
                column: "SubscriptionId",
                principalTable: "Subscriptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatRooms_Users_CreatedBy",
                table: "ChatRooms",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatRooms_Users_DeletedBy",
                table: "ChatRooms",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatRooms_Users_PatientId",
                table: "ChatRooms",
                column: "PatientId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatRooms_Users_UpdatedBy",
                table: "ChatRooms",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatSessions_Providers_ProviderId",
                table: "ChatSessions",
                column: "ProviderId",
                principalTable: "Providers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatSessions_Subscriptions_SubscriptionId",
                table: "ChatSessions",
                column: "SubscriptionId",
                principalTable: "Subscriptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatSessions_Users_CreatedBy",
                table: "ChatSessions",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatSessions_Users_DeletedBy",
                table: "ChatSessions",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatSessions_Users_UpdatedBy",
                table: "ChatSessions",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatSessions_Users_UserId",
                table: "ChatSessions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ConsultationModes_Users_CreatedBy",
                table: "ConsultationModes",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ConsultationModes_Users_DeletedBy",
                table: "ConsultationModes",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ConsultationModes_Users_UpdatedBy",
                table: "ConsultationModes",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Consultations_HealthAssessments_HealthAssessmentId",
                table: "Consultations",
                column: "HealthAssessmentId",
                principalTable: "HealthAssessments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Consultations_Providers_ProviderId",
                table: "Consultations",
                column: "ProviderId",
                principalTable: "Providers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Consultations_Subscriptions_SubscriptionId",
                table: "Consultations",
                column: "SubscriptionId",
                principalTable: "Subscriptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Consultations_Users_CreatedBy",
                table: "Consultations",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Consultations_Users_DeletedBy",
                table: "Consultations",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Consultations_Users_UpdatedBy",
                table: "Consultations",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Consultations_Users_UserId",
                table: "Consultations",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryTracking_MedicationDeliveries_MedicationDeliveryId",
                table: "DeliveryTracking",
                column: "MedicationDeliveryId",
                principalTable: "MedicationDeliveries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryTracking_Users_CreatedBy",
                table: "DeliveryTracking",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryTracking_Users_DeletedBy",
                table: "DeliveryTracking",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryTracking_Users_UpdatedBy",
                table: "DeliveryTracking",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentReferences_Documents_DocumentId",
                table: "DocumentReferences",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentReferences_Users_CreatedBy",
                table: "DocumentReferences",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentReferences_Users_DeletedBy",
                table: "DocumentReferences",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentReferences_Users_UpdatedBy",
                table: "DocumentReferences",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_DocumentTypes_DocumentTypeId",
                table: "Documents",
                column: "DocumentTypeId",
                principalTable: "DocumentTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Users_CreatedBy",
                table: "Documents",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Users_DeletedBy",
                table: "Documents",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Users_UpdatedBy",
                table: "Documents",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentTypes_Users_CreatedBy",
                table: "DocumentTypes",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentTypes_Users_DeletedBy",
                table: "DocumentTypes",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentTypes_Users_UpdatedBy",
                table: "DocumentTypes",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EventTypes_Users_CreatedBy",
                table: "EventTypes",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EventTypes_Users_DeletedBy",
                table: "EventTypes",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EventTypes_Users_UpdatedBy",
                table: "EventTypes",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HealthAssessments_Providers_ProviderId",
                table: "HealthAssessments",
                column: "ProviderId",
                principalTable: "Providers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_HealthAssessments_Users_CreatedBy",
                table: "HealthAssessments",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HealthAssessments_Users_DeletedBy",
                table: "HealthAssessments",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HealthAssessments_Users_UpdatedBy",
                table: "HealthAssessments",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HealthAssessments_Users_UserId",
                table: "HealthAssessments",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InvitationStatuses_Users_CreatedBy",
                table: "InvitationStatuses",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InvitationStatuses_Users_DeletedBy",
                table: "InvitationStatuses",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InvitationStatuses_Users_UpdatedBy",
                table: "InvitationStatuses",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MasterBillingCycles_Users_CreatedBy",
                table: "MasterBillingCycles",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MasterBillingCycles_Users_DeletedBy",
                table: "MasterBillingCycles",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MasterBillingCycles_Users_UpdatedBy",
                table: "MasterBillingCycles",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MasterCurrencies_Users_CreatedBy",
                table: "MasterCurrencies",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MasterCurrencies_Users_DeletedBy",
                table: "MasterCurrencies",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MasterCurrencies_Users_UpdatedBy",
                table: "MasterCurrencies",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MasterPrivilegeTypes_Users_CreatedBy",
                table: "MasterPrivilegeTypes",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MasterPrivilegeTypes_Users_DeletedBy",
                table: "MasterPrivilegeTypes",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MasterPrivilegeTypes_Users_UpdatedBy",
                table: "MasterPrivilegeTypes",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicationDeliveries_Providers_ProviderId",
                table: "MedicationDeliveries",
                column: "ProviderId",
                principalTable: "Providers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicationDeliveries_Subscriptions_SubscriptionId",
                table: "MedicationDeliveries",
                column: "SubscriptionId",
                principalTable: "Subscriptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicationDeliveries_Users_CreatedBy",
                table: "MedicationDeliveries",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicationDeliveries_Users_DeletedBy",
                table: "MedicationDeliveries",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicationDeliveries_Users_UpdatedBy",
                table: "MedicationDeliveries",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicationDeliveries_Users_UserId",
                table: "MedicationDeliveries",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MessageAttachments_Messages_MessageId",
                table: "MessageAttachments",
                column: "MessageId",
                principalTable: "Messages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MessageAttachments_Users_CreatedBy",
                table: "MessageAttachments",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MessageAttachments_Users_DeletedBy",
                table: "MessageAttachments",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MessageAttachments_Users_UpdatedBy",
                table: "MessageAttachments",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MessageReactions_Messages_MessageId",
                table: "MessageReactions",
                column: "MessageId",
                principalTable: "Messages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MessageReactions_Providers_ProviderId",
                table: "MessageReactions",
                column: "ProviderId",
                principalTable: "Providers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_MessageReactions_Users_CreatedBy",
                table: "MessageReactions",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MessageReactions_Users_DeletedBy",
                table: "MessageReactions",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MessageReactions_Users_UpdatedBy",
                table: "MessageReactions",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MessageReactions_Users_UserId",
                table: "MessageReactions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MessageReadReceipts_Messages_MessageId",
                table: "MessageReadReceipts",
                column: "MessageId",
                principalTable: "Messages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MessageReadReceipts_Providers_ProviderId",
                table: "MessageReadReceipts",
                column: "ProviderId",
                principalTable: "Providers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_MessageReadReceipts_Users_CreatedBy",
                table: "MessageReadReceipts",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MessageReadReceipts_Users_DeletedBy",
                table: "MessageReadReceipts",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MessageReadReceipts_Users_UpdatedBy",
                table: "MessageReadReceipts",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MessageReadReceipts_Users_UserId",
                table: "MessageReadReceipts",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Providers_ProviderId",
                table: "Messages",
                column: "ProviderId",
                principalTable: "Providers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Users_CreatedBy",
                table: "Messages",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Users_DeletedBy",
                table: "Messages",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Users_SenderId",
                table: "Messages",
                column: "SenderId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Users_UpdatedBy",
                table: "Messages",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Users_UserId",
                table: "Messages",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Users_CreatedBy",
                table: "Notifications",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Users_DeletedBy",
                table: "Notifications",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Users_UpdatedBy",
                table: "Notifications",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Users_UserId",
                table: "Notifications",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ParticipantRoles_Users_CreatedBy",
                table: "ParticipantRoles",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ParticipantRoles_Users_DeletedBy",
                table: "ParticipantRoles",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ParticipantRoles_Users_UpdatedBy",
                table: "ParticipantRoles",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ParticipantStatuses_Users_CreatedBy",
                table: "ParticipantStatuses",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ParticipantStatuses_Users_DeletedBy",
                table: "ParticipantStatuses",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ParticipantStatuses_Users_UpdatedBy",
                table: "ParticipantStatuses",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentRefunds_SubscriptionPayments_SubscriptionPaymentId",
                table: "PaymentRefunds",
                column: "SubscriptionPaymentId",
                principalTable: "SubscriptionPayments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentRefunds_Users_CreatedBy",
                table: "PaymentRefunds",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentRefunds_Users_DeletedBy",
                table: "PaymentRefunds",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentRefunds_Users_ProcessedByUserId",
                table: "PaymentRefunds",
                column: "ProcessedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentRefunds_Users_UpdatedBy",
                table: "PaymentRefunds",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentStatuses_Users_CreatedBy",
                table: "PaymentStatuses",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentStatuses_Users_DeletedBy",
                table: "PaymentStatuses",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentStatuses_Users_UpdatedBy",
                table: "PaymentStatuses",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PrescriptionItems_Prescriptions_PrescriptionId",
                table: "PrescriptionItems",
                column: "PrescriptionId",
                principalTable: "Prescriptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PrescriptionItems_Users_CreatedBy",
                table: "PrescriptionItems",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PrescriptionItems_Users_DeletedBy",
                table: "PrescriptionItems",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PrescriptionItems_Users_UpdatedBy",
                table: "PrescriptionItems",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Prescriptions_Providers_ProviderId",
                table: "Prescriptions",
                column: "ProviderId",
                principalTable: "Providers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Prescriptions_Users_CreatedBy",
                table: "Prescriptions",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Prescriptions_Users_DeletedBy",
                table: "Prescriptions",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Prescriptions_Users_UpdatedBy",
                table: "Prescriptions",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Prescriptions_Users_UserId",
                table: "Prescriptions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Privileges_Users_CreatedBy",
                table: "Privileges",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Privileges_Users_DeletedBy",
                table: "Privileges",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Privileges_Users_UpdatedBy",
                table: "Privileges",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PrivilegeUsageHistories_UserSubscriptionPrivilegeUsages_UserSubscriptionPrivilegeUsageId",
                table: "PrivilegeUsageHistories",
                column: "UserSubscriptionPrivilegeUsageId",
                principalTable: "UserSubscriptionPrivilegeUsages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PrivilegeUsageHistories_Users_CreatedBy",
                table: "PrivilegeUsageHistories",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PrivilegeUsageHistories_Users_DeletedBy",
                table: "PrivilegeUsageHistories",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PrivilegeUsageHistories_Users_UpdatedBy",
                table: "PrivilegeUsageHistories",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProviderCategories_Providers_ProviderId",
                table: "ProviderCategories",
                column: "ProviderId",
                principalTable: "Providers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProviderCategories_Users_CreatedBy",
                table: "ProviderCategories",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProviderCategories_Users_DeletedBy",
                table: "ProviderCategories",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProviderCategories_Users_UpdatedBy",
                table: "ProviderCategories",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProviderFees_Users_CreatedBy",
                table: "ProviderFees",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProviderFees_Users_DeletedBy",
                table: "ProviderFees",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProviderFees_Users_ProviderId",
                table: "ProviderFees",
                column: "ProviderId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProviderFees_Users_ReviewedByUserId",
                table: "ProviderFees",
                column: "ReviewedByUserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProviderFees_Users_UpdatedBy",
                table: "ProviderFees",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProviderOnboardings_Users_CreatedBy",
                table: "ProviderOnboardings",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProviderOnboardings_Users_DeletedBy",
                table: "ProviderOnboardings",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProviderOnboardings_Users_ReviewedByUserId",
                table: "ProviderOnboardings",
                column: "ReviewedByUserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProviderOnboardings_Users_UpdatedBy",
                table: "ProviderOnboardings",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProviderOnboardings_Users_UserId",
                table: "ProviderOnboardings",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Providers_Users_CreatedBy",
                table: "Providers",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Providers_Users_DeletedBy",
                table: "Providers",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Providers_Users_UpdatedBy",
                table: "Providers",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionnaireTemplates_Users_CreatedBy",
                table: "QuestionnaireTemplates",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionnaireTemplates_Users_DeletedBy",
                table: "QuestionnaireTemplates",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionnaireTemplates_Users_UpdatedBy",
                table: "QuestionnaireTemplates",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionOptions_Questions_QuestionId",
                table: "QuestionOptions",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionOptions_Users_CreatedBy",
                table: "QuestionOptions",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionOptions_Users_DeletedBy",
                table: "QuestionOptions",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionOptions_Users_UpdatedBy",
                table: "QuestionOptions",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Users_CreatedBy",
                table: "Questions",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Users_DeletedBy",
                table: "Questions",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Users_UpdatedBy",
                table: "Questions",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RefundStatuses_Users_CreatedBy",
                table: "RefundStatuses",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RefundStatuses_Users_DeletedBy",
                table: "RefundStatuses",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RefundStatuses_Users_UpdatedBy",
                table: "RefundStatuses",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReminderTimings_Users_CreatedBy",
                table: "ReminderTimings",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReminderTimings_Users_DeletedBy",
                table: "ReminderTimings",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReminderTimings_Users_UpdatedBy",
                table: "ReminderTimings",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReminderTypes_Users_CreatedBy",
                table: "ReminderTypes",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReminderTypes_Users_DeletedBy",
                table: "ReminderTypes",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReminderTypes_Users_UpdatedBy",
                table: "ReminderTypes",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduledPlanMigrations_SubscriptionPlans_FromPlanId",
                table: "ScheduledPlanMigrations",
                column: "FromPlanId",
                principalTable: "SubscriptionPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduledPlanMigrations_SubscriptionPlans_ToPlanId",
                table: "ScheduledPlanMigrations",
                column: "ToPlanId",
                principalTable: "SubscriptionPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduledPlanMigrations_Subscriptions_SubscriptionId",
                table: "ScheduledPlanMigrations",
                column: "SubscriptionId",
                principalTable: "Subscriptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduledPlanMigrations_Users_CreatedBy",
                table: "ScheduledPlanMigrations",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduledPlanMigrations_Users_DeletedBy",
                table: "ScheduledPlanMigrations",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduledPlanMigrations_Users_UpdatedBy",
                table: "ScheduledPlanMigrations",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceConstraints_SubscriptionPlans_SubscriptionPlanId",
                table: "ServiceConstraints",
                column: "SubscriptionPlanId",
                principalTable: "SubscriptionPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceConstraints_Users_CreatedBy",
                table: "ServiceConstraints",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceConstraints_Users_DeletedBy",
                table: "ServiceConstraints",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceConstraints_Users_UpdatedBy",
                table: "ServiceConstraints",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionPayments_Subscriptions_SubscriptionId",
                table: "SubscriptionPayments",
                column: "SubscriptionId",
                principalTable: "Subscriptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionPayments_Users_CreatedBy",
                table: "SubscriptionPayments",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionPayments_Users_DeletedBy",
                table: "SubscriptionPayments",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionPayments_Users_UpdatedBy",
                table: "SubscriptionPayments",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionPlanPrivileges_SubscriptionPlans_SubscriptionPlanId",
                table: "SubscriptionPlanPrivileges",
                column: "SubscriptionPlanId",
                principalTable: "SubscriptionPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionPlanPrivileges_Users_CreatedBy",
                table: "SubscriptionPlanPrivileges",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionPlanPrivileges_Users_DeletedBy",
                table: "SubscriptionPlanPrivileges",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionPlanPrivileges_Users_UpdatedBy",
                table: "SubscriptionPlanPrivileges",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionPlans_Users_CreatedBy",
                table: "SubscriptionPlans",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionPlans_Users_DeletedBy",
                table: "SubscriptionPlans",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionPlans_Users_UpdatedBy",
                table: "SubscriptionPlans",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_Users_CreatedBy",
                table: "Subscriptions",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_Users_DeletedBy",
                table: "Subscriptions",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_Users_UpdatedBy",
                table: "Subscriptions",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_Users_UserId",
                table: "Subscriptions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionStatusHistories_Users_ChangedByUserId",
                table: "SubscriptionStatusHistories",
                column: "ChangedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionStatusHistories_Users_CreatedBy",
                table: "SubscriptionStatusHistories",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionStatusHistories_Users_DeletedBy",
                table: "SubscriptionStatusHistories",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionStatusHistories_Users_UpdatedBy",
                table: "SubscriptionStatusHistories",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SystemSettings_Users_CreatedBy",
                table: "SystemSettings",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SystemSettings_Users_DeletedBy",
                table: "SystemSettings",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SystemSettings_Users_UpdatedBy",
                table: "SystemSettings",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswerOptions_UserAnswers_AnswerId",
                table: "UserAnswerOptions",
                column: "AnswerId",
                principalTable: "UserAnswers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswerOptions_Users_CreatedBy",
                table: "UserAnswerOptions",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswerOptions_Users_DeletedBy",
                table: "UserAnswerOptions",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswerOptions_Users_UpdatedBy",
                table: "UserAnswerOptions",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswers_UserResponses_ResponseId",
                table: "UserAnswers",
                column: "ResponseId",
                principalTable: "UserResponses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswers_Users_CreatedBy",
                table: "UserAnswers",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswers_Users_DeletedBy",
                table: "UserAnswers",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswers_Users_UpdatedBy",
                table: "UserAnswers",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserResponses_Users_CreatedBy",
                table: "UserResponses",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserResponses_Users_DeletedBy",
                table: "UserResponses",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserResponses_Users_UpdatedBy",
                table: "UserResponses",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserResponses_Users_UserId",
                table: "UserResponses",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_Users_CreatedBy",
                table: "UserRoles",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_Users_DeletedBy",
                table: "UserRoles",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_Users_UpdatedBy",
                table: "UserRoles",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_Users_CreatedBy",
                table: "UserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_Users_DeletedBy",
                table: "UserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_Users_UpdatedBy",
                table: "UserRoles");

            migrationBuilder.DropTable(
                name: "AppointmentDocuments");

            migrationBuilder.DropTable(
                name: "AppointmentEvents");

            migrationBuilder.DropTable(
                name: "AppointmentInvitations");

            migrationBuilder.DropTable(
                name: "AppointmentParticipants");

            migrationBuilder.DropTable(
                name: "AppointmentPaymentLogs");

            migrationBuilder.DropTable(
                name: "AppointmentReminders");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "BillingAdjustments");

            migrationBuilder.DropTable(
                name: "CategoryFeeRanges");

            migrationBuilder.DropTable(
                name: "ChatAttachments");

            migrationBuilder.DropTable(
                name: "ChatMessages");

            migrationBuilder.DropTable(
                name: "ChatRoomParticipants");

            migrationBuilder.DropTable(
                name: "DeliveryTracking");

            migrationBuilder.DropTable(
                name: "DocumentReferences");

            migrationBuilder.DropTable(
                name: "MessageAttachments");

            migrationBuilder.DropTable(
                name: "MessageReactions");

            migrationBuilder.DropTable(
                name: "MessageReadReceipts");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "PaymentRefunds");

            migrationBuilder.DropTable(
                name: "PrescriptionItems");

            migrationBuilder.DropTable(
                name: "PrivilegeUsageHistories");

            migrationBuilder.DropTable(
                name: "ProcessedWebhookEvents");

            migrationBuilder.DropTable(
                name: "ProviderCategories");

            migrationBuilder.DropTable(
                name: "ProviderFees");

            migrationBuilder.DropTable(
                name: "ProviderOnboardings");

            migrationBuilder.DropTable(
                name: "ScheduledPlanMigrations");

            migrationBuilder.DropTable(
                name: "ServiceConstraints");

            migrationBuilder.DropTable(
                name: "SubscriptionStatusHistories");

            migrationBuilder.DropTable(
                name: "SystemSettings");

            migrationBuilder.DropTable(
                name: "UserAnswerOptions");

            migrationBuilder.DropTable(
                name: "VideoCallEvents");

            migrationBuilder.DropTable(
                name: "VideoCallParticipants");

            migrationBuilder.DropTable(
                name: "EventTypes");

            migrationBuilder.DropTable(
                name: "InvitationStatuses");

            migrationBuilder.DropTable(
                name: "ParticipantRoles");

            migrationBuilder.DropTable(
                name: "ParticipantStatuses");

            migrationBuilder.DropTable(
                name: "RefundStatuses");

            migrationBuilder.DropTable(
                name: "ReminderTimings");

            migrationBuilder.DropTable(
                name: "ReminderTypes");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "ChatSessions");

            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "SubscriptionPayments");

            migrationBuilder.DropTable(
                name: "Prescriptions");

            migrationBuilder.DropTable(
                name: "UserSubscriptionPrivilegeUsages");

            migrationBuilder.DropTable(
                name: "QuestionOptions");

            migrationBuilder.DropTable(
                name: "UserAnswers");

            migrationBuilder.DropTable(
                name: "VideoCalls");

            migrationBuilder.DropTable(
                name: "DocumentTypes");

            migrationBuilder.DropTable(
                name: "ChatRooms");

            migrationBuilder.DropTable(
                name: "BillingRecords");

            migrationBuilder.DropTable(
                name: "SubscriptionPlanPrivileges");

            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.DropTable(
                name: "UserResponses");

            migrationBuilder.DropTable(
                name: "Appointments");

            migrationBuilder.DropTable(
                name: "MedicationDeliveries");

            migrationBuilder.DropTable(
                name: "Privileges");

            migrationBuilder.DropTable(
                name: "QuestionnaireTemplates");

            migrationBuilder.DropTable(
                name: "AppointmentStatuses");

            migrationBuilder.DropTable(
                name: "AppointmentTypes");

            migrationBuilder.DropTable(
                name: "ConsultationModes");

            migrationBuilder.DropTable(
                name: "PaymentStatuses");

            migrationBuilder.DropTable(
                name: "Consultations");

            migrationBuilder.DropTable(
                name: "MasterPrivilegeTypes");

            migrationBuilder.DropTable(
                name: "Subscriptions");

            migrationBuilder.DropTable(
                name: "HealthAssessments");

            migrationBuilder.DropTable(
                name: "SubscriptionPlans");

            migrationBuilder.DropTable(
                name: "Providers");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "MasterBillingCycles");

            migrationBuilder.DropTable(
                name: "MasterCurrencies");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "UserRoles");
        }
    }
}
