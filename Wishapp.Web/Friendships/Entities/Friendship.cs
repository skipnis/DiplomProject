using System.Linq.Expressions;

namespace Wishapp.Web.Friendships.Entities;

public class Friendship
{
    public Guid Id  { get; set; }
    public Guid RequesterId { get; set; }
    public Guid AddresseeId { get; set; }
    public FriendshipStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    
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
    
    public static Expression<Func<Friendship, bool>> AcceptedBetween(Guid userId, Guid targetId) =>
        f => f.Status == FriendshipStatus.Accepted &&
             ((f.RequesterId == userId && f.AddresseeId == targetId) ||
              (f.RequesterId == targetId && f.AddresseeId == userId));
    
    public static Expression<Func<Friendship, bool>> Between(Guid userId, Guid targetId) =>
        f => (f.RequesterId == userId && f.AddresseeId == targetId) ||
             (f.RequesterId == targetId && f.AddresseeId == userId);
}