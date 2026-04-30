using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wishapp.Web.Gamification.Entities;

namespace Wishapp.Web.Infrastructure.Database.Configs;

public sealed class CatalogItemBadgeVoteConfig : IEntityTypeConfiguration<CatalogItemBadgeVote>
{
    public void Configure(EntityTypeBuilder<CatalogItemBadgeVote> builder)
    {
        builder.HasKey(v => v.Id);

        builder.HasIndex(v => new { v.CatalogItemId, v.UserId, v.BadgeType }).IsUnique();
        builder.HasIndex(v => v.CatalogItemId);

        builder.ToTable("catalog_item_badge_votes", "gamification");
    }
}
