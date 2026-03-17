using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QRCoder;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Infrastructure.QrCode;
using Wishapp.Web.Wishlists.Entities;

namespace Wishapp.Web.Wishlists.Features.Wishlists.GetWishlistQr;

public sealed class GetWishlistQrHandler(
    ApplicationDbContext db,
    IQrCodeService qrCodeService,
    IOptions<QrCodeOptions> options)
    : IQueryHandler<GetWishlistQrQuery, byte[]>
{
    private readonly QrCodeOptions _options = options.Value;

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

        var url = $"{_options.FrontendUrl}/wishlists/{query.WishlistId}";

        var qrBytes = qrCodeService.Generate(url);

        return qrBytes;
    }
}