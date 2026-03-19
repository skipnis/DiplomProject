using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wishapp.Web.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Reservations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "wish_reservations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    wish_id = table.Column<Guid>(type: "uuid", nullable: false),
                    wishlist_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reserved_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_wish_reservations", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_wish_reservations_reserved_by_user_id",
                table: "wish_reservations",
                column: "reserved_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_wish_reservations_wish_id",
                table: "wish_reservations",
                column: "wish_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_wish_reservations_wishlist_id",
                table: "wish_reservations",
                column: "wishlist_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "wish_reservations");
        }
    }
}
