using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wishapp.Web.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class RemoveNotificationDeliveryFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_notifications_status_created_at",
                schema: "notifications",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "processed_at",
                schema: "notifications",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "retry_count",
                schema: "notifications",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "status",
                schema: "notifications",
                table: "notifications");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "processed_at",
                schema: "notifications",
                table: "notifications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "retry_count",
                schema: "notifications",
                table: "notifications",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "status",
                schema: "notifications",
                table: "notifications",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "ix_notifications_status_created_at",
                schema: "notifications",
                table: "notifications",
                columns: new[] { "status", "created_at" });
        }
    }
}
