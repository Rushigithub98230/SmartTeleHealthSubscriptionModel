using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartTelehealth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixApplicationLogsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdated",
                table: "SystemSettings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 27, 19, 55, 33, 394, DateTimeKind.Utc).AddTicks(4906),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 10, 27, 13, 1, 11, 51, DateTimeKind.Utc).AddTicks(3924));

            migrationBuilder.AlterColumn<DateTime>(
                name: "VersionCreatedDate",
                table: "SubscriptionPlans",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 27, 19, 55, 33, 263, DateTimeKind.Utc).AddTicks(3378),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 10, 27, 13, 1, 11, 0, DateTimeKind.Utc).AddTicks(1401));

            // Drop existing ApplicationLogs table if it exists
            migrationBuilder.DropTable(
                name: "ApplicationLogs");

            // Create ApplicationLogs table with proper schema
            migrationBuilder.CreateTable(
                name: "ApplicationLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LogLevel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Source = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Exception = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    Operation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AdditionalData = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CorrelationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("PK_ApplicationLogs", x => x.Id);
                    // Foreign key to AspNetUsers removed - logs are independent
                    // UserId is stored for reference only, no referential integrity needed
                });

            // Create indexes for performance
            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLogs_Timestamp",
                table: "ApplicationLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLogs_LogLevel",
                table: "ApplicationLogs",
                column: "LogLevel");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLogs_Source",
                table: "ApplicationLogs",
                column: "Source");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLogs_UserId",
                table: "ApplicationLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLogs_Timestamp_LogLevel",
                table: "ApplicationLogs",
                columns: new[] { "Timestamp", "LogLevel" });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLogs_Source_LogLevel",
                table: "ApplicationLogs",
                columns: new[] { "Source", "LogLevel" });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLogs_CorrelationId",
                table: "ApplicationLogs",
                column: "CorrelationId");

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 10, 27, 19, 55, 33, 394, DateTimeKind.Utc).AddTicks(5388), new DateTime(2025, 10, 27, 19, 55, 33, 394, DateTimeKind.Utc).AddTicks(5386) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdated",
                table: "SystemSettings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 27, 13, 1, 11, 51, DateTimeKind.Utc).AddTicks(3924),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 10, 27, 19, 55, 33, 394, DateTimeKind.Utc).AddTicks(4906));

            migrationBuilder.AlterColumn<DateTime>(
                name: "VersionCreatedDate",
                table: "SubscriptionPlans",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 27, 13, 1, 11, 0, DateTimeKind.Utc).AddTicks(1401),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 10, 27, 19, 55, 33, 263, DateTimeKind.Utc).AddTicks(3378));

            // Drop the ApplicationLogs table
            migrationBuilder.DropTable(
                name: "ApplicationLogs");

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 10, 27, 13, 1, 11, 51, DateTimeKind.Utc).AddTicks(4453), new DateTime(2025, 10, 27, 13, 1, 11, 51, DateTimeKind.Utc).AddTicks(4451) });
        }
    }
}
