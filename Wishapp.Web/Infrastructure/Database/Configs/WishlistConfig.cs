using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wishapp.Web.Wishlists.Entities;

namespace Wishapp.Web.Infrastructure.Database.Configs;

public class WishlistConfig : IEntityTypeConfiguration<Wishlist>
{
    public void Configure(EntityTypeBuilder<Wishlist> builder)
    {
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Visibility)
            .HasConversion<string>();

        builder.HasMany(w => w.Members)
            .WithOne()
            .HasForeignKey(m => m.WishlistId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(w => w.Wishes)
            .WithOne()
            .HasForeignKey(w => w.WishlistId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(w => w.OwnerId);
        
        builder.HasIndex(w => w.Visibility);
        
        builder.HasIndex(w => w.CreatedAt);
    }
}