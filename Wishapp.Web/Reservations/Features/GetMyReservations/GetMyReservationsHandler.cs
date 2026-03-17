using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Extensions;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Reservations.Dtos;
using Wishapp.Web.Users;
using Wishapp.Web.Wishlists;

namespace Wishapp.Web.Reservations.Features.GetMyReservations;

public sealed class GetMyReservationsHandler(ApplicationDbContext db, IWishlistsApi wishlistsApi, IUsersApi usersApi)
    : IQueryHandler<GetMyReservationsQuery, PagedResponse<MyReservationDto>>
{
    public async Task<Result<PagedResponse<MyReservationDto>>> HandleAsync(
        GetMyReservationsQuery query,
        CancellationToken ct = default)
    {
        var reservations = await db.WishReservations
            .AsNoTracking()
            .Where(r => r.ReservedByUserId == query.UserId)
            .OrderByDescending(r => r.CreatedAt)
            .ToPagedResponseAsync(query.Request, ct);

        if (reservations.Items.Count == 0)
        {
            return new PagedResponse<MyReservationDto>([], reservations.Page, reservations.PageSize, reservations.TotalCount);
        }

        var wishIds = reservations.Items.Select(r => r.WishId).ToList();
        
        var summaries = await wishlistsApi.GetWishesSummaryAsync(wishIds, ct);

        var summaryByWishId = summaries.ToDictionary(s => s.WishId);

        var ownerIds = summaries.Select(s => s.OwnerId).Distinct().ToList();
        
        var usernames = await usersApi.GetUsernamesAsync(ownerIds, ct);

        var items = reservations.Items
            .Where(r => summaryByWishId.ContainsKey(r.WishId))
            .Select(r =>
            {
                var s = summaryByWishId[r.WishId];
                var ownerUsername = usernames.GetValueOrDefault(s.OwnerId, "Unknown");

                return new MyReservationDto(
                    r.Id,
                    s.WishId,
                    s.WishlistId,
                    s.WishName,
                    s.ImagePath,
                    s.Price,
                    s.Currency,
                    s.WishlistName,
                    ownerUsername,
                    r.CreatedAt);
            })
            .ToList();

        return new PagedResponse<MyReservationDto>(items, reservations.Page, reservations.PageSize, reservations.TotalCount);
    }
}
