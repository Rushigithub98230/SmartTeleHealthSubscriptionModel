using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartTelehealth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixAnalyticsAndUserEntityChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminCommissionFixed",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "DiscountedPrice",
                table: "SubscriptionPlans");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "SubscriptionPlans",
                newName: "BasePrice");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdated",
                table: "SystemSettings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 27, 13, 1, 11, 51, DateTimeKind.Utc).AddTicks(3924),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 10, 21, 21, 55, 13, 228, DateTimeKind.Utc).AddTicks(2496));

            migrationBuilder.AlterColumn<DateTime>(
                name: "VersionCreatedDate",
                table: "SubscriptionPlans",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 27, 13, 1, 11, 0, DateTimeKind.Utc).AddTicks(1401),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 10, 21, 21, 55, 13, 134, DateTimeKind.Utc).AddTicks(20));

            migrationBuilder.AddColumn<decimal>(
                name: "BillingDiscountPercentage",
                table: "SubscriptionPlans",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPercentage",
                table: "SubscriptionPlans",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ApplicationLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LogLevel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Source = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Exception = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    Operation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AdditionalData = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CorrelationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationLogs_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ApplicationLogs_Users_DeletedBy",
                        column: x => x.DeletedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ApplicationLogs_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ApplicationLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 10, 27, 13, 1, 11, 51, DateTimeKind.Utc).AddTicks(4453), new DateTime(2025, 10, 27, 13, 1, 11, 51, DateTimeKind.Utc).AddTicks(4451) });

            migrationBuilder.CreateIndex(
                name: "UK_User_Plan_Active",
                table: "Subscriptions",
                columns: new[] { "UserId", "SubscriptionPlanId" },
                unique: true,
                filter: "Status IN ('Active', 'Paused')");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLogs_CorrelationId",
                table: "ApplicationLogs",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLogs_CreatedBy",
                table: "ApplicationLogs",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLogs_DeletedBy",
                table: "ApplicationLogs",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLogs_LogLevel",
                table: "ApplicationLogs",
                column: "LogLevel");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLogs_Source",
                table: "ApplicationLogs",
                column: "Source");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLogs_Source_LogLevel",
                table: "ApplicationLogs",
                columns: new[] { "Source", "LogLevel" });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLogs_Timestamp",
                table: "ApplicationLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLogs_Timestamp_LogLevel",
                table: "ApplicationLogs",
                columns: new[] { "Timestamp", "LogLevel" });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLogs_UpdatedBy",
                table: "ApplicationLogs",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLogs_UserId",
                table: "ApplicationLogs",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationLogs");

            migrationBuilder.DropIndex(
                name: "UK_User_Plan_Active",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "BillingDiscountPercentage",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "DiscountPercentage",
                table: "SubscriptionPlans");

            migrationBuilder.RenameColumn(
                name: "BasePrice",
                table: "SubscriptionPlans",
                newName: "Price");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdated",
                table: "SystemSettings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 21, 21, 55, 13, 228, DateTimeKind.Utc).AddTicks(2496),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 10, 27, 13, 1, 11, 51, DateTimeKind.Utc).AddTicks(3924));

            migrationBuilder.AlterColumn<DateTime>(
                name: "VersionCreatedDate",
                table: "SubscriptionPlans",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 21, 21, 55, 13, 134, DateTimeKind.Utc).AddTicks(20),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 10, 27, 13, 1, 11, 0, DateTimeKind.Utc).AddTicks(1401));

            migrationBuilder.AddColumn<decimal>(
                name: "AdminCommissionFixed",
                table: "SubscriptionPlans",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountedPrice",
                table: "SubscriptionPlans",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 10, 21, 21, 55, 13, 228, DateTimeKind.Utc).AddTicks(2835), new DateTime(2025, 10, 21, 21, 55, 13, 228, DateTimeKind.Utc).AddTicks(2832) });
        }
    }
}
