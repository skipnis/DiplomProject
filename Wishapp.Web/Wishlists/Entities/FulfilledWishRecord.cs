namespace Wishapp.Web.Wishlists.Entities;

public sealed class FulfilledWishRecord
{
    public Guid Id { get; private set; }
    public Guid WishId { get; private set; }
    public Guid OwnerId { get; private set; }
    public Guid? GifterId { get; private set; }
    public string WishName { get; private set; } = null!;
    public string? WishDescription { get; private set; }
    public decimal? Price { get; private set; }
    public Currency? Currency { get; private set; }
    public string? ImagePath { get; private set; }
    public string WishlistName { get; private set; } = null!;
    public bool IsFromHiddenWishlist { get; private set; }
    public DateTimeOffset FulfilledAt { get; private set; }

    private FulfilledWishRecord() { }

    public static FulfilledWishRecord Create(
        Guid wishId,
        Guid ownerId,
        Guid? gifterId,
        string wishName,
        string? wishDescription,
        decimal? price,
        Currency? currency,
        string? imagePath,
        string wishlistName,
        bool isFromHiddenWishlist,
        DateTimeOffset fulfilledAt)
    {
        return new FulfilledWishRecord
        {
            Id = Guid.CreateVersion7(),
            WishId = wishId,
            OwnerId = ownerId,
            GifterId = gifterId,
            WishName = wishName,
            WishDescription = wishDescription,
            Price = price,
            Currency = currency,
            ImagePath = imagePath,
            WishlistName = wishlistName,
            IsFromHiddenWishlist = isFromHiddenWishlist,
            FulfilledAt = fulfilledAt,
        };
    }
}
