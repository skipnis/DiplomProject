using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wishapp.Web.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddWishlistSurpriseMode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_surprise_mode_enabled",
                table: "wishlists",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_surprise_mode_enabled",
                table: "wishlists");
        }
    }
}
