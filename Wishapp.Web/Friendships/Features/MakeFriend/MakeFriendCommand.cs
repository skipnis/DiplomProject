using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Friendships.Features.MakeFriend;

public record MakeFriendCommand(Guid RequesterId, Guid AddresseeId) : ICommand;