namespace Wishapp.Web.Reservations;

public interface IReservationsApi
{
    Task<HashSet<Guid>> GetReservedWishIdsAsync(
        List<Guid> wishIds,
        CancellationToken ct = default);

    Task<Dictionary<Guid, Guid>> GetReservationsByWishIdsAsync(
        List<Guid> wishIds,
        CancellationToken ct = default);
}
