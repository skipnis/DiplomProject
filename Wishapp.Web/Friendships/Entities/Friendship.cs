using System.Linq.Expressions;

namespace Wishapp.Web.Friendships.Entities;

public sealed class Friendship
{
    public Guid Id { get; private set; }
    public Guid RequesterId { get; private set; }
    public Guid AddresseeId { get; private set; }
    public FriendshipStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    private Friendship() { }

    public static Friendship Create(Guid requesterId, Guid addresseeId)
    {
        return new Friendship
        {
            Id = Guid.CreateVersion7(),
            RequesterId = requesterId,
            AddresseeId = addresseeId,
            Status = FriendshipStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void Accept()
    {
        Status = FriendshipStatus.Accepted;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Decline()
    {
        Status = FriendshipStatus.Declined;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ResetToPending(Guid newRequesterId, Guid newAddresseeId)
    {
        RequesterId = newRequesterId;
        AddresseeId = newAddresseeId;
        Status = FriendshipStatus.Pending;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public static Expression<Func<Friendship, bool>> AcceptedBetween(Guid userId, Guid targetId) =>
        f => f.Status == FriendshipStatus.Accepted &&
             ((f.RequesterId == userId && f.AddresseeId == targetId) ||
              (f.RequesterId == targetId && f.AddresseeId == userId));

    public static Expression<Func<Friendship, bool>> AcceptedWithAny(Guid userId, List<Guid> candidates) =>
        f => f.Status == FriendshipStatus.Accepted &&
             ((f.RequesterId == userId && candidates.Contains(f.AddresseeId)) ||
              (f.AddresseeId == userId && candidates.Contains(f.RequesterId)));

    public static Expression<Func<Friendship, bool>> Between(Guid userId, Guid targetId) =>
        f => (f.RequesterId == userId && f.AddresseeId == targetId) ||
             (f.RequesterId == targetId && f.AddresseeId == userId);
}
