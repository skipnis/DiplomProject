using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Authorization;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Wishlists.Dtos;
using Wishapp.Web.Wishlists.Entities;

namespace Wishapp.Web.Wishlists;

public sealed class WishlistsApi(ApplicationDbContext db) : IWishlistsApi
{
    public Task CreateSystemWishlistsAsync(Guid userId, CancellationToken ct = default)
    {
        var hidden = Wishlist.CreateSystem(userId, "Скрытые", WishlistVisibility.Private, SystemWishlistType.Hidden);

        db.Wishlists.Add(hidden);

        return Task.CompletedTask;
    }

    public async Task<bool> WishExistsAsync(Guid wishId, CancellationToken ct = default)
    {
        return await db.Wishlists
            .AnyAsync(w => w.Wishes.Any(wish => wish.Id == wishId), ct);
    }

    public async Task<bool> CanAccessWishlistAsync(
        Guid userId,
        Guid wishlistId,
        CancellationToken ct = default)
    {
        var wishlist = await db.Wishlists
            .AsNoTracking()
            .Where(w => w.Id == wishlistId)
            .Select(w => new
            {
                w.OwnerId,
                w.Visibility,
                Members = w.Members.Select(m => m.UserId).ToList()
            })
            .FirstOrDefaultAsync(ct);

        if (wishlist is null)
        {
            return false;
        }

        return wishlist.Visibility == WishlistVisibility.Public ||
               wishlist.OwnerId == userId ||
               wishlist.Members.Contains(userId);
    }

    public async Task<WishlistAccessData?> GetWishlistAccessDataAsync(
        Guid wishlistId,
        CancellationToken ct = default)
    {
        return await db.Wishlists
            .AsNoTracking()
            .Where(w => w.Id == wishlistId)
            .Select(w => new WishlistAccessData(
                w.OwnerId,
                w.Visibility,
                w.Members.Select(m => new WishlistMemberInfo(m.UserId, m.Role)).ToList(),
                w.SystemType,
                w.IsSurpriseModeEnabled))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<bool> IsWishFulfilledAsync(Guid wishId, CancellationToken ct = default)
    {
        return await db.Wishlists
            .AsNoTracking()
            .AnyAsync(w => w.Wishes.Any(wish => wish.Id == wishId && wish.IsFulfilled), ct);
    }

    public async Task<Result> CanLinkWishlistAsync(Guid userId, Guid wishlistId, CancellationToken ct = default)
    {
        var wishlist = await db.Wishlists
            .AsNoTracking()
            .Where(w => w.Id == wishlistId)
            .Select(w => new { w.OwnerId, w.IsSystem })
            .FirstOrDefaultAsync(ct);

        if (wishlist is null)
            return Error.NotFound("Wishlists.NotFound", "Wishlist not found");

        if (wishlist.IsSystem)
            return Error.Failure("Wishlists.SystemWishlist", "Cannot link a system wishlist to an event");

        if (wishlist.OwnerId != userId)
            return Error.Forbidden("Wishlists.Forbidden", "Access denied");

        return Result.Success();
    }

    public async Task<UserWishStats> GetUserWishStatsAsync(Guid userId, CancellationToken ct = default)
    {
        var received = await db.Wishlists
            .AsNoTracking()
            .Where(wl => wl.OwnerId == userId && !wl.IsSystem)
            .SelectMany(wl => wl.Wishes)
            .CountAsync(w => w.IsFulfilled, ct);

        var gifted = await db.Wishlists
            .AsNoTracking()
            .Where(wl => wl.OwnerId != userId && !wl.IsSystem)
            .SelectMany(wl => wl.Wishes)
            .CountAsync(w => w.FulfilledByReserverId == userId, ct);

        return new UserWishStats(received, gifted);
    }

    public async Task<List<WishSummary>> GetWishesSummaryAsync(
        List<Guid> wishIds,
        CancellationToken ct = default)
    {
        return await db.Wishlists
            .AsNoTracking()
            .Where(wl => wl.Wishes.Any(w => wishIds.Contains(w.Id)))
            .Select(wl => new
            {
                Wishlist = wl,
                Wishes = wl.Wishes.Where(w => wishIds.Contains(w.Id))
            })
            .SelectMany(
                x => x.Wishes,
                (x, w) => new WishSummary(
                    w.Id,
                    x.Wishlist.Id,
                    w.Name,
                    w.ImagePath,
                    w.Price,
                    w.Currency,
                    x.Wishlist.Name,
                    x.Wishlist.OwnerId))
            .ToListAsync(ct);
    }

    public async Task<WishNotificationData?> GetWishNotificationDataAsync(
        Guid wishId,
        CancellationToken ct = default)
    {
        return await db.Wishlists
            .AsNoTracking()
            .Where(wl => wl.Wishes.Any(w => w.Id == wishId))
            .Select(wl => new WishNotificationData(
                wl.Id,
                wl.Name,
                wl.OwnerId,
                wl.IsSurpriseModeEnabled,
                wl.Wishes.Where(w => w.Id == wishId).Select(w => w.CreatedByUserId).FirstOrDefault()))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<GiftBadgeEligibilityData?> GetGiftBadgeEligibilityAsync(
        Guid wishlistId,
        Guid wishId,
        CancellationToken ct = default)
    {
        var wishlist = await db.Wishlists
            .AsNoTracking()
            .Where(wl => wl.Id == wishlistId)
            .Select(wl => new
            {
                wl.OwnerId,
                Wish = wl.Wishes.Where(w => w.Id == wishId).Select(w => new { w.IsFulfilled, w.FulfilledByReserverId, w.CreatedByUserId }).FirstOrDefault()
            })
            .FirstOrDefaultAsync(ct);

        if (wishlist is null)
            return null;

        return new GiftBadgeEligibilityData(
            wishlist.OwnerId,
            wishlist.Wish?.CreatedByUserId,
            wishlist.Wish is not null,
            wishlist.Wish?.IsFulfilled ?? false,
            wishlist.Wish?.FulfilledByReserverId);
    }

    public async Task<List<PublicFulfilledWishDto>> GetPublicFulfilledWishesAsync(Guid userId, CancellationToken ct = default)
    {
        return await db.FulfilledWishRecords
            .AsNoTracking()
            .Where(record => record.OwnerId == userId && !record.IsFromHiddenWishlist)
            .OrderByDescending(record => record.FulfilledAt)
            .Select(record => new PublicFulfilledWishDto(
                record.Id,
                record.WishName,
                record.ImagePath,
                record.WishlistName,
                record.FulfilledAt))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Guid>> GetNonSurpriseWishlistIdsByOwnerAsync(Guid ownerId, CancellationToken ct = default)
    {
        return await db.Wishlists
            .AsNoTracking()
            .Where(wl => wl.OwnerId == ownerId && !wl.IsSurpriseModeEnabled && !wl.IsSystem)
            .Select(wl => wl.Id)
            .ToListAsync(ct);
    }

    public async Task DeleteUserDataAsync(Guid userId, CancellationToken ct = default)
    {
        await db.Wishlists
            .Where(wl => wl.OwnerId == userId)
            .ExecuteDeleteAsync(ct);

        await db.WishlistMembers
            .Where(m => m.UserId == userId)
            .ExecuteDeleteAsync(ct);

        await db.FulfilledWishRecords
            .Where(r => r.OwnerId == userId)
            .ExecuteDeleteAsync(ct);
    }
}
