using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Friendships.Entities;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Friendships;

public sealed class FriendshipsApi(ApplicationDbContext db) : IFriendshipsApi
{
    public Task<bool> AreFriendsAsync(Guid userId, Guid targetId, CancellationToken ct = default) =>
        db.Friendships.AnyAsync(Friendship.AcceptedBetween(userId, targetId), ct);
}