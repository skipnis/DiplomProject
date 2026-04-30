using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wishapp.Web.Gamification.Entities;

namespace Wishapp.Web.Infrastructure.Database.Configs;

public sealed class UserAchievementConfig : IEntityTypeConfiguration<UserAchievement>
{
    public void Configure(EntityTypeBuilder<UserAchievement> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.DefinitionId).HasColumnName("type");

        builder.HasIndex(a => new { a.UserId, a.DefinitionId }).IsUnique();
        builder.HasIndex(a => a.UserId);

        builder.ToTable("user_achievements", "gamification");
    }
}
