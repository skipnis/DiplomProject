using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Users.Features.GetUserProfile;

public sealed class GetUserProfileHandler(ApplicationDbContext db)
    : IQueryHandler<GetUserProfileQuery, GetUserProfileResponse>
{
    public async Task<Result<GetUserProfileResponse>> HandleAsync(
        GetUserProfileQuery query,
        CancellationToken ct = default)
    {
        var user = await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == query.TargetUserId, ct);

        if (user is null)
        {
            return Error.NotFound("Users.NotFound", "User not found");
        }

        return new GetUserProfileResponse(
            user.Id,
            user.Username,
            user.AvatarUrl,
            user.Bio);
    }
}