using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wishapp.Web.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogRatingsAndPriorityRename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "catalog_item_ratings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    catalog_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    value = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_catalog_item_ratings", x => x.id);
                    table.CheckConstraint("CK_catalog_item_ratings_value", "value BETWEEN 1 AND 5");
                    table.ForeignKey(
                        name: "fk_catalog_item_ratings_catalog_items_catalog_item_id",
                        column: x => x.catalog_item_id,
                        principalTable: "catalog_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_catalog_item_ratings_catalog_item_id",
                table: "catalog_item_ratings",
                column: "catalog_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_catalog_item_ratings_user_id_catalog_item_id",
                table: "catalog_item_ratings",
                columns: new[] { "user_id", "catalog_item_id" },
                unique: true);

            migrationBuilder.Sql("UPDATE wishes SET priority = 'NiceToHave' WHERE priority = 'Low'");
            migrationBuilder.Sql("UPDATE wishes SET priority = 'Want' WHERE priority = 'Medium'");
            migrationBuilder.Sql("UPDATE wishes SET priority = 'ReallyWant' WHERE priority = 'High'");

            migrationBuilder.Sql("UPDATE wishlists SET name = 'Скрытые' WHERE name = 'Hidden' AND is_system = true");
            migrationBuilder.Sql("UPDATE wishlists SET name = 'Чёрный список' WHERE name = 'Blacklist' AND is_system = true");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "catalog_item_ratings");
        }
    }
}
