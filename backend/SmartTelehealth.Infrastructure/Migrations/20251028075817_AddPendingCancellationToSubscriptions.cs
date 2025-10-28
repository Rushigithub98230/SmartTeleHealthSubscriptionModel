using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartTelehealth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPendingCancellationToSubscriptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdated",
                table: "SystemSettings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 28, 7, 58, 14, 16, DateTimeKind.Utc).AddTicks(3860),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 10, 27, 19, 55, 33, 394, DateTimeKind.Utc).AddTicks(4906));

            migrationBuilder.AddColumn<bool>(
                name: "PendingCancellationAtRenewal",
                table: "Subscriptions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PendingCancellationReason",
                table: "Subscriptions",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "VersionCreatedDate",
                table: "SubscriptionPlans",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 28, 7, 58, 13, 966, DateTimeKind.Utc).AddTicks(3095),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 10, 27, 19, 55, 33, 263, DateTimeKind.Utc).AddTicks(3378));

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "ApplicationLogs",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 10, 28, 7, 58, 14, 16, DateTimeKind.Utc).AddTicks(4133), new DateTime(2025, 10, 28, 7, 58, 14, 16, DateTimeKind.Utc).AddTicks(4132) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PendingCancellationAtRenewal",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "PendingCancellationReason",
                table: "Subscriptions");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdated",
                table: "SystemSettings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 27, 19, 55, 33, 394, DateTimeKind.Utc).AddTicks(4906),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 10, 28, 7, 58, 14, 16, DateTimeKind.Utc).AddTicks(3860));

            migrationBuilder.AlterColumn<DateTime>(
                name: "VersionCreatedDate",
                table: "SubscriptionPlans",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 27, 19, 55, 33, 263, DateTimeKind.Utc).AddTicks(3378),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 10, 28, 7, 58, 13, 966, DateTimeKind.Utc).AddTicks(3095));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ApplicationLogs",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 10, 27, 19, 55, 33, 394, DateTimeKind.Utc).AddTicks(5388), new DateTime(2025, 10, 27, 19, 55, 33, 394, DateTimeKind.Utc).AddTicks(5386) });
        }
    }
}
