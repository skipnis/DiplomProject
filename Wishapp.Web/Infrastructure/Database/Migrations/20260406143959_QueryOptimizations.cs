using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wishapp.Web.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class QueryOptimizations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_wishlists_created_at",
                table: "wishlists");

            migrationBuilder.DropIndex(
                name: "ix_wishlists_owner_id",
                table: "wishlists");

            migrationBuilder.DropIndex(
                name: "ix_friendships_addressee_id",
                table: "friendships");

            migrationBuilder.DropIndex(
                name: "ix_friendships_requester_id",
                table: "friendships");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,");

            migrationBuilder.CreateIndex(
                name: "ix_wishlists_owner_id_created_at",
                table: "wishlists",
                columns: new[] { "owner_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_users_username",
                table: "users",
                column: "username")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_friendships_addressee_id_status",
                table: "friendships",
                columns: new[] { "addressee_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_friendships_requester_id_status",
                table: "friendships",
                columns: new[] { "requester_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_catalog_items_name",
                table: "catalog_items",
                column: "name")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_wishlists_owner_id_created_at",
                table: "wishlists");

            migrationBuilder.DropIndex(
                name: "ix_users_username",
                table: "users");

            migrationBuilder.DropIndex(
                name: "ix_friendships_addressee_id_status",
                table: "friendships");

            migrationBuilder.DropIndex(
                name: "ix_friendships_requester_id_status",
                table: "friendships");

            migrationBuilder.DropIndex(
                name: "ix_catalog_items_name",
                table: "catalog_items");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:pg_trgm", ",,");

            migrationBuilder.CreateIndex(
                name: "ix_wishlists_created_at",
                table: "wishlists",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_wishlists_owner_id",
                table: "wishlists",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "ix_friendships_addressee_id",
                table: "friendships",
                column: "addressee_id");

            migrationBuilder.CreateIndex(
                name: "ix_friendships_requester_id",
                table: "friendships",
                column: "requester_id");
        }
    }
}
