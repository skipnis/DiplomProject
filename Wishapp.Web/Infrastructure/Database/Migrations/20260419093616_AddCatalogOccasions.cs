using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wishapp.Web.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogOccasions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "catalog_occasions",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    key = table.Column<string>(type: "text", maxLength: 50, nullable: false),
                    label = table.Column<string>(type: "text", maxLength: 100, nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_catalog_occasions", x => x.id);
                    table.CheckConstraint("CK_catalog_occasions_key_not_empty", "trim(key) <> ''");
                    table.CheckConstraint("CK_catalog_occasions_label_not_empty", "trim(label) <> ''");
                });

            migrationBuilder.CreateIndex(
                name: "ix_catalog_occasions_key",
                schema: "catalog",
                table: "catalog_occasions",
                column: "key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_catalog_occasions_order",
                schema: "catalog",
                table: "catalog_occasions",
                column: "order");

            migrationBuilder.InsertData(
                schema: "catalog",
                table: "catalog_occasions",
                columns: ["id", "key", "label", "order"],
                values: new object[,]
                {
                    { Guid.NewGuid(), "birthday",    "🎂 День рождения",      1 },
                    { Guid.NewGuid(), "new_year",    "🎆 Новый год",          2 },
                    { Guid.NewGuid(), "valentine",   "💝 День влюблённых",    3 },
                    { Guid.NewGuid(), "wedding",     "💍 Свадьба",            4 },
                    { Guid.NewGuid(), "anniversary", "🥂 Юбилей",             5 },
                    { Guid.NewGuid(), "graduation",  "🎓 Выпускной",          6 },
                    { Guid.NewGuid(), "baby_shower", "👶 Рождение ребёнка",   7 },
                    { Guid.NewGuid(), "housewarming","🏠 Новоселье",          8 },
                    { Guid.NewGuid(), "christmas",   "🎄 Рождество",          9 },
                    { Guid.NewGuid(), "easter",      "🐣 Пасха",              10 },
                    { Guid.NewGuid(), "other",       "🎁 Другое",             11 },
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "catalog_occasions",
                schema: "catalog");
        }
    }
}
