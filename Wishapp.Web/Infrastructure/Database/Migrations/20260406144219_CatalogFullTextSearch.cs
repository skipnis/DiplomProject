using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace Wishapp.Web.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class CatalogFullTextSearch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_catalog_items_name",
                table: "catalog_items");

            migrationBuilder.AddColumn<NpgsqlTsVector>(
                name: "search_vector",
                table: "catalog_items",
                type: "tsvector",
                nullable: false,
                computedColumnSql: "to_tsvector('russian', coalesce(name, '') || ' ' || coalesce(description, ''))",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "ix_catalog_items_search_vector",
                table: "catalog_items",
                column: "search_vector")
                .Annotation("Npgsql:IndexMethod", "gin");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_catalog_items_search_vector",
                table: "catalog_items");

            migrationBuilder.DropColumn(
                name: "search_vector",
                table: "catalog_items");

            migrationBuilder.CreateIndex(
                name: "ix_catalog_items_name",
                table: "catalog_items",
                column: "name")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });
        }
    }
}
