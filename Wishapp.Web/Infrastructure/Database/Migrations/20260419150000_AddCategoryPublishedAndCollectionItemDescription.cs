using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wishapp.Web.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryPublishedAndCollectionItemDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_published",
                schema: "catalog",
                table: "catalog_categories",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "description",
                schema: "catalog",
                table: "catalog_collection_items",
                type: "text",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_catalog_categories_is_published",
                schema: "catalog",
                table: "catalog_categories",
                column: "is_published");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_catalog_categories_is_published",
                schema: "catalog",
                table: "catalog_categories");

            migrationBuilder.DropColumn(
                name: "is_published",
                schema: "catalog",
                table: "catalog_categories");

            migrationBuilder.DropColumn(
                name: "description",
                schema: "catalog",
                table: "catalog_collection_items");
        }
    }
}
