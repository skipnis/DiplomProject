using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wishapp.Web.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddFulfilledWishRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "fulfilled_wish_records",
                schema: "wishlists",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    wish_id = table.Column<Guid>(type: "uuid", nullable: false),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    gifter_id = table.Column<Guid>(type: "uuid", nullable: true),
                    wish_name = table.Column<string>(type: "text", maxLength: 200, nullable: false),
                    wish_description = table.Column<string>(type: "text", maxLength: 1000, nullable: true),
                    price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    currency = table.Column<string>(type: "text", nullable: true),
                    image_path = table.Column<string>(type: "text", maxLength: 500, nullable: true),
                    wishlist_name = table.Column<string>(type: "text", maxLength: 200, nullable: false),
                    fulfilled_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fulfilled_wish_records", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_fulfilled_wish_records_fulfilled_at",
                schema: "wishlists",
                table: "fulfilled_wish_records",
                column: "fulfilled_at");

            migrationBuilder.CreateIndex(
                name: "ix_fulfilled_wish_records_owner_id",
                schema: "wishlists",
                table: "fulfilled_wish_records",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "ix_fulfilled_wish_records_wish_id",
                schema: "wishlists",
                table: "fulfilled_wish_records",
                column: "wish_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fulfilled_wish_records",
                schema: "wishlists");
        }
    }
}
