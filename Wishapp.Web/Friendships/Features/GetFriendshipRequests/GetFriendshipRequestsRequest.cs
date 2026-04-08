using Wishapp.Web.Friendships.Entities;

namespace Wishapp.Web.Friendships.Features.GetFriendshipRequests;

public record GetFriendshipRequestsRequest(
    FriendshipStatus Status = FriendshipStatus.Pending,
    bool IsOutgoing = false,
    int Page = 1,
    int PageSize = 20);
