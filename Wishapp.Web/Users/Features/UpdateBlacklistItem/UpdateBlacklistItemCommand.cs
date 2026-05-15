using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Users.Features.UpdateBlacklistItem;

public record UpdateBlacklistItemCommand(Guid UserId, Guid ItemId, string Title) : ICommand;
