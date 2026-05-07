using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Wishlists;

namespace Wishapp.Web.Users.Features.GetUserFulfilledWishes;

public sealed class GetUserFulfilledWishesHandler(ApplicationDbContext db, IWishlistsApi wishlistsApi)
    : IQueryHandler<GetUserFulfilledWishesQuery, List<PublicFulfilledWishDto>>
{
    public async Task<Result<List<PublicFulfilledWishDto>>> HandleAsync(
        GetUserFulfilledWishesQuery query,
        CancellationToken ct = default)
    {
        var showFulfilledWishes = await db.Users
            .AsNoTracking()
            .Where(user => user.Id == query.TargetUserId)
            .Select(user => (bool?)user.ShowFulfilledWishes)
            .FirstOrDefaultAsync(ct);

        if (showFulfilledWishes is null)
            return Error.NotFound("Users.NotFound", "User not found");

        if (!showFulfilledWishes.Value)
            return new List<PublicFulfilledWishDto>();

        return await wishlistsApi.GetPublicFulfilledWishesAsync(query.TargetUserId, ct);
    }
}
