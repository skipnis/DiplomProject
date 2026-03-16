using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Wishlists.Features.Wishes.DeleteWishImage;

public record DeleteWishImageCommand(Guid WishlistId, Guid WishId) : ICommand;