using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wishapp.Web.Catalog.Entities;

namespace Wishapp.Web.Infrastructure.Database.Configs;

public class CatalogItemOccasionConfig : IEntityTypeConfiguration<CatalogItemOccasion>
{
    public void Configure(EntityTypeBuilder<CatalogItemOccasion> builder)
    {
        builder.HasKey(o => new { o.CatalogItemId, o.OccasionId });

        builder.HasOne(o => o.CatalogItem)
            .WithMany(i => i.Occasions)
            .HasForeignKey(o => o.CatalogItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(o => o.Occasion)
            .WithMany()
            .HasForeignKey(o => o.OccasionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(o => o.OccasionId);

        builder.ToTable("catalog_item_occasions", "catalog");
    }
}
