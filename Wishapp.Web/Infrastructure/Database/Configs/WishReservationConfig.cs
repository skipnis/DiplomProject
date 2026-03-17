using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wishapp.Web.Reservations.Entities;

namespace Wishapp.Web.Infrastructure.Database.Configs;

public class WishReservationConfig : IEntityTypeConfiguration<WishReservation>
{
    public void Configure(EntityTypeBuilder<WishReservation> builder)
    {
        builder.HasKey(r => r.Id);

        builder.HasIndex(r => r.WishId)
            .IsUnique();

        builder.HasIndex(r => r.ReservedByUserId);

        builder.HasIndex(r => r.WishlistId);
    }
}
