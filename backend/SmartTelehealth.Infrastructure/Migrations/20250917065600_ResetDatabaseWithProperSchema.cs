using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartTelehealth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ResetDatabaseWithProperSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BillingAdjustments_Users_AppliedByUserId",
                table: "BillingAdjustments");

            migrationBuilder.DropForeignKey(
                name: "FK_BillingRecords_MasterCurrencies_CurrencyId",
                table: "BillingRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Subscriptions_SubscriptionId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Privileges_MasterPrivilegeTypes_PrivilegeTypeId",
                table: "Privileges");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionPayments_MasterCurrencies_CurrencyId",
                table: "SubscriptionPayments");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionPayments_Subscriptions_SubscriptionId",
                table: "SubscriptionPayments");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionPlanPrivileges_MasterBillingCycles_UsagePeriodId",
                table: "SubscriptionPlanPrivileges");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionStatusHistories_Users_ChangedByUserId",
                table: "SubscriptionStatusHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSubscriptionPrivilegeUsages_Privileges_PrivilegeId",
                table: "UserSubscriptionPrivilegeUsages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_SubscriptionId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_BillingAdjustments_AppliedByUserId",
                table: "BillingAdjustments");

            migrationBuilder.DropColumn(
                name: "SubscriptionId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "AppliedByUserId",
                table: "BillingAdjustments");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "VideoCalls",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "VideoCalls",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "VideoCalls",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "VideoCallParticipants",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "VideoCallParticipants",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "VideoCallParticipants",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "VideoCallEvents",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "VideoCallEvents",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "VideoCallEvents",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "PrivilegeId",
                table: "UserSubscriptionPrivilegeUsages",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "UserSubscriptionPrivilegeUsages",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "UserSubscriptionPrivilegeUsages",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "UserSubscriptionPrivilegeUsages",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PrivilegeId1",
                table: "UserSubscriptionPrivilegeUsages",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PasswordResetToken",
                table: "Users",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Users",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<int>(
                name: "SortOrder",
                table: "UserRoles",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "UserRoles",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "UserRoles",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "UserResponses",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "UserResponses",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "UserResponses",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "UserAnswers",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "UserAnswers",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "UserAnswers",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "UserAnswerOptions",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "UserAnswerOptions",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "UserAnswerOptions",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "SubscriptionStatusHistories",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "SubscriptionStatusHistories",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "SubscriptionStatusHistories",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TrialDurationInDays",
                table: "Subscriptions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "TotalUsageCount",
                table: "Subscriptions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<bool>(
                name: "IsTrialSubscription",
                table: "Subscriptions",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Subscriptions",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Subscriptions",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<int>(
                name: "FailedPaymentAttempts",
                table: "Subscriptions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "Subscriptions",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TrialDurationInDays",
                table: "SubscriptionPlans",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "MessagingCount",
                table: "SubscriptionPlans",
                type: "int",
                nullable: false,
                defaultValue: 10,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "MaxPauseDurationDays",
                table: "SubscriptionPlans",
                type: "int",
                nullable: false,
                defaultValue: 90,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<bool>(
                name: "IsTrialAllowed",
                table: "SubscriptionPlans",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsTrending",
                table: "SubscriptionPlans",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsMostPopular",
                table: "SubscriptionPlans",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsFeatured",
                table: "SubscriptionPlans",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "SubscriptionPlans",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IncludesMedicationDelivery",
                table: "SubscriptionPlans",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IncludesFollowUpCare",
                table: "SubscriptionPlans",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<int>(
                name: "DisplayOrder",
                table: "SubscriptionPlans",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "DeliveryFrequencyDays",
                table: "SubscriptionPlans",
                type: "int",
                nullable: false,
                defaultValue: 30,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "SubscriptionPlans",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlanType",
                table: "SubscriptionPlans",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "SubscriptionPlanPrivileges",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "SubscriptionPlanPrivileges",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<int>(
                name: "DurationMonths",
                table: "SubscriptionPlanPrivileges",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "SubscriptionPlanPrivileges",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitCost",
                table: "SubscriptionPlanPrivileges",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "SubscriptionPayments",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "SubscriptionPayments",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "SubscriptionPayments",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "SubscriptionPayments",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "SubscriptionPayments",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "AttemptCount",
                table: "SubscriptionPayments",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "ServiceConstraints",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "ServiceConstraints",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "ServiceConstraints",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SortOrder",
                table: "ReminderTypes",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "ReminderTypes",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "ReminderTypes",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SortOrder",
                table: "ReminderTimings",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "MinutesBeforeAppointment",
                table: "ReminderTimings",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "ReminderTimings",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "ReminderTimings",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "RefundStatuses",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "RefundStatuses",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Questions",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Questions",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "Questions",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "QuestionOptions",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "QuestionOptions",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "QuestionOptions",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "QuestionnaireTemplates",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "QuestionnaireTemplates",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "QuestionnaireTemplates",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Providers",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "Providers",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "ProviderOnboardings",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "ProviderOnboardings",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "ProviderOnboardings",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "ProviderFees",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "ProviderFees",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "ProviderFees",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "ProviderCategories",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "ProviderCategories",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "ProviderCategories",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "PrivilegeUsageHistories",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "PrivilegeUsageHistories",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "PrivilegeUsageHistories",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Privileges",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Privileges",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "Privileges",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MasterPrivilegeTypeId",
                table: "Privileges",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Prescriptions",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Prescriptions",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "Prescriptions",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "PrescriptionItems",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "PrescriptionItems",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "PrescriptionItems",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "PaymentStatuses",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "PaymentStatuses",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "PaymentRefunds",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "PaymentRefunds",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "PaymentRefunds",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SortOrder",
                table: "ParticipantStatuses",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "ParticipantStatuses",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "ParticipantStatuses",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SortOrder",
                table: "ParticipantRoles",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "ParticipantRoles",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "ParticipantRoles",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Notifications",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Notifications",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "Notifications",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Messages",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Messages",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "Messages",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "MessageReadReceipts",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "MessageReadReceipts",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "MessageReadReceipts",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "MessageReactions",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "MessageReactions",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "MessageReactions",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "MessageAttachments",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "MessageAttachments",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "MessageAttachments",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "MedicationDeliveries",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "MedicationDeliveries",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "MedicationDeliveries",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "MasterPrivilegeTypes",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "MasterPrivilegeTypes",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "MasterCurrencies",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "MasterCurrencies",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "MasterBillingCycles",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "MasterBillingCycles",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SortOrder",
                table: "InvitationStatuses",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "InvitationStatuses",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "InvitationStatuses",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "HealthAssessments",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "HealthAssessments",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "HealthAssessments",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SortOrder",
                table: "EventTypes",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "EventTypes",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "EventTypes",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "DocumentTypes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "DocumentTypes",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<int>(
                name: "DisplayOrder",
                table: "DocumentTypes",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "DocumentTypes",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "DocumentTypes",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Documents",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Documents",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "Documents",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "DocumentReferences",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "DocumentReferences",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "DocumentReferences",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "DeliveryTracking",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "DeliveryTracking",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "DeliveryTracking",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Consultations",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Consultations",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "Consultations",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SortOrder",
                table: "ConsultationModes",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "ConsultationModes",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "ConsultationModes",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "ChatSessions",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "ChatSessions",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "ChatSessions",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "ChatRooms",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "ChatRooms",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "ChatRooms",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "ChatRoomParticipants",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "ChatRoomParticipants",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "ChatRoomParticipants",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "ChatMessages",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "ChatMessages",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "ChatMessages",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "ChatAttachments",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "ChatAttachments",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "ChatAttachments",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "CategoryFeeRanges",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "CategoryFeeRanges",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "CategoryFeeRanges",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "OneTimeConsultationDurationMinutes",
                table: "Categories",
                type: "int",
                nullable: false,
                defaultValue: 30,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<bool>(
                name: "IsTrending",
                table: "Categories",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsMostPopular",
                table: "Categories",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Categories",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<int>(
                name: "DisplayOrder",
                table: "Categories",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "Categories",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Color",
                table: "Categories",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "AllowsOneTimeConsultation",
                table: "Categories",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "BillingRecords",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "BillingRecords",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "PaymentIntentId",
                table: "BillingRecords",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "BillingRecords",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "BillingRecords",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "BillingRecords",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MasterCurrencyId",
                table: "BillingRecords",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "BillingAdjustments",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "BillingAdjustments",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "BillingAdjustments",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "BillingAdjustments",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SortOrder",
                table: "AppointmentTypes",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "AppointmentTypes",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "AppointmentTypes",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SortOrder",
                table: "AppointmentStatuses",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "AppointmentStatuses",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "AppointmentStatuses",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Appointments",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Appointments",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "Appointments",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "AppointmentReminders",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "AppointmentReminders",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "AppointmentPaymentLogs",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "AppointmentPaymentLogs",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "AppointmentParticipants",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "AppointmentParticipants",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "AppointmentInvitations",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "AppointmentInvitations",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "AppointmentInvitations",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "AppointmentEvents",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "AppointmentEvents",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "AppointmentDocuments",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "AppointmentDocuments",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "AppointmentDocuments",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_VideoCallParticipants_CreatedBy",
                table: "VideoCallParticipants",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_VideoCallParticipants_DeletedBy",
                table: "VideoCallParticipants",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_VideoCallParticipants_UpdatedBy",
                table: "VideoCallParticipants",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_VideoCallEvents_CreatedBy",
                table: "VideoCallEvents",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_VideoCallEvents_DeletedBy",
                table: "VideoCallEvents",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_VideoCallEvents_UpdatedBy",
                table: "VideoCallEvents",
                column: "UpdatedBy");

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
                name: "IX_UserSubscriptionPrivilegeUsages_PrivilegeId1",
                table: "UserSubscriptionPrivilegeUsages",
                column: "PrivilegeId1");

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
                name: "IX_UserResponses_CreatedBy",
                table: "UserResponses",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_UserResponses_DeletedBy",
                table: "UserResponses",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_UserResponses_UpdatedBy",
                table: "UserResponses",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswers_CreatedBy",
                table: "UserAnswers",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswers_DeletedBy",
                table: "UserAnswers",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswers_UpdatedBy",
                table: "UserAnswers",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswerOptions_CreatedBy",
                table: "UserAnswerOptions",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswerOptions_DeletedBy",
                table: "UserAnswerOptions",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswerOptions_UpdatedBy",
                table: "UserAnswerOptions",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionStatusHistories_ChangedAt",
                table: "SubscriptionStatusHistories",
                column: "ChangedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionStatusHistories_CreatedBy",
                table: "SubscriptionStatusHistories",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionStatusHistories_DeletedBy",
                table: "SubscriptionStatusHistories",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionStatusHistories_ToStatus",
                table: "SubscriptionStatusHistories",
                column: "ToStatus");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionStatusHistories_UpdatedBy",
                table: "SubscriptionStatusHistories",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_AutoRenew",
                table: "Subscriptions",
                column: "AutoRenew");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_CreatedBy",
                table: "Subscriptions",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_DeletedBy",
                table: "Subscriptions",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_IsTrialSubscription",
                table: "Subscriptions",
                column: "IsTrialSubscription");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_NextBillingDate",
                table: "Subscriptions",
                column: "NextBillingDate");

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
                name: "IX_Subscriptions_UpdatedBy",
                table: "Subscriptions",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlans_CreatedBy",
                table: "SubscriptionPlans",
                column: "CreatedBy");

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
                name: "IX_SubscriptionPlans_Name",
                table: "SubscriptionPlans",
                column: "Name");

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
                name: "IX_SubscriptionPlanPrivileges_UpdatedBy",
                table: "SubscriptionPlanPrivileges",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPayments_CreatedBy",
                table: "SubscriptionPayments",
                column: "CreatedBy");

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
                name: "IX_SubscriptionPayments_Type",
                table: "SubscriptionPayments",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPayments_UpdatedBy",
                table: "SubscriptionPayments",
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
                name: "IX_ServiceConstraints_UpdatedBy",
                table: "ServiceConstraints",
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
                name: "IX_Questions_CreatedBy",
                table: "Questions",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_DeletedBy",
                table: "Questions",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_UpdatedBy",
                table: "Questions",
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
                name: "IX_QuestionOptions_UpdatedBy",
                table: "QuestionOptions",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireTemplates_CreatedBy",
                table: "QuestionnaireTemplates",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireTemplates_DeletedBy",
                table: "QuestionnaireTemplates",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireTemplates_UpdatedBy",
                table: "QuestionnaireTemplates",
                column: "UpdatedBy");

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
                name: "IX_ProviderOnboardings_CreatedBy",
                table: "ProviderOnboardings",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderOnboardings_DeletedBy",
                table: "ProviderOnboardings",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderOnboardings_UpdatedBy",
                table: "ProviderOnboardings",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderFees_CreatedBy",
                table: "ProviderFees",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderFees_DeletedBy",
                table: "ProviderFees",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderFees_UpdatedBy",
                table: "ProviderFees",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderCategories_CreatedBy",
                table: "ProviderCategories",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderCategories_DeletedBy",
                table: "ProviderCategories",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderCategories_UpdatedBy",
                table: "ProviderCategories",
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
                name: "IX_Privileges_UpdatedBy",
                table: "Privileges",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_CreatedBy",
                table: "Prescriptions",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_DeletedBy",
                table: "Prescriptions",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_UpdatedBy",
                table: "Prescriptions",
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
                name: "IX_PrescriptionItems_UpdatedBy",
                table: "PrescriptionItems",
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
                name: "IX_PaymentRefunds_CreatedBy",
                table: "PaymentRefunds",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRefunds_DeletedBy",
                table: "PaymentRefunds",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRefunds_RefundedAt",
                table: "PaymentRefunds",
                column: "RefundedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRefunds_StripeRefundId",
                table: "PaymentRefunds",
                column: "StripeRefundId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRefunds_UpdatedBy",
                table: "PaymentRefunds",
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
                name: "IX_Messages_CreatedBy",
                table: "Messages",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_DeletedBy",
                table: "Messages",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_UpdatedBy",
                table: "Messages",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MessageReadReceipts_CreatedBy",
                table: "MessageReadReceipts",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MessageReadReceipts_DeletedBy",
                table: "MessageReadReceipts",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MessageReadReceipts_UpdatedBy",
                table: "MessageReadReceipts",
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
                name: "IX_MessageReactions_UpdatedBy",
                table: "MessageReactions",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MessageAttachments_CreatedBy",
                table: "MessageAttachments",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MessageAttachments_DeletedBy",
                table: "MessageAttachments",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MessageAttachments_UpdatedBy",
                table: "MessageAttachments",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MedicationDeliveries_CreatedBy",
                table: "MedicationDeliveries",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MedicationDeliveries_DeletedBy",
                table: "MedicationDeliveries",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MedicationDeliveries_UpdatedBy",
                table: "MedicationDeliveries",
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
                name: "IX_HealthAssessments_CreatedBy",
                table: "HealthAssessments",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_HealthAssessments_DeletedBy",
                table: "HealthAssessments",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_HealthAssessments_UpdatedBy",
                table: "HealthAssessments",
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
                name: "IX_Documents_UpdatedBy",
                table: "Documents",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentReferences_DeletedBy",
                table: "DocumentReferences",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentReferences_UpdatedBy",
                table: "DocumentReferences",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryTracking_CreatedBy",
                table: "DeliveryTracking",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryTracking_DeletedBy",
                table: "DeliveryTracking",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryTracking_UpdatedBy",
                table: "DeliveryTracking",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Consultations_CreatedBy",
                table: "Consultations",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Consultations_DeletedBy",
                table: "Consultations",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Consultations_UpdatedBy",
                table: "Consultations",
                column: "UpdatedBy");

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
                name: "IX_ChatSessions_CreatedBy",
                table: "ChatSessions",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ChatSessions_DeletedBy",
                table: "ChatSessions",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ChatSessions_UpdatedBy",
                table: "ChatSessions",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRooms_CreatedBy",
                table: "ChatRooms",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRooms_DeletedBy",
                table: "ChatRooms",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRooms_UpdatedBy",
                table: "ChatRooms",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRoomParticipants_CreatedBy",
                table: "ChatRoomParticipants",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRoomParticipants_DeletedBy",
                table: "ChatRoomParticipants",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRoomParticipants_UpdatedBy",
                table: "ChatRoomParticipants",
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
                name: "IX_ChatMessages_UpdatedBy",
                table: "ChatMessages",
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
                name: "IX_ChatAttachments_UpdatedBy",
                table: "ChatAttachments",
                column: "UpdatedBy");

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
                name: "IX_BillingRecords_BillingCycleId",
                table: "BillingRecords",
                column: "BillingCycleId");

            migrationBuilder.CreateIndex(
                name: "IX_BillingRecords_BillingDate",
                table: "BillingRecords",
                column: "BillingDate");

            migrationBuilder.CreateIndex(
                name: "IX_BillingRecords_CreatedBy",
                table: "BillingRecords",
                column: "CreatedBy");

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
                name: "IX_BillingRecords_Type",
                table: "BillingRecords",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_BillingRecords_UpdatedBy",
                table: "BillingRecords",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_BillingAdjustments_AppliedAt",
                table: "BillingAdjustments",
                column: "AppliedAt");

            migrationBuilder.CreateIndex(
                name: "IX_BillingAdjustments_AppliedBy",
                table: "BillingAdjustments",
                column: "AppliedBy");

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
                name: "IX_Appointments_CreatedBy",
                table: "Appointments",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_DeletedBy",
                table: "Appointments",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_UpdatedBy",
                table: "Appointments",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentReminders_CreatedBy",
                table: "AppointmentReminders",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentReminders_DeletedBy",
                table: "AppointmentReminders",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentReminders_UpdatedBy",
                table: "AppointmentReminders",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentPaymentLogs_CreatedBy",
                table: "AppointmentPaymentLogs",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentPaymentLogs_DeletedBy",
                table: "AppointmentPaymentLogs",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentPaymentLogs_UpdatedBy",
                table: "AppointmentPaymentLogs",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentParticipants_CreatedBy",
                table: "AppointmentParticipants",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentParticipants_DeletedBy",
                table: "AppointmentParticipants",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentParticipants_UpdatedBy",
                table: "AppointmentParticipants",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentInvitations_CreatedBy",
                table: "AppointmentInvitations",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentInvitations_DeletedBy",
                table: "AppointmentInvitations",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentInvitations_UpdatedBy",
                table: "AppointmentInvitations",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentEvents_CreatedBy",
                table: "AppointmentEvents",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentEvents_DeletedBy",
                table: "AppointmentEvents",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentEvents_UpdatedBy",
                table: "AppointmentEvents",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentDocuments_CreatedBy",
                table: "AppointmentDocuments",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentDocuments_DeletedBy",
                table: "AppointmentDocuments",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentDocuments_UpdatedBy",
                table: "AppointmentDocuments",
                column: "UpdatedBy");

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
                name: "FK_AppointmentInvitations_Users_UpdatedBy",
                table: "AppointmentInvitations",
                column: "UpdatedBy",
                principalTable: "Users",
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
                name: "FK_AppointmentParticipants_Users_UpdatedBy",
                table: "AppointmentParticipants",
                column: "UpdatedBy",
                principalTable: "Users",
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
                name: "FK_ChatRooms_Users_UpdatedBy",
                table: "ChatRooms",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

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
                name: "FK_Messages_Users_UpdatedBy",
                table: "Messages",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

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
                name: "FK_Privileges_MasterPrivilegeTypes_MasterPrivilegeTypeId",
                table: "Privileges",
                column: "MasterPrivilegeTypeId",
                principalTable: "MasterPrivilegeTypes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Privileges_MasterPrivilegeTypes_PrivilegeTypeId",
                table: "Privileges",
                column: "PrivilegeTypeId",
                principalTable: "MasterPrivilegeTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

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
                name: "FK_ProviderOnboardings_Users_UpdatedBy",
                table: "ProviderOnboardings",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

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
                name: "FK_SubscriptionPayments_MasterCurrencies_CurrencyId",
                table: "SubscriptionPayments",
                column: "CurrencyId",
                principalTable: "MasterCurrencies",
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
                name: "FK_SubscriptionPlanPrivileges_MasterBillingCycles_UsagePeriodId",
                table: "SubscriptionPlanPrivileges",
                column: "UsagePeriodId",
                principalTable: "MasterBillingCycles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

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

            migrationBuilder.AddForeignKey(
                name: "FK_UserSubscriptionPrivilegeUsages_Privileges_PrivilegeId",
                table: "UserSubscriptionPrivilegeUsages",
                column: "PrivilegeId",
                principalTable: "Privileges",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSubscriptionPrivilegeUsages_Privileges_PrivilegeId1",
                table: "UserSubscriptionPrivilegeUsages",
                column: "PrivilegeId1",
                principalTable: "Privileges",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserSubscriptionPrivilegeUsages_Users_CreatedBy",
                table: "UserSubscriptionPrivilegeUsages",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSubscriptionPrivilegeUsages_Users_DeletedBy",
                table: "UserSubscriptionPrivilegeUsages",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSubscriptionPrivilegeUsages_Users_UpdatedBy",
                table: "UserSubscriptionPrivilegeUsages",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VideoCallEvents_Users_CreatedBy",
                table: "VideoCallEvents",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VideoCallEvents_Users_DeletedBy",
                table: "VideoCallEvents",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VideoCallEvents_Users_UpdatedBy",
                table: "VideoCallEvents",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VideoCallParticipants_Users_CreatedBy",
                table: "VideoCallParticipants",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VideoCallParticipants_Users_DeletedBy",
                table: "VideoCallParticipants",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VideoCallParticipants_Users_UpdatedBy",
                table: "VideoCallParticipants",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VideoCalls_Users_CreatedBy",
                table: "VideoCalls",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VideoCalls_Users_DeletedBy",
                table: "VideoCalls",
                column: "DeletedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VideoCalls_Users_UpdatedBy",
                table: "VideoCalls",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppointmentDocuments_Users_CreatedBy",
                table: "AppointmentDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_AppointmentDocuments_Users_DeletedBy",
                table: "AppointmentDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_AppointmentDocuments_Users_UpdatedBy",
                table: "AppointmentDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_AppointmentEvents_Users_CreatedBy",
                table: "AppointmentEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_AppointmentEvents_Users_DeletedBy",
                table: "AppointmentEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_AppointmentEvents_Users_UpdatedBy",
                table: "AppointmentEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_AppointmentInvitations_Users_CreatedBy",
                table: "AppointmentInvitations");

            migrationBuilder.DropForeignKey(
                name: "FK_AppointmentInvitations_Users_DeletedBy",
                table: "AppointmentInvitations");

            migrationBuilder.DropForeignKey(
                name: "FK_AppointmentInvitations_Users_UpdatedBy",
                table: "AppointmentInvitations");

            migrationBuilder.DropForeignKey(
                name: "FK_AppointmentParticipants_Users_CreatedBy",
                table: "AppointmentParticipants");

            migrationBuilder.DropForeignKey(
                name: "FK_AppointmentParticipants_Users_DeletedBy",
                table: "AppointmentParticipants");

            migrationBuilder.DropForeignKey(
                name: "FK_AppointmentParticipants_Users_UpdatedBy",
                table: "AppointmentParticipants");

            migrationBuilder.DropForeignKey(
                name: "FK_AppointmentPaymentLogs_Users_CreatedBy",
                table: "AppointmentPaymentLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_AppointmentPaymentLogs_Users_DeletedBy",
                table: "AppointmentPaymentLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_AppointmentPaymentLogs_Users_UpdatedBy",
                table: "AppointmentPaymentLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_AppointmentReminders_Users_CreatedBy",
                table: "AppointmentReminders");

            migrationBuilder.DropForeignKey(
                name: "FK_AppointmentReminders_Users_DeletedBy",
                table: "AppointmentReminders");

            migrationBuilder.DropForeignKey(
                name: "FK_AppointmentReminders_Users_UpdatedBy",
                table: "AppointmentReminders");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Users_CreatedBy",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Users_DeletedBy",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Users_UpdatedBy",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_AppointmentStatuses_Users_CreatedBy",
                table: "AppointmentStatuses");

            migrationBuilder.DropForeignKey(
                name: "FK_AppointmentStatuses_Users_DeletedBy",
                table: "AppointmentStatuses");

            migrationBuilder.DropForeignKey(
                name: "FK_AppointmentStatuses_Users_UpdatedBy",
                table: "AppointmentStatuses");

            migrationBuilder.DropForeignKey(
                name: "FK_AppointmentTypes_Users_CreatedBy",
                table: "AppointmentTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_AppointmentTypes_Users_DeletedBy",
                table: "AppointmentTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_AppointmentTypes_Users_UpdatedBy",
                table: "AppointmentTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_BillingAdjustments_Users_AppliedBy",
                table: "BillingAdjustments");

            migrationBuilder.DropForeignKey(
                name: "FK_BillingAdjustments_Users_CreatedBy",
                table: "BillingAdjustments");

            migrationBuilder.DropForeignKey(
                name: "FK_BillingAdjustments_Users_DeletedBy",
                table: "BillingAdjustments");

            migrationBuilder.DropForeignKey(
                name: "FK_BillingAdjustments_Users_UpdatedBy",
                table: "BillingAdjustments");

            migrationBuilder.DropForeignKey(
                name: "FK_BillingRecords_MasterCurrencies_CurrencyId",
                table: "BillingRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_BillingRecords_MasterCurrencies_MasterCurrencyId",
                table: "BillingRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_BillingRecords_Users_CreatedBy",
                table: "BillingRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_BillingRecords_Users_DeletedBy",
                table: "BillingRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_BillingRecords_Users_UpdatedBy",
                table: "BillingRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Users_CreatedBy",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Users_DeletedBy",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Users_UpdatedBy",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_CategoryFeeRanges_Users_CreatedBy",
                table: "CategoryFeeRanges");

            migrationBuilder.DropForeignKey(
                name: "FK_CategoryFeeRanges_Users_DeletedBy",
                table: "CategoryFeeRanges");

            migrationBuilder.DropForeignKey(
                name: "FK_CategoryFeeRanges_Users_UpdatedBy",
                table: "CategoryFeeRanges");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatAttachments_Users_CreatedBy",
                table: "ChatAttachments");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatAttachments_Users_DeletedBy",
                table: "ChatAttachments");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatAttachments_Users_UpdatedBy",
                table: "ChatAttachments");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_Users_CreatedBy",
                table: "ChatMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_Users_DeletedBy",
                table: "ChatMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_Users_UpdatedBy",
                table: "ChatMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatRoomParticipants_Users_CreatedBy",
                table: "ChatRoomParticipants");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatRoomParticipants_Users_DeletedBy",
                table: "ChatRoomParticipants");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatRoomParticipants_Users_UpdatedBy",
                table: "ChatRoomParticipants");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatRooms_Users_CreatedBy",
                table: "ChatRooms");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatRooms_Users_DeletedBy",
                table: "ChatRooms");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatRooms_Users_UpdatedBy",
                table: "ChatRooms");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatSessions_Users_CreatedBy",
                table: "ChatSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatSessions_Users_DeletedBy",
                table: "ChatSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatSessions_Users_UpdatedBy",
                table: "ChatSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_ConsultationModes_Users_CreatedBy",
                table: "ConsultationModes");

            migrationBuilder.DropForeignKey(
                name: "FK_ConsultationModes_Users_DeletedBy",
                table: "ConsultationModes");

            migrationBuilder.DropForeignKey(
                name: "FK_ConsultationModes_Users_UpdatedBy",
                table: "ConsultationModes");

            migrationBuilder.DropForeignKey(
                name: "FK_Consultations_Users_CreatedBy",
                table: "Consultations");

            migrationBuilder.DropForeignKey(
                name: "FK_Consultations_Users_DeletedBy",
                table: "Consultations");

            migrationBuilder.DropForeignKey(
                name: "FK_Consultations_Users_UpdatedBy",
                table: "Consultations");

            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryTracking_Users_CreatedBy",
                table: "DeliveryTracking");

            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryTracking_Users_DeletedBy",
                table: "DeliveryTracking");

            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryTracking_Users_UpdatedBy",
                table: "DeliveryTracking");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentReferences_Users_CreatedBy",
                table: "DocumentReferences");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentReferences_Users_DeletedBy",
                table: "DocumentReferences");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentReferences_Users_UpdatedBy",
                table: "DocumentReferences");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Users_CreatedBy",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Users_DeletedBy",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Users_UpdatedBy",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentTypes_Users_CreatedBy",
                table: "DocumentTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentTypes_Users_DeletedBy",
                table: "DocumentTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentTypes_Users_UpdatedBy",
                table: "DocumentTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_EventTypes_Users_CreatedBy",
                table: "EventTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_EventTypes_Users_DeletedBy",
                table: "EventTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_EventTypes_Users_UpdatedBy",
                table: "EventTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_HealthAssessments_Users_CreatedBy",
                table: "HealthAssessments");

            migrationBuilder.DropForeignKey(
                name: "FK_HealthAssessments_Users_DeletedBy",
                table: "HealthAssessments");

            migrationBuilder.DropForeignKey(
                name: "FK_HealthAssessments_Users_UpdatedBy",
                table: "HealthAssessments");

            migrationBuilder.DropForeignKey(
                name: "FK_InvitationStatuses_Users_CreatedBy",
                table: "InvitationStatuses");

            migrationBuilder.DropForeignKey(
                name: "FK_InvitationStatuses_Users_DeletedBy",
                table: "InvitationStatuses");

            migrationBuilder.DropForeignKey(
                name: "FK_InvitationStatuses_Users_UpdatedBy",
                table: "InvitationStatuses");

            migrationBuilder.DropForeignKey(
                name: "FK_MasterBillingCycles_Users_CreatedBy",
                table: "MasterBillingCycles");

            migrationBuilder.DropForeignKey(
                name: "FK_MasterBillingCycles_Users_DeletedBy",
                table: "MasterBillingCycles");

            migrationBuilder.DropForeignKey(
                name: "FK_MasterBillingCycles_Users_UpdatedBy",
                table: "MasterBillingCycles");

            migrationBuilder.DropForeignKey(
                name: "FK_MasterCurrencies_Users_CreatedBy",
                table: "MasterCurrencies");

            migrationBuilder.DropForeignKey(
                name: "FK_MasterCurrencies_Users_DeletedBy",
                table: "MasterCurrencies");

            migrationBuilder.DropForeignKey(
                name: "FK_MasterCurrencies_Users_UpdatedBy",
                table: "MasterCurrencies");

            migrationBuilder.DropForeignKey(
                name: "FK_MasterPrivilegeTypes_Users_CreatedBy",
                table: "MasterPrivilegeTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_MasterPrivilegeTypes_Users_DeletedBy",
                table: "MasterPrivilegeTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_MasterPrivilegeTypes_Users_UpdatedBy",
                table: "MasterPrivilegeTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicationDeliveries_Users_CreatedBy",
                table: "MedicationDeliveries");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicationDeliveries_Users_DeletedBy",
                table: "MedicationDeliveries");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicationDeliveries_Users_UpdatedBy",
                table: "MedicationDeliveries");

            migrationBuilder.DropForeignKey(
                name: "FK_MessageAttachments_Users_CreatedBy",
                table: "MessageAttachments");

            migrationBuilder.DropForeignKey(
                name: "FK_MessageAttachments_Users_DeletedBy",
                table: "MessageAttachments");

            migrationBuilder.DropForeignKey(
                name: "FK_MessageAttachments_Users_UpdatedBy",
                table: "MessageAttachments");

            migrationBuilder.DropForeignKey(
                name: "FK_MessageReactions_Users_CreatedBy",
                table: "MessageReactions");

            migrationBuilder.DropForeignKey(
                name: "FK_MessageReactions_Users_DeletedBy",
                table: "MessageReactions");

            migrationBuilder.DropForeignKey(
                name: "FK_MessageReactions_Users_UpdatedBy",
                table: "MessageReactions");

            migrationBuilder.DropForeignKey(
                name: "FK_MessageReadReceipts_Users_CreatedBy",
                table: "MessageReadReceipts");

            migrationBuilder.DropForeignKey(
                name: "FK_MessageReadReceipts_Users_DeletedBy",
                table: "MessageReadReceipts");

            migrationBuilder.DropForeignKey(
                name: "FK_MessageReadReceipts_Users_UpdatedBy",
                table: "MessageReadReceipts");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Users_CreatedBy",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Users_DeletedBy",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Users_UpdatedBy",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Users_CreatedBy",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Users_DeletedBy",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Users_UpdatedBy",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_ParticipantRoles_Users_CreatedBy",
                table: "ParticipantRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_ParticipantRoles_Users_DeletedBy",
                table: "ParticipantRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_ParticipantRoles_Users_UpdatedBy",
                table: "ParticipantRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_ParticipantStatuses_Users_CreatedBy",
                table: "ParticipantStatuses");

            migrationBuilder.DropForeignKey(
                name: "FK_ParticipantStatuses_Users_DeletedBy",
                table: "ParticipantStatuses");

            migrationBuilder.DropForeignKey(
                name: "FK_ParticipantStatuses_Users_UpdatedBy",
                table: "ParticipantStatuses");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentRefunds_Users_CreatedBy",
                table: "PaymentRefunds");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentRefunds_Users_DeletedBy",
                table: "PaymentRefunds");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentRefunds_Users_UpdatedBy",
                table: "PaymentRefunds");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentStatuses_Users_CreatedBy",
                table: "PaymentStatuses");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentStatuses_Users_DeletedBy",
                table: "PaymentStatuses");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentStatuses_Users_UpdatedBy",
                table: "PaymentStatuses");

            migrationBuilder.DropForeignKey(
                name: "FK_PrescriptionItems_Users_CreatedBy",
                table: "PrescriptionItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PrescriptionItems_Users_DeletedBy",
                table: "PrescriptionItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PrescriptionItems_Users_UpdatedBy",
                table: "PrescriptionItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Prescriptions_Users_CreatedBy",
                table: "Prescriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Prescriptions_Users_DeletedBy",
                table: "Prescriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Prescriptions_Users_UpdatedBy",
                table: "Prescriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Privileges_MasterPrivilegeTypes_MasterPrivilegeTypeId",
                table: "Privileges");

            migrationBuilder.DropForeignKey(
                name: "FK_Privileges_MasterPrivilegeTypes_PrivilegeTypeId",
                table: "Privileges");

            migrationBuilder.DropForeignKey(
                name: "FK_Privileges_Users_CreatedBy",
                table: "Privileges");

            migrationBuilder.DropForeignKey(
                name: "FK_Privileges_Users_DeletedBy",
                table: "Privileges");

            migrationBuilder.DropForeignKey(
                name: "FK_Privileges_Users_UpdatedBy",
                table: "Privileges");

            migrationBuilder.DropForeignKey(
                name: "FK_PrivilegeUsageHistories_Users_CreatedBy",
                table: "PrivilegeUsageHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_PrivilegeUsageHistories_Users_DeletedBy",
                table: "PrivilegeUsageHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_PrivilegeUsageHistories_Users_UpdatedBy",
                table: "PrivilegeUsageHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_ProviderCategories_Users_CreatedBy",
                table: "ProviderCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_ProviderCategories_Users_DeletedBy",
                table: "ProviderCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_ProviderCategories_Users_UpdatedBy",
                table: "ProviderCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_ProviderFees_Users_CreatedBy",
                table: "ProviderFees");

            migrationBuilder.DropForeignKey(
                name: "FK_ProviderFees_Users_DeletedBy",
                table: "ProviderFees");

            migrationBuilder.DropForeignKey(
                name: "FK_ProviderFees_Users_UpdatedBy",
                table: "ProviderFees");

            migrationBuilder.DropForeignKey(
                name: "FK_ProviderOnboardings_Users_CreatedBy",
                table: "ProviderOnboardings");

            migrationBuilder.DropForeignKey(
                name: "FK_ProviderOnboardings_Users_DeletedBy",
                table: "ProviderOnboardings");

            migrationBuilder.DropForeignKey(
                name: "FK_ProviderOnboardings_Users_UpdatedBy",
                table: "ProviderOnboardings");

            migrationBuilder.DropForeignKey(
                name: "FK_Providers_Users_CreatedBy",
                table: "Providers");

            migrationBuilder.DropForeignKey(
                name: "FK_Providers_Users_DeletedBy",
                table: "Providers");

            migrationBuilder.DropForeignKey(
                name: "FK_Providers_Users_UpdatedBy",
                table: "Providers");

            migrationBuilder.DropForeignKey(
                name: "FK_QuestionnaireTemplates_Users_CreatedBy",
                table: "QuestionnaireTemplates");

            migrationBuilder.DropForeignKey(
                name: "FK_QuestionnaireTemplates_Users_DeletedBy",
                table: "QuestionnaireTemplates");

            migrationBuilder.DropForeignKey(
                name: "FK_QuestionnaireTemplates_Users_UpdatedBy",
                table: "QuestionnaireTemplates");

            migrationBuilder.DropForeignKey(
                name: "FK_QuestionOptions_Users_CreatedBy",
                table: "QuestionOptions");

            migrationBuilder.DropForeignKey(
                name: "FK_QuestionOptions_Users_DeletedBy",
                table: "QuestionOptions");

            migrationBuilder.DropForeignKey(
                name: "FK_QuestionOptions_Users_UpdatedBy",
                table: "QuestionOptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Users_CreatedBy",
                table: "Questions");

            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Users_DeletedBy",
                table: "Questions");

            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Users_UpdatedBy",
                table: "Questions");

            migrationBuilder.DropForeignKey(
                name: "FK_RefundStatuses_Users_CreatedBy",
                table: "RefundStatuses");

            migrationBuilder.DropForeignKey(
                name: "FK_RefundStatuses_Users_DeletedBy",
                table: "RefundStatuses");

            migrationBuilder.DropForeignKey(
                name: "FK_RefundStatuses_Users_UpdatedBy",
                table: "RefundStatuses");

            migrationBuilder.DropForeignKey(
                name: "FK_ReminderTimings_Users_CreatedBy",
                table: "ReminderTimings");

            migrationBuilder.DropForeignKey(
                name: "FK_ReminderTimings_Users_DeletedBy",
                table: "ReminderTimings");

            migrationBuilder.DropForeignKey(
                name: "FK_ReminderTimings_Users_UpdatedBy",
                table: "ReminderTimings");

            migrationBuilder.DropForeignKey(
                name: "FK_ReminderTypes_Users_CreatedBy",
                table: "ReminderTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_ReminderTypes_Users_DeletedBy",
                table: "ReminderTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_ReminderTypes_Users_UpdatedBy",
                table: "ReminderTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceConstraints_Users_CreatedBy",
                table: "ServiceConstraints");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceConstraints_Users_DeletedBy",
                table: "ServiceConstraints");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceConstraints_Users_UpdatedBy",
                table: "ServiceConstraints");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionPayments_MasterCurrencies_CurrencyId",
                table: "SubscriptionPayments");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionPayments_Subscriptions_SubscriptionId",
                table: "SubscriptionPayments");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionPayments_Users_CreatedBy",
                table: "SubscriptionPayments");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionPayments_Users_DeletedBy",
                table: "SubscriptionPayments");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionPayments_Users_UpdatedBy",
                table: "SubscriptionPayments");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionPlanPrivileges_MasterBillingCycles_UsagePeriodId",
                table: "SubscriptionPlanPrivileges");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionPlanPrivileges_Users_CreatedBy",
                table: "SubscriptionPlanPrivileges");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionPlanPrivileges_Users_DeletedBy",
                table: "SubscriptionPlanPrivileges");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionPlanPrivileges_Users_UpdatedBy",
                table: "SubscriptionPlanPrivileges");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionPlans_Users_CreatedBy",
                table: "SubscriptionPlans");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionPlans_Users_DeletedBy",
                table: "SubscriptionPlans");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionPlans_Users_UpdatedBy",
                table: "SubscriptionPlans");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_Users_CreatedBy",
                table: "Subscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_Users_DeletedBy",
                table: "Subscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_Users_UpdatedBy",
                table: "Subscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionStatusHistories_Users_ChangedByUserId",
                table: "SubscriptionStatusHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionStatusHistories_Users_CreatedBy",
                table: "SubscriptionStatusHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionStatusHistories_Users_DeletedBy",
                table: "SubscriptionStatusHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionStatusHistories_Users_UpdatedBy",
                table: "SubscriptionStatusHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAnswerOptions_Users_CreatedBy",
                table: "UserAnswerOptions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAnswerOptions_Users_DeletedBy",
                table: "UserAnswerOptions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAnswerOptions_Users_UpdatedBy",
                table: "UserAnswerOptions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAnswers_Users_CreatedBy",
                table: "UserAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAnswers_Users_DeletedBy",
                table: "UserAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAnswers_Users_UpdatedBy",
                table: "UserAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_UserResponses_Users_CreatedBy",
                table: "UserResponses");

            migrationBuilder.DropForeignKey(
                name: "FK_UserResponses_Users_DeletedBy",
                table: "UserResponses");

            migrationBuilder.DropForeignKey(
                name: "FK_UserResponses_Users_UpdatedBy",
                table: "UserResponses");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_Users_CreatedBy",
                table: "UserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_Users_DeletedBy",
                table: "UserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_Users_UpdatedBy",
                table: "UserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSubscriptionPrivilegeUsages_Privileges_PrivilegeId",
                table: "UserSubscriptionPrivilegeUsages");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSubscriptionPrivilegeUsages_Privileges_PrivilegeId1",
                table: "UserSubscriptionPrivilegeUsages");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSubscriptionPrivilegeUsages_Users_CreatedBy",
                table: "UserSubscriptionPrivilegeUsages");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSubscriptionPrivilegeUsages_Users_DeletedBy",
                table: "UserSubscriptionPrivilegeUsages");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSubscriptionPrivilegeUsages_Users_UpdatedBy",
                table: "UserSubscriptionPrivilegeUsages");

            migrationBuilder.DropForeignKey(
                name: "FK_VideoCallEvents_Users_CreatedBy",
                table: "VideoCallEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_VideoCallEvents_Users_DeletedBy",
                table: "VideoCallEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_VideoCallEvents_Users_UpdatedBy",
                table: "VideoCallEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_VideoCallParticipants_Users_CreatedBy",
                table: "VideoCallParticipants");

            migrationBuilder.DropForeignKey(
                name: "FK_VideoCallParticipants_Users_DeletedBy",
                table: "VideoCallParticipants");

            migrationBuilder.DropForeignKey(
                name: "FK_VideoCallParticipants_Users_UpdatedBy",
                table: "VideoCallParticipants");

            migrationBuilder.DropForeignKey(
                name: "FK_VideoCalls_Users_CreatedBy",
                table: "VideoCalls");

            migrationBuilder.DropForeignKey(
                name: "FK_VideoCalls_Users_DeletedBy",
                table: "VideoCalls");

            migrationBuilder.DropForeignKey(
                name: "FK_VideoCalls_Users_UpdatedBy",
                table: "VideoCalls");

            migrationBuilder.DropTable(
                name: "ProcessedWebhookEvents");

            migrationBuilder.DropIndex(
                name: "IX_VideoCalls_CreatedBy",
                table: "VideoCalls");

            migrationBuilder.DropIndex(
                name: "IX_VideoCalls_DeletedBy",
                table: "VideoCalls");

            migrationBuilder.DropIndex(
                name: "IX_VideoCalls_UpdatedBy",
                table: "VideoCalls");

            migrationBuilder.DropIndex(
                name: "IX_VideoCallParticipants_CreatedBy",
                table: "VideoCallParticipants");

            migrationBuilder.DropIndex(
                name: "IX_VideoCallParticipants_DeletedBy",
                table: "VideoCallParticipants");

            migrationBuilder.DropIndex(
                name: "IX_VideoCallParticipants_UpdatedBy",
                table: "VideoCallParticipants");

            migrationBuilder.DropIndex(
                name: "IX_VideoCallEvents_CreatedBy",
                table: "VideoCallEvents");

            migrationBuilder.DropIndex(
                name: "IX_VideoCallEvents_DeletedBy",
                table: "VideoCallEvents");

            migrationBuilder.DropIndex(
                name: "IX_VideoCallEvents_UpdatedBy",
                table: "VideoCallEvents");

            migrationBuilder.DropIndex(
                name: "IX_UserSubscriptionPrivilegeUsages_CreatedBy",
                table: "UserSubscriptionPrivilegeUsages");

            migrationBuilder.DropIndex(
                name: "IX_UserSubscriptionPrivilegeUsages_DeletedBy",
                table: "UserSubscriptionPrivilegeUsages");

            migrationBuilder.DropIndex(
                name: "IX_UserSubscriptionPrivilegeUsages_LastUsedAt",
                table: "UserSubscriptionPrivilegeUsages");

            migrationBuilder.DropIndex(
                name: "IX_UserSubscriptionPrivilegeUsages_PrivilegeId1",
                table: "UserSubscriptionPrivilegeUsages");

            migrationBuilder.DropIndex(
                name: "IX_UserSubscriptionPrivilegeUsages_UpdatedBy",
                table: "UserSubscriptionPrivilegeUsages");

            migrationBuilder.DropIndex(
                name: "IX_UserSubscriptionPrivilegeUsages_UsagePeriodEnd",
                table: "UserSubscriptionPrivilegeUsages");

            migrationBuilder.DropIndex(
                name: "IX_UserSubscriptionPrivilegeUsages_UsagePeriodStart",
                table: "UserSubscriptionPrivilegeUsages");

            migrationBuilder.DropIndex(
                name: "IX_UserRoles_CreatedBy",
                table: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_UserRoles_DeletedBy",
                table: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_UserRoles_UpdatedBy",
                table: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_UserResponses_CreatedBy",
                table: "UserResponses");

            migrationBuilder.DropIndex(
                name: "IX_UserResponses_DeletedBy",
                table: "UserResponses");

            migrationBuilder.DropIndex(
                name: "IX_UserResponses_UpdatedBy",
                table: "UserResponses");

            migrationBuilder.DropIndex(
                name: "IX_UserAnswers_CreatedBy",
                table: "UserAnswers");

            migrationBuilder.DropIndex(
                name: "IX_UserAnswers_DeletedBy",
                table: "UserAnswers");

            migrationBuilder.DropIndex(
                name: "IX_UserAnswers_UpdatedBy",
                table: "UserAnswers");

            migrationBuilder.DropIndex(
                name: "IX_UserAnswerOptions_CreatedBy",
                table: "UserAnswerOptions");

            migrationBuilder.DropIndex(
                name: "IX_UserAnswerOptions_DeletedBy",
                table: "UserAnswerOptions");

            migrationBuilder.DropIndex(
                name: "IX_UserAnswerOptions_UpdatedBy",
                table: "UserAnswerOptions");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionStatusHistories_ChangedAt",
                table: "SubscriptionStatusHistories");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionStatusHistories_CreatedBy",
                table: "SubscriptionStatusHistories");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionStatusHistories_DeletedBy",
                table: "SubscriptionStatusHistories");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionStatusHistories_ToStatus",
                table: "SubscriptionStatusHistories");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionStatusHistories_UpdatedBy",
                table: "SubscriptionStatusHistories");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_AutoRenew",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_CreatedBy",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_DeletedBy",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_IsTrialSubscription",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_NextBillingDate",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_StartDate",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_Status",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_StripeCustomerId",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_StripeSubscriptionId",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_UpdatedBy",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionPlans_CreatedBy",
                table: "SubscriptionPlans");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionPlans_DeletedBy",
                table: "SubscriptionPlans");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionPlans_IsActive",
                table: "SubscriptionPlans");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionPlans_IsFeatured",
                table: "SubscriptionPlans");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionPlans_Name",
                table: "SubscriptionPlans");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionPlans_PlanType",
                table: "SubscriptionPlans");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionPlans_StripeProductId",
                table: "SubscriptionPlans");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionPlans_UpdatedBy",
                table: "SubscriptionPlans");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionPlanPrivileges_CreatedBy",
                table: "SubscriptionPlanPrivileges");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionPlanPrivileges_DeletedBy",
                table: "SubscriptionPlanPrivileges");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionPlanPrivileges_EffectiveDate",
                table: "SubscriptionPlanPrivileges");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionPlanPrivileges_ExpirationDate",
                table: "SubscriptionPlanPrivileges");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionPlanPrivileges_UpdatedBy",
                table: "SubscriptionPlanPrivileges");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionPayments_CreatedBy",
                table: "SubscriptionPayments");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionPayments_DeletedBy",
                table: "SubscriptionPayments");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionPayments_DueDate",
                table: "SubscriptionPayments");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionPayments_PaidAt",
                table: "SubscriptionPayments");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionPayments_Status",
                table: "SubscriptionPayments");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionPayments_StripeInvoiceId",
                table: "SubscriptionPayments");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionPayments_StripePaymentIntentId",
                table: "SubscriptionPayments");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionPayments_Type",
                table: "SubscriptionPayments");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionPayments_UpdatedBy",
                table: "SubscriptionPayments");

            migrationBuilder.DropIndex(
                name: "IX_ServiceConstraints_CreatedBy",
                table: "ServiceConstraints");

            migrationBuilder.DropIndex(
                name: "IX_ServiceConstraints_DeletedBy",
                table: "ServiceConstraints");

            migrationBuilder.DropIndex(
                name: "IX_ServiceConstraints_UpdatedBy",
                table: "ServiceConstraints");

            migrationBuilder.DropIndex(
                name: "IX_ReminderTypes_CreatedBy",
                table: "ReminderTypes");

            migrationBuilder.DropIndex(
                name: "IX_ReminderTypes_DeletedBy",
                table: "ReminderTypes");

            migrationBuilder.DropIndex(
                name: "IX_ReminderTypes_UpdatedBy",
                table: "ReminderTypes");

            migrationBuilder.DropIndex(
                name: "IX_ReminderTimings_CreatedBy",
                table: "ReminderTimings");

            migrationBuilder.DropIndex(
                name: "IX_ReminderTimings_DeletedBy",
                table: "ReminderTimings");

            migrationBuilder.DropIndex(
                name: "IX_ReminderTimings_UpdatedBy",
                table: "ReminderTimings");

            migrationBuilder.DropIndex(
                name: "IX_RefundStatuses_CreatedBy",
                table: "RefundStatuses");

            migrationBuilder.DropIndex(
                name: "IX_RefundStatuses_DeletedBy",
                table: "RefundStatuses");

            migrationBuilder.DropIndex(
                name: "IX_RefundStatuses_Name",
                table: "RefundStatuses");

            migrationBuilder.DropIndex(
                name: "IX_RefundStatuses_SortOrder",
                table: "RefundStatuses");

            migrationBuilder.DropIndex(
                name: "IX_RefundStatuses_UpdatedBy",
                table: "RefundStatuses");

            migrationBuilder.DropIndex(
                name: "IX_Questions_CreatedBy",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Questions_DeletedBy",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Questions_UpdatedBy",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_QuestionOptions_CreatedBy",
                table: "QuestionOptions");

            migrationBuilder.DropIndex(
                name: "IX_QuestionOptions_DeletedBy",
                table: "QuestionOptions");

            migrationBuilder.DropIndex(
                name: "IX_QuestionOptions_UpdatedBy",
                table: "QuestionOptions");

            migrationBuilder.DropIndex(
                name: "IX_QuestionnaireTemplates_CreatedBy",
                table: "QuestionnaireTemplates");

            migrationBuilder.DropIndex(
                name: "IX_QuestionnaireTemplates_DeletedBy",
                table: "QuestionnaireTemplates");

            migrationBuilder.DropIndex(
                name: "IX_QuestionnaireTemplates_UpdatedBy",
                table: "QuestionnaireTemplates");

            migrationBuilder.DropIndex(
                name: "IX_Providers_CreatedBy",
                table: "Providers");

            migrationBuilder.DropIndex(
                name: "IX_Providers_DeletedBy",
                table: "Providers");

            migrationBuilder.DropIndex(
                name: "IX_Providers_UpdatedBy",
                table: "Providers");

            migrationBuilder.DropIndex(
                name: "IX_ProviderOnboardings_CreatedBy",
                table: "ProviderOnboardings");

            migrationBuilder.DropIndex(
                name: "IX_ProviderOnboardings_DeletedBy",
                table: "ProviderOnboardings");

            migrationBuilder.DropIndex(
                name: "IX_ProviderOnboardings_UpdatedBy",
                table: "ProviderOnboardings");

            migrationBuilder.DropIndex(
                name: "IX_ProviderFees_CreatedBy",
                table: "ProviderFees");

            migrationBuilder.DropIndex(
                name: "IX_ProviderFees_DeletedBy",
                table: "ProviderFees");

            migrationBuilder.DropIndex(
                name: "IX_ProviderFees_UpdatedBy",
                table: "ProviderFees");

            migrationBuilder.DropIndex(
                name: "IX_ProviderCategories_CreatedBy",
                table: "ProviderCategories");

            migrationBuilder.DropIndex(
                name: "IX_ProviderCategories_DeletedBy",
                table: "ProviderCategories");

            migrationBuilder.DropIndex(
                name: "IX_ProviderCategories_UpdatedBy",
                table: "ProviderCategories");

            migrationBuilder.DropIndex(
                name: "IX_PrivilegeUsageHistories_CreatedBy",
                table: "PrivilegeUsageHistories");

            migrationBuilder.DropIndex(
                name: "IX_PrivilegeUsageHistories_DeletedBy",
                table: "PrivilegeUsageHistories");

            migrationBuilder.DropIndex(
                name: "IX_PrivilegeUsageHistories_UpdatedBy",
                table: "PrivilegeUsageHistories");

            migrationBuilder.DropIndex(
                name: "IX_Privileges_CreatedBy",
                table: "Privileges");

            migrationBuilder.DropIndex(
                name: "IX_Privileges_DeletedBy",
                table: "Privileges");

            migrationBuilder.DropIndex(
                name: "IX_Privileges_IsActive",
                table: "Privileges");

            migrationBuilder.DropIndex(
                name: "IX_Privileges_MasterPrivilegeTypeId",
                table: "Privileges");

            migrationBuilder.DropIndex(
                name: "IX_Privileges_Name",
                table: "Privileges");

            migrationBuilder.DropIndex(
                name: "IX_Privileges_UpdatedBy",
                table: "Privileges");

            migrationBuilder.DropIndex(
                name: "IX_Prescriptions_CreatedBy",
                table: "Prescriptions");

            migrationBuilder.DropIndex(
                name: "IX_Prescriptions_DeletedBy",
                table: "Prescriptions");

            migrationBuilder.DropIndex(
                name: "IX_Prescriptions_UpdatedBy",
                table: "Prescriptions");

            migrationBuilder.DropIndex(
                name: "IX_PrescriptionItems_CreatedBy",
                table: "PrescriptionItems");

            migrationBuilder.DropIndex(
                name: "IX_PrescriptionItems_DeletedBy",
                table: "PrescriptionItems");

            migrationBuilder.DropIndex(
                name: "IX_PrescriptionItems_UpdatedBy",
                table: "PrescriptionItems");

            migrationBuilder.DropIndex(
                name: "IX_PaymentStatuses_CreatedBy",
                table: "PaymentStatuses");

            migrationBuilder.DropIndex(
                name: "IX_PaymentStatuses_DeletedBy",
                table: "PaymentStatuses");

            migrationBuilder.DropIndex(
                name: "IX_PaymentStatuses_Name",
                table: "PaymentStatuses");

            migrationBuilder.DropIndex(
                name: "IX_PaymentStatuses_SortOrder",
                table: "PaymentStatuses");

            migrationBuilder.DropIndex(
                name: "IX_PaymentStatuses_UpdatedBy",
                table: "PaymentStatuses");

            migrationBuilder.DropIndex(
                name: "IX_PaymentRefunds_CreatedBy",
                table: "PaymentRefunds");

            migrationBuilder.DropIndex(
                name: "IX_PaymentRefunds_DeletedBy",
                table: "PaymentRefunds");

            migrationBuilder.DropIndex(
                name: "IX_PaymentRefunds_RefundedAt",
                table: "PaymentRefunds");

            migrationBuilder.DropIndex(
                name: "IX_PaymentRefunds_StripeRefundId",
                table: "PaymentRefunds");

            migrationBuilder.DropIndex(
                name: "IX_PaymentRefunds_UpdatedBy",
                table: "PaymentRefunds");

            migrationBuilder.DropIndex(
                name: "IX_ParticipantStatuses_CreatedBy",
                table: "ParticipantStatuses");

            migrationBuilder.DropIndex(
                name: "IX_ParticipantStatuses_DeletedBy",
                table: "ParticipantStatuses");

            migrationBuilder.DropIndex(
                name: "IX_ParticipantStatuses_UpdatedBy",
                table: "ParticipantStatuses");

            migrationBuilder.DropIndex(
                name: "IX_ParticipantRoles_CreatedBy",
                table: "ParticipantRoles");

            migrationBuilder.DropIndex(
                name: "IX_ParticipantRoles_DeletedBy",
                table: "ParticipantRoles");

            migrationBuilder.DropIndex(
                name: "IX_ParticipantRoles_UpdatedBy",
                table: "ParticipantRoles");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_CreatedBy",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_DeletedBy",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UpdatedBy",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Messages_CreatedBy",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_DeletedBy",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_UpdatedBy",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_MessageReadReceipts_CreatedBy",
                table: "MessageReadReceipts");

            migrationBuilder.DropIndex(
                name: "IX_MessageReadReceipts_DeletedBy",
                table: "MessageReadReceipts");

            migrationBuilder.DropIndex(
                name: "IX_MessageReadReceipts_UpdatedBy",
                table: "MessageReadReceipts");

            migrationBuilder.DropIndex(
                name: "IX_MessageReactions_CreatedBy",
                table: "MessageReactions");

            migrationBuilder.DropIndex(
                name: "IX_MessageReactions_DeletedBy",
                table: "MessageReactions");

            migrationBuilder.DropIndex(
                name: "IX_MessageReactions_UpdatedBy",
                table: "MessageReactions");

            migrationBuilder.DropIndex(
                name: "IX_MessageAttachments_CreatedBy",
                table: "MessageAttachments");

            migrationBuilder.DropIndex(
                name: "IX_MessageAttachments_DeletedBy",
                table: "MessageAttachments");

            migrationBuilder.DropIndex(
                name: "IX_MessageAttachments_UpdatedBy",
                table: "MessageAttachments");

            migrationBuilder.DropIndex(
                name: "IX_MedicationDeliveries_CreatedBy",
                table: "MedicationDeliveries");

            migrationBuilder.DropIndex(
                name: "IX_MedicationDeliveries_DeletedBy",
                table: "MedicationDeliveries");

            migrationBuilder.DropIndex(
                name: "IX_MedicationDeliveries_UpdatedBy",
                table: "MedicationDeliveries");

            migrationBuilder.DropIndex(
                name: "IX_MasterPrivilegeTypes_CreatedBy",
                table: "MasterPrivilegeTypes");

            migrationBuilder.DropIndex(
                name: "IX_MasterPrivilegeTypes_DeletedBy",
                table: "MasterPrivilegeTypes");

            migrationBuilder.DropIndex(
                name: "IX_MasterPrivilegeTypes_Description",
                table: "MasterPrivilegeTypes");

            migrationBuilder.DropIndex(
                name: "IX_MasterPrivilegeTypes_Name",
                table: "MasterPrivilegeTypes");

            migrationBuilder.DropIndex(
                name: "IX_MasterPrivilegeTypes_SortOrder",
                table: "MasterPrivilegeTypes");

            migrationBuilder.DropIndex(
                name: "IX_MasterPrivilegeTypes_UpdatedBy",
                table: "MasterPrivilegeTypes");

            migrationBuilder.DropIndex(
                name: "IX_MasterCurrencies_Code",
                table: "MasterCurrencies");

            migrationBuilder.DropIndex(
                name: "IX_MasterCurrencies_CreatedBy",
                table: "MasterCurrencies");

            migrationBuilder.DropIndex(
                name: "IX_MasterCurrencies_DeletedBy",
                table: "MasterCurrencies");

            migrationBuilder.DropIndex(
                name: "IX_MasterCurrencies_Name",
                table: "MasterCurrencies");

            migrationBuilder.DropIndex(
                name: "IX_MasterCurrencies_SortOrder",
                table: "MasterCurrencies");

            migrationBuilder.DropIndex(
                name: "IX_MasterCurrencies_Symbol",
                table: "MasterCurrencies");

            migrationBuilder.DropIndex(
                name: "IX_MasterCurrencies_UpdatedBy",
                table: "MasterCurrencies");

            migrationBuilder.DropIndex(
                name: "IX_MasterBillingCycles_CreatedBy",
                table: "MasterBillingCycles");

            migrationBuilder.DropIndex(
                name: "IX_MasterBillingCycles_DeletedBy",
                table: "MasterBillingCycles");

            migrationBuilder.DropIndex(
                name: "IX_MasterBillingCycles_DurationInDays",
                table: "MasterBillingCycles");

            migrationBuilder.DropIndex(
                name: "IX_MasterBillingCycles_Name",
                table: "MasterBillingCycles");

            migrationBuilder.DropIndex(
                name: "IX_MasterBillingCycles_SortOrder",
                table: "MasterBillingCycles");

            migrationBuilder.DropIndex(
                name: "IX_MasterBillingCycles_UpdatedBy",
                table: "MasterBillingCycles");

            migrationBuilder.DropIndex(
                name: "IX_InvitationStatuses_CreatedBy",
                table: "InvitationStatuses");

            migrationBuilder.DropIndex(
                name: "IX_InvitationStatuses_DeletedBy",
                table: "InvitationStatuses");

            migrationBuilder.DropIndex(
                name: "IX_InvitationStatuses_UpdatedBy",
                table: "InvitationStatuses");

            migrationBuilder.DropIndex(
                name: "IX_HealthAssessments_CreatedBy",
                table: "HealthAssessments");

            migrationBuilder.DropIndex(
                name: "IX_HealthAssessments_DeletedBy",
                table: "HealthAssessments");

            migrationBuilder.DropIndex(
                name: "IX_HealthAssessments_UpdatedBy",
                table: "HealthAssessments");

            migrationBuilder.DropIndex(
                name: "IX_EventTypes_CreatedBy",
                table: "EventTypes");

            migrationBuilder.DropIndex(
                name: "IX_EventTypes_DeletedBy",
                table: "EventTypes");

            migrationBuilder.DropIndex(
                name: "IX_EventTypes_UpdatedBy",
                table: "EventTypes");

            migrationBuilder.DropIndex(
                name: "IX_DocumentTypes_CreatedBy",
                table: "DocumentTypes");

            migrationBuilder.DropIndex(
                name: "IX_DocumentTypes_DeletedBy",
                table: "DocumentTypes");

            migrationBuilder.DropIndex(
                name: "IX_DocumentTypes_UpdatedBy",
                table: "DocumentTypes");

            migrationBuilder.DropIndex(
                name: "IX_Documents_UpdatedBy",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_DocumentReferences_DeletedBy",
                table: "DocumentReferences");

            migrationBuilder.DropIndex(
                name: "IX_DocumentReferences_UpdatedBy",
                table: "DocumentReferences");

            migrationBuilder.DropIndex(
                name: "IX_DeliveryTracking_CreatedBy",
                table: "DeliveryTracking");

            migrationBuilder.DropIndex(
                name: "IX_DeliveryTracking_DeletedBy",
                table: "DeliveryTracking");

            migrationBuilder.DropIndex(
                name: "IX_DeliveryTracking_UpdatedBy",
                table: "DeliveryTracking");

            migrationBuilder.DropIndex(
                name: "IX_Consultations_CreatedBy",
                table: "Consultations");

            migrationBuilder.DropIndex(
                name: "IX_Consultations_DeletedBy",
                table: "Consultations");

            migrationBuilder.DropIndex(
                name: "IX_Consultations_UpdatedBy",
                table: "Consultations");

            migrationBuilder.DropIndex(
                name: "IX_ConsultationModes_CreatedBy",
                table: "ConsultationModes");

            migrationBuilder.DropIndex(
                name: "IX_ConsultationModes_DeletedBy",
                table: "ConsultationModes");

            migrationBuilder.DropIndex(
                name: "IX_ConsultationModes_UpdatedBy",
                table: "ConsultationModes");

            migrationBuilder.DropIndex(
                name: "IX_ChatSessions_CreatedBy",
                table: "ChatSessions");

            migrationBuilder.DropIndex(
                name: "IX_ChatSessions_DeletedBy",
                table: "ChatSessions");

            migrationBuilder.DropIndex(
                name: "IX_ChatSessions_UpdatedBy",
                table: "ChatSessions");

            migrationBuilder.DropIndex(
                name: "IX_ChatRooms_CreatedBy",
                table: "ChatRooms");

            migrationBuilder.DropIndex(
                name: "IX_ChatRooms_DeletedBy",
                table: "ChatRooms");

            migrationBuilder.DropIndex(
                name: "IX_ChatRooms_UpdatedBy",
                table: "ChatRooms");

            migrationBuilder.DropIndex(
                name: "IX_ChatRoomParticipants_CreatedBy",
                table: "ChatRoomParticipants");

            migrationBuilder.DropIndex(
                name: "IX_ChatRoomParticipants_DeletedBy",
                table: "ChatRoomParticipants");

            migrationBuilder.DropIndex(
                name: "IX_ChatRoomParticipants_UpdatedBy",
                table: "ChatRoomParticipants");

            migrationBuilder.DropIndex(
                name: "IX_ChatMessages_CreatedBy",
                table: "ChatMessages");

            migrationBuilder.DropIndex(
                name: "IX_ChatMessages_DeletedBy",
                table: "ChatMessages");

            migrationBuilder.DropIndex(
                name: "IX_ChatMessages_UpdatedBy",
                table: "ChatMessages");

            migrationBuilder.DropIndex(
                name: "IX_ChatAttachments_CreatedBy",
                table: "ChatAttachments");

            migrationBuilder.DropIndex(
                name: "IX_ChatAttachments_DeletedBy",
                table: "ChatAttachments");

            migrationBuilder.DropIndex(
                name: "IX_ChatAttachments_UpdatedBy",
                table: "ChatAttachments");

            migrationBuilder.DropIndex(
                name: "IX_CategoryFeeRanges_CreatedBy",
                table: "CategoryFeeRanges");

            migrationBuilder.DropIndex(
                name: "IX_CategoryFeeRanges_DeletedBy",
                table: "CategoryFeeRanges");

            migrationBuilder.DropIndex(
                name: "IX_CategoryFeeRanges_UpdatedBy",
                table: "CategoryFeeRanges");

            migrationBuilder.DropIndex(
                name: "IX_Categories_CreatedBy",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_DeletedBy",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_UpdatedBy",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_BillingRecords_BillingCycleId",
                table: "BillingRecords");

            migrationBuilder.DropIndex(
                name: "IX_BillingRecords_BillingDate",
                table: "BillingRecords");

            migrationBuilder.DropIndex(
                name: "IX_BillingRecords_CreatedBy",
                table: "BillingRecords");

            migrationBuilder.DropIndex(
                name: "IX_BillingRecords_DeletedBy",
                table: "BillingRecords");

            migrationBuilder.DropIndex(
                name: "IX_BillingRecords_DueDate",
                table: "BillingRecords");

            migrationBuilder.DropIndex(
                name: "IX_BillingRecords_InvoiceNumber",
                table: "BillingRecords");

            migrationBuilder.DropIndex(
                name: "IX_BillingRecords_IsRecurring",
                table: "BillingRecords");

            migrationBuilder.DropIndex(
                name: "IX_BillingRecords_MasterCurrencyId",
                table: "BillingRecords");

            migrationBuilder.DropIndex(
                name: "IX_BillingRecords_PaidAt",
                table: "BillingRecords");

            migrationBuilder.DropIndex(
                name: "IX_BillingRecords_PaymentIntentId",
                table: "BillingRecords");

            migrationBuilder.DropIndex(
                name: "IX_BillingRecords_Status",
                table: "BillingRecords");

            migrationBuilder.DropIndex(
                name: "IX_BillingRecords_StripeInvoiceId",
                table: "BillingRecords");

            migrationBuilder.DropIndex(
                name: "IX_BillingRecords_StripePaymentIntentId",
                table: "BillingRecords");

            migrationBuilder.DropIndex(
                name: "IX_BillingRecords_Type",
                table: "BillingRecords");

            migrationBuilder.DropIndex(
                name: "IX_BillingRecords_UpdatedBy",
                table: "BillingRecords");

            migrationBuilder.DropIndex(
                name: "IX_BillingAdjustments_AppliedAt",
                table: "BillingAdjustments");

            migrationBuilder.DropIndex(
                name: "IX_BillingAdjustments_AppliedBy",
                table: "BillingAdjustments");

            migrationBuilder.DropIndex(
                name: "IX_BillingAdjustments_CreatedBy",
                table: "BillingAdjustments");

            migrationBuilder.DropIndex(
                name: "IX_BillingAdjustments_DeletedBy",
                table: "BillingAdjustments");

            migrationBuilder.DropIndex(
                name: "IX_BillingAdjustments_IsApproved",
                table: "BillingAdjustments");

            migrationBuilder.DropIndex(
                name: "IX_BillingAdjustments_IsPercentage",
                table: "BillingAdjustments");

            migrationBuilder.DropIndex(
                name: "IX_BillingAdjustments_Type",
                table: "BillingAdjustments");

            migrationBuilder.DropIndex(
                name: "IX_BillingAdjustments_UpdatedBy",
                table: "BillingAdjustments");

            migrationBuilder.DropIndex(
                name: "IX_AppointmentTypes_CreatedBy",
                table: "AppointmentTypes");

            migrationBuilder.DropIndex(
                name: "IX_AppointmentTypes_DeletedBy",
                table: "AppointmentTypes");

            migrationBuilder.DropIndex(
                name: "IX_AppointmentTypes_UpdatedBy",
                table: "AppointmentTypes");

            migrationBuilder.DropIndex(
                name: "IX_AppointmentStatuses_CreatedBy",
                table: "AppointmentStatuses");

            migrationBuilder.DropIndex(
                name: "IX_AppointmentStatuses_DeletedBy",
                table: "AppointmentStatuses");

            migrationBuilder.DropIndex(
                name: "IX_AppointmentStatuses_UpdatedBy",
                table: "AppointmentStatuses");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_CreatedBy",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_DeletedBy",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_UpdatedBy",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_AppointmentReminders_CreatedBy",
                table: "AppointmentReminders");

            migrationBuilder.DropIndex(
                name: "IX_AppointmentReminders_DeletedBy",
                table: "AppointmentReminders");

            migrationBuilder.DropIndex(
                name: "IX_AppointmentReminders_UpdatedBy",
                table: "AppointmentReminders");

            migrationBuilder.DropIndex(
                name: "IX_AppointmentPaymentLogs_CreatedBy",
                table: "AppointmentPaymentLogs");

            migrationBuilder.DropIndex(
                name: "IX_AppointmentPaymentLogs_DeletedBy",
                table: "AppointmentPaymentLogs");

            migrationBuilder.DropIndex(
                name: "IX_AppointmentPaymentLogs_UpdatedBy",
                table: "AppointmentPaymentLogs");

            migrationBuilder.DropIndex(
                name: "IX_AppointmentParticipants_CreatedBy",
                table: "AppointmentParticipants");

            migrationBuilder.DropIndex(
                name: "IX_AppointmentParticipants_DeletedBy",
                table: "AppointmentParticipants");

            migrationBuilder.DropIndex(
                name: "IX_AppointmentParticipants_UpdatedBy",
                table: "AppointmentParticipants");

            migrationBuilder.DropIndex(
                name: "IX_AppointmentInvitations_CreatedBy",
                table: "AppointmentInvitations");

            migrationBuilder.DropIndex(
                name: "IX_AppointmentInvitations_DeletedBy",
                table: "AppointmentInvitations");

            migrationBuilder.DropIndex(
                name: "IX_AppointmentInvitations_UpdatedBy",
                table: "AppointmentInvitations");

            migrationBuilder.DropIndex(
                name: "IX_AppointmentEvents_CreatedBy",
                table: "AppointmentEvents");

            migrationBuilder.DropIndex(
                name: "IX_AppointmentEvents_DeletedBy",
                table: "AppointmentEvents");

            migrationBuilder.DropIndex(
                name: "IX_AppointmentEvents_UpdatedBy",
                table: "AppointmentEvents");

            migrationBuilder.DropIndex(
                name: "IX_AppointmentDocuments_CreatedBy",
                table: "AppointmentDocuments");

            migrationBuilder.DropIndex(
                name: "IX_AppointmentDocuments_DeletedBy",
                table: "AppointmentDocuments");

            migrationBuilder.DropIndex(
                name: "IX_AppointmentDocuments_UpdatedBy",
                table: "AppointmentDocuments");

            migrationBuilder.DropColumn(
                name: "PrivilegeId1",
                table: "UserSubscriptionPrivilegeUsages");

            migrationBuilder.DropColumn(
                name: "PlanType",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "UnitCost",
                table: "SubscriptionPlanPrivileges");

            migrationBuilder.DropColumn(
                name: "MasterPrivilegeTypeId",
                table: "Privileges");

            migrationBuilder.DropColumn(
                name: "MasterCurrencyId",
                table: "BillingRecords");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "VideoCalls",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "VideoCalls",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "VideoCalls",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "VideoCallParticipants",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "VideoCallParticipants",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "VideoCallParticipants",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "VideoCallEvents",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "VideoCallEvents",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "VideoCallEvents",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<Guid>(
                name: "PrivilegeId",
                table: "UserSubscriptionPrivilegeUsages",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "UserSubscriptionPrivilegeUsages",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "UserSubscriptionPrivilegeUsages",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "UserSubscriptionPrivilegeUsages",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordResetToken",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<int>(
                name: "SortOrder",
                table: "UserRoles",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "UserRoles",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "UserRoles",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "UserResponses",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "UserResponses",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "UserResponses",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "UserAnswers",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "UserAnswers",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "UserAnswers",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "UserAnswerOptions",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "UserAnswerOptions",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "UserAnswerOptions",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "SubscriptionStatusHistories",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "SubscriptionStatusHistories",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "SubscriptionStatusHistories",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<int>(
                name: "TrialDurationInDays",
                table: "Subscriptions",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "TotalUsageCount",
                table: "Subscriptions",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<bool>(
                name: "IsTrialSubscription",
                table: "Subscriptions",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Subscriptions",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Subscriptions",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<int>(
                name: "FailedPaymentAttempts",
                table: "Subscriptions",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "Subscriptions",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<int>(
                name: "TrialDurationInDays",
                table: "SubscriptionPlans",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "MessagingCount",
                table: "SubscriptionPlans",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 10);

            migrationBuilder.AlterColumn<int>(
                name: "MaxPauseDurationDays",
                table: "SubscriptionPlans",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 90);

            migrationBuilder.AlterColumn<bool>(
                name: "IsTrialAllowed",
                table: "SubscriptionPlans",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsTrending",
                table: "SubscriptionPlans",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsMostPopular",
                table: "SubscriptionPlans",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsFeatured",
                table: "SubscriptionPlans",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "SubscriptionPlans",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IncludesMedicationDelivery",
                table: "SubscriptionPlans",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IncludesFollowUpCare",
                table: "SubscriptionPlans",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<int>(
                name: "DisplayOrder",
                table: "SubscriptionPlans",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "DeliveryFrequencyDays",
                table: "SubscriptionPlans",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 30);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "SubscriptionPlans",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "SubscriptionPlanPrivileges",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "SubscriptionPlanPrivileges",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<int>(
                name: "DurationMonths",
                table: "SubscriptionPlanPrivileges",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "SubscriptionPlanPrivileges",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "SubscriptionPayments",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "SubscriptionPayments",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "SubscriptionPayments",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "SubscriptionPayments",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "SubscriptionPayments",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<int>(
                name: "AttemptCount",
                table: "SubscriptionPayments",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "ServiceConstraints",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "ServiceConstraints",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "ServiceConstraints",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<int>(
                name: "SortOrder",
                table: "ReminderTypes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "ReminderTypes",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "ReminderTypes",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<int>(
                name: "SortOrder",
                table: "ReminderTimings",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "MinutesBeforeAppointment",
                table: "ReminderTimings",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "ReminderTimings",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "ReminderTimings",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "RefundStatuses",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "RefundStatuses",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Questions",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Questions",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "Questions",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "QuestionOptions",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "QuestionOptions",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "QuestionOptions",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "QuestionnaireTemplates",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "QuestionnaireTemplates",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "QuestionnaireTemplates",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Providers",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "Providers",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "ProviderOnboardings",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "ProviderOnboardings",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "ProviderOnboardings",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "ProviderFees",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "ProviderFees",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "ProviderFees",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "ProviderCategories",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "ProviderCategories",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "ProviderCategories",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "PrivilegeUsageHistories",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "PrivilegeUsageHistories",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "PrivilegeUsageHistories",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Privileges",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Privileges",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "Privileges",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Prescriptions",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Prescriptions",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "Prescriptions",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "PrescriptionItems",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "PrescriptionItems",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "PrescriptionItems",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "PaymentStatuses",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "PaymentStatuses",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "PaymentRefunds",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "PaymentRefunds",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "PaymentRefunds",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<int>(
                name: "SortOrder",
                table: "ParticipantStatuses",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "ParticipantStatuses",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "ParticipantStatuses",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<int>(
                name: "SortOrder",
                table: "ParticipantRoles",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "ParticipantRoles",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "ParticipantRoles",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Notifications",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Notifications",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "Notifications",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Messages",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Messages",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "Messages",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<Guid>(
                name: "SubscriptionId",
                table: "Messages",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "MessageReadReceipts",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "MessageReadReceipts",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "MessageReadReceipts",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "MessageReactions",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "MessageReactions",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "MessageReactions",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "MessageAttachments",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "MessageAttachments",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "MessageAttachments",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "MedicationDeliveries",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "MedicationDeliveries",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "MedicationDeliveries",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "MasterPrivilegeTypes",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "MasterPrivilegeTypes",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "MasterCurrencies",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "MasterCurrencies",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "MasterBillingCycles",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "MasterBillingCycles",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<int>(
                name: "SortOrder",
                table: "InvitationStatuses",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "InvitationStatuses",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "InvitationStatuses",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "HealthAssessments",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "HealthAssessments",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "HealthAssessments",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<int>(
                name: "SortOrder",
                table: "EventTypes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "EventTypes",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "EventTypes",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "DocumentTypes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "DocumentTypes",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "DisplayOrder",
                table: "DocumentTypes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "DocumentTypes",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "DocumentTypes",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Documents",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Documents",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "Documents",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "DocumentReferences",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "DocumentReferences",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "DocumentReferences",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "DeliveryTracking",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "DeliveryTracking",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "DeliveryTracking",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Consultations",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Consultations",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "Consultations",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<int>(
                name: "SortOrder",
                table: "ConsultationModes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "ConsultationModes",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "ConsultationModes",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "ChatSessions",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "ChatSessions",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "ChatSessions",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "ChatRooms",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "ChatRooms",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "ChatRooms",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "ChatRoomParticipants",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "ChatRoomParticipants",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "ChatRoomParticipants",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "ChatMessages",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "ChatMessages",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "ChatMessages",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "ChatAttachments",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "ChatAttachments",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "ChatAttachments",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "CategoryFeeRanges",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "CategoryFeeRanges",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "CategoryFeeRanges",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<int>(
                name: "OneTimeConsultationDurationMinutes",
                table: "Categories",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 30);

            migrationBuilder.AlterColumn<bool>(
                name: "IsTrending",
                table: "Categories",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsMostPopular",
                table: "Categories",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Categories",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "DisplayOrder",
                table: "Categories",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "Categories",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<string>(
                name: "Color",
                table: "Categories",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "AllowsOneTimeConsultation",
                table: "Categories",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "BillingRecords",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "BillingRecords",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "PaymentIntentId",
                table: "BillingRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "BillingRecords",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "BillingRecords",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "BillingRecords",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "BillingAdjustments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "BillingAdjustments",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "BillingAdjustments",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "BillingAdjustments",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<int>(
                name: "AppliedByUserId",
                table: "BillingAdjustments",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SortOrder",
                table: "AppointmentTypes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "AppointmentTypes",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "AppointmentTypes",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<int>(
                name: "SortOrder",
                table: "AppointmentStatuses",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "AppointmentStatuses",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "AppointmentStatuses",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Appointments",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Appointments",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "Appointments",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "AppointmentReminders",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "AppointmentReminders",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "AppointmentPaymentLogs",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "AppointmentPaymentLogs",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "AppointmentParticipants",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "AppointmentParticipants",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "AppointmentInvitations",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "AppointmentInvitations",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "AppointmentInvitations",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "AppointmentEvents",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "AppointmentEvents",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "AppointmentDocuments",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "AppointmentDocuments",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "AppointmentDocuments",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SubscriptionId",
                table: "Messages",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_BillingAdjustments_AppliedByUserId",
                table: "BillingAdjustments",
                column: "AppliedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_BillingAdjustments_Users_AppliedByUserId",
                table: "BillingAdjustments",
                column: "AppliedByUserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BillingRecords_MasterCurrencies_CurrencyId",
                table: "BillingRecords",
                column: "CurrencyId",
                principalTable: "MasterCurrencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Subscriptions_SubscriptionId",
                table: "Messages",
                column: "SubscriptionId",
                principalTable: "Subscriptions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Privileges_MasterPrivilegeTypes_PrivilegeTypeId",
                table: "Privileges",
                column: "PrivilegeTypeId",
                principalTable: "MasterPrivilegeTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionPayments_MasterCurrencies_CurrencyId",
                table: "SubscriptionPayments",
                column: "CurrencyId",
                principalTable: "MasterCurrencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionPayments_Subscriptions_SubscriptionId",
                table: "SubscriptionPayments",
                column: "SubscriptionId",
                principalTable: "Subscriptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionPlanPrivileges_MasterBillingCycles_UsagePeriodId",
                table: "SubscriptionPlanPrivileges",
                column: "UsagePeriodId",
                principalTable: "MasterBillingCycles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionStatusHistories_Users_ChangedByUserId",
                table: "SubscriptionStatusHistories",
                column: "ChangedByUserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserSubscriptionPrivilegeUsages_Privileges_PrivilegeId",
                table: "UserSubscriptionPrivilegeUsages",
                column: "PrivilegeId",
                principalTable: "Privileges",
                principalColumn: "Id");
        }
    }
}
