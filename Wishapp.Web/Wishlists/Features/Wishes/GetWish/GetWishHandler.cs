using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Wishlists.Dtos;

namespace Wishapp.Web.Wishlists.Features.Wishes.GetWish;

public sealed class GetWishHandler(ApplicationDbContext db)
    : IQueryHandler<GetWishQuery, WishDto>
{
    public async Task<Result<WishDto>> HandleAsync(
        GetWishQuery query,
        CancellationToken ct = default)
    {
        var wish = await db.Wishes
            .AsNoTracking()
            .Where(w => w.Id == query.WishId && w.WishlistId == query.WishlistId)
            .Select(w => WishDto.From(w))
            .FirstOrDefaultAsync(ct);

        if (wish is null)
        {
            return Error.NotFound("Wishes.NotFound", "Wish not found");
        }

        return wish;
    }
}