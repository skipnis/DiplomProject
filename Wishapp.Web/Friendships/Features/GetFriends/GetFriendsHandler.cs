using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Extensions;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Friendships.Entities;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Friendships.Features.GetFriends;

public sealed class GetFriendsHandler(ApplicationDbContext db)
    : IQueryHandler<GetFriendsQuery, PagedResponse<FriendInfo>>
{
    public async Task<Result<PagedResponse<FriendInfo>>> HandleAsync(
        GetFriendsQuery query,
        CancellationToken ct = default)
    {
        var result = await db.Friendships
            .Where(f =>
                (f.RequesterId == query.UserId || f.AddresseeId == query.UserId) &&
                f.Status == FriendshipStatus.Accepted)
            .Select(f => new
            {
                FriendId = f.RequesterId == query.UserId ? f.AddresseeId : f.RequesterId
            })
            .Join(db.Users,
                x => x.FriendId,
                u => u.Id,
                (x, u) => new { u.Id, u.Username, u.AvatarUrl })
            .OrderBy(x => x.Username)
            .Select(x => new FriendInfo(x.Id, x.Username, x.AvatarUrl))
            .ToPagedResponseAsync(query.Request, ct);

        return result;
    }
}
