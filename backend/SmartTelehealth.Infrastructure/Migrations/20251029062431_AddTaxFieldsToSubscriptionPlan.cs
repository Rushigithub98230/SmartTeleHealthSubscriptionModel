using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartTelehealth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTaxFieldsToSubscriptionPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdated",
                table: "SystemSettings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 29, 6, 24, 29, 157, DateTimeKind.Utc).AddTicks(9835),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 10, 28, 19, 53, 1, 598, DateTimeKind.Utc).AddTicks(8565));

            migrationBuilder.AlterColumn<DateTime>(
                name: "VersionCreatedDate",
                table: "SubscriptionPlans",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 29, 6, 24, 29, 58, DateTimeKind.Utc).AddTicks(4502),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 10, 28, 19, 53, 1, 523, DateTimeKind.Utc).AddTicks(1216));

            migrationBuilder.AddColumn<decimal>(
                name: "DefaultTaxPercentage",
                table: "SubscriptionPlans",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxNotes",
                table: "SubscriptionPlans",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 10, 29, 6, 24, 29, 158, DateTimeKind.Utc).AddTicks(183), new DateTime(2025, 10, 29, 6, 24, 29, 158, DateTimeKind.Utc).AddTicks(181) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultTaxPercentage",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "TaxNotes",
                table: "SubscriptionPlans");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdated",
                table: "SystemSettings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 28, 19, 53, 1, 598, DateTimeKind.Utc).AddTicks(8565),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 10, 29, 6, 24, 29, 157, DateTimeKind.Utc).AddTicks(9835));

            migrationBuilder.AlterColumn<DateTime>(
                name: "VersionCreatedDate",
                table: "SubscriptionPlans",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 28, 19, 53, 1, 523, DateTimeKind.Utc).AddTicks(1216),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 10, 29, 6, 24, 29, 58, DateTimeKind.Utc).AddTicks(4502));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 10, 28, 19, 53, 1, 598, DateTimeKind.Utc).AddTicks(8936), new DateTime(2025, 10, 28, 19, 53, 1, 598, DateTimeKind.Utc).AddTicks(8934) });
        }
    }
}
