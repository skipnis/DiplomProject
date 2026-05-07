using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Wishlists.Dtos;

namespace Wishapp.Web.Wishlists.Features.Wishlists.GetWishlist;

public sealed class GetWishlistHandler(ApplicationDbContext db)
    : IQueryHandler<GetWishlistQuery, WishlistDto>
{
    public async Task<Result<WishlistDto>> HandleAsync(
        GetWishlistQuery query,
        CancellationToken ct = default)
    {
        var wishlist = await db.Wishlists
            .AsNoTracking()
            .Where(w => w.Id == query.WishlistId)
            .Select(w => new WishlistDto(
                w.Id, w.Name, w.Description, w.Emoji,
                w.Visibility, w.IsSystem, w.SystemType,
                w.IsSurpriseModeEnabled,
                w.Wishes.Count(wish => wish.IsFulfilled),
                w.Members
                    .OrderBy(m => m.JoinedAt)
                    .Select(m => new WishlistMemberDto(m.UserId, m.Role, m.CustomRoleName, m.JoinedAt))
                    .ToList()))
            .FirstOrDefaultAsync(ct);

        if (wishlist is null)
            return Error.NotFound("Wishlists.NotFound", "Wishlist not found");

        return wishlist;
    }
}