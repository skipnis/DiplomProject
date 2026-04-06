using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NpgsqlTypes;
using Wishapp.Web.Catalog.Entities;

namespace Wishapp.Web.Infrastructure.Database.Configs;

public class CatalogItemConfig : IEntityTypeConfiguration<CatalogItem>
{
    public void Configure(EntityTypeBuilder<CatalogItem> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Name)
            .HasMaxLength(200)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(i => i.Description)
            .HasMaxLength(2000)
            .HasColumnType("text");

        builder.Property(i => i.Price)
            .HasPrecision(18, 2);

        builder.Property(i => i.Currency)
            .HasConversion<string>()
            .HasColumnType("text");

        builder.Property(i => i.Url)
            .HasMaxLength(2048)
            .HasColumnType("text");

        builder.Property(i => i.ImagePath)
            .HasMaxLength(500)
            .HasColumnType("text");

        builder.HasOne(i => i.Category)
            .WithMany()
            .HasForeignKey(i => i.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property<NpgsqlTsVector>("SearchVector")
            .HasComputedColumnSql(
                "to_tsvector('russian', coalesce(name, '') || ' ' || coalesce(description, ''))",
                stored: true);

        builder.HasIndex("SearchVector")
            .HasMethod("gin");

        builder.HasIndex(i => i.CategoryId);
        builder.HasIndex(i => i.IsPublished);
        builder.HasIndex(i => i.CreatedAt);

        builder.ToTable("catalog_items", "catalog", t =>
        {
            t.HasCheckConstraint("CK_catalog_items_name_not_empty", "trim(name) <> ''");
            t.HasCheckConstraint("CK_catalog_items_price_positive", "price > 0");
        });
    }
}
