using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wishapp.Web.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddSortingIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_wishlists_owner_id_name",
                schema: "wishlists",
                table: "wishlists",
                columns: new[] { "owner_id", "name" });

            migrationBuilder.CreateIndex(
                name: "ix_wishes_wishlist_id_created_at",
                schema: "wishlists",
                table: "wishes",
                columns: new[] { "wishlist_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_wishes_wishlist_id_is_fulfilled",
                schema: "wishlists",
                table: "wishes",
                columns: new[] { "wishlist_id", "is_fulfilled" });

            migrationBuilder.CreateIndex(
                name: "ix_wishes_wishlist_id_name",
                schema: "wishlists",
                table: "wishes",
                columns: new[] { "wishlist_id", "name" });

            migrationBuilder.CreateIndex(
                name: "ix_wishes_wishlist_id_priority",
                schema: "wishlists",
                table: "wishes",
                columns: new[] { "wishlist_id", "priority" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_wishlists_owner_id_name",
                schema: "wishlists",
                table: "wishlists");

            migrationBuilder.DropIndex(
                name: "ix_wishes_wishlist_id_created_at",
                schema: "wishlists",
                table: "wishes");

            migrationBuilder.DropIndex(
                name: "ix_wishes_wishlist_id_is_fulfilled",
                schema: "wishlists",
                table: "wishes");

            migrationBuilder.DropIndex(
                name: "ix_wishes_wishlist_id_name",
                schema: "wishlists",
                table: "wishes");

            migrationBuilder.DropIndex(
                name: "ix_wishes_wishlist_id_priority",
                schema: "wishlists",
                table: "wishes");
        }
    }
}
