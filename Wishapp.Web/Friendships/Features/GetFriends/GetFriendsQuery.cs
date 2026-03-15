using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Friendships.Features.GetFriends;

public record GetFriendsQuery(Guid UserId) : IQuery<IEnumerable<FriendInfo>>;