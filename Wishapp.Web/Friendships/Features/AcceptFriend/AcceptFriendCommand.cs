using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Friendships.Features.AcceptFriend;

public record AcceptFriendCommand(Guid UserId, Guid RequesterId) : ICommand;