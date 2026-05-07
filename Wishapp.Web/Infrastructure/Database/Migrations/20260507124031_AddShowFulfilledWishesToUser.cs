using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wishapp.Web.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddShowFulfilledWishesToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "show_fulfilled_wishes",
                schema: "users",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "show_fulfilled_wishes",
                schema: "users",
                table: "users");
        }
    }
}
