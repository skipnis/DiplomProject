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
            .IsRequired();

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.Property(c => c.Occasion)
            .HasMaxLength(50);

        builder.Property(c => c.CoverImagePath)
            .HasMaxLength(500);

        builder.HasIndex(c => c.Order);
        builder.HasIndex(c => c.IsPublished);

        builder.HasMany(c => c.Items)
            .WithOne(i => i.Collection)
            .HasForeignKey(i => i.CollectionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

