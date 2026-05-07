using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Extensions;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Gamification;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Reservations;
using Wishapp.Web.Wishlists.Dtos;

namespace Wishapp.Web.Wishlists.Features.Wishes.GetWishes;

public sealed class GetWishesHandler(
    ApplicationDbContext db,
    IReservationsApi reservationsApi,
    IGamificationApi gamificationApi)
    : IQueryHandler<GetWishesQuery, PagedResponse<WishSummaryDto>>
{
    public async Task<Result<PagedResponse<WishSummaryDto>>> HandleAsync(
        GetWishesQuery query,
        CancellationToken ct = default)
    {
        var pagedData = await db.Wishes
            .AsNoTracking()
            .Where(w => w.WishlistId == query.WishlistId)
            .OrderByDescending(w => w.CreatedAt)
            .Select(w => new { w.Id, w.Name, w.Price, w.Currency, w.Priority, w.ImagePath, w.IsFulfilled })
            .ToPagedResponseAsync(query.Request, ct);

        var wishIds = pagedData.Items.Select(w => w.Id).ToList();

        HashSet<Guid> reservedIds = [];
        if (!query.HideReservations)
        {
            reservedIds = (await reservationsApi.GetReservedWishIdsAsync(wishIds, ct)).ToHashSet();
        }

        var wishesWithBadgeIds = await gamificationApi.GetWishIdsWithBadgesAsync(wishIds, ct);

        var items = pagedData.Items
            .Select(w => new WishSummaryDto(
                w.Id, w.Name, w.Price, w.Currency, w.Priority, w.ImagePath,
                w.IsFulfilled, reservedIds.Contains(w.Id), wishesWithBadgeIds.Contains(w.Id)))
            .ToList();

        return new PagedResponse<WishSummaryDto>(items, pagedData.Page, pagedData.PageSize, pagedData.TotalCount);
    }
}
