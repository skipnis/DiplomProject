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
        var wishes = db.Wishes
            .AsNoTracking()
            .Where(w => w.WishlistId == query.WishlistId);

        wishes = (query.SortBy, query.Direction) switch
        {
            (WishSortBy.Name,     SortDirection.Asc)  => wishes.OrderBy(w => w.Name),
            (WishSortBy.Name,     SortDirection.Desc) => wishes.OrderByDescending(w => w.Name),
            (WishSortBy.Priority, SortDirection.Asc)  => wishes.OrderBy(w => w.Priority),
            (WishSortBy.Priority, SortDirection.Desc) => wishes.OrderByDescending(w => w.Priority),
            (WishSortBy.Status,   _)                  => wishes.OrderBy(w => w.IsFulfilled),
            (_, SortDirection.Asc)                    => wishes.OrderBy(w => w.CreatedAt),
            _                                         => wishes.OrderByDescending(w => w.CreatedAt),
        };

        var pagedData = await wishes
            .Select(w => new { w.Id, w.Name, w.Price, w.Currency, w.Priority, w.ImagePath, w.IsFulfilled, w.CreatedByUserId, w.CreatedAt })
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
                w.IsFulfilled, reservedIds.Contains(w.Id), wishesWithBadgeIds.Contains(w.Id), w.CreatedByUserId, w.CreatedAt))
            .ToList();

        return new PagedResponse<WishSummaryDto>(items, pagedData.Page, pagedData.PageSize, pagedData.TotalCount);
    }
}
