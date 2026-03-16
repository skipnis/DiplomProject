using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wishapp.Web.Wishlists.Entities;

namespace Wishapp.Web.Infrastructure.Database.Configs;

public class WishConfig : IEntityTypeConfiguration<Wish>
{
    public void Configure(EntityTypeBuilder<Wish> builder)
    {
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(w => w.Description)
            .HasMaxLength(1000);

        builder.Property(w => w.Url)
            .HasMaxLength(2048);

        builder.Property(w => w.ImagePath)
            .HasMaxLength(500);

        builder.Property(w => w.Currency)
            .HasConversion<string>();

        builder.Property(w => w.Priority)
            .HasConversion<string>();

        builder.Property(w => w.Price)
            .HasPrecision(18, 2);

        builder.HasIndex(w => w.WishlistId);
        builder.HasIndex(w => w.CreatedAt);
    }
}