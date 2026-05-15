using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Extensions;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Friendships;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Wishlists.Dtos;
using Wishapp.Web.Wishlists.Entities;

namespace Wishapp.Web.Wishlists.Features.Wishlists.GetUserWishlists;

public sealed class GetUserWishlistsHandler(
    ApplicationDbContext db,
    IFriendshipsApi friendshipsApi)
    : IQueryHandler<GetUserWishlistsQuery, PagedResponse<WishlistSummaryDto>>
{
    public async Task<Result<PagedResponse<WishlistSummaryDto>>> HandleAsync(
        GetUserWishlistsQuery query,
        CancellationToken ct = default)
    {
        var areFriends = query.CurrentUserId.HasValue && 
                         await friendshipsApi.AreFriendsAsync(query.CurrentUserId.Value, query.TargetUserId, ct);
        
        var wishlists = db.Wishlists
            .AsNoTracking()
            .Where(w => w.OwnerId == query.TargetUserId)
            .Where(w =>
                w.OwnerId == query.CurrentUserId ||
                w.Visibility == WishlistVisibility.Public ||
                (w.Visibility == WishlistVisibility.Friends && areFriends) ||
                w.Members.Any(m => m.UserId == query.CurrentUserId));

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
                w.CreatedAt))
            .ToPagedResponseAsync(query.Request, ct);

        return result;
    }
}