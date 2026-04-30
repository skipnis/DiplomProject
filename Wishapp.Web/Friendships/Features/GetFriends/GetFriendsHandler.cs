using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Friendships.Entities;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Users;

namespace Wishapp.Web.Friendships.Features.GetFriends;

public sealed class GetFriendsHandler(ApplicationDbContext db, IUsersApi usersApi)
    : IQueryHandler<GetFriendsQuery, PagedResponse<FriendInfo>>
{
    public async Task<Result<PagedResponse<FriendInfo>>> HandleAsync(
        GetFriendsQuery query,
        CancellationToken ct = default)
    {
        var friendIds = await db.Friendships
            .AsNoTracking()
            .Where(f =>
                (f.RequesterId == query.UserId || f.AddresseeId == query.UserId) &&
                f.Status == FriendshipStatus.Accepted)
            .Select(f => f.RequesterId == query.UserId ? f.AddresseeId : f.RequesterId)
            .ToListAsync(ct);

        if (friendIds.Count == 0)
            return new PagedResponse<FriendInfo>([], query.Request.Page, query.Request.PageSize, 0);

        var userInfos = await usersApi.GetUsersPublicInfoAsync(friendIds, ct);

        var allFriends = friendIds
            .Where(id => userInfos.ContainsKey(id))
            .Select(id => new FriendInfo(id, userInfos[id].DisplayName, userInfos[id].AvatarUrl))
            .OrderBy(friend => friend.Username)
            .ToList();

        var totalCount = allFriends.Count;
        var items = allFriends
            .Skip((query.Request.Page - 1) * query.Request.PageSize)
            .Take(query.Request.PageSize)
            .ToList();

        return new PagedResponse<FriendInfo>(items, query.Request.Page, query.Request.PageSize, totalCount);
    }
}
