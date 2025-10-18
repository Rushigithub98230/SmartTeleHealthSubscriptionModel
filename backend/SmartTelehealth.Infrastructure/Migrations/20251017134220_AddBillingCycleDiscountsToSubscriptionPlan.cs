using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartTelehealth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBillingCycleDiscountsToSubscriptionPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdated",
                table: "SystemSettings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 17, 13, 42, 16, 443, DateTimeKind.Utc).AddTicks(7613),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 10, 16, 13, 22, 36, 52, DateTimeKind.Utc).AddTicks(5330));

            migrationBuilder.AlterColumn<DateTime>(
                name: "VersionCreatedDate",
                table: "SubscriptionPlans",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 17, 13, 42, 16, 352, DateTimeKind.Utc).AddTicks(3518),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 10, 16, 13, 22, 35, 989, DateTimeKind.Utc).AddTicks(4037));

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
                values: new object[] { new DateTime(2025, 10, 17, 13, 42, 16, 443, DateTimeKind.Utc).AddTicks(7992), new DateTime(2025, 10, 17, 13, 42, 16, 443, DateTimeKind.Utc).AddTicks(7990) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
                defaultValue: new DateTime(2025, 10, 16, 13, 22, 36, 52, DateTimeKind.Utc).AddTicks(5330),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 10, 17, 13, 42, 16, 443, DateTimeKind.Utc).AddTicks(7613));

            migrationBuilder.AlterColumn<DateTime>(
                name: "VersionCreatedDate",
                table: "SubscriptionPlans",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 16, 13, 22, 35, 989, DateTimeKind.Utc).AddTicks(4037),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 10, 17, 13, 42, 16, 352, DateTimeKind.Utc).AddTicks(3518));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 10, 16, 13, 22, 36, 52, DateTimeKind.Utc).AddTicks(5683), new DateTime(2025, 10, 16, 13, 22, 36, 52, DateTimeKind.Utc).AddTicks(5680) });
        }
    }
}
