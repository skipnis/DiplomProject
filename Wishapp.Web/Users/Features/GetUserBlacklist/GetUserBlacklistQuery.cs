using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Users.Features.GetMyBlacklist;

namespace Wishapp.Web.Users.Features.GetUserBlacklist;

public record GetUserBlacklistQuery(Guid RequestingUserId, Guid TargetUserId)
    : IQuery<List<BlacklistItemResponse>>;
