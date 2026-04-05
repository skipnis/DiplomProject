using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wishapp.Web.Catalog.Entities;

namespace Wishapp.Web.Infrastructure.Database.Configs;

public class CatalogItemRatingConfig : IEntityTypeConfiguration<CatalogItemRating>
{
    public void Configure(EntityTypeBuilder<CatalogItemRating> builder)
    {
        builder.HasKey(r => r.Id);

        builder.HasIndex(r => new { r.UserId, r.CatalogItemId }).IsUnique();
        builder.HasIndex(r => r.CatalogItemId);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_catalog_item_ratings_value", "value BETWEEN 1 AND 5");
        });
    }
}
