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
        var result = await db.Wishlists
            .AsNoTracking()
            .Where(w => w.OwnerId == query.UserId)
            .OrderByDescending(w => w.CreatedAt)
            .Select(w => new WishlistSummaryDto(
                w.Id,
                w.Name,
                w.Description,
                w.Emoji,
                w.Visibility,
                w.IsSystem,
                w.Wishes.Count,
                w.CreatedAt))
            .ToPagedResponseAsync(query.Request, ct);

        return result;
    }
}
