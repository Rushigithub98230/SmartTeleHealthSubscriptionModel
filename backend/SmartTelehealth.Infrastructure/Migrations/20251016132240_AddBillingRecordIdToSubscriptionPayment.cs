using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartTelehealth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBillingRecordIdToSubscriptionPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionPayments_BillingRecords_BillingRecordId",
                table: "SubscriptionPayments");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionPlanPrivileges_MasterUsagePeriods_UsagePeriodId",
                table: "SubscriptionPlanPrivileges");

            migrationBuilder.DropTable(
                name: "MasterUsagePeriods");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionPayments_BillingPeriodEnd",
                table: "SubscriptionPayments");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionPayments_BillingPeriodStart",
                table: "SubscriptionPayments");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionPayments_LastAttemptDate",
                table: "SubscriptionPayments");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionPayments_NextRetryAt",
                table: "SubscriptionPayments");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionPayments_PaymentMethodId",
                table: "SubscriptionPayments");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionPayments_StripeChargeId",
                table: "SubscriptionPayments");

            migrationBuilder.DropColumn(
                name: "LastRenewalDate",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "YearlyLimit",
                table: "SubscriptionPlanPrivileges");

            migrationBuilder.DropColumn(
                name: "FailureCode",
                table: "SubscriptionPayments");

            migrationBuilder.DropColumn(
                name: "LastAttemptDate",
                table: "SubscriptionPayments");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "SubscriptionPayments");

            migrationBuilder.DropColumn(
                name: "PaymentMethodId",
                table: "SubscriptionPayments");

            migrationBuilder.DropColumn(
                name: "StripeChargeId",
                table: "SubscriptionPayments");

            migrationBuilder.DropColumn(
                name: "StripeSubscriptionId",
                table: "SubscriptionPayments");

            migrationBuilder.DropColumn(
                name: "RefundAmount",
                table: "BillingRecords");

            migrationBuilder.DropColumn(
                name: "RefundReason",
                table: "BillingRecords");

            migrationBuilder.DropColumn(
                name: "RefundedAt",
                table: "BillingRecords");

            migrationBuilder.AddColumn<decimal>(
                name: "AdminCommissionFixed",
                table: "SubscriptionPlans",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AdminCommissionPercent",
                table: "SubscriptionPlans",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAutoCalculatedPrice",
                table: "SubscriptionPlans",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLatestVersion",
                table: "SubscriptionPlans",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ParentPlanId",
                table: "SubscriptionPlans",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PriceChangeNoticeDays",
                table: "SubscriptionPlans",
                type: "int",
                nullable: false,
                defaultValue: 10);

            migrationBuilder.AddColumn<decimal>(
                name: "PrivilegesTotalCost",
                table: "SubscriptionPlans",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "VersionCreatedDate",
                table: "SubscriptionPlans",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 16, 13, 22, 35, 989, DateTimeKind.Utc).AddTicks(4037));

            migrationBuilder.AddColumn<int>(
                name: "VersionNumber",
                table: "SubscriptionPlans",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AlterColumn<decimal>(
                name: "UnitCost",
                table: "SubscriptionPlanPrivileges",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AddColumn<decimal>(
                name: "PrivilegeBaseCost",
                table: "SubscriptionPlanPrivileges",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            // Step 1: Add column as NULLABLE first (to handle existing data)
            migrationBuilder.AddColumn<Guid>(
                name: "BillingRecordId",
                table: "SubscriptionPayments",
                type: "uniqueidentifier",
                nullable: true);
            
            // Step 2: DATA MIGRATION - Link existing SubscriptionPayments to BillingRecords
            migrationBuilder.Sql(@"
                -- Match by Stripe Payment Intent ID (most reliable)
                UPDATE sp
                SET sp.BillingRecordId = br.Id
                FROM SubscriptionPayments sp
                INNER JOIN BillingRecords br ON 
                    sp.StripePaymentIntentId = br.StripePaymentIntentId
                    AND sp.SubscriptionId = br.SubscriptionId
                    AND br.Type = 0  -- BillingType.Subscription
                WHERE sp.BillingRecordId IS NULL
                    AND sp.StripePaymentIntentId IS NOT NULL;
                
                -- Match by Subscription + Date proximity (for records without Stripe ID)
                UPDATE sp
                SET sp.BillingRecordId = (
                    SELECT TOP 1 br.Id
                    FROM BillingRecords br
                    WHERE br.SubscriptionId = sp.SubscriptionId
                        AND br.Type = 0  -- BillingType.Subscription
                        AND ABS(DATEDIFF(MINUTE, br.CreatedDate, sp.CreatedDate)) <= 60
                    ORDER BY br.CreatedDate DESC
                )
                FROM SubscriptionPayments sp
                WHERE sp.BillingRecordId IS NULL;
            ");
            
            // Step 3: Handle orphaned records (delete orphaned records)
            migrationBuilder.Sql(@"
                DELETE FROM SubscriptionPayments 
                WHERE BillingRecordId IS NULL;
            ");
            
            // Step 4: Make column REQUIRED (NOT NULL)
            migrationBuilder.AlterColumn<Guid>(
                name: "BillingRecordId",
                table: "SubscriptionPayments",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "ScheduledPlanMigrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NotificationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ScheduledMigrationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Pending"),
                    UserDecision = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UserDecisionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DowngradeToPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_ScheduledPlanMigrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduledPlanMigrations_SubscriptionPlans_FromPlanId",
                        column: x => x.FromPlanId,
                        principalTable: "SubscriptionPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScheduledPlanMigrations_SubscriptionPlans_ToPlanId",
                        column: x => x.ToPlanId,
                        principalTable: "SubscriptionPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScheduledPlanMigrations_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScheduledPlanMigrations_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScheduledPlanMigrations_Users_DeletedBy",
                        column: x => x.DeletedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScheduledPlanMigrations_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DefaultAdminCommissionPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 20m),
                    DefaultPriceChangeNoticeDays = table.Column<int>(type: "int", nullable: false, defaultValue: 10),
                    MaxFailedPaymentAttempts = table.Column<int>(type: "int", nullable: false, defaultValue: 3),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValue: new DateTime(2025, 10, 16, 13, 22, 36, 52, DateTimeKind.Utc).AddTicks(5330)),
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
                    table.PrimaryKey("PK_SystemSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SystemSettings_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SystemSettings_Users_DeletedBy",
                        column: x => x.DeletedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SystemSettings_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "SystemSettings",
                columns: new[] { "Id", "CreatedBy", "CreatedDate", "DefaultAdminCommissionPercent", "DefaultPriceChangeNoticeDays", "DeletedBy", "DeletedDate", "IsActive", "LastUpdated", "MaxFailedPaymentAttempts", "UpdatedBy", "UpdatedDate" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000001"), 0, new DateTime(2025, 10, 16, 13, 22, 36, 52, DateTimeKind.Utc).AddTicks(5683), 20m, 10, null, null, true, new DateTime(2025, 10, 16, 13, 22, 36, 52, DateTimeKind.Utc).AddTicks(5680), 3, null, null });

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlans_IsLatestVersion",
                table: "SubscriptionPlans",
                column: "IsLatestVersion");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlans_ParentPlanId",
                table: "SubscriptionPlans",
                column: "ParentPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlans_ParentPlanId_VersionNumber",
                table: "SubscriptionPlans",
                columns: new[] { "ParentPlanId", "VersionNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledPlanMigrations_CreatedBy",
                table: "ScheduledPlanMigrations",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledPlanMigrations_DeletedBy",
                table: "ScheduledPlanMigrations",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledPlanMigrations_FromPlanId",
                table: "ScheduledPlanMigrations",
                column: "FromPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledPlanMigrations_ScheduledMigrationDate",
                table: "ScheduledPlanMigrations",
                column: "ScheduledMigrationDate");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledPlanMigrations_Status",
                table: "ScheduledPlanMigrations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledPlanMigrations_Status_ScheduledMigrationDate",
                table: "ScheduledPlanMigrations",
                columns: new[] { "Status", "ScheduledMigrationDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledPlanMigrations_SubscriptionId",
                table: "ScheduledPlanMigrations",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledPlanMigrations_ToPlanId",
                table: "ScheduledPlanMigrations",
                column: "ToPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledPlanMigrations_UpdatedBy",
                table: "ScheduledPlanMigrations",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SystemSettings_CreatedBy",
                table: "SystemSettings",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SystemSettings_DeletedBy",
                table: "SystemSettings",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SystemSettings_UpdatedBy",
                table: "SystemSettings",
                column: "UpdatedBy");

            // Step 5: Create index for performance
            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPayments_BillingRecordId",
                table: "SubscriptionPayments",
                column: "BillingRecordId",
                unique: false);
            
            // Step 6: Add performance indexes
            migrationBuilder.Sql(@"
                CREATE INDEX IX_SubscriptionPayments_NextRetryAt 
                ON SubscriptionPayments (NextRetryAt, Status)
                WHERE Status = 2 AND NextRetryAt IS NOT NULL;

                CREATE INDEX IX_SubscriptionPayments_CreatedDate 
                ON SubscriptionPayments (CreatedDate DESC);
            ");

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionPayments_BillingRecords_BillingRecordId",
                table: "SubscriptionPayments",
                column: "BillingRecordId",
                principalTable: "BillingRecords",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionPlanPrivileges_MasterBillingCycles_UsagePeriodId",
                table: "SubscriptionPlanPrivileges",
                column: "UsagePeriodId",
                principalTable: "MasterBillingCycles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionPlans_SubscriptionPlans_ParentPlanId",
                table: "SubscriptionPlans",
                column: "ParentPlanId",
                principalTable: "SubscriptionPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionPayments_BillingRecords_BillingRecordId",
                table: "SubscriptionPayments");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionPlanPrivileges_MasterBillingCycles_UsagePeriodId",
                table: "SubscriptionPlanPrivileges");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionPlans_SubscriptionPlans_ParentPlanId",
                table: "SubscriptionPlans");

            migrationBuilder.DropTable(
                name: "ScheduledPlanMigrations");

            migrationBuilder.DropTable(
                name: "SystemSettings");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionPlans_IsLatestVersion",
                table: "SubscriptionPlans");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionPlans_ParentPlanId",
                table: "SubscriptionPlans");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionPlans_ParentPlanId_VersionNumber",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "AdminCommissionFixed",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "AdminCommissionPercent",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "IsAutoCalculatedPrice",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "IsLatestVersion",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "ParentPlanId",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "PriceChangeNoticeDays",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "PrivilegesTotalCost",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "VersionCreatedDate",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "VersionNumber",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "PrivilegeBaseCost",
                table: "SubscriptionPlanPrivileges");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastRenewalDate",
                table: "Subscriptions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "UnitCost",
                table: "SubscriptionPlanPrivileges",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldDefaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "YearlyLimit",
                table: "SubscriptionPlanPrivileges",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "BillingRecordId",
                table: "SubscriptionPayments",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "FailureCode",
                table: "SubscriptionPayments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastAttemptDate",
                table: "SubscriptionPayments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "SubscriptionPayments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethodId",
                table: "SubscriptionPayments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StripeChargeId",
                table: "SubscriptionPayments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StripeSubscriptionId",
                table: "SubscriptionPayments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RefundAmount",
                table: "BillingRecords",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RefundReason",
                table: "BillingRecords",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RefundedAt",
                table: "BillingRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MasterUsagePeriods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DurationInDays = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MasterUsagePeriods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MasterUsagePeriods_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MasterUsagePeriods_Users_DeletedBy",
                        column: x => x.DeletedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MasterUsagePeriods_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPayments_BillingPeriodEnd",
                table: "SubscriptionPayments",
                column: "BillingPeriodEnd");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPayments_BillingPeriodStart",
                table: "SubscriptionPayments",
                column: "BillingPeriodStart");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPayments_LastAttemptDate",
                table: "SubscriptionPayments",
                column: "LastAttemptDate");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPayments_NextRetryAt",
                table: "SubscriptionPayments",
                column: "NextRetryAt");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPayments_PaymentMethodId",
                table: "SubscriptionPayments",
                column: "PaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPayments_StripeChargeId",
                table: "SubscriptionPayments",
                column: "StripeChargeId");

            migrationBuilder.CreateIndex(
                name: "IX_MasterUsagePeriods_CreatedBy",
                table: "MasterUsagePeriods",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MasterUsagePeriods_DeletedBy",
                table: "MasterUsagePeriods",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MasterUsagePeriods_DurationInDays",
                table: "MasterUsagePeriods",
                column: "DurationInDays");

            migrationBuilder.CreateIndex(
                name: "IX_MasterUsagePeriods_Name",
                table: "MasterUsagePeriods",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MasterUsagePeriods_SortOrder",
                table: "MasterUsagePeriods",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_MasterUsagePeriods_UpdatedBy",
                table: "MasterUsagePeriods",
                column: "UpdatedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionPayments_BillingRecords_BillingRecordId",
                table: "SubscriptionPayments",
                column: "BillingRecordId",
                principalTable: "BillingRecords",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionPlanPrivileges_MasterUsagePeriods_UsagePeriodId",
                table: "SubscriptionPlanPrivileges",
                column: "UsagePeriodId",
                principalTable: "MasterUsagePeriods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
