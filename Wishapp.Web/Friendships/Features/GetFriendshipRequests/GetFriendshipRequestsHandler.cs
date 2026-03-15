using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Friendships.Features.GetFriendshipRequests;

public sealed class GetFriendshipRequestsHandler(ApplicationDbContext db)
    : IQueryHandler<GetFriendshipRequestsQuery, IEnumerable<FriendshipRequest>>
{
    public async Task<Result<IEnumerable<FriendshipRequest>>> HandleAsync(
        GetFriendshipRequestsQuery query,
        CancellationToken ct = default)
    {
        var requests = await db.Friendships
            .Where(f => f.AddresseeId == query.UserId && f.Status == query.Status)
            .Join(db.Users,
                f => f.RequesterId,
                u => u.Id,
                (f, u) => new FriendshipRequest(
                    f.Id,
                    u.Id,
                    u.Username,
                    u.AvatarUrl,
                    f.CreatedAt))
            .ToListAsync(ct);

        return Result.Success<IEnumerable<FriendshipRequest>>(requests);
    }
}