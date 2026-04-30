using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Wishlists.Features.Wishes.GetWishQr;

public record GetWishQrQuery(Guid WishlistId, Guid WishId, string FrontendOrigin) : IQuery<byte[]>;