using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wishapp.Web.Catalog.Entities;

namespace Wishapp.Web.Infrastructure.Database.Configs;

public class CatalogOccasionConfig : IEntityTypeConfiguration<CatalogOccasion>
{
    public void Configure(EntityTypeBuilder<CatalogOccasion> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Key)
            .HasMaxLength(50)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(o => o.Label)
            .HasMaxLength(100)
            .HasColumnType("text")
            .IsRequired();

        builder.HasIndex(o => o.Key).IsUnique();
        builder.HasIndex(o => o.Order);

        builder.ToTable("catalog_occasions", "catalog", t =>
        {
            t.HasCheckConstraint("CK_catalog_occasions_key_not_empty", "trim(key) <> ''");
            t.HasCheckConstraint("CK_catalog_occasions_label_not_empty", "trim(label) <> ''");
        });
    }
}
