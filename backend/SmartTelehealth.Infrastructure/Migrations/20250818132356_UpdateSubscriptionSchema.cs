using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartTelehealth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSubscriptionSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "Subscriptions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                table: "Subscriptions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Subscriptions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiredAt",
                table: "Subscriptions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiryDate",
                table: "Subscriptions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RenewedAt",
                table: "Subscriptions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubscriptionStatus",
                table: "Subscriptions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Amount",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "ExpiredAt",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "ExpiryDate",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "RenewedAt",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "SubscriptionStatus",
                table: "Subscriptions");
        }
    }
}
