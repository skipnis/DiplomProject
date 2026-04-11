using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wishapp.Web.Wishlists.Entities;

namespace Wishapp.Web.Infrastructure.Database.Configs;

public class WishlistConfig : IEntityTypeConfiguration<Wishlist>
{
    public void Configure(EntityTypeBuilder<Wishlist> builder)
    {
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Name)
            .HasMaxLength(100)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(w => w.Description)
            .HasMaxLength(500)
            .HasColumnType("text");

        builder.Property(w => w.Emoji)
            .HasMaxLength(10)
            .HasColumnType("text");

        builder.Property(w => w.Visibility)
            .HasConversion<string>()
            .HasColumnType("text");

        builder.Property(w => w.SystemType)
            .HasConversion<string>()
            .HasColumnType("text")
            .HasDefaultValue(SystemWishlistType.None);

        builder.HasMany(w => w.Members)
            .WithOne()
            .HasForeignKey(m => m.WishlistId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(w => w.Wishes)
            .WithOne()
            .HasForeignKey(w => w.WishlistId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(w => new { w.OwnerId, w.CreatedAt });

        builder.HasIndex(w => w.Visibility);

        builder.ToTable("wishlists", "wishlists", t => t.HasCheckConstraint("CK_wishlists_name_not_empty", "trim(name) <> ''"));
    }
}
