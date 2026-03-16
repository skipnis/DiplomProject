namespace Wishapp.Web.Friendships;

public interface IFriendshipsApi
{
    Task<bool> AreFriendsAsync(Guid userId, Guid targetId, CancellationToken ct = default);
    Task<List<Guid>> GetFriendIdsAsync(Guid userId, List<Guid> candidates, CancellationToken ct = default);
}