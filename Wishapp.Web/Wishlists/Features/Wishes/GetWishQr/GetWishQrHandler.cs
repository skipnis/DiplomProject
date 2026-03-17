using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Infrastructure.QrCode;
using Wishapp.Web.Wishlists.Entities;

namespace Wishapp.Web.Wishlists.Features.Wishes.GetWishQr;

public sealed class GetWishQrHandler(
    ApplicationDbContext db,
    IQrCodeService qrCodeService,
    IOptions<QrCodeOptions> options)
    : IQueryHandler<GetWishQrQuery, byte[]>
{
    private readonly QrCodeOptions _options = options.Value;

    public async Task<Result<byte[]>> HandleAsync(
        GetWishQrQuery query,
        CancellationToken ct = default)
    {
        var wishlist = await db.Wishlists
            .AsNoTracking()
            .Include(w => w.Wishes.Where(wish => wish.Id == query.WishId))
            .FirstOrDefaultAsync(w => w.Id == query.WishlistId, ct);

        if (wishlist is null)
        {
            return Error.NotFound("Wishlists.NotFound", "Wishlist not found");
        }

        if (wishlist.Visibility != WishlistVisibility.Public)
        {
            return Error.Failure("Wishlists.NotPublic", "QR code is only available for wishes in public wishlists");
        }

        var wish = wishlist.Wishes.FirstOrDefault();

        if (wish is null)
        {
            return Error.NotFound("Wishes.NotFound", "Wish not found");
        }

        var url = $"{_options.FrontendUrl}/wishlists/{query.WishlistId}/wishes/{query.WishId}";

        var qrBytes = qrCodeService.Generate(url);

        return qrBytes;
    }
}