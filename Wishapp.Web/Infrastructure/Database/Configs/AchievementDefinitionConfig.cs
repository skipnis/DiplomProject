using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wishapp.Web.Gamification.Entities;

namespace Wishapp.Web.Infrastructure.Database.Configs;

public sealed class AchievementDefinitionConfig : IEntityTypeConfiguration<AchievementDefinition>
{
    public void Configure(EntityTypeBuilder<AchievementDefinition> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id).ValueGeneratedOnAdd();

        builder.Property(a => a.Name)
            .HasMaxLength(200)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(a => a.Description)
            .HasMaxLength(500)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(a => a.Emoji)
            .HasMaxLength(10)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(a => a.RuleType)
            .HasConversion<int>();

        builder.ToTable("achievement_definitions", "gamification");
    }
}
