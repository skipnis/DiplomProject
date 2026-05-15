using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Friendships;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Users.Features.GetMyBlacklist;

namespace Wishapp.Web.Users.Features.GetUserBlacklist;

public sealed class GetUserBlacklistHandler(ApplicationDbContext db, IFriendshipsApi friendshipsApi)
    : IQueryHandler<GetUserBlacklistQuery, List<BlacklistItemResponse>>
{
    public async Task<Result<List<BlacklistItemResponse>>> HandleAsync(
        GetUserBlacklistQuery query,
        CancellationToken ct = default)
    {
        if (query.RequestingUserId == query.TargetUserId)
            return Error.Forbidden("Blacklist.Forbidden", "Use /users/me/blacklist to view your own blacklist");

        var areFriends = await friendshipsApi.AreFriendsAsync(query.RequestingUserId, query.TargetUserId, ct);

        if (!areFriends)
            return Error.Forbidden("Blacklist.Forbidden", "Only friends can view this blacklist");

        var items = await db.BlacklistItems
            .AsNoTracking()
            .Where(item => item.UserId == query.TargetUserId)
            .OrderBy(item => item.CreatedAt)
            .Select(item => new BlacklistItemResponse(item.Id, item.Title, item.CreatedAt))
            .ToListAsync(ct);

        return items;
    }
}
