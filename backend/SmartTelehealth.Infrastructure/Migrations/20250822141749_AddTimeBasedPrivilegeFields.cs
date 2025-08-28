using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartTelehealth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeBasedPrivilegeFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
        }
    }
}
