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
}
