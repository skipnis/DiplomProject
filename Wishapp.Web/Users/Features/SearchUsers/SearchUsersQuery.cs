using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Users.Features.SearchUsers;

public record SearchUsersQuery(string DisplayName, Guid CurrentUserId) : IQuery<UsersSearchResponse>;