using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Wishlists.Dtos;

namespace Wishapp.Web.Wishlists.Features.Wishes.GetWish;

public record GetWishQuery(Guid WishlistId, Guid WishId) : IQuery<WishDto>;