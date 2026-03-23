using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wishapp.Web.Catalog.Entities;

namespace Wishapp.Web.Infrastructure.Database.Configs;

public class CatalogItemConfig : IEntityTypeConfiguration<CatalogItem>
{
    public void Configure(EntityTypeBuilder<CatalogItem> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(i => i.Description)
            .HasMaxLength(2000);

        builder.Property(i => i.Price)
            .HasPrecision(18, 2);

        builder.Property(i => i.Currency)
            .HasConversion<string>();

        builder.Property(i => i.Url)
            .HasMaxLength(2048);

        builder.Property(i => i.ImagePath)
            .HasMaxLength(500);

        builder.HasOne(i => i.Category)
            .WithMany()
            .HasForeignKey(i => i.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(i => i.CategoryId);
        builder.HasIndex(i => i.IsPublished);
        builder.HasIndex(i => i.CreatedAt);
    }
}
