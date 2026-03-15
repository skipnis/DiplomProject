using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Friendships.Entities;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Friendships.Features.GetFriends;

public sealed class GetFriendsHandler(ApplicationDbContext db)
    : IQueryHandler<GetFriendsQuery, IEnumerable<FriendInfo>>
{
    public async Task<Result<IEnumerable<FriendInfo>>> HandleAsync(
        GetFriendsQuery query,
        CancellationToken ct = default)
    {
        var friends = await db.Friendships
            .Where(f =>
                (f.RequesterId == query.UserId || f.AddresseeId == query.UserId) &&
                f.Status == FriendshipStatus.Accepted)
            .Join(db.Users,
                f => f.RequesterId == query.UserId ? f.AddresseeId : f.RequesterId,
                u => u.Id,
                (f, u) => new FriendInfo(u.Id, u.Username, u.AvatarUrl))
            .ToListAsync(ct);

        return Result.Success<IEnumerable<FriendInfo>>(friends);
    }
}