using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Wishlists.Features.Wishes.FulfillWish;

public record FulfillWishCommand(Guid WishlistId, Guid WishId, Guid UserId) : ICommand<FulfillWishResult>;

public record FulfillWishResult(bool HasGifter);
