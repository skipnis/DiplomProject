using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Wishlists.Features.Wishlists.DuplicateWish;

public record DuplicateWishCommand(Guid WishlistId, Guid WishId) : ICommand<DuplicateWishResponse>;