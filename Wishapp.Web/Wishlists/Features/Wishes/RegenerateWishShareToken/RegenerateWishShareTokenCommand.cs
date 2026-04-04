using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Wishlists.Features.Wishes.RegenerateWishShareToken;

public record RegenerateWishShareTokenCommand(Guid WishlistId, Guid WishId) : ICommand<Guid>;
