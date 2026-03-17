using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Wishlists.Features.Wishlists.GetWishlistQr;

public record GetWishlistQrQuery(Guid WishlistId) : IQuery<byte[]>;