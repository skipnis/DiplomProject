using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wishapp.Web.Gamification.Entities;

namespace Wishapp.Web.Infrastructure.Database.Configs;

public sealed class FulfilledWishBadgeDefinitionConfig : IEntityTypeConfiguration<FulfilledWishBadgeDefinition>
{
    public void Configure(EntityTypeBuilder<FulfilledWishBadgeDefinition> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).ValueGeneratedOnAdd();
        builder.Property(b => b.Emoji).HasMaxLength(10).HasColumnType("text").IsRequired();
        builder.Property(b => b.Slug).HasMaxLength(50).HasColumnType("text").IsRequired();
        builder.Property(b => b.Label).HasMaxLength(200).HasColumnType("text").IsRequired();
        builder.Property(b => b.Description).HasMaxLength(500).HasColumnType("text").IsRequired();

        builder.HasIndex(b => b.Slug).IsUnique();

        builder.ToTable("fulfilled_wish_badge_definitions", "gamification");
    }
}
