using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Wishlists.Features.Wishes.GetWishQr;

public record GetWishQrQuery(Guid WishlistId, Guid WishId) : IQuery<byte[]>;