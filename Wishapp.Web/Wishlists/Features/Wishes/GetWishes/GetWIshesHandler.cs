using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Extensions;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Reservations;
using Wishapp.Web.Wishlists.Dtos;

namespace Wishapp.Web.Wishlists.Features.Wishes.GetWishes;

public sealed class GetWishesHandler(ApplicationDbContext db, IReservationsApi reservationsApi)
    : IQueryHandler<GetWishesQuery, PagedResponse<WishDto>>
{
    public async Task<Result<PagedResponse<WishDto>>> HandleAsync(
        GetWishesQuery query,
        CancellationToken ct = default)
    {
        var wishes = await db.Wishes
            .AsNoTracking()
            .Where(w => w.WishlistId == query.WishlistId)
            .OrderByDescending(w => w.CreatedAt)
            .ToPagedResponseAsync(query.Request, ct);

        var wishIds = wishes.Items.Select(w => w.Id).ToList();
        
        var reservedIds = await reservationsApi.GetReservedWishIdsAsync(wishIds, ct);

        var items = wishes.Items.Select(w => new WishDto(
            w.Id,
            w.Name,
            w.Description,
            w.Price,
            w.Currency,
            w.Priority,
            w.Url,
            w.ImagePath,
            w.CreatedAt,
            w.IsFulfilled,
            w.FulfilledAt,
            reservedIds.Contains(w.Id)))
            .ToList();

        return new PagedResponse<WishDto>(items, wishes.Page, wishes.PageSize, wishes.TotalCount);
    }
}
