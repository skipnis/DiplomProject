using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Wishlists.Dtos;

namespace Wishapp.Web.Wishlists.Features.Wishlists.GetWishlist;

public sealed class GetWishlistHandler(ApplicationDbContext db)
    : IQueryHandler<GetWishlistQuery, WishlistDto>
{
    public async Task<Result<WishlistDto>> HandleAsync(
        GetWishlistQuery query,
        CancellationToken ct = default)
    {
        var wishlist = await db.Wishlists
            .AsNoTracking()
            .Include(w => w.Members)
            .Include(w => w.Wishes)
            .FirstOrDefaultAsync(w => w.Id == query.WishlistId, ct);

        if (wishlist is null)
        {
            return Error.NotFound("Wishlists.NotFound", "Wishlist not found");
        }

        return WishlistDto.From(wishlist);
    }
}