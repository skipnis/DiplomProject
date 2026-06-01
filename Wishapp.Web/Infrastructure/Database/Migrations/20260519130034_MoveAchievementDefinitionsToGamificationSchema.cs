using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wishapp.Web.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class MoveAchievementDefinitionsToGamificationSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(name: "gamification");
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.tables
                        WHERE table_schema = 'users' AND table_name = 'achievement_definitions'
                    ) THEN
                        ALTER TABLE users.achievement_definitions SET SCHEMA gamification;
                    END IF;
                END $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.tables
                        WHERE table_schema = 'gamification' AND table_name = 'achievement_definitions'
                    ) THEN
                        ALTER TABLE gamification.achievement_definitions SET SCHEMA users;
                    END IF;
                END $$;
                """);
        }
    }
}
