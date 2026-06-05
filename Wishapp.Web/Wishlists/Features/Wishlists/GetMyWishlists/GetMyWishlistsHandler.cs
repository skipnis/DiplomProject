using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Extensions;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Wishlists.Dtos;

namespace Wishapp.Web.Wishlists.Features.Wishlists.GetMyWishlists;

public sealed class GetMyWishlistsHandler(ApplicationDbContext db)
    : IQueryHandler<GetMyWishlistsQuery, PagedResponse<WishlistSummaryDto>>
{
    public async Task<Result<PagedResponse<WishlistSummaryDto>>> HandleAsync(
        GetMyWishlistsQuery query,
        CancellationToken ct = default)
    {
        var userId = query.UserId;

        var wishlists = db.Wishlists
            .AsNoTracking()
            .Where(w => w.OwnerId == userId || w.Members.Any(m => m.UserId == userId));

        wishlists = (query.SortBy, query.Direction) switch
        {
            (WishlistSortBy.Name, SortDirection.Asc)  => wishlists.OrderBy(w => w.Name),
            (WishlistSortBy.Name, SortDirection.Desc) => wishlists.OrderByDescending(w => w.Name),
            (_, SortDirection.Asc)                    => wishlists.OrderBy(w => w.CreatedAt),
            _                                         => wishlists.OrderByDescending(w => w.CreatedAt),
        };

        var result = await wishlists
            .Select(w => new WishlistSummaryDto(
                w.Id,
                w.Name,
                w.Description,
                w.Emoji,
                w.Visibility,
                w.IsSystem,
                w.Wishes.Count,
                w.Wishes.Count(wish => wish.IsFulfilled),
                w.CreatedAt,
                w.OwnerId == userId))
            .ToPagedResponseAsync(query.Request, ct);

        return result;
    }
}
