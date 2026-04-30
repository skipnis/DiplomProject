using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Reservations;

public sealed class ReservationsApi(ApplicationDbContext db) : IReservationsApi
{
    public async Task<HashSet<Guid>> GetReservedWishIdsAsync(
        List<Guid> wishIds,
        CancellationToken ct = default)
    {
        var reserved = await db.WishReservations
            .AsNoTracking()
            .Where(r => wishIds.Contains(r.WishId))
            .Select(r => r.WishId)
            .ToListAsync(ct);

        return reserved.ToHashSet();
    }

    public async Task<Dictionary<Guid, Guid>> GetReservationsByWishIdsAsync(
        List<Guid> wishIds,
        CancellationToken ct = default)
    {
        return await db.WishReservations
            .AsNoTracking()
            .Where(r => wishIds.Contains(r.WishId))
            .ToDictionaryAsync(r => r.WishId, r => r.ReservedByUserId, ct);
    }

    public async Task<Guid?> GetReserverForWishAsync(Guid wishId, CancellationToken ct = default)
    {
        var reservation = await db.WishReservations
            .AsNoTracking()
            .Where(r => r.WishId == wishId)
            .Select(r => (Guid?)r.ReservedByUserId)
            .FirstOrDefaultAsync(ct);

        return reservation;
    }

    public async Task DeleteReservationForWishAsync(Guid wishId, CancellationToken ct = default)
    {
        await db.WishReservations
            .Where(r => r.WishId == wishId)
            .ExecuteDeleteAsync(ct);
    }

    public async Task DeleteUserDataAsync(Guid userId, CancellationToken ct = default)
    {
        await db.WishReservations
            .Where(r => r.ReservedByUserId == userId)
            .ExecuteDeleteAsync(ct);
    }
}
