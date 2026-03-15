namespace Wishapp.Web.Friendships.Features.GetFriendshipRequests;

public record FriendshipRequest(
    Guid FriendshipId,
    Guid UserId,
    string Username,
    string? AvatarUrl,
    DateTimeOffset CreatedAt);