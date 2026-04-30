using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Infrastructure.QrCode;

namespace Wishapp.Web.Wishlists.Features.Wishes.GetWishQr;

public sealed class GetWishQrHandler(
    ApplicationDbContext db,
    IQrCodeService qrCodeService)
    : IQueryHandler<GetWishQrQuery, byte[]>
{
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

        var wish = wishlist.Wishes.FirstOrDefault();

        if (wish is null)
        {
            return Error.NotFound("Wishes.NotFound", "Wish not found");
        }

        var url = $"{query.FrontendOrigin}/share/{wish.ShareToken}";

        return qrCodeService.Generate(url);
    }
}