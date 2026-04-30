using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Users;

namespace Wishapp.Web.Wishlists.Features.Wishes.GetMyFulfilledWishes;

public sealed class GetMyFulfilledWishesHandler(ApplicationDbContext db, IUsersApi usersApi)
    : IQueryHandler<GetMyFulfilledWishesQuery, List<FulfilledWishRecordDto>>
{
    public async Task<Result<List<FulfilledWishRecordDto>>> HandleAsync(
        GetMyFulfilledWishesQuery query,
        CancellationToken ct = default)
    {
        var records = await db.FulfilledWishRecords
            .AsNoTracking()
            .Where(r => r.OwnerId == query.UserId)
            .OrderByDescending(r => r.FulfilledAt)
            .ToListAsync(ct);

        var gifterIds = records
            .Where(r => r.GifterId.HasValue)
            .Select(r => r.GifterId!.Value)
            .Distinct()
            .ToList();

        var gifterNames = gifterIds.Count > 0
            ? await usersApi.GetUsernamesAsync(gifterIds, ct)
            : [];

        return records.Select(r => new FulfilledWishRecordDto(
            r.Id,
            r.GifterId,
            r.GifterId.HasValue ? gifterNames.GetValueOrDefault(r.GifterId.Value) : null,
            r.WishName,
            r.WishDescription,
            r.Price,
            r.Currency,
            r.ImagePath,
            r.WishlistName,
            r.FulfilledAt)).ToList();
    }
}
