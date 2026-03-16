using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Friendships.Entities;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Friendships;

public sealed class FriendshipsApi(ApplicationDbContext db) : IFriendshipsApi
{
    public Task<bool> AreFriendsAsync(Guid userId, Guid targetId, CancellationToken ct = default) =>
        db.Friendships.AnyAsync(Friendship.AcceptedBetween(userId, targetId), ct);

    public async Task<List<Guid>> GetFriendIdsAsync(
        Guid userId,
        List<Guid> candidates,
        CancellationToken ct = default)
    {
        return await db.Friendships
            .Where(Friendship.AcceptedWithAny(userId, candidates))
            .Select(f => f.RequesterId == userId ? f.AddresseeId : f.RequesterId)
            .ToListAsync(ct);
    }
}