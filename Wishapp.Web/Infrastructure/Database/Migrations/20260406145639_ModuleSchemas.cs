using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace Wishapp.Web.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class ModuleSchemas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "admin");

            migrationBuilder.EnsureSchema(
                name: "users");

            migrationBuilder.EnsureSchema(
                name: "catalog");

            migrationBuilder.EnsureSchema(
                name: "events");

            migrationBuilder.EnsureSchema(
                name: "friendships");

            migrationBuilder.EnsureSchema(
                name: "reservations");

            migrationBuilder.EnsureSchema(
                name: "wishlists");

            migrationBuilder.RenameTable(
                name: "wishlists",
                newName: "wishlists",
                newSchema: "wishlists");

            migrationBuilder.RenameTable(
                name: "wishlist_members",
                newName: "wishlist_members",
                newSchema: "wishlists");

            migrationBuilder.RenameTable(
                name: "wishes",
                newName: "wishes",
                newSchema: "wishlists");

            migrationBuilder.RenameTable(
                name: "wish_reservations",
                newName: "wish_reservations",
                newSchema: "reservations");

            migrationBuilder.RenameTable(
                name: "users",
                newName: "users",
                newSchema: "users");

            migrationBuilder.RenameTable(
                name: "user_external_tokens",
                newName: "user_external_tokens",
                newSchema: "users");

            migrationBuilder.RenameTable(
                name: "refresh_tokens",
                newName: "refresh_tokens",
                newSchema: "users");

            migrationBuilder.RenameTable(
                name: "friendships",
                newName: "friendships",
                newSchema: "friendships");

            migrationBuilder.RenameTable(
                name: "events",
                newName: "events",
                newSchema: "events");

            migrationBuilder.RenameTable(
                name: "email_otps",
                newName: "email_otps",
                newSchema: "users");

            migrationBuilder.RenameTable(
                name: "catalog_items",
                newName: "catalog_items",
                newSchema: "catalog");

            migrationBuilder.RenameTable(
                name: "catalog_item_ratings",
                newName: "catalog_item_ratings",
                newSchema: "catalog");

            migrationBuilder.RenameTable(
                name: "catalog_collections",
                newName: "catalog_collections",
                newSchema: "catalog");

            migrationBuilder.RenameTable(
                name: "catalog_collection_items",
                newName: "catalog_collection_items",
                newSchema: "catalog");

            migrationBuilder.RenameTable(
                name: "catalog_categories",
                newName: "catalog_categories",
                newSchema: "catalog");

            migrationBuilder.RenameTable(
                name: "auth_identities",
                newName: "auth_identities",
                newSchema: "users");

            migrationBuilder.RenameTable(
                name: "admin_users",
                newName: "admin_users",
                newSchema: "admin");

            migrationBuilder.AlterColumn<NpgsqlTsVector>(
                name: "search_vector",
                schema: "catalog",
                table: "catalog_items",
                type: "tsvector",
                nullable: true,
                computedColumnSql: "to_tsvector('russian', coalesce(name, '') || ' ' || coalesce(description, ''))",
                stored: true,
                oldClrType: typeof(NpgsqlTsVector),
                oldType: "tsvector",
                oldComputedColumnSql: "to_tsvector('russian', coalesce(name, '') || ' ' || coalesce(description, ''))",
                oldStored: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "wishlists",
                schema: "wishlists",
                newName: "wishlists");

            migrationBuilder.RenameTable(
                name: "wishlist_members",
                schema: "wishlists",
                newName: "wishlist_members");

            migrationBuilder.RenameTable(
                name: "wishes",
                schema: "wishlists",
                newName: "wishes");

            migrationBuilder.RenameTable(
                name: "wish_reservations",
                schema: "reservations",
                newName: "wish_reservations");

            migrationBuilder.RenameTable(
                name: "users",
                schema: "users",
                newName: "users");

            migrationBuilder.RenameTable(
                name: "user_external_tokens",
                schema: "users",
                newName: "user_external_tokens");

            migrationBuilder.RenameTable(
                name: "refresh_tokens",
                schema: "users",
                newName: "refresh_tokens");

            migrationBuilder.RenameTable(
                name: "friendships",
                schema: "friendships",
                newName: "friendships");

            migrationBuilder.RenameTable(
                name: "events",
                schema: "events",
                newName: "events");

            migrationBuilder.RenameTable(
                name: "email_otps",
                schema: "users",
                newName: "email_otps");

            migrationBuilder.RenameTable(
                name: "catalog_items",
                schema: "catalog",
                newName: "catalog_items");

            migrationBuilder.RenameTable(
                name: "catalog_item_ratings",
                schema: "catalog",
                newName: "catalog_item_ratings");

            migrationBuilder.RenameTable(
                name: "catalog_collections",
                schema: "catalog",
                newName: "catalog_collections");

            migrationBuilder.RenameTable(
                name: "catalog_collection_items",
                schema: "catalog",
                newName: "catalog_collection_items");

            migrationBuilder.RenameTable(
                name: "catalog_categories",
                schema: "catalog",
                newName: "catalog_categories");

            migrationBuilder.RenameTable(
                name: "auth_identities",
                schema: "users",
                newName: "auth_identities");

            migrationBuilder.RenameTable(
                name: "admin_users",
                schema: "admin",
                newName: "admin_users");

            migrationBuilder.AlterColumn<NpgsqlTsVector>(
                name: "search_vector",
                table: "catalog_items",
                type: "tsvector",
                nullable: false,
                computedColumnSql: "to_tsvector('russian', coalesce(name, '') || ' ' || coalesce(description, ''))",
                stored: true,
                oldClrType: typeof(NpgsqlTsVector),
                oldType: "tsvector",
                oldNullable: true,
                oldComputedColumnSql: "to_tsvector('russian', coalesce(name, '') || ' ' || coalesce(description, ''))",
                oldStored: true);
        }
    }
}
