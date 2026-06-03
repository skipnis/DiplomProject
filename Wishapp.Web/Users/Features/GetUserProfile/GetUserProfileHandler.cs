using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Wishlists;

namespace Wishapp.Web.Users.Features.GetUserProfile;

public sealed class GetUserProfileHandler(ApplicationDbContext db, IWishlistsApi wishlistsApi)
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

        var stats = await wishlistsApi.GetUserWishStatsAsync(user.Id, ct);

        return new GetUserProfileResponse(
            user.Id,
            user.DisplayName,
            user.Username,
            user.AvatarPath ?? user.AvatarUrl,
            user.Bio,
            stats.ReceivedCount,
            stats.GiftedCount,
            user.BirthDate);
    }
}