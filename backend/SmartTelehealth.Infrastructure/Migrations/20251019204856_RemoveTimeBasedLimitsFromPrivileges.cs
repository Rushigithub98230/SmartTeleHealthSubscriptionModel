using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartTelehealth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTimeBasedLimitsFromPrivileges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DailyLimit",
                table: "SubscriptionPlanPrivileges");

            migrationBuilder.DropColumn(
                name: "MonthlyLimit",
                table: "SubscriptionPlanPrivileges");

            migrationBuilder.DropColumn(
                name: "WeeklyLimit",
                table: "SubscriptionPlanPrivileges");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdated",
                table: "SystemSettings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 19, 20, 48, 53, 327, DateTimeKind.Utc).AddTicks(533),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 10, 19, 17, 31, 28, 491, DateTimeKind.Utc).AddTicks(2901));

            migrationBuilder.AlterColumn<DateTime>(
                name: "VersionCreatedDate",
                table: "SubscriptionPlans",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 19, 20, 48, 53, 188, DateTimeKind.Utc).AddTicks(3906),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 10, 19, 17, 31, 28, 416, DateTimeKind.Utc).AddTicks(4798));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 10, 19, 20, 48, 53, 327, DateTimeKind.Utc).AddTicks(1083), new DateTime(2025, 10, 19, 20, 48, 53, 327, DateTimeKind.Utc).AddTicks(1077) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdated",
                table: "SystemSettings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 19, 17, 31, 28, 491, DateTimeKind.Utc).AddTicks(2901),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 10, 19, 20, 48, 53, 327, DateTimeKind.Utc).AddTicks(533));

            migrationBuilder.AlterColumn<DateTime>(
                name: "VersionCreatedDate",
                table: "SubscriptionPlans",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 19, 17, 31, 28, 416, DateTimeKind.Utc).AddTicks(4798),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 10, 19, 20, 48, 53, 188, DateTimeKind.Utc).AddTicks(3906));

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

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 10, 19, 17, 31, 28, 491, DateTimeKind.Utc).AddTicks(3218), new DateTime(2025, 10, 19, 17, 31, 28, 491, DateTimeKind.Utc).AddTicks(3216) });
        }
    }
}
