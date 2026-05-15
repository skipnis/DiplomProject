using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Users.Features.GetMyBlacklist;

namespace Wishapp.Web.Users.Features.AddBlacklistItem;

public record AddBlacklistItemCommand(Guid UserId, string Title) : ICommand<BlacklistItemResponse>;
