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
        
        var result = await db.Wishlists
            .AsNoTracking()
            .Where(w => w.OwnerId == query.TargetUserId)
            .Where(w =>
                w.OwnerId == query.CurrentUserId ||
                w.Visibility == WishlistVisibility.Public ||
                (w.Visibility == WishlistVisibility.Friends && areFriends) ||
                w.Members.Any(m => m.UserId == query.CurrentUserId))
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