using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Users.Features.SearchUsers;

public record SearchUsersQuery(string Username, Guid CurrentUserId) : IQuery<UsersSearchResponse>;