using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;

namespace Wishapp.Web.Friendships.Features.GetFriends;

public record GetFriendsQuery(Guid UserId, PagedRequest Request)
    : IQuery<PagedResponse<FriendInfo>>;
