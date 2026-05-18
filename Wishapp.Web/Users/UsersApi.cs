using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Users;

public class UsersApi(ApplicationDbContext dbContext) : IUsersApi
{
    public async Task<Result> ExistsAsync(Guid userId, CancellationToken ct = default)
    {
        var exists = await dbContext.Users.AnyAsync(x => x.Id == userId, ct);

        return exists
            ? Result.Success()
            : Result.Failure(Error.NotFound("Users.NotFound", "User not found"));
    }

    public async Task<Dictionary<Guid, string>> GetUsernamesAsync(
        List<Guid> userIds,
        CancellationToken ct = default)
    {
        return await dbContext.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.DisplayName, ct);
    }

    public async Task<List<Guid>> FilterExistingIdsAsync(List<Guid> userIds, CancellationToken ct = default)
    {
        return await dbContext.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .Select(u => u.Id)
            .ToListAsync(ct);
    }

    public async Task<Dictionary<Guid, UserPublicInfo>> GetUsersPublicInfoAsync(
        List<Guid> userIds,
        CancellationToken ct = default)
    {
        return await dbContext.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => new UserPublicInfo(u.DisplayName, u.AvatarPath ?? u.AvatarUrl), ct);
    }

}
