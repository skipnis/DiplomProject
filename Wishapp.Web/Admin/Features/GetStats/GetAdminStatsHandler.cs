using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Admin.Features.GetStats;

public sealed class GetAdminStatsHandler(ApplicationDbContext db)
    : IQueryHandler<GetAdminStatsQuery, AdminStatsResponse>
{
    public async Task<Result<AdminStatsResponse>> HandleAsync(
        GetAdminStatsQuery query,
        CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        var sevenDaysAgo = now.AddDays(-7);
        var thirtyDaysAgo = now.AddDays(-30);

        var totalUsers = await db.Users.CountAsync(ct);
        var newUsersLast7Days = await db.Users.CountAsync(u => u.CreatedAt >= sevenDaysAgo, ct);
        var newUsersLast30Days = await db.Users.CountAsync(u => u.CreatedAt >= thirtyDaysAgo, ct);

        var totalWishlists = await db.Wishlists.CountAsync(w => !w.IsSystem, ct);
        var totalWishes = await db.Wishes.CountAsync(ct);
        var wishesWithImage = await db.Wishes.CountAsync(w => w.ImagePath != null, ct);
        var averageWishesPerWishlist = totalWishlists > 0
            ? Math.Round((double)totalWishes / totalWishlists, 1)
            : 0.0;

        var activeReservations = await db.WishReservations.CountAsync(ct);
        var fulfilledWishes = await db.FulfilledWishRecords.CountAsync(ct);
        var fulfilledWithGifter = await db.FulfilledWishRecords.CountAsync(r => r.GifterId != null, ct);

        var topGiftersRaw = await db.FulfilledWishRecords
            .AsNoTracking()
            .Where(r => r.GifterId != null)
            .GroupBy(r => r.GifterId!.Value)
            .Select(group => new { UserId = group.Key, FulfilledCount = group.Count() })
            .OrderByDescending(g => g.FulfilledCount)
            .Take(10)
            .ToListAsync(ct);

        var gifterIds = topGiftersRaw.Select(g => g.UserId).ToList();
        var gifterNames = await db.Users
            .AsNoTracking()
            .Where(u => gifterIds.Contains(u.Id))
            .Select(u => new { u.Id, u.DisplayName })
            .ToDictionaryAsync(u => u.Id, u => u.DisplayName, ct);

        var topGifters = topGiftersRaw
            .Select(g => new TopGifterDto(
                g.UserId,
                gifterNames.GetValueOrDefault(g.UserId, "Unknown"),
                g.FulfilledCount))
            .ToList();

        var topItems = await db.CatalogItems
            .AsNoTracking()
            .Where(i => i.WishCount > 0)
            .OrderByDescending(i => i.WishCount)
            .Take(10)
            .Select(i => new TopCatalogItemDto(i.Id, i.Name, i.WishCount))
            .ToListAsync(ct);

        return new AdminStatsResponse(
            new AdminUserStats(totalUsers, newUsersLast7Days, newUsersLast30Days),
            new AdminContentStats(totalWishlists, totalWishes, averageWishesPerWishlist, wishesWithImage, totalWishes - wishesWithImage),
            new AdminActivityStats(activeReservations, fulfilledWishes, fulfilledWithGifter, topGifters),
            new AdminCatalogStats(topItems));
    }
}
