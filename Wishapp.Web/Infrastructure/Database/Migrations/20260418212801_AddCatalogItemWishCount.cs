using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wishapp.Web.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogItemWishCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "wish_count",
                schema: "catalog",
                table: "catalog_items",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "ix_catalog_items_wish_count",
                schema: "catalog",
                table: "catalog_items",
                column: "wish_count");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_catalog_items_wish_count",
                schema: "catalog",
                table: "catalog_items");

            migrationBuilder.DropColumn(
                name: "wish_count",
                schema: "catalog",
                table: "catalog_items");
        }
    }
}
