using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wishapp.Web.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddWishShareToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "url",
                table: "wishes",
                type: "text",
                maxLength: 2048,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2048)",
                oldMaxLength: 2048,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "wishes",
                type: "text",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "image_path",
                table: "wishes",
                type: "text",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "wishes",
                type: "text",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "share_token",
                table: "wishes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "scope",
                table: "user_external_tokens",
                type: "text",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "provider",
                table: "user_external_tokens",
                type: "text",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "url",
                table: "catalog_items",
                type: "text",
                maxLength: 2048,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2048)",
                oldMaxLength: 2048,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "catalog_items",
                type: "text",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "image_path",
                table: "catalog_items",
                type: "text",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "catalog_items",
                type: "text",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "occasion",
                table: "catalog_collections",
                type: "text",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "catalog_collections",
                type: "text",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "catalog_collections",
                type: "text",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "cover_image_path",
                table: "catalog_collections",
                type: "text",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "catalog_categories",
                type: "text",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "username",
                table: "admin_users",
                type: "text",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddCheckConstraint(
                name: "CK_wishlists_name_not_empty",
                table: "wishlists",
                sql: "trim(name) <> ''");

            migrationBuilder.CreateIndex(
                name: "ix_wishes_share_token",
                table: "wishes",
                column: "share_token",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_wishes_name_not_empty",
                table: "wishes",
                sql: "trim(name) <> ''");

            migrationBuilder.AddCheckConstraint(
                name: "CK_wishes_price_positive",
                table: "wishes",
                sql: "price > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_users_email_not_empty",
                table: "users",
                sql: "trim(email) <> ''");

            migrationBuilder.AddCheckConstraint(
                name: "CK_users_username_not_empty",
                table: "users",
                sql: "trim(username) <> ''");

            migrationBuilder.AddCheckConstraint(
                name: "CK_events_title_not_empty",
                table: "events",
                sql: "trim(title) <> ''");

            migrationBuilder.AddCheckConstraint(
                name: "CK_catalog_items_name_not_empty",
                table: "catalog_items",
                sql: "trim(name) <> ''");

            migrationBuilder.AddCheckConstraint(
                name: "CK_catalog_items_price_positive",
                table: "catalog_items",
                sql: "price > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_catalog_collections_name_not_empty",
                table: "catalog_collections",
                sql: "trim(name) <> ''");

            migrationBuilder.AddCheckConstraint(
                name: "CK_catalog_collections_order_non_negative",
                table: "catalog_collections",
                sql: "\"order\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_catalog_categories_name_not_empty",
                table: "catalog_categories",
                sql: "trim(name) <> ''");

            migrationBuilder.AddCheckConstraint(
                name: "CK_catalog_categories_order_non_negative",
                table: "catalog_categories",
                sql: "\"order\" >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_wishlists_name_not_empty",
                table: "wishlists");

            migrationBuilder.DropIndex(
                name: "ix_wishes_share_token",
                table: "wishes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_wishes_name_not_empty",
                table: "wishes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_wishes_price_positive",
                table: "wishes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_users_email_not_empty",
                table: "users");

            migrationBuilder.DropCheckConstraint(
                name: "CK_users_username_not_empty",
                table: "users");

            migrationBuilder.DropCheckConstraint(
                name: "CK_events_title_not_empty",
                table: "events");

            migrationBuilder.DropCheckConstraint(
                name: "CK_catalog_items_name_not_empty",
                table: "catalog_items");

            migrationBuilder.DropCheckConstraint(
                name: "CK_catalog_items_price_positive",
                table: "catalog_items");

            migrationBuilder.DropCheckConstraint(
                name: "CK_catalog_collections_name_not_empty",
                table: "catalog_collections");

            migrationBuilder.DropCheckConstraint(
                name: "CK_catalog_collections_order_non_negative",
                table: "catalog_collections");

            migrationBuilder.DropCheckConstraint(
                name: "CK_catalog_categories_name_not_empty",
                table: "catalog_categories");

            migrationBuilder.DropCheckConstraint(
                name: "CK_catalog_categories_order_non_negative",
                table: "catalog_categories");

            migrationBuilder.DropColumn(
                name: "share_token",
                table: "wishes");

            migrationBuilder.AlterColumn<string>(
                name: "url",
                table: "wishes",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldMaxLength: 2048,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "wishes",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "image_path",
                table: "wishes",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "wishes",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "scope",
                table: "user_external_tokens",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "provider",
                table: "user_external_tokens",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "url",
                table: "catalog_items",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldMaxLength: 2048,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "catalog_items",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "image_path",
                table: "catalog_items",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "catalog_items",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "occasion",
                table: "catalog_collections",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "catalog_collections",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "catalog_collections",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "cover_image_path",
                table: "catalog_collections",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "catalog_categories",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "username",
                table: "admin_users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldMaxLength: 50);
        }
    }
}
