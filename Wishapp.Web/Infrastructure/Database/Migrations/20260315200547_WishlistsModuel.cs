using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wishapp.Web.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class WishlistsModuel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "wishlists",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    emoji = table.Column<string>(type: "text", nullable: true),
                    visibility = table.Column<string>(type: "text", nullable: false),
                    is_system = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_wishlists", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "wishes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    wishlist_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    currency = table.Column<string>(type: "text", nullable: true),
                    priority = table.Column<string>(type: "text", nullable: false),
                    url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    image_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_wishes", x => x.id);
                    table.ForeignKey(
                        name: "fk_wishes_wishlists_wishlist_id",
                        column: x => x.wishlist_id,
                        principalTable: "wishlists",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "wishlist_members",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    wishlist_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role = table.Column<string>(type: "text", nullable: false),
                    custom_role_name = table.Column<string>(type: "text", nullable: true),
                    joined_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_wishlist_members", x => x.id);
                    table.ForeignKey(
                        name: "fk_wishlist_members_wishlists_wishlist_id",
                        column: x => x.wishlist_id,
                        principalTable: "wishlists",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_wishes_created_at",
                table: "wishes",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_wishes_wishlist_id",
                table: "wishes",
                column: "wishlist_id");

            migrationBuilder.CreateIndex(
                name: "ix_wishlist_members_user_id",
                table: "wishlist_members",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_wishlist_members_wishlist_id_user_id",
                table: "wishlist_members",
                columns: new[] { "wishlist_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_wishlists_created_at",
                table: "wishlists",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_wishlists_owner_id",
                table: "wishlists",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "ix_wishlists_visibility",
                table: "wishlists",
                column: "visibility");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "wishes");

            migrationBuilder.DropTable(
                name: "wishlist_members");

            migrationBuilder.DropTable(
                name: "wishlists");
        }
    }
}
