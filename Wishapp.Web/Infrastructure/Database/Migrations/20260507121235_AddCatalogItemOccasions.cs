using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wishapp.Web.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogItemOccasions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "occasion",
                schema: "catalog",
                table: "catalog_collections");

            migrationBuilder.AddColumn<Guid>(
                name: "occasion_id",
                schema: "catalog",
                table: "catalog_collections",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "catalog_item_occasions",
                schema: "catalog",
                columns: table => new
                {
                    catalog_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    occasion_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_catalog_item_occasions", x => new { x.catalog_item_id, x.occasion_id });
                    table.ForeignKey(
                        name: "fk_catalog_item_occasions_catalog_items_catalog_item_id",
                        column: x => x.catalog_item_id,
                        principalSchema: "catalog",
                        principalTable: "catalog_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_catalog_item_occasions_catalog_occasions_occasion_id",
                        column: x => x.occasion_id,
                        principalSchema: "catalog",
                        principalTable: "catalog_occasions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_catalog_collections_occasion_id",
                schema: "catalog",
                table: "catalog_collections",
                column: "occasion_id");

            migrationBuilder.CreateIndex(
                name: "ix_catalog_item_occasions_occasion_id",
                schema: "catalog",
                table: "catalog_item_occasions",
                column: "occasion_id");

            migrationBuilder.AddForeignKey(
                name: "fk_catalog_collections_catalog_occasions_occasion_id",
                schema: "catalog",
                table: "catalog_collections",
                column: "occasion_id",
                principalSchema: "catalog",
                principalTable: "catalog_occasions",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_catalog_collections_catalog_occasions_occasion_id",
                schema: "catalog",
                table: "catalog_collections");

            migrationBuilder.DropTable(
                name: "catalog_item_occasions",
                schema: "catalog");

            migrationBuilder.DropIndex(
                name: "ix_catalog_collections_occasion_id",
                schema: "catalog",
                table: "catalog_collections");

            migrationBuilder.DropColumn(
                name: "occasion_id",
                schema: "catalog",
                table: "catalog_collections");

            migrationBuilder.AddColumn<string>(
                name: "occasion",
                schema: "catalog",
                table: "catalog_collections",
                type: "text",
                maxLength: 50,
                nullable: true);
        }
    }
}
