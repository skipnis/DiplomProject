using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Friendships.Entities;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Users;

namespace Wishapp.Web.Friendships.Features.GetFriendshipRequests;

public sealed class GetFriendshipRequestsHandler(ApplicationDbContext db, IUsersApi usersApi)
    : IQueryHandler<GetFriendshipRequestsQuery, PagedResponse<FriendshipRequest>>
{
    public async Task<Result<PagedResponse<FriendshipRequest>>> HandleAsync(
        GetFriendshipRequestsQuery query,
        CancellationToken ct = default)
    {
        var friendshipsQuery = query.IsOutgoing
            ? db.Friendships.AsNoTracking().Where(f => f.RequesterId == query.UserId && f.Status == query.Status)
            : db.Friendships.AsNoTracking().Where(f => f.AddresseeId == query.UserId && f.Status == query.Status);

        var totalCount = await friendshipsQuery.CountAsync(ct);

        var pagedFriendships = await friendshipsQuery
            .OrderByDescending(f => f.CreatedAt)
            .Skip((query.Request.Page - 1) * query.Request.PageSize)
            .Take(query.Request.PageSize)
            .Select(f => new
            {
                f.Id,
                OtherUserId = query.IsOutgoing ? f.AddresseeId : f.RequesterId,
                f.CreatedAt
            })
            .ToListAsync(ct);

        if (pagedFriendships.Count == 0)
            return new PagedResponse<FriendshipRequest>([], query.Request.Page, query.Request.PageSize, totalCount);

        var otherUserIds = pagedFriendships.Select(f => f.OtherUserId).ToList();
        var userInfos = await usersApi.GetUsersPublicInfoAsync(otherUserIds, ct);

        var items = pagedFriendships.Select(f =>
        {
            var userInfo = userInfos.GetValueOrDefault(f.OtherUserId);
            return new FriendshipRequest(f.Id, f.OtherUserId, userInfo?.DisplayName ?? string.Empty, userInfo?.AvatarUrl, f.CreatedAt);
        }).ToList();

        return new PagedResponse<FriendshipRequest>(items, query.Request.Page, query.Request.PageSize, totalCount);
    }
}
