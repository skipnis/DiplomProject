using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wishapp.Web.Catalog.Entities;

namespace Wishapp.Web.Infrastructure.Database.Configs;

public class CatalogCollectionItemConfig : IEntityTypeConfiguration<CatalogCollectionItem>
{
    public void Configure(EntityTypeBuilder<CatalogCollectionItem> builder)
    {
        builder.HasKey(i => new { i.CollectionId, i.CatalogItemId });

        builder.HasOne(i => i.CatalogItem)
            .WithMany()
            .HasForeignKey(i => i.CatalogItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
