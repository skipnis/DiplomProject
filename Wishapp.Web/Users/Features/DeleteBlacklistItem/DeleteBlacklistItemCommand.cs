using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Users.Features.DeleteBlacklistItem;

public record DeleteBlacklistItemCommand(Guid UserId, Guid ItemId) : ICommand;
