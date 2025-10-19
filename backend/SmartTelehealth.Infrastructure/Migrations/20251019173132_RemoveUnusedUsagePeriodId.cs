using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartTelehealth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnusedUsagePeriodId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionPlanPrivileges_MasterBillingCycles_UsagePeriodId",
                table: "SubscriptionPlanPrivileges");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionPlanPrivileges_UsagePeriodId",
                table: "SubscriptionPlanPrivileges");

            migrationBuilder.DropColumn(
                name: "UsagePeriodId",
                table: "SubscriptionPlanPrivileges");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdated",
                table: "SystemSettings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 19, 17, 31, 28, 491, DateTimeKind.Utc).AddTicks(2901),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 10, 18, 15, 47, 4, 120, DateTimeKind.Utc).AddTicks(1694));

            migrationBuilder.AlterColumn<DateTime>(
                name: "VersionCreatedDate",
                table: "SubscriptionPlans",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 19, 17, 31, 28, 416, DateTimeKind.Utc).AddTicks(4798),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 10, 18, 15, 47, 4, 66, DateTimeKind.Utc).AddTicks(9706));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 10, 19, 17, 31, 28, 491, DateTimeKind.Utc).AddTicks(3218), new DateTime(2025, 10, 19, 17, 31, 28, 491, DateTimeKind.Utc).AddTicks(3216) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdated",
                table: "SystemSettings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 18, 15, 47, 4, 120, DateTimeKind.Utc).AddTicks(1694),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 10, 19, 17, 31, 28, 491, DateTimeKind.Utc).AddTicks(2901));

            migrationBuilder.AlterColumn<DateTime>(
                name: "VersionCreatedDate",
                table: "SubscriptionPlans",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 18, 15, 47, 4, 66, DateTimeKind.Utc).AddTicks(9706),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 10, 19, 17, 31, 28, 416, DateTimeKind.Utc).AddTicks(4798));

            migrationBuilder.AddColumn<Guid>(
                name: "UsagePeriodId",
                table: "SubscriptionPlanPrivileges",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 10, 18, 15, 47, 4, 120, DateTimeKind.Utc).AddTicks(2315), new DateTime(2025, 10, 18, 15, 47, 4, 120, DateTimeKind.Utc).AddTicks(2311) });

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlanPrivileges_UsagePeriodId",
                table: "SubscriptionPlanPrivileges",
                column: "UsagePeriodId");

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionPlanPrivileges_MasterBillingCycles_UsagePeriodId",
                table: "SubscriptionPlanPrivileges",
                column: "UsagePeriodId",
                principalTable: "MasterBillingCycles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
