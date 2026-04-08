using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wishapp.Web.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddDisplayNameToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "display_name",
                schema: "users",
                table: "users",
                type: "text",
                maxLength: 100,
                nullable: true);

            migrationBuilder.Sql("UPDATE users.users SET display_name = username");

            migrationBuilder.AlterColumn<string>(
                name: "display_name",
                schema: "users",
                table: "users",
                type: "text",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "username",
                schema: "users",
                table: "users",
                type: "text",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldMaxLength: 50);

            migrationBuilder.Sql("UPDATE users.users SET username = NULL, is_onboarded = false");

            migrationBuilder.DropIndex(
                name: "ix_users_username",
                schema: "users",
                table: "users");

            migrationBuilder.Sql(@"ALTER TABLE users.users DROP CONSTRAINT IF EXISTS ""CK_users_username_not_empty""");

            migrationBuilder.Sql(@"ALTER TABLE users.users ADD CONSTRAINT ""CK_users_display_name_not_empty"" CHECK (trim(display_name) <> '')");
            migrationBuilder.Sql(@"ALTER TABLE users.users ADD CONSTRAINT ""CK_users_username_not_empty"" CHECK (username IS NULL OR trim(username) <> '')");

            migrationBuilder.CreateIndex(
                name: "ix_users_display_name",
                schema: "users",
                table: "users",
                column: "display_name")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_users_username",
                schema: "users",
                table: "users",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_users_display_name",
                schema: "users",
                table: "users");

            migrationBuilder.DropIndex(
                name: "ix_users_username",
                schema: "users",
                table: "users");

            migrationBuilder.Sql(@"ALTER TABLE users.users DROP CONSTRAINT IF EXISTS ""CK_users_display_name_not_empty""");
            migrationBuilder.Sql(@"ALTER TABLE users.users DROP CONSTRAINT IF EXISTS ""CK_users_username_not_empty""");

            migrationBuilder.Sql("UPDATE users.users SET username = display_name");

            migrationBuilder.AlterColumn<string>(
                name: "username",
                schema: "users",
                table: "users",
                type: "text",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "display_name",
                schema: "users",
                table: "users");

            migrationBuilder.CreateIndex(
                name: "ix_users_username",
                schema: "users",
                table: "users",
                column: "username")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.Sql(@"ALTER TABLE users.users ADD CONSTRAINT ""CK_users_username_not_empty"" CHECK (trim(username) <> '')");
        }
    }
}
