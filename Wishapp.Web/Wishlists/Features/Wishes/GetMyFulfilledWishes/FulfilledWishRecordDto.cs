namespace Wishapp.Web.Wishlists.Features.Wishes.GetMyFulfilledWishes;

public sealed record FulfilledWishRecordDto(
    Guid Id,
    Guid? GifterId,
    string? GifterDisplayName,
    string WishName,
    string? WishDescription,
    decimal? Price,
    Currency? Currency,
    string? ImagePath,
    string WishlistName,
    DateTimeOffset FulfilledAt);
