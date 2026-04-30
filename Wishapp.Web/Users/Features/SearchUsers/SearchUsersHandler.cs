using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Users.Features.SearchUsers;

public sealed class SearchUsersHandler(ApplicationDbContext db)
    : IQueryHandler<SearchUsersQuery, UsersSearchResponse>
{
    public async Task<Result<UsersSearchResponse>> HandleAsync(
        SearchUsersQuery query,
        CancellationToken ct = default)
    {
        var users = await db.Users
            .Where(u => EF.Functions.ILike(u.DisplayName, $"%{query.DisplayName}%") && u.Id != query.CurrentUserId)
            .Select(u => new UserSearchResult(u.Id, u.DisplayName, u.Username, u.AvatarPath ?? u.AvatarUrl))
            .ToListAsync(ct);

        return Result.Success(new UsersSearchResponse(users));
    }
}