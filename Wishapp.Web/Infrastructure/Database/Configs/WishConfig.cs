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
            .HasColumnType("text")
            .IsRequired();

        builder.Property(w => w.Description)
            .HasMaxLength(1000)
            .HasColumnType("text");

        builder.Property(w => w.Url)
            .HasMaxLength(2048)
            .HasColumnType("text");

        builder.Property(w => w.ImagePath)
            .HasMaxLength(500)
            .HasColumnType("text");

        builder.Property(w => w.Currency)
            .HasConversion<string>()
            .HasColumnType("text");

        builder.Property(w => w.Priority)
            .HasConversion<string>()
            .HasColumnType("text");

        builder.Property(w => w.Price)
            .HasPrecision(18, 2);

        builder.HasIndex(w => w.WishlistId);
        builder.HasIndex(w => w.CreatedAt);
        builder.HasIndex(w => w.UpdatedAt);
        builder.HasIndex(w => w.ShareToken).IsUnique();
        builder.HasIndex(w => new { w.WishlistId, w.CreatedAt });
        builder.HasIndex(w => new { w.WishlistId, w.Name });
        builder.HasIndex(w => new { w.WishlistId, w.Priority });
        builder.HasIndex(w => new { w.WishlistId, w.IsFulfilled });

        builder.ToTable("wishes", "wishlists", t =>
        {
            t.HasCheckConstraint("CK_wishes_name_not_empty", "trim(name) <> ''");
            t.HasCheckConstraint("CK_wishes_price_positive", "price > 0");
        });
    }
}
