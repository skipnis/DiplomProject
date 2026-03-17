namespace Wishapp.Web.Reservations.Entities;

public sealed class WishReservation
{
    public Guid Id { get; private set; }
    public Guid WishId { get; private set; }
    public Guid WishlistId { get; private set; }
    public Guid ReservedByUserId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private WishReservation() { }

    public static WishReservation Create(Guid wishId, Guid wishlistId, Guid reservedByUserId)
    {
        return new WishReservation
        {
            Id = Guid.CreateVersion7(),
            WishId = wishId,
            WishlistId = wishlistId,
            ReservedByUserId = reservedByUserId,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
