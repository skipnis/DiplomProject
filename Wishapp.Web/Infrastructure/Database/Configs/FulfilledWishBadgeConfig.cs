using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wishapp.Web.Gamification.Entities;

namespace Wishapp.Web.Infrastructure.Database.Configs;

public sealed class FulfilledWishBadgeConfig : IEntityTypeConfiguration<FulfilledWishBadge>
{
    public void Configure(EntityTypeBuilder<FulfilledWishBadge> builder)
    {
        builder.HasKey(b => b.Id);

        builder.HasIndex(b => new { b.WishId, b.BadgeType }).IsUnique();
        builder.HasIndex(b => b.WishId);
        builder.HasIndex(b => b.GifterUserId);

        builder.ToTable("fulfilled_wish_badges", "gamification");
    }
}
