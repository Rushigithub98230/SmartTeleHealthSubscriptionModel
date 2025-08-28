using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartTelehealth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeBasedPrivilegeLimits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DailyUsageData",
                table: "UserSubscriptionPrivilegeUsages");

            migrationBuilder.DropColumn(
                name: "LastPeakHourUsage",
                table: "UserSubscriptionPrivilegeUsages");

            migrationBuilder.DropColumn(
                name: "LastTimeSlotUsage",
                table: "UserSubscriptionPrivilegeUsages");

            migrationBuilder.DropColumn(
                name: "OffPeakHourUsageCount",
                table: "UserSubscriptionPrivilegeUsages");

            migrationBuilder.DropColumn(
                name: "PeakHourUsageCount",
                table: "UserSubscriptionPrivilegeUsages");

            migrationBuilder.DropColumn(
                name: "RollingWindowMaxUsage",
                table: "UserSubscriptionPrivilegeUsages");

            migrationBuilder.DropColumn(
                name: "RollingWindowStart",
                table: "UserSubscriptionPrivilegeUsages");

            migrationBuilder.DropColumn(
                name: "RollingWindowUsageCount",
                table: "UserSubscriptionPrivilegeUsages");

            migrationBuilder.DropColumn(
                name: "AllowedDaysOfWeek",
                table: "Privileges");

            migrationBuilder.DropColumn(
                name: "AllowedEndTime",
                table: "Privileges");

            migrationBuilder.DropColumn(
                name: "AllowedStartTime",
                table: "Privileges");

            migrationBuilder.DropColumn(
                name: "HasDayRestrictions",
                table: "Privileges");

            migrationBuilder.DropColumn(
                name: "HasPeakOffPeakRestrictions",
                table: "Privileges");

            migrationBuilder.DropColumn(
                name: "HasRollingWindowRestrictions",
                table: "Privileges");

            migrationBuilder.DropColumn(
                name: "HasTimeSlotRestrictions",
                table: "Privileges");

            migrationBuilder.DropColumn(
                name: "MaxUsagePerRollingWindow",
                table: "Privileges");

            migrationBuilder.DropColumn(
                name: "PeakEndTime",
                table: "Privileges");

            migrationBuilder.DropColumn(
                name: "PeakStartTime",
                table: "Privileges");

            migrationBuilder.DropColumn(
                name: "PeakUsageMultiplier",
                table: "Privileges");

            migrationBuilder.DropColumn(
                name: "RollingWindowDays",
                table: "Privileges");

            migrationBuilder.DropColumn(
                name: "RollingWindowHours",
                table: "Privileges");

            migrationBuilder.DropColumn(
                name: "TimeZone",
                table: "Privileges");

            migrationBuilder.AddColumn<int>(
                name: "DailyLimit",
                table: "SubscriptionPlanPrivileges",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MonthlyLimit",
                table: "SubscriptionPlanPrivileges",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WeeklyLimit",
                table: "SubscriptionPlanPrivileges",
                type: "int",
                nullable: true);

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
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrivilegeUsageHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrivilegeUsageHistories_UserSubscriptionPrivilegeUsages_UserSubscriptionPrivilegeUsageId",
                        column: x => x.UserSubscriptionPrivilegeUsageId,
                        principalTable: "UserSubscriptionPrivilegeUsages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PrivilegeUsageHistories");

            migrationBuilder.DropColumn(
                name: "DailyLimit",
                table: "SubscriptionPlanPrivileges");

            migrationBuilder.DropColumn(
                name: "MonthlyLimit",
                table: "SubscriptionPlanPrivileges");

            migrationBuilder.DropColumn(
                name: "WeeklyLimit",
                table: "SubscriptionPlanPrivileges");

            migrationBuilder.AddColumn<string>(
                name: "DailyUsageData",
                table: "UserSubscriptionPrivilegeUsages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastPeakHourUsage",
                table: "UserSubscriptionPrivilegeUsages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastTimeSlotUsage",
                table: "UserSubscriptionPrivilegeUsages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OffPeakHourUsageCount",
                table: "UserSubscriptionPrivilegeUsages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PeakHourUsageCount",
                table: "UserSubscriptionPrivilegeUsages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RollingWindowMaxUsage",
                table: "UserSubscriptionPrivilegeUsages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RollingWindowStart",
                table: "UserSubscriptionPrivilegeUsages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RollingWindowUsageCount",
                table: "UserSubscriptionPrivilegeUsages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "AllowedDaysOfWeek",
                table: "Privileges",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "AllowedEndTime",
                table: "Privileges",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "AllowedStartTime",
                table: "Privileges",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasDayRestrictions",
                table: "Privileges",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasPeakOffPeakRestrictions",
                table: "Privileges",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasRollingWindowRestrictions",
                table: "Privileges",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasTimeSlotRestrictions",
                table: "Privileges",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaxUsagePerRollingWindow",
                table: "Privileges",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "PeakEndTime",
                table: "Privileges",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "PeakStartTime",
                table: "Privileges",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PeakUsageMultiplier",
                table: "Privileges",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RollingWindowDays",
                table: "Privileges",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RollingWindowHours",
                table: "Privileges",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TimeZone",
                table: "Privileges",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
