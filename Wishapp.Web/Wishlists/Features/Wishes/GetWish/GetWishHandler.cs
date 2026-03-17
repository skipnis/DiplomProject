using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Reservations;
using Wishapp.Web.Wishlists.Dtos;

namespace Wishapp.Web.Wishlists.Features.Wishes.GetWish;

public sealed class GetWishHandler(ApplicationDbContext db, IReservationsApi reservationsApi)
    : IQueryHandler<GetWishQuery, WishDto>
{
    public async Task<Result<WishDto>> HandleAsync(
        GetWishQuery query,
        CancellationToken ct = default)
    {
        var wish = await db.Wishes
            .AsNoTracking()
            .Where(w => w.Id == query.WishId && w.WishlistId == query.WishlistId)
            .FirstOrDefaultAsync(ct);

        if (wish is null)
        {
            return Error.NotFound("Wishes.NotFound", "Wish not found");
        }

        var reservedIds = await reservationsApi.GetReservedWishIdsAsync([wish.Id], ct);

        return new WishDto(
            wish.Id,
            wish.Name,
            wish.Description,
            wish.Price,
            wish.Currency,
            wish.Priority,
            wish.Url,
            wish.ImagePath,
            wish.CreatedAt,
            wish.IsFulfilled,
            wish.FulfilledAt,
            reservedIds.Contains(wish.Id));
    }
}
