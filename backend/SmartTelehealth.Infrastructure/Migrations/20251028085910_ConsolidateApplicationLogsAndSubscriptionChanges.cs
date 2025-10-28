using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartTelehealth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConsolidateApplicationLogsAndSubscriptionChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdated",
                table: "SystemSettings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 28, 8, 59, 7, 635, DateTimeKind.Utc).AddTicks(3030),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 10, 27, 19, 55, 33, 394, DateTimeKind.Utc).AddTicks(4906));

            // Check and add PendingCancellationAtRenewal column if it doesn't exist
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Subscriptions') AND name = 'PendingCancellationAtRenewal')
                BEGIN
                    ALTER TABLE [Subscriptions] ADD [PendingCancellationAtRenewal] bit NOT NULL DEFAULT 0;
                END
            ");

            // Check and add PendingCancellationReason column if it doesn't exist
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Subscriptions') AND name = 'PendingCancellationReason')
                BEGIN
                    ALTER TABLE [Subscriptions] ADD [PendingCancellationReason] nvarchar(500) NULL;
                END
            ");

            migrationBuilder.AlterColumn<DateTime>(
                name: "VersionCreatedDate",
                table: "SubscriptionPlans",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 28, 8, 59, 7, 478, DateTimeKind.Utc).AddTicks(4720),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 10, 27, 19, 55, 33, 263, DateTimeKind.Utc).AddTicks(3378));

            // Drop and recreate ApplicationLogs table to fix Id column (Guid -> long with IDENTITY)
            migrationBuilder.Sql(@"
                IF OBJECT_ID('ApplicationLogs', 'U') IS NOT NULL
                BEGIN
                    DROP TABLE [ApplicationLogs];
                END
            ");

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
                    AdditionalData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CorrelationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
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
                    table.ForeignKey(
                        name: "FK_ApplicationLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

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

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 10, 28, 8, 59, 7, 635, DateTimeKind.Utc).AddTicks(3981), new DateTime(2025, 10, 28, 8, 59, 7, 635, DateTimeKind.Utc).AddTicks(3978) });
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
                oldDefaultValue: new DateTime(2025, 10, 28, 8, 59, 7, 635, DateTimeKind.Utc).AddTicks(3030));

            migrationBuilder.AlterColumn<DateTime>(
                name: "VersionCreatedDate",
                table: "SubscriptionPlans",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 27, 19, 55, 33, 263, DateTimeKind.Utc).AddTicks(3378),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 10, 28, 8, 59, 7, 478, DateTimeKind.Utc).AddTicks(4720));

            migrationBuilder.AlterColumn<string>(
                name: "AdditionalData",
                table: "ApplicationLogs",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldMaxLength: 2000,
                oldNullable: true);

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
