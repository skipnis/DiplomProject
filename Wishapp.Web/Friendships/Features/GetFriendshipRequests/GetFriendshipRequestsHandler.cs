using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Extensions;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Friendships.Features.GetFriendshipRequests;

public sealed class GetFriendshipRequestsHandler(ApplicationDbContext db)
    : IQueryHandler<GetFriendshipRequestsQuery, PagedResponse<FriendshipRequest>>
{
    public async Task<Result<PagedResponse<FriendshipRequest>>> HandleAsync(
        GetFriendshipRequestsQuery query,
        CancellationToken ct = default)
    {
        var result = await db.Friendships
            .Where(f => f.AddresseeId == query.UserId && f.Status == query.Status)
            .Join(db.Users,
                f => f.RequesterId,
                u => u.Id,
                (f, u) => new { FriendshipId = f.Id, UserId = u.Id, u.Username, u.AvatarUrl, f.CreatedAt })
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new FriendshipRequest(x.FriendshipId, x.UserId, x.Username, x.AvatarUrl, x.CreatedAt))
            .ToPagedResponseAsync(query.Request, ct);

        return result;
    }
}
