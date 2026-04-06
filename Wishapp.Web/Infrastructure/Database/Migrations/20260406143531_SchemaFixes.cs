using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wishapp.Web.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class SchemaFixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "fulfilled_by_user_id",
                table: "wishes",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "updated_at",
                table: "wishes",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.CreateIndex(
                name: "ix_wishes_updated_at",
                table: "wishes",
                column: "updated_at");

            migrationBuilder.AddCheckConstraint(
                name: "CK_friendships_no_self",
                table: "friendships",
                sql: "requester_id <> addressee_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_wishes_updated_at",
                table: "wishes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_friendships_no_self",
                table: "friendships");

            migrationBuilder.DropColumn(
                name: "fulfilled_by_user_id",
                table: "wishes");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "wishes");
        }
    }
}
