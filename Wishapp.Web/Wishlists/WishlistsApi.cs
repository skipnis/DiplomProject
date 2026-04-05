using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Infrastructure.Authorization;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Wishlists.Dtos;
using Wishapp.Web.Wishlists.Entities;

namespace Wishapp.Web.Wishlists;

public sealed class WishlistsApi(ApplicationDbContext db) : IWishlistsApi
{
    public Task CreateSystemWishlistsAsync(Guid userId, CancellationToken ct = default)
    {
        var hidden = Wishlist.CreateSystem(userId, "Скрытые", WishlistVisibility.Private);

        var blacklist = Wishlist.CreateSystem(userId, "Чёрный список", WishlistVisibility.Public);

        db.Wishlists.Add(hidden);

        db.Wishlists.Add(blacklist);

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
                w.Members.Select(m => new WishlistMemberInfo(m.UserId, m.Role)).ToList()))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<bool> IsWishFulfilledAsync(Guid wishId, CancellationToken ct = default)
    {
        return await db.Wishlists
            .AsNoTracking()
            .AnyAsync(w => w.Wishes.Any(wish => wish.Id == wishId && wish.IsFulfilled), ct);
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
}
