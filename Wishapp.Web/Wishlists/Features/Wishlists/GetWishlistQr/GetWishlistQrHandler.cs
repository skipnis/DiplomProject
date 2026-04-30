using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Infrastructure.QrCode;
using Wishapp.Web.Wishlists.Entities;

namespace Wishapp.Web.Wishlists.Features.Wishlists.GetWishlistQr;

public sealed class GetWishlistQrHandler(
    ApplicationDbContext db,
    IQrCodeService qrCodeService)
    : IQueryHandler<GetWishlistQrQuery, byte[]>
{
    public async Task<Result<byte[]>> HandleAsync(
        GetWishlistQrQuery query,
        CancellationToken ct = default)
    {
        var wishlist = await db.Wishlists
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == query.WishlistId, ct);

        if (wishlist is null)
        {
            return Error.NotFound("Wishlists.NotFound", "Wishlist not found");
        }

        if (wishlist.Visibility != WishlistVisibility.Public)
        {
            return Error.Failure("Wishlists.NotPublic", "QR code is only available for public wishlists");
        }

        var url = $"{query.FrontendOrigin}/wishlists/{query.WishlistId}";

        return qrCodeService.Generate(url);
    }
}