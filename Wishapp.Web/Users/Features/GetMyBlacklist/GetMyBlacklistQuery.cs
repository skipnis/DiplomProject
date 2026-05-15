using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Users.Features.GetMyBlacklist;

public record GetMyBlacklistQuery(Guid UserId) : IQuery<List<BlacklistItemResponse>>;
