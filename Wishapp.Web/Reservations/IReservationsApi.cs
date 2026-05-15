namespace Wishapp.Web.Reservations;

public interface IReservationsApi
{
    Task<HashSet<Guid>> GetReservedWishIdsAsync(List<Guid> wishIds, CancellationToken ct = default);
    Task<Dictionary<Guid, Guid>> GetReservationsByWishIdsAsync(List<Guid> wishIds, CancellationToken ct = default);
    Task<Guid?> GetReserverForWishAsync(Guid wishId, CancellationToken ct = default);
    Task DeleteReservationForWishAsync(Guid wishId, CancellationToken ct = default);
    Task DeleteReservationsForWishesAsync(List<Guid> wishIds, CancellationToken ct = default);
    Task DeleteUserDataAsync(Guid userId, CancellationToken ct = default);
}
