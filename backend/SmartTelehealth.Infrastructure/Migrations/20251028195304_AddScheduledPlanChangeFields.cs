using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartTelehealth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduledPlanChangeFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdated",
                table: "SystemSettings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 28, 19, 53, 1, 598, DateTimeKind.Utc).AddTicks(8565),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 10, 28, 8, 59, 7, 635, DateTimeKind.Utc).AddTicks(3030));

            migrationBuilder.AddColumn<string>(
                name: "PendingChangeType",
                table: "Subscriptions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PendingPlanChangeId",
                table: "Subscriptions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlanChangeEffectiveDate",
                table: "Subscriptions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "VersionCreatedDate",
                table: "SubscriptionPlans",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 28, 19, 53, 1, 523, DateTimeKind.Utc).AddTicks(1216),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 10, 28, 8, 59, 7, 478, DateTimeKind.Utc).AddTicks(4720));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 10, 28, 19, 53, 1, 598, DateTimeKind.Utc).AddTicks(8936), new DateTime(2025, 10, 28, 19, 53, 1, 598, DateTimeKind.Utc).AddTicks(8934) });

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_PendingPlanChangeId",
                table: "Subscriptions",
                column: "PendingPlanChangeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_SubscriptionPlans_PendingPlanChangeId",
                table: "Subscriptions",
                column: "PendingPlanChangeId",
                principalTable: "SubscriptionPlans",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_SubscriptionPlans_PendingPlanChangeId",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_PendingPlanChangeId",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "PendingChangeType",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "PendingPlanChangeId",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "PlanChangeEffectiveDate",
                table: "Subscriptions");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdated",
                table: "SystemSettings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 28, 8, 59, 7, 635, DateTimeKind.Utc).AddTicks(3030),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 10, 28, 19, 53, 1, 598, DateTimeKind.Utc).AddTicks(8565));

            migrationBuilder.AlterColumn<DateTime>(
                name: "VersionCreatedDate",
                table: "SubscriptionPlans",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 28, 8, 59, 7, 478, DateTimeKind.Utc).AddTicks(4720),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 10, 28, 19, 53, 1, 523, DateTimeKind.Utc).AddTicks(1216));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 10, 28, 8, 59, 7, 635, DateTimeKind.Utc).AddTicks(3981), new DateTime(2025, 10, 28, 8, 59, 7, 635, DateTimeKind.Utc).AddTicks(3978) });
        }
    }
}
