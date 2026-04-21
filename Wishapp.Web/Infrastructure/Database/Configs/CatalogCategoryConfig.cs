using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wishapp.Web.Catalog.Entities;

namespace Wishapp.Web.Infrastructure.Database.Configs;

public class CatalogCategoryConfig : IEntityTypeConfiguration<CatalogCategory>
{
    public void Configure(EntityTypeBuilder<CatalogCategory> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .HasMaxLength(100)
            .HasColumnType("text")
            .IsRequired();

        builder.HasIndex(c => c.Name)
            .IsUnique();

        builder.HasIndex(c => c.IsPublished);
        builder.HasIndex(c => c.Order);

        builder.ToTable("catalog_categories", "catalog", t =>
        {
            t.HasCheckConstraint("CK_catalog_categories_name_not_empty", "trim(name) <> ''");
            t.HasCheckConstraint("CK_catalog_categories_order_non_negative", "\"order\" >= 0");
        });
    }
}
