using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartTelehealth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SubscriptionPlanArchitectureFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnnualBillingDiscount",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "MonthlyBillingDiscount",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "QuarterlyBillingDiscount",
                table: "SubscriptionPlans");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdated",
                table: "SystemSettings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 20, 19, 30, 8, 561, DateTimeKind.Utc).AddTicks(171),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 10, 20, 14, 4, 44, 888, DateTimeKind.Utc).AddTicks(877));

            migrationBuilder.AlterColumn<Guid>(
                name: "BillingCycleId",
                table: "Subscriptions",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<DateTime>(
                name: "VersionCreatedDate",
                table: "SubscriptionPlans",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 20, 19, 30, 8, 453, DateTimeKind.Utc).AddTicks(6361),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 10, 20, 14, 4, 44, 840, DateTimeKind.Utc).AddTicks(7715));

            migrationBuilder.AlterColumn<Guid>(
                name: "CategoryId",
                table: "SubscriptionPlans",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 10, 20, 19, 30, 8, 561, DateTimeKind.Utc).AddTicks(671), new DateTime(2025, 10, 20, 19, 30, 8, 561, DateTimeKind.Utc).AddTicks(668) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdated",
                table: "SystemSettings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 20, 14, 4, 44, 888, DateTimeKind.Utc).AddTicks(877),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 10, 20, 19, 30, 8, 561, DateTimeKind.Utc).AddTicks(171));

            migrationBuilder.AlterColumn<Guid>(
                name: "BillingCycleId",
                table: "Subscriptions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "VersionCreatedDate",
                table: "SubscriptionPlans",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 20, 14, 4, 44, 840, DateTimeKind.Utc).AddTicks(7715),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 10, 20, 19, 30, 8, 453, DateTimeKind.Utc).AddTicks(6361));

            migrationBuilder.AlterColumn<Guid>(
                name: "CategoryId",
                table: "SubscriptionPlans",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<decimal>(
                name: "AnnualBillingDiscount",
                table: "SubscriptionPlans",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyBillingDiscount",
                table: "SubscriptionPlans",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "QuarterlyBillingDiscount",
                table: "SubscriptionPlans",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 10, 20, 14, 4, 44, 888, DateTimeKind.Utc).AddTicks(1193), new DateTime(2025, 10, 20, 14, 4, 44, 888, DateTimeKind.Utc).AddTicks(1191) });
        }
    }
}
