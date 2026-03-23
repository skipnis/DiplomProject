using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wishapp.Web.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogCollections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "catalog_collections",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    occasion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    cover_image_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    order = table.Column<int>(type: "integer", nullable: false),
                    is_published = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_catalog_collections", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "catalog_collection_items",
                columns: table => new
                {
                    collection_id = table.Column<Guid>(type: "uuid", nullable: false),
                    catalog_item_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_catalog_collection_items", x => new { x.collection_id, x.catalog_item_id });
                    table.ForeignKey(
                        name: "fk_catalog_collection_items_catalog_collections_collection_id",
                        column: x => x.collection_id,
                        principalTable: "catalog_collections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_catalog_collection_items_catalog_items_catalog_item_id",
                        column: x => x.catalog_item_id,
                        principalTable: "catalog_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_catalog_collection_items_catalog_item_id",
                table: "catalog_collection_items",
                column: "catalog_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_catalog_collections_is_published",
                table: "catalog_collections",
                column: "is_published");

            migrationBuilder.CreateIndex(
                name: "ix_catalog_collections_order",
                table: "catalog_collections",
                column: "order");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "catalog_collection_items");

            migrationBuilder.DropTable(
                name: "catalog_collections");
        }
    }
}
