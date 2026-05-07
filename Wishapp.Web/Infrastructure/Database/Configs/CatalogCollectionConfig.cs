using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wishapp.Web.Catalog.Entities;

namespace Wishapp.Web.Infrastructure.Database.Configs;

public class CatalogCollectionConfig : IEntityTypeConfiguration<CatalogCollection>
{
    public void Configure(EntityTypeBuilder<CatalogCollection> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .HasMaxLength(150)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(c => c.Description)
            .HasMaxLength(500)
            .HasColumnType("text");

        builder.HasOne(c => c.Occasion)
            .WithMany()
            .HasForeignKey(c => c.OccasionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(c => c.CoverImagePath)
            .HasMaxLength(500)
            .HasColumnType("text");

        builder.HasIndex(c => c.Order);
        builder.HasIndex(c => c.IsPublished);

        builder.ToTable("catalog_collections", "catalog", t =>
        {
            t.HasCheckConstraint("CK_catalog_collections_name_not_empty", "trim(name) <> ''");
            t.HasCheckConstraint("CK_catalog_collections_order_non_negative", "\"order\" >= 0");
        });

        builder.HasMany(c => c.Items)
            .WithOne(i => i.Collection)
            .HasForeignKey(i => i.CollectionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
