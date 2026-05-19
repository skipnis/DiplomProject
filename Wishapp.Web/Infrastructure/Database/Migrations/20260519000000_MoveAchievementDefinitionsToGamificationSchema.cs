using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wishapp.Web.Infrastructure.Database.Migrations
{
    public partial class MoveAchievementDefinitionsToGamificationSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(name: "gamification");

            migrationBuilder.Sql(
                "ALTER TABLE users.achievement_definitions SET SCHEMA gamification;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE gamification.achievement_definitions SET SCHEMA users;");
        }
    }
}
