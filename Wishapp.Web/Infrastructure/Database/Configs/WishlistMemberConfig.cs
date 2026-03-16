using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wishapp.Web.Wishlists.Entities;

namespace Wishapp.Web.Infrastructure.Database.Configs;

public class WishlistMemberConfig : IEntityTypeConfiguration<WishlistMember>
{
    public void Configure(EntityTypeBuilder<WishlistMember> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Role)
            .HasConversion<string>();

        builder.HasIndex(m => new { m.WishlistId, m.UserId }).IsUnique();
        
        builder.HasIndex(m => m.UserId);
    }
}