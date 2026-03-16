using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Wishlists.Dtos;

namespace Wishapp.Web.Wishlists.Features.Wishlists.GetWishlist;

public record GetWishlistQuery(Guid WishlistId, Guid? UserId) : IQuery<WishlistDto>;