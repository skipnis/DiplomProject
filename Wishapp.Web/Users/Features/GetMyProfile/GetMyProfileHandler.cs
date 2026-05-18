using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Users.Features.GetMyProfile;

public sealed class GetMyProfileHandler(
    ApplicationDbContext db)
    : IQueryHandler<GetMyProfileQuery, GetMyProfileResponse>
{
    public async Task<Result<GetMyProfileResponse>> HandleAsync(
        GetMyProfileQuery query,
        CancellationToken ct = default)
    {
        var userId = query.UserId;

        var user = await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user is null)
        {
            return Error.NotFound("Users.NotFound", "User not found");
        }

        return new GetMyProfileResponse(
            user.Id,
            user.DisplayName,
            user.Username,
            user.Email,
            user.AvatarPath,
            user.AvatarUrl,
            user.Bio ?? string.Empty,
            user.BirthDate,
            user.IsOnboarded,
            user.ShowFulfilledWishes);
    }
}