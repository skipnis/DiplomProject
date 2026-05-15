namespace Wishapp.Web.Users.Features.GetMyBlacklist;

public record BlacklistItemResponse(Guid Id, string Title, DateTimeOffset CreatedAt);
