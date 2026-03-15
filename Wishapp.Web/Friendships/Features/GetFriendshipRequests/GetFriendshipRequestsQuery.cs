using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Friendships.Entities;

namespace Wishapp.Web.Friendships.Features.GetFriendshipRequests;

public record GetFriendshipRequestsQuery(Guid UserId, FriendshipStatus Status) 
    : IQuery<IEnumerable<FriendshipRequest>>;