using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wishapp.Web.Friendships.Entities;

namespace Wishapp.Web.Infrastructure.Database.Configs;

public class FriendshipConfig : IEntityTypeConfiguration<Friendship>
{
    public void Configure(EntityTypeBuilder<Friendship> builder)
    {
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Status)
            .HasConversion<string>();

        builder.HasIndex(f => new { f.RequesterId, f.AddresseeId }).IsUnique();

        builder.HasIndex(f => new { f.RequesterId, f.Status });

        builder.HasIndex(f => new { f.AddresseeId, f.Status });

        builder.ToTable("friendships", "friendships", t => t.HasCheckConstraint("CK_friendships_no_self", "requester_id <> addressee_id"));
    }
}
