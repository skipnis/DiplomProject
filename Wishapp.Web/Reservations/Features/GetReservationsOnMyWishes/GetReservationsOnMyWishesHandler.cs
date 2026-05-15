using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Reservations.Dtos;
using Wishapp.Web.Users;
using Wishapp.Web.Wishlists;

namespace Wishapp.Web.Reservations.Features.GetReservationsOnMyWishes;

public sealed class GetReservationsOnMyWishesHandler(
    ApplicationDbContext db,
    IWishlistsApi wishlistsApi,
    IUsersApi usersApi)
    : IQueryHandler<GetReservationsOnMyWishesQuery, List<WishReservedOnMyWishDto>>
{
    public async Task<Result<List<WishReservedOnMyWishDto>>> HandleAsync(
        GetReservationsOnMyWishesQuery query,
        CancellationToken ct = default)
    {
        var wishlistIds = await wishlistsApi.GetNonSurpriseWishlistIdsByOwnerAsync(query.UserId, ct);

        if (wishlistIds.Count == 0)
        {
            return new List<WishReservedOnMyWishDto>();
        }

        var reservations = await db.WishReservations
            .AsNoTracking()
            .Where(r => wishlistIds.Contains(r.WishlistId))
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);

        if (reservations.Count == 0)
        {
            return new List<WishReservedOnMyWishDto>();
        }

        var wishIds = reservations.Select(r => r.WishId).ToList();
        var summaries = await wishlistsApi.GetWishesSummaryAsync(wishIds, ct);
        var summaryByWishId = summaries.ToDictionary(s => s.WishId);

        var reserverIds = reservations.Select(r => r.ReservedByUserId).Distinct().ToList();
        var reserverNames = await usersApi.GetUsernamesAsync(reserverIds, ct);

        var items = reservations
            .Where(r => summaryByWishId.ContainsKey(r.WishId))
            .Select(r =>
            {
                var summary = summaryByWishId[r.WishId];
                var reserverName = reserverNames.GetValueOrDefault(r.ReservedByUserId, "Пользователь");

                return new WishReservedOnMyWishDto(
                    summary.WishId,
                    summary.WishlistId,
                    summary.WishName,
                    summary.WishlistName,
                    summary.ImagePath,
                    summary.Price,
                    summary.Currency,
                    r.ReservedByUserId,
                    reserverName,
                    r.CreatedAt);
            })
            .ToList();

        return items;
    }
}
