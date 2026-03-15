using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Friendships.Features.DeclineFriend;

public record DeclineFriendCommand(Guid UserId, Guid RequesterId) : ICommand;