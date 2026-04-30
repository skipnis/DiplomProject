using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Wishapp.Web.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddGamificationModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "gamification");

            migrationBuilder.CreateTable(
                name: "achievement_definitions",
                schema: "gamification",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", maxLength: 500, nullable: false),
                    emoji = table.Column<string>(type: "text", maxLength: 10, nullable: false),
                    rule_type = table.Column<int>(type: "integer", nullable: false),
                    linked_badge_type_id = table.Column<int>(type: "integer", nullable: true),
                    threshold = table.Column<int>(type: "integer", nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_achievement_definitions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "catalog_badge_definitions",
                schema: "gamification",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    emoji = table.Column<string>(type: "text", maxLength: 10, nullable: false),
                    slug = table.Column<string>(type: "text", maxLength: 50, nullable: false),
                    label = table.Column<string>(type: "text", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", maxLength: 500, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_catalog_badge_definitions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "catalog_item_badge_votes",
                schema: "gamification",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    catalog_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    badge_type = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_catalog_item_badge_votes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fulfilled_wish_badge_definitions",
                schema: "gamification",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    emoji = table.Column<string>(type: "text", maxLength: 10, nullable: false),
                    slug = table.Column<string>(type: "text", maxLength: 50, nullable: false),
                    label = table.Column<string>(type: "text", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", maxLength: 500, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fulfilled_wish_badge_definitions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fulfilled_wish_badges",
                schema: "gamification",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    wish_id = table.Column<Guid>(type: "uuid", nullable: false),
                    recipient_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    gifter_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    badge_type = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fulfilled_wish_badges", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_achievements",
                schema: "gamification",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    progress = table.Column<int>(type: "integer", nullable: false),
                    is_earned = table.Column<bool>(type: "boolean", nullable: false),
                    earned_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_achievements", x => x.id);
                });

            migrationBuilder.DropTable(
                name: "catalog_item_ratings",
                schema: "catalog");

            migrationBuilder.CreateIndex(
                name: "ix_catalog_badge_definitions_slug",
                schema: "gamification",
                table: "catalog_badge_definitions",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_catalog_item_badge_votes_catalog_item_id",
                schema: "gamification",
                table: "catalog_item_badge_votes",
                column: "catalog_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_catalog_item_badge_votes_catalog_item_id_user_id_badge_type",
                schema: "gamification",
                table: "catalog_item_badge_votes",
                columns: new[] { "catalog_item_id", "user_id", "badge_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_fulfilled_wish_badge_definitions_slug",
                schema: "gamification",
                table: "fulfilled_wish_badge_definitions",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_fulfilled_wish_badges_gifter_user_id",
                schema: "gamification",
                table: "fulfilled_wish_badges",
                column: "gifter_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_fulfilled_wish_badges_wish_id",
                schema: "gamification",
                table: "fulfilled_wish_badges",
                column: "wish_id");

            migrationBuilder.CreateIndex(
                name: "ix_fulfilled_wish_badges_wish_id_badge_type",
                schema: "gamification",
                table: "fulfilled_wish_badges",
                columns: new[] { "wish_id", "badge_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_achievements_user_id",
                schema: "gamification",
                table: "user_achievements",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_achievements_user_id_type",
                schema: "gamification",
                table: "user_achievements",
                columns: new[] { "user_id", "type" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "achievement_definitions",
                schema: "gamification");

            migrationBuilder.DropTable(
                name: "catalog_badge_definitions",
                schema: "gamification");

            migrationBuilder.DropTable(
                name: "catalog_item_badge_votes",
                schema: "gamification");

            migrationBuilder.DropTable(
                name: "fulfilled_wish_badge_definitions",
                schema: "gamification");

            migrationBuilder.DropTable(
                name: "fulfilled_wish_badges",
                schema: "gamification");

            migrationBuilder.DropTable(
                name: "user_achievements",
                schema: "gamification");

            migrationBuilder.CreateTable(
                name: "catalog_item_ratings",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    catalog_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    value = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_catalog_item_ratings", x => x.id);
                    table.CheckConstraint("CK_catalog_item_ratings_value", "value BETWEEN 1 AND 5");
                    table.ForeignKey(
                        name: "fk_catalog_item_ratings_catalog_items_catalog_item_id",
                        column: x => x.catalog_item_id,
                        principalSchema: "catalog",
                        principalTable: "catalog_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_catalog_item_ratings_catalog_item_id",
                schema: "catalog",
                table: "catalog_item_ratings",
                column: "catalog_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_catalog_item_ratings_user_id_catalog_item_id",
                schema: "catalog",
                table: "catalog_item_ratings",
                columns: new[] { "user_id", "catalog_item_id" },
                unique: true);
        }
    }
}
