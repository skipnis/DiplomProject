using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Infrastructure.Authorization;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Common.Extensions;

public static class DbContextExtensions
{
    extension(ApplicationDbContext db)
    {
        public async Task<WishlistAccessContext?> GetAccessContextAsync(Guid wishlistId,
            CancellationToken ct = default)
        {
            return await db.Wishlists
                .AsNoTracking()
                .Where(w => w.Id == wishlistId)
                .Select(w => new WishlistAccessContext(
                    w.Id,
                    w.OwnerId,
                    w.Visibility,
                    w.Members.Select(m => new WishlistMemberInfo(m.UserId, m.Role)).ToList()))
                .FirstOrDefaultAsync(ct);
        }
        
        public async Task<(WishlistAccessContext? Source, WishlistAccessContext? Target)> GetAccessContextsAsync(
            Guid sourceId,
            Guid targetId,
            CancellationToken ct = default)
        {
            var contexts = await db.Wishlists
                .AsNoTracking()
                .Where(w => w.Id == sourceId || w.Id == targetId)
                .Select(w => new WishlistAccessContext(
                    w.Id,
                    w.OwnerId,
                    w.Visibility,
                    w.Members.Select(m => new WishlistMemberInfo(m.UserId, m.Role)).ToList()))
                .ToListAsync(ct);

            return (
                contexts.FirstOrDefault(c => c.WishlistId == sourceId),
                contexts.FirstOrDefault(c => c.WishlistId == targetId)
            );
        }
    }
}