using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Users.Features.CheckUsernameAvailability;

public sealed class CheckUsernameAvailabilityHandler(ApplicationDbContext db)
    : IQueryHandler<CheckUsernameAvailabilityQuery, bool>
{
    public async Task<Result<bool>> HandleAsync(
        CheckUsernameAvailabilityQuery query,
        CancellationToken ct = default)
    {
        var isTaken = await db.Users
            .AsNoTracking()
            .AnyAsync(u => u.Username == query.Username && u.Id != query.RequestingUserId, ct);

        return !isTaken;
    }
}
