using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wishapp.Web.Wishlists.Entities;

namespace Wishapp.Web.Infrastructure.Database.Configs;

public class FulfilledWishRecordConfig : IEntityTypeConfiguration<FulfilledWishRecord>
{
    public void Configure(EntityTypeBuilder<FulfilledWishRecord> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.WishName)
            .HasMaxLength(200)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(r => r.WishDescription)
            .HasMaxLength(1000)
            .HasColumnType("text");

        builder.Property(r => r.ImagePath)
            .HasMaxLength(500)
            .HasColumnType("text");

        builder.Property(r => r.WishlistName)
            .HasMaxLength(200)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(r => r.Currency)
            .HasConversion<string>()
            .HasColumnType("text");

        builder.Property(r => r.Price)
            .HasPrecision(18, 2);

        builder.HasIndex(r => r.OwnerId);
        builder.HasIndex(r => r.WishId);
        builder.HasIndex(r => r.FulfilledAt);

        builder.ToTable("fulfilled_wish_records", "wishlists");
    }
}
