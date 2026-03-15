using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Friendships.Features.RemoveFriend;

public record RemoveFriendCommand(Guid UserId, Guid FriendId) : ICommand;